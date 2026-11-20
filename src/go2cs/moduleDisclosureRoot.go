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
//
// The version is the module-cache directory's, or -module-version's for a copy of the module (which names no
// version of its own). A root that cannot be applied, because neither gives a version or the two disagree, is
// REFUSED, before any conversion where the module's go.mod allows (moduleDisclosurePreflight), and never noted.

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/mod/modfile"
	"golang.org/x/mod/module"
	"golang.org/x/mod/semver"
)

// resolveDisclosureManifestDir returns the directory whose go2cs_test_disclosures.json a -tests run
// reads, and one line to print about it ("" for none). outputPath is the package's output directory;
// root is the -module-disclosures tree ("" when the flag is absent); modulePath and moduleDir are the
// main module's import path and directory ("" for the standard library); pkgDir is the package's
// source directory.
func resolveDisclosureManifestDir(outputPath, root, modulePath, moduleDir, claimedVersion, pkgDir string) (string, string, error) {
	if root == "" || modulePath == "" || fileExists(filepath.Join(outputPath, testDisclosureFileName)) {
		return outputPath, "", nil
	}

	escapedPath, version, err := moduleDisclosureVersion(root, modulePath, moduleDir, claimedVersion)

	if err != nil {
		return outputPath, "", err
	}

	relPkg, err := filepath.Rel(moduleDir, pkgDir)

	if err != nil || relPkg == ".." || strings.HasPrefix(relPkg, ".."+string(filepath.Separator)) {
		return outputPath, "", fmt.Errorf("-module-disclosures %s cannot be applied: %s is not inside module %s at %s", root, pkgDir, modulePath, moduleDir)
	}
	moduleBase := filepath.Join(root, filepath.FromSlash(escapedPath))
	candidate := filepath.Join(moduleBase+"@"+version, relPkg)

	if fileExists(filepath.Join(candidate, testDisclosureFileName)) {
		return candidate, fmt.Sprintf("module disclosures: applying %s", filepath.Join(candidate, testDisclosureFileName)), nil
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
		return outputPath, fmt.Sprintf("module disclosures: %s exists; this run resolved %s; not applied", strings.Join(present, ", "), version), nil
	}

	return outputPath, "", nil
}

// moduleDisclosureVersion is the module version a -module-disclosures root is read for, with the module's escaped
// path: the version a module-cache directory names (moduleCacheVersion), or -module-version (claimedVersion) for a
// module source that is not one, such as a copy of the module. A copy carries no version of its own: the extracted
// tree has no version file and its go.mod does not name it. The two must agree when both are present.
//
// A root that cannot be applied is REFUSED, not noted (COORD 20261120T091624Z, GAP 1). With a note, P5's sqlite run
// from a copy applied nothing, the host-fatal disclosure never withdrew its test, and the host aborted.
func moduleDisclosureVersion(root, modulePath, moduleDir, claimedVersion string) (string, string, error) {
	escapedPath, err := module.EscapePath(modulePath)

	if err != nil {
		return "", "", fmt.Errorf("-module-disclosures %s cannot be applied: %s is not a valid module path (%v)", root, modulePath, err)
	}

	cacheVersion := moduleCacheVersion(moduleDir, escapedPath)

	if claimedVersion != "" {
		if err := module.Check(modulePath, claimedVersion); err != nil {
			return "", "", fmt.Errorf("-module-version %q is not a version of %s: %v", claimedVersion, modulePath, err)
		}

		if cacheVersion != "" && cacheVersion != claimedVersion {
			return "", "", fmt.Errorf("-module-version %s contradicts the module-cache directory %s, which is %s@%s", claimedVersion, moduleDir, modulePath, cacheVersion)
		}

		return escapedPath, claimedVersion, nil
	}

	if cacheVersion == "" {
		return "", "", fmt.Errorf("-module-disclosures %s cannot be applied: %s at %s has no version (not a module-cache directory). "+
			"Run from the module-cache directory (go mod download -json %s@<version>, its Dir), or name the version the copy is "+
			"with -module-version", root, modulePath, moduleDir, modulePath)
	}

	return escapedPath, cacheVersion, nil
}

// moduleDisclosurePreflight refuses, before any conversion, a -tests -recurse run whose -module-disclosures root cannot
// be applied, reading the module path from moduleDir's go.mod. Without a go.mod (a module-cache release that has none)
// it passes, and the per-package lookup (resolveDisclosureManifestDir) still refuses.
func moduleDisclosurePreflight(root, moduleDir, claimedVersion string) error {
	if root == "" {
		return nil
	}

	data, err := os.ReadFile(filepath.Join(moduleDir, "go.mod"))

	if err != nil {
		return nil
	}

	modulePath := modfile.ModulePath(data)

	if modulePath == "" {
		return nil
	}

	_, _, err = moduleDisclosureVersion(root, modulePath, moduleDir, claimedVersion)

	return err
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
