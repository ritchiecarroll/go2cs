// MnLockSleep is row 7 pieces 2+3's program row: N goroutines (MN_LS_N, default 2,000) each sleep, then take one
// contended sync.Mutex. Under L-min a twinned program's time.Sleep and contended Mutex.Lock were PLAIN calls, so
// every such goroutine blocked an OS thread (the MnMixedStorm capture: 46,223 threads, hangs at n=100,000). With
// sync's Lockꞏtwin (parking, box receiver) and time's Sleepꞏtwin bound, both park, and the program needs about P
// threads. Read, under GO2CS_MN=1 GO2CS_MN_READING=1: peak-os-threads and parks; the output is EQUAL to Go's.
// Deterministic: the sum and the count do not depend on order.
package main

import (
	"fmt"
	"os"
	"strconv"
	"sync"
	"time"
)

type counter struct {
	mu  sync.Mutex
	sum int
	n   int
}

func (c *counter) add(v int) {
	c.mu.Lock()
	c.sum += v
	c.n++
	time.Sleep(time.Microsecond) // hold the lock across a park, so others contend
	c.mu.Unlock()
}

func worker(c *counter, id int, wg *sync.WaitGroup) {
	defer wg.Done()
	time.Sleep(time.Millisecond)
	c.add(id)
}

func main() {
	n := 2000
	if v, err := strconv.Atoi(os.Getenv("MN_LS_N")); err == nil && v > 0 {
		n = v
	}

	c := &counter{}
	var wg sync.WaitGroup
	wg.Add(n)

	for i := 0; i < n; i++ {
		go worker(c, i, &wg)
	}

	wg.Wait()
	fmt.Println("n:", c.n, "sum:", c.sum, "match:", c.sum == n*(n-1)/2)
}
