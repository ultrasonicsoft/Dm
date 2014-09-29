using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ultrasonic.DownloadManager.Core.ViewModels
{
    public interface IPropertyDependency<TSource>
    {
        IPropertyDependency<TSource> DependsOn<TResult>(params Expression<Func<TSource, TResult>>[] propertyExpressions);
        IPropertyDependency<TSource> DependsOn<TResult>(IEnumerable<Expression<Func<TSource, TResult>>> propertyExpressions);

        IPropertyDependency<TSource> DependsOnCollection(Expression<Func<TSource, IEnumerable>> dependsOn, bool notifyOnItemChanges = false);
        IPropertyDependency<TSource> DependsOnCollection<TItem>(Expression<Func<TSource, IEnumerable<TItem>>> dependsOn, params Expression<Func<TItem, object>>[] itemDependencies);
        IPropertyDependency<TSource> DependsOnCollection<TItem>(Expression<Func<TSource, IEnumerable>> dependsOn, params Expression<Func<TItem, object>>[] itemDependencies);

        IPropertyDependency<TSource> DependsOn<TResult>(IDependencyProvider provider);
        IPropertyDependency<TSource> Invoke(Expression<Action<TSource>> action);
    }

    #region Dependencies

    public interface IDependencyProvider
    {
        IDependencyNotifier CreateNotifier();
    }

    public interface IDependencyNotifier
    {
        object AssociatedObject { get; set; }

        event EventHandler DependencyChanged;
    }

    public abstract class DependencyNotifier : IDependencyNotifier
    {
        private object _associatedObject;
        public object AssociatedObject
        {
            get { return _associatedObject; }
            set
            {
                if (Equals(_associatedObject, value)) return;

                _associatedObject = value;
                OnAssociatedObjectChanged();
            }
        }

        protected abstract void OnAssociatedObjectChanged();

        public event EventHandler DependencyChanged;
        protected virtual void OnDependencyChanged()
        {
            var handler = DependencyChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

    #endregion
}
