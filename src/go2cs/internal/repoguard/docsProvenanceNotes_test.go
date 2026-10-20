// docsProvenanceNotes_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"crypto/sha256"
	"encoding/hex"
	"flag"
	"fmt"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"
	"testing"
)

// A hidden provenance note must still point at what it describes. The pages keep their evidence in HTML
// comments: `<!-- … src/tests/Behavioral/UnsafeOperations/UnsafeOperations.cs.target:91 (`…`) -->`, a
// path and a line beside the sentence it justifies. Nothing else reads those notes. When a golden is
// re-baselined, a converted file regenerated or a converter file edited, the cited line moves, the note
// keeps the old number and the page still builds: the evidence silently points at other code. The golden
// re-baseline that added Go's blank lines moved about half of them at once.
//
// WHAT IS A NOTE: an HTML comment outside fenced code on a living page (livingMarkdownPages) that is not
// a `<!-- source: … -->` sample citation (those sit above a fenced sample and are checked by
// regenerating the sample). A TOKEN in a note is <path>:<ranges>, the ranges N or N-M joined by commas
// (`:22,28`, `:44-48,141`); a bare `:N-M` after a token (`:1226-1240 and :2416-2424`) continues that
// token's file.
//
// HOW A TOKEN RESOLVES, in order (provenanceNoteResolve): a GOROOT/ path is the pinned Go toolchain's
// and is not read; a tracked path from the repository root, with any leading ../ dropped; the previous
// resolved token's directory joined to the path (`PointerToPointer.go:25 + PointerToPointer.cs.target:24`);
// the previous token's stem when the path starts with '.' or is `cs.target` (`ArrayRangeSnapshot.go:18 ->
// .cs.target:21`); a directory the note declares with "under <dir>" or "in <dir>"; a tracked path that
// ends with it, when exactly one does. A path that ends in two or more tracked files is AMBIGUOUS and
// fails: write enough of it to name one. A path that starts at a repository top directory (src/, docs/,
// .github/, .claude/) and is not tracked is MISSING and fails. Any other untracked path is outside the
// repository (a toolchain file, a script on a lane's box) and is not read.
//
// WHAT IS CHECKED (provenanceNoteCheck): every cited line exists, and at least one is not blank; if a
// FINGERPRINT follows the token (`bufio.cs:17#1a2b3c`: the first six hex digits of the SHA-256 of the
// cited lines, whitespace collapsed and blank lines dropped), the cited lines still hash to it, and when
// they do not, the lines that now do are named if there is one such place; if a backtick quote follows,
// the cited lines hold it (whitespace collapsed; a quote shortened with … must hold each piece in order of
// appearance anywhere in the lines). A token with neither fails: it could only be checked for existence,
// which is how a note's lines drifted silently. Quote what a note cites, or fingerprint it:
// `go test ./internal/repoguard -run 'TestDocsProvenanceNotesResolve$' -args -note-fingerprints` adds the
// missing fingerprints from the lines cited now. It never rewrites one, so first make sure each new or
// unfingerprinted token cites the lines it describes.
//
// WHAT IS NOT READ: provenanceNoteRecordPages, pages whose notes are dated evidence of a moment rather
// than descriptions of the tree; and the rows of provenanceNoteKnownStale, notes that describe emission
// the converter no longer writes, whose visible prose is owed a rewrite. A known-stale row that no longer
// fails is itself a failure, so the list only shrinks.
//
// POSITIVE CONTROL: TestProvenanceNoteGuardFiresOnAPlantedStaleNote moves one real note on
// ConversionStrategies.md to the wrong line, and names it. Verified red-first when this guard landed: the
// same edit made to the committed page fails TestDocsProvenanceNotesResolve at that line.

// provenanceNoteRecordPages are living pages whose notes cite a moment, not the tree.
var provenanceNoteRecordPages = map[string]string{
	"docs/GoCorpusMigration.md": "the Go hop runbook: its notes are dated lesson provenance, citing the tree, the ledger and lane scripts at the commit or date each lesson names",
}

