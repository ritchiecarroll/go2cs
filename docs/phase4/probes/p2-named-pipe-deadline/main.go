//go:build windows

// PipeProbe: the named-overlapped-pipe pair of os TestVariousDeadlines1Proc/named_overlapped_pipe, cut into
// three steps that each discriminate one candidate for the hang. Each step runs in its own goroutine with
// a 10 s guard, so a step that hangs is reported by name and the next one still runs.
//
//	A. read deadline, NO writer: Go returns i/o timeout after ~50 ms. A hang here means a timed-out pending
//	   read is never cancelled (CancelIoEx not reaching the operation), so waitCanceled waits for data that
//	   never comes.
//	B. one write, one read, no deadlines: Go moves 5 bytes at once. A hang here means a completion on the
//	   pipe never reaches the poller (the handle's port binding).
//	C. close the reader while its read is pending, no deadline: Go's Read returns "file already closed".
//	   A hang here means Close's CancelIoEx(h, nil) does not wake the pending read.
package main

import (
	"fmt"
	"os"
	"strconv"
	"sync/atomic"
	"syscall"
	"time"
)

const (
	pipeAccessDuplex    = 0x3
	fileFlagOverlapped  = 0x40000000
	pipeTypeByte        = 0x0
	pipeReadModeByte    = 0x0
	oFileFlagOverlapped = 0x40000000 // windows.O_FILE_FLAG_OVERLAPPED, os.OpenFile's flag
)

var (
	kernel32            = syscall.NewLazyDLL("kernel32.dll")
	procCreateNamedPipe = kernel32.NewProc("CreateNamedPipeW")
	counter             atomic.Uint64
)

func pair() (r, w *os.File) {
	name := `\\.\pipe\go2cs-pipeprobe-` + strconv.Itoa(os.Getpid()) + "-" + strconv.FormatUint(counter.Add(1), 10)
	wname, _ := syscall.UTF16PtrFromString(name)
	h, _, e := procCreateNamedPipe.Call(uintptr(unsafePointer(wname)), pipeAccessDuplex|fileFlagOverlapped,
		pipeTypeByte|pipeReadModeByte, 1, 4096, 4096, 0, 0)
	if syscall.Handle(h) == syscall.InvalidHandle {
		panic(fmt.Sprintf("CreateNamedPipe: %v", e))
	}
	w = os.NewFile(h, name)
	r, err := os.OpenFile(name, os.O_RDWR|os.O_CREATE|oFileFlagOverlapped, 0666)
	if err != nil {
		panic(err)
	}
	return r, w
}

func step(name string, body func() string) {
	done := make(chan string, 1)
	t0 := time.Now()
	go func() { done <- body() }()
	select {
	case got := <-done:
		fmt.Printf("%s: %s (%v)\n", name, got, time.Since(t0).Round(time.Millisecond))
	case <-time.After(10 * time.Second):
		fmt.Printf("%s: HANG (no return in 10s)\n", name)
	}
}

func main() {
	step("A deadline, no writer", func() string {
		r, w := pair()
		_ = w
		r.SetReadDeadline(time.Now().Add(50 * time.Millisecond))
		n, err := r.Read(make([]byte, 16))
		return fmt.Sprintf("n=%d err=%v", n, err)
	})
	step("B one write, one read", func() string {
		r, w := pair()
		if _, err := w.Write([]byte("hello")); err != nil {
			return "write: " + err.Error()
		}
		buf := make([]byte, 16)
		n, err := r.Read(buf)
		return fmt.Sprintf("n=%d %q err=%v", n, buf[:n], err)
	})
	step("C close under a pending read", func() string {
		r, w := pair()
		_ = w
		res := make(chan string, 1)
		go func() {
			n, err := r.Read(make([]byte, 16))
			res <- fmt.Sprintf("n=%d err=%v", n, err)
		}()
		time.Sleep(100 * time.Millisecond)
		r.Close()
		return <-res
	})
}
