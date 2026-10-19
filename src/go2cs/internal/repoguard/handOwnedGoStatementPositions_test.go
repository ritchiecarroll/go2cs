// handOwnedGoStatementPositions_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"encoding/base64"
	"fmt"
	"go/build"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"runtime"
	"sort"
	"strings"
	"testing"

	"go2cs/internal/releasestamp"
)

// A HAND-OWNED `go` STATEMENT NAMES ITS GO LINE (C1's BOARD row, frame lines in hand-owned files; COORD
// ruling of 2026-10-19 on P1's proposal, part 1).
//
// A traceback's "created by" line names the position of the `go` statement that started the goroutine,
// read from the creator's C# line through the file's GoPositionMap record. The converter emits that record
// for a converted file only, so a goroutine started from a hand-owned ([module: GoManualConversion]) file
// printed the .cs path and line where Go prints the .go ones. Each such file now carries a hand-kept
// record: one table entry per `goǃ(` statement, mapping its C# line to the Go line of the `go` statement
// it replaces, each followed by a zero entry, so every other line of the file keeps its honest C#
// position (GoPositionMapAttribute: a frame has a Go position from a record, or its C# one, never half).
//
// THE RULE, for every tracked hand-owned src/core file with a `goǃ(` on a code line:
//
//	(a) the file carries exactly one GoPositionMap record, for its own file name;
//	(b) the record's table maps each `goǃ(` line to a Go line, and that line of the recorded Go file at
//	    the pinned GOROOT is a `go` statement;
//	(c) every other line maps to no Go line: each nonzero entry sits on a `goǃ(` line and the entry after
//	    it is zero.
//
// A hand-owned file with no `goǃ(` and no record is outside the rule. Ordinary frame lines inside
// hand-owned files are NOT covered (the ruling's part 2: disclosed per test, and a generated
// function-granularity map is owed), so (c) keeps a hand-kept table from claiming any of them.
//
// POSITIVE CONTROL: TestHandOwnedGoStatementScannerFires drives the same scanner over planted files, one
// per refusal, plus a clean control that must read empty.

var (
	handOwnMarkerLine    = regexp.MustCompile(handOwnMarkerPredicate)
	goPositionMapRecord  = regexp.MustCompile(`^\s*\[assembly:\s*(?:go\.)?GoPositionMap\(\s*"([^"]*)"\s*,\s*"([^"]*)"\s*,\s*"([^"]*)"`)
	goStatementSourceRow = regexp.MustCompile(`^\s*go\s`)
)

const goStatementCall = "goǃ("

// goStatementLines returns the 1-based lines of a C# file that start a goroutine: a `goǃ(` that is not
// inside a // comment.
func goStatementLines(lines []string) []int {
	var sites []int

	for i, line := range lines {
		at := strings.Index(line, goStatementCall)

		if at < 0 {
			continue
		}

		if comment := strings.Index(line, "//"); comment >= 0 && comment < at {
			continue
		}

		sites = append(sites, i+1)
	}

	return sites
}

// positionTableEntry is one decoded record of a GoPositionMap table.
type positionTableEntry struct{ csLine, goLine int }

// decodePositionTable reads the delta stream GoPositionMapAttribute describes, as the runtime's
// GoPositionMapRecord.decode does: a high-bit byte packs (ΔcsLine-1, zig-zag ΔgoLine) in bits 6-4 and
// 3-0; a 0x00 byte introduces two unsigned varints. Any other byte is a corrupt stream.
func decodePositionTable(table string) ([]positionTableEntry, error) {
	buffer, err := base64.StdEncoding.DecodeString(table)

	if err != nil {
		return nil, err
	}

	var entries []positionTableEntry
	csLine, goLine := 0, 0

	varint := func(index *int) (uint64, error) {
		var value uint64

		for shift := 0; *index < len(buffer); shift += 7 {
			current := buffer[*index]
			*index++
			value |= uint64(current&0x7F) << shift

			if current&0x80 == 0 {
				return value, nil
			}
		}

		return 0, fmt.Errorf("a varint runs past the end of the table")
	}

	for index := 0; index < len(buffer); {
		marker := buffer[index]
		index++

		var advance, zigzag uint64

		switch {
		case marker&0x80 != 0:
			advance, zigzag = uint64(marker>>4&0x07), uint64(marker&0x0F)
		case marker == 0x00:
			if advance, err = varint(&index); err != nil {
				return nil, err
			}

			if zigzag, err = varint(&index); err != nil {
				return nil, err
			}
		default:
			return nil, fmt.Errorf("byte 0x%02x at %d is neither a packed record nor 0x00", marker, index-1)
		}

		csLine += int(advance) + 1
		goLine += int(int64(zigzag>>1) ^ -int64(zigzag&1))
		entries = append(entries, positionTableEntry{csLine, goLine})
	}

	return entries, nil
}

