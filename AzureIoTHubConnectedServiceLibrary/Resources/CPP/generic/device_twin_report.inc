/// <summary>
/// Callback when device twin reported properties are accepted by IoT Hub.
/// </summary>
static void reportStatusCallback(int result, void* context)
{
	printf("Reported state accepted by IoT Hub. Result is: %d\n", result);
}

/// <summary>
/// Empty clone function for multitree.
/// </summary>
static int NOPCloneFunction(void** destination, const void* source)
{
	*destination = (void**)source;
	return 0;
}

/// <summary>
/// Empty free function for multitree.
/// </summary>
static void NoFreeFunction(void* value)
{
	(void)value;
}

/// <summary>
/// Updates device twin reported properties.
/// </summary>
void twinReportState(void)
{
	if (iothub_client_handle == NULL)
	{
		printf("ERROR: not initialized\n");
	}
	else
	{
		MULTITREE_HANDLE tree = MultiTree_Create(NOPCloneFunction, NoFreeFunction);

		if (tree != NULL)
		{
			MULTITREE_RESULT mtreeResult = MULTITREE_OK;

$inner$

			if (MULTITREE_OK == mtreeResult)
			{
				STRING_HANDLE res = STRING_new();

				if (res != NULL)
				{
					if (JSON_ENCODER_OK == JSONEncoder_EncodeTree(tree, res, JSONEncoder_CharPtr_ToString))
					{
						const char* json = STRING_c_str(res);

						if (IoTHubClient_LL_SendReportedState(iothub_client_handle, (unsigned char*)json, strlen(json), reportStatusCallback, 0) != IOTHUB_CLIENT_OK)
						{
							printf("ERROR: failed to set reported state\n");
						}
						else
						{
							printf("Reported state set\n");
						}
					}
					else
					{
						printf("ERROR: failed to encode JSON\n");
					}

					STRING_delete(res);
				}
				else
				{
					printf("ERROR: failed to allocate buffer for JSON\n");
				}
			}

			MultiTree_Destroy(tree);
		}
		else
		{
			printf("Failed to allocate multitree\n");
		}
	}
}
