package main

import (
	"fmt"
	"runtime"
	"time"
)

// Guards the hand-owned runtime.SetFinalizer bridge (core/runtime/mfinal.cs), which has no
// other coverage: the converted body walks type descriptors that are meaningless against
// managed objects, so it is replaced by a ConditionalWeakTable registration keyed on the
// REFERENT rather than on the pointer box — a go2cs box is frequently a per-expression
// temporary, so keying on it would register against a lifetime nothing in the program shares.
//
// Two halves, because a bridge that fires unconditionally passes the first one alone:
//   registered — a finalizer on an unreachable object RUNS
//   cleared    — SetFinalizer(p, nil) means it does NOT
//
// Both loops are BOUNDED so this reports a verdict either way instead of hanging; Go makes no
// promptness guarantee, but with explicit runtime.GC() both outcomes are stable (measured 8/8
// per side before this was written, at an earlier Go release; see payload for 1.26.9).

// payload is 16 bytes on purpose. A pointer-free object under 16 bytes goes to Go's tiny
// allocator, which packs several into one 16-byte block, and runtime.SetFinalizer's doc says
// the finalizer for an unreferenced object in such a block "may never run if it always exists
// in the same batch as a referenced object". With the earlier 8-byte payload{n int}, Go 1.26.9
// on linux printed "registered finalizer: DID NOT RUN" in 20 of 20 runs here and in 3 of 5 on
// another box, so Go's own output was not a stable oracle. At 16 bytes it RAN 20 of 20, as did a
// pointer-bearing payload, and "cleared" stayed DID NOT RUN 20 of 20 (2026-10-19).
type payload struct{ n, pad int }

// registered drops the only reference to a finalized object and reports whether it ran.
func registered() string {
	done := make(chan struct{})

	func() {
		p := &payload{n: 1}
		runtime.SetFinalizer(p, func(*payload) { close(done) })
		_ = p.n
	}()

	for i := 0; i < 400; i++ {
		select {
		case <-done:
			return "RAN"
		case <-time.After(5 * time.Millisecond):
			runtime.GC()
		}
	}
	return "DID NOT RUN"
}

// cleared registers a finalizer and then withdraws it before dropping the reference.
func cleared() string {
	done := make(chan struct{})

	func() {
		p := &payload{n: 2}
		runtime.SetFinalizer(p, func(*payload) { close(done) })
		runtime.SetFinalizer(p, nil)
		_ = p.n
	}()

	for i := 0; i < 40; i++ {
		select {
		case <-done:
			return "RAN"
		case <-time.After(5 * time.Millisecond):
			runtime.GC()
		}
	}
	return "DID NOT RUN"
}

func main() {
	fmt.Println("registered finalizer:", registered())
	fmt.Println("cleared finalizer:", cleared())
}
