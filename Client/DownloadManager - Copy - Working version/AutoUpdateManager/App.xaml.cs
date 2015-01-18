using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace AutoUpdateManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //e.Args is the string[] of command line argruments
            base.OnStartup(e);
            MainWindow window = new MainWindow();
            window.Args = e.Args;
            window.ShowDialog();
        }
    }
}
