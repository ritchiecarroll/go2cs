// census.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "regexp"

// measure is one counted shape: an item's pattern, matched over each file's whole text (a pattern may span lines),
// non-overlapping, left to right.
type measure struct {
	Item, Name string
	Pattern    *regexp.Regexp
}

// row is one measure's reading over one set.
type row struct {
	Item, Measure, Set string
	Occurrences, Files int
}

// census counts every measure over every set: occurrences, and the files holding at least one.
func census(files []file, measures []measure) []row {
	type key struct{ m, set int }
	occ := map[key]int{}
	hit := map[key]int{}
	sets := map[string]int{}
	for i, s := range setOrder {
		sets[s] = i
	}
	for _, f := range files {
		s := sets[f.set]
		for mi, m := range measures {
			if n := len(m.Pattern.FindAllIndex(f.text, -1)); n > 0 {
				occ[key{mi, s}] += n
				hit[key{mi, s}]++
			}
		}
	}
	var rows []row
	for mi, m := range measures {
		for si, s := range setOrder {
			k := key{mi, si}
			rows = append(rows, row{m.Item, m.Name, s, occ[k], hit[k]})
		}
	}
	return rows
}
