// EscapeRegistryGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace go2cs;

/// <summary>
/// Trim stage 3c-2b(iii), the escape registry: golib's containers carry no operations face (a face on a generic container
/// makes Native AOT expand without bound under partial trim; measured, GolibTests pins it), so reflect reaches a
/// container's operations through a registry. Only an interface value reaches reflect, so the registry names exactly the
/// CLOSED containers this compilation converts to an interface (a boxing conversion) and the closed containers reachable
/// from any value it converts (through struct fields, container elements and pointer targets). One module initializer
/// per package class registers them (golib's GoTypeOps.Register); it runs before any of the package's code, so the
/// operations are registered before the value can reach reflect. Converted code is untouched.
/// </summary>
/// <remarks>
/// <para>
/// WHAT STAYS AT TRIM STAGE 3d (golib's dynamic fallback, which Native AOT cannot run for a value type it never compiled;
/// the JIT is unaffected):
/// </para>
/// <list type="bullet">
/// <item>a conversion inside generic code (T to an interface), whose container is open at the site;</item>
/// <item>a container reachable only through a field another assembly does not export (Roslyn imports only public members
/// from metadata);</item>
/// <item>a container over a type the compilation cannot name: one in an assembly it does not reference directly (a
/// tests project's DisableTransitiveProjectReferences), or an inaccessible one (see <see cref="Nameable"/>);</item>
/// <item>a struct that carries no face of its own (its compilation never spells a pointer to it; TypeOpsScope).</item>
/// </list>
/// <para>
/// MEASURED (2026-10-10, the stage-3 table's row G+(iii), win-x64 Native AOT): the container probe's reflect.New / Zero(*C)
/// for an array, slice, map and chan equal Go's output (row G threw at the first); the registry spans 134-137 stdlib
/// packages and 651-662 registrations per OS; the walk costs 41-45 CPU-seconds per full stdlib build (the largest single
/// project ~3 s, against ~300 s for the other go2cs generators).
/// </para>
/// </remarks>
[Generator]
public class EscapeRegistryGenerator : ISourceGenerator
{
    private static readonly HashSet<string> s_containers = ["array", "slice", "map", "channel"];

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        Compilation compilation = context.Compilation;

        // golib's own compilation and anything that does not reference it has no registry to write.
        if (compilation.GetTypeByMetadataName("go.GoTypeOps") is not { } typeOps || typeOps.GetMembers("Register").IsEmpty)
            return;

        if (compilation.AssemblyName == "golib")
            return;

        Dictionary<INamedTypeSymbol, HashSet<ITypeSymbol>> byPackage = new(SymbolEqualityComparer.Default);
        HashSet<ITypeSymbol> visited = new(SymbolEqualityComparer.Default);

        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(tree);

