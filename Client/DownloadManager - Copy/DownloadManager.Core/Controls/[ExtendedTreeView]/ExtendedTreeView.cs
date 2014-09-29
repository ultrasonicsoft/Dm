using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Diagnostics;
using Ultrasonic.DownloadManager.Core.Utils;

namespace Ultrasonic.DownloadManager.Controls
{
    /// <summary>
    /// Known Issues -
    /// 1.  if check behavior toggles children there is a problem when children whose container hadn't been created
    ///     would have ShowCheckBox = false later on - They are still included in the ItemCheckChanged
    /// </summary>
    public class ExtendedTreeView : TreeView
    {
        #region Properties
        internal bool HasItemsWithCheckBox { get; set; }
        internal bool CalcCheckedOnSourceChange { get; set; }
        #endregion

        #region Dependency Properties
        public bool EnableCheckBoxView
        {
            get { return (bool)GetValue(EnableCheckBoxViewProperty); }
            set { SetValue(EnableCheckBoxViewProperty, value); }
        }
        public static readonly DependencyProperty EnableCheckBoxViewProperty =
            DependencyProperty.Register("EnableCheckBoxView", typeof(bool), typeof(ExtendedTreeView), new FrameworkPropertyMetadata(false));

        public ItemCheckBehavior CheckBehavior
        {
            get { return (ItemCheckBehavior)GetValue(CheckBehaviorProperty); }
            set { SetValue(CheckBehaviorProperty, value); }
        }
        public static readonly DependencyProperty CheckBehaviorProperty =
            DependencyProperty.RegisterAttached("CheckBehavior", typeof(ItemCheckBehavior), typeof(ExtendedTreeView), new FrameworkPropertyMetadata(ItemCheckBehavior.User, FrameworkPropertyMetadataOptions.Inherits));

        public ItemTypeCheckRecordMode ItemTypeCheckRecord
        {
            get { return (ItemTypeCheckRecordMode)GetValue(ItemTypeCheckRecordProperty); }
            set { SetValue(ItemTypeCheckRecordProperty, value); }
        }
        public static readonly DependencyProperty ItemTypeCheckRecordProperty =
            DependencyProperty.Register("ItemTypeCheckRecord", typeof(ItemTypeCheckRecordMode), typeof(ExtendedTreeView), new FrameworkPropertyMetadata(ItemTypeCheckRecordMode.All, FrameworkPropertyMetadataOptions.Inherits));

        public IList CheckedItems
        {
            get { return (IList)GetValue(CheckedItemsProperty); }
            internal set { SetValue(CheckedItemsPropertyKey, value); }
        }
        internal static readonly DependencyPropertyKey CheckedItemsPropertyKey =
            DependencyProperty.RegisterReadOnly("CheckedItems", typeof(IList), typeof(ExtendedTreeView), new PropertyMetadata());
        public static readonly DependencyProperty CheckedItemsProperty = CheckedItemsPropertyKey.DependencyProperty;

