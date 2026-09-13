// sstringHoistOperations.go - Gbtc
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
	"os"
)

// sstringHoist records one function-scope stack-string temp to inject: the temp's C# name and a
// representative `string(x)` conversion CallExpr whose rendering (`((sstring)x)`) initializes it.
type sstringHoist struct {
	tempName string
	convExpr *ast.CallExpr
}

// sstringHoistGroup accumulates, per source object, the eligible `string(x)` conversions found in one
// function body plus the context needed to decide whether lifting them to a single temp is both safe
// and worthwhile.
type sstringHoistGroup struct {
	obj       types.Object
	calls     []*ast.CallExpr
	anchor    ast.Stmt // first TOP-LEVEL body statement containing a use — inject the temp before it
	inLoop    bool     // some use sits inside a for/range loop (re-materialized every iteration)
	inFuncLit bool     // some use sits inside a nested func literal — disqualifies (ref struct can't cross)
}

// planSStringHoists is the per-FuncDecl pre-pass for the loop-invariant / repeated-conversion hoisting
// increment of the sstring stack-string expansion (see docs/Roadmap.md, "expand the sstring stack-string
// MVP"). The MVP re-materializes `((sstring)x)` at EVERY eligible `string(x)` comparison operand; when
// the same never-written source `x` is compared repeatedly — several operands, or one inside a loop —
// that view reconstruction is loop-invariant work the JIT will not hoist (measured: a non-throwing
// golib view made zero difference). This pass lifts each such group to ONE
// `sstring <temp> = ((sstring)x);` at function scope and rewrites every use to reference the temp.
//
// Safety is the strong, MVP-safe gate — no liveness analysis:
//   - `x` must be a plain function-local or parameter (never a package-level var, whose backing a
//     callee could mutate through a path objectIsWritten cannot see — it scans only this body).
//   - `x` must NEVER be written anywhere in the body (objectIsWritten == false), so a function-top view
//     observes exactly the same bytes every inline view would.
//   - `x` must be declared strictly before the injection point (so it is in scope there).
//   - No use may sit inside a nested func literal: an sstring is a `ref struct` and cannot cross a
//     closure boundary. (That is a compile error, so the gate keeps it a loud impossibility rather than
//     relying on the guarantee — but excluding those groups keeps the emitted code valid up front.)
//
// The maps it populates (sstringHoistedConvExprs, sstringHoistsByStmt) are reset here per function and
// consulted during body emission by convCallExpr (use → temp name) and visitBlockStmt (anchor → decl).
func (v *Visitor) planSStringHoists(funcDecl *ast.FuncDecl) {
	v.sstringHoistedConvExprs = nil
	v.sstringHoistsByStmt = nil

	if funcDecl.Body == nil || len(v.sstringConvExprs) == 0 {
		return
	}

	groups := map[types.Object]*sstringHoistGroup{}
	var order []types.Object

	// Walk the body maintaining the ancestor stack so each eligible conversion's loop / func-literal
	// context and enclosing top-level statement are known. ast.Inspect calls f(nil) after a node's
	// children (only when f returned true, which we always do for non-nil nodes), so pushing on entry
	// and popping on the nil callback keeps `stack` equal to the current node's ancestor chain:
	// stack[0] is the body, stack[1] the top-level statement that (transitively) contains the node.
	var stack []ast.Node

	ast.Inspect(funcDecl.Body, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}

		// Only a BARE identifier source (`string(buf)`) may share a hoisted temp: two conversions of
		// the same never-written identifier always view exactly the same bytes. A sub-slice or index
		// source (`string(buf[:7])` vs `string(buf[8:12])` in net/http's is408Message) differs per use —
		// grouping those by their common root ident would collapse distinct views into one (a
		// correctness bug: the second comparison would test the FIRST slice), so they stay inline.
		if call, ok := n.(*ast.CallExpr); ok && v.sstringConvExprs[call] && len(stack) >= 2 {
			if ident, isIdent := call.Args[0].(*ast.Ident); isIdent {
				if obj := v.info.ObjectOf(ident); obj != nil {
					grp := groups[obj]

					if grp == nil {
						grp = &sstringHoistGroup{obj: obj, anchor: topLevelStmt(stack)}
						groups[obj] = grp
						order = append(order, obj)
					}

					grp.calls = append(grp.calls, call)

					if stackHasFuncLit(stack) {
						grp.inFuncLit = true
					}

					if stackHasLoop(stack) {
						grp.inLoop = true
					}
				}
			}
		}

		stack = append(stack, n)
		return true
	})

	// Decide and record hoists in deterministic (first-encounter) order.
	for _, obj := range order {
		grp := groups[obj]

		// A use crosses a closure boundary, or the source is not a plain local/param, or the source
		// may be mutated during the view's now-extended lifetime — any of these makes hoisting unsafe.
		if grp.inFuncLit || !objectIsFunctionLocal(obj) || v.objectIsWritten(obj, funcDecl.Body) {
			continue
		}

		// The source must be declared strictly before the injection point (the anchor), else the
		// hoisted decl would reference an out-of-scope name — e.g. `if buf := f(); string(buf) == …`,
		// where buf is declared inside the very statement the temp would be injected before.
		if grp.anchor == nil || obj.Pos() >= grp.anchor.Pos() {
			continue
		}

		// Worth hoisting only when the conversion is actually repeated: several operands, or one
		// inside a loop (re-materialized every iteration). A lone, non-looping comparison already
		// materializes its view exactly once — hoisting it would be pure churn with no win.
		if len(grp.calls) < 2 && !grp.inLoop {
			continue
		}

		tempName := v.getTempVarName(getSanitizedIdentifier(obj.Name()))

		if v.sstringHoistsByStmt == nil {
			v.sstringHoistsByStmt = map[ast.Stmt][]sstringHoist{}
			v.sstringHoistedConvExprs = map[*ast.CallExpr]string{}
		}

		v.sstringHoistsByStmt[grp.anchor] = append(v.sstringHoistsByStmt[grp.anchor], sstringHoist{
			tempName: tempName,
			convExpr: grp.calls[0],
		})

		for _, call := range grp.calls {
			v.sstringHoistedConvExprs[call] = tempName
		}

		if os.Getenv("GO2CS_DEBUG_SSTRING") != "" {
			pos := v.fset.Position(grp.calls[0].Pos())
			fmt.Fprintf(os.Stderr, "[sstring] hoist %s (%d uses, loop=%v) at %s:%d:%d\n", tempName, len(grp.calls), grp.inLoop, pos.Filename, pos.Line, pos.Column)
		}
	}
}

