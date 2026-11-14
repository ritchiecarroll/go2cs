// FuncWordShapes: a func value carried as a number, in the six structural shapes ccgo writes (sqlite-cs S1).
// m1/m2 read a func as a word, w2 writes a func into a word slot, r1/r2 read a word as a func, w1 writes a word
// into a func variable. Nothing prints a raw word: only equalities, nil tests and call results.
package main

import (
	"fmt"
	"unsafe"
)

type binop func(int, int) int

type holder struct{ f func(int, int) int }

func add(a, b int) int { return a + b }

func mul(a, b int) int { return a * b }

// m1: ccgo's __ccgo_fp, the data word of an interface parameter that holds a func.
func ccgoFP(f interface{}) uintptr {
	return (*[2]uintptr)(unsafe.Pointer(&f))[1]
}

// m1, as ccgo spells it: the array type declared first.
func ccgoFPLocal(f interface{}) uintptr {
	type iface [2]uintptr
	return (*iface)(unsafe.Pointer(&f))[1]
}

// m2: a func variable read as a word.
func wordOf(f func(int, int) int) uintptr {
	return *(*uintptr)(unsafe.Pointer(&f))
}

// m2: a one-func-field struct read as a word.
func wordOfHolder(h holder) uintptr {
	return *(*uintptr)(unsafe.Pointer(&h))
}

// r1: a word read as a func and called at once.
func callR1(x uintptr, a, b int) int {
	return (*(*func(int, int) int)(unsafe.Pointer(&struct{ uintptr }{x})))(a, b)
}

// r1, as a named func type: the word was minted from an unnamed one.
func callR1Named(x uintptr, a, b int) int {
	return (*(*binop)(unsafe.Pointer(&struct{ uintptr }{x})))(a, b)
}

// r2: a word read through a one-func-field struct.
func callR2(x uintptr, a, b int) int {
	return (*struct{ f func(int, int) int })(unsafe.Pointer(&struct{ uintptr }{x})).f(a, b)
}

// r1, stored rather than called at once.
func recoverR1(x uintptr) func(int, int) int {
	return *(*func(int, int) int)(unsafe.Pointer(&struct{ uintptr }{x}))
}

func main() {
	wAdd := ccgoFP(add)
	wMul := ccgoFP(mul)
	fmt.Println("m1 add nonzero:", wAdd != 0)
	fmt.Println("m1 add twice equal:", wAdd == ccgoFP(add))
	fmt.Println("m1 add != mul:", wAdd != wMul)
	fmt.Println("m1 add == m2 add:", wAdd == wordOf(add))
	fmt.Println("m1 add == m2 holder add:", wAdd == wordOfHolder(holder{add}))
	fmt.Println("m1 add as binop == m1 add:", ccgoFP(binop(add)) == wAdd)
	fmt.Println("m1 ccgo spelling == m1 add:", ccgoFPLocal(add) == wAdd)

	fmt.Println("r1 add(2,3):", callR1(wAdd, 2, 3))
	fmt.Println("r1 mul(2,3):", callR1(wMul, 2, 3))
	fmt.Println("r1 named add(4,5):", callR1Named(wAdd, 4, 5))
	fmt.Println("r2 mul(4,5):", callR2(wMul, 4, 5))
	stored := recoverR1(wMul)
	fmt.Println("r1 stored mul(6,7):", stored(6, 7))

	// w2: a func written into a word slot, then read back as a word and as a func.
	var slot uintptr
	*(*func(int, int) int)(unsafe.Pointer(&slot)) = add
	fmt.Println("w2 slot == m1 add:", slot == wAdd)
	fmt.Println("w2 slot r1(7,8):", callR1(slot, 7, 8))

	// w1: a word written into a func variable.
	var g func(int, int) int
	*(*uintptr)(unsafe.Pointer(&g)) = wMul
	fmt.Println("w1 g(8,9):", g(8, 9))
	var z func(int, int) int = add
	*(*uintptr)(unsafe.Pointer(&z)) = 0
	fmt.Println("w1 zero is nil:", z == nil)

	// nil: a nil func's word is 0.
	var nf func(int, int) int
	fmt.Println("m2 nil word is 0:", wordOf(nf) == 0)

	// closures: one word per closure value, a word per value, and recovery calls it.
	base := 10
	c1 := func(a, b int) int { return a + b + base }
	c2 := func(a, b int) int { return a + b + base }
	w1c := wordOf(c1)
	fmt.Println("closure same value twice equal:", w1c == wordOf(c1))
	fmt.Println("closure two values differ:", w1c != wordOf(c2))
	fmt.Println("closure != static:", w1c != wAdd)
	base = 20
	fmt.Println("closure r1(1,2):", callR1(w1c, 1, 2))

	// a method value: a word per value, and recovery calls it with its receiver.
	acc := &counter{n: 100}
	mv := acc.plus
	wmv := wordOf(mv)
	fmt.Println("method value r1(1,2):", callR1(wmv, 1, 2))
}

type counter struct{ n int }

func (c *counter) plus(a, b int) int { return c.n + a + b }
