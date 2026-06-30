// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ModbusRx.DriverTest;

/// <summary>Handles Modbus RTU request frames for the test emulator.</summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ModbusRtuHandler"/> class.
/// </remarks>
/// <param name="controller">The controller.</param>
/// <param name="slaveId">The slave identifier.</param>
public class ModbusRtuHandler(DummyTemperatureController controller, byte slaveId = 1)
{
    /// <summary>The slave identifier handled by this instance.</summary>
    private readonly byte _slaveId = slaveId;

    /// <summary>Handles the request.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="length">The length.</param>
    /// <returns>A byte array.</returns>
    public byte[]? HandleRequest(byte[] frame, int length)
    {
        if (length < 4)
        {
            return null;
        }

        if (frame is null)
        {
            return null;
        }

        var slave = frame[0];
        if (slave != _slaveId)
        {
            return null; // Not for this slave
        }

        var function = frame[1];

        var crcReceived = (ushort)(frame[length - 2] | (frame[length - 1] << 8));
        var crcCalc = ModbusCrc.Compute(frame, length - 2);

        return crcReceived != crcCalc ? null : function switch
        {
            0x03 => HandleReadHoldingRegisters(frame),
            0x06 => HandleWriteSingleRegister(frame),
            _ => null,
        };
    }

    /// <summary>Handles a read holding registers request.</summary>
    /// <param name="frame">The request frame.</param>
    /// <returns>The response frame.</returns>
    private byte[] HandleReadHoldingRegisters(byte[] frame)
    {
        var start = (ushort)((frame[2] << 8) | frame[3]);
        var count = (ushort)((frame[4] << 8) | frame[5]);

        var response = new byte[3 + (count * 2) + 2];
        response[0] = frame[0];
        response[1] = 0x03;
        response[2] = (byte)(count * 2);

        for (var i = 0; i < count; i++)
        {
            var value = controller.ReadRegister((ushort)(start + i));
            response[3 + (i * 2)] = (byte)(value >> 8);
            response[4 + (i * 2)] = (byte)(value & 0xFF);
        }

        var crc = ModbusCrc.Compute(response, response.Length - 2);
        response[^2] = (byte)(crc & 0xFF);
        response[^1] = (byte)(crc >> 8);

        return response;
    }

    /// <summary>Handles a write single register request.</summary>
    /// <param name="frame">The request frame.</param>
    /// <returns>The response frame.</returns>
    private byte[] HandleWriteSingleRegister(byte[] frame)
    {
        var address = (ushort)((frame[2] << 8) | frame[3]);
        var value = (ushort)((frame[4] << 8) | frame[5]);

        controller.WriteRegister(address, value);

        // Echo request back (Modbus spec)
        var response = new byte[8];
        Array.Copy(frame, response, 6);

        var crc = ModbusCrc.Compute(response, 6);
        response[6] = (byte)(crc & 0xFF);
        response[7] = (byte)(crc >> 8);

        return response;
    }
}
