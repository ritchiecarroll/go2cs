// initOrderOperations.go - Gbtc
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
	"math"
	"os"
	"path/filepath"
	"sort"
	"strings"
)

// PackageInitFileName is the synthetic per-package file that carries the ordered static
// constructor for package-level vars whose initialization order cannot be reproduced by C#'s
// static-field-initializer execution order (see packageMovedInitVars). Named to not collide with
// any converted `<gofile>.cs` (no stdlib Go source file is named package_init.go).
const PackageInitFileName = "package_init.cs"

// packageMovedInitVars holds the package-level vars whose initializer must be RELOCATED into an
// ordered static constructor, mapped to their InitOrder ordinal. Go initializes package-level vars
// in dependency order (types.Info.InitOrder); C# runs static field initializers in textual order
// WITHIN a class part and in an unspecified order ACROSS parts (separate source files). So an
// initializer whose Go dependencies include a package var that is declared in a DIFFERENT file
// (cross-part order is undefined — syscall's `Stdin = getStdHandle(…)` read zsyscall_windows.go's
// `procGetStdHandle` while still nil), or LATER in the SAME file (a forward reference C# reads as
// the zero value), or that was itself relocated (its value is only assigned in the ctor, after
// every field initializer) cannot stay a C# field initializer. Those vars are emitted as BARE
// fields plus a tiny `initᴛ<name>()` method IN THEIR HOME FILE (so the rendered expression keeps
// the file's own using aliases), and a per-package `static <pkg>_package()` ctor (package_init.cs)
// calls the methods in InitOrder — which, because C# runs all static field initializers (every
// part) BEFORE any static-ctor body, is guaranteed to see every non-relocated dependency already
// initialized. Dependencies are resolved TRANSITIVELY through same-package function/method bodies
// and function literals, mirroring Go's own initialization-order analysis (spec: "references …
// directly or through function calls"). Keyed by the var's types.Object (interned per variable).
//
// A dependency is not only a package VAR. A Go CONSTANT whose C# form is an INITIALIZED FIELD —
// the string and GoBigConst residue that cannot be a get-only property
// (constEmittedAsInitializedField) — is an ordinary static field initializer too, so it carries
// the identical hazard even though Go treats it as compile-time and lists no initialization order
// for it at all. Every other const C# cannot declare `const` is emitted as a property and is
// order-free; see visitValueSpec.go's CONST arm.
var packageMovedInitVars map[types.Object]int

// packageMovedInitMethods maps each relocated initializer's InitOrder ordinal to the name of the
// per-file init method that performs the assignment; the package_init.cs ctor calls them in
// ordinal order.
var packageMovedInitMethods map[int]string

// packageTestInitHookMethod is the classic-partial-method hook a production package_init.cs
// static constructor ends with when the package converts under -tests: the test conversion
// IMPLEMENTS it (writeTestVariantInitFile) when the package's own _test.go files add relocated
// initializers to the SAME class (the internal-variant recompile model — a C# class gets ONE
// static ctor across all parts, so the test side cannot declare its own). Unimplemented, C#
// erases both the declaration and the call (classic partial method: void, no accessibility),
// so the production assembly — whose compile set excludes the *_test.cs implementation — is
// byte-for-byte unaffected at runtime. The doubled marker keeps every real relocated-var
// method name (`init<ᴛ><name>`) out of the collision space.
var packageTestInitHookMethod = "init" + TempVarMarker + TempVarMarker + "tests"

