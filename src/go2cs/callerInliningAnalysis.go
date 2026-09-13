// callerInliningAnalysis.go - Gbtc
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
)

// computeNoInliningClosure identifies every package-scope function declaration that must be
// emitted [MethodImpl(NoInlining)] to keep runtime.Caller/runtime.Callers' skip-counted frame
// walk truthful under any JIT tiering configuration — not just the function whose body directly
// calls one of them, but every THIN FORWARDER (a body of exactly one statement, a call to another
// same-package function) that transitively reaches one. flag.FlagSet.Set -> .set is the measured
// case: set directly calls runtime.Caller(2) and needs the attribute on its own account, but Set
// itself never mentions runtime.Caller at all — it is still part of the frame chain the skip count
// assumes exists, and a Release-tier JIT inlines a single-statement forwarder like Set eagerly,
// which is exactly what silently shifts the count and turns a real file:line into "?:0". Confirmed
// by hand-marking both functions and re-running the failing test under Release+TieredCompilation=0
// (docs/phase4/MAILBOX.md, i9, 2026-08-30): marking set alone did not clear it, marking both did.
//
// Scope is deliberately "thin forwarder", not "every transitive caller": a function is only ever a
// risk here because it is SMALL enough for the JIT to want to inline it, and a body of one
// statement is the shape that risk is concentrated in. A large function that happens to call into
// this chain would not have been inlined anyway, so leaving it unmarked costs nothing and keeps
// the attribute from spreading past the functions that actually need it — the corpus-wide
// blast-radius this census measures is the cost the dispatch asked to see.
func computeNoInliningClosure(files []FileEntry, pkg *types.Package, info *types.Info) map[types.Object]bool {
	seed := map[types.Object]bool{}
	// forwarderTarget[fn] = the function fn's single statement forwards to, when fn's body has
	// exactly that shape. Built once per package; the fixed-point loop below only ever reads it.
	forwarderTarget := map[types.Object]types.Object{}
	// opaqueForwarders collects the function-adapter-shaped declarations found along the way (see
	// callsOpaqueFuncValue) whose disposition depends on whether the PACKAGE has a direct
	// runtime.Caller/Callers user anywhere — decided only after every file has been walked, since
	// that user can be declared in a LATER file than an adapter it needs to protect (io's
	// writerFunc.Write precedes TestMultiWriterSingleChainFlatten in multi_test.go's own
	// declaration order, so gating inline mid-walk would miss it).
	var opaqueForwarders []types.Object

	for _, entry := range files {
		if entry.file == nil {
			continue
		}
		for _, decl := range entry.file.Decls {
			fn, ok := decl.(*ast.FuncDecl)
			if !ok || fn.Name == nil || fn.Body == nil {
				continue
			}
			obj := info.ObjectOf(fn.Name)
			if obj == nil {
				continue
			}

			if callsSkipCountedRuntimeCaller(info, fn.Body) {
				seed[obj] = true
				continue
			}

			if target := thinForwarderTarget(info, pkg, fn.Body); target != nil {
				forwarderTarget[obj] = target
				continue
			}

			if callsOpaqueFuncValue(info, fn.Body) {
				opaqueForwarders = append(opaqueForwarders, obj)
			}
		}
	}

	// An opaque forwarder is marked ONLY in a package that already has a direct
	// runtime.Caller/Callers user somewhere — see callsOpaqueFuncValue's doc comment for why an
	// unconditional mark is wrong (net/http's HandlerFunc.ServeHTTP is the same shape and has
	// nothing to do with frame counting).
	if len(seed) > 0 {
		for _, obj := range opaqueForwarders {
			seed[obj] = true
		}
	}

	// Fixed point: a forwarder whose target just joined the set joins it too, which can chain
	// (a forwarder to a forwarder to the Caller-calling function). Bounded by the package's own
	// function count, so this always terminates.
	for changed := true; changed; {
		changed = false
		for fn, target := range forwarderTarget {
			if seed[fn] {
				continue
			}
			if seed[target] {
				seed[fn] = true
				changed = true
			}
		}
	}

	return seed
}

// callsOpaqueFuncValue reports whether body is a single-statement forwarder (return-of-call, or a
// bare call expression statement) whose callee is a *types.Var of function-signature type — the Go
// "function adapter" idiom that satisfies an interface by forwarding to an arbitrary func value
// (io's writerFunc/readerFunc: `func (f writerFunc) Write(p []byte) (int, error) { return f(p) }`,
// where f is the method's own function-typed receiver; net/http's HandlerFunc.ServeHTTP is the
// same shape via a bare ExprStmt call). Unlike thinForwarderTarget's named-function case, the
// actual callee is not resolvable statically — at runtime it can be ANY closure of the right
// signature, including one that calls runtime.Caller/Callers, which is exactly what io's flatten
// tests construct (writerFunc(func(p []byte) (int, error) { runtime.Callers(1, pc); ... })) and
// exactly what made TestMultiWriterSingleChainFlatten/TestMultiReaderFlatten keep failing — off by
// one frame per branch — after computeNoInliningClosure's named-target analysis alone had already
// marked both flatten tests themselves (docs/phase4/MAILBOX.md, i9, 2026-08-30): writerFunc.Write's
// own frame, not any func-literal frame, was the one still collapsing under Release+TC0.
//
// Because the target can't be resolved, the caller does not propagate through forwarderTarget's
// conditional fixed point — it seeds unconditionally, but ONLY when the enclosing package already
// has some direct runtime.Caller/Callers user (computeNoInliningClosure gates this). A package with
// no such user has nothing for an opaque forwarder to be protecting, and this idiom is common
// enough on genuinely hot paths — HandlerFunc.ServeHTTP dispatches every request — that marking it
// everywhere would be a real, unearned cost rather than a defensive one.
func callsOpaqueFuncValue(info *types.Info, body *ast.BlockStmt) bool {
	if len(body.List) != 1 {
		return false
	}

	var call *ast.CallExpr
	switch stmt := body.List[0].(type) {
	case *ast.ReturnStmt:
		if len(stmt.Results) != 1 {
			return false
		}
		call, _ = stmt.Results[0].(*ast.CallExpr)
	case *ast.ExprStmt:
		call, _ = stmt.X.(*ast.CallExpr)
	}

	if call == nil {
		return false
	}

	ident, ok := call.Fun.(*ast.Ident)

	if !ok {
		return false
	}

	callee, ok := info.Uses[ident].(*types.Var)

	if !ok {
		return false
	}

	_, isFuncValue := callee.Type().Underlying().(*types.Signature)

	return isFuncValue
}

