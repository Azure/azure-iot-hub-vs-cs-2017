using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

class AzureIoTHub
{
    public AzureIoTHub()
    {
        // create Azure IoT Hub client from embedded connection string
        deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);
    }

    DeviceClient deviceClient = null;

    //
    // Note: this connection string is specific to the device "kraaa". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=mukan.azure-devices.net;DeviceId=kraaa;SharedAccessKey=a8AhFsuEm/yVcIzBkox1qBz35tTUrX90FR7JH1E0Hck=";

    //
    // To monitor messages sent to device "kraaa" use iothub-explorer as follows:
    //    iothub-explorer monitor-events --login HostName=mukan.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=ZAX49V76BSFs/Idg5N8cqPCnIx1FTW8/u03nUjv2HOU= "kraaa"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    public async Task SendDeviceToCloudMessageAsync()
    {
#if WINDOWS_UWP
        var str = "{\"deviceId\":\"kraaa\",\"messageId\":1,\"text\":\"Hello, Cloud from a UWP C# app!\"}";
#else
        var str = "{\"deviceId\":\"kraaa\",\"messageId\":1,\"text\":\"Hello, Cloud from a C# app!\"}";
#endif
        var message = new Message(Encoding.ASCII.GetBytes(str));

        await deviceClient.SendEventAsync(message);
    }

    async Task<MethodResponse> DirectMethodCallback(MethodRequest methodRequest, object userContext)
    {
        Console.WriteLine("Method has been called");
        return new MethodResponse(new byte[] { 1, 2, 3, 4, 5 }, 200);
    }

    public async Task RegisterDirectMethodsAsync()
    {
        await deviceClient.SetMethodHandlerAsync("myMethod", DirectMethodCallback, null);
    }

    public async Task GetDeviceTwinAsync()
    {
        Twin twin = await deviceClient.GetTwinAsync();

        Console.WriteLine(twin.ToJson());
    }

    private async Task OnDesiredPropertiesUpdated(TwinCollection desiredProperties, object userContext)
    {
        Console.WriteLine("Desired properties updated");
        Console.WriteLine(desiredProperties.ToJson());
    }

    public async Task RegisterTwinUpdateAsync()
    {
        await deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertiesUpdated, null);
    }

    public async Task UpdateDeviceTwin()
    {
        TwinCollection tc = new TwinCollection();

        tc["property1"] = "value1";
        tc["property2"] = "value2";

        await deviceClient.UpdateReportedPropertiesAsync(tc);
    }

    public async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
