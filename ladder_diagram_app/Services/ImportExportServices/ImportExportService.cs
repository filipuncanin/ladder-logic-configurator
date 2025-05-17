using System.Text.Json;
using ladder_diagram_app.Models.CanvasElements;
using ladder_diagram_app.Models.DeviceElement;
using ladder_diagram_app.Models.Variables;
using ladder_diagram_app.Views;
using ladder_diagram_app.Models.CanvasElements.Instances;
using System.Diagnostics;
using ladder_diagram_app.Services.CanvasServices;

namespace ladder_diagram_app.Services.ImportExportServices
{
    /// <summary>
    /// Handles importing and exporting of ladder diagram configurations to and from JSON format.
    /// </summary>
    public class ImportExportService
    {
        private readonly VariablesManager _variablesManager;
        private readonly Device _device;
        private readonly WiresManager _wiresManager;
        private readonly CanvasManager _canvasManager;
        private readonly DevicePinManager _devicePinManager;
        private bool _abortExport;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportExportService"/> class.
        /// </summary>
        /// <param name="variablesManager">Manages variables for ladder elements.</param>
        /// <param name="device">The device configuration.</param>
        /// <param name="wiresManager">Manages wires on the canvas.</param>
        /// <param name="canvasManager">Manages canvas rendering.</param>
        /// <param name="devicePinManager">Manages device pin configurations.</param>
        public ImportExportService(
            VariablesManager variablesManager,
            Device device,
            WiresManager wiresManager,
            CanvasManager canvasManager,
            DevicePinManager devicePinManager)
        {
            _variablesManager = variablesManager;
            _device = device;
            _wiresManager = wiresManager;
            _canvasManager = canvasManager;
            _devicePinManager = devicePinManager;
        }

        /// <summary>
        /// Exports the current configuration to a JSON string.
        /// </summary>
        /// <param name="owner">The main window for displaying notifications.</param>
        /// <returns>The JSON string representing the configuration, or null if export fails.</returns>
        public string? ExportToJson(MainWindow owner)
        {
            var exportData = PrepareExportData(owner);
            if (exportData == null) return null;

            try
            {
                return JsonSerializer.Serialize(exportData);
            }
            catch (Exception ex)
            {
                new NotificationWindow($"Export failed: {ex.Message}", owner).Show();
                return null;
            }
        }

