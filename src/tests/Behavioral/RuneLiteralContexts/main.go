// Rune literals in every slot the converter treats differently: the slots where C# takes a bare
// char ('x'), the byte-sized slots that keep one cast spelled as the target ((byte)'x'), and the
// slots that keep (rune)'x' because C# would otherwise infer or box a char.
package main

import "fmt"

const constLit = 'a'
const constDiff = 'a' - 'A'

type letter rune

func isByte(b byte) bool { return b == 'x' }

func takesRune(r rune) rune { return r + 1 }

func retRune() rune { return 'Z' }

func retByte() byte { return 'z' }

func main() {
	// Boxed to an interface: Go prints the int32 value and type.
	fmt.Println('a')
	fmt.Printf("%T %v\n", 'a', 'a')
	var anyv any = 'w'
	fmt.Printf("%T %v\n", anyv, anyv)

	// Inferred locals stay int32.
	r := 'a'
	fmt.Printf("%T %v\n", r, r)
	var typed rune = 'b'
	fmt.Println(typed)

	// A const initializer and a constant-only expression keep their value and type.
	fmt.Printf("%T %v %T %v\n", constLit, constLit, constDiff, constDiff)
	x := constDiff
	fmt.Printf("%T %v\n", x, x)

	// Byte, int8 and int16 slots.
	var b byte = 'q'
	switch b {
	case 'p', 'q':
		fmt.Println("byte case")
	}
	fmt.Println(isByte('x'), retByte())
	bs := []byte{'h', 'i'}
	bs[0] = 'H'
	bs = append(bs, '!')
	fmt.Println(string(bs))
	var i8 int8 = 'A'
	var i16 int16 = 'B'
	fmt.Println(i8, i16)

	// A rune beyond the BMP.
	astral := '\U0001F600'
	fmt.Printf("%T %v %c\n", astral, astral, astral)

	// Builtins that infer their type from the arguments.
	fmt.Println(max('a', r), min(r, 'b'))

	// Rune parameter, return, comparison, case label and arithmetic.
	fmt.Println(takesRune('y'), retRune())
	if r == 'a' && r < 'z' {
		fmt.Println("rune compare")
	}
	switch r {
	case 'a', 'b':
		fmt.Println("rune case")
	}
	digit := byte('7')
	fmt.Println(digit-'0', int(digit-'0'))

	// Composite literals: elements, values and keys.
	rs := []rune{'x', 'y'}
	rs[1] = 'Y'
	m := map[string]rune{"a": 'A'}
	keys := [128]bool{'a': true, 'e': true}
	fmt.Println(string(rs), m["a"], keys['a'], keys['b'])

	// Conversions and wider integer and float targets.
	fmt.Println(byte('a'), rune('b'), uint16('c'), float64('d'))
	var u32 uint32 = 'u'
	var u64 uint64 = 'v'
	fmt.Println(u32-'a', u64*'b', float32('e')/2)

	// A named rune type keeps its conversion.
	var l letter = 'L'
	fmt.Printf("%T %v\n", l, l)

	// A send.
	ch := make(chan rune, 1)
	ch <- 's'
	fmt.Println(<-ch)

	// Natural-type positions: the literal decides the type C# infers, so it keeps a cast. A func
	// literal's return types its lambda; an inferred local and a generic argument type themselves.
	shrink := func(r rune) rune { return 'a' }
	pair := func() (rune, bool) { return 'b', true }
	wide := func() uint16 { return 'u' }
	widest := func() int { return 'w' }
	small := func() byte { return 'z' }
	fmt.Printf("%T %v\n", shrink, shrink(1))
	pr, pok := pair()
	fmt.Printf("%T %v %v\n", pair, pr, pok)
	fmt.Printf("%T %v %T %v %T %v\n", wide, wide(), widest, widest(), small, small())
	var inferred = 'v'
	fmt.Printf("%T %v %T %v\n", inferred, inferred, ident('i'), ident('i'))
	got := apply(func() rune { return 'g' })
	fmt.Printf("%T %v\n", got, got)
	var tx, ty uint16
	tx, ty = 'x', 'y'
	fmt.Printf("%T %v %v\n", tx, tx, ty)

	// A tuple assignment, a select send and a goroutine's argument.
	tm := map[string]rune{}
	tm["a"], tm["b"] = 'a', 'b'
	tw := []uint16{0, 0}
	tw[0], tw[1] = 'x', 'y'
	sel := make(chan rune, 1)
	select {
	case sel <- 's':
	default:
	}
	done := make(chan rune)
	go func(r rune) { done <- r }('g')
	fmt.Println(tm["a"], tm["b"], tw, <-sel, <-done)

	// A tagless switch's case comparison, which C# writes as a pattern, over byte, int8 and rune operands.
	for _, c := range []byte("a_7.") {
		var i8 int8 = int8(c)
		switch {
		case c == '_':
			fmt.Println("underscore")
		case '0' <= c && c <= '9' && i8 != '0':
			fmt.Println("digit", c-'0')
		case i8 == '.' || (c > 'z'):
			fmt.Println("dot")
		case rune(c) == 'a':
			fmt.Println("letter a")
		}
		switch d := c; true {
		case d == '.':
			fmt.Println("true-tag dot")
		}
	}
}

func ident[T any](v T) T { return v }

func apply[T any](f func() T) T { return f() }
