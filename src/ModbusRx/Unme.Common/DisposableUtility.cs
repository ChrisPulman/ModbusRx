// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Unme.Common;

internal static class DisposableUtility
{
    public static void Dispose<T>(ref T? item)
        where T : class, IDisposable
    {
        if (item is null)
        {
            return;
        }

        item.Dispose();
        item = default;
    }
}
