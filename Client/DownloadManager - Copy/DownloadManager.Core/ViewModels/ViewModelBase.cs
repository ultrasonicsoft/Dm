using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Ultrasonic.DownloadManager.Core.Utils;
using System.Linq.Expressions;
using System.Windows.Input;
using Ultrasonic.DownloadManager.Controls.Commands;
using System.Diagnostics;
using System.Reflection;

namespace Ultrasonic.DownloadManager.Core.ViewModels
{
    [Serializable]
    public class ViewModelBase : IViewModelBase, ICloneSerializeCallback
    {
        #region Fields

        [NonSerialized]
        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        [NonSerialized]
        DelegatePropertyHandler _propertyHandler;

        #endregion

        #region Init

        public ViewModelBase()
        {
            OnInit(false);
        }

        [System.Runtime.Serialization.OnDeserialized]
        void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            if (!_isDeserializeSuspended)
            {
                OnInit(true);
            }
        }

        void OnInit(bool deserializing)
        {
            _subscribedNotifiers = new Lazy<List<IDependencyNotifier>>();
            _dependencyDescription = GetDependencyDescription();

            _propertyHandler = new DelegatePropertyHandler(this);

            InitializeCore(deserializing);
        }

        protected virtual void InitializeCore(bool deserializing)
        {
        }

        #endregion

        #region ICloneSerializeCallback

        bool _isDeserializeSuspended;

        void ICloneSerializeCallback.SuspendDeserializationCallback()
        {
            _isDeserializeSuspended = true;
        }

        void ICloneSerializeCallback.ResumeDeserializationCallback()
        {
            _isDeserializeSuspended = false;
        }

        void ICloneSerializeCallback.OnDeserialized()
        {
            OnInit(true);
        }

        #endregion

        #region IViewModelBase
        public string Error
        {
            get { throw new NotSupportedException(); }
        }

        public string this[string columnName]
        {
            get { return ((IViewModelBase)this).ValidateProperty(columnName, _propertyHandler); }
        }

        string IViewModelBase.ValidateProperty(string propertyName, IPropertyHandler propertyHandler)
        {
            return ValidatePropertyCore(propertyName, propertyHandler);
        }

        protected virtual string ValidatePropertyCore(string propertyName, IPropertyHandler propertyHandler)
        {
            return null;
        }
        #endregion

