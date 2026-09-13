// foreignTypeAliases.go - Gbtc
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

// The SECOND metadata class a missing package_info.cs takes with it (the first — the collision
// renames — is foreignNameCollisions.go): a dependency's RE-EXPORTED type aliases. `os` declares
// `type FileMode = fs.FileMode`, which the converted `os` emits as an assembly-scoped
// `global using FileMode = go.io.fs_package.FileMode;` and publishes as
// `[assembly: GoTypeAlias("FileMode", "go.io.fs_package.FileMode")]`. The re-export is therefore a
// USING ALIAS inside os's assembly, never a member of `os_package` — so a consumer converted
// without that artifact emits `os.FileMode` and gets CS0426. `os.FileMode`/`FileInfo`/`DirEntry`/
// `PathError` are pervasive in real Go, which makes this the end-user-facing half of the class.
//
// Same INVARIANT as the collision derivation: what a dependency publishes is a function of THAT
// package's own declarations, never of which packages the current run converts. Both shapes
// visitTypeSpec publishes through this route are reproduced here from the dependency's go/types
// scope (plus its syntax where the declaration's RHS carries the distinction):
//
//  1. a Go type ALIAS (`type FileMode = fs.FileMode`), and
//  2. a DEFINED type over a NAMED interface (`type PublicKey any`, `type Reader io.Reader`), which
//     is emitted as a using alias too because it has exactly the interface's method set.
//
// A target this cannot render with CERTAINTY is SKIPPED rather than guessed — a missing entry
// leaves the reference exactly as it converts today, while a wrong one names a type that does not
// exist. The deliberate omissions are recorded at foreignAliasTargetName.
//
// Still NOT derivable, and out of scope here: the dependency's `GoImplement` pairs
// (loadPackageImplements). Those are recorded at CONVERSION time from the cast/witness sites the
// dependency's own bodies contain — which adapter classes its assembly actually carries is a
// product of its EMISSION, not of its declarations, so there is nothing sound to compute from
// types. Over-approximating would reference an adapter that does not exist (CS0246), which is
// strictly worse than the present behavior (the consumer records and emits its own local adapter,
// which compiles and behaves identically — only the class is duplicated rather than shared).

// foreignDerivedAliases caches the full derived entry set per package. Package objects are interned
// per run (as with packageMethodNames), so the pointer is a stable key.
var foreignDerivedAliases map[*types.Package][][2]string

// foreignDerivedTypeAliases returns every `GoTypeAlias` (source, target) pair the converted form of
// pkg publishes that can be derived from pkg alone — its collision renames plus its re-exported
// type aliases — in the RAW package_info.cs form, so the caller runs them through the same
// normalization as parsed entries (applyExportedTypeAliases).
func foreignDerivedTypeAliases(pkg *packages.Package) [][2]string {
	if pkg == nil || pkg.Types == nil {
		return nil
	}

	packageLock.Lock()
	cached, isCached := foreignDerivedAliases[pkg.Types]
	packageLock.Unlock()

	if isCached {
		return cached
	}

	// Derived OUTSIDE the lock — the derivations take packageLock themselves. Derivation is
	// deterministic, so a concurrent double-compute stores the same result.
	collisions := foreignCollisionTypeAliases(pkg)
	reExports := deriveReExportedTypeAliases(pkg)

	// A fresh slice: foreignCollisionTypeAliases hands back its own cached backing array, which an
	// append could write through.
	aliases := make([][2]string, 0, len(collisions)+len(reExports))
	aliases = append(aliases, collisions...)
	aliases = append(aliases, reExports...)

	packageLock.Lock()

	if foreignDerivedAliases == nil {
		foreignDerivedAliases = map[*types.Package][][2]string{}
	}

	foreignDerivedAliases[pkg.Types] = aliases
	packageLock.Unlock()

	return aliases
}

