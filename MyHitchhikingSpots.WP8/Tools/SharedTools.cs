using System;
using System.IO;
using System.Linq;
//using MyHitchhikingSpots.ViewModels;
//using MyHitchhikingSpots.Models;



using System.Collections;
using System.Xml.Linq;
//using MyHitchhikingSpots.Core.Services.SyncStream;


#if NETFX_CORE || USE_WP8_NATIVE_SQLITE
using System.IO.IsolatedStorage;
using MyHitchhikingSpots.Models;
using MyHitchhikingSpots.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
//using System.IO.Compression;

#else
using System.IO.Compression;
using System.Xml;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using MyHitchhikingSpots.Models;
using MyHitchhikingSpots.ViewModels;
#endif


namespace MyHitchhikingSpots.Tools
{
    public static class Tool
    {
        static double _eQuatorialEarthRadius = 6378.1370D;
        static double _d2r = (Math.PI / 180D);

        public static bool InvokeFallibleMethod(Action command)
        {
            var noError = true;
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                command.Invoke();
            }
            catch(Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
                noError = false;
            }
            finally
            {
                // Cursor.Current = Cursors.Default;
            }

            return noError;
        }

        public static void CreateFolder(string folder)
        {
            if (!Directory.Exists(string.Format("{0}/{1}", DocumentsPath, folder)))
            {
                Directory.CreateDirectory(string.Format("{0}/{1}", DocumentsPath, folder));
            }
        }

        /// <summary>
        /// Converts kilometers to latitude degrees
        /// </summary>
        public static double KilometersToLatitudeDegrees(double kilometers)
        {
            double earthRadius = 6371.0;
            double radiansToDegrees = 180.0 / Math.PI;
            return (kilometers / earthRadius) * radiansToDegrees;
        }

        /// <summary>
        /// Converts kilometers to longitudinal degrees at a specified latitude
        /// </summary>
        public static double KilometersToLongitudeDegrees(double kilometers, double atLatitude)
        {
            double earthRadius = 6371.0;
            double degreesToRadians = Math.PI / 180.0;
            double radiansToDegrees = 180.0 / Math.PI;

            // derive the earth's radius at that point in latitude
            double radiusAtLatitude = earthRadius * Math.Cos(atLatitude * degreesToRadians);
            return (kilometers / radiusAtLatitude) * radiansToDegrees;
        }

        public static string DocumentsPath
        {
            get
            {
                //#if X86
                //                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                //#endif
#if WINDOWS_PHONE
                return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            }
        }

        public static string PersonalPath
        {
            get
            {
#if WINDOWS_PHONE
                return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif
            }
        }

        public static double HaversineInKM(double lat1, double long1, double lat2, double long2)
        {
            double dlong = (long2 - long1) * _d2r;
            double dlat = (lat2 - lat1) * _d2r;
            double a = Math.Pow(Math.Sin(dlat / 2D), 2D) + Math.Cos(lat1 * _d2r) * Math.Cos(lat2 * _d2r) * Math.Pow(Math.Sin(dlong / 2D), 2D);
            double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
            double d = _eQuatorialEarthRadius * c;

            return d;
        }

        public static string AllowOnlyDigits(string text, int length, bool allowDouble, bool deleteZeroFromStart = false)
        {
            string _text = text;

            if (deleteZeroFromStart)
                _text = RemoveBeginningZero(_text);

            if (!allowDouble)
            {
                if (_text.Length > 0 && !Char.IsDigit(_text.Last()))
                    _text = _text.Substring(0, _text.Length - 1);
            }
            else
            {
                if (_text.Length > 0 && (!Char.IsDigit(_text.Last()) && !Char.IsPunctuation(_text.Last())))
                    _text = _text.Substring(0, _text.Length - 1);
            }

            if (length == 0)
                return _text;

            if (_text.Length > length)
            {
                _text = _text.Substring(0, length);
            }

            return _text;
        }

        static string RemoveBeginningZero(string text)
        {
            if (text.StartsWith("0"))
                text = text.Substring(1);
            if (text.StartsWith("0"))
                return RemoveBeginningZero(text);
            else
                return text;
        }
#if WINDOWS_PHONE
		public static string LogError (string Message, Exception ex, string method)
        {
			var vm = ServiceContainer.Resolve<LogViewModel> ();
			return vm.CreateLogEntry (Message, ex, method);
        }

		public static string LogError (string Message, LogType logType, string method)
        {
			var vm = ServiceContainer.Resolve<LogViewModel> ();
			return vm.CreateLogEntry (Message, logType, method);
        }
#endif

        //public static void GZUnZip (filebase item)
        //{
        //    var basepath = Path.Combine (DocumentsPath, "Temp");
        //    var iFile = new FileInfo (item.filename);
        //    var filename = item.filename.Remove (item.filename.Length - iFile.Extension.Length, iFile.Extension.Length);
        //    var completePath = Path.Combine (basepath, filename);