        #region IRaiseNotifyPropertyChanged
        void IRaiseNotifyPropertyChanged.RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));

                ProcessFlatDependencies(propertyName);
            }
        }

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        protected void VerifyPropertyName(string propertyName)
        {
            if (!propertyName.Equals(String.Empty) && // Empty string represents that all properties have been changed
                TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;
                throw new ArgumentException(msg);
            }
        }

        protected void OnPropertyChanged(MethodBase method)
        {
            OnPropertyChanged(method.Name.Substring("set_".Length));
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event for the specified property.
        /// <example>
        /// Usage:
        /// <code>
        /// OnPropertyChanged((MyClass o) => o.MyProperty);
        /// - or -
        /// OnPropertyChanged&lt;MyClass&gt;(o => o.MyProperty);
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        protected void OnPropertyChanged<TSource>(Expression<Func<TSource, object>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            var memberExpr = propertyExpression.Body as MemberExpression;
            if (memberExpr == null)
            {
                var convertExpr = propertyExpression.Body as UnaryExpression;
                if (convertExpr != null)
                {
                    memberExpr = convertExpr.Operand as MemberExpression;
                }
            }
            if (memberExpr == null || memberExpr.Expression != propertyExpression.Parameters[0] || memberExpr.Member.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("Invalid property expression.", "propertyExpression");
            }
            OnPropertyChanged(memberExpr.Member.Name);
        }

        /// <summary>
        /// Sets the field and invokes the <see cref="PropertyChanged"/> event
        /// if the new value is different from the current one.
        /// <remarks>
        /// Use <see cref="MethodBase.GetCurrentMethod"/> to obtain the current method,
        /// which is used to resolve the property name.
        /// </remarks>
        /// <example>
        /// Usage:
        /// <code>
        /// private string myPropertyField;
        /// public string MyProperty
        /// {
        /// get { return myPropertyField; }
        /// set { SetProperty(MethodBase.GetCurrentMethod(), ref myPropertyField, value); }
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        protected void SetProperty<TValue>(MethodBase method, ref TValue field, TValue value)
        {
            SetProperty(method, ref field, value, true);
        }

        /// <summary>
        /// Sets the field and invokes the <see cref="PropertyChanged"/> event.
        /// <remarks>
        /// Use <see cref="MethodBase.GetCurrentMethod"/> to obtain the current method,
        /// which is used to resolve the property name.
        /// </remarks>
        /// <example>
        /// Usage:
        /// <code>
        /// private string myPropertyField;
        /// public string MyProperty
        /// {
        /// get { return myPropertyField; }
        /// set { SetProperty(MethodBase.GetCurrentMethod(), ref myPropertyField, value); }
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="checkEquality">if set to <c>true</c>, the property is set only if the new value is different from the current.</param>
        protected void SetProperty<TValue>(MethodBase method, ref TValue field, TValue value, bool checkEquality)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (!checkEquality || !Equals(field, value))
            {
                field = value;
                OnPropertyChanged(method);
            }
        }

        /// <summary>
        /// Sets the model's property and invokes the <see cref="PropertyChanged"/> event.
        /// <remarks>
        /// Use <see cref="MethodBase.GetCurrentMethod"/> to obtain the current method,
        /// which is used to resolve the property name.
        /// </remarks>
        /// <example>
        /// Usage:
        /// <code>
        /// private Model _model;
        /// public string MyProperty
        /// {
        /// get { return myPropertyField; }
        /// set { SetProperty(MethodBase.GetCurrentMethod(), value, _model.MyProperty, v => _model.MyProperty = v); }
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="propertySetter">The property setter.</param>
        protected void SetProperty<TValue>(MethodBase method, TValue newValue, TValue oldValue, Action<TValue> propertySetter)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (!Equals(newValue, oldValue))
            {
                propertySetter(newValue);
                OnPropertyChanged(method);
            }
        }

        #endregion

        #region Dependencies

        #region Dependency Helper Classes

        class CompositeTypeDependencyDescription
        {
            readonly List<TypeDependencyDescription> _descriptions;
            readonly bool _hasData;

            public IEnumerable<Dictionary<string, List<PropertyDependency>>> FlatBoundProperties
            {
                get
                {
                    return _hasData
                        ? _descriptions.Select(d => d.FlatBoundProperties)
                        : Enumerable.Empty<Dictionary<string, List<PropertyDependency>>>();
                }
            }

            public CompositeTypeDependencyDescription(Type sourceType, IDictionary<Type, TypeDependencyDescription> dependencyStore)
            {
                _descriptions = new List<TypeDependencyDescription>();

                while (sourceType != null)
                {
                    TypeDependencyDescription desc;
                    if (dependencyStore.TryGetValue(sourceType, out desc))
                    {
                        _hasData = true;
                        _descriptions.Add(desc);
                    }

                    sourceType = sourceType.BaseType;
                }
            }

            public IEnumerable<Tuple<PropertyDependency, IEnumerable<IDependencyNotifier>>> CreateNotifiers(object source)
            {
                return _hasData
                    ? _descriptions.SelectMany(d => d.CreateNotifiers(source))
                    : Enumerable.Empty<Tuple<PropertyDependency, IEnumerable<IDependencyNotifier>>>();
            }
        }

        class TypeDependencyDescription
        {
            readonly List<PropertyDependency> _allPropertyDependencyPaths;

            public Dictionary<string, List<PropertyDependency>> FlatBoundProperties { get; private set; }

            public TypeDependencyDescription()
            {
                _allPropertyDependencyPaths = new List<PropertyDependency>();
                FlatBoundProperties = new Dictionary<string, List<PropertyDependency>>();
            }

            public IPropertyDependency<TSource> CreateDependency<TSource, TProperty>(Expression<Func<TSource, TProperty>> property)
            {
                string propName = ExpressionHelper.GetPropertyName(property);

                var dependency = new PropertyDependency<TSource>();

                if (dependency.LoadDependentProperty(propName, property))
                {
                    _allPropertyDependencyPaths.Add(dependency);

                    dependency.UpdateAction = SynchronizePath;
                }

                return dependency;
            }

            public IPropertyDependency<TSource> CreateDependency<TSource>(Expression<Action<TSource>> callback)
            {
                var dependency = new PropertyDependency<TSource>();

                _allPropertyDependencyPaths.Add(dependency);
                dependency.UpdateAction = SynchronizePath;

                dependency.Invoke(callback);

                return dependency;
            }

            private void SynchronizePath(PropertyDependency dependency, SourcePathDescription path)
            {
                if (path.Descriptions.Count == 1)
                {
                    string dependsOn = path.Descriptions[0].NotifyPropertyName;
                    List<PropertyDependency> dependentPropertyNames;
                    if (!FlatBoundProperties.TryGetValue(dependsOn, out dependentPropertyNames))
                    {
                        dependentPropertyNames = new List<PropertyDependency>();
                        FlatBoundProperties.Add(dependsOn, dependentPropertyNames);
                    }

                    dependentPropertyNames.Add(dependency);
                }
            }

            public IEnumerable<Tuple<PropertyDependency, IEnumerable<IDependencyNotifier>>> CreateNotifiers(object source)
            {
                return _allPropertyDependencyPaths
                    .Select(p => Tuple.Create(p, p.CreateNotifiers(source)));
            }
        }

        abstract class PropertyDependency
        {
            public bool IsNotification { get; protected set; }
            public string DependentPropertyName { get; protected set; }
            protected Type SourceType { get; set; }
            public bool IsCommand { get; protected set; }
            public abstract Func<object, object> ExtractMember { get; }

            protected List<SourcePathDescription> BoundPaths { get; private set; }
            protected List<IDependencyProvider> CustomNotifiers { get; private set; }

            internal Action<PropertyDependency, SourcePathDescription> UpdateAction { get; set; }

            protected PropertyDependency()
            {
                BoundPaths = new List<SourcePathDescription>();
                CustomNotifiers = new List<IDependencyProvider>();
            }

            public abstract IEnumerable<IDependencyNotifier> CreateNotifiers(object source);
            public abstract void ExecuteInvocations(object source);
        }

        class PropertyDependency<TType> : PropertyDependency, IPropertyDependency<TType>
        {
            Func<object, object> _extractMember;
            public override Func<object, object> ExtractMember
            {
                get { return _extractMember; }
            }

            private readonly Lazy<List<Delegate>> _invocationsLazy;

            public PropertyDependency()
            {
                SourceType = typeof(TType);

                _invocationsLazy = new Lazy<List<Delegate>>();
            }

            public bool LoadDependentProperty<TProperty>(string propertyName, Expression<Func<TType, TProperty>> dependentProperty)
            {
                IsNotification = true;
                DependentPropertyName = propertyName;

                var propertyInfo = SourceType.GetProperty(DependentPropertyName);
                //if (propertyInfo.DeclaringType != SourceType)
                //{
                //    return false;
                //}

                IsCommand = typeof(ICommand).IsAssignableFrom(propertyInfo.PropertyType);

                if (IsCommand)
                {
                    var getProp = dependentProperty.Compile();
                    _extractMember = o => getProp((TType)o);
                }

                return true;
            }

            public IPropertyDependency<TType> DependsOn<TResult>(params Expression<Func<TType, TResult>>[] propertyExpressions)
            {
                return DependsOn((IEnumerable<Expression<Func<TType, TResult>>>)propertyExpressions);
            }

            public IPropertyDependency<TType> DependsOn<TResult>(IEnumerable<Expression<Func<TType, TResult>>> propertyExpressions)
            {
                if (propertyExpressions == null)
                {
                    throw new ArgumentNullException("propertyExpressions");
                }

                foreach (var item in propertyExpressions)
                {
                    var path = SourcePathDescription.Create(item);
                    BoundPaths.Add(path);

                    if (UpdateAction != null)
                    {
                        UpdateAction(this, path);
                    }
                }

                return this;
            }

            public IPropertyDependency<TType> DependsOnCollection(Expression<Func<TType, IEnumerable>> dependsOn, bool notifyOnItemChanges = false)
            {
                var path = SourcePathDescription.Create(dependsOn);

                CustomNotifiers.Add(new CollectionDependencyProvider(path, notifyOnItemChanges));

                return this;
            }

            public IPropertyDependency<TType> DependsOnCollection<TItem>(Expression<Func<TType, IEnumerable>> dependsOn, params Expression<Func<TItem, object>>[] itemDependencies)
            {
                var path = SourcePathDescription.Create(dependsOn);

                var itemPaths = itemDependencies.Select(SourcePathDescription.Create).ToList();
                CustomNotifiers.Add(new CollectionDependencyProvider(path, itemPaths));

                return this;
            }

            public IPropertyDependency<TType> DependsOnCollection<TItem>(Expression<Func<TType, IEnumerable<TItem>>> dependsOn, params Expression<Func<TItem, object>>[] itemDependencies)
            {
                var path = SourcePathDescription.Create(dependsOn);

                var itemPaths = itemDependencies.Select(SourcePathDescription.Create).ToList();
                CustomNotifiers.Add(new CollectionDependencyProvider(path, itemPaths));

                return this;
            }

            public IPropertyDependency<TType> Invoke(Expression<Action<TType>> action)
            {
                _invocationsLazy.Value.Add(action.Compile());
                return this;
            }

            public IPropertyDependency<TType> DependsOn<TResult>(IDependencyProvider provider)
            {
                CustomNotifiers.Add(provider);
                return this;
            }

            public override IEnumerable<IDependencyNotifier> CreateNotifiers(object source)
            {
                IEnumerable<IDependencyNotifier> notifiers = BoundPaths
                .Where(p => p.Descriptions.Count > 1)
                .Select(p =>
                {
                    var n = (IDependencyNotifier)new ObjectChangedNotifier(p);

                    n.AssociatedObject = source;

                    return n;
                });

                if (CustomNotifiers.Count > 0)
                {
                    notifiers = notifiers.Concat(CustomNotifiers.Select(p =>
                    {
                        var n = p.CreateNotifier();

                        n.AssociatedObject = source;

                        return n;
                    }));
                }

                return notifiers;
            }

            public override void ExecuteInvocations(object source)
            {
                if (_invocationsLazy.IsValueCreated)
                {
                    _invocationsLazy.Value.ForEach(d => d.DynamicInvoke(source));
                }
            }
        }

        class SingleSourcePathDescription
        {
            public string NotifyPropertyName { get; internal set; }
            public Func<object, object> ExtractMemberMethod { get; private set; }
            private string Display { get; set; }
            public bool IsNotifier { get; private set; }

            private SingleSourcePathDescription(Func<object, object> extractMember, string notifyPropertyName, string display, bool isNotifier)
            {
                ExtractMemberMethod = extractMember;
                NotifyPropertyName = notifyPropertyName;
                Display = display;
                IsNotifier = isNotifier;
            }

            internal static SingleSourcePathDescription Create<TContainer, TObject>(Expression<Func<TContainer, TObject>> sourcePathExpression)
            {
                if (sourcePathExpression == null)
                {
                    throw new ArgumentNullException("sourcePathExpression");
                }

                var memExpression = sourcePathExpression.Body as MemberExpression;
                return Create(sourcePathExpression, memExpression);
            }

            internal static SingleSourcePathDescription Create<TContainer, TObject>(Expression<Func<TContainer, TObject>> sourcePathExpression, MemberExpression memberExpression)
            {
                if (sourcePathExpression == null)
                {
                    throw new ArgumentNullException("sourcePathExpression");
                }
                if (memberExpression == null)
                {
                    throw new ArgumentNullException("memberExpression");
                }

                Delegate extractMember = System.Linq.Expressions.Expression.Lambda(
                    memberExpression, sourcePathExpression.Parameters[0]).Compile();
                Func<object, object> extractMemberFunc = o => extractMember.DynamicInvoke(o);

                return new SingleSourcePathDescription(extractMemberFunc, memberExpression.Member.Name,
                    memberExpression.ToString(), typeof(INotifyPropertyChanged).IsAssignableFrom(memberExpression.Type));
            }
        }

        class SourcePathDescription
        {
            private readonly List<SingleSourcePathDescription> _descriptions;
            public ReadOnlyCollection<SingleSourcePathDescription> Descriptions
            {
                get { return _descriptions.AsReadOnly(); }
            }

            public Type AssignableType { get; private set; }

            private SourcePathDescription(List<SingleSourcePathDescription> descriptions, Type assignableType)
            {
                _descriptions = descriptions;
                AssignableType = assignableType;
            }

            public static SourcePathDescription Create<TContainer, TSource>(Expression<Func<TContainer, TSource>> sourcePathExpression)
            {
                if (sourcePathExpression == null)
                {
                    throw new ArgumentNullException("sourcePathExpression");
                }
                if (sourcePathExpression.Parameters.Count != 1)
                {
                    throw new ArgumentException("Incompatible parameter count", "sourcePathExpression");
                }

                var descs = new List<SingleSourcePathDescription>();
                var propNames = new List<string>();

                var memberExpression = TryGetMemberExpression(sourcePathExpression.Body);
                while (memberExpression != null)
                {
                    SingleSourcePathDescription desc = SingleSourcePathDescription.Create(sourcePathExpression, memberExpression);
                    descs.Add(desc);
                    propNames.Add(desc.NotifyPropertyName);

                    memberExpression = TryGetMemberExpression(memberExpression.Expression);
                }

                int count = descs.Count;
                for (int i = 0; i < count; i++)
                {
                    descs[i].NotifyPropertyName = i == 0 ? propNames[count - 1] : propNames[i - 1];
                }

                descs.Reverse();

                return new SourcePathDescription(descs, sourcePathExpression.Parameters[0].Type);
            }

            private static MemberExpression TryGetMemberExpression(System.Linq.Expressions.Expression expression)
            {
                var member = expression as MemberExpression;

                if (member == null)
                {
                    var unary = expression as UnaryExpression;
                    if (unary != null && unary.NodeType == ExpressionType.Convert)
                    {
                        member = TryGetMemberExpression(unary.Operand);
                    }
                }

                return member;
            }
        }

        class ObjectChangedNotifier : DependencyNotifier, IWeakEventListener
        {
            #region Fields
            bool _internalChange;
            WeakReference _containerRef;
            readonly List<WeakReference> _boundContainers;
            #endregion

            #region Properties
            protected SourcePathDescription DescriptionPath { get; private set; }
            #endregion

            #region Constructors
            public ObjectChangedNotifier(SourcePathDescription sourcePathDescription)
            {
                if (sourcePathDescription == null || sourcePathDescription.Descriptions.Count == 0)
                {
                    throw new ArgumentException("containerDescription");
                }

                DescriptionPath = sourcePathDescription;

                _boundContainers = new List<WeakReference>();
            }
            #endregion

            protected override void OnAssociatedObjectChanged()
            {
                if (!_internalChange)
                {
                    object container = AssociatedObject;

                    if (container == null)
                    {
                        throw new ArgumentNullException("container");
                    }

                    if (!DescriptionPath.AssignableType.IsInstanceOfType(container))
                    {
                        throw new ArgumentException("Incompatible container type", "container");
                    }

                    _containerRef = new WeakReference(container);

                    BindContainerPath(container, 0);

                    PrepareAssociatedObject();

                    try
                    {
                        _internalChange = true;

                        AssociatedObject = null;
                    }
                    finally
                    {
                        _internalChange = false;
                    }
                }
            }

            protected virtual void PrepareAssociatedObject()
            {
            }

            #region Property Bindings
            void BindContainerPath(object container, int startIndex)
            {
                bool boundContainers = _boundContainers.Count > 0;

                for (int i = startIndex; i < DescriptionPath.Descriptions.Count; i++)
                {
                    SingleSourcePathDescription desc = DescriptionPath.Descriptions[i];

                    if (boundContainers)
                    {
                        WeakReference prevContainerRef = _boundContainers[i];

                        if (prevContainerRef != null)
                        {
                            object prevContainer = prevContainerRef.Target;
                            if (prevContainer != null)
                            {
                                UnbindPropertyChanged(prevContainer, desc.NotifyPropertyName);
                            }
                        }
                    }

                    object currentContainer = null;
                    if (i == DescriptionPath.Descriptions.Count - 1)
                    {
                        BindPropertyChanged(container, desc.NotifyPropertyName);
                        currentContainer = container;
                    }
                    else
                    {
                        if (desc.IsNotifier)
                        {
                            try
                            {
                                currentContainer = desc.ExtractMemberMethod(container);
                                BindPropertyChanged(currentContainer, desc.NotifyPropertyName);
                            }
                            catch { }
                        }
                    }

                    if (!boundContainers)
                    {
                        _boundContainers.Add(new WeakReference(currentContainer));
                    }
                    else
                    {
                        _boundContainers[i] = new WeakReference(currentContainer);
                    }
                }
            }

            void BindPropertyChanged(object source, params string[] notifyPropertyNames)
            {
                BindPropertyChanged(source, (IEnumerable<string>)notifyPropertyNames);
            }

            void BindPropertyChanged(object source, IEnumerable<string> notifyPropertyNames)
            {
                if (source != null)
                {
                    var sourceNotify = source as INotifyPropertyChanged;

                    if (sourceNotify != null)
                    {
                        foreach (var property in notifyPropertyNames)
                        {
                            PropertyChangedEventManager.AddListener(sourceNotify, this, property);
                        }
                    }
                }
            }

            void UnbindPropertyChanged(object source, params string[] notifyPropertyNames)
            {
                UnbindPropertyChanged(source, (IEnumerable<string>)notifyPropertyNames);
            }

            void UnbindPropertyChanged(object source, IEnumerable<string> notifyPropertyNames)
            {
                if (source != null)
                {
                    var sourceNotify = source as INotifyPropertyChanged;

                    if (sourceNotify != null)
                    {
                        foreach (var property in notifyPropertyNames)
                        {
                            PropertyChangedEventManager.RemoveListener(sourceNotify, this, property);
                        }
                    }
                }
            }

            bool TryFindContainerDescription(object container, object sender, string propName, out SingleSourcePathDescription desc, out int matchIndex)
            {
                int count = DescriptionPath.Descriptions.Count;

                if (ReferenceEquals(container, sender))
                {
                    desc = DescriptionPath.Descriptions[count - 1];
                    matchIndex = count - 1;
                    return true;
                }

                for (int i = 0; i < count - 1; i++)
                {
                    SingleSourcePathDescription checkDesc = DescriptionPath.Descriptions[i];

                    if (checkDesc.IsNotifier && ReferenceEquals(checkDesc.ExtractMemberMethod(container), sender) && checkDesc.NotifyPropertyName == propName)
                    {
                        desc = checkDesc;
                        matchIndex = i;
                        return true;
                    }
                }

                desc = null;
                matchIndex = -1;
                return false;
            }

            public virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                var propArgs = e as PropertyChangedEventArgs;

                if (propArgs != null && _containerRef != null)
                {
                    object container;
                    if (TryGetSource(out container))
                    {
                        SingleSourcePathDescription containerDesc;
                        int matchIndex;
                        if (TryFindContainerDescription(container, sender, propArgs.PropertyName, out containerDesc, out matchIndex))
                        {
                            if (matchIndex > 0)
                            {
                                BindContainerPath(container, matchIndex - 1);
                            }

                            OnDependencyChanged();
                        }
                    }
                }

                return true;
            }

            protected bool TryGetSource(out object source)
            {
                source = _containerRef.Target;

                return source != null && _containerRef.IsAlive;
            }
            #endregion
        }

        class ObjectSourceNotifier : DependencyNotifier, IDisposable, IWeakEventListener
        {
            private bool _internalChange;
            private readonly bool _hasItemPaths;
            private ObjectChangedNotifier[] _pathChangedNotifiers;
            readonly IList<SourcePathDescription> _itemPaths;
            WeakReference _sourceRef;
            bool _isSubscribed;

            public ObjectSourceNotifier(IList<SourcePathDescription> itemPaths)
            {
                _hasItemPaths = itemPaths != null && itemPaths.Count > 0;
                _itemPaths = itemPaths;
            }

            protected override void OnAssociatedObjectChanged()
            {
                if (!_internalChange && AssociatedObject != null)
                {
                    _sourceRef = new WeakReference(AssociatedObject);

                    if (_hasItemPaths)
                    {
                        _pathChangedNotifiers = _itemPaths
                            .Select(p =>
                            {
                                var n = new ObjectChangedNotifier(p) {AssociatedObject = AssociatedObject};

                                n.DependencyChanged += OnInnerDependencyChanged;

                                return n;
                            }).ToArray();
                    }
                    else
                    {
                        var o = AssociatedObject as INotifyPropertyChanged;
                        if (o != null)
                        {
                            _isSubscribed = true;
                            PropertyChangedEventManager.AddListener(o, this, string.Empty);
                        }
                    }

                    try
                    {
                        _internalChange = true;

                        AssociatedObject = null;
                    }
                    finally
                    {
                        _internalChange = false;
                    }
                }
            }

            void OnInnerDependencyChanged(object sender, EventArgs e)
            {
                OnDependencyChanged();
            }

            public void Dispose()
            {
                if (_pathChangedNotifiers != null)
                {
                    foreach (var item in _pathChangedNotifiers)
                    {
                        item.DependencyChanged -= OnInnerDependencyChanged;
                    }
                }

                _pathChangedNotifiers = null;

                if (_isSubscribed)
                {
                    var source = _sourceRef.Target as INotifyPropertyChanged;

                    if (source != null)
                    {
                        PropertyChangedEventManager.RemoveListener(source, this, string.Empty);
                    }
                }
            }

            bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                var propArgs = e as PropertyChangedEventArgs;
                if (propArgs != null)
                {
                    OnDependencyChanged();
                }

                return true;
            }

            public bool TryGetSource(out object source)
            {
                source = _sourceRef.Target;
                return source != null;
            }
        }

        class CollectionDependencyNotifier : ObjectChangedNotifier
        {
            #region Fields

            readonly object _syncLock = new object();
            bool _internalUpdate;
            readonly bool _shouldSubscribe;
            WeakReference _boundCollection;
            private readonly List<ObjectSourceNotifier> _subscribedItems;
            readonly IList<SourcePathDescription> _itemPaths;

            #endregion

            #region Constructors
            public CollectionDependencyNotifier(SourcePathDescription sourcePathDescription, bool notifyOnItemChanges, IList<SourcePathDescription> itemPaths)
                : base(sourcePathDescription)
            {
                if (notifyOnItemChanges || (itemPaths != null && itemPaths.Count > 0))
                {
                    _shouldSubscribe = true;
                    _subscribedItems = new List<ObjectSourceNotifier>();
                }

                _itemPaths = itemPaths;
            }
            #endregion

            protected override void PrepareAssociatedObject()
            {
                base.PrepareAssociatedObject();

                OnSourcePathChanged();
            }

            protected override void OnDependencyChanged()
            {
                if (!_internalUpdate)
                {
                    OnSourcePathChanged();
                }

                base.OnDependencyChanged();
            }

            private void OnSourcePathChanged()
            {
                if (_shouldSubscribe)
                {
                    lock (_syncLock)
                    {
                        OnCollectionReset();
                    }
                }

                INotifyCollectionChanged col;
                if (TryGetBoundCollection(out col))
                {
                    CollectionChangedEventManager.RemoveListener(col, this);
                }

                if (TryGetDependency(out col))
                {
                    CollectionChangedEventManager.AddListener(col, this);

                    ProcessExistingItems(col);
                }

                _boundCollection = col == null ? null : new WeakReference(col);
            }

            private void ProcessExistingItems(INotifyCollectionChanged col)
            {
                if (_shouldSubscribe)
                {
                    var collection = col as IEnumerable;
                    if (collection != null)
                    {
                        lock (_syncLock)
                        {
                            foreach (var item in collection.OfType<INotifyPropertyChanged>())
                            {
                                StartListening(item);
                            }
                        }
                    }
                }
            }

            private bool TryGetBoundCollection(out INotifyCollectionChanged collection)
            {
                collection = null;

                if (_boundCollection == null)
                {
                    return false;
                }

                collection = _boundCollection.Target as INotifyCollectionChanged;

                return collection != null;
            }

            bool TryGetDependency<T>(out T dependency)
            {
                dependency = default(T);

                object source;
                if (TryGetSource(out source))
                {
                    try
                    {
                        dependency = (T)DescriptionPath.Descriptions.Last().ExtractMemberMethod(source);

                        return dependency != null;
                    }
                    catch { }
                }

                return false;
            }

            private void OnCollectionReset()
            {
                foreach (var itemRef in _subscribedItems)
                {
                    StopListening(itemRef);
                }

                _subscribedItems.Clear();
            }

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                _internalUpdate = true;

                try
                {
                    if (_shouldSubscribe)
                    {
                        lock (_syncLock)
                        {
                            if (e.Action != NotifyCollectionChangedAction.Reset)
                            {
                                if (e.OldItems != null)
                                {
                                    foreach (var item in e.OldItems.OfType<INotifyPropertyChanged>())
                                    {
                                        StopListening(item);
                                    }
                                }

                                if (e.NewItems != null)
                                {
                                    foreach (var item in e.NewItems.OfType<INotifyPropertyChanged>())
                                    {
                                        StartListening(item);
                                    }
                                }
                            }
                            else
                            {
                                OnCollectionReset();

                                ProcessExistingItems(sender as INotifyCollectionChanged);
                            }
                        }
                    }

                    OnDependencyChanged();
                }
                finally
                {
                    _internalUpdate = false;
                }
            }

            void OnItemDependencyChanged(object sender, EventArgs e)
            {
                try
                {
                    _internalUpdate = true;

                    OnDependencyChanged();
                }
                finally
                {
                    _internalUpdate = false;
                }
            }

            private void StartListening(INotifyPropertyChanged item)
            {
                var notifier = new ObjectSourceNotifier(_itemPaths) {AssociatedObject = item};

                notifier.DependencyChanged += OnItemDependencyChanged;

                _subscribedItems.Add(notifier);
            }

            private void StopListening(INotifyPropertyChanged item)
            {
                int index;
                ObjectSourceNotifier notifier;
                if (TryFindItemNotifier(item, out notifier, out index))
                {
                    _subscribedItems.RemoveAt(index);
                    StopListening(notifier);
                }
            }

            private void StopListening(ObjectSourceNotifier notifier)
            {
                notifier.Dispose();
                notifier.DependencyChanged -= OnItemDependencyChanged;
            }

            private bool TryFindItemNotifier(INotifyPropertyChanged item, out ObjectSourceNotifier notifier, out int index)
            {
                index = -1;
                foreach (var subscribedNotifier in _subscribedItems)
                {
                    index++;

                    object bound;
                    if (subscribedNotifier.TryGetSource(out bound) && ReferenceEquals(item, bound))
                    {
                        notifier = subscribedNotifier;
                        return true;
                    }
                }

                notifier = null;
                index = -1;
                return false;
            }

            public override bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                var args = e as NotifyCollectionChangedEventArgs;
                if (args != null)
                {
                    OnCollectionChanged(sender, args);
                    return true;
                }

                return base.ReceiveWeakEvent(managerType, sender, e);
            }
        }

        class CollectionDependencyProvider : IDependencyProvider
        {
            private readonly bool _notifyOnItemChanges;
            private readonly IList<SourcePathDescription> _itemPaths;
            private readonly SourcePathDescription _sourcePathDescription;

            public CollectionDependencyProvider(SourcePathDescription sourcePathDescription, bool notifyOnItemChanges = false)
                : this(sourcePathDescription, notifyOnItemChanges, null)
            {

            }

            public CollectionDependencyProvider(SourcePathDescription sourcePathDescription, IList<SourcePathDescription> itemPaths)
                : this(sourcePathDescription, false, itemPaths)
            {

            }

            private CollectionDependencyProvider(SourcePathDescription sourcePathDescription, bool notifyOnItemChanges, IList<SourcePathDescription> itemPaths)
            {
                _notifyOnItemChanges = notifyOnItemChanges;
                _sourcePathDescription = sourcePathDescription;
                _itemPaths = itemPaths;
            }

            public IDependencyNotifier CreateNotifier()
            {
                return new CollectionDependencyNotifier(_sourcePathDescription, _notifyOnItemChanges, _itemPaths);
            }
        }

        #endregion

        [NonSerialized]
        CompositeTypeDependencyDescription _dependencyDescription;
        [NonSerialized]
        Lazy<List<IDependencyNotifier>> _subscribedNotifiers;

        private CompositeTypeDependencyDescription GetDependencyDescription()
        {
            var desc = new CompositeTypeDependencyDescription(this.GetType(), _dependencyStore);

            foreach (var notifierDesc in desc.CreateNotifiers(this))
            {
                foreach (var notifier in notifierDesc.Item2)
                {
                    var dependentProperty = notifierDesc.Item1;
                    notifier.DependencyChanged += delegate
                    {
                        OnDependentPropertyChanged(dependentProperty);
                    };

                    _subscribedNotifiers.Value.Add(notifier);
                }
            }

            return desc;
        }

        static readonly ConcurrentDictionary<Type, TypeDependencyDescription> _dependencyStore = new ConcurrentDictionary<Type, TypeDependencyDescription>();

        protected static IPropertyDependency<TSource> AddPropertyDependency<TSource, TProperty>(Expression<Func<TSource, TProperty>> property)
        {
            Type sourceType = typeof(TSource);

            TypeDependencyDescription desc = _dependencyStore.GetOrAdd(sourceType,
                t => new TypeDependencyDescription());

            return desc.CreateDependency(property);
        }

        protected static IPropertyDependency<TSource> AddCallbackDependency<TSource>(Expression<Action<TSource>> callback)
        {
            Type sourceType = typeof(TSource);

            TypeDependencyDescription desc = _dependencyStore.GetOrAdd(sourceType,
                t => new TypeDependencyDescription());

            return desc.CreateDependency(callback);
        }

        private void ProcessFlatDependencies(string propertyName)
        {
            if (_dependencyDescription != null)
            {
                foreach (var flatBound in _dependencyDescription.FlatBoundProperties)
                {
                    List<PropertyDependency> dependsOn;
                    if (flatBound.TryGetValue(propertyName, out dependsOn))
                    {
                        foreach (var dependent in dependsOn)
                        {
                            OnDependentPropertyChanged(dependent);
                        }
                    }
                }
            }
        }

        private void OnDependentPropertyChanged(PropertyDependency property)
        {
            if (property.IsNotification)
            {
                bool notify = true;
                if (property.IsCommand)
                {
                    var command = property.ExtractMember(this) as DelegateCommandBase;

                    if (command != null)
                    {
                        notify = false;
                        command.RaiseCanExecuteChanged();
                    }
                    else
                    {
                        var cmd = property.ExtractMember(this) as ICommandEx;

                        if (cmd != null)
                        {
                            notify = false;
                            cmd.RaiseCanExecuteChanged();
                        }
                    }
                }

                if (notify)
                {
                    OnPropertyChanged(property.DependentPropertyName);
                }
            }
            
            property.ExecuteInvocations(this);
        }

        #endregion

        #region Commands

        public Dictionary<string, ICommand> Commands
        {
            get { return _commands; }
        }

        protected void RegisterCommand(ICommandEx commandEx)
        {
            RegisterCommand(commandEx.Name, commandEx);
        }

        protected void RegisterCommand(string name, ICommand command)
        {
            _commands[name] = command;
        }

        #endregion
    }
}
