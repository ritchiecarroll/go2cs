// namedUnionBreadcrumb_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards namedConstraintUnionText (constraintOperations.go): a NAMED constraint whose type set is one
// union of value types (internal/trace's `type EventDetails interface { Metric | Label | … }`) emits its
// union beside its name in the `new()` breadcrumb, `where T : /* Details = A | B */ new()`. With only the
// name, the elided-constraint ratchet (repoguard) gated internal/trace/event.cs:374,394 as a lost
// constraint. The control: an INLINE union keeps its existing breadcrumb, unchanged.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertNamedUnionFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/namedunion\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type A struct{ n int }
type B struct{ s string }

// POSITIVE -- a named constraint over a union of struct types (EventDetails' shape).
type Details interface {
	A | B
}

type Config[T Details] struct{ v T }

func Make[T Details](c Config[T]) T { return c.v }

// CONTROL -- an inline union keeps its breadcrumb as it was.
func Inline[T A | B](v T) T { return v }

func main() {
	fmt.Println(Make(Config[A]{A{1}}), Inline(B{"x"}))
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "namedunion", "main.cs"))
}

// TestNamedUnionConstraintBreadcrumbCarriesItsUnion requires both the generic type and the generic func
// over the named constraint to read `/* Details = A | B */ new()`, never the bare `/* Details */ new()`,
// and the inline union to keep `/* A | B */ new()`.
func TestNamedUnionConstraintBreadcrumbCarriesItsUnion(t *testing.T) {
	cs := convertNamedUnionFixture(t)

	if strings.Contains(cs, "/* Details */ new()") {
		t.Errorf("the named union constraint's breadcrumb is the bare name (the ratchet gates it):\n%s", cs)
	}

	if got := strings.Count(cs, "where T : /* Details = A | B */ new()"); got != 2 {
		t.Errorf("want the union beside the name at both the generic type and the generic func, got %d:\n%s", got, cs)
	}

	// The inline union keeps the breadcrumb it always had: the constraint's own text (go/types spells its
	// terms with their import path), never a `Name =` prefix, since an inline constraint has no name.
	if !strings.Contains(cs, "where T : /* example.com/namedunion.A | example.com/namedunion.B */ new()") {
		t.Errorf("the inline union's breadcrumb changed; it must stay the constraint's own text:\n%s", cs)
	}
}
