// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using Microsoft.VisualStudio.ConnectedServices;

namespace AzureIoTHubConnectedService
{
    [ConnectedServiceHandlerExport("Microsoft.AzureIoTHubService",
    AppliesTo = "VisualC+!WindowsAppContainer")]
    internal class CppHandler : GenericAzureIoTHubServiceHandler
    {
        protected override HandlerManifest BuildHandlerManifest(bool useTPM)
        {
            if (useTPM)
            {
                throw new NotSupportedException("TPM support for this project type is not yet supported");
            }

            HandlerManifest manifest = new HandlerManifest();

            // add packages only if not linux project
            if (!m_isLinuxProject)
            {
                manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.C.SharedUtility", "1.0.33"));
                manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.IoTHub.AmqpTransport", "1.1.14"));
                manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.IoTHub.MqttTransport", "1.1.14"));
                manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.IoTHub.IoTHubClient", "1.1.14"));
                manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.uamqp", "1.0.33"));
                manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.umqtt", "1.0.33"));
            }
            else
            {

            }

            manifest.Files.Add(new FileToAdd("CPP/NonWAC/azure_iot_hub.cpp"));
            manifest.Files.Add(new FileToAdd("CPP/NonWAC/azure_iot_hub.h"));

            return manifest;
        }

        protected override AddServiceInstanceResult CreateAddServiceInstanceResult(ConnectedServiceHandlerContext context)
        {
            return new AddServiceInstanceResult(
                "", // context.ServiceInstance.Name,
                null //new Uri(m_isLinuxProject ? "http://aka.ms/azure-iot-hub-vs-cs-c-linux" : "http://aka.ms/azure-iot-hub-vs-cs-c")
                );
        }

        protected override ConnectedServiceHandlerHelper GetConnectedServiceHandlerHelper(ConnectedServiceHandlerContext context)
        {

            string type = context.ProjectHierarchy.GetTargetPlatformIdentifier();
            // Linux project GUID: f5f201d3-915f-46f7-a3a0-1bb14e4b8c42


            EnvDTE.Project project = context.ProjectHierarchy.GetDteProject();
            object vcproject = project.Object;

            //string[] libs = new string[] { "lib1", "lib2", "lib3" };
            //string[] incs = new string[] { "incdir1", "incdir2"};
 
            //UpdateCppProject(vcproject, libs, incs);

            if (type.Contains("Linux"))
            {
                m_isLinuxProject = true;
            }

            return new AzureIoTHubConnectedServiceHandlerHelper(context);
        }

        private void UpdateCppProject(object vcProject, string[] additionalLibraryDirs, string[] additionalLibraries, string[] additionalIncludePaths)
        {
            System.Collections.IEnumerable configurations = vcProject.GetType().GetProperty("Configurations").GetValue(vcProject) as System.Collections.IEnumerable;

            foreach (object configuration in configurations)
            {
                System.Collections.IEnumerable tools = configuration.GetType().GetProperty("Tools").GetValue(configuration) as System.Collections.IEnumerable;

                foreach (object item in tools)
                {
                    if (item.ToString() == "Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCLinkerToolShim")
                    {
                        string value = item.GetType().GetProperty("AdditionalLibraryDirectories").GetValue(item) as string;

                        foreach (string  dir in additionalLibraryDirs)
                        {
                            if (!value.Contains(dir)) value += " " + dir;
                        }

                        item.GetType().GetProperty("AdditionalLibraryDirectories").SetValue(item, value);

                        value = item.GetType().GetProperty("AdditionalDependencies").GetValue(item) as string;

                        foreach (string library in additionalLibraries)
                        {
                            if (!value.Contains(library)) value += " " + library;
                        }
                       
                        item.GetType().GetProperty("AdditionalDependencies").SetValue(item, value);
                    }
                    else if (item.ToString() == "Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCCLCompilerToolShim")
                    {
                        string value = item.GetType().GetProperty("AdditionalIncludeDirectories").GetValue(item) as string;

                        foreach (string inc in additionalIncludePaths)
                        {
                            if (!value.Contains(inc)) value += " " + inc;
                        }

                        item.GetType().GetProperty("AdditionalIncludeDirectories").SetValue(item, value);

                    }
                }
            }
        }
    }
}

