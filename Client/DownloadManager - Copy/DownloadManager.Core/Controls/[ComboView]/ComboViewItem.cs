using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Ultrasonic.DownloadManager.Controls
{
    public class ComboViewItem : ComboBoxItem
    {
        #region Properties
        public ComboBox ComboBoxParent { get; set; }
        #endregion

        #region Ctors
        public ComboViewItem(ComboBox comboBoxParent)
        {
            this.ComboBoxParent = comboBoxParent;
        }
        #endregion
    }
}
