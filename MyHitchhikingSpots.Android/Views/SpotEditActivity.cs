using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MyHitchhikingSpots.ViewModels;
using MyHitchhikingSpots.Tools;
using MyHitchhikingSpots.Interfaces;
namespace MyHitchhikingSpots.Views
{
    [Activity(Label = "Spot", NoHistory = true)]//, MainLauncher = true
    public class SpotEditActivity : Activity
    {
        EditText txtNote, txtWaitingTime;
        Spinner lstHitchability, lstAttemptResult, lstMaps;

        Button btnSave, btnDelete, btnGetMoreInfo;
        MapViewModel vm;
        LocationViewModel locVM;
        LinearLayout boxBasicForm, boxEvaluationForm;
        ArrayAdapter<String> adapterHitchability, adapterAttemptResult, adapterMap;

        private TextView dateDisplay;
        private Button pickDate;
        private DateTime date;
        private Button pick_button;

        private int hour;
        private int minute;

        const int TIME_DIALOG_ID = 0;
        const int DATE_DIALOG_ID = 1;

        public SpotEditActivity()
        {
            Console.WriteLine("entrou SpotEditActivity");
            vm = ServiceContainer.Resolve<MapViewModel>();
            locVM = ServiceContainer.Resolve<LocationViewModel>();

            if (vm.SelectedMapItem == null && locVM.CurrentPosition != null)
            {
                vm.SelectedMapItem = new MyHitchhikingSpots.Models.MapItem
                {
                    Latitude = locVM.CurrentPosition.Latitude,
                    Longitude = locVM.CurrentPosition.Longitude
                };
            }

            if (vm.SelectedMapItem != null && !vm.SelectedMapItem.IsReverseGeocoded)
                vm.ReverseGeocodeSelectedMapItem();


            date = DateTime.Today;
            // Get the current time
            hour = DateTime.Now.Hour;
            minute = DateTime.Now.Minute;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SpotEdit);

            if (vm.HitchabilityOptions == null || vm.AttemptResultOptions == null)
                vm.SetOptions(GetHitchabilityOptions(), GetAttemptResultOptions());

            boxBasicForm = FindViewById<LinearLayout>(Resource.Id.boxBasicForm);
            boxEvaluationForm = FindViewById<LinearLayout>(Resource.Id.boxEvaluationForm);

            txtNote = FindViewById<EditText>(Resource.Id.txtNote);
            txtWaitingTime = FindViewById<EditText>(Resource.Id.txtWaitingTime);
            lstHitchability = FindViewById<Spinner>(Resource.Id.lstHitchability);
            lstAttemptResult = FindViewById<Spinner>(Resource.Id.lstAttemptResult);
            lstMaps = FindViewById<Spinner>(Resource.Id.lstMap);
            dateDisplay = FindViewById<TextView>(Resource.Id.dateDisplay);
            pickDate = FindViewById<Button>(Resource.Id.pickDate);
            pick_button = FindViewById<Button>(Resource.Id.pickTime);


            btnSave = FindViewById<Button>(Resource.Id.btnSave);
            btnDelete = FindViewById<Button>(Resource.Id.btnDelete);
            btnGetMoreInfo = FindViewById<Button>(Resource.Id.btnGetMoreInfo);

            pickDate.Click += delegate { ShowDialog(DATE_DIALOG_ID); };
            pick_button.Click += delegate { ShowDialog(TIME_DIALOG_ID); };
            btnSave.Click += btnSave_Click;
            btnDelete.Click += btnDelete_Click;
            btnGetMoreInfo.Click += btnGetMoreInfo_Click;

            adapterHitchability = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, vm.HitchabilityOptions.Select(i => i.Item2).ToList());
            adapterHitchability.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            lstHitchability.Adapter = adapterHitchability;

