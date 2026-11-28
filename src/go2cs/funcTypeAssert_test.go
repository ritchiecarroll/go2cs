// funcTypeAssert_test.go - Gbtc
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
	"runtime"
	"strings"
	"testing"
)

// An assertion to a func type takes golib's `_ᶠ` forms, never the general `_<T>`. `_<T>` annotates T with
// DynamicallyAccessedMembers(PublicMethods), which on a delegate reaches Delegate.CreateDelegate's
// RequiresUnreferencedCode overloads, so the trim analyzer reported every func-type assertion in a converted
// program (IL2026 and IL2111, three of each a site). `_ᶠ` takes a Delegate-constrained T with no annotation.
//
// No generated overload exists for `_ᶠ`, so the source interface is named in the call, `x._ᶠ<I, F>()`, wherever
// the converter can name it (error included) and a failure's text stays Go's; any other source takes the object
// form and the text "interface {}". The comma-ok form, the folded if-init and the type-switch guard take `_ᶠ` too.
func TestFuncTypeAssertForms(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/funcassert\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "other", "other.go"), `package other

import "io"

type hidden interface {
	io.Reader
	io.Writer
}

type impl struct{}

func (*impl) Read(p []byte) (int, error)  { return 0, io.EOF }
func (*impl) Write(p []byte) (int, error) { return len(p), nil }

// Get hands out a value whose static type no other package can name.
func Get() hidden { return &impl{} }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"io"

	"example.com/funcassert/other"
)

// rw is a func type with methods: the only kind of func type a non-empty interface can assert to.
type rw func([]byte) (int, error)

func (f rw) Read(p []byte) (int, error)  { return f(p) }
func (f rw) Write(p []byte) (int, error) { return f(p) }
func (f rw) Error() string               { return "rw" }
func (f rw) name() string                { return "rw" }

// Methodless.
type plain func(int) int

type loc interface{ name() string }

func fromReader(reader io.Reader) rw      { return reader.(rw) }
func fromError(err error) rw              { return err.(rw) }
func fromLoc(unexp loc) rw                { return unexp.(rw) }
func fromAny(anyValue any) plain          { return anyValue.(plain) }
func fromAnyLit(litValue any) func() int  { return litValue.(func() int) }
func commaOK(okValue any) (plain, bool)   { v, ok := okValue.(plain); return v, ok }

func folded(foldValue any) int {
	if f, ok := foldValue.(plain); ok {
		return f(1)
	}
	if g, ok := foldValue.(func() int); ok {
		return g()
	}
	return 0
}

func switched(switchValue any) int {
	switch f := switchValue.(type) {
	case plain:
		return f(2)
	case func() int:
		return f()
	}
	return 0
}

func main() {
	var f rw = func(p []byte) (int, error) { return len(p), nil }
	fmt.Println(fromReader(f), fromError(f), fromLoc(f), fromAny(plain(nil)), fromAnyLit(func() int { return 1 }))
	fmt.Println(commaOK(plain(nil)))
	fmt.Println(folded(plain(nil)), switched(plain(nil)))

	hid := other.Get()
	fmt.Println(hid.(rw))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "funcassert", "main.cs"))

	for _, want := range []struct{ line, why string }{
		{"reader._ᶠ<io.Reader, rw>()", "an exported interface of another package is named"},
		{"err._ᶠ<error, rw>()", "error is named"},
		{"unexp._ᶠ<loc, rw>()", "an unexported interface of this package is named"},
		{"anyValue._ᶠ<plain>()", "the empty interface takes the object form"},
		{"litValue._ᶠ<Func<nint>>()", "an anonymous func target renders its delegate"},
		{"okValue._ᶠ<plain>(ᐧ)", "the comma-ok form"},
		// Two sibling ifs declaring one `ok` fold their inits into golib's out-parameter form.
		{"foldValue._ᶠ<plain>(out var f)", "the folded if-init"},
		{"foldValue._ᶠ<Func<nint>>(out var g)", "the folded if-init, anonymous target"},
		{"when Δf._ᶠ<plain>(out var f)", "a named func type's type-switch guard"},
		{"when Δf._ᶠ<Func<nint>>(out var f)", "an anonymous func type's type-switch guard"},
		{"hid._ᶠ<rw>()", "a source the file cannot name takes the object form, unwidened"},
	} {
		if !strings.Contains(mainCs, want.line) {
			t.Errorf("missing %q (%s) in:\n%s", want.line, want.why, mainCs)
		}
	}

	for _, unwanted := range []string{"._<rw>", "._<plain>", "._<Func<nint>>", "((object)hid)"} {
		if strings.Contains(mainCs, unwanted) {
			t.Errorf("unexpected %q in:\n%s", unwanted, mainCs)
		}
	}
}
