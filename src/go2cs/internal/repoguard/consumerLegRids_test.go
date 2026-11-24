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
// which RIDs the consumer stages (release-smoke, aot-smoke, nugetgo-behavior, cgo-nuget-e2e) restore, build and run those
// packages on.
// The two are written in two languages and moved apart once: S6 shipped linux-arm64 in every package while the consumer
// stages kept four hard-coded legs, so no hosted leg ever consumed the flavour a release would publish for it. This test
// holds the consumer legs EQUAL to the shipped set, so a RID the pack gains is read on its own OS the day it ships, and a
// leg the matrix keeps for a RID the pack dropped fails here too.
var consumerLegBlock = regexp.MustCompile(`(?s)if \(\$env:INPUT_STAGE -in 'release-smoke', 'aot-smoke', 'nugetgo-behavior', 'cgo-nuget-e2e'\) \{\s*\$targets = @\((.*?)\)\s*Write-Host`)

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

// EVERY CONSUMER LEG THE CGO SMOKE SKIPS IS A LEG WITH NO PINNED C TOOLCHAIN, AND ONLY THAT (batch 308).
//
// release-smoke-cgo builds its probe library with the OS's DEFAULT pinned toolchain (.github/cgo-probe.ps1: llvm-mingw on
// windows, zig elsewhere, from .github/cgo-toolchain.ps1's one pin table). It ran on the consumer legs' matrix, so when
// seat S5c added linux-arm64 the cgo job inherited a leg with no pin and refused ("no zig pin for runner linux-arm64",
// run 43696423446). The plan step now emits a cgo matrix without the RIDs named in $cgoUnpinnedRids. This test holds
// that list to exactly the consumer legs whose default toolchain has no pin: a leg with no pin that the list does not
// name fails here (the refusal reached the run first), and a listed RID that gains a pin fails here too, so the list
// retires the day the pin lands and the leg rejoins the cgo smoke.
var cgoUnpinnedRidsLine = regexp.MustCompile(`\$cgoUnpinnedRids = @\(([^)]*)\)`)

// consumerLegTarget is one target's goos and rid in the consumer-leg block.
var consumerLegTarget = regexp.MustCompile(`goos = '([^']+)';\s*runner = '[^']+';\s*rid = '([^']+)'`)

// cgoPinKey is one key of cgo-toolchain.ps1's pin table, 'toolchain|rid'.
var cgoPinKey = regexp.MustCompile(`'(zig|llvm-mingw)\|([^']+)'\s*=`)

// quotedWord is one single-quoted item of a PowerShell array literal.
var quotedWord = regexp.MustCompile(`'([^']+)'`)

func TestCgoSmokeSkipsOnlyTheLegsWithNoPinnedToolchain(t *testing.T) {
	workflow := readRepoFile(t, ".github/workflows/os-matrix.yml")
	blocks := consumerLegBlock.FindAllStringSubmatch(workflow, -1)

	if len(blocks) != 1 {
		t.Fatalf(".github/workflows/os-matrix.yml: %d consumer-leg target lists match, want 1", len(blocks))
	}

	lists := cgoUnpinnedRidsLine.FindAllStringSubmatch(workflow, -1)

	if len(lists) != 1 {
		t.Fatalf(".github/workflows/os-matrix.yml: %d $cgoUnpinnedRids lists, want 1: release-smoke-cgo reads the consumer legs less the RIDs it names", len(lists))
	}

	var unpinned []string

	for _, word := range quotedWord.FindAllStringSubmatch(lists[0][1], -1) {
		unpinned = append(unpinned, word[1])
	}

	pins := map[string]bool{}

	for _, key := range cgoPinKey.FindAllStringSubmatch(readRepoFile(t, ".github/cgo-toolchain.ps1"), -1) {
		pins[key[1]+"|"+key[2]] = true
	}

	if len(pins) == 0 {
		t.Fatalf(".github/cgo-toolchain.ps1: no pin keys match %s; the table moved and this test reads nothing", cgoPinKey)
	}

	var want []string
	legs := consumerLegTarget.FindAllStringSubmatch(blocks[0][1], -1)

	if len(legs) == 0 {
		t.Fatalf("the consumer-leg block names no goos/rid targets matching %s", consumerLegTarget)
	}

	for _, leg := range legs {
		toolchain := "zig"

		if leg[1] == "windows" {
			toolchain = "llvm-mingw"
		}

		if !pins[toolchain+"|"+leg[2]] {
			want = append(want, leg[2])
		}
	}

	slices.Sort(want)
	slices.Sort(unpinned)

	if !slices.Equal(want, unpinned) {
		t.Errorf("$cgoUnpinnedRids is %v; the consumer legs whose default cgo toolchain has no pin in .github/cgo-toolchain.ps1 are %v. A leg with no pin runs the cgo smoke into a refusal; a listed RID with a pin is skipped for nothing (retire it from the list).", unpinned, want)
	}
}
