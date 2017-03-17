using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Object = Java.Lang.Object;
using Android.Gms.Maps;
using Android.Gms.Common;
using Android.Gms.Maps.Model;
using Android.Locations;
using Xamarin.Geolocation;
using MyHitchhikingSpots.ViewModels;
using MyHitchhikingSpots.Tools;
using MyHitchhikingSpots.Models;
using MyHitchhikingSpots.Interfaces;

namespace MyHitchhikingSpots
{
    [Activity(Label = "My Hitchhiking Spots", MainLauncher = true)]//)]//
    public class MainView : Android.Support.V4.App.FragmentActivity
    {
        private bool _isGooglePlayServicesInstalled;
        public static readonly int InstallGooglePlayServicesId = 1000;
        MapFragment _mapFragment;
        GoogleMap _map;
        MapViewModel vm;
        LocationViewModel locVM;
        public static bool IsAppSetup = false;
        string _mapTabTag = "mapTab", _listTabTag = "listTab";

        Button btnRecord, btnGotARide, btnCopyUrl;
        TabHost tabhost;

        public static void AppStartup()
        {
            if (!IsAppSetup)
            {
                //DBs
                Console.WriteLine("entrou AppStartup1");
                ServiceContainer.Register<MyHitchhikingSpots.Databases.PrivateDB>();
                Console.WriteLine("entrou AppStartup2");
                ServiceContainer.Register<MyHitchhikingSpots.Databases.LocationsDB>();

                //View Models
                Console.WriteLine("entrou AppStartup3");
                ServiceContainer.Register<MapViewModel>();
                Console.WriteLine("entrou AppStartup4");
                ServiceContainer.Register<LocationViewModel>();

                //Services
                Console.WriteLine("entrou AppStartup5");
                ServiceContainer.Register<MyHitchhikingSpots.Services.MapService>(() => new MyHitchhikingSpots.Services.MapService());

                Console.WriteLine("entrou AppStartup6");
                IsAppSetup = true;
            }
        }

        public MainView()
        {
            try
            {
                Console.WriteLine("entrou mainview1");
                AppStartup();
                Console.WriteLine("entrou mainview2");
                vm = ServiceContainer.Resolve<MapViewModel>();
                Console.WriteLine("entrou mainview3");
                locVM = ServiceContainer.Resolve<LocationViewModel>();
                Console.WriteLine("entrou mainview4");
            }
            catch (Exception ex) { 
            Tool.WriteExceptionMessagesToOutputBox(ex);}
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Console.WriteLine("entrou oncreate");
            SetContentView(Resource.Layout.MainActivity);

            _isGooglePlayServicesInstalled = TestIfGooglePlayServicesIsInstalled();

            tabhost = FindViewById<TabHost>(Resource.Id.tabHost1);

            btnRecord = FindViewById<Button>(Resource.Id.btnRecord);
            btnGotARide = FindViewById<Button>(Resource.Id.btnGotARide);
            btnCopyUrl = FindViewById<Button>(Resource.Id.btnCopyUrl);

            btnRecord.Click += btnRecord_Click;
            btnGotARide.Click += GotARide_Click;
            btnCopyUrl.Click += btnCopyUrl_Click;

            SetUpTabHost();

        }

        private void SetCamera(LatLng position)
        {
            // Move the map so that it is showing the markers we added above.
            if (_mapFragment != null && _mapFragment.Map != null && position != null)
            {
                _mapFragment.Map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(position, 13));
                tabhost.SetCurrentTabByTag(_mapTabTag);
            }
        }

        protected override void OnStart()
        {
            base.OnStart(); // Always call the superclass first.
            Console.WriteLine("entrou onstart");

            vm.LoadMap();
            if(locVM.locator==null)
            locVM.SetGeolocator(new Geolocator(this) { DesiredAccuracy = 5 }, false);

            InitMapFragment();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Console.WriteLine("entrou onresume");

            try
            {
                locVM.OnCurrentPositionChanged += UpdateRecordButton;
                locVM.StartGeolocatorListener();

                if (vm.SelectedMapItem.GetDataState() == SpotDataState.BasicInfoInserted)
                    btnGotARide.Enabled = true;
                else
                    btnGotARide.Enabled = false;

                if (vm.MapItemList.Count() > 0)
                    btnCopyUrl.Visibility = ViewStates.Visible;
                else
                    btnCopyUrl.Visibility = ViewStates.Gone;

                UpdateRecordButton(null, null);

                InitSpotsList();

                SetupMapIfNeeded();

                _map.MyLocationEnabled = true;

                CenterMapOnMyLocation();

                // Setup a handler for when the user clicks on a marker.
                _map.MarkerClick += MapOnMarkerClick;
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
                //Toast.MakeText(this, "Something went wrong, please try to navigate back and access Come and Go again.", ToastLength.Long).Show();
            }
        }

