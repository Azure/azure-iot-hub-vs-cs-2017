﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Globalization;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    public class Sku
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "tier")]
        public string Tier { get; set; }

        [DataMember(Name = "capacity")]
        public string Capacity { get; set; }
    }

    [DataContract]
    public class IoTHub
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }

        [DataMember(Name = "subscriptionid")]
        public string SubscriptionId { get; set; }

        [DataMember(Name = "resourcegroup")]
        public string ResourceGroup { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "properties")]
        public IoTHubProperties Properties { get; set; }

        [DataMember(Name = "sku")]
        public Sku Sku { get; set; }

        public AuthorizationPolicies AuthorizationPolicies { get; set; }

        public string Tier()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", this.Sku.Name, this.Sku.Tier);
        }

        public string GetIoTHubOwnerPrimaryKey()
        {
            return this.AuthorizationPolicies.Policies.First((_) => _.KeyName == "iothubowner").PrimaryKey;
        }

        public string GetServicePrimaryKey()
        {
            return this.AuthorizationPolicies.Policies.First((_) => _.KeyName == "service").PrimaryKey;
        }
    }
}