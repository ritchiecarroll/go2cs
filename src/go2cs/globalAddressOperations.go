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

	// Likewise the pointer-receiver method values that test half takes on a production var
	// (siblingTestMethodValues), resolved against the real types by the scan's own rule.
	for _, candidate := range siblingTestMethodValues {
		if varObj := siblingMethodValueRoot(pkg, candidate); varObj != nil {
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
	// other flavours by its own in-package calls), nothing else. The same arm also boxes every exported
	// value-kind var of an importable package, the cross-package address of a variable
	// (docs/phase4/DESIGN-crosspkg-exported-box.md).
	astFiles := make([]*ast.File, 0, len(files))

	for _, fileEntry := range files {
		astFiles = append(astFiles, fileEntry.file)
	}

	markExportedBoxOnlyGlobals(pkg, astFiles, packageAddressedGlobals)
}

// scanAddressedGlobals records into `into` every package-level value var of `pkg` that `file` takes
// the address of (via &g, &g.field, &g[i]), calls a capture-mode / pointer-receiver method on, or
// takes a pointer-receiver method VALUE of (`f := g.inc`, which is `(&g).inc`). It is
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

		case *ast.SelectorExpr:
			// A pointer-receiver METHOD VALUE on a package-level value var (`f := g.inc`,
			// `apply(gh.t.inc)`, `ga[1].inc`) binds `&g` at evaluation, so g needs its box exactly as
			// for `&g`: otherwise the emission falls back to `Ꮡ(g).inc`, a box of a COPY, and every
			// write through the method value is lost. (A CALL of the same selector is the CallExpr
			// arm's; this arm marks nothing for it that the call arm does not.)
			if varObj := methodValueStorageRoot(node, info); varObj != nil && varObj.Parent() == pkg.Scope() {
				into[varObj] = true
			}
		}

		return true
	})
}

// methodValueStorageRoot returns the variable whose storage the method value `selectorExpr` takes
// the address of — the root of `g.m`, `g.f.m` or `g[i].m` through value fields and ARRAY indexes —
// when the selection is a pointer-receiver method on addressable value storage. Nil otherwise: a
// value receiver copies; a pointer operand, a field reached through a pointer, an embedded pointer
// on the path (selection.Indirect) and a slice element all already address the real storage.
func methodValueStorageRoot(selectorExpr *ast.SelectorExpr, info *types.Info) *types.Var {
	selection := info.Selections[selectorExpr]

	if selection == nil || selection.Kind() != types.MethodVal || selection.Indirect() {
		return nil
	}

	if signature, ok := selection.Obj().Type().(*types.Signature); !ok || signature.Recv() == nil {
		return nil
	} else if _, pointerMethod := signature.Recv().Type().(*types.Pointer); !pointerMethod {
		return nil
	}

	operand := ast.Unparen(selectorExpr.X)

	for {
		operandType := info.TypeOf(operand)

		if operandType == nil {
			return nil
		}

		if _, isPtr := operandType.Underlying().(*types.Pointer); isPtr {
			return nil
		}

		switch expr := operand.(type) {
		case *ast.Ident:
			varObj, _ := info.Uses[expr].(*types.Var)
			return varObj
		case *ast.SelectorExpr:
			field := info.Selections[expr]

			if field == nil {
				// A qualified identifier, `pkg.V`.
				varObj, _ := info.Uses[expr.Sel].(*types.Var)
				return varObj
			}

			if field.Kind() != types.FieldVal || field.Indirect() {
				return nil
			}

			operand = ast.Unparen(expr.X)
		case *ast.IndexExpr:
			if baseType := info.TypeOf(expr.X); baseType == nil {
				return nil
			} else if _, isArray := baseType.Underlying().(*types.Array); !isArray {
				return nil
			}

			operand = ast.Unparen(expr.X)
		default:
			return nil
		}
	}
}

