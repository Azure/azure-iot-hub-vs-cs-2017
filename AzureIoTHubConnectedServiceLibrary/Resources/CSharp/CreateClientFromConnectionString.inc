        // create Azure IoT Hub client from embedded connection string
        deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);
