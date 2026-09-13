// displacedRefReceiverCall_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the CALL SITES of a displaced method whose receiver is emitted as a `[GoRecv] this ref T`
// primary rather than as the box (`this ж<T>`).
//
// manualConversionFuncs displaces a body; it does not — and must not — change how the declaration's
// CALLERS spell the receiver. convSelectorExpr carried the opposite assumption: every listed
// "Recv.func" entry was taken to be a BOX-receiver hand-own (the assumption reads correctly against
// runtime's `g.guintptr`, which captures the receiver's identity and genuinely takes `this ж<g>`),
// so a registered method called on a deref-aliased pointer was emitted as `Ꮡrecv.M(…)` whatever the
// method's own emitted receiver form. That is an assumption, not a derivation, and exprIsDerefAliasedPointer
// — the predicate it was paired with — is not a box-existence predicate: it answers "does `x` already
// render as the pointed-to VALUE, so a `~` deref would be CS0023?", which is true of a pointer
// PARAMETER (box `Ꮡp` IS the parameter) and equally true of a pointer RECEIVER on a `[GoRecv] this
// ref T` primary, where NO box is declared at all.
//
// The measured instance (lane R, 2026-09-03): registering reflect's `abiSeq.regAssign` — a
// `[GoRecv] internal static bool regAssign(this ref abiSeq a, …)` — displaced the body correctly
// (one placeholder, zero duplicate bodies) and rewrote its one caller, `addArg`, whose receiver is
// also `this ref abiSeq a`:
//
//	if (!a.regAssign(Ꮡt, 0)) {      ->      if (!Ꮡa.regAssign(Ꮡt, 0)) {
//
// abi.cs(151,10): error CS0103: The name 'Ꮡa' does not exist in the current context.
//
// The non-displaced path never reaches this shape, and TestRefReceiverCallKeepsRefFormWithoutDisplacement
// is the control that says so: a `ref`-receiver method calling another `ref`-receiver method on its
// own receiver already emits the bare form, because the capture-mode fixpoint only routes a caller
// through a box when the CALLEE is direct-ж — and promotes the caller to direct-ж when it is, so the
// box it spells always exists. The registration bypassed that fixpoint; the fix restores it by
// requiring a box in scope rather than assuming one.
//
// The two box-bearing arms are kept as controls, not as decoration: they are the axes the fix must
// NOT move (a displaced method reached through a pointer PARAMETER, and through the receiver of a
// DIRECT-ж caller), and they are what makes the corpus-inertness of the change a measurement rather
// than a hope — `g.guintptr` is the pointer-parameter arm's corpus witness.

package main

import (
	"strings"
	"testing"
	"unicode"
)

// refReceiverCallSource is the fixture for all three arms — one conversion, three receiver KINDS.
//
//   - `assign` is the displaced callee. Its body takes no field address, does not return, reassign,
//     close over or pass on its receiver, and defers nothing, so the capture-mode pass leaves it a
//     plain `[GoRecv] this ref seq` primary — the shape reflect's regAssign has.
//   - `add` calls it on ITS OWN `ref` receiver: the defect's shape, no box in scope.
//   - `addViaParam` calls it on a pointer PARAMETER: the box `Ꮡp` IS the parameter, so the box form
//     is correct and must survive.
//   - `addBoxed` takes its receiver's field address, which makes it direct-ж (`this ж<seq> Ꮡs`), so
//     its receiver box exists and the box form is correct there too.
const refReceiverCallSource = `package main

type seq struct {
	steps []int
	total int
}

func (s *seq) assign(n int) bool {
	if n <= 0 {
		return false
	}
	s.total += n
	return true
}

func (s *seq) add(n int) bool {
	s.steps = append(s.steps, n)
	if !s.assign(n) {
		s.total = 0
		return false
	}
	return true
}

func (s *seq) addBoxed(n int) bool {
	p := &s.total
	*p += n
	return s.assign(n)
}

func addViaParam(p *seq, n int) bool {
	return p.assign(n)
}

func main() {
	var s seq
	_ = s.add(3)
	_ = s.addBoxed(4)
	_ = addViaParam(&s, 5)
	println(s.total)
}
`

