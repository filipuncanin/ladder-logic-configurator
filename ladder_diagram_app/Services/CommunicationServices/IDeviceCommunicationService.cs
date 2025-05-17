namespace ladder_diagram_app.Services.CommunicationServices
{
    /// <summary>
    /// Defines the contract for device communication services, supporting connection management and data exchange.
    /// </summary>
    public interface IDeviceCommunicationService : IDisposable
    {
        /// <summary>
        /// Occurs when configuration data is received from the device.
        /// </summary>
        event EventHandler<string> ConfigurationReceived;

        /// <summary>
        /// Occurs when monitor data is received from the device.
        /// </summary>
        event EventHandler<string> MonitorDataReceived;

        /// <summary>
        /// Occurs when one-wire data is received from the device.
        /// </summary>
        event EventHandler<string> OneWireDataReceived;

        /// <summary>
        /// Occurs when the connection status changes.
        /// </summary>
        event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>
        /// Gets a value indicating whether the service is connected to a device.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the type of connection (e.g., "MQTT" or "BLE").
        /// </summary>
        string ConnectionType { get; }

        /// <summary>
        /// Connects to a device asynchronously using the specified identifier.
        /// </summary>
        /// <param name="deviceIdentifier">The identifier of the device to connect to.</param>
        /// <returns>True if connection is successful, otherwise false.</returns>
        Task<bool> ConnectAsync(string deviceIdentifier);

        /// <summary>
        /// Disconnects from the device asynchronously.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Sends a JSON configuration to the device asynchronously.
        /// </summary>
        /// <param name="configJson">The JSON configuration string to send.</param>
        /// <returns>True if sending is successful, otherwise false.</returns>
        Task<bool> SendConfigurationAsync(string configJson);

        /// <summary>
        /// Requests the configuration from the device asynchronously.
        /// </summary>
        Task RequestConfigurationAsync();
    }
}