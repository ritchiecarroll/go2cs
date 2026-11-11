// sliceWideningWriteRefusals.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The refusal BY NAME of the element-wise []N widening where the callee may write the slice, checked before any file
// is written (docs/phase4/DESIGN-slice-widening-write-refusal.md).
//
// A generic parameter declared []N, N constrained by a method-set interface, given a []*T: convCallExpr passes the
// argument as an element-wise copy through the generated adapter (`widen<ж<T>, I>(all, elemᴛ0 => new TжI(elemᴛ0))`).
// The copy shares each element's box, so method calls reach the caller's objects, but it does not share the slice: an
// element assignment, an append into the backing array, a sort, or a hand-off to anything that does those, writes the
// copy. Go's `reverse(all)` reverses all; the converted program would leave it as it was, with no diagnostic. So a
// site whose callee is not PROVEN read-only for that parameter is refused with GO2CS2006.
//
// The trigger is elementWiseSliceWidening, the one function convCallExpr's emission calls too, so the gate examines
// every argument the emitter widens. The emitter also needs an adapter class for the pair, so it widens a subset of
// the arguments the trigger names; the gate examines them all.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"math"
	"path/filepath"
	"sort"

	"golang.org/x/tools/go/packages"
)

// elementWiseSliceWidening reports whether argument i of callExpr takes convCallExpr's element-wise []N widening: the
// callee's declared parameter is a slice of a type parameter whose constraint is a method-set interface with methods,
// and the parameter is instantiated here with a slice of a pointer type that implements it. It returns the type
// parameter and the instantiated pointer element, or nil, nil.
func (v *Visitor) elementWiseSliceWidening(callExpr *ast.CallExpr, i int) (*types.TypeParam, *types.Pointer) {
	if i >= len(callExpr.Args) {
		return nil, nil
	}

	funcSignature := v.getFunctionSignature(callExpr)

	if funcSignature == nil {
		return nil, nil
	}

	paramType, ok := getParameterType(funcSignature, i)

	if !ok {
		return nil, nil
	}

	declSlice, ok := paramType.Underlying().(*types.Slice)

	if !ok {
		return nil, nil
	}

	tp, ok := types.Unalias(declSlice.Elem()).(*types.TypeParam)

	if !ok {
		return nil, nil
	}

	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 || !iface.IsMethodSet() {
		return nil, nil
	}

	instParam := v.instantiatedParamType(callExpr, i)

	if instParam == nil {
		return nil, nil
	}

	instSlice, ok := instParam.Underlying().(*types.Slice)

	if !ok {
		return nil, nil
	}

	ptrElem, ok := instSlice.Elem().(*types.Pointer)

	if !ok || !types.Implements(ptrElem, iface) {
		return nil, nil
	}

	return tp, ptrElem
}

// sliceWideningSite is one argument the trigger names, with the callee's verdict for the receiving parameter.
type sliceWideningSite struct {
	position token.Position // the widened argument
	arg      string         // the argument as written
	text     string         // the diagnostic text; empty when the callee is proven read-only
}

// sliceWideningRefusal is the error refuseSliceWideningWrites returns: one canonical diagnostic per refused site.
type sliceWideningRefusal struct {
	pkgPath string
	sites   []sliceWideningSite
}

func (refusal *sliceWideningRefusal) Error() string {
	return fmt.Sprintf("package %s passes a slice through the element-wise []N widening to a callee that may write it (%d site(s)); see the %s diagnostics",
		refusal.pkgPath, len(refusal.sites), diagnosticSliceWideningRefused)
}

// report prints each refused site as a canonical diagnostic.
func (refusal *sliceWideningRefusal) report() {
	for _, site := range refusal.sites {
		reportDiagnostic(site.position.String(), diagnosticSliceWideningRefused, site.text)
	}
}

// refuseSliceWideningWrites finds every widened argument whose callee may write it. It returns nil when there is none.
func refuseSliceWideningWrites(pkg *packages.Package) *sliceWideningRefusal {
	var refused []sliceWideningSite

	for _, site := range sliceWideningSites(pkg) {
		if site.text != "" {
			refused = append(refused, site)
		}
	}

	if len(refused) == 0 {
		return nil
	}

	return &sliceWideningRefusal{pkgPath: pkg.PkgPath, sites: refused}
}

