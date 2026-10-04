// anonStructPublicization_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// THE ANONYMOUS STRUCT UNDER AN EXPORTED VAR OR FIELD. collectUnexportedNamedTypes peels pointers,
// slices, arrays, maps, chans and signatures looking for package-local unexported named types that
// an exported surface would expose — but it had no case for an anonymous STRUCT, so the walk
// stopped at one and never reached its fields.
//
// time's abs_test.go is the corpus instance, new at 1.24.13:
//
//	var InternalTests = []struct {
//		Name string
//		Test func(testingT)
//	}{…}
//
// `testingT` is never publicized, while the LIFT the anonymous struct becomes is emitted PUBLIC —
// generatedTypeScope reads the synthesized name `InternalTestsᴛ1`, whose capital I belongs to the
// exported VAR and to no type Go exported. A public field then carries an internal type: CS0052 at
// abs_test.cs, plus CS0050/CS0051 on the members the TypeGenerator generates from the same struct.
//
// ⚠ THE NEGATIVE CONTROLS CARRY AS MUCH WEIGHT AS THE POSITIVE, because this arm REVERSES a
// deliberate omission (collectSignatureTypes' doc argues an exported var over an anonymous struct
// is legal "when its own enclosing type is internal"). That argument holds for a FUNCTION-LOCAL
// lift, which localTypeAccess pins internal and which this package-scope walk cannot reach. It does
// not hold for a lift named after an exported package-level var or field. The controls below are
// what keep the new case from becoming "publicize everything reachable from an anonymous struct".
package main

import "testing"

// collectFixturePublicized loads a one-file module and returns the publicize pass's result by
// simple type name.
func collectFixturePublicized(t *testing.T, source string) map[string]bool {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":  "module example/anon\n\ngo 1.23\n",
		"anon.go": source,
	})

	production := loadProductionForDir(t, dir)

	resetPackageState(production)
	packagePublicizedTypes = nil
	packagePublicizedLiftedTypes = nil

	collectPublicizedTypes(production.Types, production.Fset)

	return publicizedTypeNames()
}

const anonPublicizationFixture = `package anon

type testingT interface{ Helper() }

type hiddenElem struct{}

type hiddenDeep struct{}

type hiddenUnexportedField struct{}

type hiddenLocalOnly struct{}

// THE SUBJECT, time's exact shape: an exported package-level var whose type is a SLICE of an
// anonymous struct with an exported field over a signature mentioning an unexported interface.
var InternalTests = []struct {
	Name string
	Test func(testingT)
}{}

// A DIRECT anonymous-struct var, no slice to peel first.
var Direct = struct{ Elem hiddenElem }{}

// NESTED one anonymous struct inside another, to prove the recursion is the same walk.
var Nested = struct {
	Inner struct{ Deep hiddenDeep }
}{}

// NEGATIVE CONTROL 1: the anonymous struct's field is UNEXPORTED, so the emitted field is internal
// and may hold an internal type. Publicizing here would be the blanket the omission warned about.
var Exported = struct{ hidden hiddenUnexportedField }{}

// NEGATIVE CONTROL 2: the VAR is unexported, so nothing public carries the struct at all.
var unexported = struct{ Elem hiddenLocalOnly }{}
`

// ARM 1, THE SUBJECT. The walk must reach through the slice, into the anonymous struct, and through
// the field's signature to the unexported interface.
func TestAnAnonymousStructUnderAnExportedVarPublicizesItsFieldTypes(t *testing.T) {
	publicized := collectFixturePublicized(t, anonPublicizationFixture)

	if !publicized["testingT"] {
		t.Fatalf("time's shape: an exported var over []struct{…func(testingT)} must publicize testingT; publicized: %v", publicized)
	}

	if !publicized["hiddenElem"] {
		t.Errorf("a DIRECT anonymous-struct var must publicize its exported field's type; publicized: %v", publicized)
	}

	if !publicized["hiddenDeep"] {
		t.Errorf("the recursion must reach a NESTED anonymous struct; publicized: %v", publicized)
	}
}

// ARM 2, THE CONTROLS. An unexported field of the anonymous struct emits internal and holds an
// internal type legally; an unexported var exposes nothing. Both must stay unpublicized, or the new
// case is a blanket rather than a rule.
func TestTheAnonymousStructWalkStaysBoundedByExportedness(t *testing.T) {
	publicized := collectFixturePublicized(t, anonPublicizationFixture)

	if publicized["hiddenUnexportedField"] {
		t.Errorf("an UNEXPORTED field of the anonymous struct emits internal and forces nothing; publicized: %v", publicized)
	}

	if publicized["hiddenLocalOnly"] {
		t.Errorf("an UNEXPORTED var carries no public surface at all; publicized: %v", publicized)
	}
}
