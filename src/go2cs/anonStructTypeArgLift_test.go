// anonStructTypeArgLift_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the lift of a NON-EMPTY anonymous struct reached ONLY through a generic TYPE-ARGUMENT
// position. `reflect.TypeFor[struct{ f int }]()` is the measured shape: it appears in Go 1.24's
// reflect test sources (all_test.go:3547 inside the pre-existing TestAllocations, and :6921 inside
// the new TestTypeFieldReadOnly) and in no earlier release, and it made the whole `reflect` row
// unreadable — `go2cs -tests -test-action all` exited 1 at CONVERT, in both build configurations,
// before any build, run or comparison could happen.
//
// The cause was a REGISTRATION gap, not a resolution one. visitStructType publishes a lifted
// anonymous struct into the package-wide dedup registry only for a package-level lift or one at a
// call boundary (`!v.inFunction || v.liftAtCallBoundary`); a type argument is a position that gate
// never enumerated, so a function-scoped one minted no lift at all and the deferred marker fell
// through to "emit the raw Go signature and record the site".
//
// ⚠ THE FIXTURE CARRIES THE SHAPE AT **TWO** SITES WITH **ONE** SIGNATURE ON PURPOSE. The resolver
// warns and records once per SIGNATURE, not once per site, so a guard anchored on the reported
// line alone would go green on a fix that repaired one occurrence and left the other emitting raw
// Go. That is exactly how the real defect presented: "1 unresolved dynamic type(s)" named
// all_test.cs(4201) while all_test.cs(8451) carried the same raw text unmentioned.
//
// The two controls in the same fixture are what make the trigger one axis rather than three:
// `struct{}` and a named type argument both resolve on the BROKEN converter, so the guard cannot
// be satisfied by anything as broad as "anonymous type arguments do not work".

package main

import (
	"regexp"
	"strings"
	"testing"
)

// anonTypeArgFixture instantiates one generic function four ways from four different enclosing
// functions. Sites A and B share a signature; Empty and Named are the controls.
//
// The two sites sit in SEPARATE functions deliberately: a single function containing both would
// also be repaired by a purely function-local lift, and the registry publication this guard exists
// for would go unmeasured.
const anonTypeArgFixture = `package main

type Box struct{ v any }

func typeFor[T any]() Box {
	var z T
	return Box{v: z}
}

func SiteA() Box { return typeFor[struct{ f int }]() }

func SiteB() Box { return typeFor[struct{ f int }]() }

func ControlEmpty() Box { return typeFor[struct{}]() }

func ControlNamed() Box { return typeFor[int]() }

func main() {}
`

// rawGoStructText matches an anonymous Go struct signature that survived into the emission. C#
// has no such syntax, so any match is a file that cannot compile — this is the same fallback text
// the unresolved-type record carries.
var rawGoStructText = regexp.MustCompile(`struct\{[^}]*\w[^}]*\}`)

// typeForCallArgument captures what each `typeFor<…>()` CALL SITE named as its type argument.
//
// ⚠ The `return ` anchor is load-bearing. Without it the pattern also matches the generic
// DECLARATION `internal static Box typeFor<T>() {`, which made the site count read 5 instead of 4 —
// caught by the count assertion rather than by anything looking wrong, which is the whole reason
// the count is asserted before the sites are read positionally.
var typeForCallArgument = regexp.MustCompile(`return typeFor<([^>]*)>\(\);`)

// TestRawGoStructTextDetectorFires is the positive control for this file's own instrument
// (safety floor 13: a gate that has never been made to fail proves nothing). The assertions below
// are worth nothing if the detector cannot see the text they are looking for, and a detector that
// silently matches nothing would make every one of them pass.
func TestRawGoStructTextDetectorFires(t *testing.T) {
	if !rawGoStructText.MatchString("    return typeFor<struct{f int}>();") {
		t.Fatal("the raw-Go-struct detector does not match the exact text the defect emitted; " +
			"every assertion in this file that relies on it is vacuous")
	}

	if rawGoStructText.MatchString("    return typeFor<SiteA_T>();") {
		t.Fatal("the raw-Go-struct detector matches a correctly lifted emission, so it cannot " +
			"distinguish the fixed state from the broken one")
	}

	// The EMPTY struct must not be read as the defect: it resolves on the broken converter too,
	// and counting it would make the guard pass for the wrong reason.
	if rawGoStructText.MatchString("    return typeFor<struct{}>();") {
		t.Fatal("the detector matches an empty anonymous struct; the trigger is a NON-EMPTY one")
	}
}

