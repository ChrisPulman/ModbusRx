// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using ModbusRx.Utility;

namespace ModbusRx.IO;

/// <summary>
/// High-performance Modbus message factory with cross-platform optimizations.
/// </summary>
public static class OptimizedModbusMessageFactory
{
    private static readonly ModbusBufferManager BufferManager = new();

    /// <summary>
    /// Creates a read holding registers request with high performance.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <returns>The serialized message bytes.</returns>
    public static byte[] CreateReadHoldingRegistersRequest(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        var buffer = BufferManager.RentByteBuffer(8);
        try
        {
            buffer[0] = slaveAddress;
            buffer[1] = Modbus.ReadHoldingRegisters;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(2, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(4, 2), numberOfPoints);

            // Calculate and append CRC
            var crc = ModbusUtility.CalculateCrc(buffer.AsSpan(0, 6).ToArray());
            buffer[6] = crc[0];
            buffer[7] = crc[1];

            var result = new byte[8];
            Array.Copy(buffer, result, 8);
            return result;
        }
        finally
        {
            BufferManager.ReturnByteBuffer(buffer);
        }
    }

    /// <summary>
    /// Creates a read coils request with high performance.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <returns>The serialized message bytes.</returns>
    public static byte[] CreateReadCoilsRequest(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        var buffer = BufferManager.RentByteBuffer(8);
        try
        {
            buffer[0] = slaveAddress;
            buffer[1] = Modbus.ReadCoils;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(2, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(4, 2), numberOfPoints);

            // Calculate and append CRC
            var crc = ModbusUtility.CalculateCrc(buffer.AsSpan(0, 6).ToArray());
            buffer[6] = crc[0];
            buffer[7] = crc[1];

            var result = new byte[8];
            Array.Copy(buffer, result, 8);
            return result;
        }
        finally
        {
            BufferManager.ReturnByteBuffer(buffer);
        }
    }

    /// <summary>
    /// Creates a write single register request with high performance.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="registerAddress">The register address.</param>
    /// <param name="value">The value to write.</param>
    /// <returns>The serialized message bytes.</returns>
    public static byte[] CreateWriteSingleRegisterRequest(byte slaveAddress, ushort registerAddress, ushort value)
    {
        var buffer = BufferManager.RentByteBuffer(8);
        try
        {
            buffer[0] = slaveAddress;
            buffer[1] = Modbus.WriteSingleRegister;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(2, 2), registerAddress);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(4, 2), value);

            // Calculate and append CRC
            var crc = ModbusUtility.CalculateCrc(buffer.AsSpan(0, 6).ToArray());
            buffer[6] = crc[0];
            buffer[7] = crc[1];

