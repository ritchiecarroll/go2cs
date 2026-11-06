// deferReceiverCopy_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the receiver of a deferred or spawned value-method call through a pointer (visitDeferStmt, visitGoStmt,
// convSelectorExpr's deferGoCallSelector): Go copies *p at the defer or go statement, so the receiver the lowering
// snapshots there is the copy, `(~p)` for a pointer local and the alias for a parameter. The value-method twin
// (`p.M()` on the box) copies *p when the call runs, so a write through p after the statement leaked in, and a nil p
// went past the statement where Go panics at it. A plain call through a pointer local keeps the twin.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func convertDeferReceiverFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type path struct{ name string }

func (p path) Done() { fmt.Println("done", p.name) }

func (p path) Pop(k int) { fmt.Println("pop", k, p.name) }

func deferLocal() {
	p := &path{"pointee"}
	defer p.Done()
	*p = path{"replaced"}
}

func deferLocalArg(k int) {
	p := &path{"pointee"}
	defer p.Pop(k)
	*p = path{"replaced"}
}

func goLocal(k int) {
	p := &path{"pointee"}
	go p.Pop(k)
	*p = path{"replaced"}
}

func deferParam(p *path) {
	defer p.Done()
	*p = path{"replaced"}
}

func plainLocal() {
	p := &path{"pointee"}
	p.Done()
}

func main() {
	deferLocal()
	deferLocalArg(1)
	goLocal(2)
	deferParam(&path{"pointee"})
	plainLocal()
}
`)

	out := convertFieldAccessModule(t, root)

	return readGenerated(t, filepath.Join(out, "main.cs"))
}

// TestDeferredValueMethodThroughAPointerCopiesAtTheStatement requires a deferred or spawned value-method call through
// a pointer to snapshot the copied value, and a plain call to keep the twin.
func TestDeferredValueMethodThroughAPointerCopiesAtTheStatement(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertDeferReceiverFixture(t)

	for _, row := range []struct{ fn, want string }{
		{"deferLocal(", "ᴛ0 => ᴛ0.Done(), (~p"},
		{"deferLocalArg(", "(ᴛ0, ᴛ1) => ᴛ0.Pop(ᴛ1), (~p"},
		{"goLocal(", "(ᴛ0, ᴛ1) => ᴛ0.Pop(ᴛ1), (~p"},
		{"deferParam(", "ᴛ0 => ᴛ0.Done(), Ꮡp.Value"},
		{"plainLocal(", "p.Done();"}, // the twin: Go copies *p at the call, after its (absent) arguments
	} {
		if body := funcBody(t, cs, row.fn); !strings.Contains(body, row.want) {
			t.Errorf("%s does not read %q:\n%s", row.fn, row.want, body)
		}
	}

	if body := funcBody(t, cs, "plainLocal("); strings.Contains(body, "(~p") {
		t.Errorf("a plain call through a pointer local lost the twin:\n%s", body)
	}
}
