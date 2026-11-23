// R7 seat 2: a Go method declared on a named CHANNEL, MAP, NUMERIC or STRING type always wins over a member the generated
// wrapper declares for its own use (Capacity, Length, IsClosed, IsUnbuffered, Receive, Received, Receiving, Sending,
// Make, GetEnumerator, IsNil, Count, CompareTo), even when its call fits the wrapper member exactly, and the wrapper still
// ranges, selects, measures and orders as Go does. A named pointer type cannot declare methods in Go, but its NAME can be
// one of the pointer wrapper's members (Value, ValueSlot, IsNull, IsNilPointer, NilInstance, of, at), as a type of any
// kind can be named like its own wrapper's members.
package main

import (
	"fmt"
	"slices"
)

// Pipe claims every channel-wrapper name.
type Pipe chan int

func (p Pipe) Capacity() int        { return -1 }
func (p Pipe) Length() int          { return -2 }
func (p Pipe) IsClosed() string     { return "never closed" }
func (p Pipe) IsUnbuffered() string { return "who knows" }
func (p Pipe) Receive() int         { return 77 }
func (p Pipe) Receiving() string    { return "receiving" }
func (p Pipe) Sending(v int) string { return fmt.Sprint("sending ", v) }
func (p Pipe) Received(v int) bool  { return v > 0 }
func (p Pipe) Send(v int) string    { return fmt.Sprint("send ", v) }
func (p Pipe) Sent(v int) bool      { return v < 0 }
func (p Pipe) Make(n int) Pipe      { return make(Pipe, n) }
func (p Pipe) GetEnumerator() int   { return 3 }

// Counts claims the map wrapper's names.
type Counts map[string]int

func (c Counts) Length() int           { return 100 + len(c) }
func (c Counts) IsNil() string         { return "not nil" }
func (c Counts) Count() int            { return -len(c) }
func (c Counts) Make() int             { return 1 }
func (c Counts) GetEnumerator() string { return "map enumerator" }
func (c Counts) Add(k string) int      { return len(k) }
func (c Counts) Remove(k string) bool  { return k == "" }

// Level's CompareTo has exactly IComparable<Level>'s shape.
type Level int

func (l Level) CompareTo(o Level) int { return 42 }

// Gauge's Value is the wrapper's underlying-value property's name (shape (b): a property is never invoked).
type Gauge float64

func (g Gauge) Value() float64 { return float64(g) * 2 }

// Name claims the named-string surface's names.
type Name string

func (n Name) Length() int           { return 500 + len(n) }
func (n Name) CompareTo(o Name) int  { return 43 }
func (n Name) GetEnumerator() string { return "name enumerator" }

// The near-miss rows: each method is ONE NAME AWAY from a claimed member, so the wrapper keeps every member and
// len, cap, range, send, receive, min and < still read as Go beside the Go methods.
type Wire chan int

func (w Wire) Capacities() int { return 9000 }
func (w Wire) Lengths() int    { return 9001 }

type Tally map[string]int

func (t Tally) Counts() int { return 9002 }

type Rank int

func (r Rank) CompareTwo(o Rank) int { return 9003 }

type Label string

func (l Label) Len() int { return 9004 }

// Types NAMED like their own wrapper's members, with no methods.
type IsNull *int
type Capacity chan string
type Count map[int]bool
type CompareTo int
type Length string

// Pointer types named like the pointer wrapper's members.
type point struct{ X, Y int }

type Value *[3]int
type ValueSlot *[]int
type IsNilPointer *[2]int
type NilInstance *string
type of *point
type at *[3]int

// strPtr is the near-miss for NilInstance: the same shape under a name the wrapper does not declare.
type strPtr *string

// bump writes through a Value parameter, and through a dereference of it.
func bump(p Value) {
	p[2]++
	*p = [3]int{p[0], p[1], p[2] * 10}
}

