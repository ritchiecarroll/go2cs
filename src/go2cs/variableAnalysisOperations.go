// variableAnalysisOperations.go - Gbtc
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
	"sort"
	"strings"
)

type TrackerType string

const (
	BlockTracker      TrackerType = "block"
	RangeTracker      TrackerType = "range"
	ForTracker        TrackerType = "for"
	FuncTracker       TrackerType = "func"
	IfTracker         TrackerType = "if"
	SwitchTracker     TrackerType = "switch"
	TypeSwitchTracker TrackerType = "typeswitch"
	SelectTracker     TrackerType = "select"
)

// These variable analysis functions are used to analyze variable declarations and assignments
// in the Go source code. The analysis is performed on global variables and on a per-function
// basis. The goal is to identify reassignments and shadowed variables so these can be handled
// correctly during the conversion process since C# does not allow redeclarations or shadowing
// of variables with the same name, at least within the same function scope. Two derivative
// functions use the results of this analysis: `getIdentName` and `isReassignment`.

// Perform variable analysis on the global ValueSpec declarations
func performGlobalVariableAnalysis(decls []ast.Decl, info *types.Info, globalIdentNames map[*ast.Ident]string, globalScope map[string]*types.Var) {
	for _, decl := range decls {
		switch genDecl := decl.(type) {
		case *ast.GenDecl:
			for _, spec := range genDecl.Specs {
				switch spec := spec.(type) {
				case *ast.ValueSpec:
					for _, ident := range spec.Names {
						varName := ident.Name

						if isDiscardedVar(varName) {
							continue
						}

						obj := info.Defs[ident]

						if obj == nil {
							continue
						}

						var varObj *types.Var
						var ok bool

						if varObj, ok = obj.(*types.Var); !ok {
							continue
						}

						globalIdentNames[ident] = getSanitizedIdentifier(varName)
						globalScope[varName] = varObj

						// THE rename site for a package-level var — so it is also where a
						// `-tests` conversion records the rename for the OTHER variant to read.
						// `export_test.go`'s `var Lock = lock` collides with RWMutex's own `Lock`
						// METHOD, and getSanitizedIdentifier above Δ-renames the var to `ΔLock`
						// exactly as getCollisionAvoidanceIdentifier's doc comment describes for a
						// type-vs-method collision. The EXTERNAL test variant referencing
						// `runtime.Lock` renders through whiteboxBridgeMember, whose own
						// nameCollisions is the fresh per-variant map whiteboxBridgeNamedType's doc
						// comment already explains for types — it has no entry for a name the
						// external variant's files never declare, so the bridge spelled the bare
						// `Lock` against a declaration emitted as `ΔLock` (CS1503, metrics_test).
						// testTypeRenames is object-keyed and session-scoped for precisely this.
						//
						// It must be registered HERE and not in visitValueSpec: by the time that
						// visitor runs, getIdentName returns this map's POST-rename `ΔLock`, for
						// which nameCollisions answers false — a registration there cannot fire for
						// any package-level var. Nil outside a `-tests` conversion (and during the
						// production pass that precedes one), which is the whole gate this needs:
						// only a `-tests` run has a second variant to inform, and only an object
						// declared in the run's own package-under-test can reach the lookup
						// (whiteboxBridgeObject). Nothing here is type-specific, so a renamed var
						// registers into the identical map visitTypeSpec uses.
						if testTypeRenames != nil && nameCollisions[varName] {
							testTypeRenames[varObj] = true
						}
					}
				}
			}
		}
	}
}

// trackerRegistry manages all our nested trackers
type trackerRegistry struct {
	trackers map[TrackerType]*nestedVarTracker
}

func newTrackerRegistry() *trackerRegistry {
	reg := &trackerRegistry{
		trackers: make(map[TrackerType]*nestedVarTracker),
	}
	// Initialize all tracker types
	for _, t := range []TrackerType{
		BlockTracker,
		RangeTracker,
		ForTracker,
		FuncTracker,
		IfTracker,
		SwitchTracker,
		TypeSwitchTracker,
		SelectTracker,
	} {
		reg.trackers[t] = newNestedVarTracker(string(t))
	}
	return reg
}

func (r *trackerRegistry) get(t TrackerType) *nestedVarTracker {
	return r.trackers[t]
}

// nestedVarTracker handles tracking of variables across nested scopes
type nestedVarTracker struct {
	stmtType   string                            // type of statement being tracked
	level      int                               // current nesting level
	vars       map[int]map[*ast.Ident]*types.Var // vars at each nesting level
	processing bool                              // flag for when processing vars in this tracker
}

func newNestedVarTracker(stmtType string) *nestedVarTracker {
	return &nestedVarTracker{
		stmtType: stmtType,
		vars:     make(map[int]map[*ast.Ident]*types.Var),
	}
}

func (t *nestedVarTracker) enter() {
	t.level++
	t.vars[t.level] = make(map[*ast.Ident]*types.Var)
}

func (t *nestedVarTracker) exit() {
	delete(t.vars, t.level)
	t.level--
}

func (t *nestedVarTracker) isTrackedAtOtherLevel(ident *ast.Ident) bool {
	for level := 1; level < t.level; level++ {
		if _, exists := t.vars[level][ident]; exists {
			return true
		}
	}
	return false
}

func (t *nestedVarTracker) track(ident *ast.Ident, varObj *types.Var) {
	t.vars[t.level][ident] = varObj
}

// varProcessor handles variable declaration and assignment processing
type varProcessor struct {
	tracker            *nestedVarTracker
	functionLevelDecls map[string]*types.Var
	declaredPos        map[*types.Var]token.Pos
	info               *types.Info
	declareVar         func(string, *types.Var, *ast.Ident)
	reassignVar        func(string, *ast.Ident)
}

func newVarProcessor(
	tracker *nestedVarTracker,
	functionLevelDecls map[string]*types.Var,
	declaredPos map[*types.Var]token.Pos,
	info *types.Info,
	declareVar func(string, *types.Var, *ast.Ident),
	reassignVar func(string, *ast.Ident),
) *varProcessor {
	return &varProcessor{
		tracker:            tracker,
		functionLevelDecls: functionLevelDecls,
		declaredPos:        declaredPos,
		info:               info,
		declareVar:         declareVar,
		reassignVar:        reassignVar,
	}
}

func (p *varProcessor) processIdent(ident *ast.Ident, isDefine bool) {
	if ident == nil || isDiscardedVar(ident.Name) {
		return
	}

	if isDefine {
		if obj := p.info.Defs[ident]; obj != nil {
			if varObj, ok := obj.(*types.Var); ok {
				p.declareVar(ident.Name, varObj, ident)
				p.tracker.track(ident, varObj)
			}
		}
	} else {
		if obj := p.info.Uses[ident]; obj != nil {
			if varObj, ok := obj.(*types.Var); ok {
				if !p.tracker.isTrackedAtOtherLevel(ident) {
					p.functionLevelDecls[ident.Name] = varObj
					p.declaredPos[varObj] = ident.Pos()
				}
				p.tracker.track(ident, varObj)
				p.reassignVar(ident.Name, ident)
			}
		}
	}
}

func (p *varProcessor) processAssignStmt(stmt *ast.AssignStmt) {
	isDefine := stmt.Tok == token.DEFINE
	for _, expr := range stmt.Lhs {
		if ident, ok := expr.(*ast.Ident); ok {
			p.processIdent(ident, isDefine)
		}
	}
}

func newLambdaCapture() *LambdaCapture {
	return &LambdaCapture{
		capturedVars:         make(map[*ast.Ident]*CapturedVarInfo),
		stmtCaptures:         make(map[ast.Node]map[*ast.Ident]bool),
		pendingCaptures:      make(map[string]*CapturedVarInfo),
		currentLambdaVars:    make(map[string]string),
		currentLambdaVarObjs: make(map[string]types.Object),
		boxRefVars:           make(map[types.Object]bool),
		detectingCaptures:    true,
	}
}

// Helper functions to manage conversion phase lambda context. enter/exit are paired (the callers
// always `defer v.exitLambdaConversion()`) and nest: enter PUSHES the current conversion state and
// installs fresh state for `node`; exit POPS and restores the enclosing state. Restoring (rather
// than resetting to false/nil) is what lets a receiver/box reference in an enclosing lambda's body
// keep rendering through its box after a NESTED lambda finishes (CS8175 — see conversionStack).
func (v *Visitor) enterLambdaConversion(node ast.Node) {
	v.lambdaCapture.conversionStack = append(v.lambdaCapture.conversionStack, lambdaConversionState{
		conversionInLambda:   v.lambdaCapture.conversionInLambda,
		currentConversion:    v.lambdaCapture.currentConversion,
		currentLambdaVars:    v.lambdaCapture.currentLambdaVars,
		currentLambdaVarObjs: v.lambdaCapture.currentLambdaVarObjs,
	})
	v.lambdaCapture.conversionInLambda = true
	v.lambdaCapture.currentConversion = node
	v.lambdaCapture.currentLambdaVars = make(map[string]string)
	v.lambdaCapture.currentLambdaVarObjs = make(map[string]types.Object)
}

func (v *Visitor) exitLambdaConversion() {
	if n := len(v.lambdaCapture.conversionStack); n > 0 {
		prev := v.lambdaCapture.conversionStack[n-1]
		v.lambdaCapture.conversionStack = v.lambdaCapture.conversionStack[:n-1]
		v.lambdaCapture.conversionInLambda = prev.conversionInLambda
		v.lambdaCapture.currentConversion = prev.currentConversion
		v.lambdaCapture.currentLambdaVars = prev.currentLambdaVars
		v.lambdaCapture.currentLambdaVarObjs = prev.currentLambdaVarObjs
		return
	}

	// Defensive: an unbalanced exit (no matching enter) falls back to the top-level reset.
	v.lambdaCapture.conversionInLambda = false
	v.lambdaCapture.currentConversion = nil
	v.lambdaCapture.currentLambdaVars = nil
	v.lambdaCapture.currentLambdaVarObjs = nil
}

