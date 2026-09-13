// linknameVarAliasRegistry_test.go - Gbtc
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
	"testing"
)

// TestLinknameVarAliasRegistryMatchesGoSource checks every linknameVarAliasTargets row against the
// REAL Go source in GOROOT — the same verification TestLinknamePushRegistryMatchesGoSource performs
// for the push direction, and it exists for the same structural reason. Converting
// `internal/syscall/windows`, the converter sees only that package's syntax: `var CanUseLongPaths
// bool` under a one-arg handle is indistinguishable from any other opened var, and runtime's
// two-argument directive — the thing that makes the two declarations ONE variable — is invisible.
// The registry records that missing half as a judgment, and an unverified judgment is exactly what
// rots.
//
// Both halves of the alias are re-derived here, because a row can be wrong in two directions:
//
//   - the FORWARDING side must still declare the symbol as a package-level var AND still carry Go's
//     one-argument `//go:linkname` handle. The handle is the authorization; varLinknameAliasForward
//     requires it and fails closed to a plain field without it, so a row that outlives the handle
//     would silently forward nothing — the row would look live and do nothing at all.
//   - the STORAGE side must declare its own var AND carry the two-argument directive naming this
//     row's key. That directive is what makes the inversion faithful rather than an invention: Go
//     really does alias these two names onto one word of memory, and go2cs is only choosing which
//     assembly holds it.
//
// Build constraints are ignored (parseGoPackageDir scans every .go file in the package directory),
// so the Windows-only pair is verifiable from any lane.
func TestLinknameVarAliasRegistryMatchesGoSource(t *testing.T) {
	goRoot := testGoRoot(t)

	if len(linknameVarAliasTargets) == 0 {
		t.Fatal("linknameVarAliasTargets is empty: the registry guard is vacuous")
	}

	for key, alias := range linknameVarAliasTargets {
		targetPkg, symbol, ok := splitLastDot(key)

		if !ok {
			t.Errorf("registry key %q is not <pkgPath>.<symbol>", key)
			continue
		}

		storagePkg, storageName, ok := splitLastDot(alias.storage)

		if !ok {
			t.Errorf("registry row %q has storage %q, which is not <pkgPath>.<member>", key, alias.storage)
			continue
		}

		if storagePkg == targetPkg {
			t.Errorf("registry row %q keeps its storage in the SAME package (%q): an alias inversion moves the storage to the OTHER side, and a self-reference emits a property that reads itself", key, storagePkg)
			continue
		}

		// The FORWARDING side: the var exists and Go opened it.
		if !pkgDeclaresVar(t, goRoot, targetPkg, symbol) {
			t.Errorf("registry row %q: no package-level var %s in %s — the row names a symbol Go's source does not have (renamed? deleted?)", key, symbol, targetPkg)
			continue
		}

		if !pkgHasLinknameHandle(t, goRoot, targetPkg, symbol) {
			t.Errorf("registry row %q: %s does not carry the one-argument `//go:linkname %s` handle — that handle is Go's authorization for the alias, and varLinknameAliasForward fails closed without it, so this row forwards NOTHING while looking live", key, targetPkg, symbol)
		}

		// The STORAGE side: the var exists and really does perform the alias.
		if !pkgDeclaresVar(t, goRoot, storagePkg, storageName) {
			t.Errorf("registry row %q: no package-level var %s in %s — the storage this row forwards to does not exist in Go's source", key, storageName, storagePkg)
			continue
		}

		if !pkgHasLinknamePush(t, goRoot, storagePkg, storageName, key) {
			t.Errorf("registry row %q: %s does not carry `//go:linkname %s %s` — the alias this row inverts does not exist in Go's source, so the emitted forwarding property would be an invention rather than a faithful projection of Go's link-time identity", key, storagePkg, storageName, key)
		}
	}
}

