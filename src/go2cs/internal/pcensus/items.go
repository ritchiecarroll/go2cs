// items.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"regexp"
	"strings"
)

// item is one row of the plan's footprint table (docs/PLAN-parity-train.md §1), or one shape of the standing
// test-refresh-1262 class, with everything both halves of the instrument need:
//   - Measures: what `count` counts (the item's OLD shape, the one its seat removes);
//   - Canon: rewrites applied to BOTH lines of a pair; the item explains a pair when the two canonical lines are
//     equal and the pair moves in the item's direction (Old loses an occurrence, or New gains one);
//   - Join: a two-line form the seat folds into one line (E1's else-if, H2's declaration and initializer);
//   - Block: line classes the seat deletes or inserts within a hunk holding both of its anchors (S1);
//   - Guard: the removed line must match it for the item to explain the pair (a shape that only means something
//     on certain lines);
//   - TestOnly: the shape is not legitimate in a production file of src/core (the test-refresh-1262 class: the
//     production emission was regenerated at the freeze; converted tests and goldens were not).
//
// Where a seat's new spelling is not ruled yet, the canon reads the plan's stated default and says so beside it.
type item struct {
	ID, Seat, Title string
	Measures        []measure
	Canon           []rewrite
	Old, New        *regexp.Regexp
	Guard           *regexp.Regexp // when set, the item explains only a pair whose removed line matches it
	Join            *joinRule
	Block           *blockRule
	TestOnly        bool
}

// rewrite is one canonicalizing replacement (Go regexp template syntax).
type rewrite struct {
	re   *regexp.Regexp
	repl string
}

// joinRule folds a removed two-line form into the one line the seat emits: when line i matches First and line i+1
// matches Second, and every Same pair of submatches (First's, Second's) is equal, the two become Fold(first's
// submatches, second's submatches).
type joinRule struct {
	First, Second *regexp.Regexp
	Same          [][2]int
	Fold          func(f, s []string) string
}

// blockRule admits a structural rewrite inside one hunk: a hunk whose removed lines hold OldAnchor and whose added
// lines hold NewAnchor may delete lines matching Delete and insert lines matching Insert, and may re-indent the
// lines it keeps.
type blockRule struct {
	OldAnchor, NewAnchor *regexp.Regexp
	Delete, Insert       []*regexp.Regexp
}

// id is a C# identifier as the converter spells one (Go identifiers, plus the converter's own letters: Ꮡ, ʗ, ᴛ, ǃ, ꓸ).
const id = `[\p{L}_][\p{L}\p{N}_]*`

// generic is a type-argument list body nested up to three deep (ж<slice<map<K, V>>>).
const generic = `(?:[^<>\n]|<(?:[^<>\n]|<[^<>\n]*>)*>)*`

// basic is the converted spelling of a basic numeric type (golib aliases the Go names; C# keywords appear too).
const basic = `(?:nint|nuint|int|uint|long|ulong|short|ushort|byte|sbyte|float|double|uintptr|rune|float32|float64)`

func rx(s string) *regexp.Regexp { return regexp.MustCompile(s) }

func m(itemID, name, pattern string) measure {
	return measure{Item: itemID, Name: name, Pattern: rx(pattern)}
}

func rw(pattern, repl string) rewrite { return rewrite{rx(pattern), repl} }

