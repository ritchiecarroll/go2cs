// goModFile.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/json"
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

// moduleCacheGoMod returns the module cache's synthesized go.mod for dir when dir IS a module-cache
// module root (<GOMODCACHE>/<escaped path>@<version>, never a package below one) that ships no
// go.mod of its own, else "".
func moduleCacheGoMod(dir string) string {
	cache := goModCacheDir()

	if cache == "" {
		return ""
	}

	rel, ok := moduleCacheRel(cache, dir)

	if !ok {
		return ""
	}

	rel = filepath.ToSlash(rel)

	if rel == "cache" || strings.HasPrefix(rel, "cache/") {
		return ""
	}

	at := strings.LastIndex(rel, "@")

	if at <= 0 || strings.Contains(rel[at:], "/") {
		return ""
	}

	if _, err := os.Stat(filepath.Join(dir, "go.mod")); err == nil {
		return ""
	}

	synthesized := filepath.Join(cache, "cache", "download", filepath.FromSlash(rel[:at]), "@v", rel[at+1:]+".mod")

	if info, err := os.Stat(synthesized); err != nil || info.IsDir() {
		return ""
	}

	return synthesized
}

// moduleCacheRel returns dir relative to the module cache root, and whether dir lies strictly inside it. The two can
// spell one folder differently through a symlink: on macOS /var is a symlink to /private/var, so GOMODCACHE keeps
// /var/... while a relative spelling of a release, made absolute through the working directory, reads /private/var/...
// (train 5's darwin go-test). The textual answer is taken when it says inside; otherwise both sides are compared
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

// copyModuleCacheRelease copies the module-cache release at dir (a root moduleCacheGoMod answers for) into a new work
// directory, under the same <escaped path>@<version> leaf, and writes the cache's synthesized go.mod into the COPY. The
// cache is only read. It returns the copy and the work directory, which the caller removes.
func copyModuleCacheRelease(dir, synthesized string) (string, string, error) {
	goMod, err := os.ReadFile(synthesized)

	if err != nil {
		return "", "", fmt.Errorf("reading the module cache's synthesized go.mod %s: %w", synthesized, err)
	}

	sweepStaleModuleCopies(os.TempDir(), 24*time.Hour)
	work, err := os.MkdirTemp("", "go2cs-modcopy-")

	if err != nil {
		return "", "", err
	}

	// The temp root's real path: go reports a source under it resolved (macOS's /var is /private/var; C1 met it on the
	// hosted Macs), and every map from the copy back to the release compares that spelling (COORD's INC 4 review, NIT).
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
// paths that exist on their machine (COORD's ruling on INC 4 increment 1).
var copiedModuleRoot, originalModuleRoot string

func remapCopiedPaths(s string) string {
	if copiedModuleRoot == "" {
		return s
	}

	s = strings.ReplaceAll(s, copiedModuleRoot, originalModuleRoot)

	// A message may quote the copy forward-slashed (on Windows the native spelling has backslashes), and the copy is
	// gone when the user reads it (P3's delta read, NIT 2).
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
// (P3's read of INC 4 increment 1, MUST 1: from the copy it recorded `<module>@/<file>`, with no version).
//
// In a copy run, a source that lies under a go2cs-modcopy-* directory but not under this run's copy is an error naming
// both spellings: the loader reported the copy under another spelling (a TMPDIR reached through a symlink, a Windows
// 8.3 name), and passing it through would record the main-module `<module>@/<file>` form again, which the pack guard
// accepts (P3's delta read, SHOULD 1). No route to it is known; it fails loud rather than silently.
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
// told reliably on every OS, so only a copy older than any conversion runs is removed (P3's read, SHOULD 4).
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

// installGoModOverlay hands the go commands of a -tests -recurse run the same answer goModFile gives
// in-process: for a module-cache module with no go.mod it writes a `go build -overlay` file mapping
// <moduleDir>/go.mod to the cache's synthesized go.mod, and appends -overlay=<file> to GOFLAGS so the
// loader's `go list`, and every `go test` the run starts, read it. It says so on stdout. A module with
// its own go.mod is untouched (returns ""), as is every other conversion: this runs only from the
// -tests -recurse driver. (Plain -recurse converts a copy instead: copyModuleCacheRelease.)
func installGoModOverlay(moduleDir string, outRoot string) (string, error) {
	synthesized := moduleCacheGoMod(moduleDir)

	if synthesized == "" {
		return "", nil
	}

	goflags := os.Getenv("GOFLAGS")

	if strings.Contains(goflags, "-overlay") {
		return "", fmt.Errorf("the module at %s ships no go.mod, and GOFLAGS already carries an -overlay (%q): "+
			"refusing to replace it", moduleDir, goflags)
	}

	overlayPath := filepath.Join(outRoot, "go2cs-gomod-overlay.json")

	// GOFLAGS splits on white space, so the overlay file's path must not carry any.
	if strings.ContainsAny(overlayPath, " \t") {
		return "", fmt.Errorf("the module at %s ships no go.mod, and its overlay file %s would have to reach `go` through "+
			"GOFLAGS, which cannot carry a path with white space: choose an output root without spaces", moduleDir, overlayPath)
	}

	data, err := json.Marshal(map[string]map[string]string{"Replace": {filepath.Join(moduleDir, "go.mod"): synthesized}})

	if err != nil {
		return "", err
	}

	if err := os.MkdirAll(outRoot, 0o755); err != nil {
		return "", err
	}

	if err := os.WriteFile(overlayPath, data, 0o644); err != nil {
		return "", fmt.Errorf("writing the go.mod overlay: %w", err)
	}

	if err := os.Setenv("GOFLAGS", strings.TrimSpace(goflags+" -overlay="+overlayPath)); err != nil {
		return "", err
	}

	fmt.Printf("go.mod: the module at %s ships none; using the module cache's %s, as go does (overlay %s)\n",
		moduleDir, synthesized, overlayPath)

	return overlayPath, nil
}
