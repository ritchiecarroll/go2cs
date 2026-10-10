// nugetMapRun_test.go - Gbtc
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

// OWNER ARM 1 (owner UX ruling 2026-09-30): nugetgo.net stays the FALLBACK behind a user source. The
// user's file answers the module it names and the registry answers the rest. The registry's request
// count is the POSITIVE control for owner arm 2: it reads 1 here, so its 0 there is a measurement.
func TestNuGetMapUserSourceAnswersItsModulesAndTheRegistryTheRest(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/registry", mapRow(modA.path, "reg.a", "canonical")+mapRow(modB.path, "reg.b", "canonical"))
	mine := writeMapFile(t, "mine.txt", mapRow(modA.path, "mine.a", "community"))

	decisions, _, err := runNuGetMapResolution([]thirdPartyModule{modA, modB}, nugetMapOptions{sources: []string{mine}}, t.TempDir())

	if err != nil {
		t.Fatal(err)
	}

	if a := decisionFor(t, decisions, modA.path); !a.mapped || a.nugetID != "mine.a" || a.layer != mine {
		t.Errorf("A: %+v; want mine.a from the user's file", a)
	}

	if b := decisionFor(t, decisions, modB.path); !b.mapped || b.nugetID != "reg.b" || b.layer != nugetMapRegistryURL {
		t.Errorf("B: %+v; want reg.b from the registry", b)
	}

	if n := f.count("/registry"); n != 1 {
		t.Errorf("the registry was requested %d times; want 1 (the positive control)", n)
	}
}

// OWNER ARM 2: -nuget-map-only drops the registry. An unlisted module stays local AND the registry is
// never requested.
func TestNuGetMapOnlyNeverFetchesTheRegistry(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/registry", mapRow(modA.path, "reg.a", "canonical")+mapRow(modB.path, "reg.b", "canonical"))
	mine := writeMapFile(t, "mine.txt", mapRow(modA.path, "mine.a", "community"))

	decisions, _, err := runNuGetMapResolution([]thirdPartyModule{modA, modB}, nugetMapOptions{sources: []string{mine}, only: true}, t.TempDir())

	if err != nil {
		t.Fatal(err)
	}

	if a := decisionFor(t, decisions, modA.path); !a.mapped || a.nugetID != "mine.a" {
		t.Errorf("A: %+v; want mine.a", a)
	}

	if b := decisionFor(t, decisions, modB.path); b.mapped {
		t.Errorf("B: %+v; an unlisted module was mapped under -nuget-map-only", b)
	}

	if n := f.count("/registry"); n != 0 {
		t.Errorf("the registry was requested %d times under -nuget-map-only; want 0", n)
	}
}

// The DEFAULT, pinned at the registry (S3b's flip of S3a's dormant pin): no -nuget-map flag consults the
// registry once, maps what it answers, and writes the lock.
func TestNuGetMapDefaultConsultsTheRegistry(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/registry", mapRow(modA.path, "reg.a", "canonical"))
	outRoot := t.TempDir()

	decisions, _, err := runNuGetMapResolution([]thirdPartyModule{modA}, nugetMapOptions{}, outRoot)

	if err != nil {
		t.Fatal(err)
	}

	if a := decisionFor(t, decisions, modA.path); !a.mapped || a.nugetID != "reg.a" || a.layer != nugetMapRegistryURL {
		t.Errorf("A: %+v; want reg.a from the registry by default", a)
	}

	if n := f.count("/registry"); n != 1 {
		t.Errorf("the registry was requested %d times with no -nuget-map flag; want 1", n)
	}

	if _, err := os.Stat(nugetLockPath(outRoot)); err != nil {
		t.Errorf("the default wrote no %s: %v", nugetLockFileName, err)
	}
}

