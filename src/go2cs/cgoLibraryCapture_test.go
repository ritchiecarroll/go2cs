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
		// The admission of cmd/go's default CGO_LDFLAGS is token-exact (§11.15, as amended).
		{"linux", "-O3", "is not one Phase 2 passes"},
		{"linux", "-g3", "is not one Phase 2 passes"},
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
// not per -l. On ELF the per-symbol fallback must give the same answer as the capture, so the fallback is proven and not
// only described (COORD's 4b ruling): where the linker takes --trace-symbol (GNU ld) that is the two routes agreeing, and
// where it does not (zig) the capture IS the fallback.
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

	fallback, err := elfProbeLoadNames(toolchain, flags, symbols, t.TempDir())

	if err != nil {
		t.Fatal(err)
	}

	for _, entry := range want {
		if fallback[entry.symbol] != entry.name {
			t.Errorf("the per-symbol fallback maps %s to %q, the capture to %q", entry.symbol, fallback[entry.symbol], entry.name)
		}
	}
}

// A system library every host has: cos through -lm, captured as each linker records it. Windows records the import as
// LINKED, an API set included (COORD's Q3 ruling), so its reading is logged and held to name a C runtime DLL. A windows
// toolchain whose libm links cos STATICALLY (zig's mingw libm does) is refused by name, which is §11.5's own example.
func TestCgoCaptureNamesTheSystemMathLibrary(t *testing.T) {
	toolchain := cgoCaptureToolchain(t)

	got, err := captureCgoLibraries(toolchain, runtime.GOOS, cgoLinkFlags{libraries: []string{"m"}}, []string{"cos"}, t.TempDir())

	if err != nil && runtime.GOOS == "windows" && strings.Contains(err.Error(), "cos resolved to no shared library") {
		t.Logf("READING cos via -lm: resolved statically by this toolchain, so refused by name: %v", err)
		return
	}

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

// G1's windows arm needs a C runtime symbol that BOTH the host mingw gcc and the pinned zig resolve DYNAMICALLY (§11.10, as
// amended): cos does not, being static under zig. Each candidate is captured alone, so a static one cannot hide another,
// and each reading is logged; kernel32's GetCurrentProcessId is held to resolve dynamically under any windows toolchain.
func TestCgoCaptureReadsWindowsCRuntimeCandidatesOneByOne(t *testing.T) {
	if runtime.GOOS != "windows" {
		t.Skip("the windows C runtime candidates for G1: NOT MEASURED off windows")
	}

	toolchain := cgoCaptureToolchain(t)

	for _, symbol := range []string{"strlen", "puts", "GetCurrentProcessId"} {
		got, err := captureCgoLibraries(toolchain, runtime.GOOS, cgoLinkFlags{}, []string{symbol}, t.TempDir())

		if err != nil {
			t.Logf("READING %s: not dynamic under this toolchain: %v", symbol, err)

			if symbol == "GetCurrentProcessId" {
				t.Errorf("kernel32's GetCurrentProcessId must resolve dynamically: %v", err)
			}

			continue
		}

		t.Logf("READING %s: %s", symbol, got[0].name)

		if symbol == "GetCurrentProcessId" && !strings.EqualFold(got[0].name, "kernel32.dll") {
			t.Errorf("GetCurrentProcessId resolves to %q, want kernel32.dll", got[0].name)
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

// darwin loads a captured library only by an absolute install name (§11.12, mandatory since increment 4d): dyld's search
// for a bare name reached the current directory on both hosted RIDs (golib-tests 42936395011), and an @ name is one the
// CLR's loader cannot resolve as dyld does. Both are refused by name; an absolute name is admitted.
func TestCgoDarwinCaptureAdmitsOnlyAbsoluteInstallNames(t *testing.T) {
	for _, c := range []struct{ library, refusal string }{
		{"/usr/lib/libSystem.B.dylib", ""},
		{"/tmp/go2cs-cgo-probe/libgo2csprobe.dylib", ""},
		{"@rpath/libfoo.dylib", "cannot resolve as dyld does"},
		{"libgo2csprobe.dylib", "dyld's search for a bare name reaches the current directory"},
		{"lib/libfoo.dylib", "dyld's search for a bare name reaches the current directory"},
	} {
		err := cgoDarwinInstallName("f", c.library)

		switch {
		case c.refusal == "" && err != nil:
			t.Errorf("%s: refused (%v), want it admitted", c.library, err)
		case c.refusal != "" && (err == nil || !strings.Contains(err.Error(), c.refusal) || !strings.Contains(err.Error(), c.library)):
			t.Errorf("%s: err %v, want a refusal naming it and saying %q", c.library, err, c.refusal)
		}
	}
}
