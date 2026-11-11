// PublicSurfaceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// golib's public surface is the baseline in <c>src/core/golib/PublicSurface.golib.txt</c> (docs/PublicSurface.md): a
/// tier-1 type is listed and visible, every other public type is hidden from IntelliSense, a new non-Go name goes in
/// namespace <c>go.golib</c>, and a new public member name of <c>go.builtin</c> is a reviewed change.
/// </summary>
/// <remarks>
/// Each violation fails by name, and so does a baseline entry golib no longer has. The negative control runs the same
/// check against a baseline with one entry of each section removed and one stale entry added, and requires exactly
/// the lines those edits cause.
/// </remarks>
[TestClass]
public class PublicSurfaceTests
{
    private const string BaselineFile = "PublicSurface.golib.txt";

    private const string HiddenAttribute = "System.ComponentModel.EditorBrowsableAttribute";

    // The file's sections: [tier1], [go-tier2] and [builtin], each a set of names.
    private static Dictionary<string, HashSet<string>> ReadBaseline()
    {
        string path = Path.Combine(AppContext.BaseDirectory, BaselineFile);
        Dictionary<string, HashSet<string>> sections = new(StringComparer.Ordinal);
        HashSet<string>? current = null;

        foreach (string raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                current = sections[line[1..^1]] = new HashSet<string>(StringComparer.Ordinal);
                continue;
            }

            if (current is null)
                throw new InvalidDataException($"{BaselineFile}: '{line}' comes before any section");

            current.Add(line);
        }

        foreach (string section in new[] { "tier1", "go-tier2", "builtin" })
        {
            if (!sections.ContainsKey(section))
                throw new InvalidDataException($"{BaselineFile} has no [{section}] section");
        }

        return sections;
    }

    private static bool IsHidden(MemberInfo member) => member.GetCustomAttributesData().Any(attribute =>
        attribute.AttributeType.FullName == HiddenAttribute &&
        attribute.ConstructorArguments is [{ Value: 1 }]); // EditorBrowsableState.Never

    private static bool IsInsideHiddenType(Type type)
    {
        for (Type? container = type.DeclaringType; container is not null; container = container.DeclaringType)
        {
            if (IsHidden(container))
                return true;
        }

        return false;
    }

    // The names `using static go.builtin` brings into scope: its public static members, compiler-generated accessors
    // and operators excluded, and its nested types excluded (they are checked as types).
    private static HashSet<string> BuiltinMemberNames() =>
        typeof(builtin)
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is not Type && member is not MethodInfo { IsSpecialName: true })
            .Select(member => member.Name)
            .ToHashSet(StringComparer.Ordinal);

    private static List<string> Violations(Dictionary<string, HashSet<string>> baseline)
    {
        HashSet<string> tier1 = baseline["tier1"];
        HashSet<string> goTier2 = baseline["go-tier2"];
        HashSet<string> builtinNames = baseline["builtin"];

        Type[] exported = typeof(builtin).Assembly.GetExportedTypes();
        HashSet<string> names = exported.Select(type => type.FullName!).ToHashSet(StringComparer.Ordinal);
        List<string> violations = [];

        foreach (Type type in exported.OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            string name = type.FullName!;
            bool isTier1 = tier1.Contains(name);
            bool hidden = IsHidden(type);

            if (isTier1 && hidden)
                violations.Add($"{name}: listed in [tier1] but hidden from IntelliSense");

            if (!isTier1 && !hidden && !IsInsideHiddenType(type))
                violations.Add($"{name}: not in [tier1] and not hidden; mark it [EditorBrowsable(EditorBrowsableState.Never)], or list it in [tier1] if C# programs use it directly");

            if (!type.IsNested && type.Namespace == "go" && !isTier1 && !goTier2.Contains(name))
                violations.Add($"{name}: a new non-Go name in namespace go; new golib names go in namespace go.golib");
        }

        foreach (string entry in tier1.Concat(goTier2).Where(entry => !names.Contains(entry)).OrderBy(entry => entry, StringComparer.Ordinal))
            violations.Add($"{entry}: listed in {BaselineFile} but golib has no such public type");

        HashSet<string> members = BuiltinMemberNames();

        foreach (string member in members.Where(member => !builtinNames.Contains(member)).OrderBy(member => member, StringComparer.Ordinal))
            violations.Add($"go.builtin.{member}: a new public member name of builtin; add it to [builtin] once it is meant to be in scope for every `using static go.builtin`");

        foreach (string member in builtinNames.Where(member => !members.Contains(member)).OrderBy(member => member, StringComparer.Ordinal))
            violations.Add($"go.builtin.{member}: listed in [builtin] but builtin has no such public member");

        return violations;
    }

    [TestMethod]
    public void GolibsPublicSurfaceMatchesItsBaseline()
    {
        Dictionary<string, HashSet<string>> baseline = ReadBaseline();
        List<string> violations = Violations(baseline);

        Assert.AreEqual(0, violations.Count,
            $"golib's public surface differs from {BaselineFile} (docs/PublicSurface.md):\n" + string.Join("\n", violations));

        // Not vacuous: the baseline names the types the page promises, and the reflection read the whole assembly.
        CollectionAssert.IsSubsetOf(new[] { "go.string", "go.slice`1", "go.map`2", "go.channel`1", "go.ж`1", "go.builtin", "go.PanicException" },
            baseline["tier1"].ToArray());
        Assert.IsTrue(typeof(builtin).Assembly.GetExportedTypes().Length > baseline["tier1"].Count + baseline["go-tier2"].Count,
            "the reflection found fewer public golib types than the baseline lists");
    }

    // The negative control: each kind of drift is named. Removing a tier-1 entry makes that type both unlisted-and-visible
    // and a new name in namespace go; removing a [go-tier2] entry makes that type a new name; removing a builtin name
    // makes it new; an entry golib lacks is stale.
    [TestMethod]
    public void EachKindOfDriftIsNamed()
    {
        Dictionary<string, HashSet<string>> baseline = ReadBaseline();

        Assert.IsTrue(baseline["tier1"].Remove("go.NilType"));
        Assert.IsTrue(baseline["go-tier2"].Remove("go.GoFrame"));
        Assert.IsTrue(baseline["builtin"].Remove("len"));
        baseline["tier1"].Add("go.ᴛNoSuchType");

        CollectionAssert.AreEquivalent(new[]
        {
            "go.NilType: not in [tier1] and not hidden; mark it [EditorBrowsable(EditorBrowsableState.Never)], or list it in [tier1] if C# programs use it directly",
            "go.NilType: a new non-Go name in namespace go; new golib names go in namespace go.golib",
            "go.GoFrame: a new non-Go name in namespace go; new golib names go in namespace go.golib",
            "go.ᴛNoSuchType: listed in PublicSurface.golib.txt but golib has no such public type",
            "go.builtin.len: a new public member name of builtin; add it to [builtin] once it is meant to be in scope for every `using static go.builtin`"
        }, Violations(baseline));
    }
}
