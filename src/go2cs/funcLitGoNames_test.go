// funcLitGoNames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the Go name every function literal is emitted with (funcLitGoNames.go). The expected names are Go's own,
// measured on go1.26.11 with `go run -gcflags=-l` over the same two files with the same declarations, each literal
// returning runtime.FuncForPC of its own frame there where it returns its key string here.
package main

import (
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"strconv"
	"testing"
)

const funcLitNamesFileA = `package main

type ring struct{ n int }

type List[T any] struct{ v T }

type gauge struct{}

// A var declared first whose initializer reads a later one is initialized after it, and Go numbers the literals in
// initialization order.
var aFirst = func() string { return "aFirst" + bVal() }

var aSecond = func() string { return "aSecond" }

var aMulti, aMulti2 = func() string { return "aMulti" }, func() string { return "aMulti2" }

var aNested = func() func() string { return func() string { return "aNestedInner" } }

var _ = func() string { return "blank" }()

func (r *ring) init() string { f := func() string { return "ringInit" }; return f() }

func (l *List[T]) Push() string { f := func() string { return "listPush" }; return f() }

func (g gauge) Name() string { f := func() string { return "gaugeName" }; return f() }

func Map[T any](x T) string { f := func() string { return "map" }; return f() }

func outer() string {
	a := func() string {
		b := func() string { return "outer11" }
		c := func() string { return "outer12" }
		return b() + c()
	}
	d := func() string { return "outer2" }
	return a() + d()
}

func init() { _ = func() string { return "init0" } }
`

const funcLitNamesFileB = `package main

var bVal = func() string { return "bVal" }

var tbl = []func() string{func() string { return "tbl0" }, func() string { return "tbl1" }}

func init() { _ = func() string { return "init1" } }

func _() { _ = func() string { return "blankFunc" } }
`

// TestFuncLitGoNamesAreGosNames checks every literal's name against the one Go gives it.
func TestFuncLitGoNamesAreGosNames(t *testing.T) {
	fset := token.NewFileSet()
	var files []*ast.File

	for _, source := range []struct{ name, text string }{{"a.go", funcLitNamesFileA}, {"b.go", funcLitNamesFileB}} {
		file, err := parser.ParseFile(fset, source.name, source.text, 0)

		if err != nil {
			t.Fatal(err)
		}

		files = append(files, file)
	}

	info := &types.Info{Defs: map[*ast.Ident]types.Object{}, Uses: map[*ast.Ident]types.Object{}}

	if _, err := (&types.Config{Importer: importer.Default()}).Check("main", fset, files, info); err != nil {
		t.Fatal(err)
	}

	collectFuncLitGoNames(info, files)

	defer func() { packageFuncLitGoNames = map[*ast.FuncLit]string{} }()

	// key: the string the literal returns first; name: Go's, measured.
	want := map[string]string{
		"aSecond":      "init.func1",
		"aMulti":       "init.func2",
		"aMulti2":      "init.func3",
		"aNestedInner": "init.func4.1",
		"blank":        "init.func5",
		"bVal":         "init.func6",
		"aFirst":       "init.func7",
		"tbl0":         "init.func8",
		"tbl1":         "init.func9",
		"ringInit":     "(*ring).init.func1",
		"listPush":     "(*List[...]).Push.func1",
		"gaugeName":    "gauge.Name.func1",
		"map":          "Map[...].func1",
		"outer11":      "outer.func1.1",
		"outer12":      "outer.func1.2",
		"outer2":       "outer.func2",
		"init0":        "init.0.func1",
		"init1":        "init.1.func1",
	}

	// The literal that holds aNestedInner's literal is itself init.func4.
	wantOuter := map[string]string{"aNestedInner": "init.func4", "outer11": "outer.func1"}

	got := map[string]string{}
	gotOuter := map[string]string{}

	for _, file := range files {
		ast.Inspect(file, func(node ast.Node) bool {
			lit, ok := node.(*ast.FuncLit)

			if !ok {
				return true
			}

			key := firstStringIn(lit)
			got[key] = funcLitGoName(lit)

			ast.Inspect(lit.Body, func(inner ast.Node) bool {
				if nested, ok := inner.(*ast.FuncLit); ok {
					if _, tracked := wantOuter[firstStringIn(nested)]; tracked {
						gotOuter[firstStringIn(nested)] = funcLitGoName(lit)
					}
				}

				return true
			})

			return true
		})
	}

	for key, name := range want {
		if got[key] != name {
			t.Errorf("literal %q: got %q, want Go's %q", key, got[key], name)
		}
	}

	for key, name := range wantOuter {
		if gotOuter[key] != name {
			t.Errorf("literal enclosing %q: got %q, want Go's %q", key, gotOuter[key], name)
		}
	}

	// A blank function's literal is left unnamed: the runtime's fallback names it.
	if got["blankFunc"] != "" {
		t.Errorf("literal in func _: got %q, want no name", got["blankFunc"])
	}
}

// firstStringIn answers the first string literal under node, unquoted.
func firstStringIn(node ast.Node) string {
	found := ""

	ast.Inspect(node, func(n ast.Node) bool {
		if found != "" {
			return false
		}

		if lit, ok := n.(*ast.BasicLit); ok && lit.Kind == token.STRING {
			found, _ = strconv.Unquote(lit.Value)
			return false
		}

		return true
	})

	return found
}
