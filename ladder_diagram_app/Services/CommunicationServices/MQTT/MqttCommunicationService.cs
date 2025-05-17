using System.Diagnostics;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Configuration;

namespace ladder_diagram_app.Services.CommunicationServices.MQTT
{
    /// <summary>
    /// Provides MQTT communication services for connecting to and interacting with a device using MQTT protocol.
    /// </summary>
    public class MqttCommunicationService : IDeviceCommunicationService, IDisposable
    {
        private readonly IMqttClient _mqttClient;
        private string _macAddress = string.Empty;

        public static readonly string BrokerAddress = ConfigurationManager.AppSettings["MqttBrokerAddress"] ?? throw new ConfigurationErrorsException("MqttBrokerAddress is missing in App.config");
        public static readonly int BrokerPort = int.Parse(ConfigurationManager.AppSettings["MqttBrokerPort"] ?? throw new ConfigurationErrorsException("MqttBrokerPort is missing in App.config"));
        public static readonly string? BrokerUsername = ConfigurationManager.AppSettings["MqttBrokerUsername"];
        public static readonly string? BrokerPassword = ConfigurationManager.AppSettings["MqttBrokerPassword"];

        private const string MqttTopicConnectionRequest = "/connection_request";
        private const string MqttTopicConnectionResponse = "/connection_response";
        private const string MqttTopicMonitor = "/monitor";
        private const string MqttTopicOneWire = "/one_wire";
        private const string MqttTopicConfigRequest = "/config_request";
        private const string MqttTopicConfigResponse = "/config_response";
        private const string MqttTopicConfig = "/config_device";

        public event EventHandler<string>? ConfigurationReceived;
        public event EventHandler<string>? MonitorDataReceived;
        public event EventHandler<string>? OneWireDataReceived;
        public event EventHandler<bool>? ConnectionStatusChanged;

        /// <summary>
        /// Gets a value indicating whether the service is connected to the device.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets the type of connection, which is "MQTT".
        /// </summary>
        public string ConnectionType => "MQTT";

        private System.Timers.Timer? _presentTimer;
        private DateTime? _lastMonitorMessageTime = null;
        private const int ChunkSize = 800; // Adjusted for ESP32-S3 buffer
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttCommunicationService"/> class.
        /// </summary>
        public MqttCommunicationService()
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            SetupEventHandlers();
        }

