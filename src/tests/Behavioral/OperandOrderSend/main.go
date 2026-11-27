// Go's order in a channel send: gc reads a plain operand of the value after the send's calls,
// including a call that yields the channel.
package main

import "fmt"

func main() {
	c := make(chan int, 1)

	w := 1
	getC := func() chan int { w = 10; return c }
	getC() <- w
	fmt.Println("channel from a call:", <-c)

	v := 1
	setV := func() int { v = 10; return 2 }
	c <- v + setV()
	fmt.Println("value with a call:", <-c)

	u := 1
	setU := func() int { u = 10; return 2 }
	select {
	case c <- u + setU():
	}
	fmt.Println("select send:", <-c)
}
