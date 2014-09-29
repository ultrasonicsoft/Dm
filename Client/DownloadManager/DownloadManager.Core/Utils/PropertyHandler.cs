using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Diagnostics.Contracts;

namespace Ultrasonic.DownloadManager.Core.Utils
{
    public interface IPropertyHandler
    {
        T GetPropertyValue<T>(string propertyName);

        void SetPropertyValue<T>(string propertyName, T value);
    }

    public class DelegatePropertyHandler : DynamicObject, IPropertyHandler
    {
        object _obj;
        Type _typeOfObj;

        public DelegatePropertyHandler(object obj)
        {
            Contract.Requires(obj != null);

            _obj = obj;
            _typeOfObj = _obj.GetType();
        }

        public T GetPropertyValue<T>(string propertyName)
        {
            return (T)_typeOfObj.GetProperty(propertyName).GetValue(_obj, null);
        }

        public void SetPropertyValue<T>(string propertyName, T value)
        {
            _typeOfObj.GetProperty(propertyName).SetValue(_obj, value, null);
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
}
