// visitStructType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

const StructPrefixMarker = ">>MARKER:STRUCT_%s_PREFIX<<"

// promotedInterfaceForwarder is one `[GoRecv]` extension a DUAL-embed struct owes: a method the
// struct's *T method set obtains ONLY through an embedded-interface field (the pointer-only
// satisfaction arm below). Collected during the embed walk, emitted right after the struct's
// declaration closes.
type promotedInterfaceForwarder struct {
	structName    string
	embedName     string
	method        *types.Func
	valueReceiver bool
}

func (v *Visitor) visitStructType(structType *ast.StructType, identType types.Type, name string, doc *ast.CommentGroup, lifted bool, target *strings.Builder) (structTypeName string) {
	// The struct's OWN type, captured before the embed loop shadows `identType` with each field's —
	// the promoted-pair guard below asks go/types whether the STRUCT implements the embedded
	// interface, and it must ask about the right side.
	declaredStructType := identType

	// Re-entrancy mark: a field's anonymous type can recurse into visitStructType, and each
	// invocation must emit exactly the forwarders its own embed walk collected.
	forwarderMark := len(v.promotedInterfaceForwarders)

	var preLiftIndentLevel int
	var structPrefix *strings.Builder
	var liftedIsPublicized bool

	// Intra-function type declarations are not allowed in C#
	if lifted {
		// A lift can arrive with an EMPTY name — an anonymous struct in a call-argument slot
		// whose parameter is unnamed (builtin `new(struct{ types.Type })`, go/internal/
		// gccgoimporter's reserved). An empty name would declare `partial struct  {` and
		// register "" for every reference to the type — a whole-package syntax cascade. Fall
		// back to the generic "type" the other anonymous-type call sites pass
		// (convStructType/convStarExpr).
		if name == "" {
			name = "type"
		}

		structSignatureType := v.getType(structType, false)

		// Structurally IDENTICAL anonymous struct types are ONE Go type: repeated textual
		// occurrences of `struct{ A Struct }` inside a function must lift to a SINGLE C# type,
		// or reflect.Type identity splits per occurrence (encoding/binary's TestSizeStructCache
		// counts descriptor-cache entries — Go adds ONE for four occurrences). The SAME rule at
		// PACKAGE level: two package vars over one written anonymous struct are one Go type, and
		// splitting them makes their C# types un-unifiable where Go unifies freely —
		// internal/reflectlite's `append(assignableTests, implementsTests...)` could not type
		// (CS9244 + CS8130 ×2), because each var's slice element had lifted to its own nominal
		// struct. A NAMED declaration keeps per-declaration identity and never dedupes. The
		// key's scope discriminator is EXPLICIT ("" at package level) rather than
		// v.currentFuncName, which is never reset after a FuncDecl and would leak the previous
		// function's name onto a package var declared below it. Scopes stay separate (a
		// function-local lift never unifies with a package-level one) and the map is
		// per-visitor, so the residual narrows to the cross-FILE and cross-SCOPE splits.
		var anonLiftKey string

		// The anonymity test falls back to structSignatureType when there is no identType at
		// all. convStructType's no-ident branch — a TYPE ASSERTION's anonymous struct operand,
		// `v0.Interface().(struct{ X, Y int })` — passes identType == nil, so asking only
		// `identType.(*types.Struct)` answered "not anonymous" for the one shape that is
		// anonymous BY CONSTRUCTION (a nil identType means no *types.Object names this type).
		// Both dedupe channels were then skipped and a SECOND name was minted for a type the
		// registry already held: reflect's TestAddr lifted `TestAddr_p` for `var p struct{ X, Y
		// int }` and `TestAddr_type` for the assertion, which Go calls one type and C# would
		// not convert (CS0029). The pointer spelling of the same assertion was always fine —
		// convStarExpr sources a real *types.Struct — which is what localizes this to the one
		// path.
		isAnonStruct := false

		if identType != nil {
			_, isAnonStruct = identType.(*types.Struct)
		} else {
			_, isAnonStruct = structSignatureType.(*types.Struct)
		}

		if isAnonStruct && structSignatureType != nil {
			liftScope := ""

			if v.inFunction {
				liftScope = v.currentFuncName
			}

			anonLiftKey = liftScope + "\x00" + structSignatureType.String()

			if existing, ok := v.liftedAnonStructNames[anonLiftKey]; ok {
				// Never key the map on a nil identType: liftedNameFor is pointer-keyed, so a
				// nil entry would answer for any later site whose getType returns nil.
				if identType != nil {
					v.liftedTypeMap[identType] = existing
				}

				v.liftedTypeMap[structSignatureType] = existing

				return existing
			}
		}

		// The SAME rule one scope wider, and the residual that comment names: a function-local lift
		// of an anonymous struct the PACKAGE has already lifted is a lift of a type that already has
		// a C# name. encoding/xml's read_test.go declares `type Child struct { G struct{ I int } }`
		// — package-level, lifted `Child_G` and registered — and then writes the very same anonymous
		// type as a composite literal inside TestUnmarshalEmptyValues, which minted a SECOND type
		// `TestUnmarshalEmptyValues_type`. Go says those are one type and assigns one to the other;
		// C# saw two structs and refused (CS1503 ×6, the package's only remaining build error and
		// all 386 of its verdicts).
		//
		// The registry is the authority and needs no widening: it is package-scoped (so an
		// unexported field name can only mean this package's, keeping signature equality equivalent
		// to Go type identity) and its key is the full types.String() including field TAGS, which is
		// exactly what Go's own struct identity compares. Reuse is one-directional — a function-local
		// lift adopts a package-level name, never the reverse — so no package-level lift is ever
		// renamed by this and the shared registry keeps its single deterministic winner.
		//
		// The residual that remains is ORDERING, not scope: the package-level declaration must have
		// been visited already for its name to be registered, which is guaranteed within one file
		// (declaration order) and not across files. A cross-file instance still splits, exactly as
		// before.
		//
		// Guarded the same way visitInterfaceType's twin block is: C#'s rule is TYPE accessibility
		// >= MEMBER accessibility, so a reuse is only unsafe when the member this lift names
		// (liftNameNeedsPublicType — the segment after the lift name's last underscore, i.e. the
		// actual field/param Go declared) is EXPORTED. An unexported member never conflicts with
		// any reuse — internal is C#'s accessibility floor — even when the COMBINED lift name
		// reads public by first character alone (comparing combined names, this block's first
		// version, wrongly rejected safe reuses on that basis; see visitInterfaceType's twin
		// comment for the concrete hash_test.go/IfaceKey case that caught it). A FUNCTION-LOCAL
		// lift needs no check: localTypeAccess writes an EXPLICIT `internal` there, overriding
		// name inference, so reusing any name is safe. Falling through to a fresh mint when the
		// check fails is always safe — it only forgoes a dedup opportunity, never breaks one.
		//
		// A THIRD disjunct used to also allow reuse whenever generatedTypeScope(existing) read
		// "public" — trusting the CANDIDATE's own name-based accessibility guess as a stand-in for
		// its real recorded accessibility. That guess cannot see localTypeAccess's override: a
		// package-wide-registered function-local type is ALWAYS internal regardless of what its
		// mangled name's capitalization suggests (localTypeAccess's own doc comment), so the guess
		// reads "public" for any such type whose enclosing function happens to be exported —
		// exactly reflect's `TestTypeFieldOutOfRangePanic_i` (all_test.go, function-local, always
		// internal) reused by `Δtypeᴛ37`'s public field `A` (visiblefields_test.go): CS0052/50/51.
		// 0d6549ae5 widened package-wide registration to call-boundary function-local lifts, which
		// is what first let a name-mangling-inherits-the-enclosing-function registrant reach this
		// check at all — the guess itself was always unsound here, just never exercised against
		// one before. Removed rather than repaired: a truthful check needs the candidate's REAL
		// recorded accessibility, which means seeing past this file's own visit (cross-file,
		// concurrent, no barrier here the way deferredDynamicTypeName's marker path has), and the
		// fallback is unconditionally safe regardless. Measured before removing it (i9's
		// cross-tier census, 2026-09-01): 0 hits in the whole `-stdlib` corpus, 2 in reflect's own
		// `-tests` (both false — guess public, actual internal), 0 in the five reflect-importer
		// canaries' `-tests` — this removal costs no currently-safe dedup anywhere measured.
		if anonLiftKey != "" {
			existing := lookupDynamicTypeName(structSignatureType.String())
			admissible := existing != "" && (v.inFunction || !liftNameNeedsPublicType(name))

			// The reference-model `-tests` sibling of the same-pass check above: a name
			// PRODUCTION already lifted and published via GoDynamicTypeLift for this exact
			// signature (see seedProductionDynamicTypeLifts and visitInterfaceType's twin of
			// this block, added for hash_test.go's IfaceKey/ifaceHash pair). Interfaces needed
			// it to compile at all; structs had no failing case yet, but the gap is the same
			// architectural hole, so closed identically rather than left latent.
			//
			// And it carries the same cross-ASSEMBLY question its interface twin does, for the same
			// reason: the candidate may live in another assembly and this arm emits no declaration
			// at all, so what decides the outcome is the candidate's accessibility THERE. Measured
			// on the interface side (2026-09-01, bisected to 5442b402e — `errors`' external suite
			// adopting production's internal `is_typeᴛ1`, `join_test.cs(49,48): error CS0122:
			// 'errors_package.is_typeᴛ1' is inaccessible due to its protection level`); the struct
			// arm reaches production's registry through the identical path and is gated identically
			// rather than left to produce the same defect under a shape nobody has hit yet. A
			// cross-assembly reuse is admissible only when the reused declaration is REACHABLE from
			// the assembly doing the reusing — see productionLiftReuseReachable; `v.inFunction` and
			// liftNameNeedsPublicType reason within ONE assembly, so they are conjoined with it
			// rather than allowed to escape it. Falling through to a fresh mint is always safe.
			if existing == "" {
				existing = lookupProductionDynamicTypeName(structSignatureType.String())
				admissible = productionLiftReuseReachable(existing, v.options) &&
					(v.inFunction || !liftNameNeedsPublicType(name))
			}

			if admissible {
				if identType != nil {
					v.liftedTypeMap[identType] = existing
				}

				v.liftedTypeMap[structSignatureType] = existing
				v.liftedAnonStructNames[anonLiftKey] = existing

				return existing
			}
		}

		if v.inFunction {
			if target == nil {
				target = &strings.Builder{}
			}

			if !strings.HasPrefix(name, v.currentFuncName+"_") {
				name = fmt.Sprintf("%s_%s", v.currentFuncName, name)
			}

			preLiftIndentLevel = v.indentLevel
			v.indentLevel = 0
		}

		structTypeName = v.getUniqueLiftedTypeName(name)

		// A lift out of a GENERIC function registers the CONSTRUCTED spelling — every use site
		// resolves through liftedTypeMap and must name a bound type (`doBlockingWithCtx_result<T>`),
		// while the DECLARATION below keeps the bare identifier and carries the parameter list
		// separately. The two spellings are the same type; only the declaration may write the
		// parameters as a binding. See localTypeUsedTypeParams for the used-only scoping.
		liftedUseName := structTypeName + liftedTypeParamList(v.localTypeUsedTypeParams(identType))

		if identType != nil {
			v.liftedTypeMap[identType] = liftedUseName
		}

		v.liftedTypeMap[structSignatureType] = liftedUseName

		if anonLiftKey != "" {
			v.liftedAnonStructNames[anonLiftKey] = structTypeName
		}

		// Package-level lifted structs are shared across the package so other files
		// can resolve cross-file references to this anonymous type (an ordinary function-local
		// lift is file/function-scoped and stays out of the shared registry) — EXCEPT a lift at a
		// call boundary (see liftAtCallBoundary's doc comment): its type is externally significant
		// across function scopes the same way a package-level declaration's is, so it publishes
		// too, letting whichever side of a matching signature/call-argument pair is visited first
		// win the name and the second side simply reuse it via the wide lookup below.
		if (!v.inFunction || v.liftAtCallBoundary) && structSignatureType != nil {
			registerDynamicTypeName(structSignatureType.String(), structTypeName)
		}

		// A lifted anonymous struct referenced by a PUBLICIZED interface method (or an exported
		// method/func/delegate) signature must itself be emitted `public`, or it is less accessible
		// than the public member (CS0050/CS0051 — testing's `type corpusEntry = struct{…}` alias
		// lifts to `corpusEntryᴛ1`, referenced by the public `testDeps` fuzzing methods). The lift
		// has no *types.Object, so the publicize pre-pass records the anonymous type itself.
		liftedIsPublicized = isPublicizedLiftedType(identType) || isPublicizedLiftedType(structSignatureType)
	} else {
		structTypeName = name
	}

	if target == nil {
		target = v.outputBuilder

		if !v.inFunction {
			target.WriteString(v.newline)
		}
	}

	structTypeName = getSanitizedIdentifier(structTypeName)
	typeParams, constraints := v.getGenericDefinition(identType)

	// A LIFTED function-local type declares no type parameters of its own — they belong to the
	// enclosing function — so getGenericDefinition returns empty for it and the lift emitted a
	// struct whose fields name unbound parameters: net's `doBlockingWithCtx[T any]` declares
	// `type result struct { res T; err error }`, which became `partial struct
	// doBlockingWithCtx_result` with a `T` field and no `<T>` (CS0246 on the declaration and in
	// every go2cs-gen record over it — the second darwin census's remaining errors).
	//
	// Only the parameters the local type ACTUALLY REFERENCES are threaded (coordinator scoping
	// directive, 2026-08-23): a local type using none lifts exactly as before, which is every
	// existing lift site in the corpus — one that had needed a parameter could not have compiled —
	// so emission stays byte-identical wherever this bug does not bite. Constraints stay empty:
	// the enclosing function's own constraint clause governs the call, and a lifted struct binding
	// `<T>` needs no independent bound to be legal C#.
	if lifted && typeParams == "" {
		typeParams = liftedTypeParamList(v.localTypeUsedTypeParams(identType))
	}

	if len(constraints) == 0 {
		constraints = " "
	} else {
		constraints = fmt.Sprintf("%s%s%s", constraints, v.newline, v.indent(v.indentLevel))
	}

	if !v.inFunction {
		structPrefix = &strings.Builder{}
	}

	structPrefixMarker := fmt.Sprintf(StructPrefixMarker, structTypeName)
	target.WriteString(structPrefixMarker)
	v.writeDocString(target, doc, structType.Pos())

	var dynamic string

	if lifted {
		dynamic = "(\"dyn\")"
	}

	// A lifted function-local NAMED type carries its original Go name so the reflection
	// bridge's %T / Type.String() prints Go's `binary.Person`, never the function-prefixed
	// lifted identifier (encoding/binary's TestNoFixedSize asserts the exact error text). A
	// separate attribute, never a [GoType] definition token — the TypeGenerator matches the
	// definition slot by exact string (I2.R R-8). Anonymous lifts have no Go name to stamp.
	var localNameAttr string

	if lifted && v.inFunction {
		if named, ok := identType.(*types.Named); ok {
			localNameAttr = fmt.Sprintf("[GoLocalName(\"%s\")] ", named.Obj().Name())
		}
	}

	// Consume any pending publicized-type access modifier (an unexported type used as an
	// exported field). Only the top-level type declaration carries it; nested/anonymous lifts do
	// not, so read and clear before visiting fields (which may recurse into this function).
	access := v.pendingTypeAccess
	v.pendingTypeAccess = ""

	// A lifted anonymous type carries no pendingTypeAccess (only a top-level TypeSpec sets it), so a
	// lift reached through a public surface is publicized here instead (see liftedIsPublicized).
	if liftedIsPublicized && access == "" {
		access = "public "
	}

	// A FUNCTION-LOCAL lift that is neither publicized nor carrying a TypeSpec's modifier would be
	// emitted BARE, leaving go2cs-gen to scope it from the hoisted `<Func>_<name>` identifier — whose
	// leading case belongs to the enclosing function, not to a type Go ever made visible. Pin it
	// internal instead, so the local declarations of one function share one accessibility
	// (localTypeAccess).
	if access == "" {
		access = v.localTypeAccess()
	}

	// A struct carrying FIXED-SIZE ARRAY fields (directly, or through another such struct) is not
	// completely copied by a plain C# struct assignment — `array<T>` is a struct over a shared T[]
	// backing, so the copy's array writes reach back into the source. Name those fields for
	// go2cs-gen, which generates the struct's IGoValueClone `Clone()`; every Go by-value copy site
	// appends it (typeNeedsValueClone / arrayCloneOperations.go). A struct that needs nothing is
	// unstamped and unchanged.
	var valueCloneAttr string

	if cloneFields := structValueCloneFields(identType); len(cloneFields) > 0 {
		quotedFields := make([]string, len(cloneFields))

		for i, fieldName := range cloneFields {
			quotedFields[i] = fmt.Sprintf("%q", fieldName)
		}

		valueCloneAttr = fmt.Sprintf("[GoValueClone(%s)] ", strings.Join(quotedFields, ", "))
	}

	// Both stamps are MOVABLE: their consumers read them off the TYPE (go2cs-gen resolves the
	// symbol's declarations, golib's reflection bridge reads the runtime Type), and C# unions the
	// attributes of every partial declaration — so they belong on the package_info.cs accessibility
	// record, out of the reader's way, and the `[GoType]` declaration keeps only what identifies it.
	inlineAttrs := v.recordTypeAccessibility("struct", structTypeName, typeParams, access, localNameAttr+valueCloneAttr)

	// A struct carrying a ZERO-SIZE Go field is LARGER in C# than in Go unless it is laid out
	// explicitly at Go's own offsets — see zeroSizeFieldLayout.go for why that matters (Reinterpret's
	// size guard) and for the two limits that bound it (unmanaged only; zero-size fields readonly).
	// A struct that needs nothing, or cannot take it, is unstamped and emitted exactly as before.
	zeroSizeLayout, hasZeroSizeLayout := v.structZeroSizeLayout(v.underlyingStruct(identType), identType)
	var structLayoutAttr string

	if hasZeroSizeLayout {
		structLayoutAttr = zeroSizeLayout.structLayoutAttribute()
		v.addRequiredUsing("System.Runtime.InteropServices")
	}

	v.writeStringLn(target, "[GoType%s] %s%s%spartial struct %s%s%s{", dynamic, inlineAttrs, structLayoutAttr, access, structTypeName, typeParams, constraints)
	v.indentLevel++

	var prevNameDiscardedCount int

	// The FLAT go/types field index, which is what indexes the layout's offsets — the AST walks
	// GROUPS (`a, b int` is one group, two fields), so the two orders only agree if this advances
	// once per NAME rather than once per group.
	var layoutFieldIndex int

	for _, field := range structType.Fields.List {
		v.writeDocString(target, field.Doc, field.Pos())

		if field.Tag != nil {
			v.writeString(target, "[GoTag(")
			target.WriteString(v.convBasicLit(field.Tag, BasicLitContext{u8StringOK: false, spanTargetUnsupported: true}))
			target.WriteString(")]")
			target.WriteString(v.newline)
		}

		// The array dims this field's type reaches through a hop no zero instance can measure — a
		// POINTER's pointee, a MAP's key or element. Every name in a Go field group shares
		// field.Type, so one attribute line covers the whole group (see fieldDimsCargo.go).
		if dimsAttributes := emitFieldDimsAttributes(v.getType(field.Type, false)); dimsAttributes != "" {
			v.writeString(target, "%s", dimsAttributes)
			target.WriteString(v.newline)
		}

		// The DESCRIPTOR CARRIER for a field whose Go type is a defined-over-interface type the
		// emission erased to a `using` alias: the field's C# type is `object` (or the target
		// interface), which carries no Go name, so reflect.Type.Field(i).Type.Name() answered ""
		// where Go answers the declared name. Same rule and same reason as the dims cargo directly
		// above — the datum cannot live in the managed type, so it travels as a stamp. Only the
		// field's OWN type is stamped here; what Elem()/Key() hand down needs the carrier on the
		// DESCRIPTOR rather than at the access, which is a descriptor-shape change sequenced after
		// this one.
		if carrier := v.descriptorCarrierFor(v.getType(field.Type, false)); carrier != "" {
			v.writeString(target, "[GoDescriptorType(Self = typeof(%s))]", carrier)
			target.WriteString(v.newline)
		}

		var indentOffset int

		if v.inFunction {
			indentOffset = 1
		} else {
			indentOffset = -1
		}

		// Lift the anonymous struct/interface the FIELD's declared type reaches, at ANY depth of its
		// composition — `struct{…}`, `*struct{…}` and `[N]struct{…}`, and equally the composed
		// `[N]*struct{…}`, `[]*struct{…}`, `map[K]struct{…}` and `chan struct{…}` — so the field
		// declaration resolves to a named type (`array<Composed_Ptrs>`) instead of the raw,
		// un-compilable Go `struct{…}` text. This arm used to peel the field type BY HAND, one
		// container level per kind, which is the same one-level shallowness that produced net's
		// CS1031 cascade at the declaration sites; it now shares those sites' recursive descent
		// (extractStructType / extractInterfaceType, both of which already exclude the empty
		// struct/interface — an empty `interface{}` field must map to `any`, never to a marker
		// interface nothing implements). getAliasQualifiedTypeName resolves each composed element through
		// liftedTypeMap. Struct is probed first, matching the previous arm order.
		//
		// A field type carrying an anonymous literal always NAMES the field (an embedded field is a
		// type name by the Go spec, never a literal), so the name guard only skips cases that cannot
		// arise — it is what lets the lift name stay `<struct>_<field>` for every shape.
		if len(field.Names) > 0 {
			if subStructType, subStructIdentType := v.extractStructType(field.Type); subStructType != nil && !v.liftedTypeExists(subStructType) {
				v.indentLevel += indentOffset
				v.visitStructType(subStructType, subStructIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true, structPrefix)
				v.indentLevel -= indentOffset

				if structPrefix != nil {
					structPrefix.WriteString(v.newline)
				}

				// Sub-struct tracking (addImplicitSubStructConversions) describes the field's OWN
				// declared type, so it records the two DIRECT shapes it has always recorded: the
				// field IS the anonymous struct, or a pointer straight to it. A struct reached
				// through a slice/array/map/chan element is not the field's type and is not tracked.
				var trackedType types.Type

				if _, isDirect := field.Type.(*ast.StructType); isDirect {
					trackedType = subStructIdentType
				} else if ptrType, isPointer := field.Type.(*ast.StarExpr); isPointer && ptrType.X == ast.Expr(subStructType) {
					trackedType = v.getExprType(ptrType)
				}

				if trackedType != nil {
					v.subStructTypes[identType] = append(v.subStructTypes[identType], trackedType)
				}
			} else if interfaceType, interfaceIdentType := v.extractInterfaceType(field.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
				v.indentLevel += indentOffset
				v.visitInterfaceType(interfaceType, interfaceIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true, structPrefix)
				v.indentLevel -= indentOffset

				if structPrefix != nil {
					structPrefix.WriteString(v.newline)
				}
			}
		}

		fieldType := v.getType(field.Type, false)
		goTypeName := v.getAliasQualifiedTypeName(fieldType, false)
		goFullTypeName := v.getFullyQualifiedTypeName(fieldType, false)
		csFullTypeName := convertToCSTypeName(goFullTypeName)

		// The fully-qualified form for emission INTO this source file's body. csFullTypeName is a
		// RELATIVE dotted name (`io.fs_package.FS`); when its leading segment is also imported as a
		// package alias in this file (`using io = io_package;`) C# binds it to that TYPE alias, so the
		// name resolves to the nonexistent nested type `io_package.fs_package.FS` (CS0426). Root-qualify
		// (`go.io.fs_package.FS`) so the leading segment resolves as the child NAMESPACE it names. The
		// unqualified csFullTypeName is kept below as the promotedInterfaceImplementations map KEY, which
		// feeds generator-consumed strings that live in alias-less files (where the relative form
		// resolves and the key must stay stable).
		csEmitTypeName := rootQualifyIfAmbiguous(csFullTypeName)

		// For the actual NAMED-field declaration, prefer the readable file-local package alias
		// (`atomic.Int32` over `sync.atomic_package.Int32`) when this file imports the type's
		// package — keeping the emitted field visually close to the Go source. The fully-qualified
		// csFullTypeName is retained for promotion/interface registration below, which feeds
		// generator-consumed strings that live in alias-less files. (Embedded fields keep the full
		// form for their promoted accessors; only the named-field branch uses the display name.)
		goDisplayTypeName := v.getScopeCheckedTypeName(fieldType)
		csDisplayTypeName := convertToCSTypeName(goDisplayTypeName)

		// A func-typed field whose signature names a type from a MULTI-SEGMENT import path
		// (`Values func([]reflect.Value, *rand.Rand)`, where `rand` is `math/rand`) must be
		// rendered structurally as an Action/Func delegate via getCSharpTypeName. The string-based
		// getAliasQualifiedTypeName/convertToCSTypeName path stringifies the signature as
		// `func([]reflect.Value, *math/rand.Rand)` and then feeds the slash-bearing import path to
		// convertImportPathToNamespace, which splits on '/' and emits the dotted `math.rand.Rand` —
		// but `math` aliases to `math_package`, so `math.rand` resolves to the non-existent
		// `math_package.rand` (CS0426). getCSharpTypeName recurses through the signature per element,
		// qualifying each named type by its package NAME (`rand.Rand`), the alias the file imports.
		//
		// A VARIADIC func-typed field reroutes too: the string path cannot render a variadic
		// signature at all — getAliasQualifiedTypeName's '..' strip reduces the ellipsis of
		// `JoinPath func(elem ...string) string` (go/build's Context) to `.string`, emitting the
		// unparseable `Func<.@string, @string>` (CS1031 + CS1003 ×2), and even unstripped it has
		// no variadic lowering. Structurally the field renders the golib variadic delegate family
		// (`Funcꓸꓸꓸ<@string, @string>` — see iifeDelegateType), which loose-arg, empty and spread
		// calls through the field all bind.
		//
		// Every other signature keeps the display path: a func field with no cross-package import —
		// `func(string) (importPath string, ok bool)` — preserves its named tuple elements
		// (structural rendering drops them). Compiling correctness for the broken cases is worth
		// the lost tuple names in the rare rerouted field.
		if sig, isSignature := fieldType.(*types.Signature); isSignature && (sig.Variadic() || strings.Contains(goDisplayTypeName, "/")) {
			csDisplayTypeName = v.getCSharpTypeName(fieldType)
		}

		displayLenDeviation := token.Pos(len(csDisplayTypeName) - len(goDisplayTypeName))
		typeLenDeviation := token.Pos(len(csFullTypeName) - len(goFullTypeName))

		// The Go ZERO of a field whose managed default is not already it: a fixed-size array's
		// length and a directional channel's direction are both parts of the Go TYPE that the
		// emitted C# type cannot hold, so both are carried as an initializer the generated
		// parameterless constructor runs. At most one applies to any field.
		var fieldInitializer string

		// Unparen: `x ([32]int32)` declares the same fixed-size array field as `x [32]int32`
		// (Go's grammar admits the parentheses; reflectlite's typeTests writes every entry that
		// way), and missing the wrapper silently dropped the `= new(N)` initializer — the zero
		// instance then carried a backing-less array and every dims read answered [0]N.
		if arrayType, ok := ast.Unparen(field.Type).(*ast.ArrayType); ok {
			if arrayType.Len != nil {
				lengthExpr := v.convExpr(arrayType.Len, nil)

				// A LIFTED struct (an anonymous type pulled out of its enclosing function to a
				// top-level declaration) can no longer see that function's own locals, so an array
				// length spelled as a bare identifier reference (`const n = ...; y [n]byte`) is not
				// just unresolvable -- it can silently rebind to an unrelated same-named C# member
				// elsewhere in the shared partial-class package (runtime's gc_test.go `const n`
				// inside TestHugeGCInfo bound to malloc_test.go's unrelated package-level `var n =
				// flag.Int(...)`, CS1503 on the type mismatch; a same-typed collision would have
				// compiled and silently used the WRONG value). Go's own grammar guarantees an array
				// length is always a compile-time constant, so folding it to a literal is never
				// wrong for a NON-lifted field either -- narrowed to lifted alone as the surgical
				// fix for the shape that actually breaks, leaving the general expression path
				// unchanged everywhere else.
				if lifted {
					if tv, ok := v.info.Types[arrayType.Len]; ok && tv.Value != nil {
						if folded, ok := constArrayLength(tv.Value); ok {
							lengthExpr = folded
						}
					}
				}

				fieldInitializer = fmt.Sprintf(" = new(%s)", v.arrayZeroValueArgs(lengthExpr, fieldType))
			}
		}

		// A DIRECTIONAL channel FIELD carries its direction the same way an array field carries its
		// length — as an initializer the generated parameterless constructor runs, which is where
		// GoReflect.FieldChanDir reads it back off a cached zero instance. This is the position
		// reflectlite's `struct{ x chan<- string }` row reads through Field(0).Type(): there is no
		// channel VALUE to measure and no attribute to consult, only the zero the field declares.
		if chanInitializer := v.chanDirNilValue(fieldType); chanInitializer != "" {
			fieldInitializer = " = " + chanInitializer
		}

		if field.Names == nil {
			// Check for promoted fields
			var ident *ast.Ident
			var ok bool

			var isIdentFieldType bool
			var selectorType bool

			// A GENERIC embed (`node[K, V]` — internal/concurrent's entry) arrives as an
			// IndexExpr/IndexListExpr over the base type expression; unwrap it (in both the
			// plain and pointer forms below) so the embed emits — it was silently DROPPED
			// (the struct lost the field entirely, every promoted access CS0117).
			unwrapGeneric := func(expr ast.Expr) ast.Expr {
				switch index := expr.(type) {
				case *ast.IndexExpr:
					return index.X
				case *ast.IndexListExpr:
					return index.X
				}

				return expr
			}

			if ident, ok = unwrapGeneric(field.Type).(*ast.Ident); ok {
				isIdentFieldType = true
			} else if ptrType, ok := field.Type.(*ast.StarExpr); ok {
				if ident, ok = unwrapGeneric(ptrType.X).(*ast.Ident); ok {
					isIdentFieldType = true
				}
			}

			if !isIdentFieldType {
				if selectorExpr, ok := unwrapGeneric(field.Type).(*ast.SelectorExpr); ok {
					if ident, ok = selectorExpr.X.(*ast.Ident); ok {
						isIdentFieldType = true
						selectorType = true
					}
				} else if ptrType, ok := field.Type.(*ast.StarExpr); ok {
					if selectorExpr, ok := unwrapGeneric(ptrType.X).(*ast.SelectorExpr); ok {
						if ident, ok = selectorExpr.X.(*ast.Ident); ok {
							isIdentFieldType = true
							selectorType = true
						}
					}
				}
			}

			if !isIdentFieldType {
				continue
			}

			// A generic embed's MEMBER NAME is the base type name (Go promotes entry[K,V]'s
			// embedded node[K,V] through the selector `.node`), so strip the type arguments —
			// and do it BEFORE the selector dot-strip: the arguments may contain qualified
			// types whose dots otherwise win the LastIndex (uniqueMap's
			// `*concurrent.HashTrieMap[T, weak.Pointer[T]]` named its member `Pointer`).
			if bracketIndex := strings.Index(goTypeName, "["); bracketIndex != -1 {
				goTypeName = goTypeName[:bracketIndex]
			}

			// An embedded field's NAME is the UNQUALIFIED type name (Go spec), so strip any package
			// qualifier. A selector embed (`io.Writer`) carries it explicitly; a DOT-IMPORTED ident
			// embed does too once resolved — io_test's `import . "io"` + embedded `ReaderFrom` reaches
			// here as a bare *ast.Ident whose getAliasQualifiedTypeName still renders the (collision-renamed)
			// package qualifier `Δio.ReaderFrom`. Gating the strip on selectorType left that qualifier
			// in the field name (`Δio.ReaderFrom`), whose dot is a C# syntax error (CS1003/CS1026).
			// Strip whenever a qualifier survives, covering both forms; a same-package embed has no
			// dot, so this is a no-op there (byte-identical).
			if dotIndex := strings.LastIndex(goTypeName, "."); dotIndex != -1 {
				// Get the unqualified name of the embedded type
				goTypeName = goTypeName[dotIndex+1:]
			}

			// Lookup identity to determine if it's an interface — for a SELECTOR embed
			// (io.Writer) resolve the SEL, not the package ident: a cross-package
			// INTERFACE embed otherwise took the promoted-STRUCT property form, and the
			// generator tried to construct the interface (archive/tar's lifted
			// `struct{ io.Writer }`, CS0144 ×8 + CS1929 ×4).
			identObj := v.info.ObjectOf(ident)

			if selectorType {
				if selectorExpr, ok := unwrapGeneric(field.Type).(*ast.SelectorExpr); ok {
					identObj = v.info.ObjectOf(selectorExpr.Sel)
				} else if ptrType, ok := field.Type.(*ast.StarExpr); ok {
					if selectorExpr, ok := unwrapGeneric(ptrType.X).(*ast.SelectorExpr); ok {
						identObj = v.info.ObjectOf(selectorExpr.Sel)
					}
				}
			}

			if identObj == nil {
				continue // Could not find the object of ident
			}

			identType := identObj.Type().Underlying()

			// An EMBEDDED field is NAMED BY GO, not by the C# rendering of its type. The two coincide
			// for every ordinary embed — which is why the rendered name served for so long — but they
			// part company the moment the converter RENAMES the type: a function-local `type myInt int`
			// is hoisted to package scope as `TestAnonymousFields_myIntᴛ1`, and naming the member after
			// that left the declaration (and go2cs-gen's constructor and promotion, both read off it)
			// spelling one thing while every use site spelled the Go field name — `s.myInt` (CS1061) and
			// `new S3(embed1: …)` (CS1739) in encoding/json's suite. The Go object's OWN name is the
			// field name by definition (Go spec: an embedded field's name is the unqualified type name),
			// so it is authoritative where the rendered string is derived — and it settles the field's
			// EXPORTEDNESS too, which the hoisted name silently flipped (`embed1` is unexported; the
			// `TestUnmarshalEmbeddedUnexported_` prefix made the member public, the opposite of what the
			// test asserts about it). The bracket/dot stripping above is what this supersedes: a
			// generic embed's object is already the base type, and a qualified one is already
			// unqualified, so the stripping is left in place only for the unclaimed PkgName arm.
			switch identObj.(type) {
			case *types.Var, *types.TypeName:
				// A same-package embed resolves to the FIELD itself (*types.Var, whose name IS
				// the Go field name); a SELECTOR embed was re-resolved above to the embedded
				// type's own TypeName, which the spec makes the field name too. A *types.PkgName
				// — an unresolved selector — is deliberately not claimed: it would name the
				// member after the package.
				if name := identObj.Name(); name != "" {
					goTypeName = name
				}
			}

			// An EMBEDDED field's member name is the unqualified type name (Go spec), so it can
			// equal the ENCLOSING struct's own name — io_test.go's `type Buffer struct{
			// bytes.Buffer }` derives the member `Buffer` inside struct `Buffer`, which C# forbids
			// (CS0542). Apply the same disambiguation marker the NAMED-field path below uses: the
			// ACCESS sites already emit the renamed form (structFieldBoxName / convIdent run
			// typeCollidingFieldName for any field whose name equals its enclosing type, embedded
			// or not — `rb.of(Buffer.ᏑΔBuffer)`), so only this declaration was out of step.
			// Both sides compare RAW, mirroring the named-field compare.
			embedName := getCoreSanitizedIdentifier(goTypeName)

			if strings.TrimPrefix(embedName, "@") == strings.TrimPrefix(strings.TrimPrefix(structTypeName, ShadowVarMarker), "@") {
				embedName = typeCollidingFieldName(embedName)
			}

			if ifaceType, ok := identType.(*types.Interface); ok {
				// Record the promoted pair ONLY when Go itself says the struct implements the
				// embedded interface — the samePackageImplements doctrine ("record what Go already
				// says is true") applied at the embed. An embedded interface CONTRIBUTES its
				// methods, but Go's promotion rule REMOVES a method with more than one equal-depth
				// provider, and a Go program can lean on that removal deliberately: io_test.go's
				// `Buffer` embeds `bytes.Buffer` AND the `ReaderFrom`/`WriterTo` interfaces
				// precisely so ReadFrom/WriteTo drop out of the method set and `io.Copy` cannot
				// take its fast paths — the interface fields stay nil by design. The unconditional
				// record re-asserted the pair anyway; go2cs-gen then faithfully amplified it into
				// a conformance member and a method-set twin, and the method Go had DELETED came
				// back at runtime, forwarding to the nil field (JOB-010 Shape C: eight io tests
				// nil-panicking inside a shell that should not exist).
				//
				// The VALUE form is what the record claims (the generated `partial struct T :
				// iface` makes the value satisfy in C#), so the value method set is what is
				// checked; a pair only *T satisfies is samePackageImplements' business, whose
				// pointer-form records carry their own realizability gates. Type-parameter-carrying
				// structs keep the old unconditional behavior — types.Implements is undefined over
				// uninstantiated generics, the same exclusion convertToInterfaceType makes.
				if typeContainsTypeParams(declaredStructType) || types.Implements(declaredStructType, ifaceType) {
					packageLock.Lock()

					if promotions, exists := promotedInterfaceImplementations[csFullTypeName]; exists {
						promotions.Add(structTypeName)
					} else {
						promotedInterfaceImplementations[csFullTypeName] = NewHashSet([]string{structTypeName})
					}

					packageLock.Unlock()
				} else if types.Implements(types.NewPointer(declaredStructType), ifaceType) {
					// POINTER-ONLY satisfaction through a DUAL embed — the fakeDNSPacketConn shape
					// (net's dnsclient_unix_test.go:966): the struct embeds this interface AND a
					// struct, and an explicit pointer-receiver override (or a pointer-receiver
					// collision resolver) shadows the interface-promoted method OUT of the VALUE
					// method set, so the value-form check above is FALSE while *T implements. The
					// value-form Promoted record would be an OVER-CLAIM here — a VALUE stored into
					// `any` would assert true where Go says false (measured on the three-arm probe,
					// 2026-09-02) — so this arm mints the POINTER-form record instead, and emits a
					// `[GoRecv]` forwarder below for each method only the interface field provides,
					// which is what lets go2cs-gen's existing pointer-record adapter compose (under
					// the record alone the ImplementGenerator emitted NOTHING for the pair, silently
					// — the missing extension surface was why). Dispatch then matches Go: the box
					// satisfies, the value refuses, the explicit override wins, the field forwards
					// the rest. Guarded by tests/Behavioral/EmbeddedInterfaceWitness's dual-embed
					// arms; the reached corpus consumer is net's Linux DNS-client suite (35 verdicts
					// behind `c.(PacketConn)` selecting the STREAM arm on a UDP conn).
					//
					// KNOWN RESIDUAL, stated not silently narrowed: this arm walks DIRECT embeds, so
					// pointer-only satisfaction whose interface methods arrive at depth >= 2 (a
					// struct embed whose OWN embedded interface supplies them) mints nothing here —
					// no corpus consumer reaches that shape today, and the census that says so is
					// the two-seeded diff this cut banks with.
					pointerRecordName := PointerPrefix + "<" + structTypeName + ">"

					packageLock.Lock()

					if implementations, exists := interfaceImplementations[csFullTypeName]; exists {
						implementations.Add(pointerRecordName)
					} else {
						interfaceImplementations[csFullTypeName] = NewHashSet([]string{pointerRecordName})
					}

					packageLock.Unlock()

					for i := range ifaceType.NumMethods() {
						method := ifaceType.Method(i)

						// The forwarder exists only for a method whose *T-method-set provider IS
						// the interface promotion through this embed: an explicit method or a
						// struct-embed promotion already has its extension, and an equal-depth
						// collision resolves to nil (Go removed it; emitting would resurrect it —
						// the JOB-010 direction).
						provider, _, _ := types.LookupFieldOrMethod(declaredStructType, true, v.pkg, method.Name())

						providerFunc, isFunc := provider.(*types.Func)

						if !isFunc || providerFunc.Type().(*types.Signature).Recv() == nil {
							continue
						}

						if _, recvIsIface := providerFunc.Type().(*types.Signature).Recv().Type().Underlying().(*types.Interface); !recvIsIface {
							continue
						}

						if sig, ok := method.Type().(*types.Signature); ok && sig.Variadic() {
							// Not reached by any current corpus consumer; a variadic forwarder needs
							// the params-array spelling the adapter matcher expects, which this arm
							// does not synthesize. Loud, so a future consumer is a report, not a
							// silent method-set hole.
							showWarning("embedded-interface promoted method %s.%s is variadic - pointer-form forwarder not emitted", structTypeName, method.Name())
							continue
						}

						// An interface-promoted method that ALSO survives in the VALUE method set
						// (nothing shadows it — pointerOnly's Read, not its Write) is a value
						// method in Go, so its forwarder takes the value-receiver extension form
						// the generator's own Promoted path emits (no [GoRecv], no ref) — which is
						// what keeps reflect's NumMethod arithmetic matching Go on BOTH forms
						// (measured: ptr 2 / val 1 in Go; the all-ref first cut read val 0).
						valueReceiver := false
						valueMethodSet := types.NewMethodSet(declaredStructType)

						for j := range valueMethodSet.Len() {
							if valueMethodSet.At(j).Obj() == providerFunc {
								valueReceiver = true
								break
							}
						}

						v.promotedInterfaceForwarders = append(v.promotedInterfaceForwarders, promotedInterfaceForwarder{
							structName:    structTypeName,
							embedName:     embedName,
							method:        method,
							valueReceiver: valueReceiver,
						})
					}
				}

				v.writeString(target, "%s %s %s;", getAccess(goTypeName), csEmitTypeName, embedName)
			} else {
				var handled bool

				if _, ok := identObj.(*types.PkgName); !ok {
					if ptrType, ok := identType.(*types.Pointer); ok {
						if _, ok = ptrType.Elem().(*types.Named); !ok {
							// An embedded pointer to a PREDECLARED type has nothing to promote and is a plain field;
							// the [GoEmbedded] stamp is what lets the reflection projection report it Anonymous
							// (a field named after its type is otherwise indistinguishable from an embed).
							v.writeString(target, "[GoEmbedded] %s %s %s;", getAccess(goTypeName), csEmitTypeName, embedName)
							handled = true
						}
					} else if _, ok = identType.(*types.Struct); !ok {
						if _, ok := identObj.Type().(*types.Named); !ok {
							// An embedded PREDECLARED type (`struct{ int }`): the same plain-field emission, stamped.
							v.writeString(target, "[GoEmbedded] %s %s %s;", getAccess(goTypeName), csEmitTypeName, embedName)
							handled = true
						}
					}
				}

				// Handle promoted struct implementations
				if !handled {
					v.writeString(target, "%s partial ref %s %s { get; }", getAccess(goTypeName), csEmitTypeName, embedName)
				}
			}

			v.writeCommentString(target, field.Comment, field.Type.End()+typeLenDeviation)
			target.WriteString(v.newline)
		} else {
			// Match the Go source's line grouping for readability: when a single Go field
			// declaration groups multiple names (`x, y int`), emit one combined C# line
			// (`internal nint x, y;`). This is only safe when every name shares the same
			// access modifier and emitted type and none needs per-name special handling —
			// blank `_` (renamed per occurrence), a name colliding with the struct type
			// (Δ-marker rename), or a per-field array initializer (` = new(N)`). The names in
			// one field group already share field.Type/Tag/Comment, so only access and the
			// per-name renames can diverge. When any apply, fall back to one line per name.
			canCombine := len(field.Names) > 1 && fieldInitializer == ""

			if canCombine {
				groupAccess := getAccess(field.Names[0].Name)

				for _, ident := range field.Names {
					fieldName := getCoreSanitizedIdentifier(ident.Name)

					if fieldName == "_" || fieldName == structTypeName || getAccess(ident.Name) != groupAccess {
						canCombine = false
						break
					}
				}
			}

			// Explicit layout stamps each field with its OWN offset, so a combined `a, b int`
			// declaration — which can carry only one attribute list — cannot express it.
			if hasZeroSizeLayout {
				canCombine = false
			}

			if canCombine {
				fieldNames := make([]string, len(field.Names))

				for i, ident := range field.Names {
					fieldNames[i] = getCoreSanitizedIdentifier(ident.Name)
				}

				layoutFieldIndex += len(field.Names)

				v.writeString(target, "%s %s %s;", getAccess(field.Names[0].Name), csDisplayTypeName, strings.Join(fieldNames, ", "))
				v.writeCommentString(target, field.Comment, field.Type.End()+displayLenDeviation)
				target.WriteString(v.newline)
			} else {
				for _, ident := range field.Names {
					fieldName := getCoreSanitizedIdentifier(ident.Name)

					if fieldName == "_" {
						for range prevNameDiscardedCount {
							fieldName = fieldName + "_"
						}

						prevNameDiscardedCount++
					} else if strings.TrimPrefix(fieldName, "@") == strings.TrimPrefix(strings.TrimPrefix(structTypeName, ShadowVarMarker), "@") {
						// C# forbids a member sharing its enclosing type's name (CS0542), so rename a
						// field whose name equals the struct type with the disambiguation marker. Field
						// accesses are renamed to match (see convSelectorExpr / convIdent). Both sides
						// compare RAW (escape/rename markers stripped): net parse.go's `type file
						// struct{ file *os.File }` renames the TYPE to Δfile (CS9056) and escapes the
						// FIELD to @file — the literal compare missed, declaring `@file` while every
						// access site emitted `Δfile` (CS1061 ×3).
						fieldName = typeCollidingFieldName(fieldName)
					}

					// Under explicit layout the field carries its Go offset, and a ZERO-SIZE field is
					// additionally readonly: it shares its offset with the field Go puts there, and a
					// C# write to it would put its one byte over that neighbour (Go's writes nothing).
					var offsetAttr, readOnly string

					if hasZeroSizeLayout {
						offsetAttr = zeroSizeLayout.fieldOffsetAttribute(layoutFieldIndex)

						if zeroSizeLayout.fieldIsZeroSize(layoutFieldIndex) {
							readOnly = "readonly "
						}
					}

					layoutFieldIndex++

					v.writeString(target, "%s%s %s%s %s%s;", offsetAttr, getAccess(ident.Name), readOnly, csDisplayTypeName, fieldName, fieldInitializer)
					v.writeCommentString(target, field.Comment, field.Type.End()+displayLenDeviation)
					target.WriteString(v.newline)
				}
			}
		}
	}

	v.indentLevel--
	v.writeStringLn(target, "}")

	v.emitPromotedInterfaceForwarders(target, forwarderMark)

	if structPrefix == nil {
		v.replaceMarkerString(target, structPrefixMarker, "")
	} else {
		v.replaceMarkerString(target, structPrefixMarker, structPrefix.String())
	}

	if lifted && v.inFunction {
		if v.currentFuncPrefix.Len() > 0 {
			v.currentFuncPrefix.WriteString(v.newline)
		}

		v.currentFuncPrefix.WriteString(target.String())
		target.Reset()
		v.indentLevel = preLiftIndentLevel
	}

	return
}

