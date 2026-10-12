// deferGoSpreadArgument_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a defer or go statement whose call SPREADS a slice into a variadic parameter (`defer f(xs...)`,
// `go f(xs...)`; C2's SUGGEST of 2026-09-24, COORD ruling 2026-10-12). The eager argument was the spread view
// itself (`defer((ᴛ1, ᴛ2) => show(ᴛ1, ᴛ2), "defer", xs.ꓸꓸꓸ, ref ᒐ)`), a Span that no generic thunk parameter can
// hold: CS9244 at every arity, CS1503 for the one-argument goǃ. Go evaluates the arguments at the statement, and
// a spread argument is the slice itself, so the eager argument is now the SLICE and the thunk spreads it. The
// callee's pack then shares the caller's backing array, as in Go. The behavioral guard is DeferGoSpreadArg.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestADeferredOrSpawnedSpreadCallPassesTheSliceAndSpreadsItInTheThunk(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/spr\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

func show(tag string, xs ...int) { fmt.Println(tag, xs) }

func only(xs ...int) { fmt.Println(xs) }

type c struct{ n int }

func (p *c) note(tag string, xs ...int) { p.n += len(xs) }

func deferred(p *c) {
	xs := []int{1, 2}
	defer show("defer", xs...)
	defer p.note("method", xs...)
	xs[0] = 9
}

func spawned() {
	xs := []int{3, 4}
	go show("go", xs...)
	go only(xs...)
}

func main() {
	deferred(&c{})
	spawned()
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

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "spr", "main.cs")), "\r\n", "\n")

	// The eager argument is the slice, evaluated at the statement; the thunk spreads its own parameter.
	for _, want := range []string{
		`defer((ᴛ1, ᴛ2) => show(ᴛ1, ᴛ2.ꓸꓸꓸ), (@string)"defer", xs, ref ᒐ);`,
		`defer((ᴛ1, ᴛ2) => Ꮡp.Value.note(ᴛ1, ᴛ2.ꓸꓸꓸ), (@string)"method", xs, ref ᒐ);`,
		`goǃ((ᴛ1, ᴛ2) => show(ᴛ1, ᴛ2.ꓸꓸꓸ), (@string)"go", xs);`,
		`goǃ(ᴛ1 => only(ᴛ1.ꓸꓸꓸ), xs);`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q: a spread argument of a deferred or spawned call is passed as the slice and spread by the thunk:\n%s", want, mainCs)
		}
	}

	// No spread view is ever an eager argument: a Span cannot be a generic thunk parameter (CS9244, CS1503).
	for _, unwanted := range []string{"xs.ꓸꓸꓸ, ref ᒐ);", "xs.ꓸꓸꓸ);"} {
		if strings.Contains(mainCs, unwanted) {
			t.Errorf("unwanted %q: the spread view was passed as the eager argument:\n%s", unwanted, mainCs)
		}
	}
}
