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
	lines := convertWithComments(t, packedConstSource)

	decl := lineContaining(t, lines, "@string packed =")

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
	lines := convertWithoutComments(t, packedConstSource)

	at := lineContaining(t, lines, "@string packed =")

	if !strings.Contains(lines[at], "new byte[]{0x00, 0x41, 0x03, 0x00, 0x00, 0x41, 0x03, 0x01, 0xff, 0x80}") {
		t.Errorf("without -comments the folded constant changed: %q", lines[at])
	}
}