            foreach (SyntaxNode node in tree.GetRoot().DescendantNodes(descendIntoChildren: child => child is not (BaseMethodDeclarationSyntax or AccessorDeclarationSyntax or EqualsValueClauseSyntax)))
            {
                if (node is not (BaseMethodDeclarationSyntax or AccessorDeclarationSyntax or EqualsValueClauseSyntax))
                    continue;

                if (semanticModel.GetOperation(node) is not { } body)
                    continue;

                // At a member's own start the enclosing symbol is its type (inside a body it would be the member).
                ISymbol? enclosing = semanticModel.GetEnclosingSymbol(node.SpanStart);
                INamedTypeSymbol? package = enclosing as INamedTypeSymbol ?? enclosing?.ContainingType;

                while (package?.ContainingType is not null)
                    package = package.ContainingType;

                if (package is null || package.IsGenericType || !GeneratedPartials.CanReopen(package))
                    continue;

                foreach (IConversionOperation conversion in body.Descendants().OfType<IConversionOperation>())
                {
                    ITypeSymbol? target = conversion.Type;
                    ITypeSymbol? source = conversion.Operand.Type;

                    if (target is null || source is null || !(target.SpecialType == SpecialType.System_Object || target.TypeKind == TypeKind.Interface))
                        continue;

                    if (source.SpecialType == SpecialType.System_Object || source.TypeKind is TypeKind.Interface or TypeKind.TypeParameter)
                        continue;

                    if (!byPackage.TryGetValue(package, out HashSet<ITypeSymbol>? registered))
                        byPackage[package] = registered = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

                    visited.Clear();
                    Reach(compilation, package, source, registered, visited, 0);
                }
            }
        }

        HashSet<string> hintNames = new(System.StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<INamedTypeSymbol, HashSet<ITypeSymbol>> pair in byPackage.Where(pair => pair.Value.Count > 0))
        {
            string ns = pair.Key.ContainingNamespace.ToDisplayString();
            context.AddSource(GeneratedPartials.HintName(hintNames, ns, pair.Key, "escape-registry"), Registry(ns, pair.Key, pair.Value));
        }
    }

    // A closed container reachable from a value of type: the type itself, a container's elements, a pointer's target and
    // a converted struct's fields. An interface ends the walk: what it holds is converted, and registered, where it was.
    internal static void Reach(Compilation compilation, INamedTypeSymbol package, ITypeSymbol type, HashSet<ITypeSymbol> registered, HashSet<ITypeSymbol> visited, int depth)
    {
        if (depth > 16 || !visited.Add(type) || type.TypeKind is TypeKind.Interface or TypeKind.TypeParameter or TypeKind.Delegate)
            return;

        if (type is IArrayTypeSymbol array)
        {
            Reach(compilation, package, array.ElementType, registered, visited, depth + 1);
            return;
        }

        if (type is not INamedTypeSymbol named || named.ContainingNamespace?.ToDisplayString() != "go")
        {
            if (type is INamedTypeSymbol { TypeKind: TypeKind.Struct, ContainingNamespace: { } ns } other && ns.ToDisplayString().StartsWith("go.", System.StringComparison.Ordinal))
                Fields(compilation, package, other, registered, visited, depth);

            return;
        }

        if (IsContainer(named))
        {
            if (!ContainsTypeParameter(named) && Nameable(compilation, package, named))
                registered.Add(named);

            foreach (ITypeSymbol argument in named.TypeArguments)
                Reach(compilation, package, argument, registered, visited, depth + 1);

            return;
        }

        // ж<T>, Go's pointer: its target is reachable through Elem.
        if (named.Name == "ж" && named.TypeArguments.Length == 1)
        {
            Reach(compilation, package, named.TypeArguments[0], registered, visited, depth + 1);
            return;
        }

        if (named.TypeKind == TypeKind.Struct && named.ContainingAssembly?.Name != "golib")
            Fields(compilation, package, named, registered, visited, depth);
    }

    private static void Fields(Compilation compilation, INamedTypeSymbol package, INamedTypeSymbol type, HashSet<ITypeSymbol> registered, HashSet<ITypeSymbol> visited, int depth)
    {
        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>().Where(field => !field.IsStatic && !field.IsConst))
            Reach(compilation, package, field.Type, registered, visited, depth + 1);
    }

    // A registry line must compile, so every type it spells must be one this compilation can NAME: not an error type,
    // defined in this compilation or an assembly it references DIRECTLY, and accessible from the package class. A tests
    // project sets DisableTransitiveProjectReferences, so a type reached through a referenced assembly's field signature
    // can live in an assembly it does not reference (TRAIN T3, 2026-10-10: CS0234 naming go.ast_package and go.io_package).
    // Such a container is skipped: it falls to golib's fallback, trim stage 3d's boundary under Native AOT (an exception,
    // never a wrong answer); the JIT is unaffected.
    internal static bool Nameable(Compilation compilation, INamedTypeSymbol package, ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol array => Nameable(compilation, package, array.ElementType),
        IPointerTypeSymbol pointer => Nameable(compilation, package, pointer.PointedAtType),
        INamedTypeSymbol named => named.TypeKind != TypeKind.Error &&
            (named.SpecialType != SpecialType.None || ReferencesDirectly(compilation, named.ContainingAssembly)) &&
            compilation.IsSymbolAccessibleWithin(named, package) &&
            named.TypeArguments.All(argument => Nameable(compilation, package, argument)),
        _ => false
    };

    private static bool ReferencesDirectly(Compilation compilation, IAssemblySymbol? assembly) =>
        assembly is not null &&
        (SymbolEqualityComparer.Default.Equals(assembly, compilation.Assembly) ||
         compilation.SourceModule.ReferencedAssemblySymbols.Contains(assembly, SymbolEqualityComparer.Default));

    internal static bool IsContainer(INamedTypeSymbol type) =>
        type.ContainingAssembly?.Name == "golib" && type.ContainingType is null && s_containers.Contains(type.Name) && type.IsGenericType;

    private static bool ContainsTypeParameter(ITypeSymbol type) => type switch
    {
        ITypeParameterSymbol => true,
        IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType),
        INamedTypeSymbol named => named.TypeArguments.Any(ContainsTypeParameter) || (named.ContainingType is { } outer && ContainsTypeParameter(outer)),
        _ => false
    };

    private static string Registry(string ns, INamedTypeSymbol package, IEnumerable<ITypeSymbol> containers)
    {
        StringBuilder source = new();
        source.Append("// <auto-generated/>\r\n#nullable enable\r\n\r\n");
        source.Append($"namespace {ns};\r\n\r\n");
        source.Append($"partial class {package.Name}\r\n{{\r\n");
        source.Append("    [global::System.Runtime.CompilerServices.ModuleInitializer]\r\n");
        source.Append($"    internal static void {TempVarMarker}RegisterEscapingContainers()\r\n    {{\r\n");

        foreach (string name in containers.Select(container => container.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Distinct().OrderBy(name => name, System.StringComparer.Ordinal))
            source.Append($"        global::go.GoTypeOps.Register(global::go.GoTypeOps<{name}>.Instance);\r\n");

        source.Append("    }\r\n}\r\n");

        return source.ToString();
    }

    private const string TempVarMarker = "ᴛ";
}
