// liftedIfaceTestCast_integration_test.go - Gbtc
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

// go-cmp's cmp.Reporter takes an ANONYMOUS interface parameter, which production lifts to a named
// interface (Reporter_r), and cmp's internal test passes a `*defaultReporter` to it. Production never
// makes that cast, so the test conversion must record the pointer pair itself and name the adapter
// its own metadata generates, exactly as it does for a NAMED interface. The cast site resolved the
// lifted name from this file's lifts and the package registry only, never from the PRODUCTION
// conversion's published lifts, so the name stayed a deferred marker: the record was dropped and the
// adapter was named on the production class, which never generates it (CS0426
// `defaultReporterжReporter_r` x4 in cmp's options_test.cs).
func TestTestsCastToProductionLiftedInterfaceRecordsTheTestPair(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: builds the converter and converts a module with its tests")
	}

	root := t.TempDir()
	moduleDir := filepath.Join(root, "irep")

	writeModuleFile(t, filepath.Join(moduleDir, "go.mod"), "module example.test/irep\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(moduleDir, "rep.go"), "package irep\n\n"+
		"type Step int\n\n"+
		"func Reporter(r interface {\n\tPush(Step)\n\tPop()\n}) int {\n\tr.Push(2)\n\tr.Pop()\n\treturn 1\n}\n\n"+
		"type defaultReporter struct{ n int }\n\n"+
		"func (r *defaultReporter) Push(s Step) { r.n += int(s) }\n"+
		"func (r *defaultReporter) Pop()        { r.n-- }\n")
	writeModuleFile(t, filepath.Join(moduleDir, "rep_test.go"), "package irep\n\n"+
		"import \"testing\"\n\n"+
		"func TestReporter(t *testing.T) {\n\td := &defaultReporter{}\n\tif Reporter(d) != 1 || d.n != 1 {\n\t\tt.Fatalf(\"n = %d\", d.n)\n\t}\n}\n")

	outRoot := filepath.Join(root, "out")

	if output, err := runTestsRecurse(t, moduleDir, outRoot); err != nil {
		t.Fatalf("go2cs -tests -recurse failed: %v\n%s", err, output)
	}

	packageDir := filepath.Join(outRoot, "src", "example.test", "irep")

	read := func(name string) string {
		data, err := os.ReadFile(filepath.Join(packageDir, name))

		if err != nil {
			t.Fatal(err)
		}

		return string(data)
	}

	if testFile := read("rep_test.cs"); !strings.Contains(testFile, "new irep_internal_test_package.irep_defaultReporterжReporter_r(d)") {
		t.Errorf("the cast must name the adapter the test metadata generates:\n%s", testFile)
	}

	if info := read("package_test_info.cs"); !strings.Contains(info, "defaultReporter, Reporter_r>(Pointer = true)]") {
		t.Errorf("the test conversion must record the *defaultReporter -> Reporter_r pair:\n%s", info)
	}
}
