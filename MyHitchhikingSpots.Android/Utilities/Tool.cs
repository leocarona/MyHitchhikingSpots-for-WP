using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using System.Collections.Generic;

#if EXPERIMENTSARENA
using Resource = ExperimentsArena.Resource;
#endif

namespace MyHitchhikingSpots.Tools
{
    public static class AndroidTool
    {

        public static void ShowError(Context context, string message)
        {

            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(Android.Resource.String.DialogAlertTitle);
            builder.SetIcon(Android.Resource.Drawable.IcDialogAlert);
            builder.SetMessage(message);
            builder.SetPositiveButton("OK", (sender, e) =>
            {

            });

            builder.Show();
        }

        public static void ShowInfo(Context context, string message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(Android.Resource.String.DialogAlertTitle);
            builder.SetIcon(Android.Resource.Drawable.IcDialogInfo);
            builder.SetMessage(message);
            builder.SetPositiveButton("OK", (sender, e) =>
            {

            });

            builder.Show();
        }


        public static void SetOnEnterListener(this Android.Widget.EditText editText, System.Action callBack = null)
        {
            editText.KeyPress += (object sender, Android.Views.View.KeyEventArgs e) =>
            {
                var handled = false;
                if (e.Event.Action == Android.Views.KeyEventActions.Down && e.KeyCode == Android.Views.Keycode.Enter)
                {
                    if (callBack != null)
                    {
                        callBack();
                        handled = true;
                    }
                }
                e.Handled = handled;
            };
        }

        public static void HideKeyboardOnLoseFocus(this Android.Widget.EditText editText, Context context)
        {
            editText.FocusChange += (s, e) =>
            {
                if (!e.HasFocus)
                {
                    Android.Views.InputMethods.InputMethodManager imm = (Android.Views.InputMethods.InputMethodManager)context.GetSystemService(Context.InputMethodService);
                    imm.HideSoftInputFromWindow(editText.WindowToken, 0);
                }
            };
        }

        /// <summary>
        /// Calls ClearFocus for the given view. This is just an extention to have a more intuitive name instead of ClearFocus.
        /// </summary>
        /// <param name="view"></param>
        public static void UnFocus(this View view)
        {
            view.ClearFocus();
        }


        public static T JavaCast<T>(this Java.Lang.Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }

        public static System.Object JavaCast(this Java.Lang.Object obj)
        {
            return obj.JavaCast<System.Object>();
        }

        public static void GreyOutIcon(int resourceId, Activity activity)
        {
            var view = activity.FindViewById<View>(resourceId);
            activity.RunOnUiThread(() =>
            {
                view.Background.SetColorFilter(Color.Gray, PorterDuff.Mode.Multiply);

            });
        }
        public static void GreyOutIconClear(int resourceId, Activity activity)
        {
            activity.FindViewById<View>(resourceId).Background.ClearColorFilter();
        }

        //public static Bitmap GetFullSizeImageAsBitmap(this MobiService.Core.Project.jobtable.LocalImageFile img)
        //{
        //    try
        //    {
        //        //if (IsImageLoaded())
        //        //{
        //        //    var stream = new System.IO.MemoryStream(Image);
        //        //    var bitmap = Android.Graphics.BitmapFactory.DecodeStream(stream);
        //        //    return bitmap;
        //        //}
        //        //else
        //        //{
        //        return BitmapFactory.DecodeFile(img.ImagePath);
        //        //}
        //    }
        //    catch (System.Exception ex)
        //    {
        //        MobiService.Core.Util.Tool.WriteExceptionMessagesToOutputBox(ex);
        //    }
        //    return null;
        //}


        //public bool IsImageLoaded()
        //{
        //    return Image != null;
        //}

        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            options.ApplyDimensions(width, height);

            return BitmapFactory.DecodeFile(fileName, options);
        }

        public static Bitmap LoadAndResizeBitmap(this Context context, int resource, int width, int height)
        {
            return LoadAndResizeBitmap(context.Resources, resource, width, height);
        }

        public static Bitmap LoadAndResizeBitmap(this Android.Content.Res.Resources resources, int resource, int width, int height)
        {     // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeResource(resources, resource, options);

            options.ApplyDimensions(width, height);

            return BitmapFactory.DecodeResource(resources, resource, options);
        }

        private static BitmapFactory.Options ApplyDimensions(this BitmapFactory.Options imageCurrentOptions, int newWidth, int newHeight)
        {
            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = imageCurrentOptions.OutHeight;
            int outWidth = imageCurrentOptions.OutWidth;
            int inSampleSize = 1;

            if (outHeight > newHeight || outWidth > newWidth)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / newHeight
                                   : outWidth / newWidth;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            imageCurrentOptions.InSampleSize = inSampleSize;
            imageCurrentOptions.InJustDecodeBounds = false;
            return imageCurrentOptions;
        }


        public static Android.Gms.Maps.Model.LatLng GetLatLng(this MyHitchhikingSpots.Models.MapItem mapItem)
        {
            if (mapItem.Latitude != 0 && mapItem.Longitude != 0)
                return new Android.Gms.Maps.Model.LatLng(mapItem.Latitude, mapItem.Longitude);
            else
                return null;
        }

        public static Android.Gms.Maps.Model.LatLng GetLatLng(this Xamarin.Geolocation.Position position)
        {
            if (position.Latitude != 0 && position.Longitude != 0)
                return new Android.Gms.Maps.Model.LatLng(position.Latitude, position.Longitude);
            else
                return null;
        }

    }
}