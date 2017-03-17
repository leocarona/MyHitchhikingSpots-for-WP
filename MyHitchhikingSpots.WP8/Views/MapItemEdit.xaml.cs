using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MyHitchhikingSpots.Interfaces;
using MyHitchhikingSpots.ViewModels;
using MyHitchhikingSpots.Tools;
using MyHitchhikingSpots.Resources;

namespace MyHitchhikingSpots.Views
{
    public partial class MapItemEdit : PhoneApplicationPage
    {
        MapViewModel vm;
        LocationViewModel locViewModel;

        public MapItemEdit()
        {
            InitializeComponent();

            DataContext = vm = ServiceContainer.Resolve<MapViewModel>();
            locViewModel = ServiceContainer.Resolve<LocationViewModel>();

            if (vm.SelectedMapItem == null && locViewModel.CurrentPosition != null)
            {
                vm.SelectedMapItem = new MyHitchhikingSpots.Models.MapItem
                {
                    Latitude = locViewModel.CurrentPosition.Latitude,
                    Longitude = locViewModel.CurrentPosition.Longitude,
                    City = locViewModel.City,
                    Country = locViewModel.Country
                };
            }

            if (vm.SelectedMapItem != null && !vm.SelectedMapItem.IsReverseGeocoded)
            {
                vm.CallFinished += vm_CallFinished;
                vm.ReverseGeocodeSelectedMapItem();
            }

            this.Loaded += MapItemEdit_Loaded;

            BuildLocalizedApplicationBar();
        }

        void vm_CallFinished(bool sucess)
        {
            vm.CallFinished -= vm_CallFinished;
            Tool.RunInUIThread(() => txtLocation.Text = GetLocation());
        }

        string GetQueryString(string paramKey)
        {
            if (NavigationContext.QueryString.ContainsKey(paramKey))
                return NavigationContext.QueryString[paramKey];
            else
                return String.Empty;
        }

        void MapItemEdit_Loaded(object sender, RoutedEventArgs e)
        {
            //Unsubscribe to the loaded method because when we were selecting an item from a 
            //FullMode ListPicker this method was been called again.
            this.Loaded -= MapItemEdit_Loaded;

            txtLocation.Text = GetLocation();

            switch (vm.SelectedMapItem.GetDataState())
            {
                case SpotDataState.Unknown:
                case SpotDataState.Clean:
                    {
                        txtTitle.Text = AppResources.MapItemEditCreateViewTitle;
                        txtNote.Text = "";

                        ShowBasicForm();
                        break;
                    }
                case SpotDataState.BasicInfoInserted:
                    {
                        txtTitle.Text = AppResources.MapItemEditEvaluateViewTitle;
                        txtWaitingTime.Text = vm.GetWaitingTime().ToString();

                        if (vm.SelectedMap != null && vm.Maps != null)
                            lstMaps.SelectedIndex = vm.Maps.IndexOf(m => m.Id == vm.SelectedMap.Id);

                        ShowEvaluateForm();
                        break;
                    }
                case SpotDataState.FullInfoInserted:
                    {
                        txtTitle.Text = AppResources.MapItemEditUpdateViewTitle;
                        txtNote.Text = vm.SelectedMapItem.Note;
                        lstHitchability.SelectedIndex = vm.HitchabilityOptions.IndexOf(i => i.Item1 == vm.SelectedMapItem.Hitchability);
                        dtpDate.Value = vm.SelectedMapItem.DateTime;
                        tmpTime.Value = vm.SelectedMapItem.DateTime;

                        txtWaitingTime.Text = vm.SelectedMapItem.WaitingTime.TotalMinutes.ToString();
                        lstAttemptResult.SelectedIndex = vm.AttemptResultOptions.IndexOf(i => i.Item1 == vm.SelectedMapItem.AttemptResult);

                        if (vm.SelectedMapItem.MapId > -1 && vm.Maps != null)
                            lstMaps.SelectedIndex = vm.Maps.IndexOf(m => m.Id == vm.SelectedMapItem.MapId);

                        ShowFullEditForm();
                        break;
                    }
            }
        }

        private string GetLocation()
        {
            var location = vm.SelectedMapItem.City;
            if (!string.IsNullOrEmpty(vm.SelectedMapItem.Country))
                location += " - " + vm.SelectedMapItem.Country;
            return location;
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
               await vm.LoadAllMaps();
            }
        }

        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton saveButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/save.png", UriKind.Relative));
            saveButton.Text = AppResources.MapItemEditFormSaveButtonText;
            saveButton.Click += btnSave_Click;
            ApplicationBar.Buttons.Add(saveButton);

