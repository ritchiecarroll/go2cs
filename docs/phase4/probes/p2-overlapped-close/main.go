// Command OverlappedClose reads the two windows defects the net row met in TestVariousDeadlines.
//
//	OverlappedClose race       sockets opened and closed on many goroutines at once, so a pooled
//	                           operation's record is retired by a submit on one socket while its old
//	                           socket's Close retires it too (the "Handle is not initialized" crash)
//	OverlappedClose deadlines  testVariousDeadlines' client loop, with the step each run is on, and a
//	                           watchdog that names the step a run is stuck in (the 750ns hang). A run
//	                           stuck in its read for 3 s has its deadline set again, in the PAST: a
//	                           read that then returns was waiting for a timer that never expired it;
//	                           one that does not is stuck after the deadline (the cancel and harvest)
package main

import (
	"fmt"
	"io"
	"net"
	"os"
	"runtime"
	"sync"
	"sync/atomic"
	"time"
)

func main() {
	if len(os.Args) < 2 {
		fmt.Println("usage: OverlappedClose race|deadlines")
		os.Exit(2)
	}

	switch os.Args[1] {
	case "race":
		race()
	case "deadlines":
		deadlines()
	default:
		fmt.Println("unknown mode", os.Args[1])
		os.Exit(2)
	}
}

// listen starts a server whose handler waits for the client to close each connection, as
// testVariousDeadlines' does.
func listen() net.Listener {
	ln, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		panic(err)
	}

	go func() {
		for {
			c, err := ln.Accept()
			if err != nil {
				return
			}
			go func() {
				c.Read(make([]byte, 1))
				c.Close()
			}()
		}
	}()

	return ln
}

func race() {
	const workers, rounds = 16, 200

	var wg sync.WaitGroup
	var timeouts atomic.Int64

	start := time.Now()

	for w := 0; w < workers; w++ {
		wg.Add(1)
		go func() {
			defer wg.Done()
			for r := 0; r < rounds; r++ {
				ln := listen()
				c, err := net.Dial("tcp", ln.Addr().String())
				if err != nil {
					panic(err)
				}
				c.SetDeadline(time.Now().Add(time.Duration(r%50) * time.Microsecond))
				if _, err := c.Read(make([]byte, 1)); err != nil {
					if ne, ok := err.(net.Error); ok && ne.Timeout() {
						timeouts.Add(1)
					}
				}
				c.Close()
				ln.Close()
			}
		}()
	}

	wg.Wait()
	fmt.Printf("race: %d socket pairs closed, %d reads timed out, %v\n", workers*rounds, timeouts.Load(), time.Since(start).Round(time.Millisecond))
}

func deadlines() {
	defer runtime.GOMAXPROCS(runtime.GOMAXPROCS(1))

	ln := listen()
	defer ln.Close()

	var step, conn atomic.Value
	var progress atomic.Int64
	step.Store("start")

	go func() {
		last, still := int64(-1), 0
		for range time.Tick(time.Second) {
			now := progress.Load()
			if now != last {
				last, still = now, 0
				continue
			}
			still++
			if still == 3 {
				fmt.Printf("STUCK 3 s at %v: setting the deadline again, in the past\n", step.Load())
				if c, ok := conn.Load().(net.Conn); ok {
					c.SetReadDeadline(time.Now().Add(-time.Second))
				}
			}
			if still == 6 {
				fmt.Printf("STILL STUCK 3 s after a past deadline, at %v: the read is past its deadline wait\n", step.Load())
				os.Exit(3)
			}
		}
	}()

	timeouts := []time.Duration{1, 2, 5, 50, 100, 200, 500, 750, 1000, 5000, 25000, 250000, 500000}
	slow := map[time.Duration]int{}

	for pass := 0; pass < 20; pass++ {
		for _, timeout := range timeouts {
			for run := 0; run < 3; run++ {
				name := fmt.Sprintf("pass %d %v %d/3", pass, timeout, run)
				progress.Add(1)

				step.Store(name + " dial")
				c, err := net.Dial("tcp", ln.Addr().String())
				if err != nil {
					panic(err)
				}

				conn.Store(c)

				step.Store(name + " setdeadline")
				t0 := time.Now()
				if err := c.SetDeadline(t0.Add(timeout)); err != nil {
					panic(err)
				}

				step.Store(name + " copy")
				n, err := io.Copy(io.Discard, c)
				dt := time.Since(t0)

				if dt > 3*time.Second {
					fmt.Printf("UNSTUCK %s by the past deadline after %v: its armed deadline never expired the read\n", name, dt.Round(time.Millisecond))
					os.Exit(4)
				}

				step.Store(name + " close")
				c.Close()

				if ne, ok := err.(net.Error); !ok || !ne.Timeout() {
					fmt.Printf("%s: Copy = %d, %v; want timeout\n", name, n, err)
					os.Exit(1)
				}
				if dt > 100*time.Millisecond+timeout {
					slow[timeout]++
				}
			}
		}
	}

	fmt.Printf("deadlines: %d runs, all timed out; runs over 100 ms past their deadline: %v\n", 20*len(timeouts)*3, slow)
}
