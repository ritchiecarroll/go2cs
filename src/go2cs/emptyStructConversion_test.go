// emptyStructConversion_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the EMPTY-STRUCT conversion arm in convCallExpr: a Go conversion between two different
// struct types whose shared underlying is `struct{}` emits a CONSTRUCTION, not a cast.
//
// The corpus instance is unique's handle_test.go:50, `testZeroSize(struct{}{})`, which emitted
// `((testZeroSize)new EmptyStruct())` — a cast between two unrelated C# structs (a bare [GoType]
// with no underlying argument declares no conversion operator), i.e. CS0030.
//
// The CONTROLS carry as much weight as the positives here, for two different reasons:
//
//   - `new T()` DISCARDS the operand's emission, so an operand that can carry computation must not
//     take the arm. A call operand keeps the cast and fails loudly at compile time rather than
//     silently dropping the call.
//   - a rule that fired for FIELD-BEARING structs would construct a zero value where Go copies
//     fields — silently wrong, and it would rewrite emissions across the corpus. The arm is gated
//     on NumFields() == 0 on BOTH sides, which is what makes `new T()` total rather than lossy.

package main

import (
	"go/ast"
	"go/build"
	"go/parser"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertEmptyStructConversionFixture converts the one fixture every arm below reads.
func convertEmptyStructConversionFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/emptyconv\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

// The corpus shape: a named type whose underlying is the empty struct.
type zeroNamed struct{}

// A SECOND named empty struct, so the named-to-named spelling is covered too.
type otherZero struct{}

// Field-bearing twins with an identical underlying — the scope control.
type pairA struct{ X int }
type pairB struct{ X int }

var sideEffects int

func makeZero() struct{} { sideEffects++; return struct{}{} }

func main() {
	var pureSource struct{}
	var identitySource zeroNamed
	var namedSource otherZero
	var pairSource pairB

	// POSITIVE 1 — the corpus spelling: an anonymous empty composite literal.
	zFromAnon := zeroNamed(struct{}{})

	// POSITIVE 2 — a pure read of a variable, which cannot carry computation either.
	zFromPure := zeroNamed(pureSource)

	// POSITIVE 3 — an empty composite literal of a NAMED empty struct.
	zFromNamed := zeroNamed(namedSource)

	// POSITIVE 4 — the named literal written inline.
	zFromLiteral := zeroNamed(otherZero{})

	// CONTROL 1 — a CALL operand. Taking the arm here would drop makeZero()'s increment.
	zFromCall := zeroNamed(makeZero())

	// CONTROL 2 — field-bearing structs with an identical underlying. A construction here would
	// silently zero X where Go copies it.
	pFromPair := pairA(pairSource)

	// CONTROL 3 — an identity conversion. The source is already the target type.
	zIdentity := zeroNamed(identitySource)

	fmt.Println(zFromAnon, zFromPure, zFromNamed, zFromLiteral, zFromCall, pFromPair, zIdentity, sideEffects)
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "emptyconv", "main.cs"))
}

// emittedLineFor returns the emitted line that binds the named local, so each arm is read at ITS OWN
// statement rather than anywhere in the file — without which a single positive elsewhere would
// satisfy a whole-file Contains check and every control would pass vacuously.
func emittedLineFor(t *testing.T, mainCs string, local string) string {
	t.Helper()

	for _, line := range strings.Split(mainCs, "\n") {
		if strings.Contains(line, local) && strings.Contains(line, "=") {
			return strings.TrimSpace(line)
		}
	}

	t.Fatalf("no emitted line binds %q; emission:\n%s", local, mainCs)

	return ""
}

func TestEmptyStructConversionEmitsAConstruction(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertEmptyStructConversionFixture(t)

	// The positives construct. `new zeroNamed()` is the shape the corpus already carries for a
	// zero-field [GoType] struct (200 of them exist; 15 sites already emit `new X()` over 7).
	for _, local := range []string{"zFromAnon", "zFromPure", "zFromNamed", "zFromLiteral"} {
		line := emittedLineFor(t, mainCs, local)

		if !strings.Contains(line, "new zeroNamed()") {
			t.Errorf("%s: expected a construction `new zeroNamed()`, got: %s", local, line)
		}

		if strings.Contains(line, "(zeroNamed)") {
			t.Errorf("%s: expected NO cast to zeroNamed, got: %s", local, line)
		}
	}

	// CONTROL 1 — the call must survive. `new zeroNamed()` here would discard makeZero().
	callLine := emittedLineFor(t, mainCs, "zFromCall")

	if !strings.Contains(callLine, "makeZero()") {
		t.Errorf("zFromCall: the call operand must survive the conversion, got: %s", callLine)
	}

	if strings.Contains(callLine, "new zeroNamed()") {
		t.Errorf("zFromCall: a call operand must NOT take the construction arm (it would drop the call), got: %s", callLine)
	}

	// CONTROL 2 — field-bearing structs keep whatever they emitted before; the one thing they must
	// never do is construct a zero value, which would drop X.
	pairLine := emittedLineFor(t, mainCs, "pFromPair")

	if strings.Contains(pairLine, "new pairA()") {
		t.Errorf("pFromPair: a FIELD-BEARING struct conversion must not construct a zero value, got: %s", pairLine)
	}

	// CONTROL 3 — an identity conversion has no second type to construct from.
	identityLine := emittedLineFor(t, mainCs, "zIdentity")

	if strings.Contains(identityLine, "new zeroNamed()") {
		t.Errorf("zIdentity: an identity conversion must not take the construction arm, got: %s", identityLine)
	}
}

// TestEmptyCompositeLitPredicate pins the operand gate directly, including through parentheses,
// since it is the whole reason a call operand cannot reach the arm.
// parseEmptyLitTestExpr parses a single Go expression for the predicate arms below.
func parseEmptyLitTestExpr(t *testing.T, source string) ast.Expr {
	t.Helper()

	expr, err := parser.ParseExpr(source)

	if err != nil {
		t.Fatalf("parsing %q: %v", source, err)
	}

	return expr
}

func TestEmptyCompositeLitPredicate(t *testing.T) {
	cases := []struct {
		source string
		want   bool
	}{
		{"struct{}{}", true},
		{"(struct{}{})", true},
		{"((struct{}{}))", true},
		{"T{}", true},
		{"T{1}", false},
		{"struct{ X int }{1}", false},
		{"f()", false},
		{"(f())", false},
		{"x", false},
	}

	for _, testCase := range cases {
		expr := parseEmptyLitTestExpr(t, testCase.source)

		if got := isEmptyCompositeLit(expr); got != testCase.want {
			t.Errorf("isEmptyCompositeLit(%s) = %v, want %v", testCase.source, got, testCase.want)
		}
	}
}
