// deferFinallyLowering.go - Gbtc
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
	"strconv"
	"strings"
)

// Defer→finally lowering (capability 4 of DESIGN-zh-box-three-capabilities.md).
//
// A deferred call on a RECEIVER FIELD — `defer c.mu.Unlock()` — costs a ж-box per call: the
// registration must hold the receiver, and holding `c.mu` means boxing it. The call is already
// going to run on every exit path, and a C# `finally` already runs on every exit path, so where
// the two coincide the registration is pure overhead and the call can be emitted DIRECTLY into
// the frame's finally. Nothing is stored, so nothing is boxed.
//
// "Where the two coincide" is the whole difficulty, because Go's defer is not a finally:
//
//   - Go evaluates the deferred call's RECEIVER AND ARGUMENTS at REGISTRATION, not at unwind. A
//     finally evaluates them at unwind. Every gate below exists to make that difference
//     unobservable, and each one was measured over the corpus rather than assumed.
//   - A defer that was never REACHED never runs. A finally runs whenever its try was entered. The
//     per-site flag closes that: the finally calls only what the body actually reached.
//   - Go's defers are LIFO. The finally emits them in REVERSE source order.
//
// THE LIFO ARGUMENT, since the population is no longer top-level-only. Go runs deferred calls in
// reverse REGISTRATION order, and registration happens when control reaches the `defer`. With no
// loops, no backward jumps and no defers inside function literals, the CFG is a DAG whose every
// path visits structured statements in source order — an `if`, `switch` or `select` body is entered
// and left before the statement that follows it. So if `defer A` precedes `defer B` in source and
// both are reached, A registers first. Registration order is therefore source order RESTRICTED TO
// THE REACHED DEFERS, and the per-site flag makes an unreached defer a no-op. Emitting every site
// in reverse source order behind its flag yields exactly the reached ones, in reverse source order.
//
// The argument never assumes the defers are unconditional — only that control flows forward. That
// is why the conditional widening is admissible, and why B's `top-level` gate was stricter than
// correctness required.
//
// The gates (population measured over Go 1.23.12's std: 332 sites of this shape, 225 qualify):
//
//	shape         `<recv>.<field>.<Method>()` — the boxing shape, whose registration allocates a
//	              FieldRefBox — OR `<recv>.<Method>()`, whose registration allocates a delegate.
//	              Refusing the second is what made all-or-nothing reject `FD.Write`, `Pread`,
//	              `Pwrite`, `Seek` and log/slog's `handle`, each pairing one with the other.
//	lowerable     not inside a loop (which registers one site N times, beyond one flag's reach),
//	              not inside a function literal (whose defers belong to its own frame), and the
//	              function has no goto or label (a backward jump is the loop hazard renamed;
//	              measured at ZERO sites, so the exclusion the argument needs is free)
//	no arguments  arguments are evaluated at registration; storing them is the box again
//	no recover    the frame's catch/recover protocol is untouched by this cut
//	no named result   a named-result exit reads results AFTER the finally; not in this increment
//	no Goexit/Exit    a call that never returns must not have its finally semantics reasoned about
//	receiver stable   neither the receiver nor any prefix of the path is reassigned or addressed
//	                  — `defer c.mu.Unlock(); c = other` unlocks the ORIGINAL c's mutex in Go and
//	                  would unlock the NEW one here, silently
//	prefix dereferenced first   some node PROVABLY EXECUTED before the defer already dereferences
//	                  the same prefix, so a nil receiver panics there in BOTH forms and the moved
//	                  evaluation can never be the first panic. Measured at THREE sites with no
//	                  earlier dereference at all — a gate predicted empty and measured non-zero —
//	                  and at FOUR more whose only earlier dereference was CONDITIONAL, which
//	                  witnesses nothing; both are refused.
//	all-or-nothing    every defer in the function qualifies, or none is lowered. A function that
//	                  mixes registered and lowered defers would have to interleave two LIFO orders.
//
// The prefix gate walks OUT through enclosing blocks and counts an `if`/`switch` INIT and CONDITION
// (which always run when the statement is reached) while refusing their bodies — two corrections a
// written-down falsifier forced, each a gate stricter than correctness required.

