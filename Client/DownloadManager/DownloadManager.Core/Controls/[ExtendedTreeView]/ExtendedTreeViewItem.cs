using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Ultrasonic.DownloadManager.Controls
{
    public class ExtendedTreeViewItem : TreeViewItem
    {
        #region Properties
        public ExtendedTreeViewItem ParentItem { get; set; }

        public ExtendedTreeView TreeViewParent { get; set; }

        public bool CanBeSelected
        {
            get { return AllowSelection; }
        }

        public bool IsRoot
        {
            get
            {
                return Level == 0;
            }
        }

        internal bool HasItemsWithCheckBox { get; set; }

        internal bool ShouldHandleIsChecked
        {
            get { return ShowCheckBox
                && (ItemTypeCheckRecord != ItemTypeCheckRecordMode.LeafOnly
                    || Items.Count == 0); }
        }

        internal bool? ParentIsChecked
        {
            get
            {
                if (ParentItem != null)
                {
                    return ParentItem.IsChecked;
                }

                return null;
            }
        }

        internal bool IsExpandedWhenMousedDown { get; private set; }
        #endregion

        #region Dependency Properties
        public bool AllowSelection
        {
            get { return (bool)GetValue(AllowSelectionProperty); }
            set { SetValue(AllowSelectionProperty, value); }
        }
        public static readonly DependencyProperty AllowSelectionProperty =
            DependencyProperty.Register("AllowSelection", typeof(bool), typeof(ExtendedTreeViewItem), new UIPropertyMetadata(true));

        public bool ShowCheckBox
        {
            get { return (bool)GetValue(ShowCheckBoxProperty); }
            set { SetValue(ShowCheckBoxProperty, value); }
        }
        public static readonly DependencyProperty ShowCheckBoxProperty =
            DependencyProperty.Register("ShowCheckBox", typeof(bool), typeof(ExtendedTreeViewItem),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnShowCheckBoxChanged)));

        public bool ShowCheckBoxSpace
        {
            get { return (bool)GetValue(ShowCheckBoxSpaceProperty); }
            internal set { SetValue(ShowCheckBoxSpacePropertyKey, value); }
        }
        private static readonly DependencyPropertyKey ShowCheckBoxSpacePropertyKey =
            DependencyProperty.RegisterReadOnly("ShowCheckBoxSpace", typeof(bool), typeof(ExtendedTreeViewItem), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ShowCheckBoxSpaceProperty = ShowCheckBoxSpacePropertyKey.DependencyProperty;

        public int Level
        {
            get { return (int)GetValue(LevelProperty); }
            private set { SetValue(LevelPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey LevelPropertyKey =
            DependencyProperty.RegisterReadOnly("Level", typeof(int), typeof(ExtendedTreeViewItem), new PropertyMetadata());
        public static readonly DependencyProperty LevelProperty = LevelPropertyKey.DependencyProperty;

        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(ExtendedTreeViewItem), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsCheckedChanged)));

        internal ItemCheckBehavior CheckBehavior
        {
            get { return (ItemCheckBehavior)GetValue(CheckBehaviorProperty); }
            set { SetValue(CheckBehaviorProperty, value); }
        }
        internal static readonly DependencyProperty CheckBehaviorProperty = ExtendedTreeView.CheckBehaviorProperty.AddOwner(typeof(ExtendedTreeViewItem));

        internal ItemTypeCheckRecordMode ItemTypeCheckRecord
        {
            get { return (ItemTypeCheckRecordMode)GetValue(ItemTypeCheckRecordProperty); }
            set { SetValue(ItemTypeCheckRecordProperty, value); }
        }
        internal static readonly DependencyProperty ItemTypeCheckRecordProperty = ExtendedTreeView.ItemTypeCheckRecordProperty.AddOwner(typeof(ExtendedTreeViewItem));

        public bool IsItemExpanded
        {
            get { return (bool)GetValue(IsItemExpandedProperty); }
            set { SetValue(IsItemExpandedProperty, value); }
        }
        public static readonly DependencyProperty IsItemExpandedProperty =
            DependencyProperty.Register("IsItemExpanded", typeof(bool), typeof(ExtendedTreeViewItem));

        internal bool IsExpanderVisible
        {
            get { return (bool)GetValue(IsExpanderVisibleProperty); }
            set { SetValue(IsExpanderVisibleProperty, value); }
        }
        internal static readonly DependencyProperty IsExpanderVisibleProperty =
            DependencyProperty.Register("IsExpanderVisible", typeof(bool), typeof(ExtendedTreeViewItem), new UIPropertyMetadata(true));
        #endregion

        #region Routed Events
        #region CheckedEvent
        public static readonly RoutedEvent CheckedEvent
            = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ComboView));

        public event RoutedEventHandler Checked
        {
            add { AddHandler(CheckedEvent, value); }
            remove { RemoveHandler(CheckedEvent, value); }
        }

        protected void OnChecked()
        {
            RoutedEventArgs args = new RoutedEventArgs(CheckedEvent, this);

            RaiseEvent(args);
        }
        #endregion

        #region UncheckedEvent
        public static readonly RoutedEvent UncheckedEvent
            = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ComboView));

        public event RoutedEventHandler Unchecked
        {
            add { AddHandler(UncheckedEvent, value); }
            remove { RemoveHandler(UncheckedEvent, value); }
        }

        protected void OnUnchecked()
        {
            RoutedEventArgs args = new RoutedEventArgs(UncheckedEvent, this);

            RaiseEvent(args);
        }
        #endregion
        #endregion

        #region Commands

        #region ItemExpand
        public static RoutedCommand ItemExpandCommand = new RoutedCommand("ItemExpand", typeof(ExtendedTreeViewItem));

        static void ExecuteItemExpand(object sender, ExecutedRoutedEventArgs e)
        {
            ExtendedTreeViewItem item = sender as ExtendedTreeViewItem;

            if (item != null)
            {
                if (e.Parameter != null && e.Parameter is bool)
                {
                    item.PerformExpand((bool)e.Parameter);
                }
                else
                {
                    if (!ExecuteItemExpand(item, e.Source))
                    {
                        ExecuteItemExpand(item, e.OriginalSource);
                    }
                }
            }
        }

        static bool ExecuteItemExpand(ExtendedTreeViewItem item, object obj)
        {
            ToggleButton button = obj as ToggleButton;

            if (button != null)
            {
                item.PerformExpand(button.IsChecked.GetValueOrDefault());

                return true;
            }

            return false;
        }

        static void CanItemExpand(object sender, CanExecuteRoutedEventArgs e)
        {
            ExtendedTreeViewItem item = sender as ExtendedTreeViewItem;

            if (item != null)
            {
                e.CanExecute = item.IsEnabled;
            }
        }
        #endregion

        #endregion

        #region Ctors
        static ExtendedTreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedTreeViewItem),
                new FrameworkPropertyMetadata(typeof(ExtendedTreeViewItem)));

            IsExpandedProperty.OverrideMetadata(typeof(ExtendedTreeViewItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsExpandedChanged)));
        }

        public ExtendedTreeViewItem(ExtendedTreeView treeViewParent)
        {
            SetTreeViewParent(treeViewParent);

            this.ShowCheckBoxSpace = treeViewParent.HasItemsWithCheckBox;
        }

        public ExtendedTreeViewItem(ExtendedTreeView treeViewParent, ExtendedTreeViewItem parentItem)
        {
            SetTreeViewParent(treeViewParent);
            this.ParentItem = parentItem;

            this.ShowCheckBoxSpace = parentItem.HasItemsWithCheckBox;
            ExtendedTreeViewHelper.SetChildCheckBoxWhenLoaded(parentItem.IsChecked, this, parentItem.CheckBehavior);
        }

        void SetTreeViewParent(ExtendedTreeView treeViewParent)
        {
            this.TreeViewParent = treeViewParent;

            this.ItemTypeCheckRecord = treeViewParent.ItemTypeCheckRecord;
            this.IsExpanderVisible = treeViewParent.IsExpanderVisible;

            if (treeViewParent.EnableCheckBoxView)
            {
                ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
                IsItemExpanded = true;
            }
        }
        #endregion

        #region Expand
        bool expandedIsDirty = true;
        bool expandedInternal;

        static void OnIsExpandedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ExtendedTreeViewItem item = (ExtendedTreeViewItem)o;

            if (!item.expandedInternal)
            {
                if (!item.TreeViewParent.EnableCheckBoxView
                || item.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated
                    || item.Items.Count == 0)
                {
                    item.IsItemExpanded = (bool)e.NewValue;
                }
                else
                {
                    item.expandedIsDirty = true;
                }
            }
        }

        void OnItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;

                if (expandedIsDirty)
                {
                    IsItemExpanded = IsExpanded;
                }

                if (setCheckArgs != null)
                {
                    SetCheckedItemsInternal(setCheckArgs);
                    setCheckArgs = null;
                }
            }
        }

        void PerformExpand(bool expanded)
        {
            expandedInternal = true;
            IsExpanded = expanded;
            IsItemExpanded = IsExpanded;
            expandedInternal = false;
        }
        #endregion

        #region Container Items
        object GetItem()
        {
            if (ParentItem != null)
            {
                return ParentItem.ItemContainerGenerator.ItemFromContainer(this);
            }
            else
            {
                return TreeViewParent.ItemContainerGenerator.ItemFromContainer(this);
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            ExtendedTreeViewItem container = new ExtendedTreeViewItem(TreeViewParent, this);

            container.Level = Level + 1;

            return container;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ExtendedTreeViewItem;
        }
        #endregion

        #region Input Events
        protected override void OnExpanded(RoutedEventArgs e)
        {
            TreeViewParent.SelectionInputSource = ItemSelectionInputSource.IsExpanded;
            base.OnExpanded(e);
        }

        protected override void OnCollapsed(RoutedEventArgs e)
        {
            TreeViewParent.SelectionInputSource = ItemSelectionInputSource.IsExpanded;
            base.OnCollapsed(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            IsExpandedWhenMousedDown = IsExpanded;

            ItemSelectionInputSource inputOrg = TreeViewParent.SelectionInputSource;
            TreeViewParent.SelectionInputSource = ItemSelectionInputSource.MouseDown;

            base.OnMouseLeftButtonDown(e);

            if (!e.Handled)
            {
                TreeViewParent.SelectionInputSource = inputOrg;
            }
            else
            {
                IsExpanded = IsItemExpanded;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space && ShowCheckBox && IsEnabled)
            {
                IsChecked = !IsChecked;
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Enter && IsSelectionActive && !CanBeSelected)
            {
                e.Handled = true;
            }

            TreeViewParent.SelectionInputSource = (e.Key == Key.Enter)
                ? ItemSelectionInputSource.KeyEnter
                : ItemSelectionInputSource.KeyNavigation;

            base.OnKeyDown(e);
        }
        #endregion

        #region CheckBox Handling
        #region SetCheckArgs
        class SetCheckArgs
        {
            public bool? IsChecked { get; set; }
            public Dictionary<object, bool> CheckedItems { get; set; }
            public List<object> AddedItems { get; set; }
            public List<object> RemovedItems { get; set; }
            public bool ConsiderPending { get; set; }
            public bool IsValue { get; set; }
        }
        #endregion

        SetCheckArgs setCheckArgs;
        bool performCheckInternal;

        static void OnShowCheckBoxChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ExtendedTreeViewItem item = (ExtendedTreeViewItem)o;

            bool v = (bool)e.NewValue;
            if (v && !item.TreeViewParent.EnableCheckBoxView)
            {
                item.ShowCheckBox = false;
            }
            else
            {
                if (item.ParentItem != null)
                {
                    item.ParentItem.SetChildrenShowCheckBoxSpace(v);
                }
                else
                {
                    item.TreeViewParent.SetChildrenShowCheckBoxSpace(v);
                }
            }
        }

        static void OnIsCheckedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ExtendedTreeViewItem item = (ExtendedTreeViewItem)o;

            bool isChecked = item.IsChecked.GetValueOrDefault();
            if (isChecked)
            {
                item.OnChecked();
            }
            else
            {
                item.OnUnchecked();
            }

            if (!item.performCheckInternal)
            {
                List<object> addedItems = new List<object>();
                List<object> removedItems = new List<object>();

                object content = item.GetItem();

                if (isChecked)
                {
                    if (item.ShouldHandleIsChecked)
                    {
                        addedItems.Add(content);
                    }
                }
                else
                {
                    if (item.CheckBehavior == ItemCheckBehavior.UserWithChildToggle)
                    {
                        ExtendedTreeViewItem parentContainer = item.ParentItem;
                        while (parentContainer != null)
                        {
                            if (!parentContainer.IsChecked.Equals(item.IsChecked))
                            {
                                parentContainer.SetIsCheckedInternal(isChecked);

                                if (parentContainer.ShouldHandleIsChecked)
                                {
                                    removedItems.Add(parentContainer.GetItem());
                                }
                            }

                            parentContainer = parentContainer.ParentItem;
                        }
                    }

                    if (item.ShouldHandleIsChecked)
                    {
                        removedItems.Add(content);
                    }
                }

                ExtendedTreeViewHelper.SetChildrenCheckBox(item, item.IsChecked, item.CheckBehavior, addedItems, removedItems);

                if (isChecked && item.CheckBehavior == ItemCheckBehavior.UserWithChildToggle)
                {
                    item.AlertParentChecked(addedItems);
                }

                item.TreeViewParent.OnChildrenCheckedChanged(addedItems, removedItems);
            }
        }

        internal void SetIsCheckedInternal(bool? isChecked)
        {
            try
            {
                performCheckInternal = true;

                IsChecked = isChecked;
            }
            finally
            {
                performCheckInternal = false;
            }
        }

        internal void AlertParentChecked(List<object> addedItems)
        {
            ExtendedTreeViewItem parentContainer = ParentItem;
            while (parentContainer != null)
            {
                if (!parentContainer.IsChecked.Equals(IsChecked))
                {
                    bool shouldAdd = parentContainer.ToggleChildCheckInternal();

                    if (shouldAdd && parentContainer.ShouldHandleIsChecked)
                    {
                        addedItems.Add(parentContainer.GetItem());
                    }
                    else if (!shouldAdd)
                    {
                        break;
                    }
                }

                parentContainer = parentContainer.ParentItem;
            }
        }

        internal bool ToggleChildCheckInternal()
        {
            try
            {
                performCheckInternal = true;

                bool shouldBeChecked = AreAllChildrenChecked();

                if (shouldBeChecked)
                {
                    IsChecked = true;
                }

                return shouldBeChecked;
            }
            finally
            {
                performCheckInternal = false;
            }
        }

        bool AreAllChildrenChecked()
        {
            bool result = true;

            foreach (var item in Items)
            {
                ExtendedTreeViewItem container = ItemContainerGenerator.ContainerFromItem(item) as ExtendedTreeViewItem;

                if (container != null)
                {
                    if (container.ShouldHandleIsChecked)
                    {
                        result &= container.IsChecked.GetValueOrDefault();
                    }
                    else if (container.Items.Count > 0)
                    {
                        result &= container.AreAllChildrenChecked();
                    }
                }

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        internal void SetIsCheckedInternal(bool? isChecked, List<object> addedItems, List<object> removedItems)
        {
            try
            {
                performCheckInternal = true;
                
                IsChecked = isChecked;

                ExtendedTreeViewHelper.SetChildrenCheckBox(this, IsChecked, CheckBehavior, addedItems, removedItems);
            }
            finally
            {
                performCheckInternal = false;
            }
        }

        internal bool SetCheckedItemsInternal(bool? isChecked, Dictionary<object, bool> checkedItems, List<object> addedItems, List<object> removedItems, bool considerPending, bool isValue)
        {
            SetCheckArgs args = new SetCheckArgs
            {
                IsChecked = isChecked,
                CheckedItems = checkedItems,
                AddedItems = addedItems,
                RemovedItems = removedItems,
                ConsiderPending = considerPending,
                IsValue = isValue
            };

            if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated && Items.Count > 0)
            {
                setCheckArgs = args;

                ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;

                return true;
            }
            else
            {
                SetCheckedItemsInternal(args);

                return false;
            }
        }

        void SetCheckedItemsInternal(SetCheckArgs args)
        {
            try
            {
                performCheckInternal = true;

                IsChecked = args.IsChecked;

                if (CheckBehavior == ItemCheckBehavior.UserWithChildToggle
                    && args.IsChecked.GetValueOrDefault()
                    && Items.Count == 0
                    && ParentItem != null)
                {
                    AlertParentChecked(args.AddedItems);
                }

                ExtendedTreeViewHelper.SetCheckedItems(TreeViewParent, this, args.CheckedItems, args.AddedItems, args.RemovedItems, args.ConsiderPending, args.IsValue);
            }
            finally
            {
                performCheckInternal = false;
            }
        }

        internal void SetIsCheckedWhenLoadedInternal(bool? isChecked)
        {
            try
            {
                performCheckInternal = true;

                IsChecked = isChecked;
            }
            finally
            {
                performCheckInternal = false;
            }
        }

        void SetChildrenShowCheckBoxSpace(bool value)
        {
            HasItemsWithCheckBox = ExtendedTreeViewHelper.SetChildrenShowCheckBoxSpace(this, value);
        }
        #endregion
    }
}