// performVariableAnalysis walks one function body BEFORE any C# is emitted for it, and records
// everything the emitters will need to know about its variables.
//
// It exists because Go and C# disagree about names and lifetimes in ways a single forward pass
// cannot reconcile. Go lets an inner block redeclare a name that is still live outside it; C# does
// not, so the inner one must be renamed — and the rename has to be decided before the OUTER uses
// are emitted. Go closures capture variables by reference; C# lambdas do too, but Go's loop-scoped
// variables and the converter's snapshot copies make "which binding does this name mean here"
// answerable only with the whole body in view.
//
// The results land on the Visitor (identNames, lambdaCapture, scopeStack facts and friends) and are
// consulted from convIdent outward. Nothing here emits anything.
//
// At ~1,600 lines this is one of the converter's three giant functions and is planned for
// decomposition; it is a sequence of walks over the same body, which is where the seams are.
func (v *Visitor) performVariableAnalysis(funcDecl *ast.FuncDecl, signature *types.Signature) {
	v.identNames = make(map[*ast.Ident]string)
	v.isReassigned = make(map[*ast.Ident]bool)
	v.scopeStack = []map[string]*types.Var{v.globalScope}
	v.hasDefer = false
	v.hasRecover = false
	v.captureAnalysisDecl = funcDecl
	v.captureShareFactsCache = nil

	// blockDecls is kept index-aligned with v.scopeStack. For a genuine block scope it holds the
	// full set of names declared *directly* in that block (pre-scanned when the scope is pushed),
	// so a nested variable can be shadow-renamed when it collides with a same-named variable
	// declared LATER in an enclosing block. C# CS0136 fires regardless of declaration order, but
	// the scope stack alone only records declarations seen so far (backward), so a forward
	// (later-in-source) declaration in an intermediate block would otherwise be missed. nil for
	// non-block scopes (control-statement init scopes, parameter scopes, and the global scope at
	// index 0). Generalizes the function-body-only forward detection (functionLevelDecls) to every
	// block level.
	blockDecls := []map[string]bool{nil}

	// Reset all capture-related state at the start of each function
	v.lambdaCapture = newLambdaCapture()
	v.capturedVarCount = make(map[string]int)

	// Track all function-level declarations for proper shadowing
	functionLevelDecls := make(map[string]*types.Var)
	// Expose the same map to emission (convIdent) so a global-var reference shadowed by a same-named
	// function-level local can be package-qualified. The reference is repopulated per function; by the
	// time the body is emitted the pre-scan has fully populated it.
	v.funcLevelDecls = functionLevelDecls
	var nameCounts = make(map[string]int)            // Counts for generating unique names
	var declaredPos = make(map[*types.Var]token.Pos) // Track declaration positions
	// A block-scoped `const` that shadows an enclosing var/param/const must be renamed like a
	// shadowing var (C# has no block shadowing — CS0136), but a const's object is a *types.Const, not
	// the *types.Var the scope stack / varNames track. constShadowNames maps such a renamed const
	// object to its adjusted name so both its declaration and its uses (resolved via info.Uses to the
	// same *types.Const) emit the renamed form. Only shadowing consts are recorded; a non-shadowing
	// const is left unchanged (no churn).
	constShadowNames := make(map[types.Object]string)

	// Imported PACKAGE names referenced in THIS function (`bits.LeadingZeros16(…)`). A local
	// variable sharing such a name is fine in Go — a reference before the local's declaration
	// point still resolves to the package — but C# scoping makes the local own the simple name
	// for the WHOLE block, so the earlier package reference binds to the not-yet-declared local
	// (internal/zstd fse's `bits := tableBits - highBit` after `bits.LeadingZeros16`, CS0841).
	// declareVar shadow-renames such locals.
	usedPackageNames := HashSet[string]{}

	// PACKAGE-LEVEL vars referenced in THIS function. A local sharing such a name is fine in
	// Go — a reference before the local's declaration point (including the local's OWN
	// initializer: `if hosts, ok := hosts.byAddr[addr]; ok`, net hosts.go) still resolves to
	// the package var — but C#'s whole-block scoping binds the earlier reference to the
	// not-yet-declared local (CS0841/CS8130). declareVar shadow-renames such locals; the
	// package var keeps the simple name.
	usedPackageVarNames := HashSet[string]{}

	// Every variable DECLARED anywhere in this function — receiver, parameters, results and locals
	// at every nesting depth (func literals included). The emitter spells bare package-level TYPE
	// names on its own (the `Type.Ꮡfield` box accessor); C# binds such a bare name to a same-named
	// variable in scope, so boxAccessorType consults this set to decide when the type name must be
	// package-qualified. Collected here rather than from the scope stack because the shadowing
	// variable need not be anywhere near the reference — poly1305's `func (h *MAC) Sum(b []byte)`
	// declares `var mac [TagSize]byte` and the promoted-embed hop for `h.mac.Sum(&mac)` spells
	// `mac.ᏑmacGeneric`, which binds to that `array<byte>` local (CS1061).
	v.funcScopeVarNames = HashSet[string]{}

	ast.Inspect(funcDecl, func(n ast.Node) bool {
		if ident, ok := n.(*ast.Ident); ok {
			if _, isPkg := v.info.Uses[ident].(*types.PkgName); isPkg {
				usedPackageNames.Add(ident.Name)
			}

			if varObj, isVar := v.info.Uses[ident].(*types.Var); isVar {
				if globalObj, isGlobal := v.globalScope[ident.Name]; isGlobal && globalObj == varObj {
					usedPackageVarNames.Add(ident.Name)
				}
			}

			// A struct type declared INSIDE the function contributes field objects here; a field is
			// struct-scoped and never binds a bare name, so only true variables are recorded.
			if varObj, isVar := v.info.Defs[ident].(*types.Var); isVar && !varObj.IsField() {
				v.funcScopeVarNames.Add(ident.Name)
			}
		}

		return true
	})

	// Initialize tracker registry for different statement types
	registry := newTrackerRegistry()

	// Initialize local variables names from globals
	for varName, obj := range v.globalScope {
		v.varNames[obj] = varName
	}

	// Helper function to determine if a node is directly in the function body
	// (not in a nested block or control structure)
	isFunctionLevelNode := func(node ast.Node, funcBody *ast.BlockStmt) bool {
		for _, stmt := range funcBody.List {
			if stmt == node {
				return true
			}
		}
		return false
	}

	// First pass: collect all function-level variable declarations
	collectFunctionLevelDecls := func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.FuncLit:
			// Skip the body of function literals (they have their own scope)
			return false

		case *ast.DeclStmt:
			// Only a `var` declaration that is directly in the function body is a function-level
			// declaration. Without this gate a NESTED `var z` would be recorded as the package's
			// function-level `z`, masking the real one (a later `z := …`) — then neither gets the
			// shadow rename and both emit `z`, colliding in C# (CS0136). Mirrors the AssignStmt etc.
			if isFunctionLevelNode(n, funcDecl.Body) {
				if genDecl, ok := n.Decl.(*ast.GenDecl); ok {
					for _, spec := range genDecl.Specs {
						if valueSpec, ok := spec.(*ast.ValueSpec); ok {
							for _, ident := range valueSpec.Names {
								if obj := v.info.Defs[ident]; obj != nil {
									if varObj, ok := obj.(*types.Var); ok {
										if _, exists := functionLevelDecls[ident.Name]; !exists {
											functionLevelDecls[ident.Name] = varObj
											declaredPos[varObj] = ident.Pos()
										}
									}
								}
							}
						}
					}
				}
			}

		case *ast.AssignStmt:
			if n.Tok == token.DEFINE {
				// Check if this assignment is directly in the function body
				// (not in a nested block or control structure)
				if isFunctionLevelNode(n, funcDecl.Body) {
					for _, expr := range n.Lhs {
						if ident, ok := expr.(*ast.Ident); ok {
							if obj := v.info.Defs[ident]; obj != nil {
								if varObj, ok := obj.(*types.Var); ok {
									// Only add if not already declared
									if _, exists := functionLevelDecls[ident.Name]; !exists {
										functionLevelDecls[ident.Name] = varObj
										declaredPos[varObj] = ident.Pos()
									}
								}
							}
						}
					}
				}
			}

		case *ast.RangeStmt:
			if isFunctionLevelNode(n, funcDecl.Body) && n.Tok == token.ASSIGN {
				// Handle Key
				if key, ok := n.Key.(*ast.Ident); ok {
					if obj := v.info.Uses[key]; obj != nil {
						if varObj, ok := obj.(*types.Var); ok {
							// Only add if not already declared
							if _, exists := functionLevelDecls[key.Name]; !exists {
								functionLevelDecls[key.Name] = varObj
								declaredPos[varObj] = key.Pos()
							}
						}
					}
				}
				// Handle Value
				if value, ok := n.Value.(*ast.Ident); ok && value != nil {
					if obj := v.info.Uses[value]; obj != nil {
						if varObj, ok := obj.(*types.Var); ok {
							// Only add if not already declared
							if _, exists := functionLevelDecls[value.Name]; !exists {
								functionLevelDecls[value.Name] = varObj
								declaredPos[varObj] = value.Pos()
							}
						}
					}
				}
			}

		// NOTE: a `for init; …` loop's `:=` variable is scoped to the for statement, NOT to the
		// function body — so it is deliberately NOT collected here. Recording it as the package's
		// function-level decl (it is collected first, in source order) would mask the REAL
		// function-level variable of the same name declared later (`for b := …{} … b := newBucket()`),
		// defeating the shadow-rename for both → C# CS0136. The for-loop var's own shadowing of an
		// enclosing variable is handled by the scope-stack pass (isDeclaredInOuterScopes). Same intent
		// as the DeclStmt gate above. (Range `:=` vars are likewise not collected — the RangeStmt case
		// only handles the `=` reuse of already-declared variables.)

		case *ast.TypeSwitchStmt:
			if isFunctionLevelNode(n, funcDecl.Body) {
				if assign, ok := n.Assign.(*ast.AssignStmt); ok && assign.Tok == token.DEFINE {
					for _, expr := range assign.Lhs {
						if ident, ok := expr.(*ast.Ident); ok {
							if obj := v.info.Defs[ident]; obj != nil {
								if varObj, ok := obj.(*types.Var); ok {
									// Only add if not already declared
									if _, exists := functionLevelDecls[ident.Name]; !exists {
										functionLevelDecls[ident.Name] = varObj
										declaredPos[varObj] = ident.Pos()
									}
								}
							}
						}
					}
				}
			}
		}

		return true
	}

	// Collect all function-level declarations
	if funcDecl.Body != nil {
		ast.Inspect(funcDecl.Body, collectFunctionLevelDecls)
	}

	// Second pass: a `for i := …` (or range) loop var that ESCAPES to the heap is emitted as a
	// `ref var i = ref heap<…>(out var Ꮡi)` declaration hoisted into the ENCLOSING block — so in
	// C# it is block-scoped, not loop-scoped. Within one container (function body, block, or
	// switch/select clause), at most ONE hoisted box per name can exist: the FIRST escaped loop
	// var keeps the name; every OTHER direct-child loop var with the same name in the same
	// container is force-shadow-renamed. An escaped sibling would otherwise duplicate the hoisted
	// box decl (CS0128 — runtime `typesEqual`'s tin/tout `for i` pair inside a switch case), and a
	// non-escaping sibling nests inside the block that now owns the box name (CS0136 — runtime
	// `runqputslow` has three `for i := …` loops, the last of which escapes). A function-body-level
	// keeper is additionally recorded as a function-level decl (unless a real one exists, which is
	// never masked) so non-loop uses elsewhere in the function shadow-rename as before. A name
	// group with NO escaped var is untouched (loop-scoped in C# too — no churn).
	forcedShadowVars := make(map[*types.Var]bool)

	if funcDecl.Body != nil {
		// A range over a slice/array/map (or pointer-to-array) boxes its escaped var PER ITERATION
		// inside the loop body (Go 1.22 per-iteration semantics — see visitRangeStmt's
		// deferRangeVarBox), so it never claims a name in the enclosing container; a string/int/
		// chan/func range writes the box decl before the loop, which does.
		rangeBoxHoists := func(n *ast.RangeStmt) bool {
			if t := v.info.TypeOf(n.X); t != nil {
				switch u := t.Underlying().(type) {
				case *types.Basic:
					return u.Info()&(types.IsString|types.IsInteger) != 0
				case *types.Chan, *types.Signature:
					_ = u
					return true
				}

				return false
			}

			return true
		}

		type loopVarIdent struct {
			ident *ast.Ident
			// hoists: this var's heap box (if any) is emitted BEFORE the loop, claiming its
			// name in the enclosing container. A boxed `for i := …` clause var now boxes PER
			// ITERATION inside the body (forClausePerIterVars — Go 1.22 per-iteration
			// semantics), so it claims nothing; only the legacy clause-func-literal fallback
			// still hoists its box. Range vars keep the rangeBoxHoists split.
			hoists bool
		}

		loopVarIdents := func(stmt ast.Stmt) []loopVarIdent {
			// A labeled loop (`Label: for i := …`) hoists exactly like an unlabeled one.
			if labeled, ok := stmt.(*ast.LabeledStmt); ok {
				stmt = labeled.Stmt
			}

			var loopVars []loopVarIdent

			switch n := stmt.(type) {
			case *ast.ForStmt:
				if assign, ok := n.Init.(*ast.AssignStmt); ok && assign.Tok == token.DEFINE {
					for _, lhs := range assign.Lhs {
						if ident, ok := lhs.(*ast.Ident); ok {
							obj := v.info.Defs[ident]
							hoists := v.funcLitReferences(n.Init, obj) || v.funcLitReferences(n.Cond, obj) || v.funcLitReferences(n.Post, obj)
							loopVars = append(loopVars, loopVarIdent{ident, hoists})
						}
					}
				}
			case *ast.RangeStmt:
				if n.Tok == token.DEFINE {
					hoists := rangeBoxHoists(n)

					if key, ok := n.Key.(*ast.Ident); ok {
						loopVars = append(loopVars, loopVarIdent{key, hoists})
					}
					if value, ok := n.Value.(*ast.Ident); ok {
						loopVars = append(loopVars, loopVarIdent{value, hoists})
					}
				}
			}

			return loopVars
		}

		processContainer := func(stmts []ast.Stmt, isFuncBody bool) {
			type loopVarEntry struct {
				varObj *types.Var
				pos    token.Pos
				claims bool
			}

			groups := make(map[string][]loopVarEntry)
			var groupOrder []string

			for _, stmt := range stmts {
				for _, loopVar := range loopVarIdents(stmt) {
					ident := loopVar.ident

					if isDiscardedVar(ident.Name) {
						continue
					}

					varObj, ok := v.info.Defs[ident].(*types.Var)

					if !ok {
						continue
					}

					// The var claims a container-level name only when a heap box decl is actually
					// emitted for it AND hoisted before the loop. Mirrors convertToHeapTypeDecl's
					// gate: an inherently heap-allocated var (pointer/slice/map/chan/interface/
					// func) is already a reference and gets no box. (The lambda box-ref exception
					// there is invisible to this pre-pass — boxRefVars populates during the main
					// walk — so that exotic keeper shape is not grouped; it was never handled
					// before either.)
					claims := loopVar.hoists && v.identEscapesHeap[varObj] &&
						!isInherentlyHeapAllocatedType(v.info.TypeOf(ident))

					if len(groups[ident.Name]) == 0 {
						groupOrder = append(groupOrder, ident.Name)
					}

					groups[ident.Name] = append(groups[ident.Name], loopVarEntry{varObj, ident.Pos(), claims})
				}
			}

			for _, name := range groupOrder {
				entries := groups[name]
				keeper := -1

				for i, entry := range entries {
					if entry.claims {
						keeper = i
						break
					}
				}

				if keeper < 0 {
					continue
				}

				for i, entry := range entries {
					if i != keeper {
						forcedShadowVars[entry.varObj] = true
					}
				}

				if isFuncBody {
					if _, exists := functionLevelDecls[name]; !exists {
						functionLevelDecls[name] = entries[keeper].varObj
						declaredPos[entries[keeper].varObj] = entries[keeper].pos
					}
				}
			}
		}

		ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
			switch n := node.(type) {
			case *ast.BlockStmt:
				processContainer(n.List, n == funcDecl.Body)
			case *ast.CaseClause:
				processContainer(n.Body, false)
			case *ast.CommClause:
				processContainer(n.Body, false)
			}

			return true
		})
	}

	// Helper functions
	isDeclaredInCurrentScope := func(varName string) bool {
		scope := v.scopeStack[len(v.scopeStack)-1]
		_, exists := scope[varName]
		return exists
	}

	isDeclaredInOuterScopes := func(varName string, includeGlobal bool) bool {
		minScope := 1
		if includeGlobal {
			minScope = 0
		}
		for i := len(v.scopeStack) - 2; i >= minScope; i-- {
			if _, exists := v.scopeStack[i][varName]; exists {
				return true
			}
		}
		return false
	}

	// isForwardDeclaredInOuterBlocks reports whether varName is declared directly in any ENCLOSING
	// block scope (every level except the current innermost at len-1), consulting the pre-scanned
	// full name set so a declaration appearing LATER in that block is visible too. This complements
	// isDeclaredInOuterScopes, which only sees declarations already pushed onto the scope stack
	// (i.e. backward, in source order). Together they implement C#'s order-independent CS0136 rule
	// across intermediate blocks, not just the function body. The innermost scope is excluded so a
	// variable declared directly in the current block does not appear to shadow itself.
	isForwardDeclaredInOuterBlocks := func(varName string) bool {
		for i := len(blockDecls) - 2; i >= 1; i-- {
			if names := blockDecls[i]; names != nil && names[varName] {
				return true
			}
		}
		return false
	}

	lookupVar := func(varName string) *types.Var {
		// Check the scope stack first, innermost outward: a variable declared in a nested block
		// shadows a function-level declaration of the same name, so the inner (shadow-renamed)
		// object must win — otherwise a reassignment like `x += 1` inside the block would resolve
		// to the outer variable's name.
		for i := len(v.scopeStack) - 1; i >= 0; i-- {
			if varObj, exists := v.scopeStack[i][varName]; exists {
				return varObj
			}
		}
		// Fall back to the function-level declaration.
		if varObj, exists := functionLevelDecls[varName]; exists {
			return varObj
		}
		return nil
	}

	getShadowedVarName := func(varName string) string {
		nameCounts[varName]++
		return fmt.Sprintf("%s%s%d", varName, ShadowVarMarker, nameCounts[varName])
	}

	// Declare variable, checking if an identifier needs shadowing
	declareVar := func(varName string, varObj *types.Var, ident *ast.Ident) {
		var adjustedName string

		// First check if this variable name conflicts with a function name
		if v.isFunctionNameInScope(varName) {
			adjustedName = getShadowedVarName(varName)
		} else if v.shadowsCalledBuiltin(varName) {
			// A local named like a Go built-in (e.g. `len := len(buf)` in hash/maphash)
			// is fine in Go — the built-in call still resolves to the predeclared function.
			// But in C# the built-in is a `using static go.builtin` method, so a same-named
			// local shadows it and the call `len(...)` binds to the (non-invocable) local
			// (CS0149 / CS0841). Rename the local so the built-in call stays valid.
			adjustedName = getShadowedVarName(varName)
		} else if usedPackageNames.Contains(varName) {
			// A local sharing an IMPORTED PACKAGE name this function references: C#'s
			// whole-block scoping binds the package reference to the local even before its
			// declaration (CS0841/CS0119) — rename the local, the package alias stays.
			adjustedName = getShadowedVarName(varName)
		} else if usedPackageVarNames.Contains(varName) {
			// A local sharing a referenced PACKAGE-LEVEL var's name (pre-scan above):
			// rename the local so the package var's references keep resolving (CS0841).
			adjustedName = getShadowedVarName(varName)
		} else if forcedShadowVars[varObj] {
			// A loop var sharing its container's hoisted-box name (second pass above) — an escaped
			// or nested sibling of the keeper. Always renamed, regardless of scope-stack state.
			adjustedName = getShadowedVarName(varName)
		} else if funcLevelVar, exists := functionLevelDecls[varName]; exists {
			needsShadowing := false

			// Check if we're in any tracked statement that requires shadowing
			for _, tracker := range registry.trackers {
				if tracker.processing {
					// Only shadow if the variable is already declared in an outer scope
					if isDeclaredInOuterScopes(varName, true) {
						needsShadowing = true
						break
					}
				}
			}

			// If not in a tracked statement, fall back to position-based logic
			if !needsShadowing {
				needsShadowing = declaredPos[funcLevelVar] > ident.Pos()
			}

			// A name declared LATER in an ENCLOSING block still collides — C#'s CS0136 is
			// order-independent. mime mediatype.go: the nested `v, ok :=` (207) preceded a
			// for-body-level `v, ok :=` (213); the position fallback saw no shadow and both
			// stayed plain (nested + enclosing 'ok', CS0136). The final else-if arm already
			// consults the forward set; this arm preempted it for function-level names.
			if !needsShadowing {
				needsShadowing = isForwardDeclaredInOuterBlocks(varName)
			}

			if needsShadowing {
				adjustedName = getShadowedVarName(varName)
			} else {
				adjustedName = varName
			}
		} else if isDeclaredInOuterScopes(varName, false) || isForwardDeclaredInOuterBlocks(varName) {
			// The name collides with a variable in an enclosing block — either one already seen
			// (isDeclaredInOuterScopes, backward) or one declared later in that block
			// (isForwardDeclaredInOuterBlocks, forward). C# CS0136 forbids both regardless of order.
			adjustedName = getShadowedVarName(varName)
		} else {
			adjustedName = varName
			nameCounts[varName] = 0
		}

		v.varNames[varObj] = adjustedName
		v.identNames[ident] = adjustedName
		v.isReassigned[ident] = false

		scope := v.scopeStack[len(v.scopeStack)-1]
		scope[varName] = varObj
	}

	reassignVar := func(varName string, ident *ast.Ident) {
		varObj := lookupVar(varName)
		if varObj == nil {
			return
		}
		adjustedName := v.varNames[varObj]
		v.identNames[ident] = getSanitizedIdentifier(adjustedName)
		v.isReassigned[ident] = true
	}

	addFunctionParams := func(funcDecl *ast.FuncDecl, signature *types.Signature) {
		// Add all function parameters to the current scope
		parameters := getParameters(signature, false)
		paramIndex := 0

		if len(funcDecl.Type.Params.List) > 0 {
			for _, field := range funcDecl.Type.Params.List {
				names := field.Names

				if len(names) == 0 {
					// Anonymous parameter (no name)
					paramIndex++
				} else {
					for _, ident := range names {
						name := ident.Name

						if isDiscardedVar(name) {
							paramIndex++
							continue
						}

						param := parameters.At(paramIndex)
						declareVar(name, param, ident)
						paramIndex++
					}
				}
			}
		}

		// Handle receiver if present
		if signature.Recv() != nil && funcDecl.Recv != nil && len(funcDecl.Recv.List) > 0 {
			field := funcDecl.Recv.List[0]
			names := field.Names

			if len(names) > 0 {
				ident := names[0]
				recv := signature.Recv()
				recvName := ident.Name

				if !isDiscardedVar(recvName) {
					declareVar(recvName, recv, ident)
				}
			}
		}

		// Add named result parameters to the current scope
		results := signature.Results()
		resultIndex := 0

		if funcDecl.Type.Results != nil && len(funcDecl.Type.Results.List) > 0 {
			for _, field := range funcDecl.Type.Results.List {
				names := field.Names

				if len(names) == 0 {
					// Anonymous result (no name)
					resultIndex++
				} else {
					for _, ident := range names {
						name := ident.Name

						if isDiscardedVar(name) {
							resultIndex++
							continue
						}

						result := results.At(resultIndex)
						declareVar(name, result, ident)
						resultIndex++
					}
				}
			}
		}
	}

	// collectBlockLevelDecls returns the names of variables declared *directly* in the given block
	// statement list: top-level `:=` short declarations and `var` declarations. Names declared in
	// nested statements — including a control statement's own init `:=`, which is scoped to that
	// statement rather than the block — are excluded, mirroring collectFunctionLevelDecls'
	// direct-child gate. The result is order-independent, so forward (later-in-source) declarations
	// are captured as well as backward ones.
	collectBlockLevelDecls := func(list []ast.Stmt) map[string]bool {
		names := make(map[string]bool)

		for _, stmt := range list {
			switch n := stmt.(type) {
			case *ast.DeclStmt:
				if genDecl, ok := n.Decl.(*ast.GenDecl); ok {
					for _, spec := range genDecl.Specs {
						if valueSpec, ok := spec.(*ast.ValueSpec); ok {
							for _, ident := range valueSpec.Names {
								if !isDiscardedVar(ident.Name) {
									names[ident.Name] = true
								}
							}
						}
					}
				}

			case *ast.AssignStmt:
				if n.Tok == token.DEFINE {
					for _, expr := range n.Lhs {
						if ident, ok := expr.(*ast.Ident); ok && !isDiscardedVar(ident.Name) {
							names[ident.Name] = true
						}
					}
				}
			}
		}

		return names
	}

	// pushScope/popScope keep v.scopeStack and blockDecls index-aligned. Pass the block's
	// pre-scanned forward-decl set (from collectBlockLevelDecls) for a genuine block scope; pass nil
	// for non-block scopes (control-statement init scopes and parameter scopes).
	pushScope := func(forward map[string]bool) {
		v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
		blockDecls = append(blockDecls, forward)
	}

	popScope := func() {
		v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]
		blockDecls = blockDecls[:len(blockDecls)-1]
	}

	var visitNode func(n ast.Node)
	initialBlock := true

	visitNode = func(n ast.Node) {
		switch node := n.(type) {
		case *ast.BlockStmt:
			if node == nil {
				return
			}

			// Enter a new scope, pre-scanning this block's direct declarations so a nested variable
			// can detect a same-named variable declared later in this block (forward shadowing).
			pushScope(collectBlockLevelDecls(node.List))

			if initialBlock {
				initialBlock = false
				// Add function parameters (including receiver and results) to the current scope
				addFunctionParams(funcDecl, signature)
			} else {
				// Only track non-initial blocks
				blockTracker := registry.get(BlockTracker)
				blockTracker.enter()
				blockTracker.processing = true
			}

			for _, stmt := range node.List {
				visitNode(stmt)
			}

			if !initialBlock {
				// Only cleanup tracking for non-initial blocks. `processing` is a single flag
				// shared by every nesting level of the one BlockTracker, so an inner block's
				// cleanup must RESTORE it for a still-open enclosing block rather than clear it —
				// clearing it made a declaration that FOLLOWED a nested block (procresize's
				// second `trace := traceAcquire()` after the inner if) skip the outer-scope
				// shadow check and keep its name (CS0136 against the function-level decl).
				blockTracker := registry.get(BlockTracker)
				blockTracker.exit()
				blockTracker.processing = blockTracker.level > 0
			}

			// Exit the current scope
			popScope()

		case *ast.AssignStmt:
			if node.Tok == token.DEFINE {
				// Short variable declaration ':='
				for _, lhs := range node.Lhs {
					ident := getIdentifier(lhs)

					if ident == nil {
						continue
					}

					varName := ident.Name

					if isDiscardedVar(varName) {
						continue
					}

					if isDeclaredInCurrentScope(varName) {
						reassignVar(varName, ident)
					} else {
						// New variable declaration
						obj := v.info.Defs[ident]

						if obj == nil {
							continue
						}

						varObj := obj.(*types.Var)
						declareVar(varName, varObj, ident)
					}
				}
			} else {
				// Regular assignment '='
				for _, lhs := range node.Lhs {
					// Reassign the ROOT ident (`batch`/`m`/`p`). getIdentifier returns nil for a
					// PAREN-rooted target — `(*p)[i]`, `(arr)[i]` (Go parenthesizes because `*p[i]` parses
					// as `*(p[i])`). There is no simple root to reassign there, but the index
					// sub-expressions below STILL need their shadow-renamed idents rewritten, so do NOT bail
					// on a nil root — only skip the reassign.
					if ident := getIdentifier(lhs); ident != nil && !isDiscardedVar(ident.Name) {
						reassignVar(ident.Name, ident)
					}

					// Rename shadowed identifiers used inside the target's INDEX/KEY sub-expressions —
					// `batch[i] = …`, `m[ns] = …`, `p.f[k] = …`. getIdentifier/reassignVar only handle the
					// ROOT ident (batch/m/p); an inner index/key var was never visited, so a shadow-renamed
					// one kept its raw name and resolved to the wrong (enclosing) variable — a silent wrong
					// value, or CS0136/CS0165 once the index var itself is renamed (e.g. a renamed sibling
					// loop var `iΔ1` whose `batch[i]` LHS index stayed `i`). Descend the base chain to the
					// root, visiting every index expression so its idents get the rename.
					for cur := lhs; ; {
						switch e := cur.(type) {
						case *ast.IndexExpr:
							visitNode(e.Index)
							cur = e.X
							continue
						case *ast.IndexListExpr:
							for _, idx := range e.Indices {
								visitNode(idx)
							}
							cur = e.X
							continue
						case *ast.SelectorExpr:
							cur = e.X
							continue
						case *ast.StarExpr:
							cur = e.X
							continue
						case *ast.ParenExpr:
							cur = e.X
							continue
						case *ast.CallExpr:
							// A method-call receiver buried in the LHS chain — `x.ptr().Value.next = …`
							// (runtime stackpoolalloc's `x := gclinkptr(…)` loop, where `x` is shadow-renamed
							// `xΔ1`). The `x` inside `x.ptr()` is past the selector/index descent above; visit
							// the whole call so its receiver and argument idents get the rename — else the use
							// keeps the raw name and, being declared later, is CS0841 (or a silent wrong bind).
							// The call's RESULT is the navigated base, so the descent stops here.
							visitNode(cur)
						case *ast.TypeAssertExpr:
							// A TYPE ASSERTION in the LHS chain — `n.Values[0].(*ast.CompositeLit).Type.
							// (*ast.ArrayType).Len = nil` (go/types generate_test.go's fixGlobalTypVarDecl).
							// The assertion is a navigation step like the selector and index cases above, but
							// it was in NO arm, so the descent stopped dead at it: everything below — the
							// index sub-expressions AND the chain's root ident — went unvisited and kept its
							// raw name. `getIdentifier` does not see through an assertion either, so the root
							// gets no reassignVar mapping to fall back on, which is what makes this shape
							// distinct from the paren-rooted target noted above. Visit the OPERAND subtree
							// whole (the CallExpr treatment): it carries every ident that needs the rename,
							// while the asserted TYPE carries none. Without it, a shadow-renamed root emits
							// the ENCLOSING variable's name — `(~n).Values[0]…` against the outer `ast.Node`
							// parameter rather than the case variable `nΔ1` (CS0023, and a silent wrong bind
							// wherever the outer name happens to type-check).
							visitNode(e.X)
						}

						break
					}
				}
			}

			// Visit RHS expressions
			for _, rhs := range node.Rhs {
				visitNode(rhs)
			}

		case *ast.ValueSpec:
			// Variable declaration using 'var'
			for _, ident := range node.Names {
				varName := ident.Name

				if isDiscardedVar(varName) {
					continue
				}

				obj := v.info.Defs[ident]

				if obj == nil {
					continue
				}

				if varObj, ok := obj.(*types.Var); ok {
					declareVar(varName, varObj, ident)
				} else if _, ok := obj.(*types.Const); ok {
					// A block `const` that shadows an enclosing var/param — `const ns = 10e6` inside a
					// function that has a parameter `ns` (runtime notetsleep_internal). C# forbids the
					// shadow (CS0136); the shadow check is by NAME, so the enclosing `ns` is detected even
					// though it lives in the *types.Var scope stack. Rename the const decl + its uses.
					if isDeclaredInOuterScopes(varName, false) || isForwardDeclaredInOuterBlocks(varName) {
						adjustedName := getShadowedVarName(varName)
						constShadowNames[obj] = adjustedName
						v.identNames[ident] = adjustedName
					}
				}
			}

			// Visit values
			for _, value := range node.Values {
				visitNode(value)
			}

		case *ast.Ident:
			if obj := v.info.Uses[node]; obj != nil {
				if varObj, ok := obj.(*types.Var); ok {
					if adjustedName, ok := v.varNames[varObj]; ok {
						v.identNames[node] = adjustedName
					}
				} else if adjustedName, ok := constShadowNames[obj]; ok {
					// A use of a shadow-renamed block const resolves to the same *types.Const.
					v.identNames[node] = adjustedName
				}
			}

		case *ast.FuncLit:
			// The parameters live in the BODY's scope (Go: parameters are declared in the
			// function block), so a body-level `fpath, err := ...` REUSES the parameter err.
			// A separate param scope made that a shadow DECLARATION - `var (fpath, err)`
			// beside later reuses (CS0841/CS0128 x5, os CopyFS's WalkDir literal). Push ONE
			// scope holding params + body declarations, and visit the body's statements
			// directly so no second block scope intervenes.
			pushScope(collectBlockLevelDecls(node.Body.List))
			tracker := registry.get(FuncTracker)
			tracker.enter()
			tracker.processing = true

			// Enter lambda capture analysis context
			v.lambdaCapture.analysisInLambda = true
			v.lambdaCapture.currentLambda = node

			// Get funcDecl for the function literal
			funcDecl := &ast.FuncDecl{Type: node.Type}

			// Get signature for the function literal
			signature := v.info.TypeOf(funcDecl.Type).(*types.Signature)

			// Add function parameters (including receiver and results) to the current scope
			addFunctionParams(funcDecl, signature)

			// First pass: analyze the function body for captured variables
			ast.Inspect(node.Body, func(n ast.Node) bool {
				if id, ok := n.(*ast.Ident); ok {
					v.processPotentialCapture(id)
				}
				return true
			})

			tracker.processing = false

			// Visit the body STATEMENTS directly (the merged param+body scope above is the
			// function block; visitNode(node.Body) would push a second, param-splitting scope).
			blockTracker := registry.get(BlockTracker)
			blockTracker.enter()
			blockTracker.processing = true

			for _, stmt := range node.Body.List {
				visitNode(stmt)
			}

			blockTracker.exit()
			blockTracker.processing = blockTracker.level > 0

			// Exit lambda capture analysis context
			v.lambdaCapture.analysisInLambda = false
			v.lambdaCapture.currentLambda = nil

			// Exit the function literal scope
			tracker.exit()
			popScope()

		case *ast.GoStmt:
			// Enter capture analysis context for goroutine
			v.lambdaCapture.analysisInLambda = true
			v.lambdaCapture.currentLambda = node

			// Create capture set for this statement
			v.lambdaCapture.stmtCaptures[node] = make(map[*ast.Ident]bool)

			// Process function expression. Skip the receiver of a capture-mode method value on a
			// boxed/addressed receiver (`defer locked.Store(0)`): its box is used directly, not a copy.
			skipRecv := v.captureModeMethodValueReceiver(node.Call)
			ast.Inspect(node.Call.Fun, func(n ast.Node) bool {
				if id, ok := n.(*ast.Ident); ok {
					if id == skipRecv {
						return true
					}
					v.processPotentialCapture(id)
					// If it was captured, associate it with this statement
					if _, exists := v.lambdaCapture.capturedVars[id]; exists {
						v.lambdaCapture.stmtCaptures[node][id] = true
					}
				}
				return true
			})

			// Note: parameters are passed as arguments to C# implementation of go
			// statement, so we don't need to check arguments for needing capture:

			// // Process arguments
			// for _, arg := range node.Call.Args {
			// 	ast.Inspect(arg, func(n ast.Node) bool {
			// 		if id, ok := n.(*ast.Ident); ok {
			// 			v.processPotentialCapture(id)
			// 			// If it was captured, associate it with this statement
			// 			if _, exists := v.lambdaCapture.capturedVars[id]; exists {
			// 				v.lambdaCapture.stmtCaptures[node][id] = true
			// 			}
			// 		}
			// 		return true
			// 	})
			// }

			visitNode(node.Call)

			// Exit capture analysis context
			v.lambdaCapture.analysisInLambda = false
			v.lambdaCapture.currentLambda = nil

		// Check for defer and recover calls
		case *ast.DeferStmt:
			v.hasDefer = true

			// Enter capture analysis context for goroutine
			v.lambdaCapture.analysisInLambda = true
			v.lambdaCapture.currentLambda = node

			// Create capture set for this statement
			v.lambdaCapture.stmtCaptures[node] = make(map[*ast.Ident]bool)

			// Process function expression. Skip the receiver of a capture-mode method value on a
			// boxed/addressed receiver (`defer locked.Store(0)`): its box is used directly, not a copy.
			skipRecv := v.captureModeMethodValueReceiver(node.Call)
			ast.Inspect(node.Call.Fun, func(n ast.Node) bool {
				if id, ok := n.(*ast.Ident); ok {
					if id == skipRecv {
						return true
					}
					v.processPotentialCapture(id)
					// If it was captured, associate it with this statement
					if _, exists := v.lambdaCapture.capturedVars[id]; exists {
						v.lambdaCapture.stmtCaptures[node][id] = true
					}
				}
				return true
			})

			// Note: parameters are passed as arguments to C# implementation of defer
			// statement, so we don't need to check arguments for needing capture:

			// // Process arguments
			// for _, arg := range node.Call.Args {
			// 	ast.Inspect(arg, func(n ast.Node) bool {
			// 		if id, ok := n.(*ast.Ident); ok {
			// 			v.processPotentialCapture(id)
			// 			// If it was captured, associate it with this statement
			// 			if _, exists := v.lambdaCapture.capturedVars[id]; exists {
			// 				v.lambdaCapture.stmtCaptures[node][id] = true
			// 			}
			// 		}
			// 		return true
			// 	})
			// }

			visitNode(node.Call)

			// Exit capture analysis context
			v.lambdaCapture.analysisInLambda = false
			v.lambdaCapture.currentLambda = nil

		case *ast.IfStmt:
			// Init scope (the body's own block scope is pushed when visitNode descends into it).
			pushScope(nil)
			tracker := registry.get(IfTracker)
			tracker.enter()
			tracker.processing = true

			if node.Init != nil {
				processor := newVarProcessor(
					tracker,
					functionLevelDecls,
					declaredPos,
					v.info,
					declareVar,
					reassignVar,
				)

				switch init := node.Init.(type) {
				case *ast.AssignStmt:
					processor.processAssignStmt(init)
				case *ast.IncDecStmt:
					// Process increment/decrement expressions
					if ident, ok := init.X.(*ast.Ident); ok {
						// This is always a reassignment, never a declaration
						processor.processIdent(ident, false)
					}
				}
			}

			tracker.processing = false

			if node.Init != nil {
				// Visit ALL of the init statement's RHS expressions, not just call expressions.
				// Two things depend on the full sweep: shadow renames must reach idents inside
				// any shape (a comma-ok type assert on a renamed variable,
				// `if e, ok := err.(*strconv.NumError); ok`, leaves `err` unrenamed if the
				// assert is skipped — fmt convertFloat, CS0841/CS8130 x6), and the
				// recover-detection walk needs the call expressions among them.
				if assign, ok := node.Init.(*ast.AssignStmt); ok {
					for _, rhs := range assign.Rhs {
						visitNode(rhs)
					}
				}
			}

			// Visit condition
			if node.Cond != nil {
				visitNode(node.Cond)
			}

			// Visit body
			visitNode(node.Body)

			// Visit else
			if node.Else != nil {
				visitNode(node.Else)
			}

			tracker.exit()
			popScope()

		case *ast.SwitchStmt:
			// Init scope (case-clause bodies push their own block scopes below).
			pushScope(nil)
			tracker := registry.get(SwitchTracker)
			tracker.enter()
			tracker.processing = true

			if node.Init != nil {
				processor := newVarProcessor(
					tracker,
					functionLevelDecls,
					declaredPos,
					v.info,
					declareVar,
					reassignVar,
				)

				switch init := node.Init.(type) {
				case *ast.AssignStmt:
					processor.processAssignStmt(init)
				case *ast.IncDecStmt:
					if ident, ok := init.X.(*ast.Ident); ok {
						processor.processIdent(ident, false) // Treat as reassignment
					}
				}
			}

			tracker.processing = false

			// Visit the init statement's RHS expressions (shadow renames must reach idents
			// inside them — mirrors the IfStmt init walk above)
			if node.Init != nil {
				if assign, ok := node.Init.(*ast.AssignStmt); ok {
					for _, rhs := range assign.Rhs {
						visitNode(rhs)
					}
				}
			}

			// Visit tag and body
			if node.Tag != nil {
				visitNode(node.Tag)
			}

			visitNode(node.Body)

			for _, stmt := range node.Body.List {
				if caseClause, ok := stmt.(*ast.CaseClause); ok {
					// Enter a new scope; a case body is an implicit block, so pre-scan its direct
					// declarations for forward shadow detection.
					pushScope(collectBlockLevelDecls(caseClause.Body))

					// Visit the case body treating it as an implicit block
					blockTracker := registry.get(BlockTracker)
					blockTracker.enter()
					blockTracker.processing = true

					for _, stmt := range caseClause.Body {
						visitNode(stmt)
					}

					blockTracker.exit()
					blockTracker.processing = blockTracker.level > 0

					// Exit the current scope
					popScope()

					// Ensure case clause identifiers in expressions are checked for shadowing
					for _, expr := range caseClause.List {
						ast.Inspect(expr, func(n ast.Node) bool {
							if ident, ok := n.(*ast.Ident); ok {
								if obj := v.info.Uses[ident]; obj != nil {
									if varObj, ok := obj.(*types.Var); ok {
										if adjustedName, ok := v.varNames[varObj]; ok {
											v.identNames[ident] = adjustedName
										}
									}
								}
							}
							return true
						})
					}
				}
			}

			tracker.exit()
			popScope()

		case *ast.TypeSwitchStmt:
			// Init/guard scope (case bodies are handled within node.Body).
			pushScope(nil)
			tracker := registry.get(TypeSwitchTracker)
			tracker.enter()
			tracker.processing = true

			if node.Init != nil {
				processor := newVarProcessor(
					tracker,
					functionLevelDecls,
					declaredPos,
					v.info,
					declareVar,
					reassignVar,
				)

				switch init := node.Init.(type) {
				case *ast.AssignStmt:
					processor.processAssignStmt(init)
				}
			}

			// Handle assign statement (type switch specific)
			if assign, ok := node.Assign.(*ast.AssignStmt); ok {
				processor := newVarProcessor(
					tracker,
					functionLevelDecls,
					declaredPos,
					v.info,
					declareVar,
					reassignVar,
				)
				processor.processAssignStmt(assign)

				// The type-switch guard (`x := x.(type)`) declares an implicit variable per
				// case clause (recorded in info.Implicits, NOT info.Defs), so processAssignStmt
				// above never sees it. If the guard name shadows an enclosing variable (e.g. a
				// parameter), C# `case T x:` collides with it (CS0136). Rename the guard — and
				// every per-case implicit var so body references follow — with the shadow marker.
				if assign.Tok == token.DEFINE && len(assign.Lhs) == 1 {
					if lhsIdent, ok := assign.Lhs[0].(*ast.Ident); ok && !isDiscardedVar(lhsIdent.Name) {
						guardName := lhsIdent.Name

						if v.isFunctionNameInScope(guardName) || v.shadowsCalledBuiltin(guardName) || isDeclaredInOuterScopes(guardName, false) {
							adjustedName := getShadowedVarName(guardName)
							v.identNames[lhsIdent] = adjustedName

							for _, stmt := range node.Body.List {
								if caseClause, ok := stmt.(*ast.CaseClause); ok {
									if implicitObj := v.info.Implicits[caseClause]; implicitObj != nil {
										if implicitVar, ok := implicitObj.(*types.Var); ok {
											v.varNames[implicitVar] = adjustedName
										}
									}
								}
							}
						}
					}
				}
			}

			// Identifier USES anywhere in the guard/tag expression — both the `x.(type)` ExprStmt
			// form and the `t := x.(type)` AssignStmt form — must follow the shadow renames of the
			// variables they reference: the tag can be any expression (`s.Else`, `arrayPtrDeref(t)`),
			// and an unmapped use binds the OUTER same-named variable in the emitted C#. go/types hit
			// this three ways: builtins.go's `switch t := arrayPtrDeref(t).(type)` inside a lambda
			// whose param was renamed tΔ1 emitted `arrayPtrDeref(t)` against the outer case var
			// (CS1503); stmt.go's `s.Else`/`s.Assign` tags dereferenced the outer subject (CS0023);
			// and decl.go's `switch d.(type)` TESTED the wrong variable where it compiled. The
			// guard's own DEFINE ident is not a use (per-case implicits carry it, handled above), so
			// this maps only references. The general walk never descends into node.Assign.
			ast.Inspect(node.Assign, func(n ast.Node) bool {
				if ident, ok := n.(*ast.Ident); ok {
					if obj := v.info.Uses[ident]; obj != nil {
						if varObj, ok := obj.(*types.Var); ok {
							if adjustedName, ok := v.varNames[varObj]; ok {
								v.identNames[ident] = adjustedName
							}
						}
					}
				}
				return true
			})

			tracker.processing = false

			// Each case body is its OWN implicit block: a `sz :=` in one clause must not make
			// a sibling clause's `sz :=` a reassignment — the C# emission braces each case, so
			// the sibling needs its own `var` (CS0103 ×2, poll sockaddrToRaw). The generic
			// visitNode(node.Body) walk gave every clause the SHARED switch-body scope; mirror
			// the plain-SwitchStmt per-case scopes instead.
			var guardIdent *ast.Ident

			if assign, ok := node.Assign.(*ast.AssignStmt); ok && assign.Tok == token.DEFINE && len(assign.Lhs) == 1 {
				if lhsIdent, ok := assign.Lhs[0].(*ast.Ident); ok && !isDiscardedVar(lhsIdent.Name) {
					guardIdent = lhsIdent
				}
			}

			for _, stmt := range node.Body.List {
				if caseClause, ok := stmt.(*ast.CaseClause); ok {
					pushScope(collectBlockLevelDecls(caseClause.Body))

					// The clause implicitly BINDS the guard (C# emits `case T v:` pattern vars /
					// the default arm's binding), so it must live in the clause scope — a nested
					// `v := …` in the body otherwise sees no collision and skips its rename
					// (fmt scanOne's inner `switch v := ptr.Elem(); v.Kind()`, CS0136).
					if guardIdent != nil {
						if implicitVar, ok := v.info.Implicits[caseClause].(*types.Var); ok {
							// Its varNames entry must exist too: reassignVar resolves body
							// references through it, and a missing entry becomes an EMPTY
							// emitted name. The guard-shadow arm above may have already
							// recorded a renamed form — keep it.
							if _, exists := v.varNames[implicitVar]; !exists {
								v.varNames[implicitVar] = guardIdent.Name
							}

							v.scopeStack[len(v.scopeStack)-1][guardIdent.Name] = implicitVar
						}
					}

					blockTracker := registry.get(BlockTracker)
					blockTracker.enter()
					blockTracker.processing = true

					for _, stmt := range caseClause.Body {
						visitNode(stmt)
					}

					blockTracker.exit()
					blockTracker.processing = blockTracker.level > 0

					popScope()

					// Ensure case clause identifiers in expressions are checked for shadowing
					for _, expr := range caseClause.List {
						ast.Inspect(expr, func(n ast.Node) bool {
							if ident, ok := n.(*ast.Ident); ok {
								if obj := v.info.Uses[ident]; obj != nil {
									if varObj, ok := obj.(*types.Var); ok {
										if adjustedName, ok := v.varNames[varObj]; ok {
											v.identNames[ident] = adjustedName
										}
									}
								}
							}
							return true
						})
					}
				}
			}

			tracker.exit()
			popScope()

		case *ast.RangeStmt:
			// Range variable scope (the body's own block scope is pushed when descending into it).
			pushScope(nil)
			tracker := registry.get(RangeTracker)
			tracker.enter()
			tracker.processing = true

			processor := newVarProcessor(
				tracker,
				functionLevelDecls,
				declaredPos,
				v.info,
				declareVar,
				reassignVar,
			)

			// Track the shadowed names of range variables — keyed by the range var's OBJECT, so
			// only genuine uses of that variable rename: any same-named ident with a non-nil Uses
			// entry also matched a SelectorExpr's Sel (bound to a METHOD object), renaming the
			// method call itself — go/types typestring.go's `for _, typ := range t.embeddeds { …
			// w.typ(typ) }` inside `func (w *typeWriter) typ(…)` emitted `Ꮡw.typΔ1(typΔ1)` against
			// the `typ` declaration (CS1061).
			rangeVarObjs := make(map[types.Object]string)

			// Process range variables and remember their shadowed names
			if node.Key != nil {
				if keyIdent := getIdentifier(node.Key); keyIdent != nil {
					processor.processIdent(keyIdent, node.Tok == token.DEFINE)
					// Store any shadowed name that was created
					if shadowName, ok := v.identNames[keyIdent]; ok {
						if obj := v.info.ObjectOf(keyIdent); obj != nil {
							rangeVarObjs[obj] = shadowName
						}
					}
				}
			}

			if node.Value != nil {
				if valueIdent := getIdentifier(node.Value); valueIdent != nil {
					processor.processIdent(valueIdent, node.Tok == token.DEFINE)
					// Store any shadowed name that was created
					if shadowName, ok := v.identNames[valueIdent]; ok {
						if obj := v.info.ObjectOf(valueIdent); obj != nil {
							rangeVarObjs[obj] = shadowName
						}
					}
				}
			}

			if node.Key != nil && node.Tok == token.ASSIGN {
				if keyIdent := getIdentifier(node.Key); keyIdent != nil {
					processor.processIdent(keyIdent, false) // Treat as reassignment
				}
			}

			if node.Value != nil && node.Tok == token.ASSIGN {
				if valueIdent := getIdentifier(node.Value); valueIdent != nil {
					processor.processIdent(valueIdent, false) // Treat as reassignment
				}
			}

			tracker.processing = false

			visitNode(node.X)

			// Process identifiers in the loop body to use shadowed names (object identity, see above)
			ast.Inspect(node.Body, func(n ast.Node) bool {
				if ident, ok := n.(*ast.Ident); ok {
					if obj := v.info.Uses[ident]; obj != nil {
						if shadowName, exists := rangeVarObjs[obj]; exists {
							v.identNames[ident] = shadowName
						}
					}
				}
				return true
			})

			visitNode(node.Body)

			tracker.exit()
			popScope()

		case *ast.ForStmt:
			// For-loop init scope (the body's own block scope is pushed when descending into it).
			pushScope(nil)
			tracker := registry.get(ForTracker)
			tracker.enter()
			tracker.processing = true

			processor := newVarProcessor(
				tracker,
				functionLevelDecls,
				declaredPos,
				v.info,
				declareVar,
				reassignVar,
			)

			if node.Init != nil {
				if init, ok := node.Init.(*ast.AssignStmt); ok {
					processor.processAssignStmt(init)
				}
			}

			tracker.processing = false

			// The for-loop init's RHS expressions reference variables from the ENCLOSING scope —
			// e.g. a shadow-renamed local in `for i, x := 0, b.Value(); …` (where an inner `b` was
			// renamed `bΔ1`). processAssignStmt only processed the init's LHS (the newly declared
			// loop vars), so traverse the RHS too, or those uses keep the raw name and resolve to the
			// wrong/forward variable (CS0841 / wrong binding).
			if node.Init != nil {
				if init, ok := node.Init.(*ast.AssignStmt); ok {
					for _, rhs := range init.Rhs {
						visitNode(rhs)
					}
				}
			}

			if node.Cond != nil {
				visitNode(node.Cond)
			}
			if node.Post != nil {
				visitNode(node.Post)
			}
			visitNode(node.Body)

			tracker.exit()
			popScope()

		case *ast.SelectStmt:
			// Select scope (each comm-clause body pushes its own block scope below).
			pushScope(nil)
			tracker := registry.get(SelectTracker)
			tracker.enter()

			// Process each case's statements
			for _, s := range node.Body.List {
				if commClause, ok := s.(*ast.CommClause); ok {
					// Handle communication statement assignments
					if comm, ok := commClause.Comm.(*ast.AssignStmt); ok {
						// The RECEIVE-ASSIGNMENT arm (`case x := <-c:`) must have its RHS
						// visited too. processAssignStmt walks the LHS *only* — it declares
						// the received bindings and never looks at the channel operand — so
						// before this, no ident inside `<-c` got an identNames mapping and
						// visitSelectStmt's hoisted temp emitted the RAW name (`var selᴛ1 =
						// c;`) while every other reference in the region used the
						// shadow-renamed `cΔ1`. CS0841, not CS0103, because the rename is
						// NECESSARY: a distinct later `c` in the same C# method scope is what
						// the bare name binds forward to (reflect's TestChan, all_test.go:1720).
						//
						// Visited BEFORE the LHS is declared, which is Go's own evaluation
						// order — the channel operand is evaluated in the ENCLOSING scope, so
						// a `case c := <-c:` resolves its RHS `c` to the outer binding rather
						// than to the one this clause is about to declare.
						//
						// Sibling arms (SEND, bare RECEIVE) were given the same treatment
						// below for the same reason; this closes the third arm.
						for _, rhs := range comm.Rhs {
							visitNode(rhs)
						}

						tracker.processing = true
						processor := newVarProcessor(
							tracker,
							functionLevelDecls,
							declaredPos,
							v.info,
							declareVar,
							reassignVar,
						)
						processor.processAssignStmt(comm)
						tracker.processing = false
					} else if commClause.Comm != nil {
						// A SEND comm (`case ch <- dialResult{…, primary: primary}:`) or a bare
						// RECEIVE (`case <-done:`) was never visited, so idents inside its
						// expressions got no identNames mapping — a Δ-renamed lambda param used
						// in the send value emitted its RAW name (net dial.go's startRacer,
						// CS0841).
						visitNode(commClause.Comm)
					}

					// Enter a new scope; a comm-clause body is an implicit block, so pre-scan its
					// direct declarations for forward shadow detection.
					pushScope(collectBlockLevelDecls(commClause.Body))

					// Visit the case body treating it as an implicit block
					blockTracker := registry.get(BlockTracker)
					blockTracker.enter()
					blockTracker.processing = true

					for _, stmt := range commClause.Body {
						visitNode(stmt)
					}

					blockTracker.exit()
					blockTracker.processing = blockTracker.level > 0

					// Exit the current scope
					popScope()
				}
			}

			tracker.exit()
			popScope()

		case *ast.CallExpr:
			if fun, ok := node.Fun.(*ast.Ident); ok {
				if fun.Name == "recover" {
					obj := v.info.Uses[fun]

					if obj != nil && obj.Parent() == types.Universe {
						v.hasRecover = true
					}
				}
			}

			// Visit function
			visitNode(node.Fun)

			// Visit call arguments
			for _, arg := range node.Args {
				visitNode(arg)
			}

		case *ast.SelectorExpr:
			// First, process X part for shadowing if it's an identifier
			if id, ok := node.X.(*ast.Ident); ok {
				if obj := v.info.Uses[id]; obj != nil {
					if varObj, ok := obj.(*types.Var); ok {
						if adjustedName, ok := v.varNames[varObj]; ok {
							v.identNames[id] = adjustedName
						}
					}
				}
			}

			// Then handle the existing lambda conversion logic
			if v.requiresLambdaConversion(node) {
				v.lambdaCapture.analysisInLambda = true
				v.lambdaCapture.currentLambda = node

				// Process the receiver for method values. The ENTIRE receiver expression is captured
				// into the synthesized lambda, so mark every ident in it — a FIELD-CHAIN receiver like
				// `kdf.hash.New` (crypto/internal/hpke) roots at `kdf`, not a bare ident, so the old
				// bare-ident-only check never marked it box-ref, and the captured receiver emitted as
				// its uncapturable `ref var kdf` alias (CS1628). A nested func literal handles its own
				// captures — don't descend into it.
				ast.Inspect(node.X, func(inner ast.Node) bool {
					if _, isLit := inner.(*ast.FuncLit); isLit {
						return false
					}

					if id, ok := inner.(*ast.Ident); ok {
						v.processPotentialCapture(id)
					}

					return true
				})

				v.lambdaCapture.analysisInLambda = false
				v.lambdaCapture.currentLambda = nil
			}

			// Recursively visit X and Sel
			visitNode(node.X)
			visitNode(node.Sel)

		case *ast.TypeAssertExpr:
			// Process the expression being asserted
			if expr, ok := node.X.(*ast.Ident); ok {
				if obj := v.info.Uses[expr]; obj != nil {
					if varObj, ok := obj.(*types.Var); ok {
						if adjustedName, ok := v.varNames[varObj]; ok {
							v.identNames[expr] = adjustedName
						}
					}
				}
			}
			visitNode(node.X)
			visitNode(node.Type)

		case *ast.CompositeLit:
			// Process identifiers in key expressions for struct literals and map literals
			for _, elt := range node.Elts {
				if kv, ok := elt.(*ast.KeyValueExpr); ok {
					if ident, ok := kv.Key.(*ast.Ident); ok {
						if obj := v.info.Uses[ident]; obj != nil {
							if varObj, ok := obj.(*types.Var); ok {
								if adjustedName, ok := v.varNames[varObj]; ok {
									v.identNames[ident] = adjustedName
								}
							}
						}
					}
				}
			}

			// Visit children normally
			ast.Inspect(node, func(child ast.Node) bool {
				if child == node {
					return true
				}
				if child != nil {
					visitNode(child)
				}
				return false
			})

		case *ast.IndexExpr:
			// Process the array/slice/map being indexed
			if ident, ok := node.X.(*ast.Ident); ok {
				if obj := v.info.Uses[ident]; obj != nil {
					if varObj, ok := obj.(*types.Var); ok {
						if adjustedName, ok := v.varNames[varObj]; ok {
							v.identNames[ident] = adjustedName
						}
					}
				}
			}

			// Process the index expression
			if ident, ok := node.Index.(*ast.Ident); ok {
				if obj := v.info.Uses[ident]; obj != nil {
					if varObj, ok := obj.(*types.Var); ok {
						if adjustedName, ok := v.varNames[varObj]; ok {
							v.identNames[ident] = adjustedName
						}
					}
				}
			}

			visitNode(node.X)
			visitNode(node.Index)

		default:
			// Visit child nodes
			ast.Inspect(n, func(child ast.Node) bool {
				if child == nil {
					return false // Do not call visitNode with nil
				}

				if child == n {
					return true // Avoid processing the same node again
				}

				visitNode(child)

				// Return false to prevent ast.Inspect from recursing
				// into children, since visitNode will handle them
				return false
			})
		}
	}

	// Start traversal
	visitNode(funcDecl.Body)
}