// structHasPromotedEmbeds reports whether the type's underlying struct carries at least one
// embedded field that the generated C# stores in a constructor-initialized readonly `ж<T>` box
// (the StructTypeTemplate "Promoted Struct References"). A `default`-valued instance of such a
// struct has null boxes, so the first promoted-member access throws NullReferenceException —
// an uninitialized declaration must render `new T(nil)` instead of `default!`. The decision
// mirrors the embedded-field emission above: an embed renders as a `partial ref` promotion
// (and thus a box) unless it is a same-package interface, a builtin non-named embed (`int`),
// or a pointer to a non-named type; a CROSS-PACKAGE embed always takes the promotion path
// (the selector-type branch above bypasses every plain-field case, interfaces included).
func (v *Visitor) structHasPromotedEmbeds(t types.Type) bool {
	if t == nil {
		return false
	}

	st, ok := t.Underlying().(*types.Struct)

	if !ok {
		return false
	}

	for i := range st.NumFields() {
		field := st.Field(i)

		if !field.Anonymous() {
			continue
		}

		fieldType := field.Type()

		// Resolve the embed's named type, through one syntactic pointer (`*X`).
		named, _ := types.Unalias(fieldType).(*types.Named)

		if named == nil {
			if ptr, isPtr := fieldType.(*types.Pointer); isPtr {
				named, _ = types.Unalias(ptr.Elem()).(*types.Named)
			}
		}

		// A cross-package embed always renders as a promoted box.
		if named != nil && named.Obj().Pkg() != nil && named.Obj().Pkg() != v.pkg {
			return true
		}

		// Same-package `*X` embed: any named pointee promotes (struct underlying and named
		// non-struct both take the partial-ref path); `*int` (builtin pointee) stays plain.
		if ptr, isPtr := fieldType.(*types.Pointer); isPtr {
			if _, isNamed := types.Unalias(ptr.Elem()).(*types.Named); isNamed {
				return true
			}

			continue
		}

		underlying := fieldType.Underlying()

		// A same-package interface embed renders as a plain interface field — no box.
		if _, isInterface := underlying.(*types.Interface); isInterface {
			continue
		}

		// A named-pointer-type embed (`type P *T`) promotes only when the pointee is named.
		if ptr, isPtr := underlying.(*types.Pointer); isPtr {
			if _, isNamed := types.Unalias(ptr.Elem()).(*types.Named); isNamed {
				return true
			}

			continue
		}

		// A value embed promotes when its underlying is a struct or the embed itself is a
		// named type (`type RCode int` embeds as a partial-ref box despite the basic core).
		if _, isStruct := underlying.(*types.Struct); isStruct {
			return true
		}

		if named != nil {
			return true
		}
	}

	return false
}

