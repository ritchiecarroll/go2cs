// samePackageImplements_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/token"
	"go/types"
	"testing"
)

// syntheticPackageTypes builds a package holding one interface and a set of named types, so the
// gates below can be asked about real go/types objects rather than about strings.
type syntheticPackage struct {
	pkg *types.Package
}

func newSyntheticPackage(path, name string) syntheticPackage {
	return syntheticPackage{pkg: types.NewPackage(path, name)}
}

func (s syntheticPackage) named(name string, underlying types.Type) *types.Named {
	obj := types.NewTypeName(token.NoPos, s.pkg, name, nil)
	return types.NewNamed(obj, underlying, nil)
}

// iface builds a named interface with one nullary int-returning method of the given name.
func (s syntheticPackage) iface(name, methodName string) *types.Named {
	signature := types.NewSignatureType(nil, nil, nil, nil,
		types.NewTuple(types.NewVar(token.NoPos, s.pkg, "", types.Typ[types.Int])), false)

	method := types.NewFunc(token.NoPos, s.pkg, methodName, signature)
	underlying := types.NewInterfaceType([]*types.Func{method}, nil).Complete()

	return s.named(name, underlying)
}

// THE GATE THIS INCREMENT ADDS, and the one place the pointer form is strictly stricter than the
// value form. A VALUE record is consumed by an implicit conversion, so it needs no name and an
// unexported target is harmless — the value recorder gates the INTERFACE alone. A POINTER record is
// consumed by NAMING the generated `<T>ж<Iface>` class, and ImplementGenerator scopes that class
// `public` only when BOTH participants are public. Recording an unexported participant therefore
// advertises a class no consuming assembly can reference (CS0122) — an existence signal that is a
// lie, which is the one failure mode a record must never have.
func TestPointerRecordIsPubliclyRealizable(t *testing.T) {
	pkg := newSyntheticPackage("go2cs/ledger", "ledger")

	exportedIface := pkg.iface("Metric", "Value")
	unexportedIface := pkg.iface("probe", "depth")

	exportedTarget := pkg.named("Tally", types.NewStruct(nil, nil))
	unexportedTarget := pkg.named("tick", types.NewStruct(nil, nil))

	cases := []struct {
		name   string
		target *types.Named
		iface  *types.Named
		want   bool
	}{
		{"both exported", exportedTarget, exportedIface, true},
		{"unexported target", unexportedTarget, exportedIface, false},
		{"unexported interface", exportedTarget, unexportedIface, false},
		{"neither exported", unexportedTarget, unexportedIface, false},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			if got := pointerRecordIsPubliclyRealizable(tc.target, tc.iface); got != tc.want {
				t.Errorf("pointerRecordIsPubliclyRealizable(%s, %s) = %v, want %v",
					tc.target.Obj().Name(), tc.iface.Obj().Name(), got, tc.want)
			}
		})
	}
}

