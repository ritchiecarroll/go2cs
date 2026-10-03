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
	"os"
	"path/filepath"
	"strings"
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

	rel, err := filepath.Rel(cache, dir)

	if err != nil || rel == "." || filepath.IsAbs(rel) || rel == ".." || strings.HasPrefix(rel, ".."+string(filepath.Separator)) {
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

// installGoModOverlay hands the go commands of a -tests -recurse run the same answer goModFile gives
// in-process: for a module-cache module with no go.mod it writes a `go build -overlay` file mapping
// <moduleDir>/go.mod to the cache's synthesized go.mod, and appends -overlay=<file> to GOFLAGS so the
// loader's `go list`, and every `go test` the run starts, read it. It says so on stdout. A module with
// its own go.mod is untouched (returns ""), as is every other conversion: this runs only from the
// -tests -recurse driver.
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
