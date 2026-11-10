// cgoLibraryCapture_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/parser"
	"go/token"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"slices"
	"strings"
	"testing"
)

// §11.15's grammar, every arm: what Phase 2 passes to a tool, what it sets aside, and what it refuses by name.
func TestCgoLinkFlagsTakeOnlyPlainTokens(t *testing.T) {
	dir := t.TempDir()

	flags, err := parseCgoLinkFlags("darwin", []string{"-O2", "-g", "-lm", "-L" + dir, "-framework", "CoreFoundation", "-lfoo.bar+1"}, dir)

	if err != nil {
		t.Fatal(err)
	}

	if !slices.Equal(flags.libraries, []string{"m", "foo.bar+1"}) || !slices.Equal(flags.dirs, []string{dir}) || !slices.Equal(flags.frameworks, []string{"CoreFoundation"}) {
		t.Errorf("parsed %+v", flags)
	}

	if relative, err := parseCgoLinkFlags("linux", []string{"-L."}, dir); err != nil || !slices.Equal(relative.dirs, []string{dir}) {
		t.Errorf("-L. is the package directory: %+v %v", relative, err)
	}

	for _, refused := range []struct {
		goos, arg, says string
	}{
		{"linux", "-l:libm.so.6", "names a library FILE"},
		{"linux", "-l-whole-archive", "plain token"},
		{"linux", "-l@file", "plain token"},
		{"linux", "-lsub/dir", "plain token"},
		{"linux", "-L" + filepath.Join(dir, "missing"), "is not a directory"},
		{"linux", "-L", "names no directory"},
		{"linux", "-framework", "is darwin's"},
		{"linux", "-Wl,-rpath,/tmp", "is not one Phase 2 passes"},
		{"linux", "-fplugin=x", "is not one Phase 2 passes"},
	} {
		if _, err := parseCgoLinkFlags(refused.goos, []string{refused.arg, "x"}, dir); err == nil || !strings.Contains(err.Error(), refused.says) {
			t.Errorf("%s on %s: %v, want it to say %q", refused.arg, refused.goos, err, refused.says)
		}
	}
}

// cmd/cgo writes one //go:cgo_ldflag line per linker argument; the Phase 1 fixture carries cmd/go's default -O2 -g.
func TestCgoLinkerDirectivesReadTheFixture(t *testing.T) {
	file, err := parser.ParseFile(token.NewFileSet(), filepath.Join(cgoFixtureDir, "cgo_gotypes.go"), nil, parser.ParseComments)

	if err != nil {
		t.Fatal(err)
	}

	args, err := cgoLinkerDirectives(file)

	if err != nil {
		t.Fatal(err)
	}

	if !slices.Equal(args, []string{"-O2", "-g"}) {
		t.Errorf("the fixture's linker directives read %q", args)
	}

	if flags, err := parseCgoLinkFlags("linux", args, "."); err != nil || len(flags.libraries) != 0 {
		t.Errorf("cmd/go's defaults are set aside: %+v %v", flags, err)
	}
}

// A CC command splits as cmd/go splits it, so a pinned zig under a path with a space survives.
func TestCgoCompilerCommandsSplitAsCmdGoSplitsThem(t *testing.T) {
	for command, want := range map[string][]string{
		"zig cc":                               {"zig", "cc"},
		`"C:\Program Files\zig\zig.exe" cc -w`: {`C:\Program Files\zig\zig.exe`, "cc", "-w"},
		"'/opt/my zig/zig'  cc":                {"/opt/my zig/zig", "cc"},
		"gcc":                                  {"gcc"},
	} {
		if got, err := splitCgoCommand(command); err != nil || !slices.Equal(got, want) {
			t.Errorf("%q split %q (%v), want %q", command, got, err, want)
		}
	}

	if _, err := splitCgoCommand(`"zig cc`); err == nil {
		t.Error("an unterminated quote must be refused")
	}
}

func TestTheCgoProbeReferencesEverySymbol(t *testing.T) {
	source := cgoProbeSource([]string{"cos", "div"})

	for _, want := range []string{"extern void cos(void);", "extern void div(void);", "(void *)cos,", "(void *)div,", "go2cs_cgo_probe_refs[]"} {
		if !strings.Contains(source, want) {
			t.Errorf("the probe does not contain %q:\n%s", want, source)
		}
	}
}

// cgoCaptureToolchain is the C compiler the capture arms link with: CC when the leg sets it (the converter-cgo stage's
// pinned zig does), else go's own default. Without one they skip, except under GO2CS_REQUIRE_CC=1, where a hosted leg
// sets it and the absence reads red.
func cgoCaptureToolchain(t *testing.T) cgoToolchain {
	t.Helper()

	command := os.Getenv("CC")

	if command == "" {
		if out, err := exec.Command("go", "env", "CC").Output(); err == nil {
			command = strings.TrimSpace(string(out))
		}
	}

	fields, err := splitCgoCommand(command)

	if err == nil {
		_, err = exec.LookPath(fields[0])
	}

	if err != nil {
		if os.Getenv("GO2CS_REQUIRE_CC") == "1" {
			t.Fatalf("GO2CS_REQUIRE_CC=1, and no C compiler runs here (%q): %v", command, err)
		}

		t.Skipf("no C compiler (%q): the load-name capture cannot link here (Inconclusive, not a pass)", command)
	}

	t.Logf("READING C compiler: %q", fields)

	return cgoToolchain{cc: fields}
}

