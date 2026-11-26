// methodlessFuncTypeLocal_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a `:=` local (and a `_ =` discard) initialized from a method group whose signature matches
// a METHODLESS named func type of the package. Go types the local with the UNNAMED signature, so it
// keeps the structural delegate (`var expR = expandRef;`), never the named one: naming it gave the
// local the wrong identity, and before every defined func type had its own delegate it named a type
// that did not exist (CS0246 at Go 1.26's internal/runtime/gc/scan tests, `expR :=
// scan.ExpandReference` beside `type expandFunc func(...)`). Passing the local to a parameter of the
// named type converts it as Go does (NilSafeDelegateConversion).
//
// Control: a SELF-REFERENTIAL func type (`type stateFn func(*lexer) stateFn`) does not collapse and
// keeps its named delegate, which the state-machine reassignment `state = state(l)` needs.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestMethodlessFuncTypeNameNeverTypesALocal(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/inlinefunctype\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "strings"

// Methodless and naming no func type: its own delegate; a local from a matching method group stays unnamed.
type expandFunc func(n int, out *int)

type upperFunc func(string) string

// Self-referential: keeps its named delegate.
type stateFn func(*lexer) stateFn

type lexer struct{ n int }

func expandRef(n int, out *int) { *out = n * 2 }

func lexText(l *lexer) stateFn {
	l.n++
	if l.n > 2 {
		return nil
	}
	return lexText
}

func useExpand(f expandFunc) int {
	var x int
	f(3, &x)
	return x
}

func main() {
	expR := expandRef
	var got int
	expR(2, &got)

	up := strings.ToUpper
	_ = expandRef

	state := lexText
	l := &lexer{}
	for state != nil {
		state = state(l)
	}

	println(got, useExpand(expR), up("a"), l.n)
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

	output := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "inlinefunctype", "main.cs"))

	// Each methodless func type is declared as its own delegate.
	for _, decl := range []string{"internal delegate void expandFunc(", "internal delegate @string upperFunc("} {
		if !strings.Contains(output, decl) {
			t.Errorf("expected the delegate declaration %q:\n%s", decl, output)
		}
	}

	// The method-group locals keep the unnamed type Go gives them.
	for _, local := range []string{"var expR = expandRef;", "var up = strings.ToUpper;"} {
		if !strings.Contains(output, local) {
			t.Errorf("expected the method-group local typed structurally (%q):\n%s", local, output)
		}
	}

	// The unnamed local reaches the named parameter through the nil-safe conversion.
	if want := "useExpand(builtin.NilSafeDelegateConversion<expandFunc, Action<nint, ж<nint>>>(expR))"; !strings.Contains(output, want) {
		t.Errorf("expected the unnamed local converted to the named parameter (%q):\n%s", want, output)
	}

	// The self-referential control keeps its named delegate for the method-group local.
	if !strings.Contains(output, "stateFn state = ") {
		t.Errorf("expected the state-machine local declared with its named delegate (`stateFn state = …`):\n%s", output)
	}
}
