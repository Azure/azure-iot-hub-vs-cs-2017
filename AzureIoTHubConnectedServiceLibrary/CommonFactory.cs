using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return RegistryManager.CreateFromConnectionString(connectionString);

        }
    }
}
