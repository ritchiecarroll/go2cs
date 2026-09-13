// projitemsIntegrity_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"encoding/xml"
	"fmt"
	"io"
	"io/fs"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"testing"
)

// go2cs-src.projitems is the Visual Studio shared-project item list for the converter's own Go
// sources: go2cs-src.shproj imports it, and that shproj is a member of src\go2cs.slnx. NOTHING
// builds from it — `go build` walks the directory, not the item list — so an entry that is never
// added costs nothing at the command line and no gate notices. It only bites in Visual Studio,
// where an unlisted source simply is not in Solution Explorer: invisible to anyone working there,
// and invisible to the person who forgot it, because their own build stayed green.
//
// That makes it a one-directional drift — files land on disk, the list is only ever updated by
// hand — and it had already drifted (untypedPackageConversion_test.go, added by the issue-#33 arc,
// went unregistered until 2026-08-06). These guards are the same invariant check
// tests\Behavioral\check-solution-integrity.ps1 applies to go2cs.slnx, in the cheapest place that
// cannot be forgotten: the converter's existing `go test ./...` gate.

const (
	// projitemsFileName is resolved relative to the test's working directory, which `go test` sets
	// to the package directory — the same directory $(MSBuildThisFileDirectory) expands to.
	projitemsFileName = "go2cs-src.projitems"

	// projitemsIncludePrefix is the MSBuild property every Include is written against; the text
	// after it is the item's path relative to this directory, backslash-separated.
	projitemsIncludePrefix = `$(MSBuildThisFileDirectory)`
)

var utf8ByteOrderMark = []byte{0xEF, 0xBB, 0xBF}

// projitemsEntry is one registered item. The MSBuild item type is carried alongside the path
// because the two ItemGroups are ordered independently, so an insertion hint has to search within
// the group the new entry belongs to.
type projitemsEntry struct {
	item string // "None" (sources, go.mod/go.sum, publish profiles, docs) or "Content" (templates, icons)
	path string // relative to src\go2cs, backslash-separated, exactly as written after the prefix
}

// Every Go source under src\go2cs must be registered, including the internal\ command packages
// (which appear with backslash-separated relative paths).
func TestProjitemsRegistersEveryGoSource(t *testing.T) {
	entries := readProjitemsEntries(t)
	goSources, _ := walkConverterTree(t)

	registered := make(map[string]bool, len(entries))

	for _, entry := range entries {
		registered[entry.path] = true
	}

	var missing []string

	for _, source := range goSources {
		if !registered[source] {
			missing = append(missing, source)
		}
	}

	if len(missing) == 0 {
		return
	}

	var report strings.Builder

	fmt.Fprintf(&report, "%s does not register %d of the converter's Go sources.\n", projitemsFileName, len(missing))
	report.WriteString("Visual Studio shows only what this file lists, so each one is missing from Solution\n")
	report.WriteString("Explorer. Add these lines to the <None> ItemGroup, each after the entry named below it:\n")

	for _, source := range missing {
		report.WriteString("\n")
		report.WriteString(projitemsInsertionHint(source, entries))
	}

	report.WriteString("\n" + projitemsEditingNote)

	t.Error(report.String())
}

// The other direction: a registered path that no longer exists on disk (renamed or deleted source,
// retired publish profile). Harmless to MSBuild — a shared project does not fail on a missing item
// — which is precisely why it can sit there indefinitely.
func TestProjitemsHasNoDanglingEntries(t *testing.T) {
	entries := readProjitemsEntries(t)
	_, onDisk := walkConverterTree(t)

	var dangling []string

	for _, entry := range entries {
		if !onDisk[entry.path] {
			dangling = append(dangling, fmt.Sprintf("    <%s Include=\"%s%s\" />", entry.item, projitemsIncludePrefix, entry.path))
		}
	}

	if len(dangling) == 0 {
		return
	}

	t.Errorf("%s registers %d path(s) that do not exist on disk (renamed or deleted?). Remove:\n%s\n%s",
		projitemsFileName, len(dangling), strings.Join(dangling, "\n"), projitemsEditingNote)
}

