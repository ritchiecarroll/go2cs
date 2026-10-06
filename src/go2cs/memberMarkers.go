// memberMarkers.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "strings"

// embedMarker marks a Go EMBEDDED field that has no `partial ref` shape to say so (a predeclared type, a
// pointer to one, an interface): `/*embed*/ public Reader Reader;`. go2cs-gen's MemberRecordGenerator reads
// it (MemberMarkers.Embed, src/gen/go2cs-gen/MemberRecordGenerator.cs) and records it on the struct, which
// is where golib's reflection reads it back (docs/PLAN-marker-comment-parity.md, 5.6).
const embedMarker = "/*embed*/"

// The bodies (the text after `/*`) of the comments go2cs-gen reads as member facts. A Go block comment that
// opens with one of them is carried with a space after its `/*` (carriedComment).
var memberMarkerOpenings = []string{strings.TrimPrefix(embedMarker, "/*")}

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