// provenanceNoteKnownStale names, by page and resolved token, the notes that no longer describe the tree.
// Each is excused while it fails; the fix is owed where the reason says. The three it was born with (the
// slice-bound examples) were fixed with their prose.
//
// The five PLAN-hop-go126.md rows are one dated note ("Instrument states read at master 20279db5f6 on
// 2026-10-11"), fingerprinted from the lines it was written against (16e02568d5); the instruments have
// changed since, so the lines at those numbers now say something else. The plan's owner re-cites them at
// the lines that now hold each instrument state, or retires the note.
var provenanceNoteKnownStale = map[string]string{
	"docs/PLAN-hop-go126.md: src/_roster.ps1:475":                    "dated instrument note; the plan's owner re-cites it",
	"docs/PLAN-hop-go126.md: src/_roster.ps1:587":                    "dated instrument note; the plan's owner re-cites it",
	"docs/PLAN-hop-go126.md: src/run-h10-recon.ps1:841-843":          "dated instrument note; the plan's owner re-cites it",
	"docs/PLAN-hop-go126.md: docs/phase4/hopA-inputs/shardmap.py:94": "dated instrument note; the plan's owner re-cites it",
	"docs/PLAN-hop-go126.md: src/check-roster-format.ps1:906":        "dated instrument note; the plan's owner re-cites it",
}

var (
	provenanceNoteToken = regexp.MustCompile(
		`((?:\.\./)*(?:[\w.\-ж]+/)*[\w.\-ж]*\.(?:cs\.target|go|cs|target|csproj|props|targets|md|ps1|sh|yml|yaml|json|txt|s|py|slnx|xml)):` +
			`(\d+(?:-\d+)?(?:,\s?\d+(?:-\d+)?)*)` +
			`|(?:^|[\s(;,])(?:and )?:(\d+(?:-\d+)?(?:,\s?\d+(?:-\d+)?)*)`)
	provenanceNoteQuote = regexp.MustCompile("^\\s*\\(?\\s*`([^`\n]+)`")
	provenanceNoteUnder = regexp.MustCompile(`(?:under|in) ((?:src|docs)/[\w./\-ж]+)`)
	provenanceNoteRoot  = regexp.MustCompile(`^(?:src|docs|\.github|\.claude)/`)
	provenanceNoteElide = regexp.MustCompile(`…|\.\.\.`)
	provenanceNoteFP    = regexp.MustCompile(`^#([0-9a-f]{6})\b`)
)

// noteFingerprints asks TestDocsProvenanceNotesResolve to add each missing fingerprint before it checks.
var noteFingerprints = flag.Bool("note-fingerprints", false, "add a fingerprint to every unquoted, unfingerprinted provenance-note token, from the lines it cites now")

// provenanceNoteHow is how a token that can only be checked for existence is fixed.
const provenanceNoteHow = "quote what it cites, or fingerprint it (go test ./internal/repoguard -run 'TestDocsProvenanceNotesResolve$' -args -note-fingerprints, once it cites the right lines)"

// noteTree is what a note is resolved against: the tracked paths and a way to read one.
type noteTree struct {
	tracked []string
	set     map[string]bool
	read    func(rel string) ([]string, bool)
}

func newNoteTree(tracked []string, read func(rel string) ([]string, bool)) noteTree {
	set := make(map[string]bool, len(tracked))

	for _, t := range tracked {
		set[t] = true
	}

	return noteTree{tracked: tracked, set: set, read: read}
}

type noteToken struct {
	line   int
	text   string
	ranges [][2]int
	fp     string
	quote  string
	kind   string
	file   string
	cands  int
}

