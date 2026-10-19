// inlineConstraintPublished_test.go - Gbtc
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

// An inline constraint naming its own parameter, declared in ANOTHER package: `lib.Ord[T interface{ Less(T) bool }]`
// and `lib.Min[T interface{ Less(T) bool }]`, instantiated from main. lib lifts each to a generic interface (`Ord_T<T>`,
// `Min_T<T>`), but an importer could not name the lift, so it kept the anonymous form: every value instantiation was
// CS0315 and every pointer one CS0311. lib now publishes each lift BY DECLARATION (GoInlineConstraintLift: owner, type
// parameter index, lift), and main reads it, finding the declaration by the type parameter itself: the value records
// its implementation of the lift and the pointer takes the lift's generic proxy (foreignConstraintProxy).
//
// Control: lib.Show's inline constraint does not capture its parameter, so nothing is published for it.

func TestForeignInlineConstraintTakesThePublishedLift(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fic\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

type Ord[T interface{ Less(T) bool }] struct{ V T }

func (o Ord[T]) Below(t T) bool { return o.V.Less(t) }

func Min[T interface{ Less(T) bool }](a, b T) T {
	if b.Less(a) {
		return b
	}

	return a
}

func Show[T interface{ Name() string }](t T) string { return t.Name() }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/fic/lib"
)

type age int

func (a age) Less(b age) bool { return a < b }

type node struct{ k int }

func (n *node) Less(o *node) bool { return n.k < o.k }

func main() {
	fmt.Println(lib.Ord[age]{V: 3}.Below(5), lib.Ord[*node]{V: &node{1}}.Below(&node{0}), lib.Min(age(2), age(1)))
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

	dir := filepath.Join(options.go2csPath, "src", "example.com", "fic")
	libInfo := readGenerated(t, filepath.Join(dir, "lib", "package_info.cs"))
	mainCS := readGenerated(t, filepath.Join(dir, "main.cs"))
	mainInfo := readGenerated(t, filepath.Join(dir, "package_info.cs"))

	for _, arm := range []struct {
		name, text, want string
	}{
		{"lib publishes the type's lift", libInfo, `[assembly: GoInlineConstraintLift("Ord", 0, "Ord_T")]`},
		{"lib publishes the function's lift", libInfo, `[assembly: GoInlineConstraintLift("Min", 0, "Min_T")]`},
		{"a value records its implementation of the lift", mainInfo, "[assembly: GoImplement<age, go.example.com.fic.lib_package.Ord_T<age>>]"},
		{"a function's value argument records it too", mainInfo, "[assembly: GoImplement<age, go.example.com.fic.lib_package.Min_T<age>>]"},
		{"a pointer takes the lift's generic proxy", mainCS, "new lib.Ord<lib.жOrd_T<node>>("},
		{"and binds it", mainInfo, "[assembly: GoImplement<node, go.example.com.fic.lib_package.Ord_T<node>>(ConstraintProxy = true)]"},
	} {
		if !strings.Contains(arm.text, arm.want) {
			t.Errorf("%s: missing %s", arm.name, arm.want)
		}
	}

	if strings.Contains(libInfo, `GoInlineConstraintLift("Show"`) {
		t.Errorf("control: a lift that captures no parameter is not published by declaration")
	}
}
