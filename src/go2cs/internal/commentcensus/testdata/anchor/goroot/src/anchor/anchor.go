// anchor.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package anchor

type point struct{ x, y int }

func run(items []int, f func(int)) (n int, err error) {
	total := 0
	for _, it := range items {
		// a func literal reads total
		f(func() int { return total + it }())
	}

Outer:
	for i := range items {
		for j := range items {
			if j > i {
				// skip to the next row
				continue Outer
			}
		}
	}

	// open the item
	v, err := open(n)
	total += v

	p := point{
		x: 1,
		y: 2,
		// y is the last field
	}
	total += p.x
	return total, err
}

func open(n int) (int, error) { return n, nil }
