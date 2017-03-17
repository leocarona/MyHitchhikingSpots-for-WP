using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MyHitchhikingSpots.ViewModels;
using MyHitchhikingSpots.Resources;

namespace MyHitchhikingSpots.Views
{
    public partial class MapEditPage : PhoneApplicationPage
    {
        MapViewModel vm;

        public MapEditPage()
        {
            InitializeComponent();

            DataContext = vm = MyHitchhikingSpots.Tools.ServiceContainer.Resolve<MapViewModel>();

            this.Loaded += MapEditPage_Loaded;

            BuildLocalizedApplicationBar();
        }

        void MapEditPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedMap != null)
            {
                txtName.Text = vm.SelectedMap.Name;
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            var map = vm.SelectedMap;

            if (map == null)
                map = new MyHitchhikingSpots.Models.Map();

            map.Name = txtName.Text;


            var msg = "";

            try
            {
                if (map.Id > 0)
                {
                    msg = AppResources.MapEditFormCreateSucceededMessage;
                    vm.UpdateMap(map);
                }
                else
                {
                    msg = AppResources.MapEditFormUpdateSucceededMessage;
                    vm.SaveMap(map);
                }
            }
            catch
            {
                msg = AppResources.MapEditFormSaveFaultedMessage;
            }

            MessageBox.Show(msg);
            NavigationService.Navigate(new Uri("/Views/MapsPage.xaml", UriKind.Relative));
        }

             // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton saveButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/save.png", UriKind.Relative));
            saveButton.Text = AppResources.MapEditFormSaveButtonText;
            saveButton.Click += Save_Click;
            ApplicationBar.Buttons.Add(saveButton);
        }
    }
}