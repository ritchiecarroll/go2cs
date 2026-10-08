// ReceiverMarkTests.cs - Gbtc
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
/// Face lift A's receiver rule (docs/PLAN-marker-comment-parity.md, 5.1), on the generator side. The
/// converter emits a Go pointer receiver `this ref T` with NO mark, so a by-ref receiver is a
/// POINTER-set method unless it is marked [GoCopyBound], and [GoRecv] — which hand-written files keep —
/// still means pointer-set. RecvGenerator mints the ж twin for every pointer-set `this ref` method, and
/// TypeGenerator marks the one by-ref forwarder that is VALUE-set: a pointer-receiver method promoted
/// through an embedded POINTER. A hand-owned package (<c>[assembly: GoHandOwnedPackage]</c>) keeps the
/// old rule: there a method is a pointer receiver by [GoRecv] alone.
/// </summary>
[TestClass]
public class ReceiverMarkTests
{
    private const string Golib = """
        namespace go
        {
            public class ж<T> { public ж(T value) { } public T Value = default!; }

            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Method)]
            public sealed class GoRecvAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Method)]
            public sealed class GoCopyBoundAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class GoHandOwnedPackageAttribute : System.Attribute { }
        }
        """;

    // Package main, emitted the way the converter emits it after face lift A: Bump is a Go pointer
    // receiver with no mark; SameP reaches it through a POINTER embed (value set), SameV through a
    // VALUE embed (pointer set only).
    private const string Package = """
        namespace go
        {
            public static partial class main_package
            {
                [GoType] public partial struct Inner { public nint n; }

                public static nint Bump(this ref Inner i) { i.n++; return i.n; }

                public static nint Get(this Inner i) => i.n;

                [GoType] public partial struct SameP
                {
                    public partial ref ж<Inner> Inner { get; }
                }

                [GoType] public partial struct SameV
                {
                    public partial ref Inner Inner { get; }
                }

                // A hand-written pointer receiver that keeps [GoRecv], and a by-ref receiver marked
                // value-set, which has no pointer twin.
                [GoRecv] public static void Poke(this ref Inner i) { }

                [GoCopyBound] public static nint Peek(this ref Inner i) => i.n;
            }
        }
        """;

    private static Dictionary<string, string> Run(ISourceGenerator generator, bool handOwnedPackage = false)
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        string optOut = handOwnedPackage ? "[assembly: go.GoHandOwnedPackage]\n" : "";

        CSharpCompilation compilation = CSharpCompilation.Create("receivermark",
            [CSharpSyntaxTree.ParseText(Golib), CSharpSyntaxTree.ParseText(optOut + Package)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation).GetRunResult();

        foreach (GeneratorRunResult each in result.Results)
            Assert.IsNull(each.Exception, $"the generator must not throw: {each.Exception}");

        return result.Results.SelectMany(each => each.GeneratedSources).ToDictionary(source => source.HintName, source => source.SourceText.ToString());
    }

    private static string[] TwinsFor(Dictionary<string, string> sources, string methodName) =>
        [.. sources.Values.Where(text => text.Contains($" {methodName}(this ж<"))];

    [TestMethod]
    public void AnUnmarkedRefReceiverGetsItsPointerTwin()
    {
        Assert.AreEqual(1, TwinsFor(Run(new RecvGenerator()), "Bump").Length,
            "an unmarked `this ref` receiver is a Go pointer receiver: RecvGenerator mints its ж twin");
    }

    [TestMethod]
    public void AGoRecvRefReceiverStillGetsItsPointerTwin()
    {
        Assert.AreEqual(1, TwinsFor(Run(new RecvGenerator()), "Poke").Length,
            "[GoRecv], which hand-written files keep, still means pointer receiver");
    }

    [TestMethod]
    public void ACopyBoundRefReceiverGetsNoPointerTwin()
    {
        Assert.AreEqual(0, TwinsFor(Run(new RecvGenerator()), "Peek").Length,
            "a [GoCopyBound] receiver is VALUE-set: it has no pointer twin");
    }

    [TestMethod]
    public void AValueReceiverGetsNoPointerTwin()
    {
        Assert.AreEqual(0, TwinsFor(Run(new RecvGenerator()), "Get").Length);
    }

    [TestMethod]
    public void AHandOwnedPackageMintsTwinsForGoRecvAlone()
    {
        Dictionary<string, string> sources = Run(new RecvGenerator(), handOwnedPackage: true);

        Assert.AreEqual(0, TwinsFor(sources, "Bump").Length,
            "inside a hand-owned package an unmarked `this ref` receiver is not a pointer receiver");
        Assert.AreEqual(1, TwinsFor(sources, "Poke").Length, "[GoRecv] still is");
    }

    [TestMethod]
    public void TheForwarderThroughAPointerEmbedIsMarkedCopyBound()
    {
        string sameP = Run(new TypeGenerator()).Single(pair => pair.Key.Contains("_package.SameP.g.cs")).Value;
        string line = sameP.Split('\n').Single(text => text.Contains(" Bump(this ref "));

        StringAssert.Contains(line, "[global::go.GoCopyBound] ",
            "through a pointer hop the promoted pointer method is in the VALUE set; unmarked, golib would read it pointer-set");
        Assert.IsFalse(line.Contains("GoRecv"), line);
    }

    [TestMethod]
    public void TheForwarderThroughAValueEmbedIsPointerSet()
    {
        string sameV = Run(new TypeGenerator()).Single(pair => pair.Key.Contains("_package.SameV.g.cs")).Value;
        string line = sameV.Split('\n').Single(text => text.Contains(" Bump(this ref "));

        Assert.IsFalse(line.Contains("GoCopyBound"), $"through value hops only the method is pointer-set: {line.Trim()}");
    }
}
