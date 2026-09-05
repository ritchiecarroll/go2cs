// escapeAnalysisOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"runtime/debug"
	"strings"
	"sync"
)

// The escape analysis function is used to determine if a variable escapes the current
// stack and thus needs to be heap allocated. This is important for C# code generation
// since Go allows variables to escape the current stack automatically, adding them to
// the heap, behind the scenes. C# does not have this feature, so we need to manually
// determine if a variable needs to be heap allocated. The map that is created as a
// result of this analysis is called `identEscapesHeap`.

// The analysis works per function, marking variables that "may" escape, and it feeds two
// cooperating mechanisms rather than a single heap decision. Where every consumer of an
// address permits it, Phase-A REF-LOWERING keeps the variable on the stack and passes it
// as a C# `ref` — the "ref structure operations" an earlier version of this comment
// predicted as future work — with the A2 reversion census walking candidates back to
// stack form when a lowering proves total (see docs/phase4/EXEMPLARS-a2-ref-lowering.md).
// Everything else takes the ж<T> identity box, whose invariant is that an OBSERVED
// address always aliases its storage: capture-mode method calls on value-field chains and
// type-switch bindings (which live in Implicits) are marked here precisely so writes
// through their addresses land in the real storage, never a copy.

// Headroom that remains genuinely future: Phase A never lowers a METHOD's pointer
// parameters (the §10.1 boundary in the ref-lowering doc), and no cross-function
// lookahead exists yet — the same lowering logic could extend to same-package,
// private-scope callees whose parameter uses would tolerate `ref`, widening the
// stack-kept set beyond what a single function's view can prove.

func performEscapeAnalysis(files []FileEntry, fset *token.FileSet, pkg *types.Package, info *types.Info) {
	// The //go:cgo_unsafe_args block lift is decided once per package, ahead of every per-file
	// visitor below: its consumed `&first` must not heap-box the parameter (objectAddressTaken
	// consults the mark), and the emission visitors read the same verdict. See cgoUnsafeArgsLift.go.
	collectCgoUnsafeArgsLifts(files, info)

	var concurrentTasks sync.WaitGroup

	// A panic raised on THIS side of the `go` statement below cannot be recovered by the caller:
	// Go unwinds only the panicking goroutine's own stack, so it takes down the whole process. Both
	// batch drivers already wrap each package in a recover precisely so ONE unconvertible package
	// fails alone (ModuleConverter.convertAll records it in `failed` and converts the rest;
	// StdLibConverter.convertPackage turns it into an error) — and this pass silently defeated both
	// of them. Issue #33 is exactly that: a -recurse run over 1,726 packages died at [736/1726],
	// discarding ~1,000 packages of queued work over a single-package fault. So each worker
	// captures its own panic and the first one is re-raised on the caller's goroutine, where the
	// per-package recovers can see it.
	var (
		firstPanic     any
		firstStack     []byte
		firstPanicOnce sync.Once
	)

	for _, fileEntry := range files {
		concurrentTasks.Add(1)

		go func(fileEntry FileEntry) {
			defer concurrentTasks.Done()

			defer func() {
				if r := recover(); r != nil {
					// Captured before unwinding leaves this goroutine, so the recorded stack is
					// still the FAULTING one — re-panicking on the caller would otherwise report
					// only the re-raise site, which names no converter code at all.
					stack := debug.Stack()

					firstPanicOnce.Do(func() {
						firstPanic, firstStack = r, stack
					})
				}
			}()

			visitor := &Visitor{
				fset:             fset,
				pkg:              pkg,
				info:             info,
				identEscapesHeap: fileEntry.identEscapesHeap,
				sstringEligible:  fileEntry.sstringEligible,
				ssliceEligible:   fileEntry.ssliceEligible,
				sstringConvExprs: fileEntry.sstringConvExprs,
			}

			// Unnamed `string(x)` temporaries consumed within a comparison (`string(buf) == "…"`) or a
			// concatenation (`string(buf) + suffix`) never outlive the expression, so they are safe to
			// emit as a zero-copy sstring view when the other operand cannot mutate the source first.
			visitor.markSStringBinaryOperandConversions(fileEntry.file)

			// `switch string(x) { case …: … }` lowers to a temp compared against each case with `==`
			// (never a C# switch/pattern — string labels are not C# case constants); the tag is likewise
			// a zero-copy sstring view when every case label is a mutation-safe comparison operand.
			visitor.markSStringSwitchConversions(fileEntry.file)

			ast.Inspect(fileEntry.file, func(n ast.Node) bool {
				switch node := n.(type) {
				case *ast.FuncDecl:
					if node.Body == nil {
						return true
					}

					visitor.markCaptureModeBoxedParams(node.Type.Params, node.Body)
					visitor.markVariadicSSliceEligible(node.Type.Params, node.Body)

					// A method's VALUE RECEIVER whose own address is taken (`&r`, `&r.field…`,
					// `&r[i]`) is the THIRD signature-declared category — declared on Recv, not
					// by any body statement, so neither the define-walk below nor the parameter
					// scan above ever reaches it. Same defect, same remedy as the parameter arm.
					visitor.markAddressTakenBoxedReceiver(node.Recv, node.Body)

					visitor.analyzeBodyDeclaredVars(node.Body)

					// A named RESULT whose address is taken (`&err` / passed as `*T` /
					// `&result.field`) — e.g. tabwriter's `defer b.handlePanic(&err, …)` in
					// flush/Write — must be heap-boxed at entry, exactly like an escaping
					// address-taken local, so the box `Ꮡerr`, the deferred handler's write
					// through the pointer, and the final `return err` all reference the SAME
					// storage (Go promotes such a result to the heap). A named result is
					// declared in the signature, not by any body statement, so the define-walk
					// above never feeds it to the escape analysis — it reached one only when it
					// also happened to sit on a `:=` LHS. Analyze them explicitly against the
					// same body here.
					visitor.analyzeNamedResults(node.Type.Results, node.Body)

				case *ast.FuncLit:
					// A literal OUTSIDE any function declaration (a package-level var
					// initializer). Literals inside a FuncDecl were already marked above —
					// the already-analyzed guard makes this re-visit a no-op for them.
					visitor.markCaptureModeBoxedParams(node.Type.Params, node.Body)
					visitor.markVariadicSSliceEligible(node.Type.Params, node.Body)

					// The literal's own LOCALS need the same define-walk the FuncDecl arm runs.
					// Only a FuncDecl body reached it before, so a literal that is not inside one
					// — every `var tests = []struct{…; f func()}{{…, func(){ … }}}` table, sync
					// mutex_test's misuseTests being the worked case — had NO local analyzed at
					// all: `var mu sync.Mutex; mu.Unlock()` stayed unboxed and emitted the VALUE
					// receiver form, which binds no ж<Mutex> extension (CS1929 ×16). Literals
					// nested inside a FuncDecl were already walked against the enclosing body (a
					// superset), and performEscapeAnalysis short-circuits on an already-analyzed
					// object, so this re-visit is a no-op for them.
					visitor.analyzeBodyDeclaredVars(node.Body)

					// A function literal's named results take the same address-taken escape
					// treatment as a declaration's (see analyzeNamedResults). Nested literals
					// hit this case too — the already-analyzed guard makes any overlap a no-op.
					visitor.analyzeNamedResults(node.Type.Results, node.Body)
				}
				return true
			})
		}(fileEntry)
	}

	concurrentTasks.Wait()

	if firstPanic != nil {
		panic(fmt.Sprintf("escape analysis: %v\n\n%s", firstPanic, firstStack))
	}
}

// analyzeBodyDeclaredVars runs escape analysis over every variable DECLARED inside body — `:=`
// defines, range/for/if/switch/type-switch init defines, and `var` declarations — plus the value
// parameters of any function literal nested within it. Shared by the FuncDecl and the standalone
// FuncLit arms of the file walk so a literal that is not inside a declaration (a package-level var
// initializer) gets identical treatment; the analysis is idempotent per object, so the overlap
// between the two arms is a no-op.
//
// Nested-literal value params are marked FIRST, matching the order the declaration's own params
// take: a mixed `t, y := …` re-use of a literal param would otherwise record the define-walk's
// escape verdict first, and markCaptureModeBoxedParams skips already-analyzed objects.
func (v *Visitor) analyzeBodyDeclaredVars(body *ast.BlockStmt) {
	if body == nil {
		return
	}

	ast.Inspect(body, func(n ast.Node) bool {
		if funcLit, ok := n.(*ast.FuncLit); ok {
			v.markCaptureModeBoxedParams(funcLit.Type.Params, funcLit.Body)
		}
		return true
	})

	ast.Inspect(body, func(n ast.Node) bool {
		switch n := n.(type) {
		case *ast.AssignStmt:
			if n.Tok == token.DEFINE {
				for _, lhs := range n.Lhs {
					if ident := getIdentifier(lhs); ident != nil {
						v.performEscapeAnalysis(ident, body)
					}
				}

				// A single-value `s := string(x)` may be emittable as a stack-only
				// sstring; decide now that identEscapesHeap is populated for the LHS.
				if len(n.Lhs) == 1 && len(n.Rhs) == 1 {
					if ident := getIdentifier(n.Lhs[0]); ident != nil {
						v.markSStringEligible(ident, n.Rhs[0], body)
					}
				}
			}
		case *ast.RangeStmt:
			if n.Tok == token.DEFINE {
				if key := getIdentifier(n.Key); key != nil {
					v.performEscapeAnalysis(key, body)
				}
				if value := getIdentifier(n.Value); value != nil {
					v.performEscapeAnalysis(value, body)
				}
			}
		case *ast.DeclStmt:
			if genDecl, ok := n.Decl.(*ast.GenDecl); ok {
				for _, spec := range genDecl.Specs {
					if valueSpec, ok := spec.(*ast.ValueSpec); ok {
						for _, ident := range valueSpec.Names {
							if !isDiscardedVar(ident.Name) {
								v.performEscapeAnalysis(ident, body)
							}
						}
					}
				}
			}
		case *ast.ForStmt:
			if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
				for _, lhs := range init.Lhs {
					if ident := getIdentifier(lhs); ident != nil {
						v.performEscapeAnalysis(ident, body)
					}
				}
			}
		case *ast.IfStmt:
			if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
				for _, lhs := range init.Lhs {
					if ident := getIdentifier(lhs); ident != nil {
						v.performEscapeAnalysis(ident, body)
					}
				}
			}
		case *ast.SwitchStmt:
			if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
				for _, lhs := range init.Lhs {
					if ident := getIdentifier(lhs); ident != nil {
						v.performEscapeAnalysis(ident, body)
					}
				}
			}
		case *ast.TypeSwitchStmt:
			if assign, ok := n.Assign.(*ast.AssignStmt); ok && assign.Tok == token.DEFINE {
				for _, lhs := range assign.Lhs {
					if ident := getIdentifier(lhs); ident != nil {
						v.performEscapeAnalysis(ident, body)
					}
				}

				// The guard ident above resolves to NO object (go/types: "symbolic variables t in
				// t := x.(type) … the corresponding objects are nil"), so the call is a no-op. The
				// real bindings are one implicit *types.Var PER CASE CLAUSE, in info.Implicits —
				// analyze each so an address-taken binding is heap-boxed like any other local
				// (`&t1.Name` handed to a ж<Name> parameter wrote into a `Ꮡ(t1)` COPY box, the
				// encoding/xml Token() lost write). Narrowed to non-inherently-heap types: a
				// binding bound at an interface/slice/map type is already a reference and today's
				// no-entry state is load-bearing for it (identEscapesHeap is read raw by the
				// capture analysis), while the write-loss class is exactly the value-typed
				// bindings. The same category joins the ref-lowering locals census
				// (censusFuncLocals), so a binding whose every address-connected use feeds a
				// lowered position still reverts to a plain stack local.
				for _, stmt := range n.Body.List {
					if caseClause, ok := stmt.(*ast.CaseClause); ok {
						if implicitVar, ok := v.info.Implicits[caseClause].(*types.Var); ok &&
							!isInherentlyHeapAllocatedType(implicitVar.Type()) {
							v.performEscapeAnalysisForObject(implicitVar, body)
						}
					}
				}
			}
		}
		return true
	})
}

