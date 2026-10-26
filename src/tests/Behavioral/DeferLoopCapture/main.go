package main

import (
	"fmt"
	"sort"
	"time"
)

// closer mirrors net.Conn's shape: an interface whose method RETURNS a value. That is what
// forces the converter's synthesized-lambda arm (hasResults), where the defect lives -- the
// method-group arm binds its receiver eagerly and was always correct.
type closer interface {
	Close() error
}

type conn struct{ name string }

func (c *conn) Close() error {
	fmt.Println("close", c.name)
	return nil
}

// Reset has a POINTER receiver and no result, so a deferred `v[0].Reset()` binds a method group.
func (c *conn) Reset() {
	fmt.Println("reset", c.name)
}

// valueConn has a VALUE receiver: Go copies the receiver at the go statement.
type valueConn struct {
	name string
	out  chan string
}

func (a valueConn) Close() error {
	a.out <- a.name
	return nil
}

// asyncConn reports through a channel so the `go` shape can be drained in a deterministic
// order; printing straight from three concurrent goroutines is unordered and would flake.
type asyncConn struct {
	name string
	out  chan string
}

func (a *asyncConn) Close() error {
	a.out <- a.name
	return nil
}

// CONTROL (green before and after the fix): a deferred call's RECEIVER is bound at the defer
// STATEMENT, so reassigning the variable afterwards must not change what is closed. The
// capture-snapshot machinery already hoists a simple identifier receiver.
func receiverReassignedNoLoop() {
	fmt.Println("-- control: receiver bound at defer-statement time --")
	x := &conn{name: "first"}
	defer x.Close()
	x = &conn{name: "second"}
	_ = x
}

// RED SHAPE 1 -- net's TestConcurrentSetDeadline: a deferred method call indexed by a
// 3-clause loop variable. Without per-iteration semantics for the SYNTHESIZED lambda the
// index is read when the defer fires, i.e. at the loop's final value: index out of range.
func deferIndexedReceiverInLoop() {
	fmt.Println("-- red 1: defer c[i].Close() --")
	var c [3]closer
	for i := 0; i < 3; i++ {
		c[i] = &conn{name: fmt.Sprint("conn", i)}
		defer c[i].Close()
	}
}

// RED SHAPE 2 -- the same defect at the OTHER synthesized-lambda site. Drained with a
// timeout so the guard terminates in the broken state instead of hanging on a goroutine
// that died indexing out of range.
func goIndexedReceiverInLoop() {
	fmt.Println("-- red 2: go c[i].Close() --")
	out := make(chan string, 3)
	var c [3]closer
	for i := 0; i < 3; i++ {
		c[i] = &asyncConn{name: fmt.Sprint("g", i), out: out}
		go c[i].Close()
	}
	got := make([]string, 0, 3)
	for j := 0; j < 3; j++ {
		select {
		case s := <-out:
			got = append(got, s)
		case <-time.After(5 * time.Second):
			got = append(got, "TIMEOUT")
		}
	}
	sort.Strings(got)
	for _, s := range got {
		fmt.Println("closed", s)
	}
}

// RED SHAPE 3 -- Go evaluates a deferred method call's RECEIVER at the defer statement, so
// reassigning the array ELEMENT afterwards must not change what is closed. An array copy
// aliases its elements (array<T> is a struct over a shared backing array), so capturing the
// array and indexing it inside the thunk read the element when the thunk ran.
func deferElementReassigned() {
	fmt.Println("-- red 3: defer c[0].Close(), then c[0] = other --")
	var c [2]closer
	c[0] = &conn{name: "first"}
	defer c[0].Close()
	var p [2]*conn
	p[0] = &conn{name: "first ptr"}
	defer p[0].Close()
	c[0] = &conn{name: "second"}
	p[0] = &conn{name: "second ptr"}
}

// RED SHAPE 4 -- a POINTER-receiver method on an addressable VALUE element: Go binds the
// receiver as &v[0], so the deferred call sees a write to v[0] made after the defer statement.
// The ADDRESS is what is evaluated at the statement, not the element's value. The method-group
// form (Reset, no result) named a box that the capture copy does not have (CS0103).
func deferElementAddressReceiver() {
	fmt.Println("-- red 4: defer v[0].Close() on a [2]conn, then v[0] = other --")
	var v [2]conn
	v[0] = conn{name: "first value"}
	defer v[0].Close()
	defer v[0].Reset()
	v[0] = conn{name: "second value"}
}

// RED SHAPE 5 -- the `go` twin: the receiver is evaluated by the spawning goroutine at the go
// statement, before the element is reassigned, whatever the scheduler does next.
func goElementReassigned() {
	fmt.Println("-- red 5: go c[0].Close(), then c[0] = other --")
	out := make(chan string, 2)
	var c [1]closer
	c[0] = &asyncConn{name: "first", out: out}
	go c[0].Close()
	c[0] = &asyncConn{name: "second", out: out}
	var v [1]valueConn
	v[0] = valueConn{name: "first value", out: out}
	go v[0].Close()
	v[0] = valueConn{name: "second value", out: out}
	got := make([]string, 0, 2)
	for j := 0; j < 2; j++ {
		select {
		case s := <-out:
			got = append(got, s)
		case <-time.After(5 * time.Second):
			got = append(got, "TIMEOUT")
		}
	}
	sort.Strings(got)
	for _, s := range got {
		fmt.Println("closed", s)
	}
}

// CONTROL: a SOURCE-level func literal capturing the loop variable already takes Go 1.22
// per-iteration semantics. Must stay green -- it is what proves the fix did not change the
// path that was already correct.
func closureCapture3Clause() {
	fmt.Println("-- control: closure capture (3-clause) --")
	for i := 0; i < 3; i++ {
		defer func() { fmt.Println("closure", i) }()
	}
}

// CONTROL: range-loop equivalent.
func closureCaptureRange() {
	fmt.Println("-- control: closure capture (range) --")
	for _, v := range []string{"x", "y", "z"} {
		defer func() { fmt.Println("range", v) }()
	}
}

// CONTROL: deferred ARGUMENTS are evaluated at the defer statement. The fix deliberately does
// NOT mint a per-iteration copy for a variable appearing only here, so this shape's emission
// must be unchanged.
func plainArgs() {
	fmt.Println("-- control: plain args (3-clause) --")
	for i := 0; i < 3; i++ {
		defer fmt.Println("arg", i)
	}
}

func main() {
	receiverReassignedNoLoop()
	goIndexedReceiverInLoop()
	deferIndexedReceiverInLoop()
	closureCapture3Clause()
	closureCaptureRange()
	plainArgs()
	deferElementReassigned()
	deferElementAddressReceiver()
	goElementReassigned()
}
