// visitSwitchStmt.go - Gbtc
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
	"slices"
	"strings"
)

// caseBodyHasSwitchBreak reports whether a switch case body contains a bare `break` that targets the
// SWITCH itself — i.e. a `break` not enclosed by a nested loop/switch/select (which would catch it)
// or a function literal. A Go `break` inside a switch case exits the switch; when the switch is
// lowered to an `if / else if` chain (no enclosing C# switch), that `break` has no target (CS0139),
// so such a case body is wrapped in a `do { … } while (false)` whose `break` exits the case.
func caseBodyHasSwitchBreak(body []ast.Stmt) bool {
	found := false

	for _, stmt := range body {
		ast.Inspect(stmt, func(n ast.Node) bool {
			if found {
				return false
			}

			switch node := n.(type) {
			case *ast.ForStmt, *ast.RangeStmt, *ast.SwitchStmt, *ast.TypeSwitchStmt, *ast.SelectStmt, *ast.FuncLit:
				return false // a nested loop/switch/select/closure catches its own breaks
			case *ast.BranchStmt:
				if node.Tok == token.BREAK && node.Label == nil {
					found = true
					return false
				}
			}

			return true
		})
	}

	return found
}

// isTerminatingStmt reports whether a statement is a Go "terminating statement" (spec §Terminating
// statements) — one after which control cannot fall through. Used to decide whether a switch case can
// fall OUT of the switch: a non-terminating (and non-`fallthrough`) case means the switch is NOT terminal,
// so no unreachable trailing `return default!;` may follow it (see visitSwitchStmt). CONSERVATIVE by
// design — it returns false for any form it does not fully analyze (`for`/`switch`/`select`), which only
// forgoes the trailing return (leaving the rarer CS0161), NEVER a false "terminal" that would make the
// trailing return reachable and silently return the zero value. `fallthrough` is NOT terminating here
// (tracked separately as a case that continues into the next clause); a lone `break`/`continue` is not.
func isTerminatingStmt(stmt ast.Stmt, info *types.Info) bool {
	switch s := stmt.(type) {
	case *ast.ReturnStmt:
		return true
	case *ast.BranchStmt:
		return s.Tok == token.GOTO
	case *ast.LabeledStmt:
		return isTerminatingStmt(s.Stmt, info)
	case *ast.BlockStmt:
		return isTerminatingStmtList(s.List, info)
	case *ast.IfStmt:
		// Terminating only with an `else` where BOTH branches terminate.
		return s.Else != nil && isTerminatingStmt(s.Body, info) && isTerminatingStmt(s.Else, info)
	case *ast.ExprStmt:
		// A call to the built-in `panic` is terminating. A declaration SHADOWING `panic` makes the
		// call an ordinary one that returns normally (see identIsUniverseBuiltin), so it is not.
		if call, ok := s.X.(*ast.CallExpr); ok {
			if ident, ok := call.Fun.(*ast.Ident); ok && ident.Name == "panic" {
				if _, isBuiltin := info.ObjectOf(ident).(*types.Builtin); isBuiltin {
					return true
				}
			}
		}
	}

	return false
}

// isTerminatingStmtList reports whether a statement list terminates — its final non-empty statement is a
// terminating statement (Go spec: an empty trailing statement does not affect termination).
func isTerminatingStmtList(list []ast.Stmt, info *types.Info) bool {
	for i := len(list) - 1; i >= 0; i-- {
		if _, empty := list[i].(*ast.EmptyStmt); empty {
			continue
		}

		return isTerminatingStmt(list[i], info)
	}

	return false
}

// visitSwitchStmt wraps the core emission with the labeled-statement break target: a
// LABELED switch's `break Label` statements emit `goto break_Label` (a bare C# break
// exits only the innermost construct), so the label must be DECLARED after the switch
// (regexp/syntax parse.go's BigSwitch, CS0159 x11). Switches take no continue label.
func (v *Visitor) visitSwitchStmt(switchStmt *ast.SwitchStmt, target LabeledStmtContext) {
	v.visitSwitchStmtCore(switchStmt)

	if len(target.label) > 0 {
		v.outputBuilder.WriteString(v.newline)
		v.writeOutput("%s:;", getBreakLabelName(target.label))
	}
}

