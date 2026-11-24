// cgoRecurseSubstitution.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os/exec"
	"sort"
	"strings"
)

// refuseCgoSplitSubstitutions refuses, under -recurse -cgo, a module SUBSTITUTED by a published package when Go builds
// any of its packages from different files with cgo on (INC 1 review, finding 1; 4e98cf5fab).
//
// A substituted module's packages leave the convert-set (nugetSubstitution.go), so no per-package rule reads them,
// while the closure type-checks cgo-ON and the published package was converted cgo-OFF. A //go:build cgo / !cgo pair
// in one would type-check against its cgo-on file and bind the published cgo-off one: silently, when the two declare
// the same names. One `go list` of the module's packages each way, in the closure load's environment, compares
// GoFiles and CgoFiles; a failing `go list` refuses too (fail closed).
func (m *ModuleConverter) refuseCgoSplitSubstitutions(moduleDir string, substitutions map[string]*nugetSubstitution) error {
	if !m.options.parseCgoTargets || len(substitutions) == 0 {
		return nil
	}

	var pkgPaths []string

	for pkgPath, modulePath := range m.thirdPartyPackages {
		if _, substituted := substitutions[modulePath]; substituted {
			pkgPaths = append(pkgPaths, pkgPath)
		}
	}

	if len(pkgPaths) == 0 {
		return nil
	}

	sort.Strings(pkgPaths)

	env := converterLoaderEnv()

	if parts := strings.Split(m.options.targetPlatform, "/"); len(parts) == 2 {
		env = converterLoaderEnv("GOOS="+parts[0], "GOARCH="+parts[1])
	}

	listings := [2]map[string]string{}

	for i, setting := range []string{"CGO_ENABLED=0", "CGO_ENABLED=1"} {
		// The run's own build flags (-tags), as the closure load passes them: a split under the run's tag is invisible
		// to a listing without it (INC 1 review, one SHOULD).
		args := append([]string{"list", "-e"}, m.options.loaderBuildFlags()...)
		args = append(append(args, "-f", "{{.ImportPath}} {{join .GoFiles \",\"}} {{join .CgoFiles \",\"}}"), pkgPaths...)
		cmd := exec.Command("go", args...)
		cmd.Env = append(append([]string{}, env...), setting)
		cmd.Dir = moduleDir
		out, err := cmd.Output()

		if err != nil {
			return fmt.Errorf("-recurse -cgo: go list of the substituted modules' packages at %s: %w", setting, err)
		}

		listings[i] = map[string]string{}

		for _, line := range strings.Split(strings.ReplaceAll(string(out), "\r\n", "\n"), "\n") {
			if path, files, found := strings.Cut(line, " "); found {
				listings[i][path] = files
			}
		}
	}

	var problems []string

	for _, pkgPath := range pkgPaths {
		if listings[0][pkgPath] == listings[1][pkgPath] {
			continue
		}

		modulePath := m.thirdPartyPackages[pkgPath]
		substitution := substitutions[modulePath]
		problems = append(problems, fmt.Sprintf("module %s is substituted by the published package %s, converted cgo-off, and Go builds its package %s from different files with cgo on",
			modulePath, substitution.nugetID, pkgPath))
	}

	if len(problems) == 0 {
		return nil
	}

	for _, problem := range problems {
		reportDiagnostic("-", diagnosticCgoRefused, problem)
	}

	return fmt.Errorf("-recurse -cgo: %s; the importer would type-check against the cgo-on files and bind the cgo-off build (convert the module locally: -nuget-map off)",
		strings.Join(problems, "; "))
}
