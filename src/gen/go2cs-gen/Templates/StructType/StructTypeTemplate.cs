// StructTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.StructType;

internal class StructTypeTemplate : TemplateBase
{
    // Template Parameters
    public required GeneratorExecutionContext Context;
    public required string StructName;
    public required string FullyQualifiedStructType;
    public required List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, bool isPublic)> StructMembers;
    public required bool HasEqualityOperators;
    // Non-null exactly when HasEqualityOperators is false: the members whose type cannot use ==
    // (see GetEqualityFallbackMembers). May be empty — a generic struct with no type-parameter-
    // dependent members compares every member with == despite failing the whole-struct gate.
    public HashSet<string>? EqualityFallbackMembers;
    // Members whose field initializer is a directional channel stamp (channel<T>.RecvOnly /
    // .SendOnly — see GetChanDirInitializerMembers). GenerateConstructor skips the field-wise
    // assignment for these when the argument is nil, so the field initializer that already ran
    // stands, exactly as the fixed-array member case preserves its own `= new(N)` initializer.
    public HashSet<string> ChanDirInitializerMembers = [];
    public string[] ValueCloneFields = [];

    private string? m_nonGenericStructName;
    public string NonGenericStructName => m_nonGenericStructName ??= GetSimpleName(StructName, true);

    private List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, bool isPublic)>? m_publicStructMembers;
    private List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, bool isPublic)> PublicStructMembers =>
        m_publicStructMembers ??= StructMembers.Where(item =>
        {
            string simpleName = GetSimpleName(item.memberName);

            // Blank/underscore fields (e.g. an embedded unexported marker like `_ noCopy`)
            // are not exported and must not drive a public constructor — their type can be
            // less accessible than the struct, which makes a public ctor invalid (CS0051).
            return !simpleName.StartsWith("_") && GetScope(simpleName) == "public";
        }).ToList();

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{Scope}} partial struct {{StructName}}{{ValueCloneBaseList}}
            {
                // Promoted Struct Fields
                {{PromotedStructDeclarations}}

                // Field References
                {{FieldReferences}}

                // Constructors
                {{Constructors}}
                {{ValueCloneImplementation}}
                // Handle comparisons between struct '{{NonGenericStructName}}' instances
                public bool Equals({{StructName}} other) =>
                    {{CompareFields}};
                
                public override bool Equals(object? obj) => obj is {{StructName}} other && Equals(other);
                
                public override int GetHashCode() => {{HashCode}};
                
                public static bool operator ==({{StructName}} left, {{StructName}} right) => left.Equals(right);
                
                public static bool operator !=({{StructName}} left, {{StructName}} right) => !(left == right);
        
                // Handle comparisons between 'nil' and struct '{{NonGenericStructName}}'
                public static bool operator ==({{StructName}} value, NilType nil) => value.Equals(default({{StructName}}));

                public static bool operator !=({{StructName}} value, NilType nil) => !(value == nil);

                public static bool operator ==(NilType nil, {{StructName}} value) => value == nil;

                public static bool operator !=(NilType nil, {{StructName}} value) => value != nil;

                public static implicit operator {{StructName}}(NilType nil) => default({{StructName}});

                public override string ToString() => string.Concat("{", string.Join(" ",
                [
                    {{(StructMembers.Count > 0 ? string.Join(",\r\n            ", StructMembers.Select(GetToStringImplementation)) : "\"\"")}}
                ]), "}");
            }{{PromotedStructReceivers()}}
        """;

    // A Go struct carrying FIXED-SIZE ARRAY fields is not completely copied by a plain C# struct
    // assignment: `array<T>` (and the generated named-array wrapper) is a struct over a shared T[]
    // backing, so the copy's array writes reach back into the source — crypto/sha256's `Sum` copies
    // the digest (`d0 := *d`) precisely so it can finalize the copy while the caller keeps writing
    // the original, and the aliased state made every second Sum wrong. The CONVERTER decides which
    // fields need the deep copy (it alone has the Go type information) and names them in
    // [GoValueClone(…)]; this emits the matching Clone(), and every Go by-value copy site calls it.
    // An EMBEDDED member needs no listing for its own sake — it is an INLINE field (see
    // PromotedStructDeclarations), so the C# struct copy already copies it, exactly as Go does. It is
    // listed only when the embedded TYPE itself carries a fixed array, and then the ordinary
    // `copy.<member> = <member>.ΔClone()` line is correct: the assignment lands in the copy's own
    // inline storage, never in the source's.
    private string ValueCloneBaseList => ValueCloneFields.Length > 0 ? " : IGoValueClone" : "";

    private string ValueCloneImplementation
    {
        get
        {
            if (ValueCloneFields.Length == 0)
                return "";

            StringBuilder result = new();

            result.Append($"\r\n{TypeElemIndent}// Go by-value copy of struct '{NonGenericStructName}'");
            // EscapeCsTypeName: a struct named after a CONTEXTUAL keyword makes
            // `public record ΔClone()` parse as a positional record declaration.
            string declaredName = EscapeCsTypeName(StructName);

            result.Append($"\r\n{TypeElemIndent}public {declaredName} {ValueCloneMethod}()");
            result.Append($"\r\n{TypeElemIndent}{{");
            result.Append($"\r\n{TypeElemIndent}    {declaredName} copy = this;");

            // One member name serves EVERY clone-needing field type: a nested struct declares
            // ValueCloneMethod itself, and the array kinds (golib's array<T>, the named-array and
            // array-view wrappers) alias it to their own public Clone(). The converter's stamp is
            // the authority on WHICH fields need it.
            foreach (string fieldName in ValueCloneFields)
                result.Append($"\r\n{TypeElemIndent}    copy.{fieldName} = {fieldName}.{ValueCloneMethod}();");

            result.Append($"\r\n{TypeElemIndent}    return copy;");
            result.Append($"\r\n{TypeElemIndent}}}");
            result.Append($"\r\n\r\n{TypeElemIndent}object ICloneable.Clone() => {ValueCloneMethod}();");
            result.Append("\r\n");

            return result.ToString();
        }
    }

    private string PromotedStructDeclarations
    {
        get
        {
            (string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, bool isPublic)[] promotedStructs = StructMembers.Where(item => item.isPromotedStruct).ToArray();

            if (promotedStructs.Length == 0)
                return $"// -- {NonGenericStructName} has no promoted structs";

            StringBuilder result = new();

            // An embed is an INLINE FIELD, never a box. Go copies an embedded struct with the value
            // that carries it — it is a field like any other — so a heap ж<T> box gave the embed
            // REFERENCE semantics that a C# struct assignment then shared: `copy = value` handed both
            // sides one storage and every write through the copy reached the source. That is the
            // defect that made go/types judge a type parameter not identical to itself — subst's
            // `copy := *v` mutated the ORIGIN Var's field type in place, so the second instantiation
            // of a generic type substituted over an already-substituted underlying (see the
            // EmbeddedStructValueCopy behavioral test, and ConversionStrategies-Reference under
            // *An embedded struct is an INLINE field, so a value copy copies it*).
            //
            // The composed field name must use the unescaped member name — a C#-keyword embed
            // (`type base struct{…}`) arrives escaped `@base`, but `ʗ@base` is invalid ('@' only
            // leads an identifier). The standalone member ACCESS sites keep `@base`.
            foreach ((string typeName, string memberName, _, _, _) in promotedStructs)
            {
                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                result.Append($"private {typeName} {CapturedVarMarker}{GetUnsanitizedIdentifier(memberName)};");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Accessors");

            // [UnscopedRef] is what makes the ref accessor legal at all: a struct member returning a
            // ref to its own instance state is CS8170 by default, because the receiver could be a
            // temporary. The attribute states the ref's lifetime is the RECEIVER's — exactly the
            // guarantee Go gives, since the selection IS the enclosing value's storage — and moves the
            // burden to the call site, where C#'s ref-safety rules then reject precisely the cases Go
            // also rejects (taking the address of a non-variable). Same technique the
            // InheritedTypeTemplate uses to forward a defined-type-over-struct's fields.
            //
            // The accessor is a REF, not get/set, for the same reason it always was: in Go the
            // selection is a variable — addressable, and usable as the receiver of a value-receiver
            // method the converter emits `this ref`. A POINTER embed's slot holds a possibly-null
            // ж<T>; reading or ASSIGNING it never dereferences (internal/godebug's
            // `s.setting = lookup(…)` post-construction shape), and a genuine deref still panics
            // downstream on the held ж<T>'s own Value — the promoted field/method accessors descend
            // `<embed>.Value.<member>`.
            // The accessor's scope follows the MEMBER, not the type it is written over. The two are
            // the same string for an ordinary embed — which is why reading it off the type served
            // for so long — but they part company whenever the converter RENAMES the type: a
            // function-local `type MyInt int` is hoisted to `<Func>_MyInt`, whose leading case
            // belongs to the enclosing function, not to the field. The declaration this implements
            // now carries the Go FIELD name and its exportedness (visitStructType), so reading the
            // type could hand back the opposite modifier and C# rejects the pair outright (CS8799 —
            // encoding/json's TestAnonymousFields). Every sibling accessor below already scopes by
            // member name; this one was the outlier.
            foreach ((string typeName, string memberName, _, _, _) in promotedStructs)
            {
                string memberScope = GetScope(GetSimpleName(memberName));
                result.Append($"\r\n{TypeElemIndent}[global::System.Diagnostics.CodeAnalysis.UnscopedRef] {memberScope} partial ref {typeName} {memberName} => ref {CapturedVarMarker}{GetUnsanitizedIdentifier(memberName)};");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Field Accessors");

            // Go's shadowing rule: an OWN (declared) field with the same name shadows the embedded
            // member, so promotion must skip it — reflect's makeFuncImpl declares `fn` while
            // embedding makeFuncCtxt, whose `fn` would otherwise emit a duplicate accessor
            // (CS0102) and a duplicate Ꮡfn reference (CS0111).
            HashSet<string> declaredMemberNames = new(StructMembers.Select(m => GetSimpleName(m.memberName)));

            // Go's AMBIGUITY rule is DEPTH-AWARE: a member name is promoted iff there is a
            // UNIQUE occurrence at the SHALLOWEST embedding depth. Same-depth duplicates are
            // ambiguous and drop (bufio's ReadWriter embeds *Reader AND *Writer, both carrying
            // err/buf — CS0102/CS0111), but a shallower occurrence WINS over deeper ones —
            // macho's FatArch embeds FatArchHeader (Cpu/SubCpu at depth 1) AND *File (whose
            // flattened surface carries FileHeader's Cpu/SubCpu at depth 2): the depth-blind
            // count dropped BOTH, orphaning the converter's bare `fa.Cpu` (CS1061 x4).
            Dictionary<string, int> promotedFieldMinDepth = new(StringComparer.Ordinal);
            Dictionary<string, int> promotedFieldMinDepthCount = new(StringComparer.Ordinal);

            foreach ((string promotedStructType, _, _, _, _) in promotedStructs)
            {
                foreach ((_, string memberName, int depth) in getStructMembers(promotedStructType))
                {
                    string simpleName = GetSimpleName(memberName);

                    if (!promotedFieldMinDepth.TryGetValue(simpleName, out int minDepth) || depth < minDepth)
                    {
                        promotedFieldMinDepth[simpleName] = depth;
                        promotedFieldMinDepthCount[simpleName] = 1;
                    }
                    else if (depth == minDepth)
                    {
                        promotedFieldMinDepthCount[simpleName]++;
                    }
                }
            }

            bool promotes(string simpleName, int depth) =>
                promotedFieldMinDepth[simpleName] == depth && promotedFieldMinDepthCount[simpleName] == 1;

            foreach ((string promotedStructType, string promotedMemberName, _, _, _) in promotedStructs)
            {
                // Rewrite the embed's type PARAMETERS to this instantiation's type ARGUMENTS on the
                // promoted member TYPE (a generic embed's field carries `Point`, out of scope here).
                Dictionary<string, string> typeArgMap = GetEmbedTypeArgumentMap(promotedStructType);

                foreach ((string typeName, string memberName, int depth) in getStructMembers(promotedStructType))
                {
                    // A blank `_` field (padding / embedded unexported marker) is never promoted or
                    // selectable in Go; emitting an accessor for it collides with the enclosing
                    // struct's own `_` field (CS0102).
                    if (GetSimpleName(memberName) == "_")
                        continue;

                    if (declaredMemberNames.Contains(GetSimpleName(memberName)))
                        continue;

                    if (!promotes(GetSimpleName(memberName), depth))
                        continue;

                    // Scope derives from the MEMBER name (its exportedness), matching the box-field
                    // accessors: a lowercase field TYPE (uintptr Size_) made an EXPORTED promoted
                    // member internal - invisible cross-assembly (reflect via abi, CS1061 x22).
                    string typeScope = GetScope(GetSimpleName(memberName));

                    // A promoted-field accessor whose name equals the ENCLOSING type name is CS0542
                    // (a member cannot share its type's name) — debug/gosym's `type Func struct{ *Sym }`
                    // where `Sym` has a field `Func *Func`, so the promoted `Sym.Func` lands as a `Func`
                    // accessor inside `Func`. Δ-prefix the accessor NAME (the field ACCESS on the right
                    // keeps the original name), matching the ΔGoType/Δslice collision-rename precedent.
                    // The promoted field is read directly on the embedded struct (`sym.Func`), never via
                    // the outer value, so no converter reference needs coordinating; a package that DID
                    // read `outer.Func` would surface CS1061 in the gate.
                    // Like the Ꮡ-prefixed accessors below, the Δ-prefixed NAME composes on the
                    // UNESCAPED member name — `Δ@base` is invalid ('@' only leads).
                    string accessorName = GetSimpleName(memberName) == NonGenericStructName ? $"{ShadowVarMarker}{GetUnsanitizedIdentifier(memberName)}" : memberName;
                    result.Append($"\r\n{TypeElemIndent}[global::System.Diagnostics.CodeAnalysis.UnscopedRef] {typeScope} ref {SubstituteTypeParameters(typeName, typeArgMap)} {accessorName} => ref {EmbedHop(promotedStructType, promotedMemberName)}.{memberName};");
                }
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Field Accessor References");

            foreach ((string promotedStructType, string promotedMemberName, _, _, _) in promotedStructs)
            {
                Dictionary<string, string> typeArgMap = GetEmbedTypeArgumentMap(promotedStructType);

                foreach ((string typeName, string memberName, int depth) in getStructMembers(promotedStructType))
                {
                    // Blank `_` field — unaddressable in Go, and its `Ꮡ_` would collide with the
                    // enclosing struct's own `Ꮡ_` (CS0111). Every uniquified spelling counts; see
                    // IsGoBlankMemberName.
                    if (IsGoBlankMemberName(GetSimpleName(memberName)))
                        continue;

                    // Own-field shadowing — see the accessor loop above (CS0111 on Ꮡfn).
                    if (declaredMemberNames.Contains(GetSimpleName(memberName)))
                        continue;

                    // Cross-embed ambiguity — see the depth-aware counting pass above.
                    if (!promotes(GetSimpleName(memberName), depth))
                        continue;

                    // The Ꮡ-prefixed accessor NAME must use the unescaped member name — a C#-keyword
                    // field is escaped `@base`, but `Ꮡ@base` is invalid ('@' only leads). The member
                    // ACCESS on the right keeps `@base`.
                    // Scope derives from the MEMBER name (its exportedness), matching the box-field
                    // accessors: a lowercase field TYPE (uintptr Size_) made an EXPORTED promoted
                    // member internal - invisible cross-assembly (reflect via abi, CS1061 x22).
                    string typeScope = GetScope(GetSimpleName(memberName));
                    // StructName (with type parameters), not NonGenericStructName — a GENERIC
                    // struct's instance param must carry them (Δentry<K, V>, CS0305); the
                    // promoted-struct MEMBER access strips its type arguments (the property is
                    // `node`, not `node<K, V>` — internal/concurrent's entry[K,V]).
                    string promotedFieldType = SubstituteTypeParameters(typeName, typeArgMap);
                    string accessorRef = $"{AddressPrefix}{GetUnsanitizedIdentifier(memberName)}";

                    // A promotion that crosses an EMBEDDED POINTER names storage in ANOTHER
                    // allocation — Go's `f.pfd` for `type File struct{ *file }` IS `f.file.pfd` —
                    // so the hop must be taken BEFORE the field reference is built. A `ref`
                    // accessor that derefs on the right (`instance.@file.Value.pfd`) reaches the
                    // right storage but leaves `of()` rooting the pointer at the OUTER box, and a
                    // ж field reference's identity is (containing allocation, field token): the
                    // same Go address then compares unequal depending on which struct it was
                    // selected through. Emit the pointer-returning shape instead — `of` has an
                    // overload for it, so no call site changes — and let the INNER type's own
                    // accessor build the reference, which composes for a multi-level embed
                    // (fileWithoutReadFrom → *File → *file) because that accessor may itself be
                    // this shape. Value embeds keep the plain `ref` form: their promoted fields
                    // live in the enclosing allocation, so the existing rooting is already right.
                    string pointerEmbedInnerType = PointerEmbedInnerType(promotedStructType, promotedMemberName);

                    result.Append(pointerEmbedInnerType is null
                        ? $"\r\n{TypeElemIndent}{typeScope} static ref {promotedFieldType} {accessorRef}(ref {StructName} instance) => ref instance.{EmbedHop(promotedStructType, promotedMemberName)}.{memberName};"
                        : $"\r\n{TypeElemIndent}{typeScope} static {PointerPrefix}<{promotedFieldType}> {accessorRef}(ref {StructName} instance) => instance.{promotedMemberName}.of({pointerEmbedInnerType}.{accessorRef});");
                }
            }

            return result.ToString();

            IEnumerable<(string typeName, string memberName, int depth)> getStructMembers(string structTypeName)
            {
                // Collect the embedded struct's members TRANSITIVELY: a member promoted into the
                // enclosing struct may itself come from a NESTED embedded struct (Go promotes through
                // every embedding level). e.g. stackWorkBuf embeds stackWorkBufHdr embeds workbufhdr,
                // so stackWorkBuf.nobj must promote workbufhdr's nobj — but reading stackWorkBufHdr's
                // DECLARED members alone misses it (nobj is a generated accessor on stackWorkBufHdr,
                // not a declared field). The emitted accessor stays single-hop (`stackWorkBuf.nobj =>
                // ref stackWorkBufHdr.nobj`), resolving through stackWorkBufHdr's own 1-level promotion.
                // EVERY occurrence is collected, duplicates included: Go's promotion rule is
                // DEPTH-AWARE and the caller applies it (promotes(): unique at the shallowest depth).
                // A name-dedupe here would decide the question first, and decide it WRONG — "first
                // (closest) declaration wins" is only half of Go's rule. It holds across DIFFERENT
                // depths; at the SAME depth Go promotes from neither. Deduping silently turned an
                // ambiguity into a win: reflect's `S0` embeds `D1` AND `D2`, both declaring `d`, so
                // the second `d` was dropped here, the caller counted ONE occurrence, and `S1`
                // promoted `d` through `S0` — emitting `instance.S0.d`, which does not exist (CS1061
                // ×2), while `S0`'s own shell correctly emitted no `d` at all.
                //
                // Feeding the counter every occurrence does not re-open the case it was written for
                // (macho's `FatArch`, Cpu at depth 1 and depth 2): minDepth 1 with count 1 still
                // emits the depth-1 accessor and skips the depth-2 one.
                List<(string typeName, string memberName, int depth)> collected = [];

                collect(structTypeName, [], 1);

                return collected;

                void collect(string typeName, HashSet<string> seenTypes, int depth)
                {
                    // Go forbids embedding cycles, but guard anyway so a malformed input can't loop.
                    if (!seenTypes.Add(typeName))
                        return;

                    (StructDeclarationSyntax? structDecl, Compilation? compilation) = Context.GetStructDeclaration(typeName);

                    if (structDecl is null)
                    {
                        // A CROSS-PACKAGE embedded type cannot be resolved by syntax in a real MSBuild
                        // build: project references arrive as METADATA references, never CompilationReference,
                        // so the referenced-compilations walk finds nothing — `type rtype struct { *abi.Type }`
                        // (runtime type.go) promoted NO fields and every `t.TFlag`/`t.Str`/`t.Kind_` was
                        // CS1061. Fall back to the semantic model: resolve the embedded type's symbol by
                        // metadata name and enumerate its public instance fields (the Go-visible members of
                        // a converted struct are plain public fields). Transitive promotion through a
                        // metadata type's own embeds is not chased (none of the runtime's cross-package
                        // embeds need it); the emitted single-hop accessor resolves through the embedded
                        // type's own generated promotion when it exists.
                        foreach ((string fieldTypeName, string fieldName) in getMetadataStructFields(typeName))
                            collected.Add((fieldTypeName, fieldName, depth));

                        return;
                    }

                    foreach ((string memberType, string memberName, _, bool isEmbedded, _) in structDecl.GetStructMembers(compilation!, true))
                    {
                        collected.Add((memberType, memberName, depth));

                        // An embedded struct field contributes its own members transitively. The
                        // converter emits every embed - value or POINTER - as a `partial ref`
                        // PROPERTY (the marker the top level keys isPromotedStruct on), so recurse
                        // on that flag; a NAMED field whose name merely equals its type's simple
                        // name (`RCode RCode` in dnsmessage.Header) is a plain FIELD and must not
                        // contribute nested promotions.
                        if (isEmbedded)
                            collect(memberType, seenTypes, depth + 1);
                    }
                }

                IEnumerable<(string typeName, string fieldName)> getMetadataStructFields(string typeName)
                {
                    // Normalize the source-form type reference to a CLR metadata name: strip the ж<>
                    // pointer wrapper, the `global::` prefix and `@` identifier escapes, root it in the
                    // `go` namespace, and mark the final containing package class as a NESTED type —
                    // `ж<@internal.abi_package.Type>` → `go.internal.abi_package+Type` (a converted
                    // package is a static class inside the go namespace; its types are nested members).
                    string metadataName = GeneratorExecutionContextExtensions.GetUnderlyingTypeName(typeName)
                        .Replace("global::", "").Replace("@", "");

                    if (!metadataName.StartsWith("go.", StringComparison.Ordinal))
                        metadataName = $"go.{metadataName}";

                    int lastDot = metadataName.LastIndexOf('.');

                    if (lastDot < 0)
                        yield break;

                    INamedTypeSymbol? typeSymbol =
                        Context.Compilation.GetTypeByMetadataName($"{metadataName[..lastDot]}+{metadataName[(lastDot + 1)..]}");

                    if (typeSymbol is null)
                        yield break;

                    // Membership is Go's own promotion rule projected through existing metadata
                    // (see PromotesFromMetadata): public members always — a genuine cross-package
                    // embed sees exactly what Go exports — and unexported ones exactly when the
                    // embedding struct belongs to the SAME Go package, which the `-tests`
                    // reference model splits across an assembly seam (net's resolvConfTest over
                    // *resolverConfig: the internal initOnce/dnsConfig/lastChecked promoted
                    // NOTHING under the old public-only filter, and every converter-emitted
                    // promoted selection was a missing member). The friend grant
                    // (InternalsVisibleTo) is what makes those members legal C# here, and
                    // IsSymbolAccessibleWithin is what consults it.
                    string? embedGoPackage = GetGoPackageName(typeSymbol.ContainingType);

                    // `fieldSymbol`, not `field`: this local function is nested inside the
                    // `PromotedStructDeclarations` GET accessor, and C# 14 makes `field` a KEYWORD there
                    // (the synthesized-backing-field feature). Under LangVersion 14 the declaration is
                    // CS9273 and each use binds to the synthesized backing field instead of the loop
                    // variable — CS1061 on `.DeclaredAccessibility`/`.IsStatic`/`.Name`/`.Type`, which
                    // is what broke every build on the .NET 10 SDK (issue #34). The sibling
                    // `property` loop below is unaffected; `field` is the only contextual keyword the
                    // accessor scope introduces.
                    foreach (IFieldSymbol fieldSymbol in typeSymbol.GetMembers().OfType<IFieldSymbol>())
                    {
                        if (fieldSymbol.IsStatic || fieldSymbol.IsImplicitlyDeclared || !PromotesFromMetadata(fieldSymbol, embedGoPackage))
                            continue;

                        if (GetSimpleName(fieldSymbol.Name) == "_")
                            continue;

                        // A SYMBOL's .Name never carries the `@` a C# keyword needs (unlike syntax
                        // Identifier.Text, which GetStructMembers's source-based sibling reads) —
                        // escape it before it reaches emitted C# (runtime's white-box AddrRange
                        // promoting addrRange.base: CS1519/CS1001/CS1002 in the generated accessor).
                        yield return (fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), EscapeCsKeyword(fieldSymbol.Name));
                    }

                    // A REFERENCED assembly's generated wrapper exposes its embedded member and its
                    // transitively-promoted fields as public REF-RETURNING PROPERTIES (the embed
                    // accessor `public partial ref Type Type` and the promoted accessors) - the
                    // fields-only enumeration missed them, so a struct embedding a cross-package
                    // wrapper (reflect's structType over abi.StructType over abi.Type) promoted
                    // NOTHING from the deeper levels (CS1061 x32 + CS0117 x13). Yield them too;
                    // the standard single-hop `X => ref Embed.X` emission handles both shapes.
                    foreach (IPropertySymbol property in typeSymbol.GetMembers().OfType<IPropertySymbol>())
                    {
                        if (property.IsStatic || !property.ReturnsByRef || !PromotesFromMetadata(property, embedGoPackage))
                            continue;

                        if (property.IsIndexer || GetSimpleName(property.Name) == "_")
                            continue;

                        yield return (property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), EscapeCsKeyword(property.Name));
                    }
                }
            }
        }
    }

    private string PromotedStructReceivers()
    {
        (string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, bool isPublic)[] promotedStructs = StructMembers.Where(item => item.isPromotedStruct).ToArray();

        if (promotedStructs.Length == 0)
            return "";

        StringBuilder result = new();

        result.Append("\r\n\r\n    // Promoted Struct Receivers");

        // Get all extension methods for the struct, any directly defined receivers
        // take precedence over promoted struct methods that have the same name
        (StructDeclarationSyntax? structDecl, Compilation? compilation) = Context.GetStructDeclaration(FullyQualifiedStructType);
        IEnumerable<MethodInfo>? structMethods = structDecl is null ? [] : structDecl.GetExtensionMethods(compilation!);
        HashSet<string> structMethodNames = new(structMethods?.Select(method => method.Name) ?? [], StringComparer.Ordinal);

        // A POINTER-receiver method emitted ONLY as its box primary `M(this ж<T> …)` — Go's
        // `func (s *structType) string()` becomes a direct-ж extension when it takes the address
        // of a receiver field — is invisible to IsExtensionMethodForStruct (value-receiver forms
        // only). Without folding those names in, a same-named method promoted from an embed
        // (encoding/gob's structType embeds CommonType, both declaring string()/safeString()) is
        // not suppressed and its promoted box overload duplicates the direct one (CS0111).
        if (structDecl is not null)
            structMethodNames.UnionWith(structDecl.GetBoxReceiverMethodNames(compilation!));

        // POINTER-embedded types (`*state` → the `ж<state>` hop property): their BOX-receiver primaries
        // (`state.Write(this ж<state>)`) promote too — the embed hop `target.<embed>` is already a ж<T>,
        // so `target.<embed>.M()` binds the box receiver directly. GetExtensionMethods harvests only
        // value-receiver forms, so those box primaries are collected separately below. Keyed by embedded
        // TYPE name (matches promotedStructType). Gated to POINTER embeds: a VALUE embed's `target.<embed>`
        // is a value that cannot bind a ж-receiver (sha3's cshakeState embeds *state — CS1929 without this).
        // Keyed by the embed's SIMPLE type name (dropping any collision prefix) — promotedStructType
        // arrives as the qualified box form `global::go.ж<…Inner>`, which the loops below reduce with
        // the same GetSimpleName call the forwarder emission uses, so both sides normalize to `Inner`.
        HashSet<string> pointerEmbedTypeNames = structDecl is null
            ? new(StringComparer.Ordinal)
            : new(structDecl.GetEmbeddedPointerHopNames().Select(hop => GetSimpleName(hop.TypeName, dropCollisionPrefix: true)), StringComparer.Ordinal);

        // promotedStructType arrives as the qualified BOX form, so its GetSimpleName carries a trailing
        // `.Value` (the pointer deref) the embed keys above do not — strip it before the membership test.
        static string embedKey(string typeName) =>
            GetSimpleName(typeName, dropCollisionPrefix: true) is var n && n.EndsWith(".Value", StringComparison.Ordinal)
                ? n[..^".Value".Length]
                : n;

        // Go's AMBIGUITY rule: a method name promoted from TWO OR MORE embeds at the same depth
        // is not promoted at all — bufio ReadWriter's Reader.Size vs Writer.Size (Go requires the
        // qualified rw.Reader.Size(); both generated wrappers were CS0111).
        Dictionary<string, int> promotedMethodCounts = new(StringComparer.Ordinal);

        foreach ((string promotedStructType, _, _, _, _) in promotedStructs)
        {
            HashSet<string> embedMethodNames = new(StringComparer.Ordinal);

            // A DIRECT (depth-1) NON-GENERIC VALUE embed also promotes its box-receiver (direct-ж)
            // primaries through a descent shim (see IsValueEmbedBoxRecv). Count them for the same
            // cross-embed ambiguity rule so two such embeds carrying the same method name drop (as
            // Go's ambiguity rule requires) rather than emitting a duplicate forwarder. A plain
            // value embed's type name never carries '<' — both the pointer-box embed form (`ж<…>`) and a
            // generic embed do — which is a more robust discriminator than the pointerEmbedTypeNames
            // membership test (whose `@`-keyword-escaped names, e.g. os.File's `ж<@file>`, mismatch).
            bool directEmbedIsValue = !promotedStructType.Contains("<");

            countPromotedMethods(promotedStructType, []);

            foreach (string name in embedMethodNames)
                promotedMethodCounts[name] = promotedMethodCounts.TryGetValue(name, out int count) ? count + 1 : 1;

            void countPromotedMethods(string typeName, HashSet<string> seenTypes)
            {
                if (!seenTypes.Add(typeName))
                    return;

                (StructDeclarationSyntax? decl, Compilation? comp) = Context.GetStructDeclaration(typeName);

                if (decl is null)
                {
                    // METADATA embed: count its same-Go-package promoted methods too, so the
                    // cross-embed ambiguity rule reads the same method set the collection pass
                    // below will forward (see GetMetadataPromotedMethods).
                    (List<MethodInfo> metadataValueMethods, List<MethodInfo> metadataBoxMethods) = GetMetadataPromotedMethods(typeName);

                    foreach (MethodInfo m in metadataValueMethods)
                        embedMethodNames.Add(m.Name);

                    if (pointerEmbedTypeNames.Contains(embedKey(typeName)) || (typeName == promotedStructType && directEmbedIsValue))
                    {
                        foreach (MethodInfo m in metadataBoxMethods)
                            embedMethodNames.Add(m.Name);
                    }

                    return;
                }

                foreach (MethodInfo m in decl.GetExtensionMethods(comp!) ?? [])
                    embedMethodNames.Add(m.Name);

                // A POINTER embed's box-receiver primaries promote too (see below) — count them for
                // the same cross-embed ambiguity rule. A direct VALUE embed likewise promotes its
                // box-receiver primaries via the descent shim.
                if (pointerEmbedTypeNames.Contains(embedKey(typeName)) || (typeName == promotedStructType && directEmbedIsValue))
                {
                    foreach (MethodInfo m in decl.GetBoxReceiverExtensionMethods(comp!))
                        embedMethodNames.Add(m.Name);
                }

                foreach ((string memberType, _, _, bool isEmbedded, _) in decl.GetStructMembers(comp!, true))
                {
                    if (isEmbedded)
                        countPromotedMethods(memberType, seenTypes);
                }
            }
        }

        foreach ((string promotedStructType, string promotedMemberName, _, _, _) in promotedStructs)
        {
            // Rewrite the embed's type PARAMETERS to this instantiation's type ARGUMENTS on the
            // promoted method's return + parameter types — a generic embed's method signature may
            // carry `Point` (`pointFromAffine` returns `(Point, error)`), out of scope on the
            // non-generic enclosing struct.
            Dictionary<string, string> typeArgMap = GetEmbedTypeArgumentMap(promotedStructType);

            // Collect the embedded struct's methods TRANSITIVELY — its own plus those promoted into
            // it from a deeper embedding level (Go promotes methods through every level). A 2+-level
            // method (e.g. top.greet from inner, via top→mid→inner) is forwarded through the 1-level
            // accessor `target.<promotedStruct>.greet()`, which exists by the embedded struct's own
            // one-level promotion of that method. Closest declaration of a name wins (Go's rule).
            List<MethodInfo> promotedStructMethods = [];
            HashSet<string> promotedMethodNames = new(StringComparer.Ordinal);

            // See the identical computation in the ambiguity-counting pass above.
            bool directEmbedIsValue = !promotedStructType.Contains("<");

            // The NARROWER unexported-embed form, kept for the return-type accessibility relaxation
            // below only — that relaxation is about a cross-package call the converter emits as a bare
            // `Ꮡt.M()`, which is the unexported-embed case alone (see its own comment).
            bool directEmbedIsUnexportedValue = directEmbedIsValue &&
                GetScope(GetSimpleName(promotedStructType, dropCollisionPrefix: true)) != "public";

            collectPromotedMethods(promotedStructType, []);

            void collectPromotedMethods(string typeName, HashSet<string> seenTypes)
            {
                // Go forbids embedding cycles, but guard anyway.
                if (!seenTypes.Add(typeName))
                    return;

                (StructDeclarationSyntax? decl, Compilation? comp) = Context.GetStructDeclaration(typeName);

                if (decl is null)
                {
                    // METADATA embed — the `-tests` reference model's same-Go-package shape (net's
                    // resolvConfTest over *resolverConfig, whose init/tryAcquireSema/releaseSema
                    // live in the referenced production assembly). Harvest its promoted methods
                    // from metadata so the standard value/pointer forwarders below are emitted
                    // exactly as a same-assembly embed's would be; a genuine CROSS-package embed
                    // yields nothing here (see GetMetadataPromotedMethods) and keeps the
                    // converter's explicit-hop call emission.
                    (List<MethodInfo> metadataValueMethods, List<MethodInfo> metadataBoxMethods) = GetMetadataPromotedMethods(typeName);

                    foreach (MethodInfo m in metadataValueMethods)
                    {
                        if (promotedMethodNames.Add(m.Name))
                            promotedStructMethods.Add(m);
                    }

                    bool metadataValueEmbedBoxRecv = typeName == promotedStructType && directEmbedIsValue;

                    if (pointerEmbedTypeNames.Contains(embedKey(typeName)) || metadataValueEmbedBoxRecv)
                    {
                        foreach (MethodInfo m in metadataBoxMethods)
                        {
                            if (promotedMethodNames.Add(m.Name))
                                promotedStructMethods.Add(m with { IsBoxRecv = true, IsValueEmbedBoxRecv = metadataValueEmbedBoxRecv });
                        }
                    }

                    return;
                }

                foreach (MethodInfo m in decl.GetExtensionMethods(comp!) ?? [])
                {
                    if (promotedMethodNames.Add(m.Name))
                        promotedStructMethods.Add(m);
                }

                // A POINTER embed's BOX-receiver primaries (`this ж<T>`) promote unchanged — the hop
                // `target.<embed>` is a ж<T>, so the value/pointer forwarders below (`target.<embed>.M(…)`)
                // bind the box receiver directly. GetExtensionMethods above harvests only value-receiver
                // forms, so collect the box primaries here (sha3's cshakeState←*state.Write, CS1929).
                // A direct VALUE embed also promotes its box-receiver primaries, but through the
                // box-field descent shim (IsValueEmbedBoxRecv) — its hop `target.<embed>` is a value,
                // not a ж<T>, so the plain forwarder body would be CS1929. Only the direct embed is taken
                // (a deeper value hop would need a multi-level descent this narrow fix does not emit).
                //
                // ⚠ NOT gated on the embed's EXPORTEDNESS. It was, and that gate was not a Go rule: Go
                // puts a value embed's POINTER-receiver methods in the OUTER type's pointer method set
                // whatever the embed's case, and go2cs reconstructs a Go method set at run time by
                // reading these EMITTED extension methods (GetGoMethodSetCandidates). So an unemitted
                // promotion is an ABSENT Go method — `debug/dwarf`'s `*UintType` (embedding the exported
                // `BasicType`, whose `Basic()` takes a pointer receiver) did not satisfy the lifted
                // anonymous `interface{ Basic() *BasicType }`, and its assertion threw for ten of the
                // package's forty tests.
                bool valueEmbedBoxRecv = typeName == promotedStructType && directEmbedIsValue;

                if (pointerEmbedTypeNames.Contains(embedKey(typeName)) || valueEmbedBoxRecv)
                {
                    foreach (MethodInfo m in decl.GetBoxReceiverExtensionMethods(comp!))
                    {
                        if (promotedMethodNames.Add(m.Name))
                            promotedStructMethods.Add(m with { IsBoxRecv = true, IsValueEmbedBoxRecv = valueEmbedBoxRecv });
                    }
                }

                // Recurse into nested embedded struct fields. The converter emits every embed -
                // value or POINTER - as a `partial ref` PROPERTY, the marker the top level keys
                // isPromotedStruct on; GetStructMembers(..., true) surfaces it as the 4th tuple
                // element. A NAMED field whose name merely equals its type's simple name
                // (`RCode RCode` in dnsmessage.Header) is a plain FIELD, not an embed - recursing
                // into it falsely promotes the field type's methods (Message got RCode's String()
                // forwarded as `target.Header.String()`, but Header has no String - CS1929).
                foreach ((string memberType, _, _, bool isEmbedded, _) in decl.GetStructMembers(comp!, true))
                {
                    if (isEmbedded)
                        collectPromotedMethods(memberType, seenTypes);
                }
            }

            foreach (MethodInfo method in promotedStructMethods)
            {
                if (structMethodNames.Contains(method.Name))
                {
                    result.Append($"\r\n    // '{GetSimpleName(promotedStructType)}.{method.Name}' method mapped to overridden '{NonGenericStructName}' receiver method");
                    continue;
                }

                if (promotedMethodCounts.TryGetValue(method.Name, out int nameCount) && nameCount > 1)
                {
                    result.Append($"\r\n    // '{GetSimpleName(promotedStructType)}.{method.Name}' promotion is AMBIGUOUS across embeds (Go requires the qualified selector) - not promoted");
                    continue;
                }

                // Add ref extension method
                string methodScope = Scope ?? "public";

                // Downgrade a public forwarder whose return type is LESS accessible than public
                // (CS0051: a public method cannot expose an internal return type). The name-based
                // GetScope heuristic treats every Go-lowercase-named type as unexported — but golib
                // builtins (@string, error, bool, nint, …) are PUBLIC C# types, so a promoted method
                // returning one (testing.common.Name → @string) was wrongly made internal and thus
                // invisible cross-assembly. For a DIRECT UNEXPORTED VALUE embed — the only promotion
                // the converter reaches cross-package as a bare `Ꮡt.M()` (b8764925a dropped the
                // inaccessible `.of(T.Ꮡ<embed>)` descent there), where an internal forwarder lets a
                // same-named FOREIGN extension win (flag.Name → CS1929) — trust the return type's
                // ACTUAL accessibility so a public-but-lowercase return keeps the forwarder public.
                // Every other promotion keeps the conservative name heuristic (no golden/compile churn).
                //
                // The VALUE-EMBED BOX SHIM takes the accurate test for the same reason, and it is the
                // stronger case: that shim EXISTS to be reachable across assemblies (it performs a
                // descent the caller cannot spell), so emitting it `internal` defeats its own purpose.
                // The name heuristic gets more than lowercase builtins wrong — a TUPLE return reduces
                // under GetSimpleName to its last dotted segment, `error)`, which reads unexported for
                // every multi-return Go method. archive/zip is the reached case: `Open`, promoted from
                // ReadCloser's exported `Reader` embed, returns `(io.fs.File, error)` and was emitted
                // internal, so the test assembly's own ReadCloser→fs.FS adapter could not bind it
                // (CS1929, and the whole package build-blocked behind it).
                if (method.ReturnType != "void")
                {
                    bool returnTypeIsPublic = GetScope(GetSimpleName(method.ReturnType)) == "public" ||
                        ((directEmbedIsUnexportedValue || method.IsValueEmbedBoxRecv) && method.ReturnTypeIsPublic);

                    if (!returnTypeIsPublic)
                        methodScope = "internal";
                }

                // The PARAMETER-side twin of the return-type downgrade above (W3a's promoted-member-
                // forwarding site, docs/phase4/DESIGN-w3a-wrapper-scaffolding.md's own arc). A VOID-
                // returning promoted method skips the check above entirely, but its own parameters
                // can equally reference an unexported type the forwarder must not expose as public —
                // runtime's white-box `AddrRanges` embeds the unexported `addrRanges`, whose promoted
                // `cloneInto(*addrRanges)` takes an `*addrRanges` argument (CS0051). UNLIKE the
                // return-type check, the semantic answer rescues UNCONDITIONALLY here: this check is
                // new, so there is no legacy internal emission to preserve, and gating the rescue on
                // embed kind demoted Go-public promotions whose rescue flags no path sets — an
                // EXPORTED value embed's box-recv method harvested from METADATA carries neither
                // directEmbedIsUnexportedValue nor IsValueEmbedBoxRecv, so sync.WaitGroup's promoted
                // Add(nint) through an embedding struct went internal and every cross-assembly
                // consumer lost it (CS1929 — PromotedEmbedUser, and net's TCPConn.Read/Write via the
                // same shape's unexported-embed twin). Go's rule is that the METHOD NAME decides
                // visibility; the only legitimate demotion is CS0051 avoidance, which is exactly
                // "semantic says a non-receiver parameter type is not public".
                // method.Parameters[0] is the receiver (Skip(1), matching typedParams below);
                // ParametersArePublic excludes it at both harvest sites (extension-shaped symbols
                // carry it in Parameters[0], instance symbols never did).
                if (methodScope == "public")
                {
                    bool parametersArePublic = method.Parameters.Skip(1).All(param => GetScope(GetSimpleName(param.type)) == "public") ||
                        method.ParametersArePublic;

                    if (!parametersArePublic)
                        methodScope = "internal";
                }
                // The shim mirrors the SOURCE receiver kind: a by-value method stays by-value
                // so an RVALUE receiver binds (reflect v.Elem().kind() - CS1510 on forced ref).
                string recvMod = method.IsRefRecv ? "ref " : "";

                // Substituted return + parameter types (identity map for a non-generic embed).
                string returnType = SubstituteTypeParameters(method.ReturnType, typeArgMap);
                string typedParams = string.Join(", ", method.Parameters.Skip(1).Select(param => $"{SubstituteTypeParameters(param.type, typeArgMap)} {param.name}"));

                // A GENERIC enclosing struct promotes its embed's methods as GENERIC extension methods
                // carrying the struct's OWN type parameters — `wrapped<T>` embedding `tag<T>` emits
                // `static T show<T>(this wrapped<T> target)`, else the `T` in the receiver/return is an
                // undefined type name (CS0246). A NON-generic enclosing struct (p256Curve, whose embed
                // is a CONCRETE instantiation with the type argument already substituted in) carries
                // none, keeping the plain forwarder.
                int structTypeParamStart = StructName.IndexOf('<');
                string methodTypeParams = structTypeParamStart >= 0 ? StructName[structTypeParamStart..] : "";

                // A box-receiver (direct-ж) method promoted through an UNEXPORTED VALUE embed cannot use
                // the value hop `target.<embed>` (a value, not a ж<T> — CS1929). Emit a single PUBLIC box
                // shim that descends through the embed's box-field accessor exactly as the in-package
                // caller renders it inline: `M(this ж<T> Ꮡtarget) => Ꮡtarget.of(T.Ꮡ<embed>).M(args)`. The
                // `Ꮡ<embed>` accessor is `internal` (matching the unexported embed) but reachable from this
                // shim's own assembly, so a FOREIGN caller reaches the exported promoted method by the
                // plain `t.M(…)` call the converter now emits cross-package. No `this ref T` overload — a
                // box receiver cannot bind on a value. Restricted to a NON-generic enclosing struct (the
                // `T.Ꮡ<embed>` FieldReferences accessor is not itself generic); a generic one falls through.
                // The shim scope is the shared `methodScope` (the STRUCT's exportedness, downgraded for a
                // non-public return type) — an unexported enclosing struct (context's `afterFuncCtx`,
                // reflect's `structTypeUncommon`) has an internal receiver `ж<T>`, so a `public` shim there
                // is CS0051. So the shim is public only for an EXPORTED method on an EXPORTED struct
                // returning void/public — exactly the reachable-cross-package case
                // (testing.T.Errorf/Helper/Logf).
                //
                // ⚠ NOT gated on the METHOD's exportedness, for the same reason the harvest above is not
                // gated on the EMBED's. It was, on the reasoning that "an unexported method is never
                // reachable across packages, so it needs no shim and its in-package callers keep the
                // inline descent" — true of the CALL SITES the converter emits, and false of the METHOD
                // SET. Go puts a value embed's pointer-receiver methods in the outer type's pointer
                // method set whatever their case, and go2cs reconstructs that set at run time by reading
                // these EMITTED extension methods (GetGoMethodSetCandidates), so an unemitted promotion is
                // an ABSENT Go method — invisible to StructurallyImplements, to AdapterBinder, and to
                // every type assert and type switch built on them. `net`'s writev fast path is the
                // measured case: `*TCPConn` promotes the unexported `writeBuffers` from its embedded
                // `conn`, `Buffers.WriteTo` reaches it ONLY through `w.(buffersWriter)` — an unexported
                // SAME-PACKAGE interface, so no `GoImplement` record exists or is needed — and with the
                // promotion unemitted the assertion missed, the per-chunk fallback ran, and
                // `TestBuffers_WriteTo` failed nine verdicts with `write calls = 0; want 1` rather than
                // any compile error. An unexported method's shim is emitted `internal`, which is what
                // keeps it off the public surface while leaving it inside the method set the run-time
                // probe reads (an internal extension method is resolved through NonPublic binding flags,
                // exactly as the converter's own `internal static M(this ж<T> …)` primaries are).
                // Guarded by the UnexportedIfaceDynamicAssert behavioral test.
                if (method.IsValueEmbedBoxRecv)
                {
                    if (structTypeParamStart < 0)
                    {
                        string embedBox = GetUnsanitizedIdentifier(promotedMemberName);
                        string shimScope = GetScope(GetSimpleName(method.Name)) == "public" ? methodScope : "internal";

                        result.Append($"\r\n    {shimScope} static {returnType} {method.Name}(this {PointerPrefix}<{StructName}> {AddressPrefix}target");

                        if (method.Parameters.Length > 1)
                        {
                            result.Append(", ");
                            result.Append(typedParams);
                        }

                        result.Append($") => {AddressPrefix}target.of({StructName}.{AddressPrefix}{embedBox}).{method.Name}(");
                        result.Append(string.Join(", ", method.Parameters.Skip(1).Select(ArgumentName)));
                        result.Append(");");
                    }

                    continue;
                }

                // A value-receiver method binds on the embed's deref'd value (`target.<embed>.Value` —
                // GetSimpleName appends `.Value` for a pointer embed); a BOX-receiver primary (`this ж<T>`)
                // binds on the box hop itself (`target.<embed>`), so drop the `.Value` for it. EmbedHop
                // reduces a GENERIC embed's hop to the bare property name (`nistCurve<…>` → `nistCurve`) —
                // the emitted accessor is not itself generic (a no-op for a non-generic embed's hop).
                string embedAccess = EmbedHop(promotedStructType, promotedMemberName);

                if (method.IsBoxRecv && embedAccess.EndsWith(".Value", StringComparison.Ordinal))
                    embedAccess = embedAccess[..^".Value".Length];

                result.Append($"\r\n    {methodScope} static {returnType} {method.Name}{methodTypeParams}(this {recvMod}{StructName} target");

                if (method.Parameters.Length > 1)
                {
                    result.Append(", ");
                    result.Append(typedParams);
                }

                result.Append($") => target.{embedAccess}.{method.Name}(");
                result.Append(string.Join(", ", method.Parameters.Skip(1).Select(ArgumentName)));
                result.Append(");");

                // Add pointer extension method
                result.Append($"\r\n    {methodScope} static {returnType} {method.Name}{methodTypeParams}(this {PointerPrefix}<{StructName}> {AddressPrefix}target");

                if (method.Parameters.Length > 1)
                {
                    result.Append(", ");
                    result.Append(typedParams);
                }

                result.AppendLine(")");
                result.AppendLine("    {");
                result.AppendLine($"        ref var target = ref {AddressPrefix}target.Value;");
                result.Append($"        {(method.ReturnType == "void" ? "" : "return ")}target.{method.Name}(");
                result.Append(string.Join(", ", method.Parameters.Skip(1).Select(ArgumentName)));
                result.AppendLine(");");
                result.Append("    }");
            }
        }

        return result.ToString();
    }

    // EmbedHop renders the `target.<embed>` hop every promoted accessor / forwarder descends
    // through. It is the embed's DECLARED member name — NOT the embed TYPE's simple name, which the
    // two only coincidentally share: the converter Δ-renames an embedded field whose derived name
    // equals the ENCLOSING struct's own name (io_test's `type Buffer struct{ bytes.Buffer }` →
    // member `ΔBuffer`, CS0542), and a hop still spelled `Buffer` then binds the enclosing TYPE
    // instead of the member — CS0120 on every promoted field accessor, CS1061 on every `Ꮡ`
    // reference and forwarder. A POINTER embed keeps the `.Value` deref GetSimpleName appends to
    // its `ж<T>` box form; a GENERIC embed's declared member never carries type arguments (the
    // converter strips them), so StripGenericTypeArguments keeps a generic embed's simple name
    // comparable against the `.Value` suffix this test is really asking about.
    private static string EmbedHop(string promotedStructType, string memberName) =>
        StripGenericTypeArguments(GetSimpleName(promotedStructType, dropCollisionPrefix: true))
            .EndsWith(".Value", StringComparison.Ordinal) ? $"{memberName}.Value" : memberName;

    private string? m_enclosingGoPackage;
    private bool m_enclosingGoPackageResolved;

    // The Go package identity of the ENCLOSING struct's containing package class, read from its
    // [GoPackage] attribute — the identity that survives the `-tests` reference model's assembly
    // split (net_package and net_internal_test_package both carry [GoPackage("net")]; the
    // external-test class carries [GoPackage("net_test")], a DIFFERENT Go package, exactly as Go
    // defines it). Null when the struct or its container cannot be resolved, which every consumer
    // reads as "not the same package" — the conservative, pre-existing behavior.
    private string? EnclosingGoPackageName
    {
        get
        {
            if (m_enclosingGoPackageResolved)
                return m_enclosingGoPackage;

            m_enclosingGoPackageResolved = true;
            m_enclosingGoPackage = GetGoPackageName(Context.Compilation.FindTypeSymbol(FullyQualifiedStructType)?.ContainingType);

            return m_enclosingGoPackage;
        }
    }

    // Reads the [GoPackage("…")] identity off a package class symbol (source or metadata), or null
    // when the class carries none (hand-written classes outside the conversion model).
    private static string? GetGoPackageName(INamedTypeSymbol? packageClass)
    {
        return packageClass?.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass is
        {
            Name: "GoPackageAttribute",
            ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true }
        })?.ConstructorArguments.FirstOrDefault().Value as string;
    }

    // Go's promotion rule for a member read from METADATA, projected through what the compiler
    // already knows: the member must be accessible to THIS compilation — IsSymbolAccessibleWithin
    // folds in the friend grant (InternalsVisibleTo) the `-tests` reference model mints — and it
    // must be either exported (public: what Go promotes across packages) or a member of the SAME
    // Go package as the embedding struct (what Go promotes within one, which the reference model
    // splits across two assemblies). Accessibility alone would over-promote: the friend grant
    // reaches the whole test assembly, external-test (`<pkg>_test`) classes included, where Go
    // itself promotes only exported members.
    private bool PromotesFromMetadata(ISymbol member, string? memberGoPackage)
    {
        if (!Context.Compilation.IsSymbolAccessibleWithin(member, Context.Compilation.Assembly))
            return false;

        return member.DeclaredAccessibility == Accessibility.Public ||
               (memberGoPackage is not null && memberGoPackage == EnclosingGoPackageName);
    }

    // METADATA sibling of the syntax method harvests (GetExtensionMethods /
    // GetBoxReceiverExtensionMethods), for an embedded struct whose SOURCE this compilation cannot
    // see. A converted Go method is a static extension on the TYPE's containing package class, so
    // that class's metadata carries the full signatures; receivers split exactly as the syntax
    // side's do — `this T` / `this ref T` value forms versus the direct-ж box primary `this ж<T>`.
    //
    // SAME-GO-PACKAGE ONLY, by [GoPackage] identity: a genuine cross-package embed returns empty
    // and keeps the converter's explicit-hop call emission (convSelectorExpr's metadata-embed arm,
    // "a metadata embed promotes FIELDS only") — minting cross-package forwarders here would be a
    // corpus-wide generated-code change this deliberately is not. What it serves is the `-tests`
    // reference model's white-box shape: net's resolvConfTest embeds *resolverConfig, whose
    // init (box primary) and tryAcquireSema/releaseSema ([GoRecv] ref) live in the referenced
    // production assembly and are reachable through the friend grant, so Go's same-package
    // promotion must survive the assembly seam. Transitive promotion through the metadata type's
    // own embeds is not chased, matching the field scan's documented single-hop stance.
    private (List<MethodInfo> valueMethods, List<MethodInfo> boxMethods) GetMetadataPromotedMethods(string embedTypeName)
    {
        List<MethodInfo> valueMethods = [];
        List<MethodInfo> boxMethods = [];

        INamedTypeSymbol? embedType = Context.FindUnderlyingStructSymbol(embedTypeName);

        if (embedType?.ContainingType is not INamedTypeSymbol packageClass)
            return (valueMethods, boxMethods);

        string? embedGoPackage = GetGoPackageName(packageClass);

        if (embedGoPackage is null || embedGoPackage != EnclosingGoPackageName)
            return (valueMethods, boxMethods);

        // Names that must stay resolvable as BARE calls inside the embedding struct's own class.
        // Go lets a package-level FUNCTION and a METHOD share a name (they live in different
        // scopes — `LookupHost` and `(*Resolver).LookupHost`), but the emission folds both into
        // one static package class, imported into the test class via `using static` — and a
        // same-named forwarder minted INTO that class SHADOWS the import for every bare call
        // (C# member lookup finds class methods first: net's lookupCustomResolver embeds
        // *Resolver, and its minted Lookup* forwarders cost 54 CS1501s on plain `LookupHost(host)`
        // calls). The bare function call has no other spelling the converter emits, while a
        // promoted-method call always has the explicit hop through the embed — so the function
        // wins and the colliding forwarder is skipped. (Residual, unmeasured: a Go call of such a
        // colliding method THROUGH the embedding struct would need the converter's explicit hop.)
        HashSet<string> packageFunctionNames = new(StringComparer.Ordinal);

        foreach (IMethodSymbol packageFunction in packageClass.GetMembers().OfType<IMethodSymbol>())
        {
            if (packageFunction.IsStatic && !packageFunction.IsExtensionMethod &&
                packageFunction.MethodKind == MethodKind.Ordinary &&
                Context.Compilation.IsSymbolAccessibleWithin(packageFunction, Context.Compilation.Assembly))
            {
                packageFunctionNames.Add(packageFunction.Name);
            }
        }

        foreach (IMethodSymbol method in packageClass.GetMembers().OfType<IMethodSymbol>())
        {
            if (packageFunctionNames.Contains(method.Name))
                continue;

            // IsExtensionMethod keeps package-level FUNCTIONS out: a Go `func f(c resolverConfig)`
            // is an ordinary static whose first parameter merely has the embed's type — only the
            // `this`-marked receiver forms are Go METHODS.
            if (!method.IsStatic || !method.IsExtensionMethod || method.Parameters.Length == 0)
                continue;

            if (!Context.Compilation.IsSymbolAccessibleWithin(method, Context.Compilation.Assembly))
                continue;

            IParameterSymbol receiver = method.Parameters[0];

            bool isBoxReceiver = receiver.Type is INamedTypeSymbol { Name: PointerPrefix, TypeArguments.Length: 1 } boxType &&
                                 SymbolEqualityComparer.Default.Equals(boxType.TypeArguments[0].OriginalDefinition, embedType.OriginalDefinition);

            if (!isBoxReceiver && !SymbolEqualityComparer.Default.Equals(receiver.Type.OriginalDefinition, embedType.OriginalDefinition))
                continue;

            // IsRefRecv comes from the RECEIVER's ref kind here — the shared symbol-based
            // GetMethodInfo sets it from ReturnsByRef, which serves its interface-member callers
            // but is the wrong question for an extension receiver.
            MethodInfo info = method.GetMethodInfo() with { IsRefRecv = receiver.RefKind == RefKind.Ref };

            // ToParameterInfos drops a `params` variadic modifier; restore it for the same
            // reason the syntax harvest preserves it (a Go variadic promoted without `params`
            // rejects element-form calls — CS1929's shape).
            if (method.Parameters[^1].IsParams && info.Parameters.Length > 0)
                info.Parameters[^1] = ($"params {info.Parameters[^1].type}", info.Parameters[^1].name);

            // ToParameterInfos also drops a parameter's DEFAULT VALUE and its caller-info
            // ATTRIBUTES, and both are load-bearing for exactly the reason `params` is above.
            // A dropped default turns an N-argument call into an (N+1)-argument one, so the
            // promoted forwarder stops binding and every ΔValue-receiver call site fails CS1929
            // -- the forwarder is generated, so the break surfaces in the CONSUMER, pointing away
            // from the embed. A dropped [CallerMemberName] is worse because it is SILENT: the
            // forwarder still compiles and still captures a name, but it captures the FORWARDER's
            // caller correctly only when the attribute rides along; without it the parameter falls
            // back to its default and the captured name is lost with no diagnostic anywhere.
            for (int i = 1; i < info.Parameters.Length && i < method.Parameters.Length; i++)
            {
                IParameterSymbol p = method.Parameters[i];
                string type = info.Parameters[i].type;

                foreach (AttributeData attr in p.GetAttributes())
                {
                    string? attrName = attr.AttributeClass?.ToDisplayString();
                    if (attrName is "System.Runtime.CompilerServices.CallerMemberNameAttribute" or
                                    "System.Runtime.CompilerServices.CallerFilePathAttribute" or
                                    "System.Runtime.CompilerServices.CallerLineNumberAttribute" or
                                    "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute")
                    {
                        type = $"[global::{attrName}] {type}";
                    }
                }

                string name = info.Parameters[i].name;

                if (p.HasExplicitDefaultValue)
                {
                    object? dv = p.ExplicitDefaultValue;
                    string literal = dv switch
                    {
                        null => p.Type.IsValueType ? "default" : "null",
                        // Escaped by hand: Roslyn's SymbolDisplay does not resolve here (the name binds
                        // as a namespace under this file's usings), and the cases a promoted default
                        // can take are few and closed.
                        string sv => "@\"" + sv.Replace("\"", "\"\"") + "\"",
                        char cv => "(char)" + ((int)cv).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        bool bv => bv ? "true" : "false",
                        _ => System.Convert.ToString(dv, System.Globalization.CultureInfo.InvariantCulture) ?? "default"
                    };
                    name = $"{name} = {literal}";
                }

                info.Parameters[i] = (type, name);
            }

            (isBoxReceiver ? boxMethods : valueMethods).Add(info);
        }

        return (valueMethods, boxMethods);
    }

    // A recorded parameter carries its DEFAULT in the name slot (see the harvest below), because the
    // declaration emit is `$"{type} {name}"` and a default only has a legal position after the name.
    // The three CALL emits want the bare identifier, so they strip it here rather than each re-deriving
    // the split -- passing `method = ""` as an argument is a syntax error, and it would be produced at
    // three sites independently.
    private static string ArgumentName((string type, string name) parameter) =>
        MethodInfo.ParameterIdentifier(parameter.name);

    // The POINTED-TO type of a pointer embed, or null when the re-rooted form cannot be emitted for
    // it. It is what a promoted field-reference accessor must re-root at: the embed's declared type
    // is the box `ж<Inner>`, so the pointee is its single type ARGUMENT, already fully qualified
    // where the template got it. Detection reuses EmbedHop's own test — the `.Value` deref it appends
    // IS the statement "this hop crosses a pointer" — so the two can never disagree about which
    // embeds are pointers.
    //
    // Two cases decline, and both keep the plain `ref` form: it still reaches the right storage, and
    // only its pointer IDENTITY keeps the pre-existing outer rooting.
    //
    //   - A named pointer TYPE (a generated wrapper rather than `ж<T>`) parses to no type argument,
    //     so there is nothing to name as the inner type.
    //   - A CROSS-PACKAGE embed, whose declaration syntax is unavailable in this compilation. The
    //     re-rooted form names the INNER type's own `Ꮡ<member>` accessor, which exists only for the
    //     members that type's own generator saw; a cross-package embed is enumerated from METADATA
    //     instead (see getStructMembers), and metadata surfaces public fields the inner DECLARATION
    //     never had — the reflect bridge's hand-added `abi.Type.sysType`/`arrayDims`, promoted into
    //     `runtime.rtype`, have no generated accessor to name (CS0117).
    private string? PointerEmbedInnerType(string promotedStructType, string memberName)
    {
        if (EmbedHop(promotedStructType, memberName) == memberName)
            return null;

        (StructDeclarationSyntax? innerDeclaration, _) = Context.GetStructDeclaration(promotedStructType);

        if (innerDeclaration is null)
            return null;

        List<string> typeArguments = ParseTopLevelTypeArguments(promotedStructType);

        return typeArguments.Count == 1 ? typeArguments[0] : null;
    }

    private static readonly Dictionary<string, string> s_noTypeArgs = new(StringComparer.Ordinal);

    // Builds the type-parameter → type-argument substitution for a promoted embed that is a GENERIC
    // INSTANTIATION (`nistCurve<P256PointжnistPoint>`). The embed's field and method signatures are
    // harvested from the generic DECLARATION (`nistCurve<Point>`), so they reference the declaration's
    // type PARAMETER names (`Point`); those must be rewritten to the instantiation's type ARGUMENTS
    // (`P256PointжnistPoint`) before promotion onto the enclosing struct — otherwise a promoted
    // accessor/forwarder references an out-of-scope `Point` (CS0246). Empty for a non-generic embed.
    private Dictionary<string, string> GetEmbedTypeArgumentMap(string promotedStructType)
    {
        List<string> typeArguments = ParseTopLevelTypeArguments(promotedStructType);

        if (typeArguments.Count == 0)
            return s_noTypeArgs;

        (StructDeclarationSyntax? decl, _) = Context.GetStructDeclaration(promotedStructType);
        TypeParameterListSyntax? typeParameterList = decl?.TypeParameterList;

        if (typeParameterList is null || typeParameterList.Parameters.Count != typeArguments.Count)
            return s_noTypeArgs;

        Dictionary<string, string> map = new(StringComparer.Ordinal);

        for (int i = 0; i < typeArguments.Count; i++)
            map[typeParameterList.Parameters[i].Identifier.Text] = typeArguments[i];

        return map;
    }

    // Splits the OUTERMOST `<…>` of a generic type reference into its top-level type arguments,
    // tracking nesting so an inner comma stays with its argument (`Foo<Bar<A, B>, C>` → [`Bar<A, B>`, `C`]).
    private static List<string> ParseTopLevelTypeArguments(string typeName)
    {
        int lt = typeName.IndexOf('<');

        if (lt < 0 || !typeName.EndsWith(">"))
            return [];

        string inner = typeName[(lt + 1)..^1];
        List<string> arguments = [];
        int depth = 0;
        int start = 0;

        for (int i = 0; i < inner.Length; i++)
        {
            switch (inner[i])
            {
                case '<':
                    depth++;
                    break;
                case '>':
                    depth--;
                    break;
                case ',' when depth == 0:
                    arguments.Add(inner[start..i].Trim());
                    start = i + 1;
                    break;
            }
        }

        string last = inner[start..].Trim();

        if (last.Length > 0)
            arguments.Add(last);

        return arguments;
    }

    // Rewrites whole-identifier occurrences of a promoted generic embed's type PARAMETERS to the
    // instantiation's type ARGUMENTS in an emitted type string (`global::System.Func<Point>` →
    // `global::System.Func<P256PointжnistPoint>`). Scans identifier tokens so a substring match
    // never fires (a `Point` map entry leaves `P256PointжnistPoint` untouched); the ж glyph is a
    // Unicode letter, so a marker-bearing argument reads as a single identifier.
    private static string SubstituteTypeParameters(string typeName, Dictionary<string, string> typeArgumentMap)
    {
        if (typeArgumentMap.Count == 0 || string.IsNullOrEmpty(typeName))
            return typeName;

        StringBuilder result = new(typeName.Length);
        int i = 0;

        while (i < typeName.Length)
        {
            char c = typeName[i];

            if (char.IsLetter(c) || c == '_')
            {
                int start = i++;

                while (i < typeName.Length && (char.IsLetterOrDigit(typeName[i]) || typeName[i] == '_'))
                    i++;

                string identifier = typeName[start..i];
                result.Append(typeArgumentMap.TryGetValue(identifier, out string? replacement) ? replacement : identifier);
            }
            else
            {
                result.Append(c);
                i++;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Whether a member name is the C# spelling of a Go BLANK field. Go allows a struct to repeat
    /// `_`, and the converter uniquifies the repeats by appending underscores (`_`, `__`, `___`, …),
    /// so the whole all-underscores family is one Go concept: a field with a name that names nothing
    /// and, per the spec, no address to take.
    /// </summary>
    private static bool IsGoBlankMemberName(string simpleName)
    {
        if (simpleName.Length == 0)
            return false;

        foreach (char c in simpleName)
        {
            if (c != '_')
                return false;
        }

        return true;
    }

    private string FieldReferences
    {
        get
        {
            if (StructMembers.Count == 0)
                return $"// -- {NonGenericStructName} has no defined fields";

            StringBuilder result = new();

            foreach ((string typeName, string memberName, _, _, _) in StructMembers)
            {
                // A blank `_` field is unaddressable in Go; a second `_` field (own + promoted, or
                // multiple padding fields) would also make duplicate `Ꮡ_` accessors (CS0111).
                //
                // EVERY uniquified spelling counts, not just the first. The converter renames repeated
                // blanks `_`, `__`, `___`, … so a struct with two of them (sync/atomic's Int64 carries
                // `_ noCopy` and `_ align64`) kept emitting `Ꮡ__` for the second — an accessor for a
                // field Go says has no address. That was inert until the zero-size-field layout arc
                // made such a field `readonly` to stop it clobbering the field it overlaps, at which
                // point the writable ref stopped compiling (CS8160). Skipping the whole blank class is
                // what the rule above always meant.
                if (IsGoBlankMemberName(GetSimpleName(memberName)))
                    continue;

                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                // The box-field accessor's accessibility must match the FIELD's, not the field type's:
                // an EXPORTED field (`Fun`) is addressable cross-package, so `ᏑFun` must be `public`
                // even when its type's simple name is lowercase (`array<nuint>` → GetScope would give
                // `internal`, making `other.of(ITab.ᏑFun)` unreachable, CS0117). Derive the scope from
                // the member name (its exportedness), as the field declaration itself does.
                string fieldScope = GetScope(GetSimpleName(memberName));
                result.Append($"{fieldScope} static ref {typeName} {AddressPrefix}{GetUnsanitizedIdentifier(memberName)}(ref {StructName} instance) => ref instance.{memberName};");
            }

            return result.ToString();
        }
    }

    private string Constructors
    {
        get
        {
            StringBuilder result = new();

            // Construct from nil: field initializers already ran (C# executes them in every explicitly
            // declared constructor) and C# 11 auto-defaults any field the body leaves unassigned, so
            // PLAIN scalar/reference/slice/map members need no assignment — re-assigning `default!` would
            // NULL an array field's `= new(N)` backing (a `[N]T` field of `S{}` NREd on first index).
            // AppendZeroValueInitializers assigns only what `default` leaves broken: the promoted-embed
            // boxes (readonly `ж<T>` fields with no initializer) and any plain struct-typed field whose
            // own type needs construction (see AppendZeroValueInitializers).
            result.AppendLine($"public {NonGenericStructName}(NilType _)");
            result.AppendLine($"{TypeElemIndent}{{");
            AppendZeroValueInitializers(result);
            result.AppendLine($"{TypeElemIndent}}}");

            // Parameterless constructor so C# RUNS the struct's field initializers — most importantly an
            // array field's `= new(N)`, which gives a Go `[N]T` field its fixed length (its backing T[]).
            // Without an EXPLICITLY declared parameterless constructor, `new S()` uses the implicit struct
            // constructor, which zeroes every field and SKIPS field initializers — leaving an array field's
            // backing null, so indexing/`len` on it throws NullReferenceException. (C# 11 auto-defaults any
            // field lacking an initializer; a slice/map/etc. field — which has no `= new(N)` initializer —
            // stays its nil zero value, matching Go.) The promoted-embed boxes are allocated here too, so
            // `new S()` — the zero value golib's `@new<T>()`/`heap()` materialize — is fully usable.
            result.AppendLine();
            result.AppendLine($"{TypeElemIndent}public {NonGenericStructName}()");
            result.AppendLine($"{TypeElemIndent}{{");
            AppendZeroValueInitializers(result);
            result.AppendLine($"{TypeElemIndent}}}");

            // Generate exported constructor from public fields. When an ALL-fields internal
            // ctor follows (mixed-visibility struct), a same-assembly named-args call matching
            // the public subset is ambiguous between the two all-optional overloads (CS0121,
            // os fileStat) - deprioritize the subset so same-assembly calls bind the full ctor;
            // cross-assembly callers never see the internal one, so resolution there is unaffected.
            if (PublicStructMembers.Count != StructMembers.Count && PublicStructMembers.Count > 0)
                result.AppendLine($"{TypeElemIndent}[global::System.Runtime.CompilerServices.OverloadResolutionPriority(-1)]");

            GenerateConstructor("public", PublicStructMembers, result);

            // Generate internal constructor with all fields
            if (PublicStructMembers.Count != StructMembers.Count)
            {
                result.AppendLine();
                GenerateConstructor("internal", StructMembers, result);
            }

            return result.ToString();
        }
    }

    // Builds the zero value that golib's `@new<T>()`/`heap()` materialize into a FULLY USABLE
    // instance — shared by the NilType and parameterless constructors (the parameterized
    // constructors box/assign their incoming member values instead). Two things need it:
    //   (1) every promoted-embed box — a readonly `ж<T>` field with no initializer, so it exists
    //       only when a constructor allocates it; and
    //   (2) every plain (non-embed) struct-typed FIELD whose type itself needs construction — its
    //       own promoted-embed box or fixed-array (`[N]T`) backing, at any depth. C# implicitly
    //       zeroes such a field to `default(FieldType)`, whose nested box/backing is null, so the
    //       first touch NREs (`@new<pp>()` — pp has a field `fmt`, and `fmt` embeds `fmtFlags`
    //       whose box is null → clearflags). Constructing it via its own NilType constructor
    //       recursively initializes its needy members, so a single-level `new FieldType(nil)` here
    //       fixes every depth. Reference fields (pointers/interfaces/delegates) keep their nil zero
    //       value (correct — a nil `*T` matches Go); a fixed-array FIELD self-initializes via its
    //       own `= new(N)` field initializer (which every explicitly declared constructor runs), so
    //       it needs no assignment; a cross-package/golib field type is left `default` (conservative
    //       — see StructTypeNeedsConstruction).
    private void AppendZeroValueInitializers(StringBuilder result) =>
        AppendZeroValueInitializers(result, StructMembers);

    private void AppendZeroValueInitializers(StringBuilder result, IEnumerable<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, bool isPublic)> structMembers)
    {
        foreach ((string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, _) in structMembers)
        {
            if (GetSimpleName(memberName) == "_")
                continue;

            if (isPromotedStruct)
            {
                result.Append($"{TypeElemIndent}    ");
                // A keyword-named embed composes the box field from the UNESCAPED member name
                // ('@' is only valid leading an identifier - 'Ꮡʗ@base' is CS1002).
                result.AppendLine($"{CapturedVarMarker}{GetUnsanitizedIdentifier(memberName)} = new {PointerConstructedTypeName(typeName)}(nil);");
                continue;
            }

            if (isReferenceType || !StructTypeNeedsConstruction(typeName))
                continue;

            result.Append($"{TypeElemIndent}    ");
            result.AppendLine($"this.{memberName} = new {typeName}(nil);");
        }
    }

    // The TYPE a pointer member declares (`ж<T>`) is abstract under the B1 per-kind split, so a
    // CONSTRUCTION of it names the standard kind instead (`new StandardBox<T>(nil)`) — the same
    // declared-type/constructed-type pairing the converter's global-address emission uses. Only an
    // OUTER pointer spelling maps (flat or `global::go.`-rooted); a pointer nested inside another
    // type's generic arguments is that type's business, and a non-pointer name passes through.
    private static string PointerConstructedTypeName(string typeName)
    {
        string flat = PointerPrefix + "<";

        if (typeName.StartsWith(flat, StringComparison.Ordinal))
            return BoxConstructPrefix + typeName.Substring(PointerPrefix.Length);

        string rooted = "global::go." + flat;

        if (typeName.StartsWith(rooted, StringComparison.Ordinal))
            return "global::go." + BoxConstructPrefix + typeName.Substring("global::go.".Length + PointerPrefix.Length);

        return typeName;
    }

    private readonly Dictionary<string, bool> m_needsConstructionCache = new(StringComparer.Ordinal);

    // Reports whether a value struct's zero value (`default(T)`) is broken — i.e. it has a
    // promoted-embed box (constructor-allocated) or a fixed-array field (`= new(N)` initializer
    // that `default` skips), directly or through a nested value-struct field — so a field of this
    // type must be constructed rather than left `default`. Only CONVERTER-GENERATED structs
    // (resolvable by syntax in this compilation) qualify: a `true` result guarantees the type has
    // the public `T(NilType)` constructor `new T(nil)` needs, and a cross-package/golib type
    // (unresolvable here) returns false so no bare `new T(nil)` is emitted for it — its own package
    // constructs it, and its default (nil slice/map/@string) is correct anyway.
    private bool StructTypeNeedsConstruction(string typeName) =>
        NeedsConstruction(Context, typeName, m_needsConstructionCache);

    // The predicate is STATIC and takes its context and cache explicitly so the ONE definition can
    // also answer for a named fixed-size ARRAY's element (TypeGenerator's Array arm — the wrapper's
    // lazy backing must construct a needy element's zero value exactly as a needy FIELD is
    // constructed here). It stays in this file because it is one rule with one set of reasons, and a
    // second copy beside InheritedTypeTemplate is the shape this repo's array work keeps paying for
    // (the converter's own arrayLengthArgs comment names the same hazard from the other side).
    //
    // The cache is a PARAMETER rather than a static field: a bare type NAME is not unique across
    // compilations (`Pointer` names a different type in half a dozen assemblies), so a process-wide
    // cache keyed on it would answer one assembly's question with another's answer. Each caller owns
    // a cache whose lifetime is one compilation.
    internal static bool NeedsConstruction(GeneratorExecutionContext context, string typeName, Dictionary<string, bool> cache)
    {
        if (cache.TryGetValue(typeName, out bool cached))
            return cached;

        bool result = NeedsConstruction(context, typeName, [], cache);
        cache[typeName] = result;
        return result;
    }

    private static bool NeedsConstruction(GeneratorExecutionContext context, string typeName, HashSet<string> seen, Dictionary<string, bool> cache)
    {
        if (cache.TryGetValue(typeName, out bool cached))
            return cached;

        // Go forbids value-type embedding cycles (infinite size), and reference fields are skipped
        // before recursion, so a cycle cannot actually occur — the guard is purely defensive.
        if (!seen.Add(typeName))
            return false;

        (StructDeclarationSyntax? structDecl, Compilation? compilation) = context.GetStructDeclaration(typeName);

        // A type whose SOURCE this compilation cannot see — the normal shape of a cross-package
        // field in a real MSBuild build, where a <ProjectReference> arrives as compiled METADATA —
        // resolves by SYMBOL instead. Leaving it `default` here is not conservative: if the type
        // carries a fixed array at any depth, `default` gives that array a NULL backing (golib's
        // deliberate zero-value discriminator), so the first pin throws "Handle is not initialized"
        // from GCHandle.AddrOfPinnedObject (math/rand/v2's ChaCha8, whose cross-package
        // chacha8rand.State holds `buf = new(32)`). Go's `new(ChaCha8)` yields 32 real zeroed words.
        if (structDecl is null)
            return context.Compilation.FindTypeSymbol(typeName) is { TypeKind: TypeKind.Struct } typeSymbol &&
                   NeedsConstruction(typeSymbol, seen, cache);

        foreach ((string memberType, string memberName, bool isReferenceType, bool isProperty, _) in structDecl.GetStructMembers(compilation!, true))
        {
            if (GetSimpleName(memberName) == "_")
                continue;

            // A promoted embed surfaces as a `partial ref` PROPERTY (isProperty). Its inline slot's
            // own `default` is now a usable Go zero value, so this is CONSERVATIVE rather than
            // required: it keeps the constructor route (and therefore the converter's `new(nil)`
            // renderings, which key off the same shape) for every embed, whether or not the embedded
            // type is itself needy. Narrowing it to `NeedsConstruction(memberType, seen)` would be
            // correct and would drop some constructions, but it moves converter EMISSION and is left
            // to a change that owns that footprint.
            if (isProperty)
                return true;

            if (isReferenceType)
                continue;

            // A fixed-size array field (`[N]T` → golib `array<T>`) carries a `= new(N)` initializer
            // that `default(T)` skips, leaving a null backing.
            if (memberType.Contains("go.array<"))
                return true;

            if (NeedsConstruction(context, memberType, seen, cache))
                return true;
        }

        return false;
    }

    // Symbol-based counterpart of the syntax walk above, for a type that reached us as compiled
    // METADATA. Same rule, same three triggers (promoted-embed box, fixed array, needy nested
    // value struct); only the member enumeration differs.
    private static bool NeedsConstruction(INamedTypeSymbol structType, HashSet<string> seen, Dictionary<string, bool> cache)
    {
        // `new T(nil)` is only emittable when the type actually exposes the public NilType
        // constructor the generator gives converter-produced structs — a hand-written golib struct
        // (or any other referenced type) has no such constructor, and its `default` is its correct
        // Go zero value anyway. Metadata is fully compiled, so unlike the source path this can be
        // CHECKED rather than assumed.
        if (!structType.InstanceConstructors.Any(constructor =>
                constructor.DeclaredAccessibility == Accessibility.Public &&
                constructor.Parameters.Length == 1 &&
                constructor.Parameters[0].Type.Name == "NilType"))
            return false;

        foreach (ISymbol member in structType.GetMembers())
        {
            if (member.IsStatic || member.IsImplicitlyDeclared || GetSimpleName(member.Name) == "_")
                continue;

            switch (member)
            {
                // A promoted embed's `partial ref` property — its box is constructor-allocated.
                // IsIndexer excludes a ref-returning INDEXER, which Roslyn also models as an
                // IPropertySymbol but the syntax path never saw (it matches PropertyDeclarationSyntax,
                // not IndexerDeclarationSyntax). Every golib named-slice/array wrapper declares
                // `public ref T this[nint index]`, so without this the walk called EVERY such wrapper
                // needy — `image/color.Palette`, `net.IP`, `go/scanner.ErrorList` — and emitted a
                // pointless `new Palette(nil)` whose body is just `m_value = default!`, i.e. exactly
                // the `default` it replaced.
                case IPropertySymbol { ReturnsByRef: true, IsIndexer: false }:
                    return true;
                case IFieldSymbol { Type: { IsReferenceType: false } fieldType }:
                {
                    // A fixed-size array field (`[N]T` → golib `array<T>`) carries a `= new(N)`
                    // initializer that `default(T)` skips, leaving a null backing.
                    if (fieldType is INamedTypeSymbol { Name: "array", Arity: 1, ContainingNamespace.Name: "go" })
                        return true;

                    if (fieldType is INamedTypeSymbol { TypeKind: TypeKind.Struct } nestedType &&
                        seen.Add(nestedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)) &&
                        NeedsConstruction(nestedType, seen, cache))
                        return true;

                    break;
                }
            }
        }

        return false;
    }

    private void GenerateConstructor(string scope, List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, bool isPublic)> structMembers, StringBuilder result)
    {
        if (structMembers.Count == 0)
            return;

        result.AppendLine();
        result.Append($"{TypeElemIndent}{scope} {NonGenericStructName}(");
        // A needy VALUE-struct member takes a NULLABLE parameter (`onceError? rerr = default!`) so an
        // OMITTED argument is a genuine `null` sentinel the body can distinguish from a passed value —
        // a non-nullable `onceError` param can only default to the BROKEN `default(onceError)` (null
        // embed box), which the body could not tell apart from a caller-supplied zero. See the body's
        // `?? new T(nil)` reconstruction and IsNeedyValueStructMember.
        result.Append(string.Join(", ", structMembers.Select(item =>
            $"{(IsNeedyValueStructMember(item.typeName, item.isReferenceType, item.isPromotedStruct) ? $"{item.typeName}?" : item.typeName)} {item.memberName} = default!")));
        result.AppendLine(")");
        result.AppendLine($"{TypeElemIndent}{{");

        foreach ((string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, _) in structMembers)
        {
            result.Append($"{TypeElemIndent}    ");

            // A POINTER embed (its member type is the reference type ж<T>) whose argument is
            // omitted arrives as a raw null; hold a nil BOX (`new ж<T>(nil)`) instead, matching
            // the NilType/parameterless constructors, so a genuine deref of the nil embed panics
            // (Go nil-pointer semantics) rather than NREs, and a nil embed compares equal
            // regardless of which constructor produced it (ж.Equals: nil == nil).
            string memberValue = isPromotedStruct && isReferenceType ? $"{memberName} ?? new {PointerConstructedTypeName(typeName)}(nil)" : memberName;

            // A fixed-size array member (`[N]T` → golib array<T>) carries a `= new(N)` field
            // initializer that gives the field its Go length; an OMITTED argument arrives as the
            // zero value `default!`, whose backing is null — assigning it unconditionally nulled
            // the initializer's backing, so a composite literal omitting the field (strings'
            // stringFinder{pattern:…, goodSuffixSkip:…}) left its `[256]int` unusable. Keep the
            // initializer for a zero-value argument (Go's zero `[N]T` IS the zeroed backing the
            // initializer produced); a constructed argument assigns as before.
            // A needy VALUE-struct member (a plain field whose OWN zero value is broken — its type
            // carries a promoted-embed box or fixed array at some depth) must be CONSTRUCTED when the
            // argument is omitted: `default(T)` leaves that box null and the first use NREs (io.pipe's
            // `onceError rerr/werr`, whose embedded sync.Mutex box is null → Store/Load `Lock()` NRE on
            // the goroutine, crashing io.Pipe consumers). The parameterless/NilType ctors already build
            // it via AppendZeroValueInitializers; the field-wise ctor is the last gap. The nullable
            // param makes an omitted arg a real `null`, so `?? new T(nil)` reconstructs ONLY when
            // omitted (a caller-supplied value is used as-is — no extra allocation, unchanged reference
            // semantics), exactly mirroring the POINTER-embed `memberValue` handling above.
            // A directional channel field (`<-chan T`/`chan<- T` → channel<T> Cancel = channel<T>.
            // RecvOnly) carries the SAME shape as the fixed-array case one level down: its `=
            // channel<T>.RecvOnly` field initializer already ran before this body, and a nil
            // argument — omitted or explicitly passed, Go has no other spelling for a directional
            // channel's zero value — must leave that stamped nil in place rather than overwrite it
            // with the unstamped `default!` a bare parameter produces. A non-nil argument (a real
            // channel the caller constructed) assigns as before; only the nil case defers to the
            // initializer. See GetChanDirInitializerMembers for why this is scoped to direction
            // stamps alone and never a general field-initializer fallback.
            result.AppendLine(isPromotedStruct ?
                $"{CapturedVarMarker}{GetUnsanitizedIdentifier(memberName)} = {memberValue};" :
                IsFixedArrayMember(typeName, isReferenceType) ?
                    $"if ({memberName}.Source is not null) this.{memberName} = {memberName};" :
                    ChanDirInitializerMembers.Contains(memberName) ?
                        $"if ({memberName} != nil) this.{memberName} = {memberName};" :
                        IsNeedyValueStructMember(typeName, isReferenceType, isPromotedStruct) ?
                            $"this.{memberName} = {memberName} ?? new {typeName}(nil);" :
                            $"this.{memberName} = {memberName};");
        }

        // A MIXED-VISIBILITY struct's PUBLIC subset constructor names only the exported members, so
        // every unexported member is absent from the parameter list AND the body — left at
        // `default(T)`, which is the very state the NilType/parameterless constructors call
        // AppendZeroValueInitializers to avoid. The needy ones (a promoted-embed box, or a field
        // whose own type carries a box/fixed array at some depth) are therefore broken in exactly
        // the instances a cross-package composite literal produces, and only there — which is what
        // made it survive: the same struct built by `@new<T>()`, by the all-fields internal ctor, or
        // from inside its own package is fine, so the defect reads as data-dependent rather than
        // structural. syscall.SockaddrUnix is the measured case: `&SockaddrUnix{Name: path}` left
        // `raw` default, so `raw.Path`'s `[108]int8` backing was zero-length and syscall's own
        // `if n > len(sa.raw.Path)` guard returned EINVAL before bind ever reached the kernel —
        // every AF_UNIX listen/dial on Windows failed with "invalid argument". The omitted members
        // get the same zero-value construction the parameterless ctor gives them; the all-fields
        // internal ctor omits nothing, so this adds nothing there.
        if (structMembers.Count != StructMembers.Count)
        {
            HashSet<string> named = new(structMembers.Select(item => item.memberName), StringComparer.Ordinal);

            AppendZeroValueInitializers(result, StructMembers.Where(item => !named.Contains(item.memberName)));
        }

        result.Append($"{TypeElemIndent}}}");
    }

    // A plain (non-embed) VALUE-struct member whose own zero value is broken — its type needs
    // construction (a promoted-embed box or a fixed-array backing at some depth; see
    // StructTypeNeedsConstruction), so a field-wise-ctor argument left `default` leaves that member
    // unusable. Detected identically to AppendZeroValueInitializers' predicate: not a promoted embed
    // (its box is built by the isPromotedStruct branch), not a reference (a nil pointer/interface IS
    // the correct Go zero), and not a fixed-array member (which self-heals via its own `= new(N)`
    // field initializer). Cross-package/golib types resolve false (no public NilType ctor to call),
    // matching StructTypeNeedsConstruction's conservatism, so no unconstructable `new T(nil)` is emitted.
    private bool IsNeedyValueStructMember(string typeName, bool isReferenceType, bool isPromotedStruct) =>
        !isPromotedStruct && !isReferenceType &&
        !IsFixedArrayMember(typeName, isReferenceType) &&
        StructTypeNeedsConstruction(typeName);

    // The member's type IS golib's fixed-array struct (`global::go.array<…>` / `go.array<…>`) — a
    // PREFIX match, not Contains: a reference member like `ж<array<T>>` (pointer-to-array field,
    // runtime's inlineUnwinder) or a struct member merely instantiated over an array type has no
    // `Source` T[] discriminator and must keep the plain assignment.
    private static bool IsFixedArrayMember(string typeName, bool isReferenceType)
    {
        if (isReferenceType)
            return false;

        string name = typeName.StartsWith("global::", StringComparison.Ordinal) ? typeName["global::".Length..] : typeName;

        return name.StartsWith("go.array<", StringComparison.Ordinal) || name.StartsWith("array<", StringComparison.Ordinal);
    }

    private static string GetToStringImplementation((string typeName, string memberName, bool isReferenceType, bool isPromotedStruct, bool isPublic) item)
    {
        return item.isReferenceType ? $"{item.memberName}?.ToString() ?? \"<nil>\"" : $"{item.memberName}.ToString()";
    }

    private string CompareFields => StructMembers.Count == 0 ? "true /* empty */" :
        HasEqualityOperators || EqualityFallbackMembers is not null ?
            string.Join(" &&\r\n            ", CompareList) :
            "false /* missing equality constraints */";

    // Qualify the left operand with `this.` so a field whose name collides with the `Equals`
    // parameter (`other`) compares the field-to-field, not parameter-to-field. e.g. a struct with
    // a field literally named `other` would otherwise emit `other == other.other` — binding the
    // left `other` to the parameter (CS0019). `this.other == other.other` is unambiguous.
    // A member in EqualityFallbackMembers has no legal == for its type (it depends on an
    // unconstrained type parameter), so it compares via golib's AreEqual — the same routing the
    // converter emits for Go == on any type-parameter operand: EqualityComparer speed on value
    // types but IEEE semantics on floats (EqualityComparer alone reports NaN equal to itself,
    // inverting Go) and typed-null/runtime-type semantics on reference and interface arguments.
    private IEnumerable<string> CompareList => StructMembers.Select(member =>
        EqualityFallbackMembers?.Contains(member.memberName) == true ?
            $"global::go.builtin.AreEqual(this.{member.memberName}, other.{member.memberName})" :
            $"this.{member.memberName} == other.{member.memberName}");

    public string HashCode => StructMembers.Count == 0 ? "base.GetHashCode()" :
        $"""
        global::go.golib.HashCode.Combine(
                    {ParamList})
        """;

    private string ParamList => string.Join(",\r\n            ", StructMembers.Select(member => member.memberName));
}
