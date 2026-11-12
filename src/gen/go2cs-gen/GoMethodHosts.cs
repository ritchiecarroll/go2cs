// GoMethodHosts.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace go2cs;

/// <summary>
/// Finds, in metadata, the classes that hold a Go type's methods of its own package.
/// </summary>
/// <remarks>
/// <para>
/// A method a Go type declares is an extension method on the type's method holder, a static class beside the package
/// class that the converter declares once and marks <c>[GoMethodHost(typeof(T))]</c>. The methods a <c>_test.go</c>
/// file declares on a type of the package it tests are on a second holder where both files compile into one
/// assembly, so a type has every holder its namespace marks for it. A type without a holder (a hand-owned type whose
/// methods are written in its package class) keeps them in the package class. A method a struct
/// promotes from an embedded field of its own package is an extension method on the struct's promoted-method class,
/// which <c>StructTypeTemplate</c> generates beside the package class and marks <c>[GoPromotedHost(typeof(T))]</c>. A
/// reader that asks which methods a type has asks all three, through
/// <see cref="PackageMethods(INamedTypeSymbol, ITypeSymbol)"/>.
/// </para>
/// <para>
/// Each class is found by its attribute, never by deriving its name from the type's: the name is unique only
/// while no package class stem holds the lifted-name marker, which GO2CS0001 reports and does not refuse.
/// </para>
/// </remarks>
internal static class GoMethodHosts
{
    private const string PromotedHostAttributeName = "GoPromotedHostAttribute";

    /// <summary>
    /// The name of the attribute that marks a type's method holder.
    /// </summary>
    public const string MethodHostAttributeName = "GoMethodHostAttribute";

    // The classes of one namespace that hold methods, each by the type its attribute names.
    private sealed class Hosts
    {
        public readonly Dictionary<ISymbol, INamedTypeSymbol> Promoted = new(SymbolEqualityComparer.Default);
        public readonly Dictionary<ISymbol, List<INamedTypeSymbol>> Declared = new(SymbolEqualityComparer.Default);
    }

    private static readonly ConditionalWeakTable<INamespaceSymbol, Hosts> s_hosts = new();

    /// <summary>
    /// Gets the methods <paramref name="type"/> declares: those of <paramref name="packageClass"/> followed by
    /// those of each of the type's method holders.
    /// </summary>
    /// <remarks>
    /// The package class comes first and whole: it holds the package's functions and, for a type without a holder,
    /// its methods, and each caller already selects by receiver.
    /// </remarks>
    public static IEnumerable<IMethodSymbol> DeclaredMethods(INamedTypeSymbol packageClass, ITypeSymbol type)
    {
        IEnumerable<IMethodSymbol> methods = packageClass.GetMembers().OfType<IMethodSymbol>();

        return methods.Concat(MethodHolders(type).SelectMany(holder => holder.GetMembers().OfType<IMethodSymbol>()));
    }

    /// <summary>
    /// Gets the methods named <paramref name="name"/> that <paramref name="type"/> declares: those of
    /// <paramref name="packageClass"/> followed by those of each of the type's method holders.
    /// </summary>
    public static IEnumerable<IMethodSymbol> DeclaredMethods(INamedTypeSymbol packageClass, ITypeSymbol type, string name)
    {
        IEnumerable<IMethodSymbol> methods = packageClass.GetMembers(name).OfType<IMethodSymbol>();

        return methods.Concat(MethodHolders(type).SelectMany(holder => holder.GetMembers(name).OfType<IMethodSymbol>()));
    }

    /// <summary>
    /// Gets the methods <paramref name="type"/> declares (<see cref="DeclaredMethods(INamedTypeSymbol, ITypeSymbol)"/>)
    /// followed by the promoted methods its own class holds, when it has one.
    /// </summary>
    public static IEnumerable<IMethodSymbol> PackageMethods(INamedTypeSymbol packageClass, ITypeSymbol type)
    {
        IEnumerable<IMethodSymbol> methods = DeclaredMethods(packageClass, type);

        return PromotedHost(type) is INamedTypeSymbol host ? methods.Concat(host.GetMembers().OfType<IMethodSymbol>()) : methods;
    }

    /// <summary>
    /// Gets the methods named <paramref name="name"/> that <paramref name="type"/> declares followed by those its
    /// own promoted-method class holds, when it has one.
    /// </summary>
    public static IEnumerable<IMethodSymbol> PackageMethods(INamedTypeSymbol packageClass, ITypeSymbol type, string name)
    {
        IEnumerable<IMethodSymbol> methods = DeclaredMethods(packageClass, type, name);

        return PromotedHost(type) is INamedTypeSymbol host ? methods.Concat(host.GetMembers(name).OfType<IMethodSymbol>()) : methods;
    }