// goLineFor is the runtime's predecessor search (GoPositionMapRecord.GoLineFor): the Go line of the last
// entry at or above csLine, or 0 above the first entry.
func goLineFor(entries []positionTableEntry, csLine int) int {
	goLine := 0

	for _, entry := range entries {
		if entry.csLine > csLine {
			break
		}

		goLine = entry.goLine
	}

	return goLine
}

// scanHandOwnedGoStatements applies the rule to files (repository-relative path to content). readGo returns
// the lines of a GOROOT-relative Go source. It returns one finding per violation.
func scanHandOwnedGoStatements(files map[string]string, readGo func(goFile string) ([]string, error)) []string {
	var findings []string
	paths := make([]string, 0, len(files))

	for p := range files {
		paths = append(paths, p)
	}

	sort.Strings(paths)

	for _, p := range paths {
		lines := strings.Split(strings.ReplaceAll(files[p], "\r\n", "\n"), "\n")
		marked := false

		for _, line := range lines {
			if handOwnMarkerLine.MatchString(line) {
				marked = true
				break
			}
		}

		sites := goStatementLines(lines)

		if !marked || len(sites) == 0 {
			continue
		}

		var records [][]string

		for _, line := range lines {
			if m := goPositionMapRecord.FindStringSubmatch(line); m != nil {
				records = append(records, m)
			}
		}

		csFile := path.Base(p)

		if len(records) != 1 || records[0][2] != csFile {
			findings = append(findings, fmt.Sprintf("%s: %d goǃ( statement(s) at line(s) %v and %d GoPositionMap record(s); want exactly one, for %q (rule a)", p, len(sites), sites, len(records), csFile))
			continue
		}

		goFile, table := records[0][1], records[0][3]
		entries, err := decodePositionTable(table)

		if err != nil {
			findings = append(findings, fmt.Sprintf("%s: the GoPositionMap table does not decode: %v", p, err))
			continue
		}

		goLines, err := readGo(goFile)

		if err != nil {
			findings = append(findings, fmt.Sprintf("%s: the recorded Go file %s cannot be read at the pinned GOROOT: %v", p, goFile, err))
			continue
		}

		isSite := map[int]bool{}

		for _, site := range sites {
			isSite[site] = true
			goLine := goLineFor(entries, site)

			switch {
			case goLine <= 0:
				findings = append(findings, fmt.Sprintf("%s:%d: goǃ( has no Go line in the file's GoPositionMap record (rule b)", p, site))
			case goLine > len(goLines) || !goStatementSourceRow.MatchString(goLines[goLine-1]):
				findings = append(findings, fmt.Sprintf("%s:%d: goǃ( maps to %s:%d, which is not a go statement (rule b)", p, site, goFile, goLine))
			}
		}

		for i, entry := range entries {
			if entry.goLine == 0 {
				continue
			}

			if !isSite[entry.csLine] {
				findings = append(findings, fmt.Sprintf("%s:%d: the GoPositionMap table maps a line that is not a goǃ( statement to %s:%d (rule c)", p, entry.csLine, goFile, entry.goLine))
			} else if i+1 >= len(entries) || entries[i+1].goLine != 0 || entries[i+1].csLine != entry.csLine+1 {
				findings = append(findings, fmt.Sprintf("%s:%d: the goǃ( entry is not followed by a zero entry on the next line, so the lines after it claim %s:%d (rule c)", p, entry.csLine, goFile, entry.goLine))
			}
		}
	}

	return findings
}

// pinnedGoRoot is the GOROOT whose Go files the records are checked against, and it must be the Go release
// src/version.props pins. Under another release a `go` statement can sit on another line or not exist at all
// (WaitGroup.Go is new in Go 1.25), which reads as a confusing red or a false green, so a mismatch fails and
// names both releases, as TestTrackedFixturesMatchPinnedGoRoot does. The version compared is the VERSION file of
// the GOROOT this guard reads, since that is the tree the check depends on.
func pinnedGoRoot(t *testing.T, root string) string {
	props, err := os.ReadFile(filepath.Join(root, "src", "version.props"))

	if err != nil {
		t.Fatalf("cannot read src/version.props: %v", err)
	}

	pin := releasestamp.StdLibVersion(string(props))

	if pin == "" {
		t.Fatalf("src/version.props has no <GoStdLibVersion>; the Go release the records name has no source")
	}

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	if want, got := "go"+pin, goRootVersion(goRoot); got != want {
		t.Fatalf("the hand-owned `go` statements map to %s (src/version.props), but GOROOT %s is %q (running toolchain %s). "+
			"Run with GOTOOLCHAIN=%s, as every lane does; this guard fails rather than skips, since a skip measures nothing",
			want, goRoot, got, runtime.Version(), want)
	}

	return goRoot
}

