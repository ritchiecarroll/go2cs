// PromotedParameterDefaultTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Pins a promoted forwarder's carriage of a parameter's DEFAULT and its caller-info ATTRIBUTES.
/// The harvest records each parameter as a bare <c>(type, name)</c> pair, which drops both unless
/// they are carried deliberately — the same gap the <c>params</c> modifier already had, and closed
/// beside it.
/// </summary>
/// <remarks>
/// Both losses are silent in the generator and loud (or worse, not loud) somewhere else. A dropped
/// DEFAULT turns an N-argument call into an (N+1)-argument one, so every promoted call site stops
/// binding and CS1929 is raised in the CONSUMER — pointing at the caller, not at the embed that
/// caused it; reflect's <c>ΔValue</c>/<c>flag</c> pair hit exactly this, 30 errors naming
/// <c>mustBe</c> at call sites while the defect sat in the forwarder. A dropped
/// <c>[CallerMemberName]</c> has no diagnostic at all: the forwarder still compiles and still has a
/// name parameter, but it can no longer capture its caller, so the value quietly becomes the default
/// — which for reflect's panic names is the difference between Go's text and a wrong one.
/// </remarks>
[TestClass]
public class PromotedParameterDefaultTests
{
    private const string Source = """
        namespace Probe;

        public static class Ext
        {
            public static void Named(this int target, int kind,
                [System.Runtime.CompilerServices.CallerMemberName] string method = "") { }

            public static void Plain(this int target, int kind) { }

            public static void Defaulted(this int target, string label = "x", bool flag = true, int count = 3) { }
        }
        """;

    private static MethodInfo Harvest(string methodName)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(Source);

        CSharpCompilation compilation = CSharpCompilation.Create("probe", [tree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        MethodDeclarationSyntax declaration = tree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(method => method.Identifier.Text == methodName);

        return declaration.GetMethodInfo(compilation);
    }

    [TestMethod]
    public void PromotedParameterCarriesCallerMemberNameAndDefault()
    {
        MethodInfo info = Harvest("Named");

        // Parameters[0] is the `this` receiver; the threaded name is the third slot.
        (string type, string name) method = info.Parameters[2];

        StringAssert.Contains(method.type, "CallerMemberName",
            "the attribute must ride along or the forwarder silently stops capturing its caller");
        Assert.AreEqual("method = \"\"", method.name,
            "the default rides in the name slot -- the declaration emit is `{type} {name}`");
    }

    [TestMethod]
    public void PromotedCallSiteStripsTheDefault()
    {
        // The declaration keeps `= ""`; the CALL must not -- `f(method = "")` is a syntax error.
        MethodInfo info = Harvest("Named");

        StringAssert.Contains(info.TypedParameters, "method = \"\"");
        Assert.IsFalse(info.CallParameters.Contains('='),
            $"call parameters must be bare identifiers, got: {info.CallParameters}");
        StringAssert.EndsWith(info.CallParameters, "method");
    }

    [TestMethod]
    public void UndefaultedParametersAreUnchanged()
    {
        // The control: carriage must not perturb a parameter that has neither attribute nor default,
        // or every promoted forwarder in the corpus would shift.
        MethodInfo info = Harvest("Plain");

        Assert.AreEqual("kind", info.Parameters[1].name);
        Assert.IsFalse(info.Parameters[1].type.Contains('['), info.Parameters[1].type);
        Assert.IsFalse(info.CallParameters.Contains('='), info.CallParameters);
    }

    [TestMethod]
    public void DefaultLiteralsRoundTripByType()
    {
        // A default is re-emitted as SOURCE, so each literal kind has to render back to something
        // that re-parses -- a string needs its quotes, a bool its lowercase keyword.
        MethodInfo info = Harvest("Defaulted");

        Assert.AreEqual("label = \"x\"", info.Parameters[1].name);
        Assert.AreEqual("flag = true", info.Parameters[2].name);
        Assert.AreEqual("count = 3", info.Parameters[3].name);

        // And the whole declaration must still parse as a member.
        string rendered = $"public static void M({info.TypedParameters}) {{ }}";
        Diagnostic[] errors = CSharpSyntaxTree.ParseText($"class C {{ {rendered} }}")
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(0, errors.Length, string.Join("\r\n", errors.Select(error => error.ToString())));
    }
}
