// convUnaryExpr.go - Gbtc
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
	"math"
	"strconv"
	"strings"
)

// lazyArrayBackingProjection returns the member access that must be appended to a NAMED
// fixed-size array expression before it is handed to golib's BY-VALUE element-aliasing overload
// `Ꮡ<T>(IArray<T> target, int index)` — `".Value"` when one is needed, `""` otherwise.
//
// The two-arg overload takes its target by value, so the CALL SITE boxes the wrapper and golib
// only ever sees that private copy. For golib's own `array<E>` — an eagerly-allocated readonly
// struct over a shared `E[]` — a copy shares the storage and everything aliases correctly, which
// is the reasoning the two `Ꮡ(x, i)` receiver arms below were written on. It does NOT hold for a
// go2cs-gen NAMED fixed-size array wrapper: that wrapper allocates its backing LAZILY
// (`Value => m_value ??= new array<E>(N)`), so over a still-zero wrapper the backing materializes
// on the boxing temp and the caller's storage is never written. Every element pointer taken that
// way names a fresh private array, and every write through it is silently lost — no concurrency
// required, and single-threaded reproducible (ElemAliasProbe arm5).
//
// `.Value` is the fix that needs nothing golib cannot already see: the wrapper's `Value` getter is
// a MUTATING struct member, so invoking it on the ref receiver (or on a field of it) runs the
// `??=` against the REAL storage, and the `array<E>` it hands back SHARES that backing. Both
// wrapper flavors carry it — a direct-array RHS (`type Mont [4]uint64`) yields `Value : array<E>`,
// and a named RHS (`type pallocBits pageBits`) yields the view wrapper whose `Value` is that named
// type, itself an `IArray<E>` over the same storage (writtenRHSIsUnnamedArray distinguishes them;
// neither needs distinguishing here, because both materialize in place and both bind the overload).
//
// The `.at<E>(i)` route — which publishes through the box and would also be correct — is NOT
// available at these two sites: a Go pointer receiver renders as `this ref T recv`, which has no
// `ж<>` box companion (see convSelectorExpr.go's direct-ж note), and that is exactly why these
// arms fall back to the two-arg overload in the first place.
//
// An UNNAMED `[N]E` base renders as golib `array<E>`, which has no `Value` member and needs none;
// it returns "" and the emission is unchanged.
func (v *Visitor) lazyArrayBackingProjection(baseType types.Type) string {
	if baseType == nil {
		return ""
	}

	named, isNamed := types.Unalias(baseType).(*types.Named)

	if !isNamed {
		return ""
	}

	if _, isArray := named.Underlying().(*types.Array); !isArray {
		return ""
	}

	return ".Value"
}

// convArrayIndex emits an array/slice index expression for the golib `ж.at<T>(nint)`
// element-address accessor. Go permits any integer type as an array/slice index and
// converts it to `int` for the access; the `at` accessor takes `nint`, but C# has no
// implicit nuint/uint/ulong→nint conversion, so a non-`int` index (e.g. a `uintptr`
// loop var, or a `uint % 2` whose C# result type widens to `long`) must be narrowed
// explicitly. An `int` index, or an untyped int constant (which renders as a plain int
// literal), needs no cast. Mirrors Go's index-to-int conversion in the emitted C#.
func (v *Visitor) convArrayIndex(index ast.Expr) string {
	expr := v.convExpr(index, nil)

	if basic, ok := v.getType(index, true).(*types.Basic); ok {
		info := basic.Info()

		if info&types.IsInteger != 0 && info&types.IsUntyped == 0 && basic.Kind() != types.Int {
			return fmt.Sprintf("(nint)(%s)", expr)
		}
	}

	return expr
}

// boxAccessorType qualifies the struct type name in a box-field accessor (`Type.Ꮡfield`, used by
// `receiver.of(Type.Ꮡfield)`) with its package class ONLY when it would otherwise be shadowed by
// the `.of()` receiver variable of the same name — pervasive in runtime, where a local is routinely
// named after its own type: `m := getg().m; &m.park` → `m.of(runtime_package.m.Ꮡpark)`; a bare
// `m.Ꮡpark` binds to the variable `m` (a `ж<m>`), which has no `Ꮡpark` (CS1061). A converted struct
// is nested in its package's static class, so the qualifier is that class (`runtime_package.m`) —
// which a same-named local cannot shadow — not a bare `m` nor `go.m` (the struct is not a direct
// member of the `go` namespace). Qualifying only on a collision keeps every other box accessor
// unchanged (no golden churn, Go-like un-namespaced form preserved). A `Ꮡ`-prefixed receiver (`Ꮡx`)
// can never equal a bare type name, so those never qualify; an already-qualified cross-package type
// (contains '.') is returned unchanged.
//
// The receiver is only ONE of the ways the bare name can be captured. C# scopes a local to its whole
// enclosing block, so ANY variable named like the type — however unrelated to this accessor — owns
// the simple name at this point. poly1305's purego `mac` is the general case: `type mac struct{
// macGeneric }`, and `func (h *MAC) Sum(b []byte)` declares `var mac [TagSize]byte`, so the
// promoted-embed hop for `h.mac.Sum(&mac)` spells `Ꮡ(h.mac).of(mac.ᏑmacGeneric)` where `mac` binds to
// the `array<byte>` local (CS1061 ×2 — it errors in `Sum`/`Verify`, which declare that local, and not
// in `Write`, which does not). Go guarantees such a reference is unambiguous — inside the shadowed
// scope the type is not reachable by that name at all, so every bare occurrence the EMITTER produces
// is meant as the type — which makes package-qualifying it always value-correct. funcScopeVarNames
// carries the current function's declared variable names (see performVariableAnalysis).
// owner carries the accessor's declaring type when the call site has it, so the qualifier can name
// the class that actually DECLARES it (see boxAccessorQualifier); a nil owner keeps the production
// class, which is what every non-test conversion resolves to anyway.
func (v *Visitor) boxAccessorType(typeName, receiver string, owner types.Type) string {
	// An already-qualified cross-package type (contains '.') is left alone.
	if strings.Contains(typeName, ".") {
		return typeName
	}

	// A type whose name collides with a same-package method/function (`type funcInfo` vs
	// `func (f *Func) funcInfo()`) is declared Δ-renamed (`ΔfuncInfo`), since the package static
	// method shadows the namespace type. The box accessor must use that renamed name too — a bare
	// `funcInfo.Ꮡnfuncdata` binds to the method group (CS0119). Callers that pass the RAW Go name are
	// normalized here; callers that pass an already-sanitized name arrive Δ-prefixed already.
	if nameCollisions[typeName] {
		typeName = getCollisionAvoidanceIdentifier(typeName)
	}

	// Qualify the bare type name with its package static class (which a local variable can NEVER shadow)
	// in either case:
	//   - The type is COLLISION-renamed (Δ-prefixed): a Go local named after its type (`p`) is renamed
	//     to the SAME `Δp`, so any such local in the enclosing function shadows a bare `Δp.Ꮡfield`. At
	//     runtime malloc's `persistentalloc1`, `persistent = &mp.p.ptr().palloc` →
	//     `(~mp).p.ptr().of(Δp.Ꮡpalloc)` binds `Δp` to the local `Δp` declared just below (CS0841;
	//     CS1061 regardless). The receiver (`(~mp).p.ptr()`) is not the colliding local, so the receiver
	//     check alone misses it. A type is never shadow-renamed (types are package-level), so a
	//     Δ-prefixed accessor type is always a collision type — qualifying is always value-correct.
	//   - The type name equals the `.of()` RECEIVER variable (`m := getg().m; &m.park` →
	//     `m.of(runtime_package.m.Ꮡpark)`) — a bare `m.Ꮡpark` binds to the `ж<m>` variable `m` — or the
	//     receiver is that variable's lambda CAPTURE (`mʗ1`): inside `systemstack(func(){ notesleep(&m.park) })`
	//     the captured receiver renames to `mʗ1`, but the ENCLOSING local `m` is still visible to the
	//     lambda, so a bare `m.Ꮡpark` binds to it all the same (CS1061, runtime rwmutex `lockSlow`) —
	//     or the receiver is that variable's heap BOX (`Ꮡw`, a box-ref capture): the enclosing local
	//     `w` is likewise still visible inside the lambda. When that local's declared type IS the
	//     type named `w`, C#'s identical-simple-name rule happens to bind the type; but a box-ref'd
	//     local whose declared type differs (a pointer-typed `w := &w{…}`, declared `ж<w>`) has no
	//     such fallback — the bare name binds the variable (CS1061) — so the box-receiver render
	//     qualifies uniformly. The box-DEREF receiver (`Ꮡw.ValueSlot`, a heap-boxed pointer local
	//     read through its box inside a lambda) is the same case: the enclosing `ж<w>`-declared
	//     local `w` is visible and has no identical-simple-name fallback, so a bare `w.Ꮡpark`
	//     binds the uncapturable ref-local (CS8175/CS1061). The trailing `.` anchors the name
	//     boundary (`Ꮡmatch.Value` does not qualify type `m`).
	//   - A variable of that name is declared ANYWHERE in the enclosing function (the general form of
	//     the receiver cases above — the shadowing local need not participate in this accessor at all).
	if strings.HasPrefix(typeName, ShadowVarMarker) || typeName == receiver ||
		strings.HasPrefix(receiver, typeName+CapturedVarMarker) ||
		receiver == AddressPrefix+typeName ||
		strings.HasPrefix(receiver, AddressPrefix+typeName+".") ||
		v.funcScopeVarNames.Contains(typeName) {
		return v.boxAccessorQualifier(owner) + "." + typeName
	}

	return typeName
}

