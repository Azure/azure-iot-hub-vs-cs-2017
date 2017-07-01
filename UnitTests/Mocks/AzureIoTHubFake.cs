using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AzureIoTHubConnectedService;

namespace AzureIoTHubConnectedService
{
    class AzureIoTHubFake : IAzureIoTHub
    {
        public bool Equals(IAzureResource hub)
        {
            return (hub.Id == "fakehub");
        }

        public Task<PrimaryKeys> GetPrimaryKeysAsync(CancellationToken cancellationToken)
        {
            return new Task<PrimaryKeys>(() => { return new PrimaryKeys(); });
        }

        public string Id
        {
            get
            {
                return "fakehub";
            }
        }

        public IReadOnlyDictionary<string, string> Properties
        {
            get
            {
                return _properties;
            }
        }

        public IDictionary<string, string> WritableProperties
        {
            get
            {
                return _properties;
            }
        }

        private Dictionary<string,string> _properties = new Dictionary<string,string>();
    }
}
