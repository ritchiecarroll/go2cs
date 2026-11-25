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
//	   200 times: the wakes all complete; their latency goes to stderr with MN_INV_READING=1;
//	V5 K goroutines block in a native sleep-poll loop that consumer C# brackets with GoNativeCall.Enter(Blocks) and
//	   Exit: under GO2CS_MN=1 they hand their Ps off at once (handoffs rise, retakes do not, while they are blocked);
//	V6 the same bracketed with Enter(MayBlock): the scheduler retakes the Ps (retakes rise) and injects none.
//
// MN_INV_ONLY=V7 (never in the compared run: Go completes it) sets the thread ceiling with debug.SetMaxThreads(16),
// prints the old ceiling, and then runs V2: under GO2CS_MN=1 the carriers V2 needs pass the ceiling, and the program
// ends as Go's does past it ("runtime: program exceeds 16-thread limit", "fatal error: thread exhaustion", exit 2).
//
// The census rows (3d) run only when MN_INV_ONLY names one (C-pipe, C-accept, C-netread, C-exec, C-sleep, C-chan,
// C-stdin, C-dns): K goroutines block in one standard-library call at once, main waits, then releases them. The
// scheduler's READING line (GO2CS_MN_READING=1) says whether the blocks were seen: handoffs or retakes, or only
// injections. MN_INV_WARM=1 runs a census row's pass twice and reads the second (census, below). MN_INV_ONLY=V<n> runs
// one V row alone.
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
	"runtime/debug"
	"sort"
	"strconv"
	"time"
)

// newManagedEvent returns a wait that blocks until set is called. nil under Go; hand_impl.cs sets a .NET event.
var newManagedEvent func() (wait func(), set func())

// newNativeFlag returns a wait that polls in native code until set is called. nil under Go; hand_impl.cs sets it.
var newNativeFlag func() (wait func(), set func())

// newBracketedFlag is newNativeFlag with the native call bracketed by GoNativeCall.Enter / Exit, with the Blocks hint
// or MayBlock. nil under Go; hand_impl.cs sets it.
var newBracketedFlag func(blocks bool) (wait func(), set func())

// counters reads the scheduler's handoffs, retakes, injections and sensed waits, and whether it is on. nil under Go.
var counters func() (handoffs, retakes, injections, sensed int64, on bool)

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
	return blockAllThen(k, wait, set, nil)
}

// blockAllThen is blockAll that calls blocked (when not nil) while the k goroutines are still blocked, just before set:
// what the scheduler did about the blocks is read there, before the release's own traffic.
func blockAllThen(k int, wait func(), set func(), blocked func()) int {
	gate := make(chan struct{})
	done := make(chan int, k)
	for i := 0; i < k; i++ {
		go blocker(gate, wait, done)
	}
	close(gate)
	time.Sleep(50 * time.Millisecond)
	if blocked != nil {
		blocked()
	}
	set()
	n := 0
	for i := 0; i < k; i++ {
		n += <-done
	}
	return n
}

// v1: under GO2CS_MN=1 the scheduler must have SENSED the waits (N5 3a), not injected Ps for them. A few injections in
// the window are main's own step that starts the k goroutines while they queue (traced on linux: goroutine 1, 20 to 60 ms),
// which the policy injects for; the blocks themselves leave fewer than k/3. The same bound holds for V6.
func v1(k int) {
	mk := newManagedEvent
	if mk == nil {
		mk = channelEvent
	}
	ww, ws := channelEvent()
	blockAll(k, ww, ws)
	var i0, s0, i1, s1 int64
	on := false
	if counters != nil {
		_, _, i0, s0, _ = counters()
	}
	wait, set := mk()
	ok := blockAllThen(k, wait, set, func() {
		if counters != nil {
			_, _, i1, s1, on = counters()
		}
	}) == k
	if on {
		ok = ok && s1 > s0 && i1-i0 < int64(k/3)
	}
	fmt.Println("V1 goroutines released from a managed wait, which the scheduler sensed:", ok)
}

