// StructDeclarationSyntaxExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs;

public static class StructDeclarationSyntaxExtensions
{
    public static List<(string typeName, string fieldName, bool isReferenceType)> GetStructFields(
        this StructDeclarationSyntax structDeclaration, 
        GeneratorExecutionContext context)
    {
        // Obtain the SemanticModel from the context
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);

        List<(string typeName, string fieldName, bool isReferenceType)> fields = [];

        foreach (FieldDeclarationSyntax? fieldDeclaration in structDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            if (fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            TypeInfo typeInfo = semanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type);
            ITypeSymbol? typeSymbol = typeInfo.Type;
            string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

            // Determine if the type is a reference type or an unconstrained generic type
            bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

            foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                fields.Add((fullyQualifiedTypeName, variable.Identifier.Text, isReferenceType));
        }

        return fields;
    }

    public static List<(string typeName, string propertyName, bool isReferenceType)> GetStructProperties(
        this StructDeclarationSyntax structDeclaration, 
        GeneratorExecutionContext context)
    {
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);
        List<(string typeName, string propertyName, bool isReferenceType)> properties = [];

        foreach (PropertyDeclarationSyntax? propertyDeclaration in structDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            TypeSyntax propertyType = propertyDeclaration.Type is RefTypeSyntax refType ? refType.Type : propertyDeclaration.Type;
            TypeInfo typeInfo = semanticModel.GetTypeInfo(propertyType);
            ITypeSymbol? typeSymbol = typeInfo.Type;
            string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

            // Determine if the type is a reference type or an unconstrained generic type
            bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

            properties.Add((fullyQualifiedTypeName, propertyDeclaration.Identifier.Text, isReferenceType));
        }

        return properties;
    }

    // Gets fields and properties of a struct, maintaining the order in which they are defined
    public static List<(string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic)> GetStructMembers(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation,
        bool filterToRefProperties = false)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(structDeclaration.SyntaxTree);
        List<(string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic)> members = [];

        foreach (MemberDeclarationSyntax member in structDeclaration.Members)
        {
            if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            switch (member)
            {
                case PropertyDeclarationSyntax propertyDeclaration:
                    {
                        if (filterToRefProperties && propertyDeclaration.Type.Kind() != SyntaxKind.RefType)
                            continue;

                        TypeSyntax propertyType = propertyDeclaration.Type is RefTypeSyntax refType ? refType.Type : propertyDeclaration.Type;
                        TypeInfo typeInfo = semanticModel.GetTypeInfo(propertyType);
                        ITypeSymbol? typeSymbol = typeInfo.Type;
                        string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

                        // Determine if the type is a reference type or an unconstrained generic type
                        bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

                        members.Add((fullyQualifiedTypeName, propertyDeclaration.Identifier.Text, isReferenceType, true, IsMemberTypePublic(typeSymbol)));

                        break;
                    }
                case FieldDeclarationSyntax fieldDeclaration:
                    {
                        TypeInfo typeInfo = semanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type);
                        ITypeSymbol? typeSymbol = typeInfo.Type;
                        string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

                        // Determine if the type is a reference type or an unconstrained generic type
                        bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);
                        bool isPublic = IsMemberTypePublic(typeSymbol);

                        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                            members.Add((fullyQualifiedTypeName, variable.Identifier.Text, isReferenceType, false, isPublic));

                        break;
                    }
            }
        }

        return members;
    }

    // Reports whether a FORWARDED member's own type is effectively public — the same question
    // ImplementGenerator.AdapterSidePublic and ImplicitConvGenerator ask of a type they are about to
    // expose through a generated public member (Common.EffectiveScopeIsPublic), asked here of a
    // struct member a wrapper type forwards. A wrapper's forwarded field/property accessor
    // (InheritedTypeTemplate.ForwardedMembers) must not be emitted `public` over an internal
    // PRODUCTION type (CS0050/CS0053) — e.g. runtime's white-box `MSpan` bridge (`type MSpan =
    // mspan`, exported so tests can use it) forwarding `mspan`'s unexported `gcBits`/`mutex`/
    // `special`/`addrRange` fields, whose OWN types stay internal even though the bridge struct
    // around them is deliberately public.
    //
    // EffectiveScopeIsPublic alone answers only for the type SYMBOL handed to it, which is wrong for
    // a CONSTRUCTED GENERIC: `ж<mspan>`'s own DeclaredAccessibility is golib's `ж<T>` definition
    // (always public) — it says nothing about the TYPE ARGUMENT `mspan` substituted in, which stays
    // internal. A public accessor returning `ж<mspan>` is still CS0053 for exactly that argument, so
    // every type argument is checked too (recursively — a generic can nest, `slice<ж<mspan>>`), the
    // same peeling `typeReferencesUnexportedProductionNamed` (typeAccessibilityOperations.go) does on
    // the converter's own, parallel Go-side version of this question.
    public static bool IsMemberTypePublic(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return false;

        if (!Common.EffectiveScopeIsPublic(typeSymbol, Common.GetSimpleName(typeSymbol.Name, dropCollisionPrefix: true)))
            return false;

        if (typeSymbol is INamedTypeSymbol { IsGenericType: true } named)
        {
            foreach (ITypeSymbol typeArg in named.TypeArguments)
            {
                if (!IsMemberTypePublic(typeArg))
                    return false;
            }
        }

        if (typeSymbol is IArrayTypeSymbol arrayType)
            return IsMemberTypePublic(arrayType.ElementType);

        return true;
    }

    /// <summary>
    /// Gets the names of the struct's instance members (the same set <see cref="GetStructMembers"/>
    /// enumerates with <c>filterToRefProperties</c>) whose type cannot appear in a C# <c>==</c>
    /// comparison — the members the generated <c>Equals</c> must compare via golib's
    /// <c>AreEqual</c> instead. Consulted only for a GENERIC struct
    /// that failed the whole-struct <c>IEqualityOperators</c> gate: the gate disqualified the ENTIRE
    /// struct when ANY type parameter lacked the constraint, emitting a constant-<c>false</c> Equals
    /// that broke every field-independent comparison (unique's <c>Handle&lt;T&gt;</c> holds only a
    /// <c>ж&lt;T&gt;</c>, whose pointer-identity <c>==</c> is valid for every T — yet no two handles
    /// ever compared equal, contradicting the type's documented contract).
    /// </summary>
    public static HashSet<string> GetEqualityFallbackMembers(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(structDeclaration.SyntaxTree);
        INamedTypeSymbol? equalityOperators = compilation.GetTypeByMetadataName("System.Numerics.IEqualityOperators`3");
        HashSet<string> fallbackMembers = new(StringComparer.Ordinal);

        foreach (MemberDeclarationSyntax member in structDeclaration.Members)
        {
            if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            switch (member)
            {
                case PropertyDeclarationSyntax propertyDeclaration:
                {
                    // Same membership as GetStructMembers(filterToRefProperties: true): only the
                    // promoted-embed `partial ref` properties participate in the comparison.
                    if (propertyDeclaration.Type is not RefTypeSyntax refType)
                        continue;

                    if (!SupportsEqualityOperator(semanticModel.GetTypeInfo(refType.Type).Type, equalityOperators))
                        fallbackMembers.Add(propertyDeclaration.Identifier.Text);

                    break;
                }
                case FieldDeclarationSyntax fieldDeclaration:
                {
                    if (SupportsEqualityOperator(semanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type, equalityOperators))
                        continue;

                    foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                        fallbackMembers.Add(variable.Identifier.Text);

                    break;
                }
            }
        }

        return fallbackMembers;
    }

    /// <summary>
    /// Gets the names of the struct's instance members whose type is a Go INTERFACE — <c>any</c>
    /// included — for which C#'s <c>==</c> compiles but means the WRONG THING, so the generated
    /// <c>Equals</c> must compare them through golib's <c>AreEqual</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the one member kind where "== compiles" and "== is Go's relation" come apart, and the
    /// sibling <see cref="GetEqualityFallbackMembers"/> deliberately answers the first question only.
    /// C# <c>==</c> on an interface or <c>object</c> operand is REFERENCE identity; Go compares two
    /// interface values by DYNAMIC TYPE and dynamic value, which is what <c>builtin.AreEqual</c>
    /// implements and what the converter already emits for a bare Go <c>==</c> between interface
    /// operands. A struct with an interface-typed field therefore compared equal only to itself —
    /// and because a struct's <c>Equals</c> is also what a map LOOKUP calls, such a struct could
    /// never be found under a key it had itself been stored under.
    /// </para>
    /// <para>
    /// The measured consumer is encoding/json's cycle detector, whose slice arm keys
    /// <c>e.ptrSeen</c> on <c>struct{ ptr any; len int }</c>. The lookup never matched, no cycle was
    /// reported for a self-referential slice, and the encoder recursed until the process died. Its
    /// map arm survived only because that key is the pointer ITSELF rather than a struct around it.
    /// </para>
    /// <para>
    /// Interfaces only. A <c>ж&lt;T&gt;</c>, a named-pointer wrapper and a delegate are reference
    /// types too and each keeps <c>==</c>: pointer identity IS Go's pointer relation, and a Go struct
    /// holding a func is not comparable at all, so no valid Go program observes that member's answer.
    /// </para>
    /// </remarks>
    public static HashSet<string> GetInterfaceValueMembers(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(structDeclaration.SyntaxTree);
        HashSet<string> interfaceMembers = new(StringComparer.Ordinal);

        foreach (MemberDeclarationSyntax member in structDeclaration.Members)
        {
            if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            switch (member)
            {
                case PropertyDeclarationSyntax propertyDeclaration:
                {
                    // Same membership as GetStructMembers(filterToRefProperties: true): only the
                    // promoted-embed `partial ref` properties participate in the comparison.
                    if (propertyDeclaration.Type is RefTypeSyntax refType &&
                        IsGoInterfaceValue(semanticModel.GetTypeInfo(refType.Type).Type))
                    {
                        interfaceMembers.Add(propertyDeclaration.Identifier.Text);
                    }

                    break;
                }
                case FieldDeclarationSyntax fieldDeclaration:
                {
                    if (!IsGoInterfaceValue(semanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type))
                        continue;

                    foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                        interfaceMembers.Add(variable.Identifier.Text);

                    break;
                }
            }
        }

        return interfaceMembers;
    }

    /// <summary>
    /// Gets the names of the struct's fields whose initializer is specifically a directional
    /// channel STAMP — <c>channel&lt;T&gt;.RecvOnly</c> or <c>channel&lt;T&gt;.SendOnly</c>, the
    /// converter's emission for a <c>&lt;-chan T</c>/<c>chan&lt;- T</c> struct field (e.g.
    /// <c>net/http</c>'s <c>Request.Cancel</c>). Unlike a VALUE initializer, this is TYPE cargo:
    /// Go's zero value for a directional channel field is the direction-stamped nil, not a bare
    /// unstamped one, and that holds for the field REGARDLESS of whether a caller's argument was
    /// omitted or explicitly nil — there is no Go-expressible way to store a differently-directioned
    /// nil into a <c>&lt;-chan T</c>-typed slot. <see cref="GenerateConstructor"/> consults this to
    /// decide when a field-wise constructor's assignment must be skipped for a nil argument (letting
    /// the field initializer that already ran stand), matching the fixed-array member's own
    /// zero-argument handling — never for a general "any field with an initializer" case, which
    /// would wrongly let a value initializer (a field defaulting to a nonzero constant) override an
    /// explicitly-passed zero.
    /// </summary>
    /// <remarks>
    /// Named-argument default values must be compile-time constants, so this cannot be expressed as
    /// a parameter default (`Cancel = channel&lt;EmptyStruct&gt;.RecvOnly` does not compile) — the
    /// fix lives in the constructor BODY, which is why this returns names for GenerateConstructor to
    /// consult rather than feeding AppendConstructorSignature.
    /// </remarks>
    public static HashSet<string> GetChanDirInitializerMembers(
        this StructDeclarationSyntax structDeclaration)
    {
        HashSet<string> chanDirMembers = new(StringComparer.Ordinal);

        foreach (MemberDeclarationSyntax member in structDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax { Declaration.Type: GenericNameSyntax { Identifier.Text: "channel" } } fieldDeclaration)
                continue;

            foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
            {
                if (variable.Initializer?.Value is MemberAccessExpressionSyntax { Name.Identifier.Text: "RecvOnly" or "SendOnly" })
                    chanDirMembers.Add(variable.Identifier.Text);
            }
        }

        return chanDirMembers;
    }

    /// <summary>
    /// The members the converter emitted READONLY under an explicit layout: the Go ZERO-SIZE fields of the
    /// zero-size-field layout arc (src/go2cs/zeroSizeFieldLayout.go). Only that arc emits `readonly` fields
    /// under <c>[StructLayout(LayoutKind.Explicit)]</c>, and the arc is bounded to unmanaged structs, so the
    /// constructor-initialized readonly embed boxes (managed) can never appear here.
    /// </summary>
    public static HashSet<string> GetReadOnlyExplicitLayoutMembers(
        this StructDeclarationSyntax structDeclaration)
    {
        HashSet<string> members = new(StringComparer.Ordinal);

        bool explicitLayout = structDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attribute => attribute.Name.ToString() is "StructLayout" or "StructLayoutAttribute" or "System.Runtime.InteropServices.StructLayout" &&
                              attribute.ArgumentList?.Arguments.FirstOrDefault()?.ToString().EndsWith("Explicit", StringComparison.Ordinal) == true);

        if (!explicitLayout)
            return members;

        foreach (MemberDeclarationSyntax member in structDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax fieldDeclaration || !fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
                continue;

            foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                members.Add(variable.Identifier.Text);
        }

        return members;
    }

    // A Go interface value: a C# interface, or `object` — which is how `any` is spelled.
    private static bool IsGoInterfaceValue(ITypeSymbol? type)
    {
        return type is not null && (type.TypeKind == TypeKind.Interface || type.SpecialType == SpecialType.System_Object);
    }

    // Reports whether `left == right` COMPILES for operands of this type — the per-member question
    // the fallback set is built from. Deliberately about compilability, not semantics: wherever ==
    // is legal it is emitted, matching what a NON-generic struct's memberwise compare has always
    // done for the same member type. The one member kind where compilability and SEMANTICS diverge
    // is handled separately — see GetInterfaceValueMembers.
    private static bool SupportsEqualityOperator(ITypeSymbol? type, INamedTypeSymbol? equalityOperators)
    {
        if (type is null)
            return false;

        // A type parameter supports == only through a constraint carrying the static abstract
        // operator (IEqualityOperators<T, T, bool>). ANY one qualifying constraint suffices for
        // C# operator resolution — unlike the whole-struct gate, which requires EVERY constraint
        // of every parameter to qualify. Checked before IsReferenceType: a class-constrained
        // parameter is a "reference type" whose reference-== is farther from Go semantics than
        // AreEqual's typed dispatch.
        if (type is ITypeParameterSymbol typeParameter)
        {
            return equalityOperators is not null &&
                   typeParameter.ConstraintTypes.Any(constraint => TypeDeclarationSyntaxExtensions.ImplementsInterface(constraint, equalityOperators));
        }

        // Reference types (classes incl. ж<T> and unsafe.Pointer, interfaces, arrays, delegates):
        // == always compiles — user-defined where declared, reference equality otherwise.
        if (type.IsReferenceType)
            return true;

        // Enums, native pointers and function pointers compare with built-in ==.
        if (type.TypeKind is TypeKind.Enum or TypeKind.Pointer or TypeKind.FunctionPointer)
            return true;

        if (type is not INamedTypeSymbol namedType)
            return false;

        // Built-in value types (bool, char, the numerics, nint/nuint, decimal, DateTime, …).
        if (namedType.SpecialType != SpecialType.None)
            return true;

        // A [GoType] struct ALWAYS receives a same-type operator == (StructTypeTemplate and
        // InheritedTypeTemplate both emit it unconditionally). The attribute — not the operator —
        // must be the test for a struct of THIS compilation: its generated operator does not exist
        // yet while this generator is running, so a member scan cannot see it.
        if (namedType.OriginalDefinition.GetAttributes().Any(attribute =>
                attribute.AttributeClass is { Name: "GoTypeAttribute", ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true } }) ||
            namedType.OriginalDefinition.IsConvertedGoTypeSymbol())
        {
            return true;
        }

        // Any other value type (golib's slice/map/array/@string/…, arriving as metadata) qualifies
        // only by actually declaring a same-type operator ==.
        return namedType.GetMembers("op_Equality").OfType<IMethodSymbol>().Any(op =>
            op.Parameters.Length == 2 &&
            SymbolEqualityComparer.Default.Equals(op.Parameters[0].Type.OriginalDefinition, namedType.OriginalDefinition) &&
            SymbolEqualityComparer.Default.Equals(op.Parameters[1].Type.OriginalDefinition, namedType.OriginalDefinition));
    }

    /// <summary>
    /// METADATA counterpart to <see cref="GetStructMembers"/>: the members of a struct whose SOURCE
    /// this compilation cannot see, read from its type SYMBOL. Same tuple shape, so a caller can use
    /// either resolution interchangeably.
    /// </summary>
    /// <remarks>
    /// A real MSBuild build hands a <c>&lt;ProjectReference&gt;</c> to the compiler as compiled
    /// metadata, so the syntax walk resolves NOTHING cross-assembly — which is why a defined type over
    /// a foreign struct (<c>type index Index</c> in a white-box <c>_test.go</c>, or a plain
    /// <c>type P otherpkg.Point</c>) forwarded no members and every field selection on it was CS1061.
    /// Membership mirrors <c>StructTypeTemplate.getMetadataStructFields</c>: instance FIELDS plus the
    /// REF-RETURNING properties a referenced assembly's generated wrapper exposes for its embedded and
    /// promoted members. Visibility is decided by <see cref="Compilation.IsSymbolAccessibleWithin"/>
    /// rather than a public-only test, which is exactly Go's own rule projected into C#: an exported
    /// field is public and always forwards, while an unexported one is internal and forwards only where
    /// C# can reach it — i.e. the friend (<c>InternalsVisibleTo</c>) test assembly, which is precisely
    /// the same-Go-package case where Go permits the selection.
    /// </remarks>
    public static List<(string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic)> GetForeignStructMembers(
        INamedTypeSymbol structType,
        Compilation compilation)
    {
        List<(string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic)> members = [];

        foreach (ISymbol member in structType.GetMembers())
        {
            if (member.IsStatic || member.IsImplicitlyDeclared || Common.GetSimpleName(member.Name) == "_")
                continue;

            if (!compilation.IsSymbolAccessibleWithin(member, compilation.Assembly))
                continue;

            ITypeSymbol? memberType = member switch
            {
                IFieldSymbol field => field.Type,
                // Only a REF-RETURNING, non-indexer property is a converted struct's member surface
                // (the embed accessor and its promoted field accessors); a by-value property is
                // generator scaffolding (`Value` on a wrapper) and must not be forwarded.
                IPropertySymbol { ReturnsByRef: true, IsIndexer: false } property => property.Type,
                _ => null
            };

            if (memberType is null)
                continue;

            members.Add((
                memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                // GetStructMembers's SOURCE-based sibling reads syntax Identifier.Text, which already
                // carries the converter's own `@`-escape when the Go field name is a C# keyword
                // (pageCache's `base` field declares as `@base`). A SYMBOL's .Name never does — escape
                // it here so a forwarded keyword-named field (runtime's white-box PageCache/AddrRange
                // wrappers over pageCache.base/addrRange.base) emits `@base`, not a bare `base` that
                // parses as the base-access keyword (CS1519/CS1001/CS1002 in the generated forwarder).
                Common.EscapeCsKeyword(member.Name),
                IsReferenceTypeOrUnconstrainedGeneric(memberType),
                member is IPropertySymbol,
                IsMemberTypePublic(memberType)));
        }

        return members;
    }

    // Determine if type is a reference type or unconstrained generic type parameter
    private static bool IsReferenceTypeOrUnconstrainedGeneric(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return true; // Default to true for safety if type is unknown

        // If it's already a reference type, return true
        if (typeSymbol.IsReferenceType)
            return true;

        // Check if it's a type parameter (generic) and has no constraints or only has reference type constraint
        return typeSymbol is ITypeParameterSymbol { HasValueTypeConstraint: false };
    }

    public static IEnumerable<MethodInfo> GetExtensionMethods(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation)
    {
        string structName = structDeclaration.Identifier.Text;

        // A GENERIC struct's receiver renders WITH its type parameters (`this ref nistCurve<Point>
        // curve`), which the bare identifier `nistCurve` never equals — so a generic struct's methods
        // matched NONE and a generic embed promoted no methods (crypto/elliptic's p256Curve embedding
        // nistCurve<Point>). Append the type-parameter list (matching the converter's `<T, …>` render)
        // so the receiver comparison in IsExtensionMethodForStruct succeeds.
        if (structDeclaration.TypeParameterList is { Parameters.Count: > 0 } typeParameterList)
            structName += $"<{string.Join(", ", typeParameterList.Parameters.Select(parameter => parameter.Identifier.Text))}>";

        // Get all extension method declarations in the compilation
        IEnumerable<MethodDeclarationSyntax> extensions = compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => 
                    method.Modifiers.Any(m =>  m.IsKind(SyntaxKind.StaticKeyword)) &&
                    method.ParameterList.Parameters.Count > 0))
            .Where(method => method.IsExtensionMethodForStruct(structName));

        return extensions.Select(method => method.GetMethodInfo(compilation));
    }

    /// <summary>
    /// Box-receiver counterpart to <see cref="GetExtensionMethods"/>: the struct's direct-ж primary
    /// methods (<c>static M(this ж&lt;T&gt; …)</c>), as full <see cref="MethodInfo"/>. Such a method
    /// promotes through a POINTER embed unchanged — the converter emits the embed hop
    /// <c>target.&lt;embed&gt;</c> as a <c>ж&lt;T&gt;</c>, so <c>target.&lt;embed&gt;.M()</c> binds the
    /// box receiver directly (no box hop). <see cref="IsExtensionMethodForStruct"/> matches only
    /// value-receiver forms, so these need a separate harvest — sha3's <c>cshakeState</c> embeds
    /// <c>*state</c>, whose <c>Write</c> is <c>this ж&lt;state&gt;</c>; without this it had no promoted
    /// forwarder (CS1929). Callers must gate to POINTER embeds: a VALUE embed's <c>target.&lt;embed&gt;</c>
    /// is a value that cannot bind a ж-receiver (that shape needs the box-hop form).
    /// </summary>
    public static IEnumerable<MethodInfo> GetBoxReceiverExtensionMethods(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation)
    {
        string boxType = $"{PointerPrefix}<{structDeclaration.Identifier.Text}>";

        return compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method =>
                    method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) &&
                    method.ParameterList.Parameters.Count > 0))
            .Where(method =>
            {
                ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();

                return firstParam is not null &&
                       firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)) &&
                       (firstParam.Type?.ToString() ?? "") == boxType;
            })
            .Select(method => method.GetMethodInfo(compilation));
    }

    /// <summary>
    /// Gets the embedded-POINTER hop properties on the struct — the `public partial ref
    /// ж&lt;X&gt; F { get; }` members the converter emits for a Go embedded pointer field
    /// (`type rtype struct { *abi.Type }`) — as (property name, embedded type name) pairs.
    /// Method promotion through such an embed is syntax-resolved at Go call sites (the converter
    /// emits the hop `t.F.Value.M()`), so an interface member with no direct struct method must
    /// forward through the hop the same way. The embedded type name lets the caller split the
    /// hop receiver per method: direct-ж primaries bind the box field itself (`this.F.M()`).
    /// </summary>
    public static List<(string Name, string TypeName)> GetEmbeddedPointerHopNames(this StructDeclarationSyntax structDeclaration)
    {
        List<(string, string)> hops = [];

        foreach (PropertyDeclarationSyntax property in structDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            TypeSyntax type = property.Type is RefTypeSyntax refType ? refType.Type : property.Type;
            string typeText = type.ToString();

            if (typeText.StartsWith("ж<") && typeText.EndsWith(">") && property.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                hops.Add((property.Identifier.Text, typeText.Substring(2, typeText.Length - 3)));
        }

        return hops;
    }

    /// <summary>
    /// Gets the (field name, embedded type name) pairs for embedded VALUE struct fields — the
    /// converter emits an embed as a <c>partial ref</c> property whose name matches its type's
    /// simple name (<c>public partial ref CommonType CommonType {{ get; }}</c>). The TypeGenerator
    /// heap-boxes the field and emits a public static ref accessor (<c>Ꮡ{Embed}</c>), so a
    /// pointer-interface adapter can project the receiver box onto the embedded field's box.
    /// </summary>
    public static List<(string Name, string TypeName)> GetEmbeddedValueHopNames(this StructDeclarationSyntax structDeclaration)
    {
        List<(string, string)> hops = [];

        foreach (PropertyDeclarationSyntax property in structDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            TypeSyntax type = property.Type is RefTypeSyntax refType ? refType.Type : property.Type;
            string typeText = type.ToString();

            if (typeText.StartsWith("ж<") || typeText.Contains('<') || !property.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                continue;

            string simpleTypeName = typeText;
            int lastDot = simpleTypeName.LastIndexOf('.');

            if (lastDot >= 0)
                simpleTypeName = simpleTypeName.Substring(lastDot + 1);

            if (simpleTypeName == property.Identifier.Text || ShadowVarMarker + simpleTypeName == property.Identifier.Text)
                hops.Add((property.Identifier.Text, typeText));
        }

        return hops;
    }

    private static bool IsExtensionMethodForStruct(this MethodDeclarationSyntax method, string structName)
    {
        ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();

        if (firstParam is null || !firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)))
            return false;

        string paramType = firstParam.Type?.ToString() ?? "";

        return paramType == structName ||
               paramType == $"ref {structName}" ||
               paramType == $"in {structName}" ||
               paramType == $"ref readonly {structName}";
    }

    /// <summary>
    /// Gets the names of extension methods whose receiver is the struct's box <c>ж&lt;T&gt;</c> —
    /// the direct-ж primary form (emitted by the converter when a method needs the real receiver
    /// box, e.g. it takes the address of a receiver field). These bind on the box itself, so a
    /// pointer-interface adapter forwards to <c>m_box.M(...)</c> directly.
    /// </summary>
    public static HashSet<string> GetBoxReceiverMethodNames(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation)
    {
        return GetBoxReceiverMethodNames(structDeclaration.Identifier.Text, compilation);
    }

    /// <summary>
    /// Type-name form of <see cref="GetBoxReceiverMethodNames(StructDeclarationSyntax, Compilation)"/>
    /// for types with no local declaration in hand — e.g. the TARGET of an embedded-pointer hop
    /// (os's `fileWithoutWriteTo` embeds `*File`; File's `Read` is a direct-ж primary, so the hop
    /// must bind `this.File.Read(p)`, not the deref'd value — CS1929). Only converter-emitted
    /// primaries are visible in this compilation's syntax trees (sibling-generator ж-twins are
    /// not), which is exactly the discrimination needed.
    /// </summary>
    public static HashSet<string> GetBoxReceiverMethodNames(string typeName, Compilation compilation)
    {
        string boxType = $"ж<{typeName}>";

        return new HashSet<string>(compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method =>
                    method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) &&
                    method.ParameterList.Parameters.Count > 0))
            .Where(method =>
            {
                ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();

                return firstParam is not null &&
                       firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)) &&
                       (firstParam.Type?.ToString() ?? "") == boxType;
            })
            .Select(method => method.Identifier.Text), StringComparer.Ordinal);
    }

    /// <summary>
    /// RETURN TYPES of the direct-ж primaries <see cref="GetBoxReceiverMethodNames(string, Compilation)"/>
    /// names, keyed by method name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The name-only form above is everything the forwarding RECEIVER needs, and it was everything
    /// anyone needed until a member's DECLARED result turned out to be an interface the forwarded Go
    /// method does not return (crypto/mlkem's projected constraint — see
    /// <c>AdapterImplTemplate.ForwardResultWraps</c>). Deciding that requires the forwarded method's
    /// return type, and a direct-ж primary is invisible to <c>GetExtensionMethods</c>, so there was
    /// nowhere to read it from.
    /// </para>
    /// <para>
    /// ⚠ The spelling is <c>GlobalQualify(ToDisplayString())</c> — the SAME pair of helpers
    /// <c>GetReturnType</c> applies to a <c>MethodInfo</c>'s return type and that the interface side
    /// composes with. That is deliberate and load-bearing: the caller compares the two strings, and a
    /// comparison between two spellings of one type is the defect class this generator has already
    /// paid for on its adapter names.
    /// </para>
    /// </remarks>
    public static Dictionary<string, string> GetBoxReceiverMethodReturnTypes(string typeName, Compilation compilation)
    {
        string boxType = $"ж<{typeName}>";
        Dictionary<string, string> returnTypes = new(StringComparer.Ordinal);

        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            SemanticModel? semanticModel = null;

            foreach (MethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword)) || method.ParameterList.Parameters.Count == 0)
                    continue;

                ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();

                if (firstParam is null || !firstParam.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ThisKeyword)) ||
                    (firstParam.Type?.ToString() ?? "") != boxType)
                    continue;

                semanticModel ??= compilation.GetSemanticModel(tree);

                ITypeSymbol? returnSymbol = semanticModel.GetTypeInfo(method.ReturnType).Type;

                if (returnSymbol is not null)
                    returnTypes[method.Identifier.Text] = GlobalQualify(returnSymbol.ToDisplayString());
            }
        }

        return returnTypes;
    }

    /// <summary>
    /// The SYMBOL-side twin of <see cref="GetBoxReceiverMethodReturnTypes"/>, for a struct this
    /// compilation does not declare.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ⚠⚠ WITHOUT THIS, A FOREIGN STRUCT HAS NO FORWARDED RETURN TYPES AT ALL and the projected-result
    /// wrap is never even considered: both of its sources are gated on the struct's DECLARATION —
    /// <c>GetExtensionMethods</c> takes a <c>StructDeclarationSyntax</c> and the syntax scan above
    /// takes its identifier — and a foreign struct has neither. The wrap loop takes its first
    /// <c>continue</c> on the lookup and the member forwards bare.
    /// </para>
    /// <para>
    /// ⚠ THAT, NOT THE WRAP MAP'S BOUND, IS WHAT crypto/mlkem's CS0266 ×2 ACTUALLY REACHED. R reported
    /// the map's assembly bound as the cause (mailbox <c>d6d2970a2</c> §2) from an elimination over
    /// the map's own filters, and the elimination was sound about the map and silent about whether the
    /// lookup ran at all — it did not. Measured, one compilation, both before and after that bound was
    /// lifted: the LOCAL projected pair wraps and the FOREIGN one stays bare, which no change to the
    /// map can explain. The bound still had to be lifted — a foreign pair the map does not hold cannot
    /// be named once the lookup does run — so the two changes are consecutive gates, not alternatives.
    /// </para>
    /// <para>
    /// Bounded to the struct's OWN package class, which is where the converter emits a Go
    /// pointer-receiver method's box extension, and matched on the box by the same
    /// <c>named.Name == PointerPrefix</c> test the member scan below already uses rather than a second
    /// spelling of it. The return type takes the same <c>GlobalQualify(ToDisplayString())</c> pair as
    /// its syntax twin, because the caller compares the two strings.
    /// </para>
    /// </remarks>
    public static Dictionary<string, string> GetForeignBoxReceiverMethodReturnTypes(ITypeSymbol structType)
    {
        Dictionary<string, string> returnTypes = new(StringComparer.Ordinal);

        if (structType.ContainingType is not INamedTypeSymbol packageClass)
            return returnTypes;

        foreach (IMethodSymbol method in packageClass.GetMembers().OfType<IMethodSymbol>())
        {
            if (!method.IsExtensionMethod || method.Parameters.Length == 0)
                continue;

            if (method.Parameters[0].Type is not INamedTypeSymbol { TypeArguments.Length: 1 } box || box.Name != PointerPrefix)
                continue;

            if (!SymbolEqualityComparer.Default.Equals(box.TypeArguments[0], structType))
                continue;

            returnTypes[method.Name] = GlobalQualify(method.ReturnType.ToDisplayString());
        }

        return returnTypes;
    }

    /// <summary>
    /// SIMPLE-NAME form of <see cref="GetBoxReceiverMethodNames(string, Compilation)"/>, for a struct
    /// whose declaration is METADATA-ONLY here yet which gains direct-ж primaries from THIS
    /// compilation — the `-tests` white-box friend bridge, whose internal <c>_test.go</c> methods on a
    /// referenced PRODUCTION receiver emit into the test assembly. The exact-text match cannot serve
    /// it: the bridge spells the box parameter through whatever qualification its own file needs
    /// (<c>this ж&lt;Replacer&gt;</c> via an imported alias, but <c>this
    /// ж&lt;global::go.sync_package.poolChain&gt;</c> once a <c>go/*</c> package in the closure
    /// shadows the root namespace), so the receiver is matched on the LAST dotted segment of the ж
    /// type argument instead. Narrow by construction — it is consulted only where the struct has no
    /// local declaration, so a same-simple-named local type cannot be the one found.
    /// </summary>
    public static HashSet<string> GetBoxReceiverMethodNamesBySimpleName(string simpleTypeName, Compilation compilation)
    {
        return new HashSet<string>(compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method =>
                    method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) &&
                    method.ParameterList.Parameters.Count > 0))
            .Where(method =>
            {
                ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();

                if (firstParam is null || !firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)))
                    return false;

                string paramType = firstParam.Type?.ToString() ?? "";

                if (!paramType.StartsWith($"{PointerPrefix}<", StringComparison.Ordinal) || !paramType.EndsWith(">", StringComparison.Ordinal))
                    return false;

                string argument = paramType.Substring(PointerPrefix.Length + 1, paramType.Length - PointerPrefix.Length - 2);

                return Common.GetSimpleName(argument) == simpleTypeName;
            })
            .Select(method => method.Identifier.Text), StringComparer.Ordinal);
    }

    /// <summary>
    /// Ref-receiver counterpart to <see cref="GetBoxReceiverMethodNamesBySimpleName"/>: gets the
    /// names of <c>[GoRecv]</c>-style ref extension methods (<c>static M(this ref T, …)</c>)
    /// declared anywhere in the CURRENT compilation whose receiver's simple type name matches
    /// <paramref name="simpleTypeName"/>.
    /// </summary>
    /// <remarks>
    /// Needed for the same friend-bridge shape as the box scan, in the other receiver form: an
    /// internal white-box test package declares a ref-receiver method for a PRODUCTION type
    /// (crypto/tls's <c>unmarshal(this ref SessionState, …)</c>), whose declaration discovery hands
    /// back no syntax because the struct itself lives in the referenced assembly. The method still
    /// binds on the receiver box through its RecvGenerator ж-twin — generator output this generator
    /// cannot observe, so the SOURCE form is the evidence, exactly the assumption
    /// <c>MethodInfo.IsRefRecv</c> already encodes for a locally-declared struct. Matching is by the
    /// receiver's SIMPLE name for the same reason the box scan's is: the bridge spells the type
    /// through whatever qualification its own file needs.
    /// </remarks>
    public static HashSet<string> GetRefReceiverMethodNamesBySimpleName(string simpleTypeName, Compilation compilation)
    {
        return new HashSet<string>(compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method =>
                    method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) &&
                    method.ParameterList.Parameters.Count > 0))
            .Where(method =>
            {
                ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();

                if (firstParam is null ||
                    !firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)) ||
                    !firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword)))
                    return false;

                string paramType = firstParam.Type?.ToString() ?? "";

                return paramType.Length > 0 && Common.GetSimpleName(paramType) == simpleTypeName;
            })
            .Select(method => method.Identifier.Text), StringComparer.Ordinal);
    }

    /// <summary>
    /// METADATA counterpart to <see cref="GetBoxReceiverMethodNames(string, Compilation)"/>: gets the
    /// names of PUBLIC direct-ж extension methods (<c>static M(this ж&lt;T&gt;)</c>) declared on a
    /// FOREIGN type's containing package class, visible only through compiled metadata — a syntax-tree
    /// scan of the current compilation cannot see them. Needed to forward an interface member promoted
    /// through a VALUE-embedded foreign field: database/sql's <c>driverConn</c> value-embeds
    /// <c>sync.Mutex</c>, whose <c>Lock</c>/<c>Unlock</c> are <c>this ж&lt;Mutex&gt;</c> extensions in
    /// the compiled sync assembly, so the box hop must bind <c>m_box.of(driverConn.ᏑMutex).Lock()</c>
    /// exactly as a local direct-ж primary would. Mirrors the foreignStruct arm's boxBound scan:
    /// only a PUBLIC ж-extension binds cross-assembly (unexported RecvGenerator twins are internal).
    /// </summary>
    public static HashSet<string> GetForeignBoxReceiverMethodNames(INamedTypeSymbol embedType)
    {
        HashSet<string> boxMethods = new(StringComparer.Ordinal);

        if (embedType.ContainingType is not INamedTypeSymbol packageClass)
            return boxMethods;

        foreach (IMethodSymbol method in packageClass.GetMembers().OfType<IMethodSymbol>())
        {
            if (!method.IsStatic ||
                method.DeclaredAccessibility != Accessibility.Public ||
                method.Parameters.Length == 0)
                continue;

            if (method.Parameters[0].Type is INamedTypeSymbol recvType &&
                recvType.Name == "ж" &&
                recvType.TypeArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(recvType.TypeArguments[0], embedType))
            {
                boxMethods.Add(method.Name);
            }
        }

        return boxMethods;
    }

    /// <summary>
    /// Gets the (field name, embedded type) pairs for a FOREIGN struct's VALUE-embedded struct fields,
    /// read from METADATA (the struct has no syntax declaration here). The converter emits an embed as a
    /// <c>public partial ref {Embed} {Embed}</c> property, so the member name equals its type's simple
    /// name — the same Go-embed convention the syntax-based <see cref="GetEmbeddedValueHopNames"/> matches.
    /// Used to forward an interface member the foreign struct PROMOTES through the embed (parse's
    /// <c>RangeNode</c> embeds <c>BranchNode</c>, whose <c>String</c> is promoted, not declared).
    /// </summary>
    public static List<(string Name, INamedTypeSymbol Type)> GetForeignValueEmbeds(INamedTypeSymbol structType)
    {
        List<(string, INamedTypeSymbol)> embeds = [];

        foreach (ISymbol member in structType.GetMembers())
        {
            if (member.IsStatic)
                continue;

            INamedTypeSymbol? memberType = member switch
            {
                IPropertySymbol property => property.Type as INamedTypeSymbol,
                IFieldSymbol field => field.Type as INamedTypeSymbol,
                _ => null
            };

            // Embed convention: member NAME equals its type's simple name; a NON-generic struct (a
            // generic member type would be a container field, not an embed).
            if (memberType is not null && !memberType.IsGenericType && member.Name == memberType.Name)
                embeds.Add((member.Name, memberType));
        }

        return embeds;
    }

    /// <summary>
    /// Gets the (member name, element type) pairs for a struct's embedded-POINTER fields read from its
    /// type SYMBOL — a Go embedded pointer field (<c>*bufio.Writer</c>) emits a <c>ж&lt;X&gt;</c> member
    /// named after X's simple name (<c>partial ref ж&lt;Writer&gt; Writer</c>), so the embed convention is
    /// the pointer sibling of <see cref="GetForeignValueEmbeds"/>. Works for FOREIGN structs (metadata
    /// members) and for resolving a LOCAL struct's FOREIGN hop element, where the syntax-based
    /// <see cref="GetEmbeddedPointerHopNames"/> knows only the member's type TEXT. A Δ-collision-renamed
    /// element keeps its markerless member name, mirroring the embedded-interface-field detection.
    /// </summary>
    public static List<(string Name, INamedTypeSymbol Type)> GetPointerEmbeds(ITypeSymbol structType)
    {
        List<(string, INamedTypeSymbol)> embeds = [];

        foreach (ISymbol member in structType.GetMembers())
        {
            if (member.IsStatic)
                continue;

            INamedTypeSymbol? memberType = member switch
            {
                IPropertySymbol property => property.Type as INamedTypeSymbol,
                IFieldSymbol field => field.Type as INamedTypeSymbol,
                _ => null
            };

            if (memberType is { TypeArguments.Length: 1 } named && named.Name == PointerPrefix &&
                named.TypeArguments[0] is INamedTypeSymbol elementType &&
                (member.Name == elementType.Name || ShadowVarMarker + member.Name == elementType.Name))
            {
                embeds.Add((member.Name, elementType));
            }
        }

        return embeds;
    }

    /// <summary>
    /// METADATA scan of a FOREIGN type's containing package class for its PUBLIC VALUE/REF-receiver
    /// extension methods (<c>static M(this {ref|in} T)</c>) — the sibling of
    /// <see cref="GetForeignBoxReceiverMethodNames"/> for the non-box receiver forms. Maps each method
    /// name to its receiver <see cref="RefKind"/> so a promoted-embed forward can spell the static call's
    /// receiver argument (<c>ref</c>/<c>in</c>/value). Only PUBLIC extensions bind cross-assembly.
    /// </summary>
    public static Dictionary<string, RefKind> GetForeignValueReceiverMethods(INamedTypeSymbol type)
    {
        Dictionary<string, RefKind> methods = new(StringComparer.Ordinal);

        if (type.ContainingType is not INamedTypeSymbol packageClass)
            return methods;

        foreach (IMethodSymbol method in packageClass.GetMembers().OfType<IMethodSymbol>())
        {
            if (!method.IsStatic ||
                method.DeclaredAccessibility != Accessibility.Public ||
                method.Parameters.Length == 0)
                continue;

            // The receiver is the type ITSELF (value/ref/in) — NOT the ж<T> box form, which
            // GetForeignBoxReceiverMethodNames covers and which binds on a box hop, not m_box.Value.<embed>.
            if (SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, type) &&
                !methods.ContainsKey(method.Name))
            {
                methods[method.Name] = method.Parameters[0].RefKind;
            }
        }

        return methods;
    }
}
