// convIdent.go - Gbtc
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
	"strings"
)

// identIsUniverseNil reports whether an identifier IS the Go literal `nil`. `nil` is a PREDECLARED
// identifier in universe scope, not a keyword, so a user object may shadow it (`nil := 5`) — only
// one that resolves to the universe nil is the literal, and a shadowing object renders as an
// ordinary identifier. A synthetic ident with no type info keeps the literal reading.
//
// Whether that literal then emits golib's `nil` or the typeless `default!` is a matter of RENDER
// context and belongs to the caller (see convIdent); callers that must know only whether they are
// looking at a nil — such as the ambiguous one-field constructor argument in convCompositeLit —
// share this predicate rather than re-deriving it.
func (v *Visitor) identIsUniverseNil(ident *ast.Ident) bool {
	if ident == nil || ident.Name != "nil" {
		return false
	}

	obj := v.info.ObjectOf(ident)
	_, isUniverseNil := obj.(*types.Nil)

	return isUniverseNil || obj == nil
}

func (v *Visitor) convIdent(ident *ast.Ident, context IdentContext) string {
	// A selected method remains an extension-method member name. External white-box files import
	// the bridge statically; inserting the class between receiver and method would form
	// `recv.bridge.Method`, which is not a C# extension invocation.
	if !context.isMethod && v.whiteboxBridgeUse(ident) {
		return v.whiteboxBridgeMember(ident)
	}

	// A package qualifier (`runtime.Goexit()`) renders its using alias, which is
	// collision-renamed when a same-named child namespace is visible from the import
	// closure (CS0576 — see importAliasOperations.go).
	if pkgName, isPkg := v.identifierIsPackageName(ident); isPkg {
		if renamed, ok := packageImportAliasRenames[pkgName]; ok {
			return getSanitizedImport(renamed)
		}
	}

	// The Go literal `nil` (see identIsUniverseNil): pointer context renders golib's `nil`, value
	// context the typeless `default!`. A shadowing object is not the literal and falls through to
	// normal identifier rendering, mirroring the `true`/`false` handling below.
	if v.identIsUniverseNil(ident) {
		// A nil bound for a pointer-to-ARRAY target carries the array's LENGTH, which `array<E>`
		// erases and nothing downstream can recover -- the position that knows the target puts it
		// in nilArrayTarget (see appendNilArrayDimsContext). Ahead of both returns below because
		// the cargo IS the pointer's nil value, in either context.
		if context.nilArrayTarget != nil {
			if nilArray := v.nilArrayPtrValueForTarget(context.nilArrayTarget); nilArray != "" {
				return nilArray
			}
		}

		if context.isPointer {
			return "nil"
		}

		return "default!"
	}

	// `true`/`false` are C# KEYWORDS but Go PREDECLARED identifiers (universe scope). A VALUE
	// use resolves to that universe-scope const — emit the bare C# literal. A shadowing
	// VARIABLE/parameter named `true`/`false` (text/template/parse's `newBool(pos Pos, true
	// bool)`) has a package-scoped object and falls through to the escaped `@true`/`@false`
	// below (`true`/`false` are in the keyword-escape set).
	if ident.Name == "true" || ident.Name == "false" {
		if c, ok := v.info.ObjectOf(ident).(*types.Const); ok && c.Pkg() == nil {
			return ident.Name
		}
	}

	if context.isPointer {
		// Check if the identifier is an unsafe pointer
		if basic, ok := v.getIdentType(ident).(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
			// Sanitize the name so a C# keyword identifier (e.g. an `unsafe.Pointer`
			// parameter named `new`, as in internal/runtime/atomic) is escaped to `@new`
			// rather than emitted bare, which C# parses as the `new` operator (CS1526).
			return fmt.Sprintf("%s.Value", getSanitizedIdentifier(v.getIdentName(ident)))
		}

		// A direct-ж method's receiver used as a bare pointer value (e.g. `recv.field = recv`):
		// the receiver box is the parameter `Ꮡrecv`, so emit that. A value-ref receiver
		// (`this ref T recv`) has no box and its deref'd value cannot stand in for the pointer
		// — this is why such methods are marked direct-ж (see packageDirectBoxReceiverMethods).
		// Object identity, not name: a shadowing local keeps its own render (identResolvesToReceiver).
		if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(ident, recvName) && isDirectBoxReceiverMethod(v.currentFuncDecl, v.info) {
			return AddressPrefix + strings.TrimPrefix(v.getIdentName(ident), "@")
		}

		var identEscapesHeap bool
		obj := v.info.ObjectOf(ident)

		if obj != nil {
			identEscapesHeap = v.identEscapesHeap[obj]
		}

		identType := v.getIdentType(ident)

		// The current method's REF receiver has NO box — only a direct-ж method carries
		// Ꮡrecv (handled above), and receivers are never heap-decl'd. An ESCAPE-MARKED
		// receiver must not take the heap-box render below: flate init's
		// `d.fill = (*compressor).fillStore` emitted a nonexistent Ꮡd (CS0103 ×11).
		isCurrentRefReceiver := false

		if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(ident, recvName) {
			isCurrentRefReceiver = true
		}

		// Check if the identifier is not already a pointer type or is a parameter or escapes heap,
		// in these cases, we need to add the address operator to reference the pointer variable.
		// The box keeps the RAW Go name (`Ꮡp`), even when the value alias is collision-renamed
		// (`Δp`) — an escaping local is `ref var Δp = ref heap(new T(), out var Ꮡp)`, a deref'd
		// pointer param `ref var Δp = ref Ꮡp.Value` — so reference it by the raw name, not `ᏑΔp`
		// (not in scope → CS0103). boxBaseName is a no-op when nothing is shadow-renamed (no churn).
		// NOTE: this arm renders the ident's POINTER-form for a VALUE-type local (its box) — an
		// inherently heap-allocated local stays on the plain render even when it now owns an
		// address-taken box (identHasHeapBox): here the pointer VALUE is the ident itself
		// (`new Middle(Inner: inner)`), and `Ꮡinner` (the ж<ж<T>> box) is only what an explicit
		// `&inner` wants — that renders through convUnaryExpr.
		if !isCurrentRefReceiver {
			if _, ok := identType.(*types.Pointer); !ok || v.identIsParameter(ident) || (identEscapesHeap && !isInherentlyHeapAllocatedType(identType)) {
				return AddressPrefix + v.boxBaseName(ident)
			}
		}
	}

	if context.isType {
		// A reference to a function-local (lifted) named type must use the lifted C# name
		// (`makePoint_point`), not the bare Go name — e.g. a composite literal `point{…}` or a
		// cast `point(x)`. Only lifted/anonymous types are in liftedTypeMap, so package-level
		// types are unaffected.
		if identType := v.getIdentType(ident); identType != nil {
			if liftedName, ok := v.liftedTypeMap[identType]; ok {
				return liftedName
			}

			// A DOT-IMPORTED (`. "go/types"`) foreign type is referenced BARE, so there is no
			// selector for the qualified-name resolver to rewrite — yet the type may be
			// collision-renamed inside its own package, in which case the raw Go name binds
			// nothing (CS0246). go/types Δ-renames both `Error` (its own `Error()` method) and
			// `Info` (`Basic.Info()`), so internal/types/errors' external test file emitted
			// `err._<Error>(ᐧ)` and `new Info(…)` against declarations named ΔError/ΔInfo —
			// while its package_test_info.cs had already minted the right
			// `typesꓸError`/`typesꓸInfo` aliases and left them unused. Route through the same
			// recorded-alias lookup the QUALIFIED path takes (getScopeCheckedTypeName /
			// getCSharpTypeName both consult it), so one Go type cannot have two spellings
			// depending on whether the source named it bare or through its package.
			//
			// Only the AST-ident type positions reach here — a type-assertion target and a
			// composite-literal type. The type-driven positions (declarations, parameters,
			// conversions) resolve from types.Type and were already correct, which is why
			// `var mu Mutex` through a dot import has always worked (DotImportRenamedPackage).
			// foreignAliasedTypeName is a no-op for a same-package type and for any type with
			// no recorded alias, so nothing else moves.
			if aliased, ok := v.foreignAliasedTypeName(identType); ok {
				return aliased
			}
		}

		return convertToCSTypeName(v.getIdentName(ident))
	}

	if context.isMethod {
		var name string

		// A FIELD selection must not take the function-name Main→ΔMain special (the C#
		// entry-point reservation applies to METHODS): runtime/debug's BuildInfo.Main
		// field access emitted `bi.ΔMain` against the raw `Main` declaration (CS1061 ×2).
		// Fields otherwise share the function-name path (core sanitize, no package-level
		// nameCollisions Δ — a field is struct-scoped and declared unrenamed). Use the field's
		// OWN Go name, NOT getIdentName: a field is struct-scoped, so a same-named shadow-renamed
		// LOCAL var must not rewrite the field selector — compress/bzip2's `for i, length := range`
		// renamed the loop var to `lengthΔ1`, and `pairs[i].length = length` then emitted the field
		// access as `pairs[i].lengthΔ1` against the `length` field decl (CS1061).
		if context.isField {
			name = getCoreSanitizedIdentifier(ident.Name)
		} else {
			name = getSanitizedFunctionName(v.getIdentName(ident))

			// A `-tests` variant Δ-renamed this test-file method declarator to keep production
			// symbol names immutable (B2/B9, see performNameCollisionAnalysis) — matched by
			// OBJECT identity, so the same-named production type / dot-imported function keeps
			// its plain emission at every other site.
			if testMethodRenames[v.info.ObjectOf(ident)] {
				name = ShadowVarMarker + name
			}
		}

		// A field whose name equals its enclosing struct's type name is renamed with the
		// disambiguation marker (CS0542); match the renamed declaration at the access site.
		if context.fieldCollidesWithType {
			name = typeCollidingFieldName(name)

			// A CROSS-package field whose enclosing type is itself Δ-renamed in ITS package (a
			// type-vs-method collision the current pkg's nameCollisions can't see) declares the field
			// DOUBLE-marked (ΔΔLabel); typeCollidingFieldName only single-marked it here, so upgrade to
			// match (internal/trace/testtrace's `l.Label`, CS1061). In-package fields already doubled.
			if context.fieldTypeIsRenamed && strings.HasPrefix(name, ShadowVarMarker) && !strings.HasPrefix(name, ShadowVarMarker+ShadowVarMarker) {
				name = ShadowVarMarker + name
			}
		}

		return name
	}

	// A heap-boxed local captured by-box inside a lambda is read through its box: the ref-local
	// alias `ref var m = ref Ꮡm.Value` can't be captured by the closure (CS8175), so a value use must
	// deref the box directly (`Ꮡm.Value`). The box `Ꮡm` is a capturable reference. Address uses
	// (`&m`, `&m.field`) are rendered from the box name in convUnaryExpr, bypassing this rewrite.
	// A heap-boxed PARAM of the FUNCTION LITERAL currently being converted is excluded: its box
	// prologue re-declares the ref alias INSIDE this very lambda (see convFuncLit), so the alias
	// is a plain local here — only a NESTED lambda (whose enterLambdaConversion context replaces
	// currentConversion) must read it through the box.
	if v.lambdaCapture != nil && v.lambdaCapture.conversionInLambda && v.isLambdaBoxRefVar(v.info.ObjectOf(ident)) &&
		!v.identIsCurrentFuncLitParam(ident) {
		// The current method's REF receiver has NO box — a genuine closure capture would
		// have promoted the method direct-ж (bodyCapturesReceiverInClosure); reaching here
		// as a ref receiver means a PSEUDO-lambda conversion context (a method-value assign)
		// touched an escape-marked receiver, and the box render is a nonexistent name
		// (flate init's `d.fill = (*compressor).fillStore`, CS0103 ×11).
		isRefReceiver := false

		if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(ident, recvName) {
			isRefReceiver = !isDirectBoxReceiverMethod(v.currentFuncDecl, v.info)
		}

		// A Phase-A ref-LOWERED PARAMETER is box-less for the SAME reason and reaches the SAME
		// render: `doInRoot(r *Root)` lowers to `ref Root r`, so `Ꮡr` names nothing and the
		// deferred `r.root.decref()` emitted `Ꮡr.Value.root.decref` — CS0103, os 1.24's
		// root_openat.cs:123. The comment above records this identical class for a ref RECEIVER;
		// ref-lowering later made a pointer PARAMETER box-less the same way and this guard was
		// never widened to it. The question the render needs is "does this ident have a box at
		// all", not "is this a ref receiver".
		//
		// Read the RECORDED decision — LoweredParamVars, keyed by *types.Var identity — rather
		// than re-deriving it, so the emitter cannot disagree with the analysis that made it.
		identHasNoBox := isRefReceiver || v.paramIsRefLoweredObj(v.info.ObjectOf(ident))

		if !identHasNoBox {
			// For a box-of-POINTER (or other inherently-heap) local — `Ꮡm` is a `ж<ж<T>>` — reading the box
			// is reading the HELD pointer value, not a dereference of it, so it must use `.ValueSlot` (no
			// nil-pointer-dereference check): in Go reading `*(&p)` for a nil `*T`/slice/map yields the nil
			// value, no panic. The box of a value-struct local (`ж<box>`) or a deref'd pointer PARAMETER
			// (`ж<pointed-to-T>`) is a genuine dereference, so it keeps the strict `.Value`.
			valAccessor := ".Value"
			if v.isBoxedPointerLocal(ident) {
				valAccessor = ".ValueSlot"
			}
			return AddressPrefix + v.boxBaseName(ident) + valAccessor
		}
	}

	// A same-package GLOBAL variable reference shadowed by a same-named function-level LOCAL: C# locals
	// are function-scoped, so the bare global name binds to the local everywhere in the function — a use
	// of the global BEFORE the local's declaration is CS0841 (and would read the wrong variable
	// regardless). Runtime `traceallocfree.traceSnapshotMemory` reads the global `trace.minPageHeapAddr`
	// before declaring the local `trace := traceAcquire()`; both are collision-renamed `Δtrace`. Qualify
	// the global with its package static class (`runtime_package.Δtrace`), which a local can never shadow.
	// Gated to an ident that resolves to a package-level var of THIS package with a same-named
	// function-level local — so an ordinary global (no shadowing local) and the local's own uses (which
	// resolve to the local, not the package scope) are untouched.
	if v.funcLevelDecls != nil {
		if _, shadowed := v.funcLevelDecls[ident.Name]; shadowed && v.pkg != nil {
			if vr, ok := v.info.ObjectOf(ident).(*types.Var); ok && vr.Parent() == v.pkg.Scope() {
				return v.packageScopeClassName(vr) + "." + getSanitizedIdentifier(v.getIdentName(ident))
			}
		}
	}

	// The same collision against a package-level CONST. Go's `q := big.NewInt(q)` (crypto/internal/
	// mlkem768's TestZetas/TestGammas, over `const q = 3329`) is legal because a short var decl's
	// scope starts AFTER its own ValueSpec, so the initializer still reads the constant; C# scopes
	// the local to the whole block and the initializer binds to the local it is declaring — CS0841.
	//
	// A const cannot fall back on the local-rename half of this defence: performVariableAnalysis's
	// usedPackageVarNames pre-scan records only objects that are *types.Var and live in globalScope
	// (which is map[string]*types.Var), so a const-shadowing local is never shadow-renamed. That is
	// why the const arm qualifies rather than renames, and why it uses the WIDER local set:
	// funcLevelDecls holds only declarations directly in the function body, but the same shape
	// inside an `if`/`for` init is not function-level, while funcScopeVarNames is every variable
	// declared anywhere in the function (nested blocks and func literals included). Qualifying a
	// reference that no local actually shadows costs verbosity and never changes meaning, so the
	// wider set is the safe side to err on.
	if v.pkg != nil && v.funcScopeVarNames.Contains(ident.Name) {
		if cn, ok := v.info.ObjectOf(ident).(*types.Const); ok && cn.Parent() == v.pkg.Scope() {
			return v.packageScopeClassName(cn) + "." + getSanitizedIdentifier(v.getIdentName(ident))
		}
	}

	// A PACKAGE ident whose using-alias is shadowed by a same-package method/function name
	// (`func (s *byLiteral) sort(…)` vs `import "sort"` — the member lookup binds the method
	// group before the alias, CS0119, compress/flate) qualifies through the _package class.
	if packageFuncMethodNames != nil && packageFuncMethodNames[ident.Name] {
		if pkgName, ok := v.info.ObjectOf(ident).(*types.PkgName); ok {
			return rootQualifyIfAmbiguous(convertImportPathToNamespace(pkgName.Imported().Path(), PackageSuffix))
		}
	}

	// The arms below each decide the identifier's SPELLING, and the generic-function-VALUE
	// type-argument append at the end applies to whichever one wins — so a Δ-renamed or
	// bridge-qualified generic function passed as a method group is spelled out too.
	name := ""

	// A `-tests` variant Δ-renamed this test-file declarator to keep production symbol names
	// immutable (see performNameCollisionAnalysis). A METHOD name reaches the isMethod arm above,
	// but a package-level FREE FUNCTION referenced as a call target or a function value falls
	// through to here — and its reference must follow the rename or it binds to nothing (CS0103).
	// Object identity, so a same-named production symbol keeps its plain emission.
	if testMethodRenames[v.info.ObjectOf(ident)] {
		name = ShadowVarMarker + getSanitizedIdentifier(v.getIdentName(ident))
	}

	// A production package-level member the WHITE-BOX BRIDGE declares a same-named member for.
	// The bridge is the same GO package, so the reference is bare, and it binds production
	// through `using static <pkg>_package` — but a `using static` import loses to any member of
	// the enclosing class, so the bridge's own declaration HIDES it (container/heap's
	// `[GoRecv] Pop(this ref myHeap)` hid `heap_package.Pop(Interface)`; every `Pop(h)`/`Push(h,
	// x)` in heap_test then bound the extension by value — CS1620 ×8). Qualify through the
	// production class, which nothing declared in the bridge can shadow.
	if obj := v.info.ObjectOf(ident); name == "" && v.whiteboxProductionNameShadowed(obj) {
		name = globalQualifyRooted(packageNamespace+"."+getSanitizedImport(v.options.testProductionName+PackageSuffix)) +
			"." + getSanitizedIdentifier(v.getIdentName(ident))
	}

	// A DOT-IMPORTED (`. "time"`) package member is referenced BARE, so there is no selector for
	// the qualified-name resolver to rewrite — yet the member may be collision-renamed inside its
	// own package, in which case the raw Go name binds nothing (CS0103).
	if name == "" {
		if renamed, isRenamed := v.dotImportedRenamedMember(ident); isRenamed {
			return renamed
		}
	}

	if name == "" {
		name = getSanitizedIdentifier(v.getIdentName(ident))
	}

	// A SAME-PACKAGE generic function referenced as a VALUE — the bare-ident method group
	// `apply(s1, Reverse)` against `Reverse[S ~[]E, E any](s S)` — must spell its type arguments:
	// C# infers a method group's type parameters only from the target delegate's own parameter
	// types, and E is reachable from none of them (CS0411, slices' TestInference). go/types
	// already resolved the instantiation into info.Instances keyed by this ident.
	//
	// This is the QUALIFIED path's twin: convSelectorExpr has emitted the same list for
	// `pkg.Func` since the SortFunc/bytes.Compare fix, and a bare reference to a function in the
	// current package reached no equivalent — one Go reference cannot have two spellings
	// depending on whether the source named it bare or through its package. The callee and
	// explicit-instantiation-base positions are excluded (see
	// IdentContext.suppressGenericTypeArgs); so is every non-generic ident, which has no recorded
	// instance and renders unchanged.
	// A function whose parameters Phase A REF-LOWERED, referenced as a VALUE. The emitted signature
	// takes `ref T` where the Go signature says `*T`, but a func value's delegate type is rendered
	// from the GO signature — every pointer parameter as `ж<T>` — and C# has no method-group
	// conversion that can absorb a `ref` parameter (CS0123). net/http's export_test.go is the shape:
	// `var ExportRefererForURL = refererForURL`, whose second parameter lowered because the
	// production body only reads its fields.
	//
	// The X5-func-value veto exists to stop exactly this, and it holds for every reference the
	// analysis can SEE — but that scan skips `_test.go` by a standing ruling (emission determinism
	// between the `-stdlib` and `-tests` runs), so a test-only func value reaches emission with the
	// callee already lowered. Adapt at the REFERENCE rather than reopening the ruling: wrap in a
	// lambda that re-derives the ref at each lowered position, which is the same
	// `ref (…).DerefOrNull()` the call-site rows emit, and leaves the production emission — and the
	// corpus — untouched. Parameters are typed explicitly so the wrapper is valid in a position with
	// no target-typed delegate, and a variadic signature is declined (its `params` rendering has no
	// delegate to convert to, exactly as the IIFE machinery declines it).
	//
	// This arm shares the generic-value append's guard below, which is what keeps it off a CALLEE:
	// a call's callee suppresses that flag (convCallExpr's calleeIdentContext), and the generic
	// append has depended on precisely that for as long as it has existed.
	if !context.suppressGenericTypeArgs && !context.isMethod && !context.isType {
		if funcObj, isFunc := v.info.Uses[ident].(*types.Func); isFunc {
			if wrapper, wrapped := v.refLoweredFuncValueWrapper(funcObj, name); wrapped {
				return wrapper
			}
		}
	}

	if !context.suppressGenericTypeArgs && !context.isMethod && !context.isType {
		if _, isFunc := v.info.Uses[ident].(*types.Func); isFunc {
			if inst, ok := v.info.Instances[ident]; ok && inst.TypeArgs != nil && inst.TypeArgs.Len() > 0 {
				// Erased (pointer-core) positions leave the emitted list (see renderedTypeArgs);
				// a list that erases to empty falls through to the bare form.
				if typeArgs := v.renderedTypeArgs(ident, inst.TypeArgs); len(typeArgs) > 0 {
					return fmt.Sprintf("%s<%s>", name, strings.Join(typeArgs, ", "))
				}
			}
		}
	}

	return name
}

