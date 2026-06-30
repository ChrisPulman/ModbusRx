// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.IO.Ports;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.IO;
#else
using ModbusRx.IO;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Message;
#else
using ModbusRx.Message;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Device;
#else
namespace ModbusRx.Device;
#endif

/// <summary>Modbus serial master device.</summary>
public sealed class ModbusSerialMaster : ModbusMaster, IModbusSerialMaster
{
    /// <summary>Initializes a new instance of the Modbus Serial Master class.</summary>
    /// <param name="transport">The transport value.</param>
    private ModbusSerialMaster(ModbusTransport transport)
        : base(transport)
    {
    }

    /// <summary>Gets the Modbus Transport.</summary>
    ModbusSerialTransport? IModbusSerialMaster.Transport =>
        (ModbusSerialTransport?)Transport;

    /// <summary>Modbus ASCII master factory method.</summary>
    /// <param name="serialPort">The serial port.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">serialPort.</exception>
    public static ModbusSerialMaster CreateAscii(SerialPortRx serialPort)
    {
        if (serialPort is null)
        {
            throw new ArgumentNullException(nameof(serialPort));
        }

        return CreateAscii(new SerialPortAdapter(serialPort));
    }

    /// <summary>Modbus ASCII master factory method.</summary>
    /// <param name="tcpClient">The TCP client.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">tcpClient.</exception>
    public static ModbusSerialMaster CreateAscii(TcpClientRx tcpClient)
    {
        if (tcpClient is null)
        {
            throw new ArgumentNullException(nameof(tcpClient));
        }

        return CreateAscii(new TcpClientAdapter(tcpClient));
    }

    /// <summary>Modbus ASCII master factory method.</summary>
    /// <param name="udpClient">The UDP client.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">udpClient.</exception>
    public static ModbusSerialMaster CreateAscii(UdpClientRx udpClient)
    {
        if (udpClient is null)
        {
            throw new ArgumentNullException(nameof(udpClient));
        }

        if (!udpClient.Client.Connected)
        {
            throw new InvalidOperationException(Resources.UdpClientNotConnected);
        }

        return CreateAscii(new UdpClientAdapter(udpClient));
    }

    /// <summary>Modbus ASCII master factory method.</summary>
    /// <param name="streamResource">The stream resource.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">streamResource.</exception>
    public static ModbusSerialMaster CreateAscii(IStreamResource streamResource)
    {
        if (streamResource is null)
        {
            throw new ArgumentNullException(nameof(streamResource));
        }

        return new ModbusSerialMaster(new ModbusAsciiTransport(streamResource));
    }

    /// <summary>Modbus RTU master factory method.</summary>
    /// <param name="serialPort">The serial port.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">serialPort.</exception>
    public static ModbusSerialMaster CreateRtu(SerialPortRx serialPort)
    {
        if (serialPort is null)
        {
            throw new ArgumentNullException(nameof(serialPort));
        }

        return CreateRtu(new SerialPortAdapter(serialPort));
    }

    /// <summary>Modbus RTU master factory method.</summary>
    /// <param name="tcpClient">The TCP client.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">tcpClient.</exception>
    public static ModbusSerialMaster CreateRtu(TcpClientRx tcpClient)
    {
        if (tcpClient is null)
        {
            throw new ArgumentNullException(nameof(tcpClient));
        }

        return CreateRtu(new TcpClientAdapter(tcpClient));
    }

    /// <summary>Modbus RTU master factory method.</summary>
    /// <param name="udpClient">The UDP client.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">udpClient.</exception>
    public static ModbusSerialMaster CreateRtu(UdpClientRx udpClient)
    {
        if (udpClient is null)
        {
            throw new ArgumentNullException(nameof(udpClient));
        }

        if (!udpClient.Client.Connected)
        {
            throw new InvalidOperationException(Resources.UdpClientNotConnected);
        }

        return CreateRtu(new UdpClientAdapter(udpClient));
    }

    /// <summary>Modbus RTU master factory method.</summary>
    /// <param name="streamResource">The stream resource.</param>
    /// <returns>A ModbusSerialMaster.</returns>
    /// <exception cref="System.ArgumentNullException">streamResource.</exception>
    public static ModbusSerialMaster CreateRtu(IStreamResource streamResource)
    {
        if (streamResource is null)
        {
            throw new ArgumentNullException(nameof(streamResource));
        }

        return new ModbusSerialMaster(new ModbusRtuTransport(streamResource));
    }

    /// <summary>Performs the serial-line return query diagnostic and verifies the echoed data.</summary>
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
