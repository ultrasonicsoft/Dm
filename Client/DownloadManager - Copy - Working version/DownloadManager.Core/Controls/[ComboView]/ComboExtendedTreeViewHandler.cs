using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using Ultrasonic.DownloadManager.Core.Utils;

namespace Ultrasonic.DownloadManager.Controls
{
    public class ComboExtendedTreeViewHandler : ComboTreeViewHandler
    {
        IList<object> added;
        IList<object> removed;
        ExtendedTreeView treeView;
        object lastSelectedItem;
        object lastSelectedItemSet;
        bool setSelectedCalled;
        bool itemSourceSet;

        void OnItemMouseEvent(object sender, MouseButtonEventArgs e)
        {
            if (ComboView.IsDropDownOpen)
            {
                ExtendedTreeViewItem item = ((DependencyObject)e.OriginalSource).FindVisualAncestorByType<ExtendedTreeViewItem>();

                ConsiderSingleSelection(item);
            }
        }

        void SetComboViewSingleSelection(object item)
        {
            lastSelectedItemSet = item;
            ComboView.SetSelection(item);
        }

        void OnItemKeyEvent(object sender, KeyEventArgs e)
        {
            if (ComboView.IsDropDownOpen && e.Key == Key.Enter)
            {
                ExtendedTreeViewItem item = ((DependencyObject)e.OriginalSource).FindVisualAncestorByType<ExtendedTreeViewItem>();

                ConsiderSingleSelection(item);
            }
        }

        void ConsiderSingleSelection(ExtendedTreeViewItem item)
        {
            if (item != null && item.CanBeSelected)
            {
                if ((Object.Equals(item.Header, lastSelectedItem)
                    && item.TreeViewParent.SelectionInputSource != ItemSelectionInputSource.IsExpanded)
                || ((item.TreeViewParent.SelectionInputSource == ItemSelectionInputSource.MouseDown
                        || item.TreeViewParent.SelectionInputSource == ItemSelectionInputSource.KeyEnter)
                    && ComboView.SelectedViewItems.Count == 0 && item.Header.Equals(treeView.SelectedItem)))
                {
                    lastSelectedItem = item.Header;

                    if (ComboView.CloseDropDownUponSelection
                        && Object.Equals(item.Header, lastSelectedItemSet))
                    {
                        ComboView.IsDropDownOpen = false;
                    }
                    else
                    {
                        SetComboViewSingleSelection(lastSelectedItem);
                    }
                }
            }
        }

        void OnItemSelectedEvent(object sender, RoutedEventArgs e)
        {
            if (ComboView.IsDropDownOpen)
            {
                ExtendedTreeViewItem item = ((DependencyObject)e.OriginalSource).FindVisualAncestorByType<ExtendedTreeViewItem>();

                if (item != null && item.CanBeSelected)
                {
                    lastSelectedItem = item.Header;
                }
            }
        }

        protected override void OnViewInitialized()
        {
            treeView = (ExtendedTreeView)ItemsView;

            base.OnViewInitialized();
        }

        public override void OnItemsSourceChangedPreview(IEnumerable oldValue, IEnumerable newValue, bool selectingGlobal)
        {
            treeView.CalcCheckedOnSourceChange = !selectingGlobal;
        }

        public override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            bool hasItems = (newValue != null && newValue.OfType<object>().Count() > 0);

            if (!treeView.EnableCheckBoxView)
            {
                if (hasItems && oldValue != null)
                {
                    treeView.IgnoreNextSelectionChange = true;
                }

                base.OnItemsSourceChanged(oldValue, newValue);
            }
            else if (ComboView.SelectedViewItems != null
                && ComboView.SelectedViewItems.Count > 0
                && !hasItems)
            {
                ComboView.ResetSelectedValueItems(new ObservableCollection<object>(), false);
            }
        }

        protected override void OnSelectionAfterItemSourceSetLoaded()
        {
            //occurs only in non-checkbox view
            //didn't find the item
            if (lastSelectedItemSet != null && !lastSelectedItemSet.Equals(treeView.SelectedItem))
            {
                lastSelectedItemSet = null;

                ComboView.ResetSelectedValueItems(new ObservableCollection<object>());
            }
            

            base.OnSelectionAfterItemSourceSetLoaded();
        }