    /// <summary>
    /// Gets the classes that hold the methods <paramref name="type"/> declares, in the ordinal order of their
    /// names; none when its package class holds them.
    /// </summary>
    public static IReadOnlyList<INamedTypeSymbol> MethodHolders(ITypeSymbol type) =>
        HostsOf(type) is { } hosts && hosts.Declared.TryGetValue(type.OriginalDefinition, out List<INamedTypeSymbol>? holders) ? holders : [];

    /// <summary>
    /// Gets the class that holds <paramref name="type"/>'s promoted methods of its own package, or null when
    /// it has none.
    /// </summary>
    public static INamedTypeSymbol? PromotedHost(ITypeSymbol type) =>
        HostsOf(type) is { } hosts && hosts.Promoted.TryGetValue(type.OriginalDefinition, out INamedTypeSymbol? host) ? host : null;

    /// <summary>
    /// Gets whether <paramref name="class"/> is a method holder: a class marked <c>[GoMethodHost]</c>.
    /// </summary>
    public static bool IsMethodHost(INamedTypeSymbol? @class) =>
        @class is { IsStatic: true } && @class.GetAttributes().Any(attribute => IsHostAttribute(attribute, MethodHostAttributeName, out _));

    /// <summary>
    /// Gets the package class of the type whose methods <paramref name="class"/> holds, or <paramref name="class"/>
    /// itself when it is no method holder.
    /// </summary>
    public static INamedTypeSymbol PackageClassOf(INamedTypeSymbol @class)
    {
        if (!@class.IsStatic)
            return @class;

        foreach (AttributeData attribute in @class.GetAttributes())
        {
            if (!IsHostAttribute(attribute, MethodHostAttributeName, out INamedTypeSymbol? named))
                continue;

            INamedTypeSymbol? package = named!.ContainingType;

            while (package?.ContainingType is not null)
                package = package.ContainingType;

            return package ?? @class;
        }

        return @class;
    }

    /// <summary>
    /// Gets the fully qualified name of the class a static call of <paramref name="type"/>'s method
    /// <paramref name="name"/> names: its promoted-method class when that class holds the name, else the method
    /// holder that holds it, else its package class. A type that declares a method promotes none of that name and
    /// declares it once, so one class holds every form of it.
    /// </summary>
    public static string StaticClass(ITypeSymbol type, string name)
    {
        string member = name.TrimStart('@');
        INamedTypeSymbol? host = PromotedHost(type);

        if (host is null || host.GetMembers(member).IsEmpty)
            host = MethodHolders(type).FirstOrDefault(holder => !holder.GetMembers(member).IsEmpty) ?? type.ContainingType;

        return host?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "";
    }

    private static Hosts? HostsOf(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol || type.ContainingType?.ContainingNamespace is not INamespaceSymbol @namespace)
            return null;

        return s_hosts.GetValue(@namespace, ReadHosts);
    }

    // Every method-holding class of one namespace, by the type its attribute names. A type's ContainingNamespace is
    // its own module's, so this reads one assembly's classes and never another's.
    private static Hosts ReadHosts(INamespaceSymbol @namespace)
    {
        Hosts hosts = new();

        foreach (INamedTypeSymbol candidate in @namespace.GetTypeMembers())
        {
            if (!candidate.IsStatic)
                continue;

            foreach (AttributeData attribute in candidate.GetAttributes())
            {
                if (IsHostAttribute(attribute, PromotedHostAttributeName, out INamedTypeSymbol? named))
                    hosts.Promoted[named!.OriginalDefinition] = candidate;
                else if (IsHostAttribute(attribute, MethodHostAttributeName, out named))
                    HoldersOf(named!.OriginalDefinition).Add(candidate);
            }
        }

        // The namespace lists its classes in no stated order; a reader that takes the first holder of a name takes
        // the same one on every run.
        foreach (List<INamedTypeSymbol> holders in hosts.Declared.Values)
            holders.Sort(static (left, right) => string.CompareOrdinal(left.Name, right.Name));

        return hosts;

        List<INamedTypeSymbol> HoldersOf(ISymbol type)
        {
            if (!hosts.Declared.TryGetValue(type, out List<INamedTypeSymbol>? holders))
                hosts.Declared[type] = holders = [];

            return holders;
        }
    }

    private static bool IsHostAttribute(AttributeData attribute, string name, out INamedTypeSymbol? named)
    {
        named = null;

        if (attribute.AttributeClass is not { ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true } } attributeClass ||
            attributeClass.Name != name || attribute.ConstructorArguments is not [{ Value: INamedTypeSymbol target }])
        {
            return false;
        }

        named = target;
        return true;
    }
}
