// globalAddressOperations.go - Gbtc
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

	"golang.org/x/tools/go/packages"
)

// packageAddressedGlobals holds the package-level value vars whose address is taken
// somewhere in the package (via &g, &g.field, or &g[i]). Such a global must be backed
// by a heap box (ж<T>) so the pointer references the original storage rather than a
// copy — `Ꮡ(value)` heap-allocates a copy, which silently breaks `&global` mutation.
// Populated by a synchronous pre-pass over all files (so cross-file address-taking is
// visible at the global's declaration), then read-only during concurrent file visiting.
// Keyed by the var's types.Object, which is interned per variable across files.
var packageAddressedGlobals map[types.Object]bool

// collectAddressedGlobals scans every file for address-of expressions rooted at a
// package-level var and records those vars in packageAddressedGlobals. It also records a
// package-level value var on which a capture-mode method is called (`var locked atomic.Int32;
// locked.CompareAndSwap(...)`) — such a method needs the receiver box (`Ꮡlocked`), which the
// heap-box backing supplies and convSelectorExpr routes the call through, exactly as for an
// explicitly address-taken global. Runs after collectCaptureModeMethods, so the capture-mode
// set is populated.
func collectAddressedGlobals(files []FileEntry, pkg *types.Package, info *types.Info) {
	// Seed with the globals the package's own in-package `_test.go` half addresses. Those files are
	// absent from a production go/packages load, but their `&g` still has to alias g's real storage,
	// and only the production emission can declare the box (siblingTestAddressedGlobalNames). The
	// scan is name-based and type-check-free, so resolve each candidate here — anything that is not
	// a package-level var in the real scope is dropped.
	for _, name := range siblingTestAddressedGlobalNames {
		if varObj, ok := pkg.Scope().Lookup(name).(*types.Var); ok {
			packageAddressedGlobals[varObj] = true
		}
	}

	for _, fileEntry := range files {
		scanAddressedGlobals(fileEntry.file, pkg, info, packageAddressedGlobals)
	}

	// The OWNER arm of the cross-package box binding (the darwin run layer, 2026-09-03): an EXPORTED
	// package-level value var whose pointer method set carries a capture-mode / direct-box method is
	// boxed even when nothing in this package addresses it. Such a method is emitted with only a ж<T>
	// receiver, so an IMPORTER's `pkg.V.M()` has no in-place form — the box is the only correct
	// target, and the owner is the only emission that can declare it. Measured before the rule: a
	// library `var Plain sync.RWMutex` it never touches emitted a plain static field, and an importer's
	// `lib.Plain.RLock(); lib.Plain.RUnlock()` (the copy form) fataled `sync: RUnlock of unlocked
	// RWMutex`. Corpus footprint at the rule's landing: syscall.ForkLock on windows (boxed on the
	// other flavours by its own in-package calls), nothing else.
	markExportedBoxOnlyGlobals(pkg, packageAddressedGlobals)
}

