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

	return convertCgoRecurseEnv(t, files, nil, args...)
}

// convertCgoRecurseEnv is convertCgoRecurse with extra environment settings, last so they win.
func convertCgoRecurseEnv(t *testing.T, files map[string]string, extraEnv []string, args ...string) (string, string, error) {
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
	cmd.Env = append(append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED="), extraEnv...)
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

// R5 (COORD's ruling on P3's INC 1 review, option b). The go.* packages are the standard library converted cgo-OFF
// (the corpus ruling). For most std packages Go builds differently with cgo on, that is a DISCLOSED divergence and not a
// refusal: a converted package importing net binds the cgo-off build, exactly as Go itself does under CGO_ENABLED=0 or
// the netgo tag, the pure-Go resolver (docs/Limitations.md). Refused by name (GO2CS2001), the package skipped and the
// rest of the module converted, are only the packages whose cgo-off build is NOT a working Go build of the same API:
// runtime/cgo (R5c) and plugin. The divergence set is still derived per target from `go list std` both ways.
const cgoRecurseNetUser = `package netuser

import "net"

// Port resolves a literal address, so no lookup leaves the host.
func Port() int {
	addr, err := net.ResolveTCPAddr("tcp", "127.0.0.1:8080")

	if err != nil {
		return -1
	}

	return addr.Port
}
`

const cgoRecurseHandleUser = `package handleuser

import "runtime/cgo"

// Round stores v and reads it back through a cgo.Handle.
func Round(v int) int {
	h := cgo.NewHandle(v)
	defer h.Delete()
	return h.Value().(int)
}
`

const cgoRecursePluginUser = `package pluginuser

import "plugin"

// Open opens a plugin by path.
func Open(path string) error {
	_, err := plugin.Open(path)
	return err
}
`

// cgoRecurseConverted reports whether the conversion under output wrote a .cs file for the package directory part.
func cgoRecurseConverted(t *testing.T, output, part string) bool {
	t.Helper()

	return len(cgoRecurseWritten(t, output, part+"/", ".cs")) > 0
}

// The refusal skips the one package and not the module: cptr and main convert in the same run.
func requireCgoRecurseRestConverted(t *testing.T, output, combined string) {
	t.Helper()

	if !cgoRecurseConverted(t, output, "cptr") {
		t.Errorf("cptr was not converted in the run that refused another package:\n%s", combined)
	}

	if len(cgoRecurseWritten(t, output, "cgorecurse/main.cs", "")) == 0 && len(cgoRecurseWritten(t, output, "main.cs", ".cs")) == 0 {
		t.Errorf("main was not converted in the run that refused another package:\n%s", combined)
	}
}

func TestARecurseCgoConversionConvertsAPackageThatImportsNet(t *testing.T) {
	requireHostCC(t)

	combined, output, err := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "netuser/netuser.go": cgoRecurseNetUser, "main.go": cgoRecurseMain},
		"-recurse=nuget", "-cgo=true")

	if err != nil {
		t.Fatalf("a module whose package imports net (a disclosed divergence) did not convert: %v\n%s", err, combined)
	}

	if !cgoRecurseConverted(t, output, "netuser") {
		t.Errorf("netuser was not converted:\n%s", combined)
	}

	requireCgoRecurseRestConverted(t, output, combined)
}

func TestARecurseCgoConversionRefusesAnImportOfRuntimeCgo(t *testing.T) {
	requireHostCC(t)

	combined, output, _ := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "handleuser/handleuser.go": cgoRecurseHandleUser, "main.go": cgoRecurseMain},
		"-recurse=nuget", "-cgo=true")

	for _, want := range []string{"GO2CS2001", "example.com/cgorecurse/handleuser", `"runtime/cgo"`} {
		if !strings.Contains(combined, want) {
			t.Errorf("the refusal does not name %q:\n%s", want, combined)
		}
	}

	if cgoRecurseConverted(t, output, "handleuser") {
		t.Errorf("the refused handleuser was written:\n%s", combined)
	}

	requireCgoRecurseRestConverted(t, output, combined)
}

func TestARecurseCgoConversionRefusesAnImportOfPlugin(t *testing.T) {
	requireHostCC(t)

	if runtime.GOOS == "windows" {
		t.Skip("plugin is built from the same files cgo-on and cgo-off on windows; the row is linux's and darwin's: NOT MEASURED")
	}

	combined, output, _ := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "pluginuser/pluginuser.go": cgoRecursePluginUser, "main.go": cgoRecurseMain},
		"-recurse=nuget", "-cgo=true")

	for _, want := range []string{"GO2CS2001", "example.com/cgorecurse/pluginuser", `"plugin"`} {
		if !strings.Contains(combined, want) {
			t.Errorf("the refusal does not name %q:\n%s", want, combined)
		}
	}

	requireCgoRecurseRestConverted(t, output, combined)
}

