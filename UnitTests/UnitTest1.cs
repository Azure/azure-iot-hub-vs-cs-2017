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

            model.CurrentHub = hub;
            Assert.AreEqual<string>("testhub", model.CurrentHub_Name);

            model.CurrentHub = null;
        }

        [TestMethod]
        public void TestHubUnselected()
        {
            AzureIoTHubFake hub = new AzureIoTHubFake();
            WizardMain model = new WizardMain();

            hub.WritableProperties.Add("IoTHubName", "testhub");
            hub.WritableProperties.Add("iotHubUri", "test.azuredevices.net");

            model.CurrentHub = hub;
            Assert.AreEqual<string>("testhub", model.CurrentHub_Name);

            model.CurrentHub = null;

            Assert.AreEqual<string>("", model.CurrentHub_Host);
            Assert.AreEqual<string>("", model.CurrentHub_Name);
            Assert.AreEqual<string>("", model.CurrentHub_ConnectionString);
        }

        public void TestHubFromConnectionString()
        {
            WizardMain model = new WizardMain();

            model.CurrentHub_ConnectionString = "XXXXX-XXXX-XXXXX-CONNECTION-STRING";
        }



        [TestMethod]
        public void TestDeviceSelected()
        {
        }

        [TestMethod]
        public void TestDeviceUnselected()
        {
        }

        [TestMethod]
        public void TestDeviceCreateSuccessful()
        {
            WizardMain model = new WizardMain();
            model.NewDevice_Name = "newdevice";

            model.CreateNewDevice();

            // verify that hub is currently selected
            Assert.IsNotNull(model.CurrentDevice);
            Assert.AreEqual<string>("newdevice", model.CurrentDevice_Id);

            // verify that fields are cleared
            Assert.AreEqual<string>("", model.NewDevice_Name);
            Assert.IsTrue(model.NewDevice_FieldsEnabled);

        }

        [TestMethod]
        public void TestDeviceCreateFailed()
        {

        }

        [TestMethod]
        public void TestHubCreateSuccessful()
        {
            WizardMain model = new WizardMain();
            model.NewHub_Name = "newhubname";
            model.NewHub_SubscriptionName = "test subscription";
            model.NewHub_ResourceGroupName = "testrg";

            model.CreateNewHub();

            // verify if hub was correctly added to the list

            // verify that hub is currently selected
            Assert.IsNotNull(model.CurrentHub);
            Assert.AreEqual<string>("newhubname.azuredevices.net", model.CurrentHub_Host);
            Assert.AreEqual<string>("newhubname", model.CurrentHub_Name);
            Assert.AreEqual<string>("HostName=newhubname.azuredevices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=fakekey-fakekey-fakekey", model.CurrentHub_ConnectionString);

            // verify that fields are cleared
            Assert.AreEqual<string>("", model.NewHub_Name);
            Assert.AreEqual<string>("", model.NewHub_ResourceGroupName);
            Assert.AreEqual<string>("", model.NewHub_SubscriptionName);
            Assert.IsTrue(model.NewHub_FieldsEnabled);

            // verify that message was displayed


            // verify that appropriate property updates were called
        }

        [TestMethod]
        public void TestHubCreateFailed()
        {
            WizardMain model = new WizardMain();
            model.NewHub_Name = "newhubname";
            model.NewHub_SubscriptionName = "test subscription";
            model.NewHub_ResourceGroupName = "testrg";

            // make sure next operation fails
            model.simulateOperationFailure = true;

            model.CreateNewHub();

            // verify no new hub was created
            Assert.IsNull(model.CurrentHub);
            Assert.AreEqual<string>("", model.CurrentHub_Host);
            Assert.AreEqual<string>("", model.CurrentHub_Name);
            Assert.AreEqual<string>("", model.CurrentHub_ConnectionString);

            // verify that fields are remain unchanged
            Assert.AreEqual<string>("newhubname", model.NewHub_Name);
            Assert.AreEqual<string>("testrg", model.NewHub_ResourceGroupName);
            Assert.AreEqual<string>("test subscription", model.NewHub_SubscriptionName);
            Assert.IsTrue(model.NewHub_FieldsEnabled);
            Assert.IsTrue(model.NewHub_CanCreate);
          
            // verify that error message was displayed
        }
    }
}
