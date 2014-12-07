using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;
using Ultrasonic.DownloadManager.Core.Utils;

namespace Ultrasonic.DownloadManager.Controls
{
    public class ComboTreeViewHandler : BaseComboItemsViewHandler
    {
        TreeView treeView;
        object lastSelected;

        protected override void OnViewInitialized()
        {
            base.OnViewInitialized();

            treeView = (TreeView)ItemsView;

            SetViewBinding(ComboView.SelectedValuePathProperty, TreeView.SelectedValuePathProperty, false);
        }

        protected override void SetItemsViewSelectionHooks()
        {
            treeView.SelectedItemChanged += OnItemsViewSelectedItemChanged;
        }

        protected override void UnsetItemsViewSelectionHooks()
        {
            treeView.SelectedItemChanged -= OnItemsViewSelectedItemChanged;
        }

        protected override void OnSelectionAfterItemSourceSetLoaded()
        {
            //if (treeView.SelectedItem != null)
            //{
            //    ComboView.SelectedViewItems.Clear();
            //}
        }

        protected virtual void OnItemsViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            lastSelected = e.NewValue;

            ComboView.SetSelection(e.NewValue);
        }

        public override void SetSelectedItems(IList selectedItems)
        {
            if (selectedItems.Count > 0)
            {
                TreeViewHelper.SelectItem(treeView, selectedItems[0]);
            }
        }

        public override void SetSelectedValues(IList selectedValues)
        {
            if (selectedValues.Count > 0)
            {
                TreeViewHelper.SelectValue(treeView, selectedValues[0]);
            }
            else
            {
                treeView.ApplyActionToAllTreeViewItems(t => t.IsSelected = false);
            }
        }
    }
}