        //    using (var outFile = File.Create (completePath)) {
        //        var str = new MemoryStream (item.data);

        //        using (var Decompress = new GZipStream (str, CompressionMode.Decompress))
        //            Decompress.CopyTo (outFile);
        //    }
        //}

        public static IDictionary GetReverseGeocodeResultAsDictionary(this XDocument doc)
        {
            var GeocodeResponse = doc.Element("GeocodeResponse"); //NOTE: In the xml file result of a reverse geocodification the parent tag is also called GeocodeResponse. Example: http://bit.ly/1j2BKac
            var element1 = GeocodeResponse.Element("status");


            if (element1 != null && element1.Value == "OK")
            { //element1.Value != "ZERO_RESULTS"
                var result = GeocodeResponse.Element("result");
                var xnList = result.Elements("address_component");

                return xnList
                    .ToDictionary(d => (string)d.Element("type").Value,
                    d => (string)d.Element("long_name").Value);
            }
            else
                return null;
        }

        public static IDictionary GetGeocodeResultAsDictionary(this XDocument doc)
        {
            var GeocodeResponse = doc.Element("GeocodeResponse");
            var element1 = GeocodeResponse.Element("status");


            if (element1 != null && element1.Value == "OK")
            { //element1.Value != "ZERO_RESULTS"
                var result = GeocodeResponse.Element("result");
                var xnList = result.Element("geometry").Element("location").Descendants();

                //Returns always two dictionary entries: lat and lng.
                return xnList
                    .ToDictionary(d => (string)d.Name.LocalName,
                                  d => (string)d.Value);
            }
            return null;
        }

#if !NETFX_CORE && !WINDOWS_PHONE
        public static IDictionary GetGeocodeResultAsDictionary(this XmlDocument doc)
        {
            //Returns always two dictionary entries: lat and lng.
            var d = new Dictionary<string, string>();
            d.Add("lat", doc.GetElementsByTagName("lat")[0].InnerText);
            d.Add("lng", doc.GetElementsByTagName("lng")[0].InnerText);
            return d;
        }
#endif
        /// <summary>
        /// Copies properties from one object til another object. Fx: from LB.Sales.custtable to LB.POS.custtable. 
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object (same object type, but different namespace)</param>
        public static void SetProperties(object source, object target)
        {
            var customerType = target.GetType();
            foreach (var prop in source.GetType().GetProperties())
            {
                var propGetter = prop.GetGetMethod();
                var propSetter = customerType.GetProperty(prop.Name).GetSetMethod();
                var valueToSet = propGetter.Invoke(source, null);
                propSetter.Invoke(target, new[] { valueToSet });
            }
        }

        //        public static GeoCoordinate ConvertToGeoCoordinate(this Geocoordinate geocoordinate)
        //        {
        //            return new GeoCoordinate
        //                (
        //                geocoordinate.Latitude,
        //                geocoordinate.Longitude,
        //                geocoordinate.Altitude ?? Double.NaN,
        //                geocoordinate.Accuracy,
        //                geocoordinate.AltitudeAccuracy ?? Double.NaN,
        //                geocoordinate.Speed ?? Double.NaN,
        //                geocoordinate.Heading ?? Double.NaN
        //                );
        //        }
        //
        //        public static GeoCoordinate ConvertToGeoCoordinate(this MapItem geocoordinate)
        //        {
        //            return new GeoCoordinate
        //              (
        //                geocoordinate.Latitude,
        //                geocoordinate.Longitude
        //                );
        //        }

