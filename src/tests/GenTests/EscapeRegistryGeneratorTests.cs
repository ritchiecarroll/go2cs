// EscapeRegistryGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards the generated escape registry (trim stage 3c-2b(iii)): every CLOSED container a package converts to an
/// interface, or reaches through a converted value's fields or a pointer's target, is registered with golib's GoTypeOps;
/// a container that never escapes is not, and an open one (inside generic code) cannot be. A container the registry
/// misses is one reflect.New / Zero(*C) cannot serve under Native AOT (row G's container probe, measured). The fixture's
/// seven shapes: direct to object, via any, via a struct field, a private element type, via a pointer's target, never
/// escaping (absent), open generic (absent).
/// </summary>
[TestClass]
public class EscapeRegistryGeneratorTests
{
    private const string Path = @"C:\go2cs\src\core\etest\etest.cs";

    private const string Package =
        """
        namespace go;

        [GoPackage("etest")]
        public static partial class etest_package
        {
            partial struct Holder { internal slice<@string> names; internal map<@string, nint> counts; }

            private partial struct Secret { internal nint x; }

            internal static void Use()
            {
                object direct = default(slice<nint>);
                any viaAny = default(array<byte>);
                object viaField = default(Holder);
                object privateElement = default(slice<Secret>);
                object viaPointer = default(ж<channel<int>>);
                channel<bool> local = default;
                _ = local;
            }

            internal static object Generic<T>(slice<T> s) => s;
        }
        """;

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private static (string[] registered, Compilation output) Run()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("etest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\etest\obj\etest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new EscapeRegistryGenerator()).WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] registered = output.SyntaxTrees.Select(tree => tree.ToString())
            .SelectMany(source => Regex.Matches(source, @"global::go\.GoTypeOps\.Register\(global::go\.GoTypeOps<(.+)>\.Instance\);").Select(match => match.Groups[1].Value))
            .OrderBy(name => name, StringComparer.Ordinal).ToArray();

        return (registered, output);
    }

    [TestMethod]
    public void EveryEscapingClosedContainerIsRegistered()
    {
        (string[] registered, Compilation output) = Run();

        string[] expected =
        [
            "global::go.array<byte>",                                  // converted to any
            "global::go.channel<int>",                                 // reached through a pointer's target
            "global::go.map<global::go.@string, nint>",                // reached through a converted struct's field
            "global::go.slice<global::go.@string>",                    // reached through a converted struct's field
            "global::go.slice<global::go.etest_package.Secret>",       // a private element: registered inside the package class
            "global::go.slice<nint>",                                  // converted to object
        ];

        CollectionAssert.AreEqual(expected.OrderBy(name => name, StringComparer.Ordinal).ToArray(), registered, "registered: " + string.Join(", ", registered));
        Assert.IsFalse(registered.Any(name => name.Contains("channel<bool>")), "a container that never escapes is not registered");
        Assert.IsFalse(registered.Any(name => name.Contains("<T>")), "an open container cannot be registered");

        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual(0, errors.Length, "the registry binds: " + string.Join("; ", errors.Take(3)));
    }
}