// The item list is UTF-8 WITH a byte-order mark, and Visual Studio rewrites it that way every time
// it touches the file — so a writer that drops the BOM is undone (and the whole file churns) at the
// next VS save. The BOM is real content in the blob, which git neither adds nor strips, so it is
// checkable in any checkout.
//
// Line endings are NOT checkable that way: the blob stores LF and core.autocrlf smudges the working
// tree to CRLF on Windows, so the file is legitimately CRLF here and LF on a Linux clone. The
// portable invariant is that whatever a checkout has, the file uses it CONSISTENTLY — which is
// exactly what a hand-inserted `<None>` line gets wrong when the editor writes a bare LF into the
// CRLF file the guard above asks it to edit.
func TestProjitemsKeepsItsByteOrderMarkAndConsistentLineEndings(t *testing.T) {
	contents, err := os.ReadFile(projitemsFileName)

	if err != nil {
		t.Fatalf("read %s: %v", projitemsFileName, err)
	}

	if !bytes.HasPrefix(contents, utf8ByteOrderMark) {
		t.Errorf("%s lost its UTF-8 byte-order mark; Visual Studio will put it back and rewrite the whole file.\n%s",
			projitemsFileName, projitemsEditingNote)
	}

	body := bytes.TrimPrefix(contents, utf8ByteOrderMark)

	lineFeeds := bytes.Count(body, []byte("\n"))
	carriageReturns := bytes.Count(body, []byte("\r"))
	pairs := bytes.Count(body, []byte("\r\n"))

	if carriageReturns != pairs {
		t.Errorf("%s has %d carriage return(s) not paired with a line feed", projitemsFileName, carriageReturns-pairs)
	}

	if pairs != 0 && pairs != lineFeeds {
		t.Errorf("%s has mixed line endings: %d CRLF and %d bare LF. Match what the rest of the file uses.\n%s",
			projitemsFileName, pairs, lineFeeds-pairs, projitemsEditingNote)
	}
}

const projitemsEditingNote = "The file is UTF-8 WITH a BOM and its line endings are uniform: preserve both. " +
	"PowerShell 5.1\nGet-Content/Out-File corrupts it — edit in place, or use [System.IO.File]::ReadAllText/WriteAllText."

// readProjitemsEntries returns every registered item in file order. It decodes the document rather
// than pattern-matching lines so an Include this guard cannot interpret is a loud failure instead of
// a silently skipped entry — a guard that quietly ignores what it does not recognize guards nothing.
func readProjitemsEntries(t *testing.T) []projitemsEntry {
	t.Helper()

	contents, err := os.ReadFile(projitemsFileName)

	if err != nil {
		t.Fatalf("read %s: %v", projitemsFileName, err)
	}

	decoder := xml.NewDecoder(bytes.NewReader(bytes.TrimPrefix(contents, utf8ByteOrderMark)))

	var entries []projitemsEntry

	for {
		token, err := decoder.Token()

		if err == io.EOF {
			break
		}

		if err != nil {
			t.Fatalf("%s is not well-formed XML: %v", projitemsFileName, err)
		}

		element, isStart := token.(xml.StartElement)

		if !isStart {
			continue
		}

		for _, attribute := range element.Attr {
			if attribute.Name.Local != "Include" {
				continue
			}

			path, hasPrefix := strings.CutPrefix(attribute.Value, projitemsIncludePrefix)

			if !hasPrefix {
				t.Fatalf("<%s Include=%q /> in %s is not written against %s, so this guard cannot resolve it to a file",
					element.Name.Local, attribute.Value, projitemsFileName, projitemsIncludePrefix)
			}

			entries = append(entries, projitemsEntry{item: element.Name.Local, path: path})
		}
	}

	if len(entries) == 0 {
		t.Fatalf("%s registers no items at all; this guard is reading the wrong file", projitemsFileName)
	}

	return entries
}

// walkConverterTree walks src\go2cs once and returns the Go sources (sorted) plus every file it
// found, both keyed the way the item list writes them: relative and backslash-separated on any host.
// The full set is what the dangling check tests against — comparing to the walk rather than to
// os.Stat keeps the check case-sensitive on Windows too, so a mis-cased entry is caught here
// instead of on the next Linux build.
func walkConverterTree(t *testing.T) (goSources []string, onDisk map[string]bool) {
	t.Helper()

	onDisk = map[string]bool{}

	err := filepath.WalkDir(".", func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}

		if entry.IsDir() {
			return nil
		}

		relative := strings.ReplaceAll(filepath.ToSlash(path), "/", `\`)

		onDisk[relative] = true

		if strings.HasSuffix(entry.Name(), ".go") {
			goSources = append(goSources, relative)
		}

		return nil
	})

	if err != nil {
		t.Fatalf("walk the converter source tree: %v", err)
	}

	sort.Strings(goSources)

	return goSources, onDisk
}

// projitemsInsertionHint renders the line a missing source needs, plus the entry it belongs after,
// so the fix is one mechanical edit that lands where Visual Studio would have put it. Both
// ItemGroups are held in case-insensitive ordinal order by path (verified against the file as
// committed); inserting anywhere else works but VS re-sorts it on the next touch, turning a
// one-line fix into a diff.
func projitemsInsertionHint(missing string, entries []projitemsEntry) string {
	key := strings.ToLower(missing)
	predecessor := ""

	for _, entry := range entries {
		if entry.item == "None" && strings.ToLower(entry.path) < key {
			predecessor = entry.path
		}
	}

	line := fmt.Sprintf(`    <None Include="%s%s" />`, projitemsIncludePrefix, missing)

	if predecessor == "" {
		return line + "\n  as the FIRST entry of the <None> ItemGroup\n"
	}

	return fmt.Sprintf("%s\n  after\n    <None Include=\"%s%s\" />\n", line, projitemsIncludePrefix, predecessor)
}
