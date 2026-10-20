// constraintPublicization_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// AN UNEXPORTED CONSTRAINT ON AN EXPORTED GENERIC MEMBER. internal/runtime/gc/internal/gen (Go 1.26)
// declares `type wrap[T Value] interface { Value; wrap(x *op) T }` and constrains the exported
// `func Arg[W wrap[T], T Word](fn *Func) T` and `func Deref[W wrap[T], T Value](ptr Ptr[W]) T` on it.
// The public static methods emit `where W : Δwrap<T>` over an internal interface: CS0703 at gen.cs and
// val.cs, and the TypeGenerator's CS0314 cascade after them, on all three flavours. collectPublicizedTypes
// walked an exported func's params and results and never its type parameters' constraints.
//
// ⚠ ONLY A CONSTRAINT C# NAMES IS PUBLICIZED. A type-set-only constraint (`~int | ~int8`) renders as a
// breadcrumb comment beside lifted operator interfaces, so the clause never names it; the control arm
// holds that line.
package main

import "testing"

const constraintPublicizationFixture = `package constraint

type Value interface{ kind() string }

type Word interface {
	Value
	isWord()
}

type op struct{}

// THE SUBJECT, gen's exact shape: an unexported generic interface constraint on an exported generic func.
type wrap[T Value] interface {
	Value
	wrap(x *op) T
}

func Arg[W wrap[T], T Word]() T {
	var x W
	return x.wrap(nil)
}

// A constraint's TYPE ARGUMENT is spelled inside the clause too.
type getter[T any] interface{ get() T }

type hiddenElem struct{}

func Nested[G getter[hiddenElem]](g G) { _ = g.get() }

// An exported generic TYPE names its constraint in its own public declaration.
type sealed interface{ seal() }

type Holder[T sealed] struct{ v T }

// CONTROL: an UNEXPORTED generic func is internal, so its constraint may stay internal.
type private interface{ p() }

func use[T private](x T) { x.p() }

// CONTROL: a type-set-only constraint is never named by the emitted clause.
type small interface{ ~int | ~int8 }

func Twice[T small](x T) T { return x + x }
`

// ARM 1, THE SUBJECT. The exported func's constraint is publicized, and the cascade carries the publicized
// interface's member signature (`wrap(x *op)` is a public interface member once wrap is public).
func TestAnExportedGenericFuncPublicizesItsConstraint(t *testing.T) {
	isolateConverterPathGlobals(t)
	publicized := collectFixturePublicized(t, constraintPublicizationFixture)

	if !publicized["wrap"] {
		t.Errorf("Arg's constraint wrap[T] must be publicized (CS0703); publicized: %v", publicized)
	}

	if !publicized["op"] {
		t.Errorf("the publicized constraint's member signature must cascade to op; publicized: %v", publicized)
	}
}

// ARM 2. A constraint's type argument and an exported generic type's constraint are publicized too.
func TestAConstraintTypeArgumentAndAGenericTypesConstraintArePublicized(t *testing.T) {
	isolateConverterPathGlobals(t)
	publicized := collectFixturePublicized(t, constraintPublicizationFixture)

	for _, name := range []string{"getter", "hiddenElem", "sealed"} {
		if !publicized[name] {
			t.Errorf("%s is named by a public where clause and must be publicized; publicized: %v", name, publicized)
		}
	}
}

// ARM 3, THE CONTROLS. An unexported generic func exposes nothing, and a type-set-only constraint is
// never named in C#.
func TestUnexportedOrTypeSetConstraintsStayInternal(t *testing.T) {
	isolateConverterPathGlobals(t)
	publicized := collectFixturePublicized(t, constraintPublicizationFixture)

	for _, name := range []string{"private", "small"} {
		if publicized[name] {
			t.Errorf("%s is not named by any public where clause and must stay internal; publicized: %v", name, publicized)
		}
	}
}
