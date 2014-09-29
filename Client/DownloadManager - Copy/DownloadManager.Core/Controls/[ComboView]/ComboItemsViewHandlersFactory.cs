using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace Ultrasonic.DownloadManager.Controls
{
    public static class ComboItemsViewHandlersFactory
    {
        public static IComboItemsViewHandler GetHandler(ComboView comboView, ItemsControl itemsView)
        {
            IComboItemsViewHandler handler = null;

            if (itemsView is Selector)
            {
                handler = new ComboSelectorViewHandler();
            }
            else if (itemsView is ExtendedTreeView)
            {
                handler = new ComboExtendedTreeViewHandler();
            }
            else if (itemsView is TreeView)
            {
                handler = new ComboTreeViewHandler();
            }
            else
            {
                throw new InvalidOperationException("Couldn't find a matching combo view handler");
            }

            return handler;
        }
    }
}
