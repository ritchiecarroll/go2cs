// GoTypeOpsSourceTests.cs - Gbtc
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
/// Guards the generated operations face (golib's IGoTypeOpsSource; trim stage 3c-2b, docs/PLAN-golib-full-trim.md
/// section 9): every struct and struct-kind named type names exactly <c>GoTypeOps&lt;itself&gt;</c> and
/// nothing more (a pointer is a reference type, whose operations golib's fallback loads under Native AOT); a class wrapper
/// (a named pointer type) names none.
/// </summary>
/// <remarks>
/// THE HAZARD GUARD (COORD's ruling on section 9's hazard rule): a face member that named its own type inside its own
/// type -- <c>GoTypeOps&lt;ж&lt;ж&lt;S&gt;&gt;&gt;</c>, <c>GoTypeOps&lt;S&lt;S&lt;T&gt;&gt;&gt;</c> -- would make ILC expand one more
/// generic level per level, without bound (a 70-line probe ground 30+ CPU-minutes on one). <see cref="EveryFaceMemberNamesOnlyItself"/>
/// reads every GoTypeOps instantiation the generated parts name and fails on any other argument.
/// </remarks>
[TestClass]
public class GoTypeOpsSourceTests
{
    private const string Path = @"C:\go2cs\src\core\otest\otest.cs";

    private const string Package =
        """
        namespace go;

        [GoPackage("otest")]
        public static partial class otest_package
        {
            partial struct Plain { internal nint x; }

            partial struct Generic<T> { internal T value; }

            partial struct Needy { internal array<nint> a; }

            [GoType("Plain")] partial struct WrappedPlain;

            [GoType("[]nint")] partial struct Ints;

            [GoType("ж<Plain>")] partial class PlainPtr;

            partial struct Unpointed { internal nint x; }

            // The compilation spells a pointer to each face-bearing type (TypeOpsScope); Unpointed's is never spelled.
            internal static void Points(ж<Plain> a, ж<Generic<nint>> b, ж<WrappedPlain> c, ж<Ints> d) { _ = @new<Needy>(); }
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

    private static string[] GeneratedParts()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("otest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\otest\obj\otest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        return [.. CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGenerators(compilation).GetRunResult().GeneratedTrees.Select(tree => tree.ToString())];
    }

    private static string PartFor(string declaration) => GeneratedParts().Single(source => source.Contains(declaration));

    private static void AssertFace(string part, string self)
    {
        StringAssert.Contains(part, "global::go.IGoTypeOpsSource", $"{self} implements the operations face");
        StringAssert.Contains(part, $"global::go.IGoTypeOps global::go.IGoTypeOpsSource.TypeOps => global::go.GoTypeOps<{self}>.Instance;", $"{self} names its own operations");
        Assert.IsFalse(part.Contains("GoTypeOps<global::go.ж<"), $"{self} names no pointer's ops: a reference type's are the fallback's (the measured shape)");
    }

    [TestMethod]
    public void AStructNamesItsOwnOps() => AssertFace(PartFor(" partial struct Plain"), "Plain");

    [TestMethod]
    public void AGenericStructNamesItsOwnClosedOps() => AssertFace(PartFor(" partial struct Generic<T>"), "Generic<T>");

    [TestMethod]
    public void ANeedyStructCarriesTheFaceBesideItsZeroHook()
    {
        string part = PartFor(" partial struct Needy");

        AssertFace(part, "Needy");
        StringAssert.Contains(part, "global::go.IGoZeroConstructed", "the zero hook stays, in the same generated block");
    }

    [TestMethod]
    public void ANamedStructAndSliceTypeNameTheirOwnOps()
    {
        AssertFace(PartFor(" partial struct WrappedPlain"), "WrappedPlain");
        AssertFace(PartFor(" partial struct Ints"), "Ints");
    }

    // The face goes only where the compilation spells a pointer to the type (ж<S> or @new<S>): a type no code points to
    // would otherwise pull the whole ж<S> machinery into a Native AOT executable (measured +41% on an fmt/reflect program
    // when every struct carried it; the stage-3 table, rows D and E).
    [TestMethod]
    public void ANeverPointedToStructCarriesNoFace()
    {
        string part = PartFor(" partial struct Unpointed");

        Assert.IsFalse(part.Contains("IGoTypeOpsSource"), "no pointer to Unpointed is spelled, so it carries no face");
        Assert.IsFalse(part.Contains("GoTypeOps<"), "and names no operations");
    }

    [TestMethod]
    public void ANamedPointerTypeCarriesNoFace()
    {
        Assert.IsFalse(PartFor(" partial class PlainPtr").Contains("IGoTypeOpsSource"), "a pointer's source would name a pointer to a pointer, and so on");
    }

    [TestMethod]
    public void EveryFaceMemberNamesOnlyItself()
    {
        int checkedFaces = 0;

        foreach (string part in GeneratedParts())
        {
            foreach (string declaration in new[] { "Plain", "Generic<T>", "Needy", "WrappedPlain", "Ints" })
            {
                if (!part.Contains($" partial struct {declaration}") || !part.Contains("IGoTypeOpsSource"))
                    continue;

                foreach (string argument in GoTypeOpsArguments(part))
                    Assert.AreEqual(declaration, argument, $"{declaration}'s face names GoTypeOps<{argument}>: only itself, never a level over it");

                checkedFaces++;
            }
        }

        Assert.AreEqual(5, checkedFaces, "the guard read every face it exists for");
    }

    [TestMethod]
    public void TheHazardGuardNamesASelfNestingMember()
    {
        // The guard's own control: a face that named a pointer to a pointer to itself is refused by name.
        string planted = "global::go.IGoTypeOps global::go.IGoTypeOpsSource.TypeOps => global::go.GoTypeOps<global::go.ж<global::go.ж<Plain>>>.Instance;";

        string[] arguments = [.. GoTypeOpsArguments(planted)];

        CollectionAssert.AreEqual(new[] { "global::go.ж<global::go.ж<Plain>>" }, arguments);
        Assert.AreNotEqual("Plain", arguments[0], "the planted level is not one the guard admits");
    }

    // Every type argument a part gives GoTypeOps<...>, balanced across nested angle brackets.
    private static IEnumerable<string> GoTypeOpsArguments(string source)
    {
        const string marker = "global::go.GoTypeOps<";

        for (int at = source.IndexOf(marker, StringComparison.Ordinal); at >= 0; at = source.IndexOf(marker, at + 1, StringComparison.Ordinal))
        {
            int start = at + marker.Length, depth = 1, i = start;

            for (; i < source.Length && depth > 0; i++)
            {
                if (source[i] == '<')
                    depth++;
                else if (source[i] == '>')
                    depth--;
            }

            yield return source.Substring(start, i - start - 1);
        }
    }
}
