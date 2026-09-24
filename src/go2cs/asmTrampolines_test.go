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
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// asmTrampolineFixtureGo declares one bodyless function per trampoline SHAPE; asm_linux_amd64.s
// (asmTrampolineFixtureAsm) supplies their bodies, exactly as golang.org/x/sys/unix does.
const asmTrampolineFixtureGo = `package main

import (
	"fmt"
	"syscall"
)

// Shape 1: a JMP to an EXPORTED function of another package with an IDENTICAL signature
// (x/sys/unix's Syscall): forwards.
func Syscall(trap, a1, a2, a3 uintptr) (r1, r2 uintptr, err syscall.Errno)

// Shape 2: the REAL x/sys/unix gettimeofday. The jump target is syscall·gettimeofday, but the
// parameter is this package's OWN Timeval, so the Go signatures are not identical: stays a stub.
type Timeval struct {
	Sec  int64
	Usec int64
}

func gettimeofday(tv *Timeval) (err syscall.Errno)

// Shape 3: a JMP to an UNEXPORTED function of another package: stays a stub.
func rawNoError(trap, a1, a2, a3 uintptr) (r1, r2 uintptr)

// Shape 4: real machine code (a raw SYSCALL), not a trampoline: stays a stub.
func SyscallNoError(trap, a1, a2, a3 uintptr) (r1, r2 uintptr)

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

TEXT ·gettimeofday(SB),NOSPLIT,$0-16
	JMP	syscall·gettimeofday(SB)

TEXT ·rawNoError(SB),NOSPLIT,$0-48
	JMP	syscall·rawSyscallNoError(SB)

TEXT ·SyscallNoError(SB),NOSPLIT,$0-48
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

// emittedFunction returns the emitted DECLARATION of the named function through the end of its body
// (a bodyless partial is one line). Comment lines never match, so a doc comment that mentions the name
// cannot stand in for the declaration.
func emittedFunction(t *testing.T, mainCs string, name string) string {
	t.Helper()

	lines := strings.Split(mainCs, "\n")

	for i, line := range lines {
		trimmed := strings.TrimSpace(line)

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
	}

	for name, calls := range forwards {
		body := emittedFunction(t, linux, name)

		if strings.Contains(body, " partial ") {
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

	// Shapes that must NOT forward: signatures that differ in Go types (the real x/sys gettimeofday),
	// an unexported cross-package target, real machine code, and a same-package target whose parameter
	// Phase A lowered.
	for _, name := range []string{"gettimeofday", "rawNoError", "SyscallNoError", "derefAlias"} {
		if body := emittedFunction(t, linux, name); !strings.Contains(body, " partial ") {
			t.Errorf("shape %s: must stay a partial stub:\n%s", name, body)
		}
	}

	// CONTROL: the assembly file is linux-only by its name, so a windows conversion has no
	// trampoline to read and every shape keeps its stub.
	windows := convertAsmTrampolineFixture(t, "windows/amd64")

	for _, name := range []string{"Syscall", "gettimeofday", "rawNoError", "SyscallNoError", "localAlias", "derefAlias"} {
		if body := emittedFunction(t, windows, name); !strings.Contains(body, " partial ") {
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
