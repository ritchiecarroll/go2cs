// partialInstantiationAnonymousTypeArg_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards completedInstantiationTypeArgs' lift of a written ANONYMOUS type argument (constraintOperations.go).
// Go 1.26's reflect/all_test.go writes `testTypeAssert[interface{ A() }](t, vv, nil, false)` against
// `testTypeAssert[T comparable, V any]`: a PARTIAL instantiation, whose missing V go/types infers. C# has no partial
// instantiation, so the converter completes the list from the resolved instance and renders each position from its
// go/types TYPE. The written anonymous interface was never visited as an expression, so no lift declared it. It
// rendered as raw Go text, `interface{A()}`, an unresolved dynamic type that failed reflect's whole -tests
// conversion. The fixture is that shape for an interface and for a struct, plus the complete-list control, whose
// written argument is visited directly and must keep its rendering.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertPartialAnonymousTypeArgFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/partialanon\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type withM struct{ s string }

func (w withM) M() {}

func assertAs[T any, V any](v V) (T, bool) {
	t, ok := any(v).(T)
	return t, ok
}

func only[T any](v any) (T, bool) {
	t, ok := v.(T)
	return t, ok
}

// POSITIVE -- reflect's shape: a partial instantiation whose written argument is an anonymous interface.
func partialInterface() bool {
	_, ok := assertAs[interface{ A() }](withM{"x"})
	return ok
}

// POSITIVE -- the same path for an anonymous struct.
func partialStruct() bool {
	_, ok := assertAs[struct{ X int }](struct{ X int }{1})
	return ok
}

// CONTROL -- a complete list visits its written argument directly.
func complete() bool {
	_, ok := only[interface{ M() }](withM{"x"})
	return ok
}

func main() {
	fmt.Println(partialInterface(), partialStruct(), complete())
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "partialanon", "main.cs"))
}

// TestPartialInstantiationLiftsAnonymousTypeArg requires each partial instantiation to name a LIFTED type for its
// written anonymous argument, the way the complete list does, and never the raw Go text.
func TestPartialInstantiationLiftsAnonymousTypeArg(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertPartialAnonymousTypeArgFixture(t)

	for name, want := range map[string]string{"partialInterface": "assertAs<partialInterface_type, withM>", "partialStruct": "assertAs<partialStruct_type, partialStruct_type>"} {
		body := funcBody(t, cs, name+"(")

		if strings.Contains(body, "interface{") || strings.Contains(body, "struct{") {
			t.Errorf("%s: the written anonymous type argument renders as raw Go text:\n%s", name, body)
		}

		if !strings.Contains(body, want) {
			t.Errorf("%s: the partial instantiation does not name the lifted type for its written argument (want %q):\n%s", name, want, body)
		}

		if !strings.Contains(cs, "interface "+name+"_type") && !strings.Contains(cs, "struct "+name+"_type") {
			t.Errorf("%s: no lifted declaration %s_type is emitted", name, name)
		}
	}

	// CONTROL: the complete list's written argument is lifted on the visit it always had.
	if body := funcBody(t, cs, "complete("); !strings.Contains(body, "only<complete_type>") {
		t.Errorf("the complete instantiation lost its lifted type argument:\n%s", body)
	}
}