// loweredDefer is one admitted site: the flag that records the body reached it, and the call
// rendered as an ordinary expression (filled in when visitDeferStmt reaches the statement).
type loweredDefer struct {
	flagName  string
	call      string
	aliasName string
}

// planDeferFinallyLowering decides, before the body is visited, which of this function's defer
// statements are emitted into the frame's finally instead of registered. It populates
// v.loweredDefers in SOURCE order and v.loweredDeferIndex for visitDeferStmt to look itself up in;
// both are empty when the function does not qualify, which is the overwhelming majority.
func (v *Visitor) planDeferFinallyLowering(funcDecl *ast.FuncDecl) {
	v.loweredDefers = nil
	v.loweredDeferIndex = nil

	if funcDecl == nil || funcDecl.Body == nil || !v.hasDefer {
		return
	}

	// The frame's recover protocol and the named-result exit are both out of scope for this
	// increment: the first shares the catch arm this cut does not touch, the second reads its
	// results after the finally has run.
	if v.hasRecover || v.namedReturnDeferMode {
		return
	}

	recvName := funcDeclReceiverName(funcDecl)

	if recvName == "" {
		return
	}

	// Go evaluates the receiver at registration. If the body can rebind it — or take its address,
	// after which this pass cannot prove it is not rebound — the lowered call would run against a
	// different receiver than Go's would.
	if receiverPathUnstable(funcDecl.Body, recvName) {
		return
	}

	// A call that does not return normally leaves the finally's relationship to Go's defer order
	// something to reason about rather than something to state. Measured at zero sites in the
	// qualifying population; refused regardless, because refusing costs nothing.
	if bodyReachesNonReturning(funcDecl.Body) {
		return
	}

	// A backward jump can re-reach a defer and register it twice, which a single flag cannot
	// express — the loop hazard without the loop syntax. Measured at ZERO sites in the qualifying
	// population, so this exclusion is free; it is here because the LIFO argument needs it, not
	// because the corpus does.
	if functionHasJump(funcDecl.Body) {
		return
	}

	// Collect the LOWERABLE defers in SOURCE order — every defer that is not inside a loop and not
	// inside a function literal, at any nesting depth. B admitted only the body's direct children;
	// that was a SIZING proxy carried into the emission, and the reached-flag makes a conditional
	// defer correct by construction (B2's first widening).
	lowerable := collectLowerableDefers(funcDecl.Body)

	if len(lowerable) == 0 {
		return
	}

	// All-or-nothing still: every defer the function has must be one of these. A defer in a loop or
	// in a literal is counted here and disqualifies the function, exactly as before.
	total := 0

	ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
		if _, ok := node.(*ast.DeferStmt); ok {
			total++
		}

		return true
	})

	if total != len(lowerable) {
		return
	}

	// Every site must carry a lowerable SHAPE and pass its own gates — all-or-nothing.
	for _, deferStmt := range lowerable {
		field := isReceiverFieldMethodCall(deferStmt.Call, recvName)
		method := isReceiverMethodCall(deferStmt.Call, recvName)

		if !field && !method {
			return
		}

		if len(deferStmt.Call.Args) != 0 || deferStmt.Call.Ellipsis.IsValid() {
			return
		}

		if !prefixDereferencedBefore(deferStmt, funcDecl.Body, !field) {
			return
		}
	}



	v.loweredDefers = make([]*loweredDefer, 0, len(lowerable))
	v.loweredDeferIndex = make(map[*ast.DeferStmt]int, len(lowerable))

	for i, deferStmt := range lowerable {
		v.loweredDeferIndex[deferStmt] = i
		v.loweredDefers = append(v.loweredDefers, &loweredDefer{
			flagName: GoFrameVar + "d" + strconv.Itoa(i+1),
		})
	}
}

