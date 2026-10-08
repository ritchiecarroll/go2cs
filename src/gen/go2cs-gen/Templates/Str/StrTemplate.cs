// StrTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.Str;

// Renders an sstring twin's @string forwarder and, for a package-level function, its canonical value
// delegate (see StrGenerator). Types are rendered fully qualified from the symbols, never from
// the source file's text: the converted file's `using` aliases (`Δio = io_package`, `ꓸꓸꓸany =
// Span<any>`) are not in scope in a generated file.
internal class StrTemplate : TemplateBase
{
    private const string SStringType = "go.sstring";

    private static readonly SymbolDisplayFormat s_typeFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    // Template Parameters
    public required IMethodSymbol Method;

    // The declared parameter names as SPELLED (`@base` keeps its escape; the symbol's name does not).
    public required string[] ParameterNames;

    // The body member's [MethodImpl(NoInlining)] mark, which the forwarder repeats (see RecvGenerator).
    public required bool NoInlining;

    private bool IsTwinned(IParameterSymbol parameter) => parameter.Type.ToDisplayString() == SStringType;

    // The forwarder re-declares the signature from the symbols; a generic method, a ref return or a
    // parameter list whose names do not line up with the symbols is outside what it can render.
    public bool IsRenderable =>
        !Method.IsGenericMethod &&
        !Method.ReturnsByRef && !Method.ReturnsByRefReadonly &&
        ParameterNames.Length == Method.Parameters.Length &&
        Method.Parameters.Any(IsTwinned);

    private bool IsExtension => Method.IsExtensionMethod;

    private string Access => Method.DeclaredAccessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Private => "private",
        _ => "internal"
    };

    // The @string type of the parameter's twin, or the parameter's own type.
    private string ParameterType(IParameterSymbol parameter) =>
        IsTwinned(parameter) ? "global::go.@string" : parameter.Type.ToDisplayString(s_typeFormat);

    private static string RefKindPrefix(RefKind refKind) => refKind switch
    {
        RefKind.Ref => "ref ",
        RefKind.Out => "out ",
        RefKind.In => "in ",
        RefKind.RefReadOnlyParameter => "ref readonly ",
        _ => ""
    };

    private string ForwarderParameters => string.Join(", ", Method.Parameters.Select((parameter, index) =>
        (index == 0 && IsExtension ? "this " : "") +
        (parameter.IsParams ? "params " : "") +
        RefKindPrefix(parameter.RefKind) +
        $"{ParameterType(parameter)} {ParameterNames[index]}"));

    // The body member's arguments: each twinned parameter converted to its view, explicitly, so the call
    // reads as the forward it is (overload resolution would bind the body member either way).
    private string ForwardArguments => string.Join(", ", Method.Parameters
        .Select((parameter, index) => (parameter, index))
        .Where(item => !(item.index == 0 && IsExtension))
        .Select(item => RefKindPrefix(item.parameter.RefKind) + (IsTwinned(item.parameter) ? $"(global::go.sstring){ParameterNames[item.index]}" : ParameterNames[item.index])));

    private string CallTarget => IsExtension ? $"{ParameterNames[0]}.{Method.Name}" : Method.Name;

    private string ReturnType => Method.ReturnsVoid ? "void" : Method.ReturnType.ToDisplayString(s_typeFormat);

    // The body member's [GoRecv], which the forwarder repeats where a hand-written body carries it,
    // so the two read alike. The forwarder re-declares the same `this ref T` receiver, which golib
    // reads as a POINTER-set method whether marked or not (TypeExtensions.IsPointerSetByRefReceiver;
    // only [GoCopyBound] makes a by-ref receiver value-set, and converted code never writes it on a
    // body). RecvGenerator never sees the forwarder (a generator sees no generator output), so its
    // receiver mints no ж overload.
    private string GoRecvMark => Method.GetAttributes().Any(attribute => attribute.AttributeClass is
    {
        Name: "GoRecvAttribute",
        ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true }
    })
        ? ", global::go.GoRecv"
        : "";

    private string ForwarderAttributes => NoInlining
        ? $"[{GeneratedCodeAttribute}, global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining), global::System.Runtime.CompilerServices.OverloadResolutionPriority(-1), {NonUserCodeAttribute}{GoRecvMark}]"
        : $"[{GeneratedCodeAttribute}, global::System.Runtime.CompilerServices.OverloadResolutionPriority(-1), {NonUserCodeAttribute}{GoRecvMark}]";

    // A delegate TYPE ARGUMENT: a tuple loses its element names, which delegate identity ignores, so
    // the field's type reads as the converter's rendering of the same Go func type at every value site.
    private static string TypeArgument(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsTupleType: true } tuple
            ? $"({string.Join(", ", tuple.TupleElements.Select(element => TypeArgument(element.Type)))})"
            : type.ToDisplayString(s_typeFormat);

    // The canonical delegate's type: golib's variadic family for a `params Span<T>` tail (the converter's
    // rendering of `...T`), otherwise System.Func / System.Action.
    private string DelegateType
    {
        get
        {
            List<string> typeArguments = [];
            bool variadic = false;

            foreach (IParameterSymbol parameter in Method.Parameters)
            {
                if (parameter.IsParams && parameter.Type is INamedTypeSymbol { Name: "Span", TypeArguments.Length: 1 } span)
                {
                    variadic = true;
                    typeArguments.Add(TypeArgument(span.TypeArguments[0]));
                    continue;
                }

                typeArguments.Add(IsTwinned(parameter) ? "global::go.@string" : TypeArgument(parameter.Type));
            }

            if (!Method.ReturnsVoid)
                typeArguments.Add(TypeArgument(Method.ReturnType));

            string family = (variadic, Method.ReturnsVoid) switch
            {
                (true, true) => $"global::go.Action{EllipsisOperator}",
                (true, false) => $"global::go.Func{EllipsisOperator}",
                (false, true) => "global::System.Action",
                _ => "global::System.Func"
            };

            return typeArguments.Count == 0 ? family : $"{family}<{string.Join(", ", typeArguments)}>";
        }
    }

    private string LambdaParameters => string.Join(", ", Method.Parameters.Select((parameter, index) =>
        $"{ParameterType(parameter)} {ParameterNames[index]}"));

    private string LambdaArguments => string.Join(", ", ParameterNames);

    // The canonical value delegate exists for a package-level function only: a method value binds its
    // receiver, so no single delegate can stand for it. A by-ref parameter has no delegate spelling here.
    private bool HasCanonicalDelegate =>
        !IsExtension && Method.Parameters.All(parameter => parameter.RefKind == RefKind.None);

    private string CanonicalDelegate => HasCanonicalDelegate
        ? $$"""
            
                // The canonical func value of {{Method.Name}}: a twinned function has no single method group (CS0123).
                {{Access}} static readonly {{DelegateType}} {{Method.Name}}{{FuncValueMarker}} = [global::go.GoTwinForwarder("{{Method.Name}}")] static ({{LambdaParameters}}) => {{Method.Name}}({{LambdaArguments}});

            """
        : "";

    public override string TemplateBody =>
        $$"""
            // The @string member of the sstring twin {{Method.Name}}: it forwards to the member that carries the Go body.
            {{ForwarderAttributes}}
            {{Access}} static {{ReturnType}} {{Method.Name}}({{ForwarderParameters}}) => {{CallTarget}}({{ForwardArguments}});
        {{CanonicalDelegate}}
        """;
}
