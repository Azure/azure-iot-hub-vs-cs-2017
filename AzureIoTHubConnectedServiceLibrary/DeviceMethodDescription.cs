using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    public class DirectMethodDescription
    {
        public DirectMethodDescription(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
