using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MyHitchhikingSpots.Tools;
using MyHitchhikingSpots.ViewModels;
using MyHitchhikingSpots.Resources;

namespace MyHitchhikingSpots.Views
{
    public partial class MapsPage : PhoneApplicationPage
    {
        MapViewModel vm;

        public MapsPage()
        {
            InitializeComponent();

            DataContext = vm = ServiceContainer.Resolve<MapViewModel>();

            BuildLocalizedApplicationBar();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                await vm.LoadAllMaps();
            }
        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Because when we set SelectedItem to null the SelectionChanged event handler is called again, we need to make this verification
            if (e.AddedItems != null && e.AddedItems.Count > 0 && ((Microsoft.Phone.Controls.LongListSelector)(sender)).SelectedItem != null)
            {
                var item = e.AddedItems[0] as Models.Map;

                if (item != null)
                {
                    ((Microsoft.Phone.Controls.LongListSelector)(sender)).SelectedItem = null;

                    NavigationService.Navigate(new Uri("/Views/MapPage.xaml?mapId=" + item.Id.ToString(), UriKind.Relative));
                }
            }
        }

        private void CreateNew_Click(object sender, EventArgs e)
        {
            vm.UnselectMap();
            NavigationService.Navigate(new Uri("/Views/MapEditPage.xaml", UriKind.Relative));
        }

        private void UnselectMap_Click(object sender, EventArgs e)
        {
            vm.UnselectMap();
            //mapId=0 on the URL will load all the maps
            NavigationService.Navigate(new Uri("/Views/MapPage.xaml?mapId=0", UriKind.Relative));
        }

        private void EditRecord_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((System.Windows.Controls.Control)(e.OriginalSource)).Tag.ToString();
            int mapId = 0;

            if (Int32.TryParse(tag, out mapId))
            {
                vm.SelectMap(mapId);
                NavigationService.Navigate(new Uri("/Views/MapEditPage.xaml", UriKind.Relative));
                //WP8Tool.ShowRouteTo(new System.Device.Location.GeoCoordinate(item.Latitude, item.Longitude));
            }
        }

        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton createNewButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/add.png", UriKind.Relative));
            createNewButton.Text = AppResources.MapsViewCreateNewButtonText;
            createNewButton.Click += CreateNew_Click;
            ApplicationBar.Buttons.Add(createNewButton);

            ApplicationBarIconButton unselectMapButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/close.png", UriKind.Relative));
            unselectMapButton.Text = AppResources.MapsViewUnselectMapButtonText;
            unselectMapButton.Click += UnselectMap_Click;
            ApplicationBar.Buttons.Add(unselectMapButton);
        }
    }
}