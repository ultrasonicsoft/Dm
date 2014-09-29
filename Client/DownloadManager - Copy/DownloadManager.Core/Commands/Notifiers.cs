using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.Events;
using Ultrasonic.DownloadManager.Core.Utils;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ultrasonic.DownloadManager.Controls.Commands
{
    public interface INotifyCanExecuteChanged
    {
        event EventHandler CanExecuteChanged;

        bool CanExecute();
    }

    public abstract class NotifyCanExecuteChangedBase : INotifyCanExecuteChanged
    {
        public event EventHandler CanExecuteChanged;
        protected virtual void OnCanExecuteChanged()
        {
            var evt = CanExecuteChanged;
            if (evt != null)
            {
                evt(this, EventArgs.Empty);
            }
        }

        public abstract bool CanExecute();
    }

    #region ObjectChangedCanExecuteNotifier
    
    #region Member Descriptions
    public class SingleSourcePathDescription
    {
        public string NotifyPropertyName { get; internal set; }
        public Func<object, object> ExtractMemberMethod { get; private set; }
        public string Display { get; private set; }
        public bool IsNotifier { get; private set; }

        internal SingleSourcePathDescription(Func<object, object> extractMember, string notifyPropertyName, string display, bool isNotifier)
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

            MemberExpression memExpression = sourcePathExpression.Body as MemberExpression;
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

    public class SourcePathDescription
    {
        private List<SingleSourcePathDescription> _descriptions;
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

            List<SingleSourcePathDescription> descs = new List<SingleSourcePathDescription>();
            List<string> propNames = new List<string>();

            MemberExpression memberExpression = sourcePathExpression.Body as MemberExpression;
            while (memberExpression != null)
            {
                SingleSourcePathDescription desc = SingleSourcePathDescription.Create(sourcePathExpression, memberExpression);
                descs.Add(desc);
                propNames.Add(desc.NotifyPropertyName);

                memberExpression = memberExpression.Expression as MemberExpression;
            }

            int count = descs.Count;
            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    descs[i].NotifyPropertyName = propNames[count - 1];
                }
                else
                {
                    descs[i].NotifyPropertyName = propNames[i - 1];
                }
            }

            descs.Reverse();

            return new SourcePathDescription(descs, sourcePathExpression.Parameters[0].Type);
        }
    }

    public class SourceMembersDescription
    {
        public IEnumerable<string> NotifyPropertyNames { get; private set; }

        SourceMembersDescription(List<string> notifyPropertyNames)
        {
            NotifyPropertyNames = notifyPropertyNames;
        }

        public static SourceMembersDescription Create<TSource>(params Expression<Func<TSource, object>>[] sourcePropertyExpressions)
        {
            List<string> notifyPropertyNames = new List<string>();

            foreach (Expression<Func<TSource, object>> sourcePropExpr in sourcePropertyExpressions)
            {
                notifyPropertyNames.Add(ExpressionHelper.GetPropertyName(sourcePropExpr));
            }

            notifyPropertyNames.Add(string.Empty);

            return new SourceMembersDescription(notifyPropertyNames);
        }
    }
    #endregion

    public class ObjectChangedNotifier<T> : NotifyCanExecuteChangedBase, IWeakEventListener
    {
        #region Fields
        SourceMembersDescription _sourceDescription;
        SourcePathDescription _containerDescriptionPath;
        WeakReference _sourceRef;
        WeakReference _containerRef;
        List<WeakReference> _boundContainers;
        Func<T, bool> _canExecuteMethod;
        #endregion

        #region Properties
        bool? _nullValue;
        public bool NullValue
        {
            get { return _nullValue.GetValueOrDefault(); }
            set { _nullValue = value; }
        }
        #endregion

        #region Constructors
        public ObjectChangedNotifier(object source, SourceMembersDescription sourceDescription, Func<T, bool> canExecuteMethod)
            : this(null, source, null, sourceDescription, canExecuteMethod, null)
        {
        }

        public ObjectChangedNotifier(object source, SourceMembersDescription sourceDescription, Func<T, bool> canExecuteMethod, bool nullValue)
            : this(null, source, null, sourceDescription, canExecuteMethod, nullValue)
        {
        }

        public ObjectChangedNotifier(object container, SourcePathDescription sourcePathDescription, SourceMembersDescription sourceDescription, Func<T, bool> canExecuteMethod)
            : this(container, null, sourcePathDescription, sourceDescription, canExecuteMethod, null)
        {
        }

        public ObjectChangedNotifier(object container, SourcePathDescription sourcePathDescription, SourceMembersDescription sourceDescription, Func<T, bool> canExecuteMethod, bool nullValue)
            : this(container, null, sourcePathDescription, sourceDescription, canExecuteMethod, nullValue)
        {
        }

        private ObjectChangedNotifier(object container, object source, SourcePathDescription sourcePathDescription, SourceMembersDescription sourceDescription, Func<T, bool> canExecuteMethod, bool? nullValue)
        {
            _nullValue = nullValue;
            _canExecuteMethod = canExecuteMethod;
            _containerDescriptionPath = sourcePathDescription;
            _sourceDescription = sourceDescription;

            if (source == null)
            {
                if (sourcePathDescription == null || sourcePathDescription.Descriptions.Count == 0)
                {
                    throw new ArgumentException("containerDescription");
                }

                if (container != null)
                {
                    TryGetSource(container, out source);
                }
            }

            if (container != null)
            {
                if (sourcePathDescription == null || sourcePathDescription.Descriptions.Count == 0)
                {
                    throw new ArgumentException("containerDescription");
                }

                if (!sourcePathDescription.AssignableType.IsAssignableFrom(container.GetType()))
                {
                    throw new ArgumentException("Incompatible container type", "container");
                }

                _containerRef = new WeakReference(container);
                _boundContainers = new List<WeakReference>();

                BindContainerPath(container, 0);
            }

            if (source != null)
            {
                BindPropertyChanged(source, sourceDescription.NotifyPropertyNames);
                _sourceRef = new WeakReference(source);
            }
        }
        #endregion

        #region Factory
        public static IObjectChangedNotifierFactory<T> FromSource(object source, SourceMembersDescription sourceDescription)
        {
            return new ObjectChangedNotifierFactory<T>(null, source, null, sourceDescription, false);
        }

        public static IObjectChangedNotifierFactory<T> FromPath(object container, SourcePathDescription sourcePathDescription, SourceMembersDescription sourceDescription)
        {
            return new ObjectChangedNotifierFactory<T>(container, null, sourcePathDescription, sourceDescription, true);
        }
        #endregion

        #region Property Bindings
        void BindContainerPath(object container, int startIndex)
        {
            bool boundContainers = _boundContainers.Count > 0;

            for (int i = startIndex; i < _containerDescriptionPath.Descriptions.Count; i++)
            {
                SingleSourcePathDescription desc = _containerDescriptionPath.Descriptions[i];

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
                if (i == _containerDescriptionPath.Descriptions.Count - 1)
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
                INotifyPropertyChanged sourceNotify = source as INotifyPropertyChanged;

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
                INotifyPropertyChanged sourceNotify = source as INotifyPropertyChanged;

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
            int count = _containerDescriptionPath.Descriptions.Count;

            if (Object.ReferenceEquals(container, sender))
            {
                desc = _containerDescriptionPath.Descriptions[count - 1];
                matchIndex = count - 1;
                return true;
            }

            for (int i = 0; i < count - 1; i++)
            {
                SingleSourcePathDescription checkDesc = _containerDescriptionPath.Descriptions[i];

                if (checkDesc.IsNotifier && Object.ReferenceEquals(checkDesc.ExtractMemberMethod(container), sender) && checkDesc.NotifyPropertyName == propName)
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

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            PropertyChangedEventArgs propArgs = e as PropertyChangedEventArgs;

            bool raiseCanExecute = false;

            if (propArgs != null)
            {
                bool hasSource = _sourceRef != null;
                bool processedContainer = false;

                if (_containerRef != null)
                {
                    object container = _containerRef.Target;
                    if (container != null)
                    {
                        SingleSourcePathDescription containerDesc;
                        int matchIndex;
                        if (TryFindContainerDescription(container, sender, propArgs.PropertyName, out containerDesc, out matchIndex))
                        {
                            object newSource;
                            bool bindNewSource = TryGetSource(container, out newSource);

                            if (hasSource)
                            {
                                object source = _sourceRef.Target;
                                if (source != null)
                                {
                                    if (Object.ReferenceEquals(source, newSource))
                                    {
                                        bindNewSource = false;
                                    }
                                    else
                                    {
                                        UnbindPropertyChanged(source, _sourceDescription.NotifyPropertyNames);
                                    }
                                }
                            }

                            if (newSource != null)
                            {
                                if (bindNewSource)
                                {
                                    _sourceRef = new WeakReference(newSource);
                                    BindPropertyChanged(newSource, _sourceDescription.NotifyPropertyNames);
                                }
                            }
                            else
                            {
                                _sourceRef = null;
                            }

                            if (matchIndex > 0)
                            {
                                BindContainerPath(container, matchIndex - 1);
                            }

                            raiseCanExecute = true;
                            processedContainer = true;
                        }
                    }
                }

                if (!processedContainer && hasSource)
                {
                    object source = _sourceRef.Target;
                    if (source != null && Object.ReferenceEquals(source, sender))
                    {
                        if (_sourceDescription.NotifyPropertyNames.Contains(propArgs.PropertyName))
                        {
                            raiseCanExecute = true;
                        }
                    }
                }
            }

            if (raiseCanExecute)
            {
                OnCanExecuteChanged();
            }

            return true;
        }

        bool TryGetSource(object container, out object source)
        {
            source = null;

            try
            {
                for (int i = 0; i < _containerDescriptionPath.Descriptions.Count; i++)
                {
                    var desc = _containerDescriptionPath.Descriptions[i];
                    source = desc.ExtractMemberMethod(container);

                    if (source == null)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                source = null;
                return false;
            }
        }
        #endregion

        #region NotifyCanExecuteChangedBase
        public override bool CanExecute()
        {
            if (_canExecuteMethod != null)
            {
                T o = default(T);

                bool converted = false;

                if (_sourceRef != null)
                {
                    object source = _sourceRef.Target;

                    if (!Object.Equals(source, default(T)))
                    {
                        o = (T)source;
                        converted = true;
                    }
                }

                if (!converted && _nullValue.HasValue)
                {
                    return _nullValue.Value;
                }
                else
                {
                    return _canExecuteMethod(o);
                }
            }

            return true;
        }
        #endregion
    }

    public interface IObjectChangedNotifierFactory<T>
    {
        IObjectChangedNotifierFactory<T> CanExecute(Func<T, bool> canExecuteMethod);
        IObjectChangedNotifierFactory<T> CanExecute(bool nullValue, Func<T, bool> canExecuteMethod);

        ObjectChangedNotifier<T> Create();
    }

    class ObjectChangedNotifierFactory<T> : IObjectChangedNotifierFactory<T>
    {
        object _container;
        object _source;
        SourcePathDescription _sourcePathDescription;
        bool _useContainer;
        Func<T, bool> _canExecuteMethod;
        SourceMembersDescription _sourceDescription;
        bool? _nullValue;

        internal ObjectChangedNotifierFactory(object container, object source, SourcePathDescription sourcePathDescription, SourceMembersDescription sourceDescription, bool useContainer)
        {
            _container = container;
            _source = source;
            _sourcePathDescription = sourcePathDescription;
            _sourceDescription = sourceDescription;
            _useContainer = useContainer;
        }

        public IObjectChangedNotifierFactory<T> CanExecute(Func<T, bool> canExecuteMethod)
        {
            _canExecuteMethod = canExecuteMethod;

            return this;
        }

        public IObjectChangedNotifierFactory<T> CanExecute(bool nullValue, Func<T, bool> canExecuteMethod)
        {
            _nullValue = nullValue;

            return CanExecute(canExecuteMethod);
        }

        public ObjectChangedNotifier<T> Create()
        {
            ObjectChangedNotifier<T> result = _useContainer
                ? new ObjectChangedNotifier<T>(_container, _sourcePathDescription, _sourceDescription, _canExecuteMethod)
                : new ObjectChangedNotifier<T>(_source, _sourceDescription, _canExecuteMethod);

            if (_nullValue.HasValue)
            {
                result.NullValue = _nullValue.Value;
            }

            return result;
        }
    }
    #endregion

    #region CompositeEventCanExecuteNotifier
    public class CalEventNotifier<TEventType, T> : NotifyCanExecuteChangedBase
        where TEventType : CompositePresentationEvent<T>, new()
    {
        Func<T, bool> _canExecute;
        T _lastArgs;
        bool _receivedEvent;

        public bool DefaultValue { get; set; }

        public CalEventNotifier(IEventAggregator eventAggregator, Func<T, bool> canExecute)
        {
            DefaultValue = true;
            _canExecute = canExecute;

            eventAggregator.GetEvent<TEventType>().Subscribe(
                e =>
                {
                    _receivedEvent = true;
                    _lastArgs = e;

                    OnCanExecuteChanged();
                });
        }

        public CalEventNotifier(bool defaultValue, IEventAggregator eventAggregator, Func<T, bool> canExecute)
            : this(eventAggregator, canExecute)
        {
            DefaultValue = defaultValue;
        }

        public override bool CanExecute()
        {
            if (_receivedEvent && _canExecute != null)
            {
                return _canExecute(_lastArgs);
            }

            return DefaultValue;
        }
    }
    #endregion
}
