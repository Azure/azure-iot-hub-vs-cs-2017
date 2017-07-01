// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using Microsoft.VisualStudio.ConnectedServices;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NuGet.VisualStudio;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Azure.Devices;
using System.Globalization;

namespace AzureIoTHubConnectedService
{

    internal abstract class GenericAzureIoTHubServiceHandler : ConnectedServiceHandler
    {

        [Import]
        private IVsPackageInstaller PackageInstaller { get; set; }

        [Import]
        private IVsPackageInstallerServices PackageInstallerServices { get; set; }


        protected abstract void GenerateDeviceMethodCode(ConnectedServiceHandlerHelper helper, DeviceMethodDescription[] methods);
        protected abstract void GenerateDeviceTwinReportedCode(ConnectedServiceHandlerHelper helper, DeviceTwinProperty[] properties);
        protected abstract void GenerateDeviceTwinDesiredCode(ConnectedServiceHandlerHelper helper, DeviceTwinProperty[] properties);

        public override async Task<AddServiceInstanceResult> AddServiceInstanceAsync(ConnectedServiceHandlerContext context, CancellationToken ct)
        {
            var cancel = context.ServiceInstance.Metadata["Cancel"];
            if (cancel != null)
            {
                if ((bool)cancel)
                {
                    Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent("vs/iothubcs/Cancelled");
                    // Cancellation
                    throw new OperationCanceledException();
                }
            }

            // Once C++ is officially supported, we can switch this to context.HandlerHelper, removing AzureIoTHubConnectedServiceHandlerHelper
            var handlerHelper = GetConnectedServiceHandlerHelper(context);


            handlerHelper.TokenReplacementValues.Add("stdafx", this.m_isLinuxProject ? "" : "#include \"stdafx.h\"");

            bool bUseTPM = (bool)context.ServiceInstance.Metadata["TPM"];
            if (bUseTPM)
            {
                handlerHelper.TokenReplacementValues.Add("TPMSlot", "0");
            }
            else
            {

                IAzureIoTHub iotHubAccount = context.ServiceInstance.Metadata["IoTHubAccount"] as IAzureIoTHub;
                var primaryKeys = await iotHubAccount.GetPrimaryKeysAsync(ct);

                var ioTHubUri = context.ServiceInstance.Metadata["iotHubUri"] as string;

                bool isDeviceProvisioned = ((context.ServiceInstance.Metadata["ProvisionedDevice"] as bool?) == true);

                object methods = null;
                try { methods = context.ServiceInstance.Metadata["DeviceMethods"]; } catch (Exception) {}
                object properties = null;
                try { properties = context.ServiceInstance.Metadata["DeviceTwinProperties"]; } catch (Exception) {}

                handlerHelper.TokenReplacementValues.Add("iotHubUri", ioTHubUri);

                var device = context.ServiceInstance.Metadata["Device"] as Device;
                if (device == null)
                {
                    throw new OperationCanceledException();
                }
                else
                {
                    // just use %s so the connection string can be used as sprintf pattern in the sample code
                    handlerHelper.TokenReplacementValues.Add("deviceId", isDeviceProvisioned ? "%s" : device.Id);
                    handlerHelper.TokenReplacementValues.Add("deviceKey", isDeviceProvisioned ? "%s" : device.Authentication.SymmetricKey.PrimaryKey);
                    handlerHelper.TokenReplacementValues.Add("iotHubOwnerPrimaryKey", primaryKeys.IoTHubOwner);
                    handlerHelper.TokenReplacementValues.Add("servicePrimaryKey", primaryKeys.Service);

                    GenerateDeviceMethodCode(handlerHelper, methods as DeviceMethodDescription[]);
                    GenerateDeviceTwinDesiredCode(handlerHelper, properties as DeviceTwinProperty[]);
                    GenerateDeviceTwinReportedCode(handlerHelper, properties as DeviceTwinProperty[]);
                }
            }

            HandlerManifest configuration = this.BuildHandlerManifest(bUseTPM);
            await this.AddSdkReferenceAsync(context, configuration, ct);

            foreach (var fileToAdd in configuration.Files)
            {
                var file = this.CopyResourceToTemporaryPath(fileToAdd.Path, handlerHelper);
                string targetPath = Path.GetFileName(fileToAdd.Path); // Use the same name
                string addedFile = await handlerHelper.AddFileAsync(file, targetPath);
            }

            AddServiceInstanceResult result = this.CreateAddServiceInstanceResult(context);

            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, "New service instance {0} created", context.ServiceInstance.Name);

            Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent("vs/iothubcs/ServiceCreated");

            return result;
        }

        protected abstract AddServiceInstanceResult CreateAddServiceInstanceResult(ConnectedServiceHandlerContext context);

        protected abstract ConnectedServiceHandlerHelper GetConnectedServiceHandlerHelper(ConnectedServiceHandlerContext context);

