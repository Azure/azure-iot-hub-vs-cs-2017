﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace AzureIoTHubConnectedService.View
{
    public partial class CommonResources : ResourceDictionary
    {
        public CommonResources()
        {
            this.InitializeComponent();
            this.InitializeStyle();
        }

        private void InitializeStyle()
        {
            Style commonDialogStyle = (Style) this["ConnectedServicesCommonDialogStyle"];
            var setter = new Setter(ThemedDialogStyleLoader.UseDefaultThemedDialogStylesProperty, true);
            commonDialogStyle.Setters.Add(setter);
        }
    }
}
