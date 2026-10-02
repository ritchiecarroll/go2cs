// nativeRootFieldViewCensus_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// A READING, NOT A GATE: the census of FIELD VIEWS OVER A NATIVE-CAPABLE ROOT OF A REFERENCE-BEARING
// STRUCT (COORD ruling 2026-10-02, on the native-array-view seat). It counts; it does not refuse.
//
// THE CLASS. `p.of(S.Ꮡf)` takes the field's address from the CLR slot: for a root in NATIVE memory that
// is the native base plus f's CLR OFFSET. A converted struct carrying a ж<>, array<>, slice<>, map<> or
// @string field holds managed references, so the CLR lays it out automatically and that offset is not
// Go's. Every read or address taken through such a view of native memory lands on the wrong bytes, with
// no exception: the class is SILENT everywhere it exists, which is why COORD wanted the number before
// the fixes. Measured on the seat that cut this reading, through the real converted
// syscall.RawSockaddrInet4: Port at a CLR offset that is not its Go offset 2
// (GolibTests NativeFieldArrayViewTests).
//
// THE PREDICATE, and what it over- and under-counts, stated rather than discovered later:
//
//	MINT    a local bound by one of the two routes that CAN yield a native box over a struct pointer:
//	        `var X = ….Reinterpret<A, S>()` (native when its source is) and `var X = (ж<S>)(uintptr)(…)`
//	        (native when the address has no provenance record). Whether the root IS native is a run-time
//	        property this scanner cannot see, so the count is of CANDIDATES, an upper bound on the class.
//	VIEW    `X.of(` on a later line of the same file, or the inline `….Reinterpret<A, S>().of(`. The
//	        local is keyed by name per FILE, not per function, so a same-named local in another function
//	        can stand in: another reason the number is a bound, not a census of defects.
//	STRUCT  S reference-bearing, resolved TRANSITIVELY from its declaration by the same table the
//	        native-boundary guard uses (nativeBoundaryBoxDeref_test.go); a name it cannot resolve is
//	        counted UNRESOLVED, never benign.
//	EXCLUDED  comments, and hand-own companion files counted separately (their machinery is usually
//	        the remedy). A line that also carries the native field-view door (NativeFieldArrayPointer)
//	        is counted once, BEHIND THE DOOR: the door views the field at its Go offset for a native
//	        root, and only a managed root reaches the CLR-slot view the raw route takes.
//
// The POSITIVE CONTROL is the site this reading was cut beside: net/darwin/cgo_unix.cs's port alias,
// both arms (RawSockaddrInet4 and RawSockaddrInet6), must be counted.

var reinterpretMintPattern = regexp.MustCompile(`^\s*(?:var\s+)?([A-Za-z_\p{L}][A-Za-z0-9_\p{L}]*)\s*=\s*.*\.Reinterpret<(.+)>\(\)\s*;`)
var uintptrMintPattern = regexp.MustCompile(`^\s*(?:var\s+)?([A-Za-z_\p{L}][A-Za-z0-9_\p{L}]*)\s*=\s*\(ж<([^>]+)>\)\s*\(uintptr\)`)
var inlineReinterpretViewPattern = regexp.MustCompile(`\.Reinterpret<([^()]+)>\(\)\.of\(`)
var fieldViewPattern = regexp.MustCompile(`(^|[^A-Za-z0-9_\p{L}.])([A-Za-z_\p{L}][A-Za-z0-9_\p{L}]*)\.of\(`)

// secondTypeArgument returns the destination of `Reinterpret<A, S>`: the text after the top-level comma.
func secondTypeArgument(args string) string {
	depth := 0

	for i, r := range args {
		switch r {
		case '<':
			depth++
		case '>':
			depth--
		case ',':
			if depth == 0 {
				return strings.TrimSpace(args[i+1:])
			}
		}
	}

	return ""
}

// referenceBearingByConsensus answers for a pointee the shared resolver may call ambiguous. A QUALIFIED
// pointee -- `syscall.RawSockaddrInet4` read from net -- names a struct declared once per flavour
// directory, and structTable.resolve declines a name declared more than once outside the site's own
// package. Each declaration then answers for itself, and the answer stands only when they all agree;
// a disagreement is unresolved, never a verdict. Measured: without this the positive control below read
// 0, the port alias's struct resolving as reference-FREE by default.
func referenceBearingByConsensus(table *structTable, dir, name string) (bearing, resolved bool) {
	if _, ok := table.resolve(dir, name); ok {
		return table.referenceBearing(dir, name, true, nil), true
	}

	decls := table.byName[name]

	if len(decls) == 0 {
		return false, false
	}

	first := table.referenceBearing(decls[0].dir, name, true, nil)

	for _, decl := range decls[1:] {
		if table.referenceBearing(decl.dir, name, true, nil) != first {
			return false, false
		}
	}

	return first, true
}