// collectMovedInitVars flags the package-level var initializers that must be relocated into the
// ordered static constructor (see packageMovedInitVars). It walks types.Info.InitOrder — Go's
// dependency-sorted list of package-level initializers — resolving each initializer's same-package
// var dependencies transitively through called/referenced package functions (and func-literal
// bodies, which Go's own analysis also treats as references), then flags the initializer when a
// dependency is cross-file, a same-file forward reference, or itself already flagged.
func collectMovedInitVars(fset *token.FileSet, pkg *types.Package, info *types.Info, syntax []*ast.File) {
	scope := pkg.Scope()

	// Per-function direct references: package-level vars read and package functions/methods
	// referenced by each function body in the package (methods included — their *types.Func is
	// the map key either way). Built over the FULL syntax set (including files skipped for
	// manual conversion — dependency analysis is about Go semantics, not emission).
	funcVarRefs := map[*types.Func]map[*types.Var]bool{}
	funcFuncRefs := map[*types.Func]map[*types.Func]bool{}
	funcConstRefs := map[*types.Func]map[*types.Const]bool{}

	collectRefs := func(node ast.Node, vars map[*types.Var]bool, funcs map[*types.Func]bool, consts map[*types.Const]bool) {
		ast.Inspect(node, func(n ast.Node) bool {
			ident, ok := n.(*ast.Ident)

			if !ok {
				return true
			}

			switch obj := info.Uses[ident].(type) {
			case *types.Var:
				if obj.Parent() == scope {
					vars[obj] = true
				}
			case *types.Func:
				funcs[obj] = true
			case *types.Const:
				// Only a package-scope const that is emitted as an INITIALIZED FIELD carries an
				// initialization-order hazard; a real C# `const` is folded by the compiler, and a
				// get-only property re-evaluates at every read — both are order-free (see
				// constEmittedAsInitializedField).
				if obj.Parent() == scope && constEmittedAsInitializedField(obj) {
					consts[obj] = true
				}
			}

			return true
		})
	}

	for _, file := range syntax {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Body == nil {
				continue
			}

			funcObj, ok := info.Defs[funcDecl.Name].(*types.Func)

			if !ok {
				continue
			}

			vars := map[*types.Var]bool{}
			funcs := map[*types.Func]bool{}
			consts := map[*types.Const]bool{}
			collectRefs(funcDecl.Body, vars, funcs, consts)
			funcVarRefs[funcObj] = vars
			funcFuncRefs[funcObj] = funcs
			funcConstRefs[funcObj] = consts
		}
	}

	// Transitive closure: all package vars reachable from a function (cycle-safe, memoized).
	closureCache := map[*types.Func]map[*types.Var]bool{}
	inProgress := map[*types.Func]bool{}

	var funcClosure func(fn *types.Func) map[*types.Var]bool

	funcClosure = func(fn *types.Func) map[*types.Var]bool {
		if cached, ok := closureCache[fn]; ok {
			return cached
		}

		if inProgress[fn] {
			return nil // recursion cycle — the outer walk already accumulates these refs
		}

		directVars, hasBody := funcVarRefs[fn]

		if !hasBody {
			return nil // imported/bodyless (asm) function — no same-package var deps
		}

		inProgress[fn] = true
		result := map[*types.Var]bool{}

		for varObj := range directVars {
			result[varObj] = true
		}

		for callee := range funcFuncRefs[fn] {
			for varObj := range funcClosure(callee) {
				result[varObj] = true
			}
		}

		delete(inProgress, fn)
		closureCache[fn] = result

		return result
	}

	// Same transitive closure over the INITIALIZED-FIELD CONSTS a function reaches. A Go constant
	// whose value is a string (`const labelPipe label = "pipe"`), or whose magnitude escapes to
	// GoBigConst, has neither a C# `const` form nor a property form — it is emitted as a
	// `static readonly` FIELD (see constEmittedAsInitializedField), so it is subject to exactly
	// the cross-part field-initializer ordering C# leaves undefined, just like a package var.
	constClosureCache := map[*types.Func]map[*types.Const]bool{}
	constInProgress := map[*types.Func]bool{}

	var constClosure func(fn *types.Func) map[*types.Const]bool

	constClosure = func(fn *types.Func) map[*types.Const]bool {
		if cached, ok := constClosureCache[fn]; ok {
			return cached
		}

		if constInProgress[fn] {
			return nil // recursion cycle — the outer walk already accumulates these refs
		}

		directConsts, hasBody := funcConstRefs[fn]

		if !hasBody {
			return nil // imported/bodyless (asm) function — no same-package const deps
		}

		constInProgress[fn] = true
		result := map[*types.Const]bool{}

		for constObj := range directConsts {
			result[constObj] = true
		}

		for callee := range funcFuncRefs[fn] {
			for constObj := range constClosure(callee) {
				result[constObj] = true
			}
		}

		delete(constInProgress, fn)
		constClosureCache[fn] = result

		return result
	}

	// Tier C's extra edge: does a function TRANSITIVELY reach a body that reads a hoisted
	// string-literal field? Those fields are ordinary C# static field initializers declared above
	// their first-using function, so they are subject to the very cross-part ordering C# leaves
	// undefined — a package-level var initializer that calls such a function could observe
	// `default(@string)` ("") instead of the literal. Relocating that initializer into the ordered
	// static ctor closes the class, because C# runs EVERY static field initializer (all parts)
	// before any static-ctor body. Same memoized, cycle-safe shape as funcClosure above; keyed on
	// the same funcFuncRefs graph (see hoistedLiteralOperations.go §4.4).
	hoistReaderCache := map[*types.Func]bool{}
	hoistInProgress := map[*types.Func]bool{}

	var readsHoistedClosure func(fn *types.Func) bool

	readsHoistedClosure = func(fn *types.Func) bool {
		if cached, ok := hoistReaderCache[fn]; ok {
			return cached
		}

		if hoistInProgress[fn] {
			return false // recursion cycle — the outer walk already accumulates this edge
		}

		hoistInProgress[fn] = true
		result := funcReadsHoistedLiteral(fn)

		if !result {
			for callee := range funcFuncRefs[fn] {
				if readsHoistedClosure(callee) {
					result = true
					break
				}
			}
		}

		delete(hoistInProgress, fn)
		hoistReaderCache[fn] = result

		return result
	}

	fileOf := func(pos token.Pos) string {
		if pos == token.NoPos {
			return ""
		}

		return fset.Position(pos).Filename
	}

	movedVars := map[*types.Var]bool{}

	for ordinal, initializer := range info.InitOrder {
		// Direct references in the RHS — INCLUDING func-literal bodies: Go's analysis counts
		// references inside literals (and an IIFE literal actually executes at init time).
		directVars := map[*types.Var]bool{}
		directFuncs := map[*types.Func]bool{}
		directConsts := map[*types.Const]bool{}
		collectRefs(initializer.Rhs, directVars, directFuncs, directConsts)

		deps := map[*types.Var]bool{}
		constDeps := map[*types.Const]bool{}
		readsHoisted := false

		for varObj := range directVars {
			deps[varObj] = true
		}

		for constObj := range directConsts {
			constDeps[constObj] = true
		}

		for fn := range directFuncs {
			for varObj := range funcClosure(fn) {
				deps[varObj] = true
			}

			for constObj := range constClosure(fn) {
				constDeps[constObj] = true
			}

			if !readsHoisted && readsHoistedClosure(fn) {
				readsHoisted = true
			}
		}

		if len(deps) == 0 && len(constDeps) == 0 && !readsHoisted {
			continue
		}

		// A hoisted-field reader on its own forces the move — the initializer need not depend on
		// any package var at all (`var greeting = greet()` where greet returns a hoisted literal).
		mustMove := readsHoisted

		for _, lhs := range initializer.Lhs {
			lhsFile := fileOf(lhs.Pos())

			for dep := range deps {
				if movedVars[dep] {
					mustMove = true // depends on a relocated var — only the ctor assigns it
				} else if fileOf(dep.Pos()) != lhsFile {
					mustMove = true // cross-file: C# cross-part initializer order is undefined
				} else if dep.Pos() > lhs.Pos() {
					mustMove = true // same-file forward reference: C# reads the zero value
				}
			}

			// An initialized-FIELD const dependency is never itself relocatable (Go constants have
			// no initialization order), so only its DECLARATION SITE matters — the same two
			// hazards a var dependency has. `var pipeLabel = string(labelPipe) + "!"` reading a
			// named-string const declared in a later file is the surviving shape: the const's own
			// `static readonly` initializer may not have run, so the concatenation folds onto an
			// empty @string.
			for dep := range constDeps {
				if fileOf(dep.Pos()) != lhsFile {
					mustMove = true // cross-file: C# cross-part initializer order is undefined
				} else if dep.Pos() > lhs.Pos() {
					mustMove = true // same-file forward reference: C# reads the zero value
				}
			}
		}

		if !mustMove {
			continue
		}

		for _, lhs := range initializer.Lhs {
			movedVars[lhs] = true

			// Blank (`_`) initializers exist only for their (discarded) side effect/witness;
			// their value is never read, so their init order is immaterial. Leaving them inline
			// keeps the emission simple and never mis-orders a real reader.
			if lhs.Name() != "_" {
				packageMovedInitVars[lhs] = ordinal
			}
		}
	}
}

