﻿using System;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltBeaconOrg.BoundBeacon;
using Android.App;
using Android.Content;

using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Widget;

namespace Xamarinibeacontest.Droid
{
	[Activity (Label = "Xamarin iBeacon test", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, IDialogInterfaceOnDismissListener, IBeaconConsumer
	{
		private readonly MonitorNotifier _monitorNotifier;
		private readonly RangeNotifier _rangeNotifier;

		AltBeaconOrg.BoundBeacon.Region _tagRegion, _emptyRegion;

		private TextView _scanLabel;
		private Button _scanButton;
		private TextView TextViewHolder;
		private BeaconManager _beaconManager;
		private readonly List<Beacon> _data;
		private bool isScanning = false;

		public MainActivity()
		{
			_monitorNotifier = new MonitorNotifier();
			_rangeNotifier = new RangeNotifier();
			_data = new List<Beacon>();
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.MainLayout);

			VerityBluetooth();

			_beaconManager = BeaconManager.GetInstanceForApplication(this);
			// By default the AndroidBeaconLibrary will only find AltBeacons.  If you wish to make it
			// find a different type of beacon, you must specify the byte layout for that beacon's
			// advertisement with a line like below.  The example shows how to find a beacon with the
			// same byte layout as AltBeacon but with a beaconTypeCode of 0xaabb
			//
			// beaconManager.getBeaconParsers().add(new BeaconParser().
			//        setBeaconLayout("m:2-3=aabb,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25"));
			//
			// In order to find out the proper BeaconLayout definition for other kinds of beacons, do
			// a Google search for "setBeaconLayout" (including the quotes in your search.)

			var iBeaconParser = new BeaconParser();
			//	Estimote > 2013
			iBeaconParser.SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24");
			_beaconManager.BeaconParsers.Add(iBeaconParser);

			_beaconManager.Bind(this);
			_beaconManager.SetBackgroundMode(false);

			_monitorNotifier.EnterRegionComplete += EnteredRegion;
			_monitorNotifier.ExitRegionComplete += ExitedRegion;
			_monitorNotifier.DetermineStateForRegionComplete += DeterminedStateForRegionComplete;
			_rangeNotifier.DidRangeBeaconsInRegionComplete += RangingBeaconsInRegion;

			_scanButton = (Button)FindViewById (Resource.Id.scanButton);
			_scanButton.Click += OnClickScanButtonClick;
			_scanLabel = (TextView)FindViewById (Resource.Id.scanLabel);
			TextViewHolder = (TextView)FindViewById (Resource.Id.results);
		}

		protected override void OnResume()
		{
			base.OnResume();

			if(_beaconManager.IsBound(this))
			{
				_beaconManager.SetBackgroundMode(false);
			}
		}

		void OnClickScanButtonClick (object sender, EventArgs e)
		{
			if (isScanning) {
				_beaconManager.StopMonitoringBeaconsInRegion(_tagRegion);
				_beaconManager.StopRangingBeaconsInRegion(_tagRegion);

				_beaconManager.StopMonitoringBeaconsInRegion(_emptyRegion);
				_beaconManager.StopRangingBeaconsInRegion(_emptyRegion);
				_scanLabel.Text = "Not scanning now...";
				_scanButton.Text = "Start scanning";
			} else {
				_beaconManager.StartMonitoringBeaconsInRegion(_tagRegion);
				_beaconManager.StartRangingBeaconsInRegion(_tagRegion);

				_beaconManager.StartMonitoringBeaconsInRegion(_emptyRegion);
				_beaconManager.StartRangingBeaconsInRegion(_emptyRegion);
				_scanLabel.Text = "Scanning...";
				_scanButton.Text = "Stop scanning";
			}
			isScanning = !isScanning;
		}

		protected override void OnPause()
		{
			base.OnPause();

			if(_beaconManager.IsBound(this))
			{
				_beaconManager.SetBackgroundMode(true);
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if(_beaconManager.IsBound(this)) _beaconManager.Unbind(this);
		}

		async void RangingBeaconsInRegion(object sender, RangeEventArgs e)
		{
			await ClearData();

			var allBeacons = new List<Beacon>();
			if(e.Beacons.Count > 0)
			{
				var beaconNumber = 0;
				foreach(var b in e.Beacons)
				{
					allBeacons.Add((Beacon)b);
				}

				var orderedBeacons = allBeacons.OrderBy(b => b.Distance).ToList();

				orderedBeacons.ForEach(async (firstBeacon) =>
					{
						beaconNumber++;
						await UpdateData(firstBeacon);
					});
			}
			else
			{
				// unknown
				await ClearData();
			}
		}

		void RemoveBeaconsNoLongerVisible(List<Beacon> allBeacons)
		{
			if(allBeacons == null || allBeacons.Count == 0) return;

			var delete = new List<Beacon>();
			foreach(var d in _data)
			{
				if(allBeacons.All(ab => ab.Id1.ToString() != d.Id1.ToString()))
				{
					delete.Add(d);
				}
			}

			_data.RemoveAll(d => delete.Any(del => del.Id1.ToString() == d.Id1.ToString()));

			if(delete.Count > 0)
			{
				delete = null;
				UpdateList();
			}
		}

		void VerityBluetooth()
		{
			try 
			{
				if (!BeaconManager.GetInstanceForApplication(this).CheckAvailability())
				{
					var builder = new AlertDialog.Builder(this);
					builder.SetTitle("Bluetooth not enabled");
					builder.SetMessage("Please enable bluetooth in settings and restart this application.");
					EventHandler<DialogClickEventArgs> handler = null;
					builder.SetPositiveButton(Android.Resource.String.Ok, handler);
					builder.SetOnDismissListener(this);
					builder.Show();
				}		
			}
			catch (BleNotAvailableException e) 
			{
				Log.Debug("BleNotAvailableException", e.Message);

				var builder = new AlertDialog.Builder(this);
				builder.SetTitle("Bluetooth LE not available");
				builder.SetMessage("Sorry, this device does not support Bluetooth LE.");
				EventHandler<DialogClickEventArgs> handler = null;
				builder.SetPositiveButton(Android.Resource.String.Ok, handler);
				builder.SetOnDismissListener(this);
				builder.Show();
			}
		}

		private async Task UpdateDisplay(string message, Color color = default(Color))
		{
			Log.Debug ("IBEACONTEST", message);
		}

		private async Task UpdateData(Beacon beacon)
		{
			await Task.Run(() =>
				{
					if(_data.All(d => d.Id1.ToString() != beacon.Id1.ToString()))
					{
						RunOnUiThread(() =>
							{
								_data.Add(beacon);
								_data.Sort((x,y) => x.Distance.CompareTo(y.Distance));
								UpdateList();
							});
					}
				});
		}

		async void EnteredRegion(object sender, MonitorEventArgs e)
		{
			await UpdateDisplay("A new beacon just showed up!");
		}

		async void ExitedRegion(object sender, MonitorEventArgs e)
		{
			await UpdateDisplay("They went away :(");
		}

		async void DeterminedStateForRegionComplete(object sender, MonitorEventArgs e)
		{
			await UpdateDisplay("I have just switched from seeing/not seeing beacons: " + e.State);
		}

		public bool BindService(Intent p0, IServiceConnection p1, int p2)
		{
			return true;
		}

		public void OnBeaconServiceConnect()
		{
			_beaconManager.SetForegroundBetweenScanPeriod(5000); // 5000 milliseconds

			_beaconManager.SetMonitorNotifier((IMonitorNotifier)_monitorNotifier); 
			_beaconManager.SetRangeNotifier((IRangeNotifier)_rangeNotifier);

			_tagRegion = new AltBeaconOrg.BoundBeacon.Region("myUniqueBeaconId", Identifier.Parse("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6"), null, null);
			_emptyRegion = new AltBeaconOrg.BoundBeacon.Region("myEmptyBeaconId", null, null, null);

			_beaconManager.StartMonitoringBeaconsInRegion(_tagRegion);
			_beaconManager.StartRangingBeaconsInRegion(_tagRegion);

			_beaconManager.StartMonitoringBeaconsInRegion(_emptyRegion);
			_beaconManager.StartRangingBeaconsInRegion(_emptyRegion);
		}

		public void OnDismiss(IDialogInterface dialog)
		{
			Finish();
		}

		private async Task ClearData()
		{
			RunOnUiThread(() =>
				{
					TextViewHolder.Text = "Just found: 0 beacons.\n\n";
				});
		}

		private void UpdateList()
		{
			Log.Debug ("IBEACONTEST", "Update list");
			RunOnUiThread (() => {
				TextViewHolder.Text = "Just found: " + _data.Count + " beacons.\n\n";
				foreach (Beacon beacon in _data) {
					TextViewHolder.Text += "UUID : " + beacon.Id1 + "\n";
					TextViewHolder.Text += "Major: " + beacon.Id2 + "\n";
					TextViewHolder.Text += "Minor : " + beacon.Id3 + "\n";
				}
			});
		}
	}
}