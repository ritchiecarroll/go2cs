// structConversionCopy.go - Gbtc
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

// conversionOperandCopy renders the operand of a conversion to a struct or array type as the copy Go
// converts. `c := chord(lc)` copies lc, and a write to `c.Notes` does not reach `lc.Notes`. The emitted
// conversion operator (and go2cs-gen's function for two foreign types) builds its result from the
// operand's fields, so an `array<T>` field, or a nested struct holding one, shares its backing with the
// operand: `new chord(src.Name, src.Notes, src.In)`. Where the operand is existing storage the copy is
// taken first, `(chord)lc.ΔClone()`, exactly as an assignment, a return or a send takes it
// (exprReadsValueNeedingClone). A fresh operand (a call, a composite literal) is reachable by no other
// name and is left as it is.
//
// A defined type written over another named STRUCT type, `type Counts counts`, is go2cs-gen's inherited
// wrapper, and its two operators with that base already copy (`[GoValueClone("Value")]`,
// InheritedTypeTemplate.ConversionCopy); that pair is left to them, so the value is copied once.
//
// An array reached only through an EMBEDDED struct is not cloned here, as it is not by any other copy
// site: typeNeedsValueClone does not walk embeds (see its header).
func (v *Visitor) conversionOperandCopy(target types.Type, operand ast.Expr, rendered string) string {
	if target == nil {
		return rendered
	}

	switch target.Underlying().(type) {
	case *types.Struct, *types.Array:
	default:
		return rendered
	}

	if !v.exprReadsValueNeedingClone(operand) || conversionCopiedByWrapperOperator(target, v.getExprType(operand)) {
		return rendered
	}

	return appendValueClone(rendered, v.getExprType(operand))
}

// conversionCopiedByWrapperOperator reports whether the conversion between target and source binds an
// inherited wrapper's operator that copies by itself: one side is a defined type written over another named
// type, that type is the other side, and the wrapper is stamped to clone (wrapperValueCloneAttr).
func conversionCopiedByWrapperOperator(target, source types.Type) bool {
	writtenOver := func(defined, base types.Type) bool {
		named, ok := types.Unalias(defined).(*types.Named)

		if !ok || wrapperValueCloneAttr(named) == "" {
			return false
		}

		// Written over another NAMED type: a type written with a struct literal is no wrapper, and its
		// literal is identical to an unnamed operand that still needs the site clone.
		rhs, found := writtenTypeSpecRHS(named.Obj())

		if !found {
			return false
		}

		if _, overNamed := types.Unalias(rhs).(*types.Named); !overNamed {
			return false
		}

		return types.Identical(types.Unalias(rhs), types.Unalias(base))
	}

	if target == nil || source == nil {
		return false
	}

	return writtenOver(target, source) || writtenOver(source, target)
}