// funcDeclReceiverName is the method receiver's name, or "" for a function, a blank receiver or an
// unnamed one — none of which can carry capability 4's shape.
func funcDeclReceiverName(funcDecl *ast.FuncDecl) string {
	if funcDecl.Recv == nil || len(funcDecl.Recv.List) == 0 || len(funcDecl.Recv.List[0].Names) == 0 {
		return ""
	}

	name := funcDecl.Recv.List[0].Names[0].Name

	if name == "_" {
		return ""
	}

	return name
}

// collectLowerableDefers returns, in SOURCE order, every defer that is not inside a loop and not
// inside a function literal — at any nesting depth.
//
// B collected only the body's direct children. That was a proxy for "unconditional", written for
// the CENSUS where over-admitting only mis-sizes a population, and carried into the EMISSION
// unexamined. It is stricter than correctness requires: the per-site reached-flag makes a
// CONDITIONAL defer correct by construction, because the flag is set at the defer's own position
// and the finally calls only what the body reached.
//
// A loop still cannot be admitted — it reaches one defer statement N times and registers it N
// times, which one boolean cannot express — and a literal's defers belong to the literal's own
// frame.
func collectLowerableDefers(body *ast.BlockStmt) []*ast.DeferStmt {
	var out []*ast.DeferStmt

	var walk func(node ast.Node)
	walk = func(node ast.Node) {
		switch n := node.(type) {
		case *ast.ForStmt, *ast.RangeStmt, *ast.FuncLit:
			return
		case *ast.DeferStmt:
			out = append(out, n)
			return
		}

		// Descend through everything else — blocks, conditionals, switches, selects — so a defer
		// nested in a branch is collected in the source order the LIFO argument relies on.
		var children []ast.Node

		ast.Inspect(node, func(c ast.Node) bool {
			if c == node || c == nil {
				return true
			}

			children = append(children, c)

			return false
		})

		for _, c := range children {
			walk(c)
		}
	}

	for _, stmt := range body.List {
		walk(stmt)
	}

	return out
}

// functionHasJump reports a goto or a label anywhere in the body. Reverse SOURCE order equals
// reverse REGISTRATION order only while control flows forward; a backward jump breaks that the way
// a loop does. Measured at zero sites in the qualifying population.
func functionHasJump(body *ast.BlockStmt) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		switch s := node.(type) {
		case *ast.LabeledStmt:
			found = true
		case *ast.BranchStmt:
			if s.Tok == token.GOTO {
				found = true
			}
		}

		return !found
	})

	return found
}

// isReceiverMethodCall reports B2's second shape: `<recv>.<Method>()`, a method on the RECEIVER
// ITSELF. Its registration allocates a DELEGATE rather than a FieldRefBox — the receiver's box is
// the method's own parameter and already exists — which is why B refused it as carrying no box to
// remove. It is worth lowering anyway (the delegate is a real allocation), and refusing it is what
// made all-or-nothing reject `FD.Write`, `Pread`, `Pwrite`, `Seek` and log/slog's `handle`, each of
// which pairs one with a receiver-FIELD defer.
func isReceiverMethodCall(call *ast.CallExpr, recvName string) bool {
	selectorExpr, ok := call.Fun.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	base, ok := selectorExpr.X.(*ast.Ident) // exactly ONE level: recv.Method, not recv.field.Method

	return ok && base.Name == recvName
}

// isReceiverFieldMethodCall reports capability 4's shape: `<recv>.<field>.<Method>()`, a method
// called on a FIELD of the receiver. That is the emission whose registration boxes the field.
func isReceiverFieldMethodCall(call *ast.CallExpr, recvName string) bool {
	outer, ok := call.Fun.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	inner, ok := outer.X.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	base, ok := inner.X.(*ast.Ident)

	return ok && base.Name == recvName
}

// receiverPathUnstable reports whether the body assigns to the receiver name or takes its address.
// Go binds the deferred call's receiver at REGISTRATION; a finally binds it at unwind. If the name
// can be rebound between the two, the lowered call runs against the wrong object — silently, with
// no compile error and no panic, which is the worst failure this cut could have.
func receiverPathUnstable(body *ast.BlockStmt, recvName string) bool {
	unstable := false

	ast.Inspect(body, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.AssignStmt:
			for _, lhs := range n.Lhs {
				if ident, ok := lhs.(*ast.Ident); ok && ident.Name == recvName {
					unstable = true
				}
			}
		case *ast.IncDecStmt:
			if ident, ok := n.X.(*ast.Ident); ok && ident.Name == recvName {
				unstable = true
			}
		case *ast.UnaryExpr:
			if n.Op == token.AND {
				if ident, ok := n.X.(*ast.Ident); ok && ident.Name == recvName {
					unstable = true
				}
			}
		}

		return !unstable
	})

	return unstable
}

