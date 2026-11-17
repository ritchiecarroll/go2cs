// sstringTwinCallRule_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"strings"
	"testing"
)

// The call rule of validateSStringTwin: a twinned parameter passed to a same-package function that is
// not a twin at that index is a hidden sstring → @string copy on every call. unicode/utf8's
// DecodeRuneInString passed its whole remaining input to decodeRuneInStringSlow(@string), copying the
// rest of the source per non-ASCII rune in the converted TypeScript scanner
// (docs/phase4/SCOUT-ts-perf.md, item S2); fmt.Errorf did the same through errorf.
const sstringTwinCallRuleFixture = `package rule

import "strings"

func helper(s string) int { return len(s) }

func Twin(s string) int { return helper(s[1:]) }

func Chained(s string) int { return Twin(s) }

func Foreign(s string) int { return strings.IndexByte(s, 'x') }

func Value(s string, f func(string) int) int { return f(s) }
`

func validateFixtureTwin(t *testing.T, name string, twins map[string][]int) (refusal string) {
	t.Helper()

	fileSet := token.NewFileSet()
	file, err := parser.ParseFile(fileSet, "rule.go", sstringTwinCallRuleFixture, 0)

	if err != nil {
		t.Fatal(err)
	}

	info := &types.Info{Uses: map[*ast.Ident]types.Object{}, Defs: map[*ast.Ident]types.Object{}, Types: map[ast.Expr]types.TypeAndValue{}}
	pkg, err := (&types.Config{Importer: importer.Default()}).Check("rule", fileSet, []*ast.File{file}, info)

	if err != nil {
		t.Fatal(err)
	}

	for key, indices := range twins {
		sstringTwins[key] = indices
	}

	t.Cleanup(func() {
		for key := range twins {
			delete(sstringTwins, key)
		}
	})

	var decl *ast.FuncDecl

	for _, d := range file.Decls {
		if fd, ok := d.(*ast.FuncDecl); ok && fd.Name.Name == name {
			decl = fd
		}
	}

	defer func() {
		if r := recover(); r != nil {
			refusal = fmt.Sprint(r)
		}
	}()

	v := &Visitor{pkg: pkg, info: info}
	v.validateSStringTwin(decl, info.Defs[decl.Name].(*types.Func), twins["rule."+name])

	return ""
}

func TestSStringTwinCallRuleRefusesASamePackageCopy(t *testing.T) {
	refusal := validateFixtureTwin(t, "Twin", map[string][]int{"rule.Twin": {0}})

	if !strings.Contains(refusal, "parameter s is passed to rule.helper as argument #0") {
		t.Fatalf("a twin passing its parameter to a same-package non-twin must be refused by name; got %q", refusal)
	}
}

func TestSStringTwinCallRuleAdmitsATwinCallee(t *testing.T) {
	if refusal := validateFixtureTwin(t, "Twin", map[string][]int{"rule.Twin": {0}, "rule.helper": {0}}); refusal != "" {
		t.Fatalf("a twin passing its parameter to a twin at that index must be admitted; got %q", refusal)
	}

	if refusal := validateFixtureTwin(t, "Chained", map[string][]int{"rule.Chained": {0}, "rule.Twin": {0}, "rule.helper": {0}}); refusal != "" {
		t.Fatalf("a twin calling a twin must be admitted; got %q", refusal)
	}
}

func TestSStringTwinCallRuleAdmitsAForeignCallee(t *testing.T) {
	if refusal := validateFixtureTwin(t, "Foreign", map[string][]int{"rule.Foreign": {0}}); refusal != "" {
		t.Fatalf("a call into another package keeps that package's @string contract and must be admitted; got %q", refusal)
	}
}

func TestSStringTwinCallRuleRefusesAFuncValue(t *testing.T) {
	refusal := validateFixtureTwin(t, "Value", map[string][]int{"rule.Value": {0}})

	if !strings.Contains(refusal, "parameter s is passed to a function value as argument #0") {
		t.Fatalf("a twin passing its parameter to a func value must be refused by name; got %q", refusal)
	}
}
