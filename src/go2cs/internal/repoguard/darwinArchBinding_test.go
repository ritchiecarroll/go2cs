// darwinArchBinding_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"encoding/json"
	"regexp"
	"slices"
	"testing"
)

// EVERY STAGE THE FLAVOUR PROBE READS BINDS THE ARCH src/_paths.ps1 GIVES (seat D5, DESIGN-darwin-arm64-flavour.md
// section 6).
//
// On an Apple-silicon runner src/_paths.ps1 sets GoTargetArch=arm64 for every instrument that dot-sources it, and the
// flavour probe fails a leg whose planned arch is not the arch those instruments build. So the stages on which os-matrix's
// plan leaves the osx-arm64 leg UNBOUND must be exactly the stages the probe step skips: a stage the probe reads but the
// plan leaves unbound is a red on its first darwin dispatch (the probe expects amd64, _paths.ps1 gives arm64), and a stage
// the plan leaves unbound that the probe also skips builds nothing the probe could check. Both lists are literal JSON
// arrays in the workflow, one in the plan step's PowerShell and one in the probe step's if: expression; this holds them
// equal, as sets.
var darwinArchUnboundList = regexp.MustCompile(`\$darwinArchUnbound = '(\[[^']*\])' \| ConvertFrom-Json`)

// flavourProbeSkipList is the flavour probe step's if: skip list.
var flavourProbeSkipList = regexp.MustCompile(`(?m)- name: Flavour probe — the arch this leg compiles\s*\n\s*if: \$\{\{ !contains\(fromJson\('(\[[^']*\])'\), needs\.plan\.outputs\.stage\) \}\}`)

func TestProbedStagesBindTheArchPathsGives(t *testing.T) {
	const workflow = ".github/workflows/os-matrix.yml"

	text := readRepoFile(t, workflow)

	read := func(name string, pattern *regexp.Regexp) []string {
		found := pattern.FindAllStringSubmatch(text, -1)

		if len(found) != 1 {
			t.Fatalf("%s: %d %s lists match %s, want 1; the list moved and this pin reads nothing", workflow, len(found), name, pattern)
		}

		var stages []string

		if err := json.Unmarshal([]byte(found[0][1]), &stages); err != nil {
			t.Fatalf("%s: the %s list %s is not a JSON array of stage names: %v", workflow, name, found[0][1], err)
		}

		if len(stages) == 0 {
			t.Fatalf("%s: the %s list is empty", workflow, name)
		}

		slices.Sort(stages)

		return stages
	}

	unbound := read("darwin-arch-unbound", darwinArchUnboundList)
	skipped := read("flavour-probe skip", flavourProbeSkipList)

	for _, stage := range skipped {
		if !slices.Contains(unbound, stage) {
			t.Errorf("%s: the flavour probe skips %q, but the plan binds the osx-arm64 leg's arch on it: add it to $darwinArchUnbound, or probe it", workflow, stage)
		}
	}

	for _, stage := range unbound {
		if !slices.Contains(skipped, stage) {
			t.Errorf("%s: the plan leaves the osx-arm64 leg unbound on %q, but the flavour probe reads it, and src/_paths.ps1 gives arm64 on that runner, so the probe would fail it: bind it, or add it to the probe's skip list", workflow, stage)
		}
	}
}
