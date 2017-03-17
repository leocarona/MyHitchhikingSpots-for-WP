﻿//
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
    public class DateTimeToDateConverter : IValueConverter
    {
        string _stringFormat = "dd/MM/yyyy"; //"dd/MM/yyyy H:mm:ss"

        public string StringFormat
        {
            get { return _stringFormat; }
            set { _stringFormat = value; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return System.Convert.ToDateTime(value).ToString(StringFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException ();
        }
    }
}