// structZeroValueNeedsConstruction reports whether a struct type's zero value default(T) is
// BROKEN — it has a promoted-embed box (constructor-allocated) or a fixed-size array field
// (`= new(N)` field initializer that default(T) skips), directly or through a nested value-struct
// field — so `var z T` must run the generated parameterless constructor (`new()`) rather than
// emit `default!`. Mirrors go2cs-gen StructTypeTemplate.NeedsConstruction; a false result keeps the
// existing `default!`/bare emission. The top-level promoted-embed case is routed to `new(nil)` by
// the caller's earlier structHasPromotedEmbeds check — this predicate still recurses for it so a
// NESTED field whose own type carries a promoted embed (or array) also constructs.
func (v *Visitor) structZeroValueNeedsConstruction(t types.Type) bool {
	return v.structZeroValueNeedsConstructionRec(t, map[*types.Struct]bool{})
}

func (v *Visitor) structZeroValueNeedsConstructionRec(t types.Type, seen map[*types.Struct]bool) bool {
	if t == nil {
		return false
	}

	st, ok := t.Underlying().(*types.Struct)

	if !ok {
		return false
	}

	// Go forbids value-type embedding cycles (infinite size), so a cycle cannot actually occur —
	// the guard is purely defensive.
	if seen[st] {
		return false
	}

	seen[st] = true

	// Any promoted embed surfaces as a constructor-allocated `ж<T>` box — default leaves it null.
	if v.structHasPromotedEmbeds(t) {
		return true
	}

	for i := range st.NumFields() {
		field := st.Field(i)

		if field.Name() == "_" {
			continue
		}

		fieldType := field.Type()

		// A reference field keeps its nil zero value (correct — a nil pointer/slice/map/chan/func
		// matches Go), so it never forces construction; skipping it also stops the recursion from
		// descending through a self-referential pointer field.
		if isInherentlyHeapAllocatedType(fieldType) {
			continue
		}

		// A fixed-size array field (`[N]T` → golib array<T>) carries a `= new(N)` field initializer
		// that default(T) skips, leaving a null backing.
		if _, isArray := fieldType.Underlying().(*types.Array); isArray {
			return true
		}

		// A nested value-struct field whose own type needs construction.
		if v.structZeroValueNeedsConstructionRec(fieldType, seen) {
			return true
		}
	}

	return false
}

