// evalOrderSpill.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns ONE property of a statement:
//
//	A plain read of a local variable sees what a later call in the same statement wrote to it.
//
// gc's order pass (cmd/compile/internal/walk/order.go, expr1's OCALLFUNC arm) copies every call in a
// statement into a temporary ahead of the statement, in source order, and leaves the plain reads in
// the expression, so they run after all of those calls. The spec allows it: it orders the calls among
// themselves and leaves plain operands unordered against them. C# evaluates strictly left to right,
// so a read that comes before a call in the source sees the value BEFORE the call:
//
//	p := pair{1, 2}
//	println(scaled(p, mutate(&p)))    // gc 200004 — scaled sees the mutated p; C# as written: 2004
//
// The remedy is gc's own rewrite, emitted only where the difference is observable:
//
//	var ᴛ1 = mutate(ref p);
//	println(scaled(p, ᴛ1));
//
// Every call up to the last hazardous one spills, in evaluation (post-) order, so the calls keep their
// order among themselves; the plain reads stay where Go wrote them. The record is
// docs/phase4/SCOUT-evaluation-order.md (§3 the forms, §5 option A, §6 gate A1).
//
// WHEN A READ IS HAZARDOUS (gate A1). A read of local storage R comes before ordered operation C in
// C#'s order, C runs (in gc) before R is read — C is nested in R's context, which is the argument list
// of R's innermost enclosing call, or the whole statement — and the statement itself shows that C can
// reach R's storage:
//
//   - C takes R's storage by address: a pointer-receiver method on it, `&R` as an argument, a pointer,
//     slice or map argument whose referent R reads (callWritePaths, shared with returnOperandOrder.go);
//     AND, when C's callee is a same-package function with a body, that body writes through the
//     parameter what R reads, or hands the parameter on (the one-level callee read). A callee in
//     another package, or with no Go body, counts as writing.
//   - C is a closure that assigns R's root: a literal called in place, or a local bound once to one.
//
// An operand gc copies in place (evalOrderInPlace.go: an interface conversion that takes its address,
// or a slow-path map key) is the context of the reads inside it, so only the calls inside it count.
//
// SCOPE — step 1 of the scout's three. Package-level variables (which need the callee walk) and the
// contexts with no statement boundary (a for condition, the right side of && and ||) are steps 2 and
// 3: a statement whose spill would have to move a call out of an && / || right side is left as it is.
// A multi-value return keeps its own spill (returnOperandOrder.go) and is not planned here.
package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
)

// eoEvent is one read or one ordered operation, in the order C# evaluates the statement.
type eoEvent struct {
	read     bool
	path     storagePath
	readExpr ast.Expr
	node     ast.Expr   // the ordered operation (a call or a receive)
	ctx      ast.Node   // a read's context: its innermost enclosing call, or nil for the statement
	within   []ast.Node // an operation's enclosing calls (the contexts it is nested in)
	call     *ast.CallExpr
	writes   bool // a real call or a receive (a builtin or a conversion writes nothing)
	spillOK  bool
}

type eoPlanner struct {
	v       *Visitor
	events  []eoEvent
	stack   []ast.Node // enclosing calls, and operands gc copies in place (evalOrderInPlace.go)
	inCond  int        // inside the right side of && or ||
	inPlace map[ast.Expr]bool
}

// indexPackageFuncDecls maps each package-level function and method of the package to its declaration,
// for the one-level callee read.
func indexPackageFuncDecls(files []FileEntry, info *types.Info) map[*types.Func]*ast.FuncDecl {
	decls := map[*types.Func]*ast.FuncDecl{}

	for _, entry := range files {
		if entry.file == nil {
			continue
		}

		for _, decl := range entry.file.Decls {
			if funcDecl, ok := decl.(*ast.FuncDecl); ok {
				if obj, ok := info.Defs[funcDecl.Name].(*types.Func); ok {
					decls[obj] = funcDecl
				}
			}
		}
	}

	return decls
}

// planEvalOrderSpills plans the spills for one statement whose expressions are given in C# evaluation
// order (an assignment's LHS index and deref bases, then its RHS). It returns the function that ends
// the plan; call it when the statement has been emitted.
func (v *Visitor) planEvalOrderSpills(exprs ...ast.Expr) func() {
	savedSpill, savedTemp := v.evalOrderSpill, v.evalOrderTemp

	v.evalOrderSpill, v.evalOrderTemp = nil, nil

	if v.hoistedDecls != nil && v.inFunction {
		planner := &eoPlanner{v: v}
		planner.markInPlace(exprs...)

		for _, expr := range exprs {
			planner.walk(expr, nil)
		}

		if spills := v.evalOrderSpillSet(planner); len(spills) > 0 {
			v.evalOrderSpill = spills
			v.evalOrderTemp = map[ast.Expr]string{}
		}
	}

	return v.endEvalOrderPlan(savedSpill, savedTemp)
}

