// M:N N5 (docs/phase4/DESIGN-mn-n5-invisible-blocking.md section 5): blocking the scheduler cannot see. A goroutine
// on the scheduler that blocks in a wait golib does not know about holds its carrier and its P; when enough of them
// block, a goroutine that would release them never gets a P.
//
// The hooks below are nil under Go, and each row falls back to a channel, which Go (and golib) sees. hand_impl.cs
// sets them before main, so the converted program blocks in a .NET wait or a native call instead:
//
//	V1 K = 4P goroutines block in a managed wait (a ManualResetEventSlim) that main sets after a sleep;
//	V2 the same, blocked in a native sleep-poll loop (a 1 ms native sleep per poll) on a flag main sets;
//	V4 P goroutines run CPU steps of MN_INV_STEP_MS (default 200) ms between parks while one goroutine sleeps 25 ms
//	   200 times: the wakes all complete; their latency goes to stderr with MN_INV_READING=1.
//
// The census rows (3d) run only when MN_INV_ONLY names one (C-pipe, C-accept, C-netread, C-exec, C-sleep, C-chan,
// C-stdin, C-dns): K goroutines block in one standard-library call at once, main waits, then releases them. The
// scheduler's READING line (GO2CS_MN_READING=1) says whether the blocks were seen: handoffs or retakes, or only
// injections. MN_INV_ONLY=V<n> runs one V row alone.
//
// Each blocked goroutine first receives from a gate, a park point, so it has a hidden twin and runs on the scheduler
// under GO2CS_MN=1. Deterministic by construction: nothing printed depends on the P count, the order or the timing.
package main

import (
	"fmt"
	"net"
	"os"
	"os/exec"
	"runtime"
	"sort"
	"strconv"
	"time"
)

// newManagedEvent returns a wait that blocks until set is called. nil under Go; hand_impl.cs sets a .NET event.
var newManagedEvent func() (wait func(), set func())

// newNativeFlag returns a wait that polls in native code until set is called. nil under Go; hand_impl.cs sets it.
var newNativeFlag func() (wait func(), set func())

var reading = os.Getenv("MN_INV_READING") == "1"

func channelEvent() (wait func(), set func()) {
	ch := make(chan struct{})
	return func() { <-ch }, func() { close(ch) }
}

func blocker(gate chan struct{}, wait func(), done chan<- int) {
	<-gate
	wait()
	done <- 1
}

// blockAll starts k goroutines blocked in wait, waits until they have had time to block, calls set, and counts them.
func blockAll(k int, wait func(), set func()) int {
	gate := make(chan struct{})
	done := make(chan int, k)
	for i := 0; i < k; i++ {
		go blocker(gate, wait, done)
	}
	close(gate)
	time.Sleep(50 * time.Millisecond)
	set()
	n := 0
	for i := 0; i < k; i++ {
		n += <-done
	}
	return n
}

func v1(k int) {
	mk := newManagedEvent
	if mk == nil {
		mk = channelEvent
	}
	wait, set := mk()
	fmt.Println("V1 goroutines released from a managed wait:", blockAll(k, wait, set) == k)
}

func v2(k int) {
	mk := newNativeFlag
	if mk == nil {
		mk = channelEvent
	}
	wait, set := mk()
	fmt.Println("V2 goroutines released from a native wait:", blockAll(k, wait, set) == k)
}

func busy(d time.Duration) int {
	x := 0
	for began := time.Now(); time.Since(began) < d; {
		for i := 0; i < 1000; i++ {
			x += i ^ (x >> 3)
		}
	}
	return x
}

func cpu(gate, stop chan struct{}, step time.Duration, sinks chan<- int) {
	<-gate
	sink := 0
	for {
		select {
		case <-stop:
			sinks <- sink
			return
		default:
		}
		sink += busy(step)
		runtime.Gosched()
	}
}

func v4(procs int) {
	step := 200 * time.Millisecond
	if s, err := strconv.Atoi(os.Getenv("MN_INV_STEP_MS")); err == nil && s > 0 {
		step = time.Duration(s) * time.Millisecond
	}
	gate, stop := make(chan struct{}), make(chan struct{})
	sinks := make(chan int, procs)
	for p := 0; p < procs; p++ {
		go cpu(gate, stop, step, sinks)
	}
	close(gate)

	const wakes = 200
	lat := make([]time.Duration, 0, wakes)
	for i := 0; i < wakes; i++ {
		began := time.Now()
		time.Sleep(25 * time.Millisecond)
		lat = append(lat, time.Since(began)-25*time.Millisecond)
	}
	close(stop)
	for p := 0; p < procs; p++ {
		<-sinks
	}
	if reading {
		sort.Slice(lat, func(i, j int) bool { return lat[i] < lat[j] })
		fmt.Fprintf(os.Stderr, "reading: V4 step_ms=%d wakes=%d p50_ms=%.1f p99_ms=%.1f max_ms=%.1f\n", step.Milliseconds(), len(lat),
			ms(lat[len(lat)/2]), ms(lat[len(lat)*99/100]), ms(lat[len(lat)-1]))
	}
	fmt.Println("V4 every wake ran:", len(lat) == wakes)
}

func ms(d time.Duration) float64 { return float64(d.Microseconds()) / 1000 }

// The census rows. Each blocks k goroutines in one call, and main releases them after the scheduler has had time to
// see (or not see) the blocks.

func pipeReader(gate chan struct{}, r *os.File, done chan<- int) {
	<-gate
	b := make([]byte, 1)
	n, _ := r.Read(b)
	done <- n
}

