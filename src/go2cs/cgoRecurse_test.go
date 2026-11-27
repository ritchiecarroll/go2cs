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

	for _, want := range []string{"this -cgo conversion", target, "CC"} {
		if !strings.Contains(combined, want) {
			t.Errorf("the refusal does not name %q:\n%s", want, combined)
		}
	}
}

// R3b (P3's INC 2 shape read, MUST 1): a C compiler that is NAMED but compiles for the host does not make bindings for
// another target. cmd/cgo would size C types with it (C.long is 8 bytes on linux, 4 on windows) under the other target's
// Register string, and nothing links at conversion time to notice. The route asks the compiler what it targets
// (-dumpmachine) and refuses an OS or architecture other than the requested one, by name. LOCAL: the host's own CC.
func TestARecurseCgoConversionForAnotherTargetWithTheHostsCCIsRefused(t *testing.T) {
	requireHostCC(t)
	target := cgoRecurseOtherTarget()
	out, err := exec.Command("go", "env", "CC").Output()

	if err != nil {
		t.Fatal(err)
	}

	combined, _, err := convertCgoRecurseEnv(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "main.go": cgoRecurseMain},
		[]string{"CC=" + strings.TrimSpace(string(out))}, "-recurse=nuget", "-cgo=true", "-platforms", target)

	if err == nil {
		t.Fatalf("a -cgo conversion for %s with the host's C compiler named as CC converted:\n%s", target, combined)
	}

	for _, want := range []string{"this -cgo conversion", target, "-dumpmachine"} {
		if !strings.Contains(combined, want) {
			t.Errorf("the refusal does not name %q:\n%s", want, combined)
		}
	}
}

// X1, the cross-target conversion with a named C compiler. HOSTED. GO2CS_CGO_CROSS_TARGET and GO2CS_CGO_CROSS_CC name
// the target and its compiler; without them, when the C compiler go would use is zig (the converter-cgo stage's
// `;cc=zig`, .github/cgo-toolchain.ps1), the row converts for the other target with `<zig> cc -target <triple>`.
// Otherwise it is NOT MEASURED: this box reaches no zig (ziglang.org is a proxy 403 here).
func TestARecurseCgoConversionForAnotherTargetWithANamedCCBindsThatTarget(t *testing.T) {
	target, cc := os.Getenv("GO2CS_CGO_CROSS_TARGET"), os.Getenv("GO2CS_CGO_CROSS_CC")

	if target == "" || cc == "" {
		target, cc = cgoRecurseZigCrossTarget(t)
	}

	if target == "" || cc == "" {
		t.Skip("no cross target and compiler here (GO2CS_CGO_CROSS_TARGET/GO2CS_CGO_CROSS_CC, or zig as go's CC): NOT MEASURED")
	}

	// One size-dependent fact per target (P3's INC 2 shape read): C's long is 4 bytes on windows and 8 on the 64-bit
	// unix targets, and the route folds unsafe.Sizeof of it to a constant from the compiler's own layout.
	long := cgoRecurseCptr + "\n// LongBytes is the width of C's long on the target.\nfunc LongBytes() int { return int(unsafe.Sizeof(C.long(0))) }\n"
	combined, output, err := convertCgoRecurseEnv(t, map[string]string{"cptr/cptr.go": long, "main.go": cgoRecurseMain}, []string{"CC=" + cc},
		"-recurse=nuget", "-cgo=true", "-platforms", target)

	if err != nil {
		t.Fatalf("the cross-target conversion for %s failed: %v\n%s", target, err, combined)
	}

	width := "(uintptr)8"

	if strings.HasPrefix(target, "windows/") {
		width = "(uintptr)4"
	}

	sources := strings.Builder{}

	for _, text := range cgoRecurseWritten(t, output, "cptr", ".cs") {
		sources.WriteString(text)
	}

	cs := sources.String()

	for _, want := range []string{`EntryPoint = "malloc")]`, `GoCgoLibraries.Register(`, `"` + target + `"`, "unsafe.Sizeof(_Ctype_long(0)) */ " + width, "// C target triple: "} {
		if !strings.Contains(cs, want) {
			t.Errorf("cptr's %s emission lacks %q:\n%s", target, want, cs)
		}
	}

	t.Logf("READING %s via %q: %d bytes of cptr C#", target, cc, len(cs))
}