func TestNuGetMapOffFetchesNothing(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/registry", mapRow(modA.path, "reg.a", "canonical"))
	outRoot := t.TempDir()

	decisions, _, err := runNuGetMapResolution([]thirdPartyModule{modA}, nugetMapOptions{off: true}, outRoot)

	if err != nil || len(decisions) != 0 || f.count("/registry") != 0 {
		t.Errorf("-nuget-map off resolved: decisions %+v, registry requests %d, err %v", decisions, f.count("/registry"), err)
	}

	if _, err := os.Stat(nugetLockPath(outRoot)); !os.IsNotExist(err) {
		t.Errorf("-nuget-map off wrote %s", nugetLockFileName)
	}
}

// The lock records every MAPPED module in seven TAB-separated fields from day one, '-' in the two S3b
// fills (package version, content hash); an unmapped module is not locked.
func TestNuGetMapWritesTheSevenFieldLock(t *testing.T) {
	newNuGetMapFixture(t)
	mine := writeMapFile(t, "mine.txt", mapRow(modA.path, "mine.a", "community"))
	outRoot := t.TempDir()

	if _, _, err := runNuGetMapResolution([]thirdPartyModule{modA, modB}, nugetMapOptions{sources: []string{mine}, only: true}, outRoot); err != nil {
		t.Fatal(err)
	}

	data, err := os.ReadFile(nugetLockPath(outRoot))

	if err != nil {
		t.Fatal(err)
	}

	var rows []string

	for _, line := range strings.Split(string(data), "\n") {
		if line != "" && !strings.HasPrefix(line, "#") {
			rows = append(rows, line)
		}
	}

	// The layer is the user's file relative to the output root, never its absolute path (gap 6 of the nugetgo rehearsal).
	layer, err := filepath.Rel(outRoot, mine)

	if err != nil {
		t.Fatal(err)
	}

	want := strings.Join([]string{modA.path, "v1.0.0", "mine.a", "community", filepath.ToSlash(layer), "-", "-"}, "\t")

	if len(rows) != 1 || rows[0] != want {
		t.Errorf("lock rows %q; want exactly %q", rows, want)
	}

	if strings.Contains(string(data), "\r") {
		t.Errorf("the lock carries CR line endings")
	}
}

// A second run keeps what the lock pinned and reports the registry's change; -nuget-map-refresh adopts
// it and rewrites the lock.
func TestNuGetMapSecondRunKeepsTheLockUntilRefresh(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/registry", mapRow(modA.path, "reg.a", "canonical"))
	mine := writeMapFile(t, "mine.txt", "# nothing of my own\n")
	outRoot := t.TempDir()
	opts := nugetMapOptions{sources: []string{mine}}

	if _, _, err := runNuGetMapResolution([]thirdPartyModule{modA}, opts, outRoot); err != nil {
		t.Fatal(err)
	}

	f.set("/registry", mapRow(modA.path, "reg2.a", "canonical"))
	decisions, warnings, err := runNuGetMapResolution([]thirdPartyModule{modA}, opts, outRoot)

	if err != nil {
		t.Fatal(err)
	}

	if a := decisionFor(t, decisions, modA.path); a.nugetID != "reg.a" || !a.fromLock || len(warnings) != 1 {
		t.Errorf("second run A: %+v, warnings %q; want the locked reg.a and one disagreement warning", a, warnings)
	}

	opts.refresh = true
	decisions, _, err = runNuGetMapResolution([]thirdPartyModule{modA}, opts, outRoot)

	if err != nil {
		t.Fatal(err)
	}

	if a := decisionFor(t, decisions, modA.path); a.nugetID != "reg2.a" {
		t.Errorf("refresh A: %+v; want reg2.a adopted", a)
	}

	lock, err := readNuGetLock(outRoot)

	if err != nil || lock[modA.path].nugetID != "reg2.a" {
		t.Errorf("after refresh the lock holds %+v (err %v); want reg2.a", lock[modA.path], err)
	}
}

