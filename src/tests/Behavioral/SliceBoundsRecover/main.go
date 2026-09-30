// SliceBoundsRecover: every slice-bound failure shape of S-c R1-A (docs/phase4/DESIGN-slice-bounds-r1a.md), in ONE
// process. Each case must recover Go's panic, print Go's text, and report that the value is a runtime.Error. A panic
// that escapes recover() ends the process, so an escape shows as every later case missing from the output.
//
// The 2-index shapes (probe R1aShapes) cover slice, named slice, array, pointer-to-array, named array, string, named
// string and string literal, with int, int64, uint32 and uint64 bounds. The 3-index cases (probe R1aSentinel) pass
// a RUNTIME -1, which golib's optional-parameter .slice(low, high, max) read as "bound omitted".
//
// Not here: a uint64 bound at or above 2^63. R1-A casts a wide unsigned bound to nint (ruling U1), so that panic
// prints the bound as a negative number; the standing slice-bounds probe keeps that residual shape.
package main

import "fmt"

type named []int

type namedStr string

type namedArr [3]int

var sink any

func try(name string, f func()) {
	defer func() {
		r := recover()
		_, isErr := r.(interface{ RuntimeError() })
		fmt.Printf("%-24s %v | runtime.Error=%v\n", name, r, isErr)
	}()
	f()
}

func main() {
	s := make([]int, 3, 10)
	var a [3]int
	p := &a
	n := named(s)
	var na namedArr
	str := "abc"
	ns := namedStr("abc")
	neg, big, lo4, hi2, hi5, hi11 := -1, 1<<32+5, 4, 2, 5, 11
	var u64 uint64 = 1<<40 + 3
	var u32 uint32 = 11
	var i64 int64 = 1<<32 + 5

	var a6 [6]int
	two, five := 2, 5

	cases := []struct {
		name string
		f    func()
	}{
		{"slice [:hi>cap]", func() { sink = s[:hi11] }},
		{"slice [:len<hi<=cap]", func() { sink = len(s[:hi5]) }},
		{"slice [:-1]", func() { sink = s[:neg] }},
		{"slice [-1:]", func() { sink = s[neg:] }},
		{"slice [-1:2]", func() { sink = s[neg:hi2] }},
		{"slice [4:2]", func() { sink = s[lo4:hi2] }},
		{"slice [4:]", func() { sink = s[lo4:] }},
		{"slice [:big]", func() { sink = s[:big] }},
		{"slice [big:]", func() { sink = s[big:] }},
		{"slice [:i64]", func() { sink = s[:i64] }},
		{"slice [:u64]", func() { sink = s[:u64] }},
		{"slice [:u32]", func() { sink = s[:u32] }},
		{"named [:-1]", func() { sink = n[:neg] }},
		{"named [4:2]", func() { sink = n[lo4:hi2] }},
		{"array [:5]", func() { sink = a[:hi5] }},
		{"array [:-1]", func() { sink = a[:neg] }},
		{"array [4:2]", func() { sink = a[lo4:hi2] }},
		{"array [4:]", func() { sink = a[lo4:] }},
		{"ptrarray [:5]", func() { sink = p[:hi5] }},
		{"namedarr [:5]", func() { sink = na[:hi5] }},
		{"string [:5]", func() { sink = str[:hi5] }},
		{"string [:-1]", func() { sink = str[:neg] }},
		{"string [-1:]", func() { sink = str[neg:] }},
		{"string [4:2]", func() { sink = str[lo4:hi2] }},
		{"string [4:]", func() { sink = str[lo4:] }},
		{"string [:big]", func() { sink = str[:big] }},
		{"namedstr [:5]", func() { sink = ns[:hi5] }},
		{"strlit [:5]", func() { var t string = "abc"[:hi5]; sink = t }},
		{"strlit [-1:]", func() { var t string = "abc"[neg:]; sink = t }},
		{"slice3 [-1:2:5]", func() { t := s[neg:two:five]; sink = t; fmt.Println("len", len(t), "cap", cap(t)) }},
		{"slice3 [0:-1:5]", func() { t := s[0:neg:five]; sink = t; fmt.Println("len", len(t), "cap", cap(t)) }},
		{"slice3 [0:2:-1]", func() { t := s[0:two:neg]; sink = t; fmt.Println("len", len(t), "cap", cap(t)) }},
		{"array3 [-1:2:5]", func() { t := a6[neg:two:five]; sink = t; fmt.Println("len", len(t), "cap", cap(t)) }},
		{"array3 [0:2:-1]", func() { t := a6[0:two:neg]; sink = t; fmt.Println("len", len(t), "cap", cap(t)) }},
		{"slice3 [:2:5] in range", func() { t := s[:two:five]; sink = t; fmt.Println("len", len(t), "cap", cap(t)) }},
	}

	for _, c := range cases {
		try(c.name, c.f)
	}

	fmt.Println("done")
}
