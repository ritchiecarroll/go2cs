// The NARROW shape witness, every piece in one package (stage 1): a generic declaration under a NON-self method-set
// constraint whose own signature puts T where the proxy routes refuse it -- a generic named type, or T inside a result
// -- takes a witness instead of a proxy. T stays the pointer itself, so results, slices and named types keep Go's own
// element type and aliasing. Shapes from the TS readiness census (tsc's lsconv, autoimport) and gc/internal/gen.
package main

import (
	"fmt"
	stdio "io"
	"strings"
)

// ---- lsconv's shapes ----

type Script interface {
	FileName() string
	Text() string
}

type SourceFile struct{ name, text string }

func (f *SourceFile) FileName() string { return f.name }
func (f *SourceFile) Text() string     { return f.text }

// No methods: no witness parameter, so its identity is Go's MappedSpan[T].
type MappedSpan[T Script] struct {
	Script T
	Start  int
}

func FromLSPRange[T Script](script T, start int) []MappedSpan[T] {
	return lspRangeToVirtual([]T{script}, start)
}

func lspRangeToVirtual[T Script](scripts []T, start int) []MappedSpan[T] {
	var out []MappedSpan[T]
	for i, s := range scripts {
		out = append(out, MappedSpan[T]{s, start + i + len(s.Text())})
	}
	return out
}

// A NON-generic caller instantiating with the pointer (lsconv converters.go:136's shape).
func FromLSPRangeForSourceFile(files []*SourceFile, start int) []MappedSpan[*SourceFile] {
	return lspRangeToVirtual(files, start)
}

// T only as a plain parameter, but it forwards T into a selected declaration: the closure selects dump too.
func dump[T Script](x T) any { return FromLSPRange(x, 0) }

// ---- autoimport's shape: a witness-carrying type in a field, with a pointer type argument ----

type Named interface{ Name() string }

type Export struct{ name string }

func (e *Export) Name() string { return e.name }

type Index[T Named] struct{ entries []T }

// nameOf takes T only as a plain parameter, but a witnessed body hands it a witnessed T: the closure selects it DOWNWARD
// too, so it takes the witness rather than keeping a C# clause the box cannot satisfy.
func nameOf[T Named](x T) string { return x.Name() }

func (idx *Index[T]) Find(name string) []T {
	var out []T
	for _, e := range idx.entries {
		if nameOf(e) == name {
			out = append(out, e)
		}
	}
	return out
}

func (idx *Index[T]) Add(e T) { idx.entries = append(idx.entries, e) }

type registry struct{ exports Index[*Export] }

// ---- gc/internal/gen's shapes: value implementers, and a type whose own method calls T's ----

type Value interface{ kind() string }

type u8 struct{ v byte }

func (u8) kind() string { return "u8" }

type Ptr[T Value] struct{ off int }

func (Ptr[T]) kind() string { var x T; return "Ptr[" + x.kind() + "]" }

type Array[T Value] struct{ vals []T }

func (Array[T]) kind() string { var x T; return "Array[" + x.kind() + "]" }

func ConstArray[T Value](vals []T, name string) (y Array[T]) {
	y.vals = vals
	return y
}

// ---- the generic witness: a VALUE implementer, an INTERFACE type argument, and a carrier selected by a pointer ----

// memFile implements Script by value, so FromLSPRange binds the generic witness ScriptΔshapeOf<memFile>.
type memFile struct{ name string }

func (m memFile) FileName() string { return m.name }
func (m memFile) Text() string     { return "mem:" + m.name }

// boxed implements Value only through its pointer: ConstArray([]*boxed) keeps Array and ConstArray selected, so the u8
// instantiation above names ValueΔshapeOf<u8>, and Array's own kind() calls T's through its witness.
type boxed struct{ n int }

func (*boxed) kind() string { return "boxed" }