func (v *Visitor) visitSwitchStmtCore(switchStmt *ast.SwitchStmt) {
	// A tagged switch whose tag is the CONSTANT `true` (`switch c := s[i]; true { case cond: … }`
	// — strconv readFloat) is Go's idiom for an expressionless switch with an init statement:
	// each boolean case CONDITION matches when it holds. Normalize to the expressionless form
	// (nil tag) — the tagged routes would otherwise emit the conditions as C# case labels /
	// constant patterns, which must be compile-time constants (CS9135).
	tag := switchStmt.Tag

	if tag != nil {
		if tv, ok := v.info.Types[tag]; ok && tv.Value != nil && tv.Value.Kind() == constant.Bool && constant.BoolVal(tv.Value) {
			tag = nil
		}
	}

	var caseClauses []*ast.CaseClause
	var caseHasFallthroughStmt []bool

	for _, stmt := range switchStmt.Body.List {
		if caseClause, ok := stmt.(*ast.CaseClause); ok {
			caseClauses = append(caseClauses, caseClause)
			caseHasFallthroughStmt = append(caseHasFallthroughStmt, false)
		} else {
			v.showWarning("@visitSwitchStmt - unexpected Stmt type (non CaseClause) encountered in SwitchStmt: %T", stmt)
		}
	}

	allConst := true
	namedTypes := false
	hasFallthroughs := false
	defaultCaseFallsThrough := false

	// A TAG that is itself a CONSTANT emitted as `static readonly` — an untyped const's
	// UntypedInt wrapper (`switch goarch.PtrSize`, reflect abi.go) or a uintptr-struct const —
	// cannot govern a C# switch: the int case labels are not constants OF the wrapper struct
	// type (CS9135 ×2). Force the if-else (==) form, whose wrapper == operators compare
	// cleanly. The recorded tag TYPE is no help here (go/types records the untyped constant's
	// DEFAULT type in tag position), so gate on the object resolution: a constant-valued tag
	// that is not a true C# const. A variable tag (Value == nil) stays switchable. The same
	// tag also disables the `is` constant-pattern form below (`exprᴛ1 is 4` needs 4 to be a
	// constant OF the wrapper type — same CS9135); only the `==` operator compares cleanly.
	tagIsStaticReadonlyConst := false

	if tag != nil {
		if tv, ok := v.info.Types[tag]; ok && tv.Value != nil && !v.isCSharpConstantExpr(tag) {
			allConst = false
			tagIsStaticReadonlyConst = true
		}
	}

	// A tag whose type is a NAMED (non-interface) type renders as a [GoType] wrapper struct. An
	// untyped-LITERAL case label adopts the tag's named type in context (go/types records it on
	// the label), but its C# render is a bare literal of the UNDERLYING type: it can neither be a
	// constant pattern (`exprᴛ1 is 0x01` — CS9135, constant pattern against the wrapper; net/http
	// socksReply.String) nor compare bare (`exprᴛ1 == 0x01` is ambiguous between the wrapper's ==
	// and the underlying's built-in ==, both reachable through the wrapper's two-way implicit
	// operators). Force the if-else (==) form and cast such literal labels to the tag type.
	// (Ident/selector labels render at the wrapper type already and need no cast; the label
	// screening below cannot flag this case via namedTypes — it short-circuits per label once
	// allConst is false, so a mixed const-ident + literal switch never reaches its named check.)
	tagIsNamedWrapper := false
	var tagNamedType *types.Named

	if tag != nil {
		if named, ok := types.Unalias(v.getType(tag, false)).(*types.Named); ok {
			if tagIface, _ := isInterface(named); !tagIface {
				tagIsNamedWrapper = true
				tagNamedType = named
			}
		}
	}

	for i, caseClause := range caseClauses {
		if caseClause.List == nil {
			if i > 0 && caseHasFallthroughStmt[i-1] {
				defaultCaseFallsThrough = true
			}

			// A LEADING/MIDDLE default can itself fall through (ascii85's
			// `default: … fallthrough; case 3:`) — without recording it the next case
			// emitted `else if` after the bare default block (CS8641 cascade ×5).
			if caseClause.Body != nil {
				for _, stmt := range caseClause.Body {
					if branchStmt, ok := stmt.(*ast.BranchStmt); ok && branchStmt.Tok == token.FALLTHROUGH {
						hasFallthroughs = true
						caseHasFallthroughStmt[i] = true
					}
				}
			}

			continue
		}

		// Check if all case clauses are constant values
		for _, expr := range caseClause.List {
			// Check if the expression is a function call, a non-value type or is an untyped expression
			if !v.isNonCallValue(expr) || v.containsUntypedExpr(expr) {
				allConst = false
				break
			}

			// A C# `switch` case label must be a compile-time constant. An identifier/selector label
			// that resolves to anything other than a C# `const` — a plain variable, a struct field
			// (`frame.fp`), a const emitted as `static readonly` (untyped/named/cross-package, e.g.
			// `goarch.PtrSize`), or an address-of expression — is not, so a C# switch is invalid
			// (CS9135). containsUntypedExpr/the *types.Named check above miss these because the label
			// is typed to the switch tag's type in context. Force the if-else (==) form for them.
			switch labelExpr := expr.(type) {
			case *ast.Ident, *ast.SelectorExpr:
				if !v.isCSharpConstantExpr(expr) {
					allConst = false
				}
			case *ast.UnaryExpr:
				// An address-of (`&frame.fp` → `Ꮡframe.Value.fp`) is a runtime value, never a constant.
				if labelExpr.Op == token.AND {
					allConst = false
				}
			}

			if !allConst {
				break
			}

			tv, ok := v.info.Types[expr]

			if !ok {
				break
			}

			// A case label that is not even a GO compile-time constant (`case 1<<flt.expbits - 1:`
			// — flt is a variable; Go tagged-switch labels may be runtime expressions) can never
			// be a C# case label (CS9135). Ident/Selector/&-labels are screened above; this
			// catches the compound forms (binary/index/paren…). Force the if-else (==) form.
			if tv.Value == nil {
				allConst = false
				break
			}

			// Named typed are not constant values in C# conversion
			if _, ok := tv.Type.(*types.Named); ok {
				allConst = false
				namedTypes = true
				break
			}

			// A uintptr-typed label (even a plain literal — it adopts the switch tag's type in
			// context, so `case 4:` under `switch t.Size_` types as uintptr) can never be a C#
			// constant: uintptr is a golib STRUCT (golib/uintptr.cs), so a constant-label switch
			// over it is invalid (CS9135). Force the if-else (==) form.
			if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Kind() == types.Uintptr {
				allConst = false
				break
			}
		}

		// Check if any case clause has a fallthrough statement
		if caseClause.Body != nil {
			for _, stmt := range caseClause.Body {
				if branchStmt, ok := stmt.(*ast.BranchStmt); ok && branchStmt.Tok == token.FALLTHROUGH {
					hasFallthroughs = true
					caseHasFallthroughStmt[i] = true
				}
			}
		}
	}

	// When the switch lowers to an if/else-if chain (non-constant cases on a tagged switch), the
	// `default` clause becomes the trailing `else`. Go allows `default` in any position, but in the
	// chain a leading/middle default emits a bare `{ /* default: */ … }` followed by `else if`
	// (CS8641 "else cannot start a statement"). Move the single default clause to the end. This is
	// safe even when the switch has fallthroughs, AS LONG AS the default itself does not participate
	// in fallthrough — the preceding clause must not fall through into it and it must not fall
	// through out (those are source-order-sensitive); removing a fallthrough-independent default
	// preserves every other clause's fallthrough link. The C# `switch` branches accept `default`
	// anywhere, so this is only for the if/else-chain path.
	//
	// A default this does NOT move — because it participates in fallthrough, or because the switch
	// took the chain via hasFallthroughs with constant labels — is guarded IN PLACE on the
	// precomputed any-case-match below instead. Widening this gate to cover those was tried and
	// rejected: reordering moves the clause's STATEMENTS but comments attach by source position, so
	// regexp/syntax parseEscape's default-clause commentary migrated into the octal arm above it.
	// Guarding in place fixes the same defect with no code motion, and keeps the emitted C# in Go's
	// clause order.
	if !allConst && tag != nil {
		for i, caseClause := range caseClauses {
			if caseClause.List != nil || i == len(caseClauses)-1 {
				continue
			}

			fallsIntoDefault := i > 0 && caseHasFallthroughStmt[i-1]

			if fallsIntoDefault || caseHasFallthroughStmt[i] {
				break // default participates in fallthrough — keep source order
			}

			reorderedClauses := make([]*ast.CaseClause, 0, len(caseClauses))
			reorderedFlags := make([]bool, 0, len(caseClauses))

			for j := range caseClauses {
				if j == i {
					continue
				}

				reorderedClauses = append(reorderedClauses, caseClauses[j])
				reorderedFlags = append(reorderedFlags, caseHasFallthroughStmt[j])
			}

			reorderedClauses = append(reorderedClauses, caseClause)
			reorderedFlags = append(reorderedFlags, caseHasFallthroughStmt[i])
			caseClauses = reorderedClauses
			caseHasFallthroughStmt = reorderedFlags
			break
		}
	}

	if switchStmt.Init != nil {
		// Any declared variable will be scoped to switch statement, so create a sub-block for it
		v.outputBuilder.WriteString(v.newline)
		v.writeOutput("{")
		v.indentLevel++

		v.visitStmt(switchStmt.Init, []StmtContext{})
	}

	v.outputBuilder.WriteString(v.newline)

	if hasFallthroughs || (!allConst && tag != nil) {
		// Most complex scenario with standalone if's, and fallthrough
		exprVarName := v.getTempVarName("expr")

		var matchVarName string

		if hasFallthroughs {
			matchVarName = v.getTempVarName("match")
		}

		// A NON-TRAILING default participating in fallthrough (ascii85's leading
		// `default: … fallthrough`) must run only when NO case matches while executing in
		// SOURCE order — so a SECOND var PRECOMPUTES the OR of every case condition and the
		// default emits as `if (!anyMatch) { … }`. The running match flag keeps its exact
		// false-init + per-case set semantics (each case's `!match &&` prefix reads it, so
		// it cannot double as the precomputed predicate). Conditions are rendered ONCE here
		// (convExpr has side effects) and consumed by the emission loop below.
		// ANY default the reorder above left in a non-trailing position needs it — not just one that
		// falls through. The running match flag only reflects the arms emitted BEFORE the default, so
		// every guard built from it is wrong for a value one of the LATER cases matches: `!match` is
		// true, the default body runs, and those cases are dead code. Three shapes reach here, and the
		// corpus held one live instance of each of the first two:
		//
		//   FALLEN INTO — `case reflect.Interface: … fallthrough; default: …; case reflect.Array,
		//   reflect.Slice:` (encoding/json decode.go array). Emitted `if (fallthrough || !match)`, so
		//   every array and slice target took the error arm: json.Unmarshal failed EVERY JSON array
		//   whose target was not a bare interface ("cannot unmarshal array into Go value of type
		//   [1]interface {}" — net/rpc/jsonrpc passes params as [1]any — and the same for every []T).
		//
		//   BARE BLOCK — a leading/middle default in a constant-label switch with fallthroughs, which
		//   the reorder's gate does not cover. Emitted `{ /* default: */ … }` with no condition at
		//   all, so it ran unconditionally: internal/bisect parsePattern failed EVERY pattern with
		//   "invalid pattern syntax". (regexp/syntax parseEscape has the same shape and survived only
		//   because its default body re-tests !isalnum(c), excluding its own case labels by accident.)
		//
		//   FALLS OUT — `default: … fallthrough; case 3:` (ascii85), already handled here.
		nonTrailingDefault := false

		for ci, cc := range caseClauses {
			if cc.List == nil && ci < len(caseClauses)-1 {
				nonTrailingDefault = true
			}
		}

		var anyMatchVarName string

		if nonTrailingDefault {
			anyMatchVarName = v.getTempVarName("match")
		}

		caseConds := make([]string, len(caseClauses))
		caseUsesPattern := make([]bool, len(caseClauses))

		for ci, cc := range caseClauses {
			if cc.List == nil {
				continue
			}

			caseClauseCount := len(cc.List)
			usePattenMatch := !namedTypes && !tagIsStaticReadonlyConst && !tagIsNamedWrapper && v.canUsePatternMatch(caseClauseCount, cc, tag != nil)
			caseUsesPattern[ci] = usePattenMatch

			tagNeedsAreEqual := false

			if !usePattenMatch && tag != nil {
				if tagIface, _ := isInterface(v.getType(tag, false)); tagIface {
					tagNeedsAreEqual = true
				}
			}

			cond := &strings.Builder{}

			for li, expr := range cc.List {
				needAreEqualClose := false

				if li == 0 {
					if tag != nil {
						if usePattenMatch {
							cond.WriteString(exprVarName)
							cond.WriteString(" is ")
						} else if tagNeedsAreEqual {
							cond.WriteString(fmt.Sprintf("AreEqual(%s, ", exprVarName))
							needAreEqualClose = true
						} else {
							cond.WriteString(exprVarName)
							cond.WriteString(" == ")
						}
					}
				} else {
					if usePattenMatch {
						cond.WriteString(" or ")
					} else {
						cond.WriteString(" || ")

						if tag != nil {
							if tagNeedsAreEqual {
								cond.WriteString(fmt.Sprintf("AreEqual(%s, ", exprVarName))
								needAreEqualClose = true
							} else {
								cond.WriteString(exprVarName)
								cond.WriteString(" == ")
							}
						}
					}
				}

				context := DefaultPatternMatchExprContext()

				if usePattenMatch {
					context.usePattenMatch = true
					context.declareIsExpr = li == 0
				} else if caseClauseCount > 1 && tag == nil {
					cond.WriteRune('(')
				}

				labelExpr := v.convExpr(expr, []ExprContext{context})

				// A CONSTANT label under a named-wrapper tag renders from its underlying-typed
				// parts (a bare `0x01`), so the `==` is ambiguous between the wrapper's operator
				// and the underlying's built-in (see tagIsNamedWrapper above) — cast it to the tag
				// type. Ident/selector labels already render AT the wrapper type (a `static
				// readonly socksReply` const), and a conversion-call label spells its own type.
				if tagIsNamedWrapper {
					switch expr.(type) {
					case *ast.Ident, *ast.SelectorExpr, *ast.CallExpr:
					default:
						if tv, ok := v.info.Types[expr]; ok && tv.Value != nil {
							// getCSharpTypeName resolves lazily HERE — resolving it at switch entry
							// registers a file-local import alias for a cross-package tag type
							// even when no label ever needs the cast (an unused using).
							labelExpr = "(" + v.getCSharpTypeName(tagNamedType) + ")(" + labelExpr + ")"
						}
					}
				}

				cond.WriteString(labelExpr)

				if needAreEqualClose {
					cond.WriteRune(')')
				}

				if !usePattenMatch && caseClauseCount > 1 && tag == nil {
					cond.WriteRune(')')
				}
			}

			caseConds[ci] = cond.String()
		}

		if tag != nil {
			if v.options.preferVarDecl {
				v.writeOutput("var ")
			} else {
				exprType := convertToCSTypeName(v.getExpressionTypeName(tag, false))
				v.outputBuilder.WriteString(exprType)
				v.outputBuilder.WriteRune(' ')
			}

			v.outputBuilder.WriteString(exprVarName)
			v.outputBuilder.WriteString(" = ")
			v.outputBuilder.WriteString(v.convExpr(tag, v.switchTagExprContexts(tag)))
			v.outputBuilder.WriteString(";" + v.newline)
		}

		if hasFallthroughs {
			if v.options.preferVarDecl {
				v.writeOutput("var ")
			} else {
				v.writeOutput("bool ")
			}

			v.outputBuilder.WriteString(matchVarName)
			v.outputBuilder.WriteString(" = false;" + v.newline)
		}

		// Precomputed any-case-match (see above). Declared independently of the running match flag:
		// a non-trailing default needs it whether or not the switch has fallthroughs. Note this
		// evaluates every case label up front — a side-effecting label would reorder; Go
		// tagged-switch labels are near-universally constants.
		if anyMatchVarName != "" {
			parts := make([]string, 0, len(caseClauses))

			for ci, cc := range caseClauses {
				if cc.List == nil {
					continue
				}

				condPart := caseConds[ci]

				if len(cc.List) > 1 || strings.Contains(condPart, " || ") {
					condPart = "(" + condPart + ")"
				}

				parts = append(parts, condPart)
			}

			if v.options.preferVarDecl {
				v.writeOutput("var ")
			} else {
				v.writeOutput("bool ")
			}

			v.outputBuilder.WriteString(anyMatchVarName)
			v.outputBuilder.WriteString(" = ")
			v.outputBuilder.WriteString(strings.Join(parts, " || "))
			v.outputBuilder.WriteString(";" + v.newline)
		}

		// A `default:` reached via fallthrough is emitted as a GUARDED `if (fallthrough || !match)` block
		// (below), whose condition C# cannot prove always executes — so if the switch is the last
		// statement of a value-returning function and every case is terminal, C# reports CS0161 ("not all
		// code paths return a value") even though the Go `default` makes it exhaustive. Track that form to
		// emit an (unreachable) trailing `return default!;` after the if-chain — but ONLY when the switch
		// is genuinely terminal (allCasesTerminal below). A switch with a case that can fall OUT (a
		// `break`, or a body that neither returns nor falls through) is NOT terminal: reachable Go code
		// follows it, so a trailing `return default!;` there would execute and wrongly return the zero value.
		guardedTerminalDefault := false
		allCasesTerminal := true

		// A LEADING/MIDDLE `default` with no fallthrough guard emits as a BARE BLOCK
		// (`{ /* default: */ … }`) rather than an `if`, so nothing follows it that an `else` could
		// attach to — `} else if (…)` after a plain block is CS8641 ("'else' cannot start a
		// statement"), which is why the chain is broken there in the first place (regexp/syntax
		// parseEscape leads with `default:`). Track it so the else-chaining below skips such a clause.
		prevClauseIsBareBlock := false

		// Write "if" statements for each case clause
		for i, caseClause := range caseClauses {
			v.writeOutput("")

			caseFallsThrough := false
			thisClauseIsBareBlock := false

			// Case falls through if the previous case clause has a fallthrough statement
			if i > 0 && caseHasFallthroughStmt[i-1] {
				caseFallsThrough = true
			}

			nextClauseIsDefault := i < len(caseClauses)-1 && (i == len(caseClauses)-2 || caseClauses[i+1].List == nil)

			// Dropping the `else` is only sound when EVERY preceding clause terminates — otherwise the
			// clause is no longer chained and runs after a preceding clause already ran. That is harmless
			// for a `case` (its condition cannot match a value an earlier case matched) but silently WRONG
			// for `default`, which has no condition and therefore always executes: os's (*Process).wait
			// emitted `if (s == WAIT_OBJECT_0) { do { break; } while (false); } else if (s == WAIT_FAILED)
			// { return … } { /* default: */ return errors.New("os: unexpected result from
			// WaitForSingleObject") }` — the success path fell straight into the error return, so EVERY
			// child-process wait failed. `lastStatementWasReturn` only reports the IMMEDIATELY preceding
			// clause (here the returning WAIT_FAILED case), missing the non-terminating `break` case before
			// it; allCasesTerminal is the accumulated check the trailing-return logic below already relies
			// on (see its note that the shallow flag is wrong for this purpose). Keeping the original
			// condition as one arm makes this strictly ADD an `else` where one was missing — never remove
			// one — so no already-correct emission changes. A preceding BARE-BLOCK default takes no
			// `else` at all (see prevClauseIsBareBlock).
			if i > 0 && !caseFallsThrough && !prevClauseIsBareBlock && (!v.lastStatementWasReturn || !allCasesTerminal) {
				v.outputBuilder.WriteString("else ")
			}

			// Handle default case
			if caseClause.List == nil {
				nonTrailing := i < len(caseClauses)-1

				if caseFallsThrough {
					// A TRAILING default may read the running flag: every case has been tested by the
					// time control reaches it. A NON-TRAILING one may not (see nonTrailingDefault
					// above) — it reads the precomputed predicate over EVERY case label instead, which
					// is what makes the arms below it reachable.
					guardVarName := matchVarName

					if nonTrailing {
						guardVarName = anyMatchVarName
					}

					v.outputBuilder.WriteString(fmt.Sprintf("if (fallthrough || !%s) { /* default: */", guardVarName))

					// A trailing default emitted in the guarded form leaves C# unable to prove the switch
					// is exhaustive (see guardedTerminalDefault above).
					if !nonTrailing {
						guardedTerminalDefault = true
					}
				} else if nonTrailing {
					// A leading/middle default runs only when NO case matches (anyMatch is precomputed
					// above). This covers the clause that falls OUT into the next case (ascii85) and
					// equally the one that participates in no fallthrough at all — which used to emit a
					// BARE, unguarded block that ran every time (internal/bisect). Emitting an `if`
					// rather than a block also keeps the chain intact, so the next clause can take its
					// `else` (a plain block followed by `else if` is CS8641, which is why the reorder
					// above exists at all).
					v.outputBuilder.WriteString(fmt.Sprintf("if (!%s) { /* default: */", anyMatchVarName))
				} else if hasFallthroughs {
					// A TRAILING default in a switch WITH fallthroughs must be guarded on !match. The
					// else-if chain is broken by the fallthrough-TARGET if-blocks (a case rendered
					// `if (fallthrough || !match && <labels>) {…}`), so a bare `else` here is the else of
					// that separate if and fires after a matched NON-fallthrough case — fmt's printValue
					// ran `unknownType` (→ reflect name resolution) right after formatting an int/slice
					// element. Guarding on !match makes the default run only when nothing matched
					// (equivalent to the bare else in a pure else-if chain, correct in the broken chain).
					// Like the fallthrough-reached default, this leaves C# unable to prove exhaustiveness.
					v.outputBuilder.WriteString(fmt.Sprintf("if (!%s) { /* default: */", matchVarName))
					guardedTerminalDefault = true
				} else {
					v.outputBuilder.WriteString("{ /* default: */")
					thisClauseIsBareBlock = true
				}
			} else {
				caseClauseCount := len(caseClause.List)

				v.outputBuilder.WriteString("if (")

				usePattenMatch := caseUsesPattern[i]

				if caseFallsThrough {
					v.outputBuilder.WriteString(fmt.Sprintf("fallthrough || !%s && ", matchVarName))

					if caseClauseCount > 1 || !usePattenMatch && tag == nil {
						v.outputBuilder.WriteRune('(')
					}
				}

				// Condition pre-rendered above (single convExpr per label).
				v.outputBuilder.WriteString(caseConds[i])

				if hasFallthroughs {
					// Only close the wrapping paren when it was actually opened above
					// (same guard as the matching '(' write). Otherwise a fallthrough
					// case with a single pattern-matched value emits an unbalanced ')'.
					if caseFallsThrough && (caseClauseCount > 1 || !usePattenMatch && tag == nil) {
						v.outputBuilder.WriteRune(')')
					}

					v.outputBuilder.WriteString(") {")

					if !nextClauseIsDefault || defaultCaseFallsThrough {
						v.outputBuilder.WriteRune(' ')
						v.outputBuilder.WriteString(matchVarName)
						v.outputBuilder.WriteString(" = true;")
					}
				} else {
					v.outputBuilder.WriteString(") {")
				}
			}

			v.indentLevel++

			// A Go `break` targeting this (now if-else) switch has no C# target — wrap the body in a
			// `do { … } while (false)` so the `break` exits the case. Only when such a break exists,
			// to avoid disturbing every other case.
			switchBreakWrap := caseBodyHasSwitchBreak(caseClause.Body)

			if switchBreakWrap {
				// Emit `do {` on its own properly-indented line. writeOutput prepends the indent then
				// the text, so the leading newline must be written raw first — otherwise `do {` glues
				// onto the preceding `) {` with the indent spaces between them (`) {        do {`).
				v.outputBuilder.WriteString(v.newline)
				v.writeOutput("do {")
				v.indentLevel++

				// The wrapper is a C# ITERATION statement, so a Go `continue` in this body meaning
				// "continue the enclosing loop" must not be emitted bare — C# would bind it to the
				// wrapper, which exits on its false condition and falls through past the switch,
				// silently discarding the continue (`break` was retargeted by the wrapper; `continue`
				// is its symmetric twin). Record the wrapper so visitBranchStmt emits `goto` to the
				// enclosing loop's end-of-body label instead (see wrappedContinueLoopLabel); a nested
				// real loop inside the case pushes its own entry, keeping ITS continues bare.
				v.continueTargetStack = append(v.continueTargetStack, &continueTargetEntry{isWrapper: true})
			}

			for _, stmt := range caseClause.Body {
				v.visitListStmt(stmt)
			}

			if switchBreakWrap {
				v.continueTargetStack = v.continueTargetStack[:len(v.continueTargetStack)-1]
			}

			// A case whose body can fall OUT of the switch — it wraps a switch-`break`, or it is neither a
			// Go terminating statement list (isTerminatingStmtList — a genuine spec check, so an
			// `if { return }` WITHOUT an `else` or a possibly-zero-trip loop is correctly NON-terminating)
			// nor a fallthrough — makes the switch non-terminal, so no trailing `return default!;` may be
			// added (see guardedTerminalDefault). Using the shallow "last emitted line was a return" flag
			// here is WRONG: it false-positives on `if { return }` and lets a reachable `return default!;`
			// silently return the zero value.
			if switchBreakWrap || !(isTerminatingStmtList(caseClause.Body, v.info) || caseHasFallthroughStmt[i]) {
				allCasesTerminal = false
			}

			// `fallthrough` must be the final non-empty statement in its clause (Go spec), so
			// "control reached the end of the body" IS "the fallthrough statement was reached" —
			// which is why the flag can be raised at the end rather than at the statement. But in a
			// break-wrapped case that only holds INSIDE the wrap: a Go `break` exits the SWITCH, so
			// it must skip the fallthrough entirely. Raising the flag after `} while (false);` makes
			// every break fall through instead — silently, and only in a case that has both, which is
			// how `time.Parse` lost RFC3339's `Z` arm (it consumes the Z, breaks, and then re-entered
			// the numeric-offset arm on the remaining text: `cannot parse "Z07:00"` where Go reports
			// `extra text: "07:00"`).
			if caseHasFallthroughStmt[i] && switchBreakWrap {
				// The wrap's closer below writes its own leading newline, so this one terminates the
				// last body statement's line and the assignment is left unterminated for it.
				v.outputBuilder.WriteString(v.newline)
				v.writeOutput("fallthrough = true;")
			}

			if switchBreakWrap {
				v.indentLevel--
				// Likewise close on its own indented line. A `"%s} while…"` format would emit the
				// indent BEFORE the newline (trailing whitespace on the prior line) and drop the
				// `} while (false);` to column 0; write the newline raw, then the indented text.
				v.outputBuilder.WriteString(v.newline)
				v.writeOutput("} while (false);")
			}

			v.outputBuilder.WriteString(v.newline)

			if caseHasFallthroughStmt[i] && !switchBreakWrap {
				v.writeOutputLn("fallthrough = true;")
			}

			v.indentLevel--
			v.writeOutputLn("}")

			prevClauseIsBareBlock = thisClauseIsBareBlock
		}

		// A trailing `default:` in the guarded (fallthrough) form is a runtime-conditional `if` that C#
		// cannot prove exhaustive, so a value-returning function ending in this switch fails CS0161 ("not
		// all code paths return a value") even though the Go `default` makes it exhaustive. Emit an
		// unreachable `return default!;` — a guarded-terminal-default switch cannot be legally followed by
		// reachable Go code (the switch always returns/exits), so nothing else can run after it. Skip in
		// namedReturnDefer mode: there the body sits inside a `void` `func((defer, recover) => …)` wrapper,
		// so a value `return default!;` is CS8030 — and the void wrapper needs no trailing return anyway.
		if guardedTerminalDefault && allCasesTerminal && !v.namedReturnDeferMode && v.currentReturnSignature != nil && v.currentReturnSignature.Results().Len() > 0 {
			v.writeOutputLn("return default!;")
		}
	} else if allConst && tag != nil {
		// Most simple scenario when all case values are constant, a common C# switch will suffice
		v.writeOutput("switch (")
		v.outputBuilder.WriteString(v.convExpr(tag, v.switchTagExprContexts(tag)))
		v.outputBuilder.WriteString(") {")
		v.outputBuilder.WriteString(v.newline)

		for i, caseClause := range caseClauses {
			if i > 0 {
				v.outputBuilder.WriteString(v.newline)
			}

			if caseClause.List == nil {
				v.writeOutput("default: {")
			} else {
				for i, expr := range caseClause.List {
					if i == 0 {
						v.writeOutput("case ")
					} else {
						v.outputBuilder.WriteString(" or ")
					}

					v.outputBuilder.WriteString(v.convExpr(expr, nil))

					if i == len(caseClause.List)-1 {
						v.outputBuilder.WriteString(": {")
					}
				}
			}

			v.indentLevel++

			// visitStmt resets this per statement, but an EMPTY case body (a comment-only final
			// `case idle >= 0 && interval >= 0:` — net tcpsockopt_windows) never calls visitStmt,
			// so the flag would carry the PREVIOUS clause's terminal `return` at the SAME indent
			// and suppress the mandatory `break;` (CS8070 on a final case, CS0163 otherwise).
			v.lastStatementWasReturn = false

			for _, stmt := range caseClause.Body {
				v.visitListStmt(stmt)
			}

			v.outputBuilder.WriteString(v.newline)

			if !v.lastStatementWasReturn || v.lastReturnIndentLevel != v.indentLevel {
				v.writeOutputLn("break;")
			}

			v.indentLevel--
			v.writeOutput("}")
		}

		v.outputBuilder.WriteRune('}')
		v.outputBuilder.WriteString(v.newline)
	} else {
		// Most common scenario with expression switches
		v.writeOutput("switch (%s) {%s", TrueMarker, v.newline)

		for clauseIndex, caseClause := range caseClauses {
			if clauseIndex > 0 {
				v.outputBuilder.WriteString(v.newline)
			}

			if caseClause.List == nil {
				v.writeOutput("default: {")
			} else {
				// Use pattern match when all case list expressions are
				// use comparison operators and the same target
				caseClauseCount := len(caseClause.List)
				usePattenMatch := !namedTypes && !tagIsStaticReadonlyConst && v.canUsePatternMatch(caseClauseCount, caseClause, tag != nil)

				for i, expr := range caseClause.List {
					if i == 0 {
						v.writeOutput("case {} when ")
					} else {
						if usePattenMatch {
							v.outputBuilder.WriteString(" or ")
						} else {
							v.outputBuilder.WriteString(" || ")
						}
					}

					context := DefaultPatternMatchExprContext()

					if usePattenMatch {
						context.usePattenMatch = true
						context.declareIsExpr = i == 0
					} else if caseClauseCount > 1 {
						v.outputBuilder.WriteRune('(')
					}

					// A constant-TRUE case condition ahead of other clauses (`switch {
					// case true: … case cond: …}` — time parseStrictRFC3339 deliberately
					// disabling its strict checks) makes every LATER case provably
					// unreachable, which Go compiles as dead code but C# rejects (CS8120
					// ×3). Emit the golib `ᐧᐧ` marker (a static readonly bool the compiler
					// cannot fold) instead of the literal; a trailing constant-true clause
					// keeps the literal (nothing follows to be unreachable — no churn).
					caseIsFoldableTrue := false

					if clauseIndex < len(caseClauses)-1 {
						if tv, ok := v.info.Types[expr]; ok && tv.Value != nil && tv.Value.Kind() == constant.Bool && constant.BoolVal(tv.Value) {
							caseIsFoldableTrue = true
						}
					}

					if caseIsFoldableTrue {
						v.outputBuilder.WriteString(OpaqueTrueMarker)
					} else {
						v.outputBuilder.WriteString(v.convExpr(expr, []ExprContext{context}))
					}

					if !usePattenMatch && caseClauseCount > 1 {
						v.outputBuilder.WriteRune(')')
					}

					if i == caseClauseCount-1 {
						v.outputBuilder.WriteString(": {")
					}
				}
			}

			v.indentLevel++

			// visitStmt resets this per statement, but an EMPTY case body (a comment-only final
			// `case idle >= 0 && interval >= 0:` — net tcpsockopt_windows) never calls visitStmt,
			// so the flag would carry the PREVIOUS clause's terminal `return` at the SAME indent
			// and suppress the mandatory `break;` (CS8070 on a final case, CS0163 otherwise).
			v.lastStatementWasReturn = false

			for _, stmt := range caseClause.Body {
				v.visitListStmt(stmt)
			}

			v.outputBuilder.WriteString(v.newline)

			if !v.lastStatementWasReturn || v.lastReturnIndentLevel != v.indentLevel {
				v.writeOutputLn("break;")
			}

			v.indentLevel--
			v.writeOutput("}")
		}

		v.outputBuilder.WriteRune('}')
		v.outputBuilder.WriteString(v.newline)
	}

	// Close any locally scoped declared variable sub-block
	if switchStmt.Init != nil {
		v.indentLevel--
		v.writeOutputLn("}")
	}
}

