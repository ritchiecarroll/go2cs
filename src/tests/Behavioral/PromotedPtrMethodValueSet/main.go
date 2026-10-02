package main

import (
	"bufio"
	"fmt"
	"io"
	"reflect"
	"strings"
)

// A POINTER-receiver method promoted through an embed belongs to the outer type's VALUE method set
// exactly when the embed path crosses a pointer; through value embeds only, it belongs to the
// pointer method set alone. Every arm below is read three ways: the reflect method table, a
// method value called through reflect, and a dynamic interface assertion.

type Inner struct{ n int }

func (i *Inner) Bump() int { i.n++; return i.n }
func (i Inner) Get() int   { return i.n }

// Pointer hop at depth 1: Bump is in SameP's value set.
type SameP struct{ *Inner }

// Value hop only: Bump is in *SameV's set, not SameV's.
type SameV struct{ Inner }

// Depth 2, the pointer hop second: Outer{Mid{*Inner}}.
type MidP struct{ *Inner }
type OuterVP struct{ MidP }

// Depth 2, the pointer hop first: Outer{*Mid{Inner}}.
type MidV struct{ Inner }
type OuterPV struct{ *MidV }

// Depth 2, value hops only: Bump stays out of the value set.
type OuterVV struct{ MidV }

type bumper interface{ Bump() int }

func names(t reflect.Type) string {
	var sb strings.Builder

	for i := 0; i < t.NumMethod(); i++ {
		if i > 0 {
			sb.WriteString(",")
		}

		sb.WriteString(t.Method(i).Name)
	}

	return sb.String()
}

func describe(label string, v any) {
	t := reflect.TypeOf(v)
	fmt.Printf("%s: NumMethod=%d [%s]\n", label, t.NumMethod(), names(t))

	rv := reflect.ValueOf(v)
	m := rv.MethodByName("Bump")
	fmt.Printf("%s: MethodByName(Bump) valid=%v\n", label, m.IsValid())

	if m.IsValid() {
		out := m.Call(nil)
		fmt.Printf("%s: Bump via MethodByName = %d\n", label, out[0].Int())
	}

	for i := 0; i < t.NumMethod(); i++ {
		if t.Method(i).Name == "Bump" {
			out := rv.Method(i).Call(nil)
			fmt.Printf("%s: Bump via Method(%d).Call = %d\n", label, i, out[0].Int())
		}
	}

	b, ok := v.(bumper)
	fmt.Printf("%s: assert bumper ok=%v\n", label, ok)

	if ok {
		fmt.Printf("%s: Bump via interface = %d\n", label, b.Bump())
	}
}

func main() {
	p := SameP{&Inner{10}}
	describe("SameP", p)
	describe("*SameP", &p)
	fmt.Println("SameP inner after:", p.n)

	v := SameV{Inner{20}}
	describe("SameV", v)
	describe("*SameV", &v)
	fmt.Println("SameV inner after:", v.n)

	vp := OuterVP{MidP{&Inner{30}}}
	describe("OuterVP", vp)
	describe("*OuterVP", &vp)
	fmt.Println("OuterVP inner after:", vp.n)

	pv := OuterPV{&MidV{Inner{40}}}
	describe("OuterPV", pv)
	describe("*OuterPV", &pv)
	fmt.Println("OuterPV inner after:", pv.n)

	vv := OuterVV{MidV{Inner{50}}}
	describe("OuterVV", vv)
	describe("*OuterVV", &vv)
	fmt.Println("OuterVV inner after:", vv.n)

	// The standard library's own shape: bufio.ReadWriter embeds *Reader and *Writer, so the
	// VALUE carries ReadString and satisfies io.Reader.
	rw := bufio.ReadWriter{Reader: bufio.NewReader(strings.NewReader("first\nsecond\n"))}
	rt := reflect.TypeOf(rw)
	_, hasReadString := rt.MethodByName("ReadString")
	fmt.Println("bufio.ReadWriter value has ReadString:", hasReadString)

	out := reflect.ValueOf(rw).MethodByName("ReadString").Call([]reflect.Value{reflect.ValueOf(byte('\n'))})
	fmt.Printf("ReadString via reflect = %q\n", out[0].String())

	var x any = rw
	r, ok := x.(io.Reader)
	fmt.Println("bufio.ReadWriter value asserts io.Reader:", ok)

	if ok {
		buf := make([]byte, 6)
		n, _ := r.Read(buf)
		fmt.Printf("Read via io.Reader = %q\n", string(buf[:n]))
	}
}
