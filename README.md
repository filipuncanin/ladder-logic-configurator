# ESP32 Ladder Logic Configuration Tool

## Overview
The **Ladder Diagram Configurator** is a C# WPF application designed for creating and managing ladder logic diagrams to control ESP32 microcontrollers. It enables users to visually design control logic, manage variables, and remotely configure ESP32 devices via Bluetooth Low Energy (BLE) or MQTT protocols. The application sends JSON configurations to the ESP32, eliminating the need for reprogramming when updating functionality. This README provides a comprehensive guide to the application's features, installation, usage, and technical details, leveraging ESP32 firmware documentation for system synergy.

## Purpose and Scope
The application aims to provide engineers, developers, and hobbyists with an intuitive tool for designing and implementing ladder logic on ESP32 devices. Through a graphical interface, users can create logic, configure variables, and monitor real-time data, making it ideal for automation, IoT, and industrial projects. This README serves as a guide for understanding, installing, and using the application, emphasizing its interaction with the ESP32.

## Key Features
The application offers a range of features to streamline interaction with ESP32 devices:

| **Feature** | **Description** |
|-------------|---------------|
| **Ladder Diagram Editor** | Enables creation and editing of diagrams with elements like NO/NC contacts, coils, timers, counters, and comparison operations using a drag-and-drop interface. |
| **Variable Management** | Supports defining variables such as digital inputs/outputs, booleans, numerics, timers, counters, ADC sensors, and one-wire inputs. |
| **Device Communication** | Connects to ESP32 via BLE or MQTT for sending configurations and receiving real-time data. |
| **Monitoring Panel** | Displays variable statuses and sensor data, such as temperature from one-wire sensors. |
| **Configuration Import/Export** | Allows saving and loading configurations in JSON format for sharing and persistence. |
| **User Interface** | Intuitive WPF interface with windows for device selection, notifications, and time picking. |

## System Architecture
The system comprises two main components:

1. **C# WPF Application**:
   - **Role**: Provides a graphical interface for designing ladder logic, configuring variables, and communicating with the ESP32.
   - **Components**:
     - **Diagram Editor**: Facilitates ladder diagram creation using classes like `CanvasManager` and `CanvasInteractionManager`.
     - **Variable Management**: Supports various variable types via `VariablesManager` and classes like `TimerVariable` and `OneWireInputVariable`.
     - **Communication Services**: Implemented through `BleCommunicationService` and `MqttCommunicationService` using asynchronous connections.
     - **Monitoring Panel**: Displays real-time data via `MonitorDataService` and `OneWireDataService`.
   - **Technologies**: C# with WPF, .NET Framework, MQTTnet for MQTT, and Windows.Devices.Bluetooth for BLE.

2. **ESP32 Firmware**:
   - **Role**: Executes ladder logic, manages hardware, and communicates with the application.
   - **Functionalities**: Receives JSON configurations via BLE (using characteristics like `WRITE_CONFIGURATION_CHAR_UUID`) or MQTT (via topics like `/config_device`), parses them using the `cJSON` library, applies settings, and stores them in Non-Volatile Storage (NVS).

**Communication Flow**:
- **BLE Configuration**:
  - The application sends JSON configurations to the `WRITE_CONFIGURATION_CHAR_UUID` characteristic in chunks due to MTU size limitations.
  - It can read the current configuration from `READ_CONFIGURATION_CHAR_UUID`.
- **MQTT Configuration**:
  - The application publishes configurations to the `/config_device` topic.
  - It can request configurations using `/config_request` and receive responses on `/config_response`.
- **Data Exchange**:
  - The ESP32 publishes sensor and variable state data to the `/monitor` topic, which the application subscribes to for real-time updates.

## Inter-Device Communication

The application supports configuring inter-device communication, enabling multiple ESP32 devices to share data. Users can assign one or more parent devices to an ESP32 in the hardware configuration section of the interface. Once set, the device automatically sends its data (e.g., variable states, sensor readings) to its designated parent devices via MQTT. This feature facilitates coordinated control in distributed IoT or automation systems.

- **Configuration**: In the C# APP Parent(s) configuration panel, specify parent devices by adding their MAC addresses.

