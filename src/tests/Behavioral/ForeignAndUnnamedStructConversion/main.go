// ForeignAndUnnamedStructConversion moves struct values between types Go treats as convertible and C# declares
// apart (sqlite-cs W12).
//
// FOREIGN TO LOCAL: `hertz(p)` where p is another package's struct with the same fields. The conversion operator
// lives in the local type and names the foreign one by its qualified name; two packages that each declare a
// `Pitch` get an operator each. At master the generated operator named the foreign type bare (CS0246).
//
// NAMED AND UNNAMED: an unnamed struct type is lifted to a generated C# type. An explicit conversion TO it,
// `struct{ Hz int }(h)`, was emitted as a call of the lifted type's name (CS1955); a comparison of a named
// value with an unnamed one had two candidate operators (CS9342). Every other move between the two is here as
// well, one per function, with the named side local and foreign: a declaration with a type, an assignment, an
// argument, a result, a struct field, a slice element, a map value and a channel send. The remaining places Go
// assigns a value follow, each once in each direction: a package variable, an assignment to a field, an element,
// a map entry and through a pointer, a variadic, method, function-value, deferred and go-statement argument, one
// of two results, a function literal's result, a positional field, an array element, a map key, a map index,
// delete, append, a send in a select, a receive, the comma-ok forms, a call's two results, a range clause that
// assigns, a type assertion's result, and a switch that compares the two.
//
// TWO LIFTS OF ONE UNNAMED TYPE: the same unnamed struct type written in two places is one Go type, and a
// value moves between the two with nothing written. Where the converter gives each place its own C# type (a
// function's own slot and the result of a function declared below it), the move is recorded as the named and
// unnamed moves are, and a comparison casts one operand to the other's type.
//
// A conversion copies the value: a write to the copy does not reach the source, a slice or pointer field still
// shares its target, and a pointer conversion aliases the source.
package main

import (
	"fmt"

	"ForeignAndUnnamedStructConversion/lib"
	"ForeignAndUnnamedStructConversion/other"
)

type hertz struct {
	Hz int
}

func (h hertz) half() int { return h.Hz / 2 }

type Hertz struct {
	Hz int
}

type boxed hertz

type note struct {
	Name   string
	Pitch  lib.Pitch
	Marks  []int
	Next   *lib.Note
	Weight float64
}

func show(h hertz) int { return h.Hz }

func made() hertz { return hertz(lib.Middle()) }

func foreignToLocal() {
	p := lib.Pitch{Hz: 440}

	h := hertz(p)
	h.Hz++
	fmt.Println("foreign to local:", h.Hz, p.Hz, h.half())
	H := Hertz(p)
	H.Hz += 2
	fmt.Println("foreign to exported local:", H.Hz, p.Hz)

	back := lib.Pitch(h)
	fmt.Println("local to foreign:", back.Hz, back.Double())

	o := other.Pitch{Hz: 7}
	fmt.Println("a second package's Pitch:", hertz(o).Hz, other.Pitch(h).Hz)

	b := boxed(p)
	fmt.Println("a type defined over the local:", b.Hz, lib.Pitch(b).Hz)

	fmt.Println("argument, result, call operand:", show(hertz(p)), made().Hz, hertz(lib.Middle()).half())

	n := lib.Note{Name: "A", Pitch: p, Marks: []int{1, 2}, Next: &lib.Note{Name: "B"}, Weight: 1.5}
	m := note(n)
	m.Marks[0] = 9
	m.Next.Name = "C"
	m.Name = "a"
	fmt.Println("several fields:", m.Name, n.Name, m.Pitch.Hz, n.Marks[0], n.Next.Name, m.Weight)
	n2 := lib.Note(m)
	fmt.Println("several fields back:", n2.Name, len(n2.Marks), n2.Next.Name)

	ph := (*hertz)(&p)
	ph.Hz = 100
	fmt.Println("pointer, foreign to local:", p.Hz, ph.half())
	pp := (*lib.Pitch)(&h)
	pp.Hz = 200
	fmt.Println("pointer, local to foreign:", h.Hz, pp.Double())
}

// Named to unnamed: the explicit conversion.
func convLocal(h hertz) int       { x := struct{ Hz int }(h); x.Hz++; return x.Hz + h.Hz }
func convForeign(p lib.Pitch) int { x := struct{ Hz int }(p); x.Hz++; return x.Hz + p.Hz }
func convOperand(p lib.Pitch) int { return struct{ Hz int }(p).Hz }

