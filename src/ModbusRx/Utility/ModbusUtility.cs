// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace ModbusRx.Utility;

/// <summary>
///     Modbus utility methods with high-performance optimizations.
/// </summary>
public static class ModbusUtility
{
    private static readonly ushort[] CrcTable =
    {
            0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241,
            0XC601, 0X06C0, 0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440,
            0XCC01, 0X0CC0, 0X0D80, 0XCD41, 0X0F00, 0XCFC1, 0XCE81, 0X0E40,
            0X0A00, 0XCAC1, 0XCB81, 0X0B40, 0XC901, 0X09C0, 0X0880, 0XC841,
            0XD801, 0X18C0, 0X1980, 0XD941, 0X1B00, 0XDBC1, 0XDA81, 0X1A40,
            0X1E00, 0XDEC1, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41,
            0X1400, 0XD4C1, 0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641,
            0XD201, 0X12C0, 0X1380, 0XD341, 0X1100, 0XD1C1, 0XD081, 0X1040,
            0XF001, 0X30C0, 0X3180, 0XF141, 0X3300, 0XF3C1, 0XF281, 0X3240,
            0X3600, 0XF6C1, 0XF781, 0X3740, 0XF501, 0X35C0, 0X3480, 0XF441,
            0X3C00, 0XFCC1, 0XFD81, 0X3D40, 0XFF01, 0X3FC0, 0X3E80, 0XFE41,
            0XFA01, 0X3AC0, 0X3B80, 0XFB41, 0X3900, 0XF9C1, 0XF881, 0X3840,
            0X2800, 0XE8C1, 0XE981, 0X2940, 0XEB01, 0X2BC0, 0X2A80, 0XEA41,
            0XEE01, 0X2EC0, 0X2F80, 0XEF41, 0X2D00, 0XEDC1, 0XEC81, 0X2C40,
            0XE401, 0X24C0, 0X2580, 0XE541, 0X2700, 0XE7C1, 0XE681, 0X2640,
            0X2200, 0XE2C1, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041,
            0XA001, 0X60C0, 0X6180, 0XA141, 0X6300, 0XA3C1, 0XA281, 0X6240,
            0X6600, 0XA6C1, 0XA781, 0X6740, 0XA501, 0X65C0, 0X6480, 0XA441,
            0X6C00, 0XACC1, 0XAD81, 0X6D40, 0XAF01, 0X6FC0, 0X6E80, 0XAE41,
            0XAA01, 0X6AC0, 0X6B80, 0XAB41, 0X6900, 0XA9C1, 0XA881, 0X6840,
            0X7800, 0XB8C1, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41,
            0XBE01, 0X7EC0, 0X7F80, 0XBF41, 0X7D00, 0XBDC1, 0XBC81, 0X7C40,
            0XB401, 0X74C0, 0X7580, 0XB541, 0X7700, 0XB7C1, 0XB681, 0X7640,
            0X7200, 0XB2C1, 0XB381, 0X7340, 0XB101, 0X71C0, 0X7080, 0XB041,
            0X5000, 0X90C1, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
            0X9601, 0X56C0, 0X5780, 0X9741, 0X5500, 0X95C1, 0X9481, 0X5440,
            0X9C01, 0X5CC0, 0X5D80, 0X9D41, 0X5F00, 0X9FC1, 0X9E81, 0X5E40,
            0X5A00, 0X9AC1, 0X9B81, 0X5B40, 0X9901, 0X59C0, 0X5880, 0X9841,
            0X8801, 0X48C0, 0X4980, 0X8941, 0X4B00, 0X8BC1, 0X8A81, 0X4A40,
            0X4E00, 0X8EC1, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
            0X4400, 0X84C1, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641,
            0X8201, 0X42C0, 0X4380, 0X8341, 0X4100, 0X81C1, 0X8081, 0X4040,
    };

    /// <summary>
    ///     Converts four UInt16 values into a IEEE 64 floating point format using optimized memory operations.
    /// </summary>
    /// <param name="b3">Highest-order ushort value.</param>
    /// <param name="b2">Second-to-highest-order ushort value.</param>
    /// <param name="b1">Second-to-lowest-order ushort value.</param>
    /// <param name="b0">Lowest-order ushort value.</param>
    /// <returns>IEEE 64 floating point value.</returns>
    public static double GetDouble(ushort b3, ushort b2, ushort b1, ushort b0)
    {
        var value = BitConverter.GetBytes(b0)
            .Concat(BitConverter.GetBytes(b1))
            .Concat(BitConverter.GetBytes(b2))
            .Concat(BitConverter.GetBytes(b3))
            .ToArray();

        return BitConverter.ToDouble(value, 0);
    }

