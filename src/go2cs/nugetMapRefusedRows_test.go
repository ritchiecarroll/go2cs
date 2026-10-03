// nugetMapRefusedRows_test.go - Gbtc
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

// A row the ID PATTERN refuses (a "go."-prefixed ID, owner ruling 2026-10-02) is SKIPPED with a warning naming the
// source and line, and the rest of the source still answers (COORD 2026-10-03, from TRAIN N's shared-file review).
// The pattern is a POLICY a newer converter can apply more strictly than an older registry row was linted against,
// so one such row must never stop every user's conversion -- under S3b the registry is read by default. A FORMAT
// fault still refuses the source (TestNuGetMapRefusesAMalformedSourceByFileAndLine): that signals corruption.
func TestNuGetMapSkipsARowTheIDPatternRefusesWithAWarning(t *testing.T) {
	path := filepath.Join(t.TempDir(), "mine.txt")
	body := mapRow("github.com/a/b", "nugetgo.github.com.a.b", "community") +
		mapRow("github.com/c/d", "go.github.com.c.d", "canonical") +
		mapRow("github.com/e/f", "nugetgo.github.com.e.f", "canonical")

	if err := os.WriteFile(path, []byte(body), 0o644); err != nil {
		t.Fatal(err)
	}

	src, warnings, err := loadNuGetMapSource(path, false)

	if err != nil {
		t.Fatalf("one go.-prefixed row refused the whole source: %v", err)
	}

	if len(src.rows) != 2 || src.rows["github.com/a/b"].nugetID == "" || src.rows["github.com/e/f"].nugetID == "" {
		t.Errorf("rows %+v; want the two valid rows kept", src.rows)
	}

	if _, kept := src.rows["github.com/c/d"]; kept {
		t.Errorf("the refused row was kept: %+v", src.rows["github.com/c/d"])
	}

	if len(warnings) != 1 || !strings.Contains(warnings[0], path+":2") || !strings.Contains(warnings[0], `uses the "go." prefix`) || !strings.Contains(warnings[0], "skipped") {
		t.Errorf("warnings %q; want one naming %s:2, the go. prefix and the skip", warnings, path)
	}
}

// The lock is the FOURTH reader of a mapping's ID (after the source parser, the registry lint and the pack): a pin
// the ID pattern refuses is DROPPED with a warning and the module re-resolves from the sources, so a lock written
// before the rule (or edited by hand) can neither apply a reserved ID nor stop the conversion.
func TestNuGetLockDropsAPinTheIDPatternRefuses(t *testing.T) {
	lock := map[string]nugetLockEntry{
		modA.path: {module: modA.path, version: "v1.0.0", nugetID: "go.github.com.a.a", status: "canonical", layer: "registry", packageVersion: "-", contentHash: "-"},
		modB.path: {module: modB.path, version: "v2.1.0", nugetID: "Go.Github.com.b.b", status: "community", layer: "registry", packageVersion: "-", contentHash: "-"},
	}
	registry := mustSource(t, "registry", mapRow(modA.path, "nugetgo.github.com.a.a", "canonical"))

	decisions, warnings := resolveNuGetMappings([]thirdPartyModule{modA, modB}, []nugetMapSource{registry}, lock, nugetMapOptions{sources: []string{"x"}})

	if a := decisionFor(t, decisions, modA.path); !a.mapped || a.nugetID != "nugetgo.github.com.a.a" || a.fromLock {
		t.Errorf("A: %+v; want the pin dropped and the registry's nugetgo.github.com.a.a adopted", a)
	}

	if b := decisionFor(t, decisions, modB.path); b.mapped || b.fromLock {
		t.Errorf("B: %+v; want the pin dropped and the module unmapped (no source maps it)", b)
	}

	if len(warnings) != 2 {
		t.Fatalf("warnings %q; want one per dropped pin", warnings)
	}

	for i, id := range []string{"go.github.com.a.a", "Go.Github.com.b.b"} {
		if !strings.Contains(warnings[i], nugetLockFileName) || !strings.Contains(warnings[i], id) || !strings.Contains(warnings[i], `uses the "go." prefix`) {
			t.Errorf("warning %q; want it to name %s, the pinned %s and the go. prefix", warnings[i], nugetLockFileName, id)
		}
	}
}
