// cgoWrappers_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"maps"
	"os"
	"path/filepath"
	"regexp"
	"slices"
	"strings"
	"testing"
)

// cgoWrapperTarget is one OS the wrapper golden is generated for: the C library its libc symbols bind in, the width of
// C long there (LP64 on linux and darwin, LLP64 on windows), the errno arm its libc can take, and the C structs and
// struct-crossing functions its arms call (increment 3b2a), with the library of each symbol not in libc.
type cgoWrapperTarget struct {
	goos, library, crt, long, errnoDecl, structDecls string
	libraries                                        map[string]string
	ldflags                                          []string // the -l names its #cgo LDFLAGS would carry (increment 4c)
	goarch                                           string   // the architecture its bindings are converted for (increment 5b)
}

// goldenName is the target's golden file: GoCgoWrappers.<goos>.golden.cs for amd64, and <goos>-<goarch> otherwise.
func (target cgoWrapperTarget) goldenName() string {
	if target.goarch == "amd64" {
		return "GoCgoWrappers." + target.goos + ".golden.cs"
	}

	return "GoCgoWrappers." + target.goos + "-" + target.goarch + ".golden.cs"
}

var cgoWrapperTargets = []cgoWrapperTarget{
	// close(-1) sets EBADF on every POSIX libc.
	{"linux", "libc.so.6", "", "int64", "func _C2func_close(p0 _Ctype_int) (r1 _Ctype_int, r2 error) { return }", cgoLinuxStructs, nil, []string{"c"}, "amd64"},
	{"darwin", "/usr/lib/libSystem.B.dylib", "", "int64", "func _C2func_close(p0 _Ctype_int) (r1 _Ctype_int, r2 error) { return }", cgoDarwinStructs, nil, []string{"System"}, "amd64"},
	// Apple silicon runs an arm64 process, and golib's resolver holds bindings to the target they were converted for
	// (§5.3, §11.20), so darwin's arm64 GolibTests leg compiles bindings converted for darwin/arm64.
	{"darwin", "/usr/lib/libSystem.B.dylib", "", "int64", "func _C2func_close(p0 _Ctype_int) (r1 _Ctype_int, r2 error) { return }", cgoDarwinStructs, nil, []string{"System"}, "arm64"},
	// The UCRT's _close(-1) would raise the invalid-parameter handler, so windows' errno arm is exp's overflow (ERANGE).
	{"windows", "ucrtbase.dll", "ucrtbase.dll", "int32", "func _C2func_exp(p0 _Ctype_double) (r1 _Ctype_double, r2 error) { return }", cgoWindowsStructs,
		map[string]string{"PtInRect": "user32.dll", "UnionRect": "user32.dll", "GetSystemTime": "kernel32.dll"}, []string{"ucrtbase", "user32", "kernel32"}, "amd64"},
}

// The struct arms per OS. div by value everywhere; the by-value argument is inet_ntoa's struct in_addr on linux and
// darwin and PtInRect's POINT on windows; the copy-back is uname's struct utsname and GetSystemTime's SYSTEMTIME.
// UnionRect(&r, &r, &o) on windows passes one struct through two pointers, which must share one mirror.
const cgoLinuxStructs = `
// div_t: two ints, by value in the result.
type _Ctype_div_t struct {
	quot _Ctype_int
	rem  _Ctype_int
}

// struct in_addr: one in_addr_t (uint32_t), by value in the argument.
type _Ctype_struct_in_addr struct {
	s_addr _Ctype_uint
}

// glibc's struct utsname: six char[65] (the sixth is domainname under _GNU_SOURCE, __domainname otherwise).
type _Ctype_struct_utsname struct {
	sysname    [65]_Ctype_char
	nodename   [65]_Ctype_char
	release    [65]_Ctype_char
	version    [65]_Ctype_char
	machine    [65]_Ctype_char
	domainname [65]_Ctype_char
}

func _Cfunc_div(p0 _Ctype_int, p1 _Ctype_int) (r1 _Ctype_div_t) { return }
func _Cfunc_inet_ntoa(p0 _Ctype_struct_in_addr) (r1 *_Ctype_char) { return }
func _Cfunc_uname(p0 *_Ctype_struct_utsname) (r1 _Ctype_int) { return }

// struct tm: nine ints, then glibc's and darwin's two extensions, tm_gmtoff and the pointer MEMBER tm_zone
// (increment 3b2b). time_t is C long, and cmd/cgo writes the typedef as a Go alias.
type _Ctype_struct_tm struct {
	tm_sec    _Ctype_int
	tm_min    _Ctype_int
	tm_hour   _Ctype_int
	tm_mday   _Ctype_int
	tm_mon    _Ctype_int
	tm_year   _Ctype_int
	tm_wday   _Ctype_int
	tm_yday   _Ctype_int
	tm_isdst  _Ctype_int
	tm_gmtoff _Ctype_long
	tm_zone   *_Ctype_char
}

type _Ctype_time_t = _Ctype_long

func _Cfunc_gmtime_r(p0 *_Ctype_time_t, p1 *_Ctype_struct_tm) (r1 *_Ctype_struct_tm) { return }
func _Cfunc_frexp(p0 _Ctype_double, p1 *_Ctype_int) (r1 _Ctype_double) { return }
`

