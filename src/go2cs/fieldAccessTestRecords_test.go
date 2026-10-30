// fieldAccessTestRecords_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// Seat #45 (class A): a mixed white-box suite's [GoFieldAccess] records follow the variant that recorded them,
// exactly as its GoImplement records do. The records were not part of the per-variant record split, so every
// test-info file a variant wrote received the whole live set, and the external suite's `rootMultiTest` (os),
// `atofTest` (strconv) and `Planet` (sort) reached package_info_internal_test.cs, whose scope is the production
// class and the bridge: CS0246 in the R0 rehearsal's os, strconv and sort test builds.

import (
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// writeFieldAccessMixedSuiteFixture writes a mixed suite in which each of the three compilations reads a field of a
// struct of its own through a pointer: production's box, the internal test's probe, the external test's sample.
func writeFieldAccessMixedSuiteFixture(t *testing.T) string {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/mixedfields\n\ngo 1.23\n",
		testOnlyFixtureDir + "/shape.go": "package " + testOnlyFixturePackage + "\n\n" +
			"type Box struct{ W int }\n\n" +
			"func Width(b *Box) int { return b.W }\n",
		testOnlyFixtureDir + "/shape_test.go": "package " + testOnlyFixturePackage + "\n\n" +
			"import \"testing\"\n\n" +
			"type probe struct{ n int }\n\n" +
			"func TestProbe(t *testing.T) {\n" +
			"\tp := &probe{n: 2}\n" +
			"\tif p.n != 2 {\n" +
			"\t\tt.Fatal(\"probe\")\n" +
			"\t}\n" +
			"}\n",
		testOnlyFixtureDir + "/shapex_test.go": "package " + testOnlyFixturePackage + "_test\n\n" +
			"import \"testing\"\n\n" +
			"type sample struct{ v int }\n\n" +
			"func TestSample(t *testing.T) {\n" +
			"\ts := &sample{v: 3}\n" +
			"\tif s.v != 3 {\n" +
			"\t\tt.Fatal(\"sample\")\n" +
			"\t}\n" +
			"}\n",
	})

	return filepath.Join(dir, testOnlyFixtureDir)
}

// fieldAccessRecordLines returns a file's [GoFieldAccess] record lines.
func fieldAccessRecordLines(body string) []string {
	var records []string

	for _, line := range strings.Split(body, "\n") {
		if isFieldAccessRecordLine(line) {
			records = append(records, strings.TrimSpace(line))
		}
	}

	return records
}

func recordsNaming(records []string, name string) []string {
	var named []string

	for _, record := range records {
		if strings.Contains(record, "(typeof("+name+"))") || strings.Contains(record, "."+name+"))") {
			named = append(named, record)
		}
	}

	return named
}

func TestFieldAccessRecordsFollowTheirTestVariant(t *testing.T) {
	isolateConverterPathGlobals(t)
	files := convertMixedSuiteFixture(t, writeFieldAccessMixedSuiteFixture(t))

	bridge := fieldAccessRecordLines(bridgeAnchorOf(t, files))
	testInfo := fieldAccessRecordLines(files[testPackageInfoFileName])

	t.Logf("%s: %v", internalTestPackageInfoFileName, bridge)
	t.Logf("%s: %v", testPackageInfoFileName, testInfo)

	// Vacuity: the internal suite's own record reaches the bridge, and the external suite's reaches the test anchor.
	if len(recordsNaming(bridge, "probe")) == 0 {
		t.Fatalf("the bridge anchor must carry the internal suite's probe record: %v", bridge)
	}
	if len(recordsNaming(testInfo, "sample")) == 0 {
		t.Fatalf("%s must carry the external suite's sample record: %v", testPackageInfoFileName, testInfo)
	}

	// The subject: the external suite's struct is out of the bridge anchor's scope.
	if named := recordsNaming(bridge, "sample"); len(named) > 0 {
		t.Errorf("the external suite's record reached %s, where `sample` does not resolve (CS0246): %v", internalTestPackageInfoFileName, named)
	}

	// One record per struct: the bridge's own struct is not repeated in the test anchor.
	if named := recordsNaming(testInfo, "probe"); len(named) > 0 {
		t.Errorf("the internal suite's bridge-declared record must anchor at the bridge alone: %v", named)
	}
}

// The recompile model's external split, the same rule as its conversion pairs: a bare record is the external test
// package's own and goes to its unit, a qualified one keeps the production anchor.
func TestExternalVariantFieldAccessRecordPartition(t *testing.T) {
	isolateConverterPathGlobals(t)
	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	fieldAccessRecords.Add("sample")
	fieldAccessRecords.Add("pair<>")
	fieldAccessRecords.Add("value_package.Box")

	testAnchored, productionAnchored := splitExternalVariantRecords("value_package", false)

	if !testAnchored.fieldAccess.Contains("sample") || !testAnchored.fieldAccess.Contains("pair<>") || testAnchored.fieldAccess.Contains("value_package.Box") {
		t.Errorf("test-anchored records = %v, want the bare sample and pair<> alone", testAnchored.fieldAccess.Keys())
	}
	if !productionAnchored.fieldAccess.Contains("value_package.Box") || len(productionAnchored.fieldAccess.Keys()) != 1 {
		t.Errorf("production-anchored records = %v, want value_package.Box alone", productionAnchored.fieldAccess.Keys())
	}

	resetPackageState(&packages.Package{})
}