            if (vm.SelectedMapItem.GetDataState() == SpotDataState.FullInfoInserted)
            {
                ApplicationBarIconButton deleteButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/delete.png", UriKind.Relative));
                deleteButton.Text = AppResources.MapItemEditFormDeleteButtonText;
                deleteButton.Click += Delete_Click;
                ApplicationBar.Buttons.Add(deleteButton);

                ApplicationBarIconButton downloadButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/download.png", UriKind.Relative));
                downloadButton.Text = AppResources.MapItemEditFormGetSpotNameButtonText;
                downloadButton.Click += ReverseGeocode_Click;
                ApplicationBar.Buttons.Add(downloadButton);
            }
        }

        void ShowBasicForm()
        {
            //((ApplicationBarIconButton)ApplicationBar.Buttons[ApplicationBar.Buttons.Count - 1]).IsEnabled = false;
            stpBasicForm.Visibility = System.Windows.Visibility.Visible;
            stpEvaluationForm.Visibility = System.Windows.Visibility.Collapsed;
        }

        void ShowEvaluateForm()
        {
            //((ApplicationBarIconButton)ApplicationBar.Buttons[ApplicationBar.Buttons.Count - 1]).IsEnabled = true;
            stpBasicForm.Visibility = System.Windows.Visibility.Collapsed;
            stpEvaluationForm.Visibility = System.Windows.Visibility.Visible;
        }

        void ShowFullEditForm()
        {
            stpBasicForm.Visibility = System.Windows.Visibility.Visible;
            stpEvaluationForm.Visibility = System.Windows.Visibility.Visible;
        }

        Hitchability? GetSelectedHitchability()
        {
            Hitchability? hitchability = null;
            if (lstHitchability.SelectedItem != null)
            {
                var hitchabilityTuple = lstHitchability.SelectedItem as Tuple<Hitchability, String>;
                if (hitchabilityTuple != null)
                    hitchability = hitchabilityTuple.Item1 as Hitchability?;
            }
            return hitchability;
        }

        AttemptResult? GetSelectedAttemptResult()
        {
            AttemptResult? attemptResult = null;
            if (lstAttemptResult.SelectedItem != null)
            {
                var attemptResultTuple = lstAttemptResult.SelectedItem as Tuple<AttemptResult, String>;
                if (attemptResultTuple != null)
                    attemptResult = attemptResultTuple.Item1 as AttemptResult?;
            }
            return attemptResult;
        }

        MyHitchhikingSpots.Models.Map GetSelectedMap()
        {
            MyHitchhikingSpots.Models.Map res = null;
            if (lstMaps.SelectedItem != null)
                res = lstMaps.SelectedItem as MyHitchhikingSpots.Models.Map;
            return res;
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                vm.IsBusy = true;
                var hitchability = GetSelectedHitchability();
                var attemptResult = GetSelectedAttemptResult();
                var mapItem = GetSelectedMap();

                var state = vm.SelectedMapItem.GetDataState();
                var mapItemId = -1;

                if (mapItem != null)
                    mapItemId = mapItem.Id;

                switch (state)
                {
                    case SpotDataState.Unknown:
                    case SpotDataState.Clean:
                        {
                            var errMsg = "";

                            if (!hitchability.HasValue)
                                errMsg = AppResources.MapItemEditHitchabilityMandatoryText;

                            if (!String.IsNullOrEmpty(errMsg))
                                MessageBox.Show(errMsg);
                            else
                            {
                                var date = dtpDate.Value.Value.Date;
                                date = date.Add(((DateTime)tmpTime.Value).TimeOfDay);

                                await vm.SaveSpot("", txtNote.Text, hitchability.Value, date)
                                    .ContinueWith(t =>
                                    {
                                        if (t.IsFaulted)
                                            Tool.WriteExceptionMessagesToOutputBox(t.Exception);
                                    });
                                vm.IsBusy = false;

                                //MessageBox.Show("Spot recorded successfully!");
                                NavigationService.Navigate(new Uri("/Views/MapPage.xaml", UriKind.Relative));
                            }
                            break;
                        }
                    case SpotDataState.BasicInfoInserted:
                        {
                            var errMsg = "";
                            int waitingTime = 0;

                            if (!int.TryParse(txtWaitingTime.Text, out waitingTime))
                                errMsg = AppResources.MapItemEditInvalidWaitingTimeText;
                            if (attemptResult == null || !attemptResult.HasValue)
                                errMsg = AppResources.MapItemEditAttemptResultMandatoryText;

                            if (!String.IsNullOrEmpty(errMsg))
                                MessageBox.Show(errMsg);
                            else
                            {
                                await vm.EvaluateSpot(vm.SelectedMapItem, waitingTime, attemptResult.Value, mapItemId)
                                      .ContinueWith(t =>
                                      {
                                          if (t.IsFaulted)
                                              Tool.WriteExceptionMessagesToOutputBox(t.Exception);
                                      });
                                vm.IsBusy = false;
                                MessageBox.Show(AppResources.MapItemEditEvaluateSaveSucceededText);
                                NavigationService.Navigate(new Uri("/Views/MapPage.xaml", UriKind.Relative));
                            }
                            break;
                        }
                    case SpotDataState.FullInfoInserted:
                        {
                            var errMsg = "";
                            int waitingTime = 0;

                            if (!hitchability.HasValue)
                                errMsg = AppResources.MapItemEditHitchabilityMandatoryText;
                            if (!int.TryParse(txtWaitingTime.Text, out waitingTime))
                                errMsg = AppResources.MapItemEditInvalidWaitingTimeText;
                            if (attemptResult == null || !attemptResult.HasValue)
                                errMsg = AppResources.MapItemEditAttemptResultMandatoryText;

                            if (!String.IsNullOrEmpty(errMsg))
                                MessageBox.Show(errMsg);
                            else
                            {
                                var date = dtpDate.Value.Value.Date;
                                date = date.Add(((DateTime)tmpTime.Value).TimeOfDay);

                                await vm.UpdateSpot(vm.SelectedMapItem, "", txtNote.Text, hitchability.Value, date, waitingTime, attemptResult.Value, mapItemId)
                                         .ContinueWith(t =>
                                         {
                                             if (t.IsFaulted)
                                                 Tool.WriteExceptionMessagesToOutputBox(t.Exception);
                                         });
                                vm.IsBusy = false;
                                MessageBox.Show(AppResources.MapItemEditUpdateSucceededText);
                                NavigationService.Navigate(new Uri("/Views/MapPage.xaml", UriKind.Relative));
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                vm.IsBusy = false;
                MessageBox.Show("Something unexpected happened when trying to save or update these information.\n" + ex.GetExceptionMessagesAsString());
            }
        }

        private void lstAttemptResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void Delete_Click(object sender, EventArgs e)
        {
            try
            {
                var confirm = MessageBox.Show(AppResources.DeleteSpotConfirmationMessage, AppResources.DeleteSpotConfirmationTitle, MessageBoxButton.OKCancel);

                if (confirm == MessageBoxResult.OK)
                {
                    vm.IsBusy = true;
                    await vm.DeleteSpot(vm.SelectedMapItem);
                    MessageBox.Show(AppResources.DeleteSpotSucceededMessage);
                    vm.IsBusy = false;
                    NavigationService.Navigate(new Uri("/Views/MapPage.xaml", UriKind.Relative));
                }
            }
            catch { }
        }

        private void ReverseGeocode_Click(object sender, EventArgs e)
        {
            bool shouldContinue = true;

            if (vm.SelectedMapItem.IsReverseGeocoded)
            {
                var confirm = MessageBox.Show(AppResources.GetSpotNameRedoConfirmation, "", MessageBoxButton.OKCancel);
                if (confirm == MessageBoxResult.Cancel)
                    shouldContinue = false;
            }

            if (shouldContinue)
            {
                vm.IsBusy = true;
                DisableSaveButton();
                vm.CallFinished += OnReverseGeocodeCompleted;
                vm.ReverseGeocodeSelectedMapItem();
            }
        }

        private void DisableSaveButton(){
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
        }

        private void EnableSaveButton()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
        }

        void OnReverseGeocodeCompleted(bool sucess)
        {
            vm.CallFinished -= OnReverseGeocodeCompleted;

            var msg = "";
            if (sucess)
                msg = String.Format(AppResources.GetSpotNameSucceededMessage, vm.SelectedMapItem.City, vm.SelectedMapItem.Country);
            else
                msg = AppResources.GetSpotNameFaultedMessage;

            txtLocation.Text = GetLocation();

            EnableSaveButton();
            vm.IsBusy = false;
            MessageBox.Show(msg);
        }

    }
}