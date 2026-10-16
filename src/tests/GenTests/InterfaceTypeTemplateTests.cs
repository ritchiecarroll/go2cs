// InterfaceTypeTemplateTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using go2cs.Templates.InterfaceType;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go2cs.Symbols;

namespace go2cs.Tests;

/// <summary>
/// Pins the identifiers a runtime duck-typing SHELL emits. Every shape here is reachable only
/// through a keyword-named Go interface or member, or through a Go parameter that collides with the
/// shell's own receiver name — cases the behavioral corpus covers for METHODS but cannot cover for
/// the interface NAME, because the converter always suffixes a lifted anonymous interface with
/// <c>_type</c>, so a Δ shell over a verbatim interface name (database/sql's <c>decimal</c>) is only
/// reachable through a named interface.
/// </summary>
/// <remarks>
/// These tests originally pinned the <c>ᴛAs</c> renderer that anonymous interfaces used to carry.
/// That renderer was retired when dyn interfaces moved onto the shells
/// (docs/phase4/DESIGN-named-interface-wrappers.md, stage 3) — every property below is a property of
/// the surviving one, retargeted rather than dropped, because each was a real emitted-identifier bug.
/// </remarks>
[TestClass]
public class InterfaceTypeTemplateTests
{
    private static MethodInfo Method(string name, string returnType, params (string type, string name)[] parameters) =>
        new()
        {
            Name = name,
            ReturnType = returnType,
            Parameters = parameters,
            GenericTypes = "",
            TypeConstraints = new Dictionary<string, string[]>()
        };

    private static string GenerateShells(string interfaceName, params MethodInfo[] methods) =>
        new InterfaceTypeTemplate
        {
            PackageNamespace = "go",
            PackageName = "probe",
            Scope = "internal",
            InterfaceName = interfaceName,
            OperatorConstraints = [],
            Methods = methods,
            EmitShells = true,
            UsingStatements = []
        }
        .Generate();

    // The generated shells reference types this test assembly does not have (ж<>,
    // IInterfaceAdapter, GoShellBinding), so only SYNTAX is asserted — which is exactly what a
    // mid-token '@' breaks.
    private static void AssertParses(string source)
    {
        Diagnostic[] errors = CSharpSyntaxTree.ParseText(source)
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(0, errors.Length, string.Join("\r\n", errors.Select(error => error.ToString())));
    }

    [TestMethod]
    public void KeywordNamedInterfaceComposesABareShadowMarkerShellName()
    {
        // database/sql's `decimal`: the converter declares the interface escaped.
        string generated = GenerateShells("@decimal", Method("Scan", "void"));

        AssertParses(generated);

        // The interface DECLARATION keeps the escape; both composed shell names drop it.
        StringAssert.Contains(generated, "partial interface @decimal");
        StringAssert.Contains(generated, $"class {ShadowVarMarker}decimal<{ShadowVarMarker}TTarget>");
        StringAssert.Contains(generated, $"class {ShadowVarMarker}decimal{TempVarMarker}Obj");
        Assert.IsFalse(generated.Contains($"{ShadowVarMarker}@decimal"), $"composed the invalid '{ShadowVarMarker}@decimal'");
    }

    [TestMethod]
    public void KeywordNamedMethodComposesBareDelegateAndFieldNames()
    {
        // testing.TB's sealing method, declared DIRECTLY on the interface: syntax-sourced names
        // arrive already escaped.
        string generated = GenerateShells("probe_type", Method("@private", "void"), Method("Label", "global::go.@string"));

        AssertParses(generated);

        // The forwarder DECLARATION keeps the escape.
        StringAssert.Contains(generated, "public void @private()");

        // The binder matches reflection's Name, which is the BARE form — and it is a string
        // literal, not nameof(), because Roslyn hands back the unescaped name for an inherited
        // member and the two must agree.
        StringAssert.Contains(generated, $"ResolveReceiverMethods(typeof({ShadowVarMarker}TTarget), \"private\"");

        // Every COMPOSED name drops the escape — `s_@privateByPtr` is not one token.
        foreach (string composed in new[] { "privateByPtr", "privateByVal", "s_privateByPtr", "s_privateByVal" })
            StringAssert.Contains(generated, composed);

        Assert.IsFalse(generated.Contains("s_@private"), "composed the invalid 's_@privateByPtr'");
    }

