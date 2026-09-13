// interfaceConversion.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the single hardest mapping in the converter: turning a Go value into an
// INTERFACE value in C#.
//
// Go interfaces are satisfied structurally — a type implements io.Reader by having the right
// Read method, with no declaration saying so — while C# requires an explicit `: IReader`. go2cs
// closes that gap with the go2cs-gen source generators, and this file decides, per conversion
// site, which of their mechanisms applies:
//
//   - the type already implements the C# interface (same assembly, generator-emitted): pass it through
//   - it needs an ADAPTER class (`FileжWriter`) wrapping a pointer so the interface can be
//     implemented on the pointer rather than the value
//   - the adapter lives in ANOTHER assembly, so the reference must be qualified to reach it
//   - the target is the empty interface (`any`), which every value satisfies by boxing
//
// Picking wrong does not produce bad output; it produces C# that does not compile, which is why
// the arms below are so heavily annotated with the exact compiler error each one prevents.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

func (v *Visitor) convertExprToInterfaceType(interfaceExpr ast.Expr, targetExpr ast.Expr, exprResult string) string {
	// Target selector or index expression source if this source of the interface expression
	if selectorExpr, ok := interfaceExpr.(*ast.SelectorExpr); ok {
		// A selector that already types as an interface (`x.expr`, a struct field) must stay whole;
		// redirecting to `.Sel` loses the field type and records the conversion on the wrong target.
		exprType := v.getType(interfaceExpr, false)
		exprIsInterface := false

		if exprType != nil {
			exprIsInterface, _ = isInterface(exprType)
		}

		if !exprIsInterface {
			interfaceExpr = selectorExpr.Sel
		}
	} else if indexExpr, ok := interfaceExpr.(*ast.IndexExpr); ok {
		// A container-element index (`mr.readers[0]`, `m[k]`) already types as its ELEMENT — the
		// interface itself — so keep the whole expression. Redirect to X only when the indexed
		// expression is not interface-typed (e.g. a generic instantiation F[T]); redirecting a
		// container gave the SLICE/MAP type and recorded/keyed the conversion on the wrong type.
		exprType := v.getType(interfaceExpr, false)
		exprIsInterface := false

		if exprType != nil {
			exprIsInterface, _ = isInterface(exprType)
		}

		if !exprIsInterface {
			interfaceExpr = indexExpr.X
		}
	}

	return v.convertToInterfaceType(v.getType(interfaceExpr, false), v.getType(targetExpr, false), exprResult)
}

// convertToInterfaceType routes a Go value into an interface-typed slot: it RECORDS the
// `[assembly: GoImplement<Src, Iface>]` pair the duck-typed implementation needs, and returns the
// expression the cast site emits.
//
// A TYPE-PARAMETER slot is the one case where those two halves part company, and it needs both of
// them changed. Passing a concrete value to `func checkStringParseRoundTrip[P netipTypeCmp](…, x P,
// …)` (net/netip's fuzz_test.go) must satisfy P's CONSTRAINT, which is the only one of the two with
// a C# spelling — carrying the bare parameter name through instead recorded
// `[assembly: GoImplement<AddrPort, P>]`, a type argument out of scope at assembly-attribute level
// (CS0246 ×3) and not an interface at all, which the ImplementGenerator rejects outright and which
// takes the whole generated adapter set down with it (CS8785). So the record is minted against the
// constraint, which is also what makes it USEFUL: C# enforces `where P : netipTypeCmp` NOMINALLY, so
// the concrete type needs the pair naming the constraint.
//
// And precisely because C# enforces the constraint nominally and INFERS P from the argument, the
// argument must arrive as its OWN type: an adapter wrap there is a `netip_ΔAddrᴠnetipTypeCmp` handed
// to a parameter of type P (CS1503 ×5). Record the pair, emit the value unchanged.
//
// An unconstrained `[T any]` resolves to the empty interface and is dropped by the empty-interface
// guards exactly as before.
func (v *Visitor) convertToInterfaceType(interfaceType types.Type, targetType types.Type, exprResult string) string {
	if typeParam, ok := types.Unalias(interfaceType).(*types.TypeParam); ok {
		v.convertToInterfaceTypeSlot(typeParam.Constraint(), targetType, exprResult)
		return exprResult
	}

	return v.convertToInterfaceTypeSlot(interfaceType, targetType, exprResult)
}

