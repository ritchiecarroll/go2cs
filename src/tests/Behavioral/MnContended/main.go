// S2 arm 2 of the M:N spike's owed list (docs/phase4/DATA-mn-spike-2.md 9.5 and 8.6): contended paths with
// every P busy. Three phases run one after another, each with more goroutines than Ps, and every goroutine's
// work ends, so no phase depends on preemption (S2 arm 1, MnSpinCap, measures that):
//
//  1. a contended sync.Mutex: twice the P count of workers each take the lock for a fixed number of
//     increments, doing a little work inside and outside the lock;
//  2. a contended unbuffered channel: twice the P count of senders and of receivers share one channel;
//  3. a select with several ready cases: twice the P count of goroutines each select across four buffered
//     channels of their own, every one holding a value, and refill the channel they took from, so all four
//     cases are ready at every pick.
//
// Every goroutine first receives from its phase's start gate, a park point, so each has a hidden twin and
// runs on the scheduler under GO2CS_MN=1.
//
// Go picks uniformly among a select's ready cases. Phase 3 prints whether every case was picked and whether
// none was starved (each case at least a tenth of every goroutine's picks, where uniform gives a quarter),
// so a select that always takes the first ready case reads false. The cases are ready by construction rather
// than by the senders' timing: with senders on unbuffered channels Go itself starves cases at one P. MN_CONT_READING=1 prints each phase's wall time and the
// per-case pick counts to stderr, which the output comparison does not read.
//
// Deterministic by construction: nothing printed depends on the P count, the order of the picks or the timing.
package main

import (
	"fmt"
	"os"
	"runtime"
	"sync"
	"time"
)

const (
	lockIters = 2000
	chanItems = 2000
	picks     = 10000
)

var reading = os.Getenv("MN_CONT_READING") == "1"

func spin(n int) int {
	x := 0
	for i := 0; i < n; i++ {
		x += i ^ (x >> 3)
	}
	return x
}

func report(phase string, began time.Time) {
	if reading {
		fmt.Fprintf(os.Stderr, "reading: %s elapsed_ms=%.1f\n", phase, float64(time.Since(began).Microseconds())/1000)
	}
}

func contendedMutex(workers int) {
	var mu sync.Mutex
	var wg sync.WaitGroup
	start := make(chan struct{})
	counter := 0
	sink := make([]int, workers)

	for w := 0; w < workers; w++ {
		wg.Add(1)
		go func(w int) {
			defer wg.Done()
			<-start
			for i := 0; i < lockIters; i++ {
				mu.Lock()
				counter++
				sink[w] += spin(20)
				mu.Unlock()
				sink[w] += spin(50)
			}
		}(w)
	}

	began := time.Now()
	close(start)
	wg.Wait()
	report("mutex", began)

	fmt.Println("mutex: every increment counted:", counter == workers*lockIters)
}

func contendedChannel(pairs int) {
	var wg sync.WaitGroup
	start := make(chan struct{})
	ch := make(chan int)
	sums := make([]int, pairs)

	for s := 0; s < pairs; s++ {
		wg.Add(1)
		go func(s int) {
			defer wg.Done()
			<-start
			for i := 1; i <= chanItems; i++ {
				ch <- i
			}
		}(s)
	}

	var rg sync.WaitGroup
	for r := 0; r < pairs; r++ {
		rg.Add(1)
		go func(r int) {
			defer rg.Done()
			<-start
			for i := 0; i < chanItems; i++ {
				sums[r] += <-ch
			}
		}(r)
	}

	began := time.Now()
	close(start)
	wg.Wait()
	rg.Wait()
	report("channel", began)

	total := 0
	for _, v := range sums {
		total += v
	}
	fmt.Println("channel: every item received once:", total == pairs*chanItems*(chanItems+1)/2)
}

func readySelect(selectors int) {
	start := make(chan struct{})
	var wg sync.WaitGroup
	got := make([][4]int, selectors)

	for g := 0; g < selectors; g++ {
		wg.Add(1)
		go func(g int) {
			defer wg.Done()
			var chs [4]chan int
			for c := range chs {
				chs[c] = make(chan int, 1)
				chs[c] <- c
			}
			<-start
			for i := 0; i < picks; i++ {
				var v int
				select {
				case v = <-chs[0]:
				case v = <-chs[1]:
				case v = <-chs[2]:
				case v = <-chs[3]:
				}
				got[g][v]++
				chs[v] <- v
			}
		}(g)
	}

	began := time.Now()
	close(start)
	wg.Wait()
	report("select", began)

	every, fair := true, true
	for g := range got {
		for _, n := range got[g] {
			every = every && n > 0
			fair = fair && n >= picks/10
		}
	}
	if reading {
		fmt.Fprintf(os.Stderr, "reading: select picks[0]=%v\n", got[0])
	}
	fmt.Println("select: every case picked:", every)
	fmt.Println("select: no case starved:", fair)
}

func main() {
	procs := runtime.GOMAXPROCS(0)
	if reading {
		fmt.Fprintf(os.Stderr, "reading: procs=%d\n", procs)
	}

	contendedMutex(2 * procs)
	contendedChannel(2 * procs)
	readySelect(2 * procs)
}
