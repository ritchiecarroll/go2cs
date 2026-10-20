// census.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Package commentcensus measures how faithfully converted C# keeps Go's comments and blank lines: comments
// missed or misplaced, end-of-line comment runs that gofmt aligned and C# does not, runs of two or more blank
// lines, and blank lines Go has where C# has none (and the reverse), at top level, between the specs of a group
// and inside function bodies.
//
// It is the instrument behind docs/PLAN-comment-and-spacing-parity.md (the `ccensus` of its Appendix A, moved
// here unchanged in its measures) and the guard of that plan's seats: the planted red test in this package,
// commentParityUtf8_test.go in the converter, and TestCommentParityCensus, the corpus ratchet. Every count it
// reports is a defect count; a fix lowers some and a regression raises one. See Counts for the class keys.
package commentcensus

import (
	"fmt"
	"go/ast"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
)

var goosDirs = map[string]bool{"windows": true, "linux": true, "darwin": true}

type pair struct {
	pkg, base, cs, gofile, flavor string
}

func findPairs(core, goroot string) []pair {
	type cand struct {
		p    pair
		rank int
	}
	best := map[string]cand{}
	filepath.Walk(core, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return nil
		}
		rel, _ := filepath.Rel(core, path)
		rel = filepath.ToSlash(rel)
		if info.IsDir() {
			b := info.Name()
			if b == "golib" || b == "obj" || b == "bin" || b == "Generated" || b == "go2cs" || rel == "unsafe" || rel == "testing" {
				return filepath.SkipDir
			}
			return nil
		}
		name := info.Name()
		if !strings.HasSuffix(name, ".cs") || strings.HasSuffix(name, "_test.cs") || strings.HasSuffix(name, "_impl.cs") ||
			name == "package_info.cs" || name == "package_test_info.cs" || name == "go2cs_test_host.cs" || strings.HasPrefix(name, "package_") {
			return nil
		}
		dir := filepath.Dir(rel)
		flavor := ""
		if goosDirs[filepath.Base(dir)] {
			parent := filepath.Dir(dir)
			// a flavour dir has no project of its own and sits in a package dir that has one
			if m, _ := filepath.Glob(filepath.Join(core, dir, "*.csproj")); len(m) == 0 {
				if m2, _ := filepath.Glob(filepath.Join(core, parent, "*.csproj")); len(m2) > 0 {
					flavor = filepath.Base(dir)
					dir = parent
				}
			}
		}
		data, err := os.ReadFile(path)
		if err != nil || strings.Contains(string(data), "GoManualConversion") {
			return nil
		}
		base := strings.TrimSuffix(name, ".cs")
		gofile := filepath.Join(goroot, "src", filepath.FromSlash(dir), base+".go")
		if _, err := os.Stat(gofile); err != nil {
			return nil
		}
		rank := map[string]int{"": 0, "windows": 1, "linux": 2, "darwin": 3}[flavor]
		key := dir + "/" + base
		if c, ok := best[key]; !ok || rank < c.rank {
			best[key] = cand{pair{dir, base, path, gofile, flavor}, rank}
		}
		return nil
	})
	var out []pair
	for _, c := range best {
		out = append(out, c.p)
	}
	sort.Slice(out, func(i, j int) bool { return out[i].pkg+"/"+out[i].base < out[j].pkg+"/"+out[j].base })
	return out
}

var keywords = map[string]bool{}

func init() {
	for _, k := range strings.Fields(`if else for return var func const type package import switch case default go defer select range struct interface map chan break continue goto fallthrough nil true false
public internal private protected static partial class readonly new ref out in this var using namespace foreach while do throw try catch finally null void bool byte int uint long ulong short ushort float double string object nint nuint rune
@string array slice ж Ꮡ sizeof typeof is as unsafe fixed get set value goto lock checked unchecked override virtual abstract sealed extern implicit explicit operator event delegate enum struct`) {
		keywords[k] = true
	}
}

var identRe = regexp.MustCompile(`[\pL_][\pL\pN_]*`)

var decorRe = regexp.MustCompile(`(ʗp|ᴛ\d*|ʗ\d*)$`)

func csIdents(code string) map[string]bool {
	m := map[string]bool{}
	for _, w := range identRe.FindAllString(code, -1) {
		if !keywords[w] {
			m[w] = true
			n := strings.TrimLeft(w, "ᏑΔ@")
			n = decorRe.ReplaceAllString(n, "")
			m[n] = true
		}
	}
	return m
}

func goIdents(ids []string) map[string]bool {
	m := map[string]bool{}
	for _, w := range ids {
		if !keywords[w] && w != "_" {
			m[w] = true
		}
	}
	return m
}

func intersects(a, b map[string]bool) bool {
	for k := range a {
		if b[k] {
			return true
		}
	}
	return false
}

type counter map[string]int

func (c counter) add(k string, n int) { c[k] += n }

type example struct{ class, where, detail string }