// convertToInterfaceTypeSlot is convertToInterfaceType's body; see that function for the
// type-parameter split it sits behind.
func (v *Visitor) convertToInterfaceTypeSlot(interfaceType types.Type, targetType types.Type, exprResult string) string {
	// A type ALIAS is TRANSPARENT — `type Expr = ast.Expr` names the type ast.Expr already names —
	// but a SPELLING is not a type, and every name composed below is GENERATOR-FACING: the
	// `[assembly: GoImplement<Src, Iface>]` record, and the `<pkg>_<Src>ᴠ<Iface>` adapter class the
	// cast site references. go2cs-gen composes that class name from the RESOLVED SYMBOL, never from
	// the spelling the record carries, so a spelling that is not the type's own name puts the two
	// sides on different names — the cast site references a class the generator never emits (CS0246).
	// Resolve both operands to the type itself, once, so the record and the reference agree by
	// construction. (Five sites below already reach through the alias ad hoc to get at the named
	// type; doing it at the entry is that same move made total.)
	//
	// go/types' `rangeStmt` is the archetype: it declares `type Expr = ast.Expr` function-locally,
	// which lifts to `rangeStmt_Expr`, and `check.errorf(lhs[i], …)` then composed
	// `ast_rangeStmt_Exprᴠpositioner` against the generator's `ast_Exprᴠpositioner`. The defect is
	// older than the lift that exposed it: ANY alias whose name differs from its target's — a
	// package-level `type E = ast.Expr` just as much — mismatched the same way. It stayed invisible
	// only because the pre-lift local alias happened to be spelled exactly like its target.
	interfaceType, targetType = types.Unalias(interfaceType), types.Unalias(targetType)

	// Track interface types that need to an implementation mapping
	// to properly handle duck typed Go interface implementations
	var interfaceTypeName string

	if iface, ok := interfaceType.(*types.Interface); ok && !iface.Empty() {
		// An ANONYMOUS non-empty interface (a bare *types.Interface, not through a Named)
		// resolves to its LIFTED name — the raw Go literal is not valid C# (its `}` breaks
		// the GoImplement assembly attribute and the adapter class name; internal/trace's
		// `readBatch(r interface{io.Reader; io.ByteReader})` cast cross-file from
		// generation.go, CS1730 cascade). Prefer this file's lift, then the shared package
		// registry, then a deferred marker resolved after the file-visit barrier — the same
		// three-step resolution dynamicStructTypeName performs for anonymous structs.
		if name, ok := v.liftedTypeMap[interfaceType]; ok {
			interfaceTypeName = name
		} else if name := lookupDynamicTypeName(interfaceType.String()); name != "" {
			interfaceTypeName = name
		} else {
			interfaceTypeName = dynamicTypeMarker(interfaceType.String())
		}
	} else {
		interfaceTypeName = convertToCSTypeName(v.getFullyQualifiedTypeName(interfaceType, false))
	}

	targetTypeName := convertToCSTypeName(v.getFullyQualifiedTypeName(targetType, false))

	// Register the interface's DECLARED embeds so the inheritance PRUNE sees CROSS-ASSEMBLY
	// relations too — elf's errorReader records both io.ReadSeeker and io.Reader; C#'s
	// ReadSeeker : Reader (structural inheritance emitted at io's own declaration) makes the
	// two value-form partials implement Reader.Read twice (CS0111 + CS8646) unless the
	// subsumed record prunes. interfaceInheritances was only populated at LOCAL interface
	// declarations (visitInterfaceType), so a foreign base was invisible here.
	if named, ok := types.Unalias(interfaceType).(*types.Named); ok {
		if iface, ok := named.Underlying().(*types.Interface); ok && iface.NumEmbeddeds() > 0 {
			var embeds []string

			for ei := range iface.NumEmbeddeds() {
				if embNamed, ok := types.Unalias(iface.EmbeddedType(ei)).(*types.Named); ok {
					if _, isIface := embNamed.Underlying().(*types.Interface); isIface {
						embeds = append(embeds, convertToCSTypeName(v.getFullyQualifiedTypeName(embNamed, false)))
					}
				}
			}

			if len(embeds) > 0 {
				packageLock.Lock()

				if existing, ok := interfaceInheritances[interfaceTypeName]; ok {
					for _, embed := range embeds {
						existing.Add(embed)
					}
				} else {
					interfaceInheritances[interfaceTypeName] = NewHashSet(embeds)
				}

				packageLock.Unlock()
			}
		}
	}

	if targetTypeName == "" || targetTypeName == "nil" || targetTypeName == "any" {
		return exprResult
	}

	var prefix string
	pointerTarget := false

	if strings.HasPrefix(targetTypeName, PointerPrefix+"<") {
		targetTypeName = targetTypeName[3 : len(targetTypeName)-1]
		pointerTarget = true
		prefix = PointerDerefOp
	}

	// An interface-to-interface conversion is sometimes satisfied by the C# inheritance
	// emitted at the source interface declaration (structural bases — see
	// getStructuralInterfaceBases). When the target interface is declared downstream in a
	// different package, though, the source interface cannot inherit it retroactively; record
	// a generated interface adapter and wrap the value at the conversion site.
	targetIsIface, targetIfaceEmpty := isInterface(targetType)

	// An OPEN generic target — the receiver's own instantiation cast to an interface INSIDE its
	// generic method (`return newBoringPrivateKey(c, …)` with `c *nistCurve[Point]`, crypto/ecdh) —
	// still needs its CONVERSION emitted (wrapped in the generic adapter `nistCurveжCurve<Point>`,
	// which the CLOSED per-instantiation records already generate), but must NOT be RECORDED as an
	// implementation: recording it emits `[assembly: GoImplement<nistCurve<Point>, Curve>]` carrying
	// the type PARAMETER `Point` in an assembly-attribute type argument, where it is out of scope
	// (CS0246). So this flag gates only the record below, not `recordable` (the conversion gate).
	targetIsOpenGeneric := false

	{
		openTarget := targetType

		if ptr, ok := openTarget.(*types.Pointer); ok {
			openTarget = ptr.Elem()
		}

		if named, ok := types.Unalias(openTarget).(*types.Named); ok {
			for i := 0; i < named.TypeArgs().Len(); i++ {
				if _, isParam := named.TypeArgs().At(i).(*types.TypeParam); isParam {
					targetIsOpenGeneric = true
					break
				}
			}
		}
	}

	recordableInterface := false

	if targetIsIface && !targetIfaceEmpty && exprResult != "" &&
		interfaceTypeName != "" && interfaceTypeName != "nil" &&
		interfaceTypeName != targetTypeName && interfaceTypeName != "any" &&
		!strings.Contains(targetTypeName, "interface{") &&
		!typeContainsTypeParams(interfaceType) && !typeContainsTypeParams(targetType) &&
		!types.Identical(interfaceType, targetType) {
		if iface, ok := interfaceType.Underlying().(*types.Interface); ok && !iface.Empty() && types.Implements(targetType, iface) {
			targetCoveredByInheritance := false
			qualifiedInterfaceTypeName := rootQualifyIfAmbiguous(interfaceTypeName)

			packageLock.Lock()

			if inheritedInterfaces, ok := interfaceInheritances[targetTypeName]; ok {
				targetCoveredByInheritance = inheritedInterfaces.Contains(interfaceTypeName) ||
					inheritedInterfaces.Contains(qualifiedInterfaceTypeName)
			}

			packageLock.Unlock()

			recordableInterface = !targetCoveredByInheritance
		}
	}

	// A concrete-target record must be REAL: a [GoImplement] pair generates a partial-struct /
	// adapter whose members forward to the target's like-named methods, so the recorded form's Go
	// method set (T for a value record, *T for a ж<T> record — targetType already carries the
	// pointer) must actually satisfy the interface. Every conversion the Go checker admitted
	// passes trivially; the gate only drops a pair a caller composed from MISMATCHED types —
	// net/http's `err = http2GoAwayError{ErrCode: …}`: the keyed composite's sparse-array ident
	// context leaked the error LHS onto the ErrCode FIELD value and recorded
	// GoImplement<http2ErrCode, error>, whose generated Error() forwarded to a method http2ErrCode
	// does not have (CS0103). Folding the gate into recordableBase also suppresses the matching
	// adapter-wrapping emissions below — a wrap referencing a never-generated adapter is CS0246.
	// A type-param-carrying target (the open-generic receiver conversion, crypto/ecdh) skips the
	// check — types.Implements is undefined for uninstantiated generics, and that arm's emission
	// must stay (its record is already excluded by targetIsOpenGeneric).
	recordSatisfiesIface := true

	if iface, ok := interfaceType.Underlying().(*types.Interface); ok && !iface.Empty() && !typeContainsTypeParams(targetType) {
		recordSatisfiesIface = types.Implements(targetType, iface)
	}

	recordableBase := !targetIsIface && recordSatisfiesIface &&
		interfaceTypeName != "" && interfaceTypeName != "nil" &&
		interfaceTypeName != targetTypeName &&
		interfaceTypeName != "any" &&
		!strings.Contains(targetTypeName, "interface{")

	recordable := recordableBase && v.isLocalImplType(targetType)

	// A VALUE conversion of a FOREIGN type to a LOCAL interface (os.Signal is DOWNSTREAM
	// of syscall.Signal - neither assembly can partial the other) records too: the
	// generator emits a local VALUE ADAPTER class wrapping a COPY (Go value semantics)
	// instead of the impossible foreign partial (exec_posix p.Signal(Kill), CS1503).
	// Gate to a NAMED foreign type - tuples/other shapes must not record (the first cut
	// wrapped a destructured tuple result in a phantom adapter). The -tests
	// production-under-test package is carved out (B4/B5): its types are a foreign Go
	// PACKAGE but compile into the SAME test assembly, so the generator takes the
	// partial-struct route for them and a ᴠ value-adapter class is never emitted —
	// referencing one is CS0246 (sort_test's `sort_IntSliceᴠInterface`).
	targetIsForeignNamed := false
	targetIsSameAssemblyForeign := false

	// The WHITE-BOX bridge's own PRODUCTION-declared target: the same GO package (so every
	// pkg != v.pkg gate below reads it as local) but a CLOSED referenced C# assembly, which the
	// generator cannot partial — it emits a per-interface ᴠ VALUE ADAPTER for it, exactly as for
	// any other foreign struct.
	whiteboxProductionTarget := false

	if named, ok := types.Unalias(targetType).(*types.Named); ok {
		whiteboxProductionTarget = v.whiteboxProductionObject(named.Obj())

		if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg {
			if v.isSameAssemblyPkg(pkg) {
				targetIsSameAssemblyForeign = true
			} else {
				targetIsForeignNamed = true
			}
		}
	}

	recordableValueForeign := recordableBase && !pointerTarget && targetIsForeignNamed && !v.isLocalImplType(targetType) && v.isLocalImplType(interfaceType)

	// The white-box production target takes that same route. Both consequences matter and only
	// the pair together is correct: the cast site must CONSTRUCT the adapter, and the record must
	// be EXEMPT from the interface-inheritance prune — which is sound only for the partial-struct
	// shape of one type carrying one interface list. encoding/binary is the corpus instance.
	// `TestByteOrder` casts `BigEndian`/`LittleEndian` to its function-local `byteOrder`, which
	// EMBEDS the production `ByteOrder`, so `bigEndian → ByteOrder` was recorded and then pruned
	// as covered — true while the recompile model merged a `bigEndian : TestByteOrder_byteOrder`
	// partial into the local declaration, false once production is a referenced assembly. Every
	// `Read(r, BigEndian, data)` in the suite then failed CS1503 (~30 sites).
	if recordableBase && !pointerTarget && whiteboxProductionTarget {
		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			if pkg := named.Obj().Pkg(); pkg != nil {
				ifacePkgName := pkg.Name()

				if ifaceNamed, ok := types.Unalias(interfaceType).(*types.Named); ok {
					if ifacePkg := ifaceNamed.Obj().Pkg(); ifacePkg != nil {
						ifacePkgName = ifacePkg.Name()
					}
				}

				key := implementRecordKey(pkg.Name(), targetTypeName, interfaceTypeName, ifacePkgName)

				// The PRODUCTION assembly already implements the pair (its package_info.cs carries
				// the value record, loaded when the reference model bound it as an ordinary import),
				// so the bare value converts implicitly and a second adapter is dead machinery.
				// Trustworthy only if the production assembly realized the pair as a partial struct
				// (valueRecordRealizesAsPartialStruct).
				packageLock.Lock()
				productionImplements := importedValueImplements.Contains(key)
				packageLock.Unlock()

				recordableValueForeign = !(productionImplements && valueRecordRealizesAsPartialStruct(targetType))
			}
		}
	}

	// The POINTER twin of the arm above. The white-box bridge is the SAME Go package as production,
	// so a `*prodT → prodIface` cast inside an internal `_test.go` reads as local and records the pair
	// again — but production is a REFERENCED assembly that already generated the ж adapter from its
	// OWN record, and `InternalsVisibleTo <assembly>.tests` makes even an unexported one reachable. The
	// duplicate record mints a SECOND adapter class under a test anchor, and that copy resolves its
	// forwarding members in the TEST class's scope: context's internal test converts
	// `*cancelCtx`/`*timerCtx` to `canceler` for the `contains(pc.children, …)` map key, and the
	// duplicate bound `Done` to the unrelated `afterFuncContext.Done` extension (CS1929) while emitting
	// `cancel` with an EMPTY body — a silently degraded override, not merely a build error.
	//
	// Suppressing only the RECORD is what fixes it: resolveAdapterNameMarkers resolves a pair that
	// reached no record to the unqualified name it would have had, which is production's own
	// `<prodClass>.<T>ж<Iface>` — so the cast site references the real adapter with no other change.
	// Gated on production actually carrying the pair (its package_info.cs is loaded by
	// convertTestVariant), never assumed: a pair only the test converts still needs its local record.
	productionPointerImplemented := false

	if recordable && pointerTarget {
		// The pointer target arrives as the *types.Pointer, so whiteboxProductionTarget (computed
		// from the unwrapped VALUE form above) is structurally false here — unwrap and ask directly.
		namedTarget := targetType

		if ptr, ok := namedTarget.(*types.Pointer); ok {
			namedTarget = ptr.Elem()
		}

		if named, ok := types.Unalias(namedTarget).(*types.Named); ok && v.whiteboxProductionObject(named.Obj()) {
			if pkg := named.Obj().Pkg(); pkg != nil {
				ifacePkgName := pkg.Name()

				if ifaceNamed, ok := types.Unalias(interfaceType).(*types.Named); ok {
					if ifacePkg := ifaceNamed.Obj().Pkg(); ifacePkg != nil {
						ifacePkgName = ifacePkg.Name()
					}
				}

				// Composed through implementRecordKey so this use side can never drift from the
				// loader's spelling; its canonicalization also subsumes the earlier global::-strip
				// (a `global::go` segment never carries the package suffix, so the segment scan
				// drops it) — the duplicate-adapter/empty-cancel-body regression stays fixed.
				key := implementRecordKey(pkg.Name(),
					getCoreSanitizedIdentifier(named.Obj().Name()), interfaceTypeName, ifacePkgName)

				packageLock.Lock()
				productionPointerImplemented = importedPointerImplements.Contains(key)
				packageLock.Unlock()
			}
		}
	}

	// A LOCAL NAMED FUNC type's VALUE record also generates a per-interface adapter CLASS — a C#
	// delegate cannot be a partial struct, so the generator takes the `<src>ᴠ<iface>` route (see the
	// emission arm below, and flag's generated funcValue-...-Value-val.g.cs). It therefore needs the
	// SAME exemption from the interface-inheritance prune as the ж<T> and foreign-value forms: the
	// cast site references the adapter for the EXACT interface it targets. flag's boolFuncValue is
	// recorded against BOTH boolFlag and Value (boolFlag EMBEDS Value), so the prune dropped the
	// subsumed Value pair as "covered by inheritance" — true for a partial struct that folds the base
	// into one interface list, FALSE for a per-interface adapter class — and flag.cs's
	// `new boolFuncValueᴠValue(…)` lost its type (CS0246). Computed independently of exprResult: the
	// record-only probe paths record the same pair, and the exemption must hold wherever it lands.
	recordableValueLocalFunc := false

	if recordable && !pointerTarget {
		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			if _, isSig := named.Underlying().(*types.Signature); isSig {
				recordableValueLocalFunc = true
			}
		}
	}

	// A VALUE conversion where BOTH sides are FOREIGN (encoding/binary's `BigEndian` passed as
	// binary.ByteOrder from debug/plan9obj): when the defining assembly already implements the
	// pair (its package_info carries the value-form GoImplement record), the bare value converts
	// implicitly — otherwise record the pair locally so the generator emits the LOCAL value
	// adapter for the foreign struct (same route as the local-interface case above).
	if recordableBase && !pointerTarget && targetIsForeignNamed && !v.isLocalImplType(interfaceType) {
		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg {
				// The interface side of the key is the CANONICAL class-relative name — the
				// simple name collides across same-named interfaces (image's
				// Paletted→image.Image record must not satisfy a Paletted→draw.Image cast; see
				// canonicalImplementRecordIfaceName). A dotless render qualifies with the
				// INTERFACE's own package (not the target struct's).
				ifacePkgName := pkg.Name()

				if ifaceNamed, ok := types.Unalias(interfaceType).(*types.Named); ok {
					if ifacePkg := ifaceNamed.Obj().Pkg(); ifacePkg != nil {
						ifacePkgName = ifacePkg.Name()
					}
				}

				key := implementRecordKey(pkg.Name(), targetTypeName, interfaceTypeName, ifacePkgName)

				packageLock.Lock()
				foreignValueImplExists := importedValueImplements.Contains(key)
				packageLock.Unlock()

				// A record the declaring assembly realized as an adapter CLASS rather than as a
				// partial struct (a named FUNC type) gives the consumer nothing to convert
				// through — keep the LOCAL record, and the local adapter with it.
				if !foreignValueImplExists || !valueRecordRealizesAsPartialStruct(targetType) {
					recordableValueForeign = true
				}
			}
		}
	}

	// A VALUE conversion of the PRODUCTION-UNDER-TEST package's type from an EXTERNAL test file
	// (sort_test's `Sort(IntSlice(data))`, B4/B5): the type's declaration compiles into this
	// same test assembly, so the pair is realized by the partial-struct route — record it
	// (production-qualified, so the metadata write anchors it to the production class where the
	// declaration lives) unless the production package already records the pair itself (its
	// package_info.cs pairs are loaded by convertTestVariant), and let the cast site fall
	// through to the plain value emission the production package's own local casts use.
	recordableValueSameAssembly := false

	if recordableBase && !pointerTarget && targetIsSameAssemblyForeign {
		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			if pkg := named.Obj().Pkg(); pkg != nil {
				ifacePkgName := pkg.Name()

				if ifaceNamed, ok := types.Unalias(interfaceType).(*types.Named); ok {
					if ifacePkg := ifaceNamed.Obj().Pkg(); ifacePkg != nil {
						ifacePkgName = ifacePkg.Name()
					}
				}

				key := implementRecordKey(pkg.Name(), targetTypeName, interfaceTypeName, ifacePkgName)

				packageLock.Lock()
				recordableValueSameAssembly = !(importedValueImplements.Contains(key) && valueRecordRealizesAsPartialStruct(targetType))
				packageLock.Unlock()
			}
		}
	}

	if recordableInterface && !targetIsOpenGeneric {
		packageLock.Lock()

		if implementations, exists := interfaceImplementations[interfaceTypeName]; exists {
			implementations.Add(targetTypeName)
		} else {
			interfaceImplementations[interfaceTypeName] = NewHashSet([]string{targetTypeName})
		}

		// An interface-sourced pair generates a distinct `<src>ᴠ<iface>` adapter class the
		// cast site references — exempt it from the interface-inheritance prune.
		adapterClassImplementations.Add(interfaceTypeName + "|" + targetTypeName)

		packageLock.Unlock()
	}

	if ((recordable && !productionPointerImplemented) || recordableValueForeign || recordableValueSameAssembly) && !targetIsOpenGeneric {
		// A POINTER-sourced cast records the ж<T>-wrapped name; the attribute emission unwraps
		// it to `GoImplement<T, Iface>(Pointer = true)`, which generates the IжAdapter wrapper
		// instead of the value-boxing partial struct (see convert-to-interface emission below).
		// An OPEN generic target is excluded here (its `<Point>` argument cannot appear in an
		// assembly attribute — CS0246) while still taking the adapter-wrapping conversion below.
		recordName := targetTypeName

		if pointerTarget {
			recordName = PointerPrefix + "<" + targetTypeName + ">"
		}

		packageLock.Lock()

		if implementations, exists := interfaceImplementations[interfaceTypeName]; exists {
			implementations.Add(recordName)
		} else {
			interfaceImplementations[interfaceTypeName] = NewHashSet([]string{recordName})
		}

		// A foreign-struct VALUE pair generates a distinct `<pkg>_<src>ᴠ<iface>` adapter class
		// the cast site references — exempt it from the interface-inheritance prune (a local
		// impl folds into the type's own partial-struct base list and still prunes; the
		// pointer-form record is already exempt there by its ж< prefix).
		if recordableValueForeign || recordableValueLocalFunc {
			adapterClassImplementations.Add(interfaceTypeName + "|" + recordName)
		}

		packageLock.Unlock()
	}

	if derivedInterfaceType, ok := interfaceType.Underlying().(*types.Interface); ok {
		if targetStructType, ok := targetType.(*types.Named); ok {
			// Iterate over methods of the derived interface looking for struct parameters
			for i := 0; i < derivedInterfaceType.NumMethods(); i++ {
				interfaceMethod := derivedInterfaceType.Method(i)
				interfaceMethodSignature, ok := interfaceMethod.Type().(*types.Signature)

				if !ok {
					continue
				}

				// Lookup matching receiver method for target struct by name
				methodInfo, _, _ := types.LookupFieldOrMethod(types.NewPointer(targetStructType), true, v.pkg, interfaceMethod.Name())

				if methodInfo == nil {
					methodInfo, _, _ = types.LookupFieldOrMethod(targetStructType, true, v.pkg, interfaceMethod.Name())
				}

				if methodInfo == nil {
					continue
				}

				targetMethodSignature, ok := methodInfo.Type().(*types.Signature)

				if !ok {
					continue
				}

				// Iterate over parameters of the interface method
				totalParameters := interfaceMethodSignature.Params().Len()

				for j := 0; j < totalParameters; j++ {
					// Underlying() is used ONLY for the struct-KIND checks below — recording the
					// underlying's name stringifies a raw *types.Struct into Go-ish text
					// (`GoImplicitConv<struct{p printer}, …>` and a mangled anonymous decoder-state
					// monster in encoding/xml's package_info.cs — not valid C# attribute args).
					interfaceParamType := interfaceMethodSignature.Params().At(j).Type()
					targetParameterType := targetMethodSignature.Params().At(j).Type()

					// Check if targetParamType is a struct or a pointer to a struct
					if ptrType, ok := targetParameterType.Underlying().(*types.Pointer); ok {
						targetParameterType = ptrType.Elem()
					}

					if _, ok := targetParameterType.Underlying().(*types.Struct); ok {
						// Check if interfaceParamType is a struct or a pointer to a struct
						if ptrType, ok := interfaceParamType.Underlying().(*types.Pointer); ok {
							interfaceParamType = ptrType.Elem()
						}

						if _, ok := interfaceParamType.Underlying().(*types.Struct); ok {
							// Both interfaceParamType and targetParamType are structs, track implicit conversions
							interfaceParamTypeName := v.implicitConvStructTypeName(interfaceParamType)
							targetParamTypeName := v.implicitConvStructTypeName(targetParameterType)

							// An IDENTICAL pair (the interface and target methods share the
							// parameter type) is a self-conversion — meaningless, and a
							// user-defined operator cannot convert a type to itself (CS0555 from
							// the generator). Marker-form names compare by signature, so identical
							// anonymous structs are also skipped; differing markers resolve after
							// the barrier (see resolveImplicitConvTypeName).
							if interfaceParamTypeName == targetParamTypeName {
								continue
							}
							var conversions HashSet[string]
							var exists bool

							packageLock.Lock()

							// For interface methods that have struct parameters, tracked implicit conversions
							// are inverted to allow for implicit conversions from struct to interface
							if conversions, exists = invertedImplicitConversions[interfaceParamTypeName]; exists {
								conversions.Add(targetParamTypeName)
							} else {
								conversions = NewHashSet([]string{targetParamTypeName})
								invertedImplicitConversions[interfaceParamTypeName] = conversions
							}

							packageLock.Unlock()
						}
					}
				}
			}
		}
	}

	if recordableInterface && exprResult != "" {
		adapterSource := targetTypeName

		// A SAME-ASSEMBLY source interface (the -tests production-under-test package, B4/B5)
		// composes UNPREFIXED: the generator's foreign check is by containing assembly, so it
		// generates the bare composed name — and the record is adapter-class-marked, so the
		// -tests metadata split anchors it in the test package unit where the bare name resolves.
		//
		// The WHITE-BOX bridge's PRODUCTION-declared source interface is the exception, and it is
		// the same one the value arm below already makes: go/packages merges the production files
		// into the internal variant's own Go package, so `pkg == v.pkg` reads it as local, while
		// its C# lives in the REFERENCED production assembly — foreign to the generator's
		// containing-assembly check, which therefore prefixes (`net_ConnᴠReader`). Both sides must
		// compose the same name (net: 26 CS0426 across seven internal test files).
		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			if pkg := named.Obj().Pkg(); pkg != nil && ((pkg != v.pkg && !v.isSameAssemblyPkg(pkg)) || whiteboxProductionTarget) {
				simpleTarget := targetTypeName

				if idx := strings.LastIndex(simpleTarget, "."); idx >= 0 {
					simpleTarget = simpleTarget[idx+1:]
				}

				adapterSource = getSanitizedIdentifier(pkg.Name()) + "_" + simpleTarget
			}
		}

		return fmt.Sprintf("new %s(%s)", v.testOwnedAdapterRef(valueAdapterTypeRef(adapterSource, interfaceTypeName), targetTypeName, interfaceTypeName), exprResult)
	}

	// A POINTER-sourced cast to a locally-implemented interface routes through the generated
	// IжAdapter wrapper: Go's interface value holds the *T, so the adapter aliases the receiver
	// box exactly — every call through the interface mutates the original object, direct-ж
	// receiver methods bind on the box, and a type assert back to *T unwraps to the same box.
	// The old `~box` deref boxed a COPY into the C# interface (aliasing divergence) and could
	// not serve direct-ж members at all (math/rand lockedSource CS1929/CS1503). Non-local impl
	// types keep the deref-copy form below (their adapter is not generated in this assembly).
	// A recorded VALUE-form foreign conversion references the LOCAL value adapter. Its class
	// name is PACKAGE-QUALIFIED (`new syscall_ΔSignalᴠΔSignal(sig)`) for the same reason as
	// the local pointer adapters: two same-named foreign types adapting to one interface
	// otherwise compose a single colliding class (math/big's bytes.Reader + strings.Reader).
	if recordableValueForeign && exprResult != "" {
		qualifiedTarget := targetTypeName

		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			// The white-box production target composes the same `<pkg>_<Simple>` form: the
			// generator's foreign check is by CONTAINING ASSEMBLY, so it prefixes the class name
			// there too, and the two sides must agree (see ForeignPackagePrefix).
			if pkg := named.Obj().Pkg(); pkg != nil && (pkg != v.pkg || whiteboxProductionTarget) {
				simpleTarget := targetTypeName

				if idx := strings.LastIndex(simpleTarget, "."); idx >= 0 {
					simpleTarget = simpleTarget[idx+1:]
				}

				qualifiedTarget = getSanitizedIdentifier(pkg.Name()) + "_" + simpleTarget
			}
		}

		return fmt.Sprintf("new %s(%s)", v.testOwnedAdapterRef(valueAdapterTypeRef(qualifiedTarget, interfaceTypeName), targetTypeName, interfaceTypeName), exprResult)
	}

	// A LOCAL NAMED FUNC type with methods (flag's funcValue implementing Value): a C#
	// delegate cannot be a partial struct, so the generator emits a VALUE adapter class —
	// reference it at the conversion site (the record fires via the recordable arm above).
	if recordable && !pointerTarget && exprResult != "" {
		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			if _, isSig := named.Underlying().(*types.Signature); isSig {
				return fmt.Sprintf("new %s(%s)", v.testOwnedAdapterRef(valueAdapterTypeRef(targetTypeName, interfaceTypeName), targetTypeName, interfaceTypeName), exprResult)
			}
		}
	}

	if pointerTarget && recordable && exprResult != "" {
		return fmt.Sprintf("new %s(%s)", adapterTypeRef(v.whiteboxVariantAdapterStructName(targetTypeName), interfaceTypeName), exprResult)
	}

	// A POINTER-sourced cast to an interface implemented by a FOREIGN type routes through the
	// foreign assembly's PUBLIC adapter when that package recorded the same pointer-implement
	// pair (parsed from its package_info) - os's `err = &PathError{...}` references io/fs's
	// generated `io.fs_package.PathErrorжerror` (the bare value emission was CS0029 x38: the
	// foreign VALUE struct does not implement the interface; only its pointer adapter does).
	if pointerTarget && !recordable && exprResult != "" {
		// The pointer-sourced target arrives as the *types.Pointer - unwrap to its named elem.
		namedTarget := targetType

		if ptr, ok := namedTarget.(*types.Pointer); ok {
			namedTarget = ptr.Elem()
		}

		if named, ok := types.Unalias(namedTarget).(*types.Named); ok {
			if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg {
				// The key is the SHARED spelling both sides can compute (implementRecordKey):
				// the TYPE side from the RENDERED C# target, because the record carries the
				// emitted name and a collision-renamed type (`ΔRGBA`, `ΔSignature`, `ΔError`)
				// never matched its own Go name; the INTERFACE side collapsed to the
				// class-relative tail, because neither side's raw chain is stable — go/types
				// records its own `Object` whole where the cast site renders it short, and
				// text/template/parse records its own `Node` bare where the cast site renders it
				// whole. The package CLASS stays in the key: the simple name alone collides
				// across same-named interfaces (image's Paletted→image.Image record must not
				// satisfy a Paletted→draw.Image cast — image/draw CS1503).
				ifacePkgName := pkg.Name()

				if ifaceNamed, ok := types.Unalias(interfaceType).(*types.Named); ok {
					if ifacePkg := ifaceNamed.Obj().Pkg(); ifacePkg != nil {
						ifacePkgName = ifacePkg.Name()
					}
				}

				key := implementRecordKey(pkg.Name(), targetTypeName, interfaceTypeName, ifacePkgName)

				packageLock.Lock()
				foreignAdapterExists := importedPointerImplements.Contains(key)
				packageLock.Unlock()

				if foreignAdapterExists {
					// Reference through the file-local package ALIAS (`CrossPkgLib.Meter` +
					// adapter suffix), not the raw package-class qualifier
					// (`CrossPkgLib_package.…`) - user-ruled style; getAliasQualifiedTypeName both yields
					// the aliased form and registers the file-local using for it.
					aliasQualified := v.getAliasQualifiedTypeName(named, false)
					adapterBase := convertToCSTypeName(aliasQualified)

					adapterBase = wholeTypeAliasAdapterBase(aliasQualified, adapterBase, targetTypeName)

					return fmt.Sprintf("new %s(%s)", adapterTypeRef(adapterBase, interfaceTypeName), exprResult)
				}

				// NO exported adapter — the defining package never converts this pair itself
				// (os never casts *File to io.Reader). Record the pairing LOCALLY: the
				// generator emits a LOCAL adapter class for the foreign struct, binding its
				// methods from metadata (fmt's Fscan(os.Stdin, …), CS1503 ×3). Aliasing is
				// faithful — the adapter wraps the ж<T> box itself, unlike the deref-COPY
				// fallback this replaces. The class name is PACKAGE-QUALIFIED
				// (`os_FileжReader`): two same-named foreign structs adapting to the same
				// interface otherwise compose ONE colliding class (math/big records both
				// bytes.Reader and strings.Reader against io.ByteScanner — CS0102/CS0111).
				if recordableBase && exprResult != "" {
					recordName := PointerPrefix + "<" + targetTypeName + ">"

					packageLock.Lock()

					if implementations, exists := interfaceImplementations[interfaceTypeName]; exists {
						implementations.Add(recordName)
					} else {
						interfaceImplementations[interfaceTypeName] = NewHashSet([]string{recordName})
					}

					packageLock.Unlock()

					// The PRODUCTION-UNDER-TEST package's struct compiles INTO this test
					// assembly (B4/B5): the generator resolves its declaration locally and
					// composes the adapter WITHOUT the package prefix, nested in the production
					// class (the production-qualified ж<T> record above anchors there).
					// Reference it through the aliased package qualifier — the same form as the
					// foreign-adapter-exists arm — instead of `<pkg>_<Type>`, which names a
					// class the generator never emits in a same-assembly build (strings_test's
					// `strings_BuilderжWriter` CS0246).
					if v.isSameAssemblyPkg(pkg) {
						// Same whole-type-alias rebuild as the foreign-adapter arm above: crypto/ecdh's
						// PublicKey is collision-renamed (ΔPublicKey) and the external test file reaches
						// it through `global using ecdhꓸPublicKey`, so the alias-qualified render is a
						// single identifier and the composed `ecdhꓸPublicKeyж_ᴛ1` named nothing (CS0246),
						// while its un-renamed sibling PrivateKey — rendered `ecdh.PrivateKey` — composed
						// correctly. The generator anchors this adapter in the production class, so the
						// reference must read `ecdh.ΔPublicKeyж_ᴛ1`.
						aliasQualified := v.getAliasQualifiedTypeName(named, false)
						adapterBase := wholeTypeAliasAdapterBase(aliasQualified, convertToCSTypeName(aliasQualified), targetTypeName)

						return fmt.Sprintf("new %s(%s)", adapterTypeRef(adapterBase, interfaceTypeName), exprResult)
					}

					simpleTarget := targetTypeName

					if idx := strings.LastIndex(simpleTarget, "."); idx >= 0 {
						simpleTarget = simpleTarget[idx+1:]
					}

					qualifiedTarget := getSanitizedIdentifier(pkg.Name()) + "_" + simpleTarget

					return fmt.Sprintf("new %s(%s)", adapterTypeRef(qualifiedTarget, interfaceTypeName), exprResult)
				}
			}
		}
	}

	// Handle special case for pointer dereference of immediate address of operation, this
	// is an unnecessary operation as it creates a pointer to an object and then immediately
	// dereferences the pointer value, so we can just return the expression result instead
	if prefix == PointerDerefOp {
		if strings.HasPrefix(exprResult, AddressPrefix+"(") {
			return strings.TrimSuffix(strings.TrimPrefix(exprResult, AddressPrefix+"("), ")")
		} else if strings.HasPrefix(exprResult, "@new<") {
			return fmt.Sprintf("new %s()", strings.TrimSuffix(strings.TrimPrefix(exprResult, "@new<"), ">()"))
		}
	}

	return prefix + exprResult
}