## Hardware Requirements
The following hardware is required to use the application:

| **Component** | **Details** |
|---------------|-------------|
| **Microcontroller** | ESP32 module (e.g., ESP32-S3) with firmware supporting BLE/MQTT. |
| **Sensors** | Supports ADC sensors (e.g., TM7711) and one-wire sensors (e.g., DS18x20). |
| **Peripherals** | Configured GPIO pins, one-wire interfaces. |
| **Network** | Wi-Fi for MQTT or BLE for local communication. |
| **Computer** | Windows PC with Bluetooth support or access to an MQTT broker. |

## Software Requirements
The application has the following software requirements:

| **Component** | **Details** |
|---------------|-------------|
| **Operating System** | Windows 10 or later. |
| **.NET Framework** | Version 4.7.2 or higher. |
| **Development Tools** | Visual Studio for building the application. |
| **Dependencies** | MQTTnet for MQTT communication, Windows.Devices.Bluetooth for BLE. |
| **ESP32 Firmware** | Compatible firmware (not included in this repository). |

## Installation
To set up the application, follow these steps:

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/your-repo/ladder_diagram_app.git
   cd ladder_diagram_app
   ```

2. **Build the Application**:
   - Open the `.sln` file in Visual Studio.
   - Restore NuGet packages to install dependencies.
   - Build the solution in Debug or Release mode.

3. **Configure Communication Settings**:
   - **MQTT**: Edit `App.config` to include the MQTT broker URI, username, and password.
   - **BLE**: Ensure your PC has Bluetooth capabilities and installed drivers.

4. **Set Up the ESP32 Device**:
   - Ensure the ESP32 has firmware supporting BLE or MQTT communication.
   - Configure the device to connect to the same MQTT broker or be discoverable via BLE.

## Usage
The application is designed for intuitive use, with the following workflow:

### Designing Ladder Diagrams
1. **Launch the Application**:
   - Run the executable from Visual Studio or the built binary.

2. **Add Elements**:
   - Use the toolbar to select ladder diagram elements (e.g., NO contact, coil, timer).
   - Drag and drop elements from the toolbar onto the canvas to place them.

3. **Configure Elements**:
   - Select an element to enable mapping to specific variables through a selection interface.
   - Example: Map an NO contact to a digital input variable by choosing from available variables.

4. **Connect Elements**:
   - Place elements on rungs, where each rung represents a set of conditions.
   - Add branches to rungs to represent parallel conditions. Branches can be nested within other branches to create multiple parallel conditions for complex logic.

5. **Branches on Rungs**:
   - Branches are a powerful feature allowing parallel logic paths within a single rung. Each branch represents an independent set of conditions that can evaluate in parallel with other branches.
   - Nested branches enable complex logic by allowing branches within branches, supporting multiple layers of parallel conditions.
   - Example: A rung might have a main branch checking a sensor input and a parallel branch checking a timer, with a nested branch in the timer branch evaluating a counter condition.
   - Use the drag-and-drop interface to add branches to rungs and configure their elements, ensuring flexibility in designing sophisticated control logic.

6. **Manage Variables**:
   - In the variables panel, add variables by specifying type, name, and other required properties.
   - Example: Add a `TimerVariable` with a preset time or a `CounterVariable` with an initial value.

7. **Save and Export**:
   - Save the diagram locally via the application.
   - Export the configuration as a JSON file for device transfer.

### Connecting to the Device
1. **Select Communication Method**:
   - Choose BLE for local connections or MQTT for network setups.

2. **BLE Connection**:
   - Open the BLE device selection window.
   - Scan for ESP32 devices (e.g., named `ESP_XXYYZZ`).
   - Connect to the desired device.

3. **MQTT Connection**:
   - Ensure the ESP32 is subscribed to the MQTT broker.
   - Connect the application to the same broker using configured topics.
   - Specify the MAC address of the ESP32 device to establish a connection.

4. **Send Configuration**:
   - Load or create a configuration.
   - Send it to the ESP32, which applies it immediately.

### Monitoring
1. **View Real-Time Data**:
   - Use the monitoring panel to see variable states and sensor readings.
   - Example: Track the value of a one-wire temperature sensor.

2. **One-Wire Sensor Configuration**:
   - Configure one-wire sensors in the variables panel.
   - The one-wire section displays sensors in three states:
     - **Present on the device and added to logic**: Sensors detected on the ESP32 and mapped to ladder logic.
     - **Present on the device but not added to logic**: Sensors detected on the ESP32 but not used in the diagram.
     - **Added to logic but not detected on the device**: Sensors configured in the logic but not physically connected or detected.

## Configuration Format
Configurations are JSON files with three main sections:

1. **Device**:
   - Defines hardware settings, such as digital/analog pins.

2. **Variables**:
   - Specifies variable types, names, and properties.

3. **Wires**:
   - Represents ladder logic rungs with nodes (e.g., contacts, coils).

**Example Configuration**:
```json
{
  "Device": {
    "device_name": "ESP32-S3",
    "logic_voltage": 3.3,
    "digital_inputs": [ 48, 47, 33, 34 ],
    "digital_inputs_names": [ "I1", "I2", "I3", "I4" ],
    "digital_outputs": [ 37, 38, 39, 40 ],
    "digital_outputs_names": [ "T REL", "REL1", "REL2", "REL3" ],
    "analog_inputs": [],
    "analog_inputs_names": [],
    "dac_outputs": [],
    "dac_outputs_names": [],
    "one_wire_inputs": [ 36 ],
    "one_wire_inputs_names": [ [] ],
    "one_wire_inputs_devices_types": [ [] ],
    "one_wire_inputs_devices_addresses": [ [] ],
    "pwm_channels": 8,
    "max_hardware_timers": 4,
    "has_rtos": true,
    "UART": [ 1, 2 ],
    "I2C": [ 0, 1 ],
    "SPI": [ 1, 2 ],
    "USB": true,
    "parent_devices": []
  },
  "Variables": [
    {"Type": "Digital Input", "Name": "dig_in_1", "Pin": "I1"},
    {"Type": "Digital Output", "Name": "dig_out_1", "Pin": "T REL"},
    {"Type": "Timer", "Name": "timer_1", "PT": 5000, "ET": 0, "IN": false, "Q": false}
  ],
  "Wires": [
    {
      "Nodes": [
        {"Type": "LadderElement", "ElementType": "NOContact", "ComboBoxValues": ["dig_in_1"]},
        {"Type": "LadderElement", "ElementType": "Coil", "ComboBoxValues": ["dig_out_1"]}
      ]
    }
  ]
}
```
This configuration sets up a simple ladder logic where digital input `I1` controls digital output `T REL`.

## Example: Temperature-Based Control
To control a device based on temperature:

1. **Define Variables**:
   - Add a `OneWireInputVariable` for a temperature sensor on pin 36.
   - Add a `NumericVariable` for the threshold (e.g., 25.0).
   - Add a `DigitalOutputVariable` for the output.

2. **Design Ladder Logic**:
   - Create a rung with a `GreaterCompare` element comparing the temperature to the threshold.
   - Link it to a `Coil` element tied to the output.

3. **Send Configuration**:
   - Export and send the JSON configuration to the ESP32.

The ESP32 will activate the output if the temperature exceeds 25°C.

## Technical Details
- **Language**: C# with WPF for the user interface.
- **Framework**: .NET Framework 4.7.2 or higher.
- **Communication**: BLE uses Windows.Devices.Bluetooth, MQTT uses MQTTnet.
- **Data Format**: JSON for configurations.
- **Dependencies**: Listed in the project's NuGet packages.

## Demo
Watch the Ladder Diagram Configurator in action! This video demonstrates key features, including drag-and-drop diagram creation, branch configuration, and real-time monitoring.

![Ladder Diagram Configurator Demo](./docs/demo.mp4)

## Contributing
1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/your-feature`).
3. Commit your changes (`git commit -m "Add your feature"`).
4. Push to the branch (`git push origin feature/your-feature`).
5. Open a pull request.

## Contact
For support, contact filipuncanin@gmail.com or open a GitHub issue.