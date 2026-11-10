package main

import (
	"fmt"
	"unsafe"
)

// A Go alias of a C type, and Go declarations that use C types: each reaches the pseudo-package as C.<name>.
type cint = _Ctype_int

type pair struct {
	a, b _Ctype_int
}

func add(a, b _Ctype_int) _Ctype_int { return a + b }

func walk(p *_Ctype_struct_point) (n int) {
	for ; p != nil; p = p.next {
		n++
	}

	return
}

func colorName(c _Ctype_enum_color) string {
	switch c {
	case _Ciconst_RED:
		return "red"
	case _Ciconst_GREEN:
		return "green"
	}

	return "other"
}

func main() {
	var d _Ctype_div_t
	d.quot = _Ctype_int(_Ciconst_LIMIT)
	d.rem = 3

	p := _Ctype_struct_point{x: 1, y: 2}
	p.name[0] = 'a'
	q := &_Ctype_struct_point{x: 3, next: &p}

	// A C struct with an inline array is copied by value, array included.
	p2 := p
	p2.name[0] = 'b'

	seg := _Ctype_struct_segment{from: p, to: *q}
	seg.to.name[1] = 'z'

	var v _Ctype_void
	var u _Ctype_union_value
	u[0] = 7

	var size _Ctype_size_t = 42
	var lim _Ctype_long = _Ciconst_LIMIT
	var k cint = add(2, 3)

	big := _Ctype_int(2147483647)
	big++

	xs := []_Ctype_int{1, 2, 3}
	m := map[_Ctype_char]_Ctype_double{'z': 1.5}
	pr := pair{a: 4, b: 5}

	fmt.Println(d.quot, d.rem, p.x+p.y, p.name[0], p2.name[0], seg.from.x, seg.to.x, seg.to.name[1], q.name[1])
	fmt.Println(unsafe.Sizeof(p), unsafe.Sizeof(v), unsafe.Sizeof(u), unsafe.Sizeof(seg), unsafe.Sizeof(d))
	fmt.Println(walk(q), u[0], size, lim, k, big, len(xs), xs[2], m['z'], pr.a+pr.b)
	fmt.Println(colorName(_Ciconst_RED), colorName(_Ciconst_GREEN), colorName(_Ciconst_BLUE), _Cfconst_HALF, _Csconst_GREETING+"!")
	fmt.Println(_Ctype_uint(1), _Ctype_schar(-2), _Ctype_uchar(3), _Ctype_short(-4), _Ctype_ushort(5), _Ctype_ulong(6), _Ctype_longlong(-7), _Ctype_ulonglong(8), _Ctype_float(0.25), _Ctype_double(0.125))
	fmt.Printf("%T %T %T %T %T\n", p, k, d, size, _Ctype_enum_color(_Ciconst_BLUE))
}
