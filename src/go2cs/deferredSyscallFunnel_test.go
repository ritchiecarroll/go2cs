// deferredSyscallFunnel_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the seam between the syscall-funnel interception (syscallKeepAliveAnalysis.go) and the
// defer/go temp-parameter form (visitDeferStmt/visitGoStmt), and — as with returnOperandOrder's
// controls — the SCOPE of that seam matters as much as the positive: the interception still owns
// every ORDINARY funnel call, including the uintptrkeepalive machinery it exists for.
//
// The defect this pins (measured 2026-09-02): convCallExpr intercepted EVERY funnel call before
// the general path threaded `callArgs`, and convSyscallFunnelCall takes no LambdaContext, so a
// DEFERRED funnel call rendered its arguments inside the CALL ITSELF while every eager slot stayed
// empty —
//
//	defer(syscall.Syscall(syscall.SYS_CLOSE, (uintptr)fds[i], 0, 0), , , , , ref ᒐ)
//
// CS0839 ×4, and a SEMANTIC defect underneath the compile error: `fds[i]` inside the callee's own
// argument list is read at UNWIND, where Go reads it at the defer statement. The corpus instance
// is runtime's memmove_linux_amd64_test.go:44, which is why runtime's Linux `-tests` build is the
// row-level gate.
//
// The correct emission evaluates every NON-CONSTANT argument in an eager slot and hands it to a
// temp-parameter thunk; the constant arguments render inside the thunk's call, because a constant
// has the same value whenever it is evaluated (seat #33, goDeferConstArgs.go):
//
//	defer((ᴛ1, ᴛ2) => syscall.Syscall(ᴛ1, ᴛ2, 0, 0), trap, (uintptr)fds[i], ref ᒐ)
//
// A deferred funnel call with no constant argument keeps the ordinary method-group form, binding
// golib's `defer<T1, T2, T3, T4, TResult>(Func<…>, T1, T2, T3, T4, ref GoFrame)`. The assertions
// below are deliberately arity-INDEPENDENT: `trap` and `fds[i]` are the only non-constant arguments
// on every platform, so the one fixture covers unix's 4-argument `syscall.Syscall` and Windows's
// 5-argument form with no per-platform expected string.
//
// This lives in the CONVERTER suite rather than in tests/Behavioral because the shape cannot be
// written portably in Go: `syscall.Syscall` is `(trap, a1, a2, a3)` on unix and
// `(trap, nargs, a1, a2, a3)` on Windows, so one behavioral source cannot compile on both, and a
// build-tag split would give the project a per-platform golden set — manufacturing the very
// captured-on-one-OS drift EnvironBlockWalk already costs us. The fixture below is written by the
// test, so each host writes the form its own syscall package declares and the assertions are
// derived from that arity.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// deferredSyscallFunnelFixture is the Go source both tests convert. `trap` is a variable rather
// than a platform constant so the one fixture type-checks everywhere (no host needs to declare
// SYS_CLOSE), and the deferred call's argument is read from a slice the loop MUTATES on the very
// next line — the mutation is what makes eager-vs-lazy argument evaluation observable at all.
func deferredSyscallFunnelFixture() string {
	deferArgs := "trap, uintptr(fds[i]), 0, 0"
	plainArgs := "trap, uintptr(fd), 1, 0"
	pointerArgs := "trap, 0, uintptr(unsafe.Pointer(&buf[0])), uintptr(len(buf))"

	if runtime.GOOS == "windows" {
		deferArgs = "trap, 3, uintptr(fds[i]), 0, 0"
		plainArgs = "trap, 3, uintptr(fd), 1, 0"
		pointerArgs = "trap, 3, 0, uintptr(unsafe.Pointer(&buf[0])), uintptr(len(buf))"
	}

	return `package main

import (
	"fmt"
	"syscall"
	"unsafe"
)

var trap uintptr

// POSITIVE - a deferred funnel call inside a loop whose argument CHANGES per iteration, with the
// slice mutated immediately after. Go evaluates ` + "`fds[i]`" + ` at the defer statement.
func deferredInLoop(fds []int) {
	for i := 0; i < len(fds); i++ {
		defer syscall.Syscall(` + deferArgs + `)
		fds[i] = -1
	}
}

// CONTROL A - an ORDINARY funnel call still renders its arguments inline: the interception is
// unchanged for every shape but defer/go.
func plainCall(fd int) uintptr {
	r1, _, _ := syscall.Syscall(` + plainArgs + `)
	return r1
}

// CONTROL B - an ordinary funnel call with a POINTER-DERIVED argument still routes through the
// uintptrkeepalive machinery the interception exists for: a temp holding the box, cast to uintptr
// at the call site, kept alive after the statement.
func pointerArg(buf []byte) uintptr {
	r1, _, _ := syscall.Syscall(` + pointerArgs + `)
	return r1
}

func main() {
	fds := []int{3, 4}
	deferredInLoop(fds)
	fmt.Println(plainCall(0), pointerArg(make([]byte, 8)))
}
`
}