// adapterTypeRef renders the reference to the generated pointer-interface adapter class for a
// *T → iface cast: `<struct>ж<ifaceSimple>` (PointerPrefix - user-ruled style; keep in
// sync with the generator, which composes the same name via Symbols.PointerPrefix), nested in the struct's package class like
// the struct itself, so a same-package reference is the bare name. The interface side uses its
// SIMPLE name — the generator derives the same identifier via GetSimpleName, so both sides must
// agree on last-dot-segment naming.
// testOwnedAdapterRef qualifies an adapter class generated from reference-test metadata through
// that metadata file's first class. Production conversion and recompile tests keep the historical
// bare member name because their generated adapter already shares the current package class.
//
// A MIXED white-box suite has TWO anchors, so the reference must name the one the pair's record
// will land in — the participants are the record's own two spellings, matched against the same
// bridge-declared-name predicate splitWhiteboxVariantRecords applies. (The POINTER form reaches the
// same answer through the deferred marker's emittedAdapterPairAnchors; a VALUE adapter's name is
// composed inline and has no marker to resolve, so it decides here.)
func (v *Visitor) testOwnedAdapterRef(adapterName string, participants ...string) string {
	if !v.options.testWhiteboxReference || v.options.testMetadataAnchorName == "" {
		return adapterName
	}

	anchor := v.options.testMetadataAnchorName

	if v.options.testInternalBridgeName != "" {
		for _, participant := range participants {
			if v.whiteboxBridgeDeclaredType(participant) {
				anchor = v.options.testInternalBridgeName
				break
			}
		}
	}

	return anchor + "." + adapterName
}

