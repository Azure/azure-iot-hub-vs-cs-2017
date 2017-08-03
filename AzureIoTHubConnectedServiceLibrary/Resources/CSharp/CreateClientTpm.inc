        Microsoft.Devices.Tpm.TpmDevice myDevice = new Microsoft.Devices.Tpm.TpmDevice(0); // Use logical device 0 on the TPM
        string hubUri = myDevice.GetHostName();
        string deviceId = myDevice.GetDeviceId();
        string sasToken = myDevice.GetSASToken();

        var deviceClient = DeviceClient.Create(
            hubUri,
            AuthenticationMethodFactory.
                CreateAuthenticationWithToken(deviceId, sasToken), TransportType.Mqtt);
