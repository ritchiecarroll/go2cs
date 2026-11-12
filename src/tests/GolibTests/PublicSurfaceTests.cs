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

    // The file's sections: [tier1], [go-tier2], [builtin] and [tier1-members], each a set of lines.
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

        foreach (string section in new[] { "tier1", "go-tier2", "builtin", "tier1-members" })
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

    // ---- [tier1-members]: the signature of every public or protected member of a tier-1 type ----------------------
    //
    // One line per member, "<type> :: <kind> <signature>": the member's name, its generic arity, every parameter's type
    // with ref, out, in and params, an optional parameter's default, the return type, and static or instance. Parameter
    // NAMES and generic constraints are not in it. A member that carries [EditorBrowsable(Never)] is tier 2 and is left
    // out. A removed or changed member reads as one line gone and one line new; an added member reads as one line new.

    private static string TypeName(Type type)
    {
        if (type.IsGenericParameter)
            return type.Name;
        if (type.IsByRef)
            return TypeName(type.GetElementType()!) + "&";
        if (type.IsPointer)
            return TypeName(type.GetElementType()!) + "*";
        if (type.IsArray)
            return TypeName(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";

        Type[] arguments = type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;
        return TypeName(type, arguments, arguments.Length);
    }

    // A generic nested type's arguments include its declaring type's: the first ones belong to the outermost type.
    private static string TypeName(Type type, Type[] arguments, int count)
    {
        string simple = type.Name;
        int tick = simple.IndexOf('`');
        int own = 0;

        if (tick >= 0)
        {
            own = int.Parse(simple[(tick + 1)..]);
            simple = simple[..tick];
        }

        int outer = count - own;
        string prefix = type.IsNested ? TypeName(type.DeclaringType!, arguments, outer) + "." :
            string.IsNullOrEmpty(type.Namespace) ? "" : type.Namespace + ".";
        string list = own > 0 ? "<" + string.Join(", ", arguments.Skip(outer).Take(own).Select(TypeName)) + ">" : "";

        return prefix + simple + list;
    }

    private static string Parameter(ParameterInfo parameter)
    {
        string modifier = "";
        Type type = parameter.ParameterType;

        if (type.IsByRef)
        {
            modifier = parameter.IsOut ? "out " : parameter.IsIn ? "in " : "ref ";
            type = type.GetElementType()!;
        }
        else if (parameter.GetCustomAttributesData().Any(attribute => attribute.AttributeType.Name is "ParamArrayAttribute" or "ParamCollectionAttribute"))
        {
            modifier = "params ";
        }

        string optional = !parameter.IsOptional ? "" :
            " = " + (parameter.HasDefaultValue ? (parameter.DefaultValue is null ? "null" : Convert.ToString(parameter.DefaultValue, System.Globalization.CultureInfo.InvariantCulture)) : "default");

        return modifier + TypeName(type) + optional;
    }

    private static string Parameters(ParameterInfo[] parameters) => "(" + string.Join(", ", parameters.Select(Parameter)) + ")";

    private static bool IsVisible(MethodBase? method) => method is { IsPublic: true } or { IsFamily: true } or { IsFamilyOrAssembly: true };

    private static bool IsVisible(FieldInfo field) => field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly;

    internal static SortedSet<string> MemberSignatures(IEnumerable<Type> types)
    {
        SortedSet<string> lines = new(StringComparer.Ordinal);
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (Type type in types)
        {
            string owner = TypeName(type) + " :: ";
            HashSet<MethodInfo> accessors = [];

            foreach (PropertyInfo property in type.GetProperties(all))
            {
                MethodInfo?[] methods = [property.GetMethod, property.SetMethod];
                accessors.UnionWith(methods.OfType<MethodInfo>());

                if (!methods.Any(IsVisible) || IsHidden(property))
                    continue;

                MethodInfo any = methods.First(IsVisible)!;
                string index = property.GetIndexParameters() is { Length: > 0 } parameters ? "[" + string.Join(", ", parameters.Select(Parameter)) + "]" : "";
                string get = IsVisible(property.GetMethod) ? " get;" : "";
                string set = IsVisible(property.SetMethod) ? " set;" : "";
                lines.Add($"{owner}property {(any.IsStatic ? "static " : "")}{TypeName(property.PropertyType)} {property.Name}{index} {{{get}{set} }}");
            }

            foreach (EventInfo @event in type.GetEvents(all))
            {
                MethodInfo?[] methods = [@event.AddMethod, @event.RemoveMethod, @event.RaiseMethod];
                accessors.UnionWith(methods.OfType<MethodInfo>());

                if (methods.Any(IsVisible) && !IsHidden(@event))
                    lines.Add($"{owner}event {TypeName(@event.EventHandlerType!)} {@event.Name}");
            }

            foreach (FieldInfo field in type.GetFields(all))
            {
                if (!IsVisible(field) || IsHidden(field))
                    continue;

                string kind = field.IsLiteral ? "const " : (field.IsStatic ? "static " : "") + (field.IsInitOnly ? "readonly " : "");
                lines.Add($"{owner}field {kind}{TypeName(field.FieldType)} {field.Name}");
            }

            foreach (ConstructorInfo constructor in type.GetConstructors(all))
            {
                if (IsVisible(constructor) && !constructor.IsStatic && !IsHidden(constructor))
                    lines.Add($"{owner}ctor {Parameters(constructor.GetParameters())}");
            }

            foreach (MethodInfo method in type.GetMethods(all))
            {
                if (!IsVisible(method) || accessors.Contains(method) || IsHidden(method))
                    continue;

                string generic = method.IsGenericMethodDefinition ? "<" + string.Join(", ", method.GetGenericArguments().Select(argument => argument.Name)) + ">" : "";
                string returns = method.ReturnType.IsByRef ? "ref " + TypeName(method.ReturnType.GetElementType()!) : TypeName(method.ReturnType);
                lines.Add($"{owner}method {(method.IsStatic ? "static " : "")}{returns} {method.Name}{generic}{Parameters(method.GetParameters())}");
            }
        }

        return lines;
    }

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

        // The tier-1 types' members, signature for signature.
        HashSet<string> baselineMembers = baseline["tier1-members"];
        SortedSet<string> signatures = MemberSignatures(exported.Where(type => tier1.Contains(type.FullName!)).OrderBy(type => type.FullName, StringComparer.Ordinal));

        foreach (string line in baselineMembers.Where(line => !signatures.Contains(line)).OrderBy(line => line, StringComparer.Ordinal))
            violations.Add($"[tier1-members] {line}: removed or changed; a tier-1 member changes only by the deprecation path (docs/PublicSurface.md)");

        foreach (string line in signatures.Where(line => !baselineMembers.Contains(line)))
            violations.Add($"[tier1-members] {line}: a new tier-1 member; add the line once it is meant to be a promise, or hide it");

        return violations;
    }

    [TestMethod]
    public void GolibsPublicSurfaceMatchesItsBaseline()
    {
        Dictionary<string, HashSet<string>> baseline = ReadBaseline();

        foreach ((string section, HashSet<string> lines) in baseline)
            Console.WriteLine($"{BaselineFile} [{section}]: {lines.Count}");

        // The write mode: GO2CS_PUBLIC_SURFACE_MEMBERS=<path> writes golib's [tier1-members] lines there, from the same code.
        if (Environment.GetEnvironmentVariable("GO2CS_PUBLIC_SURFACE_MEMBERS") is { Length: > 0 } membersPath)
        {
            HashSet<string> tier1Names = baseline["tier1"];
            File.WriteAllLines(membersPath, MemberSignatures(typeof(builtin).Assembly.GetExportedTypes()
                .Where(type => tier1Names.Contains(type.FullName!)).OrderBy(type => type.FullName, StringComparer.Ordinal)));
        }

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
        // A type leaves tier 1 with its member lines.
        Assert.IsTrue(baseline["tier1-members"].RemoveWhere(line => line.StartsWith("go.NilType :: ", StringComparison.Ordinal)) > 0);
        Assert.IsTrue(baseline["go-tier2"].Remove("go.GoFrame"));
        Assert.IsTrue(baseline["builtin"].Remove("len"));
        baseline["tier1"].Add("go.ᴛNoSuchType");
        string member = baseline["tier1-members"].Where(line => line.StartsWith("go.slice<T> :: ", StringComparison.Ordinal)).Min(StringComparer.Ordinal)!;
        Assert.IsTrue(baseline["tier1-members"].Remove(member));
        baseline["tier1-members"].Add("go.slice<T> :: method ᴛNoSuchMember()");

        CollectionAssert.AreEquivalent(new[]
        {
            "go.NilType: not in [tier1] and not hidden; mark it [EditorBrowsable(EditorBrowsableState.Never)], or list it in [tier1] if C# programs use it directly",
            "go.NilType: a new non-Go name in namespace go; new golib names go in namespace go.golib",
            "go.GoFrame: a new non-Go name in namespace go; new golib names go in namespace go.golib",
            "go.ᴛNoSuchType: listed in PublicSurface.golib.txt but golib has no such public type",
            "go.builtin.len: a new public member name of builtin; add it to [builtin] once it is meant to be in scope for every `using static go.builtin`",
            "[tier1-members] go.slice<T> :: method ᴛNoSuchMember(): removed or changed; a tier-1 member changes only by the deprecation path (docs/PublicSurface.md)",
            $"[tier1-members] {member}: a new tier-1 member; add the line once it is meant to be a promise, or hide it"
        }, Violations(baseline));
    }
}
