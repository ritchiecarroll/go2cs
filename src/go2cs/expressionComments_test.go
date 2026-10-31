// expressionComments_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the comments on an expression's operands (class G of docs/PLAN-comment-and-spacing-parity.md; the
// owner's decision 3): a comment follows Go's line breaks where the C# expression can break the same way, and is
// otherwise written inline as `/* c */` at its Go position. It is never hoisted above the statement or
// declaration.

package main

import (
	"strings"
	"testing"
)

// A string constant concatenated from literals across lines, each with a trailing comment, that the converter folds
// into one byte array (x/text norm's recompMapPacked is 941 such operands). The array initializer can break between
// elements, so each operand's bytes take their own line with the comment trailing.
const packedConstSource = `package main

import "fmt"

const packed = "" +
	"\x00A\x03\x00" + // first: 0x00410300
	"\x00A\x03\x01" + // second: 0x00410301
	"\xff\x80" // last

func main() {
	fmt.Println(len(packed), packed[1])
}
`

func TestPackedConstOperandCommentsTrailTheirBytes(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, packedConstSource)

	decl := lineContaining(t, lines, "@string packed"+StaticBackingFieldMarker+" =")

	for _, c := range []struct{ comment, bytes string }{
		{"// first: 0x00410300", "0x00, 0x41, 0x03, 0x00,"},
		{"// second: 0x00410301", "0x00, 0x41, 0x03, 0x01,"},
		{"// last", "0xff, 0x80"},
	} {
		at := lineContaining(t, lines, c.comment)

		if at <= decl {
			t.Errorf("%q is at line %d, above the declaration at line %d", c.comment, at+1, decl+1)
		}

		if code, _, _ := strings.Cut(lines[at], "//"); !strings.Contains(code, c.bytes) {
			t.Errorf("%q does not trail its operand's bytes %q: %q", c.comment, c.bytes, lines[at])
		}
	}
}

func TestPackedConstUnchangedWithoutComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithoutComments(t, packedConstSource)

	at := lineContaining(t, lines, "@string packed"+StaticBackingFieldMarker+" =")

	if !strings.Contains(lines[at], "new byte[]{0x00, 0x41, 0x03, 0x00, 0x00, 0x41, 0x03, 0x01, 0xff, 0x80}") {
		t.Errorf("without -comments the folded constant changed: %q", lines[at])
	}
}

// Comments on call arguments and binary operands, inline and across lines.
const operandCommentsSource = `package main

import "fmt"

func cipher(k []byte, read bool) int { return len(k) }

func main() {
	a := cipher(nil, false /* not for reading */)
	wantLen := 1 + /*sec*/ 8 + /*nsec*/ 4
	var s []int
	if len(s) > 4 || // header
		len(s) > 2 || // session
		len(s) > 1 {
		fmt.Println(a, wantLen)
	}
	x := 1<<2 | // two
		1<<3
	fmt.Println(x)
}
`

func TestOperandCommentsAtTheirGoPosition(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, operandCommentsSource)

	// inline, where Go has them
	for _, text := range []string{
		"cipher(default, false /* not for reading */)",
		"1 + /*sec*/ 8 + /*nsec*/ 4",
	} {
		lineContaining(t, lines, text)
	}

	// across lines: each operand's line comment trails its operator, the next operand starts the next line
	for _, c := range []struct{ trailing, next string }{
		{"len(s) > 4 || // header", "len(s) > 2 ||"},
		{"len(s) > 2 || // session", "len(s) > 1)"},
		{"| // two", "(1 << 3)"},
	} {
		at := lineContaining(t, lines, c.trailing)

		if at+1 >= len(lines) || !strings.Contains(lines[at+1], c.next) {
			t.Errorf("the line after %q does not continue with %q:\n%s", c.trailing, c.next, strings.Join(lines[at:min(len(lines), at+2)], "\n"))
		}
	}

	// nothing hoisted out of the expressions
	for i, line := range lines {
		if trimmed := strings.TrimSpace(line); trimmed == "// header" || trimmed == "// session" || trimmed == "// two" || trimmed == "/* not for reading */" || trimmed == "/*sec*/" {
			t.Errorf("line %d: a comment hoisted out of its expression: %q", i+1, line)
		}
	}
}

// A composite literal's elements inside a call's func-literal argument are not that call's argument list: the
// comments after the literal (in the loop body below it) stay where Go has them, and the literal stays on its line.
const literalInCallSource = `package main

import "fmt"

func run(name string, f func()) { f() }

func main() {
	run("x", func() {
		for _, n := range []int{1, 2} {
			// in the loop body
			fmt.Println(n)
		}
	})
}
`

func TestLiteralInCallKeepsItsComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, literalInCallSource)

	loop := lineContaining(t, lines, "foreach (var (_, n) in")

	if !strings.Contains(lines[loop], "slice<nint>([1, 2])) {") {
		t.Errorf("the literal left its line: %q", lines[loop])
	}

	if at := lineContaining(t, lines, "// in the loop body"); at != loop+1 {
		t.Errorf("the loop body's comment is at line %d, want %d (the first line of the body):\n%s", at+1, loop+2, strings.Join(lines[loop:min(len(lines), loop+5)], "\n"))
	}
}

// Own-line comments between the operands of a multi-line chain keep their own lines, one comment line per C#
// line, the next operand after them; and a comment on an argument that a call's conversion renders more than
// once is never lost.
const ownLineOperandCommentsSource = `package main

import "fmt"

func printable(b byte, star bool) bool {
	return 'a' <= b && b <= 'z' ||
		b == '?' ||
		// This is technically not allowed.
		// However, certificates use it.
		(star && b == '*')
}

type ext struct{ v int }

func build() (ret []ext) {
	ret = make([]ext, 10 /* maximum number of elements. */)
	return
}

func main() {
	fmt.Println(printable('a', true), len(build()))
}
`

func TestOwnLineOperandCommentsKeepTheirLines(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, ownLineOperandCommentsSource)

	first := lineContaining(t, lines, "// This is technically not allowed.")
	second := lineContaining(t, lines, "// However, certificates use it.")

	if strings.TrimSpace(lines[first]) != "// This is technically not allowed." || second != first+1 || strings.TrimSpace(lines[second]) != "// However, certificates use it." {
		t.Errorf("the own-line comments are not on lines of their own:\n%s", strings.Join(lines[max(0, first-1):min(len(lines), first+3)], "\n"))
	}

	if next := strings.TrimSpace(lines[second+1]); !strings.HasPrefix(next, "(star && b == '*')") {
		t.Errorf("the operand after the comments is %q, want it on the next line", next)
	}

	// make's size argument: rendered, then rebuilt; its comment is written somewhere, never dropped
	lineContaining(t, lines, "maximum number of elements.")
}
