// structConversionCopy.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "go/types"

// THE COPY A CONVERSION TAKES. Go converts a copy of its operand: after `c := chord(lc)` a write to
// `c.Notes` does not reach `lc.Notes`. The emitted conversion operator (and go2cs-gen's function for two
// foreign types) builds its result from the operand's fields, so an `array<T>` field, or a nested or
// embedded struct holding one, shares its backing with the operand: `new chord(src.Name, src.Notes, src.In)`.
//
// The copy is taken ONCE, where the converted value is transferred, as for any other value: the
// conversion arm of exprReadsValueNeedingClone answers that a conversion reads existing storage exactly
// when its operand does, so an assignment, a return, a send or a composite-literal element clones the
// result, `var c = ((chord)lc).ΔClone();`. A conversion consumed by a read (a value-receiver call, an
// index, a field) is not transferred and copies nothing, as Go's own read of the converted value copies
// nothing observable. A fresh operand (a call, a composite literal) is reachable by no other name and is
// not cloned at all.
//
// A defined type written over another named STRUCT type, `type Counts counts`, is go2cs-gen's inherited
// wrapper, and its two operators with that base already copy (`[GoValueClone("Value")]`,
// InheritedTypeTemplate.ConversionCopy); that pair takes no clone, so the value is copied once.
//
// (Until batch 312 the operand was cloned as well, `(chord)lc.ΔClone()`, at every conversion: with the
// transfer's clone that copied twice, and a read copied once where it needs no copy.)

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
