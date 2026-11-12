package main

import (
	"fmt"
	"os"
	"runtime"
	"strings"
	"time"
)

// M:N PROTOTYPE (A2). Prototype code: nothing from this branch lands on master as it is.
//
// A goroutine parks three calls below its `go` statement and then reads its own stack: runtime.Callers,
// runtime.Caller, and runtime.Callers from a deferred function while a panic raised after a park is running.
// Under generated twins the calls above the parked function are suspended twins, not frames on the thread, so
// each reading has to find them through the link from a suspended twin to the caller that awaits it. The reading
// taken before the park is the control: every frame is on the thread then.
//
// MN_CRASH=1 leaves the panic unrecovered, for the traceback.

// ready is buffered: the goroutine says it is about to receive, and does not park to say so. The first
// stack reading is slow enough (tens of milliseconds, cold) to outlast a sleep on its own.
var ready = make(chan bool, 1)

func name(pc uintptr) string {
	f := runtime.FuncForPC(pc)

	if f == nil {
		return "?"
	}

	return f.Name()
}

// A frame of the function `bare` prints without its line.
func show(tag string, pcs []uintptr, bare string) {
	var b strings.Builder
	frames := runtime.CallersFrames(pcs)

	for {
		f, more := frames.Next()

		if strings.HasPrefix(f.Function, "main.") && f.Function != bare {
			fmt.Fprintf(&b, " %s:%d", f.Function, f.Line)
		} else {
			fmt.Fprintf(&b, " %s", f.Function)
		}

		if !more {
			break
		}
	}

	fmt.Println(tag + b.String())
}

func read(tag string) {
	pcs := make([]uintptr, 16)
	n := runtime.Callers(2, pcs)
	show(tag+" callers", pcs[:n], "")

	for i := 1; i <= 6; i++ {
		pc, _, line, ok := runtime.Caller(i)

		if !ok {
			fmt.Println(tag, "caller", i, "none")
			continue
		}

		fn := name(pc)

		if strings.HasPrefix(fn, "main.") {
			fmt.Println(tag, "caller", i, fn, line)
		} else {
			fmt.Println(tag, "caller", i, fn)
		}
	}
}

func leaf(c chan int) int {
	read("before")
	ready <- true
	v := <-c
	read("after")
	return v
}

func mid(c chan int) int {
	v := leaf(c)
	return v + 1
}

func top(c chan int) int {
	v := mid(c)
	return v + 1
}

func root(c chan int, d chan int) {
	v := top(c)
	d <- v
}

func boom(c chan int) {
	ready <- true
	v := <-c
	panic(fmt.Sprint("after park ", v))
}

func bmid(c chan int) int {
	boom(c)
	fmt.Println("not reached")
	return 0
}

func btop(c chan int) (r int) {
	defer func() {
		pcs := make([]uintptr, 16)
		n := runtime.Callers(1, pcs)

		// The deferring function's own line is left out: without twins too it reads one statement late while
		// the deferred call runs, which is not what this program measures.
		show("deferred callers", pcs[:n], "main.btop")

		if os.Getenv("MN_CRASH") == "" {
			fmt.Println("recovered", recover())
			r = 7
		}
	}()

	v := bmid(c)
	return v
}

func broot(c chan int, d chan int) {
	v := btop(c)
	d <- v
}

func main() {
	c := make(chan int)
	d := make(chan int)

	go root(c, d)

	// The goroutine is parked in leaf before the value is sent.
	<-ready
	time.Sleep(20 * time.Millisecond)
	c <- 1
	v := <-d
	fmt.Println("walk", v)

	go broot(c, d)

	<-ready
	time.Sleep(20 * time.Millisecond)
	c <- 2
	v = <-d
	fmt.Println("panic", v)

	// A goroutine started from a function literal: under twins the literal's body is a lambda's state
	// machine, and it is the caller the parked functions below it lead back to.
	go func() {
		v := mid(c)
		d <- v + 10
	}()

	<-ready
	time.Sleep(20 * time.Millisecond)
	c <- 3
	v = <-d
	fmt.Println("literal", v)
}