// deriveReExportedTypeAliases mirrors visitTypeSpec's using-alias arm — the branch taken by a Go
// type alias and by a defined type over a named interface, both of which record an exported
// `GoTypeAlias` naming the TARGET's rendered C# type — computed from pkg's own scope instead of
// from a conversion. scope.Names() is sorted, so the result is deterministic.
func deriveReExportedTypeAliases(pkg *packages.Package) [][2]string {
	scope := pkg.Types.Scope()

	var aliases [][2]string

	for _, name := range scope.Names() {
		// Only an EXPORTED name can be referenced from another package, and the exporter records
		// only those (visitTypeSpec's getAccess gate).
		if getAccess(name) != "public" {
			continue
		}

		obj, isTypeName := scope.Lookup(name).(*types.TypeName)

		if !isTypeName {
			continue
		}

		// A COLLIDING DEFINED type belongs to the collision derivation's lane, which publishes the
		// Δ rename and — for the defined-over-empty-interface shape — the second hop to `object`
		// (encoding/json's `Token`); deriving it here too would double-publish the same source name
		// with a different target. A Go type ALIAS is exempt: performNameCollisionAnalysis records
		// only DEFINED types, so an alias name that also names a method is NOT Δ-renamed and IS
		// published as a plain re-export — internal/reflectlite's `type Kind = abi.Kind` alongside
		// `(*rtype).Kind()` publishes `("Kind", "go.@internal.abi_package.ΔKind")`.
		if !obj.IsAlias() && packageHasMethodNamed(pkg.Types, name) {
			continue
		}

		target, isUsingAlias := usingAliasTargetType(pkg, obj)

		if !isUsingAlias {
			continue
		}

		rendered, isRenderable := foreignAliasTargetName(pkg, target)

		if !isRenderable {
			continue
		}

		aliases = append(aliases, [2]string{getCoreSanitizedIdentifier(name), rendered})
	}

	return aliases
}

// usingAliasTargetType returns the type whose rendering the dependency puts on the RHS of the
// `global using` it emits for obj, and whether obj is one of the two declarations that take that
// route at all. Everything else — a defined struct/array/map/func/basic type, an inline interface
// definition — is emitted as a real nested type and publishes no alias.
func usingAliasTargetType(pkg *packages.Package, obj *types.TypeName) (types.Type, bool) {
	if obj.IsAlias() {
		// A Go type alias: the target is the declaration's RHS. Under gotypesalias the object's
		// type is the *types.Alias itself, whose Rhs is that declaration's RHS; without it the
		// object's type IS the RHS already. An alias-to-an-ALIAS is left to the caller's renderer,
		// which declines a non-Named target — the corpus has no such chain and the converted form
		// of one is not reproducible here.
		if alias, isAlias := obj.Type().(*types.Alias); isAlias {
			return alias.Rhs(), true
		}

		return obj.Type(), true
	}

	// A DEFINED type over an INTERFACE has exactly that interface's method set and can carry no
	// methods of its own, so visitTypeSpec emits it as a using alias rather than a nested type —
	// but ONLY when the declaration's RHS is a NAMED type (`type Token any`, `type Reader
	// io.Reader`). An inline interface DEFINITION (`type X interface{…}`) is a real nested C#
	// interface. That distinction lives only in the declaration's RHS, so it is read from the
	// dependency's syntax; a dependency loaded without syntax yields nothing rather than a guess.
	if _, isInterface := obj.Type().Underlying().(*types.Interface); !isInterface {
		return nil, false
	}

	rhs := definedTypeSpecRHS(pkg, obj.Name())

	switch rhs.(type) {
	case *ast.Ident, *ast.SelectorExpr:
	default:
		return nil, false
	}

	// The EMPTY-interface RHS (`any`, or a named alias of it) renders as `object` with no further
	// resolution, so it needs no type info — which keeps the most common shape of this class
	// (crypto's `PublicKey`/`PrivateKey`/`DecrypterOpts`, plugin's `Symbol`, database/sql/driver's
	// `Value`) derivable even from a dependency whose TypesInfo was not retained.
	if iface, isInterface := obj.Type().Underlying().(*types.Interface); isInterface && iface.Empty() {
		return obj.Type(), true
	}

	if pkg.TypesInfo == nil {
		return nil, false
	}

	return pkg.TypesInfo.TypeOf(rhs), true
}

