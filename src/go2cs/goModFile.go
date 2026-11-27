// goModFile.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"log"
	"os"
	"path/filepath"
	"strings"
	"time"
)

// goModFile returns the go.mod that describes the module rooted at dir, and whether there is one.
//
// That is dir/go.mod when it exists. A module release that predates modules ships NO go.mod, and the
// module cache holds it as <GOMODCACHE>/<escaped path>@<version> with no file to read; `go` itself
// then reads the go.mod the proxy synthesized for it, cache/download/<escaped path>/@v/<version>.mod
// (github.com/pkg/errors v0.9.1: one line, `module github.com/pkg/errors`). goModFile answers with
// that same file, so every in-process reader sees the module `go` sees -- and nothing is ever written
// into the read-only cache, and no go.mod is ever made up: a go.mod-less directory OUTSIDE the
// module cache has none.
func goModFile(dir string) (string, bool) {
	path := filepath.Join(dir, "go.mod")

	if info, err := os.Stat(path); err == nil && !info.IsDir() {
		return path, true
	}

	if synthesized := moduleCacheGoMod(dir); synthesized != "" {
		return synthesized, true
	}

	return "", false
}

// moduleCacheReleaseRoot reports whether dir IS a module-cache module root (<GOMODCACHE>/<escaped path>@<version>,
// never a package below one and never under cache/), with its escaped path and version.
func moduleCacheReleaseRoot(dir string) (escapedPath, version string, ok bool) {
	cache := goModCacheDir()

	if cache == "" {
		return "", "", false
	}

	rel, ok := moduleCacheRel(cache, dir)

	if !ok {
		return "", "", false
	}

	rel = filepath.ToSlash(rel)

	if rel == "cache" || strings.HasPrefix(rel, "cache/") {
		return "", "", false
	}

	at := strings.LastIndex(rel, "@")

	if at <= 0 || strings.Contains(rel[at:], "/") {
		return "", "", false
	}

	return rel[:at], rel[at+1:], true
}

// moduleCacheReleaseGoMod returns, for dir a module-cache module root, the go.mod a copy of it carries: its own when
// it ships one, else the cache's synthesized one (moduleCacheGoMod). "" when dir is not a cache release root, or is a
// go.mod-less one whose synthesized go.mod the cache does not hold.
func moduleCacheReleaseGoMod(dir string) string {
	if _, _, ok := moduleCacheReleaseRoot(dir); !ok {
		return ""
	}

	if info, err := os.Stat(filepath.Join(dir, "go.mod")); err == nil && info.Mode().IsRegular() {
		return filepath.Join(dir, "go.mod")
	}

	return moduleCacheGoMod(dir)
}

// moduleCacheGoMod returns the module cache's synthesized go.mod for dir when dir IS a module-cache
// module root (<GOMODCACHE>/<escaped path>@<version>, never a package below one) that ships no
// go.mod of its own, else "".
func moduleCacheGoMod(dir string) string {
	escapedPath, version, ok := moduleCacheReleaseRoot(dir)

	if !ok {
		return ""
	}

	if _, err := os.Stat(filepath.Join(dir, "go.mod")); err == nil {
		return ""
	}

	synthesized := filepath.Join(goModCacheDir(), "cache", "download", filepath.FromSlash(escapedPath), "@v", version+".mod")

	if info, err := os.Stat(synthesized); err != nil || info.IsDir() {
		return ""
	}

	return synthesized
}

// moduleCacheRel returns dir relative to the module cache root, and whether dir lies strictly inside it. The two can
// spell one folder differently through a symlink: on macOS /var is a symlink to /private/var, so GOMODCACHE keeps
// /var/... while a relative spelling of a release, made absolute through the working directory, reads /private/var/...
// (a darwin go-test, 17f79e9ba9). The textual answer is taken when it says inside; otherwise both sides are compared
// with their symlinks resolved.
func moduleCacheRel(cache, dir string) (string, bool) {
	inside := func(base, target string) (string, bool) {
		rel, err := filepath.Rel(base, target)

		if err != nil || rel == "." || filepath.IsAbs(rel) || rel == ".." || strings.HasPrefix(rel, ".."+string(filepath.Separator)) {
			return "", false
		}

		return rel, true
	}

	if rel, ok := inside(cache, dir); ok {
		return rel, true
	}

	realCache, err := filepath.EvalSymlinks(cache)

	if err != nil {
		return "", false
	}

	realDir, err := filepath.EvalSymlinks(dir)

	if err != nil {
		return "", false
	}

	return inside(realCache, realDir)
}

