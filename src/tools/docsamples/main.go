// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// docsamples regenerates the code samples of the docs pages from the files they were read from.
//
// A sample is a fenced block that follows a source comment:
//
//	<!-- source: src/tests/Behavioral/MapCommaOk/main.cs.target:1-29 -->
//
// The comment names one tracked file and one or more line ranges ("7", "7-12", "7-12, 20-24",
// "7 and :12-14"), with an optional note in parentheses. The tool reads that file at two refs:
// -base, the ref the sample was written against, and -target, the ref to regenerate from. It never
// takes a sample's text from anywhere but the target file.
//
// HOW A SAMPLE IS TIED TO ITS LINES. The pages use the comment in two ways, and the tool reads both.
//
//   - EXPLICIT. The cited ranges are the sample: one run of lines per range, and a line holding only
//     an ellipsis between two runs. The sample is rewritten from the ranges, so a sample that has
//     drifted from its file is repaired.
//   - ENVELOPE. The cited range is the stretch the sample was abridged from, and each run of sample
//     lines between ellipsis lines is a contiguous run of file lines inside it. The tool finds each
//     run in the base file, where it must match exactly, and that fixes which lines the sample shows.
//
// Either way the tool then carries each run's lines from the base file to the target file through a
// line map built from the difference between the two, and writes the target's lines in their place,
// with the indentation the sample used. Ellipsis lines and the blank lines around them stay as they
// are. The comment's line numbers are rewritten to the target's.
//
// WHAT IS REFUSED, BY NAME (page, line, reason), and left untouched: a comment in a free form the
// grammar does not read; a file missing at either ref; a range past the end of the file; an envelope
// sample one of whose runs is not in the cited range of the base file (hand-edited, or cited wrongly);
// a run whose indentation is not uniform. A refusal for a sample whose file is the same at both refs
// is a note: that sample did not need regenerating. A refusal for a sample whose file CHANGED is an
// error, and the tool exits 1: a stale sample it could not regenerate.
//
// A run whose first or last line falls inside a stretch that changed size between the refs takes the
// whole changed stretch, and the sample is listed under REVIEW.
//
// usage, from the repository root:
//
//	docsamples [-base REF] [-target REF] [-write] [-recite] [-v] [page ...]
//
// -recite corrects a comment whose lines do not hold its sample: when every run of the sample is in
// the base file as written, in order, but not inside the cited stretch, the sample is taken where it
// is and the comment is rewritten to the stretch it spans.
//
// REF is a git ref, or WORKTREE for the files on disk. The default for both is HEAD, which rewrites
// nothing unless a sample has drifted. With no page arguments it reads every tracked page under docs/
// outside the records (docs/phase3, docs/phase4, docs/doctrine and the release snapshots under
// docs/validation). Pages are always read from, and with -write written to, the working tree.
package main

import (
	"bytes"
	"flag"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"
)

// fileReader returns a file's lines at one ref, or an error when the ref does not hold the file.
type fileReader func(path string) ([]string, error)

var (
	sourceComment = regexp.MustCompile(`^(\s*)<!--\s*source:\s*(.*?)\s*-->\s*$`)
	strictSource  = regexp.MustCompile(`^([A-Za-z0-9_./+\-]+\.[A-Za-z0-9.]+):(\d+(?:-\d+)?(?:\s*(?:,|and)\s*:?\d+(?:-\d+)?)*)((?:\s+at\s+\S+(?:\s+\S+)?)?(?:\s*\(.*\))?)$`)
	rangeToken    = regexp.MustCompile(`(\d+)(?:-(\d+))?`)
	fenceOpen     = regexp.MustCompile("^(\\s*)(`{3,}|~{3,})")
	recordPage    = regexp.MustCompile(`^docs/(phase3|phase4|doctrine)/|^docs/validation/\d+(\.\d+)+/`)
)

type span struct{ lo, hi int } // 1-based, inclusive

// sample is one sourced block of a page.
type sample struct {
	commentLine int // 0-based index of the source comment
	open, close int // 0-based indices of the fence lines
	indent      string
	spec        string
	path        string
	ranges      []span
	rangeText   string
	tail        string
}

type outcome struct {
	page    string
	line    int
	class   string // same, rewritten, refused
	reason  string
	changed bool // the source file differs between base and target
	recited bool // the comment's lines were corrected to where the sample is
	drifted bool // the sample differed from its file at the base ref by a line or two
	review  bool
}

