// testRefsFootprint_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// TestRefsFootprintOverCommittedTestProjects is an INSTRUMENT, not a gate: it skips unless GO2CS_REFS_FOOTPRINT_GOOS
// names a target. It runs the converter's own reference closure (refsFootprintClosure, the call testConversion.go
// makes when it writes a -tests project) over every committed std test project, src/core/**/*.tests.csproj, and
// writes one TSV row per project: the import paths the closure adds to what the committed project references.
//
// The inputs are the committed files the converter itself reads at that step: the project's ProjectReferences (as
// the already-referenced set), its <Compile> items (a _test.go file whose conversion the project does not compile is
// compile-excluded), the package's package_info.cs for the target (the implemented-interface gate) and its
// package_test_info.cs (the record seeds). Run it at two refs with the same inputs and diff the TSVs: what differs is
// the footprint of the change between them, per project, per target. CLASS A's footprint (C1, 2026-11-20) is measured
// this way; the harness is committed so the reading can be repeated rather than re-derived.
//
//	GO2CS_REFS_FOOTPRINT_GOOS   windows | linux | darwin
//	GO2CS_REFS_FOOTPRINT_OUT    TSV path to write (required)
//	GO2CS_REFS_FOOTPRINT_SRC    the src directory holding core/ (default: ../ from this package)
//
// Needs only the committed metadata under src/core (package_info.cs, package_test_info.cs, *.tests.csproj) and the
// GOROOT the corpus is converted from. No conversion and no .NET.
func TestRefsFootprintOverCommittedTestProjects(t *testing.T) {
	goos := os.Getenv("GO2CS_REFS_FOOTPRINT_GOOS")
	if goos == "" {
		t.Skip("instrument: set GO2CS_REFS_FOOTPRINT_GOOS and GO2CS_REFS_FOOTPRINT_OUT to run")
	}

	out := os.Getenv("GO2CS_REFS_FOOTPRINT_OUT")
	if out == "" {
		t.Fatal("GO2CS_REFS_FOOTPRINT_OUT is required")
	}

	src := os.Getenv("GO2CS_REFS_FOOTPRINT_SRC")
	if src == "" {
		src = ".."
	}

	src, err := filepath.Abs(src)
	if err != nil {
		t.Fatal(err)
	}

	goRoot := goEnvGOROOT(t)
	coreRoot := filepath.Join(src, "core")

	var projects []string

	err = filepath.WalkDir(coreRoot, func(path string, entry os.DirEntry, err error) error {
		if err != nil {
			return err
		}

		if !entry.IsDir() && strings.HasSuffix(entry.Name(), ".tests.csproj") {
			projects = append(projects, path)
		}

		return nil
	})
	if err != nil {
		t.Fatal(err)
	}

	sort.Strings(projects)

	if len(projects) == 0 {
		t.Fatalf("no *.tests.csproj under %s", coreRoot)
	}

	importPaths := make([]string, 0, len(projects))
	projectFor := map[string]string{}

	for _, project := range projects {
		rel, _ := filepath.Rel(coreRoot, filepath.Dir(project))
		importPath := filepath.ToSlash(rel)
		importPaths = append(importPaths, importPath)
		projectFor[importPath] = project
	}

	tags := resolveBuildTags(false, true, false, false, false, nil)

	config := &packages.Config{
		Mode: packages.NeedName | packages.NeedFiles | packages.NeedCompiledGoFiles | packages.NeedImports |
			packages.NeedDeps | packages.NeedTypes | packages.NeedTypesInfo | packages.NeedSyntax | packages.NeedModule,
		Dir:        filepath.Join(goRoot, "src"),
		Env:        append(os.Environ(), "GOOS="+goos, "GOARCH=amd64", "CGO_ENABLED=1", "GOFLAGS=", "GOWORK=off"),
		BuildFlags: []string{"-tags=" + strings.Join(tags, ",")},
		Tests:      true,
	}

	loaded, err := packages.Load(config, importPaths...)
	if err != nil {
		t.Fatal(err)
	}

	byID := map[string]*packages.Package{}

	for _, pkg := range loaded {
		byID[pkg.ID] = pkg
	}

	options := Options{goRoot: goRoot, go2csPath: src, targetPlatform: goos + "/amd64", buildTags: tags}
	foreign := foreignImplementBasesResolver(options)

	var rows []string
	rows = append(rows, "target\tgoos\tmodel_refs\tadded")

	for _, importPath := range importPaths {
		project := projectFor[importPath]
		projectDir := filepath.Dir(project)

		production := byID[importPath]
		internal := byID[importPath+" ["+importPath+".test]"]
		external := byID[importPath+"_test ["+importPath+".test]"]

		if production == nil {
			rows = append(rows, fmt.Sprintf("%s\t%s\t-\tNOT-LOADED", importPath, goos))
			continue
		}

		if internal == nil {
			internal = production
		}

		data, err := os.ReadFile(project)
		if err != nil {
			t.Fatal(err)
		}

		referenced := []string{importPath}

		for _, match := range footprintRefPattern.FindAllStringSubmatch(string(data), -1) {
			referenced = append(referenced, match[1])
		}

		compiled := map[string]bool{}

		for _, match := range footprintCompilePattern.FindAllStringSubmatch(string(data), -1) {
			compiled[strings.TrimSuffix(filepath.Base(filepath.FromSlash(match[1])), ".cs")] = true
		}

		compileExcluded := map[string]bool{}

		for _, variant := range []*packages.Package{internal, external} {
			if variant == nil {
				continue
			}

			for _, file := range variant.CompiledGoFiles {
				if strings.HasSuffix(file, "_test.go") && !compiled[strings.TrimSuffix(filepath.Base(file), ".go")] {
					compileExcluded[filepath.Clean(file)] = true
				}
			}
		}

		roots := []*packages.Package{production, internal}
		if external != nil {
			roots = append(roots, external)
		}

		added := refsFootprintClosure(roots, compileExcluded, referenced,
			packageImplementBases(platformPackageInfoPath(projectDir, options.targetPlatform)), foreign,
			filepath.Join(projectDir, testPackageInfoFileName))

		sort.Strings(added)
		rows = append(rows, fmt.Sprintf("%s\t%s\t%d\t%s", importPath, goos, len(referenced)-1, strings.Join(added, ",")))
	}

	if err := os.WriteFile(out, []byte(strings.Join(rows, "\n")+"\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	t.Logf("READING %d projects, %s: %s", len(importPaths), goos, out)
}

var (
	footprintRefPattern     = regexp.MustCompile(`ProjectReference Include="\$\(go2csPath\)core/([^"]+)/[^/"]+\.csproj"`)
	footprintCompilePattern = regexp.MustCompile(`<Compile Include="([^"]+\.cs)"`)
)

func goEnvGOROOT(t *testing.T) string {
	t.Helper()

	output, err := runCommandWithTimeout(defaultConvertTimeout, ".", Options{}, "go", "env", "GOROOT")
	if err != nil {
		t.Fatalf("go env GOROOT: %v", err)
	}

	return strings.TrimSpace(output)
}
