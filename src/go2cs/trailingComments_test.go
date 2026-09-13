// trailingComments_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the placement of a Go END-OF-LINE comment in `-comments` output.
//
// Go's AST attaches comments to declarations, fields and specs but never to STATEMENTS, so a
// statement's trailing comment is free-floating. Nothing claimed it where it belonged, so it was
// flushed at the next emission point instead — on its own line AFTER the statement, and, when the
// statement was the last of its block, after that block's closing brace. Two Tour of go2cs examples
// showed both halves: `pow[i] = 1 << uint(i) // == 2**i` lost its comment out of the enclosing loop,
// and the pointers example's final `// see the new value of j` landed past `Main`'s closing brace,
// outside the method entirely.
//
// The invariant these tests pin is positional, not textual: a comment written on the same source
// line as a statement's end is emitted on the same line as that statement's LAST emitted line (a Go
// statement can lower to several C# lines), and a comment never leaves the block that contained it.
// Asserting the *shape* of the line rather than the exact converted expression keeps the guard from
// breaking every time an unrelated emission detail changes.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertWithComments converts a single-file package with `-comments` on and returns the emitted
// C# split into lines.
func convertWithComments(t *testing.T, source string) []string {
	t.Helper()

	root := t.TempDir()
	pkgDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(pkgDir, "main.go"), source)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		includeComments:     true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	outDir := filepath.Join(root, "out")

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("conversion failed: %v", err)
	}

	// The converter emits CRLF; drop the CRs so a line's trailing content is its own.
	emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "main.cs")), "\r", "")

	return strings.Split(emitted, "\n")
}

// findCommentLine returns the index of the single line carrying the given comment text, failing if
// the comment vanished or was emitted more than once.
func findCommentLine(t *testing.T, lines []string, comment string) int {
	t.Helper()

	index := -1

	for i, line := range lines {
		if strings.Contains(line, comment) {
			if index >= 0 {
				t.Fatalf("comment %q emitted more than once (lines %d and %d)", comment, index+1, i+1)
			}

			index = i
		}
	}

	if index < 0 {
		t.Fatalf("comment %q was dropped from the conversion:\n%s", comment, strings.Join(lines, "\n"))
	}

	return index
}

// requireTrailing asserts the comment shares its line with emitted code that ENDS the statement —
// the defect emitted the very same text on a line of its own, so "the comment is present" proves
// nothing and only the code before it does.
func requireTrailing(t *testing.T, lines []string, comment string) int {
	t.Helper()

	index := findCommentLine(t, lines, comment)
	code := strings.TrimSpace(lines[index][:strings.Index(lines[index], comment)])

	if code == "" || strings.HasPrefix(code, "//") {
		t.Errorf("comment %q is not trailing any code — emitted as %q", comment, lines[index])
	}

	return index
}

// requireOwnLine is the negative control for requireTrailing: a comment that was on its own line in
// Go must stay on its own line, so a fix for the trailing shape cannot be a blanket "append every
// pending comment to whatever was last written".
func requireOwnLine(t *testing.T, lines []string, comment string) int {
	t.Helper()

	index := findCommentLine(t, lines, comment)

	if code := strings.TrimSpace(lines[index][:strings.Index(lines[index], comment)]); code != "" {
		t.Errorf("leading comment %q was pulled onto a code line — emitted as %q", comment, lines[index])
	}

	return index
}

// TestTrailingCommentStaysOnItsStatementLine is the Tour of go2cs pointers example: every statement
// carries an end-of-line comment, and the last one used to escape `Main` entirely.
func TestTrailingCommentStaysOnItsStatementLine(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	lines := convertWithComments(t, `package main

import "fmt"

func main() {
	i, j := 42, 2701

	p := &i         // point to i
	fmt.Println(*p) // read i through the pointer
	*p = 21         // set i through the pointer
	fmt.Println(i)  // see the new value of i

	p = &j         // point to j
	*p = *p / 37   // divide j through the pointer
	fmt.Println(j) // see the new value of j
}
`)

	for _, comment := range []string{
		"// point to i",
		"// read i through the pointer",
		"// set i through the pointer",
		"// see the new value of i",
		"// point to j",
		"// divide j through the pointer",
	} {
		requireTrailing(t, lines, comment)
	}

	// The final statement's comment is the escaping one: with no further statement to lead, it was
	// flushed after the method had already been closed.
	last := requireTrailing(t, lines, "// see the new value of j")
	closing := -1

	for i := last; i < len(lines); i++ {
		if strings.TrimSpace(lines[i]) == "}" {
			closing = i
			break
		}
	}

	if closing < 0 {
		t.Fatalf("no closing brace found after the final statement:\n%s", strings.Join(lines, "\n"))
	}
}

// TestTrailingCommentOnLastStatementOfBlock is the Tour of go2cs range example: the comment belongs
// to the only statement of a loop body, so the block boundary is what used to strand it — the
// statement emitted, the `}` closed the loop, and the comment followed on its own line OUTSIDE.
func TestTrailingCommentOnLastStatementOfBlock(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	lines := convertWithComments(t, `package main

import "fmt"

func main() {
	pow := make([]int, 10)

	for i := range pow {
		pow[i] = 1 << uint(i) // == 2**i
	}

	for _, value := range pow {
		fmt.Printf("%d\n", value)
	}
}
`)

	index := requireTrailing(t, lines, "// == 2**i")

	// Inside the loop, not after it: the next line closes the loop body.
	if next := strings.TrimSpace(lines[index+1]); next != "}" {
		t.Errorf("loop body does not close immediately after its commented statement — next line is %q", next)
	}

	indent := len(lines[index]) - len(strings.TrimLeft(lines[index], " "))
	closingIndent := len(lines[index+1]) - len(strings.TrimLeft(lines[index+1], " "))

	if indent <= closingIndent {
		t.Errorf("commented statement is not indented inside the loop body (statement indent %d, closing brace indent %d)", indent, closingIndent)
	}
}

// TestTrailingCommentAcrossStatementKinds covers the shapes the two Tour examples do not: a block's
// closing brace as the statement end (`} // …`), a `case` body — a statement list that is not a
// block — and the two forms that must NOT change, a whole-line comment and a multi-line block
// comment that merely opens on a statement's line.
func TestTrailingCommentAcrossStatementKinds(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	lines := convertWithComments(t, `package main

import "fmt"

func main() {
	total := 0 // running total

	// a whole-line comment of its own
	if total > 10 {
		total = 10 // clamp
	} else {
		total = -1 // sentinel
	} // after the if/else

	switch total {
	case 10:
		total++ // bump
	default:
		total = 0 // reset
	} // after the switch

	/* a block comment
	   spanning lines */
	fmt.Println(total) // done
}
`)

	for _, comment := range []string{
		"// running total",
		"// clamp",
		"// sentinel",
		"// after the if/else",
		"// bump",
		"// reset",
		"// after the switch",
		"// done",
	} {
		requireTrailing(t, lines, comment)
	}

	requireOwnLine(t, lines, "// a whole-line comment of its own")

	// A block comment that opens on one line and closes on another cannot be tucked onto a single
	// emitted line; it stays with the standalone path that can indent its continuation.
	requireOwnLine(t, lines, "/* a block comment")
}
