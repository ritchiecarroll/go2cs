// convCompositeLit.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
	"strings"
)

// recordStructFieldInterfaceCasts records the GoImplement pair and routes the RENDER for every
// composite element whose target STRUCT FIELD is a (non-empty) interface: a value- or
// pointer-receiver concrete assigned to an interface field must be boxed via its partial struct
// or wrapped in its generated adapter (`new <T>ж<Iface>(…)`), never passed bare (CS1503). It is
// shared by the TYPED composite path (checkStructFields) and the ELIDED element-composite path
// (`{v0, v1, …}` inside a `[]struct{…}{…}` / `map[K]struct{…}{…}`), whose struct type is resolved
// by inference — that path previously emitted its interface fields unconverted, so a pointer form
// lost its adapter wrap and a value form used only in an elided literal was never recorded at all
// (errors' `wrap_test` `[]struct{err error; …}{{&poser{…}, …}, {errorUncomparable{}, …}}`).
func (v *Visitor) recordStructFieldInterfaceCasts(compositeLit *ast.CompositeLit, structType *types.Struct, callContext *CallExprContext) {
	for i := range structType.NumFields() {
		field := structType.Field(i)

		if i < len(compositeLit.Elts) {
			// Check if field is an embedded interface
			if fieldType := field.Type(); fieldType != nil {
				if needsInterfaceCast, isEmpty := isInterface(fieldType); needsInterfaceCast && !isEmpty {
					// Record implementation. A KEYED composite resolves the element by FIELD
					// NAME — blindly pairing Elts[0]'s value with EVERY interface field
					// recorded a BOGUS GoImplement (gif's `encoder{g: *g}`: field 0 is
					// `w writer`, the first value is a GIF → GoImplement<GIF, writer>,
					// CS1929 ×3 in the generated impl for methods GIF never had).
					var eltType types.Type
					eltIndex := i

					if _, ok := compositeLit.Elts[0].(*ast.KeyValueExpr); ok {
						eltIndex = -1

						for ei, elt := range compositeLit.Elts {
							if kv, ok := elt.(*ast.KeyValueExpr); ok {
								if keyIdent, ok := kv.Key.(*ast.Ident); ok && keyIdent.Name == field.Name() {
									eltType = v.info.TypeOf(kv.Value)
									eltIndex = ei
									break
								}
							}
						}
					} else {
						eltType = v.info.TypeOf(compositeLit.Elts[i])
					}

					if eltType != nil {
						_, eltIsStruct := eltType.Underlying().(*types.Struct)

						// A named NON-struct element that implements the interface field —
						// hpack's `DecodingError{InvalidIndexError(idx)}`, where
						// `type InvalidIndexError int` has an Error() method satisfying the
						// `error` field — must ALSO be recorded + routed, or it is passed bare
						// to the interface-typed constructor parameter (surfaces as NilType,
						// CS1503). The struct/embedded triggers are unchanged; this adds
						// method-set satisfaction for a named, non-struct, non-interface value
						// (the call-argument path already routes any arg into an interface param
						// without a struct-only restriction). An interface-typed element is
						// excluded — it is already the interface, so it needs no adapter.
						eltImplementsIface := false

						if !eltIsStruct && !field.Embedded() {
							// The concrete type carrying the satisfying method set: the element
							// itself, OR the pointee of a POINTER element whose pointer-receiver
							// methods satisfy the field — `&logLoggerLevel` (`*LevelVar`) feeding a
							// `Leveler` field in log/slog's `&handlerWriter{…, &logLoggerLevel, …}`,
							// where `*LevelVar` implements Leveler via a pointer-receiver `Level()`
							// (CS1503; no `GoImplement<LevelVar, Leveler>(Pointer = true)` was
							// recorded because this arm previously matched only a NAMED value, not a
							// pointer). types.Implements is tested on the ELEMENT type (the pointer
							// method set), while the non-interface guard tests the pointee.
							implSource := eltType

							if ptr, ok := eltType.(*types.Pointer); ok {
								implSource = ptr.Elem()
							}

							if named, ok := implSource.(*types.Named); ok {
								if _, eltIsIface := named.Underlying().(*types.Interface); !eltIsIface {
									if iface, ok := fieldType.Underlying().(*types.Interface); ok && types.Implements(eltType, iface) {
										eltImplementsIface = true
									}
								}
							}
						}

						if eltIsStruct || field.Embedded() || eltImplementsIface {
							v.convertToInterfaceType(fieldType, eltType, "")

							// Route the element through the interface conversion at RENDER
							// too — the record-only call above can miss paths that bypass
							// it, and the ctor's interface param cannot take the bare
							// struct without its partial (archive/tar's lifted
							// `struct{ io.Reader }{fr}`, CS1503 ×3).
							if eltIndex >= 0 {
								callContext.interfaceTypes[eltIndex] = fieldType
							}
						}
					}
				} else if ok := isPointer(fieldType); ok {
					callContext.argTypeIsPtr[i] = true
				}
			}
		}
	}
}

func (v *Visitor) convCompositeLit(compositeLit *ast.CompositeLit, context KeyValueContext) string {
	return v.convCompositeLitAs(compositeLit, nil, context)
}

