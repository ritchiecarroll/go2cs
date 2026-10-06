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

// TestByRefReceiversFollowTheReceiverRule reads the COMMITTED corpus (production, test and hand-owned
// files alike) and holds both halves of face lift A's receiver rule:
//   - CONVERTED code carries no [GoRecv] and no [GoCopyBound]: a Go pointer receiver is `this ref T`
//     and nothing more (the converter's own rendering is pinned by
//     TestPointerReceiversAreEmittedByRefWithNoMark);
//   - a HAND-WRITTEN `this ref` / `this in` receiver carries [GoRecv] or [GoCopyBound], on its own line
//     or on the attribute line directly above, unless it is one of the allowlisted helpers.
//
// The generated promoted-method forwarders are not committed; their rule is the generator's
// (TypeGenerator marks a `this ref` forwarder [GoCopyBound] exactly when its embed path holds a pointer
// hop), proved by the PromotedPtrMethodValueSet behavioral test and golib's CopyBoundReceiverTests.
func TestByRefReceiversFollowTheReceiverRule(t *testing.T) {
	coreDir := filepath.Join("..", "core")

	found := map[string][]string{}
	var convertedMarks []string
	handMarked, convertedByRef, scanned := 0, 0, 0

	err := filepath.WalkDir(coreDir, func(filePath string, entry fs.DirEntry, walkErr error) error {
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

		if !strings.HasSuffix(entry.Name(), ".cs") {
			return nil
		}

		data, readErr := os.ReadFile(filePath)

		if readErr != nil {
			return readErr
		}

		scanned++

		rel, _ := filepath.Rel(coreDir, filePath)
		rel = filepath.ToSlash(rel)
		handWritten := isHandWritten(rel, data)
		previous := ""

		for index, line := range strings.Split(string(data), "\n") {
			trimmed := strings.TrimSpace(line)

			if strings.HasPrefix(trimmed, "//") {
				continue
			}

			code, _, _ := strings.Cut(line, "//")

			if !handWritten {
				if strings.Contains(code, "[GoRecv") || strings.Contains(code, "GoRecv]") || strings.Contains(code, "GoCopyBound") {
					convertedMarks = append(convertedMarks, rel+":"+strconv.Itoa(index+1))
				}

				if byRefReceiverDecl.MatchString(line) {
					convertedByRef++
				}
			} else if byRefReceiverDecl.MatchString(line) {
				marked := func(text string) bool {
					return strings.Contains(text, "GoRecv") || strings.Contains(text, "GoCopyBound")
				}

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

			if trimmed != "" {
				previous = trimmed
			}
		}

		return nil
	})

	if err != nil {
		t.Fatal(err)
	}

	// A guard that read nothing would pass on an empty corpus: the converted pointer receivers are in
	// the thousands and the hand-written [GoRecv] receivers in the hundreds, and every allowlisted
	// helper must still be where the list says it is.
	if scanned < 4000 || convertedByRef < 4000 || handMarked < 100 {
		t.Fatalf("the guard read %d files, %d converted by-ref receivers and %d marked hand-written ones: too few to be the corpus", scanned, convertedByRef, handMarked)
	}

	for _, site := range convertedMarks {
		t.Errorf("converted code carries a receiver mark: %s -- a Go pointer receiver is emitted `this ref T` unmarked; re-convert the package", site)
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

	t.Logf("scanned %d files: %d converted by-ref receivers, %d marked hand-written by-ref receivers, %d converted marks, %d allowlisted helper names",
		scanned, convertedByRef, handMarked, len(convertedMarks), len(found))
}
