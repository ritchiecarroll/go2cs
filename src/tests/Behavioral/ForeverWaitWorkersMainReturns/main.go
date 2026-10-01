package main

import (
	"fmt"
	"time"
)

// A goroutine blocked forever -- in `select {}`, or on a send or a receive on a nil channel -- is
// parked, not a deadlock: Go reports "all goroutines are asleep" only when nothing can run again.
// Here main keeps running, so the three parked goroutines are simply left behind when main returns
// and the process exits around them.
func main() {
	var nilc chan int

	go func() { select {} }()
	go func() { <-nilc }()
	go func() { nilc <- 1 }()

	time.Sleep(500 * time.Millisecond)

	fmt.Println("main returned with three goroutines parked forever")
}
