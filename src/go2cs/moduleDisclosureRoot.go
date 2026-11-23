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
	"bytes"
	"fmt"
	"os"
	"path/filepath"
	"slices"
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

	claimed := claimedVersion != "" && moduleCacheVersion(moduleDir, escapedPath) == ""

	if fileExists(filepath.Join(candidate, testDisclosureFileName)) {
		note := fmt.Sprintf("module disclosures: applying %s", filepath.Join(candidate, testDisclosureFileName))

		if claimed {
			note += fmt.Sprintf(" (version %s claimed by -module-version, not resolved)", version)
		}

		return candidate, note, nil
	}

	// The module is kept under OTHER versions only: say so, by name, and apply nothing.
	others := moduleVersionDirs(moduleBase)
	var present []string

	for _, other := range others {
		if fileExists(filepath.Join(other, relPkg, testDisclosureFileName)) {
			present = append(present, other)
		}
	}

	if len(present) > 0 {
		sort.Strings(present)

		// With a CLAIMED version this is most likely a wrong claim, and a note would leave the run exactly where GAP 1
		// found it: nothing applied, a host-fatal test not withdrawn (review SHOULD 2, 6c978b079b). A RESOLVED version keeps
		// the ruled line (2026-10-05).
		if claimed {
			return outputPath, "", fmt.Errorf("-module-version %s: %s holds %s only under other versions (%s); the claim is "+
				"most likely wrong", version, root, modulePath, strings.Join(present, ", "))
		}

		return outputPath, fmt.Sprintf("module disclosures: %s exists; this run resolved %s; not applied", strings.Join(present, ", "), version), nil
	}

	return outputPath, "", nil
}

// moduleDisclosureVersion is the module version a -module-disclosures root is read for, with the module's escaped
// path: the version a module-cache directory names (moduleCacheVersion), or -module-version (claimedVersion) for a
// module source that is not one, such as a copy of the module. A copy carries no version of its own: the extracted
// tree has no version file and its go.mod does not name it. The two must agree when both are present.
//
// A root that cannot be applied is REFUSED, not noted (GAP 1, 3edeaaffab). With a note, a sqlite run
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

		if cacheVersion == "" {
			if cached := claimedGoModMismatch(moduleDir, escapedPath, claimedVersion); cached != "" {
				return "", "", fmt.Errorf("-module-version %s is contradicted by the copy itself: %s differs from the module "+
					"cache's go.mod for %s@%s (%s)", claimedVersion, filepath.Join(moduleDir, "go.mod"), modulePath, claimedVersion, cached)
			}
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

// claimedVersionCacheDir is the module cache a CLAIMED version is checked against (claimedGoModMismatch).
// A variable so a test can point it at a cache of its own; goModCacheDir memoizes the real one.
var claimedVersionCacheDir = goModCacheDir

// moduleDisclosureFlagsError refuses -module-disclosures and -module-version where they cannot take effect (review
// SHOULD 3 and NIT 4), rather than accepting and ignoring them. Only a -tests -recurse run resolves the main
// module (runModuleTests); the single-package -tests route never does, so a root given there was ignored silently.
// -module-version names the version a root is read for, so it means nothing without one.
func moduleDisclosureFlagsError(options Options) error {
	if options.testModuleDisclosures != "" && !(options.convertTests && options.recurse) {
		return fmt.Errorf("-module-disclosures takes effect only with -tests -recurse: a single-package run never " +
			"resolves its module, so the disclosure root would be ignored")
	}

	if options.testModuleVersion != "" && options.testModuleDisclosures == "" {
		return fmt.Errorf("-module-version names the version a -module-disclosures root is read for, and no " +
			"-module-disclosures was given")
	}

	return nil
}

// moduleVersionClaimed is the version a run took from -module-version because the module directory is not a
// module-cache directory: CLAIMED, not resolved. The comparison record carries it (testEnvironmentRecord.
// ModuleVersionClaimed) and the proof page says so, so a row read under a claim cannot be banked unnoticed (review
// MUST 1a). "" when the version was resolved, including a cache directory -module-version agrees with.
func moduleVersionClaimed(options Options) string {
	if options.testModuleVersion == "" || options.mainModulePath == "" {
		return ""
	}

	escapedPath, err := module.EscapePath(options.mainModulePath)

	if err != nil || moduleCacheVersion(options.mainModuleDir, escapedPath) != "" {
		return ""
	}

	return options.testModuleVersion
}

// claimedGoModMismatch is the cheap contradiction check on a claimed version (review MUST 1b): when the module
// cache holds that version's go.mod (cache/download/<escaped path>/@v/<escaped version>.mod) and the copy has a
// go.mod, the two must be the same file, line endings aside. A trimmed copy rules out a whole-tree hash, so this is
// the check that can be made; "" when they agree or either is absent.
func claimedGoModMismatch(moduleDir, escapedPath, version string) string {
	escapedVersion, err := module.EscapeVersion(version)

	if err != nil {
		return ""
	}

	cached := filepath.Join(claimedVersionCacheDir(), "cache", "download", filepath.FromSlash(escapedPath), "@v", escapedVersion+".mod")
	want, err := os.ReadFile(cached)

	if err != nil {
		return ""
	}

	have, err := os.ReadFile(filepath.Join(moduleDir, "go.mod"))

	if err != nil {
		return ""
	}

	if bytes.Equal(bytes.ReplaceAll(want, []byte("\r\n"), []byte("\n")), bytes.ReplaceAll(have, []byte("\r\n"), []byte("\n"))) {
		return ""
	}

	return cached
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

	escapedPath, version, err := moduleDisclosureVersion(root, modulePath, moduleDir, claimedVersion)

	if err != nil || claimedVersion == "" || moduleCacheVersion(moduleDir, escapedPath) != "" {
		return err
	}

	// A claimed version the root holds the module under OTHER versions only: refused here too (SHOULD 2), at module
	// level, since the per-package lookup would only reach it after the conversion.
	moduleBase := filepath.Join(root, filepath.FromSlash(escapedPath))
	others := moduleVersionDirs(moduleBase)

	if len(others) > 0 && !slices.Contains(others, moduleBase+"@"+version) {
		sort.Strings(others)
		return fmt.Errorf("-module-version %s: %s holds %s only under other versions (%s); the claim is most likely wrong",
			version, root, modulePath, strings.Join(others, ", "))
	}

	return nil
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

// moduleVersionDirs lists the directories a -module-disclosures root keeps a module under, one per version:
// <moduleBase>@<version>, sorted. Read with os.ReadDir and a name prefix rather than filepath.Glob, which read a root
// path holding a glob metacharacter ([ or *) as a pattern and matched nothing (review NIT 6).
func moduleVersionDirs(moduleBase string) []string {
	entries, err := os.ReadDir(filepath.Dir(moduleBase))

	if err != nil {
		return nil
	}

	prefix := filepath.Base(moduleBase) + "@"
	var dirs []string

	for _, entry := range entries {
		if entry.IsDir() && strings.HasPrefix(entry.Name(), prefix) {
			dirs = append(dirs, filepath.Join(filepath.Dir(moduleBase), entry.Name()))
		}
	}

	sort.Strings(dirs)

	return dirs
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