        public bool IsExpanderVisible
        {
            get { return (bool)GetValue(IsExpanderVisibleProperty); }
            set { SetValue(IsExpanderVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsExpanderVisibleProperty =
            DependencyProperty.Register("IsExpanderVisible", typeof(bool), typeof(ExtendedTreeView), new UIPropertyMetadata(true));

        public bool ExpandParentsOnItemChecked
        {
            get { return (bool)GetValue(ExpandParentsOnItemCheckedProperty); }
            set { SetValue(ExpandParentsOnItemCheckedProperty, value); }
        }
        public static readonly DependencyProperty ExpandParentsOnItemCheckedProperty =
            DependencyProperty.Register("ExpandParentsOnItemChecked", typeof(bool), typeof(ExtendedTreeView), new UIPropertyMetadata(OnExpandParentOnItemCheckedChanged));
        static void OnExpandParentOnItemCheckedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ExtendedTreeView tv = (ExtendedTreeView)o;

                if (tv.EnableCheckBoxView)
                {
                    tv.AddHandler(ExtendedTreeViewItem.CheckedEvent,
                        new RoutedEventHandler(tv.OnItemChecked), false);
                }
            }
        }
        #endregion

        #region Routed Events
        #region ItemCheckChanged
        public static readonly RoutedEvent ItemCheckChangedEvent
            = EventManager.RegisterRoutedEvent("ItemCheckChanged", RoutingStrategy.Bubble, typeof(EventHandler<ItemCheckChangedEventArgs>), typeof(ComboView));

        public event EventHandler<ItemCheckChangedEventArgs> ItemCheckChanged
        {
            add { AddHandler(ItemCheckChangedEvent, value); }
            remove { RemoveHandler(ItemCheckChangedEvent, value); }
        }

        protected void OnItemCheckChanged(ItemCheckChangedEventArgs args)
        {
            RaiseEvent(args);

        }

        protected ItemCheckChangedEventArgs GetItemCheckChangedArgs()
        {
            return new ItemCheckChangedEventArgs(ExtendedTreeView.ItemCheckChangedEvent, this);
        }

        protected ItemCheckChangedEventArgs GetItemCheckChangedArgs(IList<object> addedItems, IList<object> removedItems)
        {
            ItemCheckChangedEventArgs args = GetItemCheckChangedArgs();

            args.AddedItems = addedItems;
            args.RemovedItems = removedItems;

            return args;
        }

        protected ItemCheckChangedEventArgs GetAddedItemCheckChangedArgs(params object[] addedItems)
        {
            ItemCheckChangedEventArgs args = GetItemCheckChangedArgs();

            args.AddedItems = new List<object>(addedItems);
            args.RemovedItems = new List<object>();

            return args;
        }

        protected ItemCheckChangedEventArgs GetRemovedItemCheckChangedArgs(params object[] removedItems)
        {
            ItemCheckChangedEventArgs args = GetItemCheckChangedArgs();

            args.RemovedItems = new List<object>(removedItems);
            args.AddedItems = new List<object>();

            return args;
        }
        #endregion
        #endregion

        #region Initialization
        static ExtendedTreeView()
        {
            EventManager.RegisterClassHandler(typeof(ExtendedTreeViewItem),
                ExtendedTreeViewItem.SelectedEvent, new RoutedEventHandler(OnItemSelected), true);
        }

        public ExtendedTreeView()
        {
            CheckedItems = new ObservableCollection<object>();

            CalcCheckedOnSourceChange = true;
        }
        #endregion

        #region Overrides
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (lastSelectedItem != null)
            {
                lastSelectedItem.Focus();
                lastSelectedItem.BringIntoView();
            }
            else if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated && Items.Count > 0)
            {
                ExtendedTreeViewItem container = ItemContainerGenerator.ContainerFromIndex(0) as ExtendedTreeViewItem;

                if (container != null)
                {
                    SelectionInputSource = ItemSelectionInputSource.InitialFocus;
                    ignoreSelection = true;
                    container.Focus();
                    container.BringIntoView();
                    ignoreSelection = false;
                }
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (EnableCheckBoxView)
            {
                if (CheckedItems != null && CheckedItems.Count > 0)
                {
                    itemsSourceChanged = true;

                    SetCheckedItems(CheckedItems);
                }
            }
        }
        #endregion

        #region Selection
        bool ignoreSelection;
        internal bool IgnoreNextSelectionChange { get; set; }

        internal void ReselectItem(ExtendedTreeViewItem container)
        {
            ignoreSelection = true;
            container.IsSelected = false;
            ignoreSelection = false;

            container.IsSelected = true;
        }

        private ExtendedTreeViewItem lastSelectedItem;

        private ItemSelectionInputSource selectionInputSource = ItemSelectionInputSource.None;
        internal ItemSelectionInputSource SelectionInputSource
        {
            get { return selectionInputSource; }
            set { selectionInputSource = value; }
        }

        static void OnItemSelected(object sender, RoutedEventArgs e)
        {
            ExtendedTreeViewItem item = e.OriginalSource as ExtendedTreeViewItem;

            if (item != null && !item.TreeViewParent.ignoreSelection)
            {
                item.TreeViewParent.lastSelectedItem = item;
            }
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            e.Handled = (ignoreSelection
                || (lastSelectedItem != null && !lastSelectedItem.CanBeSelected)
                || IgnoreNextSelectionChange);

            if (!e.Handled)
            {
                base.OnSelectedItemChanged(e);
            }

            lastSelectedItem = null;
            IgnoreNextSelectionChange = false;
            SelectionInputSource = ItemSelectionInputSource.None;
        }
        #endregion

        #region Container Items
        protected override DependencyObject GetContainerForItemOverride()
        {
            ExtendedTreeViewItem container = new ExtendedTreeViewItem(this);
            return container;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ExtendedTreeViewItem;
        }
        #endregion

        #region CheckBox Handling
        List<IList> pendingChecked = null;
        List<IList> pendingCheckedValues = null;
        bool itemsSourceChanged;

        internal int CheckProcessCounter { get; set; }

        public void SetCheckedItems(params object[] checkedItems)
        {
            SetCheckedItems((IList)checkedItems);
            
        }

        public void SetCheckedItems(IList checkedItems)
        {
            SetChecked(checkedItems, false);
        }