// bodyReachesNonReturning reports a direct `runtime.Goexit()` or `os.Exit()` call in the body.
// Measured at zero sites in the qualifying population; refused regardless.
func bodyReachesNonReturning(body *ast.BlockStmt) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		call, ok := node.(*ast.CallExpr)

		if !ok {
			return !found
		}

		if sel, ok := call.Fun.(*ast.SelectorExpr); ok {
			if pkg, ok := sel.X.(*ast.Ident); ok {
				if (pkg.Name == "runtime" && sel.Sel.Name == "Goexit") ||
					(pkg.Name == "os" && sel.Sel.Name == "Exit") {
					found = true
				}
			}
		}

		return !found
	})

	return found
}

// prefixDereferencedBefore reports whether a statement BEFORE the defer already dereferences the
// deferred call's receiver prefix (`c.mu` for `defer c.mu.Unlock()`).
//
// Go evaluates that prefix AT the defer statement, so a nil `c` panics there and registers nothing.
// A lowered finally evaluates it at EXIT — so the body would run to completion first and the panic
// would surface late, with whatever the body did already committed. No flag repairs that: evaluating
// the prefix at registration means STORING it, which is the box this capability exists to remove.
//
// But when a statement ahead of the defer dereferences the identical prefix — the
// `c.mu.Lock(); defer c.mu.Unlock()` idiom that dominates the population — that statement panics
// first in BOTH forms, so the defer's own evaluation can never be the first panic and the
// divergence is unreachable.
//
// The match must be UNCONDITIONAL. A dereference inside an `if`, a `switch`, a `select`, a loop or
// a func literal proves nothing: that branch may not have run, so the defer's own evaluation can
// still be the first dereference and the divergence is back. Measured: FOUR of the qualifying sites
// leaned on exactly such a conditional match, so this is a populated hole rather than a theoretical
// one — the first form of this predicate accepted them.
//
// Measured: THREE sites in Go 1.23.12's std have no earlier dereference at all — a gate predicted
// empty, measured non-zero, and those three would have been mis-lowered without it.
// recvOnly selects the RECEIVER-METHOD form of the question — is the receiver itself dereferenced —
// rather than the receiver-FIELD form, which asks about `<recv>.<field>`.
func prefixDereferencedBefore(deferStmt *ast.DeferStmt, body *ast.BlockStmt, recvOnly bool) bool {
	outer, ok := deferStmt.Call.Fun.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	var want string

	if recvOnly {
		base, ok := outer.X.(*ast.Ident)

		if !ok {
			return false
		}

		want = base.Name
	} else {
		prefix, ok := outer.X.(*ast.SelectorExpr)

		if !ok {
			return false
		}

		base, ok := prefix.X.(*ast.Ident)

		if !ok {
			return false
		}

		want = base.Name + "." + prefix.Sel.Name
	}

	matches := func(sel *ast.SelectorExpr) bool {
		base, ok := sel.X.(*ast.Ident)

		if !ok {
			return false
		}

		if recvOnly {
			return base.Name == want
		}

		return base.Name+"."+sel.Sel.Name == want
	}

	for _, node := range provablyBefore(body.List, deferStmt) {
		found := false

		ast.Inspect(node, func(n ast.Node) bool {
			if _, isLit := n.(*ast.FuncLit); isLit {
				return false // a literal's body may never run either
			}

			if sel, ok := n.(*ast.SelectorExpr); ok && matches(sel) {
				found = true
			}

			return !found
		})

		if found {
			return true
		}
	}

	return false
}

