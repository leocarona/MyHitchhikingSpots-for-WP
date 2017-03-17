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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MyHitchhikingSpots.Converters
{
    public class RGBStringToColorConverter : IValueConverter
    {
        public bool AlphaValueIsDouble = true;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String rgbColorSemicolonSepareted = value as String;

            if (!String.IsNullOrEmpty(rgbColorSemicolonSepareted))
            {
                if (String.IsNullOrEmpty(rgbColorSemicolonSepareted))
                    return Colors.Transparent;

                int alphaColor = 255; //255 corresponds to 100% of oppacity for windows phone 
                var arr = rgbColorSemicolonSepareted.Split(new string[] { ";" }, StringSplitOptions.None);

                var rgbByteArr = (from b in arr.Take(3)
                                  select System.Convert.ToByte(b)).ToArray();

                //If the alpha channel is percent, we must multiply it for 255 to get its corresponding value for windows phone.
                if (AlphaValueIsDouble && arr.Count() > 3)
                    alphaColor = System.Convert.ToInt32(alphaColor * System.Convert.ToDouble(arr[3]));

                return Color.FromArgb(System.Convert.ToByte(alphaColor), rgbByteArr[0], rgbByteArr[1], rgbByteArr[2]);
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