// markVariadicSSliceEligible records whether the final variadic parameter may bind directly to its
// incoming params Span<T> through a stack-only sslice<T>. The proof is deliberately narrow: every
// use must consume only the slice header within this frame (len/cap, direct range, or element access).
// Anything that could retain, capture, reassign, grow, box, return, or pass the slice falls back to
// the existing heap slice<T> prologue.
func (v *Visitor) markVariadicSSliceEligible(params *ast.FieldList, body *ast.BlockStmt) {
	if params == nil || body == nil || len(params.List) == 0 {
		return
	}

	field := params.List[len(params.List)-1]

	if _, ok := field.Type.(*ast.Ellipsis); !ok {
		return
	}

	for _, ident := range field.Names {
		if isDiscardedVar(ident.Name) {
			continue
		}

		obj := v.info.ObjectOf(ident)

		if obj == nil || v.identEscapesHeap[obj] || !v.ssliceUsesAreSafe(obj, body) {
			continue
		}

		v.ssliceEligible[obj] = true

		if os.Getenv("GO2CS_DEBUG_SSLICE") != "" {
			pos := v.fset.Position(ident.Pos())
			fmt.Fprintf(os.Stderr, "[sslice] eligible variadic: %s at %s:%d:%d\n", ident.Name, pos.Filename, pos.Line, pos.Column)
		}
	}
}

// ssliceUsesAreSafe reports whether every occurrence of obj is a non-escaping operation supported
// directly by sslice<T>: len/cap, the base of an element index, or the expression ranged over.
// A use inside a nested function literal rejects the candidate because it would capture a ref struct.
// Taking an indexed element's address is also rejected: the converter's element-address helpers are
// heap-slice based, and the resulting pointer may outlive the stack view.
func (v *Visitor) ssliceUsesAreSafe(obj types.Object, body *ast.BlockStmt) bool {
	safeIdents := map[*ast.Ident]bool{}
	var stack []ast.Node

	ast.Inspect(body, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}

		inNestedFunc := stackHasFuncLit(stack)

		switch e := n.(type) {
		case *ast.CallExpr:
			if !inNestedFunc {
				if fn, ok := e.Fun.(*ast.Ident); ok && (fn.Name == "len" || fn.Name == "cap") {
					if builtin, ok := v.info.ObjectOf(fn).(*types.Builtin); ok &&
						(builtin.Name() == "len" || builtin.Name() == "cap") {
						for _, arg := range e.Args {
							if id, ok := arg.(*ast.Ident); ok && v.info.ObjectOf(id) == obj {
								safeIdents[id] = true
							}
						}
					}
				}
			}

		case *ast.IndexExpr:
			if !inNestedFunc {
				if id, ok := e.X.(*ast.Ident); ok && v.info.ObjectOf(id) == obj {
					addressTaken := false

					if len(stack) > 0 {
						if unary, ok := stack[len(stack)-1].(*ast.UnaryExpr); ok && unary.Op == token.AND && unary.X == e {
							addressTaken = true
						}
					}

					if !addressTaken {
						safeIdents[id] = true
					}
				}
			}

		case *ast.RangeStmt:
			if !inNestedFunc {
				if id, ok := e.X.(*ast.Ident); ok && v.info.ObjectOf(id) == obj {
					safeIdents[id] = true
				}
			}
		}

		stack = append(stack, n)
		return true
	})

	allSafe := true

	ast.Inspect(body, func(n ast.Node) bool {
		if !allSafe {
			return false
		}

		if id, ok := n.(*ast.Ident); ok && v.info.ObjectOf(id) == obj && !safeIdents[id] {
			allSafe = false
		}

		return true
	})

	return allSafe
}

// markCaptureModeBoxedParams marks the function's VALUE parameters that need an entry-time heap
// box. Two triggers:
//
//   - The body calls a capture-mode (direct-ж) method on the parameter — go/format's
//     `format(…, cfg printer.Config)` calling `cfg.Fprint(…)`, where (*Config).Fprint is emitted
//     with only the `this ж<Config>` receiver (CS1929 on the raw value).
//   - The parameter's own storage has its ADDRESS TAKEN (`&r`, `&r.field…`, `&r[i]` — see
//     paramAddressTakenNeedsBox), the form that hands other code a pointer INTO it. Without a
//     box this emits the call-site `Ꮡ(r)` copy-box, which compiles but silently drops every write
//     the callee makes through the pointer: image/draw's `DrawMask(…, r image.Rectangle, …)` calls
//     `clip(dst, &r, …)`, so the clipped rectangle never reaches the drawing loop. Go promotes such
//     a parameter to the heap; this is the parameter-side analogue of analyzeNamedResults, which
//     closed the identical gap for the other signature-declared category.
//
// Parameters are otherwise deliberately NOT fed through the full escape analysis, so this is the
// primary writer of a parameter into identEscapesHeap — visitFuncDecl reads such an entry as the
// entry-time-box trigger (see paramNeedsHeapBox): the incoming value arrives under the `ʗp` name
// and the parameter preamble declares `ref var cfg = ref heap(cfgʗp, out var Ꮡcfg);`, so body uses
// hit the boxed alias, `&cfg` renders as the identity box `Ꮡcfg`, and convSelectorExpr routes
// capture-mode calls through it. Entry-time boxing (never a call-site copy-box) preserves Go's
// by-value parameter + auto-address semantics exactly. Serves both function DECLARATIONS and
// function LITERALS (whose prologue/rename convFuncLit emits — see funcLitHeapBoxParamIdents).
//
// The trigger set here and paramBoxReasonHolds's (the emission-side re-verification) must stay
// IDENTICAL: a reason recorded by analysis but missing there leaves body uses referencing a box
// that was never declared (CS0103), and the reverse declares a box nothing references.
func (v *Visitor) markCaptureModeBoxedParams(params *ast.FieldList, body *ast.BlockStmt) {
	if params == nil {
		return
	}

	for _, field := range params.List {
		// A variadic parameter already re-declares its Go name in the prologue
		// (`var xs = xsʗp.slice();`), and its unnamed []T type carries no methods.
		if _, isVariadic := field.Type.(*ast.Ellipsis); isVariadic {
			continue
		}

		for _, ident := range field.Names {
			if isDiscardedVar(ident.Name) {
				continue
			}

			obj := v.info.ObjectOf(ident)

			if obj == nil {
				continue
			}

			// A pointer parameter already carries its box (`Ꮡp` IS the emitted parameter).
			if _, isPointer := obj.Type().(*types.Pointer); isPointer {
				continue
			}

			if _, found := v.identEscapesHeap[obj]; found {
				continue
			}

			// A METHOD VALUE of a pointer-receiver method (`cfg.Fprint` handed to a func
			// parameter, never called here) implicitly takes `&cfg` exactly like the call
			// form does — see pointerMethodValueAddressTaken.
			captureMode := v.bodyCallsCaptureModeMethodOn(ident, body) || v.pointerMethodValueAddressTaken(obj, body)

			// ж-box A2 (§3.3's reversion): a value parameter address-taken ONLY into ref-lowered
			// positions stays a plain parameter — `ref x` at those sites aliases its own storage.
			// A reverting verdict is mutually exclusive with captureMode, and since 2026-08-26
			// that holds for a stated reason rather than by breadth: captureMode is exactly
			// `bodyCallsCaptureModeMethodOn || pointerMethodValueAddressTaken`, and B′ §4.2's
			// receiverUseKeptReason records a kept-reason for precisely those two shapes (the
			// direct-ж callee and the method value — collectCaptureModeMethods writes both
			// method sets under one condition, so they never diverge). What it no longer keeps
			// is the ordinary call on a `[GoRecv] ref` method, which consumes no box.
			if refLoweringLocalReverts(obj) {
				continue
			}

			if captureMode || v.paramAddressTakenNeedsBox(obj, body) {
				v.identEscapesHeap[obj] = true

				// An inherently-heap value (named slice/map/chan) is already a reference, so
				// identHasHeapBox boxes it only for a recorded capture-mode reason — same rule
				// as the local-var arm in performEscapeAnalysis. The ADDRESS-TAKEN reason
				// deliberately records none: identHasHeapBox's own `identAddressTaken` gate
				// already answers that case, and answers it correctly (see
				// paramAddressTakenNeedsBox).
				if captureMode && packageCaptureModeBoxIdents != nil && isInherentlyHeapAllocatedType(obj.Type()) {
					packageCaptureModeBoxIdents[obj] = true
				}
			}
		}
	}
}

