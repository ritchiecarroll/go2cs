// MnBoxAlias: N2's aliasing probe (DESIGN-mn-box-twin.md SN-6). A pointer-receiver method parks; while it is parked,
// another goroutine writes through the SAME pointer; after the park the method reads the write, as Go shares memory.
// Through a root pointer, a field of one, a slice element taken after an append (the old backing array), a value
// method through a pointer (a copy: the write must NOT be seen, and a mutation inside must NOT reach *p), an element
// of the own receiver's slice field, a nil receiver and a nil pointer's field (Go's panics). Every call is DIRECT, in a statement of its own, so a twin awaits
// the box twin; a twin that COPIED its receiver (GoTwinsBoxCopy, the control) prints stale values.
package main

import (
	"fmt"
)

type inner struct{ n int }

type acct struct {
	bal int
	sub inner
}

type cell struct{ v int }

type holder struct{ c cell }

type bag struct{ s []cell }

// The review's t.s[i] row: an element of a slice field of the box twin's OWN receiver.
func (b *bag) waitAt(i int) int {
	return b.s[i].waitAndRead()
}

var ready = make(chan struct{})
var resume = make(chan struct{})

func (a *acct) waitAndRead() int {
	ready <- struct{}{}
	<-resume
	return a.bal
}

func (s *inner) waitAndRead() int {
	ready <- struct{}{}
	<-resume
	return s.n
}

func (c *cell) waitAndRead() int {
	ready <- struct{}{}
	<-resume
	return c.v
}

// A value method: Go copies the receiver at the call.
func (a acct) valueRead() int {
	ready <- struct{}{}
	<-resume
	got := a.bal // the copy's, taken at the call: the write during the park must NOT show
	a.bal = 777  // a mutation of the copy: must NOT reach *p
	return got
}

// byref-param rows (DESIGN-mn-box-twin.md 12): an unexported function whose pointer parameter the converter
// ref-lowers to `ref cell c`; it parks, another goroutine writes through the same pointer, and it reads the write.
func readCell(c *cell) int {
	ready <- struct{}{}
	<-resume
	return c.v
}

// forward hands its own pointer parameter on: inside its twin the parameter is a box already.
func forward(c *cell) int {
	return readCell(c)
}

// byref-param probe rows of 12.1 (P4): each callee parks, a writer writes through the same storage, and the callee
// reads after the park. Two parameters over one storage, an array and its element, a nil field. 12.1's later-parking-
// argument row `f(p, <-ch)` is NOT here: gc reads p after the receive and the plain emission reads it before, a
// pre-existing converter divergence (COORD 2026-11-16 15:12Z, a BOARD row) that would redden this fixture's Output
// phase under every arm; its repro is src/tools/mnspike2/MnLaterArg.
type ptrs struct{ p *cell }

func readBoth(h *holder, c *cell) int {
	ready <- struct{}{}
	<-resume
	return h.c.v*100 + c.v
}

func readArr(a *[2]cell, c *cell) int {
	ready <- struct{}{}
	<-resume
	return a[1].v*100 + c.v
}

// thunkRead is launched by a go statement (12.1, Thunks): the converter wraps `go thunkRead(done, p)` in a literal
// that binds p at the go statement and hands it to the ref parameter; its twin hands the bound box.
func thunkRead(done chan int, c *cell) {
	ready <- struct{}{}
	<-resume
	done <- c.v
}

func nilParam(nh *holder) (out string) {
	defer func() {
		if r := recover(); r != nil {
			out = fmt.Sprint("panic: ", r)
		}
	}()
	v := readCell(&nh.c)
	return fmt.Sprint(v)
}

// writer waits for the method to park, writes, and lets it resume.
func writer(write func()) {
	<-ready
	write()
	resume <- struct{}{}
}

func nilRoot(np *acct) (out string) {
	defer func() {
		if r := recover(); r != nil {
			out = fmt.Sprint("panic: ", r)
		}
	}()
	v := np.waitAndRead()
	return fmt.Sprint(v)
}

func nilField(nh *holder) (out string) {
	defer func() {
		if r := recover(); r != nil {
			out = fmt.Sprint("panic: ", r)
		}
	}()
	v := nh.c.waitAndRead()
	return fmt.Sprint(v)
}

func main() {
	a := &acct{bal: 1, sub: inner{n: 1}}

	go writer(func() { a.bal = 2 })
	root := a.waitAndRead()
	fmt.Println("root", root)

	go writer(func() { a.sub.n = 7 })
	field := a.sub.waitAndRead()
	fmt.Println("field", field)

	cells := []cell{{v: 1}}
	old := cells
	cells = append(cells, cell{v: 9}) // a new backing array; old keeps the first
	go writer(func() { old[0].v = 4 })
	element := old[0].waitAndRead()
	fmt.Println("element", element)
	fmt.Println("element new-array", cells[0].v, "old-array", old[0].v)

	b := &bag{s: []cell{{v: 1}, {v: 1}}}
	go writer(func() { b.s[1].v = 3 })
	ownElement := b.waitAt(1)
	fmt.Println("own-element", ownElement)

	pc := &cell{v: 1}
	go writer(func() { pc.v = 5 })
	param := readCell(pc)
	fmt.Println("param", param)

	go writer(func() { pc.v = 6 })
	forwarded := forward(pc)
	fmt.Println("param-forward", forwarded)

	var hc cell
	hc.v = 1
	hp := &hc // the address escapes: hc is a heap local
	go writer(func() { hp.v = 8 })
	fmt.Println("param-heap", readCell(&hc))

	cs := []cell{{v: 1}, {v: 1}}
	go writer(func() { cs[1].v = 9 })
	fmt.Println("param-element", readCell(&cs[1]))

	h := &holder{c: cell{v: 1}}
	go writer(func() { h.c.v = 10 })
	fmt.Println("param-field", readCell(&h.c))

	e := &ptrs{p: &cell{v: 1}}
	go writer(func() { e.p.v = 11 })
	fmt.Println("param-ptr-field", readCell(e.p))

	hh := &holder{c: cell{v: 1}}
	go writer(func() { hh.c.v = 12 })
	fmt.Println("param-two", readBoth(hh, &hh.c))

	var arr [2]cell
	ap := &arr // the address escapes: arr is a heap local
	go writer(func() { ap[1].v = 13 })
	fmt.Println("param-array", readArr(&arr, &arr[1]))

	tp := &cell{v: 1}
	done := make(chan int)
	go thunkRead(done, tp)
	go writer(func() { tp.v = 14 })
	fmt.Println("param-thunk", <-done)

	go writer(func() { a.bal = 50 })
	value := a.valueRead()
	fmt.Println("value", value)
	fmt.Println("value after", a.bal)

	go writer(func() {})
	fmt.Println("nil", nilRoot(nil))

	fmt.Println("nil-field", nilField(nil))

	// Last: under GoTwinsBoxCopy the callee's prologue copies through the nil box and faults before nilParam's recover
	// is installed, which ends the program; every row above is read first.
	fmt.Println("param-nil-field", nilParam(nil))
}