type results struct {
	files            int
	groupsByClass    counter
	missedByClass    counter
	partialByClass   counter
	missedLines      int
	presentLines     int
	misplacedByClass counter
	misplacedKind    counter
	checkedByClass   counter
	eol              counter
	csMisaligned     int
	csRuns           int
	goRunsMisaligned int
	blank2           counter
	topLevel         counter
	groupSpec        counter
	body             counter
	examples         map[string][]example
	missedByPkg      counter
	filesByClass     counter
	fileHit          map[string]bool
	groupsByPkg      counter
}

func newResults() *results {
	return &results{groupsByClass: counter{}, missedByClass: counter{}, partialByClass: counter{}, misplacedByClass: counter{},
		misplacedKind: counter{}, checkedByClass: counter{}, eol: counter{}, blank2: counter{}, topLevel: counter{}, groupSpec: counter{},
		body: counter{}, examples: map[string][]example{}, missedByPkg: counter{}, groupsByPkg: counter{}, filesByClass: counter{}}
}

func (r *results) ex(class, where, detail string) {
	if r.fileHit != nil {
		k := class
		if i := strings.Index(k[strings.Index(k, ":")+1:], ":"); i >= 0 && strings.HasPrefix(k, "missed:") {
			k = "missed:" + k[strings.Index(k, ":")+1:][:i]
		}
		if strings.HasPrefix(k, "missed:") || strings.HasPrefix(k, "partial:") {
			k = "missed-or-partial"
		} else if strings.HasPrefix(k, "misplaced:") {
			k = "misplaced:" + strings.Split(k, ":")[1]
		}
		r.fileHit[k] = true
	}
	if len(r.examples[class]) < 400 {
		r.examples[class] = append(r.examples[class], example{class, where, detail})
	}
}

func isBlankCS(cs *csFile, ln int) bool { // 1-based
	return ln >= 1 && ln <= len(cs.lines) && strings.TrimSpace(cs.lines[ln-1]) == ""
}

func commentOnly(cs *csFile, ln int) bool {
	return !cs.codeOnLine[ln-1] && strings.TrimSpace(cs.lines[ln-1]) != ""
}

var attrRe = regexp.MustCompile(`^\s*\[[A-Za-z]`)

// scaffoldRe matches the lines the converter writes ahead of a statement's own translation: a func literal's
// capture copy (`var xʗ1 = x;`) and a heap box for an escaping local (`ref var x = ref heap<T>(out var Ꮡx);`).
var scaffoldRe = regexp.MustCompile(`^(var [\pL\pN_@]+ʗ\d+ = [\pL\pN_@Ꮡ.]+;|ref var [\pL\pN_@]+ = ref heap(<.*>)?\(.*out var Ꮡ[\pL\pN_@]+\);)$`)

// walkScaffoldRe is the part of scaffoldRe that is never a statement of its own: a capture copy, and the BARE heap
// box `ref var x = ref heap<T>(out var Ꮡx);`, which the next line's assignment completes. A box WITH an initializer
// (`ref var buf = ref heap(new T(), out var Ꮡbuf);`, an escaping `var buf T`) is a whole Go statement, so the walk
// that places a Go blank must stop at it: crossing it credits the previous statement's blank to the line below.
var walkScaffoldRe = regexp.MustCompile(`^(var [\pL\pN_@]+ʗ\d+ = [\pL\pN_@Ꮡ.]+;|ref var [\pL\pN_@]+ = ref heap<.*>\(out var Ꮡ[\pL\pN_@]+\);)$`)

// gotoLabelRe matches the goto a labeled continue or break becomes; its label is Go's.
var gotoLabelRe = regexp.MustCompile(`\bgoto (?:continue|break)_([\pL_][\pL\pN_]*);`)

// anchorIdents is csIdents for a comment's anchor line, plus the Go label of a labeled continue or break.
func anchorIdents(code string) map[string]bool {
	m := csIdents(code)

	for _, match := range gotoLabelRe.FindAllStringSubmatch(code, -1) {
		m[match[1]] = true
	}

	return m
}

func nextCodeLineCS(cs *csFile, ln int) int {
	for l := ln + 1; l <= len(cs.lines); l++ {
		if cs.codeOnLine[l-1] && !attrRe.MatchString(cs.codeText[l-1]) {
			return l
		}
	}
	return 0
}

