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
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
/// value or the receiver itself (G's ruling, 2026-10-19). Converted programs publish with a partial trim, which keeps
/// every method of a constructed type, so the arms cost their size wherever the type is constructed (measured +2.59 MB on
/// a fmt-only program; the owner's principle accepts it: parity outranks size).
/// </para>
/// <para>
/// Each arm is keyed by the method's EMITTED name, which is what golib's method table holds, and by whether it is
/// reached through <c>*T</c>: a value method has an arm for each, a pointer method only the <c>*T</c> one. A
/// <c>*T</c> arm takes the <c>ж&lt;T&gt;</c> box; a value method reached through it reads the pointee at each
/// call, the method value's too, as Go's reflect does, and a nil <c>*T</c> panics there with Go's text. A pointer
/// method emitted <c>this ref T</c> is called through RecvGenerator's <c>ж&lt;T&gt;</c> twin, which this generator
/// cannot see but the call names by its box argument.
/// </para>
/// <para>
/// The type's own methods are read from the compilation. Its PROMOTED methods are the forwarders this generator itself
/// writes into the type's generated part (the struct template's promoted receivers, and the sibling class of
/// cross-package ones), which the compilation does not hold yet, so they are read from that part's syntax: the same
/// methods golib's method table finds at run time.
/// </para>
/// <para>
/// A variadic tail takes golib's <c>Funcꓸꓸꓸ</c>/<c>Actionꓸꓸꓸ</c> family and more than 16 parameters golib's
/// <c>Func</c>/<c>Action</c> ladder, the types golib's reflect builds for those signatures. A signature past every
/// family (golib mints its delegate type at run time, so source cannot name it) has no arm, and golib binds it as
/// before.
/// </para>
/// </remarks>
internal static class MethodSet
{
    private const string Interface = "global::go.IGoMethodSet";
    private const int BclDelegateParameters = 16;
    private const string Ellipsis = "ꓸꓸꓸ";

    // How a method takes its receiver: by value; by reference but as a value method, bound through a copy
    // ([GoCopyBound]); or as a pointer method, called through a ж<T> box.
    private enum Receiver { Value, CopyBound, Pointer }

    // One Go method as the arms call it: the call target, the fixed parameter types, the variadic tail's element type
    // (or null), the return type ("" for void), and whether the type promotes it from an embedded field. Every type is
    // spelled so it binds inside the type's own part.
    private readonly record struct Shape(string Name, Receiver Receiver, string Target, IReadOnlyList<string> Parameters, string? Variadic, string Returns,
        bool Promoted = false);

