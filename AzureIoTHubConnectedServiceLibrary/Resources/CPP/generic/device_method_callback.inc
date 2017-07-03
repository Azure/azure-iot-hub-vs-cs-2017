/// <summary>
/// Callback when device method is called.
/// </summary>
static int deviceMethodCallback(const char *methodName,
	const unsigned char *payload,
	size_t size,
	unsigned char **response,
	size_t *response_size,
	void *userContextCallback)
{
	Log_Debug("Try to invoke method %s\n", methodName);
	const char *responseMessage = onSuccess;
	int result = 200;

$inner$
	else
	{
		LogError("No method %s found\r\n", methodName);
		responseMessage = notFound;
		result = 404;
	}

	*response_size = strlen(responseMessage);
	*response = (unsigned char *)malloc(*response_size);

	if (*response != NULL)
	{
		strncpy((char*)(*response), responseMessage, *response_size);
	}

	return result;
}
