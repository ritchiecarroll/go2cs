// constraintProxyAliasDot_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"testing"
)

// q94 (COORD 9fd51104f, from C2's §7 of the RED 8 (a) read). constraintProxyFor qualifies a FOREIGN
// constraint interface by splitting its resolved C# name at the last separator. That split was
// ASCII-DOT ONLY — while getScopeCheckedTypeName's FIRST branch answers with an imported TYPE ALIAS,
// whose separator is TypeAliasDot (`curvesꓸPoint`), because a C# identifier cannot carry a `.`:
// getAliasedTypeName mints the alias form as ReplaceAll(key, ".", TypeAliasDot). Such a qualifier holds
// NO ascii dot, so strings.LastIndex returned -1, the guard declined, and it declined SILENTLY — the
// site dropped to the box leaving the reader of the emitted C# an unexplained CS0310.
//
// ⚠ THE DECLINE IS LATENT IN THE CORPUS, WHICH IS WHY THE RULING ASKS FOR A PLANT.
// Measured with the converter instrumented at the decision point, over crypto/ecdh, crypto/ecdsa and
// both fips140 siblings: the foreign-qualifier branch is ENTERED 140 times, SUCCEEDS 140 times, and the
// decline fires ZERO times — because `ecdsa.Point` (80 lookups) and `ecdh.Point` (60) are never present
// in importedTypeAliases, while siblings in the same table are (`big.Int` 480 hits, `bigmod.Nat` 7). The
// static census agrees from the other side: of 229 glyph aliases corpus-wide only 11 name interfaces,
// and the two self-referential generic constraints (`Point<P>`, `nistPoint<T>`) are not among them. The
// populations do not intersect today, so no fixture drawn from the corpus can reach the path — the alias
// must be PLANTED.
//
// The fixture is loadCrossPackageProxyFixture, REUSED rather than re-minted: it already builds the exact
// shape (owner `curves` declaring `Point[P]`, a consumer instantiating it, both visitors carrying the
// importQueue and referencedForeignPackages that getScopeCheckedTypeName consults), and its own test
// pins the unplanted answer at `curves.P1жPoint`. Three fixtures for one shape would be two too many.

// plantForeignAlias seeds the imported-type alias for a foreign type — the entry the corpus never
// carries for a CONSTRAINT interface — so getScopeCheckedTypeName's alias branch answers with the glyph
// form. importedTypeAliases is package-GLOBAL and a real run re-makes it per package, so the plant
// restores exactly what it displaced: seeding without restoring would leak into sibling tests in this
// same package and could make another test pass or fail for this test's reason.
//
// Note the shape of the value: getAliasedTypeName returns ReplaceAll(KEY, ".", TypeAliasDot) for a
// non-const entry, so the target governs EXISTENCE only and the rendered qualifier is derived from the
// key. The target is therefore written as what a real alias would hold, not as the expected output.
func plantForeignAlias(t *testing.T, plainKey string, target string) {
	t.Helper()

	packageLock.Lock()

	// The map is NIL outside a conversion run: only packageStateOperations makes it, per package, and
	// these fixtures never run that. Assigning into it panics — the same hazard loadProxyFixture already
	// documents for constraintProxies.
	//
	// ⚠ THE PLANT MUST NOT MUTATE THE MAP IT FOUND. Restoring the map REFERENCE is a NO-OP whenever the
	// global is already non-nil, because the saved reference and the mutated object are the same map:
	// the planted key survives the restore and leaks into every later test in the package. That is not
	// hypothetical. It failed TestConstraintProxyConsumerNamesTheOwnersProxy,
	// TestConstraintProxyConsumerWithoutTheInterfaceImportQualifiesFully and this file's own ascii-dot
	// bound under the FULL SUITE, while passing under `-run ConstraintProxy`, where nothing had
	// populated the global first and the nil restore was accidentally correct.
	//
	// So the plant is COPY-ON-WRITE: build a fresh map carrying the previous contents plus the plant,
	// install that, and put the original object back untouched. Correct whether or not the global was
	// nil, and — the property that matters — independent of what ran before.
	previous := importedTypeAliases

	planted := make(map[string]string, len(previous)+1)

	for key, value := range previous {
		planted[key] = value
	}

	planted[plainKey] = target
	importedTypeAliases = planted
	packageLock.Unlock()

	t.Cleanup(func() {
		packageLock.Lock()
		importedTypeAliases = previous
		packageLock.Unlock()
	})
}

// THE CLASS, red-first. With the owner's constraint interface aliased, the resolved qualifier is
// `curvesꓸPoint` — no ascii dot anywhere — and the proxy must still be qualified, through the GLYPH.
// Against the unsplit converter LastIndex(qualified, ".") reads -1, the guard declines, and this arm
// fails with ok == false and an empty name.
//
// The expected string is CONCRETE rather than a "contains the glyph" check: the qualifier is the alias
// form up to and including the separator, and the proxy name is element + PointerPrefix+interface as
// ImplementGenerator spells it — `curvesꓸ` + `P1жPoint`. A containment assertion would also pass on a
// qualifier that kept the wrong number of bytes, which is precisely the failure the width half guards.
func TestForeignConstraintProxySplitsOnAliasDot(t *testing.T) {
	_, consumer, _, consumerCalls := loadCrossPackageProxyFixture(t)
	constraintProxies = make(map[string][3]string)
	consumer.importQueue.Add("example/proxycross/curves")

	plantForeignAlias(t, "curves.Point", "go.example.proxycross.curves_package.Point")

	funIdent, typeArgs := crossPackageInstance(t, consumer, consumerCalls, "consumerCall")
	got, ok := consumer.constraintProxySigArg(funIdent, typeArgs, 0)

	want := "curves" + TypeAliasDot + "P1" + PointerPrefix + "Point"

	if !ok {
		t.Fatalf("constraintProxyFor DECLINED an alias-qualified foreign constraint (got %q): the qualifier carries %q and no ascii dot, which is q94's silent decline", got, TypeAliasDot)
	}

	if got != want {
		t.Fatalf("alias-qualified proxy = %q, want %q (the separator must be the glyph and must advance by its WIDTH: TypeAliasDot is three bytes, so a +1 advance slices mid-rune)", got, want)
	}
}

// THE BOUND: with nothing planted the ordinary ASCII-dot qualifier resolves exactly as before — the
// shape the corpus actually carries at all 140 measured sites, and the answer loadCrossPackageProxyFixture's
// own test already pins. This arm must stay GREEN with the split reverted; it is what keeps the widening
// from being "split on anything" and proves the cut ADDS a case rather than changing one.
func TestForeignConstraintProxyAsciiDotUnchanged(t *testing.T) {
	_, consumer, _, consumerCalls := loadCrossPackageProxyFixture(t)
	constraintProxies = make(map[string][3]string)
	consumer.importQueue.Add("example/proxycross/curves")

	funIdent, typeArgs := crossPackageInstance(t, consumer, consumerCalls, "consumerCall")
	got, ok := consumer.constraintProxySigArg(funIdent, typeArgs, 0)

	want := "curves.P1" + PointerPrefix + "Point"

	if !ok || got != want {
		t.Fatalf("the ascii-dot path = %q, %v; want %q, true — this is the shape all 140 corpus sites take and the cut must leave it untouched", got, ok, want)
	}
}
