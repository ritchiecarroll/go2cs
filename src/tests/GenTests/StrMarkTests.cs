// StrMarkTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Face lift S's twin rule (docs/PLAN-marker-comment-parity.md, 5.7), on the generator side. The converter
/// emits the member of an sstring twin that carries the Go body with each twinned parameter typed
/// <c>sstring</c> and NO mark, so StrGenerator selects a method by an <c>sstring</c> parameter, and
/// [GoStr], which hand-written files keep, still selects one. A hand-owned package
/// (<c>[assembly: GoHandOwnedPackage]</c>) keeps the old rule: there a method is a twin by [GoStr] alone.
/// </summary>
[TestClass]
public class StrMarkTests
{
    private const string Golib = """
        namespace go
        {
            public readonly struct @string { }

            public readonly ref struct sstring
            {
                public static explicit operator sstring(@string value) => default;
            }

            [System.AttributeUsage(System.AttributeTargets.Method)]
            public sealed class GoStrAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class GoHandOwnedPackageAttribute : System.Attribute { }

            // golib's variadic pack, the converter's rendering of a Go `...T` tail.
            [System.Runtime.CompilerServices.CollectionBuilder(typeof(VariadicBuilder), "Create")]
            public readonly ref struct variadic<T>
            {
                public System.Span<T>.Enumerator GetEnumerator() => default;
            }

            public static class VariadicBuilder
            {
                public static variadic<T> Create<T>(System.ReadOnlySpan<T> items) => default;
            }
        }

        namespace other
        {
            // A type that is NOT golib's view, spelled the same.
            public readonly struct sstring { }
        }
        """;

    // Package main, emitted the way the converter emits it after face lift S: Count and write carry the Go
    // body with their twinned parameter typed sstring and no mark; Poke is a hand-written twin that keeps
    // [GoStr]; Plain takes no sstring; Other takes a type of the same name that is not golib's.
    private const string Package = """
        namespace go
        {
            public static partial class main_package
            {
                public partial struct buf { }

                public static nint Count(sstring s) => 0;

                public static @string Mixed(@string prefix, global::go.sstring s) => prefix;

                internal static void write(this ref buf b, sstring s) { }

                [GoStr] public static nint Poke(sstring s) => 1;

                public static nint Plain(@string s) => 2;

                public static nint Other(global::other.sstring s) => 3;

                public static @string Sprintf(sstring format, params variadic<object> a) => default;

                public static @string Sprintln(sstring prefix, params System.Span<object> a) => default;
            }
        }
        """;

    private static Dictionary<string, string> Run(bool handOwnedPackage = false)
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        string optOut = handOwnedPackage ? "[assembly: go.GoHandOwnedPackage]\n" : "";

        CSharpCompilation compilation = CSharpCompilation.Create("strmark",
            [CSharpSyntaxTree.ParseText(Golib), CSharpSyntaxTree.ParseText(optOut + Package)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new StrGenerator()).RunGenerators(compilation).GetRunResult();

        foreach (GeneratorRunResult each in result.Results)
            Assert.IsNull(each.Exception, $"the generator must not throw: {each.Exception}");

        return result.Results.SelectMany(each => each.GeneratedSources).ToDictionary(source => source.HintName, source => source.SourceText.ToString());
    }

    // The @string forwarders StrGenerator rendered for a method name.
    private static string[] ForwardersFor(Dictionary<string, string> sources, string methodName) =>
        [.. sources.Values.Where(text => text.Contains($"The @string member of the sstring twin {methodName}:"))];

    [TestMethod]
    public void AnUnmarkedMethodWithAnSStringParameterIsATwin()
    {
        Dictionary<string, string> sources = Run();

        Assert.AreEqual(1, ForwardersFor(sources, "Count").Length,
            "an unmarked method with an sstring parameter carries a twin's Go body: StrGenerator renders its @string forwarder");
        StringAssert.Contains(ForwardersFor(sources, "Count")[0], "Count(global::go.@string s) => Count((global::go.sstring)s);");
    }

    [TestMethod]
    public void TheParameterIsReadInEverySpellingOfGolibsView()
    {
        Dictionary<string, string> sources = Run();

        Assert.AreEqual(1, ForwardersFor(sources, "Mixed").Length, "`global::go.sstring` is golib's view too");
        StringAssert.Contains(ForwardersFor(sources, "Mixed")[0], "Mixed(global::go.@string prefix, global::go.@string s)");
    }

    [TestMethod]
    public void AnUnmarkedReceiverMethodWithAnSStringParameterIsATwin()
    {
        string[] forwarders = ForwardersFor(Run(), "write");

        Assert.AreEqual(1, forwarders.Length);
        StringAssert.Contains(forwarders[0], "write(this ref global::go.main_package.buf b, global::go.@string s) => b.write((global::go.sstring)s);");
    }

    [TestMethod]
    public void AGoStrMethodIsStillATwin()
    {
        Assert.AreEqual(1, ForwardersFor(Run(), "Poke").Length, "[GoStr], which hand-written files keep, still selects a twin");
    }

    [TestMethod]
    public void AMethodWithNoSStringParameterIsNoTwin()
    {
        Dictionary<string, string> sources = Run();

        Assert.AreEqual(0, ForwardersFor(sources, "Plain").Length);
        Assert.AreEqual(0, ForwardersFor(sources, "Other").Length, "a type named sstring that is not golib's view selects nothing");
    }

    // The canonical value delegate of a variadic twin is golib's Funcꓸꓸꓸ family, its tail's ELEMENT the last type
    // argument but one: fmt.Sprintfᶠ's public field type. With the tail a go.variadic<T> the template once missed it
    // and fell through to System.Func<..., go.variadic<object>, ...>, which compiles but changes the field's type
    // (2ee6ee8175; P3 145156Z NIT 5). A hand-written params Span<T> tail takes the family too.
    [TestMethod]
    public void AVariadicTwinsValueDelegateIsTheVariadicFamily()
    {
        Dictionary<string, string> sources = Run();
        string sprintf = string.Join("\n", ForwardersFor(sources, "Sprintf"));
        string sprintln = string.Join("\n", ForwardersFor(sources, "Sprintln"));

        StringAssert.Contains(sprintf, "global::go.Funcꓸꓸꓸ<global::go.@string, object, global::go.@string> Sprintfᶠ");
        StringAssert.Contains(sprintln, "global::go.Funcꓸꓸꓸ<global::go.@string, object, global::go.@string> Sprintlnᶠ");
        Assert.IsFalse(sprintf.Contains("System.Func<"), "the variadic<T> tail must not fall through to System.Func");
    }

    [TestMethod]
    public void AHandOwnedPackageSelectsTwinsByGoStrAlone()
    {
        Dictionary<string, string> sources = Run(handOwnedPackage: true);

        Assert.AreEqual(0, ForwardersFor(sources, "Count").Length,
            "inside a hand-owned package an unmarked method with an sstring parameter is not a twin");
        Assert.AreEqual(0, ForwardersFor(sources, "write").Length);
        Assert.AreEqual(1, ForwardersFor(sources, "Poke").Length, "[GoStr] still is");
    }
}
