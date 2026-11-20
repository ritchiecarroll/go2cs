// CcgoStringTable: ccgo's string-literal table, as modernc.org/sqlite and modernc.org/libc are generated. Every C string
// literal of a package lives in one Go string, whose data address is taken ONCE, at package initialization, as a
// number, and read at `__ccgo_ts + N` for the program's whole life (docs/phase4/DESIGN-sqlite-ccgo-ts.md, W14). The
// vfs package carries a second such pair under other names. The reads happen in a LATER function, after forced
// collections with finalizers and allocation churn, so a number that named moved memory would read other bytes. The
// program prints only the bytes it reads; the data address itself differs from Go's by design and is never printed.
package main

import (
	"fmt"
	"reflect"
	"runtime"
	"unsafe"
)

var __ccgo_ts1 = "hello\x00world\x00%s=%d\x00table\x00"
var __ccgo_ts = (*reflect.StringHeader)(unsafe.Pointer(&__ccgo_ts1)).Data

var ts1 = "unix-none\x00unix-dotfile\x00"
var ts = (*reflect.StringHeader)(unsafe.Pointer(&ts1)).Data

var sink [][]byte

// libc's GoString: a NUL-terminated C string at a number.
func goString(p uintptr) string {
	var b []byte
	for {
		c := *(*byte)(unsafe.Pointer(p))
		if c == 0 {
			break
		}
		b = append(b, c)
		p++
	}
	return string(b)
}

func churn() {
	for round := 0; round < 3; round++ {
		for i := 0; i < 20000; i++ {
			sink = append(sink, make([]byte, 64))
		}
		sink = nil
		runtime.GC()
	}
}

func main() {
	churn()
	fmt.Println(goString(__ccgo_ts+0), goString(__ccgo_ts+6), goString(__ccgo_ts+12), goString(__ccgo_ts+18))
	fmt.Println(goString(ts+0), goString(ts+10))
	churn()
	fmt.Println(goString(__ccgo_ts+6), goString(ts+10))
}
