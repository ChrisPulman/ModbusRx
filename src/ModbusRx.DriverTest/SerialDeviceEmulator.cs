// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO.Ports;
using CP.IO.Ports;

namespace ModbusRx.DriverTest;

/// <summary>Tests the SerialDeviceEmulator behavior.</summary>
public class SerialDeviceEmulator : IDisposable
{
    /// <summary>The emulated serial port.</summary>
    private readonly SerialPortRx _port;

    /// <summary>The simulated temperature controller.</summary>
    private readonly DummyTemperatureController _controller;

    /// <summary>The Modbus request handler.</summary>
    private readonly ModbusRtuHandler _handler;

    /// <summary>A value indicating whether the emulator has been disposed.</summary>
    private bool _disposedValue;

    /// <summary>Initializes a new instance of the <see cref="SerialDeviceEmulator"/> class.</summary>
    /// <param name="portName">Name of the port.</param>
    /// <param name="slaveId">The slave identifier.</param>
    public SerialDeviceEmulator(string portName, byte slaveId = 1)
    {
        _controller = new();
        _handler = new(_controller, slaveId);

        _port = new(portName, 9600, 8, Parity.None, StopBits.One);
        _ = _port.Open();
        _ = _port.IsOpenObservable.Where(x => x).Subscribe(isOpen =>
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Serial port {portName} opened.");
            _controller.Update(); // Update once to set initial data
            _ = Task.Run(ReceiveLoop);
        });
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
            _port.Dispose();
        }

        _disposedValue = true;
    }

    /// <summary>Receives and handles Modbus RTU frames from the serial port.</summary>
    /// <returns>A task that completes when the receive loop exits.</returns>
    private async Task ReceiveLoop()
    {
        var buffer = new List<byte>();
        try
        {
            while (true)
            {
                var b = (byte)_port.ReadByte();
                buffer.Add(b);
                if (buffer.Count >= 8)
                {
                    var frame = buffer.ToArray();
                    var crcReceived = (ushort)(frame[^2] | (frame[^1] << 8));
                    var crcCalc = ModbusCrc.Compute(frame, frame.Length - 2);
                    if (crcReceived == crcCalc)
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Emulator RX: {BitConverter.ToString(frame)}");
                        var response = _handler.HandleRequest(frame, frame.Length);
                        if (response is not null)
                        {
                            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Emulator TX: {BitConverter.ToString(response)}");
                            _port.Write(response, 0, response.Length);
                        }

                        buffer.Clear();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Port disposed, exit gracefully
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReceiveLoop error: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}
