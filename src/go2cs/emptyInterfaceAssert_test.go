// emptyInterfaceAssert_test.go - Gbtc
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

// An assertion to the anonymous empty interface holds for every non-nil dynamic value and fails only
// on a nil interface (panic in the one-value form, false in the comma-ok form): `any` exactly. Written
// as the type literal `interface{}` it was LIFTED to a dynamic interface -- objx's
// `v.data.(interface{})` emitted `_<Inter_type>()` beside a declared `partial interface Inter_type` --
// which no value implements, so every non-nil assert failed (objx TestInter, TestIsInter). Both forms
// must emit `_<any>`, exactly as the identifier `any` already does, and nothing may be lifted for it.
func TestEmptyInterfaceAssertIsAny(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/emptyassert\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type Value struct{ data interface{} }

func (v *Value) Inter(optionalDefault ...interface{}) interface{} {
	if s, ok := v.data.(interface{}); ok {
		return s
	}
	if len(optionalDefault) == 1 {
		return optionalDefault[0]
	}
	return nil
}

func (v *Value) MustInter() interface{} {
	return v.data.(interface{})
}

func (v *Value) IsInter() bool {
	_, ok := v.data.(interface{})
	return ok
}

func (v *Value) IsAny() bool {
	_, ok := v.data.(any)
	return ok
}

func main() {
	s := &Value{data: "something"}
	n := &Value{}
	fmt.Println(s.Inter(), s.MustInter(), s.IsInter(), s.IsAny())
	fmt.Println(n.Inter("default"), n.IsInter(), n.IsAny())
	defer func() { fmt.Println("recovered:", recover() != nil) }()
	n.MustInter()
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

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "emptyassert")
	mainCs := readGenerated(t, filepath.Join(outDir, "main.cs"))
	packageInfo := readGenerated(t, filepath.Join(outDir, "package_info.cs"))

	// Both forms of the literal, and the identifier, which is the control.
	for _, want := range []string{
		"v.data._<any>(ᐧ)",
		"return v.data._<any>();",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing %q in:\n%s", want, mainCs)
		}
	}

	if count := strings.Count(mainCs, "v.data._<any>(ᐧ)"); count != 3 {
		t.Errorf("%d comma-ok asserts to any, want 3 (Inter, IsInter and the IsAny control) in:\n%s", count, mainCs)
	}

	// Nothing is lifted for `interface{}` (its signature, hex-encoded, is 696e746572666163657b7d).
	if strings.Contains(packageInfo, "696e746572666163657b7d") || strings.Contains(mainCs, "_type>(") {
		t.Errorf("the empty interface is lifted to a dynamic type in:\n%s\n%s", packageInfo, mainCs)
	}
}
