// stackAppendHints_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"go/types"
	"go/version"
	"os/exec"
	"path/filepath"
	"runtime"
	"sort"
	"strings"
	"testing"
)

// TestStackAppendVerdictsJoinEveryPathForm: -m prints a position as the compile that FIRST produced it saw the path,
// and cmd/go replays a cached compile's output verbatim, so one site can arrive import-path relative (-trimpath),
// relative to some earlier working directory, or absolute. Every form joins on base:line:col. RED with the full
// path kept in stackAppendPositionKey: only the bare form joined the site's key, and the census's replayed
// `strings/...` leg read as a target disagreement.
func TestStackAppendVerdictsJoinEveryPathForm(t *testing.T) {
	for _, path := range []string{
		"example.com/sa/sa.go", // -trimpath
		"./sa.go",              // the package directory
		"sa.go",
		"sa/sa.go",                        // replayed, relative to a parent directory's compile
		"/tmp/build/example.com/sa/sa.go", // absolute
	} {
		verdicts := parseAppendVerdicts(path + ":9:12: append does not escape\n" +
			path + ":30:9: inlining call to sa.callee\n" +
			"variablemake triggered " + path + ":8:6 0101\n" +
			path + ":8:6 [bisect-match 0x01]\n")

		if got := verdicts.direct["sa.go:9:12"]; len(got) != 1 || got[0] {
			t.Errorf("%s: direct[sa.go:9:12] = %v, want one non-escaping report", path, got)
		}

		if got := verdicts.inlinedAt["sa.go:30:9"]; len(got) != 1 || got[0] != "sa.callee" {
			t.Errorf("%s: inlinedAt[sa.go:30:9] = %v, want [sa.callee]", path, got)
		}

		if got := verdicts.hashCount["sa.go:8:6"]; got != 1 {
			t.Errorf("%s: hashCount[sa.go:8:6] = %d, want 1 (the bisect-match line is not a log line)", path, got)
		}
	}
}

// TestStackAppendVerdictsCountOnlyThePlainDeclarationLog: a backing-store log line whose position carries an inline
// chain ("<call>; <declaration>") is an inlined copy's, not the declaration's own, so only plain lines count.
func TestStackAppendVerdictsCountOnlyThePlainDeclarationLog(t *testing.T) {
	verdicts := parseAppendVerdicts("" +
		"variablemake triggered example.com/sa/sa.go:8:6 0101\r\n" +
		"variablemake triggered example.com/sa/sa.go:30:9; example.com/sa/sa.go:8:6 0110\r\n" +
		"variablemake triggered example.com/sa/sa.go:8:6 0101\r\n")

	if got := verdicts.hashCount["sa.go:8:6"]; got != 2 {
		t.Errorf("hashCount[sa.go:8:6] = %d, want 2", got)
	}

	if got := verdicts.hashCount["sa.go:30:9"]; got != 0 {
		t.Errorf("hashCount[sa.go:30:9] = %d, want 0 (a chained line names no declaration of its own)", got)
	}
}

// stackAppendSelectionFixture names each append with a trailing comment; the selection tests address sites by it.
const stackAppendSelectionFixture = `package sa

var global []int64

type wide struct{ a, b, c, d, e int64 }

func first() {
	var x []int64
	x = append(x, 1) // x1
	x = append(x, 2) // x2
	_ = x
}

func candidates(src []int64) {
	global = append(global, 1) // package
	var w []wide
	w = append(w, wide{}) // wide
	var y []int64
	y = append(y, src...) // spread
	var z []int64
	f := func() {
		z = append(z, 1) // outer
		var l []int64
		l = append(l, 1) // local
		_ = l
	}
	f()
	_, _, _ = w, y, z
}

func callee(s []int64) []int64 {
	return append(s, 1) // callee
}

func twoAppends(s []int64) []int64 {
	s = append(s, 1)    // two1
	return append(s, 2) // two2
}

func closure() {
	g := func() {
		var c []int64
		c = append(c, 1) // closure
		_ = c
	}
	g()
}
`

