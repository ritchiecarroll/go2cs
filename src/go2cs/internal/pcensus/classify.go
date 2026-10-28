// classify.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"flag"
	"fmt"
	"os"
	"regexp"
	"sort"
	"strings"
)

// The pair classifier: every -/+ line pair of a unified diff is attributed to exactly one item's mechanism, to a
// named composition of two items, or refused. Lines are CR-normalized. Each run of consecutive changed lines (a
// -U0 hunk, or the changed lines between context lines) is read on its own:
//  1. an item's Join folds a removed two-line form into the one line its seat emits (E1, H2);
//  2. when the run's removed and added lines are as many, they pair in order;
//  3. otherwise, or when an in-order pair is refused, the lines are aligned by their canonical form (every item's
//     canon applied), and a line left unaligned must be one an active Block deletes or inserts (S1).
// A pair is refused as UNCLASSIFIED (no item or composition explains it), AMBIGUOUS (more than one does), or
// TEST-ONLY (only a test-refresh-1262 shape explains it, in a production file).

// mapLabel is the pseudo-item for the GoPositionMap re-encode a line-moving change forces.
const mapLabel = "MAP"

// refusal is one line pair, or one unpaired line, no mechanism explains. side is '-' or '+' for an unpaired line
// (its text may be empty), 0 for a pair.
type refusal struct {
	file, at, kind, rem, add string
	side                     byte
}

// tally is a classification's reading: pairs per label, per file, and the refusals.
type tally struct {
	labels  map[string]int
	perFile map[string]map[string]int
	refused []refusal
}

func newTally() *tally {
	return &tally{labels: map[string]int{}, perFile: map[string]map[string]int{}}
}

func (t *tally) credit(file, label string) {
	if label == "" {
		return
	}
	t.labels[label]++
	if t.perFile[file] == nil {
		t.perFile[file] = map[string]int{}
	}
	t.perFile[file][label]++
}

// classifier holds the items a classification may attribute to.
type classifier struct {
	allowed  []*item
	testOnly bool      // every changed file must be a converted test (the test-refresh-1262 rule 2)
	pool     *filePool // the current file's unexplained lines, when an allowed item carries a Block (endFile)
}

// pooled is a line no run explained, held for its file's block resolution, with the refusal it carries otherwise.
type pooled struct {
	line, at, why string
}

// filePool holds, for one file, the lines its runs left unexplained and which Block anchors the file's diff shows.
type filePool struct {
	rem, add         []pooled
	orig             []refusal // the runs' own refusals, reported as they stand when no Block anchors the file
	oldSeen, newSeen map[*item]bool
}

// key is a line's alignment form: every allowed item's canon applied, trimmed. The chain repeats until the line
// stops changing, so one item's rewrite is read by an item listed before it (S1's `switch (tag()) {` by N2's
// `))` collapse) and the form does not depend on the items' order.
func (c *classifier) key(s string) string {
	for range 8 {
		prev := s
		for _, it := range c.allowed {
			s = canon(it, s)
		}
		if s == prev {
			break
		}
	}
	return strings.TrimSpace(s)
}

// blocking reports whether any allowed item carries a Block, which makes every file pool what its runs leave.
func (c *classifier) blocking() bool {
	for _, it := range c.allowed {
		if it.Block != nil {
			return true
		}
	}
	return false
}

// noteAnchors records which Block anchors a run's lines show.
func (c *classifier) noteAnchors(rem, add []string) {
	if c.pool == nil {
		return
	}
	for _, it := range c.allowed {
		b := it.Block
		if b == nil {
			continue
		}
		for _, l := range rem {
			if b.OldAnchor.MatchString(l) {
				c.pool.oldSeen[it] = true
			}
		}
		for _, l := range add {
			if b.NewAnchor != nil && b.NewAnchor.MatchString(l) {
				c.pool.newSeen[it] = true
			}
		}
	}
}

