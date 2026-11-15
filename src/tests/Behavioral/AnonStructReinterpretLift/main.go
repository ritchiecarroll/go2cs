package main

import (
	"fmt"
	"math"
	"unsafe"
)

// sqlite-cs L1 (modernc.org/libc on linux refused conversion: "unresolved dynamic type(s) were emitted as raw Go
// source"). ccgo type-puns floats through anonymous structs, and two of its shapes left a function-local lift unnamed:
//
//   - the source of a pointer reinterpret is the address of an anonymous-struct LITERAL, which is lifted only while the
//     operand renders, after the Reinterpret's element names were taken (`Reinterpret<struct{f float64}, …>`);
//   - `var ux, uy struct{…}`: go/types checks the type once per name, so the first name's struct is a second object
//     the lift was never keyed on, and its declaration and every `Ꮡux.Reinterpret<…>` were raw Go.
type Tuint32_t = uint32
type Tuint64_t = uint64

// X__fpclassify's shape: one local, filled from a reinterpret of a composite literal's address.
func fpclassify(x float64) int32 {
	var e int32
	var u struct {
		Fi [0]Tuint64_t
		Ff float64
	}
	_ = u
	u = *(*struct {
		Fi [0]Tuint64_t
		Ff float64
	})(unsafe.Pointer(&struct{ f float64 }{f: x}))
	e = int32(*(*Tuint64_t)(unsafe.Pointer(&u)) >> int32(52) & uint64(0x7ff))
	return e
}

// Xcopysignf's shape: TWO locals declared together, each written through a pointer to it.
func copysignf(x float32, y float32) float32 {
	var ux, uy struct {
		Fi [0]Tuint32_t
		Ff float32
	}
	_, _ = ux, uy
	ux = *(*struct {
		Fi [0]Tuint32_t
		Ff float32
	})(unsafe.Pointer(&struct{ f float32 }{f: x}))
	uy = *(*struct {
		Fi [0]Tuint32_t
		Ff float32
	})(unsafe.Pointer(&struct{ f float32 }{f: y}))
	*(*Tuint32_t)(unsafe.Pointer(&ux)) &= uint32(0x7fffffff)
	*(*Tuint32_t)(unsafe.Pointer(&ux)) |= *(*Tuint32_t)(unsafe.Pointer(&uy)) & uint32(0x80000000)
	return *(*float32)(unsafe.Pointer(&ux))
}

// The multi-name shape without a reinterpret: the first name's plain declaration was raw Go too.
func pair(a, b uint32) uint32 {
	var p, q struct{ A uint32 }
	p.A, q.A = a, b
	return p.A*10 + q.A
}

func main() {
	fmt.Println(fpclassify(1.5), fpclassify(0), fpclassify(math.Inf(1)))
	fmt.Println(copysignf(3, -1), copysignf(-2.5, 1))
	fmt.Println(pair(4, 2))
}
