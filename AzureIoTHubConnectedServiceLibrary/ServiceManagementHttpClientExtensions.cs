// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using Newtonsoft.Json;
using System.Text;

namespace AzureIoTHubConnectedService
{
    internal static class ServiceManagementHttpClientExtensions
    {
        public static async Task<T> PostEmptyBodyAsync<T>(this ServiceManagementHttpClient client, string relativeUri, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await client.PostAsync(relativeUri, null, cancellationToken).ConfigureAwait(false);
            await client.EnsureSuccessOrThrow(response).ConfigureAwait(false);
            return await client.ReadContentAs<T>(response).ConfigureAwait(false);
        }

        public static async Task EnsureSuccessOrThrow(this ServiceManagementHttpClient client, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                // if it is not successful, try to get the exception message from the response
                AzureErrorResponse errorResponse = await client.ReadContentAs<AzureErrorResponse>(response);
                string code = errorResponse?.Error?.Code;
                string message = errorResponse?.Error?.Message;
                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(message))
                {
                    if (code == "DisallowedProvider")
                    {
                        throw new InvalidOperationException("AzureResourceManagementResources.DisallowedProviderErrorMessge");
                    }

                    throw new InvalidOperationException(message);
                }
            }

            // if we didn't get an exception message from the response, just ensure a successful status code
            response.EnsureSuccessStatusCode();
        }

        private const int MaxRetryAttempts = 5;

        private const string IoTHubPreviewApiVersion = "2015-08-15-preview";

        public static Task<IoTHubListResponse> GetIoTHubsAsync(this ServiceManagementHttpClient client, CancellationToken cancellationToken)
        {
            string relativeUrl = string.Format(CultureInfo.InvariantCulture,
                                               "subscriptions/{0}/providers/Microsoft.Devices/IoTHubs?api-version={1}",
                                               client.SubscriptionId,
                                               IoTHubPreviewApiVersion);

            return client.GetAsync<IoTHubListResponse>(relativeUrl, cancellationToken);
        }

        public static Task<ResourceGroupListResponse> GetResourceGroupsAsync(this ServiceManagementHttpClient client, CancellationToken cancellationToken)
        {
            string relativeUrl = string.Format(CultureInfo.InvariantCulture,
                                               "/subscriptions/{0}/resourcegroups?api-version=2017-05-10",
                                               client.SubscriptionId);

            //Task<HttpResponseMessage> msg = client.GetAsync(relativeUrl, cancellationToken);

            //msg.Wait();

            return client.GetAsync<ResourceGroupListResponse>(relativeUrl, cancellationToken);
        }

        public static async Task<IoTHub> GetIoTHubDetailsAsync(this ServiceManagementHttpClient client, IoTHub iotHubAccount, CancellationToken cancellationToken)
        {
            /// POST:
            ///  subscriptions/{subscriptionId}/resourceGroups/
            ///  {resourceGroupName}/providers/Microsoft.Devices/IotHubs/
            ///  {IotHubName}/IoTHubKeys/listkeys?api-version={api-version}

            string relativeUrl = string.Format(CultureInfo.InvariantCulture,
                                   "subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Devices/IotHubs/{2}/IoTHubKeys/listKeys?api-version={3}",
                                   client.SubscriptionId,
                                   Uri.EscapeDataString(iotHubAccount.ResourceGroup),
                                   Uri.EscapeDataString(iotHubAccount.Name),
                                   IoTHubPreviewApiVersion);

            AuthorizationPolicies authorizationPolicies = await client.PostEmptyBodyAsync<AuthorizationPolicies>(relativeUrl, cancellationToken).ConfigureAwait(false);
            iotHubAccount.AuthorizationPolicies = authorizationPolicies;
            return iotHubAccount;
        }

        public static async Task<IoTHub> CreateIotHubAsync(this ServiceManagementHttpClient client, string rgName, string hubName, CancellationToken cancellationToken)
        {

            string subscriptionId = client.SubscriptionId;

            var description = new
            {
                name = hubName,
                location = "East US",
                sku = new
                {
                    name = "S1",
                    tier = "Standard",
                    capacity = 1
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(description), Encoding.UTF8, "application/json");
            var requestUri = string.Format("https://management.azure.com/subscriptions/{0}/resourcegroups/{1}/providers/Microsoft.devices/IotHubs/{2}?api-version=2016-02-03", subscriptionId, rgName, hubName);
            var result = client.PutAsync(requestUri, content).Result;

            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed {0}", result.Content.ReadAsStringAsync().Result);
                return null;
            }

            var asyncStatusUri = result.Headers.GetValues("Azure-AsyncOperation").First();

            string body;
            do
            {
                Thread.Sleep(10000);
                HttpResponseMessage deploymentstatus = client.GetAsync(asyncStatusUri).Result;
                body = deploymentstatus.Content.ReadAsStringAsync().Result;
            } while (body == "{\"status\":\"Running\"}");

            if (body == "{\"status\":\"Succeeded\"}")
            {
                // query new list of hubs for this subscription
                IoTHubListResponse hubs = await GetIoTHubsAsync(client, cancellationToken);

                foreach (IoTHub h in hubs.Accounts)
                {
                    if (h.Name == hubName)
                        return h;
                }
            }

            return null;
        }
        public static async Task<ResourceGroup> CreateResourceGroupAsync(this ServiceManagementHttpClient client, string rgName, CancellationToken cancellationToken)
        {

            string subscriptionId = client.SubscriptionId;

            var description = new
            {
                location = "East US"
            };

            var content = new StringContent(JsonConvert.SerializeObject(description), Encoding.UTF8, "application/json");
            var requestUri = string.Format("https://management.azure.com/subscriptions/{0}/resourcegroups/{1}?api-version=2017-05-10", subscriptionId, rgName);
            var result = client.PutAsync(requestUri, content).Result;

            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed {0}", result.Content.ReadAsStringAsync().Result);
                return null;
            }

            return new ResourceGroup(rgName, "East US");
        }
    }
}
