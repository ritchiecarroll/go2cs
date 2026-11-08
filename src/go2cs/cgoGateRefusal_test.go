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

// P1: every file excluded because cgo is off, and the excluded file imports "C".
func TestACgoOnlyPackageWithCgoOffIsRefused(t *testing.T) {
	for _, args := range [][]string{nil, {"-cgo=true"}} {
		dir := writeCgoGateModule(t, map[string]string{"add.go": cgoImportSource})
		stderr, output, err := convertCgoGateFixture(t, dir, args, "CGO_ENABLED=0")
		requireCgoRefusal(t, stderr, err, output, "add.go", "cgogatefixture")
	}
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
