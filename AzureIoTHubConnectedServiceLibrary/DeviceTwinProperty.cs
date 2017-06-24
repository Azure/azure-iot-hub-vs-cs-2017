using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    public class DeviceTwinProperty
    {
        public DeviceTwinProperty(string name, string type, string dataType)
        {
            PropertyName = name;
            PropertyType = type;
            PropertyDataType = dataType;
        }

        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public string PropertyDataType { get; set; }
    }
}
