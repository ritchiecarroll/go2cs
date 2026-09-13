// visitForStmt.go - Gbtc
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

const ForVarInitMarker = ">>MARKER:FOR_VAR_INIT<<"

// LoopContinueLabelMarker (formatted with the loop's minted label name, so nested loops never
// collide) holds the end-of-body slot where a loop declares its wrapped-continue target label —
// see prepareLoopContinueTarget / finalizeLoopContinueTarget.
const LoopContinueLabelMarker = ">>MARKER:LOOP_CONTINUE_LABEL_%s<<"

// prepareLoopContinueTarget opens a loop's wrapped-continue bookkeeping: a `do { … } while
// (false)` switch-break wrapper in the body (visitSwitchStmtCore) is itself a C# iteration
// statement, so a Go `continue` inside one cannot be emitted bare — it targets a label at the
// very END of the loop body instead, where control reaches the post clause and condition exactly
// as `continue` would (flowing through the per-iteration copy-backs, which follow the label).
// Whether any case needs it is only known after the body has been emitted, so the label's slot in
// the body's innerSuffix is a marker: the caller appends it AFTER any `continue_<label>:` target
// and BEFORE the copy-backs, pushes the entry around the body visitation, and hands both to
// finalizeLoopContinueTarget, which resolves the marker to the labeled empty statement — or to
// nothing, keeping every label-free loop byte-identical. The label name is minted eagerly (the
// `continue` temp-name counter feeds no other emission, so an unused mint disturbs nothing).
func (v *Visitor) prepareLoopContinueTarget() (*continueTargetEntry, string) {
	entry := &continueTargetEntry{labelName: v.getTempVarName("continue")}
	return entry, fmt.Sprintf(LoopContinueLabelMarker, entry.labelName)
}

// finalizeLoopContinueTarget resolves a loop's end-of-body label marker after the body has been
// emitted: to `<labelName>:;` (column 0, the same rendering as `continue_<label>:;`) when some
// wrapped continue targeted the loop, to nothing otherwise. See prepareLoopContinueTarget.
func (v *Visitor) finalizeLoopContinueTarget(entry *continueTargetEntry, marker string) {
	if entry.labelUsed {
		v.replaceMarker(marker, fmt.Sprintf("%s%s:;", v.newline, entry.labelName))
	} else {
		v.replaceMarker(marker, "")
	}
}

