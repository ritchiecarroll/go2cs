package main

import "fmt"

type Row [4]int

type Digits []int

type holder struct {
	arr [4]int
}

// arrayValue: `range` over an ARRAY VALUE evaluates the range expression ONCE, so the loop
// iterates a COPY — writes to the container inside the body are invisible to later iterations.
func arrayValue() {
	a := [4]int{1, 2, 3, 4}

	for i, v := range a {
		if i == 0 {
			a[1], a[2], a[3] = 91, 92, 93
		}

		fmt.Println("arrayValue", i, v)
	}

	fmt.Println("arrayValue after:", a)
}

// namedArrayValue: same rule through a NAMED array type (the generated wrapper).
func namedArrayValue() {
	r := Row{1, 2, 3, 4}

	for i, v := range r {
		if i == 0 {
			r[1], r[2], r[3] = 91, 92, 93
		}

		fmt.Println("namedArrayValue", i, v)
	}

	fmt.Println("namedArrayValue after:", r)
}

// arrayField: the range expression is a struct FIELD of array type — still a value, still copied.
func arrayField() {
	h := holder{arr: [4]int{1, 2, 3, 4}}

	for i, v := range h.arr {
		if i == 0 {
			h.arr[1], h.arr[2], h.arr[3] = 91, 92, 93
		}

		fmt.Println("arrayField", i, v)
	}

	fmt.Println("arrayField after:", h.arr)
}

// arrayOfArrays: the copy is of the OUTER array; the element rows are values inside it, so a
// write to `m[1][0]` after the loop starts is likewise invisible.
func arrayOfArrays() {
	m := [3][2]int{{1, 2}, {3, 4}, {5, 6}}

	for i, row := range m {
		if i == 0 {
			m[1][0], m[2][0] = 91, 92
		}

		fmt.Println("arrayOfArrays", i, row)
	}

	fmt.Println("arrayOfArrays after:", m)
}

// pointerArray: the CONTROL that must differ — ranging a POINTER to an array does NOT copy, so
// the loop observes every write. Go evaluates the pointer once; the pointee is shared.
func pointerArray() {
	b := [4]int{1, 2, 3, 4}
	p := &b

	for i, v := range p {
		if i == 0 {
			b[1], b[2], b[3] = 91, 92, 93
		}

		fmt.Println("pointerArray", i, v)
	}

	fmt.Println("pointerArray after:", b)
}

// sliceValue: the second CONTROL — a slice header is copied but the backing store is shared, so
// the loop observes every write, exactly as the pointer case does.
func sliceValue() {
	s := []int{1, 2, 3, 4}

	for i, v := range s {
		if i == 0 {
			s[1], s[2], s[3] = 91, 92, 93
		}

		fmt.Println("sliceValue", i, v)
	}

	fmt.Println("sliceValue after:", s)
}

// indexOnly: with at most one iteration variable and a constant length, Go does not evaluate the
// range expression at all — the body reads the LIVE array through the index.
func indexOnly() {
	a := [4]int{1, 2, 3, 4}

	for i := range a {
		if i == 0 {
			a[1], a[2], a[3] = 91, 92, 93
		}

		fmt.Println("indexOnly", i, a[i])
	}
}

// assignVars: the `=` (not `:=`) range form over an array value takes the same snapshot.
func assignVars() {
	a := [4]int{1, 2, 3, 4}
	var i, v int

	for i, v = range a {
		if i == 0 {
			a[1], a[2], a[3] = 91, 92, 93
		}

		fmt.Println("assignVars", i, v)
	}
}

// mutableRangeVar: the value variable is written in the body, which routes the emission through
// the iterate-a-temp escape — the snapshot must survive that route too.
func mutableRangeVar() {
	a := [4]int{1, 2, 3, 4}

	for i, v := range a {
		if i == 0 {
			a[1], a[2], a[3] = 91, 92, 93
		}

		v *= 10

		fmt.Println("mutableRangeVar", i, v)
	}
}

// aliasedElement: a pointer into the array written through during the loop is the same aliasing
// the snapshot must hide.
func aliasedElement() {
	a := [4]int{1, 2, 3, 4}
	q := &a[2]

	for i, v := range a {
		if i == 0 {
			*q = 90
		}

		fmt.Println("aliasedElement", i, v)
	}

	fmt.Println("aliasedElement after:", a)
}

// arrayOfNamedSlices: the shape math/big's `var bitsList = [...]Bits{...}` has -- an ARRAY whose
// element is a NAMED SLICE type. Go copies the array of slice HEADERS, so the snapshot shares every
// backing store with the original: appending through the loop variable cannot be seen (a fresh
// header), but writing an ELEMENT through it must be, in both the original and later iterations.
func arrayOfNamedSlices() {
	rows := [3]Digits{{1, 2}, {3, 4}, {5, 6}}

	for i, r := range rows {
		if i == 0 {
			rows[1] = Digits{91, 92}
			rows[2][0] = 93
		}

		fmt.Println("arrayOfNamedSlices", i, r)
	}

	fmt.Println("arrayOfNamedSlices after:", rows)
}

func main() {
	arrayValue()
	namedArrayValue()
	arrayField()
	arrayOfArrays()
	pointerArray()
	sliceValue()
	indexOnly()
	assignVars()
	mutableRangeVar()
	aliasedElement()
	arrayOfNamedSlices()
}
