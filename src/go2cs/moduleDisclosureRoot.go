// moduleDisclosureRoot.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The committed home of a third-party module row's disclosures (COORD ruling 2026-10-05, 21:27Z).
//
// A disclosure manifest is read from the package's OUTPUT directory (loadTestDisclosures). For the
// standard library that directory is the committed src/core/<pkg>; for a module converted with
// -tests -recurse it is whatever output root the run was given, which nothing in the repository holds.
// -module-disclosures names a committed tree laid out like the module cache:
//
//	<root>/<escaped module path>@<version>/<package dir within the module>/go2cs_test_disclosures.json
//
// The tree is consulted ONLY when the output directory holds no manifest (an output-directory manifest
// still wins), and ONLY for the module version this run resolved: a manifest kept for another version is
// never applied, and one line names both. Whatever is applied enters the same loader, scope and orphan
// rule a std manifest does.

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/mod/module"
	"golang.org/x/mod/semver"
)

// resolveDisclosureManifestDir returns the directory whose go2cs_test_disclosures.json a -tests run
// reads, and one line to print about it ("" for none). outputPath is the package's output directory;
// root is the -module-disclosures tree ("" when the flag is absent); modulePath and moduleDir are the
// main module's import path and directory ("" for the standard library); pkgDir is the package's
// source directory.
func resolveDisclosureManifestDir(outputPath, root, modulePath, moduleDir, pkgDir string) (string, string) {
	if root == "" || modulePath == "" || fileExists(filepath.Join(outputPath, testDisclosureFileName)) {
		return outputPath, ""
	}

	escapedPath, err := module.EscapePath(modulePath)

	if err != nil {
		return outputPath, fmt.Sprintf("module disclosures: %s is not a valid module path (%v); %s not consulted", modulePath, err, root)
	}

	version := moduleCacheVersion(moduleDir, escapedPath)

	if version == "" {
		return outputPath, fmt.Sprintf("module disclosures: %s has no resolved version (not a module-cache directory); %s not consulted", modulePath, root)
	}

	relPkg, err := filepath.Rel(moduleDir, pkgDir)

	if err != nil || relPkg == ".." || strings.HasPrefix(relPkg, ".."+string(filepath.Separator)) {
		return outputPath, fmt.Sprintf("module disclosures: %s is not inside module %s; %s not consulted", pkgDir, modulePath, root)
	}

	moduleBase := filepath.Join(root, filepath.FromSlash(escapedPath))
	candidate := filepath.Join(moduleBase+"@"+version, relPkg)

	if fileExists(filepath.Join(candidate, testDisclosureFileName)) {
		return candidate, fmt.Sprintf("module disclosures: applying %s", filepath.Join(candidate, testDisclosureFileName))
	}

	// The module is kept under OTHER versions only: say so, by name, and apply nothing.
	others, _ := filepath.Glob(moduleBase + "@*")
	var present []string

	for _, other := range others {
		if fileExists(filepath.Join(other, relPkg, testDisclosureFileName)) {
			present = append(present, other)
		}
	}

	if len(present) > 0 {
		sort.Strings(present)
		return outputPath, fmt.Sprintf("module disclosures: %s exists; this run resolved %s; not applied", strings.Join(present, ", "), version)
	}

	return outputPath, ""
}

// moduleCacheVersion is the version a module-cache directory names (`…/<escaped path>@<version>`), or ""
// when moduleDir is not one: a plain checkout of a module has no version this run can claim.
func moduleCacheVersion(moduleDir, escapedPath string) string {
	if moduleDir == "" {
		return ""
	}

	dir := filepath.ToSlash(filepath.Clean(moduleDir))
	marker := escapedPath + "@"
	index := strings.LastIndex(dir, marker)

	if index < 0 || (index > 0 && dir[index-1] != '/') {
		return ""
	}

	escapedVersion := dir[index+len(marker):]

	if strings.Contains(escapedVersion, "/") {
		return ""
	}

	version, err := module.UnescapeVersion(escapedVersion)

	if err != nil || !semver.IsValid(version) {
		return ""
	}

	return version
}

func fileExists(path string) bool {
	info, err := os.Stat(path)
	return err == nil && !info.IsDir()
}

// absPathOrEmpty makes a command-line path absolute ("" stays ""), so a relative -module-disclosures
// resolves against the directory the converter was started in, not whatever a child process uses.
func absPathOrEmpty(path string) string {
	if path == "" {
		return ""
	}

	if abs, err := filepath.Abs(path); err == nil {
		return abs
	}

	return path
}