const cgoDarwinStructs = `
// div_t: two ints, by value in the result.
type _Ctype_div_t struct {
	quot _Ctype_int
	rem  _Ctype_int
}

// struct in_addr: one in_addr_t (uint32_t), by value in the argument.
type _Ctype_struct_in_addr struct {
	s_addr _Ctype_uint
}

// darwin's struct utsname: five char[_SYS_NAMELEN], which is 256.
type _Ctype_struct_utsname struct {
	sysname  [256]_Ctype_char
	nodename [256]_Ctype_char
	release  [256]_Ctype_char
	version  [256]_Ctype_char
	machine  [256]_Ctype_char
}

func _Cfunc_div(p0 _Ctype_int, p1 _Ctype_int) (r1 _Ctype_div_t) { return }
func _Cfunc_inet_ntoa(p0 _Ctype_struct_in_addr) (r1 *_Ctype_char) { return }
func _Cfunc_uname(p0 *_Ctype_struct_utsname) (r1 _Ctype_int) { return }

// struct tm: nine ints, then glibc's and darwin's two extensions, tm_gmtoff and the pointer MEMBER tm_zone
// (increment 3b2b). time_t is C long, and cmd/cgo writes the typedef as a Go alias.
type _Ctype_struct_tm struct {
	tm_sec    _Ctype_int
	tm_min    _Ctype_int
	tm_hour   _Ctype_int
	tm_mday   _Ctype_int
	tm_mon    _Ctype_int
	tm_year   _Ctype_int
	tm_wday   _Ctype_int
	tm_yday   _Ctype_int
	tm_isdst  _Ctype_int
	tm_gmtoff _Ctype_long
	tm_zone   *_Ctype_char
}

type _Ctype_time_t = _Ctype_long

func _Cfunc_gmtime_r(p0 *_Ctype_time_t, p1 *_Ctype_struct_tm) (r1 *_Ctype_struct_tm) { return }
func _Cfunc_frexp(p0 _Ctype_double, p1 *_Ctype_int) (r1 _Ctype_double) { return }
`

