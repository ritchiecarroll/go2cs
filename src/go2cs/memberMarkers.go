// memberMarkers.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"strconv"
	"strings"
	"unicode/utf8"
)

// embedMarker marks a Go EMBEDDED field that has no `partial ref` shape to say so (a predeclared type, a
// pointer to one, an interface): `/*embed*/ public Reader Reader;`. go2cs-gen's MemberRecordGenerator reads
// it (MemberMarkers.Embed, src/gen/go2cs-gen/MemberRecordGenerator.cs) and records it on the struct, which
// is where golib's reflection reads it back (docs/PLAN-marker-comment-parity.md, 5.6).
const embedMarker = "/*embed*/"

// structTagComment is the comment converted code carries for a Go struct tag, at the end of the field's line
// after its `;`. It is Go's backquoted spelling of the tag when a backquote can hold it in a comment, and
// otherwise Go's quoted spelling, with any `*/` written `*\x2f`: both are Go's own spellings of the same string.
// go2cs-gen reads either (MemberMarkers.TagOf) and records the tag on the struct, where golib's reflection
// reads it back (docs/PLAN-marker-comment-parity.md, 5.5).
func structTagComment(tag string) string {
	if utf8.ValidString(tag) && !strings.ContainsRune(tag, '`') && !strings.Contains(tag, "*/") && strings.IndexFunc(tag, isNotPrint) < 0 {
		return "/*`" + tag + "`*/"
	}

	return "/*" + strings.ReplaceAll(strconv.Quote(tag), "*/", `*\x2f`) + "*/"
}

func isNotPrint(r rune) bool { return !strconv.IsPrint(r) }

// dimsComment is the comment converted code carries for Go array dims, directly before the type they
// describe: Go's own array prefix, `/*[32]*/ array<byte> hash`, `/*[4][8]*/` for nested arrays, the pointee's
// for a pointer to an array. go2cs-gen reads it (MemberMarkers.DimsBefore) and records it on the declaring
// type, where golib's reflection reads it back (docs/PLAN-marker-comment-parity.md, 5.4). The trailing space
// is part of the spelling: the reader takes the comment only directly before the type.
func dimsComment(dims []int64) string {
	var comment strings.Builder

	comment.WriteString("/*")

	for _, dim := range dims {
		fmt.Fprintf(&comment, "[%d]", dim)
	}

	comment.WriteString("*/ ")

	return comment.String()
}

// linknameMarker marks a linkname or assembly-trampoline FORWARDER, the first token of its declaration
// line: `/*linkname*/ internal static partial slice<@string> runtime_args() {`. go2cs-gen's
// NoInliningPartialGenerator reads it on a partial method with a body and writes [StackTraceHidden] on the
// declaring part instead of the no-inline mark (docs/PLAN-marker-comment-parity.md, section 11).
const linknameMarker = "/*linkname*/"

// The bodies (the text after `/*`) of the comments go2cs-gen reads as facts: the embed marker, the two
// openings of a tag comment, the opening of a dims comment and the linkname marker. A Go block comment
// that opens with one of them is carried with a space after its `/*` (carriedComment).
var memberMarkerOpenings = []string{strings.TrimPrefix(embedMarker, "/*"), "`", `"`, "[", "linkname"}

// carriedComment is the text converted code carries for a comment from the Go source. The member facts
// above are block comments the converter writes itself, and go2cs-gen reads one only as spelled here, so
// a Go block comment that opens the same way is written with a space after its `/*`: a comment carried
// from the Go source is never read as a marker, wherever it lands. Every other comment is carried as is.
func carriedComment(text string) string {
	if body, isBlock := strings.CutPrefix(text, "/*"); isBlock {
		for _, opening := range memberMarkerOpenings {
			if strings.HasPrefix(body, opening) {
				return "/* " + body
			}
		}
	}

	return text
}
