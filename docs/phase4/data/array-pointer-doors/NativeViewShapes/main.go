// NativeViewShapes: the shapes libc v1.80.1 and sqlite v1.62.0 use over RawMem, one arm per process.
package main

import (
	"fmt"
	"os"
	"runtime"
	"syscall"
	"unsafe"
)

// libc64.go:22
type RawMem [1<<50 - 1]byte

// A named array small enough to allocate: the same emission, without the size.
type smallMem [64]byte

type hdr struct {
	a   uint32
	buf [12]byte
}

func native(n int) uintptr {
	b, err := syscall.Mmap(-1, 0, n, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)
	if err != nil {
		panic(err)
	}
	return uintptr(unsafe.Pointer(&b[0]))
}

func peek(p uintptr, n int) string {
	out := make([]byte, n)
	for i := 0; i < n; i++ {
		out[i] = *(*byte)(unsafe.Pointer(p + uintptr(i)))
	}
	return fmt.Sprintf("%q", out)
}

func poke(p uintptr, s string) {
	for i := 0; i < len(s); i++ {
		*(*byte)(unsafe.Pointer(p + uintptr(i))) = s[i]
	}
}

func fill(b []byte, c byte) int {
	for i := range b {
		b[i] = c
	}
	return len(b)
}

func main() {
	arm := os.Args[1]
	p := native(4096)
	q := native(4096)
	poke(p, "XXXXXXXXXXXXXXXX") // the memory is not zero when a view is taken
	poke(q, "abcdefghijklmnop")

	switch arm {
	case "A": // libc.go:751 copy((*RawMem)(unsafe.Pointer(str))[:r:r], b)
		s := "hello"
		n := len(s)
		c := copy((*RawMem)(unsafe.Pointer(p))[:n:n], s)
		fmt.Println(c, peek(p, 8))
	case "B": // libc.go:1965 memcpy
		n := 6
		c := copy((*RawMem)(unsafe.Pointer(p))[:n:n], (*RawMem)(unsafe.Pointer(q))[:n:n])
		fmt.Println(c, peek(p, 8))
	case "C": // libc_windows.go:710 copy((*RawMem)(unsafe.Pointer(buf))[:], wd)
		wd := []byte("/tmp")
		c := copy((*RawMem)(unsafe.Pointer(p))[:], wd)
		fmt.Println(c, peek(p, 8))
	case "D": // libc_windows.go:711 (*RawMem)(unsafe.Pointer(buf))[len(wd)] = 0
		wd := []byte("/tmp")
		(*RawMem)(unsafe.Pointer(p))[len(wd)] = 0
		fmt.Println(peek(p, 8))
	case "E": // libc.go:2715 bzero, libc_windows.go:1904 fread
		n := 5
		b := (*RawMem)(unsafe.Pointer(p))[:n]
		for i := range b {
			b[i] = 0
		}
		b[0] = 'u'
		b = b[1:]
		fmt.Println(len(b), cap(b) == 1<<50-2, peek(p, 8))
	case "F": // libc_windows.go:6536 var raw = (*RawMem)(unsafe.Pointer(p))
		var raw = (*RawMem)(unsafe.Pointer(q))
		w := *(*uint16)(unsafe.Pointer(&raw[2]))
		raw[1] = 'Z'
		fmt.Println(w, peek(q, 4))
	case "G": // libc_windows.go:1657 a view over Go memory
		var buf [16]byte
		c := copy((*RawMem)(unsafe.Pointer(&buf[0]))[:len(buf):len(buf)], "errno 7\x00")
		fmt.Printf("%d %q\n", c, buf[:8])
	case "H": // libc.go:2617 crand.Read((*RawMem)(unsafe.Pointer(buffer))[:length])
		n := fill((*RawMem)(unsafe.Pointer(p))[:4], 'r')
		fmt.Println(n, peek(p, 8))
	case "I": // sqlite conn.go:280 copy(v, (*libc.RawMem)(unsafe.Pointer(p))[:len:len])
		v := make([]byte, 6)
		c := copy(v, (*RawMem)(unsafe.Pointer(q))[:6:6])
		fmt.Printf("%d %q\n", c, v)
	case "J": // libc.go:2631 unsafe.Sizeof(RawMem{})
		fmt.Println(uint64(unsafe.Sizeof(RawMem{})))
	case "K": // sqlite vtab.go:333 mem := (*libc.RawMem)(unsafe.Pointer(p))[:sz:sz]; the memory changes after
		mem := (*RawMem)(unsafe.Pointer(q))[:4:4]
		poke(q, "WXYZ")
		mem[3] = '!'
		fmt.Printf("%q %s\n", mem, peek(q, 4))
	case "L": // libc_musl.go:1508 a uintptr constant bound
		var r [16]byte
		for i := range r {
			r[i] = byte('0' + i%10)
		}
		c := copy((*RawMem)(unsafe.Pointer(p))[:unsafe.Sizeof(r)], r[:])
		fmt.Println(c, peek(p, 16))
	case "M": // the same shapes over a named array that fits: whole [:] and an index store
		c := copy((*smallMem)(unsafe.Pointer(p))[:], "small")
		(*smallMem)(unsafe.Pointer(p))[5] = 0
		fmt.Println(c, peek(p, 8))
	case "C0": // C over memory that is still zero
		z := native(4096)
		wd := []byte("/tmp")
		c := copy((*RawMem)(unsafe.Pointer(z))[:], wd)
		fmt.Println(c, peek(z, 8))
	case "D0":
		z := native(4096)
		(*RawMem)(unsafe.Pointer(z))[4] = 7
		fmt.Println(peek(z, 8))
	case "F0":
		z := native(4096)
		var raw = (*RawMem)(unsafe.Pointer(z))
		w := *(*uint16)(unsafe.Pointer(&raw[2]))
		raw[1] = 'Z'
		fmt.Println(w, peek(z, 4))
	case "M0":
		z := native(4096)
		c := copy((*smallMem)(unsafe.Pointer(z))[:], "small")
		(*smallMem)(unsafe.Pointer(z))[5] = 0
		fmt.Println(c, peek(z, 8))
	case "N": // Go memory of another element type: a uint64 viewed as bytes
		var x uint64 = 0x0102030405060708
		b := (*[8]byte)(unsafe.Pointer(&x))[:8:8]
		b[0] = 0xff
		fmt.Println(x == 0x01020304050607ff)
	case "O": // a byte-array field of a heap struct, a collection between the view and the write
		h := &hdr{}
		v := (*RawMem)(unsafe.Pointer(&h.buf[0]))[:12:12]
		runtime.GC()
		copy(v, "field")
		fmt.Printf("%q\n", h.buf[:6])
	case "P": // an interior address of Go memory through unsafe.Add
		buf := make([]byte, 32)
		v := (*RawMem)(unsafe.Add(unsafe.Pointer(&buf[0]), 4))[:8:8]
		runtime.GC()
		copy(v, "interior")
		fmt.Printf("%q\n", buf[:14])
	case "Q": // an unnamed array viewed whole
		var arr [4]uint32
		w := (*[16]byte)(unsafe.Pointer(&arr))[:]
		w[4] = 1
		fmt.Println(arr)
	case "R": // sqlite lib: an unnamed array indexed through a conversion
		(*[4]int32)(unsafe.Pointer(p))[1] = 7
		fmt.Println((*[4]int32)(unsafe.Pointer(p))[1])
	case "S":
		e := &(*[4]int32)(unsafe.Pointer(p))[2]
		*e = 9
		fmt.Println(*(*int32)(unsafe.Pointer(p + 8)))
	case "T": // libc_windows.go:1786
		*(*[2]byte)(unsafe.Pointer(p)) = [2]byte{'C', 0}
		fmt.Println(peek(p, 8))
	}
}