// foreignAliasTargetName renders the C# type name the dependency's own conversion puts on the RHS
// of its `global using` — the string its package_info.cs publishes as the alias TARGET — or false
// when the shape cannot be reproduced with certainty here.
//
// Reproduced: the EMPTY interface (`object`) and a NAMED target that its own package emits as a
// real nested type, qualified through that package's converted class and carrying that package's
// OWN collision rename (internal/reflectlite's `type Kind = abi.Kind` publishes
// `go.@internal.abi_package.ΔKind`, because `abi` Δ-renames `Kind` against `(*Type).Kind()`).
//
// Declined, each because the target's converted spelling is a product of the dependency's EMISSION
// rather than of its declarations:
//   - a composite/basic RHS (`type Table = map[string]int`), whose rendering runs the whole
//     convertToCSFullTypeName lowering, and an alias-to-an-alias chain;
//   - an ANONYMOUS struct/interface RHS, which is LIFTED under a generated name whose numbering
//     only a conversion assigns (internal/fuzz's `CorpusEntry` → `CorpusEntryᴛ1`);
//   - a generic target, whose type arguments need that same lowering;
//   - a methodless named FUNC type, rendered inline as its base delegate with no named type to
//     point at (the same omission typeCollisionAliases makes);
//   - a target that is ITSELF emitted as a using alias by its own package (a defined type over a
//     named interface), which would need a second hop this derivation does not follow.
func foreignAliasTargetName(pkg *packages.Package, target types.Type) (string, bool) {
	if target == nil {
		return "", false
	}

	// The empty-interface target (`type X any` / `type X = any`) IS `object` — a csproj-level `any`
	// alias does not resolve in a using-alias RHS, so visitTypeSpec emits object directly.
	if iface, isInterface := target.Underlying().(*types.Interface); isInterface && iface.Empty() {
		return "object", true
	}

	named, isNamed := target.(*types.Named)

	if !isNamed {
		return "", false
	}

	obj := named.Obj()
	targetPkg := obj.Pkg()

	if targetPkg == nil {
		return "", false
	}

	if typeArgs := named.TypeArgs(); typeArgs != nil && typeArgs.Len() > 0 {
		return "", false
	}

	if typeParams := named.TypeParams(); typeParams != nil && typeParams.Len() > 0 {
		return "", false
	}

	if _, isInlineFuncType := methodlessNamedFuncSignature(named); isInlineFuncType {
		return "", false
	}

	// A named INTERFACE target is a nested C# interface only when its own declaration defines the
	// interface INLINE (io/fs's `type FileInfo interface{…}`, go/ast's `Expr`). A defined type over
	// a NAMED interface is a using alias in its own package, so naming it as a package member would
	// be CS0426 — decline, as does a target package loaded without syntax.
	if _, isInterface := named.Underlying().(*types.Interface); isInterface {
		source := loadedPackageSource(pkg, targetPkg)

		if source == nil {
			return "", false
		}

		switch definedTypeSpecRHS(source, obj.Name()).(type) {
		case *ast.Ident, *ast.SelectorExpr, nil:
			return "", false
		}
	}

	// The target package's OWN collision rename applies — the reference names a member of THAT
	// package's class, spelled the way that package declares it (packageHasMethodNamed is the same
	// foreign-package-aware test the collision derivation uses).
	name := getCoreSanitizedIdentifier(obj.Name())

	if packageHasMethodNamed(targetPkg, obj.Name()) {
		name = getCollisionAvoidanceIdentifier(obj.Name())
	}

	// Matches getFullyQualifiedTypeName's cross-package arm run through convertToCSFullTypeName: the package
	// CLASS path (path with the final segment replaced by the package NAME, for a major-version
	// tail) converted to the rooted namespace, then the member.
	namespace := convertImportPathToNamespace(packageClassPath(targetPkg.Path(), targetPkg.Name()), PackageSuffix)

	return RootNamespace + "." + namespace + "." + name, true
}

// loadedPackageSource returns the loaded package whose types are targetPkg, reached from pkg — pkg
// itself for a same-package target, otherwise its DIRECT import (an alias RHS naming `x.T` can only
// come from a package pkg imports). Returns nil when the handle is not available, which every
// caller must treat as "cannot derive".
func loadedPackageSource(pkg *packages.Package, targetPkg *types.Package) *packages.Package {
	if pkg.Types == targetPkg {
		return pkg
	}

	if imported, isImported := pkg.Imports[targetPkg.Path()]; isImported && imported.Types == targetPkg {
		return imported
	}

	// The import map is keyed by the path as WRITTEN in the source, which a GOROOT-vendored
	// dependency spells differently from its resolved package path.
	for _, imported := range pkg.Imports {
		if imported.Types == targetPkg {
			return imported
		}
	}

	return nil
}