// endFile reads the file's pool. When the file's diff shows a Block's two anchors, the pooled lines are that
// rewrite's: a removed and an added line of equal canonical form pair (a line the old form emitted out of source
// order, a line the rewrite only re-indents, or a label whose replacement git placed in another run), then Delete
// and Insert admit the scaffolding. Every line left, and every pooled line of a file without both anchors, is
// refused as its run would have refused it.
func (c *classifier) endFile(t *tally, file string) {
	p := c.pool
	if p == nil {
		return
	}
	c.pool = &filePool{oldSeen: map[*item]bool{}, newSeen: map[*item]bool{}}
	var active *item
	for _, it := range c.allowed {
		if it.Block != nil && p.oldSeen[it] && (p.newSeen[it] || it.Block.Standalone) {
			active = it
			break
		}
	}
	if active == nil {
		t.refused = append(t.refused, p.orig...)
		return
	}
	matches := func(set []*regexp.Regexp, l string) bool {
		for _, re := range set {
			if re.MatchString(l) {
				return true
			}
		}
		return false
	}
	took := make([]bool, len(p.add))
	for _, r := range p.rem {
		if active != nil {
			k, done := c.key(r.line), false
			for j, a := range p.add {
				if active.Block.Moves && !took[j] && c.key(a.line) == k {
					took[j], done = true, true
					break
				}
			}
			if done || matches(active.Block.Delete, r.line) {
				t.credit(file, active.ID)
				continue
			}
		}
		t.refused = append(t.refused, refusal{file, r.at, r.why, r.line, "", '-'})
	}
	for j, a := range p.add {
		if took[j] {
			continue // credited with its removed half
		}
		if active != nil && matches(active.Block.Insert, a.line) {
			t.credit(file, active.ID)
			continue
		}
		t.refused = append(t.refused, refusal{file, a.at, a.why, "", a.line, '+'})
	}
}

func newClassifier(only string, testOnly bool) (*classifier, error) {
	c := &classifier{testOnly: testOnly}
	if strings.TrimSpace(only) == "" {
		for i := range items {
			c.allowed = append(c.allowed, &items[i])
		}
		return c, nil
	}
	for _, id := range strings.Split(only, ",") {
		it := itemByID(strings.TrimSpace(id))
		if it == nil {
			return nil, fmt.Errorf("no item %q (see `pcensus items`)", id)
		}
		c.allowed = append(c.allowed, it)
	}
	return c, nil
}

func canon(it *item, s string) string {
	for _, r := range it.Canon {
		if r.fn != nil {
			s = r.re.ReplaceAllStringFunc(s, r.fn)
		} else {
			s = r.re.ReplaceAllString(s, r.repl)
		}
	}
	return s
}

func moves(it *item, rem, add string) bool {
	if it.Old != nil && len(it.Old.FindAllStringIndex(rem, -1)) > len(it.Old.FindAllStringIndex(add, -1)) {
		return true
	}
	return it.New != nil && len(it.New.FindAllStringIndex(add, -1)) > len(it.New.FindAllStringIndex(rem, -1))
}

func isTestFile(path string) bool { return strings.HasSuffix(path, "_test.cs") }

// isCoreProduction reports a production file of src/core, where a TestOnly shape is refused.
func isCoreProduction(path string) bool {
	return strings.HasPrefix(path, "src/core/") && !isTestFile(path) && setOf(path) == setProd
}