// constEmittedAsInitializedField reports whether a package-scope Go CONSTANT is emitted as a C#
// `static readonly` FIELD WITH AN INITIALIZER — the only shape that carries an initialization-order
// hazard, because it is the only one whose value can be observed before it is assigned.
//
// The other two shapes cannot be: a real C# `const` is folded into every use site, and a get-only
// property (`internal static kind kindFile => 1;`) re-evaluates its expression on every read, so
// neither can ever hand back a zero value. That property form is what visitValueSpec.go's CONST arm
// emits for EVERY const C# cannot declare `const` — named types, uintptr, complex, untyped and
// out-of-int-range native ints alike — which is exactly why this predicate does not name them.
//
// Two forms stay initialized fields, because a property would rebuild an allocation on every read:
//
//   - a STRING-valued const (`static readonly @string s = "x"u8`, or a named-string wrapper such as
//     `const labelPipe label = "pipe"`) — the hoisted u8 literal;
//   - a const whose magnitude escapes every C# numeric type and falls to
//     `static readonly GoBigConst x = GoBigConst.Parse(…)` — a BigInteger parse.
//
// Mirrors those two emission decisions in visitValueSpec.go's CONST arm (the `constant.String` arm
// and `writeUntypedConst`). Where mirroring is not cheap it ERRS TOWARD `true`, because a false
// positive only costs one ordered relocation while a false negative reinstates the zero-value read
// this rule exists to prevent.
func constEmittedAsInitializedField(c *types.Const) bool {
	if c == nil {
		return false
	}

	switch c.Val().Kind() {
	case constant.String:
		// @string (and every [GoType("@string")] wrapper) is a struct holding a u8 literal, so a
		// string const is a `static readonly` field at package scope regardless of its Go type.
		return true
	case constant.Complex:
		// A complex const beyond its declared width falls to GoBigConst; a representable one is a
		// property. Distinguishing the two here would mean re-deriving exactComplexConstString's
		// per-width test, so take the safe side — package-level complex consts are vanishingly rare
		// and the only cost is a redundant relocation.
		return true
	case constant.Int:
		// GoBigConst.Parse is reached only when the value fits NEITHER int64 nor uint64; everything
		// narrower has a numeric C# form and therefore a property form.
		if _, exact := constant.Int64Val(c.Val()); exact {
			return false
		}

		_, exact := constant.Uint64Val(c.Val())

		return !exact
	case constant.Float:
		// Likewise: only a magnitude outside float64 (the `strconv.ParseFloat` failure the emission
		// tests) escapes to GoBigConst. Float64Val saturates to ±Inf there.
		value, _ := constant.Float64Val(c.Val())

		return math.IsInf(value, 0)
	}

	return false
}

