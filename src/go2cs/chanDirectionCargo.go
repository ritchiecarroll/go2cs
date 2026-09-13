// chanDirectionCargo.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/types"
	"strings"
)

// A Go channel's DIRECTION is part of its type, and it is the one part the managed emission cannot
// carry: `chan T`, `chan<- T` and `<-chan T` all render as golib's `channel<T>`, distinguished only
// by the `/*<-*/` marker comment the type renderer places for the reader. So the direction is
// carried on the VALUE instead, as descriptor cargo, exactly the way a fixed-size array's length is
// (see emitGoArrayDimsAttribute and arrayZeroValueArgs): the reflection bridge reads it back off
// the live channel and stamps it on the abi.Type, which is what makes reflect.Type.ChanDir() and
// String() answer `chan<- string` rather than the bidirectional type.
//
// The emission sites that stamp it are the places a directional channel VALUE is born in
// converted code — the same finite set the array dims occupy, position for position:
//
//   - `make(chan<- T[, n])` — the MADE channel (convCallExpr's make arm);
//   - the ZERO value of a directional channel type: a `var` declaration (LOCAL and GLOBAL — both
//     rungs of visitValueSpec's inline ladder since 2026-09-01; before that the doc claimed this
//     site and the implementation lacked it, which is exactly how reflect's TestChanOf read
//     `TypeOf(left)` as bidirectional), a named result (zeroValueInitializer), a struct FIELD's
//     initializer (visitStructType), and an array ELEMENT (visitArrayType);
//   - `new(chan<- T)`, whose pointee is that same zero value (convCallExpr's new arm);
//   - a CONVERSION OF NIL to a directional type — `(chan<- string)(nil)` — which mints the same
//     zero value through a cast (convCallExpr's nil-conversion arm, added 2026-09-01: see below).
//
// What is deliberately NOT stamped, and why — AMENDED 2026-09-01, because the original rule's
// premise died on measurement:
//
//   - a NARROWING conversion of a LIVE channel (`var s chan<- int = ch`) keeps the source value's
//     direction. Go makes a new value of a new TYPE there, but the narrowing has no construction
//     to hook — it is a plain struct copy — and stamping it would mean an explicit call at every
//     assignment, argument and return of a directional channel in the corpus (89 such positions).
//     The ORIGINAL r39d exclusion covered the nil-cast and the zero-value var too, justified as
//     "a datum no measured consumer reads" — and then reflect's own suite measured FOUR consumers
//     (TestAll #12, TestTypes #20-22, TestChanOf, TestChanOfDir), so the coordinator ruled the
//     narrowing CARRIED (2026-09-01) and the construction-shaped positions above joined the stamp
//     set. What remains excluded is only the live-copy narrowing, which no measured consumer
//     reads YET; if one appears, this paragraph is the precedent for how the rule falls.
//   - a DEFINED channel type (`type closeWaiter chan struct{}`) is not stamped, for the same reason
//     a defined ARRAY type carries no dims: its managed form is a go2cs-gen wrapper struct rather
//     than `channel<T>`, so there is no field to carry the cargo and no reader to consume it. An
//     ALIAS for a channel type IS its target, so it is stamped (types.Unalias resolves it).
//   - a type PARAMETER instantiated at a channel type: `make<T>(…)` routes through ISupportMake,
//     which has no direction-taking form.

// chanDirCargoName renders the golib GoChanDir member for a directional, undefined Go channel type,
// or "" when the type carries no direction this emission stamps.
func chanDirCargoName(t types.Type) string {
	if t == nil {
		return ""
	}

	resolved := types.Unalias(t)

	if _, isNamed := resolved.(*types.Named); isNamed {
		return ""
	}

	chanType, isChan := resolved.Underlying().(*types.Chan)

	if !isChan {
		return ""
	}

	switch chanType.Dir() {
	case types.SendOnly:
		return "GoChanDir.Send"
	case types.RecvOnly:
		return "GoChanDir.Recv"
	}

	return ""
}

// Increment D: a channel whose ELEMENT is itself a channel or an array carries a per-level direction
// CHAIN and the element's array dims on the value, as ONE cargo (golib ChanCargo). The emission rule
// that keeps every existing site byte-identical: the cargo form is emitted ONLY when the NORMALIZED
// chain has more than one entry or the element carries dims; a scalar direction with no dims keeps
// the pre-D forms (.SendOnly / .RecvOnly / the GoChanDir constructor argument) exactly as they were,
// and a bare bidirectional channel still emits nothing. Production std has ZERO such sites (the D
// census, 2026-09-04), so the two-seeded -stdlib diff is predicted EMPTY by this construction and the
// footprint lives in reflect's test emission, where its seven consumers are.

// chanDirChain walks the nested channel levels of an undefined channel type, outermost first, and
// NORMALIZES the chain exactly as abi.normalizeChanDirChain does: trailing bidirectional entries are
// trimmed (they say nothing their absence would not), interior ones are kept (they are positional:
// `chan (<-chan T)` is [Both, Recv]), and an all-bidirectional chain is nil. The walk stops at a
// DEFINED channel type at any level, which is a go2cs-gen wrapper with no field to carry cargo.
func chanDirChain(t types.Type) []types.ChanDir {
	var chain []types.ChanDir

	for cur := t; cur != nil; {
		resolved := types.Unalias(cur)

		if _, isNamed := resolved.(*types.Named); isNamed {
			break
		}

		chanType, isChan := resolved.Underlying().(*types.Chan)

		if !isChan {
			break
		}

		chain = append(chain, chanType.Dir())
		cur = chanType.Elem()
	}

	end := len(chain)

	for end > 0 && chain[end-1] == types.SendRecv {
		end--
	}

	if end == 0 {
		return nil
	}

	return chain[:end]
}

