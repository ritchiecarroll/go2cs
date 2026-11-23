// MnHandBack isolates the P hand-back (the s_p convoy row's second instrument, COORD's ruling of 2026-11-22): PAIRS
// goroutine pairs (MN_HB_PAIRS, default 100) ping-pong CYCLES times each (MN_HB_CYCLES, default 1,000) through two
// Mutexes held as binary semaphores and reached through a sync.Locker, an interface call a twinned program keeps plain:
// each Lock BLOCKS its thread and gives its P back, and the partner's Unlock returns it, so every cycle is one block ->
// return through GoScheduler.ExitBlocked, with only 2 x PAIRS threads (no thread-count bound). Each return is timed
// from the partner's stamp just before its Unlock to the moment Lock returns. stderr reads HANDBACK p50/p90/p99/max in
// microseconds and the total wall; stdout prints only the totals, EQUAL to Go's.
package main

import (
	"fmt"
	"os"
	"runtime"
	"sort"
	"strconv"
	"sync"
	"time"
)

type pair struct {
	a, b    sync.Mutex
	stampA  int64 // set by b before it unlocks a
	stampB  int64 // set by a before it unlocks b
	latency []int64
}

func side(me, other sync.Locker, myStamp, otherStamp *int64, lat []int64, cycles int, first bool, done *sync.WaitGroup) {
	defer done.Done()
	// One Gosched makes this function a TWIN, so it runs on a carrier and each plain Lock below blocks THAT carrier: the
	// block -> return goes through GoScheduler.EnterBlocked and ExitBlocked. Without it the goroutine is plain, runs on
	// its own thread, and never touches the P tokens (the first version of this fixture read both arms alike).
	runtime.Gosched()
	for i := 0; i < cycles; i++ {
		me.Lock() // blocks until the partner unlocks it
		if !first || i > 0 {
			lat[i] = time.Now().UnixNano() - *myStamp
		}
		*otherStamp = time.Now().UnixNano()
		other.Unlock()
	}
}

func envInt(name string, fallback int) int {
	if v, err := strconv.Atoi(os.Getenv(name)); err == nil && v > 0 {
		return v
	}
	return fallback
}

func main() {
	pairs := envInt("MN_HB_PAIRS", 100)
	cycles := envInt("MN_HB_CYCLES", 1000)

	ps := make([]*pair, pairs)
	var done sync.WaitGroup
	done.Add(2 * pairs)
	start := time.Now()

	for p := range ps {
		x := &pair{latency: make([]int64, 2*cycles)}
		ps[p] = x
		x.a.Lock()
		x.b.Lock()
		var la, lb sync.Locker = &x.a, &x.b
		go side(la, lb, &x.stampA, &x.stampB, x.latency[:cycles], cycles, true, &done)
		go side(lb, la, &x.stampB, &x.stampA, x.latency[cycles:], cycles, false, &done)
		x.stampA = time.Now().UnixNano()
		x.a.Unlock() // a starts the ping-pong
	}

	done.Wait()
	wall := time.Since(start)

	var all []int64
	for _, x := range ps {
		for i, v := range x.latency {
			if i != 0 && v > 0 {
				all = append(all, v)
			}
		}
	}
	sort.Slice(all, func(i, j int) bool { return all[i] < all[j] })
	us := func(v int64) string { return strconv.FormatFloat(float64(v)/1000, 'f', 1, 64) }
	n := len(all)
	fmt.Fprintln(os.Stderr, "HANDBACK samples="+strconv.Itoa(n)+" p50="+us(all[n/2])+" p90="+us(all[n*9/10])+" p99="+us(all[n*99/100])+
		" max="+us(all[n-1])+" us wall-ms="+strconv.FormatInt(wall.Milliseconds(), 10))
	fmt.Println("pairs:", pairs, "cycles:", cycles)
}
