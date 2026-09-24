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

	// the population predicates: a literal reaches each parameter once; logf's is in format position
	if p := get("logf"); p.litFmt != 1 || p.lit != 0 {
		t.Errorf("logf: want 1 format-position literal site, got lit=%d fmt=%d", p.lit, p.litFmt)
	}
	if p := get("safeLen"); p.lit+p.litLeak != 1 {
		t.Errorf("safeLen: want 1 literal site, got lit=%d leak=%d", p.lit, p.litLeak)
	}
}
