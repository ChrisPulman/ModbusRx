// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using ModbusRx.Device;
using ModbusRx.IO;
using Moq;
using Xunit;

namespace ModbusRx.UnitTests.Device;

/// <summary>
/// ModbusMasterFixture.
/// </summary>
public class ModbusMasterFixture
{
    /// <summary>
    /// Gets the stream rsource.
    /// </summary>
    /// <value>
    /// The stream rsource.
    /// </value>
    private static IStreamResource StreamRsource => new Mock<IStreamResource>(MockBehavior.Strict).Object;

    /// <summary>
    /// Gets the master.
    /// </summary>
    /// <value>
    /// The master.
    /// </value>
    private static ModbusSerialMaster Master => ModbusSerialMaster.CreateRtu(StreamRsource);

    /// <summary>
    /// Reads the coils.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadCoils()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadCoilsAsync(1, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadCoilsAsync(1, 1, 2001));
    }

    /// <summary>
    /// Reads the inputs.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadInputs()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadInputsAsync(1, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadInputsAsync(1, 1, 2001));
    }

    /// <summary>
    /// Reads the holding registers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadHoldingRegistersAsync()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadHoldingRegistersAsync(1, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadHoldingRegistersAsync(1, 1, 126));
    }

    /// <summary>
    /// Reads the input registers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadInputRegistersAsync()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadInputRegistersAsync(1, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadInputRegistersAsync(1, 1, 126));
    }

    /// <summary>
    /// Writes the multiple registers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteMultipleRegistersAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Master.WriteMultipleRegistersAsync(1, 1, null!));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.WriteMultipleRegistersAsync(1, 1, Array.Empty<ushort>()));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.WriteMultipleRegistersAsync(1, 1, Enumerable.Repeat<ushort>(1, 124).ToArray()));
    }

    /// <summary>
    /// Writes the multiple coils.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteMultipleCoilsAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Master.WriteMultipleCoilsAsync(1, 1, null!));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.WriteMultipleCoilsAsync(1, 1, Array.Empty<bool>()));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.WriteMultipleCoilsAsync(1, 1, Enumerable.Repeat(false, 1969).ToArray()));
    }

    /// <summary>
    /// Reads the write multiple registers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadWriteMultipleRegistersAsync()
    {
        // validate numberOfPointsToRead
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 0, 1, new ushort[] { 1 }));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 126, 1, new ushort[] { 1 }));

        // validate writeData
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 1, 1, null!));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 1, 1, Array.Empty<ushort>()));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 1, 1, Enumerable.Repeat<ushort>(1, 122).ToArray()));
    }
}
