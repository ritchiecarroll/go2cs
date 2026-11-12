package main

import (
	"bufio"
	"bytes"
	"fmt"
	"os"
	"os/exec"
	"runtime"
	"strings"
	"sync/atomic"
	"time"
)

// M:N SPIKE (R5b, goroutine 1). Spike code: it stays on the spike branch and never lands.
//
// How the main goroutine ends after it has parked. Every arm parks main once, on a value that arrives 20 ms after
// its receive began, and then ends main one way: a return, a runtime.Goexit, a panic, a wait that never ends. Under
// generated twins main's body runs on the scheduler's carriers, and each of these ends has to read as goroutine 1's.
//
// Each arm ends the process, so the program re-runs itself once per arm and prints, for each, the child's stdout,
// the first line of its stderr, the first goroutine header of its trace where that is the same on every host, and
// its exit status. A child that has not ended after ten seconds is killed and reads TIMEOUT. The header is read for
// the panic only: golib's deadlock report is the fatal line alone, with no goroutine block, in plain code too.

var arms = []struct {
	name   string
	header bool
}{
	{"a: main returns while two goroutines are parked", false},
	{"b: main calls Goexit, a goroutine finishes, then no goroutines", false},
	{"c: a goroutine calls os.Exit(0) after main's Goexit", false},
	{"d: a goroutine panics after main's Goexit", false},
	{"e: the Goexit is raised two parking frames below main", false},
	{"f: main and the goroutine left both block forever in select{}", false},
	{"g: main panics", true},
	{"h: main blocks forever in select{} with no goroutine left", false},
	{"i: init locked main to its thread, main parks and returns", false},
}

func init() {
	if os.Getenv("MN_MAIN_EXIT_ARM") == "i" {
		runtime.LockOSThread()
	}
}

func main() {
	switch os.Getenv("MN_MAIN_EXIT_ARM") {
	case "":
		for _, a := range arms {
			parent(a.name, a.header)
		}
		return
	case "a":
		armA()
	case "b":
		armB()
	case "c":
		armC()
	case "d":
		armD()
	case "e":
		armE()
	case "f":
		armF()
	case "g":
		armG()
	case "h":
		armH()
	case "i":
		armI()
	}

	fmt.Println("main returned")
}

// wake sends after the receiver has parked.
func wake(c chan int) {
	time.Sleep(20 * time.Millisecond)
	c <- 1
}

func parked(started chan int, never chan int, id int) {
	started <- id
	<-never
}

func armA() {
	c := make(chan int)
	go wake(c)
	<-c

	started := make(chan int)
	never := make(chan int)
	go parked(started, never, 1)
	go parked(started, never, 2)
	fmt.Println("main received", <-started+<-started)
}

// Each goroutine below waits for a channel that main's LAST deferred call closes (deferred calls run last-registered
// first, so the close is registered first): its output then follows main's deferred calls on every host.

func finish(ready chan struct{}) {
	<-ready
	fmt.Println("goroutine finished its work")
}

func armB() {
	c := make(chan int)
	go wake(c)
	<-c

	ready := make(chan struct{})
	defer close(ready)
	go finish(ready)

	defer func() {
		fmt.Println("main's deferred call ran; recover() =", recover())
	}()
	runtime.Goexit()
	fmt.Println("unreachable: after Goexit")
}

func exitZero(ready chan struct{}) {
	<-ready
	fmt.Println("goroutine calls os.Exit(0)")
	os.Exit(0)
}

func armC() {
	c := make(chan int)
	go wake(c)
	<-c

	ready := make(chan struct{})
	defer close(ready)
	go exitZero(ready)

	defer fmt.Println("main's deferred call ran")
	runtime.Goexit()
}

func panics(ready chan struct{}) {
	<-ready
	panic("a goroutine panics after main's Goexit")
}

func armD() {
	c := make(chan int)
	go wake(c)
	<-c

	ready := make(chan struct{})
	defer close(ready)
	go panics(ready)

	defer fmt.Println("main's deferred call ran")
	runtime.Goexit()
}

func armE() {
	defer fmt.Println("main's deferred call ran")
	c := make(chan int)
	go wake(c)
	<-c
	outer()
	fmt.Println("unreachable: outer returned")
}

func outer() {
	defer fmt.Println("outer's deferred call ran")
	c := make(chan int)
	go wake(c)
	<-c
	inner()
	fmt.Println("unreachable: inner returned")
}

func inner() {
	defer func() {
		fmt.Println("inner's deferred call ran; recover() =", recover())
	}()
	c := make(chan int)
	go wake(c)
	<-c
	runtime.Goexit()
}

func forever(started chan struct{}) {
	close(started)
	select {}
}

func armF() {
	c := make(chan int)
	go wake(c)
	<-c

	started := make(chan struct{})
	go forever(started)
	<-started
	fmt.Println("main blocks forever beside a goroutine that does")
	select {}
}

func armG() {
	c := make(chan int)
	go wake(c)
	<-c
	panic("main panics after it parked")
}

func armH() {
	c := make(chan int)
	go wake(c)
	<-c
	fmt.Println("main blocks forever alone")
	select {}
}

func armI() {
	c := make(chan int)
	go wake(c)
	fmt.Println("main received", <-c)
}

func parent(name string, header bool) {
	cmd := exec.Command(os.Args[0])
	cmd.Env = append(os.Environ(), "MN_MAIN_EXIT_ARM="+name[:1])
	var stdout, stderr bytes.Buffer
	cmd.Stdout = &stdout
	cmd.Stderr = &stderr

	var timedOut atomic.Bool
	err := cmd.Start()

	if err == nil {
		timer := time.AfterFunc(10*time.Second, func() {
			timedOut.Store(true)
			cmd.Process.Kill()
		})
		err = cmd.Wait()
		timer.Stop()
	}

	code := 0
	if exit, ok := err.(*exec.ExitError); ok {
		code = exit.ExitCode()
	} else if err != nil {
		code = -1
	}

	fmt.Println("== arm", name)
	for _, line := range strings.Split(strings.TrimRight(stdout.String(), "\n"), "\n") {
		if line != "" {
			fmt.Println("  stdout:", strings.TrimRight(line, "\r"))
		}
	}

	if timedOut.Load() {
		fmt.Println("  TIMEOUT")
		return
	}

	first, trace := "", ""
	scanner := bufio.NewScanner(&stderr)
	for scanner.Scan() {
		line := scanner.Text()
		if first == "" {
			first = line
		}
		if strings.HasPrefix(line, "goroutine ") && strings.Contains(line, " [") {
			trace = line
			break
		}
	}
	fmt.Println("  stderr first line:", first)
	if header {
		fmt.Println("  first goroutine header:", trace)
	}
	fmt.Println("  exit status:", code)
}
