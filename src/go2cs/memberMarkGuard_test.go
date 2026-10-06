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
	"regexp"
	"strconv"
	"strings"
	"testing"
)

// memberMarkKinds are the member facts converted code states as a comment rather than an attribute
// (docs/PLAN-marker-comment-parity.md, sections 5.4-5.6), each by its old attribute and the text that
// opens its new comment: a tag comment follows a field's `;` or an embed property's `}`, in either of
// Go's spellings; a dims comment precedes a type. stays names the lines on which the attribute is still
// the converter's spelling (section 11's STAYS rows): those are counted, never reported.
var memberMarkKinds = []struct {
	attribute string
	comments  []string
	stays     func(line string) bool
}{
	{"[GoEmbedded]", []string{embedMarker}, nil},
	{"[GoTag(", []string{"; /*`", `; /*"`, "} /*`", `} /*"`}, nil},
	{"[GoArrayDims(", []string{"/*["}, dimsAttributeStays},
}

var (
	dimsFieldLine  = regexp.MustCompile(`^\[GoArrayDims\(`)
	dimsTypeLine   = regexp.MustCompile(`\bpartial (struct|class) `)
	dimsMethodDecl = regexp.MustCompile(`^(public|internal|private|protected)\b[^(=]*\(`)
	leadingAttrs   = regexp.MustCompile(`^(\[[^\]]*\]\s*)+`)
)

// dimsAttributeStays reports whether a [GoArrayDims] on this line is one the converter still writes: a
// lambda's or a local function's parameter, a generic func's (an attribute argument cannot name a type
// parameter), a func type's or an interface member's. A field line, a named type's line and a non-generic
// declaration's parameters carry the comment instead.
func dimsAttributeStays(line string) bool {
	code := strings.TrimSpace(line)

	if !dimsFieldLine.MatchString(code) {
		code = leadingAttrs.ReplaceAllString(code, "")
	}

	switch {
	case dimsFieldLine.MatchString(code), dimsTypeLine.MatchString(code):
		return false
	case strings.Contains(code, " delegate "), strings.Contains(code, "=>"):
		return true
	case dimsMethodDecl.MatchString(code):
		// Generic when the method NAME carries type parameters (`first<T>(`), not a return type (`slice<byte> f(`).
		return strings.HasSuffix(strings.TrimSpace(code[:strings.Index(code, "(")]), ">")
	}

	return true
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
	stays := map[string]int{}

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

					if strings.Contains(line, kind.attribute) && kind.stays != nil && kind.stays(line) {
						stays[kind.attribute] += strings.Count(line, kind.attribute)
					} else if strings.Contains(line, kind.attribute) {
						attributes++

						if !convertedTest {
							attributeSites = append(attributeSites, rel+":"+strconv.Itoa(index+1)+" "+kind.attribute)
						}
					}

					for _, comment := range kind.comments {
						marks += strings.Count(line, comment)
					}
				}

				comments[kind.comments[0]] += marks

				if convertedTest && attributes > 0 && marks > 0 {
					mixedTests = append(mixedTests, rel+" ("+kind.attribute+" "+strconv.Itoa(attributes)+", "+kind.comments[0]+" "+strconv.Itoa(marks)+")")
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

	t.Logf("read %d converted files: %d attribute sites where none may stand, %d mixed test sources, comments %v, attributes that stay %v", scanned, len(attributeSites), len(mixedTests), comments, stays)
}
