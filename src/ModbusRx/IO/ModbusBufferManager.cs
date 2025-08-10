// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using System.Buffers;
#endif

namespace ModbusRx.IO;

/// <summary>
/// High-performance buffer manager for Modbus message processing with cross-platform compatibility.
/// </summary>
public sealed class ModbusBufferManager : IDisposable
{
#if NET8_0_OR_GREATER
    private readonly ArrayPool<byte> _bytePool;
    private readonly ArrayPool<ushort> _ushortPool;
    private readonly ArrayPool<bool> _boolPool;
#endif
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusBufferManager"/> class.
    /// </summary>
    public ModbusBufferManager()
    {
#if NET8_0_OR_GREATER
        _bytePool = ArrayPool<byte>.Shared;
        _ushortPool = ArrayPool<ushort>.Shared;
        _boolPool = ArrayPool<bool>.Shared;
#endif
    }

    /// <summary>
    /// Copies data efficiently between arrays.
    /// </summary>
    /// <typeparam name="T">The type of data to copy.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="sourceIndex">The source index.</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="destinationIndex">The destination index.</param>
    /// <param name="length">The length to copy.</param>
    /// <returns>The number of elements copied.</returns>
    public static int CopyData<T>(T[] source, int sourceIndex, T[] destination, int destinationIndex, int length)
    {
        if (source == null || destination == null)
        {
            return 0;
        }

        var copyCount = Math.Min(length, Math.Min(source.Length - sourceIndex, destination.Length - destinationIndex));
        if (copyCount <= 0)
        {
            return 0;
        }

        Array.Copy(source, sourceIndex, destination, destinationIndex, copyCount);
        return copyCount;
    }

    /// <summary>
    /// Performs a high-performance comparison between two arrays.
    /// </summary>
    /// <typeparam name="T">The type of data to compare.</typeparam>
    /// <param name="array1">The first array.</param>
    /// <param name="array2">The second array.</param>
    /// <returns>True if the arrays are equal in content.</returns>
    public static bool CompareArrays<T>(T[] array1, T[] array2)
        where T : IEquatable<T>
    {
        if (array1 == null && array2 == null)
        {
            return true;
        }

        if (array1 == null || array2 == null)
        {
            return false;
        }

        if (array1.Length != array2.Length)
        {
            return false;
        }

        for (var i = 0; i < array1.Length; i++)
        {
            if (!array1[i].Equals(array2[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Clears an array with high performance.
    /// </summary>
    /// <typeparam name="T">The type of data to clear.</typeparam>
    /// <param name="array">The array to clear.</param>
    public static void ClearArray<T>(T[] array)
    {
        if (array != null)
        {
            Array.Clear(array, 0, array.Length);
        }
    }

    /// <summary>
    /// Rents a byte buffer from the pool or creates a new one.
    /// </summary>
    /// <param name="minimumLength">The minimum length required.</param>
    /// <returns>A rented buffer that should be returned when finished.</returns>
    public byte[] RentByteBuffer(int minimumLength)
    {
        lock (_lock)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ModbusBufferManager));
            }

#if NET8_0_OR_GREATER
            return _bytePool.Rent(minimumLength);
#else
            return new byte[minimumLength];
#endif
        }
    }

    /// <summary>
    /// Rents a ushort buffer from the pool or creates a new one.
    /// </summary>
    /// <param name="minimumLength">The minimum length required.</param>
    /// <returns>A rented buffer that should be returned when finished.</returns>
    public ushort[] RentUshortBuffer(int minimumLength)
    {
        lock (_lock)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ModbusBufferManager));
            }

#if NET8_0_OR_GREATER
            return _ushortPool.Rent(minimumLength);
#else
            return new ushort[minimumLength];
#endif
        }
    }

    /// <summary>
    /// Returns a byte buffer to the pool.
    /// </summary>
    /// <param name="buffer">The buffer to return.</param>
    /// <param name="clearArray">Whether to clear the array.</param>
    public void ReturnByteBuffer(byte[] buffer, bool clearArray = true)
    {
        if (buffer == null)
        {
            return;
        }

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

#if NET8_0_OR_GREATER
            _bytePool.Return(buffer, clearArray);
#endif
        }
    }

    /// <summary>
    /// Returns a ushort buffer to the pool.
    /// </summary>
    /// <param name="buffer">The buffer to return.</param>
    /// <param name="clearArray">Whether to clear the array.</param>
    public void ReturnUshortBuffer(ushort[] buffer, bool clearArray = true)
    {
        if (buffer == null)
        {
            return;
        }

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

#if NET8_0_OR_GREATER
            _ushortPool.Return(buffer, clearArray);
#endif
        }
    }

    /// <summary>
    /// Disposes the buffer manager and releases all resources.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            _disposed = true;
        }
    }
}
