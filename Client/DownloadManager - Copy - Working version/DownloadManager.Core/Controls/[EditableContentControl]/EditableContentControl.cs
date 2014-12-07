using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace Ultrasonic.DownloadManager.Controls
{
    /// <summary>
    /// Represents a content control that displays either the view or edit template according to whether it is in edit mode.
    /// </summary>
    public class EditableContentControl : ContentControl
    {
        #region Dependency Properties

        #region ContentEditTemplate

        /// <summary>
        /// ContentEditTemplate Dependency Property.
        /// </summary>
        public static readonly DependencyProperty ContentEditTemplateProperty =
            DependencyProperty.Register("ContentEditTemplate", typeof(DataTemplate), typeof(EditableContentControl),
            new FrameworkPropertyMetadata(OnContentEditTemplateChanged));

        static void OnContentEditTemplateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EditableContentControl ctrl = o as EditableContentControl;

            if (ctrl != null)
            {
                DataTemplate template = e.NewValue as DataTemplate;

                if (ctrl.IsEditMode && template != null)
                {
                    ctrl.ContentTemplate = template;
                }
            }
        }

        /// <summary>
        /// Gets or sets the edit template.
        /// </summary>
        public DataTemplate ContentEditTemplate
        {
            get { return (DataTemplate)GetValue(ContentEditTemplateProperty); }
            set { SetValue(ContentEditTemplateProperty, value); }
        }

        #endregion

        #region ContentViewTemplate

        /// <summary>
        /// ContentViewTemplate Dependency Property.
        /// </summary>
        public static readonly DependencyProperty ContentViewTemplateProperty =
            DependencyProperty.Register("ContentViewTemplate", typeof(DataTemplate), typeof(EditableContentControl),
                new FrameworkPropertyMetadata(OnContentViewTemplateChanged));

        static void OnContentViewTemplateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EditableContentControl ctrl = o as EditableContentControl;

            if (ctrl != null)
            {
                DataTemplate template = e.NewValue as DataTemplate;

                if (!ctrl.IsEditMode && template != null)
                {
                    ctrl.ContentTemplate = template;
                }
            }
        }

        /// <summary>
        /// Gets or sets the view template.
        /// </summary>
        public DataTemplate ContentViewTemplate
        {
            get { return (DataTemplate)GetValue(ContentViewTemplateProperty); }
            set { SetValue(ContentViewTemplateProperty, value); }
        }

        #endregion

        #endregion

        #region Attached Properties

        #region IsEditMode

        /// <summary>
        /// IsEditMode Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.RegisterAttached("IsEditMode", typeof(bool), typeof(EditableContentControl), 
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnIsEditModeChanged)));

        static void OnIsEditModeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EditableContentControl ctrl = o as EditableContentControl;

            if (ctrl != null)
            {
                if ((bool)e.NewValue)
                {
                    if (ctrl.ContentEditTemplate != null)
                    {
                        ctrl.ContentTemplate = ctrl.ContentEditTemplate;
                    }
                }
                else
                {
                    if (ctrl.ContentViewTemplate != null)
                    {
                        ctrl.ContentTemplate = ctrl.ContentViewTemplate;
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether the control is in edit mode.
        /// </summary>
        public static bool GetIsEditMode(DependencyObject d)
        {
            return (bool)d.GetValue(IsEditModeProperty);
        }

        /// <summary>
        /// Sets whether the control is in edit mode.
        /// </summary>
        public static void SetIsEditMode(DependencyObject d, bool value)
        {
            d.SetValue(IsEditModeProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the control is in edit mode.
        /// </summary>
        public bool IsEditMode
        {
            get { return GetIsEditMode(this); }
            set { SetIsEditMode(this, value); }
        }

        #endregion

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EditableContentControl"/> class.
        /// </summary>
        public EditableContentControl()
        {
            //override getting focus to get the underlying template to receive the focusing appropriately
            Focusable = false;
        }

        #endregion

        #region Bindings
        protected Binding CreateForwardBinding(DependencyProperty sourceProperty)
        {
            Binding binding = new Binding(sourceProperty.Name)
            {
                Source = this
            };

            Binding boundValueBinding = BindingOperations.GetBinding(this, sourceProperty);
            if (boundValueBinding != null)
            {
                binding.AsyncState = boundValueBinding.AsyncState;
                binding.BindingGroupName = boundValueBinding.BindingGroupName;
                binding.IsAsync = boundValueBinding.IsAsync;
                binding.Mode = boundValueBinding.Mode;
                binding.UpdateSourceTrigger = boundValueBinding.UpdateSourceTrigger;
                binding.ValidatesOnDataErrors = boundValueBinding.ValidatesOnDataErrors;
                binding.ValidatesOnExceptions = boundValueBinding.ValidatesOnExceptions;
                binding.NotifyOnSourceUpdated = boundValueBinding.NotifyOnSourceUpdated;
                binding.NotifyOnTargetUpdated = boundValueBinding.NotifyOnTargetUpdated;
                binding.NotifyOnValidationError = boundValueBinding.NotifyOnValidationError;
            }

            return binding;
        }
        #endregion
    }
}
