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

namespace MyHitchhikingSpots.Dialog
{
    /// <summary>
    /// Base dialog class, mainly for setting the CustomDialogTheme
    /// </summary>
    public class BaseDialog : Android.App.Dialog
    {
        public BaseDialog(Context context)
            : base(context, Resource.Style.CustomDialogTheme)
        {

        }
    }
}