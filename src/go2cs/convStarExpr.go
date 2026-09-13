// convStarExpr.go - Gbtc
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

func (v *Visitor) convStarExpr(starExpr *ast.StarExpr, context StarExprContext) string {
	ident := getIdentifier(starExpr.X)
	pointerRecv, recvName := v.isPointerReceiver()

	// A deref whose RESULT is still a reference-like value (pointer/slice/map/chan/func/
	// interface — `*pp` on a `**node`) is a READ OF THE HELD VALUE, not a dereference of it:
	// Go's `*pp` may legally yield nil (`for pp := &head; *pp != nil; …`). The strict `val`
	// panics on a null held value, so these derefs read through `ValueSlot` (identical real
	// slot, no nil check — reads and writes both persist); a deref producing a VALUE keeps the
	// strict `val` (a nil `*node` deref must panic, as in Go).
	derefAccessor := ".Value"

	if resultType := v.info.TypeOf(starExpr); resultType != nil && isInherentlyHeapAllocatedType(resultType) {
		derefAccessor = ".ValueSlot"
	}

	// The parameter-deref shortcut below applies to `*p` (the whole parameter) and `**p` — where the
	// operand denotes the parameter itself. It must NOT fire for `*p.field` (a deref of a pointer FIELD
	// reached through the parameter): `getIdentifier` digs through the selector to the param root, but
	// the operand `p.field` is a distinct lvalue that still needs its own dereference. Letting it take
	// the shortcut returned the field pointer un-dereferenced (`gp.ancestors` instead of
	// `gp.ancestors.Value`), e.g. `for _, a := range *gp.ancestors` → a Span/slice mismatch (CS8130). A
	// selector operand falls through to the selector handling below, which derefs via `.Value`.
	_, starXIsSelector := starExpr.X.(*ast.SelectorExpr)

	if ident != nil && v.identIsParameter(ident) && !starXIsSelector {
		// Check if the star expression is a pointer to pointer dereference
		if v.isPointer(ident) {
			if _, ok := starExpr.X.(*ast.StarExpr); ok {
				return v.convExpr(starExpr.X, nil) + derefAccessor
			}
		}

		// The remaining shortcut applies only when the operand IS the parameter ident itself
		// (`*p` → the deref-aliased ref-local `p`). Any other operand rooted at a parameter —
		// an INDEX like `*temps[depth]` (math/big natdiv's `qhat := *temps[depth]`, a
		// slice-of-pointers ELEMENT deref) — is a distinct lvalue that still needs its own
		// dereference; eliding it left the raw ж<nat> flowing into every use (CS1929/CS1503
		// ×29). Mirrors the earlier `*p.field` selector exclusion.
		if _, isDirectIdent := starExpr.X.(*ast.Ident); isDirectIdent {
			// A pointer parameter is dereferenced in the function body via a `ref var p = ref
			// Ꮡp.Value` local. That ref-local cannot be captured by a lambda/closure (CS8175 —
			// "cannot use ref local inside an anonymous method"), so inside a lambda dereference
			// the heap box parameter directly (`Ꮡp.Value`), which is a capturable reference type.
			if v.lambdaCapture != nil && v.lambdaCapture.conversionInLambda {
				return AddressPrefix + strings.TrimPrefix(v.getIdentName(ident), "@") + derefAccessor
			}

			// Prefer to use local reference instead of dereferencing a pointer
			return v.convExpr(starExpr.X, nil)
		}
	}

	// Special handling for field access (e.g., outer.ptr)
	if selectorExpr, ok := starExpr.X.(*ast.SelectorExpr); ok {
		// A pointer to a cross-package COLLISION-RENAMED type in a TYPE position (`*time.Location`,
		// where time's `Location` → `ΔLocation`, shadowed by the `Time.Location()` method) must
		// render the recorded import alias. convExpr below emits the raw `time_package.Location`,
		// which is CS0426 in the alias-less `[GoType("ж<…>")]` wrapper of slog's `type timeLocation
		// *time.Location`. Only a TYPE operand is redirected — a pointer FIELD deref (`*outer.ptr`)
		// keeps the convExpr rendering — and foreignAliasedTypeName fires ONLY for a renamed foreign
		// type (a non-renamed `*time.Time` has no alias entry and falls through unchanged).
		if tv, isType := v.info.Types[starExpr.X]; isType && tv.IsType() {
			if aliased, ok := v.foreignAliasedTypeName(v.getType(starExpr.X, false)); ok {
				return fmt.Sprintf("%s<%s>", PointerPrefix, aliased)
			}
		}

		baseExpr := v.convExpr(starExpr.X, nil)

		// One star is ONE deref: `*i.pprev` on a `**special` field yields a `*special` —
		// `i.pprev.Value`. The old multi-level arm added an EXTRA `.Value` for pointer depth > 1,
		// double-dereferencing every single-star of a double-pointer field (runtime mheap.go's
		// specialsIter walk — CS0029 in both assignment directions). A genuine `**pp` is two
		// nested StarExprs, each contributing its own `.Value`.
		if _, ok := v.getIdentType(selectorExpr.Sel).(*types.Pointer); !ok {
			// Selector is not a pointer, assume this is a pointer cast operation
			return fmt.Sprintf("%s<%s>", PointerPrefix, baseExpr)
		}

		return baseExpr + derefAccessor
	}

	// In a parenthesis, we are applying a pointer cast operation — but only when the starred
	// operand denotes a TYPE (`(*int)(p)`, `(*MyType)(p)`). When it is a VALUE, e.g. a function
	// pointer variable `newInc`, then `(*newInc)(args)` is a dereference-then-call of that pointer
	// (`newInc.Value(args)`), not a cast; let it fall through to the deref path below.
	starXIsType := false
	if tv, ok := v.info.Types[starExpr.X]; ok && tv.IsType() {
		starXIsType = true
	}

	// When the starred operand denotes a TYPE, `*T` is the pointer TYPE `ж<T>` — in a `(*T)(p)`
	// cast (inParenExpr) and equally in any other type position, e.g. a type assertion `x.(*T)`
	// (which renders the asserted type via convStarExpr). A non-paren `*type` would otherwise fall
	// through to the value-deref path and emit `T.Value` (CS0426: `val` is not a member type of `T`).
	if starXIsType {
		// Check if the pointer target type is a struct or pointer to a struct
		if structType, exprType := v.extractStructType(starExpr); structType != nil && !v.liftedTypeExists(structType) {
			v.indentLevel++
			v.visitStructType(structType, exprType, "type", nil, true, nil)
			v.indentLevel--
		}

		// Check if the pointer target type is an anonymous interface
		if interfaceType, exprType := v.extractInterfaceType(starExpr); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
			v.indentLevel++
			v.visitInterfaceType(interfaceType, exprType, "type", nil, true, nil)
			v.indentLevel--
		}

		starType := v.getType(starExpr.X, false)
		pointerType := convertToCSTypeName(v.getAliasQualifiedTypeName(starType, false))
		return fmt.Sprintf("%s<%s>", PointerPrefix, pointerType)
	}

	// Check for a call expr that contains a paren expr with a star expresssion inside it where starExpr.X is an identifier
	if callExpr, ok := starExpr.X.(*ast.CallExpr); ok {
		if len(callExpr.Args) == 1 {
			if parenExpr, ok := callExpr.Fun.(*ast.ParenExpr); ok {
				if _, ok := parenExpr.X.(*ast.StarExpr); ok {
					if ident := getIdentifier(parenExpr.X); ident != nil {
						// In this case we are dealing with a casted pointer dereference, e.g., "*(*int)"
						lambdaContext := DefaultLambdaContext()
						lambdaContext.isPointerCast = true
						result := v.convExpr(starExpr.X, []ExprContext{lambdaContext})

						if context.inLhsAssign {
							return fmt.Sprintf("(%s)%s", result, derefAccessor)
						} else {
							return fmt.Sprintf("%s%s", PointerDerefOp, result)
						}
					}
				}
			}
		}
	}

	// The receiver-deref shortcut (`*u` → the deref-aliased ref-local `u`) applies only when the
	// operand IS the receiver ident itself — getIdentifier digs through an index/selector to the
	// receiver ROOT, but `*u.handles[x]` (a deref of a pointer-valued map ELEMENT reached through
	// the receiver; go/types unify.go's `return *u.handles[x]` / `*u.handles[x] = t`) is a
	// distinct lvalue that still needs its own dereference — eliding it returned/assigned the raw
	// ж<ΔType> (CS0266 both directions). Mirrors the direct-ident gate the parameter shortcut
	// above already has; non-direct operands fall through to the tail deref. Object identity via
	// identResolvesToReceiver, matching the other receiver-specific renders.
	if pointerRecv && ident != nil {
		if _, isDirectIdent := starExpr.X.(*ast.Ident); isDirectIdent && v.identResolvesToReceiver(ident, recvName) {
			return v.convExpr(starExpr.X, nil)
		}
	}

	// A deref whose operand is a type CONVERSION renders as a C# cast, and postfix binds tighter
	// than a cast — a naked `.Value` re-binds onto the cast's inner operand: runtime panic.go's
	// `return *(*func())(add(…)), true` emitted `(ж<Action>)(uintptr)(add(…)).Value` (the `.Value`
	// reads the inner @unsafe.Pointer → CS0029 ж<Action>→Action in the tuple). The cast-deref
	// branch above misses it because a FUNC-type (or other non-ident) starred inner has no
	// identifier. Wrap the whole cast before dereferencing.
	if call, ok := starExpr.X.(*ast.CallExpr); ok && v.callExprIsTypeConversion(call) {
		return fmt.Sprintf("(%s)%s", v.convExpr(starExpr.X, nil), derefAccessor)
	}

	// A deref whose operand is ITSELF a deref renders with the prefix `~` form, on which a
	// naked postfix `.Value` mis-binds (postfix beats unary: `~X.Value` is `~(X.Value)`) —
	// reflect MapOf's `**(**mapType)(unsafe.Pointer(&imap))` read the inner unsafe.Pointer's
	// slot and left ж<mapType> unwrapped once (CS0029). Wrap the inner deref before
	// dereferencing again. (A pointer PARAMETER's `**p` takes the shortcut above and never
	// reaches here.)
	if _, ok := starExpr.X.(*ast.StarExpr); ok {
		return fmt.Sprintf("(%s)%s", v.convExpr(starExpr.X, nil), derefAccessor)
	}

	return v.convExpr(starExpr.X, nil) + derefAccessor
}

// Helper to get pointer depth for selector expressions
