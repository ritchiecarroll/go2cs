package main

import (
	"fmt"
	"time"
)

// Every Go path that yields a ZERO VALUE of a struct holding a fixed array must yield one whose array has its full
// length: a map's missing key, a generic function's `var z T` and named result, `*new(T)`, a closed channel's
// receive, and a failed comma-ok type assertion. `*new(T)` already constructed before GoZero and is the control.
// A receive that PARKS on a channel another goroutine then closes takes a different wake path from a receive on
// an already-closed channel; both must yield the same zero.

type counts struct {
	vals [4]int
	n    int
}

func zeroVar[T any]() T {
	var z T
	return z
}

func zeroNamed[T any]() (z T) {
	return
}

func zeroNew[T any]() T {
	return *new(T)
}

// A generic local a closure captures (iter.Pull's shape) and one whose address is taken: both are declared on the
// heap rather than as a plain local.
func zeroCaptured[T any]() T {
	var z T
	get := func() T { return z }
	return get()
}

func zeroAddressed[T any]() T {
	var z T
	p := &z
	return *p
}

func arm(name string, f func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(name, "PANIC", r)
		}
	}()
	f()
}

func main() {
	arm("map missing key:", func() {
		m := map[string]counts{}
		v, ok := m["x"]
		fmt.Println("map missing key:", len(v.vals), v.vals[3], ok)
	})
	arm("generic var:", func() {
		z := zeroVar[counts]()
		fmt.Println("generic var:", len(z.vals), z.vals[3])
	})
	arm("generic named result:", func() {
		z := zeroNamed[counts]()
		fmt.Println("generic named result:", len(z.vals), z.vals[3])
	})
	arm("generic new:", func() {
		z := zeroNew[counts]()
		fmt.Println("generic new:", len(z.vals), z.vals[3])
	})
	arm("generic captured var:", func() {
		z := zeroCaptured[counts]()
		fmt.Println("generic captured var:", len(z.vals), z.vals[3])
	})
	arm("generic addressed var:", func() {
		z := zeroAddressed[counts]()
		fmt.Println("generic addressed var:", len(z.vals), z.vals[3])
	})
	arm("closed channel:", func() {
		ch := make(chan counts)
		close(ch)
		v, ok := <-ch
		fmt.Println("closed channel:", len(v.vals), v.vals[3], ok)
	})
	arm("parked receive:", func() {
		ch := make(chan counts)
		go func() {
			time.Sleep(100 * time.Millisecond)
			close(ch)
		}()
		v, ok := <-ch
		fmt.Println("parked receive:", len(v.vals), v.vals[3], ok)
	})
	arm("parked bare receive:", func() {
		ch := make(chan counts)
		go func() {
			time.Sleep(100 * time.Millisecond)
			close(ch)
		}()
		v := <-ch
		fmt.Println("parked bare receive:", len(v.vals), v.vals[3])
	})
	arm("failed assertion:", func() {
		var x any = 7
		v, ok := x.(counts)
		fmt.Println("failed assertion:", len(v.vals), v.vals[3], ok)
	})
}
