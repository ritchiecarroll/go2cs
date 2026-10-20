// prelude.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package prelude

func one(xs ...int) (n int) {
	n = len(xs)
	return
}

func sum(a [4]int, xs ...int) (n int) {
	// first body comment
	for _, x := range xs {
		n += x
	}
	return n + a[0]
}

func esc(a [2]int, x int) (p *int, n int) {
	n = a[1]
	p = &x
	return
}

func first(a [2]int) (n int) {
	var z int

	n = a[0] + z
	return
}

func grow(xs ...int) (n int) {
	buf := make([]int, len(xs))
	n = copy(buf, xs)
	return
}

func opens(xs ...int) (n int) {
	// opens with a comment, then Go's blank line

	n = len(xs)
	return
}

func regrow(xs ...int) (n int) {
	n = len(xs)

	more := make([]int, n)
	n += len(more)
	return
}

var sink *int

func boxed() int {
	p := 1

	q := p + 1
	sink = &q
	return q
}

func boxedVar() int {
	p := 1

	var q int

	sink = &q
	return p + q
}