// provenanceNoteBodies returns each note's text, with the page line each character came from.
func provenanceNoteBodies(text string) (bodies []string, lineOf [][]int) {
	lines := strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n")
	fence := ""
	var cur *strings.Builder
	var curLines []int

	for i, line := range lines {
		if cur == nil {
			if m := blockFence.FindStringSubmatch(line); m != nil {
				switch {
				case fence == "":
					fence = m[1]
				case m[1][0] == fence[0] && len(m[1]) >= len(fence):
					fence = ""
				}

				continue
			}

			if fence != "" {
				continue
			}
		}

		pos := 0

		for {
			if cur == nil {
				k := strings.Index(line[pos:], "<!--")

				if k < 0 {
					break
				}

				cur, curLines = &strings.Builder{}, nil
				pos += k + 4
			} else if pos == 0 && i > 0 {
				cur.WriteByte('\n')
				curLines = append(curLines, i+1)
			}

			e := strings.Index(line[pos:], "-->")
			end := len(line)

			if e >= 0 {
				end = pos + e
			}

			cur.WriteString(line[pos:end])

			for k := pos; k < end; k++ {
				curLines = append(curLines, i+1)
			}

			if e < 0 {
				break
			}

			bodies, lineOf = append(bodies, cur.String()), append(lineOf, curLines)
			cur, pos = nil, end+3
		}
	}

	return bodies, lineOf
}

func provenanceNoteRanges(s string) [][2]int {
	var out [][2]int

	for _, part := range strings.Split(strings.ReplaceAll(s, " ", ""), ",") {
		a, b, found := strings.Cut(part, "-")
		x, _ := strconv.Atoi(a)
		y := x

		if found {
			y, _ = strconv.Atoi(b)
		}

		out = append(out, [2]int{x, y})
	}

	return out
}

// provenanceNoteResolve applies the header's resolution order to one written path.
func provenanceNoteResolve(written, prev string, under []string, tree noteTree) (kind, file string, cands int) {
	if strings.HasPrefix(written, "GOROOT/") {
		return "goroot", "", 0
	}

	p := written

	for strings.HasPrefix(p, "../") {
		p = p[3:]
	}

	if tree.set[p] {
		return "rooted", p, 0
	}

	if prev != "" {
		dir := path.Dir(prev) + "/"

		if tree.set[dir+p] {
			return "context", dir + p, 0
		}

		stem, _, _ := strings.Cut(path.Base(prev), ".")

		if strings.HasPrefix(p, ".") && tree.set[dir+stem+p] {
			return "stem", dir + stem + p, 0
		}

		if strings.HasPrefix(p, "cs.target") && tree.set[dir+stem+"."+p] {
			return "stem", dir + stem + "." + p, 0
		}
	}

	for _, u := range under {
		if tree.set[u+p] {
			return "under", u + p, 0
		}
	}

	var hits []string

	for _, t := range tree.tracked {
		if strings.HasSuffix(t, "/"+p) {
			hits = append(hits, t)
		}
	}

	switch {
	case len(hits) == 1:
		return "suffix", hits[0], 0
	case len(hits) > 1:
		return "ambiguous", "", len(hits)
	case provenanceNoteRoot.MatchString(p):
		return "missing", "", 0
	}

	return "outside", "", 0
}

// provenanceNoteTokens reads every token of every note on one page.
func provenanceNoteTokens(text string, tree noteTree) []noteToken {
	var out []noteToken
	bodies, lineOf := provenanceNoteBodies(text)

	for n, body := range bodies {
		if strings.HasPrefix(strings.TrimLeft(body, " \t\n"), "source:") {
			continue
		}

		var under []string

		for _, m := range provenanceNoteUnder.FindAllStringSubmatch(body, -1) {
			under = append(under, strings.TrimRight(m[1], "./:")+"/")
		}

		prev := ""

		for _, m := range provenanceNoteToken.FindAllStringSubmatchIndex(body, -1) {
			if m[1] < len(body) && isNoteWordByte(body[m[1]]) {
				continue
			}

			t := noteToken{line: lineOf[n][min(m[0], len(lineOf[n])-1)]}

			if m[2] >= 0 {
				t.text = body[m[2]:m[1]]
				t.ranges = provenanceNoteRanges(body[m[4]:m[5]])
				t.kind, t.file, t.cands = provenanceNoteResolve(body[m[2]:m[3]], prev, under, tree)
			} else {
				if prev == "" {
					continue
				}

				t.text = strings.TrimLeft(body[m[0]:m[1]], " \t\n(;,")
				t.ranges = provenanceNoteRanges(body[m[6]:m[7]])
				t.kind, t.file = "continues", prev
			}

			switch t.kind {
			case "rooted", "context", "stem", "under", "suffix":
				prev = t.file
			}

			rest := body[m[1]:]

			if f := provenanceNoteFP.FindStringSubmatch(rest); f != nil {
				t.fp, rest = f[1], rest[len(f[0]):]
			}

			if q := provenanceNoteQuote.FindStringSubmatch(rest); q != nil {
				t.quote = q[1]
			}

			out = append(out, t)
		}
	}

	return out
}

