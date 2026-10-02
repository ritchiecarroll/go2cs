// nugetSubstitution.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"sort"
	"strings"

	"go2cs/internal/sourcemeta"
	"go2cs/internal/stdlibmeta"
)

// The SUBSTITUTION a -recurse=nuget run applies (docs/PLAN-nugetgo.md 4.2, stage S3b): a third-party module
// whose mapping selected a qualifying package is not converted; every import of one of its packages becomes
// a PackageReference to that package, exact-pinned (one per MODULE), and the importer reads the package's
// metadata from its self-description instead of a package_info.cs on disk.

// nugetSubstitution is one third-party module this run references as a published package.
type nugetSubstitution struct {
	module         string
	version        string
	nugetID        string
	packageVersion string
	description    sourcemeta.Description
}

// nugetSubstitutions holds this run's substitutions by module path. ModuleConverter sets it for the run it
// converts (the way it sets conversionGraph), and every package conversion in that run reads it.
var nugetSubstitutions map[string]*nugetSubstitution

// nugetSubstitutionFor returns the substitution covering an import path -- the one whose module path is the
// LONGEST prefix of it (a nested module is its own module) -- or nil.
func nugetSubstitutionFor(importPath string) *nugetSubstitution {
	var best *nugetSubstitution

	for modulePath, substitution := range nugetSubstitutions {
		if importPath != modulePath && !strings.HasPrefix(importPath, modulePath+"/") {
			continue
		}

		if best == nil || len(modulePath) > len(best.module) {
			best = substitution
		}
	}

	return best
}

// metadataLines returns the self-description's metadata section for one of the package's import paths, for
// a conversion targeting goos: the flavored section first, then the unqualified one (the stdlib record's rule).
func (s *nugetSubstitution) metadataLines(importPath string, goos string) ([]string, bool) {
	name := strings.ReplaceAll(importPath, "/", ".")

	if goos != "" {
		if lines, ok := s.description.Sections[name+stdlibmeta.FlavorSeparator+goos]; ok {
			return lines, true
		}
	}

	lines, ok := s.description.Sections[name]
	return lines, ok
}

// closeNuGetSubstitutions enforces CLOSURE CONSISTENCY (COORD ruling 2026-10-02). A substituted module's
// package brings every module it requires through NuGet; if this run converts one of those locally, two
// assemblies would carry one namespace. So a substitution stands only while every module its package
// requires is either absent from this run's closure (nothing converts it locally; NuGet supplies it) or
// substituted HERE by the same package at the same module version. Anything else DEMOTES it to local
// conversion -- and demotion is a fixed point, because a demoted module now converts locally and so blocks
// every substitution that requires it. Returns each demoted module's reason, naming its blocker.
func closeNuGetSubstitutions(substitutions map[string]*nugetSubstitution, modules map[string]thirdPartyModule) map[string]string {
	demoted := make(map[string]string)

	for changed := true; changed; {
		changed = false

		paths := make([]string, 0, len(substitutions))

		for path := range substitutions {
			paths = append(paths, path)
		}

		sort.Strings(paths)

		for _, path := range paths {
			substitution := substitutions[path]

			for _, require := range substitution.description.Requires {
				selected, inClosure := modules[require.Module]

				if !inClosure {
					continue
				}

				reason := ""
				other, substituted := substitutions[require.Module]

				switch {
				case !substituted:
					reason = fmt.Sprintf("demoted: its package requires %s, which this run converts locally", require.Module)
				case other.nugetID != require.NuGetID:
					reason = fmt.Sprintf("demoted: its package requires %s as %s, but this run maps it to %s", require.Module, require.NuGetID, other.nugetID)
				case selected.version != require.Version:
					reason = fmt.Sprintf("demoted: its package was built against %s %s, and this run selects %s", require.Module, require.Version, selected.version)
				}

				if reason != "" {
					demoted[path] = reason
					delete(substitutions, path)
					changed = true
					break
				}
			}
		}
	}

	return demoted
}

// substituteNuGetMappings turns this run's mapping decisions into substitutions: it selects a qualifying
// package version for every mapped module, demotes what closure consistency forbids, marks each decision
// APPLIED or records why the module converts locally, and pins every applied package's version and SHA-512
// in the output root's go2cs.nuget.lock. A mapped module that is not applied keeps its name-level lock line
// with '-' in both package fields, so the next run selects again. It returns the substitutions by module
// path and the warnings for the report; its error is an integrity failure (a locked hash that moved).
func substituteNuGetMappings(decisions []nugetMapDecision, modules map[string]thirdPartyModule, outRoot string, refresh bool) (map[string]*nugetSubstitution, []string, error) {
	// No decision at all: mapping is off (-nuget-map off), or the closure has no third-party module. Nothing
	// to select, and nothing of this run's to record -- under off, the lock is not even read.
	if len(decisions) == 0 {
		return nil, nil, nil
	}

	lock, err := readNuGetLock(outRoot)

	if err != nil {
		return nil, nil, err
	}

	substitutions := make(map[string]*nugetSubstitution)
	reasons := make(map[string]string)
	var warnings []string

	for _, decision := range decisions {
		if !decision.mapped {
			continue
		}

		mod := modules[decision.module]

		if mod.path == "" {
			mod = thirdPartyModule{path: decision.module, version: decision.version}
		}

		locked, haveLock := lock[decision.module]
		choice, notes, err := selectNuGetPackage(decision.nugetID, mod, locked, haveLock, refresh)

		if err != nil {
			return nil, nil, err
		}

		if choice.packageVersion == "" {
			reasons[decision.module] = "no published version of " + decision.nugetID + " qualifies -- " + strings.Join(notes, "; ")
			warnings = append(warnings, fmt.Sprintf("%s maps to %s, but %s, so it converts locally", decision.module, decision.nugetID, reasons[decision.module]))
			continue
		}

		substitutions[decision.module] = &nugetSubstitution{module: decision.module, version: mod.version, nugetID: decision.nugetID,
			packageVersion: choice.packageVersion, description: choice.description}

		if entry, ok := lock[decision.module]; ok {
			entry.packageVersion, entry.contentHash = choice.packageVersion, choice.contentHash
			lock[decision.module] = entry
		}
	}

	for module, reason := range closeNuGetSubstitutions(substitutions, modules) {
		reasons[module] = reason
		warnings = append(warnings, fmt.Sprintf("%s converts locally: %s", module, reason))
	}

	for index := range decisions {
		decision := &decisions[index]

		if substitution, ok := substitutions[decision.module]; ok {
			decision.applied, decision.packageVersion = true, substitution.packageVersion
			continue
		}

		if reason, ok := reasons[decision.module]; ok {
			decision.note = reason

			if entry, locked := lock[decision.module]; locked {
				entry.packageVersion, entry.contentHash = "-", "-"
				lock[decision.module] = entry
			}
		}
	}

	if err := writeNuGetLock(outRoot, lock); err != nil {
		return nil, nil, fmt.Errorf("writing %s: %w", nugetLockPath(outRoot), err)
	}

	sort.Strings(warnings)
	return substitutions, warnings, nil
}
