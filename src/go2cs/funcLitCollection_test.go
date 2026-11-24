// funcLitCollection_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the collection fallback for attributed literals (funcLitGoNames.go attributedLiteralOpensCollection). Every
// literal carries [GoFunc]; a C# collection expression whose first bracket token opens that attribute list is read
// by Roslyn as an attributed lambda, the whole collection with it (internal/profile's decoder tables:
// `[default, [GoFunc("init.func1")] (…) => {…}, …]`, CS1001). Such a collection is written `new T[]{…}`, the
// converter's own form for a literal that does not take the builtin, in all four positions the builtin forms take:
// a targeted collection, the slice builtin, the array builtin, and an elided slice behind a pointer element.
//
// Control: a collection whose first bracket is something else (an index) keeps the collection expression, since
// Roslyn's attribute guess ends there.
package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

const attributedCollectionFixture = `package main

type decoder func(int) int

type cfg struct{ n int }

type labeled struct{ name string }

func (l labeled) label() string { return l.name }

var table = []decoder{
	nil,
	func(n int) int { return n + 1 },
}

var appliers = []func(*cfg){
	func(c *cfg) { c.n++ },
}

var values = []func() int{func() int { return 7 }}

func main() {
	l := labeled{"a"}

	typed := []func() string{
		l.label,
		func() string { return "lit" },
	}

	arr := [2]func() int{func() int { return 2 }}

	ptrs := []*[]func() int{{func() int { return 3 }}}

	kept := []func() int{values[0], func() int { return 4 }}

	c := &cfg{}
	appliers[0](c)
	println(table[1](1), typed[0](), typed[1](), arr[0](), (*ptrs[0])[0](), kept[0](), kept[1](), c.n)
}
`

// TestAttributedLiteralKeepsItsCollectionAnArray pins the fallback in each position and the control.
func TestAttributedLiteralKeepsItsCollectionAnArray(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/collattr\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), attributedCollectionFixture)

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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "collattr", "main.cs"))

	for _, want := range []string{
		// a targeted collection whose first element, `default`, does not stop the attribute guess; its element is
		// the defined func type decoder, which is a delegate of its own
		"internal static slice<decoder> table = new decoder[]{",
		// a targeted collection that opens with a literal
		"internal static slice<Action<ж<cfg>>> appliers = new Action<ж<cfg>>[]{",
		`internal static slice<Func<nint>> values = new Func<nint>[]{[GoFunc("init.func3")] () => 7}.slice();`,
		// the slice builtin, opening with a method value's `(`
		"var typed = new Func<@string>[]{",
		// the array builtin
		`var arr = new Func<nint>[]{[GoFunc("main.func2")] () => 2}.array(2);`,
		// an elided slice behind a pointer element
		`Ꮡ(new Func<nint>[]{[GoFunc("main.func3")] () => 3}.slice())`,
		// CONTROL - an index is the first bracket, so the collection expression stays
		`var kept = slice<Func<nint>>([values[0], [GoFunc("main.func4")] () => 4]);`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing %q in:\n%s", want, mainCs)
		}
	}
}

// TestAttributedLiteralOpensCollectionReadsTheFirstBracket pins the predicate's lexing: strings, characters and
// comments are skipped, and only the first bracket decides.
func TestAttributedLiteralOpensCollectionReadsTheFirstBracket(t *testing.T) {
	for _, c := range []struct {
		elements string
		want     bool
	}{
		{`[GoFunc("f.func1")] () => 1]`, true},
		{"default, // 0\n    [GoFunc(\"init.func1\")] (nint n) => n]", true},
		{`() => x.label(), [MethodImpl(MethodImplOptions.NoInlining), GoFunc("f.func1")] () => 2]`, true},
		{`"[", [GoFunc("f.func1")] () => 1]`, true},
		{`'[', @"a""[", /* [ */ [GoFunc("f.func1")] () => 1]`, true},
		{`values[0], [GoFunc("f.func1")] () => 1]`, false},
		{`1, 2]`, false},
		{`new(Name: "x")]`, false},
	} {
		if got := attributedLiteralOpensCollection(c.elements); got != c.want {
			t.Errorf("attributedLiteralOpensCollection(%q) = %v, want %v", c.elements, got, c.want)
		}
	}
}