func (v *Visitor) endEvalOrderPlan(savedSpill map[ast.Expr]bool, savedTemp map[ast.Expr]string) func() {
	return func() {
		for node := range v.evalOrderSpill {
			if _, rendered := v.evalOrderTemp[node]; !rendered {
				// A planned call that never reached convExprCore would leave a LATER spilled call ahead of
				// it. Say so loudly; the footprint run reads this line.
				position := v.fset.Position(node.Pos())
				fmt.Fprintf(os.Stderr, "go2cs: evaluation-order spill not rendered at %s:%d\n", position.Filename, position.Line)
			}
		}

		v.evalOrderSpill, v.evalOrderTemp = savedSpill, savedTemp
	}
}

// planEvalOrderEvents is planEvalOrderSpills for events already walked.
func (v *Visitor) planEvalOrderEvents(planner *eoPlanner) func() {
	savedSpill, savedTemp := v.evalOrderSpill, v.evalOrderTemp

	v.evalOrderSpill, v.evalOrderTemp = nil, nil

	if v.hoistedDecls != nil && v.inFunction {
		if spills := v.evalOrderSpillSet(planner); len(spills) > 0 {
			v.evalOrderSpill = spills
			v.evalOrderTemp = map[ast.Expr]string{}
		}
	}

	return v.endEvalOrderPlan(savedSpill, savedTemp)
}

// renderEvalOrderSpill renders a planned operation into the statement's pre-statement sink as
// `var ᴛN = …;` and returns ᴛN, or returns "", false when expr is not planned.
func (v *Visitor) renderEvalOrderSpill(expr ast.Expr, contexts []ExprContext) (string, bool) {
	if v.evalOrderSpill == nil || !v.evalOrderSpill[expr] || v.hoistedDecls == nil {
		return "", false
	}

	if temp, ok := v.evalOrderTemp[expr]; ok {
		return temp, true
	}

	// Render the operation itself; the calls nested in it spill first, into the same sink, which is
	// gc's order.
	delete(v.evalOrderSpill, expr)
	rendered := v.convExprCore(expr, contexts)
	v.evalOrderSpill[expr] = true

	v.tupleTempIndex++
	temp := fmt.Sprintf("%s%d", TempVarMarker, v.tupleTempIndex)
	v.evalOrderTemp[expr] = temp

	if v.hoistedDecls.Len() == 0 {
		v.hoistedDecls.WriteString(v.newline)
	}

	v.hoistedDecls.WriteString(v.indent(v.indentLevel))
	v.hoistedDecls.WriteString(fmt.Sprintf("var %s = %s;", temp, rendered))
	v.hoistedDecls.WriteString(v.newline)

	return temp, true
}

// evalOrderSpillSet returns the operations to spill for the statement, or nil.
func (v *Visitor) evalOrderSpillSet(planner *eoPlanner) map[ast.Expr]bool {
	last := -1

	for i, read := range planner.events {
		if !read.read {
			continue
		}

		for j := i + 1; j < len(planner.events); j++ {
			op := planner.events[j]

			if op.read || !op.writes || j <= last {
				continue
			}

			if !op.nestedIn(read.ctx) {
				continue
			}

			if v.evalOrderOpWrites(op, read) {
				last = j
			}
		}
	}

	if last < 0 {
		return nil
	}

	spills := map[ast.Expr]bool{}

	for j := 0; j <= last; j++ {
		op := planner.events[j]

		if op.read {
			continue
		}

		if !op.spillOK {
			return nil // an operation that cannot move would end up after one that did
		}

		spills[op.node] = true
	}

	return spills
}

func (op eoEvent) nestedIn(ctx ast.Node) bool {
	if ctx == nil {
		return true
	}

	for _, enclosing := range op.within {
		if enclosing == ctx {
			return true
		}
	}

	return false
}

func (p *eoPlanner) context() ast.Node {
	if len(p.stack) == 0 {
		return nil
	}

	return p.stack[len(p.stack)-1]
}

func (p *eoPlanner) addRead(expr ast.Expr) {
	path, ok := p.v.eoPathOf(expr)

	if !ok {
		return
	}

	variable, ok := path.root.(*types.Var)

	if !ok || variable.Pkg() == nil || variable.Parent() == variable.Pkg().Scope() || variable.IsField() {
		return // step 1 reads LOCAL storage only
	}

	p.events = append(p.events, eoEvent{read: true, path: path, readExpr: expr, ctx: p.context()})
}