// convCompositeLitAs renders a composite literal, optionally against a type the CALLER resolved
// rather than the one the literal's own TYPE SYNTAX names.
//
// Everything below the elided block reads the literal's type from `compositeLit.Type`, which an
// ELIDED literal does not have — that is the whole reason the elided block exists as a separate,
// smaller renderer. But the two are not equal in reach: the typed path carries the named-composite
// machinery (empty vs keyed vs positional, array vs slice vs map, alias vs named, the wrapper ctor),
// and elision is pure surface syntax, so a shape the typed path renders correctly must render
// identically when spelled elided. `elidedType` is how a caller that has RESOLVED the type reaches
// that one renderer instead of growing a second copy of it beside the first — see the NAMED
// non-struct pointee routing in the `*types.Pointer` arm below, which is its only caller today.
//
// Passing nil is the ordinary path and leaves every existing caller byte-identical: the elided block
// still runs, and the typed path still reads its type from the AST exactly as before.
func (v *Visitor) convCompositeLitAs(compositeLit *ast.CompositeLit, elidedType types.Type, context KeyValueContext) string {
	result := &strings.Builder{}

	// Go's all-or-nothing keying rule is a STRUCT-literal rule. An ARRAY or SLICE literal may MIX
	// positional and keyed elements (`[]byte{0xfe, 0x80, 15: 0x01}`), and every keyed path below
	// decides from Elts[0] alone. A mixed literal therefore took the plain positional emission
	// while its keyed elements still rendered through the key/value arm, whose sparse form wants a
	// target ident it does not have here: `new byte[]{0xfe, 0x80, <nil>[15] = 0x01}` — CS1525.
	// Normalizing the positional elements to their Go indices makes the literal all-keyed, so the
	// existing sparse-array machinery renders it and nothing else here needs a mixed-literal case.
	v.normalizeMixedKeyedComposite(compositeLit)

	// A caller-supplied type means this literal has already been resolved and is being rendered
	// THROUGH the typed path on purpose; the elided block would re-answer the same question with
	// the smaller renderer and defeat the delegation.
	if compositeLit.Type == nil && elidedType == nil {
		// An untyped (type-inferred) composite literal — e.g. the inner `{lockRankSysmon, …}` of a
		// `[][]lockRank{ key: {…} }`. The target-typed `new(…)` ctor form below is correct for a STRUCT
		// element type (the struct ctor takes the field values), but a SLICE/ARRAY element type has no
		// element-list ctor — `new slice<lockRank>(a, b, …)` is CS1729. Emit the element-array
		// projection (`new lockRank[]{…}.slice()` / `.array()`) for those, matching the typed path.
		if inferred := v.info.TypeOf(compositeLit); inferred != nil {
			// Go lets a composite literal elide the `&T` of an element (or map value) whose type is
			// `*T` — `[]*[4]byte{{}}` IS `[]*[4]byte{&[4]byte{}}` — so an elided literal can arrive
			// here with a POINTER inferred type. The `*types.Pointer` arm below renders the STRUCT
			// pointee, whose generated constructor takes the field values; an ARRAY, SLICE or MAP
			// pointee has no such constructor, matched no arm, and fell out of the switch onto the
			// struct-ctor fallback below — emitting a bare `new()` against the ABSTRACT `ж<T>`
			// (golib's box), i.e. CS0144, for a literal the explicit `&[4]byte{}` spelling converts
			// and compiles. Render the POINTEE through the arm that already renders that shape and
			// take its address, which is what the struct arm does one level up.
			//
			// A NAMED pointee is NOT rendered here, and the reason is a measurement rather than a
			// preference: `[]nb{{}}` (the same pointee, no `&`) emits the structural projection
			// `new nb[]{new byte[]{}.array(4)}`, which binds the named slot through the generated
			// implicit conversion — and that conversion is between VALUES, not between BOXES, so the
			// same projection with the address taken is a `ж<array<byte>>` and cannot bind a `ж<nb>`
			// slot. A named pointee is built by the wrapper ctor (`Ꮡ(new nb(new byte[4].array()))`),
			// which lives in the typed path's named-composite machinery, so it is ROUTED to that
			// renderer in the `*types.Pointer` arm below rather than copied into one here.
			addrOf := func(expr string) string { return expr }

			if ptr, isPtr := inferred.Underlying().(*types.Pointer); isPtr {
				if _, isNamed := types.Unalias(ptr.Elem()).(*types.Named); !isNamed {
					switch ptr.Elem().Underlying().(type) {
					case *types.Array, *types.Slice, *types.Map:
						inferred = ptr.Elem()
						addrOf = func(expr string) string { return fmt.Sprintf("%s(%s)", AddressPrefix, expr) }
					}
				}
			}

			switch u := inferred.Underlying().(type) {
			case *types.Slice:
				csElem := convertToCSTypeName(v.getAliasQualifiedTypeName(u.Elem(), false))
				// A KEYED elided slice literal (`{5: "x"}` inside a `[]map[…][]T{…}`) renders as a
				// golib SparseArray with `[index] = value` elements; the plain `new T[]{…}` array
				// initializer below cannot take the Go `key: value` keyed syntax (CS1003 cascade).
				// Mirrors the typed slice sparse-array path (see keyValueSource==ArraySource below).
				if compositeLitIsKeyed(compositeLit.Elts) {
					return addrOf(fmt.Sprintf("new golib.SparseArray<%s>{%s}.slice()", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, sparseArrayCompositeContext(inferred, compositeLit.Elts))))
				}
				return addrOf(fmt.Sprintf("new %s[]{%s}.slice()", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, v.withValueCloneArgs(compositeLit.Elts, v.elidedPointerElemContext(u.Elem(), compositeLit.Elts)))))
			case *types.Array:
				csElem := convertToCSTypeName(v.getAliasQualifiedTypeName(u.Elem(), false))
				// An ELIDED array literal is still `[N]T` long, so its projection carries the
				// declared length whenever the literal writes fewer elements — the same zero-fill
				// rule the typed path applies below (see the padding comment there). A KEYED
				// elided literal always needs it: SparseArray's own extent is `max index + 1`.
				//
				// The length ALONE is not enough, for the same reason the typed path below spells
				// out: when the element's own zero value must be CONSTRUCTED — another unnamed
				// fixed array (`[][2][3]int{{}}`, whose inner length lives only in the Go type and
				// never in `array<T>`) or a struct whose fixed-array field initializer runs only
				// inside a declared constructor — padding with `default(T)` sizes the OUTER
				// dimension and leaves every zero-filled element unusable storage: `len(x[0][0])`
				// reported 0 where Go says 3, and reflect measured `[2][0]`. This arm was the one
				// renderer of the four `arrayLengthArgs` documents that never carried the factory,
				// so the ELIDED and TYPED spellings of one Go type disagreed — exactly the split
				// that fix closed between the literal and the declaration. Reuse that ladder rather
				// than a second copy of the rule; the factory is empty for every element whose
				// `default(T)` is already the Go zero value, so every existing golden is unchanged.
				elidedArrayArgs := ""

				if int64(len(compositeLit.Elts)) < u.Len() {
					elidedArrayArgs = arrayLengthArgs(strconv.FormatInt(u.Len(), 10), v.arrayElemFactory(u.Elem()))
				}

				// A KEYED elided ARRAY literal — the inner `{joiningL: stateBefore, …}` of a
				// `[][numJoinTypes]joinState{stateStart: {…}}` (x/net/idna joinStates, CS1003 ×62).
				// Same SparseArray treatment as the slice case; `.array()` materializes the dense
				// fixed-length backing, matching the typed array sparse path.
				if compositeLitIsKeyed(compositeLit.Elts) {
					return addrOf(fmt.Sprintf("new golib.SparseArray<%s>{%s}.array(%s)", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, sparseArrayCompositeContext(inferred, compositeLit.Elts)), arrayLengthArgs(strconv.FormatInt(u.Len(), 10), v.arrayElemFactory(u.Elem()))))
				}
				return addrOf(fmt.Sprintf("new %s[]{%s}.array(%s)", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, v.withValueCloneArgs(compositeLit.Elts, v.elidedPointerElemContext(u.Elem(), compositeLit.Elts))), elidedArrayArgs))
			case *types.Pointer:
				// An untyped composite whose inferred type is `*Struct` — the `[]*T{ {…} }` shorthand
				// for `&T{…}` (e.g. runtime's `dbgvars = []*dbgVar{ {name, &debug.x}, … }`). Emit the
				// boxed struct constructor `Ꮡ(new T(field: val, …))`; a bare `new(…)` targets the box
				// `ж<T>`, whose constructor has no such field params (CS1739).
				//
				// A NAMED non-struct pointee (`type nb [4]byte; []*nb{{}}`) is the same shorthand one
				// type-kind over, and it is the one pointee this switch cannot render itself: its
				// value is built by the generated WRAPPER ctor, which is the typed path's
				// named-composite machinery (empty vs keyed vs positional, array vs slice vs map,
				// alias vs named), not the structural projection the arms above emit. Left here it
				// matched nothing, fell out of the switch onto the struct-ctor fallback and emitted a
				// bare `new()` against the abstract `ж<T>` — CS0144, for a value whose explicit
				// `&nb{}` spelling has always converted and compiled. Route it THROUGH that renderer
				// with the pointee supplied as the resolved type, so the two spellings reach one
				// renderer and cannot drift; the address is taken here exactly as the struct arm
				// below takes it. A named STRUCT pointee is excluded because the arm below already
				// renders it, and a named pointee whose underlying is a basic/pointer/chan type has
				// no composite literal to render at all.
				if named, isNamed := types.Unalias(u.Elem()).(*types.Named); isNamed {
					switch named.Underlying().(type) {
					case *types.Array, *types.Slice, *types.Map:
						return fmt.Sprintf("%s(%s)", AddressPrefix, v.convCompositeLitAs(compositeLit, u.Elem(), context))
					}
				}

				if _, ok := u.Elem().Underlying().(*types.Struct); ok {
					structName := v.getCSharpTypeName(u.Elem())

					// Thread the pointed-to struct type so a keyed field named like its OWN struct type
					// takes the CS0542 type-colliding rename in the generated ctor (net/mail's
					// `[]*Address{{Address: …}}` kept the unrenamed `Address:` key, CS1739). Set
					// u8StringArgOK per element to match the nil-context default (an empty map would
					// silently strip the u8 suffix from string-literal element values).
					ptrElidedContext := DefaultCallExprContext()
					ptrElidedContext.keyValueCompositeType = u.Elem()

					for i := range compositeLit.Elts {
						ptrElidedContext.u8StringArgOK[i] = true
					}

					// A POSITIONAL string-literal element in an `any` field slot boxes through
					// @string — the u8 span set above has no conversion to the ctor's object
					// parameter (CS1503).
					if st, ok := u.Elem().Underlying().(*types.Struct); ok {
						v.markAnyFieldLits(st, compositeLit.Elts, ptrElidedContext)

						// Record + route an INTERFACE struct field, exactly as the sibling composite
						// paths do (the typed checkStructFields and the elided STRUCT arm below).
						// This arm marked `any` fields but never interface ones, so a concrete
						// element in an interface slot reached the generated ctor bare — net
						// ip_test's `[]*struct{ in IP; str string; byt []byte; error }` handed a
						// `ж<AddrError>` to the embedded `error` parameter with no `AddrErrorжerror`
						// wrap (CS1503). The `[]struct{…}` sibling shape routed correctly all along.
						v.recordStructFieldInterfaceCasts(compositeLit, st, ptrElidedContext)
					}

					v.withValueCloneArgs(compositeLit.Elts, ptrElidedContext)

					return fmt.Sprintf("%s(new %s(%s))", AddressPrefix, structName, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, ptrElidedContext))
				}
			case *types.Map:
				// A MAP element type — the inner `{"domain": 53}` of a
				// `map[string]map[string]int{…}` (net lookup.go) — takes the map
				// collection-initializer form with `[key] = value` elements; the `new(…)` ctor
				// fallback rendered STRUCT-style named args (`"domain"u8: 53`, a 30-error
				// syntax cascade).
				mapContext := DefaultCallExprContext()
				mapContext.keyValueSource = MapSource
				mapContext.keyValueCompositeType = inferred

				for i := range compositeLit.Elts {
					mapContext.u8StringArgOK[i] = true
				}

				keyCS := convertToCSTypeName(v.getAliasQualifiedTypeName(u.Key(), false))
				valCS := convertToCSTypeName(v.getAliasQualifiedTypeName(u.Elem(), false))

				return addrOf(fmt.Sprintf("new map<%s, %s>{%s}", keyCS, valCS, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, mapContext)))
			}
		}

		rparenSuffix := ""

		if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].End(), compositeLit.Rbrace) {
			rparenSuffix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
		}

		// Thread the ELIDED literal's resolved type to the keyed-field emission so a field
		// named like its own struct type detects the declaration's type-colliding rename
		// (runtime/metrics `{Description: …}` inside `[]Description{…}`, CS1739 ×56).
		elidedContext := DefaultCallExprContext()
		elidedContext.keyValueCompositeType = v.info.TypeOf(compositeLit)

		// The nil-context path this replaces defaulted u8StringOK true per element; an empty
		// map defaults it FALSE, silently stripping the u8 suffix from string-literal elements.
		for i := range compositeLit.Elts {
			elidedContext.u8StringArgOK[i] = true
		}

		// A POSITIONAL string-literal element in an `any` field slot boxes through @string —
		// the u8 span set above has no conversion to the ctor's object parameter (CS1503).
		if inferred := elidedContext.keyValueCompositeType; inferred != nil {
			if st, ok := inferred.Underlying().(*types.Struct); ok {
				v.markAnyFieldLits(st, compositeLit.Elts, elidedContext)

				// Record + route an interface STRUCT FIELD exactly as the typed path does — an
				// ELIDED element composite (`{&poser{…}, err1, true}` inside a `[]struct{err
				// error; …}{…}`) resolves its struct type by inference, so without this its
				// pointer fields lost the `new <T>ж<Iface>(…)` adapter wrap and a value form
				// used only here was never recorded at all (errors' wrap_test, CS1503 ×17).
				v.recordStructFieldInterfaceCasts(compositeLit, st, elidedContext)
			}
		}

		v.withValueCloneArgs(compositeLit.Elts, elidedContext)

		result.WriteString(fmt.Sprintf("new(%s", v.convExprList(compositeLit.Elts, compositeLit.Lbrace, elidedContext)))
		v.writeStandAloneCommentString(result, compositeLit.Rbrace, nil, " ")
		result.WriteString(fmt.Sprintf("%s)", rparenSuffix))

		return result.String()
	}

	var name string

	if context.ident != nil {
		if len(context.ident.Name) > 0 {
			name = context.ident.Name
		}
	}

	if len(name) == 0 {
		name = "type"
	}

	var indentOffset int

	if v.inFunction {
		indentOffset = 1
	}

	// Check if the composite type is a struct or pointer to a struct
	if structType, exprType := v.extractStructType(compositeLit.Type); structType != nil && !v.liftedTypeExists(structType) {
		v.indentLevel += indentOffset
		v.visitStructType(structType, exprType, name, nil, true, nil)
		v.indentLevel -= indentOffset
	}

	// Check if the composite type is an anonymous interface
	if interfaceType, exprType := v.extractInterfaceType(compositeLit.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
		v.indentLevel += indentOffset
		v.visitInterfaceType(interfaceType, exprType, name, nil, true, nil)
		v.indentLevel -= indentOffset
	}

	// The literal's type comes from its TYPE SYNTAX, except when a caller resolved it for us (an
	// elided literal routed here — see convCompositeLitAs). `extractStructType`/`extractInterfaceType`
	// above are already nil-safe on the missing syntax node (typeSyntaxOf falls through and
	// firstAnonymousTypeLiteral returns on nil), and a routed literal names a type that exists, so
	// neither has an anonymous struct or interface to lift.
	exprType := elidedType

	if exprType == nil {
		exprType = v.getExprType(compositeLit.Type)
	}

	arrayTypeContext := DefaultArrayTypeContext()
	callContext := DefaultCallExprContext()
	// Thread the enclosing statement's hoist target through the composite so a
	// func-literal FIELD value's capture decls hoist (see KeyValueContext.deferredDecls).
	callContext.deferredDecls = context.deferredDecls
	callContext.keyValueIdent = context.ident

	// Every POSITIONAL element that reads an ARRAY value out of existing storage clones into
	// its slot — Go copies the array into array/slice elements and struct fields alike (keyed
	// elements clone in convKeyValueExpr).
	v.withValueCloneArgs(compositeLit.Elts, callContext)

	// Thread the composite's RESOLVED type to the keyed-field emission so a field named like
	// its OWN struct type detects the declaration's type-colliding rename (runtime/metrics
	// `Description{Description: …}`, CS1739 ×57). Derived from the LITERAL, not its Type
	// syntax — an elided element literal (`{Name: …}` inside `[]Description{…}`) has a nil
	// Type node but still resolves contextually.
	if tv, ok := v.info.Types[compositeLit]; ok && tv.Type != nil {
		if _, ok := tv.Type.Underlying().(*types.Struct); ok {
			callContext.keyValueCompositeType = tv.Type
		}
	}

	compositeSuffix := ""

	// Get the element type for arrays/slices/maps to check for interfaces
	var elementType types.Type
	var needsInterfaceCast bool
	var isEmpty bool
	var definedLen int
	var namedArrayComposite bool
	var namedMapComposite bool
	var namedMapRender string
	var aliasArrayComposite bool
	var namedStructWrapRender string

	// Check composite lit elements against struct fields
	checkStructFields := func(structType *types.Struct) {
		// A POSITIONAL string-literal element in a plain `string` field slot renders as the `u8`
		// span, which binds the generated constructor's `@string` parameter through one implicit
		// conversion. KEYED elements already emit `u8` (they route through convKeyValueExpr), so
		// this makes positional match; without it the element stayed a bare C# string that
		// `Encoding.UTF8.GetBytes` transcoded on EVERY evaluation — `new StructuralError("empty
		// integer")` and its 82 siblings across 9 types were the corpus's last converter-emitted
		// bare-UTF-16 literal class in a constructor position. `any` fields keep the boxed
		// `(@string)"…"u8` form markAnyFieldLits selects (it runs after and overrides).
		v.markStringFieldLits(structType, compositeLit.Elts, callContext)

		// A POSITIONAL string-literal element in an `any` field slot boxes through @string
		// (keyed elements take convKeyValueExpr's `any`-field arm instead).
		v.markAnyFieldLits(structType, compositeLit.Elts, callContext)

		v.recordStructFieldInterfaceCasts(compositeLit, structType, callContext)

		// A NAMED FUNC-type field initialized with a value of a DIFFERENT delegate type must
		// wrap in the target delegate's constructor — C# has no implicit conversion between
		// distinct delegate types (internal/concurrent's `keyHash: mapType.Hasher` feeding a
		// hashFunc field from a Func<…,…> field, CS1503 ×3). The MIRROR direction is handled
		// too, matching the call-site rule (see convCallExpr): a STRUCTURAL func-type field
		// receiving a value that RENDERS as a named delegate re-wraps in the synthesized
		// structural delegate (sort example_keys_test's planetSorter `by: by` — the `By` value
		// against the written `func(p1, p2 *Planet) bool` field, CS1503). Keyed-aware: elements
		// resolve their field by NAME, so a reordered/partial literal maps correctly. FuncLit
		// values (anonymous-method conversion applies), method groups (convert natively), and
		// nil stay bare.
		fieldByName := map[string]*types.Var{}

		for i := range structType.NumFields() {
			fieldByName[structType.Field(i).Name()] = structType.Field(i)
		}

		// A struct instantiated over a constraint proxy (nistCurve<P224PointжnistPoint>) renders its
		// Point-typed FUNC fields with the proxy (`Func<P224PointжnistPoint> newPoint`), so a
		// method-group / func-value initializer needs a lambda re-wrap below (see wrapArgWithLambda).
		compositeHasProxy := false

		if litNamed, ok := types.Unalias(exprType).(*types.Named); ok {
			compositeHasProxy = v.namedHasConstraintProxy(litNamed)
		}

		for i, elt := range compositeLit.Elts {
			var fieldVar *types.Var
			var valueExpr ast.Expr

			if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
				if keyIdent, ok := keyValue.Key.(*ast.Ident); ok {
					fieldVar = fieldByName[keyIdent.Name]
				}

				valueExpr = keyValue.Value
			} else {
				if i < structType.NumFields() {
					fieldVar = structType.Field(i)
				}

				valueExpr = elt
			}

			if fieldVar == nil || valueExpr == nil {
				continue
			}

			// FUNC field of a constraint-proxy instantiation: re-wrap the initializer as a lambda so
			// the proxy delegate position applies the ж↔proxy conversion a method-group conversion
			// can't (nistCurve's `newPoint: nistec.NewP224Point` → `() => nistec.NewP224Point()`,
			// CS0407). A FuncLit initializer already targets the proxy return, and nil stays bare.
			if compositeHasProxy {
				if sig, isSig := fieldVar.Type().Underlying().(*types.Signature); isSig {
					if _, isLit := valueExpr.(*ast.FuncLit); !isLit {
						if tv, isNil := v.info.Types[valueExpr]; !isNil || !tv.IsNil() {
							if callContext.wrapArgWithLambda == nil {
								callContext.wrapArgWithLambda = make(map[int]string)
							}

							params := make([]string, sig.Params().Len())

							for p := range params {
								params[p] = fmt.Sprintf("%sp%d", ShadowVarMarker, p)
							}

							callContext.wrapArgWithLambda[i] = strings.Join(params, ", ")
							continue
						}
					}
				}
			}

			if _, isLit := valueExpr.(*ast.FuncLit); isLit {
				continue
			}

			if tv, ok := v.info.Types[valueExpr]; ok && tv.IsNil() {
				continue
			}

			valueType := v.info.TypeOf(valueExpr)

			if valueType == nil {
				continue
			}

			wrapDelegate := false

			if named, ok := types.Unalias(fieldVar.Type()).(*types.Named); ok {
				// NAMED func-type field ← any different delegate type.
				if _, isSig := named.Underlying().(*types.Signature); isSig {
					wrapDelegate = !types.Identical(types.Unalias(valueType), types.Unalias(fieldVar.Type()))
				}
			} else if _, isSig := types.Unalias(fieldVar.Type()).(*types.Signature); isSig && !typeContainsTypeParams(fieldVar.Type()) {
				// The MIRROR: STRUCTURAL func-type field ← value rendering as a named delegate.
				// Same two named-rendering shapes as the call-site rule (convCallExpr): a value
				// whose GO type is a named func type with methods (a METHODLESS one already
				// renders as the structural delegate itself and stays bare), and a `:=` local
				// declared from a method group (typed with the matching package named delegate
				// by visitAssignStmt's methodGroupDelegateType). Method groups and func literals
				// themselves convert natively and are excluded by both gates.
				if argNamed, ok := types.Unalias(valueType).(*types.Named); ok {
					if _, argIsSig := argNamed.Underlying().(*types.Signature); argIsSig {
						if _, collapses := methodlessNamedFuncSignature(argNamed); !collapses {
							wrapDelegate = true
						}
					}
				} else if argSig, ok := types.Unalias(valueType).(*types.Signature); ok {
					if argIdent, ok := valueExpr.(*ast.Ident); ok &&
						v.namedFuncTypeNameForSignature(argSig) != "" && v.identDeclaredFromMethodGroup(argIdent) {
						wrapDelegate = true
					}
				}
			}

			if !wrapDelegate {
				continue
			}

			if callContext.wrapArgWithNew == nil {
				callContext.wrapArgWithNew = make(map[int]string)
			}

			callContext.wrapArgWithNew[i] = v.getCSharpTypeName(fieldVar.Type())
		}
	}

	// Dispatch on the UNALIASED type (*types.Alias, Go 1.22+): an alias to an unnamed array
	// (fiat's `type p224UntypedFieldElement = [4]uint64`) matched no arm, so the literal kept
	// C# collection-initializer braces on the alias name (`new words{…}` — CS1061, array<T>
	// has no Add). An alias renders through its name (an Ident, not an ast.ArrayType), so the
	// array/slice arms also flag the typeRender swap to the element-array projection form the
	// unnamed literal uses (`new uint64[]{…}.array()`). An alias to a NAMED type falls into
	// the *types.Named arm and keeps the alias name (same wrapper struct either way).
	_, isAliasType := exprType.(*types.Alias)

	switch t := types.Unalias(exprType).(type) {
	case *types.Array:
		elementType = t.Elem()
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".array()"
		definedLen = int(t.Len())
		aliasArrayComposite = isAliasType
	case *types.Slice:
		elementType = t.Elem()
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".slice()"
		aliasArrayComposite = isAliasType
	case *types.Map:
		elementType = t.Elem()
		callContext.keyValueSource = MapSource
		// Carry the map type so convKeyValueExpr can box a pointer-typed VALUE (a bare-ident pointer
		// into a `ж<T>` map slot is CS0029, like the struct-field pointer case).
		callContext.keyValueCompositeType = t
	case *types.Named:
		if structType, ok := t.Underlying().(*types.Struct); ok {
			checkStructFields(structType)

			// A DEFINED type over a NAMED STRUCT type (`type decoder coder`) is emitted as
			// a WRAPPER whose only ctor takes the underlying (`decoder(coder value)`) — its
			// composite literal constructs the UNDERLYING and wraps:
			// `new decoder(new coder(order: …))` (encoding/binary, CS1739 ×5). A type
			// written directly over a struct LITERAL keeps its own keyed ctor (unchanged).
			if rhs, okRHS := packageTypeSpecRHS[t.Obj()]; okRHS && rhs != nil {
				if rhsNamed, ok := types.Unalias(rhs).(*types.Named); ok {
					if _, isStruct := rhsNamed.Underlying().(*types.Struct); isStruct {
						namedStructWrapRender = convertToCSTypeName(v.getAliasQualifiedTypeName(rhsNamed, false))
					}
				}
			}
		} else if arrayType, ok := t.Underlying().(*types.Array); ok {
			// A named array type (e.g. `type d [3]rune`) lowers to a struct wrapping
			// array<T>; its composite literal cannot use C# collection-initializer braces
			// (no Add). Emit the underlying array literal wrapped in the named ctor:
			// `new d(new rune[]{...}.array())`.
			elementType = arrayType.Elem()
			callContext.keyValueSource = ArraySource
			arrayTypeContext.compositeInitializer = true
			compositeSuffix = ".array()"
			definedLen = int(arrayType.Len())
			namedArrayComposite = true
		} else if sliceType, ok := t.Underlying().(*types.Slice); ok {
			// A named slice type (e.g. `type s []int`) lowers to a struct wrapping
			// slice<T>; same treatment, with the slice constructor form.
			elementType = sliceType.Elem()
			callContext.keyValueSource = ArraySource
			arrayTypeContext.compositeInitializer = true
			compositeSuffix = ".slice()"
			namedArrayComposite = true
		} else if mapType, ok := t.Underlying().(*types.Map); ok {
			// A named map type's composite literal wraps the CONCRETE map literal in the
			// named ctor — `new Grades(new map<@string, nint>{["a"u8] = 1})` — mirroring
			// named arrays/slices. The wrapper struct's default instance has no backing
			// dictionary for a direct indexer-initializer, and without this arm the keys
			// emitted Go-style (`"a"u8: 1` inside C# braces — CS1513/CS1002).
			elementType = mapType.Elem()
			callContext.keyValueSource = MapSource
			// Carry the map type, exactly as the UNNAMED map arm above does: every MapSource
			// key/value slot rule in convKeyValueExpr is gated on it (a pointer KEY or VALUE boxes
			// to `Ꮡx`, an `any` key or value slot re-renders a string literal as `(@string)"…"u8`,
			// an untyped-constant `any` key boxes at Go's default type, an array key clones). Left
			// nil, a named map type silently opted out of all of them: `namedAny{nil: 1, "b": 2}`
			// over `map[any]int` emitted a bare `["b"u8]` span key, which has no conversion to the
			// object key slot (CS1503), while the identical unnamed literal one line above got
			// `[(@string)"b"u8]`.
			callContext.keyValueCompositeType = mapType
			namedMapComposite = true
			namedMapRender = fmt.Sprintf("map<%s, %s>", convertToCSTypeName(v.getAliasQualifiedTypeName(mapType.Key(), false)), convertToCSTypeName(v.getAliasQualifiedTypeName(mapType.Elem(), false)))
		}
	case *types.Struct:
		checkStructFields(t)
	}

	// Check if element type is an interface
	if elementType != nil {
		needsInterfaceCast, isEmpty = isInterface(elementType)
		if needsInterfaceCast && !isEmpty {
			for i := range compositeLit.Elts {
				callContext.interfaceTypes[i] = elementType
			}
		}

		// A POINTER element type renders a bare-ident element as the pointer VALUE — the box
		// (`Ꮡc`), not a deref'd receiver ref-local (`c`) — mirroring the struct-field pointer
		// routing below and the call-argument pointer arm (convCallExpr): `[]*CommentGroup{c}`
		// inside a method that deref-aliases its `c *CommentGroup` parameter otherwise emits
		// the VALUE alias into a `ж<CommentGroup>[]` array (go/ast addComment, CS0029). Gated
		// to bare idents of pointer type: keyed elements (maps) and address-of/composite
		// elements manage their own pointer rendering.
		if _, ok := elementType.Underlying().(*types.Pointer); ok {
			for i, elt := range compositeLit.Elts {
				if ident, ok := elt.(*ast.Ident); ok {
					if _, isPtr := v.getType(ident, false).(*types.Pointer); isPtr {
						callContext.argTypeIsPtr[i] = true
					}
				}
			}
		}

		// A STRING element type takes the `u8` span rendering for its string-literal elements —
		// `new @string[]{"…"u8}` — instead of the bare UTF-16 C# string the flag-less TYPED path
		// leaves, which re-transcodes through Encoding.UTF8.GetBytes at every evaluation. The span
		// binds the element slot through @string's implicit ReadOnlySpan<byte> conversion (a NAMED
		// type over string converts the same way, through its generated span conversion), so this
		// is the element twin of markStringFieldLits. An ELIDED literal already renders u8 (its
		// nil element context defaults u8StringOK on); this closes the typed half.
		// KeyValueExpr elements (maps, sparse arrays) are not BasicLits and route through
		// convKeyValueExpr instead.
		//
		// The flag is set for EVERY element, not just the BasicLits, because it doubles as the
		// slot's span-tolerance signal: convExprList derives spanTargetUnsupported from it, and a
		// false there makes convBinaryExpr suppress u8 on a string CONCAT element's literal
		// operand (`[]string{prefix + "-a"}` rendered `prefix + "-a"`, paying a GetBytes transcode
		// plus a throwaway intermediate @string on every evaluation, where `+ "-a"u8` binds
		// golib's `operator +(@string, ReadOnlySpan<byte>)` and block-copies ROM bytes straight
		// into the single result buffer). That suppression is meant for span-HOSTILE slots — an
		// `object[]` vararg cannot box a ReadOnlySpan<byte> — but a string element slot is not one,
		// which the sibling spellings already proved: the same concat rendered u8 when
		// parenthesized (convParenExpr drops the incoming literal context) or when the composite's
		// type was ELIDED (nil element context). Setting it unconditionally makes the three agree.
		// Harmless for the other element shapes: u8StringOK only reaches BasicLit rendering, and
		// castToGoString consults the flag only on the deferred-call path (callArgs != nil), which
		// no composite literal takes.
		if basic, ok := elementType.Underlying().(*types.Basic); ok && basic.Kind() == types.String {
			for i := range compositeLit.Elts {
				callContext.u8StringArgOK[i] = true
			}
		}

		// An EMPTY-interface element type takes no adapter wrap (excluded above), but a
		// string-literal element must still box through @string — `new any[]{(@string)"a"u8}` —
		// instead of the bare C# string the flag-less path leaves, which boxes the wrong type
		// (a later Go x.(string) assertion fails). The `u8` half keeps the bytes a compile-time
		// constant (the cast is what makes the span boxable, exactly as markAnyFieldLits does for
		// the struct-field twin). An untyped-CONSTANT element boxes through Go's
		// default type for its kind for the same reason (`new any[]{(nint)(4)}`, else a later
		// x.(int) fails) — the numeric twin, applied via the same per-element castArgToType
		// plumbing convExprList honors. KeyValueExpr elements (maps, sparse arrays) are not
		// BasicLits and route through convKeyValueExpr instead.
		if isEmptyInterfaceTarget(elementType) {
			for i, elt := range compositeLit.Elts {
				if isStringBasicLit(elt) {
					callContext.u8StringArgOK[i] = true
					callContext.useGoStringArg[i] = true
				} else if castType := v.untypedConstBoxCast(elt); castType != "" {
					if callContext.castArgToType == nil {
						callContext.castArgToType = make(map[int]string)
					}

					callContext.castArgToType[i] = castType
				} else if castType := v.variadicFuncBoxCastType(v.getType(elt, false)); castType != "" {
					// A VARIADIC func element boxes as C#'s SYNTHESIZED anonymous delegate
					// unless it is cast to its Go func type — the third member of this same
					// carry-your-Go-type family, applied through the same plumbing (see
					// typedNilInterfaceBoxing.go). `[]any{escaper}` read back as
					// `slots[0].(func(...any) string)` cannot match without it.
					if callContext.castArgToType == nil {
						callContext.castArgToType = make(map[int]string)
					}

					callContext.castArgToType[i] = castType
				}

				// A POINTER element crosses into interface space as its BOX, carrying its Go
				// type — the same boundary the call-argument arm applies (see
				// typedNilInterfaceBoxing.go); both halves are consumed in convExprList.
				if _, eltIsPtr := v.getType(elt, false).(*types.Pointer); eltIsPtr {
					callContext.argTypeIsPtr[i] = true
					callContext.anyBoxedPtrArgs[i] = true
				}

				// The FUNC sibling of the arm above, and the one slot of this family that was
				// measurably WRONG rather than merely unreached: `[]any{nilFunc}` emitted a bare
				// null, so the element compared equal to nil where Go — whose interface holds
				// (func-type, nil) — says it does not. Measured `true false` against Go's
				// `false false`, with the map-VALUE slot beside it already correct.
				if eltType := v.getType(elt, false); eltType != nil {
					if _, eltIsFunc := eltType.Underlying().(*types.Signature); eltIsFunc {
						callContext.anyBoxedFuncArgs[i] = true
					}
				}
			}
		}
	}

	// A NARROW-INTEGER element type (int8/uint8/int16/uint16) receiving a binary/unary
	// arithmetic element: Go evaluates `b/100 + '0'` at the element's narrow width (with
	// overflow wrapping), but C# promotes sub-int integer arithmetic to `int`, so a
	// NON-CONSTANT element needs an explicit cast back to the element type — both to compile
	// (int→narrow is not implicit, CS0266) and to preserve Go's wrap semantics. This is the
	// composite-literal twin of the narrow-integer call-argument cast (see convCallExpr),
	// which is why the equivalent `append(buf, b/100+'0')` form already compiles. Gated on
	// the element's Go type already matching the element type (so Go accepts it without a
	// conversion) and on it being an arithmetic expression — a bare ident is already the
	// narrow type, and a constant literal element gets C#'s implicit constant-expression
	// conversion. Key-value elements (sparse arrays / maps) are not ast.BinaryExpr/UnaryExpr,
	// so they never match.
	if elementType != nil && callContext.keyValueSource == ArraySource {
		if elemBasic, ok := elementType.Underlying().(*types.Basic); ok && isNarrowIntegerKind(elemBasic.Kind()) {
			csElemType := convertToCSTypeName(v.getAliasQualifiedTypeName(elementType, false))

			for i, elt := range compositeLit.Elts {
				switch elt.(type) {
				case *ast.BinaryExpr, *ast.UnaryExpr:
					if eltType := v.getType(elt, false); eltType != nil && types.Identical(eltType, elementType) {
						if callContext.castArgToType == nil {
							callContext.castArgToType = make(map[int]string)
						}

						callContext.castArgToType[i] = csElemType
					}
				}
			}
		}
	}

	// STRUCT-composite twin of the narrow-integer element cast above: each element maps to a
	// FIELD whose type may be a narrow integer (image/png's `color.Gray{(b >> 7) * 0xff}`,
	// field Y uint8). Same rationale — C# promotes sub-int arithmetic to int, so a
	// non-constant arithmetic element needs an explicit cast back to the field type (CS1503 +
	// Go wrap semantics). Positional and keyed both resolve their field; a bare ident / literal
	// element is already the field's width so it never matches.
	if st, ok := exprType.Underlying().(*types.Struct); ok {
		fieldByName := map[string]*types.Var{}

		for i := range st.NumFields() {
			fieldByName[st.Field(i).Name()] = st.Field(i)
		}

		for i, elt := range compositeLit.Elts {
			var fieldVar *types.Var
			var valueExpr ast.Expr

			if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
				if keyIdent, ok := keyValue.Key.(*ast.Ident); ok {
					fieldVar = fieldByName[keyIdent.Name]
				}

				valueExpr = keyValue.Value
			} else if i < st.NumFields() {
				fieldVar = st.Field(i)
				valueExpr = elt
			}

			if fieldVar == nil || valueExpr == nil {
				continue
			}

			elemBasic, ok := fieldVar.Type().Underlying().(*types.Basic)

			if !ok || !isNarrowIntegerKind(elemBasic.Kind()) {
				continue
			}

			switch valueExpr.(type) {
			case *ast.BinaryExpr, *ast.UnaryExpr:
				if vt := v.getType(valueExpr, false); vt != nil && types.Identical(vt, fieldVar.Type()) {
					if callContext.castArgToType == nil {
						callContext.castArgToType = make(map[int]string)
					}

					callContext.castArgToType[i] = convertToCSTypeName(v.getAliasQualifiedTypeName(fieldVar.Type(), false))
				}
			}
		}
	}

	// A ONE-FIELD struct's positional literal carrying `nil` — `testClose{nil}` (archive/tar's
	// writer_test.go), `stubDriverStmt{nil}` (database/sql). The universe nil renders in a value
	// context as the TYPELESS `default!`, which cannot take part in C# overload resolution, and the
	// generated struct offers exactly two one-argument constructors: `T(NilType)` and the field
	// constructor `T(F field = default!)`. `default!` converts to both, so the call is CS0121 —
	// ambiguous, ×9 in archive/tar alone. Give the argument the field's type so it names the field
	// constructor and nothing else.
	//
	// Only a struct with ONE field can hit this: Go requires a positional literal to list every
	// field in order, so any other arity already differs from the NilType constructor's. And only
	// `nil` can: every other element renders with a type of its own. Nothing else moves.
	if st, ok := exprType.Underlying().(*types.Struct); ok && st.NumFields() == 1 && len(compositeLit.Elts) == 1 {
		if ident, isIdent := compositeLit.Elts[0].(*ast.Ident); isIdent && v.identIsUniverseNil(ident) {
			if _, isPointerField := st.Field(0).Type().Underlying().(*types.Pointer); !isPointerField {
				if callContext.castArgToType == nil {
					callContext.castArgToType = make(map[int]string)
				}

				callContext.castArgToType[0] = convertToCSTypeName(v.getAliasQualifiedTypeName(st.Field(0).Type(), false))
			}
		}
	}

	var lbracePrefix, rbracePrefix string
	lbrace := "{"
	rbrace := "}"

	// A struct composite uses the constructor form `new T(field: v)`. Test the UNDERLYING type so
	// this also covers a type ALIAS to a struct (`type name = abi.Name`, a *types.Alias in Go 1.22+,
	// which is neither *types.Named nor *types.Struct) — otherwise it would wrongly keep the `{`
	// object-initializer braces and emit the un-compilable Go form `new name{field: v}`.
	if exprType != nil {
		if _, ok := exprType.Underlying().(*types.Struct); ok {
			lbrace = "("
			rbrace = ")"
		}
	}

	if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].Pos(), compositeLit.Rbrace) {
		rbracePrefix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
	}

	// Check for sparse array initialization
	if callContext.keyValueSource == ArraySource && len(compositeLit.Elts) > 0 {
		if _, ok := compositeLit.Elts[0].(*ast.KeyValueExpr); ok {
			callContext.keyValueSource = MapSource
			// This is an indexed slice/array literal emitted as a SparseArray (int-indexed), NOT a
			// real map — record it so a defined-integer-type key is cast to the int index type.
			callContext.keyValueArrayBacked = true
			// Thread the array/slice type so convKeyValueExpr's MapSource value slot can see an
			// EMPTY-interface element type (a sparse `[N]any{i: "v"}` value boxes through @string).
			callContext.keyValueCompositeType = exprType
			maxKeyValue := 0
			// Whether every key folded to a literal index. This is TRACKED SEPARATELY from
			// maxKeyValue because 0 is a legal index, not a "no constant keys" sentinel: a
			// literal whose only key IS 0 (`[8]byte{0: 1}`) read as "unresolved" and fell to
			// the SparseArray projection, whose Count is `max index + 1` (1) rather than the
			// array's DECLARED length (8) - the same dropped-length defect as the positional
			// form below, reached by a different route.
			constKeys := true

			for _, elt := range compositeLit.Elts {
				if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
					if basicLit, ok := keyValue.Key.(*ast.BasicLit); ok {
						// Check for rune literal — DECODE it (escapes and multi-byte runes): byte
						// [0] of the unquoted text read the BACKSLASH of an escape sequence (92),
						// so every escaped key (`'\t': 1`, bytes asciiSpace) corrupted to '\' — a
						// CS1012 syntax cascade across bytes/strings/os/fmt.
						if strings.HasPrefix(basicLit.Value, "'") && strings.HasSuffix(basicLit.Value, "'") {
							if r, _, _, err := strconv.UnquoteChar(basicLit.Value[1:len(basicLit.Value)-1], '\''); err == nil {
								basicLit.Value = strconv.Itoa(int(r))
							}
						}

						if keyValue, err := strconv.ParseInt(basicLit.Value, 0, 64); err == nil {
							if int(keyValue) > maxKeyValue {
								maxKeyValue = int(keyValue)
							}
						} else {
							maxKeyValue = 0
							constKeys = false
							break
						}
					} else {
						// A key that is not a BasicLit at all (a const IDENT or const
						// expression - Go requires array/slice literal keys to be constant,
						// but not to be literals). Its index is unknown to this scan, so the
						// SparseArray projection stands; a fixed-length array still needs its
						// declared length, which the padding below supplies.
						constKeys = false
						break
					}
				}
			}

			if constKeys {
				arrayTypeContext.compositeInitializer = false

				if definedLen > 0 {
					arrayTypeContext.maxLength = definedLen

					// A FIXED-SIZE array renders as `array<T>(N){[i] = v}`, whose ctor fills every
					// unset index with `default(T)` — so an element whose zero value must itself be
					// constructed needs the same factory the positional padding carries (see the
					// `.array(N, () => …)` suffix below). Only the fixed-array branch: the slice
					// form below renders a different type, whose constructor set this argument does
					// not belong to.
					arrayTypeContext.maxLengthElemFactory = v.arrayElemFactory(elementType)
				} else {
					arrayTypeContext.maxLength = maxKeyValue + 1
				}

				compositeSuffix = ""
			} else {
				arrayTypeContext.indexedInitializer = true
			}
		}
	}

	// A FIXED-SIZE array composite literal is `[N]T` long no matter how many elements it writes:
	// Go zero-fills the rest (`[8]byte{}` is EIGHT zero bytes; `[8]byte{1, 2}` is 1, 2 and six
	// zeros). The `.array()` projection builds its backing from the C# element array, which holds
	// ONLY the written elements, so a short literal produced a SHORT array — `[8]byte{}` became
	// length 0 and the first index panicked ("index out of range [7] with length 0"; math/rand/v2
	// chacha8's Seed). Pass the DECLARED length so the backing is sized and zero-filled. Only a
	// SHORT literal needs it: a full one — and every `[...]T{…}` ellipsis literal, whose length IS
	// its element count — already yields the right length and keeps the plain projection, so the
	// goldens for those are unchanged. A SLICE literal is genuinely as long as its elements
	// (`[]byte{}` IS empty), and its `.slice()` suffix never matches here.
	//
	// The length alone is not always enough. When the ELEMENT's own zero value must be CONSTRUCTED —
	// another unnamed fixed array (`[2][3]uint8`, whose inner length lives only in the Go type and
	// never in `array<T>`), or a struct whose fixed-array field initializer runs only inside a
	// declared constructor — padding with `default(T)` sizes the OUTER dimension while leaving every
	// zero-filled element unusable storage: `len(x[0])` reported 0 where Go says 3, and the first
	// indexed write into one panicked. That is the SAME defect one level down, and it survived the
	// length fix because the DECLARED form (`var x [2][3]uint8`) never takes this route — it goes
	// through the zero-value construction ladder, and was correct all along, so the two spellings of
	// one Go type disagreed (found through reflect, by the ArrayOf guard, whose constructed
	// `[2][3]uint8` compared unequal to the literal-built one). Reuse that ladder's own element
	// factory and its one argument-list renderer rather than a second copy of the rule, so the
	// literal and the declaration cannot drift apart; the factory is empty for every element whose
	// `default(T)` is already the Go zero value, which is nearly all of them, so the bare-length
	// render — and every existing golden — is unchanged.
	if definedLen > 0 && compositeSuffix == ".array()" && len(compositeLit.Elts) < definedLen {
		compositeSuffix = fmt.Sprintf(".array(%s)", arrayLengthArgs(strconv.Itoa(definedLen), v.arrayElemFactory(elementType)))
	}

	var newSpace string

	if callContext.keyValueSource == ArraySource || callContext.keyValueSource == MapSource || callContext.keyValueSource == StructSource || arrayTypeContext.compositeInitializer {
		newSpace = " "
	} else {
		newSpace = ""
	}

	identContext := DefaultIdentContext()
	identContext.isType = true

	if context.ident != nil {
		identContext.ident = context.ident
	}

	contexts := []ExprContext{arrayTypeContext, identContext}

	// A routed literal has no type syntax to render, so its name comes from the RESOLVED type — the
	// same spelling the constraint-proxy override a few lines below already uses, so no second way of
	// naming a type enters this file.
	var typeRender string

	if compositeLit.Type != nil {
		typeRender = v.convExpr(compositeLit.Type, contexts)
	} else {
		typeRender = convertToCSTypeName(v.getAliasQualifiedTypeName(exprType, false))
	}

	// A generic type instantiated with a self-referential constraint-proxy pointer argument
	// (`nistCurve[*P224Point]`) must render its type arguments through the PROXY
	// (`nistCurve<P224PointжnistPoint>`) — convExpr walked the AST literal `nistCurve[*P224Point]`
	// and rendered the box `ж<P224Point>`, which mismatches the pointer adapter that wraps
	// `ж<nistCurve<P224PointжnistPoint>>` (CS0311/type mismatch). Re-render from the RESOLVED type,
	// whose getAliasQualifiedTypeName substitutes the proxy. Gated to the proxy case, so no churn elsewhere.
	if named, ok := types.Unalias(exprType).(*types.Named); ok && v.namedHasConstraintProxy(named) {
		typeRender = convertToCSTypeName(v.getAliasQualifiedTypeName(named, false))
	}

	if aliasArrayComposite {
		// The alias renders as its Ident name, which cannot take the composite-initializer
		// bracket rewrite an ast.ArrayType gets in convArrayType — substitute the same forms
		// the unnamed literal produces: the element-array projection for plain elements
		// (`new uint64[]{…}` + `.array()`/`.slice()`), SparseArray for non-constant indexed
		// keys, and the alias's int-length ctor for constant-indexed sparse literals
		// (`new words(4){[2] = 30}` — the alias IS array<T>, which has that ctor).
		csElementType := convertToCSTypeName(v.getAliasQualifiedTypeName(elementType, false))

		if arrayTypeContext.indexedInitializer {
			typeRender = fmt.Sprintf("golib.SparseArray<%s>", csElementType)
		} else if arrayTypeContext.compositeInitializer {
			typeRender = fmt.Sprintf("%s[]", csElementType)
		} else if arrayTypeContext.maxLength > 0 {
			typeRender = fmt.Sprintf("%s(%s)", typeRender, arrayTypeContext.lengthArgs())
		}
	}

	if namedArrayComposite {
		csElementType := convertToCSTypeName(v.getAliasQualifiedTypeName(elementType, false))

		// An EMPTY composite of a named-over-array/slice (`tmpBuf{}` where `type tmpBuf [32]byte`
		// — runtime string.go's `*buf = tmpBuf{}` — or `pm{}` over a named slice) is the type's
		// ZERO VALUE. The generic named-composite `nil` filler below would land INSIDE the
		// element literal (`new tmpBuf(new byte[]{nil}.array())` — CS0029 NilType→byte). Emit
		// the zero value directly: a zeroed FIXED-LENGTH backing for an array wrapper (Go's
		// zero `[N]T` — an empty `{}` literal would produce a length-0 backing, not `[N]T`),
		// and an empty non-nil backing for a slice wrapper.
		if len(compositeLit.Elts) == 0 {
			if definedLen > 0 {
				// When the ELEMENT's own zero value must be CONSTRUCTED, `new T[N]` is N copies of
				// `default(T)` — sized, but unusable storage — which is the same defect the short-literal
				// padding above closes, one level down and reached through the wrapper: `nn{}` over
				// `type nn [2][3]int` gave two length-ZERO inner arrays (`2 0 [[] []]` against Go's
				// `2 3 [[0 0 0] [0 0 0]]`), and a named array over a struct carrying a fixed-array field
				// (runtime's `semTable` shape) PANICKED on the first inner index. Back it with the
				// element-factory `array<T>` ctor — the same form the KEYED branch below already uses,
				// and the same one argument-list renderer, so the literal and the declaration cannot
				// drift apart. This is the fourth caller `arrayLengthArgs` documents; until now the
				// wrapper carried the factory only on its keyed path.
				if factory := v.arrayElemFactory(elementType); factory != "" {
					return fmt.Sprintf("new %s(new array<%s>(%s))", typeRender, csElementType,
						arrayLengthArgs(strconv.Itoa(definedLen), factory))
				}

				// `new T[N]` is ALREADY the zero-filled declared length, so this form keeps the
				// plain projection rather than the padding suffix computed above — the length
				// would merely be restated (`new byte[6].array(6)`). Unchanged for every element
				// whose `default(T)` is already the Go zero value, which is nearly all of them.
				return fmt.Sprintf("new %s(new %s[%d].array())", typeRender, csElementType, definedLen)
			}

			return fmt.Sprintf("new %s(new %s[]{}%s)", typeRender, csElementType, compositeSuffix)
		}

		if arrayTypeContext.maxLength > 0 {
			// A KEYED (sparse, constant-index) array literal — `timedEventArgs{1: v}` over
			// `[N]uint64` (internal/trace/oldtrace) — renders its elements as the `[i] = v` indexed
			// initializer, which is invalid on a raw C# array (`new uint64[]{[1] = v}` — CS0131). Back
			// it with the indexer-capable golib `array<T>(length)` instead, mirroring the alias form
			// (`new words(4){[2] = 30}`); the named ctor takes an `array<T>` just as the positional
			// `.array()` path produces.
			typeRender = fmt.Sprintf("%s(new array<%s>(%s)", typeRender, csElementType, arrayTypeContext.lengthArgs())
			compositeSuffix += ")"
		} else {
			// Wrap the underlying array/slice literal in the named type's constructor:
			// `new d(new rune[]{...}.array())`. The element literal and its `.array()`/
			// `.slice()` suffix render via the ArraySource path below; close the ctor here.
			typeRender = fmt.Sprintf("%s(new %s[]", typeRender, csElementType)
			compositeSuffix += ")"
		}
	}

	if namedMapComposite {
		// Open the named ctor around the concrete map literal; the ')' closes after the
		// brace via compositeSuffix.
		typeRender = fmt.Sprintf("%s(new %s", typeRender, namedMapRender)
		compositeSuffix += ")"
	}

	if namedStructWrapRender != "" {
		// Open the wrapper ctor around the underlying named struct's keyed ctor; the
		// closing ')' follows the rbrace via compositeSuffix.
		typeRender = fmt.Sprintf("%s(new %s", typeRender, namedStructWrapRender)
		compositeSuffix += ")"
	}

	result.WriteString(fmt.Sprintf("new%s%s%s%s", newSpace, typeRender, lbracePrefix, lbrace))

	if len(compositeLit.Elts) > 0 {
		v.writeStandAloneCommentString(result, compositeLit.Elts[0].Pos(), nil, " ")
	} else {
		// If constructing a struct with no parameters, pass in a nill value (a named-map
		// composite's braces belong to the inner CONCRETE map literal — nothing to fill)
		if _, ok := exprType.(*types.Named); ok && !namedMapComposite {
			result.WriteString("nil")
		}
	}

	// Convert elements with potential interface casting
	if needsInterfaceCast && !isEmpty {
		elements := &strings.Builder{}

		for i, elt := range compositeLit.Elts {
			if i > 0 {
				elements.WriteString(", ")
			}

			var expr string

			if kv, ok := elt.(*ast.KeyValueExpr); ok {
				// A keyed element (a map, or a KEYED/sparse array emitted as a golib
				// SparseArray) takes the same `[key] = value` indexer form convKeyValueExpr's
				// MapSource arm emits, with the VALUE routed through the interface conversion.
				// The old flat `key, value` pair fed the collection initializer one item at a
				// time — Add(key) then Add(value), CS1950/CS1503 ×21 pairs on gccgoimporter's
				// `[...]types.Type{gccgoBuiltinINT8: types.Typ[types.Int8], …}` — and the key
				// skipped the defined-integer-type index cast (sparseArrayKey).
				keyContext := DefaultKeyValueContext()
				keyContext.source = callContext.keyValueSource
				keyContext.arrayBacked = callContext.keyValueArrayBacked
				keyContext.compositeType = callContext.keyValueCompositeType

				keyStr := v.sparseArrayKey(kv.Key, keyContext)
				valStr := v.convExpr(kv.Value, nil)

				if callContext.keyValueSource == MapSource {
					expr = fmt.Sprintf("[%s] = %s", keyStr, v.convertToInterfaceType(elementType, v.getType(kv.Value, false), valStr))
				} else {
					expr = fmt.Sprintf("%s, %s", keyStr, v.convertToInterfaceType(elementType, v.getType(kv.Value, false), valStr))
				}
			} else {
				// Handle regular elements
				expr = v.convertToInterfaceType(elementType, v.getType(elt, false), v.convExpr(elt, nil))
			}

			elements.WriteString(expr)
		}
		result.WriteString(elements.String())
	} else {
		result.WriteString(v.convExprList(compositeLit.Elts, compositeLit.Lbrace, callContext))
	}

	result.WriteString(fmt.Sprintf("%s%s%s", rbracePrefix, rbrace, compositeSuffix))

	return result.String()
}