// whiteboxBridgeDeclaredType mirrors splitWhiteboxVariantRecords' isBridgeName — including its
// variant gate, since this decides the anchor a record's adapter is NAMED through and the two must
// agree. A BARE name in the bridge's declared-type set, read only while the BRIDGE is the variant
// under conversion: a bare name emitted by the external suite is external-declared whatever the
// bridge spells the same way (gob's two `Point` declarations), because the external variant reaches
// a bridge type only through the object-identity route, which qualifies it. The go/types half of
// the set is known before either variant converts; the LIFTED half (a function-local type promoted
// to package level) is claimed as the bridge itself converts, so it is read from the live claim set
// while the bridge is the variant under conversion — encoding/binary's `TestByteOrder_byteOrder` is
// exactly that shape.
func (v *Visitor) whiteboxBridgeDeclaredType(name string) bool {
	if name == "" || strings.Contains(name, ".") || v.options.testExternalVariant {
		return false
	}

	name = strings.TrimPrefix(name, ShadowVarMarker)

	if whiteboxBridgeTypeNames.Contains(name) {
		return true
	}

	return v.options.testInlineTypeAccess && packageLiftedTypeNames.Contains(name)
}

// whiteboxVariantAdapterStructName qualifies a BARE struct spelling with the white-box variant class
// that declares it — but ONLY for a name BOTH `-tests` variants declare.
//
// A deferred pointer-adapter marker carries the CAST's own spelling, and a cast written in the
// declaring scope spells its struct unqualified, which is why the resolver falls back to matching
// records by simple name. That fallback rests on "the pair resolves identically wherever it appears",
// and a white-box suite that declares one name TWICE is exactly where it does not: net/http declares
// `delegateReader` in requestwrite_test.go (package http) and again in transport_test.go (package
// http_test). Both records exist, each qualified by its own anchor, and both satisfied the fallback's
// anchor tie-break — so whichever came first in the accumulated union won and was substituted into
// EVERY file, handing the internal variant's cast the EXTERNAL variant's adapter class (CS1503,
// `ж<http_internal_test_package.delegateReader>` into `http_test_package.delegateReaderжReader`).
//
// The variant is known HERE and nowhere downstream, so state it: qualifying with this variant's own
// anchor makes the resolver's EXACT-key pass find the one record the cast belongs to, and no guess is
// left to make. Restricted to the ambiguous set — the same both-variants-declare-it set the record
// split keys on — so every unambiguous name keeps its bare spelling and resolves byte for byte as
// before. This is the POINTER form of the answer testOwnedAdapterRef already gives the VALUE form,
// whose comment names this path as reaching "the same answer through the deferred marker's
// emittedAdapterPairAnchors": true of one anchor, and not of two.
func (v *Visitor) whiteboxVariantAdapterStructName(structTypeName string) string {
	if !v.options.testWhiteboxReference || strings.Contains(structTypeName, ".") {
		return structTypeName
	}

	if !testAmbiguousLocalTypeNames.Contains(stripSanitizationMarkers(strings.TrimPrefix(structTypeName, ShadowVarMarker))) {
		return structTypeName
	}

	anchor := v.options.testMetadataAnchorName

	if !v.options.testExternalVariant && v.options.testInternalBridgeName != "" {
		anchor = v.options.testInternalBridgeName
	}

	if anchor == "" {
		return structTypeName
	}

	return anchor + "." + structTypeName
}

