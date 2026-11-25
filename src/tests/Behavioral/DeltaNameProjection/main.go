// DeltaNameProjection: a Go name that starts with Δ is the user's own, and keeps its Δ through reflect, encoding/json,
// fmt and runtime frames, as in Go. The converter's own collision renames (a field named slice) still project to
// the Go name.
package main

import (
	"encoding/json"
	"fmt"
	"reflect"
	"runtime"

	"DeltaNameProjection/lib"
)

type P struct{ Δt float64 }

type ΔMatrix struct{ n int }

type Matrix struct{ m int }

type Namer interface{ Name() string }

func (m ΔMatrix) Name() string { return fmt.Sprint("ΔMatrix ", m.n) }

func (m Matrix) Name() string { return fmt.Sprint("Matrix ", m.m) }

type S struct{}

func (S) ΔFoo() string { return "ΔFoo called" }

type Fooer interface{ Foo() string }

// A field named like its struct.
type Δu struct{ Δu int }

// The same struct name without the field's Δ.
type Δv struct{ v int }

// A field named slice, which the converter renames: its Go name is still "slice".
type R struct{ slice int }

// A lower-case δ is not upper-case: δt is unexported.
type L struct {
	δt int
	X  int
}

type ΔEmb struct{ e int }

type Outer struct {
	ΔEmb
	k int
}

type Pair struct{ a, b int }

// A struct the converter emits as Δslice (slice is a golib name) with a field the Go source spells Δslice: the
// emitted names are equal, so the field is renamed, and its Go name is still Δslice.
type slice struct{ Δslice int }

// A field named like its struct, with no Δ anywhere: the converter's own rename, reported as Go's name.
type Node struct{ Node int }

// An embedded interface whose Go name starts with Δ: its field is named ΔFooer and promotes ΔFoo.
type ΔFooer interface{ ΔFoo() string }

type Wrap struct{ ΔFooer }

// A Δ-led method one name away from String: fmt does not call it.
type T struct{ n int }

func (T) ΔString() string { return "never" }

// Pointer-receiver methods on a Δ-led type, one of them Δ-led, and a Δ-led method an embed promotes.
func (m *ΔMatrix) Bump() { m.n++ }

func (p *P) ΔScale(k float64) { p.Δt *= k }

func (ΔEmb) ΔHello() string { return "ΔHello" }

func Δf(p Pair) (string, int) {
	pc, _, line, _ := runtime.Caller(0)
	return runtime.FuncForPC(pc).Name(), line + p.a - p.b
}

// S2 satisfies lib.Lower: Δfoo is Δ then a lower-case letter, exported in Go.
type S2 struct{ s string }

func (s S2) Δfoo() string { return "Δfoo of " + s.s }

// OuterL embeds another package's ΔEmbL and promotes its Δm.
type OuterL struct{ lib.ΔEmbL }

// HasRec holds another package's Δ-led lower-case type.
type HasRec struct{ R lib.Δrec }

// A bare Δ type with a bare Δ field, declared here.
type Δ struct{ Δ int }

// A package-level Δ-led named func type.
type Δg func() string

// An embed whose emitted name is Δslice, promoting slice's field Δslice, beside an own field one letter away. (An own
// field spelled Δslice here would share the embed's C# name, and GO2CS2015 refuses it.)
type X struct {
	slice
	Δslicex int
}

