// lineKeyedRecords_test.go - Gbtc
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
	"regexp"
	"sort"
	"strconv"
	"strings"
	"testing"
	"unicode/utf16"

	"go2cs/internal/golauncher"
)

// A LINE-KEYED RECORD LANDS ON ITS CODE (COORD ruling of 2026-11-05 on C2's stale-record scout).
//
// Some records in a package's info files name a position in a converted .cs file: a GoBoundedGoroutine
// record names the line, column and ordinal of a goǃ call, and a GoPositionMap record's table, calls and thunks
// fields name C# lines (and, for a thunk, a column). Two seats that edit one file can merge without a
// conflict into a record that names a position the merged file no longer has there: a bounded record is
// one line per go statement, apart from the file's position map, so it is the case a merge never marks.
// Until this guard only CNR at the union, or GO2CS0010 at build, said so.
//
// THE RULE, for every record in every tracked package info file (package_info.cs, package_test_info.cs,
// package_info_internal_test.cs), checked against the .cs file it names:
//
//	(a) GoBoundedGoroutine(file, line, column, ordinal): the file exists, the text at (line, column) begins goǃ, and
//	    that call is the file's ordinal-th goǃ call (internal/golauncher, the count go2cs-gen and the converter share),
//	    so a record a merge left on a NEIGHBOUR's goǃ fails too; a record without the ordinal is one it cannot read;
//	(b) GoPositionMap: the table decodes, its C# lines increase, no mapped line is past the end of the
//	    file, every calls key is a table line, and every thunk's column follows "=>".
//
// The .cs file is looked for beside the info file, then in its parent directory, then in its GOOS
// subdirectories; a record passes when it fits one of them (an L3 package's flat info file can describe a
// file each GOOS emits on its own). Columns are 1-based UTF-16 units, as the records write them.
//
// Each check reads only committed files: no build, no transpile. Measured at 8ae147d458 (C2's scout): every
// record fits as committed, and with each record read one line off, 209 of 215 bounded records and 1424 of
// 1442 thunks fail. A table line that is blank is NOT a failure: 75 mapped lines are legitimately blank,
// a statement whose emission starts with a separator line.
//
// WHAT (b) CANNOT SEE: a GoPositionMap record that is stale but still FITS its file. Rule (b) checks that a table
// is well formed and inside the file, not that it is this file's table, so a shift that keeps every mapped line,
// calls key and thunk inside the file passes. Two narrower rules close the shapes measured so far: (a)'s ordinal for
// a bounded record, and testInfoPositionMapCopies_test.go for a test info's copy of a production record. A
// production package_info.cs record shifted inside its own file has no witness short of re-emitting the package.
//
// POSITIVE CONTROL: TestLineKeyedRecordScannerFires plants one violation per rule and a clean control.

var (
	boundedGoroutineRecord = regexp.MustCompile(`^\s*\[assembly:\s*(?:global::)?(?:go\.)?GoBoundedGoroutine\(\s*"([^"]+)"\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(-?\d+)\s*\)\]`)
	positionMapRecordLine  = regexp.MustCompile(`^\s*\[assembly:\s*(?:global::)?(?:go\.)?GoPositionMap\((.*)\)\]\s*$`)
	quotedArgument         = regexp.MustCompile(`"((?:[^"\\]|\\.)*)"`)
	packageInfoFileName    = regexp.MustCompile(`^package_(?:info|test_info|info_internal_test)\.cs$`)
)

// lineKeyedGOOS are the GOOS subdirectories an L3 package emits a per-OS file into.
var lineKeyedGOOS = []string{"darwin", "linux", "windows"}

// sourceLines splits a converted file into its lines: BOM dropped, CRLF folded, no phantom line after a
// final newline.
func sourceLines(content string) []string {
	content = strings.TrimPrefix(content, "\uFEFF")
	content = strings.ReplaceAll(content, "\r\n", "\n")
	content = strings.TrimSuffix(content, "\n")

	return strings.Split(content, "\n")
}

// textBeforeColumn is a line's text before a 1-based UTF-16 column, and whether the column is on the line.
func textBeforeColumn(line string, column int) (string, bool) {
	units := utf16.Encode([]rune(line))

	if column < 1 || column-1 > len(units) {
		return "", false
	}

	return string(utf16.Decode(units[:column-1])), true
}