// Named to unnamed with no conversion written.
func takeAnon(a struct{ Hz int }) int { return a.Hz }

type anonBox struct {
	V struct{ Hz int }
}

func varLocal(h hertz) int       { var x struct{ Hz int } = h; return x.Hz }
func varForeign(p lib.Pitch) int { var x struct{ Hz int } = p; return x.Hz }
func assignLocal(h hertz) int {
	var x struct{ Hz int }
	x = h
	return x.Hz
}
func assignForeign(p lib.Pitch) int {
	var x struct{ Hz int }
	x = p
	return x.Hz
}
func argLocal(h hertz) int                    { return takeAnon(h) }
func argForeign(p lib.Pitch) int              { return takeAnon(p) }
func retLocal(h hertz) struct{ Hz int }       { return h }
func retForeign(p lib.Pitch) struct{ Hz int } { return p }
func fieldLocal(h hertz) int                  { b := anonBox{V: h}; return b.V.Hz }
func fieldForeign(p lib.Pitch) int            { b := anonBox{V: p}; return b.V.Hz }
func elemLocal(h hertz) int                   { s := []struct{ Hz int }{h}; return s[0].Hz }
func elemForeign(p lib.Pitch) int             { s := []struct{ Hz int }{p}; return s[0].Hz }
func mapLocal(h hertz) int                    { m := map[string]struct{ Hz int }{"k": h}; return m["k"].Hz }
func mapForeign(p lib.Pitch) int              { m := map[string]struct{ Hz int }{"k": p}; return m["k"].Hz }
func sendLocal(h hertz) int {
	c := make(chan struct{ Hz int }, 1)
	c <- h
	return (<-c).Hz
}
func sendForeign(p lib.Pitch) int {
	c := make(chan struct{ Hz int }, 1)
	c <- p
	return (<-c).Hz
}

// Unnamed to named, written and not.
func takeLocal(h hertz) int       { return h.Hz }
func takeForeign(p lib.Pitch) int { return p.Double() }

type localBox struct{ V hertz }
type foreignBox struct{ V lib.Pitch }

func backConvLocal(a struct{ Hz int }) int   { return hertz(a).half() }
func backConvForeign(a struct{ Hz int }) int { return lib.Pitch(a).Double() }
func backVarLocal(a struct{ Hz int }) int    { var x hertz = a; return x.half() }
func backVarForeign(a struct{ Hz int }) int  { var x lib.Pitch = a; return x.Double() }
func backAssignLocal(a struct{ Hz int }) int {
	var x hertz
	x = a
	return x.half()
}
func backAssignForeign(a struct{ Hz int }) int {
	var x lib.Pitch
	x = a
	return x.Double()
}
func backArgLocal(a struct{ Hz int }) int         { return takeLocal(a) }
func backArgForeign(a struct{ Hz int }) int       { return takeForeign(a) }
func backRetLocal(a struct{ Hz int }) hertz       { return a }
func backRetForeign(a struct{ Hz int }) lib.Pitch { return a }
func backFieldLocal(a struct{ Hz int }) int       { b := localBox{V: a}; return b.V.half() }
func backFieldForeign(a struct{ Hz int }) int     { b := foreignBox{V: a}; return b.V.Double() }
func backElemLocal(a struct{ Hz int }) int        { s := []hertz{a}; return s[0].half() }
func backElemForeign(a struct{ Hz int }) int      { s := []lib.Pitch{a}; return s[0].Double() }
func backMapLocal(a struct{ Hz int }) int         { m := map[string]hertz{"k": a}; return m["k"].half() }
func backMapForeign(a struct{ Hz int }) int {
	m := map[string]lib.Pitch{"k": a}
	return m["k"].Double()
}
func backSendLocal(a struct{ Hz int }) int {
	c := make(chan hertz, 1)
	c <- a
	return (<-c).half()
}
func backSendForeign(a struct{ Hz int }) int {
	c := make(chan lib.Pitch, 1)
	c <- a
	return (<-c).Double()
}

// A named value compared with an unnamed one, either side first.
func sameLocal(a struct{ Hz int }, h hertz) (bool, bool)       { return a == h, h != a }
func sameForeign(a struct{ Hz int }, p lib.Pitch) (bool, bool) { return a == p, p != a }