        /// <summary>
        /// Imports a configuration from a JSON string and updates the application state.
        /// </summary>
        /// <param name="jsonString">The JSON string to import.</param>
        /// <param name="owner">The main window for displaying notifications.</param>
        public void ImportFromJson(string jsonString, MainWindow owner)
        {
            try
            {
                var importData = JsonSerializer.Deserialize<ExportData>(jsonString);
                if (importData == null)
                {
                    new NotificationWindow("Failed to parse JSON data.", owner).Show();
                    return;
                }

                // Update Device
                if (importData.Device != null)
                {
                    _device.UpdateFrom(importData.Device);
                    if (!_device.Validate())
                    {
                        new NotificationWindow("Imported device data is invalid.", owner).Show();
                        return;
                    }
                    _devicePinManager.UpdateDevicePinOptions(_device);
                }
                else
                {
                    new NotificationWindow("Imported device data is null.", owner).Show();
                    return;
                }

                // Clear existing data
                _variablesManager.ClearVariablesList();
                _variablesManager.Device = _device;
                _wiresManager.ClearWires();
                owner.MainCanvas.Children.Clear();

                // Import Variables
                if (importData.Variables != null)
                {
                    foreach (var varData in importData.Variables)
                    {
                        if (!varData.TryGetValue("Type", out var typeObj) || typeObj == null ||
                            !varData.TryGetValue("Name", out var nameObj) || nameObj == null)
                        {
                            Debug.WriteLine("Skipping variable: Type or Name is missing or null");
                            continue;
                        }

                        string type = ((JsonElement)typeObj).GetString() ?? string.Empty;
                        string name = ((JsonElement)nameObj).GetString() ?? string.Empty;

                        if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(name))
                        {
                            Debug.WriteLine($"Skipping variable: Type='{type}', Name='{name}' is empty");
                            continue;
                        }

                        string pinName = varData.TryGetValue("Pin", out var pinObj) && pinObj is JsonElement pinJe && pinJe.ValueKind == JsonValueKind.String ? pinJe.ToString() : "";
                        string sensorType = varData.TryGetValue("Sensor Type", out var stObj) && stObj is JsonElement stJe && stJe.ValueKind == JsonValueKind.String ? stJe.ToString() : "";
                        string pdSck = varData.TryGetValue("PD_SCK", out var pdObj) && pdObj is JsonElement pdJe && pdJe.ValueKind == JsonValueKind.String ? pdJe.ToString() : "";
                        string dout = varData.TryGetValue("DOUT", out var doutObj) && doutObj is JsonElement doutJe && doutJe.ValueKind == JsonValueKind.String ? doutJe.ToString() : "";
                        string samplingRate = varData.TryGetValue("Sampling Rate", out var srObj) && srObj is JsonElement srJe && srJe.ValueKind == JsonValueKind.String ? srJe.ToString() : "";
                        double mapLow = varData.TryGetValue("Map Low", out var mlObj) && mlObj is JsonElement mlJe && mlJe.ValueKind == JsonValueKind.Number ? mlJe.GetDouble() : 0.0;
                        double mapHigh = varData.TryGetValue("Map High", out var mhObj) && mhObj is JsonElement mhJe && mhJe.ValueKind == JsonValueKind.Number ? mhJe.GetDouble() : 0.0;
                        double gain = varData.TryGetValue("Gain", out var gainObj) && gainObj is JsonElement gainJe && gainJe.ValueKind == JsonValueKind.Number ? gainJe.GetDouble() : 0.0;
                        bool boolValue = varData.TryGetValue("Value", out var boolObj) && boolObj is JsonElement boolJe && boolJe.ValueKind == JsonValueKind.True;
                        double numValue = varData.TryGetValue("Value", out var numObj) && numObj is JsonElement numJe && numJe.ValueKind == JsonValueKind.Number ? numJe.GetDouble() : 0.0;
                        double timeValue = varData.TryGetValue("Value", out var timeObj) && timeObj is JsonElement timeJe && timeJe.ValueKind == JsonValueKind.Number ? timeJe.GetDouble() : 0.0;
                        double pv = varData.TryGetValue("PV", out var pvObj) && pvObj is JsonElement pvJe && pvJe.ValueKind == JsonValueKind.Number ? pvJe.GetDouble() : 0.0;
                        double cv = varData.TryGetValue("CV", out var cvObj) && cvObj is JsonElement cvJe && cvJe.ValueKind == JsonValueKind.Number ? cvJe.GetDouble() : 0.0;
                        bool cu = varData.TryGetValue("CU", out var cuObj) && cuObj is JsonElement cuJe && cuJe.ValueKind == JsonValueKind.True;
                        bool cd = varData.TryGetValue("CD", out var cdObj) && cdObj is JsonElement cdJe && cdJe.ValueKind == JsonValueKind.True;
                        double pt = varData.TryGetValue("PT", out var ptObj) && ptObj is JsonElement ptJe && ptJe.ValueKind == JsonValueKind.Number ? ptJe.GetDouble() : 0.0;
                        double et = varData.TryGetValue("ET", out var etObj) && etObj is JsonElement etJe && etJe.ValueKind == JsonValueKind.Number ? etJe.GetDouble() : 0.0;

                        _variablesManager.AddVariable(
                            name: name,
                            type: type,
                            pinName: pinName,
                            sensorType: sensorType,
                            pdSck: pdSck,
                            dout: dout,
                            samplingRate: samplingRate,
                            mapLow: mapLow,
                            mapHigh: mapHigh,
                            gain: gain,
                            boolValue: boolValue,
                            numValue: numValue,
                            pv: pv,
                            cv: cv,
                            cu: cu,
                            cd: cd,
                            pt: pt,
                            et: et,
                            timeValue: timeValue,
                            owner: owner
                        );
                    }
                }

                // Import Wires
                if (importData.Wires != null)
                {
                    foreach (var wireData in importData.Wires)
                    {
                        var wire = new Wire();
                        if (wireData.Nodes != null)
                        {
                            wire.Nodes = ImportNodes(wireData.Nodes, wire);
                            _wiresManager.AddWire(wire);
                        }
                    }
                }

                _canvasManager.UpdateCanvas();
            }
            catch (Exception ex)
            {
                new NotificationWindow($"Import failed", owner).Show();
                Debug.WriteLine($"Import failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Prepares the data for export, validating variables and collecting device, variables, and wire information.
        /// </summary>
        /// <param name="owner">The main window for displaying notifications.</param>
        /// <returns>The prepared export data, or null if validation fails.</returns>
        private ExportData? PrepareExportData(MainWindow owner)
        {
            if (!_variablesManager.ValidateVariables(owner))
                return null;

            _abortExport = false;

            var exportData = new ExportData
            {
                Device = _device,
                Variables = _variablesManager.VariablesList
                    .Where(v => !v.Name.Contains("     "))
                    .Select(v => v.ToExportDictionary())
                    .Where(dict => dict != null)
                    .ToList(),
                Wires = _wiresManager.Wires.Select(w => new ExportWire
                {
                    Nodes = ExportNodes(w.Nodes)
                }).ToList()
            };

            if (_abortExport)
            {
                new NotificationWindow("All element comboboxes must have a selected value", owner).Show();
                return null;
            }

            return exportData;
        }

        /// <summary>
        /// Converts a list of nodes to a list of exportable node data.
        /// </summary>
        /// <param name="nodes">The nodes to export.</param>
        /// <returns>A list of exportable node data.</returns>
        private List<ExportNode> ExportNodes(List<Node> nodes)
        {
            var exportNodes = new List<ExportNode>();
            foreach (var node in nodes)
            {
                if (node is LadderElement element)
                {
                    exportNodes.Add(new ExportNode
                    {
                        Type = "LadderElement",
                        ElementType = element.Type,
                        ComboBoxValues = element.VariableComboBoxes.Select(cb => cb.SelectedItem?.ToString() ?? string.Empty).ToList()
                    });
                    if (element.VariableComboBoxes.Any(cb => cb.SelectedIndex == -1))
                    {
                        _abortExport = true;
                    }
                }
                else if (node is Branch branch)
                {
                    exportNodes.Add(new ExportNode
                    {
                        Type = "Branch",
                        Nodes1 = ExportNodes(branch.Nodes1),
                        Nodes2 = ExportNodes(branch.Nodes2)
                    });
                }
            }
            return exportNodes;
        }

        /// <summary>
        /// Converts a list of exportable node data back into a list of nodes.
        /// </summary>
        /// <param name="exportNodes">The exportable node data.</param>
        /// <param name="parentWire">The parent wire, if applicable.</param>
        /// <returns>A list of nodes.</returns>
        private List<Node> ImportNodes(List<ExportNode> exportNodes, Wire? parentWire = null)
        {
            var nodes = new List<Node>();
            foreach (var exportNode in exportNodes)
            {
                if (exportNode.Type == "LadderElement")
                {
                    if (exportNode.ElementType == null)
                    {
                        Debug.WriteLine("Skipping LadderElement: ElementType is null");
                        continue;
                    }
                    var element = new LadderElement(
                        exportNode.ElementType,
                        _variablesManager.VariablesListContacts,
                        _variablesManager.VariablesListCoils,
                        _variablesManager.VariablesListMath,
                        _variablesManager.VariablesListCompare,
                        _variablesManager.VariablesListCounter,
                        _variablesManager.VariablesListTimer,
                        _variablesManager.VariablesListReset
                    );

                    if (exportNode.ComboBoxValues != null)
                    {
                        for (int i = 0; i < exportNode.ComboBoxValues.Count && i < element.VariableComboBoxes.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(exportNode.ComboBoxValues[i]))
                            {
                                element.VariableComboBoxes[i].SelectedItem = exportNode.ComboBoxValues[i];
                            }
                        }
                    }

                    element.Parent = parentWire;
                    nodes.Add(element);
                }
                else if (exportNode.Type == "Branch")
                {
                    var branch = new Branch();
                    branch.Parent = parentWire;
                    branch.Nodes1 = exportNode.Nodes1 != null ? ImportNodes(exportNode.Nodes1, parentWire) : new List<Node>();
                    branch.Nodes2 = exportNode.Nodes2 != null ? ImportNodes(exportNode.Nodes2, parentWire) : new List<Node>();
                    foreach (var node in branch.Nodes1) node.Parent = branch;
                    foreach (var node in branch.Nodes2) node.Parent = branch;
                    nodes.Add(branch);
                }
            }
            return nodes;
        }

        private class ExportData
        {
            public Device? Device { get; set; }
            public List<Dictionary<string, object>>? Variables { get; set; }
            public List<ExportWire>? Wires { get; set; }
        }

        private class ExportWire
        {
            public List<ExportNode>? Nodes { get; set; }
        }

        private class ExportNode
        {
            public string? Type { get; set; }
            public string? ElementType { get; set; }
            public List<string>? ComboBoxValues { get; set; }
            public List<ExportNode>? Nodes1 { get; set; }
            public List<ExportNode>? Nodes2 { get; set; }
        }
    }
}