// chanElemArrayDims is the channel sibling of sliceElemArrayDims: the array length(s) of an undefined
// channel type's element, outermost first, or nil when the element is not a fixed-size array.
func chanElemArrayDims(t types.Type) []int64 {
	if t == nil {
		return nil
	}

	resolved := types.Unalias(t)

	if _, isNamed := resolved.(*types.Named); isNamed {
		return nil
	}

	chanType, isChan := resolved.Underlying().(*types.Chan)

	if !isChan {
		return nil
	}

	var dims []int64

	for elem := chanType.Elem(); ; {
		array, ok := elem.Underlying().(*types.Array)

		if !ok {
			break
		}

		dims = append(dims, array.Len())
		elem = array.Elem()
	}

	return dims
}

func goChanDirMember(dir types.ChanDir) string {
	switch dir {
	case types.SendOnly:
		return "GoChanDir.Send"
	case types.RecvOnly:
		return "GoChanDir.Recv"
	}

	return "GoChanDir.Both"
}

// chanCargoExpr renders the golib ChanCargo.Of(chain, dims) expression for a channel type that needs
// the unified cargo, or "" for every type the pre-D forms already describe (see the rule above).
func chanCargoExpr(t types.Type) string {
	chain := chanDirChain(t)
	dims := chanElemArrayDims(t)

	if len(dims) == 0 && len(chain) <= 1 {
		return ""
	}

	chainExpr := "null"

	if len(chain) > 0 {
		members := make([]string, len(chain))

		for i, dir := range chain {
			members[i] = goChanDirMember(dir)
		}

		chainExpr = "new GoChanDir[] { " + strings.Join(members, ", ") + " }"
	}

	dimsExpr := "null"

	if len(dims) > 0 {
		values := make([]string, len(dims))

		for i, dim := range dims {
			values[i] = fmt.Sprintf("%d", dim)
		}

		dimsExpr = "new nint[] { " + strings.Join(values, ", ") + " }"
	}

	return fmt.Sprintf("ChanCargo.Of(%s, %s)", chainExpr, dimsExpr)
}

// chanDirNilValue renders the NIL channel of a directional type — the zero VALUE of `chan<- T` /
// `<-chan T`, which is still a value whose Go type has a direction — as golib's SendOnly/RecvOnly
// factory on the emitted channel type. Returns "" for any type with no direction to carry, which
// leaves every existing zero-value emission byte-identical.
func (v *Visitor) chanDirNilValue(t types.Type) string {
	// Increment D: a channel of channels or of arrays takes the cargo-bearing nil factory; every
	// other directional type keeps the pre-D member below, byte for byte.
	if cargo := chanCargoExpr(t); cargo != "" {
		return v.getCSharpTypeName(t) + ".Nil(" + cargo + ")"
	}

	var member string

	switch chanDirCargoName(t) {
	case "GoChanDir.Send":
		member = ".SendOnly"
	case "GoChanDir.Recv":
		member = ".RecvOnly"
	default:
		return ""
	}

	return v.getCSharpTypeName(t) + member
}

// chanDirNarrowedValue wraps an already-rendered channel expression in golib's
// `.WithDirection(...)` re-stamp when the value NARROWS from a bidirectional Go channel type into a
// directional one — `var s <-chan T = ch`, the argument of a `func(<-chan T)`, a directional result.
// Returns "" when the flow is not a narrowing, which leaves every other emission byte-identical.
//
// This is the LIVE-COPY position the exclusion at the top of this file deferred, and it is the one
// the reflection bridge could not answer without: the construction-shaped positions stamp the
// direction at BIRTH (make/zero/new/nil-cast, through channel<T>'s directional constructors), but a
// narrowing has no construction to hook — Go makes a value of a new TYPE out of a value that
// already exists. The re-stamp shares the SAME core, so Go's channel identity (==, Equals and the
// hash all read the core) and every operation survive; only the carried direction differs.
// reflect's TestMakeFuncInvalidReturnAssignments is the measured consumer: it returns a `<-chan int`
// narrowed from `make(chan int)` into a `chan int` result, and Go REFUSES that widening — unstamped,
// the marshalling identity arm sees two bidirectional `channel<int>` values and admits it.
//
// dst is the STATIC target type (the declared var type, the parameter type, the result type); src is
// the expression's own type. A DEFINED channel type on either side (`type closeWaiter chan struct{}`)
// is a go2cs-gen wrapper with no direction cargo, exactly as chanDirCargoName already refuses.
func (v *Visitor) chanDirNarrowedValue(dst, src types.Type, rendered string) string {
	if dst == nil || src == nil || rendered == "" {
		return ""
	}

	dstDir := chanDirCargoName(dst)

	if dstDir == "" {
		return ""
	}

	// The SOURCE must be a bidirectional, undefined channel: a source that already carries the
	// destination's direction is not a narrowing (it was stamped at birth or narrowed upstream),
	// and a named/defined source has no direction cargo to re-stamp.
	srcResolved := types.Unalias(src)

	if _, isNamed := srcResolved.(*types.Named); isNamed {
		return ""
	}

	srcChan, isChan := srcResolved.Underlying().(*types.Chan)

	if !isChan || srcChan.Dir() != types.SendRecv {
		return ""
	}

	return fmt.Sprintf("%s.WithDirection(%s)", rendered, dstDir)
}
