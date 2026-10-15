// typedNilUnsafePointer_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the typed-nil boundary's THIRD arm (typedNilInterfaceBoxing.go,
// applyTypedNilUnsafePointerBox): an unsafe.Pointer entering an EMPTY interface carries its type
// even when nil. unsafe.Pointer is a CLASS in the managed model, so a nil one renders a C# null that
// boxes as nothing — reflect's TestIsZero reached `{unsafe.Pointer(nil), true}` as the ZERO Value.
// The controls carry as much weight as the positives: a conversion FROM a pointer or a uintptr
// constructs a Pointer and can never be null, and a NAMED type over unsafe.Pointer renders as its
// wrapper struct, so neither may take the accessor.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"testing"
)

func TestUnsafePointerIntoEmptyInterfaceCarriesItsTypedNil(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/unsafenil\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

type named unsafe.Pointer

var global int

// POSITIVES - each can render a C# null.
func nilConversion() any              { return unsafe.Pointer(nil) }
func fromVariable(p unsafe.Pointer) any { return p }
func identityConversion(p unsafe.Pointer) any { return unsafe.Pointer(p) }

// POSITIVES at the ARGUMENT-LIST boundary (convExprList's slot flags), a second route in: a positional
// any-typed struct field (reflect's TestIsZero row), an []any element, a variadic ...any argument, an
// append element.
type row struct {
	v    any
	want bool
}

func structField() []row                 { return []row{{(unsafe.Pointer)(nil), true}} }
func sliceElement(p unsafe.Pointer) []any { return []any{p} }
func variadicArg(p unsafe.Pointer)       { fmt.Println(p) }
func appendElement(p unsafe.Pointer) []any { return append([]any{}, p) }

// CONTROLS - each renders a constructed Pointer or a wrapper struct.
func fromPointer() any         { return unsafe.Pointer(&global) }
func fromUintptr(u uintptr) any { return unsafe.Pointer(u) }
func fromNamed(p named) any    { return p }

func main() {
	fmt.Println(nilConversion(), fromVariable(nil), identityConversion(nil), fromPointer(), fromUintptr(0), fromNamed(nil))
	fmt.Println(structField(), sliceElement(nil), appendElement(nil))
	variadicArg(nil)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "unsafenil", "main.cs"))
	accessor := regexp.QuoteMeta("@unsafe.Pointer." + TypedNilUnsafePointerAccessor + "(")

	// Each function's emission, from its signature line through the first closing brace at the
	// method's own indentation.
	bodyOf := func(name string) string {
		match := regexp.MustCompile(`(?m)^( *)[^\n]*\b` + name + `\([^\n]*\n(?s:.*?)^( *)\}\s*$`).FindString(mainCs)

		if match == "" {
			t.Fatalf("%s: not found in the emission:\n%s", name, mainCs)
		}

		return match
	}

	for _, positive := range []string{"nilConversion", "fromVariable", "identityConversion", "structField", "sliceElement", "variadicArg", "appendElement"} {
		if !regexp.MustCompile(accessor).MatchString(bodyOf(positive)) {
			t.Errorf("%s: expected the value to cross into `any` through `@unsafe.Pointer.%s(…)`; emission:\n%s",
				positive, TypedNilUnsafePointerAccessor, bodyOf(positive))
		}
	}

	for _, control := range []string{"fromPointer", "fromUintptr", "fromNamed"} {
		if regexp.MustCompile(accessor).MatchString(bodyOf(control)) {
			t.Errorf("%s: a rendering that can never be null must NOT take the accessor; emission:\n%s",
				control, bodyOf(control))
		}
	}
}
