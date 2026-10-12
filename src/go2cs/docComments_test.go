// docComments_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards doc comments in `-comments` output (classes B and C of docs/PLAN-comment-and-spacing-parity.md,
// the "missed comments" half of the census).
//
// B: inside a grouped declaration (`const ( … )`, `var ( … )`, `type ( … )`) each spec can carry its own
// doc comment, which go/parser attaches to the SPEC. The converter passed only the group's doc to every
// spec, so a spec's doc was dropped (954 of 958 in the corpus census), and the group's doc, written once,
// left a blank line after every spec of a documented group.
//
// C: the doc of a named type that is neither a struct nor an interface (`type Flags uint16`, `type List
// []int`, `type Table map[K]V`, `type Pipe chan T`, a func type, a pointer type, a type over another
// package's type) was never written (422 of 429): only the struct and interface emitters took it.
//
// The invariant pinned here: every Go doc comment appears exactly once, and the next code line after it
// declares what Go's doc documents. A group's doc stays above its first spec.

package main

import (
	"strings"
	"testing"
)

const docCommentsSource = `package main

import (
	"fmt"
	"time"
)

// Group doc.
const (
	// A doc.
	A = 1
	B = 2
	// C doc.
	C = 3
)

var (
	// v1 doc.
	v1 = 1
	v2 = 2
)

type (
	// Mode doc.
	Mode int
	// Shape doc.
	Shape struct{ x int }
)

// Flags doc.
type Flags uint16

// List doc.
type List []int

// Table doc.
type Table map[string]int

// Pipe doc.
type Pipe chan int

// Handler doc.
type Handler func(int) error

// Ptr doc.
type Ptr *int

// Dur doc.
type Dur time.Duration

func main() {
	var s Shape
	fmt.Println(A, B, C, v1, v2, Mode(1), s.x, Flags(2), List{}, Table{}, Pipe(nil), Handler(nil), Ptr(nil), Dur(0))
}
`

// docAbove returns the first code line after the single emitted line carrying the doc comment.
func docAbove(t *testing.T, lines []string, doc string) string {
	t.Helper()

	at := -1

	for i, line := range lines {
		if strings.TrimSpace(line) == doc {
			if at >= 0 {
				t.Errorf("%q emitted twice (lines %d and %d)", doc, at+1, i+1)
				return ""
			}
			at = i
		}
	}

	if at < 0 {
		t.Errorf("%q not emitted", doc)
		return ""
	}

	for _, line := range lines[at+1:] {
		trimmed := strings.TrimSpace(line)

		if trimmed != "" && !strings.HasPrefix(trimmed, "//") {
			return trimmed
		}
	}

	return ""
}

func TestDocCommentsOfSpecsAndTypes(t *testing.T) {
	lines := convertWithComments(t, docCommentsSource)

	for _, c := range []struct{ doc, declares string }{
		{"// Group doc.", " A => 1"}, // the group's doc stays above its first spec (and A's own doc)
		{"// A doc.", " A => 1"},
		{"// C doc.", " C => 3"},
		{"// v1 doc.", " v1 = 1"},
		{"// Mode doc.", " Mode"},
		{"// Shape doc.", " Shape"},
		{"// Flags doc.", " Flags"},
		{"// List doc.", " List"},
		{"// Table doc.", " Table"},
		{"// Pipe doc.", " Pipe"},
		{"// Handler doc.", " Handler"},
		{"// Ptr doc.", " Ptr"},
		{"// Dur doc.", " Dur"},
	} {
		if got := docAbove(t, lines, c.doc); got != "" && !strings.Contains(got, c.declares) {
			t.Errorf("%q documents %q, want the declaration containing %q", c.doc, got, c.declares)
		}
	}

	// The group's doc is written once, so no blank line follows each spec of the documented group.
	for _, spec := range []string{" B => 2", " C => 3"} {
		for i, line := range lines {
			if strings.Contains(line, spec) && i > 0 && strings.TrimSpace(lines[i-1]) == "" {
				t.Errorf("a blank line above %q, which Go does not have", strings.TrimSpace(line))
			}
		}
	}
}
