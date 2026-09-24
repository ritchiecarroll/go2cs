// Package fixture controls every class of the O1 survival census both ways: each function below takes
// one string parameter s, and main_test.go states what the census must say about it.
package fixture

var global string

type T struct{ f string }

type Named string

type I interface{ M(s string) int }

type unexpIface interface{ m(s string) int }

// ---- expected to SURVIVE ----

func safeLen(s string) int      { return len(s) }
func safeIndex(s string) byte   { return s[0] }
func safeCompare(s string) bool { return s == "x" || s < "y" }
func safeSwitch(s string) int {
	switch s {
	case "a":
		return 1
	}
	return 0
}
func safeAlias(s string) int   { t := s[1:]; return len(t) }
func safeConcat(s string) int  { return len(s + "x") }
func safeBytes(s string) int   { return len([]byte(s)) }
func safeDiscard(s string) int { _ = s; return 0 }
func onwardOK(s string) int    { return safeLen(s) }
func onwardChain(s string) int { return onwardOK(s[1:]) }
func logf(format string, a ...any) int {
	return len(format) + len(a)
}

// ---- expected to FAIL, one class each ----

func boxes(s string) int                        { var x any = s; return lenAny(x) }
func lenAny(x any) int                          { return 0 }
func storeField(s string) int                   { var t T; t.f = s; return len(t.f) }
func storeElem(s string) int                    { var a [2]string; a[0] = s; return len(a[0]) }
func storeComposite(s string) int               { t := T{f: s}; return len(t.f) }
func storeGlobal(s string)                      { global = s }
func captures(s string) int                     { f := func() int { return len(s) }; return f() }
func deferred(s string)                         { defer safeLen(s) }
func mapKey(s string, m map[string]int) int     { return m[s] }
func returns(s string) string                   { return s }
func addr(s string) int                         { p := &s; return len(*p) }
func generic[E any](v E) int                    { return 0 }
func callsGeneric(s string) int                 { return generic(s) }
func variadic(s string) int                     { return count(s, "y") }
func count(xs ...string) int                    { return len(xs) }
func fixedSig(s string, f func(string) int) int { return f(s) }
func namedConv(s string) int                    { return len(Named(s)) }
func onwardBad(s string) int                    { return boxes(s) }
func valueUsed(s string) int                    { return len(s) }

var funcValue = valueUsed

type impl struct{}

func (impl) m(s string) int        { return len(s) } // matches unexpIface.m: binder
func (impl) Exported(s string) int { return len(s) } // exported method: reflection

// ---- GAPS: fail today, survive once sstring gains the member ----

func ranges(s string) int {
	n := 0
	for range s {
		n++
	}
	return n
}
func copies(s string) int { var b [4]byte; return copy(b[:], s) }
func spreads(s string) int {
	var buf [8]byte
	b := append(buf[:0], s...)
	return len(b)
}

// Callers reaches every function above with a literal argument (production call sites).
func Callers(m map[string]int) int {
	var x impl
	return safeLen("a") + int(safeIndex("a")) + b2i(safeCompare("a")) + safeSwitch("a") + safeAlias("ab") +
		safeConcat("a") + safeBytes("a") + safeDiscard("a") + onwardOK("a") + onwardChain("ab") +
		logf("format %d", 1) +
		boxes("a") + storeField("a") + storeElem("a") + storeComposite("a") + captures("a") +
		mapKey("a", m) + len(returns("a")) + addr("a") + callsGeneric("a") + variadic("a") +
		fixedSig("a", valueUsed) + namedConv("a") + onwardBad("a") + valueUsed("a") + x.m("a") + x.Exported("a") +
		ranges("a") + copies("a") + spreads("a") + one(storeGlobal, deferred)
}

func b2i(b bool) int {
	if b {
		return 1
	}
	return 0
}

func one(f, g func(string)) int { f("a"); g("a"); return 1 }

// testOnly is taken as a value only by fixture_test.go (the export_test.go shape).
func testOnly(s string) int { return len(s) }

func CallsTestOnly() int { return testOnly("a") }