func analyze(p pair, r *results, verbose bool) {
	g, err := parseGo(p.gofile)
	if err != nil {
		return
	}
	data, err := os.ReadFile(p.cs)
	if err != nil {
		return
	}
	cs := lexCS(string(data))
	r.files++
	r.fileHit = map[string]bool{}
	defer func() {
		for k := range r.fileHit {
			r.filesByClass.add(k, 1)
		}
	}()
	where := p.pkg + "/" + p.base

	// ---- 1+2: assign Go comment lines to C# comment occurrences, in order ----
	queue := map[string][]int{}
	for i, c := range cs.comments {
		queue[c.text] = append(queue[c.text], i)
	}
	used := map[int]bool{}
	take := func(t string) int {
		q := queue[t]
		for len(q) > 0 {
			i := q[0]
			q = q[1:]
			if !used[i] {
				used[i] = true
				queue[t] = q
				return i
			}
		}
		queue[t] = q
		return -1
	}
	firstCS := map[*goGroup]int{}
	lineCS := map[int]int{} // go line -> cs comment index (first comment on that line)
	for _, gg := range g.groups {
		if len(gg.lines) == 0 {
			continue
		}
		r.groupsByClass.add(gg.class, 1)
		if gg.declKind != "" {
			r.groupsByClass.add("  by decl kind: "+gg.class+" / "+gg.declKind, 1)
		}
		r.groupsByPkg.add(p.pkg, 1)
		found := 0
		first := -1
		for k, t := range gg.lines {
			i := take(t)
			if i >= 0 {
				found++
				if first < 0 {
					first = i
				}
				if _, ok := lineCS[gg.lineNos[k]]; !ok {
					lineCS[gg.lineNos[k]] = i
				}
			}
		}
		r.missedLines += len(gg.lines) - found
		r.presentLines += found
		switch {
		case found == 0:
			r.missedByClass.add(gg.class, 1)
			if gg.declKind != "" {
				r.missedByClass.add("  by decl kind: "+gg.class+" / "+gg.declKind, 1)
			}
			r.missedByPkg.add(p.pkg, 1)
			r.ex("missed:"+gg.class, fmt.Sprintf("%s.go:%d", where, gg.startLn), gg.lines[0])
		case found < len(gg.lines):
			r.partialByClass.add(gg.class, 1)
			r.ex("partial:"+gg.class, fmt.Sprintf("%s.go:%d", where, gg.startLn), fmt.Sprintf("%d of %d lines; first: %s", found, len(gg.lines), gg.lines[0]))
		}
		if found == len(gg.lines) {
			firstCS[gg] = first
		}
	}

	// ---- 1: misplacement of fully-present groups ----
	for _, gg := range g.groups {
		i, ok := firstCS[gg]
		if !ok || gg.class == "directive" || gg.class == "file-header" || gg.class == "package-doc" {
			continue
		}
		c := cs.comments[i]
		r.checkedByClass.add(gg.class, 1)
		var goAnchor map[string]bool
		goFirst := ""
		goNextTok := token.ILLEGAL
		firstOf := func(ids []string) string {
			for _, w := range ids {
				if !keywords[w] && w != "_" {
					return w
				}
			}
			return ""
		}
		if gg.trailing {
			goAnchor = goIdents(g.idents[gg.startLn])
			goFirst = firstOf(g.idents[gg.startLn])
		} else {
			for ln := gg.endLn + 1; ln <= len(g.src); ln++ {
				if t, ok := g.firstTok[ln]; ok {
					goAnchor = goIdents(g.idents[ln])
					goFirst = firstOf(g.idents[ln])
					goNextTok = t
					break
				}
			}
		}
		var csAnchor map[string]bool
		csNext := ""
		if c.trailing {
			csAnchor = anchorIdents(cs.codeText[c.line-1])
		} else {
			// skip the rest of this comment block, then the next code line; past the converter's scaffolding
			// while it does not carry Go's anchor
			nl := nextCodeLineCS(cs, c.line)
			for nl > 0 && goFirst != "" && !anchorIdents(cs.codeText[nl-1])[goFirst] && scaffoldRe.MatchString(strings.TrimSpace(cs.codeText[nl-1])) {
				nl = nextCodeLineCS(cs, nl)
			}
			if nl > 0 {
				csAnchor = anchorIdents(cs.codeText[nl-1])
				csNext = strings.TrimSpace(cs.codeText[nl-1])
			}
		}
		kind := ""
		switch {
		case gg.trailing && !c.trailing:
			kind = "trailing-to-own-line"
		case !gg.trailing && c.trailing:
			kind = "own-line-to-trailing"
		case len(goAnchor) > 0 && len(csAnchor) > 0 && !csAnchor[goFirst]:
			kind = "anchor-differs"
		// a closing brace: C# closes a struct literal's constructor call with `)`
		case len(goAnchor) == 0 && goNextTok == token.RBRACE && !strings.HasPrefix(csNext, "}") && !strings.HasPrefix(csNext, ")"):
			kind = "anchor-differs"
		}
		if kind != "" {
			r.misplacedByClass.add(gg.class, 1)
			r.misplacedKind.add(gg.class+" / "+kind, 1)
			r.ex("misplaced:"+gg.class+":"+kind, fmt.Sprintf("%s.go:%d -> .cs:%d", where, gg.startLn, c.line), gg.lines[0])
		}
	}

	// ---- 3: EOL alignment, Go-anchored ----
	type tl struct {
		line, col int
		indent    string
	}
	var goTrail []tl
	for _, gg := range g.groups {
		if gg.trailing && len(gg.cols) > 0 && gg.cols[0] >= 0 && gg.class != "directive" {
			src := g.src[gg.startLn-1]
			ind := src[:len(src)-len(strings.TrimLeft(src, "\t "))]
			goTrail = append(goTrail, tl{gg.startLn, gg.cols[0], ind})
		}
	}
	sort.Slice(goTrail, func(i, j int) bool { return goTrail[i].line < goTrail[j].line })
	for i := 0; i < len(goTrail); {
		j := i + 1
		for j < len(goTrail) && goTrail[j].line == goTrail[j-1].line+1 && goTrail[j].indent == goTrail[i].indent {
			j++
		}
		if j-i >= 2 {
			run := goTrail[i:j]
			aligned := true
			for _, t := range run {
				if t.col != run[0].col {
					aligned = false
				}
			}
			if !aligned {
				r.goRunsMisaligned++
				r.eol.add("go-run-not-aligned-by-gofmt", 1)
			} else {
				status := ""
				var csl []int
				for _, t := range run {
					ci, ok := lineCS[t.line]
					if !ok {
						status = "missing"
						break
					}
					if !cs.comments[ci].trailing {
						status = "moved-to-own-line"
						break
					}
					csl = append(csl, ci)
				}
				if status == "" {
					consecutive, same := true, true
					for k := 1; k < len(csl); k++ {
						if cs.comments[csl[k]].line != cs.comments[csl[k-1]].line+1 {
							consecutive = false
						}
						if cs.comments[csl[k]].col != cs.comments[csl[0]].col {
							same = false
						}
					}
					switch {
					case consecutive && same:
						status = "aligned"
					case consecutive:
						status = "misaligned"
					default:
						status = "split"
					}
				}
				r.eol.add("go-aligned-run:"+status, 1)
				r.eol.add("go-aligned-run-lines:"+status, len(run))
				if status != "aligned" {
					r.ex("eol:"+status, fmt.Sprintf("%s.go:%d-%d", where, run[0].line, run[len(run)-1].line), strings.TrimSpace(g.src[run[0].line-1]))
				}
			}
		}
		i = j
	}
	// C#-side runs of consecutive trailing comments
	var csTrail []csComment
	seenLine := map[int]bool{}
	for _, c := range cs.comments {
		if c.trailing && !c.block && !seenLine[c.line] {
			csTrail = append(csTrail, c)
			seenLine[c.line] = true
		}
	}
	for i := 0; i < len(csTrail); {
		j := i + 1
		for j < len(csTrail) && csTrail[j].line == csTrail[j-1].line+1 {
			j++
		}
		if j-i >= 2 {
			r.csRuns++
			for k := i + 1; k < j; k++ {
				if csTrail[k].col != csTrail[i].col {
					r.csMisaligned++
					break
				}
			}
		}
		i = j
	}

	// ---- 4a: runs of 2+ blank lines in C# ----
	for ln := 1; ln <= len(cs.lines); {
		if !isBlankCS(cs, ln) {
			ln++
			continue
		}
		e := ln
		for e+1 <= len(cs.lines) && isBlankCS(cs, e+1) {
			e++
		}
		if e-ln+1 >= 2 && e < len(cs.lines) {
			prev, next := "", ""
			if ln > 1 {
				prev = cs.lines[ln-2]
			}
			if e < len(cs.lines) {
				next = cs.lines[e]
			}
			ctx := blankContext(prev, next)
			r.blank2.add(ctx, 1)
			r.blank2.add("lines", e-ln+1)
			r.ex("blank2:"+ctx, fmt.Sprintf("%s.cs:%d", where, ln), strings.TrimSpace(prev)+" ⏎⏎ "+strings.TrimSpace(next))
		}
		ln = e + 1
	}

	// ---- 4b: blank-line separation, Go vs C# ----
	declLines := []int{}
	for i := range cs.lines {
		if isCSDecl(cs, i+1) {
			declLines = append(declLines, i+1)
		}
	}
	cursor := 0
	findDeclKind := func(name, kind string) int {
		q := `(^|[^\pL\pN_@ΔᏑ])Ꮡ?Δ?@?` + regexp.QuoteMeta(name)
		var re *regexp.Regexp
		switch kind {
		case "func":
			re = regexp.MustCompile(q + `\s*[(<]`)
		case "type":
			re = regexp.MustCompile(`(struct|interface|class|record|delegate)\s+Δ?@?` + regexp.QuoteMeta(name) + `([^\pL\pN_]|$)`)
		default:
			re = regexp.MustCompile(q + `\s*(=>|=|;|,)`)
		}
		for k := cursor; k < len(declLines); k++ {
			if re.MatchString(cs.codeText[declLines[k]-1]) {
				cursor = k + 1
				return declLines[k]
			}
		}
		return 0
	}
	findDecl := func(name string) int { return findDeclKind(name, "value") }
	csStart := func(ln int) int { // walk up over comments and attributes
		s := ln
		for s-1 >= 1 && (commentOnly(cs, s-1) || attrRe.MatchString(cs.lines[s-2])) {
			s--
		}
		return s
	}
	goBlankAbove := func(startLn int) bool { return g.blank[startLn-1] }
	decls := g.file.Decls
	for di, d := range decls {
		name, startLn := "", 0
		switch x := d.(type) {
		case *ast.FuncDecl:
			name = x.Name.Name
			startLn = g.fset.Position(x.Pos()).Line
			if x.Doc != nil {
				startLn = g.fset.Position(x.Doc.Pos()).Line
			}
		case *ast.GenDecl:
			if x.Tok == token.IMPORT {
				continue
			}
			startLn = g.fset.Position(x.Pos()).Line
			if x.Doc != nil {
				startLn = g.fset.Position(x.Doc.Pos()).Line
			}
			for _, sp := range x.Specs {
				switch s := sp.(type) {
				case *ast.ValueSpec:
					for _, n := range s.Names {
						if n.Name != "_" {
							name = n.Name
							break
						}
					}
				case *ast.TypeSpec:
					name = s.Name.Name
				}
				if name != "" {
					break
				}
			}
		}
		if name == "" || name == "init" || name == "_" {
			continue
		}
		dkind := "value"
		switch x := d.(type) {
		case *ast.FuncDecl:
			dkind = "func"
		case *ast.GenDecl:
			if x.Tok == token.TYPE {
				dkind = "type"
			}
		}
		save := cursor
		cl := findDeclKind(name, dkind)
		if cl == 0 {
			cursor = save
		}
		if cl == 0 {
			k := "func"
			if gd, ok := d.(*ast.GenDecl); ok {
				k = genKind(gd)
			} else if d.(*ast.FuncDecl).Recv != nil {
				k = "method"
			}
			r.topLevel.add("unmapped:"+k, 1)
			r.ex("top:unmapped:"+k, fmt.Sprintf("%s.go:%d", where, startLn), name)
			continue
		}
		if di > 0 {
			gb := goBlankAbove(startLn)
			cb := isBlankCS(cs, csStart(cl)-1)
			switch {
			case gb && !cb:
				r.topLevel.add("go-blank-cs-none", 1)
				r.ex("top:go-blank-cs-none", fmt.Sprintf("%s.go:%d -> .cs:%d", where, startLn, cl), strings.TrimSpace(cs.lines[cl-1]))
			case !gb && cb:
				r.topLevel.add("go-none-cs-blank", 1)
				r.ex("top:go-none-cs-blank", fmt.Sprintf("%s.go:%d -> .cs:%d", where, startLn, cl), strings.TrimSpace(cs.lines[cl-1]))
			default:
				r.topLevel.add("same", 1)
			}
		}
		// grouped const/var specs
		if gd, ok := d.(*ast.GenDecl); ok && gd.Lparen.IsValid() && (gd.Tok == token.CONST || gd.Tok == token.VAR) {
			for si, sp := range gd.Specs {
				vs := sp.(*ast.ValueSpec)
				if si == 0 {
					continue
				}
				nm := vs.Names[0].Name
				if nm == "_" {
					continue
				}
				sl := g.fset.Position(vs.Pos()).Line
				if vs.Doc != nil {
					sl = g.fset.Position(vs.Doc.Pos()).Line
				}
				scl := findDecl(nm)
				if scl == 0 {
					r.groupSpec.add("unmapped", 1)
					continue
				}
				gb := g.blank[sl-1]
				cb := isBlankCS(cs, csStart(scl)-1)
				switch {
				case gb && !cb:
					r.groupSpec.add("go-blank-cs-none", 1)
					r.ex("group:go-blank-cs-none", fmt.Sprintf("%s.go:%d -> .cs:%d", where, sl, scl), strings.TrimSpace(cs.lines[scl-1]))
				case !gb && cb:
					r.groupSpec.add("go-none-cs-blank", 1)
					r.ex("group:go-none-cs-blank", fmt.Sprintf("%s.go:%d -> .cs:%d", where, sl, scl), strings.TrimSpace(cs.lines[scl-1]))
				default:
					r.groupSpec.add("same", 1)
				}
			}
		}
		// function bodies: each Go blank line, matched to its element in C#; each C# blank, classified
		if fd, ok := d.(*ast.FuncDecl); ok && fd.Body != nil {
			lb := g.fset.Position(fd.Body.Lbrace).Line
			rb := g.fset.Position(fd.Body.Rbrace).Line
			cOpen, cEnd, okb := csBodyRange(cs, cl)
			if !okb {
				r.body.add("cs-body-not-found", 1)
				continue
			}
			r.body.add("functions", 1)
			matchedStarts := map[int]bool{}
			separator := preludeSeparator(cs, g, cOpen, cEnd, lb, rb)
			cur := cOpen + 1
			for L := lb + 1; L < rb; L++ {
				if !g.blank[L-1] || g.blank[L] {
					continue
				}
				// L is the first non-blank line after a Go blank line inside the body
				csLine := 0
				if ci, ok := lineCS[L]; ok && cs.comments[ci].line > cOpen && cs.comments[ci].line < cEnd {
					csLine = cs.comments[ci].line
				} else if ids := goAnchorIdents(g.idents[L]); len(ids) > 0 {
					for c := cur; c < cEnd; c++ {
						if !cs.codeOnLine[c-1] {
							continue
						}
						ci := csIdents(cs.codeText[c-1])
						all := true
						for k := range ids {
							if !ci[k] {
								all = false
								break
							}
						}
						if all {
							csLine = c
							break
						}
					}
				}
				r.body.add("go-blank-lines", 1)
				if csLine == 0 {
					r.body.add("go-blank:unmapped", 1)
					continue
				}
				cur = csLine + 1
				// The walk back crosses what the converter writes ahead of L's own translation: comments, attributes, and
				// the lines that are only a prefix of L (walkScaffoldRe), so an escaping local's bare heap box above its
				// assignment is part of L.
				st := csLine
				for st-1 > cOpen && (commentOnly(cs, st-1) || attrRe.MatchString(cs.lines[st-2]) || walkScaffoldRe.MatchString(strings.TrimSpace(cs.codeText[st-2]))) {
					st--
				}
				matchedStarts[st] = true
				// A walk back over comment lines must not land on the prelude's separator: that blank is the
				// converter's, never Go's. When a function opens with a comment and Go puts a blank line between
				// it and the first statement, the C# that drops that blank keeps the comment directly under the
				// prelude's separator, which the walk would otherwise credit as Go's blank. It applies only when
				// Go's own line above its blank is a comment, so the comment the walk crossed is that one; where
				// it is code, the identifier search can anchor on an earlier line that names the same identifiers.
				goAbove := strings.TrimSpace(g.src[L-3])
				goCommentAbove := strings.HasPrefix(goAbove, "//") || strings.HasSuffix(goAbove, "*/")
				if isBlankCS(cs, st-1) && !(goCommentAbove && st < csLine && st-1 == separator) {
					r.body.add("go-blank:cs-blank", 1)
				} else {
					r.body.add("go-blank:cs-NONE", 1)
					r.ex("body:go-blank-cs-none", fmt.Sprintf("%s.go:%d -> .cs:%d", where, L, st), strings.TrimSpace(g.src[L-1]))
				}
			}
			for c := cOpen + 1; c < cEnd; c++ {
				if !isBlankCS(cs, c) || isBlankCS(cs, c-1) {
					continue
				}
				nx := c + 1
				for nx < cEnd && isBlankCS(cs, nx) {
					nx++
				}
				if matchedStarts[nx] {
					continue
				}
				if c == separator {
					r.body.add("prelude-separator", 1)
					continue
				}
				prev := strings.TrimSpace(cs.codeText[c-2])
				kind := "extra:unexplained"
				switch {
				case strings.HasSuffix(prev, "= default!;") || strings.HasSuffix(prev, "= default;"):
					kind = "extra:after-named-result-locals"
				case strings.HasSuffix(prev, "}}"):
					kind = "extra:after-lowered-switch"
				case preludeRe.MatchString(prev):
					kind = "extra:after-param-prelude"
				case c-1 == cOpen:
					kind = "extra:after-open-brace"
				case nx == cEnd || strings.HasPrefix(strings.TrimSpace(cs.codeText[nx-1]), "}"):
					kind = "extra:before-close-brace"
				}
				r.body.add(kind, 1)
				if kind == "extra:unexplained" {
					r.ex("body:"+kind, fmt.Sprintf("%s.cs:%d", where, c), prev+" ⏎ "+strings.TrimSpace(cs.lines[nx-1]))
				}
			}
		}
	}
	if verbose {
		fmt.Fprintf(os.Stderr, "%s: %d groups\n", where, len(g.groups))
	}
}

