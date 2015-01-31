﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ultrasonic.DownloadManager.ViewModel;

namespace Ultrasonic.DownloadManager.View
{
    /// <summary>
    /// Interaction logic for MainContainerView.xaml
    /// </summary>
    public partial class ContainerView : Window 
    {

        public ContainerView()
        {
            this.DataContext = new ContainerViewModel();
            InitializeComponent();
        }
    }
}
