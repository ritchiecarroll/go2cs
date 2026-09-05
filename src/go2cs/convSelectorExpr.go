// convSelectorExpr.go - Gbtc
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
	"path/filepath"
	"strings"
)

// convExprInLambdaContext renders expr with conversionInLambda forced on, so a deref-aliased
// pointer receiver/param inside it renders through its capturable box (`Ꮡp.Value`) rather than the
// uncapturable `ref var p` alias. Used when synthesizing a wrapping lambda for a method value whose
// receiver expression is captured (`kdf.hash.New` → `() => Ꮡkdf.Value.hash.New()`). Only the
// conversionInLambda flag is toggled — currentLambdaVars is preserved (not reset as
// enterLambdaConversion would), so an ENCLOSING lambda's captured-var renames still apply when the
// method value nests inside a func literal. Restored to the prior value afterward.
// receiverTempPrefix names the statement-level temp a method value's receiver is evaluated into
// when the receiver is not a bare ident (see hoistReceiverEvaluation). It is a prefix rather than a
// whole name so it shares getCapturedVarName's counter with the capture snapshots, which is what
// keeps `recvʗ1` from ever colliding with a real variable's `xʗ1`.
const receiverTempPrefix = "recv"

func (v *Visitor) convExprInLambdaContext(expr ast.Expr) string {
	if v.lambdaCapture == nil {
		return v.convExpr(expr, nil)
	}

	saved := v.lambdaCapture.conversionInLambda
	v.lambdaCapture.conversionInLambda = true
	result := v.convExpr(expr, nil)
	v.lambdaCapture.conversionInLambda = saved

	return result
}

// typeCollidingFieldName renames a struct field whose C# name would equal its enclosing
// struct's type name (C# forbids it — CS0542) by prefixing the disambiguation marker.
func typeCollidingFieldName(name string) string {
	// A Δ-prefixed name can never be a C# keyword, so a leading '@' keyword-escape is
	// dropped before composing — `Δ@file` is not a valid identifier ('@' only leads a
	// token; net parse.go's `type file struct{ file *os.File }`, CS1003 ×4). A
	// KEYWORD-family name (arrived '@'-escaped) has its enclosing TYPE Δ-renamed too
	// (`partial struct Δfile`, CS9056), so the member doubles the marker (ΔΔfile) to
	// still differ from the type (CS0542). Deterministic from the name alone, keeping
	// every declaration/access site consistent.
	if raw, wasEscaped := strings.CutPrefix(name, "@"); wasEscaped {
		return ShadowVarMarker + ShadowVarMarker + raw
	}

	// When the enclosing TYPE is itself Δ-renamed for a type-vs-method collision — internal/trace's
	// `type Label struct{ Label string }` alongside `func (e Event) Label() Label`, so the TYPE
	// becomes ΔLabel — a single-marker field ΔLabel would EQUAL the type ΔLabel (CS0542 again).
	// Double the marker (ΔΔLabel) to differ, matching the keyword-family branch above. The type is
	// only single-Δ (getCollisionAvoidanceIdentifier's non-reserved branch), so ΔΔ is unambiguous.
	if nameCollisions[name] {
		return ShadowVarMarker + ShadowVarMarker + name
	}

	return ShadowVarMarker + name
}

// fieldCollidesWithType reports whether a field selector's name equals the C# type name of the
// struct it belongs to (`type Node struct{ Node *Node }` → field `Node` in struct `Node`).
func (v *Visitor) fieldCollidesWithType(sel *ast.Ident, x ast.Expr) bool {
	xType := v.info.TypeOf(x)

	if xType == nil {
		return false
	}

	if ptr, ok := xType.Underlying().(*types.Pointer); ok {
		xType = ptr.Elem()
	}

	named, ok := xType.(*types.Named)

	if !ok {
		return false
	}

	// Only a package-level named type keeps its Go name as the C# type name. A function-local
	// (or otherwise lifted) type is emitted under a qualified name (e.g. `Uncommon_u`), so its
	// field does not collide in C# even when the Go field and type names match — and
	// visitStructType only renames the field declaration in the package-level case.
	obj := named.Obj()

	if obj.Pkg() == nil || obj.Parent() != obj.Pkg().Scope() {
		return false
	}

	return getSanitizedIdentifier(sel.Name) == getSanitizedIdentifier(obj.Name())
}

// packageMethodNames caches, per package, the set of every method/func name declared in it. Used to
// detect a type-vs-method collision (a type `T` whose package also declares a func/method named `T`,
// which Δ-renames the TYPE) for a FOREIGN type, where the current package's `nameCollisions` map does
// not apply. Package objects are interned per run, so the cache key is stable.
var packageMethodNames map[*types.Package]map[string]bool

func packageHasMethodNamed(pkg *types.Package, name string) bool {
	if pkg == nil {
		return false
	}

	packageLock.Lock()
	defer packageLock.Unlock()

	if packageMethodNames == nil {
		packageMethodNames = map[*types.Package]map[string]bool{}
	}

	set, ok := packageMethodNames[pkg]

	if !ok {
		set = map[string]bool{}
		scope := pkg.Scope()

		for _, n := range scope.Names() {
			switch o := scope.Lookup(n).(type) {
			case *types.Func:
				set[o.Name()] = true
			case *types.TypeName:
				if named, isNamed := o.Type().(*types.Named); isNamed {
					for i := range named.NumMethods() {
						set[named.Method(i).Name()] = true
					}
				}
			}
		}

		packageMethodNames[pkg] = set
	}

	return set[name]
}

// fieldTypeIsRenamed reports whether a field selector's enclosing named type is itself Δ-renamed for a
// type-vs-method collision IN ITS OWN package. A FOREIGN such type — internal/trace's `Label`, renamed
// `ΔLabel` because `func (Event) Label()` shares the name — is not in the CURRENT package's
// nameCollisions map, so a cross-package field access (`l.Label`) would emit the SINGLE-marker field
// name (`ΔLabel`) instead of the DOUBLE the declaration used (`ΔΔLabel`) — CS1061 (internal/trace/
// testtrace). The double is applied at the access site by consulting the field-type's OWN package.
func (v *Visitor) fieldTypeIsRenamed(x ast.Expr) bool {
	xType := v.info.TypeOf(x)

	if xType == nil {
		return false
	}

	if ptr, ok := xType.Underlying().(*types.Pointer); ok {
		xType = ptr.Elem()
	}

	named, ok := xType.(*types.Named)

	if !ok {
		return false
	}

	obj := named.Obj()

	if obj.Pkg() == nil || obj.Parent() != obj.Pkg().Scope() {
		return false
	}

	return packageHasMethodNamed(obj.Pkg(), obj.Name())
}

// selectorTargetIsDirectBoxMethod reports whether the method a selector names is emitted with the
// BOX as its receiver (`this ж<T>`) rather than as a `[GoRecv] ref` extension. Such a target binds
// only a real `ж<T>`, so a caller that would otherwise prefer a plain addressable member chain must
// keep the box form. Origin-keyed, matching packageDirectBoxReceiverMethods' interning.
func (v *Visitor) selectorTargetIsDirectBoxMethod(selectorExpr *ast.SelectorExpr) bool {
	funcObj, isFunc := v.info.ObjectOf(selectorExpr.Sel).(*types.Func)

	return isFunc && packageDirectBoxReceiverMethods[funcObj.Origin()]
}

// structFieldBoxName returns the member name for a struct field's box accessor (`Type.Ꮡ<member>`),
// matching the field's DECLARED C# name (visitStructType uses getCoreSanitizedIdentifier plus the
// type-colliding rename) and the TypeGenerator's `Ꮡ<member>` static. It deliberately does NOT apply
// the package-level nameCollisions rename (the type-vs-method `Δ` prefix) that convExpr/convIdent
// would: a struct field is struct-scoped, so a field named like a package type/method (`trace`,
// `stack`, `p`) is declared unrenamed (`trace`) — emitting `ᏑΔtrace` here would not match the
// generated `Ꮡtrace` static (CS0117). The leading '@' keyword-escape is stripped (`Ꮡ@base` is
// invalid — '@' only leads; the generator strips it the same way via GetUnsanitizedIdentifier).
func (v *Visitor) structFieldBoxName(sel *ast.Ident, structExpr ast.Expr) string {
	name := getCoreSanitizedIdentifier(sel.Name)

	if v.fieldCollidesWithType(sel, structExpr) {
		name = typeCollidingFieldName(name)
	}

	return removeLeadingSanitizationMarker(name)
}

// structFieldReachable reports whether a field named `name` is reachable on the struct — either
// as a direct field or promoted through an embedded (anonymous) field, including an embedded
// pointer. The deref decision for a pointer's field selector must consider promoted fields too:
// otherwise `x.PromotedField` on a `ж<T>` box is emitted without a deref, and the box has no such
// member (CS1061). Go forbids embedding cycles, so the recursion terminates.
func structFieldReachable(structType *types.Struct, name string) bool {
	for i := range structType.NumFields() {
		field := structType.Field(i)

		if field.Name() == name {
			return true
		}

		if !field.Embedded() {
			continue
		}

		embType := field.Type()

		if ptr, ok := embType.Underlying().(*types.Pointer); ok {
			embType = ptr.Elem()
		}

		if embStruct, ok := embType.Underlying().(*types.Struct); ok {
			if structFieldReachable(embStruct, name) {
				return true
			}
		}
	}

	return false
}

// exprIsPointerLocalField reports whether expr is a field selector `base.field` whose base is a
// pointer LOCAL (a `*T` variable that is neither a parameter nor the receiver). Such a local holds
// the heap box `ж<T>` directly, so `base.field` reached through the value-returning `~` deref is an
// rvalue; the field's address `&base.field` must instead go through the box accessor
// `base.of(T.Ꮡfield)`. A pointer parameter and the receiver are deref-aliased to a value
// (`ref var p = ref Ꮡp.Value`), so their fields are already assignable — those are excluded (handled
// by exprIsDerefdPointerParam / the receiver paths).
func (v *Visitor) exprIsPointerLocalField(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	baseIdent, ok := sel.X.(*ast.Ident)

	if !ok {
		return false
	}

	baseType := v.info.TypeOf(baseIdent)

	if baseType == nil {
		return false
	}

	if _, isPtr := baseType.Underlying().(*types.Pointer); !isPtr {
		return false
	}

	if v.identIsParameter(baseIdent) {
		return false
	}

	if obj := v.info.ObjectOf(baseIdent); obj != nil && v.currentFuncSignature != nil && v.currentFuncSignature.Recv() == obj {
		return false
	}

	return true
}

// exprFieldRootsAtAddressedGlobal reports whether expr is a VALUE field selector (`global.field`,
// possibly nested through further value fields) that roots at a package value global whose address
// is taken (heap-boxed). Such a field's real address is `Ꮡglobal.of(T.Ꮡfield)`; a ж-only method
// (e.g. an atomic `func (x *Uint32) Store`) called on it must route through that box, since a plain
// value/ref of the field cannot bind the box receiver (CS1929). The walk bails at any pointer hop —
// beyond a pointer the field already has a real address, and those forms (pointer locals/params,
// the deref'd receiver) are handled by the boxed/local-field branches above and must not be
// disturbed (routing a ref-accessible receiver field through `&` would need a `Ꮡrecv` box that a
// non-direct-ж receiver lacks → CS0103). A pointer FIELD carries its own box and is excluded.
func (v *Visitor) exprFieldRootsAtAddressedGlobal(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	selection, ok := v.info.Selections[sel]

	if !ok || selection.Kind() != types.FieldVal {
		return false
	}

	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	base := sel.X

	for {
		if t := v.info.TypeOf(base); t != nil {
			if _, isPtr := t.Underlying().(*types.Pointer); isPtr {
				return false
			}
		}

		if s, ok := base.(*ast.SelectorExpr); ok {
			base = s.X
			continue
		}

		break
	}

	ident, ok := base.(*ast.Ident)

	return ok && v.isAddressedGlobal(ident)
}

// exprIsValueFieldOfPointerRvalue reports whether expr is a VALUE (non-pointer) struct field whose
// selector chain, after peeling value-field selectors, roots at a pointer-to-struct RVALUE expression
// that already yields a box `ж<T>` directly — a pointer-returning CALL (`getg()`, `q.tail.ptr()`,
// `Δp.chunkOf(ci)`, `getg().m.p.ptr()`) or a pointer ELEMENT index (`batch[i]`). The Go auto-deref
// renders the field access as `(~root).field`, an rvalue, so a `[GoRecv] ref` (pointer-receiver)
// method called on it cannot bind (CS1510). Unlike a deref-aliased ident param/receiver (which has a
// `ref`), the root call/index value IS the box, so the receiver is materialized through the
// &-machinery as `root.of(T.Ꮡfield)` — never a `Ꮡ(value)` copy (which would lose the write).
//
// This is the rvalue COMPLEMENT of exprIsValueFieldOfPointer: that one roots at a pointer FIELD
// selector or pointer LOCAL ident; this one roots at a NON-ident, NON-selector pointer expression (a
// call/index). The two domains are disjoint, so the routing branches never overlap.
func (v *Visitor) exprIsValueFieldOfPointerRvalue(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	if selection, ok := v.info.Selections[sel]; !ok || selection.Kind() != types.FieldVal {
		return false
	}

	// The field itself must be a VALUE — a pointer field is already a box.
	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	base := sel.X

	for {
		switch b := base.(type) {
		case *ast.SelectorExpr:
			// A pointer field/chain mid-way is exprIsValueFieldOfPointer's territory, not this.
			if _, ok := v.getType(b, false).(*types.Pointer); ok {
				return false
			}

			// Keep peeling only through VALUE-field selectors.
			if selection, ok := v.info.Selections[b]; !ok || selection.Kind() != types.FieldVal {
				return false
			}

			base = b.X
		case *ast.Ident:
			// An ident root (pointer local/param/receiver) is handled by the sibling predicates — not
			// this rvalue case.
			return false
		default:
			// A type CONVERSION (`(*T)(p)`) renders as a C# CAST — a low-precedence form on which a
			// trailing `.of(…)` mis-binds to the inner operand. Exclude it so a pointer reinterpret
			// (`(*structTypeUncommon)(unsafe.Pointer(t))`) keeps its existing `Ꮡ(…)` form (S1 territory).
			if call, ok := b.(*ast.CallExpr); ok && v.callExprIsTypeConversion(call) {
				return false
			}

			// A genuine CALL / INDEX / other rvalue: route only when it is a pointer-to-struct box (the
			// value it yields IS a `ж<T>`, so `root.of(T.Ꮡfield)` field-refs the real storage).
			ptrType, ok := v.getType(b, false).(*types.Pointer)

			if !ok {
				return false
			}

			_, ok = ptrType.Elem().Underlying().(*types.Struct)

			return ok
		}
	}
}

