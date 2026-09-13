// untypedInterfaceFuncLit_test.go - Gbtc
// Copyright (c) 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the func-literal result type across an EMPTY-interface slot reached through a KEYED
// composite literal — a map[K]any value, an any struct field, a sparse-[N]any element.
//
// Such a slot has no delegate target, so C# derives the delegate type from the literal's body.
// That is exact for a literal with a reachable return, and LOSSY for one whose body never
// completes normally: with no return statement to derive from C# infers Action, and the Go
// result type is gone. The reflection bridge then truthfully reports NumOut()==0 — not a bridge
// defect but a missing datum in the emission — and text/template's own goodFunc rejects
// FuncMap{"die": func() bool { panic("die") }} as "0 return values", panicking at registration
// and taking 16 of that package's 52 verdicts with it.
//
// The MULTI-result arm has the same owner from the opposite end: every return renders as a C#
// tuple carrying a typeless element, so no arm contributes a natural type and inference fails
// outright (html/template escape_test's pred, CS8917 + CS1662/CS8716 per return).
//
// The ARGUMENT position already stated the declared result type explicitly
// (CallExprContext.emptyInterfaceArgs -> LambdaContext.untypedInterfaceTarget); this is the same
// rule reached through the keyed composite forms, which never route through convExprList's
// argument plumbing.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestUntypedInterfaceFuncLitResultType pins the explicit return type on a func literal in an
// any slot of a keyed composite — single-result and multi-result alike — and pins the CONTROL
// that a concrete func-typed slot, which does have a delegate target, is left exactly as it was.
func TestUntypedInterfaceFuncLitResultType(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), `module example.com/uifl

go 1.23
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"reflect"
)

// text/template's FuncMap, verbatim in shape.
type FuncMap map[string]any

type holder struct {
	fn any
}

func main() {
	m := FuncMap{
		// The measured shape: a body that never completes normally, so there is no return
		// statement for C# to derive a delegate type from.
		"die":  func() bool { panic("die") },
		"live": func() string { return "ok" },
		// html/template escape_test's shape: MULTI-result, every arm a tuple with a
		// typeless element, so no arm can fix the delegate type either.
		"pred": func(a ...any) (any, error) {
			if len(a) == 1 {
				if i, ok := a[0].(int); ok && i > 0 {
					return i - 1, nil
				}
			}
			return nil, fmt.Errorf("undefined pred(%v)", a)
		},
	}

	// An interface{} STRUCT FIELD is the same slot class, reached through the same keyed path.
	h := holder{fn: func() int { panic("boom") }}

	// CONTROL: a concrete func-typed slot HAS a delegate target, so nothing is owed and
	// nothing may change.
	typed := map[string]func() bool{"t": func() bool { panic("t") }}

	fmt.Println(reflect.TypeOf(m["die"]).NumOut(), reflect.TypeOf(m["live"]).NumOut())
	fmt.Println(reflect.TypeOf(m["pred"]).NumOut(), reflect.TypeOf(h.fn).NumOut(), reflect.TypeOf(typed["t"]).NumOut())
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "uifl", "main.cs"))

	// The defect's own signature: the panic-only literal rendered with no return type at all,
	// which is exactly what C# reads as Action.
	if strings.Contains(mainCs, "[\"die\"u8] = () =>") {
		t.Errorf("panic-only func literal in an any map slot lost its result type (renders as Action): %s", mainCs)
	}

	// Each any slot states the DECLARED Go result type, whatever the body's shape and arity.
	//
	// `pred` is the VARIADIC one, and it carries a second thing its nullary siblings do not: the
	// empty-interface boxing CAST to its Go func type (typedNilInterfaceBoxing.go). Without it the
	// box's dynamic type is C#'s synthesized `<>f__AnonymousDelegate0` — a `params` signature has
	// no BCL delegate for C#'s natural function type to be — and no Go-spelled assertion or
	// `reflect.TypeOf` can ever match it. The two properties are pinned in ONE string here because
	// they are emitted at the same site and a change to either would otherwise be invisible: the
	// result type must still be stated INSIDE the cast, not replaced by it.
	for _, want := range []string{
		"[\"die\"u8] = bool () =>",
		"[\"live\"u8] = @string () =>",
		"[\"pred\"u8] = ((Funcꓸꓸꓸ<any, (any, error)>)((any, error) (params ",
		"fn: nint () =>",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("func literal in an any slot must state its Go result type (%q): %s", want, mainCs)
		}
	}

	// CONTROL: the concrete map[string]func() bool slot keeps the bare, target-typed lambda.
	// A return-type prefix here would be pure churn — the delegate type is already stated by
	// the map's own value type.
	if !strings.Contains(mainCs, "new map<@string, Func<bool>>{[\"t\"u8] = () => {") {
		t.Errorf("a concrete func-typed slot must keep its target-typed lambda unchanged: %s", mainCs)
	}
}