func (p *eoPlanner) addOp(node ast.Expr, call *ast.CallExpr, writes bool) {
	// The operations enclosing this one. An operand copied in place may be this very call, pushed by
	// walk before walkCall pushes it again; an operation is never nested in itself.
	within := make([]ast.Node, 0, len(p.stack))

	for _, enclosing := range p.stack {
		if enclosing != ast.Node(node) {
			within = append(within, enclosing)
		}
	}

	spillOK := p.inCond == 0

	if spillOK {
		// Only a single-valued result can be held in one temporary.
		if tv, ok := p.v.info.Types[node]; !ok || tv.Type == nil || tv.Value != nil {
			spillOK = false
		} else if _, isTuple := tv.Type.(*types.Tuple); isTuple || tv.IsVoid() {
			spillOK = false
		}
	}

	p.events = append(p.events, eoEvent{node: node, call: call, within: within, writes: writes, spillOK: spillOK})
}

// walk visits expr in C#'s evaluation order.
func (p *eoPlanner) walk(expr ast.Expr, _ ast.Node) {
	if expr == nil {
		return
	}

	if p.inPlace[expr] {
		// gc copies this operand where it stands: the reads inside it see only the calls inside it.
		p.stack = append(p.stack, expr)
		defer func() { p.stack = p.stack[:len(p.stack)-1] }()
	}

	switch e := expr.(type) {
	case *ast.ParenExpr:
		p.walk(e.X, nil)
	case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr, *ast.IndexListExpr, *ast.StarExpr:
		p.walkPath(expr)
	case *ast.CallExpr:
		p.walkCall(e)
	case *ast.UnaryExpr:
		if e.Op == token.AND {
			p.walkAddress(e.X)
			return
		}

		p.walk(e.X, nil)

		if e.Op == token.ARROW {
			p.addOp(e, nil, true)
		}
	case *ast.BinaryExpr:
		p.walk(e.X, nil)

		if e.Op == token.LAND || e.Op == token.LOR {
			p.inCond++
			p.walk(e.Y, nil)
			p.inCond--

			return
		}

		p.walk(e.Y, nil)
	case *ast.CompositeLit:
		isMap := false

		if literalType := p.v.getType(e, false); literalType != nil {
			_, isMap = literalType.Underlying().(*types.Map)
		}

		for _, elt := range e.Elts {
			keyValue, keyed := elt.(*ast.KeyValueExpr)

			if isMap {
				// gc fills a map literal entry by entry, as C#'s collection initializer does; each entry
				// is its own context, and its key is a read.
				p.stack = append(p.stack, elt)

				if keyed {
					p.walk(keyValue.Key, nil)
					p.walk(keyValue.Value, nil)
				} else {
					p.walk(elt, nil)
				}

				p.stack = p.stack[:len(p.stack)-1]

				continue
			}

			if keyed {
				p.walk(keyValue.Value, nil) // a struct field name or a constant array index
			} else {
				p.walk(elt, nil)
			}
		}
	case *ast.SliceExpr:
		p.walk(e.X, nil)

		for _, index := range []ast.Expr{e.Low, e.High, e.Max} {
			if index != nil {
				p.walk(index, nil)
			}
		}
	case *ast.TypeAssertExpr:
		p.walk(e.X, nil)
	case *ast.FuncLit, *ast.BasicLit:
		// A literal's body runs when it is called, not here.
	}
}

// walkPath records a storage read and then visits the calls inside its index operands, which C#
// evaluates after the base.
func (p *eoPlanner) walkPath(expr ast.Expr) {
	if tv, ok := p.v.info.Types[expr]; ok && tv.Value != nil {
		return // a constant
	}

	if _, ok := p.v.eoPathOf(expr); ok {
		p.addRead(expr)
		p.walkIndexOperands(expr)

		return
	}

	switch e := expr.(type) {
	case *ast.SelectorExpr:
		p.walk(e.X, nil)
	case *ast.IndexExpr:
		p.walk(e.X, nil)
		p.walk(e.Index, nil)
	case *ast.IndexListExpr:
		p.walk(e.X, nil)
	case *ast.StarExpr:
		p.walk(e.X, nil)
	}
}

func (p *eoPlanner) walkIndexOperands(expr ast.Expr) {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		p.walkIndexOperands(e.X)
	case *ast.SelectorExpr:
		p.walkIndexOperands(e.X)
	case *ast.StarExpr:
		p.walkIndexOperands(e.X)
	case *ast.IndexExpr:
		p.walkIndexOperands(e.X)
		p.walk(e.Index, nil)
	}
}

