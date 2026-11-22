// NamedSliceArrayClaimedMemberTests.cs - Gbtc
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
using go2cs.Templates.InheritedType;

namespace go2cs.Tests;

/// <summary>
/// R7 seat 1: a Go method declared on a named SLICE or ARRAY (or a type defined over a named array) takes away the wrapper
/// member of the same name. C# binds an instance member before the extension method a Go method becomes, so a member whose
/// parameters fit the Go call ran instead, silently (<c>s.Slice(i, n)</c> returned the wrapper's (start, length) window),
/// bound RecvGenerator's forwarder to the wrong result (Append, GetEnumerator: CS0029), and a type NAMED like a member
/// could not declare it at all (CS0542). Behavioral NamedSliceMethodShadow runs the program; these rows read the generated
/// wrapper itself: the guard fails on a public ASCII member its template does not list as claimable, and the claim rows
/// prove every listed member really moves.
/// </summary>
[TestClass]
public class NamedSliceArrayClaimedMemberTests
{
    private const string FilePath = @"C:\go2cs\src\core\atest\atest.cs";

    // Names a Go method can never claim on the wrapper: the converter renames a Go method of these names
    // (identifierNaming.go `reserved`), so it reaches C# as ΔEquals, ΔToString, ...
    private static readonly HashSet<string> s_reserved = ["Equals", "GetHashCode", "GetType", "ToString", "slice", "MemberwiseClone"];

    // Public wrapper members outside the three templates' own lists, each with its reason. Value is the underlying-value
    // property every reader spells through Common.WrapperValueMember; a Go method named Value is call-correct beside it
    // (a property is not invocable, so C# falls back to the extension: measured on internal/pkgbits' CodeVal) and is R7
    // seat 2's (the numeric template shares the property). AssignFrom is InheritedTypeTemplate's own, already ΔAssignFrom
    // when claimed.
    private static readonly Dictionary<string, string> s_outsideTheTemplate = new()
    {
        ["Value"] = "the underlying-value property (WrapperValueMember); R7 seat 2",
        ["AssignFrom"] = "InheritedTypeTemplate.AssignFromMethod: already ΔAssignFrom when claimed",
    };

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

