// cgoRecurseSubstitution_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// P3's INC 1 review, finding 1 (MUST; COORD's ruling): under -recurse=nuget a module SUBSTITUTED by a published package
// leaves the convert-set, so no per-package rule of the seat reads it, while the closure type-checks cgo-ON and the
// published package was converted cgo-OFF. A //go:build cgo / !cgo pair in it would type-check against the cgo-on file
// and bind the published cgo-off one: silently, with the same exported names.
//
// The check is driven here against a REAL closure: example.com/dep (its q package holds the pair) is loaded through the
// module driver's own loadClosure and partition, and named substituted. Producing a substitution end to end needs a
// feed whose published package qualifies (nugetSubstitution.go); the decision this row holds is the check's.
func cgoSplitClosure(t *testing.T, cgo bool, pair bool) (*ModuleConverter, string) {
	return cgoSplitClosureTagged(t, cgo, pair, false)
}

// cgoSplitClosureTagged is cgoSplitClosure; tagged puts the pair under the run's own build tag (-tags cgosplittag) and
// adds a plain file for the build without it, so the split exists only for a `go list` that carries the run's tags.
func cgoSplitClosureTagged(t *testing.T, cgo, pair, tagged bool) (*ModuleConverter, string) {
	t.Helper()
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	depDir := filepath.Join(root, "dep")

	writeModuleFile(t, filepath.Join(depDir, "go.mod"), "module example.com/dep\n\ngo 1.23\n")

	switch {
	case tagged:
		writeModuleFile(t, filepath.Join(depDir, "q", "q_cgo.go"), strings.Replace(cgoRecursePairOn, "//go:build cgo", "//go:build cgo && cgosplittag", 1))
		writeModuleFile(t, filepath.Join(depDir, "q", "q_nocgo.go"), strings.Replace(cgoRecursePairOff, "//go:build !cgo", "//go:build !cgo && cgosplittag", 1))
		writeModuleFile(t, filepath.Join(depDir, "q", "q_plain.go"), "//go:build !cgosplittag\n\npackage pair\n\n// Mode names the build.\nfunc Mode() string { return \"untagged\" }\n")
	case pair:
		writeModuleFile(t, filepath.Join(depDir, "q", "q_cgo.go"), cgoRecursePairOn)
		writeModuleFile(t, filepath.Join(depDir, "q", "q_nocgo.go"), cgoRecursePairOff)
	default:
		writeModuleFile(t, filepath.Join(depDir, "q", "q.go"), "package pair\n\n// Mode names the build.\nfunc Mode() string { return \"one file\" }\n")
	}

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/app\n\ngo 1.23\n\nrequire example.com/dep v0.0.0\n\nreplace example.com/dep => ../dep\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\nimport (\n\t\"fmt\"\n\n\tq \"example.com/dep/q\"\n)\n\nfunc main() {\n\tfmt.Println(q.Mode())\n}\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	build.Default.GOROOT = goRoot

	options := Options{goRoot: goRoot, goPath: build.Default.GOPATH, go2csPath: filepath.Join(root, "out"), recurse: true, nugetRefs: true,
		parseCgoTargets: cgo, targetPlatform: runtime.GOOS + "/" + runtime.GOARCH, indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}

	if tagged {
		options.buildTags = []string{"cgosplittag"}
	}

	m := NewModuleConverter(options)

	closure, err := m.loadClosure(appDir)

	if err != nil {
		t.Fatal(err)
	}

	m.partition(closure)

	return m, appDir
}

func TestASubstitutedModuleWithACgoSplitPackageIsRefusedUnderRecurseCgo(t *testing.T) {
	m, appDir := cgoSplitClosure(t, true, true)
	err := m.refuseCgoSplitSubstitutions(appDir, map[string]*nugetSubstitution{"example.com/dep": {module: "example.com/dep", nugetID: "dep.nuget"}})

	if err == nil {
		t.Fatal("a substituted module whose package q Go builds from different files with cgo on was not refused")
	}

	for _, want := range []string{"example.com/dep", "example.com/dep/q", "dep.nuget"} {
		if !strings.Contains(err.Error(), want) {
			t.Errorf("the refusal does not name %q: %v", want, err)
		}
	}
}

// The controls: the same split without -cgo (the closure is cgo-off, as the published package is), and a substituted
// module whose package has one file for both builds.
func TestASubstitutedModuleIsKeptWhereTheCgoBuildsAgree(t *testing.T) {
	for _, c := range []struct {
		name      string
		cgo, pair bool
	}{
		{"without -cgo", false, true},
		{"no split", true, false},
	} {
		t.Run(c.name, func(t *testing.T) {
			m, appDir := cgoSplitClosure(t, c.cgo, c.pair)

			if err := m.refuseCgoSplitSubstitutions(appDir, map[string]*nugetSubstitution{"example.com/dep": {module: "example.com/dep", nugetID: "dep.nuget"}}); err != nil {
				t.Errorf("refused: %v", err)
			}
		})
	}
}

// P3's INC 1 CLEAR, one SHOULD (COORD's ruling): the check's `go list` carries the run's build flags, as the closure
// load does. Here the split exists only under the run's own tag, and the untagged file is the same both ways, so a
// listing without the run's tags reads no difference.
func TestASubstitutedModuleWhoseCgoSplitIsUnderTheRunsTagIsRefused(t *testing.T) {
	m, appDir := cgoSplitClosureTagged(t, true, false, true)
	err := m.refuseCgoSplitSubstitutions(appDir, map[string]*nugetSubstitution{"example.com/dep": {module: "example.com/dep", nugetID: "dep.nuget"}})

	if err == nil || !strings.Contains(err.Error(), "example.com/dep/q") {
		t.Fatalf("a substituted module whose package splits on cgo under the run's tag was not refused: %v", err)
	}
}