// walkAddress visits `&X`: X's storage is not read, but the calls in its index operands run.
func (p *eoPlanner) walkAddress(expr ast.Expr) {
	if _, ok := p.v.eoPathOf(expr); ok {
		p.walkIndexOperands(expr)
		return
	}

	p.walk(expr, nil)
}

func (p *eoPlanner) walkCall(call *ast.CallExpr) {
	if tv, ok := p.v.info.Types[call]; ok && tv.Value != nil {
		return // a constant: `len` of an array, `unsafe.Sizeof`
	}

	if isConversion, _ := p.v.isTypeConversion(call); isConversion {
		// A conversion of a read is still the read.
		for _, arg := range call.Args {
			p.walk(arg, nil)
		}

		return
	}

	builtin := false

	if ident, ok := ast.Unparen(call.Fun).(*ast.Ident); ok && p.v.identIsUniverseBuiltin(ident) {
		builtin = true
	}

	p.stack = append(p.stack, call)

	switch fun := ast.Unparen(call.Fun).(type) {
	case *ast.SelectorExpr:
		if selection, ok := p.v.info.Selections[fun]; ok && selection.Kind() == types.MethodVal {
			if method, ok := selection.Obj().(*types.Func); ok {
				if signature, ok := method.Type().(*types.Signature); ok && signature.Recv() != nil {
					if _, pointerRecv := signature.Recv().Type().(*types.Pointer); pointerRecv {
						// The receiver is taken by address: not a read of its value.
						p.walkAddress(fun.X)
					} else {
						p.walk(fun.X, nil) // a value receiver is copied when the call is made
					}
				}
			}
		} else {
			p.walk(fun.X, nil)
		}
	case *ast.FuncLit:
	case *ast.Ident:
	default:
		p.walk(call.Fun, nil)
	}

	for _, arg := range call.Args {
		p.walk(arg, nil)
	}

	p.stack = p.stack[:len(p.stack)-1]

	p.addOp(call, call, !builtin)
}

// evalOrderOpWrites applies gate A1: the statement shows op reaching read's storage.
func (v *Visitor) evalOrderOpWrites(op eoEvent, read eoEvent) bool {
	if op.call == nil {
		return false // a receive writes no variable
	}

	call := op.call

	// A closure that assigns the read's root.
	if lit := v.evalOrderClosureOf(call.Fun); lit != nil && funcLitAssigns(v.info, lit, read.path.root) {
		return true
	}

	type conflict struct {
		position int // -1 for the receiver, else the argument index
		rel      []storageStep
	}

	var conflicting []conflict

	v.evalOrderWritePaths(call, func(position int, written storagePath) {
		if read.path.root == written.root && eoStepsConflict(written.steps, read.path.steps) {
			// What the read sees, relative to the storage handed to the callee.
			var rel []storageStep

			if len(read.path.steps) > len(written.steps) {
				rel = read.path.steps[len(written.steps):]
			}

			conflicting = append(conflicting, conflict{position, rel})
		}
	})

	if len(conflicting) == 0 {
		return false
	}

	// One level into the callee: a same-package body that provably neither writes through nor hands on
	// the parameter receiving the storage clears the site.
	callee := v.evalOrderCallee(call)

	if callee == nil {
		return true
	}

	decl := v.packageFuncDecls[callee.Origin()]

	if decl == nil || decl.Body == nil {
		return true
	}

	for _, c := range conflicting {
		param := evalOrderParamObject(v.info, decl, c.position)

		if param == nil {
			if evalOrderParamUnnamed(decl, c.position) {
				continue // a blank or unnamed parameter cannot be written through
			}

			return true
		}

		if v.evalOrderBodyMayWrite(decl.Body, param, c.rel) {
			return true
		}
	}

	return false
}

// evalOrderWritePaths reports the storage a call receives by reference, with the receiving position
// (-1 for the receiver): a pointer-receiver method's receiver, an `&x` argument, and a pointer, slice
// or map argument, whose referent the callee can write. Paths are eoPathOf's, so the referent of a
// pointer, slice or map lies past a deref.
func (v *Visitor) evalOrderWritePaths(call *ast.CallExpr, add func(int, storagePath)) {
	if selector, ok := ast.Unparen(call.Fun).(*ast.SelectorExpr); ok {
		if selection, ok := v.info.Selections[selector]; ok && selection.Kind() == types.MethodVal {
			if method, ok := selection.Obj().(*types.Func); ok {
				if signature, ok := method.Type().(*types.Signature); ok && signature.Recv() != nil {
					if _, isPointerRecv := signature.Recv().Type().(*types.Pointer); isPointerRecv {
						if path, ok := v.eoPathOf(selector.X); ok {
							if v.eoIsReference(selector.X) {
								path = appendStep(path, storageStep{deref: true})
							}

							add(-1, path) // an addressable value's own storage, or a pointer's pointee
						}
					}
				}
			}
		}
	}

	for i, arg := range call.Args {
		arg = ast.Unparen(arg)

		if unary, ok := arg.(*ast.UnaryExpr); ok && unary.Op == token.AND {
			if path, ok := v.eoPathOf(unary.X); ok {
				add(i, path)
			}

			continue
		}

		if v.eoIsReference(arg) {
			if path, ok := v.eoPathOf(arg); ok {
				add(i, appendStep(path, storageStep{deref: true}))
			}
		}
	}
}

