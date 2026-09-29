package main

import (
	"fmt"
	"runtime"
)

// runtime.Frame.Func is nil only for an INLINED frame (runtime.Frame's own doc: "Func may be nil for
// non-Go code or fully inlined functions"). A frame of a Go function that is not inlined carries its
// function's *Func, whose Name() is the frame's Function, and every frame of one function carries the SAME
// *Func (it points at the function's one symbol-table entry). The functions below are //go:noinline so that
// Go's own frames are real too.

//go:noinline
func capture() runtime.Frame {
	pcs := make([]uintptr, 1)
	runtime.Callers(2, pcs)
	frame, _ := runtime.CallersFrames(pcs).Next()
	return frame
}

//go:noinline
func twoSites() (runtime.Frame, runtime.Frame) {
	first := capture()
	second := capture()
	return first, second
}

//go:noinline
func other() runtime.Frame {
	return capture()
}

func main() {
	first, second := twoSites()
	third := other()

	fmt.Println("function:", first.Function)
	fmt.Println("func non-nil:", first.Func != nil)
	fmt.Println("func name matches:", first.Func != nil && first.Func.Name() == first.Function)
	fmt.Println("same function, two call sites, same *Func:", first.Func != nil && first.Func == second.Func)
	fmt.Println("different function, different *Func:", third.Func != nil && third.Func != first.Func)
	fmt.Println("other's name:", third.Func != nil && third.Func.Name() == "main.other")
}