func isEllipsis(s string) bool {
	t := strings.TrimSpace(s)
	return t == "…" || t == "..."
}

func leading(s string) string { return s[:len(s)-len(strings.TrimLeft(s, " \t"))] }

// findSamples returns the sourced blocks of a page: a source comment, then, after any blank lines or
// other comments, a fence.
func findSamples(lines []string) (samples []sample, loose []int) {
	for i := 0; i < len(lines); i++ {
		m := sourceComment.FindStringSubmatch(lines[i])

		if m == nil {
			continue
		}

		j := i + 1

		for j < len(lines) && (strings.TrimSpace(lines[j]) == "" || strings.HasPrefix(strings.TrimSpace(lines[j]), "<!--")) {
			if sourceComment.MatchString(lines[j]) {
				break
			}

			j++
		}

		var f []string

		if j < len(lines) {
			f = fenceOpen.FindStringSubmatch(lines[j])
		}

		if f == nil {
			loose = append(loose, i)
			continue
		}

		k := j + 1

		for k < len(lines) {
			if c := fenceOpen.FindStringSubmatch(lines[k]); c != nil && c[2][0] == f[2][0] && len(c[2]) >= len(f[2]) && strings.TrimSpace(lines[k]) == c[2] {
				break
			}

			k++
		}

		if k >= len(lines) {
			loose = append(loose, i)
			continue
		}

		samples = append(samples, sample{commentLine: i, open: j, close: k, indent: f[1], spec: m[2]})
		i = k
	}

	return samples, loose
}

// parseSpec reads the strict form of a source comment into the sample.
func (s *sample) parseSpec() bool {
	m := strictSource.FindStringSubmatch(s.spec)

	if m == nil {
		return false
	}

	s.path, s.rangeText, s.tail = m[1], m[2], m[3]

	for _, r := range rangeToken.FindAllStringSubmatch(m[2], -1) {
		lo, _ := strconv.Atoi(r[1])
		hi := lo

		if r[2] != "" {
			hi, _ = strconv.Atoi(r[2])
		}

		if lo < 1 || hi < lo {
			return false
		}

		s.ranges = append(s.ranges, span{lo, hi})
	}

	return len(s.ranges) > 0
}

// runs splits a sample's body into runs, each the body indices of its lines, with the blank lines at
// either end left out. At level 0 a run ends at an ellipsis line; at level 1 at a blank line too, for
// a sample that skips lines across a blank line; at level 2 every line is its own run, for a sample
// that leaves the file's blank lines out.
func runs(body []string, level int) [][2]int {
	var out [][2]int
	start := 0

	flush := func(end int) {
		lo, hi := start, end

		for lo < hi && strings.TrimSpace(body[lo]) == "" {
			lo++
		}

		for hi > lo && strings.TrimSpace(body[hi-1]) == "" {
			hi--
		}

		if lo < hi {
			out = append(out, [2]int{lo, hi})
		}
	}

	for i, b := range body {
		switch {
		case isEllipsis(b) || (level >= 1 && strings.TrimSpace(b) == ""):
			flush(i)
			start = i + 1
		case level >= 2:
			flush(i)
			start = i
		}
	}

	flush(len(body))

	return out
}

// reindent states how a run's lines differ from the file's in leading whitespace: cut characters
// removed from the file line, then pad put in front.
type reindent struct {
	cut int
	pad string
}

func (r reindent) apply(line string) string {
	lead := leading(line)

	// A line of whitespace alone loses what the rule cuts and gains no pad.
	if len(lead) == len(line) {
		if r.cut >= len(line) {
			return ""
		}

		return line[r.cut:]
	}

	if r.cut > len(lead) {
		return r.pad + line[len(lead):]
	}

	return r.pad + line[r.cut:]
}

// reindentOf derives the rule from the first non-blank pair of a run and its file lines.
func reindentOf(sampleLines, fileLines []string) (reindent, bool) {
	for i := range sampleLines {
		if i >= len(fileLines) || strings.TrimSpace(sampleLines[i]) == "" || strings.TrimSpace(fileLines[i]) == "" {
			continue
		}

		sl, fl := leading(sampleLines[i]), leading(fileLines[i])

		switch {
		case strings.HasSuffix(fl, sl):
			return reindent{cut: len(fl) - len(sl)}, true
		case strings.HasSuffix(sl, fl):
			return reindent{pad: sl[:len(sl)-len(fl)]}, true
		}

		return reindent{}, false
	}

	return reindent{}, true
}

