using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    internal abstract class DeviceProvisionerBase
    {
        internal abstract void ProvisionDevice(string deviceId, string sharedAccessKey);
    }
}
