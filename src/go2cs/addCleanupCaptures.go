// addCleanupCaptures.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// THE CAPTURE FACT FOR runtime.AddCleanup'S CLOSES-OVER-PTR CHECK (Go 1.26).
//
// Go's AddCleanup panics "cleanup function closes over ptr, cleanup will never run" when a pointer
// slot of the cleanup's funcval object addresses ptr's allocation (mcleanup.go). The managed
// AddCleanup (core/runtime/mcleanup.cs) cannot read a C# delegate's captures without reflection, and
// a C# closure object also holds variables that only a SIBLING lambda of its scope captured. The
// converter knows exactly which Go variables the cleanup closes over, so at the call site it passes
// their values as a fourth argument, `new object?[] { … }`, and golib checks each one. The answer is
// the same under the JIT and native AOT.
//
// WHAT IS PASSED, following what Go's funcval holds:
//   - a func LITERAL: each free variable captured BY VALUE whose type is a pointer, unsafe.Pointer
//     or interface. A variable Go
//     captures BY REFERENCE (one assigned after its declaration, or whose address is taken) puts its
//     heap cell in the funcval, never ptr, so it is not passed: Go accepts that cleanup and so do we.
//   - a METHOD VALUE `x.m` with x a variable: its receiver, which Go's funcval binds by value.
//   - a func-typed LOCAL whose every assignment in its function is one of the two above: the union
//     of their captures (funcVariableCaptures). All 20 func-variable call sites in the go1.26.9
//     tree have this shape.
// The current method's own pointer receiver is passed as its box, never its dereferenced name (see
// captureValueText).
// Nothing else is passed: a method expression or package function has no captures, and any other
// func value (a parameter, a package-level variable, a field, a call result) has no static fact and
// stays unchecked. That residual is 0 call sites in the go1.26.9 tree.
// A STRUCT or ARRAY captured by value is passed when some field or element, at any depth, is a pointer
// slot. It goes in a fifth argument, and golib walks it through GoReflect.GoFields (captureKindOf).
// Slices, maps, channels, funcs and strings are not passed: Go's slot scan sees their header
// pointer, not ptr's object. The one exception Go refuses and this does not is a slice over ptr's
// own array (BOARD row). When nothing qualifies, the call keeps its three-argument form.

// addCleanupCaptureArgs returns the C# text of the extra AddCleanup arguments for callExpr, or ""
// when callExpr is not a call to runtime.AddCleanup or its cleanup carries no qualifying capture.
// Pointer, unsafe.Pointer and interface captures go in the fourth argument. Struct and array
// captures holding a pointer slot go in a fifth, which golib walks field by field: kept apart so an
// interface capture whose dynamic value is a struct is never walked (Go's slot scan sees only the
// interface's data word, which addresses a copy).
func (v *Visitor) addCleanupCaptureArgs(callExpr *ast.CallExpr) string {
	if len(callExpr.Args) != 3 || !v.isRuntimeAddCleanup(callExpr.Fun) {
		return ""
	}

	var captures []*ast.Ident

	switch cleanup := ast.Unparen(callExpr.Args[1]).(type) {
	case *ast.FuncLit:
		captures = v.funcLitByValueCaptures(cleanup)
	case *ast.SelectorExpr:
		if receiver := v.methodValueReceiver(cleanup); receiver != nil {
			captures = []*ast.Ident{receiver}
		}
	case *ast.Ident:
		captures = v.funcVariableCaptures(cleanup, callExpr.Pos())
	}

	var pointers, composites []string

	for _, ident := range captures {
		kind := v.captureKindOf(v.info.TypeOf(ident))

		if kind == captureNone {
			continue
		}

		text := v.captureValueText(ident)

		if text == "" {
			continue
		}

		if kind == capturePointer {
			pointers = append(pointers, text)
		} else {
			composites = append(composites, text)
		}
	}

	if len(composites) > 0 {
		first := "null"

		if len(pointers) > 0 {
			first = "new object?[] { " + strings.Join(pointers, ", ") + " }"
		}

		return first + ", new object?[] { " + strings.Join(composites, ", ") + " }"
	}

	if len(pointers) == 0 {
		return ""
	}

	return "new object?[] { " + strings.Join(pointers, ", ") + " }"
}

