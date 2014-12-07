using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Ultrasonic.DownloadManager.Core.Utils;

namespace Ultrasonic.DownloadManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        UnityContainer _container;
        internal UnityContainer Container
        {
            get { return _container; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //new ObjectProducerTest().Test();
            
            base.OnStartup(e);

            //quick simple bootstrap
            _container = new UnityContainer();
            _container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
        }
    }
}
