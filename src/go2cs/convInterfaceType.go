// convInterfaceType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"
)

func (v *Visitor) convInterfaceType(interfaceType *ast.InterfaceType, context IdentContext) string {
	var name string
	var identType types.Type

	t := v.getType(interfaceType, false)

	if liftedName, ok := v.liftedNameFor(t); ok {
		return liftedName
	}

	if context.ident == nil {
		name = "type"
	} else {
		if len(context.ident.Name) > 0 {
			name = context.ident.Name
		}

		identType = v.getIdentType(context.ident)
	}

	return v.visitInterfaceType(interfaceType, identType, name, nil, true, nil)
}
