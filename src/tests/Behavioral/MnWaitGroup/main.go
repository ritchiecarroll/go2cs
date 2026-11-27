// M:N, sync.WaitGroup.Wait's twin (docs/phase4/DESIGN-mn-waitgroup-twin.md, row R4): goroutines that park only in
// WaitGroup.Wait. Under GO2CS_MN=1 each waiter is a twin and parks in Waitꞏtwin instead of holding its carrier; with the
// scheduler off it waits in the plain Wait. Either way the output is Go's.
//
//  1. K = 4P waiters each report ready, then wait on one WaitGroup (through waitAll, which parks ONLY in wg.Wait, so it
//     is a twin only because Wait has one: the selection proof reads it in the twin report). Main waits for every
//     ready report, lets them park, then drains the count; every waiter must finish.
//  2. Three generations reuse one WaitGroup, each Add after the previous Wait has returned (Go's rule).
//  3. A Wait on a zero count returns at once.
//
// MN_WG_READING=1 prints K and the P count to stderr, which the output comparison does not read. Deterministic by
// construction: nothing printed depends on the P count, the order or the timing.
package main

import (
	"fmt"
	"os"
	"runtime"
	"sync"
	"time"
)

var reading = os.Getenv("MN_WG_READING") == "1"

// waitAll parks only through wg.Wait.
func waitAll(wg *sync.WaitGroup) {
	wg.Wait()
}

func released(k int) bool {
	var work, ready, done sync.WaitGroup
	results := make([]int, k)

	work.Add(1)
	ready.Add(k)
	done.Add(k)

	for i := 0; i < k; i++ {
		go func(i int) {
			defer done.Done()
			ready.Done()
			waitAll(&work)
			results[i] = i * i
		}(i)
	}

	ready.Wait()
	time.Sleep(20 * time.Millisecond)
	work.Done()
	done.Wait()

	for i, r := range results {
		if r != i*i {
			return false
		}
	}
	return true
}

func generations(k, n int) bool {
	var wg sync.WaitGroup
	total := 0
	var mu sync.Mutex

	for gen := 0; gen < n; gen++ {
		wg.Add(k)
		for i := 0; i < k; i++ {
			go func() {
				defer wg.Done()
				mu.Lock()
				total++
				mu.Unlock()
			}()
		}
		wg.Wait()
	}
	return total == k*n
}

func main() {
	procs := runtime.GOMAXPROCS(0)
	k := 4 * procs
	if k < 8 {
		k = 8
	}
	if reading {
		fmt.Fprintf(os.Stderr, "reading: procs=%d waiters=%d\n", procs, k)
	}

	fmt.Println("every waiter released:", released(k))
	fmt.Println("three generations counted:", generations(k, 3))

	var zero sync.WaitGroup
	zero.Wait()
	fmt.Println("zero-count wait returned")
}
