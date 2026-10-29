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
//     equal and the pair moves in the item's direction (Old, or any one of Olds, loses an occurrence, or New gains
//     one; Olds keeps shapes apart that one pair can trade, as P1's `(~X).f` read becoming `X.Value.f`'s field);
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
	Olds            []*regexp.Regexp
	Guard           *regexp.Regexp // when set, the item explains only a pair whose removed line matches it
	Join            *joinRule
	Block           *blockRule
	TestOnly        bool
	// Respaces marks an item whose change is the spacing its Canon removes, with no shape that appears or goes:
	// a pair its Canon makes equal moves whenever the two lines differ.
	Respaces bool
}

// rewrite is one canonicalizing replacement (Go regexp template syntax), or, when fn is set, a function of each match.
type rewrite struct {
	re   *regexp.Regexp
	repl string
	fn   func(string) string
}

// joinRule folds a removed two-line form into the one line the seat emits: when line i matches First and line i+1
// matches Second, and every Same pair of submatches (First's, Second's) is equal, the two become Fold(first's
// submatches, second's submatches).
type joinRule struct {
	First, Second *regexp.Regexp
	Same          [][2]int
	Fold          func(f, s []string) string
}

// blockRule admits a structural rewrite across a FILE: a real diff keeps the unchanged lines between as context, so
// the rewrite's scaffolding lands in many runs and its moved lines in two. In a file whose diff removes an OldAnchor
// line and adds a NewAnchor line, the lines no run explains are read together at the file's end (classifier
// endFile): a removed and an added line of equal canonical form pair (with Moves, a clause the old form emitted out
// of source order; a re-indented kept line; a label whose replacement sits in another run), then Delete and Insert
// admit the scaffolding, and what is left is refused. A PerSide rule gates each side on its own anchor instead: a
// file may delete Delete lines when its removed lines hold OldAnchor, and insert Insert lines when its added lines
// hold NewAnchor, a nil anchor holding always (P1's records, deleted from a converted file and inserted into
// package_info.cs).
type blockRule struct {
	OldAnchor, NewAnchor *regexp.Regexp
	Delete, Insert       []*regexp.Regexp
	// Standalone admits a hunk that holds OldAnchor alone: a declaration the seat deletes far from the
	// rewrite that retires it (#33's hoisted fields) shows as a deletion hunk with no NewAnchor.
	Standalone bool
	Moves      bool
	PerSide    bool
	// File, when set, confines the rule to files whose path it matches (A1's alias lines, only in package_info.cs).
	File *regexp.Regexp
	// Hunk, when set, admits an Insert line only in a hunk whose `@@` header matches it: git names the enclosing
	// declaration there (CM's member docs, only inside `partial interface X`).
	Hunk *regexp.Regexp
	// Closer, when set, admits an Insert line only where a Closer line ends the inserted lines it starts, in the same
	// hunk (CM's doc, closed by the line naming the member the interface inherits).
	Closer *regexp.Regexp
}

// derefSelects respells every `(~X).` as `X.`, X any expression with balanced parentheses: P1's read through a
// pointer, whose base may be a chain, a call or an index (`(~(~e.prev).next).x` reads `e.prev.next.x`). A `(~X)`
// not followed by `.` is kept.
func derefSelects(s string) string {
	for from := len(s); ; {
		open := strings.LastIndex(s[:from], "(~")

		if open < 0 {
			return s
		}

		depth, close := 0, -1

	scan:
		for k := open; k < len(s); k++ {
			switch s[k] {
			case '(':
				depth++
			case ')':
				if depth--; depth == 0 {
					close = k
					break scan
				}
			}
		}

		if close >= 0 && close+1 < len(s) && s[close+1] == '.' {
			s = s[:open] + s[open+2:close] + s[close+1:]
		}

		from = open
	}
}

// valueSelect is one `X.Value.f` of P1's write through a pointer, f possibly `@`-escaped.
var valueSelect = rx(`([\p{L}\p{N}_\])])\.Value\.([\p{L}_@])`)

// valueSelects respells every `X.Value.f` as `X.f`, to a fixed point: a match consumes the first letter of its field,
// so a chain through a one-letter field (`cst.Value.c.Value.Timeout`) needs a second pass.
func valueSelects(s string) string {
	for {
		next := valueSelect.ReplaceAllString(s, "$1.$2")

		if next == s {
			return s
		}

		s = next
	}
}

