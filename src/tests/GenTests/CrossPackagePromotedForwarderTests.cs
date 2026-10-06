// CrossPackagePromotedForwarderTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Go promotes an embedded struct's exported methods whatever package declares the embed, and the
/// run-time method set is read off emitted extension methods — so a struct embedding another
/// package's struct had NONE of its methods at run time (<c>struct{ time.Time }</c>: NumMethod 0).
/// The TypeGenerator now mints those promotions into a public SIBLING class,
/// <c>{pkg}ᴛ{Struct}ᴛxpkg</c>, and leaves the package class byte for byte what it was.
/// </summary>
/// <remarks>
/// These tests run the REAL TypeGenerator over a chain of assemblies: a stub golib, package
/// <c>xa</c>, package <c>xb</c> (which embeds xa's types and carries, hand-written, the sibling class
/// its own generator run would have minted), and the consumer under test. Every run asserts the
/// generator neither threw nor reported a diagnostic, except the one arm that asks for GO2CS0001.
/// </remarks>
[TestClass]
public class CrossPackagePromotedForwarderTests
{
    private const string GolibSource =
        """
        namespace go
        {
            public class ж<T> { public ж(T value) { } public T Value = default!; }

            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            public class GoPackageAttribute : System.Attribute
            {
                public GoPackageAttribute(string name) { }
            }

            public class GoRecvAttribute : System.Attribute { }

            public class GoEmbeddedAttribute : System.Attribute { }
        }
        """;

    // Package xa: the embedded types. Inner carries one method of every receiver shape the
    // generator distinguishes, plus the three a cross-package promotion must refuse.
    private const string PackageA =
        """
        namespace go
        {
            [GoPackage("xa")]
            public static partial class xa_package
            {
                public partial struct Inner { public int N; }

                public partial struct Foreign { }

                public partial struct Box<T> { }

                internal partial struct hidden { }

                public interface ReadCloser
                {
                    int Read();
                    void Close();
                }

                // A value-receiver method.
                public static string Name(this Inner i) => "";

                // A pointer-receiver method as the converter emits it, with RecvGenerator's twin.
                [GoRecv] public static void Set(this ref Inner i, int n) { }

                [global::System.CodeDom.Compiler.GeneratedCode("go2cs-gen", "1.0")]
                public static void Set(this ж<Inner> Ꮡi, int n) => Ꮡi.Value.Set(n);

                // A pointer-receiver method emitted in the direct-ж (box) form: no value form at all.
                public static void Reset(this ж<Inner> Ꮡi) { }

                // Inner also "implements" ReadCloser's names, for the composite-interface arm.
                public static int Read(this Inner i) => 0;

                public static void Close(this Inner i) { }

                // A method and a package-level FUNCTION sharing one name (time's Unix/Date/After).
                public static long Unix(this Inner i) => 0;

                public static Inner Unix(long sec, long nsec) => default;

                // Refused across packages: an unexported Go name (public in C# or not), and an
                // exported one whose signature names a type that is not public.
                public static void lower(this Inner i) { }

                internal static void secret(this Inner i) { }

                internal static hidden Leak(this Inner i) => default;

                public static int M(this Foreign f) => 0;

                public static int Get<T>(this Box<T> b) => 0;
            }
        }
        """;