// preludeLineRe matches a line of the converter's prelude, the lines it writes after a function's opening brace
// and ahead of the Go body: named-result locals and variadic slices (`T n = default!;`, `var xs = xsʗp.slice();`),
// array clones (`a = a.Clone();`), pointer derefs, heap boxes and named-result aliases (`ref var t = ref Ꮡt.val;`,
// `ref var x = ref heap(xʗp, out var Ꮡx);`, `heap<T>(out var Ꮡn);`), and a deferring function's frame head
// (`GoFrame ᒐ = default;`, `try {`).
var preludeLineRe = regexp.MustCompile(`^([^=(]+ [\pL\pN_@]+ = [^;]+;|[\pL\pN_@]+ = [\pL\pN_@]+\.\w+\(\);|ref (var|\S+) [\pL\pN_@]+ = ref .+;|heap<.*>\(out var \S+\);|try \{)$`)

// preludeSeparator returns the C# line of the one blank line that sets the converter's prelude apart from the Go
// body (the owner's decision 2, docs/PLAN-comment-and-spacing-parity.md class E2), or 0 when the body has none.
// The prelude runs without a blank line from the line after the opening brace, every line one preludeLineRe
// matches; one blank line follows it; and the line after that opens the translation of the Go body's first line:
// its comment, or the first line holding its identifiers, with only scaffolding (scaffoldRe), a bare `{` or
// comments between. A blank line inside the prelude, or a second one after it, is not the separator.
func preludeSeparator(cs *csFile, g *goFile, cOpen, cEnd, lb, rb int) int {
	c := cOpen + 1
	for c < cEnd && !isBlankCS(cs, c) {
		if !preludeLineRe.MatchString(strings.TrimSpace(cs.codeText[c-1])) {
			return 0
		}
		c++
	}
	if c == cOpen+1 || c+1 >= cEnd || isBlankCS(cs, c+1) || lb+1 >= rb {
		return 0
	}
	goFirst := strings.TrimSpace(g.src[lb])
	if strings.HasPrefix(goFirst, "//") || strings.HasPrefix(goFirst, "/*") {
		if commentOnly(cs, c+1) {
			return c
		}
		return 0
	}
	ids := goAnchorIdents(g.idents[lb+1])
	for l := c + 1; l < cEnd && !isBlankCS(cs, l); l++ {
		if !cs.codeOnLine[l-1] {
			continue
		}
		ci := csIdents(cs.codeText[l-1])
		all := true
		for k := range ids {
			if !ci[k] {
				all = false
				break
			}
		}
		if all {
			return c
		}
		code := strings.TrimSpace(cs.codeText[l-1])
		if code != "{" && !scaffoldRe.MatchString(code) && !attrRe.MatchString(cs.codeText[l-1]) {
			return 0
		}
	}
	return 0
}

