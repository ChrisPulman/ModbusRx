// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Message;
#else
using ModbusRx.Message;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Unme.Common;
#else
using ModbusRx.Unme.Common;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.IO;
#else
namespace ModbusRx.IO;
#endif

/// <summary>Modbus transport. Abstraction - http://en.wikipedia.org/wiki/Bridge_Pattern.</summary>
public abstract class ModbusTransport : IDisposable
{
    /// <summary>Stores the sync Lock value.</summary>
    private readonly Lock _syncLock = new();

    /// <summary>Stores the wait To Retry Milliseconds value.</summary>
    private int _waitToRetryMilliseconds = Modbus.DefaultWaitToRetryMilliseconds;

    /// <summary>Stores the stream Resource value.</summary>
    private IStreamResource? _streamResource;

    /// <summary>Initializes a new instance of the <see cref="ModbusTransport"/> class. This constructor is called by the NullTransport.</summary>
    internal ModbusTransport()
    {
    }

    /// <summary>Initializes a new instance of the Modbus Transport class.</summary>
    /// <param name="streamResource">The stream Resource value.</param>
    internal ModbusTransport(IStreamResource streamResource)
    {
        Debug.Assert(streamResource is not null, "Argument streamResource cannot be null.");

        _streamResource = streamResource!;
    }

/// <summary>
/// Gets or sets number of times to retry sending message after encountering a failure such as an IOException,
/// TimeoutException, or a corrupt message.
/// </summary>
    public int Retries { get; set; } = Modbus.DefaultRetries;

/// <summary>
/// Gets or sets if non-zero, this will cause a second reply to be read if the first is behind the sequence number of the
/// request by less than this number.  For example, set this to 3, and if when sending request 5, response 3 is
/// read, we will attempt to re-read responses.
/// </summary>
    public uint RetryOnOldResponseThreshold { get; set; }

/// <summary>
/// Gets or sets a value indicating whether if set, Slave Busy exception causes retry count to be used.  If false, Slave Busy will cause infinite retries.
/// </summary>
    public bool SlaveBusyUsesRetryCount { get; set; }

/// <summary>
/// Gets or sets the number of milliseconds the tranport will wait before retrying a message after receiving
/// an ACKNOWLEGE or SLAVE DEVICE BUSY slave exception response.
/// </summary>
    public int WaitToRetryMilliseconds
    {
        get => _waitToRetryMilliseconds;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException(Resources.WaitRetryGreaterThanZero);
            }

