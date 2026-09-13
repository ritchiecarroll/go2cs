// visitCommClause.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
)

func (v *Visitor) visitCommClause(commClause *ast.CommClause) {
	v.writeOutputLn("%s", "/* visitCommClause: " + v.getPrintedNode(commClause) + " */")
}
