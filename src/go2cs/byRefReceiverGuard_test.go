// byRefReceiverGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"io/fs"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"
	"testing"
)

// byRefReceiverHelpers is the ALLOWLIST of committed hand-written extension methods that take their
// receiver by reference with neither mark and are not Go methods at all. golib reads every other
// unmarked by-ref receiver as a Go POINTER-receiver method (TypeExtensions.IsPointerSetByRefReceiver),
// so a hand-written by-ref receiver says what it is: [GoRecv] for a pointer receiver (hand-written
// files keep the attribute; docs/PLAN-marker-comment-parity.md, 5.1), [GoCopyBound] for a VALUE-set
// method, or an entry here. Each entry is tagged with the class that keeps it out of every Go method
// set: its receiver is no Go type, or its name is not exported.
var byRefReceiverHelpers = map[string]struct {
	count int
	class string
}{
	"golib/builtin.cs\tToUTF8Bytes": {1, "span receiver (ReadOnlySpan<rune> is a ref struct, never a Go dynamic type)"},
	"golib/slice.cs\tslice":         {4, "golib slicing helper (`in slice<T>`; the name is unexported, so no method table lists it)"},
}

// The declaration test reads PAST any parenthesis before the receiver: a tuple return type
// (`static (nint n, error err) Read(this ref …)`) opens one, and a pattern that stops at the first
// parenthesis never sees that method at all (the seat's first census read 624 forwarders for 693).
var (
	byRefReceiverDecl = regexp.MustCompile(`\bstatic\b.*\(this (?:ref|in) `)
	byRefReceiverName = regexp.MustCompile(`([\pL_@][\pL\pN_@]*)\s*(?:<[^>(]*>)?\s*\(this (?:ref|in) `)
	manualConversion  = regexp.MustCompile(`(?m)^\s*\[module:\s*(?:go\.)?GoManualConversion`)
)

// isHandWritten reports whether a committed file under src/core is hand-owned rather than converted:
// golib, unsafe and the hand-owned testing package, an `*_impl.cs` companion, or a whole-file
// `[module: GoManualConversion]` replacement.
func isHandWritten(rel string, data []byte) bool {
	first, rest, _ := strings.Cut(rel, "/")

	switch {
	case first == "golib" || first == "unsafe":
		return true
	case first == "testing" && !strings.Contains(rest, "/"):
		return true
	case strings.HasSuffix(rel, "_impl.cs"):
		return true
	}

	return manualConversion.Match(data)
}

