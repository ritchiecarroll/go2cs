// structFieldEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards two struct-field emission rules internal/reflectlite's test suite was the first thing
// to reach (its typeTests table writes every field type PARENTHESIZED and passes bare
// keyword-led types to func fields):
//
//  1. A PARENTHESIZED fixed-size array field type — `x ([4]int32)` — declares the same array as
//     the bare spelling, so it must keep its `= new(N)` field initializer. Missing the
//     ast.ParenExpr wrapper silently dropped it: the zero instance carried a backing-less array
//     and every dims read (StructType synthesis, reflect field walks) answered [0]N.
//
//  2. A BARE keyword-led parameter type in a func-typed field — `func(chan *integer, *int8)` —
//     keeps its leading type keyword. The delegate lowering's name-stripping heuristic read
//     `chan` as a parameter NAME and the channel layer vanished from the delegate
//     (`Action<ж<integer>, ж<int8>>`); the rule is convertToCSResultList's, now shared: a
//     leading token is a name only when it is a plain identifier that is not a type-leading
//     keyword.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestStructFieldEmissionKeepsParenArrayDimsAndBareChanParams(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldemit\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type integer int

// The reflectlite typeTests shapes: a parenthesized fixed-size array field, a func field with a
// BARE chan-led parameter, and a func field with a NAMED parameter as the control.
var probe = struct {
	a ([4]int32)
	c func(chan *integer, *int8)
	n func(count int) bool
}{}

func main() {
	_ = probe
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "fieldemit", "main.cs"))

	// 1. The parenthesized array field keeps its dims-carrying initializer.
	if !strings.Contains(mainCs, "= new(4)") {
		t.Errorf("a parenthesized [4]int32 field must keep its `= new(4)` initializer:\n%s", mainCs)
	}

	// 2. The bare chan parameter keeps its channel layer...
	if !strings.Contains(mainCs, "channel<ж<integer>>") {
		t.Errorf("a bare `chan *integer` func-field parameter must render channel<ж<integer>>:\n%s", mainCs)
	}

	// ...and a NAMED parameter still strips its name (the heuristic's ordinary case).
	if strings.Contains(mainCs, "count") {
		t.Errorf("a named func-field parameter must strip the name from the delegate:\n%s", mainCs)
	}
}
