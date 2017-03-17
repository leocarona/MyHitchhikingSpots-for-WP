using System;
using Xamarin.Geolocation;
using System.Threading.Tasks;
using System.Xml;
using MyHitchhikingSpots.Models;
using System.Threading;
using MyHitchhikingSpots.Tools;
//using MyHitchhikingSpots.Utilities;
using System.Globalization;
using System.Net;
using MyHitchhikingSpots.Databases;
using System.Linq;

#if NETFX_CORE || USE_WP8_NATIVE_SQLITE
using Timer = MyHitchhikingSpots.Tools.Timer;
//using System.Net;
using MyHitchhikingSpots.Interfaces;
using System.Collections.Generic;

#else
using System.Timers;
using Timer = System.Timers.Timer;
using MyHitchhikingSpots.Interfaces;
using System.Collections.Generic;
#endif

namespace MyHitchhikingSpots.ViewModels
{
    public class LocationViewModel : ViewModelBase
    {
        Position _currentPosition;
        PrivateDB db;
        //readonly ILocateService locService;

        /// <summary>
        /// The device current location
        /// </summary>
        public Position CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                _currentPosition = value;
                CurrentPositionChanged();
            }
        }

        public string Country { get; set; }

        public string City { get; set; }

        Timer _saveLocationTimer;
        public Geolocator locator { get; private set; }


        public LocationViewModel()
        {
            db = ServiceContainer.Resolve<PrivateDB>();
            //locService = ServiceContainer.Resolve<ILocateService>();

            _saveLocationTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            _saveLocationTimer.Elapsed += _saveLocationTimer_Elapsed;
            _saveLocationTimer.Start();

            //try
            //{
            //    _currentPosition = GetUserLastStoredPosition();
            //}
            //catch (Exception ex)
            //{
            //    Tool.WriteExceptionMessagesToOutputBox(ex);
            //}
        }

        async void _saveLocationTimer_Elapsed(object sender, EventArgs e)
        {
            try
            {
                var locHolder = new LocationHolder()
                    {
                        RequestString = CurrentPositionReverseGeocodeUri,
                        Latitude = _currentPosition.Latitude,
                        Longitude = _currentPosition.Longitude,
                        Accuracy = _currentPosition.Accuracy,
                        Heading = _currentPosition.Heading,
                        Altitude = _currentPosition.Altitude,
                        AltitudeAccuracy = _currentPosition.AltitudeAccuracy,
                        Speed = _currentPosition.Speed,
                        DateTime = DateTime.Now
                    };
                await db.GetConnection().InsertAsync(locHolder);
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
        }

        public void SetGeolocator(Geolocator obj, bool startListening = true)
        {
            locator = obj;
            locator.PositionChanged += (s, e) => OnPositionChanged(e.Position);

            if (startListening)
                StartGeolocatorListener();
        }

        public void StartGeolocatorListener()
        {
            if (locator != null)
                locator.StartListening(
                  minTime: TimeSpan.FromMinutes(1).Milliseconds, //minTime: A hint for the minimum time between position updates in milliseconds.
                  minDistance: 10 //minDistance: A hint for the minimum distance between position updates in meters.
                  );
        }

        public void StopGeolocatorListener()
        {
            if (locator != null)
                locator.StopListening();
        }

        public Task<List<LocationHolder>> LoadSavedLocations()
        {
            return db.GetConnection().Table<LocationHolder>().OrderBy(l => l.DateTime).ToListAsync();
        }

        public string GetDBFileName() { return db.DBFileName; }

        public void CloseConnection()
        {
            db.CloseConnection();
        }

        public void OpenNewConnection()
        {
            ServiceContainer.Register<PrivateDB>(() => new PrivateDB());
            db = ServiceContainer.Resolve<PrivateDB>();
        }

        public async Task<Position> GetUserLastStoredPosition()
        {
            Position currentPosition = null;
            try
            {
                // var lastLocation = db.GetConnection().QueryAsync<LocationHolder>("SELECT * FROM LocationHolder WHERE ID = (SELECT MAX(ID) FROM LocationHolder);").Result;
                var locations = await db.GetConnection().Table<LocationHolder>().ToListAsync();

                if (locations != null && locations.Any())
                {
                    var lastLocation = locations.Last();
                    if (lastLocation != null)
                    {
                        currentPosition = new Position
                        {
                            Latitude = lastLocation.Latitude,
                            Longitude = lastLocation.Longitude
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
            return currentPosition;

        }



        string CurrentPositionReverseGeocodeUri
        {
            get
            {
                if (_currentPosition == null)
                    return String.Empty;
                else
                {
                    return Tool.GetReverseGeocoderUri(_currentPosition.Latitude, _currentPosition.Longitude);
                }
            }
        }



        public Task UpdateCurrentLocationThreadAsync()
        {
            return locator.GetPositionAsync(timeout: 10000, cancelToken: new CancellationToken(), includeHeading: false)
                .ContinueWith(t =>
                {
                    if (!t.IsFaulted && t.Result != null)
                        OnPositionChanged(t.Result);
                    //else
                    //    CurrentPosition = await GetUserLastStoredPosition(); //Use last known position if failed to get the current one
                });
        }


        private void OnPositionChanged(Position currentLocation)
        {
            try
            {
                //if (currentLocation != null)
#if WINDOWS_PHONE
                WP8Tool.RunOnUiThread(() =>
                {
                    CurrentPosition = currentLocation;
                    UpdateCurrentLocationData(); //Reset City and Country values
                });
#else
                CurrentPosition = currentLocation;
                UpdateCurrentLocationData(); //Reset City and Country values
#endif

                //if (currentLocation != null && currentLocation.Latitude != 0 && currentLocation.Longitude != 0)
                //{
                //    var location = GetReverseGeocodedUri(CurrentPositionReverseGeocodeUri);

                //    if (location != null)
                //    {
                //        //If we already have data for the current position, use it.
                //        UpdateCurrentLocationData(location);

                //        //Call user's subscribed methods
                //        CurrentPositionCompleted();
                //    }
                //    else
                //    {
                //var locHolder = new LocationHolder()
                //{
                //    RequestString = CurrentPositionReverseGeocodeUri,
                //    Latitude = _currentPosition.Latitude,
                //    Longitude = _currentPosition.Longitude
                //};
                //db.GetConnection().InsertAsync(locHolder).Wait();

                ////Reverse geocode of the current location to get the City and Country asynchronously.
                //GeoCoder.ReverseGeocodeAsync(CurrentPositionReverseGeocodeUri, LoadReverseGeocodeResult);

                //}
                //}

            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
        }

        private LocationHolder GetReverseGeocodedUri(string reverseGeocodeUri)
        {
            var location = db.GetConnection().Table<LocationHolder>().Where(i => i.RequestString == reverseGeocodeUri).FirstOrDefaultAsync().Result;
            return location;
        }



        /// <summary>
        /// Update the location data by copying values from locHolder or reset the location data to default values.
        /// </summary>
        /// <param name="locHolder">The object to copy the data from, or null to set the data to default values.</param>
        void UpdateCurrentLocationData()
        {
            if (CurrentPosition != null)
            {
                try
                {
                    var uri = Tool.GetReverseGeocoderUri(CurrentPosition.Latitude, CurrentPosition.Longitude);
                    GeoCoder.ReverseGeocodeAsync(uri, ReverseGeocodeCompleted);
                }
                catch (Exception ex)
                {
                    Tools.Tool.WriteExceptionMessagesToOutputBox(ex);
                }
            }
        }
        void ReverseGeocodeCompleted(Dictionary<string, string> result)
        {
            try
            {
                if (result != null)
                {
                    foreach (var i in result)
                    {
                        switch (i.Key)
                        {
                            case "country":
                                Country = result["country"];
                                break;
                            case "locality":
                                City = result["locality"];
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Tool.WriteExceptionMessagesToOutputBox(ex);
            }


            OnPropertyChanged("City");
            OnPropertyChanged("Country");
        }


        /// <summary>
        /// Event for when the current position is updated. Here the reverse geocodification hasn't completed yet, consider using <see cref="OnCurrentPositionCompleted"/> in that case. 
        /// </summary>
        public event EventHandler OnCurrentPositionChanged;

        /// <summary>
        /// Event for when the coordinate is fetched and its reverse geocodification is completed. If you just need the coordinate, use <see cref="OnCurrentPositionChanged"/> instead.
        /// </summary>
        public event EventHandler OnCurrentPositionCompleted;

        protected void CurrentPositionChanged()
        {
            OnPropertyChanged("CurrentPosition");

            if (OnCurrentPositionChanged != null)
                OnCurrentPositionChanged(this, EventArgs.Empty);
        }

        protected void CurrentPositionCompleted()
        {
            if (OnCurrentPositionCompleted != null)
                OnCurrentPositionCompleted(this, EventArgs.Empty);
        }



    }
}

