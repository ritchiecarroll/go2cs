// typeSpecTrailingComment_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards seat #43 (class 1 of the comment-parity residue): a Go type spec's TRAILING comment rides the emitted
// declaration line, after any /*fact*/ block, whatever the defined type's kind. sort's
// `type sortedHint int // hint for pdqsort when choosing the pivot` became `partial struct sortedHint /*num:nint*/;`
// with the comment gone; maps, pointers, channels and one-line structs in a type group lost theirs the same way.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestTypeSpecTrailingCommentRidesTheDeclarationLine(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/tstc\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type sortedHint int // hint for pdqsort when choosing the pivot

type depSet map[any]bool // set of true bools

type byteptr *byte // used when the value is a string

type signals chan int // delivered in order

type (
	int64Val int64              // Int values representable as an int64
	intVal   struct{ val *int } // Int values not representable as an int64
)

type kept struct { // a struct's opening line
	x int // a field comment
}

func main() {
	var h sortedHint
	var d depSet
	var p byteptr
	var s signals
	var a int64Val
	var b intVal
	var k kept
	_, _, _, _, _, _, _ = h, d, p, s, a, b, k
}
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
		includeComments:     true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "tstc", "main.cs")), "\r\n", "\n")
	lines := strings.Split(mainCs, "\n")

	// Each declaration, named by the text that opens it, and the Go comment its line must end with.
	for _, c := range []struct{ decl, comment string }{
		{"partial struct sortedHint", "// hint for pdqsort when choosing the pivot"},
		{"partial struct depSet", "// set of true bools"},
		{"partial class byteptr", "// used when the value is a string"},
		{"partial struct signals", "// delivered in order"},
		{"partial struct int64Val", "// Int values representable as an int64"},
		{"partial struct intVal", "// Int values not representable as an int64"},
	} {
		found := false

		for _, line := range lines {
			if strings.Contains(line, c.decl) {
				found = true

				if !strings.HasSuffix(strings.TrimRight(line, " "), c.comment) {
					t.Errorf("%q: the declaration line does not end with the Go comment %q:\n  %s", c.decl, c.comment, line)
				} else if fact := strings.Index(line, "/*"); fact >= 0 && fact > strings.Index(line, c.comment) {
					t.Errorf("%q: the Go comment precedes the /*fact*/ block:\n  %s", c.decl, line)
				}
			}
		}

		if !found {
			t.Errorf("control: no line declares %q:\n%s", c.decl, mainCs)
		}
	}

	// Controls: a struct's opening line and its field already carried their comments, once each.
	for _, comment := range []string{"// a struct's opening line", "// a field comment"} {
		if n := strings.Count(mainCs, comment); n != 1 {
			t.Errorf("control: %q appears %d times, want 1:\n%s", comment, n, mainCs)
		}
	}
}
