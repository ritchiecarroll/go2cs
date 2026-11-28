// syscallCallbackAot_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// syscall.NewCallback under Native AOT (COORD 220506Z; docs/phase4/DESIGN-newcallback-native-aot.md). compileCallback
// (src/core/syscall/windows/syscall_windows_callback_impl.cs) closed its per-arity binder with MakeGenericType, so a
// Native AOT program had no native code for it and died at its first NewCallback: modernc libc's init reached it as
// GoCallbackBinder1<uintptr,uintptr>. The remedy registers each func type from the call site (go2cs-gen's
// NewCallbackShapeGenerator), so this row's program is Go, converted with -recurse as a user's module is, and its
// projects reference go2cs-gen as every converted project does. It runs under Go (the reference), the JIT and Native
// AOT, and prints one line per shape:
//   - one callback per arity syscall implements, through a real native caller (user32's CallWindowProcW for four
//     words, SyscallN for none, one and two), and the identity of two NewCallback calls on one func;
//   - one line per argument shape the converter emits (a literal, a local variable, a field of a named func type, a
//     func returned by another package, a func through a package that takes any as x/sys/windows does, a method
//     value), each with a word type no other line names, so each line proves its own registration, and one func type
//     registered by two packages;
//   - a func already in an any: the JIT binds it by reflection, Native AOT refuses it by name;
//   - Go's refusals, which must read the same under both runtimes.
// Go and the JIT print the same lines but arity 3, which go2cs refuses as its own limit; Native AOT prints what the
// JIT prints but the any line.

var callbackAotFixture = map[string]string{
	"go.mod":         "module example.com/callbackaot\n\ngo 1.26\n",
	"main.go":        callbackAotMain,
	"cbpkg/cbpkg.go": callbackAotCbpkg,
	"fwd/fwd.go":     callbackAotFwd,
}

const callbackAotMain = `package main

import (
	"fmt"
	"syscall"

	"example.com/callbackaot/cbpkg"
	"example.com/callbackaot/fwd"
)

// One word type per shape line, so each line proves its own registration.
type (
	litW uintptr
	locW uintptr
	fwdW uintptr
	winW uintptr
	anyW uintptr
)

type cbT func(p uintptr) uintptr

type holder struct{ fn cbT }

type big struct{ a, b uint64 }

type window struct{ base uintptr }

func (w *window) wndProc(p winW) uintptr { return w.base + uintptr(p) }

func none() uintptr                                  { return 7 }
func one(a uintptr) uintptr                          { return a + 1 }
func two(a, b uintptr) uintptr                       { return a * b }
func four(hwnd, msg, wparam, lparam uintptr) uintptr { return hwnd + msg + wparam + lparam }
func local(p locW) uintptr                           { return uintptr(p) + 3 }

func line(name string, body func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(name, "panics:", r)
		}
	}()

	fmt.Println(name, "=", body())
}

func call1(cb uintptr) string {
	r, _, _ := syscall.SyscallN(cb, 41)
	return fmt.Sprint(r)
}

func main() {
	callWindowProc := syscall.NewLazyDLL("user32.dll").NewProc("CallWindowProcW")

	line("arity 0", func() string { r, _, _ := syscall.SyscallN(syscall.NewCallback(none)); return fmt.Sprint(r) })
	line("arity 1", func() string { return call1(syscall.NewCallback(one)) })
	line("arity 2", func() string { r, _, _ := syscall.SyscallN(syscall.NewCallback(two), 6, 7); return fmt.Sprint(r) })
	line("arity 4", func() string {
		r, _, _ := callWindowProc.Call(syscall.NewCallback(four), 1, 2, 3, 4)
		return fmt.Sprint(r)
	})
	line("same func, same pointer", func() string {
		return fmt.Sprint(syscall.NewCallback(one) == syscall.NewCallback(one))
	})

	line("literal", func() string { return call1(syscall.NewCallback(func(p litW) uintptr { return uintptr(p) + 2 })) })
	f := local
	line("local variable", func() string { return call1(syscall.NewCallback(f)) })
	h := holder{fn: func(p uintptr) uintptr { return p + 4 }}
	line("named func type field", func() string { return call1(syscall.NewCallback(h.fn)) })
	line("returned by another package", func() string { return call1(syscall.NewCallback(cbpkg.Make())) })
	line("through a forwarder", func() string {
		return call1(fwd.NewCallback(func(p fwdW) uintptr { return uintptr(p) + 6 }))
	})
	w := &window{base: 9}
	line("method value", func() string { return call1(syscall.NewCallback(w.wndProc)) })
	line("registered by two packages", func() string { return call1(cbpkg.Pointer()) })
	var x any = func(p anyW) uintptr { return uintptr(p) + 7 }
	line("through any", func() string { return call1(syscall.NewCallback(x)) })

	line("no result", func() string { return fmt.Sprint(syscall.NewCallback(func(p uintptr) {})) })
	line("float argument", func() string { return fmt.Sprint(syscall.NewCallback(func(v float64) uintptr { return 0 })) })
	line("float result", func() string { return fmt.Sprint(syscall.NewCallback(func() float64 { return 0 })) })
	line("oversized argument", func() string { return fmt.Sprint(syscall.NewCallback(func(v big) uintptr { return 0 })) })
	line("wrong-size result", func() string { return fmt.Sprint(syscall.NewCallback(func() uint32 { return 0 })) })
	line("non-function", func() string { return fmt.Sprint(syscall.NewCallback(42)) })
	line("arity 3", func() string { return fmt.Sprint(syscall.NewCallback(func(a, b, c uintptr) uintptr { return 0 })) })
}
`

