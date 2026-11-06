package main

import (
	"bufio"
	"bytes"
	"fmt"
	"iter"
	"os"
	"os/exec"
	"runtime"
	"strings"
)

// runtime.Goexit called from the main goroutine (docs/DESIGN-goexit-main.md). Go ends main without returning: main's
// deferred calls run, recover() in them reads nil, and the program keeps running its other goroutines. When none can
// run any more it dies with a fatal error, exit status 2: "no goroutines (main called runtime.Goexit) - deadlock!"
// when none is left, "all goroutines are asleep - deadlock!" when the ones left are all blocked. Until then another
// goroutine may end the process with os.Exit, or with a panic.
//
// Each arm ends the process, so the program re-runs itself once per arm and prints, for each, the child's stdout,
// the first line of its stderr and its exit status: the parts that are the same on every host.

var arms = []struct {
	name string
	run  func()
}{
	{"a: main's defers run, recover reads nil, a goroutine finishes, then no goroutines", armA},
	{"b: a goroutine calls os.Exit(0) after main's Goexit", armB},
	{"c: a goroutine panics after main's Goexit", armC},
	{"d: the goroutine left blocks forever in select{}", armD},
	{"e: the Goexit is raised two deferring frames below main", armE},
	{"f: the Goexit reaches main through iter.Pull's next", armF},
}

func main() {
	if arm := os.Getenv("GOEXIT_MAIN_ARM"); arm != "" {
		for _, a := range arms {
			if a.name[:1] == arm {
				a.run()
			}
		}
		fmt.Println("unreachable: main returned")
		return
	}

	for _, a := range arms {
		parent(a.name)
	}
}

// Each goroutine below waits for a channel that main's LAST deferred call closes (deferred calls run last-registered
// first, so the close is registered first): its output then follows main's deferred calls on every host.

func armA() {
	ready := make(chan struct{})
	defer close(ready)

	go func() {
		<-ready
		fmt.Println("goroutine finished its work")
	}()

	defer func() {
		fmt.Println("main's deferred call ran; recover() =", recover())
	}()

	runtime.Goexit()
	fmt.Println("unreachable: after Goexit")
}

func armB() {
	ready := make(chan struct{})
	defer close(ready)

	go func() {
		<-ready
		fmt.Println("goroutine calls os.Exit(0)")
		os.Exit(0)
	}()

	defer fmt.Println("main's deferred call ran")
	runtime.Goexit()
}

func armC() {
	ready := make(chan struct{})
	defer close(ready)

	go func() {
		<-ready
		panic("a goroutine panics after main's Goexit")
	}()

	defer fmt.Println("main's deferred call ran")
	runtime.Goexit()
}

func armD() {
	started := make(chan struct{})

	go func() {
		fmt.Println("goroutine blocks forever")
		close(started)
		select {}
	}()

	<-started

	defer fmt.Println("main's deferred call ran")
	runtime.Goexit()
}

func armE() {
	defer fmt.Println("main's deferred call ran")
	outer()
	fmt.Println("unreachable: outer returned")
}

func outer() {
	defer fmt.Println("outer's deferred call ran")
	inner()
	fmt.Println("unreachable: inner returned")
}

func inner() {
	defer func() {
		fmt.Println("inner's deferred call ran; recover() =", recover())
	}()
	runtime.Goexit()
}

func armF() {
	defer fmt.Println("main's deferred call ran")

	seq := func(yield func(int) bool) {
		if !yield(1) {
			return
		}
		runtime.Goexit()
	}

	next, stop := iter.Pull(iter.Seq[int](seq))
	defer stop()

	v, ok := next()
	fmt.Println("first value:", v, ok)
	next()
	fmt.Println("unreachable: next returned after the seq's Goexit")
}

func parent(name string) {
	cmd := exec.Command(os.Args[0])
	cmd.Env = append(os.Environ(), "GOEXIT_MAIN_ARM="+name[:1])
	var stdout, stderr bytes.Buffer
	cmd.Stdout = &stdout
	cmd.Stderr = &stderr
	err := cmd.Run()

	code := 0
	if exit, ok := err.(*exec.ExitError); ok {
		code = exit.ExitCode()
	} else if err != nil {
		code = -1
	}

	fmt.Println("== arm", name)
	for _, line := range strings.Split(strings.TrimRight(stdout.String(), "\n"), "\n") {
		if line != "" {
			fmt.Println("  stdout:", line)
		}
	}

	first := ""
	if scanner := bufio.NewScanner(&stderr); scanner.Scan() {
		first = scanner.Text()
	}
	fmt.Println("  stderr first line:", first)
	fmt.Println("  exit status:", code)
}
