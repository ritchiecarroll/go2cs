// toolchainCommand_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// withModulePin points the pins at a fake release's module zip for the test's duration.
func withModulePin(t *testing.T, sum string) {
	t.Helper()

	savedModules, savedArchives := pinnedGoModules, pinnedGoArchives
	pinnedGoModules = map[string]string{runtime.GOOS + "-" + runtime.GOARCH: sum}
	pinnedGoArchives = map[string]goArchivePin{}

	t.Cleanup(func() { pinnedGoModules, pinnedGoArchives = savedModules, savedArchives })
}

// install -from takes a file the user already has, only when it matches the pin, and downloads nothing.
func TestToolchainInstallFromALocalFile(t *testing.T) {
	module := toolchainModuleVersion(fakeRelease, runtime.GOOS+"-"+runtime.GOARCH)
	good := buildZip(t, "golang.org/toolchain@"+module+"/", fakeToolchainFiles(runtime.GOOS))
	files := fakeToolchainFiles(runtime.GOOS)
	files["VERSION"] = fakeRelease + "\nchanged\n"
	bad := buildZip(t, "golang.org/toolchain@"+module+"/", files)

	dir := t.TempDir()
	goodFile, badFile := filepath.Join(dir, "good.zip"), filepath.Join(dir, "bad.zip")
	os.WriteFile(goodFile, good, 0o644)
	os.WriteFile(badFile, bad, 0o644)

	withModulePin(t, moduleZipHash(t, good))

	server, hits := serve(t, map[string][]byte{})
	never := toolchainSource{name: "never", url: server.URL + "/x", verify: func(string) error { return nil }, unpack: func(string, string) error { return nil }}
	p := testProvisioner(t, map[string]string{}, []toolchainSource{never})

	var out bytes.Buffer

	if err := p.command("install", []string{"-from", badFile}, &out); err == nil || !strings.Contains(err.Error(), "is not the pinned") {
		t.Fatalf("a file that does not match the pin was installed or refused for the wrong reason: %v", err)
	}

	if p.checkRelease(p.cachedRoot()) == nil {
		t.Fatal("a refused file left an install behind")
	}

	if err := p.command("install", []string{"-from", goodFile}, &out); err != nil {
		t.Fatal(err)
	}

	if err := p.checkRelease(p.cachedRoot()); err != nil {
		t.Fatalf("install -from left no whole install: %v", err)
	}

	if !strings.Contains(out.String(), p.cachedRoot()) {
		t.Fatalf("install did not say where: %q", out.String())
	}

	if err := p.command("install", []string{"-from", badFile}, &out); err == nil {
		t.Fatal("a file that does not match the pin was reported fine because the release was already installed")
	}

	if hits.Load() != 0 {
		t.Fatal("install -from downloaded")
	}

	out.Reset()

	if err := p.command("env", nil, &out); err != nil {
		t.Fatal(err)
	}

	if want := "GOROOT=" + p.cachedRoot() + "\nGOVERSION=" + fakeRelease + "\n"; out.String() != want {
		t.Fatalf("env printed %q, want %q", out.String(), want)
	}
}

// prune removes other releases and interrupted unpacks, keeps the pinned one unless -all, and leaves
// anything it did not create.
func TestToolchainPruneKeepsThePinnedRelease(t *testing.T) {
	p := testProvisioner(t, map[string]string{}, nil)

	for _, dir := range []string{filepath.Base(p.cachedRoot()), "go1.20.1.linux-amd64", ".unpack-123", "someone-elses"} {
		os.MkdirAll(filepath.Join(p.cacheRoot, dir), 0o755)
	}

	var out bytes.Buffer

	if err := p.command("prune", nil, &out); err != nil {
		t.Fatal(err)
	}

	for dir, want := range map[string]bool{filepath.Base(p.cachedRoot()): true, "go1.20.1.linux-amd64": false, ".unpack-123": false, "someone-elses": true} {
		if _, err := os.Stat(filepath.Join(p.cacheRoot, dir)); (err == nil) != want {
			t.Fatalf("after prune, %s present=%v, want %v", dir, err == nil, want)
		}
	}

	if err := p.command("prune", []string{"-all"}, &out); err != nil {
		t.Fatal(err)
	}

	if _, err := os.Stat(p.cachedRoot()); err == nil {
		t.Fatal("prune -all kept the pinned release")
	}

	if _, err := os.Stat(filepath.Join(p.cacheRoot, "someone-elses")); err != nil {
		t.Fatal("prune -all removed a directory it did not create")
	}
}
