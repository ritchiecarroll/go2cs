// funcLitNamedNumericLiteralArm_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the multi-result func-literal return-type rule (convFuncLit.go) when one arm is fully typed
// and another returns a numeric literal its declared element cannot take as a TYPE. A numeric
// literal emits bare, so its tuple infers the literal's natural C# type (`int` for an INT literal).
// The rule stated the declared result signature only when NO arm was fully typed, on the reasoning
// that the literal arm converts to the typed one; for a literal whose type does not convert to the
// element (an `int` into a uintptr, a uint32, a named type over either) it does not, and neither
// tuple converts to the other. Go 1.26's os/root_openat.go rootMkdirAll is the shape, on windows
// where sysfdType is syscall.Handle (`type Handle uintptr`):
//
//	openDirFunc := func(parent sysfdType, name string) (sysfdType, error) {
//		…; return fd, err
//		…; return 0, &PathError{…}
//
// The arms' C# types are (Handle, error) and (int, PathError adapter); neither converts to the
// other, so the delegate has no best common type (CS8917). Such a literal now states the signature.
//
// Control: a named type over plain `int` keeps the inferred form, as Go `int` does at a basic
// position (an INT literal reaches it through int→nint, which every use site accepts).
package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertNamedNumericLiteralArmFixture converts the one fixture the test reads and returns its C#.
func convertNamedNumericLiteralArmFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/namednumericarm\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type handle uintptr

type slot int

type pathError struct{ name string }

func (e *pathError) Error() string { return e.name }

func openHandle(h handle) (handle, error) { return h, nil }

func openSlot(s slot) (slot, error) { return s, nil }

func walkHandles(open func(handle, string) (handle, error)) handle {
	h, _ := open(1, "x")
	return h
}

func walkSlots(open func(slot, string) (slot, error)) slot {
	s, _ := open(1, "x")
	return s
}

// SUBJECT - the rootMkdirAll shape: a typed arm and a literal arm at a named uintptr position.
func subject() handle {
	openDir := func(parent handle, name string) (handle, error) {
		for try := range 2 {
			fd, err := openHandle(parent)
			if err == nil {
				return fd, err
			}
			if try > 0 {
				return 0, &pathError{name}
			}
		}
		panic("unreachable")
	}

	return walkHandles(openDir)
}

// CONTROL - the same shape at a named int position keeps natural inference.
func control() slot {
	openDir := func(parent slot, name string) (slot, error) {
		for try := range 2 {
			fd, err := openSlot(parent)
			if err == nil {
				return fd, err
			}
			if try > 0 {
				return 0, &pathError{name}
			}
		}
		panic("unreachable")
	}

	return walkSlots(openDir)
}

func main() {
	println(subject(), control())
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

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "namednumericarm", "main.cs"))
}

// TestFuncLitStatesItsResultAtANamedNumericLiteralArm pins the rule and its control.
func TestFuncLitStatesItsResultAtANamedNumericLiteralArm(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertNamedNumericLiteralArmFixture(t)

	// SUBJECT - the declared result tuple is stated, so each arm converts to it in place.
	subject := emittedFuncBody(t, mainCs, "subject")

	if !strings.Contains(subject, "var openDir = (handle, error) (handle parent, @string name) =>") {
		t.Errorf("subject: expected the literal to state its declared result `(handle, error)` (CS8917 without it):\n%s", subject)
	}

	// CONTROL - a named int position keeps the bare, inferred literal.
	control := emittedFuncBody(t, mainCs, "control")

	if !strings.Contains(control, "var openDir = (slot parent, @string name) =>") {
		t.Errorf("control: a named int result position must keep natural inference:\n%s", control)
	}
}
