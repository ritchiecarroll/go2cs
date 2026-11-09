// ParkStorm parks N goroutines on one channel, holds them, then releases them: the M:N spike's demonstrator
// (docs/DESIGN-mn-scheduler-sizing.md Appendix A, with its per-tenth progress logging restored, so a run stopped by
// the harness still reports how far it got). Usage: parkstorm <n> <holdSeconds>
package main

import (
	"fmt"
	"os"
	"runtime"
	"strconv"
	"sync"
	"time"
)

func main() {
	n, _ := strconv.Atoi(os.Args[1])
	hold, _ := strconv.Atoi(os.Args[2])
	release := make(chan struct{})
	var started, finished sync.WaitGroup
	started.Add(n)
	finished.Add(n)

	t0 := time.Now()
	tenth := n / 10
	if tenth == 0 {
		tenth = 1
	}

	for i := 0; i < n; i++ {
		go func() {
			started.Done()
			<-release
			finished.Done()
		}()

		if (i+1)%tenth == 0 {
			fmt.Printf("STARTED %d of %d in %.2fs\n", i+1, n, time.Since(t0).Seconds())
		}
	}

	started.Wait()
	fmt.Printf("PARKED %d goroutines=%d in %.2fs\n", n, runtime.NumGoroutine(), time.Since(t0).Seconds())

	time.Sleep(time.Duration(hold) * time.Second) // the harness samples memory and threads here

	t1 := time.Now()
	close(release)
	finished.Wait()
	fmt.Printf("RELEASED %d in %.2fs\n", n, time.Since(t1).Seconds())
}
