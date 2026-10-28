package main

import "fmt"

// Visual-parity train seat #27: the four shapes it respells, each run and printed so the converted
// program is compared with Go. F1: immediately-invoked function literals of every arity form; the
// channel directions narrowed at an assignment, a call argument and a return; the two infinite loop
// forms; and compound shifts by literal and non-literal counts.

type count uint8

type holder struct{ bits uint64 }

func recvOnly(c <-chan int) int { return <-c }

func sendOnly(c chan<- int, v int) { c <- v }

func narrowed(ch chan int) <-chan int { return ch }

func loops() int {
	i := 0
	for {
		i++
		if i > 3 {
			break
		}
	}
	for j := 0; ; j++ {
		if j > 2 {
			return i + j
		}
	}
}

func shifts(x uint64, n uint, c count, h *holder) (uint64, count, uint64) {
	x <<= 3
	x >>= 2
	x <<= n
	h.bits <<= 4
	h.bits >>= n
	c <<= 1
	return x, c, h.bits
}

func pick[T any](v T) T {
	return func() T { return v }()
}

func plusOne() int {
	return func() int { return 1 }() + 1
}

func main() {
	v := func() int { return 42 }()
	func() { fmt.Println("void IIFE") }()
	w := func(a int) int { return a * 2 }(5)
	s, err := func() (string, error) { return "s", nil }()
	sum := func(a, b, c int) int { return a + b + c }(1, 2, 3)
	fmt.Println(v, w, s, err, sum, pick(7), pick("seven"), plusOne(), loops())
	fmt.Printf("%T %T %T\n", func() int32 { return 1 }(), func() int { return 1 }(), func() uint8 { return 1 }())

	// Directions: assignment, call argument and return, each read on a legal direction.
	ch := make(chan int, 2)
	var r <-chan int = ch
	var snd chan<- int = ch
	snd <- 9
	fmt.Println("assignment:", <-r)
	sendOnly(ch, 10)
	fmt.Println("argument:", recvOnly(ch))
	ch <- 11
	fmt.Println("return:", <-narrowed(ch))
	fmt.Printf("%T %T\n", r, snd)

	h := &holder{bits: 1}
	fmt.Println(shifts(1, 1, 1, h))
}
