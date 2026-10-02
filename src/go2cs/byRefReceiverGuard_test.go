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

// byRefReceiverHelpers is the ALLOWLIST of committed extension methods that take their receiver by
// reference WITHOUT [GoRecv] and are not Go methods at all. golib binds any other such method as a Go
// VALUE-receiver method through a copy of its receiver (TypeExtensions.IsCopyBoundReceiver), so a
// by-ref receiver that is really a Go pointer-receiver method must say so with [GoRecv], or reflect
// counts a method Go does not have and calls it on a copy. Each entry is tagged with the class that
// keeps it out of every Go method set: its receiver is no Go type, or its name is not exported.
var byRefReceiverHelpers = map[string]struct {
	count int
	class string
}{
	"golib/builtin.cs\tToUTF8Bytes":        {1, "span receiver (ReadOnlySpan<rune> is a ref struct, never a Go dynamic type)"},
	"golib/slice.cs\tslice":                {4, "golib slicing helper (`in slice<T>`; the name is unexported, so no method table lists it)"},
	"runtime/arena_impl.cs\tuserArenaKeep": {1, "hand-owned runtime helper with no Go counterpart (private, unexported)"},
}

// The declaration test reads PAST any parenthesis before the receiver: a tuple return type
// (`static (nint n, error err) Read(this ref …)`) opens one, and a pattern that stops at the first
// parenthesis never sees that method at all (the seat's first census read 624 forwarders for 693).
var (
	byRefReceiverDecl = regexp.MustCompile(`\bstatic\b.*\(this (?:ref|in) `)
	byRefReceiverName = regexp.MustCompile(`([\pL_@][\pL\pN_@]*)\s*(?:<[^>(]*>)?\s*\(this (?:ref|in) `)
)

// TestByRefReceiversCarryGoRecv reads the COMMITTED corpus (production, test and hand-owned files alike)
// and requires every `this ref` / `this in` extension receiver to carry [GoRecv], on its own line or on
// the attribute line directly above, unless it is one of the allowlisted helpers. The generated
// promoted-method forwarders are not committed; their rule is the generator's (TypeGenerator stamps
// [GoRecv] on a `this ref` forwarder exactly when its embed path holds no pointer hop), proved by the
// PromotedPtrMethodValueSet behavioral test and golib's CopyBoundReceiverTests.
func TestByRefReceiversCarryGoRecv(t *testing.T) {
	coreDir := filepath.Join("..", "core")

	found := map[string][]string{}
	marked, scanned := 0, 0

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
		previous := ""

		for index, line := range strings.Split(string(data), "\n") {
			trimmed := strings.TrimSpace(line)

			if strings.HasPrefix(trimmed, "//") {
				continue
			}

			if byRefReceiverDecl.MatchString(line) {
				if strings.Contains(line, "GoRecv") || (strings.HasPrefix(previous, "[") && strings.Contains(previous, "GoRecv")) {
					marked++
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

	// A guard that read nothing would pass on an empty corpus: the [GoRecv] population is in the thousands,
	// and every allowlisted helper must still be where the list says it is.
	if scanned < 4000 || marked < 4000 {
		t.Fatalf("the guard read %d files and %d [GoRecv] by-ref receivers: too few to be the corpus", scanned, marked)
	}

	var keys []string

	for key := range found {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	for _, key := range keys {
		helper, allowed := byRefReceiverHelpers[key]

		if !allowed {
			t.Errorf("by-ref receiver without [GoRecv], not an allowlisted helper: %s -- a Go pointer-receiver method must carry [GoRecv]; golib would otherwise bind it through a COPY as a value-receiver method",
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

	t.Logf("scanned %d files: %d by-ref receivers carry [GoRecv], %d allowlisted helper names", scanned, marked, len(found))
}
