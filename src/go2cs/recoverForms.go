// recoverForms.go - Gbtc
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

// GO'S DIRECT-CALL RULE FOR recover (COORD ruling 2026-10-22, Option B). The spec: recover returns nil when it
// is not called directly by a deferred function. Whether a function IS "the deferred function" depends on how
// it was CALLED, not on where it was written: `defer catchError(&err)` recovers, while
// `defer func() { catchError(&err) }()` reads nil there and the panic continues. So the decision is made at run
// time, and the converter only marks the two ends of it:
//
//   - a named function whose body calls recover() opens with one line that takes golib's mark, and its recover
//     calls pass what it took:
//
//         var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?
//         ... recover(calledByDefer) ...
//
//   - every defer whose callee may be such a function registers with `recovers: true`, and golib marks the
//     thread just before that call (GoFrame.Recovering). A converter thunk between the registration and the
//     function is transparent: the function's first line is where the mark is taken.
//
// A function literal that IS the deferred literal (`defer func() { ... recover() ... }()`) is direct by
// construction and keeps `recover()`. A literal that is NOT (it reaches a defer through a func value) is checked
// by its caller at run time, `recoverChecked()`, which reads the stack only while a panic is waiting to be
// recovered. `defer recover()` defers recover itself, which is never "a function called by the deferred call",
// so its recover reads `recover(calledByDefer: false)` and the panic continues.
//
// STATED LIMITS. An EXPORTED function that calls recover is kept as it was (`recover()`): another package can
// defer it, and that defer site cannot know to set the mark, so the function cannot tell a deferred call from a
// helper call and recovers in both (the false-true case; package main exports nothing and is exempt). A defer
// whose callee is DYNAMIC (a func value, a func-typed field, an interface method) is marked only when its
// signature matches a recovering function of this package (for an interface method, a recovering method of the
// same name): a dynamic call cannot be resolved, and an unmatched one cannot reach such a function.

type recoverForm int

const (
	recoverFormPlain         recoverForm = iota // `recover()`: the deferred literal itself, or an exported function
	recoverFormFlagged                          // `recover(calledByDefer)`: a named function that took the mark
	recoverFormChecked                          // `recoverChecked()`: a literal that is not the deferred literal
	recoverFormNeverDeferred                    // `recover(calledByDefer: false)`: `defer recover()`
)

// recoverCallSite is one recover() call's form, and for recoverFormFlagged the local its function took the mark into.
type recoverCallSite struct {
	form  recoverForm
	local string
}

// Package-wide, computed once by collectRecoverForms and read by every file's Visitor.
var (
	packageRecoverForms       map[*ast.CallExpr]recoverCallSite // the recover() calls whose form is not recoverFormPlain
	packageRecovererFlagNames map[*ast.FuncDecl]string          // a function taking the mark -> its local's name
	packageDeferRecoverMarks  map[*ast.DeferStmt]bool           // a defer registered with `recovers: true`
)

// recoverFlagLocalName is the local a recovering function takes golib's mark into: named for what it means at
// the recover call, `recover(calledByDefer)`.
const recoverFlagLocalName = "calledByDefer"

// collectRecoverForms classifies every recover() call and every defer statement of the package (see above).
func collectRecoverForms(files []FileEntry, pkg *types.Package, info *types.Info) {
	packageRecoverForms = make(map[*ast.CallExpr]recoverCallSite)
	packageRecovererFlagNames = make(map[*ast.FuncDecl]string)
	packageDeferRecoverMarks = make(map[*ast.DeferStmt]bool)

	exportExempt := pkg != nil && pkg.Name() == "main"

	// `defer recover()`: the deferred call IS recover, so no enclosing function calls it directly.
	deferredRecovers := make(map[*ast.CallExpr]bool)
	directLiterals := make(map[*ast.FuncLit]bool)

	for _, entry := range files {
		ast.Inspect(entry.file, func(n ast.Node) bool {
			deferStmt, ok := n.(*ast.DeferStmt)

			if !ok {
				return true
			}

			if isBuiltinRecoverCall(deferStmt.Call, info) {
				deferredRecovers[deferStmt.Call] = true
				packageRecoverForms[deferStmt.Call] = recoverCallSite{form: recoverFormNeverDeferred}
			} else if funcLit, ok := ast.Unparen(deferStmt.Call.Fun).(*ast.FuncLit); ok {
				directLiterals[funcLit] = true
			}

			return true
		})
	}

	// The recovering named functions, and their signatures (receiver stripped) for the dynamic-callee match.
	recoverers := make(map[types.Object]bool)

	type recovererSignature struct {
		name   string
		method bool
		sig    *types.Signature
	}

	var signatures []recovererSignature

	for _, entry := range files {
		for _, decl := range entry.file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Body == nil {
				continue
			}

			calls := directRecoverCalls(funcDecl.Body, info, deferredRecovers)

			if len(calls) == 0 || funcDecl.Name.IsExported() && !exportExempt {
				continue
			}

			local := recoverFlagName(funcDecl)
			packageRecovererFlagNames[funcDecl] = local

			for _, call := range calls {
				packageRecoverForms[call] = recoverCallSite{form: recoverFormFlagged, local: local}
			}

			if obj, ok := info.Defs[funcDecl.Name].(*types.Func); ok {
				recoverers[obj] = true

				if sig, ok := obj.Type().(*types.Signature); ok {
					signatures = append(signatures, recovererSignature{funcDecl.Name.Name, sig.Recv() != nil, withoutReceiver(sig)})
				}
			}
		}

		// Function literals: the deferred literal keeps recover(); any other is checked by its caller.
		ast.Inspect(entry.file, func(n ast.Node) bool {
			funcLit, ok := n.(*ast.FuncLit)

			if !ok || directLiterals[funcLit] {
				return true
			}

			for _, call := range directRecoverCalls(funcLit.Body, info, deferredRecovers) {
				packageRecoverForms[call] = recoverCallSite{form: recoverFormChecked}
			}

			return true
		})
	}

	if len(recoverers) == 0 {
		return
	}

	matchesRecoverer := func(callee types.Type, methodName string) bool {
		sig, ok := callee.Underlying().(*types.Signature)

		if !ok {
			return false
		}

		plain := withoutReceiver(sig)

		for _, r := range signatures {
			if methodName != "" && (!r.method || r.name != methodName) {
				continue
			}

			if types.Identical(plain, r.sig) {
				return true
			}
		}

		return false
	}

	for _, entry := range files {
		ast.Inspect(entry.file, func(n ast.Node) bool {
			deferStmt, ok := n.(*ast.DeferStmt)

			if !ok || deferredRecovers[deferStmt.Call] {
				return true
			}

			if deferMayCallRecoverer(deferStmt.Call.Fun, info, recoverers, matchesRecoverer) {
				packageDeferRecoverMarks[deferStmt] = true
			}

			return true
		})
	}
}