var items = []item{
	{
		ID: "N1", Seat: "#20", Title: "an interface, error or func compared to nil reads `!= nil`",
		Measures: []measure{m("N1", "cmp-default", `[!=]= default!`)},
		// Seat #20 ruled stage 1 per operand: an interface or func compare reads `nil`, while a slice, map or chan
		// compare on the SAME line keeps `default` (D1 drops its `!`). So a nil compare's literal canonicalizes
		// whatever its new spelling, the pair must hold a `default!` compare (Guard), and it moves only when a `nil`
		// compare appears (New): a compare that only lost its `!` stays D1's.
		Canon: []rewrite{
			rw(`([!=]=) (?:default!|nil\b)`, "$1 ⦃nil⦄"),
			rw(`([!=]=) default([^!(\p{L}\p{N}_]|$)`, "$1 ⦃nil⦄$2"),
		},
		New:   rx(`[!=]= nil\b`),
		Guard: rx(`[!=]= default!`),
	},
	{
		ID: "D1", Seat: "#20", Title: "`default!` becomes `default`",
		Measures: []measure{m("D1", "default-bang", `default!`)},
		Canon:    []rewrite{rw(`\bdefault!`, "default")},
		Old:      rx(`\bdefault!`),
	},
	{
		ID: "Z1", Seat: "#21", Title: "`T{}` emits `new T()`",
		Measures: []measure{m("Z1", "new-nil", `new [\p{L}_][\p{L}\p{N}_<>, ]*\(nil\)`)},
		Canon:    []rewrite{rw(`new ([@\p{L}_][@\p{L}\p{N}_<>, .]*)\(nil\)`, "new $1()")},
		Old:      rx(`new [@\p{L}_][@\p{L}\p{N}_<>, .]*\(nil\)`),
	},
	{
		ID: "C1", Seat: "#23", Title: "rune literals drop `(rune)` where C# takes a char",
		Measures: []measure{m("C1", "rune-cast", `\(rune\)'`)},
		Canon: []rewrite{
			rw(`\(\(rune\)('(?:[^'\\]|\\.)*')\)`, "$1"),
			rw(`\(rune\)'`, "'"),
		},
		Old: rx(`\(rune\)'`),
	},
	{
		// After #20, a slice, map or chan compared to nil reads `!= default` (D1 dropped the `!`); stage 2 makes it
		// `!= nil`.
		ID: "N1b", Seat: "#24", Title: "slice, map and chan compared to `nil` (N1 stage 2)",
		Measures: []measure{m("N1b", "cmp-default-nobang", `[!=]= default(?:[^!\p{L}\p{N}_]|$)`)},
		Canon:    []rewrite{rw(`([!=]=) (?:default|nil)\b(?:([^!])|$)`, "$1 ⦃nil⦄$2")},
		Old:      rx(`[!=]= default(?:[^!\p{L}\p{N}_]|$)`),
	},
	{
		ID: "R5", Seat: "#25", Title: "un-aliased pointer parameters keep their Go name",
		Measures: []measure{m("R5", "box-param", `(?:\(|, )ж<`+generic+`> Ꮡ[\p{L}_]`)},
		// A box name loses its `Ꮡ` wherever it is not a member (a field's `T.Ꮡf` is P2's, never R5's).
		Canon: []rewrite{rw(`(^|[^.\p{L}\p{N}_])Ꮡ(`+id+`)`, "$1$2")},
		Old:   rx(`(?:^|[^.\p{L}\p{N}_])Ꮡ` + id),
	},
	{
		ID: "N2", Seat: "#26", Title: "a named numeric converts to a basic numeric in one cast",
		Measures: []measure{m("N2", "double-cast", `\(`+basic+`\)\(`+basic+`\)`)},
		Canon:    []rewrite{rw(`\((`+basic+`)\)\(`+basic+`\)`, "($1)")},
		Old:      rx(`\(` + basic + `\)\(` + basic + `\)`),
	},
	{
		ID: "F1", Seat: "#27", Title: "an IIFE reads `func(() => {…})()`",
		Measures: []measure{m("F1", "iife-close", `\}\)\)\(`)},
		// The value form is #27's question 1; COORD's default `func(nint () => …)()` is the new spelling here.
		Canon: []rewrite{
			rw(`\(\(Action\)\(`, "func("),
			rw(`\(\(Func<(`+generic+`)>\)\(\(\) =>`, "func($1 () =>"),
			rw(`\}\)\)\(`, "})("),
		},
		Old: rx(`\(\((?:Action|Func<` + generic + `>)\)\(|\}\)\)\(`),
	},
	{
		ID: "DIR", Seat: "#27", Title: "a channel direction reads `SendOnly()`/`RecvOnly()`",
		Measures: []measure{m("DIR", "with-direction", `WithDirection\(`)},
		// #27's question 2; COORD's default spelling.
		Canon: []rewrite{
			rw(`\.WithDirection\(GoChanDir\.Send\)`, ".SendOnly()"),
			rw(`\.WithDirection\(GoChanDir\.Recv\)`, ".RecvOnly()"),
		},
		Old: rx(`\.WithDirection\(`),
	},
	{
		ID: "FOR", Seat: "#27", Title: "an always-true condition reads `for (;;)`",
		Measures: []measure{
			m("FOR", "while-true", `while \(ᐧ\)`),
			m("FOR", "for-true", `for \([^;\n]*; ᐧ ;`),
		},
		Canon: []rewrite{
			rw(`while \(ᐧ\)`, "for (;;)"),
			rw(`for \(([^;\n]*); ᐧ ;`, "for ($1; ;"),
		},
		Old: rx(`while \(ᐧ\)|; ᐧ ;`),
	},
	{
		ID: "SHIFT", Seat: "#27", Title: "a constant shift count in `<<=`/`>>=` drops `(int)(N)`",
		Measures: []measure{m("SHIFT", "shift-assign-cast", `(?:<<|>>)= \(int\)\(\d+\)`)},
		Canon:    []rewrite{rw(`(<<|>>)= \(int\)\((\d+)\)`, "$1= $2")},
		Old:      rx(`(?:<<|>>)= \(int\)\(\d+\)`),
	},
	{
		ID: "S1", Seat: "#28", Title: "a non-constant tag switch becomes a C# pattern switch",
		Measures: []measure{
			m("S1", "tag-temp", `var expr\x{1D1B}\d* =`),
			m("S1", "do-while-false", `\} while \(false\);`),
		},
		// The if-chain over `exprᴛN` (and its do/while(false) wrapper and continueᴛ label) is deleted and a
		// `switch (tag) { case var ᴛ when ᴛ == L: … }` inserted; the bodies it keeps may re-indent. The new form is
		// the plan's (§1 #28); its exact case spelling is #28's, so Insert admits any case label.
		Block: &blockRule{
			OldAnchor: rx(`\bvar exprᴛ\d+ = |\} while \(false\);`),
			NewAnchor: rx(`^\s*switch \(`),
			Delete: []*regexp.Regexp{
				rx(`^\s*var exprᴛ\d+ = .*;$`),
				rx(`^\s*(?:\}\s*)?(?:else )?if \(.*\bexprᴛ\d+\b.*\) \{$`),
				rx(`^\s*(?:\}\s*)?else \{(?: /\* default: \*/)?$`),
				rx(`^\s*do \{$`),
				rx(`^\s*\} while \(false\);$`),
				rx(`^\s*continueᴛ\d+:;?$`),
				rx(`^\s*goto continueᴛ\d+;$`),
				rx(`^\s*[{}]$`),
			},
			Insert: []*regexp.Regexp{
				rx(`^\s*switch \(.*\) \{$`),
				rx(`^\s*case .*:$`),
				rx(`^\s*default:$`),
				rx(`^\s*break;$`),
				rx(`^\s*[{}]$`),
			},
		},
	},
	{
		ID: "H2", Seat: "#29", Title: "the heap declaration and its initializer fold into one statement",
		Measures: []measure{m("H2", "heap-decl", `ref var `+id+` = ref heap<`+generic+`>\(out var`)},
		Canon:    []rewrite{rw(`ref heap<`+generic+`>\(`, "ref heap(")},
		Join: &joinRule{
			First:  rx(`^(\s*)ref var (` + id + `) = ref heap(?:<` + generic + `>)?\(out var Ꮡ(` + id + `)\);$`),
			Second: rx(`^(\s*)(` + id + `) = (.+);$`),
			Same:   [][2]int{{1, 1}, {2, 2}},
			Fold: func(f, s []string) string {
				return f[1] + "ref var " + f[2] + " = ref heap(" + s[3] + ", out var Ꮡ" + f[3] + ");"
			},
		},
	},
	{
		ID: "L1", Seat: "#30", Title: "slice literals as collection expressions",
		Measures: []measure{m("L1", "slice-literal", `new [^\[\]\s(){}]+\[\]\{[^{}]*\}\.slice\(\)`)},
		Canon: []rewrite{
			rw(`new [^\[\]\s(){}]+\[\]\{`, "["),
			rw(`\}\.slice\(\)`, "]"),
			// `x := []T{…}` becomes `slice<T> x = […]` (owner ruling §6.3: the type moves to the left).
			rw(`\bslice<`+generic+`> (`+id+`) = \[`, "var $1 = ["),
		},
		Old: rx(`\[\]\{|\}\.slice\(\)`),
	},
	{
		ID: "P1", Seat: "#31", Title: "`p.f` through the generated extension properties",
		Measures: []measure{
			m("P1", "deref-select", `\(~`+id+`\)\.`),
			m("P1", "value-select", id+`\.Value\.[\p{L}_]`),
		},
		Canon: []rewrite{
			rw(`\(~(`+id+`)\)\.`, "$1."),
			rw(`(`+id+`)\.Value\.([\p{L}_])`, "$1.$2"),
		},
		Old: rx(`\(~` + id + `\)\.|` + id + `\.Value\.[\p{L}_]`),
	},
	{
		ID: "P2", Seat: "#32", Title: "`&b.off` becomes `b.Ꮡoff`",
		// The plan's "(not `.Value`)" sets it apart from P1's `p.Value.f`; no `.Value.of(` exists at 5931bad29b.
		Measures: []measure{m("P2", "of-field", `\.of\([^()]*\.Ꮡ[\p{L}_]`)},
		Canon:    []rewrite{rw(`\.of\([^()]*?\.(Ꮡ`+id+`)\)`, ".$1")},
		Old:      rx(`\.of\([^()]*\.Ꮡ[\p{L}_]`),
	},
	{
		ID: "E1", Seat: "#19", Title: "`} else if (` on one line, and `) {` at the end of a line",
		Measures: []measure{
			m("E1", "else-newline-if", `\} else[ \t]*\r?\n[ \t]*if`),
			m("E1", "paren-brace-eol", `(?m)\)\{\r?$`),
		},
		// The brace may carry a trailing comment (`if (x){ // why`), and the join takes the scoping `{` of an else-if
		// with an init clause as well as an else-if's `if (` (COORD's R2 ruling: `} else {` on Go's line).
		Canon: []rewrite{rw(`\)\{(\s*//.*)?$`, ") {$1")},
		Old:   rx(`\)\{(\s*//.*)?$`),
		Join: &joinRule{
			First:  rx(`^(\s*)\} else[ \t]*$`),
			Second: rx(`^\s*(if \(.*|\{)$`),
			Fold:   func(f, s []string) string { return f[1] + "} else " + s[1] },
		},
	},

	// The standing test-refresh-1262 class (COORD rulings 2026-10-27T23:27:49Z and 23:47:36Z), legitimate only in
	// converted test files: shapes a, b and c, as R's red-tested classifier spells them (claude/r-sweep-1262
	// ce8c02a192). The GoPositionMap re-encode they force is the MAP pseudo-item in classify.go.
	{
		// The ruling's example is `});` -> `}, GoStack.Bounded);`; seat #3's goldens (4b19dd0c4, 0400b63f7) show the
		// mechanism is wider: ANY goǃ launch gains the trailing argument, `goǃ(Ꮡs.send, (nint)7, GoStack.Bounded);`.
		ID: "TRa", Seat: "test-refresh-1262", Title: "a goǃ launch gains a trailing `GoStack.Bounded` (seat #3)",
		Measures: []measure{m("TRa", "bounded", `, GoStack\.Bounded\);`)},
		Canon:    []rewrite{rw(`, GoStack\.Bounded\);\s*$`, ");")},
		New:      rx(`, GoStack\.Bounded\);\s*$`),
		Guard:    rx(`\bgoǃ\(|^\s*\}.*\);\s*$`), // a launch, or the close of a multi-line one (`}, @out);`)
		TestOnly: true,
	},
	{
		// `Ꮡ(x, i)` -> `Ꮡelem(x, i)`, and the ruling's `p.Ꮡelem<E>(i)` form, whose old spelling is `p.at<E>(i)`
		// (seat #10's goldens, b31294be2).
		ID: "TRb", Seat: "test-refresh-1262", Title: "an element-address compare reads `Ꮡelem` (seat #10)",
		Measures: []measure{m("TRb", "elem", `Ꮡelem[<(]`)},
		Canon:    []rewrite{rw(`\.Ꮡelem<`, ".at<"), rw(`Ꮡelem(?:<[^()]*>)?\(`, "Ꮡ(")},
		New:      rx(`Ꮡelem`),
		TestOnly: true,
	},
	{
		ID: "TRc", Seat: "test-refresh-1262", Title: "a deferred or go method call evaluates its receiver at the statement (seat #4)",
		Measures: []measure{m("TRc", "receiver-snapshot", `(?:defer|goǃ)\(ᴛ0 => ᴛ0\.`)},
		Canon: []rewrite{rw(`\b(defer|goǃ)\(ᴛ0 => ᴛ0\.(\w+)\(([^()]*)\), (.+?)(, ref ᒐ)?\);\s*$`,
			"$1(() => $4.$2($3)$5);")},
		New:      rx(`\b(?:defer|goǃ)\(ᴛ0 => ᴛ0\.`),
		TestOnly: true,
	},
}

func itemByID(id string) *item {
	for i := range items {
		if strings.EqualFold(items[i].ID, id) {
			return &items[i]
		}
	}
	return nil
}