// id is a C# identifier as the converter spells one (Go identifiers, plus the converter's own letters: Ꮡ, ʗ, ᴛ, ǃ, ꓸ).
const id = `[\p{L}_][\p{L}\p{N}_]*`

// generic is a type-argument list body nested up to three deep (ж<slice<map<K, V>>>).
const generic = `(?:[^<>\n]|<(?:[^<>\n]|<[^<>\n]*>)*>)*`

// dirMark is the converter's channel-direction comment inside a type name (`channel/*<-*/<T>`): one token, whose `<`
// opens no type-argument list.
const dirMark = `/\*<-\*/`

// sliceLitElem is a slice literal's C# element type in `new T[]{` or `slice<T>(`: no bracket, brace, paren or space
// outside its generic arguments, which may hold spaces and nest three deep (`ж<array<array<nint>>>`), and a channel's
// direction comment at any depth.
const sliceLitElem = `(?:` + dirMark + `|[^\[\]\s(){}<>]|<(?:` + dirMark + `|[^<>\n]|<(?:` + dirMark + `|[^<>\n]|<[^<>\n]*>)*>)*>)+`

// castName is the type a conversion names in its `(T)` prefix.
const castName = `@?[\p{L}_][\p{L}\p{N}_.]*`

// basic is the converted spelling of a basic numeric type (golib aliases the Go names; C# keywords appear too).
const basic = `(?:nint|nuint|int|uint|long|ulong|short|ushort|byte|sbyte|float|double|uintptr|rune|float32|float64)`

// sizedBasic is basic plus golib's sized aliases (int8 … uint64), the spelling the converter writes for a named
// numeric's underlying. N2's classifier needs it: the converter's hop is `(nint)(int8)lvl`, `(uint64)(uintptr)tk`.
// The plan's sizing measure keeps basic, so its counts stay comparable with the plan table (§1).
const sizedBasic = `(?:nint|nuint|int|uint|long|ulong|short|ushort|byte|sbyte|float|double|uintptr|rune|float32|float64|int8|int16|int32|int64|uint8|uint16|uint32|uint64)`

func rx(s string) *regexp.Regexp { return regexp.MustCompile(s) }

func m(itemID, name, pattern string) measure {
	return measure{Item: itemID, Name: name, Pattern: rx(pattern)}
}

func rw(pattern, repl string) rewrite { return rewrite{re: rx(pattern), repl: repl} }

// statementEnd is the index of the `;` that ends the C# statement s begins, or -1 when s does not end it (a value
// that continues on the next line): the first `;` outside parentheses, brackets, braces and string or character
// literals, so a lambda body's own statements stay inside the value.
func statementEnd(s string) int {
	depth := 0
	for i := 0; i < len(s); i++ {
		switch c := s[i]; c {
		case '"', '\'':
			for i++; i < len(s) && s[i] != c; i++ {
				if s[i] == '\\' {
					i++
				}
			}
		case '(', '[', '{':
			depth++
		case ')', ']', '}':
			depth--
		case ';':
			if depth == 0 {
				return i
			}
		}
	}
	return -1
}

// rwf is a rewrite computed from each match: fn receives the regexp's submatches.
func rwf(pattern string, fn func(sub []string) string) rewrite {
	re := rx(pattern)
	return rewrite{re: re, fn: func(match string) string { return fn(re.FindStringSubmatch(match)) }}
}

// An IIFE's head, in the cast form F1 retires and in the `func(…)` form it emits. Both normalize to one spelling that
// keeps the attribute, the result type, every parameter type and every parameter name, `func⦃attr|result|T1, T2|a, b⦄
// =>`, so the two heads of one literal are equal exactly when all of those agree. A variadic `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ`
// literal never takes the IIFE path, and its head matches neither pattern.
const (
	iifeAttr    = `(\[[^\]\n]*\] )?`
	iifeOldHead = `\(\((Action|Func)(?:<(` + generic + `)>)?\)\(` + iifeAttr + `(\([^()\n]*\)|[^\s()=,]+) =>`
	iifeNewHead = `func\(` + iifeAttr + `(?:(\([^()\n]*\)|[^\s()]+) )?\(((?:[^()\n]|\([^()\n]*\))*)\) =>`
)

