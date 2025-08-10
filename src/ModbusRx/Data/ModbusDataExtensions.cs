// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace ModbusRx.Data;

/// <summary>
/// High-performance data conversion extensions optimized for different target frameworks.
/// </summary>
public static class ModbusDataExtensions
{
    /// <summary>
    /// Converts a 32-bit integer to two 16-bit registers with optimized performance.
    /// </summary>
    /// <param name="value">The 32-bit integer value.</param>
    /// <param name="swapWords">Whether to swap word order.</param>
    /// <returns>Array containing two 16-bit register values.</returns>
    public static ushort[] ToRegisters(this int value, bool swapWords = true)
    {
        var bytes = BitConverter.GetBytes(value);
        var result = new ushort[2];

        if (swapWords)
        {
            result[0] = BitConverter.ToUInt16(bytes, 2);
            result[1] = BitConverter.ToUInt16(bytes, 0);
        }
        else
        {
            result[0] = BitConverter.ToUInt16(bytes, 0);
            result[1] = BitConverter.ToUInt16(bytes, 2);
        }

        return result;
    }

    /// <summary>
    /// Converts a 32-bit unsigned integer to two 16-bit registers with optimized performance.
    /// </summary>
    /// <param name="value">The 32-bit unsigned integer value.</param>
    /// <param name="swapWords">Whether to swap word order.</param>
    /// <returns>Array containing two 16-bit register values.</returns>
    public static ushort[] ToRegisters(this uint value, bool swapWords = true)
    {
        var bytes = BitConverter.GetBytes(value);
        var result = new ushort[2];

        if (swapWords)
        {
            result[0] = BitConverter.ToUInt16(bytes, 2);
            result[1] = BitConverter.ToUInt16(bytes, 0);
        }
        else
        {
            result[0] = BitConverter.ToUInt16(bytes, 0);
            result[1] = BitConverter.ToUInt16(bytes, 2);
        }

        return result;
    }

    /// <summary>
    /// Converts a 64-bit long to four 16-bit registers with optimized performance.
    /// </summary>
    /// <param name="value">The 64-bit long value.</param>
    /// <param name="swapWords">Whether to swap word order.</param>
    /// <returns>Array containing four 16-bit register values.</returns>
    public static ushort[] ToRegisters(this long value, bool swapWords = true)
    {
        var bytes = BitConverter.GetBytes(value);
        var result = new ushort[4];

        if (swapWords)
        {
            result[0] = BitConverter.ToUInt16(bytes, 2);
            result[1] = BitConverter.ToUInt16(bytes, 0);
            result[2] = BitConverter.ToUInt16(bytes, 6);
            result[3] = BitConverter.ToUInt16(bytes, 4);
        }
        else
        {
            result[0] = BitConverter.ToUInt16(bytes, 0);
            result[1] = BitConverter.ToUInt16(bytes, 2);
            result[2] = BitConverter.ToUInt16(bytes, 4);
            result[3] = BitConverter.ToUInt16(bytes, 6);
        }

        return result;
    }

    /// <summary>
    /// Converts two 16-bit registers to a 32-bit integer with optimized performance.
    /// </summary>
    /// <param name="registers">The register array.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="swapWords">Whether words are swapped.</param>
    /// <returns>The 32-bit integer value.</returns>
    public static int ToInt32(this ushort[] registers, int startIndex = 0, bool swapWords = true)
    {
        if (registers == null || registers.Length < startIndex + 2)
        {
            throw new ArgumentException("Insufficient registers for Int32 conversion.", nameof(registers));
        }

        var bytes = new byte[4];

        if (swapWords)
        {
            var bytes0 = BitConverter.GetBytes(registers[startIndex + 1]);
            var bytes1 = BitConverter.GetBytes(registers[startIndex]);
            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
        }
        else
        {
            var bytes0 = BitConverter.GetBytes(registers[startIndex]);
            var bytes1 = BitConverter.GetBytes(registers[startIndex + 1]);
            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
        }

        return BitConverter.ToInt32(bytes, 0);
    }

