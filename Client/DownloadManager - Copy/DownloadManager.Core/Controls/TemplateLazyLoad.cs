using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;

namespace Ultrasonic.DownloadManager.Controls
{
    public class TemplateLazyLoad : Decorator
    {
        bool templateLoaded;

        #region Dependency Properties

        public DataTemplate LoadTemplate
        {
            get { return (DataTemplate)GetValue(LoadTemplateProperty); }
            set { SetValue(LoadTemplateProperty, value); }
        }
        public static readonly DependencyProperty LoadTemplateProperty =
            DependencyProperty.Register("LoadTemplate", typeof(DataTemplate), typeof(TemplateLazyLoad));

        #endregion

        public TemplateLazyLoad()
        {
            Loaded += new RoutedEventHandler(TemplateLazyLoad_Loaded);
        }

        void TemplateLazyLoad_Loaded(object sender, RoutedEventArgs e)
        {
            if (!templateLoaded && LoadTemplate != null && LoadTemplate.HasContent)
            {
                UIElement elem = LoadTemplate.LoadContent() as UIElement;

                if (elem != null)
                {
                    Dispatcher.BeginInvoke(
                        new Action(() => this.Child = elem),
                        DispatcherPriority.Background);
                }

                templateLoaded = true;
            }
        }
    }
}
