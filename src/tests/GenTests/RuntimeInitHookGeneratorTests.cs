// RuntimeInitHookGeneratorTests.cs - Gbtc
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
/// Guards the lazy runtime-initialization hook: a converted EXECUTABLE that references the converted <c>runtime</c>
/// package gets a module initializer installing golib's <c>CrashReport.RuntimeInitializer</c>, so a program that never
/// touches runtime still prints Go's traceback when it crashes. A library gets none (its executable carries the hook),
/// and neither does runtime itself.
/// </summary>
[TestClass]
public class RuntimeInitHookGeneratorTests
{
    // A stand-in for the converted runtime package, compiled as its own assembly, as the real one is.
    private const string RuntimeStub =
        """
        namespace go;

        public static partial class runtime_package
        {
            public static void Touch() { }
        }
        """;

    private const string Program =
        """
        namespace go;

        public static partial class main_package
        {
            internal static void Main() { }
        }
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.golib.CrashReport).Assembly.Location));
    }

    private static (string[] hooks, Compilation output) Run(OutputKind kind, bool referenceRuntime)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);
        List<MetadataReference> references = [.. References()];

        if (referenceRuntime)
        {
            CSharpCompilation runtime = CSharpCompilation.Create("runtime",
                [CSharpSyntaxTree.ParseText(RuntimeStub, options)], references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            references.Add(runtime.ToMetadataReference());
        }

        CSharpCompilation compilation = CSharpCompilation.Create("program",
            [CSharpSyntaxTree.ParseText(Program, options)], references,
            new CSharpCompilationOptions(kind));

        CSharpGeneratorDriver.Create(new RuntimeInitHookGenerator()).WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] hooks = output.SyntaxTrees.Select(tree => tree.ToString())
            .Where(source => source.Contains("CrashReport.RuntimeInitializer", StringComparison.Ordinal))
            .ToArray();

        return (hooks, output);
    }

    [TestMethod]
    public void AnExecutableThatReferencesRuntimeGetsTheHook()
    {
        (string[] hooks, Compilation output) = Run(OutputKind.ConsoleApplication, referenceRuntime: true);

        Assert.AreEqual(1, hooks.Length, "a converted executable referencing runtime must carry exactly one runtime-init hook");
        StringAssert.Contains(hooks[0], "ModuleInitializer", "the hook is installed from a module initializer");
        StringAssert.Contains(hooks[0], "RunModuleConstructor(typeof(global::go.runtime_package).Module.ModuleHandle)",
            "the hook initializes runtime through a typeof, which roots it for trimming and AOT");

        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual(0, errors.Length, "the generated hook must compile: " + string.Join("; ", errors.Select(error => error.ToString())));
    }

    [TestMethod]
    public void ALibraryGetsNoHook()
    {
        (string[] hooks, _) = Run(OutputKind.DynamicallyLinkedLibrary, referenceRuntime: true);

        Assert.AreEqual(0, hooks.Length, "a library's hook belongs to the executable that references it");
    }

    [TestMethod]
    public void AnExecutableWithoutRuntimeGetsNoHook()
    {
        (string[] hooks, _) = Run(OutputKind.ConsoleApplication, referenceRuntime: false);

        Assert.AreEqual(0, hooks.Length, "with no runtime to initialize there is nothing to hook");
    }
}