// siblingMethodValueRoot resolves one siblingTestMethodValues candidate against the production types
// by methodValueStorageRoot's rule: the package-level var it names when each step is a value field or
// an array index and the method is a pointer-receiver method of the operand's addressable value; nil
// for any other shape (a local, a func, a value receiver, a pointer on the path, an interface method).
func siblingMethodValueRoot(pkg *types.Package, candidate siblingMethodValue) *types.Var {
	varObj, ok := pkg.Scope().Lookup(candidate.root).(*types.Var)

	if !ok {
		return nil
	}

	operandType := varObj.Type()

	for _, step := range candidate.path {
		if _, isPtr := operandType.Underlying().(*types.Pointer); isPtr {
			return nil
		}

		if step == siblingIndexStep {
			array, isArray := operandType.Underlying().(*types.Array)

			if !isArray {
				return nil
			}

			operandType = array.Elem()
			continue
		}

		obj, _, indirect := types.LookupFieldOrMethod(operandType, true, pkg, step)
		field, isField := obj.(*types.Var)

		if !isField || indirect {
			return nil
		}

		operandType = field.Type()
	}

	if _, isPtr := operandType.Underlying().(*types.Pointer); isPtr {
		return nil
	}

	obj, _, indirect := types.LookupFieldOrMethod(operandType, true, pkg, candidate.method)
	method, isMethod := obj.(*types.Func)

	if !isMethod || indirect {
		return nil
	}

	if signature, ok := method.Type().(*types.Signature); !ok || signature.Recv() == nil {
		return nil
	} else if _, pointerMethod := signature.Recv().Type().(*types.Pointer); !pointerMethod {
		return nil
	}

	return varObj
}

// markExportedBoxOnlyGlobals is the owner arm described in collectAddressedGlobals. It boxes an EXPORTED
// package-level var on two rules, both decided from the owner's own types and syntax, so an importer
// (importedGlobalIsBoxed) runs this same function and agrees by construction:
//   - a var whose pointer method set carries a box-only method (the 2026-09-03 rule above), whatever
//     package declares it;
//   - every exported VALUE-kind var of an importable package (go.* 1.26.10.2's headline,
//     docs/phase4/DESIGN-crosspkg-exported-box.md): Go hands an importer the variable itself, so `&pkg.V`,
//     a pointer-receiver method value and a field or element pointer must reach it, and only the owner can
//     declare the box. Not a pointer or an interface (the value kind COORD ruled, OQ-1), not `package main`
//     (nothing can import it), and not a linkname FORWARD (see linknameForwardVars): its storage is
//     another package's, and a box would sever Go's alias.
func markExportedBoxOnlyGlobals(pkg *types.Package, files []*ast.File, into map[types.Object]bool) {
	var forwards map[string]bool

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
			continue
		}

		if pkg.Name() == "main" {
			continue
		}

		if _, isInterface := types.Unalias(varObj.Type()).Underlying().(*types.Interface); isInterface {
			continue
		}

		if forwards == nil {
			forwards = linknameForwardVars(pkg, files)
		}

		if !forwards[name] {
			into[varObj] = true
		}
	}
}

// linknameForwardVars names the package-level vars of `pkg` whose storage is ANOTHER symbol's, which
// visitValueSpec emits as a forwarding property and never as a field of their own: a two-argument
// `//go:linkname local remote` pull (varLinknamePull), and the inverted alias the registry records
// (linknameVarAliasTargets: internal/syscall/windows.CanUseLongPaths -> runtime.canUseLongPaths). A
// one-argument handle (`//go:linkname X86`) names the var's OWN storage and is not a forward.
func linknameForwardVars(pkg *types.Package, files []*ast.File) map[string]bool {
	forwards := map[string]bool{}

	for _, file := range files {
		for _, group := range file.Comments {
			for _, comment := range group.List {
				if fields := strings.Fields(comment.Text); len(fields) == 3 && fields[0] == "//go:linkname" {
					forwards[fields[1]] = true
				}
			}
		}
	}

	prefix := pkg.Path() + "."

	for key := range linknameVarAliasTargets {
		if name, ok := strings.CutPrefix(key, prefix); ok && !strings.Contains(name, ".") {
			forwards[name] = true
		}
	}

	return forwards
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

			markExportedBoxOnlyGlobals(owner, loaded.Syntax, boxed)
		}

		importedAddressedGlobals[owner] = boxed
	}

	return boxed[varObj]
}

// importedBoxedGlobal reports whether expr is a package-qualified selector `pkg.V` naming ANOTHER
// package's package-level value var whose owner emission declares its box (importedGlobalIsBoxed), so
// `&pkg.V` is `pkg.ᏑV`. The importer's field and element arms root an address inside the var on it.
func (v *Visitor) importedBoxedGlobal(expr ast.Expr) bool {
	selectorExpr, ok := expr.(*ast.SelectorExpr)

	if !ok || v.selectorBasePackageObj(selectorExpr) == nil {
		return false
	}

	varObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Var)

	if !ok || varObj.Pkg() == nil || varObj.Parent() != varObj.Pkg().Scope() {
		return false
	}

	if _, isPtr := types.Unalias(varObj.Type()).(*types.Pointer); isPtr {
		return false
	}

	return importedGlobalIsBoxed(varObj)
}

