// foreignGenericAdapter_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"testing"
)

// TestForeignGenericAdapterRoute pins foreignGenericAdapterIsOpen, the converter's choice between
// ONE generic adapter over a foreign generic's open type parameters and one closed adapter per
// instantiation (C33, tsc's internal/module). The open route is taken only when every method the
// interface requires is declared on the generic definition with a signature free of its type
// parameters; every other row keeps the closed route, the corpus's existing shape.
func TestForeignGenericAdapterRoute(t *testing.T) {
	const src = `package lib

type Field interface {
	IsPresent() bool
	Describe() string
}

type Expected[T any] struct{ Value T }

func (e *Expected[T]) IsPresent() bool  { return true }
func (e *Expected[T]) Describe() string { return "" }

// Get mentions T: only Box[string] satisfies Getter (the internal/sync HashTrieMap shape).
type Getter interface{ Get() string }

type Box[T any] struct{ Item T }

func (b *Box[T]) Get() T { return b.Item }

// Visit's parameter is a func type that mentions T.
type Visitor interface{ Visit(func(string)) }

type Walker[T any] struct{}

func (w *Walker[T]) Visit(f func(T)) {}

// Promoted: Wrapper[T] reaches IsPresent and Describe only through its embedded field.
type Wrapper[T any] struct{ *Expected[T] }

// A generic interface instantiated at the type argument.
type Source[T any] interface{ Next() T }

type Stream[T any] struct{}

func (s *Stream[T]) Next() T { var zero T; return zero }

// A non-generic struct never takes the generic route.
type Plain struct{}

func (p *Plain) IsPresent() bool  { return true }
func (p *Plain) Describe() string { return "" }
`

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "lib.go", src, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{Importer: importer.Default()}).Check("lib", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	lookup := func(name string) types.Type { return pkg.Scope().Lookup(name).Type() }

	instantiate := func(name string, args ...types.Type) *types.Named {
		inst, err := types.Instantiate(nil, lookup(name), args, true)

		if err != nil {
			t.Fatalf("instantiate %s: %v", name, err)
		}

		return inst.(*types.Named)
	}

	str := types.Typ[types.String]
	strMap := types.NewMap(str, str)

	cases := []struct {
		name  string
		named *types.Named
		iface types.Type
		open  bool
	}{
		{"Expected[string] as Field (control: the TS pair, open)", instantiate("Expected", str), lookup("Field"), true},
		{"Expected[map[string]string] as Field (the second TS instantiation, open)", instantiate("Expected", strMap), lookup("Field"), true},
		{"Box[string] as Getter: Get mentions T", instantiate("Box", str), lookup("Getter"), false},
		{"Walker[string] as Visitor: a func-typed parameter mentions T", instantiate("Walker", str), lookup("Visitor"), false},
		{"Wrapper[string] as Field: promoted methods", instantiate("Wrapper", str), lookup("Field"), false},
		{"Stream[string] as Source[string]: a generic interface", instantiate("Stream", str), instantiate("Source", str), false},
		{"Plain as Field: not generic", lookup("Plain").(*types.Named), lookup("Field"), false},
	}

	for _, c := range cases {
		if got := foreignGenericAdapterIsOpen(c.named, c.iface); got != c.open {
			t.Errorf("%s: foreignGenericAdapterIsOpen = %v, want %v", c.name, got, c.open)
		}
	}
}

// TestForeignGenericAdapterRouteKeepsTheCorpusPair is the negative control against the corpus's one
// existing foreign-generic pointer pair: internal/sync's HashTrieMap[any, any] as sync's mapInterface
// (sync/map_reference_test.go). HashTrieMap's methods take K and V, so the pair must stay on the
// closed route the committed sync test emission already uses. mapInterface lives in a test file, so
// its method set is declared here; HashTrieMap is the real one, type-checked from GOROOT source.
func TestForeignGenericAdapterRouteKeepsTheCorpusPair(t *testing.T) {
	const src = `package probe

import isync "internal/sync"

type mapInterface interface {
	Load(key any) (value any, ok bool)
	Store(key, value any)
	LoadOrStore(key, value any) (actual any, loaded bool)
	LoadAndDelete(key any) (value any, loaded bool)
	Delete(any)
	Swap(key, value any) (previous any, loaded bool)
	CompareAndSwap(key, old, new any) (swapped bool)
	CompareAndDelete(key, old any) (deleted bool)
	Range(func(key, value any) (shouldContinue bool))
	Clear()
}

var _ mapInterface = &isync.HashTrieMap[any, any]{}
`

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "probe.go", src, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{Importer: importer.ForCompiler(fset, "source", nil)}).Check("probe", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	var hashTrieMap types.Type

	for _, imported := range pkg.Imports() {
		if imported.Path() == "internal/sync" {
			hashTrieMap = imported.Scope().Lookup("HashTrieMap").Type()
		}
	}

	if hashTrieMap == nil {
		t.Fatal("internal/sync.HashTrieMap not found")
	}

	anyType := types.Universe.Lookup("any").Type()
	inst, err := types.Instantiate(nil, hashTrieMap, []types.Type{anyType, anyType}, true)

	if err != nil {
		t.Fatal(err)
	}

	if foreignGenericAdapterIsOpen(inst.(*types.Named), pkg.Scope().Lookup("mapInterface").Type()) {
		t.Error("HashTrieMap[any, any] as mapInterface took the OPEN route; its methods take K and V, so it must stay closed")
	}
}