const cgoWindowsStructs = `
type _Ctype_ushort uint16

// div_t: two ints, by value in the result.
type _Ctype_div_t struct {
	quot _Ctype_int
	rem  _Ctype_int
}

// POINT and RECT: LONGs, which are C long (32 bits under LLP64). POINT crosses by value, RECT by pointer.
type _Ctype_POINT struct {
	x _Ctype_long
	y _Ctype_long
}

type _Ctype_RECT struct {
	left   _Ctype_long
	top    _Ctype_long
	right  _Ctype_long
	bottom _Ctype_long
}

// SYSTEMTIME: eight WORDs, written by GetSystemTime through its pointer.
type _Ctype_SYSTEMTIME struct {
	wYear         _Ctype_ushort
	wMonth        _Ctype_ushort
	wDayOfWeek    _Ctype_ushort
	wDay          _Ctype_ushort
	wHour         _Ctype_ushort
	wMinute       _Ctype_ushort
	wSecond       _Ctype_ushort
	wMilliseconds _Ctype_ushort
}

func _Cfunc_div(p0 _Ctype_int, p1 _Ctype_int) (r1 _Ctype_div_t) { return }
func _Cfunc_PtInRect(p0 *_Ctype_RECT, p1 _Ctype_POINT) (r1 _Ctype_int) { return }
func _Cfunc_GetSystemTime(p0 *_Ctype_SYSTEMTIME) (r1 _Ctype_void) { return }
func _Cfunc_UnionRect(p0 *_Ctype_RECT, p1 *_Ctype_RECT, p2 *_Ctype_RECT) (r1 _Ctype_int) { return }

// windows' struct tm is the nine ints alone, and __time64_t is long long (increment 3b2b: a pointer to one scalar).
type _Ctype_longlong int64

type _Ctype_struct_tm struct {
	tm_sec   _Ctype_int
	tm_min   _Ctype_int
	tm_hour  _Ctype_int
	tm_mday  _Ctype_int
	tm_mon   _Ctype_int
	tm_year  _Ctype_int
	tm_wday  _Ctype_int
	tm_yday  _Ctype_int
	tm_isdst _Ctype_int
}

type _Ctype___time64_t = _Ctype_longlong

func _Cfunc__gmtime64_s(p0 *_Ctype_struct_tm, p1 *_Ctype___time64_t) (r1 _Ctype_int) { return }
func _Cfunc_frexp(p0 _Ctype_double, p1 *_Ctype_int) (r1 _Ctype_double) { return }
`

// The probe library's declarations (increment 4d), as cmd/cgo gives them on every OS: src/tests/GolibTests/cgo-probe's
// probe.c, which the hosted golib-tests legs build per RID with the OS's default C toolchain. A pair by value both ways
// (windows' stand-in for inet_ntoa, whose in_addr is a union there), one struct through two pointers, a union inside a
// struct as the bytes cmd/cgo gives it, a struct of two doubles by pointer, and strtol's errno through the probe.
const cgoProbeDecls = `
type _Ctype_struct_go2cs_probe_pair struct {
	a _Ctype_int
	b _Ctype_int
}

type _Ctype_struct_go2cs_probe_tagged struct {
	tag _Ctype_int
	_   [4]byte
	u   [8]byte
}

type _Ctype_struct_go2cs_probe_hfa struct {
	x _Ctype_double
	y _Ctype_double
}

func _Cfunc_go2cs_probe_pair(p0 _Ctype_int, p1 _Ctype_int) (r1 _Ctype_struct_go2cs_probe_pair) { return }
func _Cfunc_go2cs_probe_pair_sum(p0 _Ctype_struct_go2cs_probe_pair) (r1 _Ctype_int) { return }
func _Cfunc_go2cs_probe_add_into(p0 *_Ctype_struct_go2cs_probe_pair, p1 *_Ctype_struct_go2cs_probe_pair, p2 *_Ctype_struct_go2cs_probe_pair) (r1 _Ctype_int) { return }
func _Cfunc_go2cs_probe_tagged_double(p0 *_Ctype_struct_go2cs_probe_tagged) (r1 _Ctype_int) { return }
func _Cfunc_go2cs_probe_hfa_scale(p0 *_Ctype_struct_go2cs_probe_hfa, p1 _Ctype_double) (r1 _Ctype_double) { return }
func _C2func_go2cs_probe_strtol(p0 *_Ctype_char, p1 _Ctype_int) (r1 _Ctype_long, r2 error) { return }
`

// cgoProbeLibrary is the probe library's load-time name on goos, as the hosted leg builds it (its SONAME on linux, its
// install name on darwin, its DLL name on windows). darwin's is absolute, as every darwin load name must be (§11.12):
// the leg builds the probe at that fixed path.
func cgoProbeLibrary(goos string) string {
	switch goos {
	case "windows":
		return "go2csprobe.dll"
	case "darwin":
		return "/tmp/go2cs-cgo-probe/libgo2csprobe.dylib"
	default:
		return "libgo2csprobe.so"
	}
}