func (v *Visitor) visitForStmt(forStmt *ast.ForStmt, target LabeledStmtContext) {
	// A func literal passed as a call argument in the condition (`for (…; underIs(t, func(u){…}); …)`)
	// emits its captured-variable snapshot declarations (`var tʗ1 = t;`) — statements, invalid inside
	// the condition expression. Convert the condition into a hoist buffer so those decls can be
	// written on their own lines before the loop (mirrors visitExprStmt). Save/restore guards nesting.
	savedHoist := v.hoistedDecls

	// A `for i := …; cond; post` clause variable captured by a func literal in the body (or
	// heap-boxed) takes Go 1.22+ per-iteration semantics: each iteration owns a fresh copy of the
	// variable, initialized from the previous iteration's final value. A C# for-clause variable is
	// ONE variable shared by every iteration, so a captured `() => i` would observe the post-mutated
	// final value (Go prints `0 1 2`, the shared form `3 3 3`) and a stored `&i` would alias one
	// shared box. The clause is therefore rewritten to drive a renamed CARRIER (`iᴛ1`), the real
	// variable is re-declared fresh from it at the top of the body (`var i = iᴛ1;`, or a fresh heap
	// box), and — when the body can write the variable — its value is copied back to the carrier at
	// every transfer to the post clause (end of body, unlabeled `continue`, `continue_<label>:`).
	perIterVars := v.forClausePerIterVars(forStmt)

	if forStmt.Init == nil && forStmt.Post == nil {
		// Handle while-style for loops
		hoistBuf := &strings.Builder{}
		var cond string

		if forStmt.Cond != nil {
			v.hoistedDecls = hoistBuf
			cond = v.convExpr(forStmt.Cond, nil)
			v.hoistedDecls = savedHoist
		}

		if hoistBuf.Len() > 0 {
			// The buffer carries its own leading newline+indent per decl and a trailing newline (the
			// per-decl trailing indent is trimmed by convFuncLit); writeOutput supplies `while`'s indent.
			v.outputBuilder.WriteString(hoistBuf.String())
		} else {
			v.outputBuilder.WriteString(v.newline)
		}

		v.writeOutput("while (")

		if forStmt.Cond == nil {
			v.outputBuilder.WriteString(TrueMarker)
		} else {
			v.outputBuilder.WriteString(cond)
		}
	} else {
		// Register the per-iteration clause vars BEFORE emitting the clause: every ident in the
		// init/cond/post refers to the CARRIER, and the carrier stays a plain value even for a
		// heap-boxed variable (its fresh box is declared per-iteration in the body, so
		// convertToHeapTypeDecl must yield nothing for the clause's own declaration).
		if len(perIterVars) > 0 {
			if v.forPerIterVars == nil {
				v.forPerIterVars = make(map[types.Object]bool)
			}

			if v.identNames == nil {
				v.identNames = make(map[*ast.Ident]string)
			}

			for _, perIterVar := range perIterVars {
				v.forPerIterVars[perIterVar.obj] = true
			}

			for _, clause := range []ast.Node{forStmt.Init, forStmt.Cond, forStmt.Post} {
				if clause == nil {
					continue
				}

				ast.Inspect(clause, func(n ast.Node) bool {
					ident, ok := n.(*ast.Ident)

					if !ok {
						return true
					}

					obj := v.info.ObjectOf(ident)

					for _, perIterVar := range perIterVars {
						if perIterVar.obj == obj {
							v.identNames[ident] = perIterVar.carrier
							break
						}
					}

					return true
				})
			}
		}

		v.outputBuilder.WriteString(v.newline)

		// Handle traditional for loops
		v.outputBuilder.WriteString(ForVarInitMarker)
		v.writeOutput("for (")

		// Escape analysis should be performed on the for loop body for
		// any initializations that may be using shadowed variables
		heapTypeDeclTarget := &strings.Builder{}

		format := FormattingContext{
			useNewLine:         false,
			includeSemiColon:   false,
			useIndent:          false,
			heapTypeDeclTarget: heapTypeDeclTarget,
		}

		contexts := []StmtContext{format}

		if forStmt.Init != nil {
			// Allowed statements in the init part of a for loop:
			//   - short variable declaration (can have heap allocations)
			//   - assignment
			//   - increment / decrement statement
			//   - send statement (source code):
			// Flag the init clause so a multi-variable mixed-type `:=` is emitted as a single
			// tuple-deconstruction declaration (a for-init clause cannot hold `;`-separated decls).
			initFormat := format
			initFormat.forInit = true
			v.visitStmt(forStmt.Init, []StmtContext{initFormat})
		}

		// Convert the condition AFTER the init (preserving capture-counter ordering). Any func-literal
		// capture-snapshot decls are collected here and hoisted before the `for`, at the same marker
		// position as the for-init heap allocations.
		condHoistBuf := &strings.Builder{}
		var cond string

		if forStmt.Cond != nil {
			v.hoistedDecls = condHoistBuf
			cond = v.convExpr(forStmt.Cond, nil)
			v.hoistedDecls = savedHoist
		}

		// Replace the marker with the hoisted condition snapshot decls followed by any heap
		// allocations for the for loop. The marker sits after a leading newline and before the
		// `<indent>for (`, so each group is emitted as `<indent>content` lines ending in a newline.
		var markerReplacement strings.Builder

		if condHoistBuf.Len() > 0 {
			// Reformat the buffer's `\r\n<indent>decl;…\r\n<indent>` into `<indent>decl;…\r\n` lines:
			// drop the leading newline (the marker already follows one) and the trailing indent.
			markerReplacement.WriteString(strings.TrimRight(strings.TrimPrefix(condHoistBuf.String(), v.newline), " "))
		}

		if heapTypeDeclTarget.Len() > 0 {
			markerReplacement.WriteString(v.indent(v.indentLevel))
			markerReplacement.WriteString(heapTypeDeclTarget.String())
			markerReplacement.WriteString(v.newline)
		}

		v.replaceMarker(ForVarInitMarker, markerReplacement.String())

		v.outputBuilder.WriteString("; ")

		if forStmt.Cond == nil {
			v.outputBuilder.WriteString(TrueMarker)
			v.outputBuilder.WriteRune(' ')
		} else {
			v.outputBuilder.WriteString(cond)
		}

		v.outputBuilder.WriteString("; ")

		if forStmt.Post != nil {
			// Allowed statements in the post part of a for loop:
			//   - assignment
			//   - increment / decrement statement
			//   - send statement (source code):
			v.inForPost = true
			v.forPostReAlias = ""
			v.visitStmt(forStmt.Post, contexts)
			v.inForPost = false
		}
	}

	v.outputBuilder.WriteRune(')')

	blockContext := DefaultBlockStmtContext()
	blockContext.format.useNewLine = false

	bodyIndent := v.indent(v.indentLevel + 1)

	// A deref-aliased pointer param/box repointed in the POST (`for ; scope != nil;
	// scope = scope.Outer`) stashed its value re-alias here — inject it as the first
	// statement of the loop body so each iteration re-binds the value var to the new box
	// (the box-repoint stayed in the post). go/ast resolve.go's scope-chain walk.
	if len(v.forPostReAlias) > 0 {
		blockContext.innerPrefix = v.newline + bodyIndent + v.forPostReAlias
		v.forPostReAlias = ""
	}

	// Open the body with each per-iteration variable's fresh declaration — a plain copy of the
	// carrier, or a fresh heap box assigned from it — and collect the copy-back statements the
	// post-clause transfer points need (end of body, unlabeled `continue` via loopCopyBackStack,
	// and after the `continue_<label>:` target).
	var copyBacks []string

	for _, perIterVar := range perIterVars {
		name := getSanitizedIdentifier(perIterVar.name)

		if perIterVar.boxDecl != "" {
			blockContext.innerPrefix += fmt.Sprintf("%s%s%s%s%s = %s;", v.newline, bodyIndent, perIterVar.boxDecl, v.newline+bodyIndent, name, perIterVar.carrier)
		} else if v.options.preferVarDecl {
			blockContext.innerPrefix += fmt.Sprintf("%s%svar %s = %s;", v.newline, bodyIndent, name, perIterVar.carrier)
		} else {
			blockContext.innerPrefix += fmt.Sprintf("%s%s%s %s = %s;", v.newline, bodyIndent, v.getCSharpTypeName(v.getIdentType(perIterVar.ident)), name, perIterVar.carrier)
		}

		if perIterVar.copyBack {
			copyBacks = append(copyBacks, fmt.Sprintf("%s = %s;", perIterVar.carrier, name))
		}
	}

	if len(target.label) > 0 {
		blockContext.innerSuffix = fmt.Sprintf("%s%s:;", v.newline, getContinueLabelName(target.label))
		blockContext.outerSuffix = fmt.Sprintf("%s%s:;", v.newline, getBreakLabelName(target.label))
	}

	// The wrapped-continue label slot (resolved to a label or dropped after the body emits — see
	// prepareLoopContinueTarget) sits with the `continue_<label>:` target, ahead of the copy-backs.
	continueTarget, continueLabelMarker := v.prepareLoopContinueTarget()
	blockContext.innerSuffix += continueLabelMarker

	// The copy-backs follow the continue label so a `goto continue_<label>` path flows through
	// them on its way to the post clause (an unlabeled `continue` is handled at its own site).
	for _, copyBack := range copyBacks {
		blockContext.innerSuffix += v.newline + bodyIndent + copyBack
	}

	v.loopCopyBackStack = append(v.loopCopyBackStack, copyBacks)
	v.continueTargetStack = append(v.continueTargetStack, continueTarget)
	v.visitBlockStmt(forStmt.Body, blockContext)
	v.continueTargetStack = v.continueTargetStack[:len(v.continueTargetStack)-1]
	v.loopCopyBackStack = v.loopCopyBackStack[:len(v.loopCopyBackStack)-1]
	v.finalizeLoopContinueTarget(continueTarget, continueLabelMarker)

	for _, perIterVar := range perIterVars {
		delete(v.forPerIterVars, perIterVar.obj)
	}
}

