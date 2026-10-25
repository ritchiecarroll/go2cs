// tailCalledFuncValue_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the tail-call seat's lever B (markTailCalledObservingValues, callerInliningAnalysis.go): a function or func
// literal whose LAST statement calls through a function VALUE whose static target observes its caller is emitted
// no-inline, because a no-inline method makes no implicit tail call, and the target's own mark cannot keep this frame
// (the exact callee of a delegate call is Invoke). testing's TestTBHelper is the measured case: testHelper ends
// `genericIntHelper(t, "GenericInt")`, and on linux the tail call erased testHelper's frame.
//
// Every fixture function opens with a println, so none is a one-statement thin or opaque forwarder: those the
// analysis marks on its own account in a package with a runtime.Caller user, and a positive marked that way would pass
// without lever B. The CONTROLS carry the scope: the same value called NOT last; a value whose target observes nothing; a parameter,
// which has no static target. (A DIRECT call to where is no control here: where's runtime.Caller(1) skip window marks its
// callers on its own account, and go2cs-gen's [DynamicSecurityMethod] on where keeps them against the tail call.)

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertTailCalledFuncValueFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/tailcalledvalue\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"runtime"
	"testing"
)

// where observes its caller: runtime.Caller(1) names the frame that called it.
func where() (string, int) {
	_, file, line, _ := runtime.Caller(1)
	return file, line
}

func plain() int { return 1 }

var whereValue = where
var plainValue = plain

// POSITIVE 1 -- a declaration ending with a call through a package var whose target observes its caller.
func viaPackageValue() {
	println("one")
	whereValue()
}

// POSITIVE 2 -- the same call as the returned result.
func returnsViaPackageValue() (string, int) {
	println("two")
	return whereValue()
}

// POSITIVE 3 -- a local t.Helper closure, called last: its t.Log names viaLocalValue's line.
func viaLocalValue(t *testing.T) {
	println("three")
	check := func() {
		t.Helper()
		t.Log("checked")
	}
	check()
}

// POSITIVE 4 -- a func literal ending with a call through the observing value.
func literalCaller() func() {
	return func() {
		println("in literal")
		whereValue()
	}
}

// CONTROL 1 -- the observing value called, but not last.
func notLast() int {
	println("five")
	whereValue()
	return 0
}

// CONTROL 2 -- a tail call through a value whose target observes nothing.
func viaPlainValue() int {
	println("six")
	return plainValue()
}

// CONTROL 3 -- a tail call through a parameter: no static target to judge.
func viaParameter(f func()) {
	println("seven")
	f()
}

// CONTROL 4 -- a local closure that logs but is not a helper, called last: its t.Log names its OWN line.
func viaLocalLogger(t *testing.T) {
	println("eight")
	logIt := func() {
		t.Log("logged")
	}
	logIt()
}

func main() {
	viaPackageValue()
	_, _ = returnsViaPackageValue()
	viaLocalValue(nil)
	literalCaller()()
	_ = notLast()
	_ = viaPlainValue()
	viaParameter(func() {})
	viaLocalLogger(nil)
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "tailcalledvalue", "main.cs"))
}

func TestTailCalledFuncValueKeepsTheCallersFrame(t *testing.T) {
	isolateConverterPathGlobals(t)
	mainCs := convertTailCalledFuncValueFixture(t)

	for _, name := range []string{"viaPackageValue", "returnsViaPackageValue", "viaLocalValue"} {
		if line := declarationLine(t, mainCs, name); !keepsOwnFrame(line) {
			t.Errorf("%s ends with a call through a caller-observing function value and is not marked: %s", name, line)
		}
	}

	for _, name := range []string{"notLast", "viaPlainValue", "viaParameter", "viaLocalLogger"} {
		if line := declarationLine(t, mainCs, name); keepsOwnFrame(line) {
			t.Errorf("%s is marked, but its tail call is not through a caller-observing function value: %s", name, line)
		}
	}

	// The literal's mark rides its own text: literalCaller's body holds the one literal in the fixture.
	start := strings.Index(mainCs, " literalCaller(")
	end := strings.Index(mainCs, " notLast(")

	if start < 0 || end < start {
		t.Fatalf("literalCaller's body not found in the emission:\n%s", mainCs)
	}

	if body := mainCs[start:end]; !strings.Contains(body, "[MethodImpl(MethodImplOptions.NoInlining)]") {
		t.Errorf("the literal ending with a call through whereValue is not marked:\n%s", body)
	}
}