// The two realizability bounds, and the fact that they DIFFER. The VALUE bound admits a single-embed
// promotion (index length 2) because a partial struct resolves a promoted member as the converter's own
// call sites do (CrossPkgUser's `rig` embeds Device embeds Sensor, where Label lives, is the depth-3
// case both bounds reject: `CrossPkgLib_package.Label(this.Device)` is CS1503).
//
// The POINTER bound is STRICTER: direct resolution only. The ж adapter's promoted-member arms are
// keyed on embedded POINTER fields and, with exactly one present, take every unbound member
// unconditionally — sound for a DEMANDED record (the cast type-checked through that hop) but not for a
// SPECULATIVE one, whose member may come from a different embed entirely.
// StructPointerPromotionWithInterface's `MyCustomError` is the corpus instance: it embeds both the
// `Abser` interface and `*MyError`, `Abs` is promoted from the INTERFACE, and the adapter bound it
// against `MyError` — where the only candidate in scope was `time.Abs(Duration)`, CS1929 from a
// generated file. Depth is not the discriminator there (that promotion is index length 2, which the
// value bound admits); the KIND of hop is.
func TestRealizabilityBoundsDifferByForm(t *testing.T) {
	pkg := newSyntheticPackage("go2cs/depth", "depth")

	metric := pkg.iface("Metric", "Value")
	ifaceType := metric.Underlying().(*types.Interface)

	// A type declaring Value on its POINTER receiver: direct, admitted by BOTH bounds.
	direct := pkg.named("Direct", types.NewStruct(nil, nil))
	direct.AddMethod(types.NewFunc(token.NoPos, pkg.pkg, "Value",
		types.NewSignatureType(types.NewVar(token.NoPos, pkg.pkg, "d", types.NewPointer(direct)), nil, nil, nil,
			types.NewTuple(types.NewVar(token.NoPos, pkg.pkg, "", types.Typ[types.Int])), false)))

	if !generatorCanForwardMethodSet(types.NewPointer(direct), ifaceType, pkg.pkg) {
		t.Error("value bound rejected a directly declared method")
	}

	if !generatorCanForwardPointerMethodSet(types.NewPointer(direct), ifaceType, pkg.pkg) {
		t.Error("pointer bound rejected a directly declared method")
	}

	// One embed hop: `Hop` embeds Direct by value, so Value promotes at index length 2. The VALUE
	// bound admits it; the POINTER bound must NOT.
	hop := pkg.named("Hop", types.NewStruct(
		[]*types.Var{types.NewField(token.NoPos, pkg.pkg, "Direct", direct, true)}, nil))

	if !generatorCanForwardMethodSet(types.NewPointer(hop), ifaceType, pkg.pkg) {
		t.Error("value bound rejected a single-embed promotion — depth 2 must be admitted there")
	}

	if generatorCanForwardPointerMethodSet(types.NewPointer(hop), ifaceType, pkg.pkg) {
		t.Error("pointer bound ADMITTED a promoted member — the ж adapter may forward it through the wrong embed")
	}

	// Two embed hops: `Deep` embeds Hop embeds Direct, index length 3. Rejected by both.
	deep := pkg.named("Deep", types.NewStruct(
		[]*types.Var{types.NewField(token.NoPos, pkg.pkg, "Hop", hop, true)}, nil))

	if generatorCanForwardMethodSet(types.NewPointer(deep), ifaceType, pkg.pkg) {
		t.Error("value bound admitted a two-hop promotion")
	}

	if generatorCanForwardPointerMethodSet(types.NewPointer(deep), ifaceType, pkg.pkg) {
		t.Error("pointer bound admitted a two-hop promotion")
	}

	// A type with no such method at all is rejected outright by both.
	bare := pkg.named("Bare", types.NewStruct(nil, nil))

	if generatorCanForwardMethodSet(types.NewPointer(bare), ifaceType, pkg.pkg) ||
		generatorCanForwardPointerMethodSet(types.NewPointer(bare), ifaceType, pkg.pkg) {
		t.Error("a type with no matching method was admitted")
	}
}

// The pointer method set is a SUPERSET of the value method set, which is why the recorder asks both
// questions of every candidate rather than choosing one. A pair satisfied on the value set is
// satisfied on the pointer set too, and both records are wanted: Go's `T` and `*T` are two dynamic
// types, realized in C# as `partial struct T : Iface` and the `TжIface` adapter respectively.
// Dropping the pointer record for a value-satisfied pair would leave a consumer's `var i Iface = &t`
// with nothing to reference, and it would mint the local adapter this increment exists to remove.
func TestPointerMethodSetSubsumesValueMethodSet(t *testing.T) {
	pkg := newSyntheticPackage("go2cs/subsume", "subsume")

	metric := pkg.iface("Metric", "Value")
	ifaceType := metric.Underlying().(*types.Interface)

	// Value receiver: both sets implement.
	valueImpl := pkg.named("Counter", types.NewStruct(nil, nil))
	valueImpl.AddMethod(types.NewFunc(token.NoPos, pkg.pkg, "Value",
		types.NewSignatureType(types.NewVar(token.NoPos, pkg.pkg, "c", valueImpl), nil, nil, nil,
			types.NewTuple(types.NewVar(token.NoPos, pkg.pkg, "", types.Typ[types.Int])), false)))

	if !types.Implements(valueImpl, ifaceType) {
		t.Fatal("value receiver: value method set must implement")
	}

	if !types.Implements(types.NewPointer(valueImpl), ifaceType) {
		t.Error("value receiver: the POINTER method set must implement too — it is a superset")
	}

	// Pointer receiver: only the pointer set implements.
	pointerImpl := pkg.named("Tally", types.NewStruct(nil, nil))
	pointerImpl.AddMethod(types.NewFunc(token.NoPos, pkg.pkg, "Value",
		types.NewSignatureType(types.NewVar(token.NoPos, pkg.pkg, "t", types.NewPointer(pointerImpl)), nil, nil, nil,
			types.NewTuple(types.NewVar(token.NoPos, pkg.pkg, "", types.Typ[types.Int])), false)))

	if types.Implements(pointerImpl, ifaceType) {
		t.Error("pointer receiver: the VALUE method set must NOT implement")
	}

	if !types.Implements(types.NewPointer(pointerImpl), ifaceType) {
		t.Fatal("pointer receiver: the pointer method set must implement")
	}
}
