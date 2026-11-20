package main

import (
	"fmt"
	"runtime"
	"strings"
)

var mode = "fast"

const kind = "b"

func goos() {
	// The Tour's switch: a local tag, read in place.
	switch os := runtime.GOOS; os {
	case "darwin":
		fmt.Println("macOS")
	case "linux", "windows", "freebsd":
		fmt.Println("known")
	default:
		fmt.Println("other")
	}
}

func shapes(s string, n int) {
	// A body writes the tag, then falls through: the next test is short-circuited.
	switch n {
	case 1:
		n = 3
		fallthrough
	case 2:
		fmt.Println("two or fell", n)
	case 3:
		fmt.Println("three", n)
	}

	// A body writes the tag without falling through: no later test runs.
	switch n {
	case 3:
		n = 4
	case 4:
		fmt.Println("never", n)
	default:
		fmt.Println("default", n)
	}

	// A non-trailing default with fallthrough.
	switch s {
	default:
		fmt.Println("default first", s)
		fallthrough
	case "x":
		fmt.Println("x or after default", s)
	case "y":
		fmt.Println("y", s)
	}

	// Two switches on one local in one function.
	switch s {
	case "a", "b":
		fmt.Println("a or b")
	case "c":
		fmt.Println("c")
	}
}

func refused(s string) {
	// A label that calls: the tag keeps a temp named after it.
	switch s {
	case strings.ToLower("A"):
		fmt.Println("lower a")
	case "b":
		fmt.Println("b")
	}

	// A tag read inside a func literal keeps its temp.
	f := func() string { return s }

	switch s {
	case "b":
		fmt.Println("b again", f())
	case "c":
		fmt.Println("c")
	}

	// An address-taken tag keeps its temp.
	p := &s

	switch s {
	case "b":
		*p = "changed"
		fmt.Println("b via pointer", s)
	case "changed":
		fmt.Println("never")
	}

	// A package variable and a constant keep temps named after them.
	switch mode {
	case "fast":
		fmt.Println("fast")
	case "slow":
		fmt.Println("slow")
	}

	switch kind {
	case "a":
		fmt.Println("kind a")
	case "b":
		fmt.Println("kind b")
	}
}

func keywordNamed(base string) {
	// A Go name that is a C# keyword: the temp drops the escape.
	n := 0
	if len(base) > 1 {
		n = 2
	}

	switch base {
	case "x":
		fmt.Println("x")
		fallthrough
	case "base":
		fmt.Println("base", n)
	}
}

func selects() {
	c := make(chan int, 1)
	quit := make(chan int)
	done := make(chan bool)

	// Only the send is ready; the send case is named after its channel.
	select {
	case c <- 1:
		fmt.Println("sent")
	case v := <-quit:
		fmt.Println("quit", v)
	}

	// Two cases on one channel, both ready: either prints the same line.
	select {
	case v := <-c:
		fmt.Println("received", v)
	case v, ok := <-c:
		if ok {
			fmt.Println("received", v)
		}
	case done <- true:
		fmt.Println("done")
	}
}

func main() {
	goos()
	shapes("y", 1)
	shapes("z", 2)
	refused("b")
	keywordNamed("base")
	keywordNamed("x")
	selects()
}
