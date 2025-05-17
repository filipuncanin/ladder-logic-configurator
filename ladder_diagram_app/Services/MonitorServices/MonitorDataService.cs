using System.Text.Json;
using System.Diagnostics;

namespace ladder_diagram_app.Services.MonitorServices
{
    /// <summary>
    /// Processes and displays monitor data received from a device in the main window.
    /// </summary>
    public class MonitorDataService
    {
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorDataService"/> class.
        /// </summary>
        /// <param name="mainWindow">The main window where monitor data will be displayed.</param>
        public MonitorDataService(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Handles incoming monitor data, deserializes it, and updates the main window's monitor display.
        /// </summary>
        /// <param name="monitorData">The JSON string containing monitor data.</param>
        public void OnMonitorDataReceived(string monitorData)
        {
            _mainWindow.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var variables = JsonSerializer.Deserialize<List<MonitorVariable>>(monitorData);
                    if (variables == null)
                    {
                        _mainWindow.MonitorTextBlock.Text = "No data received";
                        return;
                    }
                    var formattedText = string.Join("\n", variables.Select(v => v.ToString()));
                    _mainWindow.MonitorTextBlock.Text = formattedText;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing monitor data: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Represents a variable in the monitor data with properties for various variable types.
        /// </summary>
        private class MonitorVariable
        {
            public string? Type { get; set; }
            public string? Name { get; set; }
            public string? Pin { get; set; }
            public object? Value { get; set; }
            // ADC Sensor
            public string? SensorType { get; set; }
            public string? PD_SCK { get; set; }
            public string? DOUT { get; set; }
            public double? MapLow { get; set; }
            public double? MapHigh { get; set; }
            public double? Gain { get; set; }
            public string? SamplingRate { get; set; }
            // Counter
            public double? PV { get; set; }
            public double? CV { get; set; }
            public bool? CU { get; set; }
            public bool? CD { get; set; }
            public bool? QU { get; set; }
            public bool? QD { get; set; }
            // Timer
            public double? PT { get; set; }
            public double? ET { get; set; }
            public bool? IN { get; set; }
            public bool? Q { get; set; }

            /// <summary>
            /// Returns a string representation of the monitor variable, including all non-null properties.
            /// </summary>
            public override string ToString()
            {
                var parts = new List<string> { $"Type={Type}", $"Name={Name}" };
                if (!string.IsNullOrEmpty(Pin)) parts.Add($"Pin={Pin}");
                if (Value != null) parts.Add($"Value={Value}");
                // ADC Sensor
                if (!string.IsNullOrEmpty(SensorType)) parts.Add($"Sensor Type={SensorType}");
                if (!string.IsNullOrEmpty(PD_SCK)) parts.Add($"PD_SCK={PD_SCK}");
                if (!string.IsNullOrEmpty(DOUT)) parts.Add($"DOUT={DOUT}");
                if (MapLow.HasValue) parts.Add($"Map Low={MapLow.Value}");
                if (MapHigh.HasValue) parts.Add($"Map High={MapHigh.Value}");
                if (Gain.HasValue) parts.Add($"Gain={Gain.Value}");
                if (!string.IsNullOrEmpty(SamplingRate)) parts.Add($"Sampling Rate={SamplingRate}");
                // Counter
                if (PV.HasValue) parts.Add($"PV={PV.Value}");
                if (CV.HasValue) parts.Add($"CV={CV.Value}");
                if (CU.HasValue) parts.Add($"CU={CU.Value}");
                if (CD.HasValue) parts.Add($"CD={CD.Value}");
                if (QU.HasValue) parts.Add($"QU={QU.Value}");
                if (QD.HasValue) parts.Add($"QD={QD.Value}");
                // Timer
                if (PT.HasValue) parts.Add($"PT={PT.Value}");
                if (ET.HasValue) parts.Add($"ET={ET.Value}");
                if (IN.HasValue) parts.Add($"IN={IN.Value}");
                if (Q.HasValue) parts.Add($"Q={Q.Value}");
                return string.Join(", ", parts);
            }
        }
    }
}