package main

// SyscallKeystonePulls guards the //go:linkname PULLS internal/syscall/unix makes of the syscall
// package's keystone family (syscall_syscall, syscall_syscallPtr, syscall_syscall6, syscall_syscall6X,
// syscall_syscall9 -- net_darwin.go on darwin). Each was a bodyless partial filled by the stub
// generator's throw in the darwin corpus until darwin increment 10 (a), so the FIRST line of any
// exec.Command by bare name (LookPath -> unix.Eaccess -> faccessat -> syscall_syscall6) and every
// os/user lookup (getpwuid_r -> syscall_syscall6) died with NotImplementedException on both mac legs
// -- the SigIgnoreDisposition probe's finding, 2026-09-05. Two exec-free, terminal-free reaches:
// LookPath resolves the Go toolchain every leg carries on PATH (no fork), and user.Current walks
// getpwuid_r. Both print host-independent shapes so the same program compares against `go run`
// on every leg; on windows neither path involves a linkname pull and the row is a plain smoke.
import (
	"fmt"
	"os/exec"
	"os/user"
	"path/filepath"
	"strings"
)

func main() {
	p, err := exec.LookPath("go")
	if err != nil {
		fmt.Println("LookPath(go): error:", err)
	} else {
		fmt.Println("LookPath(go):", strings.TrimSuffix(filepath.Base(p), ".exe"), "absolute:", filepath.IsAbs(p))
	}

	u, err := user.Current()
	if err != nil {
		fmt.Println("user.Current: error:", err)
	} else {
		fmt.Println("user.Current: username non-empty:", u.Username != "", "uid non-empty:", u.Uid != "", "home absolute:", filepath.IsAbs(u.HomeDir))
	}

	_, err = exec.LookPath("no-such-executable-go2cs-keystone")
	fmt.Println("LookPath(missing): error:", err != nil)

	fmt.Println("done")
}
