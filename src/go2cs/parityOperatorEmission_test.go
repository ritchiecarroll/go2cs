// parityOperatorEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/token"
	"strings"
	"testing"
)

// The parity operator seat: a shift or bitwise expression spells only the parentheses C# needs to group it as Go
// does, a bitwise result C# already types as Go's keeps no re-cast, a literal shift count is Go's, and a conversion
// or literal cast written as a whole expression carries no extra parentheses. Each assertion is on the emitted
// text; ParityOperatorPrecedence holds the matrix that proves the grouping by running it against Go.
const parityOperatorSource = `package main

import "fmt"

type flags uint32

type word uintptr

func le32(b []byte) uint32 {
	return uint32(b[0]) | uint32(b[1])<<8 | uint32(b[2])<<16 | uint32(b[3])<<24
}

func masked(a, m uint64) bool { return a&m == 0 }

func shifted(x, y uint32) uint32 { return x<<3 + y }

func mixed(a, b, c uint32) uint32 { return a | b ^ c }

func narrow(a, b byte) byte { return a | b }

func named(a, b flags) flags { return a | b }

func pointerSized(a, b word) word { return a | b }

func cleared(a uint64) uint64 { return a &^ 0xFF }

func maskedCount(u uint64, k uint) uint64 { return u >> (k & 63) }

func reducedCount(u uint64, k uint) uint64 { return u >> (k % 64) }

func main() {
	x := uint32(7)
	fmt.Println(uint64(x)|1, le32([]byte{1, 2, 3, 4}), masked(3, 4), shifted(1, 2), mixed(1, 2, 3))
	fmt.Println(narrow(1, 2), named(1, 2), pointerSized(1, 2), cleared(0x1234))
}
`

func TestParityOperatorEmissionReadsAsGo(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, parityOperatorSource)

	for _, want := range []string{
		// The byte assembly, token for token with Go: no re-casts, no shift parentheses, Go's shift counts.
		"return (uint32)b[0] | (uint32)b[1] << 8 | (uint32)b[2] << 16 | (uint32)b[3] << 24;",
		// C# ranks & below ==, so Go's `a&m == 0` keeps exactly that pair.
		"return (a & m) == 0;",
		// C# ranks << below +, so Go's `x<<3 + y` keeps the shift's pair.
		"return (x << 3) + y;",
		// Go groups `a | b ^ c` left to right; C# ranks ^ above |, so the pair stays.
		"return (a | b) ^ c;",
		// `&^` renders `& ~`: a cast constant complement keeps the result at Go's width with no result cast.
		"return a & ~(uint64)0xFF;",
		// A conversion as an argument, with no parentheses of its own.
		"fmt.Println((uint64)x | 1,",
		// A non-literal shift count takes C#'s `int` cast over Go's own parentheses, not a second pair of its own.
		"return u >> (int)(k & 63);",
		"return u >> (int)(k % 64);",
	} {
		lineContaining(t, lines, want)
	}

	// The casts that stay: C# computes `byte | byte` in int, a named numeric's operators are generated, and
	// uintptr is a golib struct.
	for _, want := range []string{
		"return (byte)(a | b);",
		"return (flags)(a | b);",
		"return (word)(a | b);",
	} {
		lineContaining(t, lines, want)
	}

	for i, line := range lines {
		// A literal shift count cast to int, a cast over a doubled parenthesis, or a conversion argument re-wrapped.
		if strings.Contains(line, "<< (int)(") || strings.Contains(line, ")((") || strings.Contains(line, "Println(((uint64)x") {
			t.Errorf("line %d keeps a cast or parenthesis Go does not have: %q", i+1, line)
		}
	}
}

func TestOperandParenthesesFollowCSharpPrecedence(t *testing.T) {
	for _, c := range []struct {
		child, parent token.Token
		right, want   bool
	}{
		{token.SHL, token.OR, false, false},  // x<<8 | y
		{token.SHL, token.OR, true, false},   // y | x<<8
		{token.AND, token.EQL, false, true},  // (a & m) == 0
		{token.SHL, token.ADD, false, true},  // (x << 3) + y
		{token.SHL, token.ADD, true, true},   // y + (x << 3)
		{token.OR, token.XOR, false, true},   // (a | b) ^ c
		{token.AND, token.XOR, false, false}, // a & b ^ c
		{token.OR, token.OR, false, false},   // a | b | c
		{token.OR, token.OR, true, true},     // a | (b | c)
		{token.SHL, token.SHR, false, false}, // a << b >> c
		{token.AND_NOT, token.AND, false, false},
		{token.XOR, token.LSS, false, true},
		{token.SHL, token.EQL, false, false}, // a << 3 == b
	} {
		if got := operandParensNeeded(c.child, c.parent, c.right); got != c.want {
			t.Errorf("%s under %s (right=%v): parentheses %v, want %v", c.child, c.parent, c.right, got, c.want)
		}
	}
}

func TestWrappedWholeSeesOnlyOneParenthesizedExpression(t *testing.T) {
	for _, c := range []struct {
		expr string
		want bool
	}{
		{"(a | b)", true},
		{"((uint32)x)", true},
		{"(a) | (b)", false},
		{"(a, b)", false},
		{"(uint32)(x)", false},
		{`("a)b" + c)`, true},
		{`(c == ')')`, true},
		{"(x /* ) */ + y)", true},
		{"(f(a, b))", true},
		{"()", true},
		{"(", false},
	} {
		if got := wrappedWhole(c.expr); got != c.want {
			t.Errorf("wrappedWhole(%q) = %v, want %v", c.expr, got, c.want)
		}
	}
}

// An operand already rendered as ONE parenthesized expression takes the cast directly: `(int)(c[3] & 63)`, not
// `(int)((c[3] & 63))`. A tuple and two groups side by side are not one expression and keep the cast's own pair.
func TestCastOfAWrappedOperandTakesNoSecondPair(t *testing.T) {
	for _, c := range []struct{ rendered, want string }{
		{"(c[3] & 63)", "(int)(c[3] & 63)"},
		{"(k % 64)", "(int)(k % 64)"},
		{"((uint64)x)", "(int)((uint64)x)"},
		{"(a, b)", "(int)((a, b))"},
		{"(a) + (b)", "(int)((a) + (b))"},
		{"(a)(b)", "(int)((a)(b))"},
	} {
		if got := castOf("int", c.rendered); got != c.want {
			t.Errorf("castOf(int, %q) = %q, want %q", c.rendered, got, c.want)
		}
	}
}

func TestCastOfDropsParenthesesOnlyAroundAnUnsignedLiteral(t *testing.T) {
	for _, c := range []struct{ rendered, want string }{
		{"500", "(uint32)500"},
		{"0x1F", "(uint32)0x1F"},
		{"1.5e3", "(uint32)1.5e3"},
		{"8UL", "(uint32)8UL"},
		{"'a'", "(uint32)'a'"},
		{"-1", "(uint32)(-1)"},
		{"a + b", "(uint32)(a + b)"},
		{"x", "(uint32)(x)"},
	} {
		if got := castOf("uint32", c.rendered); got != c.want {
			t.Errorf("castOf(uint32, %q) = %q, want %q", c.rendered, got, c.want)
		}
	}
}