func censusPipe(k int) int {
	r, w, err := os.Pipe()
	if err != nil {
		return -1
	}
	gate, done := make(chan struct{}), make(chan int, k)
	for i := 0; i < k; i++ {
		go pipeReader(gate, r, done)
	}
	close(gate)
	time.Sleep(300 * time.Millisecond)
	w.Write(make([]byte, k))
	n := 0
	for i := 0; i < k; i++ {
		n += <-done
	}
	return n
}

func acceptor(gate chan struct{}, l net.Listener, done chan<- int) {
	<-gate
	c, err := l.Accept()
	if err == nil {
		c.Close()
		done <- 1
		return
	}
	done <- 0
}

func censusAccept(k int) int {
	l, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		return -1
	}
	defer l.Close()
	gate, done := make(chan struct{}), make(chan int, k)
	for i := 0; i < k; i++ {
		go acceptor(gate, l, done)
	}
	close(gate)
	time.Sleep(300 * time.Millisecond)
	for i := 0; i < k; i++ {
		if c, err := net.Dial("tcp", l.Addr().String()); err == nil {
			defer c.Close()
		}
	}
	n := 0
	for i := 0; i < k; i++ {
		n += <-done
	}
	return n
}

func connReader(gate chan struct{}, c net.Conn, done chan<- int) {
	<-gate
	b := make([]byte, 1)
	n, _ := c.Read(b)
	done <- n
}

func censusNetRead(k int) int {
	l, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		return -1
	}
	defer l.Close()
	accepted := make(chan net.Conn, k)
	go func() {
		for i := 0; i < k; i++ {
			c, err := l.Accept()
			if err != nil {
				close(accepted)
				return
			}
			accepted <- c
		}
	}()
	writers := make([]net.Conn, 0, k)
	for i := 0; i < k; i++ {
		c, err := net.Dial("tcp", l.Addr().String())
		if err != nil {
			return -1
		}
		writers = append(writers, c)
	}
	gate, done := make(chan struct{}), make(chan int, k)
	for i := 0; i < k; i++ {
		go connReader(gate, <-accepted, done)
	}
	close(gate)
	time.Sleep(300 * time.Millisecond)
	for _, c := range writers {
		c.Write([]byte{1})
	}
	n := 0
	for i := 0; i < k; i++ {
		n += <-done
	}
	return n
}

func waiter(gate chan struct{}, cmd *exec.Cmd, done chan<- int) {
	<-gate
	if cmd.Wait() == nil {
		done <- 1
		return
	}
	done <- 0
}

func censusExec(k int) int {
	self, err := os.Executable()
	if err != nil {
		return -1
	}
	gate, done := make(chan struct{}), make(chan int, k)
	for i := 0; i < k; i++ {
		cmd := exec.Command(self)
		cmd.Env = append(os.Environ(), "MN_INV_CHILD=1", "GO2CS_MN=")
		if cmd.Start() != nil {
			return -1
		}
		go waiter(gate, cmd, done)
	}
	close(gate)
	n := 0
	for i := 0; i < k; i++ {
		n += <-done
	}
	return n
}

func sleeper(gate chan struct{}, done chan<- int) {
	<-gate
	time.Sleep(300 * time.Millisecond)
	done <- 1
}

func censusSleep(k int) int {
	gate, done := make(chan struct{}), make(chan int, k)
	for i := 0; i < k; i++ {
		go sleeper(gate, done)
	}
	close(gate)
	n := 0
	for i := 0; i < k; i++ {
		n += <-done
	}
	return n
}

func stdinReader(gate chan struct{}) {
	<-gate
	b := make([]byte, 1)
	os.Stdin.Read(b)
}

// censusStdin cannot release its readers (nothing writes this process's stdin), so it reports and exits.
func censusStdin(k int) {
	gate := make(chan struct{})
	for i := 0; i < k; i++ {
		go stdinReader(gate)
	}
	close(gate)
	time.Sleep(300 * time.Millisecond)
	fmt.Println("C-stdin blocked:", k)
	os.Exit(0)
}

func resolver(gate chan struct{}, done chan<- int) {
	<-gate
	net.LookupHost("go2cs-census.invalid")
	done <- 1
}

func censusDNS(k int) int {
	gate, done := make(chan struct{}), make(chan int, k)
	for i := 0; i < k; i++ {
		go resolver(gate, done)
	}
	close(gate)
	n := 0
	for i := 0; i < k; i++ {
		n += <-done
	}
	return n
}

func runs(row string) bool {
	only := os.Getenv("MN_INV_ONLY")
	return only == "" || only == row
}

func main() {
	if os.Getenv("MN_INV_CHILD") == "1" {
		time.Sleep(300 * time.Millisecond)
		return
	}

	procs := runtime.GOMAXPROCS(0)
	k := 4 * procs

	switch os.Getenv("MN_INV_ONLY") {
	case "C-pipe":
		fmt.Println("C-pipe released:", censusPipe(k) == k)
		return
	case "C-accept":
		fmt.Println("C-accept released:", censusAccept(k) == k)
		return
	case "C-netread":
		fmt.Println("C-netread released:", censusNetRead(k) == k)
		return
	case "C-exec":
		fmt.Println("C-exec released:", censusExec(k) == k)
		return
	case "C-sleep":
		fmt.Println("C-sleep released:", censusSleep(k) == k)
		return
	case "C-chan":
		wait, set := channelEvent()
		fmt.Println("C-chan released:", blockAll(k, wait, set) == k)
		return
	case "C-stdin":
		censusStdin(k)
		return
	case "C-dns":
		fmt.Println("C-dns released:", censusDNS(k) == k)
		return
	}

	if runs("V1") {
		v1(k)
	}
	if runs("V2") {
		v2(k)
	}
	if runs("V4") {
		v4(procs)
	}
}