// boxAccessorQualifier names the static class that DECLARES the box accessor's owner type.
//
// Normally that is the production `<pkg>_package`. Under the white-box test model the emission unit
// is the bridge class instead, and a type contributed by an internal `_test.go` is nested THERE, not
// in the production class — so the production qualifier names nothing (CS0117). database/sql's
// `fakedb_test.go` declares `type table struct { mu sync.Mutex; … }` beside a colliding
// `func (db *fakeDB) table(string)`, which Δ-renames the type and therefore forces the qualification
// above on every `t.mu.Lock()` in the file: six sites spelled `sql_package.Δtable.Ꮡmu`, a class that
// has no such member. packageScopeClassName already draws exactly this production/bridge line for
// package-level VALUE references; a box accessor's owner type is the same question about a type.
//
// A nil or non-named owner (a lifted anonymous struct, whose name the call site resolves by other
// means) keeps the production class — unchanged behavior for every site that cannot name an object.
func (v *Visitor) boxAccessorQualifier(owner types.Type) string {
	if owner != nil {
		if pointer, ok := owner.(*types.Pointer); ok {
			owner = pointer.Elem()
		}

		if named, ok := owner.(*types.Named); ok && named.Obj() != nil {
			return v.packageScopeClassName(named.Obj())
		}
	}

	return getSanitizedImport(packageName + PackageSuffix)
}

// isHeapBoxedExpr reports whether the expression refers to a variable that the
// converter has given a heap-boxed pointer companion (the "Ꮡname" form) — i.e. it
// escapes to the heap and is not an inherently heap-allocated type. Package-level
// value vars and non-escaping locals return false (their address must be taken via
// the Ꮡ(value) constructor form). Mirrors the escape check in convUnaryExpr.
func (v *Visitor) isHeapBoxedExpr(expr ast.Expr) bool {
	ident := getIdentifier(expr)

	if ident == nil {
		return false
	}

	// A package-level var whose address is taken is backed by a heap box (the
	// "Ꮡname" companion), same as an escaping local.
	if v.isAddressedGlobal(ident) {
		return true
	}

	obj := v.info.Defs[ident]

	if obj == nil {
		obj = v.info.Uses[ident]
	}

	if obj == nil {
		return false
	}

	return v.identHasHeapBox(obj, v.getIdentType(ident))
}

// boxBaseName returns the base name (without the `Ꮡ` prefix) of an address-taken var's heap box.
// The box name tracks how the value alias was renamed, and the two rename kinds use DIFFERENT box
// names:
//   - A type-COLLISION rename prepends the marker (`p` colliding with type `p` → `Δp`), but its box
//     keeps the RAW Go name — `ref var Δp = ref heap(new T(), out var Ꮡp)`. So a `&p` reference must
//     use the raw `Ꮡp`; `Ꮡ`+the alias (`ᏑΔp`) is not in scope (CS0103).
//   - A nested-scope SHADOW rename appends the marker + a counter (`i` → `iΔ1`, `iΔ2`), and its box
//     keeps the SHADOW name — `ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1)`. So `&i` correctly uses
//     `ᏑiΔ1` (the alias), and must be left unchanged.
//
// Only the collision form (the alias is exactly `Δ`+rawname) is rewritten to the raw name; every
// other case (no rename, shadow rename, `@`-keyword escape) keeps the existing alias-derived box
// name — no churn.
func (v *Visitor) boxBaseName(ident *ast.Ident) string {
	base := strings.TrimPrefix(getSanitizedIdentifier(v.getIdentName(ident)), "@")

	// A deref-aliased pointer PARAMETER keeps its box under the RAW parameter name, whether the value
	// alias was collision-renamed (`Δp`) or SHADOW-renamed (`randΔ1`): the deref is
	// `ref var randΔ1 = ref Ꮡrand.Value` — the box `Ꮡrand` is named for the raw parameter, not the
	// value alias. This is UNLIKE an escaping LOCAL, whose shadow box keeps the RENDERED name
	// (`ᏑiΔ1`, handled by the alias-derived `base` below). So a shadow-renamed pointer param passed
	// as a pointer must reference `Ꮡrand`, not `ᏑrandΔ1` — testing/quick's `rand` (shadowing the
	// `rand` package) and crypto/rsa (CS0103, ~50 sites). The collision case already resolved to the
	// raw name via the `base` check below; this generalizes it to the shadow-rename case for params.
	if v.identIsParameter(ident) {
		if _, isPointer := v.getIdentType(ident).(*types.Pointer); isPointer {
			return ident.Name
		}
	}

	if base == ShadowVarMarker+ident.Name {
		return ident.Name
	}

	return base
}

// lambdaBoxRefAddressForm renders `&m` / `&m.field` when `m` is a heap-boxed local captured by-box
// inside the lambda currently being converted (see LambdaCapture.boxRefVars). The address is taken
// through the box `Ꮡm` — a capturable reference — rather than the uncapturable ref-local alias.
// Returns ("", false) when the operand is not rooted at a box-ref var, so the caller falls through
// to the normal address handling.
// receiverValueFieldChain unwraps a `&recv.f1.f2...fn` selector chain rooted at the current pointer
// receiver, returning the hops OUTERMOST-LAST so emission can fold `.of(...)` left to right.
//
// The one-hop form `&recv.field` is the common case and was the only one the converter recognised
// until 2026-08-23. A DEEPER chain matched no arm and fell through to the `Ꮡ(value)` copy-box,
// which boxes a COPY -- golib's `Ꮡ(in T)` ctor is documented as doing exactly that -- so every
// write through the returned pointer was silently dropped. Measured in
// `vendor/golang.org/x/net/dns/dnsmessage`'s `incrementSectionCount`, whose four
// `count = &b.header.<section>` sites made `*count++` increment a temporary: the DNS header's
// QDCOUNT stayed 0, so every message the Builder produced was rejected by any conformant parser and
// the failure surfaced three levels away as an unexplained lookup timeout. Corpus census at the fix:
// 4 write-context sites, all in that one function.
//
// EVERY INTERMEDIATE HOP MUST BE A VALUE STRUCT FIELD. A pointer-typed intermediate is already its
// own box and the pointer-variable arm field-refs through it correctly; routing it here would
// address the pointer's own storage instead of the pointee's field -- the mirror-image defect. The
// receiver is exempt from that test: it is a pointer by construction and the direct-ж box IS it.
func (v *Visitor) receiverValueFieldChain(selectorExpr *ast.SelectorExpr, recvName string) ([]*ast.SelectorExpr, bool) {
	hops := []*ast.SelectorExpr{selectorExpr}
	current := selectorExpr

	for {
		switch base := current.X.(type) {
		case *ast.Ident:
			// Receiver match is by OBJECT identity: a pointer LOCAL shadowing the receiver name must
			// not take this arm -- it would emit `Ꮡ`+raw, a box that does not exist for a
			// non-direct-ж method (CS0103). The rendered==raw check stays as the fallback defense
			// for an unresolvable ident.
			if !v.identResolvesToReceiver(base, recvName) || v.getIdentName(base) != base.Name {
				return nil, false
			}

			// A DEEP chain additionally requires the enclosing method to actually BE direct-ж, and
			// this guard is load-bearing: the box `Ꮡrecv` only exists when the method was marked,
			// and marking happens by scanning for an explicit `&recv.f1.f2` (bodyTakesReceiverFieldAddress).
			// An IMPLICIT address is invisible to that scan -- Go's `h.mac.Sum(&mac)`, where `Sum` is
			// promoted from an embedded field, takes `&h.mac.macGeneric` with no ast.UnaryExpr
			// anywhere -- so emitting the box form there names a receiver that does not exist
			// (CS0103, measured in vendor/golang.org/x/crypto/internal/poly1305). Declining leaves
			// those sites on their previous value-chain form, which is already correct for them: the
			// receiver is a `ref`, so `h.mac.macGeneric` reaches the real storage and writes land.
			//
			// The one-hop case deliberately skips this test, exactly as it always has: an explicit
			// `&recv.field` is always seen by the marking scan, so the box is always there.
			if len(hops) > 1 && !isDirectBoxReceiverMethod(v.currentFuncDecl, v.info) {
				return nil, false
			}

			return hops, true
		case *ast.SelectorExpr:
			selection, ok := v.info.Selections[base]

			if !ok || selection.Kind() != types.FieldVal {
				return nil, false
			}

			if _, isStruct := v.getType(base, true).(*types.Struct); !isStruct {
				return nil, false
			}

			hops = append([]*ast.SelectorExpr{base}, hops...)
			current = base
		default:
			return nil, false
		}
	}
}

