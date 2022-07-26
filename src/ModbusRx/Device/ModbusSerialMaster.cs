﻿// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.IO;
using ModbusRx.Message;

namespace ModbusRx.Device;

/// <summary>
///     Modbus serial master device.
/// </summary>
public sealed class ModbusSerialMaster : ModbusMaster, IModbusSerialMaster
{
    private ModbusSerialMaster(ModbusTransport transport)
        : base(transport)
    {
    }

    /// <summary>
    ///     Gets the Modbus Transport.
    /// </summary>
    ModbusSerialTransport? IModbusSerialMaster.Transport =>
        (ModbusSerialTransport?)Transport;

    /// <summary>
    /// Modbus ASCII master factory method.
    /// </summary>
    /// <param name="serialPort">The serial port.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">serialPort.</exception>
    public static ModbusSerialMaster CreateAscii(SerialPortRx serialPort)
    {
        if (serialPort == null)
        {
            throw new ArgumentNullException(nameof(serialPort));
        }

        return CreateAscii(new SerialPortAdapter(serialPort));
    }

    /// <summary>
    /// Modbus ASCII master factory method.
    /// </summary>
    /// <param name="tcpClient">The TCP client.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">tcpClient.</exception>
    public static ModbusSerialMaster CreateAscii(TcpClientRx tcpClient)
    {
        if (tcpClient == null)
        {
            throw new ArgumentNullException(nameof(tcpClient));
        }

        return CreateAscii(new TcpClientAdapter(tcpClient));
    }

    /// <summary>
    /// Modbus ASCII master factory method.
    /// </summary>
    /// <param name="udpClient">The UDP client.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">udpClient.</exception>
    public static ModbusSerialMaster CreateAscii(UdpClientRx udpClient)
    {
        if (udpClient == null)
        {
            throw new ArgumentNullException(nameof(udpClient));
        }

        if (!udpClient.Client.Connected)
        {
            throw new InvalidOperationException(Resources.UdpClientNotConnected);
        }

        return CreateAscii(new UdpClientAdapter(udpClient));
    }

    /// <summary>
    /// Modbus ASCII master factory method.
    /// </summary>
    /// <param name="streamResource">The stream resource.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">streamResource.</exception>
    public static ModbusSerialMaster CreateAscii(IStreamResource streamResource)
    {
        if (streamResource == null)
        {
            throw new ArgumentNullException(nameof(streamResource));
        }

        return new ModbusSerialMaster(new ModbusAsciiTransport(streamResource));
    }

    /// <summary>
    /// Modbus RTU master factory method.
    /// </summary>
    /// <param name="serialPort">The serial port.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">serialPort.</exception>
    public static ModbusSerialMaster CreateRtu(SerialPortRx serialPort)
    {
        if (serialPort == null)
        {
            throw new ArgumentNullException(nameof(serialPort));
        }

        return CreateRtu(new SerialPortAdapter(serialPort));
    }

    /// <summary>
    /// Modbus RTU master factory method.
    /// </summary>
    /// <param name="tcpClient">The TCP client.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">tcpClient.</exception>
    public static ModbusSerialMaster CreateRtu(TcpClientRx tcpClient)
    {
        if (tcpClient == null)
        {
            throw new ArgumentNullException(nameof(tcpClient));
        }

        return CreateRtu(new TcpClientAdapter(tcpClient));
    }

    /// <summary>
    /// Modbus RTU master factory method.
    /// </summary>
    /// <param name="udpClient">The UDP client.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">udpClient.</exception>
    public static ModbusSerialMaster CreateRtu(UdpClientRx udpClient)
    {
        if (udpClient == null)
        {
            throw new ArgumentNullException(nameof(udpClient));
        }

        if (!udpClient.Client.Connected)
        {
            throw new InvalidOperationException(Resources.UdpClientNotConnected);
        }

        return CreateRtu(new UdpClientAdapter(udpClient));
    }

    /// <summary>
    /// Modbus RTU master factory method.
    /// </summary>
    /// <param name="streamResource">The stream resource.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">streamResource.</exception>
    public static ModbusSerialMaster CreateRtu(IStreamResource streamResource)
    {
        if (streamResource == null)
        {
            throw new ArgumentNullException(nameof(streamResource));
        }

        return new ModbusSerialMaster(new ModbusRtuTransport(streamResource));
    }

    /// <summary>
    ///     Serial Line only.
    ///     Diagnostic function which loops back the original data.
    ///     NModbus only supports looping back one ushort value, this is a limitation of the "Best Effort" implementation of
    ///     the RTU protocol.
    /// </summary>
    /// <param name="slaveAddress">Address of device to test.</param>
    /// <param name="data">Data to return.</param>
    /// <returns>Return true if slave device echoed data.</returns>
    public bool ReturnQueryData(byte slaveAddress, ushort data)
    {
        var request = new DiagnosticsRequestResponse(
            Modbus.DiagnosticsReturnQueryData,
            slaveAddress,
            new RegisterCollection(data));

        var response = Transport?.UnicastMessage<DiagnosticsRequestResponse>(request);

        return response!.Data[0] == data;
    }
}
