// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Services.Client.AccountManagement;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;

namespace AzureIoTHubConnectedService
{
    [ConnectedServiceProviderExport("Microsoft.AzureIoTHubService")]
    internal class AzureIoTHubProvider : ConnectedServiceProvider
    {
        [Import]
        private IAzureIoTHubAccountManager IoTHubAccountManager { get; set; }

        [Import(typeof(Microsoft.VisualStudio.Shell.SVsServiceProvider))]
        private IServiceProvider ServiceProvider { get; set; }

        [ImportingConstructor]
        public AzureIoTHubProvider()
        {

            this.Category = "Microsoft";
            this.Name = "Azure IoT Hub";
            this.Description = "Communicate between IoT devices and the cloud.";
            this.Icon = new BitmapImage(new Uri("pack://application:,,/" + this.GetType().Assembly.ToString() + ";component/AzureIoTHubProviderIcon.png"));
            this.CreatedBy = "Microsoft";
            this.Version = new Version(1, 8, 0);
            this.MoreInfoUri = new Uri("http://aka.ms/iothubgetstartedVSCS");
        }

        EnvDTE.Project GetActiveProject()
        {
            var dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;

            EnvDTE.Project activeProject = null;

            Array activeSolutionProjects = dte.ActiveSolutionProjects as Array;
            if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
            {
                activeProject = activeSolutionProjects.GetValue(0) as EnvDTE.Project;
            }

            return activeProject;
        }

        static readonly Guid CsProjectType = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

        public override Task<ConnectedServiceConfigurator> CreateConfiguratorAsync(ConnectedServiceProviderContext context)
        {
            ConnectedServiceConfigurator configurator;

            var project = GetActiveProject();
            var projectKind = new Guid(project.Kind);

            configurator = new WizardMain(this.IoTHubAccountManager, this.ServiceProvider, (projectKind == CsProjectType));

            // Load our package here to trigger telemetry event. This is exactly when user chooses our extension.
            IVsShell shell = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShell)) as IVsShell;
            if (shell != null)
            {
                IVsPackage package = null;
                Guid PackageToBeLoadedGuid =
                    new Guid(AzureIoTHubConnectedServicePackage.PackageGuidString);
                shell.LoadPackage(ref PackageToBeLoadedGuid, out package);

                // This should me improved, we should have a separate class for WizardMain, not depend on configurator
                (package as AzureIoTHubConnectedServicePackage).MainModel = (configurator as WizardMain);
            }

            return Task.FromResult(configurator);
        }
    }
}
