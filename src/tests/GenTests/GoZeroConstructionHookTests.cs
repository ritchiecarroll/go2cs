// GoZeroConstructionHookTests.cs - Gbtc
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
/// Guards the zero-construction hook (trim stage 3b, docs/PLAN-golib-full-trim.md section 9.6; golib's
/// IGoZeroConstructed): a struct whose constructor builds what <c>default</c> skips names its own Go zero on its
/// generated part, so golib never asks a type parameter for its constructor. A GENERIC one included: that is what the
/// GoZero factory registration cannot give it, and the residual the GoZero ruling stated. A struct whose constructor
/// builds nothing carries no hook, so its zero stays <c>default</c>.
/// </summary>
[TestClass]
public class GoZeroConstructionHookTests
{
    private const string Path = @"C:\go2cs\src\core\ctest\ctest.cs";

    private const string Package =
        """
        namespace go;

        [GoPackage("ctest")]
        public static partial class ctest_package
        {
            partial struct Needy { internal array<nint> a; }

            partial struct GenericNeedy<T> { internal array<T> vals; }

            partial struct Plain { internal nint x; }

            partial struct Padded { internal nint x; internal array<byte> _; }

            [GoType("Needy")] partial struct WrappedNeedy;
        }
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        """;

    private static string PartFor(string name)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("ctest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\ctest\obj\ctest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        string[] generated = CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGenerators(compilation).GetRunResult().GeneratedTrees.Select(tree => tree.ToString()).ToArray();

        return generated.Single(source => source.Contains($" partial struct {name}"));
    }

    [TestMethod]
    public void ANeedyStructNamesItsOwnZero()
    {
        string part = PartFor("Needy");

        StringAssert.Contains(part, "global::go.IGoZeroConstructed", "the needy struct implements the hook");
        StringAssert.Contains(part, "object global::go.IGoZeroConstructed.GoZeroNew() => new Needy();", "its zero is its own constructor");
    }

    [TestMethod]
    public void AGenericNeedyStructNamesItsOwnClosedZero()
    {
        string part = PartFor("GenericNeedy<T>");

        StringAssert.Contains(part, "global::go.IGoZeroConstructed", "a GENERIC needy struct implements the hook too, which no factory registration could give it");
        StringAssert.Contains(part, "object global::go.IGoZeroConstructed.GoZeroNew() => new GenericNeedy<T>();", "it names its own closed instantiation");
    }

    [TestMethod]
    public void AStructWhoseOnlyBuiltFieldIsBlankStillNamesItsZero()
    {
        // IsNeedy (the factory rule) leaves an unobservable blank field out; the hook follows what the constructor builds.
        StringAssert.Contains(PartFor("Padded"), "object global::go.IGoZeroConstructed.GoZeroNew() => new Padded();");
    }

    [TestMethod]
    public void ADefinedTypeOverAStructNamesItsOwnZero()
    {
        string part = PartFor("WrappedNeedy");

        StringAssert.Contains(part, "global::go.IGoZeroConstructed", "the wrapper implements the hook");
        StringAssert.Contains(part, "object global::go.IGoZeroConstructed.GoZeroNew() => new WrappedNeedy();", "its zero is its constructed underlying");
    }

    [TestMethod]
    public void AStructWhoseConstructorBuildsNothingHasNoHook()
    {
        Assert.IsFalse(PartFor("Plain").Contains("IGoZeroConstructed"), "its zero is default, so golib asks nothing of it");
    }
}