// R3's two other texts (P3's INC 2 shape read, SHOULD 5): an explicit sysroot and C++ sources are refused by name in
// INC 2. Both LOCAL, at the host's own target.
func TestARecurseCgoConversionWithASysrootIsRefused(t *testing.T) {
	requireHostCC(t)

	combined, _, err := convertCgoRecurseEnv(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "main.go": cgoRecurseMain},
		[]string{"CGO_CFLAGS=--sysroot=/nonexistent-sysroot"}, "-recurse=nuget", "-cgo=true")

	if err == nil {
		t.Fatalf("a -cgo conversion with a sysroot converted:\n%s", combined)
	}

	for _, want := range []string{"this -cgo conversion", "sysroot", "CGO_CFLAGS"} {
		if !strings.Contains(combined, want) {
			t.Errorf("the refusal does not name %q:\n%s", want, combined)
		}
	}
}

// RE-CITED (P3's 2a CLEAR, SHOULD 4; COORD's ruling): before 2a a host-target conversion of a package with C++ sources
// converted (measured at 6bdf33a157, single-package -cgo: rc=0), so INC 2 must change nothing there. The refusal holds
// for another target only, where C++ needs a compiler for it (CXX) INC 2 does not take; that row is hosted, with the
// pinned zig, beside X1.
func TestARecurseCgoConversionWithCppSourcesAtTheHostsTargetConvertsAsBefore(t *testing.T) {
	requireHostCC(t)

	combined, output, err := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "cptr/helper.cc": "int go2cs_helper() { return 1; }\n",
		"main.go": cgoRecurseMain}, "-recurse=nuget", "-cgo=true")

	if err != nil || strings.Contains(combined, "C compiler only") {
		t.Fatalf("a host-target conversion of a package with C++ sources did not convert as it did before 2a (%v):\n%s", err, combined)
	}

	if !cgoRecurseConverted(t, output, "cptr") {
		t.Errorf("cptr was not converted:\n%s", combined)
	}
}

func TestARecurseCgoConversionWithCppSourcesForAnotherTargetIsRefused(t *testing.T) {
	target, cc := os.Getenv("GO2CS_CGO_CROSS_TARGET"), os.Getenv("GO2CS_CGO_CROSS_CC")

	if target == "" || cc == "" {
		target, cc = cgoRecurseZigCrossTarget(t)
	}

	if target == "" || cc == "" {
		t.Skip("no cross target and compiler here (GO2CS_CGO_CROSS_TARGET/GO2CS_CGO_CROSS_CC, or zig as go's CC): NOT MEASURED")
	}

	combined, _, err := convertCgoRecurseEnv(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "cptr/helper.cc": "int go2cs_helper() { return 1; }\n",
		"main.go": cgoRecurseMain}, []string{"CC=" + cc}, "-recurse=nuget", "-cgo=true", "-platforms", target)

	_ = err

	for _, want := range []string{"C compiler only", "C++", "CXX", "helper.cc"} {
		if !strings.Contains(combined, want) {
			t.Errorf("the refusal does not name %q:\n%s", want, combined)
		}
	}
}

// cgoRecurseZigCrossTarget answers X1's target and compiler from a zig the C compiler go would use names: the other
// target than the host's, with zig's -target triple for it. It answers nothing when go's CC is not zig.
func cgoRecurseZigCrossTarget(t *testing.T) (string, string) {
	t.Helper()

	out, err := exec.Command("go", "env", "CC").Output()

	if err != nil {
		return "", ""
	}

	fields, err := splitCgoCommand(strings.TrimSpace(string(out)))

	if err != nil || !strings.HasPrefix(strings.ToLower(filepath.Base(fields[0])), "zig") {
		return "", ""
	}

	target := cgoRecurseOtherTarget()
	triples := map[string]string{"windows/amd64": "x86_64-windows-gnu", "linux/amd64": "x86_64-linux-gnu"}

	return target, fields[0] + " cc -target " + triples[target]
}