// methodValueReceiver returns the receiver variable of a method value `x.m` whose receiver can
// address an allocation (a pointer, an interface, or a struct or array holding a pointer slot), or nil for anything else (a method expression, a package function, a
// field, or a receiver that is not a plain variable).
func (v *Visitor) methodValueReceiver(selector *ast.SelectorExpr) *ast.Ident {
	if selection := v.info.Selections[selector]; selection == nil || selection.Kind() != types.MethodVal {
		return nil
	}

	if receiver, ok := ast.Unparen(selector.X).(*ast.Ident); ok && v.captureKindOf(v.info.TypeOf(receiver)) != captureNone {
		return receiver
	}

	return nil
}

// funcVariableCaptures returns the captures to pass for a cleanup named by a func-typed VARIABLE,
// or nil when the converter cannot prove what the variable holds. A LOCAL of the current function
// whose every assignment there is a func literal or a method value can hold only those values, so
// the UNION of their captures is passed. A superset is safe: a literal that could flow there and
// closes over ptr is one Go would refuse. The residual stays unchecked: a parameter, a package-level
// variable, a local assigned anything else (a call, a field, another variable, several values at
// once) or whose address is taken. In the go1.26.9 tree all 20 func-variable call sites are covered
// and the residual is 0 (a census by this predicate, mcleanup.cs divergence 5).
//
// A capture is passed only if its name resolves to the same variable at the call site (callPos).
// One that is out of scope or shadowed there is dropped, so the call stays compilable.
func (v *Visitor) funcVariableCaptures(ident *ast.Ident, callPos token.Pos) []*ast.Ident {
	local, ok := v.info.Uses[ident].(*types.Var)
	decl := v.currentFuncDecl

	if !ok || local.Pkg() == nil || local.Parent() == local.Pkg().Scope() || decl == nil || local.Pos() < decl.Pos() || local.Pos() >= decl.End() {
		return nil
	}

	var sources []ast.Expr
	proven := true

	assigned := func(value ast.Expr) {
		switch value := ast.Unparen(value).(type) {
		case *ast.FuncLit:
			sources = append(sources, value)
		case *ast.SelectorExpr:
			if selection := v.info.Selections[value]; selection != nil && selection.Kind() == types.MethodVal {
				sources = append(sources, value)
			} else {
				proven = false
			}
		default:
			proven = false
		}
	}

	ast.Inspect(decl, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.Field:
			for _, name := range n.Names {
				if v.info.Defs[name] == local {
					proven = false // a parameter or result
				}
			}
		case *ast.AssignStmt:
			for index, lhs := range n.Lhs {
				if name, ok := lhs.(*ast.Ident); !ok || (v.info.Defs[name] != local && v.info.Uses[name] != local) {
					continue
				}

				if len(n.Rhs) != len(n.Lhs) {
					proven = false
				} else {
					assigned(n.Rhs[index])
				}
			}
		case *ast.ValueSpec:
			for index, name := range n.Names {
				if v.info.Defs[name] != local || len(n.Values) == 0 {
					continue // a declaration without a value holds nil, which captures nothing
				}

				if len(n.Values) != len(n.Names) {
					proven = false
				} else {
					assigned(n.Values[index])
				}
			}
		case *ast.UnaryExpr:
			if n.Op == token.AND && v.namesVar(n.X, local) {
				proven = false
			}
		case *ast.RangeStmt:
			if v.namesVar(n.Key, local) || v.namesVar(n.Value, local) {
				proven = false
			}
		}

		return proven
	})

	if !proven {
		return nil
	}

	var result []*ast.Ident
	seen := map[types.Object]bool{}
	scope := v.pkg.Scope().Innermost(callPos)

	add := func(capture *ast.Ident) {
		object := v.info.Uses[capture]

		if object == nil || seen[object] || scope == nil {
			return
		}

		if _, resolved := scope.LookupParent(capture.Name, callPos); resolved != object {
			return
		}

		seen[object] = true
		result = append(result, capture)
	}

	for _, source := range sources {
		switch source := source.(type) {
		case *ast.FuncLit:
			for _, capture := range v.funcLitByValueCaptures(source) {
				if v.captureKindOf(v.info.TypeOf(capture)) != captureNone {
					add(capture)
				}
			}
		case *ast.SelectorExpr:
			if receiver := v.methodValueReceiver(source); receiver != nil {
				add(receiver)
			}
		}
	}

	return result
}