    /// <summary>
    ///     Converts two UInt16 values into a IEEE 32 floating point format using optimized memory operations.
    /// </summary>
    /// <param name="highOrderValue">High order ushort value.</param>
    /// <param name="lowOrderValue">Low order ushort value.</param>
    /// <returns>IEEE 32 floating point value.</returns>
    public static float GetSingle(ushort highOrderValue, ushort lowOrderValue)
    {
        var value = BitConverter.GetBytes(lowOrderValue)
            .Concat(BitConverter.GetBytes(highOrderValue))
            .ToArray();

        return BitConverter.ToSingle(value, 0);
    }

    /// <summary>
    /// Converts two UInt16 values into a UInt32 using optimized memory operations.
    /// </summary>
    /// <param name="highOrderValue">The high order value.</param>
    /// <param name="lowOrderValue">The low order value.</param>
    /// <returns>A UInt32 value.</returns>
    public static uint GetUInt32(ushort highOrderValue, ushort lowOrderValue)
    {
        var value = BitConverter.GetBytes(lowOrderValue)
            .Concat(BitConverter.GetBytes(highOrderValue))
            .ToArray();

        return BitConverter.ToUInt32(value, 0);
    }

    /// <summary>
    ///     Converts a span of bytes to an ASCII byte array using high-performance operations.
    /// </summary>
    /// <param name="numbers">The byte span.</param>
    /// <returns>An array of ASCII byte values.</returns>
    public static byte[] GetAsciiBytes(ReadOnlySpan<byte> numbers)
    {
        var result = new List<byte>(numbers.Length * 2);
        foreach (var b in numbers)
        {
            var hex = b.ToString("X2");
            result.AddRange(Encoding.UTF8.GetBytes(hex));
        }

        return result.ToArray();
    }

    /// <summary>
    ///     Converts a span of UInt16 to an ASCII byte array using high-performance operations.
    /// </summary>
    /// <param name="numbers">The ushort span.</param>
    /// <returns>An array of ASCII byte values.</returns>
    public static byte[] GetAsciiBytes(ReadOnlySpan<ushort> numbers)
    {
        var result = new List<byte>(numbers.Length * 4);
        foreach (var us in numbers)
        {
            var hex = us.ToString("X4");
            result.AddRange(Encoding.UTF8.GetBytes(hex));
        }

        return result.ToArray();
    }

    /// <summary>
    ///     Converts an array of bytes to an ASCII byte array.
    /// </summary>
    /// <param name="numbers">The byte array.</param>
    /// <returns>An array of ASCII byte values.</returns>
    public static byte[] GetAsciiBytes(params byte[] numbers) =>
        Encoding.UTF8.GetBytes(numbers.SelectMany(n => n.ToString("X2")).ToArray());

    /// <summary>
    ///     Converts an array of UInt16 to an ASCII byte array.
    /// </summary>
    /// <param name="numbers">The ushort array.</param>
    /// <returns>An array of ASCII byte values.</returns>
    public static byte[] GetAsciiBytes(params ushort[] numbers) =>
        Encoding.UTF8.GetBytes(numbers.SelectMany(n => n.ToString("X4")).ToArray());

    /// <summary>
    ///     Converts a network order byte span to a span of UInt16 values in host order using high-performance operations.
    /// </summary>
    /// <param name="networkBytes">The network order byte span.</param>
    /// <param name="result">The result span to write to.</param>
    /// <returns>The number of UInt16 values written.</returns>
    /// <exception cref="ArgumentException">Network bytes length is not even or result span is too small.</exception>
    public static int NetworkBytesToHostUInt16(ReadOnlySpan<byte> networkBytes, Span<ushort> result)
    {
        if (networkBytes.Length % 2 != 0)
        {
            throw new ArgumentException("Network bytes length must be even.", nameof(networkBytes));
        }

        var count = networkBytes.Length / 2;
        if (result.Length < count)
        {
            throw new ArgumentException("Result span is too small.", nameof(result));
        }

        for (var i = 0; i < count; i++)
        {
            var networkValue = BinaryPrimitives.ReadInt16BigEndian(networkBytes.Slice(i * 2, 2));
            result[i] = (ushort)networkValue;
        }

        return count;
    }

