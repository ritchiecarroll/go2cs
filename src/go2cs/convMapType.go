// convMapType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convMapType(mapType *ast.MapType) string {
	if v.options.preferVarDecl {
		var mapKeyTypeName, mapValueTypeName string

		mapKeyTypeName = convertToCSTypeName(v.getExpressionTypeName(mapType.Key, false))
		mapValueTypeName = convertToCSTypeName(v.getExpressionTypeName(mapType.Value, false))

		return fmt.Sprintf("map<%s, %s>", mapKeyTypeName, mapValueTypeName)
	}

	return "()"
}
