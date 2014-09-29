using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;
using Ultrasonic.DownloadManager.Core.Utils;

namespace Ultrasonic.DownloadManager.Controls
{
    //There is an issue with the control where the entire tree is loaded, 
    //which may affect performance and memory allocation when working with many items. 
    //Unfortunately, I don't think I would be able to fix that any time soon
    [TemplatePart(Name = "PART_ItemsViewPresenter", Type = typeof(Decorator))]
    [TemplatePart(Name = "PART_ItemsViewPlaceholder", Type = typeof(Decorator))]
    public class ComboView : ComboBox
    {
        #region Fields
        Decorator itemsViewPresenter;
        Decorator itemsViewPlaceholder;

        bool boundToCollection;
        bool boundToValuesCollection;
        bool overrideDropDownClose;
        bool propertyChangeInternal;
        bool hasLoaded;
        #endregion

        #region Properties
        ItemsControl itemsView;
        public ItemsControl ItemsView
        {
            get { return itemsView; }
            private set { itemsView = value; }
        }
        #endregion

        #region Dependency Properties
        public DataTemplate ItemsViewTemplate
        {
            get { return (DataTemplate)GetValue(ItemsViewTemplateProperty); }
            set { SetValue(ItemsViewTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemsViewTemplateProperty =
            DependencyProperty.Register("ItemsViewTemplate", typeof(DataTemplate), typeof(ComboView));

        public DataTemplate SelectedItemTemplate
        {
            get { return (DataTemplate)GetValue(SelectedItemTemplateProperty); }
            set { SetValue(SelectedItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemTemplateProperty =
            DependencyProperty.Register("SelectedItemTemplate", typeof(DataTemplate), typeof(ComboView));

        public string SelectionDisplayMemberPath
        {
            get { return (string)GetValue(SelectionDisplayMemberPathProperty); }
            set { SetValue(SelectionDisplayMemberPathProperty, value); }
        }
        public static readonly DependencyProperty SelectionDisplayMemberPathProperty =
            DependencyProperty.Register("SelectionDisplayMemberPath", typeof(string), typeof(ComboView));

        public string SelectionDisplayDelimiter
        {
            get { return (string)GetValue(SelectionDisplayDelimiterProperty); }
            set { SetValue(SelectionDisplayDelimiterProperty, value); }
        }
        public static readonly DependencyProperty SelectionDisplayDelimiterProperty =
            DependencyProperty.Register("SelectionDisplayDelimiter", typeof(string), typeof(ComboView));

        public string SelectionDisplay
        {
            get { return (string)GetValue(SelectionDisplayProperty); }
            set { SetValue(SelectionDisplayProperty, value); }
        }
        public static readonly DependencyProperty SelectionDisplayProperty =
            DependencyProperty.Register("SelectionDisplay", typeof(string), typeof(ComboView));

        public object SelectionHeader
        {
            get { return (object)GetValue(SelectionHeaderProperty); }
            set { SetValue(SelectionHeaderProperty, value); }
        }
        public static readonly DependencyProperty SelectionHeaderProperty =
            DependencyProperty.Register("SelectionHeader", typeof(object), typeof(ComboView));

        public Style SelectionContentStyle
        {
            get { return (Style)GetValue(SelectionContentStyleProperty); }
            set { SetValue(SelectionContentStyleProperty, value); }
        }
        public static readonly DependencyProperty SelectionContentStyleProperty =
            DependencyProperty.Register("SelectionContentStyle", typeof(Style), typeof(ComboView));

        public object SelectedViewItem
        {
            get { return (object)GetValue(SelectedViewItemProperty); }
            set { SetValue(SelectedViewItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedViewItemProperty =
            DependencyProperty.Register("SelectedViewItem", typeof(object), typeof(ComboView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public IList SelectedViewItems
        {
            get { return (IList)GetValue(SelectedViewItemsProperty); }
            set { SetValue(SelectedViewItemsProperty, value); }
        }
        public static readonly DependencyProperty SelectedViewItemsProperty =
            DependencyProperty.Register("SelectedViewItems", typeof(IList), typeof(ComboView), new FrameworkPropertyMetadata(OnSelectedViewItemsChanged, CoerceSelectedViewItems));

        private static object CoerceSelectedViewItems(DependencyObject o, object value)
        {
            return value ?? new ObservableCollection<object>();
        }

        public object SelectedViewValue
        {
            get { return (object)GetValue(SelectedViewValueProperty); }
            set { SetValue(SelectedViewValueProperty, value); }
        }
        public static readonly DependencyProperty SelectedViewValueProperty =
            DependencyProperty.Register("SelectedViewValue", typeof(object), typeof(ComboView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

        public IList SelectedViewValues
        {
            get { return (IList)GetValue(SelectedViewValuesProperty); }
            set { SetValue(SelectedViewValuesProperty, value); }
        }
        public static readonly DependencyProperty SelectedViewValuesProperty =
            DependencyProperty.Register("SelectedViewValues", typeof(IList), typeof(ComboView), new FrameworkPropertyMetadata(OnSelectedViewValuesChanged, CoerceSelectedViewValues));

        private static object CoerceSelectedViewValues(DependencyObject o, object value)
        {
            return value ?? new ObservableCollection<object>();
        }

        public bool SyncSelectedViewValues
        {
            get { return (bool)GetValue(SyncSelectedViewValuesProperty); }
            set { SetValue(SyncSelectedViewValuesProperty, value); }
        }
        public static readonly DependencyProperty SyncSelectedViewValuesProperty =
            DependencyProperty.Register("SyncSelectedViewValues", typeof(bool), typeof(ComboView));

        public bool CloseDropDownUponSelection
        {
            get { return (bool)GetValue(CloseDropDownUponSelectionProperty); }
            set { SetValue(CloseDropDownUponSelectionProperty, value); }
        }
        public static readonly DependencyProperty CloseDropDownUponSelectionProperty =
            DependencyProperty.Register("CloseDropDownUponSelection", typeof(bool), typeof(ComboView), new FrameworkPropertyMetadata(false));

        public Style PopupStyle
        {
            get { return (Style)GetValue(PopupStyleProperty); }
            set { SetValue(PopupStyleProperty, value); }
        }
        public static readonly DependencyProperty PopupStyleProperty =
            DependencyProperty.Register("PopupStyle", typeof(Style), typeof(ComboView));

        public IComboItemsViewHandler ItemsViewHandler
        {
            get { return (IComboItemsViewHandler)GetValue(ItemsViewHandlerProperty); }
            set { SetValue(ItemsViewHandlerProperty, value); }
        }
        public static readonly DependencyProperty ItemsViewHandlerProperty =
            DependencyProperty.Register("ItemsViewHandler", typeof(IComboItemsViewHandler), typeof(ComboView));

        public PopupAnimation PopupAnimation
        {
            get { return (PopupAnimation)GetValue(PopupAnimationProperty); }
            set { SetValue(PopupAnimationProperty, value); }
        }
        public static readonly DependencyProperty PopupAnimationProperty =
            DependencyProperty.Register("PopupAnimation", typeof(PopupAnimation), typeof(ComboView), new UIPropertyMetadata(SystemParameters.ComboBoxPopupAnimation));

        public bool IsPopupResizeEnabled
        {
            get { return (bool)GetValue(IsPopupResizeEnabledProperty); }
            set { SetValue(IsPopupResizeEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsPopupResizeEnabledProperty =
            DependencyProperty.Register("IsPopupResizeEnabled", typeof(bool), typeof(ComboView), new UIPropertyMetadata(true));

        public bool ShowToolTip
        {
            get { return (bool)GetValue(ShowToolTipProperty); }
            set { SetValue(ShowToolTipProperty, value); }
        }
        public static readonly DependencyProperty ShowToolTipProperty =
            DependencyProperty.Register("ShowToolTip", typeof(bool), typeof(ComboView), new UIPropertyMetadata(true));

        public ComboViewSelectionMode SelectionMode
        {
            get { return (ComboViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register("SelectionMode", typeof(ComboViewSelectionMode), typeof(ComboView), new UIPropertyMetadata(ComboViewSelectionMode.Normal));

        public bool ShowGlobalSelection
        {
            get { return (bool)GetValue(ShowGlobalSelectionProperty); }
            set { SetValue(ShowGlobalSelectionProperty, value); }
        }
        public static readonly DependencyProperty ShowGlobalSelectionProperty =
            DependencyProperty.Register("ShowGlobalSelection", typeof(bool), typeof(ComboView));

        public InitialSelectionMode InitialSelection
        {
            get { return (InitialSelectionMode)GetValue(InitialSelectionProperty); }
            set { SetValue(InitialSelectionProperty, value); }
        }
        public static readonly DependencyProperty InitialSelectionProperty =
            DependencyProperty.Register("InitialSelection", typeof(InitialSelectionMode), typeof(ComboView), new UIPropertyMetadata(InitialSelectionMode.None));

        public bool SelectionInvokersFocusable
        {
            get { return (bool)GetValue(SelectionInvokersFocusableProperty); }
            set { SetValue(SelectionInvokersFocusableProperty, value); }
        }
        public static readonly DependencyProperty SelectionInvokersFocusableProperty =
            DependencyProperty.Register("SelectionInvokersFocusable", typeof(bool), typeof(ComboView), new UIPropertyMetadata(true));

        public static readonly DependencyProperty UninitializeCollectionsProperty =
            DependencyProperty.Register("UninitializeCollections", typeof(bool), typeof(ComboView), new FrameworkPropertyMetadata(false, OnUninitializeCollectionsChanged));

        private static void OnUninitializeCollectionsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var comboView = (ComboView)o;
                comboView.overlookSelectedItemsChanged = true;
                comboView.overlookSelectedValuesChanged = true;
                o.ClearValue(SelectedViewItemsProperty);
                o.ClearValue(SelectedViewValuesProperty);
                comboView.overlookSelectedValuesChanged = false;
                comboView.overlookSelectedItemsChanged = false;
            }
        }
                
        #endregion

        #region Routed Events
        #region ViewSelectionChanged
        public static readonly RoutedEvent ViewSelectionChangedEvent
            = EventManager.RegisterRoutedEvent("ViewSelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ComboView));

        public event RoutedEventHandler ViewSelectionChanged
        {
            add { AddHandler(ViewSelectionChangedEvent, value); }
            remove { RemoveHandler(ViewSelectionChangedEvent, value); }
        }

        protected void OnViewSelectionChanged()
        {
            RoutedEventArgs args = new RoutedEventArgs(ViewSelectionChangedEvent, this);

            RaiseEvent(args);
        }
        #endregion
        #endregion

        #region Commands

        #region ApplySelection
        public static RoutedCommand ApplySelectionCommand = new RoutedCommand("ApplySelection", typeof(ComboView));

        static void ExecuteApplySelection(object sender, ExecutedRoutedEventArgs e)
        {
            ComboView combo = sender as ComboView;

            if (combo != null)
            {
                combo.EndSelectionIteration(true);
            }
        }

        static void CanExecuteApplySelection(object sender, CanExecuteRoutedEventArgs e)
        {
            ComboView combo = sender as ComboView;

            if (combo != null)
            {
                e.CanExecute = combo.IsDropDownOpen && combo.SelectionMode == ComboViewSelectionMode.Confirm;
            }
        }
        #endregion

        #region CancelSelection
        public static RoutedCommand CancelSelectionCommand = new RoutedCommand("CancelSelection", typeof(ComboView));

        static void ExecuteCancelSelection(object sender, ExecutedRoutedEventArgs e)
        {
            ComboView combo = sender as ComboView;

            if (combo != null)
            {
                combo.EndSelectionIteration(false);
            }
        }

        static void CanExecuteCancelSelection(object sender, CanExecuteRoutedEventArgs e)
        {
            ComboView combo = sender as ComboView;

            if (combo != null)
            {
                e.CanExecute = combo.IsDropDownOpen && combo.SelectionMode == ComboViewSelectionMode.Confirm;
            }
        }
        #endregion

        #region GlobalSelection
        public static RoutedCommand GlobalSelectionCommand = new RoutedCommand("GlobalSelection", typeof(ComboView));

        static void ExecuteGlobalSelection(object sender, ExecutedRoutedEventArgs e)
        {
            ComboView combo = sender as ComboView;

            bool select = true;
            if (e.Parameter != null)
            {
                select = bool.Parse(e.Parameter.ToString());
            }

            combo.SelectGlobal(select);
        }

        static void CanExecuteGlobalSelection(object sender, CanExecuteRoutedEventArgs e)
        {
            ComboView combo = sender as ComboView;

            if (combo != null)
            {
                e.CanExecute = combo.IsDropDownOpen;
            }
        }
        #endregion

        #endregion

        #region Ctors
        static ComboView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboView), new FrameworkPropertyMetadata(typeof(ComboView)));

            CommandManager.RegisterClassCommandBinding(typeof(ComboView),
                new CommandBinding(ApplySelectionCommand, ExecuteApplySelection, CanExecuteApplySelection));

            CommandManager.RegisterClassCommandBinding(typeof(ComboView),
                new CommandBinding(CancelSelectionCommand, ExecuteCancelSelection, CanExecuteCancelSelection));

            CommandManager.RegisterClassCommandBinding(typeof(ComboView),
                new CommandBinding(GlobalSelectionCommand, ExecuteGlobalSelection, CanExecuteGlobalSelection));
        }

        public ComboView()
        {
            overlookSelectedItemsChanged = true;

            CoerceValue(SelectedViewItemsProperty);
            CoerceValue(SelectedViewValuesProperty);

            overlookSelectedItemsChanged = false;
            Loaded += ComboView_Loaded;
        }
        #endregion

        #region Load
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ItemsViewTemplate == null)
            {
                throw new ArgumentNullException("ItemsViewTemplate");
            }

            itemsViewPresenter = GetTemplateChild("PART_ItemsViewPresenter") as Decorator;
            if (itemsViewPresenter == null)
            {
                throw new ArgumentNullException("PART_ItemsViewPresenter");
            }

            itemsViewPlaceholder = GetTemplateChild("PART_ItemsViewPlaceholder") as Decorator;
            if (itemsViewPresenter == null)
            {
                throw new ArgumentNullException("PART_ItemsViewPlaceholder");
            }

            ItemsView = ItemsViewTemplate.LoadContent() as ItemsControl;

            if (ItemsView == null)
            {
                throw new ArgumentNullException("ItemsView");
            }

            if (ItemsViewHandler != null)
            {
                ItemsViewHandler.UnloadView();
            }

            ItemsViewHandler = ComboItemsViewHandlersFactory.GetHandler(this, ItemsView);

            OnItemsViewLoaded();

            templateApplied = true;

            if (hasLoaded)
            {
                LoadPendingSelectedItems();
            }
        }

        bool templateApplied;

        void OnItemsViewLoaded()
        {
            itemsViewPlaceholder.Child = ItemsView;

            ItemsViewHandler.InitializeView(this, ItemsView);
        }

        void ComboView_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ComboView_Loaded;
            hasLoaded = true;

            if (templateApplied)
            {
                LoadPendingSelectedItems();
            }
        }

        #endregion

        #region Drop Down Show
        protected override void OnDropDownOpened(EventArgs e)
        {
            itemsViewPlaceholder.Child = null;
            itemsViewPlaceholder.Visibility = Visibility.Collapsed;
            itemsViewPresenter.Child = ItemsView;
            
            ItemsViewHandler.OnDropDownOpened();

            base.OnDropDownOpened(e);

            if (SelectionMode == ComboViewSelectionMode.Confirm)
            {
                BeginSelectionIteration();
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ItemsView.Focus();
            }));
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            ItemsViewHandler.OnDropDownClosed();

            if (SelectionMode == ComboViewSelectionMode.Confirm
                && !overrideEndingIteration)
            {
                EndSelectionIteration(false);
            }

            overrideEndingIteration = false;

            itemsViewPlaceholder.Visibility = Visibility.Visible;
            itemsViewPresenter.Child = null;
            itemsViewPlaceholder.Child = ItemsView;
        }
        #endregion

        #region Container Items
        protected override DependencyObject GetContainerForItemOverride()
        {
            ComboViewItem container = new ComboViewItem(this);

            return container;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ComboViewItem;
        }
        #endregion

        #region Property State Changes

        private int selectedItemLock;

        private static void OnSelectedItemChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ComboView comboView = (ComboView)source;
            if (comboView.selectedItemLock == 0)
            {
                ++comboView.selectedItemLock;
                comboView.SetSelectedItems(e.NewValue);
                --comboView.selectedItemLock;
            }
        }

        private int selectedValueLock;

        private static void OnSelectedValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ComboView comboView = (ComboView)source;
            if (comboView.selectedValueLock == 0)
            {
                ++comboView.selectedValueLock;
                comboView.SetSelectedValues(e.NewValue);
                --comboView.selectedValueLock;
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            bool hasItems = (SelectedViewItems != null && SelectedViewItems.Count > 0);

            bool selectGlobal = ((InitialSelection == InitialSelectionMode.All)
                || (InitialSelection == InitialSelectionMode.AllOneTime
                    && !hasItems
                    && (pendingSetSelectedItems == null || pendingSetSelectedItems.Count == 0)
                    && (pendingSetSelectedValues == null || pendingSetSelectedValues.Count == 0)));

            if (ItemsViewHandler != null)
            {
                ItemsViewHandler.OnItemsSourceChangedPreview(oldValue, newValue, selectGlobal);
            }

            base.OnItemsSourceChanged(oldValue, newValue);

            if (selectGlobal)
            {
                if (hasItems && (newValue == null || newValue.OfType<object>().Count() == 0))
                {
                    ResetSelectedValueItems(new ObservableCollection<object>(), false);
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(() => SelectGlobal(true)),
                        DispatcherPriority.Loaded);
                    //SelectGlobal(true);
                }
            }
            else if (ItemsViewHandler != null)
            {
                ItemsViewHandler.OnItemsSourceChanged(oldValue, newValue);
            }
        }

        static void OnSelectedViewItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ComboView combo = o as ComboView;

            bool hasNewItems = (e.NewValue != null);

            if (combo.boundToCollection)
            {
                INotifyCollectionChanged col = e.OldValue as INotifyCollectionChanged;

                col.CollectionChanged -= combo.OnItemsCollectionChanged;
                combo.boundToCollection = false;
            }

            if (hasNewItems)
            {
                INotifyCollectionChanged col = e.NewValue as INotifyCollectionChanged;
                combo.boundToCollection = col != null;

                if (combo.boundToCollection)
                {
                    col.CollectionChanged += combo.OnItemsCollectionChanged;
                }
            }

            if (!combo.overlookSelectedItemsChanged && !DependencyPropertyHelper.GetValueSource(o, SelectedViewItemsProperty).IsCoerced)
            {
                if (combo.propertyChangeInternal)
                {
                    combo.OnSetSelection(combo.SelectedViewItems, e.OldValue as IList, combo.resetSelectedValues);
                }
                else
                {
                    combo.ResetItemsSelection(false);
                }
            }
        }

        void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!overlookSelectedItemsChanged)
            {
                if (ItemsViewHandler != null)
                {
                    ResetItemsSelection(false);
                    //OnSetSelection(e.NewItems, e.OldItems);
                }
                else
                {
                    ResetItemsSelection(false);
                    //if (pendingSetSelectedItems == null)
                    //{
                    //    SetSelectedItems(SelectedViewItems);
                    //}
                }
            }
        }

        static void OnSelectedViewValuesChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ComboView combo = o as ComboView;

            if (combo.boundToValuesCollection)
            {
                INotifyCollectionChanged col = e.OldValue as INotifyCollectionChanged;

                col.CollectionChanged -= combo.OnValuesCollectionChanged;
                combo.boundToValuesCollection = false;
            }

            if (e.NewValue != null)
            {
                INotifyCollectionChanged col = e.NewValue as INotifyCollectionChanged;
                combo.boundToValuesCollection = col != null;

                if (combo.boundToValuesCollection)
                {
                    col.CollectionChanged += combo.OnValuesCollectionChanged;
                }
            }

            if (!combo.overlookSelectedItemsChanged && !combo.overlookSelectedValuesChanged
                 && !DependencyPropertyHelper.GetValueSource(o, SelectedViewItemsProperty).IsCoerced)
            {
                combo.ResetItemsSelection(true);
                //combo.SetSelectedValues(combo.SelectedViewValues);
            }
        }

        void OnValuesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!overlookSelectedValuesChanged)
            {
                ResetItemsSelection(true);
                //SetSelectedValues(SelectedViewValues);
            }
        }
        #endregion

        #region Selection
        List<IList> pendingSetSelectedItems = null;
        List<IList> pendingSetSelectedValues = null;
        bool resetSelectedValues;
        bool overlookSelectedItemsChanged;
        bool overlookSelectedValuesChanged;

        internal void ResetSelectedValueItems(IList newItems)
        {
            ResetSelectedValueItems(newItems, true);
        }

        internal void ResetSelectedValueItems(IList newItems, bool setInternal)
        {
            resetSelectedValues = true;

            propertyChangeInternal = setInternal;
            SelectedViewItems = newItems;
            propertyChangeInternal = false;

            resetSelectedValues = false;
        }

        void LoadPendingSelectedItems()
        {
            if (pendingSetSelectedItems != null && pendingSetSelectedItems.Count > 0)
            {
                //handle only last
                SetSelectedItems(pendingSetSelectedItems[pendingSetSelectedItems.Count - 1]);
                //foreach (var item in pendingSetSelectedItems)
                //{
                //    SetSelectedItems(item);
                //}

                pendingSetSelectedItems.Clear();
            }
            else if (pendingSetSelectedValues != null && pendingSetSelectedValues.Count > 0)
            {
                SetSelectedValues(pendingSetSelectedValues[pendingSetSelectedValues.Count - 1]);

                pendingSetSelectedValues.Clear();
            }
        }

        public void SetSelectedItems(params object[] items)
        {
            SetSelectedItems((IList)items);
        }

        public void SetSelectedItems(IList items)
        {
            if (ItemsViewHandler == null)
            {
                if (pendingSetSelectedItems == null)
                {
                    pendingSetSelectedItems = new List<IList>();
                }

                pendingSetSelectedItems.Add(items);
            }
            else if (items != null)
            {
                ItemsViewHandler.SetSelectedItems(items);
            }
        }

        public void SetSelectedValues(params object[] values)
        {
            SetSelectedValues((IList)values);
        }

        public void SetSelectedValues(IList values)
        {
            if (ItemsViewHandler == null)
            {
                if (pendingSetSelectedValues == null)
                {
                    pendingSetSelectedValues = new List<IList>();
                }

                pendingSetSelectedValues.Add(values);
            }
            else if (values != null)
            {
                ItemsViewHandler.SetSelectedValues(values);
            }
        }

        public IEnumerable GetSelectedValues()
        {
            if (SyncSelectedViewValues)
            {
                return SelectedViewValues;
            }

            return GetSelectedValuesInternal();
        }

        IEnumerable GetSelectedValuesInternal()
        {
            if (SelectedViewItems != null && SelectedViewItems.Count > 0)
            {
                bool hasPath = !string.IsNullOrEmpty(SelectedValuePath);

                foreach (var item in SelectedViewItems)
                {
                    object v = item;

                    if (hasPath)
                    {
                        v = BindingHelper.Eval<object>(item, SelectedValuePath);
                    }

                    if (v != null)
                    {
                        yield return v;
                    }
                }
            }
            else
            {
                yield break;
            }
        }

        internal void SetSelection(params object[] selectedItems)
        {
            SetSelection((IList)selectedItems);
        }

        internal void SetSelection(bool keepDropDownOpen, params object[] selectedItems)
        {
            SetSelection(keepDropDownOpen, (IList)selectedItems);
        }

        internal void SetSelection(IList selectedItems)
        {
            SetSelection(false, selectedItems);
        }

        internal void SetSelection(bool keepDropDownOpen, IList selectedItems)
        {
            overrideDropDownClose = keepDropDownOpen;

            propertyChangeInternal = true;

            if (selectedItems != null)
            {
                Type t = selectedItems.GetType();
                if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(ObservableCollection<>)))
                {
                    SelectedViewItems = selectedItems;
                }
                else
                {
                    SelectedViewItems = new ObservableCollection<object>(selectedItems.OfType<object>());
                }
            }

            propertyChangeInternal = false;

            overrideDropDownClose = false;
        }

