using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using Ultrasonic.DownloadManager.Core.Utils;

namespace Ultrasonic.DownloadManager.Controls
{
    [TemplatePart(Name = "PART_ImageHolder", Type = typeof(ContentPresenter))]
    public class StatusIndicator : ContentControl
    {
        #region Fields
        private ImageStrip imageStrip;
        #endregion

        #region Dependency Properties
        public Style ImageStripStyle
        {
            get { return (Style)GetValue(ImageStripStyleProperty); }
            set { SetValue(ImageStripStyleProperty, value); }
        }
        public static readonly DependencyProperty ImageStripStyleProperty =
            DependencyProperty.Register("ImageStripStyle", typeof(Style), typeof(StatusIndicator));
        #endregion

        #region Ctors
        static StatusIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusIndicator),
                new FrameworkPropertyMetadata(typeof(StatusIndicator)));
        }

        public StatusIndicator()
        {
            
        }
        #endregion

        #region Overrides
        protected override void OnInitialized(EventArgs e)
        {
            imageStrip = new ImageStrip();
            imageStrip.Name = "ProgressImage";
            imageStrip.Style = ImageStripStyle;

            base.OnInitialized(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            SetVisualTree();
        }

        private void SetVisualTree()
        {
            imageStrip.Style = ImageStripStyle;

            ContentPresenter imageHolder = GetTemplateChild("PART_ImageHolder") as ContentPresenter;

            imageHolder.Content = imageStrip;
        }
        #endregion
    }
}