// respelledBuiltins are the Go builtins the translation spells differently, so an anchor never requires them on
// the C# line: `x := make([]T, n)` becomes `var x = new slice<T>(n);` and `p := new(T)` becomes
// `var p = @new<T>();`. (`new` is also a C# keyword, which goIdents already drops; it is listed for what it is.)
var respelledBuiltins = map[string]bool{"make": true, "new": true}

// goAnchorIdents returns the identifiers a C# line must hold to be the translation of a Go line: goIdents less
// respelledBuiltins. preludeSeparator and the placement of a Go blank line both anchor through it.
func goAnchorIdents(ids []string) map[string]bool {
	m := goIdents(ids)
	for name := range respelledBuiltins {
		delete(m, name)
	}
	return m
}

var preludeRe = regexp.MustCompile(`(DerefOrNull\(\);|heap<.*>\(out var \S+\);|= \S+\.Clone\(\);|\.sslice\(\);|= GoZero<.*>\(\);|= nonnil\(ref .*;|= default!;)$`)

var csDeclRe = regexp.MustCompile(`^(\[[^\]]*\]\s*)*(public|internal|private|protected|partial|static|readonly|unsafe|const|ref|file|new)\b`)

func isCSDecl(cs *csFile, ln int) bool {
	return csDeclRe.MatchString(cs.codeText[ln-1]) && !strings.HasPrefix(cs.codeText[ln-1], "partial class ") && !strings.Contains(cs.codeText[ln-1], "_package")
}

