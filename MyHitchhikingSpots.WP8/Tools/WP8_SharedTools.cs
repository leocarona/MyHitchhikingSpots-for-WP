using MyHitchhikingSpots.Models;
using Nokia.Phone.HereLaunchers;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace MyHitchhikingSpots.Tools
{
    public static class WP8Tool
    {
        public static GeoCoordinate ConvertToGeoCoordinate(this Geocoordinate geocoordinate)
        {
            return new GeoCoordinate
                (
                geocoordinate.Latitude,
                geocoordinate.Longitude,
                geocoordinate.Altitude ?? Double.NaN,
                geocoordinate.Accuracy,
                geocoordinate.AltitudeAccuracy ?? Double.NaN,
                geocoordinate.Speed ?? Double.NaN,
                geocoordinate.Heading ?? Double.NaN
                );
        }

        public static GeoCoordinate ConvertToGeoCoordinate(this MapItem geocoordinate)
        {
            return new GeoCoordinate
              (
                geocoordinate.Latitude,
                geocoordinate.Longitude
                );
        }

        public static GeoCoordinate ConvertToGeoCoordinate(this LocationHolder geocoordinate)
        {
            return new GeoCoordinate
              (
                geocoordinate.Latitude,
                geocoordinate.Longitude
                );
        }


        public static System.Device.Location.GeoCoordinate ConvertToGeoCoordinate(this Xamarin.Geolocation.Position geocoordinate)
        {
            return new System.Device.Location.GeoCoordinate
              (
                geocoordinate.Latitude,
                geocoordinate.Longitude,
                geocoordinate.Altitude,
                Double.NaN,
                Double.NaN, //Maybe geocoordinate.AltitudeAccuracy corresponds to the vertical accuracy?
                geocoordinate.Speed,
                Double.NaN //Maybe geocoordinate.Heading corresponds to the course of the geographic coordinate?
                );
        }

        /// <summary>
        /// Launches the specified app to app URI 
        /// uses the appToAppUri with Nokia devices, and webFallbackUri with others (when available).
        /// </summary>
        /// <param name="appToAppUri">The app to app URI.</param>
        [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule", Justification = "Adding the SecurityCritical attribute was over the top")]
        public static void Launch(Uri appToAppUri)
        {
            //Method copied from the HereLauncher sample project available at https://github.com/nokia-developer/here-launchers

            if (Environment.OSVersion.Version.Major >= 8)
            {
                string _appid = Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId;
                string _uri = appToAppUri.OriginalString;

                if (_appid != "")
                {
                    if (_uri.EndsWith("/"))
                    {
                        _uri += "?appid=" + _appid;
                    }
                    else
                    {
                        _uri += "&appid=" + _appid;
                    }
                }
                else
                {
#if DEBUG
                    Debug.WriteLine("Warning: The Here Launcher API requires your application to have a valid Application ID and Authentication Token. See http://msdn.microsoft.com/en-US/library/windowsphone/develop/jj207033(v=vs.105).aspx#BKMK_appidandtoken");
#else
                    //throw new InvalidOperationException("The application did not set an Application ID. See http://msdn.microsoft.com/en-US/library/windowsphone/develop/jj207033(v=vs.105).aspx#BKMK_appidandtoken");
#endif
                }

                Debug.WriteLine("Launching Nokia App with " + _uri);
#pragma warning disable 4014  // Disable as we're launching the app - we don't want to wait
                Windows.System.Launcher.LaunchUriAsync(new Uri(_uri));
#pragma warning restore 4014  // CS4014
                return;

            }
            else
            {
                throw new InvalidOperationException("This API is intented to work only from Windowns Phone 8 and newer");
            }
        }

        /// <summary>
        /// This method opens HereMaps and shows a spot at the given coordinate.
        /// </summary>
        /// <param name="coord"></param>
        public static void ShowRouteTo(GeoCoordinate coord)
        {
            try
            {
                DirectionsRouteDestinationTask routeTo = new DirectionsRouteDestinationTask();
                routeTo.Destination = coord;
                routeTo.Mode = RouteMode.Car;
                routeTo.Show();
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
        }

       


        public static string ReadImageFromInstallationFolder(string imagePath)
        {// Reference the installation folder with the appdata URI scheme.
            Uri myInitialSettingsFileUri = new System.Uri("appdata:/AppSetup/initialSettings1.xml");

            // Get a file from the installation folder with the ms-appx URI scheme.
            //var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///AppSetup/initialSettings2.xml"));
            return System.IO.File.OpenText(imagePath).ReadToEnd();
        }

        public static byte[] ReadFileFromIsolatedStorage(string filePath)
        {
            byte[] byteFile = null;

            using (var iso = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(filePath))
                {
                    using (var img = iso.OpenFile(filePath, System.IO.FileMode.Open))
                    {
                        // Initialize the buffer
                        byteFile = new byte[img.Length];

                        // Copy the file from installation folder to isolated storage.
                        img.Read(byteFile, 0, byteFile.Length);
                    }
                }
            }
            return byteFile;
        }

        public static void WriteFileToIsolatedStorage(Stream input, string fileName, string outputDirectoryPath, bool replaceIfAlreadyExists)
        {
            if (fileName.Contains("/"))
                throw new Exception("The file name should just contain the file name and its extension. For the input or output directories, use the other paratmer options.");

            //Make sure that the fileName is not in the outputPath
            outputDirectoryPath = outputDirectoryPath.Replace(fileName, "");

            string outputFile_Path = "";

            if (!String.IsNullOrEmpty(outputDirectoryPath))
                outputFile_Path = Path.Combine(outputDirectoryPath, fileName);
            else
                outputFile_Path = fileName;


            // Obtain the virtual store for the application.
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!iso.DirectoryExists(outputDirectoryPath))
                    iso.CreateDirectory(outputDirectoryPath);

                //If the output file does not exist or must be replaced
                if ((replaceIfAlreadyExists || !iso.FileExists(outputFile_Path)))
                {
                    // Create stream for the new file in the isolated storage
                    using (IsolatedStorageFileStream output = iso.OpenFile(outputFile_Path, FileMode.OpenOrCreate))
                    {
                        // Initialize the buffer
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the file from installation folder to isolated storage.
                        while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            output.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }
            }
        }

        public static void WriteFileToIsolatedStorage(byte[] inputBytes, string fileName, string outputPath, bool replaceIfAlreadyExists)
        {
            using (var inputFile = new MemoryStream(inputBytes))
                WriteFileToIsolatedStorage(inputFile, fileName, outputPath, replaceIfAlreadyExists);
        }

        public static string GetHtmlContentToDefaultTemplate(string strToReplace, string content)
        {
            var result = "";

            if (content != null)
            {
                try { result = ReadImageFromInstallationFolder("template/template.html"); 
             }
                catch(Exception ex) {
                    Tool.WriteExceptionMessagesToOutputBox(ex);
                }

                if (result != "")
                {
                    result = result.Replace("<!doctype html>", "<html>");
                    result = result.Replace(strToReplace, content);
                }
            }

            return result;
        }


        public static void CopyFileFromInstallationFolder(string fileName, string dir = null, bool replaceIfAlreadyExists = false)
        {
            if (fileName.Contains("/"))
                throw new Exception("If the file is within a directory, inform it as 'dir' parameter otherwise an exception would be thrown if that directory were not created yet.");

            // Obtain the virtual store for the application.
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var filePath = "";

                if (!String.IsNullOrEmpty(dir))
                {
                    if (!iso.DirectoryExists(dir))
                        iso.CreateDirectory(dir);
                    filePath += dir + "/";
                }

                filePath += fileName;

                //THIS METHOD WAS COPIED FROM http://bit.ly/1vdlNRC

                if (replaceIfAlreadyExists || !iso.FileExists(filePath))
                {
                    // Create stream for the file in the installation folder.
                    var installFolderFilePath = new Uri(filePath, UriKind.Relative);
                    using (Stream input = System.Windows.Application.GetResourceStream(installFolderFilePath).Stream)
                    {
                        // Create stream for the new file in the isolated storage
                        using (IsolatedStorageFileStream output = iso.CreateFile(filePath))
                        {
                            // Initialize the buffer
                            byte[] readBuffer = new byte[4096];
                            int bytesRead = -1;

                            // Copy the file from installation folder to isolated storage.
                            while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                            {
                                output.Write(readBuffer, 0, bytesRead);
                            }
                        }
                    }
                }
            }
        }

        public static void MoveFileFromDirectory(string fileName, string inputPath, string outputPath, bool replaceIfAlreadyExists = true)
        {
            string inputFile_Path = "";

            if (!String.IsNullOrEmpty(inputPath))
                inputFile_Path = inputPath + "/" + fileName;
            else
                inputFile_Path = fileName;

            // Create stream for the file in the installation folder.
            // Obtain the virtual store for the application.
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(inputFile_Path))
                {
                    using (var inputFile = iso.OpenFile(inputFile_Path, FileMode.Open))
                        WriteFileToIsolatedStorage(inputFile, fileName, outputPath, replaceIfAlreadyExists);

                    iso.DeleteFile(inputFile_Path);
                }
            }
        }



        //public static byte[] GetStoredBackgroundImageFromIsolatedStorage()
        //{
        //    return ReadFileFromIsolatedStorage(MyHitchhikingSpots.Core.WP8.Utilities.WP8Constants.DefaultCustomBackgroundImageNamePath);
        //}

        public static byte[] ReadAllBytes(string fileName)
        {
            return ReadFileFromIsolatedStorage(fileName);
        }

        public static void WriteAllBytes(string outputPath, byte[] bytes)
        {
            var fileName = Path.GetFileName(outputPath);

            WriteFileToIsolatedStorage(bytes, fileName, outputPath, true);
        }

        public static byte[] ReadAsByteArray(this Stream input)
        {
            //Method coppied from here - http://stackoverflow.com/a/221941

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static BitmapImage ConvertToBitmapImage(this byte[] bytes)
        {
            var result = new BitmapImage();

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                result.SetSource(stream);
            }
            return result;
        }

        public static BitmapImage ConvertToBitmapImage(this WriteableBitmap wb, int width, int height)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                wb.SaveJpeg(ms, width, height, 0, 100);
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(ms);
                return bmp;
            }
        }

        public static byte[] ConvertToBytes(this BitmapImage bitmapImage)
        {
            WriteableBitmap btmMap = new WriteableBitmap(bitmapImage);

            return btmMap.ConvertToBytes();
        }

        public static byte[] ConvertToBytes(this WriteableBitmap btmMap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // write an image into the stream
                btmMap.SaveJpeg(ms, btmMap.PixelWidth, btmMap.PixelHeight, 0, 100);
                //ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        public static void RunAfter(Action action, TimeSpan span)
        {
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer { Interval = span };
            dispatcherTimer.Tick += (sender, args) =>
            {
                var timer = sender as System.Windows.Threading.DispatcherTimer;
                if (timer != null)
                {
                    timer.Stop();
                }

                action();
            };
            dispatcherTimer.Start();
        }

        public static BitmapImage ToBitmapImage(this byte[] imageAsByteArray, string DefaultImageUri = null)
        {
            BitmapImage result = null;

            //If DefaultImageUri was informed and imageAsByteArray is not a valid byte array, load and return the image at DefaultImageUri
            try
            {
                if (!String.IsNullOrEmpty(DefaultImageUri) && imageAsByteArray == null)
                {
                    result = new BitmapImage(new Uri(DefaultImageUri, UriKind.Relative));
                }
                else
                {
                    result = new BitmapImage();

                    using (MemoryStream stream = new MemoryStream(imageAsByteArray))
                    {
                        result.SetSource(stream);
                    }
                }
             }
                catch(Exception ex) {
                    Tool.WriteExceptionMessagesToOutputBox(ex);
                 result = null; }

            return result;
        }

        public static void RunOnUiThread(this Action task)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(task);
        }
    }
}
