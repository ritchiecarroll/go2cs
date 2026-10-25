// paramSkipWindowFrames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the PARAMETER-CARRIED WINDOW of computeNoInliningClosure (callerInliningAnalysis.go): a
// runtime.Caller/Callers skip that is the function's own PARAMETER is sized at each same-package call site
// that passes it a constant. log is the measured case: output calls runtime.Caller(calldepth), Fatalf
// passes 2, and the linux Release TieredCompilation=0 JIT inlined Fatalf into TestCallDepth's one-line
// closure, so the reported line moved to the closure's caller (FAIL untiered, PASS tiered, PASS untiered
// with DOTNET_JitNoInline=1).
//
// The fixture converts as a MODULE, where the fixed-skip path closure already marks every SMALL function on
// a path to a parameter skip (fixedSkipCallerClosure_test.go). So each positive here is LARGE (a loop):
// only the window can mark it, which is the standard library's case (the fixed-skip closure never applies
// there).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"testing"
)

func convertParamSkipWindowFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/paramskip\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "runtime"

var sink string

// SITE 1 -- runtime.Caller(calldepth): the skip is a PARAMETER, sized at each call site.
func output(calldepth int, s string) {
	_, file, _, _ := runtime.Caller(calldepth)
	sink = file + s
}

// POSITIVE 1 -- passes 2: output (0) and fatalf (1) are skipped, depth 2 reports. log.Fatalf's shape,
// made large so only the window marks it.
func fatalf(s string) {
	for i := 0; i < 1; i++ {
		s += "!"
	}
	output(2, s)
}

// POSITIVE 2 -- passes 3: logDeep (1) and its caller (2) are skipped.
func logDeep(s string) {
	for i := 0; i < 1; i++ {
		s += "?"
	}
	output(3, s)
}

// POSITIVE 3 -- depth 2 of POSITIVE 2's call, large.
func deepCaller(s string) {
	for i := 0; i < 1; i++ {
		s += "."
	}
	logDeep(s)
}

// CONTROL 1 -- depth 2 of POSITIVE 1 is the REPORTED frame: the caller's code, never marked.
func useFatalf() {
	for i := 0; i < 1; i++ {
		fatalf("x")
	}
}

// NEGATIVE 1 -- a NON-CONSTANT argument (log.Output's calldepth+1) sizes nothing.
func outputAt(d int, s string) {
	for i := 0; i < 1; i++ {
		s += ","
	}
	output(d+1, s)
}

// SITE 2 -- the callee REASSIGNS its skip parameter, so no call site can size it.
func shifted(calldepth int) {
	calldepth++
	_, file, _, _ := runtime.Caller(calldepth)
	sink = file
}

// NEGATIVE 2 -- passes a constant to SITE 2's reassigned parameter: not marked.
func viaShifted() {
	for i := 0; i < 1; i++ {
		shifted(2)
	}
}

// NEGATIVE 3 -- passes 1: runtime.Caller(1) reports depth 1, this frame, so nothing above output is skipped.
func reportsItself(s string) {
	for i := 0; i < 1; i++ {
		s += ";"
	}
	output(1, s)
}

func main() {
	useFatalf()
	deepCaller("y")
	outputAt(1, "z")
	viaShifted()
	reportsItself("w")
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "paramskip", "main.cs"))
}

func TestParamSkipWindowFramesAreNotInlined(t *testing.T) {
	isolateConverterPathGlobals(t)

	mainCs := convertParamSkipWindowFixture(t)

	// output and shifted call runtime.Caller directly and were marked before the window existed; the window
	// adds fatalf, logDeep and deepCaller.
	for _, name := range []string{"output", "shifted", "fatalf", "logDeep", "deepCaller"} {
		if line := declarationLine(t, mainCs, name); !keepsOwnFrame(line) {
			t.Errorf("%s is a frame the skip counts, but it is not marked: %s", name, line)
		}
	}

	for _, name := range []string{"useFatalf", "outputAt", "viaShifted", "reportsItself"} {
		if line := declarationLine(t, mainCs, name); keepsOwnFrame(line) {
			t.Errorf("%s is marked, but no skip counts its frame: %s", name, line)
		}
	}
}
