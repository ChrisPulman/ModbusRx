// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using ModbusRx.Message;
using ModbusRx.Utility;
using Xunit;

namespace ModbusRx.UnitTests.Utility;

/// <summary>
/// ModbusUtilityFixture.
/// </summary>
public class ModbusUtilityFixture
{
    /// <summary>
    /// Gets the ASCII bytes from empty.
    /// </summary>
    [Fact]
    public void GetAsciiBytesFromEmpty()
    {
        Assert.Equal(Array.Empty<byte>(), ModbusUtility.GetAsciiBytes(Array.Empty<byte>()));
        Assert.Equal(Array.Empty<byte>(), ModbusUtility.GetAsciiBytes(Array.Empty<ushort>()));
    }

    /// <summary>
    /// Gets the ASCII bytes from bytes.
    /// </summary>
    [Fact]
    public void GetAsciiBytesFromBytes()
    {
        byte[] buf = { 2, 5 };
        byte[] expectedResult = { 48, 50, 48, 53 };
        var result = ModbusUtility.GetAsciiBytes(buf);
        Assert.Equal(expectedResult, result);
    }

    /// <summary>
    /// Gets the ASCII bytes from ushorts.
    /// </summary>
    [Fact]
    public void GetAsciiBytesFromUshorts()
    {
        ushort[] buf = { 300, 400 };
        byte[] expectedResult = { 48, 49, 50, 67, 48, 49, 57, 48 };
        var result = ModbusUtility.GetAsciiBytes(buf);
        Assert.Equal(expectedResult, result);
    }

    /// <summary>
    /// Hexadecimals to bytes.
    /// </summary>
    [Fact]
    public void HexToBytes() => Assert.Equal(new byte[] { 255 }, ModbusUtility.HexToBytes("FF"));

    /// <summary>
    /// Hexadecimals to bytes2.
    /// </summary>
    [Fact]
    public void HexToBytes2() => Assert.Equal(new byte[] { 204, 255 }, ModbusUtility.HexToBytes("CCFF"));

    /// <summary>
    /// Hexadecimals to bytes empty.
    /// </summary>
    [Fact]
    public void HexToBytesEmpty() => Assert.Equal(Array.Empty<byte>(), ModbusUtility.HexToBytes(string.Empty));

    /// <summary>
    /// Hexadecimals to bytes null.
    /// </summary>
    [Fact]
    public void HexToBytesNull() => Assert.Throws<ArgumentNullException>(() => ModbusUtility.HexToBytes(null!));

    /// <summary>
    /// Hexadecimals to bytes odd.
    /// </summary>
    [Fact]
    public void HexToBytesOdd() => Assert.Throws<FormatException>(() => ModbusUtility.HexToBytes("CCF"));

    /// <summary>
    /// Calculates the CRC.
    /// </summary>
    [Fact]
    public void CalculateCrc()
    {
        var result = ModbusUtility.CalculateCrc(new byte[] { 1, 1 });
        Assert.Equal(new byte[] { 193, 224 }, result);
    }

    /// <summary>
    /// Calculates the CRC2.
    /// </summary>
    [Fact]
    public void CalculateCrc2()
    {
        var result = ModbusUtility.CalculateCrc(new byte[] { 2, 1, 5, 0 });
        Assert.Equal(new byte[] { 83, 12 }, result);
    }

    /// <summary>
    /// Calculates the CRC empty.
    /// </summary>
    [Fact]
    public void CalculateCrcEmpty() => Assert.Equal(new byte[] { 255, 255 }, ModbusUtility.CalculateCrc(Array.Empty<byte>()));

    /// <summary>
    /// Calculates the CRC null.
    /// </summary>
    [Fact]
    public void CalculateCrcNull() => Assert.Throws<ArgumentNullException>(() => ModbusUtility.CalculateCrc(null!));

    /// <summary>
    /// Calculates the LRC.
    /// </summary>
    [Fact]
    public void CalculateLrc()
    {
        var a = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 10);
        Assert.Equal(243, ModbusUtility.CalculateLrc(new byte[] { 1, 1, 0, 1, 0, 10 }));
    }

    /// <summary>
    /// Calculates the LRC2.
    /// </summary>
    [Fact]
    public void CalculateLrc2()
    {
        // : 02 01 0000 0001 FC
        var a = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 0, 1);
        Assert.Equal(252, ModbusUtility.CalculateLrc(new byte[] { 2, 1, 0, 0, 0, 1 }));
    }

    /// <summary>
    /// Calculates the LRC null.
    /// </summary>
    [Fact]
    public void CalculateLrcNull() => Assert.Throws<ArgumentNullException>(() => ModbusUtility.CalculateLrc(null!));

    /// <summary>
    /// Calculates the LRC empty.
    /// </summary>
    [Fact]
    public void CalculateLrcEmpty() => Assert.Equal(0, ModbusUtility.CalculateLrc(Array.Empty<byte>()));

    /// <summary>
    /// Networks the bytes to host u int16.
    /// </summary>
    [Fact]
    public void NetworkBytesToHostUInt16() => Assert.Equal(new ushort[] { 1, 2 }, ModbusUtility.NetworkBytesToHostUInt16(new byte[] { 0, 1, 0, 2 }));

    /// <summary>
    /// Networks the bytes to host u int16 null.
    /// </summary>
    [Fact]
    public void NetworkBytesToHostUInt16Null() => Assert.Throws<ArgumentNullException>(() => ModbusUtility.NetworkBytesToHostUInt16(null!));

    /// <summary>
    /// Networks the bytes to host u int16 odd number of bytes.
    /// </summary>
    [Fact]
    public void NetworkBytesToHostUInt16OddNumberOfBytes() => Assert.Throws<FormatException>(() => ModbusUtility.NetworkBytesToHostUInt16(new byte[] { 1 }));

    /// <summary>
    /// Networks the bytes to host u int16 empty bytes.
    /// </summary>
    [Fact]
    public void NetworkBytesToHostUInt16EmptyBytes() => Assert.Equal(Array.Empty<ushort>(), ModbusUtility.NetworkBytesToHostUInt16(Array.Empty<byte>()));

    /// <summary>
    /// Gets the double.
    /// </summary>
    [Fact]
    public void GetDouble()
    {
        Assert.Equal(0.0, ModbusUtility.GetDouble(0, 0, 0, 0));
        Assert.Equal(1.0, ModbusUtility.GetDouble(16368, 0, 0, 0));
        Assert.Equal(Math.PI, ModbusUtility.GetDouble(16393, 8699, 21572, 11544));
        Assert.Equal(500.625, ModbusUtility.GetDouble(16511, 18944, 0, 0));
    }

    /// <summary>
    /// Gets the single.
    /// </summary>
    [Fact]
    public void GetSingle()
    {
        Assert.Equal(0F, ModbusUtility.GetSingle(0, 0));
        Assert.Equal(1F, ModbusUtility.GetSingle(16256, 0));
        Assert.Equal(9999999F, ModbusUtility.GetSingle(19224, 38527));
        Assert.Equal(500.625F, ModbusUtility.GetSingle(17402, 20480));
    }

    /// <summary>
    /// Gets the u int32.
    /// </summary>
    [Fact]
    public void GetUInt32()
    {
        Assert.Equal(0U, ModbusUtility.GetUInt32(0, 0));
        Assert.Equal(1U, ModbusUtility.GetUInt32(0, 1));
        Assert.Equal(45U, ModbusUtility.GetUInt32(0, 45));
        Assert.Equal(65536U, ModbusUtility.GetUInt32(1, 0));
    }
}