// emitPromotedInterfaceForwarders writes the `[GoRecv]` extensions collected since mark — one per
// method a DUAL-embed struct's *T method set obtains ONLY through an embedded-interface field —
// immediately after the struct declaration, so the extension surface is complete before go2cs-gen
// composes the pointer-form adapter the arm's `GoImplement<T, Iface>(Pointer = true)` record asks
// for (under the record alone the ImplementGenerator emits nothing, silently). Go's promotion
// makes such a method delegate to the field — nil-panicking if the field was never assigned,
// which is Go's own behavior for the shape and exactly what the forwarder reproduces. Emitted at
// the struct's own indent, so a function-local lift rides currentFuncPrefix to member level with
// its type. Variadic methods were excluded (with a warning) at collection.
func (v *Visitor) emitPromotedInterfaceForwarders(target *strings.Builder, mark int) {
	for _, fwd := range v.promotedInterfaceForwarders[mark:] {
		signature := fwd.method.Type().(*types.Signature)

		var params, args strings.Builder

		for i := range signature.Params().Len() {
			param := signature.Params().At(i)
			paramName := param.Name()

			if paramName == "" || paramName == "_" {
				paramName = fmt.Sprintf("arg%dᴛ", i+1)
			}

			paramName = getSanitizedIdentifier(paramName)

			params.WriteString(", ")
			params.WriteString(convertToCSTypeName(v.getAliasQualifiedTypeName(param.Type(), false)))
			params.WriteRune(' ')
			params.WriteString(paramName)

			if i > 0 {
				args.WriteString(", ")
			}

			args.WriteString(paramName)
		}

		var resultType string
		results := signature.Results()

		switch results.Len() {
		case 0:
			resultType = "void"
		case 1:
			resultType = convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(0).Type(), false))
		default:
			var tuple strings.Builder

			tuple.WriteRune('(')

			for i := range results.Len() {
				if i > 0 {
					tuple.WriteString(", ")
				}

				tuple.WriteString(convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(i).Type(), false)))
			}

			tuple.WriteRune(')')
			resultType = tuple.String()
		}

		methodName := getSanitizedIdentifier(fwd.method.Name())

		// C# forbids a method more accessible than its parameter types (CS0051), so an exported
		// interface method promoted onto an UNEXPORTED struct takes the struct's accessibility —
		// the same floor visitFuncDecl's explicit-method emission lands on.
		access := getAccess(fwd.method.Name())

		if getAccess(fwd.structName) == "internal" {
			access = "internal"
		}

		target.WriteString(v.newline)
		v.writeStringLn(target, "// Go method set entry for the promoted '%s.%s()' - provided ONLY by the embedded", fwd.embedName, methodName)
		v.writeStringLn(target, "// interface field in *%s's method set; see the pointer-only satisfaction record.", fwd.structName)

		if fwd.valueReceiver {
			// The method survives in Go's VALUE method set (nothing shadows it), so the forwarder
			// is a value method — the generator's own Promoted-path extension form.
			v.writeStringLn(target, "%s static %s %s(this %s recvᴛ%s) => recvᴛ.%s.%s(%s);", access, resultType, methodName, fwd.structName, params.String(), fwd.embedName, methodName, args.String())
		} else {
			v.writeStringLn(target, "[GoRecv] %s static %s %s(this ref %s recvᴛ%s) => recvᴛ.%s.%s(%s);", access, resultType, methodName, fwd.structName, params.String(), fwd.embedName, methodName, args.String())
		}
	}

	v.promotedInterfaceForwarders = v.promotedInterfaceForwarders[:mark]
}
