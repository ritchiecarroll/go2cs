package main

import (
	"fmt"
	"sync"
	"time"
)

// A WaitGroup whose count is raised again before its woken waiters have returned. Go's Wait re-reads the state after
// its wake and panics when it is not zero: "sync: WaitGroup is reused before previous Wait has returned".
//
// Each round, eight goroutines wait on a count of 1 and report what their Wait panicked with ("" when it returned).
// Once all eight have said they are about to wait, main sleeps, then calls Done and Add(1) with nothing in between,
// collects the eight reports, and lowers the count again.
//
// The reuse is a race: a waiter that reads the state before the Add returns normally. So the program does not print
// how many waiters panicked. It prints how many reported, whether any report was the panic, and the panic's text.
// Twenty rounds of eight waiters each would all have to lose that race for the second line to read false.
//
// A waiter that reaches Wait after the Add would block until the last Done, and main would wait for its report. The
// timer lowers the count early in that case, so a slow start cannot hang the program; it does not change a line.
const (
	rounds  = 20
	waiters = 8
)

func main() {
	reports := 0
	panicked := false
	text := ""

	for round := 0; round < rounds; round++ {
		var wg sync.WaitGroup
		wg.Add(1)

		starting := make(chan struct{}, waiters)
		report := make(chan string, waiters)

		for i := 0; i < waiters; i++ {
			go func() {
				defer func() {
					if x := recover(); x != nil {
						report <- fmt.Sprint(x)
						return
					}

					report <- ""
				}()

				starting <- struct{}{}
				wg.Wait()
			}()
		}

		for i := 0; i < waiters; i++ {
			<-starting
		}

		time.Sleep(10 * time.Millisecond)

		wg.Done()
		wg.Add(1)

		lowered := false
		late := time.After(500 * time.Millisecond)

		for got := 0; got < waiters; {
			select {
			case s := <-report:
				got++
				reports++

				if s != "" {
					panicked = true
					text = s
				}
			case <-late:
				lowered = true
				wg.Done()
			}
		}

		if !lowered {
			wg.Done()
		}
	}

	fmt.Println("reports:", reports)
	fmt.Println("a waiter panicked:", panicked)
	fmt.Println(text)
}
