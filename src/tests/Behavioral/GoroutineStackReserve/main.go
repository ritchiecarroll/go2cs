// The two goroutine stack reservations (docs/DESIGN-mn-scheduler-sizing.md, option A).
//
// Go grows a goroutine's stack; a converted goroutine runs on a .NET thread whose stack is a fixed
// reservation, and overflowing it ends the process. So the converter proves which goroutines cannot
// recurse and starts only those on a small reservation (goǃ(…, GoStack.Bounded)); every other
// goroutine keeps the Go-scale one. This program holds both sides:
//   - a goroutine that recurses 200,000 levels deep must still have room to do it, as in Go;
//   - goroutines that only send on a channel are bounded and all complete.
//
// Deterministic: results arrive through channels and are summed or printed in a fixed order.
package main

import "fmt"

const depth = 200_000

// depthOf recurses n levels, so its goroutine is not bounded and keeps the default reservation.
func depthOf(n int, acc [4]int) int {
	if n == 0 {
		return acc[0] + acc[1] + acc[2] + acc[3]
	}

	acc[n%4]++
	return depthOf(n-1, acc)
}

// produce makes only static calls and sends, so its goroutine is bounded.
func produce(id int, out chan<- int) {
	out <- square(id)
}

func square(n int) int { return n * n }

func main() {
	deep := make(chan int)

	go func() {
		deep <- depthOf(depth, [4]int{})
	}()

	fmt.Println("deep recursion levels:", <-deep)

	const workers = 100
	out := make(chan int, workers)

	for i := 1; i <= workers; i++ {
		go produce(i, out)
	}

	sum := 0
	for range workers {
		sum += <-out
	}

	fmt.Println("bounded goroutines:", workers, "sum of squares:", sum)
}
