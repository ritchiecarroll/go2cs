// nativeIntConstWidth_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the beyond-int32 integer-constant emission — the CS8778 family.
//
// A Go integer constant whose value falls outside the C# int32 range cannot be written as a bare
// C# literal in a native-width (`nint`/`nuint`) slot: `long` has no implicit conversion to `nint`.
// The converter therefore emitted `(nint)<digits>L` for EVERY such literal, regardless of the type
// the literal actually resolved to — which is wrong twice over:
//
//  1. When the literal resolved to Go `int64` the cast is simply the wrong type. `int64` IS C#
//     `long`, so the digits alone denote it exactly; the `(nint)` came from the untyped-int
//     DEFAULT type leaking through, and on a 32-bit target it TRUNCATES. The compiler says so
//     (CS8778), and math/rand's 607-element `rngCooked` table was the whole of it: every element
//     is negative, and go/types leaves the operand of a CONSTANT unary expression untyped
//     (updateExprType0 does not descend into constant operands), so all 607 lost their `int64`
//     element type while their POSITIVE siblings kept it.
//
//  2. When the literal really is native-width, the constant conversion still needs `unchecked`
//     to be legal without a warning.
//
// The same `unchecked` requirement applies to the two other emitters of a beyond-int32
// native-width constant: convBinaryExpr's constant FOLD and csNintLiteral's array LENGTH.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestBeyondInt32ConstEmission converts a fixture exercising each resolved-type route and pins the
// emitted form for all of them at once. Transpile-only: the assertions are on emitted text.
func TestBeyondInt32ConstEmission(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/nicw\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

// The rngCooked shape: a NEGATED element beside a positive one, both int64.
var cooked = [...]int64{-4181792142133755926, 1395769623340756751}

const maxInt = int(^uint(0) >> 1)

func main() {
	var wide int64 = -4181792142133755926
	var native int = 4611686018427387903
	var boxed any = 4611686018427387903
	var inRange int64 = 1000000
	fmt.Println(cooked[0], cooked[1], wide, native, boxed, inRange, maxInt/2)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "nicw", "main.cs"))

	// (1) The int64 routes take the bare `L` literal — no cast at all. The NEGATED composite-literal
	// element is the regression that motivated this: it must read exactly like the Go source.
	for _, want := range []string{
		"-4181792142133755926L", // negated int64 element (context-resolved) and the int64 var
		"1395769623340756751L",  // its positive sibling (directly typed by go/types)
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing bare int64 literal %s:\n%s", want, mainCs)
		}
	}

	// The pre-fix emission, in either spelling, must be gone: `(nint)` on an int64 constant is a
	// 32-bit truncation.
	for _, bad := range []string{
		"(nint)4181792142133755926L",
		"(nint)1395769623340756751L",
	} {
		if strings.Contains(mainCs, bad) {
			t.Errorf("int64 constant still emitted as a native-int cast (%s):\n%s", bad, mainCs)
		}
	}

	// (2) A genuinely native-width constant KEEPS the cast — `long` does not convert to `nint`, and
	// an `any` slot must box a Go int as nint so a later `x.(int)` succeeds — but carries
	// `unchecked` so the constant conversion does not warn.
	if strings.Count(mainCs, "unchecked((nint)4611686018427387903L)") != 2 {
		t.Errorf("expected both the `int` var and the `any` slot to emit unchecked((nint)4611686018427387903L):\n%s", mainCs)
	}

	// (3) convBinaryExpr's constant FOLD (`maxInt/2`) keeps its parenthesized cast body and gains
	// the wrapper. Critically it must NOT be double-wrapped — that is what wholeExprIsCastOfType's
	// unchecked peel prevents.
	if !strings.Contains(mainCs, "unchecked((nint)(4611686018427387903L))") {
		t.Errorf("folded native-int constant missing its unchecked wrapper:\n%s", mainCs)
	}

	if strings.Contains(mainCs, "(nint)(unchecked(") {
		t.Errorf("folded constant was double-wrapped — wholeExprIsCastOfType did not peel `unchecked(`:\n%s", mainCs)
	}

	// (4) An in-range constant is untouched by all of this — no suffix, no cast, no wrapper.
	if !strings.Contains(mainCs, "1000000") || strings.Contains(mainCs, "unchecked((nint)1000000") {
		t.Errorf("in-range constant emission changed:\n%s", mainCs)
	}
}

// TestWholeExprIsCastOfTypePeelsUnchecked pins the recognizer generalization on its own. Every
// redundancy guard in the converter asks this function "is the whole expression already a cast to
// this type?"; an `unchecked(…)` wrapper does not change the answer, and before the peel those
// guards re-wrapped a wrapped fold into `(nint)(unchecked((nint)(…)))`.
func TestWholeExprIsCastOfTypePeelsUnchecked(t *testing.T) {
	cases := []struct {
		expr     string
		castType string
		want     bool
	}{
		// The wrapped fold — the shape convBinaryExpr now emits beyond 32 bits.
		{"unchecked((nint)(4611686018427387903L))", "nint", true},
		{"unchecked((nuint)(16294579238595022365UL))", "nuint", true},
		{"unchecked((nint)(4611686018427387903L))", "nuint", false},
		// The unwrapped fold still matches exactly as before (in-range folds keep this form).
		{"(nint)(42)", "nint", true},
		{"(nuint)(42)", "nuint", true},
		{"(nint)(42)", "int64", false},
		// A cast that does NOT span the whole expression is still rejected, wrapped or not.
		{"(nint)(a) + b", "nint", false},
		{"unchecked((nint)(a) + b)", "nint", false},
		// An `unchecked` wrapper that does not span the whole expression must not be peeled.
		{"unchecked((nint)(a)) + b", "nint", false},
		// Parens inside a char literal must not perturb the balance (the pre-existing contract).
		{"(rune)('(')", "rune", true},
		{"unchecked((rune)('('))", "rune", true},
		// Not a cast at all.
		{"unchecked(x + y)", "nint", false},
		{"", "nint", false},
	}

	for _, c := range cases {
		if got := wholeExprIsCastOfType(c.expr, c.castType); got != c.want {
			t.Errorf("wholeExprIsCastOfType(%q, %q) = %v, want %v", c.expr, c.castType, got, c.want)
		}
	}
}
