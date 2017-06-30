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
    }
}
