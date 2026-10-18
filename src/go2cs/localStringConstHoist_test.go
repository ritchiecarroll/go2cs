// localStringConstHoist_test.go - Gbtc
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

const localStringConstFixture = `package lsc

const pkgConst = "package-level"

// a package-level initializer that reaches a function with a local const: relocated (§4.4)
var greeting = makeGreeting()

func makeGreeting() string {
	const hello = "hello"
	return hello
}

func atoi(s string) string {
	const fnAtoi = "Atoi"
	return fnAtoi + s
}

func twice() string {
	const fnAtoi = "second"
	return fnAtoi
}

func empty() int {
	const e = ""
	return len(e)
}

func ints() int {
	const n = 3
	return n
}

var fromInit string

func init() {
	const once = "init-only"
	fromInit = once
}

func use() string { return pkgConst + greeting + fromInit }
`

// TestLocalStringConstHoistsUnderItsOwnName controls arm C (DESIGN-string-literal-allocation.md §8) both
// ways: a function-local string const is hoisted to one `static readonly` field named after the const (the
// big-const pattern: ᶜ, with a package-wide ordinal) that the local copies, and the function joins the
// hoisted-field readers, so a package-level initializer reaching it is relocated into the ordered static
// constructor. A package-level const, an empty const, a non-string const and a `func init()` const are
// not hoisted.
//
// RED at master 4c53b02a0a: every local string const emitted `@string fnAtoi = "Atoi"u8;` in the body.
func TestLocalStringConstHoistsUnderItsOwnName(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/lsc\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lsc.go"), localStringConstFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	pkgDir := filepath.Join(options.go2csPath, "src", "example.com", "lsc")
	emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(pkgDir, "lsc.cs")), "\r\n", "\n")

	for _, want := range []string{
		"private static readonly @string fnAtoiᶜ = \"Atoi\"u8;",
		"private static readonly @string fnAtoiᶜ1 = \"second\"u8;", // the same name in a second function
		"private static readonly @string helloᶜ = \"hello\"u8;",
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want the hoisted field %q:\n%s", want, emitted)
		}
	}

	for decl, want := range map[string]string{
		"internal static @string atoi(":         "@string fnAtoi = fnAtoiᶜ;",
		"internal static @string twice(":        "@string fnAtoi = fnAtoiᶜ1;",
		"internal static @string makeGreeting(": "@string hello = helloᶜ;",
		"internal static nint empty(":           "@string e = \"\"u8;", // empty: not hoisted
		"internal static void init(":            "@string once = \"init-only\"u8;",
	} {
		if body := liftFunctionBody(t, emitted, decl); !strings.Contains(body, want) {
			t.Errorf("%s: want %q:\n%s", decl, want, body)
		}
	}

	for _, refuse := range []string{"pkgConstᶜ", "onceᶜ", "eᶜ", "nᶜ"} {
		if strings.Contains(emitted, refuse) {
			t.Errorf("%s must not be hoisted:\n%s", refuse, emitted)
		}
	}

	if !strings.Contains(emitted, "static readonly @string pkgConst = ") {
		t.Errorf("the package-level const keeps its own static readonly field:\n%s", emitted)
	}

	// the relocation: greeting's initializer runs from the ordered static constructor, after helloᶜ's
	// field initializer, rather than as a field initializer that could read it first
	if !strings.Contains(emitted, "internal static void initᴛgreeting() { greeting = makeGreeting(); }") {
		t.Errorf("greeting's initializer must be relocated (it reaches a hoisted const field):\n%s", emitted)
	}
}