            var result = new byte[8];
            Array.Copy(buffer, result, 8);
            return result;
        }
        finally
        {
            BufferManager.ReturnByteBuffer(buffer);
        }
    }

    /// <summary>
    /// Creates a write multiple registers request with high performance.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="values">The values to write.</param>
    /// <returns>The serialized message bytes.</returns>
    public static byte[] CreateWriteMultipleRegistersRequest(byte slaveAddress, ushort startAddress, ushort[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var messageLength = 9 + (values.Length * 2); // Header + byte count + data + CRC
        var buffer = BufferManager.RentByteBuffer(messageLength);
        try
        {
            buffer[0] = slaveAddress;
            buffer[1] = Modbus.WriteMultipleRegisters;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(2, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(4, 2), (ushort)values.Length);
            buffer[6] = (byte)(values.Length * 2); // Byte count

            // Write register values
            var dataIndex = 7;
            for (var i = 0; i < values.Length; i++)
            {
                BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(dataIndex, 2), values[i]);
                dataIndex += 2;
            }

            // Calculate and append CRC
            var crc = ModbusUtility.CalculateCrc(buffer.AsSpan(0, messageLength - 2).ToArray());
            buffer[messageLength - 2] = crc[0];
            buffer[messageLength - 1] = crc[1];

            var result = new byte[messageLength];
            Array.Copy(buffer, result, messageLength);
            return result;
        }
        finally
        {
            BufferManager.ReturnByteBuffer(buffer);
        }
    }

    /// <summary>
    /// Creates a write single coil request with high performance.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="coilAddress">The coil address.</param>
    /// <param name="value">The value to write.</param>
    /// <returns>The serialized message bytes.</returns>
    public static byte[] CreateWriteSingleCoilRequest(byte slaveAddress, ushort coilAddress, bool value)
    {
        var buffer = BufferManager.RentByteBuffer(8);
        try
        {
            buffer[0] = slaveAddress;
            buffer[1] = Modbus.WriteSingleCoil;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(2, 2), coilAddress);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(4, 2), value ? (ushort)0xFF00 : (ushort)0x0000);

            // Calculate and append CRC
            var crc = ModbusUtility.CalculateCrc(buffer.AsSpan(0, 6).ToArray());
            buffer[6] = crc[0];
            buffer[7] = crc[1];

            var result = new byte[8];
            Array.Copy(buffer, result, 8);
            return result;
        }
        finally
        {
            BufferManager.ReturnByteBuffer(buffer);
        }
    }

    /// <summary>
    /// Creates a write multiple coils request with high performance.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="values">The values to write.</param>
    /// <returns>The serialized message bytes.</returns>
    public static byte[] CreateWriteMultipleCoilsRequest(byte slaveAddress, ushort startAddress, bool[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var byteCount = (values.Length + 7) / 8; // Round up to next byte
        var messageLength = 9 + byteCount; // Header + byte count + data + CRC
        var buffer = BufferManager.RentByteBuffer(messageLength);
        try
        {
            buffer[0] = slaveAddress;
            buffer[1] = Modbus.WriteMultipleCoils;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(2, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(4, 2), (ushort)values.Length);
            buffer[6] = (byte)byteCount;

            // Pack boolean values into bytes
            var dataIndex = 7;
            for (var i = 0; i < values.Length; i++)
            {
                var byteIndex = dataIndex + (i / 8);
                var bitIndex = i % 8;

                if (values[i])
                {
                    buffer[byteIndex] |= (byte)(1 << bitIndex);
                }
            }

            // Calculate and append CRC
            var crc = ModbusUtility.CalculateCrc(buffer.AsSpan(0, messageLength - 2).ToArray());
            buffer[messageLength - 2] = crc[0];
            buffer[messageLength - 1] = crc[1];

            var result = new byte[messageLength];
            Array.Copy(buffer, result, messageLength);
            return result;
        }
        finally
        {
            BufferManager.ReturnByteBuffer(buffer);
        }
    }

    /// <summary>
    /// Parses a read holding registers response with high performance.
    /// </summary>
    /// <param name="responseData">The response data.</param>
    /// <returns>The parsed register values.</returns>
    /// <exception cref="ArgumentException">Thrown when response data is invalid.</exception>
    public static ushort[] ParseReadHoldingRegistersResponse(byte[] responseData)
    {
        if (responseData == null)
        {
            throw new ArgumentNullException(nameof(responseData));
        }

        if (responseData.Length < 5)
        {
            throw new ArgumentException("Response data too short.", nameof(responseData));
        }

        var byteCount = responseData[2];
        var expectedLength = 5 + byteCount; // Slave + Function + ByteCount + Data + CRC

        if (responseData.Length < expectedLength)
        {
            throw new ArgumentException("Response data incomplete.", nameof(responseData));
        }

        var valueCount = byteCount / 2;
        var values = new ushort[valueCount];

        for (var i = 0; i < valueCount; i++)
        {
            var dataIndex = 3 + (i * 2);
            values[i] = BinaryPrimitives.ReadUInt16BigEndian(responseData.AsSpan(dataIndex, 2));
        }

        return values;
    }

    /// <summary>
    /// Parses a read coils response with high performance.
    /// </summary>
    /// <param name="responseData">The response data.</param>
    /// <param name="numberOfCoils">The number of coils requested.</param>
    /// <returns>The parsed coil values.</returns>
    /// <exception cref="ArgumentException">Thrown when response data is invalid.</exception>
    public static bool[] ParseReadCoilsResponse(byte[] responseData, int numberOfCoils)
    {
        if (responseData == null)
        {
            throw new ArgumentNullException(nameof(responseData));
        }

        if (responseData.Length < 5)
        {
            throw new ArgumentException("Response data too short.", nameof(responseData));
        }

        var byteCount = responseData[2];
        var expectedLength = 5 + byteCount; // Slave + Function + ByteCount + Data + CRC

        if (responseData.Length < expectedLength)
        {
            throw new ArgumentException("Response data incomplete.", nameof(responseData));
        }

        var values = new bool[numberOfCoils];

        for (var i = 0; i < numberOfCoils; i++)
        {
            var byteIndex = 3 + (i / 8);
            var bitIndex = i % 8;

            if (byteIndex < responseData.Length)
            {
                values[i] = (responseData[byteIndex] & (1 << bitIndex)) != 0;
            }
        }

        return values;
    }

    /// <summary>
    /// Validates a Modbus message CRC with high performance.
    /// </summary>
    /// <param name="messageData">The complete message data including CRC.</param>
    /// <returns>True if CRC is valid.</returns>
    public static bool ValidateMessageCrc(byte[] messageData)
    {
        if (messageData == null || messageData.Length < 4)
        {
            return false;
        }

        var dataLength = messageData.Length - 2;
        var dataForCrc = new byte[dataLength];
        Array.Copy(messageData, dataForCrc, dataLength);

        var calculatedCrc = ModbusUtility.CalculateCrc(dataForCrc);
        var receivedCrc = new byte[2];
        Array.Copy(messageData, dataLength, receivedCrc, 0, 2);

        return ModbusBufferManager.CompareArrays(calculatedCrc, receivedCrc);
    }

    /// <summary>
    /// Disposes the shared buffer manager.
    /// </summary>
    public static void DisposeSharedResources() => BufferManager?.Dispose();
}