// isRuntimeAddCleanup reports whether fun names the runtime package's AddCleanup, instantiated or not.
func (v *Visitor) isRuntimeAddCleanup(fun ast.Expr) bool {
	if index, ok := fun.(*ast.IndexListExpr); ok {
		fun = index.X
	} else if index, ok := fun.(*ast.IndexExpr); ok {
		fun = index.X
	}

	var ident *ast.Ident

	switch f := fun.(type) {
	case *ast.SelectorExpr:
		ident = f.Sel
	case *ast.Ident:
		ident = f
	default:
		return false
	}

	fn, ok := v.info.Uses[ident].(*types.Func)

	return ok && fn.Name() == "AddCleanup" && fn.Pkg() != nil && fn.Pkg().Path() == "runtime"
}

// funcLitByValueCaptures returns, in order of first use, one identifier per local variable declared
// outside lit that lit's body reads and that Go captures by value.
func (v *Visitor) funcLitByValueCaptures(lit *ast.FuncLit) []*ast.Ident {
	var result []*ast.Ident
	seen := map[*types.Var]bool{}

	ast.Inspect(lit.Body, func(node ast.Node) bool {
		ident, ok := node.(*ast.Ident)
		if !ok {
			return true
		}

		local, ok := v.info.Uses[ident].(*types.Var)
		if !ok || seen[local] || local.IsField() || local.Pkg() == nil || local.Parent() == local.Pkg().Scope() {
			return true
		}

		if local.Pos() >= lit.Pos() && local.Pos() < lit.End() {
			return true
		}

		seen[local] = true

		if !v.capturedByReference(local) {
			result = append(result, ident)
		}

		return true
	})

	return result
}

// capturedByReference reports Go's capture mode for a local the way cmd/compile decides it: by
// reference when the variable is assigned anywhere after its declaration or its address is taken.
// (The compiler also captures a value larger than 128 bytes by reference; captureKindOf applies that
// rule, which only a struct or array can reach.)
func (v *Visitor) capturedByReference(local *types.Var) bool {
	scope := local.Parent()
	if scope == nil {
		return true
	}

	// A local is declared, assigned and addressed only inside its enclosing function. Without one
	// (a package-level func literal), answer by reference: the variable is then not passed, which
	// can only leave a cleanup unchecked, never refuse one Go accepts.
	decl := v.currentFuncDecl
	if decl == nil || decl.Pos() > scope.Pos() || decl.End() < scope.End() {
		return true
	}

	found := false

	{
		ast.Inspect(decl, func(node ast.Node) bool {
			if found || node == nil || node.End() < scope.Pos() || node.Pos() > scope.End() {
				return false
			}

			switch n := node.(type) {
			case *ast.AssignStmt:
				if n.Tok != token.DEFINE {
					for _, lhs := range n.Lhs {
						if v.namesVar(lhs, local) {
							found = true
						}
					}
				} else {
					// A `:=` redeclaring an existing variable assigns it.
					for _, lhs := range n.Lhs {
						if ident, ok := lhs.(*ast.Ident); ok && v.info.Uses[ident] == local {
							found = true
						}
					}
				}
			case *ast.IncDecStmt:
				if v.namesVar(n.X, local) {
					found = true
				}
			case *ast.RangeStmt:
				if n.Tok == token.ASSIGN && (v.namesVar(n.Key, local) || v.namesVar(n.Value, local)) {
					found = true
				}
			case *ast.UnaryExpr:
				if n.Op == token.AND && v.namesVar(n.X, local) {
					found = true
				}
			}

			return !found
		})
	}

	return found
}

