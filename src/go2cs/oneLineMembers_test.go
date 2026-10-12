// oneLineMembers_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards adjacent one-line functions (class K of docs/PLAN-comment-and-spacing-parity.md; the owner's decision 5):
// functions Go writes on one line, one after another, become one-line C# members written one after another — an
// expression body when the C# body is one expression, `{ }` when it is empty. A body C# needs more lines for keeps
// them, and a one-line function Go writes on its own is unchanged. The rule holds in every conversion.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

const oneLineMembersSource = `package main

import "fmt"

type T struct{ n int }

func (t T) Len() int     { return t.n }
func (t *T) Add(k int)   { t.n += k }
func (T) marker()        {}
// Twice doubles the count.
func (t T) Twice() int   { return 2 * t.n }
func (t T) Show() string { return fmt.Sprint(t.n) }

func alone() int { return 1 }

func multi() int { a := alone(); return a + 1 }
func three() int { return 3 }

func main() {
	t := &T{n: 1}
	t.Add(2)
	t.marker()
	fmt.Println(t.Len(), t.Twice(), t.Show(), alone(), multi(), three())
}
`

func checkOneLineMembers(t *testing.T, lines []string) {
	t.Helper()

	for _, c := range []struct{ name, line string }{
		{"Len(", "=> t.n;"},
		{"Add(", "=> t.n += k;"},
		{"marker(", "{ }"},
		{"Twice(", "=> 2 * t.n;"},
		{"Show(", "=> fmt.Sprint(t.n);"},
		{"nint three()", "=> 3;"},
	} {
		at := lineContaining(t, lines, " "+c.name)

		if !strings.HasSuffix(strings.TrimSpace(lines[at]), c.line) {
			t.Errorf("%s is not one line ending %q: %q", c.name, c.line, lines[at])
		}
	}

	// Adjacent in Go, adjacent in C#; a doc comment stays directly above its member.
	for _, name := range []string{" Add(", " marker(", "// Twice doubles the count."} {
		if blankAbove(t, lines, name) {
			t.Errorf("a blank line above %q, which Go writes adjacently", name)
		}
	}

	if at := lineContaining(t, lines, " Twice("); !strings.Contains(lines[at-1], "// Twice doubles the count.") {
		t.Errorf("the doc comment is not directly above Twice: %q", lines[at-1])
	}

	// A one-line function Go writes on its own keeps its three lines.
	if at := lineContaining(t, lines, "nint alone()"); !strings.HasSuffix(lines[at], "{") || !blankAbove(t, lines, "nint alone()") {
		t.Errorf("alone() changed: %q", lines[at])
	}

	// A body C# needs more lines for keeps them, and the blank lines either side of it.
	if at := lineContaining(t, lines, "nint multi()"); !strings.HasSuffix(lines[at], "{") {
		t.Errorf("multi() collapsed: %q", lines[at])
	}

	if !blankAbove(t, lines, "nint three()") {
		t.Errorf("no blank line between multi()'s body and three()")
	}
}

func TestAdjacentOneLineMembersUnderComments(t *testing.T) {
	checkOneLineMembers(t, convertWithComments(t, oneLineMembersSource))
}

func TestAdjacentOneLineMembersWithoutComments(t *testing.T) {
	lines := convertWithoutComments(t, oneLineMembersSource)

	for _, c := range []struct{ name, line string }{
		{" Len(", "=> t.n;"},
		{" marker(", "{ }"},
		{" Twice(", "=> 2 * t.n;"},
	} {
		if at := lineContaining(t, lines, c.name); !strings.HasSuffix(strings.TrimSpace(lines[at]), c.line) {
			t.Errorf("%s is not one line ending %q without -comments: %q", c.name, c.line, lines[at])
		}
	}

	if blankAbove(t, lines, " Add(") {
		t.Errorf("a blank line above Add() without -comments")
	}
}

const oneLineMembersPositionSource = `package main

import "runtime"

func callerLine() int {
	_, _, line, _ := runtime.Caller(1)
	return line
}

func siteA() int { return callerLine() }
func siteB() int { return callerLine() }
func siteC()     {}
func siteD() int { return siteB() }

func main() {
	println(siteA(), siteB(), siteD())
	siteC()
}
`

// A one-line member keeps its Go line in the position map: runtime.Caller from inside siteA answers siteA's Go line
// and from inside siteB answers siteB's (Behavioral/RuntimeCallerFrames, `distinct call sites`). The statement's
// sentinel is written after the header's `{`, where it marks the NEXT line, the body's; once the body is on the
// member's line, the next line is the next member, and a trailing sentinel there gave it the line above.
func TestOneLineMembersKeepTheirGoLines(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/onelinepos\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), oneLineMembersPositionSource)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "onelinepos")
	mainCs := readGenerated(t, filepath.Join(outDir, "main.cs"))
	info := readGenerated(t, filepath.Join(outDir, "package_info.cs"))

	record := regexp.MustCompile(`GoPositionMap\("[^"]*", "main\.cs", "([A-Za-z0-9+/=]*)"\)`).FindStringSubmatch(info)

	if record == nil {
		t.Fatalf("package_info.cs carries no GoPositionMap record for main.cs:\n%s", info)
	}

	table := decodePositionTable(t, record[1])

	for _, c := range []struct {
		fragment string
		goLine   int
	}{
		{" siteA() =>", 10},
		{" siteB() =>", 11},
		{" siteC() { }", 12},
		{" siteD() =>", 13},
		{"println(siteA(), siteB(), siteD());", 16},
	} {
		if got := answeringGoLine(table, csLineContaining(t, mainCs, c.fragment)); got != c.goLine {
			t.Errorf("%q answers Go line %d, want %d", c.fragment, got, c.goLine)
		}
	}
}
