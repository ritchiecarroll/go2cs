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
	"go/constant"
	"go/token"
	"go/types"
	"path/filepath"
	"strings"
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
//
// moduleScope (see fixedSkipClosureApplies) adds the FIXED-SKIP PATH CLOSURE for a package outside the
// standard library: every SMALL function (isSmallCallerBody) on a same-package call path, of any length,
// up to a function whose runtime.Caller/Callers skip is FIXED (isFixedCallerSkip). logrus is the measured
// case: getCaller's runtime.Callers(minimumCallerDepth, ...) assumes a fixed frame count up to
// Entry.Info, the Release TieredCompilation=0 JIT inlines the small Info -> Log -> logArgs bodies (Log is
// an if, so not thin, and the skip is a package var, so no window), and the reported caller lands one
// frame too far out. Not the standard library: the same rule there measured +189 functions and no std
// failure needs it (ROOT 3 census, 2026-10-06). Never an init function: the runtime runs it in declaration
// order, no Go code calls it, and its reported name reads right unmarked -- the same reason std stays out.
func computeNoInliningClosure(files []FileEntry, pkg *types.Package, info *types.Info, moduleScope bool) map[types.Object]bool {
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
	// thinAllocators collects the single-allocation declarations (see isThinAllocator), each with
	// whether it was declared in a _test.go file, because its disposition depends on whether the
	// package READS THE HEAP PROFILE (readsHeapProfile) -- decided after the walk, like the opaque
	// forwarders above.
	type thinAllocator struct {
		obj    types.Object
		inTest bool
	}
	var thinAllocators []thinAllocator
	// directed collects the declarations Go's own source marks //go:noinline. They join the set only
	// after the fixed point below, so a thin forwarder TO one does not join (the directive protects
	// that one frame, not its callers), and they never count as the package's Caller/Callers user the
	// opaque-forwarder gate asks about.
	var directed []types.Object
	// launchers collects the declarations whose OWN body executes a `go` statement -- each is the
	// creator golib names for that goroutine. They join after the fixed point, like directed, so a
	// caller of one never joins (the creator is the frame executing the `go`, not its callers), and
	// they never count as the Caller/Callers user the opaque-forwarder gate asks about.
	var launchers []types.Object
	// genericFuncs collects the GENERIC declarations: one that directly calls a seeded thin allocator
	// joins the set once the allocators are decided (see callsSeededThinAllocator below).
	type genericFunc struct {
		obj  types.Object
		body *ast.BlockStmt
	}
	var genericFuncs []genericFunc
	// callers[g] = the package-scope declarations whose OWN body calls g (a closure's calls are its own
	// frame), and windows = the constant-skip runtime.Caller/Callers sites with the deepest caller depth
	// each one SKIPS -- see runtimeCallerSkipWindow. Recorded for every declaration, seeds included,
	// since a skipped frame can sit anywhere in the call graph.
	callers := map[types.Object]map[types.Object]bool{}
	type skipWindow struct {
		obj   types.Object
		depth int64
	}
	var windows []skipWindow
	// fixedSkipUsers and smallBodies feed the fixed-skip path closure (moduleScope only), applied after
	// the walk because a path's callers can be declared in any file.
	fixedSkipUsers := map[types.Object]bool{}
	smallBodies := map[types.Object]bool{}

	for _, entry := range files {
		if entry.file == nil {
			continue
		}
		inTest := strings.HasSuffix(entry.filePath, "_test.go")
		for _, decl := range entry.file.Decls {
			fn, ok := decl.(*ast.FuncDecl)
			if !ok || fn.Name == nil || fn.Body == nil {
				continue
			}
			obj := info.ObjectOf(fn.Name)
			if obj == nil {
				continue
			}

			if hasNoinlineDirective(fn.Doc) {
				directed = append(directed, obj)
			}

			if executesGoStatement(fn.Body) {
				launchers = append(launchers, obj)
			}

			if fn.Type.TypeParams != nil {
				genericFuncs = append(genericFuncs, genericFunc{obj: obj, body: fn.Body})
			}

			recordSamePackageCallees(info, pkg, obj, fn.Body, callers)

			if depth, ok := runtimeCallerSkipWindow(info, fn.Body); ok {
				windows = append(windows, skipWindow{obj: obj, depth: depth})
			}

			if moduleScope {
				if callsFixedSkipRuntimeCaller(info, fn) {
					fixedSkipUsers[obj] = true
				}

				// An init is never selected: the runtime runs it, no Go code calls it, and its reported name
				// already reads right unmarked (InitFrameNames).
				if isSmallCallerBody(fn.Body) && !(fn.Recv == nil && fn.Name.Name == "init") {
					smallBodies[obj] = true
				}
			}

			if callsSkipCountedRuntimeCaller(info, fn.Body) || callsSkipCountedWalker(info, fn.Body) {
				seed[obj] = true
				continue
			}

			if target := thinForwarderTarget(info, pkg, fn.Body); target != nil {
				forwarderTarget[obj] = target
				continue
			}

			if callsOpaqueFuncValue(info, fn.Body) {
				opaqueForwarders = append(opaqueForwarders, obj)
				continue
			}

			if isThinAllocator(info, fn.Body) {
				thinAllocators = append(thinAllocators, thinAllocator{obj: obj, inTest: inTest})
			}
		}
	}

	// The SKIP WINDOW: every in-package caller at a depth a constant-skip runtime.Caller/Callers SKIPS keeps
	// its frame, whatever its shape -- the thin-forwarder fixed point below only reaches one-statement
	// bodies. net/http's ServeMux is the measured case: registerErr's runtime.Caller(3) skips register (an
	// if with a panic) and Handle (an if/else), the Release TieredCompilation=0 JIT inlined one of them, and
	// TestRegisterErr's "registered at" landed on testing.tRunner (10/10 at TC0, 0/10 tiered). Only SKIPPED
	// depths: the reported frame and everything above it are the caller's code, and marking the first
	// reported frame too measured +167 std functions the JIT never inlines (a std census put the window
	// itself at +7 per target: net/http's 5 and internal/reflectlite's flag.mustBeExported/mustBeAssignable).
	// Seeded before the fixed point, so a thin forwarder to a window frame chains as usual.
	for _, window := range windows {
		for _, caller := range callersWithinDepth(window.obj, window.depth, callers) {
			seed[caller] = true
		}
	}

	// The FIXED-SKIP PATH CLOSURE (moduleScope only -- see this function's doc comment): walk upward over
	// every same-package caller, of any size, from each fixed-skip user, and mark the small functions on
	// the way. A large function on the path is walked THROUGH (a small caller above it still shifts the
	// count when inlined) but not marked: the JIT does not inline it.
	if len(fixedSkipUsers) > 0 {
		onPath := map[types.Object]bool{}
		var queue []types.Object

		for user := range fixedSkipUsers {
			onPath[user] = true
			queue = append(queue, user)
		}

		for len(queue) > 0 {
			fn := queue[0]
			queue = queue[1:]

			for caller := range callers[fn] {
				if onPath[caller] {
					continue
				}

				onPath[caller] = true
				queue = append(queue, caller)

				if smallBodies[caller] {
					seed[caller] = true
				}
			}
		}
	}

	// A thin allocator is marked ONLY in a package that reads the heap profile, and the reader must
	// be where the allocator is emitted: a PRODUCTION allocator needs a PRODUCTION reader. A -tests
	// conversion walks production and test files together, so gating a production allocator on a
	// test file's reader would emit it differently from the -stdlib conversion of the same file --
	// a standing closure shape every sweep would then have to restore.
	if len(thinAllocators) > 0 {
		productionReads := readsHeapProfile(files, info, false)
		anyReads := productionReads || readsHeapProfile(files, info, true)
		seededAllocators := map[types.Object]bool{}
		for _, a := range thinAllocators {
			if productionReads || (a.inTest && anyReads) {
				seed[a.obj] = true
				seededAllocators[a.obj] = true
			}
		}

		// A GENERIC function that directly calls a seeded thin allocator is marked too, before the
		// fixed point so a thin forwarder to it chains as usual. Its specialized instantiation is small
		// and the JIT inlines it under the Release TieredCompilation=0 default, so its frame vanishes
		// from the allocation's stack: runtime/pprof's TestGenericsInlineLocations read
		// TestGenericsInlineLocations;storeAlloc with both nonRecursiveGenericAllocFunction frames gone.
		// Generic callers only (a census of the heap-profile-reading packages found 4, all in test
		// files): a non-generic direct caller there is a large Test* body the JIT does not inline, so
		// marking it would add emission without keeping a frame. The allocator's own gate decides
		// production versus test, so a production caller is emitted identically by -stdlib and -tests.
		if len(seededAllocators) > 0 {
			for _, g := range genericFuncs {
				if callsSeededThinAllocator(info, g.body, seededAllocators) {
					seed[g.obj] = true
				}
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

	for _, obj := range directed {
		seed[obj] = true
	}

	// THE GO CREATOR: golib names a goroutine's creator (Go's gp.gopc, printed as `created by <func>`)
	// by walking the launching thread's stack, so the function executing the `go` must keep its frame.
	// Under the Release TieredCompilation=0 JIT it was elided two ways: inlined into its caller, or --
	// for a `go` in tail position -- replaced by an opportunistic tail call into goǃ. Measured
	// 2026-10-01: unique's map-cleanup goroutine read `created by sync.(*Once).doSlow` on net/http at
	// TC0 (runtime.unique_runtime_registerUniqueMapCleanup, reached behind a delegate invoke, tail-called
	// its trailing goǃ). A probe at TC0 showed [MethodImpl(NoInlining)] suppresses BOTH mechanisms, on a
	// method and on a C# lambda or local function alike (correcting the earlier sizing claim that it
	// could not reach a tail call). Func literals take the same rule in litNoInliningPrefix.
	for _, obj := range launchers {
		seed[obj] = true
	}

	return seed
}

// executesGoStatement reports whether body ITSELF executes a `go` statement -- one not inside a nested
// func literal, which is its own frame and so its own creator. A literal that is the `go` statement's
// callee (`go func() { ... }()`) is the goroutine's body, not a creator, and is not descended into.
func executesGoStatement(body *ast.BlockStmt) bool {
	if body == nil {
		return false
	}

	found := false

	ast.Inspect(body, func(n ast.Node) bool {
		if found {
			return false
		}

		switch n.(type) {
		case *ast.FuncLit:
			return false
		case *ast.GoStmt:
			found = true
			return false
		}

		return true
	})

	return found
}

// hasNoinlineDirective reports whether a declaration's doc comment carries Go's `//go:noinline`. Go's
// compiler then keeps the function's own frame, and Go code relies on that frame: runtime's
// TestRuntimePanic needs unexportedPanicForTesting's frame to exist when its index panic is raised, so
// panicCheck1 sees package runtime (golib's RuntimePanicCheck), and under the Release TieredCompilation=0
// default the JIT otherwise inlines it into the caller and the fatal becomes a recoverable panic.
func hasNoinlineDirective(doc *ast.CommentGroup) bool {
	if doc == nil {
		return false
	}

	for _, comment := range doc.List {
		if comment.Text == "//go:noinline" {
			return true
		}
	}

	return false
}

// skipCountedWalkers are the stack walkers whose recorded frames include their CALLER's, keyed by package
// path and name: the runtime's own skip-counting walkers -- `callers` and `gcallers` (hand-owned over the
// managed walk, captureCallers) and `saveblockevent` (hand-owned, forwarding its skip to callers) -- and
// the frame-LISTING walkers a Go program calls, `runtime.Stack`, `runtime/debug.Stack` and
// `runtime/debug.PrintStack`, whose printed traceback starts at their caller's frame. runtime/debug's
// TestStack is the measured frame-listing case: its one-line `(*T).ptrmethod` / `T.method` forwarders into
// debug.Stack vanished from the traceback, and hand-marking both [MethodImpl(NoInlining)] brought both
// frames back (G, 2026-09-28, linux). `runtime.unlock2` (hand-owned) walks too: its contended-unlock
// record captures the mutex-profile stack with a fixed skip of 3 (recordUnlock's frame, unlock2,
// unlockWithRank) so the stack starts at runtime.unlock, and under Release TieredCompilation=0 the JIT
// inlined the thin unlockWithRank/unlock forwarders and the record lost runtime.unlock (census A3,
// GolibTests RuntimeLockProfileTests). A test extends the set copy-on-write to stand a fixture package in.
var skipCountedWalkers = map[string]bool{
	"runtime.callers":          true,
	"runtime.gcallers":         true,
	"runtime.saveblockevent":   true,
	"runtime.unlock2":          true,
	"runtime.Stack":            true,
	"runtime/debug.Stack":      true,
	"runtime/debug.PrintStack": true,
}

// callsSkipCountedWalker reports whether body calls one of skipCountedWalkers directly. Such a function
// is the HOP nearest the walk: its own frame is one of the Go frames the skip it passes counts, so a JIT
// that inlines it removes a counted frame and every stack the walk records starts one real frame too
// high -- the rule captureCallers states for runtime's own entry points (managed_impl.cs: "an inlined hop
// would silently shift every answer by one"). The converted runtime.blockevent is the measured case:
// runtime/pprof's TestBlockProfileBias read [TestBlockProfileBias, tRunner] where Go reads
// [blockFrequentShort, TestBlockProfileBias, tRunner], and hand-marking blockevent [MethodImpl(NoInlining)]
// alone restored the frame and passed the test (G, 2026-09-28, linux). runtime.mutexevent is the same
// shape, one hop over the same walker.
func callsSkipCountedWalker(info *types.Info, body *ast.BlockStmt) bool {
	found := false

	ast.Inspect(body, func(n ast.Node) bool {
		if found {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit {
			return false // a literal's calls belong to the literal's own frame
		}

		call, ok := n.(*ast.CallExpr)

		if !ok {
			return true
		}

		var ident *ast.Ident

		switch fun := ast.Unparen(call.Fun).(type) {
		case *ast.Ident:
			ident = fun
		case *ast.SelectorExpr:
			ident = fun.Sel
		}

		if ident == nil {
			return true
		}

		if callee, ok := info.Uses[ident].(*types.Func); ok && callee.Pkg() != nil && skipCountedWalkers[callee.Pkg().Path()+"."+callee.Name()] {
			found = true
		}

		return !found
	})

	return found
}

// isThinAllocator reports whether body is a single statement whose whole work is ONE heap allocation
// Go's memory profile samples in that function's frame: `return make(...)` / `return new(T)` /
// `return &T{...}` / a slice or map composite literal, or the same as the sole right-hand side of an
// assignment or as a bare expression statement. runtime/pprof's genericAllocFunc is the measured case
// (`return make([]T, n)`): a Release-tier JIT inlines a body this small into its caller, the
// allocation's sampled stack then starts at the CALLER, and TestGenericsHashKeyInPprofBuilder's
// `...;runtime/pprof.genericAllocFunc[...]` frame is simply absent. Hand-marking genericAllocFunc
// [MethodImpl(NoInlining)] restored the frame (G, 2026-09-28, the linux cut of class F).
//
// Same scope rule as thinForwarderTarget: a larger allocating function is not an inlining candidate in
// the first place, so marking it would cost without protecting anything.
func isThinAllocator(info *types.Info, body *ast.BlockStmt) bool {
	if body == nil || len(body.List) != 1 {
		return false
	}

	var expr ast.Expr

	switch stmt := body.List[0].(type) {
	case *ast.ReturnStmt:
		if len(stmt.Results) != 1 {
			return false
		}
		expr = stmt.Results[0]
	case *ast.AssignStmt:
		if len(stmt.Rhs) != 1 {
			return false
		}
		expr = stmt.Rhs[0]
	case *ast.ExprStmt:
		expr = stmt.X
	default:
		return false
	}

	switch e := ast.Unparen(expr).(type) {
	case *ast.CallExpr:
		if ident, ok := ast.Unparen(e.Fun).(*ast.Ident); ok {
			if builtin, ok := info.Uses[ident].(*types.Builtin); ok {
				return builtin.Name() == "make" || builtin.Name() == "new"
			}
		}
	case *ast.UnaryExpr:
		if e.Op == token.AND {
			_, isLiteral := ast.Unparen(e.X).(*ast.CompositeLit)
			return isLiteral
		}
	case *ast.CompositeLit:
		if t := info.TypeOf(e); t != nil {
			switch t.Underlying().(type) {
			case *types.Slice, *types.Map:
				return true
			}
		}
	}

	return false
}

// callsSeededThinAllocator reports whether body directly calls one of the seeded thin allocators --
// by name, through an instantiation (f[T](...)), or through a selector -- resolved through go/types, so
// a generic allocator's instantiation matches its origin.
func callsSeededThinAllocator(info *types.Info, body *ast.BlockStmt, seeded map[types.Object]bool) bool {
	found := false

	ast.Inspect(body, func(n ast.Node) bool {
		if found {
			return false
		}

		call, ok := n.(*ast.CallExpr)
		if !ok {
			return true
		}

		var ident *ast.Ident

		switch fun := call.Fun.(type) {
		case *ast.Ident:
			ident = fun
		case *ast.IndexExpr:
			ident, _ = fun.X.(*ast.Ident)
		case *ast.IndexListExpr:
			ident, _ = fun.X.(*ast.Ident)
		case *ast.SelectorExpr:
			ident = fun.Sel
		}

		if ident == nil {
			return true
		}

		obj := info.Uses[ident]

		if fn, ok := obj.(*types.Func); ok && fn.Origin() != nil {
			obj = fn.Origin()
		}

		if obj != nil && seeded[obj] {
			found = true
		}

		return !found
	})

	return found
}

// heapProfileReaders are the functions through which Go code reads the memory profile, by the
// package path and name go/types reports for the used object.
var heapProfileReaders = map[string]bool{
	"runtime.MemProfile":              true,
	"runtime/pprof.WriteHeapProfile": true,
	"runtime/pprof.Lookup":            true,
}

// readsHeapProfile reports whether any file in the chosen set -- the production files, or with
// tests set, the _test.go files -- USES one of heapProfileReaders, qualified (`pprof.Lookup`) or not
// (an internal test file of runtime/pprof itself calls `WriteHeapProfile` bare). A use, never a
// definition: the package that DECLARES WriteHeapProfile does not thereby read the profile.
func readsHeapProfile(files []FileEntry, info *types.Info, tests bool) bool {
	for _, entry := range files {
		if entry.file == nil || strings.HasSuffix(entry.filePath, "_test.go") != tests {
			continue
		}

		found := false

		ast.Inspect(entry.file, func(n ast.Node) bool {
			if found {
				return false
			}

			ident, ok := n.(*ast.Ident)

			if !ok {
				return true
			}

			if fn, ok := info.Uses[ident].(*types.Func); ok && fn.Pkg() != nil && heapProfileReaders[fn.Pkg().Path()+"."+fn.Name()] {
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

// runtimeCallerSkipWindow reports the deepest CALLER depth that body's constant-skip runtime.Caller /
// runtime.Callers calls skip, with body's own function at depth 0. Go's skip semantics: Caller(k) skips
// depths 0..k-1 and reports depth k; Callers(k) counts itself as frame 0, so it skips depths 0..k-2 and
// records from depth k-1. ok is false when no call has a constant skip that reaches past body's own frame
// (that frame is the direct caller, which the seed already keeps). A non-constant skip cannot be sized and
// is ignored here; its direct caller is still seeded by callsSkipCountedRuntimeCaller. Stops at a nested
// *ast.FuncLit: a closure is a frame of its own.
func runtimeCallerSkipWindow(info *types.Info, body *ast.BlockStmt) (int64, bool) {
	if body == nil || info == nil {
		return 0, false
	}

	var deepest int64

	ast.Inspect(body, func(n ast.Node) bool {
		if _, isLit := n.(*ast.FuncLit); isLit {
			return false
		}

		call, ok := n.(*ast.CallExpr)

		if !ok || len(call.Args) == 0 {
			return true
		}

		sel, ok := call.Fun.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		used, ok := info.Uses[sel.Sel].(*types.Func)

		if !ok || used.Pkg() == nil || used.Pkg().Path() != "runtime" {
			return true
		}

		var unskipped int64 // Caller(k) skips k frames counting F; Callers(k) skips k-1 of them

		switch used.Name() {
		case "Caller":
			unskipped = 1
		case "Callers":
			unskipped = 2
		default:
			return true
		}

		value := info.Types[call.Args[0]].Value

		if value == nil {
			return true
		}

		if skip, exact := constant.Int64Val(value); exact && skip-unskipped > deepest {
			deepest = skip - unskipped
		}

		return true
	})

	return deepest, deepest > 0
}

// recordSamePackageCallees adds caller to callers[g] for every same-package function g that body calls
// directly (an identifier, a selector, or an explicit instantiation), keyed by g's origin so a generic
// callee's instances fold into its declaration. A call through an interface or a function value names no
// declaration and adds nothing. Stops at a nested *ast.FuncLit: a closure's calls are its own frame's.
func recordSamePackageCallees(info *types.Info, pkg *types.Package, caller types.Object, body *ast.BlockStmt, callers map[types.Object]map[types.Object]bool) {
	if body == nil || info == nil {
		return
	}

	ast.Inspect(body, func(n ast.Node) bool {
		if _, isLit := n.(*ast.FuncLit); isLit {
			return false
		}

		call, ok := n.(*ast.CallExpr)

		if !ok {
			return true
		}

		fun := ast.Unparen(call.Fun)

		switch index := fun.(type) {
		case *ast.IndexExpr:
			fun = index.X
		case *ast.IndexListExpr:
			fun = index.X
		}

		var ident *ast.Ident

		switch f := fun.(type) {
		case *ast.Ident:
			ident = f
		case *ast.SelectorExpr:
			ident = f.Sel
		default:
			return true
		}

		callee, ok := info.Uses[ident].(*types.Func)

		if !ok || callee.Pkg() != pkg {
			return true
		}

		target := callee.Origin()

		if callers[target] == nil {
			callers[target] = map[types.Object]bool{}
		}

		callers[target][caller] = true

		return true
	})
}

// callersWithinDepth returns every declaration that reaches fn through at most depth same-package calls
// (fn itself excluded), breadth-first over callers.
func callersWithinDepth(fn types.Object, depth int64, callers map[types.Object]map[types.Object]bool) []types.Object {
	var reached []types.Object

	seen := map[types.Object]bool{fn: true}
	frontier := []types.Object{fn}

	for level := int64(0); level < depth && len(frontier) > 0; level++ {
		var next []types.Object

		for _, g := range frontier {
			for caller := range callers[g] {
				if !seen[caller] {
					seen[caller] = true
					reached = append(reached, caller)
					next = append(next, caller)
				}
			}
		}

		frontier = next
	}

	return reached
}

// fixedSkipClosureApplies reports whether computeNoInliningClosure's fixed-skip path closure applies to the
// package converted from dir: every package outside goRoot's src tree (GOROOT-vendored golang.org/x/...
// included in the standard library). An unknown dir or GOROOT answers false, the conservative side.
func fixedSkipClosureApplies(dir, goRoot string) bool {
	if dir == "" || goRoot == "" {
		return false
	}

	return !isPathUnder(dir, filepath.Join(goRoot, "src"))
}

// callsFixedSkipRuntimeCaller reports whether fn's OWN body (a nested *ast.FuncLit is a frame of its own)
// calls runtime.Caller or runtime.Callers with a FIXED skip (isFixedCallerSkip).
func callsFixedSkipRuntimeCaller(info *types.Info, fn *ast.FuncDecl) bool {
	if fn == nil || fn.Body == nil || info == nil {
		return false
	}

	found := false

	ast.Inspect(fn.Body, func(n ast.Node) bool {
		if found {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit {
			return false
		}

		call, ok := n.(*ast.CallExpr)

		if !ok || len(call.Args) == 0 {
			return true
		}

		sel, ok := call.Fun.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		used, ok := info.Uses[sel.Sel].(*types.Func)

		if !ok || used.Pkg() == nil || used.Pkg().Path() != "runtime" || (used.Name() != "Caller" && used.Name() != "Callers") {
			return true
		}

		found = isFixedCallerSkip(info, fn, call.Args[0])

		return !found
	})

	return found
}

// isFixedCallerSkip reports whether a runtime.Caller/Callers skip argument is FIXED -- the same count on
// every call, so it assumes the frames between the call and its target exist: a constant, a package-level
// variable (logrus' minimumCallerDepth), or a parameter of fn itself (log's output(pc, calldepth), whose
// callers pass constants). A skip computed inside fn (a loop variable, arithmetic on a local) is not.
func isFixedCallerSkip(info *types.Info, fn *ast.FuncDecl, arg ast.Expr) bool {
	arg = ast.Unparen(arg)

	if tv, ok := info.Types[arg]; ok && tv.Value != nil {
		return true
	}

	ident, ok := arg.(*ast.Ident)

	if !ok {
		return false
	}

	v, ok := info.Uses[ident].(*types.Var)

	if !ok {
		return false
	}

	if v.Pkg() != nil && v.Parent() == v.Pkg().Scope() {
		return true
	}

	if fn.Type.Params != nil {
		for _, field := range fn.Type.Params.List {
			for _, name := range field.Names {
				if info.Defs[name] == v {
					return true
				}
			}
		}
	}

	return false
}

// isSmallCallerBody reports whether body is small enough for the Release TieredCompilation=0 JIT to want to
// inline: at most 3 statements counted at every nesting level (an if and its one-statement body are 2), and
// no loop, switch, select, defer, go statement or func literal.
func isSmallCallerBody(body *ast.BlockStmt) bool {
	if body == nil {
		return false
	}

	count := 0
	small := true

	ast.Inspect(body, func(n ast.Node) bool {
		switch n.(type) {
		case *ast.ForStmt, *ast.RangeStmt, *ast.SelectStmt, *ast.SwitchStmt, *ast.TypeSwitchStmt, *ast.DeferStmt, *ast.GoStmt, *ast.FuncLit:
			small = false
		case *ast.BlockStmt:
		case ast.Stmt:
			count++
		}

		return small
	})

	return small && count <= 3
}
