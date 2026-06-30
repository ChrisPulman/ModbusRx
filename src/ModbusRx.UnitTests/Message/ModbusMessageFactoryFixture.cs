// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using ModbusRx.Data;
using ModbusRx.Message;

namespace ModbusRx.UnitTests.Message;

/// <summary>Tests the ModbusMessageFactoryFixture behavior.</summary>
public class ModbusMessageFactoryFixture
{
    /// <summary>Creates the modbus message read coils request.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadCoilsRequest()
    {
        var request =
            ModbusMessageFactory.CreateModbusMessage<ReadCoilsInputsRequest>([
                11, Modbus.ReadCoils, 0, 19, 0, 37,
            ]);

        var expectedRequest = new ReadCoilsInputsRequest(Modbus.ReadCoils, 11, 19, 37);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(request, expectedRequest);
        Assert.Equal(expectedRequest.StartAddress, request.StartAddress);
        Assert.Equal(expectedRequest.NumberOfPoints, request.NumberOfPoints);
    }

    /// <summary>Creates the size of the modbus message read coils request with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadCoilsRequestWithInvalidFrameSize()
    {
        byte[] frame = { 11, Modbus.ReadCoils, 4, 1, 2 };
        _ = Assert.Throws<FormatException>(() => ModbusMessageFactory.CreateModbusMessage<ReadCoilsInputsRequest>(frame));
    }

