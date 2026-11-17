// testProjectCandidateReferences_integration_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// candidateReferencesModule is CLASS A's fixture (COORD rulings 20261117T145209Z and 20261117T172451Z). Five tested
// packages, each referencing what its own test file imports. The first three need an assembly no Go import of their
// tests names; keyarm is the control and valarm the probe:
//   - refarm calls (*sql.Tx).Commit, and reflib declares a pointer-receiver Commit that go2cs emits as
//     `Commit(this ref Backup)` with a database/sql/driver result. C# weighs it as a candidate for the call and loads
//     its signature (CS0012 without driver). t2b's shape, from the TS session's repro (claude/coord-t2b-repro
//     4bdd28cb9e), and modernc.org/mathutil's `Next(this ref FCBig)` as it reaches the sqlite root test.
//   - boxarm: the same Commit on a receiver that escapes, emitted `Commit(this ж<Holder>)`. Under a ж receiver
//     (tx is a ж<sql.Tx>) C# loads that candidate's signature too: measured CS0012 without driver (cgo-e2e linux,
//     runs 44134561916 and 44134568782). It was the control until then; the pointer-receiver superset is what
//     builds it.
//   - keyarm is the control, registry's shape: k.Commit() on keylib.Key, a VALUE type with its own value-receiver
//     Commit, and boxlib's `Commit(this ж<Holder>)` the other candidate in scope, as os.File.Stat is for registry's
//     k.Stat(). Registry's banked windows suite builds without io/fs, so keyarm should build without driver.
//   - valarm is the probe: vallib declares Commit on a VALUE receiver (emitted `this Plain`), called on the same ж
//     receiver as boxarm; the candidate's receiver kind is the one axis changed. go2cs-gen gives every converted
//     value method a `this ж<Plain>` twin (RecvGenerator), so a red cannot say which of the two C# bound, only that
//     a value-receiver method needs its signature's assemblies.
//   - logtarm passes a *testing.T as check.TestingT, which go2cs emits as `TestingT : logt.LogT`, a C# base Go's
//     interface does not have; its GoImplement record then needs internal/logt (gotest.tools/v3's assert/LogT).
var candidateReferencesModule = map[string]string{
	"boxarm/boxarm.go":        "package boxarm\n\nfunc Answer() int { return 42 }\n",
	"boxarm/boxarm_test.go":   "package boxarm\n\nimport (\n\t\"database/sql\"\n\t\"testing\"\n\n\t\"example.test/refsext/boxlib\"\n)\n\nfunc commitIfOpen(tx *sql.Tx) error {\n\tif tx == nil {\n\t\treturn nil\n\t}\n\treturn tx.Commit()\n}\n\nfunc TestCommitCompiles(t *testing.T) {\n\tif err := commitIfOpen(nil); err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif boxlib.Version() == 0 || Answer() != 42 {\n\t\tt.Fatal(\"unexpected\")\n\t}\n}\n",
	"boxlib/boxlib.go":        "// Package boxlib declares the same Commit on a receiver that escapes, so go2cs emits it as a `this ж<Holder>`\n// extension: the control.\npackage boxlib\n\nimport \"database/sql/driver\"\n\ntype Holder struct{ self *Holder }\n\nvar last *Holder\n\nfunc (h *Holder) Commit() (driver.Conn, error) {\n\th.self = h\n\tlast = h\n\treturn nil, nil\n}\n\nfunc Version() int { return 2 }\n",
	"check/check.go":          "// Package check is shaped like gotest.tools/v3/assert: TestingT has LogT's method, and check imports logt.\npackage check\n\nimport \"example.test/refsext/internal/logt\"\n\ntype TestingT interface {\n\tFailNow()\n\tFail()\n\tLog(args ...any)\n}\n\nfunc Equal(t TestingT, got, want int) {\n\tif got != want {\n\t\tlogt.Note(t, \"not equal\")\n\t\tt.FailNow()\n\t}\n}\n",
	"go.mod":                  "module example.test/refsext\n\ngo 1.23\n",
	"internal/logt/logt.go":   "// Package logt declares the one-method interface that check's TestingT has the method of.\npackage logt\n\ntype LogT interface {\n\tLog(args ...any)\n}\n\nfunc Note(t LogT, msg string) { t.Log(msg) }\n",
	"keyarm/keyarm.go":        "package keyarm\n\nfunc Answer() int { return 42 }\n",
	"keyarm/keyarm_test.go":   "package keyarm\n\nimport (\n\t\"testing\"\n\n\t\"example.test/refsext/boxlib\"\n\t\"example.test/refsext/keylib\"\n)\n\nfunc commitKey(k keylib.Key) error {\n\treturn k.Commit()\n}\n\nfunc TestCommitCompiles(t *testing.T) {\n\tif err := commitKey(keylib.Key(0)); err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif boxlib.Version() == 0 || Answer() != 42 {\n\t\tt.Fatal(\"unexpected\")\n\t}\n}\n",
	"keylib/keylib.go":        "// Package keylib declares a value type with a value-receiver Commit, registry's Key shape: k.Commit() has a value\n// receiver, so boxlib's `this ж<Holder>` Commit is a candidate only by name.\npackage keylib\n\ntype Key uintptr\n\nfunc (k Key) Commit() error { return nil }\n",
	"logtarm/logtarm.go":      "package logtarm\n\nfunc Answer() int { return 42 }\n",
	"logtarm/logtarm_test.go": "package logtarm\n\nimport (\n\t\"testing\"\n\n\t\"example.test/refsext/check\"\n)\n\nfunc TestEqual(t *testing.T) {\n\tcheck.Equal(t, Answer(), 42)\n}\n",
	"refarm/refarm.go":        "package refarm\n\nfunc Answer() int { return 42 }\n",
	"refarm/refarm_test.go":   "package refarm\n\nimport (\n\t\"database/sql\"\n\t\"testing\"\n\n\t\"example.test/refsext/reflib\"\n)\n\nfunc commitIfOpen(tx *sql.Tx) error {\n\tif tx == nil {\n\t\treturn nil\n\t}\n\treturn tx.Commit()\n}\n\nfunc TestCommitCompiles(t *testing.T) {\n\tif err := commitIfOpen(nil); err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif reflib.Version() == 0 || Answer() != 42 {\n\t\tt.Fatal(\"unexpected\")\n\t}\n}\n",
	"reflib/reflib.go":        "// Package reflib declares a pointer-receiver method named Commit whose result names a database/sql/driver type. Its\n// receiver never escapes, so go2cs emits it as a `this ref Backup` extension.\npackage reflib\n\nimport \"database/sql/driver\"\n\ntype Backup struct{}\n\nfunc (b *Backup) Commit() (driver.Conn, error) { return nil, nil }\n\nfunc Version() int { return 1 }\n",
	"valarm/valarm.go":        "package valarm\n\nfunc Answer() int { return 42 }\n",
	"valarm/valarm_test.go":   "package valarm\n\nimport (\n\t\"database/sql\"\n\t\"testing\"\n\n\t\"example.test/refsext/vallib\"\n)\n\nfunc commitIfOpen(tx *sql.Tx) error {\n\tif tx == nil {\n\t\treturn nil\n\t}\n\treturn tx.Commit()\n}\n\nfunc TestCommitCompiles(t *testing.T) {\n\tif err := commitIfOpen(nil); err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif vallib.Version() == 0 || Answer() != 42 {\n\t\tt.Fatal(\"unexpected\")\n\t}\n}\n",
	"vallib/vallib.go":        "// Package vallib declares boxlib's Commit on a VALUE receiver, so go2cs emits it as `Commit(this Plain)`: the probe.\npackage vallib\n\nimport \"database/sql/driver\"\n\ntype Plain struct{ n int }\n\nfunc (p Plain) Commit() (driver.Conn, error) { return nil, nil }\n\nfunc Version() int { return 3 }\n",
}

