// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Message;

/// <summary>
///     Methods specific to a modbus request message.
/// </summary>
public interface IModbusRequest : IModbusMessage
{
    /// <summary>
    /// Validate the specified response against the current request.
    /// </summary>
    /// <param name="response">The response.</param>
    void ValidateResponse(IModbusMessage response);
}
