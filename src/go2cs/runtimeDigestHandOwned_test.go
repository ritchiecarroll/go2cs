// runtimeDigestHandOwned_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards what the test manifest's runtime digest covers. runtimeSourcesDigest is the "the runtime the converted tests
// build against changed" half of a manifest's inputDigest (validateTestManifest refuses a stale one). It hashed every
// *.cs under core/golib and core/testing -- but core/testing also receives the `testing` row's OWN -tests emission
// (testing_test.cs, helper_test.cs, go2cs_test_host.cs, package_test_info.cs, ...), so re-emitting that one row moved
// every other package's digest, and the digest depended on whether `testing` had been converted first (measured on
// the -tests footprint: 214 manifests moved on an unchanged re-conversion, every row before `testing`). In
// core/testing it now hashes only the hand-owned host, the files carrying `[module: go.GoManualConversion]`.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

const runtimeDigestMarkerLine = "[module: go.GoManualConversion]"

// stageRuntimeRoot writes a go2csPath with a golib file, a hand-owned host file (marked) and a -tests-emitted test
// source (unmarked) under core/testing, and returns the go2csPath and the two core/testing paths.
func stageRuntimeRoot(t *testing.T) (go2csPath, host, emitted string) {
	t.Helper()

	go2csPath = t.TempDir()
	golib := filepath.Join(go2csPath, "core", "golib")
	testingDir := filepath.Join(go2csPath, "core", "testing")

	for _, dir := range []string{golib, testingDir} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	host = filepath.Join(testingDir, "TestHost.cs")
	emitted = filepath.Join(testingDir, "helper_test.cs")

	writeRuntimeFile(t, filepath.Join(golib, "slice.cs"), "namespace go;\npublic struct slice {}\n")
	writeRuntimeFile(t, host, "// TestHost.cs - Gbtc\n"+runtimeDigestMarkerLine+"\nnamespace go.testing_runtime;\npublic static class TestHost {}\n")
	writeRuntimeFile(t, emitted, "// Copyright 2017 The Go Authors. All rights reserved.\nnamespace go;\npartial class testing_package {}\n")

	return go2csPath, host, emitted
}

func writeRuntimeFile(t *testing.T, path, content string) {
	t.Helper()

	if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
		t.Fatal(err)
	}
}

func runtimeDigestOf(go2csPath string) string {
	return runtimeSourcesDigest(Options{go2csPath: go2csPath})
}

// The `testing` row re-emitting its own test sources is not a runtime change.
func TestRuntimeDigestIgnoresTestsEmittedSourcesUnderCoreTesting(t *testing.T) {
	go2csPath, _, emitted := stageRuntimeRoot(t)
	before := runtimeDigestOf(go2csPath)

	writeRuntimeFile(t, emitted, "// Copyright 2017 The Go Authors. All rights reserved.\nnamespace go;\npartial class testing_package { /* re-emitted */ }\n")

	if after := runtimeDigestOf(go2csPath); after != before {
		t.Errorf("re-emitting a -tests source under core/testing moved the runtime digest (%s -> %s): the digest must cover only the hand-owned host", before, after)
	}
}

// The control: a change to the hand-owned host IS a runtime change, and to golib too.
func TestRuntimeDigestMovesWithTheHandOwnedHostAndGolib(t *testing.T) {
	go2csPath, host, _ := stageRuntimeRoot(t)
	before := runtimeDigestOf(go2csPath)

	writeRuntimeFile(t, host, "// TestHost.cs - Gbtc\n"+runtimeDigestMarkerLine+"\nnamespace go.testing_runtime;\npublic static class TestHost { /* changed */ }\n")

	afterHost := runtimeDigestOf(go2csPath)

	if afterHost == before {
		t.Errorf("a change to the hand-owned host did not move the runtime digest (%s)", before)
	}

	writeRuntimeFile(t, filepath.Join(go2csPath, "core", "golib", "slice.cs"), "namespace go;\npublic struct slice { /* changed */ }\n")

	if runtimeDigestOf(go2csPath) == afterHost {
		t.Errorf("a change to golib did not move the runtime digest (%s)", afterHost)
	}
}

// A hand-owned file that LOSES its marker drops out of the digest -- which moves it -- and is NAMED in a warning, so a
// deleted marker cannot silently shrink what the stale check covers.
func TestRuntimeDigestNamesAHostFileThatLostItsMarker(t *testing.T) {
	go2csPath, host, _ := stageRuntimeRoot(t)
	before := runtimeDigestOf(go2csPath)

	writeRuntimeFile(t, host, "// TestHost.cs - Gbtc\nnamespace go.testing_runtime;\npublic static class TestHost {}\n")

	var after string
	warnings := captureStderr(t, func() { after = runtimeDigestOf(go2csPath) })

	if after == before {
		t.Errorf("a host file that lost its marker left the runtime digest unchanged (%s); it must drop out", before)
	}

	// DROPS OUT, not merely "changed": the digest equals the one taken with the file absent.
	if err := os.Remove(host); err != nil {
		t.Fatal(err)
	}

	var absent string
	captureStderr(t, func() { absent = runtimeDigestOf(go2csPath) })

	if after != absent {
		t.Errorf("a host file that lost its marker must drop out of the digest: with it %s, without it %s", after, absent)
	}

	if !strings.Contains(warnings, "TestHost.cs") || !strings.Contains(warnings, "GoManualConversion") {
		t.Errorf("a host file that lost its marker must be named in a warning; stderr was:\n%s", warnings)
	}

	// The emitted test source never carried the marker and is not named.
	if strings.Contains(warnings, "helper_test.cs") {
		t.Errorf("a -tests-emitted source is not a lost marker and must not be named:\n%s", warnings)
	}
}
