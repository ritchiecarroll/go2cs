// foreignNameCollisions.go - Gbtc
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

	"golang.org/x/tools/go/packages"
)

// A converted package publishes its OWN name-collision renames as `GoTypeAlias` entries in its
// package_info.cs, and a CONSUMER reads them back at conversion time (loadImportedTypeAliases) to
// spell the foreign member the way the dependency declared it. That artifact only exists once the
// dependency has BEEN converted into the output root, so a consumer converted on its own — the
// end-user `go2cs <dir>` and `-recurse` paths, and any partial `-stdlib <pkg>` run — emitted the
// UNRENAMED foreign name: `time.Second` instead of `time.ΔSecond`, which binds the
// `Second(this Time)` extension method group and does not compile (CS0019/CS1503/CS0023). The
// committed 302-package corpus is spelled correctly only because a full `-stdlib` run converts
// `time` before `archive/tar`.
//
// INVARIANT: a foreign package's collision renames are a function of THAT package's own
// declarations alone — never of which packages the current run happens to convert. They are
// derived here from the dependency's loaded go/types scope (plus its syntax for the one shape
// types alone cannot distinguish), reproducing exactly the entries performNameCollisionAnalysis
// would have exported had the dependency been converted, so a single-package conversion and a
// whole-stdlib conversion emit the identical spelling. This is the same discipline
// packageHasMethodNamed already applies to the field-rename decision: recompute a FOREIGN
// package's collisions from its own types.Package rather than from run-accumulated state.
//
// Scope: the collision renames only. The dependency's RE-EXPORTED type aliases (`type DirEntry =
// fs.DirEntry` → `go.io.fs_package.DirEntry`) are the sibling class, derived under the same
// invariant in foreignTypeAliases.go; foreignDerivedTypeAliases there composes both and is what the
// alias loader calls. Its GoImplement pairs remain underivable — they are a product of the
// dependency's EMISSION rather than of its declarations (see that file's header).

// foreignCollisionAliases caches the derived entries per package. Package objects are interned per
// run (as with packageMethodNames), so the pointer is a stable key.
var foreignCollisionAliases map[*types.Package][][2]string

// foreignCollisionTypeAliases returns the `GoTypeAlias` (source, target) pairs the converted form
// of pkg publishes for its own name collisions, in the RAW package_info.cs form so a caller can
// run them through the same normalization as parsed entries (applyExportedTypeAliases) — the two
// paths must never diverge in how a target is qualified.
func foreignCollisionTypeAliases(pkg *packages.Package) [][2]string {
	if pkg == nil || pkg.Types == nil {
		return nil
	}

	packageLock.Lock()
	cached, isCached := foreignCollisionAliases[pkg.Types]
	packageLock.Unlock()

	if isCached {
		return cached
	}

	// Derived OUTSIDE the lock — packageHasMethodNamed takes packageLock itself. Derivation is
	// deterministic, so a concurrent double-compute stores the same result.
	aliases := deriveCollisionTypeAliases(pkg)

	packageLock.Lock()

	if foreignCollisionAliases == nil {
		foreignCollisionAliases = map[*types.Package][][2]string{}
	}

	foreignCollisionAliases[pkg.Types] = aliases
	packageLock.Unlock()

	return aliases
}