    /// <summary>
    ///     Converts a network order byte array to an array of UInt16 values in host order.
    /// </summary>
    /// <param name="networkBytes">The network order byte array.</param>
    /// <returns>The host order ushort array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when networkBytes is null.</exception>
    /// <exception cref="FormatException">Thrown when networkBytes length is not even.</exception>
    public static ushort[] NetworkBytesToHostUInt16(byte[] networkBytes)
    {
        if (networkBytes == null)
        {
            throw new ArgumentNullException(nameof(networkBytes));
        }

        if (networkBytes.Length % 2 != 0)
        {
            throw new FormatException(Resources.NetworkBytesNotEven);
        }

        var result = new ushort[networkBytes.Length / 2];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(networkBytes, i * 2));
        }

        return result;
    }

    /// <summary>
    ///     Converts a hex string to bytes using high-performance parsing.
    /// </summary>
    /// <param name="hex">The hex string.</param>
    /// <param name="result">The result span to write to.</param>
    /// <returns>The number of bytes written.</returns>
    /// <exception cref="ArgumentException">Thrown when hex length is not even or result span is too small.</exception>
    public static int HexToBytes(ReadOnlySpan<char> hex, Span<byte> result)
    {
        if (hex.Length % 2 != 0)
        {
            throw new ArgumentException("Hex string length must be even.", nameof(hex));
        }

        var count = hex.Length / 2;
        if (result.Length < count)
        {
            throw new ArgumentException("Result span is too small.", nameof(result));
        }

        for (var i = 0; i < count; i++)
        {
            var hexString = hex.Slice(i * 2, 2).ToString();
            if (!byte.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, null, out var value))
            {
                throw new FormatException($"Invalid hex characters at position {i * 2}.");
            }

            result[i] = value;
        }

        return count;
    }

    /// <summary>
    ///     Converts a hex string to a byte array.
    /// </summary>
    /// <param name="hex">The hex string.</param>
    /// <returns>Array of bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when hex is null.</exception>
    /// <exception cref="FormatException">Thrown when hex length is not even.</exception>
    public static byte[] HexToBytes(string hex)
    {
        if (hex == null)
        {
            throw new ArgumentNullException(nameof(hex));
        }

        if (hex.Length % 2 != 0)
        {
            throw new FormatException(Resources.HexCharacterCountNotEven);
        }

        var bytes = new byte[hex.Length / 2];

        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }

    /// <summary>
    ///     Calculate Longitudinal Redundancy Check using high-performance operations.
    /// </summary>
    /// <param name="data">The data span used in LRC.</param>
    /// <returns>LRC value.</returns>
    public static byte CalculateLrc(ReadOnlySpan<byte> data)
    {
        byte lrc = 0;
        foreach (var b in data)
        {
            lrc += b;
        }

        return (byte)((lrc ^ 0xFF) + 1);
    }

    /// <summary>
    ///     Calculate Longitudinal Redundancy Check.
    /// </summary>
    /// <param name="data">The data used in LRC.</param>
    /// <returns>LRC value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    public static byte CalculateLrc(byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        byte lrc = 0;

        foreach (var b in data)
        {
            lrc += b;
        }

        lrc = (byte)((lrc ^ 0xFF) + 1);

        return lrc;
    }

    /// <summary>
    ///     Calculate Cyclical Redundancy Check using high-performance operations.
    /// </summary>
    /// <param name="data">The data span used in CRC.</param>
    /// <param name="result">The result span to write CRC bytes to (must be at least 2 bytes).</param>
    /// <returns>The number of bytes written (always 2).</returns>
    /// <exception cref="ArgumentException">Thrown when result span is too small.</exception>
    public static int CalculateCrc(ReadOnlySpan<byte> data, Span<byte> result)
    {
        if (result.Length < 2)
        {
            throw new ArgumentException("Result span must be at least 2 bytes.", nameof(result));
        }

        var crc = ushort.MaxValue;
        foreach (var b in data)
        {
            var tableIndex = (byte)(crc ^ b);
            crc >>= 8;
            crc ^= CrcTable[tableIndex];
        }

        BinaryPrimitives.WriteUInt16LittleEndian(result, crc);
        return 2;
    }

    /// <summary>
    ///     Calculate Cyclical Redundancy Check.
    /// </summary>
    /// <param name="data">The data used in CRC.</param>
    /// <returns>CRC value as byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    public static byte[] CalculateCrc(byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var crc = ushort.MaxValue;

        foreach (var b in data)
        {
            var tableIndex = (byte)(crc ^ b);
            crc >>= 8;
            crc ^= CrcTable[tableIndex];
        }

        return BitConverter.GetBytes(crc);
    }

    /// <summary>
    /// Writes a double value to a span of UInt16 registers using optimized operations.
    /// </summary>
    /// <param name="value">The double value to write.</param>
    /// <param name="registers">The span of registers to write to (must be at least 4 elements).</param>
    /// <param name="swapWords">Whether to swap word order.</param>
    /// <exception cref="ArgumentException">Thrown when registers span is too small.</exception>
    public static void WriteDouble(double value, Span<ushort> registers, bool swapWords = true)
    {
        if (registers.Length < 4)
        {
            throw new ArgumentException("Registers span must be at least 4 elements.", nameof(registers));
        }

        var bytes = BitConverter.GetBytes(value);

        if (swapWords)
        {
            registers[0] = BitConverter.ToUInt16(bytes, 2);
            registers[1] = BitConverter.ToUInt16(bytes, 0);
            registers[2] = BitConverter.ToUInt16(bytes, 6);
            registers[3] = BitConverter.ToUInt16(bytes, 4);
        }
        else
        {
            registers[0] = BitConverter.ToUInt16(bytes, 0);
            registers[1] = BitConverter.ToUInt16(bytes, 2);
            registers[2] = BitConverter.ToUInt16(bytes, 4);
            registers[3] = BitConverter.ToUInt16(bytes, 6);
        }
    }

    /// <summary>
    /// Writes a float value to a span of UInt16 registers using optimized operations.
    /// </summary>
    /// <param name="value">The float value to write.</param>
    /// <param name="registers">The span of registers to write to (must be at least 2 elements).</param>
    /// <param name="swapWords">Whether to swap word order.</param>
    /// <exception cref="ArgumentException">Thrown when registers span is too small.</exception>
    public static void WriteSingle(float value, Span<ushort> registers, bool swapWords = true)
    {
        if (registers.Length < 2)
        {
            throw new ArgumentException("Registers span must be at least 2 elements.", nameof(registers));
        }

        var bytes = BitConverter.GetBytes(value);

        if (swapWords)
        {
            registers[0] = BitConverter.ToUInt16(bytes, 2);
            registers[1] = BitConverter.ToUInt16(bytes, 0);
        }
        else
        {
            registers[0] = BitConverter.ToUInt16(bytes, 0);
            registers[1] = BitConverter.ToUInt16(bytes, 2);
        }
    }

    /// <summary>
    /// Reads a double value from a span of UInt16 registers using optimized operations.
    /// </summary>
    /// <param name="registers">The span of registers to read from (must be at least 4 elements).</param>
    /// <param name="swapWords">Whether words are swapped.</param>
    /// <returns>The double value.</returns>
    /// <exception cref="ArgumentException">Thrown when registers span is too small.</exception>
    public static double ReadDouble(ReadOnlySpan<ushort> registers, bool swapWords = true)
    {
        if (registers.Length < 4)
        {
            throw new ArgumentException("Registers span must be at least 4 elements.", nameof(registers));
        }

        var bytes = new byte[8];

        if (swapWords)
        {
            var bytes0 = BitConverter.GetBytes(registers[1]);
            var bytes1 = BitConverter.GetBytes(registers[0]);
            var bytes2 = BitConverter.GetBytes(registers[3]);
            var bytes3 = BitConverter.GetBytes(registers[2]);

            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
            Array.Copy(bytes2, 0, bytes, 4, 2);
            Array.Copy(bytes3, 0, bytes, 6, 2);
        }
        else
        {
            var bytes0 = BitConverter.GetBytes(registers[0]);
            var bytes1 = BitConverter.GetBytes(registers[1]);
            var bytes2 = BitConverter.GetBytes(registers[2]);
            var bytes3 = BitConverter.GetBytes(registers[3]);

            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
            Array.Copy(bytes2, 0, bytes, 4, 2);
            Array.Copy(bytes3, 0, bytes, 6, 2);
        }

        return BitConverter.ToDouble(bytes, 0);
    }

    /// <summary>
    /// Reads a float value from a span of UInt16 registers using optimized operations.
    /// </summary>
    /// <param name="registers">The span of registers to read from (must be at least 2 elements).</param>
    /// <param name="swapWords">Whether words are swapped.</param>
    /// <returns>The float value.</returns>
    /// <exception cref="ArgumentException">Thrown when registers span is too small.</exception>
    public static float ReadSingle(ReadOnlySpan<ushort> registers, bool swapWords = true)
    {
        if (registers.Length < 2)
        {
            throw new ArgumentException("Registers span must be at least 2 elements.", nameof(registers));
        }

        var bytes = new byte[4];

        if (swapWords)
        {
            var bytes0 = BitConverter.GetBytes(registers[1]);
            var bytes1 = BitConverter.GetBytes(registers[0]);

            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
        }
        else
        {
            var bytes0 = BitConverter.GetBytes(registers[0]);
            var bytes1 = BitConverter.GetBytes(registers[1]);

            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
        }

        return BitConverter.ToSingle(bytes, 0);
    }
}
