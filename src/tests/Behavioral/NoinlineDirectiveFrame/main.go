package main

import (
	"fmt"
	"runtime"
)

// callerName names the function that called it, one frame up -- the frame a
// //go:noinline function promises to keep.
func callerName() string {
	pc, _, _, ok := runtime.Caller(1)
	if !ok {
		return "<no caller>"
	}
	return runtime.FuncForPC(pc).Name()
}

// keeper is two statements, so no other rule marks it; only the directive
// keeps its frame out of main under an optimizing JIT.
//
//go:noinline
func keeper(b []byte, i int) string {
	name := callerName()
	return fmt.Sprintf("%s read %d", name, b[i])
}

type counter struct{ n int }

// bump is a directed method on a pointer receiver.
//
//go:noinline
func (c *counter) bump() string {
	c.n++
	return callerName()
}

// forward is a thin forwarder TO a directed function; the directive does not
// reach it (it is marked only by the module fixed-skip path closure).
func forward(b []byte, i int) string { return keeper(b, i) }

func main() {
	b := []byte{7, 9}
	c := &counter{}

	for i := 0; i < 200; i++ {
		k := keeper(b, i%2)
		m := c.bump()
		f := forward(b, 1)

		if i == 0 || i == 199 {
			fmt.Println(k)
			fmt.Println(m)
			fmt.Println(f)
		}
	}

	fmt.Println("bumped", c.n)
}