// emitSStringHoist writes one hoisted stack-string declaration — `sstring <temp> = ((sstring)x);` — at
// the current indentation. suppressSStringHoist is set only for the initializer render so convCallExpr
// emits the real `((sstring)x)` view (the same form the eligible decl / comparison operand would emit)
// rather than short-circuiting to the temp name.
func (v *Visitor) emitSStringHoist(hoist sstringHoist) {
	v.outputBuilder.WriteString(v.newline)
	v.outputBuilder.WriteString(v.indent(v.indentLevel))
	v.outputBuilder.WriteString("sstring ")
	v.outputBuilder.WriteString(hoist.tempName)
	v.outputBuilder.WriteString(" = ")

	saved := v.suppressSStringHoist
	v.suppressSStringHoist = true
	v.outputBuilder.WriteString(v.convExpr(hoist.convExpr, nil))
	v.suppressSStringHoist = saved

	v.outputBuilder.WriteString(";")
}

// topLevelStmt returns the top-level function-body statement for the node whose ancestor chain is
// `stack` (stack[0] is the body, stack[1] the direct child of it), or nil if unavailable.
func topLevelStmt(stack []ast.Node) ast.Stmt {
	if len(stack) >= 2 {
		if stmt, ok := stack[1].(ast.Stmt); ok {
			return stmt
		}
	}

	return nil
}

// stackHasFuncLit reports whether any ancestor in the chain is a function literal — the walk starts at
// the func body (a BlockStmt, not a FuncLit), so any FuncLit means the node sits inside a nested closure.
func stackHasFuncLit(stack []ast.Node) bool {
	for _, n := range stack {
		if _, ok := n.(*ast.FuncLit); ok {
			return true
		}
	}

	return false
}

// stackHasLoop reports whether any ancestor in the chain is a for/range loop (so a conversion at the
// node is re-evaluated every iteration).
func stackHasLoop(stack []ast.Node) bool {
	for _, n := range stack {
		switch n.(type) {
		case *ast.ForStmt, *ast.RangeStmt:
			return true
		}
	}

	return false
}

// objectIsFunctionLocal reports whether obj is a plain function-local or parameter variable — i.e. a
// *types.Var whose declaring scope is nested inside the function, not the package scope. Package-level
// vars are excluded: extending a stack-view over one across the whole function would assume no callee
// mutates it, which the body-only objectIsWritten scan cannot verify.
func objectIsFunctionLocal(obj types.Object) bool {
	varObj, ok := obj.(*types.Var)

	if !ok || varObj.Pkg() == nil {
		return false
	}

	return varObj.Parent() != nil && varObj.Parent() != varObj.Pkg().Scope()
}
