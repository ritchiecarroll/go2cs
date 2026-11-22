// MnLockWake reads the wake latency of a PARKED Mutex waiter (row 7's review: sync's Lockꞏtwin wakes a parked waiter
// through one ThreadPool hop, SemaphoreSlim's async completion). Each of N iterations (MN_LW_N, default 2,000): main
// holds the Mutex, starts a waiter that blocks in Lock (parks, in a twinned program under GO2CS_MN=1), sleeps 200 us so
// the waiter is waiting, stamps t0 and unlocks; the waiter stamps t1 when it holds the lock. stderr reads the hand-off
// t1-t0 as LATENCY p50/p90/p99/max in microseconds; stdout prints only the iteration count, EQUAL to Go's.
package main

import (
	"fmt"
	"os"
	"sort"
	"strconv"
	"sync"
	"time"
)

type state struct {
	mu sync.Mutex
	t0 time.Time
}

func waiter(s *state, lat []time.Duration, i int, done chan<- bool) {
	s.mu.Lock()
	lat[i] = time.Since(s.t0)
	s.mu.Unlock()
	done <- true
}

func main() {
	n := 2000
	if v, err := strconv.Atoi(os.Getenv("MN_LW_N")); err == nil && v > 0 {
		n = v
	}

	s := &state{}
	lat := make([]time.Duration, n)
	done := make(chan bool)

	for i := 0; i < n; i++ {
		s.mu.Lock()
		go waiter(s, lat, i, done)
		time.Sleep(200 * time.Microsecond)
		s.t0 = time.Now()
		s.mu.Unlock()
		<-done
	}

	sort.Slice(lat, func(a, b int) bool { return lat[a] < lat[b] })
	us := func(d time.Duration) string { return strconv.FormatFloat(float64(d.Nanoseconds())/1000, 'f', 1, 64) }
	fmt.Fprintln(os.Stderr, "LATENCY p50="+us(lat[n/2])+" p90="+us(lat[n*9/10])+" p99="+us(lat[n*99/100])+" max="+us(lat[n-1])+" us")
	fmt.Println("iterations:", n)
}