// explain answers the label of one changed pair, or a refusal kind. join names the item whose Join produced rem
// ("" when none): it is part of the explanation, so the canon items must explain the rest.
func (c *classifier) explain(file, rem, add, join string) (label, refusedAs string) {
	if strings.Contains(rem, "GoPositionMap") && strings.Contains(add, "GoPositionMap") {
		return mapLabel, ""
	}
	usable := func(it *item) bool {
		return it.Canon != nil && (!it.TestOnly || !isCoreProduction(file)) && (it.Guard == nil || it.Guard.MatchString(rem))
	}
	name := func(ids ...string) string {
		var parts []string
		if join != "" {
			parts = append(parts, join)
		}
		for _, id := range ids {
			if id != join {
				parts = append(parts, id)
			}
		}
		return strings.Join(parts, "+")
	}
	if rem == add {
		// Identical once CR-normalized: no change to attribute (an empty label is not credited).
		return join, ""
	}

	var singles []string
	for _, it := range c.allowed {
		if usable(it) && canon(it, rem) == canon(it, add) && moves(it, rem, add) {
			singles = append(singles, it.ID)
		}
	}
	if len(singles) == 1 {
		return name(singles[0]), ""
	}
	if len(singles) > 1 {
		return "", "AMBIGUOUS(" + strings.Join(singles, ",") + ")"
	}

	var comps []string
	for i, x := range c.allowed {
		for _, y := range c.allowed[i+1:] {
			if !usable(x) || !usable(y) || !moves(x, rem, add) || !moves(y, rem, add) {
				continue
			}
			xy := canon(y, canon(x, rem)) == canon(y, canon(x, add))
			yx := canon(x, canon(y, rem)) == canon(x, canon(y, add))
			if xy || yx {
				comps = append(comps, name(x.ID, y.ID))
			}
		}
	}
	if len(comps) == 1 {
		return comps[0], ""
	}
	if len(comps) > 1 {
		return "", "AMBIGUOUS(" + strings.Join(comps, ",") + ")"
	}

	// A test-refresh shape in a production file is refused by its own name, not as UNCLASSIFIED.
	for _, it := range c.allowed {
		if it.TestOnly && isCoreProduction(file) && (it.Guard == nil || it.Guard.MatchString(rem)) &&
			canon(it, rem) == canon(it, add) && moves(it, rem, add) {
			return "", "TEST-ONLY(" + it.ID + ")"
		}
	}
	return "", "UNCLASSIFIED"
}

// fold applies every allowed Join to a run of lines: the folded lines, and per line the joining item ("" if none).
func (c *classifier) fold(lines []string) ([]string, []string) {
	var out, by []string
	for i := 0; i < len(lines); i++ {
		joined := false
		if i+1 < len(lines) {
			for _, it := range c.allowed {
				j := it.Join
				if j == nil {
					continue
				}
				f, s := j.First.FindStringSubmatch(lines[i]), j.Second.FindStringSubmatch(lines[i+1])
				if f == nil || s == nil {
					continue
				}
				ok := true
				for _, p := range j.Same {
					if f[p[0]] != s[p[1]] {
						ok = false
					}
				}
				if ok {
					out, by = append(out, j.Fold(f, s)), append(by, it.ID)
					i++
					joined = true
					break
				}
			}
		}
		if !joined {
			out, by = append(out, lines[i]), append(by, "")
		}
	}
	return out, by
}

// run classifies one run of changed lines into t.
func (c *classifier) run(t *tally, file, at string, rem, add []string) {
	c.noteAnchors(rem, add)

	rf, rby := c.fold(rem)
	af, aby := c.fold(add)
	// A join both sides carry is not this diff's change.
	joinOf := func(i, j int) string {
		if rby[i] != "" && aby[j] == "" {
			return rby[i]
		}
		return ""
	}

	type result struct {
		labels  []string
		refused []refusal
	}
	inOrder := func() result {
		var r result
		for i := range rf {
			label, why := c.explain(file, rf[i], af[i], joinOf(i, i))
			if why != "" {
				r.refused = append(r.refused, refusal{file, at, why, rf[i], af[i], 0})
			} else {
				r.labels = append(r.labels, label)
			}
		}
		return r
	}

	var best *result
	if len(rf) == len(af) {
		r := inOrder()
		if len(r.refused) == 0 {
			for _, l := range r.labels {
				t.credit(file, l)
			}
			return
		}
		best = &r
	}

	aligned := c.aligned(file, at, rf, af, joinOf)
	if best == nil || len(aligned.refused) < len(best.refused) {
		best = &result{aligned.labels, aligned.refused}
	}
	for _, l := range best.labels {
		t.credit(file, l)
	}
	if c.pool == nil {
		t.refused = append(t.refused, best.refused...)
		return
	}
	// Under a Block, what the run could not explain is pooled for its file (endFile), keeping the refusal it would
	// carry: the rewrite's other half may sit in another run.
	c.pool.orig = append(c.pool.orig, best.refused...)
	for _, r := range best.refused {
		switch {
		case strings.HasPrefix(r.kind, "TOO-LARGE"):
			t.refused = append(t.refused, r)
		case r.side == '-':
			c.pool.rem = append(c.pool.rem, pooled{r.rem, r.at, r.kind})
		case r.side == '+':
			c.pool.add = append(c.pool.add, pooled{r.add, r.at, r.kind})
		default:
			c.pool.rem = append(c.pool.rem, pooled{r.rem, r.at, r.kind})
			c.pool.add = append(c.pool.add, pooled{r.add, r.at, r.kind})
		}
	}
}

