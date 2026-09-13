// returnOperandOrder_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the two properties of a MULTI-VALUE `return` that a C# tuple literal does not give for
// free, and — as importantly — the SCOPE of the first: a rule that spilled every call-bearing
// operand would be correct and would also rewrite most of the corpus, so the controls below carry
// as much weight as the positives.
//
//  1. ORDER (returnOperandOrder.go). gc spills a return's calls to temporaries and reads the plain
//     operands afterwards; a C# tuple literal reads left to right. Where a later call can WRITE
//     what an earlier operand reads, the emission spills to match gc.
//
//  2. IDENTITY (typedNilInterfaceBoxing.go, applied in visitReturnStmt's forwarded arm). A
//     multi-value POINTER-returning call forwarded whole into an EMPTY-interface result must put
//     the pointer BOX in the interface, not `~`-unwrap it into a copy of the pointee.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertReturnOrderFixture converts the one fixture both tests read and returns its emitted C#.
func convertReturnOrderFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/returnorder\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type oid struct{ der []byte }

// A POINTER receiver: ` + "`o.fill()`" + ` takes o's address, so the plain ` + "`o`" + ` beside it can observe the write.
func (o *oid) fill(text string) error { o.der = []byte(text); return nil }

type counter struct{ n int }

func (c *counter) bump() int  { c.n++; return c.n }
func (c counter) peek() int   { c.n = 99; return c.n }

func raise(n *int) int { *n += 10; return *n }

// HAZARD 1 - crypto/x509 ParseOID's exact shape.
func parseOID(text string) (oid, error) {
	var o oid
	return o, o.fill(text)
}

// HAZARD 2 - the read is a FIELD of the value the call mutates.
func readThenBump() (int, int) {
	var c counter
	return c.n, c.bump()
}

// HAZARD 3 - the address is handed over EXPLICITLY rather than through a receiver.
func addressArgument() (int, int) {
	n := 0
	return n, raise(&n)
}

// HAZARD 4 - the read reaches THROUGH a pointer the call writes through.
func throughPointer() (int, int) {
	c := &counter{}
	return c.n, c.bump()
}

// CONTROL 1 - the operand IS the pointer, so both orders yield the same pointer value.
func pointerIdentity() (*counter, int) {
	c := &counter{}
	return c, c.bump()
}

// CONTROL 2 - the call writes a different variable entirely.
func unrelatedOperand() (int, int) {
	var a counter
	var b counter
	return a.n, b.bump()
}

// CONTROL 3 - a VALUE receiver is a copy; the caller can observe nothing.
func valueReceiverCall() (int, int) {
	var c counter
	return c.n, c.peek()
}

type node struct {
	handler int
	pat     *counter
}

// CONTROL 4 - net/http's shape. The read is inside *nd; the call writes *(nd.pat), which is storage
// of its own. A root-plus-indirect model reads both as "behind nd" and spills; the access path
// diverges at .handler vs .pat and does not.
func pointerFieldUnrelated(nd *node) (int, int) {
	return nd.handler, nd.pat.bump()
}

// HAZARD 5 - the same two hops, now READING through the very pointer field the call writes through.
// The write path is a prefix of the read path with no deref between them, so it conflicts.
func pointerFieldSame(nd *node) (int, int) {
	return nd.pat.n, nd.pat.bump()
}

type thing struct{ name string }

func newThing(name string) (*thing, error) { return &thing{name: name}, nil }

// IDENTITY - a multi-value POINTER-returning call forwarded whole into (any, error).
func forwardPointer(name string) (any, error) { return newThing(name) }

func main() {
	o, _ := parseOID("abc")
	a, b := readThenBump()
	p, q := addressArgument()
	r, s := throughPointer()
	pc, pb := pointerIdentity()
	ua, ub := unrelatedOperand()
	va, vb := valueReceiverCall()
	nd := &node{pat: &counter{}}
	fa, fb := pointerFieldUnrelated(nd)
	ga, gb := pointerFieldSame(nd)
	v, _ := forwardPointer("one")
	fmt.Println(string(o.der), a, b, p, q, r, s, pc.n, pb, ua, ub, va, vb, fa, fb, ga, gb, v)
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "returnorder", "main.cs"))
}

