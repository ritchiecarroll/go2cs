// MethodDeclarationSyntaxExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Templates.TemplateBase;
using static go2cs.Symbols;

namespace go2cs;

public record MethodInfo
{
    public required string Name { get; init; }

    public required string ReturnType { get; init; }

    public required (string type, string name)[] Parameters { get; init; }

    public required string GenericTypes { get; init; }

    public required Dictionary<string, string[]> TypeConstraints { get; init; }

    public bool IsRefRecv { get; init; }

    // True when the return type is a PUBLIC C# type transitively (see IsEffectivelyPublicType). The
    // name-based GetScope heuristic reads every Go-lowercase-named type as unexported, but golib
    // builtins (@string, error, bool, nint, slice<T>, …) are PUBLIC C# types despite their lowercase
    // names. A promoted forwarder returning such a type was wrongly downgraded to `internal` and so
    // was invisible cross-assembly (testing.T.Name → @string → CS1929 in x/net/nettest). This lets the
    // promotion machinery keep such a forwarder public where the return type is genuinely accessible.
    // Defaults false so any MethodInfo built without semantic type info falls back to the heuristic.
    public bool ReturnTypeIsPublic { get; init; }

    // The parameter-side twin of ReturnTypeIsPublic — true when every parameter EXCEPT the receiver
    // is a PUBLIC C# type transitively. The receiver slot (Parameters[0] of the extension-shaped
    // symbols Go methods compile to) is excluded by the harvest: its type is the embed itself, which
    // for an unexported embed is internal by design and says nothing about the promoted signature.
    // Both harvest paths — the syntax-tree walk and GetMetadataPromotedMethods' referenced-assembly
    // symbols — route through the symbol-based GetMethodInfo, so both compute this for real.
    public bool ParametersArePublic { get; init; }

    // Set when this is a direct-ж (box-receiver, `this ж<T>`) primary being promoted through a POINTER
    // embed: the promoted forwarder must call it on the embed's BOX hop (`target.<embed>`), not the
    // deref'd value (`target.<embed>.Value`) a value-receiver method uses — the box receiver needs ж<T>.
    public bool IsBoxRecv { get; init; }

    // Set when this is a direct-ж (box-receiver) primary promoted through an UNEXPORTED VALUE embed
    // (`testing.T.common`'s `Errorf`). Unlike a POINTER embed (whose hop `target.<embed>` is already a
    // ж<T>), a VALUE embed hop is a value that cannot bind a ж-receiver, so the converter renders the
    // in-package call as the box-field descent `Ꮡt.of(T.Ꮡ<embed>).M(…)`. That descent uses the embed's
    // `Ꮡ<embed>` box accessor, which is `internal` (matching the unexported embed) and thus invisible
    // cross-assembly (CS0117 — crypto/internal/cryptotest reaching testing.T.common). This flag makes
    // the promoted forwarder emit that descent in a PUBLIC shim, so a foreign package can reach the
    // exported promoted method by the plain `t.M(…)` call.
    public bool IsValueEmbedBoxRecv { get; init; }

    // Set for a cross-assembly UNEXPORTED interface method — Go's package-sealing markers such as
    // ast.Expr's `exprNode()`, ast.Stmt's `stmtNode()`, ast.Decl's `declNode()`, or
    // text/template/parse.Node's `tree()`/`writeTo()`. Its C# implementation is an INTERNAL extension
    // method in the INTERFACE's own assembly (ast's `internal static void exprNode(this ref IndexExpr)`),
    // so an adapter generated in a DIFFERENT assembly (go/internal/typeparams casting go/ast's *IndexExpr
    // to ast.Expr) cannot see it — forwarding `m_box.Value.exprNode()` is CS1061. Go never lets such a
    // method be called from outside its defining package, so the adapter satisfies the still-required
    // (public) interface member with a STUB body instead of forwarding to the inaccessible impl.
    public bool IsInaccessibleMarker { get; init; }

