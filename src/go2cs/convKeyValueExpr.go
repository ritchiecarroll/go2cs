// convKeyValueExpr.go - Gbtc
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
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
)

// anyBoxedStringLitContext returns the literal context that boxes a string literal through
// @string — `(@string)"…"u8` — for an EMPTY-interface (`any`) target slot. A bare `"…"u8`
// ReadOnlySpan<byte> has no conversion to object (CS1503/CS0029) and a bare C# string boxes the
// wrong type (a later Go x.(string) assertion fails), so the CAST is mandatory here; the `u8`
// suffix is then free, and eliminates the per-evaluation `Encoding.UTF8.GetBytes` transcode the
// UTF-16 literal paid (2.1–2.4× ASCII, 4.2× non-ASCII). The cast is what makes the combination
// legal: it converts the ROM span to @string, which boxes. Mirrors visitReturnStmt's any-result
// literal context; only string basic-literals consult these flags.
// spanTargetUnsupported stays set: the SLOT still cannot hold a bare span, so a literal CONCAT in
// this position keeps its plain-string operands (see BasicLitContext.spanTargetUnsupported).
func anyBoxedStringLitContext() BasicLitContext {
	litContext := DefaultBasicLitContext()
	litContext.u8StringOK = true
	litContext.castToGoString = true
	litContext.spanTargetUnsupported = true

	return litContext
}

// isStringBasicLit reports whether expr is a string basic-literal — the only expression form
// whose rendering consults the u8StringOK/castToGoString literal flags.
func isStringBasicLit(expr ast.Expr) bool {
	lit, ok := expr.(*ast.BasicLit)

	return ok && lit.Kind == token.STRING
}