// rootsAtImportedBoxedGlobal reports whether expr is `pkg.V`, or a chain of VALUE field selectors over
// it (`pkg.V.f.g`), with pkg.V an importedBoxedGlobal: the storage expr names lives inside the owner's
// box, so its address is a field reference through `pkg.ᏑV`, never a `Ꮡ(value)` copy. Any pointer hop
// ends the walk: beyond a pointer the storage already has a real address of its own.
func (v *Visitor) rootsAtImportedBoxedGlobal(expr ast.Expr) bool {
	for {
		if v.importedBoxedGlobal(expr) {
			return true
		}

		selectorExpr, ok := expr.(*ast.SelectorExpr)

		if !ok {
			return false
		}

		selection, ok := v.info.Selections[selectorExpr]

		if !ok || selection.Kind() != types.FieldVal || selection.Indirect() {
			return false
		}

		if t := v.info.TypeOf(selectorExpr.X); t != nil {
			if _, isPtr := t.Underlying().(*types.Pointer); isPtr {
				return false
			}
		}

		expr = selectorExpr.X
	}
}

// writeAddressedGlobalDecl emits a package-level var that is backed by a heap box so
// `&global` (emitted as the "Ꮡname" identifier) references the original storage. The
// box holds the value; the var name becomes a ref-returning property over the box, so
// reads/writes of the global are unchanged. An empty initExpr defaults the value.
func (v *Visitor) writeAddressedGlobalDecl(access, csTypeName, csIDName, initExpr string, valueIsRefLike bool) {
	boxDecl, refProperty := addressedGlobalDecl(access, csTypeName, csIDName, initExpr, valueIsRefLike)

	v.writeOutput("%s", boxDecl)
	v.outputBuilder.WriteString(v.newline)
	v.writeOutput("%s", refProperty)
}

// addressedGlobalDecl is writeAddressedGlobalDecl's two lines: the box field and the var's `ref` property over it. A lazy
// addressed var (writeLazyAddressedPackageVar) writes the same two, the box field inside its holder.
func addressedGlobalDecl(access, csTypeName, csIDName, initExpr string, valueIsRefLike bool) (boxDecl, refProperty string) {
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

	// The property reads `ValueSlot`, the real slot with no nil check, for every package-level box. A
	// package box is never nil (it is constructed here with a value), and for a value-typed T the strict
	// `Value`'s IsNull is constant false, so the two read the same storage. For a REFERENCE-LIKE valued
	// global (`var head *node`) the held value may legitimately be nil (Go reads a nil pointer global
	// freely; only DEREFERENCING it panics), which is why it read `ValueSlot` before every box did, and
	// why valueIsRefLike once chose the accessor.

	// THE construction-position emitter (B2-I2's role split; see BoxConstructPrefix in symbols.json): the
	// box is constructed as the PACKAGE-LEVEL kind, golib's StaticBox<T> (PackageBoxConstructPrefix), a
	// StandardBox<T> that also answers Go's non-heap question: a package-level sync.WaitGroup reads
	// Unbubbled in a synctest bubble, as in Go.
	//
	// Its field is `static readonly`, still DECLARED `ж<T>` so `&X` keeps that type (`var p = ᏑX;` may later
	// be re-pointed, `p = Ꮡ(y)`), and the property reads it as the sealed StaticBox<T>'s `ValueSlot`
	// (DESIGN-crosspkg-exported-box.md section 5, the read A/B in docs/phase4/probes/g-crosspkg-read-ab).
	// An exported var read in a hot loop cost 16x a field on the JIT and 24x under Native AOT as `ж<T>`
	// read through `Value`: the read is a virtual call, and Value's IsNull branch keeps AOT from inlining
	// it. `readonly` lets tier-1 see the exact object, and the cast to the sealed type lets AOT inline
	// `ValueSlot` too: 3.1x a field on the JIT, 2.5x under AOT, and +2-3% on encoding/binary's
	// LittleEndian.Uint32. Without the cast, AOT stays at 7x. The cast cannot fail, since the field is
	// constructed as a StaticBox<T> right here and is never rebound: nothing assigns a package box after
	// its initializer, and an assignment that did would be CS0198 on the readonly field.
	boxDecl = fmt.Sprintf("%s static readonly %s<%s> %s = new %s<%s>(%s);", access, PointerPrefix, csTypeName, box, PackageBoxConstructPrefix, csTypeName, initExpr)
	refProperty = fmt.Sprintf("%s static ref %s %s => ref ((%s<%s>)%s).ValueSlot;", access, csTypeName, csIDName, PackageBoxConstructPrefix, csTypeName, box)

	return boxDecl, refProperty
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
