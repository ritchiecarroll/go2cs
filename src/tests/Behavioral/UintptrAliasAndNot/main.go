package main

import "fmt"

// zeebo/xxh3 declares `type ui = uintptr` and aligns with `ui(l) &^ 15` (hash64.go:90). An alias names the SAME type
// as uintptr, so every form here must convert exactly as its uintptr spelling does.
type ui = uintptr

type ni = int

type nu = uint

type u64 = uint64

// A NAMED type over a native or 64-bit unsigned type takes the same bare-literal complement in its own width.
type np uintptr

type nw uint

type n64 uint64

const shift = 4

func top(l int) ui { return ui(l) &^ 15 }

func topNamed(l int) np { return np(l) &^ 15 }

func main() {
	fmt.Println(top(37), top(16), top(0))

	var a ui = 1000
	a &^= 7
	fmt.Println(a)

	b := ui(0xFFFF)
	fmt.Println(b&^(1<<shift-1), b&(1<<shift-1), b|1<<shift)

	var n ni = -37
	fmt.Println(n&^15, n&^-16)

	var u nu = 0xABCD
	fmt.Println(u&^0xFF, u&^(1<<shift-1))

	var w u64 = ^u64(0)
	fmt.Println(w&^1, w&^(1<<63))
	w &^= 0xF0
	fmt.Println(w)

	fmt.Println(topNamed(37), topNamed(16))

	x := nw(0xABCD)
	fmt.Println(x&^0xFF, x&^1)

	y := ^n64(0)
	fmt.Println(y&^1, y&^0xFFFF)
}