func (v *Visitor) processPotentialCapture(ident *ast.Ident) {
	var escapesToHeap bool

	if obj := v.info.ObjectOf(ident); obj != nil {
		if escapes, exists := v.identEscapesHeap[obj]; exists {
			escapesToHeap = escapes
		}
	}

	if !v.lambdaCapture.analysisInLambda || isDiscardedVar(ident.Name) {
		return
	}

	// Skip if we've already recorded this capture
	if v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda] != nil {
		if _, exists := v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda][ident]; exists {
			return
		}
	}

	obj := v.info.Uses[ident]
	if obj == nil {
		return
	}

	varObj, ok := obj.(*types.Var)

	if !ok {
		return
	}

	if !v.shouldCapture(varObj, ident) {
		return
	}

	// A var an ENCLOSING lambda's analysis already marked box-ref is referenced through its box
	// EVERYWHERE — the box is a plain reference local that closures at any nesting depth capture
	// directly — so a nested literal must not snapshot-copy it: the copy's box render is a name
	// that is never declared (`ᏑlazyCertʗ1`, CS0103), and the copy divorces the nested writes Go
	// shares (crypto/x509 AppendCertsFromPEM's inner `lazyCert.v, _ = ParseCertificate(…)`).
	if v.lambdaCapture.boxRefVars[varObj] {
		return
	}

	// A deref'd pointer parameter or pointer receiver is a `ref var p = ref Ꮡp.Value` alias, which a C#
	// closure cannot capture (CS8175). Inside a lambda, reference it through its box `Ꮡp` instead —
	// value uses become `Ꮡp.Value`, address uses `Ꮡp` (see convIdent / convUnaryExpr). The box is a
	// plain reference-typed parameter, captured by reference, matching Go's capture of the pointer.
	if v.varIsDerefdPointerParam(varObj) {
		v.lambdaCapture.boxRefVars[varObj] = true
		return
	}

	// A heap-boxed VALUE parameter is the same shape: its entry-time box prologue re-declares the
	// Go name as a ref-local alias (`ref var cfg = ref heap(cfgʗp, out var Ꮡcfg);` — see
	// paramNeedsHeapBox), which a C# closure cannot capture (CS8175), and a snapshot copy divorces
	// the closure from the boxed storage the direct-ж callee mutates through the receiver pointer
	// (Go's closure and the callee share the ONE parameter variable). Reference it through its box.
	if v.paramNeedsHeapBox(varObj) {
		v.lambdaCapture.boxRefVars[varObj] = true
		return
	}

	// Check if variable needs capture due to:
	// 1. Being a reference type that needs copying, OR
	// 2. Having escaped to heap (being a ref in C#)
	needsRef := v.capturedLambdaVarRequiresCopy(varObj.Type())
	needsCapture := needsRef || escapesToHeap

	if !needsCapture {
		return
	}

	// A heap-boxed local whose address is taken inside this lambda must be referenced through its
	// box, not snapshot-copied: the value copy loses the box (writes through the captured `&m` are
	// lost) and the copy declaration `var mʗ1 = m;` is invalid in expression position (a func
	// literal passed as a call argument has no statement slot). Mark it box-ref and skip the
	// snapshot — emission then renders `&m` as `Ꮡm` and value uses as `Ꮡm.Value` (see convUnaryExpr
	// / convIdent). The box `Ꮡm` is a plain local, captured by reference by the C# closure, which
	// matches Go's capture-by-reference semantics.
	if escapesToHeap && v.varAddressTakenInLambda(varObj) {
		v.lambdaCapture.boxRefVars[varObj] = true
		return
	}

	// A heap-boxed VALUE local used as the receiver of a POINTER-receiver method call inside this
	// lambda is address-taken IMPLICITLY — Go auto-takes `&state` for `defer state.free()` (state a
	// value local, free a `*handleState` method; log/slog handler.go). Emission binds it through the
	// box (`Ꮡstate.free`, see pointerReceiverBoxMethodGroup), so it too must be referenced by-box and
	// NOT snapshot-copied: the copy `var stateʗ1 = state;` is a plain value whose box `Ꮡstateʗ1` is
	// then referenced yet never declared (CS0103 ×3). Mark box-ref and skip the snapshot; emission
	// renders `Ꮡstate` from the original heap box — matching Go's semantics of binding the address of
	// the LIVE variable at defer time (a value snapshot would also miss body mutations before free).
	if escapesToHeap && v.varUsedAsImplicitAddrReceiverInLambda(varObj) {
		v.lambdaCapture.boxRefVars[varObj] = true
		return
	}

	// A value snapshot (`var tʗ1 = t;` hoisted before the lambda, in-lambda references renamed) is
	// observationally correct only while NEITHER side writes the variable after the snapshot point.
	// Go closures share the ONE variable, so once anything writes it the snapshot silently diverges:
	// the closure's writes land in a divorced copy (`bump := func() { t.total += 100 }` never affects
	// t), and body writes after the lambda's creation are invisible to a read-only closure (`get :=
	// func() int { return t.total }` reads a stale copy) — including a deferred literal, which in Go
	// observes the variable's FINAL value. When the capture is a genuine closure-body reference and
	// the variable is written after capture (varShareFacts), route it to SHARED storage instead:
	//   - a heap-boxed variable is referenced through its box (boxRefVars → `Ꮡt` / `Ꮡt.Value`),
	//     which the C# closure captures by reference — the box is the shared storage;
	//   - an unboxed variable (a value parameter, or a slice/map/chan local, whose copy would
	//     diverge on reassignment) is captured NATIVELY — no snapshot, no rename: the C# display-
	//     class capture shares the local exactly as Go shares the variable (the same treatment
	//     non-escaping basics have always had).
	// Read-only-after-capture variables keep the snapshot (observationally identical, zero churn).
	// A loop-statement-defined variable also keeps it: its per-lambda copy approximates Go's
	// per-iteration variable, which shared routing would break. The literal's OWN parameters,
	// results, and locals (declared within its extent) are not captures at all — writes to them are
	// the literal's private state — so they fall through to the recording path, whose conversion-
	// time name filter drops them (convFuncLit); routing one box-ref would render a box that is
	// never declared.
	if lit := v.closureBodyCaptureLit(); lit != nil && (varObj.Pos() < lit.Pos() || varObj.Pos() > lit.End()) {
		if facts := v.varShareFacts(varObj); facts.writtenAfterCapture && !facts.loopStmtDefined {
			if escapesToHeap {
				v.lambdaCapture.boxRefVars[varObj] = true
			}

			return
		}
	}

	// Record the capture
	if v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda] == nil {
		v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda] = make(map[*ast.Ident]bool)
	}

	v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda][ident] = true
}