// differing counts the lines of a that are not b's, trailing whitespace aside. The two are of one
// length.
func differing(a, b []string) int {
	n := 0

	for i := range a {
		if strings.TrimRight(a[i], " \t") != strings.TrimRight(b[i], " \t") {
			n++
		}
	}

	return n
}

func sameLines(a, b []string) bool {
	if len(a) != len(b) {
		return false
	}

	for i := range a {
		if strings.TrimRight(a[i], " \t") != strings.TrimRight(b[i], " \t") {
			return false
		}
	}

	return true
}

func render(r reindent, fileLines []string) []string {
	out := make([]string, len(fileLines))

	for i, l := range fileLines {
		out[i] = r.apply(l)
	}

	return out
}

// lineMap carries a base line span to the target file.
type lineMap struct {
	same  bool
	hunks []hunk
}

// hunk is one stretch that differs: base lines [aLo,aHi) became target lines [bLo,bHi), 0-based.
type hunk struct{ aLo, aHi, bLo, bHi int }

// diffLines finds the stretches that differ between two files: common head and tail first, then lines
// unique to both sides as anchors (the longest run of them in the same order), and whatever lies
// between two anchors is one hunk.
func diffLines(a, b []string) lineMap {
	if len(a) == len(b) {
		same := true

		for i := range a {
			if a[i] != b[i] {
				same = false
				break
			}
		}

		if same {
			return lineMap{same: true}
		}
	}

	var hunks []hunk
	var walk func(aLo, aHi, bLo, bHi int)

	walk = func(aLo, aHi, bLo, bHi int) {
		for aLo < aHi && bLo < bHi && a[aLo] == b[bLo] {
			aLo++
			bLo++
		}

		for aHi > aLo && bHi > bLo && a[aHi-1] == b[bHi-1] {
			aHi--
			bHi--
		}

		if aLo == aHi && bLo == bHi {
			return
		}

		countA, countB, posB := map[string]int{}, map[string]int{}, map[string]int{}

		for i := aLo; i < aHi; i++ {
			countA[a[i]]++
		}

		for j := bLo; j < bHi; j++ {
			countB[b[j]]++
			posB[b[j]] = j
		}

		// Anchors in base order, each with its target position; keep the longest increasing run.
		type anchor struct{ i, j int }
		var anchors []anchor

		for i := aLo; i < aHi; i++ {
			if countA[a[i]] == 1 && countB[a[i]] == 1 {
				anchors = append(anchors, anchor{i, posB[a[i]]})
			}
		}

		best := make([]int, len(anchors))
		prev := make([]int, len(anchors))
		end := -1

		for x := range anchors {
			best[x], prev[x] = 1, -1

			for y := 0; y < x; y++ {
				if anchors[y].j < anchors[x].j && best[y]+1 > best[x] {
					best[x], prev[x] = best[y]+1, y
				}
			}

			if end < 0 || best[x] > best[end] {
				end = x
			}
		}

		if end < 0 {
			hunks = append(hunks, pairSimilar(a, b, aLo, aHi, bLo, bHi)...)
			return
		}

		var chain []anchor

		for x := end; x >= 0; x = prev[x] {
			chain = append(chain, anchors[x])
		}

		ca, cb := aLo, bLo

		for x := len(chain) - 1; x >= 0; x-- {
			walk(ca, chain[x].i, cb, chain[x].j)
			ca, cb = chain[x].i+1, chain[x].j+1
		}

		walk(ca, aHi, cb, bHi)
	}

	walk(0, len(a), 0, len(b))
	sort.Slice(hunks, func(x, y int) bool { return hunks[x].aLo < hunks[y].aLo })

	return lineMap{hunks: hunks}
}

var wordToken = regexp.MustCompile(`[A-Za-z0-9_@]+`)

