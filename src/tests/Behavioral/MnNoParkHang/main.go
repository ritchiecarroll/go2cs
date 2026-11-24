// N1's hang rows (docs/phase4/DESIGN-mn-nopark.md 5.1), all at ONE P. Each phase blocks the only carrier behind an
// edge N1 declares not a park, and the goroutine whose work releases it must still run:
//
//  N1-9  a twin goroutine formats a value whose String receives from a channel. fmt's method hook is a cut site, so
//        String runs plain and blocks its carrier; a second goroutine sends.
//  N1-10 fmt.Fscan over an io.Pipe. fmt's rune reader is a cut site, so the read runs plain and blocks its carrier;
//        a second goroutine writes.
//  N1-11 a function that is not a twin (its only park is a weak Lock) locks a mutex held by a twin goroutine parked
//        on a channel; a third goroutine wakes the holder, which unlocks.
//
// Every goroutine first receives from a gate, a park point, so each has a twin and runs on the scheduler under
// GO2CS_MN=1. Under the twinned standard library (GoStdlibTwins=true) the cut and weak edges are the ones the
// generator writes plain. A hang reds the row by name: the runner's run timeout kills it.
//
// Deterministic by construction: main prints each phase's result after the phase completes.
package main

import (
	"fmt"
	"io"
	"runtime"
	"sync"
	"time"
)

type slow struct{ ch chan int }

func (s slow) String() string {
	v := <-s.ch
	return fmt.Sprintf("slow(%d)", v)
}

func hookBlocks() string {
	gate := make(chan struct{})
	ch := make(chan int)
	done := make(chan string)

	go func() {
		<-gate
		done <- fmt.Sprint(slow{ch})
	}()

	go func() {
		<-gate
		time.Sleep(10 * time.Millisecond)
		ch <- 9
	}()

	close(gate)
	return <-done
}

func readerBlocks() int {
	gate := make(chan struct{})
	r, w := io.Pipe()
	done := make(chan int)

	go func() {
		<-gate
		var n int
		fmt.Fscan(r, &n)
		done <- n
	}()

	go func() {
		<-gate
		time.Sleep(10 * time.Millisecond)
		fmt.Fprint(w, "10 ")
		w.Close()
	}()

	close(gate)
	return <-done
}

// Not a twin: its only park is the weak Lock.
func plainLock(mu *sync.Mutex, n *int) int {
	mu.Lock()
	defer mu.Unlock()
	*n++
	return *n
}

func weakLockBlocks() (int, int) {
	var mu sync.Mutex
	n := 0
	held := make(chan struct{})
	wake := make(chan struct{})
	holder := make(chan int)
	locker := make(chan int)

	go func() {
		mu.Lock()
		close(held)
		<-wake
		n += 10
		mu.Unlock()
		holder <- 1
	}()

	go func() {
		<-held
		locker <- plainLock(&mu, &n)
	}()

	go func() {
		<-held
		time.Sleep(10 * time.Millisecond)
		close(wake)
	}()

	return <-holder, <-locker
}

func main() {
	runtime.GOMAXPROCS(1)

	fmt.Println("N1-9 hook:", hookBlocks())
	fmt.Println("N1-10 reader:", readerBlocks())
	h, l := weakLockBlocks()
	fmt.Println("N1-11 weak lock:", h, l)
}
