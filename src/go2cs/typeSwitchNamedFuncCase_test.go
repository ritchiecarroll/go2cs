// typeSwitchNamedFuncCase_test.go - Gbtc
// Copyright (c) 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a type-switch case on a defined func type with no methods. Every defined func type declares a
// delegate of its own, so the case names it, as a type assertion does: mapstructure's DecodeHookExec
// (`case DecodeHookFuncType:`). Two defined func types of one signature are two Go types, so their
// cases never collide: each keeps its label and its body, where collapsing both to the base delegate
// merged identical bodies and made differing ones a CS8120 compile error.
//
// A func-type case takes the interface cases' guard form, `case {} Δf when Δf._<T>(out var f):`. A
// typed nil func held in an interface is its func type's carrier, which no C# type pattern matches,
// so the pattern form sent it to the default arm. Two unnamed delegates sharing a first type
// argument stay apart: the duplicate-mapped-case merge once cut its key at the first space, which
// truncated `Func<nint, nint>` and `Func<nint, @string>` to one key.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestTypeSwitchNamedFuncCase pins a defined func type's case in its guard form, with and without a
// binding, under its own name; two defined func types of one signature kept apart whether their bodies
// differ or match; and two distinct unnamed delegates sharing a first type argument kept apart.
func TestTypeSwitchNamedFuncCase(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), `module example.com/tscf

go 1.23
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type Hook func(int) (interface{}, error)
type Name func(string) int
type Left func(bool) bool
type Right func(bool) bool

func bound(raw interface{}) string {
	switch f := raw.(type) {
	case Hook:
		out, _ := f(1)
		return fmt.Sprint(out)
	}
	return ""
}

func unbound(raw interface{}) string {
	switch raw.(type) {
	case Name:
		return "name"
	}
	return ""
}

// Same signature, different bodies: two Go types, two cases.
func differ(raw interface{}) string {
	switch raw.(type) {
	case Left:
		return "left"
	case Right:
		return "right"
	}
	return ""
}

// Same signature, same body: still two Go types, two cases.
func same(raw interface{}) string {
	switch raw.(type) {
	case Left:
		return "either"
	case Right:
		return "either"
	}
	return ""
}

// Two DIFFERENT delegates sharing a first type argument, same body: never merged.
func distinct(raw interface{}) string {
	switch raw.(type) {
	case func(int) int:
		return "fn"
	case func(int) string:
		return "fn"
	}
	return ""
}

func main() {
	fmt.Println(bound(nil), unbound(nil), differ(nil), same(nil), distinct(nil))
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

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "tscf", "main.cs"))

	// A func-type case is a guard label: `case {} <temp> when <temp>._<delegate>(out var <binding>):`.
	guard := func(temp string, delegate string, binding string) string {
		return "    case {} " + temp + " when " + temp + "._<" + delegate + ">(out var " + binding + "):"
	}

	unbound := TempVarMarker + "0"

	for _, want := range []string{
		guard(ShadowVarMarker+"f", "Hook", "f"), // a binding (mapstructure's shape)
		guard(unbound, "Name", "_"),             // no binding
		guard(unbound, "Func<nint, nint>", "_"), // distinct delegates sharing a first type argument
		guard(unbound, "Func<nint, @string>", "_"),
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("expected the case label %q: %s", want, mainCs)
		}
	}

	// differ and same each keep both labels: Left and Right are two Go types whatever their bodies.
	for _, name := range []string{"Left", "Right"} {
		if got := strings.Count(mainCs, guard(unbound, name, "_")); got != 2 {
			t.Errorf("want 2 guard labels on %s (one in differ, one in same), got %d: %s", name, got, mainCs)
		}
	}

	if strings.Contains(mainCs, "merged with an earlier case") {
		t.Errorf("no two cases here map to one C# type, so none may merge: %s", mainCs)
	}

	for _, delegate := range []string{"Func<nint, (any, error)>", "Func<@string, nint>", "Func<bool, bool>"} {
		if strings.Contains(mainCs, "<"+delegate+">") || strings.Contains(mainCs, "case "+delegate) {
			t.Errorf("a defined func type's case names the type, never its base delegate %s: %s", delegate, mainCs)
		}
	}
}
