// cgoGateRefusal_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The two routes S3 (cgoSelectedRefusal_test.go) does not reach, and the flag that bypassed the one
// gate that did. Measured at master 30ad20e17e on a host with no C compiler (PLAN-cgo-interop.md, the
// 2026-11-08 re-ground, arms P1/P3/P3'):
//
//   - P1: a package whose every file is excluded because cgo is off, and whose excluded file imports
//     "C", printed `error GO2CS1001: build constraints exclude all Go files` and then EXITED 0, having
//     written nothing a build could use: an error diagnostic beside a zero exit, the false-green shape.
//   - P3: with CGO_ENABLED=1 and no C compiler, go list never runs cgo, so the RAW `import "C"` file is
//     paired with its own syntax tree, S3 (which keys on a selected file WITHOUT a tree) stays quiet,
//     and visitImportSpec's gate refused only mid-visit, after a sibling's .cs was already written.
//   - P3': the same load with -cgo=true skipped that gate and EXITED 0 with an emission naming a
//     `C_package` that does not exist.
//
// Each case must now refuse with a non-zero exit, name the file, and write no .cs or .csproj. What
// must stay quiet is unchanged: a cgo file the build deselects for its PLATFORM (not for cgo being
// off), and a package that still has plain sources when cgo is off (S3's ruled quiet case).

package main

