// testInfoPositionMapCopies_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"path"
	"path/filepath"
	"sort"
	"strings"
	"testing"
)

// A TEST INFO'S COPY OF A PRODUCTION POSITION MAP EQUALS THE PRODUCTION RECORD (COORD, 2026-11-06).
//
// A package's test info file (package_test_info.cs, package_info_internal_test.cs) can carry a GoPositionMap
// record for a PRODUCTION file of the package, a copy of the record in that package's package_info.cs: the test
// assembly compiles the production file too. -stdlib re-encodes package_info.cs and never the test infos, so a
// change that moves a production file's lines leaves the copy describing the file as it was. Guard A
// (lineKeyedRecords_test.go) checks each record against the file it names, and caught such copies (net/netip,
// text/tabwriter after the init-scope seat shortened netip.cs and tabwriter.cs) only because both maps then ran
// past the end of the file; a shift that stays inside the file passes it.
//
// THE RULE: a GoPositionMap record in a test info whose Go file and C# file are those of a record in a production
// package_info.cs (beside the test info, in its GOOS subdirectories, or in its parent, the places guard A looks for
// the .cs file) has the same arguments as one of those records. The attribute's spelling (`GoPositionMap`,
// `go.GoPositionMap`) is not compared. A test info's records for its own test files have no production
// counterpart and are not this rule's.
//
// Measured at b0b48783dc: 965 GoPositionMap records in test infos, 4 of them production copies (crypto/sha3
// sha3.cs, net/netip netip.cs and uint128.cs, text/tabwriter tabwriter.cs), all 4 equal. No test-info record names
// a production file's .cs with another Go file, or the reverse.
//
// POSITIVE CONTROL: TestTestInfoPositionMapCopyScannerFires plants a stale copy beside clean controls.

// positionMapArguments reads the string arguments of every GoPositionMap record in an info file, keyed by line.
func positionMapArguments(content string) map[int][]string {
	records := map[int][]string{}

	for index, recordLine := range sourceLines(content) {
		match := positionMapRecordLine.FindStringSubmatch(recordLine)

		if match == nil {
			continue
		}

		var arguments []string

		for _, quoted := range quotedArgument.FindAllStringSubmatch(match[1], -1) {
			arguments = append(arguments, quoted[1])
		}

		if len(arguments) >= 2 {
			records[index+1] = arguments
		}
	}

	return records
}

// scanTestInfoPositionMapCopies reports every production-file GoPositionMap record in a test info that differs
// from the production package_info.cs records for that file, and returns how many copies it checked.
func scanTestInfoPositionMapCopies(infos map[string]string) ([]string, int) {
	production := map[string][][]string{} // directory -> its package_info.cs records

	for infoPath, content := range infos {
		if path.Base(infoPath) != "package_info.cs" {
			continue
		}

		for _, arguments := range positionMapArguments(content) {
			production[path.Dir(infoPath)] = append(production[path.Dir(infoPath)], arguments)
		}
	}

	paths := make([]string, 0, len(infos))

	for p := range infos {
		if path.Base(p) != "package_info.cs" {
			paths = append(paths, p)
		}
	}

	sort.Strings(paths)

	var findings []string
	copies := 0

	for _, infoPath := range paths {
		dir := path.Dir(infoPath)
		candidates := []string{dir, path.Dir(dir)}

		for _, goos := range lineKeyedGOOS {
			candidates = append(candidates, path.Join(dir, goos))
		}

		records := positionMapArguments(infos[infoPath])
		lines := make([]int, 0, len(records))

		for line := range records {
			lines = append(lines, line)
		}

		sort.Ints(lines)

		for _, line := range lines {
			arguments := records[line]
			var counterparts []string
			equal := false

			for _, candidate := range candidates {
				for _, original := range production[candidate] {
					if original[0] != arguments[0] || original[1] != arguments[1] {
						continue
					}

					counterparts = append(counterparts, path.Join(candidate, "package_info.cs"))

					if strings.Join(original, "\x00") == strings.Join(arguments, "\x00") {
						equal = true
					}
				}
			}

			if len(counterparts) == 0 {
				continue
			}

			copies++

			if !equal {
				findings = append(findings, fmt.Sprintf("%s:%d: the GoPositionMap copy for %s (%s) differs from %s, a stale copy of the production record (re-emit the package's tests, or copy the record)",
					infoPath, line, arguments[1], arguments[0], strings.Join(counterparts, ", ")))
			}
		}
	}

	return findings, copies
}

