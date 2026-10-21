// PipeSharedFileObject guards the bind a pollable Windows handle gets at open, and the event path an
// FD takes when that bind is refused.
//
// Go's netpollopen associates every pollable handle with its completion port at os.NewFile. The
// association belongs to the FILE OBJECT, not to the handle, so a second handle to an object that is
// already associated is refused; os.newFile ignores the error, the FD's pd keeps runtimeCtx 0, and
// execIO gives each of its operations its own event and waits on that. A shell's `2>&1` into a pipe
// produces exactly this: stdout and stderr are two handles to one pipe object, and since Go 1.25
// os.NewFile probes the handles and polls them. A converted program writing stdout, stderr, stdout
// there died on the stderr write, because the bind ran at the first submit and threw.
//
// This program builds that state itself, so it does not depend on how the harness wires its own
// output: two handles (DuplicateHandle) to the client end of one overlapped named pipe, each wrapped
// by os.NewFile, then stdout, stderr, stdout through them. It then checks the refused FD from both
// sides:
//
//  1. the three writes arrive at the server end, in order;
//  2. the refused FD has no deadlines (os.ErrNoDeadline) while the bound one does;
//  3. a READ through the refused FD, the event path in the other direction;
//  4. the SAME-VALUE variant: os.NewFile twice on ONE handle. The first FD is bound and the second is
//     refused, and the second's writes must not move the first off its binding: the first still takes
//     a deadline, and its own write and a timed-out read still behave as a polled FD's;
//  5. File.Fd still takes a BOUND FD off the poller (Go 1.26's windows Fd calls DisassociateIOCP):
//     the bind now happens at open, so Fd has a real association to drop, and the FD's later IO
//     waits on its own events. No step before this one calls Fd, because an Fd call before the
//     writes moves an FD off the poller and hides the refusal this program exists to reach.
//
// Every step runs under a guard, so a hang is printed by name rather than stalling the run.
package main

import (
	"errors"
	"fmt"
	"os"
	"strconv"
	"strings"
	"sync/atomic"
	"syscall"
	"time"
	"unsafe"
)

const (
	pipeAccessDuplex   = 0x3
	fileFlagOverlapped = 0x40000000
	pipeTypeByte       = 0x0
	pipeReadModeByte   = 0x0
)

var (
	kernel32            = syscall.NewLazyDLL("kernel32.dll")
	procCreateNamedPipe = kernel32.NewProc("CreateNamedPipeW")
	counter             atomic.Uint64
)

// pipe creates one overlapped named pipe and returns its server end, wrapped, and the raw handle of
// its client end, opened for overlapped IO.
func pipe() (*os.File, syscall.Handle) {
	name := `\\.\pipe\go2cs-pipeshared-` + strconv.Itoa(os.Getpid()) + "-" + strconv.FormatUint(counter.Add(1), 10)
	wname, _ := syscall.UTF16PtrFromString(name)

	h, _, e := procCreateNamedPipe.Call(uintptr(unsafe.Pointer(wname)), pipeAccessDuplex|fileFlagOverlapped,
		pipeTypeByte|pipeReadModeByte, 1, 4096, 4096, 0, 0)
	if syscall.Handle(h) == syscall.InvalidHandle {
		panic(fmt.Sprintf("CreateNamedPipe: %v", e))
	}

	c, err := syscall.CreateFile(wname, syscall.GENERIC_READ|syscall.GENERIC_WRITE, 0, nil,
		syscall.OPEN_EXISTING, fileFlagOverlapped, 0)
	if err != nil {
		panic(fmt.Sprintf("CreateFile: %v", err))
	}

	return os.NewFile(h, name), c
}

// readLines reads from r until it holds n lines.
func readLines(r *os.File, n int) string {
	var got []byte
	buf := make([]byte, 256)

	for strings.Count(string(got), "\n") < n {
		k, err := r.Read(buf)
		got = append(got, buf[:k]...)
		if err != nil {
			return string(got) + "<" + err.Error() + ">"
		}
	}

	return string(got)
}

func step(name string, body func() string) {
	done := make(chan string, 1)
	go func() { done <- body() }()

	select {
	case got := <-done:
		fmt.Printf("%s: %s\n", name, got)
	case <-time.After(10 * time.Second):
		fmt.Printf("%s: HANG (no return in 10s)\n", name)
	}
}

func main() {
	server, client := pipe()

	var dup syscall.Handle
	self, _ := syscall.GetCurrentProcess()
	if err := syscall.DuplicateHandle(self, client, self, &dup, 0, false, syscall.DUPLICATE_SAME_ACCESS); err != nil {
		panic(fmt.Sprintf("DuplicateHandle: %v", err))
	}

	stdout := os.NewFile(uintptr(client), "stdout")
	stderr := os.NewFile(uintptr(dup), "stderr")

	step("1 stdout, stderr, stdout", func() string {
		for _, w := range []struct {
			f    *os.File
			text string
		}{{stdout, "stdout line\n"}, {stderr, "stderr line\n"}, {stdout, "stdout again\n"}} {
			if _, err := w.f.Write([]byte(w.text)); err != nil {
				return w.f.Name() + " write: " + err.Error()
			}
		}
		return strconv.Quote(readLines(server, 3))
	})

	step("2 deadlines", func() string {
		refused := stderr.SetDeadline(time.Now().Add(time.Hour))
		bound := stdout.SetDeadline(time.Now().Add(time.Hour))
		stdout.SetDeadline(time.Time{})
		return fmt.Sprintf("stderr ErrNoDeadline=%v stdout err=%v", errors.Is(refused, os.ErrNoDeadline), bound)
	})

	step("3 read through stderr", func() string {
		if _, err := server.Write([]byte("reply\n")); err != nil {
			return "server write: " + err.Error()
		}
		return strconv.Quote(readLines(stderr, 1))
	})

	step("4 one handle, two files", func() string {
		server2, client2 := pipe()
		first := os.NewFile(uintptr(client2), "first")
		second := os.NewFile(uintptr(client2), "second")

		if _, err := second.Write([]byte("second line\n")); err != nil {
			return "second write: " + err.Error()
		}
		deadline := first.SetWriteDeadline(time.Now().Add(time.Hour))
		if _, err := first.Write([]byte("first line\n")); err != nil {
			return "first write: " + err.Error()
		}
		arrived := readLines(server2, 2)

		first.SetReadDeadline(time.Now().Add(50 * time.Millisecond))
		_, timeout := first.Read(make([]byte, 16))

		return fmt.Sprintf("%q first deadline err=%v first read timed out=%v second ErrNoDeadline=%v",
			arrived, deadline, errors.Is(timeout, os.ErrDeadlineExceeded),
			errors.Is(second.SetDeadline(time.Now().Add(time.Hour)), os.ErrNoDeadline))
	})

	step("5 Fd disassociates", func() string {
		server3, client3 := pipe()
		f := os.NewFile(uintptr(client3), "f")

		if _, err := f.Write([]byte("before Fd\n")); err != nil {
			return "write before Fd: " + err.Error()
		}
		before := f.SetDeadline(time.Time{})
		f.Fd()
		after := f.SetDeadline(time.Now().Add(time.Hour))
		if _, err := f.Write([]byte("after Fd\n")); err != nil {
			return "write after Fd: " + err.Error()
		}

		return fmt.Sprintf("%q deadline before Fd err=%v after Fd ErrNoDeadline=%v",
			readLines(server3, 2), before, errors.Is(after, os.ErrNoDeadline))
	})
}
