package main

import (
	"fmt"

	"GenericIfaceEmbedForwarder/workerlib"
)

// worker is satisfied by *gw[T] partly through gw's own pointer methods and partly through the embedded worker
// field, so gw's pointer method set needs a forwarder for each method only the embed provides.
type worker interface {
	run()
	name() string
	input(x any)
	inputs(xs ...int)
}

type gw[T any] struct {
	worker
	v T
}

func (w *gw[T]) run() { fmt.Println("run", w.v) }

func (w *gw[T]) name() string { return fmt.Sprint("gw ", w.v) }

type impl struct{ tag string }

func (impl) run() {}

func (impl) name() string { return "impl" }

func (i impl) input(x any) { fmt.Println(i.tag, "input", x) }

func (i impl) inputs(xs ...int) { fmt.Println(i.tag, "inputs", xs) }

// Two type parameters: the forwarders and the record carry both.
type pair[K comparable, V any] struct {
	worker
	k K
	v V
}

func (p *pair[K, V]) run() { fmt.Println("pair run", p.k, p.v) }

func (p *pair[K, V]) name() string { return fmt.Sprint("pair ", p.k) }

func use(w worker) {
	w.run()
	fmt.Println(w.name())
	w.input(7)
	w.inputs(1, 2)
}

func main() {
	use(&gw[int]{worker: impl{"a"}, v: 3})
	use(&gw[string]{worker: impl{"b"}, v: "s"})
	use(&pair[string, float64]{worker: impl{"c"}, k: "k", v: 1.5})

	// The promoted methods are callable on the struct pointer directly, too.
	g := &gw[bool]{worker: impl{"d"}, v: true}
	g.input("direct")
	g.inputs()

	// The same shape in a library, through its method holder.
	workerlib.Demo()
}
