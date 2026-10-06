// embedMark_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards face lift F (docs/PLAN-marker-comment-parity.md, 5.6) from the converter's own rendering: an
// embedded field with no `partial ref` shape -- a predeclared type, a pointer to one, or an interface --
// is marked with a `/*embed*/` comment, which go2cs-gen records on the struct, and never with
// [GoEmbedded]. A field named after its type but not embedded carries no mark.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestEmbeddedFieldsAreMarkedByComment(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/emk\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type Reader interface{ Read() int }

type withEmbeds struct {
	int
	*byte
	Reader
	N int
}

type named struct {
	Reader Reader
}

func main() {
	var w withEmbeds
	var n named
	fmt.Println(w.int, w.byte == nil, w.Reader == nil, w.N, n.Reader == nil)
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
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "emk", "main.cs"))

	if strings.Contains(mainCs, "GoEmbedded") {
		t.Errorf("converted code still carries [GoEmbedded]:\n%s", mainCs)
	}

	for _, want := range []string{
		"/*embed*/ internal nint @int;",
		"/*embed*/ internal ж<byte> @byte;",
		"/*embed*/ public Reader Reader;",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in:\n%s", want, mainCs)
		}
	}

	if got := strings.Count(mainCs, "/*embed*/"); got != 3 {
		t.Errorf("got %d /*embed*/ marks, want 3 (the named field Reader Reader is not embedded):\n%s", got, mainCs)
	}
}
