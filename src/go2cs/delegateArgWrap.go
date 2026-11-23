// delegateArgWrap.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// How a func value is passed where Go assigns it to another func type: a call argument (convCallExpr), a struct
// field in a composite literal (convCompositeLit), and every other assignment site through funcValueForSlot (an
// assignment, a declaration, a result, a send, an element). Go's assignment never panics, and a nil func value
// arrives nil.
//
// C# has no implicit conversion between distinct delegate types, so a value of one delegate type reaching a slot of
// another is re-wrapped. The .NET delegate-copy constructor `new D(x)` throws when x is null ("Delegate to an instance
// method cannot have null 'this'"), where Go passes nil, so a wrapped VALUE goes through golib's
// NilSafeDelegateConversion: null in, null out. A method group or a func literal is never null and keeps `new D(x)`.
//
// And where both sides render as the SAME C# delegate, there is nothing to wrap: two unnamed signatures that are
// identical render as one structural delegate.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

// delegateWrapKind is how a func value is passed to a slot of another func type.
type delegateWrapKind int

const (
	// delegateWrapNone: both render as the same C# delegate, so the value passes as it is.
	delegateWrapNone delegateWrapKind = iota

	// delegateWrapConstruct: a method group or func literal, never null, takes the delegate constructor `new D(x)`.
	delegateWrapConstruct

	// delegateWrapNilSafe: a value of a different delegate type, which may be nil, takes NilSafeDelegateConversion.
	delegateWrapNilSafe
)

// delegateWrap classifies passing arg (Go type argType) to a slot of Go type target. For delegateWrapNilSafe it also
// returns the C# delegate type arg renders as, the conversion's source type argument.
//
// A method group or func literal keeps the constructor even where the delegate types match: it has no delegate type of
// its own to match, `new D(group)` can never throw, and leaving it unchanged keeps every such site as it was.
func (v *Visitor) delegateWrap(target types.Type, arg ast.Expr, argType types.Type) (delegateWrapKind, string) {
	if _, isLit := ast.Unparen(arg).(*ast.FuncLit); isLit || v.exprIsMethodGroup(arg) {
		return delegateWrapConstruct, ""
	}

	if targetSig, ok := v.rendersAsStructuralDelegate(target, nil); ok {
		if argSig, ok := v.rendersAsStructuralDelegate(argType, arg); ok && types.Identical(targetSig, argSig) {
			return delegateWrapNone, ""
		}
	}

	return delegateWrapNilSafe, v.funcValueDelegateType(arg, argType)
}

// rendersAsStructuralDelegate reports whether a func value of Go type t renders as the structural delegate of its
// signature, and returns that signature: an unnamed signature. A named func type renders as its own delegate. expr, when given, is the value itself: a `:=` local declared from a method group is typed with
// the package's matching named delegate instead (visitAssignStmt's methodGroupDelegateType).
func (v *Visitor) rendersAsStructuralDelegate(t types.Type, expr ast.Expr) (*types.Signature, bool) {
	switch typed := types.Unalias(t).(type) {
	case *types.Signature:
		if expr != nil && v.renderedAsMethodGroupLocal(expr, typed) {
			return nil, false
		}

		return typed, true
	}

	return nil, false
}

// renderedAsMethodGroupLocal reports whether expr is a `:=` local declared from a method group whose signature names
// one of the package's named func types, which visitAssignStmt declares with that named delegate.
func (v *Visitor) renderedAsMethodGroupLocal(expr ast.Expr, sig *types.Signature) bool {
	ident, ok := ast.Unparen(expr).(*ast.Ident)

	return ok && v.namedFuncTypeNameForSignature(sig) != "" && v.identDeclaredFromMethodGroup(ident)
}

// funcValueDelegateType is the C# delegate type a func value renders as.
func (v *Visitor) funcValueDelegateType(expr ast.Expr, t types.Type) string {
	if sig, ok := types.Unalias(t).(*types.Signature); ok && v.renderedAsMethodGroupLocal(expr, sig) {
		return v.namedFuncTypeNameForSignature(sig)
	}

	return v.getCSharpTypeName(t)
}

// recordDelegateArgWrap records how argument index of a call (or element index of a composite literal) passes arg to
// a slot of Go type target, a slot of a different Go type than arg's (the callers' gate): no wrap, the delegate
// constructor, or the nil-safe conversion.
func (v *Visitor) recordDelegateArgWrap(context *CallExprContext, index int, target types.Type, arg ast.Expr, argType types.Type) {
	switch kind, source := v.delegateWrap(target, arg, argType); kind {
	case delegateWrapConstruct:
		if context.wrapArgWithNew == nil {
			context.wrapArgWithNew = make(map[int]string)
		}

		context.wrapArgWithNew[index] = v.getCSharpTypeName(target)
	case delegateWrapNilSafe:
		if context.nilSafeDelegateArgs == nil {
			context.nilSafeDelegateArgs = make(map[int]string)
		}

		context.nilSafeDelegateArgs[index] = v.getCSharpTypeName(target) + ", " + source
	}
}

// funcValueForSlot converts the rendered func value expr to a slot of Go type target where Go assigns between two
// DIFFERENT func types: a defined func type and its unnamed signature, either way round. Go's assignability makes
// that implicit (the underlying types are identical and one side is unnamed); every defined func type renders as a
// delegate of its own, and C# converts between two delegate types only through a new delegate, so the value takes
// golib's NilSafeDelegateConversion (a nil value stays nil, as in Go). Every other value is returned as it was: the
// same Go type, a non-func slot or value (untyped nil included), a func literal or method group (each converts to
// the target delegate natively), or two types that render as one delegate.
func (v *Visitor) funcValueForSlot(target types.Type, expr ast.Expr, rendered string) string {
	if target == nil || expr == nil {
		return rendered
	}

	source := v.info.TypeOf(expr)

	if source == nil || types.Identical(source, target) {
		return rendered
	}

	if _, ok := target.Underlying().(*types.Signature); !ok {
		return rendered
	}

	if _, ok := source.Underlying().(*types.Signature); !ok {
		return rendered
	}

	if typeContainsTypeParams(target) || typeContainsTypeParams(source) {
		return rendered
	}

	kind, sourceName := v.delegateWrap(target, expr, source)

	if kind != delegateWrapNilSafe {
		return rendered
	}

	return fmt.Sprintf("NilSafeDelegateConversion<%s, %s>(%s)", v.getCSharpTypeName(target), sourceName, rendered)
}