// movedInitOrdinal reports whether obj is a relocated package-var initializer and, if so, its
// InitOrder ordinal (used to sequence the ctor's init-method calls).
func (v *Visitor) movedInitOrdinal(obj types.Object) (int, bool) {
	if packageMovedInitVars == nil || obj == nil {
		return 0, false
	}

	ordinal, ok := packageMovedInitVars[obj]
	return ordinal, ok
}

// packageInitMethodName composes the per-file init method name for a relocated var — the
// TempVarMarker makes a collision practically unreachable (ᴛ, U+1D1B, is an exotic glyph no
// real Go code uses — but it IS a Unicode letter, so a pathological Go identifier CAN contain
// it; see the marker-shape exposure note in ConversionStrategies-Reference.md), and a
// keyword-escaped var name (`@base`) drops its escape (the composed name is not a keyword).
func packageInitMethodName(csIDName string) string {
	return "init" + TempVarMarker + strings.TrimPrefix(csIDName, "@")
}

// recordMovedInitMethod registers a relocated initializer's per-file method for the ordered ctor.
func recordMovedInitMethod(ordinal int, methodName string) {
	packageLock.Lock()
	packageMovedInitMethods[ordinal] = methodName
	packageLock.Unlock()
}

// writePackageInitFile emits the synthetic per-package init file whose static constructor calls
// each relocated initializer's per-file init method in Go's InitOrder. No-op when nothing was
// relocated. The methods live in their home files, so this file needs no using directives.
// Under -tests (withTestHook), the ctor additionally ends with the erasable test hook (see
// packageTestInitHookMethod) so an internal-variant test conversion can append its own relocated
// initializers to the same class's single static-ctor slot.
func writePackageInitFile(outputDir, packageNamespace, packageName string, withTestHook bool) error {
	if len(packageMovedInitMethods) == 0 {
		return nil
	}

	packageClassName := getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))

	var sb strings.Builder

	sb.WriteString("// Code generated by go2cs. DO NOT EDIT.\r\n")
	sb.WriteString("// Package-level variable initialization ordered to match Go's dependency order\r\n")
	sb.WriteString("// (types.Info.InitOrder), where C#'s static field initializer order (undefined\r\n")
	sb.WriteString(fmt.Sprintf("// across partial class files) would differ. Each init%s method lives beside its\r\n", TempVarMarker))
	sb.WriteString("// variable's declaration; C# runs every static field initializer before this\r\n")
	sb.WriteString("// constructor body, so all non-relocated dependencies are already initialized.\r\n")
	sb.WriteString(fmt.Sprintf("namespace %s;\r\n\r\n", packageNamespace))
	sb.WriteString(fmt.Sprintf("partial class %s {\r\n", packageClassName))
	sb.WriteString(fmt.Sprintf("    static %s() {\r\n", packageClassName))
	writeOrderedInitCalls(&sb)

	if withTestHook {
		sb.WriteString("        ")
		sb.WriteString(packageTestInitHookMethod)
		sb.WriteString("();\r\n")
	}

	sb.WriteString("    }\r\n")

	if withTestHook {
		sb.WriteString("\r\n")
		sb.WriteString("    // -tests hook: implemented by the internal test variant's relocated-initializer\r\n")
		sb.WriteString("    // file when its _test.go files need init-order relocation into this same class;\r\n")
		sb.WriteString("    // erased entirely (declaration and call) when unimplemented — the production\r\n")
		sb.WriteString("    // compile set excludes the *_test.cs implementation.\r\n")
		sb.WriteString(fmt.Sprintf("    static partial void %s();\r\n", packageTestInitHookMethod))
	}

	sb.WriteString(fmt.Sprintf("} // end %s\r\n", packageClassName))

	return os.WriteFile(filepath.Join(outputDir, PackageInitFileName), []byte(sb.String()), 0644)
}

