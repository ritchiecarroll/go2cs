package main

import (
	"fmt"
	"os"
	"syscall"
)

// os.Getpagesize is syscall.Getpagesize, which Go's runtime provides as runtime.physPageSize:
// the page size the OS reported at startup (osinit). It is printed as a VALUE, because the Go
// binary and the converted host run on the same machine: 4096 on x86-64, 16384 on Apple silicon.
// A converted runtime that never ran osinit, or a constant standing in for it, prints something
// else on at least one of those.
func main() {
	size := os.Getpagesize()
	fmt.Println("page size:", size)
	fmt.Println("matches syscall.Getpagesize:", size == syscall.Getpagesize())
	fmt.Println("positive power of two:", size > 0 && size&(size-1) == 0)
}
