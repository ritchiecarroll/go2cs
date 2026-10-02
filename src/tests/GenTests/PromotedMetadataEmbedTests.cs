// PromotedMetadataEmbedTests.cs - Gbtc
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
/// A `-tests` white-box test struct can EMBED a production type by pointer — net's
/// dnsclient_unix_test.go declares <c>resolvConfTest</c> over <c>*resolverConfig</c> — and under
/// the reference test model that embedded type is METADATA in the test compilation (the test
/// assembly references the production project; it never recompiles its sources). Go promotes the
/// embed's unexported fields AND methods there, because the test file IS the same Go package; the
/// friend grant (InternalsVisibleTo) is what projects that same-package visibility into C#. The
/// TypeGenerator's metadata fallback read only PUBLIC fields (correct for a genuine cross-package
/// embed, where Go itself hides unexported members) and had NO metadata method harvest at all, so
/// promotion did not happen and every promoted selection the converter emitted was a missing
/// member (net cgo-off Linux build: CS0117/CS1061/CS1929 ×8, one file, one type).
/// </summary>
/// <remarks>
/// These tests run the REAL TypeGenerator over the two-assembly shape. The discriminator under
/// test is Go's own promotion rule projected through existing metadata: a member is promoted when
/// it is accessible to this compilation AND (it is public, or the embedding struct's containing
/// class carries the same <c>[GoPackage]</c> identity as the embedded type's). Cross-package
/// method promotion stays converter territory (the explicit-hop emission in convSelectorExpr) —
/// the generator must NOT start minting cross-package forwarders.
/// </remarks>
[TestClass]
public class PromotedMetadataEmbedTests
{
    // The minimal production surface: the box, the attributes the generator matches by full name,
    // and the friend grant the real test model mints via InternalsVisibleTo. resolverConfig
    // mirrors net's: an INTERNAL struct with internal fields, an internal direct-ж (box) primary,
    // and internal [GoRecv]-style ref-receiver methods. Server is the cross-package control: a
    // public struct with a public and an internal field, and a public and an internal method.
    private const string ProductionSource =
        """
        using System.Runtime.CompilerServices;

        [assembly: InternalsVisibleTo("promo-test")]

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

            [GoPackage("net")]
            public static partial class net_package
            {
                internal partial struct resolverConfig
                {
                    internal long lastChecked;
                    internal int initOnce;
                }

                public partial struct Server
                {
                    public int Port;
                    internal int secret;
                }

                internal static void init(this ж<resolverConfig> Ꮡconf) { }

                internal static bool tryAcquireSema(this ref resolverConfig conf) => true;

                internal static void releaseSema(this ref resolverConfig conf) { }

                // The pointer TWINS RecvGenerator compiles beside every [GoRecv] ref-receiver method in
                // the real production assembly, spelled as its ReceiverMethodTemplate emits them. They
                // are part of the metadata the test compilation reads; without them this fixture could
                // not see a harvest that counts one Go method twice (net's linux CS1929, 2026-09-23).
                [global::System.CodeDom.Compiler.GeneratedCode("go2cs-gen", "1.0")]
                internal static bool tryAcquireSema(this ж<resolverConfig> Ꮡconf) => Ꮡconf.Value.tryAcquireSema();

                [global::System.CodeDom.Compiler.GeneratedCode("go2cs-gen", "1.0")]
                internal static void releaseSema(this ж<resolverConfig> Ꮡconf) => Ꮡconf.Value.releaseSema();

                // A method and a package-level FUNCTION sharing one name — legal in Go
                // (different scopes: net's LookupHost function vs (*Resolver).LookupHost).
                // The forwarder for the METHOD must be suppressed, or it shadows every bare
                // call of the FUNCTION inside the test class.
                internal static bool lookupColliding(this ref resolverConfig conf) => true;

                internal static bool lookupColliding(string host) => true;

                public static void ServePublic(this ref Server s) { }

                internal static void serveInternal(this ref Server s) { }
            }
        }
        """;

