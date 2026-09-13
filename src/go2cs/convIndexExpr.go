// convIndexExpr.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strconv"
	"strings"
)

func (v *Visitor) convIndexExpr(indexExpr *ast.IndexExpr, context IndexExprContext) string {
	var contexts []ExprContext
	var ptrDeref string

	// The call-site zero factory a map READ of a shape-carrying element type must supply, or "" —
	// see mapReadShapedZero. Function-scoped because both the comma-ok arm (which returns early)
	// and the single-value arm (which falls through to the shared tail) render it.
	shapedZero := ""

	if typeAndVal, ok := v.info.Types[indexExpr.X]; ok {
		// Check if the type is a map and its key is an empty interface. A CONSTRAINED TYPE
		// PARAMETER with a map core (`M ~map[K]V` — the maps package) indexes through the same
		// IMap<K, V> surface (its comma-ok two-value indexer is on the interface), so it takes
		// this branch too; the concrete *types.Map check alone missed it, emitting a single-arg
		// index whose V result failed the (v, ok) deconstruction (CS8130/CS8129). Underlying:
		// a NAMED map type (dwarf's `type abbrevTable map[uint32]abbrev`) indexes through the
		// same generated map-wrapper surface — the bare assertion missed it, emitting a
		// single-value index under a (v, ok) deconstruction (CS8129 on the struct element).
		mapType, isMap := typeAndVal.Type.Underlying().(*types.Map)

		if !isMap {
			if tp, isTypeParam := types.Unalias(typeAndVal.Type).(*types.TypeParam); isTypeParam {
				if core := typeParamMapCore(tp); core != nil {
					mapType, isMap = core, true
				}
			}
		}

		if isMap {
			mapKeyInterfaceType := types.Type(nil)

			if keyIsInterface, keyIsEmptyInterface := isInterface(mapType.Key()); keyIsInterface && !keyIsEmptyInterface {
				mapKeyInterfaceType = mapType.Key()
			}

			// A POINTER-keyed map (`map[*typeInfo]bool`, encoding/gob buildEncEngine) indexed by
			// a deref-aliased pointer parameter needs the parameter's BOX as the key — the
			// alias `ref var info = ref Ꮡinfo.Value` is the wrapper VALUE, so `building[info]`
			// passed a typeInfo where ж<typeInfo> was expected (CS1503). The isPointer ident
			// context renders the box (`building[Ꮡinfo]`), mirroring the pointer-field struct
			// initializer in convKeyValueExpr.
			_, mapKeyIsPointer := mapType.Key().(*types.Pointer)

			// The box (`Ꮡ`) key rendering applies ONLY where the box exists under the raw name:
			// a deref-aliased pointer PARAMETER (its value alias `ref var info = ref Ꮡinfo.Value`
			// is the wrapper VALUE, so `m[info]` needs `m[Ꮡinfo]` to supply the `ж<T>` key), or
			// the RECEIVER of a direct-ж method (`this ж<persistConn> Ꮡpc` plus the same alias —
			// net/http transport.go closeConnIfStillIdle's `t.idleLRU.m[pc]` comma-ok read passed
			// the VALUE where ж<persistConn> was expected, CS1503 + the (v, ok) deconstruction
			// cascade). A LOCAL already holding a pointer (`var tΔ1 = scan.typ`, reflect's
			// FieldByNameFunc — tΔ1 is `ж<structType>`) IS the key and must NOT get `Ꮡ`: `ᏑtΔ1`
			// has no box accessor (CS0103 ×4). A ref-receiver method's receiver has no box at all
			// and stays excluded — the receiver-as-map-key body scan promotes such a method to
			// direct-ж (see bodyUsesReceiverAsPointerValue), so the box is always in scope here.
			if mapKeyIsPointer {
				ident, isBare := indexExpr.Index.(*ast.Ident)

				if !isBare || (!v.identIsParameter(ident) && !v.exprIsCurrentDirectBoxReceiver(ident)) {
					mapKeyIsPointer = false
				}
			}

			// An INTERFACE-typed key with a POINTER operand must render the operand in its BOX
			// form — the Pointer=true interface adapter wraps the box, and a deref-aliased pointer
			// parameter otherwise renders as the pointed-to VALUE alias, which the adapter ctor
			// cannot take (`info.Implicits[new ast_ImportSpecжNode(imp)]` needed `Ꮡimp` — go/types
			// api.go PkgNameOf, CS1503). The isPointer ident context mirrors convExprList's
			// interface-argument handling: a parameter renders its box (`Ꮡimp`), while a local/
			// field already holding the pointer renders unchanged (it IS the box).
			mapKeyOperandIsPointer := false

			if mapKeyInterfaceType != nil {
				if _, isPtr := v.getType(indexExpr.Index, false).(*types.Pointer); isPtr {
					mapKeyOperandIsPointer = true
				}
			}

			// A READ whose element type's Go zero value carries SHAPE the C# type cannot supplies
			// that zero from the call site — see mapReadShapedZero.
			if !context.isAssignmentTarget {
				shapedZero = v.mapReadShapedZero(mapType)
			}

			// Comma-ok map access (`v, ok := m[k]`): use golib's two-value indexer
			// `m[key, ꟷ]`, which returns `(value, present)`.
			if context.isTupleResult {
				// Go's m[string(b)] read special case — see mapReadTmpStringKey. The interface/
				// pointer/untyped-const key machinery below is all for non-string key types, so a
				// matched key skips it whole.
				if keyExpr, matched := v.mapReadTmpStringKey(mapType, indexExpr.Index); matched {
					if shapedZero != "" {
						return fmt.Sprintf("%s[%s, %s, %s]", v.convExpr(indexExpr.X, nil), keyExpr, shapedZero, OverloadDiscriminator)
					}

					return fmt.Sprintf("%s[%s, %s]", v.convExpr(indexExpr.X, nil), keyExpr, OverloadDiscriminator)
				}

				keyContexts := []ExprContext{}

				if types.Identical(mapType.Key(), types.NewInterfaceType(nil, nil)) {
					keyContexts = append(keyContexts, anyBoxedStringLitContext())
				} else if mapKeyIsPointer || mapKeyOperandIsPointer {
					identContext := DefaultIdentContext()
					identContext.isPointer = true
					keyContexts = append(keyContexts, identContext)
				}

				// A POINTER key looked up in an `any`-keyed map crosses the interface boundary
				// like any other pointer: as its BOX, carrying its Go type (the arm above sets
				// isPointer only for a pointer-KEYED map). See typedNilInterfaceBoxing.go.
				keyContexts = v.emptyInterfacePointerContexts(mapType.Key(), indexExpr.Index, keyContexts)

				keyExpr := v.convExpr(indexExpr.Index, keyContexts)

				if mapKeyInterfaceType != nil {
					keyExpr = v.convertToInterfaceType(mapKeyInterfaceType, v.getType(indexExpr.Index, false), keyExpr)
				}

				keyExpr = v.boxUntypedConstAsDefaultType(mapType.Key(), indexExpr.Index, keyExpr)
				keyExpr = v.boxPointerIntoEmptyInterface(mapType.Key(), indexExpr.Index, keyExpr)

				if shapedZero != "" {
					return fmt.Sprintf("%s[%s, %s, %s]", v.convExpr(indexExpr.X, nil), keyExpr, shapedZero, OverloadDiscriminator)
				}

				return fmt.Sprintf("%s[%s, %s]", v.convExpr(indexExpr.X, nil), keyExpr, OverloadDiscriminator)
			}

			// Check if the key type is an empty interface
			if types.Identical(mapType.Key(), types.NewInterfaceType(nil, nil)) {
				// A string-literal KEY boxes through @string (anyBoxedStringLitContext), matching the
				// composite-literal store: golib's map compares boxed keys with the default comparer,
				// so a bare C# string lookup would MISS a key the literal path stored as @string.
				contexts = []ExprContext{anyBoxedStringLitContext()}
			} else if mapKeyIsPointer || mapKeyOperandIsPointer {
				identContext := DefaultIdentContext()
				identContext.isPointer = true
				contexts = []ExprContext{identContext}
			}

			// A POINTER key looked up in an `any`-keyed map crosses as its BOX (see the tuple
			// arm above and typedNilInterfaceBoxing.go).
			contexts = v.emptyInterfacePointerContexts(mapType.Key(), indexExpr.Index, contexts)
		} else if _, isPtr := typeAndVal.Type.(*types.Pointer); isPtr {
			// The deref-aliased-parameter exception applies only when the base ITSELF is the
			// parameter ident (`p[i]` renders through the value alias). A pointer FIELD reached
			// through a selector — `mp.cgoCallers[0]`, where cgoCallers is `*cgoCallers` (runtime
			// proc.go) — is a real ж box and needs the `.Value` deref: the old root-ident test
			// mistook the selector's parameter ROOT for the indexed pointer and skipped it
			// (CS0021 on every named-array-wrapper box index).
			if ident, isBare := indexExpr.X.(*ast.Ident); !isBare || !v.identIsParameter(ident) {
				ptrDeref = ".Value"
			}
		}
	}

	if v.isGenericTypeArgument(indexExpr) {
		context := DefaultIdentContext()
		context.isType = true

		if len(contexts) > 0 {
			contexts = append(contexts, contexts...)
		} else {
			contexts = []ExprContext{context}
		}

		// The base (X) renders WITHOUT its own generic type arguments: this branch appends the
		// explicit `<Index>` here, so letting convSelectorExpr also append the inferred instance
		// args (the generic-function-value path) produced `pkg.Func<T><T>` (CS1525/CS0119/CS8124).
		xContext := DefaultLambdaContext()
		xContext.suppressGenericTypeArgs = true

		// The BARE-IDENT form of the same suppression: a same-package generic function referenced
		// as a value appends its resolved instantiation in convIdent, and this branch appends the
		// written `<Index>` — two copies would emit `Equal<Slice, E><Slice>`.
		xIdentContext := DefaultIdentContext()
		xIdentContext.suppressGenericTypeArgs = true
		xContexts := []ExprContext{xContext, xIdentContext}

		// A single written type argument pinning an ERASED (pointer-core) position — the partial
		// instantiation `clone[*thing](…)` — drops entirely: the position no longer exists in the
		// emitted C# generic parameter list, and the remaining parameters infer from the value
		// arguments (see explicitTypeArgsAfterErasure).
		writtenCount := 1

		if kept, erased := v.explicitTypeArgsAfterErasure(indexExpr.X, []ast.Expr{indexExpr.Index}); erased {
			if len(kept) == 0 {
				return fmt.Sprintf("%s%s", v.convExpr(indexExpr.X, xContexts), ptrDeref)
			}

			writtenCount = len(kept)
		}

		// A PARTIAL Go instantiation of a generic FUNCTION completes from the resolved instance —
		// C# has no partial instantiation (CS0305). See completedInstantiationTypeArgs; a complete
		// written list returns nil and the written argument renders verbatim.
		if completed := v.completedInstantiationTypeArgs(indexExpr.X, writtenCount); completed != nil {
			return fmt.Sprintf("%s%s<%s>", v.convExpr(indexExpr.X, xContexts), ptrDeref, strings.Join(completed, ", "))
		}

		return fmt.Sprintf("%s%s<%s>", v.convExpr(indexExpr.X, xContexts), ptrDeref, v.convExpr(indexExpr.Index, contexts))
	}

	index := v.convExpr(indexExpr.Index, contexts)

	if typeAndVal, ok := v.info.Types[indexExpr.X]; ok {
		if mapType, isMap := typeAndVal.Type.Underlying().(*types.Map); isMap {
			// Go's m[string(b)] READ special case — see mapReadTmpStringKey. Assignment targets are
			// excluded: a map STORE retains its key, so the store side keeps the copying conversion
			// (exactly the boundary Go's own optimization draws).
			if !context.isAssignmentTarget {
				if keyExpr, matched := v.mapReadTmpStringKey(mapType, indexExpr.Index); matched {
					index = keyExpr
				}
			}

			if keyIsInterface, keyIsEmptyInterface := isInterface(mapType.Key()); keyIsInterface && !keyIsEmptyInterface {
				index = v.convertToInterfaceType(mapType.Key(), v.getType(indexExpr.Index, false), index)
			}

			// An untyped-constant KEY looked up in an `any`-keyed map boxes at Go's DEFAULT TYPE, so it
			// matches a key stored by the composite-literal path (which applies the same cast) or by any
			// real `int` value — golib's map compares boxed keys with the default Dictionary comparer,
			// which does not normalize nint(6) against Int32(6).
			index = v.boxUntypedConstAsDefaultType(mapType.Key(), indexExpr.Index, index)
			index = v.boxPointerIntoEmptyInterface(mapType.Key(), indexExpr.Index, index)
		}
	}

	// A STRING base indexed by a wide/unsigned integer: a string LITERAL renders as a
	// ReadOnlySpan<byte> (`"…"u8`) whose indexer takes int — a uintptr index is CS1503
	// (runtime heapdump.go's `"0123456789abcdef"[pc&15]`, pc a uintptr). Go converts any
	// integer index to int for the access; route the wide kinds (uint/uint32/uint64/
	// uintptr/int64) through the same `(int)` cast the element-address seams use. An
	// `@string` variable's indexer binds an int argument too, so the cast is safe for
	// both renders; an int/small-integer index is emitted unchanged (no churn).
	if baseType := v.getType(indexExpr.X, false); baseType != nil {
		if basic, ok := baseType.Underlying().(*types.Basic); ok && basic.Info()&types.IsString != 0 {
			// A string LITERAL base (`"…"u8`) is an int-only-indexed ReadOnlySpan<byte>, so a
			// plain Go `int` index (→ C# nint) needs the (int) cast too; an @string variable's
			// indexer accepts nint, so it keeps the wide-only cast (no churn on int indices).
			if _, isLit := indexExpr.X.(*ast.BasicLit); isLit {
				index = v.castStringLiteralIndexToInt(indexExpr.Index)
			} else {
				index = v.castWideIntegerToInt(indexExpr.Index)
			}
		}

		// A SLICE/ARRAY base indexed by a PLAIN BASIC integer kind with no implicit
		// conversion to the golib nint indexer — int64/uint/uint32/uint64 (`r.s[r.i]`,
		// bytes Reader's int64 cursor; CS1503 long→nint) — takes an explicit (nint) cast,
		// matching Go's implicit index conversion. Kinds that widen implicitly (int32,
		// int16, byte, …) are unchanged, and a NAMED index type is left alone — its own
		// conversion surface binds the indexer (a `(nint)` cast of a named-over-uintptr
		// would chain two user conversions, CS0030 — SparseArrayNamedIntKey's errno keys).
		switch baseType.Underlying().(type) {
		case *types.Slice, *types.Array:
			idxType := types.Unalias(v.getType(indexExpr.Index, false))

			if basic, isBasic := idxType.(*types.Basic); isBasic {
				switch basic.Kind() {
				case types.Uint, types.Uint32, types.Uint64, types.Uintptr, types.Int64:
					index = fmt.Sprintf("(nint)(%s)", index)
				}
			} else if named, isNamed := idxType.(*types.Named); isNamed {
				// A NAMED type over signed `int64` — internal/trace's `type ProcID int64` indexing
				// `spans[procID]` (CS1503) — has no bare path to the indexer: there is no `this[long]`
				// overload, `int64→nint` does not narrow implicitly, and `int64→ulong` (which would
				// bind `this[ulong]`) is a signed→unsigned conversion. `(nint)(x)` composes as one
				// user conversion (named→long) plus one built-in (long→nint). Every OTHER kind is
				// deliberately EXCLUDED — no churn, and casting some would even break: an UNSIGNED
				// named type binds the golib `this[ulong]` overload bare (`type kindT uint`/uint32/
				// uint64), and a nuint-backed wrapper (uint/uintptr) is CS0030 under a `(nint)` cast;
				// an int/int32/nint underlying narrows implicitly (`type rank int` stays bare).
				if ub, ok := named.Underlying().(*types.Basic); ok && ub.Kind() == types.Int64 {
					index = fmt.Sprintf("(nint)(%s)", index)
				}
			} else if tp, isTP := idxType.(*types.TypeParam); isTP && typeParamIsInteger(tp) {
				// A numeric TYPE PARAMETER index — internal/trace's `dataTable[EI ~uint64]`
				// indexing `d.dense[id]` — has no C# cast to nint (a constrained type parameter is
				// not directly convertible). Route through golib's ConvertToUInt64<T> bridge (the
				// integer-type-param conversion family, cf. rand.N's E(x)), then narrow to nint.
				index = fmt.Sprintf("(nint)(ConvertToUInt64<%s>(%s))", v.getCSharpTypeName(tp), index)
			}
		}
	}

	// The BASE of an index-expression assignment TARGET converts in assignment context so a
	// pointer auto-deref takes the writable `.Value` path — `req.Value.Header[k] = vv`, not the
	// rvalue read form `(~req).Header[k] = vv` (see IndexExprContext.isAssignmentTarget).
	var xContexts []ExprContext

	if context.isAssignmentTarget {
		assignContext := DefaultLambdaContext()
		assignContext.isAssignment = true
		xContexts = []ExprContext{assignContext}
	}

	baseExpr := v.convExpr(indexExpr.X, xContexts)

	// A type-CONVERSION base renders as a C# cast, and postfix binds tighter than a cast — both
	// the pointer auto-deref `.Value` and the index itself would re-bind onto the cast's INNER
	// operand: Go malloc.go's `(*[2]uint64)(x)[0] = 0` emitted
	// `(ж<array<uint64>>)(uintptr)(x).Value[0]` — the `.Value` read the inner @unsafe.Pointer's
	// uintptr, then indexed a nuint (CS0021). Wrap the cast before appending. Fifth instance of
	// the cast-precedence family.
	if call, ok := indexExpr.X.(*ast.CallExpr); ok && v.callExprIsTypeConversion(call) {
		baseExpr = "(" + baseExpr + ")"
	}

	// A map READ whose element carries shape takes golib's zero-factory indexer overload.
	if shapedZero != "" {
		return fmt.Sprintf("%s%s[%s, %s]", baseExpr, ptrDeref, index, shapedZero)
	}

	return fmt.Sprintf("%s%s[%s]", baseExpr, ptrDeref, index)
}

