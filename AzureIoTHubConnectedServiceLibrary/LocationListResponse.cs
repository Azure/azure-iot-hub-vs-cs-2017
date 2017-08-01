// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class LocationListResponse
    {
        [DataMember(Name = "value")]
        public IList<ResourceLocation> Locations { get; set; }

        public LocationListResponse()
        {
            Locations = new List<ResourceLocation>();
        }
    }
}