// aligned reads a run by aligning its lines on every item's canonical form (an LCS), so a structural rewrite's
// deleted and inserted lines stand apart from the pairs it keeps.
func (c *classifier) aligned(file, at string, rem, add []string, joinOf func(i, j int) string) struct {
	labels  []string
	refused []refusal
} {
	var out struct {
		labels  []string
		refused []refusal
	}
	key := func(s string) string {
		for _, it := range c.allowed {
			s = canon(it, s)
		}
		return strings.TrimSpace(s)
	}
	n, mm := len(rem), len(add)
	if n*mm > 25_000_000 {
		out.refused = append(out.refused, refusal{file, at, fmt.Sprintf("TOO-LARGE(-%d +%d)", n, mm), "", "", 0})
		return out
	}
	rk, ak := make([]string, n), make([]string, mm)
	for i, l := range rem {
		rk[i] = key(l)
	}
	for j, l := range add {
		ak[j] = key(l)
	}
	lcs := make([][]int32, n+1)
	for i := range lcs {
		lcs[i] = make([]int32, mm+1)
	}
	for i := n - 1; i >= 0; i-- {
		for j := mm - 1; j >= 0; j-- {
			if rk[i] == ak[j] {
				lcs[i][j] = lcs[i+1][j+1] + 1
			} else if lcs[i+1][j] >= lcs[i][j+1] {
				lcs[i][j] = lcs[i+1][j]
			} else {
				lcs[i][j] = lcs[i][j+1]
			}
		}
	}

	refuse := func(kind, r, a string, side byte) {
		out.refused = append(out.refused, refusal{file, at, kind, r, a, side})
	}
	pair := func(i, j int) {
		label, why := c.explain(file, rem[i], add[j], joinOf(i, j))
		if why == "" {
			out.labels = append(out.labels, label)
		} else {
			refuse(why, rem[i], add[j], 0)
		}
	}
	var lostRem, lostAdd []int // the lines the alignment leaves unpaired, read once the walk is done
	unpaired := func() {
		for _, i := range lostRem {
			refuse("UNPAIRED", rem[i], "", '-')
		}
		for _, j := range lostAdd {
			refuse("UNPAIRED", "", add[j], '+')
		}
	}
	unpairedRem := func(i int) { lostRem = append(lostRem, i) }
	unpairedAdd := func(j int) { lostAdd = append(lostAdd, j) }

	i, j := 0, 0
	for i < n && j < mm {
		switch {
		case rk[i] == ak[j]:
			pair(i, j)
			i, j = i+1, j+1
		case lcs[i+1][j] >= lcs[i][j+1]:
			unpairedRem(i)
			i++
		default:
			unpairedAdd(j)
			j++
		}
	}
	for ; i < n; i++ {
		unpairedRem(i)
	}
	for ; j < mm; j++ {
		unpairedAdd(j)
	}
	unpaired()
	return out
}