// TestTestsProjectReferencesWhatItsCandidatesAndRecordsName is the reference half of CLASS A, which needs Go only: each
// arm's test project references the assembly its build needs. The build half is
// TestTestsProjectsBuildWithTheirCandidateReferences.
func TestTestsProjectReferencesWhatItsCandidatesAndRecordsName(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: builds the converter and converts a module with its tests")
	}

	root := t.TempDir()
	moduleDir := filepath.Join(root, "refsext")
	writeModuleFiles(t, moduleDir, candidateReferencesModule)
	outRoot := filepath.Join(root, "out")

	output, err := runTestsRecurse(t, moduleDir, outRoot)

	if err != nil {
		t.Fatalf("go2cs -tests -recurse failed: %v\n%s", err, output)
	}

	srcRoot := filepath.Join(outRoot, "src", "example.test", "refsext")
	projectOf := func(pkg string) string {
		projects, _ := filepath.Glob(filepath.Join(srcRoot, pkg, "*.tests.csproj"))

		if len(projects) != 1 {
			t.Fatalf("%s: want one test project, got %v\n%s", pkg, projects, output)
		}

		data, readErr := os.ReadFile(projects[0])

		if readErr != nil {
			t.Fatal(readErr)
		}

		return string(data)
	}

	const driverReference = `Include="$(go2csPath)core/database/sql/driver/database.sql.driver.csproj"`

	for _, arm := range []struct{ pkg, want, why string }{
		{"refarm", driverReference, "reflib's `Commit(this ref Backup)` is a candidate for tx.Commit() and names driver.Conn"},
		{"boxarm", driverReference, "boxlib's `Commit(this ж<Holder>)` is a candidate under tx's ж receiver: the ruled superset"},
		{"keyarm", driverReference, "boxlib's Commit is a pointer-receiver candidate by name: the superset references more than the control needs"},
		{"logtarm", `Include="../internal/logt/example.test.refsext.internal.logt.csproj"`, "the GoImplement record binds check.TestingT, emitted with the base logt.LogT"},
	} {
		if project := projectOf(arm.pkg); !strings.Contains(project, arm.want) {
			t.Errorf("%s: the test project does not reference %s (%s)", arm.pkg, arm.want, arm.why)
		}
	}
}

