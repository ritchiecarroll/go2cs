// PromotedForwarderScopeTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// The forwarder of a promoted method is as visible as Go makes the method: an exported method name is exported,
/// whatever its return type is called. The one demotion left is the one C# requires, a signature that names a type
/// which is not public (CS0050, CS0051).
/// </summary>
/// <remarks>
/// <para>
/// RED before the rule read the method's name: the forwarder's scope was read off the spelling of its return type,
/// and <c>@string</c>, <c>bool</c> and every tuple read as unexported. An exported struct that embeds an exported
/// struct by value got <c>internal</c> forwarders for the embed's exported methods, and a caller in another assembly
/// could not bind them (CS1929). The reached case is a package whose struct embeds a JSON value type and whose
/// importer calls the promoted <c>AsString()</c>: 10 of that package's 12 forwarders were internal.
/// </para>
/// <para>
/// A hand-written file may declare a Go type with the GoType attribute and no modifier, as these fixtures do. The
/// type then takes its modifier from go2cs-gen's own part and reads private while a generator runs. (The converter
/// states each type's accessibility in package_info.cs, and such a type reads as declared.) The rule reads the
/// hand-written form as the generator will declare it (Common.EffectiveScopeIsPublic), so a signature that names
/// one inside a tuple or a type argument is public too.
/// </para>
/// </remarks>
[TestClass]
public class PromotedForwarderScopeTests
{
    // golib's lowercase-named public types, which PromotedHostClassTests' golib does not declare.
    private const string Builtins =
        """
        namespace go
        {
            public struct @string { }

            public struct slice<T> { }
        }
        """;

    // Value is an exported struct embedded by value in the exported Holder. Each method of Value is one case.
    // Gen<T> is a generic struct whose one method returns its type parameter; Keep embeds it over an exported type
    // and Wrap over an unexported one.
    private const string Package =
        """
        using go;

        namespace go
        {
            [GoPackage("pe")]
            public static partial class pe_package
            {
                [GoType] partial struct Value { public int n; }

                [GoType] partial struct Node { public int n; }

                [GoType] partial struct hidden { public int n; }

                public static @string AsString(this Value v) => default;

                public static bool IsOk(this Value v) => true;

                public static (@string, bool) Both(this Value v) => default;

                public static (ж<Node>, @string) Find(this Value v) => default;

                public static slice<Node> List(this Value v) => default;

                public static void Put(this Value v, (slice<Node>, @string) nodes) { }

                internal static (ж<hidden>, @string) Peek(this Value v) => default;

                internal static slice<hidden> Rest(this Value v) => default;

                internal static void Hide(this Value v, (slice<hidden>, @string) h) { }

                internal static bool isOk(this Value v) => true;

                internal static (@string, bool) both(this Value v) => default;

                [GoType] partial struct Holder
                {
                    public partial ref Value Value { get; }
                }

                [GoType] partial struct Gen<T> { public T v; }

                public static (T, @string) Get<T>(this Gen<T> g) => default;

                [GoType] partial struct Keep
                {
                    public partial ref Gen<Node> Gen { get; }
                }

                [GoType] partial struct Wrap
                {
                    internal partial ref Gen<hidden> Gen { get; }
                }
            }
        }
        """;

    // The modifiers of every form of each forwarder in a struct's promoted-method class, by method name.
    private static Dictionary<string, string[]> Scopes(string structName = "Holder")
    {
        Dictionary<string, string> sources = PromotedHostClassTests.Generate(Package, Builtins);
        string key = sources.Keys.Single(hint => hint.Contains($"_package.{structName}.g.cs"));

        return CSharpSyntaxTree.ParseText(sources[key]).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(method => method.Parent is ClassDeclarationSyntax { } host && host.Identifier.Text == $"peᴛ{structName}ᴛpkg")
            .GroupBy(method => method.Identifier.Text)
            .ToDictionary(group => group.Key, group => group.Select(Scope).ToArray());
    }

    private static string Scope(MethodDeclarationSyntax method) =>
        string.Join(" ", method.Modifiers.Where(modifier => modifier.IsKind(SyntaxKind.PublicKeyword) || modifier.IsKind(SyntaxKind.InternalKeyword)).Select(modifier => modifier.Text));

    private static void AssertScope(Dictionary<string, string[]> scopes, string name, string expected)
    {
        Assert.IsTrue(scopes.ContainsKey(name), $"{name} is promoted; got: {string.Join(", ", scopes.Keys)}");
        Assert.AreEqual(2, scopes[name].Length, $"{name}: the value form and the pointer form");

        foreach (string scope in scopes[name])
            Assert.AreEqual(expected, scope, $"{name}: every form of the forwarder");
    }

    // The reached case and its two neighbours: a golib type with a lowercase name, a C# keyword type, a tuple of both.
    [TestMethod]
    public void AnExportedMethodsForwarderIsPublicWhateverItsReturnTypeIsCalled()
    {
        Dictionary<string, string[]> scopes = Scopes();

        AssertScope(scopes, "AsString", "public");
        AssertScope(scopes, "IsOk", "public");
        AssertScope(scopes, "Both", "public");
    }

    // An exported type of the package itself in the hand-written form, with no modifier when the generator reads
    // it. Find and Put name it in a tuple that ends in a lowercase-named type, and were internal. List is the control
    // that was public before: its return type's spelling ends in the exported name.
    [TestMethod]
    public void AnExportedMethodsForwarderIsPublicOverAnExportedTypeOfItsOwnPackage()
    {
        Dictionary<string, string[]> scopes = Scopes();

        AssertScope(scopes, "Find", "public");
        AssertScope(scopes, "List", "public");
        AssertScope(scopes, "Put", "public");

        // The same over a generic embed's type argument: Keep's Get returns (Node, @string).
        AssertScope(Scopes("Keep"), "Get", "public");
    }

    // The control. C# allows no public method over a type that is not public, so these stay internal; and an
    // unexported method's forwarder gains nothing from the rule, whatever it returns.
    [TestMethod]
    public void AForwarderStaysInternalOverAnUnexportedTypeAndForAnUnexportedMethod()
    {
        Dictionary<string, string[]> scopes = Scopes();

        AssertScope(scopes, "Peek", "internal");
        AssertScope(scopes, "Rest", "internal");
        AssertScope(scopes, "Hide", "internal");
        AssertScope(scopes, "isOk", "internal");
        AssertScope(scopes, "both", "internal");

        // A generic embed's method is declared over its type parameter, and the forwarder is written over the
        // embed's type argument: Wrap's Get returns (hidden, @string).
        AssertScope(Scopes("Wrap"), "Get", "internal");
    }
}