// varAddressTakenInLambda reports whether the address of varObj is taken inside the lambda currently
// being analyzed, in a form that emission renders through the box (see lambdaBoxRefAddressForm):
// the bare identifier (`&m`) or a value-struct field (`&m.field`). An element address (`&m[i]`) is
// NOT matched — it keeps the existing snapshot path (no box-ref emission form for it). Emission then
// renders every address/value form of the box-ref var consistently through the box `Ꮡm`.
func (v *Visitor) varAddressTakenInLambda(varObj types.Object) bool {
	lambda := v.lambdaCapture.currentLambda

	if lambda == nil || varObj == nil {
		return false
	}

	found := false

	ast.Inspect(lambda, func(n ast.Node) bool {
		if found {
			return false
		}

		unaryExpr, ok := n.(*ast.UnaryExpr)

		if !ok || unaryExpr.Op != token.AND {
			return true
		}

		// &m
		if ident, ok := unaryExpr.X.(*ast.Ident); ok && v.info.ObjectOf(ident) == varObj {
			found = true
			return false
		}

		// &m.field where m is a value struct (matches the SelectorExpr case in lambdaBoxRefAddressForm)
		if sel, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
			if ident, ok := sel.X.(*ast.Ident); ok && v.info.ObjectOf(ident) == varObj {
				if _, ok := v.getType(sel.X, true).(*types.Struct); ok {
					found = true
					return false
				}
			}
		}

		return true
	})

	return found
}

