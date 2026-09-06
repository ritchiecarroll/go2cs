package main

import (
	"fmt"
	"net"
	"runtime"
)

// A PROBE, not a guard (never for merge): does darwin increment 12's Getaddrinfo hand `net` a chain
// it can walk, and does the CONSUMER's port alias survive reading it?
//
// WHY TWO ARMS, AND WHY THE OBVIOUS ONE IS NOT ENOUGH. net/lookup_unix.go:72 tries cgoLookupPort
// FIRST and SWALLOWS its error into goLookupPort ("Issue 18213: if cgo fails, first check to see
// whether we have the answer baked-in"), and goLookupPort reads /etc/services, which every macOS
// runner has with `http 80`. So the LookupPort arm ALONE answers 80 on both arms of the A/B
// whenever the seam returns an error instead of dying -- a control that cannot vary the axis it is
// pointed at. It is here because a CRASH it cannot swallow is the likely shape (an AUTO-layout
// record and a value-peeked out-cell handed to libc kill the process rather than return an errno),
// and increment 11's pair read exactly that way.
//
// The LookupHost arm is the one with no fallback to hide behind: lookup_unix.go:63 calls
// cgoLookupIP OUTRIGHT when hostLookupOrder answers hostLookupCgo, so its answer is attributable --
// at the cost of depending on conf.go's ordering heuristic on the runner, which is why it is the
// second arm rather than the only one.
//
// READING THE RESULT: an IDENTICAL pair across the two arms of the A/B is reported as DID NOT
// MEASURE, never as a pass. With this fallback in the path, two green arms are the expected output
// of a probe that never reached the seam.
//
// Every line printed is a BOOLEAN or a fixed number -- no address, no host name, no count that
// varies with the runner's resolver -- so Go and C# can be compared byte for byte.
func main() {
	if runtime.GOOS != "darwin" {
		// The seam is darwin-only (net/cgo_unix_syscall.go is `!netgo && darwin`), and there is no
		// linux or windows cgo_unix.cs at all, so any other platform has nothing to say here.
		fmt.Println("not darwin: probe skipped")
		fmt.Println("done")
		return
	}

	// ARM 1 -- the shortest path to _C_getaddrinfo. Masked by the /etc/services fallback on an
	// error return; NOT masked by a crash.
	port, err := net.LookupPort("tcp", "http")
	fmt.Println("LookupPort tcp/http == 80:", port == 80, "err==nil:", err == nil)

	// ARM 2 -- no baked-in fallback when the order is cgo. `localhost` needs no external DNS, so a
	// runner with no network still exercises the seam.
	addrs, err2 := net.LookupHost("localhost")
	fmt.Println("LookupHost localhost err==nil:", err2 == nil, "any:", len(addrs) > 0)

	fmt.Println("done")
}
