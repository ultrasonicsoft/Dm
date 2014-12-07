using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;

namespace Ultrasonic.DownloadManager.Controls
{
    public abstract class BaseComboItemsViewHandler : IComboItemsViewHandler
    {
        protected ComboView ComboView { get; set; }
        protected ItemsControl ItemsView { get; set; }
        protected bool ItemSourceLoad { get; set; }

        public void InitializeView(ComboView comboView, ItemsControl itemsView)
        {
            ComboView = comboView;
            ItemsView = itemsView;

            OnViewInitialized();
            SetItemsViewSelectionHooks();
        }

        public void UnloadView()
        {
            UnsetItemsViewSelectionHooks();
        }

        protected virtual void OnViewInitialized()
        {
            SetViewBinding(ItemsControl.ItemsSourceProperty);
            SetViewBinding(ItemsControl.ItemContainerStyleProperty);
            SetViewBinding(ItemsControl.ItemTemplateProperty);
            SetViewBinding(ItemsControl.DisplayMemberPathProperty);
            SetViewBinding(ItemsControl.ItemContainerStyleSelectorProperty);
            SetViewBinding(ItemsControl.ItemTemplateSelectorProperty);
            SetViewBinding(ItemsControl.ItemStringFormatProperty);
        }

        protected abstract void SetItemsViewSelectionHooks();
        protected abstract void UnsetItemsViewSelectionHooks();
        public abstract void SetSelectedItems(IList selectedItems);
        public abstract void SetSelectedValues(IList selectedValues);

        public virtual bool IsSelectionSubscribed
        {
            get { return true; }
        }

        protected void SetViewBinding(DependencyProperty p)
        {
            SetViewBinding(p, p, false);
        }

        protected void SetViewBinding(DependencyProperty source, DependencyProperty target, bool twoWay)
        {
            Binding b = new Binding(source.Name);
            b.Source = ComboView;

            if (twoWay)
            {
                b.Mode = BindingMode.TwoWay;
            }

            ItemsView.SetBinding(target, b);
        }

        protected void SetComboBinding(DependencyProperty source, DependencyProperty target, bool twoWay)
        {
            Binding b = new Binding(source.Name);
            b.Source = ItemsView;

            if (twoWay)
            {
                b.Mode = BindingMode.TwoWay;
            }

            ComboView.SetBinding(target, b);
        }

        public virtual void OnItemsSourceChangedPreview(IEnumerable oldValue, IEnumerable newValue, bool selectingGlobal)
        {
        }

        public virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            ItemSourceLoad = true;

            bool hasViewItems = (ComboView.SelectedViewItems != null && ComboView.SelectedViewItems.Count > 0);
            bool hasViewValues = (ComboView.SelectedViewValues != null && ComboView.SelectedViewValues.Count > 0);

            if (!hasViewItems && !hasViewValues)
            {
                ComboView.ResetSelectedValueItems(new ObservableCollection<object>());

                ItemSourceLoad = false;
            }
            else if (hasViewItems)
            {
                IList items = ComboView.SelectedViewItems;

                //LoadSelectionAfterItemSourceSet(items, false);
                ComboView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadSelectionAfterItemSourceSet(items, false);
                }));
            }
            else if (hasViewValues)
            {
                IList items = ComboView.SelectedViewValues;

                //LoadSelectionAfterItemSourceSet(items, true);
                ComboView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadSelectionAfterItemSourceSet(items, true);
                }));
            }
        }

        void LoadSelectionAfterItemSourceSet(IList items, bool isValue)
        {
            if (isValue)
            {
                SetSelectedValues(items);
            }
            else
            {
                SetSelectedItems(items);
            }

            OnSelectionAfterItemSourceSetLoaded();

            ItemSourceLoad = false;
        }

        protected virtual void OnSelectionAfterItemSourceSetLoaded()
        {
        }

        public virtual void OnDropDownOpened()
        {
        }

        public virtual void OnDropDownClosed()
        {
        }

        #region Selection Iteration
        public bool IsIterationInProgress { get; private set; }

        protected virtual bool IsSelectinIterationSupported
        {
            get { return false; }
        }

        public void BeginSelectionIteration()
        {
            EnsureIterationSupport();

            IsIterationInProgress = true;

            OnBeginSelectionIteration();
        }

        public void EndSelectionIteration(bool apply)
        {
            EnsureIterationSupport();

            OnEndSelectionIteration(apply);

            IsIterationInProgress = false;
        }

        protected virtual void OnBeginSelectionIteration() { }
        protected virtual void OnEndSelectionIteration(bool apply) { }

        private void EnsureIterationSupport()
        {
            if (!IsSelectinIterationSupported)
            {
                throw new NotSupportedException("Selection iteration not supported");
            }
        }
        #endregion
    }
}
