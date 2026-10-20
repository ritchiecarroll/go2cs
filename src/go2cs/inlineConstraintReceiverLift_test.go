// inlineConstraintReceiverLift_test.go - Gbtc
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
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// A generic type's inline constraint that names its own parameter (`type Ord[T interface{ Less(T) bool }]`)
// lifts to `Ord_T<T>`, and each METHOD reaches the constraint as its receiver's own copy: a different
// object, possibly in another file, under a receiver that may rename the parameter. Such a lift captures a
// parameter, so it is never registered by signature (its `T` means something else in every declaration),
// and the method's clause fell through to the raw Go text, `where T : interface{Less(T) bool}<T>`, a C#
// syntax cascade. It is now found by its DECLARATION (constraintLiftKey).
//
// Arms: a method declared in the same file BEFORE the type, a method in another file whose receiver renames
// T to U, and a pointer-receiver method; each clause names the lift over its own parameter. Control: the
// type's own clause, which resolved before too.

func TestInlineConstraintReachesEveryReceiverCopy(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/ordlift\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "a_methods.go"), `package main

func (o Ord[U]) Below(t U) bool { return o.v.Less(t) }

func (o *Ord[T]) Set(t T) { o.v = t }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type age int

func (a age) Less(b age) bool { return a < b }

func (o Ord[T]) Above(t T) bool { return t.Less(o.v) }

type Ord[T interface{ Less(T) bool }] struct{ v T }

func main() {}
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

	dir := filepath.Join(options.go2csPath, "src", "example.com", "ordlift")
	mainCS := readGenerated(t, filepath.Join(dir, "main.cs"))
	methodsCS := readGenerated(t, filepath.Join(dir, "a_methods.cs"))

	clause := func(cs, function, param string) string {
		match := regexp.MustCompile(`(?s)` + function + `<` + param + `>\([^)]*\)\s*where ` + param + ` : ([^\r\n{]*)`).FindStringSubmatch(cs)

		if match == nil {
			return "<no clause>"
		}

		return strings.TrimSpace(match[1])
	}

	for _, arm := range []struct {
		name, cs, function, param, want string
	}{
		{"same file, before the type", mainCS, "Above", "T", "Ord_T<T>"},
		{"other file, renamed receiver", methodsCS, "Below", "U", "Ord_T<U>"},
		{"other file, pointer receiver", methodsCS, "Set", "T", "Ord_T<T>"},
	} {
		if got := clause(arm.cs, arm.function, arm.param); got != arm.want {
			t.Errorf("%s: %s's clause is `%s`, want `%s`", arm.name, arm.function, got, arm.want)
		}
	}

	if !regexp.MustCompile(`partial struct Ord<T>\s*where T : Ord_T<T>`).MatchString(mainCS) {
		t.Errorf("control: the type's own clause does not name Ord_T<T>")
	}

	for name, cs := range map[string]string{"main.cs": mainCS, "a_methods.cs": methodsCS} {
		if strings.Contains(cs, "interface{") {
			t.Errorf("%s carries raw Go interface text", name)
		}
	}

	if t.Failed() {
		t.Logf("main.cs:\n%s\na_methods.cs:\n%s", mainCS, methodsCS)
	}
}