        /// <summary>
        /// Sets up MQTT client event handlers for connection, disconnection, and message receipt.
        /// </summary>
        private void SetupEventHandlers()
        {
            _mqttClient.ConnectedAsync += async e =>
            {
                await SubscribeToTopics();
                await RequestConnectionAsync();
            };

            _mqttClient.DisconnectedAsync += e =>
            {
                IsConnected = false;
                _presentTimer?.Stop();
                _presentTimer?.Dispose();
                ConnectionStatusChanged?.Invoke(this, false);
                return Task.CompletedTask;
            };

            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                HandleIncomingMessage(e.ApplicationMessage.Topic, message);
                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// Connects to the MQTT broker and initiates communication with the specified device.
        /// </summary>
        /// <param name="deviceId">The MAC address of the device to connect to.</param>
        /// <returns>True if connection is successful, otherwise false.</returns>
        public async Task<bool> ConnectAsync(string deviceId)
        {
            try
            {
                if (IsConnected) return true;

                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    Debug.WriteLine("MQTT Connection failed: Device MAC Address is null or empty");
                    return false;
                }

                _macAddress = deviceId.ToUpper();

                var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                    .WithTcpServer(BrokerAddress, BrokerPort)
                    .WithClientId(Guid.NewGuid().ToString());

                if (!string.IsNullOrEmpty(BrokerUsername) && !string.IsNullOrEmpty(BrokerPassword))
                {
                    mqttClientOptionsBuilder = mqttClientOptionsBuilder.WithCredentials(BrokerUsername, BrokerPassword);
                }

                var mqttClientOptions = mqttClientOptionsBuilder.Build();

                int retries = 3;
                for (int i = 0; i < retries; i++)
                {
                    try
                    {
                        await _mqttClient.ConnectAsync(mqttClientOptions);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (i == retries - 1)
                        {
                            Debug.WriteLine($"MQTT Connection failed after {retries} retries: {ex.Message}");
                            return false;
                        }
                        await Task.Delay(1000);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT Connection setup failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the MQTT broker and cleans up resources.
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (!IsConnected || _mqttClient == null) return;

                await UnsubscribeFromTopics();

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"{_macAddress}{MqttTopicConnectionRequest}")
                    .WithPayload("Disconnect")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();
                await _mqttClient.PublishAsync(message);

                await _mqttClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT Disconnection failed: {ex.Message}");
            }
            finally
            {
                IsConnected = false;
                _presentTimer?.Stop();
                _presentTimer?.Dispose();
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        /// <summary>
        /// Subscribes to relevant MQTT topics for the device.
        /// </summary>
        private async Task SubscribeToTopics()
        {
            try
            {
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter($"{_macAddress}{MqttTopicConnectionResponse}", MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithTopicFilter($"{_macAddress}{MqttTopicConfigResponse}", MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithTopicFilter($"{_macAddress}{MqttTopicMonitor}", MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithTopicFilter($"{_macAddress}{MqttTopicOneWire}", MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();
                await _mqttClient.SubscribeAsync(subscribeOptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT Subscription failed: {ex.Message}");
                await DisconnectAsync();
            }
        }

        /// <summary>
        /// Unsubscribes from all MQTT topics for the device.
        /// </summary>
        private async Task UnsubscribeFromTopics()
        {
            try
            {
                var unsubscribeOptions = new MqttClientUnsubscribeOptionsBuilder()
                    .WithTopicFilter($"{_macAddress}{MqttTopicConnectionResponse}")
                    .WithTopicFilter($"{_macAddress}{MqttTopicMonitor}")
                    .WithTopicFilter($"{_macAddress}{MqttTopicOneWire}")
                    .WithTopicFilter($"{_macAddress}{MqttTopicConfigResponse}")
                    .Build();
                await _mqttClient.UnsubscribeAsync(unsubscribeOptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT Unsubscription failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles incoming MQTT messages based on their topic.
        /// </summary>
        /// <param name="topic">The topic of the message.</param>
        /// <param name="message">The message payload.</param>
        private void HandleIncomingMessage(string topic, string message)
        {
            if (string.IsNullOrEmpty(_macAddress) || string.IsNullOrEmpty(topic)) return;

            if (topic == $"{_macAddress}{MqttTopicConnectionResponse}" && message == "Connected")
            {
                IsConnected = true;
                ConnectionStatusChanged?.Invoke(this, true);
                _lastMonitorMessageTime = DateTime.Now;
                InitializePresentTimer();
                Task.Run(() => RequestConfigurationAsync());
            }
            else if (topic == $"{_macAddress}{MqttTopicMonitor}")
            {
                _lastMonitorMessageTime = DateTime.Now;
                MonitorDataReceived?.Invoke(this, message);
            }
            else if (topic == $"{_macAddress}{MqttTopicConfigResponse}")
            {
                ConfigurationReceived?.Invoke(this, message);
            }
            else if (topic == $"{_macAddress}{MqttTopicOneWire}")
            {
                OneWireDataReceived?.Invoke(this, message);
            }
        }

        /// <summary>
        /// Requests a connection to the device via MQTT.
        /// </summary>
        public async Task RequestConnectionAsync()
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"{_macAddress}{MqttTopicConnectionRequest}")
                    .WithPayload("Connect")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT Requesting Connection failed: {ex.Message}");
                await DisconnectAsync();
            }
        }

        /// <summary>
        /// Requests the device's configuration via MQTT.
        /// </summary>
        public async Task RequestConfigurationAsync()
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"{_macAddress}{MqttTopicConfigRequest}")
                    .WithPayload("Request Configuration")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT Requesting Configuration failed: {ex.Message}");
                await DisconnectAsync();
            }
        }

        /// <summary>
        /// Initializes a timer to periodically send "Present" messages to maintain connection.
        /// </summary>
        private void InitializePresentTimer()
        {
            _presentTimer = new System.Timers.Timer(1000) { AutoReset = true };
            _presentTimer.Elapsed += async (s, args) =>
            {
                try
                {
                    if (IsConnected)
                    {
                        if (_lastMonitorMessageTime.HasValue && (DateTime.Now - _lastMonitorMessageTime.Value).TotalSeconds > 10)
                        {
                            await _mqttClient.DisconnectAsync();
                            return;
                        }

                        var message = new MqttApplicationMessageBuilder()
                            .WithTopic($"{_macAddress}{MqttTopicConnectionRequest}")
                            .WithPayload("Present")
                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                            .Build();
                        await _mqttClient.PublishAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in MQTT timer callback: {ex.Message}");
                }
            };
            _presentTimer.Start();
        }

        /// <summary>
        /// Sends a JSON configuration to the device in chunks.
        /// </summary>
        /// <param name="configJson">The JSON configuration string to send.</param>
        /// <returns>True if sending is successful, otherwise false.</returns>
        public async Task<bool> SendConfigurationAsync(string configJson)
        {
            try
            {
                if (!IsConnected)
                {
                    Debug.WriteLine("MQTT Sending Configuration failed: Not connected");
                    return false;
                }

                if (string.IsNullOrEmpty(configJson))
                {
                    Debug.WriteLine("MQTT Sending Configuration failed: Config JSON is null or empty");
                    return false;
                }

                for (int i = 0; i < configJson.Length; i += ChunkSize)
                {
                    int chunkLength = Math.Min(ChunkSize, configJson.Length - i);
                    string chunk = configJson.Substring(i, chunkLength);

                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic($"{_macAddress}{MqttTopicConfig}")
                        .WithPayload(chunk)
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();

                    await _mqttClient.PublishAsync(message);
                    await Task.Delay(50); // Delay for reliable reception
                }

                Debug.WriteLine("Configuration sent successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT Sending Configuration failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disposes of the service, releasing resources.
        /// </summary>
        /// <param name="disposing">Indicates whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _presentTimer?.Stop();
                    _presentTimer?.Dispose();
                    if (_mqttClient != null && _mqttClient.IsConnected)
                    {
                        try
                        {
                            _ = _mqttClient.DisconnectAsync();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"MQTT Dispose disconnect failed: {ex.Message}");
                        }
                    }
                    _mqttClient?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes of the service and releases resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}