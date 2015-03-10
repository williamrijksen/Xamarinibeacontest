// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Xamarinibeacontest.iOS
{
	[Register ("MainViewController")]
	partial class MainViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton ScanButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel ScanLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextView TextViewHolder { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (ScanButton != null) {
				ScanButton.Dispose ();
				ScanButton = null;
			}
			if (ScanLabel != null) {
				ScanLabel.Dispose ();
				ScanLabel = null;
			}
			if (TextViewHolder != null) {
				TextViewHolder.Dispose ();
				TextViewHolder = null;
			}
		}
	}
}
