//
//  Copyright 2012  Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using MyHitchhikingSpots.Tools;
using System;
using System.Windows;
using System.Windows.Controls;
using Windows.System;

namespace MyHitchhikingSpots.Controls
{
    public sealed partial class MapPopup : UserControl
    {
        public delegate void SeeDetailsButtonClick(object selectedObject);
        public SeeDetailsButtonClick SeeDetailsButtonClickHandler;

        public bool DisplayClosePopupButton = true;
        public delegate void closeAllPopups();
        public closeAllPopups OnCloseAllPopupsClickHandler { get; set; }

        public MapPopup()
        {
            this.InitializeComponent();
            this.Loaded += onMapLoaded;
        }

        public void onMapLoaded(object sender, RoutedEventArgs e)
        {
            if (OnCloseAllPopupsClickHandler == null || !DisplayClosePopupButton)
                ClosePopupButton.Visibility = Visibility.Collapsed;
        }



        private void OnDirectionClick(object sender, RoutedEventArgs e)
        {
            var item = DataContext as Models.MapItem;

            if (item != null)
            {
                WP8Tool.ShowRouteTo(new System.Device.Location.GeoCoordinate(item.Latitude, item.Longitude));
                //WP8Tool.Launch(new Uri(string.Format(MyHitchhikingSpots.Tools.Constants.BingMapsRouteLaunchUrl,
                //    item.Latitude, item.Longitude, item.Street, item.City, item.State, item.Zip)));
            }
            //else
            //    await Launcher.LaunchUriAsync(new Uri("bingmaps:"));
        }

        public delegate void EditClick(Models.MapItem m);
        public EditClick OnEditClick;

        private void EditRecord_Click(object sender, RoutedEventArgs e)
        {
            var item = DataContext as Models.MapItem;

            if (OnEditClick!=null)
            OnEditClick(item);            
        }

        private void OnSeeDetailsClick(object sender, RoutedEventArgs e)
        {
            //var customerAddress = DataContext as custtable;

            //if (SeeDetailsButtonClickHandler != null)
            //    SeeDetailsButtonClickHandler(customerAddress);
        }

        private void ClosePopupClickHandler(object sender, RoutedEventArgs e)
        {
            if (OnCloseAllPopupsClickHandler != null)
                OnCloseAllPopupsClickHandler();
        }
    }
}
