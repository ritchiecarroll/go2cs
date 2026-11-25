// operandOrder.go - Gbtc
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
	"strings"
)

// Go's order for the operands of one statement, as gc implements it: every call and receive the statement holds runs
// first, in lexical order, and the other operands (a variable, a field, a dereference, an element) are read after
// them. C# reads every operand left to right. The two orders differ only where a call can write an operand read
// before it, so only there does the converter change anything: each call or receive, up to the last one that follows
// such an operand, moves in lexical order into a statement prelude (`var evalᴛ1 = ᐸꟷ(kc);`) and the operands stay where
// they are (`readWith(pp, evalᴛ1)`). docs/DESIGN-operand-order.md has the design and its limits.
//
// gc orders more than calls. These EARLY forms run at their lexical place among the calls, reading their operands
// then: len, cap, append, min, max, make, real, imag, complex, copy, a conversion to []byte, to []rune or from a slice
// to an array, a type assertion, a slice expression, a map index whose key is string(bytes), a map literal that reads
// an operand a call can write, and a value boxed to an interface that is not addressable and is a bool, a one-byte
// integer, a struct, an array or &T{...}. An early form never triggers a hoist; when a later call moves ahead of it,
// it moves too, in order, as its own temp. An && or || that holds a call, and a map literal that holds one, are one
// ordered call: the first so its short circuit stands, the second because gc evaluates its elements in place.
//
// &T{...} evaluates the literal's elements like any other operands; any other &x evaluates only what locates x (an
// index, a pointer), as the left side of an assignment does.
//
// What is never hoisted, and what a call never reorders:
//   - an operand of && or ||, which runs only after the left one: an && or || that holds a call moves whole or not
//     at all;
//   - an element of a map literal, which moves with its literal;
//   - a func literal's body, which is another function;
//   - a multi-value call, and a statement whose right side is one (a, b = f()) or a comma-ok form.
//
// A fail-safe backs the walk (operandOrderCovers): a statement holding a call, receive or early form before its last
// hoist that the walk did not record keeps C#'s order, so a form the walk does not know never reorders two calls.

type operandOrderKind int

const (
	operandOrderRead  operandOrderKind = iota // a read of an operand a call can write
	operandOrderCall                          // a call or receive: it runs before the plain operands
	operandOrderEarly                         // an early form: ordered among the calls, but never a reason to hoist
)

// operandOrderEvent is one step of a statement's evaluation, in Go's lexical order.
type operandOrderEvent struct {
	node      ast.Expr
	kind      operandOrderKind
	ancestors map[ast.Expr]bool // the calls and early forms whose operands hold this event: it runs with them
}

type operandOrderWalk struct {
	v      *Visitor
	stack  []ast.Expr
	events []operandOrderEvent
}

func (w *operandOrderWalk) record(node ast.Expr, kind operandOrderKind) {
	ancestors := make(map[ast.Expr]bool, len(w.stack))

	for _, ancestor := range w.stack {
		ancestors[ancestor] = true
	}

	w.events = append(w.events, operandOrderEvent{node: node, kind: kind, ancestors: ancestors})
}

// read records node when it is an operand a call can write (operandIsMutable).
func (w *operandOrderWalk) read(node ast.Expr) {
	if w.v.operandIsMutable(node) {
		w.record(node, operandOrderRead)
	}
}

// walkUnit visits the operands of a call or an early form, then records the unit itself: its operands are evaluated
// when it runs. targets are the types its operands are assigned to, where known, for the boxed early form.
func (w *operandOrderWalk) walkUnit(unit ast.Expr, kind operandOrderKind, targets []types.Type, operands ...ast.Expr) {
	w.stack = append(w.stack, unit)

	for i, operand := range operands {
		var target types.Type

		if i < len(targets) {
			target = targets[i]
		}

		w.walkTo(operand, target)
	}

	w.stack = w.stack[:len(w.stack)-1]
	w.record(unit, kind)
}

// walkTo visits expr, assigned to target (nil when not known): a value boxed to an interface there is an early form
// when gc reads it at its place (operandBoxedEarly).
func (w *operandOrderWalk) walkTo(expr ast.Expr, target types.Type) {
	if target != nil && w.v.operandBoxedEarly(expr, target) {
		w.walkUnit(expr, operandOrderEarly, nil, expr)
		return
	}

	w.walk(expr)
}