// copyModuleCacheRelease copies the module-cache release at dir (a root moduleCacheReleaseGoMod answers for) into a new
// work directory, under the same <escaped path>@<version> leaf, and writes goModSource -- the release's own go.mod, or
// the cache's synthesized one -- into the COPY. The cache is only read. It returns the copy and the work directory,
// which the caller removes.
func copyModuleCacheRelease(dir, goModSource string) (string, string, error) {
	goMod, err := os.ReadFile(goModSource)

	if err != nil {
		return "", "", fmt.Errorf("reading the go.mod %s for a copy of the module-cache release %s: %w", goModSource, dir, err)
	}

	sweepStaleModuleCopies(os.TempDir(), 24*time.Hour)
	work, err := os.MkdirTemp("", "go2cs-modcopy-")

	if err != nil {
		return "", "", err
	}

	// The temp root's real path: go reports a source under it resolved (macOS's /var is /private/var; it was met on the
	// hosted Macs), and every map from the copy back to the release compares that spelling (INC 4 review, NIT).
	if real, realErr := filepath.EvalSymlinks(work); realErr == nil {
		work = real
	}

	copyDir := filepath.Join(work, filepath.Base(dir))

	err = filepath.WalkDir(dir, func(path string, entry os.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		rel, err := filepath.Rel(dir, path)

		if err != nil {
			return err
		}

		target := filepath.Join(copyDir, rel)

		if entry.IsDir() {
			return os.MkdirAll(target, 0o755)
		}

		if !entry.Type().IsRegular() {
			return nil
		}

		data, err := os.ReadFile(path)

		if err != nil {
			return err
		}

		return os.WriteFile(target, data, 0o644)
	})

	if err == nil {
		err = os.WriteFile(filepath.Join(copyDir, "go.mod"), goMod, 0o644)
	}

	if err != nil {
		_ = os.RemoveAll(work)
		return "", "", fmt.Errorf("copying the module-cache release %s: %w", dir, err)
	}

	return copyDir, work, nil
}

// copiedModuleRoot and originalModuleRoot are set while a go.mod-less module-cache release converts from its copy:
// remapCopiedPaths writes the release's own path for the copy's in every diagnostic and load warning, so a user reads
// paths that exist on their machine (decision on INC 4 increment 1, 75eb0f85ce).
var copiedModuleRoot, originalModuleRoot string

func remapCopiedPaths(s string) string {
	if copiedModuleRoot == "" {
		return s
	}

	s = strings.ReplaceAll(s, copiedModuleRoot, originalModuleRoot)

	// A message may quote the copy forward-slashed (on Windows the native spelling has backslashes), and the copy is
	// gone when the user reads it (delta review, NIT 2).
	return strings.ReplaceAll(s, filepath.ToSlash(copiedModuleRoot), filepath.ToSlash(originalModuleRoot))
}

// remapCopiedError is err with the copy's path written as the release's (remapCopiedPaths), its chain kept for errors.As
// and errors.Is; err itself when its text names no copy.
func remapCopiedError(err error) error {
	if err == nil {
		return nil
	}

	if text := remapCopiedPaths(err.Error()); text != err.Error() {
		return remappedCopyError{text: text, err: err}
	}

	return err
}

type remappedCopyError struct {
	text string
	err  error
}

func (e remappedCopyError) Error() string { return e.text }
func (e remappedCopyError) Unwrap() error { return e.err }

// originalSourcePath is the release's own path for a source under the copy, else path unchanged: the same map, for the
// Go file identity a package records (goSourceIdentity), which reads the module and its VERSION from the cache path
// (review of INC 4 increment 1, MUST 1: from the copy it recorded `<module>@/<file>`, with no version).
//
// In a copy run, a source that lies under a go2cs-modcopy-* directory but not under this run's copy is an error naming
// both spellings: the loader reported the copy under another spelling (a TMPDIR reached through a symlink, a Windows
// 8.3 name), and passing it through would record the main-module `<module>@/<file>` form again, which the pack guard
// accepts (delta review, SHOULD 1). No route to it is known; it fails loud rather than silently.
func originalSourcePath(path string) (string, error) {
	if copiedModuleRoot == "" {
		return path, nil
	}

	if isPathUnder(path, copiedModuleRoot) {
		if rel, err := filepath.Rel(copiedModuleRoot, path); err == nil {
			return filepath.Join(originalModuleRoot, rel), nil
		}
	}

	if underModuleCopy(path) {
		return "", fmt.Errorf("the Go source %s lies under a go2cs-modcopy-* directory, but not under this run's copy %s of the module-cache release %s, so its identity cannot be mapped back to the release", path, copiedModuleRoot, originalModuleRoot)
	}

	return path, nil
}

// releaseSourcePath is originalSourcePath for a reader that records the source in emitted output (goSourceIdentity,
// sourceProvenance): an unmapped copy source refuses the conversion rather than record the copy.
func releaseSourcePath(path string) string {
	mapped, err := originalSourcePath(path)

	if err != nil {
		log.Fatalf("Refusing to convert: %s\n", err)
	}

	return mapped
}

