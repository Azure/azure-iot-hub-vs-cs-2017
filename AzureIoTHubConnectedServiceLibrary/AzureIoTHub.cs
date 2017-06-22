// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    public interface IAzureResource : IEquatable<IAzureResource>
    {
        string Id { get; }
        IReadOnlyDictionary<string, string> Properties { get; }
    }

    public struct PrimaryKeys
    {
        public string IoTHubOwner;
        public string Service;
    }

    public interface IAzureIoTHub : IAzureResource
    {
        Task<PrimaryKeys> GetPrimaryKeysAsync(CancellationToken cancellationToken);
    }
}