// A locked module that is not in this run's closure is KEPT: one output root can hold several modules'
// conversions, and the lock is that root's record (the go2cs.modules.lock rule).
func TestNuGetMapKeepsLockEntriesOutsideThisRun(t *testing.T) {
	newNuGetMapFixture(t)
	outRoot := t.TempDir()
	other := nugetLockEntry{module: "github.com/o/o", version: "v1.0.0", nugetID: "o", status: "canonical", layer: "registry", packageVersion: "-", contentHash: "-"}

	if err := writeNuGetLock(outRoot, map[string]nugetLockEntry{other.module: other}); err != nil {
		t.Fatal(err)
	}

	mine := writeMapFile(t, "mine.txt", mapRow(modA.path, "mine.a", "community"))

	if _, _, err := runNuGetMapResolution([]thirdPartyModule{modA}, nugetMapOptions{sources: []string{mine}, only: true}, outRoot); err != nil {
		t.Fatal(err)
	}

	lock, err := readNuGetLock(outRoot)

	if err != nil || lock[other.module] != other || lock[modA.path].nugetID != "mine.a" {
		t.Errorf("lock %+v (err %v); want the other module kept beside the new entry", lock, err)
	}
}

// THE LOCK NAMES NO HOST PATH (the nugetgo rehearsal, 2026-10-09, gap 6). A conversion repository commits its
// go2cs.nuget.lock, so the layer that answered a mapping is recorded by URL, or relative to the output root, never as
// the absolute path the user typed; a lock written before the rule is rewritten the same way at its next write, and a
// layer read back from the lock (already relative to the root) is kept as it is.
func TestNuGetLockRecordsTheMappingSourceRelativeOrByURL(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/registry", mapRow(modB.path, "reg.b", "canonical"))
	work := t.TempDir()
	outRoot := filepath.Join(work, "out")
	mapsDir := filepath.Join(work, "maps")

	if err := os.MkdirAll(mapsDir, 0o755); err != nil {
		t.Fatal(err)
	}

	mine := filepath.Join(mapsDir, "mine.txt")

	if err := os.WriteFile(mine, []byte(mapRow(modA.path, "mine.a", "community")), 0o644); err != nil {
		t.Fatal(err)
	}

	// A legacy entry for a module outside this run, pinned with the absolute path an older converter wrote.
	legacy := nugetLockEntry{module: "github.com/o/o", version: "v1.0.0", nugetID: "o", status: "canonical", layer: mine, packageVersion: "-", contentHash: "-"}

	if err := writeNuGetLock(outRoot, map[string]nugetLockEntry{legacy.module: legacy}); err != nil {
		t.Fatal(err)
	}

	if _, _, err := runNuGetMapResolution([]thirdPartyModule{modA, modB}, nugetMapOptions{sources: []string{mine}}, outRoot); err != nil {
		t.Fatal(err)
	}

	lock, err := readNuGetLock(outRoot)

	if err != nil {
		t.Fatal(err)
	}

	want := map[string]string{modA.path: "../maps/mine.txt", modB.path: nugetMapRegistryURL, legacy.module: "../maps/mine.txt"}

	for module, layer := range want {
		if got := lock[module].layer; got != layer {
			t.Errorf("%s: the lock records layer %q, want %q", module, got, layer)
		}
	}

	data, _ := os.ReadFile(nugetLockPath(outRoot))

	if strings.Contains(string(data), filepath.ToSlash(work)) || strings.Contains(string(data), work) {
		t.Errorf("the lock names the host path %s:\n%s", work, data)
	}

	// A second run reads the relative layers back and keeps them, unchanged.
	if _, _, err := runNuGetMapResolution([]thirdPartyModule{modA, modB}, nugetMapOptions{sources: []string{mine}}, outRoot); err != nil {
		t.Fatal(err)
	}

	again, _ := os.ReadFile(nugetLockPath(outRoot))

	if string(again) != string(data) {
		t.Errorf("a second run rewrote the lock:\n%s\nwant\n%s", again, data)
	}
}
