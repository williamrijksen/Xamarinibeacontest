using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using EstimoteSdk;
using System.Collections.Generic;

namespace Xamarinibeacontest.Droid
{
	[Activity (Label = "Xamarin iBeacon test", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, BeaconManager.IServiceReadyCallback
	{

		private BeaconManager beaconManager;
		private EstimoteSdk.Region region;

		private TextView _scanLabel;
		private Button _scanButton;
		private TextView TextViewHolder;
		private bool isScanning = false;
		const string BeaconId = "com.williamrijksen";

		public MainActivity()
		{
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.MainLayout);

			_scanButton = (Button)FindViewById (Resource.Id.scanButton);
			_scanButton.Click += OnClickScanButtonClick;
			_scanLabel = (TextView)FindViewById (Resource.Id.scanLabel);
			TextViewHolder = (TextView)FindViewById (Resource.Id.results);
			beaconManager = new BeaconManager(this);
			beaconManager.Ranging += BeaconManagerRanging;

			beaconManager.EnteredRegion += (sender, args) => SetBeaconText(true);

			beaconManager.ExitedRegion += (sender, args) => SetBeaconText(false);
			beaconManager.Connect(this);
		}

		private void SetBeaconText(bool beaconsClose)
		{
			if (beaconsClose)
			{
				TextViewHolder.Text = "Entered region";
			}
			else
			{
				TextViewHolder.Text = "Exited region";
			}
		}


		void BeaconManagerRanging(object sender, BeaconManager.RangingEventArgs e)
		{
			if (e.Beacons == null)
			{
				Log.Debug ("Xamarinibeacontest", ""+e.Beacons.Count);
				return;
			}
			UpdateList (e.Beacons);
		}


		public void OnServiceReady()
		{
			if (region != null)
			{
				beaconManager.StopRanging(region);
				beaconManager.StopMonitoring(region);
				region = null;
			}

			region = new EstimoteSdk.Region(BeaconId, null, null, null);

			// This method is called when BeaconManager is up and running.
			beaconManager.StartRanging(region);
			beaconManager.StartMonitoring(region);
		}

		void OnClickScanButtonClick (object sender, EventArgs e)
		{
			if (isScanning) {
				_scanLabel.Text = "Not scanning now...";
				_scanButton.Text = "Start scanning";
			} else {
				_scanLabel.Text = "Scanning...";
				_scanButton.Text = "Stop scanning";
			}
			isScanning = !isScanning;
		}


		protected override void OnResume()
		{
			base.OnResume();

			if (region != null)
				beaconManager.Connect(this);
		}

		protected override void OnPause()
		{
			base.OnPause();
			if (region != null)
				beaconManager.Disconnect();
		}

		protected override void OnDestroy()
		{
			beaconManager.Disconnect();
			base.OnDestroy();
		}

		public void OnDismiss(IDialogInterface dialog)
		{
			Finish();
		}

		private void UpdateList(IList<Beacon> Beacons)
		{
			Log.Debug ("IBEACONTEST", "Update list");
			RunOnUiThread (() => {
				TextViewHolder.Text = "Just found: " + Beacons.Count + " beacons.\n\n";
				foreach (Beacon beacon in Beacons) {
					TextViewHolder.Text += "UUID : " + beacon.ProximityUUID + "\n";
					TextViewHolder.Text += "Major: " + beacon.Major + "\n";
					TextViewHolder.Text += "Minor : " + beacon.Minor + "\n";
				}
			});
		}
	}
}