// mapReadShapedZero renders the CALL-SITE zero factory a map READ must supply when the map's
// ELEMENT type's Go zero value carries run-time SHAPE its C# type does not — a fixed-size array,
// whose Go length lives only in the constructed `array<T>` instance. Returns "" for every other
// element shape, so the ordinary `m[key]` emission is unchanged.
//
// Go's read of an absent key (a nil map included) yields the element type's zero value, which for
// `[N]T` is N zeroed elements. `map<TKey, TValue>`'s plain indexer hands back `default(TValue)`,
// and `default(array<T>)` has LENGTH ZERO — so the first index into a missed entry panicked
// `index out of range [0] with length 0` where Go reads a zero. The measured witness is `html`'s
// `unescapeEntity`: `entity2` is `map[string][2]rune` and its miss is the normal path.
//
// The shape has to come from HERE. It is a property of the Go map TYPE, and neither the emitted
// `map<TKey, TValue>` nor a nil one carries it; reading it off an existing entry would answer only
// for a populated map. This is the same zero-value ladder every declaration site uses
// (zeroValueInitializer / arrayZeroValueArgs) — a map read is simply a zero-value site the ladder
// had never reached.
//
// The caller passes the RESOLVED map type, so all three map emissions are covered by one rule —
// the plain `map<K, V>`, a NAMED map type's generated wrapper (which forwards the overload; see
// go2cs-gen's IMapTypeTemplate) and a map-cored type parameter (which reaches it as the
// `IMap<K, V>` default member). Two exclusions keep the emission unchanged where it is already
// correct:
//
//   - a NAMED array element — its generated wrapper allocates its backing lazily from its own
//     known size, so `default` is already its Go zero (the same exclusion arrayElemFactory
//     documents); and
//   - an assignment TARGET (the caller gates on isAssignmentTarget): a STORE carries a value and
//     needs no zero.
func (v *Visitor) mapReadShapedZero(mapType *types.Map) string {
	if mapType == nil {
		return ""
	}

	elemType := mapType.Elem()

	if _, isNamed := types.Unalias(elemType).(*types.Named); isNamed {
		return ""
	}

	array, isArray := elemType.Underlying().(*types.Array)

	if !isArray {
		return ""
	}

	// The element factory rides along for a NESTED shape (`map[k][2][3]int`), exactly as it does
	// for a declaration — otherwise the inner arrays would themselves be length zero.
	return fmt.Sprintf("() => new %s(%s)", v.getCSharpTypeName(elemType),
		v.arrayZeroValueArgs(strconv.FormatInt(array.Len(), 10), array))
}