func (v *Visitor) convKeyValueExpr(keyValueExpr *ast.KeyValueExpr, context KeyValueContext) string {
	// For a struct field initializer whose field is a pointer type, the value must be emitted as a
	// pointer (box) — e.g. a deref'd pointer parameter `val` used as `key: val` where `key` is `*V`
	// needs `Ꮡval` (like the same value passed as a pointer call argument). Resolve the field from
	// the key name so this works for keyed literals regardless of field order.
	var valueContexts []ExprContext
	var structFieldIfaceType types.Type
	var structFieldType types.Type

	if context.source == StructSource {
		if keyIdent, ok := keyValueExpr.Key.(*ast.Ident); ok {
			if fieldObj := v.info.Uses[keyIdent]; fieldObj != nil {
				structFieldType = fieldObj.Type()

				if _, isPtr := fieldObj.Type().(*types.Pointer); isPtr {
					identContext := DefaultIdentContext()
					identContext.isPointer = true
					valueContexts = []ExprContext{identContext}
				} else if needsCast, isEmpty := isInterface(fieldObj.Type()); needsCast && !isEmpty {
					// An INTERFACE field routes its value through the interface conversion below —
					// a POINTER value (`src: &runtimeSource{}`, math/rand's Rand literal) renders
					// as the box and wraps in the pointer-interface adapter
					// (`src: new runtimeSourceᴵSource(Ꮡ(new runtimeSource()))`); without this the
					// raw box meets the interface-typed ctor parameter (CS1503).
					structFieldIfaceType = fieldObj.Type()

					if _, valIsPtr := v.getExprType(keyValueExpr.Value).(*types.Pointer); valIsPtr {
						identContext := DefaultIdentContext()
						identContext.isPointer = true
						valueContexts = []ExprContext{identContext}
					}
				} else if isEmptyInterfaceTarget(fieldObj.Type()) && isStringBasicLit(keyValueExpr.Value) {
					// An EMPTY-interface (`any`) field bypasses the interface wrap above, so a
					// string-literal value must box through @string — `inner: (@string)"hi"`, NOT
					// the default `"hi"u8` span, which has no conversion to the generated ctor's
					// object parameter (CS1503). Applies to typed, elided, and pointer-elided keyed
					// composites alike (the key resolves through info.Uses in each).
					valueContexts = append(valueContexts, anyBoxedStringLitContext())
				}
			}
		}
	}

	// A MAP whose VALUE type is a pointer needs its bare-ident pointer value boxed the same way as a
	// pointer struct field — `map[K]*T{k: c}` where `c` is a deref'd pointer parameter otherwise
	// renders the VALUE alias into a `ж<T>` map slot (CS0029). Mirror the struct-field routing: a
	// pointer-typed value expr sets isPointer so a bare ident renders as its box `Ꮡc` (a value that is
	// already a box — `&x`, a pointer local — is unaffected). Gated on the map's declared ELEMENT type
	// being a pointer, so an interface-valued map (`map[K]any{…}`) still routes through the interface
	// conversion below.
	if context.source == MapSource && context.compositeType != nil {
		if mapType, ok := types.Unalias(context.compositeType).Underlying().(*types.Map); ok {
			if _, elemIsPtr := mapType.Elem().Underlying().(*types.Pointer); elemIsPtr {
				if _, valIsPtr := v.getExprType(keyValueExpr.Value).(*types.Pointer); valIsPtr {
					identContext := DefaultIdentContext()
					identContext.isPointer = true
					valueContexts = []ExprContext{identContext}
				}
			}
		}
	}

	// A MapSource VALUE slot whose declared element type is the EMPTY interface — a
	// `map[K]any{k: "v"}` value or a sparse `[N]any{i: "v"}` element (arrayBacked, the composite
	// type is the array/slice itself) — boxes a string-literal value through @string
	// (`[k] = (@string)"v"`), NOT the default `"v"u8` span, which has no conversion to the object
	// slot (CS0029/CS1503). Mirrors the struct-field `any` routing above.
	if context.source == MapSource && context.compositeType != nil && isStringBasicLit(keyValueExpr.Value) {
		var elemType types.Type

		switch u := types.Unalias(context.compositeType).Underlying().(type) {
		case *types.Map:
			elemType = u.Elem()
		case *types.Array:
			elemType = u.Elem()
		case *types.Slice:
			elemType = u.Elem()
		}

		if isEmptyInterfaceTarget(elemType) {
			valueContexts = append(valueContexts, anyBoxedStringLitContext())
		}
	}

	// The value's declared SLOT — a struct field, or the map/array/slice element the keyed
	// composite stores into. Resolved before the value is rendered because an EMPTY-interface
	// slot changes the rendering of a POINTER value (it crosses as its BOX, not as a deref
	// alias whose boxing would capture a copy of the pointee — typedNilInterfaceBoxing.go); the
	// untyped-constant and typed-nil boxing below consume the same answer.
	var valueSlotType types.Type

	if context.source == StructSource {
		valueSlotType = structFieldType
	} else if context.source == MapSource && context.compositeType != nil {
		switch u := types.Unalias(context.compositeType).Underlying().(type) {
		case *types.Map:
			valueSlotType = u.Elem()
		case *types.Array:
			valueSlotType = u.Elem()
		case *types.Slice:
			valueSlotType = u.Elem()
		}
	}

	// A func LITERAL in an EMPTY-interface value slot is NATURAL-typed by C# for exactly the
	// reason one in an `any` ARGUMENT slot is: there is no delegate target, so C# derives the
	// delegate type from the body. A literal whose body never completes normally has NO return
	// statement to derive from, so C# infers `Action` and the Go RESULT TYPE IS LOST —
	// `FuncMap{"die": func() bool { panic("die") }}` (text/template's exec_test funcs) emitted
	// `["die"u8] = () => { throw panic("die"); }`, which the reflection bridge then describes
	// truthfully as a ZERO-result func, so text/template's own goodFunc rejects a function Go
	// accepts and every FuncMap holding one panics at registration (16 of 52 verdicts).
	// Marking the slot makes convFuncLit state the declared Go result type explicitly — the same
	// treatment CallExprContext.emptyInterfaceArgs already gives the argument position, reached
	// here through the KEYED composite forms (map value, `any` struct field, sparse-array
	// element) that never route through convExprList's argument plumbing.
	untypedInterfaceLambda := false

	if _, valueIsFuncLit := keyValueExpr.Value.(*ast.FuncLit); valueIsFuncLit {
		untypedInterfaceLambda = isEmptyInterfaceTarget(valueSlotType)
	}

	// Thread the enclosing statement's hoist target so a func-literal VALUE's capture
	// snapshot decls hoist instead of dumping inline in the argument list (elf file.go's
	// readSeekerFromReader{reset: func() {…}}, CS1003 cascade ×6).
	if context.deferredDecls != nil || untypedInterfaceLambda {
		hoistLambdaContext := DefaultLambdaContext()
		hoistLambdaContext.deferredDecls = context.deferredDecls
		hoistLambdaContext.untypedInterfaceTarget = untypedInterfaceLambda
		valueContexts = append(valueContexts, hoistLambdaContext)
	}

	valueContexts = v.emptyInterfacePointerContexts(valueSlotType, keyValueExpr.Value, valueContexts)

	valueExpr := v.convExpr(keyValueExpr.Value, valueContexts)

	// A keyed VALUE that reads an ARRAY out of existing storage clones into its slot — Go
	// copies the array into a struct field, map value, or sparse-array element alike, and the
	// emitted struct copy would alias its backing (see exprReadsValueNeedingClone). Applied
	// before the interface wraps below so an interface-typed slot boxes the clone.
	if v.exprReadsValueNeedingClone(keyValueExpr.Value) {
		valueExpr = appendValueClone(valueExpr, v.getExprType(keyValueExpr.Value))
	}

	// Box an untyped CONSTANT VALUE in an EMPTY-interface slot at Go's default type for its kind — a
	// struct `any` field (`box{v: 9}`) or a `map[K]any` / sparse-`[N]any` value — so a later `x.(int)`
	// matches Go's boxed `int`. A string LITERAL already took the tighter `(@string)"…"` rendering
	// from anyBoxedStringLitContext above, which applyUntypedConstBoxCast leaves alone; a string
	// CONCATENATION (which that literal-only flag cannot reach) is boxed here. A no-op for any other
	// slot or a non-untyped-constant value. The POINTER twin (a nil one carries its Go type across)
	// follows it — see typedNilInterfaceBoxing.go.
	valueExpr = v.boxUntypedConstAsDefaultType(valueSlotType, keyValueExpr.Value, valueExpr)
	valueExpr = v.boxPointerIntoEmptyInterface(valueSlotType, keyValueExpr.Value, valueExpr)

	// A NARROW-integer arithmetic value in a struct-FIELD initializer needs the same
	// cast-back as the assignment forms — Go wraps the arithmetic at the operand width,
	// C# promotes it to int (image/draw's RGBA64 literal `R: uint16(…)+srgba.R`,
	// CS1503 ×4 against the generated ushort ctor parameter).
	if structFieldType != nil {
		if narrowCast := v.narrowArithmeticCastTypeFor(structFieldType, keyValueExpr.Value, valueExpr); len(narrowCast) > 0 {
			valueExpr = fmt.Sprintf("(%s)(%s)", narrowCast, valueExpr)
		}
	}

	if structFieldIfaceType != nil {
		valueExpr = v.convertToInterfaceType(structFieldIfaceType, v.getExprType(keyValueExpr.Value), valueExpr)
	}

	// The interface a keyed VALUE must convert to is the composite's OWN value slot — never the type
	// of the variable the composite is being assigned to. A composite assigned to an interface-typed
	// variable is converted to that interface AS A WHOLE (visitAssignStmt's convertExprToInterfaceType
	// / visitValueSpec's convInterfaceDeclValue), so applying the LHS's interface to each ELEMENT as
	// well is a second, wrong conversion: `fsys = fstest.MapFS{"william": {Data: …}}` with `fsys` of
	// type `fs.FS` (os's TestCopyFS) ran every `*MapFile` value through a `*MapFile`→`fs.FS` cast,
	// whose deref-copy fallback collapses `~Ꮡ(new MapFile(…))` to the bare struct — a
	// `map[string]*MapFile` slot holding a VALUE (CS0029 ×5). The same literal in a `var`
	// declaration, a call argument, or assigned to its own concrete type was always correct; only
	// the reassignment-to-an-interface path carried the LHS type down here.
	//
	// The LHS ident stays as the FALLBACK for a composite that does not state its slot type here
	// (the reason it was consulted in the first place — sparse-array initializations), so a slot the
	// composite does name decides, and an unknown one behaves exactly as before. structFieldIfaceType
	// has already converted an interface STRUCT field above; skipping it here keeps that single.
	valueIfaceType := valueSlotType

	if valueIfaceType == nil && context.ident != nil {
		valueIfaceType = v.getIdentType(context.ident)
	}

	if structFieldIfaceType == nil && valueIfaceType != nil {
		if needsInterfaceCast, isEmpty := isInterface(valueIfaceType); needsInterfaceCast && !isEmpty {
			valueType := v.getExprType(keyValueExpr.Value)
			valueExpr = v.convertToInterfaceType(valueIfaceType, valueType, valueExpr)
		}
	}

	if context.source == StructSource {
		// Struct field initializer. The key is a FIELD NAME, which is struct-scoped — it must use
		// the field's DECLARED C# name (getCoreSanitizedIdentifier), NOT the package-level
		// nameCollisions `Δ`-rename that convExpr/convIdent applies. A field named like a colliding
		// package type/method (`type funcInfo` + `func (*Func) funcInfo()` → field `funcInfo` of
		// `Frame`) is declared unrenamed (`funcInfo`), so a keyed literal `Frame{funcInfo: …}` must
		// emit `funcInfo:`, not `ΔfuncInfo:` (which is not a parameter of the generated ctor → CS1739).
		key := v.convExpr(keyValueExpr.Key, nil)

		if keyIdent, ok := keyValueExpr.Key.(*ast.Ident); ok {
			if nameCollisions[keyIdent.Name] {
				key = removeLeadingSanitizationMarker(getCoreSanitizedIdentifier(keyIdent.Name))
			}

			// A field named like its OWN struct type (`Description.Description`, runtime/metrics)
			// is DECLARED with the type-colliding rename (CS0542 avoidance), so the keyed literal
			// must name the renamed ctor parameter too — the unrenamed key was CS1739 ×57. Only a
			// PACKAGE-LEVEL named type keeps its Go name as the C# type name (the same gate
			// fieldCollidesWithType applies for selector emission).
			// An ELIDED pointer-composite (`[]*Address{{Address: …}}`, net/mail) carries the slice's
			// element type *Address; unwrap the pointer so the type-colliding-field check sees the
			// underlying struct (else the keyed literal kept the unrenamed `Address:`, CS1739).
			collisionType := types.Unalias(context.compositeType)

			if ptr, isPtr := collisionType.(*types.Pointer); isPtr {
				collisionType = types.Unalias(ptr.Elem())
			}

			if named, ok := collisionType.(*types.Named); ok {
				obj := named.Obj()

				if obj.Pkg() != nil && obj.Parent() == obj.Pkg().Scope() && getSanitizedIdentifier(keyIdent.Name) == getSanitizedIdentifier(obj.Name()) {
					key = removeLeadingSanitizationMarker(typeCollidingFieldName(getCoreSanitizedIdentifier(keyIdent.Name)))
				}
			}
		}

		return fmt.Sprintf("%s: %s", key, valueExpr)
	} else if context.source == MapSource {
		// Map key/value initializer — or, when array-backed, a SparseArray index initializer
		keyExpr := v.sparseArrayKey(keyValueExpr.Key, context)

		// A pointer-KEY map (`map[*T]V{c: 1}`) boxes the bare-ident key the same way as a pointer
		// value — the value alias into a `ж<T>` key slot is CS0029. (A pointer key cannot be
		// array-backed, so the sparseArrayKey int-cast path never applies here.)
		if context.compositeType != nil {
			if mapType, ok := types.Unalias(context.compositeType).Underlying().(*types.Map); ok {
				if _, keyIsPtr := mapType.Key().Underlying().(*types.Pointer); keyIsPtr {
					if _, exprIsPtr := v.getExprType(keyValueExpr.Key).(*types.Pointer); exprIsPtr {
						keyIdentContext := DefaultIdentContext()
						keyIdentContext.isPointer = true
						keyExpr = v.convExpr(keyValueExpr.Key, []ExprContext{keyIdentContext})
					}
				}

				// An EMPTY-interface KEY slot (`map[any]V{"ky": …}`) boxes a string-literal key
				// through @string — `[(@string)"ky"] = …`, not `["ky"u8]` (span → object key,
				// CS1503). The NON-empty interface key wrap stays with the map-index adapter
				// machinery; only the `any` key needs the literal re-render.
				if isEmptyInterfaceTarget(mapType.Key()) && isStringBasicLit(keyValueExpr.Key) {
					keyExpr = v.convExpr(keyValueExpr.Key, []ExprContext{anyBoxedStringLitContext()})
				}

				// A POINTER key in an `any` key slot crosses as its BOX — the pointer-KEYED arm
				// above sets isPointer only when the map's own key type is a pointer. Re-rendered
				// here for the same reason the string literal is (typedNilInterfaceBoxing.go).
				if v.pointerBoxesIntoEmptyInterface(mapType.Key(), keyValueExpr.Key) {
					keyExpr = v.convExpr(keyValueExpr.Key, v.emptyInterfacePointerContexts(mapType.Key(), keyValueExpr.Key, nil))
				}

				// An untyped CONSTANT KEY in an `any` key slot boxes at Go's default type too —
				// `[(nint)(6)] = …`. golib's map uses the default Dictionary comparer (no numeric
				// normalization: nint(6) != Int32(6)), so the store and every LOOKUP must agree on the
				// boxed type. convIndexExpr applies the same cast to an untyped-constant index of an
				// `any`-keyed map, which keeps `map[any]int{6:1}[6]` round-tripping AND makes a lookup by
				// a real `int` VALUE (`m[n]`, boxed nint — the only form Go can distinguish) HIT, which
				// the former leave-both-as-Int32 behavior missed.
				keyExpr = v.boxUntypedConstAsDefaultType(mapType.Key(), keyValueExpr.Key, keyExpr)
				keyExpr = v.boxPointerIntoEmptyInterface(mapType.Key(), keyValueExpr.Key, keyExpr)
			}
		}

		// An ARRAY-typed map KEY (arrays are comparable, so `map[[2]int]V` is legal Go) is
		// copied into the map on store; clone it so a later write through the source variable
		// cannot mutate the stored key's backing out from under the dictionary. A sparse-array
		// key is an int index and never matches the type gate.
		if v.exprReadsValueNeedingClone(keyValueExpr.Key) {
			keyExpr = appendValueClone(keyExpr, v.getExprType(keyValueExpr.Key))
		}

		return fmt.Sprintf("[%s] = %s", keyExpr, valueExpr)
	} else if context.source == ArraySource {
		// Sparse array initializer
		return fmt.Sprintf("%s[%s] = %s", context.ident, v.sparseArrayKey(keyValueExpr.Key, context), valueExpr)
	} else {
		panic(fmt.Sprintf("Unexpected key/value source: %#v", context.source))
	}
}