// analyzeNamedResults heap-marks a function's (or literal's) named result whose address is taken
// in its body. A named result is declared in the signature, not by any body statement, so the
// body's define-walk never reaches it unless it also happens to sit on a `:=` LHS; without this an
// address-taken named result (`&err` passed to a deferred handler — tabwriter's
// `defer b.handlePanic(&err, …)`) is left unboxed and `Ꮡ(err)` boxes a COPY, silently dropping
// writes the handler makes through the pointer (Go promotes such a result to the heap).
//
// The trigger is ADDRESS-TAKEN specifically — the one condition that hands other code a pointer
// into the result's own storage, requiring the box `Ꮡerr`, the pointee write, and `return err` to
// share one slot (mirrors identHasHeapBox's box gate for an inherently-heap ident). A result that
// is merely referenced or WRITTEN inside a closure does NOT qualify: a C# closure already captures
// the outer local by reference, so the full escape analysis (which blanket-marks any inherently-
// heap ident referenced in a closure) would needlessly flip such a result to box-ref emission. The
// verdict is only ever SET true here — a non-address-taken result stays absent from the map, so
// emission for every result that does not take its own address is byte-unchanged.
func (v *Visitor) analyzeNamedResults(results *ast.FieldList, body *ast.BlockStmt) {
	if results == nil || body == nil {
		return
	}

	for _, field := range results.List {
		for _, name := range field.Names {
			if isDiscardedVar(name.Name) {
				continue
			}

			obj := v.info.ObjectOf(name)

			if obj == nil {
				continue
			}

			// Already analyzed (it also sits on a `:=` LHS) — keep the existing verdict.
			if _, found := v.identEscapesHeap[obj]; found {
				continue
			}

			// ж-box A2 (§3.3's reversion): a named result address-taken ONLY into ref-lowered
			// positions stays a plain result local — same refinement as the parameter arm.
			if refLoweringLocalReverts(obj) {
				continue
			}

			if v.objectAddressTaken(obj, body, false) || v.pointerMethodValueAddressTaken(obj, body) {
				v.identEscapesHeap[obj] = true
			}
		}
	}
}

// markAddressTakenBoxedReceiver marks a method's VALUE RECEIVER whose own address is taken in the
// body — `&r`, `&r.field…` or `&r[i]` — so the receiver is heap-boxed AT ENTRY. This is the third
// and last SIGNATURE-declared category, after named results (analyzeNamedResults) and value
// parameters (the address-taken arm of markCaptureModeBoxedParams): the receiver is declared on
// `funcDecl.Recv`, not by any body statement, so the define-walk never reaches it and the parameter
// scan never sees it (it walks `funcType.Params`). Without a box the address-of falls back to the
// call-site `Ꮡ(r)` COPY box, which compiles and silently drops every write the callee makes through
// the pointer — `func (b Box) Bumped() int { bumpBox(&b); return b.N }` returns the UNBUMPED value —
// and an ARRAY receiver's `&a[i]` is worse than silent: the emission already spells the identity box
// `Ꮡa` (the array-parameter copy-box fallback in convUnaryExpr is keyed on identIsParameter, which
// deliberately excludes the receiver), naming a box that was never declared (CS0103).
//
// The trigger is the SAME restricted predicate the parameter arm uses (paramAddressTakenNeedsBox),
// so an inherently-heap receiver — a named slice/map/chan/func/interface type — boxes only for the
// bare `&r`: `&r[i]` on a slice receiver addresses the shared backing array, which the emitted
// `Ꮡ(r, i)` element form already aliases. Emission re-verifies through recvBoxReasonHolds; the two
// must stay identical for the same reason the parameter pair must (CS0103 one way, a box nothing
// references the other).
//
// Deliberately NARROWER than the parameter trigger: a capture-mode (direct-ж) method called on a
// receiver is already served by packageDirectBoxReceiverMethods, and a receiver the capture analysis
// routed to box-ref storage must NOT take the `ʗp` form (see paramNeedsHeapBox).
func (v *Visitor) markAddressTakenBoxedReceiver(recv *ast.FieldList, body *ast.BlockStmt) {
	if recv == nil || body == nil {
		return
	}

	for _, field := range recv.List {
		for _, ident := range field.Names {
			if isDiscardedVar(ident.Name) {
				continue
			}

			obj := v.info.ObjectOf(ident)

			if obj == nil {
				continue
			}

			// A pointer receiver already carries its box (`Ꮡr` IS the emitted receiver).
			if _, isPointer := obj.Type().(*types.Pointer); isPointer {
				continue
			}

			if _, found := v.identEscapesHeap[obj]; found {
				continue
			}

			if v.paramAddressTakenNeedsBox(obj, body) {
				v.identEscapesHeap[obj] = true
			}
		}
	}
}

// paramAddressTakenNeedsBox reports whether the VALUE PARAMETER obj has its address taken in body
// in a form that requires obj's OWN storage to be heap-boxed at entry. A method's value RECEIVER is
// parameter 0 in this model — the converter itself concatenates it as such (getParameters) — and
// takes the identical rule (see markAddressTakenBoxedReceiver).
//
// For an ordinary value type (struct, array, basic, named struct) every address form qualifies:
// `&p`, `&p.field…` and `&p[i]` all hand out a pointer INTO the parameter's storage, so the box,
// the callee's write through the pointer, and every later body read must share one slot.
//
// An INHERENTLY-HEAP type (slice/map/chan/interface/func — and a type parameter, whose underlying
// is its constraint interface) is already a reference: only the address of the reference VARIABLE
// ITSELF (`&p`) needs a box. `&p[i]` addresses the SHARED BACKING ARRAY, which aliases correctly
// with no box — Go likewise does not heap-promote a slice header for an element address, and the
// emitted element form (`Ꮡ(p, i)`) already shares storage. Mirroring identHasHeapBox's own box gate
// here keeps the analysis verdict, that gate, and every emitter in agreement (a verdict the gate
// then refuses would leave `identEscapesHeap` set with no box — read raw by the capture analysis and
// several emitters), and keeps hot slice-parameter helpers allocation-free: without the restriction
// unicode's `is16`/`is32`, `slices.Equal`, `subtle.XORBytes` and 40-odd other `&s[i]` sites boxed
// their slice header on every call for no semantic gain.
func (v *Visitor) paramAddressTakenNeedsBox(obj types.Object, body ast.Node) bool {
	if obj == nil {
		return false
	}

	return v.objectAddressTaken(obj, body, isInherentlyHeapAllocatedType(obj.Type()))
}

// objectAddressTaken reports whether the storage of obj has its address taken anywhere in body
// (including nested function literals): `&obj`, `&obj.field…` (a value-field selector chain rooted
// at obj), or `&obj[i]`. When directOnly is set, only the bare `&obj` form counts — the address of
// obj's own reference variable — which is what an inherently-heap parameter needs (see
// paramAddressTakenNeedsBox). Only such a form hands other code a pointer INTO obj's own storage —
// the case that needs obj heap-boxed, so the box, the pointee write, and every later read share one
// slot. This is the analogue of performEscapeAnalysis's UnaryExpr address-of arm for the two
// SIGNATURE-declared categories the body's define-walk never reaches: named results
// (analyzeNamedResults) and value parameters (paramAddressTakenNeedsBox).
//
// This is the single implementation of the walk. identAddressTaken is its memoized
// current-function, directOnly specialization, used at emission time rather than during analysis.
func (v *Visitor) objectAddressTaken(obj types.Object, body ast.Node, directOnly bool) bool {
	if obj == nil || body == nil {
		return false
	}

	found := false

	ast.Inspect(body, func(n ast.Node) bool {
		if found {
			return false
		}

		unary, ok := n.(*ast.UnaryExpr)

		if !ok || unary.Op != token.AND {
			return true
		}

		// The `&first` a //go:cgo_unsafe_args block lift CONSUMES (cgoUnsafeArgsLift.go): the lift
		// reads the parameter's VALUE into its synthesized block, so this address-of hands nobody a
		// pointer into the parameter's storage and must not box it.
		if packageLiftConsumedAddressOf[unary] {
			return true
		}

		switch x := unary.X.(type) {
		case *ast.Ident:
			if v.info.ObjectOf(x) == obj {
				found = true
			}
		case *ast.IndexExpr:
			if id, ok := x.X.(*ast.Ident); !directOnly && ok && v.info.ObjectOf(id) == obj {
				found = true
			}
		case *ast.SelectorExpr:
			if !directOnly && selectorChainRootsAtIdent(x, obj, v.info) {
				found = true
			}
		}

		return !found
	})

	return found
}

// refLoweringLocalReverts reports whether obj is an address-taken local/value-parameter/named-
// result the ж-box classification (stage A2, DESIGN-zh-box-reduction §3.3) proved reverts to a
// plain stack local: its EVERY address-connected use feeds a Phase-A ref-lowered position —
// directly, outside defer/go, outside any nested closure — so no heap box (and no eager pinnable
// slot) is needed; the lowered call sites take `ref obj` into the plain local's own storage.
// The classification pass runs BEFORE escape analysis in every conversion driver, so the verdict
// is always resolved by the time this is consulted. Deliberately consulted only for types that
// are NOT inherently heap-allocated: an inherently-heap ident's identEscapesHeap verdict serves
// purposes beyond boxing, so those keep today's behavior (allocation-neutral, conservative).
func refLoweringLocalReverts(obj types.Object) bool {
	result := packageRefLoweringResult

	if result == nil || result.RevertedLocalVars == nil {
		return false
	}

	objVar, ok := obj.(*types.Var)

	return ok && result.RevertedLocalVars[objVar]
}

