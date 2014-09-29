using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Ultrasonic.DownloadManager.Core.Utils
{
    public interface IWeakEventManagerLogger
    {
        void Error(string message, Exception error);
    }

    public enum TrackerCleanupMode
    {
        Timer,
        Subscription
    }

    public enum TrackerCleanupThread
    {
        Task,
        Dispatcher
    }

    public abstract class WeakEventManagerBase
    {
        #region Defaults

        protected const TrackerCleanupMode DefaultCleanupMode = TrackerCleanupMode.Subscription;
        protected const int DefaultCleanTimerIntervalInSeconds = 60 * 2;
        protected const DispatcherPriority DefaultCleanupPriority = DispatcherPriority.ContextIdle;
        protected const TrackerCleanupThread DefaultCleanupThread = TrackerCleanupThread.Task;

        class DefaultLogger : IWeakEventManagerLogger
        {
            public void Error(string message, Exception error)
            {
                MessageBox.Show(string.Format("{0}\r\n{1}", message, error));
            }
        }

        #endregion

        #region EquatableWeakReference

        protected class EquatableWeakReference : WeakReference
        {
            private readonly WeakReference _targetRef;
            private readonly int _hashCode;

            public EquatableWeakReference(object target)
                : base(target)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }

                _targetRef = new WeakReference(target);
                _hashCode = target.GetHashCode();
            }

            public bool TryGetTarget(out object target)
            {
                target = _targetRef.IsAlive ? _targetRef.Target : null;
                return target != null;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as EquatableWeakReference;
                if (other == null)
                {
                    return false;
                }

                object otherTarget;
                object target;
                return (TryGetTarget(out target) && other.TryGetTarget(out otherTarget) && ReferenceEquals(otherTarget, target));
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }

        #endregion

        #region WeakObjectTracker

        /// <summary>
        /// A tracker for listening to object events and invoking subscribers
        /// </summary>
        /// <remarks>
        /// Instead of using a ConcurrentDictionary, we could initialize a set for all needed keys once per type and not use locks to add a collection per property.
        /// Downsides: 1) A bit of reflection work, but it is done once per type | 2) Consuming more memory and initializing more collections than actually needed
        /// Upsides: no locks when dealing with property collections
        /// </remarks>
        protected abstract class WeakObjectTracker : IDisposable
        {
            private readonly EquatableWeakReference _targetRef;

            public bool IsAlive
            {
                get { return _targetRef.IsAlive; }
            }

            private bool IsDisposed { get; set; }

            protected WeakObjectTracker(INotifyPropertyChanged target)
                : this(target, new EquatableWeakReference(target))
            {
            }

            protected WeakObjectTracker(object target, EquatableWeakReference targetRef)
            {
                _targetRef = targetRef;

                Subscribe(target);
            }

            protected abstract void Unsubscribe(object target);

            protected abstract void Subscribe(object target);

            public abstract void CleanSubscribers();

            protected abstract void OnDisposed();

            public void Dispose()
            {
                if (IsDisposed) return;

                object target;
                if (_targetRef.TryGetTarget(out target))
                {
                    Unsubscribe(target);
                }

                OnDisposed();

                IsDisposed = true;
            }
        }

        #endregion

        private readonly Lazy<IWeakEventManagerLogger> _loggerLazy;
        private readonly Dispatcher _dispatcher;
        protected ConcurrentDictionary<EquatableWeakReference, WeakObjectTracker> Trackers { get; private set; }

        protected IWeakEventManagerLogger Logger
        {
            get { return _loggerLazy.Value; }
        }

        protected WeakEventManagerBase()
        {
            _dispatcher = Application.Current.Dispatcher;

            _loggerLazy =  new Lazy<IWeakEventManagerLogger>(() => new DefaultLogger());

            Trackers = new ConcurrentDictionary<EquatableWeakReference, WeakObjectTracker>();

            SetCleanupOptionsCore(DefaultCleanupMode, DefaultCleanTimerIntervalInSeconds, DefaultCleanupPriority, DefaultCleanupThread, false);
        }

        protected void OnTrackerEventInvocations(WeakObjectTracker tracker)
        {
            if (IsCleanupOnSubscription)
            {
                ScheduleCleanupCore();
            }
        }

        #region Cleanup

        private DispatcherTimer _cleanTimer;
        private int _cleanupRequests;
        private TrackerCleanupMode _cleanupMode;
        private int _cleanupTimerIntervalInSeconds;
        private DispatcherPriority _cleanupPriority;
        private TrackerCleanupThread _cleanupThread;
        private bool _isCleanpOptionsSet;
        private bool _isSubscriptionCleanup;
        private bool _isCleanupTask;

        protected bool IsCleanupOnSubscription
        {
            get { return _isSubscriptionCleanup; }
        }

        public void SetCleanupOptionsCore(TrackerCleanupMode cleanupMode, int timerIntervalInSeconds, DispatcherPriority cleanupPriority, TrackerCleanupThread cleanupThread, bool external)
        {
            if (_isCleanpOptionsSet)
            {
                throw new InvalidOperationException("Cleanup options had already been set.");
            }

            _isCleanpOptionsSet = external;

            _cleanupMode = cleanupMode;
            _cleanupTimerIntervalInSeconds = timerIntervalInSeconds;
            _cleanupPriority = cleanupPriority;
            _cleanupThread = cleanupThread;

            if (_cleanTimer != null)
            {
                _cleanTimer.Stop();
            }

            _isSubscriptionCleanup = _cleanupMode == TrackerCleanupMode.Subscription;
            _isCleanupTask = _cleanupThread == TrackerCleanupThread.Task;

            if (!_isSubscriptionCleanup)
            {
                _cleanTimer = new DispatcherTimer(
                    TimeSpan.FromSeconds(_cleanupTimerIntervalInSeconds),
                    _cleanupPriority, OnCleanTimer, _dispatcher);
            }
            else
            {
                _isSubscriptionCleanup = true;
            }
        }

        private bool ShouldPerformCleanup()
        {
            return Interlocked.Increment(ref _cleanupRequests) == 1;
        }

        private void OnCleanTimer(object sender, EventArgs e)
        {
            if (ShouldPerformCleanup())
            {
                if (_isCleanupTask)
                {
                    Task.Factory.StartNew(PerformCleanup);
                }
                else
                {
                    PerformCleanup();
                }
            }
        }

        protected void ScheduleCleanupCore()
        {
            if (ShouldPerformCleanup())
            {
                if (_isCleanupTask)
                {
                    Task.Factory.StartNew(PerformCleanup);
                }
                else
                {
                    _dispatcher.BeginInvoke(_cleanupPriority, new DispatcherOperationCallback(CleanupOperation), null);
                }
            }
        }

        private object CleanupOperation(object arg)
        {
            PerformCleanup();

            return null;
        }

        private void PerformCleanup()
        {
            var logger = _loggerLazy.Value;
            bool hasLogger = logger != null;

            try
            {
                CleanTrackersCore();
            }
            catch (Exception ex)
            {
                if (hasLogger)
                {
                    logger.Error("An error occurred while trying to clean subscriptions in the WeakPropertyChangedEventManager.", ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                Interlocked.Exchange(ref _cleanupRequests, 0);
            }
        }

        private void CleanTrackersCore()
        {
            if (Trackers.Count == 0) return;

            foreach (var tracker in Trackers)
            {
                if (!tracker.Value.IsAlive)
                {
                    WeakObjectTracker removedTracker;
                    if (Trackers.TryRemove(tracker.Key, out removedTracker))
                    {
                        removedTracker.Dispose();
                    }
                }
                else
                {
                    tracker.Value.CleanSubscribers();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Manager for the INotifyPropertyChanged.PropertyChanged event which isn't a dispatcher object.
    /// </summary>
    /// <remarks>
    /// Currently, the cleaning process cleans not-alive sources, and for the live ones, it removes not-alive subscribers.
    /// If an object is not garbage collected, its record in the dictionary would remain. Additionally, its tracker would continue holding property collections for every property that was subscribed to. (the collections are emptied from not-alive subscribers, but they're not removed)
    /// It was decided to keep the cleaning process as such, because the memory consumption shouldn't be a great deal in this case, and it allows us to work with minimal synchronization locks.
    /// The cleaning process is made according to the cleanup mode - Timer: uses a DispatcherTimer to perform cleanup / Subscription: Same as PropertyChangedEventManager, schedules cleanup upon listener addition and event invocation.
    /// </remarks>
    public class WeakPropertyChangedEventManager : WeakEventManagerBase
    {
        #region Static Accessors

        private static readonly WeakPropertyChangedEventManager _current = new WeakPropertyChangedEventManager();

        public static void AddListener(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
        {
            _current.AddListenerCore(source, listener, propertyName);
        }

        public static void RemoveListener(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
        {
            _current.RemoveListenerCore(source, listener, propertyName);
        }

        /// <summary>
        /// Set the cleanup options for the event manager.
        /// This method is not thread-safe and should be called only once if the defaults are insufficient.
        /// </summary>
        /// <param name="cleanupMode">The cleanup mode.</param>
        /// <param name="timerIntervalInSeconds">The cleanup timer's interval in seconds if the mode is set to 'Timer'.</param>
        /// <param name="cleanupPriority">The dispatcher priority in which to schedule cleanups for.</param>
        /// <param name="cleanupThread">The cleanup thread.</param>
        public static void SetCleanupOptions(TrackerCleanupMode cleanupMode = DefaultCleanupMode, int timerIntervalInSeconds = DefaultCleanTimerIntervalInSeconds, DispatcherPriority cleanupPriority = DefaultCleanupPriority, TrackerCleanupThread cleanupThread = DefaultCleanupThread)
        {
            _current.SetCleanupOptionsCore(cleanupMode, timerIntervalInSeconds, cleanupPriority, cleanupThread, true);
        }

        internal static void ScheduleCleanup()
        {
            _current.ScheduleCleanupCore();
        }

        #endregion

        #region WeakPropertyChangeTracker

        protected class WeakPropertyChangeTracker : WeakObjectTracker
        {
            private readonly WeakPropertyChangedEventManager _manager;
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<EquatableWeakReference, bool>> _subscribers;

            public WeakPropertyChangeTracker(WeakPropertyChangedEventManager manager, INotifyPropertyChanged target)
                : this(manager, target, new EquatableWeakReference(target))
            {
            }

            public WeakPropertyChangeTracker(WeakPropertyChangedEventManager manager, INotifyPropertyChanged target, EquatableWeakReference targetRef)
                : base(target, targetRef)
            {
                _manager = manager;

                _subscribers = new ConcurrentDictionary<string, ConcurrentDictionary<EquatableWeakReference, bool>>();
            }

            protected override void Subscribe(object target)
            {
                ((INotifyPropertyChanged)target).PropertyChanged += OnSourcePropertyChanged;
            }

            protected override void Unsubscribe(object target)
            {
                ((INotifyPropertyChanged)target).PropertyChanged -= OnSourcePropertyChanged;
            }

            void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == string.Empty)
                {
                    foreach (var propSubsPair in _subscribers)
                    {
                        InvokeSubscribers(propSubsPair.Value, sender, new PropertyChangedEventArgs(propSubsPair.Key));
                    }
                }
                else
                {
                    ConcurrentDictionary<EquatableWeakReference, bool> propSubs;
                    if (_subscribers.TryGetValue(e.PropertyName, out propSubs))
                    {
                        InvokeSubscribers(propSubs, sender, e);
                    }
                }

                _manager.OnTrackerEventInvocations(this);
            }

            private static void InvokeSubscribers(ConcurrentDictionary<EquatableWeakReference, bool> subscribers, object sender, PropertyChangedEventArgs e)
            {
                foreach (var propSub in subscribers)
                {
                    object subscriber;
                    if (propSub.Key.TryGetTarget(out subscriber))
                    {
                        var listener = (IWeakEventListener)subscriber;

                        listener.ReceiveWeakEvent(typeof(WeakPropertyChangedEventManager), sender, e);
                    }
                }
            }

            public void AddListener(IWeakEventListener listener, string key)
            {
                var propSubs = _subscribers.GetOrAdd(key, _ => new ConcurrentDictionary<EquatableWeakReference, bool>());

                propSubs.TryAdd(new EquatableWeakReference(listener), true);
            }

            public void RemoveListener(IWeakEventListener listener, string key)
            {
                ConcurrentDictionary<EquatableWeakReference, bool> propSubs;
                if (_subscribers.TryGetValue(key, out propSubs))
                {
                    bool v;
                    propSubs.TryRemove(new EquatableWeakReference(listener), out v);
                }
            }

            public override void CleanSubscribers()
            {
                foreach (var propSubsPair in _subscribers)
                {
                    if (propSubsPair.Value.Count > 0)
                    {
                        foreach (var propSub in propSubsPair.Value)
                        {
                            if (!propSub.Key.IsAlive)
                            {
                                bool v;
                                propSubsPair.Value.TryRemove(propSub.Key, out v);
                            }
                        }
                    }
                }
            }

            protected override void OnDisposed()
            {
                _subscribers.Clear();
            }

            public string GetOutput()
            {
                var sb = new StringBuilder();

                foreach (var subscriber in _subscribers)
                {
                    sb.AppendFormat(", {0} - {1}", subscriber.Key, subscriber.Value.Count);
                }

                return sb.ToString();
            }
        }

        #endregion

        private void AddListenerCore(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            var sourceRef = new EquatableWeakReference(source);

            var tracker = Trackers.GetOrAdd(sourceRef, _ => new WeakPropertyChangeTracker(this, source, sourceRef));

            ((WeakPropertyChangeTracker)tracker).AddListener(listener, propertyName);

            if (IsCleanupOnSubscription)
            {
                ScheduleCleanupCore();
            }
        }

        private void RemoveListenerCore(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            var sourceRef = new EquatableWeakReference(source);

            WeakObjectTracker tracker;
            if (Trackers.TryGetValue(sourceRef, out tracker))
            {
                ((WeakPropertyChangeTracker)tracker).RemoveListener(listener, propertyName);
            }
        }
    }

    public class WeakCollectionChangedEventManager : WeakEventManagerBase
    {
        #region Static Accessors

        private static readonly WeakCollectionChangedEventManager _current = new WeakCollectionChangedEventManager();

        public static void AddListener(INotifyCollectionChanged source, IWeakEventListener listener)
        {
            _current.AddListenerCore(source, listener);
        }

        public static void RemoveListener(INotifyCollectionChanged source, IWeakEventListener listener)
        {
            _current.RemoveListenerCore(source, listener);
        }

        /// <summary>
        /// Set the cleanup options for the event manager.
        /// This method is not thread-safe and should be called only once if the defaults are insufficient.
        /// </summary>
        /// <param name="cleanupMode">The cleanup mode.</param>
        /// <param name="timerIntervalInSeconds">The cleanup timer's interval in seconds if the mode is set to 'Timer'.</param>
        /// <param name="cleanupPriority">The dispatcher priority in which to schedule cleanups for.</param>
        /// <param name="cleanupThread">The cleanup thread.</param>
        public static void SetCleanupOptions(TrackerCleanupMode cleanupMode = DefaultCleanupMode, int timerIntervalInSeconds = DefaultCleanTimerIntervalInSeconds, DispatcherPriority cleanupPriority = DefaultCleanupPriority, TrackerCleanupThread cleanupThread = DefaultCleanupThread)
        {
            _current.SetCleanupOptionsCore(cleanupMode, timerIntervalInSeconds, cleanupPriority, cleanupThread, true);
        }

        internal static void ScheduleCleanup()
        {
            _current.ScheduleCleanupCore();
        }

        #endregion

        #region WeakCollectionChangeTracker

        protected class WeakCollectionChangeTracker : WeakObjectTracker
        {
            private readonly WeakCollectionChangedEventManager _manager;
            private readonly ConcurrentDictionary<EquatableWeakReference, bool> _subscribers;

            public WeakCollectionChangeTracker(WeakCollectionChangedEventManager manager, INotifyCollectionChanged target)
                : this(manager, target, new EquatableWeakReference(target))
            {
            }

            public WeakCollectionChangeTracker(WeakCollectionChangedEventManager manager, INotifyCollectionChanged target, EquatableWeakReference targetRef)
                : base(target, targetRef)
            {
                _manager = manager;

                _subscribers = new ConcurrentDictionary<EquatableWeakReference, bool>();
            }

            protected override void Subscribe(object target)
            {
                ((INotifyCollectionChanged)target).CollectionChanged += OnCollectionChanged;
            }

            protected override void Unsubscribe(object target)
            {
                ((INotifyCollectionChanged)target).CollectionChanged -= OnCollectionChanged;
            }

            private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                InvokeSubscribers(_subscribers, sender, e);

                _manager.OnTrackerEventInvocations(this);
            }

            private static void InvokeSubscribers(ConcurrentDictionary<EquatableWeakReference, bool> subscribers, object sender, NotifyCollectionChangedEventArgs e)
            {
                foreach (var propSub in subscribers)
                {
                    object subscriber;
                    if (propSub.Key.TryGetTarget(out subscriber))
                    {
                        var listener = (IWeakEventListener)subscriber;

                        listener.ReceiveWeakEvent(typeof(WeakCollectionChangedEventManager), sender, e);
                    }
                }
            }

            public void AddListener(IWeakEventListener listener)
            {
                _subscribers.TryAdd(new EquatableWeakReference(listener), true);
            }

            public void RemoveListener(IWeakEventListener listener)
            {
                bool v;
                _subscribers.TryRemove(new EquatableWeakReference(listener), out v);
            }

            public override void CleanSubscribers()
            {
                foreach (var propSub in _subscribers)
                {
                    if (!propSub.Key.IsAlive)
                    {
                        bool v;
                        _subscribers.TryRemove(propSub.Key, out v);
                    }
                }
            }

            protected override void OnDisposed()
            {
                _subscribers.Clear();
            }

            public string GetOutput()
            {
                return _subscribers.Count.ToString();
            }
        }

        #endregion

        private void AddListenerCore(INotifyCollectionChanged source, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            var sourceRef = new EquatableWeakReference(source);

            var tracker = Trackers.GetOrAdd(sourceRef, _ => new WeakCollectionChangeTracker(this, source, sourceRef));

            ((WeakCollectionChangeTracker)tracker).AddListener(listener);

            if (IsCleanupOnSubscription)
            {
                ScheduleCleanupCore();
            }
        }

        private void RemoveListenerCore(INotifyCollectionChanged source, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            var sourceRef = new EquatableWeakReference(source);

            WeakObjectTracker tracker;
            if (Trackers.TryGetValue(sourceRef, out tracker))
            {
                ((WeakCollectionChangeTracker)tracker).RemoveListener(listener);
            }
        }
    }
}
