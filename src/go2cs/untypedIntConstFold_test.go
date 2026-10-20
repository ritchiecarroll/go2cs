// untypedIntConstFold_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards foldedMixedIntegerConstLiteral's second trigger, untypedIntEvaluationDiffers (untypedIntConstFold.go).
// Go 1.26's internal/strconv/math.go asserts at compile time that `_ = assert[(div1e8m*5*5*5*5*5*5*5*5)%(1<<64)-1]`
// and tabulates `maxUint64 / 5`. Both name an untyped constant, so they rendered as golib UntypedInt arithmetic,
// which is int64: `1 << 64` is 1 there, the index is -1, and trimZeros panicked "index out of range [-1]" under
// every shortest %v of a float64. `maxUint64 / 5` read -1 / 5 = 0. The fixture is both shapes, plus the controls
// the fold must leave alone: one whose int64 evaluation wraps to Go's exact value, and one that never leaves int64.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertUntypedIntFoldFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/untypedfold\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

const maxUint64 = 1<<64 - 1

const small = 10

// POSITIVE -- internal/strconv's div5Tab: maxUint64 / 5 is -1 / 5 = 0 in int64.
func quotients() (uint64, uint64) {
	return maxUint64 / 5, maxUint64 / 5 / 5
}

// POSITIVE -- internal/strconv's trimZeros assert: the index is the CONSTANT 0, and -1 in int64.
func asserted() int {
	const div1e1m = 0xcccccccccccccccd
	var assert [1]struct{}
	_ = assert[(div1e1m*5)%(1<<64)-1]
	return len(assert)
}

// CONTROL -- int64 evaluation wraps to Go's exact value: kept as operators.
func wraps() uint64 {
	return maxUint64 - 1
}

// CONTROL -- never leaves int64: kept as operators.
func inRange() int64 {
	return small * 3
}

func main() {
	q1, q2 := quotients()
	fmt.Println(q1, q2, asserted(), wraps(), inRange(), 0.5)
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "untypedfold", "main.cs"))
}

// TestUntypedIntArithmeticThatLeavesInt64FoldsToGoValue requires each positive site to emit Go's exact value
// beside its Go text, never the UntypedInt operators, and each control to keep its operators.
func TestUntypedIntArithmeticThatLeavesInt64FoldsToGoValue(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertUntypedIntFoldFixture(t)
	quotients := funcBody(t, cs, "quotients(")

	for _, want := range []string{"/* maxUint64 / 5 */ 3689348814741910323UL", "/* maxUint64 / 5 / 5 */ 737869762948382064UL"} {
		if !strings.Contains(quotients, want) {
			t.Errorf("the quotient is not folded to Go's value %q:\n%s", want, quotients)
		}
	}

	asserted := funcBody(t, cs, "asserted(")

	if !strings.Contains(asserted, "/* (div1e1m*5)%(1<<64) - 1 */ 0L") {
		t.Errorf("the compile-time assert's index is not folded to Go's constant 0:\n%s", asserted)
	}

	if strings.Contains(asserted, "<< 64") || strings.Contains(asserted, "<< (int)(64)") {
		t.Errorf("the assert still evaluates 1 << 64 at run time (1 in int64, index -1):\n%s", asserted)
	}

	// CONTROLS: an evaluation that wraps to Go's value, and one that never leaves int64, keep their operators.
	if wraps := funcBody(t, cs, "wraps("); strings.Contains(wraps, "/* ") || !strings.Contains(wraps, "maxUint64 - 1") {
		t.Errorf("an int64 evaluation equal to Go's value was folded; it must keep its operators:\n%s", wraps)
	}

	if inRange := funcBody(t, cs, "inRange("); strings.Contains(inRange, "/* ") || !strings.Contains(inRange, "small * 3") {
		t.Errorf("an in-range constant expression was folded; it must keep its operators:\n%s", inRange)
	}
}