// convertDeferredSyscallFunnelFixture converts the fixture and returns its emitted C#.
func convertDeferredSyscallFunnelFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/deferfunnel\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), deferredSyscallFunnelFixture())

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "deferfunnel", "main.cs"))
}

// deferStatementLine returns the one emitted line carrying the deferred funnel call.
func deferStatementLine(t *testing.T, mainCs string) string {
	t.Helper()

	for _, line := range strings.Split(mainCs, "\n") {
		if strings.Contains(line, "defer(") && strings.Contains(line, "syscall.Syscall") {
			return strings.TrimSpace(line)
		}
	}

	t.Fatalf("no deferred syscall.Syscall statement in emission:\n%s", mainCs)

	return ""
}

// TestDeferredSyscallFunnelCallFillsItsEagerArgumentSlots pins the positive property and both
// controls. Proven against a neuter: restoring the unconditional interception (dropping the
// `context.callArgs == nil` guard in convCallExpr) makes the positive report the malformed
// `defer(syscall.Syscall(trap, (uintptr)fds[i], 0, 0), , , ,` form on every assertion below, and
// leaves both controls passing — so the controls are measuring the scope, not carrying the verdict.
func TestDeferredSyscallFunnelCallFillsItsEagerArgumentSlots(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertDeferredSyscallFunnelFixture(t)
	deferLine := deferStatementLine(t, mainCs)

	// The non-constant arguments sit in eager slots, evaluated at the defer statement exactly as Go
	// evaluates them, and the thunk's call reads them back as temps. The defect's signature is the
	// opposite: an argument read inside the callee's own argument list, at unwind.
	//
	// The call is located rather than assumed: under the defect the thunk prefix may be ABSENT, and
	// an unguarded strings.Index would return -1 and panic the whole test on its own failure path,
	// losing the slot-count and empty-slot findings below, which are exactly the ones that describe
	// the defect.
	const thunkCall = "=> syscall.Syscall("

	callAt := strings.Index(deferLine, thunkCall)

	if callAt < 0 {
		t.Errorf("the deferred funnel call does not take the temp-parameter thunk form:\n    %s", deferLine)
	} else {
		rest := deferLine[callAt+len(thunkCall):]
		closeAt, depth := -1, 1

		for k, r := range rest {
			if r == '(' {
				depth++
			} else if r == ')' {
				if depth--; depth == 0 {
					closeAt = k
					break
				}
			}
		}

		if closeAt < 0 {
			t.Fatalf("the thunk's call does not close:\n    %s", deferLine)
		}

		args, eager := rest[:closeAt], rest[closeAt+1:]

		if strings.Contains(args, "fds") || strings.Contains(args, "trap") {
			t.Errorf("the deferred funnel call reads an argument inside the call, so at unwind rather than at the defer statement:\n    %s", deferLine)
		}

		if !strings.Contains(eager, "fds[i]") {
			t.Errorf("the per-iteration argument is not in an eager slot:\n    %s", deferLine)
		}

		// Exactly the two non-constant arguments reach a slot: the emitted statement carries that
		// many commas between the thunk and the frame argument.
		if slots := strings.Count(eager, ",") - 1; slots != 2 {
			t.Errorf("expected 2 eager argument slots, counted %d:\n    %s", slots, deferLine)
		}
	}

	// An empty argument slot is the defect's other signature, and it survives the method-group
	// check on its own — assert against it directly.
	if strings.Contains(deferLine, ", ,") {
		t.Errorf("deferred funnel call has an EMPTY argument slot (CS0839):\n    %s", deferLine)
	}

	if !strings.HasSuffix(deferLine, "ref ᒐ);") {
		t.Errorf("deferred funnel call does not close on the frame argument:\n    %s", deferLine)
	}
}

// TestOrdinarySyscallFunnelCallKeepsItsInterception is the SCOPE half: the fall-through added for
// defer/go must not have widened. An ordinary funnel call still renders its arguments inline, and
// a pointer-derived one still gets the box temp and the GC.KeepAlive that reproduce Go's
// uintptrkeepalive contract — the whole reason the interception exists.
func TestOrdinarySyscallFunnelCallKeepsItsInterception(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertDeferredSyscallFunnelFixture(t)

	// CONTROL A - arguments inline, no temp parameters anywhere near the ordinary call.
	if !strings.Contains(mainCs, "syscall.Syscall(trap, ") {
		t.Errorf("an ordinary funnel call no longer renders its arguments inline:\n%s", mainCs)
	}

	// CONTROL B - the pointer-derived argument's box is held by a temp, cast at the call site,
	// and kept alive after the statement.
	if !strings.Contains(mainCs, "System.GC.KeepAlive(ᴋ") {
		t.Errorf("the uintptrkeepalive contract is gone from an ordinary funnel call:\n%s", mainCs)
	}

	if !strings.Contains(mainCs, "(uintptr)ᴋ") {
		t.Errorf("the pointer-derived argument no longer casts its box temp at the call site:\n%s", mainCs)
	}
}
