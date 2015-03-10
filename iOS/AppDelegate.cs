using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace Xamarinibeacontest.iOS
{

	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window; 
		public static UIStoryboard Storyboard = UIStoryboard.FromName ("Main", null); 
		public static UIViewController initialViewController; 
		public override UIWindow Window { get; set; }

		public override void FinishedLaunching (UIApplication application)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			initialViewController = Storyboard.InstantiateInitialViewController () as UIViewController;

			window.RootViewController = initialViewController;
			window.MakeKeyAndVisible ();
			return ;
		}
	}
}

