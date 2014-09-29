using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Controls.Primitives;
using Ultrasonic.DownloadManager.Core.Utils;

namespace Ultrasonic.DownloadManager.Controls
{
    internal static class ExtendedTreeViewHelper
    {
        internal static bool SetChildrenShowCheckBoxSpace(ItemsControl itemsControl, bool value)
        {
            List<ExtendedTreeViewItem> containers = null;

            if (!value)
            {
                containers = new List<ExtendedTreeViewItem>();
            }

            foreach (var item in itemsControl.Items)
            {
                ExtendedTreeViewItem container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as ExtendedTreeViewItem;

                if (container != null)
                {
                    if (value)
                    {
                        container.ShowCheckBoxSpace = true;
                    }
                    else
                    {
                        value = value || container.ShowCheckBox;
                    }
                }

                if (containers != null)
                {
                    containers.ForEach(c => c.ShowCheckBoxSpace = value);
                }
            }

            return value;
        }

        internal static void SetChildrenCheckBox(ExtendedTreeViewItem parentContainer, bool? isChecked, ItemCheckBehavior behavior, List<object> addedItems, List<object> removedItems)
        {
            if (parentContainer.Items.Count > 0 && behavior == ItemCheckBehavior.UserWithChildToggle)
            {
                foreach (var item in parentContainer.Items)
                {
                    ExtendedTreeViewItem container = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as ExtendedTreeViewItem;

                    bool whenLoaded = (addedItems == null || removedItems == null);

                    if (!whenLoaded)
                    {
                        bool hasContainer = (container != null);
                        bool shouldAdd = !hasContainer
                            || (!container.IsChecked.Equals(isChecked)
                                && container.ShouldHandleIsChecked);

                        if (shouldAdd)
                        {
                            if (isChecked.GetValueOrDefault())
                            {
                                addedItems.Add(item);
                            }
                            else
                            {
                                removedItems.Add(item);
                            }
                        }

                        if (hasContainer)
                        {
                            container.SetIsCheckedInternal(isChecked, addedItems, removedItems);
                        }
                    }
                    else if (container != null)
                    {
                        SetChildCheckBoxWhenLoaded(isChecked, container, parentContainer.CheckBehavior);
                    }
                }
            }
        }

        internal static void SetChildCheckBoxWhenLoaded(bool? parentIsChecked, ExtendedTreeViewItem child, ItemCheckBehavior behavior)
        {
            if (behavior == ItemCheckBehavior.UserWithChildToggle)
            {
                child.SetIsCheckedWhenLoadedInternal(parentIsChecked);
            }
        }

        internal static void SetCheckedItems(ExtendedTreeView treeView, Dictionary<object, bool> checkedItems, bool considerPending, bool isValue)
        {
            List<object> addedItems = new List<object>();
            List<object> removedItems = new List<object>();

            treeView.CheckProcessCounter = 1;

            SetCheckedItems(treeView, treeView, checkedItems, addedItems, removedItems, considerPending, isValue);
        }

        internal static void SetCheckedItems(ExtendedTreeView treeView, ItemsControl itemsControl, Dictionary<object, bool> checkedItems, List<object> addedItems, List<object> removedItems, bool considerPending, bool isValue)
        {
            treeView.CheckProcessCounter += itemsControl.Items.Count;
            treeView.CheckProcessCounter--;

            foreach (var item in itemsControl.Items)
            {
                ExtendedTreeViewItem container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as ExtendedTreeViewItem;

                object itemToMatch = item;
                if (isValue)
                {
                    itemToMatch = BindingHelper.Eval<object>(item, treeView.SelectedValuePath);
                }

                bool found = checkedItems.ContainsKey(itemToMatch)
                    || (container.CheckBehavior == ItemCheckBehavior.UserWithChildToggle
                        && container.ParentIsChecked.GetValueOrDefault());

                if (container.ShouldHandleIsChecked && !container.IsChecked.Equals(found))
                {
                    if (found)
                    {
                        addedItems.Add(item);
                    }
                    else
                    {
                        removedItems.Add(item);
                    }
                }

                container.SetCheckedItemsInternal(found, checkedItems, addedItems, removedItems, considerPending, isValue);
            }

            if (treeView.CheckProcessCounter == 0 && itemsControl.Items.Count == 0)
            {
                OnSetCheckedItemsComplete(treeView, addedItems, removedItems);
            }
        }

        static void OnSetCheckedItemsComplete(ExtendedTreeView treeView, List<object> addedItems, List<object> removedItems)
        {
            treeView.OnChildrenCheckedChanged(addedItems, removedItems);
        }
    }
}
