// The reflect `Value` singles (increment E3 of the reflect tail): one root per commit, each row
// pinned against `go run` beside the reflect suite's own verdict.
//
//  1. SetCap -- the fifth raw-slice-header member, hand-owned beside SetLen: Go's bound check
//     (len <= n <= cap) and its panic text, and the three-index window s[:len:n] written back
//     through the addressable slot (TestSetLenCap).
//  2. Bytes -- the Array arm decided by element KIND, in Go's order: non-byte element, then
//     addressability, then an ALIAS of the array's own backing (TestBytes).
//  3. Name of an instantiated generic type keeps its type arguments: the package qualifier ends
//     before the first '[', not at the last '.' of the whole spelling (TestIssue50208).
//  4. unsafe.Pointer identity across the two address models: a Pointer reflect mints (the identity
//     token) and one the converter mints from the same box (its address) are the SAME pointer
//     (TestImplicitMapConversion #5/#7).
//  5. Convert's pointer family: (*[N]T)(s) ALIASES the slice's backing; (*B)(p) between pointers whose
//     pointees have one representation aliases the same storage; nil converts to the destination's
//     typed nil, dims and all (TestConvert).
package main

import (
	"bytes"
	"fmt"
	"io"
	"reflect"
	"strings"
	"unsafe"
)

// expectPanic runs f and reports whether it panicked and whether the panic text mentions want --
// TestSetLenCap's shouldPanic, printed rather than asserted.
func expectPanic(label, want string, f func()) {
	defer func() {
		r := recover()
		msg := fmt.Sprint(r)
		fmt.Printf("%-16s panicked: %v  mentions %q: %v  text: %s\n", label, r != nil, want, strings.Contains(msg, want), msg)
	}()
	f()
}

type gA struct{}
type gB[T any] struct{}

type integer int
type MyBytes []byte
type MyBytesArray0 [0]byte
type MyBytesArray [4]byte
type MyBytesArrayPtr0 *[0]byte
type MyBytesArrayPtr *[4]byte
type MyBuffer bytes.Buffer

// convRow converts x to want's type and reports kind, DeepEqual against want, and nil-ness -- TestConvert's
// own comparison (type identity, then DeepEqual of the interfaces) restated per row.
func convRow(label string, x, want any) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%-34s PANIC: %v\n", label, r)
		}
	}()
	v := reflect.ValueOf(x).Convert(reflect.TypeOf(want))
	fmt.Printf("%-34s type==%v deepEqual=%v nil=%v\n", label, v.Type() == reflect.TypeOf(want), reflect.DeepEqual(v.Interface(), want), v.Kind() == reflect.Ptr && v.IsNil())
}

type IntChan chan int
type IntChanRecv <-chan int
type IntChanSend chan<- int

