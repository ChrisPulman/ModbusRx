// <copyright file="ModbusSerialSlave.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics;
using System.IO.Ports;
using CP.IO.Ports;
using ModbusRx.IO;
using ModbusRx.Message;

namespace ModbusRx.Device;

/// <summary>
///     Modbus serial slave device.
/// </summary>
public sealed class ModbusSerialSlave : ModbusSlave
{
    private ModbusSerialSlave(byte unitId, ModbusTransport transport)
        : base(unitId, transport)
    {
    }

    private ModbusSerialTransport? SerialTransport
    {
        get
        {
            if (Transport is not ModbusSerialTransport transport)
            {
                throw new ObjectDisposedException("SerialTransport");
            }

            return transport;
        }
    }

    /// <summary>
    /// Modbus ASCII slave factory method.
    /// </summary>
    /// <param name="unitId">The unit identifier.</param>
    /// <param name="serialPort">The serial port.</param>
    /// <returns>A ModbusSerialSlave.</returns>
    /// <exception cref="System.ArgumentNullException">serialPort.</exception>
    public static ModbusSerialSlave CreateAscii(byte unitId, SerialPortRx serialPort)
    {
        if (serialPort == null)
        {
            throw new ArgumentNullException(nameof(serialPort));
        }

        return CreateAscii(unitId, new SerialPortAdapter(serialPort));
    }

    /// <summary>
    /// Modbus ASCII slave factory method.
    /// </summary>
    /// <param name="unitId">The unit identifier.</param>
    /// <param name="streamResource">The stream resource.</param>
    /// <returns>A ModbusSerialSlave.</returns>
    /// <exception cref="System.ArgumentNullException">streamResource.</exception>
    public static ModbusSerialSlave CreateAscii(byte unitId, IStreamResource streamResource)
    {
        if (streamResource == null)
        {
            throw new ArgumentNullException(nameof(streamResource));
        }

        return new ModbusSerialSlave(unitId, new ModbusAsciiTransport(streamResource));
    }

    /// <summary>
    /// Modbus RTU slave factory method.
    /// </summary>
    /// <param name="unitId">The unit identifier.</param>
    /// <param name="serialPort">The serial port.</param>
    /// <returns>A ModbusSerialSlave.</returns>
    /// <exception cref="System.ArgumentNullException">serialPort.</exception>
    public static ModbusSerialSlave CreateRtu(byte unitId, SerialPortRx serialPort)
    {
        if (serialPort == null)
        {
            throw new ArgumentNullException(nameof(serialPort));
        }

        return CreateRtu(unitId, new SerialPortAdapter(serialPort));
    }

    /// <summary>
    /// Modbus RTU slave factory method.
    /// </summary>
    /// <param name="unitId">The unit identifier.</param>
    /// <param name="streamResource">The stream resource.</param>
    /// <returns>A ModbusSerialSlave.</returns>
    /// <exception cref="System.ArgumentNullException">streamResource.</exception>
    public static ModbusSerialSlave CreateRtu(byte unitId, IStreamResource streamResource)
    {
        if (streamResource == null)
        {
            throw new ArgumentNullException(nameof(streamResource));
        }

        return new ModbusSerialSlave(unitId, new ModbusRtuTransport(streamResource));
    }

    /// <summary>
    /// Start slave listening for requests.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ListenAsync()
    {
        while (true)
        {
            try
            {
                try
                {
                    // TODO: remove delay once async will be implemented in transport level
                    await Task.Delay(20).ConfigureAwait(false);

                    // read request and build message
                    var frame = await SerialTransport?.ReadRequest()!;
                    var request = ModbusMessageFactory.CreateModbusRequest(frame!);

                    if (SerialTransport!.CheckFrame && !SerialTransport.ChecksumsMatch(request, frame!))
                    {
                        var msg = $"Checksums failed to match {string.Join(", ", request.MessageFrame)} != {string.Join(", ", frame!)}.";
                        Debug.WriteLine(msg);
                        throw new IOException(msg);
                    }

                    // only service requests addressed to this particular slave
                    if (request.SlaveAddress != UnitId)
                    {
                        Debug.WriteLine($"NModbus Slave {UnitId} ignoring request intended for NModbus Slave {request.SlaveAddress}");
                        continue;
                    }

                    // perform action
                    var response = ApplyRequest(request);

                    // write response
                    SerialTransport.Write(response);
                }
                catch (IOException ioe)
                {
                    Debug.WriteLine($"IO Exception encountered while listening for requests - {ioe.Message}");
                    SerialTransport?.DiscardInBuffer();
                }
                catch (TimeoutException te)
                {
                    Debug.WriteLine($"Timeout Exception encountered while listening for requests - {te.Message}");
                    SerialTransport?.DiscardInBuffer();
                }

                // TODO better exception handling here, missing FormatException, NotImplemented...
            }
            catch (InvalidOperationException)
            {
                // when the underlying transport is disposed
                break;
            }
        }
    }
}