func (v *Visitor) containsUntypedExpr(expr ast.Expr) bool {
	// First check the type information directly
	if t := v.info.TypeOf(expr); t != nil {
		if basic, ok := t.Underlying().(*types.Basic); ok {
			isUntyped := basic.Info()&types.IsUntyped != 0
			if isUntyped {
				return true
			}
		}
	}

	// Handle identifiers specifically (including references to untyped constants)
	if ident, ok := expr.(*ast.Ident); ok {
		// Check if this identifier refers to a constant
		if obj := v.info.ObjectOf(ident); obj != nil {
			// Check if it's a constant
			if cnst, ok := obj.(*types.Const); ok {
				// Check if the constant has an untyped type
				if basic, ok := cnst.Type().(*types.Basic); ok {
					isUntyped := basic.Info()&types.IsUntyped != 0
					return isUntyped
				}
			}
		}

		// Also check type-and-value information
		if tv, ok := v.info.Types[ident]; ok {
			if tv.IsValue() && tv.Value != nil && tv.Type != nil {
				if basic, ok := tv.Type.Underlying().(*types.Basic); ok {
					isUntyped := basic.Info()&types.IsUntyped != 0
					return isUntyped
				}
			}
		}
	}

	// Recursive checking for compound expressions
	switch e := expr.(type) {
	case *ast.BinaryExpr:
		return v.containsUntypedExpr(e.X) || v.containsUntypedExpr(e.Y)
	case *ast.UnaryExpr:
		return v.containsUntypedExpr(e.X)
	case *ast.StarExpr:
		return v.containsUntypedExpr(e.X)
	case *ast.ParenExpr:
		return v.containsUntypedExpr(e.X)
	case *ast.SelectorExpr:
		return v.containsUntypedExpr(e.X)
	case *ast.IndexExpr:
		return v.containsUntypedExpr(e.X) || v.containsUntypedExpr(e.Index)
	case *ast.SliceExpr:
		untyped := v.containsUntypedExpr(e.X)

		if e.Low != nil {
			untyped = untyped || v.containsUntypedExpr(e.Low)
		}

		if e.High != nil {
			untyped = untyped || v.containsUntypedExpr(e.High)
		}

		if e.Max != nil {
			untyped = untyped || v.containsUntypedExpr(e.Max)
		}

		return untyped
	case *ast.CallExpr:
		return slices.ContainsFunc(e.Args, v.containsUntypedExpr)
	case *ast.CompositeLit:
		if e.Type != nil && v.containsUntypedExpr(e.Type) {
			return true
		}

		return slices.ContainsFunc(e.Elts, v.containsUntypedExpr)
	case *ast.KeyValueExpr:
		return v.containsUntypedExpr(e.Key) || v.containsUntypedExpr(e.Value)
	}

	return false
}