// P3's 2a CLEAR, SHOULDs 1 and 2 (COORD's ruling). The triple's ABI is read: gnu for linux, gnu or mingw for windows;
// musl, android and msvc are R3 by name. A target-changing flag (-target, --target=) in CGO_CFLAGS or CGO_CPPFLAGS is
// R3 like a sysroot: cmd/cgo would compile with it while the -dumpmachine probe, which runs CC alone, cannot see it. A
// -target in CC itself is what the probe answers for (the pinned zig is `zig cc -target <triple>`). LOCAL: a CC wrapper
// that answers -dumpmachine with a musl triple and is gcc otherwise.
func cgoRecurseMuslCC(t *testing.T) string {
	t.Helper()

	if runtime.GOOS == "windows" {
		t.Skip("the musl wrapper is a shell script: NOT MEASURED on windows")
	}

	out, err := exec.Command("go", "env", "CC").Output()

	if err != nil {
		t.Fatal(err)
	}

	wrapper := filepath.Join(t.TempDir(), "cc-musl")
	script := "#!/bin/sh\nif [ \"$1\" = \"-dumpmachine\" ]; then echo " + runtime.GOARCH + "-linux-musl; exit 0; fi\nexec " + strings.TrimSpace(string(out)) + " \"$@\"\n"
	script = strings.Replace(script, "amd64-linux-musl", "x86_64-linux-musl", 1)

	if err := os.WriteFile(wrapper, []byte(script), 0o755); err != nil {
		t.Fatal(err)
	}

	return wrapper
}

func TestARecurseCgoConversionWithAMuslCompilerIsRefused(t *testing.T) {
	requireHostCC(t)

	if runtime.GOOS != "linux" {
		t.Skip("the musl row is linux's: NOT MEASURED")
	}

	combined, _, err := convertCgoRecurseEnv(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "main.go": cgoRecurseMain},
		[]string{"CC=" + cgoRecurseMuslCC(t)}, "-recurse=nuget", "-cgo=true")

	if err == nil {
		t.Fatalf("a -cgo conversion with a musl C compiler converted:\n%s", combined)
	}

	for _, want := range []string{"this -cgo conversion", "musl"} {
		if !strings.Contains(combined, want) {
			t.Errorf("the refusal does not name %q:\n%s", want, combined)
		}
	}
}

func TestARecurseCgoConversionWithATargetFlagInCgoCflagsIsRefused(t *testing.T) {
	requireHostCC(t)

	for _, name := range []string{"CGO_CFLAGS", "CGO_CPPFLAGS"} {
		t.Run(name, func(t *testing.T) {
			combined, _, err := convertCgoRecurseEnv(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "main.go": cgoRecurseMain},
				[]string{name + "=--target=x86_64-windows-gnu"}, "-recurse=nuget", "-cgo=true")

			if err == nil {
				t.Fatalf("a -cgo conversion with a target flag in %s converted:\n%s", name, combined)
			}

			for _, want := range []string{"this -cgo conversion", name, "--target"} {
				if !strings.Contains(combined, want) {
					t.Errorf("the refusal does not name %q:\n%s", want, combined)
				}
			}
		})
	}
}

// The triple reader, one row per shape the R3 check meets (P3's 2a CLEAR, SHOULD 1).
func TestTheCgoTripleReaderReadsOSArchitectureAndABI(t *testing.T) {
	for _, c := range []struct{ triple, goos, goarch, abi string }{
		{"x86_64-linux-gnu", "linux", "amd64", "gnu"},
		{"aarch64-linux-gnu", "linux", "arm64", "gnu"},
		{"x86_64-linux-musl", "linux", "amd64", "musl"},
		{"aarch64-linux-android", "android", "arm64", "android"},
		{"x86_64-w64-mingw32", "windows", "amd64", "mingw"},
		{"x86_64-unknown-windows-gnu", "windows", "amd64", "gnu"},
		{"x86_64-pc-windows-msvc", "windows", "amd64", "msvc"},
		{"aarch64-apple-darwin24.0.0", "darwin", "arm64", ""},
		{"x86_64-macos-none", "darwin", "amd64", ""},
	} {
		goos, goarch, abi := cgoTripleTarget(c.triple)

		if goos != c.goos || goarch != c.goarch || abi != c.abi {
			t.Errorf("%s reads %s/%s %q, want %s/%s %q", c.triple, goos, goarch, abi, c.goos, c.goarch, c.abi)
		}
	}
}

