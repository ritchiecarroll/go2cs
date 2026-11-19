// forwardAliasChainTestRecord_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards sqlite-cs W7's forward alias chain under -tests (P3's review, arm c): an exported alias declared in an
// INTERNAL _test.go file ABOVE the anonymous struct its chain reaches. The alias's captured record value carries the
// test variant's qualifier in front of a deferred marker; its GoTypeAlias record must name the lift the converted
// test file declares, with no marker left and nothing reported.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestForwardAliasChainInAnInternalTestFileRecordsItsLift(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	dir := t.TempDir()
	files := map[string]string{
		"go.mod": "module example/fwd\n\ngo 1.23\n",
		"lib.go": "package fwd\n\nfunc Seven() int32 { return 7 }\n",
		"lib_test.go": "package fwd\n\nimport \"testing\"\n\n" +
			"type Cursor = cursor\n\n" +
			"type Tcursor = struct {\n\tFpVtab uintptr\n\tFnRef  int32\n}\n\n" +
			"type cursor = Tcursor\n\n" +
			"func TestForward(t *testing.T) {\n" +
			"\tvar c Cursor = Cursor{FnRef: Seven()}\n" +
			"\tif c.FnRef != 7 {\n\t\tt.Fatal(\"want 7\")\n\t}\n}\n",
	}

	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}

	internal, _ := loadBothTestVariantsForDir(t, dir)

	if internal == nil {
		t.Fatal("the internal test variant was not loaded")
	}

	outputPath := t.TempDir()
	options := Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}

	options.testClassNameOverride = getSanitizedImport("fwd_internal_test" + PackageSuffix)
	options.testWhiteboxReference = true
	options.testInlineTypeAccess = true
	options.testProductionPath = "example/fwd"

	takeUnresolvedDynamicTypes()
	t.Cleanup(func() { takeUnresolvedDynamicTypes() })

	if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", productionSeed{}, options); err != nil {
		t.Fatal(err)
	}

	captured, ok := exportedTypeAliases["Cursor"]

	if !ok {
		t.Fatalf("no exported alias record was captured for Cursor; captured: %v", exportedTypeAliases)
	}

	record := exportedTypeAliasRecord("Cursor", captured, "package_test_info.cs")

	if strings.Contains(record, dynamicTypeMarkerPrefix) {
		t.Fatalf("the record kept its marker: %s (captured %q)", record, captured)
	}

	if sites := takeUnresolvedDynamicTypes(); len(sites) != 0 {
		t.Fatalf("a resolvable chain reported unresolved site(s): %+v", sites)
	}

	data, err := os.ReadFile(filepath.Join(outputPath, "lib_test.cs"))

	if err != nil {
		t.Fatal(err)
	}

	// The record's last segment is the lift the converted file declares.
	value := strings.TrimSuffix(record[strings.LastIndex(record, ", \"")+3:], "\")]")
	lift := value[strings.LastIndex(value, ".")+1:]

	if lift == "" || !strings.Contains(string(data), "struct "+lift) {
		t.Fatalf("record %s names %q, which lib_test.cs does not declare:\n%s", record, lift, data)
	}

	t.Logf("captured %q -> %s", captured, record)
}