        public static MemoryStream ConvertToMemoryStream(this Stream stream)
        {
            MemoryStream result = null;
            if (stream != null)
            {
                try
                {
                    result = new MemoryStream(0x10000);

                    byte[] buffer = new byte[0x1000];
                    int bytes;
                    while ((bytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        result.Write(buffer, 0, bytes);
                    }
                }
                catch(Exception ex)
                {
                    Tool.WriteExceptionMessagesToOutputBox(ex);
                }
            }

            return result;
        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }


        public static void BinarySerialize<T>(this FileStream file, T obj)
        {
#if WINDOWS_PHONE
             file.Serialize<T>(obj);

            //using (var writer = System.Xml.XmlDictionaryWriter.CreateBinaryWriter(file))
            //{
            //var dcs = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            //dcs.WriteObject(writer, obj);
            //writer.Flush();
            //}
#else
            var serializer = new BinaryFormatter();
            serializer.Serialize(file, obj);
#endif
        }

        public static T BinaryDeserialize<T>(this FileStream file)
        {
#if WINDOWS_PHONE
            return file.Deserialize<T>();

            //using (var reader = System.Xml.XmlDictionaryReader.CreateBinaryReader(
            //    file, System.Xml.XmlDictionaryReaderQuotas.Max))
            //{
            //    var dcs = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            //    return (T)dcs.ReadObject(reader);
            //}
#else
            var binFormatter = new BinaryFormatter();
            return (T)binFormatter.Deserialize(file);
#endif
        }

        public static void Serialize<T>(this FileStream file, T obj)
        {
#if WINDOWS_PHONE
            using (var writer = System.Xml.XmlDictionaryWriter.CreateBinaryWriter(file))
            {
                var ser = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
                ser.WriteObject(file, obj);
                writer.Close();
            }
#else
            //TODO: implement this method
            throw new NotFiniteNumberException();
#endif
        }

        public static T Deserialize<T>(this FileStream file)
        {
            T result;
#if WINDOWS_PHONE
            using (var reader = System.Xml.XmlDictionaryReader.CreateTextReader(file, System.Xml.XmlDictionaryReaderQuotas.Max))
            {
                var ser = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
                // Deserialize the data and read it from the instance.
                result = (T)ser.ReadObject(reader, true);
            }
#else
            //TODO: implement this method
            throw new NotFiniteNumberException();
#endif
            return result;
        }

        public static void Serialize<T>(this FileStream file, T obj, bool UseBinarySerialization)
        {
            if (UseBinarySerialization)
                file.BinarySerialize<T>(obj);
            else
                file.Serialize<T>(obj);
        }

        public static T Deserialize<T>(this FileStream file, bool UseBinarySerialization)
        {
            if (UseBinarySerialization)
                return file.BinaryDeserialize<T>();
            else
                return file.Deserialize<T>();
        }

        //
        // Summary:
        //     Searches for an element that matches the conditions defined by the specified
        //     predicate, and returns the zero-based index of the first occurrence within
        //     the entire System.Collections.Generic.IEnumerable<object>.
        //
        // Parameters:
        //   match:
        //     The System.Predicate<dynamic> delegate that defines the conditions of the element
        //     to search for.
        //
        // Returns:
        //     The zero-based index of the first occurrence of an element that matches the
        //     conditions defined by match, if found; otherwise, –1.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     match is null.
        public static int IndexOf<T>(this System.Collections.Generic.IEnumerable<T> list, Predicate<T> match)
        {
            return list.ToList().FindIndex(match);
        }


#if ANDROID
        public static bool IsOnline(this Android.Content.Context context)
        {
            //Android IsOnline verification copied from here: http://stackoverflow.com/a/2789821/1094261
            var cm = (Android.Net.ConnectivityManager)context.GetSystemService(Android.Content.Context.ConnectivityService);

            var ni = cm.ActiveNetworkInfo;
            if (ni == null)
            {
                // There are no active networks.
                return false;
            }
            return true;
        }
#else
        public static bool IsOnline()
        {
            bool isConnectionOnline = false;

#if WINDOWS_PHONE 
            isConnectionOnline = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
#elif !WINDOWS_PHONE
            //var reach = Reachability.InternetConnectionStatus ();
            //isConnectionOnline = (reach != NetworkStatus.NotReachable);
#endif

            return isConnectionOnline;
        }
#endif

        public static void RunInUIThread(Action action)
        {
#if WINDOWS_PHONE
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => action());
#elif X86 
		    System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(action);
#elif ANDROID
            RunInUIThread(() => action());
#else
            //TODO: make action run on a UI thread 
            EnsureInvokedOnMainThread(action);
#endif
        }

#if !WINDOWS_PHONE && !X86 && !ANDROID
        static bool IsMainThread()
        {
            return NSThread.Current.IsMainThread;
            //return Messaging.bool_objc_msgSend(GetClassHandle("NSThread"), new Selector("isMainThread").Handle);
        }


        public static void EnsureInvokedOnMainThread(Action action)
        {
            if (IsMainThread())
            {
                action();
                return;
            }
            using (var pool = new NSAutoreleasePool())
            {
                try
                {
                    pool.BeginInvokeOnMainThread(() => action());
                }
                catch
                {
                }
            }


        }
#endif

        public static TimeSpan GetDuration(DateTime time1, DateTime time2)
        {
            if (time2 == null || time1 == null)
                return new TimeSpan();
            else if (time1 > time2) //This should happen, but we're considering also this scenarium to avoid negative numbers
                return time1.Subtract(time2);
            else
                return time2.Subtract(time1);
        }

        public static TimeSpan GetDuration(TimeSpan time1, TimeSpan time2)
        {
            if (time2 == null || time1 == null)
                return new TimeSpan();
            else if (time1 > time2) //This should happen, but we're considering also this scenarium to avoid negative numbers
                return time1.Subtract(time2);
            else
                return time2.Subtract(time1);
        }