func main() {
	// --- root 1: SetLen / SetCap ---
	xs := []int{1, 2, 3, 4, 5, 6, 7, 8}
	xa := [8]int{10, 20, 30, 40, 50, 60, 70, 80}
	vs := reflect.ValueOf(&xs).Elem()
	expectPanic("SetLen(10)", "SetLen", func() { vs.SetLen(10) })
	expectPanic("SetCap(10)", "SetCap", func() { vs.SetCap(10) })
	expectPanic("SetLen(-1)", "SetLen", func() { vs.SetLen(-1) })
	expectPanic("SetCap(-1)", "SetCap", func() { vs.SetCap(-1) })
	expectPanic("SetCap(6)<len", "SetCap", func() { vs.SetCap(6) }) // smaller than len
	vs.SetLen(5)
	fmt.Println("after SetLen(5): len, cap =", len(xs), cap(xs))
	vs.SetCap(6)
	fmt.Println("after SetCap(6): len, cap =", len(xs), cap(xs))
	vs.SetCap(5)
	fmt.Println("after SetCap(5): len, cap =", len(xs), cap(xs), "contents", xs)
	expectPanic("SetCap(4)<len", "SetCap", func() { vs.SetCap(4) })
	expectPanic("SetLen(6)>cap", "SetLen", func() { vs.SetLen(6) })
	va := reflect.ValueOf(&xa).Elem()
	expectPanic("array SetLen", "SetLen", func() { va.SetLen(8) })
	expectPanic("array SetCap", "SetCap", func() { va.SetCap(8) })
	// the re-capped slice still aliases the original backing: a write through it lands in the array
	backing := xs[:cap(xs)]
	backing[0] = 99
	fmt.Println("write through the re-capped window seen by the original:", xs[0] == 99)
	// --- root 2: Bytes ---
	expectPanic("Bytes on int", "on int Value", func() { reflect.ValueOf(0).Bytes() })
	expectPanic("Bytes []string", "of non-byte slice", func() { reflect.ValueOf([]string{}).Bytes() })
	type S []byte
	x := S{1, 2, 3, 4}
	y := reflect.ValueOf(x).Bytes()
	y[0] = 42
	fmt.Println("S bytes:", y, "aliases x:", x[0] == 42)
	type A [4]byte
	a := A{1, 2, 3, 4}
	expectPanic("Bytes [4]byte value", "unaddressable", func() { reflect.ValueOf(a).Bytes() })
	expectPanic("Bytes *[4]byte", "on ptr Value", func() { reflect.ValueOf(&a).Bytes() })
	b := reflect.ValueOf(&a).Elem().Bytes()
	b[1] = 43
	fmt.Println("A bytes:", b, "aliases a:", a[1] == 43)
	// issue #24746: byte-KIND elements qualify even where the language conversion would not
	type B byte
	type SB []B
	type AB [4]B
	fmt.Println("[]B   bytes:", reflect.ValueOf([]B{1, 2, 3, 4}).Bytes())
	fmt.Println("*[4]B bytes:", reflect.ValueOf(new([4]B)).Elem().Bytes())
	fmt.Println("SB    bytes:", reflect.ValueOf(SB{1, 2, 3, 4}).Bytes())
	fmt.Println("*AB   bytes:", reflect.ValueOf(new(AB)).Elem().Bytes())
	ab := AB{5, 6, 7, 8}
	expectPanic("Bytes AB value", "unaddressable", func() { reflect.ValueOf(ab).Bytes() })
	c := reflect.ValueOf(&ab).Elem().Bytes()
	c[2] = 44
	fmt.Println("AB bytes:", c, "aliases ab:", ab[2] == 44)
	expectPanic("Bytes [4]int", "of non-byte array", func() { reflect.ValueOf(&[4]int{}).Elem().Bytes() })

	// --- root 3: Name of an instantiated generic ---
	fmt.Println("Name gB[gA]:", reflect.TypeOf(new(gB[gA])).Elem().Name())
	fmt.Println("Name gB[gB[gA]]:", reflect.TypeOf(new(gB[gB[gA]])).Elem().Name())
	fmt.Println("String gB[gA]:", reflect.TypeOf(gB[gA]{}).String())
	fmt.Println("Name plain gA:", reflect.TypeOf(gA{}).Name())

	// --- root 4: unsafe.Pointer identity across address models ---
	m5 := make(map[io.Reader]io.Writer)
	mv5 := reflect.ValueOf(m5)
	b1, b2 := new(bytes.Buffer), new(bytes.Buffer)
	mv5.SetMapIndex(reflect.ValueOf(b1), reflect.ValueOf(b2))
	x5, ok5 := m5[b1]
	fmt.Println("#5 entry is b2:", x5 == b2, ok5)
	p5 := mv5.MapIndex(reflect.ValueOf(b1)).Elem().UnsafePointer() // a LOCAL, as the test holds it: the comparison is then pointer identity
	fmt.Println("#5 MapIndex(b1).Elem().UnsafePointer() == unsafe.Pointer(b2):", p5 == unsafe.Pointer(b2))
	type MyBuffer bytes.Buffer
	m7 := make(map[*MyBuffer]*bytes.Buffer)
	mv7 := reflect.ValueOf(m7)
	k7, v7 := new(MyBuffer), new(bytes.Buffer)
	mv7.SetMapIndex(reflect.ValueOf(k7), reflect.ValueOf(v7))
	p7 := mv7.MapIndex(reflect.ValueOf(k7)).UnsafePointer()
	fmt.Println("#7 MapIndex(b1).UnsafePointer() == unsafe.Pointer(b2):", p7 == unsafe.Pointer(v7))
	// the same box minted twice by reflect, and by reflect vs the language, are one pointer
	n := new(int)
	pn1, pn2 := reflect.ValueOf(n).UnsafePointer(), reflect.ValueOf(n).UnsafePointer()
	fmt.Println("reflect twice:", pn1 == pn2, " reflect vs unsafe.Pointer(n):", pn1 == unsafe.Pointer(n))
	// different boxes stay different; an interior offset is not the base
	n2 := new(int)
	var arr [4]int64
	fmt.Println("different boxes:", pn1 == unsafe.Pointer(n2), " interior != base:", unsafe.Add(unsafe.Pointer(&arr), 8) != unsafe.Pointer(&arr), " base == base:", unsafe.Pointer(&arr) == unsafe.Pointer(&arr))

	// the Equals/GetHashCode contract, observed through a map keyed by unsafe.Pointer: a key minted by
	// reflect is found by the language's pointer to the same box, and a numeric key by its equal number
	keyed := map[unsafe.Pointer]int{reflect.ValueOf(n).UnsafePointer(): 1}
	_, foundBox := keyed[unsafe.Pointer(n)]
	interior := map[unsafe.Pointer]int{unsafe.Add(unsafe.Pointer(&arr), 8): 2}
	_, foundNumber := interior[unsafe.Add(unsafe.Pointer(&arr), 8)]
	_, missOther := keyed[unsafe.Pointer(n2)]
	fmt.Println("map: same box twice found:", foundBox, " same number twice found:", foundNumber, " other box found:", missOther)
	// a FIELD's address minted twice is one pointer: each `&h.p` is a fresh field-ref box over the same
	// storage, and identity is the referent's order token, not the box object (the ManagedAtomicPointer
	// guard's row 8, caught in the full suite during the referent cut)
	type holder struct{ p *int }
	var h holder
	fp := unsafe.Pointer(&h.p)
	fieldKeyed := map[unsafe.Pointer]int{fp: 3}
	_, foundField := fieldKeyed[unsafe.Pointer(&h.p)]
	fmt.Println("same field twice:", fp == unsafe.Pointer(&h.p), " field vs other box:", fp == unsafe.Pointer(n), " map: same field twice found:", foundField)

	// --- root 5: Convert's pointer family ---
	convRow("[]byte(nil) -> *[0]byte", []byte(nil), (*[0]byte)(nil))
	convRow("[]byte{} -> *[0]byte", []byte{}, new([0]byte))
	convRow("[]byte{7} -> *[1]byte", []byte{7}, &[1]byte{7})
	convRow("MyBytes{9} -> *[1]byte", MyBytes([]byte{9}), &[1]byte{9})
	convRow("[]byte{1,2,3,4} -> MyBytesArrayPtr", []byte{1, 2, 3, 4}, MyBytesArrayPtr(&[4]byte{1, 2, 3, 4}))
	convRow("[]byte(nil) -> MyBytesArrayPtr0", []byte(nil), MyBytesArrayPtr0(nil))
	convRow("[]byte{1,2,3,4} -> *MyBytesArray", []byte{1, 2, 3, 4}, &MyBytesArray{1, 2, 3, 4})
	convRow("[]byte(nil) -> *MyBytesArray0", []byte(nil), (*MyBytesArray0)(nil))
	convRow("new([0]byte) -> *MyBytesArray0", new([0]byte), new(MyBytesArray0))
	convRow("new(MyBytesArray0) -> *[0]byte", new(MyBytesArray0), new([0]byte))
	convRow("MyBytesArrayPtr0(nil) -> *[0]byte", MyBytesArrayPtr0(nil), (*[0]byte)(nil))
	convRow("(*[0]byte)(nil) -> MyBytesArrayPtr0", (*[0]byte)(nil), MyBytesArrayPtr0(nil))
	convRow("new(int) -> *integer", new(int), new(integer))
	convRow("new(integer) -> *int", new(integer), new(int))
	convRow("*MyBuffer -> *bytes.Buffer", new(MyBuffer), new(bytes.Buffer))
	// the converted array pointer ALIASES the slice: a write through it lands in the slice
	src := []byte{1, 2, 3, 4}
	ap := reflect.ValueOf(src).Convert(reflect.TypeOf((*[4]byte)(nil))).Interface().(*[4]byte)
	ap[2] = 99
	fmt.Println("array pointer aliases the slice:", src[2] == 99)
	sh := reflect.ValueOf([]byte{1, 2, 3, 4})
	expectPanic("Convert short slice", "cannot convert slice with length 4 to pointer to array with length 8", func() { sh.Convert(reflect.TypeOf((*[8]byte)(nil))) })

	// --- root 5 follow-up 7e: Convert between channel types carries the destination's direction ---
	convRow("IntChan(nil) -> chan<- int", IntChan(nil), (chan<- int)(nil))
	convRow("IntChan(nil) -> <-chan int", IntChan(nil), (<-chan int)(nil))
	convRow("chan int(nil) -> IntChanRecv", (chan int)(nil), IntChanRecv(nil))
	convRow("chan int(nil) -> IntChanSend", (chan int)(nil), IntChanSend(nil))
	convRow("IntChanRecv(nil) -> <-chan int", IntChanRecv(nil), (<-chan int)(nil))
	convRow("<-chan int(nil) -> IntChanRecv", (<-chan int)(nil), IntChanRecv(nil))
	convRow("IntChanSend(nil) -> chan<- int", IntChanSend(nil), (chan<- int)(nil))
	convRow("chan<- int(nil) -> IntChanSend", (chan<- int)(nil), IntChanSend(nil))
	convRow("IntChan(nil) -> chan int", IntChan(nil), (chan int)(nil))
	// a LIVE channel converted to a directional type is the same channel: a send through the
	// converted send-only view is received on the original
	live := make(IntChan, 1)
	sendOnly := reflect.ValueOf(live).Convert(reflect.TypeOf((chan<- int)(nil)))
	sendOnly.Interface().(chan<- int) <- 7
	fmt.Println("converted send-only view type:", sendOnly.Type(), " interface type:", reflect.TypeOf(sendOnly.Interface()), " received on the original:", <-live)

	// --- 7f: Set honours the slot's channel direction -- a directional value into a slot of the SAME
	// direction is identity, a bidirectional value narrows and takes the slot's type, and a directional
	// value into a bidirectional slot or the opposite direction is refused (TestConvert's Set rows) ---
	setRow := func(label string, slot reflect.Type, x reflect.Value) {
		defer func() {
			if r := recover(); r != nil {
				fmt.Printf("%-38s PANIC: %v\n", label, r)
			}
		}()
		v := reflect.New(slot).Elem()
		v.Set(x)
		fmt.Printf("%-38s slot=%v value=%v nil=%v\n", label, v.Type(), reflect.TypeOf(v.Interface()), v.IsNil())
	}
	sendT, recvT, bidiT := reflect.TypeOf((chan<- int)(nil)), reflect.TypeOf((<-chan int)(nil)), reflect.TypeOf((chan int)(nil))
	setRow("chan<- int <- IntChan(nil).Convert", sendT, reflect.ValueOf(IntChan(nil)).Convert(sendT))
	setRow("<-chan int <- chan int(nil).Convert", recvT, reflect.ValueOf((chan int)(nil)).Convert(recvT))
	setRow("chan int <- chan<- int(nil)", bidiT, reflect.ValueOf((chan<- int)(nil)))
	setRow("<-chan int <- chan<- int(nil)", recvT, reflect.ValueOf((chan<- int)(nil)))
	setRow("chan<- int <- chan int(nil)", sendT, reflect.ValueOf((chan int)(nil)))
	setRow("chan<- int <- live IntChan.Convert", sendT, reflect.ValueOf(live).Convert(sendT))
	// a live bidirectional channel Set into a send-only slot is the same channel: a send through the
	// slot's value is received on the original
	slot := reflect.New(sendT).Elem()
	slot.Set(reflect.ValueOf(live))
	slot.Interface().(chan<- int) <- 8
	fmt.Println("send-only slot value type:", reflect.TypeOf(slot.Interface()), " received on the original:", <-live)

	// --- 7e-b: a DEFINED channel type's descriptor carries the direction its marker cannot spell (the wrapper's
	// [GoChanDir] stamp, read by synthType ahead of its interning key): ChanDir answers, ConvertibleTo/AssignableTo
	// answer Go's matrix, and the same type minted through the VALUE route, a SLOT route and Elem() is ONE
	// descriptor -- the stamp decides on every route (COORD's condition at 27c307e3d) ---
	fmt.Println("IntChanRecv dir:", reflect.TypeOf(IntChanRecv(nil)).ChanDir(), " IntChanSend dir:", reflect.TypeOf(IntChanSend(nil)).ChanDir(), " IntChan dir:", reflect.TypeOf(IntChan(nil)).ChanDir())
	canRow := func(x, y any) {
		fmt.Printf("%-14v -> %-14v convertible=%v assignable=%v\n", reflect.TypeOf(x), reflect.TypeOf(y), reflect.TypeOf(x).ConvertibleTo(reflect.TypeOf(y)), reflect.TypeOf(x).AssignableTo(reflect.TypeOf(y)))
	}
	canRow((<-chan int)(nil), IntChanRecv(nil))
	canRow((chan<- int)(nil), IntChanSend(nil))
	canRow(IntChan(nil), IntChanRecv(nil))
	canRow(IntChanRecv(nil), IntChan(nil))
	canRow(IntChanRecv(nil), (chan<- int)(nil))
	canRow(IntChanRecv(nil), (chan int)(nil))
	canRow(IntChanSend(nil), (<-chan int)(nil))
	canRow(IntChanRecv(nil), IntChanSend(nil))
	type chanSlots struct {
		R IntChanRecv
		S IntChanSend
	}
	cst := reflect.TypeOf(chanSlots{})
	fmt.Println("IntChanRecv identity value/slot/elem:", reflect.TypeOf(IntChanRecv(nil)) == cst.Field(0).Type, reflect.TypeOf(IntChanRecv(nil)) == reflect.TypeOf((*IntChanRecv)(nil)).Elem(), cst.Field(0).Type.ChanDir())
	fmt.Println("IntChanSend identity value/slot/elem:", reflect.TypeOf(IntChanSend(nil)) == cst.Field(1).Type, reflect.TypeOf(IntChanSend(nil)) == reflect.TypeOf((*IntChanSend)(nil)).Elem(), cst.Field(1).Type.ChanDir())
	fmt.Println("chan slot zero re-describes:", reflect.TypeOf(reflect.Zero(cst.Field(0).Type).Interface()), reflect.Zero(cst.Field(0).Type).Type().ChanDir())

	// --- 7g: a DEFINED pointer-to-array type's descriptor carries the array LENGTH its marker cannot spell (the
	// wrapper's [GoArrayDims] stamp): a nil and a live value are ONE descriptor, Elem().Len() answers, and the
	// value route, a slot route and Elem() agree -- the stamp decides ---
	fmt.Println("named ptr-to-array one descriptor:", reflect.TypeOf(MyBytesArrayPtr0(nil)) == reflect.TypeOf(MyBytesArrayPtr0(new([0]byte))), reflect.TypeOf(MyBytesArrayPtr(nil)) == reflect.TypeOf(MyBytesArrayPtr(new([4]byte))), " elem:", reflect.TypeOf(MyBytesArrayPtr(nil)).Elem(), reflect.TypeOf(MyBytesArrayPtr0(nil)).Elem().Len())
	canRow(MyBytes(nil), MyBytesArrayPtr0(nil))
	canRow(MyBytesArrayPtr0(nil), (*[0]byte)(nil))
	canRow(MyBytesArrayPtr(nil), (*[4]byte)(nil))
	canRow(MyBytesArrayPtr(nil), MyBytesArrayPtr0(nil))
	type ptrSlots struct {
		P MyBytesArrayPtr
		Z MyBytesArrayPtr0
	}
	pst := reflect.TypeOf(ptrSlots{})
	fmt.Println("MyBytesArrayPtr identity value/slot/elem:", reflect.TypeOf(MyBytesArrayPtr(new([4]byte))) == pst.Field(0).Type, reflect.TypeOf(MyBytesArrayPtr(nil)) == reflect.TypeOf((*MyBytesArrayPtr)(nil)).Elem(), pst.Field(0).Type.Elem().Len())
	fmt.Println("MyBytesArrayPtr0 identity value/slot/elem:", reflect.TypeOf(MyBytesArrayPtr0(new([0]byte))) == pst.Field(1).Type, reflect.TypeOf(MyBytesArrayPtr0(nil)) == reflect.TypeOf((*MyBytesArrayPtr0)(nil)).Elem(), pst.Field(1).Type.Elem().Len())
	fmt.Println("ptr slot New re-describes:", reflect.TypeOf(reflect.New(pst.Field(1).Type).Elem().Interface()), reflect.New(pst.Field(0).Type).Elem().Type().Elem().Len())

}