// compositeLitIsKeyed reports whether a composite literal's elements are index/key keyed
// (KeyValueExpr). Reading Elts[0] is representative because normalizeMixedKeyedComposite has
// already given every element of a MIXED array/slice literal an explicit index — Go's
// all-or-nothing keying rule holds for struct literals only, and this check ran before that
// normalization existed.
func compositeLitIsKeyed(elts []ast.Expr) bool {
	if len(elts) == 0 {
		return false
	}

	_, keyed := elts[0].(*ast.KeyValueExpr)
	return keyed
}

// normalizeMixedKeyedComposite rewrites the POSITIONAL elements of a MIXED array/slice composite
// literal into keyed ones carrying the index Go gives them, leaving an all-keyed element list for
// the sparse-array paths. Go's rule: the first element is index 0, a keyed element sets the index
// to its (constant) key, and each following positional element takes the next index — so
// `[]byte{0xfe, 0x80, 15: 0x01}` is `{0: 0xfe, 1: 0x80, 15: 0x01}`, a SIXTEEN-byte value.
//
// Applies to array and slice literals only: a MAP literal is always fully keyed and a STRUCT
// literal genuinely cannot mix. An all-positional or already-all-keyed literal is left untouched
// (byte-identical output, which is why the whole corpus is unmoved by this), and so is one whose
// keys this scan cannot fold to constants — an index it cannot compute is one it must not invent.
func (v *Visitor) normalizeMixedKeyedComposite(compositeLit *ast.CompositeLit) {
	if len(compositeLit.Elts) == 0 {
		return
	}

	litType := v.info.TypeOf(compositeLit)

	if litType == nil {
		return
	}

	switch types.Unalias(litType).Underlying().(type) {
	case *types.Array, *types.Slice:
	default:
		return
	}

	anyKeyed := false
	anyPositional := false

	for _, elt := range compositeLit.Elts {
		if _, keyed := elt.(*ast.KeyValueExpr); keyed {
			anyKeyed = true
		} else {
			anyPositional = true
		}
	}

	if !anyKeyed || !anyPositional {
		return
	}

	// Indices are computed for the WHOLE list before any element is replaced: a key that will not
	// fold leaves every later index unknown, and a half-rewritten literal would be worse than the
	// untouched one this bails out to.
	indices := make([]int64, len(compositeLit.Elts))
	next := int64(0)

	for i, elt := range compositeLit.Elts {
		if keyValue, keyed := elt.(*ast.KeyValueExpr); keyed {
			typeAndValue, recorded := v.info.Types[keyValue.Key]

			if !recorded || typeAndValue.Value == nil {
				return
			}

			index, exact := constant.Int64Val(constant.ToInt(typeAndValue.Value))

			if !exact {
				return
			}

			next = index + 1

			continue
		}

		indices[i] = next
		next++
	}

	for i, elt := range compositeLit.Elts {
		if _, keyed := elt.(*ast.KeyValueExpr); keyed {
			continue
		}

		compositeLit.Elts[i] = &ast.KeyValueExpr{
			Key:   &ast.BasicLit{ValuePos: elt.Pos(), Kind: token.INT, Value: strconv.FormatInt(indices[i], 10)},
			Colon: elt.Pos(),
			Value: elt,
		}
	}
}

