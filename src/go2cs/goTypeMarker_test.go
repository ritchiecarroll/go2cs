// goTypeMarker_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the face lift's converter half for B and C (docs/PLAN-marker-comment-parity.md §5.2, §5.3): a
// converted Go type declaration carries no `[GoType]`. A plain Go type carries nothing, because go2cs-gen
// recognizes it by where it is declared and adds the attribute to its own generated part. A defined type
// carries its definition as a comment right after its name, or after its type-parameter list, where
// go2cs-gen reads it back (Common.GetDefinitionComment).

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestGoTypeMarkerRendersTheFaceLiftForm(t *testing.T) {
	for _, test := range []struct {
		definition, attribute, comment string
	}{
		// A plain Go type: nothing, ahead of it or after its name.
		{"", "", ""},
		// A defined type: its definition, the text the attribute argument held, as a comment.
		{"num:int64", "", " /*num:int64*/"},
		{"dyn", "", " /*dyn*/"},
		{"map[@string, nint]", "", " /*map[@string, nint]*/"},
		// A definition no block comment can hold keeps the attribute, which go2cs-gen reads first.
		{"[8]/* n */byte", `[GoType("[8]/* n */byte")] `, ""},
		{"a*/b", `[GoType("a*/b")] `, ""},
	} {
		attribute, comment := goTypeMarker(test.definition)

		if attribute != test.attribute || comment != test.comment {
			t.Errorf("goTypeMarker(%q) = (%q, %q), want (%q, %q)", test.definition, attribute, comment, test.attribute, test.comment)
		}
	}
}

func TestConvertedGoTypesCarryNoGoTypeAttribute(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/gtm\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type Point struct{ X, Y int }

type Shape interface{ Area() float64 }

type Duration int64

type IntPtr *int

type Names []string

type Block [4]byte

type Index map[string]int

type Pipe chan int

type Box[T any] struct{ V T }

type Number interface{ ~int | ~float64 }

func sum[T Number](values ...T) T {
	var total T
	for _, v := range values {
		total += v
	}
	return total
}

func local() int {
	type pair struct{ a, b int }
	type sizer interface{ Size() int }
	var s sizer
	_ = s
	return pair{1, 2}.a
}

func main() {
	var p IntPtr
	fmt.Println(Point{1, 2}, Duration(3), p == nil, Names{"a"}, Block{}, Index{"a": 1}, make(Pipe), Box[int]{4}, sum(1, 2), local())
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "gtm", "main.cs"))

	// The whole point: no converted declaration carries the attribute.
	if strings.Contains(mainCs, "[GoType") {
		t.Errorf("a converted declaration still carries [GoType]:\n%s", mainCs)
	}

	for _, want := range []string{
		"partial struct Point {",
		"partial interface Shape {",
		"partial struct Duration /*num:int64*/;",
		"partial class IntPtr /*ж<nint>*/;",
		"partial struct Names /*[]@string*/;",
		"partial struct Block /*[4]byte*/;",
		"partial struct Index /*map[@string, nint]*/;",
		"partial struct Pipe /*chan nint*/;",
		"partial struct Box<T> {",
		"partial struct local_pair /*dyn*/ {",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in main.cs:\n%s", want, mainCs)
		}
	}

	// A function-local interface lift: its definition after the lifted name.
	if !regexp.MustCompile(`partial interface local_sizer /\*dyn\*/ `).MatchString(mainCs) {
		t.Errorf("want the local interface lift `partial interface local_sizer /*dyn*/` in main.cs:\n%s", mainCs)
	}

	// A constraint interface: the CRTP `<ΔT>` list is the declaration's type-parameter list, so the
	// definition comment, its operator sets, follows the list's `>` (where go2cs-gen reads it), never the name.
	constraint := regexp.MustCompile(`partial interface Number` + regexp.QuoteMeta("<"+TypeT+">") + ` /\*operators = [^*]+\*/ `)

	if !constraint.MatchString(mainCs) {
		t.Errorf("want the constraint interface `partial interface Number<%s> /*operators = …*/` in main.cs:\n%s", TypeT, mainCs)
	}
}