// cgoWrapperFixture is the cgo-shaped package the golden binds, as cmd/cgo would declare it for target.
func cgoWrapperFixture(t *testing.T, target cgoWrapperTarget) *types.Package {
	t.Helper()

	source := `package cgotest

type _Ctype_char int8
type _Ctype_int int32
type _Ctype_uint uint32
type _Ctype_long ` + target.long + `
type _Ctype_double float64
type _Ctype_void [0]byte

func _Cfunc_abs(p0 _Ctype_int) (r1 _Ctype_int) { return }
func _Cfunc_labs(p0 _Ctype_long) (r1 _Ctype_long) { return }
func _Cfunc_srand(p0 _Ctype_uint) (r1 _Ctype_void) { return }
` + target.errnoDecl + "\n" + target.structDecls + cgoProbeDecls

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "cgotest.go", source, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{Importer: importer.Default(), Sizes: types.SizesFor("gc", target.goarch)}).Check("cgotest", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	return pkg
}

// cgoWrapperGolden is what the GolibTests golden for target holds: a header, then the generator's own output.
func cgoWrapperGolden(t *testing.T, target cgoWrapperTarget) string {
	t.Helper()

	members := map[string]string{}
	set := newCgoWrapperSet(cgoBindingTarget{goos: target.goos, library: target.library, crt: target.crt}, types.SizesFor("gc", target.goarch))
	set.libraries = maps.Clone(target.libraries)

	if set.libraries == nil {
		set.libraries = map[string]string{}
	}

	for _, symbol := range []string{"go2cs_probe_pair", "go2cs_probe_pair_sum", "go2cs_probe_add_into", "go2cs_probe_tagged_double", "go2cs_probe_hfa_scale", "go2cs_probe_strtol"} {
		set.libraries[symbol] = cgoProbeLibrary(target.goos)
	}

	if target.crt != "" {
		members["ᴍerrno"] = cgoErrnoLocationBinding(target.crt)
	}

	for _, declaration := range cgoDeclarations(cgoWrapperFixture(t, target)) {
		if declaration.kind != cgoFunc && declaration.kind != cgoErrnoFunc {
			continue
		}

		text, err := set.wrapper(declaration)

		if err != nil {
			t.Fatal(err)
		}

		members[declaration.cName+cgoKindLabel(declaration.kind)] = text
	}

	for name, text := range set.support() {
		members[name] = text
	}

	// The resolver's table: every library the bindings load, each named for its -l (increment 4c).
	var loaded []cgoSymbolLibrary

	for _, name := range append([]string{target.library}, slices.Sorted(maps.Values(set.libraries))...) {
		loaded = append(loaded, cgoSymbolLibrary{name: name})
	}

	members[cgoMirrorSuffix+"0 libraries"] = cgoLibrariesRegistration("cgotest", target.goos+"/"+target.goarch, cgoLibraryTable(cgoLinkFlags{libraries: append(slices.Clone(target.ldflags), "go2csprobe")}, loaded))

	return "// " + target.goldenName() + " - generated by go2cs's cgo wrapper generator for " + target.goos + "/" + target.goarch + ".\n" +
		"// DO NOT EDIT: src/go2cs's TestCgoScalarWrapperGoldensAreTheGeneratorsOutput holds this file equal to the\n" +
		"// generator; run it with GO2CS_UPDATE_CGO_GOLDEN=1 to rewrite it. GolibTests compiles and runs it against libc.\n\n" +
		"namespace go;\n\n" + cgoPseudoPackageText("cgotest", members)
}

func cgoKindLabel(kind cgoDeclKind) string {
	for _, entry := range cgoKindLabels {
		if entry.kind == kind {
			return " " + entry.label
		}
	}

	return ""
}

func TestCgoPseudoPackageClassIsThePackageWithTheCMark(t *testing.T) {
	if got, want := cgoPseudoPackageClass("cgotest"), "cgotestꓸC_package"; got != want {
		t.Errorf("pseudo-package class %q, want %q", got, want)
	}
}