// textFromColumn is a line's text from a 1-based UTF-16 column on.
func textFromColumn(line string, column int) (string, bool) {
	units := utf16.Encode([]rune(line))

	if column < 1 || column-1 > len(units) {
		return "", false
	}

	return string(utf16.Decode(units[column-1:])), true
}

// lineKeyedCandidates are the repository-relative paths a record in infoPath may name csFile by.
func lineKeyedCandidates(infoPath, csFile string, exists func(string) bool) []string {
	dir := path.Dir(infoPath)
	var candidates []string

	for _, p := range []string{path.Join(dir, csFile), path.Join(path.Dir(dir), csFile)} {
		if exists(p) {
			return []string{p}
		}
	}

	for _, goos := range lineKeyedGOOS {
		if p := path.Join(dir, goos, csFile); exists(p) {
			candidates = append(candidates, p)
		}
	}

	return candidates
}

// boundedRecordProblem is why a bounded record does not land on its goǃ call in lines, or "".
func boundedRecordProblem(lines []string, line, column, ordinal int) string {
	if line < 1 || line > len(lines) {
		return fmt.Sprintf("line %d is past the end of the file (%d lines)", line, len(lines))
	}

	text, ok := textFromColumn(lines[line-1], column)

	if !ok {
		return fmt.Sprintf("column %d is past the end of line %d", column, line)
	}

	if !strings.HasPrefix(text, "goǃ") {
		return fmt.Sprintf("line %d column %d reads %q, not a goǃ call", line, column, truncateForReport(text))
	}

	for _, call := range golauncher.Positions(strings.Join(lines, "\n")) {
		if call.Line == line && call.Column == column {
			if call.Ordinal != ordinal {
				return fmt.Sprintf("the record names goǃ call #%d, but line %d column %d holds goǃ call #%d (a record left on a neighbour's call)",
					ordinal, line, column, call.Ordinal)
			}

			return ""
		}
	}

	return fmt.Sprintf("line %d column %d begins goǃ, but not a goǃ call the ordinal count reads (a comment, a string, or not a call)", line, column)
}

// positionMapProblems are the reasons a GoPositionMap record's C#-line fields do not fit lines.
func positionMapProblems(lines []string, table, calls, thunks string) []string {
	entries, err := decodePositionTable(table)

	if err != nil {
		return []string{fmt.Sprintf("the table does not decode: %v", err)}
	}

	var problems []string
	tableLines := map[int]bool{}
	previous := 0

	for _, entry := range entries {
		if entry.csLine <= previous {
			problems = append(problems, fmt.Sprintf("the table's C# lines do not increase (%d after %d)", entry.csLine, previous))
		}

		previous = entry.csLine
		tableLines[entry.csLine] = true
	}

	if previous > len(lines) {
		problems = append(problems, fmt.Sprintf("the table maps line %d, past the end of the file (%d lines)", previous, len(lines)))
	}

	for _, group := range strings.Split(calls, ";") {
		if group == "" {
			continue
		}

		key, _, _ := strings.Cut(group, "=")

		if line, err := strconv.Atoi(key); err != nil || !tableLines[line] {
			problems = append(problems, fmt.Sprintf("calls group %q is keyed by a line the table does not map", truncateForReport(group)))
		}
	}

	for _, thunk := range strings.Split(thunks, ",") {
		if thunk == "" {
			continue
		}

		lineText, columnText, _ := strings.Cut(thunk, ":")
		line, lineErr := strconv.Atoi(lineText)
		column, columnErr := strconv.Atoi(columnText)

		if lineErr != nil || columnErr != nil || line < 1 || line > len(lines) {
			problems = append(problems, fmt.Sprintf("thunk %q is not a line of the file", thunk))
			continue
		}

		before, ok := textBeforeColumn(lines[line-1], column)

		if !ok || !strings.HasSuffix(strings.TrimRight(before, " \t"), "=>") {
			problems = append(problems, fmt.Sprintf("thunk %s does not follow \"=>\" (line %d reads %q)", thunk, line, truncateForReport(strings.TrimSpace(lines[line-1]))))
		}
	}

	return problems
}

func truncateForReport(text string) string {
	if runes := []rune(text); len(runes) > 60 {
		return string(runes[:60]) + "..."
	}

	return text
}

