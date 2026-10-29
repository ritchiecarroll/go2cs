// goDeferThunkSites_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards seat #42: Go compiles a go or defer statement's call into a deferwrap/gowrap closure marked as a
// wrapper, which its tracebacks elide; the converter's thunk lambda (`defer(() => where("x"), ref ᒐ)`) is that
// closure, and the runtime elides it only if it can recognise it. The file's GoPositionMap record carries the
// (C# line, column) of each thunk's BODY -- the lambda method's first sequence point -- and nothing else: a
// user func literal, even a Go one-liner sharing a line with a thunk, is a block lambda whose first sequence
// point is the line below its `=> {`, so it is never recorded.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"slices"
	"strconv"
	"strings"
	"testing"
	"unicode/utf16"
)

// thunkBodyPattern finds a converter thunk in emitted text: a go/defer call whose first argument is an
// expression-bodied lambda. Group 1 ends where the body begins.
var thunkBodyPattern = regexp.MustCompile(`((?:defer|goǃ)\((?:\([^()]*\)|ᴛ\d+|_) => )([^{])`)

func TestGoDeferThunkSitesAreRecordedAndLiteralsAreNot(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/gdt\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type T struct{ n int }

func (t T) show(tag string) { fmt.Println(tag, t.n) }

func where(tag string) { fmt.Println(tag) }

func result(tag string) int { fmt.Println(tag); return 1 }

func shapes(s string) {
	t := T{1}
	defer where("constant")
	defer where(s)
	defer t.show("value receiver")
	defer result("dropped result")
	defer func() { where("one-liner") }()
	defer where("shares"); defer func() { where("shares, literal") }()
	go where("go constant")
	go func() { where("go literal") }()
}

func main() { shapes("v") }
`)

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

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "gdt")
	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "main.cs")), "\r\n", "\n")
	infoCs := strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "package_info.cs")), "\r\n", "\n")

	// The thunks the emission holds, at the position of each body, as the PDB counts it: 1-based line and
	// 1-based UTF-16 column.
	var want []string
	lines := strings.Split(mainCs, "\n")

	for index, line := range lines {
		for _, match := range thunkBodyPattern.FindAllStringSubmatchIndex(line, -1) {
			want = append(want, strconv.Itoa(index+1)+":"+strconv.Itoa(len(utf16.Encode([]rune(line[:match[3]])))+1))
		}
	}

	// Control: the fixture's five thunks, so a pattern that matched nothing cannot pass.
	if len(want) != 5 {
		t.Fatalf("control: want 5 thunks in the emission (constant, value receiver, dropped result, shares, go constant), found %d:\n%s", len(want), mainCs)
	}

	var record string

	for _, line := range strings.Split(infoCs, "\n") {
		if strings.Contains(line, "GoPositionMap(") && strings.Contains(line, `"main.cs"`) {
			record = line
		}
	}

	if record == "" {
		t.Fatalf("no GoPositionMap record for main.cs:\n%s", infoCs)
	}

	args := regexp.MustCompile(`"((?:[^"\\]|\\.)*)"`).FindAllStringSubmatch(record, -1)

	if len(args) < 7 {
		t.Fatalf("the record carries no thunk sites (want a seventh argument %q):\n%s", strings.Join(want, ","), record)
	}

	got := strings.Split(args[6][1], ",")

	if !slices.Equal(got, want) {
		t.Errorf("thunk sites: got %q, want %q:\n%s", got, want, mainCs)
	}

	// Every recorded site begins a thunk's body; a block literal's first statement (the one-liners' `where`)
	// is never one.
	for _, site := range got {
		lineText, columnText, _ := strings.Cut(site, ":")
		lineNumber, _ := strconv.Atoi(lineText)
		column, _ := strconv.Atoi(columnText)

		if lineNumber < 1 || lineNumber > len(lines) {
			t.Errorf("site %s is outside main.cs", site)
			continue
		}

		prefix := string(utf16.Decode(utf16.Encode([]rune(lines[lineNumber-1]))[:column-1]))

		if !strings.HasSuffix(prefix, " => ") || strings.Contains(lines[lineNumber-1], "=> {") && !thunkBodyPattern.MatchString(lines[lineNumber-1]) {
			t.Errorf("site %s does not begin a thunk's body: %q", site, lines[lineNumber-1])
		}
	}
}
