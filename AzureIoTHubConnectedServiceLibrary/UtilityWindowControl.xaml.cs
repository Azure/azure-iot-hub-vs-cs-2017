//------------------------------------------------------------------------------
// <copyright file="ToolWindow1Control.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace AzureIoTHubConnectedService
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class UtilityWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UtilityWindowControl"/> class.
        /// </summary>
        public UtilityWindowControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]

        public object Model
        {
            get
            {
                return _Model;
            }
            set
            {
                _Model = value;
                Summary.DataContext = value;
                PageReceiveMessages.DataContext = value;
                PageDeviceMethod.DataContext = value;
                PageDeviceTwin.DataContext = value;
                PageCloudToDeviceMsg.DataContext = value;
            }
        }

        private object _Model = null;
    }
}