// inlineConstraintClosedRecord_test.go - Gbtc
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

// A generic type whose inline constraint names its own parameter (`type Ord[T interface{ Less(T) bool }]`)
// lifts it to `Ord_T<T>`, and C# checks `where T : Ord_T<T>` at every instantiation. A value argument needs
// the record `GoImplement<age, Ord_T<age>>`, and a pointer one the constraint proxy `nodeжOrd_T` with its
// ConstraintProxy record, exactly as a NAMED constraint of that shape (`Lesser[T]`) already gets. Neither
// was made, because the instantiation paths read only a named constraint: every `Ord[age]` was CS0315 and
// every `Ord[*node]` CS0311. inlineConstraintNamed now gives the inline form the named view.
//
// The instantiations live in a different file from the declaration, so whichever file the converter
// visits first, the records are the same: a site that arrives before the lift is named defers to the
// barrier. Control: the named Lesser[T] equivalent, which recorded before too.

func TestInlineConstraintIsClosedOverItsInstantiation(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/ordclose\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "a_use.go"), `package main

import "fmt"

func main() {
	a := Ord[age]{v: 3}
	b := Ord[*node]{v: &node{1}}
	a2 := Ord2[age]{v: 3}
	b2 := Ord2[*node]{v: &node{1}}
	fmt.Println(a.Below(5), b.Below(&node{0}), a2.Below(5), b2.Below(&node{0}))
}
`)
	writeModuleFile(t, filepath.Join(appDir, "z_ord.go"), `package main

type Ord[T interface{ Less(T) bool }] struct{ v T }

func (o Ord[T]) Below(t T) bool { return o.v.Less(t) }

type Lesser[T any] interface{ Less(T) bool }

type Ord2[T Lesser[T]] struct{ v T }

func (o Ord2[T]) Below(t T) bool { return o.v.Less(t) }

type age int

func (a age) Less(b age) bool { return a < b }

type node struct{ k int }

func (n *node) Less(o *node) bool { return n.k < o.k }
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

	dir := filepath.Join(options.go2csPath, "src", "example.com", "ordclose")
	info := readGenerated(t, filepath.Join(dir, "package_info.cs"))
	use := readGenerated(t, filepath.Join(dir, "a_use.cs"))

	for _, arm := range []struct {
		name, text, want string
	}{
		{"value record", info, "[assembly: GoImplement<age, Ord_T<age>>]"},
		{"pointer proxy record", info, "[assembly: GoImplement<node, Ord_T<node>>(ConstraintProxy = true)]"},
		{"pointer proxy at the instantiation", use, "new Ord<nodeжOrd_T>("},
		{"control: named value record", info, "[assembly: GoImplement<age, Lesser<age>>]"},
		{"control: named proxy at the instantiation", use, "new Ord2<nodeжLesser>("},
	} {
		if !strings.Contains(arm.text, arm.want) {
			t.Errorf("%s: missing %s", arm.name, arm.want)
		}
	}

	for name, text := range map[string]string{"package_info.cs": info, "a_use.cs": use} {
		if strings.Contains(text, dynamicTypeMarkerPrefix) {
			t.Errorf("%s carries an unresolved deferred marker", name)
		}
	}

	if t.Failed() {
		t.Logf("package_info.cs:\n%s\na_use.cs:\n%s", info, use)
	}
}
