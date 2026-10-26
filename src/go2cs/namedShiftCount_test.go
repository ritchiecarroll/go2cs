// namedShiftCount_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the count of a shift on a NAMED integer type: an unbounded count reaches the wrapper's generated operator
// WIDENED, as the unnamed guard's count is (shiftCountUint64Operand), never narrowed by an `(int)` cast. An unsigned
// count keeps its full magnitude (Go's `w << (1 << 32)` is 0) and a signed one widens to int64, whose overload
// raises Go's negative-shift panic, in the binary and the compound form alike. The controls keep their emission: a
// literal count, a count masked into range, an unnamed operand, which takes golib's guard, and an UNTYPED
// constant left operand with a named result (io/fs's `m&(1<<uint(31-i))`), which shifts through the underlying
// basic's native operator and so keeps its `int` count.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func TestNamedShiftCountIsWidened(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()

	writeModuleFile(t, filepath.Join(root, "app", "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(root, "app", "main.go"), `package main

import "fmt"

type W uint32

func unsignedCount(w W, s uint) W   { return w << s }
func wideCount(w W, s uint64) W     { return w >> s }
func intCount(w W, n int) W         { return w << n }
func compoundUnsigned(w W, s uint) W { w <<= s; return w }
func compoundSigned(w W, n int) W   { w >>= n; return w }

func literalCount(w W) W           { return w << 3 }
func maskedCount(w W, s uint) W    { return w << (s & 31) }
func unnamed(u uint32, s uint) uint32 { return u << s }
func constLeft(m W, i int) bool       { return m&(1<<uint(31-i)) != 0 }

func main() {
	fmt.Println(unsignedCount(1, 1), wideCount(1, 1), intCount(1, 1), compoundUnsigned(1, 1), compoundSigned(1, 1),
		literalCount(1), maskedCount(1, 1), unnamed(1, 1), constLeft(1, 1))
}
`)

	cs := readGenerated(t, filepath.Join(convertFieldAccessModule(t, root), "main.cs"))

	for _, want := range []struct{ fn, form string }{
		{"unsignedCount(", "w << s"},
		{"wideCount(", "w >> s"},
		{"intCount(", "w << (int64)(n)"},
		{"compoundUnsigned(", "w <<= s;"},
		{"compoundSigned(", "w >>= (int64)(n);"},
	} {
		body := funcBody(t, cs, want.fn)

		if !strings.Contains(body, want.form) || strings.Contains(body, "(int)") {
			t.Errorf("a named shift's unbounded count in %s is not passed widened (want %q, no `(int)`):\n%s", want.fn, want.form, body)
		}
	}

	for _, kept := range []struct{ fn, form string }{
		{"literalCount(", "w << 3"},
		{"maskedCount(", "w << (int)(s & 31)"},
		{"unnamed(", "u.Lsh(s)"},
		{"constLeft(", "(uint32)1 << (int)("},
	} {
		if body := funcBody(t, cs, kept.fn); !strings.Contains(body, kept.form) {
			t.Errorf("a control left its emission in %s (want %q):\n%s", kept.fn, kept.form, body)
		}
	}
}