// similar reports that two differing lines are one line edited: they share at least half the words
// of the longer one. "[GoRecv] public static nint Read(" and "public static nint Read(" are; a tag's
// attribute line and the field under it are not.
func similar(x, y string) bool {
	wx, wy := wordToken.FindAllString(x, -1), wordToken.FindAllString(y, -1)

	if len(wx) == 0 || len(wy) == 0 {
		return strings.TrimSpace(x) == strings.TrimSpace(y)
	}

	set := map[string]bool{}

	for _, w := range wx {
		set[w] = true
	}

	shared, seen := 0, map[string]bool{}

	for _, w := range wy {
		if set[w] && !seen[w] {
			shared++
		}

		seen[w] = true
	}

	return shared*2 >= max(len(set), len(seen))
}

// pairSimilar splits one differing stretch into smaller hunks: lines that are one line edited are
// paired, in order, each pair a hunk of one line for one; what lies between two pairs is a hunk of
// its own. A stretch too large to pair is returned whole.
func pairSimilar(a, b []string, aLo, aHi, bLo, bHi int) []hunk {
	n, m := aHi-aLo, bHi-bLo

	if n == 0 || m == 0 || n*m > 250000 {
		return []hunk{{aLo, aHi, bLo, bHi}}
	}

	// best[i][j]: the most pairs among a[aLo+i:], b[bLo+j:].
	best := make([][]int, n+1)

	for i := range best {
		best[i] = make([]int, m+1)
	}

	for i := n - 1; i >= 0; i-- {
		for j := m - 1; j >= 0; j-- {
			best[i][j] = max(best[i+1][j], best[i][j+1])

			if similar(a[aLo+i], b[bLo+j]) {
				best[i][j] = max(best[i][j], best[i+1][j+1]+1)
			}
		}
	}

	var out []hunk
	i, j, gi, gj := 0, 0, 0, 0

	flush := func() {
		if i > gi || j > gj {
			out = append(out, hunk{aLo + gi, aLo + i, bLo + gj, bLo + j})
		}
	}

	for i < n && j < m {
		switch {
		case similar(a[aLo+i], b[bLo+j]) && best[i][j] == best[i+1][j+1]+1:
			flush()
			out = append(out, hunk{aLo + i, aLo + i + 1, bLo + j, bLo + j + 1})
			i, j = i+1, j+1
			gi, gj = i, j
		case best[i+1][j] >= best[i][j+1]:
			i++
		default:
			j++
		}
	}

	i, j = n, m
	flush()

	return out
}

// carry maps the 1-based inclusive base span to the target. inside reports that an end of the span
// fell in a hunk whose two sides differ in length, so the whole hunk was taken at that end.
func (m lineMap) carry(s span) (out span, inside bool) {
	if m.same {
		return s, false
	}

	lo, hi := s.lo-1, s.hi // 0-based half-open
	newLo, newHi := -1, -1
	shift := 0

	for _, h := range m.hunks {
		if newLo < 0 && lo < h.aLo {
			newLo = lo + shift
		}

		if newHi < 0 && hi <= h.aLo {
			newHi = hi + shift
		}

		even := h.aHi-h.aLo == h.bHi-h.bLo

		if newLo < 0 && lo < h.aHi {
			if even {
				newLo = h.bLo + (lo - h.aLo)
			} else {
				newLo = h.bLo
				inside = inside || lo > h.aLo
			}
		}

		if newHi < 0 && hi <= h.aHi {
			if even {
				newHi = h.bLo + (hi - h.aLo)
			} else {
				newHi = h.bHi
				inside = inside || hi < h.aHi
			}
		}

		shift = h.bHi - h.aHi
	}

	if newLo < 0 {
		newLo = lo + shift
	}

	if newHi < 0 {
		newHi = hi + shift
	}

	return span{newLo + 1, newHi}, inside
}

func (m lineMap) touches(s span) bool {
	for _, h := range m.hunks {
		if h.aLo < s.hi && s.lo-1 < h.aHi || (h.aLo == h.aHi && h.aLo >= s.lo-1 && h.aLo < s.hi) {
			return true
		}
	}

	return false
}

func formatRanges(text string, ranges []span) string {
	i := 0

	return rangeToken.ReplaceAllStringFunc(text, func(string) string {
		r := ranges[i]
		i++

		if r.lo == r.hi {
			return strconv.Itoa(r.lo)
		}

		return fmt.Sprintf("%d-%d", r.lo, r.hi)
	})
}

