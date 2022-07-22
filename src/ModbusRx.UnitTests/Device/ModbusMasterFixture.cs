// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
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
    [Fact]
    public void ReadCoils()
    {
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadCoils(1, 1, 0));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadCoils(1, 1, 2001));
    }

    /// <summary>
    /// Reads the inputs.
    /// </summary>
    [Fact]
    public void ReadInputs()
    {
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadInputs(1, 1, 0));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadInputs(1, 1, 2001));
    }

    /// <summary>
    /// Reads the holding registers.
    /// </summary>
    [Fact]
    public void ReadHoldingRegisters()
    {
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadHoldingRegisters(1, 1, 0));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadHoldingRegisters(1, 1, 126));
    }

    /// <summary>
    /// Reads the input registers.
    /// </summary>
    [Fact]
    public void ReadInputRegisters()
    {
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadInputRegisters(1, 1, 0));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadInputRegisters(1, 1, 126));
    }

    /// <summary>
    /// Writes the multiple registers.
    /// </summary>
    [Fact]
    public void WriteMultipleRegisters()
    {
        Assert.Throws<ArgumentNullException>(() => ModbusMasterFixture.Master.WriteMultipleRegisters(1, 1, null!));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.WriteMultipleRegisters(1, 1, Array.Empty<ushort>()));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.WriteMultipleRegisters(1, 1, Enumerable.Repeat<ushort>(1, 124).ToArray()));
    }

    /// <summary>
    /// Writes the multiple coils.
    /// </summary>
    [Fact]
    public void WriteMultipleCoils()
    {
        Assert.Throws<ArgumentNullException>(() => ModbusMasterFixture.Master.WriteMultipleCoils(1, 1, null!));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.WriteMultipleCoils(1, 1, Array.Empty<bool>()));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.WriteMultipleCoils(1, 1, Enumerable.Repeat(false, 1969).ToArray()));
    }

    /// <summary>
    /// Reads the write multiple registers.
    /// </summary>
    [Fact]
    public void ReadWriteMultipleRegisters()
    {
        // validate numberOfPointsToRead
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadWriteMultipleRegisters(1, 1, 0, 1, new ushort[] { 1 }));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadWriteMultipleRegisters(1, 1, 126, 1, new ushort[] { 1 }));

        // validate writeData
        Assert.Throws<ArgumentNullException>(() => ModbusMasterFixture.Master.ReadWriteMultipleRegisters(1, 1, 1, 1, null!));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadWriteMultipleRegisters(1, 1, 1, 1, Array.Empty<ushort>()));
        Assert.Throws<ArgumentException>(() => ModbusMasterFixture.Master.ReadWriteMultipleRegisters(1, 1, 1, 1, Enumerable.Repeat<ushort>(1, 122).ToArray()));
    }
}
