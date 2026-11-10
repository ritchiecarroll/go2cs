// EmbedDepthMethodPromotionTests.cs - Gbtc
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
/// Go's selector rule for a promoted method: <c>x.M</c> names the <c>M</c> at the SHALLOWEST embedding depth, and only
/// when exactly one <c>M</c> sits at that depth. The TypeGenerator's promoted receivers applied it in two depth-blind
/// halves and saw a box-receiver primary (<c>this ж&lt;T&gt;</c>) on a value path only when the DIRECT embed declared
/// it. The TypeScript compiler's ast package is the measured case (TS readiness C29, 533 CS1929): NodeDefault embeds
/// Node and redeclares some of its methods, NodeBase embeds NodeDefault, and the concrete nodes embed NodeBase beside
/// sibling embeds. The behavioral project EmbedDepthMethodPromotion runs the same shapes against Go's output; this
/// reads the forwarders themselves.
/// </summary>
[TestClass]
public class EmbedDepthMethodPromotionTests
{
    private const string FilePath = @"C:\go2cs\src\core\dtest\dtest.cs";

    // The converter's own emission for each shape: a `/*embed*/` field named for its type, a ref-receiver method as
    // `this ref T`, and a method that needs its receiver's address as the box primary `this ж<T>`. Each embed sits
    // on a line of its own: the marker is read from the field's LEADING trivia, and a comment that follows `{` on
    // one line is that brace's trailing trivia.
    private const string Source =
        """
        namespace go;

        [GoPackage("dtest")]
        public static partial class dtest_package
        {
            // A: box primaries two and three value embeds down, behind a struct with two embeds.
            partial struct aNode { internal nint k; }
            internal static ж<aNode> Root(this ж<aNode> Ꮡn) => Ꮡn;
            internal static ж<aNode> Self(this ж<aNode> Ꮡn) => Ꮡn;
            partial struct aDefault {
                /*embed*/ internal aNode aNode;
            }
            internal static ж<aNode> Self(this ж<aDefault> Ꮡd) => default!;
            partial struct aBase {
                /*embed*/ internal aDefault aDefault;
            }
            partial struct aSide { internal nint f; }
            internal static nint Flag(this ref aSide s) => s.f;
            partial struct aStmt {
                /*embed*/ internal aBase aBase;
                /*embed*/ internal aSide aSide;
            }
            partial struct aIf {
                /*embed*/ internal aStmt aStmt;
            }

            // B: one name through two direct embeds at different depths, in both field orders.
            partial struct bDeep { }
            internal static @string Name(this ref bDeep d) => "deep";
            partial struct bMid {
                /*embed*/ internal bDeep bDeep;
            }
            partial struct bFlat { }
            internal static @string Name(this ref bFlat f) => "flat";
            partial struct bBoth {
                /*embed*/ internal bMid bMid;
                /*embed*/ internal bFlat bFlat;
            }
            partial struct bFirst {
                /*embed*/ internal bFlat bFlat;
                /*embed*/ internal bMid bMid;
            }
            partial struct bOuter {
                /*embed*/ internal bBoth bBoth;
            }

            // C: a box primary shadows a ref-receiver method one level further down.
            partial struct cNode { internal nint k; }
            internal static nint Visit(this ref cNode n) => n.k;
            partial struct cDefault {
                /*embed*/ internal cNode cNode;
            }
            internal static nint Visit(this ж<cDefault> Ꮡd) => 0;
            partial struct cBase {
                /*embed*/ internal cDefault cDefault;
            }
            partial struct cStmt {
                /*embed*/ internal cBase cBase;
            }

            // D: a name one embed reaches twice at one depth and a sibling embed declares once, shallower.
            partial struct dLeft { }
            internal static @string Tag(this ref dLeft l) => "left";
            partial struct dRight { }
            internal static @string Tag(this ref dRight r) => "right";
            partial struct dPair {
                /*embed*/ internal dLeft dLeft;
                /*embed*/ internal dRight dRight;
            }
            partial struct dTop { }
            internal static @string Tag(this ref dTop t) => "top";
            partial struct dAll {
                /*embed*/ internal dPair dPair;
                /*embed*/ internal dTop dTop;
            }

            // E: inside one embed the closest declaration has another signature than the first one met.
            partial struct eFar { }
            internal static nint Pick(this ref eFar f, nint n) => n;
            partial struct eVia {
                /*embed*/ internal eFar eFar;
            }
            partial struct eNear { }
            internal static @string Pick(this ref eNear n) => "near";
            partial struct eFork {
                /*embed*/ internal eVia eVia;
                /*embed*/ internal eNear eNear;
            }
            partial struct eOuter {
                /*embed*/ internal eFork eFork;
            }

            // F: a three-way tie at the shallowest depth.
            partial struct fOne { }
            internal static @string Tie(this ref fOne o) => "one";
            partial struct fTwo { }
            internal static @string Tie(this ref fTwo t) => "two";
            partial struct fBoth {
                /*embed*/ internal fOne fOne;
                /*embed*/ internal fTwo fTwo;
            }
            partial struct fDeep { }
            internal static @string Tie(this ref fDeep d) => "deep";
            partial struct fWrap {
                /*embed*/ internal fDeep fDeep;
            }
            partial struct fAll {
                /*embed*/ internal fBoth fBoth;
                /*embed*/ internal fWrap fWrap;
            }
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

    private static readonly Lazy<Dictionary<string, string>> s_generated = new(() =>
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("dtest",
            [CSharpSyntaxTree.ParseText(Source, options, path: FilePath), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\dtest\obj\dtest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        return output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .GroupBy(tree => System.IO.Path.GetFileName(tree.FilePath), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => string.Join("\n", group.Select(tree => tree.ToString())), StringComparer.Ordinal);
    });

    // The generated part of one struct: the file whose name ends `.<structName>.g.cs`.
    private static string GeneratedFor(string structName)
    {
        string[] parts = s_generated.Value.Where(pair => pair.Key.EndsWith($".{structName}.g.cs", StringComparison.Ordinal)).Select(pair => pair.Value).ToArray();

        Assert.AreEqual(1, parts.Length, $"expected one generated part for {structName}; files: {string.Join(", ", s_generated.Value.Keys)}");

        return parts[0];
    }

    [TestMethod]
    public void ABoxPrimaryBelowTheDirectEmbedIsPromotedAtEveryLevel()
    {
        // Root is declared once, three embeds below aStmt; Self twice, and aDefault's shadows aNode's. Each type
        // above takes a shim that descends ONE level, into its direct embed's box.
        //
        // RED at the base (0c553534df's generator): aBase, aStmt and aIf have neither method, and the
        // pointer adapter of a type whose chain passes aStmt's two embeds calls them on the outer box (CS1929).
        foreach ((string outer, string embed) in new[] { ("aBase", "aDefault"), ("aStmt", "aBase"), ("aIf", "aStmt") })
        {
            string generated = GeneratedFor(outer);

            StringAssert.Contains(generated, $"Root(this ж<{outer}> Ꮡtarget) => Ꮡtarget.of({outer}.Ꮡ{embed}).Root();", $"{outer} must promote Root:\n{generated}");
            StringAssert.Contains(generated, $"Self(this ж<{outer}> Ꮡtarget) => Ꮡtarget.of({outer}.Ꮡ{embed}).Self();", $"{outer} must promote Self:\n{generated}");
            Assert.IsFalse(generated.Contains($"Self(this ref {outer} "), $"a box primary has no by-ref forwarder on {outer}");
        }

        // The sibling embed's ref-receiver method promotes as it always did.
        StringAssert.Contains(GeneratedFor("aStmt"), "Flag(this ref aStmt target) => target.aSide.Flag();");
    }

    [TestMethod]
    public void TheShallowestOfTwoEmbedsForwardsTheNameAlone()
    {
        // Name sits at depth 1 through bFlat and at depth 2 through bMid: Go takes bFlat's.
        //
        // RED at the base: the name counted two embeds, whatever their depths, and neither forwarded it.
        foreach (string outer in new[] { "bBoth", "bFirst" })
        {
            string generated = GeneratedFor(outer);

            StringAssert.Contains(generated, $"Name(this ref {outer} target) => target.bFlat.Name();", $"{outer} must take bFlat's Name:\n{generated}");
            Assert.IsFalse(generated.Contains("target.bMid.Name()"), $"{outer} must not forward the deeper Name:\n{generated}");
            StringAssert.Contains(generated, "'bMid.Name' is SHADOWED", $"{outer} must say why bMid's Name is not promoted:\n{generated}");
        }

        StringAssert.Contains(GeneratedFor("bOuter"), "Name(this ref bOuter target) => target.bBoth.Name();");
    }

    [TestMethod]
    public void AShadowingBoxPrimaryIsPromotedInPlaceOfTheMethodItShadows()
    {
        // cDefault.Visit (a box primary) shadows cNode.Visit (by ref). cBase holds the name as a shim, so cStmt
        // must take a shim too.
        //
        // RED at the base: cStmt forwarded cNode's Visit by ref through cBase, which has no such method (CS1929).
        StringAssert.Contains(GeneratedFor("cBase"), "Visit(this ж<cBase> Ꮡtarget) => Ꮡtarget.of(cBase.ᏑcDefault).Visit();");

        string stmt = GeneratedFor("cStmt");

        StringAssert.Contains(stmt, "Visit(this ж<cStmt> Ꮡtarget) => Ꮡtarget.of(cStmt.ᏑcBase).Visit();", "cStmt must promote the shadowing Visit:\n" + stmt);
        Assert.IsFalse(stmt.Contains("Visit(this ref cStmt "), "the shadowed by-ref Visit must not be promoted:\n" + stmt);
    }

    [TestMethod]
    public void ANameAnEmbedAnnihilatesIsStillPromotedFromAShallowerSibling()
    {
        // dPair reaches Tag twice at one depth and has none. dAll reaches it once at depth 1 (dTop) and twice at
        // depth 2: Go takes dTop's.
        //
        // RED at the base: a name annihilated inside ONE embed was withheld from every embed.
        string pair = GeneratedFor("dPair");

        Assert.IsFalse(pair.Contains(" Tag(this "), "dPair has no Tag:\n" + pair);

        string all = GeneratedFor("dAll");

        StringAssert.Contains(all, "Tag(this ref dAll target) => target.dTop.Tag();", "dAll must take dTop's Tag:\n" + all);
        StringAssert.Contains(all, "'dPair.Tag' is SHADOWED", "dAll must say why dPair's Tag is not promoted:\n" + all);
    }

    [TestMethod]
    public void TheForwarderTakesTheSignatureOfTheClosestDeclaration()
    {
        // Inside eFork, Pick() sits at depth 1 (eNear) and Pick(nint) at depth 2 (eFar), and the walk meets
        // eFar's first.
        //
        // RED at the base: eOuter's forwarder had eFar's signature and called eFork.Pick(n), which eFork does
        // not have (CS1929).
        string outer = GeneratedFor("eOuter");

        StringAssert.Contains(outer, "Pick(this ref eOuter target) => target.eFork.Pick();", "eOuter must take eNear's signature:\n" + outer);
        Assert.IsFalse(outer.Contains("nint n"), "eOuter must not take eFar's signature:\n" + outer);
    }

    [TestMethod]
    public void ATieAtTheShallowestDepthPromotesNothing()
    {
        // Tie sits three times at depth 2 of fAll: twice through fBoth, once through fWrap. Nothing is promoted,
        // before the fix and after it: that half is the control.
        //
        // RED at the base in the stated REASON only: fWrap's Tie was reported as ambiguous inside the embed
        // itself, though fWrap reaches it once; the set of names fBoth annihilates was applied to every embed.
        string all = GeneratedFor("fAll");

        Assert.IsFalse(all.Contains(" Tie(this "), "a name that ties at its shallowest depth is not promoted:\n" + all);
        StringAssert.Contains(all, "'fBoth.Tie' is AMBIGUOUS inside the embed itself");
        StringAssert.Contains(all, "'fWrap.Tie' promotion is AMBIGUOUS across embeds");
        StringAssert.Contains(GeneratedFor("fWrap"), "Tie(this ref fWrap target) => target.fDeep.Tie();");
    }
}