        protected override void SetItemsViewSelectionHooks()
        {
            if (treeView.EnableCheckBoxView)
            {
                treeView.ItemCheckChanged += OnViewItemCheckChanged;
            }
            else
            {
                treeView.AddHandler(ExtendedTreeViewItem.MouseLeftButtonUpEvent,
                    new MouseButtonEventHandler(OnItemMouseEvent), true);

                treeView.AddHandler(ExtendedTreeViewItem.SelectedEvent,
                    new RoutedEventHandler(OnItemSelectedEvent), true);

                treeView.AddHandler(ExtendedTreeViewItem.KeyDownEvent,
                    new KeyEventHandler(OnItemKeyEvent), true);

                base.SetItemsViewSelectionHooks();
            }
        }

        protected override void OnItemsViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!Object.Equals(lastSelectedItemSet, e.NewValue))
            {
                bool commitSelection = false;
                switch (treeView.SelectionInputSource)
                {
                    case ItemSelectionInputSource.MouseDown:
                    case ItemSelectionInputSource.KeyEnter:
                        commitSelection = true;
                        break;
                        
                    case ItemSelectionInputSource.IsExpanded:
                        if (itemSourceSet)
                        {
                            ComboView.ResetSelectedValueItems(new ObservableCollection<object>());
                        }
                        else if (setSelectedCalled)
                        {
                            commitSelection = true;
                        }
                        break;

                    case ItemSelectionInputSource.None:
                        lastSelectedItem = e.NewValue;
                        commitSelection = true;
                        break;
                }

                if (commitSelection)
                {
                    SetComboViewSingleSelection(e.NewValue);
                }
            }

            //setSelectedCalled = false;
            setSelectedCalled = (e.NewValue == null && treeView.SelectionInputSource == ItemSelectionInputSource.None);
            itemSourceSet = false;
        }

        protected override void UnsetItemsViewSelectionHooks()
        {
            if (treeView.EnableCheckBoxView)
            {
                treeView.ItemCheckChanged -= OnViewItemCheckChanged;
            }
            else
            {
                base.UnsetItemsViewSelectionHooks();
            }
        }

        void OnViewItemCheckChanged(object sender, ItemCheckChangedEventArgs e)
        {
            if (IsIterationInProgress)
            {
                if (!isEnding)
                {
                    hasChanged = true;

                    if (added == null)
                    {
                        added = e.AddedItems;
                    }
                    else
                    {
                        added = added.Except(e.RemovedItems).Union(e.AddedItems).ToList();
                    }

                    if (removed == null)
                    {
                        removed = e.RemovedItems;
                    }
                    else
                    {
                        removed = removed.Except(e.AddedItems).Union(e.RemovedItems).ToList();
                    }
                }
            }
            else
            {
                SetComboSelectedItems(e.AddedItems, e.RemovedItems);
            }
        }

        public override void SetSelectedItems(IList selectedItems)
        {
            setSelectedCalled = true;
            if (treeView.EnableCheckBoxView)
            {
                treeView.SetCheckedItems(selectedItems);
            }
            else
            {
                if (selectedItems != null && selectedItems.Count == 0)
                {
                    lastSelectedItemSet = null;
                }

                base.SetSelectedItems(selectedItems);
            }
        }

        public override void SetSelectedValues(IList selectedValues)
        {
            setSelectedCalled = true;
            if (treeView.EnableCheckBoxView)
            {
                treeView.SetCheckedValues(selectedValues);
            }
            else
            {
                base.SetSelectedValues(selectedValues);
            }
        }

        void SetComboSelectedItems(IList<object> added, IList<object> removed)
        {
            ComboView.SetSelection(added as IList, removed as IList);
        }

        //void SetComboSelectedItems(IList items)
        //{
        //    ComboView.SetSelection(GetDuplicatedCheckedItems());
        //}

        IList GetDuplicatedCheckedItems()
        {
            return new ObservableCollection<object>(
                (IEnumerable<object>)treeView.CheckedItems);
        }

        #region Selection Iteration
        bool hasChanged;
        bool isEnding;
        IList lastItems;

        protected override bool IsSelectinIterationSupported
        {
            get { return true; }
        }

        protected override void OnBeginSelectionIteration()
        {
            hasChanged = false;
            lastItems = GetDuplicatedCheckedItems();
            added = null;
            removed = null;
        }

        protected override void OnEndSelectionIteration(bool apply)
        {
            isEnding = true;

            //cancel
            if (!apply && hasChanged)
            {
                treeView.SetCheckedItems(lastItems);
            }
            else if (apply && hasChanged) //apply
            {
                //SetComboSelectedItems(treeView.CheckedItems);
                SetComboSelectedItems(
                    added.Except(lastItems.OfType<object>()).ToList(),
                    removed.Intersect(lastItems.OfType<object>()).ToList());
            }

            isEnding = false;
        }
        #endregion
    }
}