func isNoteWordByte(b byte) bool {
	return b == '_' || b >= '0' && b <= '9' || b >= 'a' && b <= 'z' || b >= 'A' && b <= 'Z'
}

// provenanceNoteCheck returns "" when the token holds, else what is wrong.
func provenanceNoteCheck(t noteToken, tree noteTree) string {
	lines, ok := tree.read(t.file)

	if !ok {
		return "cannot be read"
	}

	var span []string

	for _, r := range t.ranges {
		if r[0] < 1 || r[1] < r[0] || r[1] > len(lines) {
			return fmt.Sprintf("cites line %d-%d of a %d-line file", r[0], r[1], len(lines))
		}

		span = append(span, lines[r[0]-1:r[1]]...)
	}

	blank := true

	for _, s := range span {
		blank = blank && strings.TrimSpace(s) == ""
	}

	if blank {
		return "every cited line is blank"
	}

	if t.fp != "" {
		if got := provenanceNoteFingerprintOf(span); got != t.fp {
			return fmt.Sprintf("the cited lines changed since the note was written (#%s, now #%s)%s; re-cite it at the lines that hold what it describes",
				t.fp, got, provenanceNoteRelocated(lines, t))
		}
	}

	if t.quote != "" {
		joined := strings.Join(strings.Fields(strings.Join(span, " ")), " ")

		for _, piece := range provenanceNoteElide.Split(t.quote, -1) {
			if p := strings.Join(strings.Fields(piece), " "); p != "" && !strings.Contains(joined, p) {
				return fmt.Sprintf("the cited lines do not hold the quoted `%s`", t.quote)
			}
		}
	}

	if t.fp == "" && t.quote == "" {
		return "neither quotes nor fingerprints what it cites, so only its existence could be checked: " + provenanceNoteHow
	}

	return ""
}

// provenanceNoteFingerprintOf is the fingerprint of cited lines: the first six hex digits of the SHA-256
// of the non-blank lines, each with its whitespace collapsed, one per line. A blank line added or dropped
// inside the stretch leaves it unchanged; any other edit, or the stretch moving, changes it.
func provenanceNoteFingerprintOf(span []string) string {
	var b strings.Builder

	for _, s := range span {
		if f := strings.Fields(s); len(f) > 0 {
			b.WriteString(strings.Join(f, " "))
			b.WriteByte('\n')
		}
	}

	sum := sha256.Sum256([]byte(b.String()))

	return hex.EncodeToString(sum[:])[:6]
}

// provenanceNoteRelocated names where a one-range token's fingerprinted lines now are, when they are in
// exactly one place in the file, and is empty otherwise.
func provenanceNoteRelocated(lines []string, t noteToken) string {
	if len(t.ranges) != 1 {
		return ""
	}

	n := t.ranges[0][1] - t.ranges[0][0] + 1
	var at []int

	for s := 0; s+n <= len(lines); s++ {
		if strings.TrimSpace(lines[s]) != "" && provenanceNoteFingerprintOf(lines[s:s+n]) == t.fp {
			at = append(at, s+1)
		}
	}

	switch {
	case len(at) != 1:
		return ""
	case n == 1:
		return fmt.Sprintf("; those lines now read at :%d", at[0])
	}

	return fmt.Sprintf("; those lines now read at :%d-%d", at[0], at[0]+n-1)
}

// provenanceNoteRangesGoOn reports whether the text after a match continues its ranges (`-12`, `,7`,
// `, 7`), so the match is a prefix of a longer token rather than the token itself.
var provenanceNoteRangesGoOnRe = regexp.MustCompile(`^(?:\d|-\d|,\s?\d)`)