// sliceWideningSites is every argument in the package that elementWiseSliceWidening names, in source order, each with
// its verdict.
func sliceWideningSites(pkg *packages.Package) []sliceWideningSite {
	if pkg.TypesInfo == nil {
		return nil
	}

	v := &Visitor{info: pkg.TypesInfo}
	analysis := newSliceWriteAnalysis(pkg)

	var sites []sliceWideningSite

	for _, file := range pkg.Syntax {
		ast.Inspect(file, func(node ast.Node) bool {
			callExpr, ok := node.(*ast.CallExpr)

			if !ok {
				return true
			}

			for i, arg := range callExpr.Args {
				tp, _ := v.elementWiseSliceWidening(callExpr, i)

				if tp == nil {
					continue
				}

				site := sliceWideningSite{position: pkg.Fset.Position(arg.Pos()), arg: types.ExprString(arg)}
				callee, _ := pkg.TypesInfo.Uses[calleeIdent(callExpr.Fun)].(*types.Func)

				var write string

				if callee == nil {
					write = "it is not a declared function, so its use of the slice is not known"
				} else {
					write = analysis.sliceArgWrite(callee.Origin(), i)
				}

				if write != "" {
					name := analysis.funcName(callee)
					site.text = fmt.Sprintf("%s(...) receives %s as []%s; %s may write it: %s",
						name, site.arg, tp.Obj().Name(), name, write)
				}

				sites = append(sites, site)
			}

			return true
		})
	}

	sort.SliceStable(sites, func(a, b int) bool {
		if sites[a].position.Filename != sites[b].position.Filename {
			return sites[a].position.Filename < sites[b].position.Filename
		}

		return sites[a].position.Offset < sites[b].position.Offset
	})

	return sites
}

// calleeIdent is the identifier naming a call's function, through a selector, an explicit instantiation or
// parentheses; nil for any other callee expression.
func calleeIdent(fun ast.Expr) *ast.Ident {
	switch expr := fun.(type) {
	case *ast.Ident:
		return expr
	case *ast.SelectorExpr:
		return expr.Sel
	case *ast.IndexExpr:
		return calleeIdent(expr.X)
	case *ast.IndexListExpr:
		return calleeIdent(expr.X)
	case *ast.ParenExpr:
		return calleeIdent(expr.X)
	}

	return nil
}

// sliceParamKey names one parameter of one function declaration.
type sliceParamKey struct {
	fn    *types.Func
	index int
}

// noCycleDepth is the cycle depth of a verdict that assumed nothing about a function still being analysed.
const noCycleDepth = math.MaxInt

// sliceWriteAnalysis answers, for a function and one of its parameters, whether every use of that parameter in the
// function's body (closures included) is proven a READ of the caller's slice, following the slice through statically
// called declared functions. Anything not proven is a write. The rules are DESIGN-slice-widening-write-refusal.md's
// section 3.
type sliceWriteAnalysis struct {
	root  *packages.Package
	fset  *token.FileSet
	decls map[*types.Package]map[*types.Func]*ast.FuncDecl
	infos map[*types.Package]*types.Info
	memo  map[sliceParamKey]string
	depth map[sliceParamKey]int
}

func newSliceWriteAnalysis(root *packages.Package) *sliceWriteAnalysis {
	return &sliceWriteAnalysis{
		root:  root,
		fset:  root.Fset,
		decls: map[*types.Package]map[*types.Func]*ast.FuncDecl{},
		infos: map[*types.Package]*types.Info{},
		memo:  map[sliceParamKey]string{},
		depth: map[sliceParamKey]int{},
	}
}

// sliceArgWrite is "" when fn is proven read-only for its parameter index, else the first use that is not.
func (analysis *sliceWriteAnalysis) sliceArgWrite(fn *types.Func, index int) string {
	write, _ := analysis.classify(fn, index)
	return write
}

// funcName is fn's name as a diagnostic names it: qualified by its package when that is not the one converting.
func (analysis *sliceWriteAnalysis) funcName(fn *types.Func) string {
	if fn == nil {
		return "the callee"
	}

	if fn.Pkg() != nil && fn.Pkg() != analysis.root.Types {
		return fn.Pkg().Name() + "." + fn.Name()
	}

	return fn.Name()
}

// at is a position as a write reason names it: the file's base name and the line.
func (analysis *sliceWriteAnalysis) at(node ast.Node) string {
	position := analysis.fset.Position(node.Pos())
	return fmt.Sprintf("%s:%d", filepath.Base(position.Filename), position.Line)
}