// trailingCommentStart is the index of the `//` that opens a line's trailing comment: the first one outside string
// and character literals (a verbatim string's doubled quote included), or -1 when the line has none.
func trailingCommentStart(s string) int {
	for i := 0; i < len(s); i++ {
		switch c := s[i]; {
		case c == '@' && i+1 < len(s) && s[i+1] == '"':
			for i += 2; i < len(s); i++ {
				if s[i] == '"' {
					if i+1 < len(s) && s[i+1] == '"' {
						i++
						continue
					}
					break
				}
			}
		case c == '"' || c == '\'':
			for i++; i < len(s) && s[i] != c; i++ {
				if s[i] == '\\' {
					i++
				}
			}
		case c == '/' && i+1 < len(s) && s[i+1] == '/':
			return i
		}
	}
	return -1
}

// padTrailingComment respells the spaces between a line's code and its trailing comment as one.
func padTrailingComment(line string) string {
	at := trailingCommentStart(line)
	if at <= 0 {
		return line
	}
	code := strings.TrimRight(line[:at], " ")
	if code == "" || len(code) == at {
		return line
	}
	return code + " " + line[at:]
}

func iifeNormal(attr, result string, types, names []string) string {
	return "func⦃" + attr + "|" + result + "|" + strings.Join(types, ", ") + "|" + strings.Join(names, ", ") + "⦄ =>"
}

// splitTop splits a list at its top-level commas (not those inside <…> or (…)), each element trimmed; "" is empty.
func splitTop(s string) []string {
	var parts []string
	depth, start := 0, 0
	for i, r := range s {
		switch r {
		case '<', '(':
			depth++
		case '>', ')':
			depth--
		case ',':
			if depth == 0 {
				parts = append(parts, strings.TrimSpace(s[start:i]))
				start = i + 1
			}
		}
	}
	if last := strings.TrimSpace(s[start:]); last != "" || len(parts) > 0 {
		parts = append(parts, last)
	}
	return parts
}

// iifeOldNormal reads `((Func<T1, T2, R>)((a, b) => ` or `((Action<T1>)(a => `: the delegate's type arguments are the
// parameter types (and, for a Func, the result last); the lambda gives the names.
func iifeOldNormal(sub []string) string {
	args, params := splitTop(sub[2]), sub[4]
	result := ""
	if sub[1] == "Func" && len(args) > 0 {
		result, args = args[len(args)-1], args[:len(args)-1]
	}
	names := []string{params}
	if strings.HasPrefix(params, "(") {
		names = splitTop(params[1 : len(params)-1])
	}
	if len(names) != len(args) {
		return sub[0]
	}
	return iifeNormal(sub[3], result, args, names)
}

