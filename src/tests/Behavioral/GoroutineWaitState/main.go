// What a goroutine traceback says a BLOCKED goroutine is waiting for.
//
// runtime.Stack(buf, all=true) prints one `goroutine N [<state>]:` header per live goroutine, and the
// bracketed word is Go's wait reason: the runtime publishes it at every gopark and a traceback reads
// it back. Go's own tests grep exactly this — runtime/pprof's awaitBlockedGoroutine builds a regex
// around `^goroutine \d+ \[sync\.Mutex\.Lock\]:` — so the words are a contract, not a diagnostic.
//
// This program parks one goroutine on each of four different Go primitives, then reads the four
// reasons back out of the dump. It is the guard for go2cs's park accounting
// (docs/phase4/DESIGN-cooperative-scheduler.md §5.3): each blocking operation names its wait, the
// runtime records it, and Stack renders it in Go's own vocabulary.
//
// THE OUTPUT IS NORMALIZED, and that is the whole trick. A raw dump can never be compared between
// two runtimes — goroutine ids, frame counts, file paths and PC offsets all differ, and go2cs
// cannot walk another goroutine's stack at all, so it prints a placeholder where Go prints frames.
// What both runtimes CAN be held to is which reasons are present, so that is all this prints:
// one `<reason>: <bool>` line per reason, in a fixed order, from a dump neither side shows.
//
// Three arms, so the guard can go red in every direction that matters:
//
//   phase 1 (positive) — the four reasons the parked goroutines must report, plus `running` for the
//                        goroutine calling Stack. If the accounting were missing these read false.
//   phase 1 (negative) — `chan send`, a reason NO goroutine in this program ever has. If the dump
//                        printed reasons indiscriminately this would read true.
//   phase 2 (release)  — the same four reasons after every goroutine is unblocked and finished. A
//                        reason that were set but never CLEARED would still read true here.
package main

import (
	"fmt"
	"runtime"
	"strings"
	"sync"
	"time"
)

// Wide enough that the dump is never truncated: the runtimes differ in how many internal goroutines
// they carry, and a truncated dump would fail as a missing reason rather than as what it is.
const dumpSize = 1 << 18

// Poll budget for a reason to appear. Every park below happens within microseconds of its goroutine
// starting; this is slack for a loaded machine, not a timing dependency. Exhausting it prints false
// and fails the comparison, which is the correct — and diagnosable — outcome.
const pollLimit = 2000

// The reasons this program's goroutines park on, in Go's own spelling. `semacquire` rather than any
// name containing WaitGroup: Go's WaitGroup.Wait calls runtime_Semacquire, which parks with
// waitReasonSemacquire, so `semacquire` is what a Go traceback prints for a blocked Wait.
var parked = []string{
	"sync.Mutex.Lock",
	"chan receive",
	"select",
	"semacquire",
}

// present reports whether a goroutine header for `state` appears in the dump. The closing bracket is
// load-bearing: without it "select" would also match "select (no cases)" and "chan receive" would
// also match "chan receive (nil chan)", which are different waits with different reasons.
func present(dump string, state string) bool {
	return strings.Contains(dump, "["+state+"]:")
}

// dump renders the whole-process traceback as a string.
func dump(buf []byte) string {
	return string(buf[:runtime.Stack(buf, true)])
}

// await polls the traceback until `want` reports `sense` for every reason in it, or the budget runs
// out. It returns the last dump either way, so the caller reports what is actually there rather than
// what the loop was hoping for.
func await(buf []byte, want []string, sense bool) string {
	var d string

	for i := 0; i < pollLimit; i++ {
		d = dump(buf)
		ok := true

		for _, state := range want {
			if present(d, state) != sense {
				ok = false
				break
			}
		}

		if ok {
			return d
		}

		// Gosched first (Go's own awaitBlockedGoroutine does the same), then yield the core: a
		// goroutine that has not been scheduled yet has not reached its park.
		runtime.Gosched()
		time.Sleep(time.Millisecond)
	}

	return d
}

func main() {
	buf := make([]byte, dumpSize)

	// Held by main for the whole of phase 1, so the goroutine below cannot acquire it.
	var mu sync.Mutex
	mu.Lock()

	// Never sent to during phase 1: a receive on it is a real park.
	recv := make(chan int)

	// TWO cases, deliberately: a single-case select is compiled to the bare channel operation, so it
	// would park as `chan receive` and this arm would prove nothing about select at all.
	selA := make(chan int)
	selB := make(chan int)

	// Never Done during phase 1.
	var wg sync.WaitGroup
	wg.Add(1)

	// Each goroutine reports its own completion, so phase 2 waits for the release to actually land
	// rather than for a duration.
	done := make(chan string, len(parked))

	go func() {
		mu.Lock()
		mu.Unlock()
		done <- "mutex"
	}()

	go func() {
		<-recv
		done <- "recv"
	}()

	go func() {
		select {
		case <-selA:
		case <-selB:
		}
		done <- "select"
	}()

	go func() {
		wg.Wait()
		done <- "waitgroup"
	}()

	// ---- phase 1: every parked reason is reported, and only the ones that are real --------------

	blocked := await(buf, parked, true)

	for _, state := range parked {
		fmt.Printf("parked %s: %v\n", state, present(blocked, state))
	}

	// The goroutine calling Stack is running, in both runtimes and by definition.
	fmt.Printf("parked running: %v\n", present(blocked, "running"))

	// The negative arm: nothing in this program ever blocks on a send.
	fmt.Printf("parked chan send: %v\n", present(blocked, "chan send"))

	// ---- phase 2: releasing every goroutine clears every reason ---------------------------------

	mu.Unlock()
	close(recv)
	close(selA)
	wg.Done()

	for range parked {
		<-done
	}

	released := await(buf, parked, false)

	for _, state := range parked {
		fmt.Printf("released %s: %v\n", state, present(released, state))
	}

	fmt.Printf("released running: %v\n", present(released, "running"))
}
