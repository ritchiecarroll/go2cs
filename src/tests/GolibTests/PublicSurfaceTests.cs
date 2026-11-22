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

    // The file's sections: [tier1], [go-tier2], [builtin], [tier1-members] and [mn-frozen], each a set of lines.
    private static Dictionary<string, HashSet<string>> ReadBaseline() =>
        ParseBaseline(File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, BaselineFile)));

    private static Dictionary<string, HashSet<string>> ParseBaseline(IEnumerable<string> lines)
    {
        Dictionary<string, HashSet<string>> sections = new(StringComparer.Ordinal);
        HashSet<string>? current = null;

        foreach (string raw in lines)
        {
            string line = raw.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                // A repeated header used to REPLACE the earlier section silently: train 3 met 7a's and 7d's [mn-frozen],
                // and each side's test read the other's lines. One header, one section.
                if (sections.ContainsKey(line[1..^1]))
                    throw new InvalidDataException($"{BaselineFile}: the section {line} appears twice; keep one section of that name");

                current = sections[line[1..^1]] = new HashSet<string>(StringComparer.Ordinal);
                continue;
            }

            if (current is null)
                throw new InvalidDataException($"{BaselineFile}: '{line}' comes before any section");

            current.Add(line);
        }

        foreach (string section in new[] { "tier1", "go-tier2", "builtin", "tier1-members", "mn-frozen" })
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

    // includeHidden: also the members that carry [EditorBrowsable(Never)] ([mn-frozen] freezes hidden members); names:
    // only the members of those names (".ctor" for a constructor). The defaults are [tier1-members]' read, unchanged.
    // frozenDetail ([mn-frozen] only, P3's read of 80c099ee33): a member's abstract/virtual/override modifier (GoFuncPair
    // is subclassed in user assemblies), a constructor's accessibility, and a generic method's constraints.
    private static string Modifier(MethodInfo? method) =>
        method is null ? "" :
        method.IsAbstract ? "abstract " :
        method.IsVirtual && !method.IsFinal ? (method.GetBaseDefinition() == method ? "virtual " : "override ") : "";

    private static string Access(MethodBase method) =>
        method.IsPublic ? "public " : method.IsFamilyOrAssembly ? "protected internal " : method.IsFamily ? "protected " : "internal ";

    private static string Constraints(MethodInfo method)
    {
        if (!method.IsGenericMethodDefinition)
            return "";

        List<string> clauses = [];

        foreach (Type parameter in method.GetGenericArguments())
        {
            GenericParameterAttributes attributes = parameter.GenericParameterAttributes;
            List<string> constraints = [];

            if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                constraints.Add("class");

            if ((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                constraints.Add("struct");

            constraints.AddRange(parameter.GetGenericParameterConstraints().Where(constraint => constraint != typeof(ValueType))
                .Select(TypeName).OrderBy(name => name, StringComparer.Ordinal));

            if ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0 && (attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0)
                constraints.Add("new()");

            if (((int)attributes & 0x20) != 0) // AllowByRefLike: `allows ref struct`
                constraints.Add("allows ref struct");

            if (constraints.Count > 0)
                clauses.Add(parameter.Name + " : " + string.Join(", ", constraints));
        }

        return clauses.Count > 0 ? " where " + string.Join("; ", clauses) : "";
    }

    internal static SortedSet<string> MemberSignatures(IEnumerable<Type> types, bool includeHidden = false, ISet<string>? names = null, bool frozenDetail = false)
    {
        SortedSet<string> lines = new(StringComparer.Ordinal);
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        bool Skip(MemberInfo member) => (!includeHidden && IsHidden(member)) || (names is not null && !names.Contains(member.Name));

        foreach (Type type in types)
        {
            string owner = TypeName(type) + " :: ";
            HashSet<MethodInfo> accessors = [];

            foreach (PropertyInfo property in type.GetProperties(all))
            {
                MethodInfo?[] methods = [property.GetMethod, property.SetMethod];
                accessors.UnionWith(methods.OfType<MethodInfo>());

                if (!methods.Any(IsVisible) || Skip(property))
                    continue;

                MethodInfo any = methods.First(IsVisible)!;
                string index = property.GetIndexParameters() is { Length: > 0 } parameters ? "[" + string.Join(", ", parameters.Select(Parameter)) + "]" : "";
                string get = IsVisible(property.GetMethod) ? " get;" : "";
                string set = IsVisible(property.SetMethod) ? " set;" : "";
                lines.Add($"{owner}property {(any.IsStatic ? "static " : "")}{(frozenDetail ? Modifier(any) : "")}{TypeName(property.PropertyType)} {property.Name}{index} {{{get}{set} }}");
            }

            foreach (EventInfo @event in type.GetEvents(all))
            {
                MethodInfo?[] methods = [@event.AddMethod, @event.RemoveMethod, @event.RaiseMethod];
                accessors.UnionWith(methods.OfType<MethodInfo>());

                if (methods.Any(IsVisible) && !Skip(@event))
                    lines.Add($"{owner}event {TypeName(@event.EventHandlerType!)} {@event.Name}");
            }

            foreach (FieldInfo field in type.GetFields(all))
            {
                if (!IsVisible(field) || Skip(field))
                    continue;

                string kind = field.IsLiteral ? "const " : (field.IsStatic ? "static " : "") + (field.IsInitOnly ? "readonly " : "");
                lines.Add($"{owner}field {kind}{TypeName(field.FieldType)} {field.Name}");
            }

            foreach (ConstructorInfo constructor in type.GetConstructors(all))
            {
                if (IsVisible(constructor) && !constructor.IsStatic && !Skip(constructor))
                    lines.Add($"{owner}ctor {(frozenDetail ? Access(constructor) : "")}{Parameters(constructor.GetParameters())}");
            }

            foreach (MethodInfo method in type.GetMethods(all))
            {
                if (!IsVisible(method) || accessors.Contains(method) || Skip(method))
                    continue;

                string generic = method.IsGenericMethodDefinition ? "<" + string.Join(", ", method.GetGenericArguments().Select(argument => argument.Name)) + ">" : "";
                string returns = method.ReturnType.IsByRef ? "ref " + TypeName(method.ReturnType.GetElementType()!) : TypeName(method.ReturnType);
                lines.Add($"{owner}method {(method.IsStatic ? "static " : "")}{(frozenDetail ? Modifier(method) : "")}{returns} {method.Name}{generic}{Parameters(method.GetParameters())}{(frozenDetail ? Constraints(method) : "")}");
            }
        }

        return lines;
    }

    // ---- [mn-frozen]: the M:N surface frozen for L-min (docs/phase4/FREEZE-mn-surface.md, block 7b) -------
    //
    // Every public or protected member of the six task types, of GoFuncPair, and of the two attributes generated code
    // places or names (GoParkForwarder; GoTwinOf, M:N item 7d's stamp, folded in at train 3), hidden or not, and the
    // named twin entry points (each name's whole overload family). They are tier 2 by their EditorBrowsable marks, so
    // [tier1-members] does not see them; a twin-built assembly binds them, so they never disappear and change shape only
    // where their plain member does. Same spelling as [tier1-members].
    private static readonly string[] s_mnFrozenTypes =
    [
        "go.golib.GoTask", "go.golib.GoTask`1", "go.golib.GoTaskAwaiter", "go.golib.GoTaskAwaiter`1",
        "go.golib.GoTaskBuilder", "go.golib.GoTaskBuilder`1", "go.golib.GoFuncPair",
        "go.golib.GoParkForwarderAttribute", "go.golib.GoTwinOfAttribute"
    ];

    private static readonly (string Type, string[] Names)[] s_mnFrozenMembers =
    [
        ("go.builtin", ["RunMainꞏtwin", "goǃtwin", "selectꞏtwin", "ᐸꟷꞏtwin", "Goschedꞏtwin"]),
        ("go.channel`1", ["ᐸꟷꞏtwin"])
    ];

    private static string ShapeSignature(Type type)
    {
        bool readOnly = type.GetCustomAttributesData().Any(attribute => attribute.AttributeType.FullName == "System.Runtime.CompilerServices.IsReadOnlyAttribute");

        string kind = type.IsInterface ? "interface" :
            type.IsValueType ? (readOnly ? "readonly struct" : "struct") :
            type.IsAbstract && type.IsSealed ? "static class" : type.IsAbstract ? "abstract class" : type.IsSealed ? "sealed class" : "class";

        List<string> parts = [];

        if (!type.IsValueType && type.BaseType is { } baseType && baseType != typeof(object))
            parts.Add("base " + TypeName(baseType));

        string[] interfaces = type.GetInterfaces().Where(face => face.IsPublic || face.IsNestedPublic).Select(TypeName).OrderBy(name => name, StringComparer.Ordinal).ToArray();

        if (interfaces.Length > 0)
            parts.Add("implements " + string.Join(", ", interfaces));

        // A type subclassed outside golib (GoFuncPair) freezes its abstract members of ANY accessibility: a new internal
        // abstract member breaks every subclass in another assembly (P3's read of 80c099ee33).
        if (type.IsAbstract && !type.IsSealed && !type.IsInterface)
        {
            const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            string[] abstracts = type.GetMethods(declared).Where(method => method.IsAbstract)
                .Select(method => method.Name + Parameters(method.GetParameters())).OrderBy(name => name, StringComparer.Ordinal).ToArray();

            if (abstracts.Length > 0)
                parts.Add("abstract-members " + string.Join(", ", abstracts));
        }

        if (type.GetCustomAttributesData().FirstOrDefault(attribute => attribute.AttributeType.FullName == "System.Runtime.CompilerServices.AsyncMethodBuilderAttribute") is { } builder &&
            builder.ConstructorArguments is [{ Value: Type builderType }])
            parts.Add("builder " + TypeName(builderType));

        // An attribute that generated code places (GoParkForwarder) freezes where it may go: a narrower target or a
        // change to AllowMultiple breaks every twin-built assembly that carries it.
        if (typeof(Attribute).IsAssignableFrom(type) && type.GetCustomAttribute<AttributeUsageAttribute>(inherit: false) is { } usage)
            parts.Add($"usage {usage.ValidOn}, multiple {usage.AllowMultiple}, inherited {usage.Inherited}");

        return $"{TypeName(type)} :: shape {kind}" + (parts.Count > 0 ? "; " + string.Join("; ", parts) : "");
    }

    internal static SortedSet<string> MnFrozenSignatures()
    {
        Assembly golib = typeof(builtin).Assembly;
        Type Load(string name) => golib.GetType(name, throwOnError: true)!;

        SortedSet<string> lines = MemberSignatures(s_mnFrozenTypes.Select(Load), includeHidden: true, frozenDetail: true);

        // One SHAPE line per frozen type: its kind, its public interfaces, a base other than object, and its async
        // method builder. A twin-built caller binds these too (P3's read of 7b): an awaiter that stops implementing
        // ICriticalNotifyCompletion breaks every await on it.
        foreach (Type type in s_mnFrozenTypes.Select(Load))
            lines.Add(ShapeSignature(type));

        foreach ((string type, string[] names) in s_mnFrozenMembers)
            lines.UnionWith(MemberSignatures([Load(type)], includeHidden: true, names: new HashSet<string>(names, StringComparer.Ordinal), frozenDetail: true));

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

        // The frozen M:N surface, signature for signature.
        HashSet<string> baselineFrozen = baseline["mn-frozen"];
        SortedSet<string> frozen = MnFrozenSignatures();

        foreach (string line in baselineFrozen.Where(line => !frozen.Contains(line)).OrderBy(line => line, StringComparer.Ordinal))
            violations.Add($"[mn-frozen] {line}: removed or changed; a frozen M:N member never disappears and changes shape only where its plain member does (FREEZE-SHEET.md)");

        foreach (string line in frozen.Where(line => !baselineFrozen.Contains(line)))
            violations.Add($"[mn-frozen] {line}: a new member of the frozen M:N surface; add the line once it is meant to be frozen, or make it non-public");

        return violations;
    }

    // M:N item 7d: the [GoTwinOf] stamp holds its shape (G's condition 1). Generated code in every converted assembly
    // names its ctor, so its lines hold across a break floor. Since train 3 they sit in the ONE [mn-frozen] section, on
    // its predicate (ctor, properties and a shape line with the AttributeUsage), and GolibsPublicSurfaceMatchesItsBaseline
    // checks them line for line; this row keeps the stamp named by a test of its own.
    [TestMethod]
    public void TheGoTwinOfStampIsInTheOneFrozenSection()
    {
        const string Stamp = "go.golib.GoTwinOfAttribute :: ";
        string[] stamp = [.. MnFrozenSignatures().Where(line => line.StartsWith(Stamp, StringComparison.Ordinal))];

        Assert.IsTrue(stamp.Any(line => line.StartsWith(Stamp + "ctor ", StringComparison.Ordinal)), "no [GoTwinOf] ctor line");
        Assert.IsTrue(stamp.Any(line => line.StartsWith(Stamp + "shape ", StringComparison.Ordinal) && line.Contains("; usage Method,", StringComparison.Ordinal)),
            "no [GoTwinOf] shape line with its AttributeUsage");
        CollectionAssert.IsSubsetOf(stamp, ReadBaseline()["mn-frozen"].ToArray(), "a [GoTwinOf] line is missing from [mn-frozen]");
    }

    // A repeated section header is refused by name (train 3: two [mn-frozen] headers, the second silently replacing the
    // first).
    [TestMethod]
    public void ARepeatedSectionHeaderIsRefusedByName()
    {
        InvalidDataException refusal = Assert.ThrowsException<InvalidDataException>(() =>
            ParseBaseline(["[tier1]", "go.string", "[mn-frozen]", "a :: ctor public ()", "", "[mn-frozen]", "b :: ctor public ()"]));

        StringAssert.Contains(refusal.Message, "the section [mn-frozen] appears twice");
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

        // The same for [mn-frozen]: GO2CS_PUBLIC_SURFACE_MN_FROZEN=<path> writes its lines there.
        if (Environment.GetEnvironmentVariable("GO2CS_PUBLIC_SURFACE_MN_FROZEN") is { Length: > 0 } frozenPath)
            File.WriteAllLines(frozenPath, MnFrozenSignatures());

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
        string frozen = baseline["mn-frozen"].Where(line => line.StartsWith("go.golib.GoTaskBuilder :: ", StringComparison.Ordinal)).Min(StringComparer.Ordinal)!;
        Assert.IsTrue(baseline["mn-frozen"].Remove(frozen));
        baseline["mn-frozen"].Add("go.golib.GoTask :: method ᴛNoSuchFrozenMember()");

        CollectionAssert.AreEquivalent(new[]
        {
            "[mn-frozen] go.golib.GoTask :: method ᴛNoSuchFrozenMember(): removed or changed; a frozen M:N member never disappears and changes shape only where its plain member does (FREEZE-SHEET.md)",
            $"[mn-frozen] {frozen}: a new member of the frozen M:N surface; add the line once it is meant to be frozen, or make it non-public",
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
