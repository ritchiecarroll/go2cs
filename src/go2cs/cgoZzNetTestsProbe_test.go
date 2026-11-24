// cgoZzNetTestsProbe_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// PROBE, never landed (claude/h-cgo-res-4b-probe only). Seat 4b's first reading (DESIGN-cgo-phase3-marshaling section
// 19, Q2 to Q4): go2cs -cgo -tests over GOROOT's net, into a scratch root seeded from this checkout's src, filtered to
// the seven TestCgoLookup tests. It logs and always passes.

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

func TestZzCgoNetTestsProbe(t *testing.T) {
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1", cgoBuildEnv)
	}

	goRoot, err := exec.Command("go", "env", "GOROOT").Output()

	if err != nil {
		t.Skipf("go env GOROOT: %v", err)
	}

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	scratch := filepath.Join(t.TempDir(), "src")

	if err := os.MkdirAll(scratch, 0o755); err != nil {
		t.Fatal(err)
	}

	// The seed: what a -tests build reads beside core (the generator, the props, the toolchain pin).
	for _, entry := range []string{"core", "gen", "Directory.Build.props", "version.props"} {
		if out, err := exec.Command("cp", "-r", filepath.Join(treeSrc, entry), filepath.Join(scratch, entry)).CombinedOutput(); err != nil {
			t.Fatalf("seed %s: %v\n%s", entry, err, out)
		}
	}

	// go2cs-gen embeds the converter's field-collision list (go2cs-gen.csproj's one input outside gen).
	if err := os.MkdirAll(filepath.Join(scratch, "go2cs"), 0o755); err != nil {
		t.Fatal(err)
	}

	if out, err := exec.Command("cp", filepath.Join(treeSrc, "go2cs", "fieldAccessCollisions.txt"), filepath.Join(scratch, "go2cs")).CombinedOutput(); err != nil {
		t.Fatalf("seed fieldAccessCollisions.txt: %v\n%s", err, out)
	}

	input := filepath.Join(strings.TrimSpace(string(goRoot)), "src", "net")
	output := filepath.Join(scratch, "core", "net")
	args := []string{"-go2cspath", scratch, "-cgo", "-tests", "-test-action", "all", "-test-timeout", "20m",
		"-test-filter", "^TestCgoLookup", input, output}

	cmd := exec.Command(driverBinary(t), args...)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1")
	combined, err := cmd.CombinedOutput()

	t.Logf("PROBE exit: %v", err)

	seen := map[string]bool{}

	for _, line := range strings.Split(string(combined), "\n") {
		if at := strings.Index(line, ": error "); at >= 0 && !seen[line[at:]] {
			seen[line[at:]] = true
			t.Logf("PROBE error: %s", strings.TrimSpace(line))
		}
	}

	t.Logf("PROBE distinct errors: %d", len(seen))

	for _, line := range strings.Split(lastLines(string(combined), 120), "\n") {
		t.Logf("PROBE out: %s", line)
	}

	if data, readErr := os.ReadFile(filepath.Join(output, "go2cs_test_comparison.json")); readErr == nil {
		for _, line := range strings.Split(string(data), "\n") {
			t.Logf("PROBE comparison: %s", line)
		}
	} else {
		t.Logf("PROBE comparison: none (%v)", readErr)
	}
}
