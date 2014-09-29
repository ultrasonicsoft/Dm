using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Ultrasonic.DownloadManager.Controls;
using System.Diagnostics;
using System.Windows.Input;

namespace Ultrasonic.DownloadManager.Core.Themes
{
    public partial class ResizerGeneric : ResourceDictionary
    {
        private void PART_Grip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                (sender as FrameworkElement).CaptureMouse();
                Resizer.StartResizeCommand.Execute(sender as FrameworkElement, sender as FrameworkElement);
                e.Handled = true;
            }
        }

        private void PART_Grip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement resizeGrip = sender as FrameworkElement;
            Debug.Assert(resizeGrip != null);

            if (resizeGrip.IsMouseCaptured)
            {
                Resizer.EndResizeCommand.Execute(null, sender as FrameworkElement);
                resizeGrip.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void PART_Grip_MouseMove(object sender, MouseEventArgs e)
        {
            if ((sender as FrameworkElement).IsMouseCaptured)
            {
                Resizer.UpdateSizeCommand.Execute(null, sender as FrameworkElement);
                e.Handled = true;
            }
        }

        private void PART_Grip_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Resizer.AutoSizeCommand.Execute(null, sender as FrameworkElement);
                e.Handled = true;
            }
        }
    }
}
