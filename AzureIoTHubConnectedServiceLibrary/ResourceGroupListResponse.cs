// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    public class ResourceGroupListResponse
    {
        [DataMember(Name = "value")]
        public IList<ResourceGroup> Accounts { get; set; }

        public ResourceGroupListResponse()
        {
            Accounts = new List<ResourceGroup>();
        }
    }
}
