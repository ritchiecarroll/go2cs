// escapingAnonStructPointerLift_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the up-front lift of an anonymous struct whose `:=` local ESCAPES to the heap when the
// right-hand side takes the literal's ADDRESS: `data := &struct{...}{}` followed by `&data`.
//
// The heap declaration (`ref var data = ref heap<T>(out var Ꮡdata)`) renders its box type before the
// right-hand side would trigger the lift, so visitAssignStmt lifts an escaping anonymous-struct
// literal up front -- but it recognized only a bare composite literal, never one under `&`, and the
// declaration emitted the raw Go type `ж<struct{Foo string}>` ("Unresolved dynamic struct type"),
// which no C# context compiles. github.com/mitchellh/mapstructure v1.5.0's TestNextSquashMapstructure
// is the measured instance: one site blocked the module's whole test project.
//
// The CONTROLS are the spellings that already worked, measured the same day: the bare literal (no
// `&`), the `var` spelling, and a non-escaping local.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertEscapingAnonStructPointerFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/anonptr\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

func decode(output interface{}) string { return fmt.Sprintf("%T", output) }

func main() {
	// POSITIVE -- the mapstructure shape: an escaping := local holding the ADDRESS of the literal.
	escapingPtr := &struct{ Foo string }{}
	fmt.Println(decode(&escapingPtr))

	// CONTROL 1 -- no & on the literal (the bare-literal arm the up-front lift already took).
	escapingVal := struct{ Bar string }{}
	fmt.Println(decode(&escapingVal))

	// CONTROL 2 -- the var spelling of the positive.
	var varPtr = &struct{ Baz string }{}
	fmt.Println(decode(&varPtr))

	// CONTROL 3 -- the positive's literal with a local that does NOT escape.
	plainPtr := &struct{ Qux string }{}
	fmt.Println(plainPtr.Qux)
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "anonptr", "main.cs"))
}

func TestEscapingAnonStructPointerIsLiftedBeforeItsHeapDeclaration(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertEscapingAnonStructPointerFixture(t)

	for _, local := range []string{"escapingPtr", "escapingVal", "varPtr", "plainPtr"} {
		line := emittedLineFor(t, mainCs, local)

		if strings.Contains(line, "struct{") {
			t.Errorf("%s: the anonymous struct reached the declaration as raw Go: %s", local, line)
		}
	}

	// The positive's heap box names the LIFTED type through a pointer.
	if line := emittedLineFor(t, mainCs, "escapingPtr"); !strings.Contains(line, "heap<ж<") {
		t.Errorf("escapingPtr: want a heap box of a pointer to the lifted type, got: %s", line)
	}

	if strings.Contains(mainCs, "struct{") {
		t.Errorf("raw Go type text in the emission:\n%s", mainCs)
	}
}