// scanAddressedGlobals records into `into` every package-level value var of `pkg` that `file` takes
// the address of (via &g, &g.field, &g[i]) or calls a capture-mode / pointer-receiver method on. It is
// the per-file half of collectAddressedGlobals, split out so the SAME predicate can be run over an
// IMPORTED package's syntax (importedGlobalIsBoxed) — an importer must bind the owner's box exactly
// when the owner's own emission declares one.
func scanAddressedGlobals(file *ast.File, pkg *types.Package, info *types.Info, into map[types.Object]bool) {
	ast.Inspect(file, func(n ast.Node) bool {
		switch node := n.(type) {
		case *ast.UnaryExpr:
			if node.Op != token.AND {
				return true
			}

			// Peel field selectors and index expressions down to the root operand,
			// e.g. &G.X or &G[i] both make G escape.
			root := node.X

			for {
				switch expr := root.(type) {
				case *ast.SelectorExpr:
					root = expr.X
					continue
				case *ast.IndexExpr:
					root = expr.X
					continue
				case *ast.ParenExpr:
					root = expr.X
					continue
				}

				break
			}

			if ident, ok := root.(*ast.Ident); ok {
				if varObj, ok := info.Uses[ident].(*types.Var); ok && varObj.Parent() == pkg.Scope() {
					into[varObj] = true
				}
			}

		case *ast.CallExpr:
			// A capture-mode / direct-ж method called on a package-level value global — directly
			// (`var locked atomic.Int32; locked.CompareAndSwap(…)`) or on a value FIELD of one
			// (`prof.signalLock.Store(…)`, `Δscavenge.gcPercentGoal.Store(…)`) — needs that global
			// heap-boxed so the receiver box (`Ꮡprof` → `Ꮡprof.of(T.Ꮡfield)`) exists and the call
			// routes through it. Such a method is emitted with only a `ж<T>` (box) receiver, so a
			// plain value/ref of the field cannot bind it (CS1929).
			selectorExpr, ok := node.Fun.(*ast.SelectorExpr)

			if !ok {
				return true
			}

			// Peel value field selectors to the receiver root. Bail at a pointer hop: beyond a
			// pointer the field address is already real (no global boxing needed), and that path
			// is intentionally NOT handled here to avoid disturbing pointer-receiver/param fields.
			recv := selectorExpr.X

			for {
				if t := info.TypeOf(recv); t != nil {
					if _, isPtr := t.Underlying().(*types.Pointer); isPtr {
						return true
					}
				}

				switch r := recv.(type) {
				case *ast.SelectorExpr:
					recv = r.X
					continue
				case *ast.IndexExpr:
					// A package-level value ARRAY whose element has a pointer-receiver method called on
					// it (`matchPool[i].Get()`, regexp's [N]sync.Pool) needs the array boxed so the
					// element address `Ꮡmatchpool.at<T>(i)` resolves (CS0103).
					recv = r.X
					continue
				}

				break
			}

			ident, ok := recv.(*ast.Ident)

			if !ok {
				return true
			}

			varObj, ok := info.Uses[ident].(*types.Var)

			if !ok || varObj.Parent() != pkg.Scope() {
				return true
			}

			// A pointer global already carries its box; only value globals need boxing.
			if _, isPtr := varObj.Type().(*types.Pointer); isPtr {
				return true
			}

			funcObj, ok := info.ObjectOf(selectorExpr.Sel).(*types.Func)

			if !ok || funcObj == nil {
				return true
			}

			// Box for a capture-mode method (known same-package) OR any pointer-receiver method —
			// the latter covers cross-package atomic methods (`func (x *Uint32) Store`), whose
			// capture-mode status is not in this package's set but which are likewise ж-only.
			shouldBox := packageCaptureModeMethods != nil && packageCaptureModeMethods[funcObj.Origin()]

			if !shouldBox {
				if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Recv() != nil {
					_, shouldBox = sig.Recv().Type().(*types.Pointer)
				}
			}

			if shouldBox {
				into[varObj] = true
			}
		}

		return true
	})
}

// markExportedBoxOnlyGlobals is the owner arm described in collectAddressedGlobals.
func markExportedBoxOnlyGlobals(pkg *types.Package, into map[types.Object]bool) {
	for _, name := range pkg.Scope().Names() {
		varObj, ok := pkg.Scope().Lookup(name).(*types.Var)

		if !ok || !varObj.Exported() || into[varObj] {
			continue
		}

		if _, isPtr := types.Unalias(varObj.Type()).(*types.Pointer); isPtr {
			continue
		}

		if typeHasBoxOnlyPointerMethod(varObj.Type()) {
			into[varObj] = true
		}
	}
}

// typeHasBoxOnlyPointerMethod reports whether *T's method set carries a method the emission gives
// only a ж<T> receiver (capture-mode / direct-box — see captureModeOperations.go).
func typeHasBoxOnlyPointerMethod(t types.Type) bool {
	mset := types.NewMethodSet(types.NewPointer(t))

	for i := 0; i < mset.Len(); i++ {
		if fn, ok := mset.At(i).Obj().(*types.Func); ok {
			origin := fn.Origin()

			if packageCaptureModeMethods[origin] || packageDirectBoxReceiverMethods[origin] {
				return true
			}
		}
	}

	return false
}

// currentLoadedPackage is the root of the loaded package graph for the conversion in progress,
// recorded by collectCaptureModeMethods; importedAddressedGlobals caches, per imported package, the
// result of running scanAddressedGlobals + markExportedBoxOnlyGlobals over THAT package's syntax —
// i.e. which of its package-level value vars its own emission backs with a heap box (ᏑName).
var currentLoadedPackage *packages.Package
var importedAddressedGlobals = map[*types.Package]map[types.Object]bool{}

