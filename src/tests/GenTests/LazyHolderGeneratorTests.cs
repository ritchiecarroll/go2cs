// LazyHolderGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// LazyHolderGenerator (docs/DESIGN-lazy-static-data.md, COORD's ruling S1, 2026-10-22): the converter writes a lazy package
/// var's declaration inside a <c>[GoLazy]</c> holder named for it, and the generator writes the var's <c>ref</c> accessor
/// in the package class, plus, for <c>[GoLazy(Initial = true)]</c>, the initial-value snapshot. The arms compile the
/// converter's rendering of each shape, read the generated text, and RUN the image: the holders are built on their first
/// read, never when the package class is touched, and an assignment to an exported var before a lazy initializer reads it
/// does not reach that initializer.
/// </summary>
[TestClass]
public class LazyHolderGeneratorTests
{
    // golib's attribute and a pointer stand-in; the package class is the converter's rendering.
    private const string Stubs = """
        using System;

        namespace go
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class GoLazyAttribute : Attribute { public bool Initial { get; set; } }

            [AttributeUsage(AttributeTargets.Class)]
            public sealed class OtherAttribute : Attribute { }

            public sealed class Box(string name) { public string Name = name; }

            // ж<T>'s stand-in: a heap cell whose Value is the var's storage.
            public sealed class Cell<T>(T value) { public T Value = value; }
        }
        """;

    // The converter's rendering of a var whose address is taken: the box inside the holder, the var's ref property beside it.
    // Every package box is readonly (one emitter constructs it, as a field initializer, and nothing reassigns it).
    private const string AddressedPackage = """
        using System.Collections.Generic;

        namespace go;

        public static partial class addr_package {

        public static List<string> built = new();

        private static int[] make() { built.Add("table"); return new[] { 1, 2, 3 }; }

        [GoLazy] partial class lazyᴛtable { internal static readonly Cell<int[]> Ꮡtable = new Cell<int[]>(make()); }
        internal static ref int[] table => ref Ꮡtable.Value;

        // `&table` before the first read builds it; a write through `table` lands in that box; the address is unchanged.
        public static string Run() {
            var touched = string.Join(",", built);
            var before = Ꮡtable;
            var builtByAddress = string.Join(",", built);
            table[0] = 9;
            var after = Ꮡtable;
            return $"touched:[{touched}] built:[{builtByAddress}] same:{ReferenceEquals(before, after)} seen:{before.Value[0]}";
        }

        } // end addr_package
        """;

