package main

import (
	"fmt"
	"os"
	"time"
)

// Go's `go serve(); select {}` main idiom: main parks forever while a worker does the program's
// work. That is not a deadlock while the worker can still run, and here the worker ends the
// program itself.
func main() {
	go func() {
		time.Sleep(500 * time.Millisecond)
		fmt.Println("the worker served while main sat in select {}")
		os.Exit(0)
	}()

	select {}
}
