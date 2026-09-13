// repoRoot_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"testing"
)

// repoRootFromPackageDir walks up from the package directory to the first ancestor holding a .git
// entry, which is the repository root.
//
// WHY A SECOND COPY EXISTS. The repo-wide guards moved to src/go2cs/internal/repoguard (e2f9b118f) and
// took this helper with them. That was right for master, where no remaining package-main test called
// it. But three train-47 seats cut before the move add package-main tests that DO call it --
// platformScopedDisclosures_test.go, valueCloneStampMembers_test.go and duplicatePartialMembers_test.go
// -- and a Go package's tests compile as ONE unit, so after a rebase onto master a single undefined
// symbol stops the whole converter test build, not three tests. Measured before this file existed:
// master alone vets clean; master plus the one file coord-stamp-guard adds fails
// `undefined: repoRootFromPackageDir`. Each seat was green on its own branch, so no per-branch gate
// could see it -- a clean merge whose result compiles nowhere.
//
// This is a DELIBERATE duplicate of the helper in internal/repoguard/fleetIdentifierCensus_test.go.
// The two live in different packages and a _test.go symbol cannot be shared between them. KEEP THEM IN
// LOCKSTEP: two derivations of one predicate drift apart, and the one that drifts reads a wrong root.
//
// It SEARCHES rather than counting levels, because a fixed depth breaks on every move. .git is tested
// with os.Stat and never IsDir, because in a git worktree it is a FILE naming the real git directory.
// No .git anywhere above is a loud failure: a guard that read a wrong root would scan the wrong tree
// and pass.
func repoRootFromPackageDir(t *testing.T) string {
	t.Helper()
	wd, err := os.Getwd()
	if err != nil {
		t.Fatalf("cannot determine the working directory: %v", err)
	}
	for dir := wd; ; {
		if _, err := os.Stat(filepath.Join(dir, ".git")); err == nil {
			return dir
		}
		parent := filepath.Dir(dir)
		if parent == dir {
			t.Fatalf("repository root not found: no .git entry in %s or any ancestor", wd)
		}
		dir = parent
	}
}
