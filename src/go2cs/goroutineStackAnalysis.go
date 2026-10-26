// goroutineStackAnalysis.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"
	"sync"

	"golang.org/x/tools/go/packages"
)

// A goroutine runs on a thread of its own, and a .NET thread's stack is a FIXED reservation whose overflow kills the
// whole process, where Go's goroutine stack grows. So golib reserves a Go-scale stack for every goroutine
// (Goroutine.StackReserve, 256 MiB), and with 10⁵ goroutines parked at once that reservation is what makes them
// slow to start (docs/DESIGN-mn-scheduler-sizing.md §1). A goroutine whose call graph provably cannot recurse does
// not need the headroom: its depth is bounded by the graph, and golib's bounded reserve holds it. This file proves
// that per go statement, and visitGoStmt marks the goroutines it proves.
//
// The proof is conservative in one direction only, because an under-reserved goroutine crashes where Go would have
// grown its stack. A goroutine is bounded only when every call its body can make, transitively, resolves STATICALLY
// to a function whose body this load carries, and no chain of those calls reaches a function already on it.
// Everything the analysis cannot follow reads as "may recurse" and keeps the default reserve:
//   - a call through an interface method (a type parameter's constraint method included) or through a func value;
//   - a range-over-func loop, whose body the iterator calls;
//   - panic, print or println with a value that is not of a basic type (printing it may run its methods);
//   - a function without a body (assembly or linkname) that takes a func or an interface, which it may call, or, in
//     runtime, reflect, internal/reflectlite or syscall, whose body-less functions call back into Go (reflectcall,
//     OS callbacks), one that takes any pointer, unsafe.Pointer or uintptr;
//   - a call this load does not resolve.
// A function literal is followed only where it is called directly (`func() { … }()`, `defer func() { … }()`): passed
// or stored, it runs only when something calls it through a func value, which is the dynamic case above. A `go`
// statement inside a followed body is not followed into: its call runs on the new goroutine's stack, and only its
// operands are evaluated on this one.

// stackBoundCallbackPackages declare body-less functions that call back into Go code.
var stackBoundCallbackPackages = map[string]bool{
	"runtime":              true,
	"reflect":              true,
	"internal/reflectlite": true,
	"syscall":              true,
}

type stackBound struct {
	bounded bool
	why     string
}

// stackBoundsAnalysis answers for one loaded package graph. Its memo holds both answers: a body that reaches a function
// already on the walk is itself on that cycle, so "may recurse" is as much a property of the body as "bounded" is.
type stackBoundsAnalysis struct {
	loaded map[*types.Package]*packages.Package
	decls  map[*types.Package]map[*types.Func]*ast.FuncDecl
	memo   map[ast.Node]stackBound
}

// ONE cached analysis, keyed by the loaded root: keyed longer, it would pin every converted package's syntax for the
// whole run (see arrayDimsFactsFor).
var (
	stackBoundsLock sync.Mutex
	stackBoundsRoot *packages.Package
	stackBounds     *stackBoundsAnalysis
)

// goroutineStackBounded reports whether the goroutine `go call` starts provably cannot recurse; why names what defeated
// the proof otherwise.
func goroutineStackBounded(call *ast.CallExpr, info *types.Info) (bounded bool, why string) {
	stackBoundsLock.Lock()
	defer stackBoundsLock.Unlock()

	packageLock.Lock()
	root := currentLoadedPackage
	packageLock.Unlock()

	if root == nil || info == nil {
		return false, "no loaded package graph"
	}

	if stackBoundsRoot != root {
		stackBoundsRoot = root
		stackBounds = newStackBoundsAnalysis(root)
	}

	return stackBounds.callee(call, info, map[ast.Node]bool{})
}

func newStackBoundsAnalysis(root *packages.Package) *stackBoundsAnalysis {
	analysis := &stackBoundsAnalysis{
		loaded: map[*types.Package]*packages.Package{},
		decls:  map[*types.Package]map[*types.Func]*ast.FuncDecl{},
		memo:   map[ast.Node]stackBound{},
	}

	queue := []*packages.Package{root}

	for len(queue) > 0 {
		pkg := queue[0]
		queue = queue[1:]

		if pkg.Types == nil || analysis.loaded[pkg.Types] != nil {
			continue
		}

		analysis.loaded[pkg.Types] = pkg

		for _, imported := range pkg.Imports {
			queue = append(queue, imported)
		}
	}

	return analysis
}

// declOf finds fn's declaration and the type information of the package that declares it.
func (a *stackBoundsAnalysis) declOf(fn *types.Func) (*ast.FuncDecl, *types.Info) {
	pkg := a.loaded[fn.Pkg()]

	if pkg == nil || pkg.TypesInfo == nil {
		return nil, nil
	}

	decls, indexed := a.decls[fn.Pkg()]

	if !indexed {
		decls = map[*types.Func]*ast.FuncDecl{}

		for _, file := range pkg.Syntax {
			for _, decl := range file.Decls {
				if funcDecl, ok := decl.(*ast.FuncDecl); ok {
					if defined, ok := pkg.TypesInfo.Defs[funcDecl.Name].(*types.Func); ok {
						decls[defined] = funcDecl
					}
				}
			}
		}

		a.decls[fn.Pkg()] = decls
	}

	return decls[fn], pkg.TypesInfo
}

