// boundedGoOrdinal_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the ordinal a [GoBoundedGoroutine] record carries (goroutineStackRecords.go, assignBoundedGoOrdinals): each
// bounded site takes its index among the file's goǃ calls (internal/golauncher), a site the scan does not find takes
// -1, and the records still sort by line and column with the ordinal as a fourth argument.

package main

import (
	"strings"
	"testing"
)

func TestAssignBoundedGoOrdinalsIndexesEveryLauncherAndRefusesAMiss(t *testing.T) {
	text := "goǃ(a);\ngoǃ(b);\n// goǃ(c);\ngoǃ(d);"
	sites := assignBoundedGoOrdinals(text, []boundedGoSite{{csLine: 2, column: 1}, {csLine: 4, column: 1}, {csLine: 3, column: 4}})

	for i, want := range []int{1, 2, -1} {
		if sites[i].ordinal != want {
			t.Errorf("site %d (%d:%d): ordinal %d, want %d", i, sites[i].csLine, sites[i].column, sites[i].ordinal, want)
		}
	}
}

func TestBoundedGoRecordsSortByLineAndColumnWithTheOrdinal(t *testing.T) {
	records := []string{
		`[assembly: GoBoundedGoroutine("a.cs", 12, 9, 3)]`,
		`[assembly: GoBoundedGoroutine("a.cs", 12, 5, 2)]`,
		`[assembly: GoBoundedGoroutine("a.cs", 3, 5, 0)]`,
	}

	sortBoundedGoRecords(records)

	if got := strings.Join(records, " "); got != `[assembly: GoBoundedGoroutine("a.cs", 3, 5, 0)] [assembly: GoBoundedGoroutine("a.cs", 12, 5, 2)] [assembly: GoBoundedGoroutine("a.cs", 12, 9, 3)]` {
		t.Errorf("records sorted as %s", got)
	}
}
