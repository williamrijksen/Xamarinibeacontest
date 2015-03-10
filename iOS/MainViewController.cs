using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using Estimote;

using CoreLocation;

namespace Xamarinibeacontest.iOS
{
	partial class MainViewController : UIViewController
	{
		BeaconManager beaconManager;
		BeaconRegion region;
		bool isScanning = false;

		public MainViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.Title = "Select Beacon";
			beaconManager = new BeaconManager ();
			beaconManager.ReturnAllRangedBeaconsAtOnce = true;
			var uuid = new NSUuid ("44C0FFEE-988A-49DC-0BAD-A55C0DE2D1E4");
			region = new BeaconRegion (uuid, "BeaconSample");

			beaconManager.RangedBeacons += (sender, e) => 
			{
				TextViewHolder.Text = "Just found: " + e.Beacons.Length + " beacons.\n\n";
				foreach (Beacon beacon in e.Beacons){
					TextViewHolder.Text += "UUID : " + beacon.ProximityUUID.AsString() + "\n";
					TextViewHolder.Text += "Major: " + beacon.Major + "\n";
					TextViewHolder.Text += "Minor : " + beacon.Minor + "\n";
				}
			};

			ScanButton.TouchUpInside += delegate {
				if (isScanning) {
					beaconManager.StopRangingBeacons (region);

					ScanButton.SetTitle ("Start scanning", UIControlState.Normal);
					ScanLabel.Text = "Not scanning now...";
				} else {
					beaconManager.StartRangingBeacons (region);
					ScanButton.SetTitle ("Stop scanning", UIControlState.Normal);
					ScanLabel.Text = "Scanning...";
				}
				isScanning = !isScanning;
			};
		}
	}
}