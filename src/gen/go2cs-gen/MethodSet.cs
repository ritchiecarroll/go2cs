// MethodSet.cs - Gbtc
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
/// Writes golib's <c>IGoMethodSet</c> on a generated part of every value type with exported Go methods: for each one,
/// the unbound func reflect's <c>Type.Method(i).Func</c> hands out and the binder behind <c>Value.Method(i)</c>, both as
/// compiled delegates.
/// </summary>
/// <remarks>
/// <para>
/// Reflect built both at run time, binding the method's <c>MethodInfo</c> into a <c>Func</c>/<c>Action</c> it
/// instantiated over the receiver's type. Under Native AOT that instantiation has no native code for a value-type
/// receiver, so no method func and no method value of a struct could be built. Here compiled code creates each delegate,
/// so the instantiations exist for every type the program constructs.
/// </para>
/// <para>
/// The arms are reached ONLY through the interface slot, which only reflect's method paths call, through the type's zero
/// value or the receiver itself (G's ruling, 2026-10-19). A program with no reachable reflect method access calls no
/// implementation of the slot, so Native AOT compiles none of the arms, as Go's linker drops the method table.
/// </para>
/// <para>
/// Each arm is keyed by the method's EMITTED name, which is what golib's method table holds, and by whether it is
/// reached through <c>*T</c>: a value method has an arm for each, a pointer method only the <c>*T</c> one. A
/// <c>*T</c> arm takes the <c>ж&lt;T&gt;</c> box; a value method reached through it copies the pointee, at the call for
/// the unbound func and at the bind for a method value (Go's rule). A pointer method emitted <c>this ref T</c> is called
/// through RecvGenerator's <c>ж&lt;T&gt;</c> twin, which this generator cannot see but the call names by its box
/// argument. A method whose signature has no BCL <c>Func</c>/<c>Action</c> form (a variadic tail, more than 16
/// parameters) has no arm, and golib binds it as before.
/// </para>
/// </remarks>
internal static class MethodSet
{
    private const string Interface = "global::go.IGoMethodSet";
    private const int MaxDelegateParameters = 16;

    /// <summary>
    /// The source of a generated part of <paramref name="type"/> implementing <c>IGoMethodSet</c>, or <c>null</c> when it
    /// is not a value type, has no exported Go method with a delegate form, or cannot be reopened.
    /// </summary>
    public static string? Source(string ns, INamedTypeSymbol? type)
    {
        if (type is not { IsValueType: true, IsRefLikeType: false } || !GeneratedPartials.CanReopen(type))
            return null;

        INamedTypeSymbol? package = type.ContainingType;

        while (package?.ContainingType is not null)
            package = package.ContainingType;

        if (package is null || package.IsGenericType)
            return null;

        List<(string name, bool pointer, string unbound, string bound)> arms = [];
        HashSet<(string, bool)> seen = [];

        foreach (IMethodSymbol method in package.GetMembers().OfType<IMethodSymbol>())
        {
            foreach ((string name, bool pointer, string unbound, string bound) arm in Arms(method, type, package))
            {
                if (seen.Add((arm.name, arm.pointer)))
                    arms.Add(arm);
            }
        }

        if (arms.Count == 0)
            return null;

        StringBuilder body = new();

        body.Append($"    global::System.Delegate? {Interface}.{TempVarMarker}Method(string method, bool pointer, bool bound, object? receiver)\r\n");
        body.Append("    {\r\n");
        body.Append("        switch ((method, pointer))\r\n");
        body.Append("        {\r\n");

        foreach ((string name, bool pointer, string unbound, string bound) in arms)
        {
            body.Append($"            case ({SymbolDisplay.FormatLiteral(name, true)}, {(pointer ? "true" : "false")}):\r\n");
            body.Append($"                if (!bound)\r\n");
            body.Append($"                    return {unbound};\r\n");
            body.Append($"                {{\r\n");
            body.Append($"                    {bound}\r\n");
            body.Append($"                }}\r\n");
        }

        body.Append("            default:\r\n");
        body.Append("                return null;\r\n");
        body.Append("        }\r\n");
        body.Append("    }\r\n");

        return GeneratedPartials.Source(ns, type, [], $" : {Interface}", body.ToString());
    }

