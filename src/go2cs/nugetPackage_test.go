// nugetPackage_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"archive/zip"
	"bytes"
	"crypto/sha512"
	"encoding/base64"
	"os"
	"path/filepath"
	"reflect"
	"strings"
	"testing"

	"go2cs/internal/sourcemeta"
)

// buildNupkg returns a zip holding the given entries -- enough of a nupkg for the converter, which reads
// only go2cs/source-metadata.txt from it.
func buildNupkg(t *testing.T, entries map[string]string) []byte {
	t.Helper()
	var buffer bytes.Buffer
	writer := zip.NewWriter(&buffer)

	for name, body := range entries {
		file, err := writer.Create(name)

		if err != nil {
			t.Fatal(err)
		}

		if _, err := file.Write([]byte(body)); err != nil {
			t.Fatal(err)
		}
	}

	if err := writer.Close(); err != nil {
		t.Fatal(err)
	}

	return buffer.Bytes()
}

// selfDescription renders a v1 self-description for a one-package module.
func selfDescription(t *testing.T, module, version, release string, requires []sourcemeta.Require, lines ...string) string {
	t.Helper()
	data, err := sourcemeta.Generate(sourcemeta.Description{
		Module: module, ModuleVersion: version, Go2csRelease: release, Requires: requires,
		Packages: []sourcemeta.Package{{ImportPath: module, Assembly: strings.ReplaceAll(module, "/", ".")}},
		Sections: map[string][]string{strings.ReplaceAll(module, "/", "."): lines},
	})

	if err != nil {
		t.Fatal(err)
	}

	return string(data)
}

// publish serves a package's versions on the fixture's flat container.
func (f *nugetMapFixture) publish(id string, versions map[string][]byte) {
	lowerID := strings.ToLower(id)
	var list []string

	for version, nupkg := range versions {
		lowerVersion := strings.ToLower(version)
		list = append(list, `"`+lowerVersion+`"`)
		f.set("/flat/"+lowerID+"/"+lowerVersion+"/"+lowerID+"."+lowerVersion+".nupkg", string(nupkg))
	}

	f.set("/flat/"+lowerID+"/index.json", `{"versions":[`+strings.Join(list, ",")+`]}`)
}

func nupkgHash(data []byte) string {
	sum := sha512.Sum512(data)
	return "sha512-" + base64.StdEncoding.EncodeToString(sum[:])
}

var modShape = thirdPartyModule{path: "example.test/shape", version: "v1.0.0"}

// B3: a release module version vX.Y.Z is published as X.Y.Z and rebuilt as X.Y.Z.N; a prerelease or
// pseudo-version L as L and L.0.N; +incompatible is dropped. Candidates come highest revision first, and
// nothing else -- a rehearsal (-local.1), another patch, or a prerelease of the same core -- is one.
func TestNuGetPackageCandidatesFollowB3(t *testing.T) {
	cases := []struct {
		version   string
		published []string
		want      []string
	}{
		{"v1.6.0", []string{"1.6.0", "1.6.0.2", "1.6.1", "1.6.0-local.1", "1.6.0.1", "1.6.0.x", "1.6.0.1.1"}, []string{"1.6.0.2", "1.6.0.1", "1.6.0"}},
		{"v2.0.0+incompatible", []string{"2.0.0.1", "2.0.0"}, []string{"2.0.0.1", "2.0.0"}},
		{"v1.0.0-rc.1", []string{"1.0.0-rc.1", "1.0.0-rc.1.0.1", "1.0.0-rc.1.1", "1.0.0"}, []string{"1.0.0-rc.1.0.1", "1.0.0-rc.1"}},
		{"v0.0.0-20251001235044-fca9a0999f15", []string{"0.0.0-20251001235044-fca9a0999f15.0.2", "0.0.0-20251001235044-fca9a0999f15"}, []string{"0.0.0-20251001235044-fca9a0999f15.0.2", "0.0.0-20251001235044-fca9a0999f15"}},
		{"v1.6.0", []string{"1.6.1"}, nil},
	}

	for _, c := range cases {
		got, err := nugetPackageCandidates(c.version, c.published)

		if err != nil || !reflect.DeepEqual(got, c.want) {
			t.Errorf("%s over %v = %v (err %v); want %v", c.version, c.published, got, err, c.want)
		}
	}

	if _, err := nugetPackageCandidates("v1.0.0-RC.1", []string{"1.0.0-rc.1"}); err == nil || !strings.Contains(err.Error(), "uppercase") {
		t.Errorf("an uppercase prerelease was not refused (B3): %v", err)
	}
}

