// goModFile_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"sort"
	"strings"
	"testing"
	"time"
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

// The module cache and the release can be spelled through a symlink on one side only: on macOS /var is a symlink to
// /private/var, so GOMODCACHE keeps /var/... while a relative spelling of the release, made absolute through the
// working directory, reads /private/var/.... Both name one folder, and the release is still the cache's (train 5's
// darwin go-test, TestPlainRecurseConvertsAModuleCacheReleaseWithNoGoMod's relative arm). Reproduced on every OS with
// a symlinked cache root, both ways round.
func TestModuleCacheGoModReadsThroughASymlinkedSpelling(t *testing.T) {
	cache, moduleDir := moduleCacheFixture(t)
	link := filepath.Join(t.TempDir(), "cache-link")

	if err := os.Symlink(cache, link); err != nil {
		t.Skipf("this host cannot create a symlink: %v", err)
	}

	// Compared resolved: the test's own temp directory can sit behind a symlink too (macOS's /var).
	want, err := filepath.EvalSymlinks(filepath.Join(cache, "cache", "download", "example.com", "!old", "@v", "v0.9.1.mod"))

	if err != nil {
		t.Fatal(err)
	}

	viaLink := filepath.Join(link, "example.com", "!old@v0.9.1")

	// GOMODCACHE through the link, the release by its real path (macOS's /var against /private/var).
	goModCache = link

	if got := moduleCacheGoMod(moduleDir); got == "" {
		t.Errorf("the cache spelled through a symlink: the release at its real path is not the cache's")
	} else if resolved, err := filepath.EvalSymlinks(got); err != nil || resolved != want {
		t.Errorf("the cache spelled through a symlink: want %s, got %s", want, got)
	}

	// The other way round: GOMODCACHE by its real path, the release through the link.
	goModCache = cache

	if got := moduleCacheGoMod(viaLink); got == "" {
		t.Errorf("the release spelled through a symlink: it is not the cache's")
	} else if resolved, err := filepath.EvalSymlinks(got); err != nil || resolved != want {
		t.Errorf("the release spelled through a symlink: want %s, got %s", want, got)
	}

	// Still never anything else: a package below the root, through the link, is not a root.
	if got := moduleCacheGoMod(filepath.Join(viaLink, "sub")); got != "" {
		t.Errorf("a package below the module root, through a symlink, took %s", got)
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
// -recurse driver handled one, through a go.mod overlay that go list and go build both refuse beneath GOMODCACHE, so
// plain -recurse failed its dependency preflight: "go: no modules specified". It now converts a COPY carrying the
// cache's synthesized go.mod. Driven through the real binary against offline fabricated caches, each run under a
// private TMPDIR, with COORD's conditions and P3's reads as arms.
func TestPlainRecurseConvertsAModuleCacheReleaseWithNoGoMod(t *testing.T) {
	cache, moduleDir := moduleCacheFixture(t)
	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	tmp := t.TempDir()

	runWith := func(moduleCache, cwd, dir string, extra ...string) (string, string, error) {
		output := t.TempDir()
		cmd := exec.Command(driverBinary(t), "-recurse", "-go2cspath", treeSrc, dir, output)
		cmd.Dir = cwd
		cmd.Env = append(append(os.Environ(), "GOMODCACHE="+moduleCache, "GOFLAGS=", "GOPROXY=off", "GOWORK=off", "GOTOOLCHAIN=local", "GONOSUMDB=*",
			"TMPDIR="+tmp, "TMP="+tmp, "TEMP="+tmp), extra...)
		combined, err := cmd.CombinedOutput()

		return string(combined), output, err
	}

	run := func(dir string) (string, string, error) { return runWith(cache, "", dir) }

	// Every emitted .cs by its path in the output, package_info.cs included: it holds the package's Go file identities.
	emitted := func(t *testing.T, output string) map[string]string {
		t.Helper()
		files := map[string]string{}

		_ = filepath.WalkDir(output, func(path string, entry os.DirEntry, err error) error {
			if err == nil && !entry.IsDir() && strings.HasSuffix(entry.Name(), ".cs") {
				data, _ := os.ReadFile(path)
				rel, _ := filepath.Rel(output, path)
				files[filepath.ToSlash(rel)] = string(data)
			}

			return nil
		})

		return files
	}

	copiesLeft := func() []string {
		matches, _ := filepath.Glob(filepath.Join(tmp, "go2cs-modcopy-*"))
		return matches
	}

	// A twin: the same release with its own go.mod, IN a cache of its own at the same path and version, so it is read
	// exactly as go reads a release that ships a go.mod.
	twinCache := func(t *testing.T, escaped string, files map[string]string) (string, string) {
		t.Helper()
		other := t.TempDir()
		dir := filepath.Join(other, filepath.FromSlash(escaped))

		for name, contents := range files {
			writeModuleFile(t, filepath.Join(dir, name), contents)
		}

		return other, dir
	}

	// Source with a body, so its package records a position map that names it.
	const oldSource = "package old\n\n// One is one.\nfunc One() int { return 1 }\n"
	const oldRoot = "src/example.com/Old/"
	writeModuleFile(t, filepath.Join(moduleDir, "old.go"), oldSource)

	t.Run("the cache release converts, recording the release's own Go file identities", func(t *testing.T) {
		combined, output, err := run(moduleDir)

		if err != nil {
			t.Fatalf("plain -recurse refused the module-cache release with no go.mod: %v\n%s", err, lastLines(combined, 12))
		}

		got := emitted(t, output)

		if got[oldRoot+"old.cs"] == "" || got[oldRoot+"sub/sub.cs"] == "" {
			t.Fatalf("want old.cs and sub.cs converted, got %d files\n%s", len(got), lastLines(combined, 12))
		}

		// P3's read, MUST 1: the copy's sources are the release's, so their identities carry its version.
		if info := got[oldRoot+"package_info.cs"]; !strings.Contains(info, `"example.com/Old@v0.9.1/old.go"`) || strings.Contains(info, "@/old.go") {
			t.Errorf("package_info.cs does not record old.go as the release's @v0.9.1 source:\n%s", lastLines(info, 30))
		}

		twinModCache, twinDir := twinCache(t, "example.com/!old@v0.9.1", map[string]string{"old.go": oldSource, "sub/sub.go": "package sub\n", "go.mod": "module example.com/Old\n"})
		twinCombined, twinOut, err := runWith(twinModCache, "", twinDir)

		if err != nil {
			t.Fatalf("the go.mod twin in its cache did not convert (%v):\n%s", err, lastLines(twinCombined, 12))
		}

		for name, want := range emitted(t, twinOut) {
			if got[name] != want {
				t.Errorf("%s differs from the go.mod twin's, read from its cache at the same path and version", name)
			}
		}

		if _, err := os.Stat(filepath.Join(moduleDir, "go.mod")); !os.IsNotExist(err) {
			t.Fatal("a go.mod was written into the module cache")
		}

		if left := copiesLeft(); len(left) != 0 {
			t.Errorf("the converting run left its copy: %v", left)
		}
	})

	// P3's read, MUST 1 of the plan read: the absolute directory, so a relative spelling into a real cache release converts.
	t.Run("a relative spelling of the cache release converts", func(t *testing.T) {
		combined, output, err := runWith(cache, filepath.Join(cache, "example.com"), "!old@v0.9.1")

		if err != nil || len(emitted(t, output)) == 0 {
			t.Fatalf("a relative spelling of the cache release did not convert (%v):\n%s", err, lastLines(combined, 12))
		}
	})

	// A `+incompatible` release, the most common go.mod-less shape on the proxy: its synthesized go.mod is named with the
	// suffix in cache/download. (nugetgo-pack packs it as NuGet 2.0.0: Get-NugetgoVersion drops +incompatible.)
	t.Run("a +incompatible cache release converts", func(t *testing.T) {
		incompatible := filepath.Join(cache, "example.com", "inc@v2.0.0+incompatible")
		writeModuleFile(t, filepath.Join(incompatible, "inc.go"), "package inc\n\n// Two is two.\nfunc Two() int { return 2 }\n")
		writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "inc", "@v", "v2.0.0+incompatible.mod"), "module example.com/inc\n")
		combined, output, err := run(incompatible)

		if err != nil || emitted(t, output)["src/example.com/inc/inc.cs"] == "" {
			t.Fatalf("the +incompatible release did not convert (%v):\n%s", err, lastLines(combined, 12))
		}
	})

	// The stack-append harvest and the identities, against a go.mod twin IN a cache: the two emissions must be
	// byte-equal, package_info.cs included, and must carry stack-append hints at all.
	t.Run("a go.mod-less release emits what its go.mod twin in the cache emits", func(t *testing.T) {
		hint := filepath.Join(cache, "example.com", "hint@v1.0.0")
		writeModuleFile(t, filepath.Join(hint, "sappend.go"), stackAppendEmissionFixture)
		writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "hint", "@v", "v1.0.0.mod"), "module example.com/hint\n")
		twinModCache, twinDir := twinCache(t, "example.com/hint@v1.0.0", map[string]string{"sappend.go": stackAppendEmissionFixture, "go.mod": "module example.com/hint\n"})

		combined, cacheOut, err := run(hint)

		if err != nil {
			t.Fatalf("the go.mod-less release did not convert (%v):\n%s", err, lastLines(combined, 12))
		}

		twinCombined, twinOut, err := runWith(twinModCache, "", twinDir)

		if err != nil {
			t.Fatalf("its go.mod twin did not convert (%v):\n%s", err, lastLines(twinCombined, 12))
		}

		got, want := emitted(t, cacheOut), emitted(t, twinOut)
		const root = "src/example.com/hint/"

		if !strings.Contains(want[root+"sappend.cs"], "new stackAppend<") {
			t.Fatalf("the twin carries no stack-append hint, so this arm measures nothing:\n%s", want[root+"sappend.cs"])
		}

		for _, name := range []string{root + "sappend.cs", root + "package_info.cs"} {
			if got[name] != want[name] {
				t.Errorf("%s differs from the go.mod twin's (stack-append hints: %d here, %d in the twin)", name,
					strings.Count(got[name], "new stackAppend<"), strings.Count(want[name], "new stackAppend<"))
			}
		}

		if !strings.Contains(got[root+"package_info.cs"], `"example.com/hint@v1.0.0/sappend.go"`) {
			t.Errorf("package_info.cs does not record sappend.go with the release's version")
		}
	})

	// A diagnostic or load warning names the release's own path, never the copy's. Only the "go.mod:" line, which says
	// what was copied where, names the copy.
	t.Run("diagnostics name the cache path, not the copy's", func(t *testing.T) {
		needy := filepath.Join(cache, "example.com", "needy@v1.0.0")
		writeModuleFile(t, filepath.Join(needy, "needy.go"), "package needy\n\nimport \"example.com/other\"\n\n// V is other's value.\nvar V = other.V\n")
		writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "needy", "@v", "v1.0.0.mod"), "module example.com/needy\n")
		combined, _, _ := run(needy)

		if !strings.Contains(combined, filepath.Join(needy, "needy.go")) {
			t.Fatalf("no diagnostic names the release's own file:\n%s", lastLines(combined, 12))
		}

		for _, line := range strings.Split(combined, "\n") {
			if strings.Contains(line, "go2cs-modcopy-") && !strings.HasPrefix(line, "go.mod: ") {
				t.Errorf("a line names the copy: %s", line)
			}
		}
	})

	// P3's read, SHOULD 3: the copy is a main module to go, where a user's GOWORK would apply. It converts with GOWORK
	// naming a workspace that does not list it.
	t.Run("a GOWORK naming a workspace does not reach the copy", func(t *testing.T) {
		workspace := t.TempDir()
		writeModuleFile(t, filepath.Join(workspace, "member", "go.mod"), "module example.com/member\n\ngo 1.21\n")
		writeModuleFile(t, filepath.Join(workspace, "go.work"), "go 1.21\n\nuse ./member\n")
		combined, output, err := runWith(cache, "", moduleDir, "GOWORK="+filepath.Join(workspace, "go.work"))

		if err != nil || emitted(t, output)[oldRoot+"old.cs"] == "" {
			t.Fatalf("a GOWORK workspace reached the copy (%v):\n%s", err, lastLines(combined, 12))
		}
	})

	// P3's read, SHOULD 4: no copy is left after a REFUSED run either (a release whose go line this go2cs cannot convert
	// with), and a copy an earlier run left by os.Exit or a signal is swept at start once it is older than a day; a young
	// one, which a live run may hold, is kept.
	t.Run("no copy is left after a refused run, and a stale one is swept", func(t *testing.T) {
		future := filepath.Join(cache, "example.com", "future@v1.0.0")
		writeModuleFile(t, filepath.Join(future, "future.go"), "package future\n")
		writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "future", "@v", "v1.0.0.mod"), "module example.com/future\n\ngo 1.99\n")

		stale, young := filepath.Join(tmp, "go2cs-modcopy-stale"), filepath.Join(tmp, "go2cs-modcopy-young")
		writeModuleFile(t, filepath.Join(stale, "x", "a.go"), "package a\n")
		writeModuleFile(t, filepath.Join(young, "x", "a.go"), "package a\n")
		old := time.Now().Add(-25 * time.Hour)

		if err := os.Chtimes(stale, old, old); err != nil {
			t.Fatal(err)
		}

		combined, _, err := run(future)

		if err == nil {
			t.Fatalf("a release asking for go 1.99 converted:\n%s", lastLines(combined, 12))
		}

		left := copiesLeft()

		if len(left) != 1 || left[0] != young {
			t.Errorf("after the refused run, want only the young planted copy left, got %v", left)
		}
	})

	// P3's delta read: licensing.go and embedDirective.go read the module directory, which is now the copy. A release with
	// a //go:embed file and a LICENSE, converted the way nugetgo-pack converts each flavor (-recurse=nuget -provenance),
	// emits every file -- not only the .cs -- equal to its go.mod twin's, read from its own cache at the same version.
	t.Run("a release with an embedded file and a LICENSE emits what its go.mod twin emits, every file", func(t *testing.T) {
		const embedSource = "package emb\n\nimport _ \"embed\"\n\n// Data is the embedded file.\n//\n//go:embed data.txt\nvar Data string\n"
		const license = "MIT License\n\nCopyright (c) 2026 Example Authors\n\nPermission is hereby granted, free of charge, to any person obtaining a copy.\n"
		files := map[string]string{"emb.go": embedSource, "data.txt": "embedded bytes\n", "LICENSE": license}

		release := filepath.Join(cache, "example.com", "emb@v1.0.0")

		for name, contents := range files {
			writeModuleFile(t, filepath.Join(release, name), contents)
		}

		writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "emb", "@v", "v1.0.0.mod"), "module example.com/emb\n")

		twinFiles := map[string]string{"go.mod": "module example.com/emb\n"}

		for name, contents := range files {
			twinFiles[name] = contents
		}

		twinModCache, twinDir := twinCache(t, "example.com/emb@v1.0.0", twinFiles)

		every := func(t *testing.T, moduleCache, dir string) map[string]string {
			t.Helper()
			output := t.TempDir()
			cmd := exec.Command(driverBinary(t), "-recurse=nuget", "-provenance", "-go2cspath", treeSrc, dir, output)
			cmd.Env = append(os.Environ(), "GOMODCACHE="+moduleCache, "GOFLAGS=", "GOPROXY=off", "GOWORK=off", "GOTOOLCHAIN=local", "GONOSUMDB=*",
				"TMPDIR="+tmp, "TMP="+tmp, "TEMP="+tmp)
			combined, err := cmd.CombinedOutput()

			if err != nil {
				t.Fatalf("%s did not convert (%v):\n%s", dir, err, lastLines(string(combined), 12))
			}

			all := map[string]string{}

			_ = filepath.WalkDir(output, func(path string, entry os.DirEntry, err error) error {
				if err == nil && !entry.IsDir() {
					data, _ := os.ReadFile(path)
					rel, _ := filepath.Rel(output, path)
					all[filepath.ToSlash(rel)] = string(data)
				}

				return nil
			})

			return all
		}

		got, want := every(t, cache, release), every(t, twinModCache, twinDir)

		if len(want) == 0 || !strings.Contains(strings.Join(mapKeys(want), "\n"), "emb.cs") {
			t.Fatalf("the twin emitted no emb.cs, so this arm measures nothing: %v", mapKeys(want))
		}

		if !strings.Contains(want["src/example.com/emb/emb.cs"], "in module example.com/emb@v1.0.0") {
			t.Errorf("the twin's emb.cs carries no versioned provenance, so the provenance half measures nothing")
		}

		for name, contents := range want {
			if got[name] != contents {
				t.Errorf("%s differs from the go.mod twin's:\n--- go.mod-less release ---\n%s\n--- twin ---\n%s", name, firstLines(got[name], 8), firstLines(contents, 8))
			}
		}

		for name := range got {
			if _, ok := want[name]; !ok {
				t.Errorf("%s is emitted only for the go.mod-less release", name)
			}
		}
	})

	// COORD's INC 4 review, NIT: the copy's work directory is its real path, so a TMPDIR reached through a symlink (macOS's
	// /var is /private/var) maps back too. The go.mod line names the copy by its resolved path.
	t.Run("a TMPDIR reached through a symlink converts and names the resolved copy", func(t *testing.T) {
		real := t.TempDir()
		link := filepath.Join(t.TempDir(), "link")

		if err := os.Symlink(real, link); err != nil {
			t.Skipf("no symlink here: %v", err)
		}

		combined, output, err := runWith(cache, "", moduleDir, "TMPDIR="+link, "TMP="+link, "TEMP="+link)

		if err != nil || emitted(t, output)[oldRoot+"old.cs"] == "" {
			t.Fatalf("the conversion under a symlinked TMPDIR failed (%v):\n%s", err, lastLines(combined, 12))
		}

		var copyLine string

		for _, line := range strings.Split(combined, "\n") {
			if strings.HasPrefix(line, "go.mod: ") {
				copyLine = line
			}
		}

		if !strings.Contains(copyLine, filepath.Join(real, "go2cs-modcopy-")) {
			t.Errorf("the copy is not named by its resolved path under %s: %s", real, copyLine)
		}
	})

	refused := func(t *testing.T, dir string) {
		t.Helper()
		combined, _, err := run(dir)

		if err == nil || !strings.Contains(combined, "-recurse needs a module") || !strings.Contains(combined, dir) {
			t.Fatalf("not refused by name (%v):\n%s", err, lastLines(combined, 12))
		}
	}

	// COORD's INC 4 review, fold 1: the refusal covers only a directory NO go.mod covers. A package directory inside a
	// module, with the go.mod above it, converts under -recurse as it did before INC 4 (master found the parent go.mod).
	t.Run("a package directory inside a module still converts", func(t *testing.T) {
		module := t.TempDir()
		writeModuleFile(t, filepath.Join(module, "go.mod"), "module example.com/inside\n\ngo 1.24\n")
		writeModuleFile(t, filepath.Join(module, "sub", "sub.go"), "package sub\n\n// One is one.\nfunc One() int { return 1 }\n")
		combined, output, err := run(filepath.Join(module, "sub"))

		if err != nil || strings.Contains(combined, "-recurse needs a module") {
			t.Fatalf("a package directory inside a module was refused (%v):\n%s", err, lastLines(combined, 12))
		}

		if !strings.Contains(strings.Join(mapKeys(emitted(t, output)), "\n"), "sub.cs") {
			t.Fatalf("the package inside the module emitted no sub.cs:\n%s", lastLines(combined, 12))
		}
	})

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

