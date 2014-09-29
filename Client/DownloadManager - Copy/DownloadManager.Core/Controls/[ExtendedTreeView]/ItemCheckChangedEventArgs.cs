using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Ultrasonic.DownloadManager.Controls
{
    public class ItemCheckChangedEventArgs : RoutedEventArgs
    {
        public IList<object> AddedItems { get; internal set; }
        public IList<object> RemovedItems { get; internal set; }

        public ItemCheckChangedEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
        }

        public ItemCheckChangedEventArgs(RoutedEvent routedEvent, object source, IList<object> addedItems, IList<object> removedItems)
            : this(routedEvent, source)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
        }
    }
}