// The remaining places Go assigns a value, named to unnamed. The named side alternates between the local type
// and the foreign one.
var pkgHertz = hertz{Hz: 3}
var pkgAnon struct{ Hz int } = pkgHertz

type taker struct{}

func (taker) anon(a struct{ Hz int }) int { return a.Hz }
func (taker) named(p lib.Pitch) int       { return p.Double() }

func takeAnons(xs ...struct{ Hz int }) int {
	n := 0
	for _, x := range xs {
		n += x.Hz
	}
	return n
}

func setFieldLocal(h hertz) int { var b anonBox; b.V = h; return b.V.Hz }
func setElemForeign(p lib.Pitch) int {
	s := make([]struct{ Hz int }, 1)
	s[0] = p
	return s[0].Hz
}
func setMapLocal(h hertz) int {
	m := map[string]struct{ Hz int }{}
	m["k"] = h
	return m["k"].Hz
}
func setDerefForeign(p lib.Pitch) int {
	var x struct{ Hz int }
	q := &x
	*q = p
	return x.Hz
}
func variadicLocal(h hertz) int     { return takeAnons(h, h) }
func methodForeign(p lib.Pitch) int { return taker{}.anon(p) }
func funcValueLocal(h hertz) int    { f := takeAnon; return f(h) }
func deferForeign(p lib.Pitch) (n int) {
	defer func(a struct{ Hz int }) { n = a.Hz }(p)
	return 0
}
func goLocal(h hertz) int {
	c := make(chan int)
	go func(a struct{ Hz int }) { c <- a.Hz }(h)
	return <-c
}
func twoForeign(p lib.Pitch) (struct{ Hz int }, error) { return p, nil }
func literalLocal(h hertz) int {
	f := func() struct{ Hz int } { return h }
	return f().Hz
}
func positionalForeign(p lib.Pitch) int { b := anonBox{p}; return b.V.Hz }
func arrayLocal(h hertz) int            { s := [2]struct{ Hz int }{h, h}; return s[1].Hz }
func keyForeign(p lib.Pitch) int {
	m := map[struct{ Hz int }]int{p: 3}
	return m[struct{ Hz int }{Hz: p.Hz}]
}
func indexLocal(h hertz) int {
	m := map[struct{ Hz int }]int{{Hz: h.Hz}: 4}
	return m[h]
}
func deleteForeign(p lib.Pitch) int {
	m := map[struct{ Hz int }]int{{Hz: p.Hz}: 4, {Hz: 1}: 5}
	delete(m, p)
	return len(m)
}
func appendLocal(h hertz) int {
	var s []struct{ Hz int }
	s = append(s, h, h)
	return len(s) + s[1].Hz
}
func selectForeign(p lib.Pitch) int {
	c := make(chan struct{ Hz int }, 1)
	select {
	case c <- p:
	default:
	}
	return (<-c).Hz
}
func receiveLocal(h hertz) int {
	c := make(chan hertz, 1)
	c <- h
	var x struct{ Hz int }
	x = <-c
	return x.Hz
}
func receiveOkForeign(p lib.Pitch) int {
	c := make(chan lib.Pitch, 1)
	c <- p
	var x struct{ Hz int }
	var ok bool
	x, ok = <-c
	if !ok {
		return -1
	}
	return x.Hz
}
func mapOkLocal(h hertz) int {
	m := map[string]hertz{"k": h}
	var x struct{ Hz int }
	var ok bool
	x, ok = m["k"]
	if !ok {
		return -1
	}
	return x.Hz
}
func pitchAndTrue(p lib.Pitch) (lib.Pitch, bool) { return p, true }
func resultsForeign(p lib.Pitch) int {
	var x struct{ Hz int }
	var ok bool
	x, ok = pitchAndTrue(p)
	if !ok {
		return -1
	}
	return x.Hz
}
func rangeLocal(h hertz) int {
	var x struct{ Hz int }
	for _, x = range []hertz{h, {Hz: h.Hz + 1}} {
	}
	return x.Hz
}
func assertForeign(p lib.Pitch) int {
	var i any = p
	var x struct{ Hz int }
	x = i.(lib.Pitch)
	return x.Hz
}

// The same places, unnamed to named, with the local and the foreign side exchanged.
var pkgUnnamed = struct{ Hz int }{Hz: 4}
var pkgPitch lib.Pitch = pkgUnnamed

