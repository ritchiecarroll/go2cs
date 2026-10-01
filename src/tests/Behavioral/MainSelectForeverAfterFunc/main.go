package main

import (
	"fmt"
	"os"
	"time"
)

// Main parks forever with no other goroutine alive, but a timer is still pending. Go's checkdead
// does not report a deadlock while a timer can still fire, and this one's function starts the
// goroutine that ends the program.
func main() {
	time.AfterFunc(500*time.Millisecond, func() {
		fmt.Println("the timer fired while main sat in select {}")
		os.Exit(0)
	})

	select {}
}
