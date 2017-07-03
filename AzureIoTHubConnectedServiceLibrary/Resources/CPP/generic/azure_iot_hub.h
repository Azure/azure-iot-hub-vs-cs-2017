/// \file config.h
/// \brief This header defines a sample interface for performing basic operations with Azure
/// IoT Hub
///
/// Add #include "azure_iot_hub.h" in your project.
/// Call runDemo() to run entire demo or call specific function to perform specific operations.
#pragma once

/// <summary>
///     Runs entire demo, depending on the configuration:
///      - connection to IoT Hub
///      - sending messages
///      - receiving messages
///      - handling direct method calls
///      - updating device twin
///      - receiving device twin updates
/// </summary>
void runDemo();

/// <summary>
///     Establishes connection to Azure IoT Hub
/// </summary>
bool clientConnect();

/// <summary>
///     Disconnect from Azure IoT Hub and releases resources
/// </summary>
void clientDisconnect();

/// <summary>
///     Reports device state using device twin
/// </summary>
void twinReportState();

/// <summary>
///     Sends sample message.
/// </summary>
void sendMessage();

/// <summary>
///     Keeps IoT Hub Client alive.
///     Call this function periodically.
/// </summary>
void doWork();
