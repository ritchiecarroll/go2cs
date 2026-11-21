// promotedOneHop_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

// TS PERF ITEM 1b (COORD 2026-11-21 06:58Z): a pointer-receiver method promoted through N value embeds was called through
// N views, one per level (`x.of(A.ᏑB).of(B.ᏑC).of(C.ᏑD).M()`), each minted and kept per box. Go reaches the method's
// receiver in one step, and go2cs-gen already declares the outer type's accessor for the promoted embed (`A.ᏑD`, a ref
// to `x.B.C.D`), whose Go offset golib resolves by name. So the call takes ONE view, `x.of(A.ᏑD).M()`, which is the same
// Go pointer as the chain's (managed-exact identity: one root, one Go offset). TypeScript's checkdemo minted six views
// per node this way in LeftHandSideExpressionBase.AsNode.
const promotedOneHopMain = `package main

import "fmt"

type D struct{ n int }

func (d *D) Bump() *D { d.n++; return d }

type C struct{ D }
type B struct{ C }
type A struct{ B }

func main() {
	x := &A{}
	p := x.Bump()
	q := x.Bump()
	fmt.Println(x.n, p == q, p == &x.D, p == &x.B.C.D)
}
`

// The emission: one view through the promoted embed's accessor, and no intermediate level.
func TestAPromotedCallTakesOneViewThroughTheDeepAccessor(t *testing.T) {
	out := convertInlineModule(t, "app", map[string]string{"main.go": promotedOneHopMain})
	main := readGenerated(t, filepath.Join(out, "main.cs"))

	if !strings.Contains(main, ".of(A.ᏑD).Bump()") {
		t.Errorf("the promoted call does not take one view through A.ᏑD:\n%s", main)
	}

	if strings.Contains(main, ".of(B.ᏑC)") || strings.Contains(main, ".of(C.ᏑD).Bump()") {
		t.Errorf("the promoted call still walks the intermediate levels:\n%s", main)
	}
}

// End to end: the one view is the same Go pointer as &x.D and as the spelled chain &x.B.C.D, and the method's writes
// land in x. GO2CS_INLINE_BUILD=1 runs it.
func TestAPromotedCallThroughOneViewReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := map[string]string{"main.go": promotedOneHopMain}
	want := goRunInlineModule(t, "app", files)
	out := convertInlineModule(t, "app", files)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints: %s; go2cs prints: %s", want, got)

	if got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
	}
}

// A chain that crosses a FOREIGN embed above its last owner, the shape of TypeScript's checker structs over ast nodes.
// go2cs-gen walks a metadata embed one level, but that level includes the embed's own generated promoted `ref`
// properties (lib.X carries `Z`), so Outer declares `ᏑZ` and the call takes one view through it. Measured before this
// test was kept: a seat decline written on the opposite premise (no `ᏑZ`, CS0117) was refuted by this e2e passing
// WITHOUT it, and was withdrawn.
const promotedOneHopForeignLib = `package lib

type Z struct{ N int }

func (z *Z) Bump() *Z { z.N++; return z }

type Y struct{ Z }
type X struct{ Y }
`

const promotedOneHopForeignMain = `package main

import (
	"fmt"

	"example.com/app/lib"
)

type Outer struct{ lib.X }

func main() {
	o := &Outer{}
	p := o.Bump()
	q := o.Bump()
	fmt.Println(p.N, p == q)
}
`

func TestAPromotedCallThroughAForeignEmbedTakesOneView(t *testing.T) {
	files := map[string]string{"lib/lib.go": promotedOneHopForeignLib, "main.go": promotedOneHopForeignMain}
	out := convertInlineModule(t, "app", files)
	main := readGenerated(t, filepath.Join(out, "main.cs"))

	if !strings.Contains(main, ".of(Outer.ᏑZ).Bump()") {
		t.Errorf("the promoted call through a foreign embed does not take one view through Outer.ᏑZ:\n%s", main)
	}
}

// End to end: the one view across the foreign embed builds (Outer.ᏑZ exists) and reads as Go. GO2CS_INLINE_BUILD=1 runs it.
func TestAPromotedCallThroughAForeignEmbedReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := map[string]string{"lib/lib.go": promotedOneHopForeignLib, "main.go": promotedOneHopForeignMain}
	want := goRunInlineModule(t, "app", files)
	out := convertInlineModule(t, "app", files)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints: %s; go2cs prints: %s", want, got)

	if got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
	}
}