// valueAdapterTypeRef renders the reference to the generated VALUE-form foreign adapter
// class: `<structSimple>ᴠ<ifaceSimple>` (ValueAdapterInfix), emitted in the INTERFACE's
// package (the converting package), so the reference is the bare composed name.
func valueAdapterTypeRef(structTypeName string, interfaceTypeName string) string {
	structSimple := structTypeName

	if idx := strings.LastIndex(structSimple, "."); idx >= 0 {
		structSimple = structSimple[idx+1:]
	}

	// A keyword-escaped part (`@fixed`, or a pre-qualified `os_@fixed`) cannot carry its
	// marker into the composed class name — see stripSanitizationMarkers.
	structSimple = stripSanitizationMarkers(structSimple)

	ifaceSimple := interfaceTypeName

	// A deferred dynamic-type marker (an anonymous interface lifted in a not-yet-visited
	// file) must survive INTACT to the post-barrier resolution — it resolves as one unit
	// to the already-simple lifted name, so no strip applies.
	if !strings.Contains(ifaceSimple, dynamicTypeMarkerPrefix) {
		if idx := strings.LastIndex(ifaceSimple, "."); idx >= 0 {
			ifaceSimple = ifaceSimple[idx+1:]
		}

		ifaceSimple = stripSanitizationMarkers(ifaceSimple)
	}

	return structSimple + ValueAdapterInfix + ifaceSimple
}

