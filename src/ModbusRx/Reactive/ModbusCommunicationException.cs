// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Reactive
{
    /// <summary>
    /// Modbus Communication Exception.
    /// </summary>
    /// <seealso cref="System.Exception"/>
    [Serializable]
    public class ModbusCommunicationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusCommunicationException"/> class.
        /// </summary>
        public ModbusCommunicationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusCommunicationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ModbusCommunicationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusCommunicationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ModbusCommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusCommunicationException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the
        /// serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains
        /// contextual information about the source or destination.
        /// </param>
        protected ModbusCommunicationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