// varUsedAsImplicitAddrReceiverInLambda reports whether varObj is used inside the lambda currently
// being analyzed as the VALUE receiver of a POINTER-receiver method call (`defer state.free()`,
// state a value local, free a `*handleState` method) — directly or PROMOTED through value embeds
// (`lazyCert.Do(…)` on a struct embedding sync.Once). Go auto-takes the receiver's address, and
// emission binds this through the box (`Ꮡstate.free` / the `.of(…)` field projection, see
// pointerReceiverBoxMethodGroup's value arm), so an escaping such local must be captured by-box,
// not snapshot-copied. The direct case mirrors that arm's guard exactly: a NAMED value receiver
// whose type is identical to the method's pointer-receiver pointee. An ALREADY-pointer receiver
// (`defer conf.releaseSema()`) is excluded — its box group is the pointer variable itself, whose
// snapshot name IS declared, so it needs no box-ref treatment.
func (v *Visitor) varUsedAsImplicitAddrReceiverInLambda(varObj types.Object) bool {
	lambda := v.lambdaCapture.currentLambda

	if lambda == nil || varObj == nil {
		return false
	}

	found := false

	ast.Inspect(lambda, func(n ast.Node) bool {
		if found {
			return false
		}

		call, ok := n.(*ast.CallExpr)

		if !ok {
			return true
		}

		if v.callImpliesReceiverAddr(call, varObj) {
			found = true
			return false
		}

		return true
	})

	return found
}