// elidedPointerElemContext renders a bare pointer-typed IDENT element of an ELIDED (type-inferred)
// slice/array literal as its box (`Ꮡc`, the pointer value) instead of a deref-aliased value alias —
// the untyped-elided twin of the TYPED pointer-element path (argTypeIsPtr, see convCompositeLit
// above). The inner `{c}` of `[][]*Certificate{{c}}` (crypto/x509 Verify) is an elided
// `[]*Certificate`; its element `c` is a deref-aliased `*Certificate` receiver, and emitting the
// value alias `c` into a `ж<Certificate>[]` array is CS0029. Returns nil when the element type is
// not a pointer OR no element is a bare pointer ident, so every already-correct elided literal keeps
// its exact nil-context rendering (zero golden churn). u8StringArgOK is set to preserve the
// nil-context default (u8StringOK true per element) that switching to a non-nil context would drop.
func (v *Visitor) elidedPointerElemContext(elem types.Type, elts []ast.Expr) *CallExprContext {
	if _, ok := elem.Underlying().(*types.Pointer); !ok {
		return nil
	}

	context := DefaultCallExprContext()
	hasPtrIdent := false

	for i, elt := range elts {
		context.u8StringArgOK[i] = true

		if ident, ok := elt.(*ast.Ident); ok {
			if _, isPtr := v.getType(ident, false).(*types.Pointer); isPtr {
				context.argTypeIsPtr[i] = true
				hasPtrIdent = true
			}
		}
	}

	if !hasPtrIdent {
		return nil
	}

	return context
}

