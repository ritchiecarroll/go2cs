// goModFile_test.go - Gbtc
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
	"testing"
)

// moduleCacheFixture builds a module cache holding a module release that ships NO go.mod (the
// github.com/pkg/errors v0.9.1 shape) with the go.mod the proxy synthesized for it under
// cache/download, and points goModCacheDir at it for the test.
func moduleCacheFixture(t *testing.T) (cache string, moduleDir string) {
	t.Helper()

	cache = t.TempDir()
	moduleDir = filepath.Join(cache, "example.com", "!old@v0.9.1")
	writeModuleFile(t, filepath.Join(moduleDir, "old.go"), "package old\n")
	writeModuleFile(t, filepath.Join(moduleDir, "sub", "sub.go"), "package sub\n")
	writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "!old", "@v", "v0.9.1.mod"), "module example.com/Old\n")

	previous := goModCache
	goModCache = cache
	t.Cleanup(func() { goModCache = previous })

	return cache, moduleDir
}

// A module-cache module with no go.mod is read through the go.mod `go` itself uses for it, by every
// in-process reader. At the base the driver refused it before converting anything ("reading the
// module's go directive: open .../go.mod") and the module root of its sub-package was not found.
func TestModuleCacheGoModIsTheModulesGoMod(t *testing.T) {
	cache, moduleDir := moduleCacheFixture(t)

	goLine, err := moduleGoDirective(moduleDir)

	if err != nil || goLine != "" {
		t.Fatalf("the synthesized go.mod has no go line: want (\"\", nil), got (%q, %v)", goLine, err)
	}

	if got := moduleRootDir(filepath.Join(moduleDir, "sub")); got != moduleDir {
		t.Fatalf("the sub-package's module root: want %s, got %s", moduleDir, got)
	}

	// Never anything else: a package BELOW the module root is not a root, a go.mod-less directory
	// outside the cache has no go.mod to take, and a release that ships its own go.mod keeps it.
	if path, ok := goModFile(filepath.Join(moduleDir, "sub")); ok {
		t.Fatalf("a package below the module root took %s", path)
	}

	outside := t.TempDir()

	if path, ok := goModFile(outside); ok {
		t.Fatalf("a go.mod-less directory outside the module cache took %s", path)
	}

	own := filepath.Join(cache, "example.com", "new@v1.0.0")
	writeModuleFile(t, filepath.Join(own, "go.mod"), "module example.com/new\n\ngo 1.21\n")
	writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "new", "@v", "v1.0.0.mod"), "module example.com/new\n")

	if path, ok := goModFile(own); !ok || path != filepath.Join(own, "go.mod") {
		t.Fatalf("a release with its own go.mod: want its own file, got %q (%v)", path, ok)
	}
}

// The driver hands the go commands the same answer through GOFLAGS -overlay, says so, and writes
// nothing into the cache.
func TestInstallGoModOverlayForAModuleWithoutGoMod(t *testing.T) {
	_, moduleDir := moduleCacheFixture(t)
	outRoot := t.TempDir()
	t.Setenv("GOFLAGS", "-mod=mod")

	overlay, err := installGoModOverlay(moduleDir, outRoot)

	if err != nil || overlay == "" {
		t.Fatalf("want an overlay, got %q (%v)", overlay, err)
	}

	if got, want := os.Getenv("GOFLAGS"), "-mod=mod -overlay="+overlay; got != want {
		t.Fatalf("GOFLAGS: want %q, got %q", want, got)
	}

	data, err := os.ReadFile(overlay)

	if err != nil {
		t.Fatal(err)
	}

	synthesized, _ := goModFile(moduleDir)

	if want := `{"Replace":{` + quoteJSON(filepath.Join(moduleDir, "go.mod")) + `:` + quoteJSON(synthesized) + `}}`; string(data) != want {
		t.Fatalf("overlay: want %s, got %s", want, data)
	}

	if _, err := os.Stat(filepath.Join(moduleDir, "go.mod")); !os.IsNotExist(err) {
		t.Fatal("a go.mod was written into the module cache")
	}

	// A module that ships its own go.mod gets no overlay and GOFLAGS is left as it is.
	t.Setenv("GOFLAGS", "")
	own := t.TempDir()
	writeModuleFile(t, filepath.Join(own, "go.mod"), "module example.com/own\n")

	if overlay, err := installGoModOverlay(own, outRoot); err != nil || overlay != "" || os.Getenv("GOFLAGS") != "" {
		t.Fatalf("a module with its own go.mod: want no overlay, got %q (%v), GOFLAGS %q", overlay, err, os.Getenv("GOFLAGS"))
	}

	// An existing -overlay is never replaced.
	t.Setenv("GOFLAGS", "-overlay=elsewhere.json")

	if _, err := installGoModOverlay(moduleDir, outRoot); err == nil {
		t.Fatal("an existing GOFLAGS -overlay was replaced")
	}
}

func quoteJSON(s string) string {
	out := []byte{'"'}

	for _, r := range s {
		if r == '\\' || r == '"' {
			out = append(out, '\\')
		}

		out = append(out, string(r)...)
	}

	return string(append(out, '"'))
}
