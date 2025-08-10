# ModbusRx - A Reactive Modbus Implementation

![License](https://img.shields.io/github/license/ChrisPulman/ModbusRx.svg) [![Build](https://github.com/ChrisPulman/ModbusRx/actions/workflows/BuildOnly.yml/badge.svg)](https://github.com/ChrisPulman/ModbusRx/actions/workflows/BuildOnly.yml) ![Nuget](https://img.shields.io/nuget/dt/ModbusRx?color=pink&style=plastic) [![NuGet](https://img.shields.io/nuget/v/ModbusRx.svg?style=plastic)](https://www.nuget.org/packages/ModbusRx)

![Alt](https://repobeats.axiom.co/api/embed/003b94efc502d37d72af8cc355f4cf4ef95f317a.svg "Repobeats analytics image")

<p align="left">
  <a href="https://github.com/ChrisPulman/ModbusRx">
    <img alt="ModbusRx" src="https://github.com/ChrisPulman/ModbusRx/blob/main/Images/ModbusRx.png" width="200"/>
  </a>
</p>

## Overview

ModbusRx is a modern, reactive implementation of the Modbus protocol for .NET applications. Built on the foundation of NModbus4 and leveraging Reactive Extensions (Rx.NET), it provides a powerful, observable-based API for industrial communication scenarios.

### Key Features

- **Full Modbus Protocol Support**: RTU, ASCII, TCP, and UDP variants
- **Reactive Design**: Built with Rx.NET for responsive, event-driven applications
- **Master/Slave Architecture**: Complete client and server implementations
- **High Performance**: Optimized for speed with memory-efficient operations
- **Comprehensive Testing**: Extensive unit and integration test coverage
- **WPF Visualization**: Built-in components for data visualization and testing
- **Simulation Capabilities**: Advanced simulation modes for development and testing

### Supported Protocols

✅ **Modbus RTU Master/Slave**  
✅ **Modbus ASCII Master/Slave**  
✅ **Modbus TCP Master/Slave**  
✅ **Modbus UDP Master/Slave**  
✅ **Modbus TCP/UDP Server** (New!)

## Quick Start

### Installation

```bash
dotnet add package ModbusRx
```

### Basic TCP Master Example

```csharp
using ModbusRx.Device;
using ModbusRx.Reactive;
using CP.IO.Ports;

// Create a TCP master
var client = new TcpClientRx("127.0.0.1", 502);
using var master = ModbusIpMaster.CreateIp(client);

// Read holding registers
var registers = await master.ReadHoldingRegistersAsync(1, 0, 10);
Console.WriteLine($"Read {registers.Length} registers");

// Write a single register
await master.WriteSingleRegisterAsync(1, 0, 12345);
```

### Reactive TCP Master Example

```csharp
using ModbusRx.Reactive;

// Create a reactive TCP master with automatic connection management
var masterStream = Create.TcpIpMaster("192.168.1.100", 502);

// Continuously read holding registers
var subscription = masterStream
    .ReadHoldingRegisters(startAddress: 0, numberOfPoints: 10, interval: 1000)
    .Subscribe(
        result => 
        {
            if (result.error == null)
            {
                Console.WriteLine($"Registers: [{string.Join(", ", result.data)}]");
            }
            else
            {
                Console.WriteLine($"Error: {result.error.Message}");
            }
        });

// Dispose subscription when done
subscription.Dispose();
```

### Creating a Modbus Server

```csharp
using ModbusRx.Device;
using ModbusRx.Data;

// Create and configure a server
using var server = new ModbusServer();

// Start TCP and UDP endpoints
server.StartTcpServer(502, unitId: 1);
server.StartUdpServer(503, unitId: 1);

// Enable simulation mode for testing
server.SimulationMode = true;

// Start the server
server.Start();

// Load test data
server.LoadSimulationData(
    holdingRegisters: [1, 2, 3, 4, 5],
    inputRegisters: [10, 20, 30, 40, 50],
    coils: [true, false, true, false, true],
    inputs: [false, true, false, true, false]
);

Console.WriteLine("Server running. Press any key to stop.");
Console.ReadKey();
```

### Serial RTU Master Example

```csharp
using CP.IO.Ports;
using ModbusRx.Device;

// Configure serial port
using var port = new SerialPortRx("COM1")
{
    BaudRate = 9600,
    DataBits = 8,
    Parity = Parity.None,
    StopBits = StopBits.One
};

await port.Open();

// Create RTU master
using var master = ModbusSerialMaster.CreateRtu(port);

// Read coils
var coils = await master.ReadCoilsAsync(slaveAddress: 1, startAddress: 0, numberOfPoints: 16);
Console.WriteLine($"Coils: {string.Join(", ", coils.Select(c => c ? "1" : "0"))}");
```

## Advanced Features

### Reactive Data Observation

```csharp
using ModbusRx.Reactive;

// Create a server and observe data changes
using var server = new ModbusServer();

// Observe holding register changes
server.ObserveHoldingRegisters(startAddress: 0, count: 10, interval: 100)
    .Subscribe(registers =>
    {
        Console.WriteLine($"Holding registers changed: [{string.Join(", ", registers)}]");
    });

// Observe coil changes
server.ObserveCoils(startAddress: 0, count: 8, interval: 100)
    .Subscribe(coils =>
    {
        Console.WriteLine($"Coils changed: {string.Join("", coils.Select(c => c ? "1" : "0"))}");
    });
```

### Data Type Conversions

```csharp
using ModbusRx.Reactive;

// Read registers and convert to float
var registers = await master.ReadHoldingRegistersAsync(1, 0, 2);
var floatValue = registers.ToFloat(0); // Convert registers at index 0-1 to float

// Read registers and convert to double
var doubleRegisters = await master.ReadHoldingRegistersAsync(1, 0, 4);
var doubleValue = doubleRegisters.ToDouble(0); // Convert registers at index 0-3 to double

// Convert float to registers for writing
var outputRegisters = new ushort[2];
123.45f.FromFloat(outputRegisters, 0);
await master.WriteMultipleRegistersAsync(1, 0, outputRegisters);
```

### Simulation and Testing

```csharp
using ModbusRx.Data;

// Create simulation data provider
using var simulator = new SimulationDataProvider();
var dataStore = DataStoreFactory.CreateDefaultDataStore();

// Generate test patterns
var sineWave = SimulationDataProvider.GenerateSineWave(100, amplitude: 32767);
var squareWave = SimulationDataProvider.GenerateSquareWave(100, highValue: 65535, lowValue: 0);
var randomData = simulator.GenerateRandomData(100, minValue: 0, maxValue: 1000);

// Load predefined test patterns
simulator.LoadTestPattern(dataStore, TestPattern.CountingUp);
simulator.LoadTestPattern(dataStore, TestPattern.SineWave);
simulator.LoadTestPattern(dataStore, TestPattern.Random);

// Start dynamic simulation
simulator.Start(dataStore, TimeSpan.FromMilliseconds(500), SimulationType.SineWave);
```

### Error Handling and Resilience

```csharp
using ModbusRx.Reactive;

var masterStream = Create.TcpIpMaster("192.168.1.100", 502);

masterStream
    .ReadHoldingRegisters(0, 10, 1000)
    .Retry(3) // Retry up to 3 times on errors
    .Subscribe(
        result =>
        {
            if (result.error != null)
            {
                Console.WriteLine($"Error after retries: {result.error.Message}");
            }
            else
            {
                Console.WriteLine($"Success: {string.Join(", ", result.data)}");
            }
        });
```

## Protocol Details

### Function Codes Supported

| Function Code | Description | Master | Slave |
|---------------|-------------|---------|-------|
| 01 | Read Coils | ✅ | ✅ |
| 02 | Read Discrete Inputs | ✅ | ✅ |
| 03 | Read Holding Registers | ✅ | ✅ |
| 04 | Read Input Registers | ✅ | ✅ |
| 05 | Write Single Coil | ✅ | ✅ |
| 06 | Write Single Register | ✅ | ✅ |
| 15 | Write Multiple Coils | ✅ | ✅ |
| 16 | Write Multiple Registers | ✅ | ✅ |
| 23 | Read/Write Multiple Registers | ✅ | ✅ |

### Address Ranges

| Data Type | Address Range | Description |
|-----------|---------------|-------------|
| Coils | 1-65536 | Read/Write discrete outputs |
| Discrete Inputs | 1-65536 | Read-only discrete inputs |
| Holding Registers | 1-65536 | Read/Write 16-bit registers |
| Input Registers | 1-65536 | Read-only 16-bit registers |

## WPF Visualization

ModbusRx includes a comprehensive WPF visualization component for real-time data monitoring and testing.

```csharp
using ModbusRx.Visualization;

// Create view model for WPF binding
var viewModel = new ModbusServerViewModel();

// The view model provides:
// - Real-time data display for registers and coils
// - Server start/stop controls
// - Simulation pattern selection
// - Test data loading capabilities
```

## Configuration and Performance

### Connection Settings

```csharp
// TCP Master with custom settings
var client = new TcpClientRx("192.168.1.100", 502);
using var master = ModbusIpMaster.CreateIp(client);

// Configure timeouts and retries
master.Transport.ReadTimeout = 5000; // 5 seconds
master.Transport.Retries = 3;
master.Transport.WaitToRetryMilliseconds = 1000;
```

### Serial Port Configuration

```csharp
using var port = new SerialPortRx("COM1")
{
    BaudRate = 19200,
    DataBits = 8,
    Parity = Parity.Even,
    StopBits = StopBits.One,
    Handshake = Handshake.None
};

await port.Open();
using var master = ModbusSerialMaster.CreateRtu(port);
```

## Testing

### Unit Testing with ModbusRx

```csharp
[Test]
public async Task TestModbusRead()
{
    // Create test server
    using var server = new ModbusServer();
    server.StartTcpServer(10502, 1);
    server.Start();
    
    // Load test data
    server.LoadSimulationData([1, 2, 3, 4, 5]);
    
    // Create client and test
    var client = new TcpClientRx("127.0.0.1", 10502);
    using var master = ModbusIpMaster.CreateIp(client);
    
    var result = await master.ReadHoldingRegistersAsync(1, 0, 5);
    
    Assert.AreEqual(5, result.Length);
    Assert.AreEqual(1, result[0]);
    Assert.AreEqual(5, result[4]);
}
```

### Integration Testing

```csharp
[Test]
public void TestReactiveServer()
{
    using var server = new ModbusServer();
    server.SimulationMode = true;
    server.Start();
    
    var dataReceived = false;
    
    server.ObserveDataChanges(50)
        .Take(1)
        .Subscribe(_ => dataReceived = true);
    
    Thread.Sleep(100);
    Assert.IsTrue(dataReceived);
}
```

## Performance Guidelines

### Best Practices

1. **Connection Pooling**: Reuse connections when possible
2. **Batch Operations**: Use bulk read/write operations for multiple registers
3. **Appropriate Intervals**: Don't poll faster than necessary
4. **Error Handling**: Implement proper retry logic with exponential backoff
5. **Resource Cleanup**: Always dispose of connections and subscriptions

### Memory Optimization

```csharp
// Use optimized data store operations
dataStore.ReadDataOptimized<RegisterCollection, ushort>(
    dataSource: dataStore.HoldingRegisters,
    startAddress: 0,
    count: 100);

dataStore.WriteDataOptimized(
    items: newValues,
    destination: dataStore.HoldingRegisters,
    startAddress: 0);
```

## Troubleshooting

### Common Issues

1. **Connection Timeouts**: Increase timeout values for slow networks
2. **Address Errors**: Ensure addresses are within valid ranges (1-65536)
3. **Function Code Errors**: Verify the slave supports the requested function
4. **Serial Port Issues**: Check baud rate, parity, and stop bit settings

### Debugging

```csharp
// Enable debug logging (in development)
Debug.WriteLine("Modbus operation completed");

// Use reactive operators for debugging
masterStream
    .ReadHoldingRegisters(0, 10)
    .Do(result => Debug.WriteLine($"Received: {result.data?.Length} registers"))
    .Subscribe();
```

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 8+ SDK
3. Run `dotnet restore`
4. Run `dotnet build`
5. Run `dotnet test`

## License

ModbusRx is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Support

- 📖 [Documentation](https://github.com/ChrisPulman/ModbusRx/wiki)
- 🐛 [Issue Tracker](https://github.com/ChrisPulman/ModbusRx/issues)
- 💬 [Discussions](https://github.com/ChrisPulman/ModbusRx/discussions)

## Acknowledgments

- Based on [NModbus4](https://github.com/NModbus/NModbus)
- Built with [Reactive Extensions](https://github.com/dotnet/reactive)

---

**ModbusRx** - Making industrial communication reactive and modern! 🏭⚡
