// cslex.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package commentcensus

import (
	"strings"
)

// csComment is one comment in a C# file. A block comment spanning lines yields one csComment per line.
type csComment struct {
	line     int    // 1-based
	col      int    // visual column of the comment start (tabs expanded to 4)
	text     string // normalized text (delimiters stripped, whitespace collapsed)
	trailing bool   // code precedes it on the same line
	block    bool
}

type csFile struct {
	lines    []string
	comments []csComment
	// codeOnLine[i] is true when line i+1 carries non-comment, non-blank text
	codeOnLine []bool
	// codeText[i] is the line's code with comments and string/char literal contents removed
	codeText []string
}

func visualCol(line string, byteIdx int, tab int) int {
	col := 0
	for i, r := range line {
		if i >= byteIdx {
			break
		}
		if r == '\t' {
			col += tab - col%tab
		} else {
			col++
		}
	}
	return col
}

func normText(s string) string {
	return strings.Join(strings.Fields(s), " ")
}

// lexCS finds comments in C# source, skipping string and char literals (regular, verbatim, interpolated,
// and raw """ strings) so that a // inside a literal is never read as a comment.
func lexCS(src string) *csFile {
	src = strings.ReplaceAll(src, "\r\n", "\n")
	f := &csFile{lines: strings.Split(src, "\n")}
	n := len(f.lines)
	f.codeOnLine = make([]bool, n)
	f.codeText = make([]string, n)
	inBlock := false
	blockStartLine := 0
	_ = blockStartLine
	inVerbatim := false // verbatim or raw string continuing across lines
	rawQuotes := 0
	for li, line := range f.lines {
		var code strings.Builder
		i := 0
		for i < len(line) {
			if inBlock {
				end := strings.Index(line[i:], "*/")
				seg := line[i:]
				if end >= 0 {
					seg = line[i : i+end]
				}
				t := normText(seg)
				if t != "" && t != "*" {
					f.comments = append(f.comments, csComment{line: li + 1, col: visualCol(line, i, 4), text: strings.TrimPrefix(t, "* "), trailing: false, block: true})
				}
				if end < 0 {
					i = len(line)
					break
				}
				i += end + 2
				inBlock = false
				continue
			}
			if inVerbatim {
				if rawQuotes > 0 {
					q := strings.Repeat("\"", rawQuotes)
					end := strings.Index(line[i:], q)
					if end < 0 {
						i = len(line)
						break
					}
					i += end + rawQuotes
					inVerbatim = false
					rawQuotes = 0
					code.WriteString("\"\"")
					continue
				}
				// verbatim: "" is an escaped quote
				for i < len(line) {
					if line[i] == '"' {
						if i+1 < len(line) && line[i+1] == '"' {
							i += 2
							continue
						}
						i++
						inVerbatim = false
						code.WriteString("\"")
						break
					}
					i++
				}
				continue
			}
			c := line[i]
			switch {
			case c == '/' && i+1 < len(line) && line[i+1] == '/':
				t := normText(line[i+2:])
				trailing := strings.TrimSpace(code.String()) != ""
				f.comments = append(f.comments, csComment{line: li + 1, col: visualCol(line, i, 4), text: strings.TrimPrefix(strings.TrimPrefix(t, "/"), " "), trailing: trailing})
				i = len(line)
			case c == '/' && i+1 < len(line) && line[i+1] == '*':
				end := strings.Index(line[i+2:], "*/")
				trailing := strings.TrimSpace(code.String()) != ""
				if end >= 0 {
					t := normText(line[i+2 : i+2+end])
					// value comments the converter writes inside code (/* '�' */) are recorded too;
					// they never match a Go comment's text, so they cannot hide a miss
					f.comments = append(f.comments, csComment{line: li + 1, col: visualCol(line, i, 4), text: t, trailing: trailing, block: true})
					code.WriteString(" ")
					i += 2 + end + 2
				} else {
					t := normText(line[i+2:])
					if t != "" {
						f.comments = append(f.comments, csComment{line: li + 1, col: visualCol(line, i, 4), text: t, trailing: trailing, block: true})
					}
					inBlock = true
					i = len(line)
				}
			case c == '"' && i+2 < len(line) && line[i+1] == '"' && line[i+2] == '"':
				// raw string literal
				q := 3
				for i+q < len(line) && line[i+q] == '"' {
					q++
				}
				end := strings.Index(line[i+q:], strings.Repeat("\"", q))
				code.WriteString("\"\"")
				if end < 0 {
					inVerbatim = true
					rawQuotes = q
					i = len(line)
				} else {
					i += q + end + q
				}
			case c == '@' && i+1 < len(line) && line[i+1] == '"', c == '$' && i+2 < len(line) && line[i+1] == '@' && line[i+2] == '"', c == '@' && i+2 < len(line) && line[i+1] == '$' && line[i+2] == '"':
				j := strings.IndexByte(line[i:], '"') + i + 1
				code.WriteString("\"")
				i = j
				inVerbatim = true
				for i < len(line) {
					if line[i] == '"' {
						if i+1 < len(line) && line[i+1] == '"' {
							i += 2
							continue
						}
						i++
						inVerbatim = false
						code.WriteString("\"")
						break
					}
					i++
				}
			case c == '"':
				i++
				for i < len(line) {
					if line[i] == '\\' {
						i += 2
						continue
					}
					if line[i] == '"' {
						i++
						break
					}
					i++
				}
				code.WriteString("\"\"")
			case c == '\'':
				// char literal: '\'' or 'x' or '\u0000'
				j := i + 1
				for j < len(line) {
					if line[j] == '\\' {
						j += 2
						continue
					}
					if line[j] == '\'' {
						j++
						break
					}
					j++
				}
				code.WriteString("' '")
				i = j
			default:
				code.WriteByte(c)
				i++
			}
		}
		f.codeText[li] = code.String()
		f.codeOnLine[li] = strings.TrimSpace(code.String()) != ""
	}
	return f
}