// dotImportedRenamedMember resolves a BARE reference to a package-level CONST or VAR declared in
// ANOTHER package — only a dot import can produce one — to the name that package's converted form
// actually declares, when a name collision renamed it. A foreign TYPE resolves the same rename
// through foreignAliasedTypeName, which works from go/types rather than from the source spelling;
// the type-DRIVEN positions got that for free, but the two AST-ident type positions (a
// type-assertion target and a composite-literal type) had to be routed there explicitly — see the
// isType arm above. A const/var had no equivalent at all, so time's external test
// files (`. "time"`) emitted `Second`, `UTC`, `Hour`, `Minute`, `Nanosecond` and `Local` raw —
// every one of which time Δ-renames because a `Time` METHOD shares the name (CS0103 ×176 across
// five files). The renamed member is emitted BARE: a dot import renders as `using static
// <pkg>_package`, which exposes it under exactly that name.
//
// The recorded `GoTypeAlias` entries are the single source of truth for the rename, shared with the
// qualified path (getAliasedTypeName), so the two spellings of one Go reference can never disagree.
// Only `const:`-marked entries are honored — a TYPE entry resolves to a `pkgꓸName` global-using
// alias, which is the type layer's business and not a value identifier.
func (v *Visitor) dotImportedRenamedMember(ident *ast.Ident) (string, bool) {
	obj := v.info.ObjectOf(ident)

	if obj == nil {
		return "", false
	}

	switch obj.(type) {
	case *types.Const, *types.Var:
	default:
		return "", false
	}

	pkg := obj.Pkg()

	if pkg == nil || pkg == v.pkg || obj.Parent() != pkg.Scope() {
		return "", false
	}

	plainKey := fmt.Sprintf("%s.%s", getSanitizedIdentifier(pkg.Name()), getCoreSanitizedIdentifier(obj.Name()))

	packageLock.Lock()
	alias, exists := importedTypeAliases[plainKey]
	isConst := constImportedTypeAliases.Contains(plainKey)
	packageLock.Unlock()

	return alias, exists && isConst
}