// Perform escape analysis on the given identifier within the specified block
func (v *Visitor) performEscapeAnalysis(ident *ast.Ident, parentBlock *ast.BlockStmt) {
	identObj := v.info.ObjectOf(ident)

	if identObj == nil {
		return // Could not find the object of ident
	}

	v.performEscapeAnalysisForObject(identObj, parentBlock)
}

// performEscapeAnalysisForObject is performEscapeAnalysis keyed by the resolved object — the
// form a type-switch case binding needs: its defining ident is the shared guard (`t1 := x.(type)`),
// whose ObjectOf is nil, while the real per-case *types.Var lives in info.Implicits keyed by the
// case clause. Body uses resolve to that object through info.Uses, so every arm of the walk below
// matches by object identity exactly as it does for a Defs-declared local.
func (v *Visitor) performEscapeAnalysisForObject(identObj types.Object, parentBlock *ast.BlockStmt) {
	if parentBlock == nil || identObj == nil {
		return
	}

	// If analysis has already been performed, return
	if _, found := v.identEscapesHeap[identObj]; found {
		return
	}

	// Check if the type is inherently heap allocated
	if isInherentlyHeapAllocatedType(identObj.Type()) {
		v.identEscapesHeap[identObj] = true

		// An inherently-heap value var is already a reference, so identHasHeapBox does NOT box it
		// by default. But a capture-mode pointer-receiver method called on it (`frontier.Push(…)`
		// with Push/Pop on `*orderEventList`, a NAMED SLICE) needs the ж overload's receiver box
		// (CS1929 without it). Record that reason so identHasHeapBox forces the box for exactly
		// these vars (a non-inherently-heap struct like atomic.Int32 is already boxed below).
		// A pointer-receiver METHOD VALUE (`l.push` passed as a func arg) needs the same box:
		// `Ꮡ(l)` copy-boxes the slice HEADER, so every `*l = append(*l, …)` the method value
		// performs lands in the copy and the caller's `l` never grows.
		if packageCaptureModeBoxIdents != nil &&
			(v.bodyCallsCaptureModeMethodOnObject(identObj, parentBlock) || v.pointerMethodValueAddressTaken(identObj, parentBlock)) {
			packageCaptureModeBoxIdents[identObj] = true
		}

		return
	}

	// ж-box A2 (§3.3's reversion): address-taken ONLY into Phase-A ref-lowered positions →
	// stack. The classification already proved no other box-forcing use exists (no closure
	// crossing, no defer/go feed, no BOX-CONSUMING receiver use, no escape), so the address-of
	// walk below could only re-derive the box this refinement exists to remove.
	//
	// "Box-consuming receiver use" is B′ §4.2's call-site selection, not the blanket
	// "any pointer-receiver call" this said until 2026-08-26: a `[GoRecv] this ref T` method
	// binds the local's own storage, so an ordinary call on one consumes no box and must not
	// hold the local off the stack (receiverUseKeptReason). The three shapes that DO consume it
	// — a direct-ж callee, a method value, a defer/go receiver — still record a kept-reason, so
	// this arm remains unreachable for them.
	if refLoweringLocalReverts(identObj) {
		v.identEscapesHeap[identObj] = false
		return
	}

	escapes := false

	// Visitor function to traverse the AST
	inspectFunc := func(node ast.Node) bool {
		if escapes {
			return false // Stop traversal if escape is found
		}

		switch n := node.(type) {
		case *ast.UnaryExpr:
			// Check if ident is used in an address-of operation
			if n.Op == token.AND {
				// Direct address of identifier
				if id, ok := n.X.(*ast.Ident); ok {
					if obj := v.info.ObjectOf(id); obj == identObj {
						escapes = true
						return false
					}
				}

				// Address of array/slice element
				if indexExpr, ok := n.X.(*ast.IndexExpr); ok {
					if id, ok := indexExpr.X.(*ast.Ident); ok {
						if obj := v.info.ObjectOf(id); obj == identObj {
							escapes = true
							return false
						}
					}
				}

				// Address of a struct-field chain rooted at the identifier: `&x.field`,
				// `&x.a.b`. Only the CallExpr arm below peeled selector roots — and only for
				// pointer ARGUMENTS — so an assignment/return/composite-position field address
				// left the local unboxed and the emitted `Ꮡ(x).of(T.Ꮡval)` boxed a COPY,
				// silently dropping writes made through the pointer (Go reads the write back
				// through `x`; C# did not).
				if selectorChainRootsAtIdent(n.X, identObj, v.info) {
					escapes = true
					return false
				}

				// For composite literals, special case:
				// If the variable is used only as part of calculating a value for a field,
				// it doesn't escape to the heap
				if compLit, ok := n.X.(*ast.CompositeLit); ok {
					// First check if our identifier is used in the type part
					// (like array size) - if so, it doesn't escape
					if containsIdentInTypeExpr(compLit.Type, identObj, v.info) {
						return true
					}

					// Now check if our identifier is directly stored in a field
					// or just used in a calculation
					for _, elt := range compLit.Elts {
						// Check value expressions - by default assume safe unless
						// we find a direct assignment of our identifier
						if kv, ok := elt.(*ast.KeyValueExpr); ok {
							// Key doesn't matter, only value
							if id, ok := kv.Value.(*ast.Ident); ok {
								if obj := v.info.ObjectOf(id); obj == identObj {
									// Direct assignment of our identifier to a field
									// This is a gray area - in many cases it's safe,
									// but for simplicity assume escape
									escapes = true
									return false
								}
							}

							// If our identifier is used in a calculation but not
							// directly stored, it doesn't escape
							// e.g., &Struct{field: n+1} doesn't make n escape
							if containsIdentInValueCalc(kv.Value, identObj, v.info) {
								// Don't mark as escaping - continue checking other elements
								continue
							}
						} else if id, ok := elt.(*ast.Ident); ok {
							// Direct use of identifier as element
							if obj := v.info.ObjectOf(id); obj == identObj {
								escapes = true
								return false
							}
						}
					}

					// If we get here, the identifier is only used in calculations
					// for field values, not directly stored in the composite literal
					return true
				}
			}

		case *ast.CallExpr:
			// Check if ident's STORAGE is passed as a pointer argument. Only a literal
			// address-of whose peeled ROOT is the ident (`&i`, `&i.field`, `&i[k]`) — or the
			// bare ident itself — hands the callee a pointer into the ident's storage. An
			// ident that merely appears in a subexpression of a pointer arg computes a VALUE:
			// in `xs[i].link(&xs[i+1])` or `typesEqual(tin[i], vin[i], seen)` the element/
			// slice storage escapes (the peeled root `xs`/`tin`), but the INDEX `i` does not —
			// the old contains-anywhere check heap-boxed every such loop index (a spurious
			// allocation, and duplicate hoisted boxes for sibling loops).
			for i, arg := range n.Args {
				// Skip this arg if it's a nested call expression
				if _, isNestedCall := arg.(*ast.CallExpr); isNestedCall {
					continue
				}

				if argRootIsIdent(arg, identObj, v.info) {
					// Get the function type
					funType := v.info.TypeOf(n.Fun)

					// An expression whose operand went INVALID is never recorded at all —
					// go/types' Checker.record returns early for `mode == invalid` — so TypeOf
					// hands back a nil interface and any method call on it is a hard crash. That
					// is not a hypothetical: a -recurse run converts whatever app and third-party
					// packages it is given, and some of those legitimately do not type-check on
					// the host (a symbol behind a build tag, a cgo-only file, a dependency whose
					// own import failed). The package loads with errors, the converter reports
					// them and converts best-effort — and hit this line (issue #33, on both a
					// third-party `could not import … (invalid package name: "")` and, under
					// -recurse=module, the app's own `undefined: …`). An untyped callee simply
					// tells us nothing about whether the argument escapes, so move on.
					if funType == nil {
						continue
					}

					sig, ok := funType.Underlying().(*types.Signature)

					if !ok {
						continue
					}

					var paramType types.Type

					if paramType, ok = getParameterType(sig, i); !ok {
						continue
					}

					// Check if paramType is a pointer type
					if _, ok := paramType.Underlying().(*types.Pointer); ok {
						// Passed as a pointer; may cause escape
						escapes = true
						return false
					}

					// We do not currently consider interface types as causing an escape since
					// in C# value types are boxed as needed making value basically read-only,
					// thus matching Go semantics
				}
			}

		case *ast.GoStmt:
			// Check if ident is used inside a goroutine
			goStmtContainsIdent := false
			takesAddress := false
			usedAsRef := false

			ast.Inspect(n.Call, func(n ast.Node) bool {
				if id := getIdentifier(n); id != nil {
					obj := v.info.ObjectOf(id)
					if obj == identObj {
						goStmtContainsIdent = true

						// Check if it's a value type
						if _, ok := obj.Type().Underlying().(*types.Basic); ok {
							// Value types only escape if their address is taken
							return true // continue checking for address operations
						}
						// Reference types still need to escape
						return false
					}
				}

				// Check for address-of operations
				if unary, ok := n.(*ast.UnaryExpr); ok && unary.Op == token.AND {
					if id := getIdentifier(unary.X); id != nil {
						if obj := v.info.ObjectOf(id); obj == identObj {
							takesAddress = true
							return false
						}
					}
				}

				return true
			})

			// Only escape if:
			// 1. It's a reference type used in goroutine
			// 2. It's a value type whose address is taken
			// 3. It's passed by reference somewhere
			if goStmtContainsIdent && (!isValueType(identObj.Type()) || takesAddress || usedAsRef) {
				escapes = true
				return false
			}

		case *ast.DeferStmt:
			// Check if ident is used inside a deferred function
			deferStmtContainsIdent := false
			takesAddress := false
			usedAsRef := false

			ast.Inspect(n.Call, func(n ast.Node) bool {
				if id := getIdentifier(n); id != nil {
					obj := v.info.ObjectOf(id)
					if obj == identObj {
						deferStmtContainsIdent = true

						// Check if it's a value type
						if _, ok := obj.Type().Underlying().(*types.Basic); ok {
							// Value types only escape if their address is taken
							return true // continue checking
						}
						return false
					}
				}

				// Check for address-of operations
				if unary, ok := n.(*ast.UnaryExpr); ok && unary.Op == token.AND {
					if id := getIdentifier(unary.X); id != nil {
						if obj := v.info.ObjectOf(id); obj == identObj {
							takesAddress = true
							return false
						}
					}
				}

				return true
			})

			// Only escape if necessary
			if deferStmtContainsIdent && (!isValueType(identObj.Type()) || takesAddress || usedAsRef) {
				escapes = true
				return false
			}

		case *ast.FuncLit:
			// A variable DECLARED INSIDE this literal is not CAPTURED by it — it is one of its
			// own locals, and Go scoping puts it out of reach of every frame but this one, so
			// there is nothing for a shared box to make visible. The walk below cannot tell the
			// two apart on its own: it matches any mention of the object lexically inside the
			// literal's body, which for an inside-declared variable is its own declaration. That
			// made `var t Time; t.UnmarshalText(in)` inside a closure heap-box `t` — 128 bytes
			// per call, and the box `Ꮡt` was never even referenced (`time`'s
			// TestUnmarshalTextAllocations asserts zero) — while the identical statements outside
			// one emitted a plain local. Compare it with the pointer-method-value note below: the
			// escape trigger is code OUTSIDE the frame reaching the storage, and no code outside
			// this literal can name this variable.
			//
			// Skip only THIS literal, and keep descending: a literal NESTED inside it does close
			// over the variable, and its own turn through this arm marks the escape correctly.
			// Everything that makes an inside-declared variable genuinely escape — `&x`, `&x.f`,
			// `&x[i]`, a pointer argument, a capture-mode method, a pointer-receiver method value,
			// a go/defer use — is decided by an arm that walks the WHOLE enclosing body, literal
			// bodies included, so none of them is lost here.
			if identObj.Pos() >= n.Pos() && identObj.Pos() < n.End() {
				return true
			}

			// Check if ident is used inside a closure
			closureContainsIdent := false
			takesAddress := false
			usedAsRef := false

			ast.Inspect(n.Body, func(n ast.Node) bool {
				if id := getIdentifier(n); id != nil {
					obj := v.info.ObjectOf(id)
					if obj == identObj {
						closureContainsIdent = true

						// Check if it's a value type
						if _, ok := obj.Type().Underlying().(*types.Basic); ok {
							// Value types only escape if their address is taken
							return true // continue checking for address operations
						}
						// Reference types still need to escape
						return false
					}
				}

				// Check for address-of operations
				if unary, ok := n.(*ast.UnaryExpr); ok && unary.Op == token.AND {
					if id := getIdentifier(unary.X); id != nil {
						if obj := v.info.ObjectOf(id); obj == identObj {
							takesAddress = true
							return false
						}
					}
				}

				return true
			})

			// Only escape if:
			// 1. It's a reference type used in closure
			// 2. It's a value type whose address is taken
			// 3. It's passed by reference somewhere
			escapes = (closureContainsIdent && !isValueType(identObj.Type())) ||
				takesAddress ||
				usedAsRef
		}

		return true // Continue traversing
	}

	ast.Inspect(parentBlock, inspectFunc)

	// A value var on which a capture-mode pointer-receiver method is called (e.g.
	// `var i atomic.Int32; i.Store(10)`, or `var frontier orderEventList; frontier.Push(…)`)
	// must be heap-boxed so the call can be routed through the ж overload — the only path that
	// sets up the receiver box the method needs for `&recv.field`. The receiver operand may be
	// the var itself or a value-field chain rooted at it (`x.i.Add(delta)` — see
	// bodyCallsCaptureModeMethodOnObject).
	if !escapes && v.bodyCallsCaptureModeMethodOnObject(identObj, parentBlock) {
		escapes = true
	}

	// A pointer-receiver METHOD VALUE taken on the var (`bufio`'s `s.Split(c.split)`) is Go
	// shorthand for `(&c).split` — the same implicit address-of the UnaryExpr arm catches for
	// an explicit `&c`, just written without the `&` (see pointerMethodValueAddressTaken).
	if !escapes && v.pointerMethodValueAddressTaken(identObj, parentBlock) {
		escapes = true
	}

	v.identEscapesHeap[identObj] = escapes
}

