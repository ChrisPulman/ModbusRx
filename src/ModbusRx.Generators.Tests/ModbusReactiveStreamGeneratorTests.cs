// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ModbusRx.Generators;

namespace ModbusRx.Generators.Tests;

/// <summary>Tests for the reactive stream source generator.</summary>
public class ModbusReactiveStreamGeneratorTests
{
    /// <summary>Verifies generated properties expose a matching observable and binding method.</summary>
    [TUnit.Core.Test]
    public void GeneratesPropertyObservableAndBinding()
    {
        const string source = """
using System;
using ModbusRx.Device;
using ModbusRx.Generators;

[ModbusReactiveDevice(ConnectionMember = "MasterStream")]
public partial class BoilerMap
{
    public IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> MasterStream { get; set; } = default!;

    [HoldingRegister(0)]
    public partial ushort? Temperature { get; private set; }
}
""";

        var result = RunGenerator(source);
        var generatedSource = ConcatenateGeneratedSources(result.GeneratedTrees);

        Assert.Contains("TemperatureObservable", generatedSource);
        Assert.Contains("BindGeneratedModbusStreams", generatedSource);
        Assert.Contains("global::ModbusRx.Create.ReadHoldingRegisters(this.MasterStream, 0, 1, 1000", generatedSource);
    }

    /// <summary>Verifies generated code compiles against ModbusRx and ReactiveUI.Primitives.</summary>
    [TUnit.Core.Test]
    public void GeneratedCodeCompiles()
    {
        const string source = """
using System;
using ModbusRx.Device;
using ModbusRx.Generators;

namespace Maps;

[ModbusReactiveDevice(ConnectionMember = "MasterStream")]
public partial class BoilerMap
{
    public IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> MasterStream { get; set; } = default!;

    [HoldingRegister(0)]
    public partial ushort? Temperature { get; private set; }

    [Coil(3)]
    public partial bool? Enabled { get; private set; }
}
""";

        var result = RunGenerator(source);
        var diagnostics = CollectErrors(result.Compilation.GetDiagnostics());

        Assert.True(diagnostics.Count == 0, FormatDiagnostics(diagnostics));
    }

    /// <summary>Verifies generated code compiles against the ModbusRx.Reactive shim namespace.</summary>
    [TUnit.Core.Test]
    public void GeneratedReactiveShimCodeCompiles()
    {
        const string source = """
using System;
using ModbusRx.Generators;
using ModbusRx.Reactive.Device;

namespace Maps;

[ModbusReactiveDevice(ConnectionMember = "MasterStream")]
public partial class BoilerMap
{
    public IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> MasterStream { get; set; } = default!;

    [HoldingRegister(0)]
    public partial ushort? Temperature { get; private set; }
}
""";

        var result = RunGenerator(source);
        var generatedSource = ConcatenateGeneratedSources(result.GeneratedTrees);
        var diagnostics = CollectErrors(result.Compilation.GetDiagnostics());

        Assert.Contains("global::ModbusRx.Reactive.Create.ReadHoldingRegisters(this.MasterStream, 0, 1, 1000", generatedSource);
        Assert.True(diagnostics.Count == 0, FormatDiagnostics(diagnostics));
    }

    /// <summary>Runs the reactive stream generator against a source document.</summary>
    /// <param name="source">The source code to compile.</param>
    /// <returns>The generated trees and updated compilation.</returns>
    private static GeneratorRunResult RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            GetReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var generator = new ModbusReactiveStreamGenerator();
        var driver = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], parseOptions: new CSharpParseOptions(LanguageVersion.Preview));
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var errors = CollectErrors(diagnostics);
        Assert.True(errors.Count == 0, FormatDiagnostics(errors));
        return new GeneratorRunResult(driver.GetRunResult().GeneratedTrees, outputCompilation);
    }

    /// <summary>Gets metadata references used by in-memory generator test compilations.</summary>
    /// <returns>The metadata references needed to compile the generated source.</returns>
    private static IEnumerable<MetadataReference> GetReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (!string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator))
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    yield return MetadataReference.CreateFromFile(path);
                }
            }
        }

        yield return MetadataReference.CreateFromFile(typeof(ModbusRx.Device.ModbusIpMaster).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(ModbusRx.Reactive.Device.ModbusIpMaster).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(ReactiveUI.Primitives.Signals.Signal).Assembly.Location);
    }

    /// <summary>Collects error diagnostics from a diagnostic sequence.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <returns>The diagnostics with error severity.</returns>
    private static List<Diagnostic> CollectErrors(IEnumerable<Diagnostic> diagnostics)
    {
        var errors = new List<Diagnostic>();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                errors.Add(diagnostic);
            }
        }

        return errors;
    }

    /// <summary>Formats diagnostics as a multi-line assertion message.</summary>
    /// <param name="diagnostics">The diagnostics to format.</param>
    /// <returns>The formatted diagnostic message.</returns>
    private static string FormatDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        var builder = new StringBuilder();
        foreach (var diagnostic in diagnostics)
        {
            if (builder.Length > 0)
            {
                _ = builder.AppendLine();
            }

            _ = builder.Append(diagnostic.ToString());
        }

        return builder.ToString();
    }

    /// <summary>Concatenates generated syntax tree source text.</summary>
    /// <param name="generatedTrees">The generated trees to concatenate.</param>
    /// <returns>The generated source text.</returns>
    private static string ConcatenateGeneratedSources(IReadOnlyList<SyntaxTree> generatedTrees)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < generatedTrees.Count; i++)
        {
            if (i > 0)
            {
                _ = builder.AppendLine();
            }

            _ = builder.Append(generatedTrees[i].GetText().ToString());
        }

        return builder.ToString();
    }

    /// <summary>Stores generator run output for assertions.</summary>
    /// <param name="generatedTrees">The generated syntax trees.</param>
    /// <param name="compilation">The updated compilation.</param>
    private sealed class GeneratorRunResult(IReadOnlyList<SyntaxTree> generatedTrees, Compilation compilation)
    {
        /// <summary>Gets the generated syntax trees.</summary>
        public IReadOnlyList<SyntaxTree> GeneratedTrees { get; } = generatedTrees;

        /// <summary>Gets the updated compilation.</summary>
        public Compilation Compilation { get; } = compilation;
    }
}
