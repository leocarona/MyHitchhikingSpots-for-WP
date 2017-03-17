using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Device.Location;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using MyHitchhikingSpots.Interfaces;
using MyHitchhikingSpots.Tools;
using MyHitchhikingSpots.ViewModels;
//using MyHitchhikingSpots.Tools;
using MyHitchhikingSpots.Controls;
using MyHitchhikingSpots.Resources;
using Microsoft.Phone.Tasks;
using Windows.Storage;
using Windows.Storage;
using Windows.Storage.Streams;
using System;
using System.IO;
using Microsoft.Live;
using System.IO.IsolatedStorage;
namespace MyHitchhikingSpots.Views
{
    public partial class MapPage : PhoneApplicationPage
    {
        MapViewModel vm;
        LocationViewModel locViewModel;

        public MapPage()
        {
            InitializeComponent();

            DataContext = vm = ServiceContainer.Resolve<MapViewModel>();
            locViewModel = ServiceContainer.Resolve<LocationViewModel>();

            locViewModel.PropertyChanged += locViewModel_PropertyChanged;

            // Instantiate MapLayers - We use three map layers: one for the user location, one for the items and one for the balloon that opens when an item is clicked.
            UserCurrentLocationLayer = new MapLayer();
            MapItemsLayer = new MapLayer();
            MapPopupLayer = new MapLayer();

            // Add the MapLayer to the Map.
            sampleMap.Layers.Add(MapItemsLayer);
            sampleMap.Layers.Add(UserCurrentLocationLayer);
            sampleMap.Layers.Add(MapPopupLayer);
        }

