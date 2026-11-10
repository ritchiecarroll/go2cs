package main

import "fmt"

// A Go switch compares its tag with each case value in order, top to bottom and left to right, and a case value need
// not be a constant: arrays and structs are comparable, and any expression of the tag's type is a case. tsc's
// internal/vfs/internal decodeBytes switches a [2]byte byte-order mark over two array literals (TS readiness C30).

func bomKind(s string) string {
	var bom [2]byte
	if len(s) >= 2 {
		bom = [2]byte{s[0], s[1]}
		switch bom {
		case [2]byte{0xFF, 0xFE}:
			return "le"
		case [2]byte{0xFE, 0xFF}:
			return "be"
		}
	}
	return "none"
}

func pair(b [2]byte) string {
	switch b {
	case [2]byte{1, 2}, [2]byte{3, 4}:
		return "low"
	case [2]byte{9, 9}:
		return "nines"
	default:
		return "other"
	}
}

type point struct{ X, Y int }

func where(p point) string {
	switch p {
	case point{0, 0}:
		return "origin"
	case point{1, 0}, point{0, 1}:
		return "unit"
	}
	return "elsewhere"
}

func sum(n, a, b int) string {
	switch n {
	case a + b:
		return "sum"
	case a * b:
		return "product"
	}
	return "neither"
}

func through(b [2]byte) []string {
	var out []string
	switch b {
	case [2]byte{1, 1}:
		out = append(out, "one")
		fallthrough
	case [2]byte{2, 2}:
		out = append(out, "two")
	case [2]byte{3, 3}:
		out = append(out, "three")
	}
	return out
}

var trace []int

func at(i int, v [2]byte) [2]byte { trace = append(trace, i); return v }

func order(b [2]byte) string {
	trace = nil
	switch b {
	case at(1, [2]byte{7, 7}), at(2, [2]byte{8, 8}):
		return fmt.Sprint("first ", trace)
	case at(3, [2]byte{9, 9}):
		return fmt.Sprint("second ", trace)
	}
	return fmt.Sprint("none ", trace)
}

// Control: constant cases stay a C# switch with constant labels.
func digit(n int) string {
	switch n {
	case 1:
		return "one"
	case 2, 3:
		return "few"
	}
	return "many"
}

func main() {
	fmt.Println(bomKind("\xff\xfeab"), bomKind("\xfe\xffab"), bomKind("ab"), bomKind("a"))
	fmt.Println(pair([2]byte{1, 2}), pair([2]byte{3, 4}), pair([2]byte{9, 9}), pair([2]byte{5, 5}))
	fmt.Println(where(point{}), where(point{0, 1}), where(point{2, 2}))
	fmt.Println(sum(5, 2, 3), sum(6, 2, 3), sum(7, 2, 3))
	fmt.Println(through([2]byte{1, 1}), through([2]byte{2, 2}), through([2]byte{3, 3}), len(through([2]byte{})))
	fmt.Println(order([2]byte{8, 8}), order([2]byte{9, 9}), order([2]byte{0, 0}))
	fmt.Println(digit(1), digit(3), digit(9))
}