// csBodyRange returns the lines of the brace body that opens on or after line ln: its opening and closing lines.
func csBodyRange(cs *csFile, ln int) (int, int, bool) {
	depth := 0
	open := 0
	for l := ln; l <= len(cs.lines) && l < ln+5000; l++ {
		code := cs.codeText[l-1]
		if open == 0 && strings.Contains(code, "=>") && !strings.Contains(code, "{") {
			return 0, 0, false
		}
		for _, ch := range code {
			switch ch {
			case '{':
				if open == 0 {
					open = l
				}
				depth++
			case '}':
				depth--
				if open > 0 && depth == 0 {
					return open, l, true
				}
			}
		}
	}
	return 0, 0, false
}

func csBodyBlanks(cs *csFile, ln int) (int, bool) {
	depth := 0
	started := false
	blanks := 0
	for l := ln; l <= len(cs.lines) && l < ln+5000; l++ {
		code := cs.codeText[l-1]
		if !started {
			if strings.Contains(code, "=>") && !strings.Contains(code, "{") {
				return 0, false
			}
		}
		if started && depth > 0 && strings.TrimSpace(cs.lines[l-1]) == "" {
			blanks++
		}
		for _, ch := range code {
			switch ch {
			case '{':
				depth++
				started = true
			case '}':
				depth--
				if started && depth == 0 {
					return blanks, true
				}
			}
		}
	}
	return 0, false
}