// regenerate rewrites the sourced samples of one page. It returns the new lines and one outcome per
// source comment.
func regenerate(page string, lines []string, base, target fileReader, recite bool) ([]string, []outcome) {
	samples, loose := findSamples(lines)
	var outcomes []outcome

	for _, i := range loose {
		outcomes = append(outcomes, outcome{page: page, line: i + 1, class: "refused", reason: "no fenced block follows the source comment"})
	}

	type edit struct {
		from, to int // replace lines[from:to]
		with     []string
	}

	var edits []edit

	for _, s := range samples {
		o := outcome{page: page, line: s.commentLine + 1}
		refuse := func(format string, args ...any) {
			o.class, o.reason = "refused", fmt.Sprintf(format, args...)
			outcomes = append(outcomes, o)
		}

		if !s.parseSpec() {
			refuse("the source comment is not in the form FILE:LINES the tool reads: %q", s.spec)
			continue
		}

		baseFile, err := base(s.path)

		if err != nil {
			refuse("%s is missing at the base ref", s.path)
			continue
		}

		targetFile, err := target(s.path)

		if err != nil {
			o.changed = true
			refuse("%s is missing at the target ref", s.path)
			continue
		}

		lmap := diffLines(baseFile, targetFile)
		o.changed = !lmap.same
		past := false

		for _, r := range s.ranges {
			past = past || r.hi > len(baseFile)
		}

		if past {
			refuse("%s has %d lines at the base ref, fewer than the comment cites (%s)", s.path, len(baseFile), s.rangeText)
			continue
		}

		body := lines[s.open+1 : s.close]
		// Each run's lines in the base file. The envelope reading is tried first: every run found, as
		// written, in order, inside the cited stretch.
		var rs [][2]int
		var at []span
		lo, hi := s.ranges[0].lo, s.ranges[0].hi

		for _, r := range s.ranges {
			lo, hi = min(lo, r.lo), max(hi, r.hi)
		}

		// drifted is how many lines of a run may differ from the file's and the run still be that
		// stretch of the file: none, unless the sample is being read as drifted, then one line in
		// eight of a run of three lines or more.
		drifted := false

		align := func(lo, hi int) int {
			pos := lo

			for x, r := range rs {
				n, found := r[1]-r[0], -1
				allow := 0

				if drifted && n >= 3 {
					allow = (n + 7) / 8
				}

				for start := pos; start+n-1 <= hi; start++ {
					fileLines := baseFile[start-1 : start-1+n]
					rule, ok := reindentOf(body[r[0]:r[1]], fileLines)

					if ok && differing(render(rule, fileLines), body[r[0]:r[1]]) <= allow {
						found = start
						break
					}
				}

				if found < 0 {
					return x
				}

				at[x] = span{found, found + n - 1}
				pos = found + n
			}

			return -1
		}

		// The coarsest runs that fit are taken: whole runs between ellipsis lines first.
		alignAny := func(lo, hi int) bool {
			for level := 0; level <= 2; level++ {
				rs = runs(body, level)
				at = make([]span, len(rs))

				if len(rs) > 0 && align(lo, hi) < 0 {
					return true
				}
			}

			return false
		}

		failed := -1

		if !alignAny(lo, hi) {
			// A sample that fits its cited stretch but for a line or two has drifted from its file,
			// and is rewritten from it.
			drifted = true

			for level := 0; level <= 1 && !o.drifted; level++ {
				rs = runs(body, level)
				at = make([]span, len(rs))
				o.drifted = len(rs) > 0 && align(lo, hi) < 0
			}

			drifted = false
		}

		if !o.drifted && !alignAny(lo, hi) {
			// With -recite, a sample that is in the file as written but not on the cited lines is
			// taken where it is, and its comment is rewritten to the stretch it spans.
			if recite && alignAny(1, len(baseFile)) {
				s.ranges = []span{{at[0].lo, at[len(rs)-1].hi}}
				s.rangeText = "0-0"
				o.recited = true
			} else {
				rs = runs(body, 0)
				at = make([]span, len(rs))
				failed = max(align(lo, hi), 0)
			}
		}

		if failed >= 0 && len(rs) == 0 {
			refuse("the sample is empty")
			continue
		}

		if failed >= 0 {
			// The explicit reading repairs a drifted sample: one run per cited range, of the range's
			// length, and at least half its lines still the file's. Fewer than half is another excerpt
			// that happens to have the same shape, and is refused.
			explicit := len(rs) == len(s.ranges)
			alike, counted := 0, 0

			for x := 0; explicit && x < len(rs); x++ {
				explicit = rs[x][1]-rs[x][0] == s.ranges[x].hi-s.ranges[x].lo+1

				for y := 0; explicit && y < rs[x][1]-rs[x][0]; y++ {
					sampleLine := strings.TrimSpace(body[rs[x][0]+y])

					if sampleLine == "" {
						continue
					}

					counted++

					if sampleLine == strings.TrimSpace(baseFile[s.ranges[x].lo-1+y]) {
						alike++
					}
				}

				at[x] = s.ranges[x]
			}

			if !explicit || counted == 0 || alike*2 < counted {
				refuse("the sample's line %q (and the %d line(s) of its run) is not in %s:%d-%d at the base ref as written: hand-edited, or cited by the wrong lines", strings.TrimSpace(body[rs[failed][0]]), rs[failed][1]-rs[failed][0], s.path, lo, hi)
				continue
			}
		}

		// Rewrite each run from the target file, last run first so body indices hold.
		newBody := append([]string(nil), body...)
		ok := true

		for x := len(rs) - 1; x >= 0 && ok; x-- {
			r := rs[x]
			rule, uniform := reindentOf(body[r[0]:r[1]], baseFile[at[x].lo-1:at[x].hi])

			if !uniform {
				rule = reindent{pad: s.indent}
			}

			to, inside := lmap.carry(at[x])
			o.review = o.review || inside

			if to.lo < 1 || to.hi > len(targetFile) {
				refuse("the lines of the run at %s:%d-%d are past the end of the file at the target ref", s.path, at[x].lo, at[x].hi)
				ok = false
				break
			}

			// A run whose lines are all gone at the target leaves the sample; it is listed for review
			// unless it was one line of a longer stretch, where its neighbours show what replaced it.
			if to.hi < to.lo {
				o.review = o.review || r[1]-r[0] > 1
				newBody = append(newBody[:r[0]], newBody[r[1]:]...)
				continue
			}

			replacement := render(rule, targetFile[to.lo-1:to.hi])
			newBody = append(newBody[:r[0]], append(replacement, newBody[r[1]:]...)...)
		}

		if !ok {
			continue
		}

		newComment := lines[s.commentLine]

		if !lmap.same || o.recited {
			carried := make([]span, len(s.ranges))

			for x, r := range s.ranges {
				carried[x], _ = lmap.carry(r)
			}

			m := sourceComment.FindStringSubmatch(lines[s.commentLine])
			newComment = m[1] + "<!-- source: " + s.path + ":" + formatRanges(s.rangeText, carried) + s.tail + " -->"
		}

		if sameLines(newBody, body) && len(newBody) == len(body) && newComment == lines[s.commentLine] {
			exact := true

			for x := range body {
				exact = exact && body[x] == newBody[x]
			}

			if exact {
				o.class = "same"
				outcomes = append(outcomes, o)
				continue
			}
		}

		o.class = "rewritten"
		outcomes = append(outcomes, o)
		edits = append(edits, edit{s.open + 1, s.close, newBody}, edit{s.commentLine, s.commentLine + 1, []string{newComment}})
	}

	sort.Slice(edits, func(x, y int) bool { return edits[x].from > edits[y].from })
	out := append([]string(nil), lines...)

	for _, e := range edits {
		out = append(out[:e.from], append(append([]string(nil), e.with...), out[e.to:]...)...)
	}

	return out, outcomes
}

