// Regression test: a goroutine that panics AFTER its deferred wg.Done() must still crash the whole
// process like Go -- the panic report on stderr and exit code 2 -- never exit 0 with an empty stderr.
//
// The race: wg.Done() runs as one of the panicking goroutine's deferred calls, so wg.Wait() returns
// and main returns while that goroutine is still running its deferred calls and has not reported yet.
// Go's runtime.main does not exit at once: while any goroutine runs a panic's deferred calls
// (runningPanicDefers) it yields, and once one is printing its report (panicking) it parks, so the
// dying goroutine prints and exits 2 (runtime/proc.go, "Make racy client program work"; issues 3934
// and 20018). The yield is bounded, though: at most 1000 Gosched calls (go1.26 proc.go:310). With
// several Ps, a goroutine whose thread the OS deschedules can miss that window, and Go exits 0.
//
// runtime.GOMAXPROCS(1) closes the window on Go's side. With one P, the panicking goroutine keeps the
// P after wg.Done() wakes main, and each of main's Gosched calls hands the P back to it, so the report
// always begins before main's wait ends. golib's GOMAXPROCS only records the setting (goroutines are
// CLR threads), so the converted program still returns from main mid-panic and exercises golib's
// main-return tail (MainExit), which is what this test guards. That tail waits while the panicking
// goroutine can run, up to 20 ms of its own CPU, so CPU contention delays the report rather than losing
// it (docs/phase4/DESIGN-main-return-wait.md).
//
// GoroutinePanicExitCode is the control: a goroutine panic with main blocked, so no race at all.
// Nothing prints after wg.Wait(): in Go that line races the crash.
package main

import (
	"fmt"
	"runtime"
	"sync"
)

func main() {
	runtime.GOMAXPROCS(1)

	fmt.Println("before goroutine panic")

	var wg sync.WaitGroup
	wg.Add(1)

	go func() {
		defer wg.Done()
		panic("panic after deferred Done")
	}()

	wg.Wait()
}
