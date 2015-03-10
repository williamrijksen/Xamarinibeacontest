## Ranging Example Code

	public class MainActivity : Activity, IBeaconConsumer
	{
		private RelativeLayout _layout;
		private EditText _editText;
		private BeaconManager _beaconManager;
		private long _lineCount = 0;

		public MainActivity()
		{
			_iBeaconManager = IBeaconManager.GetInstanceForApplication(this);

			_rangeNotifier = new RangeNotifier();
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.Main);

			_layout = FindViewById<RelativeLayout>(Resource.Id.layout);
			_editText = FindViewById<EditText>(Resource.Id.monitoringText);

			_beaconManager = BeaconManager.GetInstanceForApplication(this);
			
			var iBeaconParser = new BeaconParser();
			iBeaconParser.SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24");
			_beaconManager.BeaconParsers.Add(iBeaconParser);
			
			_iBeaconManager.Bind(this);
			_rangeNotifier.DidRangeBeaconsInRegionComplete += RangingBeaconsInRegion;
		}

		public void OnIBeaconServiceConnect()
		{
			_iBeaconManager.SetRangeNotifier(_rangeNotifier);
			_iBeaconManager.StartRangingBeaconsInRegion(_rangingRegion);
		}
		
		async void RangingBeaconsInRegion(object sender, RangeEventArgs e)
		{
			if(e.Beacons.Count > 0)
			{
				var beaconNumber = 0;
				var allBeacons = new List<Beacon>();
				foreach(var b in e.Beacons)
				{
					allBeacons.Add(b);
				}

				var orderedBeacons = allBeacons.OrderBy(b => b.Distance).ToList();

				orderedBeacons.ForEach(async (firstBeacon) =>
				{
					beaconNumber++;
					if(firstBeacon.Distance <= .5)
					{
						// ~immediate
						await UpdateDisplay("Beacon " + beaconNumber + " of " + 
							orderedBeacons.Count + 
							"\n" + firstBeacon.Id1 + "\nis about " + 
							string.Format("{0:N2}", firstBeacon.Distance) + 
							" meters away.", Color.Red);
					}
					else if(firstBeacon.Distance > .5 && firstBeacon.Distance <= 10)
					{
						// ~near
						await UpdateDisplay("Beacon " + beaconNumber + " of " + 
							orderedBeacons.Count + 
							"\n" + firstBeacon.Id1 + "\nis about " + 
							string.Format("{0:N2}", firstBeacon.Distance) + 
							" meters away.", Color.Yellow);
					}
					else if(firstBeacon.Distance > 10)
					{
						// ~far
						await UpdateDisplay("Beacon " + beaconNumber + " of " + 
							orderedBeacons.Count + 
							"\n" + firstBeacon.Id1 + "\nis about " + 
							string.Format("{0:N2}", firstBeacon.Distance) + 
							" meters away.", Color.Blue);
					}
					else
					{
						// ~unknown
						await UpdateDisplay("I'm not sure how close you are to the " + 
							beaconNumber + 
							" of " + orderedBeacons.Count + 
							" beacon.\n", Color.Transparent);
					}
				});
			}
			else
			{
				// ~unknown
				await UpdateDisplay("I don't see any beacons nearby.", Color.Transparent);
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if(_beaconManager.IsBound(this)) _beaconManager.Unbind(this);
		}
	
		private async Task UpdateDisplay(string message, Color color = default(Color))
		{
			Console.WriteLine(message);
			await Task.Run(() =>
			{
				RunOnUiThread(() =>
				{
					_lineCount++;
					_layout.SetBackgroundColor(color);
					_editText.Append(_lineCount + ": " + message + "\n\n");
				});
			});
		}
		
		public bool BindService(Intent intent, IServiceConnection conn, int p2)
		{
			return true;
		}

		public void OnBeaconServiceConnect()
		{
			_beaconManager.SetRangeNotifier(_rangeNotifier);

			_tagRegion = new AltBeaconOrg.BoundBeacon.Region("myUniqueBeaconId", 
					Identifier.Parse("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6"), 
					null, null);
			_emptyRegion = new AltBeaconOrg.BoundBeacon.Region("myEmptyBeaconId", 
					null, 
					null, null);

			_beaconManager.StartRangingBeaconsInRegion(_tagRegion);
			_beaconManager.StartRangingBeaconsInRegion(_emptyRegion);

			_startButton.Enabled = false;
		}
	}