// callExprIsTypeConversion reports whether a CallExpr is a Go type CONVERSION (`T(x)`, `(*T)(p)`) —
// its Fun denotes a TYPE — rather than a genuine function/method call. A conversion renders as a C#
// cast (`(T)x`), a low-precedence form on which a trailing `.of(…)` would mis-bind to the inner
// operand; a genuine call renders as a postfix `f(…)` that `.of(…)` chains off cleanly. The
// pointer-rvalue field-receiver routing excludes conversions for this reason.
func (v *Visitor) callExprIsTypeConversion(callExpr *ast.CallExpr) bool {
	tv, ok := v.info.Types[callExpr.Fun]
	return ok && tv.IsType()
}

// exprIsValueFieldOfPointer reports whether expr is a VALUE (non-pointer) struct field whose base is
// a pointer-to-struct *field selector* (`o.h.wait`, `gp.m.mLockProfile`) — a pointer reached by
// dereferencing another field. Such a pointer deref is an rvalue (`(~o.h)`), so the value field on it
// is NOT addressable, and a pointer-receiver method called on it cannot bind ([GoRecv] ref / ж
// overload, CS1510 / CS1929). Taking the field's address goes through the box-field accessor
// (`o.h.of(holder.Ꮡwait)`, real storage), which the &-machinery renders. The base is intentionally
// restricted to a SELECTOR: a bare ident base is the method's RECEIVER or a deref'd pointer PARAMETER
// (both emitted as an addressable `ref`, so `f.c.Get()` binds directly — routing them through `&`
// would emit `Ꮡf.of(…)`, but a value-ref receiver has no `Ꮡf` box → regression, the historical
// ReceiverFieldMethodCall failure) or a pointer LOCAL (handled by exprIsPointerLocalField above). A
// pointer FIELD is excluded (it is already a box — exprIsAlreadyBoxedPointerFieldOrElement).
func (v *Visitor) exprIsValueFieldOfPointer(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	if selection, ok := v.info.Selections[sel]; !ok || selection.Kind() != types.FieldVal {
		return false
	}

	// The field itself must be a VALUE — a pointer field is already a box.
	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	// Walk the base, peeling value-field selectors, until reaching the pointer the chain is rooted
	// at — `o.h.wait` (base `o.h` is the pointer field) or `mp.mLockProfile.waitTime` (peel
	// `mp.mLockProfile` to `mp`, a `*m` pointer local). The root may be a pointer-to-struct SELECTOR
	// (always an rvalue deref) or a pointer-to-struct LOCAL identifier (the box is accessed via `~`,
	// also an rvalue). It must NOT be the method's RECEIVER or a deref'd pointer PARAMETER: those are
	// emitted as an addressable `ref`, so `f.c.Get()` binds directly and routing them through `&`
	// would emit `Ꮡf.of(…)` with no `Ꮡf` box (the historical ReceiverFieldMethodCall regression).
	base := sel.X

	for {
		switch b := base.(type) {
		case *ast.SelectorExpr:
			if ptrType, ok := v.getType(b, false).(*types.Pointer); ok {
				_, ok := ptrType.Elem().Underlying().(*types.Struct)
				return ok
			}

			// A value-field selector — peel to its own base and keep walking toward the root.
			base = b.X
		case *ast.Ident:
			// Root identifier: route a pointer-to-struct LOCAL only (its box is dereferenced via `~`,
			// an rvalue). A deref'd pointer parameter and the receiver are addressable refs.
			ptrType, ok := v.getType(b, false).(*types.Pointer)

			if !ok {
				return false
			}

			if _, ok := ptrType.Elem().Underlying().(*types.Struct); !ok {
				return false
			}

			if v.identIsParameter(b) {
				return false
			}

			// Object identity, not name: a pointer-to-struct LOCAL shadowing the receiver name
			// must still be routed here (identResolvesToReceiver).
			if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(b, recvName) {
				return false
			}

			return true
		default:
			return false
		}
	}
}

// exprIsIndexableElement reports whether indexExpr indexes storage whose ELEMENT the &-machinery can
// alias — a slice, an array, or a pointer-to-array (Go's auto-deref). For those three the index
// branch of convUnaryExpr renders an element-ALIASING address (`Ꮡ(s, i)` / `Ꮡ(a, i)` / `p.at<E>(i)`)
// that shares the backing storage, so `&s[i].field` can be built as that address plus a field ref
// rather than as `Ꮡ(s[i])` — a box over a COPY of the element, through which every write is lost.
//
// A MAP is deliberately excluded: Go does not permit `&m[k]` at all (a map element is not
// addressable), so an index over one can never legitimately reach the address-of machinery, and
// admitting it would only mask a front-end error as a plausible emission. A STRING is excluded for
// the same reason (`&s[i]` is illegal) — and neither has a field to select in any case. A GENERIC
// instantiation shares *ast.IndexExpr's shape but is not an index at all; it types as a signature or
// a named type rather than a slice/array/pointer-to-array, so it falls out here without a special
// case.
func (v *Visitor) exprIsIndexableElement(indexExpr *ast.IndexExpr) bool {
	baseType := v.getType(indexExpr.X, true)

	if baseType == nil {
		return false
	}

	switch t := baseType.Underlying().(type) {
	case *types.Slice, *types.Array:
		return true
	case *types.Pointer:
		// `&t[i].field` where t is `*[N]E`: Go auto-derefs the index, and the element lives in the
		// pointed-to array, which `.at<E>(i)` aliases through the box.
		_, isArray := t.Elem().Underlying().(*types.Array)
		return isArray
	}

	return false
}

// exprIsValueFieldOfDerefdPointerRoot reports whether expr is a VALUE struct field whose selector
// chain, after peeling value-field selectors, roots at a deref-aliased pointer PARAMETER or the
// pointer RECEIVER — a bare ident emitted as `ref var x = ref Ꮡx.Value`, whose box is `Ꮡx`. Examples:
// `Δp.scav.index` (root `p`, a `*pageAlloc` receiver), `mp.trace.seqlock` (root `mp`, a `*m` param),
// `h.userArena.readyList` (root `h`, a `*mheap` param).
//
// This is the deliberate COMPLEMENT of exprIsValueFieldOfPointer, which roots at a pointer FIELD/chain
// or a pointer LOCAL and EXCLUDES the param/receiver root: a value field-chain on such a root is
// addressable, so a `[GoRecv] ref` method binds on it directly and must be left alone. A DIRECT-ж
// (box-receiver) method, however, needs the real nested field box `Ꮡx.of(T.Ꮡf1).of(…Ꮡf2)` (which the
// &-machinery renders once it recurses through this root) — the value chain is not a box (CS1929).
// Callers MUST therefore gate on a direct-ж method (selectorCallsDirectBoxMethod), so a `[GoRecv]` ref
// method on the same chain keeps binding directly (no churn).
func (v *Visitor) exprIsValueFieldOfDerefdPointerRoot(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	if selection, ok := v.info.Selections[sel]; !ok || selection.Kind() != types.FieldVal {
		return false
	}

	// The field itself must be a VALUE — a pointer field is already a box.
	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	base := sel.X

	for {
		switch b := base.(type) {
		case *ast.SelectorExpr:
			// A pointer field/chain mid-way is exprIsValueFieldOfPointer's territory, not this.
			if _, ok := v.getType(b, false).(*types.Pointer); ok {
				return false
			}

			// Keep peeling only through VALUE-field selectors.
			if selection, ok := v.info.Selections[b]; !ok || selection.Kind() != types.FieldVal {
				return false
			}

			base = b.X
		case *ast.Ident:
			// Root identifier: a deref-aliased pointer PARAMETER or the pointer RECEIVER (box `Ꮡx`).
			// A pointer LOCAL is excluded — it is handled by exprIsValueFieldOfPointer / the
			// exprIsPointerLocalField branch.
			if _, ok := v.getType(b, false).(*types.Pointer); !ok {
				return false
			}

			// The pointer RECEIVER has a box `Ꮡrecv` ONLY when the enclosing method is itself direct-ж
			// (`this ж<T> Ꮡrecv`). A `[GoRecv] ref` receiver has no box, so routing through `Ꮡrecv`
			// would be CS0103 — leave it (that would need transitive direct-ж propagation, a separate
			// capture-mode concern). Checked before identIsParameter since the receiver is not a param.
			// Object identity, not name: a pointer LOCAL shadowing the receiver name is not the
			// receiver and keeps its own routing (identResolvesToReceiver).
			if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(b, recvName) {
				return isDirectBoxReceiverMethod(v.currentFuncDecl, v.info)
			}

			// A genuine pointer PARAMETER is always deref-aliased with a box `Ꮡp`.
			if v.identIsParameter(b) {
				return true
			}

			return false
		default:
			return false
		}
	}
}

// exprIsAlreadyBoxedPointerFieldOrElement reports whether expr is a field selector or an indexed
// element whose OWN type is a Go pointer — so its C# value is already a `ж<T>` box (e.g.
// cpuProfile's `log *profBuf`, accessed as `cpuprof.log`). A direct-ж (capture-mode) or
// pointer-receiver method called on it binds to that box directly; taking its address via the
// &-machinery would double-box to `ж<ж<T>>` (CS1929). A VALUE field/element (an atomic `Int32`, a
// plain struct field) is NOT already a box and still needs the box machinery. This discriminates a
// pointer FIELD of a boxed global (`cpuprof.log`, already a box) from a deref'd pointer PARAMETER
// (`s` in `s.Prev()`, a value alias whose box is `Ꮡs`) — the latter is a bare ident, not a
// selector/index, so it is correctly left to the box routing.
func (v *Visitor) exprIsAlreadyBoxedPointerFieldOrElement(expr ast.Expr) bool {
	switch expr.(type) {
	case *ast.SelectorExpr, *ast.IndexExpr:
		_, isPtr := v.info.TypeOf(expr).(*types.Pointer)
		return isPtr
	}

	return false
}

// exprIsIndexedValueElement reports whether expr is an indexed element `container[i]` of an
// ADDRESSABLE container (an array or slice — NOT a map, whose elements are not addressable) whose
// element type is a VALUE (not already a pointer/box). A pointer-receiver / direct-ж method called
// on such an element — `bh.Value[i].Load()` (an array of atomic `UnsafePointer`) — operates on the
// element VALUE, so the `[GoRecv] ref` / `ж` overload cannot bind (CS1510 / CS1929). The receiver
// must be routed through the element's box via the &-machinery (`Ꮡ(slice, i)` / `…at<T>(i)`).
func (v *Visitor) exprIsIndexedValueElement(expr ast.Expr) bool {
	indexExpr, ok := expr.(*ast.IndexExpr)

	if !ok {
		return false
	}

	// A generic instantiation `Type[Arg]` is also an *ast.IndexExpr; require the indexed operand to
	// be an array/slice VALUE so a type-instantiation (or a map index) is excluded.
	containerType := v.getType(indexExpr.X, true)

	if containerType == nil {
		return false
	}

	var elem types.Type

	switch container := containerType.Underlying().(type) {
	case *types.Array:
		elem = container.Elem()
	case *types.Slice:
		elem = container.Elem()
	default:
		return false
	}

	// A pointer element is already a box (exprIsAlreadyBoxedPointerFieldOrElement); only a value
	// element needs the box machinery.
	_, isPtr := elem.Underlying().(*types.Pointer)

	return !isPtr
}

// selectorCallsDirectBoxMethod reports whether the selector calls a DIRECT-ж (box-receiver) method —
// one emitted as `this ж<T>` (it takes the address of a field of its receiver, or otherwise needs
// the box), rather than `[GoRecv] this ref T`. Only a direct-ж method requires its receiver be a
// box; a `[GoRecv] ref` method binds to any addressable value directly. Used to decide whether an
// indexed value element must be routed through its box for the call (an addressable element already
// satisfies a `[GoRecv] ref` method, so routing it would be needless churn).
func (v *Visitor) selectorCallsDirectBoxMethod(selectorExpr *ast.SelectorExpr) bool {
	funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func)

	return ok && funcObj != nil && packageDirectBoxReceiverMethods[funcObj.Origin()]
}

// isPointerReceiverMethodCall reports whether the selector calls a method with a POINTER receiver
// (`func (x *T) M()`), emitted as a `[GoRecv]` extension over `ref T` / a `ж<T>` overload. Such a
// method needs an addressable receiver, so a value-returning `~` deref of a field receiver is an
// rvalue (CS1510 on the generated `ref`).
func (v *Visitor) isPointerReceiverMethodCall(selectorExpr *ast.SelectorExpr) bool {
	sel, ok := v.info.Selections[selectorExpr]

	if !ok || sel.Kind() != types.MethodVal {
		return false
	}

	sig, ok := sel.Obj().Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return false
	}

	_, isPtr := sig.Recv().Type().(*types.Pointer)

	return isPtr
}