// underModuleCopy reports whether a directory of path is named go2cs-modcopy-* (copyModuleCacheRelease's work directory).
func underModuleCopy(path string) bool {
	for _, segment := range strings.Split(filepath.ToSlash(path), "/") {
		if strings.HasPrefix(segment, "go2cs-modcopy-") {
			return true
		}
	}

	return false
}

// sweepStaleModuleCopies removes go2cs-modcopy-* directories under dir older than maxAge: a run that ended by os.Exit or a
// signal leaves its copy, since the deferred removal never runs. Whether another live run holds a younger one cannot be
// told reliably on every OS, so only a copy older than any conversion runs is removed (review SHOULD 4).
func sweepStaleModuleCopies(dir string, maxAge time.Duration) {
	entries, err := os.ReadDir(dir)

	if err != nil {
		return
	}

	for _, entry := range entries {
		if !entry.IsDir() || !strings.HasPrefix(entry.Name(), "go2cs-modcopy-") {
			continue
		}

		if info, err := entry.Info(); err == nil && time.Since(info.ModTime()) > maxAge {
			_ = os.RemoveAll(filepath.Join(dir, entry.Name()))
		}
	}
}

// enterModuleCacheReleaseCopy sets a go.mod-less module-cache release at dir up to convert, and test, from a COPY
// carrying the go.mod the cache synthesized for it (copyModuleCacheRelease), the one go itself reads. Not a go.mod
// overlay: go list and go build refuse any overlay beneath GOMODCACHE ("Files beneath GOMODCACHE must not be
// replaced"), and only go mod download accepts one. While inside, every diagnostic and Go source identity names the
// release, not the copy (copiedModuleRoot), and GOWORK is off for every go command: the copy sits outside the cache, so
// go loads it as a main module, where a user's GOWORK would apply ("directory ... does not contain modules listed in
// go.work"; a load from inside the cache turns it off, conversionDriver.go). leave restores GOWORK, clears the roots and
// removes the copy. A caller that returns an error from inside defers remapCopiedError AFTER leave, so it runs first.
// Plain -recurse enters it in ConvertModule; -tests -recurse in runModuleTests, around both of its phases, so its go
// test runs see the same copy its conversion read.
//
// EVERY module-cache release enters it, not only a go.mod-less one (decided 2026-11-22 at a stop line; 85efd7b257): with a
// cache directory as its main module, the dependency preflight's `go mod download` rewrites a go.mod that has no `go`
// directive and writes a go.sum, even under -mod=readonly, and a root process is not stopped by the cache's read-only
// modes (gopkg.in/yaml.v3 v3.0.1 and github.com/google/uuid v1.6.0, measured). In the copy those writes are harmless.
func enterModuleCacheReleaseCopy(dir, goModSource string) (copyDir string, leave func(), err error) {
	copyDir, work, err := copyModuleCacheRelease(dir, goModSource)

	if err != nil {
		return "", nil, err
	}

	if goModSource == filepath.Join(dir, "go.mod") {
		fmt.Printf("module cache: converting a copy of %s, so that no go command runs in the cache (%s, removed at exit)\n", dir, copyDir)
	} else {
		fmt.Printf("go.mod: the module at %s ships none; converting a copy carrying the module cache's %s, as go reads it (%s, removed at exit)\n",
			dir, goModSource, copyDir)
	}

	copiedModuleRoot, originalModuleRoot = copyDir, dir
	savedGowork, hadGowork := os.LookupEnv("GOWORK")
	_ = os.Setenv("GOWORK", "off")

	leave = func() {
		if hadGowork {
			_ = os.Setenv("GOWORK", savedGowork)
		} else {
			_ = os.Unsetenv("GOWORK")
		}

		copiedModuleRoot, originalModuleRoot = "", ""
		_ = os.RemoveAll(work)
	}

	return copyDir, leave, nil
}

// defaultGoModCache is Go's own default module cache, for when neither GOMODCACHE nor `go env` answers: the first
// GOPATH entry's pkg/mod, and with no GOPATH, <home>/go/pkg/mod, as `go` defaults GOPATH. Empty when neither is known,
// rather than the relative pkg/mod an empty GOPATH would join into, which no absolute path is ever under.
func defaultGoModCache(gopath, home string) string {
	if entries := filepath.SplitList(gopath); len(entries) > 0 && entries[0] != "" {
		return filepath.Join(entries[0], "pkg", "mod")
	}

	if home != "" {
		return filepath.Join(home, "go", "pkg", "mod")
	}

	return ""
}

// userHomeDirOrEmpty is the user's home directory, or empty when the OS cannot say.
func userHomeDirOrEmpty() string {
	home, _ := os.UserHomeDir()
	return home
}