// wholeTypeAliasAdapterBase rebuilds a pointer-adapter class BASE whose alias-qualified render
// collapsed to a WHOLE-TYPE `global using` alias — `imageꓸRGBA` = go.image_package.ΔRGBA,
// `ecdhꓸPublicKey` = go.crypto.ecdh_package.ΔPublicKey. Only a COLLISION-RENAMED type takes that
// route, and the alias is a single IDENTIFIER, not a qualified path, while the adapter is a MEMBER
// of the declaring package's class — so composing the adapter infix onto the alias names nothing
// (`imageꓸRGBAжImage` / `ecdhꓸPublicKeyж_ᴛ1`, CS0246). Rebuild the base as the file's package
// qualifier plus the type's EMITTED simple name, which is what the declaring generator composed the
// class from (image's own casts read `new ΔRGBAжImage(…)`). A render that already carries a
// qualifier — every un-renamed type, e.g. `ecdh.PrivateKey` — is returned untouched, so this is a
// no-op for them.
func wholeTypeAliasAdapterBase(aliasQualified, adapterBase, targetTypeName string) string {
	if strings.Contains(adapterBase, ".") {
		return adapterBase
	}

	if idx := strings.LastIndex(aliasQualified, "."); idx >= 0 {
		return aliasQualified[:idx+1] + simpleCSTypeName(targetTypeName)
	}

	return adapterBase
}

