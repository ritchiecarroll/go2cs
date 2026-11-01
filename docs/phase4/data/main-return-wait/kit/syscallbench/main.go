// The main-return wait seat's microbench (P6): the cost the windows syscall trampoline's new mark adds to every
// system call, read as a tight loop of a cheap one through syscall.SyscallN. GetCurrentProcessId neither blocks nor
// fails, so the loop measures the trampoline and the mark, not the kernel.
package main

import (
	"fmt"
	"syscall"
	"time"
)

func main() {
	proc := syscall.MustLoadDLL("kernel32.dll").MustFindProc("GetCurrentProcessId")
	addr := proc.Addr()

	var pid uintptr

	// Warm-up: every method on the path compiled and tiered up before the clock starts.
	for i := 0; i < 2_000_000; i++ {
		pid, _, _ = syscall.SyscallN(addr)
	}

	const n = 10_000_000
	start := time.Now()

	for i := 0; i < n; i++ {
		pid, _, _ = syscall.SyscallN(addr)
	}

	elapsed := time.Since(start)

	fmt.Printf("checksum: %v\n", pid != 0)
	fmt.Printf("syscall_ns: %.3f\n", float64(elapsed.Nanoseconds())/n)
}
