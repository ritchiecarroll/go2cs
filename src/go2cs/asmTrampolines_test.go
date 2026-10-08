// asmTrampolines_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"go/token"
	"go/types"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestLayoutIdenticalStructs pins the field-by-field proof the pointer bridge rests on, in both
// directions: the layout-identical Timeval passes, and every difference the bridge could not copy
// faithfully is refused.
func TestLayoutIdenticalStructs(t *testing.T) {
	sizes := types.SizesFor("gc", "amd64")

	field := func(name string, typ types.Type) *types.Var {
		return types.NewField(token.NoPos, nil, name, typ, false)
	}

	int32T, int64T := types.Typ[types.Int32], types.Typ[types.Int64]
	timeval := types.NewStruct([]*types.Var{field("Sec", int64T), field("Usec", int64T)}, nil)

	cases := []struct {
		name  string
		other *types.Struct
		want  bool
	}{
		{"identical", types.NewStruct([]*types.Var{field("Sec", int64T), field("Usec", int64T)}, nil), true},
		{"a narrower field type", types.NewStruct([]*types.Var{field("Sec", int32T), field("Usec", int64T)}, nil), false},
		{"swapped field names", types.NewStruct([]*types.Var{field("Usec", int64T), field("Sec", int64T)}, nil), false},
		{"an extra field", types.NewStruct([]*types.Var{field("Sec", int64T), field("Usec", int64T), field("Pad", int64T)}, nil), false},
		{"a pointer field", types.NewStruct([]*types.Var{field("Sec", int64T), field("Usec", types.NewPointer(int64T))}, nil), false},
	}

	for _, c := range cases {
		fields, got := layoutIdenticalStructs(timeval, c.other, sizes)

		if got != c.want {
			t.Errorf("%s: layoutIdenticalStructs = %v, want %v", c.name, got, c.want)
		}

		if got && strings.Join(fields, ",") != "Sec,Usec" {
			t.Errorf("%s: fields to copy = %v, want [Sec Usec]", c.name, fields)
		}
	}

	// A blank field has no name to copy by, so even two identical structs carrying one are refused.
	blank := types.NewStruct([]*types.Var{field("_", int64T), field("Usec", int64T)}, nil)

	if _, got := layoutIdenticalStructs(blank, blank, sizes); got {
		t.Errorf("a struct with a blank field was proven layout-identical; its field cannot be copied by name")
	}
}

// TestPackageFuncAccessPublicizesAsmJumpTargets pins the other half of an asmJumpForwardTargets row:
// the listed target is emitted public in its OWN package, and nothing else moves.
func TestPackageFuncAccessPublicizesAsmJumpTargets(t *testing.T) {
	saved := currentPackagePath
	defer func() { currentPackagePath = saved }()

	currentPackagePath = "syscall"

	if got := packageFuncAccess("gettimeofday", true); got != "public" {
		t.Errorf("syscall.gettimeofday: access %q, want public (it has an asmJumpForwardTargets row)", got)
	}

	if got := packageFuncAccess("settimeofday", true); got == "public" {
		t.Errorf("syscall.settimeofday was publicized without a row")
	}

	currentPackagePath = "example.com/other"

	if got := packageFuncAccess("gettimeofday", true); got == "public" {
		t.Errorf("another package's gettimeofday was publicized by syscall's row")
	}
}