// eoIsReference reports a pointer, slice or map: a value whose referent lives past a deref.
func (v *Visitor) eoIsReference(expr ast.Expr) bool {
	t := v.getType(expr, false)

	if t == nil {
		return false
	}

	switch t.Underlying().(type) {
	case *types.Pointer, *types.Slice, *types.Map:
		return true
	}

	return false
}

// eoPathOf is returnOperandOrder.go's access path with one difference: an element of a slice, map or
// string lives in a backing store the header only points at, so its index step follows a deref.
// Writing a slice's elements therefore cannot change a read of the slice header itself
// (`buf = buf[:read(buf)]`), while it still reaches a read of an element (`s[0] + fill(s)`).
func (v *Visitor) eoPathOf(expr ast.Expr) (storagePath, bool) {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		return v.eoPathOf(e.X)
	case *ast.Ident:
		if variable, ok := v.info.ObjectOf(e).(*types.Var); ok {
			return storagePath{root: variable}, true
		}
	case *ast.SelectorExpr:
		if ident, ok := e.X.(*ast.Ident); ok {
			if _, isPkg := v.info.ObjectOf(ident).(*types.PkgName); isPkg {
				if variable, ok := v.info.ObjectOf(e.Sel).(*types.Var); ok {
					return storagePath{root: variable}, true
				}

				return storagePath{}, false
			}
		}

		base, ok := v.eoPathOf(e.X)

		if !ok {
			return storagePath{}, false
		}

		if _, isPointer := v.getType(e.X, false).(*types.Pointer); isPointer {
			base = appendStep(base, storageStep{deref: true})
		}

		selection, ok := v.info.Selections[e]

		if !ok || selection.Kind() != types.FieldVal || len(selection.Index()) != 1 {
			return base, true // truncated: names a larger location, which can only over-report
		}

		field, isField := selection.Obj().(*types.Var)

		if !isField {
			return base, true
		}

		return appendStep(base, storageStep{field: field}), true
	case *ast.IndexExpr:
		base, ok := v.eoPathOf(e.X)

		if !ok {
			return storagePath{}, false
		}

		inPlace := false

		if baseType := v.getType(e.X, false); baseType != nil {
			switch underlying := baseType.Underlying().(type) {
			case *types.Array:
				inPlace = true
			case *types.Pointer:
				if _, toArray := underlying.Elem().Underlying().(*types.Array); toArray {
					base = appendStep(base, storageStep{deref: true})
					inPlace = true
				}
			}
		}

		if !inPlace {
			base = appendStep(base, storageStep{deref: true})
		}

		return appendStep(base, storageStep{}), true
	case *ast.StarExpr:
		base, ok := v.eoPathOf(e.X)

		if !ok {
			return storagePath{}, false
		}

		return appendStep(base, storageStep{deref: true}), true
	}

	return storagePath{}, false
}

// evalOrderCallee is the statically known function or method a call invokes, or nil.
func (v *Visitor) evalOrderCallee(call *ast.CallExpr) *types.Func {
	switch fun := ast.Unparen(call.Fun).(type) {
	case *ast.Ident:
		fn, _ := v.info.Uses[fun].(*types.Func)
		return fn
	case *ast.SelectorExpr:
		if selection, ok := v.info.Selections[fun]; ok {
			if _, isIface := selection.Recv().Underlying().(*types.Interface); isIface {
				return nil
			}

			fn, _ := selection.Obj().(*types.Func)

			return fn
		}

		fn, _ := v.info.Uses[fun.Sel].(*types.Func)

		return fn
	case *ast.IndexExpr:
		return v.evalOrderCallee(&ast.CallExpr{Fun: fun.X})
	case *ast.IndexListExpr:
		return v.evalOrderCallee(&ast.CallExpr{Fun: fun.X})
	}

	return nil
}

