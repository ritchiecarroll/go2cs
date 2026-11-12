// evalOrderSpill_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards evalOrderSpill.go: where a statement reads a LOCAL variable before a call that can write
// it, the call spills to a temporary ahead of the statement (gc's order), and nowhere else. The
// controls carry as much weight as the hazards: a rule that spilled every call would be correct and
// would rewrite most of the corpus.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

const evalOrderFixture = `package main

import "fmt"

type pair struct{ a, b int32 }

func mutate(p *pair) int32         { p.a = 100; return 2 }
func scaled(p pair, k int32) int32 { return (p.a*1000 + p.b) * k }
func readOnly(p *pair) int32       { return p.a + p.b }
func writeB(p *pair) int32         { p.b = 9; return 1 }
func raise(n *int) int             { *n += 10; return *n }

type counter struct{ n int }

func (c *counter) bump() int { c.n += 10; return 1 }
func (c *counter) peek() int { return c.n }

type holder struct{ p *pair }

func repoint(pp **pair) int32  { *pp = &pair{50, 0}; return 1 }
func (h *holder) touch() int32 { h.p.a = 5; return 1 }

func get(s []byte) byte    { return s[0] }
func put(s []byte, v byte) { s[0] = v }
func elem(p *int) int      { *p = 3; return 1 }

type small struct {
	f int8
	g int
}

func (s *small) poke() int { s.f, s.g = 9, 9; return 1 }

type key struct {
	a int
	s string
}

type node struct{ n int }

func (x *node) id() int              { return x.n }
func mk(name int, b *node) *node     { b.n = name; return b }
func use(o interface{ id() int }) {}

func sum3(a, b, c int) int { return a*100 + b*10 + c }
func fill(s []int) int     { s[0] = 9; return 1 }
func other() int           { return 1 }

func hazards() {
	p := pair{1, 2}
	fmt.Println(scaled(p, mutate(&p)))

	n := int32(1)
	bump := func() int32 { n = 50; return 3 }
	fmt.Println(n + bump())

	t := counter{n: 1}
	fmt.Println(sum3(t.n, t.bump(), t.n))

	k := 1
	setK := func() int { k = 7; return 1 }
	s := []int{k, setK(), k}
	fmt.Println(s)

	ys := []int{0, 0}
	swap := func() int { ys = []int{5, 5}; return 4 }
	ys[1] = swap()

	c := 1
	setC := func() int { c = 7; return 1 }
	c += setC()

	m := 1
	setM := func() int { m = 7; return 1 }
	a, b := m, setM()
	fmt.Println(a, b)

	ch := make(chan int, 1)
	q := 1
	setQ := func() int { q = 7; return 1 }
	ch <- q + setQ()

	pt := &counter{n: 1}
	fmt.Println(pt.n + pt.bump())

	sl := []int{1, 2}
	fmt.Println(sl[0] + fill(sl))

	rp := &pair{1, 2}
	fmt.Println(rp.a + repoint(&rp))

	sa := small{1, 1}
	fmt.Println(sa, sa.poke())

	sw := small{1, 1}
	fmt.Println(sw.g+1, sw.poke())
}

func singleReturn() int {
	v := 1
	setV := func() int { v = 7; return 1 }
	return v + setV()
}

func controls() {
	p2 := pair{1, 2}
	fmt.Println(scaled(p2, readOnly(&p2)))

	p3 := pair{1, 2}
	fmt.Println(p3.a + writeB(&p3))

	t2 := counter{n: 1}
	fmt.Println(t2.n + t2.peek())

	x := 1
	fmt.Println(other() + x)

	h := &holder{p: &pair{1, 2}}
	fmt.Println(h.p == nil, h.touch())

	buf := []int{1, 2, 3}
	buf = buf[:fill(buf)]

	ctr := []byte{1}
	put(ctr, get(ctr)+1)

	tt := []int{1, 2, 3}
	tt[2] = elem(&tt[0])
	_ = tt[1] // a blank target has no type: the in-place marks must not read one

	w, ok := 1, true
	setW := func() int { w = 7; return 1 }
	fmt.Println(ok && w+setW() > 0)

	se := small{1, 1}
	fmt.Println(se.f+1, se.poke())

	sk := small{1, 1}
	mk := map[key]int{{1, "a"}: 10}
	fmt.Println(mk[key{sk.g, "a"}] + sk.poke())

	sc := small{1, 1}
	setSc := func() int { sc.f = 9; return 1 }
	fmt.Println(sc, setSc())

	kc := key{1, "a"}
	setKc := func() int { kc.a = 9; return 1 }
	fmt.Println(mk[kc] + setKc())

	nd := &node{1}
	use(mk2(nd.n, nd))
}

func mk2(name int, b *node) *node { return mk(name, b) }

// A type parameter has no size, so the in-place tests must not ask for one.
func generic[T any, K comparable](v T, m map[K]int, k K) {
	s := small{1, 1}
	fmt.Println(v, struct{ x T }{v}, s.poke())
	fmt.Println(m[k] + s.poke())
}

func multiReturn() (int, int) {
	n := 0
	return n, raise(&n)
}

func main() {
	hazards()
	fmt.Println(singleReturn())
	controls()
	fmt.Println(multiReturn())
	generic(1, map[string]int{"a": 1}, "a")
}
`

func convertEvalOrderFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/evalorder\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), evalOrderFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "evalorder", "main.cs"))
}

