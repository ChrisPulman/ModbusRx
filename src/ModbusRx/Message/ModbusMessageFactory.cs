// <copyright file="ModbusMessageFactory.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ModbusRx.Message;

/// <summary>
///     Modbus message factory.
/// </summary>
public static class ModbusMessageFactory
{
    /// <summary>
    ///     Minimum request frame length.
    /// </summary>
    private const int MinRequestFrameLength = 3;

    /// <summary>
    ///     Create a Modbus message.
    /// </summary>
    /// <typeparam name="T">Modbus message type.</typeparam>
    /// <param name="frame">Bytes of Modbus frame.</param>
    /// <returns>New Modbus message based on type and frame bytes.</returns>
    public static T CreateModbusMessage<T>(byte[] frame)
        where T : IModbusMessage, new()
    {
        IModbusMessage message = new T();
        message.Initialize(frame);

        return (T)message;
    }

    /// <summary>
    ///     Create a Modbus request.
    /// </summary>
    /// <param name="frame">Bytes of Modbus frame.</param>
    /// <returns>Modbus request.</returns>
    public static IModbusMessage CreateModbusRequest(byte[] frame)
    {
        if (frame?.Length < MinRequestFrameLength)
        {
            throw new FormatException($"Argument 'frame' must have a length of at least {MinRequestFrameLength} bytes.");
        }

        var functionCode = frame![1];
        return functionCode switch
        {
            Modbus.ReadCoils or Modbus.ReadInputs => CreateModbusMessage<ReadCoilsInputsRequest>(frame),
            Modbus.ReadHoldingRegisters or Modbus.ReadInputRegisters => CreateModbusMessage<ReadHoldingInputRegistersRequest>(frame),
            Modbus.WriteSingleCoil => CreateModbusMessage<WriteSingleCoilRequestResponse>(frame),
            Modbus.WriteSingleRegister => CreateModbusMessage<WriteSingleRegisterRequestResponse>(frame),
            Modbus.Diagnostics => CreateModbusMessage<DiagnosticsRequestResponse>(frame),
            Modbus.WriteMultipleCoils => CreateModbusMessage<WriteMultipleCoilsRequest>(frame),
            Modbus.WriteMultipleRegisters => CreateModbusMessage<WriteMultipleRegistersRequest>(frame),
            Modbus.ReadWriteMultipleRegisters => CreateModbusMessage<ReadWriteMultipleRegistersRequest>(frame),
            _ => throw new ArgumentException($"Unsupported function code {functionCode}", nameof(frame)),
        };
    }
}
