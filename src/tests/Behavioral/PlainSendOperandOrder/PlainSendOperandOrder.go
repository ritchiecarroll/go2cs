package main

import "fmt"

// A PLAIN send whose value forwards a multi-value call -- `chanFor(ch) <- must(pair())` -- spreads the pair
// into a pre-statement. Go evaluates the channel operand BEFORE the value (calls in lexical order), so a
// channel operand that calls must still run first, in every statement form a send can take.

var log []string

func pair() (int, error) {
	log = append(log, "pair")
	return 7, nil
}

func must(v int, err error) int {
	if err != nil {
		panic(err)
	}
	return v
}

func chanFor(ch chan int) chan int {
	log = append(log, "chan")
	return ch
}

func idx() int {
	log = append(log, "idx")
	return 1
}

func deferredSend(ch chan int) {
	defer func() { chanFor(ch) <- must(pair()) }()
	log = append(log, "body")
}

func main() {
	ch := make(chan int, 8)

	// A calling channel operand.
	chanFor(ch) <- must(pair())
	fmt.Println("call:", log, <-ch)

	// A compound channel operand whose index calls.
	log = nil
	chans := []chan int{nil, ch}
	chans[idx()] <- must(pair())
	fmt.Println("index:", log, <-ch)

	// The same send inside a goroutine's func literal.
	log = nil
	done := make(chan bool)
	go func() {
		chanFor(ch) <- must(pair())
		done <- true
	}()
	<-done
	fmt.Println("go:", log, <-ch)

	// ... and inside a deferred func literal.
	log = nil
	deferredSend(ch)
	fmt.Println("defer:", log, <-ch)

	// CONTROL: a bare channel operand has no ordering obligation and stays inline.
	log = nil
	ch <- must(pair())
	fmt.Println("bare:", log, <-ch)
}
