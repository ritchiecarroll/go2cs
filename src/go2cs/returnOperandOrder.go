// returnOperandOrder.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns ONE property of a MULTI-VALUE `return`:
//
//	A plain operand is read AFTER the calls that share the return statement with it.
//
// Go's spec fixes the order of the CALLS in a return statement ("all function calls, method calls,
// receive operations, and binary logical operations are evaluated in lexical left-to-right order")
// and deliberately leaves the plain operands unordered against them. gc resolves that freedom the
// same way every time, because its order pass rewrites the statement: each call is spilled to a
// temporary FIRST, and the result list is then assembled from those temporaries and whatever plain
// operands remain — so every plain operand is read after every call. Measured at go1.23.12:
//
//	func mk() (OID, error) { var o OID; return o, o.fill() }        // o.der == [1 2 3]
//	func pair() (int, int) { var b box; return b.n, b.bump() }      // 1, 1
//	func trio() (int, int, int) { … return b.n, side(), b.bump() }  // "side" printed, then 1, 7, 1
//
// A C# tuple literal has no such freedom: `return (o, o.fill());` evaluates STRICTLY left to right,
// so `o` is copied BEFORE the call that fills it and the returned value is the pre-mutation one.
// The divergence is silent — both sides compile, both return two values, and only the CONTENT of
// the first differs. crypto/x509's `ParseOID` is the corpus instance: `return o, o.unmarshalOIDText(oid)`
// returned an EMPTY OID beside a nil error, so every parse "succeeded" with no bytes.
//
// The remedy is gc's own rewrite, emitted: spill the calls to temporaries ahead of the statement and
// let the tuple name them, which puts the plain operands' reads after the calls exactly as gc does.
// It is the RETURN-tuple sibling of lhsReusedInLaterRhs (visitAssignStmt), which routes a parallel
// assignment's read-after-write hazard through a deconstruction for the same reason: C# sequencing
// where Go has none.
//
// SCOPE — the spill fires only where the ordering is OBSERVABLE, so the emission stays where it is
// for every return that cannot diverge. That question is decided by comparing ACCESS PATHS (see
// storagePath): the storage an operand reads against the storage a later call can write.
// Deliberately NOT covered, each because deciding it needs more than the statement itself:
//
//   - an operand that CONTAINS a call of its own (`return o.f + g(), o.mutate()`) — gc spills g()
//     too and reads o.f last, so spilling the whole operand would re-create the problem rather than
//     fix it; the shape has no corpus instance;
//   - a pointer whose POINTEE no path can name — `f(getPtr())`, a pointer out of a call or a type
//     assertion — which is the interprocedural question one step removed; and
//   - a call that reaches the operand through a package-level variable or a captured closure
//     variable, which is interprocedural outright.
package main

import (
	"go/ast"
	"go/token"
	"go/types"
)

// storageStep is one hop along an access path, and its two kinds differ in exactly the way this
// file's conflict test turns on: a FIELD or ELEMENT step stays INSIDE the location it hangs off,
// while a DEREF leaves it for storage somewhere else entirely. That is what separates
// `return n, n.mutate()` — reading the pointer VALUE, which no write through it can change — from
// `return n.field, n.mutate()`, where the read is inside what the write covers.
type storageStep struct {
	deref bool
	// field names a struct field; nil with deref false is an ELEMENT step, which deliberately does
	// not distinguish one index from another — `s[i]` and `s[j]` compare equal, so an operand and a
	// call that reach into the same container conflict whatever their indices say.
	field *types.Var
}

// storagePath names a piece of storage as a root VARIABLE plus the hops from it to that storage. A
// single object plus an "indirect" flag cannot do this job: it reads `n.handler` and `*(n.pattern)`
// as the same storage — both merely "something behind n" — and net/http's
// `return n.handler, n.pattern.String(), n.pattern, matches` then spills a call that provably cannot
// touch what the first operand reads.
//
// A path is TRUNCATED rather than abandoned wherever a hop cannot be modelled exactly (a field
// promoted through embedding, most notably). A truncated path names a LARGER location than the
// expression does, so it can only ever over-report a conflict — never miss one.
type storagePath struct {
	root  types.Object
	steps []storageStep
}

// pathsConflict reports whether writing one of these locations can change what reading the other
// yields. They must share a root and agree on every hop they both have; the extra hops of the longer
// path then decide it — all FIELD/ELEMENT steps mean one location is contained in the other, while
// any DEREF means the longer path left for storage of its own.
//
//	n.handler  vs  *(n.pattern)    diverge at .handler/.pattern   — no conflict
//	n          vs  *n              extra [deref]                  — no conflict
//	n.handler  vs  *n              extra [.handler]               — CONFLICT
//	q.p.x      vs  *(q.p)          extra [.x]                     — CONFLICT
func pathsConflict(a, b storagePath) bool {
	if a.root == nil || a.root != b.root {
		return false
	}

	shared := len(a.steps)

	if len(b.steps) < shared {
		shared = len(b.steps)
	}

	for i := range shared {
		if a.steps[i] != b.steps[i] {
			return false
		}
	}

	longer := a.steps

	if len(b.steps) > len(a.steps) {
		longer = b.steps
	}

	for _, step := range longer[shared:] {
		if step.deref {
			return false
		}
	}

	return true
}

