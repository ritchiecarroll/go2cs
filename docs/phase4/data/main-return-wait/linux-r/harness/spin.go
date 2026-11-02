// spin <n> <seconds>: n goroutines busy-looping for the given time (the leg's "busy loops").
package main

import (
	"os"
	"strconv"
	"time"
)

func main() {
	n, _ := strconv.Atoi(os.Args[1])
	s, _ := strconv.Atoi(os.Args[2])
	for i := 0; i < n; i++ {
		go func() {
			x := 0
			for {
				x++
			}
		}()
	}
	time.Sleep(time.Duration(s) * time.Second)
}
