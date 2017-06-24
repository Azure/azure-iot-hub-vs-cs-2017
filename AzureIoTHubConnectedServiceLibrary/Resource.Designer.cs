﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AzureIoTHubConnectedService {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AzureIoTHubConnectedService.Resource", typeof(Resource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create New IoT Hub.
        /// </summary>
        internal static string CreateServiceInstanceText {
            get {
                return ResourceManager.GetString("CreateServiceInstanceText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to create device &apos;{0}&apos;. IoT Hub returned an error..
        /// </summary>
        internal static string DeviceCreationFailure {
            get {
                return ResourceManager.GetString("DeviceCreationFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A file with the name &apos;{0}&apos; already exists. Do you want to replace it?.
        /// </summary>
        internal static string FileAlreadyExists {
            get {
                return ResourceManager.GetString("FileAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select an existing IoT Hub or create a new one by clicking the link below..
        /// </summary>
        internal static string GridHeaderText {
            get {
                return ResourceManager.GetString("GridHeaderText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Communicate between your devices and Azure IoT Hub..
        /// </summary>
        internal static string IoTHubProvdierDescription {
            get {
                return ResourceManager.GetString("IoTHubProvdierDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loading Devices....
        /// </summary>
        internal static string LoadingDevices {
            get {
                return ResourceManager.GetString("LoadingDevices", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Installing NuGet package &apos;{0}&apos; version {1}..
        /// </summary>
        internal static string LogMessage_AddingNuGetPackage {
            get {
                return ResourceManager.GetString("LogMessage_AddingNuGetPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The code being added depends on NuGet package ‘{0}’ version {1}.  A newer version ({2}) is already installed.  This may cause compatibility issues..
        /// </summary>
        internal static string LogMessage_NewerMajorVersionNuGetPackageExists {
            get {
                return ResourceManager.GetString("LogMessage_NewerMajorVersionNuGetPackageExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Upgrading NuGet package &apos;{0}&apos; from version {1} to {2}.  A major version upgrade may introduce compatibility issues with existing code..
        /// </summary>
        internal static string LogMessage_OlderMajorVersionNuGetPackageExists {
            get {
                return ResourceManager.GetString("LogMessage_OlderMajorVersionNuGetPackageExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Uninstalling NuGet package &apos;{0}&apos; version {1}..
        /// </summary>
        internal static string LogMessage_RemovingNuGetPackage {
            get {
                return ResourceManager.GetString("LogMessage_RemovingNuGetPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Upgrading NuGet package &apos;{0}&apos; from version {1} to {2}..
        /// </summary>
        internal static string LogMessage_UpgradingNuGetPackage {
            get {
                return ResourceManager.GetString("LogMessage_UpgradingNuGetPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To view your IoT Hubs, add a user account or reenter your credentials..
        /// </summary>
        internal static string NeedToAuthenticateText {
            get {
                return ResourceManager.GetString("NeedToAuthenticateText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No IoT Hub instances were found.  Choose another user or create a new IoT Hub instance from Azure Portal..
        /// </summary>
        internal static string NoServiceInstancesText {
            get {
                return ResourceManager.GetString("NoServiceInstancesText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Region.
        /// </summary>
        internal static string Region {
            get {
                return ResourceManager.GetString("Region", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resource Group.
        /// </summary>
        internal static string ResourceGroup {
            get {
                return ResourceManager.GetString("ResourceGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IoT Hub Name.
        /// </summary>
        internal static string ServiceInstanceNameLabelText {
            get {
                return ResourceManager.GetString("ServiceInstanceNameLabelText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subscription.
        /// </summary>
        internal static string Subscription {
            get {
                return ResourceManager.GetString("Subscription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tier.
        /// </summary>
        internal static string Tier {
            get {
                return ResourceManager.GetString("Tier", resourceCulture);
            }
        }
    }
}
