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

    public class MapItemToAddressConverter : IValueConverter
    {
        public bool ExcludeStreet { get; set; }
        public bool ExcludeZip { get; set; }
        public bool ExcludeState { get; set; }
        public bool ExcludeCity { get; set; }
        public bool ExcludeCountry { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mapItem = value as Models.MapItem;

            if (mapItem == null)
                return String.Empty;
            else
                return String.Format("{0}{1}{2}{3}{4}",
                                        (!ExcludeStreet && mapItem.Street != null) ? mapItem.Street + ", " : "",
                                        (!ExcludeZip && mapItem.Zip != null) ? mapItem.Zip + ", " : "",
                                        (!ExcludeCity && mapItem.City != null) ? mapItem.City + ", " : "",
                                        (!ExcludeState && mapItem.State != null) ? mapItem.State + ", " : "",
                                        (!ExcludeCountry && mapItem.Country != null) ? mapItem.Country + ", " : ""
                                    ).TrimEnd(' ', ',');
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
