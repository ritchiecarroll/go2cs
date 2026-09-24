package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// TestClassesBothWays runs the census over testdata/fixture, with the fixture's own -gcflags=-m, and
// checks every class both ways: each FAIL function carries its class and does not survive, each SURVIVE
// function carries no disqualifying class and survives, and each GAP function fails today and survives
// with the gaps closed.
func TestClassesBothWays(t *testing.T) {
	dir := t.TempDir()
	mFile := filepath.Join(dir, "m.txt")
	cmd := exec.Command("go", "build", "-gcflags=-m", "-o", os.DevNull, "./testdata/fixture")
	out, err := cmd.CombinedOutput()
	if err != nil {
		t.Fatalf("go build -gcflags=-m: %v\n%s", err, out)
	}
	if err := os.WriteFile(mFile, out, 0o644); err != nil {
		t.Fatal(err)
	}
	c, err := run(mFile, "", []string{"./testdata/fixture"})
	if err != nil {
		t.Fatal(err)
	}
	byName := map[string]*param{}
	for _, p := range c.params {
		name := p.fn[strings.LastIndex(p.fn, ".")+1:]
		if p.idx == 0 {
			byName[name] = p
		}
	}
	get := func(name string) *param {
		p := byName[name]
		if p == nil {
			t.Fatalf("%s: no string parameter found", name)
		}
		return p
	}

	for _, name := range []string{"safeLen", "safeIndex", "safeCompare", "safeSwitch", "safeAlias", "safeConcat",
		"safeBytes", "safeDiscard", "onwardOK", "onwardChain", "logf"} {
		p := get(name)
		if !p.survive[0] || !p.survive[1] {
			t.Errorf("%s: want SURVIVE both ways, got today=%v gaps-closed=%v (verdict %s, reasons %v)", name, p.survive[0], p.survive[1], p.verdict, p.reasons(1, modes[1]))
		}
	}

	fails := map[string]string{
		"boxes": cBoxIface, "storeField": cStoreField, "storeElem": cStoreElem, "storeComposite": cStoreComp,
		"storeGlobal": cStoreGlobal, "captures": cCapture, "deferred": cDeferGo, "mapKey": cMapKey,
		"returns": cReturn, "addr": cAddr, "callsGeneric": cGeneric, "variadic": cVariadic, "fixedSig": cFixedSig,
		"namedConv": cNamedConv, "onwardBad": cCascade, "valueUsed": cFuncValue, "m": cIfaceMatch, "Exported": cReflect,
	}
	for name, class := range fails {
		p := get(name)
		rs := p.reasons(1, modes[1])
		has := false
		for _, r := range rs {
			if r == class {
				has = true
			}
		}
		if !has || p.survive[1] {
			t.Errorf("%s: want FAIL with %q, got survive=%v reasons %v", name, class, p.survive[1], rs)
		}
	}

	for name, class := range map[string]string{"ranges": gRange, "copies": gCopy, "spreads": gSpread} {
		p := get(name)
		if p.survive[0] || !p.survive[1] || p.classes[class] == 0 {
			t.Errorf("%s: want GAP %q (fail today, survive with gaps closed), got today=%v gaps-closed=%v classes %v verdict %s", name, class, p.survive[0], p.survive[1], p.classes, p.verdict)
		}
	}

	// the adapter mode lifts exactly the func-value classes: valueUsed fails at S1 and survives at S2
	if p := get("valueUsed"); p.survive[1] || !p.survive[2] {
		t.Errorf("valueUsed: want fail at S1 and survive at S2, got S1=%v S2=%v", p.survive[1], p.survive[2])
	}
	if p := get("testOnly"); p.classes[cFuncValueT] == 0 || p.classes[cFuncValue] != 0 || p.survive[1] || !p.survive[2] {
		t.Errorf("testOnly: want the test-only func-value class, fail at S1, survive at S2; got classes %v S1=%v S2=%v", p.classes, p.survive[1], p.survive[2])
	}
	if p := get("boxes"); p.survive[2] {
		t.Errorf("boxes: the adapter mode must not lift a box")
	}

	// the defer/go callee class: deferSink is called only by a defer statement
	if p := get("deferSink"); p.classes[cDeferCall] == 0 || p.survive[2] || !p.survive[3] {
		t.Errorf("deferSink: want the defer/go callee class, fail at S2, survive at S3 (twin); got classes %v S2=%v S3=%v", p.classes, p.survive[2], p.survive[3])
	}
	if p := get("safeLen"); p.classes[cDeferCall] != 0 {
		t.Errorf("safeLen: not a defer callee, got classes %v", p.classes)
	}

	// the twin shape lifts every signature class but a hand-own or a missing body, and no use class
	for _, name := range []string{"valueUsed", "testOnly", "m", "Exported", "deferSink"} {
		if p := get(name); !p.survive[3] {
			t.Errorf("%s: want SURVIVE at S3 (twin), reasons %v", name, p.reasons(3, modes[3]))
		}
	}
	for _, name := range []string{"boxes", "captures", "storeField", "returns", "fixedSig", "onwardBad"} {
		if p := get(name); p.survive[3] {
			t.Errorf("%s: a use class must still fail at S3 (twin)", name)
		}
	}

	// the Tier C hoist predicate: seven literal sites, one hoisted
	if p := get("hoistSink"); p.lit != 7 || p.litHoist != 1 {
		t.Errorf("hoistSink: want 7 literal sites with 1 hoisted, got lit=%d hoisted=%d", p.lit, p.litHoist)
	}

	// the population predicates: a literal reaches each parameter once; logf's is in format position
	if p := get("logf"); p.litFmt != 1 || p.lit != 0 {
		t.Errorf("logf: want 1 format-position literal site, got lit=%d fmt=%d", p.lit, p.litFmt)
	}
	if p := get("safeLen"); p.lit+p.litLeak != 1 {
		t.Errorf("safeLen: want 1 literal site, got lit=%d leak=%d", p.lit, p.litLeak)
	}
}