            _waitToRetryMilliseconds = value;
        }
    }

    /// <summary>Gets or sets the number of milliseconds before a timeout occurs when a read operation does not finish.</summary>
    public int ReadTimeout
    {
        get => StreamResource.ReadTimeout;
        set => StreamResource.ReadTimeout = value;
    }

    /// <summary>Gets or sets the number of milliseconds before a timeout occurs when a write operation does not finish.</summary>
    public int WriteTimeout
    {
        get => StreamResource.WriteTimeout;
        set => StreamResource.WriteTimeout = value;
    }

    /// <summary>Gets the stream resource.</summary>
    internal IStreamResource StreamResource => _streamResource!;

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Executes the Unicast Message operation.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="message">The message value.</param>
    /// <returns>The result.</returns>
    internal virtual T UnicastMessage<T>(IModbusMessage message)
        where T : IModbusMessage, new()
    {
        var attempt = 1;

        while (true)
        {
            try
            {
                var response = WriteAndReadResponse<T>(message);
                ValidateResponse(message, response);
                return (T)response;
            }
            catch (SlaveException se) when (se.SlaveExceptionCode == Modbus.SlaveDeviceBusy)
            {
                if (SlaveBusyUsesRetryCount && attempt > Retries)
                {
                    throw;
                }

                attempt = RetryAfterSlaveBusy(attempt);
            }
            catch (Exception e) when (IsRetryableException(e))
            {
                var actualException = e is AggregateException ae ? ae.GetBaseException() : e;
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {actualException.GetType().Name}, {Retries - attempt + 1} retries remaining - {actualException}");

                if (attempt > Retries)
                {
                    throw;
                }

                attempt++;
            }

            static bool IsRetryableException(Exception exception)
            {
                var actualException = exception is AggregateException aggregateException
                    ? aggregateException.GetBaseException()
                    : exception;

                return actualException is FormatException or
                    NotSupportedException or
                    TimeoutException or
                    IOException;
            }
        }
    }

    /// <summary>Creates a response message from a frame.</summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="frame">The frame task.</param>
    /// <returns>The parsed response message.</returns>
    internal virtual async Task<IModbusMessage> CreateResponseMessage<T>(Task<byte[]> frame)
        where T : IModbusMessage, new()
    {
        var lframe = await frame;
        var functionCode = lframe[1];

        // check for slave exception response else create message from frame
        return functionCode > Modbus.ExceptionOffset ? ModbusMessageFactory.CreateModbusMessage<SlaveExceptionResponse>(lframe) : ModbusMessageFactory.CreateModbusMessage<T>(lframe);
    }

    /// <summary>Executes the Create Response operation.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="frame">The frame value.</param>
    /// <returns>The result.</returns>
    internal virtual Task<IModbusMessage> CreateResponse<T>(Task<byte[]> frame)
        where T : IModbusMessage, new() =>
        CreateResponseMessage<T>(frame);

    /// <summary>Executes the Validate Response operation.</summary>
    /// <param name="request">The request value.</param>
    /// <param name="response">The response value.</param>
    internal void ValidateResponse(IModbusMessage request, IModbusMessage response)
    {
        // always check the function code and slave address, regardless of transport protocol
        if (request.FunctionCode != response.FunctionCode)
        {
            var msg = $"Received response with unexpected Function Code. Expected {request.FunctionCode}, received {response.FunctionCode}.";
            throw new IOException(msg);
        }

        if (request.SlaveAddress != response.SlaveAddress)
        {
            var msg = $"Response slave address does not match request. Expected {response.SlaveAddress}, received {request.SlaveAddress}.";
            throw new IOException(msg);
        }

        // message specific validation
        var req = request as IModbusRequest;

        req?.ValidateResponse(response);

        OnValidateResponse(request, response);
    }

    /// <summary>Checks whether another response should be read before processing the current response.</summary>
    /// <param name="request">The request value.</param>
    /// <param name="response">The response value.</param>
    /// <returns>The result.</returns>
    internal bool ShouldRetryResponse(IModbusMessage request, IModbusMessage response)
    {
        // These checks are enforced in ValidateRequest, we don't want to retry for these
        if (request.FunctionCode != response.FunctionCode)
        {
            return false;
        }

        return request.SlaveAddress != response.SlaveAddress ? false : OnShouldRetryResponse(request, response);
    }

    /// <summary>Provide hook to check whether receiving a response should be retried.</summary>
    /// <param name="request">The request value.</param>
    /// <param name="response">The response value.</param>
    /// <returns>The result.</returns>
    internal virtual bool OnShouldRetryResponse(IModbusMessage request, IModbusMessage response) => false;

    /// <summary>Provide hook to do transport level message validation.</summary>
    /// <param name="request">The request value.</param>
    /// <param name="response">The response value.</param>
    internal abstract void OnValidateResponse(IModbusMessage request, IModbusMessage response);

    /// <summary>Executes the Read Request operation.</summary>
    /// <returns>The result.</returns>
    internal abstract Task<byte[]> ReadRequest();

    /// <summary>Executes the Read Response operation.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <returns>The result.</returns>
    internal abstract Task<IModbusMessage> ReadResponse<T>()
        where T : IModbusMessage, new();

    /// <summary>Executes the Build Message Frame operation.</summary>
    /// <param name="message">The message value.</param>
    /// <returns>The result.</returns>
    internal abstract byte[] BuildMessageFrame(IModbusMessage message);

    /// <summary>Executes the Write operation.</summary>
    /// <param name="message">The message value.</param>
    internal abstract void Write(IModbusMessage message);

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///     unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        DisposableUtility.Dispose(ref _streamResource);
    }

    /// <summary>Writes a message and reads the matching response.</summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="message">The request message.</param>
    /// <returns>The response message.</returns>
    private IModbusMessage WriteAndReadResponse<T>(IModbusMessage message)
        where T : IModbusMessage, new()
    {
        lock (_syncLock)
        {
            Write(message);
            return ReadResponseUntilReady<T>(message);
        }
    }

    /// <summary>Reads responses until one is ready for validation.</summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="message">The request message.</param>
    /// <returns>The response message.</returns>
    private IModbusMessage ReadResponseUntilReady<T>(IModbusMessage message)
        where T : IModbusMessage, new()
    {
        while (true)
        {
            var response = ReadResponse<T>().Result;
            if (TryHandleAcknowledgeResponse(response) || ShouldRetryResponse(message, response))
            {
                continue;
            }

            return response;
        }
    }

    /// <summary>Handles an acknowledge exception response.</summary>
    /// <param name="response">The response message.</param>
    /// <returns>True when another response should be read.</returns>
    private bool TryHandleAcknowledgeResponse(IModbusMessage response)
    {
        if (response is not SlaveExceptionResponse exceptionResponse)
        {
            return false;
        }

        if (exceptionResponse.SlaveExceptionCode != Modbus.Acknowledge)
        {
            throw new SlaveException(exceptionResponse);
        }

        Debug.WriteLine($"Received ACKNOWLEDGE slave exception response, waiting {_waitToRetryMilliseconds} milliseconds and retrying to read response.");
        Task.Delay(WaitToRetryMilliseconds).Wait();
        return true;
    }

    /// <summary>Waits after a slave-busy response and returns the next attempt number.</summary>
    /// <param name="attempt">The current attempt number.</param>
    /// <returns>The next attempt number.</returns>
    private int RetryAfterSlaveBusy(int attempt)
    {
        Debug.WriteLine($"Received SLAVE_DEVICE_BUSY exception response, waiting {_waitToRetryMilliseconds} milliseconds and resubmitting request.");
        Task.Delay(WaitToRetryMilliseconds).Wait();
        return SlaveBusyUsesRetryCount ? attempt + 1 : attempt;
    }
}