// namesVar reports whether expr is a bare use of local (parentheses aside).
func (v *Visitor) namesVar(expr ast.Expr, local *types.Var) bool {
	if expr == nil {
		return false
	}

	ident, ok := ast.Unparen(expr).(*ast.Ident)

	return ok && v.info.Uses[ident] == local
}

// captureValueText renders a captured pointer or interface variable's VALUE at the call site, or ""
// when it has none to give. The current method's pointer receiver is the one special case: its name
// is the dereferenced alias (`ref var c = ref Ꮡc.DerefOrNull()`), and the pointer is the receiver
// box `Ꮡc` that a direct-ж method carries (convIdent's pointer-context rule). Rendering the name
// would pass a struct COPY, and would also make the converter emit the alias prologue. A ref
// receiver (`this ref T c`) has no box, so it yields no value and is not checked.
func (v *Visitor) captureValueText(ident *ast.Ident) string {
	if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(ident, recvName) {
		if !isDirectBoxReceiverMethod(v.currentFuncDecl, v.info) {
			return ""
		}

		return AddressPrefix + strings.TrimPrefix(v.getIdentName(ident), "@")
	}

	return v.convExpr(ident, nil)
}

// The captured kinds this check passes (see the header).
const (
	captureNone      = iota // nothing in it can address ptr's allocation
	capturePointer          // a pointer, unsafe.Pointer or interface: checked as itself
	captureComposite        // a struct or array with a pointer slot: walked field by field in golib
)

// maxByValueCaptureSize is cmd/compile's limit for capturing a variable by VALUE: a larger one is
// captured by reference, so its funcval slot addresses a heap cell, never ptr.
const maxByValueCaptureSize = 128

// captureKindOf classifies a by-value capture of type typ. A struct or array is a composite only if
// some field or element, at any depth, is a pointer slot, and only within the by-value size limit
// (sized for the target GOARCH); a slice, map, channel, func or string is never walked, since Go's
// slot scan sees its header pointer, not ptr's object.
func (v *Visitor) captureKindOf(typ types.Type) int {
	if typ == nil {
		return captureNone
	}

	if isPointerSlot(typ) {
		return capturePointer
	}

	if !holdsPointerSlot(typ, map[types.Type]bool{}) {
		return captureNone
	}

	sizes := types.SizesFor("gc", platformArch(v.options.targetPlatform))

	if sizes == nil {
		sizes = types.SizesFor("gc", "amd64")
	}

	if sizes.Sizeof(typ) > maxByValueCaptureSize {
		return captureNone
	}

	return captureComposite
}

// isPointerSlot reports whether a value of typ is one pointer slot: a pointer, unsafe.Pointer or
// interface.
func isPointerSlot(typ types.Type) bool {
	switch under := typ.Underlying().(type) {
	case *types.Pointer, *types.Interface:
		return true
	case *types.Basic:
		return under.Kind() == types.UnsafePointer
	}

	return false
}

// holdsPointerSlot reports whether a struct or array of type typ has a pointer slot among its fields
// or elements, at any depth. seen stops a recursive type.
func holdsPointerSlot(typ types.Type, seen map[types.Type]bool) bool {
	if seen[typ] {
		return false
	}

	seen[typ] = true

	switch under := typ.Underlying().(type) {
	case *types.Struct:
		for index := range under.NumFields() {
			field := under.Field(index).Type()

			if isPointerSlot(field) || holdsPointerSlot(field, seen) {
				return true
			}
		}
	case *types.Array:
		return under.Len() > 0 && (isPointerSlot(under.Elem()) || holdsPointerSlot(under.Elem(), seen))
	}

	return false
}
