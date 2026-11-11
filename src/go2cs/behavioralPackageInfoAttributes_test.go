// behavioralPackageInfoAttributes_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
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
// TRAIN B rehearsal (ledger 12833683e1). An IDENTICAL attribute line never has a reason to repeat ON
// ONE DECLARATION, whatever its arguments. A cgo package's file declares two classes, the package's and
// its C pseudo-package's, and each carries its own [GoPackage] stamp, so a standalone attribute line is
// keyed by the declaration it applies to, the next line that is not one, as CS0579 is. An [assembly:] or
// [module:] line applies to the whole compilation, and is keyed alone.
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

		for _, repeat := range repeatedAttributeLines(string(contents)) {
			t.Errorf("%s:%s", file, repeat)
		}
	}
}

// repeatedAttributeLines names each attribute line that repeats one already seen on the same declaration.
func repeatedAttributeLines(contents string) []string {
	lines := strings.Split(strings.ReplaceAll(contents, "\r\n", "\n"), "\n")
	seen := map[string]int{}
	var repeats []string

	for i, line := range lines {
		trimmed := strings.TrimSpace(line)

		if !strings.HasPrefix(trimmed, "[") {
			continue
		}

		key := trimmed

		// A line that is only attributes applies to the declaration below it.
		if strings.HasSuffix(trimmed, "]") && !strings.HasPrefix(trimmed, "[assembly:") && !strings.HasPrefix(trimmed, "[module:") {
			for j := i + 1; j < len(lines); j++ {
				if next := strings.TrimSpace(lines[j]); next != "" && !strings.HasPrefix(next, "[") {
					key = trimmed + " on " + next
					break
				}
			}
		}

		if first, repeated := seen[key]; repeated {
			repeats = append(repeats, fmt.Sprintf("%d repeats the attribute line %q first seen at line %d (CS0579 when the attribute is not AllowMultiple)", i+1, trimmed, first))
			continue
		}

		seen[key] = i + 1
	}

	return repeats
}

// The guard's predicate, planted: a repeat on one declaration fires, and one stamp on each of two classes does not.
func TestRepeatedAttributeLinesKeysEachLineByItsDeclaration(t *testing.T) {
	for _, plant := range []struct {
		name, text string
		repeats    int
	}{
		{"one class stamped twice", "[GoPackage(\"main\")]\n[GoTestMatchingConsoleOutput]\n[GoTestMatchingConsoleOutput]\npublic static partial class main_package\n{\n}\n", 1},
		{"a record repeated", "partial class main_package\n{\n    [GoLocalName(\"x\")] internal partial struct x {}\n    [GoLocalName(\"x\")] internal partial struct x {}\n}\n", 1},
		{"an assembly attribute repeated", "[assembly: GoFieldAccess(typeof(T))]\n[assembly: GoFieldAccess(typeof(T))]\nnamespace go;\n", 1},
		{"two classes, one stamp each", "[GoPackage(\"main\")]\npublic static partial class main_package\n{\n}\n\n[GoPackage(\"main\")]\npartial class mainꓸC_package\n{\n}\n", 0},
	} {
		if got := repeatedAttributeLines(plant.text); len(got) != plant.repeats {
			t.Errorf("%s: %d repeats %v, want %d", plant.name, len(got), got, plant.repeats)
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