        /// <summary>
        ///   Add three markers to the map.
        /// </summary>
        private void AddMarker(string title, double lat, double lng, string snippet)
        {
            RunOnUiThread(() =>
            {
                MarkerOptions markerOpt1 = new MarkerOptions();
                markerOpt1.SetTitle(title);
                markerOpt1.SetSnippet(snippet);
                markerOpt1.SetPosition(new LatLng(lat, lng));

                _map.AddMarker(markerOpt1);
            });
        }




        private void SetUpTabHost()
        {
            tabhost.Setup();

            //TabHost.TabSpec tabItemConsumtion = tabhost.NewTabSpec("tabItemConsumtion");
            //tabItemConsumtion.SetContent(Resource.IdDetailesTabHost.tabItemConsumtion);
            //tabItemConsumtion.SetIndicator("Item Consumtion");
            //tabhost.AddTab(tabItemConsumtion);

            TabHost.TabSpec mapTab = tabhost.NewTabSpec(_mapTabTag);
            mapTab.SetContent(Resource.Id.mapTab);
            mapTab.SetIndicator(Resources.GetString(Resource.String.MapTabTitle));
            tabhost.AddTab(mapTab);

            TabHost.TabSpec listTab = tabhost.NewTabSpec(_listTabTag);
            listTab.SetContent(Resource.Id.listTab);
            listTab.SetIndicator(Resources.GetString(Resource.String.ListTabTitle));
            tabhost.AddTab(listTab);

            //LocalActivityManager mLocalActivityManager = new LocalActivityManager(this, false);
            //mLocalActivityManager.DispatchCreate(state); // state will be bundle your activity state which you get in onCreate
            //tabhost.Setup(mLocalActivityManager);
            //TabHost.TabSpec tabObjects = tabhost.NewTabSpec("tabObjects");
            //var intent = new Intent(this, typeof(MapTabFragment));
            //intent.SetFlags(ActivityFlags.NewTask);
            //tabObjects.SetContent(intent);
            //tabObjects.SetIndicator("Service Objects");
            //tabhost.AddTab(tabObjects);

            // FindViewById<LinearLayout>(Resource.IdDetailesTabHost.tabItemServieObjects).AddView(new ServiceObjectsTab());

        }

        private void InitSpotsList()
        {
            var adapter = new SpotsListAdapter(this, vm.MapItemList);
            adapter.EditClick = btnSpotEditClick;
            adapter.ZoomInClick = btnSpotZoomInClick;
            var list = FindViewById<ListView>(Resource.Id.spotsList);
            list.Adapter = adapter;

        }

        void btnSpotEditClick(object sender, MapItem item)
        {
            vm.SelectMapItem(item.Id);
            StartActivity(typeof(MyHitchhikingSpots.Views.SpotEditActivity));
        }

        void btnSpotZoomInClick(object sender, MapItem item)
        {
            SetCamera(item.GetLatLng());
            //MapOnMarkerClick(null, null);
        }

        protected override void OnPause()
        {
            base.OnPause();

            locVM.OnCurrentPositionChanged -= UpdateRecordButton;
            locVM.StopGeolocatorListener();

            // Pause the GPS - we won't have to worry about showing the 
            // location.
            if (_map != null)
            {
                _map.MyLocationEnabled = false;
                _map.CameraChange -= ZoomToCoverAllMarkers;
                _map.MarkerClick -= MapOnMarkerClick;
            }
        }