        /// <summary>
        /// Validates and converts the given string into double
        /// </summary>
        /// <returns>True if the txtHours is a valid hour, false if not.</returns>
        public static bool TryConvertHours(string txtHours, out double result, out string validationErrorMessage)
        {
            if (!double.TryParse(txtHours, out result) || result < 0 || result > 23)
            {
                validationErrorMessage = "Hours must be a number equal or between 0 and 23.";
                return false;
            }
            else
            {
                validationErrorMessage = "";
                return true;
            }
        }

        /// <summary>
        /// Validates and converts the given string into double
        /// </summary>
        /// <returns>True if the txtMinutes is a valid minute, false if not.</returns>
        public static bool TryConvertMinutes(string txtMinutes, out double result, out string validationErrorMessage)
        {
            if (!double.TryParse(txtMinutes, out result) || result < 0 || result > 59)
            {
                validationErrorMessage = "Minutes must be a number equal or between 0 and 59.";
                return false;
            }
            else
            {
                validationErrorMessage = "";
                return true;
            }
        }

        /// <summary>
        /// Validates and converts the given string into double
        /// </summary>
        /// <returns>True if the txtHours is a valid hour, false if not.</returns>
        public static bool TryConvertHours(string txtHours, out double result)
        {
            var errMsg = "";
            return TryConvertMinutes(txtHours, out result, out errMsg);
        }

        /// <summary>
        /// Validates and converts the given string into double
        /// </summary>
        /// <returns>True if the txtMinutes is a valid minute, false if not.</returns>
        public static bool TryConvertMinutes(string txtMinutes, out double result)
        {
            var errMsg = "";
            return TryConvertMinutes(txtMinutes, out result, out errMsg);
        }

        public static void WriteExceptionMessagesToOutputBox(Exception e)
        {
            List<String> exceptions = e.GetExceptionMessagesAsList();
            Debug.WriteLine("=> An exception was thrown:");
            exceptions.ForEach(errMsg => Debug.WriteLine("+ " + errMsg));
        }

        public static String GetExceptionMessagesAsString(this Exception ex)
        {
            var exceptions = ex.GetExceptionMessagesAsList();
            var result = "=> An exception was thrown:";
            exceptions.ForEach(errMsg => result += "+ " + errMsg);
            return result;
        }

        /// <summary>
        /// Checks if SelectedMapItem is a clean object (no data was set yet) or if it already has either the basic information set or the full information set. 
        /// </summary>
        /// <returns></returns>
        public static SpotDataState GetDataState(this MapItem item)
        {
            if (item == null)
                return SpotDataState.Unknown;
            else
                if (item.AttemptResult != MyHitchhikingSpots.Interfaces.AttemptResult.Unknown)
                    return SpotDataState.FullInfoInserted;
                else
                    if (item.Hitchability != Interfaces.Hitchability.Unknown)
                        return SpotDataState.BasicInfoInserted;
                    else
                        return SpotDataState.Clean;
        }

        public static List<String> GetExceptionMessagesAsList(this Exception ex)
        {
            List<String> exceptions = new List<String>();

            while (ex != null)
            {
                exceptions.Add(ex.Message);
                ex = ex.InnerException;
            }
            return exceptions;
        }

        public static string GetReverseGeocoderUri(double latitude, double longitude)
        {
            return Constants.GoogleReverseGeocoderUri.Format(latitude, longitude);
        }

        public static string Format(this String str, double latitude, double longitude)
        {
#if !NETFX_CORE && !WINDOWS_PHONE
            var culture = System.Globalization.CultureInfo.GetCultureInfo(Constants.DefaultCulture);
#else
                    var culture = new System.Globalization.CultureInfo(Constants.DefaultCulture); ;
#endif
            return String.Format(str, latitude.ToString(culture), longitude.ToString(culture));
        }

        public static string SubstringCut(this string snippet, int maxChars)
        {
            var note = "";

            try
            {
                if (!String.IsNullOrEmpty(snippet))
                {
                    if (snippet.Count() <= maxChars)
                        note = snippet;
                    else
                        note = snippet.Substring(0, maxChars - 4) + "..";
                }
            }
            catch(Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }

            return note;
        }

        public static string GetOnlyFirstLine(this string snippet)
        {
            var note = "";

            try
            {
                if (!String.IsNullOrEmpty(snippet))
                {
                    //Remove all "\n" from the biggining of the text, if there's any
                    while (snippet.Substring(0, 1) == "\n")
                        snippet = snippet.Substring(1);

                    var firstBreakLineIndex = snippet.IndexOf("\n");

                    if (firstBreakLineIndex < 0)
                        note = snippet;
                    else
                        note = snippet.Substring(0, firstBreakLineIndex) + "..";
                }
            }
            catch(Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }

            return note;
        }
    }



}