// walk visits expr in evaluation order.
func (w *operandOrderWalk) walk(expr ast.Expr) {
	v := w.v

	// A constant reads nothing and runs nothing: len("abc") and min(1, 2) stay where they are.
	if tv, ok := v.info.Types[expr]; ok && tv.Value != nil {
		return
	}

	switch e := expr.(type) {
	case nil:
	case *ast.ParenExpr:
		w.walk(e.X)
	case *ast.FuncLit, *ast.BasicLit:
	case *ast.Ident:
		w.read(e)
	case *ast.CompositeLit:
		literalType := types.Unalias(v.getExprType(e)).Underlying()

		if _, isMap := literalType.(*types.Map); isMap {
			// gc evaluates a map literal's elements in place, at the literal's lexical position among the calls, so the
			// literal is one ordered unit: a call when it holds one, like && and ||, and otherwise an early form when it
			// reads an operand a call can write. Either way it moves whole.
			if operandHoldsCall(v, e) {
				w.walkUnit(e, operandOrderCall, nil)
				return
			}

			elements := &operandOrderWalk{v: v}

			for _, elt := range e.Elts {
				if kv, ok := elt.(*ast.KeyValueExpr); ok {
					elements.walk(kv.Key)
					elements.walk(kv.Value)
				} else {
					elements.walk(elt)
				}
			}

			if len(elements.events) > 0 {
				w.walkUnit(e, operandOrderEarly, nil)
			}

			return
		}

		for i, elt := range e.Elts {
			var target types.Type

			if kv, ok := elt.(*ast.KeyValueExpr); ok {
				elt = kv.Value

				if structType, isStruct := literalType.(*types.Struct); isStruct {
					if key, isIdent := kv.Key.(*ast.Ident); isIdent {
						for j := 0; j < structType.NumFields(); j++ {
							if structType.Field(j).Name() == key.Name {
								target = structType.Field(j).Type()
							}
						}
					}
				}
			} else if structType, isStruct := literalType.(*types.Struct); isStruct && i < structType.NumFields() {
				target = structType.Field(i).Type()
			}

			switch container := literalType.(type) {
			case *types.Slice:
				target = container.Elem()
			case *types.Array:
				target = container.Elem()
			}

			w.walkTo(elt, target)
		}
	case *ast.BinaryExpr:
		if e.Op == token.LAND || e.Op == token.LOR {
			// One ordered unit when it holds a call: its right operand runs only after its left, so it moves whole.
			if operandHoldsCall(v, e) {
				w.walkUnit(e, operandOrderCall, nil)
				return
			}
		}

		w.walk(e.X)
		w.walk(e.Y)
	case *ast.UnaryExpr:
		switch e.Op {
		case token.ARROW:
			w.walkUnit(e, operandOrderCall, nil, e.X)
		case token.AND:
			// &T{...} evaluates the literal's elements; any other &x evaluates only what locates x (an index, a
			// pointer), as an assignment's left side does, and reads nothing at x itself.
			if literal, isLiteral := ast.Unparen(e.X).(*ast.CompositeLit); isLiteral {
				w.walk(literal)
			} else {
				w.walkStore(e.X)
			}
		default:
			w.walk(e.X)
		}
	case *ast.SliceExpr:
		w.walkUnit(e, operandOrderEarly, nil, e.X, e.Low, e.High, e.Max)
	case *ast.IndexExpr:
		if v.operandMapIndexByBytes(e) {
			w.walkUnit(e, operandOrderEarly, nil, e.X, e.Index)
			return
		}

		if !operandHoldsCall(v, e) && !operandHoldsLiteral(e) {
			w.read(e)
			return
		}

		w.walk(e.X)
		w.walk(e.Index)
		w.read(e)
	case *ast.StarExpr, *ast.SelectorExpr:
		if !operandHoldsCall(v, e) && !operandHoldsLiteral(e) {
			w.read(e)
			return
		}

		switch e := e.(type) {
		case *ast.StarExpr:
			w.walk(e.X)
		case *ast.SelectorExpr:
			w.walk(e.X)
		}

		w.read(e)
	case *ast.TypeAssertExpr:
		w.walkUnit(e, operandOrderEarly, nil, e.X)
	case *ast.CallExpr:
		switch v.operandCallKind(e) {
		case operandCallEarly:
			w.walkUnit(e, operandOrderEarly, nil, e.Args...)
		case operandCallTransparent:
			for _, arg := range e.Args {
				w.walk(arg)
			}
		default:
			operands := append([]ast.Expr{}, e.Args...)
			targets := v.operandParamTypes(e)

			if selector, ok := ast.Unparen(e.Fun).(*ast.SelectorExpr); ok && !v.isPackageQualifier(selector.X) {
				if v.operandReceiverAddressed(selector) {
					// A pointer method called on a value takes the value's address, which reads nothing a call can
					// change: only what locates the value (an index, a pointer) is evaluated, as for an assignment.
					w.stack = append(w.stack, e)
					w.walkStore(selector.X)
					w.stack = w.stack[:len(w.stack)-1]
				} else {
					operands = append([]ast.Expr{selector.X}, operands...)
					targets = append([]types.Type{nil}, targets...)
				}
			} else if _, isIdent := ast.Unparen(e.Fun).(*ast.Ident); !isIdent {
				operands = append([]ast.Expr{e.Fun}, operands...)
				targets = append([]types.Type{nil}, targets...)
			}

			w.walkUnit(e, operandOrderCall, targets, operands...)
		}
	}
}

