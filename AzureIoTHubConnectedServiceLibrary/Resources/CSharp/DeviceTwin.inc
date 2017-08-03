
    public static async Task GetDeviceTwinAsync()
    {
        CreateClient();

        Console.WriteLine("Getting device twin");
        Twin twin = await deviceClient.GetTwinAsync();
        Console.WriteLine(twin.ToJson());
    }

    private static async Task OnDesiredPropertiesUpdated(TwinCollection desiredProperties, object userContext)
    {
        Console.WriteLine("Desired properties were updated");
        Console.WriteLine(desiredProperties.ToJson());
    }

    public static async Task RegisterTwinUpdateAsync()
    {
        CreateClient();

        Console.WriteLine("Registering Device Twin update callback");
        await deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertiesUpdated, null);
    }

    public static async Task UpdateDeviceTwin()
    {
        CreateClient();

        TwinCollection tc = new TwinCollection();
        tc["SampleProperty1"] = "test value";

        Console.WriteLine("Updating Device Twin reported properties");
        await deviceClient.UpdateReportedPropertiesAsync(tc);
    }