// importedGlobalIsBoxed reports whether the package-level value var `varObj` of an IMPORTED package
// is emitted with a heap box by its owner, so a cross-package `&pkg.V` — written, or synthesized for
// a pointer-receiver call — can bind `pkg.ᏑV` instead of boxing a copy. Deterministic: the owner's
// own predicate over the owner's own syntax, which LoadAllSyntax carries for every dependency (the
// same handle collectCaptureModeMethods walks). False when the syntax is not reachable, which keeps
// the previous emission.
func importedGlobalIsBoxed(varObj *types.Var) bool {
	owner := varObj.Pkg()

	if owner == nil || currentLoadedPackage == nil {
		return false
	}

	packageLock.Lock()
	defer packageLock.Unlock()

	boxed, cached := importedAddressedGlobals[owner]

	if !cached {
		boxed = map[types.Object]bool{}

		if loaded := loadedPackageSource(currentLoadedPackage, owner); loaded != nil && loaded.TypesInfo != nil {
			for _, file := range loaded.Syntax {
				scanAddressedGlobals(file, owner, loaded.TypesInfo, boxed)
			}

			markExportedBoxOnlyGlobals(owner, boxed)
		}

		importedAddressedGlobals[owner] = boxed
	}

	return boxed[varObj]
}

// writeAddressedGlobalDecl emits a package-level var that is backed by a heap box so
// `&global` (emitted as the "Ꮡname" identifier) references the original storage. The
// box holds the value; the var name becomes a ref-returning property over the box, so
// reads/writes of the global are unchanged. An empty initExpr defaults the value.
func (v *Visitor) writeAddressedGlobalDecl(access, csTypeName, csIDName, initExpr string, valueIsRefLike bool) {
	// A KEYWORD-named global (`var null = …`, net/rpc/jsonrpc) arrives keyword-escaped
	// (`@null`); composed after the marker glyph the escape is INTERIOR (`Ꮡ@null`), which
	// lexes as two tokens. The glyph prefix already de-keywords the identifier, so strip
	// the escape — matching every use-site composition (boxBaseName / convIdent).
	box := AddressPrefix + strings.TrimPrefix(csIDName, "@")

	if len(initExpr) == 0 {
		// Use an explicitly typed default so the ж(in T value) constructor is chosen
		// (a bare `default` would bind to the ж(NilType) ctor and yield a nil box).
		initExpr = fmt.Sprintf("default(%s)", csTypeName)
	}

	// A REFERENCE-LIKE valued global (`var head *node`) reads the HELD value through the box,
	// which may legitimately be nil (Go reads a nil pointer global freely; only DEREFERENCING
	// it panics). The strict `val` nil-checks the slot, so the property reads `ValueSlot`
	// (the identical real slot, no check); a plain value global keeps the strict `val`.
	accessor := "Value"

	if valueIsRefLike {
		accessor = "ValueSlot"
	}

	// THE construction-position emitter (B2-I2's role split; see BoxConstructPrefix in
	// symbols.json): the DECLARED type here is PointerPrefix and never changes; the target-typed
	// `new(...)` constructs the box, and B2-I3 rewrites it to name the concrete standard-kind
	// class — `new BoxConstructPrefix<T>(...)` — when the base goes abstract. This line is the
	// 754-site class of the design's emission-priced radius.
	v.writeOutput("%s static %s<%s> %s = new %s<%s>(%s);", access, PointerPrefix, csTypeName, box, BoxConstructPrefix, csTypeName, initExpr)
	v.outputBuilder.WriteString(v.newline)
	v.writeOutput("%s static ref %s %s => ref %s.%s;", access, csTypeName, csIDName, box, accessor)
}

// isAddressedGlobal reports whether the identifier resolves to a package-level var
// whose address is taken in the package (and so is backed by a heap box).
func (v *Visitor) isAddressedGlobal(ident *ast.Ident) bool {
	if ident == nil || packageAddressedGlobals == nil {
		return false
	}

	obj := v.info.Uses[ident]

	if obj == nil {
		obj = v.info.Defs[ident]
	}

	return obj != nil && packageAddressedGlobals[obj]
}