            adapterAttemptResult = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, vm.AttemptResultOptions.Select(i => i.Item2).ToList());
            adapterAttemptResult.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            lstAttemptResult.Adapter = adapterAttemptResult;



            if (vm.Maps == null || !vm.Maps.Any())
            {
                lstMaps.Visibility = ViewStates.Gone;
                FindViewById<TextView>(Resource.Id.lstMapTitle).Visibility = ViewStates.Gone;
            }
            else
            {
                adapterMap = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, vm.Maps.Select(i => i.Name).ToList());
                adapterMap.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                lstMaps.Adapter = adapterMap;
            }

            try
            {
                switch (vm.SelectedMapItem.GetDataState())
                {
                    case SpotDataState.Unknown:
                    case SpotDataState.Clean:
                    default:
                        {
                            //txtTitle.Text = AppResources.MapItemEditCreateViewTitle;
                            txtNote.Text = "";

                            ShowBasicForm();
                            break;
                        }
                    case SpotDataState.BasicInfoInserted:
                        {
                            //txtTitle.Text = AppResources.MapItemEditEvaluateViewTitle;

                            txtWaitingTime.Text = vm.GetWaitingTime().ToString();

                            if (vm.SelectedMap != null && vm.Maps != null)
                                lstMaps.SetSelection(vm.Maps.IndexOf(vm.SelectedMap));

                            ShowEvaluateForm();
                            break;
                        }
                    case SpotDataState.FullInfoInserted:
                        {
                            //txtTitle.Text = AppResources.MapItemEditUpdateViewTitle;
                            txtNote.Text = vm.SelectedMapItem.Note;
                            lstHitchability.SetSelection(vm.HitchabilityOptions.FindIndex(i => i.Item1 == vm.SelectedMapItem.Hitchability));
                            date = vm.SelectedMapItem.DateTime;
                            hour = vm.SelectedMapItem.DateTime.Hour;
                            minute = vm.SelectedMapItem.DateTime.Minute;

                            txtWaitingTime.Text = vm.SelectedMapItem.WaitingTime.TotalMinutes.ToString();
                            lstAttemptResult.SetSelection(vm.AttemptResultOptions.FindIndex(i => i.Item1 == vm.SelectedMapItem.AttemptResult));

                            if (vm.SelectedMapItem.MapId > -1 && vm.Maps != null)
                                lstMaps.SetSelection(vm.Maps.FindIndex(m => m.Id == vm.SelectedMapItem.MapId));

                            ShowFullEditForm();
                            break;
                        }
                }

                // display the current date (this method is below)
                UpdateDisplay();
            }
            catch
            {

            }
        }

        List<Tuple<AttemptResult, String>> GetAttemptResultOptions()
        {
            var options = new List<Tuple<AttemptResult, String>>();

            options.Add(new Tuple<AttemptResult, String>(AttemptResult.Successful, Resources.GetString(Resource.String.AttemptResultGotARideOption)));
            options.Add(new Tuple<AttemptResult, String>(AttemptResult.Gaveup, Resources.GetString(Resource.String.AttemptResultGaveUpOption)));

            return options;
        }

        List<Tuple<Hitchability, String>> GetHitchabilityOptions()
        {
            var options = new List<Tuple<Hitchability, String>>();

            options.Add(new Tuple<Hitchability, String>(Hitchability.NoAnswer, Resources.GetString(Resource.String.HitchabilityNoAnswerOption)));
            options.Add(new Tuple<Hitchability, String>(Hitchability.VeryGood, Resources.GetString(Resource.String.HitchabilityVeryGoodOption)));
            options.Add(new Tuple<Hitchability, String>(Hitchability.Good, Resources.GetString(Resource.String.HitchabilityGoodOption)));
            options.Add(new Tuple<Hitchability, String>(Hitchability.Average, Resources.GetString(Resource.String.HitchabilityAverageOption)));
            options.Add(new Tuple<Hitchability, String>(Hitchability.Bad, Resources.GetString(Resource.String.HitchabilityBadOption)));
            options.Add(new Tuple<Hitchability, String>(Hitchability.Senseless, Resources.GetString(Resource.String.HitchabilitySenselessOption)));

            return options;
        }


        DateTime FullDateTime
        {
            get
            {
                if (date == null || hour < 0 || minute < 0)
                    return DateTime.MinValue;
                else
                    return date.Date.AddHours(hour).AddMinutes(minute);
            }
        }

        // updates the date in the TextView
        private void UpdateDisplay()
        {
            //Show the date and time showing the date on the device's culture format and the time without showing the seconds
            dateDisplay.Text = FullDateTime.ToShortDateString() + " " + FullDateTime.ToShortTimeString();
        }
        // the event received when the user "sets" the date in the dialog
        void DatePickerCallback(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            this.date = e.Date;
            UpdateDisplay();
        }
        private void TimePickerCallback(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            hour = e.HourOfDay;
            minute = e.Minute;
            UpdateDisplay();
        }
        protected override Android.App.Dialog OnCreateDialog(int id)
        {
            switch (id)
            {
                case DATE_DIALOG_ID:
                    return new DatePickerDialog(this, DatePickerCallback, date.Year, date.Month - 1, date.Day);
                    break;
                case TIME_DIALOG_ID:
                    return new TimePickerDialog(this, TimePickerCallback, hour, minute, false);
                    break;
            }
            return null;
        }

        void ShowBasicForm()
        {
            btnDelete.Visibility = ViewStates.Gone;
            btnGetMoreInfo.Visibility = ViewStates.Gone;
            boxBasicForm.Visibility = ViewStates.Visible;
            boxEvaluationForm.Visibility = ViewStates.Gone;
        }

        void ShowEvaluateForm()
        {
            btnDelete.Visibility = ViewStates.Gone;
            btnGetMoreInfo.Visibility = ViewStates.Gone;
            boxBasicForm.Visibility = ViewStates.Gone;
            boxEvaluationForm.Visibility = ViewStates.Visible;
        }

        void ShowFullEditForm()
        {
            btnDelete.Visibility = ViewStates.Visible;
            btnGetMoreInfo.Visibility = ViewStates.Visible;
            boxBasicForm.Visibility = ViewStates.Visible;
            boxEvaluationForm.Visibility = ViewStates.Visible;
        }


        void btnGetMoreInfo_Click(object sender, EventArgs e)
        {
            //Reverse geocode
            DisableSaveButton();
            vm.CallFinished += OnReverseGeocodeCompleted;
            vm.ReverseGeocodeSelectedMapItem();
        }

        ProgressDialog _busyDialog;

        private void DisableSaveButton()
        {
            if (_busyDialog == null)
            {
                _busyDialog = new ProgressDialog(this);
                _busyDialog.SetTitle("Please wait");
                _busyDialog.SetMessage("Getting more info from Google Maps.");
            }

            _busyDialog.Show();
        }

        private void EnableSaveButton()
        {
            if (_busyDialog != null)
                _busyDialog.Cancel();
        }

        void OnReverseGeocodeCompleted(bool sucess)
        {
            RunOnUiThread(() =>
            {
                vm.CallFinished -= OnReverseGeocodeCompleted;

                string msg = "";
                if (sucess)
                {
                    msg = Resources.GetString(Resource.String.GetSpotNameSucceededMessage);
                    msg = String.Format(msg, vm.SelectedMapItem.City, vm.SelectedMapItem.Country);
                }
                else
                {
                    msg = Resources.GetString(Resource.String.GetSpotNameFaultedMessage);
                }

                EnableSaveButton();

                var dialog = new AlertDialog.Builder(this);
                //dialog.SetTitle("Information on this spot");
                dialog.SetMessage(msg);
                dialog.SetPositiveButton("Ok", delegate { });
                dialog.Show();
            });
        }

        void btnDelete_Click(object sender, EventArgs e)
        {
            var dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(Resources.GetString(Resource.String.DeleteSpotConfirmationTitle));
            dialog.SetMessage(Resources.GetString(Resource.String.DeleteSpotConfirmationMessage));
            dialog.SetPositiveButton("Yes", delegate
            {
                vm.DeleteSpot(vm.SelectedMapItem).Wait();
                Toast.MakeText(this, Resources.GetString(Resource.String.DeleteSpotSucceededMessage), ToastLength.Short).Show();
                Finish();
            });
            dialog.SetNegativeButton("No", delegate { });
            dialog.Show();
        }

        Hitchability? GetSelectedHitchability()
        {
            Hitchability? hitchability = null;
            if (lstHitchability.SelectedItemPosition >= 0)
            {
                var hitchabilityTuple = vm.HitchabilityOptions[lstHitchability.SelectedItemPosition];
                if (hitchabilityTuple != null)
                    hitchability = hitchabilityTuple.Item1 as Hitchability?;
            }
            return hitchability;
        }

        AttemptResult? GetSelectedAttemptResult()
        {
            AttemptResult? attemptResult = null;
            if (lstAttemptResult.SelectedItemPosition >= 0)
            {
                var attemptResultTuple = vm.AttemptResultOptions[lstAttemptResult.SelectedItemPosition];
                if (attemptResultTuple != null)
                    attemptResult = attemptResultTuple.Item1 as AttemptResult?;
            }
            return attemptResult;
        }

        MyHitchhikingSpots.Models.Map GetSelectedMap()
        {
            MyHitchhikingSpots.Models.Map res = null;
            if (lstMaps.SelectedItemPosition >= 0)
                res = vm.Maps[lstMaps.SelectedItemPosition];
            return res;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
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
                                errMsg = Resources.GetString(Resource.String.MapItemEditHitchabilityMandatoryText);

                            if (!String.IsNullOrEmpty(errMsg))
                                Toast.MakeText(this, errMsg, ToastLength.Long).Show();
                            else
                            {
                                // date = date.Add(((DateTime)tmpTime.Value).TimeOfDay);

                                vm.SaveSpot("", txtNote.Text, hitchability.Value, FullDateTime)
                                    .ContinueWith(t =>
                                    {
                                        if (t.IsFaulted)
                                            MyHitchhikingSpots.Tools.Tool.WriteExceptionMessagesToOutputBox(t.Exception);
                                    })
                                    .Wait();
                                //MessageBox.Show("Spot recorded successfully!");
                                Finish(); 
                            }
                            break;
                        }
                    case SpotDataState.BasicInfoInserted:
                        {
                            var errMsg = "";
                            int waitingTime = 0;

                            if (!int.TryParse(txtWaitingTime.Text, out waitingTime))
                                errMsg = Resources.GetString(Resource.String.MapItemEditInvalidWaitingTimeText);
                            if (attemptResult == null || !attemptResult.HasValue)
                                errMsg = Resources.GetString(Resource.String.MapItemEditAttemptResultMandatoryText);

                            if (!String.IsNullOrEmpty(errMsg))
                                Toast.MakeText(this, errMsg, ToastLength.Long).Show();
                            else
                            {
                                vm.EvaluateSpot(vm.SelectedMapItem, waitingTime, attemptResult.Value, mapItemId)
                                    .ContinueWith(t =>
                                    {
                                        if (t.IsFaulted)
                                            MyHitchhikingSpots.Tools.Tool.WriteExceptionMessagesToOutputBox(t.Exception);
                                    })
                                    .Wait();
                                Toast.MakeText(this, Resources.GetString(Resource.String.MapItemEditEvaluateSaveSucceededText), ToastLength.Long).Show();
                                Finish(); 
                            }
                            break;
                        }
                    case SpotDataState.FullInfoInserted:
                        {
                            var errMsg = "";
                            int waitingTime = 0;

                            if (!hitchability.HasValue)
                                errMsg = Resources.GetString(Resource.String.MapItemEditHitchabilityMandatoryText);
                            if (!int.TryParse(txtWaitingTime.Text, out waitingTime))
                                errMsg = Resources.GetString(Resource.String.MapItemEditInvalidWaitingTimeText);
                            if (attemptResult == null || !attemptResult.HasValue)
                                errMsg = Resources.GetString(Resource.String.MapItemEditAttemptResultMandatoryText);

                            if (!String.IsNullOrEmpty(errMsg))
                                Toast.MakeText(this, errMsg, ToastLength.Long).Show();
                            else
                            {
                                vm.UpdateSpot(vm.SelectedMapItem, "", txtNote.Text, hitchability.Value, FullDateTime, waitingTime, attemptResult.Value, mapItemId)
                                    .ContinueWith(t =>
                                    {
                                        if (t.IsFaulted)
                                            MyHitchhikingSpots.Tools.Tool.WriteExceptionMessagesToOutputBox(t.Exception);
                                    })
                                     .Wait();
                                Toast.MakeText(this, Resources.GetString(Resource.String.MapItemEditUpdateSucceededText), ToastLength.Long).Show();
                                Finish();
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Something unexpected happened when trying to save or update these information.\n" + ex.GetExceptionMessagesAsString(), ToastLength.Long).Show();
            }
        }
    }
}