// lh.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package lh

import "text/template"

// funcs maps names to the functions that render them.
var funcs = template.FuncMap{
	"first":  first,
	"second": first,
}

// limit is eager.
var limit = 3

func first() int { return limit }
