using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Ultrasonic.DownloadManager.Core.Converters
{
    public class BooleanVisibilityConverter : ValueConverter
    {
        public Visibility DefaultVisibility { get; set; }
        public Visibility FalseVisibility { get; set; }
        public Visibility TrueVisibility { get; set; }

        public BooleanVisibilityConverter()
        {
            DefaultVisibility = Visibility.Collapsed;
            TrueVisibility = Visibility.Visible;
            FalseVisibility = Visibility.Collapsed;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = Visibility.Collapsed;

            if (value is bool)
            {
                bool v = (bool)value;

                vis = v ? TrueVisibility : FalseVisibility;
            }

            return vis;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
