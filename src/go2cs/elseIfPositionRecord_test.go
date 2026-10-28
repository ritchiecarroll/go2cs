// elseIfPositionRecord_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// An `else if` is reached from visitIfStmt directly, never through visitStmt, so no position sentinel
// was written for it. Its emission splits over two C# lines -- `} else ` and `if (cond) {` -- and with
// no record on the second, the condition's C# line answered the PREVIOUS record's Go line: a panic in
// an else-if condition reported the line before it. Measured in the IDE-mode spike (SPIKE-ide-mode.md,
// E9): 17 of strconv's 1,730 statement lines. The record lives in package_info's GoPositionMap only.
//
// Seat #19 (visual-parity review E1) then joined the two lines, as Go writes them: `} else if (cond) {`
// is one C# line, and the sentinel before the `if` now marks that line rather than the next. The
// record keeps answering the else-if's own Go line, which in Go is the `} else` line too.

const elseIfPositionFixture = `package main

func classify(x int) int {
	if x == 1 {
		return 10
	} else if x == 2 {
		return 20
	} else if y := x * 2; y == 6 {
		return 30
	} else {
		return 0
	}
}

func main() {
	println(classify(1), classify(2), classify(3), classify(4))
}
`

func convertElseIfPositionFixture(t *testing.T) (mainCs string, table []positionEntry) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/elseifpos\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), elseIfPositionFixture)

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

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "elseifpos")
	mainCs = readGenerated(t, filepath.Join(outDir, "main.cs"))
	info := readGenerated(t, filepath.Join(outDir, "package_info.cs"))

	record := regexp.MustCompile(`GoPositionMap\("[^"]*", "main\.cs", "([A-Za-z0-9+/=]*)"\)`).FindStringSubmatch(info)

	if record == nil {
		t.Fatalf("package_info.cs carries no GoPositionMap record for main.cs:\n%s", info)
	}

	return mainCs, decodePositionTable(t, record[1])
}

// answeringGoLine is the runtime's predecessor search: a C# line answers the Go line of the last
// record at or before it.
func answeringGoLine(table []positionEntry, csLine int) int {
	goLine := 0

	for _, entry := range table {
		if entry.csLine > csLine {
			break
		}

		goLine = entry.goLine
	}

	return goLine
}

func csLineContaining(t *testing.T, mainCs, fragment string) int {
	t.Helper()

	for index, line := range strings.Split(mainCs, "\n") {
		if strings.Contains(line, fragment) {
			return index + 1
		}
	}

	t.Fatalf("no emitted line contains %q:\n%s", fragment, mainCs)
	return 0
}

func TestElseIfConditionAnswersItsOwnGoLine(t *testing.T) {
	isolateConverterPathGlobals(t)
	mainCs, table := convertElseIfPositionFixture(t)

	cases := []struct {
		fragment string
		goLine   int
	}{
		{"if (x == 1)", 4}, // control: the leading if, recorded through visitStmt
		{"if (x == 2)", 6}, // else if
		{"y == 6", 8},      // else if with an init clause: the condition's line answers the else-if line
		{"return 0;", 11},  // control: a statement inside the final else block
	}

	for _, c := range cases {
		csLine := csLineContaining(t, mainCs, c.fragment)

		if got := answeringGoLine(table, csLine); got != c.goLine {
			t.Errorf("C# line %d (%q) answers Go line %d, want %d\ntable: %+v", csLine, c.fragment, got, c.goLine, table)
		}
	}
}

// The else-if chain reads as Go writes it: `} else if (cond) {` on one line, an else-if with an init
// clause opening its scoping block on the `} else {` line, and a space before every `{` that follows a
// condition (an `if` followed by an `else` lost it to the body's trim).
func TestElseIfChainEmitsOnGoLines(t *testing.T) {
	isolateConverterPathGlobals(t)
	mainCs, _ := convertElseIfPositionFixture(t)

	for _, want := range []string{
		`if (x == 1) {`,
		`} else if (x == 2) {`,
		`nint y = x * 2; if (y == 6) {`,
		`} else {`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("no emitted line carries %q", want)
		}
	}

	// The init clause's scoping block opens on the `} else` line, never on a line of its own.
	if !regexp.MustCompile(`\} else \{\r?\n\s+nint y = x \* 2; if \(y == 6\) \{`).MatchString(mainCs) {
		t.Errorf("the else-if with an init clause does not open its block on the `} else` line")
	}

	for index, line := range strings.Split(mainCs, "\n") {
		line = strings.TrimRight(line, "\r")

		if strings.HasSuffix(line, "else") || strings.HasSuffix(line, "else ") || strings.HasSuffix(line, "){") {
			t.Errorf("C# line %d is split or unspaced: %q", index+1, line)
		}
	}

	if t.Failed() {
		t.Logf("emitted main.cs:\n%s", mainCs)
	}
}
