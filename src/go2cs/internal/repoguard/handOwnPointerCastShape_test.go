// handOwnPointerCastShape_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// WHAT THIS WATCHES (docs/phase4/DESIGN-interior-pointer-root.md 4.1 and 10.1; R, 2026-11-15).
//
// Go's `(*T)(p)` over an unsafe.Pointer p converts to `@unsafe.Pointer.To<T>(p)`, which keeps what p carried. The
// converter's earlier spelling, `(ж<T>)(uintptr)(p)`, turned p into a number first, so the typed pointer kept nothing
// of the box p was minted from: a GC-safety hole batch 305 closed in converted code. A HAND-OWNED file is never
// regenerated, so it keeps whatever spelling it was written in. Batch 305 left nine such lines behind in three of them,
// found by refreshing their .cs.auto siblings, and a census found 35 in ten (all respelled with this guard).
//
// The rule: no hand-owned file under src/core holds the parenthesised number cast `(ж<T>)(uintptr)(` in CODE. Write
// `@unsafe.Pointer.To<T>(p)` over an unsafe.Pointer, and `@unsafe.Pointer.To<T>((uintptr)x)` (the number conversion,
// unchanged) over a native address or a number. A line that must keep the old spelling goes in
// handOwnPointerCastAllowed with the reason it is safe; a row that no longer matches a line fails, so the list only
// shrinks.
//
// Hand-owned is interiorPointerIsHandOwned's answer (either marker spelling, or an `_impl` companion); the cast is
// counted by numberCastCount, P5's arm-3 scanner, over each line with its comment and string literals removed. P5's
// HAND-OWNED CASTS table (interiorPointerMintCensus_test.go) ratchets both cast forms per file, prose included; this
// guard is the absolute rule for the parenthesised form in code.

// handOwnPointerCastAllowed is "path under src/core: the line, trimmed" -> why it is safe. Empty: every site was
// respelled when the guard landed.
var handOwnPointerCastAllowed = map[string]string{}

// handOwnBlockComment matches a /* */ comment that opens and closes on one line.
var handOwnBlockComment = regexp.MustCompile(`/\*.*?\*/`)

// handOwnCodeOf is a line with its string literals and comments removed (a block comment spanning lines is tracked by
// the caller through inBlock).
func handOwnCodeOf(line string, inBlock *bool) string {
	if *inBlock {
		end := strings.Index(line, "*/")

		if end < 0 {
			return ""
		}

		*inBlock = false
		line = line[end+2:]
	}

	line = mintStringLiteral.ReplaceAllString(line, `""`)
	line = handOwnBlockComment.ReplaceAllString(line, "")

	if at := strings.Index(line, "//"); at >= 0 {
		line = line[:at]
	}

	if at := strings.Index(line, "/*"); at >= 0 {
		*inBlock = true
		line = line[:at]
	}

	return line
}

// handOwnPointerCastFindings scans the files given (path under src/core -> text) and returns one finding per
// offending line, one per allow-list row no line answers, and the number of hand-owned files scanned.
func handOwnPointerCastFindings(files map[string]string, allowed map[string]string) (findings []string, scanned int) {
	answered := map[string]bool{}

	for _, rel := range sortedKeys(files) {
		text := files[rel]

		if !interiorPointerIsHandOwned(rel, text) {
			continue
		}

		scanned++
		inBlock := false

		for i, line := range strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n") {
			if parenthesised, _ := numberCastCount(handOwnCodeOf(line, &inBlock)); parenthesised == 0 {
				continue
			}

			key := rel + ": " + strings.TrimSpace(line)

			if _, ok := allowed[key]; ok {
				answered[key] = true
				continue
			}

			findings = append(findings, fmt.Sprintf("HAND-OWNED NUMBER CAST: src/core/%s:%d holds `(ж<T>)(uintptr)(`, which drops what an "+
				"unsafe.Pointer carried. Write @unsafe.Pointer.To<T>(p) over an unsafe.Pointer, or @unsafe.Pointer.To<T>((uintptr)x) "+
				"over a native address or number; or add the line to handOwnPointerCastAllowed with why it is safe: %s",
				rel, i+1, strings.TrimSpace(line)))
		}
	}

	for _, key := range sortedKeys(allowed) {
		if !answered[key] {
			findings = append(findings, fmt.Sprintf("handOwnPointerCastAllowed row answers no line: remove it: %s", key))
		}
	}

	return findings, scanned
}

