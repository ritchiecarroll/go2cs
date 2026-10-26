// ParkStorm parks N goroutines on one channel at the same moment, holds them parked, then releases them.
// It is the demonstrator of docs/DESIGN-mn-scheduler-sizing.md (Appendix A): run it as
//
//	parkstorm <N> <hold seconds>
//
// while a harness samples the process's private memory and OS threads (park-storm.ps1 beside this folder).
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
	step := n / 10
	if step == 0 {
		step = 1
	}

	for i := 0; i < n; i++ {
		go func() {
			started.Done()
			<-release
			finished.Done()
		}()
		if (i+1)%step == 0 {
			fmt.Printf("STARTED %d in %.2fs\n", i+1, time.Since(t0).Seconds())
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