    // The arms of one method: none when it is not an exported Go method of `type` with a delegate form, the *T arm for a
    // pointer method, and both for a value method. Each is the unbound func's expression and the binder's statements,
    // which read `receiver`.
    private static IEnumerable<(string name, bool pointer, string unbound, string bound)> Arms(IMethodSymbol method, INamedTypeSymbol type, INamedTypeSymbol package)
    {
        if (!method.IsExtensionMethod || method.Parameters.Length == 0 || method.ReturnsByRef || method.ReturnsByRefReadonly)
            yield break;

        string projected = method.Name.Length > ShadowVarMarker.Length && method.Name.StartsWith(ShadowVarMarker) ? method.Name.Substring(ShadowVarMarker.Length) : method.Name;

        if (!char.IsUpper(projected[0]))
            yield break;

        IParameterSymbol receiver = method.Parameters[0];
        bool boxed = IsBox(receiver.Type, out ITypeSymbol element);

        if (element is not INamedTypeSymbol receiverType || !SymbolEqualityComparer.Default.Equals(receiverType.OriginalDefinition, type.OriginalDefinition))
            yield break;

        IMethodSymbol closed = method;
        string typeArguments = "";

        if (type.IsGenericType)
        {
            // A Go method declares no type parameters of its own, so each of the emitted method's type parameters is one
            // of the receiver's type arguments; map each to the type's parameter at that position (MethodSource's rule).
            if (!method.IsGenericMethod)
                yield break;

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
                yield break;

            closed = method.Construct(arguments!);
            typeArguments = $"<{string.Join(", ", arguments.Select(argument => argument!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))}>";
        }
        else if (method.IsGenericMethod || !SymbolEqualityComparer.Default.Equals(receiverType, type))
        {
            yield break;
        }

        List<string> types = [];
        List<string> names = [];

        for (int i = 1; i < closed.Parameters.Length; i++)
        {
            IParameterSymbol parameter = closed.Parameters[i];

            // A by-ref parameter has no Func/Action form; a Span<T> is Go's variadic tail, whose family golib names.
            if (parameter.RefKind != RefKind.None || parameter.Type is INamedTypeSymbol { Name: "Span", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } || parameter.Type.IsRefLikeType)
                yield break;

            types.Add(parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            names.Add($"arg{i}");
        }

        if (types.Count + 1 > MaxDelegateParameters)
            yield break;

        string returns = closed.ReturnsVoid ? "" : closed.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string self = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string box = $"global::go.ж<{self}>";
        string name = SyntaxFacts.GetKeywordKind(method.Name) != SyntaxKind.None ? "@" + method.Name : method.Name;
        string target = $"{package.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{name}{typeArguments}";
        string arguments_ = string.Concat(names.Select(argument => ", " + argument));
        string parameters = string.Join(", ", types.Zip(names, (t, n) => $"{t} {n}"));

        // A by-ref receiver is a pointer-receiver method (called through its ж<T> twin) unless it is copy-bound, a value
        // method bound through a copy of its receiver.
        bool pointerMethod = boxed || receiver.RefKind != RefKind.None && method.IsPointerSetMethod();
        bool copyBound = !boxed && receiver.RefKind != RefKind.None && !pointerMethod;

        if (pointerMethod)
        {
            yield return (method.Name, true,
                Unbound(box, "recv", $"{target}(recv{arguments_})"),
                $"{box} p = ({box})receiver!; return {Delegate(types, returns)}(({parameters}) => {target}(p{arguments_}));");

            yield break;
        }

        // A value method: called on a copy. A copy-bound one takes its receiver by reference, so each call gets a fresh
        // copy of its own, never the bound value or the pointee itself.
        string valueCall(string receiverValue) => copyBound
            ? $"{{ {self} c = {receiverValue}; {(returns.Length == 0 ? "" : "return ")}{target}(ref c{arguments_}); }}"
            : $"{target}({receiverValue}{arguments_})";

        string boundValue = $"return {Delegate(types, returns)}(({parameters}) => {valueCall("r")});";

        yield return (method.Name, false,
            Unbound(self, "recv", valueCall("recv")),
            $"{self} r = ({self})receiver!; {boundValue}");

        yield return (method.Name, true,
            Unbound(box, "recv", valueCall("recv.Value")),
            $"{self} r = (({box})receiver!).Value; {boundValue}");

        string Unbound(string receiverType_, string receiverName, string call) =>
            $"{Delegate([receiverType_, .. types], returns)}(({string.Join(", ", new[] { $"{receiverType_} {receiverName}" }.Concat(types.Zip(names, (t, n) => $"{t} {n}")))}) => {call})";
    }

    // A cast to the BCL delegate of that signature: the type golib's reflect builds for it (GoReflect.MakeDelegateType).
    private static string Delegate(IReadOnlyList<string> parameters, string returns)
    {
        if (returns.Length == 0)
            return parameters.Count == 0 ? "(global::System.Action)" : $"(global::System.Action<{string.Join(", ", parameters)}>)";

        return $"(global::System.Func<{string.Join(", ", parameters.Concat([returns]))}>)";
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
