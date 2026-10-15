// interfaceMethodDiamond_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a call on an interface receiver whose method two embedded interfaces BOTH declare (H7 class C,
// go1.26.9 archive/zip reader.go:1001, `s.Name()` on `fileInfoDirEntry interface { fs.FileInfo; fs.DirEntry }`).
// Go allows the overlap and go/types resolves the call to one method. C# finds Name through both base
// interfaces and rejects the bare call as ambiguous (CS0121), so the call is emitted against the interface
// go/types resolved: `((fs.FileInfo)(s)).Name()`. The controls: a method only one embedded interface
// declares is called as is, and a diamond through ONE shared declaring interface is not ambiguous in C#.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestACallOnAMethodTwoEmbeddedInterfacesDeclareNamesTheResolvedInterface(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/imd\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "io/fs"

type Namer interface {
	Name() string
	Size() int
}

type Labeler interface {
	Name() string
	Kind() string
}

type both interface {
	Namer
	Labeler
}

type Base interface{ ID() int }

type left interface{ Base }

type right interface{ Base }

type shared interface {
	left
	right
}

type fileInfoDirEntry interface {
	fs.FileInfo
	fs.DirEntry
}

type impl struct{}

func (impl) Name() string { return "n" }
func (impl) Size() int     { return 1 }
func (impl) Kind() string  { return "k" }
func (impl) ID() int       { return 2 }

func get() both     { return impl{} }
func ids() shared   { return impl{} }
func stat(s fileInfoDirEntry) bool { return s.Name() == "." }

func main() {
	b := get()
	println(b.Name(), b.Size(), b.Kind(), ids().ID(), stat(nil))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "imd", "main.cs"))

	for _, want := range []string{
		// the overlap: emitted against the interface go/types resolved, the first embedded declarer
		"((Namer)(b)).Name()",
		"((fs.FileInfo)(s)).Name()",
		// the controls: one declarer, and one declarer reached by two paths
		"b.Size()",
		"b.Kind()",
		"ids().ID()",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("main.cs lacks %q\n--- main.cs ---\n%s", want, mainCs)
		}
	}

	for _, unwanted := range []string{"(b.Name()", " b.Name()", "s.Name() =="} {
		if strings.Contains(mainCs, unwanted) {
			t.Errorf("main.cs still calls the overlapping method on the bare receiver (%q)\n--- main.cs ---\n%s", unwanted, mainCs)
		}
	}
}