// The generator's output for each target, byte for byte, is the golden GolibTests compiles and runs: what is proven
// against libc there is what the converter writes.
func TestCgoScalarWrapperGoldensAreTheGeneratorsOutput(t *testing.T) {
	for _, target := range cgoWrapperTargets {
		path := filepath.Join("..", "tests", "GolibTests", target.goldenName())
		want := cgoWrapperGolden(t, target)

		if os.Getenv("GO2CS_UPDATE_CGO_GOLDEN") == "1" {
			if err := os.WriteFile(path, []byte(strings.ReplaceAll(want, "\n", "\r\n")), 0o644); err != nil {
				t.Fatal(err)
			}

			continue
		}

		committed, err := os.ReadFile(path)

		if err != nil {
			t.Fatalf("%s: %v (run with GO2CS_UPDATE_CGO_GOLDEN=1 to write it)", path, err)
		}

		if got := strings.ReplaceAll(string(committed), "\r\n", "\n"); got != want {
			t.Errorf("%s is not the generator's output for %s; run with GO2CS_UPDATE_CGO_GOLDEN=1 and read the diff\n got:\n%s\nwant:\n%s", path, target.goos, got, want)
		}
	}
}

// What 3b1 does not bind is refused by name: a pointer parameter, one of cmd/cgo's own helpers, and uintptr.
func TestCgoScalarWrapperRefusesWhatIsNotWhollyScalar(t *testing.T) {
	source := `package cgotest

import "unsafe"

type _Ctype_int int32
type _Ctype_char int8
type _Ctype_struct_x struct{ a _Ctype_int }

func _Cfunc_free(p0 unsafe.Pointer) (r1 _Ctype_int) { return }
func _Cfunc_scale(p0 *_Ctype_struct_x) (r1 _Ctype_int) { return }
func _Cfunc_CString(p0 string) (r1 *_Ctype_char) { return }
func _Cfunc_handle(p0 uintptr) (r1 _Ctype_int) { return }
`
	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "cgotest.go", source, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{Importer: importer.Default()}).Check("cgotest", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	want := map[string]string{
		"free":    "C.free's parameter 1 is not a scalar",
		"scale":   "C.scale's parameter 1 is not a scalar (a C struct is increment 3b2",
		"CString": "C.CString is one of cmd/cgo's own helpers",
		"handle":  "C.handle's parameter 1 is of basic kind uintptr",
	}

	for _, declaration := range cgoDeclarations(pkg) {
		reason, listed := want[declaration.cName]

		if !listed {
			continue
		}

		_, err := cgoScalarWrapper(declaration, cgoBindingTarget{goos: "linux", library: "libc.so.6"})

		if err == nil || !strings.Contains(err.Error(), reason) {
			t.Errorf("C.%s: err %v, want it to say %q", declaration.cName, err, reason)
		}

		delete(want, declaration.cName)
	}

	for name := range want {
		t.Errorf("C.%s was not recognized", name)
	}
}

// Increment 4d: the two-result form crosses a pointer for a SCALAR result (the probe's strtol is its arm); an errno form
// returning a struct or a pointer waits for a measured arm and is refused by name.
func TestCgoErrnoFormThroughAPointerTakesAScalarResultOnly(t *testing.T) {
	source := `package cgotest

type _Ctype_int int32
type _Ctype_struct_x struct{ a _Ctype_int }

func _C2func_count(p0 *_Ctype_int) (r1 _Ctype_int, r2 error) { return }
func _C2func_fill(p0 *_Ctype_int) (r1 _Ctype_struct_x, r2 error) { return }
func _C2func_find(p0 *_Ctype_struct_x) (r1 *_Ctype_int, r2 error) { return }
`
	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "cgotest.go", source, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{Importer: importer.Default()}).Check("cgotest", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	const refused = "an errno form with a struct or pointer result waits for a measured arm"
	want := map[string]string{"count": "", "fill": refused, "find": refused}

	for _, goos := range []string{"linux", "windows"} {
		set := newCgoWrapperSet(cgoBindingTarget{goos: goos, library: "libc.so.6", crt: "ucrtbase.dll"}, types.SizesFor("gc", "amd64"))
		seen := 0

		for _, declaration := range cgoDeclarations(pkg) {
			reason, listed := want[declaration.cName]

			if !listed || declaration.kind != cgoErrnoFunc {
				continue
			}

			seen++
			text, err := set.wrapper(declaration)

			if reason != "" {
				if err == nil || !strings.Contains(err.Error(), reason) {
					t.Errorf("%s C.%s: err %v, want it to say %q", goos, declaration.cName, err, reason)
				}

				continue
			}

			if err != nil {
				t.Fatalf("%s C.%s: %v", goos, declaration.cName, err)
			}

			// The pointer crosses as 3b2b's scalar pointer does, and errno is read inside the transition: by the binding
			// on linux, through the C runtime's slot on windows.
			for _, line := range []string{"internal static (@int, error) countꓸerrno(ж<@int> p0)", "StructPointer(p0, \"count\"", "return ((@int)(int32)r, err);"} {
				if !strings.Contains(text, line) {
					t.Errorf("%s C.count's errno form lacks %q:\n%s", goos, line, text)
				}
			}

			errnoRead := map[string]string{"linux": "SetLastError = true", "windows": "errno = global::System.Runtime.InteropServices.Marshal.ReadInt32(errnoAddress);"}[goos]

			if !strings.Contains(text, errnoRead) {
				t.Errorf("%s C.count's errno form does not read errno by %q:\n%s", goos, errnoRead, text)
			}
		}

		if seen != len(want) {
			t.Errorf("%s: %d of the %d errno forms were recognized", goos, seen, len(want))
		}
	}
}

