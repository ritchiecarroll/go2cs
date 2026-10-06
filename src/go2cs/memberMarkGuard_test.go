// memberMarkGuard_test.go - Gbtc
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
	"strconv"
	"strings"
	"testing"
)

// memberMarkKinds are the member facts converted code states as a comment rather than an attribute
// (docs/PLAN-marker-comment-parity.md, sections 5.4-5.6), each by its old attribute and its new comment.
var memberMarkKinds = []struct{ attribute, comment string }{
	{"[GoEmbedded]", "/*embed*/"},
}

// TestMemberMarksFollowTheCommentRule reads the COMMITTED corpus and the behavioral suite and holds each
// member mark in the halves COORD ruled for the face lift (2026-10-06): converted PRODUCTION code and the
// BEHAVIORAL sources and goldens carry the comment and never the attribute; a converted TEST source is the
// windows emission of record, refreshed at a train's landing, so until then each file carries one spelling
// or the other, never both. Hand-written files keep the attribute and are not read.
func TestMemberMarksFollowTheCommentRule(t *testing.T) {
	coreDir := filepath.Join("..", "core")
	behavioralDir := filepath.Join("..", "tests", "Behavioral")

	var attributeSites, mixedTests []string
	scanned := 0
	comments := map[string]int{}

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

			rel, _ := filepath.Rel(root, filePath)
			rel = filepath.ToSlash(rel)

			if !behavioral && isHandWritten(rel, data) {
				return nil
			}

			scanned++

			if behavioral {
				rel = "tests/Behavioral/" + rel
			}

			convertedTest := !behavioral && strings.HasSuffix(rel, "_test.cs")

			for _, kind := range memberMarkKinds {
				attributes, marks := 0, 0

				for index, line := range strings.Split(string(data), "\n") {
					if strings.HasPrefix(strings.TrimSpace(line), "//") {
						continue
					}

					if strings.Contains(line, kind.attribute) {
						attributes++

						if !convertedTest {
							attributeSites = append(attributeSites, rel+":"+strconv.Itoa(index+1)+" "+kind.attribute)
						}
					}

					marks += strings.Count(line, kind.comment)
				}

				comments[kind.comment] += marks

				if convertedTest && attributes > 0 && marks > 0 {
					mixedTests = append(mixedTests, rel+" ("+kind.attribute+" "+strconv.Itoa(attributes)+", "+kind.comment+" "+strconv.Itoa(marks)+")")
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
		t.Fatalf("the guard read %d converted files: too few to be the corpus", scanned)
	}

	for _, site := range attributeSites {
		t.Errorf("converted production or behavioral code carries a member attribute where the comment belongs: %s -- re-convert", site)
	}

	for _, file := range mixedTests {
		t.Errorf("converted test source mixes the old and new member mark: %s -- a file is one emission or the other (a hand edit?)", file)
	}

	t.Logf("read %d converted files: %d attribute sites where none may stand, %d mixed test sources, comments %v", scanned, len(attributeSites), len(mixedTests), comments)
}
