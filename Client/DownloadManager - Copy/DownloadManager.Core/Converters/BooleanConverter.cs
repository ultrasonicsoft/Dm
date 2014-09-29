using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Ultrasonic.DownloadManager.Core.Converters
{
    public class BooleanConverter : ValueConverter
    {
        public bool IsOpposite { get; set; }

        public BooleanConverter()
        {
            IsOpposite = true;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            if (value is bool)
            {
                bool v = (bool)value;

                result = IsOpposite ? !v : v;
            }

            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