    /// <summary>Creates the modbus message read coils response.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadCoilsResponse()
    {
        var response =
            ModbusMessageFactory.CreateModbusMessage<ReadCoilsInputsResponse>([
                11, Modbus.ReadCoils, 1, 1,
            ]);

        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadCoils, 11, 1, new DiscreteCollection(true, false, false, false));

        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);

        Assert.Equal(expectedResponse.Data.NetworkBytes, response.Data.NetworkBytes);
    }

    /// <summary>Creates the modbus message read coils response with no byte count.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadCoilsResponseWithNoByteCount()
    {
        byte[] frame = { 11, Modbus.ReadCoils };
        _ = Assert.Throws<FormatException>(() => ModbusMessageFactory.CreateModbusMessage<ReadCoilsInputsResponse>(frame));
    }

    /// <summary>Creates the size of the modbus message read coils response with invalid data.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadCoilsResponseWithInvalidDataSize()
    {
        byte[] frame = { 11, Modbus.ReadCoils, 4, 1, 2, 3 };
        _ = Assert.Throws<FormatException>(() => ModbusMessageFactory.CreateModbusMessage<ReadCoilsInputsResponse>(frame));
    }

    /// <summary>Creates the modbus message read holding registers request.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadHoldingRegistersRequest()
    {
        var request =
            ModbusMessageFactory.CreateModbusMessage<ReadHoldingInputRegistersRequest>([
                17, Modbus.ReadHoldingRegisters, 0, 107, 0, 3,
            ]);

        var expectedRequest = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 17, 107, 3);

        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedRequest, request);

        Assert.Equal(expectedRequest.StartAddress, request.StartAddress);
        Assert.Equal(expectedRequest.NumberOfPoints, request.NumberOfPoints);
    }

    /// <summary>Creates the size of the modbus message read holding registers request with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadHoldingRegistersRequestWithInvalidFrameSize() =>
        Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<ReadHoldingInputRegistersRequest>(
                [ 11, Modbus.ReadHoldingRegisters, 0, 0, 5]));

    /// <summary>Creates the modbus message read holding registers response.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadHoldingRegistersResponse()
    {
        var response =
            ModbusMessageFactory.CreateModbusMessage<ReadHoldingInputRegistersResponse>([
                11, Modbus.ReadHoldingRegisters, 4, 0, 3, 0, 4,
            ]);

        var expectedResponse = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 11, new RegisterCollection(3, 4));

        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
    }

    /// <summary>Creates the size of the modbus message read holding registers response with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadHoldingRegistersResponseWithInvalidFrameSize() =>
        Assert.Throws<FormatException>(() => ModbusMessageFactory.CreateModbusMessage<ReadHoldingInputRegistersResponse>([ 11, Modbus.ReadHoldingRegisters]));

    /// <summary>Creates the modbus message slave exception response.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageSlaveExceptionResponse()
    {
        var response =
            ModbusMessageFactory.CreateModbusMessage<SlaveExceptionResponse>([ 11, 129, 2]);

        var expectedException = new SlaveExceptionResponse(11, Modbus.ReadCoils + Modbus.ExceptionOffset, 2);

        Assert.Equal(expectedException.FunctionCode, response.FunctionCode);
        Assert.Equal(expectedException.SlaveAddress, response.SlaveAddress);
        Assert.Equal(expectedException.MessageFrame, response.MessageFrame);
        Assert.Equal(expectedException.ProtocolDataUnit, response.ProtocolDataUnit);
    }

    /// <summary>Creates the modbus message slave exception response with invalid function code.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageSlaveExceptionResponseWithInvalidFunctionCode() =>
        Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<SlaveExceptionResponse>([ 11, 128, 2]));

    /// <summary>Creates the size of the modbus message slave exception response with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageSlaveExceptionResponseWithInvalidFrameSize() =>
        Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<SlaveExceptionResponse>([ 11, 128]));

    /// <summary>Creates the modbus message write single coil request response.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteSingleCoilRequestResponse()
    {
        var request =
            ModbusMessageFactory.CreateModbusMessage<WriteSingleCoilRequestResponse>([
                17, Modbus.WriteSingleCoil, 0, 172, byte.MaxValue, 0,
            ]);

        var expectedRequest = new WriteSingleCoilRequestResponse(17, 172, true);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedRequest, request);

        Assert.Equal(expectedRequest.StartAddress, request.StartAddress);
        Assert.Equal(expectedRequest.Data.NetworkBytes, request.Data.NetworkBytes);
    }

    /// <summary>Creates the size of the modbus message write single coil request response with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteSingleCoilRequestResponseWithInvalidFrameSize() =>
        Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<WriteSingleCoilRequestResponse>([ 11, Modbus.WriteSingleCoil, 0, 105, byte.MaxValue]));

    /// <summary>Creates the modbus message write single register request response.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteSingleRegisterRequestResponse()
    {
        var request =
            ModbusMessageFactory.CreateModbusMessage<WriteSingleRegisterRequestResponse>([
                17, Modbus.WriteSingleRegister, 0, 1, 0, 3,
            ]);

        var expectedRequest = new WriteSingleRegisterRequestResponse(17, 1, 3);

        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedRequest, request);

        Assert.Equal(expectedRequest.StartAddress, request.StartAddress);
        Assert.Equal(expectedRequest.Data.NetworkBytes, request.Data.NetworkBytes);
    }

    /// <summary>Creates the size of the modbus message write single register request response with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteSingleRegisterRequestResponseWithInvalidFrameSize() =>
        Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<WriteSingleRegisterRequestResponse>([ 11, Modbus.WriteSingleRegister, 0, 1, 0]));

    /// <summary>Creates the modbus message write multiple registers request.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteMultipleRegistersRequest()
    {
        var request =
            ModbusMessageFactory.CreateModbusMessage<WriteMultipleRegistersRequest>([
                11, Modbus.WriteMultipleRegisters, 0, 5, 0, 1, 2, 255, 255,
            ]);

        var expectedRequest = new WriteMultipleRegistersRequest(11, 5, new RegisterCollection(ushort.MaxValue));

        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedRequest, request);

        Assert.Equal(expectedRequest.StartAddress, request.StartAddress);
        Assert.Equal(expectedRequest.NumberOfPoints, request.NumberOfPoints);
        Assert.Equal(expectedRequest.ByteCount, request.ByteCount);
        Assert.Equal(expectedRequest.Data.NetworkBytes, request.Data.NetworkBytes);
    }

    /// <summary>Creates the size of the modbus message write multiple registers request with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteMultipleRegistersRequestWithInvalidFrameSize() =>
        Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<WriteMultipleRegistersRequest>([ 11, Modbus.WriteMultipleRegisters, 0, 5, 0, 1, 2]));

    /// <summary>Creates the modbus message write multiple registers response.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteMultipleRegistersResponse()
    {
        var response =
            ModbusMessageFactory.CreateModbusMessage<WriteMultipleRegistersResponse>([
                17, Modbus.WriteMultipleRegisters, 0, 1, 0, 2,
            ]);
        var expectedResponse = new WriteMultipleRegistersResponse(17, 1, 2);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);

        Assert.Equal(expectedResponse.StartAddress, response.StartAddress);
        Assert.Equal(expectedResponse.NumberOfPoints, response.NumberOfPoints);
    }

    /// <summary>Creates the modbus message write multiple coils request.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteMultipleCoilsRequest()
    {
        var request =
            ModbusMessageFactory.CreateModbusMessage<WriteMultipleCoilsRequest>([
                17, Modbus.WriteMultipleCoils, 0, 19, 0, 10, 2, 205, 1,
            ]);

        var expectedRequest = new WriteMultipleCoilsRequest(17, 19, new DiscreteCollection(true, false, true, true, false, false, true, true, true, false));

        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedRequest, request);

        Assert.Equal(expectedRequest.StartAddress, request.StartAddress);
        Assert.Equal(expectedRequest.NumberOfPoints, request.NumberOfPoints);
        Assert.Equal(expectedRequest.ByteCount, request.ByteCount);
        Assert.Equal(expectedRequest.Data.NetworkBytes, request.Data.NetworkBytes);
    }

    /// <summary>Creates the size of the modbus message write multiple coils request with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteMultipleCoilsRequestWithInvalidFrameSize() =>
        Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<WriteMultipleCoilsRequest>([ 17, Modbus.WriteMultipleCoils, 0, 19, 0, 10, 2]));

    /// <summary>Creates the modbus message write multiple coils response.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteMultipleCoilsResponse()
    {
        var response =
            ModbusMessageFactory.CreateModbusMessage<WriteMultipleCoilsResponse>([
                17, Modbus.WriteMultipleCoils, 0, 19, 0, 10,
            ]);
        var expectedResponse = new WriteMultipleCoilsResponse(17, 19, 10);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);

        Assert.Equal(expectedResponse.StartAddress, response.StartAddress);
        Assert.Equal(expectedResponse.NumberOfPoints, response.NumberOfPoints);
    }

    /// <summary>Creates the size of the modbus message write multiple coils response with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageWriteMultipleCoilsResponseWithInvalidFrameSize() =>
        Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<WriteMultipleCoilsResponse>([ 17, Modbus.WriteMultipleCoils, 0, 19, 0]));

    /// <summary>Creates the modbus message read write multiple registers request.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadWriteMultipleRegistersRequest()
    {
        var request =
            ModbusMessageFactory.CreateModbusMessage<ReadWriteMultipleRegistersRequest>([
                0x05, 0x17, 0x00, 0x03, 0x00, 0x06, 0x00, 0x0e, 0x00, 0x03, 0x06, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            ]);
        var writeCollection = new RegisterCollection(255, 255, 255);
        var expectedRequest = new ReadWriteMultipleRegistersRequest(5, 3, 6, 14, writeCollection);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedRequest, request);
    }

    /// <summary>Creates the size of the modbus message read write multiple registers request with invalid frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReadWriteMultipleRegistersRequestWithInvalidFrameSize()
    {
        byte[] frame = { 17, Modbus.ReadWriteMultipleRegisters, 1, 2, 3 };
        _ = Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<ReadWriteMultipleRegistersRequest>(frame));
    }

    /// <summary>Creates the modbus message return query data request response.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReturnQueryDataRequestResponse()
    {
        const byte slaveAddress = 5;
        var data = new RegisterCollection(50);
        var networkBytes = data.NetworkBytes;
        var frame = new byte[networkBytes.Length + 4];
        frame[0] = slaveAddress;
        frame[1] = 8;
        frame[2] = 0;
        frame[3] = 0;
        Array.Copy(networkBytes, 0, frame, 4, networkBytes.Length);
        var message =
            ModbusMessageFactory.CreateModbusMessage<DiagnosticsRequestResponse>(frame);
        var expectedMessage = new DiagnosticsRequestResponse(Modbus.DiagnosticsReturnQueryData, slaveAddress, data);

        Assert.Equal(expectedMessage.SubFunctionCode, message.SubFunctionCode);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedMessage, message);
    }

    /// <summary>Creates the modbus message return query data request response too small.</summary>
    [TUnit.Core.Test]
    public void CreateModbusMessageReturnQueryDataRequestResponseTooSmall()
    {
        var frame = new byte[] { 5, 8, 0, 0, 5 };
        _ = Assert.Throws<FormatException>(() =>
            ModbusMessageFactory.CreateModbusMessage<DiagnosticsRequestResponse>(frame));
    }

    /// <summary>Creates the modbus request with invalid message frame.</summary>
    [TUnit.Core.Test]
    public void CreateModbusRequestWithInvalidMessageFrame() =>
        Assert.Throws<FormatException>(() => ModbusMessageFactory.CreateModbusRequest([ 0, 1]));

    /// <summary>Creates the modbus request with invalid function code.</summary>
    [TUnit.Core.Test]
    public void CreateModbusRequestWithInvalidFunctionCode() =>
        Assert.Throws<ArgumentException>(() => ModbusMessageFactory.CreateModbusRequest([ 1, 99, 0, 0, 0, 1, 23]));

    /// <summary>Creates the modbus request for read coils.</summary>
    [TUnit.Core.Test]
    public void CreateModbusRequestForReadCoils()
    {
        var req = new ReadCoilsInputsRequest(1, 2, 1, 10);
        var request = ModbusMessageFactory.CreateModbusRequest(req.MessageFrame);
        Assert.Equal(typeof(ReadCoilsInputsRequest), request.GetType());
    }

    /// <summary>Creates the modbus request for diagnostics.</summary>
    [TUnit.Core.Test]
    public void CreateModbusRequestForDiagnostics()
    {
        var diagnosticsRequest = new DiagnosticsRequestResponse(0, 2, new RegisterCollection(45));
        var request = ModbusMessageFactory.CreateModbusRequest(diagnosticsRequest.MessageFrame);
        Assert.Equal(typeof(DiagnosticsRequestResponse), request.GetType());
    }
}
