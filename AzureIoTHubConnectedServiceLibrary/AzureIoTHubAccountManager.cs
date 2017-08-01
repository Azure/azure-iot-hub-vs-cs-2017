// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Services.Client.AccountManagement;
using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System.Threading;

namespace AzureIoTHubConnectedService
{
    [Export(typeof(IAzureIoTHubAccountManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class AzureIoTHubAccountManager : IAzureIoTHubAccountManager
    {
        public AzureIoTHubAccountManager()
        {
        }

        public async Task<IEnumerable<IAzureIoTHub>> EnumerateIoTHubAccountsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(subscription);

            var client = await builder.CreateAsync().ConfigureAwait(false);

            IoTHubListResponse response = await ServiceManagementHttpClientExtensions.GetIoTHubsAsync(client, cancellationToken).ConfigureAwait(false);

            return response.Accounts.Select(p => new IoTHubResource(subscription, p)).ToList();
        }

        public async Task<IEnumerable<ResourceLocation>> EnumerateLocationsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(subscription);

            var client = await builder.CreateAsync().ConfigureAwait(false);

            LocationListResponse response = await ServiceManagementHttpClientExtensions.GetLocationsAsync(client, cancellationToken).ConfigureAwait(false);

            return response.Locations;
        }

        public async Task<IEnumerable<ResourceGroup>> EnumerateResourceGroupsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(subscription);

            var client = await builder.CreateAsync().ConfigureAwait(false);

            ResourceGroupListResponse response = await ServiceManagementHttpClientExtensions.GetResourceGroupsAsync(client, cancellationToken).ConfigureAwait(false);

            return response.Accounts.Select(p => new ResourceGroup(p.Name, p.Location) ).ToList();
        }

        public async Task<IAzureIoTHub> CreateIoTHubAsync(IAzureRMSubscription subscription, IServiceProvider serviceProvider, Account userAccount, string rgName, string location, string hubName, CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(subscription);
            var client = await builder.CreateAsync().ConfigureAwait(false);

            IoTHub response = await ServiceManagementHttpClientExtensions.CreateIotHubAsync(client, rgName, location, hubName, cancellationToken).ConfigureAwait(false);

            return new IoTHubResource(subscription, response);
        }

        public async Task<ResourceGroup> CreateResourceGroupAsync(IAzureRMSubscription subscription, IServiceProvider serviceProvider, Account userAccount, string rgName, CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(subscription);
            var client = await builder.CreateAsync().ConfigureAwait(false);

            ResourceGroup response = await ServiceManagementHttpClientExtensions.CreateResourceGroupAsync(client, rgName, cancellationToken).ConfigureAwait(false);

            return response;
        }
    }
}