func gitReader(ref string) fileReader {
	cache := map[string][]string{}
	missing := map[string]bool{}

	return func(path string) ([]string, error) {
		if missing[path] {
			return nil, os.ErrNotExist
		}

		if l, ok := cache[path]; ok {
			return l, nil
		}

		var data []byte
		var err error

		if ref == "WORKTREE" {
			data, err = os.ReadFile(filepath.FromSlash(path))
		} else {
			data, err = exec.Command("git", "show", ref+":"+path).Output()
		}

		if err != nil {
			missing[path] = true
			return nil, os.ErrNotExist
		}

		cache[path] = splitLines(string(bytes.TrimPrefix(data, []byte("\xef\xbb\xbf"))))

		return cache[path], nil
	}
}

func splitLines(text string) []string {
	lines := strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n")

	if n := len(lines); n > 0 && lines[n-1] == "" {
		lines = lines[:n-1]
	}

	return lines
}

func defaultPages() ([]string, error) {
	out, err := exec.Command("git", "ls-files", "-z", "docs").Output()

	if err != nil {
		return nil, fmt.Errorf("git ls-files failed: %w", err)
	}

	var pages []string

	for _, p := range strings.Split(string(out), "\x00") {
		if strings.HasSuffix(p, ".md") && !recordPage.MatchString(p) {
			pages = append(pages, p)
		}
	}

	return pages, nil
}