// classify returns fn's verdict for its parameter index, and the smallest analysis depth of any function still being
// analysed that the verdict ASSUMED read-only (noCycleDepth when it assumed none). A recursive cycle adds no write of
// its own, so a parameter that reaches itself again is assumed read-only there; a read-only verdict that rests on an
// assumption about a function deeper in the cycle than itself is not memoised, because that function may still turn
// out to write. A write is a write whatever was assumed, and is always memoised.
func (analysis *sliceWriteAnalysis) classify(fn *types.Func, index int) (string, int) {
	key := sliceParamKey{fn, index}

	if write, ok := analysis.memo[key]; ok {
		return write, noCycleDepth
	}

	if depth, ok := analysis.depth[key]; ok {
		return "", depth
	}

	decl, info := analysis.source(fn)

	if decl == nil || decl.Body == nil || info == nil {
		write := fmt.Sprintf("the body of %s is not available to show it only reads the slice", analysis.funcName(fn))
		analysis.memo[key] = write
		return write, noCycleDepth
	}

	params := fn.Type().(*types.Signature).Params()

	if index >= params.Len() {
		write := fmt.Sprintf("%s has no parameter %d", analysis.funcName(fn), index)
		analysis.memo[key] = write
		return write, noCycleDepth
	}

	myDepth := len(analysis.depth)
	analysis.depth[key] = myDepth
	defer delete(analysis.depth, key)

	param := params.At(index)
	write, low := "", noCycleDepth

	var stack []ast.Node

	ast.Inspect(decl.Body, func(node ast.Node) bool {
		if node == nil {
			stack = stack[:len(stack)-1]
			return false
		}

		if write != "" {
			return false
		}

		stack = append(stack, node)

		if ident, ok := node.(*ast.Ident); ok && info.Uses[ident] == param {
			useWrite, useLow := analysis.useWrite(info, stack)
			write = useWrite
			low = min(low, useLow)
		}

		return true
	})

	if write != "" || low >= myDepth {
		analysis.memo[key] = write
		return write, noCycleDepth
	}

	return write, low
}

// source is fn's declaration and the types.Info that types it, from the package being converted or any package the
// load reached (the driver loads with LoadAllSyntax, so every dependency carries syntax). Nil when there is none.
func (analysis *sliceWriteAnalysis) source(fn *types.Func) (*ast.FuncDecl, *types.Info) {
	owner := fn.Pkg()

	if owner == nil {
		return nil, nil
	}

	decls, cached := analysis.decls[owner]

	if !cached {
		decls = map[*types.Func]*ast.FuncDecl{}

		if loaded := loadedPackageInGraph(analysis.root, owner); loaded != nil && loaded.TypesInfo != nil {
			for _, file := range loaded.Syntax {
				for _, decl := range file.Decls {
					if funcDecl, ok := decl.(*ast.FuncDecl); ok {
						if object, ok := loaded.TypesInfo.Defs[funcDecl.Name].(*types.Func); ok {
							decls[object] = funcDecl
						}
					}
				}
			}

			analysis.infos[owner] = loaded.TypesInfo
		}

		analysis.decls[owner] = decls
	}

	return decls[fn], analysis.infos[owner]
}

// loadedPackageInGraph is the loaded package whose types are target, searched from root through every import,
// transitively; nil when the load did not reach it.
func loadedPackageInGraph(root *packages.Package, target *types.Package) *packages.Package {
	seen := map[*packages.Package]bool{}
	queue := []*packages.Package{root}

	for len(queue) > 0 {
		pkg := queue[0]
		queue = queue[1:]

		if seen[pkg] {
			continue
		}

		seen[pkg] = true

		if pkg.Types == target {
			return pkg
		}

		paths := make([]string, 0, len(pkg.Imports))

		for path := range pkg.Imports {
			paths = append(paths, path)
		}

		sort.Strings(paths)

		for _, path := range paths {
			queue = append(queue, pkg.Imports[path])
		}
	}

	return nil
}

