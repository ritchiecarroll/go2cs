// visitInterfaceType.go - Gbtc
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
	"go/token"
	"go/types"
	"slices"
	"strings"
)

const InterfaceTypeAttributeMarker = ">>MARKER:INTERFACE_TYPE_ATTRS<<"
const InterfacePostAtributeMarker = ">>MARKER:POST_INTERFACE_ATTRS<<"
const InterfaceInheritanceMarker = ">>MARKER:INHERITED_INTERFACES<<"
const InterfaceConstraintMarker = ">>MARKER:INTERFACE_CONSTRAINTS<<"

// For interface types with generic constraints, we will be adding a C# type parameter to the
// converted Go interface to handle operators. Since methods in interfaces can have their own
// type constraints, we mark the type so that it will not conflict with generic method types
const TypeT = ShadowVarMarker + "T"

// Handles interface types in context of a TypeSpec
func (v *Visitor) visitInterfaceType(interfaceType *ast.InterfaceType, identType types.Type, name string, doc *ast.CommentGroup, lifted bool, target *strings.Builder) (interfaceTypeName string) {
	// Consume any pending publicized-type access modifier — an unexported interface used in an
	// exported signature (e.g. testing's `func MainStart(deps testDeps, …)`) is recorded by the
	// accessibility pre-pass and must emit `public` or it defaults to `internal` and is less
	// accessible than the exported member that references it (CS0051). Every other top-level
	// type-kind emitter consumes v.pendingTypeAccess (see visitStructType); the interface emitter
	// was the sole one dropping it. Read and clear at ENTRY: only the top-level declaration carries
	// it, so the lifted/anonymous interfaces visited recursively in the method-scan loop below (and
	// any nested struct/interface lifts) correctly see an empty value.
	access := v.pendingTypeAccess
	v.pendingTypeAccess = ""

	// A FUNCTION-LOCAL interface (declared, or lifted anonymously, inside a function body) has no Go
	// exportedness to read a modifier out of — see the struct twin in visitStructType and the rule in
	// localTypeAccess. Pin it internal so it agrees with the sibling types the same function declares.
	if access == "" {
		access = v.localTypeAccess()
	}

	for _, field := range interfaceType.Methods.List {
		// Check if this is an actual method (has a function type)
		if funcType, ok := field.Type.(*ast.FuncType); ok {
			var indentOffset int

			if v.inFunction {
				indentOffset = 1
			} else {
				indentOffset = -1
			}

			// Loop through function results to check if any are structs
			if funcType.Results != nil {
				for index, resultField := range funcType.Results.List {
					var fieldName string

					if resultField.Names == nil {
						fieldName = fmt.Sprintf("%sR%d", name, index)
					} else {
						fieldName = fmt.Sprintf("%s_%s", name, resultField.Names[0].Name)
					}

					// Check if the return type is a struct or pointer to a struct
					if structType, exprType := v.extractStructType(resultField.Type); structType != nil && !v.liftedTypeExists(structType) {
						v.indentLevel += indentOffset
						v.visitStructType(structType, exprType, fieldName, resultField.Comment, true, target)
						v.indentLevel -= indentOffset
					}

					// Check if the return type is an anonymous interface
					if interfaceType, exprType := v.extractInterfaceType(resultField.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
						v.indentLevel += indentOffset
						v.visitInterfaceType(interfaceType, exprType, fieldName, resultField.Comment, true, target)
						v.indentLevel -= indentOffset
					}
				}
			}

			// Loop through function parameters to check if any are structs
			if funcType.Params != nil {
				for _, paramField := range funcType.Params.List {
					for _, paramName := range paramField.Names {
						// Check if the parameter type is a struct or pointer to a struct
						if structType, exprType := v.extractStructType(paramField.Type); structType != nil && !v.liftedTypeExists(structType) {
							v.indentLevel += indentOffset
							v.visitStructType(structType, exprType, fmt.Sprintf("%s_%s", name, paramName.Name), paramField.Comment, true, target)
							v.indentLevel -= indentOffset
						}

						// Check if the parameter type is an anonymous interface
						if interfaceType, exprType := v.extractInterfaceType(paramField.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
							v.indentLevel += indentOffset
							v.visitInterfaceType(interfaceType, exprType, fmt.Sprintf("%s_%s", name, paramName.Name), paramField.Comment, true, target)
							v.indentLevel -= indentOffset
						}
					}
				}
			}
		}
	}

	var preLiftIndentLevel int

	// Intra-function type declarations are not allowed in C#
	if lifted {
		// Structurally IDENTICAL anonymous interfaces are ONE Go type — the interface twin of
		// visitStructType's dedup (that function's own anonLiftKey/lookupDynamicTypeName block),
		// which this type never had. Without it, two occurrences of the same anonymous interface
		// shape each mint their OWN nominally-distinct C# type, and passing one where the other
		// is expected fails even though Go calls them one type. Concretely: runtime's
		// `ifaceHash(i interface{ F() }, seed uintptr)` (alg.go, production) and hash_test.go's
		// `IfaceKey.i interface{ F() }` field share this exact shape; hash_test.go calls the
		// production function through the export_test.go bridge (`IfaceHash(k.i, 0)`), so
		// `k.i`'s field type must BE `ifaceHash_i`, not a second `IfaceKey_i` (CS1503).
		// Checked in order: the same-pass registry (lookupDynamicTypeName — a within-pass reuse
		// interfaces never had either, mirroring the struct side), then production's registry
		// (lookupProductionDynamicTypeName — the reference-model `-tests` case: a later test
		// pass adopting a name PRODUCTION already lifted and published via GoDynamicTypeLift;
		// see seedProductionDynamicTypeLifts). A hit means an equivalent type already compiles
		// somewhere reachable, so no new declaration is emitted at all — return its name.
		//
		// C#'s rule is TYPE accessibility >= MEMBER accessibility, so a reuse is only unsafe when
		// the member this lift names (liftNameNeedsPublicType — the segment after the lift name's
		// last underscore, i.e. the actual field/param Go declared) is EXPORTED. AnonymousInterfaces'
		// `WithInlineField.R` (exported field, so the "R" segment needs public) reusing
		// `takesReader_r` (internal by name — first character 't' — for an unrelated unexported
		// param) is exactly the case this correctly refuses: CS0050/CS0051/CS0052 if it didn't. An
		// unexported member — hash_test.go's `IfaceKey.i` field, the "i" segment — never conflicts
		// with ANY reuse: internal is C#'s accessibility floor, so `ifaceHash_i` (also internal) is
		// always safe for it, even though the COMBINED name "IfaceKey_i" itself reads public by
		// first character (an earlier, wrong version of this check compared combined names and
		// rejected that exact reuse). A FUNCTION-LOCAL lift needs no check at all: localTypeAccess
		// writes an EXPLICIT `internal` there, overriding name inference entirely, so reuse is
		// always safe regardless of case. Falling through to a fresh mint when the check fails is
		// always safe — it only forgoes a dedup opportunity, never breaks one.
		//
		// A THIRD disjunct used to also allow reuse whenever generatedTypeScope(existing) read
		// "public" — the struct twin's comment (visitStructType.go) has the full account of why it
		// was unsound (it cannot see localTypeAccess's override) and the measurement that showed
		// removing it costs no currently-safe dedup anywhere in the corpus (i9's cross-tier census,
		// 2026-09-01: 0 hits `-stdlib`-wide, 2 in reflect's own `-tests`, both false, 0 in the five
		// reflect-importer canaries' `-tests`).
		if signatureType := v.getType(interfaceType, false); signatureType != nil {
			signature := signatureType.String()
			existing := lookupDynamicTypeName(signature)
			admissible := existing != "" && (v.inFunction || !liftNameNeedsPublicType(name))

			// The PRODUCTION-registry arm carries an accessibility question the same-pass arm above
			// cannot have: its candidate may live in ANOTHER assembly, and this arm emits no
			// declaration at all. 2026-09-01, bisected to 5442b402e — `errors`' external suite (all
			// four of its test files are `package errors_test`, so its production csproj carries no
			// InternalsVisibleTo) adopted production's internal `is_typeᴛ1` for join_test.go's
			// function-local `interface{ Unwrap() []error }`: `join_test.cs(49,48): error CS0122:
			// 'errors_package.is_typeᴛ1' is inaccessible due to its protection level`. A
			// cross-assembly reuse is admissible only when the reused declaration is REACHABLE from
			// the assembly doing the reusing — see productionLiftReuseReachable, which is why
			// neither `v.inFunction` nor liftNameNeedsPublicType may ESCAPE the check here the way
			// they legitimately do above (both reason within ONE assembly, so they are conjoined
			// with the reachability rule, not replaced by it). Falling through to a fresh mint is
			// exactly what this emission did before 5442b402e.
			if existing == "" {
				existing = lookupProductionDynamicTypeName(signature)
				admissible = productionLiftReuseReachable(existing, v.options) &&
					(v.inFunction || !liftNameNeedsPublicType(name))
			}

			if admissible {
				if identType != nil {
					v.liftedTypeMap[identType] = existing
				}

				v.liftedTypeMap[signatureType] = existing

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

		interfaceTypeName = v.getUniqueLiftedTypeName(name)
		v.liftedTypeMap[identType] = interfaceTypeName
		v.liftedTypeMap[v.getType(interfaceType, false)] = interfaceTypeName

		// Lifted interfaces are shared across the package so other files can resolve
		// cross-file references to the anonymous type — INCLUDING function-scoped lifts,
		// which hoist to file level and are referenced from other files' CALL sites
		// (internal/trace's `readBatch(r interface{io.Reader; io.ByteReader})`, cast at
		// generation.go's `readBatch(r)`: the GoImplement attribute and the adapter class
		// name need the lifted name — the raw Go literal's `}` breaks the assembly
		// attribute parse, CS1730 cascade). Mirrors the lifted anonymous-struct
		// registration (visitStructType); see convertToInterfaceType for the resolution.
		if identType != nil {
			registerDynamicTypeName(identType.String(), interfaceTypeName)
		}

		if t := v.getType(interfaceType, false); t != nil {
			registerDynamicTypeName(t.String(), interfaceTypeName)
		}
	} else {
		interfaceTypeName = name
	}

	if target == nil {
		target = v.outputBuilder

		if !v.inFunction {
			target.WriteString(v.newline)
		}
	}

	v.writeDocString(target, doc, interfaceType.Pos())

	var structuralBases []string
	var canonicalStructuralBases []string
	structuralCovered := HashSet[string]{}

	// Structural (non-embedded) satisfaction of an imported interface is emitted as C#
	// interface inheritance: Go converts fs.File to io.Reader implicitly because the method
	// set suffices, but C# interfaces are nominal, so the declaration site must carry the
	// link (os's CopyFS passes an fs.File to io.Copy — CS1503). Inheritance is
	// identity-preserving (no adapter wrapper — the dynamic value flows through type asserts)
	// and free at every downstream conversion site. Skipped for lifted/dyn interfaces
	// (reflection-implemented) and for constraint interfaces (generic machinery).
	if !lifted && identType != nil {
		hasConstraint := false

		for _, method := range interfaceType.Methods.List {
			if len(method.Names) == 0 && method.Type != nil {
				if isConstraint, _ := v.isTypeConstraint(method.Type); isConstraint {
					hasConstraint = true
					break
				}
			}
		}

		if !hasConstraint {
			structuralBases, canonicalStructuralBases, structuralCovered = v.getStructuralInterfaceBases(interfaceType, identType)
		}
	}

	result := &strings.Builder{}
	inheritedInterfaces := []string{}
	canonicalInheritedInterfaces := []string{}
	typeConstraints := HashSet[ConstraintType]{}
	var operatorSets HashSet[OperatorSet]
	outerIndent := v.indent(v.indentLevel)

	// A GENERIC interface — one whose own Go type parameter is USED in its member signatures
	// (crypto/elliptic's `nistPoint[T]`, whose `Add(T, T) T` / `SetBytes([]byte) (T, error)`
	// mention T) — must carry its `<T>` type parameters (and any constraints) in C#, exactly
	// like a generic struct. Without them the declaration is arity-0 `interface nistPoint`, yet
	// a type-parameter constraint that references it renders the instantiated arity-1 name
	// `where Point : nistPoint<Point>` (getGenericDefinition/getAliasQualifiedTypeName spell it that way) — an
	// arity mismatch (CS0308) — and every bare `T` in a member is undefined (CS0246). Go's
	// OPERATOR constraint interfaces (`Ordered`) are arity-0 in Go, so getGenericDefinition
	// yields nothing for them and the separate `<ΔT>` operator machinery below is untouched.
	// Skipped for lifted/anonymous interfaces (reflection-implemented, never generic here).
	genericTypeParams := ""
	genericConstraints := ""

	if !lifted && identType != nil {
		genericTypeParams, genericConstraints = v.getGenericDefinition(identType)
	}

	result.WriteString(outerIndent)
	result.WriteString(fmt.Sprintf("[GoType%s]%spartial interface %s%s%s%s{", InterfaceTypeAttributeMarker, InterfacePostAtributeMarker, getSanitizedIdentifier(interfaceTypeName), genericTypeParams, InterfaceInheritanceMarker, InterfaceConstraintMarker))
	result.WriteString(v.newline)

	v.indentLevel++
	innerIndent := v.indent(v.indentLevel)

	for _, method := range interfaceType.Methods.List {
		if len(method.Names) == 1 {
			// A declared member covered by a structural base is inherited, not re-declared —
			// redeclaring would HIDE the base member (distinct C# members, implementers would
			// need both). Its signature is guaranteed compatible: the base was only chosen
			// because types.Implements matched the full method set.
			if structuralCovered.Contains(method.Names[0].Name) {
				continue
			}

			v.writeDocString(result, method.Doc, method.Pos())

			goMethodName := method.Names[0].Name
			csMethodName := getSanitizedFunctionName(goMethodName)
			typeLenDeviation := token.Pos(len(csMethodName) - len(goMethodName))
			methodType := v.info.ObjectOf(method.Names[0]).(*types.Func)

			if methodType == nil {
				panic("@visitInterfaceType - Failed to find interface method \"" + goMethodName + "\" in the type info")
			}

			signature := methodType.Signature()
			resultSignature := v.generateResultSignature(signature)
			parameterSignature, _ := v.generateParametersSignature(signature, false)

			typeLenDeviation += token.Pos(len(parameterSignature) - v.getSourceParameterSignatureLen(signature))
			typeLenDeviation += token.Pos(len(resultSignature) - v.getSourceResultSignatureLen(signature))

			result.WriteString(fmt.Sprintf("%s%s %s(%s);", innerIndent, resultSignature, csMethodName, parameterSignature))
			v.writeCommentString(result, method.Comment, method.Type.End()+typeLenDeviation)
			result.WriteString(v.newline)
		} else if method.Type != nil {
			if isConstraint, methodCount := v.isTypeConstraint(method.Type); isConstraint {
				// Collapse multi-line constraint unions (e.g. "~int | ... | ~string" spanning
				// several source lines) onto a single comment line; otherwise the continuation
				// lines are emitted as raw, uncompilable C# inside the interface body.
				constraintText := strings.Join(strings.Fields(v.getPrintedNode(method.Type)), " ")
				result.WriteString(fmt.Sprintf("%s//  Type constraints: %s%s", innerIndent, constraintText, v.newline))
				typeConstraints.UnionWithSet(v.getConstraintTypeSetFromExpr(method.Type))
				operatorSets = getOperatorSet(typeConstraints)
				result.WriteString(fmt.Sprintf("%s// Derived operators: %s%s", innerIndent, getOperatorSetAsString(operatorSets), v.newline))

				// If type constraint constains any methods, add it to the inherited interfaces
				if methodCount > 0 {
					inheritedInterfaces = append(inheritedInterfaces, fmt.Sprintf("%s<%s>", v.convExpr(method.Type, nil), TypeT))
				}
			} else {
				// Go's built-in `comparable` is not a C# base. It admits every ==-able type, a set no
				// C# interface describes — golib's `comparable<T>` CRTP is implemented by NOTHING,
				// which is exactly why the bare-constraint arm emits no C# constraint for it
				// (constraintOperations.go). Embedded, it was appended to the inheritance list as a
				// bare `comparable`, which is that unimplementable generic named with no type
				// argument: CS0305 on net/netip's `type netipTypeCmp interface{ comparable; netipType }`.
				// It contributes no methods, so dropping it leaves the interface's C# surface exact —
				// and leaves it a method set, which is the form the constraint side can express.
				if embedType := v.getType(method.Type, false); embedType != nil && isPredeclaredComparable(embedType) {
					continue
				}

				isDynamicInterface := v.isDynamicInterface(method.Type)

				if isDynamicInterface {
					v.indentLevel--
					v.removeLastLineFeed(result)
					v.removeLastLineFeed(v.outputBuilder)
				}

				// A LIFTED interface EMBEDDED in another interface must name the lifted
				// declaration. reflect's TestMethodPkgPath declares `type I interface{…}` and
				// then `type i interface{ I; … }` in the same function body: both hoist to
				// member level, but the embed rendered the bare Go name `I`, which exists
				// nowhere after the hoist — CS0246, with the rest of all_test.cs behind it.
				// Re-resolve through liftedNameFor first: the same hoist-aware lookup
				// visitArrayType / visitChanType / visitMapType already make for their element,
				// key and value types. A package-level embed is not in the map and renders
				// exactly as before, so nothing else in the corpus moves.
				embedName := ""

				if embedType := v.getType(method.Type, false); embedType != nil {
					if liftedEmbed, ok := v.liftedNameFor(embedType); ok {
						embedName = getSanitizedIdentifier(liftedEmbed)
					}
				}

				if embedName == "" {
					embedName = v.convExpr(method.Type, nil)
				}

				inheritedInterfaces = append(inheritedInterfaces, embedName)

				// Track the CANONICAL (full-name) render too: the duplicate-implementation
				// prune keys interfaceImplementations by getFullyQualifiedTypeName (a FOREIGN embed
				// records `go.io.fs_package.FileInfo`), so the alias render alone
				// (`fs.FileInfo`) never matches and both the derived and base impls emit
				// the same explicit members (zip headerFileInfo : fileInfoDirEntry +
				// fs.FileInfo, CS8646 ×6/CS0111 ×2).
				if embedType := v.getType(method.Type, false); embedType != nil {
					canonicalName := convertToCSTypeName(v.getFullyQualifiedTypeName(embedType, false))

					if canonicalName != "" {
						canonicalInheritedInterfaces = append(canonicalInheritedInterfaces, canonicalName)
					}
				}

				if isDynamicInterface {
					v.indentLevel++
					v.outputBuilder.WriteString(v.newline)
				}
			}
		} else {
			panic("@visitInterfaceType - Unexpected method declaration in interface: %s" + v.getPrintedNode(method))
		}
	}

	v.indentLevel--
	result.WriteString(v.indent(v.indentLevel))
	result.WriteRune('}')
	result.WriteString(v.newline)

	// Structural bases follow declared embeds, deduplicated against them
	for _, base := range structuralBases {
		if !slices.Contains(inheritedInterfaces, base) {
			inheritedInterfaces = append(inheritedInterfaces, base)
		}
	}

	inheritedResult := ""

	// The CRTP `<ΔT>` marker list serves Go's ARITY-0 constraint interfaces (`Ordered`,
	// `Number`) — a GENERIC constraint interface (`PtrOf[T any] interface{ *T }`) already
	// carries its own `<T>` list above, and appending both produced a malformed double list
	// (`PtrOf<T><ΔT>`, CS1003).
	if len(typeConstraints) > 0 && genericTypeParams == "" {
		inheritedResult = fmt.Sprintf("%s<%s>", inheritedResult, TypeT)
	}

	interfaceAttrs := ""
	postAttrs := " "

	if lifted {
		// Add "dyn" implementation attribute to lifted types since
		// they cannot be directly implemented in C# code. For these
		// types, a reflection based type implementation is used when
		// type assertions and comparisons are needed.
		interfaceAttrs = "dyn"
	}

	// The generated operator machinery, like the `<ΔT>` marker list above, serves ARITY-0
	// constraint interfaces — for a GENERIC constraint interface (`PtrOf[T any] interface{ *T }`)
	// the go2cs-gen InterfaceTypeTemplate would reference the marker parameter the declaration
	// no longer carries (CS0246 on ΔT in the .g.cs).
	if len(operatorSets) > 0 && genericTypeParams == "" {
		if len(interfaceAttrs) > 0 {
			interfaceAttrs += "; "
		}

		interfaceAttrs += fmt.Sprintf("operators = %s", getOperatorSetAttributes(operatorSets))
		postAttrs = v.newline
	}

	if len(interfaceAttrs) > 0 {
		interfaceAttrs = fmt.Sprintf("(\"%s\")", interfaceAttrs)
	}

	// Inject the publicized access modifier (if any) into the slot between `[GoType…]` and
	// `partial interface`. postAttrs is " " normally or a newline when operator sets are present;
	// appending `access` ("public ") yields `[GoType] public partial interface` (or the newline
	// form `[GoType(…)]\npublic partial interface`). Empty when not publicized — no churn.
	postAttrs += access

	// The declaration's type-parameter list for the package_info.cs accessibility section: a
	// GENERIC interface carries its own `<T…>`, while an arity-0 CONSTRAINT interface carries the
	// CRTP `<ΔT>` marker the inheritance slot supplies above (the two are mutually exclusive by the
	// same condition used there). The section's part must repeat the list verbatim — partial
	// declarations of a generic type agree on parameter names.
	declaredTypeParams := genericTypeParams

	if len(typeConstraints) > 0 && genericTypeParams == "" {
		declaredTypeParams = fmt.Sprintf("<%s>", TypeT)
	}

	// A lifted function-local NAMED interface carries its original Go name, exactly as
	// visitStructType stamps a lifted function-local struct. Without it the pair is asymmetric —
	// 68 stamped struct lifts against 0 stamped interface lifts corpus-wide — and go2cs-gen's
	// embedded-interface promotion cannot see the embed at all: it identifies an embedded interface
	// field by matching the FIELD name to the interface type's simple name, and a function-local
	// lift renames the TYPE to `<Func>_<name>` while the field keeps `<name>`. For
	// `func TestCallPanic() { type T1 interface{…}; type T2 struct { T1 } }` the field is `T1` and
	// the type is `TestCallPanic_T1`, the match fails, and promotion falls back to naming the TYPE
	// as the accessor — `recvᴛ.TestCallPanic_T1.Y()`, a member that does not exist (CS1061/CS0120,
	// reflect's all_test).
	//
	// Anonymous lifts have no Go name to stamp, matching the struct side's condition.
	var localNameAttr string

	if lifted && v.inFunction {
		if named, ok := identType.(*types.Named); ok {
			localNameAttr = fmt.Sprintf("[GoLocalName(\"%s\")] ", named.Obj().Name())
		}
	}

	v.recordTypeAccessibility("interface", getSanitizedIdentifier(interfaceTypeName), declaredTypeParams, access, localNameAttr)

	if len(inheritedInterfaces) > 0 {
		inheritedResult += " :" + v.newline

		for i, inheritedInterface := range inheritedInterfaces {
			if i > 0 {
				inheritedResult += "," + v.newline
			}

			inheritedResult += innerIndent + inheritedInterface
		}

		inheritedResult += v.newline + outerIndent

		// Track which interfaces this interface inherits from so duplicate interface
		// implementations can be avoided. The set carries BOTH the alias render (emission
		// form) and the canonical full-name render — the prune's implementation-map keys
		// are canonical, so the alias form alone never matched a foreign base (zip's
		// headerFileInfo implemented fs.FileInfo twice, CS8646 x6).
		trackedInheritances := append([]string{}, inheritedInterfaces...)
		trackedInheritances = append(trackedInheritances, canonicalInheritedInterfaces...)
		trackedInheritances = append(trackedInheritances, canonicalStructuralBases...)

		packageLock.Lock()
		interfaceInheritances[interfaceTypeName] = NewHashSet(trackedInheritances)
		packageLock.Unlock()
	} else {
		inheritedResult += " "
	}

	target.WriteString(strings.ReplaceAll(strings.ReplaceAll(strings.ReplaceAll(strings.ReplaceAll(result.String(),
		InterfaceTypeAttributeMarker, interfaceAttrs),
		InterfacePostAtributeMarker, postAttrs),
		InterfaceInheritanceMarker, inheritedResult),
		InterfaceConstraintMarker, genericConstraints))

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

// getStructuralInterfaceBases finds EXPORTED method interfaces from directly imported packages
// whose method sets are STRICT subsets of the declared interface's — Go satisfies such
// conversions structurally, so the C# declaration inherits them and skips re-declaring the
// covered members. The strict-subset guard also rules out inheritance cycles (equal method
// sets can never inherit each other). Candidates already covered by a declared EMBED are
// skipped (the embed emission handles those, and a second differently-rendered base of the
// same type would be a duplicate-interface error). Returns the rendered base type names and
// the covered method names.
func (v *Visitor) getStructuralInterfaceBases(interfaceType *ast.InterfaceType, identType types.Type) ([]string, []string, HashSet[string]) {
	named, ok := identType.(*types.Named)

	if !ok {
		return nil, nil, nil
	}

	iface, ok := named.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 {
		return nil, nil, nil
	}

	var embeddedTypes []types.Type

	for _, method := range interfaceType.Methods.List {
		if len(method.Names) == 0 && method.Type != nil {
			if embedType := v.getType(method.Type, false); embedType != nil {
				if _, isIface := embedType.Underlying().(*types.Interface); isIface {
					embeddedTypes = append(embeddedTypes, embedType)
				}
			}
		}
	}

	var bases []*types.Named

	for _, imported := range v.pkg.Imports() {
		scope := imported.Scope()

		for _, name := range scope.Names() {
			typeName, ok := scope.Lookup(name).(*types.TypeName)

			if !ok || !typeName.Exported() || typeName.IsAlias() {
				continue
			}

			candidate, ok := typeName.Type().(*types.Named)

			if !ok || candidate.TypeParams().Len() > 0 {
				continue
			}

			candidateIface, ok := candidate.Underlying().(*types.Interface)

			if !ok || candidateIface.NumMethods() == 0 || candidateIface.NumMethods() >= iface.NumMethods() || !candidateIface.IsMethodSet() {
				continue
			}

			if !types.Implements(named, candidateIface) {
				continue
			}

			// A declared embed that implements the candidate already carries its members
			coveredByEmbed := false

			for _, embedType := range embeddedTypes {
				if types.Implements(embedType, candidateIface) {
					coveredByEmbed = true
					break
				}
			}

			if !coveredByEmbed {
				bases = append(bases, candidate)
			}
		}
	}

	if len(bases) == 0 {
		return nil, nil, nil
	}

	// Keep the minimal covering set: drop a base another chosen base already implements
	// (fs.File satisfies io.Reader, io.Closer AND io.ReadCloser — only ReadCloser is
	// listed; C# reaches the others through it). Equal-sized sets tie-break by index so
	// mutual implementers cannot drop each other both ways.
	baseNames := make([]string, 0, len(bases))
	canonicalBaseNames := make([]string, 0, len(bases))
	coveredCounts := map[string]int{}

	for i, candidate := range bases {
		candidateIface := candidate.Underlying().(*types.Interface)
		subsumed := false

		for j, other := range bases {
			if i == j {
				continue
			}

			otherIface := other.Underlying().(*types.Interface)

			if (otherIface.NumMethods() > candidateIface.NumMethods() ||
				(otherIface.NumMethods() == candidateIface.NumMethods() && j < i)) &&
				types.Implements(other, candidateIface) {
				subsumed = true
				break
			}
		}

		if subsumed {
			continue
		}

		// Reference through the file-local package ALIAS (`CrossPkgLib.Labeled`, user-ruled
		// style, mirroring the foreign-adapter references): getAliasQualifiedTypeName both yields the
		// aliased form and registers the file-local using — needed because the declaring
		// Go FILE may not import the candidate's package (fs.go declares File without
		// importing io).
		baseNames = append(baseNames, convertToCSTypeName(v.getAliasQualifiedTypeName(candidate, false)))

		// The CANONICAL (full-name) render feeds the duplicate-implementation prune,
		// which keys interfaceImplementations by getFullyQualifiedTypeName (see visit tracking).
		canonicalBaseNames = append(canonicalBaseNames, convertToCSTypeName(v.getFullyQualifiedTypeName(candidate, false)))

		// The base's FULL method set (embedded members included) is inherited
		for k := 0; k < candidateIface.NumMethods(); k++ {
			coveredCounts[candidateIface.Method(k).Name()]++
		}
	}

	// A member covered by exactly ONE listed base is inherited and skipped. A member covered
	// by TWO OR MORE bases (interfaces that share a method without subsuming each other) is
	// RE-DECLARED instead: the redeclaration hides both inherited slots, so a call through
	// this interface stays unambiguous (CS0121) — Go needs only one method to satisfy all.
	covered := HashSet[string]{}

	for name, count := range coveredCounts {
		if count == 1 {
			covered.Add(name)
		}
	}

	return baseNames, canonicalBaseNames, covered
}

func (v *Visitor) getSourceParameterSignatureLen(signature *types.Signature) int {
	parameters := signature.Params()

	if parameters == nil {
		return 0
	}

	result := 0

	for i := 0; i < parameters.Len(); i++ {
		param := parameters.At(i)

		if i > 0 {
			result += 2
		}

		result += len(v.getAliasQualifiedTypeName(param.Type(), false))

		if param.Name() != "" {
			result += 1 + len(param.Name())
		}
	}

	return result
}

func (v *Visitor) getSourceResultSignatureLen(signature *types.Signature) int {
	results := signature.Results()

	if results == nil {
		return 0
	}

	if results.Len() == 1 {
		return len(v.getAliasQualifiedTypeName(results.At(0).Type(), false))
	}

	result := 2

	for i := 0; i < results.Len(); i++ {
		if i > 0 {
			result += 2
		}

		result += len(v.getAliasQualifiedTypeName(results.At(i).Type(), false))
	}

	return result
}
