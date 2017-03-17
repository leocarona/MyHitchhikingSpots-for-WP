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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MyHitchhikingSpots.Converters
{

    /// <summary>
    /// Converts a null object into a Visibility. If null, visibility is collapsed.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If set to True, conversion is reversed: True will become Collapsed.
        /// </summary>
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Check if is null
            bool isNullOrEmpty = (value == null);

            //Check if is empty (empty string or empty collection)
            if (value != null)
                if (value is System.Collections.ICollection)
                    isNullOrEmpty = (((System.Collections.ICollection)value).Count == 0);
                else if (value is String)
                    isNullOrEmpty = String.IsNullOrEmpty((String)value);

            if (IsReversed)
                isNullOrEmpty = !isNullOrEmpty;

            if (isNullOrEmpty)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
