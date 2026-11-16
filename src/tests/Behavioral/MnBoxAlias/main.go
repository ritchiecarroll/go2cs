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

	go writer(func() { a.bal = 50 })
	value := a.valueRead()
	fmt.Println("value", value)
	fmt.Println("value after", a.bal)

	go writer(func() {})
	fmt.Println("nil", nilRoot(nil))

	fmt.Println("nil-field", nilField(nil))
}
