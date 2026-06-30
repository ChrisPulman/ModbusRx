// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
using ModbusRx.Reactive.Device;
#else
using ModbusRx.Device;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Utility;
#else
using ModbusRx.Utility;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Extensions.Enron;
#else
namespace ModbusRx.Extensions.Enron;
#endif

/// <summary>Utility extensions for the Enron Modbus dialect.</summary>
public static class EnronModbusExtensions
{
    /// <summary>Provides Enron Modbus dialect operations for masters.</summary>
    /// <param name="master">The Modbus master.</param>
    extension(ModbusMaster master)
    {
    /// <summary>Read contiguous block of 32 bit holding registers.</summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <returns>Holding registers status.</returns>
    public async Task<uint[]> ReadHoldingRegisters32Async(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints)
    {
        if (master is null)
        {
            throw new ArgumentNullException(nameof(master));
        }

        ValidateNumberOfPoints(numberOfPoints, 62);

        // read 16 bit chunks and perform conversion
        var rawRegisters = await master.ReadHoldingRegistersAsync(
            slaveAddress,
            startAddress,
            (ushort)(numberOfPoints * 2));

        return Convert(rawRegisters);
    }

    /// <summary>Read contiguous block of 32 bit input registers.</summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <returns>Input registers status.</returns>
    public async Task<uint[]> ReadInputRegisters32Async(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints)
    {
        if (master is null)
        {
            throw new ArgumentNullException(nameof(master));
        }

        ValidateNumberOfPoints(numberOfPoints, 62);

        var rawRegisters = await master.ReadInputRegistersAsync(
            slaveAddress,
            startAddress,
            (ushort)(numberOfPoints * 2));

        return Convert(rawRegisters);
    }

    /// <summary>Write a single 16 bit holding register.</summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="registerAddress">Address to write.</param>
    /// <param name="value">Value to write.</param>
    public async void WriteSingleRegister32(
        byte slaveAddress,
        ushort registerAddress,
        uint value)
    {
        if (master is null)
        {
            throw new ArgumentNullException(nameof(master));
        }

        await master.WriteMultipleRegisters32Async(slaveAddress, registerAddress, [value]);
    }

    /// <summary>Write a block of contiguous 32 bit holding registers.</summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="startAddress">Address to begin writing values.</param>
    /// <param name="data">Values to write.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task WriteMultipleRegisters32Async(
        byte slaveAddress,
        ushort startAddress,
        uint[] data)
    {
        if (master is null)
        {
            throw new ArgumentNullException(nameof(master));
        }

        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (data.Length is 0 or > 61)
        {
            throw new ArgumentException("The length of argument data must be between 1 and 61 inclusive.");
        }

        await master.WriteMultipleRegistersAsync(slaveAddress, startAddress, Convert(data));
    }
    }

    /// <summary>Convert the 32 bit registers to two 16 bit values.</summary>
    /// <param name="registers">The registers value.</param>
    /// <returns>The result.</returns>
    private static ushort[] Convert(uint[] registers)
    {
        var result = new ushort[registers.Length * 2];
        for (var i = 0; i < registers.Length; i++)
        {
            var bytes = BitConverter.GetBytes(registers[i]);
            result[i * 2] = BitConverter.ToUInt16(bytes, 0);
            result[(i * 2) + 1] = BitConverter.ToUInt16(bytes, 2);
        }

        return result;
    }

    /// <summary>Convert the 16 bit registers to 32 bit registers.</summary>
    /// <param name="registers">The registers value.</param>
    /// <returns>The result.</returns>
    private static uint[] Convert(ushort[] registers)
    {
        var result = new uint[registers.Length / 2];
        for (var i = 0; i < result.Length; i++)
        {
            var registerIndex = i * 2;
            result[i] = ModbusUtility.GetUInt32(registers[registerIndex + 1], registers[registerIndex]);
        }

        return result;
    }

    /// <summary>Executes the Validate Number Of Points operation.</summary>
    /// <param name="numberOfPoints">The number Of Points value.</param>
    /// <param name="maxNumberOfPoints">The max Number Of Points value.</param>
    private static void ValidateNumberOfPoints(ushort numberOfPoints, ushort maxNumberOfPoints)
    {
        if (numberOfPoints >= 1 && numberOfPoints <= maxNumberOfPoints)
        {
            return;
        }

        var msg = $"Argument numberOfPoints must be between 1 and {maxNumberOfPoints} inclusive.";
        throw new ArgumentException(msg);
    }
}