// deferMayCallRecoverer reports whether a deferred call's callee is, or may dynamically be, a function of this
// package that takes the recover mark.
func deferMayCallRecoverer(fun ast.Expr, info *types.Info, recoverers map[types.Object]bool, matches func(types.Type, string) bool) bool {
	fun = ast.Unparen(fun)

	// An instantiated generic callee (`f[T](x)`) is its base function.
	switch indexed := fun.(type) {
	case *ast.IndexExpr:
		fun = ast.Unparen(indexed.X)
	case *ast.IndexListExpr:
		fun = ast.Unparen(indexed.X)
	}

	switch callee := fun.(type) {
	case *ast.FuncLit:
		return false
	case *ast.Ident:
		switch obj := info.Uses[callee].(type) {
		case *types.Func:
			return recoverers[obj.Origin()]
		case *types.Var:
			return matches(obj.Type(), "")
		default:
			return false
		}
	case *ast.SelectorExpr:
		if selection, ok := info.Selections[callee]; ok {
			switch {
			case selection.Kind() == types.FieldVal:
				return matches(selection.Type(), "")
			case types.IsInterface(selection.Recv()):
				return matches(selection.Type(), callee.Sel.Name)
			default:
				if method, ok := selection.Obj().(*types.Func); ok {
					return recoverers[method.Origin()]
				}

				return false
			}
		}

		// A qualified identifier: a function of another package never takes this package's mark, and a
		// package-level func value of another package is dynamic.
		switch obj := info.Uses[callee.Sel].(type) {
		case *types.Var:
			return matches(obj.Type(), "")
		default:
			return false
		}
	default:
		if t := info.TypeOf(fun); t != nil {
			return matches(t, "")
		}

		return false
	}
}

// directRecoverCalls returns the builtin recover() calls a function body makes itself: not those of a nested
// function literal, and not `defer recover()`, whose call the deferred-call machinery makes.
func directRecoverCalls(body *ast.BlockStmt, info *types.Info, deferredRecovers map[*ast.CallExpr]bool) []*ast.CallExpr {
	var calls []*ast.CallExpr

	ast.Inspect(body, func(n ast.Node) bool {
		switch node := n.(type) {
		case *ast.FuncLit:
			return false
		case *ast.CallExpr:
			if !deferredRecovers[node] && isBuiltinRecoverCall(node, info) {
				calls = append(calls, node)
			}
		}

		return true
	})

	return calls
}

func isBuiltinRecoverCall(call *ast.CallExpr, info *types.Info) bool {
	ident, ok := ast.Unparen(call.Fun).(*ast.Ident)

	if !ok || len(call.Args) != 0 {
		return false
	}

	builtin, ok := info.Uses[ident].(*types.Builtin)

	return ok && builtin.Name() == "recover"
}

// recoverFlagName names the local a recovering function takes the mark into: recoverFlagLocalName, unless the
// function already uses that identifier, when the converter's own ᒐ prefix keeps the two apart.
func recoverFlagName(funcDecl *ast.FuncDecl) string {
	taken := false

	ast.Inspect(funcDecl, func(n ast.Node) bool {
		if ident, ok := n.(*ast.Ident); ok && ident.Name == recoverFlagLocalName {
			taken = true
		}

		return !taken
	})

	if taken {
		return GoFrameVar + recoverFlagLocalName
	}

	return recoverFlagLocalName
}

// withoutReceiver is a signature's parameters and results alone, for comparing a method with a func value.
func withoutReceiver(sig *types.Signature) *types.Signature {
	return types.NewSignatureType(nil, nil, nil, sig.Params(), sig.Results(), sig.Variadic())
}

// hasRecoverMarkedDefer reports whether a function body registers a defer with `recovers: true` (see
// collectRecoverForms): defer-to-finally lowering leaves such a function as it was.
func (v *Visitor) hasRecoverMarkedDefer(body *ast.BlockStmt) bool {
	marked := false

	ast.Inspect(body, func(n ast.Node) bool {
		if deferStmt, ok := n.(*ast.DeferStmt); ok && packageDeferRecoverMarks[deferStmt] {
			marked = true
		}

		return !marked
	})

	return marked
}