// stackAppendFixtureCheck parses and type-checks the selection fixture.
func stackAppendFixtureCheck(t *testing.T) (*token.FileSet, *ast.File, *types.Package, *types.Info) {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "sa.go", stackAppendSelectionFixture, parser.ParseComments)

	if err != nil {
		t.Fatalf("parse: %v", err)
	}

	info := &types.Info{
		Types: map[ast.Expr]types.TypeAndValue{},
		Defs:  map[*ast.Ident]types.Object{},
		Uses:  map[*ast.Ident]types.Object{},
	}

	pkg, err := (&types.Config{}).Check("example.com/sa", fset, []*ast.File{file}, info)

	if err != nil {
		t.Fatalf("type-check: %v", err)
	}

	return fset, file, pkg, info
}

// stackAppendFixtureSites returns the selection fixture's append sites, and each by its comment.
func stackAppendFixtureSites(t *testing.T) ([]*appendSite, map[string]*appendSite) {
	t.Helper()

	fset, file, _, info := stackAppendFixtureCheck(t)
	sites := collectAppendSites([]FileEntry{{file: file, filePath: "sa.go"}}, fset, info)
	named := map[string]*appendSite{}

	for _, group := range file.Comments {
		for _, comment := range group.List {
			line := fset.Position(comment.Pos()).Line

			for _, site := range sites {
				if fset.Position(site.call.Lparen).Line == line {
					named[strings.TrimSpace(strings.TrimPrefix(comment.Text, "//"))] = site
				}
			}
		}
	}

	if len(named) != len(sites) {
		t.Fatalf("named %d of %d append sites", len(named), len(sites))
	}

	return sites, named
}

// TestStackAppendCandidates: a candidate appends at least one value of 1..32 bytes, with no spread, to a variable
// of the innermost function. RED without the innermost-frame check: the closure's append to the outer z, which Go
// lowers in place because the closure captures z by reference, was a candidate.
func TestStackAppendCandidates(t *testing.T) {
	_, named := stackAppendFixtureSites(t)

	for name, want := range map[string]bool{
		"x1": true, "x2": true, "local": true, "callee": true, "two1": true, "two2": true, "closure": true,
		"package": false, // a package variable
		"wide":    false, // a 40-byte element
		"spread":  false, // append(y, src...)
		"outer":   false, // a variable of the enclosing function, appended inside a function literal
	} {
		if got := named[name].candidate; got != want {
			t.Errorf("%s: candidate = %v, want %v", name, got, want)
		}
	}

	if _, ok := named["local"].frame.(*ast.FuncLit); !ok {
		t.Errorf("local: frame is %T, want the function literal", named["local"].frame)
	}
}

// stackAppendTarget is one target's -m output, built from its parts.
type stackAppendTarget struct {
	qualify  []string            // sites whose own copy does not escape and whose backing-store check ran
	escape   []string            // sites whose own copy escapes
	unlogged []string            // sites whose own copy does not escape and whose check did not run
	inlined  map[string][]string // a call position -> the callees inlined there
	copies   map[string][]bool   // a call position -> its inlined copies' reports, true when one escapes
}

func (target stackAppendTarget) verdicts(named map[string]*appendSite) *appendVerdicts {
	verdicts := &appendVerdicts{direct: map[string][]bool{}, inlinedAt: target.inlined, hashCount: map[string]int{}}

	if verdicts.inlinedAt == nil {
		verdicts.inlinedAt = map[string][]string{}
	}

	report := func(names []string, escapes bool, logs int) {
		for _, name := range names {
			site := named[name]
			verdicts.direct[site.key] = append(verdicts.direct[site.key], escapes)
			verdicts.hashCount[site.declKey] = logs
		}
	}

	report(target.qualify, false, 2)
	report(target.escape, true, 2)
	report(target.unlogged, false, 1)

	for position, reports := range target.copies {
		verdicts.direct[position] = append(verdicts.direct[position], reports...)
	}

	return verdicts
}