func blankContext(prev, next string) string {
	p, n := strings.TrimSpace(prev), strings.TrimSpace(next)
	scaffold := regexp.MustCompile(`^(namespace |using |partial class |public static partial class |} // end |#region|#endregion|\[assembly|\[module|// <auto-generated|// Hoisted|// Static|// Package|\[GoPackage)`)
	switch {
	case scaffold.MatchString(p) || scaffold.MatchString(n) || p == "}" && strings.HasPrefix(n, "} //"):
		return "scaffold"
	case prev != "" && (prev[0] == ' ' || prev[0] == '\t') && next != "" && (next[0] == ' ' || next[0] == '\t'):
		return "inside-body"
	default:
		return "member-boundary"
	}
}

// ---- the exported surface ----------------------------------------------------------------------

// Pair is one converted production file and the Go source it was converted from.
type Pair struct {
	Pkg    string // package path, e.g. "unicode/utf8"
	Base   string // file name without extension
	CS     string // path of the converted .cs
	GoFile string // path of the Go source
	Flavor string // "" for a flat file, else the GOOS directory it was read from
}

// FindPairs pairs every converted production .cs under core with its Go source under goroot/src, skipping
// hand-owned files ([module: GoManualConversion], *_impl.cs), test files, package_info.cs and golib. A file
// converted per GOOS is read from the flat copy when there is one, else from the windows, linux, darwin flavour
// in that order.
func FindPairs(core, goroot string) []Pair {
	var out []Pair

	for _, p := range findPairs(core, goroot) {
		out = append(out, Pair{Pkg: p.pkg, Base: p.base, CS: p.cs, GoFile: p.gofile, Flavor: p.flavor})
	}

	return out
}

// Results accumulates the measures over one or more analyzed pairs.
type Results struct {
	r *results
}

