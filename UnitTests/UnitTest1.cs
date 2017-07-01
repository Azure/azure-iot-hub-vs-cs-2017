using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AzureIoTHubConnectedService;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestNewDeviceInputValidation()
        {
            WizardMain model = new WizardMain();

            Assert.IsNotNull(model);
            model.NewDevice_Name = "abcdef";
            Assert.IsTrue(model.NewDevice_CanCreate);

            // shouldn't be able to create if device name is empty
            model.NewDevice_Name = "";
            Assert.IsFalse(model.NewDevice_CanCreate);

            // check all allowed characters
            //@"^[a-zA-Z0-9_\-\:\.\+\%\#\*\?\!\(\)\,\=\@\;\$]+$"

            model.NewDevice_Name = "abcdefghijklmnopqrstuvwxyz";
            Assert.IsTrue(model.NewDevice_CanCreate);

            model.NewDevice_Name = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Assert.IsTrue(model.NewDevice_CanCreate);

            model.NewDevice_Name = "0123456789";
            Assert.IsTrue(model.NewDevice_CanCreate);

            model.NewDevice_Name = "_-:.+%#*?!(),=@;$";
            Assert.IsTrue(model.NewDevice_CanCreate);

            model.NewDevice_Name = " ";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "[";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "]";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "{";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "}";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "&";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "^";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "|";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "~";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "\"";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "'";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "\\";
            Assert.IsFalse(model.NewDevice_CanCreate);

            model.NewDevice_Name = "/";
            Assert.IsFalse(model.NewDevice_CanCreate);
        }

        [TestMethod]
        public void TestNewHubInputValidation()
        {
            WizardMain model = new WizardMain();

            Assert.IsFalse(model.NewHub_CanCreate);

            model.NewHub_Name = "hub";
            model.NewHub_SubscriptionName = "sub";
            model.NewHub_ResourceGroupName = "rgr";

            Assert.IsTrue(model.NewHub_CanCreate);

            // verify empty hub name
            model.NewHub_Name = "";
            Assert.IsFalse(model.NewHub_CanCreate);

            // verify empty subscription name
            model.NewHub_Name = "hub";
            model.NewHub_SubscriptionName = "";
            Assert.IsFalse(model.NewHub_CanCreate);

            // verify empty resource group name
            model.NewHub_ResourceGroupName = "";
            model.NewHub_SubscriptionName = "sub";
            Assert.IsFalse(model.NewHub_CanCreate);
            model.NewHub_ResourceGroupName = "rg";

            Assert.IsTrue(model.NewHub_CanCreate);

            // verify all characters that should match hub name
            model.NewHub_Name = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Assert.IsTrue(model.NewHub_CanCreate);

            // verify all characters that should match resource group name
            model.NewHub_ResourceGroupName = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._-()";
            Assert.IsTrue(model.NewHub_CanCreate);

            // period at the end of resource group name is not allowed
            model.NewHub_ResourceGroupName = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._-().";
            Assert.IsFalse(model.NewHub_CanCreate);

            // verify that resource group name can be one character
            model.NewHub_ResourceGroupName = "a";
            Assert.IsTrue(model.NewHub_CanCreate);

            // verify that hub name can be single character
            model.NewHub_Name = "a";
            Assert.IsTrue(model.NewHub_CanCreate);

            // verify characters not allowed in hub name
            foreach (char ch in " _-:.+%#*?!(),=@;$[]{}&^|~\"'\\/")
            {
                model.NewHub_Name = "aaa" + ch + "bbb";
                Assert.IsFalse(model.NewHub_CanCreate);
                model.NewHub_Name = ch + "bbb";
                Assert.IsFalse(model.NewHub_CanCreate);
                model.NewHub_Name = "aaa + " + ch;
                Assert.IsFalse(model.NewHub_CanCreate);
            }

            model.NewHub_Name = "abc";
            Assert.IsTrue(model.NewHub_CanCreate);

            // verify characters not allowed in resource group name
            foreach (char ch in " :+%#*?!,=@;$[]{}&^|~\"'\\/")
            {
                model.NewHub_ResourceGroupName = "aaa" + ch + "bbb";
                Assert.IsFalse(model.NewHub_CanCreate);
                model.NewHub_ResourceGroupName = ch + "bbb";
                Assert.IsFalse(model.NewHub_CanCreate);
                model.NewHub_ResourceGroupName = "aaa + " + ch;
                Assert.IsFalse(model.NewHub_CanCreate);
            }

            model.NewHub_ResourceGroupName = "xyz";
            Assert.IsTrue(model.NewHub_CanCreate);
        }

        [TestMethod]
        public void TestHubSelected()
        {
            AzureIoTHubFake hub = new AzureIoTHubFake();
            WizardMain model = new WizardMain();

            //{ "IoTHubName", iotHubAccount.Name },
            //    { "Region", iotHubAccount.Location },
            //    { "SubscriptionName", subscription.SubscriptionName },
            //    { "ResourceGroup", iotHubAccount.ResourceGroup },
            //    { "Tier", iotHubAccount.Tier() },
            //    { "iotHubUri", iotHubAccount.Properties.HostName },

            hub.WritableProperties.Add("IoTHubName", "testhub");
            hub.WritableProperties.Add("iotHubUri", "test.azuredevices.net");

            model.SelectedHub = hub;

            //Assert.AreEqual<string>("test.azuredevices.net", model.SelectedHubHost);
            Assert.AreEqual<string>("testhub", model.IoTHubName);
        }

        [TestMethod]
        public void TestHubUnselected()
        {
        }

        [TestMethod]
        public void TestDeviceSelected()
        {
        }

        [TestMethod]
        public void TestDeviceUnselected()
        {
        }
    }
}