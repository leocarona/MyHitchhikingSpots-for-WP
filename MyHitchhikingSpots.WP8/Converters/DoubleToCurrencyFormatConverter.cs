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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;


namespace MyHitchhikingSpots.Converters
{

    /// <summary>
    /// Converts a Boolean into a Visibility.
    /// </summary>
    public class DoubleToCurrencyFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            System.Globalization.CultureInfo culture = CultureInfo.CurrentCulture;
            var mutableNfi = (NumberFormatInfo)culture.NumberFormat.Clone();

            //Remove the currency symbol
            mutableNfi.CurrencySymbol = ""; 

            return System.Convert.ToDouble(value).ToString("F", mutableNfi);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
