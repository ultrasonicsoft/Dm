using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;

namespace Ultrasonic.DownloadManager.Controls.Commands
{
    public interface ICommandEx : ICommand, IActiveAware
    {
        void RaiseCanExecuteChanged();

        string Name { get; }
    }

    public abstract class BaseDelegateCommandEx<T> : DelegateCommand<T>, ICommandEx
    {
        private readonly List<INotifyCanExecuteChanged> _notifiers;
        public IEnumerable<INotifyCanExecuteChanged> Notifiers
        {
            get { return _notifiers; }
        }

        public string Name { get; private set; }

        #region Constructors

        protected BaseDelegateCommandEx(Action<T> executeMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(executeMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        protected BaseDelegateCommandEx(string name, Action<T> executeMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(name, executeMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        protected BaseDelegateCommandEx(Action<T> executeMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : this(null, executeMethod, null, notifiers)
        {
        }

        protected BaseDelegateCommandEx(string name, Action<T> executeMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : this(name, executeMethod, null, notifiers)
        {
        }

        protected BaseDelegateCommandEx(Action<T> executeMethod, Func<T, bool> canExecuteMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(null, executeMethod, canExecuteMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        protected BaseDelegateCommandEx(string name, Action<T> executeMethod, Func<T, bool> canExecuteMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(name, executeMethod, canExecuteMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        protected BaseDelegateCommandEx(string name, Action<T> executeMethod, Func<T, bool> canExecuteMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : base(executeMethod, canExecuteMethod ?? DefaultCanExecute)
        {
            Name = string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString() : name;

            _notifiers = new List<INotifyCanExecuteChanged>();

            WithNotifiers(notifiers);
        }

        private static bool DefaultCanExecute(T o)
        {
            return true;
        }

        #endregion

        #region ICommand

        bool ICommand.CanExecute(object parameter)
        {
            if (_notifiers.Any(notifier => !notifier.CanExecute()))
            {
                return false;
            }

            return CanExecute(parameter);
        }

        #endregion

        #region Notifiers

        public BaseDelegateCommandEx<T> WithNotifiers(params INotifyCanExecuteChanged[] notifiers)
        {
            return WithNotifiers((IEnumerable<INotifyCanExecuteChanged>)notifiers);
        }

        public BaseDelegateCommandEx<T> WithNotifiers(IEnumerable<INotifyCanExecuteChanged> notifiers)
        {
            if (notifiers != null)
            {
                var notifiersInline = new List<INotifyCanExecuteChanged>();
                foreach (INotifyCanExecuteChanged notifier in notifiers)
                {
                    notifier.CanExecuteChanged += (s, e) => RaiseCanExecuteChanged();
                    notifiersInline.Add(notifier);
                }

                _notifiers.AddRange(notifiersInline);
            }

            return this;
        }

        public BaseDelegateCommandEx<T> WithObjectNotifiers<TObject>(params ObjectChangedNotifier<TObject>[] notifiers)
        {
            return WithObjectNotifiers((IEnumerable<ObjectChangedNotifier<TObject>>)notifiers);
        }

        public BaseDelegateCommandEx<T> WithObjectNotifiers<TObject>(IEnumerable<ObjectChangedNotifier<TObject>> notifiers)
        {
            return WithNotifiers(notifiers);
        }

        #endregion
    }

    /// <summary>
    /// A wrapper for <see cref="Microsoft.Practices.Commands.DelegateCommand"/>. 
    /// </summary>
    /// <typeparam name="T">Parameter type.</typeparam>
    public class DelegateCommandEx<T> : BaseDelegateCommandEx<T>
    {
        #region Constructors

        public DelegateCommandEx(Action<T> executeMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(null, executeMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        public DelegateCommandEx(string name, Action<T> executeMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(name, executeMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        public DelegateCommandEx(Action<T> executeMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : this(null, executeMethod, null, notifiers)
        {
        }

        public DelegateCommandEx(string name, Action<T> executeMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : this(name, executeMethod, null, notifiers)
        {
        }

        public DelegateCommandEx(Action<T> executeMethod, Func<T, bool> canExecuteMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(null, executeMethod, canExecuteMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        public DelegateCommandEx(string name, Action<T> executeMethod, Func<T, bool> canExecuteMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(name, executeMethod, canExecuteMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        public DelegateCommandEx(Action<T> executeMethod, Func<T, bool> canExecuteMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : this(null, executeMethod, canExecuteMethod, notifiers)
        {
        }

        public DelegateCommandEx(string name, Action<T> executeMethod, Func<T, bool> canExecuteMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : base(name, executeMethod, canExecuteMethod, notifiers)
        {
        }

        public static DelegateCommandEx<T> Create(Action<T> executeMethod, Func<T, bool> canExecuteMethod = null)
        {
            return Create(null, executeMethod, canExecuteMethod);
        }

        public static DelegateCommandEx<T> Create(string name, Action<T> executeMethod, Func<T, bool> canExecuteMethod = null)
        {
            return new DelegateCommandEx<T>(name, executeMethod, canExecuteMethod);
        }

        public DelegateCommandEx<T> ApplyAction(Action<DelegateCommandEx<T>> applyAction)
        {
            if (applyAction == null)
            {
                throw new ArgumentNullException("applyAction");
            }

            applyAction(this);
            return this;
        }

        #endregion
    }

    /// <summary>
    /// A non generic wrapper for <see cref="Microsoft.Practices.Commands.DelegateCommand"/>.
    /// </summary>
    public class DelegateCommandEx : BaseDelegateCommandEx<object>
    {
        #region Constructors

        public DelegateCommandEx(Action executeMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(null, executeMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        public DelegateCommandEx(string name, Action executeMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(name, executeMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        public DelegateCommandEx(Action executeMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : this(null, executeMethod, null, notifiers)
        {
        }

        public DelegateCommandEx(string name, Action executeMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : this(name, executeMethod, null, notifiers)
        {
        }

        public DelegateCommandEx(Action executeMethod, Func<bool> canExecuteMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(null, executeMethod, canExecuteMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        public DelegateCommandEx(string name, Action executeMethod, Func<bool> canExecuteMethod, params INotifyCanExecuteChanged[] notifiers)
            : this(name, executeMethod, canExecuteMethod, (IEnumerable<INotifyCanExecuteChanged>)notifiers)
        {
        }

        public DelegateCommandEx(string name, Action executeMethod, Func<bool> canExecuteMethod, IEnumerable<INotifyCanExecuteChanged> notifiers)
            : base(name, o => executeMethod(), canExecuteMethod == null ? (Func<object, bool>)null : o => canExecuteMethod(), notifiers)
        {
        }

        public static DelegateCommandEx Create(Action executeMethod, Func<bool> canExecuteMethod = null)
        {
            return Create(null, executeMethod, canExecuteMethod);
        }

        public static DelegateCommandEx Create(string name, Action executeMethod, Func<bool> canExecuteMethod = null)
        {
            return new DelegateCommandEx(name, executeMethod, canExecuteMethod);
        }

        public DelegateCommandEx ApplyAction(Action<DelegateCommandEx> applyAction)
        {
            if (applyAction == null)
            {
                throw new ArgumentNullException("applyAction");
            }

            applyAction(this);
            return this;
        }

        #endregion
    }
}
