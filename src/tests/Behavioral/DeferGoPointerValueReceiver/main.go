package main

import (
	"fmt"
	"sync"
)

// A deferred or spawned VALUE-method call through a POINTER copies *p when the defer or go statement runs,
// as it evaluates every other operand of the call there. A write through p after the statement does not
// reach the call, and a nil p panics at the statement, before anything after it runs. Covered: a pointer
// local deferred with and without an argument, a pointer parameter deferred with and without one, a go
// statement on a pointer local, and a nil pointer local deferred.

type path struct{ name string }

func (p path) Done()     { fmt.Println("done", p.name) }
func (p path) Pop(k int) { fmt.Println("pop", k, p.name) }
func (p path) Run(wg *sync.WaitGroup) {
	fmt.Println("run", p.name)
	wg.Done()
}

func pointerLocal() {
	p := &path{"pointee"}
	defer p.Done()
	*p = path{"replaced"}
}

func pointerLocalArg(k int) {
	p := &path{"pointee"}
	defer p.Pop(k)
	*p = path{"replaced"}
}

func pointerParam(p *path) {
	defer p.Done()
	*p = path{"replaced"}
}

func pointerParamArg(p *path, k int) {
	defer p.Pop(k)
	*p = path{"replaced"}
}

func goLocal() {
	var wg sync.WaitGroup
	wg.Add(1)
	p := &path{"pointee"}
	go p.Run(&wg)
	*p = path{"replaced"}
	wg.Wait()
}

func nilDefer() {
	defer func() { fmt.Println("recovered:", recover() != nil) }()
	var p *path
	defer p.Done()
	fmt.Println("after the defer statement")
}

func main() {
	pointerLocal()
	pointerLocalArg(1)
	pointerParam(&path{"pointee"})
	pointerParamArg(&path{"pointee"}, 2)
	goLocal()
	nilDefer()
}
