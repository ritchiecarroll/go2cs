// WrapperMemberYieldTests.cs - Gbtc
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
/// A Go method is emitted as an extension method, and C# binds an applicable INSTANCE member before it
/// ever looks at extensions, so a public member the generated wrapper declares for its own use runs in
/// place of a Go method of the same name and shape, silently: a named map's <c>Add(k, v)</c> ran
/// Dictionary.Add (and threw on a repeated key), <c>Remove</c>, <c>Clear</c> and <c>ContainsKey</c>
/// answered the map's own question, and a channel's <c>Send(v)</c> skipped the method body. The wrapper
/// therefore YIELDS a member name to any Go method declared on the type, by name and whatever the
/// receiver: a member that implements an interface moves to its explicit implementation (golib still
/// reaches it through the interface), and <c>Set</c>, which no interface declares, is dropped; the
/// converter's own nested-map write always has <c>Set‿</c>, a name no Go identifier can spell.
/// </summary>
[TestClass]
public class WrapperMemberYieldTests
{
    // The converter's spelling of the wrapper's own Set (Symbols.MapWrapperSet), written out so the
    // test pins the emitted name rather than whatever the table says.
    private const string SetDoor = "Set‿";

    private const string Source =
        """
        namespace go
        {
            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            public class GoRecvAttribute : System.Attribute { }

            public readonly struct map<K, V> { private readonly object store; }
            public struct channel<T> { private object queue; }

            public static partial class demo_package
            {
                [GoType("map[nint, nint]")] partial struct Claims;

                public static void Add(this Claims c, nint k, nint v) { }
                public static void Set(this Claims c, nint k, nint v) { }
                public static bool Remove(this Claims c, nint k) => false;
                public static void Clear(this Claims c) { }
                public static bool ContainsKey(this Claims c, nint k) => false;
                public static bool TryGetValue(this Claims c, nint k, nint v) => false;

                [GoType("map[nint, nint]")] partial struct ByPointer;

                [GoRecv] public static void Set(this ref ByPointer p, nint k, nint v) { }

                [GoType("map[T, nint]")] partial struct Uniq<T>;

                public static bool Remove<T>(this Uniq<T> u, T item) => false;

                [GoType("map[nint, nint]")] partial struct Plain;

                [GoType("chan nint")] partial struct Pipe;

                public static void Send(this Pipe p, nint v) { }
                public static bool Sent(this Pipe p, nint v) => false;

                [GoType("chan nint")] partial struct PlainPipe;
            }
        }
        """;

    private static Dictionary<string, string> RunTypeGenerator()
    {
        string coreDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("wrapper-member-yield-test",
            [CSharpSyntaxTree.ParseText(Source, new CSharpParseOptions(LanguageVersion.Latest))],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Runtime.dll"))
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator());
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());
    }

    // The type's generated methods: public instance names, and explicit interface implementations as
    // name/arity (the ICollection<KeyValuePair> Add and Remove are explicit already, with one parameter).
    private static (HashSet<string> publicNames, HashSet<string> explicitNames) Members(Dictionary<string, string> sources, string type)
    {
        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains($".{type}.") || hint.Contains($".{type}_"));
        Assert.IsNotNull(key, $"the TypeGenerator must generate {type}; got: {string.Join(", ", sources.Keys)}");

        List<MethodDeclarationSyntax> methods = CSharpSyntaxTree.ParseText(sources[key!], new CSharpParseOptions(LanguageVersion.Latest))
            .GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

        return (
            methods.Where(method => method.ExplicitInterfaceSpecifier is null && method.Modifiers.Any(SyntaxKind.PublicKeyword) && !method.Modifiers.Any(SyntaxKind.StaticKeyword))
                .Select(method => method.Identifier.Text).ToHashSet(),
            methods.Where(method => method.ExplicitInterfaceSpecifier is not null)
                .Select(method => $"{method.Identifier.Text}/{method.ParameterList.Parameters.Count}").ToHashSet());
    }

    // Each member as name/arity.
    private static void AssertYields(Dictionary<string, string> sources, string type, params string[] members)
    {
        (HashSet<string> publicNames, HashSet<string> explicitNames) = Members(sources, type);

        foreach (string member in members)
        {
            string name = member[..member.IndexOf('/')];
            Assert.IsFalse(publicNames.Contains(name), $"{type} declares a Go method {name}, so its wrapper must not declare a public {name} (it would run instead of the Go method)");
            Assert.IsTrue(explicitNames.Contains(member), $"{type}'s wrapper must still implement {member} explicitly, for golib's interface callers");
        }
    }

    private static void AssertKeeps(Dictionary<string, string> sources, string type, params string[] names)
    {
        (HashSet<string> publicNames, _) = Members(sources, type);

        foreach (string name in names)
            Assert.IsTrue(publicNames.Contains(name), $"control: {type} declares no Go method {name}, so its wrapper keeps the public {name}");
    }

    [TestMethod]
    public void AMapWrapperYieldsEveryInterfaceMemberAGoMethodNames() =>
        AssertYields(RunTypeGenerator(), "Claims", "Add/2", "Remove/1", "Clear/0", "ContainsKey/1", "TryGetValue/2");

    [TestMethod]
    public void AGenericMapWrapperYieldsToo() =>
        AssertYields(RunTypeGenerator(), "Uniq", "Remove/1");

    [TestMethod]
    public void AChannelWrapperYieldsSendAndSent() =>
        AssertYields(RunTypeGenerator(), "Pipe", "Send/1", "Sent/1");

    [TestMethod]
    public void AGoSetRemovesTheWrapperSetWhateverItsReceiver()
    {
        Dictionary<string, string> sources = RunTypeGenerator();

        foreach (string type in new[] { "Claims", "ByPointer" })
        {
            (HashSet<string> publicNames, HashSet<string> explicitNames) = Members(sources, type);
            Assert.IsFalse(publicNames.Contains("Set") || explicitNames.Any(member => member.StartsWith("Set/")), $"{type} declares a Go method Set, so its wrapper must not declare Set at all");
            Assert.IsTrue(publicNames.Contains(SetDoor), $"{type}'s wrapper must declare {SetDoor}, the nested-map write the converter emits for it");
        }
    }

    [TestMethod]
    public void EveryMapWrapperDeclaresTheSetDoor()
    {
        Dictionary<string, string> sources = RunTypeGenerator();

        foreach (string type in new[] { "Claims", "ByPointer", "Uniq", "Plain" })
            Assert.IsTrue(Members(sources, type).publicNames.Contains(SetDoor), $"{type}'s wrapper must declare {SetDoor}");
    }

    [TestMethod]
    public void WrappersWithoutGoMethodsKeepTheirPublicMembers()
    {
        Dictionary<string, string> sources = RunTypeGenerator();

        AssertKeeps(sources, "Plain", "Add", "Set", "Remove", "Clear", "ContainsKey", "TryGetValue");
        AssertKeeps(sources, "PlainPipe", "Send", "Sent");
        AssertKeeps(sources, "ByPointer", "Add", "Remove", "Clear", "ContainsKey", "TryGetValue");
    }
}
