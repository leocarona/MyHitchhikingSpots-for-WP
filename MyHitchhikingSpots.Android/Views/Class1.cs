using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace MyHitchhikingSpots
{
    [Activity(Label = "DELETE", Icon = "@drawable/icon")]//, MainLauncher = true
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Console.WriteLine("entrou");
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MainActivity);
            StartActivity(typeof(MyHitchhikingSpots.Views.SpotEditActivity));
            // Get our button from the layout resource,
            // and attach an event to it
        //    Button button = FindViewById<Button>(Resource.Id.MyButton);

        //    button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };
        }
    }
}