// TestMultiValueReturnSpillsCallsThatCanWriteAnEarlierOperand pins the ORDER property and its scope.
// Proven against three neuters, each isolating one thing the rule has to get right:
//
//   - returnMultiValueHoistThrough forced to -1 - all five hazards report the unspilled
//     `return (x, call);` form, so the positives are not passing by accident;
//   - forced to len(results)-1 - all four controls report a spill they must not have, so the SCOPE
//     is measured rather than merely asserted; and
//   - pathsConflict forced to same-root, which is the root-plus-indirect model the access path
//     replaced - exactly `pointerIdentity` and `pointerFieldUnrelated` report, the very pair the
//     path model exists to separate, and net/http's `return n.handler, n.pattern.String(), ...` is
//     the corpus instance of the second.
func TestMultiValueReturnSpillsCallsThatCanWriteAnEarlierOperand(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertReturnOrderFixture(t)

	// Each hazard spills its call to a temporary AHEAD of the return, so the plain operand — which
	// stays in the tuple — is read after it, exactly as gc reads it.
	hazards := []struct {
		name    string
		spill   string
		spilled string
	}{
		{"parseOID", "= o.fill(text);", "return (o, "},
		{"readThenBump", "= c.bump();", "return (c.n, "},
		{"addressArgument", "= raise(ref n);", "return (n, "},
		{"throughPointer", "= c.bump();", "return ((~c).n, "},
		{"pointerFieldSame", "= nd.pat.bump();", "return ((~nd.pat).n, "},
	}

	for _, hazard := range hazards {
		if !strings.Contains(mainCs, "var "+TempVarMarker) || !strings.Contains(mainCs, hazard.spill) {
			t.Errorf("%s: expected its call spilled to a temporary (`var %s… %s`); emission:\n%s",
				hazard.name, TempVarMarker, hazard.spill, mainCs)
		}

		if !strings.Contains(mainCs, hazard.spilled+TempVarMarker) {
			t.Errorf("%s: expected the tuple to name the temporary (`%s%s…`); emission:\n%s",
				hazard.name, hazard.spilled, TempVarMarker, mainCs)
		}
	}

	// The controls keep the call IN the tuple: nothing they return can observe it, and a rule that
	// spilled them anyway would rewrite emissions across the corpus for no correctness gain.
	controls := []struct {
		name    string
		emitted string
	}{
		{"pointerIdentity", "return (c, c.bump());"},
		{"unrelatedOperand", "return (a.n, b.bump());"},
		{"valueReceiverCall", "return (c.n, c.peek());"},
		{"pointerFieldUnrelated", "return (nd.handler, nd.pat.bump());"},
	}

	for _, control := range controls {
		if !strings.Contains(mainCs, control.emitted) {
			t.Errorf("%s: expected the call to stay in the tuple (`%s`); emission:\n%s",
				control.name, control.emitted, mainCs)
		}
	}
}

// TestForwardedMultiValueReturnBoxesPointerIntoEmptyInterface pins the IDENTITY property: the
// forwarded arm must hand the pointer BOX to the `any` result, carrying its typed-nil form, and must
// never `~`-unwrap it (which would box a copy of the pointee — dynamic type T where Go says *T).
// Proven failing-first by routing the empty-interface element back through convertToInterfaceType:
// the emission returns to `return (~ᴛ1, ᴛ2);`.
func TestForwardedMultiValueReturnBoxesPointerIntoEmptyInterface(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertReturnOrderFixture(t)

	boxed := "return (" + TempVarMarker + "1." + TypedNilBoxAccessor + ", " + TempVarMarker + "2);"

	if !strings.Contains(mainCs, boxed) {
		t.Errorf("forwardPointer: expected the pointer box forwarded into `any` (`%s`); emission:\n%s", boxed, mainCs)
	}

	if strings.Contains(mainCs, "("+PointerDerefOp+TempVarMarker) {
		t.Errorf("forwardPointer: the forwarded tuple element is DEREFERENCED (`%s%s…`), so the `any` holds a copy of the pointee; emission:\n%s",
			PointerDerefOp, TempVarMarker, mainCs)
	}
}
