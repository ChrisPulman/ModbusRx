# ModbusRx - A Reactive Modbus Implementation

![License](https://img.shields.io/github/license/ChrisPulman/ModbusRx.svg) [![Build](https://github.com/ChrisPulman/ModbusRx/actions/workflows/BuildOnly.yml/badge.svg)](https://github.com/ChrisPulman/ModbusRx/actions/workflows/BuildOnly.yml) ![Nuget](https://img.shields.io/nuget/dt/ModbusRx?color=pink&style=plastic) [![NuGet](https://img.shields.io/nuget/v/ModbusRx.svg?style=plastic)](https://www.nuget.org/packages/ModbusRx)

![Alt](https://repobeats.axiom.co/api/embed/003b94efc502d37d72af8cc355f4cf4ef95f317a.svg "Repobeats analytics image")

<p align="left">
  <a href="https://github.com/ChrisPulman/ModbusRx">
    <img alt="ModbusRx" src="https://github.com/ChrisPulman/ModbusRx/blob/main/Images/ModbusRx.png" width="200"/>
  </a>
</p>

## Overview

ModbusRx is a modern, reactive implementation of the Modbus protocol for .NET applications. Built on the foundation of NModbus4 and leveraging Reactive Extensions (Rx.NET), it provides a powerful, observable-based API for industrial communication scenarios with comprehensive support for all major Modbus variants.

### Key Features

- **🔧 Full Modbus Protocol Support**: RTU, ASCII, TCP, and UDP variants
- **⚡ Reactive Design**: Built with Rx.NET for responsive, event-driven applications  
- **🏭 Master/Slave Architecture**: Complete client and server implementations
- **🚀 High Performance**: Optimized for speed with memory-efficient operations
- **✅ Comprehensive Testing**: Extensive unit and integration test coverage
- **📊 Advanced Simulation**: Built-in simulation capabilities for testing and development
- **📡 Connection Management**: Automatic reconnection and health monitoring
- **🔄 Data Type Conversions**: Built-in support for float, double, and custom data types
- **🛠️ SerialPortRx**: Built on [CP.IO.Ports](https://github.com/ChrisPulman/SerialPortRx) for robust, Reactive serial communication

### Supported Protocols & Target Frameworks

**Protocols:**
- ✅ **Modbus RTU Master/Slave** (Serial)
- ✅ **Modbus ASCII Master/Slave** (Serial)  
- ✅ **Modbus TCP Master/Slave** (Ethernet)
- ✅ **Modbus UDP Master/Slave** (Ethernet)
- ✅ **Modbus TCP/UDP Server** with client aggregation

**Target Frameworks:**
- `.NET Standard 2.0` (Cross-platform compatibility)
- `.NET 8` (Long-term support)
- `.NET 9` (Latest features)
- `.NET Framework 4.8` (Legacy support)

## Installation

```bash
dotnet add package ModbusRx
```

Or via Package Manager Console:

```powershell
Install-Package ModbusRx
```

## Quick Start Guide

### 1. Basic TCP Master Operations

#### Simple TCP Master Connection

```csharp
using ModbusRx.Device;
using CP.IO.Ports;

// Create a TCP master
var client = new TcpClientRx("192.168.1.100", 502);
using var master = ModbusIpMaster.CreateIp(client);

// Read holding registers (Function Code 03)
var registers = await master.ReadHoldingRegistersAsync(
    slaveAddress: 1, 
    startAddress: 0, 
    numberOfPoints: 10);

Console.WriteLine($"Read {registers.Length} registers: [{string.Join(", ", registers)}]");

// Write a single register (Function Code 06)  
await master.WriteSingleRegisterAsync(
    slaveAddress: 1, 
    registerAddress: 0, 
    value: 12345);

// Write multiple registers (Function Code 16)
var dataToWrite = new ushort[] { 100, 200, 300, 400, 500 };
await master.WriteMultipleRegistersAsync(
    slaveAddress: 1, 
    startAddress: 10, 
    data: dataToWrite);
```

#### Advanced TCP Master with Error Handling

```csharp
using ModbusRx.Device;
using CP.IO.Ports;

try
{
    var client = new TcpClientRx("192.168.1.100", 502)
    {
        ReadTimeout = 5000,   // 5 second timeout
        WriteTimeout = 5000
    };
    
    using var master = ModbusIpMaster.CreateIp(client);
    
    // Configure transport settings
    master.Transport!.ReadTimeout = 5000;
    master.Transport.Retries = 3;
    master.Transport.WaitToRetryMilliseconds = 1000;
    
    // Read all data types
    var coils = await master.ReadCoilsAsync(1, 0, 16);
    var discreteInputs = await master.ReadInputsAsync(1, 0, 16);
    var holdingRegisters = await master.ReadHoldingRegistersAsync(1, 0, 10);
    var inputRegisters = await master.ReadInputRegistersAsync(1, 0, 10);
    
    Console.WriteLine($"Coils: {string.Join("", coils.Select(c => c ? "1" : "0"))}");
    Console.WriteLine($"Discrete Inputs: {string.Join("", discreteInputs.Select(d => d ? "1" : "0"))}");
    Console.WriteLine($"Holding Registers: [{string.Join(", ", holdingRegisters)}]");
    Console.WriteLine($"Input Registers: [{string.Join(", ", inputRegisters)}]");
}
catch (ModbusException ex)
{
    Console.WriteLine($"Modbus Error: {ex.Message}");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Timeout Error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"General Error: {ex.Message}");
}
```

### 2. Reactive Masters with Automatic Connection Management

#### TCP and UDP

```csharp
using ModbusRx.Reactive;
using System.Reactive.Linq;

// Reactive TCP master
var tcp = Create.TcpIpMaster("192.168.1.100", 502);
var tcpSub = tcp
    .ReadHoldingRegisters(startAddress: 0, numberOfPoints: 10, interval: 1000)
    .Subscribe(result =>
    {
        if (result.error == null && result.data != null)
        {
            Console.WriteLine($"TCP: [{string.Join(", ", result.data)}]");
        }
    });

// Reactive UDP master
var udp = Create.UdpIpMaster("192.168.1.101", 502);
var udpSub = udp
    .ReadHoldingRegisters(startAddress: 0, numberOfPoints: 10, interval: 1000)
    .Subscribe(result =>
    {
        if (result.error == null && result.data != null)
        {
            Console.WriteLine($"UDP: [{string.Join(", ", result.data)}]");
        }
    });
```

#### Reactive Serial RTU/ASCII Masters

```csharp
using ModbusRx.Reactive;
using System.IO.Ports;
using System.Reactive.Linq;

// Reactive Serial RTU master
var rtu = Create.SerialRtuMaster("COM3", 19200, 8, Parity.None, StopBits.One);
var rtuSub = rtu
    .ReadHoldingRegisters(slaveAddress: 1, startAddress: 0, numberOfPoints: 10, interval: 500)
    .Subscribe(result =>
    {
        if (result.error == null && result.data != null)
        {
            Console.WriteLine($"RTU: [{string.Join(", ", result.data)}]");
        }
    });


// Convenience overload (defaults slave 1)
var rtuQuickSub = rtu
    .ReadHoldingRegisters(startAddress: 0, numberOfPoints: 5, interval: 1000)
    .Subscribe();

// Reactive Serial ASCII master
var ascii = Create.SerialAsciiMaster("COM4", 9600, 7, Parity.Even, StopBits.One);
var asciiSub = ascii
    .ReadCoils(slaveAddress: 1, startAddress: 0, numberOfPoints: 8, interval: 1000)
    .Subscribe(result =>
    {
        if (result.error == null && result.data != null)
        {
            Console.WriteLine($"ASCII Coils: {string.Join("", result.data.Select(c => c ? "1" : "0"))}");
        }
    });
```

### 3. UDP Master Operations

```csharp
using ModbusRx.Device;
using CP.IO.Ports;
using System.Net;

// Create UDP master
var client = new UdpClientRx();
var endPoint = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 502);
client.Connect(endPoint);

using var master = ModbusIpMaster.CreateIp(client);

// UDP operations are similar to TCP
var registers = await master.ReadHoldingRegistersAsync(1, 0, 10);
Console.WriteLine($"UDP Read: [{string.Join(", ", registers)}]");

// Write coils (Function Code 15)
var coilData = new bool[] { true, false, true, true, false, false, true, false };
await master.WriteMultipleCoilsAsync(1, 0, coilData);
```

### 4. Serial RTU Master

```csharp
using CP.IO.Ports;
using ModbusRx.Device;
using System.IO.Ports;

// Configure serial port
using var port = new SerialPortRx("COM1")
{
    BaudRate = 9600,
    DataBits = 8,
    Parity = Parity.None,
    StopBits = StopBits.One,
    Handshake = Handshake.None
};

await port.OpenAsync();

// Create RTU master
using var master = ModbusSerialMaster.CreateRtu(port);

try
{
    // Read coils
    var coils = await master.ReadCoilsAsync(
        slaveAddress: 1, 
        startAddress: 0, 
        numberOfPoints: 16);
    
    Console.WriteLine($"Coils: {string.Join("", coils.Select(c => c ? "1" : "0"))}");
    
    // Read/Write multiple registers (Function Code 23)
    var writeData = new ushort[] { 1000, 2000, 3000 };
    var readData = await master.ReadWriteMultipleRegistersAsync(
        slaveAddress: 1,
        startReadAddress: 0,
        numberOfPointsToRead: 5,
        startWriteAddress: 10,
        writeData: writeData);
    
    Console.WriteLine($"Read/Write Result: [{string.Join(", ", readData)}]");
}
finally
{
    await port.CloseAsync();
}
```

### 5. Serial ASCII Master

```csharp
using CP.IO.Ports;
using ModbusRx.Device;
using System.IO.Ports;

// Configure serial port for ASCII
using var port = new SerialPortRx("COM2")
{
    BaudRate = 9600,
    DataBits = 7,           // ASCII typically uses 7 data bits
    Parity = Parity.Even,   // ASCII typically uses even parity
    StopBits = StopBits.One
};

await port.OpenAsync();

// Create ASCII master
using var master = ModbusSerialMaster.CreateAscii(port);

// ASCII operations are identical to RTU in terms of function calls
var registers = await master.ReadHoldingRegistersAsync(1, 0, 10);
Console.WriteLine($"ASCII Read: [{string.Join(", ", registers)}]");
```

## Advanced Server Implementations

### 1. Creating a Comprehensive Modbus Server

```csharp
using ModbusRx.Device;
using ModbusRx.Data;

// Create and configure a server
using var server = new ModbusServer();

// Start multiple protocol endpoints
var tcpSubscription = server.StartTcpServer(502, unitId: 1);
var udpSubscription = server.StartUdpServer(503, unitId: 1);

// Enable simulation mode for testing
server.SimulationMode = true;

// Start the server
server.Start();

// Load initial test data
server.LoadSimulationData(
    holdingRegisters: new ushort[] { 1, 2, 3, 4, 5, 100, 200, 300, 400, 500 },
    inputRegisters: new ushort[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 },
    coils: new bool[] { true, false, true, false, true, false, true, false },
    inputs: new bool[] { false, true, false, true, false, true, false, true }
);

Console.WriteLine("Server running on TCP:502 and UDP:503");
Console.WriteLine("Press any key to stop...");
Console.ReadKey();

// Cleanup
server.Stop();
tcpSubscription.Dispose();
udpSubscription.Dispose();
```

### 2. TCP Slave Implementation

```csharp
using ModbusRx.Device;
using ModbusRx.Data;
using System.Net.Sockets;

// Create TCP listener
var tcpListener = new TcpListener(IPAddress.Any, 502);

// Create TCP slave
using var slave = ModbusTcpSlave.CreateTcp(unitId: 1, tcpListener);

// Configure custom data store
slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

// Load test data
for (ushort i = 1; i <= 100; i++)
{
    slave.DataStore.HoldingRegisters[i] = (ushort)(i * 10);
    slave.DataStore.InputRegisters[i] = (ushort)(i * 5);
    slave.DataStore.CoilDiscretes[i] = (i % 2) == 0;
    slave.DataStore.InputDiscretes[i] = (i % 3) == 0;
}

// Start listening
var listenTask = Task.Run(async () => await slave.ListenAsync());

Console.WriteLine("TCP Slave listening on port 502...");
Console.WriteLine("Press any key to stop...");
Console.ReadKey();

// Stop the slave
slave.Dispose();
```

### 3. UDP Slave Implementation

```csharp
using ModbusRx.Device;
using CP.IO.Ports;

// Create UDP client for listening
var udpClient = new UdpClientRx(502);

// Create UDP slave
using var slave = ModbusUdpSlave.CreateUdp(unitId: 1, udpClient);

// Start listening
var listenTask = Task.Run(async () => await slave.ListenAsync());

Console.WriteLine("UDP Slave listening on port 502...");
Console.WriteLine("Press any key to stop...");
Console.ReadKey();
```

### 4. Serial RTU Slave

```csharp
using ModbusRx.Device;
using CP.IO.Ports;
using System.IO.Ports;

// Configure serial port
using var port = new SerialPortRx("COM1")
{
    BaudRate = 9600,
    DataBits = 8,
    Parity = Parity.None,
    StopBits = StopBits.One
};

await port.OpenAsync();

// Create RTU slave
using var slave = ModbusSerialSlave.CreateRtu(unitId: 1, port);

// Start listening
var listenTask = Task.Run(async () => await slave.ListenAsync());

Console.WriteLine("RTU Slave listening on COM1...");
Console.WriteLine("Press any key to stop...");
Console.ReadKey();
```

## Data Type Conversions and Utilities

### 1. Working with Float and Double Values

```csharp
using ModbusRx.Reactive;

// Read registers and convert to float
var registers = await master.ReadHoldingRegistersAsync(1, 0, 2);
var floatValue = registers.ToFloat(0); // Convert registers at index 0-1 to float
Console.WriteLine($"Float value: {floatValue}");

// Read registers and convert to double
var doubleRegisters = await master.ReadHoldingRegistersAsync(1, 0, 4);
var doubleValue = doubleRegisters.ToDouble(0); // Convert registers at index 0-3 to double
Console.WriteLine($"Double value: {doubleValue}");

// Convert values back to registers for writing
var outputRegisters = new ushort[2];
123.45f.FromFloat(outputRegisters, 0);
await master.WriteMultipleRegistersAsync(1, 0, outputRegisters);

var outputDoubleRegisters = new ushort[4];
987.654321.FromDouble(outputDoubleRegisters, 0);
await master.WriteMultipleRegistersAsync(1, 10, outputDoubleRegisters);

// Using spans for high-performance operations
var registerSpan = registers.AsSpan();
var floatFromSpan = registerSpan.ToFloat(0);
var doubleFromSpan = registerSpan.ToDouble(2);

var outputSpan = outputRegisters.AsSpan();
456.78f.FromFloat(outputSpan, 0);
```

### 2. Working with Different Word Orders

```csharp
// Handle different byte/word ordering conventions
var registers = await master.ReadHoldingRegistersAsync(1, 0, 4);

// Standard word order (high word first)
var floatStandard = registers.ToFloat(0, swapWords: false);

// Swapped word order (low word first) - common with some PLCs
var floatSwapped = registers.ToFloat(0, swapWords: true);

Console.WriteLine($"Standard: {floatStandard}, Swapped: {floatSwapped}");

// Same applies to double values
var doubleStandard = registers.ToDouble(0, swapWords: false);
var doubleSwapped = registers.ToDouble(0, swapWords: true);
```

## Reactive Programming Features

### 1. Observing Data Changes

```csharp
using ModbusRx.Reactive;
using System.Reactive.Linq;

// Create a server and observe data changes
using var server = new ModbusServer();
server.SimulationMode = true;
server.Start();

// Observe all data changes
var allDataSubscription = server.ObserveDataChanges(interval: 100)
    .Subscribe(data =>
    {
        Console.WriteLine($"Holding Registers: [{string.Join(", ", data.holdingRegisters.Take(5))}]");
        Console.WriteLine($"Coils: {string.Join("", data.coils.Take(8).Select(c => c ? "1" : "0"))}");
    });

// Observe specific holding registers with change detection
var holdingRegSubscription = server.ObserveHoldingRegisters(
        startAddress: 0, 
        count: 10, 
        interval: 100)
    .Subscribe(registers =>
    {
        Console.WriteLine($"Holding registers changed: [{string.Join(", ", registers)}]");
    });

// Observe coil changes
var coilSubscription = server.ObserveCoils(
        startAddress: 0, 
        count: 8, 
        interval: 100)
    .Subscribe(coils =>
    {
        Console.WriteLine($"Coils changed: {string.Join("", coils.Select(c => c ? "1" : "0"))}");
    });

// Observe input registers
var inputRegSubscription = server.ObserveInputRegisters(
        startAddress: 0,
        count: 5,
        interval: 200)
    .Subscribe(registers =>
    {
        Console.WriteLine($"Input registers: [{string.Join(", ", registers)}]");
    });

// Let it run
await Task.Delay(10000);

// Cleanup
allDataSubscription.Dispose();
holdingRegSubscription.Dispose();
coilSubscription.Dispose();
inputRegSubscription.Dispose();
```

### 2. Creating Reactive Servers

```csharp
using ModbusRx.Reactive;

// Create a reactive server that automatically manages lifecycle
var serverSubscription = ModbusServerExtensions.CreateReactiveServer(server =>
{
    // Configure the server
    server.StartTcpServer(502, 1);
    server.StartUdpServer(503, 1);
    server.SimulationMode = true;
    
    // Load initial data
    server.LoadSimulationData(
        holdingRegisters: Enumerable.Range(1, 100).Select(i => (ushort)(i * 10)).ToArray(),
        coils: Enumerable.Range(0, 50).Select(i => i % 2 == 0).ToArray()
    );
})
.Subscribe(
    server => Console.WriteLine("Server started and configured"),
    error => Console.WriteLine($"Server error: {error.Message}"),
    () => Console.WriteLine("Server stopped")
);

// Server runs automatically while subscribed
await Task.Delay(30000);

// Dispose to stop server
serverSubscription.Dispose();
```

### 3. Advanced Reactive Master Operations

```csharp
using ModbusRx.Reactive;
using System.Reactive.Linq;

// Create reactive master streams
var tcpMasterStream = Create.TcpIpMaster("192.168.1.100", 502);
var udpMasterStream = Create.UdpIpMaster("192.168.1.101", 502);

// Combine multiple data sources
var combinedData = Observable.CombineLatest(
    tcpMasterStream.ReadHoldingRegisters(0, 10, 1000),
    udpMasterStream.ReadHoldingRegisters(0, 10, 1000),
    (tcpData, udpData) => new { TCP = tcpData.data, UDP = udpData.data })
.Subscribe(combined =>
{
    if (combined.TCP != null && combined.UDP != null)
    {
        Console.WriteLine($"TCP: [{string.Join(", ", combined.TCP)}]");
        Console.WriteLine($"UDP: [{string.Join(", ", combined.UDP)}]");
    }
});

// Read different data types reactively
var multiDataSubscription = tcpMasterStream
    .ReadHoldingRegisters(0, 5, 1000)
    .CombineLatest(
        tcpMasterStream.ReadCoils(0, 8, 1000),
        tcpMasterStream.ReadInputs(0, 8, 1000),
        (registers, coils, inputs) => new { Registers = registers.data, Coils = coils.data, Inputs = inputs.data })
    .Where(data => data.Registers != null && data.Coils != null && data.Inputs != null)
    .Subscribe(data =>
    {
        Console.WriteLine($"Registers: [{string.Join(", ", data.Registers!)}]");
        Console.WriteLine($"Coils: {string.Join("", data.Coils!.Select(c => c ? "1" : "0"))}");
        Console.WriteLine($"Inputs: {string.Join("", data.Inputs!.Select(i => i ? "1" : "0"))}");
    });

await Task.Delay(30000);

// Cleanup
combinedData.Dispose();
multiDataSubscription.Dispose();
```

## Simulation and Testing Features

### 1. Using the Simulation Data Provider

```csharp
using ModbusRx.Data;

// Create simulation data provider
using var simulator = new SimulationDataProvider();
var dataStore = DataStoreFactory.CreateDefaultDataStore();

// Generate different wave patterns
var sineWave = SimulationDataProvider.GenerateSineWave(
    length: 100, 
    amplitude: 32767, 
    frequency: 1.0, 
    phase: 0.0);

var squareWave = SimulationDataProvider.GenerateSquareWave(
    length: 100, 
    highValue: 65535, 
    lowValue: 0, 
    dutyCycle: 0.3);

var sawtoothWave = SimulationDataProvider.GenerateSawtoothWave(
    length: 100, 
    maxValue: 1000, 
    minValue: 0);

var randomData = simulator.GenerateRandomData(
    length: 100, 
    minValue: 0, 
    maxValue: 1000);

Console.WriteLine($"Sine Wave (first 5): [{string.Join(", ", sineWave.Take(5))}]");
Console.WriteLine($"Square Wave (first 5): [{string.Join(", ", squareWave.Take(5))}]");
Console.WriteLine($"Sawtooth Wave (first 5): [{string.Join(", ", sawtoothWave.Take(5))}]");
Console.WriteLine($"Random Data (first 5): [{string.Join(", ", randomData.Take(5))}]");

// Generate boolean patterns for discrete values
var boolPatterns = new[]
{
    simulator.GenerateBooleanPattern(8, BooleanPattern.AllTrue),
    simulator.GenerateBooleanPattern(8, BooleanPattern.AllFalse),
    simulator.GenerateBooleanPattern(8, BooleanPattern.Alternating),
    simulator.GenerateBooleanPattern(8, BooleanPattern.Random)
};

Console.WriteLine("Boolean Patterns:");
Console.WriteLine($"All True: {string.Join("", boolPatterns[0].Select(b => b ? "1" : "0"))}");
Console.WriteLine($"All False: {string.Join("", boolPatterns[1].Select(b => b ? "1" : "0"))}");
Console.WriteLine($"Alternating: {string.Join("", boolPatterns[2].Select(b => b ? "1" : "0"))}");
Console.WriteLine($"Random: {string.Join("", boolPatterns[3].Select(b => b ? "1" : "0"))}");
```

### 2. Loading Predefined Test Patterns

```csharp
using ModbusRx.Data;

using var simulator = new SimulationDataProvider();
var dataStore = DataStoreFactory.CreateDefaultDataStore();

// Load different test patterns
var patterns = new[]
{
    TestPattern.CountingUp,
    TestPattern.CountingDown,
    TestPattern.SineWave,
    TestPattern.SquareWave,
    TestPattern.Random,
    TestPattern.AllZeros,
    TestPattern.AllOnes
};

foreach (var pattern in patterns)
{
    simulator.LoadTestPattern(dataStore, pattern);
    
    // Get first 5 values to see the pattern
    var holdingRegs = dataStore.HoldingRegisters.Skip(1).Take(5).ToArray();
    var coils = dataStore.CoilDiscretes.Skip(1).Take(8).ToArray();
    
    Console.WriteLine($"{pattern}:");
    Console.WriteLine($"  Holding Registers: [{string.Join(", ", holdingRegs)}]");
    Console.WriteLine($"  Coils: {string.Join("", coils.Select(c => c ? "1" : "0"))}");
}
```

### 3. Dynamic Simulation

```csharp
using ModbusRx.Data;

// Create a server with dynamic simulation
using var server = new ModbusServer();
using var simulator = new SimulationDataProvider();

// Start server
server.StartTcpServer(502, 1);
server.Start();

// Start different simulation types
Console.WriteLine("Starting Random simulation...");
simulator.Start(server.DataStore!, TimeSpan.FromMilliseconds(500), SimulationType.Random);

await Task.Delay(5000);

Console.WriteLine("Switching to Counting Up...");
simulator.Stop();
simulator.Start(server.DataStore!, TimeSpan.FromMilliseconds(200), SimulationType.CountingUp);

await Task.Delay(5000);

Console.WriteLine("Switching to Sine Wave...");
simulator.Stop();
simulator.Start(server.DataStore!, TimeSpan.FromMilliseconds(100), SimulationType.SineWave);

await Task.Delay(5000);

simulator.Stop();
Console.WriteLine("Simulation stopped.");
```

## Error Handling and Resilience

### 1. Comprehensive Error Handling

```csharp
using ModbusRx.Device;
using ModbusRx.Reactive;
using System.Reactive.Linq;

// Create master with comprehensive error handling
async Task<bool> SafeModbusOperation()
{
    try
    {
        var client = new TcpClientRx("192.168.1.100", 502);
        using var master = ModbusIpMaster.CreateIp(client);
        
        var registers = await master.ReadHoldingRegistersAsync(1, 0, 10);
        Console.WriteLine($"Success: [{string.Join(", ", registers)}]");
        return true;
    }
    catch (SlaveException ex)
    {
        Console.WriteLine($"Slave Exception - Function Code: {ex.FunctionCode}, Slave Code: {ex.SlaveExceptionCode}");
        return false;
    }
    catch (InvalidModbusRequestException ex)
    {
        Console.WriteLine($"Invalid Request: {ex.Message}");
        return false;
    }
    catch (ModbusCommunicationException ex)
    {
        Console.WriteLine($"Communication Error: {ex.Message}");
        return false;
    }
    catch (TimeoutException ex)
    {
        Console.WriteLine($"Timeout: {ex.Message}");
        return false;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected Error: {ex.Message}");
        return false;
    }
}

// Use reactive error handling with retry
var masterStream = Create.TcpIpMaster("192.168.1.100", 502);

var resilientRead = masterStream
    .ReadHoldingRegisters(0, 10, 1000)
    .Retry(3) // Retry up to 3 times
    .Catch<(ushort[]? data, Exception? error), Exception>(ex =>
    {
        Console.WriteLine($"All retries failed: {ex.Message}");
        return Observable.Return<(ushort[]? data, Exception? error)>((null, ex));
    })
    .Subscribe(result =>
    {
        if (result.error == null && result.data != null)
        {
            Console.WriteLine($"Data: [{string.Join(", ", result.data)}]");
        }
        else
        {
            Console.WriteLine($"Final error: {result.error?.Message}");
        }
    });

await Task.Delay(30000);
resilientRead.Dispose();
```

### 2. Connection Health Monitoring

```csharp
using ModbusRx.Reactive;
using System.Reactive.Linq;

// Monitor connection health
var masterStream = Create.TcpIpMaster("192.168.1.100", 502);

var healthMonitor = masterStream
    .Select(status => new
    {
        Timestamp = DateTime.Now,
        Connected = status.connected,
        Error = status.error?.Message,
        Master = status.master != null ? "Available" : "Not Available"
    })
    .DistinctUntilChanged(h => h.Connected)
    .Subscribe(health =>
    {
        Console.WriteLine($"[{health.Timestamp:HH:mm:ss}] Connection: {(health.Connected ? "UP" : "DOWN")} - {health.Error ?? "OK"}");
    });

// Read data only when connected
var dataReader = masterStream
    .Where(status => status.connected && status.master != null)
    .SelectMany(status => status.master!.ReadHoldingRegistersAsync(1, 0, 5)
        .ToObservable()
        .Catch<ushort[], Exception>(ex => 
        {
            Console.WriteLine($"Read error: {ex.Message}");
            return Observable.Empty<ushort[]>();
        }))
    .Subscribe(data => Console.WriteLine($"Data: [{string.Join(", ", data)}]"));

await Task.Delay(60000);

healthMonitor.Dispose();
dataReader.Dispose();
```

## Protocol Details and Function Codes

### Supported Function Codes

| Function Code | Description | Master Support | Slave Support | Example Usage |
|---------------|-------------|----------------|---------------|---------------|
| 01 | Read Coils | ✅ | ✅ | `master.ReadCoilsAsync(1, 0, 16)` |
| 02 | Read Discrete Inputs | ✅ | ✅ | `master.ReadInputsAsync(1, 0, 16)` |
| 03 | Read Holding Registers | ✅ | ✅ | `master.ReadHoldingRegistersAsync(1, 0, 10)` |
| 04 | Read Input Registers | ✅ | ✅ | `master.ReadInputRegistersAsync(1, 0, 10)` |
| 05 | Write Single Coil | ✅ | ✅ | `master.WriteSingleCoilAsync(1, 0, true)` |
| 06 | Write Single Register | ✅ | ✅ | `master.WriteSingleRegisterAsync(1, 0, 12345)` |
| 15 | Write Multiple Coils | ✅ | ✅ | `master.WriteMultipleCoilsAsync(1, 0, coilArray)` |
| 16 | Write Multiple Registers | ✅ | ✅ | `master.WriteMultipleRegistersAsync(1, 0, regArray)` |
| 23 | Read/Write Multiple Registers | ✅ | ✅ | `master.ReadWriteMultipleRegistersAsync(...)` |

### Address Ranges and Limitations

| Data Type | Address Range | Max Read/Write | Storage Type |
|-----------|---------------|----------------|--------------|
| Coils | 1-65536 | 2000 per request | `bool[]` |
| Discrete Inputs | 1-65536 | 2000 per request | `bool[]` |
| Holding Registers | 1-65536 | 125 per request | `ushort[]` |
| Input Registers | 1-65536 | 125 per request | `ushort[]` |

### Example: Working with All Function Codes

```csharp
using ModbusRx.Device;
using CP.IO.Ports;

var client = new TcpClientRx("192.168.1.100", 502);
using var master = ModbusIpMaster.CreateIp(client);

// Function Code 01: Read Coils
var coils = await master.ReadCoilsAsync(
    slaveAddress: 1, 
    startAddress: 0, 
    numberOfPoints: 16);
Console.WriteLine($"FC01 - Coils: {string.Join("", coils.Select(c => c ? "1" : "0"))}");

// Function Code 02: Read Discrete Inputs  
var discreteInputs = await master.ReadInputsAsync(
    slaveAddress: 1,
    startAddress: 0,
    numberOfPoints: 16);
Console.WriteLine($"FC02 - Discrete Inputs: {string.Join("", discreteInputs.Select(d => d ? "1" : "0"))}");

// Function Code 03: Read Holding Registers
var holdingRegisters = await master.ReadHoldingRegistersAsync(
    slaveAddress: 1,
    startAddress: 0,
    numberOfPoints: 10);
Console.WriteLine($"FC03 - Holding Registers: [{string.Join(", ", holdingRegisters)}]");

// Function Code 04: Read Input Registers
var inputRegisters = await master.ReadInputRegistersAsync(
    slaveAddress: 1,
    startAddress: 0,
    numberOfPoints: 10);
Console.WriteLine($"FC04 - Input Registers: [{string.Join(", ", inputRegisters)}]");

// Function Code 05: Write Single Coil
await master.WriteSingleCoilAsync(
    slaveAddress: 1,
    coilAddress: 0,
    value: true);
Console.WriteLine("FC05 - Single coil written");

// Function Code 06: Write Single Register
await master.WriteSingleRegisterAsync(
    slaveAddress: 1,
    registerAddress: 0,
    value: 12345);
Console.WriteLine("FC06 - Single register written");

// Function Code 15: Write Multiple Coils
var coilsToWrite = new bool[] { true, false, true, true, false, false, true, false };
await master.WriteMultipleCoilsAsync(
    slaveAddress: 1,
    startAddress: 10,
    data: coilsToWrite);
Console.WriteLine("FC15 - Multiple coils written");

// Function Code 16: Write Multiple Registers
var registersToWrite = new ushort[] { 1000, 2000, 3000, 4000, 5000 };
await master.WriteMultipleRegistersAsync(
    slaveAddress: 1,
    startAddress: 20,
    data: registersToWrite);
Console.WriteLine("FC16 - Multiple registers written");

// Function Code 23: Read/Write Multiple Registers
var readWriteResult = await master.ReadWriteMultipleRegistersAsync(
    slaveAddress: 1,
    startReadAddress: 0,
    numberOfPointsToRead: 5,
    startWriteAddress: 30,
    writeData: new ushort[] { 100, 200, 300 });
Console.WriteLine($"FC23 - Read/Write result: [{string.Join(", ", readWriteResult)}]");
```

## Performance Optimization and Best Practices

### 1. High-Performance Data Operations

```csharp
using ModbusRx.Data;
using System.Buffers;

// Use memory pools for large data operations
var pool = ArrayPool<ushort>.Shared;
var buffer = pool.Rent(1000);

try
{
    // Bulk read operation
    var registers = await master.ReadHoldingRegistersAsync(1, 0, 1000);
    
    // Process data efficiently
    for (int i = 0; i < registers.Length; i++)
    {
        buffer[i] = (ushort)(registers[i] * 2); // Example processing
    }
    
    // Write back efficiently
    await master.WriteMultipleRegistersAsync(1, 1000, buffer.AsSpan(0, registers.Length).ToArray());
}
finally
{
    pool.Return(buffer);
}

// Use spans for zero-copy operations where possible
ReadOnlySpan<ushort> dataSpan = registers;
var floatValue = dataSpan.ToFloat(0);
var doubleValue = dataSpan.ToDouble(2);
```

### 2. Optimized Server Configuration

```csharp
using ModbusRx.Device;
using ModbusRx.Data;

// Create high-performance server
using var server = new ModbusServer();

// Use custom data store for better performance
var customDataStore = new DataStore();

// Pre-allocate data for known size
const int dataSize = 10000;
for (int i = 1; i <= dataSize; i++)
{
    customDataStore.HoldingRegisters.Add((ushort)(i % 65536));
    customDataStore.InputRegisters.Add((ushort)(i % 65536));
    customDataStore.CoilDiscretes.Add((i % 2) == 0);
    customDataStore.InputDiscretes.Add((i % 3) == 0);
}

server.DataStore = customDataStore;

// Start optimized TCP server
server.StartTcpServer(502, 1);
server.Start();

Console.WriteLine($"High-performance server started with {dataSize} data points");
```

### 3. Connection Pooling and Management

```csharp
using ModbusRx.Device;
using CP.IO.Ports;
using System.Collections.Concurrent;

// Simple connection pool
public class ModbusConnectionPool : IDisposable
{
    private readonly ConcurrentQueue<ModbusIpMaster> _availableConnections = new();
    private readonly string _ipAddress;
    private readonly int _port;
    private readonly int _maxConnections;
    private int _currentConnections;

    public ModbusConnectionPool(string ipAddress, int port = 502, int maxConnections = 10)
    {
        _ipAddress = ipAddress;
        _port = port;
        _maxConnections = maxConnections;
    }

    public async Task<ModbusIpMaster?> GetConnectionAsync()
    {
        if (_availableConnections.TryDequeue(out var connection))
        {
            return connection;
        }

        if (_currentConnections < _maxConnections)
        {
            try
            {
                var client = new TcpClientRx(_ipAddress, _port);
                var master = ModbusIpMaster.CreateIp(client);
                Interlocked.Increment(ref _currentConnections);
                return master;
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public void ReturnConnection(ModbusIpMaster connection)
    {
        if (connection != null)
        {
            _availableConnections.Enqueue(connection);
        }
    }

    public void Dispose()
    {
        while (_availableConnections.TryDequeue(out var connection))
        {
            connection.Dispose();
        }
    }
}

// Usage example
using var connectionPool = new ModbusConnectionPool("192.168.1.100", 502, 5);

// Perform multiple concurrent operations
var tasks = Enumerable.Range(0, 20).Select(async i =>
{
    var master = await connectionPool.GetConnectionAsync();
    if (master != null)
    {
        try
        {
            var data = await master.ReadHoldingRegistersAsync(1, (ushort)(i * 10), 10);
            Console.WriteLine($"Task {i}: [{string.Join(", ", data)}]");
        }
        finally
        {
            connectionPool.ReturnConnection(master);
        }
    }
});

await Task.WhenAll(tasks);
```

## Testing and Debugging

### 1. Unit Testing with ModbusRx

```csharp
using ModbusRx.Device;
using ModbusRx.Data;
using Xunit;

public class ModbusTests
{
    [Fact]
    public async Task TestModbusReadWrite()
    {
        // Arrange - Create test server
        using var server = new ModbusServer();
        var port = GetAvailablePort();
        server.StartTcpServer(port, 1);
        server.Start();
        
        // Load test data
        server.LoadSimulationData(new ushort[] { 1, 2, 3, 4, 5 });
        
        // Create client
        var client = new TcpClientRx("127.0.0.1", port);
        using var master = ModbusIpMaster.CreateIp(client);
        
        // Act
        var result = await master.ReadHoldingRegistersAsync(1, 0, 5);
        
        // Assert
        Assert.Equal(5, result.Length);
        Assert.Equal(1, result[0]);
        Assert.Equal(5, result[4]);
    }

    [Fact]
    public async Task TestModbusWriteOperations()
    {
        using var server = new ModbusServer();
        var port = GetAvailablePort();
        server.StartTcpServer(port, 1);
        server.Start();
        
        var client = new TcpClientRx("127.0.0.1", port);
        using var master = ModbusIpMaster.CreateIp(client);
        
        // Test write single register
        await master.WriteSingleRegisterAsync(1, 0, 12345);
        var readResult = await master.ReadHoldingRegistersAsync(1, 0, 1);
        Assert.Equal(12345, readResult[0]);
        
        // Test write multiple registers
        var writeData = new ushort[] { 1000, 2000, 3000 };
        await master.WriteMultipleRegistersAsync(1, 10, writeData);
        var multiReadResult = await master.ReadHoldingRegistersAsync(1, 10, 3);
        Assert.Equal(writeData, multiReadResult);
    }

    private static int GetAvailablePort()
    {
        using var socket = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        socket.Start();
        var port = ((System.Net.IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }
}
```

### 2. Integration Testing with Simulation

```csharp
using ModbusRx.Data;
using Xunit;

public class SimulationTests
{
    [Fact]
    public void TestSimulationPatterns()
    {
        using var provider = new SimulationDataProvider();
        var dataStore = DataStoreFactory.CreateDefaultDataStore();
        
        // Test counting pattern
        provider.LoadTestPattern(dataStore, TestPattern.CountingUp);
        Assert.Equal(0, dataStore.HoldingRegisters[1]);
        Assert.Equal(1, dataStore.HoldingRegisters[2]);
        Assert.Equal(2, dataStore.HoldingRegisters[3]);
        
        // Test sine wave pattern
        provider.LoadTestPattern(dataStore, TestPattern.SineWave);
        Assert.True(dataStore.HoldingRegisters.Skip(1).Take(100).Any(x => x > 0));
    }

    [Fact]
    public void TestWaveGeneration()
    {
        var sineWave = SimulationDataProvider.GenerateSineWave(360, 32767);
        Assert.Equal(360, sineWave.Length);
        Assert.Equal(32767, sineWave[0]); // sin(0) + amplitude
        Assert.True(sineWave[90] > 32767); // sin(90°) = 1
        
        var squareWave = SimulationDataProvider.GenerateSquareWave(100, 1000, 0, 0.5);
        var highCount = squareWave.Count(x => x == 1000);
        Assert.True(Math.Abs(highCount - 50) <= 1); // 50% duty cycle
    }
}
```

### 3. Debugging and Diagnostics

```csharp
using ModbusRx.Device;
using ModbusRx.Reactive;
using System.Diagnostics;

// Enable debug logging
Debug.WriteLine("Starting Modbus operations");

var client = new TcpClientRx("192.168.1.100", 502);
using var master = ModbusIpMaster.CreateIp(client);

// Monitor transport layer
if (master.Transport != null)
{
    Debug.WriteLine($"Transport Type: {master.Transport.GetType().Name}");
    Debug.WriteLine($"Read Timeout: {master.Transport.ReadTimeout}ms");
    Debug.WriteLine($"Retries: {master.Transport.Retries}");
}

try
{
    var stopwatch = Stopwatch.StartNew();
    var registers = await master.ReadHoldingRegistersAsync(1, 0, 10);
    stopwatch.Stop();
    
    Debug.WriteLine($"Read completed in {stopwatch.ElapsedMilliseconds}ms");
    Debug.WriteLine($"Data: [{string.Join(", ", registers)}]");
}
catch (Exception ex)
{
    Debug.WriteLine($"Error: {ex.GetType().Name} - {ex.Message}");
    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
}

// Use reactive operators for debugging
var masterStream = Create.TcpIpMaster("192.168.1.100", 502);

masterStream
    .ReadHoldingRegisters(0, 10, 1000)
    .Do(result => Debug.WriteLine($"Before processing: {result.data?.Length} registers"))
    .Where(result => result.error == null)
    .Do(result => Debug.WriteLine($"After filtering: Success"))
    .Subscribe(
        result => Debug.WriteLine($"Final result: [{string.Join(", ", result.data!)}]"),
        error => Debug.WriteLine($"Observable error: {error.Message}"));
```

## Configuration and Deployment

### 1. Configuration for Different Environments

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModbusRx.Device;

// Configuration class
public class ModbusConfiguration
{
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 502;
    public int ReadTimeout { get; set; } = 5000;
    public int Retries { get; set; } = 3;
    public byte SlaveAddress { get; set; } = 1;
}

// Service registration
public static class ServiceExtensions
{
    public static IServiceCollection AddModbusRx(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<ModbusConfiguration>(
            configuration.GetSection("Modbus"));
        
        services.AddSingleton<IModbusMaster>(provider =>
        {
            var config = provider.GetRequiredService<IOptions<ModbusConfiguration>>().Value;
            var client = new TcpClientRx(config.IpAddress, config.Port)
            {
                ReadTimeout = config.ReadTimeout,
                WriteTimeout = config.ReadTimeout
            };
            
            var master = ModbusIpMaster.CreateIp(client);
            if (master.Transport != null)
            {
                master.Transport.ReadTimeout = config.ReadTimeout;
                master.Transport.Retries = config.Retries;
            }
            
            return master;
        });
        
        return services;
    }
}

// Usage in application
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddModbusRx(builder.Configuration);

var app = builder.Build();

// Use injected Modbus master
app.MapGet("/read-registers", async (IModbusMaster master) =>
{
    var registers = await master.ReadHoldingRegistersAsync(1, 0, 10);
    return Results.Ok(registers);
});
```

### 2. appsettings.json Configuration

```json
{
  "Modbus": {
    "IpAddress": "192.168.1.100",
    "Port": 502,
    "ReadTimeout": 5000,
    "Retries": 3,
    "SlaveAddress": 1
  },
  "ModbusServer": {
    "TcpPort": 502,
    "UdpPort": 503,
    "UnitId": 1,
    "SimulationMode": true,
    "SimulationInterval": 500
  }
}
```

## Troubleshooting Guide

### Common Issues and Solutions

#### 1. Connection Timeouts

**Problem**: Operations timeout frequently
**Solutions**:
```csharp
// Increase timeouts
var client = new TcpClientRx("192.168.1.100", 502)
{
    ReadTimeout = 10000,  // 10 seconds
    WriteTimeout = 10000
};

var master = ModbusIpMaster.CreateIp(client);
master.Transport!.ReadTimeout = 10000;
master.Transport.Retries = 5;
```

#### 2. Address Errors

**Problem**: "Illegal Data Address" exceptions
**Solutions**:
```csharp
// Ensure addresses are within valid ranges (1-65536)
// Check that start + count doesn't exceed device limits

try
{
    var registers = await master.ReadHoldingRegistersAsync(1, 0, 10);
}
catch (SlaveException ex) when (ex.SlaveExceptionCode == 2)
{
    Console.WriteLine("Address out of range - reduce count or change start address");
}
```

#### 3. Serial Port Issues

**Problem**: Serial communication failures
**Solutions**:
```csharp
// Check all serial parameters match device settings
using var port = new SerialPortRx("COM1")
{
    BaudRate = 9600,      // Must match device
    DataBits = 8,         // Check device manual
    Parity = Parity.None, // Common configurations: None, Even, Odd
    StopBits = StopBits.One,
    Handshake = Handshake.None
};

// Verify port is available
var availablePorts = SerialPortRx.GetPortNames();
Console.WriteLine($"Available ports: {string.Join(", ", availablePorts)}");
```

#### 4. Network Connectivity

**Problem**: Cannot connect to remote devices
**Solutions**:
```csharp
// Test network connectivity first
using var ping = new System.Net.NetworkInformation.Ping();
var reply = await ping.SendPingAsync("192.168.1.100", 1000);

if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
{
    Console.WriteLine($"Ping successful: {reply.RoundtripTime}ms");
}
else
{
    Console.WriteLine($"Ping failed: {reply.Status}");
}

// Test port connectivity
using var tcpClient = new System.Net.Sockets.TcpClient();
try
{
    await tcpClient.ConnectAsync("192.168.1.100", 502);
    Console.WriteLine("Port 502 is accessible");
}
catch (Exception ex)
{
    Console.WriteLine($"Port 502 not accessible: {ex.Message}");
}
```

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/ChrisPulman/ModbusRx.git
   cd ModbusRx
   ```

2. **Install .NET SDK**
   - .NET 8.0 or later
   - .NET Framework 4.8 (for full framework support)

3. **Restore packages and build**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

### Project Structure

```
ModbusRx/
└── src/
    ├── ModbusRx/                  # Core library
    ├── ModbusRx.UnitTests/        # Unit tests
    ├── ModbusRx.IntegrationTests/ # Integration tests
    └── ModbusRx.Server.UI/        # WPF visualization app
```

### Building for Different Targets

```bash
# Build for all target frameworks
dotnet build

# Build for specific framework
dotnet build -f net9.0
dotnet build -f netstandard2.0
dotnet build -f net48
```

## License

ModbusRx is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Support and Community

- 📖 **[Documentation](https://github.com/ChrisPulman/ModbusRx/wiki)** - Comprehensive guides and API reference
- 🐛 **[Issue Tracker](https://github.com/ChrisPulman/ModbusRx/issues)** - Report bugs or request features
- 💬 **[Discussions](https://github.com/ChrisPulman/ModbusRx/discussions)** - Community support and questions
- 📧 **Email**: For commercial support inquiries

### Getting Help

1. **Check the documentation** - Most common scenarios are covered
2. **Search existing issues** - Someone may have had the same problem
3. **Create a minimal reproduction** - When reporting issues
4. **Provide environment details** - OS, .NET version, device information

## Acknowledgments

- **Based on NModbus4** - Solid foundation for Modbus protocol implementation
- **Built with [Reactive Extensions](https://github.com/dotnet/reactive)** - Powerful reactive programming support
- **Inspired by industrial automation needs** - Real-world requirements drive development

---

**ModbusRx** - Making industrial communication reactive and modern! 🏭⚡