// evalOrderClosureOf returns the literal a call invokes: one called in place, or a local bound to a
// literal exactly once in the enclosing function.
func (v *Visitor) evalOrderClosureOf(fun ast.Expr) *ast.FuncLit {
	switch f := ast.Unparen(fun).(type) {
	case *ast.FuncLit:
		return f
	case *ast.Ident:
		variable, ok := v.info.Uses[f].(*types.Var)

		if !ok || v.currentFuncDecl == nil || v.currentFuncDecl.Body == nil {
			return nil
		}

		var bound *ast.FuncLit
		binds := 0

		ast.Inspect(v.currentFuncDecl.Body, func(n ast.Node) bool {
			switch s := n.(type) {
			case *ast.AssignStmt:
				for i, lhs := range s.Lhs {
					if ident, ok := lhs.(*ast.Ident); ok && v.info.ObjectOf(ident) == variable {
						binds++

						if len(s.Lhs) == len(s.Rhs) {
							bound, _ = ast.Unparen(s.Rhs[i]).(*ast.FuncLit)
						}
					}
				}
			case *ast.ValueSpec:
				for i, name := range s.Names {
					if v.info.ObjectOf(name) == variable {
						binds++

						if len(s.Values) == len(s.Names) {
							bound, _ = ast.Unparen(s.Values[i]).(*ast.FuncLit)
						}
					}
				}
			}

			return true
		})

		if binds == 1 {
			return bound
		}
	}

	return nil
}

// funcLitAssigns reports whether the literal's body assigns, increments or takes the address of root.
func funcLitAssigns(info *types.Info, lit *ast.FuncLit, root types.Object) bool {
	found := false

	ast.Inspect(lit.Body, func(n ast.Node) bool {
		if found {
			return false
		}

		var targets []ast.Expr

		switch s := n.(type) {
		case *ast.AssignStmt:
			targets = s.Lhs
		case *ast.IncDecStmt:
			targets = []ast.Expr{s.X}
		case *ast.UnaryExpr:
			if s.Op == token.AND {
				targets = []ast.Expr{s.X}
			}
		case *ast.RangeStmt:
			if s.Tok == token.ASSIGN {
				targets = []ast.Expr{s.Key, s.Value}
			}
		}

		for _, target := range targets {
			if target != nil && evalOrderRootObject(info, target) == root {
				found = true
			}
		}

		return !found
	})

	return found
}

// evalOrderRootObject is the variable an lvalue expression is rooted at, through fields, indices and
// derefs.
func evalOrderRootObject(info *types.Info, expr ast.Expr) types.Object {
	for {
		switch e := expr.(type) {
		case *ast.ParenExpr:
			expr = e.X
		case *ast.Ident:
			return info.ObjectOf(e)
		case *ast.SelectorExpr:
			if _, isPkg := info.ObjectOf(identOf(e.X)).(*types.PkgName); isPkg {
				return info.ObjectOf(e.Sel)
			}

			expr = e.X
		case *ast.IndexExpr:
			expr = e.X
		case *ast.StarExpr:
			expr = e.X
		default:
			return nil
		}
	}
}

func identOf(expr ast.Expr) *ast.Ident {
	ident, _ := expr.(*ast.Ident)
	return ident
}

// evalOrderParamObject is the declared receiver (position -1) or parameter object at a call position.
func evalOrderParamObject(info *types.Info, decl *ast.FuncDecl, position int) types.Object {
	if position < 0 {
		if decl.Recv == nil || len(decl.Recv.List) != 1 || len(decl.Recv.List[0].Names) != 1 {
			return nil
		}

		return info.Defs[decl.Recv.List[0].Names[0]]
	}

	index := 0

	for _, field := range decl.Type.Params.List {
		if _, variadic := field.Type.(*ast.Ellipsis); variadic {
			return nil
		}

		if len(field.Names) == 0 {
			index++
			continue
		}

		for _, name := range field.Names {
			if index == position {
				if name.Name == "_" {
					return nil
				}

				return info.Defs[name]
			}

			index++
		}
	}

	return nil
}

// evalOrderParamUnnamed reports a receiver or parameter the body cannot name (unnamed or blank, and
// not variadic).
func evalOrderParamUnnamed(decl *ast.FuncDecl, position int) bool {
	if position < 0 {
		return decl.Recv != nil && len(decl.Recv.List) == 1 &&
			(len(decl.Recv.List[0].Names) == 0 || decl.Recv.List[0].Names[0].Name == "_")
	}

	index := 0

	for _, field := range decl.Type.Params.List {
		if _, variadic := field.Type.(*ast.Ellipsis); variadic {
			return false
		}

		count := len(field.Names)

		if count == 0 {
			count = 1
		}

		for i := 0; i < count; i++ {
			if index == position {
				return len(field.Names) == 0 || field.Names[i].Name == "_"
			}

			index++
		}
	}

	return false
}

