using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    public class DeviceMethodDescription
    {
        public DeviceMethodDescription(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
