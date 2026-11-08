// positionCalls_test.go - Gbtc
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
	"strings"
	"testing"
)

// The PER-CALL table: a statement whose calls sit on later lines than its own carries, in its sentinel, one
// `name/ordinal/total/delta` entry per such call (laterLineCallPayload), and the record carries them keyed
// by the statement's C# line (encodePositionCalls). The runtime matches a frame's call by callee, ordinal
// and total; these pin what the converter writes for it.

const positionCallsSource = `package p

func line() int { return 0 }

func three(a, b, c int) []int { return nil }

type chain struct{}

func (c *chain) Add(n int) *chain { return c }
func (c *chain) Done()            {}

func shapes(c *chain, s []int) {
	_ = three(line(),
		line(),
		line())
	c.Add(1).
		Add(2).
		Done()
	f := line
	_ = f() +
		f()
	_ = int64(len(s)) +
		int64(line())
	_ = func() int { return 1 }() +
		line()
	_ = line()
	go three(line(),
		line(), 0)
}
`

func positionCallsVisitor(t *testing.T) (*Visitor, []ast.Stmt) {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "p.go", positionCallsSource, 0)

	if err != nil {
		t.Fatal(err)
	}

	info := &types.Info{
		Types:      map[ast.Expr]types.TypeAndValue{},
		Uses:       map[*ast.Ident]types.Object{},
		Defs:       map[*ast.Ident]types.Object{},
		Selections: map[*ast.SelectorExpr]*types.Selection{},
	}

	if _, err := (&types.Config{Importer: importer.Default()}).Check("p", fset, []*ast.File{file}, info); err != nil {
		t.Fatal(err)
	}

	for _, decl := range file.Decls {
		if fn, ok := decl.(*ast.FuncDecl); ok && fn.Name.Name == "shapes" {
			return &Visitor{fset: fset, info: info}, fn.Body.List
		}
	}

	t.Fatal("shapes not found")
	return nil, nil
}

func TestLaterLineCallPayloadDescribesEachShape(t *testing.T) {
	visitor, stmts := positionCallsVisitor(t)

	want := []string{
		// A multi-line call: the arguments evaluate before the call, so `three` is the fourth call and not on a
		// later line; the three `line` calls are 1..3 of 3, the later two recorded.
		"line/2/3/1,line/3/3/2",
		// A chain: Add(1) is on the statement's own line; Add(2) is the second of two Adds; Done the only Done.
		"Add/2/2/1,Done/1/1/2",
		// `f := line`: nothing on a later line.
		"",
		// A func value is a delegate, invoked through Invoke.
		"Invoke/2/2/1",
		// A conversion and a builtin are not calls a frame can be suspended in: only `line` counts.
		"line/1/1/1",
		// A statement that invokes a function literal is not described at all.
		"",
		// A single-line statement records nothing.
		"",
		// A `go` statement runs its call elsewhere: no roots.
		"",
	}

	if len(stmts) != len(want) {
		t.Fatalf("shapes has %d statements, want %d", len(stmts), len(want))
	}

	for index, stmt := range stmts {
		line := visitor.fset.Position(stmt.Pos()).Line

		if got := visitor.laterLineCallPayload(line, sentinelCallRoots(stmt)...); got != want[index] {
			t.Errorf("statement %d (line %d): payload %q, want %q", index, line, got, want[index])
		}
	}
}

func TestExtractPositionSentinelsCarriesTheCallPayload(t *testing.T) {
	text := strings.Join([]string{
		"namespace go;",
		PositionSentinel + "12;line/2/3/1,line/3/3/2" + PositionSentinel,
		"    _ = three(line(), line(), line());",
		PositionSentinel + "15" + PositionSentinel,
		"    x = 1;",
	}, "\r\n")

	stripped, entries := extractPositionSentinels(text)

	if strings.Contains(stripped, PositionSentinel) || strings.Contains(stripped, "line/2/3/1") {
		t.Fatalf("a sentinel or its payload survived stripping: %q", stripped)
	}

	want := []positionEntry{{csLine: 3, goLine: 12, calls: "line/2/3/1,line/3/3/2"}, {csLine: 5, goLine: 15}}

	if len(entries) != len(want) {
		t.Fatalf("got %d entries %+v, want %d", len(entries), entries, len(want))
	}

	for index, entry := range entries {
		if entry != want[index] {
			t.Errorf("entry %d is %+v, want %+v", index, entry, want[index])
		}
	}

	if got := encodePositionCalls(entries); got != "3=line/2/3/1,line/3/3/2" {
		t.Errorf("encoded per-call table %q, want %q", got, "3=line/2/3/1,line/3/3/2")
	}

	if got := encodePositionTable(entries); got != encodePositionTable([]positionEntry{{csLine: 3, goLine: 12}, {csLine: 5, goLine: 15}}) {
		t.Errorf("the line table changed with the payload: %q", got)
	}
}

// An init emitted as a statement of the enclosing block, `init; if (cond) {`, binds the same C# line as its
// `if`, whose own mark comes first and carries no calls (sentinelCallRoots gives an if with an init none). The
// init's payload rides onto that line's entry; the line's first binding still wins otherwise.
func TestExtractPositionSentinelsKeepsAnInitsPayloadOnItsIfsLine(t *testing.T) {
	shared := func(first, second string) []positionEntry {
		_, entries := extractPositionSentinels(strings.Join([]string{
			"namespace go;",
			PositionSentinel + first + PositionSentinel + PositionSentinel + second + PositionSentinel,
			"    nint l = command(\"a\",",
			"        \"b\").Run(); if (l > 0) {",
		}, "\r\n"))

		return entries
	}

	cases := []struct {
		name          string
		first, second string
		want          positionEntry
	}{
		{"the init's payload is kept", "145", "145;Run/1/1/1", positionEntry{csLine: 3, goLine: 145, calls: "Run/1/1/1"}},
		{"another Go line's payload is not", "145", "146;Run/1/1/1", positionEntry{csLine: 3, goLine: 145}},
		{"a payload of the line's own is kept", "145;Add/2/2/1", "145;Run/1/1/1", positionEntry{csLine: 3, goLine: 145, calls: "Add/2/2/1"}},
	}

	for _, c := range cases {
		if entries := shared(c.first, c.second); len(entries) != 1 || entries[0] != c.want {
			t.Errorf("%s: entries %+v, want [%+v]", c.name, entries, c.want)
		}
	}
}

func TestEncodePositionCallsIsEmptyWithoutLaterLineCalls(t *testing.T) {
	if got := encodePositionCalls([]positionEntry{{csLine: 3, goLine: 12}, {csLine: 5, goLine: 15}}); got != "" {
		t.Errorf("a file without later-line calls encoded %q; its record must keep its old shape", got)
	}
}
