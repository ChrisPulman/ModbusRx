// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ModbusRx.Generators;

/// <summary>Provides nullable type helpers for generator type analysis.</summary>
internal static class Nullable
{
    /// <summary>Gets the underlying type name from a nullable type symbol.</summary>
    /// <param name="typeSymbol">The type symbol to inspect.</param>
    /// <returns>The fully qualified underlying type name when the symbol is nullable.</returns>
    public static string? GetUnderlyingTypeName(ITypeSymbol typeSymbol)
    {
        return typeSymbol is INamedTypeSymbol named &&
            named.IsGenericType &&
            named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T
            ? named.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : null;
    }
}
