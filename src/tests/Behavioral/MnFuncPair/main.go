package main

import (
	"fmt"
	"sync"
)

// M:N PROTOTYPE, N3's D3 core. Prototype code: nothing from this branch lands on master as it is.
//
// N goroutines each run `run`, a function that parks on its own (so it is a twin under any caller rule) and then
// calls through a func value. The func value is a literal main creates, and the literal parks on a gate. Under the
// D3 core the literal is a PAIR (main is a twin and the literal can park) and run's twin awaits the pair's twin: the
// literal's park is a scheduler park, so N parked goroutines hold no carrier. With GoTwinsPairs=false the literal is
// plain: each park holds the carrier it runs on. The scheduler's READING line (GO2CS_MN_READING=1) shows which.

const n = 2000

var (
	ready   = make(chan struct{}, n)
	entered = make(chan struct{}, n)
	gate    = make(chan struct{})
	wg      sync.WaitGroup
	results = make([]int, n)
)

func run(f func() int, i int) {
	defer wg.Done()
	<-ready
	v := f()
	results[i] = v
}

func main() {
	k := 1
	// Every goroutine says it is in the literal, then parks on the gate: main closes it only when all n are there.
	wait := func() int {
		entered <- struct{}{}
		<-gate
		return k
	}

	for i := 0; i < n; i++ {
		ready <- struct{}{}
	}

	for i := 0; i < n; i++ {
		wg.Add(1)
		go run(wait, i)
	}

	for i := 0; i < n; i++ {
		<-entered
	}

	close(gate)
	wg.Wait()

	sum := 0
	for _, r := range results {
		sum += r
	}

	fmt.Println("parked behind a func value and released:", n, "sum", sum)
}
