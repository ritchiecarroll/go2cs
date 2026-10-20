// NativeArrayOpenerGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

/// <summary>
/// The native array openers (G's ruling, 2026-10-19: NARROWER): golib reaches two native array kinds knowing the element
/// only as a Type -- the foreign pointer array <c>*[N]*E</c> and the window over a native struct's <c>[N]X</c> field --
/// and builds each through an opener registered per element (golib's NativeArrayOpeners), so neither is built by
/// MakeGenericMethod (no code under Native AOT for a value type) nor compiled into every type. This generator registers,
/// in one module initializer per package class, exactly the elements this compilation SPELLS at a door that reaches
/// them; a miss is a Go panic naming the element, under the JIT exactly as under Native AOT.
/// </summary>
/// <remarks>
/// <para>
/// THE DOORS, each a closed spelling with E's pointer type <c>ж&lt;E&gt;</c> as the element:
/// </para>
/// <list type="bullet">
/// <item><c>array&lt;ж&lt;E&gt;&gt;.AliasPointer</c>, the converter's <c>(*[N]*E)(unsafe.Pointer(p))</c> for a <c>**E</c>
/// p (and the hand-owned C# that spells it);</item>
/// <item><c>@unsafe.Slice</c> closed over <c>ж&lt;E&gt;</c> (its type argument is inferred, so it is read from the bound
/// call), whose native branch reads the n words through the same reader;</item>
/// <item><c>@unsafe.ArrayPointer&lt;ж&lt;E&gt;&gt;</c>, <c>builtin.NativeArrayPointer&lt;ж&lt;E&gt;&gt;</c> and
/// <c>builtin.NativeFieldArrayPointer&lt;ж&lt;E&gt;&gt;</c> spelled directly;</item>
/// <item>an instance field <c>array&lt;X&gt;</c> of a struct this compilation declares registers X for the field window,
/// and E too when X is <c>ж&lt;E&gt;</c> (the window over an <c>[N]*E</c> field opens the pointer array).</item>
/// </list>
/// <para>
/// WHAT STAYS REFUSED BY NAME: an element only reflection builds (<c>reflect.NewAt(ArrayOf(N, PointerTo(E)), p)</c>), a
/// door spelled inside generic code (its element is open at the site), and an element the package class cannot name
/// (<see cref="EscapeRegistryGenerator.Nameable"/>). A <c>(*[N]*E)(unsafe.Pointer(u))</c> from any other source never
/// reaches the reader: it is ж's uintptr floor, which refuses by name on its own.
/// </para>
/// </remarks>
[Generator]
public class NativeArrayOpenerGenerator : ISourceGenerator
{
    private static readonly HashSet<string> s_pointerDoors = ["ArrayPointer", "NativeArrayPointer", "NativeFieldArrayPointer"];

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        Compilation compilation = context.Compilation;

        // golib's own compilation and anything that does not reference it has no openers to register.
        if (compilation.AssemblyName == "golib" || compilation.GetTypeByMetadataName("go.NativeArrayOpeners") is null)
            return;

        Dictionary<INamedTypeSymbol, Openers> byPackage = new(SymbolEqualityComparer.Default);

        Openers For(INamedTypeSymbol package)
        {
            if (!byPackage.TryGetValue(package, out Openers? openers))
                byPackage[package] = openers = new Openers();

            return openers;
        }

        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(tree);

            foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
            {
                switch (node)
                {
                    case GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 } name when IsDoor(name):
                    {
                        if (semanticModel.GetSymbolInfo(name).Symbol is not { } symbol || PointerElement(DoorElement(symbol)) is not { } element)
                            break;

                        if (PackageOf(semanticModel, node) is { } package && Registrable(compilation, package, element))
                            For(package).PointerArrays.Add(element);

                        break;
                    }
                    case InvocationExpressionSyntax invocation when CalleeName(invocation.Expression) == "Slice":
                    {
                        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { Name: "Slice", TypeArguments.Length: > 0 } method ||
                            method.ContainingType is not { Name: "unsafe_package", ContainingNamespace: { } ns } || ns.ToDisplayString() != "go" ||
                            PointerElement(method.TypeArguments[0]) is not { } element)
                            break;

                        if (PackageOf(semanticModel, node) is { } package && Registrable(compilation, package, element))
                            For(package).PointerArrays.Add(element);

                        break;
                    }
                    case StructDeclarationSyntax declaration:
                    {
                        if (semanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol type)
                            break;

                        INamedTypeSymbol package = Outermost(type);

                        if (!Reopenable(package))
                            break;

                        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>().Where(field => !field.IsStatic && !field.IsConst && field.DeclaringSyntaxReferences.Any(reference => reference.SyntaxTree == tree)))
                        {
                            if (ArrayElement(field.Type) is not { } element || !Registrable(compilation, package, element))
                                continue;

                            For(package).Arrays.Add(element);

                            if (PointerElement(element) is { } pointed && Registrable(compilation, package, pointed))
                                For(package).PointerArrays.Add(pointed);
                        }