// buildCgoTestLibrary builds a shared library defining one symbol, with the load-time name each linker records for it:
// a SONAME (lib<name>.so.1), an absolute install name, or the DLL's own name.
func buildCgoTestLibrary(t *testing.T, toolchain cgoToolchain, dir, name, symbol string) string {
	t.Helper()

	source := filepath.Join(dir, name+".c")
	body := "#ifdef _WIN32\n__declspec(dllexport)\n#endif\nint " + symbol + "(void) { return 1; }\n"

	if err := os.WriteFile(source, []byte(body), 0o644); err != nil {
		t.Fatal(err)
	}

	var output, loadName string
	var extra []string

	switch runtime.GOOS {
	case "windows":
		output, loadName = filepath.Join(dir, name+".dll"), name+".dll"
		extra = []string{"-Wl,--out-implib," + filepath.Join(dir, "lib"+name+".dll.a")}
	case "darwin":
		output = filepath.Join(dir, "lib"+name+".dylib")
		loadName = output
		extra = []string{"-Wl,-install_name," + output}
	default:
		output, loadName = filepath.Join(dir, "lib"+name+".so"), "lib"+name+".so.1"
		extra = []string{"-fPIC", "-Wl,-soname," + loadName}
	}

	args := append(append(append([]string{}, toolchain.cc[1:]...), "-shared", "-o", output, source), extra...)
	cmd := exec.Command(toolchain.cc[0], args...)
	cmd.Dir = dir

	if combined, err := cmd.CombinedOutput(); err != nil {
		t.Fatalf("building %s: %v\n%s", output, err, combined)
	}

	return loadName
}

// Two -l libraries defining disjoint symbols: each symbol is captured with ITS library's load-time name, per symbol and
// not per -l. On ELF the linker's --trace-symbol route and the per -l fallback must agree, so the fallback is proven and
// not only described (COORD's 4b ruling).
func TestCgoCaptureNamesEachSymbolsOwnLibrary(t *testing.T) {
	toolchain := cgoCaptureToolchain(t)
	dir := t.TempDir()

	wantA := buildCgoTestLibrary(t, toolchain, dir, "go2csa", "go2cs_cgo_a")
	wantB := buildCgoTestLibrary(t, toolchain, dir, "go2csb", "go2cs_cgo_b")
	flags := cgoLinkFlags{libraries: []string{"go2csa", "go2csb"}, dirs: []string{dir}}
	symbols := []string{"go2cs_cgo_a", "go2cs_cgo_b"}

	got, err := captureCgoLibraries(toolchain, runtime.GOOS, flags, symbols, t.TempDir())

	if err != nil {
		t.Fatal(err)
	}

	want := []cgoSymbolLibrary{{"go2cs_cgo_a", wantA}, {"go2cs_cgo_b", wantB}}

	if !slices.Equal(got, want) {
		t.Errorf("captured %+v, want %+v", got, want)
	}

	if runtime.GOOS != "linux" {
		return
	}

	fallback, err := elfLibraryLoadNames(toolchain, flags, t.TempDir())

	if err != nil {
		t.Fatal(err)
	}

	for _, entry := range want {
		if fallback[entry.symbol] != entry.name {
			t.Errorf("the per -l fallback maps %s to %q, the --trace-symbol route to %q", entry.symbol, fallback[entry.symbol], entry.name)
		}
	}
}

// A system library every host has: cos through -lm, captured as each linker records it. Windows records the import as
// LINKED, an API set included (COORD's Q3 ruling), so its reading is logged and held to name a C runtime DLL.
func TestCgoCaptureNamesTheSystemMathLibrary(t *testing.T) {
	toolchain := cgoCaptureToolchain(t)

	got, err := captureCgoLibraries(toolchain, runtime.GOOS, cgoLinkFlags{libraries: []string{"m"}}, []string{"cos"}, t.TempDir())

	if err != nil {
		t.Fatal(err)
	}

	if len(got) != 1 {
		t.Fatalf("captured %+v, want one entry for cos", got)
	}

	t.Logf("READING cos via -lm: %s", got[0].name)

	switch runtime.GOOS {
	case "linux":
		if got[0].name != "libm.so.6" {
			t.Errorf("cos resolves to %q, want libm.so.6", got[0].name)
		}
	case "darwin":
		if got[0].name != "/usr/lib/libSystem.B.dylib" {
			t.Errorf("cos resolves to %q, want /usr/lib/libSystem.B.dylib", got[0].name)
		}
	default:
		// A C runtime: the UCRT's API set or ucrtbase.dll, or msvcrt.dll for an msvcrt toolchain. Never KERNEL32.
		if name := strings.ToLower(got[0].name); !strings.HasSuffix(name, ".dll") || !strings.Contains(name, "crt") {
			t.Errorf("cos resolves to %q, which names no C runtime DLL", got[0].name)
		}
	}
}

// A symbol no library defines is refused by name: Phase 2 binds only what a shared library exports.
func TestCgoCaptureRefusesASymbolNoLibraryDefines(t *testing.T) {
	toolchain := cgoCaptureToolchain(t)

	_, err := captureCgoLibraries(toolchain, runtime.GOOS, cgoLinkFlags{}, []string{"go2cs_cgo_nowhere"}, t.TempDir())

	if err == nil || !strings.Contains(err.Error(), "go2cs_cgo_nowhere") {
		t.Errorf("err %v, want a refusal naming go2cs_cgo_nowhere", err)
	}
}
