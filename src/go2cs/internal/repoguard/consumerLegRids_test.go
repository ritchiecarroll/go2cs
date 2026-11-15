// consumerLegRids_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"regexp"
	"slices"
	"testing"
)

// EVERY RID THE PACK SHIPS IS A RID A CONSUMER LEG READS (seat S5c, DESIGN-linux-arm64-flavour.md section 6).
//
// push-nuget.ps1's $ridFlavors decides which runtimes/<rid>/ assets every go.* package carries. os-matrix.yml decides
// which RIDs the consumer stages (release-smoke, aot-smoke, nugetgo-behavior) restore, build and run those packages on.
// The two are written in two languages and moved apart once: S6 shipped linux-arm64 in every package while the consumer
// stages kept four hard-coded legs, so no hosted leg ever consumed the flavour a release would publish for it. This test
// holds the consumer legs EQUAL to the shipped set, so a RID the pack gains is read on its own OS the day it ships, and a
// leg the matrix keeps for a RID the pack dropped fails here too.
var consumerLegBlock = regexp.MustCompile(`(?s)if \(\$env:INPUT_STAGE -in 'release-smoke', 'aot-smoke', 'nugetgo-behavior'\) \{\s*\$targets = @\((.*?)\)\s*Write-Host`)

// consumerLegRid is one target's RID in that block.
var consumerLegRid = regexp.MustCompile(`rid = '([^']+)'`)

func TestConsumerLegsReadEveryShippedRid(t *testing.T) {
	blocks := consumerLegBlock.FindAllStringSubmatch(readRepoFile(t, ".github/workflows/os-matrix.yml"), -1)

	if len(blocks) != 1 {
		t.Fatalf(".github/workflows/os-matrix.yml: %d consumer-leg target lists match %s, want 1; the list moved and this pin reads nothing", len(blocks), consumerLegBlock)
	}

	var legs []string

	for _, rid := range consumerLegRid.FindAllStringSubmatch(blocks[0][1], -1) {
		legs = append(legs, rid[1])
	}

	maps := ridFlavorMaps[0]

	if maps.file != "src/push-nuget.ps1" {
		t.Fatalf("ridFlavorMaps[0] is %s, want src/push-nuget.ps1: the pack's own map is the shipped set", maps.file)
	}

	found := maps.pattern.FindAllStringSubmatch(readRepoFile(t, maps.file), -1)

	if len(found) != 1 {
		t.Fatalf("%s: %d RID maps match %s, want 1", maps.file, len(found), maps.pattern)
	}

	var shipped []string

	for _, entry := range ridFlavorEntry.FindAllStringSubmatch(found[0][1], -1) {
		shipped = append(shipped, entry[1])
	}

	if len(shipped) == 0 || len(legs) == 0 {
		t.Fatalf("read %d shipped RIDs and %d consumer legs; a pin over an empty list proves nothing", len(shipped), len(legs))
	}

	for _, rid := range shipped {
		if !slices.Contains(legs, rid) {
			t.Errorf("push-nuget.ps1 ships %s, but no consumer leg in os-matrix.yml reads it (legs: %v): a package flavour no hosted consumer has ever restored, built or run", rid, legs)
		}
	}

	for _, rid := range legs {
		if !slices.Contains(shipped, rid) {
			t.Errorf("os-matrix.yml runs a consumer leg on %s, which push-nuget.ps1 does not ship (shipped: %v)", rid, shipped)
		}
	}

	slices.Sort(legs)

	if dup := slices.Compact(slices.Clone(legs)); len(dup) != len(legs) {
		t.Errorf("os-matrix.yml lists a consumer RID twice: %v", legs)
	}
}