// asmTrampolineFixtureGo declares one bodyless function per trampoline SHAPE; asm_linux_amd64.s
// (asmTrampolineFixtureAsm) supplies their bodies, exactly as golang.org/x/sys/unix does.
const asmTrampolineFixtureGo = `package main

import (
	"fmt"
	"sync/atomic"
	"syscall"
)

// Shape 1: a JMP to an EXPORTED function of another package with an IDENTICAL signature
// (x/sys/unix's Syscall): forwards.
func Syscall(trap, a1, a2, a3 uintptr) (r1, r2 uintptr, err syscall.Errno)

// Shape 1b: a JMP to an EXPORTED function of a DIRECTLY imported package whose signature is NOT
// identical (runtime/internal/atomic's LoadUintptr -> Load64 shape: *uintptr against *uint64): the
// frames agree, the Go types do not, so it stays a stub.
func LoadUintptr(addr *uintptr) uintptr

var _ = atomic.LoadUint64

// Shape 2: the REAL x/sys/unix gettimeofday. The jump target is the UNEXPORTED syscall·gettimeofday,
// authorized by its asmJumpForwardTargets row, and the parameter is this package's OWN Timeval, whose
// layout is identical to syscall's field by field: forwards through a bridge box.
type Timeval struct {
	Sec  int64
	Usec int64
}

func gettimeofday(tv *Timeval) (err syscall.Errno)

// Shape 2b: the same authorized jump over a Timeval whose FIELD TYPE differs: stays a stub.
type TimevalNarrow struct {
	Sec  int32
	Usec int64
}

func gettimeofdayNarrow(tv *TimevalNarrow) (err syscall.Errno)

// Shape 2c: the same authorized jump over a Timeval whose FIELD NAMES are swapped (same types and
// offsets): stays a stub, because a field copy by position would not be a copy by meaning.
type TimevalSwapped struct {
	Usec int64
	Sec  int64
}

func gettimeofdaySwapped(tv *TimevalSwapped) (err syscall.Errno)

// Shape 3: a JMP to an UNEXPORTED function of another package with no asmJumpForwardTargets row:
// stays a stub.
func rawNoError(trap, a1, a2, a3 uintptr) (r1, r2 uintptr)

// Shape 4: the REAL x/sys/unix raw-SYSCALL NoError blocks: SyscallNoError (bracketed by
// entersyscall/exitsyscall) forwards to syscall.Syscall, RawSyscallNoError to syscall.RawSyscall, each
// dropping the target's errno.
func SyscallNoError(trap, a1, a2, a3 uintptr) (r1, r2 uintptr)

func RawSyscallNoError(trap, a1, a2, a3 uintptr) (r1, r2 uintptr)

// Shape 4b: real machine code that is NOT one of those exact blocks: stays a stub.
func SyscallNoErrorShort(trap, a1, a2, a3 uintptr) (r1, r2 uintptr)

// Shape 5: a JMP within the same package to a target whose parameters stay boxed: forwards.
func localAlias(x int) int

func local(x int) int { return x + 1 }

// Shape 6: a JMP within the same package to a target Phase A ref-lowers: stays a stub.
func derefAlias(p *int) int

func deref(p *int) int { return *p }

func main() {
	n := 1
	fmt.Println(localAlias(1), deref(&n))
}
`

const asmTrampolineFixtureAsm = `// Copyright notice.

#include "textflag.h"

/* A block comment
   TEXT ·NotAFunction(SB),NOSPLIT,$0
   JMP syscall·Syscall(SB) */

TEXT ·Syscall(SB),NOSPLIT,$0-56
	JMP	syscall·Syscall(SB)

TEXT ·LoadUintptr(SB),NOSPLIT,$0-16
	JMP	sync∕atomic·LoadUint64(SB)

TEXT ·gettimeofday(SB),NOSPLIT,$0-16
	JMP	syscall·gettimeofday(SB)

TEXT ·gettimeofdayNarrow(SB),NOSPLIT,$0-16
	JMP	syscall·gettimeofday(SB)

TEXT ·gettimeofdaySwapped(SB),NOSPLIT,$0-16
	JMP	syscall·gettimeofday(SB)

TEXT ·rawNoError(SB),NOSPLIT,$0-48
	JMP	syscall·rawSyscallNoError(SB)

TEXT ·SyscallNoError(SB),NOSPLIT,$0-48
	CALL	runtime·entersyscall(SB)
	MOVQ	a1+8(FP), DI
	MOVQ	a2+16(FP), SI
	MOVQ	a3+24(FP), DX
	MOVQ	$0, R10
	MOVQ	$0, R8
	MOVQ	$0, R9
	MOVQ	trap+0(FP), AX	// syscall entry
	SYSCALL
	MOVQ	AX, r1+32(FP)
	MOVQ	DX, r2+40(FP)
	CALL	runtime·exitsyscall(SB)
	RET

TEXT ·RawSyscallNoError(SB),NOSPLIT,$0-48
	MOVQ	a1+8(FP), DI
	MOVQ	a2+16(FP), SI
	MOVQ	a3+24(FP), DX
	MOVQ	$0, R10
	MOVQ	$0, R8
	MOVQ	$0, R9
	MOVQ	trap+0(FP), AX	// syscall entry
	SYSCALL
	MOVQ	AX, r1+32(FP)
	MOVQ	DX, r2+40(FP)
	RET

TEXT ·SyscallNoErrorShort(SB),NOSPLIT,$0-48
	MOVQ	a1+8(FP), DI
	MOVQ	trap+0(FP), AX	// syscall entry
	SYSCALL
	MOVQ	AX, r1+32(FP)
	MOVQ	DX, r2+40(FP)
	RET

TEXT ·localAlias(SB),NOSPLIT,$0-16
	JMP	·local(SB)

TEXT ·derefAlias(SB),NOSPLIT,$0-16
	JMP	·deref(SB)
`

