// The M:N spike's mixed storm (docs/phase4/DATA-mn-spike-2.md): n goroutines that are all alive at
// once, and each one parks four ways before it finishes.
//
//   - park 1, a select over two channels: a token from main, or the start gate closing
//   - park 2, a timer: time.Sleep
//   - park 3, a contended sync.Mutex
//   - park 4, an unbuffered send that main receives
//
// Half the goroutines start from a named method and half from a func literal that calls it, because
// the two take different routes to a hidden twin.
//
// n is 100,000 unless MN_STORM_N names another count. With one OS thread per goroutine that is past
// the thread cap of both hosted macOS runners (2,048 and 3,072), which is the reason the program
// exists. MN_STORM_HOLD_MS holds the parked goroutines so a sampler can read the process.
//
// Deterministic by construction: ids and tokens are summed, never printed in arrival order, and every
// cross-goroutine ordering comes from the WaitGroups and the channel operations. The sleep delays a
// goroutine; nothing is decided by it.
package main

import (
	"fmt"
	"os"
	"strconv"
	"sync"
	"time"
)

type storm struct {
	mu      sync.Mutex
	counter int

	start   chan struct{} // closed by main: the release gate
	tokens  chan int      // unbuffered: each token is taken by exactly one goroutine
	results chan int      // unbuffered: every send is a rendezvous with main

	arrived sync.WaitGroup
	done    sync.WaitGroup
}

func (s *storm) worker(id int) {
	defer s.done.Done()
	s.arrived.Done()

	got := 0

	select { // park 1
	case <-s.start:
	case t := <-s.tokens:
		got = t
		<-s.start
	}

	time.Sleep(time.Millisecond) // park 2

	s.mu.Lock() // park 3, under contention
	s.counter += id
	s.mu.Unlock()

	s.results <- id + got // park 4
}

func envInt(name string, fallback int) int {
	if v, err := strconv.Atoi(os.Getenv(name)); err == nil && v > 0 {
		return v
	}

	return fallback
}

func main() {
	n := envInt("MN_STORM_N", 100000)
	hold := envInt("MN_STORM_HOLD_MS", 0)

	s := &storm{
		start:   make(chan struct{}),
		tokens:  make(chan int),
		results: make(chan int),
	}

	s.arrived.Add(n)
	s.done.Add(n)

	for i := 0; i < n; i++ {
		if i%2 == 0 {
			go s.worker(i)
		} else {
			go func() {
				s.worker(i)
			}()
		}
	}

	// Cannot return until all n goroutines exist and have run: each one announces, then parks.
	s.arrived.Wait()
	fmt.Println("parked:", n)

	if hold > 0 {
		time.Sleep(time.Duration(hold) * time.Millisecond)
	}

	// A tenth of the goroutines leave the select through the token case, the rest through the gate.
	tokens := n / 10

	for i := 0; i < tokens; i++ {
		s.tokens <- 1
	}

	close(s.start)

	total := 0

	for i := 0; i < n; i++ {
		total += <-s.results
	}

	s.done.Wait()

	want := n * (n - 1) / 2

	fmt.Println("tokens:", tokens)
	fmt.Println("counter:", s.counter, "match:", s.counter == want)
	fmt.Println("total:", total, "match:", total == want+tokens)
}