type fieldViewCensus struct {
	filesScanned  int
	sites         map[string]int // file -> converted-code sites in the class
	behindDoor    map[string]int // file -> sites on a line carrying the door
	handOwn       int
	referenceFree int
	unresolved    map[string]bool
}

func scanNativeRootFieldViews(t *testing.T, root string, tracked []string) fieldViewCensus {
	t.Helper()

	table := buildStructTable(t, root, tracked)
	census := fieldViewCensus{sites: map[string]int{}, behindDoor: map[string]int{}, unresolved: map[string]bool{}}

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") {
			continue
		}

		text := readTrackedText(t, root, rel)
		census.filesScanned++

		handOwn := handOwnMarkerPattern.MatchString(text)
		dir := rel[:strings.LastIndex(rel, "/")]
		minted := map[string]string{} // local -> struct it was minted over
		inBlockComment := false

		classify := func(line string, pointee string) {
			bearing, resolved := referenceBearingByConsensus(table, dir, simpleName(pointee))

			if !resolved {
				census.unresolved[rel+" -> "+pointee] = true
				return
			}

			switch {
			case !bearing:
				census.referenceFree++
			case handOwn:
				census.handOwn++
			case strings.Contains(line, "NativeFieldArrayPointer<"):
				census.behindDoor[rel]++
			default:
				census.sites[rel]++
			}
		}

		for _, line := range strings.Split(text, "\n") {
			trimmed := strings.TrimSpace(line)
			commentHere := inBlockComment || strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "*")

			if strings.HasPrefix(trimmed, "/*") && !strings.Contains(trimmed, "*/") {
				inBlockComment = true
			} else if inBlockComment && strings.Contains(trimmed, "*/") {
				inBlockComment = false
			}

			if commentHere {
				continue
			}

			for _, inline := range inlineReinterpretViewPattern.FindAllStringSubmatch(line, -1) {
				if pointee := secondTypeArgument(inline[1]); pointee != "" {
					classify(line, pointee)
				}
			}

			// A line carrying the door names the same field view twice -- the door's argument and the
			// raw route it falls back to -- so it is ONE site, counted once.
			door := strings.Contains(line, "NativeFieldArrayPointer<")

			for _, view := range fieldViewPattern.FindAllStringSubmatch(line, -1) {
				if pointee, ok := minted[view[2]]; ok {
					classify(line, pointee)

					if door {
						break
					}
				}
			}

			if mint := reinterpretMintPattern.FindStringSubmatch(line); mint != nil {
				if pointee := secondTypeArgument(mint[2]); pointee != "" {
					minted[mint[1]] = pointee
				}
			} else if mint := uintptrMintPattern.FindStringSubmatch(line); mint != nil {
				minted[mint[1]] = mint[2]
			}
		}
	}

	return census
}

func TestNativeRootFieldViewCensus(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a census that scans nothing reads zero", len(tracked))
	}

	census := scanNativeRootFieldViews(t, root, tracked)

	const control = "src/core/net/darwin/cgo_unix.cs"

	if got := census.sites[control] + census.behindDoor[control]; got < 2 {
		t.Fatalf("POSITIVE CONTROL: %s's port alias (RawSockaddrInet4 and RawSockaddrInet6) reads %d sites, want 2; "+
			"the census cannot see the site it was cut beside, so its count means nothing", control, got)
	}

	total, door := 0, 0
	var rows []string

	for file, count := range census.sites {
		total += count
		rows = append(rows, fmt.Sprintf("%4d  %s", count, strings.TrimPrefix(file, "src/core/")))
	}

	for file, count := range census.behindDoor {
		door += count
		rows = append(rows, fmt.Sprintf("%4d  %s (behind the door)", count, strings.TrimPrefix(file, "src/core/")))
	}

	sort.Strings(rows)

	t.Logf("NATIVE-ROOT FIELD VIEWS of a reference-bearing struct (candidates; an upper bound): %d in converted code "+
		"across %d files, + %d behind the native field-view door · excluded: %d in hand-own companions, %d over "+
		"reference-free structs · %d unresolved pointees · %d files scanned",
		total, len(census.sites), door, census.handOwn, census.referenceFree, len(census.unresolved), census.filesScanned)

	for _, row := range rows {
		t.Log(row)
	}

	var unresolved []string

	for row := range census.unresolved {
		unresolved = append(unresolved, row)
	}

	sort.Strings(unresolved)

	for _, row := range unresolved {
		t.Logf("unresolved: %s", row)
	}
}