    /// <summary>
    /// The source of a generated part of <paramref name="type"/> implementing <c>IGoMethodSet</c>, or <c>null</c> when it
    /// is not a value type, has no exported Go method with a delegate form, or cannot be reopened.
    /// </summary>
    /// <param name="ns">The package namespace.</param>
    /// <param name="type">The type.</param>
    /// <param name="compilation">The compilation, which declares golib's delegate families.</param>
    /// <param name="generatedPart">The type's own generated part, which holds its promoted-method forwarders.</param>
    public static string? Source(string ns, INamedTypeSymbol? type, Compilation compilation, string generatedPart)
    {
        if (type is not { IsValueType: true, IsRefLikeType: false } || !GeneratedPartials.CanReopen(type))
            return null;

        INamedTypeSymbol? package = type.ContainingType;

        while (package?.ContainingType is not null)
            package = package.ContainingType;

        if (package is null || package.IsGenericType)
            return null;

        string self = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        List<(string name, bool pointer, string? unbound, string? bound, string? register)> arms = [];
        HashSet<(string, bool)> seen = [];

        // The type's own methods first: a method it declares hides every promotion of that name (Go's rule), and the
        // template already writes no forwarder for one, so this only orders a set that cannot collide.
        IEnumerable<Shape> shapes = package.GetMembers().OfType<IMethodSymbol>()
            .Select(method => Declared(method, type, package))
            .OfType<Shape>()
            .Concat(Promoted(type, generatedPart));

        foreach (Shape shape in shapes)
        {
            foreach ((string name, bool pointer, string? unbound, string? bound, string? register) arm in Arms(shape, self, compilation))
            {
                if ((arm.unbound is not null || arm.bound is not null) && seen.Add((arm.name, arm.pointer)))
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

        foreach ((string name, bool pointer, string? unbound, string? bound, string? register) in arms)
        {
            body.Append($"            case ({SymbolDisplay.FormatLiteral(name, true)}, {(pointer ? "true" : "false")}):\r\n");

            if (register is not null)
                body.Append(register);

            body.Append($"                if (!bound)\r\n");
            body.Append($"                    return {unbound ?? "null"};\r\n");

            if (bound is null)
            {
                body.Append($"                return null;\r\n");
                continue;
            }

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

    // A method the type declares, or null when it is not an exported Go method of `type` with a delegate form.
    private static Shape? Declared(IMethodSymbol method, INamedTypeSymbol type, INamedTypeSymbol package)
    {
        if (!method.IsExtensionMethod || method.Parameters.Length == 0 || method.ReturnsByRef || method.ReturnsByRefReadonly || !IsExported(method.Name))
            return null;

        IParameterSymbol receiver = method.Parameters[0];
        bool boxed = IsBox(receiver.Type, out ITypeSymbol element);

        if (element is not INamedTypeSymbol receiverType || !SymbolEqualityComparer.Default.Equals(receiverType.OriginalDefinition, type.OriginalDefinition))
            return null;

        IMethodSymbol closed = method;
        string typeArguments = "";

        if (type.IsGenericType)
        {
            // A Go method declares no type parameters of its own, so each of the emitted method's type parameters is one
            // of the receiver's type arguments; map each to the type's parameter at that position (MethodSource's rule).
            if (!method.IsGenericMethod)
                return null;

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

            closed = method.Construct(arguments!);
            typeArguments = $"<{string.Join(", ", arguments.Select(argument => argument!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))}>";
        }
        else if (method.IsGenericMethod || !SymbolEqualityComparer.Default.Equals(receiverType, type))
        {
            return null;
        }

        List<string> parameters = [];
        string? variadic = null;

        for (int i = 1; i < closed.Parameters.Length; i++)
        {
            IParameterSymbol parameter = closed.Parameters[i];

            // A by-ref parameter has no Func/Action form. A Span<T> is Go's variadic tail, only as the last parameter.
            if (parameter.RefKind != RefKind.None || parameter.HasExplicitDefaultValue)
                return null;

            if (IsSpan(parameter.Type, out ITypeSymbol? tail))
            {
                if (i != closed.Parameters.Length - 1)
                    return null;

                variadic = tail.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                continue;
            }

            if (parameter.Type.IsRefLikeType)
                return null;

            parameters.Add(parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        string name = SyntaxFacts.GetKeywordKind(method.Name) != SyntaxKind.None ? "@" + method.Name : method.Name;

        // A by-ref receiver is a pointer-receiver method (called through its ж<T> twin) unless it is copy-bound, a value
        // method bound through a copy of its receiver.
        Receiver kind = boxed || receiver.RefKind != RefKind.None && method.IsPointerSetMethod() ? Receiver.Pointer :
            receiver.RefKind != RefKind.None ? Receiver.CopyBound : Receiver.Value;

        return new Shape(method.Name, kind, $"{package.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{name}{typeArguments}",
            parameters, variadic, closed.ReturnsVoid ? "" : closed.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

    // The type's promoted methods: the forwarders in its generated part whose receiver is the type (`this T`,
    // `this ref T` with [GoRecv] or [GoCopyBound], or `this ж<T>`), in the package class and in the sibling class of
    // cross-package ones. A pointer method's `this ref T` forwarder and its ж<T> twin are one method, called through the
    // twin, and the template writes a value method's ж<T> forwarder beside it too, so each name is read once, from its
    // non-box form when it has one.
    private static IEnumerable<Shape> Promoted(INamedTypeSymbol type, string generatedPart)
    {
        if (!generatedPart.Contains("static "))
            yield break;

        string typeName = type.Name + (type.TypeParameters.Length > 0 ? $"<{string.Join(",", type.TypeParameters.Select(parameter => parameter.Name))}>" : "");
        bool handOwned = type.ContainingAssembly.IsHandOwnedPackage();
        Dictionary<string, (MethodDeclarationSyntax method, bool boxed)> byName = new(System.StringComparer.Ordinal);
        List<string> order = [];

        foreach (MethodDeclarationSyntax method in CSharpSyntaxTree.ParseText(generatedPart).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (!method.Modifiers.Any(SyntaxKind.StaticKeyword) || method.ParameterList.Parameters.FirstOrDefault() is not { } receiver ||
                !receiver.Modifiers.Any(SyntaxKind.ThisKeyword) || receiver.Type is null || !IsExported(method.Identifier.ValueText) ||
                method.Parent is not ClassDeclarationSyntax)
            {
                continue;
            }

            string receiverText = Unqualified(receiver.Type.ToString(), out bool boxed);

            if (receiverText != typeName)
                continue;

            if (!byName.TryGetValue(method.Identifier.ValueText, out (MethodDeclarationSyntax method, bool boxed) kept))
                order.Add(method.Identifier.ValueText);
            else if (!kept.boxed || boxed)
                continue;

            byName[method.Identifier.ValueText] = (method, boxed);
        }

        foreach (string methodName in order)
        {
            (MethodDeclarationSyntax method, bool boxed) = byName[methodName];

            if (method.ReturnType is RefTypeSyntax || method.Parent is not ClassDeclarationSyntax owner)
                continue;

            List<string> parameters = [];
            string? variadic = null;
            bool renderable = true;
            SeparatedSyntaxList<ParameterSyntax> declared = method.ParameterList.Parameters;

            for (int i = 1; i < declared.Count && renderable; i++)
            {
                ParameterSyntax parameter = declared[i];

                // A by-ref parameter, a caller-info attribute or a default has no Func/Action form.
                if (parameter.Type is null || parameter.Default is not null || parameter.AttributeLists.Count > 0 ||
                    parameter.Modifiers.Any(modifier => !modifier.IsKind(SyntaxKind.ParamsKeyword)))
                {
                    renderable = false;
                    break;
                }

                if (parameter.Modifiers.Any(SyntaxKind.ParamsKeyword))
                {
                    if (i != declared.Count - 1 || LastName(parameter.Type) is not GenericNameSyntax { Identifier.ValueText: "Span", TypeArgumentList.Arguments: [var element] })
                    {
                        renderable = false;
                        break;
                    }

                    variadic = element.ToString();
                    continue;
                }

                parameters.Add(parameter.Type.ToString());
            }

            if (!renderable)
                continue;

            Receiver kind = boxed || method.IsPointerSetMethod(handOwned) ? Receiver.Pointer :
                method.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.RefKeyword) ? Receiver.CopyBound : Receiver.Value;

            string returns = method.ReturnType.ToString() is var text && text == "void" ? "" : method.ReturnType.ToString();

            yield return new Shape(method.Identifier.ValueText, kind, $"{OwnerName(owner)}.{method.Identifier.Text}{method.TypeParameterList?.ToString() ?? ""}",
                parameters, variadic, returns, Promoted: true);
        }

        // The forwarder's class, fully qualified: the package class, or the sibling class at namespace level.
        string OwnerName(ClassDeclarationSyntax owner)
        {
            List<string> names = [];

            for (SyntaxNode? node = owner; node is not null; node = node.Parent)
            {
                switch (node)
                {
                    case ClassDeclarationSyntax declaration:
                        names.Insert(0, declaration.Identifier.Text);
                        break;
                    case BaseNamespaceDeclarationSyntax space:
                        names.Insert(0, space.Name.ToString());
                        break;
                }
            }

            return "global::" + string.Join(".", names);
        }
    }

    // The arms of one method: the *T arm for a pointer method and both for a value method. Each is the unbound func's
    // expression and the binder's statements, which read `receiver`; either is null when its signature has no delegate
    // family.
    private static IEnumerable<(string name, bool pointer, string? unbound, string? bound, string? register)> Arms(Shape shape, string self, Compilation compilation)
    {
        string box = $"global::go.ж<{self}>";
        List<string> names = [.. shape.Parameters.Select((_, i) => $"{TempVarMarker}arg{i + 1}")];
        List<string> declarations = [.. shape.Parameters.Zip(names, (t, n) => $"{t} {n}")];

        if (shape.Variadic is not null)
        {
            names.Add($"{TempVarMarker}arg{names.Count + 1}");
            declarations.Add($"global::System.Span<{shape.Variadic}> {names[^1]}");
        }

        string arguments = string.Concat(names.Select(argument => ", " + argument));
        string lambda = string.Join(", ", declarations);
        string? boundDelegate = Delegate(compilation, shape.Parameters, shape.Variadic, shape.Returns);

        string? Bound(string bind, string call) =>
            boundDelegate is null ? null : $"{bind} return {boundDelegate}(({lambda}) => {call});";

        string? Unbound(string receiverType, string call) =>
            Delegate(compilation, [receiverType, .. shape.Parameters], shape.Variadic, shape.Returns) is { } unboundDelegate
                ? $"{unboundDelegate}(({string.Join(", ", new[] { $"{receiverType} {TempVarMarker}recv" }.Concat(declarations))}) => {call})"
                : null;

        // An arm that only passes its receiver and arguments through is the method itself, a method group cast as the
        // converter emits a method expression, so reflect's func identity (keyed on the delegate's method) agrees.
        string? Method(string receiverType) =>
            Delegate(compilation, [receiverType, .. shape.Parameters], shape.Variadic, shape.Returns) is { } unboundDelegate
                ? $"{unboundDelegate}({shape.Target})"
                : null;

        // A variadic arm's delegates are called (Value.Call) through a trampoline closed over their signatures, which the arm
        // registers before handing one out, so compiled code closes it (golib's GoReflect.VariadicRegistry.cs).
        string? Register(string receiverType) => shape.Variadic is null ? null : string.Concat(
            new[] { Registration(compilation, [receiverType, .. shape.Parameters], shape.Variadic, shape.Returns), Registration(compilation, shape.Parameters, shape.Variadic, shape.Returns) }
                .OfType<string>().Select(line => $"                {line}\r\n"));

        if (shape.Receiver == Receiver.Pointer)
        {
            yield return (shape.Name, true,
                Method(box),
                Bound($"{box} {TempVarMarker}p = ({box})receiver!;", $"{shape.Target}({TempVarMarker}p{arguments})"),
                Register(box));

            yield break;
        }

        // A value method: called on a copy. A copy-bound one takes its receiver by reference, so each call gets a fresh
        // copy of its own, never the bound value or the pointee itself.
        string valueCall(string receiverValue) => shape.Receiver == Receiver.CopyBound
            ? $"{{ {self} {TempVarMarker}c = {receiverValue}; {(shape.Returns.Length == 0 ? "" : "return ")}{shape.Target}(ref {TempVarMarker}c{arguments}); }}"
            : $"{shape.Target}({receiverValue}{arguments})";

        // Through *T the pointee is read at each CALL, the method value too, as Go's reflect reads it: its method value
        // holds the pointer, and the autogenerated (*T).M wrapper dereferences it when called. A nil *T is Go's panicwrap
        // for the type's own method and the nil-pointer fault for a promoted one, both raised at the call.
        string pointee(string pointer) => shape.Promoted
            ? $"global::go.builtin.wrapperRecv({pointer}).Value"
            : $"global::go.GoReflect.ValueMethodRecv({pointer}, \"{shape.Name}\").Value";

        yield return (shape.Name, false,
            shape.Receiver == Receiver.Value ? Method(self) : Unbound(self, valueCall($"{TempVarMarker}recv")),
            Bound($"{self} {TempVarMarker}r = ({self})receiver!;", valueCall($"{TempVarMarker}r")),
            Register(self));

        yield return (shape.Name, true,
            Unbound(box, valueCall(pointee($"{TempVarMarker}recv"))),
            Bound($"{box} {TempVarMarker}p = ({box})receiver!;", valueCall(pointee($"{TempVarMarker}p"))),
            Register(box));
    }

    // The registration of a variadic signature's trampolines, the statement golib's GoReflect.RegisterVariadicFunc or
    // RegisterVariadicAction takes with the type arguments Delegate casts to; null when golib declares no such family.
    private static string? Registration(Compilation compilation, IReadOnlyList<string> parameters, string variadic, string returns)
    {
        string family = returns.Length == 0 ? "Action" : "Func";

        if (Delegate(compilation, parameters, variadic, returns) is null ||
            compilation.GetTypeByMetadataName("go.GoReflect") is not { } reflect || reflect.GetMembers($"RegisterVariadic{family}").IsEmpty)
        {
            return null;
        }

        List<string> arguments = [.. parameters, variadic];

        if (returns.Length > 0)
            arguments.Add(returns);

        return $"global::go.GoReflect.RegisterVariadic{family}<{string.Join(", ", arguments)}>();";
    }

    // A cast to the delegate type golib's reflect builds for the signature (GoReflect.MakeDelegateType): the BCL
    // Func/Action through 16 parameters, golib's ladder past them, and golib's ꓸꓸꓸ family for a variadic tail, each asked
    // of the compilation so a rung golib adds is reached the day it exists. Null when no family declares one.
    private static string? Delegate(Compilation compilation, IReadOnlyList<string> parameters, string? variadic, string returns)
    {
        bool action = returns.Length == 0;
        List<string> arguments = [.. parameters];

        if (variadic is not null)
            arguments.Add(variadic);

        if (!action)
            arguments.Add(returns);

        if (variadic is not null)
        {
            string family = (action ? "Action" : "Func") + Ellipsis;

            return compilation.GetTypeByMetadataName($"go.{family}`{arguments.Count}") is null ? null : $"(global::go.{family}<{string.Join(", ", arguments)}>)";
        }

        if (parameters.Count <= BclDelegateParameters)
        {
            if (arguments.Count == 0)
                return "(global::System.Action)";

            return $"(global::System.{(action ? "Action" : "Func")}<{string.Join(", ", arguments)}>)";
        }

        string rung = action ? "Action" : "Func";

        return compilation.GetTypeByMetadataName($"go.{rung}`{arguments.Count}") is null ? null : $"(global::go.{rung}<{string.Join(", ", arguments)}>)";
    }

    // Exported as Go judges it: the emitted name's first letter, after the collision marker's strip.
    private static bool IsExported(string emittedName)
    {
        string projected = emittedName.Length > ShadowVarMarker.Length && emittedName.StartsWith(ShadowVarMarker) ? emittedName.Substring(ShadowVarMarker.Length) : emittedName;

        return projected.Length > 0 && char.IsUpper(projected.TrimStart('@')[0]);
    }

    // A receiver's type text (its `ref` is a modifier, not part of it) reduced to the type's own name and type
    // parameters, with whether it was a ж<T> box: `global::go.pkg_package.T`, `ж<T>` and `T` all read `T`.
    private static string Unqualified(string typeText, out bool boxed)
    {
        string text = string.Concat(typeText.Where(character => !char.IsWhiteSpace(character))).Replace("@", "");

        boxed = false;
        int open = text.IndexOf('<');
        string head = open < 0 ? text : text.Substring(0, open);

        if ((head == "ж" || head.EndsWith(".ж", System.StringComparison.Ordinal)) && text.EndsWith(">", System.StringComparison.Ordinal))
        {
            boxed = true;
            text = text.Substring(open + 1, text.Length - open - 2);
        }

        int depth = 0, start = 0;

        for (int i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '<':
                    depth++;
                    break;
                case '>':
                    depth--;
                    break;
                case '.' when depth == 0:
                    start = i + 1;
                    break;
                case ':' when depth == 0:
                    start = i + 1;
                    break;
            }
        }

        return text.Substring(start);
    }

    private static NameSyntax? LastName(TypeSyntax type) => type switch
    {
        QualifiedNameSyntax qualified => qualified.Right,
        AliasQualifiedNameSyntax alias => alias.Name,
        NameSyntax name => name,
        _ => null
    };

    private static bool IsSpan(ITypeSymbol type, out ITypeSymbol? element)
    {
        if (type is INamedTypeSymbol { Name: "Span", TypeArguments: [var argument], ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } })
        {
            element = argument;
            return true;
        }

        element = null;
        return false;
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
