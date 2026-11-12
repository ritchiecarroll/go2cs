// MnCost is the M:N spike's cost fixture (phase 2, B4). Each mode times one shape on its path that never
// parks: a call from one function that can park into another, a mutex pair, a deferred call, a copy loop
// through package io in 512-byte chunks between two types that do next to nothing, and the same copy into a
// SHA-256 hash. Every timed function holds a receive it never reaches, which is what makes the generator give
// it a twin. The hold mode only sleeps, for a thread count. The mode and the sizes come from the environment.
package main

import (
	"crypto/sha256"
	"fmt"
	"io"
	"os"
	"runtime"
	"strconv"
	"sync"
	"time"
)

func envInt(name string, def int) int {
	if v, err := strconv.Atoi(os.Getenv(name)); err == nil && v > 0 {
		return v
	}

	return def
}

//go:noinline
func maybePark(c chan int, i int) int {
	if i < 0 {
		return <-c
	}

	return i + 1
}

func callLoop(c chan int, n int) int {
	sum := 0

	for i := 0; i < n; i++ {
		sum += maybePark(c, i)
	}

	return sum
}

func mutexLoop(mu *sync.Mutex, c chan int, n int) int {
	x := 0

	for i := 0; i < n; i++ {
		if i < 0 {
			x += <-c
		}

		mu.Lock()
		x++
		mu.Unlock()
	}

	return x
}

//go:noinline
func withDefer(c chan int, i int) (r int) {
	defer func() { r++ }()

	if i < 0 {
		return <-c
	}

	return i
}

func deferLoop(c chan int, n int) int {
	sum := 0

	for i := 0; i < n; i++ {
		sum += withDefer(c, i)
	}

	return sum
}

type memReader struct{ left int }

func (m *memReader) Read(p []byte) (int, error) {
	if m.left == 0 {
		return 0, io.EOF
	}

	n := len(p)

	if n > m.left {
		n = m.left
	}

	for i := 0; i < n; i += 64 {
		p[i] = byte(i)
	}

	m.left -= n

	return n, nil
}

type countWriter struct{ n int64 }

func (w *countWriter) Write(p []byte) (int, error) {
	w.n += int64(len(p))
	return len(p), nil
}

func copyLoop(c chan int, chunks int) int {
	if chunks < 0 {
		return <-c
	}

	src := &memReader{left: chunks * 512}
	dst := &countWriter{}
	n, err := io.CopyBuffer(dst, src, make([]byte, 512))

	if err != nil || n != dst.n {
		return -1
	}

	return int(n / 512)
}

func hashLoop(c chan int, chunks int) int {
	if chunks < 0 {
		return <-c
	}

	src := &memReader{left: chunks * 512}
	h := sha256.New()
	n, err := io.CopyBuffer(h, src, make([]byte, 512))

	if err != nil || len(h.Sum(nil)) != sha256.Size {
		return -1
	}

	return int(n / 512)
}

// begin and end bracket one timed repetition. The timed call sits in main itself, between the two: a call made
// through a func value runs the function's plain body under either build, and would time no twin at all.
func begin() (uint64, time.Time) {
	var m runtime.MemStats

	runtime.ReadMemStats(&m)

	return m.TotalAlloc, time.Now()
}

func end(mode string, rep, n, got int, alloc uint64, start time.Time) {
	var m runtime.MemStats

	took := time.Since(start)
	runtime.ReadMemStats(&m)

	fmt.Fprintf(os.Stderr, "COST mode=%s rep=%d n=%d check=%d ns-per-op=%.2f bytes-per-op=%.2f\n", mode, rep, n, got,
		float64(took.Nanoseconds())/float64(n), float64(m.TotalAlloc-alloc)/float64(n))
}

func main() {
	mode := os.Getenv("MN_COST_MODE")
	n := envInt("MN_COST_N", 1000)
	c := make(chan int)
	mu := &sync.Mutex{}
	reps := 5

	if mode == "hold" {
		time.Sleep(time.Duration(n) * time.Millisecond)
	}

	if mode == "" || mode == "hold" {
		reps = 0
	}

	for rep := 0; rep < reps; rep++ {
		got := 0
		alloc, start := begin()

		switch mode {
		case "call":
			got = callLoop(c, n)
		case "mutex":
			got = mutexLoop(mu, c, n)
		case "defer":
			got = deferLoop(c, n)
		case "copy":
			got = copyLoop(c, n)
		case "hash":
			got = hashLoop(c, n)
		}

		end(mode, rep, n, got, alloc, start)
	}

	fmt.Println("ok", callLoop(c, 10), mutexLoop(&sync.Mutex{}, c, 10), deferLoop(c, 10), copyLoop(c, 10))
}