func provenanceNoteRangesGoOn(after string) bool {
	return provenanceNoteRangesGoOnRe.MatchString(after)
}

// provenanceNoteAddFingerprints writes a fingerprint after every unquoted, unfingerprinted token of the
// living pages whose cited lines exist and are not all blank, and returns how many it wrote. It never
// changes a fingerprint already written.
func provenanceNoteAddFingerprints(t *testing.T, tree noteTree) int {
	t.Helper()
	root := repoRootFromPackageDir(t)
	added := 0

	for rel, text := range livingMarkdownPages(t) {
		if _, record := provenanceNoteRecordPages[rel]; record {
			continue
		}

		eol := "\n"

		if strings.Contains(text, "\r\n") {
			eol = "\r\n"
		}

		lines := strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n")
		cursor := map[int]int{}
		type insert struct{ line, col int }
		var inserts []insert
		fps := map[insert]string{}

		for _, tk := range provenanceNoteTokens(text, tree) {
			switch tk.kind {
			case "rooted", "context", "stem", "under", "suffix", "continues":
			default:
				continue
			}

			// The token's own occurrence on its line: the next one, in token order, whose ranges end there.
			l := tk.line - 1
			col := -1

			for from := cursor[l]; ; {
				k := strings.Index(lines[l][from:], tk.text)

				if k < 0 {
					break
				}

				end := from + k + len(tk.text)

				if !provenanceNoteRangesGoOn(lines[l][end:]) {
					col = end
					break
				}

				from = end
			}

			if col < 0 {
				t.Errorf("%s:%d: cannot place a fingerprint after `%s`", rel, tk.line, tk.text)
				continue
			}

			cursor[l] = col

			if tk.fp != "" || tk.quote != "" {
				continue
			}

			file, ok := tree.read(tk.file)

			if !ok {
				continue
			}

			var span []string
			valid := true

			for _, r := range tk.ranges {
				if r[0] < 1 || r[1] < r[0] || r[1] > len(file) {
					valid = false
					break
				}

				span = append(span, file[r[0]-1:r[1]]...)
			}

			if !valid || provenanceNoteFingerprintOf(span) == provenanceNoteFingerprintOf(nil) {
				continue // the check names it
			}

			at := insert{l, col}
			inserts = append(inserts, at)
			fps[at] = provenanceNoteFingerprintOf(span)
		}

		if len(inserts) == 0 {
			continue
		}

		// Right to left, so each column is still where it was read.
		sort.Slice(inserts, func(i, j int) bool {
			if inserts[i].line != inserts[j].line {
				return inserts[i].line > inserts[j].line
			}

			return inserts[i].col > inserts[j].col
		})

		for _, at := range inserts {
			lines[at.line] = lines[at.line][:at.col] + "#" + fps[at] + lines[at.line][at.col:]
		}

		if err := os.WriteFile(filepath.Join(root, filepath.FromSlash(rel)), []byte(strings.Join(lines, eol)), 0o644); err != nil {
			t.Fatalf("writing %s: %v", rel, err)
		}

		added += len(inserts)
	}

	return added
}

type noteProblem struct {
	line int
	key  string
	what string
}

// provenanceNoteProblems is the rule over one page: the tokens it checked, and those that fail.
func provenanceNoteProblems(page, text string, tree noteTree) (checked int, problems []noteProblem) {
	for _, t := range provenanceNoteTokens(text, tree) {
		what := ""

		switch t.kind {
		case "goroot", "outside":
			continue
		case "ambiguous":
			what = fmt.Sprintf("`%s` names %d tracked files; write enough of the path to name one", t.text, t.cands)
		case "missing":
			what = fmt.Sprintf("`%s` is not a tracked file", t.text)
		default:
			checked++

			if w := provenanceNoteCheck(t, tree); w != "" {
				what = fmt.Sprintf("`%s` (%s): %s", t.text, t.file, w)
			}
		}

		if what != "" {
			key := page + ": " + t.text

			if t.file != "" {
				key = page + ": " + t.file + ":" + t.text[strings.LastIndex(t.text, ":")+1:]
			}

			problems = append(problems, noteProblem{line: t.line, key: key, what: what})
		}
	}

	return checked, problems
}

