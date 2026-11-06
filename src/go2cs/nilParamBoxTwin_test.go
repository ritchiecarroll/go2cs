// nilParamBoxTwin_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the value-receiver call on a deref-aliased pointer PARAMETER or direct-ж RECEIVER (convSelectorExpr): Go's
// `p.Abs()` there is written through the box, `Ꮡp.Abs()`, so it binds the `Abs(this ж<T> Ꮡv)` twin go2cs-gen emits,
// which copies *p after the arguments run and fails on a nil p through ж<T>'s own check. On the alias, `p.Abs()`
// copied *p before the arguments, and a nil p's fault was lost when the JIT dropped a copy the method never reads
// (DOTNET_TieredCompilation=0). Inside a closure the box is captured as it is. The controls keep the alias: a method
// named like a member of ж<T> (fieldAccessCollisions.txt), and a `[GoRecv] this ref T` receiver, which has no box.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func convertNilParamFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type vertex struct{ X, Y int }

func (v vertex) Abs() int { return v.X + v.Y }

func (v vertex) Value() int { return v.X }

func (vertex) Hello() string { return "hello" }

type holder struct{ n int }

func (h holder) Size() int { return h.n }

var kept *holder

func viaParam(p *vertex) int { return p.Abs() + len(p.Hello()) }

func viaParamMixed(p *vertex) int { return p.X + p.Abs() }

func viaParamClosure(p *vertex) func() int { return func() int { return p.Abs() } }

func viaParamControl(p *vertex) int { return p.Value() }

func (h *holder) refRecv() int { return h.Size() }

func (h *holder) boxRecv() int {
	kept = h
	return h.Size()
}

func main() {
	v := &vertex{1, 2}
	h := &holder{3}
	fmt.Println(viaParam(v), viaParamClosure(v)(), viaParamControl(v), h.refRecv(), h.boxRecv())
}
`)

	out := convertFieldAccessModule(t, root)

	return readGenerated(t, filepath.Join(out, "main.cs"))
}

// TestValueMethodOnADerefAliasedPointerCallsTheBoxTwin requires a value-method call on a pointer parameter, and on a
// direct-ж receiver, to go through the box, and each control to keep the alias. A parameter whose every value use is
// such a call loses its now-dead alias (visitFuncDecl) and is the box under its Go name.
func TestValueMethodOnADerefAliasedPointerCallsTheBoxTwin(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertNilParamFixture(t)

	for _, row := range []struct{ fn, want, refuse string }{
		// Every value use is a twin call, so the alias is dead and the parameter keeps its Go name for the box.
		{"viaParam(", "return p.Abs() + len(p.Hello());", "DerefOrNull"},
		{"viaParamClosure(", "p.Abs()", "DerefOrNull"},
		// A field read keeps the alias live; the call still goes through the box.
		{"viaParamMixed(", "return p.X + Ꮡp.Abs();", ""},
		{"viaParamControl(", "return p.Value();", ""}, // named like ж<T>'s Value: the box's member would bind
		{"refRecv(", "return h.Size();", ""},          // a [GoRecv] ref receiver has no box in scope
		{"boxRecv(", "return Ꮡh.Size();", ""},
	} {
		body := funcBody(t, cs, row.fn)

		if !strings.Contains(body, row.want) {
			t.Errorf("%s does not read %q:\n%s", row.fn, row.want, body)
		}

		if row.refuse != "" && strings.Contains(body, row.refuse) {
			t.Errorf("%s still holds %q, so a call reads the alias:\n%s", row.fn, row.refuse, body)
		}
	}
}
