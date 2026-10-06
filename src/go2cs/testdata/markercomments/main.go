// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Go comments shaped like the member facts go2cs-gen reads (docs/PLAN-marker-comment-parity.md, 6).
// Converted with -comments, each is carried re-spelled, so the records go2cs-gen writes for this file
// are those of the same source without them: the two real embeds of marked, and nothing else.
package main

import "fmt"

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

func main() {
	fmt.Println(inline{}, marked{})
}
