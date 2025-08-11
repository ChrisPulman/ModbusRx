// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.IntegrationTests.CustomMessages;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// ModbusMasterFixture.
/// </summary>
/// <seealso cref="System.IDisposable" />
[Collection("NetworkTests")]
public abstract class ModbusRxMasterFixture : NetworkTestBase
{
    /// <summary>
    /// The port.
    /// </summary>
    public const int Port = 502;
    /// <summary>
    /// The slave address.
    /// </summary>
    public const byte SlaveAddress = 1;
    /// <summary>
    /// The default master serial port name.
    /// </summary>
    public const string DefaultMasterSerialPortName = "COM1";
    /// <summary>
    /// The default slave serial port name.
    /// </summary>
    public const string DefaultSlaveSerialPortName = "COM2";
    private bool _disposedValue;

    /// <summary>
    /// Gets the TCP host.
    /// </summary>
    /// <value>
    /// The TCP host.
    /// </value>
    public static IPAddress TcpHost { get; } = new IPAddress(new byte[] { 127, 0, 0, 1 });

    /// <summary>
    /// Gets the default modbus ip end point.
    /// </summary>
    /// <value>
    /// The default modbus ip end point.
    /// </value>
    public static IPEndPoint DefaultModbusIPEndPoint { get; } = new IPEndPoint(TcpHost, Port);

    /// <summary>
    /// Gets the average read time.
    /// </summary>
    /// <value>
    /// The average read time.
    /// </value>
    public static double AverageReadTime => IsRunningInCI ? 300 : 150; // More relaxed in CI

    /// <summary>
    /// Gets or sets the master.
    /// </summary>
    /// <value>
    /// The master.
    /// </value>
    protected ModbusMaster? Master { get; set; }

    /// <summary>
    /// Gets or sets the master serial port.
    /// </summary>
    /// <value>
    /// The master serial port.
    /// </value>
    protected SerialPortRx? MasterSerialPort { get; set; }

    /// <summary>
    /// Gets or sets the master TCP.
    /// </summary>
    /// <value>
    /// The master TCP.
    /// </value>
    protected TcpClientRx? MasterTcp { get; set; }

    /// <summary>
    /// Gets or sets the master UDP.
    /// </summary>
    /// <value>
    /// The master UDP.
    /// </value>
    protected UdpClientRx? MasterUdp { get; set; }

    /// <summary>
    /// Gets or sets the slave.
    /// </summary>
    /// <value>
    /// The slave.
    /// </value>
    protected ModbusSlave? Slave { get; set; }

    /// <summary>
    /// Gets or sets the slave serial port.
    /// </summary>
    /// <value>
    /// The slave serial port.
    /// </value>
    protected SerialPortRx? SlaveSerialPort { get; set; }

    /// <summary>
    /// Gets or sets the slave TCP.
    /// </summary>
    /// <value>
    /// The slave TCP.
    /// </value>
    protected TcpListener? SlaveTcp { get; set; }

    /// <summary>
    /// Gets or sets the slave UDP.
    /// </summary>
    /// <value>
    /// The slave UDP.
    /// </value>
    protected UdpClientRx? SlaveUdp { get; set; }

    /// <summary>
    /// Gets or sets the slave task.
    /// </summary>
    /// <value>
    /// The slave task.
    /// </value>
    private Task? SlaveTask { get; set; }

    /// <summary>
    /// Gets or sets the slave cancellation token source.
    /// </summary>
    /// <value>
    /// The slave cancellation token source.
    /// </value>
    private CancellationTokenSource? SlaveCancellationTokenSource { get; set; }

    /// <summary>
    /// Gets or sets the jamod.
    /// </summary>
    /// <value>
    /// The jamod.
    /// </value>
    private Process? Jamod { get; set; }

    /// <summary>
    /// Gets a value indicating whether the tests are running in CI environment.
    /// </summary>
    private static bool IsRunningInCI =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

    /// <summary>
    /// Creates the and open serial port.
    /// </summary>
    /// <param name="portName">Name of the port.</param>
    /// <returns>A SerialPort.</returns>
    public static SerialPortRx CreateAndOpenSerialPort(string portName)
    {
        var port = new SerialPortRx(portName)
        {
            Parity = Parity.None
        };
        port.Open();

        return port;
    }

