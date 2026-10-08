// sstringTwinMarkGuard_test.go - Gbtc
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
	"strconv"
	"strings"
	"testing"
)

// sstringTwinBody matches a method declaration with a parameter typed golib's `sstring` view, in any
// spelling: the shape go2cs-gen's StrGenerator selects as the member of an sstring twin that carries the Go
// body (docs/PLAN-marker-comment-parity.md, 5.7).
var sstringTwinBody = regexp.MustCompile(`\bstatic\b[^=]*[(,]\s*(?:this\s+)?(?:(?:ref|in|scoped)\s+)*(?:global::)?(?:go\.)?sstring\s+[\pL_@]`)

// TestSStringTwinsAreSelectedBySignature reads the COMMITTED corpus and the behavioral suite and holds face
// lift S's twin rule: converted code (production, tests, the behavioral sources and goldens) carries no
// [GoStr], since StrGenerator selects a twin by its sstring parameter; and a HAND-WRITTEN method with an
// sstring parameter in a converted package carries [GoStr], on its own line or on the attribute line
// directly above, because the generator makes it a twin whether it is marked or not and the mark says so.
// golib runs no generator, and a hand-owned package (unsafe, testing) selects twins by [GoStr] alone, so
// neither is read for the second half. The converter's own rendering is pinned by TestSStringTwinEmission.
func TestSStringTwinsAreSelectedBySignature(t *testing.T) {
	coreDir := filepath.Join("..", "core")
	behavioralDir := filepath.Join("..", "tests", "Behavioral")

	var markedConverted, unmarkedHandWritten []string
	scanned, twinBodies, handMarked := 0, 0, 0

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
			first, rest, _ := strings.Cut(rel, "/")
			outsideGenerator := first == "golib" || first == "unsafe" || (first == "testing" && !strings.Contains(rest, "/"))
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
				site := rel + ":" + strconv.Itoa(index+1)

				switch {
				case !handWritten:
					if strings.Contains(code, "[GoStr]") {
						markedConverted = append(markedConverted, site)
					}

					if sstringTwinBody.MatchString(code) {
						twinBodies++
					}
				case !outsideGenerator && sstringTwinBody.MatchString(code):
					if strings.Contains(code, "GoStr") || (strings.HasPrefix(previous, "[") && strings.Contains(previous, "GoStr")) {
						handMarked++
					} else {
						unmarkedHandWritten = append(unmarkedHandWritten, site)
					}
				}

				if trimmed != "" {
					previous = trimmed
				}
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

	if scanned < 5000 {
		t.Fatalf("the guard read %d files: too few to be the corpus", scanned)
	}

	for _, site := range markedConverted {
		t.Errorf("converted code carries [GoStr], which StrGenerator no longer needs: %s -- re-convert", site)
	}

	for _, site := range unmarkedHandWritten {
		t.Errorf("a hand-written method in a converted package takes an sstring parameter with no [GoStr]: %s -- StrGenerator makes it a twin either way; mark it so it says so", site)
	}

	t.Logf("read %d files: %d converted twin bodies, %d still marked [GoStr]; %d hand-written twins marked, %d unmarked", scanned, twinBodies, len(markedConverted), handMarked, len(unmarkedHandWritten))
}
