﻿// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using ModbusRx.IO;

namespace ModbusRx.Device;

/// <summary>
///     Modbus master device.
/// </summary>
public interface IModbusMaster : ICancelable
{
    /// <summary>
    ///     Gets transport used by this master.
    /// </summary>
    ModbusTransport? Transport { get; }

    /// <summary>
    ///    Asynchronously reads from 1 to 2000 contiguous coils status.
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of coils to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    Task<bool[]> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    ///    Asynchronously reads from 1 to 2000 contiguous discrete input status.
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of discrete inputs to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    Task<bool[]> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    ///    Asynchronously reads contiguous block of holding registers.
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    Task<ushort[]> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    ///    Asynchronously reads contiguous block of input registers.
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    Task<ushort[]> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    ///    Asynchronously writes a single coil value.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="coilAddress">Address to write value to.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteSingleCoilAsync(byte slaveAddress, ushort coilAddress, bool value);

    /// <summary>
    ///    Asynchronously writes a single holding register.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="registerAddress">Address to write.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteSingleRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value);

    /// <summary>
    ///    Asynchronously writes a block of 1 to 123 contiguous registers.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="startAddress">Address to begin writing values.</param>
    /// <param name="data">Values to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteMultipleRegistersAsync(byte slaveAddress, ushort startAddress, ushort[] data);

    /// <summary>
    ///    Asynchronously writes a sequence of coils.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="startAddress">Address to begin writing values.</param>
    /// <param name="data">Values to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] data);

    /// <summary>
    ///    Asynchronously performs a combination of one read operation and one write operation in a single Modbus transaction.
    ///    The write operation is performed before the read.
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startReadAddress">Address to begin reading (Holding registers are addressed starting at 0).</param>
    /// <param name="numberOfPointsToRead">Number of registers to read.</param>
    /// <param name="startWriteAddress">Address to begin writing (Holding registers are addressed starting at 0).</param>
    /// <param name="writeData">Register values to write.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<ushort[]> ReadWriteMultipleRegistersAsync(
        byte slaveAddress,
        ushort startReadAddress,
        ushort numberOfPointsToRead,
        ushort startWriteAddress,
        ushort[] writeData);
}
