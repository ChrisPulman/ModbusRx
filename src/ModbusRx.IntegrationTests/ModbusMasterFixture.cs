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
public abstract class ModbusMasterFixture : IDisposable
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
    public static double AverageReadTime => 150;

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
    /// Gets or sets the slave thread.
    /// </summary>
    /// <value>
    /// The slave thread.
    /// </value>
    private Thread? SlaveThread { get; set; }

    /// <summary>
    /// Gets or sets the jamod.
    /// </summary>
    /// <value>
    /// The jamod.
    /// </value>
    private Process? Jamod { get; set; }

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
        SlaveSerialPort = new SerialPortRx(DefaultSlaveSerialPortName)
        {
            Parity = Parity.None,
        };
        SlaveSerialPort.Open();
    }

    /// <summary>
    /// Starts the slave.
    /// </summary>
    public void StartSlave()
    {
        SlaveThread = new Thread(async () => await Slave!.ListenAsync())
        {
            IsBackground = true,
        };
        SlaveThread.Start();
    }

    /// <summary>
    /// Starts the jamod slave.
    /// </summary>
    /// <param name="program">The program.</param>
    public void StartJamodSlave(string program)
    {
        var pathToJamod = Path.Combine(
            Path.GetDirectoryName(Assembly.GetAssembly(typeof(ModbusMasterFixture))!.Location)!, "../../../../tools/jamod");
        var classpath = string.Format(@"-classpath ""{0};{1};{2}""", Path.Combine(pathToJamod, "jamod.jar"), Path.Combine(pathToJamod, "comm.jar"), Path.Combine(pathToJamod, "."));
        var startInfo = new ProcessStartInfo("java", string.Format(CultureInfo.InvariantCulture, "{0} {1}", classpath, program));
        Jamod = Process.Start(startInfo);

        Thread.Sleep(4000);
        Assert.False(Jamod?.HasExited, "Jamod Serial Ascii Slave did not start correctly.");
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Reads the coils.
    /// </summary>
    [Fact]
    public virtual void ReadCoils()
    {
        var coils = Master!.ReadCoils(SlaveAddress, 2048, 8);
        Assert.Equal(new bool[] { false, false, false, false, false, false, false, false }, coils);
    }

    /// <summary>
    /// Reads the inputs.
    /// </summary>
    [Fact]
    public virtual void ReadInputs()
    {
        var inputs = Master!.ReadInputs(SlaveAddress, 150, 3);
        Assert.Equal(new bool[] { false, false, false }, inputs);
    }

    /// <summary>
    /// Reads the holding registers.
    /// </summary>
    [Fact]
    public virtual void ReadHoldingRegisters()
    {
        var registers = Master!.ReadHoldingRegisters(SlaveAddress, 104, 2);
        Assert.Equal(new ushort[] { 0, 0 }, registers);
    }

    /// <summary>
    /// Reads the input registers.
    /// </summary>
    [Fact]
    public virtual void ReadInputRegisters()
    {
        var registers = Master!.ReadInputRegisters(SlaveAddress, 104, 2);
        Assert.Equal(new ushort[] { 0, 0 }, registers);
    }

    /// <summary>
    /// Writes the single coil.
    /// </summary>
    [Fact]
    public virtual void WriteSingleCoil()
    {
        var coilValue = Master!.ReadCoils(SlaveAddress, 10, 1)[0];
        Master.WriteSingleCoil(SlaveAddress, 10, !coilValue);
        Assert.Equal(!coilValue, Master.ReadCoils(SlaveAddress, 10, 1)[0]);
        Master.WriteSingleCoil(SlaveAddress, 10, coilValue);
        Assert.Equal(coilValue, Master.ReadCoils(SlaveAddress, 10, 1)[0]);
    }

    /// <summary>
    /// Writes the single register.
    /// </summary>
    [Fact]
    public virtual void WriteSingleRegister()
    {
        const ushort testAddress = 200;
        const ushort testValue = 350;

        var originalValue = Master!.ReadHoldingRegisters(SlaveAddress, testAddress, 1)[0];
        Master.WriteSingleRegister(SlaveAddress, testAddress, testValue);
        Assert.Equal(testValue, Master.ReadHoldingRegisters(SlaveAddress, testAddress, 1)[0]);
        Master.WriteSingleRegister(SlaveAddress, testAddress, originalValue);
        Assert.Equal(originalValue, Master.ReadHoldingRegisters(SlaveAddress, testAddress, 1)[0]);
    }

    /// <summary>
    /// Writes the multiple registers.
    /// </summary>
    [Fact]
    public virtual void WriteMultipleRegisters()
    {
        const ushort testAddress = 120;
        var testValues = new ushort[] { 10, 20, 30, 40, 50 };

        var originalValues = Master!.ReadHoldingRegisters(SlaveAddress, testAddress, (ushort)testValues.Length);
        Master.WriteMultipleRegisters(SlaveAddress, testAddress, testValues);
        var newValues = Master.ReadHoldingRegisters(SlaveAddress, testAddress, (ushort)testValues.Length);
        Assert.Equal(testValues, newValues);
        Master.WriteMultipleRegisters(SlaveAddress, testAddress, originalValues);
    }

    /// <summary>
    /// Writes the multiple coils.
    /// </summary>
    [Fact]
    public virtual void WriteMultipleCoils()
    {
        const ushort testAddress = 200;
        var testValues = new bool[] { true, false, true, false, false, false, true, false, true, false };

        var originalValues = Master!.ReadCoils(SlaveAddress, testAddress, (ushort)testValues.Length);
        Master.WriteMultipleCoils(SlaveAddress, testAddress, testValues);
        var newValues = Master.ReadCoils(SlaveAddress, testAddress, (ushort)testValues.Length);
        Assert.Equal(testValues, newValues);
        Master.WriteMultipleCoils(SlaveAddress, testAddress, originalValues);
    }

    /// <summary>
    /// Reads the maximum number of holding registers.
    /// </summary>
    [Fact]
    public virtual void ReadMaximumNumberOfHoldingRegisters()
    {
        var registers = Master!.ReadHoldingRegisters(SlaveAddress, 104, 125);
        Assert.Equal(125, registers.Length);
    }

    /// <summary>
    /// Reads the write multiple registers.
    /// </summary>
    [Fact]
    public virtual void ReadWriteMultipleRegisters()
    {
        const ushort startReadAddress = 120;
        const ushort numberOfPointsToRead = 5;
        const ushort startWriteAddress = 50;
        var valuesToWrite = new ushort[] { 10, 20, 30, 40, 50 };

        var valuesToRead = Master!.ReadHoldingRegisters(SlaveAddress, startReadAddress, numberOfPointsToRead);
        var readValues = Master.ReadWriteMultipleRegisters(SlaveAddress, startReadAddress, numberOfPointsToRead, startWriteAddress, valuesToWrite);
        Assert.Equal(valuesToRead, readValues);

        var writtenValues = Master.ReadHoldingRegisters(SlaveAddress, startWriteAddress, (ushort)valuesToWrite.Length);
        Assert.Equal(valuesToWrite, writtenValues);
    }

    /// <summary>
    /// Simples the read registers performance test.
    /// </summary>
    [Fact]
    public virtual void SimpleReadRegistersPerformanceTest()
    {
        var retries = Master!.Transport!.Retries;
        Master.Transport!.Retries = 5;
        var actualAverageReadTime = CalculateAverage(Master);
        Master.Transport.Retries = retries;
        Assert.True(actualAverageReadTime < ModbusMasterFixture.AverageReadTime, string.Format(CultureInfo.InvariantCulture, "Test failed, actual average read time {0} is greater than expected {1}", actualAverageReadTime, ModbusMasterFixture.AverageReadTime));
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
    internal static double CalculateAverage(IModbusMaster master)
    {
        const ushort startAddress = 5;
        const ushort numRegisters = 5;

        // JIT compile the IL
        master.ReadHoldingRegisters(SlaveAddress, startAddress, numRegisters);

        var stopwatch = new Stopwatch();
        long sum = 0;
        const double numberOfReads = 50;

        for (var i = 0; i < numberOfReads; i++)
        {
            stopwatch.Reset();
            stopwatch.Start();
            master.ReadHoldingRegisters(SlaveAddress, startAddress, numRegisters);
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
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Master?.Dispose();

                Slave?.Dispose();

                if (Jamod is not null)
                {
                    Jamod.Kill();
                    Thread.Sleep(4000);
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }
}
