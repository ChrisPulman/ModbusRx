// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive
#else
namespace ModbusRx
#endif
{
    /// <summary>Modbus Communication Exception.</summary>
    /// <seealso cref="System.Exception"/>
    [Serializable]
    public class ModbusCommunicationException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ModbusCommunicationException"/> class.</summary>
        public ModbusCommunicationException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModbusCommunicationException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public ModbusCommunicationException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModbusCommunicationException"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ModbusCommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
