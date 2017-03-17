using System;

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
//using MyHitchhikingSpots.Models;
using Xamarin.Geolocation;
using MyHitchhikingSpots.Interfaces;
//using MyHitchhikingSpots.ViewModels;
using MyHitchhikingSpots.Tools;
using System.Diagnostics;

namespace MyHitchhikingSpots
{
    public class LocateService : ILocateService
    {
        public Task<Position> GetCurrentLocation()
        {
            Position result;

            try
            {
                var locator = new Geolocator();
                return locator.GetPositionAsync(timeout: 10000, cancelToken: new CancellationToken(), includeHeading: false);
            }
            catch (Exception ex)
            {
                //NOTE: 
                // Remember that in order to call GetPositionAsyn on Windows Phone you 
                // must had set up <Capability Name="ID_CAP_LOCATION" /> on the AppManifest file.

                result = null;

                Tool.WriteExceptionMessagesToOutputBox(ex);
            }

            return Task.Factory.StartNew(() =>
            {
                return result;
            });
        }
    }
}