// callImpliesReceiverAddr reports whether call invokes a POINTER-receiver method on varObj held as a
// VALUE — so Go implicitly takes `&varObj` for the call. The match mirrors pointerReceiverBoxMethodGroup's
// value arm: a NAMED value receiver whose type is identical to the method's pointer-receiver pointee, or a
// pointer-receiver method PROMOTED through VALUE embeds (the address roots INTO varObj's own storage). An
// ALREADY-pointer receiver is excluded — its call copies the pointer, not the variable's address.
func (v *Visitor) callImpliesReceiverAddr(call *ast.CallExpr, varObj types.Object) bool {
	sel, ok := call.Fun.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	return v.selectorImpliesReceiverAddr(sel, varObj)
}

// selectorImpliesReceiverAddr is callImpliesReceiverAddr's core on the selector itself — also used
// for an UNCALLED method value (`f := t.Add`, Add pointer-receiver), which binds `&t` the same way.
func (v *Visitor) selectorImpliesReceiverAddr(sel *ast.SelectorExpr, varObj types.Object) bool {
	ident, ok := sel.X.(*ast.Ident)
	fieldProjection := false

	if !ok {
		// A single field projection of the var (`p.fake.setLines()`): the projected member
		// must be a FIELD selected on the var's own VALUE-struct storage — matching the
		// `&m.field` form of varAddressTakenInLambda and the lambdaBoxRefAddressForm
		// emission (`Ꮡp.of(T.Ꮡfake)`). A deeper chain or a method/pointer hop keeps the
		// snapshot handling.
		fieldSel, isSel := sel.X.(*ast.SelectorExpr)

		if !isSel {
			return false
		}

		if ident, ok = fieldSel.X.(*ast.Ident); !ok {
			return false
		}

		if _, isField := v.info.ObjectOf(fieldSel.Sel).(*types.Var); !isField {
			return false
		}

		if _, isStruct := v.getType(fieldSel.X, true).(*types.Struct); !isStruct {
			return false
		}

		fieldProjection = true
	}

	if v.info.ObjectOf(ident) != varObj {
		return false
	}

	funcObj, ok := v.info.ObjectOf(sel.Sel).(*types.Func)

	if !ok {
		return false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return false
	}

	recvPtr, isPtrRecv := sig.Recv().Type().(*types.Pointer)

	if !isPtrRecv {
		return false
	}

	recvType := v.getType(sel.X, false)

	if recvType == nil {
		return false
	}

	// Already a pointer (arm 1 of pointerReceiverBoxMethodGroup) — excluded (see doc).
	if _, alreadyPtr := recvType.(*types.Pointer); alreadyPtr {
		return false
	}

	// A NAMED value whose type is exactly the pointer-receiver's pointee (arm 2).
	if types.Identical(recvPtr.Elem(), recvType) {
		_, isNamed := recvType.(*types.Named)
		return isNamed
	}

	// A PROMOTED pointer-receiver method reached through EMBEDDED fields — `lazyCert.Do(…)`
	// on `var lazyCert struct { sync.Once; … }` (crypto/x509 AppendCertsFromPEM): Go takes
	// `&lazyCert.Once`, an address INTO the variable's own storage, so the var needs the same
	// by-box capture — emission renders the call through the box projection
	// (`ᏑlazyCert.of(T.ᏑOnce).Do(…)`), and a snapshot declares no such box (CS0103) besides
	// divorcing the closure's writes from the original. Only VALUE embeds along the selection
	// path root the address at varObj: a POINTER embed re-roots it at that pointer's target,
	// and the snapshot (which copies the pointer) stays sound there.
	// The field-projection form is matched on the exact pointee only — a method promoted
	// through the FIELD's own embeds keeps the existing snapshot handling (its emission
	// does not take the single-hop box form this analysis pairs with).
	if fieldProjection {
		return false
	}

	selection, ok := v.info.Selections[sel]

	if !ok || selection.Kind() != types.MethodVal || len(selection.Index()) < 2 {
		return false
	}

	base := recvType

	for _, fieldIndex := range selection.Index()[:len(selection.Index())-1] {
		st, isStruct := base.Underlying().(*types.Struct)

		if !isStruct || fieldIndex >= st.NumFields() {
			return false
		}

		fieldType := st.Field(fieldIndex).Type()

		if _, fieldIsPtr := fieldType.Underlying().(*types.Pointer); fieldIsPtr {
			return false
		}

		base = fieldType
	}

	return true
}

// captureShareFacts holds, per captured variable, the facts the shared-capture routing in
// processPotentialCapture derives from one scan of the enclosing declaration's body.
type captureShareFacts struct {
	// writtenAfterCapture: some write to the variable can execute after a func literal that
	// references it has been created — so a value snapshot at the literal's creation silently
	// diverges from Go's shared-variable closure semantics (in one direction or the other).
	writtenAfterCapture bool
	// loopStmtDefined: the variable is defined by a for-init or range clause. Such a variable is
	// per-iteration in Go (1.22+); the per-lambda snapshot approximates that, while shared routing
	// (one box, or the shared C# for-loop control variable) would break it — it keeps the snapshot.
	loopStmtDefined bool
}

// closureBodyCaptureLit returns the func literal whose BODY the capture analysis is currently
// inside — the literal itself, or a go/defer statement's func-literal callee — and nil otherwise.
// Only inside a literal's body do Go's shared-variable closure semantics apply. The other capture
// contexts — a method-value receiver (`f := t.Add`) and a go/defer statement's non-literal callee
// expression — evaluate the receiver/callee AT STATEMENT TIME, so their value snapshot IS the
// correct Go semantics and must stay.
func (v *Visitor) closureBodyCaptureLit() *ast.FuncLit {
	switch node := v.lambdaCapture.currentLambda.(type) {
	case *ast.FuncLit:
		return node
	case *ast.GoStmt:
		if lit, ok := node.Call.Fun.(*ast.FuncLit); ok {
			return lit
		}
	case *ast.DeferStmt:
		if lit, ok := node.Call.Fun.(*ast.FuncLit); ok {
			return lit
		}
	}

	return nil
}

// varShareFacts memoizes computeCaptureShareFacts per variable for the current analysis decl.
func (v *Visitor) varShareFacts(varObj types.Object) captureShareFacts {
	if varObj == nil {
		return captureShareFacts{}
	}

	if v.captureShareFactsCache == nil {
		v.captureShareFactsCache = make(map[types.Object]captureShareFacts)
	}

	if facts, cached := v.captureShareFactsCache[varObj]; cached {
		return facts
	}

	facts := v.computeCaptureShareFacts(varObj)
	v.captureShareFactsCache[varObj] = facts

	return facts
}

