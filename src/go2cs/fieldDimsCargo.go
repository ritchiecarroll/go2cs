// fieldDimsCargo.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/types"
	"strings"
)

// A Go array's LENGTH is part of its type and the managed emission cannot hold it -- `[2]string`
// and `[3]string` are one `array<@string>` -- so the length travels as descriptor cargo from
// whichever source knew it (see emitGoArrayDimsAttribute and arrayZeroValueArgs). For a struct
// FIELD that source has always been the field's own ZERO VALUE: the converter emits `= new(2)` and
// GoReflect.FieldArrayDims measures it off a cached zero instance of the declaring struct.
//
// That route reaches an array the field IS. It reaches nothing an array is BEHIND:
//
//   - `N *[3]float64` -- the zero instance's field is a nil `ж<array<float64>>`, and a nil pointer
//     has no pointee to measure. Every pointer depth is the same case (`***[3]int`).
//   - `Marr map[[2]string][2]*float64` -- the zero instance's field is a nil map, and even a
//     populated one reveals nothing: a map ENTRY is a value, and Key()/Elem() answer for the TYPE.
//
// Both are ordinary shapes at a DECODE TARGET, which is exactly a struct nothing has populated yet:
// encoding/gob reaches both through `reflect.Type.Field(i)` while filling a fresh value, reads
// `t.Len()` off the array it lands on (compatibleType, decode.go:1058) and off `mtyp.Key()` /
// `mtyp.Elem()` (decodeMap), and rejects the wire when the length answers 0. So the datum has to be
// IN the emitted C#, which is the same conclusion the func-parameter position reached: a type-only
// position has no value and no initializer, and an attribute is what is left.
//
// Two attributes, named for the accessor each feeds:
//
//   - `[GoArrayDims(...)]` -- what `Elem()` hands down. The meaning is unchanged from the parameter
//     position and from the descriptor's own carried dims: an array's own tail, a POINTER's
//     pointee, a MAP's element. One stamp covers any pointer depth, because a pointer hands its
//     cargo down UNSHIFTED at every hop.
//   - `[GoMapKeyDims(...)]` -- what `Key()` hands down, a map type's second accessor and the one
//     with no slot in the rule above.
//
// What is deliberately NOT stamped, and why:
//
//   - a field that IS an array. Its `= new(N)` initializer already carries the length, through a
//     route that also survives a copy; stamping it would duplicate the datum and churn every
//     array-bearing struct in the corpus for nothing.
//   - a DEFINED array or map type (`type Board [3]int`, `type Set map[[2]string]bool`), for the
//     reason a defined CHANNEL type carries no direction: its managed form is a go2cs-gen wrapper
//     rather than `array<T>`/`map<K,V>`. An ALIAS for one IS its target and is stamped
//     (types.Unalias resolves it). This is the same one-sentence boundary the chan-direction cargo
//     draws -- aliases resolve, defined types do not.
//   - a SECOND nesting level -- `map[K]map[[2]string]V`, `[][2]int`, a func field's parameters. The
//     cargo has exactly one Elem() slot and one Key() slot, so a second level has nowhere to live,
//     and no measured consumer asks (the r39d rule).

// fieldCargoDims returns the array dims a struct field of type t must carry so the reflection
// bridge can answer for the arrays its type reaches through a hop with no value to measure:
// elemDims is what Elem() hands down, keyDims what Key() does. Both are empty for a field whose
// type reaches no such array.
func fieldCargoDims(t types.Type) (elemDims []int64, keyDims []int64) {
	if t == nil {
		return nil, nil
	}

	resolved := types.Unalias(t)
	behindPointer := false

	// A pointer of ANY depth is one hop as far as the cargo is concerned: each Elem() hands the
	// dims down unshifted, so `***[3]int` and `*[3]int` carry the identical stamp.
	for {
		pointer, isPointer := resolved.(*types.Pointer)

		if !isPointer {
			break
		}

		behindPointer = true
		resolved = types.Unalias(pointer.Elem())
	}

	switch core := resolved.(type) {
	case *types.Array:
		// Only a POINTEE needs the stamp; a field that is itself an array is carried by the
		// `= new(N)` initializer the generated parameterless constructor runs.
		if behindPointer {
			elemDims = goArrayDims(core)
		}
	case *types.Map:
		elemDims = goArrayDims(core.Elem())
		keyDims = goArrayDims(core.Key())
	case *types.Slice:
		// The map ELEMENT's case again, for the same reason and with the same remedy: a slice's
		// element is a TYPE-only position. An empty or nil `[][6]uint8` has no element to measure,
		// and a populated one does not help -- Elem() answers for the TYPE, not for whichever
		// element happened to be present. So the length has to be in the emitted C#.
		//
		// Without this case `[][6]uint8` and `[][8]uint8` reach the bridge carrying no dims, key
		// identically, and intern as ONE canonical reflect.Type. That defeats DeepEqual's own
		// `if v1.Type() != v2.Type()` guard and makes it answer TRUE for two values of different Go
		// types -- a wrong answer, not a wrong name. See docs/phase4/DESIGN-descriptor-cargo.md.
		elemDims = elementArrayDims(core.Elem())
	case *types.Chan:
		// And once more, one accessor over: a channel's element is a type-only position too, so
		// `chan [3]int` loses its element's length exactly as the slice field did.
		elemDims = elementArrayDims(core.Elem())
	}

	return elemDims, keyDims
}

// elementArrayDims returns the dims a CONTAINER's element position must carry -- the array the
// element is, or the array a pointer chain from it reaches. A pointer hands its cargo down
// unshifted at every hop, so `[]*[3]int` and `[][3]int` carry the identical stamp and each accessor
// takes back its own share on the way out.
//
// It deliberately stops at one container: a `[][]  [3]int` would need the outer slice's single
// elemDims slot to hold the inner slice's element dims, and there is one slot. That is the same
// second-nesting-level limit the map-of-maps row records, and it is left recorded rather than
// worked around (the r39d rule: never invent what no source knows).
func elementArrayDims(elem types.Type) []int64 {
	resolved := types.Unalias(elem)

	for {
		pointer, isPointer := resolved.(*types.Pointer)

		if !isPointer {
			break
		}

		resolved = types.Unalias(pointer.Elem())
	}

	if array, isArray := resolved.(*types.Array); isArray {
		return goArrayDims(array)
	}

	return nil
}

// emitFieldDimsAttributes renders the descriptor-cargo attribute line a struct field declaration
// carries -- `[GoArrayDims(3)]`, `[GoArrayDims(2), GoMapKeyDims(2)]` -- or "" when the field's type
// reaches no array the emission would otherwise lose.
func emitFieldDimsAttributes(t types.Type) string {
	elemDims, keyDims := fieldCargoDims(t)

	if len(elemDims) == 0 && len(keyDims) == 0 {
		return ""
	}

	var attributes []string

	if len(elemDims) > 0 {
		attributes = append(attributes, fmt.Sprintf("GoArrayDims(%s)", renderDimsList(elemDims)))
	}

	if len(keyDims) > 0 {
		attributes = append(attributes, fmt.Sprintf("GoMapKeyDims(%s)", renderDimsList(keyDims)))
	}

	return fmt.Sprintf("[%s]", strings.Join(attributes, ", "))
}

// renderDimsList renders array dimensions as C# attribute arguments, outermost first.
func renderDimsList(dims []int64) string {
	values := make([]string, len(dims))

	for i, dim := range dims {
		values[i] = fmt.Sprintf("%d", dim)
	}

	return strings.Join(values, ", ")
}
