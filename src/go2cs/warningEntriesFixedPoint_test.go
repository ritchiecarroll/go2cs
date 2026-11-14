// warningEntriesFixedPoint_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// cnrTargets are the targets a behavioral project is converted for by check-no-regression.ps1 on each host that runs
// it. CNR passes no -platforms, so the target is the host's own; -platforms reproduces each host's conversion
// exactly, because the host reaches the warning entries only through that default target (warningEntries.go).
var cnrTargets = []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

// A behavioral project's committed .editorconfig (warningEntries.go) records each warning fact with the GOOS flavours
// whose emission holds it, and a conversion replaces only its own target's flavour. So the committed file is stable
// under every host's CNR only when it is the FIXED POINT of a conversion for each target: every flavour listed exactly
// where its fact holds. One captured from a single target is not, and the hosts it never saw rewrite it on every run
// (CgoPseudoPackageTypes, captured on windows alone, read "linux windows" on every linux CNR; BOARD, 2026-11-11).
// Each committed entry file is converted here in a copy of its project for each target, and must come back unchanged.
func TestBehavioralWarningEntriesAreEveryTargetsFixedPoint(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: converts each behavioral project that carries warning entries, once per target")
	}

	behavioral, err := filepath.Abs(filepath.Join("..", "tests", "Behavioral"))

	if err != nil {
		t.Fatal(err)
	}

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	projects, err := filepath.Glob(filepath.Join(behavioral, "*", ".editorconfig"))

	if err != nil {
		t.Fatal(err)
	}

	if len(projects) == 0 {
		t.Fatal("no behavioral project carries a .editorconfig; this guard would hold nothing")
	}

	for _, entries := range projects {
		project := filepath.Dir(entries)

		committed, err := os.ReadFile(entries)

		if err != nil {
			t.Fatal(err)
		}

		for _, target := range cnrTargets {
			t.Run(filepath.Base(project)+"/"+strings.ReplaceAll(target, "/", "-"), func(t *testing.T) {
				dir := t.TempDir()
				copyWarningEntriesProject(t, project, dir)

				cmd := exec.Command(driverBinary(t), "-go2cspath", treeSrc, "-platforms", target, dir, dir)
				cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=")

				if output, err := cmd.CombinedOutput(); err != nil {
					t.Fatalf("the conversion for %s failed: %v\n%s", target, err, output)
				}

				converted, err := os.ReadFile(filepath.Join(dir, ".editorconfig"))

				if err != nil {
					t.Fatal(err)
				}

				if got, want := normalizedEntries(converted), normalizedEntries(committed); got != want {
					t.Errorf("a %s conversion rewrites the committed warning entries, so a %s host's CNR reports them CHANGED on every run; write the file from a conversion for each target in turn\ncommitted:\n%s\nconverted:\n%s",
						target, strings.Split(target, "/")[0], want, got)
				}
			})
		}
	}
}

// copyWarningEntriesProject copies what a conversion of a behavioral project reads, its Go sources, go.mod and go.sum,
// and its committed .editorconfig, into dir.
func copyWarningEntriesProject(t *testing.T, project, dir string) {
	t.Helper()

	entries, err := os.ReadDir(project)

	if err != nil {
		t.Fatal(err)
	}

	for _, entry := range entries {
		name := entry.Name()

		if entry.IsDir() || !(strings.HasSuffix(name, ".go") || name == "go.mod" || name == "go.sum" || name == ".editorconfig") {
			continue
		}

		content, err := os.ReadFile(filepath.Join(project, name))

		if err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(filepath.Join(dir, name), content, 0o644); err != nil {
			t.Fatal(err)
		}
	}
}

// normalizedEntries is an entry file's text with its line endings unified: the file is pinned to CRLF in git
// (.gitattributes), and a checkout may hold either.
func normalizedEntries(content []byte) string {
	return strings.ReplaceAll(string(content), "\r\n", "\n")
}
