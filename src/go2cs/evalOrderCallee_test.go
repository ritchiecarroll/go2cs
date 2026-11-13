// evalOrderCallee_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the callee half of the multi-value spread (S4, SCOUT-tour-visual-parity.md 1.2): Go's spec evaluates a call's
// function value before its arguments, so in pick()(g()) pick runs before g. The spread's temp form hoists g()'s results
// ahead of the statement, so a callee that is itself a call is hoisted first. A callee that only reads (a package-level
// function, a func variable, a method's receiver) keeps the form it had: gc reads a variable after the calls in the
// operand list (H's rule), and a function name evaluates nothing.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

const evalOrderCalleeFixture = `package main

import "fmt"

func pick() func(int, int) int {
	fmt.Println("pick")
	return func(a, b int) int { return a + b }
}

func g() (int, int) {
	fmt.Println("g")
	return 1, 2
}

func add(a, b int) int { return a + b }

type point struct{ x int }

func (p point) scaled(a, b int) int { return p.x * (a + b) }

func main() {
	fmt.Println(pick()(g()))
	r := pick()(g())
	fmt.Println(r, add(g()))
	h := add
	p := point{x: 2}
	fmt.Println(h(g()), p.scaled(g()))
}
`

func convertEvalOrderCalleeFixture(t *testing.T) string {
	t.Helper()
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/calleeorder\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), evalOrderCalleeFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{goRoot: goRoot, goPath: build.Default.GOPATH, go2csPath: filepath.Join(root, "out"), recurse: true,
		targetPlatform: runtime.GOOS + "/" + runtime.GOARCH, indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "calleeorder", "main.cs")), "\r\n", "\n")
}

func TestSpreadEvaluatesACalleeThatCallsFirst(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	cs := convertEvalOrderCalleeFixture(t)

	for _, want := range []string{
		// pick()(g()): the callee's value is hoisted before g()'s results.
		"var ᴛ1 = pick();\n    var (ᴛ2, ᴛ3) = g();\n    fmt.Println(ᴛ1(ᴛ2, ᴛ3));",
		"var ᴛ4 = pick();\n    var (ᴛ5, ᴛ6) = g();\n    nint r = ᴛ4(ᴛ5, ᴛ6);",
		// Controls: a function name, a func variable and a method receiver read nothing before the calls.
		"var (ᴛ7, ᴛ8) = g();\n    fmt.Println(r, add(ᴛ7, ᴛ8));",
		"fmt.Println(h(ᴛ9, ᴛ10), p.scaled(ᴛ11, ᴛ12));",
	} {
		if !strings.Contains(cs, want) {
			t.Errorf("want\n%s\nin:\n%s", want, cs)
		}
	}
}