const callbackAotCbpkg = `package cbpkg

import "syscall"

// Word is a word type only this package's func names, so its shape is registered by no other site.
type Word uintptr

// Make returns a func the caller passes to NewCallback as a value of another package's func type.
func Make() func(p Word) uintptr { return func(p Word) uintptr { return uintptr(p) + 5 } }

// Pointer makes its own callback of a func type main also passes, so two assemblies register one type.
func Pointer() uintptr { return syscall.NewCallback(func(p uintptr) uintptr { return p + 8 }) }
`

const callbackAotFwd = `package fwd

import "syscall"

// NewCallback passes its own any parameter straight to syscall, as golang.org/x/sys/windows does.
func NewCallback(fn any) uintptr { return syscall.NewCallback(fn) }
`

// The lines Go, the JIT and Native AOT agree on, in order; arity 3 and through any are checked per runtime.
var callbackAotCommon = []string{
	"arity 0 = 7",
	"arity 1 = 42",
	"arity 2 = 42",
	"arity 4 = 10",
	"same func, same pointer = true",
	"literal = 43",
	"local variable = 44",
	"named func type field = 45",
	"returned by another package = 46",
	"through a forwarder = 47",
	"method value = 50",
	"registered by two packages = 49",
	"no result panics: compileCallback: expected function with one uintptr-sized result",
	"float argument panics: compileCallback: float arguments not supported",
	"float result panics: compileCallback: float results not supported",
	"oversized argument panics: compileCallback: argument size is larger than uintptr",
	"wrong-size result panics: compileCallback: expected function with one uintptr-sized result",
	"non-function panics: compileCallback: expected function with one uintptr-sized result",
}

const (
	callbackAotAnyAt    = 12 // the through-any line follows the common lines before it
	callbackAotAnyJit   = "through any = 48"
	callbackAotAnyAot   = "through any panics: compileCallback: no native code for callback type "
	callbackAotArity3Go = "arity 3 = "
	callbackAotArity3   = "arity 3 panics: compileCallback: 3-argument callbacks are not implemented in this port; " +
		"arities 0, 1, 2 and 4 are (docs/phase4/DESIGN-managed-newcallback.md §12)"
)

