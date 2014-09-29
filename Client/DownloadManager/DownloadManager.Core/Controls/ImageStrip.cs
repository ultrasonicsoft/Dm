using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ultrasonic.DownloadManager.Controls
{
    public class ImageStrip : Control
    {
        #region Dependency Properties

        public int Frame
        {
            get { return (int)GetValue(FrameProperty); }
            set { SetValue(FrameProperty, value); }
        }

        public static readonly DependencyProperty FrameProperty =
            DependencyProperty.Register("Frame", typeof(int), typeof(ImageStrip), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
        
        public double FrameHeight
        {
            get { return (double)GetValue(FrameHeightProperty); }
            set { SetValue(FrameHeightProperty, value); }
        }

        public static readonly DependencyProperty FrameHeightProperty =
            DependencyProperty.Register("FrameHeight", typeof(double), typeof(ImageStrip), new FrameworkPropertyMetadata(0D, FrameworkPropertyMetadataOptions.AffectsRender));
        
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageStrip), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Rendering

        protected override void OnRender(DrawingContext dc)
        {
            ImageSource image = Image;
            if (image != null)
            {
                int frame = Frame;
                double frameHeight = FrameHeight;
                Rect rect = new Rect(0, 0, RenderSize.Width, RenderSize.Height);

                ImageBrush brush = new ImageBrush(image);
                brush.Stretch = Stretch.None;
                brush.Viewbox = new Rect(0, (((frame + 0.5) * frameHeight) / image.Height) - 0.5, 1, 1);
                dc.DrawRectangle(brush, null, rect);
            }
        }

        #endregion
    }
}
