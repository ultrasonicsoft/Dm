using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ultrasonic.DownloadManager.Controls
{
    /// <summary>
    /// NOTE - The code that is related to the 3D models and the book handling was written by Roberto Sonnino
    /// Check http://www.codeproject.com/KB/WPF/3D-BookWriter.aspx
    /// </summary>
    [TemplatePart(Name = "PART_Main3D", Type = typeof(Viewport3D))]
    [TemplatePart(Name = "PART_ModelElement", Type = typeof(ModelUIElement3D))]
    [TemplatePart(Name = "PART_InputText", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_LeftPage", Type = typeof(Viewport2DVisual3D))]
    [TemplatePart(Name = "PART_RightPage", Type = typeof(Viewport2DVisual3D))]
    [TemplatePart(Name = "PART_InkDraw", Type = typeof(InkCanvas))]
    public class FeedbackView : Control
    {
        #region Fields
        const double DefaultOpenCloseInSeconds = 1.5;
        public const string InkImageExtension = "png";
        public const string InkImageContentType = "image/png";

        Type resourceFindType;
        Viewport3D main3d;
        TextBox inputText;
        Viewport2DVisual3D leftPage;
        Viewport2DVisual3D rightPage;
        ModelUIElement3D modelElement;
        InkCanvas inkDraw;
        #endregion

        #region Dependency Properties
        public string FeedbackText
        {
            get { return (string)GetValue(FeedbackTextProperty); }
            set { SetValue(FeedbackTextProperty, value); }
        }
        public static readonly DependencyProperty FeedbackTextProperty =
            DependencyProperty.Register("FeedbackText", typeof(string), typeof(FeedbackView));

        public bool IsExpandedView
        {
            get { return (bool)GetValue(IsExpandedViewProperty); }
            set { SetValue(IsExpandedViewProperty, value); }
        }
        public static readonly DependencyProperty IsExpandedViewProperty =
            DependencyProperty.Register("IsExpandedView", typeof(bool), typeof(FeedbackView),
            new UIPropertyMetadata(new PropertyChangedCallback(OnIsExpandedViewChanged)));

        public bool IsBookOpened
        {
            get { return (bool)GetValue(IsBookOpenedProperty); }
            set { SetValue(IsBookOpenedProperty, value); }
        }
        public static readonly DependencyProperty IsBookOpenedProperty =
            DependencyProperty.Register("IsBookOpened", typeof(bool), typeof(FeedbackView),
            new UIPropertyMetadata(false, new PropertyChangedCallback(OnIsBookOpenedChanged)));

        public bool OpenBookWhenExpanded
        {
            get { return (bool)GetValue(OpenBookWhenExpandedProperty); }
            set { SetValue(OpenBookWhenExpandedProperty, value); }
        }
        public static readonly DependencyProperty OpenBookWhenExpandedProperty =
            DependencyProperty.Register("OpenBookWhenExpanded", typeof(bool), typeof(FeedbackView),
            new UIPropertyMetadata(true));

        public bool CommandElementsEnabled
        {
            get { return (bool)GetValue(CommandElementsEnabledProperty); }
            set { SetValue(CommandElementsEnabledProperty, value); }
        }
        public static readonly DependencyProperty CommandElementsEnabledProperty =
            DependencyProperty.Register("CommandElementsEnabled", typeof(bool), typeof(FeedbackView),
            new UIPropertyMetadata(true));

        public Style CollapsedSwitchViewButtonStyle
        {
            get { return (Style)GetValue(CollapsedSwitchViewButtonStyleProperty); }
            set { SetValue(CollapsedSwitchViewButtonStyleProperty, value); }
        }
        public static readonly DependencyProperty CollapsedSwitchViewButtonStyleProperty =
            DependencyProperty.Register("CollapsedSwitchViewButtonStyle", typeof(Style), typeof(FeedbackView));
        #endregion

        #region Properties
        public string ImageFileFormat
        {
            get { return "png"; }
        }
        #endregion

        #region Ctors
        static FeedbackView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FeedbackView),
                new FrameworkPropertyMetadata(typeof(FeedbackView)));

            CommandManager.RegisterClassCommandBinding(typeof(FeedbackView),
                new CommandBinding(SaveFeedbackCommand, ExecuteSaveFeedback, CanExecuteSaveFeedback));

            CommandManager.RegisterClassCommandBinding(typeof(FeedbackView),
                new CommandBinding(ToggleDisplayModeCommand, ExecuteToggleDisplayMode, CanExecuteToggleDisplayMode));
        }

        public FeedbackView()
        {
            resourceFindType = this.GetType();
        }
        #endregion

        #region Commands
        #region SaveFeedbackCommand
        public static RoutedCommand SaveFeedbackCommand = new RoutedCommand("SaveFeedback", typeof(FeedbackView));

        static void ExecuteSaveFeedback(object sender, ExecutedRoutedEventArgs e)
        {
            FeedbackView view = sender as FeedbackView;

            if (view != null)
            {
                view.SaveFeedback();
            }
        }

        static void CanExecuteSaveFeedback(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion

        #region ToggleDisplayModeCommand
        public static RoutedCommand ToggleDisplayModeCommand = new RoutedCommand("ToggleDisplayMode", typeof(FeedbackView));

        static void ExecuteToggleDisplayMode(object sender, ExecutedRoutedEventArgs e)
        {
            FeedbackView view = sender as FeedbackView;

            bool? expanded = null;
            if (e.Parameter != null && e.Parameter is bool)
            {
                expanded = (bool)e.Parameter;
            }

            if (view != null)
            {
                view.ToggleDisplayMode(expanded);
            }
        }

        static void CanExecuteToggleDisplayMode(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion

        void ToggleDisplayMode(bool? isExpandedView)
        {
            bool expanded = isExpandedView.HasValue
                ? isExpandedView.Value
                : !IsExpandedView;

            IsExpandedView = expanded;
        }

        void SaveFeedback()
        {
            OnFeedbackSave();
        }
        #endregion

        #region Events
        #region FeedbackSaveEvent
        public static readonly RoutedEvent FeedbackSaveEvent
            = EventManager.RegisterRoutedEvent("FeedbackSave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FeedbackView));

        public event RoutedEventHandler FeedbackSave
        {
            add { AddHandler(FeedbackSaveEvent, value); }
            remove { RemoveHandler(FeedbackSaveEvent, value); }
        }

        protected void OnFeedbackSave()
        {
            RoutedEventArgs args = new RoutedEventArgs(FeedbackSaveEvent, this);

            RaiseEvent(args);
        }
        #endregion
        #endregion

        #region Animations
        RotateTransform3D transformLeftRotation;
        RotateTransform3D transformRightRotation;
        Transform3DGroup transformLeft;
        Transform3DGroup transformRight;
        Transform3DGroup transformFrontCover;
        Transform3DGroup transformBackCover;
        TranslateTransform3D transformSpineCoverTranslation;
        RotateTransform3D transformSpineRotation;
        Transform3DGroup transformSpineCover;

        void InitAnimationFields()
        {
            transformLeftRotation = new RotateTransform3D
            {
                Rotation = new AxisAngleRotation3D
                {
                    Angle = 15,
                    Axis = new Vector3D(0, 1, 0)
                }
            };

            transformRightRotation = new RotateTransform3D
            {
                Rotation = new AxisAngleRotation3D
                {
                    Angle = -15,
                    Axis = new Vector3D(0, 1, 0)
                }
            };

            transformLeft = new Transform3DGroup
            {
                Children = new Transform3DCollection
                {
                    new TranslateTransform3D { OffsetX = -0.72 },
                    transformLeftRotation
                }
            };

            transformRight = new Transform3DGroup
            {
                Children = new Transform3DCollection
                {
                    new TranslateTransform3D { OffsetX = 0.72 },
                    transformRightRotation
                }
            };

            transformFrontCover = new Transform3DGroup
            {
                Children = new Transform3DCollection
                {
                    new TranslateTransform3D { OffsetX = -0.72, OffsetZ = -0.125 },
                    transformLeftRotation
                }
            };

            transformBackCover = new Transform3DGroup
            {
                Children = new Transform3DCollection
                {
                    new TranslateTransform3D { OffsetX = 0.72, OffsetZ = -0.125 },
                    transformRightRotation
                }
            };

            transformSpineCoverTranslation = new TranslateTransform3D { OffsetZ = -0.140625 };

            transformSpineRotation = new RotateTransform3D
            {
                CenterZ = -0.125,
                Rotation = new AxisAngleRotation3D
                {
                    Angle = 0,
                    Axis = new Vector3D(0, 1, 0)
                }
            };

            transformSpineCover = new Transform3DGroup
            {
                Children = new Transform3DCollection
                {
                    new ScaleTransform3D { ScaleX = 0.2 },
                    transformSpineCoverTranslation,
                    transformSpineRotation
                }
            };
        }
        #endregion

        #region Event Invocations
        static void OnIsExpandedViewChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FeedbackView view = o as FeedbackView;

            if (view.main3d != null)
            {
                bool newVal = (bool)e.NewValue;
                if (!newVal)
                {
                    view.IsBookOpened = false;
                }
                else if (view.OpenBookWhenExpanded)
                {
                    view.Dispatcher.BeginInvoke(new Action(() => view.IsBookOpened = true),
                        DispatcherPriority.Background);
                }
            }
        }

        static void OnIsBookOpenedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FeedbackView view = o as FeedbackView;

            if (view.main3d != null)
            {
                bool newVal = (bool)e.NewValue;
                if (newVal)
                {
                    view.OpenBook(DefaultOpenCloseInSeconds);

                    view.inputText.Focus();
                }
                else
                {
                    view.ResetData();

                    view.CloseBook(DefaultOpenCloseInSeconds);
                }
            }
        }
        #endregion

        #region Data
        public byte[] GetInkAsImage()
        {
            if (inkDraw == null || inkDraw.Strokes.Count == 0)
            {
                return null;
            }

            //get the dimensions of the ink control
            int margin = (int)this.inkDraw.Margin.Left;
            int width = (int)this.inkDraw.ActualWidth - margin;
            int height = (int)this.inkDraw.ActualHeight - margin;

            //render ink to bitmap
            RenderTargetBitmap rtb =
               new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            rtb.Render(inkDraw);

            //save the ink to a memory stream
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            byte[] bitmapBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);

                //get the bitmap bytes from the memory stream
                ms.Position = 0;
                bitmapBytes = ms.ToArray();
            }

            return bitmapBytes;
        }

        public void ResetData()
        {
            FeedbackText = string.Empty;
            inkDraw.Strokes.Clear();
        }
        #endregion

        #region Template
        /// <summary>
        /// This is needed only if you have multiple feedback views and you wish to switch between them
        /// This is a hack needed for the showcases
        /// In a real application you will probably have 1 instance of this control
        /// </summary>
        public void RemoveNeededChildrenForInitOther()
        {
            main3d.Children.Clear();

            if (modelElement != null)
            {
                Model3DGroup modelGroup = (Model3DGroup)modelElement.Model;
                modelGroup.Children.Clear();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            main3d = GetTemplateChild("PART_Main3D") as Viewport3D;
            modelElement = GetTemplateChild("PART_ModelElement") as ModelUIElement3D;
            inputText = GetTemplateChild("PART_InputText") as TextBox;
            rightPage = GetTemplateChild("PART_RightPage") as Viewport2DVisual3D;
            leftPage = GetTemplateChild("PART_LeftPage") as Viewport2DVisual3D;
            inkDraw = GetTemplateChild("PART_InkDraw") as InkCanvas;

            InitAnimationFields();

            if (modelElement != null)
            {
                modelElement.MouseDown += Cover_MouseDown;

                Model3DGroup modelGroup = (Model3DGroup)modelElement.Model;

                modelGroup.Children[0].Transform = transformFrontCover;
                modelGroup.Children[1].Transform = transformBackCover;
                modelGroup.Children[2].Transform = transformSpineCover;
                modelGroup.Children[3].Transform = transformLeft;
                modelGroup.Children[4].Transform = transformRight;
            }
            if (inputText != null)
            {
                //inputText.MouseDoubleClick += Page_MouseDoubleClick;
            }
            if (leftPage != null)
            {
                leftPage.Transform = transformLeft;
            }
            if (rightPage != null)
            {
                rightPage.Transform = transformRight;
            }

            if (!IsBookOpened)
            {
                CloseBook(0);
            }
            else
            {
                OpenBook(0);
            }
        }

        /// <summary>
        /// Event handler for the MouseDoubleClick event of the TextBoxes
        /// </summary>
        private void Page_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //TODO - read text?
            //SpeechSynthesizer synth = new SpeechSynthesizer();
            //synth.SpeakAsync(((TextBox)sender).Text);
        }

        /// <summary>
        /// Event handler for the MouseDown event of the cover, back cover, spine and edges
        /// </summary>
        private void Cover_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsBookOpened = !IsBookOpened;
        }

        /// <summary>
        /// Event handler for the PreviewMouseRightButtonDown event of the InkCanvas (right page)
        /// </summary>
        private void InkCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Switch InkCanvas editing mode
            InkCanvas ic = sender as InkCanvas;
            ic.EditingMode = (ic.EditingMode == InkCanvasEditingMode.Ink)
                ? InkCanvasEditingMode.EraseByPoint
                : InkCanvasEditingMode.Ink;
        }

        /// <summary>
        /// Opens the 3D book.
        /// </summary>
        /// <param name="durationSeconds">Time in seconds that the animation will take.</param>
        void OpenBook(double durationSeconds)
        {
            // Transform3D_LeftRotation
            RotateTransform3D rot = transformLeftRotation;
            DoubleAnimation da = new DoubleAnimation(15, new Duration(TimeSpan.FromSeconds(durationSeconds)));
            da.DecelerationRatio = 1;
            rot.Rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

            // Transform3D_RightRotation
            rot = transformRightRotation;
            da = new DoubleAnimation(-15, new Duration(TimeSpan.FromSeconds(durationSeconds)));
            rot.Rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

            // Transform3D_SpineRotation
            rot = transformSpineRotation;
            da = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.8333 * durationSeconds)));
            rot.Rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

            // Transform3D_SpineCoverTranslation
            TranslateTransform3D trans = transformSpineCoverTranslation;
            da = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.8333 * durationSeconds)));
            trans.BeginAnimation(TranslateTransform3D.OffsetXProperty, da);

            EnsureValidCamera();

            // _Main3D.Camera
            Point3DAnimation pa = new Point3DAnimation(new Point3D(0, -2.5, 6.5), new Duration(TimeSpan.FromSeconds(durationSeconds)));
            pa.AccelerationRatio = 0.5;
            pa.DecelerationRatio = 0.5;
            ((PerspectiveCamera)main3d.Camera).BeginAnimation(PerspectiveCamera.PositionProperty, pa);
        }

        /// <summary>
        /// Closes the 3D book.
        /// </summary>
        /// <param name="durationSeconds">Time in seconds that the animation will take.</param>
        void CloseBook(double durationSeconds)
        {
            // Transform3D_LeftRotation
            RotateTransform3D rot = transformLeftRotation;
            DoubleAnimation da = new DoubleAnimation(180, new Duration(TimeSpan.FromSeconds(durationSeconds)));
            da.DecelerationRatio = 1;
            rot.Rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

            // Transform3D_RightRotation
            rot = transformRightRotation;
            da = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(durationSeconds)));
            rot.Rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

            // Transform3D_SpineRotation
            rot = transformSpineRotation;
            da = new DoubleAnimation(90, new Duration(TimeSpan.FromSeconds(0.8333 * durationSeconds)));
            rot.Rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

            // Transform3D_SpineCoverTranslation
            TranslateTransform3D trans = transformSpineCoverTranslation;
            da = new DoubleAnimation(-0.125, new Duration(TimeSpan.FromSeconds(0.8333 * durationSeconds)));
            trans.BeginAnimation(TranslateTransform3D.OffsetXProperty, da);

            EnsureValidCamera();

            // _Main3D.Camera
            Point3DAnimation pa = new Point3DAnimation(new Point3D(0.72, -2.5, 6.5), new Duration(TimeSpan.FromSeconds(durationSeconds)));
            pa.AccelerationRatio = 0.5;
            pa.DecelerationRatio = 0.5;
            ((PerspectiveCamera)main3d.Camera).BeginAnimation(PerspectiveCamera.PositionProperty, pa);
        }

        void EnsureValidCamera()
        {
            if (main3d.Camera == null || main3d.Camera.IsFrozen)
            {
                main3d.Camera = CreateCamera();
            }
        }

        Camera CreateCamera()
        {
            return new PerspectiveCamera
            {
                Position = new Point3D(0, -2.5, 6.5),
                LookDirection = new Vector3D(0, 2.5, -6.5),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 30
            };
        }
        #endregion
    }
}