// noteFakeTree is a tree of named files for the shape cases.
func noteFakeTree(files map[string]string) noteTree {
	var tracked []string

	for k := range files {
		tracked = append(tracked, k)
	}

	sort.Strings(tracked)

	return newNoteTree(tracked, func(rel string) ([]string, bool) {
		s, ok := files[rel]
		return strings.Split(s, "\n"), ok
	})
}

// TestProvenanceNoteRuleReadsEachShape pins the rule, one shape per case.
func TestProvenanceNoteRuleReadsEachShape(t *testing.T) {
	tree := noteFakeTree(map[string]string{
		"src/tests/Behavioral/A/main.go":        "package main\n\nfunc main() {\n\tx := 1\n}",
		"src/tests/Behavioral/A/main.cs.target": "namespace go;\n\npartial class main_package {\n\ninternal static void Main() {\n    nint x = 1;\n}\n}",
		"src/tests/Behavioral/B/main.go":        "package main\n\nfunc main() {}",
		"src/tests/Behavioral/C/C.go":           "package main\n\nfunc main() {}",
		"src/tests/Behavioral/C/C.cs.target":    "namespace go;\n\nx",
		"src/go2cs/main.go":                     "package main\n\n// one\n// two",
	})

	// fp is a token's fingerprint over the given cited lines.
	fp := func(lines ...string) string { return "#" + provenanceNoteFingerprintOf(lines) }

	cases := []struct {
		name    string
		page    string
		checked int
		want    []string
	}{
		{"a rooted token with a quote that holds", "<!-- src/tests/Behavioral/A/main.cs.target:6 (`nint x = 1;`) -->", 1, nil},
		{"a rooted token whose quote has moved", "<!-- src/tests/Behavioral/A/main.cs.target:5 `nint x = 1;` -->", 1, []string{"do not hold"}},
		{"a quote shortened with an ellipsis", "<!-- src/tests/Behavioral/A/main.cs.target:5-6 `internal static … nint x` -->", 1, nil},
		{"a line-only token on a blank line", "<!-- src/tests/Behavioral/A/main.cs.target:2 -->", 1, []string{"blank"}},
		{"a token past the end of its file", "<!-- src/tests/Behavioral/A/main.go:9 -->", 1, []string{"5-line file"}},
		{"a comma list and a continuation", "<!-- src/go2cs/main.go:1,3" + fp("package main", "// one") + " and :4" + fp("// two") + " -->", 2, nil},
		{"a continuation past the end", "<!-- src/go2cs/main.go:1" + fp("package main") + "; :7 -->", 2, []string{"4-line file"}},
		{"the previous token's directory", "<!-- src/tests/Behavioral/A/main.go:4" + fp("\tx := 1") + " -> main.cs.target:6 `nint x = 1;` -->", 2, nil},
		{"the previous token's stem", "<!-- src/tests/Behavioral/C/C.go:3" + fp("func main() {}") + " -> .cs.target:3 `x` -->", 2, nil},
		{"a declared directory", "<!-- in src/tests/Behavioral/A: main.cs.target:6" + fp("    nint x = 1;") + " -->", 1, nil},
		{"a unique suffix", "<!-- C/C.cs.target:3" + fp("x") + " -->", 1, nil},
		{"an unquoted token with no fingerprint", "<!-- C/C.cs.target:3 -->", 1, []string{"neither quotes nor fingerprints"}},
		{"a fingerprint ignores whitespace", "<!-- C/C.cs.target:3" + fp("  x  ") + " -->", 1, nil},
		{"a fingerprint whose lines moved", "<!-- src/tests/Behavioral/A/main.cs.target:5" + fp("    nint x = 1;") + " -->", 1, []string{"changed since the note was written"}},
		{"a fingerprint and a quote", "<!-- src/tests/Behavioral/A/main.cs.target:6" + fp("nint x = 1;") + " (`nint x = 1;`) -->", 1, nil},
		{"a fingerprint that holds but a quote that does not", "<!-- src/tests/Behavioral/A/main.cs.target:6" + fp("nint x = 1;") + " `int y` -->", 1, []string{"do not hold"}},
		{"an ambiguous name", "<!-- main.go:1 -->", 0, []string{"names 3 tracked files"}},
		{"a moved rooted path", "<!-- src/tests/Behavioral/Gone/main.go:1 -->", 0, []string{"not a tracked file"}},
		{"a docs-relative path", "<!-- ../src/go2cs/main.go:3" + fp("// one") + " -->", 1, nil},
		{"a toolchain path is not read", "<!-- GOROOT/src/strconv/ftoa.go:9999 -->", 0, nil},
		{"a file outside the repository is not read", "<!-- lane R's setup-convert.sh:26-28 -->", 0, nil},
		{"a sample citation is not a note", "<!-- source: src/tests/Behavioral/A/main.go:99 -->", 0, nil},
		{"fenced code is not read", "```\n<!-- src/tests/Behavioral/A/main.go:99 -->\n```", 0, nil},
		{"a note across lines", "text <!-- first line\n  src/tests/Behavioral/A/main.go:99 -->", 1, []string{"5-line file"}},
	}

	for _, c := range cases {
		checked, problems := provenanceNoteProblems("p.md", c.page, tree)

		if checked != c.checked || len(problems) != len(c.want) {
			t.Errorf("%s: checked %d with %d problem(s) %v, want %d and %d", c.name, checked, len(problems), problems, c.checked, len(c.want))
			continue
		}

		for i, w := range c.want {
			if !strings.Contains(problems[i].what, w) {
				t.Errorf("%s: problem %q, want it to say %q", c.name, problems[i].what, w)
			}
		}
	}

	if _, p := provenanceNoteProblems("p.md", "x\n\n<!-- first\nsecond src/tests/Behavioral/A/main.go:99 -->", tree); len(p) != 1 || p[0].line != 4 {
		t.Errorf("a problem on a note's second line: got %v, want one at line 4", p)
	}

	// A moved fingerprint names where its lines are now, when they are in one place.
	if _, p := provenanceNoteProblems("p.md", "<!-- src/tests/Behavioral/A/main.cs.target:5"+fp("    nint x = 1;")+" -->", tree); len(p) != 1 || !strings.Contains(p[0].what, "now read at :6") {
		t.Errorf("a moved fingerprint: got %v, want it relocated to :6", p)
	}
}

