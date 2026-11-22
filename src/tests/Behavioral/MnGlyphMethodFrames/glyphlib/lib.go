// Package glyphlib is a library, so each of its methods is converted in the suffix form: the body `Name⁔` in the
// package class and a Go-named forwarder in the type's holder. Under generated twins a method that parks gets an
// async twin of its body, `Name⁔ꞏtwin`, whose frames run in a state machine `<Name⁔ꞏtwin>d__N`.
package glyphlib

import (
	"runtime"
	"strconv"
	"strings"
)

type Box struct {
	ch    chan int
	trail []string
}

func New() *Box {
	return &Box{ch: make(chan int)}
}

// Put parks until Take receives.
func (b *Box) Put(v int) {
	b.ch <- v
}

// Take parks on the channel, then reads its callers.
func (b *Box) Take() int {
	v := <-b.ch
	b.trail = append(b.trail, Frames("Take"))

	return v
}

func (b *Box) Trail() []string {
	return b.trail
}

// Pass is never inlined by Go; the C# body carries NoInlining, and a caller reaches it through its forwarder.
//
//go:noinline
func (b Box) Pass(n int) string {
	return Frames("Pass " + strconv.Itoa(n))
}

// Frames names the calling function and its callers, each with its line, up to main.main or the first runtime frame.
func Frames(tag string) string {
	pcs := make([]uintptr, 32)
	n := runtime.Callers(2, pcs)
	frames := runtime.CallersFrames(pcs[:n])

	var names []string

	for {
		frame, more := frames.Next()

		if strings.HasPrefix(frame.Function, "runtime.") {
			break
		}

		names = append(names, frame.Function+":"+strconv.Itoa(frame.Line))

		if !more || frame.Function == "main.main" {
			break
		}
	}

	return tag + ": " + strings.Join(names, " <- ")
}