// TestHandOwnAtFunctionLevel controls the hand-own predicate both ways on a synthetic core: a placeholder
// line hand-owns only the function it names, and only an attribute LINE hand-owns the whole file.
func TestHandOwnAtFunctionLevel(t *testing.T) {
	core := t.TempDir()
	must := func(err error) {
		if err != nil {
			t.Fatal(err)
		}
	}
	must(os.MkdirAll(filepath.Join(core, "p"), 0o755))
	must(os.WriteFile(filepath.Join(core, "p", "a.cs"), []byte("namespace go;\n"+funcPlaceholderLead+"throwy is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])\n"), 0o644))
	must(os.WriteFile(filepath.Join(core, "p", "b.cs"), []byte("[module: GoManualConversion]\nnamespace go;\n"), 0o644))
	must(os.WriteFile(filepath.Join(core, "p", "d.cs"), []byte("using go;\n[module: go.GoManualConversion]\nnamespace go;\n"), 0o644))
	c := &census{core: core, goos: "linux", csCache: map[string]string{}}
	cases := []struct {
		file, name string
		want       bool
	}{
		{"x/a.go", "throwy", true},   // its own placeholder
		{"x/a.go", "other", false},   // a placeholder elsewhere in the file (the over-match this fixes)
		{"x/b.go", "anything", true}, // a whole-file hand-own
		{"x/d.go", "anything", true}, // the namespace-qualified spelling
		{"x/c.go", "missing", false}, // no go2cs file
	}
	for _, k := range cases {
		if got := c.funcHandOwn(k.file, "p", k.name); got != k.want {
			t.Errorf("funcHandOwn(%s, %s) = %v, want %v", k.file, k.name, got, k.want)
		}
	}
	if c.wholeFileHandOwn("x/a.go", "p") {
		t.Errorf("a placeholder comment must not make a whole-file hand-own")
	}
	if v, ok := decodeCSharp(`"a\x41\u00e9\n"`); !ok || v != "aA\u00e9\n" {
		t.Errorf("decodeCSharp: got %q", v)
	}
}