// sparseArrayKey converts a keyed-composite index expression. For an array-backed composite (a
// `[]T{i: v}` slice/array literal emitted as a SparseArray, int-indexed) a key whose Go type is a
// DEFINED integer type (e.g. runtime's `type lockRank int`, emitted `[GoType("num:nint")]`) is cast
// to `int`: such a type does not implicitly narrow to the indexer's int parameter (CS1503). A plain
// int/untyped-int key, or a real map key, is emitted unchanged.
func (v *Visitor) sparseArrayKey(key ast.Expr, context KeyValueContext) string {
	keyExpr := v.convExpr(key, nil)

	if !context.arrayBacked {
		return keyExpr
	}

	if keyType := v.info.TypeOf(key); keyType != nil {
		if named, ok := keyType.(*types.Named); ok {
			if basic, ok := named.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
				switch basic.Kind() {
				case types.Int8, types.Uint8, types.Int16, types.Uint16, types.Int32:
					// These widen implicitly to C# int (Int32), so the indexer accepts the
					// defined type directly — no cast needed (keeps output minimal).
				default:
					// int/int64/uint/uint32/uint64/uintptr underlyings do NOT implicitly narrow
					// to int32, so an explicit cast is required for the SparseArray int indexer.
					// A COMPOSITE key (`E2BIG - APPLICATION_ERROR`) must parenthesize, or the
					// cast binds only the first operand. A UINTPTR-backed named numeric converts
					// through TWO user-defined operators (named -> golib uintptr struct -> int),
					// which C# rejects in a single cast (syscall zerrors' Errno keys, CS0030
					// x131) - split the steps; other underlyings reach int with one user-defined
					// plus one standard conversion, so the direct form stays (no golden churn).
					_, isIdent := key.(*ast.Ident)

					// A composite CONSTANT key (`E2BIG - APPLICATION_ERROR`, syscall zerrors
					// x131) folds to its integer value: its rendering mixes named/underlying
					// operands whose operators are ambiguous (CS0034), and no cast placement
					// fixes both operands. Simple IDENT keys keep their symbolic form (visual
					// similarity - `[(int)rankLow]` stays).
					if !isIdent {
						if tv, ok := v.info.Types[key]; ok && tv.Value != nil {
							if i, exact := constant.Int64Val(constant.ToInt(tv.Value)); exact {
								return strconv.FormatInt(i, 10)
							}
						}

						// Non-constant composite: parenthesize so the cast binds the whole key.
						keyExpr = fmt.Sprintf("(%s)", keyExpr)
					}

					// A uintptr-backed named numeric converts through TWO user-defined
					// operators (named -> golib uintptr struct -> int), which C# rejects in a
					// single cast (CS0030) - split the steps, keeping the symbolic operand.
					// The UNSIGNED kinds share the gap: a named-over-nuint/uint/ulong key's
					// single `(int)key` chains the wrapper operator into an explicit unsigned→
					// int narrowing (reflect kindNames' ΔKind keys, num:nuint — CS0030 ×27).
					switch basic.Kind() {
					case types.Uintptr, types.Uint, types.Uint32, types.Uint64:
						return fmt.Sprintf("(int)((%s)%s)", v.getCSharpTypeName(basic), keyExpr)
					}

					return fmt.Sprintf("(int)%s", keyExpr)
				}
			}
		}
	}

	return keyExpr
}
