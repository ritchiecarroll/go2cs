// incrementalWrites_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards incremental .cs writes: re-converting an UNCHANGED package must leave every emitted .cs untouched, so its
// timestamp holds and MSBuild does not recompile the project. Every other output the converter writes (project files,
// the solution, readmes, embeds, the platform merge) already skips an identical write through needToWriteFile; the
// converted .cs went through writeOutputFile, which rewrote it unconditionally.

package main

import (
	"errors"
	"go/build"
	"io/fs"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
	"time"

	"github.com/ritchiecarroll/hashset"
	"golang.org/x/tools/go/packages"
)

// incrementalFixtureStamp is the instant every emitted .cs is set to between the two conversions. A FIXED old instant,
// not "the first run's mtime": a rewrite within the filesystem's timestamp resolution could otherwise read unchanged.
var incrementalFixtureStamp = time.Date(2001, 1, 1, 0, 0, 0, 0, time.UTC)

func incrementalFixture(t *testing.T) (pkgDir, outDir string, options Options) {
	t.Helper()

	root := t.TempDir()
	pkgDir = filepath.Join(root, "incr")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/incr\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(pkgDir, "a.go"), `package incr

type Point struct{ X, Y int }

func (p Point) Sum() int { return p.X + p.Y }
`)
	writeModuleFile(t, filepath.Join(pkgDir, "b.go"), `package incr

func Scale(p Point, k int) Point { return Point{p.X * k, p.Y * k} }
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options = Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	return pkgDir, filepath.Join(root, "out"), options
}

// stampEmittedSources sets every .cs under dir to incrementalFixtureStamp and returns their paths.
func stampEmittedSources(t *testing.T, dir string) []string {
	t.Helper()

	var sources []string

	err := filepath.WalkDir(dir, func(path string, entry fs.DirEntry, err error) error {
		if err != nil || entry.IsDir() || !strings.HasSuffix(path, ".cs") {
			return err
		}

		sources = append(sources, path)
		return os.Chtimes(path, incrementalFixtureStamp, incrementalFixtureStamp)
	})

	if err != nil {
		t.Fatalf("stamping the emitted sources: %v", err)
	}

	if len(sources) < 3 {
		t.Fatalf("the fixture emitted %d .cs files; expected a.cs, b.cs and package_info.cs at least: %v", len(sources), sources)
	}

	return sources
}

// rewrittenSources lists the sources whose timestamp moved off incrementalFixtureStamp.
func rewrittenSources(t *testing.T, sources []string) []string {
	t.Helper()

	var moved []string

	for _, path := range sources {
		info, err := os.Stat(path)

		if err != nil {
			t.Fatalf("stat %s: %v", path, err)
		}

		if !info.ModTime().Equal(incrementalFixtureStamp) {
			moved = append(moved, filepath.Base(path))
		}
	}

	return moved
}

func TestUnchangedReconversionLeavesEverySourceUntouched(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	pkgDir, outDir, options := incrementalFixture(t)

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("first conversion: %v", err)
	}

	sources := stampEmittedSources(t, outDir)

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("second conversion: %v", err)
	}

	if moved := rewrittenSources(t, sources); len(moved) > 0 {
		t.Errorf("an unchanged re-conversion rewrote %d of %d .cs files (their timestamps moved, so MSBuild recompiles): %v", len(moved), len(sources), moved)
	}
}

// The platform census tells "emitted by this run" from "seeded" by modification time against a sentinel it stamps
// (platformCensus.go snapshotConvertedRoot; h8-comparand.sh reads its root the same way), so a census conversion must
// still rewrite every source: the guard against incremental writes reaching it.
func TestCensusConversionStillRewritesEverySource(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	pkgDir, outDir, options := incrementalFixture(t)
	options.alwaysWriteSources = true

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("first conversion: %v", err)
	}

	sources := stampEmittedSources(t, outDir)

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("second conversion: %v", err)
	}

	if moved := rewrittenSources(t, sources); len(moved) != len(sources) {
		t.Errorf("a census conversion must rewrite every source; %d of %d moved: %v", len(moved), len(sources), moved)
	}
}

// A source still holding DEFERRED MARKERS when it is written (a pointer-to-interface cast names its adapter class
// through a «ADAPTER:…» marker, resolved once the package's implementation records are final) is rewritten by the
// marker pass, so the byte compare alone cannot keep its timestamp; the resolved file is compared with what it held
// before, and an unchanged one gets its time back.
func TestUnchangedReconversionKeepsMarkerBearingSourcesUntouched(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	pkgDir, outDir, options := incrementalFixture(t)

	writeModuleFile(t, filepath.Join(pkgDir, "c.go"), `package incr

import "fmt"

type Named struct{ name string }

func (n *Named) String() string { return n.name }

func Describe() string {
	var s fmt.Stringer = &Named{"n"}
	return s.String()
}
`)

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("first conversion: %v", err)
	}

	sources := stampEmittedSources(t, outDir)
	restoredBefore := markedSourcesRestored.Load()

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("second conversion: %v", err)
	}

	if markedSourcesRestored.Load() == restoredBefore {
		t.Fatalf("the fixture's second conversion restored no marker-bearing source: it no longer exercises the marker path")
	}

	if moved := rewrittenSources(t, sources); len(moved) > 0 {
		t.Errorf("an unchanged re-conversion rewrote %d of %d .cs files: %v", len(moved), len(sources), moved)
	}
}

// A `-tests` conversion RE-SEEDS its metadata anchors on every run (package_test_info.cs is written from a seed, and
// package_info_internal_test.cs is removed and re-seeded) before merging the variants' records into them, so the
// merge's byte compare reads the seed, never the previous run's file. Measured on sweep row io: every re-run rewrote
// exactly these two, byte-identical. Their previous state is remembered before the re-seed and an unchanged one gets
// its time back.
func TestUnchangedTestsReconversionKeepsMetadataAnchorsUntouched(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	// A census conversion earlier in this process leaves the setting on; processConversion is what sets it.
	alwaysWriteSources.Store(false)

	// A MIXED suite, as io's is: the internal bridge anchor is written only beside an external variant.
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod":              "module example/mixed\n\ngo 1.23\n",
		"shapes/shape.go":     "package shapes\n\nfunc Area(w, h int) int { return w * h }\n",
		"shapes/area_test.go": "package shapes\n\nimport \"testing\"\n\nfunc TestArea(t *testing.T) {\n\tif Area(2, 3) != 6 {\n\t\tt.Fatal(\"area\")\n\t}\n}\n",
		"shapes/ext_test.go":  "package shapes_test\n\nimport (\n\t\"testing\"\n\n\t\"example/mixed/shapes\"\n)\n\nfunc TestAreaExternal(t *testing.T) {\n\tif shapes.Area(1, 1) != 1 {\n\t\tt.Fatal(\"area\")\n\t}\n}\n",
	})

	inputPath := filepath.Join(dir, "shapes")
	outputPath := t.TempDir()

	convert := func() {
		t.Helper()

		loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: inputPath, Tests: true}, ".")
		if err != nil {
			t.Fatal(err)
		}

		production := findProductionPackage(loaded, inputPath)
		internal, external := findTestVariants(loaded, production)

		if model := selectTestProjectModel(internal, external); model != testProjectWhiteboxReference {
			t.Fatalf("fixture model = %v, want whitebox-reference (the model that seeds both anchors)", model)
		}

		resetPackageState(&packages.Package{})
		packageNamespace = "go"

		options := Options{
			indentSpaces:        4,
			preferVarDecl:       true,
			useChannelOperators: true,
			convertTests:        true,
			targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
			testPackagePath:     production.PkgPath,
			testPackageName:     production.Name,
		}

		if _, err = convertTestVariants(testProjectWhiteboxReference, production, internal, external,
			selectCompileExcludedTestFiles(internal, external), inputPath, outputPath, "go",
			hashset.NewHashSet(supportedTestCapabilities()), options); err != nil {
			t.Fatalf("convertTestVariants: %v", err)
		}
	}

	convert()
	sources := stampEmittedSources(t, outputPath)

	for _, anchor := range []string{testPackageInfoFileName, internalTestPackageInfoFileName} {
		if _, err := os.Stat(filepath.Join(outputPath, anchor)); err != nil {
			t.Fatalf("the fixture no longer emits %s, so it cannot guard its re-seed: %v", anchor, err)
		}
	}

	convert()

	if moved := rewrittenSources(t, sources); len(moved) > 0 {
		t.Errorf("an unchanged -tests re-conversion rewrote %d of %d .cs files: %v", len(moved), len(sources), moved)
	}
}

// A suite whose external tests record metadata that must anchor to a production type takes the RECOMPILE-MODEL
// FALLBACK (processTestConversion): the reference-model attempt converts and writes first, then fails with
// errProductionAnchoredRecords, and the conversion is re-run under the recompile model over the files that attempt
// already rewrote (and over package_info_external_test.cs, which it removed). Measured on the -tests footprint: the
// four packages in the tree with that anchor (crypto/ecdh, crypto/sha3, net/netip, text/tabwriter) moved 15 .cs on an
// unchanged re-conversion, byte-identical.
func TestUnchangedTestsReconversionThroughRecompileFallbackKeepsSourcesUntouched(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	pkgDir, outDir, options := incrementalFixture(t)

	writeModuleFile(t, filepath.Join(pkgDir, "c.go"), "package incr\n\nfunc (p Point) String() string { return \"p\" }\n")
	writeModuleFile(t, filepath.Join(pkgDir, "ext_test.go"), `package incr_test

import (
	"fmt"
	"testing"

	"example.com/incr"
)

type local struct{}

func (*local) String() string { return "l" }

func TestString(t *testing.T) {
	var s fmt.Stringer = &incr.Point{X: 1, Y: 2}
	var l fmt.Stringer = &local{}
	if s.String() != "p" || l.String() != "l" {
		t.Fatal("string")
	}
}
`)

	// What main and processTestConversion do, in that order: the production conversion, then the two models.
	convert := func() {
		t.Helper()

		if err := processConversion(pkgDir, true, outDir, options); err != nil {
			t.Fatalf("production conversion: %v", err)
		}

		loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: pkgDir, Tests: true}, ".")
		if err != nil {
			t.Fatal(err)
		}

		production := findProductionPackage(loaded, pkgDir)
		internal, external := findTestVariants(loaded, production)

		if model := selectTestProjectModel(internal, external); model != testProjectReference {
			t.Fatalf("fixture model = %v, want reference (the model that falls back)", model)
		}

		testOptions := options
		testOptions.convertTests = true
		testOptions.testPackagePath = production.PkgPath
		testOptions.testPackageName = production.Name

		_, projectNamespace := getProjectName(pkgDir, testOptions)
		compileExcluded := selectCompileExcludedTestFiles(internal, external)
		supported := hashset.NewHashSet(supportedTestCapabilities())

		_, err = convertTestVariants(testProjectReference, production, internal, external, compileExcluded, pkgDir, outDir, projectNamespace, supported, testOptions)

		if !errors.Is(err, errProductionAnchoredRecords) {
			t.Fatalf("the fixture no longer takes the recompile fallback (reference attempt: %v)", err)
		}

		if _, err = convertTestVariants(testProjectRecompile, production, internal, external, compileExcluded, pkgDir, outDir, projectNamespace, supported, testOptions); err != nil {
			t.Fatalf("recompile conversion: %v", err)
		}
	}

	convert()
	sources := stampEmittedSources(t, outDir)

	if _, err := os.Stat(filepath.Join(outDir, externalTestPackageInfoFileName)); err != nil {
		t.Fatalf("the fixture no longer emits %s, so it cannot guard its removal: %v", externalTestPackageInfoFileName, err)
	}

	convert()

	if moved := rewrittenSources(t, sources); len(moved) > 0 {
		t.Errorf("an unchanged re-conversion through the fallback rewrote %d of %d .cs files: %v", len(moved), len(sources), moved)
	}
}