// callee answers for the call itself: what runs when call's function is invoked, its operands already evaluated.
func (a *stackBoundsAnalysis) callee(call *ast.CallExpr, info *types.Info, path map[ast.Node]bool) (bool, string) {
	fun := ast.Unparen(call.Fun)

	if literal, ok := fun.(*ast.FuncLit); ok {
		return a.body(literal, literal.Body, info, path, "a function literal")
	}

	if typeAndValue, ok := info.Types[fun]; ok && typeAndValue.IsType() {
		return true, ""
	}

	switch target := stackCalleeObject(fun, info).(type) {
	case *types.Builtin:
		switch target.Name() {
		case "panic", "print", "println":
			for _, arg := range call.Args {
				if _, basic := types.Unalias(info.TypeOf(arg)).(*types.Basic); !basic {
					return false, target.Name() + " of a non-basic value"
				}
			}
		}

		return true, ""
	case *types.Func:
		return a.function(target, path)
	}

	return false, "a call through a func value"
}

// stackCalleeObject is the object a call's function expression names, or nil when it names none (a func value
// computed by an expression).
func stackCalleeObject(fun ast.Expr, info *types.Info) types.Object {
	switch instance := fun.(type) {
	case *ast.IndexExpr:
		fun = ast.Unparen(instance.X)
	case *ast.IndexListExpr:
		fun = ast.Unparen(instance.X)
	}

	switch expr := fun.(type) {
	case *ast.Ident:
		return info.Uses[expr]
	case *ast.SelectorExpr:
		if selection := info.Selections[expr]; selection != nil {
			if selection.Kind() == types.FieldVal {
				return nil
			}

			return selection.Obj()
		}

		return info.Uses[expr.Sel]
	}

	return nil
}

func (a *stackBoundsAnalysis) function(fn *types.Func, path map[ast.Node]bool) (bool, string) {
	fn = fn.Origin()
	signature, _ := fn.Type().(*types.Signature)

	if signature != nil && signature.Recv() != nil && types.IsInterface(signature.Recv().Type()) {
		return false, "an interface method call, " + fn.FullName()
	}

	decl, info := a.declOf(fn)

	if decl == nil {
		return false, "an unresolved function, " + fn.FullName()
	}

	if decl.Body != nil {
		return a.body(decl, decl.Body, info, path, fn.FullName())
	}

	// Where body-less functions call back into Go, a code address can also travel as a raw pointer or a uintptr
	// (reflectcall's fn, a syscall's callback).
	rawAddresses := fn.Pkg() != nil && stackBoundCallbackPackages[fn.Pkg().Path()]

	if signature != nil {
		for i := range signature.Params().Len() {
			if stackBoundMayHoldCode(signature.Params().At(i).Type(), rawAddresses) {
				return false, "a body-less function given code it may call, " + fn.FullName()
			}
		}
	}

	return true, ""
}

// stackBoundMayHoldCode reports whether a value of t can carry code its receiver might call: a func, an interface, or a
// slice or array of either (a variadic ...any); with rawAddresses, also an unsafe.Pointer, a uintptr or a pointer.
func stackBoundMayHoldCode(t types.Type, rawAddresses bool) bool {
	switch underlying := t.Underlying().(type) {
	case *types.Signature, *types.Interface:
		return true
	case *types.Slice:
		return stackBoundMayHoldCode(underlying.Elem(), rawAddresses)
	case *types.Array:
		return stackBoundMayHoldCode(underlying.Elem(), rawAddresses)
	case *types.Pointer:
		return rawAddresses
	case *types.Basic:
		return rawAddresses && (underlying.Kind() == types.UnsafePointer || underlying.Kind() == types.Uintptr)
	}

	return false
}

func (a *stackBoundsAnalysis) body(node ast.Node, block *ast.BlockStmt, info *types.Info, path map[ast.Node]bool, name string) (bool, string) {
	if known, ok := a.memo[node]; ok {
		return known.bounded, known.why
	}

	if path[node] {
		return false, "recursion through " + name
	}

	path[node] = true
	bounded, why := a.walk(block, info, path)
	delete(path, node)

	a.memo[node] = stackBound{bounded, why}

	return bounded, why
}

// walk follows every call root makes on this goroutine's stack.
func (a *stackBoundsAnalysis) walk(root ast.Node, info *types.Info, path map[ast.Node]bool) (bounded bool, why string) {
	bounded = true

	var visit func(node ast.Node) bool

	visit = func(node ast.Node) bool {
		if !bounded {
			return false
		}

		switch stmt := node.(type) {
		case *ast.FuncLit:
			// Runs only when called: a direct call reaches it through its CallExpr below.
			return false
		case *ast.GoStmt:
			// The call runs on the new goroutine's stack; only its operands are evaluated on this one.
			ast.Inspect(stmt.Call.Fun, visit)

			for _, arg := range stmt.Call.Args {
				ast.Inspect(arg, visit)
			}

			return false
		case *ast.RangeStmt:
			if stackBoundMayBeFunc(info.TypeOf(stmt.X)) {
				bounded, why = false, "a range-over-func loop"
				return false
			}
		case *ast.CallExpr:
			if ok, reason := a.callee(stmt, info, path); !ok {
				bounded, why = false, reason
				return false
			}
		}

		return true
	}

	ast.Inspect(root, visit)

	return bounded, why
}

// stackBoundMayBeFunc reports whether ranging over a value of t can call an iterator: a func, or a type parameter
// whose constraint admits one.
func stackBoundMayBeFunc(t types.Type) bool {
	if t == nil {
		return false
	}

	if typeParam, ok := types.Unalias(t).(*types.TypeParam); ok {
		constraint, _ := typeParam.Constraint().Underlying().(*types.Interface)

		if constraint == nil {
			return true
		}

		for i := range constraint.NumEmbeddeds() {
			embedded := constraint.EmbeddedType(i)

			if union, ok := embedded.(*types.Union); ok {
				for j := range union.Len() {
					if stackBoundMayBeFunc(union.Term(j).Type()) {
						return true
					}
				}
			} else if stackBoundMayBeFunc(embedded) {
				return true
			}
		}

		return false
	}

	_, isFunc := t.Underlying().(*types.Signature)

	return isFunc
}