                        break;
                    }
                }
            }
        }

        HashSet<string> hintNames = new(System.StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<INamedTypeSymbol, Openers> pair in byPackage.Where(pair => pair.Value.Count > 0))
        {
            string ns = pair.Key.ContainingNamespace.ToDisplayString();
            context.AddSource(GeneratedPartials.HintName(hintNames, ns, pair.Key, "native-array-openers"), Registry(ns, pair.Key, pair.Value));
        }
    }

    private sealed class Openers
    {
        public readonly HashSet<ITypeSymbol> PointerArrays = new(SymbolEqualityComparer.Default);
        public readonly HashSet<ITypeSymbol> Arrays = new(SymbolEqualityComparer.Default);

        public int Count => PointerArrays.Count + Arrays.Count;
    }

    // `array<…>` as the receiver of AliasPointer, or one of the pointer doors by name; the binding decides the rest.
    private static bool IsDoor(GenericNameSyntax name) => name.Identifier.ValueText switch
    {
        "array" => name.Parent is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "AliasPointer" } access && access.Expression == name,
        var identifier => s_pointerDoors.Contains(identifier)
    };

    // The array element a bound door spells: array<T>'s and ArrayPointer<T>'s T, or a golib builtin door's T.
    private static ITypeSymbol? DoorElement(ISymbol symbol) => symbol switch
    {
        INamedTypeSymbol { Name: "array" or "ArrayPointer", TypeArguments.Length: 1 } type when IsGo(type) => type.TypeArguments[0],
        IMethodSymbol { Name: "NativeArrayPointer" or "NativeFieldArrayPointer", TypeArguments.Length: 1, ContainingType.Name: "builtin" } method when IsGo(method.ContainingType) => method.TypeArguments[0],
        _ => null
    };

    private static bool IsGo(INamedTypeSymbol type) => type.ContainingNamespace?.ToDisplayString() == "go";

    private static string? CalleeName(ExpressionSyntax callee) => callee switch
    {
        MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText,
        SimpleNameSyntax name => name.Identifier.ValueText,
        _ => null
    };

    // E for golib's pointer ж<E>, else null.
    private static ITypeSymbol? PointerElement(ITypeSymbol? type) =>
        type is INamedTypeSymbol { Name: "ж", TypeArguments.Length: 1, ContainingAssembly.Name: "golib" } pointer ? pointer.TypeArguments[0] : null;

    // X for golib's array<X>, else null.
    private static ITypeSymbol? ArrayElement(ITypeSymbol type) =>
        type is INamedTypeSymbol { Name: "array", TypeArguments.Length: 1, ContainingAssembly.Name: "golib" } array ? array.TypeArguments[0] : null;

    // The package class enclosing a door: the outermost type around it, when a generated partial can reopen it.
    private static INamedTypeSymbol? PackageOf(SemanticModel semanticModel, SyntaxNode node)
    {
        ISymbol? enclosing = semanticModel.GetEnclosingSymbol(node.SpanStart);
        INamedTypeSymbol? type = enclosing as INamedTypeSymbol ?? enclosing?.ContainingType;

        if (type is null)
            return null;

        INamedTypeSymbol package = Outermost(type);

        return Reopenable(package) ? package : null;
    }

    // The registry is written as a partial CLASS of the package (converted code's package class is one).
    private static bool Reopenable(INamedTypeSymbol package) =>
        package is { TypeKind: TypeKind.Class, IsGenericType: false } && GeneratedPartials.CanReopen(package);

    private static INamedTypeSymbol Outermost(INamedTypeSymbol type)
    {
        while (type.ContainingType is not null)
            type = type.ContainingType;

        return type;
    }

    // A registration line must compile and register a closed element: no type parameter, and nameable from the package.
    private static bool Registrable(Compilation compilation, INamedTypeSymbol package, ITypeSymbol element) =>
        !ContainsTypeParameter(element) && EscapeRegistryGenerator.Nameable(compilation, package, element);

    private static bool ContainsTypeParameter(ITypeSymbol type) => type switch
    {
        ITypeParameterSymbol => true,
        IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType),
        INamedTypeSymbol named => named.TypeArguments.Any(ContainsTypeParameter) || (named.ContainingType is { } outer && ContainsTypeParameter(outer)),
        _ => false
    };

    private static string Registry(string ns, INamedTypeSymbol package, Openers openers)
    {
        StringBuilder source = new();
        source.Append("// <auto-generated/>\r\n#nullable enable\r\n\r\n");
        source.Append($"namespace {ns};\r\n\r\n");
        source.Append($"partial class {package.Name}\r\n{{\r\n");
        source.Append("    [global::System.Runtime.CompilerServices.ModuleInitializer]\r\n");
        source.Append($"    internal static void {TempVarMarker}RegisterNativeArrayOpeners()\r\n    {{\r\n");

        foreach (string name in Names(openers.PointerArrays))
            source.Append($"        global::go.NativeArrayOpeners.RegisterPointerArray<{name}>();\r\n");

        foreach (string name in Names(openers.Arrays))
            source.Append($"        global::go.NativeArrayOpeners.RegisterArray<{name}>();\r\n");

        source.Append("    }\r\n}\r\n");

        return source.ToString();
    }

    private static IEnumerable<string> Names(IEnumerable<ITypeSymbol> types) =>
        types.Select(type => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Distinct().OrderBy(name => name, System.StringComparer.Ordinal);

    private const string TempVarMarker = "ᴛ";
}