// provablyBefore returns every node that is GUARANTEED to have executed by the time control reaches
// the defer, walking OUT through the enclosing blocks as well as along the defer's own.
//
// Two corrections live here, and each was a gate stricter than correctness required:
//
//   - The defer's own block is not enough. `x.b.touch(); if f { defer x.b.done() }` has its witness
//     at the OUTER level, before the `if`, and an outer statement that precedes the enclosing
//     construct runs before the inner block is entered.
//   - A control statement's BODY may not execute, but its INIT and CONDITION always do when the
//     statement is reached. `if err := fd.writeLock(); err != nil { … }` is the dominant Go idiom
//     for exactly the dereference this gate looks for, and skipping the whole IfStmt refused
//     `FD.Write` — the row this widening exists to reach.
//
// Loops, `select` and labels contribute nothing: nothing inside them is guaranteed to precede a
// defer that sits outside, and a defer inside them is excluded from the population anyway.
func provablyBefore(list []ast.Stmt, deferStmt *ast.DeferStmt) []ast.Node {
	var out []ast.Node

	for _, stmt := range list {
		if stmt == ast.Stmt(deferStmt) {
			break
		}

		// The defer is somewhere inside this statement: its unconditional parts run first, then
		// recurse into the branch that holds it. Nothing after it in this list can precede it.
		if stmt.Pos() <= deferStmt.Pos() && deferStmt.End() <= stmt.End() {
			switch s := stmt.(type) {
			case *ast.IfStmt:
				if s.Init != nil {
					out = append(out, s.Init)
				}

				if s.Cond != nil {
					out = append(out, s.Cond)
				}

				if s.Body != nil {
					out = append(out, provablyBefore(s.Body.List, deferStmt)...)
				}

				if s.Else != nil {
					if elseBlock, ok := s.Else.(*ast.BlockStmt); ok {
						out = append(out, provablyBefore(elseBlock.List, deferStmt)...)
					} else if elseIf, ok := s.Else.(*ast.IfStmt); ok {
						out = append(out, provablyBefore([]ast.Stmt{elseIf}, deferStmt)...)
					}
				}
			case *ast.BlockStmt:
				out = append(out, provablyBefore(s.List, deferStmt)...)
			case *ast.SwitchStmt:
				if s.Init != nil {
					out = append(out, s.Init)
				}

				if s.Tag != nil {
					out = append(out, s.Tag)
				}

				out = append(out, provablyBeforeInCases(s.Body, deferStmt)...)
			case *ast.TypeSwitchStmt:
				if s.Init != nil {
					out = append(out, s.Init)
				}

				out = append(out, provablyBeforeInCases(s.Body, deferStmt)...)
			}

			break
		}

		// A statement entirely ahead of the defer. Its unconditional parts count; its conditional
		// bodies do not.
		switch s := stmt.(type) {
		case *ast.IfStmt:
			if s.Init != nil {
				out = append(out, s.Init)
			}

			if s.Cond != nil {
				out = append(out, s.Cond)
			}
		case *ast.SwitchStmt:
			if s.Init != nil {
				out = append(out, s.Init)
			}

			if s.Tag != nil {
				out = append(out, s.Tag)
			}
		case *ast.TypeSwitchStmt:
			if s.Init != nil {
				out = append(out, s.Init)
			}
		case *ast.SelectStmt, *ast.ForStmt, *ast.RangeStmt, *ast.LabeledStmt:
			// nothing guaranteed
		default:
			out = append(out, stmt)
		}
	}

	return out
}

// provablyBeforeInCases descends into the ONE case clause holding the defer; sibling clauses do not
// execute.
func provablyBeforeInCases(body *ast.BlockStmt, deferStmt *ast.DeferStmt) []ast.Node {
	if body == nil {
		return nil
	}

	for _, clause := range body.List {
		caseClause, ok := clause.(*ast.CaseClause)

		if !ok || caseClause.Pos() > deferStmt.Pos() || deferStmt.End() > caseClause.End() {
			continue
		}

		return provablyBefore(caseClause.Body, deferStmt)
	}

	return nil
}