// operandReceiverAddressed reports whether the method call through selector takes its operand's address: a pointer
// method called on a value that is not a pointer.
func (v *Visitor) operandReceiverAddressed(selector *ast.SelectorExpr) bool {
	selection, ok := v.info.Selections[selector]

	if !ok || selection.Kind() != types.MethodVal {
		return false
	}

	signature, ok := selection.Obj().Type().(*types.Signature)

	if !ok || signature.Recv() == nil {
		return false
	}

	if _, pointerRecv := types.Unalias(signature.Recv().Type()).(*types.Pointer); !pointerRecv {
		return false
	}

	operandType := v.getExprType(selector.X)

	if operandType == nil || types.IsInterface(operandType) {
		return false
	}

	_, pointerOperand := types.Unalias(operandType).Underlying().(*types.Pointer)
	return !pointerOperand
}

// operandParamTypes returns the parameter type each argument of call is assigned to, in argument order, or nil when
// the call's signature is not known.
func (v *Visitor) operandParamTypes(call *ast.CallExpr) []types.Type {
	signature, ok := types.Unalias(v.getExprType(call.Fun)).Underlying().(*types.Signature)

	if !ok {
		return nil
	}

	params := signature.Params()
	targets := make([]types.Type, len(call.Args))

	for i := range call.Args {
		switch {
		case signature.Variadic() && i >= params.Len()-1 && call.Ellipsis == token.NoPos:
			if variadic, isSlice := params.At(params.Len() - 1).Type().(*types.Slice); isSlice {
				targets[i] = variadic.Elem()
			}
		case i < params.Len():
			targets[i] = params.At(i).Type()
		}
	}

	return targets
}

// operandBoxedEarly reports whether expr, assigned to target, is a value gc boxes to an interface at its place among
// the calls (measured at go1.26.11): a value that is not addressable and is a bool, a one-byte integer, a struct, an
// array, or &T{...}. Any other boxed value, an addressable one included, is read after the calls.
func (v *Visitor) operandBoxedEarly(expr ast.Expr, target types.Type) bool {
	if !types.IsInterface(target) {
		return false
	}

	expr = ast.Unparen(expr)

	if tv, ok := v.info.Types[expr]; ok && tv.Value != nil {
		return false
	}

	switch e := expr.(type) {
	case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr, *ast.StarExpr, *ast.FuncLit, *ast.BasicLit:
		return false
	case *ast.UnaryExpr:
		if e.Op == token.AND {
			_, isLiteral := ast.Unparen(e.X).(*ast.CompositeLit)
			return isLiteral
		}
	case *ast.CallExpr:
		if tv, ok := v.info.Types[e.Fun]; !ok || !tv.IsType() {
			return false
		}
	}

	valueType := v.getExprType(expr)

	if valueType == nil || types.IsInterface(valueType) {
		return false
	}

	switch underlying := types.Unalias(valueType).Underlying().(type) {
	case *types.Basic:
		switch underlying.Kind() {
		case types.Bool, types.UntypedBool, types.Int8, types.Uint8:
			return true
		}
	case *types.Struct, *types.Array:
		return true
	}

	return false
}