// receiverFieldChainAddressForm renders receiverValueFieldChain's hops as
// `Ꮡrecv.of(T1.Ꮡf1).of(T2.Ꮡf2)...` -- the same `.of()` chaining the corpus already uses at
// hundreds of pointer-rooted sites, here rooted at the direct-ж receiver box. Declines when a hop's
// owner is not a named type, so a site never loses its previous emission to a half-built chain.
func (v *Visitor) receiverFieldChainAddressForm(recv *types.Var, recvName string, hops []*ast.SelectorExpr) (string, bool) {
	form := AddressPrefix + recvName

	for i, hop := range hops {
		var ownerType types.Type

		if i == 0 {
			ownerType = recv.Type()
		} else {
			ownerType = v.info.TypeOf(hop.X)
		}

		if pointer, ok := ownerType.(*types.Pointer); ok {
			ownerType = pointer.Elem()
		}

		if _, ok := ownerType.(*types.Named); !ok {
			return "", false
		}

		fieldRef := fmt.Sprintf("%s.%s%s", v.boxAccessorType(convertToCSTypeName(v.getAliasQualifiedTypeName(ownerType, false)), "", ownerType), AddressPrefix, v.structFieldBoxName(hop.Sel, hop.X))
		form = fmt.Sprintf("%s.of(%s)", form, fieldRef)
	}

	return form, true
}

func (v *Visitor) lambdaBoxRefAddressForm(unaryExpr *ast.UnaryExpr) (string, bool) {
	if v.lambdaCapture == nil || !v.lambdaCapture.conversionInLambda {
		return "", false
	}

	switch operand := unaryExpr.X.(type) {
	case *ast.Ident:
		// &m → Ꮡm
		if v.isLambdaBoxRefVar(v.info.ObjectOf(operand)) {
			return AddressPrefix + v.boxBaseName(operand), true
		}
	case *ast.SelectorExpr:
		// &m.field → Ꮡm.of(Type.ᏑField). The box `Ꮡm` is the `ж<T>` to field-ref through when `m`
		// is a value struct (`Ꮡm` boxes the value) or a deref'd pointer PARAMETER/RECEIVER (`Ꮡm`
		// *is* the Go pointer). A heap-boxed pointer LOCAL is NOT this form: its box is one level
		// higher (a `ж<ж<T>>` — see isBoxedPointerLocal), and the `ж<T>` to field-ref through is
		// the HELD pointer, so it declines below and falls through to the pointer-variable field
		// arm, whose convIdent base render reads the box (`Ꮡc.ValueSlot.of(…)`) — the bare-box
		// form fed the `ж<ж<T>>` to `.of` (CS0411, runtime allocmcache's
		// `c.flushGen.Store(…)` inside systemstack).
		baseIdent, ok := operand.X.(*ast.Ident)

		if !ok || !v.isLambdaBoxRefVar(v.info.ObjectOf(baseIdent)) {
			return "", false
		}

		var typeName string
		var ownerType types.Type

		switch t := v.getType(operand.X, true).(type) {
		case *types.Struct:
			typeName = v.dynamicStructTypeName(operand.X)
		case *types.Pointer:
			if _, ok := t.Elem().Underlying().(*types.Struct); !ok {
				return "", false
			}

			if v.isBoxedPointerLocal(baseIdent) {
				return "", false
			}

			typeName = convertToCSTypeName(v.getAliasQualifiedTypeName(t.Elem(), false))
			ownerType = t.Elem()
		default:
			return "", false
		}

		boxName := v.boxBaseName(baseIdent)
		fieldRef := fmt.Sprintf("%s.%s%s", v.boxAccessorType(typeName, "", ownerType), AddressPrefix, v.structFieldBoxName(operand.Sel, operand.X))

		return fmt.Sprintf("%s%s.of(%s)", AddressPrefix, boxName, fieldRef), true
	}

	return "", false
}

func (v *Visitor) convUnaryExpr(unaryExpr *ast.UnaryExpr, context UnaryExprContext) string {
	core := v.convUnaryExprCore(unaryExpr, context)

	// A TYPED-integer constant UNARY expression whose operand fold has widened the emitted
	// arithmetic to `long` carries its own narrowing cast — the exact counterpart of the binary
	// root's treatment in convBinaryExpr, and the ONLY place the cast can go: go/types types just
	// the unary node and leaves its constant operand untyped (see widenedConstExprCastType).
	if castType := v.widenedConstExprCastType(unaryExpr); castType != "" && !wholeExprIsCastOfType(core, castType) {
		return "(" + castType + ")(" + core + ")"
	}

	return core
}