        internal void SetSelection(IList addedItems, IList removedItems)
        {
            SetSelection(false, addedItems, removedItems);
        }

        internal void SetSelection(bool keepDropDownOpen, IList addedItems, IList removedItems)
        {
            if (removedItems == null)
            {
                removedItems = new List<object>();
            }
            if (addedItems == null)
            {
                addedItems = new List<object>();
            }

            if (addedItems.Count > 0 || removedItems.Count > 0)
            {
                overrideDropDownClose = keepDropDownOpen;

                overlookSelectedItemsChanged = true;
                propertyChangeInternal = true;

                foreach (var o in removedItems)
                {
                    SelectedViewItems.Remove(o);
                }
                foreach (var o in addedItems)
                {
                    SelectedViewItems.Add(o);
                }
                OnSetSelection(addedItems, removedItems);

                propertyChangeInternal = false;
                overlookSelectedItemsChanged = false;

                overrideDropDownClose = false;
            }
        }

        public void SelectGlobal(bool select)
        {
            if (select)
            {
                if (ItemsSource != null)
                {
                    IList source = ItemsSource.OfType<object>().ToList();

                    if (source.Count > 0)
                    {
                        SetSelectedItems(source);
                    }
                }
            }
            else
            {
                if (IsDropDownOpen && SelectionMode == ComboViewSelectionMode.Confirm)
                {
                    SetSelectedItems(new object());
                }
                else if (SelectedViewItems != null && SelectedViewItems.Count > 0)
                {
                    SelectedViewItems = new ObservableCollection<object>();
                }
            }
        }