func main() {
	a := &SourceFile{"a.ts", "let x"}
	spans := FromLSPRange(a, 10)
	fmt.Println("FromLSPRange:", len(spans), spans[0].Start, spans[0].Script == a)
	spans[0].Script.text = "changed through the result"
	fmt.Println("write through the result:", a.text)

	files := []*SourceFile{a, {"b.ts", "yz"}}
	masked := FromLSPRangeForSourceFile(files, 0)
	fmt.Println("masked site:", len(masked), masked[1].Start, masked[1].Script == files[1], fmt.Sprintf("%T", masked))

	_, ok := dump(a).([]MappedSpan[*SourceFile])
	fmt.Println("dump through any:", ok)

	var r registry
	r.exports.Add(&Export{"x"})
	r.exports.Add(&Export{"y"})
	found := r.exports.Find("x")
	fmt.Println("index field:", len(found), found[0] == r.exports.entries[0])
	found[0].name = "x2"
	fmt.Println("write through Find:", r.exports.entries[0].name)

	p := Ptr[u8]{3}
	fmt.Println("gen Ptr:", p.kind(), p.off)
	arr := ConstArray([]u8{{1}, {2}}, "k")
	arr.vals[0].v = 9
	fmt.Println("gen ConstArray:", arr.kind(), len(arr.vals), arr.vals[0].v)

	var sb strings.Builder
	for _, s := range lspRangeToVirtual(files, 1) {
		sb.WriteString(s.Script.FileName())
	}
	fmt.Println("range over the result:", sb.String())

	mems := FromLSPRange(memFile{"m.ts"}, 2)
	fmt.Println("value implementer:", len(mems), mems[0].Start, mems[0].Script.Text())

	var iface Script = a
	viaIface := FromLSPRange(iface, 0)
	fmt.Println("interface argument:", viaIface[0].Script == iface, viaIface[0].Start)

	b := &boxed{7}
	boxes := ConstArray([]*boxed{b}, "b")
	boxes.vals[0].n = 8
	fmt.Println("pointer carrier:", boxes.kind(), b.n, boxes.vals[0] == b)

	// Written instantiations carry no witness in the Go syntax: a carrier's composite literal and an explicit function
	// instantiation both render from the resolved type.
	var asValue Value = Array[u8]{vals: []u8{{5}}}
	explicit := FromLSPRange[*SourceFile](a, 3)
	fmt.Println("written instantiations:", asValue.kind(), explicit[0].Start, explicit[0].Script == a)

	srcs, n := Readers([]*memSrc{{"abc"}, {"de"}})
	fmt.Println("foreign type in the signature:", len(srcs), n, srcs[1].data)
	fmt.Println("package Span:", Span[int]{1, 2}.hi)

	// Go's f(s...) hands the callee the caller's slice itself, through a witness as anywhere else: the callee's
	// write reaches parts.
	parts := []string{"x", "y"}
	stamped := StampThrough(srcs[0], parts)
	fmt.Println("spread through the witness:", parts[0], parts[1], stamped[0] == srcs[0])
}

// ---- a constraint whose method names ANOTHER package's type, imported under an ALIAS ----

// The witness declarations live in package_info.cs, where `stdio` is not in scope: they must name io.Reader by its
// package class, not by this file's alias.
// Join and Stamp are variadic: a witness member takes `params global::go.variadic<...>`, so a spread passes through it
// whole, and package_info.cs has no `using System;`.
type Source interface {
	Open() stdio.Reader
	Join(sep string, parts ...string) string
	Stamp(parts ...string)
}

// A package type named Span: the witness declarations sit inside this package's class, where it hides System.Span.
// The variadic witness member names its pack type in full (global::go.variadic), as a package type could be named
// variadic too.
type Span[T any] struct{ lo, hi T }

type memSrc struct{ data string }

func (m *memSrc) Open() stdio.Reader { return strings.NewReader(m.data) }

func (m *memSrc) Join(sep string, parts ...string) string {
	return m.data + ":" + strings.Join(parts, sep)
}

// Stamp hands its pack on as a slice, so the callee's pack becomes a slice: for a spread, the caller's own.
func (m *memSrc) Stamp(parts ...string) { stampFirst(parts, m.data) }

func stampFirst(dst []string, v string) { dst[0] = v }

// StampThrough spreads the caller's slice into a method reached through the shape witness: []T in its result selects
// the witness route (a proxy cannot carry T into a slice), so the call is a static call through the witness.
func StampThrough[T Source](s T, parts []string) []T {
	s.Stamp(parts...)
	return []T{s}
}

func Readers[T Source](srcs []T) ([]T, int) {
	n := 0
	for _, s := range srcs {
		b, _ := stdio.ReadAll(s.Open())
		n += len(b)
		n += len(s.Join(",", "x", "y"))
	}
	return srcs, n
}
