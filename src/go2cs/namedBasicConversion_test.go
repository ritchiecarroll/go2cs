// namedBasicConversion_test.go - Gbtc
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

// A conversion between two NAMED types over the same string or bool underlying has no direct C#
// operator — each [GoType] wrapper converts only to and from its underlying — so it must hop through
// the underlying, as the numeric twin already does. Emitted as a plain cast it is CS0030 (go-cmp's
// example_test `myString(in)`, with `type myString otherString` and `type otherString string`).
func TestNamedStringAndBoolConversionsHopThroughTheUnderlying(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/namedbasic\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type A string
type B string
type otherString string
type pkgString otherString
type T bool
type U bool

func main() {
	a := A("abc")
	b := B(a)
	o := otherString("xyz")
	p := pkgString(o)
	t := T(true)
	u := U(t)
	type N int
	type M N
	fmt.Println(b, p, otherString(p), u, M(N(7)))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "namedbasic", "main.cs"))

	for _, want := range []string{
		"((B)(@string)a)",           // sibling named strings
		"((pkgString)(@string)o)",   // a same-package type defined over a named string
		"((otherString)(@string)p)", // and back
		"((U)(bool)t)",              // sibling named bools
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing %q in:\n%s", want, mainCs)
		}
	}
}
