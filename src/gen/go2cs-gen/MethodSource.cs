// MethodSource.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static go2cs.Symbols;

namespace go2cs;

/// <summary>
/// Writes golib's <c>IGoMethodSource</c> on a generated part of every GENERIC value type with Go methods: each of the
/// type's Go methods, closed over the type's own type parameters by compiled code.
/// </summary>
/// <remarks>
/// <para>
/// A Go method of <c>Pair[K, V]</c> is emitted as the generic extension method <c>String&lt;K, V&gt;(this Pair&lt;K, V&gt;)</c>,
/// and golib's method-set scan finds only its OPEN definition. Reflect and the interface shells need it closed over the
/// value's type arguments, and <c>MakeGenericMethod</c> cannot be relied on for that under Native AOT: it finds code
/// only for an instantiation the program happened to compile. Here each arm creates a delegate over the method inside
/// the type itself, <c>((ᴛGoMethod0)global::go.main_package.String&lt;K, V&gt;).Method</c>, so every instantiation the
/// program constructs compiles its own closed methods, and golib reads them through the type's zero value
/// (<c>TypeExtensions.CloseGoMethod</c>), as it reads <c>IGoFieldBoxSource</c>.
/// </para>
/// <para>
/// Each arm has its own nested delegate type with the method's exact signature, so a by-ref receiver, a parameter
/// <c>Func</c>/<c>Action</c> cannot carry, or any arity needs no special case. A pointer-receiver method emitted
/// <c>this ref T</c> is answered by its <c>ж&lt;T&gt;</c> twin, which RecvGenerator writes and this generator cannot
/// see but can name: the delegate's <c>ж&lt;T&gt;</c> receiver selects it. A non-generic type is untouched.
/// </para>
/// </remarks>
internal static class MethodSource
{
    private const string Interface = "global::go.IGoMethodSource";

    /// <summary>
    /// The source of a generated part of <paramref name="type"/> implementing <c>IGoMethodSource</c>, or <c>null</c> when
    /// it is not a generic value type, has no Go methods, or cannot be reopened.
    /// </summary>
    public static string? Source(string ns, INamedTypeSymbol? type)
    {
        if (type is not { IsGenericType: true, IsValueType: true } || !GeneratedPartials.CanReopen(type))
            return null;

        INamedTypeSymbol? package = type.ContainingType;

        while (package?.ContainingType is not null)
            package = package.ContainingType;

        if (package is null || package.IsGenericType)
            return null;

        List<(string name, bool pointer, string signature, string method)> arms = [];
        HashSet<(string, bool)> seen = [];

        foreach (IMethodSymbol method in package.GetMembers().OfType<IMethodSymbol>())
        {
            if (Arm(method, type, package) is not { } arm || !seen.Add((arm.name, arm.pointer)))
                continue;

            arms.Add(arm);
        }

        if (arms.Count == 0)
            return null;

        StringBuilder body = new();

        for (int i = 0; i < arms.Count; i++)
            body.Append($"    private delegate {arms[i].signature.Replace("ᴛNAME", $"{TempVarMarker}GoMethod{i}")};\r\n");

        body.Append("\r\n");
        body.Append($"    global::System.Reflection.MethodInfo? {Interface}.GoMethod(string name, bool pointerReceiver) => (name, pointerReceiver) switch\r\n");
        body.Append("    {\r\n");

        for (int i = 0; i < arms.Count; i++)
            body.Append($"        ({SymbolDisplay.FormatLiteral(arms[i].name, true)}, {(arms[i].pointer ? "true" : "false")}) => (({TempVarMarker}GoMethod{i}){arms[i].method}).Method,\r\n");

        body.Append("        _ => null\r\n");
        body.Append("    };\r\n");

        return GeneratedPartials.Source(ns, type, [], $" : {Interface}", body.ToString());
    }

    // One arm: the method's metadata name, whether it is a pointer-receiver method, its delegate declaration (with
    // ᴛNAME standing for the delegate's name) and the method group naming it over the type's own parameters. Null when
    // the method is not a Go method of `type`.
    private static (string name, bool pointer, string signature, string method)? Arm(IMethodSymbol method, INamedTypeSymbol type, INamedTypeSymbol package)
    {
        if (!method.IsExtensionMethod || !method.IsGenericMethod || method.Parameters.Length == 0 || method.ReturnsByRef || method.ReturnsByRefReadonly)
            return null;

        IParameterSymbol receiver = method.Parameters[0];
        bool boxed = IsBox(receiver.Type, out ITypeSymbol element);

        if (element is not INamedTypeSymbol receiverType || !SymbolEqualityComparer.Default.Equals(receiverType.OriginalDefinition, type.OriginalDefinition))
            return null;

        // A Go method declares no type parameters of its own, so each of the emitted method's type parameters is one of
        // the receiver's type arguments; map each to the type's parameter at that position.
        ITypeSymbol?[] arguments = new ITypeSymbol?[method.TypeParameters.Length];

        for (int i = 0; i < receiverType.TypeArguments.Length; i++)
        {
            if (receiverType.TypeArguments[i] is ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method } parameter &&
                SymbolEqualityComparer.Default.Equals(parameter.DeclaringMethod, method))
            {
                arguments[parameter.Ordinal] = type.TypeParameters[i];
            }
        }

        if (arguments.Any(argument => argument is null))
            return null;

        IMethodSymbol closed = method.Construct(arguments!);

        // A by-ref receiver is a pointer-receiver method answered by its ж<T> twin, unless it is copy-bound, a value-set
        // method bound through a copy of its receiver, which is answered by itself.
        bool pointer = boxed || receiver.RefKind != RefKind.None && method.IsPointerSetMethod();
        bool viaTwin = pointer && !boxed;

        List<string> parameters = [];

        for (int i = 0; i < closed.Parameters.Length; i++)
        {
            IParameterSymbol parameter = closed.Parameters[i];
            string spelled = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (i == 0 && viaTwin)
            {
                parameters.Add($"global::go.ж<{spelled}> recv");
                continue;
            }

            string modifier = parameter.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.In => "in ",
                RefKind.Out => "out ",
                _ => ""
            };

            parameters.Add($"{modifier}{spelled} arg{i}");
        }

        string returns = closed.ReturnsVoid ? "void" : closed.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string typeArguments = string.Join(", ", arguments.Select(argument => argument!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        string name = SyntaxFacts.GetKeywordKind(method.Name) != SyntaxKind.None ? "@" + method.Name : method.Name;

        return (method.MetadataName, pointer, $"{returns} ᴛNAME({string.Join(", ", parameters)})",
            $"{package.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{name}<{typeArguments}>");
    }

    private static bool IsBox(ITypeSymbol type, out ITypeSymbol element)
    {
        if (type is INamedTypeSymbol { Name: "ж", TypeArguments: [var target] } box && box.ContainingNamespace?.ToDisplayString() == "go")
        {
            element = target;
            return true;
        }

        element = type;
        return false;
    }
}
