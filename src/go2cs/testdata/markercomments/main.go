// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Go comments shaped like the member facts go2cs-gen reads (docs/PLAN-marker-comment-parity.md, 6),
// beside the real facts. Converted with -comments, each Go comment is carried re-spelled, so the records
// go2cs-gen writes for this file are those of the same source without them: the real embeds of marked
// and tagged, the struct tags of tagged, each in the spelling a comment can hold, and the array dims of
// the declarations below (a lambda's, a local function's and a generic func's stay attributes).
package main

import "fmt"

type Inner struct{ n int }

type Reader interface{ Read() int }

type inline struct { /*embed*/ x int }

type marked struct {
	/*embed*/ a int
	/*embed*/
	b int
	c int /*embed*/
	d int
	/*embed*/ Reader
	int
}

type tagged struct {
	Plain         string `json:"plain"`
	Grouped, Pair int    `json:"g"`
	Mixed, mixed  int    `json:"m"`
	Quoted        string "a:\"`b`\""
	Closer        string `x:"*/"`
	Tabbed        string "t:\"\t\""
	Separator     string "u:\"\u2028\""
	Wide          string `json:"ü"`
	Spaced        string `json:"s"` /*`json:"other"`*/
	Bare          int    /*`json:"bare"`*/
	Quote         int    /*"json:\"q\""*/
	Empty         int    ""
	Inner         `json:"inner"`
	fmt.Stringer  `json:"str"`
}

type nn [2][3]int

type P *[4]byte

type holder struct {
	p        *[3]int
	m        map[[2]string][3]int
	s        []*[5]byte
	n        int /*[9]*/
	/*[7]*/ q *[6]int
}

func hash(b [32]byte) int { return len(b) }

func fill(n int, p *[2]int32, grid [4][8]byte) {}

func first[T any](a [2]T) T { return a[0] }

func noted(a /*[9]*/ [4]byte, b /*[10]*/ int) int { return len(a) + b }

func (h *holder) put(v [3]int) {}

func main() {
	lambda := func(x [32]byte) int { return len(x) }
	local := func(x [3]int) int { return len(x) }
	var sink any = lambda
	var h holder
	h.put([3]int{})
	fmt.Println(inline{}, marked{}, tagged{}, nn{}, P(nil), h, hash([32]byte{}), first([2]int{}), noted([4]byte{}, 1), local([3]int{}), sink != nil)
	fill(0, nil, [4][8]byte{})
}
