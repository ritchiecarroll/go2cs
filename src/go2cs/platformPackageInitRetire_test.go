// platformPackageInitRetire_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the multi-platform merge's retirement of a package_init.cs no target emitted. A conversion that relocates
// nothing retires its stale package_init.cs (retireStalePackageInitFile) — in its own staging root, which the merge
// copies FROM; a file no target emitted is in no merge plan, so the corpus kept the copy whose static constructor calls
// init methods the package no longer declares (CS0103). Measured when lazy package vars took every relocation of flag,
// html/template, internal/profile and math/big/internal/asmgen (19 dangling calls in the -platforms root).

package main

import (
	"os"
	"path/filepath"
	"testing"
)

func TestMergeRetiresAPackageInitNoTargetEmitted(t *testing.T) {
	coreDir := filepath.Join(t.TempDir(), "core")
	targets := []string{"windows/amd64", "linux/amd64"}
	generated := packageInitGeneratedHeader + "\r\nnamespace go;\r\n"

	write := func(rel, content string) {
		t.Helper()
		path := filepath.Join(coreDir, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	exists := func(rel string) bool {
		_, err := os.Stat(filepath.Join(coreDir, filepath.FromSlash(rel)))
		return err == nil
	}

	// retired: converted on both targets, neither emitted package_init.cs; a flat copy and a per-GOOS copy are stale
	write("retired/package_init.cs", generated)
	write("retired/linux/package_init.cs", generated)
	// handwritten: converted, not emitted, but the file carries no generated header
	write("handwritten/package_init.cs", "// hand-written\r\nnamespace go;\r\n")
	// kept: a target emitted it this run
	write("kept/package_init.cs", generated)
	// untouched: no target converted the package
	write("untouched/package_init.cs", generated)

	emission := func(target string) *platformEmission {
		return &platformEmission{target: target, artifacts: map[string]artifactState{
			"retired/a.cs":         {logical: "retired/a.cs", emitted: true},
			"handwritten/a.cs":     {logical: "handwritten/a.cs", emitted: true},
			"kept/a.cs":            {logical: "kept/a.cs", emitted: true},
			"kept/package_init.cs": {logical: "kept/package_init.cs", emitted: true},
			"untouched/a.cs":       {logical: "untouched/a.cs"},
		}}
	}

	emissions := []*platformEmission{emission(targets[0]), emission(targets[1])}
	emitted := map[string]bool{"retired/a.cs": true, "handwritten/a.cs": true, "kept/a.cs": true, "kept/package_init.cs": true}

	removed, err := retireUnemittedPackageInitFiles(coreDir, targets, emissions, emitted)

	if err != nil {
		t.Fatal(err)
	}

	if removed != 2 || exists("retired/package_init.cs") || exists("retired/linux/package_init.cs") {
		t.Errorf("retired: removed %d, want 2 (the flat and the per-GOOS generated copies)", removed)
	}

	for _, rel := range []string{"handwritten/package_init.cs", "kept/package_init.cs", "untouched/package_init.cs"} {
		if !exists(rel) {
			t.Errorf("%s must stay", rel)
		}
	}
}

// TestMergePlatformEmissionsRetiresAnAllLazyPackageInit drives the merge itself (mergePlatformEmissions, the step a
// -platforms run ends with) over a seeded root and two staged targets. allLazy is the shape the lazy seat produced: its
// every relocation went lazy, so no target emitted package_init.cs and the root's generated copy calls methods the new
// sources no longer declare. kept still relocates on both targets and keeps its file.
func TestMergePlatformEmissionsRetiresAnAllLazyPackageInit(t *testing.T) {
	root := t.TempDir()
	targets := []string{"windows/amd64", "linux/amd64"}
	generated := packageInitGeneratedHeader + "\r\nnamespace go;\r\n"

	write := func(base, rel, content string) {
		t.Helper()
		path := filepath.Join(base, "core", filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	read := func(rel string) (string, bool) {
		data, err := os.ReadFile(filepath.Join(root, "core", filepath.FromSlash(rel)))
		return string(data), err == nil
	}

	// the seeded corpus: the previous conversion relocated in both packages
	write(root, "allLazy/a.cs", "// relocated\r\n")
	write(root, "allLazy/package_init.cs", generated+"// initᴛtable();\r\n")
	write(root, "kept/a.cs", "// relocated\r\n")
	write(root, "kept/package_init.cs", generated+"// initᴛother();\r\n")

	var emissions []*platformEmission

	for _, target := range targets {
		stage := t.TempDir()
		write(stage, "allLazy/a.cs", "// lazy\r\n")
		write(stage, "kept/a.cs", "// still relocated\r\n")
		write(stage, "kept/package_init.cs", generated+"// initᴛother();\r\n")

		emissions = append(emissions, &platformEmission{target: target, goos: goosOfTarget(target), root: stage, artifacts: map[string]artifactState{
			"allLazy/a.cs":         {hash: "lazy", emitted: true},
			"kept/a.cs":            {hash: "kept", emitted: true},
			"kept/package_init.cs": {hash: "init", emitted: true},
		}})
	}

	if err := mergePlatformEmissions(root, targets, emissions); err != nil {
		t.Fatalf("merge: %v", err)
	}

	if content, ok := read("allLazy/package_init.cs"); ok {
		t.Errorf("allLazy/package_init.cs must be retired; the corpus still holds:\n%s", content)
	}

	if content, _ := read("allLazy/a.cs"); content != "// lazy\r\n" {
		t.Errorf("control: allLazy/a.cs must carry the merged source, got %q", content)
	}

	if _, ok := read("kept/package_init.cs"); !ok {
		t.Error("kept/package_init.cs, emitted on both targets, must stay")
	}
}