// deriveCollisionTypeAliases mirrors performNameCollisionAnalysis's export rule — a package-level
// const/var/defined-type whose name is ALSO a package method or function name is Δ-renamed, and an
// EXPORTED one records a `GoTypeAlias` so consumers can follow the rename — computed from pkg's
// own scope instead of the run-accumulated analysis state. scope.Names() is sorted, so the result
// is deterministic.
func deriveCollisionTypeAliases(pkg *packages.Package) [][2]string {
	scope := pkg.Types.Scope()

	var aliases [][2]string

	for _, name := range scope.Names() {
		// Only an EXPORTED name can be referenced from another package, and the exporter records
		// only those (performNameCollisionAnalysis's getAccess gate).
		if getAccess(name) != "public" {
			continue
		}

		// The collision test itself: the name is also a method/function name in the SAME package.
		// packageHasMethodNamed walks that package's own scope (free functions plus every named
		// type's methods), which is the types-level equivalent of the analysis's FuncDecl scan.
		if !packageHasMethodNamed(pkg.Types, name) {
			continue
		}

		switch obj := scope.Lookup(name).(type) {
		case *types.Const, *types.Var:
			// A colliding const/var Δ-renames the MEMBER and exports it with the `const:` marker,
			// which keeps the consumer's reference qualified through the package class rather than
			// aliased to a type (`time.ΔSecond`, not `timeꓸSecond`).
			aliases = append(aliases, [2]string{getCoreSanitizedIdentifier(name), "const:" + getCollisionAvoidanceIdentifier(name)})
		case *types.TypeName:
			// A Go type ALIAS declaration is never collision-renamed — the analysis records only
			// DEFINED types (`!typeSpec.Assign.IsValid()`).
			if obj.IsAlias() {
				continue
			}

			aliases = append(aliases, typeCollisionAliases(pkg, obj)...)
		}
	}

	return aliases
}

// typeCollisionAliases returns the entries published for one collision-renamed DEFINED type,
// reproducing the two cases where the converted package deliberately publishes something other
// than the plain `name` → `Δname` pair.
func typeCollisionAliases(pkg *packages.Package, obj *types.TypeName) [][2]string {
	name := obj.Name()
	renamed := getCollisionAvoidanceIdentifier(name)

	// A non-generic METHODLESS named func type is rendered inline as its base delegate and emits
	// no named `<pkg>_package.<Δname>` type at all, so the converted package deliberately omits its
	// alias (writePackageInfoFile's packageInlineFuncTypeNames skip). Omit it here too, or a
	// consumer's generated `global using` names a nonexistent type (go/doc's `ast.Filter` →
	// `go.go.ast_package.ΔFilter`, CS0426).
	if _, isInlineFuncType := methodlessNamedFuncSignature(obj.Type()); isInlineFuncType {
		return nil
	}

	// A DEFINED type over a NAMED interface (`type Token any`, `type Reader io.Reader`) is emitted
	// as a `global using` alias to that interface, never a nested type, so the converted package
	// publishes a SECOND hop the consumer's loader follows (`Token` → `ΔToken` → `object`). An
	// INLINE interface definition (`type X interface{…}`) is a real nested `Δname` interface and
	// takes the plain pair below — a distinction only the declaration's RHS carries, so it is read
	// from the dependency's syntax.
	if iface, isInterface := obj.Type().Underlying().(*types.Interface); isInterface {
		switch definedTypeSpecRHS(pkg, name).(type) {
		case *ast.Ident, *ast.SelectorExpr:
			// Only the EMPTY-interface form has a reproducible target (`object`). A non-empty
			// named-interface RHS resolves to that RHS's rendered C# type name — a visitTypeSpec-only
			// computation, and a shape the 302-package corpus does not contain — so publish nothing:
			// a missing entry leaves such a reference exactly as it converts today, while a WRONG
			// one would name a type that does not exist.
			if !iface.Empty() {
				return nil
			}

			return [][2]string{{getCoreSanitizedIdentifier(name), renamed}, {renamed, "object"}}
		case nil:
			// The declaration could not be located (a dependency loaded without syntax): the two
			// interface forms are indistinguishable here, so publish nothing rather than guess.
			return nil
		}
	}

	return [][2]string{{getCoreSanitizedIdentifier(name), renamed}}
}

// definedTypeSpecRHS returns the right-hand-side expression of pkg's package-level DEFINED type
// declaration for name, or nil when the declaration cannot be located — the package was loaded
// without syntax, or name is a type alias rather than a definition.
func definedTypeSpecRHS(pkg *packages.Package, name string) ast.Expr {
	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			genDecl, isGenDecl := decl.(*ast.GenDecl)

			if !isGenDecl {
				continue
			}

			for _, spec := range genDecl.Specs {
				typeSpec, isTypeSpec := spec.(*ast.TypeSpec)

				if !isTypeSpec || typeSpec.Assign.IsValid() || typeSpec.Name.Name != name {
					continue
				}

				return typeSpec.Type
			}
		}
	}

	return nil
}