    // True when this method can be forwarded through a REFLECTIVE invoker — every parameter and the
    // return type round-trip through `object`. False for a ref-struct (a Go variadic tail lowers to
    // `params Span<T>`, which cannot be boxed), a by-ref parameter or return, a pointer type, or a
    // generic method. The non-generic object shell is emitted only when every interface method
    // qualifies; the delegate-bound shell, which forwards through a typed delegate, is unaffected.
    // Defaults true so a MethodInfo built without semantic type info keeps the prior behavior.
    public bool IsInvokerForwardable { get; init; } = true;

    // True when this method's signature can be re-declared faithfully from the recorded Parameters:
    // those carry the parameter TYPE only, so a member declared with a ref-kind modifier (`in`/`ref`/
    // `out`) — which no converted Go interface has, but a hand-written base such as the io stub's
    // `Reader.Read(in slice<byte>)` does — would re-declare without it and fail to implement the
    // member (CS0535). An interface with such a member gets no runtime shell rather than a shell that
    // does not compile. Defaults true so a MethodInfo built without semantic info keeps prior behavior.
    public bool IsSignatureRenderable { get; init; } = true;

    // The name a FORWARDING call must spell on the receiver, when that differs from the interface
    // member's own name. The member being IMPLEMENTED always carries the interface's name — an
    // explicit implementation of `Stringer.String` is spelled `String` and may never be renamed —
    // but the implementation it forwards TO carries whatever name the converter emitted for it, and
    // a `-tests` whitebox compilation Δ-renames a test-file method declarator whose bare name would
    // hijack a same-named member the file carries via `using static` (flag_test.go's five
    // `flag.Value` types each declare `String`/`Set`, colliding with the production `flag` package
    // the test variant dot-imports, so all ten emit as `ΔString`/`ΔSet`). Resolved against the
    // methods the struct actually DECLARES — exact name first, the Δ-projection only as a second
    // pass — which is the same two-pass rule golib's shell binder and method-set probe already
    // apply, so the compile-time adapter, the binder and the structural probe cannot disagree about
    // one name (see TypeExtensions.GoMethodNameMatches). Null whenever the emitted name IS the
    // interface member's name, which is every member the collision pass left alone.
    public string? ForwardName { get; init; }

    // The name to spell on the receiver of a forwarding call: the emitted name when the
    // implementation was collision-renamed, otherwise the interface member's own simple name.
    public string ForwardMemberName(string simpleMethodName) => ForwardName ?? simpleMethodName;

    public bool IsGeneric => GenericTypes.Length > 0;

    public string CallParameters => GetCallParameters(true);
    
    // A recorded parameter may carry its DEFAULT in the name slot (the declaration emit is
    // `{type} {name}` and a default has no other legal position), so every CALL emit strips it:
    // passing `method = ""` as an argument is a syntax error.
    public static string ParameterIdentifier(string name)
    {
        int assign = name.IndexOf(" = ", System.StringComparison.Ordinal);
        return assign < 0 ? name : name.Substring(0, assign);
    }

    public string GetCallParameters(bool allowDiscarded)
    {
        return string.Join(", ", Parameters.Select((param, index) =>
        {
            string name = ParameterIdentifier(param.name);

            if (name == "_")
                return allowDiscarded ? "_" : $"p{TempVarMarker}{index}";

            return name;
        }));
    }

    public string TypedParameters => GetTypedParameters(true);

    public string GetTypedParameters(bool allowDiscarded)
    {
        return string.Join(", ", Parameters.Select((param, index) =>
        {
            if (ParameterIdentifier(param.name) == "_")
                return allowDiscarded ? $"{param.type} _" : $"{param.type} p{TempVarMarker}{index}";
            
            return $"{param.type} {param.name}";
        }));
    }

    public string GetSignature(bool allowDiscarded = true)
    {
        // The method name is emitted here as its own declaration-identifier token, so a Go
        // "sealing" method whose name is a C# reserved keyword — testing.TB.private(), the
        // ast.Node markers, encoding/gob's string() — must be `@`-escaped. Names read from
        // Roslyn (IMethodSymbol.Name) arrive UNescaped, so an inherited `private()` pulled in
        // through AllInterfaces would otherwise emit `void private()` (CS1520), corrupting the
        // enclosing class body. EscapeCsKeyword is a no-op for non-keywords and for names that
        // are already escaped or qualified, so this is safe for every caller.
        return $"{EscapeCsKeyword(Name)}{GetGenericSignature()}({GetTypedParameters(allowDiscarded)}){GetWhereConstraints()}";
    }