    // Runs TypeGenerator over the declarations and returns the generated type's symbol and the errors reported inside its
    // own generated part(s).
    private static (INamedTypeSymbol Type, string[] Errors) Generated(string declarations, string typeName)
    {
        string source =
            $$"""
            namespace go;

            [GoPackage("atest")]
            public static partial class atest_package
            {
            {{declarations}}
            }
            """;

        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("atest",
            [CSharpSyntaxTree.ParseText(source, options, path: FilePath), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\atest\obj\atest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        INamedTypeSymbol package = output.GetTypeByMetadataName("go.atest_package") ?? throw new InvalidOperationException("no package class");
        INamedTypeSymbol type = package.GetTypeMembers(typeName).Single();

        string[] errors = output.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error &&
                diagnostic.Location.SourceTree?.FilePath.EndsWith($"{typeName}.g.cs", StringComparison.Ordinal) == true)
            .Select(diagnostic => diagnostic.ToString())
            .ToArray();

        return (type, errors);
    }

    // The generated wrapper's non-private members a Go method could be named like (an internal member binds a same-package
    // Go call just as a public one does): ASCII identifiers, no constructors, operators, conversions, accessors or indexers.
    private static SortedSet<string> VisibleAsciiMembers(INamedTypeSymbol type)
    {
        SortedSet<string> names = new(StringComparer.Ordinal);

        foreach (ISymbol member in type.GetMembers())
        {
            if (member.DeclaredAccessibility is Accessibility.Private or Accessibility.NotApplicable || member.IsImplicitlyDeclared)
                continue;

            if (member is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
                continue;

            if (member is IPropertySymbol { IsIndexer: true })
                continue;

            if (member.Name.Length == 0 || !member.Name.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_'))
                continue;

            names.Add(member.Name);
        }

        return names;
    }

    // One declaration per kind: the wrapper's TypeGenerator input and the template list that must cover it.
    private static readonly (string Kind, string Declarations, string TypeName, IReadOnlyCollection<string> Claimable)[] s_kinds =
    [
        ("slice", """    [GoType("[]nint")] partial struct S;""", "S", ClaimableWrapperMembers.Slice),
        ("array", """    [GoType("[4]nint")] partial struct A;""", "A", ClaimableWrapperMembers.Array),
        ("array view", """    [GoType("[4]nint")] partial struct A;""" + "\n" + """    [GoType("A")] partial struct V;""", "V", ClaimableWrapperMembers.ArrayView),
    ];

    [TestMethod]
    public void EveryPublicAsciiMemberOfTheWrapperIsClaimableReservedOrNamed()
    {
        List<string> uncovered = [];

        foreach ((string kind, string declarations, string typeName, IReadOnlyCollection<string> claimable) in s_kinds)
        {
            (INamedTypeSymbol type, string[] errors) = Generated(declarations, typeName);

            Assert.AreEqual(0, errors.Length, $"{kind}: the plain wrapper does not compile:\n{string.Join("\n", errors)}");

            SortedSet<string> members = VisibleAsciiMembers(type);

            // the guard must see the template's own members, or it reads nothing
            Assert.IsTrue(members.Contains("Length") && members.Contains("Source"), $"{kind}: the guard did not see the wrapper's members: {string.Join(", ", members)}");

            uncovered.AddRange(members
                .Where(name => !claimable.Contains(name) && !s_reserved.Contains(name) && !s_outsideTheTemplate.ContainsKey(name))
                .Select(name => $"{kind}: {name}"));
        }

        Assert.AreEqual(0, uncovered.Count,
            "a wrapper declares a public member a Go method can be named like, with no claimed form: add it to its template's " +
            $"ClaimableMembers with its explicit or Δ form, or name it here with a reason: {string.Join(", ", uncovered)}");
    }

    [TestMethod]
    public void AGoMethodOfEveryClaimableNameTakesThatMemberAway()
    {
        foreach ((string kind, string declarations, string typeName, IReadOnlyCollection<string> claimable) in s_kinds)
        {
            // Every claimable name declared as a Go method on the type (an extension method over its own receiver, the
            // form the converter emits and TypeGenerator.GoMethodNames reads).
            string goMethods = string.Join("\n", claimable.Select(name => $"    internal static nint {name}(this {typeName} r, nint a, nint b) => a + b;"));

            (INamedTypeSymbol type, string[] errors) = Generated(declarations + "\n" + goMethods, typeName);

            Assert.AreEqual(0, errors.Length, $"{kind}: the claimed forms do not compile:\n{string.Join("\n", errors)}");

            string[] stillPublic = VisibleAsciiMembers(type).Where(claimable.Contains).ToArray();

            Assert.AreEqual(0, stillPublic.Length, $"{kind}: claimed by a Go method, still a visible wrapper member: {string.Join(", ", stillPublic)}");
        }
    }

    [TestMethod]
    public void AViewOverAnArrayThatClaimsEveryNameCompiles()
    {
        // The array view reads its underlying wrapper's backing, never a member by name: when the UNDERLYING named array's
        // Go methods claim Source, Length, Slice, ToSpan, GetEnumerator, Clone, Assign or Make, `view.Source` would bind
        // the extension method Go's Source became (CS0428; NamedSliceMethodShadow's `type V Vec`).
        string underlyingClaims = string.Join("\n", ClaimableWrapperMembers.Array.Select(name => $"    internal static nint {name}(this A r, nint a, nint b) => a + b;"));

        (INamedTypeSymbol view, string[] errors) = Generated(
            """    [GoType("[4]nint")] partial struct A;""" + "\n" + """    [GoType("A")] partial struct V;""" + "\n" + underlyingClaims, "V");

        Assert.AreEqual(0, errors.Length, $"a view over a claiming array does not compile:\n{string.Join("\n", errors)}");
        Assert.IsTrue(VisibleAsciiMembers(view).IsSupersetOf(["Source", "Length", "Slice", "ToSpan", "GetEnumerator", "Clone"]),
            "the view's own members, which nothing claims on V, must stay");
    }

    [TestMethod]
    public void ATypeNamedLikeAMemberDeclaresItsWrapper()
    {
        // GoMethodNames carries the type's own name, so `type Length [2]int` / `type Capacity []string` / `type Source []byte`
        // claim the member a C# type may not declare (CS0542).
        foreach ((string declaration, string name) in new[]
                 {
                     ("""    [GoType("[]nint")] partial struct Capacity;""", "Capacity"),
                     ("""    [GoType("[]byte")] partial struct Length;""", "Length"),
                     ("""    [GoType("[2]nint")] partial struct Source;""", "Source"),
                 })
        {
            (INamedTypeSymbol type, string[] errors) = Generated(declaration, name);

            Assert.AreEqual(0, errors.Length, $"type {name}: the wrapper does not compile:\n{string.Join("\n", errors)}");
            Assert.IsFalse(VisibleAsciiMembers(type).Contains(name), $"type {name} still declares a member {name}");
        }
    }
}
