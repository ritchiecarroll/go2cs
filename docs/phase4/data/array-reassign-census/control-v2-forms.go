package main

import "unsafe"

type S struct{ arr [4]int }

func f() ([4]int, error) { return [4]int{}, nil }

func g() ([4]int, [4]int) { return [4]int{}, [4]int{} }

func main() {
	var a, b [4]int
	a = b                       // SINGLE
	ch := make(chan [4]int, 2)
	a = <-ch                    // RECV
	select {
	case a = <-ch: // SELECT
	default:
	}
	a, b = b, a                 // TUPLE-N x2
	a, b = g()                  // TUPLE-CALL x2
	m := map[int][4]int{}
	var ok bool
	a, ok = m[1]                // TUPLE-COMMAOK
	a, err := f()               // DEFINE-REDECL (a only)
	_ = err
	xs := [][4]int{{}, {}}
	for _, a = range xs {       // RANGE
	}
	buf := make([]int, 8)
	p := (*[4]int)(unsafe.Pointer(&buf[0]))
	*p = a                      // DEREF UNSAFE-CONV via var
	*(*[4]int)(unsafe.Pointer(&buf[4])) = a // DEREF UNSAFE-CONV direct
	q := (*[4]int)(buf)
	*q = a                      // DEREF SLICE-CONV
	r := &a
	*r = b                      // DEREF OTHER
	var s S
	s = S{}                     // STRUCT SINGLE
	_, _, _ = ok, s, r
}