func v2(k int) {
	mk := newNativeFlag
	if mk == nil {
		mk = channelEvent
	}
	wait, set := mk()
	fmt.Println("V2 goroutines released from a native wait:", blockAll(k, wait, set) == k)
}

// bracketed runs V5 or V6: k goroutines blocked in a bracketed native wait, and what the scheduler did about them.
func bracketed(k int, blocks bool) bool {
	mk := newBracketedFlag
	if mk == nil {
		mk = func(bool) (func(), func()) { return channelEvent() }
	}
	// A warm-up pass first, so main's own first steps (which start k goroutines) run before the counters are read.
	ww, ws := channelEvent()
	blockAll(k, ww, ws)
	var h0, r0, i0, h1, r1, i1 int64
	on := false
	if counters != nil {
		h0, r0, i0, _, _ = counters()
	}
	wait, set := mk(blocks)
	released := blockAllThen(k, wait, set, func() {
		if counters != nil {
			h1, r1, i1, _, on = counters()
		}
	}) == k
	switch {
	case !on:
		return released
	case blocks:
		return released && h1 > h0 && r1 == r0
	default:
		return released && r1 > r0 && i1-i0 < int64(k/3)
	}
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
	gate, done := make(chan struct{}), make(chan int, k)
	for i := 0; i < k; i++ {
		// The child is a system command that waits about a second: a converted program may run under the dotnet host,
		// where os.Executable names dotnet, which exits at once with no program to run.
		cmd := exec.Command("sleep", "0.3")
		if runtime.GOOS == "windows" {
			cmd = exec.Command("cmd", "/c", "ping -n 2 127.0.0.1 > nul")
		}
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

// census runs one census row. With MN_INV_WARM=1 it runs the pass once first, so the measured pass does not count the
// first pass's one-time costs (JIT, type initialization, the first timer and socket setup) as long steps; with
// MN_INV_READING=1 it prints what the scheduler did during the measured pass alone.
func census(row string, k int, pass func(int) int) {
	if os.Getenv("MN_INV_WARM") == "1" {
		pass(k)
	}
	var h0, r0, i0, s0 int64
	if counters != nil {
		h0, r0, i0, s0, _ = counters()
	}
	released := pass(k) == k
	if counters != nil && reading {
		h1, r1, i1, s1, _ := counters()
		fmt.Fprintf(os.Stderr, "reading: %s handoffs=%d retakes=%d injections=%d sensed=%d\n", row, h1-h0, r1-r0, i1-i0, s1-s0)
	}
	fmt.Println(row, "released:", released)
}

func runs(row string) bool {
	only := os.Getenv("MN_INV_ONLY")
	return only == "" || only == row
}

func main() {
	procs := runtime.GOMAXPROCS(0)
	k := 4 * procs

	switch os.Getenv("MN_INV_ONLY") {
	case "C-pipe":
		census("C-pipe", k, censusPipe)
		return
	case "C-accept":
		census("C-accept", k, censusAccept)
		return
	case "C-netread":
		census("C-netread", k, censusNetRead)
		return
	case "C-exec":
		census("C-exec", k, censusExec)
		return
	case "C-sleep":
		census("C-sleep", k, censusSleep)
		return
	case "C-chan":
		census("C-chan", k, func(k int) int {
			wait, set := channelEvent()
			return blockAll(k, wait, set)
		})
		return
	case "C-stdin":
		censusStdin(k)
		return
	case "V7":
		fmt.Println("V7 SetMaxThreads returned the old ceiling:", debug.SetMaxThreads(16))
		v2(k)
		return
	case "C-dns":
		census("C-dns", k, censusDNS)
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
	if runs("V5") {
		fmt.Println("V5 a Blocks native call hands its P off at once:", bracketed(k, true))
	}
	if runs("V6") {
		fmt.Println("V6 a MayBlock native call has its P retaken, not injected:", bracketed(k, false))
	}
}
