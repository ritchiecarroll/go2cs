// MnGoMaxProcs: the M:N scheduler's P count follows GOMAXPROCS, as Go's does. runtime.GOMAXPROCS(n) returns the
// previous setting and n < 1 only reads it; between settings, goroutines that park on channels and a WaitGroup all
// finish under one P and under three. The output is the same with the scheduler off (a goroutine is a thread and
// the setting caps nothing) and on (GO2CS_MN=1 with twins, where the setting is the scheduler's P count).
package main

import (
	"fmt"
	"runtime"
	"sync"
)

func main() {
	start := runtime.GOMAXPROCS(0)
	fmt.Println("start positive:", start > 0)

	fmt.Println("set 2 returns the start:", runtime.GOMAXPROCS(2) == start)
	fmt.Println("read:", runtime.GOMAXPROCS(0))
	fmt.Println("read with -1:", runtime.GOMAXPROCS(-1))

	fmt.Println("set 1 returns:", runtime.GOMAXPROCS(1))
	fmt.Println("sum under 1:", work(64))

	fmt.Println("set 3 returns:", runtime.GOMAXPROCS(3))
	fmt.Println("sum under 3:", work(64))

	fmt.Println("restore returns:", runtime.GOMAXPROCS(start))
	fmt.Println("restored:", runtime.GOMAXPROCS(0) == start)
}

// work starts n goroutines that each send one square on an unbuffered channel, and sums them.
func work(n int) int {
	results := make(chan int)
	var wg sync.WaitGroup

	for i := 0; i < n; i++ {
		wg.Add(1)
		go func(i int) {
			defer wg.Done()
			results <- i * i
		}(i)
	}

	go func() {
		wg.Wait()
		close(results)
	}()

	sum := 0
	for r := range results {
		sum += r
	}

	return sum
}
