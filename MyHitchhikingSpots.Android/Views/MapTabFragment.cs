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

namespace MyHitchhikingSpots
{
    [Activity(Label = "map")]
    public class MapTabFragment : Android.Support.V4.App.FragmentActivity
    {
        private bool _isGooglePlayServicesInstalled;
        public static readonly int InstallGooglePlayServicesId = 1000;
        MapFragment _mapFragment;
        GoogleMap _map;

        public MapTabFragment()
        {
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MapTab);

            _isGooglePlayServicesInstalled = TestIfGooglePlayServicesIsInstalled();

            locator = new Geolocator(this) { DesiredAccuracy = 50 };
            locator.StartListening(minTime: 200, minDistance: 1);
            locator.PositionChanged += OnPositionChanged;

            
            //SetUpTabHost();
        }

        protected override void OnStart()
        {
            base.OnStart(); // Always call the superclass first.


            InitMapFragment();
            SetupMapIfNeeded();
        }


        protected override void OnPause()
        {
            base.OnPause();

            // Pause the GPS - we won't have to worry about showing the 
            // location.
            _map.MyLocationEnabled = false;

            _map.MarkerClick -= MapOnMarkerClick;
        }


        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                SetupMapIfNeeded();

                _map.MyLocationEnabled = true;

                CenterMapOnMyLocation();

                // Setup a handler for when the user clicks on a marker.
                _map.MarkerClick += MapOnMarkerClick;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                //Toast.MakeText(this, "Something went wrong, please try to navigate back and access Come and Go again.", ToastLength.Long).Show();
            }
        }





        private void MapOnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs markerClickEventArgs)
        {
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
                    .InvokeMapType(GoogleMap.MapTypeSatellite)
                    .InvokeZoomControlsEnabled(true)
                    .InvokeCompassEnabled(true);

                _mapFragment = MapFragment.NewInstance(mapOptions);
                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                fragTx.Add(Resource.Id.mapWithOverlay, _mapFragment, "map");
                fragTx.Commit();
            }
        }

        Geolocator locator;


        private void OnPositionChanged(object sender, PositionEventArgs e)
        {
            /////----------------------------------Zooming camera to position user-----------------
            try
            {


                if (e.Position != null && _mapFragment != null & _mapFragment.Map != null)
                {
                    _mapFragment.Map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(
                             new LatLng(e.Position.Latitude, e.Position.Longitude), 13));

                    CameraPosition cameraPosition = new CameraPosition.Builder()
                            .Target(new LatLng(e.Position.Latitude, e.Position.Longitude))      // Sets the center of the map to location user
                            .Zoom(17)                   // Sets the zoom
                            .Bearing(90)                // Sets the orientation of the camera to east
                            .Tilt(40)                   // Sets the tilt of the camera to 30 degrees
                            .Build();                   // Creates a CameraPosition from the builder
                    _mapFragment.Map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            /////----------------------------------Zooming camera to position user-----------------}
        }

        private void CenterMapOnMyLocation()
        {
            Criteria criteria = new Criteria();
            LocationManager locationManager = (LocationManager)GetSystemService(Context.LocationService);
            Location location = locationManager.GetLastKnownLocation(locationManager.GetBestProvider(criteria, false));

            //OnPositionChanged(location);
        }

        private void SetupMapIfNeeded()
        {
            if (_map == null)
            {
                _map = _mapFragment.Map;
                if (_map != null)
                {
                    //AddMonkeyMarkersToMap();
                    //AddInitialPolarBarToMap();

                    // Move the map so that it is showing the markers we added above.
                    // _map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(LocationForCustomIconMarkers[1], 2));
                }
            }
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
    }

}