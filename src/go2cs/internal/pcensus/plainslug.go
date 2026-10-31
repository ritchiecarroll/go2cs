// plainslug.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"regexp"
	"strconv"
	"strings"
	"unicode/utf8"
)

// PL's canon (seat #51) names every spelling of one string literal by the slug the converter would give it: a
// hoisted field `helloWorldˢ2`, the u8 span `"hello world"u8`, the cached cast `(@string)"hello world"`, the boxed
// `(@string)"hello world"u8` and the bare `"hello world"` all read `⟪helloWorld⟫`. So a pair explains only when the
// literal that moved is the one the field stood for. The slug is the converter's own (hoistedLiteralOperations.go
// literalSlug, copied: this package cannot import package main), so a field and its literal agree exactly.

// plainLiteralForm matches one C# rendering of a Go string literal: an optional (@string) cast, a regular or verbatim
// C# string, and an optional u8 suffix.
var plainLiteralForm = regexp.MustCompile(`(?:\(@string\))?(?:@"((?:[^"]|"")*)"|"((?:[^"\\]|\\.)*)")(?:u8)?`)

// goDeferStatement matches a go/defer statement's line and its multi-line continuation, GD's lines (items.go).
var goDeferStatement = regexp.MustCompile(`^\s*(?:goǃ|defer)\(|^\s*\}[)}]*(?:, .*)?\);$`)

// goDeferOwnsLine reports whether a go/defer line's literal is GD's: a literal or hoisted field that is a DIRECT
// argument of the launch (`goǃ(f, helloˢ, …)`, `defer(f, "x"u8, ref ᒐ)`), or a thunk's own call. A literal nested in
// another call's argument (`defer(un, trace(Ꮡp, "x"u8), ref ᒐ)`) is an ordinary argument, and PL's.
func goDeferOwnsLine(line string) bool {
	if !goDeferStatement.MatchString(line) {
		return false
	}

	if strings.Contains(line, "=>") || !strings.Contains(strings.TrimSpace(line), "(") {
		return true
	}

	depth := 0

	for i := 0; i < len(line); i++ {
		switch c := line[i]; {
		case c == '(':
			depth++
		case c == ')':
			depth--
		case c == '"':
			if depth == 1 {
				return true
			}

			// Skip the literal's body; a verbatim literal (`@"…"`) takes no backslash escapes.
			verbatim := i > 0 && line[i-1] == '@'

			for i++; i < len(line) && line[i] != '"'; i++ {
				if line[i] == '\\' && !verbatim {
					i++
				}
			}
		case depth == 1 && strings.HasPrefix(line[i:], "ˢ"):
			return true
		}
	}

	return false
}

// plainFieldName matches a hoisted literal field: the slug, the marker, an optional collision ordinal.
var plainFieldName = regexp.MustCompile(`\b([A-Za-z][A-Za-z0-9]*)ˢ\d*`)

func plainSlugToken(s string) string {
	return "⟪" + s + "⟫"
}

// canonPlainLiterals respells every literal rendering and every hoisted field in a line as its slug token.
func canonPlainLiterals(line string) string {
	// A char literal holding a quote (`'"'`, `'\"'`) would open a string to the literal's pattern.
	line = strings.NewReplacer(`'"'`, `'⟪q⟫'`, `'\"'`, `'⟪q⟫'`).Replace(line)

	line = plainLiteralForm.ReplaceAllStringFunc(line, func(m string) string {
		sub := plainLiteralForm.FindStringSubmatch(m)

		if strings.Contains(m, `@"`) {
			return plainSlugToken(literalSlug(decodeCSharpString(sub[1], true)))
		}

		return plainSlugToken(literalSlug(decodeCSharpString(sub[2], false)))
	})

	return plainFieldName.ReplaceAllString(line, "⟪$1⟫")
}

// decodeCSharpString decodes a C# string literal's body: a verbatim literal doubles its quotes, a regular one takes
// C#'s escapes. Only the slug reads the result, so an escape the converter never emits decodes to itself.
func decodeCSharpString(body string, verbatim bool) string {
	if verbatim {
		return strings.ReplaceAll(body, `""`, `"`)
	}

	var b strings.Builder

	for i := 0; i < len(body); i++ {
		if body[i] != '\\' || i+1 == len(body) {
			b.WriteByte(body[i])
			continue
		}

		i++

		switch c := body[i]; c {
		case 'n':
			b.WriteByte('\n')
		case 't':
			b.WriteByte('\t')
		case 'r':
			b.WriteByte('\r')
		case 'a':
			b.WriteByte('\a')
		case 'b':
			b.WriteByte('\b')
		case 'f':
			b.WriteByte('\f')
		case 'v':
			b.WriteByte('\v')
		case '0':
			b.WriteByte(0)
		case 'u', 'U', 'x':
			width := map[byte]int{'u': 4, 'U': 8, 'x': 4}[c]
			j := i + 1

			for j < len(body) && j < i+1+width && strings.ContainsRune("0123456789abcdefABCDEF", rune(body[j])) {
				j++
			}

			if v, err := strconv.ParseUint(body[i+1:j], 16, 32); err == nil && j > i+1 {
				var buf [utf8.UTFMax]byte
				b.Write(buf[:utf8.EncodeRune(buf[:], rune(v))])
				i = j - 1
			} else {
				b.WriteByte(c)
			}
		default:
			b.WriteByte(c)
		}
	}

	return b.String()
}

// literalSlug, camelCaseSlugWord, startsWithLetter and isASCIILetter are hoistedLiteralOperations.go's, verbatim.
const maxHoistSlugLength = 24

func literalSlug(value string) string {
	var words []string
	var current strings.Builder

	for _, r := range value {
		if isASCIILetter(r) || (r >= '0' && r <= '9') {
			current.WriteRune(r)
			continue
		}

		if current.Len() > 0 {
			words = append(words, current.String())
			current.Reset()
		}
	}

	if current.Len() > 0 {
		words = append(words, current.String())
	}

	for len(words) > 0 && !startsWithLetter(words[0]) {
		words = words[1:]
	}

	if len(words) == 0 {
		return ""
	}

	var slug strings.Builder

	for i, word := range words {
		next := camelCaseSlugWord(word, i == 0)

		if slug.Len()+len(next) > maxHoistSlugLength {
			break
		}

		slug.WriteString(next)

		if slug.Len() >= maxHoistSlugLength {
			break
		}
	}

	return slug.String()
}

func camelCaseSlugWord(word string, first bool) string {
	allUpper := true
	hasLetter := false

	for i := 0; i < len(word); i++ {
		if isASCIILetter(rune(word[i])) {
			hasLetter = true

			if word[i] >= 'a' && word[i] <= 'z' {
				allUpper = false
			}
		}
	}

	if hasLetter && allUpper {
		word = strings.ToLower(word)
	}

	if first {
		run := 0
		for run < len(word) && word[run] >= 'A' && word[run] <= 'Z' {
			run++
		}

		if run > 1 {
			if run < len(word) && word[run] >= 'a' && word[run] <= 'z' {
				run--
			}

			return strings.ToLower(word[:run]) + word[run:]
		}

		return strings.ToLower(word[:1]) + word[1:]
	}

	return strings.ToUpper(word[:1]) + word[1:]
}

func startsWithLetter(s string) bool {
	for _, r := range s {
		return isASCIILetter(r)
	}

	return false
}

func isASCIILetter(r rune) bool {
	return (r >= 'a' && r <= 'z') || (r >= 'A' && r <= 'Z')
}
