package main

// SyscallKeystonePulls guards the //go:linkname PULLS internal/syscall/unix makes of the syscall
// package's keystone family (syscall_syscall, syscall_syscallPtr, syscall_syscall6, syscall_syscall6X,
// syscall_syscall9 -- net_darwin.go on darwin). Each was a bodyless partial filled by the stub
// generator's throw in the darwin corpus until darwin increment 10 (a), so the FIRST line of any
// exec.Command by bare name (LookPath -> unix.Eaccess -> faccessat -> syscall_syscall6) died with
// NotImplementedException on both mac legs -- the SigIgnoreDisposition probe's finding, 2026-09-05.
// LookPath needs no fork and no terminal, so it is the reach increment 10 (a) opens and the reach
// this row asserts.
//
// A NEGATIVE RESULT, BANKED HERE RATHER THAN DROPPED (COORD 1daa96a11, amended 2cd045a88). This row
// first also asserted user.Current(), which reaches the same five pulls through getpwuid_r. It is
// NOT usable as a guard yet, and the measurement says why: on both mac legs at increment 10 (a)
// (run 33975385965) Go printed the real record while the converted side answered
//
//     user.Current: error: user: lookup userid 501: internal buffer exceeds 1048576 bytes
//
// which is Go's own retryWithBuffer (os/user/cgo_lookup_unix.go:182) giving up after doubling the
// buffer from 1 KB to 1 MB against getpwuid_r answering ERANGE (34) EVERY time. So the call is
// reached -- the keystone bodies work -- and the failure is downstream, in the struct-passing /
// out-parameter class this repo already carries: unix.Getpwuid hands libc a converted *Passwd (a
// managed struct: six of its ten fields are ж<byte>, i.e. object references, so the CLR lays it out
// AUTO and reorders it) and a **Passwd out-parameter (which arrives NULL when the box is still nil).
// getpwuid_r WRITES pwd but READS buf, buflen and result, so ERANGE at every size points at what it
// reads: the out-parameter arm is measured first, the Passwd mirror is owed either way. os/user on
// darwin therefore stays blocked, by name, until that item lands; this row will regain the
// assertion then.
import (
	"fmt"
	"os/exec"
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

	_, err = exec.LookPath("no-such-executable-go2cs-keystone")
	fmt.Println("LookPath(missing): error:", err != nil)

	fmt.Println("done")
}