func adapterTypeRef(structTypeName string, interfaceTypeName string) string {
	ifaceSimple := interfaceTypeName

	// Keep a deferred dynamic-type marker intact (see valueAdapterTypeRef).
	if !strings.Contains(ifaceSimple, dynamicTypeMarkerPrefix) {
		if idx := strings.LastIndex(ifaceSimple, "."); idx >= 0 {
			ifaceSimple = ifaceSimple[idx+1:]
		}

		// A keyword-escaped interface part cannot carry its "@" marker into the composed
		// class name — see stripSanitizationMarkers.
		ifaceSimple = stripSanitizationMarkers(ifaceSimple)
	}

	// A GENERIC struct's closed type-argument list must TRAIL the adapter name, not sit inside
	// it. `nistCurve<ж<P224Point>>` + PointerPrefix + `Curve` otherwise composes the malformed
	// `nistCurve<ж<P224Point>>жCurve` (an identifier with `<…>` mid-name — CS1526). Split the
	// base name from its `<…>` args so the reference reads `nistCurveжCurve<ж<P224Point>>`:
	// base+PointerPrefix+iface NAMES the ONE generic adapter class the generator emits (from the
	// open `nistCurve<Point>` form), and the closed args instantiate it. Non-generic names have
	// no `<`, so this is a no-op for them. Keep in sync with ImplementGenerator's generic-adapter
	// emission (which composes the class name identically via Symbols.PointerPrefix).
	structBase := structTypeName
	typeArgs := ""

	if idx := strings.Index(structTypeName, "<"); idx >= 0 {
		structBase = structTypeName[:idx]
		typeArgs = structTypeName[idx:]
	}

	// Strip "@" keyword-escape markers from the FINAL segment of the base (the part the
	// adapter name composes onto — `@fixed` alone, or a pre-qualified `os_@fixed`); a dotted
	// qualifier's own segments are separate tokens where a leading marker stays legal.
	if idx := strings.LastIndex(structBase, "."); idx >= 0 {
		structBase = structBase[:idx+1] + stripSanitizationMarkers(structBase[idx+1:])
	} else {
		structBase = stripSanitizationMarkers(structBase)
	}

	// Whether the interface side needs a package qualifier depends on the package's FINAL
	// GoImplement record set — which is not known until every file has been visited and
	// writePackageInfoFile has applied its skips and prunes. Defer the name (see
	// adapterNameCollisions.go); resolveAdapterNameMarkers rewrites it in place afterwards.
	// An anonymous lifted interface keeps the inline composition: its DYNTYPE payload must
	// reach the earlier dynamic-type barrier intact, and a lifted name is unique by
	// construction, so it can never be the colliding member of a group.
	if strings.Contains(ifaceSimple, dynamicTypeMarkerPrefix) {
		return structBase + PointerPrefix + ifaceSimple + typeArgs
	}

	return adapterNameMarker(structBase, interfaceTypeName) + typeArgs
}

// interfaceTypeLiteralTarget reports the ANONYMOUS interface type literal a conversion targets --
// `interface{}(x)` / `interface{ Foo() int }(x)`, parenthesised or not -- or nil when the call's
// callee is anything else. A NAMED interface target is deliberately not reported: it has a
// types.Object, so isTypeConversion's ordinary peel already resolves it.
func interfaceTypeLiteralTarget(fun ast.Expr) *ast.InterfaceType {
	for {
		switch expr := fun.(type) {
		case *ast.ParenExpr:
			fun = expr.X
		case *ast.InterfaceType:
			return expr
		default:
			return nil
		}
	}
}
