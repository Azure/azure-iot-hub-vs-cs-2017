﻿using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// Interaction logic for WizardPageSummaryView.xaml
    /// </summary>
    public partial class WizardPageSummaryView : UserControl
    {
        public WizardPageSummaryView()
        {
            InitializeComponent();
        }
        
        public WizardPageSummaryView(object model)
        {
            InitializeComponent();

            DataContext = model;
        }

        public void SetModel(object model)
        {
            DataContext = model;
        }

        private void SummaryView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            dynamic d = DataContext;
            d.SummaryVisible = (e.NewValue as bool?) ?? false;
        }
    }
}