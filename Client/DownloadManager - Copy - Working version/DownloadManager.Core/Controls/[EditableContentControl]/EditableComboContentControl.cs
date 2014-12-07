using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Collections;

namespace Ultrasonic.DownloadManager.Controls
{
    /// <summary>
    /// Represents a content control that displays either the textblock or combobox according to whether it is in edit mode.
    /// </summary>
    public class EditableComboContentControl : EditableContentControl
    {
        #region Dependency Properties

        #region ComboBoxStyle

        /// <summary>
        /// ComboBoxStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty ComboBoxStyleProperty =
            DependencyProperty.Register("ComboBoxStyle", typeof(Style), typeof(EditableComboContentControl));

        /// <summary>
        /// Gets or sets the ComboBoxStyle property. 
        /// </summary>
        public Style ComboBoxStyle
        {
            get { return (Style)GetValue(ComboBoxStyleProperty); }
            set { SetValue(ComboBoxStyleProperty, value); }
        }

        #endregion

        #region TextBlockStyle

        /// <summary>
        /// TextBlockStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextBlockStyleProperty =
            DependencyProperty.Register("TextBlockStyle", typeof(Style), typeof(EditableComboContentControl));

        /// <summary>
        /// Gets or sets the TextBlockStyle property. 
        /// </summary>
        public Style TextBlockStyle
        {
            get { return (Style)GetValue(TextBlockStyleProperty); }
            set { SetValue(TextBlockStyleProperty, value); }
        }

        #endregion

        #region ItemsSource

        /// <summary>
        /// ItemsSource Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(EditableComboContentControl));

        /// <summary>
        /// Gets or sets the ItemsSource property.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        #endregion

        #region ItemTemplate

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty = ItemsControl.ItemTemplateProperty.AddOwner(typeof(EditableComboContentControl));

        #endregion

        #region DisplayMemberPath

        /// <summary>
        /// DisplayMemberPath Dependency Property
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(EditableComboContentControl));

        /// <summary>
        /// Gets or sets the DisplayMemberPath property.
        /// </summary>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        #endregion

        #region SelectedValueMemberPath

        /// <summary>
        /// SelectedValuePath Dependency Property
        /// </summary>
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(EditableComboContentControl));

        /// <summary>
        /// Gets or sets the SelectedValuePath property.
        /// </summary>
        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        #endregion

        #region SelectedItem

        public BindingBase SelectedItem { get; set; }

        #endregion

        #region SelectedIndex

        public BindingBase SelectedIndex { get; set; }

        #endregion

        #region SelectedValue

        public BindingBase SelectedValue { get; set; }

        #endregion

        #region Text

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditableComboContentControl));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            FrameworkElementFactory viewFactory = new FrameworkElementFactory(typeof(TextBlock));
            viewFactory.SetBinding(FrameworkElement.DataContextProperty, CreateForwardBinding(DataContextProperty));
            viewFactory.SetBinding(TextBlock.TextProperty, CreateForwardBinding(TextProperty));

            if (TextBlockStyle != null)
            {
                viewFactory.SetBinding(Control.StyleProperty, new Binding(TextBlockStyleProperty.Name) { Source = this });
            }

            DataTemplate viewTemplate = new DataTemplate { VisualTree = viewFactory };

            ContentViewTemplate = viewTemplate;

            FrameworkElementFactory editFactory = new FrameworkElementFactory(typeof(ComboBox));
            editFactory.SetBinding(FrameworkElement.DataContextProperty, CreateForwardBinding(DataContextProperty));

            if (ComboBoxStyle != null)
            {
                editFactory.SetBinding(Control.StyleProperty, new Binding(ComboBoxStyleProperty.Name) { Source = this });
            }

            editFactory.SetBinding(ComboBox.ItemsSourceProperty, new Binding(ItemsSourceProperty.Name) { Source = this });
            editFactory.SetBinding(ComboBox.ItemTemplateProperty, new Binding(ItemTemplateProperty.Name) { Source = this });
            editFactory.SetBinding(ComboBox.DisplayMemberPathProperty, new Binding(DisplayMemberPathProperty.Name) { Source = this });
            editFactory.SetBinding(ComboBox.SelectedValuePathProperty, new Binding(SelectedValuePathProperty.Name) { Source = this });

            if (SelectedValue != null)
            {
                editFactory.SetBinding(ComboBox.SelectedValueProperty, SelectedValue);
            }

            if (SelectedItem != null)
            {
                editFactory.SetBinding(ComboBox.SelectedItemProperty, SelectedItem);
            }

            if (SelectedIndex != null)
            {
                editFactory.SetBinding(ComboBox.SelectedIndexProperty, SelectedIndex);
            }

            DataTemplate editTemplate = new DataTemplate { VisualTree = editFactory };
            ContentEditTemplate = editTemplate;
        }
    }
}
