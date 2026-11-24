// cgoRecurseStd.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/parser"
	"go/token"
	"os/exec"
	"sort"
	"strings"
	"sync"

	"golang.org/x/tools/go/packages"
)

// Under -recurse -cgo every non-std package converts from its cgo-ON load, as Go's cgo build compiles it, while the
// standard library is the go.* packages, converted cgo-OFF by ruling (.claude/rules/corpus.md). Where Go builds a std
// package from DIFFERENT files with cgo on, a converted package importing it binds the cgo-off build. The divergent set
// is derived per target from the toolchain: at go1.26.11 net, os/user, plugin and runtime/cgo on linux, plugin and
// runtime/cgo on darwin, runtime/cgo on windows (cgo against go.* INC 1, C2 e95260f9fa).
//
// For most of that set it is a DISCLOSED divergence and converts (decision on the INC 1 review, option b; 4e98cf5fab): net's
// and os/user's cgo-off builds are working Go builds of the same API, the ones Go itself uses under CGO_ENABLED=0 or
// the netgo / osusergo tags (net's pure-Go resolver; docs/Limitations.md). Refused by name (R5) are only the packages
// of that set whose cgo-off build is NOT a working build of the same API: runtime/cgo (R5c) and plugin.
var cgoRefusedDivergentStd = map[string]bool{"runtime/cgo": true, "plugin": true}

var (
	cgoStdDivergenceMutex sync.Mutex
	cgoStdDivergence      = map[string]map[string]bool{} // keyed by target
)

// cgoDivergentStdPackages returns the importable std packages whose selected files differ between a cgo-off and a
// cgo-on load for the load's target. It runs `go list std runtime/cgo` once per setting, in the load's own environment.
// The cache is keyed by the target alone, though the listing also depends on the build tags: one run has one tag set
// (loaderBuildFlags), so the target is the only thing that varies across the packages of a run.
func cgoDivergentStdPackages(load cgoLoad, target string) (map[string]bool, error) {
	cgoStdDivergenceMutex.Lock()
	defer cgoStdDivergenceMutex.Unlock()

	if divergent, ok := cgoStdDivergence[target]; ok {
		return divergent, nil
	}

	listings := [2]map[string]string{}

	for i, setting := range []string{"CGO_ENABLED=0", "CGO_ENABLED=1"} {
		cmd := exec.Command("go", "list", "-e", "-f", "{{.ImportPath}} {{join .GoFiles \",\"}} {{join .CgoFiles \",\"}}", "std", "runtime/cgo")
		cmd.Env = append(append([]string{}, load.env...), setting)
		cmd.Dir = load.dir
		out, err := cmd.Output()

		if err != nil {
			return nil, fmt.Errorf("cgo: go list std at %s for %s: %w", setting, target, err)
		}

		listings[i] = map[string]string{}

		for _, line := range strings.Split(strings.ReplaceAll(string(out), "\r\n", "\n"), "\n") {
			if path, files, found := strings.Cut(line, " "); found {
				listings[i][path] = files
			}
		}
	}

	divergent := map[string]bool{}

	for path, files := range listings[1] {
		if listings[0][path] != files && cgoImportableStdPath(path) {
			divergent[path] = true
		}
	}

	cgoStdDivergence[target] = divergent

	return divergent, nil
}

// cgoImportableStdPath reports whether a module may import the std package: no internal or vendored path.
func cgoImportableStdPath(path string) bool {
	return !strings.HasPrefix(path, "internal/") && !strings.Contains(path, "/internal/") && !strings.HasSuffix(path, "/internal") &&
		!strings.HasPrefix(path, "vendor/")
}

// refuseCgoDivergentStdImports refuses a package, under -recurse -cgo, whose source files import a std package of
// divergent that cgoRefusedDivergentStd names, reporting each import site. Only the imports the package's OWN files declare count: cmd/cgo's output for
// any import "C" file imports runtime/cgo, syscall and unsafe itself, so pkg.Imports and the cgo-on load's syntax (its
// compiled files) would refuse every cgo package for its own plumbing. The files are re-read imports-only.
func refuseCgoDivergentStdImports(pkg *packages.Package, divergent map[string]bool) error {
	hitSet := map[string]bool{}
	fset := token.NewFileSet()

	message := func(path string) string {
		return fmt.Sprintf("package %s imports %q, which Go builds from different files with cgo on; the go.* packages are the "+
			"standard library converted cgo-off, and %s's cgo-off build is not a working build of its cgo API (docs/Limitations.md#cgo)", pkg.PkgPath, path, path)
	}

	for _, name := range pkg.GoFiles {
		file, err := parser.ParseFile(fset, name, nil, parser.ImportsOnly)

		if err != nil {
			return fmt.Errorf("cgo: reading the imports of %s: %w", name, err)
		}

		for _, spec := range file.Imports {
			path := strings.Trim(spec.Path.Value, "\"`")

			if divergent[path] && cgoRefusedDivergentStd[path] {
				hitSet[path] = true
				reportDiagnostic(fset.Position(spec.Pos()).String(), diagnosticCgoRefused, message(path))
			}
		}
	}

	if len(hitSet) == 0 {
		return nil
	}

	hits := make([]string, 0, len(hitSet))

	for path := range hitSet {
		hits = append(hits, path)
	}

	sort.Strings(hits)

	quoted := make([]string, len(hits))

	for i, path := range hits {
		quoted[i] = fmt.Sprintf("%q", path)
	}

	return fmt.Errorf("package %s imports %s, built from different files with cgo on than in the cgo-off go.* packages (docs/Limitations.md#cgo)",
		pkg.PkgPath, strings.Join(quoted, ", "))
}