// The tie-break (B3, PLAN 4.3): among the versions whose self-description matches the module exactly, the
// HIGHEST revision built for this converter's corpus release. A higher revision built for another release is
// passed over, and the reason is kept for the warning.
func TestNuGetPackageSelectsTheHighestRevisionBuiltForThisCorpus(t *testing.T) {
	f := newNuGetMapFixture(t)
	release := corpusRelease()
	rev1 := buildNupkg(t, map[string]string{sourcemeta.EntryPath: selfDescription(t, modShape.path, modShape.version, release, nil)})
	f.publish("nugetgo.example.test.shape", map[string][]byte{
		"1.0.0":   buildNupkg(t, map[string]string{sourcemeta.EntryPath: selfDescription(t, modShape.path, modShape.version, release, nil)}),
		"1.0.0.1": rev1,
		"1.0.0.2": buildNupkg(t, map[string]string{sourcemeta.EntryPath: selfDescription(t, modShape.path, modShape.version, "1.24.13.99", nil)}),
	})

	choice, notes, err := selectNuGetPackage("nugetgo.example.test.shape", modShape, nugetLockEntry{}, false, false)

	if err != nil {
		t.Fatal(err)
	}

	if choice.packageVersion != "1.0.0.1" || choice.contentHash != nupkgHash(rev1) || choice.description.Module != modShape.path {
		t.Errorf("choice %+v; want 1.0.0.1 with its SHA-512", choice)
	}

	if joined := strings.Join(notes, "\n"); !strings.Contains(joined, "1.0.0.2") || !strings.Contains(joined, "1.24.13.99") || !strings.Contains(joined, release) {
		t.Errorf("notes %q do not say why 1.0.0.2 was passed over (its release and this converter's)", notes)
	}
}

// A package without the file -- any package packed before S2 -- is never substituted; the module converts
// locally and the warning names the package (COORD ruling 2026-10-02: fall back, do not refuse).
func TestNuGetPackageWithoutSelfDescriptionIsNotSubstituted(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.publish("nugetgo.example.test.shape", map[string][]byte{"1.0.0": buildNupkg(t, map[string]string{"lib/net10.0/x.dll": "MZ"})})

	choice, notes, err := selectNuGetPackage("nugetgo.example.test.shape", modShape, nugetLockEntry{}, false, false)

	if err != nil || choice.packageVersion != "" {
		t.Fatalf("choice %+v (err %v); want none", choice, err)
	}

	if joined := strings.Join(notes, "\n"); !strings.Contains(joined, "nugetgo.example.test.shape@1.0.0") || !strings.Contains(joined, sourcemeta.EntryPath) {
		t.Errorf("notes %q do not name the package and the missing file", notes)
	}
}

// A malformed self-description says WHAT is malformed.
func TestNuGetPackageMalformedSelfDescriptionSaysWhat(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.publish("nugetgo.example.test.shape", map[string][]byte{"1.0.0": buildNupkg(t, map[string]string{sourcemeta.EntryPath: "module example.test/shape\n"})})

	_, notes, err := selectNuGetPackage("nugetgo.example.test.shape", modShape, nugetLockEntry{}, false, false)

	if joined := strings.Join(notes, "\n"); err != nil || !strings.Contains(joined, "malformed") || !strings.Contains(joined, "line 1") {
		t.Errorf("notes %q (err %v); want the parser's line-1 refusal quoted", notes, err)
	}
}

func TestNuGetPackageDescribingAnotherModuleIsPassedOver(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.publish("nugetgo.example.test.shape", map[string][]byte{
		"1.0.0": buildNupkg(t, map[string]string{sourcemeta.EntryPath: selfDescription(t, "example.test/other", "v1.0.0", corpusRelease(), nil)}),
	})

	choice, notes, err := selectNuGetPackage("nugetgo.example.test.shape", modShape, nugetLockEntry{}, false, false)

	if joined := strings.Join(notes, "\n"); err != nil || choice.packageVersion != "" || !strings.Contains(joined, "example.test/other") {
		t.Errorf("choice %+v, notes %q (err %v); want none, naming the module it describes", choice, notes, err)
	}
}

