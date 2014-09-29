using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Collections;

namespace Ultrasonic.DownloadManager.Controls
{
    public interface IComboItemsViewHandler
    {
        bool IsSelectionSubscribed { get; }

        void InitializeView(ComboView comboView, ItemsControl itemsView);

        void UnloadView();

        void OnDropDownOpened();

        void OnDropDownClosed();

        void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue);

        void OnItemsSourceChangedPreview(IEnumerable oldValue, IEnumerable newValue, bool selectingGlobal);

        void BeginSelectionIteration();

        void EndSelectionIteration(bool apply);

        void SetSelectedItems(IList selectedItems);

        void SetSelectedValues(IList selectedValues);
    }
}