    private const string Package = """
        using System;
        using System.Collections.Generic;

        namespace go;

        public static partial class probe_package {

        public static List<string> built = new();

        private static Box make(string name) { built.Add(name); return new Box(name); }

        public static void unrelated() { }

        [GoLazy] partial class lazyᴛ_Lu { internal static Box _Lu = make("_Lu"); }

        [GoLazy(Initial = true)] partial class lazyᴛLu { public static Box Lu = _Lu; } // Lu is the set of Unicode letters in category Lu.

        [GoLazy] partial class lazyᴛUpper { public static Box Upper = _Lu; } // Upper is the set of Unicode upper case letters.

        [GoLazy] partial class lazyᴛCategories { public static Dictionary<string, Box> Categories = new() { ["Lu"] = lazyᴛLu.initial }; }

        // Left alone: a partial class with no [GoLazy], and one with another attribute.
        partial class lazyᴛPlain { internal static int Plain = 1; }

        [Other] partial class lazyᴛOtherMarked { internal static int OtherMarked = 2; }

        // The program a converted importer would run: touch the package, assign the exported Lu, then read Categories.
        public static string Run() {
            unrelated();
            var touched = string.Join(",", built);
            Lu = new Box("replaced");
            var categoriesLu = Categories["Lu"];
            return $"touched:[{touched}] built:[{string.Join(",", built)}] categories:{categoriesLu.Name} lu:{Lu.Name} upper:{Upper.Name} same:{ReferenceEquals(categoriesLu, _Lu)}";
        }

        } // end probe_package
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Collections.dll"));
    }

    private sealed record Run(Dictionary<string, string> Generated, ImmutableArray<Diagnostic> Errors, ImmutableArray<Diagnostic> GeneratorDiagnostics, byte[]? Image);

    private static Run Generate(params string[] sources)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "lazy-holders-" + Guid.NewGuid().ToString("N"),
            sources.Select(source => CSharpSyntaxTree.ParseText(source, parseOptions, path: "probe.cs")),
            CoreReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create([new LazyHolderGenerator()], parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation updated, out ImmutableArray<Diagnostic> generatorDiagnostics);

        Dictionary<string, string> generated = driver.GetRunResult().Results
            .SelectMany(result => result.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

        using MemoryStream image = new();
        EmitResult emitted = updated.Emit(image);

        return new Run(generated, [.. emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)], generatorDiagnostics, emitted.Success ? image.ToArray() : null);
    }

    private static string Describe(Run run) =>
        $"errors: {string.Join("; ", run.Errors)}\r\ngenerator: {string.Join("; ", run.GeneratorDiagnostics)}\r\ngenerated: {string.Join("\r\n", run.Generated.Values)}";

    [TestMethod]
    public void TheAccessorTakesTheGoNameAndTheFieldsAccessibility()
    {
        Run run = Generate(Stubs, Package);
        string generated = string.Join("\r\n", run.Generated.Values);

        Assert.AreEqual(0, run.Errors.Length, Describe(run));

        foreach (string accessor in new[]
                 {
                     "internal static ref Box _Lu => ref lazyᴛ_Lu._Lu;",
                     "public static ref Box Lu => ref lazyᴛLu.Lu;",
                     "public static ref Box Upper => ref lazyᴛUpper.Upper;",
                     "public static ref Dictionary<string, Box> Categories => ref lazyᴛCategories.Categories;"
                 })
        {
            StringAssert.Contains(generated, accessor, Describe(run));
        }
    }

    [TestMethod]
    public void TheInitialSnapshotKeepsGosInitTimeValueAndHoldersAreBuiltOnFirstRead()
    {
        Run run = Generate(Stubs, Package);
        string generated = string.Join("\r\n", run.Generated.Values);

        Assert.IsNotNull(run.Image, Describe(run));
        StringAssert.Contains(generated, "partial class lazyᴛLu { internal static readonly Box initial; static lazyᴛLu() { initial = Lu; } }", Describe(run));
        Assert.IsFalse(generated.Contains("lazyᴛ_Lu { internal static readonly"), "only a holder marked Initial keeps a snapshot");

        // Run the program: touching the package builds nothing; the assignment to the exported Lu precedes Categories' first
        // read, and Categories still holds Lu's initial value, the same pointer as _Lu (Go built Categories at init).
        Assembly assembly = Assembly.Load(run.Image!);
        string result = (string)assembly.GetType("go.probe_package")!.GetMethod("Run")!.Invoke(null, null)!;

        Assert.AreEqual("touched:[] built:[_Lu] categories:_Lu lu:replaced upper:_Lu same:True", result);
    }

    [TestMethod]
    public void AnAddressedVarsHolderCarriesItsBoxAndTheAddressIsOneBeforeAndAfterTheFirstRead()
    {
        // The address-of increment (docs/DESIGN-lazy-static-data.md, "Use sites and address-of"): the holder holds the box,
        // the converter keeps `ref T X => ref ᏑX.Value`, and the generator writes the box accessor under the box's name. The
        // box is readonly, which a writable ref cannot return (CS8161), so the accessor returns the box itself.
        Run run = Generate(Stubs, AddressedPackage);
        string generated = string.Join("\r\n", run.Generated.Values);

        Assert.IsNotNull(run.Image, Describe(run));
        StringAssert.Contains(generated, "internal static Cell<int[]> Ꮡtable => lazyᴛtable.Ꮡtable;", Describe(run));
        Assert.AreEqual(0, run.GeneratorDiagnostics.Length, Describe(run));

        Assembly assembly = Assembly.Load(run.Image!);
        string result = (string)assembly.GetType("go.addr_package")!.GetMethod("Run")!.Invoke(null, null)!;

        Assert.AreEqual("touched:[] built:[table] same:True seen:9", result);
    }

    [TestMethod]
    public void APartialClassWithoutGoLazyIsLeftAlone()
    {
        Run run = Generate(Stubs, Package);
        string generated = string.Join("\r\n", run.Generated.Values);

        Assert.AreEqual(0, run.Errors.Length, Describe(run));
        Assert.IsFalse(generated.Contains(" Plain ") || generated.Contains("lazyᴛPlain"), "an unmarked partial class gets nothing: " + generated);
        Assert.IsFalse(generated.Contains("OtherMarked"), "a class marked with another attribute gets nothing: " + generated);
        Assert.AreEqual(0, run.GeneratorDiagnostics.Length, "no diagnostic for either: " + string.Join("; ", run.GeneratorDiagnostics));
    }

    [TestMethod]
    public void AMalformedHolderIsRefusedByNameAndItsSiblingStillGenerates()
    {
        const string malformed = """
            namespace go;

            public static partial class bad_package {

            [GoLazy] partial class lazyᴛtwoFields { internal static int twoFields = 1; internal static int extra = 2; }

            [GoLazy] partial class lazyᴛmisnamed { internal static int other = 1; }

            [GoLazy] partial class lazyᴛinstance { internal int instance = 1; }

            [GoLazy] partial class unprefixed { internal static int unprefixed = 1; }

            [GoLazy] class lazyᴛnotPartial { internal static int notPartial = 1; }

            [GoLazy(Initial = true)] partial class lazyᴛboxedInitial { internal static Cell<int> ᏑboxedInitial = new Cell<int>(1); }

            [GoLazy] partial class lazyᴛfine { internal static int fine = 1; }

            } // end bad_package
            """;

        Run run = Generate(Stubs, malformed);
        Diagnostic[] refusals = [.. run.GeneratorDiagnostics.Where(diagnostic => diagnostic.Id == "GO2CS0009")];
        string messages = string.Join("\r\n", refusals.Select(diagnostic => diagnostic.GetMessage()));

        Assert.AreEqual(6, refusals.Length, messages);
        Assert.IsTrue(refusals.All(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), "GO2CS0009 is an error");

        foreach ((string holder, string reason) in new[]
                 {
                     ("lazyᴛtwoFields", "it holds 2 member(s), not one field"),
                     ("lazyᴛmisnamed", "its field is named 'other'"),
                     ("lazyᴛinstance", "its field is not static"),
                     ("unprefixed", "its name does not start with 'lazyᴛ'"),
                     ("lazyᴛnotPartial", "it is not partial"),
                     ("lazyᴛboxedInitial", "it holds the var's box, which keeps no initial snapshot")
                 })
        {
            StringAssert.Contains(messages, $"The [GoLazy] holder '{holder}'", messages);
            Assert.IsTrue(refusals.Any(diagnostic => diagnostic.GetMessage().Contains($"'{holder}'") && diagnostic.GetMessage().Contains(reason)), $"{holder}: {reason}\r\n{messages}");
        }

        StringAssert.Contains(string.Join("\r\n", run.Generated.Values), "internal static ref int fine => ref lazyᴛfine.fine;", "the well-formed sibling still generates");
    }
}