func takePitches(xs ...lib.Pitch) int {
	n := 0
	for _, x := range xs {
		n += x.Hz
	}
	return n
}

func (taker) local(h hertz) int { return h.half() }

func backSetFieldForeign(a struct{ Hz int }) int { var b foreignBox; b.V = a; return b.V.Double() }
func backSetElemLocal(a struct{ Hz int }) int {
	s := make([]hertz, 1)
	s[0] = a
	return s[0].half()
}
func backSetMapForeign(a struct{ Hz int }) int {
	m := map[string]lib.Pitch{}
	m["k"] = a
	return m["k"].Double()
}
func backSetDerefLocal(a struct{ Hz int }) int {
	var x hertz
	q := &x
	*q = a
	return x.half()
}
func backVariadicForeign(a struct{ Hz int }) int  { return takePitches(a, a) }
func backMethodLocal(a struct{ Hz int }) int      { return taker{}.local(a) }
func backFuncValueForeign(a struct{ Hz int }) int { f := takeForeign; return f(a) }
func backDeferLocal(a struct{ Hz int }) (n int) {
	defer func(h hertz) { n = h.half() }(a)
	return 0
}
func backGoForeign(a struct{ Hz int }) int {
	c := make(chan int)
	go func(p lib.Pitch) { c <- p.Double() }(a)
	return <-c
}
func backTwoLocal(a struct{ Hz int }) (hertz, error) { return a, nil }
func backLiteralForeign(a struct{ Hz int }) int {
	f := func() lib.Pitch { return a }
	return f().Double()
}
func backPositionalLocal(a struct{ Hz int }) int { b := localBox{a}; return b.V.half() }
func backArrayForeign(a struct{ Hz int }) int    { s := [2]lib.Pitch{a, a}; return s[1].Double() }
func backKeyLocal(a struct{ Hz int }) int {
	m := map[hertz]int{a: 3}
	return m[hertz{Hz: a.Hz}]
}
func backIndexForeign(a struct{ Hz int }) int {
	m := map[lib.Pitch]int{{Hz: a.Hz}: 4}
	return m[a]
}
func backDeleteLocal(a struct{ Hz int }) int {
	m := map[hertz]int{{Hz: a.Hz}: 4, {Hz: 1}: 5}
	delete(m, a)
	return len(m)
}
func backAppendForeign(a struct{ Hz int }) int {
	var s []lib.Pitch
	s = append(s, a, a)
	return len(s) + s[1].Double()
}
func backSelectLocal(a struct{ Hz int }) int {
	c := make(chan hertz, 1)
	select {
	case c <- a:
	default:
	}
	return (<-c).half()
}
func backReceiveForeign(a struct{ Hz int }) int {
	c := make(chan struct{ Hz int }, 1)
	c <- a
	var x lib.Pitch
	x = <-c
	return x.Double()
}
func backReceiveOkLocal(a struct{ Hz int }) int {
	c := make(chan struct{ Hz int }, 1)
	c <- a
	var x hertz
	var ok bool
	x, ok = <-c
	if !ok {
		return -1
	}
	return x.half()
}
func backMapOkForeign(a struct{ Hz int }) int {
	m := map[string]struct{ Hz int }{"k": a}
	var x lib.Pitch
	var ok bool
	x, ok = m["k"]
	if !ok {
		return -1
	}
	return x.Double()
}
func unnamedAndTrue(a struct{ Hz int }) (struct{ Hz int }, bool) { return a, true }
func backResultsLocal(a struct{ Hz int }) int {
	var x hertz
	var ok bool
	x, ok = unnamedAndTrue(a)
	if !ok {
		return -1
	}
	return x.half()
}
func backRangeForeign(a struct{ Hz int }) int {
	var x lib.Pitch
	for _, x = range []struct{ Hz int }{a, {Hz: a.Hz + 1}} {
	}
	return x.Double()
}
func backAssertLocal(a struct{ Hz int }) int {
	var i any = a
	var x hertz
	x = i.(struct{ Hz int })
	return x.half()
}

// A switch compares its tag with each case value: one named, one unnamed, either as the tag.
func caseLocal(a struct{ Hz int }, h hertz) int {
	switch h {
	case a:
		return 1
	}
	return 0
}
func caseTagForeign(a struct{ Hz int }, p lib.Pitch) int {
	switch a {
	case p:
		return 1
	}
	return 0
}