// convertAsmTrampolineFixture converts the fixture for target and returns its emitted main.cs.
func convertAsmTrampolineFixture(t *testing.T, target string) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/tramp\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), asmTrampolineFixtureGo)
	writeModuleFile(t, filepath.Join(appDir, "asm_linux_amd64.s"), asmTrampolineFixtureAsm)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      target,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule (%s): %v", target, err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "tramp", "main.cs")), "\r\n", "\n")
}

// isBodylessStub reports whether an emittedFunction result is a bodyless partial STUB (one line ending in `;`),
// which is what a shape that does not forward keeps. A forwarder is a partial method too since section 11 row 6
// (`/*linkname*/ ... static partial ...(...) {`), so the word `partial` alone no longer tells them apart.
func isBodylessStub(body string) bool {
	return strings.Contains(body, " partial ") && strings.HasSuffix(strings.TrimSpace(body), ";")
}

// emittedFunction returns the emitted DECLARATION of the named function through the end of its body
// (a bodyless partial is one line). Comment lines never match, so a doc comment that mentions the name
// cannot stand in for the declaration; a forwarder's `/*linkname*/` marker opens its declaration line.
func emittedFunction(t *testing.T, mainCs string, name string) string {
	t.Helper()

	lines := strings.Split(mainCs, "\n")

	for i, line := range lines {
		trimmed := strings.TrimPrefix(strings.TrimSpace(line), linknameMarker+" ")

		if strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "*") || strings.HasPrefix(trimmed, "/*") {
			continue
		}

		if !strings.Contains(line, " static ") || !strings.Contains(line, " "+name+"(") {
			continue
		}

		if strings.HasSuffix(trimmed, ";") {
			return line
		}

		var body strings.Builder

		for _, next := range lines[i:] {
			body.WriteString(next)
			body.WriteString("\n")

			if next == "}" {
				break
			}
		}

		return body.String()
	}

	t.Fatalf("no emitted declaration of %s:\n%s", name, mainCs)

	return ""
}

// TestAsmTrampolinesForwardByShape drives the EMISSION for each trampoline shape. Before
// asmTrampolines.go every one of them was a `partial` stub completed by a throwing
// PartialStubGenerator body; golang.org/x/sys/unix's Syscall is shape 1, and go-isatty reaching it
// inside fatih/color's type initializer is what killed the README walkthrough's app on Linux.
func TestAsmTrampolinesForwardByShape(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the module's closure via go/packages")
	}

	linux := convertAsmTrampolineFixture(t, "linux/amd64")

	// The exact forwarder lines, not a substring a stub or a comment could also contain.
	forwards := map[string][]string{
		"Syscall": {
			"var (ᴛ1, ᴛ2, ᴛ3) = syscall.Syscall((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3);",
			"return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2, (syscall.Errno)(uintptr)ᴛ3);",
		},
		"localAlias": {
			"return local(x);",
		},
		// The raw-SYSCALL NoError blocks forward with the target's errno dropped.
		"SyscallNoError": {
			"var (ᴛ1, ᴛ2, ᴛ3) = syscall.Syscall((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3);",
			"return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2);",
		},
		"RawSyscallNoError": {
			"var (ᴛ1, ᴛ2, ᴛ3) = syscall.RawSyscall((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3);",
			"return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2);",
		},
		// The authorized jump to the unexported syscall.gettimeofday, bridged field by field: copied in,
		// called, copied back, and a nil pointer passes a nil box. A nested line carries its own extra
		// indent.
		"gettimeofday": {
			"ж<syscall.Timeval> ᴛb1 = default!;",
			"if (tv != nil) {",
			"    ref var ᴛv1 = ref heap(new syscall.Timeval(), out ᴛb1);",
			"    ᴛv1.Sec = tv.Value.Sec;",
			"    ᴛv1.Usec = tv.Value.Usec;",
			"var ᴛ1 = syscall.gettimeofday(ᴛb1);",
			"    tv.Value.Sec = ᴛb1.Value.Sec;",
			"    tv.Value.Usec = ᴛb1.Value.Usec;",
			"return (syscall.Errno)(uintptr)ᴛ1;",
		},
	}

	for name, calls := range forwards {
		body := emittedFunction(t, linux, name)

		if isBodylessStub(body) {
			t.Errorf("shape %s: still a partial stub, not a forwarder:\n%s", name, body)
		}

		for _, call := range calls {
			if !strings.Contains(body, "\n"+strings.Repeat(" ", 4)+call+"\n") {
				t.Errorf("shape %s: the forwarder body does not carry the line %q:\n%s", name, call, body)
			}
		}
	}

	// The shape-6 arm is only meaningful if Phase A really lowered deref's parameter; a fixture that
	// stopped lowering would make "stays a stub" pass for the wrong reason.
	if deref := emittedFunction(t, linux, "deref"); !strings.Contains(deref, "ref nint p") {
		t.Fatalf("precondition: Phase A no longer lowers deref's parameter, so shape 6 proves nothing:\n%s", deref)
	}

	// Shapes that must NOT forward: signatures that differ in Go types beyond a layout-identical pointee
	// (a pointee of another element type; Timevals whose field TYPE or field NAMES differ), an unexported
	// cross-package target with no asmJumpForwardTargets row, machine code that is not an exact NoError
	// block, and a same-package target whose parameter Phase A lowered.
	for _, name := range []string{"LoadUintptr", "gettimeofdayNarrow", "gettimeofdaySwapped", "rawNoError", "SyscallNoErrorShort", "derefAlias"} {
		if body := emittedFunction(t, linux, name); !isBodylessStub(body) {
			t.Errorf("shape %s: must stay a partial stub:\n%s", name, body)
		}
	}

	// CONTROL: the assembly file is linux-only by its name, so a windows conversion has no
	// trampoline to read and every shape keeps its stub.
	windows := convertAsmTrampolineFixture(t, "windows/amd64")

	for _, name := range []string{"Syscall", "LoadUintptr", "gettimeofday", "rawNoError", "SyscallNoError", "RawSyscallNoError", "localAlias", "derefAlias"} {
		if body := emittedFunction(t, windows, name); !isBodylessStub(body) {
			t.Errorf("windows control: %s forwarded although its assembly is not in the windows build:\n%s", name, body)
		}
	}
}