func main() {
	p := make(Pipe, 2)
	fmt.Println("Capacity/Length:", p.Capacity(), p.Length())
	fmt.Println("IsClosed/IsUnbuffered:", p.IsClosed(), p.IsUnbuffered())
	fmt.Println("Receive/Receiving/Sending/Received:", p.Receive(), p.Receiving(), p.Sending(4), p.Received(3))
	fmt.Println("Send/Sent:", p.Send(5), p.Sent(-1))
	m := p.Make(5)
	fmt.Println("Make:", cap(m), "GetEnumerator:", p.GetEnumerator())

	// Method values of the property-shaped names.
	pc, pl, pic := p.Capacity, p.Length, p.IsClosed
	fmt.Println("channel method values:", pc(), pl(), pic())

	// The select lowering on a channel that claims Receiving, Received and Sending.
	p <- 1
	select {
	case v := <-p:
		fmt.Println("selected receive:", v)
	default:
		fmt.Println("no receive")
	}
	p <- 2
	select {
	case v, ok := <-p:
		fmt.Println("selected receive ok:", v, ok)
	default:
		fmt.Println("no receive ok")
	}
	select {
	case p <- 9:
		fmt.Println("selected send")
	default:
		fmt.Println("no send")
	}
	fmt.Println("builtin len/cap:", len(p), cap(p), <-p)

	// Range over a channel that claims GetEnumerator.
	q := make(Pipe, 3)
	q <- 10
	q <- 20
	q <- 30
	close(q)
	for v := range q {
		fmt.Println("range pipe:", v)
	}

	c := Counts{"a": 1, "b": 2}
	fmt.Println("Counts:", c.Length(), c.IsNil(), c.Count(), c.Make(), c.GetEnumerator(), len(c))
	fmt.Println("Counts Add/Remove:", c.Add("xyz"), c.Remove("a"), len(c), c["a"])
	cl, cn := c.Length, c.IsNil
	fmt.Println("map method values:", cl(), cn())
	sum := 0
	for k, v := range c {
		sum += len(k) + v
	}
	fmt.Println("range counts:", sum)
	c["c"] = 3
	delete(c, "a")
	fmt.Println("map writes:", len(c), c["c"], c["a"])

	var a, b Level = 1, 2
	fmt.Println("CompareTo:", a.CompareTo(b), a < b, min(b, a), max(a, b))
	lv := a.CompareTo
	fmt.Println("numeric method value:", lv(b))
	levels := []Level{3, 1, 2}
	slices.Sort(levels)
	fmt.Println("sorted levels:", levels)

	g := Gauge(1.5)
	gv := g.Value
	fmt.Println("Gauge Value:", g.Value(), gv(), g+1)

	n, o := Name("bob"), Name("al")
	fmt.Println("Name:", n.Length(), n.CompareTo(o), n.GetEnumerator(), len(n), n < o, min(n, o), n+o)
	nl := n.Length
	fmt.Println("string method value:", nl())
	for i, r := range n {
		fmt.Println("range name:", i, string(r))
	}
	names := []Name{"c", "a", "b"}
	slices.Sort(names)
	fmt.Println("sorted names:", names, n[1:])

	w := make(Wire, 4)
	w <- 1
	fmt.Println("near-miss channel:", w.Capacities(), w.Lengths(), len(w), cap(w), <-w)
	t := Tally{"x": 1}
	fmt.Println("near-miss map:", t.Counts(), len(t), t["x"])
	r1, r2 := Rank(5), Rank(4)
	fmt.Println("near-miss numeric:", r1.CompareTwo(r2), min(r1, r2), r1 > r2)
	lb := Label("hey")
	fmt.Println("near-miss string:", lb.Len(), len(lb), lb[1:])
	for i, r := range lb {
		fmt.Println("near-miss range:", i, string(r))
	}

	x := 5
	var in IsNull = &x
	*in = 6
	fmt.Println("type IsNull:", *in, in != nil, x)
	cc := make(Capacity, 1)
	cc <- "c"
	fmt.Println("type Capacity:", len(cc), <-cc)
	k := Count{1: true}
	fmt.Println("type Count:", len(k), k[1])
	var ct CompareTo = 4
	fmt.Println("type CompareTo:", ct < 5, min(ct, 2))
	var ln Length = "four"
	fmt.Println("type Length:", len(ln), ln < "z", ln[1:])

	// The pointer wrappers whose type names are their own member names: dereference, write through, compare with nil,
	// a typed nil in an interface, a field through the pointer and its address, an element and its address.
	v1 := [3]int{7, 8, 9}
	var pv Value = &v1
	(*pv)[0] += 1
	pv[1] = 80
	bump(pv)
	sum3 := 0
	for i, e := range pv {
		sum3 += i * e
	}
	fmt.Println("type Value:", *pv, v1, pv[:2], len(pv), sum3, pv != nil)
	sl := []int{1, 2}
	var ps ValueSlot = &sl
	*ps = append(*ps, 3)
	fmt.Println("type ValueSlot:", len(*ps), sl)
	arr := [2]int{4, 5}
	var pn IsNilPointer = &arr
	fmt.Println("type IsNilPointer:", pn[1], len(pn), pn != nil)
	// A typed nil CONVERSION is the wrapper's canonical NilInstance (ΔNilInstance here); strPtr is the near-miss. (A
	// zero-valued named-pointer VARIABLE assigned to an interface reads as a nil interface in C# for every named
	// pointer type, a defect older than R7 and reported apart from it, so this program does not assign one.)
	var ni NilInstance
	var boxed any = NilInstance(nil)
	_, isNilInstance := boxed.(NilInstance)
	var boxedSp any = strPtr(nil)
	_, isStrPtr := boxedSp.(strPtr)
	s5 := "five"
	ni = &s5
	fmt.Println("typed nil:", boxed, boxed != nil, isNilInstance, boxedSp != nil, isStrPtr, ni != nil, *ni)
	pt := point{1, 2}
	var po of = &pt
	po.X = 10
	px := &po.Y
	*px = 20
	fmt.Println("type of:", po.X, po.Y, pt)
	three := [3]int{7, 8, 9}
	var pa at = &three
	pa[0] = 70
	pe := &pa[2]
	*pe = 90
	fmt.Println("type at:", pa[0], pa[1], three, pa[1:], len(pa[:2]))
}