// TestHandOwnedGoStatementsMapToGo is the guard.
func TestHandOwnedGoStatementsMapToGo(t *testing.T) {
	root := repoRootFromPackageDir(t)
	files := map[string]string{}

	for _, p := range gitTrackedFiles(t, root) {
		if !strings.HasPrefix(p, "src/core/") || !strings.HasSuffix(p, ".cs") {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))

		if err != nil {
			t.Fatalf("reading %s: %v", p, err)
		}

		// A prefilter only: the scanner reads the marker per line, so a placeholder comment that names
		// the attribute does not make a converted file hand-owned.
		if strings.Contains(string(data), "GoManualConversion") {
			files[p] = string(data)
		}
	}

	marked := 0

	for _, content := range files {
		for _, line := range strings.Split(strings.ReplaceAll(content, "\r\n", "\n"), "\n") {
			if handOwnMarkerLine.MatchString(line) {
				marked++
				break
			}
		}
	}

	if marked == 0 {
		t.Fatal("no hand-owned file found under src/core: wrong root or broken marker predicate")
	}

	goRoot := pinnedGoRoot(t, root)

	readGo := func(goFile string) ([]string, error) {
		data, err := os.ReadFile(filepath.Join(goRoot, "src", filepath.FromSlash(goFile)))

		if err != nil {
			return nil, err
		}

		return strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n"), nil
	}

	for _, finding := range scanHandOwnedGoStatements(files, readGo) {
		t.Error(finding)
	}

	t.Logf("%d hand-owned files read, checked against GOROOT %s", marked, goRoot)
}

// TestHandOwnedGoStatementScannerFires plants one violation per rule and a clean control.
func TestHandOwnedGoStatementScannerFires(t *testing.T) {
	goSource := []string{"package p", "", "func f() {", "\tgo func() {}()", "}"}
	readGo := func(string) ([]string, error) { return goSource, nil }

	// A table for (csLine, goLine) pairs, as the hand-kept records are written.
	table := func(pairs ...int) string {
		var stream []byte
		csLine, goLine := 0, 0

		uvarint := func(v uint64) {
			for ; v >= 0x80; v >>= 7 {
				stream = append(stream, byte(v)|0x80)
			}

			stream = append(stream, byte(v))
		}

		for i := 0; i < len(pairs); i += 2 {
			delta := pairs[i+1] - goLine
			zigzag := uint64(delta) << 1

			if delta < 0 {
				zigzag = uint64(-delta)<<1 - 1
			}

			stream = append(stream, 0)
			uvarint(uint64(pairs[i] - csLine - 1))
			uvarint(zigzag)
			csLine, goLine = pairs[i], pairs[i+1]
		}

		return base64.StdEncoding.EncodeToString(stream)
	}

	file := func(record string) string {
		return "[module: go.GoManualConversion]\n" + record + "\nnamespace go;\n\nstatic void f() {\n    goǃ(() => {\n    });\n}\n"
	}

	record := func(csFile, tbl string) string {
		return fmt.Sprintf(`[assembly: go.GoPositionMap("p/p.go", %q, %q)]`, csFile, tbl)
	}

	cases := []struct {
		name  string
		files map[string]string
		want  string
	}{
		{"clean", map[string]string{"src/core/p/p.cs": file(record("p.cs", table(6, 4, 7, 0)))}, ""},
		{"comment only, no record", map[string]string{"src/core/p/p.cs": "[module: go.GoManualConversion]\n// goǃ(f) in a comment\n"}, ""},
		{"no record", map[string]string{"src/core/p/p.cs": file("")}, "rule a"},
		{"another file's record", map[string]string{"src/core/p/p.cs": file(record("q.cs", table(6, 4, 7, 0)))}, "rule a"},
		{"site not covered", map[string]string{"src/core/p/p.cs": file(record("p.cs", table(9, 4, 10, 0)))}, "has no Go line"},
		{"wrong Go line", map[string]string{"src/core/p/p.cs": file(record("p.cs", table(6, 3, 7, 0)))}, "not a go statement"},
		{"no closing zero", map[string]string{"src/core/p/p.cs": file(record("p.cs", table(6, 4)))}, "not followed by a zero entry"},
		{"an extra line claimed", map[string]string{"src/core/p/p.cs": file(record("p.cs", table(2, 1, 6, 4, 7, 0)))}, "not a goǃ( statement"},
	}

	for _, c := range cases {
		findings := scanHandOwnedGoStatements(c.files, readGo)

		if c.want == "" {
			if len(findings) != 0 {
				t.Errorf("%s: want no finding, got %q", c.name, findings)
			}

			continue
		}

		if len(findings) == 0 || !strings.Contains(strings.Join(findings, "\n"), c.want) {
			t.Errorf("%s: want a finding containing %q, got %q", c.name, c.want, findings)
		}
	}
}