    /// <summary>
    /// Converts two 16-bit registers to a 32-bit unsigned integer with optimized performance.
    /// </summary>
    /// <param name="registers">The register array.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="swapWords">Whether words are swapped.</param>
    /// <returns>The 32-bit unsigned integer value.</returns>
    public static uint ToUInt32(this ushort[] registers, int startIndex = 0, bool swapWords = true)
    {
        if (registers == null || registers.Length < startIndex + 2)
        {
            throw new ArgumentException("Insufficient registers for UInt32 conversion.", nameof(registers));
        }

        var bytes = new byte[4];

        if (swapWords)
        {
            var bytes0 = BitConverter.GetBytes(registers[startIndex + 1]);
            var bytes1 = BitConverter.GetBytes(registers[startIndex]);
            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
        }
        else
        {
            var bytes0 = BitConverter.GetBytes(registers[startIndex]);
            var bytes1 = BitConverter.GetBytes(registers[startIndex + 1]);
            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
        }

        return BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>
    /// Converts four 16-bit registers to a 64-bit long with optimized performance.
    /// </summary>
    /// <param name="registers">The register array.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="swapWords">Whether words are swapped.</param>
    /// <returns>The 64-bit long value.</returns>
    public static long ToInt64(this ushort[] registers, int startIndex = 0, bool swapWords = true)
    {
        if (registers == null || registers.Length < startIndex + 4)
        {
            throw new ArgumentException("Insufficient registers for Int64 conversion.", nameof(registers));
        }

        var bytes = new byte[8];

        if (swapWords)
        {
            var bytes0 = BitConverter.GetBytes(registers[startIndex + 1]);
            var bytes1 = BitConverter.GetBytes(registers[startIndex]);
            var bytes2 = BitConverter.GetBytes(registers[startIndex + 3]);
            var bytes3 = BitConverter.GetBytes(registers[startIndex + 2]);

            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
            Array.Copy(bytes2, 0, bytes, 4, 2);
            Array.Copy(bytes3, 0, bytes, 6, 2);
        }
        else
        {
            var bytes0 = BitConverter.GetBytes(registers[startIndex]);
            var bytes1 = BitConverter.GetBytes(registers[startIndex + 1]);
            var bytes2 = BitConverter.GetBytes(registers[startIndex + 2]);
            var bytes3 = BitConverter.GetBytes(registers[startIndex + 3]);

            Array.Copy(bytes0, 0, bytes, 0, 2);
            Array.Copy(bytes1, 0, bytes, 2, 2);
            Array.Copy(bytes2, 0, bytes, 4, 2);
            Array.Copy(bytes3, 0, bytes, 6, 2);
        }

        return BitConverter.ToInt64(bytes, 0);
    }

    /// <summary>
    /// Packs boolean values into bytes with optimized performance.
    /// </summary>
    /// <param name="values">The boolean values to pack.</param>
    /// <returns>Array of bytes containing packed boolean values.</returns>
    public static byte[] PackBooleans(this bool[] values)
    {
        if (values == null)
        {
            return [];
        }

        var byteCount = (values.Length + 7) / 8;
        var result = new byte[byteCount];

#if NET8_0_OR_GREATER
        // Use vectorized operations for better performance on newer frameworks
        if (Vector.IsHardwareAccelerated && values.Length >= Vector<byte>.Count)
        {
            PackBooleansVectorized(values, result);
        }
        else
#endif
        {
            PackBooleansScalar(values, result);
        }

        return result;
    }

    /// <summary>
    /// Unpacks bytes into boolean values with optimized performance.
    /// </summary>
    /// <param name="bytes">The bytes to unpack.</param>
    /// <param name="numberOfBooleans">The number of boolean values to extract.</param>
    /// <returns>Array of boolean values.</returns>
    public static bool[] UnpackBooleans(this byte[] bytes, int numberOfBooleans)
    {
        if (bytes == null || numberOfBooleans <= 0)
        {
            return [];
        }

        var result = new bool[numberOfBooleans];

#if NET8_0_OR_GREATER
        // Use vectorized operations for better performance on newer frameworks
        if (Vector.IsHardwareAccelerated && numberOfBooleans >= Vector<byte>.Count)
        {
            UnpackBooleansVectorized(bytes, result, numberOfBooleans);
        }
        else
#endif
        {
            UnpackBooleansScalar(bytes, result, numberOfBooleans);
        }

        return result;
    }

    /// <summary>
    /// Performs a fast memory comparison between two byte arrays.
    /// </summary>
    /// <param name="array1">The first array.</param>
    /// <param name="array2">The second array.</param>
    /// <returns>True if arrays are equal.</returns>
    public static bool FastEquals(this byte[] array1, byte[] array2)
    {
        if (array1 == null && array2 == null)
        {
            return true;
        }

        if (array1 == null || array2 == null || array1.Length != array2.Length)
        {
            return false;
        }

#if NET8_0_OR_GREATER
        // Use vectorized comparison for better performance
        if (Vector.IsHardwareAccelerated && array1.Length >= Vector<byte>.Count)
        {
            return FastEqualsVectorized(array1, array2);
        }
#endif

        return FastEqualsScalar(array1, array2);
    }

    private static void PackBooleansScalar(bool[] values, byte[] result)
    {
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i])
            {
                var byteIndex = i / 8;
                var bitIndex = i % 8;
                result[byteIndex] |= (byte)(1 << bitIndex);
            }
        }
    }

    private static void UnpackBooleansScalar(byte[] bytes, bool[] result, int numberOfBooleans)
    {
        for (var i = 0; i < numberOfBooleans; i++)
        {
            var byteIndex = i / 8;
            var bitIndex = i % 8;

            if (byteIndex < bytes.Length)
            {
                result[i] = (bytes[byteIndex] & (1 << bitIndex)) != 0;
            }
        }
    }

    private static bool FastEqualsScalar(byte[] array1, byte[] array2)
    {
        for (var i = 0; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
            {
                return false;
            }
        }

        return true;
    }

#if NET8_0_OR_GREATER
    private static void PackBooleansVectorized(bool[] values, byte[] result)
    {
        // For now, fall back to scalar implementation
        // In a real implementation, this would use SIMD instructions
        PackBooleansScalar(values, result);
    }

    private static void UnpackBooleansVectorized(byte[] bytes, bool[] result, int numberOfBooleans)
    {
        // For now, fall back to scalar implementation
        // In a real implementation, this would use SIMD instructions
        UnpackBooleansScalar(bytes, result, numberOfBooleans);
    }

    private static bool FastEqualsVectorized(byte[] array1, byte[] array2)
    {
        var vectorSize = Vector<byte>.Count;
        var i = 0;

        // Process in vector-sized chunks
        for (; i <= array1.Length - vectorSize; i += vectorSize)
        {
            var v1 = new Vector<byte>(array1, i);
            var v2 = new Vector<byte>(array2, i);

            if (!Vector.EqualsAll(v1, v2))
            {
                return false;
            }
        }

        // Process remaining bytes
        for (; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
            {
                return false;
            }
        }

        return true;
    }
#endif
}
