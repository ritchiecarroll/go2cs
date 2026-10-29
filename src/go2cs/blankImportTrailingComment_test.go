// blankImportTrailingComment_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards seat #43 (item 3 of the comment-parity residue): a blank import's Go trailing comment stays its own `//`
// comment on the import's line. The converter notes a blank import with a `// blank import: ...` line, and the Go
// comment (`_ "unsafe" // for linkname`) used to follow it as text inside that one `//` comment. With a Go comment the
// note is a block comment; without one it keeps its `//` form.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestBlankImportTrailingCommentIsItsOwnComment(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/bitc\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	_ "unsafe" // for linkname
)

func main() {
	fmt.Println(other())
}
`)

	writeModuleFile(t, filepath.Join(appDir, "other.go"), `package main

import _ "unsafe"

func other() int { return 1 }
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

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "bitc")
	const note = "blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)"

	// noteLine returns the one line of a generated file that carries the blank-import note.
	noteLine := func(file string) string {
		text := strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, file)), "\r\n", "\n")
		found := ""

		for _, line := range strings.Split(text, "\n") {
			if strings.Contains(line, note) {
				if found != "" {
					t.Fatalf("control: %s carries the blank-import note twice:\n%s", file, text)
				}

				found = line
			}
		}

		if found == "" {
			t.Fatalf("control: %s carries no blank-import note:\n%s", file, text)
		}

		return found
	}

	if got, want := noteLine("main.cs"), "/* "+note+" */ // for linkname"; got != want {
		t.Errorf("a blank import with a Go trailing comment: the note must be a block comment and the Go comment its own `//` comment\n got: %s\nwant: %s", got, want)
	}

	// Control: a blank import with no Go comment keeps its `//` note.
	if got, want := noteLine("other.cs"), "// "+note; got != want {
		t.Errorf("control: a blank import with no Go comment must keep its `//` note\n got: %s\nwant: %s", got, want)
	}
}