func TestNuGetPackageNotPublishedFallsBack(t *testing.T) {
	newNuGetMapFixture(t)

	choice, notes, err := selectNuGetPackage("nugetgo.example.test.absent", modShape, nugetLockEntry{}, false, false)

	if err != nil || choice.packageVersion != "" || len(notes) == 0 {
		t.Errorf("choice %+v, notes %q (err %v); want none, with a reason", choice, notes, err)
	}
}

// The lock pins the package version and its SHA-512: a later run takes that version from the cache without
// a single request (here, with the server gone), and a package whose bytes no longer match is REFUSED.
func TestNuGetPackageLockedVersionIsReusedOffline(t *testing.T) {
	f := newNuGetMapFixture(t)
	nupkg := buildNupkg(t, map[string]string{sourcemeta.EntryPath: selfDescription(t, modShape.path, modShape.version, corpusRelease(), nil)})
	f.publish("nugetgo.example.test.shape", map[string][]byte{"1.0.0": nupkg})

	first, _, err := selectNuGetPackage("nugetgo.example.test.shape", modShape, nugetLockEntry{}, false, false)

	if err != nil || first.packageVersion != "1.0.0" {
		t.Fatalf("first selection %+v (err %v)", first, err)
	}

	f.server.Close()
	locked := nugetLockEntry{module: modShape.path, version: modShape.version, nugetID: "nugetgo.example.test.shape", status: "canonical", layer: "registry", packageVersion: "1.0.0", contentHash: first.contentHash}
	again, _, err := selectNuGetPackage("nugetgo.example.test.shape", modShape, locked, true, false)

	if err != nil || again.packageVersion != "1.0.0" || again.contentHash != first.contentHash {
		t.Errorf("the locked version offline: %+v (err %v); want 1.0.0 from the cache", again, err)
	}

	locked.contentHash = "sha512-AAAA"

	if _, _, err := selectNuGetPackage("nugetgo.example.test.shape", modShape, locked, true, false); err == nil || !strings.Contains(err.Error(), "sha512-AAAA") {
		t.Errorf("a locked hash that no longer matches was not refused naming it (err %v)", err)
	}
}

// Offline with NO cache, the NuGet global packages folder still holds what a restore downloaded.
func TestNuGetPackageFromTheGlobalPackagesFolder(t *testing.T) {
	f := newNuGetMapFixture(t)
	nupkg := buildNupkg(t, map[string]string{sourcemeta.EntryPath: selfDescription(t, modShape.path, modShape.version, corpusRelease(), nil)})
	gpf := t.TempDir()
	dir := filepath.Join(gpf, "nugetgo.example.test.shape", "1.0.0")

	if err := os.MkdirAll(dir, 0o755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(filepath.Join(dir, "nugetgo.example.test.shape.1.0.0.nupkg"), nupkg, 0o644); err != nil {
		t.Fatal(err)
	}

	saved := nugetGlobalPackagesRoot
	nugetGlobalPackagesRoot = func() string { return gpf }
	t.Cleanup(func() { nugetGlobalPackagesRoot = saved })
	f.server.Close()

	locked := nugetLockEntry{module: modShape.path, version: modShape.version, nugetID: "nugetgo.example.test.shape", status: "canonical", layer: "registry", packageVersion: "1.0.0", contentHash: nupkgHash(nupkg)}
	choice, _, err := selectNuGetPackage("nugetgo.example.test.shape", modShape, locked, true, false)

	if err != nil || choice.packageVersion != "1.0.0" {
		t.Errorf("from the global packages folder: %+v (err %v)", choice, err)
	}
}

func TestNuGetPackageOverTheCapIsPassedOver(t *testing.T) {
	f := newNuGetMapFixture(t)
	saved := nugetPackageMaxBytes
	nugetPackageMaxBytes = 64
	t.Cleanup(func() { nugetPackageMaxBytes = saved })
	f.publish("nugetgo.example.test.shape", map[string][]byte{"1.0.0": buildNupkg(t, map[string]string{sourcemeta.EntryPath: selfDescription(t, modShape.path, modShape.version, corpusRelease(), nil)})})

	choice, notes, err := selectNuGetPackage("nugetgo.example.test.shape", modShape, nugetLockEntry{}, false, false)

	if joined := strings.Join(notes, "\n"); err != nil || choice.packageVersion != "" || !strings.Contains(joined, "64") {
		t.Errorf("choice %+v, notes %q (err %v); want none, naming the 64-byte cap", choice, notes, err)
	}
}
