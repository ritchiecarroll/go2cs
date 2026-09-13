// resolveGo2CSPath_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"io"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// captureConverterStderr runs fn with os.Stderr redirected to a pipe and returns what it wrote.
// showWarning writes to os.Stderr at call time, so swapping the variable captures the real,
// user-visible diagnostic rather than a stand-in for it.
func captureConverterStderr(t *testing.T, fn func()) string {
	t.Helper()

	reader, writer, err := os.Pipe()

	if err != nil {
		t.Fatalf("os.Pipe: %v", err)
	}

	saved := os.Stderr
	os.Stderr = writer

	fn()

	os.Stderr = saved

	if err := writer.Close(); err != nil {
		t.Fatalf("close pipe writer: %v", err)
	}

	var captured bytes.Buffer

	if _, err := io.Copy(&captured, reader); err != nil {
		t.Fatalf("read captured stderr: %v", err)
	}

	if err := reader.Close(); err != nil {
		t.Fatalf("close pipe reader: %v", err)
	}

	return captured.String()
}

// makeGo2CSRoot creates the one marker isGo2CSRoot looks for — core\golib\golib.csproj — under dir,
// making dir a go2cs project-reference root as far as the resolver is concerned.
func makeGo2CSRoot(t *testing.T, dir string) string {
	t.Helper()

	golib := filepath.Join(dir, "core", "golib")

	if err := os.MkdirAll(golib, 0o755); err != nil {
		t.Fatalf("mkdir %s: %v", golib, err)
	}

	if err := os.WriteFile(filepath.Join(golib, "golib.csproj"), []byte("<Project />\n"), 0o644); err != nil {
		t.Fatalf("write golib.csproj: %v", err)
	}

	return dir
}

// pinRootWarning resets the once-per-run warning latch for the duration of one test, in the
// goModCache manner, so a test that expects a warning is not silenced by an earlier one.
func pinRootWarning(t *testing.T) {
	t.Helper()

	saved := go2csRootWarned
	go2csRootWarned = false

	t.Cleanup(func() { go2csRootWarned = saved })
}

// TestResolveGo2CSPathSelfLocation guards the recovery side of the 2026-08-06 board finding: a
// conversion whose configured -go2cspath is not a go2cs root (the ~/go2cs default on a box with no
// deploy-core staging) must self-locate the root from its OUTPUT path, so the emitted
// package_info.cs describes the same tree the output is compiled against — instead of silently
// dropping every <ImportedTypeAliases> entry. Network-free and filesystem-local.
func TestResolveGo2CSPathSelfLocation(t *testing.T) {
	root := makeGo2CSRoot(t, t.TempDir())
	outputDir := filepath.Join(root, "Tests", "Behavioral", "Thing")

	if err := os.MkdirAll(outputDir, 0o755); err != nil {
		t.Fatalf("mkdir %s: %v", outputDir, err)
	}

	pinRootWarning(t)

	options := Options{go2csPath: filepath.Join(t.TempDir(), "not-a-go2cs-root")}

	stderr := captureConverterStderr(t, func() {
		resolveGo2CSPath(&options, outputDir, true)
	})

	if options.go2csPath != root {
		t.Errorf("self-location did not recover the root from the output path:\n got %q\nwant %q", options.go2csPath, root)
	}

	if stderr != "" {
		t.Errorf("a recovered root must not warn, got:\n%s", stderr)
	}

	// An explicitly configured WORKING root wins over the walk: a caller pointing at a real,
	// different runtime root is making a deliberate choice, not a mistake to be corrected.
	configured := makeGo2CSRoot(t, filepath.Join(t.TempDir(), "configured"))
	options = Options{go2csPath: configured}

	resolveGo2CSPath(&options, outputDir, true)

	if options.go2csPath != configured {
		t.Errorf("a valid configured root was overridden by self-location: got %q, want %q", options.go2csPath, configured)
	}

	// selfLocate=false is the -recurse posture: the root doubles as the output root there, so it is
	// reported but never relocated.
	options = Options{go2csPath: filepath.Join(root, "Tests", "no-root-here")}
	before := options.go2csPath

	pinRootWarning(t)
	captureConverterStderr(t, func() {
		resolveGo2CSPath(&options, outputDir, false)
	})

	if options.go2csPath != before {
		t.Errorf("selfLocate=false relocated the root anyway: got %q, want %q", options.go2csPath, before)
	}
}

// TestResolveGo2CSPathUnusableRootWarns guards the loud-failure side: when no root can be found the
// run proceeds — converting standalone code with no deployed runtime is legitimate — but says so
// ONCE, naming the resolved path and the consequence, instead of emitting quietly alias-free
// metadata. It also pins the two suppressions: the once-per-run latch, and -recurse=nuget (which
// references the published packages and needs no local root at all).
func TestResolveGo2CSPathUnusableRootWarns(t *testing.T) {
	// A temp directory has no go2cs root anywhere above it, so both the configured root and the
	// output anchor are genuinely unresolvable.
	missing := filepath.Join(t.TempDir(), "no-such-root")
	outputDir := t.TempDir()

	pinRootWarning(t)

	options := Options{go2csPath: missing}

	stderr := captureConverterStderr(t, func() {
		resolveGo2CSPath(&options, outputDir, true)
	})

	if options.go2csPath != missing {
		t.Errorf("an unresolvable root must be left as configured: got %q, want %q", options.go2csPath, missing)
	}

	for _, want := range []string{"WARNING:", missing, filepath.Join("core", "golib", "golib.csproj"), "ImportedTypeAliases", "-go2cspath"} {
		if !strings.Contains(stderr, want) {
			t.Errorf("warning does not mention %q:\n%s", want, stderr)
		}
	}

	// Once per run: a conversion resolves dozens of imports and must not repeat the advice.
	repeat := captureConverterStderr(t, func() {
		resolveGo2CSPath(&options, outputDir, true)
	})

	if repeat != "" {
		t.Errorf("the unusable-root warning repeated within one run:\n%s", repeat)
	}

	// -recurse=nuget resolves the stdlib from the published go.<pkg>/go.lib/go.gen packages and the
	// converter's own embedded metadata, so a missing local root is not a defect there.
	pinRootWarning(t)

	nuget := Options{go2csPath: missing, recurse: true, nugetRefs: true}

	stderr = captureConverterStderr(t, func() {
		resolveGo2CSPath(&nuget, "", false)
	})

	if stderr != "" {
		t.Errorf("-recurse=nuget must not warn about a missing local root, got:\n%s", stderr)
	}

	if go2csRootWarned {
		t.Error("-recurse=nuget consumed the once-per-run warning latch")
	}
}
