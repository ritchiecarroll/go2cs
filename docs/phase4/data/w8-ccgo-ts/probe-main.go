package main

import (
	"fmt"
	"reflect"
	"runtime"
	"unsafe"
)

// ccgo's shape: every C string literal of the package lives in one Go string, and its base is taken once, as a
// number, for the program's whole life (modernc.org/sqlite: __ccgo_ts = (*reflect.StringHeader)(unsafe.Pointer(&__ccgo_ts1)).Data).
var __ccgo_ts1 = "hello\x00world\x00%s=%d\x00"
var __ccgo_ts = (*reflect.StringHeader)(unsafe.Pointer(&__ccgo_ts1)).Data

// libc's GoString: read a NUL-terminated C string at a number.
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

var sink [][]byte

func main() {
	fmt.Println("read:", goString(__ccgo_ts+0), goString(__ccgo_ts+6), goString(__ccgo_ts+12))
	for i := 0; i < 3; i++ {
		for j := 0; j < 20000; j++ {
			sink = append(sink, make([]byte, 64))
		}
		sink = nil
		runtime.GC()
	}
	fmt.Println("after GC:", goString(__ccgo_ts+0), goString(__ccgo_ts+6), goString(__ccgo_ts+12))
	again := (*reflect.StringHeader)(unsafe.Pointer(&__ccgo_ts1)).Data
	fmt.Println("same number on a second read:", again == __ccgo_ts)
	fmt.Println("equals StringData:", uintptr(unsafe.Pointer(unsafe.StringData(__ccgo_ts1))) == __ccgo_ts)
}
