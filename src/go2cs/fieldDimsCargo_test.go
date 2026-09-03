// fieldDimsCargo_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/token"
	"go/types"
	"testing"
)

// TestFieldDimsCargo pins the struct-FIELD half of the array-length cargo: the positions where a
// field's type reaches an array through a hop that has no value and no initializer to measure, and
// therefore has to carry the length in the emitted C#.
//
// The discriminator is what the ZERO INSTANCE can see. A field that IS an array is emitted with
// `= new(N)` and reads back off that instance, so it is NOT stamped — duplicating it would churn
// every array-bearing struct in the corpus. A field BEHIND a pointer reads back a nil pointer, and
// a map field reads back a nil map whose key and element types no entry could reveal; both are
// ordinary shapes at a decode target, which is exactly a struct nothing has populated yet.
//
// The rows below are the whole boundary: aliases resolve, defined types do not (the same sentence
// the chan-direction cargo draws), a pointer of any depth carries ONE stamp because the cargo
// passes down unshifted at every hop, and a second nesting level is not carried at all — the cargo
// has one Elem() slot and one Key() slot and no measured consumer asks for more.
func TestFieldDimsCargo(t *testing.T) {
	byteType := types.Typ[types.Byte]
	intType := types.Typ[types.Int]
	stringType := types.Typ[types.String]
	pkg := types.NewPackage("go2cs/test", "test")

	array2 := types.NewArray(stringType, 2)
	array3 := types.NewArray(intType, 3)
	nested := types.NewArray(types.NewArray(intType, 3), 2)
	aliased := types.NewAlias(types.NewTypeName(token.NoPos, pkg, "words", nil), types.NewArray(intType, 4))
	named := types.NewNamed(types.NewTypeName(token.NoPos, pkg, "Row", nil), array3, nil)

	// encoding/gob's T1.Marr — key AND element are arrays, and gob reads both through
	// reflect.Type.Key()/Elem() while decoding into a value it has not filled yet.
	gobMarr := types.NewMap(array2, types.NewArray(types.NewPointer(types.Typ[types.Float64]), 2))

	cases := []struct {
		name string
		typ  types.Type
		want string
	}{
		// The two measured shapes.
		{"map with array key and array elem", gobMarr, "[GoArrayDims(2), GoMapKeyDims(2)]"},
		{"pointer to array", types.NewPointer(array3), "[GoArrayDims(3)]"},

		// One stamp covers every pointer depth: each hop hands the cargo down unshifted.
		{"pointer chain to array", types.NewPointer(types.NewPointer(types.NewPointer(array3))), "[GoArrayDims(3)]"},

		// Each map accessor is fed from its own slot, independently.
		{"map with array elem only", types.NewMap(stringType, array3), "[GoArrayDims(3)]"},
		{"map with array key only", types.NewMap(array2, intType), "[GoMapKeyDims(2)]"},
		{"map with neither", types.NewMap(stringType, intType), ""},
		{"pointer to a map with an array key", types.NewPointer(types.NewMap(array2, intType)), "[GoMapKeyDims(2)]"},

		// Dimensions are outermost-first, the order Elem() consumes as dims[1:].
		{"pointer to nested array", types.NewPointer(nested), "[GoArrayDims(2, 3)]"},

		// An ALIAS is its target; a DEFINED type is a go2cs-gen wrapper with no slot to carry cargo.
		{"pointer to an alias for an array", types.NewPointer(aliased), "[GoArrayDims(4)]"},
		{"pointer to a defined array type", types.NewPointer(named), ""},
		{"map with a defined array elem", types.NewMap(stringType, named), ""},
		{"defined map type", types.NewNamed(types.NewTypeName(token.NoPos, pkg, "Set", nil), gobMarr, nil), ""},

		// A field that IS an array carries its length in its `= new(N)` initializer.
		{"array field", array3, ""},
		{"nested array field", nested, ""},

		// A slice's element DOES have somewhere to live -- the elemDims slot, the one a map's element
		// already uses. This row emitted "" while `Elem()` CONSUMED the head of the dims vector for
		// non-pointer, non-map kinds, which made a one-element vector hand the element nothing:
		// stamping it would have been inventing cargo no accessor could read, and the "" was right
		// FOR THAT REASON. `Elem()` now passes a slice's and a channel's dims down UNSHIFTED, beside
		// a pointer's and a map's, so the stamp reaches the element and the premise is gone.
		{"slice of arrays", types.NewSlice(array3), "[GoArrayDims(3)]"},
		{"chan of arrays", types.NewChan(types.SendRecv, array3), "[GoArrayDims(3)]"},
		{"slice of pointers to arrays", types.NewSlice(types.NewPointer(array3)), "[GoArrayDims(3)]"},
		{"slice of nested arrays", types.NewSlice(nested), "[GoArrayDims(2, 3)]"},

		// A second nesting level still has nowhere to live, and this row keeps its original
		// rationale unchanged: the key-dims slot is taken by the OUTER map's key, and the inner
		// map's key has no second slot.
		{"map of maps with an array key", types.NewMap(stringType, types.NewMap(array2, intType)), ""},

		// The same shape one kind over: the outer slice's single elemDims slot would have to hold
		// the INNER slice's element dims.
		{"slice of slices of arrays", types.NewSlice(types.NewSlice(array3)), ""},

		// Everything else is untouched, which is what keeps the corpus footprint to the shapes above.
		{"scalar", intType, ""},
		{"pointer to a scalar", types.NewPointer(intType), ""},
		{"pointer to a slice", types.NewPointer(types.NewSlice(byteType)), ""},
		{"nil type", nil, ""},
	}

	for _, c := range cases {
		if got := emitFieldDimsAttributes(c.typ); got != c.want {
			t.Errorf("%s: emitFieldDimsAttributes(%s) = %q, want %q", c.name, c.typ, got, c.want)
		}
	}

	// The two slots are independent and named for the accessor each feeds: elemDims is what Elem()
	// hands down, keyDims what Key() does. Reading them off one map is the whole of the design.
	elemDims, keyDims := fieldCargoDims(gobMarr)

	if len(elemDims) != 1 || elemDims[0] != 2 {
		t.Errorf("map element dims = %v, want [2]", elemDims)
	}

	if len(keyDims) != 1 || keyDims[0] != 2 {
		t.Errorf("map key dims = %v, want [2]", keyDims)
	}

	// A POINTER's stamp is the POINTEE's dims, never a dimension of its own — the invariant the
	// bridge relies on when it passes the cargo through Elem() unshifted.
	if elemDims, keyDims = fieldCargoDims(types.NewPointer(nested)); len(elemDims) != 2 || elemDims[0] != 2 || elemDims[1] != 3 || keyDims != nil {
		t.Errorf("pointer-to-nested-array dims = %v, %v, want [2 3], []", elemDims, keyDims)
	}
}