// pureBuiltins are the universe builtins that read their operands and write nothing, so a return
// operand containing one is still a plain READ for this file's purposes — and gc, which spills only
// genuine calls, reads it after the statement's calls exactly as it reads a bare identifier.
// `append`, `copy`, `delete`, `recover`, `panic`, `print`/`println`, `make` and `new` are absent
// deliberately: each either writes storage or has an effect that must keep its place.
var pureBuiltins = map[string]bool{
	"len":     true,
	"cap":     true,
	"real":    true,
	"imag":    true,
	"complex": true,
	"min":     true,
	"max":     true,
}

// returnMultiValueHoistThrough reports the HIGHEST result index whose call must be evaluated before
// the return's plain operands are read, or -1 when no operand can observe a later call's write.
//
// Every call-bearing operand at or below that index spills, not merely the hazardous one: the spec
// DOES fix the calls' order among themselves, so spilling `b.bump()` out of `return b.n, side(),
// b.bump()` while leaving `side()` in the tuple would run bump first. Operands ABOVE the index keep
// their place — their calls still run after the spilled ones (C# evaluates the tuple left to right),
// and no read below them was found to observe a write.
func (v *Visitor) returnMultiValueHoistThrough(results []ast.Expr) int {
	if len(results) < 2 {
		return -1
	}

	hazard := -1

	for i, read := range results {
		// An operand that makes a call of its own is not a plain read; see the file header.
		if v.exprEvaluatesCall(read) {
			continue
		}

		readPath, ok := v.storagePathOf(read)

		if !ok {
			continue
		}

		for j := i + 1; j < len(results); j++ {
			if j <= hazard || !v.exprEvaluatesCall(results[j]) {
				continue
			}

			if v.callWritesStorage(results[j], readPath) {
				hazard = j
			}
		}
	}

	return hazard
}

// exprEvaluatesCall reports whether evaluating expr performs an operation whose ORDER the spec
// fixes — a genuine call, or a channel RECEIVE, which that same sentence names alongside calls and
// which gc spills for the same reason. A type CONVERSION and a pure builtin (`len(o.der)`) are calls
// syntactically and reads semantically, so both answer false; gc does not spill them either. A FUNC
// LITERAL's body is not entered: the calls inside it run when the literal is invoked, not here.
func (v *Visitor) exprEvaluatesCall(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit {
			return false
		}

		// A receive is ordered WITH the calls, so an operand carrying one must spill alongside
		// them or the spill would reorder the two against each other. It reads no named storage,
		// so it is never a hazard READ either — storagePathOf answers no path for it.
		if unary, isUnary := n.(*ast.UnaryExpr); isUnary && unary.Op == token.ARROW {
			found = true

			return false
		}

		call, isCall := n.(*ast.CallExpr)

		if !isCall {
			return true
		}

		if isConversion, _ := v.isTypeConversion(call); isConversion {
			return true
		}

		if ident, ok := call.Fun.(*ast.Ident); ok && pureBuiltins[ident.Name] && v.identIsUniverseBuiltin(ident) {
			return true
		}

		found = true

		return false
	})

	return found
}

// storagePathOf resolves the location a plain operand copies out of, or ok=false when the operand
// reads no named storage at all (a literal, a composite literal, a call result, a channel receive).
func (v *Visitor) storagePathOf(expr ast.Expr) (storagePath, bool) {
	switch expr := expr.(type) {
	case *ast.ParenExpr:
		return v.storagePathOf(expr.X)
	case *ast.Ident:
		if variable, ok := v.info.ObjectOf(expr).(*types.Var); ok {
			return storagePath{root: variable}, true
		}
	case *ast.SelectorExpr:
		return v.selectorStoragePath(expr)
	case *ast.IndexExpr:
		base, ok := v.pathThroughBase(expr.X)

		if !ok {
			return storagePath{}, false
		}

		return appendStep(base, storageStep{}), true
	case *ast.StarExpr:
		base, ok := v.storagePathOf(expr.X)

		if !ok {
			return storagePath{}, false
		}

		return appendStep(base, storageStep{deref: true}), true
	}

	return storagePath{}, false
}

