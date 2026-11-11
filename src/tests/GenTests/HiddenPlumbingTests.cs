// HiddenPlumbingTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Generated plumbing is hidden from IntelliSense and the Go surface is not (docs/PublicSurface.md, tier 2): every public
/// type a generator declares, and the generated members no Go declaration names, carry
/// <c>[EditorBrowsable(EditorBrowsableState.Never)]</c>; no Go-declared type or member does. A generated internal type
/// (an interface shell) is outside the rule, since no consumer can name it.
/// </summary>
/// <remarks>
/// Every <c>[Generator]</c> in go2cs-gen runs (<see cref="GoInterfaceMemberGuardTests.AllGenerators"/>), so a generator
/// added later that declares a type is held to the rule without touching this test. The fixture reaches each plumbing
/// kind: an interface adapter (an implement record), the asserts class (an exported interface), a promoted-method host
/// (an embed), a field-token holder (a struct field), a constraint proxy (a self-referential generic interface), a
/// canonical delegate field (an <c>sstring</c> function) and a named pointer's <c>DerefOrNull</c>.
/// </remarks>
[TestClass]
public class HiddenPlumbingTests
{
    private const string Package =
        """
        namespace go;

        [GoPackage("hide")]
        public static partial class hide_package
        {
            partial interface Reader { (nint, error) Read(slice<byte> p); }

            partial interface Adder<T> { T Add(T a); }

            partial struct inner { internal nint n; }

            partial struct Buffer
            {
                /*embed*/ internal inner inner;
                internal slice<byte> data;
            }

            [GoType("ж<Buffer>")] partial class BufferPtr;

            internal static nint Val(this inner i) => i.n;

            public static (nint, error) Read(this ref Buffer b, slice<byte> p) => (0, default!);

            public static Buffer Add(this Buffer b, Buffer a) => b;

            public static nint Count(sstring s) => 0;
        }
        """;

    // The package's package_info.cs: its implement records, and the Go types' real accessibility.
    private const string PackageInfo =
        """
        using go;

        [assembly: GoImplement<global::go.remote.remote_package.Conn, global::go.hide_package.Reader>(Pointer = true)]
        [assembly: GoImplement<global::go.hide_package.Buffer, global::go.hide_package.Adder<global::go.hide_package.Buffer>>(ConstraintProxy = true)]

        namespace go;

        public static partial class hide_package
        {
            public partial interface Reader {}
            public partial interface Adder<T> {}
            internal partial struct inner {}
            public partial struct Buffer {}
            public partial class BufferPtr {}
        }
        """;

    // A struct of another package: the interface adapter is minted for a foreign struct.
    private const string Remote =
        """
        namespace go.remote;

        [GoPackage("remote")]
        public static partial class remote_package
        {
            public partial struct Conn { }

            public static (nint, error) Read(this ж<Conn> Ꮡc, slice<byte> p) => (0, default!);
        }
        """;

    private const string Usings =
        """
        global using static go.builtin;
        global using System;
        global using any = System.Object;
        """;

    private const string Hidden = "System.ComponentModel.EditorBrowsableAttribute";

    private static bool IsHidden(ISymbol symbol) => symbol.GetAttributes().Any(attribute =>
        attribute.AttributeClass?.ToDisplayString() == Hidden &&
        attribute.ConstructorArguments is [{ Value: 1 }]); // EditorBrowsableState.Never