func everyOtherPlace() {
	h := hertz{Hz: 220}
	p := lib.Pitch{Hz: 440}
	a := struct{ Hz int }{Hz: 8}

	fmt.Println("package variable:", pkgAnon.Hz, pkgPitch.Double())
	fmt.Println("assign to field, element, map entry, through pointer:", setFieldLocal(h), setElemForeign(p), setMapLocal(h), setDerefForeign(p))
	fmt.Println("variadic, method, function value:", variadicLocal(h), methodForeign(p), funcValueLocal(h))
	fmt.Println("deferred and go-statement argument:", deferForeign(p), goLocal(h))
	two, err := twoForeign(p)
	fmt.Println("one of two results, a literal's result:", two.Hz, err, literalLocal(h))
	fmt.Println("positional field, array element:", positionalForeign(p), arrayLocal(h))
	fmt.Println("map key, index, delete, append:", keyForeign(p), indexLocal(h), deleteForeign(p), appendLocal(h))
	fmt.Println("select send, receive, comma-ok receive and map read:", selectForeign(p), receiveLocal(h), receiveOkForeign(p), mapOkLocal(h))
	fmt.Println("two results, range, assertion:", resultsForeign(p), rangeLocal(h), assertForeign(p))

	fmt.Println("named: assign to field, element, map entry, through pointer:", backSetFieldForeign(a), backSetElemLocal(a), backSetMapForeign(a), backSetDerefLocal(a))
	fmt.Println("named: variadic, method, function value:", backVariadicForeign(a), backMethodLocal(a), backFuncValueForeign(a))
	fmt.Println("named: deferred and go-statement argument:", backDeferLocal(a), backGoForeign(a))
	twoBack, err := backTwoLocal(a)
	fmt.Println("named: one of two results, a literal's result:", twoBack.half(), err, backLiteralForeign(a))
	fmt.Println("named: positional field, array element:", backPositionalLocal(a), backArrayForeign(a))
	fmt.Println("named: map key, index, delete, append:", backKeyLocal(a), backIndexForeign(a), backDeleteLocal(a), backAppendForeign(a))
	fmt.Println("named: select send, receive, comma-ok receive and map read:", backSelectLocal(a), backReceiveForeign(a), backReceiveOkLocal(a), backMapOkForeign(a))
	fmt.Println("named: two results, range, assertion:", backResultsLocal(a), backRangeForeign(a), backAssertLocal(a))

	fmt.Println("switch:", caseLocal(a, h), caseLocal(a, hertz{Hz: 8}), caseTagForeign(a, p), caseTagForeign(a, lib.Pitch{Hz: 8}))
}

func namedAndUnnamed() {
	h := hertz{Hz: 220}
	p := lib.Pitch{Hz: 440}
	var a struct{ Hz int }
	a.Hz = 8

	fmt.Println("convert to unnamed:", convLocal(h), convForeign(p), convOperand(p), h.Hz, p.Hz)
	fmt.Println("declare:", varLocal(h), varForeign(p))
	fmt.Println("assign:", assignLocal(h), assignForeign(p))
	fmt.Println("argument:", argLocal(h), argForeign(p))
	fmt.Println("result:", retLocal(h).Hz, retForeign(p).Hz)
	fmt.Println("field:", fieldLocal(h), fieldForeign(p))
	fmt.Println("element:", elemLocal(h), elemForeign(p))
	fmt.Println("map value:", mapLocal(h), mapForeign(p))
	fmt.Println("send:", sendLocal(h), sendForeign(p))

	fmt.Println("convert to named:", backConvLocal(a), backConvForeign(a))
	fmt.Println("declare named:", backVarLocal(a), backVarForeign(a))
	fmt.Println("assign named:", backAssignLocal(a), backAssignForeign(a))
	fmt.Println("argument named:", backArgLocal(a), backArgForeign(a))
	fmt.Println("result named:", backRetLocal(a).Hz, backRetForeign(a).Hz)
	fmt.Println("field named:", backFieldLocal(a), backFieldForeign(a))
	fmt.Println("element named:", backElemLocal(a), backElemForeign(a))
	fmt.Println("map value named:", backMapLocal(a), backMapForeign(a))
	fmt.Println("send named:", backSendLocal(a), backSendForeign(a))

	eq, ne := sameLocal(a, h)
	fmt.Println("compare local:", eq, ne)
	eq, ne = sameLocal(a, hertz{Hz: 8})
	fmt.Println("compare local, equal:", eq, ne)
	eq, ne = sameForeign(a, p)
	fmt.Println("compare foreign:", eq, ne)
	eq, ne = sameForeign(a, lib.Pitch{Hz: 8})
	fmt.Println("compare foreign, equal:", eq, ne)

	pa := (*struct{ Hz int })(&h)
	pa.Hz = 5
	pf := (*struct{ Hz int })(&p)
	pf.Hz = 6
	fmt.Println("pointer to unnamed:", h.Hz, p.Hz)
	ph := (*hertz)(&a)
	ph.Hz = 10
	pp := (*lib.Pitch)(&a)
	fmt.Println("pointer to named:", a.Hz, ph.half(), pp.Double())
}