        void ResetItemsSelection(bool isValue)
        {
            if (!isValue)
            {
                SetSelectedItems(DuplicateResetList(SelectedViewItems));
            }
            else if (SyncSelectedViewValues)
            {
                SetSelectedValues(DuplicateResetList(SelectedViewValues));
            }
            else
            {
                SetSelectedValues(SelectedViewValues);
            }
        }

        IList DuplicateResetList(IList source)
        {
            IList dup = source;
            if (source != null && source.Count > 0)
            {
                overlookSelectedItemsChanged = true;
                dup = new List<object>(source.OfType<object>());

                source.Clear();

                overlookSelectedItemsChanged = false;
            }

            return dup;
        }

        void OnSetSelection(IList addedItems, IList removedItems)
        {
            OnSetSelection(addedItems, removedItems, false);
        }

        void OnSetSelection(IList addedItems, IList removedItems, bool itemsReset)
        {
            if (removedItems == null)
            {
                removedItems = new List<object>();
            }
            if (addedItems == null)
            {
                addedItems = new List<object>();
            }

            ++selectedItemLock;
            SelectedViewItem = (addedItems.Count > 0) ? addedItems[0] : null;
            --selectedItemLock;

            SetSelectionDisplay(itemsReset);

            if (!propertyChangeInternal
                && ItemsViewHandler.IsSelectionSubscribed)
            {
                ItemsViewHandler.SetSelectedItems(SelectedViewItems);
            }

            if (/*!selectedValuesSetInternal &&*/ SyncSelectedViewValues)
            {
                overlookSelectedItemsChanged = true;
                overlookSelectedValuesChanged = true;

                var selectedViewValues = new ObservableCollection<object>(GetSelectedValuesInternal().OfType<object>());
                ++selectedValueLock;
                SelectedViewValue = (selectedViewValues.Count > 0) ? selectedViewValues[0] : null;
                --selectedValueLock;
                SelectedViewValues = selectedViewValues;

                //IEnumerable<object> values = GetSelectedValuesInternal().OfType<object>();
                //if (SelectedViewValues == null)
                //{
                //    SelectedViewValues = new ObservableCollection<object>(values);
                //}
                //else
                //{
                //    SelectedViewValues.Clear();

                //    foreach (var value in values)
                //    {
                //        SelectedViewValues.Add(value);
                //    }
                //}

                overlookSelectedItemsChanged = false;
                overlookSelectedValuesChanged = false;
            }

            if (!overrideDropDownClose && CloseDropDownUponSelection && IsDropDownOpen)
            {
                Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        IsDropDownOpen = false;

                        RaiseSelectionChangedEvents(itemsReset, addedItems, removedItems);
                    }));