// pointerMethodValueAddressTaken reports whether body forms a METHOD VALUE — `x.M` evaluated as a
// func value rather than called — that selects a POINTER-receiver method on obj's own (non-pointer)
// storage, either directly (`c.split`) or through a value-field chain rooted at obj (`h.c.dec`).
//
// Go's spec makes such a method value shorthand for `(&x).M`: it binds a pointer INTO x's storage
// at evaluation time, so every write the method performs through its receiver must be visible in x
// afterwards. That is the identical escape condition as an explicit `&x`, written without the `&`,
// and it is the one address-of form the UnaryExpr / CallExpr arms above cannot see. Left unmarked,
// the local stays unpromoted and emission falls back to the copy-box `Ꮡ(c).split` — which compiles
// and runs, but mutates a COPY, silently dropping the method's writes (bufio's `s.Split(c.split)`
// scan-counter never decremented: "stopped with 10000 left to process"). Heap-promoting obj makes
// emission use the aliasing box `Ꮡc.split` instead, matching Go exactly.
//
// A method value is only recognized OUTSIDE call position: `c.dec()` is a call, not a value, and
// binds C#'s `this ref counter c` extension receiver directly against the variable — already
// correct, and promoting for it would heap-box every local that calls a pointer-receiver method.
// A pointer-typed base (`p.M` where p is `*counter`) is excluded too: it passes the pointer VALUE,
// taking no address of p. A VALUE-receiver method value (`c.get`) is likewise excluded — Go copies
// the receiver at evaluation time there, which is what the current copy emission already does.
func (v *Visitor) pointerMethodValueAddressTaken(obj types.Object, body ast.Node) bool {
	if obj == nil || body == nil {
		return false
	}

	var candidates []*ast.SelectorExpr
	callFuns := make(map[ast.Expr]bool)

	ast.Inspect(body, func(n ast.Node) bool {
		switch node := n.(type) {
		case *ast.CallExpr:
			callFuns[ast.Unparen(node.Fun)] = true
		case *ast.SelectorExpr:
			if v.selectsPointerMethodOn(node, obj) {
				candidates = append(candidates, node)
			}
		}

		return true
	})

	for _, candidate := range candidates {
		if !callFuns[candidate] {
			return true
		}
	}

	return false
}

// selectsPointerMethodOn reports whether sel selects a pointer-receiver method whose receiver
// operand is obj's own storage — the bare ident, or a non-indirect value-field chain rooted at it
// (selectorChainRootsAtIdent, the same root walk the explicit-`&` arm uses). Call position is NOT
// considered here; pointerMethodValueAddressTaken filters that.
func (v *Visitor) selectsPointerMethodOn(sel *ast.SelectorExpr, obj types.Object) bool {
	base := ast.Unparen(sel.X)

	if ident, ok := base.(*ast.Ident); ok {
		if v.info.ObjectOf(ident) != obj {
			return false
		}
	} else if !selectorChainRootsAtIdent(base, obj, v.info) {
		return false
	}

	// A pointer-typed receiver operand hands over the pointer VALUE — no address of obj is taken.
	if baseType := v.info.TypeOf(base); baseType == nil {
		return false
	} else if _, isPointer := baseType.Underlying().(*types.Pointer); isPointer {
		return false
	}

	selection, ok := v.info.Selections[sel]

	if !ok || selection.Kind() != types.MethodVal {
		return false
	}

	sig, ok := selection.Obj().Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return false
	}

	_, isPointerRecv := sig.Recv().Type().(*types.Pointer)

	return isPointerRecv
}

// argRootIsIdent reports whether passing arg to a pointer parameter hands the callee a
// pointer into identObj's own storage: arg is `&expr` whose storage root — peeled through
// parens, field selectors, index expressions (the CONTAINER, never the index), and derefs —
// is the ident, or arg is the bare ident itself (only possible when the ident is already
// pointer-typed). Anything else (the ident inside an index, an operand of arithmetic, a
// nested composite) contributes a value, not the ident's address; a literal `&ident` deeper
// inside such an expression is caught independently by the UnaryExpr arm.
func argRootIsIdent(arg ast.Expr, identObj types.Object, info *types.Info) bool {
	root := arg

	if unary, ok := arg.(*ast.UnaryExpr); ok && unary.Op == token.AND {
		root = unary.X

		for {
			switch expr := root.(type) {
			case *ast.ParenExpr:
				root = expr.X
				continue
			case *ast.SelectorExpr:
				root = expr.X
				continue
			case *ast.IndexExpr:
				root = expr.X
				continue
			case *ast.StarExpr:
				root = expr.X
				continue
			}

			break
		}
	}

	if id, ok := root.(*ast.Ident); ok {
		return info.ObjectOf(id) == identObj
	}

	return false
}

// selectorChainRootsAtIdent reports whether expr is a struct-field selector chain
// (`x.f1.…fn`, n>=1) whose peeled root is the ident under analysis, with every hop a
// direct VALUE field selection. Taking such a chain's address aliases the root local's
// OWN storage, so the local must be heap-boxed — the `Ꮡ(x).of(T.Ꮡval)` copy-box
// fallback otherwise orphans writes made through the pointer. A hop that crosses a
// pointer — an explicit `ptr.field` deref or a field promoted through an embedded
// pointer (both are Selection.Indirect()) — aliases the POINTEE's storage instead, so
// the root must NOT be boxed: the pointer value already routes through `.of(…)` (see
// convUnaryExpr). A missing Selections entry is a package qualifier, and a method
// value cannot stand under `&`, so both stop the walk.
func selectorChainRootsAtIdent(expr ast.Expr, identObj types.Object, info *types.Info) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	for {
		if selection, ok := info.Selections[sel]; !ok || selection.Kind() != types.FieldVal || selection.Indirect() {
			return false
		}

		base := sel.X

		for {
			if paren, ok := base.(*ast.ParenExpr); ok {
				base = paren.X
				continue
			}

			break
		}

		switch base := base.(type) {
		case *ast.SelectorExpr:
			sel = base
		case *ast.Ident:
			return info.ObjectOf(base) == identObj
		default:
			return false
		}
	}
}

