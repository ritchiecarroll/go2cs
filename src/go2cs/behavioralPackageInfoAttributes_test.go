// behavioralPackageInfoAttributes_test.go - Gbtc
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
	"regexp"
	"sort"
	"strings"
	"testing"
)

// TestBehavioralPackageInfoCarriesNoDuplicateAttribute guards the class of a committed behavioral
// package_info.cs that carries one attribute line twice. [GoTestMatchingConsoleOutput] is not
// AllowMultiple, so a repeat is CS0579 in go2cs.slnx, which nothing routinely builds. The converter
// merges an existing package_info.cs, so re-emitting a project KEEPS a hand-added attribute, and adding
// it again by hand duplicates it: LocalStringConstHoist at claude/c2-arm-c 2d068bca59, found by R's
// TRAIN B rehearsal (ledger 12833683e1). An IDENTICAL attribute line never has a reason to repeat,
// whatever its arguments.
func TestBehavioralPackageInfoCarriesNoDuplicateAttribute(t *testing.T) {
	files, err := filepath.Glob(filepath.Join("..", "tests", "Behavioral", "*", "package_info.cs"))

	if err != nil {
		t.Fatal(err)
	}

	if len(files) == 0 {
		t.Fatal("no behavioral package_info.cs found: the guard would pass vacuously")
	}

	sort.Strings(files)

	for _, file := range files {
		contents, err := os.ReadFile(file)

		if err != nil {
			t.Fatal(err)
		}

		seen := map[string]int{}

		for i, line := range strings.Split(strings.ReplaceAll(string(contents), "\r\n", "\n"), "\n") {
			trimmed := strings.TrimSpace(line)

			if !strings.HasPrefix(trimmed, "[") {
				continue
			}

			if first, repeated := seen[trimmed]; repeated {
				t.Errorf("%s:%d repeats the attribute line %q first seen at line %d (CS0579 when the attribute is not AllowMultiple)", file, i+1, trimmed, first)
				continue
			}

			seen[trimmed] = i + 1
		}
	}
}

// TestOutputComparisonListMatchesConsoleOutputAttribute keeps OutputComparisonTests.cs's project list
// and the [GoTestMatchingConsoleOutput] attribute in step, in BOTH directions. The list is what runs:
// OutputComparisonTests.CheckTarget compares Go's output with the C# output without consulting the
// attribute. But UpdateTestTargets REBUILDS the list from the attribute, so a listed project without
// it is dropped by any re-baseline, even one scoped to an unrelated project with --only (AppendOfMake
// and ValuePunBits were listed by hand in 0fcdba94d5 and 9ac6051e46 without the attribute; G's
// re-baseline carried their deletion). An attribute without a list entry is a comparison nobody runs.
// The attribute is matched exactly as the harness matches it: a trimmed line equal to the attribute.
func TestOutputComparisonListMatchesConsoleOutputAttribute(t *testing.T) {
	behavioral := filepath.Join("..", "tests", "Behavioral")
	listFile := filepath.Join(behavioral, "BehavioralTests", "OutputComparisonTests.cs")

	listSource, err := os.ReadFile(listFile)

	if err != nil {
		t.Fatal(err)
	}

	listed := map[string]bool{}

	for _, match := range regexp.MustCompile(`CheckTarget\("([^"]+)"\)`).FindAllStringSubmatch(string(listSource), -1) {
		listed[match[1]] = true
	}

	files, err := filepath.Glob(filepath.Join(behavioral, "*", "package_info.cs"))

	if err != nil {
		t.Fatal(err)
	}

	marked := map[string]bool{}

	for _, file := range files {
		contents, err := os.ReadFile(file)

		if err != nil {
			t.Fatal(err)
		}

		for _, line := range strings.Split(strings.ReplaceAll(string(contents), "\r\n", "\n"), "\n") {
			if strings.TrimSpace(line) == "[GoTestMatchingConsoleOutput]" {
				marked[filepath.Base(filepath.Dir(file))] = true
				break
			}
		}
	}

	if len(listed) == 0 || len(marked) == 0 {
		t.Fatalf("listed %d, marked %d: the guard would pass vacuously", len(listed), len(marked))
	}

	var mismatches []string

	for project := range listed {
		if !marked[project] {
			mismatches = append(mismatches, project+": listed in OutputComparisonTests.cs, but its package_info.cs has no [GoTestMatchingConsoleOutput] (UpdateTestTargets would drop it)")
		}
	}

	for project := range marked {
		if !listed[project] {
			mismatches = append(mismatches, project+": carries [GoTestMatchingConsoleOutput], but OutputComparisonTests.cs does not list it (its output is never compared)")
		}
	}

	sort.Strings(mismatches)

	for _, mismatch := range mismatches {
		t.Error(mismatch)
	}
}