// evalOrderBodyMayWrite reports whether a body can change, through param, the storage at rel (relative
// to what param refers to). Every use of param is read as its maximal ACCESS CHAIN — param followed by
// field selections, indexing and derefs — and classified by what encloses the chain:
//
//   - the target of an assignment, an increment, `&`, or a pointer method on an addressable chain:
//     a write at the chain's path, compared against rel (eoStepsConflict);
//   - copy, clear, delete or append with the chain first: a write to its elements;
//   - a chain that is a reference (pointer, slice, map, channel, func, interface) and is handed
//     anywhere else — an argument, a method receiver, a value stored, returned or sent: the callee
//     cannot see what happens next, so the answer is "may write";
//   - anything else (a value read, a comparison, len or cap): no write.
func (v *Visitor) evalOrderBodyMayWrite(body *ast.BlockStmt, param types.Object, rel []storageStep) bool {
	mayWrite := false
	var stack []ast.Node

	ast.Inspect(body, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}

		if mayWrite {
			return false
		}

		if ident, ok := n.(*ast.Ident); ok && v.info.Uses[ident] == param {
			chain, parent := evalOrderAccessChain(v.info, ident, stack)

			if v.evalOrderChainMayWrite(chain, parent, rel) {
				mayWrite = true
			}
		}

		stack = append(stack, n)

		return !mayWrite
	})

	return mayWrite
}

// evalOrderAccessChain climbs from a use of a variable to the maximal chain of field selections,
// indexing, derefs and parentheses rooted at it, and returns the chain and the node enclosing it.
func evalOrderAccessChain(info *types.Info, ident *ast.Ident, stack []ast.Node) (ast.Expr, ast.Node) {
	var chain ast.Expr = ident

	for depth := len(stack) - 1; depth >= 0; depth-- {
		switch parent := stack[depth].(type) {
		case *ast.ParenExpr, *ast.StarExpr:
			chain = parent.(ast.Expr)
			continue
		case *ast.IndexExpr:
			if parent.X == chain {
				chain = parent
				continue
			}
		case *ast.SelectorExpr:
			if selection, ok := info.Selections[parent]; ok && parent.X == chain && selection.Kind() == types.FieldVal {
				chain = parent
				continue
			}
		}

		return chain, stack[depth]
	}

	return chain, nil
}

// evalOrderChainMayWrite classifies one access chain rooted at the parameter by what encloses it.
func (v *Visitor) evalOrderChainMayWrite(chain ast.Expr, parent ast.Node, rel []storageStep) bool {
	info := v.info

	isReference := func(expr ast.Expr) bool {
		t := v.getType(expr, false)

		if t == nil {
			return true
		}

		switch t.Underlying().(type) {
		case *types.Pointer, *types.Slice, *types.Map, *types.Chan, *types.Signature, *types.Interface:
			return true
		}

		return false
	}

	writeAt := func(extra ...storageStep) bool {
		if _, isIdent := ast.Unparen(chain).(*ast.Ident); isIdent && len(extra) == 0 {
			return false // reassigning the parameter variable writes no caller storage
		}

		path, ok := v.eoPathOf(ast.Unparen(chain))

		if !ok {
			return true
		}

		// The first step crosses the reference the parameter holds (the pointer's deref, or the
		// slice's or map's element); what follows is relative to the caller's storage.
		steps := append(append([]storageStep(nil), path.steps...), extra...)

		if len(steps) > 0 {
			steps = steps[1:]
		}

		return eoStepsConflict(steps, rel)
	}

	switch p := parent.(type) {
	case *ast.AssignStmt:
		for _, lhs := range p.Lhs {
			if lhs == chain {
				return p.Tok != token.DEFINE && writeAt()
			}
		}
	case *ast.IncDecStmt:
		return writeAt()
	case *ast.RangeStmt:
		if p.Key == chain || p.Value == chain {
			return p.Tok == token.ASSIGN && writeAt()
		}

		if p.X == chain {
			return false // ranging reads
		}
	case *ast.UnaryExpr:
		if p.Op != token.AND {
			return false // -x, !x, ^x, <-ch read
		}

		if _, isIdent := ast.Unparen(chain).(*ast.Ident); isIdent {
			return true // &param hands the reference on
		}

		return writeAt()
	case *ast.SelectorExpr:
		// A method selected on the chain (fields were absorbed by the climb).
		if selection, ok := info.Selections[p]; ok && selection.Kind() == types.MethodVal {
			if method, ok := selection.Obj().(*types.Func); ok {
				if signature, ok := method.Type().(*types.Signature); ok && signature.Recv() != nil {
					if _, pointerRecv := signature.Recv().Type().(*types.Pointer); pointerRecv {
						if !isReference(chain) {
							return writeAt() // the method takes the chain's address
						}

						return true // a pointer handed to a method
					}
				}
			}

			return isReference(chain) // an interface, or a method value bound to a reference
		}
	case *ast.CallExpr:
		if isConversion, _ := v.isTypeConversion(p); isConversion {
			return isReference(p)
		}

		if fun, ok := ast.Unparen(p.Fun).(*ast.Ident); ok {
			if _, isBuiltin := info.Uses[fun].(*types.Builtin); isBuiltin {
				switch fun.Name {
				case "len", "cap":
					return false
				case "copy", "clear", "delete", "append":
					return len(p.Args) > 0 && p.Args[0] == chain && writeAt(storageStep{})
				}
			}
		}

		return isReference(chain) // handed to a call
	case *ast.SliceExpr:
		return p.X == chain // a reslice shares the backing array; what happens to it next is unknown
	case *ast.BinaryExpr, *ast.IfStmt, *ast.SwitchStmt, *ast.ExprStmt, *ast.IndexExpr:
		return false
	}

	// Stored, returned, sent, a composite element: a reference escapes.
	return isReference(chain)
}