// TestByRefReceiversFollowTheReceiverRule reads the COMMITTED corpus and the behavioral suite and holds
// face lift A's receiver rule, in the halves COORD ruled (2026-10-06):
//   - CONVERTED PRODUCTION code and the BEHAVIORAL sources and goldens (`.cs`, `.cs.target`) carry no
//     [GoRecv] and no [GoCopyBound]: a Go pointer receiver is `this ref T` and nothing more (the
//     converter's own rendering is pinned by TestPointerReceiversAreEmittedByRefWithNoMark);
//   - CONVERTED TEST sources (`_test.cs`) are the windows emission of record, refreshed once at a train's
//     landing, so until that refresh each FILE is all or none: every by-ref receiver marked (the old
//     emission) or none (the new one), never a mixture, which a hand edit would make. The first seat
//     after the landing tightens this half to none;
//   - a HAND-WRITTEN `this ref` / `this in` receiver carries [GoRecv] or [GoCopyBound], on its own line
//     or on the attribute line directly above, unless it is one of the allowlisted helpers.
//
// The generated promoted-method forwarders are not committed; their rule is the generator's
// (TypeGenerator marks a `this ref` forwarder [GoCopyBound] exactly when its embed path holds a pointer
// hop), proved by the PromotedPtrMethodValueSet behavioral test and golib's CopyBoundReceiverTests.
func TestByRefReceiversFollowTheReceiverRule(t *testing.T) {
	coreDir := filepath.Join("..", "core")
	behavioralDir := filepath.Join("..", "tests", "Behavioral")

	found := map[string][]string{}
	var unmarkedRequired, mixedTests []string
	handMarked, productionByRef, scanned, testsOld, testsNew := 0, 0, 0, 0, 0

	marked := func(text string) bool {
		return strings.Contains(text, "GoRecv") || strings.Contains(text, "GoCopyBound")
	}

	walk := func(root string, behavioral bool) error {
		return filepath.WalkDir(root, func(filePath string, entry fs.DirEntry, walkErr error) error {
			if walkErr != nil {
				return walkErr
			}

			if entry.IsDir() {
				name := entry.Name()

				if name == "bin" || name == "obj" || name == ".vs" || strings.HasPrefix(name, "Generated") {
					return fs.SkipDir
				}

				return nil
			}

			if !strings.HasSuffix(entry.Name(), ".cs") && !(behavioral && strings.HasSuffix(entry.Name(), ".cs.target")) {
				return nil
			}

			data, readErr := os.ReadFile(filePath)

			if readErr != nil {
				return readErr
			}

			scanned++

			rel, _ := filepath.Rel(root, filePath)
			rel = filepath.ToSlash(rel)
			handWritten := !behavioral && isHandWritten(rel, data)
			convertedTest := !behavioral && !handWritten && strings.HasSuffix(rel, "_test.cs")
			fileMarks, fileByRef := 0, 0
			previous := ""

			if behavioral {
				rel = "tests/Behavioral/" + rel
			}

			for index, line := range strings.Split(string(data), "\n") {
				trimmed := strings.TrimSpace(line)

				if strings.HasPrefix(trimmed, "//") {
					continue
				}

				code, _, _ := strings.Cut(line, "//")
				isByRef := byRefReceiverDecl.MatchString(line)

				switch {
				case handWritten:
					if isByRef {
						if marked(line) || (strings.HasPrefix(previous, "[") && marked(previous)) {
							handMarked++
						} else {
							name := "?"

							if match := byRefReceiverName.FindStringSubmatch(line); match != nil {
								name = match[1]
							}

							key := rel + "\t" + name
							found[key] = append(found[key], rel+":"+strconv.Itoa(index+1))
						}
					}
				case convertedTest:
					if isByRef {
						fileByRef++

						if marked(code) {
							fileMarks++
						}
					}
				default:
					if marked(code) {
						unmarkedRequired = append(unmarkedRequired, rel+":"+strconv.Itoa(index+1))
					}

					if isByRef && !behavioral {
						productionByRef++
					}
				}

				if trimmed != "" {
					previous = trimmed
				}
			}

			switch {
			case !convertedTest || fileByRef == 0:
			case fileMarks == 0:
				testsNew++
			case fileMarks == fileByRef:
				testsOld++
			default:
				mixedTests = append(mixedTests, rel+" ("+strconv.Itoa(fileMarks)+" of "+strconv.Itoa(fileByRef)+" by-ref receivers marked)")
			}

			return nil
		})
	}

	if err := walk(coreDir, false); err != nil {
		t.Fatal(err)
	}

	if err := walk(behavioralDir, true); err != nil {
		t.Fatal(err)
	}

	// A guard that read nothing would pass on an empty corpus: the converted production pointer receivers
	// are in the thousands and the hand-written marked ones in the hundreds, and every allowlisted helper
	// must still be where the list says it is.
	if scanned < 5000 || productionByRef < 4000 || handMarked < 100 {
		t.Fatalf("the guard read %d files, %d converted production by-ref receivers and %d marked hand-written ones: too few to be the corpus", scanned, productionByRef, handMarked)
	}

	for _, site := range unmarkedRequired {
		t.Errorf("converted production or behavioral code carries a receiver mark: %s -- a Go pointer receiver is emitted `this ref T` unmarked; re-convert", site)
	}

	for _, file := range mixedTests {
		t.Errorf("converted test source mixes old and new receiver emission: %s -- a file is the old emission or the new one, never a mixture (a hand edit?)", file)
	}

	var keys []string

	for key := range found {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	for _, key := range keys {
		helper, allowed := byRefReceiverHelpers[key]

		if !allowed {
			t.Errorf("hand-written by-ref receiver with neither [GoRecv] nor [GoCopyBound], not an allowlisted helper: %s -- golib reads it as a POINTER-receiver method; say which it is",
				strings.Join(found[key], ", "))
			continue
		}

		if len(found[key]) != helper.count {
			t.Errorf("allowlisted helper %q: %d by-ref declarations, the allowlist says %d (%s)", key, len(found[key]), helper.count, strings.Join(found[key], ", "))
		}
	}

	for key, helper := range byRefReceiverHelpers {
		if _, present := found[key]; !present {
			t.Errorf("allowlisted helper %q (%s) was not found: the allowlist is stale", key, helper.class)
		}
	}

	t.Logf("scanned %d files: %d converted production by-ref receivers, %d marks where none may stand; test sources %d old, %d new, %d mixed; %d marked hand-written by-ref receivers, %d allowlisted helper names",
		scanned, productionByRef, len(unmarkedRequired), testsOld, testsNew, len(mixedTests), handMarked, len(found))
}