// R6, an ASSERTION in this shape: every package the run CONVERTS loads cgo-on, so a file pair split by the cgo build tag
// with no import "C" converts its cgo-on file, as Go's cgo build does. A package the run does NOT convert -- one of a
// module SUBSTITUTED by a published package under -recurse=nuget, converted cgo-off -- is held by the substitution
// refusal instead (cgoRecurseSubstitution.go; P3's INC 1 review, finding 1).
const cgoRecursePairOn = `//go:build cgo

package pair

// Mode names the build that selected this file.
func Mode() string { return "built with cgo on" }
`

const cgoRecursePairOff = `//go:build !cgo

package pair

// Mode names the build that selected this file.
func Mode() string { return "built with cgo off" }
`

func TestARecurseCgoConversionConvertsATagSplitPairsCgoOnFile(t *testing.T) {
	requireHostCC(t)

	combined, output, err := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "pair/pair_cgo.go": cgoRecursePairOn,
		"pair/pair_nocgo.go": cgoRecursePairOff, "main.go": cgoRecurseMain}, "-recurse=nuget", "-cgo=true")

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, combined)
	}

	sources := strings.Builder{}

	for _, text := range cgoRecurseWritten(t, output, "pair", ".cs") {
		sources.WriteString(text)
	}

	if cs := sources.String(); !strings.Contains(cs, "built with cgo on") || strings.Contains(cs, "built with cgo off") {
		t.Errorf("pair does not convert exactly its cgo-on file:\n%s", cs)
	}
}

// INC 2a (C2 e167eb023): -cgo for a target other than the host's. The library capture links a probe with the C compiler,
// so a cross-target conversion needs that target's compiler, named explicitly in the environment (CC). Without one it is
// R3, refused by name; the host's own target is unchanged.
func cgoRecurseOtherTarget() string {
	if runtime.GOOS+"/"+runtime.GOARCH == "windows/amd64" {
		return "linux/amd64"
	}

	return "windows/amd64"
}

func TestARecurseCgoConversionForAnotherTargetWithoutANamedCCIsRefused(t *testing.T) {
	requireHostCC(t)
	target := cgoRecurseOtherTarget()

	combined, _, err := convertCgoRecurseEnv(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "main.go": cgoRecurseMain}, []string{"CC="},
		"-recurse=nuget", "-cgo=true", "-platforms", target)

	if err == nil {
		t.Fatalf("a -cgo conversion for %s with no C compiler named for it converted:\n%s", target, combined)
	}

	for _, want := range []string{"R3", target, "CC"} {
		if !strings.Contains(combined, want) {
			t.Errorf("the refusal does not name %q:\n%s", want, combined)
		}
	}
}

// X1, the cross-target conversion with a named C compiler. HOSTED: GO2CS_CGO_CROSS_TARGET names the target and
// GO2CS_CGO_CROSS_CC its compiler (the converter-cgo stage's pinned zig, `zig cc -target <triple>`); without both the row
// is NOT MEASURED.
func TestARecurseCgoConversionForAnotherTargetWithANamedCCBindsThatTarget(t *testing.T) {
	target, cc := os.Getenv("GO2CS_CGO_CROSS_TARGET"), os.Getenv("GO2CS_CGO_CROSS_CC")

	if target == "" || cc == "" {
		t.Skip("GO2CS_CGO_CROSS_TARGET and GO2CS_CGO_CROSS_CC name no cross target and compiler here: NOT MEASURED")
	}

	combined, output, err := convertCgoRecurseEnv(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "main.go": cgoRecurseMain}, []string{"CC=" + cc},
		"-recurse=nuget", "-cgo=true", "-platforms", target)

	if err != nil {
		t.Fatalf("the cross-target conversion for %s failed: %v\n%s", target, err, combined)
	}

	sources := strings.Builder{}

	for _, text := range cgoRecurseWritten(t, output, "cptr", ".cs") {
		sources.WriteString(text)
	}

	cs := sources.String()

	for _, want := range []string{`EntryPoint = "malloc")]`, `GoCgoLibraries.Register(`, `"` + target + `"`} {
		if !strings.Contains(cs, want) {
			t.Errorf("cptr's %s emission lacks %q:\n%s", target, want, cs)
		}
	}

	t.Logf("READING %s via %q: %d bytes of cptr C#", target, cc, len(cs))
}