// Check if the identifier is used in a type expression (like array size)
func containsIdentInTypeExpr(node ast.Expr, targetObj types.Object, info *types.Info) bool {
	if node == nil {
		return false
	}

	found := false

	ast.Inspect(node, func(n ast.Node) bool {
		if found {
			return false
		}

		if id, ok := n.(*ast.Ident); ok {
			if obj := info.ObjectOf(id); obj == targetObj {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// Check if the identifier is used in a value calculation but not directly stored
func containsIdentInValueCalc(node ast.Expr, targetObj types.Object, info *types.Info) bool {
	// Direct assignment of identifier is handled separately
	if id, ok := node.(*ast.Ident); ok {
		if obj := info.ObjectOf(id); obj == targetObj {
			return false // This is direct assignment, not just calculation
		}
		return false // Some other identifier
	}

	// Check if identifier is used in a binary operation
	if binExpr, ok := node.(*ast.BinaryExpr); ok {
		return containsIdentInValueCalc(binExpr.X, targetObj, info) ||
			containsIdentInValueCalc(binExpr.Y, targetObj, info)
	}

	// Check if identifier is used in a function call argument
	if callExpr, ok := node.(*ast.CallExpr); ok {
		for _, arg := range callExpr.Args {
			if containsIdentInValueCalc(arg, targetObj, info) {
				return true
			}
		}

		return false
	}

	// For other expression types, do a general search
	found := false
	ast.Inspect(node, func(n ast.Node) bool {
		if found {
			return false
		}

		if id, ok := n.(*ast.Ident); ok {
			if obj := info.ObjectOf(id); obj == targetObj {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// markSStringEligible records whether the string local bound by `ident := string(x)` may be emitted
// as a stack-only sstring — a zero-copy view over x's bytes — instead of the heap @string. The
// predicate is deliberately CONSERVATIVE (the MVP's safest idiom): it fires only for a plain-string
// local that does not escape, is never returned, is used only through safe reads (len/cap, byte
// index, or comparison against a string literal), and whose conversion source is never written for
// the lifetime of the view. Any uncertainty leaves the local as @string.
//
// sstring is a ref struct, so most missed escapes (storing into a field/slice/map, boxing to an
// interface, sending on a channel, capturing in a closure) become COMPILE errors rather than silent
// bugs. The two vectors that would be silently wrong — the local escaping via `return`, and mutation
// of the source buffer while the view is alive — are guarded explicitly below.
func (v *Visitor) markSStringEligible(ident *ast.Ident, rhs ast.Expr, body *ast.BlockStmt) {
	obj := v.info.ObjectOf(ident)

	if obj == nil {
		return
	}

	// Must be the built-in `string` type exactly — a named string type (`type S string`) would lose
	// its identity if emitted as sstring.
	if !types.Identical(obj.Type(), types.Typ[types.String]) {
		return
	}

	// Must not escape by any channel the escape analysis already detects.
	if v.identEscapesHeap[obj] {
		return
	}

	// Initializer must be a `string(x)` conversion whose source x is an unnamed []byte (a []rune must
	// UTF-8-encode — an allocation, no view; a named slice needs a two-hop cast C# will not chain).
	call := v.unnamedByteSliceStringConv(rhs)

	if call == nil {
		return
	}

	// The source's storage root must be an identifiable local/param so the mutation scan can track it.
	srcRoot := rootIdentObject(call.Args[0], v.info)

	if srcRoot == nil {
		return
	}

	// Every use of the local must be a safe read, and it must not be returned.
	if !v.sstringUsesAreSafe(obj, ident, body) {
		return
	}

	// The source must not be written anywhere in the function (strongest form of the guard for now).
	if v.objectIsWritten(srcRoot, body) {
		return
	}

	v.sstringEligible[obj] = true

	if os.Getenv("GO2CS_DEBUG_SSTRING") != "" {
		pos := v.fset.Position(ident.Pos())
		fmt.Fprintf(os.Stderr, "[sstring] eligible: %s at %s:%d:%d\n", ident.Name, pos.Filename, pos.Line, pos.Column)
	}
}

// sstringUsesAreSafe reports whether every use of the sstring-candidate local `obj` (other than its
// declaring occurrence `declIdent`) is a safe read: an argument to len/cap, the base of a byte index
// `s[i]`, or an operand of a comparison against a string literal (`s == "x"`). Any other use — passed
// to a function, stored, ranged, concatenated, converted, returned, reassigned — makes it ineligible.
func (v *Visitor) sstringUsesAreSafe(obj types.Object, declIdent *ast.Ident, body *ast.BlockStmt) bool {
	// Pass 1: collect the identifier nodes that sit in a safe-read slot.
	safeIdents := map[*ast.Ident]bool{}

	ast.Inspect(body, func(n ast.Node) bool {
		switch e := n.(type) {
		case *ast.CallExpr:
			if fn, ok := e.Fun.(*ast.Ident); ok && (fn.Name == "len" || fn.Name == "cap") {
				for _, arg := range e.Args {
					if id, ok := arg.(*ast.Ident); ok {
						safeIdents[id] = true
					}
				}
			}
		case *ast.IndexExpr:
			// `s[i]`: the indexed BASE is a safe byte read (an ident used as the index is separate).
			if id, ok := e.X.(*ast.Ident); ok {
				safeIdents[id] = true
			}
		case *ast.BinaryExpr:
			// A COMPARISON against a string literal (`s == "x"`) or a plain-`string` operand
			// (variable/field, `s == want`), or a string CONCATENATION (`s + suffix`): the stack
			// string compares/concatenates against a `u8` literal, another sstring, or — via the mixed
			// sstring/@string operators — a heap @string, with no heap copy of `s` (concatenation still
			// allocates the RESULT @string, but not the operand). The local never escapes through
			// either — a comparison yields a bool, a concatenation a fresh @string, neither aliasing
			// `s` — and a safe local's source is proven never-written for the whole function, so
			// evaluating the other operand cannot mutate the view.
			if isComparisonOp(e.Op) || e.Op == token.ADD {
				if id, ok := e.X.(*ast.Ident); ok && v.isPlainStringOperand(e.Y) {
					safeIdents[id] = true
				}
				if id, ok := e.Y.(*ast.Ident); ok && v.isPlainStringOperand(e.X) {
					safeIdents[id] = true
				}
			}
		case *ast.SwitchStmt:
			// `switch s { case …: … }` — the local is a switch tag over mutation-safe case labels.
			// That lowers to `var exprᴛN = s;` followed by `exprᴛN == label` comparisons (the same safe
			// `==` form as a direct comparison), so the tag read is a safe use.
			if id, ok := e.Tag.(*ast.Ident); ok && v.allCaseLabelsSafe(e.Body) {
				safeIdents[id] = true
			}
		}

		return true
	})

	// Pass 2: every occurrence of the local must be the declaration or one of those safe slots.
	allSafe := true

	ast.Inspect(body, func(n ast.Node) bool {
		if !allSafe {
			return false
		}

		if id, ok := n.(*ast.Ident); ok && id != declIdent && v.info.ObjectOf(id) == obj {
			if !safeIdents[id] {
				allSafe = false
			}
		}

		return true
	})

	return allSafe
}

// objectIsWritten reports whether `root`'s storage is (potentially) written anywhere in the body:
// as an assignment / increment target, through an address-of, or by being passed to a call that is
// not a conversion or len/cap (a slice shares its backing array, so any such callee could mutate it).
// This is the conservative "no write to the source for the whole function" form of the mutation guard.
func (v *Visitor) objectIsWritten(root types.Object, body *ast.BlockStmt) bool {
	written := false

	ast.Inspect(body, func(n ast.Node) bool {
		if written {
			return false
		}

		switch node := n.(type) {
		case *ast.AssignStmt:
			for _, lhs := range node.Lhs {
				// Skip root's OWN declaration (`root := …`): that establishes the initial value
				// before the view exists, it is not a mutation of it. Any later reassignment
				// (`root = append(root, …)`, `root = …`) is a distinct occurrence and IS flagged.
				if id := rootIdent(lhs); id != nil && v.info.ObjectOf(id) == root && id.Pos() != root.Pos() {
					written = true
				}
			}
		case *ast.IncDecStmt:
			if rootIdentObject(node.X, v.info) == root {
				written = true
			}
		case *ast.UnaryExpr:
			if node.Op == token.AND && rootIdentObject(node.X, v.info) == root {
				written = true
			}
		case *ast.CallExpr:
			// A conversion (`string(root)`, `[]byte(root)`) reads its operand; len/cap read too.
			if tv, ok := v.info.Types[node.Fun]; ok && tv.IsType() {
				return true
			}
			if fn, ok := node.Fun.(*ast.Ident); ok && (fn.Name == "len" || fn.Name == "cap") {
				return true
			}
			// Any other call receiving root (or a sub-slice of it) may mutate the shared backing.
			for _, arg := range node.Args {
				if rootIdentObject(arg, v.info) == root {
					written = true
				}
			}
		}

		return true
	})

	return written
}

// rootIdent peels an expression through parens, indexes, slice expressions and derefs to its root
// identifier, or nil if the root is not a plain identifier (a call result, a composite literal, ...).
func rootIdent(expr ast.Expr) *ast.Ident {
	for {
		switch e := expr.(type) {
		case *ast.ParenExpr:
			expr = e.X
		case *ast.IndexExpr:
			expr = e.X
		case *ast.SliceExpr:
			expr = e.X
		case *ast.StarExpr:
			expr = e.X
		case *ast.Ident:
			return e
		default:
			return nil
		}
	}
}

// rootIdentObject peels an expression to the object of its root identifier, or nil if the root is
// not a plain identifier, in which case the storage cannot be tracked.
func rootIdentObject(expr ast.Expr, info *types.Info) types.Object {
	if id := rootIdent(expr); id != nil {
		return info.ObjectOf(id)
	}

	return nil
}

// markSStringBinaryOperandConversions flags every `string(x)` conversion CallExpr that is an operand
// of a COMPARISON (`string(buf) == "…"` / `string(buf) == want`, any of ==/!=/</<=/>/>=) or a string
// CONCATENATION (`string(buf) + suffix`). Such a temporary is created and consumed within the single
// binary expression, so it cannot escape (the comparison yields a bool, the concatenation a fresh
// @string that shares no storage with the view); emitting it as a zero-copy sstring view is safe with
// NO escape analysis, provided the OTHER operand cannot mutate the source buffer before the view is
// read (see sstringOtherOperandSafe — a literal, a pure-read plain-`string` expression, or another
// `string(bytes)` conversion, none of which run code that could write x). Restricted to an unnamed
// []byte source, like the local case. Concatenation still allocates the RESULT @string; the win is
// skipping the intermediate `((@string)x)` copy of the operand (see sstring's `+` overloads).
func (v *Visitor) markSStringBinaryOperandConversions(file *ast.File) {
	ast.Inspect(file, func(n ast.Node) bool {
		binaryExpr, ok := n.(*ast.BinaryExpr)

		if !ok || !(isComparisonOp(binaryExpr.Op) || binaryExpr.Op == token.ADD) {
			return true
		}

		if call := v.unnamedByteSliceStringConv(binaryExpr.X); call != nil && v.sstringOtherOperandSafe(binaryExpr.Y) {
			v.sstringConvExprs[call] = true
		}

		if call := v.unnamedByteSliceStringConv(binaryExpr.Y); call != nil && v.sstringOtherOperandSafe(binaryExpr.X) {
			v.sstringConvExprs[call] = true
		}

		return true
	})
}

// markSStringSwitchConversions flags the `string(x)` conversion tag of a `switch string(x) { case … }`
// so it emits the zero-copy sstring view. A Go string switch ALWAYS lowers to a single temp assigned
// the tag value, then compared against each case label with `==` — an if/else chain, never a C#
// `switch` and never the constant-pattern (`is`) form: string constants render as `static readonly
// @string` (not a C# `const`), and string literals as `"…"u8`, neither of which is a C# case constant,
// so a `ref struct` is never made the subject of a pattern. Thus `var exprᴛN = ((sstring)x)` infers the
// stack string and every `exprᴛN == label` binds a zero-allocation operator (span for a `u8` literal,
// the mixed operator for an `@string` const/variable). The tag is evaluated exactly ONCE into the temp,
// so — as with the comparison case — the only requirement is that no case label can mutate x before the
// view is read: every label must be `sstringOtherOperandSafe` (a literal, a pure read, or another
// conversion — never a call), which also excludes a named string type that has no operator.
func (v *Visitor) markSStringSwitchConversions(file *ast.File) {
	ast.Inspect(file, func(n ast.Node) bool {
		switchStmt, ok := n.(*ast.SwitchStmt)

		if !ok || switchStmt.Tag == nil {
			return true
		}

		call := v.unnamedByteSliceStringConv(switchStmt.Tag)

		if call == nil {
			return true
		}

		if !v.allCaseLabelsSafe(switchStmt.Body) {
			return true
		}

		v.sstringConvExprs[call] = true

		return true
	})
}

// allCaseLabelsSafe reports whether every case label in a switch body is a mutation-safe comparison
// operand (`sstringOtherOperandSafe`) and at least one real case label is present (a `default` clause,
// which has no labels, is allowed). This is the condition under which the switch's `string(x)` tag can
// be a zero-copy sstring view: the switch lowers to `==` comparisons (see markSStringSwitchConversions)
// and no label evaluated during matching can write the tag's source.
func (v *Visitor) allCaseLabelsSafe(body *ast.BlockStmt) bool {
	if body == nil {
		return false
	}

	hasLabel := false

	for _, stmt := range body.List {
		caseClause, ok := stmt.(*ast.CaseClause)

		if !ok {
			return false
		}

		for _, label := range caseClause.List {
			if !v.sstringOtherOperandSafe(label) {
				return false
			}

			hasLabel = true
		}
	}

	return hasLabel
}

// sstringOtherOperandSafe reports whether `expr` — the operand ON THE OTHER SIDE of a comparison whose
// first operand is an unnamed `string(bytes)` conversion — is one that (a) the emitted stack-string
// comparison operators can handle and (b) cannot mutate the converted source before the comparison
// reads the zero-copy view. Three safe shapes: another `string(bytes)` conversion (also a view; the
// two compare as sstring == sstring); a string literal (`"…"u8`); or a PURE-READ plain-`string`
// expression — a variable, field, or index read, which executes no function call and so cannot write
// the buffer. A named string type is excluded (no operator against sstring). Anything with a call is
// rejected: `string(x) == f()` could see f mutate x between the (lazy) view and the compare.
func (v *Visitor) sstringOtherOperandSafe(expr ast.Expr) bool {
	if v.unnamedByteSliceStringConv(expr) != nil {
		return true
	}

	if isStringLiteralExpr(expr) {
		return true
	}

	if !isPureReadExpr(expr) {
		return false
	}

	t := v.info.TypeOf(expr)

	return t != nil && types.Identical(t, types.Typ[types.String])
}

// isPlainStringOperand reports whether `expr` is a string literal or an expression whose type is the
// built-in `string` exactly (not a named string type) — the operands an sstring can be compared
// against via its literal (u8), sstring, or mixed-@string comparison operators. Used for the
// named-local case, whose source is already proven never-written, so mutation ordering is moot and no
// purity check is needed.
func (v *Visitor) isPlainStringOperand(expr ast.Expr) bool {
	if isStringLiteralExpr(expr) {
		return true
	}

	t := v.info.TypeOf(expr)

	return t != nil && types.Identical(t, types.Typ[types.String])
}

// isPureReadExpr reports whether evaluating `expr` runs no function/method call, channel receive, or
// other side-effecting operation — so it cannot mutate anything, in particular the source buffer of a
// sibling `string(bytes)` view being compared against it. Conservative: only literals, identifiers,
// and reads composed of them (selector/index/slice/paren) qualify; a call or any other node fails.
func isPureReadExpr(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.BasicLit, *ast.Ident:
		return true
	case *ast.ParenExpr:
		return isPureReadExpr(e.X)
	case *ast.SelectorExpr:
		return isPureReadExpr(e.X)
	case *ast.IndexExpr:
		return isPureReadExpr(e.X) && isPureReadExpr(e.Index)
	case *ast.SliceExpr:
		return isPureReadExpr(e.X) &&
			(e.Low == nil || isPureReadExpr(e.Low)) &&
			(e.High == nil || isPureReadExpr(e.High)) &&
			(e.Max == nil || isPureReadExpr(e.Max))
	}

	return false
}

// unnamedByteSliceStringConv returns the CallExpr if expr is a `string(x)` conversion whose source x
// is an UNNAMED []byte — the form that can become a zero-copy sstring view — else nil. A []rune source
// must UTF-8-encode (an allocation), and a named []byte would need a two-hop cast C# will not chain.
func (v *Visitor) unnamedByteSliceStringConv(expr ast.Expr) *ast.CallExpr {
	call, ok := expr.(*ast.CallExpr)

	if !ok || len(call.Args) != 1 {
		return nil
	}

	if tv, ok := v.info.Types[call.Fun]; !ok || !tv.IsType() || !types.Identical(tv.Type, types.Typ[types.String]) {
		return nil
	}

	srcType := v.info.TypeOf(call.Args[0])

	if srcType == nil {
		return nil
	}

	if _, isNamed := types.Unalias(srcType).(*types.Named); isNamed {
		return nil
	}

	if slice, ok := srcType.Underlying().(*types.Slice); !ok {
		return nil
	} else if basic, ok := slice.Elem().Underlying().(*types.Basic); !ok || basic.Kind() != types.Uint8 {
		return nil
	}

	return call
}

func isStringLiteralExpr(expr ast.Expr) bool {
	lit, ok := expr.(*ast.BasicLit)

	return ok && lit.Kind == token.STRING
}

func isComparisonOp(op token.Token) bool {
	switch op {
	case token.EQL, token.NEQ, token.LSS, token.LEQ, token.GTR, token.GEQ:
		return true
	}

	return false
}

// ---- Heap boxing: acting on what the escape analysis above concluded ----
//
// The pass above DECIDES which identifiers need a heap box; the functions below act on that
// decision at emission time — asking whether a given identifier got one, and writing the C#
// declaration that creates it.
//
// identAddressTaken sits here beside objectAddressTaken and DELEGATES to it, so there is exactly
// one implementation of the walk and the two cannot drift apart.
//
// It asks for the directOnly sense — bare `&ident` only, never `&ident[i]` or `&ident.f` — and that
// narrowness is the point, not an omission. Its single caller is identHasHeapBox's INHERENTLY-HEAP
// branch, where `&s[i]` addresses the shared backing array and aliases correctly with no box;
// counting it would cost 40-odd hot stdlib helpers a per-call slice-header allocation for nothing.
// paramAddressTakenNeedsBox states the same policy for parameters, and says it mirrors this gate.
// Expressed as an argument, that agreement is visible; expressed as a missing case, it was not.

// identHasHeapBox reports whether the local behind obj is backed by a `Ꮡname` heap box.
// An escaping VALUE-type local always boxes. An INHERENTLY heap-allocated local (pointer/
// slice/map/chan/interface/func — already a reference, and blanket-marked escaping by the
// escape analysis) normally needs no box; it boxes only when its address is genuinely
// taken — by a capturing closure (a box-ref var: the closure writes through `&name` must
// reach the outer storage) or ANYWHERE in the current function (`zeroArray(&typ)` with
// `typ Type` — the `Ꮡ(typ)` copy-box fallback silently loses the callee's write through
// the pointer; dwarf zeroArray / InterfaceCasting replaceAnimal).
func (v *Visitor) identHasHeapBox(obj types.Object, identType types.Type) bool {
	if !v.identEscapesHeap[obj] {
		return false
	}

	if !isInherentlyHeapAllocatedType(identType) {
		return true
	}

	// An inherently-heap type (named slice/map/chan) is already a reference, so it is boxed only
	// when its address is genuinely needed: `&ident` taken, captured by-box in a closure, OR a
	// capture-mode pointer-receiver method is called on it (`frontier.Push(…)` with Push on
	// `*orderEventList`), which needs the ж overload's receiver box (CS1929 without it).
	return v.isLambdaBoxRefVar(obj) || v.identAddressTaken(obj) || packageCaptureModeBoxIdents[obj]
}

// identAddressTaken reports whether the bare form `&ident` occurs for obj anywhere in the current
// function, including nested function literals. Only the bare form: this is objectAddressTaken's
// directOnly sense, scoped to the current function declaration and memoized, and it is the sense
// its one caller wants — see the section note above for why `&ident[i]` must not count here.
//
// The memo is what earns the wrapper. This runs per identifier OCCURRENCE during emission, while
// objectAddressTaken walks a whole function body per call, so calling straight through would turn
// a cached lookup into a repeated full-body walk for every mention of every inherently-heap local.
//
// Visitor.identAddressTakenCache is safe across functions WITHOUT being reset: the key is the
// *types.Object, and a given object belongs to exactly one declaration, so an entry left over from
// an earlier function can never be consulted while converting a later one. That is why the cache is
// only lazily created and never cleared.
func (v *Visitor) identAddressTaken(obj types.Object) bool {
	// Also the guard that keeps the delegation safe: a nil *ast.FuncDecl passed as an ast.Node
	// would arrive as a non-nil interface holding a nil pointer, which objectAddressTaken's own
	// `body == nil` check cannot see.
	if v.currentFuncDecl == nil || obj == nil {
		return false
	}

	if taken, found := v.identAddressTakenCache[obj]; found {
		return taken
	}

	taken := v.objectAddressTaken(obj, v.currentFuncDecl, true)

	if v.identAddressTakenCache == nil {
		v.identAddressTakenCache = make(map[types.Object]bool)
	}

	v.identAddressTakenCache[obj] = taken
	return taken
}

func (v *Visitor) convertToHeapTypeDecl(ident *ast.Ident, createNew bool) string {
	identType := v.info.TypeOf(ident)

	// Check both Defs and Uses maps
	obj := v.info.Defs[ident]

	if obj == nil {
		obj = v.info.Uses[ident]
	}

	// A per-iteration for-clause variable's CARRIER is a plain value: its heap box (if any) is
	// declared fresh inside the loop body each pass, never at the clause declaration site (see
	// forClausePerIterVars).
	if obj != nil && v.forPerIterVars[obj] {
		return ""
	}

	if obj != nil && !v.identHasHeapBox(obj, identType) {
		return ""
	}

	goTypeName := v.getScopeCheckedTypeName(identType)
	csIDName := v.getIdentName(ident)

	// If identifier is discarded, return empty string
	if csIDName == "_" {
		return ""
	}

	// The local's name is sanitized (a C# keyword such as `base`/`as`/`event` becomes `@base`…),
	// matching how it is referenced elsewhere. The box keeps the raw name with the Ꮡ prefix
	// (`Ꮡbase` is already a valid identifier and is how its address is emitted everywhere).
	varName := getSanitizedIdentifier(csIDName)

	// Handle array types. A SLICE (`[]T` — empty length) is NOT an array: it must fall
	// through to the generic path (`heap<slice<T>>`), or the boxed ref-local's type
	// mismatches every use (a `[]nint` local boxed as `heap<array<nint>>`, CS0029).
	if arrayLen := strings.Split(strings.TrimPrefix(goTypeName, "["), "]")[0]; strings.HasPrefix(goTypeName, "[") && arrayLen != "" {

		// Get array element type
		arrayType := convertToCSTypeName(goTypeName[strings.Index(goTypeName, "]")+1:])

		// A heap-boxed local array needs the SAME per-element construction the plain-local and
		// global paths already emit: `new array<array<int32>>(16)` fills its backing with
		// `default(array<int32>)`, whose inner length exists only in the Go type — every element
		// reports len 0 and the first indexed write panics (flate's `leafCounts [16][16]int32`,
		// which is boxed because `copy(leafCounts[i][:i], …)` slices an element). Only the
		// address-taken shapes reach here, so the plain length is unchanged for every element
		// type whose `default(T)` is already the correct Go zero value (arrayZeroValueArgs).
		arrayCtorArgs := v.arrayZeroValueArgs(arrayLen, identType)

		if v.options.preferVarDecl {
			if createNew {
				return fmt.Sprintf("ref var %s = ref %s(new array<%s>(%s), out var %s%s);", varName, v.heapIntrinsicName(), arrayType, arrayCtorArgs, AddressPrefix, csIDName)
			}

			return fmt.Sprintf("ref var %s = ref %s<array<%s>>(out var %s%s);", varName, v.heapIntrinsicName(), arrayType, AddressPrefix, csIDName)
		}

		if createNew {
			return fmt.Sprintf("ref array<%s> %s = ref %s(new array<%s>(%s), out %s<array<%s>> %s%s);", arrayType, varName, v.heapIntrinsicName(), arrayType, arrayCtorArgs, PointerPrefix, arrayType, AddressPrefix, csIDName)
		}

		return fmt.Sprintf("ref array<%s> %s = ref %s<array<%s>>(out %s%s);", arrayType, varName, v.heapIntrinsicName(), arrayType, AddressPrefix, csIDName)
	}

	csTypeName := convertToCSTypeName(goTypeName)

	// An inherently heap-allocated type (interface/pointer/slice/map/chan/func) takes the
	// parameterless box form: `new Animal()` is invalid for an interface (CS0144), and the
	// reference-like zero value is exactly what `heap<T>(out …)` provides.
	if isInherentlyHeapAllocatedType(identType) {
		createNew = false
	}

	// `unsafe.Pointer` belongs in that set and the classifier cannot see it: go/types models it as a
	// BASIC kind, not a *types.Pointer, while golib models it as `Pointer : ж<uintptr>` — a CLASS
	// with no parameterless constructor. So `var x unsafe.Pointer` emitted
	// `heap(new @unsafe.Pointer(), …)`, which is CS1729 (reflect's all_test).
	//
	// Deliberately narrowed to THIS site rather than taught to isInherentlyHeapAllocatedType, which
	// has NINETEEN consumers spanning escape analysis, capture mode, star-expr deref, global
	// declarations, struct fields and IIFE prologues. Widening it there also flipped a
	// `ж<unsafe.Pointer>` deref from `.Value` to `.ValueSlot` in UnsafePointerReinterpret — a
	// project with no stdout comparison, so the change compiled and could not be shown correct.
	// The zero-value form is the whole of this defect; the rest of that surface is untouched.
	if basic, isBasic := identType.Underlying().(*types.Basic); isBasic && basic.Kind() == types.UnsafePointer {
		createNew = false
	}

	if v.options.preferVarDecl {
		if createNew {
			return fmt.Sprintf("ref var %s = ref %s(new %s(), out var %s%s);", varName, v.heapIntrinsicName(), csTypeName, AddressPrefix, csIDName)
		}

		return fmt.Sprintf("ref var %s = ref %s<%s>(out var %s%s);", varName, v.heapIntrinsicName(), csTypeName, AddressPrefix, csIDName)
	}

	if createNew {
		return fmt.Sprintf("ref %s %s = ref %s(out %s<%s> %s%s);", csTypeName, varName, v.heapIntrinsicName(), PointerPrefix, csTypeName, AddressPrefix, csIDName)
	}

	return fmt.Sprintf("ref %s %s = ref %s<%s>(out %s%s);", csTypeName, varName, v.heapIntrinsicName(), csTypeName, AddressPrefix, csIDName)
}

// isBoxedPointerLocal reports whether ident is a box-ref LOCAL of an inherently heap-allocated type
// (pointer/slice/map/chan/interface/func) — exactly the case convertToHeapTypeDecl heap-boxes as a
// `ж<ж<T>>` because its address is taken inside a capturing closure. For such a box, `Ꮡm.Value` reads the
// HELD reference value (which may legitimately be nil), so emission must use `.ValueSlot` (no nil-deref
// panic) rather than the strict `.Value`. A deref'd pointer PARAMETER is excluded: its box wraps the
// pointed-to value, so `Ꮡp.Value` is a genuine dereference that must keep the strict nil check.
func (v *Visitor) isBoxedPointerLocal(ident *ast.Ident) bool {
	obj := v.info.ObjectOf(ident)

	if obj == nil || !v.isLambdaBoxRefVar(obj) {
		return false
	}

	// A deref'd pointer PARAMETER or RECEIVER is excluded: its box `Ꮡp` wraps the pointed-to value
	// (a `ж<T>`), so `Ꮡp.Value` is a genuine dereference that must keep the strict nil check. Only a
	// pointer/slice/map/... LOCAL gets a box that wraps the pointer value itself (a `ж<ж<T>>`), where
	// `.Value` is a non-dereferencing read of the held value. (identIsParameter misses the receiver,
	// which is not in the parameter list — varIsDerefdPointerParam covers both.)
	if v.varIsDerefdPointerParam(obj) {
		return false
	}

	return isInherentlyHeapAllocatedType(v.getIdentType(ident))
}

// isInherentlyHeapAllocatedType checks if the type is inherently heap allocated,
// i.e., a reference type that is not a stack allocated value type, e.g., maps,
// slices, channels, interfaces, functions, and pointers.
func isInherentlyHeapAllocatedType(typ types.Type) bool {
	switch typ.Underlying().(type) {
	case *types.Map, *types.Slice, *types.Chan, *types.Interface, *types.Signature, *types.Pointer:
		// Maps, slices, channels, interfaces, functions and pointers are reference types
		return true
	default:
		return false
	}
}