        public void SetCheckedValues(params object[] checkedValues)
        {
            SetCheckedValues((IList)checkedValues);
        }

        public void SetCheckedValues(IList checkedValues)
        {
            SetChecked(checkedValues, true);
        }

        private void SetChecked(IList items, bool isValue)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            else if (!EnableCheckBoxView)
            {
                throw new InvalidOperationException("Checkbox mode isn't enabled");
            }
            else if (ItemContainerGenerator == null)
            {
                throw new ArgumentException("ItemContainerGenerator is null");
            }

            if (Items.Count > 0)
            {
                bool containersCreated = (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated);
                bool loadPending = !containersCreated;
                
                //loadPending is unused
                if (!loadPending)
                {
                    loadPending = true;

                    foreach (var item in Items)
                    {
                        ExtendedTreeViewItem container = ItemContainerGenerator.ContainerFromItem(item) as ExtendedTreeViewItem;

                        if (container.Items.Count > 0 && container.ItemContainerGenerator.ContainerFromIndex(0) != null)
                        {
                            loadPending = false;
                            break;
                        }
                    }
                }

                if (containersCreated)
                {
                    SetCheckedItemsInternal(items, loadPending, isValue);
                }
                else
                {
                    ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;

                    if (!isValue)
                    {
                        if (pendingChecked == null)
                        {
                            pendingChecked = new List<IList>();
                        }

                        pendingChecked.Add(items);
                    }
                    else
                    {
                        if (pendingCheckedValues == null)
                        {
                            pendingCheckedValues = new List<IList>();
                        }

                        pendingCheckedValues.Add(items);
                    }
                    
                }
            }
        }

        void OnItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;

                if (pendingChecked != null && pendingChecked.Count > 0)
                {
                    //handle only the last
                    SetCheckedItemsInternal(pendingChecked[pendingChecked.Count - 1], true, false);
                    //foreach (var item in pendingChecked)
                    //{
                    //    SetCheckedItemsInternal(item, true);
                    //}

                    pendingChecked.Clear();
                }
                else if (pendingCheckedValues != null && pendingCheckedValues.Count > 0)
                {
                    //handle only the last
                    SetCheckedItemsInternal(pendingCheckedValues[pendingCheckedValues.Count - 1], true, true);

                    pendingCheckedValues.Clear();
                }
            }
        }

        void SetCheckedItemsInternal(IList checkedItems, bool considerPending, bool isValue)
        {
            Dictionary<object, bool> setSelectedItems = new Dictionary<object, bool>(checkedItems.Count);

            foreach (var item in checkedItems)
            {
                if (!setSelectedItems.ContainsKey(item))
                {
                    setSelectedItems.Add(item, true);
                }
            }

            ExtendedTreeViewHelper.SetCheckedItems(this, setSelectedItems, considerPending, isValue);
        }

        internal void OnChildrenCheckedChanged(IList<object> addedItems, IList<object> removedItems)
        {
            ObservableCollection<object> checkedItems = CheckedItems as ObservableCollection<object>;

            bool hasItems = addedItems.Count > 0 || removedItems.Count > 0;
            bool forceNotify = false;
            if (itemsSourceChanged && CheckedItems != null)
            {
                hasItems = CheckedItems.Count != addedItems.Count;
                forceNotify = hasItems;

                if (hasItems)
                {
                    removedItems = CheckedItems.OfType<object>().Except(addedItems).ToList();
                }

                if (hasItems && CalcCheckedOnSourceChange)
                {
                    CheckedItems.Clear();
                }
            }

            if (hasItems)
            {
                foreach (var item in removedItems)
                {
                    CheckedItems.Remove(item);
                }
                foreach (var item in addedItems)
                {
                    CheckedItems.Add(item);
                }
            }

            if ((hasItems && !itemsSourceChanged) || forceNotify)
            {
                OnItemCheckChanged(GetItemCheckChangedArgs(addedItems, removedItems));
            }

            itemsSourceChanged = false;
        }

        internal void SetChildrenShowCheckBoxSpace(bool value)
        {
            HasItemsWithCheckBox = ExtendedTreeViewHelper.SetChildrenShowCheckBoxSpace(this, value);
        }

        void OnItemChecked(object sender, RoutedEventArgs e)
        {
            var tvi = UIHelper.FindVisualAncestorByType<ExtendedTreeViewItem>(e.OriginalSource as DependencyObject);

            if (tvi != null)
            {
                tvi = tvi.ParentItem;
                while (tvi != null)
                {
                    tvi.IsExpanded = true;
                    tvi = tvi.ParentItem;
                }
            }
        }
        #endregion
    }
}
