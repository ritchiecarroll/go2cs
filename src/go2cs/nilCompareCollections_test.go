// nilCompareCollections_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards parity seat #24 (N1 stage 2): an untyped nil compared with a slice, a map or a chan emits golib's
// `nil`, as Go writes it, whatever spells the operand: a named type, a directional chan, a field, a deref, a
// variadic parameter or an index expression, on either side. A type parameter with a slice or map core keeps
// IsNil (C# binds no user-defined operator on a type parameter, CS8761).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestNilCompareRendersNilForSliceMapAndChanOperands(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/nilcoll\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type bytes []byte
type counts map[string]int
type events chan int

type bag struct {
	items []string
	index map[string]int
	done  chan struct{}
}

func named(b bytes, c counts, e events) bool { return b == nil || nil == c || e != nil }
func directional(s chan<- int, r <-chan int) bool { return s == nil && nil != r }
func fields(b bag) bool { return b.items == nil || b.index != nil || nil == b.done }
func deref(p *[]int) bool { return *p == nil }
func variadic(xs ...int) bool { return xs == nil }
func lookup(m map[string][]int) bool { return m["k"] != nil }
func mixed(err error, s []int) bool { return err != nil && s == nil }
func tpSlice[S ~[]E, E any](s S) bool { return s == nil }

func main() {
	_ = named(nil, nil, nil)
	_ = directional(nil, nil)
	_ = fields(bag{})
	_ = deref(new([]int))
	_ = variadic()
	_ = lookup(nil)
	_ = mixed(nil, nil)
	_ = tpSlice([]int(nil))
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

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "nilcoll", "main.cs")), "\r\n", "\n")

	for _, want := range []string{
		"return b == nil || nil == c || e != nil;", // named slice, map and chan, either side
		"return s == nil && nil != r;",             // directional chans
		"return b.items == nil || b.index != nil || nil == b.done;",
		"return p == nil;",               // a deref'd slice
		"return xs == nil;",              // a variadic parameter is a slice
		"return m[\"k\"] != nil;",        // an index expression
		"return err != nil && s == nil;", // beside an interface compare on one line
		"return s.IsNil;",                // a slice-core type parameter keeps IsNil
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in the emitted main.cs:\n%s", want, mainCs)
		}
	}

	if strings.Contains(mainCs, "= default") || strings.Contains(mainCs, "default =") || strings.Contains(mainCs, "default !=") {
		t.Errorf("no slice, map or chan compared to nil may read `default`:\n%s", mainCs)
	}
}