func (v *Visitor) convUnaryExprCore(unaryExpr *ast.UnaryExpr, context UnaryExprContext) string {
	// Check if the unary expression is a pointer dereference
	if unaryExpr.Op == token.AND {
		// Inside a lambda, a captured heap-boxed local is referenced through its box (the ref-local
		// alias `ref var m = ref Ꮡm.Value` can't be captured — CS8175). Build address forms from the
		// box name directly, bypassing the value-rewrite that renders the box as `Ꮡm.Value`.
		if boxForm, ok := v.lambdaBoxRefAddressForm(unaryExpr); ok {
			return boxForm
		}

		// Since pointer-based receiver functions are converted to C# as ref-based
		// extension functions, we handle these cases separately
		var recv *types.Var
		var recvName string
		var refRecv bool
		var isRecvPointer bool

		if v.currentFuncSignature != nil {
			recv = v.currentFuncSignature.Recv()

			// Check if current function is a receiver function
			if recv != nil {
				// Get the receiver type
				recvType := v.currentFuncSignature.Recv().Type()

				// Check if receiver is a pointer type
				if _, ok := recvType.(*types.Pointer); ok {
					isRecvPointer = true
					recvName = recv.Name()
				}
			}
		}

		// Address of a field of the pointer receiver: &x.field. The receiver is a
		// pointer, so the selector target's type is a pointer (not a struct) and the
		// struct-field handling below would miss it. Taking `Ꮡ(x.field)` would box a
		// copy (the ж(in T) ctor copies), so writes through the pointer would be lost
		// — e.g. sync/atomic's `func (x *Int32) Store(v) { StoreInt32(&x.v, v) }`.
		// Use the field-ref form so the pointer aliases the real field. The method is emitted
		// with the box AS its receiver (`this ж<T> Ꮡx`, see packageDirectBoxReceiverMethods),
		// so we field-ref through that parameter box (direct-ж). This replaces the older static
		// ThreadLocal capture, which is a shared static reassigned per call and races across
		// threads for distinct receivers — broken for concurrent types like sync/atomic.
		if isRecvPointer {
			if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
				// The receiver match is by OBJECT identity (identResolvesToReceiver): a pointer
				// LOCAL shadowing the receiver name must not take this arm — it would emit `Ꮡ`+raw,
				// a box that does not exist for a non-direct-ж method (CS0103). The shadow falls
				// through to the pointer-variable arm below, which field-refs through the local
				// itself (`cΔ1.of(…)`). The rendered==raw check stays as the fallback defense for
				// an unresolvable ident (the shadow pass Δ-renames an inner same-named binding,
				// while the receiver always renders under its raw name).
				// The chain may be DEEPER than one hop -- `&b.header.questions`, where `header` is a
				// VALUE struct field of the receiver. Each hop gets its own `.of(...)`; a single-hop
				// chain reproduces the original form byte for byte, so no existing site moves.
				if hops, ok := v.receiverValueFieldChain(selectorExpr, recvName); ok {
					if form, ok := v.receiverFieldChainAddressForm(recv, recvName, hops); ok {
						// Direct-ж: the receiver box is the parameter `Ꮡx` (see
						// packageDirectBoxReceiverMethods), so field-ref through it directly. No
						// static ThreadLocal -- that form races across threads for distinct
						// receivers (broken for concurrent types like sync/atomic).
						return form
					}
				}

			}
		}

		// Address of a field of a pointer *variable* that is not the receiver: `&e.v` where `e` is
		// a `*entry` identifier, OR `&o.h.wait` where `o.h` is a pointer-typed FIELD, OR a pointer
		// RVALUE — a pointer-returning CALL (`&getg().schedlink`) or a pointer ELEMENT index
		// (`&batch[i].schedlink`). The base is already a box `ж<T>`, so field-ref through it directly —
		// `e.of(Type.ᏑField)` / `getg().of(Type.ᏑField)` — without the `Ꮡ(value)` copy form (which boxes
		// a COPY, so writes are lost — critical for an atomic field reached through a pointer chain,
		// where the lost write silently corrupts behavior). A more complex pointer expression (a deref
		// of an `unsafe.Pointer` cast, etc.) is none of these, so it keeps the old form. The receiver
		// case is handled above and a value-struct field falls through to the struct branch below.
		if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
			base := selectorExpr.X
			_, baseIsIdent := base.(*ast.Ident)
			_, baseIsSelector := base.(*ast.SelectorExpr)
			_, baseIsIndex := base.(*ast.IndexExpr)

			// A genuine pointer-returning CALL (`getg()`) is a postfix expression `.of(…)` chains off
			// cleanly; a type-CONVERSION CallExpr (`(*T)(p)`) renders as a C# cast (low precedence) on
			// which `.of(…)` would mis-bind, so it keeps the `Ꮡ(value)` form (S1 pointer-reinterpret).
			baseIsCall := false
			if call, ok := base.(*ast.CallExpr); ok {
				baseIsCall = !v.callExprIsTypeConversion(call)
			}

			// A TYPE-ASSERTION base — `&c.(*UDPConn).conn` (net's udpsock_test, reaching the
			// promoted `conn.Write`): the assert renders as the postfix `c._<ж<UDPConn>>()`, which
			// IS the box, so `.of(…)` chains off it exactly as off a pointer-returning call.
			// Without it the `Ꮡ(value)` fallback named a `.conn` member `ж<UDPConn>` does not have
			// (CS1061) — and, had it bound, would have boxed a COPY.
			_, baseIsTypeAssert := base.(*ast.TypeAssertExpr)

			// A DEREF base — `&(*pprev).alllink` where pprev is `**m` (runtime proc.go's allm
			// walk): the star of a double pointer renders `pprev.Value`, itself the ж<m> box, and
			// field-refs through `.of(…)` cleanly (postfix on postfix).
			baseIsStar := false
			{
				unwrapped := base

				for {
					if paren, ok := unwrapped.(*ast.ParenExpr); ok {
						unwrapped = paren.X
						continue
					}

					break
				}

				_, baseIsStar = unwrapped.(*ast.StarExpr)
			}

			if baseIsIdent || baseIsSelector || baseIsCall || baseIsIndex || baseIsStar || baseIsTypeAssert {
				if ptrType, ok := v.getType(base, false).(*types.Pointer); ok {
					if _, ok := ptrType.Elem().Underlying().(*types.Struct); ok {
						structExpr := v.convExpr(base, nil)

						// A pointer *parameter* is deref'd to a value alias (`ref var s = ref Ꮡs.Value`),
						// so its box is `Ꮡs` — field-ref through the box. A pointer *local* or a
						// pointer *field* already holds/yields the box directly, so it is used as-is.
						// The box keeps the RAW parameter name: a collision/shadow-renamed param `p`→`Δp`
						// is `ref var Δp = ref Ꮡp.Value`, so its box is `Ꮡp`, not `ᏑΔp` (CS0103). boxBaseName
						// yields the raw name when shadow-renamed and the sanitized name otherwise (no churn).
						if baseIdent, ok := base.(*ast.Ident); ok && v.identIsParameter(baseIdent) {
							structExpr = AddressPrefix + v.boxBaseName(baseIdent)
						}

						typeName := convertToCSTypeName(v.getAliasQualifiedTypeName(ptrType.Elem(), false))
						fieldRef := fmt.Sprintf("%s.%s%s", v.boxAccessorType(typeName, structExpr, ptrType.Elem()), AddressPrefix, v.structFieldBoxName(selectorExpr.Sel, selectorExpr.X))
						return fmt.Sprintf("%s.of(%s)", structExpr, fieldRef)
					}
				}
			}
		}

		// Check if the unary expression is an address of a structure field
		if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
			if _, ok := v.getType(selectorExpr.X, true).(*types.Struct); ok {
				if isRecvPointer {
					// Check if selector's target is the receiver — by OBJECT identity: a pointer
					// receiver's ident types as *T (never a bare struct), so a name match here could
					// only ever be a value-struct LOCAL shadowing the receiver name, which must keep
					// the heap-box `.of(…)` routing below, not the copy-boxing receiver form
					// (identResolvesToReceiver).
					if ident, ok := selectorExpr.X.(*ast.Ident); ok && v.identResolvesToReceiver(ident, recvName) {
						refRecv = true
					}
				}

				if refRecv {
					// For a receiver reference to a structure field, we use the "Ꮡ(StructType.Field)" syntax
					return fmt.Sprintf("%s(%s.%s)", AddressPrefix, v.convExpr(selectorExpr.X, nil), removeLeadingSanitizationMarker(v.convExpr(selectorExpr.Sel, nil)))
				} else {
					// For a structure field, we use the "ж.of(StructType.ᏑField)" syntax.
					// dynamicStructTypeName resolves anonymous struct types lifted in
					// another file of the package (e.g. `&cpu.X86.HasADX`).
					structExpr := v.convExpr(selectorExpr.X, nil)
					typeName := v.dynamicStructTypeName(selectorExpr.X)
					fieldRef := fmt.Sprintf("%s.%s%s", v.boxAccessorType(typeName, "", nil), AddressPrefix, v.structFieldBoxName(selectorExpr.Sel, selectorExpr.X))

					// When the base is itself a VALUE field reached through a pointer field —
					// `&gp.m.mLockProfile.waitTime`, base `gp.m.mLockProfile` a value field of the
					// pointer `gp.m` — recurse to take the base's address through the pointer
					// (`gp.m.of(mType.ᏑmLockProfile)`), then field-ref this level: `.of(…ᏑwaitTime)`.
					// The same applies when the base is a value field-chain rooted at a deref-aliased
					// pointer PARAMETER/RECEIVER — `&Δp.scav.index`, base `Δp.scav` rooted at the `*pageAlloc`
					// receiver — recurse through its box `Ꮡp.of(pageAlloc.Ꮡscav)`, then `.of(…Ꮡindex)`.
					// In both cases the `Ꮡ(value)` fallback would copy the chain, losing writes (atomic
					// corruption). (The single-field root `&Δp.scav` is handled directly by the receiver /
					// param-deref branches above; only the multi-level chain needs this recursion.)
					if v.exprIsValueFieldOfPointer(selectorExpr.X) || v.exprIsValueFieldOfDerefdPointerRoot(selectorExpr.X) {
						baseAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
						return fmt.Sprintf("%s.of(%s)", baseAddr, fieldRef)
					}

					// When the base is a slice/array ELEMENT — `&p.Inst[pc].Out` (regexp's
					// onePassCopy) — its address is the element-ALIASING form the index branch
					// below renders (`Ꮡ(s, i)` for a slice, `.at<E>(i)` for an array/pointer-to-
					// array), so recurse for it and field-ref this level. The `Ꮡ(value)` fallback
					// at the end of this arm boxes a COPY of the ELEMENT, and every write through
					// the returned pointer is silently dropped — the same class the slice, array
					// and pointer-to-array branches below already call out by name (tabwriter's
					// empty lines, flate's literals-only levels 2-9, crc32's all-zero tables),
					// reached here through a promoted or ordinary FIELD of the element instead of
					// through the element itself.
					//
					// This was latent while go2cs-gen held a promoted embed in a shared `ж<T>` box:
					// the embed's reference semantics meant a copied element still pointed at the
					// origin's embedded storage, so a write through `Ꮡ(elem).of(T.ᏑPromoted)` reached
					// the real element by accident. Making an embed an INLINE field (1ae49db8a) was
					// correct and removed that accident, which is what surfaced this — regexp's
					// `p_A_Other := &p.Inst[pc].Out` stopped patching the program and
					// TestCompileOnePass lost two one-pass rewrites. That commit fixed the sibling
					// arm (a promoted pointer-receiver CALL descending a copy box); this is the
					// address-of-FIELD arm of the same defect.
					//
					// Ordered BEFORE the heap-boxed branch, which recurses identically for an
					// IndexExpr base — a boxed base reaches the same emission either way, so no
					// existing site moves.
					if indexExpr, ok := selectorExpr.X.(*ast.IndexExpr); ok && v.exprIsIndexableElement(indexExpr) {
						baseAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
						return fmt.Sprintf("%s.of(%s)", baseAddr, fieldRef)
					}

					if v.isHeapBoxedExpr(selectorExpr.X) {
						// When the base is itself a nested field selector or an array/slice index —
						// `&work.sweepWaiters.lock` (field of a field) or `&stackpool[i].item.mu`
						// (field of an indexed element) of a boxed global — recurse to build the base's
						// address through `.of(...)` / `.at<T>(i)` chaining, e.g.
						// `Ꮡwork.of(workType.ᏑsweepWaiters).of(…Ꮡlock)` /
						// `Ꮡstackpool.at<T>(i).of(…Ꮡmu)`. Prefixing `Ꮡ` onto `work.sweepWaiters` /
						// `stackpool[i]` instead binds to the box variable (`Ꮡwork` / `Ꮡstackpool`,
						// whose value has no such member / no indexer) → CS1061 / CS0021.
						switch selectorExpr.X.(type) {
						case *ast.SelectorExpr, *ast.IndexExpr:
							baseAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())
							return fmt.Sprintf("%s.of(%s)", baseAddr, fieldRef)
						}

						// The operand already has a heap-boxed pointer companion (e.g. an
						// escaping local `Ꮡs`): use the identifier form "Ꮡs.of(...)". A LOCAL's
						// box keeps the RAW identifier name — a collision-renamed local (`slice`
						// → `Δslice`, reflect SliceOf) declares `ref heap<T>(out var Ꮡslice)`,
						// so the companion is `Ꮡslice`, not `ᏑΔslice` (CS0103 ×2). An addressed
						// GLOBAL is the OPPOSITE: its static box companion is declared with the
						// renamed identifier (runtime's `var sweep sweepdata` Δ-renames to
						// `Δsweep` and boxes as `ᏑΔsweep`), so a global keeps the rendered name.
						boxExpr := structExpr

						if baseIdent, ok := selectorExpr.X.(*ast.Ident); ok {
							if obj := v.info.ObjectOf(baseIdent); obj != nil && (obj.Pkg() == nil || obj.Parent() != obj.Pkg().Scope()) {
								boxExpr = v.boxBaseName(baseIdent)
							}
						}

						return fmt.Sprintf("%s%s.of(%s)", AddressPrefix, boxExpr, fieldRef)
					}

					// The operand is an addressable value with no pointer companion (e.g.
					// a package-global struct var): construct the pointer on the fly with
					// the "Ꮡ(value).of(...)" call form (mirrors the whole-value case below).
					return fmt.Sprintf("%s(%s).of(%s)", AddressPrefix, structExpr, fieldRef)
				}
			}
		}

		// Check if the unary expression is an address of an indexed array or slice
		if indexExpr, ok := unaryExpr.X.(*ast.IndexExpr); ok {
			exprType := v.getType(indexExpr.X, true)
			ident := getIdentifier(indexExpr.X)

			// `&t[i]` where t is a POINTER to an ARRAY (Go auto-derefs the index — `(*t)[i]`): the
			// element lives in the pointed-to array on the heap. `t` already IS the `ж<[N]E>` box,
			// so alias the element THROUGH THE BOX with `t.at<E>(i)` — ж's `at` materializes the
			// lazy array backing on the REAL storage first, then returns an element pointer over the
			// shared backing. The naive fallback further below boxes `Ꮡ(t.Value[i])` — a COPY of the
			// element — silently dropping every write through the returned pointer (hash/crc32
			// slicingMakeTable's `simplePopulateTable(poly, &t[0])` left the CRC tables all-zeros —
			// TestGolden/TestSlicing). A pointer-to-SLICE cannot reach here (Go does not auto-deref
			// `*[]E` for indexing; it is written `(*t)[i]`, a StarExpr the slice branch handles).
			if ptr, isPtr := exprType.(*types.Pointer); isPtr {
				if arrayType, isArray := ptr.Elem().Underlying().(*types.Array); isArray {
					// The pointer RECEIVER is the one base with NO box to go through: a Go pointer
					// receiver renders as `this ref T recv`, so `recv.at<E>(i)` names a member the
					// value does not have (CS1061). It needs no box — a named fixed-array type is
					// generated as `IArray<E>` over a shared backing `E[]`, so the two-arg
					// element-aliasing overload aliases correctly on the wrapper itself. Exactly the
					// treatment the receiver's array FIELD already gets in the array branch below,
					// for the same reason. (A deref-aliased pointer PARAMETER and a box-valued LOCAL
					// both DO have a box and keep the `.at<E>(i)` form.)
					//
					// Reached only since `&recv[i].field` began recursing here for its base — before
					// that it took the struct-field arm's `Ꮡ(value)` fallback, a box over a COPY of
					// the element, which is what made runtime-shaped `semtable.rootFor` (`&t[i].root`)
					// hand every caller a pointer into a throwaway copy.
					//
					// The base must be the receiver IDENTIFIER ITSELF, not merely rooted at it — the
					// same object-identity-vs-root-identifier rule the slice and array branches below
					// state, and for the same reason inverted: `getIdentifier` walks a selector chain
					// to its root, so `&p.chunks[l1][l2]` (runtime's pageAlloc.chunkOf) and
					// `&u.inlTree[uf.index]` (symtabinl) report the receiver as their root while their
					// actual base — a pointer-to-array FIELD — is a genuine `ж<[N]E>` rvalue that DOES
					// have a box and must keep `.at<E>(i)`. Boxing those through the two-arg overload
					// hands it a `ж<array<E>>` where it wants an `IArray<E>`, which does not bind.
					baseIsRecvIdent := false
					if baseIdent, isIdent := indexExpr.X.(*ast.Ident); isIdent {
						baseIsRecvIdent = isRecvPointer && v.identResolvesToReceiver(baseIdent, recvName)
					}

					if baseIsRecvIdent {
						// ...but a NAMED array wrapper's backing is LAZY, so the by-value overload
						// would materialize it on the call site's boxing temp and leave the receiver's
						// storage virgin — `semtable.rootFor` handed every caller a pointer into a
						// throwaway 251-entry array, so every semaphore-queue mutation through it was
						// lost. `.Value` runs the wrapper's own mutating getter against the REAL
						// storage first (see lazyArrayBackingProjection).
						return fmt.Sprintf("%s(%s%s, %s)", AddressPrefix, v.convExpr(indexExpr.X, nil), v.lazyArrayBackingProjection(ptr.Elem()), v.castWideIntegerToInt(indexExpr.Index))
					}

					elemCSType := convertToCSTypeName(v.getScopeCheckedTypeName(arrayType.Elem()))
					// Render the base in POINTER context so it yields the `ж<[N]E>` BOX (`Ꮡtab` for a
					// deref-aliased pointer PARAMETER, or the box-valued LOCAL `t` from `@new`), not the
					// deref value alias `tab` (a bare `[N]E` value has no `at`, CS1061).
					boxIdentContext := DefaultIdentContext()
					boxIdentContext.isPointer = true

					return fmt.Sprintf("%s.at<%s>(%s)", v.parenthesizeConversionBase(indexExpr.X, v.convExpr(indexExpr.X, []ExprContext{boxIdentContext})), elemCSType, v.convArrayIndex(indexExpr.Index))
				}
			}

			// Object identity, not name: a slice/array LOCAL shadowing the receiver name must
			// keep the element-aliasing `Ꮡ(x, index)` form below — the receiver form `Ꮡ(x[i])`
			// boxes a COPY of the element, silently dropping writes (identResolvesToReceiver).
			if ident != nil && isRecvPointer && v.identResolvesToReceiver(ident, recvName) {
				// Index target is an identifier matching the receiver name
				refRecv = true
			}

			// The slice element-address form applies to ANY slice-typed base expression, not just a
			// bare identifier: a method-CALL result (`&b.stk()[0]`, runtime mprof.go — `b` a pointer
			// local, so `getIdentifier` is nil) previously fell through to the ARRAY branch below
			// (a slice's type name also starts with "["), whose naive fallback textually prefixed
			// `Ꮡ` onto the postfix chain — `Ꮡb.stk().at<uintptr>(0)` binds as `(Ꮡb).stk()…`,
			// referencing a box that does not exist (CS0103).
			if _, ok := exprType.Underlying().(*types.Slice); ok {
				// refRecv is computed from the ROOT identifier of the index base (getIdentifier walks
				// selector chains), so a slice FIELD of the receiver — `&b.lines[i]`, whose base
				// `b.lines` roots at the receiver `b` — matches it too. But the receiver copy-form
				// `Ꮡ(b.lines[i])` boxes a COPY of the element via `Ꮡ(in T)` and silently drops writes
				// back through the pointer: tabwriter's terminateCell does `line := &b.lines[len-1];
				// *line = append(*line, c)`, and the append's length write never lands in b.lines, so
				// every line stays empty (empty formatted output). The copy-form is valid ONLY when the
				// receiver is DIRECTLY the slice (`indexExpr.X` is the bare receiver ident); any other
				// slice base — a field, a call result, a local — must use the element-aliasing two-arg
				// `Ꮡ(x, index)` form, which shares the backing array (see ж.ValueSlot's array-index ref).
				_, indexBaseIsRecvIdent := indexExpr.X.(*ast.Ident)

				if refRecv && indexBaseIsRecvIdent {
					// For a receiver reference to a slice, we use the "Ꮡ(slice[index])" syntax
					return fmt.Sprintf("%s(%s[%s])", AddressPrefix, v.convExpr(indexExpr.X, nil), v.convExpr(indexExpr.Index, nil))
				}
				// For address of an indexed reference into slice we use the "Ꮡ(x, index)" syntax.
				// The golib element-address overloads take `int`/`nint`, so an unsigned/wide index
				// (`&pclntable[funcoff]`, funcoff uint32) is cast to int (CS1503 otherwise).
				return fmt.Sprintf("%s(%s, %s)", AddressPrefix, v.convExpr(indexExpr.X, nil), v.castWideIntegerToInt(indexExpr.Index))
			}

			typeName := v.getAliasQualifiedTypeName(exprType, false)

			if strings.HasPrefix(typeName, "[") {
				// Same object-identity rule the slice branch above applies, for the same reason:
				// refRecv is computed from the ROOT identifier, so an array FIELD of the receiver —
				// `&d.hashHead[h]`, rooting at the receiver `d` — matched it too and took the copy-
				// form. `Ꮡ(d.hashHead[h])` binds the one-arg `Ꮡ(in T)` overload, which boxes a COPY
				// of the ELEMENT, so every write through the pointer is silently dropped: flate's
				// deflate() does `hh := &d.hashHead[hash&hashMask]; … *hh = uint32(d.index +
				// d.hashOffset)`, the hash heads stayed all-zero, findMatch was never reached (its
				// `d.chainHead-d.hashOffset >= minIndex` guard fails at 0-1), and levels 2-9 emitted
				// LITERALS ONLY — valid deflate streams ~2.3x larger than Go's (a 256x256 PNG went
				// 36,760 -> 134,644 bytes). The copy-form is valid ONLY when the receiver is DIRECTLY
				// the array (`indexExpr.X` is the bare receiver ident); any other array base must take
				// an element-aliasing form that shares the backing storage. Mirrors the pointer-to-
				// array case at the top of this branch (crc32's all-zero slicing tables) and the slice
				// case (tabwriter's empty lines).
				_, indexBaseIsRecvIdent := indexExpr.X.(*ast.Ident)

				if refRecv {
					if indexBaseIsRecvIdent {
						// For a receiver reference to an array, we use the "Ꮡ(array[index])" syntax
						return fmt.Sprintf("%s(%s[%s])", AddressPrefix, v.convExpr(indexExpr.X, nil), v.convExpr(indexExpr.Index, nil))
					}

					// An array FIELD of the receiver takes the same element-aliasing two-arg form the
					// slice branch uses, NOT the `.of(field)`/`.at<T>(i)` box machinery below: that route
					// fires on `baseIsPointer` (the Go receiver type IS `*T`), but a Go pointer receiver
					// renders as `this ref T recv`, which has NO box companion — so it emitted `Ꮡr.of(…)`
					// for `&r.Ints[reg]` (internal/abi) and every other ref-receiver field, CS0103. The
					// two-arg overload needs no box and aliases correctly regardless: `array<T>` is a
					// readonly struct wrapping an eagerly-allocated `T[]`, so evaluating the field copies
					// only the wrapper and the copy SHARES element storage (same reasoning as the
					// array-PARAMETER case below) — EXCEPT when the field's type is a NAMED array,
					// whose wrapper allocates its backing lazily and would materialize it on that
					// copy. `.Value` materializes in place first; see lazyArrayBackingProjection.
					return fmt.Sprintf("%s(%s%s, %s)", AddressPrefix, v.convExpr(indexExpr.X, nil), v.lazyArrayBackingProjection(v.getType(indexExpr.X, false)), v.castWideIntegerToInt(indexExpr.Index))
				} else {
					// For an indexed reference into an array, we use the "ж.at<T>(index)" syntax.
					// Prefer the readable file-local package alias for the element type
					// (`.at<atomic.Int32>`) when this file imports the element's package, falling back
					// to the FULLY-QUALIFIED (namespace-rooted) form otherwise: `.at<@internal.runtime.
					// atomic_package.Pointer<…>>` resolves inside `namespace go;` without a `using
					// <pkg>` alias, which the calling file may not import (a Go file can index an
					// atomic-typed array field of a struct without ever naming the element type → no
					// `using atomic` → CS0246). getScopeCheckedTypeName makes that choice per cross-package
					// type. A current-package or basic element renders identically (no churn).
					goFullTypeName := v.getScopeCheckedTypeName(exprType)
					csTypeName := convertToCSTypeName(goFullTypeName[strings.Index(goFullTypeName, "]")+1:])

					// An ANONYMOUS-struct element is lifted to a synthesized name keyed by the element
					// EXPRESSION (e.g. `mheap.central[i]` → `mheap_central`), which neither the
					// array-type-name string-parse nor getCSharpTypeName(type) recovers — a complex element
					// (e.g. a `pad [const]byte` field) renders as a raw/malformed `struct{…}` (invalid
					// C# → masking parse error). Resolve it through dynamicStructTypeName(indexExpr),
					// the same per-expression registry the `.of(…)` field path uses.
					if arrayType, ok := exprType.Underlying().(*types.Array); ok {
						if _, isStruct := arrayType.Elem().Underlying().(*types.Struct); isStruct {
							if _, isNamed := arrayType.Elem().(*types.Named); !isNamed {
								if lifted := v.dynamicStructTypeName(indexExpr); lifted != "" {
									csTypeName = lifted
								}
							}
						}
					}

					// When the array is a FIELD of a heap-boxed value — `&trace.stackTab[i]` where
					// `trace` is an address-taken global — or a field of a POINTER — `&mp.future[i]`
					// where `mp` is a `*memRecord` — its address must go through the box-field accessor,
					// not a naive `Ꮡ` prefix on `mp.future` (which binds to `Ꮡ(~mp)`, whose box value
					// type has no `future` → CS1061). Take the array field's address recursively, which
					// renders `mp.of(memRecord.Ꮡfuture)` / `Ꮡtrace.of(…ᏑstackTab)`, then index it with
					// `.at<T>(i)`.
					if sel, ok := indexExpr.X.(*ast.SelectorExpr); ok {
						baseIsPointer := false

						if baseType := v.info.TypeOf(sel.X); baseType != nil {
							_, baseIsPointer = baseType.Underlying().(*types.Pointer)
						}

						// The base check must see through NESTED value fields to the chain ROOT:
						// `&pp.wbBuf.buf[0]` (runtime mwbbuf.go) roots at the pointer `pp` through the
						// value field `wbBuf` — the one-level check (sel.X = `pp.wbBuf`, a struct) missed
						// it and fell to the naive `Ꮡ` prefix (`Ꮡpp.wbBuf…`, CS1061 on the box). The
						// recursive `&field` machinery below already renders multi-hop of-chains
						// (`pp.of(Δp.ᏑwbBuf).at(wbBuf.Ꮡbuf, 0)`), so route any chain whose root is a
						// pointer (or heap-boxed) through it; intermediate POINTER hops already fired the
						// one-level check on their own segment.
						chainRootIsPointer := false

						if !baseIsPointer {
							root := ast.Expr(sel.X)

							for {
								if inner, ok := root.(*ast.SelectorExpr); ok {
									root = inner.X
									continue
								}

								break
							}

							if rootType := v.info.TypeOf(root); rootType != nil {
								_, chainRootIsPointer = rootType.Underlying().(*types.Pointer)
							}
						}

						if v.isHeapBoxedExpr(sel) || baseIsPointer || chainRootIsPointer {
							arrayAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: indexExpr.X}, DefaultUnaryExprContext())
							index := v.convArrayIndex(indexExpr.Index)

							// Combine the field-address `of(field)` with the element-address into a single
							// `base.at(field, i)` call. The combined golib overload INFERS the element type
							// from the field accessor, dropping the explicit `.at<E>(i)` type argument — so
							// `Ꮡx.of(counters.Ꮡc).at<atomic.Int32>(0)` becomes `Ꮡx.at(counters.Ꮡc, 0)`.
							// arrayAddr is the recursively-rendered field address `base.of(Type.Ꮡfield)`;
							// rewrite its trailing `.of(field)` only when the field segment is parenthesis-
							// free (a plain `Type.Ꮡfield` accessor), so the final `)` provably matches the
							// last `.of(`. Any other shape falls back to the explicit chained form.
							if ofIndex := strings.LastIndex(arrayAddr, ".of("); ofIndex != -1 && strings.HasSuffix(arrayAddr, ")") {
								field := arrayAddr[ofIndex+len(".of(") : len(arrayAddr)-1]

								if !strings.ContainsAny(field, "()") {
									base := arrayAddr[:ofIndex]
									return fmt.Sprintf("%s.at(%s, %s)", base, field, index)
								}
							}

							return fmt.Sprintf("%s.at<%s>(%s)", arrayAddr, csTypeName, index)
						}
					}

					// A NESTED index base — `&cache.entries[ck][i]` (runtime symtab.go), a 2-D array
					// reached through a pointer — is an *ast.IndexExpr*, not a SelectorExpr, so the
					// field-routing above never sees it. When its own element address routes through the
					// box machinery (recursively: `(~cache).entries[ck]` → `cache.at(pcvalueCache.Ꮡentries,
					// ck)`), chain the outer element address onto it the same way; a plain value 2-D index
					// keeps the naive form below (no churn).
					if innerIndex, ok := indexExpr.X.(*ast.IndexExpr); ok {
						innerNeedsBoxRouting := false

						if innerSel, ok := innerIndex.X.(*ast.SelectorExpr); ok {
							root := ast.Expr(innerSel.X)

							for {
								if inner, ok := root.(*ast.SelectorExpr); ok {
									root = inner.X
									continue
								}

								break
							}

							if rootType := v.info.TypeOf(root); rootType != nil {
								_, innerNeedsBoxRouting = rootType.Underlying().(*types.Pointer)
							}

							if !innerNeedsBoxRouting {
								innerNeedsBoxRouting = v.isHeapBoxedExpr(innerSel)
							}
						} else if innerIdent, ok := innerIndex.X.(*ast.Ident); ok {
							// The inner base can also be a bare heap-escaped IDENT rather than a
							// selector — `&ack[me][i%3]` over a `[2][3]int32` local captured by a
							// goroutine literal. Its own element address now routes through the box
							// (`Ꮡack.at<array<int32>>(me)`), so the OUTER element address must chain
							// onto that; without this the pair fell to the naive prefix and emitted
							// `ᏑᏑack.Value[me].at<int32>(…)` — CS0103, the 2-D sibling of the
							// single-index case below.
							if obj := v.info.ObjectOf(innerIdent); obj != nil && v.identEscapesHeap[obj] {
								innerNeedsBoxRouting = true
							}
						}

						if innerNeedsBoxRouting {
							arrayAddr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: indexExpr.X}, DefaultUnaryExprContext())
							return fmt.Sprintf("%s.at<%s>(%s)", arrayAddr, csTypeName, v.convArrayIndex(indexExpr.Index))
						}
					}

					// An ARRAY-typed PARAMETER has no heap box: params are cloned by value in the
					// preamble (`value = value.Clone();`) but never escape-analyzed, so the naive
					// box prefix names a box that does not exist (`Ꮡvalue`, CS0103 — syscall
					// SetsockoptInet4Addr, `&value[0]` on `value [4]byte`). Box a COPY of the
					// wrapper struct instead: `Ꮡ(value).at<byte>(0)` — array<T> wraps a T[]
					// reference, so the copy SHARES element storage with the cloned parameter and
					// element reads/writes through the pointer stay behaviorally correct.
					if ident, ok := indexExpr.X.(*ast.Ident); ok && v.identIsParameter(ident) {
						if obj := v.info.ObjectOf(ident); obj == nil || !v.identEscapesHeap[obj] {
							return fmt.Sprintf("%s(%s).at<%s>(%s)", AddressPrefix, v.convExpr(indexExpr.X, nil), csTypeName, v.convArrayIndex(indexExpr.Index))
						}
					}

					// A heap-escaped array LOCAL owns an identity box — `ref var X = ref
					// heap<array<E>>(out var ᏑX)` — and its DEFAULT render is that box's value alias
					// (`ᏑX.Value`, the form a closure that captured it must use, since C# cannot
					// capture a ref local). Textually prefixing `Ꮡ` onto that composes two box
					// spellings into one name that was never declared: `&X[me]` inside a goroutine
					// literal emitted `ᏑᏑX.Value.at<int32>(me)` — CS0103, the same naive-prefix
					// hazard the slice and pointer-to-array branches above each record. Alias the
					// element THROUGH the box exactly as the pointer-to-array branch does: render
					// the base in POINTER context to get `ᏑX`, then `.at<E>(i)`. ж's `at` reads
					// through `Value` and returns a pointer over the SHARED backing, so writes
					// through it land in the escaped storage (a `Ꮡ(X[i])` copy-form would drop them).
					if ident, ok := indexExpr.X.(*ast.Ident); ok {
						if obj := v.info.ObjectOf(ident); obj != nil && v.identEscapesHeap[obj] {
							boxIdentContext := DefaultIdentContext()
							boxIdentContext.isPointer = true

							return fmt.Sprintf("%s.at<%s>(%s)", v.convExpr(indexExpr.X, []ExprContext{boxIdentContext}), csTypeName, v.convArrayIndex(indexExpr.Index))
						}
					}

					return fmt.Sprintf("%s%s.at<%s>(%s)", AddressPrefix, v.convExpr(indexExpr.X, nil), csTypeName, v.convArrayIndex(indexExpr.Index))
				}
			}
		}

		// Check if unary target is a pointer to a pointer. `Ꮡ(value)` boxes a COPY of the
		// pointer — acceptable only when no identity box exists. An ADDRESSED GLOBAL (or a
		// heap-escaped local) of pointer type has one (`Ꮡallm`, a ж<ж<m>>), and the copy form
		// would silently orphan writes through the double pointer (runtime proc.go's allm walk
		// REMOVES entries via `*pprev = mp.alllink` from `pprev := &allm`) — fall through to
		// the identity-box branches below for those.
		if _, ok := v.getType(unaryExpr.X, true).(*types.Pointer); ok {
			identityBoxed := false

			if ident := getIdentifier(unaryExpr.X); ident != nil {
				if v.isAddressedGlobal(ident) {
					identityBoxed = true
				} else if obj := v.info.ObjectOf(ident); obj != nil && v.identEscapesHeap[obj] {
					identityBoxed = true
				}
			}

			if !identityBoxed {
				return fmt.Sprintf("%s(%s)", AddressPrefix, v.convExpr(unaryExpr.X, nil))
			}
		}

		// A package-QUALIFIED selector naming ANOTHER package's package-level VALUE var — `&pkg.V`,
		// written or synthesized for a pointer-receiver call (`syscall.ForkLock.RLock()`). The copy
		// wrap below (`Ꮡ(pkg.V)`, golib's Ꮡ<T>(in T) = new StandardBox<T>(target)) boxes a COPY, so a
		// lock taken through one fresh box is released through another: measured on darwin as
		// os.Pipe's `fatal error: sync: RUnlock of unlocked RWMutex` (2026-09-03), and hidden on
		// every other platform by the hand-owned mutex's lazily shared state — a copy taken AFTER the
		// real var's first use shares it. Bind the OWNER's box when the owner's emission declares one
		// (importedGlobalIsBoxed: the owner's own predicate over the owner's own syntax), exactly as
		// the same-package path binds `ᏑForkLock`. A var the owner leaves unboxed keeps the render
		// below; the owner arm (markExportedBoxOnlyGlobals) makes that a var whose methods all have
		// a `ref T` primary, for which the box-free call is the in-place one.
		if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok && v.selectorBasePackageObj(selectorExpr) != nil {
			if varObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Var); ok && varObj.Pkg() != nil && varObj.Parent() == varObj.Pkg().Scope() {
				if _, isPtr := types.Unalias(varObj.Type()).(*types.Pointer); !isPtr && importedGlobalIsBoxed(varObj) {
					return fmt.Sprintf("%s.%s%s", v.convExpr(selectorExpr.X, nil), AddressPrefix, strings.TrimPrefix(getSanitizedIdentifier(varObj.Name()), "@"))
				}
			}
		}

		// Check if unary target is not a variable or a field
		if _, ok := unaryExpr.X.(*ast.Ident); !ok {
			// Thread the enclosing statement's hoist target into the operand — a
			// `&composite` with a func-literal FIELD value otherwise dumps its capture
			// decls inline in the ctor argument list (elf file.go, CS1003 ×6).
			var xContexts []ExprContext

			if context.deferredDecls != nil {
				hoistLambdaContext := DefaultLambdaContext()
				hoistLambdaContext.deferredDecls = context.deferredDecls
				xContexts = []ExprContext{hoistLambdaContext}
			}

			return fmt.Sprintf("%s(%s)", AddressPrefix, v.convExpr(unaryExpr.X, xContexts))
		}

		var hasHeapBox bool
		ident := getIdentifier(unaryExpr.X)

		obj := v.info.Defs[ident]

		if obj == nil {
			obj = v.info.Uses[ident]
		}

		if obj != nil {
			hasHeapBox = v.identHasHeapBox(obj, v.getIdentType(ident))
		}

		// A package-level var whose address is taken is backed by a heap box, so its
		// "Ꮡname" companion already exists — reference it directly rather than boxing a copy.
		// Strip a leading '@' keyword-escape: the box name is `Ꮡ`+raw (e.g. `Ꮡbase`), since
		// `Ꮡ@base` is not a valid identifier (a '@' is only valid as a leading prefix).
		if v.isAddressedGlobal(ident) {
			return AddressPrefix + strings.TrimPrefix(v.convExpr(unaryExpr.X, nil), "@")
		}

		if hasHeapBox {
			// If the variable escapes to heap, its `Ꮡname` box already exists. The box keeps the
			// RAW Go name (`ref var Δp = ref heap(new T(), out var Ꮡp)`), so reference it by the raw
			// name — `convExpr` yields the shadow-renamed value alias (`Δp`), giving `ᏑΔp` which is
			// not in scope (CS0103). boxBaseName is a no-op when nothing is shadow-renamed (no churn).
			return AddressPrefix + v.boxBaseName(ident)
		}

		// Otherwise, call the address of function to get a pointer reference
		return fmt.Sprintf("%s(%s)", AddressPrefix, v.convExpr(unaryExpr.X, nil))
	}

	if unaryExpr.Op == token.ARROW {
		// Check if the unary expression is channel receive operation
		if _, ok := v.getType(unaryExpr.X, true).(*types.Chan); ok {
			var tupleResult string

			if v.options.useChannelOperators {
				if context.isTupleResult {
					tupleResult = ", " + OverloadDiscriminator
				}

				// A NAMED channel operand names the element type explicitly — generic
				// inference cannot see through the wrapper (see namedChanElemTypeArg).
				return fmt.Sprintf("%s%s(%s%s)", ChannelLeftOp, v.namedChanElemTypeArg(unaryExpr.X), v.convExpr(unaryExpr.X, nil), tupleResult)
			}

			if context.isTupleResult {
				tupleResult = OverloadDiscriminator
			}

			return fmt.Sprintf("%s.Receive(%s)", v.convExpr(unaryExpr.X, nil), tupleResult)
		}
	}

	if unaryExpr.Op == token.XOR {
		operand := v.convExpr(unaryExpr.X, nil)

		// `^T(0)` — the all-ones idiom (os exec_windows' `^syscall.Handle(0)`): the constant
		// conversion folds to the bare literal, so `~0` loses the named type where the
		// parameter needs T (CS1503 ×2). Re-impose the named wrapper on a CONSTANT operand;
		// a plain-basic constant keeps the bare form (no churn).
		if tv, ok := v.info.Types[unaryExpr.X]; ok && tv.Value != nil {
			if named, ok := types.Unalias(tv.Type).(*types.Named); ok {
				if _, isBasic := named.Underlying().(*types.Basic); isBasic {
					return fmt.Sprintf("~((%s)%s)", v.getCSharpTypeName(named), operand)
				}
			}
		}

		// Go's `^x` has x's OWN type, so for a sub-int UNSIGNED type the complement wraps to that
		// width: `^uint16(5)` is 65530. C# promotes `byte`/`ushort` to `int` before applying `~`,
		// making the same expression -6 — identical in the low 16 bits, but every WIDENING use
		// then carries the sign bits. compress/flate's stored-block header is exactly that shape:
		// `writeBits(int32(^uint16(length)), 16)` became `writeBits((int32)(~(uint16)length), 16)`,
		// and writeBits' `bits |= uint64(b) << nbits` sign-extended the negative int across the
		// whole 64-bit accumulator, so every level-0 (NoCompression) stream was garbage and the
		// decoder rejected it ("flate: corrupt input before offset 59"). Truncate back to the
		// operand's own type.
		//
		// Only unsigned uint8/uint16 need this. A SIGNED narrow type is already value-correct
		// (C#'s `~` of a sign-extended operand equals the sign-extended Go result), and every type
		// at least 32 bits wide (`uint`, `uint32`, `uint64`, `uintptr`, the signed widths) keeps
		// its own type under C#'s `~`. A NAMED type routes through its golib wrapper's operator.
		if basic, isBasic := types.Unalias(v.getType(unaryExpr.X, false)).(*types.Basic); isBasic {
			if basic.Kind() == types.Uint8 || basic.Kind() == types.Uint16 {
				// A CONSTANT operand makes the whole thing a C# constant expression, and a
				// narrowing constant conversion is CHECKED at compile time: `~(ushort)0` promotes
				// to the `int` -1, and `(ushort)(-1)` is a hard CS0221 however correct the runtime
				// truncation would be. The all-ones idiom is exactly that shape —
				// dnsmessage's `int(^uint16(0))` bounds check, ×7 — so state `unchecked` there.
				// A variable operand needs nothing: C#'s default context is already unchecked, and
				// the extra keyword would only add noise at flate's `^uint16(length)` sites.
				if tv, ok := v.info.Types[unaryExpr.X]; ok && tv.Value != nil {
					return fmt.Sprintf("unchecked((%s)(~%s))", convertToCSTypeName(v.getAliasQualifiedTypeName(basic, false)), operand)
				}

				return fmt.Sprintf("((%s)(~%s))", convertToCSTypeName(v.getAliasQualifiedTypeName(basic, false)), operand)
			}
		}

		return "~" + operand
	}

	// A negated integer-valued float constant used in an integer context — `math.Inf(-1.0)`,
	// where Inf takes an int — must emit the integer form (`-1`), not `-1.0D` (CS1503). The
	// inner literal alone looks like a float (the conversion happens at the unary level), so this
	// mirrors the convBasicLit FLOAT case but keyed off the unary expression's resolved type.
	if unaryExpr.Op == token.SUB {
		if lit, ok := unaryExpr.X.(*ast.BasicLit); ok && lit.Kind == token.FLOAT {
			if tv, ok := v.info.Types[unaryExpr]; ok && tv.Type != nil && tv.Value != nil {
				if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
					return tv.Value.ExactString()
				}
			}
		}
	}

	// Go folds `-literal` into ONE constant, but convBasicLit classifies the POSITIVE operand
	// alone: `[]int32{-2147483648}` sees 2147483648 > MaxInt32 → `-(nint)2147483648L`, which has
	// no implicit conversion back to int32 (CS0266, internal/fuzz mutator's interesting32), and
	// the int64 minimum's operand routes through the unsigned branch to `-(nuint)…UL`, where C#
	// does not even define unary minus (CS0023). Classify the range on the UNARY expression's
	// resolved constant — the sign-folded value — mirroring the FLOAT arm above. The exact int32
	// minimum in an int32-typed context emits the plain negated literal (C# special-cases the
	// negated decimal int-min as an int constant, digit separators included), and the exact int64
	// minimum emits `-9223372036854775808L` (the matching long special case), cast for a Go-int
	// (nint) context since long→nint has no implicit conversion. Decimal source formatting is
	// preserved; hex/binary re-render as decimal — C# has no signed special case for those forms
	// (`-0x80000000` binds as a long-typed expression). Everything else keeps the default path
	// untouched: an operand within int32 never had a problem, an int32-min folded into a WIDER
	// context (`var x int64 = -2147483648`) compiles as `-(nint)…L` today (nint converts
	// implicitly to long) and — in an `any` slot — keeps Go-int values boxed as nint.
	if unaryExpr.Op == token.SUB {
		if lit, ok := unaryExpr.X.(*ast.BasicLit); ok && lit.Kind == token.INT {
			if opval, err := strconv.ParseInt(lit.Value, 0, 64); err != nil || opval > math.MaxInt32 {
				if tv, ok := v.info.Types[unaryExpr]; ok && tv.Type != nil && tv.Value != nil {
					if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 && basic.Info()&types.IsUnsigned == 0 {
						if folded, exact := constant.Int64Val(constant.ToInt(tv.Value)); exact {
							negated := "-" + lit.Value

							if len(lit.Value) > 1 && lit.Value[0] == '0' {
								negated = strconv.FormatInt(folded, 10) // includes the sign
							}

							if folded == math.MinInt32 && basic.Kind() == types.Int32 {
								return negated
							}

							if folded == math.MinInt64 {
								if basic.Kind() == types.Int64 {
									return negated + "L"
								}

								if basic.Kind() == types.Int {
									return fmt.Sprintf("((nint)(%sL))", negated)
								}
							}
						}
					}
				}
			}
		}
	}

	if unaryExpr.Op == token.SUB && v.isUnsignedType(unaryExpr.X) {
		// C# forbids unary minus on an unsigned operand (CS0023). Go's `-x` on an
		// unsigned value is two's-complement negation that wraps mod 2^N (e.g. the
		// `x & -x` lowest-set-bit idiom in math/bits). `(T)0 - x` has identical
		// wrap-around semantics and keeps the unsigned type T.
		typeName := convertToCSTypeName(v.getAliasQualifiedTypeName(v.getType(unaryExpr.X, false), false))
		return fmt.Sprintf("((%s)0 - %s)", typeName, v.convExpr(unaryExpr.X, nil))
	}

	if unaryExpr.Op == token.NOT && v.isNamedBooleanType(unaryExpr.X) {
		// C# forbids `!` on a NAMED boolean type (a defined type whose underlying is `bool`,
		// e.g. go/constant's `boolVal`), because the `[GoType("bool")]` struct that models it
		// has an implicit `bool` conversion but no `operator!` (CS0023). Go's `!y` on such a
		// value negates and KEEPS the named type (`case boolVal: return !y`, returned as the
		// `Value` interface). Emit `(T)(!(bool)y)` — cast to the underlying `bool`, negate, then
		// cast back to T — which preserves the named type so the result still satisfies the
		// interface. A plain `bool` operand keeps the bare `!x` form below (no golden churn).
		typeName := convertToCSTypeName(v.getAliasQualifiedTypeName(v.getType(unaryExpr.X, false), false))
		return fmt.Sprintf("((%s)(!(bool)%s))", typeName, v.convExpr(unaryExpr.X, nil))
	}

	return unaryExpr.Op.String() + v.convExpr(unaryExpr.X, nil)
}

