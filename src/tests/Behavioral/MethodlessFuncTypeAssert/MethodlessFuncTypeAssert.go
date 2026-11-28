// Guards a TYPE ASSERTION whose target is a METHODLESS named func type — archive/zip's
// `ci.(Compressor)` where `type Compressor func(io.Writer) (io.WriteCloser, error)`. Such a type
// collapses to its base C# delegate everywhere it is referenced, so its NAME is never emitted; the
// type-assertion target must render the collapsed `Func<…>`, not the bare (undefined) name (CS0246).
//
// It also guards the PANIC TEXT of a failed single-value assertion to a func type, which golib's `_ᶠ`
// forms carry: a func-type assertion does not take the general `_<T>`, whose trim annotation reaches
// Delegate.CreateDelegate (IL2026/IL2111), so its source interface is named in the call wherever the
// converter can name it. Go prints that source: `main.Named is main.tag, not main.Compressor`.
package main

import "fmt"

type Compressor func(w int) int

func lookup(i any) Compressor {
	// i.(Compressor) — the type assertion whose target collapsed to a delegate.
	if c, ok := i.(Compressor); ok {
		return c
	}
	return nil
}

// Named is exported and declared in this package; hidden is unexported; fmt.Stringer is exported from
// another package. Handler is a func type with methods, so it implements all three and error, and is the
// only kind of func type a non-empty interface can assert to.
type Named interface{ Name() string }
type hidden interface{ name() string }

type Handler func(int) int

func (Handler) Name() string   { return "handler" }
func (Handler) name() string   { return "handler" }
func (Handler) Error() string  { return "handler" }
func (Handler) String() string { return "handler" }

type tag struct{}

func (tag) Name() string   { return "tag" }
func (tag) name() string   { return "tag" }
func (tag) Error() string  { return "tag" }
func (tag) String() string { return "tag" }

// panicText runs f and prints what it panicked with.
func panicText(f func()) {
	defer func() { fmt.Println(recover()) }()
	f()
}

func main() {
	var i any = Compressor(func(w int) int { return w * 2 })
	c := lookup(i)
	fmt.Println(c(21)) // 42

	// a non-matching dynamic type asserts false.
	var j any = 7
	fmt.Println(lookup(j) == nil) // true

	// Failed single-value assertions to a func type, each naming its source as Go does.
	var n Named = tag{}
	var h hidden = tag{}
	var e error = tag{}
	var s fmt.Stringer = tag{}
	var nilNamed Named
	var nilAny any

	panicText(func() { _ = j.(Compressor) })      // interface {} is int, not main.Compressor
	panicText(func() { _ = n.(Handler) })         // main.Named is main.tag, not main.Handler
	panicText(func() { _ = h.(Handler) })         // main.hidden is main.tag, not main.Handler
	panicText(func() { _ = e.(Handler) })         // error is main.tag, not main.Handler
	panicText(func() { _ = s.(Handler) })         // fmt.Stringer is main.tag, not main.Handler
	panicText(func() { _ = nilNamed.(Handler) })  // main.Named is nil, not main.Handler
	panicText(func() { _ = nilAny.(Compressor) }) // interface {} is nil, not main.Compressor
	panicText(func() { _ = i.(func(w int) int) }) // interface {} is main.Compressor, not func(int) int
	fmt.Println(i.(Compressor)(4))                // 8

	// The same assertions succeed on a Handler, through each source.
	double := Handler(func(w int) int { return w * 2 })
	n, h, e, s = double, double, double, double
	fmt.Println(n.(Handler)(1), h.(Handler)(2), e.(Handler)(3), s.(Handler)(4))
}
