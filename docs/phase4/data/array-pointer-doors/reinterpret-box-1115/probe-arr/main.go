// Box-kind seat, a second converter probe: a view of an ARRAY field as an array of another element type of the
// same size, `(*[8]byte)(unsafe.Pointer(&d.name))`, name a [8]int8. os.(*File).readdir on darwin writes it over
// a struct variable (dir_darwin.go:77). One line per arm; a panic is printed and the next arm runs.
package main

import (
	"fmt"
	"os"
	"strconv"
	"unsafe"
)

type dirent struct {
	ino  uint64
	name [8]int8
}

type holder struct {
	d dirent
}

var gd = dirent{ino: 7, name: [8]int8{'g', 'l', 'o', 'b'}}

func try(name string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: PANIC %v\n", name, r)
		}
	}()

	fmt.Printf("%s: %s\n", name, f())
}

func trim(b []byte) string {
	for i, c := range b {
		if c == 0 {
			return string(b[:i])
		}
	}

	return string(b)
}

func ptrSlice(d *dirent) string {
	name := (*[8]byte)(unsafe.Pointer(&d.name))[:]
	return trim(name)
}

func ptrLocal(d *dirent) string {
	p := (*[8]byte)(unsafe.Pointer(&d.name))
	p[0] = 'P'
	return fmt.Sprint(p[0], p[1], d.name[0])
}

func varSlice() string {
	var d dirent
	d.name = [8]int8{'v', 'a', 'r'}
	name := (*[8]byte)(unsafe.Pointer(&d.name))[:]
	return trim(name)
}

func varLocal() string {
	var d dirent
	d.name = [8]int8{'v', 'a', 'r'}
	p := (*[8]byte)(unsafe.Pointer(&d.name))
	p[0] = 'V'
	return fmt.Sprint(p[0], p[1], d.name[0])
}

func globalSlice() string {
	name := (*[8]byte)(unsafe.Pointer(&gd.name))[:]
	return trim(name)
}

func twoHopSlice(h *holder) string {
	name := (*[8]byte)(unsafe.Pointer(&h.d.name))[:]
	return trim(name)
}

func twoHopLocal(h *holder) string {
	p := (*[8]byte)(unsafe.Pointer(&h.d.name))
	p[1] = 'W'
	return fmt.Sprint(p[0], p[1], h.d.name[1])
}

func main() {
	d := &dirent{ino: 1, name: [8]int8{'p', 't', 'r'}}
	h := &holder{d: dirent{name: [8]int8{'t', 'w', 'o'}}}
	arms := []struct {
		name string
		f    func() string
	}{
		{"pointer root slice", func() string { return ptrSlice(d) }},
		{"pointer root local", func() string { return ptrLocal(d) }},
		{"variable root slice", varSlice},
		{"variable root local", varLocal},
		{"package root slice", globalSlice},
		{"two-hop slice", func() string { return twoHopSlice(h) }},
		{"two-hop local", func() string { return twoHopLocal(h) }},
	}

	// With an argument, one arm alone, so that an arm that ends the process does not hide the arms after it.
	if len(os.Args) > 1 {
		n, _ := strconv.Atoi(os.Args[1])
		try(arms[n].name, arms[n].f)
		return
	}

	for _, a := range arms {
		try(a.name, a.f)
	}
}