// TestIsGoRootSourceDir pins the scope exclusion in both directions: a standard-library package
// directory is excluded, and nothing merely NEAR GOROOT's source tree is.
func TestIsGoRootSourceDir(t *testing.T) {
	goRoot := filepath.Join(t.TempDir(), "go")

	cases := []struct {
		dir  string
		want bool
	}{
		{filepath.Join(goRoot, "src", "syscall"), true},
		{filepath.Join(goRoot, "src", "vendor", "golang.org", "x", "sys", "cpu"), true},
		{filepath.Join(goRoot, "src"), true},
		{goRoot, false},
		{filepath.Join(goRoot, "srcx", "pkg"), false},
		{filepath.Join(goRoot, "pkg", "mod", "golang.org", "x", "sys@v0.25.0", "unix"), false},
		{filepath.Join(filepath.Dir(goRoot), "elsewhere", "src", "syscall"), false},
	}

	for _, testCase := range cases {
		if got := isGoRootSourceDir(testCase.dir, goRoot); got != testCase.want {
			t.Errorf("isGoRootSourceDir(%q) = %v, want %v", testCase.dir, got, testCase.want)
		}
	}

	if isGoRootSourceDir(filepath.Join(goRoot, "src", "syscall"), "") {
		t.Error("with no GOROOT known, nothing may be excluded")
	}
}

