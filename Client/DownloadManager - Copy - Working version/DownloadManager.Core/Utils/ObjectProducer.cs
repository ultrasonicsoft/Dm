using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using System.Diagnostics.Contracts;
using System.Collections;

namespace Ultrasonic.DownloadManager.Core.Utils
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NonMergableAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SerializeByRefAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SerializeItemsByRefAttribute : Attribute
    {
    }

    public interface ICloneSerializeCallback
    {
        void SuspendDeserializationCallback();
        void ResumeDeserializationCallback();

        void OnDeserialized();
    }

    public static class ObjectProducer
    {
        #region Fields
        static BindingFlags findFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        static IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            foreach (var field in type.GetFields(findFlags))
            {
                yield return field;
            }

            var baseType = type.BaseType;
            if (baseType != null)
            {
                var baseFields = GetAllFields(baseType);

                foreach (var field in baseFields)
                    yield return field;
            }
        }
        #endregion

        #region Binary Serialization
        #region Helpers
        static Dictionary<string, BinarySerializeTypeDescription> serializeByRefFieldNames = new Dictionary<string, BinarySerializeTypeDescription>();

        class BinarySerializeTypeDescription
        {
            internal static readonly BinarySerializeTypeDescription Empty = new BinarySerializeTypeDescription(null, null, null);

            public IEnumerable<FieldInfo> ByRefFields { get; private set; }
            public IEnumerable<FieldInfo> ItemsByRefFields { get; private set; }
            public IEnumerable<string> NonSerializableFieldNames { get; private set; }

            public BinarySerializeTypeDescription(IEnumerable<FieldInfo> byRefFields, IEnumerable<FieldInfo> itemsByRefFields, IEnumerable<string> nonSerializableFieldNames)
            {
                ByRefFields = byRefFields;
                ItemsByRefFields = itemsByRefFields;
                NonSerializableFieldNames = nonSerializableFieldNames;
            }
        }

        class BinaryPathDescription
        {
            public FieldInfo Field { get; set; }

            public bool IsBinarySerialize { get; set; }

            public object ValueToSet { get; set; }

            public bool IsByRef { get; set; }

            public bool IsItemsByRef { get; set; }

            public object DictionaryKey { get; set; }

            public int ItemIndex { get; set; }

            public List<BinaryPathDescription> InnerPaths { get; set; }

            public bool IsActive { get; set; }

            BinaryPathDescription()
            {
                InnerPaths = new List<BinaryPathDescription>();
            }

            public BinaryPathDescription(Dictionary<object, bool> objectsStore, object container, FieldInfo containerField, object val, object dictionaryKey, int itemIndex)
                : this()
            {
                IsActive = true;

                Contract.Requires(val != null);

                Type fieldType = val.GetType();

                ItemIndex = itemIndex;
                DictionaryKey = dictionaryKey;
                Field = containerField;

                bool processFields = true;

                if (ItemIndex >= 0 || DictionaryKey != null)
                {
                    ValueToSet = val;
                    processFields = false;
                }
                else
                {
                    var desc = GetBinarySerializeTypeDescription(containerField.DeclaringType);
                    IsByRef = desc.ByRefFields != null && desc.ByRefFields.Contains(containerField);
                    IsItemsByRef = desc.ItemsByRefFields != null && desc.ItemsByRefFields.Contains(containerField);

                    if (!IsByRef && !IsItemsByRef && desc.NonSerializableFieldNames != null && desc.NonSerializableFieldNames.Contains(containerField.Name))
                    {
                        IsActive = false;
                        return;
                    }

                    IsBinarySerialize = typeof(ICloneSerializeCallback).IsAssignableFrom(fieldType);

                    if (IsBinarySerialize)
                    {
                        (val as ICloneSerializeCallback).SuspendDeserializationCallback();
                    }

                    if (IsByRef)
                    {
                        ValueToSet = val;
                        containerField.SetValue(container, null);
                        processFields = false;
                    }
                    else
                    {
                        IEnumerable enumerable = val as IEnumerable;
                        if (enumerable != null)
                        {
                            if (IsItemsByRef)
                            {
                                ValueToSet = val;
                                containerField.SetValue(container, null);
                                processFields = false;
                            }
                            else
                            {
                                IList list = enumerable as IList;
                                if (list != null)
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        var listItem = list[i];

                                        if (listItem != null && IsProcessFieldType(listItem.GetType()))
                                        {
                                            BinaryPathDescription innerDesc = new BinaryPathDescription(objectsStore, null, null, listItem, null, i);
                                            if (innerDesc.IsActive)
                                            {
                                                InnerPaths.Add(innerDesc);
                                            }
                                        }
                                    }

                                    processFields = false;
                                }
                                else
                                {
                                    IDictionary dictionary = enumerable as IDictionary;

                                    if (dictionary != null)
                                    {
                                        foreach (DictionaryEntry dictionaryItem in dictionary)
                                        {
                                            if (dictionaryItem.Value != null && IsProcessFieldType(dictionaryItem.Value.GetType()))
                                            {
                                                BinaryPathDescription innerDesc = new BinaryPathDescription(objectsStore, null, null, dictionaryItem.Value, dictionaryItem.Key, -1);
                                                if (innerDesc.IsActive)
                                                {
                                                    InnerPaths.Add(innerDesc);
                                                }
                                            }
                                        }

                                        processFields = false;
                                    }
                                }
                            }
                        }
                    }
                }

                if (processFields)
                {
                    var fields = GetFields(fieldType);
                    foreach (var field in fields)
                    {
                        object fieldVal = field.GetValue(val);

                        if (fieldVal != null)
                        {
                            bool found;
                            if (!objectsStore.TryGetValue(fieldVal, out found))
                            {
                                objectsStore.Add(fieldVal, true);

                                BinaryPathDescription innerDesc = new BinaryPathDescription(objectsStore, val, field, fieldVal, null, -1);
                                if (innerDesc.IsActive)
                                {
                                    InnerPaths.Add(innerDesc);
                                }
                            }
                            else
                            {
                                InnerPaths.Add(new BinaryPathDescription(val, field, fieldVal));
                            }
                        }
                    }
                }

                IsActive |= IsByRef || IsItemsByRef || InnerPaths.Count > 0;
            }

            public BinaryPathDescription(object container, FieldInfo containerField, object val)
                : this()
            {
                IsActive = true;
                IsByRef = true;
                ValueToSet = val;
                Field = containerField;
            }

            static Dictionary<Type, IEnumerable<FieldInfo>> _processFieldsCache = new Dictionary<Type, IEnumerable<FieldInfo>>();

            static IEnumerable<FieldInfo> GetFields(Type type)
            {
                IEnumerable<FieldInfo> fields;
                if (!_processFieldsCache.TryGetValue(type, out fields))
                {
                    fields = GetAllFields(type).Where(qf => IsProcessFieldType(qf.FieldType));
                    _processFieldsCache.Add(type, fields);
                }

                return fields;
            }

            static bool IsProcessFieldType(Type fieldType)
            {
                return !fieldType.IsValueType && fieldType != typeof(Enum) && fieldType != typeof(string);
            }

            public static List<BinaryPathDescription> CreateDescriptions(object container)
            {
                List<BinaryPathDescription> list = new List<BinaryPathDescription>();

                Type type = container.GetType();
                var objectsStore = new Dictionary<object, bool>();

                objectsStore.Add(container, true);

                var fields = GetFields(type);
                foreach (var field in fields)
                {
                    object fieldVal = field.GetValue(container);

                    if (fieldVal != null)
                    {
                        bool found;
                        if (!objectsStore.TryGetValue(fieldVal, out found))
                        {
                            objectsStore.Add(fieldVal, true);

                            BinaryPathDescription innerDesc = new BinaryPathDescription(objectsStore, container, field, fieldVal, null, -1);
                            if (innerDesc.IsActive)
                            {
                                list.Add(innerDesc);
                            }
                        }
                        else
                        {
                            list.Add(new BinaryPathDescription(container, field, fieldVal));
                        }
                    }
                }

                return list;
            }

            public void ProcessDeserializedState(object container, object clonedContainer)
            {
                if (clonedContainer != null)
                {
                    object finalVal = null;
                    object finalClonedVal = null;

                    if (IsByRef)
                    {
                        finalClonedVal = ValueToSet;
                        finalVal = ValueToSet;

                        Field.SetValue(clonedContainer, ValueToSet);
                        Field.SetValue(container, ValueToSet);
                    }
                    else
                    {
                        IEnumerable enumerable = ValueToSet as IEnumerable;

                        if (IsItemsByRef)
                        {
                            finalVal = ValueToSet;
                            finalClonedVal = InitObject(finalVal.GetType());

                            IList clonedList = finalClonedVal as IList;
                            if (clonedList != null)
                            {
                                IList list = finalVal as IList;
                                foreach (object item in list)
                                {
                                    clonedList.Add(item);
                                }
                            }
                            else
                            {
                                IDictionary clonedDictionary = finalClonedVal as IDictionary;

                                if (clonedDictionary != null)
                                {
                                    IDictionary dictionary = finalVal as IDictionary;
                                    foreach (DictionaryEntry item in dictionary)
                                    {
                                        clonedDictionary.Add(item.Key, item.Value);
                                    }
                                }
                                else
                                {
                                    //treat as by ref!
                                    finalClonedVal = finalVal;
                                }
                            }

                            Field.SetValue(clonedContainer, finalClonedVal);
                            Field.SetValue(container, finalVal);
                        }
                        else if (ItemIndex >= 0)
                        {
                            IList list = container as IList;
                            IList clonedList = clonedContainer as IList;

                            finalVal = list[ItemIndex];
                            finalClonedVal = clonedList[ItemIndex];
                        }
                        else if (DictionaryKey != null)
                        {
                            IDictionary dictionary = container as IDictionary;
                            IDictionary clonedDictionary = clonedContainer as IDictionary;

                            finalVal = dictionary[DictionaryKey];
                            finalClonedVal = clonedDictionary[DictionaryKey];
                        }
                        else
                        {
                            finalClonedVal = Field.GetValue(clonedContainer);
                            finalVal = Field.GetValue(container);
                        }
                    }

                    if (InnerPaths != null && InnerPaths.Count > 0)
                    {
                        foreach (var path in InnerPaths)
                        {
                            path.ProcessDeserializedState(finalVal, finalClonedVal);
                        }
                    }

                    if (IsBinarySerialize && finalClonedVal != null)
                    {
                        ICloneSerializeCallback objSer = finalVal as ICloneSerializeCallback;
                        ICloneSerializeCallback objClonedSer = finalClonedVal as ICloneSerializeCallback;

                        if (objSer != null)
                        {
                            objSer.ResumeDeserializationCallback();
                            objSer.OnDeserialized();
                        }

                        if (objClonedSer != null && !Object.ReferenceEquals(objSer, objClonedSer))
                        {
                            objClonedSer.ResumeDeserializationCallback();
                            objClonedSer.OnDeserialized();
                        }
                    }
                }
            }
        }

        static List<IEnumerable<FieldInfo>> GetSerializeByRefFields(Type type)
        {
            List<IEnumerable<FieldInfo>> refNames = new List<IEnumerable<FieldInfo>>();

            Type t = type;
            while (t != null)
            {
                BinarySerializeTypeDescription desc = GetBinarySerializeTypeDescription(t);

                if (desc.ByRefFields != null)
                {
                    refNames.Add(desc.ByRefFields);
                }

                t = t.BaseType;
            }

            return refNames;
        }

        static BinarySerializeTypeDescription GetBinarySerializeTypeDescription(Type type)
        {
            BinarySerializeTypeDescription desc;
            if (!serializeByRefFieldNames.TryGetValue(type.FullName, out desc))
            {
                List<FieldInfo> refList = null;
                List<FieldInfo> itemsRefList = null;
                List<string> nonSerialized = null;

                bool match = false;

                var fields = type.GetFields(findFlags);
                foreach (var fieldRef in fields.Where(f => f.GetCustomAttributes(typeof(SerializeByRefAttribute), false).Length > 0))
                {
                    if (refList == null)
                    {
                        refList = new List<FieldInfo>();
                        match = true;
                    }

                    refList.Add(fieldRef);
                }

                foreach (var fieldRef in fields.Where(f => f.GetCustomAttributes(typeof(SerializeItemsByRefAttribute), false).Length > 0))
                {
                    if (itemsRefList == null)
                    {
                        itemsRefList = new List<FieldInfo>();
                        match = true;
                    }

                    itemsRefList.Add(fieldRef);
                }

                foreach (var fieldRef in fields.Where(f => f.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length > 0))
                {
                    if (nonSerialized == null)
                    {
                        nonSerialized = new List<string>();
                        match = true;
                    }

                    nonSerialized.Add(fieldRef.Name);
                }

                desc = !match ? BinarySerializeTypeDescription.Empty : new BinarySerializeTypeDescription(refList, itemsRefList, nonSerialized);
                serializeByRefFieldNames.Add(type.FullName, desc);
            }

            return desc;
        }
        #endregion

        public static T CloneByBinarySerialize<T>(T obj)
            where T : class
        {
            Contract.Requires(obj != null);

            Type t = obj.GetType();

            List<BinaryPathDescription> pathDescs = BinaryPathDescription.CreateDescriptions(obj);

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                ICloneSerializeCallback objSer = obj as ICloneSerializeCallback;

                bool isBinary = objSer != null;

                if (isBinary)
                {
                    objSer.SuspendDeserializationCallback();
                }

                formatter.Serialize(stream, obj);

                if (isBinary)
                {
                    objSer.ResumeDeserializationCallback();
                }

                stream.Position = 0;
                T other = (T)formatter.Deserialize(stream);

                foreach (var pathDesc in pathDescs)
                {
                    pathDesc.ProcessDeserializedState(obj, other);
                }

                if (isBinary)
                {
                    objSer = other as ICloneSerializeCallback;

                    objSer.ResumeDeserializationCallback();
                    objSer.OnDeserialized();
                }

                return other;
            }
        }
        #endregion

        #region Helpers
        static object InitObject(Type type)
        {
            var ctor = type.GetConstructor(findFlags, null, Type.EmptyTypes, null);
            if (ctor == null)
            {
                throw new InvalidOperationException(
                    string.Format("The object can't be cloned, {0} doesn't have a parameter-less constructor", type.FullName));
            }

            return ctor.Invoke(null);
        }
        #endregion

        #region Shallow Clone
        public static T ShallowClone<T>(T obj)
            where T : class
        {
            Contract.Requires(obj != null);

            Type typeOfObj = obj.GetType();

            T clone = (T)InitObject(typeOfObj);

            var fields = GetAllFields(typeOfObj);

            foreach (FieldInfo field in fields)
            {
                field.SetValue(clone, field.GetValue(obj));
            }

            return clone;
        }
        #endregion

        #region Shallow Merge
        public static void ShallowMerge<T>(T obj, T source)
            where T : class
        {
            Contract.Requires(obj != null);
            Contract.Requires(source != null);

            var fields = GetAllFields(obj.GetType());

            foreach (FieldInfo field in fields)
            {
                field.SetValue(obj, field.GetValue(source));
            }
        }
        #endregion
    }
}
