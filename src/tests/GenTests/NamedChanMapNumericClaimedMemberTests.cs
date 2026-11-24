// NamedChanMapNumericClaimedMemberTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go2cs.Templates.InheritedType;
using static go2cs.Tests.NamedSliceArrayClaimedMemberTests;

namespace go2cs.Tests;

/// <summary>
/// R7 seat 2: the rule NamedSliceArrayClaimedMemberTests holds for a named slice or array, held for the other wrapper kinds.
/// A Go method declared on a named CHANNEL, MAP, NUMERIC or STRING type takes away the wrapper member of the same name (C#
/// binds an instance member before the extension method a Go method becomes), and a type NAMED like a member of its own
/// wrapper declares that wrapper (CS0542). A named POINTER type cannot declare a method in Go (an invalid receiver), so
/// only its name claims. Behavioral NamedChanMapNumericShadow runs the program; these rows read the generated wrapper: the
/// guard fails on a visible ASCII member its template does not list as claimable, and the claim rows prove every listed
/// member really moves.
/// </summary>
[TestClass]
public class NamedChanMapNumericClaimedMemberTests
{
    // Visible wrapper members outside the templates' own lists, each with its reason.
    private static readonly Dictionary<string, string> s_outsideTheTemplate = new()
    {
        // A property is not invocable, so a Go method Value() binds its extension beside it, and the converter writes a
        // method value as a call inside a lambda: call-correct and method-value-correct (NamedChanMapNumericShadow's
        // Gauge row). A type NAMED Value takes Common.NamedWrapperValue instead (ATypeNamedLikeAMemberDeclaresItsWrapper).
        ["Value"] = "the underlying-value property (WrapperValueMember): a Go method Value is call- and method-value-correct beside it",
        ["AssignFrom"] = "InheritedTypeTemplate.AssignFromMethod: already ΔAssignFrom when claimed",
    };

    // One declaration per kind: the wrapper's TypeGenerator input, a member its guard must see (liveness) and the template
    // list that must cover it. Methods is false for the pointer kind, whose Go type declares none.
    private static readonly (string Kind, string Definition, string ObjectKind, string Live, IReadOnlyCollection<string> Claimable, bool Methods)[] s_kinds =
    [
        ("channel", "chan nint", "struct", "Capacity", ClaimableWrapperMembers.Channel, true),
        ("map", "map[nint, nint]", "struct", "Count", ClaimableWrapperMembers.Map, true),
        ("numeric", "num:nint", "struct", "CompareTo", ClaimableWrapperMembers.Numeric, true),
        ("named string", "@string", "struct", "Length", ClaimableWrapperMembers.String, true),
        ("pointer", "ж<nint>", "class", "IsNull", ClaimableWrapperMembers.Pointer, false),
    ];

    private static string Declaration(string definition, string objectKind, string name) =>
        $"""    [GoType("{definition}")] partial {objectKind} {name};""";

    [TestMethod]
    public void EveryVisibleAsciiMemberOfTheWrapperIsClaimableReservedOrNamed()
    {
        List<string> uncovered = [];

        foreach ((string kind, string definition, string objectKind, string live, IReadOnlyCollection<string> claimable, _) in s_kinds)
        {
            (INamedTypeSymbol type, string[] errors) = Generated(Declaration(definition, objectKind, "W"), "W");

            Assert.AreEqual(0, errors.Length, $"{kind}: the plain wrapper does not compile:\n{string.Join("\n", errors)}");

            SortedSet<string> members = VisibleAsciiMembers(type);

            // the guard must see the template's own members, or it reads nothing
            Assert.IsTrue(members.Contains(live), $"{kind}: the guard did not see the wrapper's members: {string.Join(", ", members)}");

            uncovered.AddRange(members
                .Where(name => !claimable.Contains(name) && !s_reserved.Contains(name) && !s_outsideTheTemplate.ContainsKey(name))
                .Select(name => $"{kind}: {name}"));
        }

        Assert.AreEqual(0, uncovered.Count,
            "a wrapper declares a visible member a Go method or type can be named like, with no claimed form: add it to its " +
            $"template's ClaimableMembers with its explicit or Δ form, or name it here with a reason: {string.Join(", ", uncovered)}");
    }

    [TestMethod]
    public void AGoMethodOfEveryClaimableNameTakesThatMemberAway()
    {
        foreach ((string kind, string definition, string objectKind, _, IReadOnlyCollection<string> claimable, bool methods) in s_kinds)
        {
            if (!methods)
                continue;

            // Every claimable name declared as a Go method on the type (an extension method over its own receiver, the form
            // the converter emits and TypeGenerator.GoMethodNames reads).
            string goMethods = string.Join("\n", claimable.Select(name => $"    internal static nint {name}(this W r, nint a, nint b) => a + b;"));

            (INamedTypeSymbol type, string[] errors) = Generated(Declaration(definition, objectKind, "W") + "\n" + goMethods, "W");

            Assert.AreEqual(0, errors.Length, $"{kind}: the claimed forms do not compile:\n{string.Join("\n", errors)}");

            string[] stillVisible = VisibleAsciiMembers(type).Where(claimable.Contains).ToArray();

            Assert.AreEqual(0, stillVisible.Length, $"{kind}: claimed by a Go method, still a visible wrapper member: {string.Join(", ", stillVisible)}");
        }
    }

    [TestMethod]
    public void ATypeNamedLikeAMemberDeclaresItsWrapper()
    {
        // GoMethodNames carries the type's own name, so a type named like ANY member of its wrapper claims it (CS0542 otherwise):
        // every claimable name of every kind, plus Value, which every wrapper renames through Common.WrapperValueMember.
        List<string> failures = [];

        foreach ((string kind, string definition, string objectKind, _, IReadOnlyCollection<string> claimable, _) in s_kinds)
        {
            foreach (string name in claimable.Append("Value"))
            {
                (INamedTypeSymbol type, string[] errors) = Generated(Declaration(definition, objectKind, name), name);

                if (errors.Length > 0)
                    failures.Add($"{kind} type {name}: {string.Join(" | ", errors)}");
                else if (VisibleAsciiMembers(type).Contains(name))
                    failures.Add($"{kind} type {name}: still declares a member {name}");
            }
        }

        Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
    }
}
