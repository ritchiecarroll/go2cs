// variadicGlobalAliasElement_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the referent of a variadic alias (`using ꓸꓸꓸT = Span<…>;`) whose element is a same-package
// DEFINED type over an interface. visitTypeSpec emits such a type as a `global using` alias, never a
// member of the package class (`type DecodeHookFunc interface{}` -> `global using DecodeHookFunc =
// object;`), but variadicElementParts qualified every same-package named element with the package
// class -- `Span<mapstructure_package.DecodeHookFunc>`, CS0426, github.com/mitchellh/mapstructure
// v1.5.0's only build error. The census of the other spellings (slice, map, chan, pointer, array,
// func, struct field, variadic of a slice) found ONLY the variadic alias qualifying it.
//
// The CONTROLS are the same-package elements that ARE package-class members and must stay qualified:
// a named struct and an interface DEFINITION with methods (a real nested C# interface).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertVariadicGlobalAliasFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/variadicalias\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"io"
	"strings"
)

// POSITIVE 1 -- mapstructure's shape: a defined type over an inline EMPTY interface.
type Hook interface{}

// POSITIVE 2 -- a defined type over a NAMED interface (the same global-alias route).
type Source io.Reader

// CONTROL 1 -- a named struct: a package-class member.
type box struct{ n int }

// CONTROL 2 -- an interface DEFINITION with methods: a real nested C# interface.
type Shape interface{ Area() int }

type square struct{ side int }

func (s square) Area() int { return s.side * s.side }

func hooks(hs ...Hook) int       { return len(hs) }
func sources(ss ...Source) int   { return len(ss) }
func boxes(bs ...box) int        { return len(bs) }
func shapes(ss ...Shape) int     { return len(ss) }

func main() {
	fmt.Println(hooks(1, "two"), sources(strings.NewReader("x")), boxes(box{1}), shapes(square{2}))
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "variadicalias", "main.cs"))
}

func TestVariadicAliasOfAGlobalAliasElementIsNotPackageQualified(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertVariadicGlobalAliasFixture(t)

	aliasLine := func(element string) string {
		t.Helper()

		for _, line := range strings.Split(mainCs, "\n") {
			if strings.HasPrefix(strings.TrimSpace(line), "using ꓸꓸꓸ"+element+" = ") {
				return strings.TrimSpace(line)
			}
		}

		t.Fatalf("no variadic alias for %s; emission:\n%s", element, mainCs)
		return ""
	}

	for _, element := range []string{"Hook", "Source"} {
		if line := aliasLine(element); line != "using ꓸꓸꓸ"+element+" = Span<"+element+">;" {
			t.Errorf("%s is a global alias, not a package-class member: %s", element, line)
		}
	}

	for _, element := range []string{"box", "Shape"} {
		if line := aliasLine(element); !strings.Contains(line, "Span<main_package."+element+">") {
			t.Errorf("%s is a package-class member and must stay qualified: %s", element, line)
		}
	}
}