// iifeNewNormal reads `func(R (T1 a, T2 b) => `, with an optional attribute before the result.
func iifeNewNormal(sub []string) string {
	var types, names []string
	for _, param := range splitTop(sub[3]) {
		cut := strings.LastIndex(param, " ")
		if cut < 0 {
			return sub[0]
		}
		types, names = append(types, strings.TrimSpace(param[:cut])), append(names, param[cut+1:])
	}
	return iifeNormal(sub[1], sub[2], types, names)
}

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
			// A narrowing cast around a cast literal keeps one cast: (byte)((rune)'x') reads (byte)'x'.
			rw(`(\(`+basic+`\))\(\(rune\)('(?:[^'\\]|\\.)*')\)`, "$1$2"),
			// A conversion's operand loses its own cast: (byte)(rune)'a' reads (byte)'a', (rune)(rune)'b' reads (rune)'b'.
			rw(`(\(`+basic+`\))\(rune\)'`, "$1'"),
			// A func literal's return spells its result type, `=> (rune)'u'` reads `=> (uint16)'u'`: the cast that
			// follows a lambda arrow or a return is the result type's.
			rw(`(=> |return )\(`+sizedBasic+`\)'`, "$1'"),
			// A byte, int8 or int16 slot spells its one cast as the target (COORD's ruling on #23, Q1): (rune)'q'
			// reads (byte)'q'. The converter spells those types by their Go names.
			rw(`\((?:byte|int8|int16)\)'`, "'"),
			rw(`\(rune\)'`, "'"),
		},
		// Any (rune) cast the pair loses: a conversion's operand, (rune)(rune)'b' -> (rune)'b', keeps one (rune)' on
		// each side and loses only the inner cast.
		Old: rx(`\(rune\)`),
	},
	{
		// After #20, a slice, map or chan compared to nil reads `!= default` (D1 dropped the `!`); stage 2 makes it
		// `!= nil`. Go's `nil == s` keeps its order, so `default == s` reads `nil == s`.
		ID: "N1b", Seat: "#24", Title: "slice, map and chan compared to `nil` (N1 stage 2)",
		Measures: []measure{m("N1b", "cmp-default-nobang", `[!=]= default(?:[^!\p{L}\p{N}_]|$)|(?:^|[^!.\p{L}\p{N}_])default [!=]=`)},
		Canon: []rewrite{
			rw(`([!=]=) (?:default|nil)\b(?:([^!])|$)`, "$1 ⦃nil⦄$2"),
			rw(`(^|[^!.⦃\p{L}\p{N}_])(?:default|nil) ([!=]=)`, "$1⦃nil⦄ $2"),
		},
		Old: rx(`[!=]= default(?:[^!\p{L}\p{N}_]|$)|(?:^|[^!.\p{L}\p{N}_])default [!=]=`),
	},
	{
		ID: "R5", Seat: "#25", Title: "un-aliased pointer parameters keep their Go name",
		Measures: []measure{m("R5", "box-param", `(?:\(|, )ж<`+generic+`> Ꮡ[\p{L}_]`)},
		// A box name loses its `Ꮡ` wherever it is not a member (a field's `T.Ꮡf` is P2's, never R5's). The name it
		// takes is the parameter's analyzed one: a C# keyword escaped (`Ꮡout` → `@out`), a collision prefixed or
		// numbered (`Ꮡp` → `Δp`, `Ꮡrand` → `randΔ1`). Both lines drop that decoration, so a pair compares on the Go
		// name; Old still requires the pair to lose a box token.
		Canon: []rewrite{
			rw(`(^|[^.\p{L}\p{N}_])Ꮡ(`+id+`)`, "$1$2"),
			rw(`(^|[^.\p{L}\p{N}_])[@Δ](`+id+`)`, "$1$2"),
			rw(`(^|[^.\p{L}\p{N}_])([\p{L}_][\p{L}\p{N}_]*?)Δ\d+`, "$1$2"),
		},
		Old: rx(`(?:^|[^.\p{L}\p{N}_])Ꮡ` + id),
	},
	{
		ID: "N2", Seat: "#26", Title: "a named numeric converts to a basic numeric in one cast",
		Measures: []measure{m("N2", "double-cast", `\(`+basic+`\)\(`+basic+`\)`)},
		// The seat drops the INNER cast of a named numeric's hop: right after the outer cast, `(nint)(int8)lvl`,
		// or opening the outer cast's parenthesized operand, `(uint8)((uintptr)((flag)x))`, and a parenthesized
		// operand loses a paren pair with it, `(uint8)(uint16)((x >> 8))` -> `(uint8)(x >> 8)`. Both lines drop
		// every sized cast that directly follows a parenthesis and compare runs of parens as one, so the pair
		// matches when its only difference is such a cast. Old requires the pair to lose a sized cast (a count
		// of double casts cannot: `(int64)((float64)(uint64)t` shares its middle cast between two), and a
		// cast before a rune literal is C1's (`(byte)(rune)'A'`), never dropped here. The drop runs three times: one pass
		// consumes the parenthesis an adjacent second cast needs.
		Canon: []rewrite{
			// C1's casts first: every cast in a run that ends at a rune literal is set aside, so the drop below
			// cannot take the outer cast of (byte)(rune)'a'. Three passes, as for the drop.
			rw(`\((`+sizedBasic+`)\)((?:⟨[a-z0-9]+⟩)*)'`, "⟨$1⟩$2'"),
			rw(`\((`+sizedBasic+`)\)((?:⟨[a-z0-9]+⟩)*)'`, "⟨$1⟩$2'"),
			rw(`\((`+sizedBasic+`)\)((?:⟨[a-z0-9]+⟩)*)'`, "⟨$1⟩$2'"),
			rw(`([()])\(`+sizedBasic+`\)([^'])`, "$1$2"),
			rw(`([()])\(`+sizedBasic+`\)([^'])`, "$1$2"),
			rw(`([()])\(`+sizedBasic+`\)([^'])`, "$1$2"),
			rw(`\(\(+`, "("),
			rw(`\)\)+`, ")"),
		},
		Old: rx(`\(` + sizedBasic + `\)`),
	},
	{
		ID: "F1", Seat: "#27", Title: "an IIFE reads `func(() => {…})()`",
		Measures: []measure{m("F1", "iife-close", `\}\)\)\(`)},
		// The value form is #27's question 1, ruled as option A: `func(nint () => …)()`, the parameters typed on the
		// lambda. Both heads normalize to one spelling (iifeOldNormal, iifeNewNormal).
		Canon: []rewrite{
			rwf(iifeOldHead, iifeOldNormal),
			rwf(iifeNewHead, iifeNewNormal),
			rw(`\}\)\)\(`, "})("),
		},
		Old: rx(`\(\((?:Action|Func)(?:<` + generic + `>)?\)\(|\}\)\)\(`),
	},
	{
		ID: "DIR", Seat: "#27", Title: "a channel direction reads `AsSendOnly()`/`AsRecvOnly()`",
		Measures: []measure{m("DIR", "with-direction", `WithDirection\(`)},
		// #27's question 2, as COORD ruled it: `As…`, since channel<T>'s static `SendOnly`/`RecvOnly` are the
		// NIL directional channels and an instance method of the same name is CS0102.
		Canon: []rewrite{
			rw(`\.WithDirection\(GoChanDir\.Send\)`, ".AsSendOnly()"),
			rw(`\.WithDirection\(GoChanDir\.Recv\)`, ".AsRecvOnly()"),
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
		// The if-chain over `exprᴛN` (and its do/while(false) wrapper and continueᴛ label) becomes
		// `switch (tag) { case …: { … break; } … default: { … } }`, as #28 ruled (R1-R3): one label per Go clause; a
		// clause the chain tested with `is` keeps its constant pattern (`case A or B: {`), the rest become
		// `case var ᴛ when ᴛ == A || ᴛ == B: {` (AreEqual and the named-wrapper cast kept); a nested pattern switch
		// names its variable `ᴛ2`, `ᴛ3`. A tag whose Go type is a string keeps its temp under the expressionless
		// governor (A2): `var exprᴛ1 = tag;` stays, `switch (ᐧ) {` is inserted and each label is
		// `case {} when exprᴛ1 == L: {`. The canon PAIRS each line that carries a label or the tag with the line that
		// replaces it, so a label whose value or operator changed is refused; the block only admits the scaffolding
		// either side, and a default body the chain had moved to the end is admitted back in Go's position as a move.
		// (Text cannot see the tag's type, so which of the two forms a switch took is the converter's test to hold.)
		Canon: []rewrite{
			rw(`^(\s*)case \{\} when (.*\bexprᴛ\d+\b.*): \{( //.*)?$`, "${1}case var ᴛ when $2: {$3"),
			rw(`^(\s*)\S+ exprᴛ\d+ = (.*);$`, "${1}switch ($2) {"),
			rw(`^(\s*)(?:else )?if \(exprᴛ\d+ is (.*)\) \{( //.*)?$`, "${1}case $2: {$3"),
			rw(`^(\s*)(?:else )?if \((.*\bexprᴛ\d+\b.*)\) \{( //.*)?$`, "${1}case var ᴛ when $2: {$3"),
			rw(`^(\s*)(?:else )?\{ /\* default: \*/( //.*)?$`, "${1}default: {$2"),
			rw(`\bexprᴛ\d+\b`, "ᴛ"),
			rw(`(^|[^\p{L}\p{N}_])ᴛ\d+\b`, "${1}ᴛ"),
			rw(`^(\s*)goto continueᴛ\d+;$`, "${1}continue;"),
		},
		Old: rx(`\bexprᴛ\d+\b|goto continueᴛ\d+;|/\* default: \*/`),
		New: rx(`^\s*case \{\} when .*\bexprᴛ\d+\b`),
		Block: &blockRule{
			OldAnchor: rx(`^\s*\S+ exprᴛ\d+ = .*;$|^\s*(?:else )?if \(.*\bexprᴛ\d+\b.*\) \{`),
			NewAnchor: rx(`^\s*switch \(`),
			Delete: []*regexp.Regexp{
				rx(`^\s*else \{$`),
				rx(`^\s*do \{$`),
				rx(`^\s*\} while \(false\);$`),
				rx(`^\s*continueᴛ\d+:;?$`),
				rx(`^\s*[{}]$`),
			},
			Insert: []*regexp.Regexp{
				rx(`^\s*switch \(ᐧ\) \{$`),
				rx(`^\s*break;$`),
				rx(`^\s*\}\}?$`),
			},
			Moves: true,
		},
	},
	{
		ID: "H2", Seat: "#29", Title: "the heap declaration and its initializer fold into one statement",
		Measures: []measure{m("H2", "heap-decl", `ref var `+id+` = ref heap<`+generic+`>\(out var`)},
		// COORD's R1 keeps the type argument: `ref var x = ref heap<T>(rhs, out var Ꮡx);`, the zero box's own
		// spelling with the value inserted, so the fold keeps the declaration's <T> and a changed or dropped one
		// is refused. The value is the assignment's, written `x = v;` or, for a local a closure captures,
		// `Ꮡx.ValueSlot = v;`; it ends at the statement's own `;` (statementEnd), so a trailing comment or an
		// if-init's condition after it stays where it was. A value spanning several lines folds its FIRST line into
		// the declaration (no `;` closes it there) and closes on its last: `…;` becomes `…, out var Ꮡx);` (the
		// canon pairs the two).
		Canon: []rewrite{rw(`, out var Ꮡ`+id+`\);`, ";")},
		New:   rx(`, out var Ꮡ` + id + `\);`),
		Join: &joinRule{
			// The type argument is taken whole, up to the box's `(out var`: a channel's direction rides in it as a
			// comment (`heap</*<-*/channel<T>>`), which the bracket-counting `generic` cannot read.
			First:  rx(`^(\s*)ref var (@?` + id + `) = ref heap(<.+>)\(out var Ꮡ(` + id + `)\);$`),
			Second: rx(`^(\s*)Ꮡ?(@?` + id + `)(?:\.ValueSlot|\.Value)? = (.*)$`),
			Same:   [][2]int{{1, 1}, {2, 2}},
			Fold: func(f, s []string) string {
				head := f[1] + "ref var " + f[2] + " = ref heap" + f[3] + "("
				end := statementEnd(s[3])
				if end < 0 {
					return head + s[3]
				}
				return head + s[3][:end] + ", out var Ꮡ" + f[4] + ");" + s[3][end+1:]
			},
		},
	},
	{
		ID: "L1", Seat: "#30", Title: "slice literals as collection expressions",
		Measures: []measure{m("L1", "slice-literal", `new [^\[\]\s(){}]+\[\]\{[^{}]*\}\.slice\(\)`)},
		// COORD's ruling of 2026-10-28: a collection expression cannot target slice<T> (its enumerator yields the
		// Go range pair), so the literal is golib's builtin over one, `new nint[]{1, 2}.slice()` -> `slice<nint>([1,
		// 2])`, and a local keeps `var`. The element type may carry spaced generic arguments (`map<@string, nint>`),
		// nested three deep (`ж<array<array<nint>>>`). A cast over the literal composes: its operand is now a CALL,
		// so the conversion takes the single-paren form every call operand already takes, `(@string)f()` --
		// `((@string)new byte[]{1}.slice())` becomes `(@string)slice<byte>([1])`. Over a MULTI-LINE literal the open
		// line drops its paren by the canon, and the close line `}.slice())` -> `])` is the Block's: alone it cannot
		// tell a cast's close from a call's `]))`, so it reads L1 only in a file whose diff shows the cast's two opens.
		Canon: []rewrite{
			rw(`new (`+sliceLitElem+`)\[\]\{`, "slice<$1>(["),
			rw(`\}\.slice\(\)`, "])"),
			rw(`\(\((`+castName+`)\)(slice<`+sliceLitElem+`>\(\[[^\n]*?\]\))\)`, "($1)$2"),
			rw(`\(\((`+castName+`)\)(slice<`+sliceLitElem+`>\(\[)$`, "($1)$2"),
		},
		Old: rx(`\[\]\{|\}\.slice\(\)`),
		Block: &blockRule{
			OldAnchor: rx(`\(\(` + castName + `\)new ` + sliceLitElem + `\[\]\{$`),
			NewAnchor: rx(`\(` + castName + `\)slice<` + sliceLitElem + `>\(\[$`),
			Delete:    []*regexp.Regexp{rx(`^\s*\}\.slice\(\)\)[;,]?$`)},
			Insert:    []*regexp.Regexp{rx(`^\s*\]\)[;,]?$`)},
		},
	},
	{
		ID: "P1", Seat: "#31", Title: "`p.f` through the generated extension properties",
		Measures: []measure{
			m("P1", "deref-select", `\(~`+id+`\)\.`),
			m("P1", "value-select", id+`\.Value\.[\p{L}_]`),
		},
		// #31's base is any expression, not an identifier only: a chain, a call or an index (`(~(~e.prev).next).x`,
		// `(~mk()).Y`, `m["k"].Value.X`). Its records move into package_info.cs: each record type is an inserted
		// `[assembly: GoFieldAccess(typeof(T))]` line, under its prose, and the per-file `[GoFieldAccess(…)]` line the
		// unsafe.Pointer route wrote on a package class is deleted (G, 2026-10-28, the owner's ask).
		Canon: []rewrite{
			{re: rx(`^.*$`), fn: derefSelects},
			{re: rx(`^.*$`), fn: valueSelects},
		},
		Olds: []*regexp.Regexp{rx(`\(~`), valueSelect},
		Block: &blockRule{
			PerSide:   true,
			Delete:    []*regexp.Regexp{rx(`^\s*\[GoFieldAccess\(.*\)\]$`)},
			NewAnchor: rx(`^\[assembly: (?:global::)?(?:go\.)?GoFieldAccess\(typeof\(`),
			Insert: []*regexp.Regexp{
				rx(`^\s*\[assembly: (?:global::)?(?:go\.)?GoFieldAccess\(typeof\(.*\)\)\]$`),
				rx(`^// The struct types whose fields this package reaches through a pointer, as Go spells it, ` + "`p.f`" + `:$`),
				rx(`^// go2cs-gen emits a field property on the pointer for each of their fields\.$`),
				rx(`^$`),
			},
		},
	},
	{
		ID: "P2", Seat: "#32", Title: "`&b.off` becomes `b.Ꮡoff`",
		// The plan's "(not `.Value`)" sets it apart from P1's `p.Value.f`; no `.Value.of(` exists at 5931bad29b.
		Measures: []measure{m("P2", "of-field", `\.of\([^()]*\.Ꮡ[\p{L}_]`)},
		Canon:    []rewrite{rw(`\.of\([^()]*?\.(Ꮡ`+id+`)\)`, ".$1")},
		Old:      rx(`\.of\([^()]*\.Ꮡ[\p{L}_]`),
	},
	{
		ID: "GD", Seat: "#33", Title: "a go/defer statement's constant argument moves into the thunk's call",
		Measures: []measure{m("GD", "hoisted-godefer-arg", `\b(?:goǃ|defer)\(.*\wˢ\d*[,)]`)},
		// The seat rewrites a go/defer statement's argument list: a constant leaves the capture list for the
		// thunk's call (`goǃ(ᴛ1 => fmt.Println(ᴛ1), firstˢ)` -> `goǃ(() => fmt.Println("First"))`), the eager
		// temps renumber, and a launch that keeps its form spells a string constant `(@string)"…"` in place
		// of a hoisted field. Both lines reduce to the statement and its callee; the pair must lose a temp or
		// a hoisted field, or gain a `() =>` thunk or a literal constant inside the thunk's call (a method
		// group that takes the thunk keeps a temp for each eager argument: `defer(triple, labelˢ, n, ref ᒐ)` ->
		// `defer(ᴛ1 => triple("label", ᴛ1), n, ref ᒐ)`). The hoisted fields the moved literals claimed, and their
		// banner, are deleted wherever they stood, often a hunk of their own above the function (Standalone).
		// A statement whose argument is a multi-line func literal ends on a continuation line, which loses the
		// eager constants it carried: `}), (nint)80, ref ᒐ);` -> `}), ref ᒐ);`. Such a line reduces to its
		// indentation, and it must lose a LITERAL constant (a named constant there is refused rather than
		// read, so a line that loses a variable can never classify).
		Canon: []rewrite{
			rw(`^(\s*)(goǃ|defer)\((?:(?:\([^()]*\)|ᴛ\d+) => )?(@?[\p{L}_][\p{L}\p{N}_ʗ.]*)\b.*$`, "$1$2 ⦃$3⦄"),
			rw(`^(\s*)\}[)}]*(?:, .*)?\);$`, "$1⦃go/defer continuation⦄"),
		},
		Guard: rx(`^\s*(?:goǃ|defer)\(|^\s*\}[)}]*(?:, .*)?\);$`),
		Old:   rx(`ᴛ\d|ˢ\d*[,)]|^\s*\}[)}]*, (?:\([\p{L}\p{N}_<>]+\))?(?:-?\d|"|'|true\b|false\b)`),
		New:   rx(`\(\) => |=> @?[\p{L}_][\p{L}\p{N}_ʗ.]*\((?:[^()]*, )?(?:\([\p{L}\p{N}_<>]+\))?(?:"|'|-?\d|true\b|false\b)`),
		Block: &blockRule{
			OldAnchor:  rx(`^\s*(?:private|internal) static readonly (?:object|@string) \w+ˢ\d* = `),
			Standalone: true,
			Delete: []*regexp.Regexp{
				rx(`^\s*(?:private|internal) static readonly (?:object|@string) \w+ˢ\d* = .*;$`),
				rx(`^\s*// Hoisted @string literals \(single allocation; Go keeps these in RODATA\)$`),
				rx(`^\s*$`),
			},
		},
	},
	{
		// A Go trailing comment pads to the column gofmt gave its run (docs/PLAN-comment-and-spacing-parity.md, class
		// J): only the spaces between a line's code and its comment change, so the item has no count of its own.
		ID: "J", Seat: "#38", Title: "a Go trailing comment pads to the column gofmt gave its run",
		Canon:    []rewrite{{re: rx(`^.*$`), fn: padTrailingComment}},
		Respaces: true,
	},
	{
		// The imported-alias use gate: a `global using X = …;` alias no file of the package uses is no longer written
		// into package_info.cs. The line is only ever deleted, and only there; an alias whose target changes is a
		// different change, and its added line is refused. Whether the deleted alias is still used is a fact of the
		// emission, which a diff cannot show: this item reads the shape only.
		ID: "A1", Seat: "#41", Title: "an imported type alias no file uses leaves package_info.cs",
		Block: &blockRule{
			PerSide: true,
			File:    rx(`(^|/)package_info\.cs$`),
			Delete:  []*regexp.Regexp{rx(`^global using [^\s=;]+ = [^\s;]+;$`)},
		},
	},
	{
		// The Go comments C# still dropped are carried (docs/PLAN-comment-and-spacing-parity.md, seat #43):
		//   - a blank import emits no `using`, and with a Go trailing comment its note becomes a block comment, so Go's
		//     comment stands as its own token: `// blank import: … // for linkname` -> `/* blank import: … */ // for linkname`;
		//   - a type spec's trailing comment rides its declaration line, after any fact (`partial struct sortedHint
		//     /*num:nint*/;` gains `// hint …`), as does a linkname var's on its property line;
		//   - an interface member C# inherits rather than declares keeps its doc in the body, closed by the line that
		//     names it as Go spells it: `// Read(b []byte) (n int, err error) is provided by io.ReadWriteCloser`, or
		//     `// io.Writer is inherited` for Go's own embed. Those lines are only inserted (the Block).
		ID: "CM", Seat: "#43", Title: "Go comments C# dropped: a blank import's note, a declaration's trailing comment, an inherited member's doc",
		Canon: []rewrite{
			rw(`^(\s*)/\* (blank import: .*?) \*/( +//)`, "$1// $2$3"),
			rw(`^(\s*partial (?:struct|class) `+id+`(?: /\*.*?\*/)?(?:;| \{))\s*//.*$`, "$1"),
			rw(`^(\s*(?:public|internal) static .+ \{ get => .+; set => .+; \})\s*//.*$`, "$1"),
		},
		Guard: rx(`^\s*// blank import: |^\s*partial (?:struct|class) |^\s*(?:public|internal) static .+ \{ get => `),
		Old:   rx(`^\s*// blank import: .*\) //`),
		New:   rx(`^\s*partial (?:struct|class) .*(?:;| \{)\s*//|^\s*(?:public|internal) static .+ \{ get => .+ \}\s*//`),
		Block: &blockRule{
			PerSide: true,
			Hunk:    rx(`^@@ [^@]*@@ partial interface `),
			// A doc line, a Go paragraph break (a bare `//`) among them.
			Insert: []*regexp.Regexp{rx(`^\s+//(?:[ \t].*)?$`)},
			Closer: rx(`^\s+// \S.* is (?:provided by \S.*|inherited)$`),
		},
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
