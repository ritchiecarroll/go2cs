// Regression test: a goroutine whose panic is being RECOVERED when main returns does not hold main's exit.
//
// wg.Done() runs first among the goroutine's deferred calls, so main returns while the panic is still running
// them. Go's runtime.main yields only briefly for such a goroutine (1000 Gosched calls on runningPanicDefers), and
// the process exits 0 before the recovering call's slow work prints anything. The converted main-return wait
// (golib's MainExit) used to last up to a second, long enough to print "recovered" after main had returned (C1's
// review, 3 of 3 runs); it is now as short as Go's in spirit.
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

	go func() {
		defer func() {
			recover()
			time.Sleep(500 * time.Millisecond)
			fmt.Println("recovered")
		}()
		defer wg.Done()
		panic("recovered later")
	}()

	wg.Wait()
}
