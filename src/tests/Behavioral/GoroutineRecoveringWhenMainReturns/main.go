// Regression test: a goroutine whose panic is being RECOVERED when main returns does not hold main's exit.
//
// wg.Done() runs first among the goroutine's deferred calls, so main returns while the panic is still running
// them, and the recovering call is parked on a channel. Go's runtime.main yields only briefly for such a goroutine
// (1000 Gosched calls on runningPanicDefers), and the process exits 0 before the recovering call prints anything.
// The converted main-return wait (golib's MainExit) once lasted up to a second, long enough to print "recovered"
// after main had returned (C1's review, 3 of 3 runs). It now waits only while the goroutine can run, and this one is
// parked.
//
// The channel is closed by a timer main starts as its LAST statement, so the clock starts at main's own return: the
// only window left is the exit path, against 500 ms. The earlier shape slept 500 ms from wg.Done, a window that
// included main's own wake-up from wg.Wait, which a starved Go process loses as well (the i9, 2026-10-31: Go printed
// "recovered" in 1 of 50 runs at above-normal load).
//
// GoroutinePanicAfterDeferredDone is the other side: the same race with no recovery exits 2 with the report.
package main

import (
	"fmt"
	"sync"
	"time"
)

func main() {
	fmt.Println("before goroutine panic")

	var wg sync.WaitGroup
	wg.Add(1)

	release := make(chan struct{})

	go func() {
		defer func() {
			recover()
			<-release
			fmt.Println("recovered")
		}()
		defer wg.Done()
		panic("recovered later")
	}()

	wg.Wait()

	time.AfterFunc(500*time.Millisecond, func() { close(release) })
}
