// spin: N busy goroutines for D seconds (CPU load for the race measurements).
package main

import (
	"os"
	"strconv"
	"sync"
	"time"
)

func main() {
	n, _ := strconv.Atoi(os.Args[1])
	d, _ := strconv.Atoi(os.Args[2])
	end := time.Now().Add(time.Duration(d) * time.Second)
	var wg sync.WaitGroup
	for i := 0; i < n; i++ {
		wg.Add(1)
		go func() {
			defer wg.Done()
			x := 0
			for time.Now().Before(end) {
				for j := 0; j < 100000; j++ {
					x += j
				}
			}
			_ = x
		}()
	}
	wg.Wait()
}