// computeCaptureShareFacts scans the current analysis declaration's body once for varObj and decides
// whether the variable is "written after capture": some write to it can execute after a func literal
// referencing it exists. Writes counted, conservatively by syntax:
//   - an assignment or ++/-- whose target roots at the variable (`t = …`, `t.f.g = …`, `t[i] = …`;
//     a deref target `*p = …` roots at the alias's referent, not the pointer variable, and is
//     accounted at the alias's creation instead);
//   - a pointer-receiver method CALL on the variable held as a value (Go's implicit `&t`);
//   - a `for t = range …` clause (rewritten every iteration);
//   - an explicit `&t` (or `&t.field`/`&t[i]`) ANYWHERE, and an UNCALLED pointer-receiver method
//     value — both create an alias through which later writes are syntactically invisible, so they
//     count as writes that can occur at ANY time after;
//   - any of the above INSIDE any func literal — the literal may run at any time after creation.
//
// A plain body write counts only if it can execute after some referencing literal is created: it is
// positioned after the literal, it ENCLOSES the literal (the literal is evaluated as part of the
// write — `check = func(…){ … check(…) … }`, Go's self-recursive closure), or both sit inside the
// same for/range loop (a later iteration's write follows an earlier iteration's creation).
func (v *Visitor) computeCaptureShareFacts(varObj types.Object) captureShareFacts {
	facts := captureShareFacts{}
	decl := v.captureAnalysisDecl

	if decl == nil || decl.Body == nil {
		return facts
	}

	type site struct {
		pos token.Pos
		// end bounds a WRITE site's syntactic extent. A write whose statement/expression textually
		// CONTAINS a referencing literal executes AFTER that literal is created (the RHS, or the
		// call's argument list, is evaluated first) even though it starts before it — the
		// self-recursive closure idiom `check = func(…){ … check(…) … }`. Zero for literal sites.
		end   token.Pos
		loops []ast.Node
	}

	type litEntry struct {
		lit      *ast.FuncLit
		loops    []ast.Node
		recorded bool
	}

	// The single walk below maintains four parallel stacks. They are what turn a flat traversal
	// back into the NESTING information the ordering decision needs, so each one's invariant is
	// worth stating exactly:
	//
	//	nodeStack  every node currently open, innermost last. This exists only because
	//	           ast.Inspect's exit callback is a bare nil and does not say WHICH node just
	//	           closed — so the walk records what it entered in order to know what it is
	//	           leaving. It is the bookkeeping that makes the other two stacks poppable.
	//	loopStack  the for/range statements enclosing the current node, outermost first. Two sites
	//	           that share a loop can execute in either order across iterations, which is the
	//	           whole reason loop membership is tracked rather than just source position.
	//	litStack   the func literals enclosing the current node. Non-empty means "we are inside a
	//	           closure", where a write may run at ANY time after the closure is created — so
	//	           position stops being informative and the write is promoted to anytimeWrite.
	//	           Its recorded flag makes a literal count ONCE no matter how many times it
	//	           mentions the variable.
	//
	// Every push has exactly one matching pop keyed off nodeStack, and the three must stay in
	// lockstep. A missed push or pop silently corrupts the loop/closure context of everything
	// walked afterwards, and the failure is quiet in the worst way: the analysis simply concludes
	// the wrong thing about whether a capture needs a snapshot. Under-reporting is what broke
	// regexp's makeOnePass (a self-recursive closure captured a still-nil delegate and every
	// recursive call panicked); over-reporting merely emits needless snapshot copies.
	//
	// litRefs / bodyWrites are the walk's OUTPUT: where referencing literals are created, and
	// where the variable is written, each tagged with the loops enclosing it.
	var (
		litRefs    []site
		bodyWrites []site
		loopStack  []ast.Node
		litStack   []litEntry
		nodeStack  []ast.Node
	)

	// anytimeWrite short-circuits the whole position comparison: it means a write exists whose
	// timing cannot be pinned down syntactically at all (it is inside a closure, or it happens
	// through an alias created by `&t`). Nothing can prove such a write precedes every capture,
	// so the variable is treated as written-after-capture outright.
	anytimeWrite := false

	// calledSelectors remembers which selectors appeared as the FUNCTION of a call, so the
	// SelectorExpr arm can tell an invoked pointer-receiver method (`t.Push(x)`, already counted
	// by the CallExpr arm) from an uncalled METHOD VALUE (`f := t.Push`), which hands out a
	// durable alias to the receiver and therefore counts as a write at any time.
	calledSelectors := make(map[*ast.SelectorExpr]bool)

	snapshotLoops := func() []ast.Node {
		return append([]ast.Node(nil), loopStack...)
	}

	rootsAtVar := func(expr ast.Expr) bool {
		root := v.writeTargetRootIdent(expr)
		return root != nil && v.info.ObjectOf(root) == varObj && v.info.Defs[root] == nil
	}

	// recordWrite files a write at its source position — unless it is inside a closure, where
	// position says nothing about when it runs, so it becomes an anytime write instead.
	recordWrite := func(node ast.Node) {
		if len(litStack) > 0 {
			anytimeWrite = true
			return
		}

		bodyWrites = append(bodyWrites, site{node.Pos(), node.End(), snapshotLoops()})
	}

	// The walk is a hand-rolled ast.Inspect rather than an ast.Visitor because it needs the EXIT
	// event: ast.Inspect calls this function once with the node on the way in and once with nil on
	// the way out, which is exactly the push/pop protocol the stacks require. (An ast.Visitor's
	// Visit returns a visitor rather than signalling descent completion, so unwinding would have
	// to be reconstructed some other way.)
	ast.Inspect(decl.Body, func(n ast.Node) bool {
		// Leaving a node: pop it, and pop any stack it had pushed. This is the ONLY place the
		// loop and literal stacks shrink, which is what keeps them matched to the entry pushes.
		if n == nil {
			top := nodeStack[len(nodeStack)-1]
			nodeStack = nodeStack[:len(nodeStack)-1]

			switch top.(type) {
			case *ast.ForStmt, *ast.RangeStmt:
				loopStack = loopStack[:len(loopStack)-1]
			case *ast.FuncLit:
				litStack = litStack[:len(litStack)-1]
			}

			return true
		}

		// Entering a node: record it so the matching exit above knows what it is popping.
		nodeStack = append(nodeStack, n)

		switch node := n.(type) {
		case *ast.ForStmt:
			if init, ok := node.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
				for _, lhs := range init.Lhs {
					if ident, ok := lhs.(*ast.Ident); ok && v.info.Defs[ident] == varObj {
						facts.loopStmtDefined = true
					}
				}
			}

			loopStack = append(loopStack, node)

		case *ast.RangeStmt:
			if node.Tok == token.DEFINE {
				for _, expr := range []ast.Expr{node.Key, node.Value} {
					if ident, ok := expr.(*ast.Ident); ok && v.info.Defs[ident] == varObj {
						facts.loopStmtDefined = true
					}
				}
			}

			loopStack = append(loopStack, node)

			if node.Tok == token.ASSIGN {
				for _, expr := range []ast.Expr{node.Key, node.Value} {
					if expr != nil && rootsAtVar(expr) {
						recordWrite(node)
					}
				}
			}

		case *ast.FuncLit:
			// Enter a closure. It is recorded as REFERENCING the variable only if an Ident arm
			// below actually finds a mention inside it, so a closure that ignores the variable
			// never constrains anything.
			litStack = append(litStack, litEntry{node, snapshotLoops(), false})

		case *ast.Ident:
			// A USE of the variable (Defs[node] == nil excludes the declaration itself). Every
			// enclosing literal captures it, not just the innermost — a mention three closures
			// deep is captured by all three — so the whole stack is marked. The recorded flag
			// keeps each literal to a single entry however many times it mentions the variable,
			// and the position filed is the LITERAL's, since what matters is when the closure
			// came into existence, not where inside it the variable appears.
			if v.info.ObjectOf(node) == varObj && v.info.Defs[node] == nil {
				for i := range litStack {
					if !litStack[i].recorded {
						litStack[i].recorded = true
						litRefs = append(litRefs, site{litStack[i].lit.Pos(), token.NoPos, litStack[i].loops})
					}
				}
			}

		case *ast.AssignStmt:
			for _, lhs := range node.Lhs {
				if rootsAtVar(lhs) {
					recordWrite(node)
				}
			}

		case *ast.IncDecStmt:
			if rootsAtVar(node.X) {
				recordWrite(node)
			}

		case *ast.UnaryExpr:
			// `&t` hands out a pointer. Every write through that pointer from here on is
			// syntactically invisible to this walk, so the address-taking itself is treated as a
			// write that can happen at any time.
			if node.Op == token.AND && rootsAtVar(node.X) {
				anytimeWrite = true
			}

		case *ast.CallExpr:
			// Note the selector is marked BEFORE the walk descends into it, so the SelectorExpr
			// arm below sees the mark when it is reached and does not mistake this invocation for
			// a bare method value.
			if sel, ok := node.Fun.(*ast.SelectorExpr); ok {
				calledSelectors[sel] = true
			}

			// Calling a pointer-receiver method on a value is Go's implicit `&t`, and the callee
			// may write through it — but only for the duration of the call, so unlike a bare `&t`
			// this is an ordinary positioned write.
			if v.callImpliesReceiverAddr(node, varObj) {
				recordWrite(node)
			}

		case *ast.SelectorExpr:
			// An UNCALLED pointer-receiver method value (`f := t.Push`) binds the receiver
			// address into the resulting func value, which can be invoked later from anywhere.
			if !calledSelectors[node] && v.selectorImpliesReceiverAddr(node, varObj) {
				anytimeWrite = true
			}
		}

		return true
	})

	// No closure references the variable, so there is no capture for a write to race with.
	if len(litRefs) == 0 {
		return facts
	}

	// A write with unpinnable timing exists, so no ordering argument can clear it.
	if anytimeWrite {
		facts.writtenAfterCapture = true
		return facts
	}

	// sharesLoop reports whether two sites sit inside a common loop. Loop nodes are compared by
	// IDENTITY (the same *ast.ForStmt pointer), so this asks "the same loop", not "a similar one".
	sharesLoop := func(a, b []ast.Node) bool {
		for _, x := range a {
			for _, y := range b {
				if x == y {
					return true
				}
			}
		}

		return false
	}

	// Every remaining write has a definite position, so ask pairwise whether any of them can run
	// after any capture. Three ways that happens:
	//
	//	w.pos > lit.pos                    the write simply comes later in the source
	//	lit.pos inside [w.pos, w.end)      the literal is created BY the write, evaluated as part
	//	                                   of its right-hand side, so the write lands afterwards
	//	they share a loop                  a later iteration's write follows an earlier
	//	                                   iteration's capture
	for _, w := range bodyWrites {
		for _, lit := range litRefs {
			// `w.pos < lit.pos < w.end` — the literal sits INSIDE the write, so the write lands
			// after it exists: `check = func(…){ … check(…) … }`. Snapshotting there captured the
			// still-nil delegate and every recursive call NREd (regexp's makeOnePass, which made
			// `^.$` and most of the package's patterns uncompilable).
			if w.pos > lit.pos || (lit.pos > w.pos && lit.pos < w.end) || sharesLoop(w.loops, lit.loops) {
				facts.writtenAfterCapture = true
				return facts
			}
		}
	}

	return facts
}

// writeTargetRootIdent returns the base identifier whose OWN STORAGE a write target (or `&` operand)
// roots at: `t` for `t`, `t.f.g` (value fields), and `a[i]` (array elements). It returns nil when the
// path leaves the variable's storage: a deref (`*p = …`), an IMPLICIT deref (`p.f = …` with p a
// pointer — Go's `(*p).f`, which writes the pointee, not the variable), and a slice/map element
// (`s[i] = …` writes shared backing, which a snapshot copy shares too — no divergence).
func (v *Visitor) writeTargetRootIdent(expr ast.Expr) *ast.Ident {
	for {
		switch e := expr.(type) {
		case *ast.Ident:
			return e
		case *ast.ParenExpr:
			expr = e.X
		case *ast.SelectorExpr:
			if baseType := v.getType(e.X, false); baseType != nil {
				if _, isPtr := baseType.Underlying().(*types.Pointer); isPtr {
					return nil
				}
			}

			expr = e.X
		case *ast.IndexExpr:
			baseType := v.getType(e.X, false)

			if baseType == nil {
				return nil
			}

			if _, isArray := baseType.Underlying().(*types.Array); !isArray {
				return nil
			}

			expr = e.X
		default:
			return nil
		}
	}
}

// varIsDerefdPointerParam reports whether varObj is a pointer-typed parameter (or the pointer
// receiver) of the current function. Such a parameter is emitted as the box `ж<T> Ꮡp` and aliased to
// a value with `ref var p = ref Ꮡp.Value`; the ref-local alias cannot be captured by a C# closure, so
// inside a lambda it must be referenced through the box `Ꮡp` (rendered by the box-ref-var paths).
func (v *Visitor) varIsDerefdPointerParam(varObj types.Object) bool {
	if varObj == nil || v.currentFuncSignature == nil {
		return false
	}

	// A SELF-SEEDED signature is a func literal's own, not an enclosing declaration's (see
	// convFuncLit's seed), and a literal's pointer parameter carries no deref alias — it IS the box
	// — which is the exact property this predicate reports. Answering true there marks the literal's
	// own parameter as box-ref-captured, so a NESTED literal renders the undeclared `Ꮡp` (CS0103).
	if v.funcSignatureIsLiteralSeed {
		return false
	}

	if _, isVar := varObj.(*types.Var); !isVar {
		return false
	}

	if _, isPtr := varObj.Type().Underlying().(*types.Pointer); !isPtr {
		return false
	}

	if recv := v.currentFuncSignature.Recv(); recv == varObj {
		return true
	}

	params := v.currentFuncSignature.Params()

	for i := range params.Len() {
		if params.At(i) == varObj {
			return true
		}
	}

	return false
}

// isLambdaBoxRefVar reports whether obj is a heap-boxed local marked for box-ref capture (its
// address was taken inside a lambda). Emission references such a var through its box `Ꮡm`.
func (v *Visitor) isLambdaBoxRefVar(obj types.Object) bool {
	return v.lambdaCapture != nil && obj != nil && v.lambdaCapture.boxRefVars[obj]
}

// Helper to determine if an expression requires lambda conversion
func (v *Visitor) requiresLambdaConversion(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.FuncLit:
		return true

	case *ast.SelectorExpr:
		return v.isMethodValue(e, false)

	case *ast.Ident:
		// Check if identifier refers to a function value
		if obj := v.info.ObjectOf(e); obj != nil {
			_, isFunc := obj.(*types.Func)
			return isFunc
		}

	case *ast.CallExpr:
		// Check if this is a function call that returns a function
		if typ := v.info.TypeOf(e); typ != nil {
			_, isFunc := typ.Underlying().(*types.Signature)
			return isFunc
		}
	}
	return false
}

// methodValueBindsReceiverAddress reports a method VALUE whose method has a POINTER receiver while
// the receiver expression is a VALUE — Go's implicit `(&x).M`. Such a value binds x's own storage,
// never a copy, so the receiver-capture SNAPSHOT the assignment path takes for an ordinary method
// value (`var cʗ1 = c;`, which preserves a VALUE receiver's bind-a-COPY semantics) is both wrong and
// unrenderable here: the escape analysis has already heap-boxed the local, and the emission wants
// that box (`Ꮡc.bump`) — the snapshot rename turned it into the undeclared `Ꮡcʗ1` (CS0103). A
// receiver expression that is ALREADY a pointer keeps its snapshot: Go copies the pointer there,
// which is exactly what the snapshot models.
func (v *Visitor) methodValueBindsReceiverAddress(sel *ast.SelectorExpr) bool {
	funcObj, ok := v.info.ObjectOf(sel.Sel).(*types.Func)

	if !ok {
		return false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return false
	}

	if _, isPtrRecv := sig.Recv().Type().(*types.Pointer); !isPtrRecv {
		return false
	}

	recvType := v.getType(sel.X, false)

	if recvType == nil {
		return false
	}

	_, alreadyPtr := recvType.(*types.Pointer)

	return !alreadyPtr
}

