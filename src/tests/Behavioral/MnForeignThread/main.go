// S2 arm 3 of the M:N scheduler's owed list (docs/phase4/DATA-mn-spike-2.md 9.5): callbacks on foreign threads. A
// foreign thread is one the scheduler did not make: a C# caller's own thread (the record's M3) or a thread that
// calls into Go from C. It has no carrier, no P and no goroutine identity (the native-call seam's gap M7).
//
// foreign runs f on such a thread. Under Go it is nil, and runForeign falls back to a goroutine locked to its
// thread, which is what a cgo callback runs on. hand_impl.cs sets it, before main, to a delegate that runs f on
// a new System.Threading.Thread, so the converted program runs f's plain body on a thread the scheduler has
// never seen. Six rows, each against goroutines that have twins (each parks), so they run on the scheduler
// under GO2CS_MN=1:
//
//	F1 a foreign thread blocks receiving, and a goroutine sends;
//	F2 a goroutine parks receiving, and a foreign thread sends;
//	F3 a foreign thread starts goroutines, which park, and waits for them;
//	F4 a foreign thread locks a mutex held by a goroutine parked on a channel, and a third goroutine lets it go;
//	F5 four foreign threads per P each make round trips through a goroutine of their own, while one goroutine
//	   per P runs a bounded stretch of work that never parks;
//	F6 f panics and recovers on the foreign thread.
//
// MN_FOREIGN_ONLY=F<n> runs one row alone, for a scheduler reading of that row; the output comparison runs all
// six. MN_FOREIGN_READING=1 prints each row's wall time to stderr, which the output comparison does not read.
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

// foreign runs f on a thread the scheduler did not make. nil under Go; hand_impl.cs sets it in the conversion.
var foreign func(f func())

var reading = os.Getenv("MN_FOREIGN_READING") == "1"

func runForeign(f func()) {
	if foreign != nil {
		foreign(f)
		return
	}
	go func() {
		runtime.LockOSThread()
		defer runtime.UnlockOSThread()
		f()
	}()
}

func report(row string, began time.Time) {
	if reading {
		fmt.Fprintf(os.Stderr, "reading: %s elapsed_ms=%.1f\n", row, float64(time.Since(began).Microseconds())/1000)
	}
}

func f1() {
	ch := make(chan int)
	done := make(chan int)
	began := time.Now()
	runForeign(func() {
		done <- <-ch
	})
	go func() {
		time.Sleep(5 * time.Millisecond)
		ch <- 7
	}()
	v := <-done
	report("F1", began)
	fmt.Println("F1 the foreign receiver got:", v)
}

func f2() {
	ch := make(chan int)
	got := make(chan int)
	began := time.Now()
	go func() {
		got <- <-ch
	}()
	runForeign(func() {
		time.Sleep(5 * time.Millisecond)
		ch <- 9
	})
	v := <-got
	report("F2", began)
	fmt.Println("F2 the parked goroutine got:", v)
}

func worker(gate chan struct{}, wg *sync.WaitGroup, n *int, mu *sync.Mutex) {
	defer wg.Done()
	<-gate
	mu.Lock()
	*n++
	mu.Unlock()
}

func f3() {
	const workers = 100
	done := make(chan int)
	began := time.Now()
	runForeign(func() {
		var wg sync.WaitGroup
		var mu sync.Mutex
		n := 0
		gate := make(chan struct{})
		for i := 0; i < workers; i++ {
			wg.Add(1)
			go worker(gate, &wg, &n, &mu)
		}
		close(gate)
		wg.Wait()
		done <- n
	})
	n := <-done
	report("F3", began)
	fmt.Println("F3 goroutines started on a foreign thread finished:", n)
}

func f4() {
	var mu sync.Mutex
	held := make(chan struct{})
	release := make(chan struct{})
	done := make(chan bool)
	began := time.Now()
	go func() {
		mu.Lock()
		close(held)
		<-release
		mu.Unlock()
	}()
	runForeign(func() {
		<-held
		mu.Lock()
		done <- true
		mu.Unlock()
	})
	go func() {
		<-held
		time.Sleep(5 * time.Millisecond)
		close(release)
	}()
	ok := <-done
	report("F4", began)
	fmt.Println("F4 the foreign locker got the mutex:", ok)
}

func echo(req, resp chan int) {
	for v := range req {
		resp <- v + 1
	}
	close(resp)
}

func spin(n int) int {
	x := 0
	for i := 0; i < n; i++ {
		x += i ^ (x >> 3)
	}
	return x
}

func f5() {
	const trips = 1000
	procs := runtime.GOMAXPROCS(0)
	threads := 4 * procs
	results := make(chan bool, threads)
	startBusy := make(chan struct{})
	sinks := make(chan int, procs)
	var busy sync.WaitGroup
	began := time.Now()

	for p := 0; p < procs; p++ {
		busy.Add(1)
		go func() {
			defer busy.Done()
			<-startBusy
			sink := 0
			for i := 0; i < 2000; i++ {
				sink += spin(20000)
			}
			sinks <- sink
		}()
	}
	close(startBusy)

	for t := 0; t < threads; t++ {
		req, resp := make(chan int), make(chan int)
		go echo(req, resp)
		runForeign(func() {
			ok := true
			for i := 0; i < trips; i++ {
				req <- i
				ok = ok && <-resp == i+1
			}
			close(req)
			_, open := <-resp
			results <- ok && !open
		})
	}

	all := true
	for t := 0; t < threads; t++ {
		all = all && <-results
	}
	busy.Wait()
	report("F5", began)
	fmt.Println("F5 every round trip answered:", all)
}

func f6() {
	res := make(chan string)
	began := time.Now()
	runForeign(func() {
		defer func() {
			res <- fmt.Sprint(recover())
		}()
		panic("foreign boom")
	})
	r := <-res
	report("F6", began)
	fmt.Println("F6 recovered on the foreign thread:", r)
}

// runs reports whether MN_FOREIGN_ONLY selects row. main calls each row directly: a call through a func value
// would leave main without a twin, and the rows would then start from a thread rather than the scheduler.
func runs(row string) bool {
	only := os.Getenv("MN_FOREIGN_ONLY")
	return only == "" || only == row
}

func main() {
	if runs("F1") {
		f1()
	}
	if runs("F2") {
		f2()
	}
	if runs("F3") {
		f3()
	}
	if runs("F4") {
		f4()
	}
	if runs("F5") {
		f5()
	}
	if runs("F6") {
		f6()
	}
}
