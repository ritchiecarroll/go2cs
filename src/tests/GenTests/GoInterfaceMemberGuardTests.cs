// GoInterfaceMemberGuardTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// The public-surface rule for Go interfaces (docs/PublicSurface.md; OQ5 of docs/PLAN-public-surface-stability.md): no
/// generator ever adds an ABSTRACT member to a Go-named interface. A C# class that implements a converted Go interface
/// would stop compiling the day one did. A generated addition is default-implemented, or lives on a separate interface.
/// </summary>
/// <remarks>
/// Every <c>[Generator]</c> in go2cs-gen runs, found by reflection, so a generator added later is covered without
/// touching this test. The fixture's interfaces span the shapes the converter emits (a plain interface, an embedding
/// one, a generic one, a one-method and an empty one, exported and unexported), with an implement record so the
/// implement path runs too. Each Go interface's members are compared before and after generation; an added abstract
/// member fails by name. The negative control runs the same check with a test-only generator that adds one.
/// </remarks>
[TestClass]
public class GoInterfaceMemberGuardTests
{
    private const string Package =
        """
        namespace go;

        [GoPackage("iguard")]
        public static partial class iguard_package
        {
            partial interface Reader { (nint, error) Read(slice<byte> p); }

            partial interface Writer { (nint, error) Write(slice<byte> p); }

            partial interface ReadWriter : Reader, Writer { }

            partial interface Container<T> { T Get(); void Put(T value); }

            partial interface Stringer { @string String(); }

            partial interface empty { }

            partial interface sizer { nint size(); }

            partial struct Buffer { internal slice<byte> data; }

            public static (nint, error) Read(this ref Buffer b, slice<byte> p) => (0, default!);

            public static (nint, error) Write(this ref Buffer b, slice<byte> p) => (0, default!);

            public static @string String(this Buffer b) => default!;
        }
        """;

    private const string Records =
        """
        [assembly: GoImplement<go.iguard_package.Buffer, go.iguard_package.Reader>(Pointer = true)]
        [assembly: GoImplement<go.iguard_package.Buffer, go.iguard_package.ReadWriter>(Pointer = true)]
        [assembly: GoImplement<go.iguard_package.Buffer, go.iguard_package.Stringer>]
        """;

    private const string Usings =
        """
        global using static go.builtin;
        global using System;
        global using any = System.Object;
        """;

    private static readonly string[] GoInterfaces = ["Reader", "Writer", "ReadWriter", "Container`1", "Stringer", "empty", "sizer"];

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    // Every generator go2cs-gen declares, as the analyzer host would load it.
    private static List<ISourceGenerator> AllGenerators()
    {
        List<ISourceGenerator> generators = [];

        foreach (Type type in typeof(TypeGenerator).Assembly.GetTypes())
        {
            if (type.IsAbstract || type.GetCustomAttribute<GeneratorAttribute>() is null)
                continue;

            object instance = Activator.CreateInstance(type)!;

            generators.Add(instance switch
            {
                IIncrementalGenerator incremental => incremental.AsSourceGenerator(),
                ISourceGenerator source => source,
                _ => throw new InvalidOperationException($"{type.Name} is a [Generator] of neither kind")
            });
        }

        return generators;
    }

    private static string Signature(ISymbol member) => member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

    // Abstract members each Go interface gained from generation: "Interface: member" per violation.
    private static List<string> AddedAbstractMembers(IEnumerable<ISourceGenerator> generators, out int generatorCount, out int generatedTrees)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation input = CSharpCompilation.Create("iguard",
            [
                CSharpSyntaxTree.ParseText(Package, options, path: "iguard.cs"),
                CSharpSyntaxTree.ParseText(Records, options, path: "package_info.cs"),
                CSharpSyntaxTree.ParseText(Usings, options, path: "iguard.GlobalUsings.g.cs")
            ],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        List<ISourceGenerator> list = generators.ToList();
        generatorCount = list.Count;

        CSharpGeneratorDriver.Create(list.ToArray())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(input, out Compilation output, out _);

        generatedTrees = output.SyntaxTrees.Count() - input.SyntaxTrees.Count();

        List<string> violations = [];

        foreach (string name in GoInterfaces)
        {
            INamedTypeSymbol before = input.GetTypeByMetadataName($"go.iguard_package+{name}")
                ?? throw new AssertFailedException($"the fixture's interface {name} did not bind in the input");
            INamedTypeSymbol after = output.GetTypeByMetadataName($"go.iguard_package+{name}")
                ?? throw new AssertFailedException($"the interface {name} is missing from the output");

            HashSet<string> declared = before.GetMembers().Select(Signature).ToHashSet();

            foreach (ISymbol member in after.GetMembers())
            {
                if (declared.Contains(Signature(member)) || !member.IsAbstract)
                    continue;

                violations.Add($"{name}: {Signature(member)}");
            }
        }

        return violations;
    }

    [TestMethod]
    public void NoGeneratorAddsAnAbstractMemberToAGoInterface()
    {
        List<string> violations = AddedAbstractMembers(AllGenerators(), out int count, out int generated);

        // Discovery is by reflection, so the count follows go2cs-gen; a floor only proves discovery found them at all.
        Assert.IsTrue(count >= 15, $"the generator discovery found only {count} [Generator] types in go2cs-gen");
        // Not vacuous: the generators did write for this fixture (the type, implement and registration paths at least).
        Assert.IsTrue(generated >= 3, $"the generators wrote only {generated} sources for the fixture");
        Assert.AreEqual(0, violations.Count,
            "a generator added an abstract member to a Go-named interface, which breaks every C# class that implements it " +
            "(docs/PublicSurface.md): default-implement the addition or put it on a separate interface:\n" + string.Join("\n", violations));
    }

    // The negative control: the same check, with a generator that does add one, names it.
    [TestMethod]
    public void ThePlantedAbstractMemberIsCaught()
    {
        List<ISourceGenerator> generators = AllGenerators();
        generators.Add(new AbstractMemberPlanter().AsSourceGenerator());

        List<string> violations = AddedAbstractMembers(generators, out _, out _);

        CollectionAssert.AreEqual(new[] { "Reader: go.iguard_package.Reader.ᴛPlanted()" }, violations);
    }

    private sealed class AbstractMemberPlanter : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            context.RegisterPostInitializationOutput(output => output.AddSource("planted.g.cs",
                "namespace go; public static partial class iguard_package { partial interface Reader { void ᴛPlanted(); } }"));
    }
}
