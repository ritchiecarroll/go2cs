// callerSkipWindowFrames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the SKIP WINDOW of computeNoInliningClosure (callerInliningAnalysis.go): a runtime.Caller(k) or
// runtime.Callers(k, ...) call with a CONSTANT skip counts a fixed number of frames above its caller, and
// every in-package function at one of those SKIPPED depths must keep its frame, whatever its shape. The
// thin-forwarder fixed point covers only one-statement bodies; net/http's ServeMux is the measured case
// it misses: registerErr's runtime.Caller(3) skips register (an if with a panic) and Handle (an if/else),
// the Release TieredCompilation=0 JIT inlines one of them, and TestRegisterErr's "registered at" lands on
// testing.tRunner instead of server_test.go (10/10 at TC0, 0/10 tiered).
//
// Frame model, Go's skip semantics: in F, runtime.Caller(k) skips F and its callers up to depth k-1 and
// reports depth k; runtime.Callers(k) counts itself as frame 0, so it skips F and callers up to depth
// k-2 and records from depth k-1. Only SKIPPED depths are marked: the reported frame and everything
// above it are the caller's code (variant B, which also marked the first reported frame, measured +167
// std functions the JIT never inlines). A non-constant skip cannot be sized and is out of scope.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"testing"
)

func convertCallerSkipWindowFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/skipwindow\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "runtime"

type mux struct{ loc string }

// SITE 1 -- runtime.Caller(3): registerErr (0), register (1) and Handle (2) are skipped; depth 3 reports.
func (m *mux) registerErr(p string) error {
	_, file, line, ok := runtime.Caller(3)
	if ok {
		m.loc = file
	}
	_ = line
	if p == "" {
		return nil
	}
	return nil
}

// POSITIVE 1 -- depth 1 of SITE 1, not thin (an if with a panic): net/http's ServeMux.register.
func (m *mux) register(p string) {
	if err := m.registerErr(p); err != nil {
		panic(err)
	}
}

// POSITIVE 2 -- depth 2 of SITE 1, not thin (an if/else): net/http's ServeMux.Handle.
func (m *mux) Handle(p string) {
	if p == "x" {
		m.register(p + "y")
	} else {
		m.register(p)
	}
}

// CONTROL 1 -- depth 3 of SITE 1 is the REPORTED frame: the caller's code, never marked.
func useHandle(m *mux) {
	if m == nil {
		return
	}
	m.Handle("a")
	m.Handle("b")
}

// SITE 2 -- runtime.Callers(2): only stackHere itself (depth 0) is skipped.
func stackHere(pc []uintptr) int { return runtime.Callers(2, pc) }

// CONTROL 2 -- depth 1 of SITE 2 is the first RECORDED frame, never marked.
func wrap(pc []uintptr) int {
	n := stackHere(pc)
	n++
	return n
}

// SITE 3 -- runtime.Callers(3): record (0) and its caller (1) are skipped.
func record(pc []uintptr) int {
	n := runtime.Callers(3, pc)
	return n
}

// POSITIVE 3 -- depth 1 of SITE 3, not thin (two statements before the call).
func logIt(pc []uintptr, msg string) int {
	size := len(msg)
	size += len(pc)
	return record(pc) + size
}

// SITE 4 -- a NON-CONSTANT skip cannot be sized.
func atDepth(d int) uintptr {
	var pcs [1]uintptr
	runtime.Callers(d, pcs[:])
	return pcs[0]
}

// CONTROL 3 -- a non-thin caller of SITE 4: out of scope, never marked.
func viaDepth() uintptr {
	x := atDepth(2)
	x++
	return x
}

func main() {
	m := &mux{}
	useHandle(m)
	var pc [4]uintptr
	_ = wrap(pc[:])
	_ = logIt(pc[:], "x")
	_ = viaDepth()
}
`)

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

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "skipwindow", "main.cs"))
}

func TestCallerSkipWindowFramesAreNotInlined(t *testing.T) {
	mainCs := convertCallerSkipWindowFixture(t)

	// The direct callers were marked before the window existed; the window adds register, Handle, logIt.
	for _, name := range []string{"registerErr", "register", "Handle", "stackHere", "record", "logIt", "atDepth"} {
		if line := declarationLine(t, mainCs, name); !keepsOwnFrame(line) {
			t.Errorf("%s is a frame the skip counts, but it is not marked: %s", name, line)
		}
	}

	for _, name := range []string{"useHandle", "wrap", "viaDepth"} {
		if line := declarationLine(t, mainCs, name); keepsOwnFrame(line) {
			t.Errorf("%s is marked, but no skip counts its frame: %s", name, line)
		}
	}
}