func TestSyscallNewCallbackNativeAot(t *testing.T) {
	if runtime.GOOS != "windows" || runtime.GOARCH != "amd64" {
		t.Skipf("syscall.NewCallback is windows only, and the row reads windows/amd64: NOT MEASURED on %s/%s", runtime.GOOS,
			runtime.GOARCH)
	}

	// A conversion, a Release build and a Native AOT publish of the converted closure: about 22 minutes on a hosted
	// windows runner, past a plain go test's budget, so it runs where the cgo-e2e stage turns it on.
	if os.Getenv(testsE2EEnv) != "1" {
		t.Skipf("opt-in (%s=1, the cgo-e2e stage): converts, builds and publishes the row's module with Native AOT",
			testsE2EEnv)
	}

	dotnet, err := exec.LookPath("dotnet")

	if err != nil {
		t.Fatalf("%s=1 needs the .NET SDK on PATH: %v", testsE2EEnv, err)
	}

	src, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	module := t.TempDir()

	for name, contents := range callbackAotFixture {
		if err := os.MkdirAll(filepath.Dir(filepath.Join(module, name)), 0o755); err != nil {
			t.Fatal(err)
		}

		writeCgoFixture(t, module, name, contents)
	}

	env := append(os.Environ(), "GOWORK=off", "GOFLAGS=", "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1",
		"MSBUILDDISABLENODEREUSE=1")

	// The reference: Go itself.
	goRun := exec.Command("go", "run", ".")
	goRun.Dir = module
	goRun.Env = env
	goOut, err := goRun.CombinedOutput()

	if err != nil {
		t.Fatalf("go run failed: %v\n%s", err, goOut)
	}

	output := t.TempDir()
	convert := exec.Command(driverBinary(t), "-recurse", "-go2cspath", src, module, output)
	convert.Env = env

	if combined, err := convert.CombinedOutput(); err != nil {
		t.Fatalf("converting the row's module failed: %v\n%s", err, lastLines(string(combined), 40))
	}

	project := filepath.Join(output, "src", "example.com", "callbackaot", "example.com.callbackaot.csproj")
	properties := []string{"-p:GoTargetOS=windows", "-p:UseSharedCompilation=false"}

	sdk := func(step string, args ...string) {
		cmd := exec.Command(dotnet, append(append(args, project), properties...)...)
		cmd.Env = env
		out, err := cmd.CombinedOutput()

		if err != nil {
			t.Fatalf("%s: dotnet %s failed: %v\n%s", step, strings.Join(args, " "), err, lastLines(string(out), 40))
		}
	}

	run := func(arm, exe string) []string {
		out, err := exec.Command(exe).CombinedOutput()
		text := strings.ReplaceAll(string(out), "\r\n", "\n")

		var exit *exec.ExitError

		switch {
		case err == nil:
			t.Logf("READING %s: %q (exit 0)", arm, text)
		case errors.As(err, &exit):
			t.Logf("READING %s: %q (exit %#x)", arm, text, uint32(exit.ExitCode()))
		default:
			t.Fatalf("%s: the probe did not run: %v", arm, err)
		}

		return strings.Split(strings.TrimSuffix(text, "\n"), "\n")
	}

	// check compares one run's lines against the common lines, in order, with the any and arity 3 lines held to
	// what that runtime must print.
	check := func(arm string, lines []string, anyLine, arity3 string, anyPrefix bool) {
		want := append(append([]string{}, callbackAotCommon[:callbackAotAnyAt]...), anyLine)
		want = append(append(want, callbackAotCommon[callbackAotAnyAt:]...), arity3)

		if len(lines) != len(want) {
			t.Errorf("%s: %d lines, want %d:\n%s", arm, len(lines), len(want), strings.Join(lines, "\n"))
			return
		}

		for i, line := range lines {
			prefix := (i == callbackAotAnyAt && anyPrefix) || (i == len(want)-1 && arity3 == callbackAotArity3Go)

			if prefix && !strings.HasPrefix(line, want[i]) || !prefix && line != want[i] {
				t.Errorf("%s line %d: read %q, want %q", arm, i+1, line, want[i])
			}
		}
	}

	goLines := strings.Split(strings.TrimSuffix(strings.ReplaceAll(string(goOut), "\r\n", "\n"), "\n"), "\n")
	t.Logf("READING go: %q", strings.Join(goLines, "\n"))
	check("go", goLines, callbackAotAnyJit, callbackAotArity3Go, false)

	sdk("JIT build", "build", "-c", "Release", "-nologo", "-o", filepath.Join(output, "jit"))
	check("jit", run("jit", filepath.Join(output, "jit", "callbackaot.exe")), callbackAotAnyJit, callbackAotArity3, false)

	sdk("Native AOT publish", "publish", "-c", "Release", "-r", "win-x64", "-nologo", "-p:PublishAot=true", "-o",
		filepath.Join(output, "aot"))
	check("aot", run("aot", filepath.Join(output, "aot", "callbackaot.exe")), callbackAotAnyAot, callbackAotArity3, true)
}
