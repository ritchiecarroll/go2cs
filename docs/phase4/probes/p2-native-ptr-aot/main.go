// Command NativePtrAot reads a pointer array in memory the Go runtime did not allocate: the shape
// Windows' DNS_TXT_DATAW hands net.LookupTXT, built here on linux from an anonymous mapping that
// syscall.Mmap makes (not the runtime's own). A record at the mapping's base carries a count and the
// first word of an array of pointers, each naming a NUL-terminated UTF-16 string; net's spelling
// views that array through a window far larger than the memory, re-sliced to the count. A second
// record does the same over a converted struct element, so a builtin element and a struct element
// both reach the native reader.
//
// Four arms follow (G's narrowing and COORD's ruling, 2026-10-19). unsafe.Slice reads the second
// record's words through a **point, as crypto/x509's windows chain reads do. reflect builds *[2]*point over the second record's
// words: point's reader is compiled, since the program spells *[N]*point. reflect builds *[2]*other
// over the same words: the program never spells *[N]*other, so no reader for it was compiled, and the
// converted program refuses by name where Go reads the words (the disclosed divergence). And a third
// record holds a [4]uint16 VALUE field, read through the window over a native struct's array field.
package main

import (
	"fmt"
	"reflect"
	"syscall"
	"unicode/utf16"
	"unsafe"
)

// The DNSTXTData shape: a count, then the first word of the array.
type txtData struct {
	StringCount uint32
	StringArray [1]*uint16
}

type point struct {
	X, Y int32
}

type pointData struct {
	Count  uint32
	Points [1]*point
}

// point's layout, but the program never spells a pointer array of it: only reflection builds one.
type other struct {
	X, Y int32
}

// A record whose array field holds values.
type nameRec struct {
	Len  uint32
	Name [4]uint16
}

// arm runs f, printing a panic's value rather than dying of it.
func arm(name string, f func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: panic: %v\n", name, r)
		}
	}()
	f()
}

func main() {
	b, err := syscall.Mmap(-1, 0, 4096, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)
	if err != nil {
		panic(err)
	}

	base := uintptr(unsafe.Pointer(&b[0]))

	put := func(offset int, s string) {
		for i, r := range s {
			*(*uint16)(unsafe.Pointer(&b[offset+2*i])) = uint16(r)
		}
	}
	word := func(offset int, value uintptr) { *(*uintptr)(unsafe.Pointer(&b[offset])) = value }

	// txtData at 0: count 2, words at 8 and 16.
	put(256, "hello")
	put(512, "world")
	*(*uint32)(unsafe.Pointer(&b[0])) = 2
	word(8, base+256)
	word(16, base+512)

	// pointData at 64: count 2, words at 72 and 80.
	*(*point)(unsafe.Pointer(&b[1024])) = point{3, 4}
	*(*point)(unsafe.Pointer(&b[1032])) = point{5, 12}
	*(*uint32)(unsafe.Pointer(&b[64])) = 2
	word(72, base+1024)
	word(80, base+1032)

	// The address arrives as a number, as the DNS record's does from DnsQuery.
	d := (*txtData)(unsafe.Pointer(base))

	for i, v := range (*[1 << 10]*uint16)(unsafe.Pointer(&d.StringArray[0]))[:d.StringCount:d.StringCount] {
		units := (*[1 << 10]uint16)(unsafe.Pointer(v))[:]
		n := 0
		for units[n] != 0 {
			n++
		}
		fmt.Printf("%d %s\n", i, string(utf16.Decode(units[:n])))
	}

	pd := (*pointData)(unsafe.Pointer(base + 64))

	for i, p := range (*[1 << 10]*point)(unsafe.Pointer(&pd.Points[0]))[:pd.Count:pd.Count] {
		fmt.Printf("point %d %d %d\n", i, p.X, p.Y)
	}

	// The **point arrives as a number, as CertChainContext.Chains does from CertGetCertificateChain. (Reading
	// it out of a pointer FIELD of a struct in native memory is a separate shape this probe does not test.)
	chains := (**point)(unsafe.Pointer(base + 72))

	arm("slice", func() {
		for i, p := range unsafe.Slice(chains, 2) {
			fmt.Printf("slice point %d %d %d\n", i, p.X, p.Y)
		}
	})

	// reflect over the second record's words, as *[2]*point and as *[2]*other.
	for _, e := range []reflect.Type{reflect.TypeOf(point{}), reflect.TypeOf(other{})} {
		arm("reflect "+e.String(), func() {
			v := reflect.NewAt(reflect.ArrayOf(2, reflect.PointerTo(e)), unsafe.Pointer(base+72)).Elem()
			for i := 0; i < v.Len(); i++ {
				p := v.Index(i).Elem()
				fmt.Printf("reflect %s %d %d %d\n", e, i, p.Field(0).Int(), p.Field(1).Int())
			}
		})
	}

	// nameRec at 1536: length 3, then four UTF-16 units at 1540.
	*(*uint32)(unsafe.Pointer(&b[1536])) = 3
	for i, u := range []uint16{'g', 'o', 'p', 0} {
		*(*uint16)(unsafe.Pointer(&b[1540+2*i])) = u
	}

	nr := (*nameRec)(unsafe.Pointer(base + 1536))

	arm("name", func() {
		fmt.Printf("name %d", nr.Len)
		// The field's length is a constant: ranging over nr.Name would read the whole array value.
		for i := 0; i < len(nameRec{}.Name); i++ {
			q := &nr.Name[i]
			fmt.Printf(" %d", *q)
		}
		fmt.Println()
	})

	if err := syscall.Munmap(b); err != nil {
		panic(err)
	}
}
