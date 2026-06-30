// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using ModbusRx.Device;
using ModbusRx.IO;
using Moq;

namespace ModbusRx.UnitTests.Device;

/// <summary>Tests the ModbusMasterFixture behavior.</summary>
public class ModbusMasterFixture
{
    /// <summary>Gets the stream rsource.</summary>
    /// <value>
    /// The stream rsource.
    /// </value>
    private static IStreamResource StreamRsource => new Mock<IStreamResource>(MockBehavior.Strict).Object;

    /// <summary>Gets the master.</summary>
    /// <value>
    /// The master.
    /// </value>
    private static ModbusSerialMaster Master => ModbusSerialMaster.CreateRtu(StreamRsource);

    /// <summary>Reads the coils.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadCoils()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadCoilsAsync(1, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadCoilsAsync(1, 1, 2001));
    }

    /// <summary>Reads the inputs.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadInputs()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadInputsAsync(1, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadInputsAsync(1, 1, 2001));
    }

    /// <summary>Reads the holding registers.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadHoldingRegistersAsync()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadHoldingRegistersAsync(1, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadHoldingRegistersAsync(1, 1, 126));
    }

    /// <summary>Reads the input registers.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadInputRegistersAsync()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadInputRegistersAsync(1, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadInputRegistersAsync(1, 1, 126));
    }

    /// <summary>Writes the multiple registers.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task WriteMultipleRegistersAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Master.WriteMultipleRegistersAsync(1, 1, null!));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.WriteMultipleRegistersAsync(1, 1, []));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.WriteMultipleRegistersAsync(1, 1, CreateRegisters(124, 1)));
    }

    /// <summary>Writes the multiple coils.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task WriteMultipleCoilsAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Master.WriteMultipleCoilsAsync(1, 1, null!));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.WriteMultipleCoilsAsync(1, 1, []));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.WriteMultipleCoilsAsync(1, 1, CreateCoils(1969, false)));
    }

    /// <summary>Reads the write multiple registers.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadWriteMultipleRegistersAsync()
    {
        // validate numberOfPointsToRead
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 0, 1, [ 1]));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 126, 1, [ 1]));

        // validate writeData
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 1, 1, null!));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 1, 1, []));
        await Assert.ThrowsAsync<ArgumentException>(async () => await Master.ReadWriteMultipleRegistersAsync(1, 1, 1, 1, CreateRegisters(122, 1)));
    }

    /// <summary>Creates a register buffer filled with one value.</summary>
    /// <param name="count">The number of registers to create.</param>
    /// <param name="value">The register value.</param>
    /// <returns>The populated register buffer.</returns>
    private static ushort[] CreateRegisters(int count, ushort value)
    {
        var result = new ushort[count];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = value;
        }

        return result;
    }

    /// <summary>Creates a coil buffer filled with one value.</summary>
    /// <param name="count">The number of coils to create.</param>
    /// <param name="value">The coil value.</param>
    /// <returns>The populated coil buffer.</returns>
    private static bool[] CreateCoils(int count, bool value)
    {
        var result = new bool[count];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = value;
        }

        return result;
    }
}
