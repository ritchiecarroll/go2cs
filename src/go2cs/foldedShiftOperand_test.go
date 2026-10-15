// foldedShiftOperand_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards foldedMixedConstShiftOperand (convBinaryExpr.go). Go 1.26's runtime/malloc_generated.go writes
// `*dst = (*dst)&^(((1<<(min(bits, ptrBits)))-1)<<j) | (src << j)` with `const bits uintptr`: a
// non-constant shift whose left operand is a COMPUTED constant mixing a typed constant and min() with an
// untyped named constant. It rendered as `int` arithmetic and the `&^` read CS0019 `uintptr & int` at 17
// sites (seen under the H6 runtime unmask). The fixture is that shape, plus the control the fold must
// leave alone: a computed operand of untyped constants only, which keeps its UntypedInt rendering.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertFoldedShiftFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/foldshift\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

const ptrBits = 64

// POSITIVE -- malloc_generated.go's shape: a typed local const and min() mixed with untyped ptrBits.
func set(dst *uintptr, j, src uintptr) {
	const bits uintptr = 5
	*dst = (*dst)&^(((1<<(min(bits, ptrBits)))-1)<<j) | (src << j)
}

// CONTROL -- untyped constants only: the fold must not touch it.
func untypedOnly(x, j uintptr) uintptr {
	return x &^ (((1 << (ptrBits / 16)) - 1) << j)
}

func main() {
	var dst uintptr = 0xffff
	set(&dst, 2, 3)
	fmt.Println(dst, untypedOnly(0xffff, 2))
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "foldshift", "main.cs"))
}

func funcBody(t *testing.T, cs, signature string) string {
	t.Helper()

	start := strings.Index(cs, signature)

	if start < 0 {
		t.Fatalf("no %q in the emission:\n%s", signature, cs)
	}

	end := strings.Index(cs[start:], "\n}")

	if end < 0 {
		t.Fatalf("no end for %q", signature)
	}

	return cs[start : start+end]
}

// TestMixedConstShiftOperandFoldsToItsValue requires the positive site to emit the operand's VALUE (31),
// typed to the shift (uintptr), with its Go text beside it, and to take the Go-semantics guard (`.Lsh(`,
// the count `j` is unbounded) -- never the `int` arithmetic `1 << (int)(min(` that read CS0019.
func TestMixedConstShiftOperandFoldsToItsValue(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertFoldedShiftFixture(t)
	set := funcBody(t, cs, "void set(")

	if !strings.Contains(set, "((uintptr)(/* ((1 << (min(bits, ptrBits))) - 1) */ 31))") {
		t.Errorf("the mixed computed constant is not folded to its value typed to the shift:\n%s", set)
	}

	if strings.Contains(set, "1 << (int)((min(") {
		t.Errorf("the mixed computed constant still renders as int arithmetic (CS0019 at uintptr &):\n%s", set)
	}

	if !strings.Contains(set, ".Lsh(") {
		t.Errorf("the folded operand's unbounded shift does not take the guard:\n%s", set)
	}

	// CONTROL: untyped-only arithmetic keeps its existing rendering, so no `/* … */ value` appears there.
	untyped := funcBody(t, cs, "untypedOnly(")

	if strings.Contains(untyped, "/* ") {
		t.Errorf("an untyped-only computed operand was folded; it must keep its UntypedInt rendering:\n%s", untyped)
	}
}
