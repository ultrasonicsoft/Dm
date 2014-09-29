using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Collections;

namespace Ultrasonic.DownloadManager.Controls
{
    public class ComboSelectorViewHandler : BaseComboItemsViewHandler
    {
        protected override void OnViewInitialized()
        {
            base.OnViewInitialized();

            SetViewBinding(Selector.SelectedValuePathProperty);
        }

        protected override void SetItemsViewSelectionHooks()
        {
            ((Selector)ItemsView).SelectionChanged += OnItemsViewSelectionChanged;
        }

        protected override void UnsetItemsViewSelectionHooks()
        {
            ((Selector)ItemsView).SelectionChanged -= OnItemsViewSelectionChanged;
        }

        void OnItemsViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public override void SetSelectedItems(IList selectedItems)
        {
            throw new NotImplementedException();
        }

        public override void SetSelectedValues(IList selectedValues)
        {
            throw new NotImplementedException();
        }
    }
}
