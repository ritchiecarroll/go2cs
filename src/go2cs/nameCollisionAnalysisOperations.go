// nameCollisionAnalysisOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/token"
	"go/types"

	"golang.org/x/tools/go/packages"
)

// The `performNameCollisionAnalysis` function analyzes the package for name collisions
// between constants/variables and method names. Resulting collisions are stored in the
// global `nameCollisions` map. This function is called for each package during the
// conversion process to ensure that any potential name collisions are identified and
// handled appropriately. This is important to avoid naming conflicts that could lead
// to runtime errors or unexpected behavior in the generated C# code, which is more
// strict about unique naming of discrete types than Go is in this case.

// emitterSpelledTypeNames are type names the converter's EMITTER spells independent of the Go
// source's own spelling — so a user-declared package-level TYPE of the same name must be
// package-scoped Δ-renamed (nameCollisions), never '@'-escaped or globally `reserved` (both
// would corrupt the emitter's legitimate spellings in every other package). Proven vectors:
// `any` — the `interface{}` rendering (`slice<any>` bound the user struct, CS0029 ×3);
// `rune` — the untyped rune-constant default (`c := 'x'` emits `rune c = 'x';`, CS0030);
// `nint`/`nuint` — the Go int/uint MAPPED spellings and C# native-int contextual keywords
// (`partial struct nint { internal nint d; }` is a CS0523 layout cycle, and '@' cannot fix a
// name-identity problem).
var emitterSpelledTypeNames = map[string]bool{
	"any": true, "rune": true, "nint": true, "nuint": true,
}

func performNameCollisionAnalysis(pkg *packages.Package) {
	// cmd/cgo's C type and constant names are renamed package-wide into the C pseudo-package (cgoPseudoPackage.go).
	collectCgoPseudoPackageNames(pkg.Types)

	// Track names of various declarations
	namedElementNames := make(map[string]bool)

	// Names this package declares in a C# 15 reserved TYPE position (see csReservedTypeNames).
	csReservedTypeNameDecls := make(map[string]bool)

	// Collect all named element names (top-level declarations only)
	for _, file := range pkg.Syntax {
		collectCSReservedTypeNameDecls(file, csReservedTypeNameDecls)

		for _, decl := range file.Decls {
			switch node := decl.(type) {
			case *ast.GenDecl:
				// Handle constants and variables at package level (not inside functions)
				if node.Tok == token.CONST || node.Tok == token.VAR {
					for _, spec := range node.Specs {
						if valueSpec, ok := spec.(*ast.ValueSpec); ok {
							for _, name := range valueSpec.Names {
								// The blank identifier is a discard, never referenced, and the
								// value-spec visitor gives each blank a unique name — so a `_`
								// const/var must not be treated as colliding with a `func _()`
								// (a common stringer compile-time assertion). Otherwise every
								// `_` is Δ-prefixed to the same `Δ_` and they collide (CS0102).
								if name.Name == "_" {
									continue
								}

								namedElementNames[name.Name] = false
							}
						}
					}
				}

				// Handle type declarations (structs, interfaces, type aliases)
				if node.Tok == token.TYPE {
					for _, spec := range node.Specs {
						if typeSpec, ok := spec.(*ast.TypeSpec); ok {
							if !typeSpec.Assign.IsValid() {
								namedElementNames[typeSpec.Name.Name] = true
							}
						}
					}
				}
			}
		}
	}

	// A package-level TYPE named after a spelling the EMITTER produces on its own (`any`,
	// `rune`, `nint`, `nuint` — see emitterSpelledTypeNames) shadows that spelling inside the
	// package class, breaking the converter's own emissions (`slice<any>` bound the user
	// struct, CS0029 ×3; `internal nint d;` inside `partial struct nint` is a CS0523 cycle).
	// Δ-rename every ident named it in THIS package (nameCollisions is package-scoped),
	// keeping the bare name bound to its emitter meaning. These names must never go in the
	// string-based `reserved` set: that would corrupt the emitter's legitimate spellings in
	// every OTHER package (see the comment on that set).
	for name, isType := range namedElementNames {
		if isType && emitterSpelledTypeNames[name] {
			nameCollisions[name] = true
		}
	}

	// A type, alias or type parameter named after a C# 15 reserved TYPE-position keyword (`closed`,
	// `union`; preview `safe`) breaks as a type name and at member start, where C# 15 reads it as a
	// keyword: Δ-rename it package-wide the same way (see csReservedTypeNames for the positions).
	for name := range csReservedTypeNameDecls {
		nameCollisions[name] = true
	}

	// A package-level type, constant or variable named like a METHOD of the package is no collision: a
	// method is a member of its receiver type's holder, beside the package class, and never of the class
	// that declares the element (methodHolder.go). Until 2026-11-11 both were members of the package
	// class, and the element took the rename here. For the same reason no method hides a Go built-in or
	// an import's alias anywhere but in its own holder: the two package-wide name sets that qualified
	// those references are gone, and each reference asks its holder (holderDeclares).
}

// heapIntrinsicIdent is the Go identifier that shadows golib's heap-boxing intrinsic. Unlike the
// name collisions above — which are Go declarations colliding with each OTHER through the emitter's
// naming rules — this one is a collision go2cs INVENTS: `heap` is an ordinary, legal Go identifier
// that no Go program is constrained by, and it only becomes a hazard because the converter emits
// address-taken locals through golib's `heap(value, out var Ꮡname)` helper, imported by every
// converted file via `using static go.builtin`.
const heapIntrinsicIdent = "heap"