// writeOrderedInitCalls appends the relocated init-method calls in InitOrder ordinal order.
func writeOrderedInitCalls(sb *strings.Builder) {
	ordinals := make([]int, 0, len(packageMovedInitMethods))

	for ordinal := range packageMovedInitMethods {
		ordinals = append(ordinals, ordinal)
	}

	sort.Ints(ordinals)

	for _, ordinal := range ordinals {
		sb.WriteString("        ")
		sb.WriteString(packageMovedInitMethods[ordinal])
		sb.WriteString("();\r\n")
	}
}

// writeTestVariantInitFile is writePackageInitFile's -tests mirror: the ordered initialization
// for a TEST variant's relocated package-level vars (collectMovedInitVars now runs in the test
// driver too — the three-drivers rule). Two shapes, both into a `*_test.cs`-suffixed file the
// production csproj's test-artifact exclusion already ignores:
//
//   - implementHook: the INTERNAL variant when the production package_init.cs exists — that file
//     already owns the class's single static-ctor slot, so the test side implements the erasable
//     partial hook its ctor calls LAST. Test relocations therefore run after every production
//     relocation (semantically safe: production initializers can never depend on test vars), with
//     all static field initializers (every class part) already complete.
//   - otherwise: the variant's class has no static ctor yet (the external `<pkg>_test` class, or
//     an internal variant whose production package had nothing to relocate) — emit the ordered
//     static constructor directly, exactly like package_init.cs.
func writeTestVariantInitFile(outputDir, fileName, packageNamespace, packageClassName string, implementHook bool) error {
	if len(packageMovedInitMethods) == 0 {
		return nil
	}

	var sb strings.Builder

	sb.WriteString("// Code generated by go2cs. DO NOT EDIT.\r\n")
	sb.WriteString("// Test-variant package-level variable initialization ordered to match Go's\r\n")
	sb.WriteString(fmt.Sprintf("// dependency order (types.Info.InitOrder) — see package_init.cs. Each init%s\r\n", TempVarMarker))
	sb.WriteString("// method lives beside its variable's declaration in the converted test file.\r\n")
	sb.WriteString(fmt.Sprintf("namespace %s;\r\n\r\n", packageNamespace))
	sb.WriteString(fmt.Sprintf("partial class %s {\r\n", packageClassName))

	if implementHook {
		sb.WriteString(fmt.Sprintf("    static partial void %s() {\r\n", packageTestInitHookMethod))
	} else {
		sb.WriteString(fmt.Sprintf("    static %s() {\r\n", packageClassName))
	}

	writeOrderedInitCalls(&sb)
	sb.WriteString("    }\r\n")
	sb.WriteString(fmt.Sprintf("} // end %s\r\n", packageClassName))

	return os.WriteFile(filepath.Join(outputDir, fileName), []byte(sb.String()), 0644)
}
