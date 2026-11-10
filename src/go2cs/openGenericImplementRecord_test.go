// openGenericImplementRecord_test.go - Gbtc
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

// A conversion made at a generic type's OWN type parameters (`return &it` inside `NewIter[T]`, where it is a
// `sliceIter[T]`) names the generic adapter `sliceIterжIter<T>` at the cast site, but no record could carry it:
// an attribute's type argument cannot name the enclosing method's T, so the record was suppressed. The adapter
// then existed only when some OTHER cast closed the same pair (`sliceIter[int]` in main); a type reached ONLY
// through its parameters (counter[T] below) emitted CS0246 at the cast. The pair is now recorded by its
// DEFINITION, `[assembly: GoImplement(typeof(sliceIter<>), typeof(Iter), Pointer = true)]`, which an unbound
// generic can spell, and the value form likewise.
//
// A FOREIGN generic whose required methods do not mention its type parameters (lib.Box[T] as Iter: Next() bool) is
// recorded by its definition too, package-qualified, since C33 (foreignGenericAdapterIsOpen): go2cs-gen emits one
// generic adapter `lib_BoxжIter<T>` for it. Before C33 its pointer path recorded the CLOSED
// `GoImplement<…lib_package.Box<T>, Iter>`, which names T out of scope; that record must be gone.
//
// Controls keep today's behavior: an interface over T (Getter[T]) is outside the scope, so holder[T] gets no
// definition record; the closed sliceIter[int] record is unchanged and sits beside the open one.

func TestOpenGenericImplementIsRecordedByDefinition(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/opengeneric\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

type Box[T any] struct{ V T }

func (b *Box[T]) Next() bool { return false }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/opengeneric/lib"
)

type Iter interface{ Next() bool }

type Namer interface{ Name() string }

type Getter[T any] interface{ Get() T }

type sliceIter[T any] []T

func (s *sliceIter[T]) Next() bool { return len(*s) > 0 }

type counter[T any] struct{ n T }

func (c *counter[T]) Next() bool { return false }

type pair[K comparable, V any] struct {
	k K
	v V
}

func (p pair[K, V]) Name() string { return fmt.Sprint(p.k) }

type holder[T any] struct{ v T }

func (h *holder[T]) Get() T { return h.v }

func NewIter[T any](values []T) Iter {
	it := sliceIter[T](values)
	return &it
}

func NewCounter[T any]() Iter { return &counter[T]{} }

func asNamer[K comparable, V any](p pair[K, V]) Namer { return p }

func foreign[T any]() Iter { return &lib.Box[T]{} }

func overT[T any]() Getter[T] { return &holder[T]{} }

func main() {
	closed := sliceIter[int]{1}
	var i Iter = &closed
	fmt.Println(i.Next(), NewIter([]string{"a"}).Next(), NewCounter[int]().Next(),
		asNamer(pair[int, bool]{1, true}).Name(), foreign[int]().Next(), overT[int]().Get())
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

	info := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "opengeneric", PackageInfoFileName))

	for _, want := range []string{
		"[assembly: GoImplement(typeof(sliceIter<>), typeof(Iter), Pointer = true)]",
		"[assembly: GoImplement(typeof(counter<>), typeof(Iter), Pointer = true)]",
		"[assembly: GoImplement(typeof(pair<,>), typeof(Namer))]",
		"[assembly: GoImplement<sliceIter<nint>, Iter>(Pointer = true)]", // control: the closed record is unchanged
		"[assembly: GoImplement(typeof(go.example.com.opengeneric.lib_package.Box<>), typeof(Iter), Pointer = true)]",
	} {
		if !strings.Contains(info, want) {
			t.Errorf("package_info.cs is missing %s", want)
		}
	}

	// Controls: an interface over T keeps today's behavior and gets no definition record, and the foreign generic's
	// closed record, which named T out of scope, is no longer written.
	for _, unwanted := range []string{"typeof(holder<>)", "lib_package.Box<T>"} {
		if strings.Contains(info, unwanted) {
			t.Errorf("package_info.cs records an out-of-scope definition (%s)", unwanted)
		}
	}

	if t.Failed() {
		t.Logf("package_info.cs:\n%s", info)
	}
}
