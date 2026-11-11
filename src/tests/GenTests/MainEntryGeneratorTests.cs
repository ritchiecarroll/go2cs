// MainEntryGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards the generated entry point of a converted program (C46): Go's <c>main</c> keeps its name, and an executable
/// gets <c>Main() =&gt; MainThread.Run(main)</c>, unless something in the program already declares an entry point.
/// </summary>
[TestClass]
public class MainEntryGeneratorTests
{
    private const string Program =
        """
        namespace go;

        public static partial class main_package
        {
            internal static void main() { }
        }
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.golib.MainThread).Assembly.Location));
    }

    private static (string[] entries, Compilation output) Run(OutputKind kind, params string[] sources)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("program",
            sources.Select(source => CSharpSyntaxTree.ParseText(source, options)), References(),
            new CSharpCompilationOptions(kind));

        CSharpGeneratorDriver.Create(new MainEntryGenerator()).WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] entries = output.SyntaxTrees.Select(tree => tree.ToString())
            .Where(source => source.Contains("MainThread.Run", StringComparison.Ordinal))
            .ToArray();

        return (entries, output);
    }

    private static Diagnostic[] Errors(Compilation output) =>
        output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

    [TestMethod]
    public void AProgramGetsAnEntryPointThatRunsMainOnTheMainThread()
    {
        (string[] entries, Compilation output) = Run(OutputKind.ConsoleApplication, Program);

        Assert.AreEqual(1, entries.Length, "a converted program must carry exactly one generated entry point");
        StringAssert.Contains(entries[0], "static void Main() => global::go.golib.MainThread.Run(main);");

        Diagnostic[] errors = Errors(output);
        Assert.AreEqual(0, errors.Length, "the generated entry point must compile: " + string.Join("; ", errors.Select(error => error.ToString())));

        IMethodSymbol? entry = output.GetEntryPoint(default);
        Assert.IsNotNull(entry, "the program has no entry point");
        Assert.AreEqual("go.main_package", entry.ContainingType.ToDisplayString());
        Assert.AreEqual("Main", entry.Name);
    }

    [TestMethod]
    public void ALibraryGetsNoEntryPoint()
    {
        (string[] entries, _) = Run(OutputKind.DynamicallyLinkedLibrary, Program);

        Assert.AreEqual(0, entries.Length, "a library has no entry point");
    }

    [TestMethod]
    public void AnOlderConversionKeepsItsOwnMain()
    {
        const string older =
            """
            namespace go;

            public static partial class main_package
            {
                internal static void Main() { }
            }
            """;

        (string[] entries, Compilation output) = Run(OutputKind.ConsoleApplication, older);

        Assert.AreEqual(0, entries.Length, "a program that declares Main is its own entry point");
        Assert.AreEqual(0, Errors(output).Length, "and still compiles");
    }

    [TestMethod]
    public void TheTestHostKeepsItsOwnMain()
    {
        const string host =
            """
            namespace go;

            internal static class Go2CsTestHost
            {
                public static int Main(string[] args) => 0;
            }
            """;

        (string[] entries, Compilation output) = Run(OutputKind.ConsoleApplication, Program, host);

        Assert.AreEqual(0, entries.Length, "the converted-test host is the entry point of a test of package main");
        Assert.AreEqual(0, Errors(output).Length, "and the two compile together");
    }

    [TestMethod]
    public void AGoMethodNamedMainDoesNotTakeTheEntryPointsPlace()
    {
        const string method =
            """
            namespace go;

            public static partial class main_package
            {
                public static void Main(this int receiver) { }
            }
            """;

        (string[] entries, Compilation output) = Run(OutputKind.ConsoleApplication, Program, method);

        Assert.AreEqual(1, entries.Length, "an extension method named Main is not an entry point");
        Assert.AreEqual(0, Errors(output).Length, "the program must still compile: " + string.Join("; ", Errors(output).Select(error => error.ToString())));
    }

    [TestMethod]
    public void AProgramWithoutGoMainGetsNoEntryPoint()
    {
        const string noMain =
            """
            namespace go;

            public static partial class main_package
            {
                internal static void helper() { }
            }
            """;

        (string[] entries, _) = Run(OutputKind.ConsoleApplication, noMain);

        Assert.AreEqual(0, entries.Length);
    }
}
