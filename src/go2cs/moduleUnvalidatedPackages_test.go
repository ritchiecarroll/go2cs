// moduleUnvalidatedPackages_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the record half of COORD's ruling of 2026-10-13 on the go-cmp dry run (gap 1). A -tests -recurse
// run whose tested package did not validate wrote MODULE.md from the pages it HAD: go-cmp v0.7.0 at the
// default -test-timeout lost its root package `cmp` to the package deadline, and MODULE.md read
// "409 matched across 4 package(s)" with no line about cmp at all. MODULE.md is what a module pack ships
// as VALIDATION.md, so it now names every tested package that did not validate, and a page left on disk
// by an earlier run of that package is not summed as if this run had validated it. The pack's binding
// refuses such a package (src/tools/nugetgo/Test-NugetgoValidationBinding.ps1).
//
// The same ruling's item 4: the console summary printed an aggregate roll-up's class as an empty entry,
// "(, runtime-capability)", where the proof page names it `aggregate`.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestModuleSummaryNamesTheTestedPackagesThatDidNotValidate(t *testing.T) {
	temp := t.TempDir()
	root, _ := fakeCheckout(t, filepath.Join(temp, "repo"))
	outRoot := filepath.Join(root, "modout")

	emit := func(importPath string) {
		t.Helper()

		comparison := testComparison{
			Package: importPath, Status: "validated", Matched: true,
			Go: map[string]string{"TestA": "pass"}, CSharp: map[string]string{"TestA": "pass"},
		}
		manifest := testManifest{PackageImportPath: importPath, ModulePath: "example.com/mod", GoVersion: "go1.24.13"}
		output := filepath.Join(append([]string{outRoot, "src"}, strings.Split(importPath, "/")...)...)

		if err := emitValidationProofPage(output, comparison, manifest, nil, nil, Options{targetPlatform: "linux/amd64"}); err != nil {
			t.Fatalf("emitValidationProofPage(%s): %v", importPath, err)
		}
	}

	// The root package validated in an EARLIER run into this root, so its page is on disk; this run's
	// comparison of it failed. The sub package validated in this run.
	emit("example.com/mod")
	emit("example.com/mod/sub")

	reason := `test action "all": Go/C# test comparison failed: TestDiff/Transformer/CyclicString: Go="pass" C#="fail (in flight at the package deadline)"` +
		"\n{\"package\":\"example.com/mod\",\"action\":\"run\"}"

	if err := recordUnvalidatedModulePackages(outRoot, "example.com/mod", map[string]string{"example.com/mod": reason}); err != nil {
		t.Fatalf("recordUnvalidatedModulePackages: %v", err)
	}

	data, err := os.ReadFile(filepath.Join(outRoot, "validation", "example.com", "mod", "MODULE.md"))
	if err != nil {
		t.Fatal(err)
	}

	module := strings.ReplaceAll(string(data), "\r", "")

	for _, want := range []string{
		"| `example.com/mod/sub` | 1 | 0 |",
		"**Total: 1 matched · 0 disclosed** across 1 package(s).",
		"## Packages that did not validate",
		"- `example.com/mod`: test action \"all\": Go/C# test comparison failed: TestDiff/Transformer/CyclicString: " +
			"Go=\"pass\" C#=\"fail (in flight at the package deadline)\"\n",
	} {
		if !strings.Contains(module, want) {
			t.Errorf("MODULE.md: want %q in:\n%s", want, module)
		}
	}

	if strings.Contains(module, "| `example.com/mod` |") {
		t.Errorf("MODULE.md sums a page this run did not validate (an earlier run's page of a package that failed now):\n%s", module)
	}

	if strings.Contains(module, `{"package"`) {
		t.Errorf("MODULE.md carries more than the reason's first line:\n%s", module)
	}

	// A later run in which every package validates writes no such section.
	emit("example.com/mod")

	data, err = os.ReadFile(filepath.Join(outRoot, "validation", "example.com", "mod", "MODULE.md"))
	if err != nil {
		t.Fatal(err)
	}

	if module := string(data); strings.Contains(module, "did not validate") || !strings.Contains(module, "| `example.com/mod` |") {
		t.Errorf("a run in which every package validates must list each one and no failure:\n%s", module)
	}
}

func TestAnUnvalidatedPackagesLongReasonIsClipped(t *testing.T) {
	got := unvalidatedReasonLine(strings.Repeat("x", 500) + "\nsecond line")

	if strings.Contains(got, "second line") || len([]rune(got)) > unvalidatedReasonLimit+1 || !strings.HasSuffix(got, "…") {
		t.Errorf("want the first line clipped to %d runes and an ellipsis, got %d runes: %q", unvalidatedReasonLimit, len([]rune(got)), got)
	}

	if got := unvalidatedReasonLine("  short reason  \n"); got != "short reason" {
		t.Errorf("a short reason is kept whole and trimmed, got %q", got)
	}
}

func TestOnlyAComparingUnfilteredRunRecordsUnvalidatedPackages(t *testing.T) {
	for _, c := range []struct {
		action, filter string
		want           bool
	}{
		{"all", "", true},
		{"compare", "", true},
		{"convert", "", false},
		{"build", "", false},
		{"run", "", false},
		{"all", "^TestA$", false},
	} {
		if got := recordsUnvalidatedPackages(Options{testAction: c.action, testFilter: c.filter}); got != c.want {
			t.Errorf("action %q filter %q: recordsUnvalidatedPackages = %v, want %v", c.action, c.filter, got, c.want)
		}
	}
}

func TestTheConsoleSummaryNamesTheAggregateRollUp(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestDiff/Comparer/MapKeyPointer": {Name: "TestDiff/Comparer/MapKeyPointer", Class: "runtime-capability"},
		"TestDiff/Reporter/LargeMapKey":   {Name: "TestDiff/Reporter/LargeMapKey", Class: "runtime-capability"},
	}

	got := strings.Join(disclosedClassList([]string{"TestDiff", "TestDiff/Comparer/MapKeyPointer", "TestDiff/Reporter/LargeMapKey"}, disclosures), ", ")

	if got != "aggregate, runtime-capability" {
		t.Errorf("classes: got %q, want %q (the roll-up's class is `aggregate`, as the proof page names it)", got, "aggregate, runtime-capability")
	}
}
