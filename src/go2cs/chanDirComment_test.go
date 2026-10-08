// chanDirComment_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards face-lift row 3 (docs/PLAN-marker-comment-parity.md §11, owner ruling 2026-10-07): a defined channel
// type whose direction is a single outermost level carries it in its definition comment, spelled as Go spells
// it -- `partial struct IntChanRecv /*<-chan nint*/;` -- with no [GoChanDir]; go2cs-gen reads the spelling back
// and re-emits [GoType("chan nint")] and [GoChanDir(GoChanDir.Recv)] on its generated part. A NESTED direction
// chain (`<-chan (chan<- T)`), which the outermost spelling cannot carry, keeps its [GoChanDir(...)] chain and
// its attribute definition, unchanged.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestADirectionalChannelSpellsItsDirectionInTheDefinitionComment(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/cdc\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type IntChanRecv <-chan int

type IntChanSend chan<- int

type Both chan int

type Nested <-chan (chan<- int)

func main() {
	c := make(chan int, 1)
	var r IntChanRecv = c
	var s IntChanSend = c
	var b Both = c
	var n Nested
	fmt.Println(cap(r), cap(s), cap(b), n == nil)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "cdc", "main.cs"))

	for _, want := range []string{
		"partial struct IntChanRecv /*<-chan nint*/;",
		"partial struct IntChanSend /*chan<- nint*/;",
		"partial struct Both /*chan nint*/;",
		// The nested element renders with a direction comment of its own (`channel/*<-*/<nint>`), which no
		// block comment can hold, so the definition keeps its attribute (goTypeMarker) beside the chain.
		`[GoType("chan channel/*<-*/<nint>")] [GoChanDir(GoChanDir.Recv, GoChanDir.Send)] partial struct Nested;`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in main.cs:\n%s", want, mainCs)
		}
	}

	if n := strings.Count(mainCs, "[GoChanDir("); n != 1 {
		t.Errorf("want exactly one [GoChanDir(...)] (the nested chain), got %d:\n%s", n, mainCs)
	}
}