func (v *Visitor) canUsePatternMatch(caseClauseCount int, caseClause *ast.CaseClause, inferExprTarget bool) bool {
	usePattenMatch := true

	// Verify that all expressions are not untyped (do not pattern match with implicitly type conversions)
	if slices.ContainsFunc(caseClause.List, v.containsUntypedExpr) {
		return false
	}

	if inferExprTarget {
		// Verify that all expressions are non-call values
		for _, expr := range caseClause.List {
			if !v.isNonCallValue(expr) {
				usePattenMatch = false
				break
			}

			// A C# constant pattern (`tag is Y`) requires Y to be a compile-time constant. An
			// identifier/selector case label that resolves to a const emitted as `static readonly`
			// (untyped/named/cross-package, e.g. `goarch.PtrSize`) or to a plain variable is not, so
			// the `is` form is invalid (CS9135/CS0150). An INDEX-expression label (`case Typ[UntypedNil]:`
			// — an index into a package-level array var, go/types operand.go) is never a constant either,
			// and in pattern position C# parses `Typ[UntypedNil]` as an array TYPE (CS0246/CS0270). Fall
			// back to `==` equality for all of these. (Computed literal expressions like `a + b` remain
			// valid constant patterns and are left untouched.)
			switch expr.(type) {
			case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr:
				if !v.isCSharpConstantExpr(expr) {
					usePattenMatch = false
				}
			}

			// A uintptr-typed label — even a plain literal, which adopts the tag's type in context
			// (`exprᴛ1 is 4` under a uintptr tag) — can never be a constant pattern: uintptr is a
			// golib STRUCT (CS9135). Fall back to `==` (the struct's operator).
			//
			// A POINTER-typed label is the same case one type over, and it is NOT covered by the
			// syntactic screen above: that switch lists Ident/Selector/Index, so an ADDRESS-OF label
			// (`case &inProgress:` — 1.24 runtime/type.go's GC-mask sentinel) fell through it and
			// emitted `exprᴛ1 is ᏑinProgress`, a constant pattern over a runtime value: CS9135 at the
			// pattern operand, while the `case nil` arm ONE LINE DOWN already spelled `== default!`.
			// The sibling screen that forces the whole switch to the if/else form (see the allConst
			// block) does list UnaryExpr, which is why the chain existed at all and only its operator
			// was wrong. Screening by TYPE rather than by expression kind keeps the two lists from
			// diverging again: no pointer value is a C# compile-time constant, whatever syntax names
			// it. `==` is also the RIGHT operator — on `ж<T>` it is pointer identity (ReferenceEquals
			// or equal order tokens), which is exactly what Go's `p == &inProgress` compares.
			if tv, ok := v.info.Types[expr]; ok && tv.Type != nil {
				switch underlying := tv.Type.Underlying().(type) {
				case *types.Pointer:
					usePattenMatch = false
				case *types.Basic:
					if underlying.Kind() == types.Uintptr {
						usePattenMatch = false
					}
				}
			}

			if !usePattenMatch {
				break
			}

			// A compound (binary) case label under a TAGGED switch cannot use the `is` form:
			// convBinaryExpr's pattern arm renders comparison OPERANDS (`X is <op> Y`), yielding
			// `tag is X is <op> Y` — never valid — and a runtime label is no constant pattern
			// anyway (CS0150). Fall back to `==` equality for the clause.
			if _, ok := expr.(*ast.BinaryExpr); ok {
				usePattenMatch = false
				break
			}
		}

		return usePattenMatch
	}

	if caseClauseCount > 0 {
		var firstExpr string

		for i, expr := range caseClause.List {
			// Check if expression uses comparison operators
			binaryExpr, ok := expr.(*ast.BinaryExpr)

			if !ok {
				usePattenMatch = false
				break
			}

			if !isComparisonOperator(binaryExpr.Op) {
				usePattenMatch = false
				break
			}

			// Check for bitwise operations in the left operand
			if containsBitwiseOperation(binaryExpr.X) {
				usePattenMatch = false
				break
			}

			// Make sure no rhs expression targets are call expressions
			if !v.isNonCallValue(binaryExpr.Y) {
				usePattenMatch = false
				break
			}

			// A relational/constant pattern (`x is <op> Y`) requires Y to be a C# compile-time
			// constant. A variable (`case x == y`, y a parameter) or a const emitted as
			// `static readonly` (untyped/named/cross-package) makes the pattern invalid (CS9135);
			// fall back to a `when` guard for those.
			if !v.isCSharpConstantExpr(binaryExpr.Y) {
				usePattenMatch = false
				break
			}

			// Check if all lhs expression targets are the same
			if i == 0 {
				firstExpr = v.convExpr(binaryExpr.X, nil)
			} else if firstExpr != v.convExpr(binaryExpr.X, nil) {
				usePattenMatch = false
				break
			}
		}
	} else {
		usePattenMatch = false
	}

	return usePattenMatch
}