// callReceiverToken returns the WHOLE receiver token of the line's `.assign(` call. It reads
// backwards in RUNES over the identifier characters — `Ꮡ` is a Cherokee letter, three bytes in
// UTF-8, so a byte-wise scan would split it — and the comparison is against the whole token rather
// than a `strings.Contains`: `Ꮡs.assign(` CONTAINS `s.assign(`, so a substring reject over these
// glyph-prefixed names over-matches by construction and would pass on the very emission it exists
// to refuse.
func callReceiverToken(line string) (string, bool) {
	index := strings.Index(line, ".assign(")

	if index < 0 {
		return "", false
	}

	prefix := []rune(line[:index])
	start := len(prefix)

	for start > 0 {
		r := prefix[start-1]

		if r != '_' && r != '@' && !unicode.IsLetter(r) && !unicode.IsDigit(r) {
			break
		}

		start--
	}

	return string(prefix[start:]), true
}

// requireCallForm asserts the first `assign` call emitted at or after `enclosing`'s declaration
// spells its receiver exactly `wantRecv`. Each of the fixture's three callers holds exactly one
// such call and they are emitted in source order, so the first hit after a declaration is that
// declaration's own.
func requireCallForm(t *testing.T, lines []string, enclosing string, wantRecv string) {
	t.Helper()

	start := findDeclarationLine(t, lines, enclosing)

	for i := start; i < len(lines); i++ {
		token, ok := callReceiverToken(lines[i])

		if !ok {
			continue
		}

		if token != wantRecv {
			t.Fatalf("%s called the displaced callee on receiver %q, want %q — a box spelled in a `[GoRecv] ref` body is not declared there (CS0103):\n%s", enclosing, token, wantRecv, strings.TrimSpace(lines[i]))
		}

		return
	}

	t.Fatalf("%s emitted no call to the displaced callee:\n%s", enclosing, strings.Join(lines, "\n"))
}

// TestDisplacedRefReceiverCallKeepsRefForm is the witness: with `seq.assign` displaced, the caller
// whose own receiver is a `[GoRecv] ref` primary must keep the bare receiver, and the two callers
// that DO hold a box must keep the box form.
func TestDisplacedRefReceiverCallKeepsRefForm(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	displacedFixture(t, "example.com/app", "seq.assign")

	lines := convertWithComments(t, refReceiverCallSource)

	// The registration displaced the body — without this the test would be asserting against the
	// ordinary converted path and could not fail for the reason it exists.
	findCommentLine(t, lines, "func assign is hand-converted")

	// The defect: no `Ꮡs` is declared in a `[GoRecv] this ref seq` body.
	requireCallForm(t, lines, "add(", "s")

	// Control — a pointer PARAMETER's box IS the parameter, so the box form is right and must stay.
	requireCallForm(t, lines, "addViaParam(", AddressPrefix+"p")

	// Control — a direct-ж caller's receiver box `Ꮡs` is its own parameter, so the box form is
	// right there too. This is the arm that keeps the fix from collapsing into "never emit Ꮡ".
	requireCallForm(t, lines, "addBoxed(", AddressPrefix+"s")
}

// TestRefReceiverCallKeepsRefFormWithoutDisplacement is the control for the DISPLACEMENT axis: the
// identical fixture with no registration. It passes on the defective converter — the box form was
// only ever reached through the registration — and it is what makes "a behavioral guard cannot see
// this" precise: the emission a behavioral package can produce is already correct, so only a
// converter-level test with the registry in reach can hold the invariant.
func TestRefReceiverCallKeepsRefFormWithoutDisplacement(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	lines := convertWithComments(t, refReceiverCallSource)

	requireAbsent(t, lines, "func assign is hand-converted")
	requireCallForm(t, lines, "add(", "s")
}