// aliasResolvedSelector applies the imported-type-alias resolution (getAliasedTypeName) to a
// rendered `base.member` — but ONLY when the selector's base actually denotes a PACKAGE.
//
// getAliasedTypeName is a QUALIFIED-NAME resolver: it reads its argument as `<package>.<member>`
// and rewrites either half (a collision-renamed foreign member `time.Second` → `time.ΔSecond`, a
// Δ-shadowed import qualifier `color.RGBA` → `Δcolor.RGBA`, a type alias `color.RGBA` →
// `colorꓸRGBA`). Every convSelectorExpr emission used to be passed through it unconditionally,
// which made those rewrites fire on a rendered EXPRESSION whose base merely SHARED A NAME with an
// imported package — a local variable, parameter or field. Go allows exactly that, and the stdlib's
// own test code does it: `format_test.go`'s `func checkTime(time Time, …)` and `TestFormat`'s
// `time := Unix(…)` shadow the `time` import inside a package that also dot-imports it. Every
// method call on that value was then rewritten as a package reference — `time.Year()` → `Δtime.Year()`
// (the import alias), `time.Hour()` → `time.ΔHour()` (the const rename applied to the METHOD name),
// `time.Month()` → `timeꓸMonth()` (the type alias) — 33 errors of five distinct codes, all from one
// name-based rewrite applied where the AST already knows the answer.
//
// The base is asked through go/types, never by name, so a shadowing binding is excluded by
// construction. Non-package bases (a field/box chain, an indexed element, a call result) can never
// resolve through the alias maps anyway — those are keyed `<package>.<member>` — so gating here is
// the property stated once rather than a special case per emission site.
func (v *Visitor) aliasResolvedSelector(selectorExpr *ast.SelectorExpr, rendered string) string {
	pkgName := v.selectorBasePackageObj(selectorExpr)

	if pkgName == nil {
		return rendered
	}

	if !aliasSourceMatchesPackage(rendered, pkgName.Imported()) {
		return rendered
	}

	return getAliasedTypeName(rendered)
}

// aliasSourceMatchesPackage reports whether a published entry for the raw "pkg.Member" key
// (importedTypeAliasSourceDirs — see applyExportedTypeAliases), if one exists, actually came from
// the package THIS reference resolves to. go2cs's alias table is keyed by short DECLARED package
// name, not import path, so two different import paths that declare the same package name collide
// on one key — runtime/trace and internal/trace are both literally `package trace`. Without this
// check, a reference to the package that published NOTHING under the key silently adopted the
// OTHER package's entry (CS1955: a bare `trace.Log` resolving through go's own type-checker to
// runtime/trace picked up internal/trace's unrelated `Log` → `ΔLog` rename instead).
//
// No published entry at all is NOT a mismatch — most keys are published by exactly the package
// being referenced, and a package that publishes no aliases (needs no rename) must fall through to
// its own unrenamed spelling exactly as if this check did not exist.
func aliasSourceMatchesPackage(rendered string, pkg *types.Package) bool {
	if pkg == nil {
		return true
	}

	packageLock.Lock()
	sourceDir, published := importedTypeAliasSourceDirs[rendered]
	packageLock.Unlock()

	if !published {
		return true
	}

	dir, ok := importPackageDirs[pkg.Path()]

	if !ok || dir.Dir == "" {
		return true
	}

	return filepath.Clean(dir.Dir) == sourceDir
}

// selectorBaseIsPackage reports whether the selector qualifies a PACKAGE (`time.Second`) rather
// than a value/type expression. Parentheses are peeled; anything that is not a bare identifier
// bound to a *types.PkgName is not a package qualifier.
func (v *Visitor) selectorBaseIsPackage(selectorExpr *ast.SelectorExpr) bool {
	return v.selectorBasePackageObj(selectorExpr) != nil
}

// selectorBasePackageObj extracts the *types.PkgName the selector's base identifier is bound to,
// peeling parens first, or nil if the base is not a bare package-qualifying identifier.
func (v *Visitor) selectorBasePackageObj(selectorExpr *ast.SelectorExpr) *types.PkgName {
	base := selectorExpr.X

	for {
		paren, isParen := base.(*ast.ParenExpr)

		if !isParen {
			break
		}

		base = paren.X
	}

	ident, isIdent := base.(*ast.Ident)

	if !isIdent {
		return nil
	}

	pkgName, _ := v.info.ObjectOf(ident).(*types.PkgName)

	return pkgName
}

