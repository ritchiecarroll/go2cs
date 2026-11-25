// cgoScratchMarker_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os/exec"
	"strings"
	"testing"
)

// The repository tracks no .go2cs-cgo-scratch-root. -cgo -tests converts only into a root that carries it (the
// converter's checkCgoTestsOutput, cgo Phase 3 increment 3.4, DESIGN-cgo-phase3-marshaling 19.8), and only a scratch
// root's seed step writes it. A marker copied back into the tree would mark the repository's own src, and a cgo-on
// conversion with its removal of deselected files could then run against the corpus (P3's NIT 3 on the 4a/4c fold).
func TestTheRepositoryTracksNoCgoScratchRootMarker(t *testing.T) {
	root := repoRootFromPackageDir(t)

	out, err := exec.Command("git", "-C", root, "ls-files").Output()

	if err != nil {
		t.Fatalf("git ls-files: %v", err)
	}

	tracked := strings.Fields(string(out))

	if len(tracked) == 0 {
		t.Fatal("premise: git ls-files listed nothing, so the check would be vacuous")
	}

	for _, path := range tracked {
		if path == ".go2cs-cgo-scratch-root" || strings.HasSuffix(path, "/.go2cs-cgo-scratch-root") {
			t.Errorf("the repository tracks %s: only a scratch root's seed step writes that marker, and it must never be committed", path)
		}
	}
}
