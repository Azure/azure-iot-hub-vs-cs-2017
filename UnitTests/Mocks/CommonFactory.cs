using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;

namespace AzureIoTHubConnectedService
{
    public class CommonFactory
    {
        /// <summary>
        /// Creates a new instance of RegistryManager.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>New instance of RegistryManager</returns>
        public static RegistryManager CreateRegistryManagerFromConnectionString(string connectionString)
        {
            return new RegistryManager();
        }
    }
}

namespace Microsoft.Azure.Devices
{
    public class Device /*: IETagHolder*/
    {
        public Device()
        {
            Authentication = new AuthenticationMechanism();
        }

        public Device(string id)
        {
            Authentication = new AuthenticationMechanism();
            Id = id;
        }

        public string Id { get; }
        public string GenerationId { get; }

        public string ETag { get; set; }
        //public DeviceConnectionState ConnectionState { get; }
        //public DeviceStatus Status { get; set; }
        public string StatusReason { get; set; }
        public DateTime ConnectionStateUpdatedTime { get; }
        public DateTime StatusUpdatedTime { get; }
        public DateTime LastActivityTime { get; }
        public int CloudToDeviceMessageCount { get; }
        public AuthenticationMechanism Authentication { get; set; }

    }

    public sealed class AuthenticationMechanism
    {
        public AuthenticationMechanism()
        {
            SymmetricKey = new SymmetricKey();
        }

        public SymmetricKey SymmetricKey { get; set; }
        //public X509Thumbprint X509Thumbprint { get; set; }
    }

    public sealed class SymmetricKey
    {
        public SymmetricKey()
        {
            PrimaryKey = "FAKE-PRIMARY-KEY";
            SecondaryKey = "FAKE-SECONDARY-KEY";
        }

        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public bool IsEmpty() { return false; }
        public bool IsValid(bool throwArgumentException) { return true; }
    }

    public class RegistryManager
    {
        public Task<Device> AddDeviceAsync(Device device)
        {
            Task<Device> task = new Task<Device>(() => { return device; });
            task.Start();
            return task;
        }

        public Task<IEnumerable<Device>> GetDevicesAsync(int maxCount)
        {
            Task <IEnumerable<Device>> task = new Task<IEnumerable<Device>>(() => { return new List<Device>(); });
            task.Start();
            return task;
        }
    }
}