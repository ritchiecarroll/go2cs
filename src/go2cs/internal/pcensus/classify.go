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

// refusal is one line pair, or one unpaired line, no mechanism explains.
type refusal struct {
	file, at, kind, rem, add string
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
	testOnly bool // every changed file must be a converted test (the test-refresh-1262 rule 2)
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
				r.refused = append(r.refused, refusal{file, at, why, rf[i], af[i]})
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
	t.refused = append(t.refused, best.refused...)
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
	var blocks []*item
	for _, it := range c.allowed {
		b := it.Block
		if b == nil {
			continue
		}
		hasOld, hasNew := false, false
		for _, l := range rem {
			hasOld = hasOld || b.OldAnchor.MatchString(l)
		}
		for _, l := range add {
			hasNew = hasNew || b.NewAnchor != nil && b.NewAnchor.MatchString(l)
		}
		if hasOld && (hasNew || b.Standalone) {
			blocks = append(blocks, it)
		}
	}
	block := func(line string, del bool) string {
		for _, it := range blocks {
			set := it.Block.Insert
			if del {
				set = it.Block.Delete
			}
			for _, re := range set {
				if re.MatchString(line) {
					return it.ID
				}
			}
		}
		return ""
	}

	n, mm := len(rem), len(add)
	if n*mm > 25_000_000 {
		out.refused = append(out.refused, refusal{file, at, fmt.Sprintf("TOO-LARGE(-%d +%d)", n, mm), "", ""})
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

	refuse := func(kind, r, a string) { out.refused = append(out.refused, refusal{file, at, kind, r, a}) }
	pair := func(i, j int) {
		label, why := c.explain(file, rem[i], add[j], joinOf(i, j))
		switch {
		case why == "":
			out.labels = append(out.labels, label)
		case strings.TrimSpace(rem[i]) == strings.TrimSpace(add[j]) && len(blocks) > 0:
			out.labels = append(out.labels, blocks[0].ID) // a kept line the structural rewrite re-indents
		default:
			refuse(why, rem[i], add[j])
		}
	}
	unpairedRem := func(i int) {
		if id := block(rem[i], true); id != "" {
			out.labels = append(out.labels, id)
		} else {
			refuse("UNPAIRED", rem[i], "")
		}
	}
	unpairedAdd := func(j int) {
		if id := block(add[j], false); id != "" {
			out.labels = append(out.labels, id)
		} else {
			refuse("UNPAIRED", "", add[j])
		}
	}

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
	return out
}

// classifyDiff reads a unified diff (git's, any -U) and classifies every changed line.
func (c *classifier) classifyDiff(diff string) *tally {
	t := newTally()
	var file, at, whole string
	var rem, add []string
	header := false // between `diff --git` (or a first `---`) and the first `@@`
	flush := func() {
		if len(rem) > 0 || len(add) > 0 {
			switch {
			case c.testOnly && !isTestFile(file):
				t.refused = append(t.refused, refusal{file, at, "NOT-A-TEST-FILE", first(rem), first(add)})
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
			header, whole = true, ""
			if _, b, ok := strings.Cut(ln, " b/"); ok {
				file = b
			}
		case (header || at == "") && (strings.HasPrefix(ln, "+++ ") || strings.HasPrefix(ln, "--- ")):
			flush()
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
