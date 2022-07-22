// <copyright file="ReadWriteMultipleRegistersRequest.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using ModbusRx.Data;

namespace ModbusRx.Message;

/// <summary>
/// ReadWriteMultipleRegistersRequest.
/// </summary>
/// <seealso cref="ModbusRx.Message.AbstractModbusMessage" />
/// <seealso cref="ModbusRx.Message.IModbusRequest" />
public class ReadWriteMultipleRegistersRequest : AbstractModbusMessage, IModbusRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadWriteMultipleRegistersRequest"/> class.
    /// </summary>
    public ReadWriteMultipleRegistersRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadWriteMultipleRegistersRequest"/> class.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startReadAddress">The start read address.</param>
    /// <param name="numberOfPointsToRead">The number of points to read.</param>
    /// <param name="startWriteAddress">The start write address.</param>
    /// <param name="writeData">The write data.</param>
    public ReadWriteMultipleRegistersRequest(
        byte slaveAddress,
        ushort startReadAddress,
        ushort numberOfPointsToRead,
        ushort startWriteAddress,
        RegisterCollection writeData)
        : base(slaveAddress, Modbus.ReadWriteMultipleRegisters)
    {
        ReadRequest = new ReadHoldingInputRegistersRequest(
            Modbus.ReadHoldingRegisters,
            slaveAddress,
            startReadAddress,
            numberOfPointsToRead);

        WriteRequest = new WriteMultipleRegistersRequest(
            slaveAddress,
            startWriteAddress,
            writeData);
    }

    /// <inheritdoc/>
    public override byte[] ProtocolDataUnit
    {
        get
        {
            var readPdu = ReadRequest?.ProtocolDataUnit;
            var writePdu = WriteRequest?.ProtocolDataUnit;
            var stream = new MemoryStream(readPdu!.Length + writePdu!.Length);

            stream.WriteByte(FunctionCode);

            // read and write PDUs without function codes
            stream.Write(readPdu, 1, readPdu.Length - 1);
            stream.Write(writePdu, 1, writePdu.Length - 1);

            return stream.ToArray();
        }
    }

    /// <summary>
    /// Gets the read request.
    /// </summary>
    /// <value>
    /// The read request.
    /// </value>
    public ReadHoldingInputRegistersRequest? ReadRequest { get; private set; }

    /// <summary>
    /// Gets the write request.
    /// </summary>
    /// <value>
    /// The write request.
    /// </value>
    public WriteMultipleRegistersRequest? WriteRequest { get; private set; }

    /// <inheritdoc/>
    public override int MinimumFrameSize => 11;

    /// <inheritdoc/>
    public override string ToString() =>
        $"Write {WriteRequest?.NumberOfPoints} holding registers starting at address {WriteRequest?.StartAddress}, and read {ReadRequest?.NumberOfPoints} registers starting at address {ReadRequest?.StartAddress}.";

    /// <inheritdoc/>
    public void ValidateResponse(IModbusMessage response)
    {
        var typedResponse = (ReadHoldingInputRegistersResponse)response;
        var expectedByteCount = ReadRequest?.NumberOfPoints * 2;

        if (expectedByteCount != typedResponse?.ByteCount)
        {
            var msg = $"Unexpected byte count in response. Expected {expectedByteCount}, received {typedResponse?.ByteCount}.";
            throw new IOException(msg);
        }
    }

    /// <inheritdoc/>
    protected override void InitializeUnique(byte[] frame)
    {
        if (frame?.Length < MinimumFrameSize + frame![10])
        {
            throw new FormatException("Message frame does not contain enough bytes.");
        }

        var readFrame = new byte[2 + 4];
        var writeFrame = new byte[frame.Length - 6 + 2];

        readFrame[0] = writeFrame[0] = SlaveAddress;
        readFrame[1] = writeFrame[1] = FunctionCode;

        Buffer.BlockCopy(frame, 2, readFrame, 2, 4);
        Buffer.BlockCopy(frame, 6, writeFrame, 2, frame.Length - 6);

        ReadRequest = ModbusMessageFactory.CreateModbusMessage<ReadHoldingInputRegistersRequest>(readFrame);
        WriteRequest = ModbusMessageFactory.CreateModbusMessage<WriteMultipleRegistersRequest>(writeFrame);
    }
}