// Two lifts of one unnamed type. Each function below writes `struct{ Bpm int }` for its own slot and takes the
// value from a declaration further down the file, so the slot and the value have a C# type each.
func liftVar() int {
	var x struct{ Bpm int } = laterTempo()
	x.Bpm++
	return x.Bpm + laterTempo().Bpm
}

func liftAssign() int {
	var x struct{ Bpm int }
	x = laterTempo()
	return x.Bpm
}

func liftField() int {
	var v struct{ Bpm int }
	v.Bpm = 70
	b := laterBox{V: v}
	b.V.Bpm++
	return b.V.Bpm + v.Bpm
}

func liftSetField() int {
	var v struct{ Bpm int }
	v.Bpm = 71
	var b laterBox
	b.V = v
	return b.V.Bpm
}

func liftElements() int {
	s := []struct{ Bpm int }{laterTempo()}
	m := map[struct{ Bpm int }]int{laterTempo(): 1}
	s = append(s, laterTempo())
	return s[0].Bpm + m[laterTempo()] + len(s)
}

func liftRange() int {
	var x struct{ Bpm int }
	n := 0

	for _, x = range laterTempos() {
		n += x.Bpm
	}

	return n
}

func liftReceive() int {
	var x struct{ Bpm int }
	var ok bool
	x, ok = <-laterChan()

	if !ok {
		return -1
	}

	return x.Bpm
}

func liftTuple() int {
	var x struct{ Bpm int }
	var ok bool
	x, ok = laterPair()

	if !ok {
		return -1
	}

	return x.Bpm
}

func liftArgument() int {
	var v struct{ Bpm int }
	v.Bpm = 72
	return laterTake(v)
}

func liftCompare(bpm int) (bool, bool) {
	var x struct{ Bpm int }
	x.Bpm = bpm
	return x == laterTempo(), laterTempo() != x
}

func liftSwitch(bpm int) int {
	var x struct{ Bpm int }
	x.Bpm = bpm
	n := 0

	switch x {
	case laterTempo():
		n++
	}

	switch laterTempo() {
	case x:
		n += 2
	}

	return n
}

func laterTempo() (r struct{ Bpm int }) {
	r.Bpm = 60
	return
}

func laterTempos() []struct{ Bpm int } {
	return []struct{ Bpm int }{{Bpm: 1}, {Bpm: 2}}
}

func laterChan() chan struct{ Bpm int } {
	c := make(chan struct{ Bpm int }, 1)
	c <- laterTempo()
	return c
}

func laterPair() (r struct{ Bpm int }, ok bool) {
	r.Bpm = 61
	return r, true
}

func laterTake(x struct{ Bpm int }) int { return x.Bpm }

type laterBox struct{ V struct{ Bpm int } }

func twoLifts() {
	fmt.Println("lifts: var, assign:", liftVar(), liftAssign())
	fmt.Println("lifts: field, set field:", liftField(), liftSetField())
	fmt.Println("lifts: elements, range:", liftElements(), liftRange())
	fmt.Println("lifts: receive, two results, argument:", liftReceive(), liftTuple(), liftArgument())
	eq, ne := liftCompare(60)
	fmt.Println("lifts: compare, equal:", eq, ne)
	eq, ne = liftCompare(59)
	fmt.Println("lifts: compare:", eq, ne)
	fmt.Println("lifts: switch:", liftSwitch(60), liftSwitch(59))
}

func main() {
	foreignToLocal()
	namedAndUnnamed()
	everyOtherPlace()
	twoLifts()
}