func TestTestInfoPositionMapCopiesEqualProduction(t *testing.T) {
	root := repoRootFromPackageDir(t)
	infos := map[string]string{}

	for _, p := range gitTrackedFiles(t, root) {
		if !packageInfoFileName.MatchString(path.Base(p)) {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))

		if err != nil {
			t.Fatalf("reading %s: %v", p, err)
		}

		if strings.Contains(string(data), "GoPositionMap(") {
			infos[p] = string(data)
		}
	}

	findings, copies := scanTestInfoPositionMapCopies(infos)

	for _, finding := range findings {
		t.Error(finding)
	}

	// The copies are few (4 at b0b48783dc), so a predicate that stopped finding them would pass every tree.
	if copies == 0 {
		t.Fatal("no test info carries a copy of a production GoPositionMap record: wrong root or broken predicate")
	}

	t.Logf("%d package info files read, %d production copies checked", len(infos), copies)
}

// TestTestInfoPositionMapCopyScannerFires plants a stale copy beside clean controls: an equal copy, an equal copy
// spelled `go.GoPositionMap`, a copy of an L3 package's per-GOOS record, and a test file's own record.
func TestTestInfoPositionMapCopyScannerFires(t *testing.T) {
	record := func(attribute, goFile, csFile, table string) string {
		return fmt.Sprintf(`[assembly: %s("%s", "%s", "%s")]`, attribute, goFile, csFile, table)
	}

	infos := map[string]string{
		"p/package_info.cs": strings.Join([]string{
			record("GoPositionMap", "p/a.go", "a.cs", "gJA="),
			record("GoPositionMap", "p/b.go", "b.cs", "gJA="),
		}, "\r\n"),
		"q/linux/package_info.cs": record("GoPositionMap", "q/c_linux.go", "c_linux.cs", "gIA="),
		"p/package_test_info.cs": strings.Join([]string{
			record("GoPositionMap", "p/a.go", "a.cs", "gJA="),    // equal copy
			record("go.GoPositionMap", "p/b.go", "b.cs", "gJA="), // equal, spelled with go.
			record("GoPositionMap", "p/a_test.go", "a_test.cs", "gIA="),
		}, "\n"),
		"q/package_test_info.cs": record("GoPositionMap", "q/c_linux.go", "c_linux.cs", "gIA="),
	}

	findings, copies := scanTestInfoPositionMapCopies(infos)

	if len(findings) != 0 || copies != 3 {
		t.Fatalf("the clean tree reads %d copies (want 3) and %d findings (want 0): %v", copies, len(findings), findings)
	}

	// The plant: p's copy of a.cs keeps a table the production record no longer has.
	infos["p/package_test_info.cs"] = strings.Replace(infos["p/package_test_info.cs"], record("GoPositionMap", "p/a.go", "a.cs", "gJA="), record("GoPositionMap", "p/a.go", "a.cs", "gIA="), 1)
	findings, copies = scanTestInfoPositionMapCopies(infos)

	if len(findings) != 1 || copies != 3 || !strings.Contains(findings[0], "p/package_test_info.cs:1:") || !strings.Contains(findings[0], "a.cs (p/a.go)") {
		t.Fatalf("the stale copy was not named once, at p/package_test_info.cs:1 (copies %d): %v", copies, findings)
	}
}
