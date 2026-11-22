package main

import (
	"fmt"
	"os"
	"strconv"
	"time"
)

// M:N PROTOTYPE (A1). Prototype code: nothing from this branch lands on master as it is.
//
// A recursion n frames deep parks in its last frame. The value it waited for then returns through every frame, and
// the goroutine parks twice more. Under generated twins the return through n suspended frames is n completions,
// each one resuming its caller: the goroutine has to stay on its carrier for all of them, however deep n is, or its
// next park has no carrier to park on.
//
// MN_DEEP_N=<n> runs one depth instead of the list.

func descend(n int, c chan int) int {
	if n == 0 {
		return <-c
	}

	return descend(n-1, c) + 1
}

func round(n int) int {
	c := make(chan int)
	e := make(chan int)
	d := make(chan int)

	go func() {
		v := descend(n, c)
		v += <-e
		d <- v
	}()

	// The goroutine is parked in its last frame before the value is sent, and parked again before the second.
	time.Sleep(10 * time.Millisecond)
	c <- 1
	time.Sleep(10 * time.Millisecond)
	e <- 1

	return <-d
}

func main() {
	depths := []int{1, 16, 256, 1024}

	if s := os.Getenv("MN_DEEP_N"); s != "" {
		n, err := strconv.Atoi(s)

		if err != nil {
			fmt.Println("MN_DEEP_N:", err)
			os.Exit(2)
		}

		depths = []int{n}
	}

	for _, n := range depths {
		// The call is its own statement: a twin does not await inside the arguments of a call that takes a span.
		v := round(n)
		fmt.Printf("depth %d: %d\n", n, v)
	}
}
