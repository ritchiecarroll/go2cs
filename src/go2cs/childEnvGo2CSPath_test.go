// childEnvGo2CSPath_test.go - Gbtc
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
	"strings"
	"testing"
)

// go2csPathEntries returns every entry of a child environment whose NAME matches go2csPath
// case-insensitively — i.e. every entry MSBuild would fold into the single $(go2csPath) property.
// The count is the whole point of these tests: two entries is the race, one is the invariant.
func go2csPathEntries(env []string) []string {
	matches := []string{}

	for _, entry := range env {
		if name, _, found := strings.Cut(entry, "="); found && strings.EqualFold(name, "go2csPath") {
			matches = append(matches, entry)
		}
	}

	return matches
}

// TestChildEnvCarriesSingleGo2CSPathSpelling reproduces the SHAPE of the Linux pipeline race at the
// child-environment builder: a parent environment holding several case-variants of the root — which
// only a POSIX environment block can express, so the parent env is constructed literally rather than
// read from the OS, and the test therefore measures the same thing on every platform. Exactly one
// entry must survive, spelled canonically and carrying the resolved root.
func TestChildEnvCarriesSingleGo2CSPathSpelling(t *testing.T) {
	root := filepath.Join(string(filepath.Separator)+"repo", "src")

	parentEnv := []string{
		"PATH=/usr/bin",
		"GO2CSPATH=/root/go2cs",
		"GOROOT=/usr/local/go",
		"go2csPath=/stale/tree/",
		"Go2CsPath=/another/tree",
		"HOME=/root",
	}

	env := childEnvWithGo2CSPath(parentEnv, root)
	matches := go2csPathEntries(env)

	if len(matches) != 1 {
		t.Fatalf("child environment carries %d go2csPath spellings, want exactly 1: %v", len(matches), matches)
	}

	expected := "go2csPath=" + ensureTrailingSeparator(root)

	if matches[0] != expected {
		t.Errorf("surviving entry is %q, want %q", matches[0], expected)
	}

	// The un-slashed value is what concatenated $(go2csPath)gen/... into /root/go2csgen/...; assert
	// the separator explicitly so a future edit cannot drop it while still passing the count check.
	if !strings.HasSuffix(matches[0], string(filepath.Separator)) {
		t.Errorf("surviving entry %q is not separator-terminated", matches[0])
	}
}

// TestChildEnvPreservesUnrelatedVariables holds the scrub to its scope: only go2csPath variants are
// dropped. GOROOT and PATH in particular are appended by runCommandWithTimeout on top of this
// environment, and other variables carry the toolchain the pipeline's children need.
func TestChildEnvPreservesUnrelatedVariables(t *testing.T) {
	parentEnv := []string{
		"PATH=/usr/bin",
		"GO2CSPATH=/root/go2cs",
		"GOROOT=/usr/local/go",
		"GOPATH=/root/go",
		"GO2CS_PPROF=:6060",
	}

	env := childEnvWithGo2CSPath(parentEnv, "/repo/src")

	for _, want := range []string{"PATH=/usr/bin", "GOROOT=/usr/local/go", "GOPATH=/root/go", "GO2CS_PPROF=:6060"} {
		found := false

		for _, entry := range env {
			if entry == want {
				found = true
				break
			}
		}

		if !found {
			t.Errorf("child environment dropped unrelated variable %q", want)
		}
	}

	// GO2CS_PPROF shares the GO2CS prefix but is a different variable — a prefix-based scrub would
	// eat it, so its survival above is a real assertion, not filler.
	if len(env) != len(parentEnv) {
		t.Errorf("child environment holds %d entries, want %d (one variant scrubbed, one appended)", len(env), len(parentEnv))
	}
}

// TestGo2CSPathDefaultIsNotExported is the other half of the invariant: the converter resolves its
// own default and keeps it to itself. Exporting the derived value is what put a second, un-slashed
// spelling into every pipeline child's environment in the first place.
func TestGo2CSPathDefaultIsNotExported(t *testing.T) {
	t.Setenv("GO2CSPATH", "")

	resolved := resolveGo2CSPathDefault(filepath.Join(string(filepath.Separator)+"root", "go"))

	if len(resolved) == 0 {
		t.Fatal("resolveGo2CSPathDefault returned an empty default")
	}

	if exported := os.Getenv("GO2CSPATH"); len(exported) > 0 {
		t.Errorf("converter exported its derived root as GO2CSPATH=%q; the value is consumed as the "+
			"-go2cspath flag default and must never reach a child environment as a second spelling", exported)
	}
}

// TestUserSetGo2CSPathIsHonored pins the clause that keeps the Linux harness pin working: an
// environment the OPERATOR set is still the flag's default, verbatim.
func TestUserSetGo2CSPathIsHonored(t *testing.T) {
	chosen := filepath.Join(string(filepath.Separator)+"chosen", "runtime", "root")
	t.Setenv("GO2CSPATH", chosen)

	if resolved := resolveGo2CSPathDefault("/root/go"); resolved != chosen {
		t.Errorf("resolveGo2CSPathDefault = %q, want the user-set %q", resolved, chosen)
	}
}

// TestUserSetGo2CSPathDoesNotReachChildAsSecondSpelling composes both halves against the exact
// configuration the Linux campaign runs today: the harness PINS GO2CSPATH, and the converter is
// additionally handed an explicit -go2cspath. Honoring the pin as a default must not put it in the
// child environment beside the resolved root.
func TestUserSetGo2CSPathDoesNotReachChildAsSecondSpelling(t *testing.T) {
	parentEnv := []string{"GO2CSPATH=/pinned/but/different"}

	matches := go2csPathEntries(childEnvWithGo2CSPath(parentEnv, "/repo/src"))

	if len(matches) != 1 {
		t.Fatalf("child environment carries %d go2csPath spellings, want exactly 1: %v", len(matches), matches)
	}

	if strings.Contains(matches[0], "pinned") {
		t.Errorf("child environment forwarded the ambient root %q; the resolved -go2cspath is the one answer", matches[0])
	}
}