func (v *Visitor) isMethodValue(sel *ast.SelectorExpr, isCallExpr bool) bool {
	if sel.Sel == nil {
		return false
	}

	obj := v.info.ObjectOf(sel.Sel)
	if obj == nil {
		return false
	}

	_, isFunc := obj.(*types.Func)
	if !isFunc {
		return false
	}

	// Not a method value if it's being called
	return !isCallExpr
}

func (v *Visitor) isPackageIdentifier(ident *ast.Ident) bool {
	obj := v.info.Uses[ident]
	_, ok := obj.(*types.PkgName)
	return ok
}

// isValueType determines if a type is a value type
func isValueType(t types.Type) bool {
	if t == nil {
		return false
	}

	if _, ok := t.Underlying().(*types.Basic); ok {
		return true
	}
	return false
}

// Determine if a type needs to be copied when captured
func (v *Visitor) capturedLambdaVarRequiresCopy(t types.Type) bool {
	switch typ := t.Underlying().(type) {
	case *types.Array:
		return true
	case *types.Struct:
		return true
	case *types.Chan:
		return true
	case *types.Slice:
		return true
	case *types.Map:
		return true
	case *types.Named:
		return v.capturedLambdaVarRequiresCopy(typ.Underlying())
	}
	return false
}

func (v *Visitor) getCapturedVarName(varPrefix string) string {
	if v.capturedVarCount == nil {
		v.capturedVarCount = make(map[string]int)
	}

	// Get counter specific to this variable prefix
	count := v.capturedVarCount[varPrefix]

	// Only increment during name generation phase
	if !v.lambdaCapture.detectingCaptures {
		count++
		v.capturedVarCount[varPrefix] = count
	}

	// Return the capture name using the variable-specific counter
	return fmt.Sprintf("%s%s%d", varPrefix, CapturedVarMarker, count)
}

func (v *Visitor) prepareStmtCaptures(stmt ast.Node) {
	if captures, ok := v.lambdaCapture.stmtCaptures[stmt]; ok {
		v.lambdaCapture.detectingCaptures = false

		// Use a map to ensure unique variables
		uniqueVars := make(map[string]*ast.Ident)

		for ident := range captures {
			// Only keep one instance of each variable name
			uniqueVars[ident.Name] = ident
		}

		// Create sorted list from unique variables
		idents := make([]*ast.Ident, 0, len(uniqueVars))

		for _, ident := range uniqueVars {
			idents = append(idents, ident)
		}

		sort.Slice(idents, func(i, j int) bool {
			return idents[i].Name < idents[j].Name
		})

		// Process unique captures in a consistent order
		for _, ident := range idents {
			captureName := v.getCapturedVarName(ident.Name)

			copyIdent := &ast.Ident{
				Name: captureName,
				Obj:  ident.Obj,
			}

			info := &CapturedVarInfo{
				origIdent: ident,
				copyIdent: copyIdent,
				varType:   v.info.TypeOf(ident),
			}

			v.lambdaCapture.pendingCaptures[ident.Name] = info
			v.lambdaCapture.currentLambdaVars[ident.Name] = captureName
			v.lambdaCapture.currentLambdaVarObjs[ident.Name] = v.info.ObjectOf(ident)
		}
	}
}

// Generate declarations for pending captures - written out before lambda expression
func (v *Visitor) generateCaptureDeclarations() string {
	if v.lambdaCapture == nil {
		return ""
	}

	pendingCaptures := v.lambdaCapture.pendingCaptures

	if len(pendingCaptures) == 0 {
		return ""
	}

	var decls strings.Builder

	// Sort captured names for consistent output
	names := make([]string, 0, len(pendingCaptures))

	for name, info := range pendingCaptures {
		if !info.used {
			names = append(names, name)
		}
	}

	sort.Strings(names)

	wasInLambda := false

	if v.lambdaCapture != nil {
		wasInLambda = v.lambdaCapture.conversionInLambda
	}

	for _, name := range names {
		info := pendingCaptures[name]

		decls.WriteString(v.newline)
		decls.WriteString(v.indent(v.indentLevel))

		// "var" vs. explicit type
		if v.options.preferVarDecl {
			decls.WriteString("var ")
		} else {
			decls.WriteString(v.getCSharpTypeName(info.varType))
			decls.WriteRune(' ')
		}

		// The left-hand side is the new capture name, e.g. "fʗ1"
		decls.WriteString(info.copyIdent.Name)
		decls.WriteString(" = ")

		// Temporarily turn off in-lambda logic so we don't map back to fʗ1
		if v.lambdaCapture != nil {
			v.lambdaCapture.conversionInLambda = false
		}

		// Lookup the *types.Var for the original ident:
		obj := v.info.Uses[info.origIdent]

		// Fallback to original .Name if anything is missing
		outerName := info.origIdent.Name

		// If it’s a Var, see if we have a final overshadowed name in varNames
		if varObj, ok := obj.(*types.Var); ok {
			if shadowedName, found := v.varNames[varObj]; found {
				outerName = shadowedName
			}
		}

		// A NESTED lambda declares its capture snapshot INSIDE an enclosing lambda's body, where the
		// captured variable has ALREADY been renamed to that enclosing lambda's OWN capture name. The
		// declaration RHS must read the enclosing name — testing/fuzz.go's `run` closure captures the
		// heap-boxed ref-local `fn` (`var fnʗ1 = fn;` before run), so run's inner
		// `go tRunner(t, func(t){ … fn.Call(args) })` must snapshot run's `fnʗ1`, not the outer method's
		// ref-local `fn` — a ref local is uncapturable inside a closure (CS8175). Walk the conversion
		// stack from the top (deepest) outward: pass-through levels (a go/defer stmt's own
		// enterLambdaConversion) carry an empty currentLambdaVars, so skip them and use the FIRST
		// enclosing lambda that actually renamed this variable. The object guard rejects a same-named
		// shadow that is a DIFFERENT variable. Stop at the function level (conversionInLambda false).
		for i := len(v.lambdaCapture.conversionStack) - 1; i >= 0; i-- {
			enclosing := v.lambdaCapture.conversionStack[i]
			if !enclosing.conversionInLambda {
				break
			}
			if enclosingName, ok := enclosing.currentLambdaVars[info.origIdent.Name]; ok {
				// Skip this capture's OWN owner state. pendingCaptures is shared across a function's
				// lambdas, so an OUTER lambda's snapshot can be generated while converting an INNER
				// func-literal ARGUMENT (`go dnsWaitGroupDone(ch, func(){})`, net lookup.go): the
				// outer (owner) lambda's state is then on the stack with a rename EQUAL to the name
				// being declared, and adopting it would emit `var xʗN = xʗN;` (self-reference). Keep
				// walking outward to the owner's true enclosing scope, where the RHS is valid.
				if enclosingName == info.copyIdent.Name {
					continue
				}
				if capturedObj, hasObj := enclosing.currentLambdaVarObjs[info.origIdent.Name]; !hasObj || capturedObj == obj {
					outerName = enclosingName
				}
				break
			}
		}

		// Restore old lambda state
		if v.lambdaCapture != nil {
			v.lambdaCapture.conversionInLambda = wasInLambda
		}

		// Now print e.g. "fʗ1 = fΔ1;"
		decls.WriteString(getSanitizedIdentifier(outerName))
		decls.WriteString(";")

		info.used = true
	}

	decls.WriteString(v.newline)
	decls.WriteString(v.indent(v.indentLevel))

	return decls.String()
}

// Determine if a variable should be captured
func (v *Visitor) shouldCapture(varObj *types.Var, ident *ast.Ident) bool {
	// Don't capture variables declared inside the lambda
	if v.info.Defs[ident] != nil {
		return false
	}

	// Only capture if:
	// 1. Declared outside but used inside the lambda
	// 2. Is a reference type OR address is taken
	declaredOutside := false
	for _, scope := range v.scopeStack {
		if scope[ident.Name] == varObj {
			declaredOutside = true
			break
		}
	}

	if !declaredOutside {
		return false
	}

	// A package-level global is a C# static field/property — a closure references it LIVE, so it is
	// never snapshot-captured. A value snapshot (`var gʗ1 = g`) copies the struct (so `&gʗ1` has no
	// box → CS0103, and writes through the global are lost), and is semantically wrong: Go reads/writes
	// the live global from inside a closure. The static is always in scope, so no capture is needed.
	if varObj.Pkg() != nil && varObj.Parent() == varObj.Pkg().Scope() {
		return false
	}

	// Don't capture value types unless they need to escape
	if _, ok := varObj.Type().Underlying().(*types.Basic); ok {
		if escapes, exists := v.identEscapesHeap[varObj]; exists {
			return escapes
		}
		return false
	}

	return true
}

// Check if a name conflicts with any function in scope
// shadowsCalledBuiltin reports whether a local variable declared as name would shadow a
// Go built-in function that is actually *called* within the current function. Go permits
// this (the built-in stays resolvable as the predeclared identifier), but in C# the
// built-in is a `using static go.builtin` method that a same-named local would shadow,
// breaking the call. Detecting an actual call (rather than any local matching a built-in
// name) keeps the rename — and thus golden churn — limited to the cases that need it.
func (v *Visitor) shadowsCalledBuiltin(name string) bool {
	// Only built-in functions can be shadowed this way (not predeclared types/consts like
	// int, true, nil): a built-in resolves to *types.Builtin in the universe scope.
	if obj := types.Universe.Lookup(name); obj == nil {
		return false
	} else if _, ok := obj.(*types.Builtin); !ok {
		return false
	}

	if v.currentFuncDecl == nil {
		return false
	}

	var found bool

	ast.Inspect(v.currentFuncDecl, func(n ast.Node) bool {
		if found {
			return false
		}

		callExpr, ok := n.(*ast.CallExpr)

		if !ok {
			return true
		}

		if ident, ok := callExpr.Fun.(*ast.Ident); ok && ident.Name == name {
			if _, isBuiltin := v.info.Uses[ident].(*types.Builtin); isBuiltin {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

func (v *Visitor) isFunctionNameInScope(name string) bool {
	// Check package scope for the name
	obj := v.pkg.Scope().Lookup(name)
	if obj == nil {
		return false
	}

	// Only proceed if it's a function from our package
	funcObj, ok := obj.(*types.Func)
	if !ok || funcObj.Pkg() != v.pkg {
		return false
	}

	// Now we need to analyze the current function's scope to see if:
	// 1. The function is used before any variable declaration with the same name
	// 2. The function is used as a value (delegate) rather than called
	if v.currentFuncDecl == nil {
		return false // Not in a function scope
	}

	var foundUsageBeforeDecl bool
	var foundDelegateUsage bool

	// Track parent node during traversal
	var parent ast.Node

	ast.Inspect(v.currentFuncDecl, func(n ast.Node) bool {
		if n == nil {
			return true
		}

		switch node := n.(type) {
		case *ast.CallExpr:
			// Save as parent for checking identifiers
			parent = node
		case *ast.Ident:
			if node.Name == name {
				// Check if this identifier refers to our function
				if obj, ok := v.info.Uses[node].(*types.Func); ok && obj == funcObj {
					// Check if it's part of a call expression
					if _, ok := parent.(*ast.CallExpr); ok {
						// The object resolution already proves Go bound this ident to the
						// FUNCTION — possible only where the same-named local is not yet in
						// scope: before its declaration, or WITHIN its own initializer
						// (`signame := signame(gp.sig)`, runtime panic.go — Go starts the
						// shadow AFTER the initializer, but C# scopes the local over its own
						// initializer, so the call would bind the string local: CS0149). The
						// old position guard (`call before the declaration`) excluded exactly
						// that initializer case.
						foundUsageBeforeDecl = true
					} else {
						// Used as a value rather than called
						foundDelegateUsage = true
					}
				}
			}
		default:
			// Save as parent for next iteration
			parent = node
		}
		return true
	})

	return foundUsageBeforeDecl || foundDelegateUsage
}

// enterDeferGoLambdaConversion enters the conversion state for a defer/go STATEMENT — a
// pass-through level whose call ARGUMENTS (and non-literal callee expression) are evaluated
// EAGERLY in the ENCLOSING scope when the statement executes (Go spec: the deferred/spawned
// function value and its parameters are evaluated at statement time). enterLambdaConversion
// installs EMPTY remap state, which hid an enclosing lambda's capture renames while the
// arguments rendered: an eager argument reading a heap-boxed local the enclosing IIFE
// snapshot-captured (`base` → `baseʗ1`) emitted the raw ref-local `@base` inside the enclosing
// lambda's body (CS8175 — a ref local cannot be used in an anonymous method; where the raw
// name IS capturable it instead bypasses the snapshot every other read in that body uses).
// Seed the fresh state with a COPY of the enclosing lambda's renames so arguments render
// exactly like any other expression in the enclosing body; prepareStmtCaptures then OVERRIDES
// entries for the statement's own captured callee idents (their defer-time snapshots). Copies,
// not the maps themselves: the statement's own remaps must not leak back into the enclosing
// state when exit restores it. At function level (no enclosing lambda) this is exactly
// enterLambdaConversion.
func (v *Visitor) enterDeferGoLambdaConversion(node ast.Node) {
	enclosingInLambda := v.lambdaCapture.conversionInLambda
	enclosingVars := v.lambdaCapture.currentLambdaVars
	enclosingVarObjs := v.lambdaCapture.currentLambdaVarObjs

	v.enterLambdaConversion(node)

	if !enclosingInLambda {
		return
	}

	for name, captureName := range enclosingVars {
		v.lambdaCapture.currentLambdaVars[name] = captureName
	}

	for name, obj := range enclosingVarObjs {
		v.lambdaCapture.currentLambdaVarObjs[name] = obj
	}
}

// identIsCurrentFuncLitParam reports whether ident resolves to a parameter of the function
// literal whose conversion is currently innermost (see enterLambdaConversion) — i.e. the
// ident is one of the converting lambda's OWN parameters, a plain local of that lambda.
func (v *Visitor) identIsCurrentFuncLitParam(ident *ast.Ident) bool {
	if v.lambdaCapture == nil {
		return false
	}

	funcLit, ok := v.lambdaCapture.currentConversion.(*ast.FuncLit)

	if !ok {
		return false
	}

	return v.funcLitOwnsParam(funcLit, v.info.ObjectOf(ident))
}

// funcLitOwnsParam reports whether varObj is declared as one of funcLit's own parameters.
func (v *Visitor) funcLitOwnsParam(funcLit *ast.FuncLit, varObj types.Object) bool {
	if funcLit.Type.Params == nil {
		return false
	}

	for _, field := range funcLit.Type.Params.List {
		for _, ident := range field.Names {
			if v.info.ObjectOf(ident) == varObj {
				return true
			}
		}
	}

	return false
}