// sparseArrayCompositeContext builds the element context for a KEYED (sparse) array/slice
// composite literal so its KeyValueExpr elements render as `[index] = value` against a golib
// SparseArray (MapSource + arrayBacked), rather than the invalid Go `key: value` form. Mirrors
// the elided-map handling and the typed keyValueSource==ArraySource→MapSource conversion. The
// composite's array/slice type is threaded through so an EMPTY-interface element type can box a
// string-literal value through @string (see convKeyValueExpr's MapSource `any` value slot).
func sparseArrayCompositeContext(compositeType types.Type, elts []ast.Expr) *CallExprContext {
	context := DefaultCallExprContext()
	context.keyValueSource = MapSource
	context.keyValueArrayBacked = true
	context.keyValueCompositeType = compositeType

	for i := range elts {
		context.u8StringArgOK[i] = true
	}

	return context
}

// markStringFieldLits enables the `u8` span rendering for POSITIONAL elements whose struct field
// slot is a Go `string` (or a named type over one). Go requires a positional literal to list every
// field in order, so element index i is field i; KEYED elements are ast.KeyValueExpr — index i is
// then NOT field i — and already render `u8` through convKeyValueExpr, so they are skipped.
//
// Every positional element in a string field slot is marked, not just the BasicLits, for the same
// reason the slice/array element twin above marks all of its own: the flag doubles as the slot's
// span-tolerance signal, and a false there makes convBinaryExpr suppress u8 on a string CONCAT
// element's literal operand. For an `@string` field that only cost a transcode
// (`rec{p + "-f"}` rendered `new rec(p + "-f")`, an Encoding.UTF8.GetBytes plus a throwaway
// intermediate on every evaluation); for a field of a NAMED string type it did not compile at all,
// because the plain form binds C#'s `string.Concat` and yields a `string` where the ctor wants the
// wrapper (`rec{base + "-s"}` over a `version` field, CS1503).
func (v *Visitor) markStringFieldLits(structType *types.Struct, elts []ast.Expr, context *CallExprContext) {
	for i, elt := range elts {
		if i >= structType.NumFields() {
			continue
		}

		if _, isKeyed := elt.(*ast.KeyValueExpr); isKeyed {
			continue
		}

		if basic, ok := structType.Field(i).Type().Underlying().(*types.Basic); ok && basic.Kind() == types.String {
			context.u8StringArgOK[i] = true
		}
	}
}

