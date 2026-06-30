// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.IO;
using ModbusRx.Message;
using ModbusRx.Utility;
using Moq;

namespace ModbusRx.UnitTests.IO;

/// <summary>Tests the ModbusTransportFixture behavior.</summary>
public class ModbusTransportFixture
{
    /// <summary>Disposes the multiple times should not throw.</summary>
    [TUnit.Core.Test]
    public void Dispose_MultipleTimes_ShouldNotThrow()
    {
        var streamMock = new Mock<IStreamResource>(MockBehavior.Strict);
        _ = streamMock.Setup(s => s.Dispose());

        var mock = new Mock<ModbusTransport>(streamMock.Object) { CallBase = true };

        using var transport = mock.Object;
        _ = Assert.NotNull(transport.StreamResource);
        transport.Dispose();
        Assert.Null(transport.StreamResource);
    }

    /// <summary>Reads the write timeouts.</summary>
    [TUnit.Core.Test]
    public void ReadWriteTimeouts()
    {
        const int expectedReadTimeout = 42;
        const int expectedWriteTimeout = 33;
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);

        _ = mock.SetupProperty(s => s.ReadTimeout, expectedReadTimeout)
            .SetupProperty(s => s.WriteTimeout, expectedWriteTimeout);

        var transport = new Mock<ModbusTransport>(MockBehavior.Strict, mock.Object) { CallBase = true }.Object;

        Assert.Equal(expectedReadTimeout, transport.ReadTimeout);
        Assert.Equal(expectedWriteTimeout, transport.WriteTimeout);

        // Swapping
        transport.ReadTimeout = expectedWriteTimeout;
        transport.WriteTimeout = expectedReadTimeout;

        Assert.Equal(expectedWriteTimeout, transport.ReadTimeout);
        Assert.Equal(expectedReadTimeout, transport.WriteTimeout);

