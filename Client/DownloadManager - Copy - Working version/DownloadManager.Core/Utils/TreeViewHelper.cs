using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Ultrasonic.DownloadManager.Controls;

namespace Ultrasonic.DownloadManager.Core.Utils
{
    public static class TreeViewHelper
    {
        /// <summary>
        /// Expands all children of a TreeView
        /// </summary>
        /// <param name="treeView">The TreeView whose children will be expanded</param>
        public static void ExpandAll(this TreeView treeView)
        {
            ExpandSubContainers(treeView, false);
        }

        public static void ExpandAll2(this TreeView treeView)
        {
            ApplyActionToAllTreeViewItems(treeView, itemsControl =>
            {
                itemsControl.IsExpanded = true;
                UIHelper.WaitForPriority(DispatcherPriority.ContextIdle);
            });
        }

        /// <summary>
        /// Expands all children of a TreeView or TreeViewItem
        /// </summary>
        /// <param name="parentContainer">The TreeView or TreeViewItem containing the children to expand</param>
        internal static void ExpandSubContainers(this ItemsControl parentContainer, bool preserveState)
        {
            foreach (object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    //expand the item
                    if (preserveState)
                    {
                        currentContainer.Tag = currentContainer.IsExpanded;
                    }
                    currentContainer.IsExpanded = true;

                    //if the item's children are not generated, they must be expanded
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        //store the event handler in a variable so we can remove it (in the handler itself)
                        EventHandler eh = null;
                        eh = new EventHandler(delegate
                        {
                            //once the children have been generated, expand those children's children then remove the event handler
                            if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                ExpandSubContainers(currentContainer, preserveState);
                                currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                                if (preserveState)
                                {
                                    currentContainer.IsExpanded = (bool)currentContainer.Tag;
                                }
                            }
                        });

                        currentContainer.ItemContainerGenerator.StatusChanged += eh;
                    }
                    else //otherwise the children have already been generated, so we can now expand those children
                    {
                        ExpandSubContainers(currentContainer, preserveState);
                    }
                }
            }
        }

        /// <summary>
        /// Searches a TreeView for the provided object and selects it if found
        /// </summary>
        /// <param name="treeView">The TreeView containing the item</param>
        /// <param name="item">The item to search and select</param>
        public static void SelectItem(this TreeView treeView, object item)
        {
            ExpandAndSelectItem(treeView, item, false);
        }

        /// <summary>
        /// Searches a TreeView for the provided object and selects it if found
        /// </summary>
        /// <param name="treeView">The TreeView containing the item</param>
        /// <param name="item">The item to search and select</param>
        public static void SelectValue(this TreeView treeView, object value)
        {
            ExpandAndSelectItem(treeView, value, true);
        }

        static void ExpandAndSelectItem(TreeView treeView, object obj, bool isValue)
        {
            ExpandAndSelectItem(treeView, obj, isValue, null, null);
        }

        internal static void ExpandAndSelectItem(TreeView treeView, object obj, bool isValue, Action<TreeViewItem> beforeSelection, Action<TreeViewItem> afterSelection)
        {
            if (treeView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                EventHandler eh = null;
                eh = new EventHandler(delegate
                {
                 if (treeView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                 {
                     ExpandAndSelectItem(treeView, obj, isValue, treeView.SelectedValuePath, beforeSelection, afterSelection);

                     //remove the StatusChanged event handler since we just handled it (we only needed it once)
                     treeView.ItemContainerGenerator.StatusChanged -= eh;
                 }
                });

                treeView.ItemContainerGenerator.StatusChanged += eh;
            }
            else
            {
                ExpandAndSelectItem(treeView, obj, isValue, treeView.SelectedValuePath, beforeSelection, afterSelection);
            }
        }

        /// <summary>
        /// Finds the provided object in an ItemsControl's children and selects it
        /// </summary>
        /// <param name="parentContainer">The parent container whose children will be searched for the selected item</param>
        /// <param name="itemToSelect">The item to select</param>
        /// <returns>True if the item is found and selected, false otherwise</returns>
        private static bool ExpandAndSelectItem(ItemsControl parentContainer, object itemToSelect, bool isValue, string valuePath, Action<TreeViewItem> beforeSelection, Action<TreeViewItem> afterSelection)
        {
            //check all items at the current level
            foreach (object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                object equalItem = item;
                if (isValue)
                {
                    equalItem = BindingHelper.Eval<object>(item, valuePath);
                }

                //if the data item matches the item we want to select, set the corresponding
                //TreeViewItem IsSelected to true
                if (currentContainer != null)
                {
                    if (equalItem.Equals(itemToSelect))
                    {
                        if (beforeSelection != null)
                        {
                            beforeSelection(currentContainer);
                        }

                        //the following is in order to reset selection if the item is already selected
                        //TODO - get an external action, don't try casting like that.. but hey, tough
                        if (currentContainer.IsSelected)
                        {
                            ExtendedTreeViewItem extendedContainer = currentContainer as ExtendedTreeViewItem;
                            if (extendedContainer != null)
                            {
                                extendedContainer.TreeViewParent.ReselectItem(extendedContainer);
                            }
                        }
                        else
                        {
                            currentContainer.IsSelected = true;
                        }
                        
                        currentContainer.BringIntoView();
                        currentContainer.Focus();
                        if (afterSelection != null)
                        {
                            afterSelection(currentContainer);
                        }

                        //the item was found
                        return true;
                    }
                    else if (currentContainer.IsSelected)
                    {
                        currentContainer.IsSelected = false;
                    }
                }
            }

            //if we get to this point, the selected item was not found at the current level, so we must check the children
            foreach (object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                //if children exist
                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    //keep track of if the TreeViewItem was expanded or not
                    bool wasExpanded = currentContainer.IsExpanded;

                    //expand the current TreeViewItem so we can check its child TreeViewItems
                    currentContainer.IsExpanded = true;

                    //if the TreeViewItem child containers have not been generated, we must listen to
                    //the StatusChanged event until they are
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        //store the event handler in a variable so we can remove it (in the handler itself)
                        EventHandler eh = null;
                        eh = new EventHandler(delegate
                        {
                            if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                if (ExpandAndSelectItem(currentContainer, itemToSelect, isValue, valuePath, beforeSelection, afterSelection) == false)
                                {
                                    //The assumption is that code executing in this EventHandler is the result of the parent not
                                    //being expanded since the containers were not generated.
                                    //since the itemToSelect was not found in the children, collapse the parent since it was previously collapsed
                                    currentContainer.IsExpanded = false;
                                }

                                //remove the StatusChanged event handler since we just handled it (we only needed it once)
                                currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                            }
                        });
                        currentContainer.ItemContainerGenerator.StatusChanged += eh;
                    }
                    else //otherwise the containers have been generated, so look for item to select in the children
                    {
                        if (ExpandAndSelectItem(currentContainer, itemToSelect, isValue, valuePath, beforeSelection, afterSelection) == false)
                        {
                            //restore the current TreeViewItem's expanded state
                            currentContainer.IsExpanded = wasExpanded;
                        }
                        else //otherwise the node was found and selected, so return true
                        {
                            return true;
                        }
                    }
                }
            }

            //no item was found
            return false;
        }

        public static void ApplyActionToAllTreeViewItems(this ItemsControl itemsControl, Action<TreeViewItem> itemAction)
        {
            Stack<ItemsControl> itemsControlStack = new Stack<ItemsControl>();
            itemsControlStack.Push(itemsControl);

            while (itemsControlStack.Count != 0)
            {
                ItemsControl currentItem = itemsControlStack.Pop() as ItemsControl;
                TreeViewItem currentTreeViewItem = currentItem as TreeViewItem;
                if (currentTreeViewItem != null)
                {
                    itemAction(currentTreeViewItem);
                }
                if (currentItem != null) // this handles the scenario where some TreeViewItems are already collapsed
                {
                    foreach (object dataItem in currentItem.Items)
                    {
                        ItemsControl childElement = (ItemsControl)currentItem.ItemContainerGenerator.ContainerFromItem(dataItem);

                        itemsControlStack.Push(childElement);
                    }
                }
            }
        }

        public static void SelectOne(this TreeView treeView, object item, bool isValue)
        {
            ApplyActionToAllTreeViewItems(treeView, itemsControl =>
            {
                object equalItem = itemsControl.Header;
                if (isValue)
                {
                    equalItem = BindingHelper.Eval<object>(equalItem, treeView.SelectedValuePath);
                }

                if (equalItem.Equals(item))
                {
                    itemsControl.IsSelected = true;
                    itemsControl.BringIntoView();
                    itemsControl.Focus();

                    return;
                }
                else
                {
                    itemsControl.IsSelected = false;
                }

                bool wasExpanded = itemsControl.IsExpanded;
                itemsControl.IsExpanded = true;
                
                UIHelper.WaitForPriority(DispatcherPriority.ContextIdle);
                
                itemsControl.IsExpanded = wasExpanded;

            });
        }
    }
}
