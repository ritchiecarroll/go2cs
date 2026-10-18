// unionNonOperatorMember_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the operator lift for a type-set constraint with a member that supports no operator (H7 class D,
// go1.26.9 crypto/x509 constraints.go's `nameConstraintsSet[T *net.IPNet | string, V net.IP | string]`). Go gives
// a type set an operator only when EVERY member supports it, and net.IP is a slice: it supports no +, no ==, no <.
// The slice member contributed nothing to the lifted set, so `IP | string` lifted string's operators alone, and
// `where V : IAdditionOperators<V, V, V>, IEqualityOperators<V, V, bool>, IComparisonOperators<V, V, bool>`
// rejected net.IP at every instantiation (CS0315). The same holds for an interface declaration's derived
// operators. The controls keep the operators every member does support: a numeric union, a union with a named
// numeric member, and string alone.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestATypeSetWithASliceMemberLiftsNoOperators(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/unm\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type IP []byte

type set[T *int | string, V IP | string] struct {
	vals []V
}

type ipOrString interface {
	IP | string
}

func first[V IP | string](vs []V) V { return vs[0] }

type Small int

func add[N int | int64](a, b N) N { return a + b }

func twice[N Small | int8](n N) N { return n + n }

func cat[S string](a, b S) S { return a + b }

func main() {
	s := set[*int, IP]{vals: []IP{{1}}}
	println(len(first(s.vals)), add(1, 2), twice(Small(3)), cat("a", "b"))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "unm", "main.cs"))

	for _, unwanted := range []string{
		"IAdditionOperators<V",
		"IEqualityOperators<V",
		"IComparisonOperators<V",
		"Derived operators: +",
		"Derived operators: ==",
	} {
		if strings.Contains(mainCs, unwanted) {
			t.Errorf("main.cs lifts an operator the slice member IP does not support (%q)\n--- main.cs ---\n%s", unwanted, mainCs)
		}
	}

	for _, want := range []string{
		"Derived operators: none",
		"IAdditionOperators<N, N, N>",
		"IAdditionOperators<S, S, S>",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("main.cs lacks %q\n--- main.cs ---\n%s", want, mainCs)
		}
	}
}