// TestStackAppendSelection: per variable of a function, the first qualifying site, when every target that has a
// first site agrees on it and no target refuses it there; an inlined copy is attributed only when it can be.
func TestStackAppendSelection(t *testing.T) {
	everyQualifying := []string{"x1", "x2", "local", "callee", "two1", "two2", "closure"}
	const call = "sa.go:99:5" // a call position that is no append site: where -m reports an inlined copy

	for _, test := range []struct {
		name    string
		targets []stackAppendTarget
		want    []string
	}{
		{"every target agrees: each variable's first site", []stackAppendTarget{{qualify: everyQualifying}, {qualify: everyQualifying}, {qualify: everyQualifying}},
			[]string{"x1", "local", "callee", "two1", "closure"}},
		{"a target that does not report a site does not contradict it", []stackAppendTarget{{qualify: []string{"x1", "x2"}}, {qualify: []string{"local"}}},
			[]string{"x1", "local"}},
		{"a target whose copy escapes refuses the site", []stackAppendTarget{{qualify: []string{"x1", "x2"}}, {escape: []string{"x1"}, qualify: []string{"x2"}}},
			nil},
		{"a declaration the backing-store check never logged refuses the site", []stackAppendTarget{{unlogged: []string{"x1"}}},
			nil},
		{"targets that disagree on the first site refuse the variable", []stackAppendTarget{{qualify: []string{"x1", "x2"}}, {qualify: []string{"x2"}}},
			nil},
		{"an inlined copy of a one-append callee that does not escape agrees", []stackAppendTarget{{qualify: []string{"callee"}, inlined: map[string][]string{call: {"callee"}}, copies: map[string][]bool{call: {false}}}},
			[]string{"callee"}},
		{"an inlined copy that escapes refuses the callee's site", []stackAppendTarget{{qualify: []string{"callee"}, inlined: map[string][]string{call: {"callee"}}, copies: map[string][]bool{call: {true}}}},
			nil},
		{"a callee named with its package is this package's", []stackAppendTarget{{qualify: []string{"callee"}, inlined: map[string][]string{call: {"sa.callee"}}, copies: map[string][]bool{call: {true}}}},
			nil},
		{"a copy of a two-append callee cannot be attributed", []stackAppendTarget{{qualify: []string{"two1", "two2"}, inlined: map[string][]string{call: {"twoAppends"}}, copies: map[string][]bool{call: {false}}}},
			nil},
		{"an inlined function literal is its owner's closure", []stackAppendTarget{{qualify: []string{"closure"}, inlined: map[string][]string{call: {"closure.func1"}}, copies: map[string][]bool{call: {true}}}},
			nil},
		{"an inlined function literal's agreeing copy", []stackAppendTarget{{qualify: []string{"closure"}, inlined: map[string][]string{call: {"closure.func1"}}, copies: map[string][]bool{call: {false}}}},
			[]string{"closure"}},
		{"an inlined owner carries its function literal's append", []stackAppendTarget{{qualify: []string{"closure"}, inlined: map[string][]string{call: {"closure"}}, copies: map[string][]bool{call: {true}}}},
			nil},
	} {
		t.Run(test.name, func(t *testing.T) {
			sites, named := stackAppendFixtureSites(t)
			var perTarget []map[*appendSite]stackAppendStatus

			for _, target := range test.targets {
				perTarget = append(perTarget, stackAppendStatuses(sites, "sa", target.verdicts(named)))
			}

			var got []string

			for _, site := range selectStackAppendSites(sites, perTarget) {
				for name, candidate := range named {
					if candidate == site {
						got = append(got, name)
					}
				}
			}

			want := append([]string(nil), test.want...)
			sort.Strings(got)
			sort.Strings(want)

			if strings.Join(got, ",") != strings.Join(want, ",") {
				t.Errorf("selected %v, want %v", got, want)
			}
		})
	}
}

