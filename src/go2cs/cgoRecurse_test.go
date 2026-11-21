// cgoRecurse_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"io/fs"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// A cgo package against the go.* packages (.5 cgo item, narrow v1, INC 1; C2 posts feb06781d, ca1fd3a4e). Under
// `-recurse -cgo` the module driver loads cgo-ON, and a package whose selected files import "C" converts through the
// -cgo route (planCgoRoute) inside the module conversion, its project referencing the go.* packages under =nuget.
//
// cptr is go-pointer's shape (github.com/mattn/go-pointer v0.0.1, INC 0's measured pick, 4312511a7): cgo-ONLY (no file
// is selected cgo-off, so its importer type-checks against it only from a cgo-on load), a preamble that only includes
// <stdlib.h>, C.malloc and C.free from the C toolchain's default library, and no C type in its exported surface.
const cgoRecurseCptr = `package cptr

// #include <stdlib.h>
import "C"

import "unsafe"

// Alloc allocates one byte of C memory.
func Alloc() unsafe.Pointer { return C.malloc(C.size_t(1)) }

// Free releases what Alloc returned.
func Free(p unsafe.Pointer) { C.free(p) }
`

const cgoRecurseMain = `package main

import (
	"fmt"

	"example.com/cgorecurse/cptr"
)

func main() {
	p := cptr.Alloc()
	fmt.Println(p != nil)
	cptr.Free(p)
}
`

// requireHostCC skips, NOT MEASURED, unless the C compiler go would use runs here: the -cgo route links a probe with it
// to read which library each symbol resolves to (cgoLibraryCapture.go).
func requireHostCC(t *testing.T) {
	t.Helper()

	out, err := exec.Command("go", "env", "CC").Output()

	if err != nil {
		t.Skipf("go env CC failed: %v: NOT MEASURED", err)
	}

	fields, err := splitCgoCommand(strings.TrimSpace(string(out)))

	if err == nil {
		_, err = exec.LookPath(fields[0])
	}

	if err != nil {
		t.Skipf("the C compiler go would use (%q) does not run here: NOT MEASURED", strings.TrimSpace(string(out)))
	}
}

// convertCgoRecurse converts files as module example.com/cgorecurse with the built converter under args, and returns
// its combined output, the output directory and the exit error.
func convertCgoRecurse(t *testing.T, files map[string]string, args ...string) (string, string, error) {
	t.Helper()

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	dir := t.TempDir()
	writeCgoFixture(t, dir, "go.mod", "module example.com/cgorecurse\n\ngo 1.26\n")

	for name, contents := range files {
		if err := os.MkdirAll(filepath.Dir(filepath.Join(dir, name)), 0o755); err != nil {
			t.Fatal(err)
		}

		writeCgoFixture(t, dir, name, contents)
	}

	output := t.TempDir()
	cmd := exec.Command(driverBinary(t), append(append([]string{}, args...), "-go2cspath", treeSrc, dir, output)...)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=")
	combined, err := cmd.CombinedOutput()

	return string(combined), output, err
}

// cgoRecurseWritten reads every file under root whose path contains part, keyed by its path relative to root.
func cgoRecurseWritten(t *testing.T, root, part, ext string) map[string]string {
	t.Helper()

	written := map[string]string{}

	err := filepath.WalkDir(root, func(path string, entry fs.DirEntry, err error) error {
		if err != nil || entry.IsDir() || !strings.HasSuffix(path, ext) {
			return err
		}

		relative, _ := filepath.Rel(root, path)

		if strings.Contains(filepath.ToSlash(relative), part) {
			contents, readErr := os.ReadFile(path)

			if readErr != nil {
				return readErr
			}

			written[filepath.ToSlash(relative)] = string(contents)
		}

		return nil
	})

	if err != nil {
		t.Fatal(err)
	}

	return written
}

// A1: the cgo package converts through the cgo route inside a -recurse=nuget run, and binds what Go binds.
//
// RED before INC 1: "-cgo cannot be combined with -recurse" (loaderEnvironment.go), and without -cgo the module driver
// refuses cptr GO2CS2001 (cgo-only for a cgo-off load) and skips it.
func TestARecurseNugetConversionTakesACgoPackageThroughTheCgoRoute(t *testing.T) {
	requireHostCC(t)

	combined, output, err := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "main.go": cgoRecurseMain},
		"-recurse=nuget", "-cgo=true")

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, combined)
	}

	sources := strings.Builder{}

	for _, text := range cgoRecurseWritten(t, output, "cptr", ".cs") {
		sources.WriteString(text)
	}

	cs := sources.String()
	target := runtime.GOOS + "/" + runtime.GOARCH

	for _, want := range []string{`EntryPoint = "malloc")]`, `EntryPoint = "free")]`, `GoCgoLibraries.Register(`, `"` + target + `"`} {
		if !strings.Contains(cs, want) {
			t.Errorf("cptr's emission lacks %q:\n%s\n--- converter output ---\n%s", want, cs, combined)
		}
	}

	projects := cgoRecurseWritten(t, output, "cptr", ".csproj")

	if len(projects) != 1 {
		t.Fatalf("want exactly one cptr project, have %d: %v\n%s", len(projects), projects, combined)
	}

	for path, project := range projects {
		if !strings.Contains(project, `<PackageReference Include="go.lib"`) {
			t.Errorf("%s does not reference go.lib as a package:\n%s", path, project)
		}
	}
}

// End to end: the same module under -recurse -cgo against this tree (no published go.* needed), built and run, prints
// what Go prints. GO2CS_INLINE_BUILD=1 runs it, with a C compiler for both sides.
func TestARecurseCgoModuleReadsAsGo(t *testing.T) {
	requireHostCC(t)
	dotnet := requireInlineBuild(t)
	files := map[string]string{"cptr/cptr.go": cgoRecurseCptr, "main.go": cgoRecurseMain}

	dir := t.TempDir()
	writeModuleFile(t, filepath.Join(dir, "go.mod"), "module example.com/cgorecurse\n\ngo 1.26\n")

	for file, source := range files {
		writeModuleFile(t, filepath.Join(dir, file), source)
	}

	goRun := exec.Command("go", "run", ".")
	goRun.Dir = dir
	goRun.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1")
	wantBytes, err := goRun.CombinedOutput()

	if err != nil {
		t.Fatalf("go did not run the module: %v\n%s", err, wantBytes)
	}

	want := strings.TrimSpace(strings.ReplaceAll(string(wantBytes), "\r\n", "\n"))
	combined, output, err := convertCgoRecurse(t, files, "-recurse", "-cgo=true")

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, combined)
	}

	out := filepath.Join(output, "src", "example.com", "cgorecurse")

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints: %s; go2cs prints: %s", want, got)

	if got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
	}
}