// operandMapIndexByBytes reports whether expr is a map index whose key is string(b) for a byte slice b, which gc reads
// at its place among the calls, with no copy of the key.
func (v *Visitor) operandMapIndexByBytes(expr *ast.IndexExpr) bool {
	if _, isMap := types.Unalias(v.getExprType(expr.X)).Underlying().(*types.Map); !isMap {
		return false
	}

	conversion, ok := ast.Unparen(expr.Index).(*ast.CallExpr)

	if !ok || len(conversion.Args) != 1 {
		return false
	}

	if tv, ok := v.info.Types[conversion.Fun]; !ok || !tv.IsType() {
		return false
	}

	_, fromSlice := types.Unalias(v.getExprType(conversion.Args[0])).Underlying().(*types.Slice)
	return fromSlice
}

// walkStore visits the left side of an assignment: what it evaluates to find the place it stores into.
func (w *operandOrderWalk) walkStore(expr ast.Expr) {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		w.walkStore(e.X)
	case *ast.IndexExpr:
		if _, isArray := types.Unalias(w.v.getExprType(e.X)).Underlying().(*types.Array); isArray {
			w.walkStore(e.X)
		} else {
			w.walk(e.X)
		}

		w.walk(e.Index)
	case *ast.StarExpr:
		w.walk(e.X)
	case *ast.SelectorExpr:
		if _, isPointer := types.Unalias(w.v.getExprType(e.X)).Underlying().(*types.Pointer); isPointer {
			w.walk(e.X)
		} else if !w.v.isPackageQualifier(e.X) {
			w.walkStore(e.X)
		}
	}
}

type operandCallClass int

const (
	operandCallHoistable   operandCallClass = iota // a call or method call: it runs before the plain operands
	operandCallTransparent                         // a conversion or builtin: its operands belong to the statement
	operandCallEarly                               // an early builtin or conversion: ordered among the calls
)

func (v *Visitor) operandCallKind(call *ast.CallExpr) operandCallClass {
	if tv, ok := v.info.Types[call.Fun]; ok && tv.IsType() {
		if len(call.Args) == 1 {
			from := types.Unalias(v.getExprType(call.Args[0])).Underlying()

			switch types.Unalias(tv.Type).Underlying().(type) {
			case *types.Array:
				if _, fromSlice := from.(*types.Slice); fromSlice {
					return operandCallEarly
				}
			case *types.Slice:
				// []byte(s) and []rune(s) from a string copy it where they stand.
				if basic, isBasic := from.(*types.Basic); isBasic && basic.Info()&types.IsString != 0 {
					return operandCallEarly
				}
			}
		}

		return operandCallTransparent
	}

	if ident, ok := ast.Unparen(call.Fun).(*ast.Ident); ok {
		if builtin, isBuiltin := v.info.Uses[ident].(*types.Builtin); isBuiltin {
			switch builtin.Name() {
			case "len", "cap", "append", "min", "max", "make", "real", "imag", "complex", "copy":
				return operandCallEarly
			}

			return operandCallTransparent
		}
	}

	return operandCallHoistable
}

func (v *Visitor) isPackageQualifier(expr ast.Expr) bool {
	ident, ok := expr.(*ast.Ident)

	if !ok {
		return false
	}

	_, isPackage := v.info.Uses[ident].(*types.PkgName)
	return isPackage
}