// P3 1b review, finding 1 (a CONTROL). The one hop is taken on an all-VALUE chain only. Through a POINTER embed,
// `A.ᏑD` would be a ref computed through the CURRENT `x.B.C` on every access: re-pointing `x.B.C` after the call
// would move a view Go never moves. Go writes the OLD C's D through p.
const promotedOneHopPointerMain = `package main

import "fmt"

type D struct{ n int }

func (d *D) Bump() *D { d.n++; return d }

type C struct{ D }
type B struct{ *C }
type A struct{ B }

func main() {
	x := &A{B{&C{}}}
	p := x.Bump()
	old := x.B.C
	x.B.C = &C{}
	p.n += 10
	fmt.Println(old.n, x.n, p == &old.D, p == &x.D)
}
`

func TestAPromotedCallThroughAPointerEmbedKeepsThePointerHop(t *testing.T) {
	out := convertInlineModule(t, "app", map[string]string{"main.go": promotedOneHopPointerMain})
	main := readGenerated(t, filepath.Join(out, "main.cs"))

	// The CALL is asserted, not the file: `&x.D` renders `x.of(A.ᏑD)` here too, and correctly, since Go's `&x.D` also
	// reads the current x.B.C. The call goes through the outer type's forwarder, which takes the pointer hop itself.
	if strings.Contains(main, ".of(A.ᏑD).Bump()") {
		t.Errorf("the promoted call takes one view across a POINTER embed:\n%s", main)
	}

	if !strings.Contains(main, "var p = x.Bump();") {
		t.Errorf("the promoted call through the pointer embed is not the forwarder call `x.Bump()`:\n%s", main)
	}
}

// End to end: the write through p after the re-point lands in the OLD C, as in Go. GO2CS_INLINE_BUILD=1 runs it.
func TestAPromotedCallThroughAPointerEmbedReadsAsGo(t *testing.T) {
	runPromotedOneHopE2E(t, map[string]string{"main.go": promotedOneHopPointerMain})
}

// P3 1b review, finding 2. Outer reaches lib.Z through its foreign embed lib.X (X{Y}, Y{Z}: Go depth 2 for the field)
// and through its own W (W{lib.Z}: depth 1), so Go selects W's Z and the converter names `Outer.ᏑZ`. go2cs-gen walks
// the foreign embed through METADATA, where X's promoted Z is X's own ref property, one level down: both occurrences
// read depth 1 there.
const promotedOneHopForeignTwiceLib = `package lib

type Z struct{ N int }

func (z *Z) M() *Z { z.N++; return z }

type Y struct{ Z }
type X struct{ Y }
`

const promotedOneHopForeignTwiceMain = `package main

import (
	"fmt"

	"example.com/app/lib"
)

type W struct{ lib.Z }

type Outer struct {
	lib.X
	W
}

func main() {
	o := &Outer{}
	p := o.M()
	q := o.M()
	fmt.Println(p.N, p == q, p == &o.W.Z, o.X.N)
}
`

func TestAPromotedCallWhoseFieldAForeignEmbedAlsoReachesReadsAsGo(t *testing.T) {
	runPromotedOneHopE2E(t, map[string]string{"lib/lib.go": promotedOneHopForeignTwiceLib, "main.go": promotedOneHopForeignTwiceMain})
}

// P3 1b review, NOT CONFIRMED: a ZERO-SIZE embed reached through the one hop. go2cs-gen answers a zero-size member's
// accessor with the shared slot; what the method observes there is compared to Go.
const promotedOneHopZeroSizeMain = `package main

import "fmt"

type E struct{}

func (e *E) Hit() *E { return e }

type B struct {
	E
	n int
}

type A struct{ B }

func main() {
	x := &A{}
	p := x.Hit()
	q := x.Hit()
	fmt.Println(p == q, p == &x.E, p == &x.B.E, x.n)
}
`

func TestAPromotedCallOnAZeroSizeEmbedReadsAsGo(t *testing.T) {
	runPromotedOneHopE2E(t, map[string]string{"main.go": promotedOneHopZeroSizeMain})
}

func runPromotedOneHopE2E(t *testing.T, files map[string]string) {
	t.Helper()
	dotnet := requireInlineBuild(t)
	want := goRunInlineModule(t, "app", files)
	out := convertInlineModule(t, "app", files)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints: %s; go2cs prints: %s", want, got)

	if got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
	}
}