// reRootOnEntryBox re-roots a lowered call on the receiver's BOX when the receiver is reached
// through an entry deref alias.
//
// The alias (`ref var c = ref Ꮡc.DerefOrNull();`) is declared inside the frame's TRY, because a nil
// box must fault where the catch filter can turn it into a Go panic. The finally is outside that
// scope, so a call emitted there cannot name `c` — it would not compile. The box parameter itself is
// a method parameter and is in scope everywhere, so the call is re-rooted on the expression the
// alias was declared FROM, captured verbatim at the alias's own emission site.
//
// Nothing is allocated by this: the box already exists as the receiver parameter. And nothing can
// fault that would not have faulted anyway — the flag guarding the call is set only after the body
// reached the defer, which the prefix gate proves is after a dereference of the same path.
// It runs at the frame's TAIL rather than at the defer statement, because the alias map is filled
// while the function preamble is composed — which happens AFTER the body has been rendered (the
// preamble's own emission consults the rendered body text). So the defer records the alias name it
// rendered against and the substitution happens here, where both are known.
func (v *Visitor) reRootOnEntryBox(lowered *loweredDefer) string {
	if lowered.call == "" || lowered.aliasName == "" || len(v.entryAliasBoxPaths) == 0 {
		return lowered.call
	}

	boxPath, aliased := v.entryAliasBoxPaths[lowered.aliasName]

	if !aliased || !strings.HasPrefix(lowered.call, lowered.aliasName+".") {
		return lowered.call
	}

	return boxPath + strings.TrimPrefix(lowered.call, lowered.aliasName)
}

// loweredCallAliasName is the name the lowered call's receiver base rendered as — the key the
// finally's re-rooting looks up in entryAliasBoxPaths.
// Both shapes are handled: `<recv>.<field>.<Method>()` roots at the base of the inner selector, and
// `<recv>.<Method>()` roots at the base of the outer one.
func (v *Visitor) loweredCallAliasName(call *ast.CallExpr) string {
	outer, ok := call.Fun.(*ast.SelectorExpr)

	if !ok {
		return ""
	}

	base, ok := outer.X.(*ast.Ident)

	if !ok {
		inner, isSel := outer.X.(*ast.SelectorExpr)

		if !isSel {
			return ""
		}

		base, ok = inner.X.(*ast.Ident)

		if !ok {
			return ""
		}
	}

	return v.convIdent(base, DefaultIdentContext())
}

// goFrameLoweredFlagDecls renders the reached-flags, declared with the frame itself — BEFORE the
// try, because the finally reads them and a local declared inside a try is not in scope there.
//
// Every site carries a flag, including one whose defer is the body's FIRST statement. That is a
// deliberate departure from the shape first proposed for this cut: the function preamble the frame
// emits (parameter deref aliases, entry-time boxes) sits INSIDE the try, so a panic there reaches
// the finally, and an unflagged call would run on a path Go never registered it on. The store is a
// stack local; correctness is worth more than eliding it.
func (v *Visitor) goFrameLoweredFlagDecls(indentLevel int) string {
	if len(v.loweredDefers) == 0 {
		return ""
	}

	result := strings.Builder{}

	for _, lowered := range v.loweredDefers {
		result.WriteString(v.newline)
		result.WriteString(v.indent(indentLevel))
		result.WriteString(fmt.Sprintf("bool %s = false;", lowered.flagName))
	}

	return result.String()
}

// goFrameLoweredFinallyCalls renders the lowered calls for the frame's finally, in REVERSE source
// order — Go's LIFO — each guarded by the flag that records the body reached its defer statement.
// They run BEFORE the frame's own Run(), which re-throws an unrecovered panic and would otherwise
// leave them unexecuted.
func (v *Visitor) goFrameLoweredFinallyCalls() string {
	if len(v.loweredDefers) == 0 {
		return ""
	}

	result := strings.Builder{}

	for i := len(v.loweredDefers) - 1; i >= 0; i-- {
		lowered := v.loweredDefers[i]
		call := v.reRootOnEntryBox(lowered)

		if call == "" {
			continue
		}

		result.WriteString(fmt.Sprintf("if (%s) %s; ", lowered.flagName, call))
	}

	return result.String()
}