// operandHoldsCall reports whether evaluating expr runs a call or receive, or an early form.
func operandHoldsCall(v *Visitor, expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(node ast.Node) bool {
		if found {
			return false
		}

		switch n := node.(type) {
		case *ast.FuncLit:
			return false
		case *ast.UnaryExpr:
			found = n.Op == token.ARROW
		case *ast.CallExpr:
			found = v.operandCallKind(n) != operandCallTransparent
		case *ast.SliceExpr, *ast.TypeAssertExpr:
			found = true
		case *ast.IndexExpr:
			found = v.operandMapIndexByBytes(n)
		}

		return !found
	})

	return found
}

// operandHoldsLiteral reports whether expr holds a composite literal, whose elements are operands of their own: an
// element read of the literal (map[K]V{k: g}[k]) is walked through it rather than read whole.
func operandHoldsLiteral(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(node ast.Node) bool {
		switch node.(type) {
		case *ast.FuncLit:
			return false
		case *ast.CompositeLit:
			found = true
		}

		return !found
	})

	return found
}

// operandIsMutable is the predicate: reading expr can observe a write made by a call of the same statement. A plain
// local is never such an operand, because no call can write it, which is why it is left out (the reason is that the
// write is unobservable, not that it is unreached). The operands that are:
//   - a package-level variable;
//   - a local variable, parameter or result that a func literal captures or whose address is taken;
//   - a dereference, a field reached through a pointer, and an element of a slice, a map or an array behind a pointer;
//   - a field or an array element, or a slice or string index, of an operand that is itself mutable.
func (v *Visitor) operandIsMutable(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		return v.operandIsMutable(e.X)
	case *ast.Ident:
		variable, ok := v.info.Uses[e].(*types.Var)

		if !ok || variable.IsField() {
			return false
		}

		if variable.Pkg() != nil && variable.Parent() == variable.Pkg().Scope() {
			return true
		}

		return v.operandOrderSharedLocals()[variable]
	case *ast.StarExpr:
		return true
	case *ast.SelectorExpr:
		if v.isPackageQualifier(e.X) {
			variable, ok := v.info.Uses[e.Sel].(*types.Var)
			return ok && !variable.IsField()
		}

		selection, ok := v.info.Selections[e]

		if !ok || selection.Kind() != types.FieldVal {
			return false
		}

		if selection.Indirect() {
			return true
		}

		return v.operandIsMutable(e.X)
	case *ast.IndexExpr:
		// An element of a literal is no call's to write: only its index can be.
		if _, isLiteral := ast.Unparen(e.X).(*ast.CompositeLit); isLiteral {
			return v.operandIsMutable(e.Index)
		}

		switch base := types.Unalias(v.getExprType(e.X)).Underlying().(type) {
		case *types.Slice, *types.Map:
			return true
		case *types.Pointer:
			return true
		case *types.Array, *types.Basic:
			_ = base
			return v.operandIsMutable(e.X) || v.operandIsMutable(e.Index)
		}

		return false
	case *ast.SliceExpr:
		return v.operandIsMutable(e.X) || v.operandIsMutable(e.Low) || v.operandIsMutable(e.High) ||
			v.operandIsMutable(e.Max)
	}

	return false
}

