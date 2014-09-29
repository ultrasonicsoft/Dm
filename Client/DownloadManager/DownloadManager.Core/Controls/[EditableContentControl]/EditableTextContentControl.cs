using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace Ultrasonic.DownloadManager.Controls
{
    /// <summary>
    /// Represents a content control that displays either the textblock or textbox according to whether it is in edit mode.
    /// </summary>
    public class EditableTextContentControl : EditableContentControl
    {
        #region Dependency Properties

        #region MaxLength

        /// <summary>
        /// MaxLength Dependency Property
        /// </summary>
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int), typeof(EditableTextContentControl));

        /// <summary>
        /// Gets or sets the MaxLength property.
        /// </summary>
        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        #endregion

        #region TextBoxStyle

        /// <summary>
        /// TextBoxStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register("TextBoxStyle", typeof(Style), typeof(EditableTextContentControl));

        /// <summary>
        /// Gets or sets the TextBoxStyle property.
        /// </summary>
        public Style TextBoxStyle
        {
            get { return (Style)GetValue(TextBoxStyleProperty); }
            set { SetValue(TextBoxStyleProperty, value); }
        }

        #endregion

        #region TextBlockStyle

        /// <summary>
        /// TextBlockStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextBlockStyleProperty =
            DependencyProperty.Register("TextBlockStyle", typeof(Style), typeof(EditableTextContentControl));

        /// <summary>
        /// Gets or sets the TextBlockStyle property. 
        /// </summary>
        public Style TextBlockStyle
        {
            get { return (Style)GetValue(TextBlockStyleProperty); }
            set { SetValue(TextBlockStyleProperty, value); }
        }

        #endregion

        #region Text

        private BindingBase _textBinding;
        public BindingBase Text
        {
            get { return _textBinding; }
            set { _textBinding = value; }
        }

        #endregion

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            FrameworkElementFactory viewFactory = new FrameworkElementFactory(typeof(TextBlock));
            if (_textBinding != null)
            {
                viewFactory.SetBinding(TextBlock.TextProperty, _textBinding);
            }
            viewFactory.SetBinding(FrameworkElement.DataContextProperty, CreateForwardBinding(DataContextProperty));
            if (TextBlockStyle != null)
            {
                viewFactory.SetBinding(Control.StyleProperty, new Binding(TextBlockStyleProperty.Name) { Source = this });
            }
            DataTemplate viewTemplate = new DataTemplate { VisualTree = viewFactory };

            ContentViewTemplate = viewTemplate;

            FrameworkElementFactory editFactory = new FrameworkElementFactory(typeof(TextBox));
            if (_textBinding != null)
            {
                editFactory.SetBinding(TextBox.TextProperty, _textBinding);
            }
            editFactory.SetBinding(FrameworkElement.DataContextProperty, CreateForwardBinding(DataContextProperty));
            if (TextBoxStyle != null)
            {
                editFactory.SetBinding(Control.StyleProperty, new Binding(TextBoxStyleProperty.Name) { Source = this });
            }
            editFactory.SetBinding(TextBox.MaxLengthProperty, new Binding(MaxLengthProperty.Name) { Source = this });
            DataTemplate editTemplate = new DataTemplate { VisualTree = editFactory };

            ContentEditTemplate = editTemplate;
        }
    }
}
