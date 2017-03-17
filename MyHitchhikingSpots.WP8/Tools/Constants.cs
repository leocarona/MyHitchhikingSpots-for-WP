using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHitchhikingSpots.Tools
{
    public static class Constants
    {
        /// <summary>
        //     The time, in minutes, between the device location updates.
        /// </summary>
        public static double DeviceLocationUpdateInterval = 30;
        /// <summary>
        /// Default start zoom level for the maps. 
        /// </summary>
        public static int DefaultStartMapsZoomLevel = 8;
        /// <summary>
        /// Google uri for converting addresses into geographic coordinates. For more info on Geocoding refer to http://bit.ly/1dw83fz .
        /// </summary>
        public static string GoogleGeocoderUri = "http://maps.googleapis.com/maps/api/geocode/xml?latlng={0},{1}&sensor=false";

        /// <summary>
        /// Google uri for converting geographic coordinates into a human-readable address. For more info on Reverse Geocoding refer to http://bit.ly/1dw83fz .
        /// </summary>
        public static string GoogleReverseGeocoderUri = "http://maps.googleapis.com/maps/api/geocode/xml?latlng={0},{1}&sensor=false";

        public static string DefaultCulture = "en-US";
        /// <summary>
        /// Url for launching the map route app on WP8. 
        /// 0 = Latitude, 
        ///  1 = Longitude, 
        ///  2 = Street
        ///  3 = City
        ///  4 = State
        ///  5 = Zip
        /// </summary>
        public static string BingMapsRouteLaunchUrl = "directions://v2.0/route/destination/?latlon={0},{1}";

        public static string PrivateDbFileName = "privatedb.db";
        public static string LocationsDbFileName = "locations.db";

        public const string WP8ApplicationId = "c094b3ed-6622-439f-95e2-9fcaf446be3f";
        public const string WP8AuthenticationToken = "PTPP7U1JbpfrHn-bCS6tTA";
    }
}
