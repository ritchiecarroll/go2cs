// goroutineStackRecords.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"slices"
	"sort"
	"strconv"
	"strings"

	"go2cs/internal/golauncher"

	"github.com/ritchiecarroll/hashset"
)

// A goroutine the converter proves cannot recurse (goroutineStackAnalysis.go) starts on golib's bounded stack
// reservation. Its `go` statement is emitted exactly as any other, `goǃ(f, x)`, so it reads like the Go `go f(x)`; the
// mark lives in the package's info file instead, one `[assembly: GoBoundedGoroutine("<file>.cs", <line>, <column>,
// <ordinal>)]` record per statement, naming where the `goǃ` invocation starts and which `goǃ` call of the file it is
// (internal/golauncher). go2cs-gen intercepts the invocation at that position and forwards it to the GoStack.Bounded
// rung (BoundedGoroutineInterceptorGenerator). A record that no longer lands on an unmarked `goǃ` invocation is a build
// error there, never a goroutine started on the wrong reservation, and a `go` statement without a record keeps the
// Go-scale default.
//
// The line and column are not knowable while the statement is being emitted (positionMapOperations.go says why), so
// visitGoStmt writes a BOUNDED sentinel immediately before `goǃ`, and finalizePositionMap reads its position off the
// finished text with the other sentinels. Where no info file owns the emission (a single-file conversion) there is
// nowhere to record the site, so the statement keeps the visible `GoStack.Bounded` argument and its reservation.

// BoundedGoSentinelMark is the content of a BOUNDED sentinel, SENTINEL 00 SENTINEL. Like ThunkSentinelMark it is
// decimal, so the text tests that read emitted text see no identifier, and it can never be a statement's Go line.
const BoundedGoSentinelMark = "00"

// boundedGoRecords collects the records this conversion produced, keyed by the info file of the compilation that
// compiles the marked source, as positionMapRecords is. Cleared for a target by claimPositionMapTarget.
var boundedGoRecords = map[string][]string{}

// boundedGoSite is one marked `goǃ` invocation: its 1-based emitted C# line and the 1-based UTF-16 column it starts at,
// the units Roslyn's line positions count in, and its 0-based ordinal among every `goǃ` of the file.
type boundedGoSite struct {
	csLine  int
	column  int
	ordinal int
}

// boundedGoRecordable reports whether this emission can record a bounded `go` statement in an info file; when it cannot,
// visitGoStmt keeps the visible GoStack.Bounded argument.
func (v *Visitor) boundedGoRecordable() bool {
	return v.fset != nil && strings.TrimSpace(v.options.positionMapTarget) != ""
}

// stripPositionSentinelsKeepingBounded is stripPositionSentinels for a site that REWRITES captured text from the stripped
// form (convFuncLit's single-return collapse): every sentinel goes but a BOUNDED one, which is the only mark its go
// statement has. Dropped, the statement would lose both its visible argument and its record, and its goroutine would
// start on the default reservation instead of the one the converter proved it needs no more than.
func stripPositionSentinelsKeepingBounded(fragment string) string {
	bounded := boundedGoSentinel()
	parts := strings.Split(fragment, bounded)

	for i := range parts {
		parts[i] = stripPositionSentinels(parts[i])
	}

	return strings.Join(parts, bounded)
}

// boundedGoSentinel marks the start of a bounded `goǃ` invocation. Write it immediately before `goǃ(`.
func boundedGoSentinel() string {
	return PositionSentinel + BoundedGoSentinelMark + PositionSentinel
}

// recordBoundedGoSites queues this file's bounded sites for its compilation's info file.
func recordBoundedGoSites(target string, csFile string, sites []boundedGoSite) {
	if len(sites) == 0 {
		return
	}

	records := make([]string, 0, len(sites))

	for _, site := range sites {
		records = append(records, fmt.Sprintf("[assembly: %s(%s, %d, %d, %d)]", globalQualifyRooted("go.GoBoundedGoroutine"),
			csharpStringLiteral(csFile), site.csLine, site.column, site.ordinal))
	}

	packageLock.Lock()
	boundedGoRecords[target] = append(boundedGoRecords[target], records...)
	packageLock.Unlock()
}

// boundedGoRecordsProseLines is the comment above the records. applyBoundedGoRecords removes only this exact text, so a
// change to its wording leaves the old comment in every info file a later conversion merges into.
func boundedGoRecordsProseLines() []string {
	return []string{
		"// The go statements whose goroutine the converter proved cannot recurse, by the file, line and column of",
		"// their goǃ call: go2cs-gen starts each on golib's bounded stack reservation rather than the Go-scale default.",
	}
}

// isBoundedGoRecordLine reports whether a line is an assembly-level [GoBoundedGoroutine] record, in any qualification.
func isBoundedGoRecordLine(line string) bool {
	trimmed := strings.TrimSpace(line)

	for _, prefix := range []string{"[assembly: GoBoundedGoroutine(", "[assembly: go.GoBoundedGoroutine(", "[assembly: global::go.GoBoundedGoroutine("} {
		if strings.HasPrefix(trimmed, prefix) {
			return true
		}
	}

	return false
}

