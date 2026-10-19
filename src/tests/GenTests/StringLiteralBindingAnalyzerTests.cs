// StringLiteralBindingAnalyzerTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// GO2CS0004, the fail-closed guard of plain string literals: a bare C# string literal in converted code
/// that binds an object or interface (A1), a type parameter inferred from it (A2) or an unmarked
/// System.String overload beside an @string one (A3) is a build error; a parameter marked
/// [GoTakesCSharpString] takes a C# string on purpose and is exempt.
/// </summary>
/// <remarks>
/// Each arm runs the REAL analyzer over one in-memory compilation. The stand-ins mirror golib's shapes:
/// @string converts implicitly from a C# string, len has both overloads (len(string) counts UTF-8 bytes
/// and is marked), panic takes a marked object. RED before the analyzer existed: every positive arm read
/// no diagnostic at all.
/// </remarks>
[TestClass]
public class StringLiteralBindingAnalyzerTests
{
    private const string Golib =
        """
        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public sealed class GoTakesCSharpStringAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class GoHandOwnedPackageAttribute : System.Attribute { }

            public readonly struct @string
            {
                public static implicit operator @string(string value) => default;
            }

            public static class builtin
            {
                public static long len(in @string s) => 0;
                public static long len([GoTakesCSharpString] string s) => 0;
                public static System.Exception panic([GoTakesCSharpString] object? state) => new();
            }

            public sealed class Box<T>
            {
                public void Set(T value) { }
            }
        }
        """;

    private const string Callees =
        """
        namespace go
        {
            public static partial class main_package
            {
                public static T Echo<T>(T v) => v;
                public static System.Collections.Generic.Dictionary<K, V> Pair<K, V>(K k, V v) => new();
                public static void Take(@string s) { }
                public static void Show(object v) { }
                public static void Comparable(System.IComparable v) { }
                public static void Overloaded(@string s) { }
                public static void Overloaded(string s) { }
                public static void CSharpOnly(string s) { }
            }
        }
        """;

    private static ImmutableArray<Diagnostic> Analyze(string body, string? path = null, string? assemblyAttributes = null)
    {
        string source =
            $$"""
            {{assemblyAttributes}}
            namespace go
            {
                public static partial class main_package
                {
                    public static void Body()
                    {
                        {{body}}
                    }
                }
            }
            """;

        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create(
            "string-literal-binding",
            [
                CSharpSyntaxTree.ParseText(Golib, parseOptions, path: "golib_stand_in_impl.cs"),
                CSharpSyntaxTree.ParseText(Callees, parseOptions, path: "callees_impl.cs"),
                CSharpSyntaxTree.ParseText(source, parseOptions, path: path ?? "")
            ],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Collections.dll"))
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        ImmutableArray<Diagnostic> compileErrors = [.. compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        Assert.AreEqual(0, compileErrors.Length, $"the fixture must compile: {string.Join("; ", compileErrors)}");

        return compilation.WithAnalyzers([new StringLiteralBindingAnalyzer()]).GetAnalyzerDiagnosticsAsync().Result;
    }

    private static void AssertOne(string body, string bindingFragment)
    {
        ImmutableArray<Diagnostic> diagnostics = Analyze(body);
        List<Diagnostic> matching = [.. diagnostics.Where(diagnostic => diagnostic.Id == "GO2CS0004")];

        Assert.AreEqual(1, matching.Count, $"exactly one GO2CS0004 for `{body}`: {string.Join("; ", diagnostics)}");
        Assert.AreEqual(DiagnosticSeverity.Error, matching[0].Severity);
        StringAssert.Contains(matching[0].GetMessage(), bindingFragment);
    }

    private static void AssertNone(string body, string? path = null, string? assemblyAttributes = null)
    {
        ImmutableArray<Diagnostic> diagnostics = Analyze(body, path, assemblyAttributes);
        Assert.AreEqual(0, diagnostics.Length, $"no GO2CS0004 for `{body}`: {string.Join("; ", diagnostics)}");
    }

    // A2, G's first red arm: an inferred generic call takes T from the literal, so `Echo("x")` makes
    // T = System.String, compiles, and diverges from Go's string.
    [TestMethod]
    public void AnInferredGenericCallTakingABareLiteralIsAnError() => AssertOne("""Echo("x");""", "type parameter 'T'");

    // A2: the map key type becomes System.String (`Pair("k", 1)` is a map<string, nint>).
    [TestMethod]
    public void AnInferredGenericMapKeyIsAnError() => AssertOne("""Pair("k", 1);""", "type parameter 'K'");

    // A1: the dynamic type would be System.String, which no Go type switch or assertion matches.
    [TestMethod]
    public void ABareLiteralBoxedToObjectIsAnError() => AssertOne("""Show("x");""", "'object'");

    [TestMethod]
    public void ABareLiteralConvertedToAnInterfaceIsAnError() => AssertOne("""Comparable("x");""", "'System.IComparable'");

    // A3: overload resolution chose the C# string overload over the @string one.
    [TestMethod]
    public void AnUnmarkedStringOverloadBesideAnAtStringOneIsAnError() => AssertOne("""Overloaded("x");""", "where an @string overload exists");

    // The intended binding, and every exemption.
    [TestMethod]
    public void TheIntendedBindingsAreClean()
    {
        AssertNone("""Take("x");""");                         // @string through its implicit conversion
        AssertNone("""builtin.len("a☺b☻");""");               // marked len(string): counts UTF-8 bytes
        AssertNone("""throw builtin.panic("boom");""");       // marked object: panic normalizes to @string
        AssertNone("""CSharpOnly("x");""");                   // no @string overload at that position
        AssertNone("""new Box<@string>().Set("x");""");       // a CLASS type parameter is fixed by the receiver
    }

    [TestMethod]
    public void AttributeArgumentsNameofAndParameterDefaultsAreNotValueSlots()
    {
        AssertNone("""[System.Obsolete("not a value slot")] void Local() { } Local();""");
        AssertNone("""string n = nameof(Take); _ = n;""");
        AssertNone("""void Local(string where = "default") { } Local();""");
    }

    // Converted code only: a hand-written companion (*_impl.cs) and a hand-owned package are never read.
    [TestMethod]
    public void HandWrittenSourcesAreNotRead()
    {
        AssertNone("""Echo("x"); Show("x");""", path: "main_impl.cs");
        AssertNone("""Echo("x"); Show("x");""", assemblyAttributes: "[assembly: go.GoHandOwnedPackage]");
    }
}