    public string GetGenericSignature()
    {
        return IsGeneric ? $"<{GenericTypes}>" : "";
    }

    public string GetWhereConstraints()
    {
        if (!IsGeneric || TypeConstraints.Count == 0)
            return string.Empty;

        List<string> constraints = [];

        foreach (KeyValuePair<string, string[]> kvp in TypeConstraints)
        {
            string typeParam = kvp.Key;
            string[] typeConstraints = kvp.Value;

            if (typeConstraints.Length > 0)
                constraints.Add($"where {typeParam} : {string.Join(", ", typeConstraints)}");
        }

        return $"\r\n{TypeElemIndent}{string.Join("\r\n        ", constraints)}";
    }

    public bool IsSameSignature(IMethodSymbol methodSymbol)
    {
        // Compare method names
        if (Name != methodSymbol.Name)
            return false;

        // Compare return types - convert ITypeSymbol to string representation
        string returnTypeString = GlobalQualify(methodSymbol.ReturnType.ToDisplayString());

        if (ReturnType != returnTypeString)
            return false;

        // Compare parameter counts
        if (Parameters.Length != methodSymbol.Parameters.Length)
            return false;

        // Compare parameter types
        for (int i = 0; i < Parameters.Length; i++)
        {
            string paramType = GlobalQualify(methodSymbol.Parameters[i].Type.ToDisplayString());

            if (Parameters[i].type != paramType)
                return false;
        }

        // Compare generic type parameters count
        int genericTypesCount = methodSymbol.TypeParameters.Length;

        string[] genericTypes = string.IsNullOrEmpty(GenericTypes) ?
            [] : GenericTypes.Split(',').Select(type => type.Trim()).ToArray();

        return genericTypes.Length == genericTypesCount;
    }
}

public static class MethodSyntaxExtensions
{
    // True when a type is a PUBLIC C# type transitively — the type itself and every type argument /
    // tuple element / array-or-pointer element is public (or a use-site-bound type parameter, or a
    // builtin special type). golib builtins (@string, error, bool, nint, slice<T>, …) are PUBLIC
    // despite their Go-lowercase names, which the name-based GetScope heuristic misreads as
    // unexported. Used to keep a promoted forwarder returning such a type PUBLIC (visible
    // cross-assembly) rather than wrongly downgrading it to internal (testing.T.Name → CS1929).
    internal static bool IsEffectivelyPublicType(ITypeSymbol? type)
    {
        switch (type)
        {
            case null:
                return false;
            case ITypeParameterSymbol:
                return true;                            // accessibility is bound at the use site
            case IArrayTypeSymbol array:
                return IsEffectivelyPublicType(array.ElementType);
            case IPointerTypeSymbol pointer:
                return IsEffectivelyPublicType(pointer.PointedAtType);
        }

        // A builtin special type (int, string primitive, void, …) is always public.
        if (type.SpecialType != SpecialType.None)
            return true;

        // A public type must not be nested inside a less-accessible one.
        for (ITypeSymbol? enclosing = type; enclosing is not null; enclosing = enclosing.ContainingType)
        {
            if (enclosing.DeclaredAccessibility is not (Accessibility.Public or Accessibility.NotApplicable))
                return false;
        }

        if (type is INamedTypeSymbol named)
        {
            // A ValueTuple's own accessibility is public even when an element is internal, so a Go
            // multi-return forwarder must check each element (CS0051 fires on the least-accessible one).
            if (named.IsTupleType)
                return named.TupleElements.All(element => IsEffectivelyPublicType(element.Type));

            return named.TypeArguments.All(IsEffectivelyPublicType);
        }

        return true;
    }

