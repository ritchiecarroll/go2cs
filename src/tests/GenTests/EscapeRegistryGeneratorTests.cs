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

    // TRAIN T3 (2026-10-10): a *.tests.csproj sets DisableTransitiveProjectReferences, so a type the compilation reaches
    // only through a referenced assembly's field signature can live in an assembly it does not reference -- and a
    // registry line naming it fails to compile (CS0234: go/internal/srcimporter naming go.ast_package, internal/testenv
    // and testing naming go.io_package). Two hops: A references B, B references C, and A sees C's type only through B's
    // field. The registry keeps what A can name and skips the rest (that container falls to golib's fallback: trim
    // stage 3d's boundary under Native AOT, never a wrong answer).
    [TestMethod]
    public void AContainerTheCompilationCannotNameIsNotRegistered()
    {
        MetadataReference c = Emit("cpkg", "namespace go; [GoPackage(\"cpkg\")] public static partial class cpkg_package { public partial struct Deep { public nint x; } }");
        MetadataReference b = Emit("bpkg", "namespace go; [GoPackage(\"bpkg\")] public static partial class bpkg_package { public partial struct Holder { public slice<cpkg_package.Deep> deep; public slice<nint> nums; } }", c);

        // A references B only, as a tests project with DisableTransitiveProjectReferences does.
        (string[] registered, Diagnostic[] errors) = RunOn("apkg", "internal static object Use() => default(bpkg_package.Holder);", b);

        Assert.AreEqual(0, errors.Length, "the registry compiles: " + string.Join("; ", errors.Take(3)));
        Assert.IsFalse(registered.Any(name => name.Contains("cpkg_package")), "a container over C's type, which A cannot name, is not: " + string.Join(", ", registered));
        Assert.IsFalse(registered.Contains("global::go.slice<nint>"), "and B's fields are not walked at all (see AForeignStructsFieldsAreNotWalked)");
    }

    // RELEASE BLOCKER for 1.24.13.6 (2026-10-11): walking a REFERENCED assembly's struct fields made a platform-neutral
    // package's registry depend on the OS flavor of what it references -- go.runtime.debug and
    // go.internal.coverage.decodemeta registered go.slice<go.syscall_package.WSABuf>, a type only syscall's WINDOWS flavor
    // has -- so the neutral assembly differed per OS (and one shipped windows-only metadata to every platform). The walk
    // descends into a struct's fields only when the compilation defines the struct: a package's own sources compile on
    // every OS it ships for, so every type its registry spells exists in every flavor. P stands in for a per-OS package
    // (syscall): its Holder's field is a container over a type only P has. A container A spells through its OWN field
    // (slice<P.Holder>) stays registered; one reachable only inside P's struct does not (golib's fallback answers it:
    // trim stage 3d's boundary under Native AOT, never a wrong answer).
    [TestMethod]
    public void AForeignStructsFieldsAreNotWalked()
    {
        MetadataReference p = Emit("ppkg", "namespace go; [GoPackage(\"ppkg\")] public static partial class ppkg_package { public partial struct OnlyHere { public nint x; } public partial struct Holder { public slice<ppkg_package.OnlyHere> only; } }");

        (string[] registered, Diagnostic[] errors) = RunOn("apkg",
            "partial struct Mine { internal slice<ppkg_package.Holder> holders; } internal static object Use() => default(ppkg_package.Holder); internal static object UseMine() => default(Mine);", p);

        Assert.AreEqual(0, errors.Length, "the registry compiles: " + string.Join("; ", errors.Take(3)));
        CollectionAssert.Contains(registered, "global::go.slice<global::go.ppkg_package.Holder>", "a container A reaches through its own struct's field is registered");
        Assert.IsFalse(registered.Any(name => name.Contains("OnlyHere")), "a container reachable only inside the foreign struct is not: " + string.Join(", ", registered));
    }

    private static MetadataReference Emit(string name, string source, params MetadataReference[] references)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation library = CSharpCompilation.Create(name,
            [CSharpSyntaxTree.ParseText(source, options), CSharpSyntaxTree.ParseText(TemplateUsings, options)],
            [.. References(), .. references], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        using System.IO.MemoryStream image = new();
        Microsoft.CodeAnalysis.Emit.EmitResult emitted = library.Emit(image);
        Assert.IsTrue(emitted.Success, $"{name} compiles: " + string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Take(3)));

        return MetadataReference.CreateFromImage(image.ToArray());
    }

    // Runs the generator over a package class named <name>_package holding `members`, referencing `references` only.
    private static (string[] registered, Diagnostic[] errors) RunOn(string name, string members, params MetadataReference[] references)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create(name,
            [CSharpSyntaxTree.ParseText($"namespace go; [GoPackage(\"{name}\")] public static partial class {name}_package {{ {members} }}", options, path: $@"C:\go2cs\src\core\{name}\{name}.cs"),
             CSharpSyntaxTree.ParseText(TemplateUsings, options)],
            [.. References(), .. references], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new EscapeRegistryGenerator()).WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] registered = output.SyntaxTrees.Select(tree => tree.ToString())
            .SelectMany(source => Regex.Matches(source, @"global::go\.GoTypeOps\.Register\(global::go\.GoTypeOps<(.+)>\.Instance\);").Select(match => match.Groups[1].Value))
            .ToArray();

        return (registered, output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray());
    }
}
