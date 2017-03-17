using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
//using MyHitchhikingSpots.Models;
//using MyHitchhikingSpots.Utilities;
using MyHitchhikingSpots.Tools;

namespace MyHitchhikingSpots.Tools
{
    public class GeoCoder
    {
        public delegate void GeocodeCompleted(Dictionary<string, string> dictionary);
        public event GeocodeCompleted OnGeocodeCompleted;
        public event GeocodeCompleted OnReverseGeocodeCompleted;



        static string GetUrl(string url)
        {
            // https://github.com/xamarin/monotouch-samples/blob/master/HttpClient/DotNet.cs
            string result = string.Empty;
            var request = WebRequest.Create(url);

#if !NETFX_CORE && !WINDOWS_PHONE
            //The lines bellow are commented because they couldn't compile for WP8 project
            var req = request.GetResponse();
            StreamReader sr = new StreamReader(req.GetResponseStream());
            result = sr.ReadToEnd();
#endif

            return result;
        }

        /// <summary>
        /// Converts address into geographic coordinates synchronously using XmlDocument. For more info on Geocoding refer to http://bit.ly/1dw83fz 
        /// </summary>
        /// <param name="url">The uri containing the address to be converted.</param>
        public static bool Geocode(string url, ref double longitude, ref double latitude)
        {
#if !NETFX_CORE && !WINDOWS_PHONE && !ANDROID
            //The lines bellow are commented because they couldn't compile for WP8 project
            string lat = null;
            string lng = null;
            string xmlString = string.Empty;

            try
            {
                xmlString = GetUrl(url);
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(xmlString);
               
                var values = xd.GetGeocodeResultAsDictionary();
                if(values != null)
                {
                    lat = (string)values["lat"];
                    lng = (string)values["lng"];
                }
            }
            catch { }

            if (lng != null && lat != null)
            {
                latitude = FixDotNotation(lat);
                longitude = FixDotNotation(lng);

                return true;
            }
#endif
            throw new NotImplementedException();
            return false;
        }

        /// <summary>
        /// Converts address into geographic coordinates asynchronously using LINQ. For more info on Geocoding refer to http://bit.ly/1dw83fz 
        /// </summary>
        /// <param name="url">The uri containing the address to be converted.</param>
        /// <param name="callback">The method to be called when the geocodification is completed.</param>
        public static void GeocodeAsync(string url, Action<Dictionary<string, string>> callback)
        {
            var gc = new GeoCoder();
            gc.OnReverseGeocodeCompleted += new GeocodeCompleted(callback);
            gc.GeocodeAsync(url);
        }

        /// <summary>
        /// Converts address into geographic coordinates asynchronously using LINQ. For more info on Geocoding refer to http://bit.ly/1dw83fz 
        /// </summary>
        /// <param name="request">The uri containing the address to be converted.</param>
        public void GeocodeAsync(string request)
        {
            if (OnGeocodeCompleted == null)
                throw new MissingMethodException("You must set the event handler OnGeocodeCompleted to a method that will receive the result of this geocodification ");

            var webClient = new WebClient();
            webClient.DownloadStringCompleted += GeocodeCurrentPosition_Completed;
            webClient.DownloadStringAsync(new Uri(request));
        }

        private void GeocodeCurrentPosition_Completed(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var feedXml = System.Xml.Linq.XDocument.Parse(e.Result);
                var dictionary = feedXml.GetGeocodeResultAsDictionary();
                OnGeocodeCompleted(dictionary as Dictionary<string, string>);
                throw new NotImplementedException();
            }
        }


        //Instead of calling LocateGoogle(..), call Geocode(query, out location).
        //public static bool LocateGoogle(string query, out LocationHolder location)
        //{
        //    return Geocode(query, out location);
        //}





        /// <summary>
        /// Converts geographic coordinates into human-readable address synchronously using LINQ. For more info on Reverse Geocoding refer to http://bit.ly/1dw83fz 
        /// </summary>
        /// <param name="url">The uri containing the coordinates to be converted.</param>
        public static bool ReverseGeocode(string url, ref string city, ref string country)
        {
#if !NETFX_CORE && !WINDOWS_PHONE && !ANDROID
            try{
                XmlDocument doc = new XmlDocument();
				doc.Load(url);

                var values = doc.GetGeocodeResultAsDictionary();

                if(values != null)
                {
					city = (string)values["locality"];
					country = (string)values["country"];

                    return true;
                }
            }
			catch {}
#else
            new Exception("Use ReverseGeocodeAsync instead of ReverseGeocode.");
#endif
            return false;
        }
        
        /// <summary>
        /// Converts geographic coordinates into human-readable address asynchronously using XmlDocument. For more info on Reverse Geocoding refer to http://bit.ly/1dw83fz 
        /// </summary>
        /// <param name="url">The uri containing the coordinates to be converted.</param>
        public void ReverseGeocodeAsync(string url)
        {
            if (OnReverseGeocodeCompleted == null)
                throw new MissingMethodException("You must set the event handler OnReverseGeocodeCompleted to a method that will receive the result of this reverse geocodification ");

            var webClient = new WebClient();
            webClient.DownloadStringCompleted += ReverseGeocodeCurrentPosition_Completed;
            webClient.DownloadStringAsync(new Uri(url));
        }

        /// <summary>
        /// Converts geographic coordinates into human-readable address. For more info on Reverse Geocoding refer to http://bit.ly/1dw83fz 
        /// </summary>
        /// <param name="request">The uri containing the coordinates to be converted.</param>
        /// <param name="callback">The method to be called when the reverse geocodification is completed.</param>
        public static void ReverseGeocodeAsync(string request, Action<Dictionary<string, string>> callback)
        {
            var gc = new GeoCoder();
            gc.OnReverseGeocodeCompleted +=
                ( e) =>
                {
                    callback(e); };
                //new GeocodeCompleted(callback);
            gc.ReverseGeocodeAsync(request);
        }

        private void ReverseGeocodeCurrentPosition_Completed(object sender, DownloadStringCompletedEventArgs e)
        {
            Dictionary<string, string> res = null;
            if (e.Error == null)
            {
                var feedXml = System.Xml.Linq.XDocument.Parse(e.Result);
                var dictionary = feedXml.GetReverseGeocodeResultAsDictionary();
                res = dictionary as Dictionary<string, string>;
            }
            OnReverseGeocodeCompleted(res);
        }


    }
}