// cgoNativeCall matches a call of a binding's extern (absᴍnative(…), the errno form's …ᴍnativeerrno(…)) or of a windows
// C runtime's errno slot (errnoᴍlocation()).
var cgoNativeCall = regexp.MustCompile(`\w+` + cgoMirrorSuffix + `native(errno)?\(|` + cgoErrnoLocation + `\(\)`)

// cgoSeamCoverage reads a bindings file: how many externs it declares, how many seam brackets it opens, and each native
// call made outside one, with its line.
func cgoSeamCoverage(text string) (imports, brackets int, outside []string) {
	inside := false

	for number, line := range strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n") {
		trimmed := strings.TrimSpace(line)

		switch {
		case strings.HasPrefix(trimmed, "[global::System.Runtime.InteropServices.LibraryImport("):
			imports++
		case strings.HasSuffix(trimmed, ".syscallEntersyscall();"):
			inside = true
			brackets++
		case strings.HasSuffix(trimmed, ".syscallExitsyscall();"):
			inside = false
		case strings.Contains(trimmed, "private static partial"):
			// An extern's declaration, not a call.
		case cgoNativeCall.MatchString(trimmed) && !inside:
			outside = append(outside, fmt.Sprintf("%d: %s", number+1, trimmed))
		}
	}

	return imports, brackets, outside
}

// Seam coverage, a standing property of the bindings (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20, increment 5b):
// every native call a binding makes is inside the native-call seam's bracket (runtime's syscallEntersyscall and
// syscallExitsyscall, §11.4), so a scheduler that watches the bracket sees every C call (COORD, for M:N). Each extern is
// one wrapper's, with one bracket, except windows' errno slot, which only the errno form calls, inside its own bracket.
func TestCgoBindingsCrossTheSeamOnEveryNativeCall(t *testing.T) {
	for _, target := range cgoWrapperTargets {
		content, err := os.ReadFile(filepath.Join("..", "tests", "GolibTests", target.goldenName()))

		if err != nil {
			t.Fatal(err)
		}

		text := string(content)
		imports, brackets, outside := cgoSeamCoverage(text)
		errnoSlot := 0

		if strings.Contains(text, " "+cgoErrnoLocation+"();") {
			errnoSlot = 1
		}

		t.Logf("READING %s: %d externs, %d brackets, %d errno slot", target.goos, imports, brackets, errnoSlot)

		if imports != brackets+errnoSlot {
			t.Errorf("%s: %d externs against %d brackets and %d errno slot: an extern no wrapper brackets", target.goos, imports, brackets, errnoSlot)
		}

		for _, call := range outside {
			t.Errorf("%s: a native call outside the seam's bracket, line %s", target.goos, call)
		}

		// The check's own negative control: with one bracket's opening removed, it names the call that bracket covered.
		planted := strings.Replace(text, "global::go.runtime_package.syscallEntersyscall();", "", 1)

		if _, _, outside := cgoSeamCoverage(planted); len(outside) == 0 {
			t.Errorf("%s: the check does not name a native call whose bracket was removed", target.goos)
		}
	}
}