// boundedGoRecordFile is a record's file argument, the first quoted field.
func boundedGoRecordFile(record string) string {
	parts := strings.Split(record, "\"")

	if len(parts) >= 2 {
		return parts[1]
	}

	return record
}

// applyBoundedGoRecords writes the package's bounded-goroutine records, sorted, immediately above the
// <GoSourcePositionMaps> prose, and removes any it held before. A whole-package conversion rebuilds them from this
// run's records alone. A merging write (the -tests flow, single-file conversions) keeps the records already there for
// every file this conversion did not re-emit, as positionMapSectionLines keeps their position maps: the seeded test-info
// file of a recompile-model test assembly is how production's records reach it. A re-emitted file's records are this
// run's alone, so a statement that no longer proves bounded loses its record.
func applyBoundedGoRecords(packageInfoLines []string, packageInfoFileName string, mergeExisting bool) []string {
	prose := boundedGoRecordsProseLines()
	kept := make([]string, 0, len(packageInfoLines))
	var existing []string
	bare := importsRootNamespace(packageInfoLines)

	for i := 0; i < len(packageInfoLines); i++ {
		if !isBoundedGoRecordLine(packageInfoLines[i]) {
			kept = append(kept, packageInfoLines[i])
			continue
		}

		existing = append(existing, strings.TrimSpace(packageInfoLines[i]))

		// The records' prose, written just above the first, and the blank line after the last go with them.
		if len(kept) >= len(prose) && slices.Equal(kept[len(kept)-len(prose):], prose) {
			kept = kept[:len(kept)-len(prose)]
		}

		if i+1 < len(packageInfoLines) && strings.TrimSpace(packageInfoLines[i+1]) == "" {
			i++
		}
	}

	packageLock.Lock()
	fresh := append([]string(nil), boundedGoRecords[packageInfoFileName]...)
	emitted := hashset.HashSet[string]{}

	for _, record := range positionMapRecords[packageInfoFileName] {
		emitted.Add(positionMapRecordKey(record))
	}
	packageLock.Unlock()

	records := hashset.HashSet[string]{}

	for _, record := range fresh {
		records.Add(respellRootAttribute(record, "GoBoundedGoroutine", bare))
	}

	if mergeExisting {
		for _, record := range existing {
			if !emitted.Contains(boundedGoRecordFile(record)) {
				records.Add(respellRootAttribute(record, "GoBoundedGoroutine", bare))
			}
		}
	}

	if records.IsEmpty() {
		return kept
	}

	anchor := slices.Index(kept, goSourcePositionMapsProseLines()[0])

	if anchor < 0 {
		anchor = slices.IndexFunc(kept, func(line string) bool { return strings.Contains(line, "<GoSourcePositionMaps>") })
	}

	if anchor < 0 {
		showWarning("Package info file \"%s\" has no <GoSourcePositionMaps> section; its [GoBoundedGoroutine] records were not emitted", packageInfoFileName)
		return kept
	}

	lines := records.Keys()
	sortBoundedGoRecords(lines)

	return slices.Concat(kept[:anchor], prose, lines, []string{""}, kept[anchor:])
}

// sortBoundedGoRecords orders records by file, then line and column as numbers, so a file's records read in the order
// its statements do.
func sortBoundedGoRecords(records []string) {
	key := func(record string) (string, int, int) {
		file := boundedGoRecordFile(record)
		_, rest, _ := strings.Cut(record, "\", ")
		fields := strings.Split(strings.TrimSuffix(strings.TrimSpace(rest), ")]"), ",")
		field := func(i int) int {
			if i >= len(fields) {
				return 0
			}

			n, _ := strconv.Atoi(strings.TrimSpace(fields[i]))
			return n
		}

		return file, field(0), field(1)
	}

	sort.SliceStable(records, func(i, j int) bool {
		fi, li, ci := key(records[i])
		fj, lj, cj := key(records[j])

		if fi != fj {
			return fi < fj
		}

		if li != lj {
			return li < lj
		}

		return ci < cj
	})
}

// assignBoundedGoOrdinals gives each bounded site its ordinal among the file's `goǃ` calls. A site the scan does not
// find is the converter disagreeing with itself; it keeps ordinal -1, which go2cs-gen and repoguard both refuse, so the
// disagreement fails loudly instead of recording a wrong number.
func assignBoundedGoOrdinals(text string, sites []boundedGoSite) []boundedGoSite {
	if len(sites) == 0 {
		return sites
	}

	ordinals := map[[2]int]int{}

	for _, launcher := range golauncher.Positions(text) {
		ordinals[[2]int{launcher.Line, launcher.Column}] = launcher.Ordinal
	}

	for i := range sites {
		ordinal, found := ordinals[[2]int{sites[i].csLine, sites[i].column}]

		if !found {
			ordinal = -1
			showWarning("A bounded goǃ call at line %d column %d is not a goǃ identifier the ordinal scan reads; its record carries ordinal -1", sites[i].csLine, sites[i].column)
		}

		sites[i].ordinal = ordinal
	}

	return sites
}
