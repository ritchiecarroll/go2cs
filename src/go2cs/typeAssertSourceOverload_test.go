// typeAssertSourceOverload_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/build"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"github.com/ritchiecarroll/hashset"
)

// A failed x.(T) prints the static interface x had: "io.Reader is *os.File, not *bytes.Buffer". golib's
// `_<T>(this object)` cannot see it, so the converted call takes one of three spellings:
//
//   - bare, `x._<T>()`, for an exported interface whose generated overload (an extension method in the
//     package's top-level `<pkg>_package_asserts` class) C# lookup reaches ahead of golib's object form;
//   - explicit, `x._<I, T>()`, binding golib's `_<TSource, T>`, wherever the interface can be named but
//     has no reachable overload: an unexported interface (a private nested type no top-level class can
//     name), a generic one, or an exported one in a namespace the file cannot see from where it is;
//   - widened, `((object)x)._<T>()`, for a source the file cannot name that embeds named interfaces,
//     whose bare call would bind an embedded interface's overload and name the wrong interface.
func TestTypeAssertSourceForms(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/assertsource\n\ngo 1.23\n")
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
	"io/fs"

	"example.com/assertsource/other"
)

type impl struct{}

func (*impl) Read(p []byte) (int, error)  { return 0, io.EOF }
func (*impl) Write(p []byte) (int, error) { return len(p), nil }
func (*impl) Get() int                    { return 1 }
func (*impl) Open(string) (fs.File, error) { return nil, nil }

// Unexported, embeds two interfaces.
type loc interface {
	io.Reader
	io.Writer
}

// Exported, embeds two interfaces, declared in this package.
type Exp interface {
	io.Reader
	io.Writer
}

// Embeds two interfaces and is generic, so it has no overload.
type gen[T any] interface {
	io.Reader
	io.Writer
	Get() T
}

// Unexported, embeds one interface.
type single interface {
	io.Reader
	Get() int
}

func fromReader(reader io.Reader) *impl           { return reader.(*impl) }
func fromReadWriter(readWriter io.ReadWriter) *impl { return readWriter.(*impl) }
func fromExp(exp Exp) *impl                       { return exp.(*impl) }
func fromLoc(unexp loc) *impl                     { return unexp.(*impl) }
func fromSingle(one single) *impl                 { return one.(*impl) }
func fromGeneric(generic gen[int]) *impl          { return generic.(*impl) }
func fromFS(fsys fs.FS) *impl                    { return fsys.(*impl) }
func fromAny(anyValue any) *impl                  { return anyValue.(*impl) }
func commaOK(okValue io.Reader) (*impl, bool)     { v, ok := okValue.(*impl); return v, ok }

func main() {
	fmt.Println(fromReader(&impl{}), fromReadWriter(&impl{}), fromExp(&impl{}), fromLoc(&impl{}), fromSingle(&impl{}), fromGeneric(&impl{}), fromFS(&impl{}), fromAny(&impl{}))
	fmt.Println(commaOK(&impl{}))

	hid := other.Get()
	fmt.Println(hid.(io.Reader))
	fmt.Println(hid.(*impl))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "assertsource", "main.cs"))

	for _, want := range []struct{ line, why string }{
		{"reader._<ж<impl>>()", "an exported interface binds its generated overload"},
		{"readWriter._<ж<impl>>()", "an exported interface embedding two others has its own overload"},
		{"exp._<ж<impl>>()", "an exported interface of this package has its own overload"},
		{"unexp._<loc, ж<impl>>()", "an unexported interface names itself, whatever it embeds"},
		{"one._<single, ж<impl>>()", "an unexported interface with one embedded interface names itself"},
		{"generic._<gen<nint>, ж<impl>>()", "a generic interface takes no overload, so it names itself"},
		{"fsys._<fs.FS, ж<impl>>()", "main is in the root namespace, which cannot see an overload in go.io.fs"},
		{"anyValue._<ж<impl>>()", "the empty interface stays bare"},
		{"((object)hid)._<ж<impl>>()", "another package's unexported interface embedding two others is widened"},
		{"okValue._<ж<impl>>(ᐧ)", "the comma-ok form is never touched"},
	} {
		if !strings.Contains(mainCs, want.line) {
			t.Errorf("missing %q (%s) in:\n%s", want.line, want.why, mainCs)
		}
	}

	for _, unwanted := range []string{"((object)okValue)", "((object)reader)", "((object)unexp)"} {
		if strings.Contains(mainCs, unwanted) {
			t.Errorf("unexpected %q in:\n%s", unwanted, mainCs)
		}
	}
}

