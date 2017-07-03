// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.IO;
using Microsoft.VisualStudio.ConnectedServices;
using System.Linq;
using System.Collections.Generic;

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

            // Linux and Windows source code is almost identical
            manifest.Files.Add(new FileToAdd("C/generic/azure_iot_hub.c"));
            manifest.Files.Add(new FileToAdd("C/generic/azure_iot_hub.h"));
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

            if (type.Contains("Linux"))
            {
                m_isLinuxProject = true;
            }

            return new AzureIoTHubConnectedServiceHandlerHelper(context);
        }

        protected override void GenerateDeviceMethodCode(ConnectedServiceHandlerHelper helper, DeviceMethodDescription[] methods)
        {
            if (methods == null)
            {
                helper.TokenReplacementValues.Add("deviceMethodCallbackDecl", "");
                helper.TokenReplacementValues.Add("deviceMethodCallbackRegistration", "");
                helper.TokenReplacementValues.Add("deviceMethodCode", "");
                return;
            }

            string t = "";
            var uriPrefix = "pack://application:,,/" + System.Reflection.Assembly.GetAssembly(this.GetType()).ToString() + ";component/Resources/";
            using (var streamReader = new StreamReader(System.Windows.Application.GetResourceStream(new Uri(uriPrefix + "C/generic/device_method_callback.inc")).Stream, System.Text.Encoding.ASCII))
            {
                t = streamReader.ReadToEnd();
            }

            bool first = true;
            string result = "";

            foreach (DeviceMethodDescription method in methods)
            {
                result += "    ";

                if (!first) result += "else ";

                result += "if (strcmp(methodName, \"" + method.Name + "\") == 0)\r\n";
                result += "    {\r\n";

                result += "        Log_Debug(\"Method " + method.Name + " called...\\n\");\r\n";
                result += "    }\r\n";
                first = false;
            }

            result = t.Replace("$inner$", result);
            helper.TokenReplacementValues.Add("deviceMethodCallbackDecl", "\r\nstatic int deviceMethodCallback(const char *methodName, const unsigned char* payload, size_t size, unsigned char** response, size_t *response_size, void* userContextCallback);");
            helper.TokenReplacementValues.Add("deviceMethodCallbackRegistration", "    IoTHubClient_LL_SetDeviceMethodCallback(iothub_client_handle, deviceMethodCallback, NULL);\r\n");
            helper.TokenReplacementValues.Add("deviceMethodCode", result);
        }

        protected override void GenerateDeviceTwinDesiredCode(ConnectedServiceHandlerHelper helper, DeviceTwinProperty[] properties)
        {
            if (properties == null)
            {
                helper.TokenReplacementValues.Add("deviceTwinCallbackDecl", "");
                helper.TokenReplacementValues.Add("deviceTwinCallbackRegistration", "");
                helper.TokenReplacementValues.Add("deviceTwinCodeDesired", "");
                return;
            }

            string t = "";
            var uriPrefix = "pack://application:,,/" + System.Reflection.Assembly.GetAssembly(this.GetType()).ToString() + ";component/Resources/";
            using (var streamReader = new StreamReader(System.Windows.Application.GetResourceStream(new Uri(uriPrefix + "C/generic/device_twin_callback.inc")).Stream, System.Text.Encoding.ASCII))
            {
                t = streamReader.ReadToEnd();
            }

            string result = "";

            foreach (DeviceTwinProperty property in properties)
            {
                if (property.PropertyType == "Desired")
                {
                    result += "        if (MULTITREE_OK == MultiTree_GetLeafValue(child, \"" + property.PropertyName + "\", &value))\r\n";
                    result += "        {\r\n";
                    result += "            Log_Debug(\"Property " + property.PropertyName + " changed, new value is %s\\n\", (char*) value);\r\n";
                    result += "        }\r\n";
                }

            }

            result = t.Replace("$inner$", result);
            helper.TokenReplacementValues.Add("deviceTwinCallbackDecl", "\r\nstatic void twinCallback(DEVICE_TWIN_UPDATE_STATE updateState, const unsigned char* payLoad, size_t size, void* userContextCallback);");
            helper.TokenReplacementValues.Add("deviceTwinCallbackRegistration", "    IoTHubClient_LL_SetDeviceTwinCallback(iothub_client_handle, twinCallback, NULL);\r\n");
            helper.TokenReplacementValues.Add("deviceTwinCodeDesired", result);
        }

        protected override void GenerateDeviceTwinReportedCode(ConnectedServiceHandlerHelper helper, DeviceTwinProperty[] properties)
        {
            if (properties == null)
            {
                helper.TokenReplacementValues.Add("deviceTwinCodeReported", "");
                helper.TokenReplacementValues.Add("twinReportStateCall", "");
                return;
            }

            string t = "";
            var uriPrefix = "pack://application:,,/" + System.Reflection.Assembly.GetAssembly(this.GetType()).ToString() + ";component/Resources/";
            using (var streamReader = new StreamReader(System.Windows.Application.GetResourceStream(new Uri(uriPrefix + "C/generic/device_twin_report.inc")).Stream, System.Text.Encoding.ASCII))
            {
                t = streamReader.ReadToEnd();
            }

            string result = "";

            foreach (DeviceTwinProperty property in properties)
            {
                if (property.PropertyType == "Reported")
                {
                    //result += "        if (MULTITREE_OK == MultiTree_GetLeafValue(child, \"" + property.PropertyName + "\", &value))\r\n";
                    //result += "        {\r\n";
                    //result += "            Log_Debug(\"Property " + property.PropertyName + " changed, new value is %s\\n\", (char*) value);\r\n";
                    //result += "        }\r\n";
                }
            }

            result = t.Replace("$inner$", result);

            helper.TokenReplacementValues.Add("deviceTwinCodeReported", result);
            helper.TokenReplacementValues.Add("twinReportStateCall", "\r\n    twinReportState();");
        }

        static private void ToolsPropertyAddValues(object item, string subProperty, string[] additionalValues)
        {
            List<String> values = new List<string>((item.GetType().GetProperty(subProperty).GetValue(item) as string).Split());

            foreach (string v in additionalValues)
            {
                if (!values.Any(s => s.Equals(v, StringComparison.CurrentCultureIgnoreCase)))
                {
                    values.Add(v);
                }
            }

            item.GetType().GetProperty(subProperty).SetValue(item, String.Join(" ", values));
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
                        ToolsPropertyAddValues(item, "AdditionalLibraryDirectories", additionalLibraryDirs);
                        ToolsPropertyAddValues(item, "AdditionalDependencies", additionalLibraries);
                    }
                    else if (item.ToString() == "Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCCLCompilerToolShim")
                    {
                        ToolsPropertyAddValues(item, "AdditionalIncludeDirectories", additionalIncludePaths);
                    }
                }
            }
        }
    }
}