func (v *Visitor) convSelectorExpr(selectorExpr *ast.SelectorExpr, context LambdaContext) string {
	if base, ok := selectorExpr.X.(*ast.Ident); ok {
		if _, isPackage := v.info.ObjectOf(base).(*types.PkgName); isPackage && v.whiteboxBridgeUse(selectorExpr.Sel) {
			return v.whiteboxBridgeMember(selectorExpr.Sel)
		}
	}

	// A Go method becomes a C# extension method on the receiver box (`Method(this ж<T>, …)`) emitted in
	// its DEFINING package's class. C# only finds an extension method when that class's NAMESPACE is in
	// scope. For a method whose receiver type lives in a sub-namespace package (e.g. `internal/runtime/
	// atomic` → `go.@internal.runtime`), a file that calls the method but does NOT import the package
	// (legal in Go — calling a method on a value never requires importing the value's package) gets no
	// `using @internal.runtime;`, so the extension method is invisible and the call mis-binds to a wrong
	// promoted overload (CS1929). Register the method's package namespace here so the file-local `using`
	// is emitted regardless of whether the package was explicitly imported.
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodVal {
		if obj := sel.Obj(); obj != nil {
			v.addMethodPackageNamespaceUsing(obj.Pkg())
		}
	}

	// A Go METHOD EXPRESSION — `(*timers).run`, the unbound method as a func value whose first
	// parameter is the receiver (runtime time.go's `abi.FuncPCABIInternal((*timers).run)`) —
	// selects a method off a TYPE. Emitting the selector naively renders the type in value
	// position (`(ж<timers>).run` — CS0119/CS1503). Go types the expression as the func signature
	// with the receiver prepended; render that signature as the concrete delegate type and cast
	// the method's static form to it: `(Func<ж<timers>, int64, int64>)run` — for a `[GoRecv]`
	// method the RecvGenerator's ж-overload matches the delegate exactly, and a direct-ж method's
	// primary form does. FuncPCABIInternal-style `any` parameters then take a real delegate. All
	// three return sites below wrap the cast in an OUTER paren pair: a cast-expression is a
	// unary-expression, not a postfix-expression, so a directly-invoked method expression
	// (`I.M(nil)`) parses `(T)(x)(args)` as `(T)(x(args))` — invoking the uncast x first, CS0149 —
	// unless the whole cast is `((T)(x))(args)`.
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodExpr {
		delegateType := convertToCSTypeName(v.getCSharpTypeName(v.info.TypeOf(selectorExpr)))
		methodName := v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))

		// An INTERFACE method expression (`I.M`, runtime stack_test.go's `I.M(nil)` /
		// `reflect.ValueOf(I.M)`) has no static/extension form to reference at all — unlike a
		// concrete type, Go's interface methods become genuine C# instance methods on the
		// interface, callable only as `recv.M(...)`, never as a bare or qualified method group
		// (bare `M` is CS0246/CS0123: no such symbol exists outside instance-call position).
		// Render the dispatch directly as a lambda: `(Action<I>)((p0) => p0.M())`. This bypasses
		// the qualifier logic below entirely: package qualification belongs to delegateType's own
		// (already-correct) rendering of I, never to the instance-called method name.
		if _, isInterface := types.Unalias(sel.Recv()).Underlying().(*types.Interface); isInterface {
			if fn, ok := sel.Obj().(*types.Func); ok {
				if sig, ok := fn.Type().(*types.Signature); ok {
					params := []string{"p0"}
					args := make([]string, 0, sig.Params().Len())

					for i := 0; i < sig.Params().Len(); i++ {
						name := fmt.Sprintf("p%d", i+1)
						params = append(params, name)
						args = append(args, name)
					}

					return fmt.Sprintf("((%s)((%s) => p0.%s(%s)))", delegateType, strings.Join(params, ", "), methodName, strings.Join(args, ", "))
				}
			}
		}

		// A FOREIGN-package type's method expression — `(*http.Request).Write` (net/http/httputil
		// persist.go), or `(*Reader).ReadBytes` in an EXTERNAL test (`package bufio_test`) that
		// dot-imports `bufio` — must QUALIFY the method's static form: the [GoRecv]/extension static
		// (and its RecvGenerator ж-overload) lives in the DEFINING package's class, so the bare name
		// is CS0103 (a `using static` exposes an extension method only for `recv.M()` invocation, not
		// as a bare method group; a same-named local method would instead mis-bind, CS0123). Derive
		// the qualifier from the method's OWN package via go/types — identical to how getAliasQualifiedTypeName
		// qualifies the receiver type inside the delegate (`bufio.Reader`) — rather than peeling the
		// Go source spelling: a dot-imported type is a BARE ident (`Reader`), not a `pkg.Type`
		// selector, so the source-peel misses it and drops the qualifier.
		if obj := sel.Obj(); obj != nil && obj.Pkg() != nil && (obj.Pkg() != v.pkg || v.whiteboxProductionObject(obj)) {
			pkg := obj.Pkg()
			aliasQualifier := importQualifier(pkg.Name())

			if v.whiteboxProductionObject(obj) {
				aliasQualifier = "global::" + packageNamespace + "." + getSanitizedImport(v.options.testProductionName+PackageSuffix)
			} else if v.whiteboxBridgeObject(obj) {
				// A method VALUE bound to a bridge-declared method lives in the bridge class,
				// not in the production package the object's Pkg() reports.
				aliasQualifier = "global::" + packageNamespace + "." + v.options.testInternalBridgeName
			} else if fileAlias, ok := v.importPathAliases[pkg.Path()]; ok && fileAlias != "" {
				aliasQualifier = fileAlias
			}

			methodName = aliasQualifier + "." + methodName
		}

		// A method expression on the POINTER type (`(*T).Method`) whose underlying declaration
		// has a Go VALUE receiver — legal, since a value-receiver method is in *T's method set
		// too (runtime stack_test.go's `(*structWithMethod).caller`). Neither of the two forms
		// the comment above relies on applies: it isn't `[GoRecv]` (that's the POINTER-receiver
		// case) and it isn't direct-ж, so its only C# form is the plain-value primary (`this T`),
		// which cannot bind the delegate's `ж<T>` first parameter (CS1503/CS0123). Wrap it in a
		// lambda that dereferences the box and forwards: `(Func<ж<T>, R>)(p0 => Method(p0.Value))`.
		if fn, ok := sel.Obj().(*types.Func); ok {
			if sig, ok := fn.Type().(*types.Signature); ok && sig.Recv() != nil {
				_, exprRecvIsPointer := types.Unalias(sel.Recv()).(*types.Pointer)
				_, declRecvIsPointer := types.Unalias(sig.Recv().Type()).(*types.Pointer)

				if exprRecvIsPointer && !declRecvIsPointer && !packageDirectBoxReceiverMethods[fn.Origin()] {
					params := []string{"p0"}
					args := []string{"p0.Value"}

					for i := 0; i < sig.Params().Len(); i++ {
						name := fmt.Sprintf("p%d", i+1)
						params = append(params, name)
						args = append(args, name)
					}

					return fmt.Sprintf("((%s)((%s) => %s(%s)))", delegateType, strings.Join(params, ", "), methodName, strings.Join(args, ", "))
				}
			}
		}

		return fmt.Sprintf("((%s)(%s))", delegateType, methodName)
	}

	// A method call on a manually-converted foreign-receiver method (`gp.guintptr()` on a *g —
	// see manualTypeOperations.go): the manual implementation captures the receiver's IDENTITY,
	// so it takes the receiver BOX (`this ж<g>`). A deref-aliased pointer receiver (`ref var gp
	// = ref Ꮡgp.Value`) renders as the value alias, which binds neither the box form nor identity;
	// emit the box itself — `Ꮡgp.guintptr()`.
	//
	// The box must actually EXIST, which is why the gate is exprHasReceiverBoxInScope and not the
	// broader exprIsDerefAliasedPointer it reads like. A registration displaces a BODY; it does not
	// decide the declaration's receiver FORM, and a hand-own of a `[GoRecv] this ref T` method is
	// called exactly as the converted one was. Gating on "is deref-aliased" spelled `Ꮡrecv` inside
	// `[GoRecv] this ref T` bodies, where nothing declares it — reflect's `addArg` emitted
	// `Ꮡa.regAssign(Ꮡt, 0)` (CS0103) the moment `abiSeq.regAssign` was registered. Where the box is
	// genuinely in scope both forms bind (RecvGenerator mints the ж twin beside a `ref` primary),
	// so keeping the box route there is what makes this narrowing corpus-inert.
	//
	// DIRECT selections only (len(sel.Index()) == 1). A registered method reached THROUGH an
	// embedded field — `a.add(r)` on `AddrRanges{addrRanges}` with `addrRanges.add` displaced — is a
	// PROMOTED selection whose receiver is the EMBED, not `selectorExpr.X`: spelling `Ꮡa.add(…)`
	// names the embedding type's box, which the hand-own's receiver (`ж<addrRanges>` or
	// `ref addrRanges`) cannot bind, and the promoted forwarder the generators would otherwise mint
	// is refused whenever the name collides with a package-level function (runtime's `add`) — a
	// CS1929 in the `-tests` host that no production build reaches (measured 2026-09-05, runtime
	// increment 7). The hop machinery below already answers the promoted shape from the Go body
	// (the box hop `Ꮡa.of(AddrRanges.ᏑaddrRanges).add(…)` for a direct-ж callee), displaced or not,
	// so a promoted selection falls through to it exactly as an unregistered method's does.
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodVal && len(sel.Index()) == 1 {
		if obj := sel.Obj(); obj != nil && v.isManualBoxReceiverMethod(obj) && v.exprHasReceiverBoxInScope(selectorExpr.X) {
			if ident, ok := selectorExpr.X.(*ast.Ident); ok {
				return fmt.Sprintf("%s%s.%s", AddressPrefix, v.getIdentName(ident), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
			}
		}

		// B′-S0 arm (a), OQ-7's row at the caller: a RESULT-USED call of a ref-return primary
		// (`return v.carryPropagateGeneric()` in a ж-form sibling) must bind the TWIN — the
		// primary returns `ref T`, which cannot satisfy the ж-typed consumer, and the twin
		// returns its own box, which IS Go's value here. The box must actually EXIST in scope,
		// so this gates on exprHasReceiverBoxInScope — the sub-agent's narrowing of the sibling
		// arm above, for the same reason (a `[GoRecv] this ref T` body declares no `Ꮡv`). B′-S1:
		// inside a ref-return-primary body the cascade promotes, there is no box, so the arm
		// falls through and the call binds the PRIMARY on the bare `ref` receiver — which
		// visitReturnStmt's forwarding-return prefix then returns as `return ref v.M(…)`. A
		// DISCARDED call keeps the plain alias and binds the primary — the mint-free direct form.
		if obj := sel.Obj(); obj != nil {
			if fn, isFunc := obj.(*types.Func); isFunc && packageRefReturnPrimaryMethods[fn.Origin()] && v.exprHasReceiverBoxInScope(selectorExpr.X) {
				resultUsed := true

				if discarded, isCall := v.resultDiscardedExpr.(*ast.CallExpr); isCall && discarded.Fun == selectorExpr {
					resultUsed = false
				}

				if resultUsed {
					if ident, ok := selectorExpr.X.(*ast.Ident); ok {
						return fmt.Sprintf("%s%s.%s", AddressPrefix, v.getIdentName(ident), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
					}
				}
			}
		}
	}

	// A POINTER-RECEIVER method called on an ELEMENT of a pointer-to-NAMED-ARRAY —
	// `bh[i].Load()` where bh is `*buckhashArray` (`[N]atomic.UnsafePointer`, runtime
	// mprof.go). The wrapper's ref indexer yields `ref TElem`, which cannot bind a ж-form
	// (direct-ж) extension method; route through the ELEMENT BOX — `bh.at<TElem>(i).Load()`
	// — which binds both the ж form and a [GoRecv] ref form (via its generated ж overload),
	// and whose array-index backing writes through to the real element storage.
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodVal {
		if fn, ok := sel.Obj().(*types.Func); ok {
			if sig, ok := fn.Type().(*types.Signature); ok && sig.Recv() != nil {
				if _, recvIsPtr := sig.Recv().Type().(*types.Pointer); recvIsPtr {
					if indexExpr, ok := selectorExpr.X.(*ast.IndexExpr); ok {
						if ptr, ok := v.info.TypeOf(indexExpr.X).(*types.Pointer); ok {
							if named, ok := types.Unalias(ptr.Elem()).(*types.Named); ok {
								if arrayType, ok := named.Underlying().(*types.Array); ok {
									elemTypeName := convertToCSTypeName(v.getScopeCheckedTypeName(arrayType.Elem()))

									return fmt.Sprintf("%s.at<%s>(%s).%s",
										v.convExpr(indexExpr.X, nil), elemTypeName,
										v.convExpr(indexExpr.Index, nil),
										v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
								}
							}
						}
					}
				}
			}
		}
	}

	// When this selector is the LHS of an assignment, any nested pointer dereference in its base
	// expression must use the assignable `.Value` form, not the value-returning `~` operator — a
	// chained `(~o).stack.hi = …` (the inner `o.stack` deref via `~`) is not a variable/property
	// (CS0131). Propagate the assignment context down to the base so inner pointer-field selectors
	// emit `o.Value.stack` instead of `(~o).stack`. Only set when assigning, so reads are unchanged.
	var xContexts []ExprContext

	if context.isAssignment {
		assignContext := DefaultLambdaContext()
		assignContext.isAssignment = true
		xContexts = []ExprContext{assignContext}
	}

	// Check if this is a method value being used in an assignment
	if v.isMethodValue(selectorExpr, context.isCallExpr) && context.isAssignment {
		// A POINTER-receiver method value over a VALUE receiver expression binds the ADDRESS,
		// so it takes the method-group-over-the-box emission below and needs no receiver
		// snapshot — see methodValueBindsReceiverAddress, which visitAssignStmt consults to
		// suppress the snapshot this arm would otherwise render through.
		// Check if selector expression needs to be converted to a lambda function for assignment
		if ident, ok := selectorExpr.X.(*ast.Ident); ok {
			if v.isPackageIdentifier(ident) {
				// This is a package selector (like fmt.Println) -- no need for lambda
				return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
			}
		}

		// An INTERFACE-receiver method value delegate-binds directly: the interface method is a
		// genuine C# instance method, so a method GROUP over the evaluated receiver expression
		// both compiles and preserves Go's bind-once semantics. The synthesized lambda below both
		// re-evaluated the receiver per call AND captured it — capturing a `ref` receiver is
		// CS1628 (go/types sizes.go's `f = conf.Sizes.Alignof` inside `func (conf *Config)`).
		// Mirrors the value-context arm below, which already leaves interface receivers on the
		// plain method-group emission.
		if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok {
			if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Recv() != nil && types.IsInterface(sig.Recv().Type()) {
				return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
			}
		}

		// A POINTER-receiver method value whose receiver expression is ALREADY a pointer binds
		// the method group over the BOX, not the deref'd value alias — `resolverFunc := r.lookupIP`
		// with `r *Resolver` otherwise renders `Ꮡr.Value.lookupIP(…)` inside the lambda, a struct
		// VALUE receiver against the [GoRecv] ж<T> extension (CS1929, net lookup.go). Render the
		// receiver ident in pointer context (`Ꮡr` for a deref'd param, the plain local otherwise).
		recvExpr := ""

		if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok {
			if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Recv() != nil {
				if _, isPtrRecv := sig.Recv().Type().(*types.Pointer); isPtrRecv {
					if recvType := v.getType(selectorExpr.X, false); recvType != nil {
						if _, alreadyPtr := recvType.(*types.Pointer); alreadyPtr {
							if identX, ok := selectorExpr.X.(*ast.Ident); ok {
								ptrContext := DefaultIdentContext()
								ptrContext.isPointer = true
								recvExpr = v.convIdent(identX, ptrContext)
							}
						} else {
							// A VALUE receiver expression under a POINTER-receiver method value is
							// Go's implicit address-of: `sw.Closesocket` IS `(&sw).Closesocket`. The
							// address binds ONCE and aliases the receiver's own storage — so this
							// arm takes the VALUE-context arm's emission wholesale: a method GROUP
							// over the box, with no receiver snapshot and no forwarding lambda.
							// Both of those are wrong here. The snapshot exists to preserve a VALUE
							// receiver's bind-a-COPY semantics, which a pointer receiver does not
							// have — the escape analysis already ruled the other way by heap-boxing
							// the local (see "A pointer-receiver METHOD VALUE heap-boxes its
							// receiver"), so snapshotting it produced `Ꮡcʗ1`, a box nothing
							// declares (CS0103). The lambda, in turn, called the [GoRecv] ж<T>
							// extension with a struct VALUE receiver (CS1929 / CS1501 — net's
							// `poll.CloseFunc = sw.Closesocket`, six hook installs).
							boundRecv := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
							return fmt.Sprintf("%s.%s", boundRecv, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
						}
					}
				}
			}
		}

		// The ASSIGNMENT member of the receiver-snapshot family (the non-assignment members are
		// handled at the value-context arm below, which carries the full account). visitAssignStmt
		// asks the capture machinery for the receiver snapshot that gives `m := x.label` Go's
		// bind-a-COPY semantics, and gets one — UNLESS something has heap-boxed the variable, at
		// which point processPotentialCapture returns early on `boxRefVars` ("must NOT be
		// snapshot-captured"). That early return is right for a CLOSURE, which has to observe later
		// writes through the shared box, and wrong for a value-receiver method value, which must
		// not. With no snapshot the receiver renders as the box and is read at CALL time:
		//
		//	x := frame{Name: "a"}; f := func() string { return x.Name }; m := x.label; x.Name = "b"
		//	    Go   "a b"        the method value copied at evaluation, the closure sees the write
		//	    C#   "b b"        `var m = () => Ꮡx.Value.label();`   -- measured, built and run
		//
		// So the site mints its OWN temp exactly where the machinery declines, and only there:
		// gated on the variable actually being box-ref, so the ordinary path keeps producing the
		// one snapshot it already produces correctly (two would be a second copy of one evaluation).
		// Gated on a VALUE, non-interface receiver besides — a pointer receiver binds the ADDRESS
		// and must not be copied at all, and marking one for snapshot is what turned an earlier
		// attempt at this family into CS1003/CS1002 across production files. A seeded census of the
		// whole standard library found 54 assignment-context method-value sites, of which 8 are
		// box-ref and ALL 8 are pointer-receiver (database/sql, go/parser x4, go/types x2, net) —
		// so this arm fires nowhere in the corpus today and the emission is expected byte-identical;
		// it closes a shape that is one refactor away, not an observed wrong answer.
		if recvExpr == "" {
			if recvIdent, isIdent := selectorExpr.X.(*ast.Ident); isIdent && v.hoistedDecls != nil && v.lambdaCapture != nil && v.isLambdaBoxRefVar(v.info.ObjectOf(recvIdent)) {
				if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok {
					if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Recv() != nil {
						_, isPtrRecv := sig.Recv().Type().(*types.Pointer)

						if !isPtrRecv && !types.IsInterface(sig.Recv().Type()) {
							if recvType := v.getType(selectorExpr.X, false); recvType != nil {
								if _, isPtr := recvType.(*types.Pointer); !isPtr {
									v.lambdaCapture.detectingCaptures = false
									snapshotName := v.getCapturedVarName(recvIdent.Name)

									savedInLambda := v.lambdaCapture.conversionInLambda
									v.lambdaCapture.conversionInLambda = false
									snapshotInit := v.convExpr(selectorExpr.X, nil)
									v.lambdaCapture.conversionInLambda = savedInLambda

									v.hoistedDecls.WriteString(fmt.Sprintf("%s%svar %s = %s;%s", v.newline, v.indent(v.indentLevel), snapshotName, snapshotInit, v.newline))
									recvExpr = snapshotName
								}
							}
						}
					}
				}
			}
		}

		if recvExpr == "" {
			recvExpr = v.convExpr(selectorExpr.X, nil)
		}

		// EVALUATE-ONCE. Go saves the receiver when the method value is created, not when the
		// resulting func is called; every emission below wraps the receiver in a LAMBDA, which
		// defers it to each invocation instead. Hoisting the already-rendered receiver into a
		// statement-level temp is kind-correct BY CONSTRUCTION rather than by a per-kind rule:
		// whatever `recvExpr` holds at this point is exactly what Go saves — a value COPY for a
		// value receiver, the bound ADDRESS for a pointer receiver (the `Ꮡ…` forms computed
		// above), the interface VALUE for an interface receiver — because the arms above have
		// already produced the kind-appropriate expression. Re-deriving it per kind here would be
		// the shape-first mistake that binds the address of a COPY.
		//
		// Measured at the family tip, all four in this arm (Go vs converted C#):
		//
		//	callRecv := makeFrame().label   receiver fn re-executed per call   1  vs  2
		//	idxMapV  := m13["k"].label      map lookup re-done per call        m13 vs M13
		//	chainPtr := p15.f.label         chain re-read through the pointer  p15 vs P15
		//	callPtr  := makePtr().bump      POINTER receiver, same as the first 1  vs  2
		//
		// The last one is why this is not restricted to value receivers: a pointer receiver's
		// expression can carry a side effect in exactly one legal shape (a call RETURNING a
		// pointer — a value result is not addressable), and it defers identically.
		recvExpr = v.hoistReceiverEvaluation(selectorExpr, recvExpr)

		// A BOUND METHOD VALUE with parameters — `d.compute = metricReader(read).compute`
		// (runtime metrics.go; compute takes (*statAggregate, *metricValue)) — forwards through
		// a lambda carrying the METHOD'S OWN parameters: the previous emission hardcoded arity
		// zero, mismatching any non-nullary target delegate (CS1593). Parameters are explicitly
		// typed (fresh pN names — no collision with the receiver expression) so the lambda binds
		// without a target-typed inference context. Note the receiver expression is evaluated
		// inside the lambda (per call) — Go binds it once at method-value creation; acceptable
		// for the compile milestone and the simple receivers observed (documented).
		if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok {
			if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Params().Len() > 0 {
				var paramDecls, paramUses strings.Builder

				for i := 0; i < sig.Params().Len(); i++ {
					if i > 0 {
						paramDecls.WriteString(", ")
						paramUses.WriteString(", ")
					}

					name := fmt.Sprintf("p%d", i+1)

					// A VARIADIC method's tail carries the `params ꓸꓸꓸT` convention through the
					// forwarding lambda, exactly as a declared function's does. Rendering it as the
					// plain `slice<T>` the signature stores froze the value at fixed arity: `errorf
					// := t.Errorf` emitted `(@string p1, slice<any> p2) => …`, so Go's loose-argument
					// calls through the value were CS1593 ("does not take 3 arguments") and CS1503
					// (a bare `n` against `slice<any>`) — slices' TestGrow/TestConcat, and the same
					// arity-mismatch family the explicit parameters themselves were introduced for.
					// The receiving `params ꓸꓸꓸany` parameter binds the forwarded Span directly, so
					// the call inside the lambda is unchanged.
					if sig.Variadic() && i == sig.Params().Len()-1 {
						if sliceType, isSlice := sig.Params().At(i).Type().Underlying().(*types.Slice); isSlice {
							paramDecls.WriteString(fmt.Sprintf("params %s %s", v.variadicParamType(sliceType.Elem()), name))
							paramUses.WriteString(name)
							continue
						}
					}

					paramDecls.WriteString(fmt.Sprintf("%s %s", convertToCSTypeName(v.getAliasQualifiedTypeName(sig.Params().At(i).Type(), false)), name))
					paramUses.WriteString(name)
				}

				return fmt.Sprintf("(%s) => %s.%s(%s)", paramDecls.String(), recvExpr,
					v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)), paramUses.String())
			}
		}

		return fmt.Sprintf("() => %s.%s()", recvExpr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
	}

	// A method VALUE over a POINTER-receiver method in a VALUE context — a call argument
	// (`s.nonDefaultOnce.Do(s.register)`, `registerMetric(…, s.nonDefault.Load)`; internal/godebug).
	// The [GoRecv] emission is a `ref`-receiver extension, and C# cannot create a delegate from an
	// extension whose first parameter is a value type (CS1113/CS1061). Go binds the receiver ADDRESS
	// once at method-value creation (`s.register` ≡ `(&s).register`); emit that same binding through
	// the box — `Ꮡs.register` / `Ꮡs.of(Setting.ᏑnonDefault).Load` — a method group over the
	// generated ж<T> receiver overload (class-typed, delegate-legal). A receiver expression that is
	// already a pointer needs no &-synthesis and keeps the plain emission (the base renders as the
	// box itself).
	if v.isMethodValue(selectorExpr, context.isCallExpr) && !context.isAssignment {
		if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok {
			if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Recv() != nil {
				if _, isPtrRecv := sig.Recv().Type().(*types.Pointer); isPtrRecv {
					if recvType := v.getType(selectorExpr.X, false); recvType != nil {
						if _, alreadyPtr := recvType.(*types.Pointer); !alreadyPtr {
							boundRecv := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
							return fmt.Sprintf("%s.%s", boundRecv, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
						}

						// A pointer-typed IDENT base whose plain rendering is the deref'd ref-local
						// — the [GoRecv] receiver `s` itself (`s.register` inside another method of
						// *Setting) or a deref'd pointer param. The isPointer ident context renders
						// the pointer VALUE (the box `Ꮡs`), which the ж<T> overload group binds to.
						if ident, ok := selectorExpr.X.(*ast.Ident); ok {
							identContext := DefaultIdentContext()
							identContext.isPointer = true
							return fmt.Sprintf("%s.%s", v.convIdent(ident, identContext), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
						}
					}
				} else if !types.IsInterface(sig.Recv().Type()) {
					// A VALUE-receiver method value in a call-argument context — `Map(c.ToUpper, s)`
					// (bytes ToUpperSpecial; c is unicode.SpecialCase): the emitted method is an
					// EXTENSION on a value type, from which C# cannot create a delegate (CS1113).
					// Forward through the same param-carrying lambda the assignment context uses —
					// `(rune p1) => c.ToUpper(p1)` (invocation through the extension is legal; only
					// delegate creation is not). Interface receivers are excluded: an interface
					// instance method delegate-binds directly. Same documented caveat as the
					// assignment form: the receiver expression re-evaluates per call.
					var paramDecls, paramUses strings.Builder

					for i := 0; i < sig.Params().Len(); i++ {
						if i > 0 {
							paramDecls.WriteString(", ")
							paramUses.WriteString(", ")
						}

						name := fmt.Sprintf("p%d", i+1)
						paramDecls.WriteString(fmt.Sprintf("%s %s", convertToCSTypeName(v.getAliasQualifiedTypeName(sig.Params().At(i).Type(), false)), name))
						paramUses.WriteString(name)
					}

					// The receiver expression is captured into the synthesized lambda, so render it in a
					// lambda conversion context: a deref-aliased pointer receiver/param inside it (`kdf`
					// in `kdf.hash.New`, crypto/internal/hpke) must go through its box `Ꮡkdf.Value`, not
					// the uncapturable `ref var kdf` alias (CS1628). The method was promoted direct-ж
					// (bodyCapturesReceiverInValueMethodValue) so the box exists. Only conversionInLambda
					// is toggled — currentLambdaVars is preserved so an ENCLOSING lambda's renames still
					// apply when this method value nests inside a func literal.
					//
					// The receiver is SNAPSHOT into a statement-level local first, because Go copies a
					// value receiver when the method value is EVALUATED, not when the resulting func is
					// called ("the method value x.M … the receiver is evaluated and saved"). Rendering
					// it live re-reads it per call, which goes wrong in two different ways depending on
					// how the enclosing slot rendered the variable — and both were measured against
					// `go run` before this was written:
					//
					//	`[]func() string{x.label, …}`  ->  `() => Ꮡx.Value.label()`  ->  Go `a b`, C# `b b`
					//	`[]any{x.label, …}`            ->  `() => x.label()`         ->  CS8175 (ref local)
					//
					// The assignment context reaches the same shape through visitAssignStmt's two
					// method-value sites, which snapshot only while nothing else has heap-boxed the
					// variable; once something has, the box render wins there too. One cause, three
					// presentations. The snapshot's initializer is rendered OUTSIDE lambda context (it
					// sits at statement level, where the plain local/alias is in scope) while the
					// wrapper body binds the snapshot name, so the box rewrite in convIdent — which
					// would otherwise turn the renamed ident into `Ꮡxʗ1.Value`, a box that does not
					// exist — is bypassed entirely rather than fought.
					//
					// Gated on a statement-level sink EXISTING: the declaration has to land somewhere,
					// and a rename with nowhere to declare is CS0103. Where there is no sink the
					// previous rendering stands — declare-or-do-not-rename. Restricted to a plain
					// IDENT receiver of non-pointer type, which is the shape the census found; a
					// POINTER receiver expression auto-deref'd to a value receiver takes the pointee
					// hoist immediately below instead, because what it has to snapshot is the DEREF.
					snapshotName := ""

					if recvIdent, isIdent := selectorExpr.X.(*ast.Ident); isIdent && v.hoistedDecls != nil && v.lambdaCapture != nil {
						if recvType := v.getType(selectorExpr.X, false); recvType != nil {
							if _, isPtr := recvType.(*types.Pointer); !isPtr {
								// getCapturedVarName only advances its per-name counter once the
								// detection phase is over (prepareStmtCaptures clears the flag for the
								// same reason); minting while it is still set hands out one name twice.
								v.lambdaCapture.detectingCaptures = false
								snapshotName = v.getCapturedVarName(recvIdent.Name)

								savedInLambda := v.lambdaCapture.conversionInLambda
								v.lambdaCapture.conversionInLambda = false
								snapshotInit := v.convExpr(selectorExpr.X, nil)
								v.lambdaCapture.conversionInLambda = savedInLambda

								// Leading newline+indent per entry is the hoisted-decls convention; the
								// TRAILING pair is what generateCaptureDeclarations also writes, and is
								// what keeps the following text off this declaration's line — the flush
								// substitutes the buffer FOR the statement's own leading newline (see
								// visitAssignStmt's flush), so a buffer that only leads glues the next
								// statement on. Same reasoning convSyscallFunnelCall records for its
								// temps.
								v.hoistedDecls.WriteString(fmt.Sprintf("%s%svar %s = %s;%s", v.newline, v.indent(v.indentLevel), snapshotName, snapshotInit, v.newline))
							}
						}
					}

					// The AUTO-DEREF'd pointee — the VALUE-context member of the shape the assignment
					// arm hoists through hoistReceiverEvaluation, and broken here in exactly the same
					// way: `call(h.p.label)` with `p *frame` renders the box into the wrapper and
					// offers a `ж<frame>` to an extension that wants a `frame` (**CS1929**), while Go
					// saved `(*h.p)`'s copy when the argument was evaluated. Same two narrowings as
					// the assignment arm (a deref-aliased ident is already the value; a promoted
					// method keeps its `.of(…)` hop emission), and the same sink rule — no sink, no
					// rename.
					if snapshotName == "" && v.receiverExprIsAutoDerefdPointee(selectorExpr) {
						if sel := v.info.Selections[selectorExpr]; sel != nil && sel.Kind() == types.MethodVal && len(sel.Index()) == 1 {
							snapshotName = v.hoistReceiverTemp(selectorExpr.X, !v.exprIsDerefAliasedPointer(selectorExpr.X))
						}
					}

					recvRender := snapshotName

					if recvRender == "" {
						recvRender = v.convExprInLambdaContext(selectorExpr.X)
					}

					return fmt.Sprintf("(%s) => %s.%s(%s)", paramDecls.String(), recvRender,
						v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)), paramUses.String())
				}
			}
		}
	}

	// Check if the selector BASE itself is an explicit dereference (or a pointer conversion whose
	// result the special-cased branch below derefs). This must inspect only the base's own outermost
	// shape — unwrapping parens and looking through a conversion-call's operand — NOT the whole
	// subtree: a `*T` star buried in an ARGUMENT (`stringStructOf((*string)(e.data)).str`, where the
	// star belongs to the conversion inside the call's argument) does not dereference the call's
	// RESULT, and treating it as if it did skipped the auto-deref (`.str` on the returned `ж<T>` box,
	// CS1061 — runtime arena.go; same for an extra-paren conversion base, mheap.go's
	// `((*specialWeakHandle)(unsafe.Pointer(…))).handle`).
	containsExplicitDeref := func(expr ast.Expr) bool {
		for {
			switch e := expr.(type) {
			case *ast.ParenExpr:
				expr = e.X
				continue
			case *ast.StarExpr:
				return true
			case *ast.CallExpr:
				// A pointer-CONVERSION base `(*T)(p)` — the Fun is a parenthesized star — reaches the
				// dedicated conversion branch below (which appends `.Value` itself); a plain call result
				// is NOT a deref regardless of stars inside its arguments.
				if parenFun, ok := e.Fun.(*ast.ParenExpr); ok {
					if _, isStar := parenFun.X.(*ast.StarExpr); isStar {
						return true
					}
				}

				return false
			default:
				return false
			}
		}
	}

	// Get the original expression type and check if it's a pointer
	if exprType := v.info.TypeOf(selectorExpr.X); exprType != nil {
		// A STAR base that is still POINTER-typed after its own deref — `(*outer.ptr).Value`
		// where ptr is a DOUBLE pointer (`**Inner`): the star peels one level (`outer.ptr.Value`,
		// a ж<Inner>), and Go's selector auto-deref supplies the second. Skipping the
		// suppression lets the normal pointer-base field handling below add it; treating the
		// star as a full deref left `.Value` on the box (CS1061 — surfaced when the
		// one-star-one-deref fix removed the old double-`.Value` compensation in convStarExpr).
		// Gated to ACTUAL star bases: a pointer-CONVERSION base (`(*T)(p).field`) is also
		// pointer-typed but must keep its dedicated branch below.
		starBaseStillPointer := false
		{
			unwrapped := ast.Expr(selectorExpr.X)

			for {
				if paren, ok := unwrapped.(*ast.ParenExpr); ok {
					unwrapped = paren.X
					continue
				}

				break
			}

			if _, isStar := unwrapped.(*ast.StarExpr); isStar {
				_, starBaseStillPointer = exprType.Underlying().(*types.Pointer)
			}
		}

		// Check if the selector base is itself an explicit dereference (or a pointer conversion)
		if containsExplicitDeref(selectorExpr.X) && !starBaseStillPointer {
			// Unwrap enclosing parens so an extra-paren conversion base — mheap.go's
			// `((*specialWeakHandle)(unsafe.Pointer(…))).handle` — reaches the conversion branch
			// (the same extra-paren blind spot the reinterpret routing had).
			baseExpr := selectorExpr.X

			for {
				if paren, ok := baseExpr.(*ast.ParenExpr); ok {
					baseExpr = paren.X
					continue
				}

				break
			}

			if callExpr, ok := baseExpr.(*ast.CallExpr); ok {
				// Check if the call expressions is a parenthesized expression
				if _, ok := callExpr.Fun.(*ast.ParenExpr); ok {
					// For a pointer-conversion-then-method like `(*atomic.Uint32)(c).Store(v)`, the
					// converted X is a heap box `ж<T>`. Appending `.Value` derefs it to a value, which
					// is only right for a VALUE-receiver method; a POINTER-receiver method (`func
					// (c *T) Store`) binds to the `ж<T>` overload, so the box itself is the receiver
					// and `.Value` must be omitted.
					if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.MethodVal {
						if sig, ok := sel.Obj().Type().(*types.Signature); ok && sig.Recv() != nil {
							if _, isPtr := sig.Recv().Type().(*types.Pointer); isPtr {
								return fmt.Sprintf("(%s).%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
							}
						}
					}

					return fmt.Sprintf("(%s).Value.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
				}
			}

			return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
		}

		if selection, ok := v.info.Selections[selectorExpr]; ok && selection.Kind() == types.FieldVal {
			if ptrType, isPtrType := exprType.(*types.Pointer); isPtrType {
				// Check if the expression is *directly* an intra-function identifier (the
				// receiver or a parameter) — a value-ref receiver/param accesses its fields
				// without a deref. Use a direct type assertion, NOT getIdentifier (which digs
				// to the root of a selector chain): for `e.list.root` the X is `e.list` (a
				// pointer field), which must be dereferenced even though the chain roots at `e`.
				if exprIdent, isIdent := selectorExpr.X.(*ast.Ident); v.inFunction && isIdent {
					if obj := v.info.ObjectOf(exprIdent); obj != nil {
						// Check if it's a receiver or parameter pointer variable
						if selVar, ok := obj.(*types.Var); ok {
							// If it's a receiver, skip dereferencing
							if v.currentFuncSignature.Recv() == selVar {
								return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
							}

							// Check if it's a function parameter — asked of the enclosing DECLARATION's
							// parameter objects, never of currentFuncSignature.
							//
							// The exemption is only true of a parameter visitFuncDecl emitted, because
							// that is what gives a pointer parameter its deref ALIAS (`ж<T> Ꮡp` in the
							// signature plus `ref var p = ref Ꮡp.DerefOrNull()` at entry), leaving the
							// Go name already a value. A func LITERAL's pointer parameter has no such
							// alias — the name IS the raw box — so it must take the deref below.
							//
							// currentFuncSignature cannot tell the two apart: convFuncLit SEEDS it with
							// the literal's OWN signature when it is nil (needed for nil-safety at the
							// receiver test just above), and nil is exactly the state of a package-level
							// `var` initializer converted before any func declaration in its file. The
							// literal's own parameter then matched this loop and the deref was dropped:
							// net/http's `var hostPortHandler = HandlerFunc(func(w ResponseWriter, r
							// *Request){…})` emitted `r.Close` / `r.RemoteAddr` on a `ж<Request>` (CS1061
							// x2), while every sibling handler literal inside a function body — where the
							// signature is the ENCLOSING declaration's — emitted `(~r).RemoteAddr`
							// correctly. Method calls on the same parameter were never affected: their
							// branches below already ask identIsParameter, which is object-identity based
							// and populated only by visitFuncDecl. Ask the same question here.
							if v.identIsParameter(exprIdent) {
								return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
							}
						}
					}
				}

				if obj := v.info.ObjectOf(selectorExpr.Sel); obj != nil {
					// Check if the field belongs to the struct that the pointer points to, rather than
					// to the pointer itself, if so, the pointer has been automatically dereferenced
					if _, ok := obj.(*types.Var); ok {
						// Make sure the field is not receiver target. The field may be a DIRECT member or
						// PROMOTED through an embedded field — both auto-dereference the pointer in Go, so
						// the check must recurse into embeds (a direct-fields-only check missed promoted
						// fields → `x.PromotedField` on a `ж<T>` box without a deref → CS1061).
						if structType, ok := ptrType.Elem().Underlying().(*types.Struct); ok {
							if structFieldReachable(structType, selectorExpr.Sel.Name) {
								// If the field belongs to the struct, automatically dereference the pointer
								if context.isAssignment {
									// Left-hand side of assignment cannot use pointer dereference operator
									return fmt.Sprintf("%s.Value.%s", v.convExpr(selectorExpr.X, xContexts), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
								} else {
									return fmt.Sprintf("(%s%s).%s", PointerDerefOp, v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
								}
							}
						}
					}
				}
			}
		}
	}

	// A pointer-receiver method PROMOTED through a single embedded field — `t.modify(…)` where `t` is a
	// `*timeTimer` and `modify` is a `(*timer)` method reached through timeTimer's embedded `timer` field
	// (Go auto-takes `&t.timer` for the promoted receiver). The receiver box (`t` / `Ꮡt`, a
	// `ж<timeTimer>`) is not the `ж<timer>` the promoted method's ж/[GoRecv]-ref overload binds to
	// (CS1929). Descend into the embedded field's box via the &-machinery — `t.of(timeTimer.Ꮡtimer)` —
	// exactly as the *explicit* `t.timer.modify(…)` already renders (the `&receiver.field` branch in
	// convUnaryExpr handles the pointer-param-vs-local box distinction: `Ꮡt` for a deref'd param, `t`
	// for a pointer local). An ALL-VALUE promotion chain of any depth descends hop by hop —
	// `tt.Common()` on reflect's `sliceType` (embeds `abi.SliceType`, which embeds `abi.Type`, the
	// method's receiver) appends an `.of(abi.SliceType.ᏑType)` view per extra hop (CS1929 ×4). A
	// deeper chain with a POINTER embed past the first hop falls through unchanged. A non-promoted
	// call (Index len 1) and an explicit `x.field.method` (also len 1) never match here.
	// A method CALL with a VALUE receiver reached through a POINTER expression — Go auto-derefs
	// (`z := new(nat); z.make(n)` calls make on *z). A deref-aliased pointer PARAM or RECEIVER
	// already renders as its value alias, but a pointer LOCAL (or other pointer-valued expr)
	// renders as the box, which the value-receiver extension cannot bind (math/big's nat
	// CS1929 ×9). Emit the value deref: `(~z).make(n)`. Direct (Index len 1) methods only —
	// promoted chains keep their own `.of()` hop machinery below.
	if context.isCallExpr {
		if sel := v.info.Selections[selectorExpr]; sel != nil && sel.Kind() == types.MethodVal && len(sel.Index()) == 1 {
			if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok {
				if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Recv() != nil {
					_, isPtrRecvMethod := sig.Recv().Type().(*types.Pointer)

					if !isPtrRecvMethod && !types.IsInterface(sig.Recv().Type()) {
						if _, exprIsPtr := v.getExprType(selectorExpr.X).(*types.Pointer); exprIsPtr {
							needsDeref := true

							if ident, isIdent := selectorExpr.X.(*ast.Ident); isIdent {
								if v.identIsParameter(ident) {
									needsDeref = false
								}

								// Object identity, not name: a pointer LOCAL shadowing the
								// receiver name renders as its box and still needs the deref
								// (identResolvesToReceiver).
								if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(ident, recvName) {
									needsDeref = false
								}
							}

							if needsDeref {
								return fmt.Sprintf("(%s%s).%s", PointerDerefOp, v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
							}
						}
					}
				}
			}
		}
	}

	// A method promoted through an embedded INTERFACE field — signalCtx embeds
	// context.Context, and `c.Done()` is the interface VALUE's method (no signalCtx
	// extension exists), so the bare call bound an unrelated same-package extension by
	// NAME (sync's Done, CS1929 x2 in os/signal). Hop through the field:
	// `(~c).Context.Done()` — deref a pointer local; a deref-aliased param/receiver
	// already renders as the value alias.
	if context.isCallExpr {
		if sel := v.info.Selections[selectorExpr]; sel != nil && sel.Kind() == types.MethodVal && len(sel.Index()) == 2 {
			recvType := v.info.TypeOf(selectorExpr.X)

			if ptr, ok := recvType.Underlying().(*types.Pointer); ok {
				recvType = ptr.Elem()
			}

			if structType, ok := recvType.Underlying().(*types.Struct); ok {
				embedField := structType.Field(sel.Index()[0])

				if embedField.Embedded() {
					if _, isIface := embedField.Type().Underlying().(*types.Interface); isIface {
						xExpr := v.convExpr(selectorExpr.X, nil)

						if _, exprIsPtr := v.getExprType(selectorExpr.X).(*types.Pointer); exprIsPtr {
							needsDeref := true

							if ident, isIdent := selectorExpr.X.(*ast.Ident); isIdent {
								if v.identIsParameter(ident) {
									needsDeref = false
								}

								// Object identity, not name: a pointer LOCAL shadowing the
								// receiver name renders as its box and still needs the deref
								// (identResolvesToReceiver).
								if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(ident, recvName) {
									needsDeref = false
								}
							}

							if needsDeref {
								xExpr = fmt.Sprintf("(%s%s)", PointerDerefOp, xExpr)
							}
						}

						return fmt.Sprintf("%s.%s.%s", xExpr, getSanitizedIdentifier(embedField.Name()), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)))
					}
				}
			}
		}
	}

	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) {
		if sel := v.info.Selections[selectorExpr]; sel != nil && sel.Kind() == types.MethodVal && len(sel.Index()) >= 2 {
			recvType := v.info.TypeOf(selectorExpr.X)

			if ptr, ok := recvType.Underlying().(*types.Pointer); ok {
				recvType = ptr.Elem()
			}

			if structType, ok := recvType.Underlying().(*types.Struct); ok {
				embedField := structType.Field(sel.Index()[0])

				// Only a VALUE (struct) embedded field needs the box descent: the field itself is not a
				// box, so `&field` (`.of(Type.Ꮡfield)`) materializes the `ж<field>` the promoted method
				// binds to. A POINTER embed (`traceWriter` embeds `traceBufPtr` = `*traceBuf`) already
				// yields the box as the field VALUE (`w.traceBuf`), so it is left to the existing
				// field-access handling — taking its address would double-box to `ж<ж<T>>` (CS1929).
				if embedField.Embedded() {
					if ptr, isPtr := embedField.Type().Underlying().(*types.Pointer); isPtr {
						// A POINTER embed whose embedded type is CROSS-PACKAGE has no generated
						// method forwarder: method promotion is syntax-resolved, and a metadata
						// embed promotes FIELDS only (the StructTypeTemplate metadata fallback) —
						// `t.Uncommon()` on `Δrtype` (embeds `*abi.Type`, runtime type.go) is
						// CS1929. Emit the explicit hop through the embed field's box —
						// `t.Type.Value.Uncommon(…)` — the deref'd `.Value` is a ref return, so the
						// `[GoRecv] ref` extension binds addressably. A same-package pointer embed
						// keeps its generated forwarder (no churn).
						if named, ok := ptr.Elem().(*types.Named); ok {
							// The pointer-embed hop stays single-level (Index == [embed, method]);
							// a deeper chain THROUGH a pointer embed falls through unchanged.
							if len(sel.Index()) == 2 && named.Obj().Pkg() != nil && named.Obj().Pkg() != v.pkg {
								// The hop names the FIELD, which is struct-scoped: a field named
								// like a Δ-renamed package type (rtype's embedded `Type` vs the
								// reflectlite `Type` interface) is DECLARED unrenamed, so the hop
								// must not apply the package-level rename (`t.ΔType` is CS1061).
								// A DIRECT-Ж target binds the embed field's box itself; only a
								// [GoRecv] ref target binds the deref'd `.Value` ref-return
								// (abi.Type.Uncommon promoted direct-ж by the pointer-arg
								// detector - `t.Type.Value.Uncommon()` was CS1929).
								deref := ".Value"

								if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok && packageDirectBoxReceiverMethods[funcObj.Origin()] {
									deref = ""
								}

								xExpr := v.convExpr(selectorExpr.X, nil)

								// X itself may be a POINTER rendering as a raw BOX (a ж local,
								// not a deref-aliased param/receiver) — hop through its Value
								// first (unique's `m.Load(value)` with m a ж<uniqueMap[T]>
								// local emitted `m.HashTrieMap…`, CS1061 ×4).
								if _, xIsPtr := v.info.TypeOf(selectorExpr.X).Underlying().(*types.Pointer); xIsPtr && !v.exprIsDerefAliasedPointer(selectorExpr.X) {
									xExpr += ".Value"
								}

								return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s%s.%s", xExpr,
									v.structFieldBoxName(&ast.Ident{Name: embedField.Name()}, selectorExpr.X), deref, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
							}
						}

						// A POINTER embed followed by an all-VALUE tail. net/http's transport test is
						// the shape — `type breakableConn struct { net.Conn; *brokenState }` over
						// `type brokenState struct { sync.Mutex; broken bool }` — so `w.Lock()` inside
						// `(*breakableConn).Write` promotes TWO hops. Neither arm claimed it: the
						// cross-package pointer arm just above is single-level by construction, and the
						// all-value descent below refuses a chain that STARTS at a pointer embed. It
						// fell through to the bare `Ꮡw.Lock()`, which binds nothing on the box and lets
						// overload resolution reach an unrelated same-named extension — CS1929 naming
						// `http_package.fakeLocker` from a transport test, the same misleading shape a
						// missing hop always produces.
						//
						// The first hop needs no &-machinery precisely BECAUSE it is a pointer embed:
						// the field's VALUE already IS the `ж<brokenState>` box (the reason the arm
						// above leaves pointer embeds to ordinary field access). Every remaining hop is
						// a value embed, so it composes as the same `.of(<Owner>.Ꮡ<field>)` view the
						// all-value descent uses, landing on the ж<> box of the method's receiver type:
						// `w.brokenState.of(brokenState.ᏑMutex).Lock()`. A tail broken by a second
						// pointer embed or a non-struct hop falls through unchanged, as before.
						if len(sel.Index()) > 2 {
							tailStruct, tailOK := ptr.Elem().Underlying().(*types.Struct)
							tailFields := make([]*types.Var, 0, len(sel.Index())-2)
							allValueTail := true

							for _, idx := range sel.Index()[1 : len(sel.Index())-1] {
								if !tailOK {
									allValueTail = false
									break
								}

								hop := tailStruct.Field(idx)

								if !hop.Embedded() {
									allValueTail = false
									break
								}

								if _, isHopPtr := hop.Type().Underlying().(*types.Pointer); isHopPtr {
									allValueTail = false
									break
								}

								tailFields = append(tailFields, hop)
								tailStruct, tailOK = hop.Type().Underlying().(*types.Struct)
							}

							if allValueTail && len(tailFields) > 0 {
								xExpr := v.convExpr(selectorExpr.X, nil)

								// X itself may render as a raw BOX (a ж local rather than a
								// deref-aliased param/receiver) — hop through its Value first, exactly
								// as the single-level pointer arm above does.
								if _, xIsPtr := v.info.TypeOf(selectorExpr.X).Underlying().(*types.Pointer); xIsPtr && !v.exprIsDerefAliasedPointer(selectorExpr.X) {
									xExpr += ".Value"
								}

								hopExpr := fmt.Sprintf("%s.%s", xExpr, v.structFieldBoxName(&ast.Ident{Name: embedField.Name()}, selectorExpr.X))
								owner := ptr.Elem()

								for _, hop := range tailFields {
									ownerTypeName := convertToCSTypeName(v.getAliasQualifiedTypeName(owner, false))
									hopExpr = fmt.Sprintf("%s.of(%s.%s%s)", hopExpr, v.boxAccessorType(ownerTypeName, "", owner),
										AddressPrefix, v.structFieldBoxName(&ast.Ident{Name: hop.Name()}, selectorExpr.X))
									owner = hop.Type()
								}

								return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", hopExpr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
							}
						}
					} else {
						// Resolve the FULL promotion chain: every hop (all Index entries but the
						// method's) must be an embedded VALUE struct field. A chain broken by a
						// pointer embed or a non-struct hop falls through unchanged.
						hopFields := []*types.Var{embedField}
						hopStruct, hopOK := embedField.Type().Underlying().(*types.Struct)
						allValueChain := true

						for _, idx := range sel.Index()[1 : len(sel.Index())-1] {
							if !hopOK {
								allValueChain = false
								break
							}

							hop := hopStruct.Field(idx)

							if !hop.Embedded() {
								allValueChain = false
								break
							}

							if _, isPtr := hop.Type().Underlying().(*types.Pointer); isPtr {
								allValueChain = false
								break
							}

							hopFields = append(hopFields, hop)
							hopStruct, hopOK = hop.Type().Underlying().(*types.Struct)
						}

						if allValueChain {
							// When the base is the current method's OWN receiver and that method is NOT
							// direct-ж, the receiver renders as `this ref T` with NO box — the descent's
							// `Ꮡrecv.of(…)` references a nonexistent `Ꮡrecv` (CS0103; runtime mgcscavenge
							// `sc.setEmpty()` inside `(*scavChunkData).alloc/free`). No box is needed
							// either: the embedded field(s) of a `ref` receiver are addressable, so the
							// promoted method's `[GoRecv] ref` overload binds on the explicit field call
							// `recv.embedField(…).method(…)`. (A direct-ж TARGET on the bare receiver would
							// have promoted the enclosing method via the capture-mode fixpoint, so this
							// arm's target always has the `ref` overload.) The receiver match is by
							// OBJECT identity (identResolvesToReceiver), so an inner binding that
							// shadows the receiver name keeps the descent path; the rendered==raw
							// check stays as the fallback defense for an unresolvable ident.
							if recvIdent, isIdent := selectorExpr.X.(*ast.Ident); isIdent {
								if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(recvIdent, recvName) &&
									v.getIdentName(recvIdent) == recvIdent.Name && !isDirectBoxReceiverMethod(v.currentFuncDecl, v.info) {
									hopPath := make([]string, 0, len(hopFields))

									for _, hop := range hopFields {
										hopPath = append(hopPath, v.structFieldBoxName(&ast.Ident{Name: hop.Name()}, selectorExpr.X))
									}

									return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s.%s", v.convExpr(selectorExpr.X, nil),
										strings.Join(hopPath, "."), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
								}
							}

							// First hop through the &-machinery (box-vs-param distinction), then one
							// `.of(<Owner>.Ꮡ<field>)` view per additional hop — the ж<T> field view
							// composes, landing on the ж<> box of the method's receiver type.
							embedSel := &ast.SelectorExpr{X: selectorExpr.X, Sel: &ast.Ident{Name: embedField.Name()}}
							fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: embedSel}, DefaultUnaryExprContext())

							// A promoted box-receiver method reached through an UNEXPORTED embed of a
							// FOREIGN package cannot descend via `<box>.of(T.Ꮡ<embed>)`: the `Ꮡ<embed>` box
							// accessor is `internal` (matching the embed's unexportedness), invisible
							// cross-assembly (CS0117 — crypto/internal/cryptotest reaching testing.T.common's
							// promoted Errorf/Helper/Logf). Call the method DIRECTLY on the receiver box —
							// go2cs-gen emits a public `M(this ж<T>)` descent shim for exactly this case
							// (IsValueEmbedBoxRecv). Reuse the first-hop box the &-machinery just computed,
							// dropping the inaccessible trailing `.of(…)` view. Single-hop only (a deeper
							// foreign value chain has no shim); the box is the text before the last `.of(`.
							if len(hopFields) == 1 && !embedField.Exported() && embedField.Pkg() != nil && embedField.Pkg() != v.pkg {
								if ofIndex := strings.LastIndex(fieldAddr, ".of("); ofIndex != -1 && strings.HasSuffix(fieldAddr, ")") {
									box := fieldAddr[:ofIndex]
									return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", box, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
								}

								// A POINTER receiver EXPRESSION that renders as the raw box — a
								// type-assert/call chain, `pkg.Scope().Lookup(name)._<ж<types.TypeName>>()
								// .Type()` (go/internal/gcimporter iimport.go) — has no `.of(…)` view to
								// strip: the &-machinery boxed a COPY (`Ꮡ(x.@object)`), whose spelled
								// embed hop is `internal` cross-assembly (CS1061). The box itself IS the
								// receiver — call the promoted member straight on it; the generated
								// forwarder pair on the enclosing struct includes a public
								// `M(this ж<T>)` overload that derefs and descends in its own assembly.
								// A pointer IDENT (raw-box local or deref-aliased param) renders its
								// address as `<box>.of(…)` and stays with the strip above.
								if _, xIsPtr := v.info.TypeOf(selectorExpr.X).Underlying().(*types.Pointer); xIsPtr && !v.exprIsDerefAliasedPointer(selectorExpr.X) {
									return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
								}
							}

							// The &-machinery's LAST-RESORT arm renders `Ꮡ(<value>)` — a box over a
							// COPY, because a C# struct VALUE has no address to alias. Descending that
							// box reaches the copy, so every write a promoted POINTER-RECEIVER method
							// makes is discarded. It went unnoticed while an embed was held in a shared
							// ж<T> box, which made the copy's embed the SOURCE's embed and repaired the
							// write by accident; with the embed an inline field (its correct Go shape)
							// the loss is real, and it surfaced as three behavioral tests going quiet —
							// `PromotedEmbedZeroValueField`'s count stuck at 0, `LocalShadowsEmbedHopType`
							// summing into nothing, `CrossPkgUser`'s two-level `rg.Calibrate(3)`.
							//
							// No box is needed here for the same reason the receiver arm above needs
							// none: every hop accessor is an `[UnscopedRef] ref` property, so the plain
							// member chain `<base>.<hop>….<method>` is a genuine ref into the base's own
							// storage and binds the promoted method's `[GoRecv] ref` overload with
							// faithful write-through. Gated to a copy box (a real box descends
							// correctly and keeps its path byte for byte) and to a target that HAS the
							// ref overload — a direct-ж method takes `ж<T>` and would not bind (CS1929),
							// and for it the copy box remains the only available spelling.
							if strings.HasPrefix(fieldAddr, AddressPrefix+"(") && !v.selectorTargetIsDirectBoxMethod(selectorExpr) {
								hopPath := make([]string, 0, len(hopFields))

								for _, hop := range hopFields {
									hopPath = append(hopPath, v.structFieldBoxName(&ast.Ident{Name: hop.Name()}, selectorExpr.X))
								}

								return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s.%s", v.convExpr(selectorExpr.X, nil),
									strings.Join(hopPath, "."), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
							}

							for k := 1; k < len(hopFields); k++ {
								ownerTypeName := convertToCSTypeName(v.getAliasQualifiedTypeName(hopFields[k-1].Type(), false))
								fieldAddr = fmt.Sprintf("%s.of(%s.%s%s)", fieldAddr, v.boxAccessorType(ownerTypeName, "", hopFields[k-1].Type()),
									AddressPrefix, v.structFieldBoxName(&ast.Ident{Name: hopFields[k].Name()}, selectorExpr.X))
							}

							return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
						}
					}
				}
			}
		}
	}

	// A capture-mode method called on a value field of the current direct-ж receiver —
	// `b.u.Load()` where the struct embeds an atomic-like type as a value field `u`. The callee's
	// ж overload needs a `ж<FieldType>` aliasing the real field; emit it as `(&b.u).Load()`, which
	// the `&recv.field` machinery in convUnaryExpr renders as `Ꮡb.of(Bool.Ꮡu)`. The enclosing
	// method was marked direct-ж (so `Ꮡb` is in scope) by bodyCallsCaptureModeMethodOnReceiverField.
	// I3 — the CONSUMER half of the cross-package lowering contract (refVerdictPublication.go).
	// A method an IMPORTED package published a `ref` primary for, called on a value FIELD of a base
	// the caller ALREADY holds as a ref, binds the plain member chain instead of minting a
	// field-address box for the receiver: `fd.l.Lock()`, not `Ꮡfd.of(FD.Ꮡl).Lock()`.
	//
	// This is the same mechanism the promoted-embed arm above already uses, and for the same reason:
	// every hop accessor is an `[UnscopedRef] ref` property, so the member chain is a genuine ref
	// into the base's own storage and binds the `[GoRecv] ref` overload with faithful write-through.
	// What the published record adds is the EXISTENCE PROOF that the overload is there to bind —
	// without it the box must stay, because an unpublished primary may simply not exist in the other
	// assembly and the call would be CS1929.
	//
	// The BASE test is deliberately narrower than exprIsCaptureModeFieldBase's. That predicate also
	// admits a bare pointer IDENT, whose rendering is the BOX (`e`), and `e.field` does not compile
	// on a ж<T> — the field is reached through `.Value`, which this spelling does not produce. Only
	// a base that renders as a ref LVALUE qualifies: the current method's deref-aliased receiver, or
	// a deref-aliased pointer parameter. Anything else falls through to the box arm below unchanged.
	if context.isCallExpr && v.calleePublishesRefPrimary(selectorExpr) && v.exprIsCaptureModeFieldBase(selectorExpr.X) {
		if baseSel, baseIsSel := selectorExpr.X.(*ast.SelectorExpr); baseIsSel &&
			(v.exprIsCurrentDirectBoxReceiver(baseSel.X) || v.exprIsDerefAliasedPointer(baseSel.X)) {
			return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, nil),
				v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
		}
	}

	if context.isCallExpr && v.isCaptureModeMethod(selectorExpr) && v.exprIsCaptureModeFieldBase(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// Route a capture-mode method called on a heap-boxed value receiver through the ж
	// (pointer) overload, which sets up the receiver box the method needs for
	// `&recv.field` — e.g. `var i atomic.Int32; i.Store(10)` → `Ꮡi.Store(10)`. The
	// receiver may be a heap-boxed value var (escape analysis), the current method's own direct-ж
	// receiver (its box `Ꮡrecv` is the parameter) — e.g. `func (r *Ring) Next() { return r.init() }`
	// — or a deref'd pointer parameter (its box `Ꮡp` is the parameter), e.g.
	// `func (r *Ring) Link(s *Ring) { s.Prev() }`. In each case route through the box.
	// A receiver whose GO TYPE is already a POINTER and whose RENDERING yields the box directly
	// (`itabTable.find(…)` where `var itabTable *itabTableType` is an addressed global: the
	// PROPERTY value is the ж<itabTableType> receiver) must not route through `Ꮡ` — that passes
	// the global's SLOT box (ж<ж<itabTableType>>), one layer too high (CS1929, runtime iface.go).
	// A DEREF-ALIASED pointer param/receiver is the opposite: its rendering is the value alias
	// (`ref var s = ref Ꮡs.Value`), so it still needs the box route.
	_, receiverIsPointerValue := v.info.TypeOf(selectorExpr.X).(*types.Pointer)
	receiverYieldsBox := receiverIsPointerValue && !v.exprIsDerefAliasedPointer(selectorExpr.X)

	if context.isCallExpr && !receiverYieldsBox && v.isCaptureModeMethod(selectorExpr) && !v.exprIsAlreadyBoxedPointerFieldOrElement(selectorExpr.X) && (v.isHeapBoxedExpr(selectorExpr.X) || v.exprIsCurrentDirectBoxReceiver(selectorExpr.X) || v.exprIsDerefdPointerParam(selectorExpr.X) || v.exprIsPointerLocalField(selectorExpr.X)) {
		// When the receiver base is itself a FIELD selector or an INDEX into a heap-boxed value —
		// e.g. a boxed global's atomic field `ctrl.total.Add()`, or `trace.stackTab[i].dump()` where
		// `trace` is an address-taken global — the box address must go through the &-machinery, which
		// emits `Ꮡctrl.of(controller.Ꮡtotal)` / `Ꮡtrace.of(…ᏑstackTab).at<T>(i)`. Naively prefixing
		// `Ꮡ` to `ctrl.total` / `trace.stackTab[i]` would instead bind to the box variable `Ꮡctrl` /
		// `Ꮡtrace` (whose value type has no such member) → CS1061.
		switch selectorExpr.X.(type) {
		case *ast.SelectorExpr, *ast.IndexExpr:
			fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
			return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
		}

		// The receiver box is `Ꮡ`+the DECLARING-scope box-base name — which differs between a
		// deref-aliased pointer param and an escaping value LOCAL. A deref-aliased pointer is declared
		// `ref var <shadow-name> = ref Ꮡ<raw>.Value` (its box keeps the RAW name; visitFuncDecl / heap
		// decls never shadow-rename the `Ꮡ` companion), so for a collision-renamed param `p`→`Δp` the
		// box is `Ꮡp`. A heap-boxed value LOCAL is the OPPOSITE — it is declared `ref var <shadow-name>
		// = ref heap(new T(), out var Ꮡ<shadow-name>)`, so a shadow-renamed `b`→`bΔ1` keeps its box
		// under the RENDERED name `ᏑbΔ1` (crypto/x509 marshalCertificate's inner `var b
		// cryptobyte.Builder`, renamed to dodge the outer method's own `b`). Using the raw name for the
		// local emitted `Ꮡb`, colliding with the outer `b`'s box declared later (CS0841/CS0103).
		// boxBaseName resolves the correct box base for each — but it must read the var's DECLARING
		// name, not the current lambda's capture remap: a heap-boxed local captured by a closure keeps
		// its box under the declaring name (`Ꮡonce`, the box is captured directly), NOT the value-
		// snapshot capture name `onceʗ1` (there is no `Ꮡonceʗ1` box — CS0103, sync OnceFunc). Disable
		// conversionInLambda across the boxBaseName call so getIdentName falls to the shadowing name
		// (`bΔ1`) or raw name (`once`), never the capture form.
		recvExpr := v.convExpr(selectorExpr.X, nil)

		// Substitute the box base only when the receiver renders as something other than its raw name
		// (shadow-renamed local, collision-renamed param, or a capture form). boxBaseName returns the
		// raw name for an unrenamed var, so an already-correct accessor is unchanged — no golden churn.
		if ident, ok := selectorExpr.X.(*ast.Ident); ok && recvExpr != ident.Name {
			savedInLambda := false
			if v.lambdaCapture != nil {
				savedInLambda = v.lambdaCapture.conversionInLambda
				v.lambdaCapture.conversionInLambda = false
			}
			recvExpr = v.boxBaseName(ident)
			if v.lambdaCapture != nil {
				v.lambdaCapture.conversionInLambda = savedInLambda
			}
		}

		return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s%s.%s", AddressPrefix, recvExpr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A (non-capture) pointer-receiver method called on a FIELD of a pointer LOCAL — `c.gp.set(v)`
	// where `c` is a `*coro` local and `set` has a pointer receiver. The `[GoRecv]` method needs an
	// addressable receiver, but the value `~` deref of the field is an rvalue (CS1510 on the
	// generated `ref`). Take the field's box address via the &-machinery so the call binds the `ж`
	// overload: `c.of(coro.Ꮡgp).set(v)`.
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) && v.exprIsPointerLocalField(selectorExpr.X) && !v.exprIsAlreadyBoxedPointerFieldOrElement(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A ж-only (pointer-receiver) method called on a value field rooted at a package value global —
	// `prof.signalLock.Store(…)`, `trace.seqlock.Load()`, `Δscavenge.gcPercentGoal.Store(…)` (the
	// atomic-field-of-a-global pattern). The field's value `~`/`.` access is not a box, so the
	// ж overload cannot bind (CS1929). Route the receiver through the &-machinery, which renders the
	// real field box `Ꮡglobal.of(T.Ꮡfield)`; the global is heap-boxed by collectAddressedGlobals'
	// matching pointer-receiver-method-on-global-field handling.
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) && v.exprFieldRootsAtAddressedGlobal(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A ж-only (pointer-receiver) method called on a VALUE field reached through a POINTER
	// expression — `(~(~gp).m).mLockProfile.waitTime.Add(…)`, `sgp.g.selectDone.CompareAndSwap(…)`
	// — where the base (`gp.m`, `sgp.g`) is a pointer field/chain and the field is a value (atomic)
	// field. The `~`/`.` value access is an rvalue, so the [GoRecv] ref / ж overload cannot bind
	// (CS1510 / CS1929). Route the receiver through the &-machinery, which field-refs the REAL
	// storage as `gp.m.of(mType.ᏑmLockProfile).of(…ᏑwaitTime)` — never a `Ꮡ(value)` copy, which
	// would silently lose the atomic write. (A pointer-LOCAL field is handled by the
	// exprIsPointerLocalField branch above; this covers a pointer field/chain or pointer param.)
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) && v.exprIsValueFieldOfPointer(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A ж-only / pointer-receiver method called on a VALUE field rooted at a pointer-to-struct RVALUE —
	// `(~getg()).schedlink.set(…)`, `(~batch[i]).schedlink.set(…)`, `(~Δp.chunkOf(ci)).scavenged.setRange(…)`,
	// `(~getg().m.p.ptr()).wbBuf.get2()` — where the base is a pointer-returning CALL or a pointer ELEMENT
	// index (an rvalue, not an addressable ident/field). The `~`-deref field access is an rvalue, so the
	// [GoRecv] ref overload cannot bind (CS1510). The root call/index value IS a `ж<T>` box, so route the
	// receiver through the &-machinery, which renders the real field storage `root.of(T.Ꮡfield)` — never a
	// `Ꮡ(value)` copy (which would silently lose the write). The complement of the param/receiver/field
	// roots handled above (exprIsValueFieldOfPointerRvalue is disjoint from those predicates).
	if context.isCallExpr && v.isPointerReceiverMethodCall(selectorExpr) && v.exprIsValueFieldOfPointerRvalue(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A DIRECT-ж (box-receiver) method called on a VALUE field-chain rooted at a deref-aliased pointer
	// PARAMETER/RECEIVER — `Δp.scav.index.find()`, `mp.trace.seqlock.Load()`, `h.userArena.readyList.remove(s)`.
	// The value field-chain is not a box, so the ж overload cannot bind (CS1929). The param/receiver root
	// has a box (`Ꮡp`/`Ꮡrecv`), so route the receiver through the &-machinery, which renders the real
	// nested storage `Ꮡp.of(T.Ꮡf1).of(…Ꮡf2)` — never a `Ꮡ(value)` copy (which would silently lose an
	// atomic write). GATED to direct-ж: a `[GoRecv] ref` method binds directly on the addressable value
	// field-chain (the deref-alias root is a `ref`), so it is left untouched — rerouting it would churn
	// working output (this is why exprIsValueFieldOfDerefdPointerRoot is the param/receiver complement of
	// exprIsValueFieldOfPointer, which serves the broader isPointerReceiverMethodCall branch above).
	if context.isCallExpr && v.selectorCallsDirectBoxMethod(selectorExpr) && v.exprIsValueFieldOfDerefdPointerRoot(selectorExpr.X) && !v.exprIsAlreadyBoxedPointerFieldOrElement(selectorExpr.X) {
		fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A ж-only / pointer-receiver method called on a VALUE element of an addressable array/slice —
	// `bh.Value[i].Load()` (an array of atomic `UnsafePointer`). The element value is not a box, so the
	// `[GoRecv] ref` / `ж` overload cannot bind (CS1510 / CS1929). Route the receiver through the
	// element's box via the &-machinery, which renders the real element address (`Ꮡ(slice, i)` /
	// `…at<T>(i)`) — never a `Ꮡ(value)` copy, which would lose an atomic write.
	if context.isCallExpr && v.selectorCallsDirectBoxMethod(selectorExpr) && v.exprIsIndexedValueElement(selectorExpr.X) && !v.exprIsAlreadyBoxedPointerFieldOrElement(selectorExpr.X) {
		elemAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
		return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", elemAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
	}

	// A DIRECT-ж (box-receiver) method called on a remaining VALUE field-chain — a field of a
	// plain VALUE param (`ip.addr.halves()`, netip's uint128: Go auto-addresses &ip.addr,
	// CS1929 ×2). The pointer-rooted chains took the arms above; this one routes the value
	// chain through the &-machinery, which boxes a COPY for an unaddressable value-param
	// field — faithful here, because the enclosing receiver is itself a copy (Go value-param
	// semantics: writes through the halves would only ever reach the local copy in Go too).
	if context.isCallExpr && v.selectorCallsDirectBoxMethod(selectorExpr) && !v.exprIsAlreadyBoxedPointerFieldOrElement(selectorExpr.X) {
		if _, isFieldChain := selectorExpr.X.(*ast.SelectorExpr); isFieldChain {
			if exprType := v.getType(selectorExpr.X, false); exprType != nil {
				if _, isPtr := exprType.Underlying().(*types.Pointer); !isPtr {
					fieldAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
					return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", fieldAddr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
				}
			}
		}
	}

	// A generic function referenced as a VALUE — a method-group argument like `slices.SortFunc(all,
	// slices.Compare)` — must spell its type arguments explicitly: C# cannot infer a generic method
	// group's type parameters when converting it to a delegate (CS0411). go/types recorded the inferred
	// instantiation in info.Instances keyed by the selector's Sel. The call-CALLEE path (isCallExpr) is
	// excluded — convCallExpr's own type-arg mechanism (the 66be4f914 site) handles a generic call.
	if !context.isCallExpr && !context.suppressGenericTypeArgs {
		if _, isFunc := v.info.Uses[selectorExpr.Sel].(*types.Func); isFunc {
			if inst, ok := v.info.Instances[selectorExpr.Sel]; ok && inst.TypeArgs != nil && inst.TypeArgs.Len() > 0 {
				// Erased (pointer-core) callee positions leave the emitted list (see
				// renderedTypeArgs); a list that erases to empty falls through to the bare form.
				if typeArgs := v.renderedTypeArgs(selectorExpr.Sel, inst.TypeArgs); len(typeArgs) > 0 {
					return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s<%s>", v.convExpr(selectorExpr.X, xContexts),
						v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr)), strings.Join(typeArgs, ", ")))
				}
			}
		}
	}

	return v.aliasResolvedSelector(selectorExpr, fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, xContexts), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))))
}