func main() {
	b, _ := json.Marshal(P{1})
	fmt.Println("json:", string(b))
	fmt.Println("field name:", reflect.TypeOf(P{}).Field(0).Name)
	fmt.Println("type name:", reflect.TypeOf(ΔMatrix{}).Name(), reflect.TypeOf(Matrix{}).Name())
	fmt.Printf("%%T: %T %T\n", ΔMatrix{}, Matrix{})
	for _, n := range []Namer{ΔMatrix{1}, Matrix{2}} {
		fmt.Printf("namer: %s %T\n", n.Name(), n)
	}
	var v any = S{}
	_, ok := v.(Fooer)
	fmt.Println("assert Foo:", ok, reflect.TypeOf(S{}).Method(0).Name)
	_, ok = v.(lib.Fooer)
	fmt.Println("assert ΔFoo:", ok, lib.Call(S{}))

	name, line := Δf(Pair{3, 3})
	fmt.Println("frame:", name, line)

	fmt.Println("field like struct:", reflect.TypeOf(Δu{}).Field(0).Name, Δu{Δu: 5}.Δu)
	fmt.Println("struct Δv field:", reflect.TypeOf(Δv{}).Field(0).Name, Δv{v: 6}.v)
	fmt.Println("renamed field:", reflect.TypeOf(R{}).Field(0).Name)
	lb, _ := json.Marshal(L{1, 2})
	fmt.Println("lower δ:", reflect.TypeOf(L{}).Field(0).Name, reflect.TypeOf(L{}).Field(0).IsExported(), string(lb))
	of := reflect.TypeOf(Outer{}).Field(0)
	fmt.Println("embedded:", of.Name, of.Anonymous, Outer{ΔEmb: ΔEmb{4}}.e)

	type Δx struct{ q int }
	fmt.Printf("lifted: %T %s\n", Δx{}, reflect.TypeOf(Δx{}).Name())

	Δlocal := 8
	fmt.Println("local and param:", Δlocal, twice(Δlocal))

	r := lib.Rec{Δt: 9}
	rb, _ := json.Marshal(r)
	fmt.Println("other package:", r.Δt, lib.ΔF(5), lib.ΔK, lib.ΔV, string(rb))

	sl := slice{Δslice: 10}
	sl.Δslice++
	fmt.Println("emitted-equal field:", reflect.TypeOf(sl).Name(), reflect.TypeOf(sl).Field(0).Name, sl.Δslice)
	fmt.Println("plain field like struct:", reflect.TypeOf(Node{}).Field(0).Name, Node{Node: 12}.Node)

	w := Wrap{S{}}
	wf := reflect.TypeOf(w).Field(0)
	fmt.Println("embedded interface:", wf.Name, wf.Anonymous, w.ΔFoo(), methodNames(reflect.TypeOf(w)))

	fmt.Println("near String:", T{13}, fmt.Sprintf("%v %+v", T{14}, T{15}))

	m := ΔMatrix{20}
	m.Bump()
	pm := &m
	pm.Bump()
	fmt.Println("pointer methods:", m.n, pm.Name(), methodNames(reflect.TypeOf(pm)), methodNames(reflect.TypeOf(m)))

	pp := &P{2}
	pp.ΔScale(3)
	fmt.Println("Δ pointer method:", pp.Δt, methodNames(reflect.TypeOf(pp)))

	o := Outer{ΔEmb: ΔEmb{30}}
	fmt.Println("promoted Δ method:", o.ΔHello(), methodNames(reflect.TypeOf(o)))

	var h interface{ ΔHello() string } = o
	fmt.Println("assert promoted:", h.ΔHello())
	_, isStringer := any(T{}).(fmt.Stringer)
	fmt.Println("T is Stringer:", isStringer)

	var fw ΔFooer = &w
	fmt.Println("embedded interface by pointer:", fw.ΔFoo(), lib.Call(&w), lib.Call(w))
	var hv any = H{}
	_, hNamer := hv.(Namer)
	_, hFooer := hv.(ΔFooer)
	fmt.Println("named not embedded:", hNamer, hFooer, methodNames(reflect.TypeOf(H{})), methodNames(reflect.TypeOf(&H{})))

	fb, _ := json.Marshal(struct{ F file }{file{16}})
	fmt.Println("doubled marker:", reflect.TypeOf(file{}).Name(), reflect.TypeOf(file{}).Field(0).Name, file{file: 16}.file,
		reflect.TypeOf(union{}).Name(), reflect.TypeOf(union{}).Field(0).Name, union{union: 17}.union, string(fb))
	fmt.Printf("%%+v: %+v %+v\n", file{18}, union{19})

	fmt.Println("lower after Δ, direct:", lib.CallLower(S2{"S2"}))
	var lo interface{ Δfoo() string } = S2{"iface"}
	fmt.Println("lower after Δ, through an interface:", lib.CallLower(lo), methodNames(reflect.TypeOf(S2{})))
	ol := OuterL{lib.ΔEmbL{N: 40}}
	_, olOK := any(ol).(interface{ Δm() string })
	fmt.Println("promoted foreign Δm:", ol.Δm(), olOK, methodNames(reflect.TypeOf(ol)))
	rp := &r.Δt
	*rp = 41
	fmt.Println("address of a foreign Δ field:", r.Δt)
	dr := lib.Δrec{Δt: 42}
	hr := HasRec{dr}
	fmt.Printf("foreign Δ type: %T %+v %v %s\n", dr, hr, hr.R.Δt, reflect.TypeOf(hr).Field(0).Type.Name())
	fmt.Printf("bare Δ: %T %+v %s %T %+v %s\n", Δ{43}, Δ{Δ: 44}, reflect.TypeOf(Δ{}).Field(0).Name,
		lib.Δ{Δ: 45}, lib.Δ{Δ: 46}, reflect.TypeOf(lib.Δ{}).Field(0).Name)
	wr := lib.WrapRec{}
	wr.Δt = 47
	wp := &wr.Δt
	*wp++
	fmt.Println("promoted foreign Δ field:", wr.Δt, wr.Rec.Δt)
	x := X{slice{48}, 49}
	x.Δslice++
	xp := &x.Δslice
	*xp++
	x.Δslicex++
	xf := reflect.TypeOf(x)
	fmt.Println("embed named Δslice in C#:", x.Δslice, x.slice.Δslice, x.Δslicex, xf.Field(0).Name, xf.Field(0).Anonymous,
		xf.Field(1).Name, fmt.Sprintf("%+v", x))
	var dbl lib.ΔFn = func(n int) int { return n * 2 }
	var g Δg = func() string { return "Δg called" }
	var lf lib.Δfn = func() string { return "lib.Δfn called" }
	type Δh func(int) int
	var hf Δh = func(n int) int { return n + 1 }
	fmt.Printf("Δ func types: %d %d %s %s %d %T %T %T %T %s %s\n", lib.ApplyΔ(dbl, 50), lib.ApplyΔ(lib.ΔFn(hf), 51), g(), lf(), hf(52),
		dbl, g, lf, hf, reflect.TypeOf(g).Name(), reflect.TypeOf(hf).Name())
}

func methodNames(t reflect.Type) []string {
	var names []string
	for i := range t.NumMethod() {
		names = append(names, t.Method(i).Name)
	}
	return names
}

func twice(Δn int) int { return Δn * 2 }

// A field named Matrix of type *ΔMatrix and a field named Fooer of type ΔFooer, one Δ away from embeds: neither is
// embedded, so H promotes no method.
type H struct {
	Matrix *ΔMatrix
	Fooer  ΔFooer
}

// Structs named like their fields whose emitted names take two markers: file is a C# keyword (the struct is @file,
// the field ΔΔfile), and union is a name the converter renames (the struct Δunion, the field ΔΔunion). Go's names
// are file and union.
type file struct{ file int }

type union struct{ union int }