import (
	"bytes"
	"go/ast"
	"go/parser"
	"go/token"
	"io/fs"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// convertCgoGateFixture converts the one package in dir through the real command line into a fresh
// output directory and returns its combined output, the output directory and the exit error.
func convertCgoGateFixture(t *testing.T, dir string, args []string, env ...string) (string, string, error) {
	t.Helper()

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	output := t.TempDir()

	var stderr bytes.Buffer
	cmd := exec.Command(driverBinary(t), append(append([]string{"-go2cspath", treeSrc}, args...), dir, output)...)
	cmd.Env = append(append(os.Environ(), "GOWORK=off", "GOFLAGS="), env...)
	cmd.Stdout, cmd.Stderr = &stderr, &stderr
	err = cmd.Run()

	return stderr.String(), output, err
}

// emittedProjectFiles lists every .cs and .csproj under output, relative to it.
func emittedProjectFiles(t *testing.T, output string) []string {
	t.Helper()

	var emitted []string

	_ = filepath.WalkDir(output, func(path string, entry fs.DirEntry, err error) error {
		if err == nil && !entry.IsDir() && (strings.HasSuffix(path, ".cs") || strings.HasSuffix(path, ".csproj")) {
			relative, _ := filepath.Rel(output, path)
			emitted = append(emitted, relative)
		}

		return nil
	})

	return emitted
}

func requireCgoRefusal(t *testing.T, stderr string, err error, output string, names ...string) {
	t.Helper()

	if err == nil {
		t.Errorf("the conversion must refuse with a non-zero exit\noutput:\n%s", stderr)
	}

	for _, name := range names {
		if !strings.Contains(stderr, name) {
			t.Errorf("the refusal does not name %q\noutput:\n%s", name, stderr)
		}
	}

	if !strings.Contains(stderr, "error "+diagnosticCgoRefused) {
		t.Errorf("no canonical %s diagnostic\noutput:\n%s", diagnosticCgoRefused, stderr)
	}

	if emitted := emittedProjectFiles(t, output); len(emitted) > 0 {
		t.Errorf("a refused conversion must write no project file, wrote %v", emitted)
	}
}

func writeCgoGateModule(t *testing.T, files map[string]string) string {
	t.Helper()

	dir := t.TempDir()
	writeCgoFixture(t, dir, "go.mod", "module cgogatefixture\n\ngo 1.24\n")

	for name, contents := range files {
		writeCgoFixture(t, dir, name, contents)
	}

	return dir
}

// P1: every file excluded because cgo is off, and the excluded file imports "C". The same load under -cgo asks for
// the opposite and is refused before loading (TestTheCgoFlagRefusesAnEnvironmentThatSetsCgoOff).
func TestACgoOnlyPackageWithCgoOffIsRefused(t *testing.T) {
	dir := writeCgoGateModule(t, map[string]string{"add.go": cgoImportSource})
	stderr, output, err := convertCgoGateFixture(t, dir, nil, "CGO_ENABLED=0")
	requireCgoRefusal(t, stderr, err, output, "add.go", "cgogatefixture")
}

// The internal/testpty-on-windows shape: every file excluded, one of them imports "C", but the build
// deselects it for its PLATFORM, so it is not a cgo package on this target and stays a quiet skip.
func TestAPlatformExcludedCgoFileStaysAQuietSkip(t *testing.T) {
	dir := writeCgoGateModule(t, map[string]string{
		"p_plan9.go":   "package p\n",
		"cgo_plan9.go": cgoImportSource,
	})

	stderr, output, err := convertCgoGateFixture(t, dir, nil, "CGO_ENABLED=0")

	if err != nil {
		t.Fatalf("a platform-deselected package must stay a quiet skip, got %v\noutput:\n%s", err, stderr)
	}

	if strings.Contains(stderr, diagnosticCgoRefused) {
		t.Errorf("a platform-deselected cgo file was refused as cgo\noutput:\n%s", stderr)
	}

	if emitted := emittedProjectFiles(t, output); len(emitted) > 0 {
		t.Errorf("premise: nothing is selected, so nothing converts; wrote %v", emitted)
	}
}

// P2: cgo off, a plain source left -- S3's ruled quiet case, unchanged.
func TestAMixedPackageWithCgoOffConvertsItsPlainSource(t *testing.T) {
	dir := writeCgoGateModule(t, map[string]string{
		"plain.go": "package p\n\nfunc Twice(a int) int { return a * 2 }\n",
		"add.go":   cgoImportSource,
	})

	stderr, output, err := convertCgoGateFixture(t, dir, nil, "CGO_ENABLED=0")

	if err != nil {
		t.Fatalf("the plain source must convert, got %v\noutput:\n%s", err, stderr)
	}

	if _, statErr := os.Stat(filepath.Join(output, "plain.cs")); statErr != nil {
		t.Errorf("plain.cs was not emitted: %v\noutput:\n%s", statErr, stderr)
	}
}

// requireNoCCompiler skips where cgo can actually run: there the selected file reaches S3 as cgo
// intermediates (TestRealLoadRefusesOnlyASelectedCgoFile), not as its raw self.
func requireNoCCompiler(t *testing.T) {
	t.Helper()

	for _, compiler := range []string{"gcc", "cc", "clang"} {
		if _, err := exec.LookPath(compiler); err == nil {
			t.Skipf("a C compiler (%s) is on PATH: cgo runs, and the raw-file route is not reachable here", compiler)
		}
	}
}

// P3 and P3': CGO_ENABLED=1 with no C compiler pairs the raw `import "C"` file with its own tree.
func TestARawCgoSourceIsRefusedBeforeAnyOutputWhateverTheFlag(t *testing.T) {
	requireNoCCompiler(t)

	for _, args := range [][]string{nil, {"-cgo=true"}} {
		dir := writeCgoGateModule(t, map[string]string{
			"plain.go": "package p\n\nfunc Twice(a int) int { return a * 2 }\n",
			"add.go":   cgoImportSource,
		})

		stderr, output, err := convertCgoGateFixture(t, dir, args, "CGO_ENABLED=1")
		requireCgoRefusal(t, stderr, err, output, "add.go", "cgogatefixture")
	}
}

// The darwin net/internal/cgotest shape, at unit level: no file selected, the ignored file imports "C"
// and its constraint needs only cgo. Outside GOROOT it is refused; inside GOROOT the stdlib's ruled
// CGO_ENABLED=0 convert keeps it a quiet skip.
func TestACgoOnlyPackageIsRefusedOutsideGorootOnly(t *testing.T) {
	goRoot := t.TempDir()
	dir := filepath.Join(goRoot, "src", "net", "internal", "cgotest")

	if err := os.MkdirAll(dir, 0o755); err != nil {
		t.Fatal(err)
	}

	ignored := writeCgoFixture(t, dir, "resstate.go", "//go:build cgo\n\n"+cgoImportSource)
	pkg := &packages.Package{PkgPath: "net/internal/cgotest", Dir: dir, IgnoredFiles: []string{ignored}}

	if err := refuseCgoOnlyPackage(pkg, "linux/amd64", nil, dir, ""); err == nil || !strings.Contains(err.Error(), "resstate.go") {
		t.Errorf("outside GOROOT a cgo-only package must be refused by name, got %v", err)
	}

	if err := refuseCgoOnlyPackage(pkg, "linux/amd64", nil, dir, goRoot); err != nil {
		t.Errorf("a GOROOT package converts at the stdlib's ruled CGO_ENABLED=0 and must stay quiet, got %v", err)
	}
}

// The same route at unit level, host-independent: the selected file IS paired, with its own tree, and
// that tree imports "C".
func TestARawCgoSourcePairedWithItsOwnTreeIsRefusedByName(t *testing.T) {
	dir := t.TempDir()

	plainPath := writeCgoFixture(t, dir, "plain.go", "package p\n")
	cgoPath := writeCgoFixture(t, dir, "add.go", cgoImportSource)

	fset := token.NewFileSet()
	plain, err := parser.ParseFile(fset, plainPath, nil, parser.ParseComments)

	if err != nil {
		t.Fatal(err)
	}

	raw, err := parser.ParseFile(fset, cgoPath, nil, parser.ParseComments)

	if err != nil {
		t.Fatal(err)
	}

	pkg := &packages.Package{
		PkgPath:         "p",
		Dir:             dir,
		GoFiles:         []string{plainPath, cgoPath},
		CompiledGoFiles: []string{plainPath, cgoPath},
		Syntax:          []*ast.File{plain, raw},
		Fset:            fset,
	}

	paired, _ := syntaxSourceFiles(pkg)

	if len(paired) != 2 {
		t.Fatalf("premise: both files pair with their own trees, got %d", len(paired))
	}

	refusal := refuseSelectedCgoSources(pkg, paired)

	if refusal == nil || !strings.Contains(refusal.Error(), "add.go") {
		t.Fatalf("a raw `import \"C\"` source paired with its own tree must be refused by name, got %v", refusal)
	}

	if strings.Contains(refusal.Error(), "plain.go") {
		t.Errorf("the refusal names a plain source: %v", refusal)
	}
}

// requireCCompiler skips where cgo cannot run: go list hands a cgo source to cmd/cgo only where it finds a C compiler.
func requireCCompiler(t *testing.T) {
	t.Helper()

	for _, compiler := range []string{"gcc", "cc", "clang"} {
		if _, err := exec.LookPath(compiler); err == nil {
			return
		}
	}

	t.Skip("no C compiler: cmd/cgo cannot run here, so a -cgo load cannot recognize anything (Inconclusive, not a pass)")
}

// -cgo makes the driver's own load cgo-on, with the environment leaving CGO_ENABLED unset: without the flag the loader
// pins cgo off, add.go is deselected and plain.go converts with exit 0. With no C compiler cmd/cgo still does not run,
// and the refusal says so.
func TestTheCgoFlagLoadsCgoOnAndSaysWhenCmdCgoDidNotRun(t *testing.T) {
	requireNoCCompiler(t)

	dir := writeCgoGateModule(t, map[string]string{
		"plain.go": "package p\n\nfunc Twice(a int) int { return a * 2 }\n",
		"add.go":   cgoImportSource,
	})

	stderr, output, err := convertCgoGateFixture(t, dir, []string{"-cgo=true"}, "CGO_ENABLED=")
	requireCgoRefusal(t, stderr, err, output, "add.go", "cgogatefixture",
		"under -cgo: cmd/cgo did not run for this load (no C compiler), so nothing was recognized")
}

// -cgo beside an environment asking for the opposite load is refused before anything loads: for P1's cgo-only shape,
// which a cgo-off load refuses anyway, and for a mixed package, whose cgo-off load would convert its plain file.
func TestTheCgoFlagRefusesAnEnvironmentThatSetsCgoOff(t *testing.T) {
	for _, files := range []map[string]string{
		{"add.go": cgoImportSource},
		{"plain.go": "package p\n\nfunc Twice(a int) int { return a * 2 }\n", "add.go": cgoImportSource},
	} {
		dir := writeCgoGateModule(t, files)
		stderr, output, err := convertCgoGateFixture(t, dir, []string{"-cgo=true"}, "CGO_ENABLED=0")

		if err == nil {
			t.Errorf("-cgo with CGO_ENABLED=0 must refuse with a non-zero exit\noutput:\n%s", stderr)
		}

		if !strings.Contains(stderr, "-cgo asks for a cgo-on load, and the environment sets CGO_ENABLED=0") {
			t.Errorf("the refusal does not name the contradiction\noutput:\n%s", stderr)
		}

		if emitted := emittedProjectFiles(t, output); len(emitted) > 0 {
			t.Errorf("a refused conversion must write no project file, wrote %v", emitted)
		}
	}
}

// Phase 1's EXIT GATE (PLAN-cgo-interop §6): on a host with a C compiler, the fixture's own source, loaded through the
// real driver under -cgo, has every C name recognized and its rewritten tree mapped back to it, and still emits
// nothing. The summary is the checked-in fixture's (TestCgoRecognitionSummaryCountsTheFixtureByKindAndNamesEachTreeByRole),
// asserted WHOLE to the end of its line: it names trees by role, so the build-cache names go list hands back on a real
// host cannot reach it.
func TestTheCgoFlagRecognizesTheFixtureAndStillRefuses(t *testing.T) {
	requireCCompiler(t)

	source, err := os.ReadFile(filepath.Join(cgoFixtureDir, "fixture.go"))

	if err != nil {
		t.Fatal(err)
	}

	dir := writeCgoGateModule(t, map[string]string{"fixture.go": string(source)})

	stderr, output, err := convertCgoGateFixture(t, dir, []string{"-cgo=true"}, "CGO_ENABLED=")
	logCgoRecognizedNames(t, dir)
	requireCgoRefusal(t, stderr, err, output, "fixture.go", "cgogatefixture",
		"under -cgo: "+cgoFixtureSummaryCounts+"; "+cgoFixtureSummaryRoles+"\n")

	// The reading itself, so a passing run on a hosted runner records what the host's cmd/cgo produced.
	if _, recognized, found := strings.Cut(stderr, "under -cgo: "); found {
		t.Logf("READING %s", strings.TrimSpace(strings.SplitN(recognized, "\n", 2)[0]))
	}
}

// A mixed package on a host with a C compiler: go list hands the plain file over at its own path, untouched by cmd/cgo,
// so the summary names it as plain Go source rather than counting it among cmd/cgo's own files.
func TestTheCgoFlagNamesAMixedPackagesPlainFileAsPlainSource(t *testing.T) {
	requireCCompiler(t)

	source, err := os.ReadFile(filepath.Join(cgoFixtureDir, "fixture.go"))

	if err != nil {
		t.Fatal(err)
	}

	dir := writeCgoGateModule(t, map[string]string{
		"fixture.go": string(source),
		"plain.go":   "package cgofixture\n\nfunc Twice(a int) int { return a * 2 }\n",
	})

	stderr, output, err := convertCgoGateFixture(t, dir, []string{"-cgo=true"}, "CGO_ENABLED=")
	requireCgoRefusal(t, stderr, err, output, "fixture.go", "cgogatefixture", "under -cgo: "+cgoFixtureSummaryCounts+
		"; 1 generated file -> fixture.go; plain.go: plain Go source; 2 generated by cmd/cgo with no source\n")
}

// logCgoRecognizedNames makes the load -cgo makes, in-process, and logs cgoDeclarations' names per kind. The refusal
// carries counts only, so a host whose cmd/cgo declares a different set (another libc's typedef chain) would red the
// exit gate without saying WHICH declaration moved; these lines say it, on a pass and on a red alike.
func logCgoRecognizedNames(t *testing.T, dir string) {
	t.Helper()

	pkgs, err := packages.Load(&packages.Config{
		Mode: packages.LoadAllSyntax,
		Dir:  dir,
		Env:  append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1"),
	}, ".")

	if err != nil || len(pkgs) != 1 || pkgs[0].Types == nil {
		t.Logf("READING names: not loaded (%d package(s), %v)", len(pkgs), err)
		return
	}

	names := map[cgoDeclKind][]string{}

	for _, declaration := range cgoDeclarations(pkgs[0].Types) {
		names[declaration.kind] = append(names[declaration.kind], declaration.cName)
	}

	for _, entry := range cgoKindLabels {
		if len(names[entry.kind]) > 0 {
			t.Logf("READING %s: %s", entry.label, strings.Join(names[entry.kind], ", "))
		}
	}
}
