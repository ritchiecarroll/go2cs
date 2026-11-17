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
	"path/filepath"
	"strings"
	"testing"
)

// candidateReferencesModule is CLASS A's fixture (COORD ruling 20261117T145209Z). Three tested packages, each
// referencing what its own test file imports, and each needing an assembly no Go import of its tests names:
//   - refarm calls (*sql.Tx).Commit, and reflib declares a pointer-receiver Commit that go2cs emits as
//     `Commit(this ref Backup)` with a database/sql/driver result. C# weighs it as a candidate for the call and loads
//     its signature (CS0012 without driver). t2b's shape, from the TS session's repro (claude/coord-t2b-repro
//     4bdd28cb9e), and modernc.org/mathutil's `Next(this ref FCBig)` as it reaches the sqlite root test.
//   - boxarm is the control: the same Commit on a receiver that escapes, emitted `Commit(this ж<Holder>)`. A
//     `this ж<T>` candidate is measured not to need its signature's assemblies (registry's os.File.Stat); the
//     pointer-receiver rule references driver for it all the same, as the ruled superset.
//   - logtarm passes a *testing.T as check.TestingT, which go2cs emits as `TestingT : logt.LogT`, a C# base Go's
//     interface does not have; its GoImplement record then needs internal/logt (gotest.tools/v3's assert/LogT).
var candidateReferencesModule = map[string]string{
	"boxarm/boxarm.go":        "package boxarm\n\nfunc Answer() int { return 42 }\n",
	"boxarm/boxarm_test.go":   "package boxarm\n\nimport (\n\t\"database/sql\"\n\t\"testing\"\n\n\t\"example.test/refsext/boxlib\"\n)\n\nfunc commitIfOpen(tx *sql.Tx) error {\n\tif tx == nil {\n\t\treturn nil\n\t}\n\treturn tx.Commit()\n}\n\nfunc TestCommitCompiles(t *testing.T) {\n\tif err := commitIfOpen(nil); err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif boxlib.Version() == 0 || Answer() != 42 {\n\t\tt.Fatal(\"unexpected\")\n\t}\n}\n",
	"boxlib/boxlib.go":        "// Package boxlib declares the same Commit on a receiver that escapes, so go2cs emits it as a `this ж<Holder>`\n// extension: the control.\npackage boxlib\n\nimport \"database/sql/driver\"\n\ntype Holder struct{ self *Holder }\n\nvar last *Holder\n\nfunc (h *Holder) Commit() (driver.Conn, error) {\n\th.self = h\n\tlast = h\n\treturn nil, nil\n}\n\nfunc Version() int { return 2 }\n",
	"check/check.go":          "// Package check is shaped like gotest.tools/v3/assert: TestingT has LogT's method, and check imports logt.\npackage check\n\nimport \"example.test/refsext/internal/logt\"\n\ntype TestingT interface {\n\tFailNow()\n\tFail()\n\tLog(args ...any)\n}\n\nfunc Equal(t TestingT, got, want int) {\n\tif got != want {\n\t\tlogt.Note(t, \"not equal\")\n\t\tt.FailNow()\n\t}\n}\n",
	"go.mod":                  "module example.test/refsext\n\ngo 1.23\n",
	"internal/logt/logt.go":   "// Package logt declares the one-method interface that check's TestingT has the method of.\npackage logt\n\ntype LogT interface {\n\tLog(args ...any)\n}\n\nfunc Note(t LogT, msg string) { t.Log(msg) }\n",
	"logtarm/logtarm.go":      "package logtarm\n\nfunc Answer() int { return 42 }\n",
	"logtarm/logtarm_test.go": "package logtarm\n\nimport (\n\t\"testing\"\n\n\t\"example.test/refsext/check\"\n)\n\nfunc TestEqual(t *testing.T) {\n\tcheck.Equal(t, Answer(), 42)\n}\n",
	"refarm/refarm.go":        "package refarm\n\nfunc Answer() int { return 42 }\n",
	"refarm/refarm_test.go":   "package refarm\n\nimport (\n\t\"database/sql\"\n\t\"testing\"\n\n\t\"example.test/refsext/reflib\"\n)\n\nfunc commitIfOpen(tx *sql.Tx) error {\n\tif tx == nil {\n\t\treturn nil\n\t}\n\treturn tx.Commit()\n}\n\nfunc TestCommitCompiles(t *testing.T) {\n\tif err := commitIfOpen(nil); err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif reflib.Version() == 0 || Answer() != 42 {\n\t\tt.Fatal(\"unexpected\")\n\t}\n}\n",
	"reflib/reflib.go":        "// Package reflib declares a pointer-receiver method named Commit whose result names a database/sql/driver type. Its\n// receiver never escapes, so go2cs emits it as a `this ref Backup` extension.\npackage reflib\n\nimport \"database/sql/driver\"\n\ntype Backup struct{}\n\nfunc (b *Backup) Commit() (driver.Conn, error) { return nil, nil }\n\nfunc Version() int { return 1 }\n",
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
		{"boxarm", driverReference, "boxlib's Commit is a pointer-receiver candidate too: the ruled superset"},
		{"logtarm", `Include="../internal/logt/example.test.refsext.internal.logt.csproj"`, "the GoImplement record binds check.TestingT, emitted with the base logt.LogT"},
	} {
		if project := projectOf(arm.pkg); !strings.Contains(project, arm.want) {
			t.Errorf("%s: the test project does not reference %s (%s)", arm.pkg, arm.want, arm.why)
		}
	}
}