// provenanceNoteRealTree is the repository, read from disk.
func provenanceNoteRealTree(t *testing.T) noteTree {
	t.Helper()
	root := repoRootFromPackageDir(t)

	return newNoteTree(gitTrackedFiles(t, root), func(rel string) ([]string, bool) {
		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			return nil, false
		}

		return strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n"), true
	})
}

// TestDocsProvenanceNotesResolve is the guard over the living pages.
func TestDocsProvenanceNotesResolve(t *testing.T) {
	tree := provenanceNoteRealTree(t)

	if *noteFingerprints {
		t.Logf("-note-fingerprints: added %d fingerprint(s) to the living pages", provenanceNoteAddFingerprints(t, tree))
	}

	var problems []string
	excused := map[string]bool{}
	checked := 0

	for rel, text := range livingMarkdownPages(t) {
		if _, record := provenanceNoteRecordPages[rel]; record {
			continue
		}

		n, found := provenanceNoteProblems(rel, text, tree)
		checked += n

		for _, p := range found {
			if _, stale := provenanceNoteKnownStale[p.key]; stale {
				excused[p.key] = true
				continue
			}

			problems = append(problems, fmt.Sprintf("%s:%d: %s", rel, p.line, p.what))
		}
	}

	if checked < 300 {
		t.Fatalf("VACUOUS: only %d note token(s) checked; the scanner cannot have read the pages' notes", checked)
	}

	t.Logf("%d note token(s) checked, %d known-stale row(s) excused", checked, len(excused))

	for key, why := range provenanceNoteKnownStale {
		if !excused[key] {
			problems = append(problems, fmt.Sprintf("provenanceNoteKnownStale row %q no longer fails (%s): remove the row", key, why))
		}
	}

	sort.Strings(problems)

	if len(problems) > 0 {
		t.Fatalf("%d provenance note(s) no longer point at what they describe (of %d checked); re-cite each at the line that holds it, and quote what it cites:\n  %s",
			len(problems), checked, strings.Join(problems, "\n  "))
	}
}