// TestLinknameVarAliasStorageIsDerived is the derivation guard the design asks for: the publicize
// arm's index and the registry it comes from must be one fact, not two.
//
// Two hand-maintained lists of the same thing are exactly how the storage side and the forwarding
// side come to disagree, and the disagreement is silent in the worst direction — the forwarder
// compiles only for as long as some OTHER rule happens to publicize the member. This mirrors the
// linknamePushSources derivation, and asserts the property rather than the implementation: every row
// contributes its storage, and nothing else is in there.
//
// RED PROOF: replace linknameVarAliasStorage's derivation with a hand-written literal map (even a
// correct one) and add or remove a registry row — the two sets stop agreeing.
func TestLinknameVarAliasStorageIsDerived(t *testing.T) {
	if len(linknameVarAliasStorage) != len(linknameVarAliasTargets) {
		t.Errorf("linknameVarAliasStorage has %d entries for %d registry rows: the reverse index is not derived from the registry (or two rows share one storage member, which would make one of them unreachable)",
			len(linknameVarAliasStorage), len(linknameVarAliasTargets))
	}

	for key, alias := range linknameVarAliasTargets {
		if !linknameVarAliasStorage[alias.storage] {
			t.Errorf("registry row %q names storage %q, which is absent from linknameVarAliasStorage: packageVarAccess would leave it `internal` and the forwarding property could not see it across the assembly boundary", key, alias.storage)
		}
	}

	for storage := range linknameVarAliasStorage {
		found := false

		for _, alias := range linknameVarAliasTargets {
			if alias.storage == storage {
				found = true
				break
			}
		}

		if !found {
			t.Errorf("linknameVarAliasStorage carries %q, which no registry row names: a var publicized for an alias that does not exist", storage)
		}
	}
}

// TestLinknameVarAliasPublicizesTheStorageSide exercises packageVarAccess itself rather than the
// index it reads. Publicizing the storage member is what lets the forwarding property compile across
// the assembly boundary at all — runtime's canUseLongPaths is unexported in Go and would otherwise be
// emitted `internal`, invisible to internal/syscall/windows.
//
// The negative arms matter as much as the positive one. The rule must key on the STORAGE PACKAGE, not
// on the bare name: a same-named var in an unrelated package must not be publicized on the strength
// of somebody else's registry row, which is precisely the over-broad shape the push direction's own
// comment warns about.
//
// RED PROOF: delete the linknameVarAliasStorage arm from packageVarAccess, or key it on goIDName
// alone — the first arm goes red, the last arm goes red respectively.
func TestLinknameVarAliasPublicizesTheStorageSide(t *testing.T) {
	savedPath := currentPackagePath
	savedHandles := linknameHandles

	linknameHandles = HashSet[string]{}

	t.Cleanup(func() {
		currentPackagePath = savedPath
		linknameHandles = savedHandles
	})

	for key, alias := range linknameVarAliasTargets {
		storagePkg, storageName, ok := splitLastDot(alias.storage)

		if !ok {
			continue
		}

		t.Run(alias.storage, func(t *testing.T) {
			// While converting the STORAGE package, the storage var is public.
			currentPackagePath = storagePkg

			if access := packageVarAccess(storageName, types.Typ[types.Bool]); access != "public" {
				t.Errorf("packageVarAccess(%q) = %q while converting %q, want \"public\": the forwarding property emitted for %q reads it across an assembly boundary, and an unexported Go name is otherwise `internal` there",
					storageName, access, storagePkg, key)
			}

			// The same NAME in another package is NOT publicized — the row is about a pair, not a word.
			currentPackagePath = "go2cs.invalid/unrelated"

			if access := packageVarAccess(storageName, types.Typ[types.Bool]); access == "public" {
				t.Errorf("packageVarAccess(%q) = \"public\" while converting an unrelated package: the storage rule must key on the storage PACKAGE, or one row widens every same-named var in the corpus", storageName)
			}
		})
	}
}

// pkgDeclaresVar reports whether pkgPath declares symbol as a package-level var in Go's source.
func pkgDeclaresVar(t *testing.T, goRoot string, pkgPath string, symbol string) bool {
	t.Helper()

	for _, file := range parseGoPackageDir(t, goRoot, pkgPath) {
		for _, decl := range file.Decls {
			genDecl, isGen := decl.(*ast.GenDecl)

			if !isGen || genDecl.Tok != token.VAR {
				continue
			}

			for _, spec := range genDecl.Specs {
				valueSpec, isValue := spec.(*ast.ValueSpec)

				if !isValue {
					continue
				}

				for _, name := range valueSpec.Names {
					if name != nil && name.Name == symbol {
						return true
					}
				}
			}
		}
	}

	return false
}

// The one-argument handle check this file needs is pkgHasLinknameHandle, already defined by
// linknameForwardRegistry_test.go and deliberately shared: it asks Go's source the same question
// collectLinknameHandles asks the converter's input, and a second copy would be one more pair of
// definitions that can drift. The two-argument check is pkgHasLinknamePush
// (linknamePushRegistry_test.go) — a var alias's storage side carries the identical
// `//go:linkname <local> <pkg>.<remote>` form a push does, so the same scan answers both.