    /// <summary>
    /// Setups the slave serial port.
    /// </summary>
    public void SetupSlaveSerialPort()
    {
        SkipIfRunningInCI("Serial port tests require physical hardware not available in CI");

        SlaveSerialPort = new SerialPortRx(DefaultSlaveSerialPortName)
        {
            Parity = Parity.None,
        };
        SlaveSerialPort.Open();
        RegisterDisposable(SlaveSerialPort);
    }

    /// <summary>
    /// Starts the slave.
    /// </summary>
    public void StartSlave()
    {
        if (Slave == null)
        {
            return;
        }

        RegisterDisposable(Slave);
        
        // Create cancellation token source for the slave
        SlaveCancellationTokenSource = new CancellationTokenSource();
        RegisterDisposable(SlaveCancellationTokenSource);

        SlaveTask = Task.Run(
            async () =>
            {
                try
                {
                    await Slave.ListenAsync();
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                }
                catch (ObjectDisposedException)
                {
                    // Expected when resources are disposed
                }
                catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 995)
                {
                    // Expected when I/O operation is aborted due to thread exit or application request
                    // This is normal during test cleanup in CI environments
                }
                catch (System.Net.Sockets.SocketException)
                {
                    // Other socket exceptions during cleanup are also expected
                }
            },
            SlaveCancellationTokenSource.Token);
    }

    /// <summary>
    /// Starts the jamod slave.
    /// </summary>
    /// <param name="program">The program.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartJamodSlaveAsync(string program)
    {
        SkipIfRunningInCI("Jamod external dependency not available in CI environment");

        var pathToJamod = Path.Combine(
            Path.GetDirectoryName(Assembly.GetAssembly(typeof(ModbusRxMasterFixture))!.Location)!, "../../../../tools/jamod");
        var classpath = string.Format(@"-classpath ""{0};{1};{2}""", Path.Combine(pathToJamod, "jamod.jar"), Path.Combine(pathToJamod, "comm.jar"), Path.Combine(pathToJamod, "."));
        var startInfo = new ProcessStartInfo("java", string.Format(CultureInfo.InvariantCulture, "{0} {1}", classpath, program));
        Jamod = Process.Start(startInfo);

        var timeout = GetEnvironmentAppropriateTimeout(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2));
        await Task.Delay(timeout, CancellationToken);
        Assert.False(Jamod?.HasExited, "Jamod Serial Ascii Slave did not start correctly.");
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public new void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Reads the coils.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task ReadCoils()
    {
        var coils = await Master!.ReadCoilsAsync(SlaveAddress, 2048, 8);
        Assert.Equal(new bool[] { false, false, false, false, false, false, false, false }, coils);
    }

    /// <summary>
    /// Reads the inputs.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task ReadInputs()
    {
        var inputs = await Master!.ReadInputsAsync(SlaveAddress, 150, 3);
        Assert.Equal(new bool[] { false, false, false }, inputs);
    }

    /// <summary>
    /// Reads the holding registers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task ReadHoldingRegisters()
    {
        var registers = await Master!.ReadHoldingRegistersAsync(SlaveAddress, 104, 2);
        Assert.Equal(new ushort[] { 0, 0 }, registers);
    }

    /// <summary>
    /// Reads the input registers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task ReadInputRegisters()
    {
        var registers = await Master!.ReadInputRegistersAsync(SlaveAddress, 104, 2);
        Assert.Equal(new ushort[] { 0, 0 }, registers);
    }

    /// <summary>
    /// Writes the single coil.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task WriteSingleCoil()
    {
        var coilValue = await Master!.ReadCoilsAsync(SlaveAddress, 10, 1);
        await Master.WriteSingleCoilAsync(SlaveAddress, 10, !coilValue[0]);
        Assert.Equal(!coilValue[0], (await Master.ReadCoilsAsync(SlaveAddress, 10, 1))[0]);
        await Master.WriteSingleCoilAsync(SlaveAddress, 10, coilValue[0]);
        Assert.Equal(coilValue[0], (await Master.ReadCoilsAsync(SlaveAddress, 10, 1))[0]);
    }

    /// <summary>
    /// Writes the single register.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task WriteSingleRegister()
    {
        const ushort testAddress = 200;
        const ushort testValue = 350;

        var originalValue = await Master!.ReadHoldingRegistersAsync(SlaveAddress, testAddress, 1);
        await Master.WriteSingleRegisterAsync(SlaveAddress, testAddress, testValue);
        Assert.Equal(testValue, (await Master.ReadHoldingRegistersAsync(SlaveAddress, testAddress, 1))[0]);
        await Master.WriteSingleRegisterAsync(SlaveAddress, testAddress, originalValue[0]);
        Assert.Equal(originalValue[0], (await Master.ReadHoldingRegistersAsync(SlaveAddress, testAddress, 1))[0]);
    }

    /// <summary>
    /// Writes the multiple registers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task WriteMultipleRegisters()
    {
        const ushort testAddress = 120;
        var testValues = new ushort[] { 10, 20, 30, 40, 50 };

        var originalValues = await Master!.ReadHoldingRegistersAsync(SlaveAddress, testAddress, (ushort)testValues.Length);
        await Master.WriteMultipleRegistersAsync(SlaveAddress, testAddress, testValues);
        var newValues = await Master.ReadHoldingRegistersAsync(SlaveAddress, testAddress, (ushort)testValues.Length);
        Assert.Equal(testValues, newValues);
        await Master.WriteMultipleRegistersAsync(SlaveAddress, testAddress, originalValues);
    }

    /// <summary>
    /// Writes the multiple coils.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task WriteMultipleCoils()
    {
        const ushort testAddress = 200;
        var testValues = new bool[] { true, false, true, false, false, false, true, false, true, false };

        var originalValues = await Master!.ReadCoilsAsync(SlaveAddress, testAddress, (ushort)testValues.Length);
        await Master.WriteMultipleCoilsAsync(SlaveAddress, testAddress, testValues);
        var newValues = await Master.ReadCoilsAsync(SlaveAddress, testAddress, (ushort)testValues.Length);
        Assert.Equal(testValues, newValues);
        await Master.WriteMultipleCoilsAsync(SlaveAddress, testAddress, originalValues);
    }

    /// <summary>
    /// Reads the maximum number of holding registers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task ReadMaximumNumberOfHoldingRegisters()
    {
        var registers = await Master!.ReadHoldingRegistersAsync(SlaveAddress, 104, 125);
        Assert.Equal(125, registers.Length);
    }

    /// <summary>
    /// Reads the write multiple registers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task ReadWriteMultipleRegisters()
    {
        const ushort startReadAddress = 120;
        const ushort numberOfPointsToRead = 5;
        const ushort startWriteAddress = 50;
        var valuesToWrite = new ushort[] { 10, 20, 30, 40, 50 };

        var valuesToRead = await Master!.ReadHoldingRegistersAsync(SlaveAddress, startReadAddress, numberOfPointsToRead);
        var readValues = await Master.ReadWriteMultipleRegistersAsync(SlaveAddress, startReadAddress, numberOfPointsToRead, startWriteAddress, valuesToWrite);
        Assert.Equal(valuesToRead, readValues);

        var writtenValues = await Master.ReadHoldingRegistersAsync(SlaveAddress, startWriteAddress, (ushort)valuesToWrite.Length);
        Assert.Equal(valuesToWrite, writtenValues);
    }

    /// <summary>
    /// Simples the read registers performance test.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public virtual async Task SimpleReadRegistersPerformanceTest()
    {
        var retries = Master!.Transport!.Retries;
        Master.Transport!.Retries = 5;
        var actualAverageReadTime = await CalculateAverageAsync(Master);
        Master.Transport.Retries = retries;
        Assert.True(
            actualAverageReadTime < AverageReadTime,
            string.Format(
                CultureInfo.InvariantCulture,
                "Test failed, actual average read time {0} is greater than expected {1}",
                actualAverageReadTime,
                AverageReadTime));
    }

    /// <summary>
    /// Executes the custom message read holding registers.
    /// </summary>
    [Fact]
    public virtual void ExecuteCustomMessage_ReadHoldingRegisters()
    {
        var request = new CustomReadHoldingRegistersRequest(3, SlaveAddress, 104, 2);
        var response = Master!.ExecuteCustomMessage<CustomReadHoldingRegistersResponse>(request);
        Assert.Equal(new ushort[] { 0, 0 }, response.Data);
    }

    /// <summary>
    /// Executes the custom message write multiple registers.
    /// </summary>
    [Fact]
    public virtual void ExecuteCustomMessage_WriteMultipleRegisters()
    {
        const ushort testAddress = 120;
        var testValues = new ushort[] { 10, 20, 30, 40, 50 };
        var readRequest = new CustomReadHoldingRegistersRequest(3, SlaveAddress, testAddress, (ushort)testValues.Length);
        var writeRequest = new CustomWriteMultipleRegistersRequest(16, SlaveAddress, testAddress, new RegisterCollection(testValues));

        var response = Master!.ExecuteCustomMessage<CustomReadHoldingRegistersResponse>(readRequest);
        var originalValues = response.Data;
        Master.ExecuteCustomMessage<CustomWriteMultipleRegistersResponse>(writeRequest);
        response = Master.ExecuteCustomMessage<CustomReadHoldingRegistersResponse>(readRequest);
        var newValues = response.Data;
        Assert.Equal(testValues, newValues);
        writeRequest = new CustomWriteMultipleRegistersRequest(16, SlaveAddress, testAddress, new RegisterCollection(originalValues));
        Master.ExecuteCustomMessage<CustomWriteMultipleRegistersResponse>(writeRequest);
    }

    /// <summary>
    /// Calculates the average.
    /// </summary>
    /// <param name="master">The master.</param>
    /// <returns>A double.</returns>
    internal static async Task<double> CalculateAverageAsync(IModbusMaster master)
    {
        const ushort startAddress = 5;
        const ushort numRegisters = 5;

        // JIT compile the IL
        await master.ReadHoldingRegistersAsync(SlaveAddress, startAddress, numRegisters);

        var stopwatch = new Stopwatch();
        long sum = 0;
        var numberOfReads = IsRunningInCI ? 25.0 : 50.0; // Reduce iterations in CI

        for (var i = 0; i < numberOfReads; i++)
        {
            stopwatch.Reset();
            stopwatch.Start();
            await master.ReadHoldingRegistersAsync(SlaveAddress, startAddress, numRegisters);
            stopwatch.Stop();

            checked
            {
                sum += stopwatch.ElapsedMilliseconds;
            }
        }

        return sum / numberOfReads;
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Cancel slave operations first
                try
                {
                    SlaveCancellationTokenSource?.Cancel();
                }
                catch
                {
                    // Ignore cancellation exceptions
                }

                // Stop TCP listener before disposing slave
                if (SlaveTcp != null)
                {
                    try
                    {
                        SlaveTcp.Stop();
                    }
                    catch
                    {
                        // Ignore cleanup exceptions
                    }
                }

                // Dispose slave and master
                try
                {
                    Slave?.Dispose();
                }
                catch
                {
                    // Ignore disposal exceptions
                }

                try
                {
                    Master?.Dispose();
                }
                catch
                {
                    // Ignore disposal exceptions
                }

                // Wait for slave task to complete with timeout
                if (SlaveTask != null && !SlaveTask.IsCompleted)
                {
                    try
                    {
                        var timeout = GetEnvironmentAppropriateTimeout(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
                        SlaveTask.Wait(timeout);
                    }
                    catch
                    {
                        // Ignore task wait exceptions
                    }
                }

                // Handle Jamod process
                if (Jamod is not null)
                {
                    try
                    {
                        Jamod.Kill();

                        // Use synchronous wait in disposal
                        Thread.Sleep(GetEnvironmentAppropriateTimeout(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1)));
                    }
                    catch
                    {
                        // Ignore cleanup exceptions
                    }
                }

                // Register additional resources for cleanup
                if (MasterTcp != null)
                {
                    RegisterDisposable(MasterTcp);
                }

                if (MasterUdp != null)
                {
                    RegisterDisposable(MasterUdp);
                }

                if (MasterSerialPort != null)
                {
                    RegisterDisposable(MasterSerialPort);
                }

                if (SlaveSerialPort != null)
                {
                    RegisterDisposable(SlaveSerialPort);
                }

                if (SlaveUdp != null)
                {
                    RegisterDisposable(SlaveUdp);
                }
            }

            _disposedValue = true;
        }

        // Call base NetworkTestBase disposal
        base.Dispose(disposing);
    }
}
