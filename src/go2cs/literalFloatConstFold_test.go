// literalFloatConstFold_test.go - Gbtc
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

// A literal-only float constant expression is folded only when C#'s step-by-step evaluation of the
// operator form would differ from Go's exact value rounded once (internal/trace/traceviewer's
// `1 - .999`, 0.0010000000000000009 in C# against Go's 0.001). Everything that already evaluates to
// Go's value keeps its operator form.

const literalFloatConstFixture = `package main

import "fmt"

func main() {
	q := []float64{0, 1 - .999, 1 - .99, 1 - .95}
	x := 0.1 + 0.2
	y := 1.5 * 2.0
	z := 7/2 + .5
	var f float32 = 0.1 + 0.2
	w := (1 - .999) * 1000
	fmt.Println(q, x, y, z, f, w)
}
`

func convertLiteralFloatConstFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/litfloat\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), literalFloatConstFixture)

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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "litfloat", "main.cs"))
}

func TestInexactLiteralFloatConstFoldsToGosValue(t *testing.T) {
	mainCs := convertLiteralFloatConstFixture(t)

	for _, want := range []string{
		"/* 1 - .999 */ 0.001D",
		"/* 1 - .99 */ 0.01D",
		"/* 1 - .95 */ 0.05D",
		"var x = /* 0.1 + 0.2 */ 0.3D;",
		"var w = /* (1 - .999) * 1000 */ 1D;", // the whole literal expression folds, once
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing %q in:\n%s", want, mainCs)
		}
	}
}

func TestExactLiteralFloatConstKeepsItsOperatorForm(t *testing.T) {
	mainCs := convertLiteralFloatConstFixture(t)

	for _, want := range []string{
		"var y = 1.5D * 2.0D;",     // exact in double: the must-not-fold control
		"var z = 7 / 2 + .5D;",     // integer pair stays integer arithmetic, 3.5 exactly
		"float32 f = 0.1F + 0.2F;", // float32 steps land on Go's float32(0.3)
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing %q in:\n%s", want, mainCs)
		}
	}
}
