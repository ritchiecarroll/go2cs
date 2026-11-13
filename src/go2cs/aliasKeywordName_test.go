// aliasKeywordName_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

// A package-level alias whose Go name is a C# keyword (modernc.org/libc's `long` and `ulong`, sqlite-cs S9) or a
// golib reserved name is declared under the escaped name its uses are already spelled with: the `global using` is
// the only declaration, so a bare `long` there is a C# parse error, and a bare `slice` would leave every Δslice use
// unbound.
const aliasKeywordNameSource = `package main

import "fmt"

type (
	long  = int32
	ulong = uint32
	slice = []int
)

func size(n int) long { return long(n) }

func main() {
	var u ulong = 7
	var s slice = slice{1, 2}
	fmt.Println(size(3), u, long(-1), len(s))
}
`

func TestAliasKeywordNameIsDeclaredEscaped(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, aliasKeywordNameSource)
	emitted := strings.Join(lines, "\n")

	for _, want := range []string{"global using @long = int;", "global using @ulong = uint;", "global using Δslice = "} {
		if !strings.Contains(emitted, want) {
			t.Errorf("missing %q", want)
		}
	}

	for _, use := range []string{"internal static @long size(", "@ulong u = 7;"} {
		if !strings.Contains(emitted, use) {
			t.Errorf("the uses moved: missing %q", use)
		}
	}

	for _, line := range lines {
		if strings.HasPrefix(line, "global using long ") || strings.HasPrefix(line, "global using ulong ") || strings.HasPrefix(line, "global using slice ") {
			t.Errorf("a keyword or reserved alias is declared bare: %q", line)
		}
	}
}