// forPerIterVar describes one `for i := …` clause variable emitted with Go 1.22+ per-iteration
// semantics (see forClausePerIterVars).
type forPerIterVar struct {
	ident    *ast.Ident   // the `:=` LHS ident (clause references are renamed to carrier)
	obj      types.Object // the loop variable's object (keys the box-suppression set)
	name     string       // adjusted body-scope name (shadow renames applied)
	carrier  string       // clause-scope carrier name (name + ᴛN)
	boxDecl  string       // non-empty: heap-boxed — this fresh box decl opens each iteration
	copyBack bool         // body writes must flow back to the carrier before the post clause
}

// forClausePerIterVars decides which `for init; cond; post` clause-declared variables need
// Go 1.22+ per-iteration emission: any that a body func literal captures (the shared C# clause
// variable would leak the post-mutated final value into every closure), or that is heap-boxed
// (a stored `&i` must point to a DISTINCT box each pass — mirrors visitRangeStmt's deferred
// range-var box). Untouched loops emit exactly as before. A boxed variable that a CLAUSE func
// literal references keeps the legacy hoisted box instead: the body-scoped box would not be in
// scope at the clause, and that pathological capture retains the (pre-existing) whole-loop
// sharing rather than breaking the clause reference.
func (v *Visitor) forClausePerIterVars(forStmt *ast.ForStmt) []*forPerIterVar {
	assignStmt, ok := forStmt.Init.(*ast.AssignStmt)

	if !ok || assignStmt.Tok != token.DEFINE {
		return nil
	}

	var perIterVars []*forPerIterVar

	for _, lhs := range assignStmt.Lhs {
		ident, ok := lhs.(*ast.Ident)

		if !ok || ident.Name == "_" {
			continue
		}

		obj := v.info.Defs[ident]

		if obj == nil {
			continue
		}

		// Probe the box decl BEFORE the object joins forPerIterVars (which suppresses it).
		boxDecl := v.convertToHeapTypeDecl(ident, false)

		if boxDecl == "" && !v.funcLitReferences(forStmt.Body, obj) && !v.deferOrGoCalleeReferences(forStmt.Body, obj) {
			continue
		}

		if boxDecl != "" && (v.funcLitReferences(forStmt.Init, obj) || v.funcLitReferences(forStmt.Cond, obj) || v.funcLitReferences(forStmt.Post, obj)) {
			continue
		}

		name := v.getIdentName(ident)

		perIterVars = append(perIterVars, &forPerIterVar{
			ident:    ident,
			obj:      obj,
			name:     name,
			carrier:  v.getTempVarName(name),
			boxDecl:  boxDecl,
			copyBack: boxDecl != "" || v.loopVarWrittenIn(forStmt.Body, obj),
		})
	}

	return perIterVars
}

