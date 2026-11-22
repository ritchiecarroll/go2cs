// MnGoschedFair is the Gosched-park seat's fairness row (P3's check (b)): with one P, two goroutines that ping-pong
// through each other's run-next slot must not starve a goroutine that yields with runtime.Gosched onto the global
// queue's tail. Go serves its global queue on a schedule (every 61st schedtick); go2cs bounds a run-next chain
// (GoScheduler.MaxRunNextChain), after which a ready goroutine goes to the global queue behind the yielder. Without
// that bound the yielder never runs again, and this program hangs: the mirror image of S2 arm 1 (MnSpinCap).
//
// Run it with ONE P and no injection: GO2CS_MN=1 GO2CS_MN_PROCS=1 GO2CS_MN_MAXINJECT=0 (runtime.GOMAXPROCS does not
// resize the scheduler's Ps; with injection on, the monitor adds a carrier for a cold step at start-up, and that second
// carrier serves the yielder whatever the bound is), and GOMAXPROCS=1 for Go. Read: about 128 run-next steps per
// yield with the bound, a hang with it lifted. MN_FAIR_YIELDS names how many times the yielder yields (default 1000).
// Deterministic: nothing printed depends on timing.
package main

import (
	"fmt"
	"os"
	"runtime"
	"strconv"
	"sync/atomic"
)

func pinger(a, b chan int, stop *atomic.Bool, done chan<- int64) {
	var trips int64
	for !stop.Load() {
		a <- 1
		<-b
		trips++
	}
	close(a)
	done <- trips
}

// ponger receives explicitly, never with `for v := range a`: a range over a channel in a twin is a synchronous foreach
// (it blocks its thread instead of parking; N11), so the pair never chained through run-next, and the first version of
// this row read green with the chain bound lifted. Both sides must park for the row to test the bound.
func ponger(a, b chan int) {
	for {
		v, ok := <-a
		if !ok {
			return
		}
		b <- v
	}
}

func yielder(start <-chan struct{}, n int, stop *atomic.Bool, done chan<- int64) {
	<-start
	for i := 0; i < n; i++ {
		runtime.Gosched()
	}
	stop.Store(true)
	done <- int64(n)
}

func main() {
	n := 1000
	if s := os.Getenv("MN_FAIR_YIELDS"); s != "" {
		if v, err := strconv.Atoi(s); err == nil && v > 0 {
			n = v
		}
	}

	a, b := make(chan int), make(chan int)
	start := make(chan struct{})
	var stop atomic.Bool
	pings, yields := make(chan int64, 1), make(chan int64, 1)

	go ponger(a, b)
	go pinger(a, b, &stop, pings)
	go yielder(start, n, &stop, yields)
	close(start)

	y := <-yields
	p := <-pings

	fmt.Println("the yielder finished its yields:", y == int64(n))
	fmt.Println("the ping-pong made progress:", p > 0)
}
