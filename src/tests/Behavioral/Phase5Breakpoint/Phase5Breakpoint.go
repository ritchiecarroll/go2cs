// Phase5Breakpoint guards runtime.Breakpoint: with no debugger attached, Go's trap is fatal, so the
// program prints "before", the runtime reports the trap on stderr and the process exits 2 without
// printing "after".
package main

import (
	"fmt"
	"runtime"
)

func main() {
	fmt.Println("before")
	runtime.Breakpoint()
	fmt.Println("after")
}