// TestAnonymousStructTypeArgumentLiftsAtEverySite is the guard. It asserts the property the
// defect violated at BOTH sites, not at the one the resolver happens to report.
func TestAnonymousStructTypeArgumentLiftsAtEverySite(t *testing.T) {
	isolateConverterPathGlobals(t)
	withDynamicTypeRegistry(t, func() {
		mainCs := convertFunnelFixture(t, "example.com/anonta", "anonta", anonTypeArgFixture)

		arguments := typeForCallArgument.FindAllStringSubmatch(mainCs, -1)

		if len(arguments) != 4 {
			t.Fatalf("expected 4 `typeFor<…>()` call sites in the emission, found %d:\n%s",
				len(arguments), mainCs)
		}

		// Sites are in source order: SiteA, SiteB, ControlEmpty, ControlNamed.
		siteA, siteB := arguments[0][1], arguments[1][1]
		controlEmpty, controlNamed := arguments[2][1], arguments[3][1]

		// ⚠ Both sites, separately. The resolver reports once per signature, so asserting only on
		// the first would pass while the second still carried raw Go.
		for i, site := range []string{siteA, siteB} {
			if rawGoStructText.MatchString(site) {
				t.Errorf("site %d emitted the raw Go type argument %q, which is not valid C#", i+1, site)
			}
		}

		// The whole file, because a raw signature can also reach a DECLARATION or an attribute,
		// not only a call site.
		if leftovers := rawGoStructText.FindAllString(mainCs, -1); len(leftovers) > 0 {
			t.Errorf("emission still carries raw Go struct text %q", leftovers)
		}

		// The two sites share one Go type, so they must share one C# type — the point of
		// publishing into the package-wide registry rather than lifting per function.
		if siteA != siteB {
			t.Errorf("the two sites share one Go signature but emitted different type arguments: "+
				"%q and %q", siteA, siteB)
		}

		// A name is not enough: the lifted type must actually be DECLARED, or the emission names
		// a type that does not exist.
		if !strings.Contains(mainCs, "struct "+siteA) && !strings.Contains(mainCs, "record struct "+siteA) {
			t.Errorf("no declaration of the lifted type %q in the emission:\n%s", siteA, mainCs)
		}

		// ⚠ And exactly ONE declaration, which is the half that proves the registry PUBLICATION
		// rather than merely a per-function lift. Before the fix the converter minted and declared
		// `SiteA_type` AND `SiteB_type` — two C# types for one Go type — while both call sites
		// still carried raw Go, so "a lift exists" was true and worth nothing. The second site must
		// now reuse the first's name and declare nothing of its own.
		if strings.Contains(mainCs, "struct SiteB_type") {
			t.Errorf("the second site minted its own lifted type; one Go type became two C# types:\n%s", mainCs)
		}

		// The controls, which resolve on the BROKEN converter too. They are here so a fix cannot
		// be credited for something broader than the one axis, and so a regression in either is
		// attributed to this change rather than hunted elsewhere.
		if controlEmpty != "EmptyStruct" {
			t.Errorf("control: empty anonymous struct argument emitted %q, want EmptyStruct", controlEmpty)
		}

		if controlNamed != "nint" {
			t.Errorf("control: named `int` argument emitted %q, want nint", controlNamed)
		}
	})
}

// TestAnonymousStructTypeArgumentRecordsNoUnresolvedType is the other half, and it is the half the
// `-tests` pipeline actually gates on: the W2b record is what turns this shape into `Conversion
// failed: N unresolved dynamic type(s)` and exit 1. An emission that looked right while still
// recording a site would leave the row unreadable for the same reason as before.
func TestAnonymousStructTypeArgumentRecordsNoUnresolvedType(t *testing.T) {
	isolateConverterPathGlobals(t)
	withDynamicTypeRegistry(t, func() {
		convertFunnelFixture(t, "example.com/anontarec", "anontarec", anonTypeArgFixture)

		if unresolved := takeUnresolvedDynamicTypes(); len(unresolved) > 0 {
			t.Fatalf("conversion recorded %d unresolved dynamic type(s), so `-tests` would still "+
				"refuse this package: %v", len(unresolved), unresolved)
		}
	})
}