    [TestMethod]
    public void ForwardingLocalNeverShadowsAMethodParameter()
    {
        // net/http.Pusher's real shape: a parameter literally named `target`, the same name the
        // shell gives its own receiver (CS0136) — and a by-value call would then hand the RECEIVER
        // to the parameter's slot (CS1503). Neither is a syntax error, so this is asserted on the
        // emitted text.
        string generated = GenerateShells("probe_type",
            Method("Push", "global::go.error", ("global::go.@string", "target"), ("nint", "weight")));

        AssertParses(generated);

        string receiver = $"target{CapturedVarMarker}";

        // The shell's receiver name is marker-suffixed everywhere it is DECLARED, so it cannot
        // collide with any converted Go parameter.
        StringAssert.Contains(generated, $"ByPtr(ж<{ShadowVarMarker}TTarget> {receiver}, global::go.@string target, nint weight)");
        StringAssert.Contains(generated, $"ByVal({ShadowVarMarker}TTarget {receiver}, global::go.@string target, nint weight)");
        StringAssert.Contains(generated, $"public {ShadowVarMarker}probe_type({ShadowVarMarker}TTarget {receiver})");

        // Receiver first, then the method's OWN parameters — never the receiver twice.
        StringAssert.Contains(generated, $"s_PushByPtr(m_target{TempVarMarker}_ptr!, target, weight)");
        StringAssert.Contains(generated, $": m_target{TempVarMarker}, target, weight)");
        Assert.IsFalse(generated.Contains("s_PushByVal!(target, target"), "forwarded the receiver in the parameter's slot");
    }

    [TestMethod]
    public void VoidForwarderDispatchesExactlyOneArm()
    {
        // A void forwarder has no `return` to leave the method on, so the two dispatch arms must be
        // if/else — otherwise the by-pointer arm ran and fell through into the by-value arm,
        // dereferencing a null delegate.
        string generated = GenerateShells("probe_type", Method("Reset", "void"));

        AssertParses(generated);
        StringAssert.Contains(generated, "s_ResetByPtr(");
        StringAssert.Contains(generated, "s_ResetByVal!(");

        // One `else` per forwarder, between the two arms.
        int byPtrIndex = generated.IndexOf("s_ResetByPtr(m_target", System.StringComparison.Ordinal);
        int elseIndex = generated.IndexOf("else", byPtrIndex, System.StringComparison.Ordinal);

        Assert.IsTrue(byPtrIndex > 0 && elseIndex > byPtrIndex && elseIndex < generated.IndexOf("s_ResetByVal!(", System.StringComparison.Ordinal),
            "the by-value arm is not guarded by an else");
    }

    [TestMethod]
    public void BothShellsPrintTheDynamicValuesGoText()
    {
        // A shell stands in for a Go dynamic value, so its ToString is that value's: the value or its ж box,
        // each of which answers its Go Error()/String() text through golib's GoMethodText.
        string generated = GenerateShells("probe_type", Method("Reset", "void"));

        AssertParses(generated);
        Assert.AreEqual(2, generated.Split("public override string ToString()").Length - 1, "one ToString per shell (delegate and object)");
        StringAssert.Contains(generated, "_ptr?.ToString() : m_target");
        StringAssert.Contains(generated, "?.ToString() ?? \"<nil>\";");
    }

    [TestMethod]
    public void AnInterfaceWithNoShellsEmitsOnlyTheDeclaration()
    {
        // EmitShells is off for a constraint, generic, empty or ref-parameter-carrying interface.
        // Nothing but the partial declaration may be emitted then — no stamp naming shells that do
        // not exist, and (since stage 3) no ᴛAs machinery either.
        string generated = new InterfaceTypeTemplate
        {
            PackageNamespace = "go",
            PackageName = "probe",
            Scope = "internal",
            InterfaceName = "probe_type",
            OperatorConstraints = [],
            Methods = [Method("Scan", "void")],
            EmitShells = false,
            UsingStatements = []
        }
        .Generate();

        AssertParses(generated);
        StringAssert.Contains(generated, "partial interface probe_type");
        Assert.IsFalse(generated.Contains("GoInterfaceShell"), "stamped a shell attribute with no shells emitted");
        Assert.IsFalse(generated.Contains($"{TempVarMarker}As"), "emitted the retired ᴛAs machinery");
        Assert.IsFalse(generated.Contains($"class {ShadowVarMarker}probe_type"), "emitted a shell class");
    }
}
