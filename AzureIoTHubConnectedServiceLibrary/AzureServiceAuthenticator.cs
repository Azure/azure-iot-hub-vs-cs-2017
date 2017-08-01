// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using Microsoft.VisualStudio.Services.Client.AccountManagement;
using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.VisualStudio.ConnectedServices;

namespace AzureIoTHubConnectedService
{
    public abstract class AzureServiceAuthenticator : ConnectedServiceAuthenticator
    {
        private AccountPickerViewModel accountPickerViewModel;

        protected AzureServiceAuthenticator(IServiceProvider serviceProvider, string providerId)
        {
            this.accountPickerViewModel = new AccountPickerViewModel(serviceProvider, "ConnectedServices:" + providerId);
            this.accountPickerViewModel.PropertyChanged += this.AccountPickerViewModel_PropertyChanged;
            this.accountPickerViewModel.AuthenticationChanged += this.AccountPickerViewModel_AuthenticationChanged;
            this.CalculateIsAuthenticated();

            this.View = new AccountPicker(this.accountPickerViewModel);
        }

        public Task<Account> GetAccountAsync()
        {
            return this.accountPickerViewModel.GetAccountAsync();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.accountPickerViewModel.PropertyChanged -= this.AccountPickerViewModel_PropertyChanged;
                    this.accountPickerViewModel.AuthenticationChanged -= this.AccountPickerViewModel_AuthenticationChanged;
                    this.accountPickerViewModel.Dispose();

                    ((AccountPicker)this.View).Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected async Task<IEnumerable<IAzureSubscriptionContext>> GetSubscriptionContextsAsync()
        {
            IEnumerable<IAzureSubscriptionContext> subscriptions = Enumerable.Empty<IAzureSubscriptionContext>();

            Account account = await this.GetAccountAsync();
            if (account != null && !account.NeedsReauthentication)
            {
                IAzureUserAccount azureUserAccount = this.accountPickerViewModel.AuthenticationManager.UserAccounts.FirstOrDefault(a => a.UniqueId == account.UniqueId);

                if (azureUserAccount != null)
                {
                    try
                    {
                        subscriptions = await azureUserAccount.GetSubscriptionsAsync(false).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        // User cancelled out of the login prompt, etc. - ignore exception and return no subscriptions
                    }
                }
            }

            return subscriptions;
        }

        public virtual async Task<bool> SelectedAccountHasSubscriptions()
        {
            return (await this.GetSubscriptionContextsAsync().ConfigureAwait(false)).Any();
        }

        private void CalculateIsAuthenticated()
        {
            this.IsAuthenticated = this.accountPickerViewModel.IsAuthenticated;
        }

        private void AccountPickerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.accountPickerViewModel.IsAuthenticated))
            {
                this.CalculateIsAuthenticated();
            }
        }

        private void AccountPickerViewModel_AuthenticationChanged(object sender, EventArgs e)
        {
            // rebroadcast the AuthenticationChanged event whenever the underlying accountPickerViewModel
            // raises the AuthenticationChanged event
            this.OnAuthenticationChanged(new AuthenticationChangedEventArgs());
        }
    }

    public class Authenticator : AzureServiceAuthenticator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IAzureRMTenantService tenantService;

        public Authenticator(IServiceProvider serviceProvider)
            : base(serviceProvider, "Microsoft.Azure.IoTHub")
        {
            this.serviceProvider = serviceProvider;
            this.tenantService = (IAzureRMTenantService)serviceProvider.GetService(typeof(IAzureRMTenantService));
            this.NeedToAuthenticateText = Resource.NeedToAuthenticateText;
        }

        public override async Task<bool> SelectedAccountHasSubscriptions()
        {
            IEnumerable<IAzureRMSubscription> azureRMSubscriptions = await this.GetAzureRMSubscriptions();

            return azureRMSubscriptions.Any();
        }

        private async Task<IEnumerable<IAzureRMSubscription>> GetAzureRMSubscriptions()
        {
            Subscriptions = Enumerable.Empty<IAzureRMSubscription>();

            Account account = await this.GetAccountAsync();
            if (account != null && !account.NeedsReauthentication)
            {
                IEnumerable<IAzureRMTenant> tenants = await this.tenantService.GetTenantsAsync(account);
                foreach (IAzureRMTenant tenant in tenants)
                {
                    Subscriptions = Subscriptions.Concat(await tenant.GetSubscriptionsAsync());
                }
            }

            return Subscriptions;
        }

        public IEnumerable<IAzureRMSubscription> Subscriptions { get; set; }

        internal async Task<IEnumerable<IAzureIoTHub>> GetAzureIoTHubs(IAzureIoTHubAccountManager accountManager, CancellationToken cancellationToken)
        {
            IEnumerable<IAzureRMSubscription> subscriptions = await this.GetAzureRMSubscriptions().ConfigureAwait(false);
            List<IAzureIoTHub> iotHubAccounts = new List<IAzureIoTHub>();
            foreach (IAzureRMSubscription subscription in subscriptions)
            {
                IEnumerable<IAzureIoTHub> subscriptionAccounts = await accountManager.EnumerateIoTHubAccountsAsync(subscription, cancellationToken).ConfigureAwait(false);
                iotHubAccounts.AddRange(subscriptionAccounts);
            }

            return iotHubAccounts;
        }

        internal async Task<List<ResourceGroup>> GetResourceGroups(IAzureIoTHubAccountManager accountManager, string subscriptionName, CancellationToken cancellationToken)
        {
            IEnumerable<IAzureRMSubscription> subscriptions = await this.GetAzureRMSubscriptions().ConfigureAwait(false);
            List<ResourceGroup> groups = null;
            foreach (IAzureRMSubscription subscription in subscriptions)
            {
                if (subscription.SubscriptionName == subscriptionName)
                {
                    groups = (await accountManager.EnumerateResourceGroupsAsync(subscription, cancellationToken).ConfigureAwait(false)).ToList<ResourceGroup>();
                }
            }

            return groups;
        }

        internal async Task<List<ResourceLocation>> GetLocations(IAzureIoTHubAccountManager accountManager, string subscriptionName, CancellationToken cancellationToken)
        {
            IEnumerable<IAzureRMSubscription> subscriptions = await this.GetAzureRMSubscriptions().ConfigureAwait(false);
            List<ResourceLocation> locations = null;
            foreach (IAzureRMSubscription subscription in subscriptions)
            {
                if (subscription.SubscriptionName == subscriptionName)
                {
                    locations = (await accountManager.EnumerateLocationsAsync(subscription, cancellationToken).ConfigureAwait(false)).ToList<ResourceLocation>();
                }
            }

            return locations;
        }

        internal async Task<IAzureIoTHub> CreateIoTHub(IAzureIoTHubAccountManager accountManager, string subscriptionName, string rgName, string location, string hubName, CancellationToken cancellationToken)
        {
            IAzureRMSubscription subscription = (from a in Subscriptions where a.SubscriptionName == subscriptionName select a).First<IAzureRMSubscription>();
            Account account = await this.GetAccountAsync();
            Debug.Assert(account != null && !account.NeedsReauthentication);

            return await accountManager.CreateIoTHubAsync(subscription, this.serviceProvider, account, rgName, location, hubName, cancellationToken);
        }

        internal async Task<ResourceGroup> CreateResourceGroup(IAzureIoTHubAccountManager accountManager, string subscriptionName, string rgName, CancellationToken cancellationToken)
        {
            IAzureRMSubscription subscription = (from a in Subscriptions where a.SubscriptionName == subscriptionName select a).First<IAzureRMSubscription>();
            Account account = await this.GetAccountAsync();
            Debug.Assert(account != null && !account.NeedsReauthentication);

            return await accountManager.CreateResourceGroupAsync(subscription, this.serviceProvider, account, rgName, cancellationToken);
        }
    }
}