func main() {
	baseRef := flag.String("base", "HEAD", "the ref the samples were written against (a git ref, or WORKTREE)")
	targetRef := flag.String("target", "", "the ref to regenerate from (default: the base ref)")
	write := flag.Bool("write", false, "write the regenerated pages; without it the tool only reports")
	verbose := flag.Bool("v", false, "list every refusal, not only the errors")
	recite := flag.Bool("recite", false, "take a sample found in its file outside the cited lines, and correct the comment")
	flag.Parse()

	if *targetRef == "" {
		*targetRef = *baseRef
	}

	pages := flag.Args()

	if len(pages) == 0 {
		var err error

		if pages, err = defaultPages(); err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(2)
		}
	}

	base := gitReader(*baseRef)
	target := base

	if *targetRef != *baseRef {
		target = gitReader(*targetRef)
	}

	count := map[string]int{}
	var errors, notes, reviews, rewritten []outcome
	pagesChanged := 0

	for _, page := range pages {
		data, err := os.ReadFile(filepath.FromSlash(page))

		if err != nil {
			fmt.Fprintf(os.Stderr, "cannot read %s: %v\n", page, err)
			os.Exit(2)
		}

		text := string(data)
		eol := "\n"

		if strings.Contains(text, "\r\n") {
			eol = "\r\n"
		}

		lines := strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n")
		out, outcomes := regenerate(page, lines, base, target, *recite)
		pageChanged := false

		for _, o := range outcomes {
			count[o.class]++

			switch {
			case o.class == "refused" && o.changed:
				errors = append(errors, o)
			case o.class == "refused":
				notes = append(notes, o)
			case o.class == "rewritten":
				rewritten = append(rewritten, o)
				pageChanged = true
			}

			if o.review && o.class != "refused" {
				reviews = append(reviews, o)
			}
		}

		if pageChanged {
			pagesChanged++

			if *write {
				if err := os.WriteFile(filepath.FromSlash(page), []byte(strings.Join(out, eol)), 0o644); err != nil {
					fmt.Fprintf(os.Stderr, "cannot write %s: %v\n", page, err)
					os.Exit(2)
				}
			}
		}
	}

	total := count["same"] + count["rewritten"] + count["refused"]

	if total == 0 {
		fmt.Fprintf(os.Stderr, "VACUOUS: no source comment found in %d page(s)\n", len(pages))
		os.Exit(2)
	}

	for _, o := range rewritten {
		if o.drifted {
			fmt.Printf("DRIFTED   %s:%d: the sample differed from its file at the base ref; rewritten from the file\n", o.page, o.line)
		}

		if o.recited {
			fmt.Printf("RECITED   %s:%d\n", o.page, o.line)
		} else {
			fmt.Printf("REWRITTEN %s:%d\n", o.page, o.line)
		}
	}

	for _, o := range reviews {
		fmt.Printf("REVIEW    %s:%d: a run begins or ends inside a stretch that changed size; the whole stretch was taken\n", o.page, o.line)
	}

	for _, o := range errors {
		fmt.Printf("ERROR     %s:%d: %s\n", o.page, o.line, o.reason)
	}

	if *verbose {
		for _, o := range notes {
			fmt.Printf("NOTE      %s:%d: %s\n", o.page, o.line, o.reason)
		}
	}

	verb := "would change"

	if *write {
		verb = "changed"
	}

	fmt.Printf("docsamples: base %s, target %s: %d page(s) read, %d source comment(s): %d the same, %d rewritten, %d refused (%d error(s), %d note(s)); %d page(s) %s\n",
		*baseRef, *targetRef, len(pages), total, count["same"], count["rewritten"], count["refused"], len(errors), len(notes), pagesChanged, verb)

	if len(errors) > 0 {
		os.Exit(1)
	}
}