// scanLineKeyedRecords applies the rule. infos maps each package info file's repository-relative path to its
// content; readSource returns a converted file's lines, and whether it exists.
func scanLineKeyedRecords(infos map[string]string, readSource func(string) ([]string, bool)) []string {
	exists := func(p string) bool {
		_, ok := readSource(p)
		return ok
	}

	var findings []string
	paths := make([]string, 0, len(infos))

	for p := range infos {
		paths = append(paths, p)
	}

	sort.Strings(paths)

	for _, infoPath := range paths {
		for index, recordLine := range sourceLines(infos[infoPath]) {
			at := fmt.Sprintf("%s:%d", infoPath, index+1)

			if match := boundedGoroutineRecord.FindStringSubmatch(recordLine); match != nil {
				line, _ := strconv.Atoi(match[2])
				column, _ := strconv.Atoi(match[3])
				ordinal, _ := strconv.Atoi(match[4])
				findings = append(findings, checkAgainstCandidates(at, "GoBoundedGoroutine", match[1], infoPath, exists, readSource,
					func(lines []string) []string {
						if problem := boundedRecordProblem(lines, line, column, ordinal); problem != "" {
							return []string{problem}
						}

						return nil
					})...)

				continue
			}

			match := positionMapRecordLine.FindStringSubmatch(recordLine)

			if match == nil {
				// A record of either kind that the patterns do not read would pass unchecked, so it is a finding.
				if strings.HasPrefix(strings.TrimSpace(recordLine), "[assembly:") &&
					(strings.Contains(recordLine, "GoBoundedGoroutine(") || strings.Contains(recordLine, "GoPositionMap(")) {
					findings = append(findings, fmt.Sprintf("%s: a line-keyed record this guard cannot read: %s", at, truncateForReport(strings.TrimSpace(recordLine))))
				}

				continue
			}

			var arguments []string

			for _, quoted := range quotedArgument.FindAllStringSubmatch(match[1], -1) {
				arguments = append(arguments, quoted[1])
			}

			if len(arguments) < 3 {
				findings = append(findings, fmt.Sprintf("%s: a GoPositionMap record with %d string arguments, fewer than its three", at, len(arguments)))
				continue
			}

			for len(arguments) < 7 {
				arguments = append(arguments, "")
			}

			findings = append(findings, checkAgainstCandidates(at, "GoPositionMap", arguments[1], infoPath, exists, readSource,
				func(lines []string) []string {
					return positionMapProblems(lines, arguments[2], arguments[5], arguments[6])
				})...)
		}
	}

	return findings
}

// checkAgainstCandidates runs check against each file csFile can name and reports when it fits none.
func checkAgainstCandidates(at, kind, csFile, infoPath string, exists func(string) bool, readSource func(string) ([]string, bool), check func([]string) []string) []string {
	candidates := lineKeyedCandidates(infoPath, csFile, exists)

	if len(candidates) == 0 {
		return []string{fmt.Sprintf("%s: %s names %s, which is not beside the info file, in its parent or in a GOOS subdirectory", at, kind, csFile)}
	}

	var reasons []string

	for _, candidate := range candidates {
		lines, _ := readSource(candidate)
		problems := check(lines)

		if len(problems) == 0 {
			return nil
		}

		reasons = append(reasons, candidate+": "+strings.Join(problems, "; "))
	}

	return []string{fmt.Sprintf("%s: %s does not land on its code (a stale record; re-emit the package): %s", at, kind, strings.Join(reasons, " | "))}
}

func TestLineKeyedRecordsLandOnTheirCode(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := map[string]bool{}
	infos := map[string]string{}

	for _, p := range gitTrackedFiles(t, root) {
		tracked[p] = true

		if !packageInfoFileName.MatchString(path.Base(p)) {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))

		if err != nil {
			t.Fatalf("reading %s: %v", p, err)
		}

		if strings.Contains(string(data), "GoPositionMap(") || strings.Contains(string(data), "GoBoundedGoroutine(") {
			infos[p] = string(data)
		}
	}

	if len(infos) == 0 {
		t.Fatal("no package info file with a GoPositionMap or GoBoundedGoroutine record found: wrong root or broken predicate")
	}

	cache := map[string][]string{}

	readSource := func(p string) ([]string, bool) {
		if !tracked[p] {
			return nil, false
		}

		if lines, ok := cache[p]; ok {
			return lines, true
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))

		if err != nil {
			return nil, false
		}

		cache[p] = sourceLines(string(data))
		return cache[p], true
	}

	for _, finding := range scanLineKeyedRecords(infos, readSource) {
		t.Error(finding)
	}

	t.Logf("%d package info files read", len(infos))
}