// TestStatementSpillsCallsThatCanWriteAnEarlierLocalRead pins the order property and its scope.
// Proven against seven neuters, each restored byte-identically:
//
//   - evalOrderOpWrites forced to false: all 14 hazards report their call inline, so the positives
//     are not passing by accident;
//   - the one-level callee read skipped (any conflicting argument counts as writing): exactly the
//     readOnly, writeB and peek controls report a spill, so the callee read is what keeps them;
//   - returnOperandOrder.go's symmetric pathsConflict in place of eoStepsConflict: exactly the
//     repointed-pointer hazard reports its call inline;
//   - eoCopiedInPlace forced to false: exactly the four copied-in-place controls report a spill, so
//     gc's addrTemp copy (evalOrderInPlace.go) is what keeps them;
//   - eoAddressTaken forced to false: exactly the address-taken iface hazard reports its call inline;
//   - eoConversionNeedsAddress forced to true: exactly the machine-word iface hazard does;
//   - addOp keeping an operation in its own enclosing list: exactly the control whose call is itself
//     an interface argument reports a spill.
func TestStatementSpillsCallsThatCanWriteAnEarlierLocalRead(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertEvalOrderFixture(t)

	temp := `ᴛ\d+`

	// Each hazard declares its call in a temporary AHEAD of the statement, and the statement names
	// the temporary where Go wrote the call.
	hazards := []struct {
		name, spill, use string
	}{
		{"args-addr", `var ` + temp + ` = mutate\(ref p\);`, `scaled\(p, ` + temp + `\)`},
		{"binary-closure", `var ` + temp + ` = bump\(\);`, `n \+ ` + temp},
		{"args-method", `var ` + temp + ` = t\.bump\(\);`, `sum3\(t\.n, ` + temp + `, t\.n\)`},
		{"complit", `var ` + temp + ` = setK\(\);`, `\[k, ` + temp + `, k\]`},
		{"lhs-index", `var ` + temp + ` = swap\(\);`, `ys\[1\] = ` + temp + `;`},
		{"compound", `var ` + temp + ` = setC\(\);`, `c \+= ` + temp + `;`},
		{"multi-assign", `var ` + temp + ` = setM\(\);`, `b = ` + temp + `;`},
		{"send", `var ` + temp + ` = setQ\(\);`, `q \+ ` + temp},
		{"ptr-recv", `var ` + temp + ` = pt\.bump\(\);`, `pt\.n \+ ` + temp},
		{"slice-arg", `var ` + temp + ` = fill\(sl\);`, `sl\[0\] \+ ` + temp},
		{"repointed pointer", `var ` + temp + ` = repoint\(ref rp\);`, `rp\.a \+ ` + temp},
		{"single-return", `var ` + temp + ` = setV\(\);`, `return v \+ ` + temp + `;`},
		{"iface operand, address taken", `var ` + temp + ` = sa\.poke\(\);`, `Println\(sa, ` + temp + `\)`},
		{"iface operand, a machine word", `var ` + temp + ` = sw\.poke\(\);`, `sw\.g \+ 1, ` + temp},
	}

	for _, hazard := range hazards {
		if !regexp.MustCompile(hazard.spill).MatchString(mainCs) || !regexp.MustCompile(hazard.use).MatchString(mainCs) {
			t.Errorf("%s: expected the call spilled ahead of the statement (%s, then %s)", hazard.name, hazard.spill, hazard.use)
		}
	}

	// Each control keeps its call where Go wrote it.
	controls := []struct {
		name, inline string
	}{
		{"read-only callee", `scaled(p2, readOnly(ref p2))`},
		{"callee writes another field", `p3.a + writeB(ref p3)`},
		{"value-reading method", `t2.n + t2.peek()`},
		{"read after the call", `other() + x`},
		{"callee writes only through the pointer read", `h.touch()`},
		{"slice header read beside an element write", `buf.slice(0, fill(buf))`},
		{"slice header passed beside an element read", `put(ctr, (byte)(get(ctr) + 1))`},
		{"slice target beside an element address", `tt[2] = elem(`},
		{"blank assignment target", `_ = tt[1];`},
		{"hazard inside && right side (step 3)", `setW()`},
		{"iface operand gc copies in place: not addressable", `(int8)(se.f + 1), se.poke())`},
		{"slow map key gc copies in place: not addressable", `mk[new key(sk.g, "a")] + sk.poke())`},
		{"iface operand gc copies in place: closure-only local", `Println(sc, setSc())`},
		{"slow map key gc copies in place: closure-only local", `mk[kc] + setKc())`},
		{"a call copied in place is not nested in its own arguments", `use(new nodeжuse_o(mk2(nd.n, nd)));`},
	}

	for _, control := range controls {
		if !strings.Contains(mainCs, control.inline) {
			t.Errorf("%s: expected the call inline (%s)", control.name, control.inline)
		}
	}

	if regexp.MustCompile(`var ` + temp + ` = h\.touch\(\);`).MatchString(mainCs) {
		t.Errorf("callee writes only through the pointer read: the pointer value cannot change, no spill")
	}

	if regexp.MustCompile(`var ` + temp + ` = setW\(\);`).MatchString(mainCs) {
		t.Errorf("hazard inside && right side: a conditional call must not move ahead of the statement")
	}

	// A multi-value return keeps its own spill (returnOperandOrder.go), exactly once.
	if got := strings.Count(mainCs, "raise(ref n)"); got != 1 {
		t.Errorf("multi-value return: raise(ref n) emitted %d times, want 1", got)
	}
}