// funcLitReferences reports whether any func literal within node references obj.
func (v *Visitor) funcLitReferences(node ast.Node, obj types.Object) bool {
	if node == nil || obj == nil {
		return false
	}

	found := false

	ast.Inspect(node, func(n ast.Node) bool {
		if found {
			return false
		}

		funcLit, ok := n.(*ast.FuncLit)

		if !ok {
			return true
		}

		ast.Inspect(funcLit.Body, func(inner ast.Node) bool {
			if found {
				return false
			}

			if ident, ok := inner.(*ast.Ident); ok && v.info.Uses[ident] == obj {
				found = true
				return false
			}

			return true
		})

		return false
	})

	return found
}

// deferOrGoCalleeReferences reports whether a `defer` or `go` statement under node names obj in
// its CALLEE expression. Such a statement is a capture site even though the Go source contains no
// func literal: visitDeferStmt/visitGoStmt SYNTHESIZE one (`() => recv.M()`) for every callee the
// method-group form cannot express -- a result-returning method (`defer c[i].Close()`, net.Conn),
// a named func type (context.CancelFunc), a variadic callee. That synthesis happens long after
// this analysis runs, so without this arm the clause variable stays SHARED and the synthesized
// lambda reads its post-loop value. net's TestConcurrentSetDeadline is the witness: its
// `defer c[i].Close()` over a `[10]Conn` indexed c[10] when the defers fired and panicked with
// index out of range -- a hard failure, not a wrong answer.
//
// Only the CALLEE is consulted. A deferred call's ARGUMENTS are already evaluated at the defer
// statement, exactly as Go requires (`defer((t1, t2) => f(t1, t2), a, i, ref frame)`), so a clause
// variable appearing only in an argument needs no per-iteration copy -- and minting one would move
// emission at every such site for no behavioral gain.
func (v *Visitor) deferOrGoCalleeReferences(node ast.Node, obj types.Object) bool {
	if node == nil || obj == nil {
		return false
	}

	found := false

	ast.Inspect(node, func(n ast.Node) bool {
		if found {
			return false
		}

		var call *ast.CallExpr

		switch stmt := n.(type) {
		case *ast.DeferStmt:
			call = stmt.Call
		case *ast.GoStmt:
			call = stmt.Call
		default:
			return true
		}

		if call == nil {
			return true
		}

		ast.Inspect(call.Fun, func(inner ast.Node) bool {
			if found {
				return false
			}

			if ident, ok := inner.(*ast.Ident); ok && v.info.Uses[ident] == obj {
				found = true
				return false
			}

			return true
		})

		return true
	})

	return found
}

// loopVarWrittenIn reports whether node contains a statement that writes obj: an assignment with
// obj on the left (including a `:=` that REUSES it, and a `for obj = range …`), or an increment/
// decrement. Nested func literals are included — a closure invoked during the iteration writes
// the per-iteration variable. Writes through a taken address are not syntactically detectable;
// callers treat every heap-boxed variable as written.
func (v *Visitor) loopVarWrittenIn(node ast.Node, obj types.Object) bool {
	if node == nil || obj == nil {
		return false
	}

	written := false

	markIfObj := func(expr ast.Expr) {
		if ident, ok := expr.(*ast.Ident); ok && v.info.Uses[ident] == obj {
			written = true
		}
	}

	ast.Inspect(node, func(n ast.Node) bool {
		if written {
			return false
		}

		switch stmt := n.(type) {
		case *ast.AssignStmt:
			for _, lhs := range stmt.Lhs {
				markIfObj(lhs)
			}
		case *ast.IncDecStmt:
			markIfObj(stmt.X)
		case *ast.RangeStmt:
			if stmt.Tok == token.ASSIGN {
				if stmt.Key != nil {
					markIfObj(stmt.Key)
				}

				if stmt.Value != nil {
					markIfObj(stmt.Value)
				}
			}
		}

		return true
	})

	return written
}