    // Renders a SYMBOL's parameter list into MethodInfo's (type, name) tuples.
    //
    // Parameter NAMES read from a symbol arrive UNescaped — unlike the syntax path below, whose
    // ParameterSyntax.Identifier.Text preserves the converter's own `@`. A Go parameter named with
    // a C# reserved keyword (sync.Map's `CompareAndSwap(key, old, new any)`, which the converter
    // emits as `any @new`) must therefore be escaped HERE: every consumer of these tuples renders
    // the name straight into a declaration or a call argument, where a bare `new` is a parse error
    // that surfaces as CS0501 on the enclosing member. EscapeCsKeyword is a no-op for every
    // non-keyword and for an already-escaped name, so this is safe for all callers.
    //
    // `withRefKind` prefixes the rendered type with the parameter's `in`/`ref`/`out` modifier: an
    // explicit interface implementation must reproduce it or it matches no member (CS0539).
    public static (string type, string name)[] ToParameterInfos(this ImmutableArray<IParameterSymbol> parameters, bool withRefKind = false)
    {
        return parameters
            .Select(parameter => (
                type: $"{(withRefKind ? RefKindPrefix(parameter.RefKind) : "")}{GlobalQualify(parameter.Type.ToDisplayString())}",
                name: EscapeCsKeyword(parameter.Name)))
            .ToArray();
    }

    // True when a type survives the object round-trip a reflective invoker performs. A ref-struct
    // cannot be boxed at all (the Go variadic tail `params ꓸꓸꓸT` resolves to `System.Span<T>`), and
    // a pointer type has no boxed form the invoker can carry back.
    internal static bool IsInvokerForwardableType(ITypeSymbol? type)
    {
        return type switch
        {
            null => false,
            IPointerTypeSymbol => false,
            IFunctionPointerTypeSymbol => false,
            _ => !type.IsRefLikeType
        };
    }

    public static MethodInfo GetMethodInfo(this MethodDeclarationSyntax methodDeclaration, Compilation compilation)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

        string[] typeParameters = methodDeclaration.TypeParameterList?.Parameters
            .Select(param => param.Identifier.Text)
            .ToArray() ?? [];

        Dictionary<string, string[]> typeConstraints = [];

        // Initialize dictionary with empty constraint arrays for each type parameter
        foreach (string typeParam in typeParameters)
            typeConstraints[typeParam] = [];

        // Process constraints if they exist
        if (methodDeclaration.ConstraintClauses.Any())
        {
            foreach (TypeParameterConstraintClauseSyntax constraintClause in methodDeclaration.ConstraintClauses)
            {
                string typeParamName = constraintClause.Name.Identifier.Text;

                if (!typeConstraints.ContainsKey(typeParamName))
                    continue;

                string[] constraints = constraintClause.Constraints
                    .Select(constraint => GetConstraintText(constraint, semanticModel))
                    .Where(text => !string.IsNullOrEmpty(text))
                    .ToArray();

                typeConstraints[typeParamName] = constraints;
            }
        }

        bool signatureRenderable = methodDeclaration.ParameterList.Parameters.All(param =>
            !param.Modifiers.Any(SyntaxKind.RefKeyword) &&
            !param.Modifiers.Any(SyntaxKind.OutKeyword) &&
            !param.Modifiers.Any(SyntaxKind.InKeyword));

        bool invokerForwardable = signatureRenderable &&
            typeParameters.Length == 0 &&
            IsInvokerForwardableType(semanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type) &&
            methodDeclaration.ParameterList.Parameters.All(param =>
                param.Type is null || IsInvokerForwardableType(semanticModel.GetTypeInfo(param.Type).Type));

