/// <summary>
/// Callback when device twin update is received from IoT Hub.
/// </summary>
static void twinCallback(DEVICE_TWIN_UPDATE_STATE updateState,
	const unsigned char *payLoad,
	size_t size,
	void *userContextCallback)
{
	char *temp = (char *)malloc(size + 1);

	if (temp == NULL)
	{
		Log_Debug("ERROR: couldn't allocate buffer for twin update payload\n");
		return;
	}

	for (int i = 0; i < size; i++)
	{
		temp[i] = (char)(payLoad[i]);
	}
	temp[size] = '\0';
	MULTITREE_HANDLE tree = NULL;

	if (JSON_DECODER_OK == JSONDecoder_JSON_To_MultiTree(temp, &tree))
	{
		MULTITREE_HANDLE child = NULL;

		if (MULTITREE_OK != MultiTree_GetChildByName(tree, "desired", &child))
		{
			LogInfo("This device twin message contains desired message only");
			child = tree;
		}
		const void *value = NULL;

$inner$
	}
	MultiTree_Destroy(tree);
	free(temp);
}