// TestStackAppendHarvestRunsOncePerKey: a multi-target conversion converts each package once per target, and every
// pass reads the same three harvests, so each runs once per process. RED with collectStackAppendHints calling
// stackAppendHarvest directly: three passes ran nine harvests.
func TestStackAppendHarvestRunsOncePerKey(t *testing.T) {
	savedHarvest, savedResults := stackAppendHarvest, stackAppendHarvests
	t.Cleanup(func() {
		stackAppendHarvest, stackAppendHarvests = savedHarvest, savedResults
		packageStackAppendSites, packageStackAppendBuffers = nil, nil
	})

	stackAppendHarvests = map[string]*stackAppendHarvestResult{}
	runs := map[string]int{}

	stackAppendHarvest = func(dir string, tests bool, target string, options Options) string {
		runs[target]++
		return ""
	}

	fset, file, pkg, info := stackAppendFixtureCheck(t)
	files := []FileEntry{{file: file, filePath: "/work/sa/sa.go"}}

	for _, target := range stackAppendHarvestTargets {
		collectStackAppendHints(files, fset, pkg, info, "/work/sa", Options{targetPlatform: target})
	}

	for _, target := range stackAppendHarvestTargets {
		if runs[target] != 1 {
			t.Errorf("%s: harvested %d times over three passes, want 1", target, runs[target])
		}
	}

	// A different build-tag set is a different build, so it harvests again.
	collectStackAppendHints(files, fset, pkg, info, "/work/sa", Options{targetPlatform: "linux/amd64", buildTags: []string{"purego"}})

	if runs["linux/amd64"] != 2 {
		t.Errorf("linux/amd64: harvested %d times after a -tags pass, want 2", runs["linux/amd64"])
	}
}

// TestStackAppendHarvestKeyHoldsEveryCompilerInput: two harvests that differ only in one input that changes the
// compiler's inlining or escape answers never share a cache entry. RED with the inherited variables left out of the
// key: the GOAMD64, GOEXPERIMENT and GOFIPS140 arms each shared the base key.
func TestStackAppendHarvestKeyHoldsEveryCompilerInput(t *testing.T) {
	options := Options{goRoot: "/toolchain"}
	base := stackAppendHarvestKey("/work/sa", false, "linux/amd64", options)

	if again := stackAppendHarvestKey("/work/sa", false, "linux/amd64", options); again != base {
		t.Fatalf("the same harvest keyed differently: %q, %q", base, again)
	}

	differs := map[string]func(t *testing.T) string{
		"the directory": func(t *testing.T) string { return stackAppendHarvestKey("/work/other", false, "linux/amd64", options) },
		"tests":         func(t *testing.T) string { return stackAppendHarvestKey("/work/sa", true, "linux/amd64", options) },
		"GOOS":          func(t *testing.T) string { return stackAppendHarvestKey("/work/sa", false, "darwin/amd64", options) },
		"GOARCH":        func(t *testing.T) string { return stackAppendHarvestKey("/work/sa", false, "linux/arm64", options) },
		"the toolchain": func(t *testing.T) string {
			return stackAppendHarvestKey("/work/sa", false, "linux/amd64", Options{goRoot: "/other"})
		},
		"the build tags": func(t *testing.T) string {
			return stackAppendHarvestKey("/work/sa", false, "linux/amd64", Options{goRoot: "/toolchain", buildTags: []string{"purego"}})
		},
		"GOAMD64": func(t *testing.T) string {
			t.Setenv("GOAMD64", "v3")
			return stackAppendHarvestKey("/work/sa", false, "linux/amd64", options)
		},
		"GOEXPERIMENT": func(t *testing.T) string {
			t.Setenv("GOEXPERIMENT", "nogreenteagc")
			return stackAppendHarvestKey("/work/sa", false, "linux/amd64", options)
		},
		"GOFIPS140": func(t *testing.T) string {
			t.Setenv("GOFIPS140", "latest")
			return stackAppendHarvestKey("/work/sa", false, "linux/amd64", options)
		},
	}

	for name, key := range differs {
		t.Run(name, func(t *testing.T) {
			if key(t) == base {
				t.Errorf("a harvest differing only in %s shares the key", name)
			}
		})
	}

	command := strings.Join(newStackAppendHarvestCommand(false, "linux/amd64", options).env, " ")

	for _, want := range []string{"GOOS=linux", "GOARCH=amd64", "CGO_ENABLED=0"} {
		if !strings.Contains(command, want) {
			t.Errorf("the harvest's environment %q lacks %s, which its key reads from it", command, want)
		}
	}
}

