// valueReceiverBoxTwin_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the value-receiver call through a pointer (convSelectorExpr, valueMethodHasBoxTwin): Go's `p.Abs()` on a
// `*Vertex`, where Abs has a value receiver, is written `p.Abs()`, and binds the `Abs(this ж<Vertex> Ꮡv)` twin
// go2cs-gen emits for every Go value method (RecvGenerator, GenTests' ValueReceiverBoxTwinTests). The controls keep
// today's `(~p).M()`: a method named like a member of ж<T> (fieldAccessCollisions.txt), a method of a package the
// converter never emits (testing's BenchmarkResult), and a method the manual registry displaces (reflect's
// Value.Len). A method VALUE is not a call and keeps its own spelling.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func convertValueReceiverFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"reflect"
	"testing"
)

type vertex struct{ X, Y int }

func (v vertex) Abs() int { return v.X + v.Y }

func (v vertex) Scale(k int) vertex { v.X *= k; return v }

func (v vertex) Value() int { return v.X }

type pair[T any] struct{ First T }

func (p pair[T]) Head() T { return p.First }

func mk() *vertex { return &vertex{1, 2} }

func positives() {
	p := &vertex{3, 4}
	fmt.Println(p.Abs(), p.Scale(2).X, mk().Abs())
	q := &pair[string]{"go"}
	fmt.Println(q.Head())
	rv := reflect.ValueOf(1)
	prv := &rv
	fmt.Println(prv.Kind())
}

func controls() {
	p := &vertex{3, 4}
	fmt.Println(p.Value())
	res := &testing.BenchmarkResult{N: 1}
	fmt.Println(res.NsPerOp())
	rv := reflect.ValueOf("go")
	prv := &rv
	fmt.Println(prv.Len())
	f := p.Abs
	fmt.Println(f())
}

func main() {
	positives()
	controls()
}
`)

	out := convertFieldAccessModule(t, root)

	return readGenerated(t, filepath.Join(out, "main.cs"))
}

// TestValueMethodThroughAPointerReadsAsGoSpellsIt requires every value-method call through a pointer the twin serves
// to read `p.M()`, and every control to keep `(~p).M()`.
func TestValueMethodThroughAPointerReadsAsGoSpellsIt(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertValueReceiverFixture(t)
	positives := funcBody(t, cs, "void positives(")

	for _, want := range []string{
		"fmt.Println(p.Abs(), p.Scale(2).X, mk().Abs());",
		"fmt.Println(q.Head());",
		"fmt.Println(prv.Kind());",
	} {
		if !strings.Contains(positives, want) {
			t.Errorf("a value method through a pointer does not read as Go spells it (want %q):\n%s", want, positives)
		}
	}

	if strings.Contains(positives, "(~") {
		t.Errorf("a value method through a pointer still dereferences at the call:\n%s", positives)
	}

	controls := funcBody(t, cs, "void controls(")

	for _, want := range []string{
		"fmt.Println((~p).Value());",     // named like ж<T>'s Value: the box's member would bind
		"fmt.Println((~res).NsPerOp());", // testing is hand-owned: no twins
		"fmt.Println((~prv).Len());",     // reflect's Value.Len is displaced by the manual registry
	} {
		if !strings.Contains(controls, want) {
			t.Errorf("a control does not keep today's spelling (want %q):\n%s", want, controls)
		}
	}

	if strings.Contains(controls, "f = p.Abs;") || strings.Contains(controls, "f = (~p).Abs;") {
		t.Errorf("a method value now binds the box's method group; it must keep its own copy-at-evaluation form:\n%s", controls)
	}
}
