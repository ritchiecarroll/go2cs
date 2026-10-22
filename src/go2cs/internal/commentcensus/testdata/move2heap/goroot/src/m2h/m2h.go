// m2h.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package m2h

//go:noinline
func pair(n int) ([]byte, []int16) {
	var a []byte
	var w []int16
	for i := range n {
		a = append(a, byte(i))
		w = append(w, int16(i))
	}

	return a, w
}

//go:noinline
func sum(n int) int {
	var g []byte
	for i := range n {
		g = append(g, byte(i))
	}
	t := 0
	// add each byte
	for _, b := range g {
		t += int(b)
	}
	return t
}