// mapReadTmpStringKey recognizes Go's `m[string(b)]` map-READ special case — the compiler avoids
// copying b's bytes into a real string because the lookup key provably does not outlive the index
// expression (runtime.slicebytetostringtmp) — and emits the key as golib's `tmpstring(b)`, a
// TRANSIENT @string aliasing the slice's backing. Zero allocation, matching Go's cost model
// (net/textproto's canonicalMIMEHeaderKey common-header probe paid one backing copy per call
// against Go's 0 on a want-zero AllocsPerRun path, L11).
//
// The scope is deliberately EXACTLY the shape whose safety Go's own optimization proves:
//   - a map INDEX in rvalue position (both plain and comma-ok reads; never an assignment target,
//     where the map STORES the key and the alias would escape into the dictionary);
//   - the map's key type is the PREDECLARED string type (the emitted key slot is then exactly
//     @string, which tmpstring produces — a NAMED string key converts through its wrapper and
//     keeps the copying path);
//   - the key expression is a conversion to predeclared string whose operand is a plain []byte
//     (elem exactly basic uint8 — a named-over-byte element emits slice<NamedByte>, which
//     tmpstring's slice<byte> parameter cannot take).
func (v *Visitor) mapReadTmpStringKey(mapType *types.Map, keyExpr ast.Expr) (string, bool) {
	keyBasic, ok := types.Unalias(mapType.Key()).(*types.Basic)

	if !ok || keyBasic.Info()&types.IsString == 0 {
		return "", false
	}

	call, ok := keyExpr.(*ast.CallExpr)

	if !ok || len(call.Args) != 1 || !v.callExprIsTypeConversion(call) {
		return "", false
	}

	// The conversion's RESULT must itself be predeclared string — `myStr(b)` keeps its wrapper path.
	convBasic, ok := types.Unalias(v.getType(call, false)).(*types.Basic)

	if !ok || convBasic.Info()&types.IsString == 0 {
		return "", false
	}

	sliceType, ok := types.Unalias(v.getType(call.Args[0], false)).(*types.Slice)

	if !ok {
		return "", false
	}

	elemBasic, ok := types.Unalias(sliceType.Elem()).(*types.Basic)

	if !ok || elemBasic.Kind() != types.Uint8 {
		return "", false
	}

	return fmt.Sprintf("tmpstring(%s)", v.convExpr(call.Args[0], nil)), true
}

