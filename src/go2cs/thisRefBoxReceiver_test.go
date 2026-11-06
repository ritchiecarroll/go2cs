// thisRefBoxReceiver_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the pointer method that calls a VALUE method on its own receiver (bodyCallsValueMethodOnReceiver): it is
// emitted over the box, `M(this ж<T> Ꮡt)`, and the call reads `Ꮡt.Show(…)`, which binds go2cs-gen's twin. The twin
// copies *t after the call's arguments run, as Go does, and fails on a nil t through ж<T>'s own check. With a
// `this ref T` receiver the ref was copied while the arguments were still being built, and the optimizing JIT dropped
// that copy, with the nil fault, for a method that never reads its receiver. The controls keep `this ref T`: a
// pointer method that touches only fields, and one that calls only pointer methods on its receiver.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func convertThisRefFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type T struct{ n int }

func (T) Hello() string { return "hello" }

func (t T) Show(s string) string { return fmt.Sprint(s, t.n) }

func (t *T) set(v int) string { t.n = v; return "set" }

func (t *T) Via() string { return "via " + t.Hello() }

func (t *T) ViaWrite() string { return t.Show(t.set(10)) }

func (t *T) Bump() { t.n++ }

func (t *T) Twice() { t.Bump(); t.Bump() }

func main() {
	q := &T{}
	q.Twice()
	fmt.Println(q.Via(), q.ViaWrite())
}
`)

	out := convertFieldAccessModule(t, root)

	return readGenerated(t, filepath.Join(out, "main.cs"))
}

// TestPointerMethodCallingAValueMethodOnItsReceiverIsEmittedOverTheBox requires each positive method to take the box
// as its receiver and to call the value method through it, and each control to keep its `this ref T` receiver.
func TestPointerMethodCallingAValueMethodOnItsReceiverIsEmittedOverTheBox(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertThisRefFixture(t)

	for _, row := range []struct{ signature, want string }{
		// The receiver is used only for the call, so no value alias is declared.
		{"Via(this ж<T> Ꮡt)", `return "via "u8 + Ꮡt.Hello();`},
		// The argument's pointer method needs the value alias; the call still goes through the box.
		{"ViaWrite(this ж<T> Ꮡt)", "return Ꮡt.Show(t.set(10));"},
		// Controls: no value method is called on the receiver.
		{"Bump(this ref T t)", "t.n++;"},
		{"Twice(this ref T t)", "t.Bump();"},
	} {
		if body := funcBody(t, cs, row.signature); !strings.Contains(body, row.want) {
			t.Errorf("%s does not read %q:\n%s", row.signature, row.want, body)
		}
	}
}
