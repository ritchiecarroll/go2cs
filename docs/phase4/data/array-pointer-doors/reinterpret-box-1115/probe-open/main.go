// Box-kind seat, the open-slice probe: `(*[8]byte)(unsafe.Pointer(&x.name))[:]`, name a [8]int8, over the three
// roots the field view does not take (a promoted field, a field of a slice element, a call's result), and the
// same view written from a variable or pointer root. One line per arm. With an argument, one arm alone, so
// that an arm that ends the process does not hide the arms after it.
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

type wrap struct {
	dirent
	tag int
}

var kept = &dirent{ino: 3, name: [8]int8{'c', 'a', 'l', 'l'}}

func get() *dirent { return kept }

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

func promoted() string {
	var w wrap
	w.name = [8]int8{'p', 'r', 'o', 'm'}
	name := (*[8]byte)(unsafe.Pointer(&w.name))[:]
	return trim(name)
}

func sliceElement() string {
	ds := []dirent{{name: [8]int8{'z'}}, {name: [8]int8{'e', 'l', 'e', 'm'}}}
	name := (*[8]byte)(unsafe.Pointer(&ds[1].name))[:]
	return trim(name)
}

func callResult() string {
	name := (*[8]byte)(unsafe.Pointer(&get().name))[:]
	return trim(name)
}

func promotedWrittenOut() string {
	var w wrap
	w.name = [8]int8{'p', 'r', 'o', 'm'}
	name := (*[8]byte)(unsafe.Pointer(&w.dirent.name))[:]
	return trim(name)
}

func sliceElementThroughPointer() string {
	ds := []dirent{{name: [8]int8{'z'}}, {name: [8]int8{'e', 'l', 'e', 'm'}}}
	p := &ds[1]
	name := (*[8]byte)(unsafe.Pointer(&p.name))[:]
	name[0] = 'E'
	return trim(name) + " " + fmt.Sprint(ds[1].name[0])
}

func callResultThroughPointer() string {
	p := get()
	name := (*[8]byte)(unsafe.Pointer(&p.name))[:]
	return trim(name)
}

func main() {
	arms := []struct {
		name string
		f    func() string
	}{
		{"promoted field", promoted},
		{"field of a slice element", sliceElement},
		{"field of a call's result", callResult},
		{"promoted field, embedded field written out", promotedWrittenOut},
		{"slice element through a pointer variable", sliceElementThroughPointer},
		{"call's result through a pointer variable", callResultThroughPointer},
	}

	if len(os.Args) > 1 {
		n, _ := strconv.Atoi(os.Args[1])
		try(arms[n].name, arms[n].f)
		return
	}

	for _, a := range arms {
		try(a.name, a.f)
	}
}