    // The white-box bridge class carries the SAME [GoPackage("net")] identity as production —
    // that is the same-Go-package statement. The external-test class is a DIFFERENT Go package
    // ("net_test"), so despite compiling in the same friend assembly its embeds promote only
    // what Go promotes cross-package: exported fields.
    private const string TestSource =
        """
        using go;

        namespace go
        {
            [GoPackage("net")]
            public static partial class net_internal_test_package
            {
                [GoType] internal partial struct resolvConfTest
                {
                    internal string dir;
                    internal partial ref ж<global::go.net_package.resolverConfig> resolverConfig { get; }
                }
            }

            [GoPackage("net_test")]
            public static partial class net_test_package
            {
                [GoType] internal partial struct serverProbe
                {
                    internal partial ref ж<global::go.net_package.Server> Server { get; }
                }
            }
        }
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    private static Dictionary<string, string> RunTypeGeneratorOverFriendShape()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation production = CSharpCompilation.Create(
            "promo-production",
            [CSharpSyntaxTree.ParseText(ProductionSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = production.Emit(image);
        Assert.IsTrue(emitted.Success, $"production compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");
        image.Position = 0;

        CSharpCompilation test = CSharpCompilation.Create(
            "promo-test",
            [CSharpSyntaxTree.ParseText(TestSource, parseOptions)],
            CoreReferences().Append(MetadataReference.CreateFromImage(image.ToArray())),
            libraryOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator());
        driver = driver.RunGenerators(test);

        return driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());
    }

    private static string GeneratedFor(Dictionary<string, string> sources, string structName)
    {
        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains(structName));
        Assert.IsNotNull(key, $"the TypeGenerator must generate for {structName}; got: {string.Join(", ", sources.Keys)}");
        return sources[key!];
    }

    [TestMethod]
    public void WhiteBoxEmbedOfProductionTypePromotesInternalFields()
    {
        string generated = GeneratedFor(RunTypeGeneratorOverFriendShape(), "resolvConfTest");

        // The promoted instance accessor and its static field-reference sibling — the exact
        // members the converter's emission binds (`conf.lastChecked = …`,
        // `conf.of(resolvConfTest.ᏑinitOnce)`).
        StringAssert.Contains(generated, "ref instance.resolverConfig.Value.lastChecked",
            "an internal field of the same-Go-package metadata embed must promote a Ꮡ field reference");
        StringAssert.Contains(generated, "lastChecked => ref resolverConfig.Value.lastChecked",
            "an internal field of the same-Go-package metadata embed must promote an instance accessor");
        StringAssert.Contains(generated, "ᏑinitOnce",
            "every promoted field carries its Ꮡ reference accessor");
    }

    [TestMethod]
    public void WhiteBoxEmbedOfProductionTypePromotesInternalMethods()
    {
        string generated = GeneratedFor(RunTypeGeneratorOverFriendShape(), "resolvConfTest");

        // The [GoRecv]-style ref-receiver methods forward through the deref'd embed hop; the
        // direct-ж (box) primary binds the embed's box itself, no `.Value`.
        StringAssert.Contains(generated, "tryAcquireSema(this ref resolvConfTest target",
            "an internal ref-receiver method of the same-Go-package metadata embed must promote a value forwarder");
        StringAssert.Contains(generated, "target.resolverConfig.Value.tryAcquireSema()",
            "the ref-receiver forwarder descends through the pointer embed's deref'd hop");
        StringAssert.Contains(generated, "releaseSema",
            "every accessible same-package method promotes");
        StringAssert.Contains(generated, "target.resolverConfig.init()",
            "a direct-ж (box) primary binds the embed's box, not the deref'd value");

        // The collision control: a promoted method whose name a package-level FUNCTION also
        // carries must NOT be minted — the forwarder would live in the test class and shadow
        // the `using static` import for every bare `lookupColliding(host)` call (net's 54
        // CS1501s on LookupHost/LookupIP/… when lookupCustomResolver embeds *Resolver).
        Assert.IsFalse(generated.Contains("lookupColliding"),
            "a forwarder colliding with a package-level function must be suppressed — it shadows the bare function call");
    }

    [TestMethod]
    public void PointerTwinOfARefReceiverMethodIsNotASecondMethod()
    {
        string generated = GeneratedFor(RunTypeGeneratorOverFriendShape(), "resolvConfTest");

        // A [GoRecv] ref-receiver method reaches the test compilation as TWO metadata members -- the
        // method and RecvGenerator's pointer twin -- and a pointer embed harvests both. Counted as two
        // occurrences at one depth they read as a name ANNIHILATED inside the embed, and the forwarder
        // was withheld: net's linux test build, CS1929 at dnsclient_unix_test.cs:420/:422
        // (tryAcquireSema/releaseSema), while init -- a box primary with no twin -- promoted.
        //
        // RED at the base (1719e3b87f's generator): the ambiguity comment is emitted and
        // the value forwarder is not.
        Assert.IsFalse(generated.Contains("AMBIGUOUS inside the embed"),
            "a method and its RecvGenerator pointer twin are one Go method, never an annihilated pair");
        StringAssert.Contains(generated, "tryAcquireSema(this ref resolvConfTest target",
            "the ref-receiver method must promote despite its pointer twin in metadata");
        StringAssert.Contains(generated, "releaseSema(this ref resolvConfTest target",
            "the ref-receiver method must promote despite its pointer twin in metadata");
        StringAssert.Contains(generated, "target.resolverConfig.init()",
            "the box primary (no twin) keeps promoting as before");
    }

    [TestMethod]
    public void CrossPackageEmbedPromotesExportedMembersOnlyAndMintsIntoTheSiblingClass()
    {
        string generated = GeneratedFor(RunTypeGeneratorOverFriendShape(), "serverProbe");

        // The external-test class is Go package "net_test": Go promotes only Server's exported
        // members there, friend grant or not.
        StringAssert.Contains(generated, "ᏑPort",
            "an exported field of a cross-package metadata embed promotes (the pre-existing metadata path)");
        Assert.IsFalse(generated.Contains("secret"),
            "an unexported field must NOT promote across Go packages — the friend grant is not a Go visibility rule");

        // An exported method promotes too, and the run-time method set is read off emitted
        // extension methods, so it needs a forwarder: minted into the SIBLING class, never into
        // the package class, and by value because the embed is a pointer. (CrossPackagePromotedForwarderTests
        // holds the rules.)
        int sibling = generated.IndexOf("public static class net_testᴛserverProbeᴛxpkg", System.StringComparison.Ordinal);
        Assert.IsTrue(sibling >= 0, "the cross-package forwarders live in the sibling class");
        StringAssert.Contains(generated[sibling..], "static void ServePublic(this global::go.net_test_package.serverProbe target) => target.Server.Value.ServePublic();",
            "an exported cross-package method gains a forwarder in the sibling class");
        Assert.IsFalse(generated[..sibling].Contains("ServePublic"),
            "the package class must not gain a cross-package forwarder");
        Assert.IsFalse(generated.Contains("serveInternal"),
            "an unexported cross-package method is not observable across Go packages and gains no forwarder, friend grant or not");
    }
}