// operandOrderSharedLocals returns the file's local variables a call can write: those a func literal captures and
// those whose address is taken, explicitly or by a pointer-method call or a slice of an array.
func (v *Visitor) operandOrderSharedLocals() map[*types.Var]bool {
	if v.operandSharedLocals != nil && v.operandSharedLocalsFile == v.astFile {
		return v.operandSharedLocals
	}

	shared := map[*types.Var]bool{}
	v.operandSharedLocals, v.operandSharedLocalsFile = shared, v.astFile

	if v.astFile == nil {
		return shared
	}

	local := func(ident *ast.Ident) *types.Var {
		variable, ok := v.info.ObjectOf(ident).(*types.Var)

		if !ok || variable.IsField() || variable.Pkg() == nil || variable.Parent() == variable.Pkg().Scope() {
			return nil
		}

		return variable
	}

	var root func(expr ast.Expr) *ast.Ident

	root = func(expr ast.Expr) *ast.Ident {
		switch e := expr.(type) {
		case *ast.Ident:
			return e
		case *ast.ParenExpr:
			return root(e.X)
		case *ast.SelectorExpr:
			if selection, ok := v.info.Selections[e]; ok && selection.Kind() == types.FieldVal && !selection.Indirect() {
				return root(e.X)
			}
		case *ast.IndexExpr:
			if _, isArray := types.Unalias(v.getExprType(e.X)).Underlying().(*types.Array); isArray {
				return root(e.X)
			}
		}

		return nil
	}

	mark := func(expr ast.Expr) {
		if ident := root(expr); ident != nil {
			if variable := local(ident); variable != nil {
				shared[variable] = true
			}
		}
	}

	ast.Inspect(v.astFile, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.FuncLit:
			ast.Inspect(n.Body, func(inner ast.Node) bool {
				if ident, ok := inner.(*ast.Ident); ok {
					if variable := local(ident); variable != nil && (variable.Pos() < n.Pos() || variable.Pos() >= n.End()) {
						shared[variable] = true
					}
				}

				return true
			})
		case *ast.UnaryExpr:
			if n.Op == token.AND {
				mark(n.X)
			}
		case *ast.SliceExpr:
			if _, isArray := types.Unalias(v.getExprType(n.X)).Underlying().(*types.Array); isArray {
				mark(n.X)
			}
		case *ast.SelectorExpr:
			if selection, ok := v.info.Selections[n]; ok && selection.Kind() == types.MethodVal {
				if signature, ok := selection.Obj().Type().(*types.Signature); ok && signature.Recv() != nil {
					// Only a value operand has its address taken by a pointer-receiver call; a pointer operand is passed
					// as it is, and the variable holding it stays unshared.
					_, pointerRecv := types.Unalias(signature.Recv().Type()).(*types.Pointer)
					_, pointerOperand := types.Unalias(v.getExprType(n.X)).Underlying().(*types.Pointer)

					if pointerRecv && !pointerOperand {
						mark(n.X)
					}
				}
			}
		}

		return true
	})

	// The converter's own heap-escape set is not consulted: it also holds variables boxed for reasons no call can
	// observe (a pointer local whose pointee escapes), and the four forms above are every way Go code takes a local's
	// address or shares it.
	return shared
}

// operandOrderHoists returns the calls and receives of one statement that move into its prelude, in the order they
// run. stores are the left sides of an assignment and exprs everything else the statement evaluates, in lexical
// order; readStores marks an assignment that also reads its left side (x += f()).
func (v *Visitor) operandOrderHoists(stores []ast.Expr, readStores bool, exprs []ast.Expr) []ast.Expr {
	walk := &operandOrderWalk{v: v}

	for _, store := range stores {
		if readStores {
			walk.walk(store)
		} else {
			walk.walkStore(store)
		}
	}

	for _, expr := range exprs {
		walk.walk(expr)
	}

	// A call must move when a read evaluated before it can see its write: a read whose every enclosing unit also
	// encloses the call. A read inside an earlier call or early form is evaluated when that unit runs, before the call
	// in either order, and a read inside the call itself is its own operand.
	last := -1

	for i, event := range walk.events {
		if event.kind != operandOrderCall {
			continue
		}

		for _, earlier := range walk.events[:i] {
			if earlier.kind == operandOrderRead && operandAncestorsWithin(earlier.ancestors, event.ancestors) {
				last = i
				break
			}
		}
	}

	var hoists []ast.Expr

	for _, event := range walk.events[:last+1] {
		if event.kind != operandOrderRead {
			hoists = append(hoists, event.node)
		}
	}

	if len(hoists) > 0 && !operandOrderCovers(v, walk.events, hoists, stores, exprs) {
		return nil
	}

	return hoists
}