// markAnyFieldLits flips the per-element literal handling for POSITIONAL literal elements whose
// struct field slot is the EMPTY interface (`any`). A string literal boxes through @string
// (`(@string)"…"u8` — the cast is mandatory, the u8 keeps the bytes constant) instead of a BARE u8
// span (no conversion to the generated ctor's object parameter, CS1503) or a bare C# string (which
// boxes the wrong type — a later Go
// x.(string) assertion fails); an untyped CONSTANT boxes through Go's default type for its kind
// (`(nint)(8)`) for the same reason (else a later x.(int) fails), via the castArgToType plumbing
// convExprList honors.
// KEYED elements are ast.KeyValueExpr, never a BasicLit, so they skip here and resolve their field
// in convKeyValueExpr's own `any`-field arm instead. Go requires a positional literal to list every
// field in order, so element index i is field i.
func (v *Visitor) markAnyFieldLits(structType *types.Struct, elts []ast.Expr, context *CallExprContext) {
	for i, elt := range elts {
		if i >= structType.NumFields() || !isEmptyInterfaceTarget(structType.Field(i).Type()) {
			continue
		}

		if isStringBasicLit(elt) {
			context.u8StringArgOK[i] = true
			context.useGoStringArg[i] = true
		} else if castType := v.untypedConstBoxCast(elt); castType != "" {
			if context.castArgToType == nil {
				context.castArgToType = make(map[int]string)
			}

			context.castArgToType[i] = castType
		} else if castType := v.variadicFuncBoxCastType(v.getType(elt, false)); castType != "" {
			// The struct-field twin of the slice-element arm above: a VARIADIC func in an
			// `any` field slot must carry its Go func type, not C#'s synthesized delegate.
			if context.castArgToType == nil {
				context.castArgToType = make(map[int]string)
			}

			context.castArgToType[i] = castType
		}

		// A POINTER in that same `any` field slot crosses as its BOX, carrying its Go type —
		// the positional twin of the keyed-field arm in convKeyValueExpr (see
		// typedNilInterfaceBoxing.go). `encoding/gob`'s TestNilPointerPanics table is the shape:
		// `[]struct{ value any; mustPanic bool }{{nilStringPtr, true}, …}`, where every nil
		// pointer row must reach gob as a TYPED nil for gob to panic on it as Go does.
		if _, eltIsPtr := v.getType(elt, false).(*types.Pointer); eltIsPtr {
			context.argTypeIsPtr[i] = true
			context.anyBoxedPtrArgs[i] = true
		}

		// The FUNC sibling, in the positional `any` FIELD slot — `[]struct{ v any }{{nilFunc}}`
		// is the same shape as the element arm one level in.
		if eltType := v.getType(elt, false); eltType != nil {
			if _, eltIsFunc := eltType.Underlying().(*types.Signature); eltIsFunc {
				context.anyBoxedFuncArgs[i] = true
			}
		}
	}
}
