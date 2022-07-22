// <copyright file="StreamResourceUtility.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text;

namespace ModbusRx.IO;

internal static class StreamResourceUtility
{
    internal static async Task<string> ReadLineAsync(IStreamResource stream)
    {
        var result = new StringBuilder();
        var singleByteBuffer = new byte[1];

        do
        {
            if (await stream.ReadAsync(singleByteBuffer, 0, 1) == 0)
            {
                continue;
            }

            result.Append(Encoding.UTF8.GetChars(singleByteBuffer).First());
        }
        while (!result.ToString().EndsWith(Modbus.NewLine));

        return result.ToString()[..(result.Length - Modbus.NewLine.Length)];
    }
}