func (v *Visitor) getSelIdentContext(selectorExpr *ast.SelectorExpr) IdentContext {
	context := DefaultIdentContext()
	context.isMethod = true

	// Flag a field selector whose name collides with its enclosing struct's type name so the
	// access is renamed to match the renamed field declaration (CS0542).
	if sel, ok := v.info.Selections[selectorExpr]; ok && sel.Kind() == types.FieldVal {
		context.isField = true
		context.fieldCollidesWithType = v.fieldCollidesWithType(selectorExpr.Sel, selectorExpr.X)

		if context.fieldCollidesWithType {
			context.fieldTypeIsRenamed = v.fieldTypeIsRenamed(selectorExpr.X)
		}
	}

	return context
}

// hoistReceiverEvaluation renders a method value's receiver EXACTLY ONCE, into a statement-level
// temp, and returns the name the wrapper lambda should bind instead of the expression.
//
// Go's rule (spec, Method values): the receiver expression is evaluated and SAVED when the method
// value is created. Every wrapper-lambda emission in this file defers it to each invocation
// instead, which is wrong in two independent ways that were measured separately:
//
//   - a receiver expression that CALLS something re-executes it per invocation -- a side-effect
//     defect, not a timing nicety, and it is KIND-INDEPENDENT (measured on both a value receiver,
//     `makeFrame().label`, and a pointer receiver, `makePtr().bump`: Go counts 1, C# counted 2);
//   - a receiver expression that READS through a reference-semantics base -- a pointer, a map --
//     re-reads live state, because the capture machinery's root-ident snapshot copies the
//     REFERENCE and keeps aliasing the same object (`p15.f.label`, `m13["k"].label`).
//
// The temp is kind-correct BY CONSTRUCTION rather than by a per-kind rule: `rendered` is whatever
// the arm above already produced, which is exactly what Go saves -- a value COPY for a value
// receiver, the bound ADDRESS (`Ꮡ…`) for a pointer receiver, the interface VALUE for an interface
// receiver. Deriving the temp's content from the KIND here instead would invite the shape-first
// error of snapshotting a value and binding ITS address, which silently redirects every write
// through the method value into the copy.
//
// A bare IDENT receiver is returned untouched, and that is not an exception to the once-rule: a
// local read has no side effect and nothing to alias, and the ident paths have already produced a
// once-evaluated temp of their own (the capture machinery's snapshot, or the box-ref arm above).
// Hoisting it again would emit a second copy of a single evaluation, not a second evaluation.
//
// Where there is no statement-level sink, the previous rendering stands and the site is left as it
// was -- never apply a rename you cannot also declare.
func (v *Visitor) hoistReceiverEvaluation(selectorExpr *ast.SelectorExpr, rendered string) string {
	if rendered == "" || v.hoistedDecls == nil || v.lambdaCapture == nil {
		return rendered
	}

	// A POINTER-typed receiver expression under a VALUE receiver is Go's implicit deref: `h.p.label`
	// with `p *frame` and a value-receiver `label` IS `(*h.p).label`, so what Go saves is the
	// POINTEE'S COPY, not the pointer. Hoisting the expression as the enclosing context renders it
	// — the box — binds a `ж<T>` where the emitted extension wants a `T`, which does not compile
	// (**CS1929**), and would be semantically wrong even where it did: the deref would then happen at
	// CALL time and observe later writes to the pointee, and a repointed pointer would be followed.
	//
	// So the temp holds the DEREF, which is what makes this shape a hoist rather than a decline: the
	// pointee copy IS the once-evaluated receiver, and `operator ~` returns `T` by value, so the
	// copy semantics of a value receiver come from the deref itself rather than from a rule about
	// it. This checks BEFORE the bare-ident early return below, because there the ident's own
	// once-evaluated rendering is the BOX — a different value from the pointee, so hoisting it is
	// not the second copy of one evaluation that return exists to prevent.
	//
	// Two narrowings, each matching a rule the CALL path already proved (the `(~z).make(n)` arm):
	// a receiver expression that is already deref-ALIASED (a pointer parameter, or the enclosing
	// method's pointer receiver) renders as the VALUE, so a second `~` would deref a non-pointer
	// (CS0023); and a PROMOTED method (selection index depth > 1) reaches its receiver through the
	// `.of(…)` hop machinery, so it keeps its existing emission rather than being handed a deref
	// this helper cannot place — do not hoist what you cannot render correctly.
	if v.receiverExprIsAutoDerefdPointee(selectorExpr) {
		if sel := v.info.Selections[selectorExpr]; sel != nil && sel.Kind() == types.MethodVal && len(sel.Index()) == 1 {
			if tempName := v.hoistReceiverTemp(selectorExpr.X, !v.exprIsDerefAliasedPointer(selectorExpr.X)); tempName != "" {
				return tempName
			}
		}

		return rendered
	}

	if _, isIdent := selectorExpr.X.(*ast.Ident); isIdent {
		return rendered
	}

	if tempName := v.hoistReceiverTemp(selectorExpr.X, false); tempName != "" {
		return tempName
	}

	return rendered
}