        return new MethodInfo()
        {
            Name = methodDeclaration.Identifier.Text,
            ReturnType = methodDeclaration.GetReturnType(semanticModel),
            ReturnTypeIsPublic = IsEffectivelyPublicType(semanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type),
            // Receiver excluded (the `this`-modified parameter): its type is the embed itself,
            // internal by design for an unexported embed — see the symbol-based harvest below.
            ParametersArePublic = methodDeclaration.ParameterList.Parameters
                .Where(param => !param.Modifiers.Any(SyntaxKind.ThisKeyword))
                .All(param => param.Type is null || IsEffectivelyPublicType(semanticModel.GetTypeInfo(param.Type).Type)),
            IsInvokerForwardable = invokerForwardable,
            IsSignatureRenderable = signatureRenderable,
            GenericTypes = string.Join(", ", typeParameters),
            TypeConstraints = typeConstraints,

            Parameters = methodDeclaration.ParameterList.Parameters.Select(param =>
            {
                if (param.Type is null)
                    return (type: "object", name: param.Identifier.Text);

                TypeInfo typeInfo = semanticModel.GetTypeInfo(param.Type);
                ITypeSymbol? typeSymbol = typeInfo.Type;
                string fullyQualifiedTypeName = GlobalQualify(typeSymbol?.ToDisplayString() ?? "object");

                // Preserve a `params` (variadic) modifier: the converter emits a Go variadic method
                // as `add(this ref Builder b, params ꓸꓸꓸbyte bytesʗp)`, but the resolved type is the
                // bare `Span<byte>` — dropping `params` makes the generated ж<Builder> overload reject
                // a call passing individual elements (`c.add(0xff)` → CS1929, falling back to the
                // ref-receiver value method). The Go variadic is always the LAST, non-receiver
                // parameter, so `params` never lands on the `this ж<T>` receiver.
                if (param.Modifiers.Any(SyntaxKind.ParamsKeyword))
                    fullyQualifiedTypeName = $"params {fullyQualifiedTypeName}";

                // Caller-info ATTRIBUTES and a parameter DEFAULT are dropped by the (type, name)
                // shape unless carried here, and both are load-bearing on a promoted forwarder for
                // the same reason `params` is above. A dropped default turns an N-argument call
                // into an (N+1)-argument one, so every promoted call site stops binding -- CS1929
                // raised in the CONSUMER, pointing away from the embed that caused it. A dropped
                // [CallerMemberName] is worse because it is SILENT: the forwarder still compiles
                // and still carries a name parameter, but it can no longer capture its own caller,
                // so the value quietly falls back to the default with no diagnostic anywhere.
                foreach (AttributeListSyntax attributeList in param.AttributeLists)
                {
                    foreach (AttributeSyntax attribute in attributeList.Attributes)
                    {
                        string? attributeName = semanticModel.GetSymbolInfo(attribute).Symbol?
                            .ContainingType?.ToDisplayString();

                        if (attributeName is "System.Runtime.CompilerServices.CallerMemberNameAttribute" or
                                             "System.Runtime.CompilerServices.CallerFilePathAttribute" or
                                             "System.Runtime.CompilerServices.CallerLineNumberAttribute" or
                                             "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute")
                        {
                            fullyQualifiedTypeName = $"[global::{attributeName}] {fullyQualifiedTypeName}";
                        }
                    }
                }

                string parameterName = param.Identifier.Text;

                if (param.Default is not null)
                    parameterName = $"{parameterName} = {param.Default.Value}";

                return (type: fullyQualifiedTypeName, name: parameterName);
            }).ToArray(),

            IsRefRecv = methodDeclaration.ParameterList.Parameters.Any(param =>
                param.Modifiers.Any(SyntaxKind.ThisKeyword) &&
                param.Modifiers.Any(SyntaxKind.RefKeyword))
        };
    }

    private static string GetConstraintText(TypeParameterConstraintSyntax constraint, SemanticModel semanticModel)
    {
        return constraint switch
        {
            ClassOrStructConstraintSyntax classOrStruct => classOrStruct.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) ? "class" : "struct",
            ConstructorConstraintSyntax => "new()",
            DefaultConstraintSyntax => "default",
            TypeConstraintSyntax typeConstraint =>
                semanticModel.GetTypeInfo(typeConstraint.Type).Type?.ToDisplayString() ?? typeConstraint.Type.ToString(),
            _ => string.Empty
        };
    }

    private static string GetReturnType(this MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
    {
        TypeSyntax returnSyntax = methodDeclaration.ReturnType;
        string refPrefix = "";

        // A `ref T` return (a B′-S0 ref-return primary) wraps its type in RefTypeSyntax, which is
        // not itself a type expression — GetTypeInfo answers null on it and the fallback said
        // "object", silently mis-typing the generated twin. Unwrap and carry the modifier; the
        // ReceiverMethodTemplate keys its own-box-returning twin variant on this prefix.
        if (returnSyntax is RefTypeSyntax refReturn)
        {
            refPrefix = "ref ";
            returnSyntax = refReturn.Type;
        }

        TypeInfo typeInfo = semanticModel.GetTypeInfo(returnSyntax);
        ITypeSymbol? typeSymbol = typeInfo.Type;

        return refPrefix + GlobalQualify(typeSymbol?.ToDisplayString() ?? "object");
    }

    public static MethodInfo GetMethodInfo(this IMethodSymbol methodSymbol)
    {
        // Convert parameters to the required tuple format
        (string type, string name)[] parameters = methodSymbol.Parameters.ToParameterInfos();

        // Extract generic type parameters
        string genericTypes = string.Join(", ", methodSymbol.TypeParameters.Select(typeParameter => typeParameter.Name));

        // Extract type constraints for generic parameters
        Dictionary<string, string[]> typeConstraints = new();

        foreach (ITypeParameterSymbol? typeParam in methodSymbol.TypeParameters)
        {
            List<string> constraints = [];

            // Add class/struct constraint
            if (typeParam.HasReferenceTypeConstraint)
                constraints.Add("class");
            else if (typeParam.HasValueTypeConstraint)
                constraints.Add("struct");

            // Add notnull constraint
            if (typeParam.HasNotNullConstraint)
                constraints.Add("notnull");

            // Add interface and type constraints
            constraints.AddRange(typeParam.ConstraintTypes.Select(constraintType => constraintType.ToDisplayString()));

            // Add unmanaged constraint
            if (typeParam.HasUnmanagedTypeConstraint)
                constraints.Add("unmanaged");

            // Add constructor constraint
            if (typeParam.HasConstructorConstraint)
                constraints.Add("new()");

            typeConstraints[typeParam.Name] = constraints.ToArray();
        }

        bool signatureRenderable = methodSymbol.Parameters.All(parameter => parameter.RefKind == RefKind.None);

        bool invokerForwardable = signatureRenderable &&
            methodSymbol.TypeParameters.Length == 0 &&
            !methodSymbol.ReturnsByRef &&
            !methodSymbol.ReturnsByRefReadonly &&
            IsInvokerForwardableType(methodSymbol.ReturnType) &&
            methodSymbol.Parameters.All(parameter => IsInvokerForwardableType(parameter.Type));

        return new MethodInfo
        {
            Name = methodSymbol.Name,
            ReturnType = GlobalQualify(methodSymbol.ReturnType.ToDisplayString()),
            ReturnTypeIsPublic = IsEffectivelyPublicType(methodSymbol.ReturnType),
            // The parameter-side twin of ReturnTypeIsPublic (W3a's promoted-member-forwarding site,
            // docs/phase4/DESIGN-w3a-wrapper-scaffolding.md's own arc): StructTypeTemplate's
            // promoted-method forwarder downgrade only ever consulted the RETURN type, so a void
            // method whose PARAMETER touches an unexported production type (runtime's white-box
            // `AddrRanges` embeds `addrRanges`, whose promoted `cloneInto(*addrRanges)` takes an
            // `*addrRanges` argument) stayed unconditionally public regardless — CS0051. The RECEIVER
            // slot must be skipped for the extension-shaped symbols Go methods compile to (`this ref
            // conn c` / `this ж<T>`): the receiver's type is the EMBED, which for an unexported embed
            // is internal BY DESIGN — counting it demoted every promotion out of an unexported embed
            // regardless of its real signature (net's public (*TCPConn).Read/Write went internal and
            // every cross-assembly consumer lost them — CS1929). Only instance-member callers
            // (interface adapters) have no receiver slot here.
            ParametersArePublic = (methodSymbol.IsExtensionMethod ? methodSymbol.Parameters.Skip(1) : methodSymbol.Parameters)
                .All(parameter => IsEffectivelyPublicType(parameter.Type)),
            IsInvokerForwardable = invokerForwardable,
            IsSignatureRenderable = signatureRenderable,
            Parameters = parameters,
            GenericTypes = genericTypes,
            TypeConstraints = typeConstraints,
            IsRefRecv = methodSymbol.ReturnsByRef
        };
    }
}