    // Package xb: Mid embeds xa.Inner. Its sibling class is what xb's own generator run mints for
    // Mid, spelled as EmitCrossPackage emits it; its `ʗInner` field is the inline embed marker
    // the TypeGenerator gives every embedding struct. Both are part of the metadata a consumer of
    // xb reads, and neither can come from running the generator here (its output is never
    // compiled into this fixture).
    private const string PackageB =
        """
        namespace go
        {
            [GoPackage("xb")]
            public static partial class xb_package
            {
                public partial struct Mid
                {
                    public global::go.xa_package.Inner ʗInner;
                }

                public partial struct MidForeign
                {
                    public global::go.xa_package.Foreign ʗForeign;
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCode("go2cs-gen", "1.0")]
            public static class xbᴛMidᴛxpkg
            {
                public static string Name(this global::go.xb_package.Mid target) => "";
                public static string Name(this ж<global::go.xb_package.Mid> Ꮡtarget) => "";
                [global::go.GoRecv] public static void Set(this ref global::go.xb_package.Mid target, int n) { }
                public static void Set(this ж<global::go.xb_package.Mid> Ꮡtarget, int n) { }
            }

            [global::System.CodeDom.Compiler.GeneratedCode("go2cs-gen", "1.0")]
            public static class xbᴛMidForeignᴛxpkg
            {
                public static int M(this global::go.xb_package.MidForeign target) => 0;
                public static int M(this ж<global::go.xb_package.MidForeign> Ꮡtarget) => 0;
            }
        }
        """;

    private const string Consumer =
        """
        using go;

        namespace go
        {
            [GoPackage("main")]
            public static partial class main_package
            {
                [GoType] partial struct Direct
                {
                    public partial ref xa_package.Inner Inner { get; }
                }

                [GoType] partial struct PtrDirect
                {
                    public partial ref ж<xa_package.Inner> Inner { get; }
                }

                // The transitive shape: main -> xb -> xa.
                [GoType] partial struct Outer
                {
                    public partial ref xb_package.Mid Mid { get; }
                }

                // The mixed path: a same-package hop, then a cross-package one, beside a package
                // var named like a promoted method.
                [GoType] partial struct localMid
                {
                    public partial ref xa_package.Inner Inner { get; }
                }

                [GoType] partial struct Mixed
                {
                    internal partial ref localMid localMid { get; }
                }

                public static int Name = 0;

                // A field one level up shadows the promoted method of its name.
                [GoType] partial struct Shadow
                {
                    public partial ref xa_package.Inner Inner { get; }
                    public int Name;
                }

                // A composite: a struct embed beside an INTERFACE embed providing two of its names.
                [GoType] partial struct Composite
                {
                    public partial ref xa_package.Inner Inner { get; }
                    [GoEmbedded] public xa_package.ReadCloser ReadCloser;
                }

                // The same composite as the converter writes it since face lift F: the embed marked by
                // the `/*embed*/` comment, which is read from the declaration, not from an attribute.
                [GoType] partial struct CompositeMarked
                {
                    public partial ref xa_package.Inner Inner { get; }
                    /*embed*/ public xa_package.ReadCloser ReadCloser;
                }

                // The same fields with the interface as a NAMED field: no marker, no provider.
                [GoType] partial struct NamedIface
                {
                    public partial ref xa_package.Inner Inner { get; }
                    public xa_package.ReadCloser rc;
                }

                // The three shapes whose answer stays master's (a same-package method of the same
                // name somewhere in the tree).
                [GoType] partial struct local
                {
                }

                internal static int M(this local l) => 1;

                [GoType] partial struct deep
                {
                }

                internal static int M(this deep d) => 2;

                [GoType] partial struct localDeep
                {
                    internal partial ref deep deep { get; }
                }

                [GoType] partial struct S1
                {
                    public partial ref xa_package.Foreign Foreign { get; }
                    internal partial ref local local { get; }
                }

                [GoType] partial struct S3
                {
                    public partial ref xa_package.Foreign Foreign { get; }
                    internal partial ref localDeep localDeep { get; }
                }

                [GoType] partial struct S4
                {
                    public partial ref xb_package.MidForeign MidForeign { get; }
                    internal partial ref localDeep localDeep { get; }
                }

                // Generics are not served: a generic enclosing struct, and a generic embed.
                [GoType] partial struct Holder<T>
                {
                    public partial ref xa_package.Inner Inner { get; }
                }

                [GoType] partial struct HasBox
                {
                    public partial ref xa_package.Box<int> Box { get; }
                }
            }
        }
        """;

