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
// runtime.GOMAXPROCS(1) closes what is left of the window on Go's side. With several Ps, another thread runs the
// timer while the OS has main's thread descheduled, and Go printed "recovered" in 3 of 100 runs under 32 spinners
// (the i9, 2026-11-01). With one P, main's thread holds it through the exit path, so the timer cannot fire unless main
// itself is starved for 500 ms inside a few microseconds of work. golib's GOMAXPROCS only records the setting
// (goroutines are CLR threads), so the converted side is unchanged.
//
// GoroutinePanicAfterDeferredDone is the other side: the same race with no recovery exits 2 with the report.
package main

import (
	"fmt"
	"runtime"
	"sync"
	"time"
)

func main() {
	runtime.GOMAXPROCS(1)

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