// operandOrderCovers is the fail-safe: every call, receive and early form the statement evaluates before its last hoist
// must be an event of the walk or lie inside a hoisted unit, or a later call moved ahead of it would run first. A
// statement holding one the walk did not see keeps C#'s order.
func operandOrderCovers(v *Visitor, events []operandOrderEvent, hoists []ast.Expr, stores []ast.Expr, exprs []ast.Expr) bool {
	seen := make(map[ast.Node]bool, len(events))

	for _, event := range events {
		seen[event.node] = true
	}

	lastHoist := hoists[len(hoists)-1]
	covered := true

	check := func(node ast.Node) {
		if seen[node] || node.Pos() >= lastHoist.End() {
			return
		}

		for _, hoist := range hoists {
			if node.Pos() >= hoist.Pos() && node.End() <= hoist.End() {
				return
			}
		}

		covered = false
	}

	for _, root := range append(append([]ast.Expr{}, stores...), exprs...) {
		ast.Inspect(root, func(node ast.Node) bool {
			if !covered {
				return false
			}

			switch n := node.(type) {
			case *ast.FuncLit:
				return false
			case *ast.UnaryExpr:
				if n.Op == token.ARROW {
					check(n)
				}
			case *ast.CallExpr:
				if v.operandCallKind(n) != operandCallTransparent {
					check(n)
				}
			case *ast.SliceExpr, *ast.TypeAssertExpr:
				check(n)
			case *ast.IndexExpr:
				if v.operandMapIndexByBytes(n) {
					check(n)
				}
			}

			return true
		})
	}

	return covered
}

func operandAncestorsWithin(inner, outer map[ast.Expr]bool) bool {
	for ancestor := range inner {
		if !outer[ancestor] {
			return false
		}
	}

	return true
}

// beginOperandOrder hoists the statement's calls that must run before its operands are read (operandOrderHoists) into
// the statement's prelude, v.hoistedDecls, and returns the function that ends the statement. A statement with no
// prelude sink, or one holding a multi-value call among the hoists, keeps C#'s order.
func (v *Visitor) beginOperandOrder(stores []ast.Expr, readStores bool, exprs ...ast.Expr) func() {
	if v.hoistedDecls == nil {
		return func() {}
	}

	hoists := v.operandOrderHoists(stores, readStores, exprs)

	if len(hoists) == 0 {
		return func() {}
	}

	for _, hoist := range hoists {
		if _, isTuple := v.getExprType(hoist).(*types.Tuple); isTuple {
			return func() {}
		}
	}

	if v.operandTemps == nil {
		v.operandTemps = map[ast.Expr]string{}
	}

	sink := v.hoistedDecls

	for _, hoist := range hoists {
		rendered := v.convExpr(hoist, nil)
		// A name of its own (`evalᴛN`, numbered per function), never the per-file tuple counter: a hoist that took a
		// tuple number would renumber every later tuple temp in the file. operandHoistCount tells a loop header that
		// its condition spilled (visitForStmt).
		name := v.getTempVarName("eval")
		v.operandHoistCount++

		// One leading newline per prelude: a hoist rendered after another (its argument) must not leave a blank line.
		if pending := sink.String(); pending == "" || !strings.HasSuffix(pending, v.newline) {
			sink.WriteString(v.newline)
		}

		sink.WriteString(v.indent(v.indentLevel))
		sink.WriteString(fmt.Sprintf("var %s = %s;", name, rendered))
		sink.WriteString(v.newline)
		v.operandHoistSink, v.operandHoistEnd = sink, sink.Len()

		v.operandTemps[hoist] = name
	}

	return func() {
		for _, hoist := range hoists {
			delete(v.operandTemps, hoist)
		}
	}
}

// beginOperandOrderPrelude hoists the calls of exprs (operandOrderHoists) and writes the prelude to the output at
// once, for a statement that writes its own leading newline after it (a local declaration, a switch). It returns the
// function that ends the statement.
func (v *Visitor) beginOperandOrderPrelude(exprs ...ast.Expr) func() {
	sink := &strings.Builder{}
	saved := v.hoistedDecls
	v.hoistedDecls = sink
	end := v.beginOperandOrder(nil, false, exprs...)
	v.hoistedDecls = saved

	if sink.Len() > 0 {
		v.outputBuilder.WriteString(strings.TrimSuffix(sink.String(), v.newline))
	}

	return end
}

// convIfCond converts the condition of an if statement or a loop, with the condition's calls hoisted ahead of its
// operands (operandOrder.go). The caller has set v.hoistedDecls to the condition's own sink.
func (v *Visitor) convIfCond(cond ast.Expr) string {
	endOperandOrder := v.beginOperandOrder(nil, false, cond)
	defer endOperandOrder()

	return v.convExpr(cond, nil)
}
