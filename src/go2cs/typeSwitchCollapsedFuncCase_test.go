// typeSwitchCollapsedFuncCase_test.go - Gbtc
// Copyright (c) 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a type-switch case on a METHODLESS named func type. Such a type is rendered as its base
// delegate everywhere and declares no type of its own (methodlessNamedFuncSignature), so the case
// must spell the delegate, as a type assertion on it already does. mapstructure's DecodeHookExec
// (`case DecodeHookFuncType:`) emitted the Go name: CS0246, and CS0103 without a binding.
//
// Collapsing makes two Go types share one C# type, so the duplicate-mapped-case merge decides what a
// colliding switch becomes: merged when the bodies are identical, both labels kept (a loud CS8120)
// when they differ. That merge keys on the label's C# type, which it cut at the FIRST space, so a
// generic type's own `, ` truncated `Func<nint, nint>` and `Func<nint, @string>` to one key.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestTypeSwitchCollapsedFuncCase pins the delegate spelling of a collapsed case (with and without
// a binding), both labels of a colliding pair whose bodies differ, the merge of a colliding pair
// whose bodies match, and two distinct delegates sharing a first type argument kept apart.
func TestTypeSwitchCollapsedFuncCase(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), `module example.com/tscf

go 1.23
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type Hook func(int) (interface{}, error)
type Name func(string) int
type Left func(bool) bool
type Right func(bool) bool

func bound(raw interface{}) string {
	switch f := raw.(type) {
	case Hook:
		out, _ := f(1)
		return fmt.Sprint(out)
	}
	return ""
}

func unbound(raw interface{}) string {
	switch raw.(type) {
	case Name:
		return "name"
	}
	return ""
}

// Same signature, different bodies: both labels stay, and C# rejects the second (CS8120).
func differ(raw interface{}) string {
	switch raw.(type) {
	case Left:
		return "left"
	case Right:
		return "right"
	}
	return ""
}

// Same signature, same body: the second label merges into the first.
func same(raw interface{}) string {
	switch raw.(type) {
	case Left:
		return "either"
	case Right:
		return "either"
	}
	return ""
}

// Two DIFFERENT delegates sharing a first type argument, same body: never merged.
func distinct(raw interface{}) string {
	switch raw.(type) {
	case func(int) int:
		return "fn"
	case func(int) string:
		return "fn"
	}
	return ""
}

func main() {
	fmt.Println(bound(nil), unbound(nil), differ(nil), same(nil), distinct(nil))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "tscf", "main.cs"))

	// LIVE labels are matched with their indentation: the merge marker comment spells the same
	// `case <type>:` text, so an unanchored match reads a merged label as present.
	for _, want := range []string{
		"    case Func<nint, (any, error)> f:", // a binding (mapstructure's shape)
		"    case Func<@string, nint>:",        // no binding
		"    case Func<nint, nint>:",           // distinct delegates sharing a first type argument
		"    case Func<nint, @string>:",
		"/* case Func<bool, bool>: merged with an earlier case mapping to the same C# type (identical body) */",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("collapsed func-type case must spell its base delegate (%q): %s", want, mainCs)
		}
	}

	if strings.Contains(mainCs, "/* case Func<nint, @string>") {
		t.Errorf("two DIFFERENT delegates sharing a first type argument must never merge: %s", mainCs)
	}

	for _, name := range []string{"Hook", "Name", "Left", "Right"} {
		if strings.Contains(mainCs, "case "+name) {
			t.Errorf("a collapsed func type is never declared, so no case may name %s: %s", name, mainCs)
		}
	}

	// differ keeps BOTH labels (the loud CS8120), same merges its second: three live Func<bool, bool>
	// labels in all.
	if got := strings.Count(mainCs, "    case Func<bool, bool>:"); got != 3 {
		t.Errorf("want 3 live Func<bool, bool> labels (2 in differ, 1 in same), got %d: %s", got, mainCs)
	}
}
