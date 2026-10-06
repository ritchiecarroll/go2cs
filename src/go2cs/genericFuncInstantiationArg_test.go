// genericFuncInstantiationArg_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the func arm of the typed-nil boundary (typedNilInterfaceBoxing.go,
// funcExprNeverRendersNull) for an EXPLICITLY INSTANTIATED generic function passed as an argument.
// `pick[string]` is an index expression whose type is a func signature; it renders as the bare method
// group `pick<@string>`, which C# gives a natural delegate type and which can no more be null than the
// bare identifier `pick` can. The arm used to cover the identifier and not the instantiation, so the
// instantiation took the nullable-func accessor, `(pick<@string>).OrTypedNilFunc()`, and C# refused
// member access on a method group (CS0119). The controls carry as much weight as the positives: an
// ordinary index into a slice or map of funcs IS nullable and must keep its accessor.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestInstantiatedGenericFuncArgumentIsNeverWrappedInTheTypedNilAccessor(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/instarg\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"reflect"
)

func pick[T any](a [3]T) T { return a[0] }

func pair[K comparable, V any](k K, v V) map[K]V { return map[K]V{k: v} }

func show(f any) { fmt.Println(reflect.TypeOf(f)) }

func apply(f func(any) string, v any) string { return f(v) }

func describe[T any](v T) string { return fmt.Sprint(v) }

// POSITIVES - an instantiation is a method group: one type argument, two type arguments, and each of
// the three argument boundaries (a named any parameter, a reflect.TypeOf call, a variadic ...any).
func oneTypeArgument() any { return reflect.TypeOf(pick[string]) }

func twoTypeArguments() any { return reflect.TypeOf(pair[string, int]) }

func namedAnyParameter() { show(pick[int]); show(pair[int, bool]) }

func variadicAnyArgument() { fmt.Printf("%T\n", pick[float64]) }

func parenthesized() any { return reflect.TypeOf((pick[string])) }

func returned() any { return pick[string] }

func funcTypedParameter() string { return apply(describe[any], 42) }

// CONTROL - already worked: bound to a variable first.
func boundFirst() any { pf := pick[string]; return pf }

// CONTROLS - an index into a slice or a map of funcs is an ordinary nullable value and keeps the
// accessor, as does a func-typed variable.
var table = []func(){func() {}}

var byName = map[string]func(){"x": func() {}}

func sliceElement() any { return table[0] }

func mapElement() any { return byName["x"] }

func variable(f func()) any { return f }

func main() {
	fmt.Println(oneTypeArgument(), twoTypeArguments(), parenthesized(), returned(), funcTypedParameter(), boundFirst())
	fmt.Println(sliceElement(), mapElement(), variable(nil))
	namedAnyParameter()
	variadicAnyArgument()
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "instarg", "main.cs"))
	accessor := TypedNilFuncAccessor

	// Each function's emission, from its signature line through the first closing brace at the
	// method's own indentation.
	bodyOf := func(name string) string {
		match := regexp.MustCompile(`(?m)^( *)[^\n]*\b` + name + `\([^\n]*\n(?s:.*?)^( *)\}\s*$`).FindString(mainCs)

		if match == "" {
			t.Fatalf("%s: not found in the emission:\n%s", name, mainCs)
		}

		return match
	}

	// The exact bare method-group spelling each positive must carry into its boundary.
	positives := map[string]string{
		"oneTypeArgument":     "reflect.TypeOf(pick<@string>)",
		"twoTypeArguments":    "reflect.TypeOf(pair<@string, nint>)",
		"namedAnyParameter":   "show(pick<nint>)",
		"variadicAnyArgument": `fmt.Printf("%T\n"u8, pick<float64>)`,
		"parenthesized":       "reflect.TypeOf((pick<@string>))",
		"returned":            "pick<@string>",
		"funcTypedParameter":  "apply(describe<any>,",
	}

	for name, want := range positives {
		body := bodyOf(name)

		if !strings.Contains(body, want) {
			t.Errorf("%s: expected the bare method group %q; emission:\n%s", name, want, body)
		}

		if strings.Contains(body, accessor) {
			t.Errorf("%s: an instantiated generic func is a method group and must NOT take `.%s`; emission:\n%s",
				name, accessor, body)
		}
	}

	if !strings.Contains(bodyOf("namedAnyParameter"), "show(pair<nint, bool>)") {
		t.Errorf("namedAnyParameter: expected the two-type-argument form passed bare; emission:\n%s", bodyOf("namedAnyParameter"))
	}

	if !strings.Contains(bodyOf("boundFirst"), "var pf = pick<@string>;") {
		t.Errorf("boundFirst: the control must still bind the method group directly; emission:\n%s", bodyOf("boundFirst"))
	}

	for _, control := range []string{"sliceElement", "mapElement", "variable"} {
		if !strings.Contains(bodyOf(control), accessor) {
			t.Errorf("%s: a nullable func value must keep `.%s`; emission:\n%s", control, accessor, bodyOf(control))
		}
	}
}