    // A type a consumer's IntelliSense can offer: it and every type containing it are public. A generated INTERNAL type
    // (an interface shell, InterfaceShellEmitter) is outside the rule; nothing outside the assembly can name it.
    private static bool IsConsumerVisible(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal))
                return false;
        }

        return true;
    }

    // A symbol declared only in generated sources, never in the fixture.
    private static bool IsGenerated(ISymbol symbol, HashSet<SyntaxTree> fixture) =>
        symbol.DeclaringSyntaxReferences.Length > 0 &&
        symbol.DeclaringSyntaxReferences.All(reference => !fixture.Contains(reference.SyntaxTree));

    private static IEnumerable<INamedTypeSymbol> TypesOf(INamespaceOrTypeSymbol container)
    {
        foreach (INamedTypeSymbol type in container.GetTypeMembers())
        {
            yield return type;

            foreach (INamedTypeSymbol nested in TypesOf(type))
                yield return nested;
        }

        if (container is INamespaceSymbol ns)
        {
            foreach (INamespaceSymbol child in ns.GetNamespaceMembers())
            {
                foreach (INamedTypeSymbol type in TypesOf(child))
                    yield return type;
            }
        }
    }

    // Runs every generator (plus any extra) over the fixture. Returns the rule's violations, one line each, and a
    // listing of what was hidden, for the not-vacuous check.
    private static (List<string> violations, List<string> listing) Check(params IIncrementalGenerator[] extra)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation input = CSharpCompilation.Create("hide",
            [
                CSharpSyntaxTree.ParseText(Package, options, path: "hide.cs"),
                CSharpSyntaxTree.ParseText(PackageInfo, options, path: "package_info.cs"),
                CSharpSyntaxTree.ParseText(Remote, options, path: "remote.cs"),
                CSharpSyntaxTree.ParseText(Usings, options, path: "hide.GlobalUsings.g.cs")
            ],
            GoInterfaceMemberGuardTests.References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        List<ISourceGenerator> generators = GoInterfaceMemberGuardTests.AllGenerators();
        generators.AddRange(extra.Select(generator => generator.AsSourceGenerator()));

        CSharpGeneratorDriver.Create(generators.ToArray())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(input, out Compilation output, out _);

        HashSet<SyntaxTree> fixture = input.SyntaxTrees.ToHashSet();
        List<string> violations = [];
        List<string> listing = [];

        foreach (INamedTypeSymbol type in TypesOf(output.Assembly.GlobalNamespace).Where(type => type.DeclaringSyntaxReferences.Length > 0))
        {
            string name = type.ToDisplayString();

            if (IsGenerated(type, fixture))
            {
                if (IsHidden(type))
                    listing.Add($"type {name}");
                else if (IsConsumerVisible(type))
                    violations.Add($"{name}: a generated type a consumer can see is not hidden");

                continue;
            }

            if (IsHidden(type))
                violations.Add($"{name}: a Go-declared type is hidden");

            // A Go-declared type's members: a hidden one must be a generated one (the canonical delegate field, a named
            // pointer's DerefOrNull), never one a Go declaration names.
            foreach (ISymbol member in type.GetMembers().Where(member => member is not INamedTypeSymbol && IsHidden(member)))
            {
                if (IsGenerated(member, fixture))
                    listing.Add($"member {member.ToDisplayString()}");
                else
                    violations.Add($"{member.ToDisplayString()}: a Go-declared member is hidden");
            }
        }

        return (violations, listing);
    }

    [TestMethod]
    public void GeneratedPlumbingIsHiddenAndTheGoSurfaceIsNot()
    {
        (List<string> violations, List<string> listing) = Check();

        Assert.AreEqual(0, violations.Count,
            "generated plumbing must carry [EditorBrowsable(EditorBrowsableState.Never)] and the Go surface must not " +
            "(docs/PublicSurface.md, tier 2):\n" + string.Join("\n", violations));

        // Not vacuous: each plumbing kind the fixture is built to reach was generated, and hidden.
        string[] expected =
        [
            "type go.hide_package.ConnжReader",                                  // interface adapter
            "type go.hide_package.BufferжAdder",                                 // constraint proxy
            "type go.hide_package_asserts",                                      // the asserts class
            "type go.hideᴛBufferᴛpkg",                                           // promoted-method host
            "type go.hide_package.Buffer.Field‿Tokens",                          // field-token holder
            "member go.hide_package.Countᶠ",                                     // canonical delegate field
            "member go.hide_package.DerefOrNull(go.hide_package.BufferPtr?)"     // named pointer's DerefOrNull
        ];

        foreach (string kind in expected)
            Assert.IsTrue(listing.Contains(kind), $"the fixture no longer reaches '{kind}'; hidden were:\n" + string.Join("\n", listing));
    }

    // The negative control: a generator that declares a visible type without the attribute, and one that hides a Go
    // type, are each named.
    [TestMethod]
    public void APlantedVisibleTypeAndAHiddenGoTypeAreCaught()
    {
        (List<string> violations, _) = Check(new Planter());

        CollectionAssert.AreEquivalent(new[]
        {
            "go.hide_package.ᴛPlanted: a generated type a consumer can see is not hidden",
            "go.hide_package.Buffer: a Go-declared type is hidden"
        }, violations);
    }

    private sealed class Planter : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            context.RegisterPostInitializationOutput(output => output.AddSource("planted.g.cs",
                """
                namespace go;

                public static partial class hide_package
                {
                    public static class ᴛPlanted { }

                    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                    public partial struct Buffer { }
                }
                """));
    }
}
