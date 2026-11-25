package main

import "fmt"

// Go's main goroutine grows its stack to 1 GB, so a deep non-tail recursion in main is ordinary Go. A converted
// program runs main on a thread whose stack is fixed when the thread starts; this program recurses past the OS
// default reservation of the process's startup thread on every supported OS (1 MiB on Windows, 8 MiB on linux and
// macOS) and well inside Go's limit.
const levels = 2_000_000

// depth adds after the recursive call returns, so the call is not in tail position.
func depth(n int) int {
	if n == 0 {
		return 0
	}
	return depth(n-1) + 1
}

// main receives its depth from a goroutine, so it parks before it recurses. Under the M:N scheduler (GO2CS_MN=1) main
// then runs as a twin, resumed on whichever carrier is free, and recurses on that carrier's stack, which must reserve
// what main's own thread does.
func main() {
	n := make(chan int)
	go func() { n <- levels }()
	fmt.Println("depth:", depth(<-n))
}