// heapIntrinsicName is the spelling a heap-box emission must use for golib's boxing intrinsic in
// the CURRENT function. Bare `heap` normally — that is what the whole corpus reads — and the
// fully-qualified `builtin.heap` where a Go declaration of that name is in scope.
//
// A C# local, parameter or containing-class member wins simple-name lookup outright over a member
// imported by `using static`, so a Go body that both declares `heap` and needs a heap box emits
// `ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);` with `heap` bound to the DECLARATION
// — `CS0149: Method name expected`, which reads as an emitter defect at the box site rather than as
// a name collision (internal/trace's `heapDebugString(heap []*batchCursor)`, the whole of that
// package's 92-verdict build wall). Qualifying is the minimal remedy: it renames nothing the Go
// source chose, and it is conditional so the corpus stays byte-identical everywhere the collision
// does not exist.
//
// An IMPORT alias named `heap` (`import "container/heap"`, which the converter renders as
// `using heap = go.container.heap_package;`) is deliberately NOT treated as shadowing: a using-alias
// does not displace a `using static` method group in an invocation, proven by container/heap's own
// banked example_pq_test.cs, which carries both the alias and two heap-box emissions and compiles.
func (v *Visitor) heapIntrinsicName() string {
	if v.heapIntrinsicShadowed {
		return "builtin." + heapIntrinsicIdent
	}

	return heapIntrinsicIdent
}

// declaresHeapIntrinsicIdent reports whether node declares — anywhere inside it, nested function
// literals included — a Go object named `heap` that would win C# simple-name lookup over golib's
// intrinsic. Package names are excluded (see heapIntrinsicName); everything else that Defs records
// is a local, parameter, result, field, constant, type or function whose emitted C# name is exactly
// `heap`.
func (v *Visitor) declaresHeapIntrinsicIdent(node ast.Node) bool {
	if node == nil || v.info == nil || !v.packageMentionsHeapIntrinsicIdent() {
		return false
	}

	shadowed := false

	ast.Inspect(node, func(n ast.Node) bool {
		if shadowed {
			return false
		}

		ident, ok := n.(*ast.Ident)

		if !ok || ident.Name != heapIntrinsicIdent {
			return true
		}

		obj := v.info.Defs[ident]

		if obj == nil {
			return true
		}

		if _, isPackageName := obj.(*types.PkgName); isPackageName {
			return true
		}

		shadowed = true

		return false
	})

	return shadowed
}

// packageMentionsHeapIntrinsicIdent reports whether the package under conversion declares an object
// named `heap` ANYWHERE, at any scope. It is the cheap gate in front of declaresHeapIntrinsicIdent's
// per-declaration AST walk: without it every function in every package pays a second full traversal
// to learn what one pass over the package's own Defs map answers for all of them. Computed once and
// cached; the overwhelmingly common answer is false, and a false answer means no walk happens at all.
func (v *Visitor) packageMentionsHeapIntrinsicIdent() bool {
	if v.heapIdentInPackage == nil {
		mentions := false

		for ident, obj := range v.info.Defs {
			if ident == nil || ident.Name != heapIntrinsicIdent || obj == nil {
				continue
			}

			if _, isPackageName := obj.(*types.PkgName); isPackageName {
				continue
			}

			mentions = true

			break
		}

		v.heapIdentInPackage = &mentions
	}

	return *v.heapIdentInPackage
}

// packageDeclaresHeapIntrinsicIdent reports whether the package under conversion declares a package
// level FUNC named `heap`, which emits as a member of the `<pkg>_package` class and so wins
// simple-name lookup inside every method of that class — a package-wide qualification, not a
// per-function one.
//
// ⚠ FUNC specifically, and the narrowing is measured, not assumed. C#'s invocable-member rule
// (§12.8.4 — a simple name used as the target of an invocation ignores TYPE MEMBERS that are not
// invocable) means a package-level Go `type heap` or `var heap`, both of which emit as non-invocable
// members, are skipped in `heap(…)` and the `using static go.builtin` method group is found anyway.
// The first version of this check tested "any non-PkgName object" and was falsified by its own A/B:
// GlobalCapturedInClosure declares `type heap` at package level, and with the fix REVERTED it still
// compiled — so the broad form was only over-qualifying, changing a golden no defect required. A
// LOCAL or PARAMETER named `heap` is a different rule and does win (see declaresHeapIntrinsicIdent);
// the invocable-member filter applies to type members, not to the local declaration space.
//
// The func shape is therefore the one package-level case that can genuinely collide. Nothing in the
// corpus declares it — GOROOT's only `heap` declarations are internal/trace/batchcursor.go's
// parameters and runtime/time.go's locals, both function-scoped — so it is reasoned from the lookup
// rule rather than reproduced; the NEGATIVE side (a package-level type must NOT trigger) is what
// GlobalCapturedInClosure guards.
func (v *Visitor) packageDeclaresHeapIntrinsicIdent() bool {
	if v.pkg == nil || v.pkg.Scope() == nil {
		return false
	}

	_, isFunc := v.pkg.Scope().Lookup(heapIntrinsicIdent).(*types.Func)

	return isFunc
}