const stackAppendEmissionFixture = `package sappend

//go:noinline
func loop() int {
	var x []int64
	for i := 0; i < 5; i++ {
		x = append(x, int64(i))
	}
	x = append(x, 5)
	return cap(x)
}

//go:noinline
func keep(p *[]int64) int { return len(*p) }

//go:noinline
func addressTaken() int {
	var a []int64
	a = append(a, 1)
	return keep(&a)
}

//go:noinline
func keyword() int {
	var out []int64
	out = append(out, 1)
	return cap(out)
}

//go:noinline
func run(f func() int) int { return f() }

//go:noinline
func siblings(n int) int {
	switch n {
	case 0:
		var buf []int64
		buf = append(buf, 1)
		return cap(buf)
	}
	switch n {
	case 1:
		var buf []int64
		buf = append(buf, 2)
		return cap(buf)
	}
	return 0
}

//go:noinline
func literalName() int {
	{
		var tmp []int64
		tmp = append(tmp, 1)
		_ = tmp
	}
	return run(func() int {
		var tmp []int64
		tmp = append(tmp, 1)
		return cap(tmp)
	})
}

//go:noinline
func closures() int {
	var outer []int64
	return run(func() int {
		var inner []int64
		inner = append(inner, 1)
		outer = append(outer, 1)
		return cap(inner) + cap(outer)
	})
}
`

// TestStackAppendEmission converts a module with the real harvest: the buffer at the entry of the function that
// appends, the first qualifying site only, a function literal's own variable in the literal's body, and no buffer
// for an address-taken variable or an outer variable appended inside a literal; a C# keyword's buffer drops its @.
// RED before the converter read the harvest: no buffer anywhere, so x's first append grew 1 2 4 4 8 where Go's reads
// 4 4 4 4 8; with the @ kept, out's buffer read @outᴷ; and before buffers were named apart, siblings declared bufᴷ twice
// at its entry (CS0128, strconv's unquote). literalName's lambda keeps tmpᴷ: it shadows the entry's, which C# allows.
func TestStackAppendEmission(t *testing.T) {
	isolateConverterPathGlobals(t)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	// The harvest reads -d=variablemakehash, which Go 1.25 added; the converter module's own go directive is newer.
	output, err := exec.Command(filepath.Join(goRoot, "bin", "go"), "env", "GOVERSION").Output()

	if goVersion := strings.TrimSpace(string(output)); err != nil || version.Compare(goVersion, "go1.25") < 0 {
		t.Fatalf("the harvest needs Go 1.25 or later at %s: %q, %v", goRoot, goVersion, err)
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/sappend\n\ngo 1.25\n")
	writeModuleFile(t, filepath.Join(appDir, "sappend.go"), stackAppendEmissionFixture)

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "sappend", "sappend.cs")), "\r\n", "\n")

	for _, want := range []string{
		"    var xᴷ = new stackAppend<int64>();\n",
		"x = append(xᴷ, x, (int64)i);",
		"x = append(x, (int64)(5));",
		"        var innerᴷ = new stackAppend<int64>();\n",
		"inner = append(innerᴷ, inner, (int64)(1));",
		"    var outᴷ = new stackAppend<int64>();\n",
		"@out = append(outᴷ, @out, (int64)(1));",
		"    var bufᴷ = new stackAppend<int64>();\n",
		"    var bufᴷΔ1 = new stackAppend<int64>();\n",
		"buf = append(bufᴷ, buf, (int64)(1));",
		"buf = append(bufᴷΔ1, buf, (int64)(2));",
		"        var tmpᴷ = new stackAppend<int64>();\n",
	} {
		if strings.Count(emitted, want) != 1 {
			t.Errorf("want exactly one %q in:\n%s", want, emitted)
		}
	}

	if strings.Count(emitted, "var tmpᴷ = new stackAppend<int64>();") != 2 || strings.Count(emitted, "tmp = append(tmpᴷ, tmp, (int64)(1));") != 2 {
		t.Errorf("want literalName's entry and its lambda each to declare and pass their own tmpᴷ:\n%s", emitted)
	}

	for _, unwanted := range []string{"aᴷ", "outerᴷ", "@outᴷ", "tmpᴷΔ1"} {
		if strings.Contains(emitted, unwanted) {
			t.Errorf("unwanted %q in:\n%s", unwanted, emitted)
		}
	}

	if lambda, buffer := strings.Index(emitted, "run(() =>"), strings.Index(emitted, "var innerᴷ"); lambda < 0 || buffer < lambda {
		t.Errorf("innerᴷ is declared at %d, want inside the literal's body (after %d):\n%s", buffer, lambda, emitted)
	}
}
