// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.IO.Ports;
using System.Reactive.Linq;
using CP.IO.Ports;

namespace ModbusRx.DriverTest;

/// <summary>
/// SerialDeviceEmulator.
/// </summary>
public class SerialDeviceEmulator : IDisposable
{
    private readonly SerialPortRx _port;
    private readonly DummyTemperatureController _controller;
    private readonly ModbusRtuHandler _handler;
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerialDeviceEmulator"/> class.
    /// </summary>
    /// <param name="portName">Name of the port.</param>
    /// <param name="slaveId">The slave identifier.</param>
    public SerialDeviceEmulator(string portName, byte slaveId = 1)
    {
        _controller = new DummyTemperatureController();
        _handler = new ModbusRtuHandler(_controller, slaveId);

        _port = new SerialPortRx(portName, 9600, 8, Parity.None, StopBits.One);
        _port.Open();
        _port.IsOpenObservable.Where(x => x).Subscribe(_ =>
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Serial port {portName} opened.");
            _controller.Update(); // Update once to set initial data
            Task.Run(ReceiveLoop);
        });
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
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _port.Dispose();
            }

            _disposedValue = true;
        }
    }

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
                    var crcReceived = (ushort)(frame[frame.Length - 2] | (frame[frame.Length - 1] << 8));
                    var crcCalc = ModbusCrc.Compute(frame, frame.Length - 2);
                    if (crcReceived == crcCalc)
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Emulator RX: {BitConverter.ToString(frame)}");
                        var response = _handler.HandleRequest(frame, frame.Length);
                        if (response != null)
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