// The probe runs once per CC value and target in a run, not once per package (P3's 2a CLEAR, SHOULD 3).
func TestTheCgoCompilerProbeRunsOncePerCompilerAndTarget(t *testing.T) {
	if runtime.GOOS == "windows" {
		t.Skip("the counting wrapper is a shell script: NOT MEASURED on windows")
	}

	dir := t.TempDir()
	count := filepath.Join(dir, "count")
	wrapper := filepath.Join(dir, "cc-count")

	if err := os.WriteFile(wrapper, []byte("#!/bin/sh\necho x >> "+count+"\necho x86_64-linux-gnu\n"), 0o755); err != nil {
		t.Fatal(err)
	}

	load := cgoLoad{env: append(os.Environ(), "CC="+wrapper), dir: dir}

	for range 3 {
		if triple, err := cgoCompilerTriple(load, "linux/amd64"); err != nil || triple != "x86_64-linux-gnu" {
			t.Fatalf("the probe answered %q, %v", triple, err)
		}
	}

	calls, err := os.ReadFile(count)

	if err != nil {
		t.Fatal(err)
	}

	if n := strings.Count(string(calls), "x"); n != 1 {
		t.Errorf("three lookups ran the probe %d times, want 1", n)
	}
}

// A route refusal under -recurse -cgo skips its package and names it; the rest of the module converts (COORD's ruling on
// C2's 193007Z: the plan says skip, and a module with one Phase 4 package must still convert the rest). bad's C function
// is defined in a C source file of its own (bad.c), which cgo Phase 4's preamble-only slice refuses by name (its §2.7),
// and both tests hold that text. A function the preamble defines is side-built since Phase 4, so the fixture no longer
// uses one. MOVE THIS FIXTURE AGAIN when a later slice compiles a package's own C files (the plan's static-link gap,
// go-sqlite3's sqlite3.c): it must keep reaching a refusal that slice does not retire. Single-package -cgo keeps the
// fatal refusal.
const cgoRecurseBad = `package bad

/*
int two(void);
*/
import "C"

// Two is C's two.
func Two() int { return int(C.two()) }
`

// cgoRecurseBadC is bad's C source file of its own.
const cgoRecurseBadC = "int two(void) { return 2; }\n"

// cgoRecurseBadRefusal is the refusal both tests hold bad to, by its text: not merely refused, refused for this reason.
const cgoRecurseBadRefusal = "C.two: no shared library the package links exports it, and the package has C source files of its own (bad.c): go2cs side-builds the preamble only"

func TestARecurseCgoConversionSkipsARouteRefusedPackageAndConvertsTheRest(t *testing.T) {
	requireHostCC(t)

	combined, output, _ := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr, "bad/bad.go": cgoRecurseBad, "bad/bad.c": cgoRecurseBadC, "main.go": cgoRecurseMain},
		"-recurse=nuget", "-cgo=true")

	for _, want := range []string{"GO2CS2001", "Refused 1 package", "example.com/cgorecurse/bad", cgoRecurseBadRefusal} {
		if !strings.Contains(combined, want) {
			t.Errorf("the run does not name %q:\n%s", want, combined)
		}
	}

	if cgoRecurseConverted(t, output, "bad") {
		t.Errorf("the refused bad was written:\n%s", combined)
	}

	requireCgoRecurseRestConverted(t, output, combined)
}

func TestASinglePackageCgoConversionKeepsTheFatalRouteRefusal(t *testing.T) {
	requireHostCC(t)

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	dir := t.TempDir()
	writeCgoFixture(t, dir, "go.mod", "module example.com/bad\n\ngo 1.26\n")
	writeCgoFixture(t, dir, "bad.go", cgoRecurseBad)
	writeCgoFixture(t, dir, "bad.c", cgoRecurseBadC)
	output := t.TempDir()
	cmd := exec.Command(driverBinary(t), "-cgo=true", "-go2cspath", treeSrc, dir, output)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=")
	combined, err := cmd.CombinedOutput()

	if err == nil || !strings.Contains(string(combined), "Refusing to convert") || !strings.Contains(string(combined), "GO2CS2001") || !strings.Contains(string(combined), cgoRecurseBadRefusal) {
		t.Errorf("a single-package -cgo conversion of a refused package did not stop with the fatal refusal (%v):\n%s", err, combined)
	}
}
