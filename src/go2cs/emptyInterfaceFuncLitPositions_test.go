// emptyInterfaceFuncLitPositions_test.go - Gbtc
// Copyright (c) 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the func-literal result type at EVERY position whose Go destination is an empty interface
// (markEmptyInterfaceFuncLits): a return, a declared variable, a conversion, an unkeyed composite
// element, a positional struct field, a channel send and a multi-name var.
//
// untypedInterfaceFuncLit_test.go pins the argument and keyed-composite slots, which thread the
// LambdaContext flag. The positions here never did, so the literal reached convFuncLit unmarked and
// C# typed it from its body: mapstructure's decode_hooks.go returns a multi-result literal through a
// DecodeHookFunc (a NAMED empty interface) result, every arm's tuple holds a `default!`, and the bare
// lambda is CS8917 (40 of the package's 43 build errors). The single untyped-constant result is the
// silent half: it compiled as Func<int> where Go's dynamic type is func() int64.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestEmptyInterfaceFuncLitPositions pins the declared result type on a func literal at each
// empty-interface position, and the CONTROL that a concrete func-typed return keeps its bare,
// target-typed lambda.
func TestEmptyInterfaceFuncLitPositions(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), `module example.com/eifl

go 1.23
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"errors"
	"fmt"
)

// mapstructure's DecodeHookFunc shape: a NAMED empty interface.
type Hook interface{}

type holder struct{ h interface{} }

func retNamed() Hook {
	return func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 3, nil
	}
}

func retConst() interface{} { return func() int64 { return 1 } }

// CONTROL: a concrete func-typed result IS a delegate target.
func retTyped() func() int64 { return func() int64 { return 9 } }

func main() {
	var declared interface{} = func() int64 { return 2 }
	converted := Hook(func() int64 { return 3 })
	elements := []interface{}{func() int64 { return 4 }}
	positional := holder{func() int64 { return 5 }}
	ch := make(chan interface{}, 1)
	ch <- func() int64 { return 6 }
	var first, second interface{} = func() int64 { return 7 }, func() int64 { return 8 }
	fmt.Println(retNamed(), retConst(), retTyped(), declared, converted, elements, positional, <-ch, first, second)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "eifl", "main.cs"))

	for _, want := range []string{
		"return (any, error) (nint x) => {", // return through a NAMED empty interface (mapstructure)
		"return int64 () => 1;",             // return through interface{}
		"any declared = int64 () => 2;",     // declared variable
		"(Hook)(int64 () => 3)",             // conversion
		"new any[]{int64 () => 4}",          // unkeyed composite element
		"new holder(int64 () => 5)",         // positional struct field
		"ꟷ(int64 () => 6)",                  // channel send
		"any first = int64 () => 7;",        // multi-name var
		"any second = int64 () => 8;",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("func literal at an empty-interface position must state its Go result type (%q): %s", want, mainCs)
		}
	}

	// CONTROL: the concrete func-typed result keeps the bare lambda its delegate target types.
	if !strings.Contains(mainCs, "return () => 9;") {
		t.Errorf("a concrete func-typed return must keep its target-typed lambda unchanged: %s", mainCs)
	}
}