func (v *Visitor) isGenericTypeArgument(indexExpr *ast.IndexExpr) bool {
	// The index being a TYPE expression of ANY form — `T`, `*T`, `[]T`, `map[K]V`, `pkg.T` —
	// marks a generic instantiation (internal/xcoff's `saferio.SliceCap[*Section]`, a POINTER
	// type argument, which the Ident/Selector cases below miss). go/types records this on the
	// expression directly; without it the Go bracket form survived while convCallExpr also
	// appended the resolved `<...>`, emitting `SliceCap[ж<…>]<ж<…>>(…)` (CS0021/CS0119).
	if tv, ok := v.info.Types[indexExpr.Index]; ok && tv.IsType() {
		return true
	}

	switch index := indexExpr.Index.(type) {
	case *ast.Ident:
		// Check if this identifier refers to a type
		if obj := v.info.Uses[index]; obj != nil {
			_, isTypeName := obj.(*types.TypeName)
			return isTypeName
		}
	case *ast.SelectorExpr:
		// A CROSS-PACKAGE qualified type argument — `reflect.TypeFor[encoding.BinaryMarshaler]`
		// (encoding/gob). Without this the Ident-only check missed it and the generic
		// instantiation kept the Go bracket form while convCallExpr also appended the C#
		// `<...>` from info.Instances, emitting `TypeFor[encoding.BinaryMarshaler]<…>()`
		// (CS1525). The Sel resolving to a TypeName routes it through the `<T>` branch.
		if obj := v.info.Uses[index.Sel]; obj != nil {
			_, isTypeName := obj.(*types.TypeName)
			return isTypeName
		}
	}

	return false
}