// NewResults returns an empty accumulator.
func NewResults() *Results {
	return &Results{r: newResults()}
}

// Analyze measures one pair. A Go file that does not parse or a C# file that cannot be read is reported as an
// error and contributes nothing.
func (r *Results) Analyze(p Pair) error {
	return r.AnalyzeFiles(p.GoFile, p.CS, p.Pkg+"/"+p.Base)
}

// AnalyzeFiles measures one Go file against one C# file; where names the pair in findings.
func (r *Results) AnalyzeFiles(goFile, csFile, where string) error {
	if _, err := parseGo(goFile); err != nil {
		return fmt.Errorf("parsing %s: %w", goFile, err)
	}

	if _, err := os.Stat(csFile); err != nil {
		return err
	}

	pkg, base := filepath.ToSlash(filepath.Dir(where)), filepath.Base(where)
	analyze(pair{pkg: pkg, base: base, cs: csFile, gofile: goFile}, r.r, false)

	return nil
}

// Files is the number of pairs analyzed.
func (r *Results) Files() int { return r.r.files }

// Groups is the number of Go comment groups seen, the census's own vacuity check: zero means nothing was read.
func (r *Results) Groups() int {
	n := 0

	for k, v := range r.r.groupsByClass {
		if !strings.HasPrefix(k, " ") {
			n += v
		}
	}

	return n
}

// Counts returns every defect count by class key. A key is present only when its count is non-zero, so an
// empty map is a file or corpus with no finding. The keys:
//
//	missed:<go class>              a Go comment group none of whose lines is in the C#
//	partial:<go class>             a Go comment group only some of whose lines are in the C#
//	misplaced:<go class>/<kind>    a fully present group at another anchor (trailing-to-own-line,
//	                               own-line-to-trailing, anchor-differs)
//	eol:<status>                   an end-of-line run gofmt aligned that is not aligned in the C#
//	                               (misaligned, split, moved-to-own-line, missing)
//	eol:cs-ragged-run              a C# run of consecutive trailing comments with ragged columns
//	blank2:<context>               a run of two or more C# blank lines (member-boundary, inside-body, scaffold)
//	top:<finding>                  top-level separation unlike Go's (go-blank-cs-none, go-none-cs-blank,
//	                               unmapped:<kind>)
//	group:<finding>                separation between the specs of a const or var group unlike Go's
//	body:go-blank-cs-none          a Go blank line inside a function body with no C# blank line
//	body:go-blank-unmapped         a Go body blank line whose element was not found in the C#
//	body:extra:<kind>              a C# body blank line no Go blank line explains
//	body:cs-body-not-found         a Go function body whose C# body was not found
//
// <go class> is where Go puts the comment: file-header, package-doc, decl-doc, spec-doc-in-group, field-doc,
// trailing-spec, trailing-field, in-composite-literal, in-expression, free-in-type, after-open-brace,
// after-case-label, trailing-stmt, before-else, before-case, before-close-brace, free-stmt, free-top-level.
func (r *Results) Counts() map[string]int {
	c := map[string]int{}
	add := func(k string, n int) {
		if n != 0 {
			c[k] += n
		}
	}

	for k, n := range r.r.missedByClass {
		if !strings.HasPrefix(k, " ") {
			add("missed:"+k, n)
		}
	}

	for k, n := range r.r.partialByClass {
		add("partial:"+k, n)
	}

	for k, n := range r.r.misplacedKind {
		add("misplaced:"+strings.Replace(k, " / ", "/", 1), n)
	}

	for k, n := range r.r.eol {
		if s, ok := strings.CutPrefix(k, "go-aligned-run:"); ok && s != "aligned" {
			add("eol:"+s, n)
		}
	}

	add("eol:cs-ragged-run", r.r.csMisaligned)

	for k, n := range r.r.blank2 {
		if k != "lines" {
			add("blank2:"+k, n)
		}
	}

	for k, n := range r.r.topLevel {
		if k != "same" {
			add("top:"+k, n)
		}
	}

	for k, n := range r.r.groupSpec {
		if k != "same" {
			add("group:"+k, n)
		}
	}

	for k, n := range r.r.body {
		switch {
		case k == "go-blank:cs-NONE":
			add("body:go-blank-cs-none", n)
		case k == "go-blank:unmapped":
			add("body:go-blank-unmapped", n)
		case k == "cs-body-not-found" || strings.HasPrefix(k, "extra:"):
			add("body:"+k, n)
		}
	}

	return c
}

// Finding is one located instance behind a count, for a failure message.
type Finding struct {
	Class, Where, Detail string
}

// Findings returns the recorded instances whose census class starts with prefix (the census keeps the first
// 400 of each class), sorted by location.
func (r *Results) Findings(prefix string) []Finding {
	var out []Finding

	for k, list := range r.r.examples {
		if strings.HasPrefix(k, prefix) {
			for _, e := range list {
				out = append(out, Finding{e.class, e.where, e.detail})
			}
		}
	}

	sort.Slice(out, func(i, j int) bool {
		if out[i].Where != out[j].Where {
			return out[i].Where < out[j].Where
		}
		return out[i].Class < out[j].Class
	})

	return out
}
