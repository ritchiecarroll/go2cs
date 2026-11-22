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
	"os/exec"
	"path/filepath"
	"strings"
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

	previous, previousFlag := goModCache, goModOverlayFlag
	goModCache = cache
	t.Cleanup(func() { goModCache, goModOverlayFlag = previous, previousFlag })

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

	// ...and with no go line it is a go 1.16 module, as cmd/go reads it, for its DefaultGODEBUG.
	if goLine, _, err := moduleGodebug(moduleDir); err != nil || goLine != "1.16" {
		t.Fatalf("a go.mod with no go line is go 1.16 to cmd/go: want (\"1.16\", nil), got (%q, %v)", goLine, err)
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

// The dependency preflight keeps the run's GOFLAGS (and so the go.mod overlay): it used to replace
// GOFLAGS with -mod=readonly, `go mod download` then saw no module, and pkg/errors v0.9.1 was refused
// before converting anything ("go: no modules specified"). Runs the real go command.
func TestGoModDownloadPreflightKeepsTheGoModOverlay(t *testing.T) {
	_, moduleDir := moduleCacheFixture(t)
	t.Setenv("GOFLAGS", "")

	if _, err := installGoModOverlay(moduleDir, t.TempDir()); err != nil {
		t.Fatal(err)
	}

	if err := goModDownloadPreflight(moduleDir); err != nil {
		t.Fatalf("the preflight must see the module through the overlay: %v", err)
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

// PLAIN -recurse over a module-cache release with no go.mod (cgo against go.* INC 4: github.com/mattn/go-pointer
// v0.0.1 is one, and nugetgo-pack converts each flavor with -recurse=nuget from its cache directory). Only the -tests
// -recurse driver installed the go.mod overlay, so plain -recurse failed the dependency preflight: "go: no modules
// specified". Driven through the real binary against an offline fabricated cache (the escaped-path release of
// moduleCacheFixture), COORD's conditions as arms: the release converts; a near-miss copy of it outside the cache is
// refused by name; and so is a cache release whose synthesized go.mod is missing from cache/download.
func TestPlainRecurseConvertsAModuleCacheReleaseWithNoGoMod(t *testing.T) {
	cache, moduleDir := moduleCacheFixture(t)
	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	runIn := func(cwd, dir string) (string, string, error) {
		output := t.TempDir()
		cmd := exec.Command(driverBinary(t), "-recurse", "-go2cspath", treeSrc, dir, output)
		cmd.Dir = cwd
		cmd.Env = append(os.Environ(), "GOMODCACHE="+cache, "GOFLAGS=", "GOPROXY=off", "GOWORK=off", "GOTOOLCHAIN=local", "GONOSUMDB=*")
		combined, err := cmd.CombinedOutput()

		return string(combined), output, err
	}

	run := func(dir string) (string, string, error) { return runIn("", dir) }

	emitted := func(t *testing.T, output string) map[string]string {
		t.Helper()
		files := map[string]string{}

		_ = filepath.WalkDir(output, func(path string, entry os.DirEntry, err error) error {
			if err == nil && !entry.IsDir() && strings.HasSuffix(entry.Name(), ".cs") && entry.Name() != "package_info.cs" {
				data, _ := os.ReadFile(path)
				files[entry.Name()] = string(data)
			}

			return nil
		})

		return files
	}

	t.Run("the cache release converts", func(t *testing.T) {
		combined, output, err := run(moduleDir)

		if err != nil {
			t.Fatalf("plain -recurse refused the module-cache release with no go.mod: %v\n%s", err, lastLines(combined, 12))
		}

		var converted []string

		_ = filepath.WalkDir(output, func(path string, entry os.DirEntry, err error) error {
			if err == nil && !entry.IsDir() && (entry.Name() == "old.cs" || entry.Name() == "sub.cs") {
				converted = append(converted, entry.Name())
			}

			return nil
		})

		if len(converted) != 2 {
			t.Fatalf("want old.cs and sub.cs converted, got %v\n%s", converted, lastLines(combined, 12))
		}

		if _, err := os.Stat(filepath.Join(moduleDir, "go.mod")); !os.IsNotExist(err) {
			t.Fatal("a go.mod was written into the module cache")
		}
	})

	// P3's read, MUST 1: the overlay and the refusal take the ABSOLUTE directory, so a relative spelling into a real cache
	// release converts rather than being refused as "not a module-cache release".
	t.Run("a relative spelling of the cache release converts", func(t *testing.T) {
		combined, output, err := runIn(filepath.Join(cache, "example.com"), "!old@v0.9.1")

		if err != nil || len(emitted(t, output)) == 0 {
			t.Fatalf("a relative spelling of the cache release did not convert (%v):\n%s", err, lastLines(combined, 12))
		}
	})

	// P3's read, SHOULD 4: a `+incompatible` release, the most common go.mod-less shape on the proxy. Its synthesized
	// go.mod is named with the suffix in cache/download.
	t.Run("a +incompatible cache release converts", func(t *testing.T) {
		incompatible := filepath.Join(cache, "example.com", "inc@v2.0.0+incompatible")
		writeModuleFile(t, filepath.Join(incompatible, "inc.go"), "package inc\n\n// Two is two.\nfunc Two() int { return 2 }\n")
		writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "inc", "@v", "v2.0.0+incompatible.mod"), "module example.com/inc\n")
		combined, output, err := run(incompatible)

		if err != nil || emitted(t, output)["inc.cs"] == "" {
			t.Fatalf("the +incompatible release did not convert (%v):\n%s", err, lastLines(combined, 12))
		}
	})

	// P3's read, SHOULD 2: the stack-append harvest runs its own go build with GOFLAGS emptied, and so never saw the overlay;
	// a go.mod-less module then converted with no stack-append hints. The same sources with the same go.mod text, beside
	// it, are the twin: the two emissions must be byte-equal, and must carry hints at all.
	t.Run("a go.mod-less release emits what its go.mod twin emits, stack-append hints included", func(t *testing.T) {
		hint := filepath.Join(cache, "example.com", "hint@v1.0.0")
		writeModuleFile(t, filepath.Join(hint, "sappend.go"), stackAppendEmissionFixture)
		writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "hint", "@v", "v1.0.0.mod"), "module example.com/hint\n")
		twin := filepath.Join(t.TempDir(), "hint")
		writeModuleFile(t, filepath.Join(twin, "sappend.go"), stackAppendEmissionFixture)
		writeModuleFile(t, filepath.Join(twin, "go.mod"), "module example.com/hint\n")

		combined, cacheOut, err := run(hint)

		if err != nil {
			t.Fatalf("the go.mod-less release did not convert (%v):\n%s", err, lastLines(combined, 12))
		}

		twinCombined, twinOut, err := run(twin)

		if err != nil {
			t.Fatalf("its go.mod twin did not convert (%v):\n%s", err, lastLines(twinCombined, 12))
		}

		got, want := emitted(t, cacheOut)["sappend.cs"], emitted(t, twinOut)["sappend.cs"]

		if !strings.Contains(want, "new stackAppend<") {
			t.Fatalf("the twin carries no stack-append hint, so this arm measures nothing:\n%s", want)
		}

		if got != want {
			t.Fatalf("the go.mod-less release's sappend.cs differs from its go.mod twin's (stack-append hints: %d here, %d in the twin)",
				strings.Count(got, "new stackAppend<"), strings.Count(want, "new stackAppend<"))
		}
	})

	refused := func(t *testing.T, dir string) {
		t.Helper()
		combined, _, err := run(dir)

		if err == nil || !strings.Contains(combined, "-recurse needs a module") || !strings.Contains(combined, dir) {
			t.Fatalf("not refused by name (%v):\n%s", err, lastLines(combined, 12))
		}
	}

	t.Run("a near-miss copy outside the cache is refused by name", func(t *testing.T) {
		copyDir := filepath.Join(t.TempDir(), "example.com", "!old@v0.9.1")
		writeModuleFile(t, filepath.Join(copyDir, "old.go"), "package old\n")
		refused(t, copyDir)
	})

	t.Run("a cache release whose synthesized go.mod is missing is refused by name", func(t *testing.T) {
		missing := filepath.Join(cache, "example.com", "gone@v1.0.0")
		writeModuleFile(t, filepath.Join(missing, "gone.go"), "package gone\n")
		refused(t, missing)
	})
}