// planEvalOrderAssignSpills plans an assignment. C# evaluates the left side's LOCATION first, and only
// the parts of it that are values can go stale under a later call: a slice or map base, a pointer being
// dereferenced, and the index operands' calls. A compound `x op= f()` reads all of x first.
func (v *Visitor) planEvalOrderAssignSpills(assignStmt *ast.AssignStmt) func() {
	planner := &eoPlanner{v: v}

	if v.hoistedDecls != nil && v.inFunction {
		planner.markInPlace(assignStmt.Lhs...)
		planner.markInPlace(assignStmt.Rhs...)
		planner.markAssignInPlace(assignStmt)
	}

	for _, lhs := range assignStmt.Lhs {
		if assignStmt.Tok == token.DEFINE {
			continue
		}

		if assignStmt.Tok != token.ASSIGN {
			planner.walkPath(lhs)
			continue
		}

		planner.walkLHS(lhs)
	}

	for _, rhs := range assignStmt.Rhs {
		planner.walk(rhs, nil)
	}

	return v.planEvalOrderEvents(planner)
}

func (p *eoPlanner) walkLHS(expr ast.Expr) {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		p.walkLHS(e.X)
	case *ast.Ident:
		// the variable itself is the location
	case *ast.SelectorExpr:
		if _, isPointer := p.v.getType(e.X, false).(*types.Pointer); isPointer {
			p.walkPath(e.X)
		} else if _, isPkg := p.v.info.ObjectOf(identOf(e.X)).(*types.PkgName); !isPkg {
			p.walkLHS(e.X)
		}
	case *ast.IndexExpr:
		if baseType := p.v.getType(e.X, false); baseType != nil {
			if _, isArray := baseType.Underlying().(*types.Array); isArray {
				p.walkLHS(e.X)
				p.walk(e.Index, nil)

				return
			}
		}

		p.walkPath(e.X)
		p.walk(e.Index, nil)
	case *ast.StarExpr:
		p.walkPath(e.X)
	default:
		p.walk(expr, nil)
	}
}

// eoStepsConflict reports whether a write at path w can change what a read at path r sees, both
// relative to one root. A write AT or ABOVE the read's storage reaches it, through any pointers stored
// there (reassigning a pointer changes every read through it); a write BELOW the read reaches it only
// while it stays inside the read's storage, that is, with no deref between them.
//
//	w = p      r = p.x     conflict    (p may now point elsewhere, or p.x was overwritten)
//	w = p.x    r = p       conflict    (part of what r copies changed)
//	w = p.*.x  r = p       no conflict (the write lands in p's pointee; the pointer read is unchanged)
//	w = p.a    r = p.b     no conflict
//
// It is direction-aware where pathsConflict (returnOperandOrder.go) is symmetric: that predicate
// reads `w = p, r = *p` as no conflict, which under-reports a callee that reassigns the pointer.
func eoStepsConflict(w, r []storageStep) bool {
	shared := len(w)

	if len(r) < shared {
		shared = len(r)
	}

	for i := 0; i < shared; i++ {
		if w[i] != r[i] {
			return false
		}
	}

	if len(w) <= len(r) {
		return true
	}

	for _, step := range w[len(r):] {
		if step.deref {
			return false
		}
	}

	return true
}
