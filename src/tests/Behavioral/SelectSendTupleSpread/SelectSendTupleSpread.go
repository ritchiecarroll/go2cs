package main

import "fmt"

// A select SEND whose value forwards a multi-value call -- `case ch <- must(pair()):` -- must spread the
// pair into must's two parameters exactly as a plain send statement does. Go evaluates a select's
// channel and send-value operands on entry, in source order: the channel operand BEFORE the value.

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

func main() {
	ch := make(chan int, 4)

	// A lone send case.
	select {
	case ch <- must(pair()):
		fmt.Println("sent alone")
	}

	// Beside a receive case that can never be ready, so the send is the one chosen.
	never := make(chan int)
	select {
	case v := <-never:
		fmt.Println("impossible", v)
	case ch <- must(pair()):
		fmt.Println("sent beside a receive")
	}

	// The channel operand has a side effect: it must run before the value's call.
	log = nil
	select {
	case chanFor(ch) <- must(pair()):
		fmt.Println("order:", log)
	}

	// CONTROL: the plain send statement, which already spreads.
	ch <- must(pair())

	fmt.Println(<-ch, <-ch, <-ch, <-ch)
}
