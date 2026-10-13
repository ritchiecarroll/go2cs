// plant.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package plant

// Numbers.
const (
	A = 1 // first
	B = 2 // second

	// Doc for C.
	C = 3
)

// F does things.
func F(x int) int {
	y := x + 1

	// second part
	z := y * 2
	if z > 3 { // big
		return z
	}
	t := []int{
		1, // one
		2, // two
	}
	return y + len(t)
}

// G is next.
func G() {}

// H is last.
func H() {}
