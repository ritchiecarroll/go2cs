// Regression test: a goroutine that panics AFTER its deferred wg.Done() must still crash the whole
// process like Go -- the panic report on stderr and exit code 2 -- never exit 0 with an empty stderr.
//
// The race: wg.Done() runs as one of the panicking goroutine's deferred calls, so wg.Wait() returns
// and main returns while that goroutine is still running its deferred calls and has not reported yet.
// Go's runtime.main does not exit then: it waits while any goroutine runs a panic's deferred calls
// (runningPanicDefers) and parks once one is printing its report (panicking), so the report always
// wins (runtime/proc.go, "Make racy client program work"; issues 3934 and 20018). Converted, main
// returned first and the process exited 0 with nothing on stderr (found by P1, 5 of 5 runs).
//
// GoroutinePanicExitCode is the control: a goroutine panic with main blocked, so no race at all.
// Nothing prints after wg.Wait(): in Go that line races the crash.
package main

import (
	"fmt"
	"sync"
)

func main() {
	fmt.Println("before goroutine panic")

	var wg sync.WaitGroup
	wg.Add(1)

	go func() {
		defer wg.Done()
		panic("panic after deferred Done")
	}()

	wg.Wait()
}
