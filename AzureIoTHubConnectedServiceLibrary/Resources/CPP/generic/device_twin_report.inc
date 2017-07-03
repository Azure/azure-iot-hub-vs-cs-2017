/// <summary>
/// Callback when device twin reported properties are accepted by IoT Hub.
/// </summary>
static void reportStatusCallback(int result, void* context)
{
	Log_Debug("Reported state accepted by IoT Hub. Result is: %d\n", result);
}

/// <summary>
/// Updates device twin reported properties.
/// </summary>
void twinReportState(void)
{
	if (iothub_client_handle == NULL)
	{
		Log_Debug("ERROR: not initialized\n");
	}
	else
	{
		const char* state = "$inner$";

		if (IoTHubClient_LL_SendReportedState(iothub_client_handle, (unsigned char*)state, strlen(state), reportStatusCallback, 0) != IOTHUB_CLIENT_OK)
		{
			Log_Debug("ERROR: failed to set reported state\n");
		}
		else
		{
			Log_Debug("Reported state sent\n");
		}
	}
}
