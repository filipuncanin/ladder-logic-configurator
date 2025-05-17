using ladder_diagram_app.Services.CommunicationServices.MQTT;
using ladder_diagram_app.Services.CommunicationServices.BLE;

namespace ladder_diagram_app.Services.CommunicationServices
{
    /// <summary>
    /// Factory class for creating instances of <see cref="IDeviceCommunicationService"/> based on the specified connection type.
    /// </summary>
    public static class CommunicationServiceFactory
    {
        /// <summary>
        /// Creates a communication service instance for the specified connection type.
        /// </summary>
        /// <param name="connectionType">The type of connection ("MQTT" or "BLE").</param>
        /// <returns>An instance of <see cref="IDeviceCommunicationService"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when an unsupported connection type is provided.</exception>
        public static IDeviceCommunicationService CreateService(string connectionType)
        {
            return connectionType switch
            {
                "MQTT" => new MqttCommunicationService(),
                "BLE" => new BleCommunicationService(),
                _ => throw new ArgumentException("Unsupported connection type", nameof(connectionType))
            };
        }
    }
}