        void locViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "City")
                txtLocation.Text = GetLocation();
        }

        private string GetLocation()
        {
            var location = locViewModel.City;
            if (!string.IsNullOrEmpty(locViewModel.Country))
                location += " - " + locViewModel.Country;
            return location;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapId">-1 to read the last map created, 0 to read all the maps or a valid map Id.</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task LoadMapPoints(int mapId = -1)
        {

            await vm.LoadMap(mapId);

            if (vm.SelectedMapItem.GetDataState() == SpotDataState.BasicInfoInserted)
                btnGotARide.IsEnabled = true;
            else
                btnGotARide.IsEnabled = false;

            OnCurrentPositionChanged(null, null);

            if (vm.MapItemList.Count() > 0)
                btnGenerateUrl.Visibility = Visibility.Visible;
            else
                btnGenerateUrl.Visibility = Visibility.Collapsed;

            if (locViewModel.CurrentPosition != null)
            {
                ShowCurrentLocation(locViewModel.CurrentPosition.ConvertToGeoCoordinate(), "/Assets/map-marker-you.png");
                CenterMapOnLocation();
            }

            vm.IsBusy = false;
        }

        public void OnCurrentPositionChanged(object sender, EventArgs e)
        {
            WP8Tool.RunOnUiThread(() =>
            {
                if (locViewModel.CurrentPosition != null)
                {
                    UserCurrentLocationLayer.Clear();
                    ShowCurrentLocation(locViewModel.CurrentPosition.ConvertToGeoCoordinate(), "/Assets/map-marker-you.png");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Current position was not fetched.");
                }


                if (locViewModel.CurrentPosition == null || btnGotARide.IsEnabled)
                    btnRecord.IsEnabled = false;
                else
                    btnRecord.IsEnabled = true;
            });
        }

        public void OnCurrentPositionChangedForFirstTime(object sender, EventArgs e)
        {
            WP8Tool.RunOnUiThread(() =>
            {
                //Only execute the bellow lines after we get a first position that is not null.
                if (locViewModel.CurrentPosition != null)
                {
                    locViewModel.OnCurrentPositionChanged -= OnCurrentPositionChangedForFirstTime;
                    ToggleLandmarks();
                    CenterMapOnLocation();

                    vm.IsBusy = false;
                }
            });
        }

        string GetQueryString(string paramKey)
        {
            if (NavigationContext.QueryString.ContainsKey(paramKey))
                return NavigationContext.QueryString[paramKey];
            else
                return String.Empty;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (locViewModel.locator == null)
                locViewModel.SetGeolocator(new Xamarin.Geolocation.Geolocator());
            locViewModel.OnCurrentPositionChanged += OnCurrentPositionChanged;
            locViewModel.OnCurrentPositionChanged += OnCurrentPositionChangedForFirstTime;


            if (e.NavigationMode == NavigationMode.New)
            {
                vm.IsBusy = true;

                var mapId = GetQueryString("mapId");

                int id = -1;
                if (!String.IsNullOrEmpty(mapId))
                    int.TryParse(mapId, out id);

               await LoadMapPoints(id);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            locViewModel.OnCurrentPositionChanged -= OnCurrentPositionChanged;
            locViewModel.OnCurrentPositionChanged -= OnCurrentPositionChangedForFirstTime;
        }

        private void ClearMapItemsLayer()
        {
            MapItemsLayer.Clear();
            //sampleMap.Layers.Remove(MapItemsLayer);
            //MapItemsLayer = null;
        }

        private void ClearUserCurrentLocationLayer()
        {
            UserCurrentLocationLayer.Clear();
            //sampleMap.Layers.Remove(UserCurrentLocationLayer);
            //UserCurrentLocationLayer = null;
        }

        private void ClearMapPopupLayer()
        {
            MapPopupLayer.Clear();
            //sampleMap.Layers.Remove(MapPopupLayer);
            //MapPopupLayer = null;
        }


        private void ShowMapPopupLayer()
        {
            // Add the MapLayer to the Map.
            ToggleLandmarks();
            CenterMapOnLocation();
        }

        void ToggleLandmarks()
        {
            sampleMap.LandmarksEnabled = true;
            if (sampleMap.ZoomLevel < vm.DefaultMapZoomLevel)
            {
                sampleMap.ZoomLevel = vm.DefaultMapZoomLevel;
            }
        }




        private void CenterMapOnLocation()
        {
            if (locViewModel.CurrentPosition != null)
                sampleMap.Center = locViewModel.CurrentPosition.ConvertToGeoCoordinate();
        }

        MapLayer MapItemsLayer = null;
        MapLayer UserCurrentLocationLayer = null;
        MapLayer MapPopupLayer = null;

        private void ShowLocation(Models.MapItem mapItem, string pinImageUri)
        {
            if (mapItem == null)
                return;

            var coordinate = mapItem.ConvertToGeoCoordinate();

            var pointer = GetPinImage(pinImageUri);
            pointer.Tag = mapItem;

            // Create a MapOverlay to contain the circle.
            var myLocationOverlay = GetMapOverlay(coordinate, pointer);

            ShowLocation(myLocationOverlay);
        }

        private void ShowLocation(Models.LocationHolder loc, string pinImageUri)
        {
            if (loc == null)
                return;

            var coordinate = loc.ConvertToGeoCoordinate();

            var pointer = GetPinImage(pinImageUri);
            pointer.Tag = loc;

            // Create a MapOverlay to contain the circle.
            var myLocationOverlay = GetMapOverlay(coordinate, pointer);

            ShowLocation(myLocationOverlay);
        }

        private void ShowLocation(GeoCoordinate coordinate, string pinImageUri)
        {
            if (coordinate == null)
                return;

            var image = GetPinImage(pinImageUri);
            image.Tag = coordinate;

            // Create a MapOverlay to contain the circle.
            var myLocationOverlay = GetMapOverlay(image.Tag as GeoCoordinate, image);

            ShowLocation(myLocationOverlay);
        }

        private void ShowLocation(MapOverlay locationOverlay)
        {
            MapItemsLayer.Add(locationOverlay);
        }

        private void ShowCurrentLocation(MapOverlay locationOverlay)
        {
            UserCurrentLocationLayer.Add(locationOverlay);
        }

        private void ShowCurrentLocation(GeoCoordinate coordinate, string pinImageUri)
        {
            if (coordinate == null)
                return;

            var image = GetPinImage(pinImageUri);
            image.Tag = coordinate;

            // Create a MapOverlay to contain the circle.
            var myLocationOverlay = GetMapOverlay(image.Tag as GeoCoordinate, image);

            ShowCurrentLocation(myLocationOverlay);
        }

        private static MapOverlay GetMapOverlay(GeoCoordinate coordinate, object content)
        {
            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = content;
            myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = coordinate;
            return myLocationOverlay;
        }

        private Image GetPinImage(string pinImageUri)
        {
            var image = new Image();
            image.Width = 30;
            image.Height = 30;
            //Because we set the Height to 30 and the middle of the image is always positioned at the spot, we need to pull it up.
            image.Margin = new Thickness(0, -30, 0, 0);
            image.Opacity = 50;
            image.Source = new BitmapImage(new Uri(pinImageUri, UriKind.RelativeOrAbsolute));
            image.Tap += image_Tap;

            return image;
        }

        void image_Tap(object sender, object e)
        {
            var pin = sender as FrameworkElement;
            var mapItem = pin.Tag as Models.MapItem;

            if (mapItem != null)
                OpenBalloon(mapItem, pin);
        }

        private void OpenBalloon(Models.MapItem mapItem, FrameworkElement pin = null)
        {
            WP8Tool.RunOnUiThread(() =>
           {  //This method needs to be refactored

               GeoCoordinate coordinate = null;

               if (mapItem != null)
                   coordinate = mapItem.ConvertToGeoCoordinate();
               else if (pin.Tag is GeoCoordinate)
                   coordinate = pin.Tag as GeoCoordinate;

               //If the coordinate is null or the user has tapped the item corresponding to the open popup, don`t do nothing.
               if (coordinate == null || (MapPopupLayer.Count > 0 && coordinate == MapPopupLayer[0].GeoCoordinate))
                   return;
               try
               {
                   //if (MapPopupLayer != null)
                   //{
                   //    //sampleMap.Layers.Remove(MapPopupLayer);
                   //    MapPopupLayer = null;
                   //}

                   var popup = new MapPopup();

                   if (mapItem != null)
                   {
                       popup.DataContext = mapItem;
                       popup.OnEditClick = m => OpenEditPage(m.Id);
                       //new
                       //{
                       //    Title = mapItem.Name,
                       //    AddressFormatted = String.Format(
                       //                                "{0}{1}{2}",
                       //                                mapItem.Street != null ? mapItem.Street + ", " : "",
                       //                                mapItem.State != null ? mapItem.State + ", " : "",
                       //                                mapItem.City != null ? mapItem.City + ", " : ""
                       //                                ).TrimEnd(' ', ',')
                       //};
                   }
                   else
                       if (locViewModel.CurrentPosition.ConvertToGeoCoordinate() == coordinate)
                       {
                           popup.DataContext = new
                           {
                               IsCurrentLocation = true,
                               Title = AppResources.CurrentLocationMapBalloonTitle,
                               //AddressFormatted = String.Format(
                               //                            "{0}{1}",
                               //                            vm.City != null ? vm.City + ", " : "",
                               //                            vm.Country != null ? vm.Country + ", " : ""
                               //                            ).TrimEnd(' ', ',')
                           };
                       }

                   popup.OnCloseAllPopupsClickHandler = ClearMapPopupLayer;

                   // Create a MapOverlay to contain the circle.
                   var locationOverlay = GetMapOverlay(coordinate, popup);

                   // Create a MapLayer to contain the MapOverlay.
                   //if (MapPopupLayer == null)
                   //    MapPopupLayer = new MapLayer();

                   MapPopupLayer.Clear();
                   MapPopupLayer.Add(locationOverlay);
               }
               catch (Exception ex)
               {
                   //NOTE on when an exception is thrown here:
                   //If you open the app and go out of the app (by clicking on the button to go to the device start page) before the loading is completed, 
                   //then you go back to the app (by using the device's fisical back button) and the data will load again. When it does so, 
                   //you click on any icon and it will hit this catch exception here, without opening any balloon.
                   //I didn't have time to go through it yet.

                   Tool.WriteExceptionMessagesToOutputBox(ex);
               }

               //ToggleLandmarks();
               sampleMap.Center = coordinate;
           });
        }


        private void EditRecord_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((System.Windows.Controls.Control)(e.OriginalSource)).Tag.ToString();
            int mapItemId = 0;

            if (Int32.TryParse(tag, out mapItemId))
            {
                OpenEditPage(mapItemId);
                //WP8Tool.ShowRouteTo(new System.Device.Location.GeoCoordinate(item.Latitude, item.Longitude));
            }
        }

        private void OpenEditPage(int mapItemId)
        {
            vm.SelectMapItem(mapItemId);
            NavigationService.Navigate(new Uri("/Views/MapItemEdit.xaml", UriKind.Relative));
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mapItemId = ((System.Windows.Controls.Control)(e.OriginalSource)).Tag.ToString();
                var item = vm.MapItemList.Where(p => p.Id == Convert.ToInt32(mapItemId)).SingleOrDefault();

                var confirm = MessageBox.Show(AppResources.DeleteSpotConfirmationMessage, AppResources.DeleteSpotConfirmationTitle, MessageBoxButton.OKCancel);

                if (confirm == MessageBoxResult.OK)
                {
                    await vm.DeleteSpot(item);

                    MapPopupLayer.Remove(GetMapOverlay(item.ConvertToGeoCoordinate(), null));
                }
            }
            catch { }
        }



        private void SeeOnMap_Click(object sender, RoutedEventArgs e)
        {
            var mapItemId = ((System.Windows.Controls.Control)(e.OriginalSource)).Tag.ToString();
            var item = vm.MapItemList.Where(p => p.Id == Convert.ToInt32(mapItemId)).SingleOrDefault();

            ShowOnMap(item);
        }

        private void ShowOnMap(Models.MapItem item)
        {
            var coord = item.ConvertToGeoCoordinate();

            myPivot.SelectedIndex = 0;

            sampleMap.ZoomLevel = 13;
            sampleMap.Center = coord;
            OpenBalloon(item);
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            vm.UnselectMapItem();
            NavigationService.Navigate(new Uri("/Views/MapItemEdit.xaml", UriKind.Relative));
        }

        private void GotARide_Click(object sender, RoutedEventArgs e)
        {
            var waitingTime = vm.GetWaitingTime();
            NavigationService.Navigate(new Uri("/Views/MapItemEdit.xaml?gotARide=" + waitingTime.ToString(), UriKind.Relative));
        }

        private void GenerateUrl_Click(object sender, RoutedEventArgs e)
        {
            bool success = false;
            try
            {
                var mapUrl = vm.GetRouteUrl();

                if (!String.IsNullOrEmpty(mapUrl))
                {
                    Clipboard.SetText(mapUrl);
                    MessageBox.Show(AppResources.RouteMapUrlCopiedMessage);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }

            if (!success)
                MessageBox.Show("Something went wrong and it was not possible to copy the route url to your clipboard.");

        }

        bool _bSignin;
        LiveConnectSession LiveSession;

        private void CloseShowOnMapPopupClicked(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (ShowOnMapPopup.IsOpen) { ShowOnMapPopup.IsOpen = false; }
        }


        /// <summary> 
        /// Change capture of session 
        /// </summary> 
        /// <param name="sender"></param> 
        /// <param name="e"></param> 
        private async void buttonSignin_SessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            vm.IsBusy = true;

            if (e.Session != null && e.Status == LiveConnectSessionStatus.Connected)
            {
                // Determined to display a panel of waiting for because the session is finalized 

                LiveSession = e.Session;

                _bSignin = true;

                UploadButton.Visibility = Visibility.Visible;
            }
            else
            {
                _bSignin = false;
                UploadButton.Visibility = Visibility.Collapsed;
            }

            vm.IsBusy = false;
        }

        string _fileName;

        private void SignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //vm.IsBusy = true;
        }

        private void ClosePopupClicked(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
        }

        // Handles the Click event on the Button on the page and opens the Popup. 
        private void ShowPopupOffsetClicked(object sender, RoutedEventArgs e)
        {
            // open the Popup if it isn't open already 
            CloseShowOnMapPopupClicked(null, null);
            if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }
        }


        private async void ExportDB_Click(object sender, RoutedEventArgs e)
        {
            if (vm.IsBusy)
                return;
            else
                vm.IsBusy = true;

            var msgResult = "Something went wrong and the data couldn't be exported.";
            try
            {
                _fileName = vm.GetDBFileName();
                var fileNameFormat = DateTime.Now.ToString("yyyy_MM_dd_HH-mm-{0}");
                vm.CloseConnection();
                locViewModel.CloseConnection();

                // WaitPanel.Visibility = System.Windows.Visibility.Visible;

                /// Connect to MS Live 
                LiveConnectClient client = new LiveConnectClient(LiveSession); 

                var isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(_fileName, FileMode.Open, isoStore))
                {
                    using (StreamReader streamData = new StreamReader(isoStream))
                    {
                        // Check for the presence of files 
                        if (streamData != null)
                        {                          
                            if (isoStore.FileExists(_fileName))
                            {
                                var destinationFileName =string.Format(fileNameFormat, _fileName);
                                var upResult = await client.UploadAsync("/me/skydrive/", destinationFileName, streamData.BaseStream, OverwriteOption.Rename);
                                msgResult = "Data saved to your OneDrive folder successfully. Generated file: SQLite database named\n'" + destinationFileName + "'";
                            }
                        }
                    }
                }

                var locationsDB = locViewModel.GetDBFileName();
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(locationsDB, FileMode.Open, isoStore))
                {
                    using (StreamReader streamData = new StreamReader(isoStream))
                    {
                        // Check for the presence of files 
                        if (streamData != null)
                        {
                            if (isoStore.FileExists(locationsDB))
                            {
                                var destinationFileName = string.Format(fileNameFormat, locationsDB);
                                var upResult = await client.UploadAsync("/me/skydrive/", destinationFileName, streamData.BaseStream, OverwriteOption.Rename);
                                msgResult += "\n'" + destinationFileName + "'";
                            }
                        }
                    }
                }



                // WaitPanel.Visibility = System.Windows.Visibility.Collapsed; 
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
                msgResult += "\n" + ex.Message;
            }

            MessageBox.Show(msgResult);

            vm.OpenNewConnection();
            locViewModel.OpenNewConnection();

            vm.IsBusy = false;

            return;

            //CODE BELLOW IS FOR WRITING THE FILE TO THE SDCARD, BUT SDCARDS BECAME AVAILABLE ONLY FROM WINDOWS PHONE 8.1

            // Get the logical root folder for all external storage devices.
            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;

            // Get the first child folder, which represents the SD card.
            StorageFolder sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();

            if (sdCard != null)
            {
                // An SD card is present and the sdCard variable now contains a reference to it.
                var f = System.IO.File.OpenRead(_fileName);
                var buffer = f.AsInputStream();

                StorageFile file = await sdCard.CreateFileAsync("", CreationCollisionOption.GenerateUniqueName);

                using (var photoOutputStream = await file.OpenStreamForWriteAsync())
                {
                    await buffer.AsStreamForRead().CopyToAsync(photoOutputStream);
                }
            }
            else
            {
                // No SD card is present.
            }

        }

        /// <summary> 
        /// Returns a stream by loading a file 
        /// </summary> 
        /// <param name="fileName"></param> 
        /// <returns></returns> 
        public async Task<System.IO.Stream> LoadStorageIOStream(string fileName)
        {
            try
            {
                // file open option call 
                var file = await CreateFileAsyncStorageFile(fileName, CreationCollisionOption.OpenIfExists);

                IRandomAccessStream accessStream = await file.OpenReadAsync();

                Stream stream = accessStream.AsStreamForRead((int)accessStream.Size);

                return stream;

            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<StorageFile> CreateFileAsyncStorageFile(string fileName, CreationCollisionOption createOption)
        {
            IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;

            var dir = Path.GetDirectoryName(fileName);
            StorageFile file;
            if (dir != "")
            {
                // folder is open only 
                var folder = await applicationFolder.CreateFolderAsync(Path.GetDirectoryName(fileName), CreationCollisionOption.OpenIfExists);

                file = await folder.CreateFileAsync(Path.GetFileName(fileName), createOption);
            }
            else
            {
                file = await applicationFolder.CreateFileAsync(Path.GetFileName(fileName), createOption);
            }

            return file;
        }

        /// <summary> 
        /// Check whether or not a file has been uploaded 
        /// </summary> 
        /// <returns></returns> 
        private async Task<bool> GetDlFileStatus()
        {
            bool _bIsFile = false;

            // Connect to MS Live 
            LiveConnectClient client = new LiveConnectClient(LiveSession);

            // I want to get the root of OneDrive 
            LiveOperationResult operationResult = await client.GetAsync("me/skydrive/files");

            dynamic dyResult = ((dynamic)operationResult.Result).data;

            foreach (var item in dyResult)
            {
                // It is a path to get the ID of the OneDrive on by checking the existence of files 
                if (((dynamic)item).name == _fileName)
                {
                    _bIsFile = true;
                    //dlsize.Text = ((dynamic)item).size.ToString();
                    break;
                }
            }

            return _bIsFile;

        }

        string showOnMapButtonContent = "";

        private void ViewOnMap_Click(object sender, RoutedEventArgs e)
        {
            // open the Popup if it isn't open already 
            ClosePopupClicked(null, null);
            if (!ShowOnMapPopup.IsOpen) { ShowOnMapPopup.IsOpen = true; }
        }

        void ShowMySavedSpots_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MapItemsLayer.Count > 0)
                    ClearMapItemsLayer();

                foreach (var p in vm.MapItemList)
                    ShowLocation(p, "/Assets/map-marker.png");
                myPivot.SelectedIndex = 0;
                ToggleLandmarks();

                ClearMapButton.Visibility = Visibility.Visible;
                if (ShowOnMapPopup.IsOpen) { ShowOnMapPopup.IsOpen = false; }

            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
        }

        async void ShowWhereHaveIBeen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MapItemsLayer.Count > 0)
                    ClearMapItemsLayer();

                var lst = await locViewModel.LoadSavedLocations();

                foreach (var p in lst)
                    ShowLocation(p, "/Assets/map-marker-you.png");
                myPivot.SelectedIndex = 0;
                ToggleLandmarks();

                ClearMapButton.Visibility = Visibility.Visible;
                if (ShowOnMapPopup.IsOpen) { ShowOnMapPopup.IsOpen = false; }

            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
        }

        void ClearMap_Click(object sender, RoutedEventArgs e)
        {
            ClearMapItemsLayer();
            ClearMapButton.Visibility = Visibility.Collapsed;
            CloseShowOnMapPopupClicked(null, null);

            myPivot.SelectedIndex = 0;
            ToggleLandmarks();
        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Because when we set SelectedItem to null the SelectionChanged event handler is called again, we need to make this verification
            if (e.AddedItems != null && e.AddedItems.Count > 0 && ((Microsoft.Phone.Controls.LongListSelector)(sender)).SelectedItem != null)
            {
                var item = e.AddedItems[0] as Models.MapItem;

                if (item != null)
                {
                    ((Microsoft.Phone.Controls.LongListSelector)(sender)).SelectedItem = null;

                    ShowOnMap(item);
                }
            }
        }



        private void OpenAllMaps_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // NavigationService.Navigate(new Uri("/Views/MapsPage.xaml", UriKind.Relative));
        }
    }
}