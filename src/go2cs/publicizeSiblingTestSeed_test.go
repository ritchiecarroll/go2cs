// publicizeSiblingTestSeed_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"runtime"
	"slices"
	"testing"
)

// In Go the in-package `_test.go` file IS the package, so a PRODUCTION type reached by an EXPORTED
// member of that file must be at least as accessible as the consumer. collectPublicizedTypes could
// never reach that conclusion on its own: the production package go/packages hands it EXCLUDES
// `_test.go`, so the declaration and its exported consumer are never in one scope at one time.
//
// encoding/json is the corpus's first instance — `type isZeroer interface{ IsZero() bool }` in
// encode.go, and four EXPORTED fields of that type in encode_test.go's `Optionals` — and the
// production emission wrote `partial interface isZeroer` with no access modifier, which is CS0052
// x4 the moment the recompile model puts production and test in one compilation.
//
// The shape the rule's own worked cases have is unaffected: context's `testingT` is declared in the
// internal test file AND consumed there, so ONE scope holds both and the ordinary pass fires. That
// is why every other package survived this, and it is why the gap is exactly one direction rather
// than "the internal-test boundary is unhandled".
func seedFixture(t *testing.T, files map[string]string) []string {
	t.Helper()

	dir := t.TempDir()
	writeModuleFiles(t, dir, files)

	signals := collectSiblingTestSignals(dir, "seed", Options{
		targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
	})

	production := loadProductionForDir(t, dir)

	resetPackageState(production)
	packagePublicizedTypes = nil
	packagePublicizedLiftedTypes = nil
	siblingTestPublicizedTypeNames = signals.publicizedTypeNames

	t.Cleanup(func() { siblingTestPublicizedTypeNames = nil })

	collectPublicizedTypes(production.Types, production.Fset)

	names := []string{}

	for name := range publicizedTypeNames() {
		names = append(names, name)
	}

	slices.Sort(names)

	return names
}

const seedProduction = `package seed

type hidden interface{ M() bool }

type alsoHidden struct{}

type carried struct{}

type reached struct{}

func (h alsoHidden) Carry() carried { return carried{} }
`

// The SUBJECT: an unexported PRODUCTION type reached by the internal test half's exported field,
// exported result and exported parameter — the CS0052/CS0050/CS0051 trio of the sizing's fixture.
func TestSiblingTestExportedMembersPublicizeProductionType(t *testing.T) {
	publicized := seedFixture(t, map[string]string{
		"go.mod":    "module example/seed\n\ngo 1.23\n",
		"seed.go":   seedProduction,
		"x_test.go": "package seed\n\ntype Exported struct {\n\tF hidden\n}\n\nfunc (Exported) Get() hidden { return nil }\n\nfunc (Exported) Set(h hidden) {}\n",
	})

	if !slices.Contains(publicized, "hidden") {
		t.Fatalf("a PRODUCTION type reached by an EXPORTED member of the in-package test half must be publicized; publicized: %v", publicized)
	}
}

// ⚠ THE CONTROL THAT KEEPS THE SEED FROM BEING "publicize everything a test touches". The same
// production type, reached ONLY by UNEXPORTED members of the same internal test file, exposes
// nothing public and must be left alone. Without this arm the subject passes for a change that
// widens the production surface far past the defect.
func TestSiblingTestUnexportedMembersPublicizeNothing(t *testing.T) {
	publicized := seedFixture(t, map[string]string{
		"go.mod":    "module example/seed\n\ngo 1.23\n",
		"seed.go":   seedProduction,
		"x_test.go": "package seed\n\ntype holder struct {\n\tf hidden\n}\n\nfunc (holder) get() hidden { return nil }\n\nfunc peek(h hidden) {}\n",
	})

	if slices.Contains(publicized, "hidden") {
		t.Fatalf("an UNEXPORTED test member exposes nothing public and must not publicize its type; publicized: %v", publicized)
	}
}

// ⚠ THE SECOND CONTROL, and it is the ruling's own boundary: an EXTERNAL `<pkg>_test` package is a
// DIFFERENT package. Its declarations emit into a different C# class and can only reach this package
// through its exported surface, so they impose no accessibility requirement on it — and Go agrees,
// since `seed_test` cannot name `hidden` at all.
func TestExternalTestPackagePublicizesNothing(t *testing.T) {
	publicized := seedFixture(t, map[string]string{
		"go.mod":    "module example/seed\n\ngo 1.23\n",
		"seed.go":   seedProduction,
		"x_test.go": "package seed_test\n\nimport \"example/seed\"\n\ntype Exported struct {\n\tF seed.Exported\n}\n",
		"y_test.go": "package seed\n\ntype Exported struct{}\n",
	})

	if slices.Contains(publicized, "hidden") {
		t.Fatalf("an EXTERNAL test package must publicize nothing in the package under test; publicized: %v", publicized)
	}
}

// ⚠ THE RESOLUTION GATE: the scan contributes NAMES, and a name the test half declares ITSELF
// resolves to nothing in the production scope. It must be dropped silently rather than publicizing
// a production object that happens to share the spelling — there is none here, and the pass must
// not invent one or panic on the miss.
func TestTestOnlyTypeNameResolvesToNothing(t *testing.T) {
	publicized := seedFixture(t, map[string]string{
		"go.mod":    "module example/seed\n\ngo 1.23\n",
		"seed.go":   seedProduction,
		"x_test.go": "package seed\n\ntype testOnly struct{}\n\ntype Exported struct {\n\tF testOnly\n}\n",
	})

	if slices.Contains(publicized, "testOnly") {
		t.Fatalf("a type declared only by the test half is not a production type and must not be publicized; publicized: %v", publicized)
	}
}

// The seed feeds the EXISTING fixpoint rather than replacing it: a seeded type's exported method's
// unexported result type is carried through by cascadePublicizedMethodTypes exactly as it would be
// from a production consumer.
func TestSeededTypeCascadesThroughItsExportedMethods(t *testing.T) {
	publicized := seedFixture(t, map[string]string{
		"go.mod":    "module example/seed\n\ngo 1.23\n",
		"seed.go":   seedProduction,
		"x_test.go": "package seed\n\ntype Exported struct {\n\tF alsoHidden\n}\n",
	})

	if !slices.Contains(publicized, "alsoHidden") {
		t.Fatalf("the seeded type itself must be publicized; publicized: %v", publicized)
	}

	if !slices.Contains(publicized, "carried") {
		t.Fatalf("a seeded type's EXPORTED method's unexported result must cascade; publicized: %v", publicized)
	}
}

// The scan half, asserted on its own so a failure says which half moved: the names the in-package
// test file contributes, and NOT the ones an unexported member reaches.
func TestSiblingScanContributesOnlyExportedMemberTypes(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":  "module example/seed\n\ngo 1.23\n",
		"seed.go": seedProduction,
		"x_test.go": "package seed\n\ntype Exported struct {\n\tF hidden\n\tg alsoHidden\n}\n\n" +
			"func Take(r reached) {}\n\nfunc take(c carried) {}\n",
		"ext_test.go": "package seed_test\n\ntype Other struct{}\n",
	})

	signals := collectSiblingTestSignals(dir, "seed", Options{
		targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
	})

	if want := []string{"hidden", "reached"}; !slices.Equal(signals.publicizedTypeNames, want) {
		t.Fatalf("sibling publicization seed = %v, want %v (an unexported field and an unexported func contribute nothing)", signals.publicizedTypeNames, want)
	}
}
