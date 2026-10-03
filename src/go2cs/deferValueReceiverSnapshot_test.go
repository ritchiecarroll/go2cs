// deferValueReceiverSnapshot_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a deferred VALUE-receiver method call (go-cmp's `defer s.curPtrs.Pop(px, py)`, COORD 2026-10-03): the
// method is a C# extension over a value type, from which no delegate can be created (CS1113), so golib's defer
// cannot take its method group. The receiver becomes the thunk's first eager argument instead, so it is copied
// at the defer statement as Go copies it. The controls hold a result-returning nullary callee and a variadic
// callee in the lambda form they already took.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestADeferredValueReceiverMethodSnapshotsItsReceiver(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/dvr\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type path struct{ name string }

func (p path) Pop(k int)            { println("pop", p.name, k) }
func (p path) Done()                { println("done", p.name) }
func (p path) Pair(a int, b string) { println("pair", p.name, a, b) }
func (p path) Close() error         { return nil }
func (p path) Log(args ...any)      { println("log", p.name, len(args)) }

type state struct{ cur path }

func (s *state) run(k int) {
	defer s.cur.Pop(k)
	defer s.cur.Done()
	defer s.cur.Pair(k, "b")
	defer s.cur.Close()
	defer s.cur.Log(k)
	s.cur = path{"replaced"}
}

func viaPointer(p *path) {
	defer p.Done()
	*p = path{"replaced"}
}

func main() {
	s := &state{cur: path{"orig"}}
	s.run(1)
	viaPointer(&path{"orig"})
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "dvr", "main.cs"))

	// The receiver is the thunk's first eager argument, so it is copied when the defer statement runs.
	for _, want := range []string{
		`defer((ᴛ0, ᴛ1) => ᴛ0.Pop(ᴛ1), Ꮡs.Value.cur, k, ref ᒐ);`,
		`defer(ᴛ0 => ᴛ0.Done(), Ꮡs.Value.cur, ref ᒐ);`,
		`defer((ᴛ0, ᴛ1, ᴛ2) => ᴛ0.Pair(ᴛ1, ᴛ2), Ꮡs.Value.cur, k, (@string)"b", ref ᒐ);`,
		`defer(ᴛ0 => ᴛ0.Done(), Ꮡp.Value, ref ᒐ);`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q: a value-receiver method has no delegate (CS1113) and Go copies its receiver at the defer:\n%s", want, mainCs)
		}
	}

	// CONTROLS: a result-returning nullary callee and a variadic callee keep the lambda form they already took.
	for _, want := range []string{
		`defer(() => Ꮡs.Value.cur.Close(), ref ᒐ);`,
		`=> Ꮡs.Value.cur.Log(`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control: want %q unchanged:\n%s", want, mainCs)
		}
	}
}