// receiverExprIsAutoDerefdPointee reports whether a method value's receiver EXPRESSION is
// POINTER-typed while the method's declared receiver is a VALUE — Go's implicit dereference,
// `h.p.label` ≡ `(*h.p).label`. The pointer is NOT what the method value saves; the pointee's copy
// is. Interface receivers are excluded: they are a genuine C# instance method over the interface
// value and never take this path.
func (v *Visitor) receiverExprIsAutoDerefdPointee(selectorExpr *ast.SelectorExpr) bool {
	recvType := v.getType(selectorExpr.X, false)

	if recvType == nil {
		return false
	}

	if _, exprIsPointer := recvType.Underlying().(*types.Pointer); !exprIsPointer {
		return false
	}

	funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func)

	if !ok {
		return false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.Recv() == nil || types.IsInterface(sig.Recv().Type()) {
		return false
	}

	_, recvIsPointer := sig.Recv().Type().(*types.Pointer)

	return !recvIsPointer
}

// hoistReceiverTemp evaluates a method value's receiver expression into a statement-level temp and
// returns the temp's name, or "" when it cannot be declared. `deref` renders the temp's initializer
// as the pointee (`~expr`) rather than the expression itself, which is what an auto-deref'd
// pointer receiver expression saves.
//
// The initializer is RE-RENDERED outside lambda context rather than reusing the caller's string,
// and that is load-bearing, not tidiness. The caller's rendering is produced in-lambda, so a
// captured base reads as the capture machinery's snapshot name (`h6ʗ1.f`) — and that snapshot is
// declared into this SAME hoist buffer, AFTER this temp, because the capture declarations are
// generated later in the emission order. Hoisting the in-lambda string therefore emits
// `var recvʗ1 = h6ʗ1.f;` above `var h6ʗ1 = h6;` — use-before-declaration, CS0841, on every
// captured base. Rendering the ORIGINAL expression in the enclosing context yields `h6.f`, which is
// in scope at the statement position where this declaration lands. Same discipline the ident arms
// already use, and for the same reason.
func (v *Visitor) hoistReceiverTemp(recvExpr ast.Expr, deref bool) string {
	if v.hoistedDecls == nil || v.lambdaCapture == nil {
		return ""
	}

	// Shares getCapturedVarName's per-prefix counter so a receiver temp can never collide with a
	// capture name; the flag it clears is the same one prepareStmtCaptures clears, and for the same
	// reason (the counter does not advance while the detection phase is still set, which would hand
	// out one name twice).
	v.lambdaCapture.detectingCaptures = false
	tempName := v.getCapturedVarName(receiverTempPrefix)

	savedInLambda := v.lambdaCapture.conversionInLambda
	v.lambdaCapture.conversionInLambda = false
	initExpr := v.convExpr(recvExpr, nil)
	v.lambdaCapture.conversionInLambda = savedInLambda

	if initExpr == "" {
		return ""
	}

	if deref {
		initExpr = PointerDerefOp + initExpr
	}

	v.hoistedDecls.WriteString(fmt.Sprintf("%s%svar %s = %s;%s", v.newline, v.indent(v.indentLevel), tempName, initExpr, v.newline))

	return tempName
}