// useWrite is "" when the use of the parameter at the top of stack is proven a read, else what it is, with the cycle
// depth of any verdict it borrowed from a callee.
func (analysis *sliceWriteAnalysis) useWrite(info *types.Info, stack []ast.Node) (string, int) {
	ident := stack[len(stack)-1].(*ast.Ident)
	parent := stack[len(stack)-2]

	var grandparent ast.Node

	if len(stack) >= 3 {
		grandparent = stack[len(stack)-3]
	}

	switch parent := parent.(type) {
	case *ast.AssignStmt:
		// The parameter itself on the left: rebinding the local writes nothing of the caller's slice. Later writes
		// through the rebound local are caught by their own rule.
		for _, lhs := range parent.Lhs {
			if lhs == ident {
				return "", noCycleDepth
			}
		}

		return "it is assigned to another variable at " + analysis.at(parent), noCycleDepth

	case *ast.RangeStmt:
		if parent.X == ident {
			return "", noCycleDepth
		}

	case *ast.BinaryExpr:
		if (parent.Op == token.EQL || parent.Op == token.NEQ) && (isNilOperand(info, parent.X) || isNilOperand(info, parent.Y)) {
			return "", noCycleDepth
		}

	case *ast.IndexExpr:
		if parent.X != ident {
			break
		}

		switch grandparent := grandparent.(type) {
		case *ast.AssignStmt:
			for _, lhs := range grandparent.Lhs {
				if lhs == parent {
					return "an element is assigned at " + analysis.at(grandparent), noCycleDepth
				}
			}
		case *ast.IncDecStmt:
			return "an element is incremented or decremented at " + analysis.at(grandparent), noCycleDepth
		case *ast.UnaryExpr:
			if grandparent.Op == token.AND {
				return "an element's address is taken at " + analysis.at(grandparent), noCycleDepth
			}
		}

		return "", noCycleDepth

	case *ast.SliceExpr:
		if parent.X != ident {
			break
		}

		// p[:0:0]: a zero-capacity view, through which nothing can reach the caller's backing array.
		if parent.Slice3 && isZeroLiteral(parent.High) && isZeroLiteral(parent.Max) && (parent.Low == nil || isZeroLiteral(parent.Low)) {
			return "", noCycleDepth
		}

		// p[a:b] read in place: a range operand, a len or cap argument, append's spread source, or the parameter's own
		// reslice (`p = p[1:]`).
		switch grandparent := grandparent.(type) {
		case *ast.AssignStmt:
			if len(grandparent.Lhs) == len(grandparent.Rhs) {
				for i, rhs := range grandparent.Rhs {
					if lhs, ok := grandparent.Lhs[i].(*ast.Ident); ok && rhs == parent && info.Uses[lhs] == info.Uses[ident] {
						return "", noCycleDepth
					}
				}
			}
		case *ast.RangeStmt:
			if grandparent.X == parent {
				return "", noCycleDepth
			}
		case *ast.CallExpr:
			if builtin, ok := info.Uses[calleeIdent(grandparent.Fun)].(*types.Builtin); ok {
				switch builtin.Name() {
				case "len", "cap":
					return "", noCycleDepth
				case "append":
					if grandparent.Ellipsis.IsValid() && len(grandparent.Args) == 2 && grandparent.Args[1] == parent {
						return "", noCycleDepth
					}
				}
			}
		}

		return "it is resliced and the reslice kept at " + analysis.at(parent), noCycleDepth

	case *ast.CallExpr:
		return analysis.callArgWrite(info, parent, ident)

	case *ast.ReturnStmt:
		return "it is returned at " + analysis.at(parent), noCycleDepth
	}

	return "it is used where a read is not proven at " + analysis.at(parent), noCycleDepth
}

// callArgWrite is useWrite for the parameter as an argument of call.
func (analysis *sliceWriteAnalysis) callArgWrite(info *types.Info, call *ast.CallExpr, ident *ast.Ident) (string, int) {
	if builtin, ok := info.Uses[calleeIdent(call.Fun)].(*types.Builtin); ok {
		switch builtin.Name() {
		case "len", "cap":
			return "", noCycleDepth
		case "append":
			if call.Ellipsis.IsValid() && len(call.Args) == 2 && call.Args[1] == ident {
				return "", noCycleDepth
			}

			return "it is append's destination at " + analysis.at(call), noCycleDepth
		case "copy":
			if len(call.Args) == 2 && call.Args[1] == ident {
				return "", noCycleDepth
			}

			return "it is copy's destination at " + analysis.at(call), noCycleDepth
		}

		return fmt.Sprintf("it is passed to the builtin %s at %s", builtin.Name(), analysis.at(call)), noCycleDepth
	}

	for argIndex, arg := range call.Args {
		if arg != ident {
			continue
		}

		callee, ok := info.Uses[calleeIdent(call.Fun)].(*types.Func)

		if !ok {
			return "it is passed to a call that is not a declared function at " + analysis.at(call), noCycleDepth
		}

		origin := callee.Origin()
		signature := origin.Type().(*types.Signature)

		if argIndex >= signature.Params().Len() || (signature.Variadic() && argIndex >= signature.Params().Len()-1) {
			return fmt.Sprintf("it is passed in the variadic position of %s at %s", analysis.funcName(origin), analysis.at(call)), noCycleDepth
		}

		write, low := analysis.classify(origin, argIndex)

		if write != "" {
			return fmt.Sprintf("it is passed to %s at %s, which may write it: %s", analysis.funcName(origin), analysis.at(call), write), noCycleDepth
		}

		return "", low
	}

	return "it is called at " + analysis.at(call), noCycleDepth
}

// isNilOperand reports whether expr is the untyped nil.
func isNilOperand(info *types.Info, expr ast.Expr) bool {
	typeAndValue, ok := info.Types[expr]
	return ok && typeAndValue.IsNil()
}

// isZeroLiteral reports whether expr is the integer literal 0.
func isZeroLiteral(expr ast.Expr) bool {
	literal, ok := expr.(*ast.BasicLit)
	return ok && literal.Kind == token.INT && literal.Value == "0"
}
