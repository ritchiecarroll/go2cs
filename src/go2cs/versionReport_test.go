// versionReport_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"crypto/sha256"
	"encoding/hex"
	"os"
	"os/exec"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// versionReportKeys is the shape `go2cs -version` promises a bug report or a build script: one
// key=value pair per line, in this order, every key always present ("unknown" when a value cannot be
// resolved). Adding a key appends it; renaming or reordering one breaks every consumer that parses it.
var versionReportKeys = []string{
	"go2cs.release",
	"go2cs.converter",
	"go.packages",
	"go.toolchain",
	"go.build",
	"vcs.revision",
	"vcs.modified",
}

// parseVersionReport checks the line shape and key order and returns the values by key.
func parseVersionReport(t *testing.T, report string) map[string]string {
	t.Helper()

	if !strings.HasSuffix(report, "\n") {
		t.Fatalf("report does not end in a newline:\n%q", report)
	}

	lines := strings.Split(strings.TrimSuffix(report, "\n"), "\n")

	if len(lines) != len(versionReportKeys) {
		t.Fatalf("report has %d lines, want %d:\n%s", len(lines), len(versionReportKeys), report)
	}

	values := make(map[string]string, len(lines))

	for i, line := range lines {
		key, value, ok := strings.Cut(line, "=")

		if !ok || key != versionReportKeys[i] {
			t.Fatalf("line %d is %q, want key %q", i+1, line, versionReportKeys[i])
		}

		if value == "" || strings.ContainsAny(value, " \t\r") {
			t.Fatalf("line %d value %q is empty or not a single token", i+1, value)
		}

		values[key] = value
	}

	return values
}

func checkVersionReportValues(t *testing.T, values map[string]string, converter string) {
	t.Helper()

	want := map[string]string{
		"go2cs.release":   corpusRelease(),
		"go2cs.converter": converter,
		"go.build":        runtime.Version(),
		"go.toolchain":    "unknown",
		"go.packages":     "unknown",
	}

	if version := goVersion(); version != "" {
		want["go.toolchain"] = "go" + version
		want["go.packages"] = version + ".*"
	}

	for key, value := range want {
		if values[key] != value {
			t.Errorf("%s = %q, want %q", key, values[key], value)
		}
	}

	revision, modified := values["vcs.revision"], values["vcs.modified"]

	if revision != "unknown" && !regexp.MustCompile(`^[0-9a-f]{40}([0-9a-f]{24})?$`).MatchString(revision) {
		t.Errorf("vcs.revision = %q, want a full commit hash or unknown", revision)
	}

	if (revision == "unknown") != (modified == "unknown") || (modified != "unknown" && modified != "true" && modified != "false") {
		t.Errorf("vcs.modified = %q with vcs.revision = %q: want true/false with a revision, unknown without", modified, revision)
	}
}

func TestVersionReportShape(t *testing.T) {
	values := parseVersionReport(t, versionReport())
	checkVersionReportValues(t, values, converterRevision())
}

// TestVersionFlagPrintsTheReport drives the real command line: -version needs no input, exits 0, writes
// the report to stdout and nothing to stderr, and names THIS binary by its executable hash.
func TestVersionFlagPrintsTheReport(t *testing.T) {
	binary := driverBinary(t)

	var stdout, stderr bytes.Buffer
	command := exec.Command(binary, "-version")
	command.Stdout, command.Stderr = &stdout, &stderr

	if err := command.Run(); err != nil {
		t.Fatalf("go2cs -version: %v\nstderr:\n%s", err, stderr.String())
	}

	if stderr.Len() != 0 {
		t.Errorf("go2cs -version wrote to stderr:\n%s", stderr.String())
	}

	data, err := os.ReadFile(binary)

	if err != nil {
		t.Fatal(err)
	}

	digest := sha256.Sum256(data)
	values := parseVersionReport(t, stdout.String())
	checkVersionReportValues(t, values, "exe-"+hex.EncodeToString(digest[:8]))
}