// mapKeys is m's keys, sorted.
func mapKeys(m map[string]string) []string {
	keys := make([]string, 0, len(m))

	for key := range m {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	return keys
}

// firstLines is text's first n lines.
func firstLines(text string, n int) string {
	lines := strings.Split(text, "\n")

	if len(lines) > n {
		lines = lines[:n]
	}

	return strings.Join(lines, "\n")
}

// P3's delta read of INC 4, SHOULD 1 and NIT 2: in a copy run, a source under a go2cs-modcopy-* directory that is not
// this run's copy (the loader reporting the copy under another spelling: a symlinked TMPDIR, a Windows 8.3 name) is an
// error naming both spellings, never the main-module `<module>@/<file>` form; and a message quoting the copy
// forward-slashed is remapped too. No route to the first is known (a symlinked TMPDIR on linux keeps its spelling).
func TestACopyRunMapsEverySourceOrRefusesByName(t *testing.T) {
	work := t.TempDir()
	copyRoot := filepath.Join(work, "go2cs-modcopy-111", "example.com", "!old@v0.9.1")
	release := filepath.Join(work, "cache", "example.com", "!old@v0.9.1")

	previousCopy, previousOriginal := copiedModuleRoot, originalModuleRoot
	t.Cleanup(func() { copiedModuleRoot, originalModuleRoot = previousCopy, previousOriginal })

	copiedModuleRoot, originalModuleRoot = "", ""

	if got, err := originalSourcePath(filepath.Join(work, "go2cs-modcopy-222", "x.go")); err != nil || got != filepath.Join(work, "go2cs-modcopy-222", "x.go") {
		t.Errorf("outside a copy run every path passes through: got (%s, %v)", got, err)
	}

	copiedModuleRoot, originalModuleRoot = copyRoot, release

	if got, err := originalSourcePath(filepath.Join(copyRoot, "sub", "sub.go")); err != nil || got != filepath.Join(release, "sub", "sub.go") {
		t.Errorf("a source under the copy maps to the release: got (%s, %v)", got, err)
	}

	other := filepath.Join(work, "elsewhere", "go2cs-modcopy-111", "example.com", "!old@v0.9.1", "old.go")

	if got, err := originalSourcePath(other); err == nil || !strings.Contains(err.Error(), other) || !strings.Contains(err.Error(), copyRoot) {
		t.Errorf("a source under another spelling of a copy is refused naming both: got (%s, %v)", got, err)
	}

	plain := filepath.Join(work, "module", "a.go")

	if got, err := originalSourcePath(plain); err != nil || got != plain {
		t.Errorf("a source outside every copy passes through: got (%s, %v)", got, err)
	}

	message := "WARNING: " + filepath.Join(copyRoot, "old.go") + " and " + filepath.ToSlash(filepath.Join(copyRoot, "old.go"))

	if got := remapCopiedPaths(message); strings.Contains(got, "go2cs-modcopy-") ||
		!strings.Contains(got, filepath.ToSlash(filepath.Join(release, "old.go"))) {
		t.Errorf("both spellings of the copy are remapped: %s", got)
	}

	// COORD's INC 4 review, NIT: an error ConvertModule returns after the copy names the release, and keeps its chain.
	sentinel := errors.New("sentinel")
	wrapped := fmt.Errorf("preflight failed in %s: %w", copyRoot, sentinel)

	if got := remapCopiedError(wrapped); strings.Contains(got.Error(), "go2cs-modcopy-") || !strings.Contains(got.Error(), release) || !errors.Is(got, sentinel) {
		t.Errorf("a returned error is remapped with its chain kept: %v (errors.Is %t)", got, errors.Is(got, sentinel))
	}

	if remapCopiedError(nil) != nil || remapCopiedError(sentinel) != sentinel {
		t.Error("nil stays nil, and an error naming no copy is returned as it is")
	}
}
