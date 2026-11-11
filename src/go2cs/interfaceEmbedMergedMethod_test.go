// interfaceEmbedMergedMethod_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the DECLARATION of an interface whose method two embedded interfaces BOTH declare (C42, typescript-go
// internal/compiler: `EmitHost interface { printer.EmitHost; declarations.DeclarationEmitHost }`, both declaring
// GetProjectReferenceFromSource). Go merges the two into one method. C# keeps each embed as a base interface with
// the method declared only there, so every lookup on the derived interface finds two members (CS0121). The call
// sites the converter writes are cast to the interface go/types resolved (interfaceMethodDiamond_test.go), but a
// generated adapter that forwards to a value of the derived interface, a call on a type parameter it constrains
// and a method value are not, and neither is hand-written C#. So the derived interface redeclares the merged
// method once, `new`, which hides both inherited members. The controls: a method one embed declares, a diamond
// through ONE shared declaring interface, a method the interface declares itself, and a constraint interface.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestAnInterfaceRedeclaresAMethodTwoEmbeddedInterfacesDeclare(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/iem\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "io/fs"

type Path string
type PathAlias = Path

type Namer interface {
	Name(p Path) string
	Size() int
}

type Labeler interface {
	Name(q PathAlias) string
	Kind() string
}

type both interface {
	Namer
	Labeler
}

type Tagger interface {
	Name(p Path) string
	Tag() string
}

type nested interface {
	both
	Tagger
}

type Base interface{ ID() int }

type left interface{ Base }

type right interface{ Base }

type shared interface {
	left
	right
}

type own interface {
	Namer
	Labeler
	Name(p Path) string
}

type fileInfoDirEntry interface {
	fs.FileInfo
	fs.DirEntry
}

type constraint interface {
	comparable
	Namer
	Labeler
}

type impl struct{}

func (impl) Name(p Path) string { return "n" + string(p) }
func (impl) Size() int           { return 1 }
func (impl) Kind() string        { return "k" }
func (impl) Tag() string         { return "t" }
func (impl) ID() int             { return 2 }

func get() both     { return impl{} }
func deep() nested  { return impl{} }
func ids() shared   { return impl{} }
func mine() own     { return impl{} }
func stat(s fileInfoDirEntry) bool { return s.Name() == "." }
func pick[T constraint](v T) string { return v.Kind() }

func local() string {
	type inner interface {
		Namer
		Tagger
	}

	var i inner = impl{}
	return i.Tag()
}

func main() {
	println(get().Size(), deep().Tag(), ids().ID(), mine().Kind(), stat(nil), pick(impl{}), local())
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
		includeComments:     true,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "iem", "main.cs")), "\r\n", "\n")

	// The body of one emitted interface, from its declaration line to its closing brace
	body := func(name string) string {
		t.Helper()

		match := regexp.MustCompile(`(?s)partial interface ` + regexp.QuoteMeta(name) + `\b[^{]*\{(.*?)\n\}`).FindStringSubmatch(mainCs)

		if match == nil {
			t.Fatalf("main.cs declares no interface %s\n--- main.cs ---\n%s", name, mainCs)
		}

		return match[1] + "\n"
	}

	for _, check := range []struct {
		iface string
		want  []string
		count map[string]int
	}{
		// the overlap: redeclared once, in the spelling of the method go/types merged to (the first declarer's)
		{"both", []string{"// Name is declared by Namer and Labeler\n", "    new @string Name(Path p);\n"}, map[string]int{"Name(": 1, "new ": 1, "Size(": 0, "Kind(": 0}},
		// an interface that embeds the merging one and a third declarer redeclares it too
		{"nested", []string{"// Name is declared by Namer, Labeler and Tagger\n", "    new @string Name(Path p);\n"}, map[string]int{"Name(": 1, "new ": 1, "Tag(": 0}},
		// both methods fs.FileInfo and fs.DirEntry share
		{"fileInfoDirEntry", []string{"// Name is declared by fs.FileInfo and fs.DirEntry\n", "    new @string Name();\n", "    new bool IsDir();\n"}, map[string]int{"new ": 2}},
		// a function-local interface is hoisted and redeclares the same way
		{"local_inner", []string{"    new @string Name(Path p);\n"}, map[string]int{"new ": 1}},
		// the controls: one declarer reached by two paths, a method the interface declares itself, a constraint
		{"shared", nil, map[string]int{"new ": 0, "ID(": 0}},
		{"own", []string{"    @string Name(Path p);\n"}, map[string]int{"Name(": 1, "new ": 0}},
		{"constraint", nil, map[string]int{"new ": 0, "Name(": 0}},
	} {
		got := body(check.iface)

		for _, want := range check.want {
			if !strings.Contains(got, want) {
				t.Errorf("interface %s lacks %q\n--- body ---\n%s", check.iface, want, got)
			}
		}

		for text, count := range check.count {
			if n := strings.Count(got, text); n != count {
				t.Errorf("interface %s holds %q %d times, want %d\n--- body ---\n%s", check.iface, text, n, count, got)
			}
		}
	}
}
