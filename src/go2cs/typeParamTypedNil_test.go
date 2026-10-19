// typeParamTypedNil_test.go - Gbtc
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

// A value of TYPE-PARAMETER type takes golib's OrTypedNilOf on its way into a REAL empty interface, so a nil pointer
// or func type argument keeps its dynamic type (typedNilInterfaceBoxing.go, applyTypedNilTypeParamBox). A slot of
// type-parameter type is NOT one: `eq(v1, v2)` with eq func(E, E) bool, `Index(s, v)` and `s[i] = v` all take E, and
// wrapping there turns E into object -- the first cut did exactly that across slices, maps and iter (26 -stdlib
// files), because several emission paths count a type-parameter slot as an empty interface.
func TestTypeParamValueTakesTheTypedNilOnlyIntoARealEmptyInterface(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/tptn\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type P struct{ x int }

func intoAny[E any](e E) any { return e }

func intoArgs[E any](e E) string { return fmt.Sprint(e) }

func intoSlot[E any](e E) []any { return []any{e} }

func intoTypeParamSlot[E comparable](s []E, v E, eq func(E, E) bool) int {
	for i := range s {
		if eq(s[i], v) {
			return i
		}
	}
	return -1
}

func passThrough[E comparable](s []E, v E) int { return intoTypeParamSlot(s, v, func(a, b E) bool { return a == b }) }

func store[E any](s []E, v E) { s[0] = v }

func main() {
	var p *P
	fmt.Println(intoAny(p) == nil, intoArgs(p), intoSlot(p), passThrough([]*P{p}, p))
	store([]*P{nil}, p)
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

	cs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "tptn", "main.cs"))

	body := func(function string) string {
		start := strings.Index(cs, " "+function+"<")

		if start < 0 {
			t.Fatalf("the fixture no longer emits %s:\n%s", function, cs)
		}

		end := strings.Index(cs[start:], "\n}\n")

		if end < 0 {
			end = strings.Index(cs[start:], "\r\n}\r\n")
		}

		return cs[start : start+end]
	}

	// Into a REAL empty interface: a return, the fmt family's ...any, an []any element.
	for _, function := range []string{"intoAny", "intoArgs", "intoSlot"} {
		if got := body(function); !strings.Contains(got, "(e)."+TypedNilTypeParamAccessor) {
			t.Errorf("%s: a type-parameter value into an empty interface does not take %s:\n%s", function, TypedNilTypeParamAccessor, got)
		}
	}

	// Into a TYPE-PARAMETER slot: a func value's E parameter, a generic callee's E parameter, an []E element.
	for _, function := range []string{"intoTypeParamSlot", "passThrough", "store"} {
		if got := body(function); strings.Contains(got, TypedNilTypeParamAccessor) {
			t.Errorf("%s: a type-parameter value into a TYPE-PARAMETER slot took %s, which turns E into object:\n%s", function, TypedNilTypeParamAccessor, got)
		}
	}
}
