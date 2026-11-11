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
/// A method a Go type declares is an extension method on its package class. A method a struct promotes from an
/// embedded field of its own package is an extension method on the struct's promoted-method class, which
/// <c>StructTypeTemplate</c> generates beside the package class and marks <c>[GoPromotedHost(typeof(T))]</c>. A
/// reader that asks which methods a foreign type has asks both, through <see cref="PackageMethods(INamedTypeSymbol, ITypeSymbol)"/>.
/// </para>
/// <para>
/// The class is found by its attribute, never by deriving its name from the struct's: the name is unique only
/// while no package class stem holds the lifted-name marker, which GO2CS0001 reports and does not refuse.
/// </para>
/// </remarks>
internal static class GoMethodHosts
{
    private const string HostAttributeName = "GoPromotedHostAttribute";

    private static readonly ConditionalWeakTable<INamespaceSymbol, Dictionary<ISymbol, INamedTypeSymbol>> s_hosts = new();

    /// <summary>
    /// Gets the methods of <paramref name="packageClass"/> followed by the promoted methods <paramref name="type"/>'s
    /// own class holds, when it has one.
    /// </summary>
    public static IEnumerable<IMethodSymbol> PackageMethods(INamedTypeSymbol packageClass, ITypeSymbol type)
    {
        IEnumerable<IMethodSymbol> methods = packageClass.GetMembers().OfType<IMethodSymbol>();

        return PromotedHost(type) is INamedTypeSymbol host ? methods.Concat(host.GetMembers().OfType<IMethodSymbol>()) : methods;
    }

    /// <summary>
    /// Gets the methods named <paramref name="name"/> of <paramref name="packageClass"/> followed by those
    /// <paramref name="type"/>'s own promoted-method class holds, when it has one.
    /// </summary>
    public static IEnumerable<IMethodSymbol> PackageMethods(INamedTypeSymbol packageClass, ITypeSymbol type, string name)
    {
        IEnumerable<IMethodSymbol> methods = packageClass.GetMembers(name).OfType<IMethodSymbol>();

        return PromotedHost(type) is INamedTypeSymbol host ? methods.Concat(host.GetMembers(name).OfType<IMethodSymbol>()) : methods;
    }

    /// <summary>
    /// Gets the class that holds <paramref name="type"/>'s promoted methods of its own package, or null when
    /// it has none.
    /// </summary>
    public static INamedTypeSymbol? PromotedHost(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol || type.ContainingType?.ContainingNamespace is not INamespaceSymbol @namespace)
            return null;

        return s_hosts.GetValue(@namespace, ReadHosts).TryGetValue(type.OriginalDefinition, out INamedTypeSymbol? host) ? host : null;
    }

    /// <summary>
    /// Gets the fully qualified name of the class a static call of <paramref name="type"/>'s method
    /// <paramref name="name"/> names: its promoted-method class when that class holds the name, else its package
    /// class. A type that declares a method promotes none of that name, so one class holds every form of it.
    /// </summary>
    public static string StaticClass(ITypeSymbol type, string name)
    {
        INamedTypeSymbol? host = PromotedHost(type);

        if (host is null || host.GetMembers(name.TrimStart('@')).IsEmpty)
            host = type.ContainingType;

        return host?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "";
    }

    // Every promoted-method class of one namespace, by the struct its attribute names. A type's ContainingNamespace is
    // its own module's, so this reads one assembly's classes and never another's.
    private static Dictionary<ISymbol, INamedTypeSymbol> ReadHosts(INamespaceSymbol @namespace)
    {
        Dictionary<ISymbol, INamedTypeSymbol> hosts = new(SymbolEqualityComparer.Default);

        foreach (INamedTypeSymbol candidate in @namespace.GetTypeMembers())
        {
            if (!candidate.IsStatic)
                continue;

            foreach (AttributeData attribute in candidate.GetAttributes())
            {
                if (attribute.AttributeClass is { Name: HostAttributeName, ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true } } &&
                    attribute.ConstructorArguments is [{ Value: INamedTypeSymbol named }])
                {
                    hosts[named.OriginalDefinition] = candidate;
                }
            }
        }

        return hosts;
    }
}
