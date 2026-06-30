// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.IO;
#else
namespace ModbusRx.IO;
#endif

/// <summary>Provides Stream Resource Utility functionality.</summary>
internal static class StreamResourceUtility
{
    /// <summary>Executes the Read Line Async operation.</summary>
    /// <param name="stream">The stream value.</param>
    /// <returns>The result.</returns>
    internal static async Task<string> ReadLineAsync(IStreamResource stream)
    {
        var result = new StringBuilder();
        var singleByteBuffer = new byte[1];
        var singleCharBuffer = new char[1];

        do
        {
            if (await stream.ReadAsync(singleByteBuffer, 0, 1) == 0)
            {
                continue;
            }

            _ = Encoding.UTF8.GetChars(singleByteBuffer, 0, 1, singleCharBuffer, 0);
            _ = result.Append(singleCharBuffer[0]);
        }
        while (!result.ToString().EndsWith(Modbus.NewLine));

        return result.ToString()[..(result.Length - Modbus.NewLine.Length)];
    }
}