// TestLineKeyedRecordScannerFires plants one violation per rule and a clean control.
func TestLineKeyedRecordScannerFires(t *testing.T) {
	// a.cs: line 1 a goǃ call at column 5, line 3 a thunk body at column 17 (after "() => "); n.cs: two goǃ calls on
	// consecutive lines, so a record one line off still begins a goǃ call (the neighbour's), and only its ordinal tells.
	clean := []string{"    goǃ(f, x);", "", "    defer(() => where(), ref ᒐ);"}
	neighbours := []string{"    goǃ(f, x);", "    goǃ(g, y);"}
	// "gJA=" is two packed table records, bytes 0x80 0x90: C# line 1 and C# line 3, both Go line 0.
	table := "gJA="

	entries, err := decodePositionTable(table)
	if err != nil || len(entries) != 2 || entries[0].csLine != 1 || entries[1].csLine != 3 {
		t.Fatalf("the planted table %q decodes to %v (%v); want C# lines 1 and 3", table, entries, err)
	}

	record := func(calls, thunks string) string {
		return fmt.Sprintf("[assembly: GoPositionMap(\"a.go\", \"a.cs\", \"%s\", \"\", \"\", \"%s\", \"%s\")]", table, calls, thunks)
	}

	sources := map[string][]string{"p/a.cs": clean, "p/b.cs": append([]string{"// shifted"}, clean...), "p/n.cs": neighbours}
	readSource := func(p string) ([]string, bool) { lines, ok := sources[p]; return lines, ok }

	cases := []struct {
		name, info, want string
	}{
		{"clean", "[assembly: GoBoundedGoroutine(\"a.cs\", 1, 5, 0)]\n[assembly: GoBoundedGoroutine(\"n.cs\", 2, 5, 1)]\n" + record("3=where/1/1/0", "3:17"), ""},
		{"bounded shifted", "[assembly: GoBoundedGoroutine(\"b.cs\", 1, 5, 0)]", "not a goǃ call"},
		{"bounded on a neighbour's call", "[assembly: GoBoundedGoroutine(\"n.cs\", 2, 5, 0)]", "holds goǃ call #1"},
		{"bounded without its ordinal", "[assembly: GoBoundedGoroutine(\"a.cs\", 1, 5)]", "cannot read"},
		{"bounded shifted, qualified", "[assembly: global::go.GoBoundedGoroutine(\"b.cs\", 1, 5, 0)]", "not a goǃ call"},
		{"bounded missing file", "[assembly: GoBoundedGoroutine(\"c.cs\", 1, 5, 0)]", "not beside the info file"},
		{"bounded past the end", "[assembly: GoBoundedGoroutine(\"a.cs\", 9, 5, 0)]", "past the end of the file"},
		{"record the guard cannot read", "[assembly: GoBoundedGoroutine(\"a.cs\", line: 1, 5, 0)]", "cannot read"},
		{"thunk shifted", strings.Replace(record("", "3:17"), "\"a.cs\"", "\"b.cs\"", 1), "does not follow \"=>\""},
		{"calls key not a table line", record("2=where/1/1/0", ""), "keyed by a line the table does not map"},
		// "gPA=" maps C# lines 1 and 9 (bytes 0x80 0xF0) against the 3-line file.
		{"table past the end", strings.Replace(record("", ""), table, "gPA=", 1), "past the end of the file"},
		// "AQ==" is the byte 0x01, neither a packed record nor 0x00.
		{"table corrupt", strings.Replace(record("", ""), table, "AQ==", 1), "does not decode"},
	}

	for _, c := range cases {
		findings := scanLineKeyedRecords(map[string]string{"p/package_info.cs": c.info}, readSource)

		if c.want == "" {
			if len(findings) != 0 {
				t.Errorf("%s: the clean control reads %v", c.name, findings)
			}

			continue
		}

		if len(findings) != 1 || !strings.Contains(findings[0], c.want) {
			t.Errorf("%s: findings %v; want one naming %q", c.name, findings, c.want)
		}
	}
}
