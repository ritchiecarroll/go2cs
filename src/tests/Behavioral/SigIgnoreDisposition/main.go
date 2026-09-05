package main

import (
	"fmt"
	"os"
	"os/exec"
	"os/signal"
	"strings"
	"syscall"
)

// A PROBE, not a guard (never for merge): after signal.Ignore(sig), what KERNEL disposition does a
// child inherit across exec? Go's Ignore is setsig(_SIG_IGN) — a kernel disposition, inherited by
// exec'd children as SIG_IGN — so Go prints SIG_IGN for every signal below. A bridge that models
// Ignore as a swallow-in-handler installs a HANDLER, which exec resets to SIG_DFL, and a bridge that
// does not map a signal at all installs nothing, so the child reads SIG_DFL either way. Reading the
// disposition in a child (python's signal.getsignal) needs no terminal and never stops a process,
// which is what makes the reading deterministic on a CI runner. Measures C1's Q64 root on linux and
// COORD's question for the darwin bridge on both mac legs with one program.
func disposition(sig syscall.Signal, name string) {
	signal.Ignore(sig)

	out, err := exec.Command("python3", "-c", fmt.Sprintf("import signal; print(int(signal.getsignal(%d)))", int(sig))).Output()
	if err != nil {
		fmt.Println(name, "probe error:", err)
		return
	}

	fmt.Println(name, "inherited disposition:", strings.TrimSpace(string(out)))
}

func main() {
	disposition(syscall.SIGUSR1, "SIGUSR1") // a signal both bridges MAP: a handler, which exec resets
	disposition(syscall.SIGTTOU, "SIGTTOU") // the job-control member the kernel consults at tcsetpgrp
	disposition(syscall.SIGTTIN, "SIGTTIN")
	disposition(syscall.SIGTSTP, "SIGTSTP")
	_ = os.Getpid
	fmt.Println("done")
}