// selectorStoragePath resolves `X.Sel`. A PACKAGE-qualified variable (`os.Args`) has no base storage
// to reach through — the qualifier is a package name, not a value — so it is the root itself. A
// field promoted through EMBEDDING has a multi-hop index this path cannot spell exactly; the base is
// returned TRUNCATED there, naming the larger enclosing location, which can only over-report.
func (v *Visitor) selectorStoragePath(selector *ast.SelectorExpr) (storagePath, bool) {
	if ident, ok := selector.X.(*ast.Ident); ok {
		if _, isPkg := v.info.ObjectOf(ident).(*types.PkgName); isPkg {
			if variable, ok := v.info.ObjectOf(selector.Sel).(*types.Var); ok {
				return storagePath{root: variable}, true
			}

			return storagePath{}, false
		}
	}

	base, ok := v.pathThroughBase(selector.X)

	if !ok {
		return storagePath{}, false
	}

	selection, ok := v.info.Selections[selector]

	if !ok || selection.Kind() != types.FieldVal || len(selection.Index()) != 1 {
		return base, true
	}

	field, isField := selection.Obj().(*types.Var)

	if !isField {
		return base, true
	}

	return appendStep(base, storageStep{field: field}), true
}

// pathThroughBase resolves the location a selector or index reaches through, adding the DEREF hop
// when the base is a pointer (Go dereferences it implicitly, and the storage reached belongs to the
// pointee rather than to the pointer variable).
func (v *Visitor) pathThroughBase(base ast.Expr) (storagePath, bool) {
	path, ok := v.storagePathOf(base)

	if !ok {
		return storagePath{}, false
	}

	if _, isPointer := v.getType(base, false).(*types.Pointer); isPointer {
		path = appendStep(path, storageStep{deref: true})
	}

	return path, true
}

// appendStep returns a path with one more hop, copying the step slice so two paths grown from a
// shared prefix cannot alias one another's backing array.
func appendStep(path storagePath, step storageStep) storagePath {
	steps := make([]storageStep, len(path.steps), len(path.steps)+1)
	copy(steps, path.steps)

	return storagePath{root: path.root, steps: append(steps, step)}
}

// callWritesStorage reports whether any call inside expr receives storage that CONFLICTS with
// readPath, and so can change what the operand reading it yields. FUNC LITERAL bodies are skipped
// for the same reason exprEvaluatesCall skips them.
func (v *Visitor) callWritesStorage(expr ast.Expr, readPath storagePath) bool {
	writes := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if writes {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit {
			return false
		}

		call, isCall := n.(*ast.CallExpr)

		if !isCall {
			return true
		}

		if isConversion, _ := v.isTypeConversion(call); isConversion {
			return true
		}

		v.callWritePaths(call, func(written storagePath) {
			if pathsConflict(readPath, written) {
				writes = true
			}
		})

		return !writes
	})

	return writes
}

// callWritePaths reports every location the call receives BY ADDRESS — the exhaustive set of ways a
// call in a return statement can write storage another operand reads:
//
//	o.M()   where M has a POINTER receiver — the call takes &o, or writes *o when o is a pointer
//	f(&x)   an explicit address argument
//	f(p)    a pointer argument, which the callee can write through
//
// A value receiver and a value argument are copies and write nothing the caller can see.
func (v *Visitor) callWritePaths(call *ast.CallExpr, add func(storagePath)) {
	if selector, ok := call.Fun.(*ast.SelectorExpr); ok {
		if selection, ok := v.info.Selections[selector]; ok && selection.Kind() == types.MethodVal {
			if method, ok := selection.Obj().(*types.Func); ok {
				if signature, ok := method.Type().(*types.Signature); ok && signature.Recv() != nil {
					if _, isPointerRecv := signature.Recv().Type().(*types.Pointer); isPointerRecv {
						v.addReceiverWritePath(selector.X, add)
					}
				}
			}
		}
	}

	for _, arg := range call.Args {
		v.addArgumentWritePath(arg, add)
	}
}

// addReceiverWritePath adds the storage a POINTER-RECEIVER method call writes through. Two shapes,
// resolving to different locations: an addressable VALUE receiver (`o.fill()`) has its address taken
// implicitly, so the call writes o's own storage; a POINTER receiver (`p.fill()`) is already the
// address, so the call writes its pointee.
func (v *Visitor) addReceiverWritePath(receiver ast.Expr, add func(storagePath)) {
	path, ok := v.storagePathOf(receiver)

	if !ok {
		return
	}

	if _, isPointer := v.getType(receiver, false).(*types.Pointer); isPointer {
		path = appendStep(path, storageStep{deref: true})
	}

	add(path)
}

// addArgumentWritePath adds the location an ARGUMENT hands the call the address of: `&x` yields x's
// own storage, and any other pointer-valued argument yields its pointee's. A value argument is a
// copy and adds nothing.
func (v *Visitor) addArgumentWritePath(argument ast.Expr, add func(storagePath)) {
	switch expr := argument.(type) {
	case *ast.ParenExpr:
		v.addArgumentWritePath(expr.X, add)
		return
	case *ast.UnaryExpr:
		if expr.Op == token.AND {
			if path, ok := v.storagePathOf(expr.X); ok {
				add(path)
			}

			return
		}
	}

	if _, isPointer := v.getType(argument, false).(*types.Pointer); !isPointer {
		return
	}

	if path, ok := v.storagePathOf(argument); ok {
		add(appendStep(path, storageStep{deref: true}))
	}
}