// containsBitwiseOperation reports whether expr performs a bitwise operation anywhere inside it,
// looking through parentheses.
//
// A C# relational pattern (`x is > 3`) accepts only a simple subject, so `case x&mask == 0` cannot
// become one — the `x&mask` subject would have to be evaluated first. The switch emitter uses this
// to fall back to a `when` guard for those arms.
func containsBitwiseOperation(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.BinaryExpr:
		return isBitwiseOperator(e.Op) ||
			containsBitwiseOperation(e.X) ||
			containsBitwiseOperation(e.Y)
	case *ast.ParenExpr:
		return containsBitwiseOperation(e.X)
	default:
		return false
	}
}

// isBitwiseOperator reports whether op is one of Go's bit-manipulation operators, including the
// shifts and Go's `&^` (AND NOT), which C# spells as `& ~`.
func isBitwiseOperator(op token.Token) bool {
	return op == token.AND || op == token.OR || op == token.XOR ||
		op == token.SHL || op == token.SHR || op == token.AND_NOT
}

// switchTagExprContexts returns the contexts the switch TAG is converted under.
//
// A tag is an OPERAND OF A COMPARISON — the lowered if/else chain compares it against every case
// label — so it takes the same pointer context convBinaryExpr gives a comparison operand, and for
// the same reason. A pointer-typed parameter that has been ref-lowered is shadowed in its own body
// by `ref var x = ref Ꮡx.DerefOrNull()`, so an identifier read WITHOUT the pointer context renders
// the POINTEE. Under the nil context this used to pass, the tag of 1.24 runtime/lock_spinbit.go's
// `switch l { case &sched.lock: }` — *mutex against *mutex in Go — emitted the dereferenced VALUE
// and compared it against a pointer: CS0019 at the comparison.
//
// The quieter half is why this is not merely a compile defect. On a `case nil:` arm the same tag
// emits `exprᴛ1 == default!`, which compares the POINTEE against its ZERO VALUE and COMPILES — a
// nil test that silently answers "is the pointee zero". A shape that reads as a wrong answer rather
// than as an error, and the reason the fix is not scoped to the arms that fail to build.
//
// The predicate is convBinaryExpr's, TRANSCRIBED rather than paraphrased so the two cannot drift:
// pointer or erased pointer core, excluding an interface tag (which already holds the box) and
// unsafe.Pointer (whose pointer arm renders `.Value`, the raw uintptr, and NullReferences on a nil
// one — the sync/atomic TestLoadPointer family). A non-pointer tag gets the default context, so
// every other switch in the corpus is untouched.
func (v *Visitor) switchTagExprContexts(tag ast.Expr) []ExprContext {
	identContext := DefaultIdentContext()
	basicLitContext := DefaultBasicLitContext()

	if tag != nil {
		tagType := v.info.TypeOf(tag)
		tagIsInterface, _ := isInterface(tagType)

		identContext.isPointer = (isPointer(tagType) || v.typeIsErasedPointerCore(tagType)) &&
			!tagIsInterface && !isUnsafePointer(tagType)
	}

	return []ExprContext{identContext, basicLitContext}
}
