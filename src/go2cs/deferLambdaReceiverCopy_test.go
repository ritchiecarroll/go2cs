// deferLambdaReceiverCopy_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the receiver COPY of a deferred value-receiver call that takes the lambda form (COORD 2026-10-03, routed
// from the go-cmp reading): a nullary callee returning a result (`defer s.cur.Close()`) or a variadic callee
// (`defer s.cur.Log(k)`) read its receiver when the thunk ran, so `s.cur = …` after the defer leaked in — silently
// wrong (`close replaced` where Go prints `close orig`). A field or dereferenced receiver is now the thunk's first
// eager argument, as the method-group shapes' already is. An IDENTIFIER receiver keeps the capture hoist that
// already copies it (`var kʗ1 = k;`), the control.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestALambdaFormDeferredValueReceiverCopiesItsReceiver(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/dlr\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type path struct{ name string }

func (p path) Close() error    { println("close", p.name); return nil }
func (p path) Log(args ...any) { println("log", p.name, len(args)) }

type state struct{ cur path }

func (s *state) run(k int) {
	defer s.cur.Close()
	defer s.cur.Log(k)
	s.cur = path{"replaced"}
}

func viaPointer(p *path) {
	defer p.Close()
	*p = path{"replaced"}
}

func local() {
	k := path{"local"}
	defer k.Close()
	k = path{"replaced"}
}

func main() {
	s := &state{cur: path{"orig"}}
	s.run(1)
	viaPointer(&path{"orig"})
	local()
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "dlr", "main.cs"))

	// A field or dereferenced receiver is the thunk's first eager argument, copied at the defer statement.
	for _, want := range []string{
		`defer(ᴛ0 => ᴛ0.Close(), Ꮡs.Value.cur, ref ᒐ);`,
		`defer((ᴛ0, ᴛ1) => ᴛ0.Log(ᴛ1), Ꮡs.Value.cur, k, ref ᒐ);`,
		`defer(ᴛ0 => ᴛ0.Close(), Ꮡp.Value, ref ᒐ);`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q: Go copies a method value's receiver at the defer statement:\n%s", want, mainCs)
		}
	}

	// CONTROL: an identifier receiver keeps the capture hoist that already copies it.
	for _, want := range []string{"var kʗ1 = k;", "defer(() => kʗ1.Close(), ref ᒐ);"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control: want %q unchanged:\n%s", want, mainCs)
		}
	}
}