// TestParseAsmTrampolines pins the parser's reading of the source forms the emission arm does not
// exercise: a division-slash import path, arm64's `B`, a label, symbols that are not Go functions of
// this package, preprocessor conditionals, and the order comments are stripped in.
func TestParseAsmTrampolines(t *testing.T) {
	source := `#include "textflag.h"
#define NOSPLIT_ALIAS 4
TEXT ·IndexByte(SB),NOSPLIT,$0-40
	JMP	internal∕bytealg·IndexByte(SB)

TEXT ·Load(SB),NOSPLIT,$0-12
	B	·Load32(SB)

TEXT ·Labeled(SB),NOSPLIT,$0
loop:
	JMP	runtime·procyield(SB)

TEXT libc_getpid_trampoline<>(SB),NOSPLIT,$0-0
	JMP	libc_getpid(SB)

TEXT ·Twice(SB),NOSPLIT,$0
	JMP	·a(SB)
	JMP	·b(SB)

TEXT ·Conditional(SB),NOSPLIT,$0
#ifdef GOAMD64_v3
	JMP	·fast(SB)
#endif

TEXT ·LineCommentFirst(SB),NOSPLIT,$0 // a /* in a line comment opens nothing
	JMP	·c(SB)

TEXT ·BlockCommentFirst(SB),NOSPLIT,$0 /* a // in a block comment ends nothing */
	JMP	·d(SB)

TEXT ·Last(SB),NOSPLIT,$0
	JMP	·a(SB)
GLOBL ·data(SB), RODATA, $8
`

	got := parseAsmTrampolines(source)

	want := map[string]string{
		"IndexByte":         "internal/bytealg.IndexByte",
		"Load":              ".Load32",
		"Labeled":           "runtime.procyield",
		"LineCommentFirst":  ".c",
		"BlockCommentFirst": ".d",
		"Last":              ".a",
	}

	for name, target := range want {
		if got[name] != target {
			t.Errorf("%s: target %q, want %q", name, got[name], target)
		}
	}

	for _, name := range []string{"Twice", "libc_getpid_trampoline", "Conditional"} {
		if target, found := got[name]; found {
			t.Errorf("%s is not a pure-JMP trampoline of this package, yet parsed to %q", name, target)
		}
	}

	if len(got) != len(want) {
		t.Errorf("parsed %d trampolines, want %d: %v", len(got), len(want), got)
	}
}

// TestAsmTrampolinesSkipGoRootPackages is the WIRING arm for the scope exclusion: the same fixture,
// converted as one package from under a GOROOT's src/, must keep every trampoline a stub, because the
// corpus governs its own assembly with hand-owns. options.goRoot alone moves (the toolchain the
// loader runs is untouched), and the identical conversion with the real GOROOT is the control that
// proves the fixture forwards when the exclusion does not apply.
func TestAsmTrampolinesSkipGoRootPackages(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the package via go/packages")
	}

	realGoRoot := build.Default.GOROOT

	if realGoRoot == "" {
		realGoRoot = runtime.GOROOT()
	}

	convert := func(goRoot string, pkgDir string) string {
		t.Helper()

		writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/tramp\n\ngo 1.23\n")
		writeModuleFile(t, filepath.Join(pkgDir, "main.go"), asmTrampolineFixtureGo)
		writeModuleFile(t, filepath.Join(pkgDir, "asm_linux_amd64.s"), asmTrampolineFixtureAsm)

		options := Options{
			goRoot:              goRoot,
			goPath:              build.Default.GOPATH,
			go2csPath:           filepath.Join(filepath.Dir(pkgDir), "runtime"),
			targetPlatform:      "linux/amd64",
			indentSpaces:        4,
			preferVarDecl:       true,
			useChannelOperators: true,
		}

		outDir := filepath.Join(filepath.Dir(pkgDir), "out")

		if err := processConversion(pkgDir, true, outDir, options); err != nil {
			t.Fatalf("processConversion (goRoot %s): %v", goRoot, err)
		}

		return strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "main.cs")), "\r\n", "\n")
	}

	// CONTROL: outside GOROOT the exported identical shape forwards.
	control := convert(realGoRoot, filepath.Join(t.TempDir(), "tramp"))

	if body := emittedFunction(t, control, "Syscall"); isBodylessStub(body) {
		t.Fatalf("control: outside GOROOT the Syscall trampoline must forward, so this arm proves nothing:\n%s", body)
	}

	// The same control for the authorized unexported jump: outside GOROOT gettimeofday forwards.
	if body := emittedFunction(t, control, "gettimeofday"); isBodylessStub(body) {
		t.Fatalf("control: outside GOROOT the authorized gettimeofday jump must forward, so this arm proves nothing:\n%s", body)
	}

	// Under options.goRoot/src the same package is corpus code: every shape keeps its stub. That
	// includes gettimeofday, whose asmJumpForwardTargets row authorizes a jump from OUTSIDE GOROOT only:
	// a jump inside the standard library to an unexported symbol still needs Go's //go:linkname handle.
	fakeGoRoot := t.TempDir()
	underGoRoot := convert(fakeGoRoot, filepath.Join(fakeGoRoot, "src", "tramp"))

	for _, name := range []string{"Syscall", "localAlias", "gettimeofday", "SyscallNoError"} {
		if body := emittedFunction(t, underGoRoot, name); !isBodylessStub(body) {
			t.Errorf("%s forwarded in a package under GOROOT's src/:\n%s", name, body)
		}
	}
}