// TestProvenanceNoteGuardFiresOnAPlantedStaleNote moves a real note to the wrong line and wants it named.
func TestProvenanceNoteGuardFiresOnAPlantedStaleNote(t *testing.T) {
	tree := provenanceNoteRealTree(t)
	rel := "docs/ConversionStrategies.md"
	text := livingMarkdownPages(t)[rel]
	real := "UnsafeOperations.cs.target:87 (`var t2 = ~Ꮡt1.Reinterpret<T1, T2>();`)"

	if strings.Count(text, real) != 1 {
		t.Fatalf("the control's note is not on %s once; re-point the control at another quoted note", rel)
	}

	line := strings.Count(text[:strings.Index(text, real)], "\n") + 1
	stale := provenanceNoteStaleOn(rel)

	if _, p := provenanceNoteProblems(rel, text, tree); len(p) != stale {
		t.Fatalf("the unplanted page has %d problem(s), want only its %d known-stale row(s): %v", len(p), stale, p)
	}

	planted := strings.Replace(text, real, strings.Replace(real, ":87 ", ":88 ", 1), 1)
	_, p := provenanceNoteProblems(rel, planted, tree)
	named := false

	for _, x := range p {
		named = named || x.line == line && strings.Contains(x.what, "do not hold")
	}

	if !named || len(p) != stale+1 {
		t.Errorf("the note moved to line 88 was not named at %s:%d: %v", rel, line, p)
	}
}

// TestProvenanceNoteGuardFiresOnAMovedFingerprint is the same control for an unquoted note: a real
// fingerprinted token moved two lines, the shape that passed the existence check before fingerprints.
func TestProvenanceNoteGuardFiresOnAMovedFingerprint(t *testing.T) {
	tree := provenanceNoteRealTree(t)
	rel := "docs/ConversionStrategies.md"
	text := livingMarkdownPages(t)[rel]
	real := "src/core/bufio/bufio.cs:17#3ddbb0"

	if strings.Count(text, real) != 1 {
		t.Fatalf("the control's token is not on %s once; re-point the control at another fingerprinted token", rel)
	}

	line := strings.Count(text[:strings.Index(text, real)], "\n") + 1
	planted := strings.Replace(text, real, "src/core/bufio/bufio.cs:19#3ddbb0", 1)
	_, p := provenanceNoteProblems(rel, planted, tree)
	named := false

	for _, x := range p {
		named = named || x.line == line && strings.Contains(x.what, "changed since the note was written") && strings.Contains(x.what, "now read at :17")
	}

	if !named || len(p) != provenanceNoteStaleOn(rel)+1 {
		t.Errorf("the token moved to line 19 was not named, and relocated to :17, at %s:%d: %v", rel, line, p)
	}

	// And with its fingerprint removed, the token is named for carrying neither a quote nor a fingerprint.
	_, p = provenanceNoteProblems(rel, strings.Replace(text, real, "src/core/bufio/bufio.cs:17", 1), tree)
	named = false

	for _, x := range p {
		named = named || x.line == line && strings.Contains(x.what, "neither quotes nor fingerprints")
	}

	if !named {
		t.Errorf("the token with its fingerprint removed was not named at %s:%d: %v", rel, line, p)
	}
}

// provenanceNoteStaleOn counts the known-stale rows on one page.
func provenanceNoteStaleOn(rel string) int {
	n := 0

	for key := range provenanceNoteKnownStale {
		if strings.HasPrefix(key, rel+": ") {
			n++
		}
	}

	return n
}
