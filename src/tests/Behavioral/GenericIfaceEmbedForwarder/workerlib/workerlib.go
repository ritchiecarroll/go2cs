// Package workerlib holds the generic embed shape in a library, where a type's methods and forwarders live in its
// method holder class (ants' goWorkerWithFuncGeneric is this shape).
package workerlib

import "fmt"

type worker interface {
	run()
	input(x any)
	inputs(xs ...int)
}

type gw[T any] struct {
	worker
	v T
}

func (w *gw[T]) run() { fmt.Println("lib run", w.v) }

type impl struct{ tag string }

func (impl) run() {}

func (i impl) input(x any) { fmt.Println(i.tag, "lib input", x) }

func (i impl) inputs(xs ...int) { fmt.Println(i.tag, "lib inputs", xs) }

func use(w worker) {
	w.run()
	w.input(9)
	w.inputs(3)
}

// Demo exercises the shape through the interface and on the struct pointer directly.
func Demo() {
	use(&gw[int]{worker: impl{"x"}, v: 4})
	g := &gw[string]{worker: impl{"y"}, v: "z"}
	g.input("direct")
}