// isNamedBooleanType reports whether the expression's DECLARED type is a defined (named) type
// whose underlying type is `bool` — e.g. `type boolVal bool`. The predeclared `bool` itself, and
// untyped bool constants, return false: C#'s `!`/`&&`/`||` apply to those directly, so only a named
// bool (modeled as a `[GoType("bool")]` struct with no logical operators) needs the cast-through-bool
// form. Shared by convUnaryExpr (`!`) and convBinaryExpr (`&&`/`||`).
func (v *Visitor) isNamedBooleanType(expr ast.Expr) bool {
	if tv, ok := v.info.Types[expr]; ok && tv.Type != nil {
		if _, isNamed := tv.Type.(*types.Named); !isNamed {
			return false
		}

		if basic, ok := tv.Type.Underlying().(*types.Basic); ok {
			return basic.Info()&types.IsBoolean != 0
		}
	}

	return false
}

// parenthesizeConversionBase wraps a rendered index BASE in parentheses when the Go expression it
// came from is a CONVERSION, so a member access appended to it binds to the conversion's RESULT.
//
// A Go conversion renders as a C# CAST, and a cast binds LOOSER than member access. 1.24
// runtime/lock_spinbit.go's key8 returns `&(*[goarch.PtrSize]uint8)(unsafe.Pointer(p))[0]` — the
// address of an element of a pointer-to-array conversion — and the base rendered as
// `(ж<array<uint8>>)(uintptr)(unsafe.Pointer.FromPinnedBox(Ꮡp))`. Appending `.at<uint8>(0)` to
// that parses as the two casts applied to `(…).at<uint8>(0)`: `at` runs on the unsafe.Pointer
// rather than on the array pointer, and the whole expression types as `ж<array<uint8>>` against a
// declared `ж<uint8>` — CS0029 ×2 per flavour, which is exactly the pair the compiler names.
//
// The test is go/types', not the rendered text: a CallExpr is a conversion exactly when its Fun
// denotes a TYPE. A genuine call returning `*[N]E` renders as `f(…)`, already a primary expression
// that member access binds to correctly, so it is left alone and no existing emission moves.
func (v *Visitor) parenthesizeConversionBase(base ast.Expr, rendered string) string {
	callExpr, ok := base.(*ast.CallExpr)

	if !ok {
		return rendered
	}

	if tv, ok := v.info.Types[callExpr.Fun]; !ok || !tv.IsType() {
		return rendered
	}

	return "(" + rendered + ")"
}
