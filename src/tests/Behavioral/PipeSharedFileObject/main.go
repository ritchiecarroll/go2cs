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
//     writes moves an FD off the poller and hides the refusal this program exists to reach;
//  6. a handle NUMBER reused after a close: closing one of two Files over one handle leaves the
//     other File's FD registered under a number the kernel is free to hand out again. A new handle
//     that gets it must open and work, as in Go, and not trip over the old FD's record.
//  7. CONSECUTIVE writes on one bound FD: eight small writes back to back, each completing at once
//     inside the pipe's buffer, then four writes larger than the buffer, each pending until the
//     server end drains it. Every write is an execIO submit that waits for its own completion, so a
//     completion that does not reach the poller hangs the write after it. A converted program
//     writing stdout to a shell-made overlapped pipe once printed its first line and never returned
//     from the second write (fixed by the named-pipe completion delivery); no step above writes twice
//     in a row through one bound FD. It runs twice, the second time on a handle given
//     FILE_SKIP_COMPLETION_PORT_ON_SUCCESS, where only a PENDING write waits for a completion (see
//     consecutiveWrites). A HANG line says how many of the pass's writes returned.
//
// Files that share one handle are kept reachable until the program exits: each File's finalizer
// closes the handle, and a second close of a reused number would close an unrelated handle.
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
	written             atomic.Int64 // writes step 7 has completed in its current pass (other steps leave it 0)
	keep                []*os.File
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
	written.Store(0)
	done := make(chan string, 1)
	go func() { done <- body() }()

	select {
	case got := <-done:
		fmt.Printf("%s: %s\n", name, got)
	case <-time.After(10 * time.Second):
		fmt.Printf("%s: HANG (no return in 10s, %d writes returned)\n", name, written.Load())
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
		keep = append(keep, first, second)

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

	step("6 a handle number reused after a close", func() string {
		server4, client4 := pipe()
		first := os.NewFile(uintptr(client4), "first")
		second := os.NewFile(uintptr(client4), "second")
		keep = append(keep, first, second)

		if _, err := first.Write([]byte("before close\n")); err != nil {
			return "first write: " + err.Error()
		}
		before := readLines(server4, 1)

		// The second pipe exists before the close, so its own handles cannot take the freed number.
		server5, other := pipe()

		// second's open was refused, so its Close retires nothing in the poller; it closes the
		// handle number first's FD is still registered under.
		second.Close()

		// Duplicates of the second pipe's client until one gets the freed number.
		var reused syscall.Handle
		for i := 0; i < 256 && reused == 0; i++ {
			var d syscall.Handle
			if err := syscall.DuplicateHandle(self, other, self, &d, 0, false, syscall.DUPLICATE_SAME_ACCESS); err != nil {
				return "DuplicateHandle: " + err.Error()
			}
			if d == client4 {
				reused = d
			}
		}
		if reused == 0 {
			return "the closed number was not handed out again"
		}

		f := os.NewFile(uintptr(reused), "reused")
		keep = append(keep, f)

		if _, err := f.Write([]byte("after reuse\n")); err != nil {
			return "write through the reused number: " + err.Error()
		}

		return fmt.Sprintf("%q %q", before, readLines(server5, 1))
	})

	step("7 consecutive writes on one bound FD", func() string { return consecutiveWrites(false) })
	step("7 the same, with skip-on-success set", func() string { return consecutiveWrites(true) })
}

// consecutiveWrites is step 7: eight small writes, then four large ones, through one bound FD. With
// skipOnSuccess the client handle is first given FILE_SKIP_COMPLETION_PORT_ON_SUCCESS, which
// os.NewFile keeps: an inline success then owes no completion packet and only a PENDING write waits
// for one. That is the shape of the stdout hang (the first write returned, a later one never did);
// without the flag, every inline success also waits for a packet.
func consecutiveWrites(skipOnSuccess bool) string {
	server7, client7 := pipe()
	if skipOnSuccess {
		if err := syscall.SetFileCompletionNotificationModes(client7, syscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS); err != nil {
			return "SetFileCompletionNotificationModes: " + err.Error()
		}
	}
	f := os.NewFile(uintptr(client7), "f")

	for i := 1; i <= 8; i++ {
		if _, err := f.Write([]byte("write " + strconv.Itoa(i) + "\n")); err != nil {
			return fmt.Sprintf("small write %d: %v", i, err)
		}
		written.Add(1)
	}
	small := readLines(server7, 8)

	const large, chunk = 4, 16 << 10
	drained := make(chan string, 1)
	go func() {
		total, buf := 0, make([]byte, 4096)
		for total < large*chunk {
			k, err := server7.Read(buf)
			total += k
			if err != nil {
				drained <- fmt.Sprintf("%d bytes <%v>", total, err)
				return
			}
		}
		drained <- fmt.Sprintf("%d bytes", total)
	}()

	for i := 1; i <= large; i++ {
		if n, err := f.Write([]byte(strings.Repeat(strconv.Itoa(i), chunk))); err != nil || n != chunk {
			return fmt.Sprintf("large write %d: n=%d err=%v", i, n, err)
		}
		written.Add(1)
	}

	return fmt.Sprintf("%d small lines, last %q; %d large writes, %s drained",
		strings.Count(small, "\n"), small[strings.LastIndex(small[:len(small)-1], "\n")+1:], large, <-drained)
}
