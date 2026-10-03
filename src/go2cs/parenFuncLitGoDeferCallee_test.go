// parenFuncLitGoDeferCallee_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a PARENTHESIZED func-literal callee of a go or defer statement (gopkg.in/check.v1's
// `go (func() { … })()`, COORD 2026-10-03, routed from the yaml.v3 reading): it is the same callee as the bare
// literal, so it must emit the same C#. Taken as a non-literal, the statement prepared its own captures while the
// literal, rendered through the ParenExpr with no hoist target, wrote its capture copies INSIDE the call
// (`goǃ((⏎var doneʗ2 = doneʗ1;⏎() => …`, CS1002/CS1003/CS1026/CS1513), and the capture analysis did not treat the
// literal's body as a closure body.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestAParenthesizedFuncLitGoOrDeferCalleeEmitsAsTheBareLiteral(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/pfl\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

var sink = make(chan bool, 1)

func spawn(c int) {
	done := make(chan bool)
	go (func() {
		fmt.Println("go", c)
		done <- true
	})()
	<-done
}

func later(done chan bool) {
	defer (func() {
		fmt.Println("defer", done == nil)
	})()
	done = nil
}

func main() {
	spawn(1)
	later(sink)
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

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "pfl", "main.cs")), "\r\n", "\n")

	// The capture copy is a statement BEFORE the go statement, and the literal is the delegate itself.
	if want := "var doneʗ1 = done;\n    goǃ(() => {\n"; !strings.Contains(mainCs, want) {
		t.Errorf("want %q: the parenthesized literal's capture copy must precede the go statement:\n%s", want, mainCs)
	}

	// The deferred literal reads `done` by reference, as Go's closure does, with no snapshot at all.
	if want := "defer(() => {\n            fmt.Println(deferˢ, done == default!);\n        }, ref ᒐ);"; !strings.Contains(mainCs, want) {
		t.Errorf("want %q: the parenthesized deferred literal must emit as the bare one:\n%s", want, mainCs)
	}

	for _, unwanted := range []string{"goǃ((\n", "defer((\n", "doneʗ2"} {
		if strings.Contains(mainCs, unwanted) {
			t.Errorf("unwanted %q: the literal's capture copy was written inside the call:\n%s", unwanted, mainCs)
		}
	}
}
