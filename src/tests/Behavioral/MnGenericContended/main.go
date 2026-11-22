// S2's generics item (docs/phase4/DATA-mn-spike-2.md 9.5): a twin of a generic function under the JIT and Native
// AOT, on the contended paths. MnAotGeneric already reads generic twins whose park is a non-generic round trip;
// here the park is on the type parameter itself, and every phase runs more goroutines than Ps:
//
//  1. pipelines: twice the P count of pipelines per element type, each four relay[T] stages long, pass a fixed
//     run of values over chan T, for T an int, a string, a struct and an array;
//  2. a contended generic counter: twice the P count of adder[T] goroutines take counter[T]'s mutex, for T an
//     int and a float64;
//  3. a generic select: pick[T] selects across two buffered chan T that both hold a value at every pick.
//
// Every goroutine first receives from its phase's start gate, so each has a hidden twin under GO2CS_MN=1. The
// relay receives are written out, with no range over a channel, so every park is one a twin can take.
// MN_GEN_READING=1 prints each phase's wall time to stderr, which the output comparison does not read.
//
// Deterministic by construction: nothing printed depends on the P count, the order of events or the timing.
package main

import (
	"fmt"
	"os"
	"runtime"
	"sync"
	"time"
)

const (
	items  = 500
	adds   = 1000
	picks  = 10000
	stages = 4
)

var reading = os.Getenv("MN_GEN_READING") == "1"

func report(phase string, began time.Time) {
	if reading {
		fmt.Fprintf(os.Stderr, "reading: %s elapsed_ms=%.1f\n", phase, float64(time.Since(began).Microseconds())/1000)
	}
}

type point struct{ x, y int }

type number interface{ ~int | ~float64 }

func relay[T any](gate <-chan struct{}, in <-chan T, out chan<- T) {
	<-gate
	for {
		v, ok := <-in
		if !ok {
			close(out)
			return
		}
		out <- v
	}
}

func source[T any](gate <-chan struct{}, out chan<- T, mk func(i int) T) {
	<-gate
	for i := 0; i < items; i++ {
		out <- mk(i)
	}
	close(out)
}

func sink[T any](in <-chan T, done chan<- []T) {
	var got []T
	for {
		v, ok := <-in
		if !ok {
			done <- got
			return
		}
		got = append(got, v)
	}
}

// pipelines runs n pipelines of relay[T] stages and reports whether every pipeline delivered every value, in order.
func pipelines[T comparable](n int, mk func(i int) T) bool {
	gate := make(chan struct{})
	done := make(chan []T, n)

	for p := 0; p < n; p++ {
		head := make(chan T)
		go source(gate, head, mk)
		in := head
		for s := 0; s < stages; s++ {
			out := make(chan T)
			go relay(gate, in, out)
			in = out
		}
		go sink(in, done)
	}

	close(gate)
	ok := true
	for p := 0; p < n; p++ {
		got := <-done
		ok = ok && len(got) == items
		for i := 0; ok && i < items; i++ {
			ok = got[i] == mk(i)
		}
	}
	return ok
}

type counter[T number] struct {
	mu sync.Mutex
	v  T
}

func (c *counter[T]) add(x T) {
	c.mu.Lock()
	c.v += x
	c.mu.Unlock()
}

func adder[T number](gate <-chan struct{}, c *counter[T], x T, wg *sync.WaitGroup) {
	defer wg.Done()
	<-gate
	for i := 0; i < adds; i++ {
		c.add(x)
	}
}

func contend[T number](n int, x T) T {
	var c counter[T]
	var wg sync.WaitGroup
	gate := make(chan struct{})
	for g := 0; g < n; g++ {
		wg.Add(1)
		go adder(gate, &c, x, &wg)
	}
	close(gate)
	wg.Wait()
	return c.v
}

func pick[T any](a, b chan T) (T, bool) {
	select {
	case v := <-a:
		a <- v
		return v, true
	case v := <-b:
		b <- v
		return v, false
	}
}

func selectFair[T any](va, vb T) bool {
	a, b := make(chan T, 1), make(chan T, 1)
	a <- va
	b <- vb
	first := 0
	for i := 0; i < picks; i++ {
		if _, fromA := pick(a, b); fromA {
			first++
		}
	}
	return first >= picks/10 && picks-first >= picks/10
}

func main() {
	procs := runtime.GOMAXPROCS(0)
	n := 2 * procs

	began := time.Now()
	fmt.Println("pipelines int:", pipelines(n, func(i int) int { return i * 3 }))
	fmt.Println("pipelines string:", pipelines(n, func(i int) string { return fmt.Sprint("s", i) }))
	fmt.Println("pipelines struct:", pipelines(n, func(i int) point { return point{i, -i} }))
	fmt.Println("pipelines array:", pipelines(n, func(i int) [4]int64 { return [4]int64{int64(i), 1, 2, 3} }))
	report("pipelines", began)

	began = time.Now()
	fmt.Println("counter int:", contend(n, 2) == 2*n*adds)
	fmt.Println("counter float64:", contend(n, 0.5) == 0.5*float64(n*adds))
	report("counter", began)

	began = time.Now()
	fmt.Println("select int:", selectFair(1, 2))
	fmt.Println("select struct:", selectFair(point{1, 1}, point{2, 2}))
	report("select", began)
}
