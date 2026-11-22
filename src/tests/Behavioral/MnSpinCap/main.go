// S2 arm 1 of the M:N spike's owed list (docs/phase4/DATA-mn-spike-2.md 9.5): Gosched and spin loops at
// the P cap. More goroutines spin on runtime.Gosched than there are Ps, and two goroutines are parked: a
// waker asleep on a timer, and a sleeper blocked on a channel the waker sends on. Only the sleeper stops
// the spinners, so the program ends only if both parked goroutines get a P while every P is held by a
// spinner that never parks.
//
// Each spinner first receives from the start gate. That receive is a park point, so the spinner has a
// hidden twin and runs on the scheduler under GO2CS_MN=1, as the parked goroutines do; without it the
// spinner would start on a thread of its own and the arm would measure nothing.
//
// MN_SPIN_EXTRA names how many spinners there are beyond the P count (default 4). MN_SPIN_READING=1
// prints the wall time and the spin count to stderr, which the output comparison does not read.
//
// Deterministic by construction: nothing printed depends on the P count, the spin counts or the timing.
package main

import (
	"fmt"
	"os"
	"runtime"
	"strconv"
	"sync"
	"sync/atomic"
	"time"
)

type arm struct {
	start chan struct{} // closed by main: the release gate
	wake  chan int      // unbuffered: the waker's one send to the sleeper

	stop  atomic.Bool  // set by the sleeper once it has run
	spins atomic.Int64 // Gosched calls made by all spinners
	got   atomic.Int64 // what the sleeper received

	done sync.WaitGroup
}

func (a *arm) spinner() {
	defer a.done.Done()
	<-a.start

	var n int64
	for !a.stop.Load() {
		n++
		runtime.Gosched()
	}
	a.spins.Add(n)
}

func (a *arm) waker() {
	defer a.done.Done()
	<-a.start
	time.Sleep(20 * time.Millisecond)
	a.wake <- 42
}

func (a *arm) sleeper() {
	defer a.done.Done()
	v := <-a.wake
	a.got.Store(int64(v))
	a.stop.Store(true)
}

func main() {
	extra := 4
	if s := os.Getenv("MN_SPIN_EXTRA"); s != "" {
		if v, err := strconv.Atoi(s); err == nil && v > 0 {
			extra = v
		}
	}

	procs := runtime.GOMAXPROCS(0)
	spinners := procs + extra

	a := &arm{start: make(chan struct{}), wake: make(chan int)}
	a.done.Add(spinners + 2)

	for i := 0; i < spinners; i++ {
		go a.spinner()
	}
	go a.sleeper()
	go a.waker()

	began := time.Now()
	close(a.start)
	a.done.Wait()
	elapsed := time.Since(began)

	fmt.Println("spinners beyond the P count:", spinners > procs)
	fmt.Println("the sleeper received:", a.got.Load())
	fmt.Println("every spinner stopped:", a.stop.Load())
	fmt.Println("the spinners yielded:", a.spins.Load() > 0)

	if os.Getenv("MN_SPIN_READING") == "1" {
		fmt.Fprintf(os.Stderr, "reading: procs=%d spinners=%d elapsed_ms=%.1f spins=%d\n",
			procs, spinners, float64(elapsed.Microseconds())/1000, a.spins.Load())
	}
}