// classifyDiff reads a unified diff (git's, any -U) and classifies every changed line.
func (c *classifier) classifyDiff(diff string) *tally {
	t := newTally()
	c.pool = nil
	if c.blocking() {
		c.pool = &filePool{oldSeen: map[*item]bool{}, newSeen: map[*item]bool{}}
	}
	var file, at, whole string
	var rem, add []string
	header := false // between `diff --git` (or a first `---`) and the first `@@`
	flush := func() {
		if len(rem) > 0 || len(add) > 0 {
			switch {
			case c.testOnly && !isTestFile(file):
				t.refused = append(t.refused, refusal{file, at, "NOT-A-TEST-FILE", first(rem), first(add), 0})
			case whole != "":
				// A file added or deleted whole is new test source or a removed one, not a mechanism: it is
				// reported once per file, never paired.
			default:
				c.run(t, file, at, rem, add)
			}
		}
		rem, add = nil, nil
	}
	for _, ln := range strings.Split(strings.ReplaceAll(diff, "\r\n", "\n"), "\n") {
		ln = strings.TrimSuffix(ln, "\r")
		switch {
		case strings.HasPrefix(ln, "diff --git "):
			flush()
			c.endFile(t, file)
			header, whole = true, ""
			if _, b, ok := strings.Cut(ln, " b/"); ok {
				file = b
			}
		case (header || at == "") && (strings.HasPrefix(ln, "+++ ") || strings.HasPrefix(ln, "--- ")):
			flush()
			c.endFile(t, file)
			header = true
			p := ln[4:]
			switch {
			case p == "/dev/null" && ln[0] == '-':
				whole = "NEW-FILE"
			case p == "/dev/null":
				whole = "DELETED-FILE"
			case ln[0] == '+':
				file = strings.TrimPrefix(p, "b/")
			}
			if ln[0] == '+' && whole != "" {
				t.credit(file, whole)
			}
		case strings.HasPrefix(ln, "@@"):
			flush()
			header = false
			flush()
			at = ln
		case strings.HasPrefix(ln, "-"):
			if len(add) > 0 { // a removal after additions starts a new change run
				flush()
			}
			rem = append(rem, strings.TrimSuffix(ln[1:], "\r"))
		case strings.HasPrefix(ln, "+"):
			add = append(add, strings.TrimSuffix(ln[1:], "\r"))
		default: // a context line, "\ No newline at end of file", or anything else ends the run
			flush()
		}
	}
	flush()
	c.endFile(t, file)
	return t
}

func first(lines []string) string {
	if len(lines) == 0 {
		return ""
	}
	return lines[0]
}

func runClassify(args []string) int {
	fl := flag.NewFlagSet("classify", flag.ExitOnError)
	only := fl.String("items", "", "comma-separated item IDs a pair may be attributed to (default: all)")
	testOnly := fl.Bool("test-only", false, "refuse any changed file that is not a converted test (*_test.cs)")
	fl.Parse(args)
	if fl.NArg() != 1 {
		usage()
	}
	c, err := newClassifier(*only, *testOnly)
	if err != nil {
		fmt.Fprintln(os.Stderr, "pcensus:", err)
		return 2
	}
	diff, err := os.ReadFile(fl.Arg(0))
	if err != nil {
		fmt.Fprintln(os.Stderr, "pcensus:", err)
		return 2
	}
	t := c.classifyDiff(string(diff))
	printTally(t)
	if len(t.refused) > 0 {
		return 1
	}
	return 0
}

func sortedKeys(m map[string]int) []string {
	keys := make([]string, 0, len(m))
	for k := range m {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	return keys
}

func printTally(t *tally) {
	files := make([]string, 0, len(t.perFile))
	for f := range t.perFile {
		files = append(files, f)
	}
	sort.Strings(files)
	for _, f := range files {
		var parts []string
		for _, k := range sortedKeys(t.perFile[f]) {
			parts = append(parts, fmt.Sprintf("%s=%d", k, t.perFile[f][k]))
		}
		fmt.Printf("%s\t%s\n", f, strings.Join(parts, " "))
	}
	var parts []string
	total := 0
	for _, k := range sortedKeys(t.labels) {
		parts = append(parts, fmt.Sprintf("%s=%d", k, t.labels[k]))
		total += t.labels[k]
	}
	fmt.Printf("FILES %d  PAIRS %d  %s  REFUSED %d\n", len(files), total, strings.Join(parts, " "), len(t.refused))
	for _, r := range t.refused {
		fmt.Printf("REFUSED %s %s %s\n  - %s\n  + %s\n", r.kind, r.file, r.at, r.rem, r.add)
	}
}