// TestHandOwnPointerCastShape is the guard.
func TestHandOwnPointerCastShape(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	files := map[string]string{}

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") || strings.HasPrefix(rel, "src/core/golib/") {
			continue
		}

		raw, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatal(err)
		}

		files[strings.TrimPrefix(rel, "src/core/")] = string(raw)
	}

	findings, scanned := handOwnPointerCastFindings(files, handOwnPointerCastAllowed)

	// Liveness: the population is the corpus's hand-owned files, a few hundred; far fewer means the predicate or the
	// walk stopped seeing them, and an empty scan passes everything.
	if scanned < 200 {
		t.Fatalf("scanned %d hand-owned files, too few: the hand-own predicate or the walk is not reaching them", scanned)
	}

	for _, finding := range findings {
		t.Error(finding)
	}
}

// TestHandOwnPointerCastShapeFires is the control: the same scanner over planted files, one axis per arm.
func TestHandOwnPointerCastShapeFires(t *testing.T) {
	const qualified = "[module: go.GoManualConversion]\n"
	const bare = "[module: GoManualConversion]\n"
	const cast = "        var a = (ж<RawSockaddrInet4>)(uintptr)(addr);\n"

	files := map[string]string{
		"a/qualified.cs":       qualified + cast,                                                             // fires
		"a/bare.cs":            bare + cast,                                                                  // fires
		"a/companion_impl.cs":  cast,                                                                         // fires: an _impl companion
		"a/converted.cs":       cast,                                                                         // silent: not hand-owned
		"a/comment.cs":         qualified + "        // was " + strings.TrimSpace(cast) + "\n",               // silent: a comment
		"a/block.cs":           qualified + "        /* was\n" + cast + "        */\n",                       // silent: a block comment
		"a/literal.cs":         qualified + "        Log(\"(ж<T>)(uintptr)(p)\");\n",                         // silent: a string
		"a/respelled.cs":       qualified + "        var a = @unsafe.Pointer.To<RawSockaddrInet4>(addr);\n",  // silent
		"a/number.cs":          qualified + "        var a = @unsafe.Pointer.To<byte>((uintptr)(void*)p);\n", // silent
		"a/allowed.cs":         qualified + cast,                                                             // silent: allow-listed
		"a/unparenthesised.cs": qualified + "        var a = (ж<byte>)(uintptr)token;\n",                     // silent: P5's ratchet owns it
	}

	allowed := map[string]string{
		"a/allowed.cs: " + strings.TrimSpace(cast): "planted",
		"a/gone.cs: " + strings.TrimSpace(cast):    "planted stale row",
	}

	findings, scanned := handOwnPointerCastFindings(files, allowed)
	joined := strings.Join(findings, "\n")

	for _, want := range []string{"src/core/a/qualified.cs:2", "src/core/a/bare.cs:2", "src/core/a/companion_impl.cs:1", "remove it: a/gone.cs"} {
		if !strings.Contains(joined, want) {
			t.Errorf("no finding containing %q; got:\n%s", want, joined)
		}
	}

	for _, quiet := range []string{"converted.cs", "comment.cs", "block.cs", "literal.cs", "respelled.cs", "number.cs", "a/allowed.cs:", "unparenthesised.cs"} {
		if strings.Contains(joined, quiet) {
			t.Errorf("a finding names %q, which must be silent; got:\n%s", quiet, joined)
		}
	}

	if len(findings) != 4 {
		t.Errorf("%d findings, want 4 (three planted casts and one stale row); got:\n%s", len(findings), joined)
	}

	// converted.cs is the one file not scanned.
	if scanned != len(files)-1 {
		t.Errorf("scanned %d planted files, want %d", scanned, len(files)-1)
	}
}