        string CopyResourceToTemporaryPath(string resource, ConnectedServiceHandlerHelper helper)
        {
            var uriPrefix = "pack://application:,,/" + Assembly.GetAssembly(this.GetType()).ToString() + ";component/Resources/";
            using (var reader = new StreamReader(Application.GetResourceStream(new Uri(uriPrefix + resource)).Stream))
            {
                var text = reader.ReadToEnd();
                var replaced = helper.PerformTokenReplacement(text);

                var path = Path.GetTempFileName();
                File.WriteAllText(path, replaced);
                return path;
            }
        }

        protected string LoadResource(string location)
        {
            string t = "";
            var uriPrefix = "pack://application:,,/" + System.Reflection.Assembly.GetAssembly(this.GetType()).ToString() + ";component/Resources/";
            using (var streamReader = new StreamReader(System.Windows.Application.GetResourceStream(new Uri(uriPrefix + location)).Stream, System.Text.Encoding.ASCII))
            {
                t = streamReader.ReadToEnd();
            }

            return t;
        }

        protected abstract HandlerManifest BuildHandlerManifest(bool useTPM);

        private async Task AddSdkReferenceAsync(ConnectedServiceHandlerContext context, HandlerManifest manifest, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            foreach (var nuget in manifest.PackageReferences)
            {
                Dictionary<string, string> packages = new Dictionary<string, string>();

                packages.Add(nuget.Id, nuget.Version);

                try
                {
                    await NuGetUtilities.InstallPackagesAsync(
                        packages,
                        "ConnectedServiceForAzureIoTHub.a8e3ec1c-7582-43ba-b8f6-d87ca58abcc4",
                        context.Logger,
                        ProjectUtilities.GetDteProject(context.ProjectHierarchy),
                        this.PackageInstallerServices,
                        this.PackageInstaller);
                }
                catch(Exception ex)
                {
                    var status = string.Format("Package {0} installation failed. Exception: '{1}'. WARNING: The project might not compile!", nuget.Id, ex.Message);
                    await context.Logger.WriteMessageAsync(LoggerMessageCategory.Warning, status);
                    Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent("vs/iothubcs/FailurePackageInstallation");
                }
            }
        }

        protected bool m_isLinuxProject = false;
        protected bool m_is4x4Project = false;
    }

    internal class NuGetReference
    {
        public NuGetReference(string packageId, string packageVersion)
        {
            this.Id = packageId;
            this.Version = packageVersion;
        }

        public string Id { get; set; }
        public string Version { get; set; }
    }

    /// <summary>
    /// The root of the handler manifest file.
    /// </summary>
    public class HandlerManifest
    {
        private List<SnippetToInsert> snippets;
        private List<FileToAdd> files;
        private List<NuGetReference> packageReferences;

        /// <summary>
        /// Gets or sets a list of snippet groups that will be inserted into the project.
        /// </summary>
        internal List<SnippetToInsert> Snippets { get { return snippets; } }

        /// <summary>
        /// Gets or sets a list of file groups that will be added to the project.
        /// </summary>
        internal List<FileToAdd> Files { get { return files; } }

        /// <summary>
        /// Gets or sets a list of NuGet references that will be added to the project.
        /// </summary>
        internal List<NuGetReference> PackageReferences { get { return packageReferences; } }

        internal HandlerManifest()
        {
            this.files = new List<FileToAdd>();
            this.snippets = new List<SnippetToInsert>();
            this.packageReferences = new List<NuGetReference>();
        }
    }

    /// <summary>
    /// The common attributes for a particular item that will be added or inserted into the project.
    /// </summary>
    internal abstract class ContentItem
    {
        /// <summary>
        /// Gets or sets the path to the file in question.
        /// </summary>
        public string Path { get; set; }
    }

    /// <summary>
    /// Add a specific file to the project.
    /// </summary>
    internal class FileToAdd : ContentItem
    {
        public FileToAdd(string resourcePath)
        {
            this.Path = resourcePath;
        }
    }

    internal class SnippetToInsert : ContentItem
    {
        public SnippetToInsert(string snippetPath, InjectionContext target)
        {
            this.Path = snippetPath;
            this.Target = target;
        }

        /// <summary>
        /// Gets or sets the target for the snippet insertion.
        /// </summary>
        public InjectionContext Target { get; set; }
    }

    /// <summary>
    /// The context for where to add a specific snippet within a project.
    /// </summary>
    internal enum InjectionContext
    {
        /// <summary>
        /// For WinJS applications, this is the start page as specified in the manifest file.
        /// </summary>
        StartPage,

        /// <summary>
        /// For Jupiter projects, this indicates that a field or member declaration should be added
        /// to the Application 'App' class.
        /// </summary>
        AppField,

        /// <summary>
        /// For Jupiter projects, this is the OnLaunched event on the Application 'App' class.
        /// </summary>
        AppStart,

        /// <summary>
        /// For C++ projects, this is an include directive in the Application class's cpp file.
        /// </summary>
        AppInclude
    }
}