    // A consumer whose package-class stem holds the lifted-name marker.
    private const string MarkedStemConsumer =
        """
        using go;

        namespace go
        {
            [GoPackage("aᴛb")]
            public static partial class aᴛb_package
            {
                [GoType] partial struct Direct
                {
                    public partial ref xa_package.Inner Inner { get; }
                }
            }
        }
        """;

    private sealed record Run(Dictionary<string, string> Sources, IReadOnlyList<Diagnostic> Diagnostics);

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    // Compiles golib, xa and xb in turn, then runs the TypeGenerator over the consumer. With
    // compilationReferences the consumer sees xa and xb as COMPILATIONS (an IDE's project-to-project
    // view) rather than as emitted images (an MSBuild build's).
    private static Run Generate(string consumerSource, bool compilationReferences = false)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);
        List<MetadataReference> references = [.. CoreReferences()];

        foreach ((string name, string source) in new[] { ("xp-golib", GolibSource), ("xp-xa", PackageA), ("xp-xb", PackageB) })
        {
            CSharpCompilation upstream = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source, parseOptions)], references, libraryOptions);

            using MemoryStream image = new();
            EmitResult emitted = upstream.Emit(image);
            Assert.IsTrue(emitted.Success, $"{name} must emit clean: {string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");

            references.Add(compilationReferences && name != "xp-golib" ? upstream.ToMetadataReference() : MetadataReference.CreateFromImage(image.ToArray()));
        }

        CSharpCompilation consumer = CSharpCompilation.Create("xp-main", [CSharpSyntaxTree.ParseText(consumerSource, parseOptions)], references, libraryOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator());
        GeneratorDriverRunResult result = driver.RunGenerators(consumer).GetRunResult();

        foreach (GeneratorRunResult each in result.Results)
            Assert.IsNull(each.Exception, $"the generator must not throw: {each.Exception}");

        return new Run(
            result.Results.SelectMany(generator => generator.GeneratedSources).ToDictionary(source => source.HintName, source => source.SourceText.ToString()),
            [.. result.Diagnostics]);
    }

    private static Run GenerateClean(string consumerSource, bool compilationReferences = false)
    {
        Run run = Generate(consumerSource, compilationReferences);
        Assert.AreEqual(0, run.Diagnostics.Count, $"the generator must report no diagnostics: {string.Join("; ", run.Diagnostics)}");
        return run;
    }

    private static string GeneratedFor(Run run, string structName)
    {
        string? key = run.Sources.Keys.FirstOrDefault(hint => hint.Contains($"_package.{structName}.g.cs"));
        Assert.IsNotNull(key, $"the TypeGenerator must generate for {structName}; got: {string.Join(", ", run.Sources.Keys)}");
        return run.Sources[key!];
    }

    // The generated text splits at the sibling class's declaration: what precedes it is the
    // struct and its package-class members, what follows is the sibling class's body.
    private static (string package, string sibling) Split(string generated, string siblingClass)
    {
        int at = generated.IndexOf($"public static class {siblingClass}", System.StringComparison.Ordinal);
        return at < 0 ? (generated, "") : (generated[..at], generated[at..]);
    }

    private const string DirectType = "global::go.main_package.Direct";

    [TestMethod]
    public void ValueEmbedMintsExportedMethodsIntoTheSiblingClass()
    {
        (string package, string sibling) = Split(GeneratedFor(GenerateClean(Consumer), "Direct"), "mainᴛDirectᴛxpkg");

        // A value-receiver method: the value form by value, and its pointer twin.
        StringAssert.Contains(sibling, $"public static string Name(this {DirectType} target) => target.Inner.Name();");
        StringAssert.Contains(sibling, $"public static string Name(this ж<{DirectType}> Ꮡtarget)");

        // A pointer-receiver method through a VALUE hop is in the pointer set alone: the value
        // form mirrors its source's `this ref` and carries [GoRecv], which keeps it out of the
        // run-time value set and off the copy-binding path.
        StringAssert.Contains(sibling, $"[global::go.GoRecv] public static void Set(this ref {DirectType} target, int n) => target.Inner.Set(n);");
        StringAssert.Contains(sibling, $"public static void Set(this ж<{DirectType}> Ꮡtarget, int n)");

        // A direct-ж (box) primary through a direct value embed: the pointer shim alone.
        StringAssert.Contains(sibling, $"public static void Reset(this ж<{DirectType}> Ꮡtarget) => Ꮡtarget.of({DirectType}.ᏑInner).Reset();");
        Assert.IsFalse(sibling.Contains($"Reset(this {DirectType} target"), "a box primary through a value hop has no value form");

        // The package-function guard is same-package only: xa's `Unix` FUNCTION lives in xa's
        // class, not in the sibling class, so the METHOD of that name still promotes.
        StringAssert.Contains(sibling, $"public static long Unix(this {DirectType} target) => target.Inner.Unix();");

        // Refused: an unexported Go name (public in C# or not), and an exported one that is not public.
        foreach (string refused in new[] { "lower", "secret", "Leak" })
            Assert.IsFalse(sibling.Contains($" {refused}("), $"'{refused}' must not promote across packages");

        // Containment: the package class gains nothing, so same-package output stays what it was.
        foreach (string name in new[] { "Name(", "Set(", "Reset(", "Unix(", "Read(", "Close(", "lower(", "secret(", "Leak(" })
            Assert.IsFalse(package.Contains($" {name}"), $"'{name}' must be minted into the sibling class only, never into the package class");
    }

    [TestMethod]
    public void EverySiblingClassHoldsOnlyQualifiedReceiversOfItsOwnStruct()
    {
        Run run = GenerateClean(Consumer);
        int siblings = 0;

        foreach (string structName in new[] { "Direct", "PtrDirect", "Outer", "Mixed", "localMid", "Shadow", "Composite", "NamedIface" })
        {
            (_, string sibling) = Split(GeneratedFor(run, structName), $"mainᴛ{structName}ᴛxpkg");
            Assert.AreNotEqual("", sibling, $"{structName} must have a sibling class");
            siblings++;

            foreach (string line in sibling.Split('\n').Where(line => line.Contains(" static ") && line.Contains("(this ")))
            {
                string receiver = line[(line.IndexOf("(this ", System.StringComparison.Ordinal) + "(this ".Length)..];

                Assert.IsTrue(
                    receiver.StartsWith($"global::go.main_package.{structName} target", System.StringComparison.Ordinal) ||
                    receiver.StartsWith($"ref global::go.main_package.{structName} target", System.StringComparison.Ordinal) ||
                    receiver.StartsWith($"ж<global::go.main_package.{structName}> Ꮡtarget", System.StringComparison.Ordinal),
                    $"a forwarder in {structName}'s sibling class must take its own struct, qualified: {line.Trim()}");

                // No by-ref receiver without [GoRecv]: that shape reads as a VALUE-set method.
                if (receiver.StartsWith("ref ", System.StringComparison.Ordinal))
                    StringAssert.Contains(line, "[global::go.GoRecv] ", $"a by-ref forwarder must carry [GoRecv]: {line.Trim()}");
            }
        }

        Assert.AreEqual(8, siblings);
    }

    [TestMethod]
    public void PointerHopForwardsAPointerReceiverMethodByValue()
    {
        (_, string sibling) = Split(GeneratedFor(GenerateClean(Consumer), "PtrDirect"), "mainᴛPtrDirectᴛxpkg");

        // Through an embedded POINTER the method is in Go's VALUE set (net/http's breakableConn
        // shape: `struct{ *sync.Mutex }` has Lock/Unlock by value). The forwarder takes its
        // receiver by value — the call goes through the pointer a copy shares — and carries no
        // [GoRecv].
        StringAssert.Contains(sibling, "public static void Set(this global::go.main_package.PtrDirect target, int n) => target.Inner.Value.Set(n);");
        StringAssert.Contains(sibling, "public static void Reset(this global::go.main_package.PtrDirect target) => target.Inner.Reset();");
        Assert.IsFalse(sibling.Contains("this ref "), "no forwarder through a pointer hop is by-ref");
        Assert.IsFalse(sibling.Contains("GoRecv"), "no forwarder through a pointer hop is pointer-set-only");
    }

    [TestMethod]
    public void PromotionCrossesTwoPackagesThroughTheEmbedsOwnSiblingClass()
    {
        (_, string sibling) = Split(GeneratedFor(GenerateClean(Consumer), "Outer"), "mainᴛOuterᴛxpkg");

        // Outer{xb.Mid{xa.Inner}}: xa's methods reach Outer through the forwarders xb minted for
        // Mid, and a forwarder's own [GoRecv] says its method is a pointer-receiver one.
        StringAssert.Contains(sibling, "public static string Name(this global::go.main_package.Outer target) => target.Mid.Name();");
        StringAssert.Contains(sibling, "[global::go.GoRecv] public static void Set(this ref global::go.main_package.Outer target, int n) => target.Mid.Set(n);");
    }

    [TestMethod]
    public void MixedPathGoesToTheSiblingClassBesideAClashingPackageVar()
    {
        (string package, string sibling) = Split(GeneratedFor(GenerateClean(Consumer), "Mixed"), "mainᴛMixedᴛxpkg");

        // Mixed{localMid{xa.Inner}} beside `var Name`: a forwarder named Name in main_package
        // would be CS0102 against the var. Any crossing hop sends the forwarder to the sibling class.
        StringAssert.Contains(sibling, "public static string Name(this global::go.main_package.Mixed target) => target.localMid.Name();");
        StringAssert.Contains(sibling, "[global::go.GoRecv] public static void Set(this ref global::go.main_package.Mixed target, int n) => target.localMid.Set(n);");
        Assert.IsFalse(package.Contains(" Name("), "the package class must not gain a method named like the package var");

        // PINNED GAP (a MISS, never an over-claim): a direct-ж primary two value hops down is
        // not forwarded — the same-package path has the same limit (it serves a DIRECT value
        // embed only). Go has (*Mixed).Reset.
        Assert.IsFalse(sibling.Contains(" Reset("), "the two-hop box-primary gap moved: update this pin and the seat's residual list");
    }

    [TestMethod]
    public void ANameThatIsNotUniqueAcrossTheTreeIsNotMinted()
    {
        Run run = GenerateClean(Consumer);

        // A field one level up shadows the promoted method of its name (Go's depth rule).
        (_, string shadow) = Split(GeneratedFor(run, "Shadow"), "mainᴛShadowᴛxpkg");
        Assert.IsFalse(shadow.Contains(" Name("), "a field named Name shadows the embed's Name method");
        StringAssert.Contains(shadow, " Unix(this global::go.main_package.Shadow target)", "the other methods still promote");

        // An embedded INTERFACE provides its methods at depth 1: the struct embed's methods of
        // those names are ambiguous in Go (`struct{ bytes.Buffer; io.ReadWriteCloser }`).
        (_, string composite) = Split(GeneratedFor(run, "Composite"), "mainᴛCompositeᴛxpkg");
        Assert.IsFalse(composite.Contains(" Read("), "Read is provided twice at depth 1: ambiguous, not promoted");
        Assert.IsFalse(composite.Contains(" Close("), "Close is provided twice at depth 1: ambiguous, not promoted");
        StringAssert.Contains(composite, " Name(this global::go.main_package.Composite target)", "a name the interface does not provide still promotes");

        // The comment-marked twin reads the same: the `/*embed*/` comment makes the interface a provider.
        (_, string marked) = Split(GeneratedFor(run, "CompositeMarked"), "mainᴛCompositeMarkedᴛxpkg");
        Assert.IsFalse(marked.Contains(" Read("), "the comment-marked embed provides Read too: ambiguous, not promoted");
        Assert.IsFalse(marked.Contains(" Close("), "the comment-marked embed provides Close too: ambiguous, not promoted");
        StringAssert.Contains(marked, " Name(this global::go.main_package.CompositeMarked target)");

        // The control: the same interface as a NAMED field is no provider, and it is the
        // converter's [GoEmbedded] marker that tells the two apart.
        (_, string named) = Split(GeneratedFor(run, "NamedIface"), "mainᴛNamedIfaceᴛxpkg");
        StringAssert.Contains(named, " Read(this global::go.main_package.NamedIface target)");
        StringAssert.Contains(named, " Close(this global::go.main_package.NamedIface target)");
    }

    [TestMethod]
    public void CollisionWithASamePackageMethodKeepsTheExistingAnswer()
    {
        Run run = GenerateClean(Consumer);

        // KNOWN DIVERGENCES, pinned. Where a cross-package method meets a same-package method of
        // the same name, the cross one is not minted and the same-package forwarder is left as
        // it was:
        //   S1  {xa.Foreign; local}            Go: ambiguous, no M.     Here: local's M stays.
        //   S3  {xa.Foreign; localDeep{deep}}  Go: Foreign.M.           Here: deep's M (membership right, dispatch wrong).
        //   S4  {xb.MidForeign; localDeep}     Go: ambiguous, no M.     Here: deep's M stays.
        foreach ((string structName, string hop) in new[] { ("S1", "local"), ("S3", "localDeep"), ("S4", "localDeep") })
        {
            string generated = GeneratedFor(run, structName);

            Assert.IsFalse(generated.Contains("ᴛxpkg"), $"{structName} must mint no cross-package forwarder");
            StringAssert.Contains(generated, $"public static int M(this {structName} target) => target.{hop}.M();",
                $"{structName} keeps its same-package forwarder; a change here is a behavior change to re-measure against Go");
        }
    }

    [TestMethod]
    public void GenericsAreNotServedAndABoxedEmbedIsNotMistakenForOne()
    {
        Run run = GenerateClean(Consumer);

        // A generic enclosing struct and a generic embed mint nothing (their forwarders would
        // need type parameters and constraints re-declared).
        Assert.IsFalse(GeneratedFor(run, "Holder_T_").Contains("ᴛxpkg"), "a generic enclosing struct mints no cross-package forwarder");
        Assert.IsFalse(GeneratedFor(run, "HasBox").Contains("ᴛxpkg"), "a generic embed mints no cross-package forwarder");

        // The test is on the SYMBOL: `ж<xa.Inner>` is a pointer embed, not a generic one.
        StringAssert.Contains(GeneratedFor(run, "PtrDirect"), "public static class mainᴛPtrDirectᴛxpkg");
    }

    [TestMethod]
    public void CompilationReferenceViewMintsNoSiblingClass()
    {
        Run run = GenerateClean(Consumer, compilationReferences: true);

        // NAMED RESIDUAL, pinned. When the embed's package is referenced as a COMPILATION (an
        // IDE's project-to-project view) its struct is found as SYNTAX and takes the same-package
        // path, exactly as before this class existed. Run-time method sets come from an MSBuild
        // build, where every cross-package embed is metadata.
        foreach (string source in run.Sources.Values)
            Assert.IsFalse(source.Contains("ᴛxpkg"), "the CompilationReference view moved: re-measure it against the metadata view");

        StringAssert.Contains(GeneratedFor(run, "Direct"), "internal static string Name(this Direct target) => target.Inner.Name();");
    }

    [TestMethod]
    public void PackageStemHoldingTheLiftedNameMarkerIsReported()
    {
        Run run = Generate(MarkedStemConsumer);

        // The sibling class's name is unique because a package stem never holds `ᴛ`. One that
        // does is reported where the class comes into being, once per struct.
        Assert.AreEqual(1, run.Diagnostics.Count, string.Join("; ", run.Diagnostics));
        Assert.AreEqual("GO2CS0001", run.Diagnostics[0].Id);
        StringAssert.Contains(run.Diagnostics[0].GetMessage(), "aᴛbᴛDirectᴛxpkg");
        StringAssert.Contains(GeneratedFor(run, "Direct"), "public static class aᴛbᴛDirectᴛxpkg");
    }
}