// assertSourceFormFor decides from the source interface alone, so the cases the module test cannot reach
// -- a namespace that IS in scope beside an interface that has no overload there -- are built directly.
func TestAssertSourceFormForCases(t *testing.T) {
	isolateConverterPathGlobals(t)

	savedNamespace, savedChildren := packageNamespace, packageChildNamespaces
	t.Cleanup(func() { packageNamespace, packageChildNamespaces = savedNamespace, savedChildren })

	fset := token.NewFileSet()
	imp := importer.ForCompiler(fset, "source", nil)

	check := func(path, name, src string) *types.Package {
		file, err := parser.ParseFile(fset, name+".go", src, 0)

		if err != nil {
			t.Fatal(err)
		}

		pkg, err := (&types.Config{Importer: imp}).Check(path, fset, []*ast.File{file}, nil)

		if err != nil {
			t.Fatal(err)
		}

		return pkg
	}

	lib := check("example.com/lib/sub", "sub", `package sub

import "io"

type hidden interface {
	io.Reader
	io.Writer
}

type Both interface {
	io.Reader
	io.Writer
}

type One interface {
	io.Reader
	Close() error
}

type plain interface {
	Close() error
}

type Gen[T any] interface {
	io.Reader
	io.Writer
	Get() T
}

type Inst = Gen[int]
`)

	classPath := convertImportPathToNamespace(lib.Path(), PackageSuffix)
	libNamespace := RootNamespace + "." + classPath[:strings.LastIndex(classPath, ".")]

	typeOf := func(pkg *types.Package, name string) types.Type {
		return pkg.Scope().Lookup(name).Type()
	}

	other := types.NewPackage("example.com/me", "me")

	packageChildNamespaces = map[string]bool{libNamespace: true}
	importsLib := &Visitor{pkg: other, requiredUsings: hashset.NewHashSet([]string{"global::" + libNamespace})}

	root := check("rootpkg", "rootpkg", `package rootpkg

import "io"

type Both interface {
	io.Reader
	io.Writer
}
`)

	for _, tc := range []struct {
		name      string
		visitor   *Visitor
		namespace string
		source    types.Type
		want      assertSourceForm
	}{
		{"unexported of another package, namespace in scope", &Visitor{pkg: other}, libNamespace, typeOf(lib, "hidden"), assertSourceWiden},
		{"unexported of another package, embedding nothing", &Visitor{pkg: other}, libNamespace, typeOf(lib, "plain"), assertSourceBare},
		{"exported of another package, caller inside its namespace", &Visitor{pkg: other}, libNamespace, typeOf(lib, "Both"), assertSourceBare},
		{"exported of another package, caller outside it and not importing it", &Visitor{pkg: other}, "go.elsewhere", typeOf(lib, "Both"), assertSourceExplicit},
		{"exported of another package, caller in the root namespace", &Visitor{pkg: other}, RootNamespace, typeOf(lib, "Both"), assertSourceExplicit},
		{"exported of another package, imported by a caller in a nested namespace", importsLib, "go.elsewhere", typeOf(lib, "Both"), assertSourceBare},
		{"exported of another package, imported by a caller in the root namespace", importsLib, RootNamespace, typeOf(lib, "Both"), assertSourceExplicit},
		{"exported of a root package, caller in the root namespace", &Visitor{pkg: other}, RootNamespace, typeOf(root, "Both"), assertSourceBare},
		{"exported of a root package, caller in a nested namespace", &Visitor{pkg: other}, "go.elsewhere", typeOf(root, "Both"), assertSourceBare},
		{"exported with one embedded interface, namespace not in scope", &Visitor{pkg: other}, "go.elsewhere", typeOf(lib, "One"), assertSourceExplicit},
		{"generic of this package", &Visitor{pkg: lib}, libNamespace, typeOf(lib, "Gen"), assertSourceExplicit},
		{"unexported of this package", &Visitor{pkg: lib}, libNamespace, typeOf(lib, "hidden"), assertSourceExplicit},
		{"no source type", &Visitor{pkg: lib}, libNamespace, nil, assertSourceBare},
	} {
		packageNamespace = tc.namespace

		if got := tc.visitor.assertSourceFormFor(tc.source); got != tc.want {
			t.Errorf("%s: form %d, want %d", tc.name, got, tc.want)
		}
	}
}