// TestTestsProjectsBuildWithTheirCandidateReferences is the build half of CLASS A, testing the ruled rule from both
// sides (COORD rulings 20261117T145209Z and 20261117T172451Z): refarm, boxarm (pointer-receiver candidates under a ж
// receiver) and logtarm (a record's emitted base) build only with the reference the converter now writes; keyarm, the
// control, builds with database/sql/driver REMOVED from its project, as registry builds without io/fs; and valarm,
// the probe, is built without driver to measure whether a value-receiver candidate needs it. A valarm red widens the
// rule to every same-named method. Opt-in on GO2CS_TESTS_E2E, as TestTestsRecurseRunsWhatItCanPastARefusedPackage is:
// each build takes the converted standard library closure.
func TestTestsProjectsBuildWithTheirCandidateReferences(t *testing.T) {
	if os.Getenv("GO2CS_TESTS_E2E") != "1" {
		t.Skip("opt-in (GO2CS_TESTS_E2E=1): builds converted test projects against the converted standard library")
	}

	dotnet, err := exec.LookPath("dotnet")

	if err != nil {
		t.Fatalf("GO2CS_TESTS_E2E=1 needs the .NET SDK on PATH: %v", err)
	}

	root := cgoBuildDir(t, t.TempDir())
	moduleDir := filepath.Join(root, "refsext")
	writeModuleFiles(t, moduleDir, candidateReferencesModule)
	outRoot := filepath.Join(root, "out")

	if output, err := runTestsRecurse(t, moduleDir, outRoot); err != nil {
		t.Fatalf("go2cs -tests -recurse failed: %v\n%s", err, output)
	}

	srcRoot := filepath.Join(outRoot, "src", "example.test", "refsext")
	projectOf := func(pkg string) string {
		projects, _ := filepath.Glob(filepath.Join(srcRoot, pkg, "*.tests.csproj"))

		if len(projects) != 1 {
			t.Fatalf("%s: want one test project, got %v", pkg, projects)
		}

		return projects[0]
	}

	// The control and the probe: each project without database/sql/driver, whatever the converter wrote.
	for _, pkg := range []string{"keyarm", "valarm"} {
		project := projectOf(pkg)
		data, err := os.ReadFile(project)

		if err != nil {
			t.Fatal(err)
		}

		var kept []string

		for _, line := range strings.SplitAfter(string(data), "\n") {
			if !strings.Contains(line, "core/database/sql/driver/database.sql.driver.csproj") {
				kept = append(kept, line)
			}
		}

		if err := os.WriteFile(project, []byte(strings.Join(kept, "")), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	for _, arm := range []struct{ pkg, why string }{
		{"keyarm", "the CONTROL, built WITHOUT database/sql/driver: under a value receiver boxlib's `this ж<Holder>` candidate must not need it, as registry builds without io/fs"},
		{"valarm", "the PROBE, built WITHOUT database/sql/driver: a red means a value-receiver candidate (`this Plain`, or its generated ж twin) needs it, and the rule widens to every same-named method"},
		{"boxarm", "boxlib's `Commit(this ж<Holder>)` candidate under tx's ж receiver names driver.Conn"},
		{"refarm", "reflib's `Commit(this ref Backup)` candidate names driver.Conn"},
		{"logtarm", "check.TestingT's emitted base logt.LogT"},
	} {
		build := exec.Command(dotnet, cgoBuildArgs(t, projectOf(arm.pkg), runtime.GOOS, "")...)
		build.Env = cgoBuildEnvironment()
		output, err := build.CombinedOutput()

		if err != nil {
			t.Errorf("%s: the test project does not build (%s): %v\n%s", arm.pkg, arm.why, err, strings.Join(cgoCompilerErrors(string(output)), "\n"))
			continue
		}

		t.Logf("READING %s: builds (%s)", arm.pkg, arm.why)
	}
}