                return;
            }

            RaiseSelectionChangedEvents(itemsReset, addedItems, removedItems);
        }

        void RaiseSelectionChangedEvents(bool itemsReset, IList addedItems, IList removedItems)
        {
            if (!itemsReset)
            {
                OnViewSelectionChanged();
                OnSelectionChanged(new SelectionChangedEventArgs(ComboView.SelectionChangedEvent, removedItems, addedItems));
            }
        }

        void SetSelectionDisplay(bool itemsReset)
        {
            if (!itemsReset && SelectedViewItems != null && SelectedViewItems.Count > 0)
            {

                StringBuilder sb = new StringBuilder();

                string memberPath = null;
                bool hasPath = false;

                if (!string.IsNullOrEmpty(SelectionDisplayMemberPath))
                {
                    memberPath = SelectionDisplayMemberPath;
                    hasPath = true;
                }
                else if (!string.IsNullOrEmpty(SelectedValuePath))
                {
                    memberPath = SelectedValuePath;
                    hasPath = true;
                }

                bool firstItem = true;
                foreach (var item in SelectedViewItems)
                {
                    string itemDisplay = string.Empty;

                    if (hasPath)
                    {
                        object itemDisplayObject = BindingHelper.Eval<object>(item, memberPath);
                        if (itemDisplayObject != null)
                        {
                            itemDisplay = itemDisplayObject.ToString();
                        }
                    }
                    else
                    {
                        itemDisplay = item.ToString();
                    }

                    if (!firstItem)
                    {
                        sb.Append(SelectionDisplayDelimiter);
                    }
                    sb.Append(itemDisplay);

                    firstItem = false;
                }

                SelectionDisplay = sb.ToString();
            }
            else
            {
                SelectionDisplay = string.Empty;
            }

            if (ShowToolTip)
            {
                if (string.IsNullOrEmpty(SelectionDisplay))
                {
                    ToolTip = DependencyProperty.UnsetValue;
                }
                else
                {
                    ToolTip = SelectionDisplay;
                }
            }

        }
        #endregion

        #region Selection Iteration
        bool overrideEndingIteration;

        void BeginSelectionIteration()
        {
            ItemsViewHandler.BeginSelectionIteration();
        }

        void EndSelectionIteration(bool apply)
        {
            ItemsViewHandler.EndSelectionIteration(apply);

            if (IsDropDownOpen)
            {
                if (CloseDropDownUponSelection)
                {
                    overrideEndingIteration = true;

                    IsDropDownOpen = false;
                }
                else
                {
                    BeginSelectionIteration();
                }
            }
        }
        #endregion
    }

    
}