        private void MapOnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs markerClickEventArgs)
        {
            if (markerClickEventArgs.Marker != null)
                RunOnUiThread(() => markerClickEventArgs.Marker.ShowInfoWindow());
            //Marker marker = markerClickEventArgs.P0; // TODO [TO201212142221] Need to fix the name of this with MetaData.xml
            //if (marker.Id.Equals(_gotoMauiMarkerId))
            //{
            //    PositionPolarBearGroundOverlay(InMaui);
            //    _map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(InMaui, 13));
            //    _gotoMauiMarkerId = null;
            //    _polarBearMarker.Remove();
            //    _polarBearMarker = null;
            //}
            //else
            //{
            //    Toast.MakeText(this, String.Format("You clicked on Marker ID {0}", marker.Id), ToastLength.Short).Show();
            //}
        }

        private bool TestIfGooglePlayServicesIsInstalled()
        {
            int queryResult = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Android.Util.Log.Info("SimpleMapDemo", "Google Play Services is installed on this device.");
                return true;
            }

            if (GooglePlayServicesUtil.IsUserRecoverableError(queryResult))
            {
                string errorString = GooglePlayServicesUtil.GetErrorString(queryResult);
                Android.Util.Log.Error("SimpleMapDemo", "There is a problem with Google Play Services on this device: {0} - {1}", queryResult, errorString);
                //var errorDialog = GooglePlayServicesUtil.GetErrorDialog(queryResult, this, InstallGooglePlayServicesId);
                //ErrorDialogFragment dialogFrag =  ErrorDialogFragment.NewInstance(errorDialog);

                //dialogFrag.Show(FragmentManager, "GooglePlayServicesDialog");
            }
            return false;
        }


        private void InitMapFragment()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;
            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeNormal)
                    .InvokeZoomControlsEnabled(true)
                    .InvokeCompassEnabled(true);

                _mapFragment = MapFragment.NewInstance(mapOptions);
                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                fragTx.Add(Resource.Id.mapWithOverlay, _mapFragment, "map");
                fragTx.Commit();
            }
        }

        private void OnFirstPositionChanged(object sender, EventArgs e)
        {
            /////----------------------------------Zooming camera to position user-----------------  
            //if (locVM.CurrentPosition != null)
            //{
            //    locVM.CurrentPosition.OnCurrentPositionChanged -= OnFirstPositionChanged;


            //        CameraPosition cameraPosition = new CameraPosition.Builder()
            //            .Target(new LatLng(locVM.CurrentPosition.Latitude, locVM.CurrentPosition.Longitude))      // Sets the center of the map to location user
            //            .Zoom(13)                   // Sets the zoom
            //            //.Bearing(90)                // Sets the orientation of the camera to east
            //            //.Tilt(40)                   // Sets the tilt of the camera to 30 degrees
            //            .Build();                   // Creates a CameraPosition from the builder

            //        _mapFragment.Map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));

            //}
            /////----------------------------------Zooming camera to position user-----------------}
        }

        private void UpdateRecordButton(object sender, EventArgs e)
        {
            if (locVM.CurrentPosition == null || btnGotARide.Enabled)
                btnRecord.Enabled = false;
            else
                btnRecord.Enabled = true;
        }

        private void CenterMapOnMyLocation()
        {
            //Criteria criteria = new Criteria();
            //LocationManager locationManager = (LocationManager)GetSystemService(Context.LocationService);
            //Location location = locationManager.GetLastKnownLocation(locationManager.GetBestProvider(criteria, false));

            //OnPositionChanged(location);
        }

        private void SetupMapIfNeeded()
        {
            if (_map == null)
            {
                _map = _mapFragment.Map;
                _map.SetInfoWindowAdapter(new InfoWindowAdapter(this));
                if (_map != null)
                {
                    foreach (var item in vm.MapItemList)
                        AddMarker(item.City + " - " + item.Country, item.Latitude, item.Longitude, item.DateTime.ToShortTimeString() + " - " + item.Note);

                    _map.CameraChange += ZoomToCoverAllMarkers;

                    //AddInitialPolarBarToMap();                                        
                }
            }

            if (!mapNotWorkingMessageShowed && !_isGooglePlayServicesInstalled || !Tool.IsOnline(this) || _map == null)
            {
                mapNotWorkingMessageShowed = true;
                var alert = new AlertDialog.Builder(this);
                alert.SetTitle(Resources.GetString(Resource.String.MapNotWorkingDialogTitle));
                alert.SetMessage(Resources.GetString(Resource.String.MapNotWorkingDialogMessage));
                alert.SetPositiveButton("Ok", delegate { });
                alert.Show();
            }
        }

        bool mapNotWorkingMessageShowed = false;

        public class InfoWindowAdapter : Java.Lang.Object, Android.Gms.Maps.GoogleMap.IInfoWindowAdapter
        {
            Context _context;
            public InfoWindowAdapter(Activity context)
            {
                _context = context;
            }

            public View GetInfoContents(Marker marker)
            {
                View v = LayoutInflater.From(_context).Inflate(Resource.Layout.MapInfoWindow, null);

                TextView tvLat = (TextView)v.FindViewById(Resource.MapInfoWindow.title);
                TextView tvLng = (TextView)v.FindViewById(Resource.MapInfoWindow.note);

                tvLat.Text = marker.Title;
                tvLng.Text = marker.Snippet;

                return v;
            }

            public View GetInfoWindow(Marker marker)
            {
                return null;
            }


        }

        void ZoomToCoverAllMarkers(object sender, GoogleMap.CameraChangeEventArgs e)
        {
            //Method copied from here: http://stackoverflow.com/a/14828739/1094261
            _mapFragment.Map.CameraChange -= ZoomToCoverAllMarkers;

            if (!vm.MapItemList.Any())
                SetCamera(locVM.CurrentPosition.GetLatLng());
            else
            {
                //Calculate the bounds of all the markers
                LatLngBounds.Builder builder = new LatLngBounds.Builder();

                builder.Include(locVM.CurrentPosition.GetLatLng());

                foreach (var address in vm.MapItemList)
                {
                    var pos = address.GetLatLng();
                    if (pos != null)
                        builder.Include(pos);
                }

                int padding = 200; // offset from edges of the map in pixels           

                try
                {
                    var bounds = builder.Build();
                    RunOnUiThread(() =>
                    {
                        //Obtain a movement description object by using the factory: CameraUpdateFactory:
                        CameraUpdate cu = CameraUpdateFactory.NewLatLngBounds(bounds, padding);

                        //Move the map
                        _mapFragment.Map.MoveCamera(cu);
                    });
                }
                catch (Exception ex)
                {
                    Tool.WriteExceptionMessagesToOutputBox(ex);
                    //bounds = new LatLngBounds(new LatLng(55, 12), new LatLng(61, 21));
                }
            }

        }

        void btnCopyUrl_Click(object sender, EventArgs e)
        {
            bool success = false;
            try
            {
                var mapUrl = vm.GetRouteUrl();

                if (!String.IsNullOrEmpty(mapUrl))
                {
                    ClipboardManager clipboard = (ClipboardManager)GetSystemService(Context.ClipboardService);
                    ClipData clip = ClipData.NewPlainText("Route url", mapUrl);
                    clipboard.PrimaryClip = clip;
                    //MessageBox.Show(AppResources.RouteMapUrlCopiedMessage);
                    RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.RouteMapUrlCopiedMessage), ToastLength.Long).Show());
                    success = true;
                }
            }
            catch { }

            if (!success)
                RunOnUiThread(() => Toast.MakeText(this, "Error occurred. / Ocorreu uma falha.", ToastLength.Long).Show());
        }

        void btnRecord_Click(object sender, EventArgs e)
        {
            vm.UnselectMapItem();
            StartActivity(typeof(MyHitchhikingSpots.Views.SpotEditActivity));
        }

        void GotARide_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MyHitchhikingSpots.Views.SpotEditActivity));
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;

            }

            return base.OnOptionsItemSelected(item);
        }



        private void GrayOutIcon(int resourceId)
        {
            RunOnUiThread(() => FindViewById<View>(resourceId).Background.SetColorFilter(Color.Gray, PorterDuff.Mode.Multiply));
        }
        private void GrayOutIconClear(int resourceId)
        {
            var icon = FindViewById<View>(resourceId);
            if (icon.Background != null)
                icon.Background.ClearColorFilter();
        }


        class SpotsListAdapter : BaseAdapter<MapItem>
        {
            Activity _context;
            List<MapItem> _mapItems;
            public EventHandler<MapItem> EditClick;
            public EventHandler<MapItem> ZoomInClick;

            public override MapItem this[int position]
            {
                get { return _mapItems[position]; }
            }

            public override int Count
            {
                get { return _mapItems.Count(); }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public SpotsListAdapter(Activity activity, List<MapItem> items)
            {
                _context = activity;
                _mapItems = items;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (convertView == null)
                {
                    convertView = _context.LayoutInflater.Inflate(Resource.Layout.spotsList_ListItem, null);
                }
                var title = "{0} - {1}";

                convertView.FindViewById<TextView>(Resource.SpotsList.txtTitle).Text = String.Format(title, this[position].City, this[position].Country).SubstringCut(17);
                convertView.FindViewById<TextView>(Resource.SpotsList.txtNote).Text = this[position].Note.GetOnlyFirstLine().SubstringCut(20);

                convertView.FindViewById<Button>(Resource.SpotsList.btnEdit).Tag = position;
                convertView.FindViewById<Button>(Resource.SpotsList.btnEdit).Click += Edit_Click;
                convertView.FindViewById<Button>(Resource.SpotsList.btnZoomIn).Tag = position;
                convertView.FindViewById<Button>(Resource.SpotsList.btnZoomIn).Click += ZoomIn_Click;

                return convertView;
            }



            void Edit_Click(object sender, EventArgs e)
            {
                var btn = sender as Button;
                var tag = btn.Tag.ToString();
                int mapItemIndex = 0;

                if (Int32.TryParse(tag, out mapItemIndex))
                {
                    EditClick(null, this[mapItemIndex]);
                }
            }

            void ZoomIn_Click(object sender, EventArgs e)
            {
                var btn = sender as Button;
                var tag = btn.Tag.ToString();
                int mapItemIndex = 0;

                if (Int32.TryParse(tag, out mapItemIndex))
                {
                    ZoomInClick(null, this[mapItemIndex]);
                }
            }
        }
    }

}