        mock.VerifyAll();
    }

    /// <summary>Waits to retry milliseconds.</summary>
    [TUnit.Core.Test]
    public void WaitToRetryMilliseconds()
    {
        var mock = new Mock<ModbusTransport>(MockBehavior.Strict) { CallBase = true };
        var transport = mock.Object;

        Assert.Equal(Modbus.DefaultWaitToRetryMilliseconds, transport.WaitToRetryMilliseconds);

        _ = Assert.Throws<ArgumentException>(() => transport.WaitToRetryMilliseconds = -1);

        const int expectedWaitToRetryMilliseconds = 42;
        transport.WaitToRetryMilliseconds = expectedWaitToRetryMilliseconds;
        Assert.Equal(expectedWaitToRetryMilliseconds, transport.WaitToRetryMilliseconds);
    }

    /// <summary>Unicasts the message.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage()
    {
        var data = new DiscreteCollection(true, false, true, false, false, false, false, false);
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()));
        _ = mock.Setup(t => t.ReadResponse<ReadCoilsInputsResponse>().Result)
            .Returns(new ReadCoilsInputsResponse(Modbus.ReadCoils, 2, 1, data));
        _ = mock.Setup(t => t.OnValidateResponse(It.IsNotNull<IModbusMessage>(), It.IsNotNull<IModbusMessage>()));

        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 3, 4);
        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadCoils, 2, 1, data);
        var response = transport.UnicastMessage<ReadCoilsInputsResponse>(request);

        Assert.Equal(expectedResponse.MessageFrame, response.MessageFrame);
        mock.VerifyAll();
    }

    /// <summary>Unicasts the message wrong response function code.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage_WrongResponseFunctionCode()
    {
        var request = new ReadCoilsInputsRequest(Modbus.ReadInputs, 2, 3, 4);
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;
        var writeCallsCount = 0;
        var readResponseCallsCount = 0;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>())).Callback(() => ++writeCallsCount);

        _ = mock.Setup(t => t.ReadResponse<ReadCoilsInputsResponse>().Result)
            .Returns(new ReadCoilsInputsResponse(Modbus.ReadCoils, 2, 0, new DiscreteCollection()))
            .Callback(() => ++readResponseCallsCount);

        _ = Assert.Throws<IOException>(() => transport.UnicastMessage<ReadCoilsInputsResponse>(request));
        Assert.Equal(Modbus.DefaultRetries + 1, writeCallsCount);
        Assert.Equal(Modbus.DefaultRetries + 1, readResponseCallsCount);

        mock.VerifyAll();
    }

    /// <summary>Unicasts the message error slave exception.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage_ErrorSlaveException()
    {
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var request = new ReadCoilsInputsRequest(Modbus.ReadInputs, 2, 3, 4);
        var transport = mock.Object;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()));
        _ = mock.Setup(t => t.ReadResponse<ReadCoilsInputsResponse>()).Throws<SlaveException>();

        _ = Assert.Throws<SlaveException>(() => transport.UnicastMessage<ReadCoilsInputsResponse>(request));

        mock.VerifyAll();
    }

    /// <summary>We should reread the response w/o retransmitting the request.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage_AcknowlegeSlaveException()
    {
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;
        var callsCount = 0;

        // Shorten retry waits so the test completes quickly.
        transport.WaitToRetryMilliseconds = 5;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()));

        // Configure repeated slave exceptions to verify retry handling.
        _ = mock.Setup(t => t.ReadResponse<ReadHoldingInputRegistersResponse>().Result)
            .Returns(() =>
            {
                if (callsCount < transport.Retries + 1)
                {
                    ++callsCount;
                    return new SlaveExceptionResponse(1, Modbus.ReadHoldingRegisters + Modbus.ExceptionOffset, Modbus.Acknowledge);
                }

                return new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1));
            });

        _ = mock.Setup(t => t.OnValidateResponse(It.IsNotNull<IModbusMessage>(), It.IsNotNull<IModbusMessage>()));

        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 1, 1);
        var expectedResponse = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1));
        var response = transport.UnicastMessage<ReadHoldingInputRegistersResponse>(request);

        Assert.Equal(transport.Retries + 1, callsCount);
        Assert.Equal(expectedResponse.MessageFrame, response.MessageFrame);
        mock.VerifyAll();
    }

    /// <summary>We should retransmit the request.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage_SlaveDeviceBusySlaveException()
    {
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;
        var writeCallsCount = 0;
        var readResponseCallsCount = 0;

        // Shorten retry waits so the test completes quickly.
        transport.WaitToRetryMilliseconds = 5;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()))
            .Callback(() => ++writeCallsCount);

        // Configure repeated slave exceptions to verify retry handling.
        _ = mock.Setup(t => t.ReadResponse<ReadHoldingInputRegistersResponse>().Result)
            .Returns(() => readResponseCallsCount == 0 ? new SlaveExceptionResponse(1, Modbus.ReadHoldingRegisters + Modbus.ExceptionOffset, Modbus.SlaveDeviceBusy) : new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1)))
            .Callback(() => ++readResponseCallsCount);

        _ = mock.Setup(t => t.OnValidateResponse(It.IsNotNull<IModbusMessage>(), It.IsNotNull<IModbusMessage>()));

        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 1, 1);
        var expectedResponse = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1));
        var response = transport.UnicastMessage<ReadHoldingInputRegistersResponse>(request);

        Assert.Equal(2, writeCallsCount);
        Assert.Equal(2, readResponseCallsCount);
        Assert.Equal(expectedResponse.MessageFrame, response.MessageFrame);

        mock.VerifyAll();
    }

    /// <summary>We should retransmit the request.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage_SlaveDeviceBusySlaveExceptionDoesNotFailAfterExceedingRetries()
    {
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;
        var writeCallsCount = 0;
        var readResponseCallsCount = 0;

        // Shorten retry waits so the test completes quickly.
        transport.WaitToRetryMilliseconds = 5;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()))
            .Callback(() => ++writeCallsCount);

        // Configure repeated slave exceptions to verify retry handling.
        _ = mock.Setup(t => t.ReadResponse<ReadHoldingInputRegistersResponse>().Result)
            .Returns(() => readResponseCallsCount < transport.Retries ? new SlaveExceptionResponse(1, Modbus.ReadHoldingRegisters + Modbus.ExceptionOffset, Modbus.SlaveDeviceBusy) : new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1)))
            .Callback(() => ++readResponseCallsCount);

        _ = mock.Setup(t => t.OnValidateResponse(It.IsNotNull<IModbusMessage>(), It.IsNotNull<IModbusMessage>()));

        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 1, 1);
        var expectedResponse = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1));
        var response = transport.UnicastMessage<ReadHoldingInputRegistersResponse>(request);

        Assert.Equal(transport.Retries + 1, writeCallsCount);
        Assert.Equal(transport.Retries + 1, readResponseCallsCount);
        Assert.Equal(expectedResponse.MessageFrame, response.MessageFrame);

        mock.VerifyAll();
    }

    /// <summary>Unicasts the message too many failing exceptions.</summary>
    /// <param name="exceptionType">Type of the exception.</param>
    [TUnit.Core.Test]
    [TUnit.Core.Arguments(typeof(TimeoutException))]
    [TUnit.Core.Arguments(typeof(IOException))]
    [TUnit.Core.Arguments(typeof(NotSupportedException))]
    [TUnit.Core.Arguments(typeof(FormatException))]
    public void UnicastMessage_TooManyFailingExceptions(Type exceptionType)
    {
        if (exceptionType is null)
        {
            throw new ArgumentNullException(nameof(exceptionType));
        }

        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;
        var writeCallsCount = 0;
        var readResponseCallsCount = 0;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()))
            .Callback(() => ++writeCallsCount);

        _ = mock.Setup(t => t.ReadResponse<ReadCoilsInputsResponse>())
            .Callback(() => ++readResponseCallsCount)
            .Throws((Exception)Activator.CreateInstance(exceptionType)!);

        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 3, 4);

        _ = Assert.Throws(exceptionType, () => transport.UnicastMessage<ReadCoilsInputsResponse>(request));
        Assert.Equal(transport.Retries + 1, writeCallsCount);
        Assert.Equal(transport.Retries + 1, readResponseCallsCount);
        mock.VerifyAll();
    }

    /// <summary>Unicasts the message with a non-retryable exception.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage_NonRetryableExceptionDoesNotRetry()
    {
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;
        var writeCallsCount = 0;
        var readResponseCallsCount = 0;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()))
            .Callback(() => ++writeCallsCount);

        _ = mock.Setup(t => t.ReadResponse<ReadCoilsInputsResponse>())
            .Callback(() => ++readResponseCallsCount)
            .Throws<NotImplementedException>();

        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 3, 4);

        _ = Assert.Throws<NotImplementedException>(() => transport.UnicastMessage<ReadCoilsInputsResponse>(request));
        Assert.Equal(1, writeCallsCount);
        Assert.Equal(1, readResponseCallsCount);
        mock.VerifyAll();
    }

    /// <summary>Unicasts the message timeout exception.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage_TimeoutException()
    {
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;
        var writeCallsCount = 0;
        var readResponseCallsCount = 0;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()))
            .Callback(() => ++writeCallsCount);

        _ = mock.Setup(t => t.ReadResponse<ReadCoilsInputsResponse>())
            .Callback(() => ++readResponseCallsCount)
            .Throws<TimeoutException>();

        var request = new ReadCoilsInputsRequest(Modbus.ReadInputs, 2, 3, 4);
        _ = Assert.Throws<TimeoutException>(() => transport.UnicastMessage<ReadCoilsInputsResponse>(request));
        Assert.Equal(Modbus.DefaultRetries + 1, writeCallsCount);
        Assert.Equal(Modbus.DefaultRetries + 1, readResponseCallsCount);
        mock.VerifyAll();
    }

    /// <summary>Unicasts the message retries.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage_Retries()
    {
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;
        var writeCallsCount = 0;
        var readResponseCallsCount = 0;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()))
            .Callback(() => ++writeCallsCount);

        transport.Retries = 5;
        _ = mock.Setup(t => t.ReadResponse<ReadCoilsInputsResponse>())
            .Callback(() => ++readResponseCallsCount)
            .Throws<TimeoutException>();

        var request = new ReadCoilsInputsRequest(Modbus.ReadInputs, 2, 3, 4);

        _ = Assert.Throws<TimeoutException>(() => transport.UnicastMessage<ReadCoilsInputsResponse>(request));
        Assert.Equal(transport.Retries + 1, writeCallsCount);
        Assert.Equal(transport.Retries + 1, readResponseCallsCount);
        mock.VerifyAll();
    }

    /// <summary>Unicasts the message re reads if should retry return true.</summary>
    [TUnit.Core.Test]
    public void UnicastMessage_ReReads_IfShouldRetryReturnTrue()
    {
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;
        var expectedResponse = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1));
        var readResponseCallsCount = 0;
        var onShouldRetryResponseCallsCount = 0;
        bool[] expectedReturn = { true, false };

        transport.RetryOnOldResponseThreshold = 3;

        _ = mock.Setup(t => t.Write(It.IsNotNull<IModbusMessage>()));

        _ = mock.Setup(t => t.OnValidateResponse(It.IsNotNull<IModbusMessage>(), It.IsNotNull<IModbusMessage>()));

        _ = mock.Setup(t => t.OnShouldRetryResponse(It.IsNotNull<IModbusMessage>(), It.IsNotNull<IModbusMessage>()))
            .Returns(() => expectedReturn[onShouldRetryResponseCallsCount++]);

        _ = mock.Setup(t => t.ReadResponse<ReadHoldingInputRegistersResponse>().Result)
            .Returns(expectedResponse)
            .Callback(() => ++readResponseCallsCount);

        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 1, 1);
        var response = transport.UnicastMessage<ReadHoldingInputRegistersResponse>(request);

        Assert.Equal(2, readResponseCallsCount);
        Assert.Equal(2, onShouldRetryResponseCallsCount);
        Assert.Equal(expectedResponse.MessageFrame, response.MessageFrame);
        mock.VerifyAll();
    }

    /// <summary>Creates the response slave exception.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task CreateResponse_SlaveExceptionAsync()
    {
        var mock = new Mock<ModbusTransport>() { CallBase = true };
        var transport = mock.Object;

        byte[] frame = { 2, 129, 2 };
        var lrc = ModbusUtility.CalculateLrc(frame);
        var responseFrame = new byte[frame.Length + 1];
        Array.Copy(frame, responseFrame, frame.Length);
        responseFrame[frame.Length] = lrc;
        var message = await transport.CreateResponse<ReadCoilsInputsResponse>(Task.FromResult(responseFrame));
        _ = Assert.IsType<SlaveExceptionResponse>(message);
    }

    /// <summary>Shoulds the retry response returns false if different message.</summary>
    [TUnit.Core.Test]
    public void ShouldRetryResponse_ReturnsFalse_IfDifferentMessage()
    {
        var mock = new Mock<ModbusTransport>(MockBehavior.Strict) { CallBase = true };
        var transport = mock.Object;

        IModbusMessage request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 1, 1);
        IModbusMessage response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, null!);

        Assert.False(transport.ShouldRetryResponse(request, response));
    }

    /// <summary>Validates the response mismatching function codes.</summary>
    [TUnit.Core.Test]
    public void ValidateResponse_MismatchingFunctionCodes()
    {
        var mock = new Mock<ModbusTransport>(MockBehavior.Strict) { CallBase = true };
        var transport = mock.Object;

        IModbusMessage request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 1);
        IModbusMessage response = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection());

        _ = Assert.Throws<IOException>(() => transport.ValidateResponse(request, response));
    }

    /// <summary>Validates the response mismatching slave address.</summary>
    [TUnit.Core.Test]
    public void ValidateResponse_MismatchingSlaveAddress()
    {
        var mock = new Mock<ModbusTransport>(MockBehavior.Strict) { CallBase = true };
        var transport = mock.Object;

        IModbusMessage request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 42, 1, 1);
        IModbusMessage response = new ReadHoldingInputRegistersResponse(Modbus.ReadCoils, 33, new RegisterCollection());

        _ = Assert.Throws<IOException>(() => transport.ValidateResponse(request, response));
    }

    /// <summary>Validates the response calls on validate response.</summary>
    [TUnit.Core.Test]
    public void ValidateResponse_CallsOnValidateResponse()
    {
        var mock = new Mock<ModbusTransport>(MockBehavior.Strict) { CallBase = true };
        var transport = mock.Object;

        _ = mock.Setup(t => t.OnValidateResponse(It.IsNotNull<IModbusMessage>(), It.IsNotNull<IModbusMessage>()));

        IModbusMessage request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 1);
        IModbusMessage response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, new DiscreteCollection());

        transport.ValidateResponse(request, response);
        mock.VerifyAll();
    }
}
