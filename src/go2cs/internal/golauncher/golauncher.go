// golauncher.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Package golauncher is the ONE definition of which goǃ calls an emitted C# file holds, and in what order: the
// ordinal a [GoBoundedGoroutine] record carries. Two readers need the same answer and cannot see each other: the
// converter, which writes each record (package main), and the line-keyed record guard (internal/repoguard, which
// cannot import main). go2cs-gen counts the same calls with Roslyn's own tokens.
package golauncher

import (
	"unicode"
	"unicode/utf16"
)

// Site is one goǃ call: the 1-based line and 1-based UTF-16 column its token starts at, and its 0-based ordinal among
// the file's goǃ calls in reading order.
type Site struct {
	Line    int
	Column  int
	Ordinal int
}

// Name is the identifier a go statement's emission calls, and the one a bounded record's ordinal counts.
const Name = "goǃ"

// Positions answers where each `goǃ` call of an emitted file starts, in reading order: its 1-based line and 1-based
// UTF-16 column, with the 0-based ordinal it holds. A call is what go2cs-gen's record check counts, an invocation whose
// callee is the bare identifier `goǃ` (written `@goǃ` or not): not a member access `x.goǃ(…)`, a generic `goǃ<T>(…)` or
// a method group. Comments, string literals (regular, verbatim, raw and interpolated, holes included) and character
// literals are skipped. A goǃ call inside an interpolation hole is not counted; the converter writes none, and the
// generator, which would count it, refuses the record that disagrees. A file's records are keyed by both the position
// and the ordinal: a line added above a call moves the call, and a record left at the old position lands on a
// NEIGHBOUR's `goǃ` there, which the position alone cannot tell from the right one.
func Positions(text string) []Site {
	runes := []rune(text)
	var starts []int

	// prev is the last significant character the scan passed, outside comments and whitespace: a `.` there makes the
	// next identifier a member name. A string or character literal leaves its quote.
	var prev rune

	isIdent := func(r rune) bool { return r == '_' || unicode.IsLetter(r) || unicode.IsDigit(r) }

	for i := 0; i < len(runes); {
		r := runes[i]

		switch {
		case r == '/' && i+1 < len(runes) && runes[i+1] == '/':
			for i < len(runes) && runes[i] != '\n' {
				i++
			}
		case r == '/' && i+1 < len(runes) && runes[i+1] == '*':
			i += 2

			for i+1 < len(runes) && !(runes[i] == '*' && runes[i+1] == '/') {
				i++
			}

			i += 2
		case r == '\'':
			i = skipCharLiteral(runes, i)
			prev = '\''
		case r == '"' || ((r == '$' || r == '@') && stringPrefixEnds(runes, i) >= 0):
			i = skipStringLiteral(runes, i)
			prev = '"'
		case isIdent(r):
			start := i

			for i < len(runes) && isIdent(runes[i]) {
				i++
			}

			if string(runes[start:i]) == Name && prev != '.' && isCalledBare(runes, i) {
				if start > 0 && runes[start-1] == '@' {
					start--
				}

				starts = append(starts, start)
			}

			prev = 'a'
		default:
			// A verbatim identifier's `@` belongs to the identifier after it, so it leaves prev as it was.
			if !unicode.IsSpace(r) && r != '@' {
				prev = r
			}

			i++
		}
	}

	sites := make([]Site, 0, len(starts))
	line, column, next := 1, 1, 0

	for i, r := range runes {
		if next < len(starts) && starts[next] == i {
			sites = append(sites, Site{Line: line, Column: column, Ordinal: next})
			next++
		}

		if r == '\n' {
			line, column = line+1, 1
		} else {
			column += utf16.RuneLen(r)
		}
	}

	return sites
}

// isCalledBare reports whether the identifier that ends at end is a callee: followed by `(` (not a method group, not
// a generic name). Whether it follows a `.` is the scan's own question (prev in Positions), since a comment can sit
// between.
func isCalledBare(runes []rune, end int) bool {
	for end < len(runes) && unicode.IsSpace(runes[end]) {
		end++
	}

	return end < len(runes) && runes[end] == '('
}

// stringPrefixEnds answers the index of the first quote of a string literal whose `$`/`@` prefix starts at i, or -1
// when the runes at i begin no string literal (a verbatim identifier, say).
func stringPrefixEnds(runes []rune, i int) int {
	j := i

	for j < len(runes) && j-i < 8 && (runes[j] == '$' || runes[j] == '@') {
		j++
	}

	if j < len(runes) && runes[j] == '"' {
		return j
	}

	return -1
}

// skipCharLiteral returns the index just past the character literal starting at i.
func skipCharLiteral(runes []rune, i int) int {
	i++

	for i < len(runes) && runes[i] != '\'' && runes[i] != '\n' {
		if runes[i] == '\\' {
			i++
		}

		i++
	}

	return i + 1
}

// skipStringLiteral returns the index just past the string literal (with any `$`/`@` prefix) starting at i. An
// interpolated literal's holes are code, and a string or character literal inside a hole is skipped with its own rules,
// so a quote there cannot end the outer literal.
func skipStringLiteral(runes []rune, i int) int {
	dollars, verbatim := 0, false

	for i < len(runes) && (runes[i] == '$' || runes[i] == '@') {
		if runes[i] == '$' {
			dollars++
		} else {
			verbatim = true
		}

		i++
	}

	quotes := 0

	for i+quotes < len(runes) && runes[i+quotes] == '"' {
		quotes++
	}

	if quotes == 2 {
		return i + 2
	}

	raw := quotes >= 3

	if !raw {
		quotes = 1
	}

	i += quotes

	for i < len(runes) {
		switch r := runes[i]; {
		case r == '"':
			run := 0

			for i+run < len(runes) && runes[i+run] == '"' {
				run++
			}

			if raw {
				if run >= quotes {
					return i + run
				}

				i += run
				continue
			}

			if verbatim && run >= 2 {
				i += 2
				continue
			}

			return i + 1
		case r == '\\' && !verbatim && !raw:
			i += 2
		case r == '\n' && !verbatim && !raw:
			return i
		case r == '{' && dollars > 0:
			open := 0

			for i+open < len(runes) && runes[i+open] == '{' {
				open++
			}

			if open < dollars || (dollars == 1 && open >= 2) {
				i += open
				continue
			}

			i = skipInterpolationHole(runes, i+open)
		default:
			i++
		}
	}

	return i
}

// skipInterpolationHole returns the index just past the `}` that closes the interpolation hole whose code starts at i.
func skipInterpolationHole(runes []rune, i int) int {
	depth := 0

	for i < len(runes) {
		switch r := runes[i]; {
		case r == '"' || ((r == '$' || r == '@') && stringPrefixEnds(runes, i) >= 0):
			i = skipStringLiteral(runes, i)
		case r == '\'':
			i = skipCharLiteral(runes, i)
		case r == '{' || r == '(' || r == '[':
			depth++
			i++
		case r == ')' || r == ']':
			depth--
			i++
		case r == '}':
			if depth == 0 {
				return i + 1
			}

			depth--
			i++
		default:
			i++
		}
	}

	return i
}
