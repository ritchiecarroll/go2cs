// displacedPromotedCall_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Guards the call sites of a displaced method reached THROUGH AN EMBEDDED FIELD — the promoted
// selection `a.add(r)` on `AddrRanges{addrRanges}` with `addrRanges.add` registered.
//
// convSelectorExpr's manual-box-receiver arm spells `Ꮡ<X>.<method>(…)` for a registered method
// called on a receiver whose box is in scope. That is right for a DIRECT selection (the registered
// `g.guintptr` on a `*g`), and wrong for a PROMOTED one: the receiver of a promoted method is the
// EMBED, so `Ꮡa.add(…)` names the embedding type's box, which the hand-own's own receiver cannot
// bind — and the promoted forwarder the generators would otherwise mint on the embedding type is
// refused whenever the method's name collides with a package-level function (Go lets `func add(…)`
// and `func (a *addrRanges) add(…)` coexist; runtime has both), so nothing can bind it.
//
// The measured instance (C1, runtime increment 7, 2026-09-05): registering `addrRanges.init`,
// `add` and `cloneInto` rewrote export_test.go's `a.add(r.addrRange)` inside `(*AddrRanges).Add`
//
//	Ꮡa.of(AddrRanges.ᏑaddrRanges).add(r.addrRange);   ->   Ꮡa.add(r.addrRange);
//
// export_test.cs(1146,5): error CS1929: 'ж<AddrRanges>' does not contain a definition for 'add' …
//
// — a `-tests` host build failure no production build reaches, because the corpus's own callers
// name the field directly (`p.inUse.add(…)`), which is not a promoted selection. The fix gates the
// arm to direct selections; a promoted one falls through to the hop machinery, which answers the
// shape from the Go body (the box hop for a direct-ж callee) exactly as it does for an unregistered
// method — so the displaced and the undisplaced emissions of the promoted call are IDENTICAL, and
// that identity is what this guard asserts, beside the direct-selection control the arm must keep.

package main

import (
	"strings"
	"testing"
)

// promotedCallSource: `ranges.add` takes a field address (a direct-ж box primary in the emission);
// `add` the package-level function shares its name (the collision that refuses a promoted
// forwarder); `Ranges` value-embeds `ranges` and calls `add` through the embed; `addDirect` is the
// direct-selection control the arm must keep spelling as the box.
const promotedCallSource = `package main

type ranges struct {
	items []int
	total int
}

func (r *ranges) add(n int) {
	p := &r.items
	*p = append(*p, n)
	r.total += n
}

func add(a, b int) int {
	return a + b
}

type Ranges struct {
	ranges
	mutable bool
}

func (r *Ranges) Add(n int) {
	if !r.mutable {
		return
	}
	r.add(n)
}

func addDirect(r *ranges, n int) {
	r.add(n)
}

func main() {
	var r Ranges
	r.mutable = true
	r.Add(add(1, 2))
	addDirect(&r.ranges, 4)
	println(r.total)
}
`

// promotedCallReceiver returns the receiver EXPRESSION of the first `.add(` call at or after the
// named declaration — the whole text before `.add(` on that line, trimmed — so a hop
// (`Ꮡr.of(Ranges.Ꮡranges)`) and a bare box (`Ꮡr`) compare as different strings rather than as a
// substring of one another.
func promotedCallReceiver(t *testing.T, lines []string, enclosing string) string {
	t.Helper()

	start := findDeclarationLine(t, lines, enclosing)

	for i := start; i < len(lines); i++ {
		line := strings.TrimSpace(lines[i])

		if index := strings.Index(line, ".add("); index >= 0 {
			return line[:index]
		}
	}

	t.Fatalf("%s emitted no call to add:\n%s", enclosing, strings.Join(lines, "\n"))
	return ""
}

// TestDisplacedPromotedCallKeepsTheHop is the witness: with `ranges.add` displaced, the promoted
// call inside `(*Ranges).Add` must spell the same receiver the undisplaced conversion spells (the
// hop through the embed), never the embedding type's own box; the direct-selection control keeps
// the box form on both sides.
func TestDisplacedPromotedCallKeepsTheHop(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	control := convertWithComments(t, promotedCallSource)
	requireAbsent(t, control, "func add is hand-converted")
	wantPromoted := promotedCallReceiver(t, control, "Add(")
	wantDirect := promotedCallReceiver(t, control, "addDirect(")

	if !strings.Contains(wantPromoted, ".of(") {
		t.Fatalf("control: the promoted call's receiver %q is not the hop through the embed; the fixture no longer exercises the promoted shape", wantPromoted)
	}

	displacedFixture(t, "example.com/app", "ranges.add")

	lines := convertWithComments(t, promotedCallSource)
	findCommentLine(t, lines, "func add is hand-converted")

	if got := promotedCallReceiver(t, lines, "Add("); got != wantPromoted {
		t.Fatalf("displaced: the promoted call spells receiver %q, want the undisplaced hop %q (the embedding type's box cannot bind the embed's method, and the colliding package function refuses a promoted forwarder — CS1929)", got, wantPromoted)
	}

	if got := promotedCallReceiver(t, lines, "addDirect("); got != wantDirect {
		t.Fatalf("displaced: the DIRECT call spells receiver %q, want %q — the arm's direct-selection behaviour must not move", got, wantDirect)
	}
}
