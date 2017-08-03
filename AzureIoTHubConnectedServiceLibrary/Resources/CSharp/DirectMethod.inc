
    private static async Task<MethodResponse> OnSampleMethod1Called(MethodRequest methodRequest, object userContext)
    {
        Console.WriteLine("SampleMethod1 has been called");
        return new MethodResponse(200);
    }

    private static async Task<MethodResponse> OnSampleMethod2Called(MethodRequest methodRequest, object userContext)
    {
        Console.WriteLine("SampleMethod2 has been called");
        return new MethodResponse(200);
    }

    public static async Task RegisterDirectMethodsAsync()
    {
        CreateClient();

        Console.WriteLine("Registering direct method callbacks");
        await deviceClient.SetMethodHandlerAsync("SampleMethod1", OnSampleMethod1Called, null);
        await deviceClient.SetMethodHandlerAsync("SampleMethod2", OnSampleMethod2Called, null);
    }
