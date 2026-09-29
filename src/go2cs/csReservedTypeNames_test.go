// csReservedTypeNames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards side seat S2, the C# 15 identifier escape (ruled 2026-09-29): C# 15 makes `closed` and `union`
// contextual keywords that bite only at a TYPE name (a type or alias named `closed` is CS9380) and at the
// START of a member declaration (`union F;` parses as a union declaration, CS9370; `closed f;` as a
// modifier, CS1519) -- and a Go identifier can reach member start only as a declared TYPE. `safe` is the
// same shape under preview. So a package that DECLARES a type, alias or type parameter so named has it
// Δ-renamed package-wide (the Δfile / emitterSpelledTypeNames precedent), and every other identifier keeps
// its bare spelling: a field, local, parameter, method or selector named `closed` is unaffected, because
// statements and expressions do not change meaning. Each member of the predicate's reach (a type, an
// alias, a type parameter) sits in its own package so one cannot mask another. The `with(` rule (a
// collection-expression element starting `with(` is now the arguments element) needs no code: the
// converter emits a Go composite literal as an array initializer, never a collection expression, which
// the control asserts. Emission only -- S1 pins C# 14, so no LangVersion 15 compile runs here.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestCSharp15ReservedTypeNamesAreRenamedWhereTheyBite(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/s2\n\ngo 1.23\n")

	// A TYPE named after each keyword, with uses in every position.
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/s2/alias"
	"example.com/s2/plain"
	"example.com/s2/tparam"
)

type closed struct{ n int }

type union int

type safe bool

type holder struct {
	c closed
	u union
}

func (c closed) twice() int { return c.n * 2 }

func main() {
	var c closed
	var u union = 3
	var s safe
	h := holder{c: closed{n: 1}, u: 2}
	fmt.Println(c.twice(), u, s, h.u, alias.Use(), tparam.Pick(4), plain.Run())
}
`)

	// An ALIAS named closed (CS9380 bans aliases too).
	writeModuleFile(t, filepath.Join(appDir, "alias", "alias.go"), `package alias

type closed = int

func Use() closed { return 5 }
`)

	// A TYPE PARAMETER named union, in a package that declares no such type.
	writeModuleFile(t, filepath.Join(appDir, "tparam", "tparam.go"), `package tparam

func Pick[union any](v union) union { return v }
`)

	// CONTROL: no type so named, so every identifier keeps its bare spelling.
	writeModuleFile(t, filepath.Join(appDir, "plain", "plain.go"), `package plain

type box struct {
	closed bool
	union  int
	safe   bool
}

func with() int { return 1 }

func g() int { return 2 }

func Run() int {
	closed := true
	b := box{closed: closed, union: 7}
	xs := []int{with(), g()}
	if b.closed && !b.safe {
		return b.union + len(xs)
	}
	return 0
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

	emitted := func(parts ...string) string {
		return readGenerated(t, filepath.Join(append([]string{options.go2csPath, "src", "example.com"}, parts...)...))
	}

	expect := func(label, cs string, wants []string) {
		for _, want := range wants {
			if !strings.Contains(cs, want) {
				t.Errorf("%s: want %q in the emission:\n%s", label, want, cs)
			}
		}
	}

	mainCs := emitted("s2", "main.cs")
	expect("type names", mainCs, []string{
		"partial struct Δclosed", "partial struct Δunion", "partial struct Δsafe",
		"Δclosed c", "Δunion u", "Δsafe s",
	})

	// No bare declaration or member-start use may survive (the two positions C# 15 changes).
	if bad := regexp.MustCompile(`(?m)partial struct (closed|union|safe)\b|^\s*(internal |public )?(closed|union|safe) [A-Za-z_]`).FindString(mainCs); bad != "" {
		t.Errorf("type names: a bare reserved spelling survives in a breaking position (%q):\n%s", strings.TrimSpace(bad), mainCs)
	}

	expect("alias", emitted("s2", "alias", "alias.cs"), []string{"Δclosed"})
	expect("type parameter", emitted("s2", "tparam", "tparam.cs"), []string{"Pick<Δunion>(Δunion v)"})

	// CONTROL: the bare spelling everywhere else, and the slice literal stays an array initializer.
	plainCs := emitted("s2", "plain", "plain.cs")
	expect("control", plainCs, []string{"bool closed;", "nint union;", "bool safe;", "new nint[]{with(), g()}"})

	if strings.Contains(plainCs, "Δclosed") || strings.Contains(plainCs, "Δunion") || strings.Contains(plainCs, "Δsafe") || strings.Contains(plainCs, "[with(") {
		t.Errorf("control: a package declaring no such type must keep every spelling bare:\n%s", plainCs)
	}
}
