package main

import "fmt"

type holder struct {
	arr [4]int
}

var pkg [4]int

func set(p *[4]int) { p[0] = 50 }

func main() {
	// No unsafe here: what else names an array's storage across a whole assignment.

	// 1. A slice of the array.
	var a [4]int
	s := a[:]
	a = [4]int{1, 2, 3, 4}
	fmt.Println("slice reads:", s[0])
	s[1] = 20
	fmt.Println("slice writes:", a[1])

	// 2. A pointer to an element.
	var b [4]int
	p := &b[0]
	b = [4]int{1, 2, 3, 4}
	*p = 30
	fmt.Println("element pointer:", b[0], *p)

	// 3. A pointer to the array.
	var c [4]int
	q := &c
	c = [4]int{1, 2, 3, 4}
	q[0] = 40
	fmt.Println("array pointer:", c[0])
	set(&c)
	fmt.Println("array pointer argument:", c[0])

	// 4. Assigned from another variable, with a slice taken before.
	var d, other [4]int
	other[0] = 9
	t := d[:]
	d = other
	t[0] = 60
	fmt.Println("from a variable:", d[0], other[0])

	// 5. A field.
	var h holder
	u := h.arr[:]
	h.arr = [4]int{1, 2, 3, 4}
	u[0] = 70
	fmt.Println("field:", h.arr[0])

	// 6. A package variable.
	v := pkg[:]
	pkg = [4]int{1, 2, 3, 4}
	v[0] = 80
	fmt.Println("package:", pkg[0])

	// 7. Assigned through a pointer to the array.
	var e [4]int
	w := e[:]
	r := &e
	*r = [4]int{1, 2, 3, 4}
	w[0] = 90
	fmt.Println("through a pointer:", e[0])
}