// literalCallsSkipCountedRuntimeCaller reports whether funcLit's OWN body — not a nested literal's
// — directly calls runtime.Caller/runtime.Callers. The func-literal counterpart of
// callsSkipCountedRuntimeCaller: a closure has no types.Object (info.Defs/Uses carries no entry
// for an *ast.FuncLit itself), so it cannot be keyed into computeNoInliningClosure's
// map[types.Object]bool the way a *ast.FuncDecl is. convFuncLit calls this directly per literal
// instead — see Visitor.litNoInliningPrefix in visitFuncDecl.go.
//
// io's TestMultiWriterSingleChainFlatten/TestMultiReaderFlatten are the measured case: the
// runtime.Callers(1, pc) call that needs protecting sits inside a closure passed to
// writerFunc(...)/readerFunc(...), not in the named test function itself, so marking only
// *ast.FuncDecl bodies (as computeNoInliningClosure did first) left it uncovered — confirmed by
// reconverting under the fix with only the FuncDecl half landed: the attribute correctly reached
// the named test function, and the test still failed identically, because the actual call site was
// never examined at all (docs/phase4/MAILBOX.md, i9, 2026-08-30).
//
// Stops at a nested *ast.FuncLit exactly as the file's other own-body-only scans do (e.g.
// funcLitReturnArmTypes): a call inside a doubly-nested closure belongs to THAT closure's own
// analysis, not this one's.
func literalCallsSkipCountedRuntimeCaller(info *types.Info, funcLit *ast.FuncLit) bool {
	if funcLit == nil || funcLit.Body == nil || info == nil {
		return false
	}

	found := false

	ast.Inspect(funcLit.Body, func(n ast.Node) bool {
		if found {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit {
			return false // a nested literal's calls belong to it
		}

		sel, ok := n.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		used, ok := info.Uses[sel.Sel].(*types.Func)

		if !ok || used.Pkg() == nil || used.Pkg().Path() != "runtime" {
			return true
		}

		switch used.Name() {
		case "Caller", "Callers":
			found = true
			return false
		}

		return true
	})

	return found
}

// thinForwarderTarget reports the function object body forwards to when body is EXACTLY one
// statement — a return of a single call, or a bare call expression statement — calling a
// function declared in pkg (the package currently being converted) resolvable through info.
// Returns nil for anything else: multi-statement bodies, calls into another package, calls
// through an interface or function value, or calls whose target can't resolve statically.
func thinForwarderTarget(info *types.Info, pkg *types.Package, body *ast.BlockStmt) types.Object {
	stmts := body.List

	// A single leading GUARD CLAUSE — a plain `if <cond> { return ... }` with no else and a
	// one-statement body — does not disqualify the forwarding shape below it. internal/bisect's
	// Matcher.Stack is the measured case: `if m == nil { return true }; return m.stack(w)`, whose own
	// doc comment says "This lets stack's body handle m == nil and potentially be inlined" — Go's own
	// compiler inlines Stack too, and stack's skip-counted runtime.Callers(2, ...) assumes Stack's
	// frame is logically present regardless of physical inlining (confirmed: TestCmdBisect's bisect
	// output was every reported source line shifted by a constant offset until Stack itself carried
	// the attribute alongside stack — docs/phase4/MAILBOX.md, i9, 2026-08-30). Only this one guard
	// shape is recognized; anything else in a two-statement body (an else branch, a multi-statement
	// guard body, a guard that itself forwards a value) is not a proven case and falls through to the
	// length check below, which rejects it.
	if len(stmts) == 2 {
		ifStmt, ok := stmts[0].(*ast.IfStmt)

		if !ok || ifStmt.Else != nil || ifStmt.Init != nil || len(ifStmt.Body.List) != 1 {
			return nil
		}

		if _, ok := ifStmt.Body.List[0].(*ast.ReturnStmt); !ok {
			return nil
		}

		stmts = stmts[1:]
	}

	if len(stmts) != 1 {
		return nil
	}

	var call *ast.CallExpr
	switch stmt := stmts[0].(type) {
	case *ast.ReturnStmt:
		if len(stmt.Results) != 1 {
			return nil
		}
		call, _ = stmt.Results[0].(*ast.CallExpr)
	case *ast.ExprStmt:
		call, _ = stmt.X.(*ast.CallExpr)
	}
	if call == nil {
		return nil
	}

	var ident *ast.Ident
	switch fun := call.Fun.(type) {
	case *ast.Ident:
		ident = fun
	case *ast.SelectorExpr:
		// A same-package method reached through a receiver selector (f.set) resolves through
		// Sel, exactly like a free function does — the receiver expression itself plays no
		// part in WHICH function is targeted, only in which value it is called on.
		ident = fun.Sel
	default:
		return nil
	}

	target, ok := info.Uses[ident].(*types.Func)
	if !ok || target.Pkg() != pkg {
		return nil
	}
	return target
}
