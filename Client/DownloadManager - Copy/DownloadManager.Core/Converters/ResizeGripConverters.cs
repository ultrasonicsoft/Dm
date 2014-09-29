using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ultrasonic.DownloadManager.Controls;

namespace Ultrasonic.DownloadManager.Core.Converters
{
    public sealed class GripAlignmentConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Orientation orientation = (Orientation)parameter;
            ResizeDirection resizeDirection = (ResizeDirection)value;

            switch (orientation)
            {
                case Orientation.Horizontal:
                    if (resizeDirection == ResizeDirection.NorthEast || resizeDirection == ResizeDirection.SouthEast)
                    {
                        return HorizontalAlignment.Right;
                    }
                    else
                    {
                        return HorizontalAlignment.Left;
                    }
                case Orientation.Vertical:
                    if (resizeDirection == ResizeDirection.NorthEast || resizeDirection == ResizeDirection.NorthWest)
                    {
                        return VerticalAlignment.Top;
                    }
                    else
                    {
                        return VerticalAlignment.Bottom;
                    }
            }

            return DependencyProperty.UnsetValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public sealed class GripCursorConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ResizeDirection resizeDirection = (ResizeDirection)value;

            switch (resizeDirection)
            {
                case ResizeDirection.NorthEast:
                case ResizeDirection.SouthWest:
                    return Cursors.SizeNESW;
                case ResizeDirection.NorthWest:
                case ResizeDirection.SouthEast:
                    return Cursors.SizeNWSE;
            }

            return DependencyProperty.UnsetValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public sealed class GripRotationConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ResizeDirection resizeDirection = (ResizeDirection)value;

            switch (resizeDirection)
            {
                case ResizeDirection.SouthWest:
                    return 90;
                case ResizeDirection.NorthWest:
                    return 180;
                case ResizeDirection.NorthEast:
                    return 270;
            }

            return 0;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
