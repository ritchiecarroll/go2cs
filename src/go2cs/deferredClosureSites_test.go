// deferredClosureSites_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards B1, the deferred-closure marker: a panic a Go func literal handed straight to defer raises is first caught
// by the deferring function's GoFrame.Run, exactly as one a converter thunk raises, and Go prints that closure's
// frame (pkg.f.funcN) over runtime.gopanic where it elides a thunk. The runtime tells the two apart by the file's
// GoPositionMap record: its EIGHTH argument lists, per deferred closure, the (C# line, column) of the lambda's first
// sequence point -- the block's `{` in a Debug build and its first statement in a Release build, so both -- and
// nothing else: not a nested literal, not a literal deferred through a variable, not a go statement's literal, not
// a thunk, and not the literal the variadic/argument form invokes from inside its thunk.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strconv"
	"strings"
	"testing"
	"unicode/utf16"
)

func TestDeferredClosureSitesAreRecordedAndNothingElse(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/dcs\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

func where(tag string) { fmt.Println(tag) }

func shapes() {
	defer func() { where("one-liner") }()
	defer func() {
		// a comment line ahead of the first statement

		where("block")
		inner := func() { where("nested") }
		inner()
	}()
	defer (func() { where("parenthesized") })()
	defer func() {}()
	defer func(tag string) { where(tag) }("argument")
	defer func(tags ...string) { where(tags[0]) }("variadic", "form")
	f := func() { where("through a variable") }
	defer f()
	go func() { where("go literal") }()
	defer where("thunk")
}

func main() { shapes() }
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

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "dcs")
	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "main.cs")), "\r\n", "\n")
	infoCs := strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "package_info.cs")), "\r\n", "\n")
	lines := strings.Split(mainCs, "\n")

	// Control: the variadic form's literal is invoked from inside a thunk, which is then the deferred function, so the
	// literal is not one the record lists. Pinned, so a change that passed the literal to defer directly fails here.
	if !strings.Contains(mainCs, "defer((ᴛ1, ᴛ2) => ((Actionꓸꓸꓸ<@string>)((params ꓸꓸꓸstring tagsʗp) => {") {
		t.Fatalf("control: the variadic form no longer emits its thunk-invoked literal:\n%s", mainCs)
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

	if len(args) < 8 {
		t.Fatalf("the record carries no deferred-closure sites (want an eighth argument):\n%s\n%s", record, mainCs)
	}

	// Each recorded site, read as the text the emission holds from that position on: what the PDB's first sequence
	// point of the lambda covers.
	var got []string

	for _, site := range strings.Split(args[7][1], ",") {
		lineText, columnText, _ := strings.Cut(site, ":")
		lineNumber, _ := strconv.Atoi(lineText)
		column, _ := strconv.Atoi(columnText)

		if lineNumber < 1 || lineNumber > len(lines) {
			t.Fatalf("site %s is outside main.cs:\n%s", site, mainCs)
		}

		units := utf16.Encode([]rune(lines[lineNumber-1]))

		if column < 1 || column > len(units) {
			t.Fatalf("site %s is outside its line %q", site, lines[lineNumber-1])
		}

		got = append(got, string(utf16.Decode(units[column-1:])))
	}

	// The literals handed straight to defer, each at its `{` and at its first statement, and the empty one at its `{`
	// alone (it holds no statement); the comment and blank line ahead of "block" are passed over. The argument form
	// passes its literal to defer itself (`defer((@string tag) => { … }, "argument", ref ᒐ)`), so it is a deferred
	// closure too, as Go's traceback has it.
	want := []string{
		`{`, `where("one-liner");`,
		`{`, `where("block");`,
		`{`, `where("parenthesized");`,
		`{`,
		`{`, `where(tag);`,
	}

	if len(got) != len(want) {
		t.Fatalf("deferred-closure sites: got %d %q, want %d %q:\n%s", len(got), got, len(want), want, mainCs)
	}

	for i := range want {
		if !strings.HasPrefix(got[i], want[i]) {
			t.Errorf("site %d (%s): the emission there reads %q, want it to begin %q:\n%s", i, strings.Split(args[7][1], ",")[i], got[i], want[i], mainCs)
		}
	}

	// The sentinels are gone from the emission, and the literals the record must not list are emitted unmarked.
	if strings.Contains(mainCs, PositionSentinel) {
		t.Errorf("a sentinel survived into main.cs:\n%s", mainCs)
	}
}
