using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Ultrasonic.DownloadManager.Core.Utils;
using System.Dynamic;
using System.Diagnostics.Contracts;

namespace Ultrasonic.DownloadManager.Core.ViewModels
{
    public interface IViewModelProxy<T> : INotifyPropertyChanged, IDataErrorInfo
        where T : IViewModelBase
    {
        void AcceptChanges();

        void RejectChanges();

        T ViewModel { get; }
    }

    public class ViewModelProxy<T> : DynamicObject, IViewModelProxy<T>, IPropertyHandler
        where T : IViewModelBase
    {
        T _viewModel;
        Type _type;
        Dictionary<AccessorKey, object> _values;

        #region Enums
        enum FieldType
        {
            Property,
            Indexer
        }

        class AccessorKey
        {
            const string IndexerName = "Item";

            public string Name { get; set; }
            public FieldType FieldType { get; set; }
            public Type[] Types { get; set; }

            public static AccessorKey GetIndexerKey(Type[] types)
            {
                return new AccessorKey { Name = IndexerName, FieldType = FieldType.Indexer, Types = types };
            }

            public static AccessorKey GetPropertyKey(string name)
            {
                return new AccessorKey { Name = name, FieldType = FieldType.Property };
            }

            public override bool Equals(object obj)
            {
                AccessorKey key = obj as AccessorKey;
                if (key == null)
                {
                    return false;
                }

                return Object.Equals(Name, key.Name) && Object.Equals(FieldType, key.FieldType)
                    && (Object.Equals(Types, key.Types) || Types.SequenceEqual(key.Types));
            }

            public override int GetHashCode()
            {
                int result = Name.GetHashCode() ^ FieldType.GetHashCode();

                if (Types != null)
                {
                    result ^= Types.Select(t => t.GetHashCode()).Aggregate((i1, i2) => i1 ^ i2);
                }

                return result;
            }
        }
        #endregion

        public ViewModelProxy(T viewModel)
        {
            Contract.Requires(viewModel != null);

            _viewModel = viewModel;
            _viewModel.PropertyChanged += (s, e) => OnPropertyChanged(e);
            _type = _viewModel.GetType();

            _values = new Dictionary<AccessorKey, object>();
        }

        public void AcceptChanges()
        {
            foreach (var item in _values)
            {
                if (item.Key.FieldType == FieldType.Property)
                {
                    _type.GetProperty(item.Key.Name).SetValue(ViewModel, item.Value, null);
                }
            }

            _values.Clear();
        }

        public void RejectChanges()
        {
            var propNames = _values.Keys.ToList();

            _values.Clear();

            foreach (var item in propNames)
            {
                if (item.FieldType == FieldType.Property)
                {
                    OnPropertyChanged(item.Name);
                }
            }
        }

        public T ViewModel
        {
            get { return _viewModel; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotSupportedException(); }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get { return ValidateProperty(columnName); }
        }

        string ValidateProperty(string propertyName)
        {
            return _viewModel.ValidateProperty(propertyName, this);
        }

        public TOutput GetPropertyValue<TOutput>(string propertyName)
        {
            object result = null;
            AccessorKey key = AccessorKey.GetPropertyKey(propertyName);
            if (!_values.TryGetValue(key, out result))
            {
                result = _type.GetProperty(key.Name).GetValue(ViewModel, null);
            }

            return (TOutput)result;
        }

        public void SetPropertyValue<TOutput>(string propertyName, TOutput value)
        {
            AccessorKey key = AccessorKey.GetPropertyKey(propertyName);

            _values[key] = value;
            OnPropertyChanged(key.Name);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetPropertyValue<object>(binder.Name);

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            SetPropertyValue(binder.Name, value);

            return true;
        }
    }

    public static class ViewModelHelper
    {
        public static IViewModelProxy<T> CreatePropertyProxy<T>(this T viewModel)
            where T : IViewModelBase
        {
            return new ViewModelProxy<T>(viewModel);
        }
    }
}
