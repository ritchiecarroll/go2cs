package main

import (
	"fmt"
	"unsafe"
)

type small [8]byte

func lenDeref(p unsafe.Pointer) int { return len(*(*[4]byte)(p)) }
func capDeref(p unsafe.Pointer) int { return cap(*(*[4]byte)(p)) }
func lenPtr(p unsafe.Pointer) int   { return len((*[4]byte)(p)) }
func capPtr(p unsafe.Pointer) int   { return cap((*[4]byte)(p)) }
func lenNamed(p unsafe.Pointer) int { return len(*(*small)(p)) }
func lenAddr(x *uint64) int         { return len(*(*[4]byte)(unsafe.Pointer(x))) }

var calls int

func get(p unsafe.Pointer) unsafe.Pointer { calls++; return p }

func lenCall(p unsafe.Pointer) int { return len(*(*[4]byte)(get(p))) }

func rangeVal(p unsafe.Pointer) (n int) {
	for _, b := range *(*[4]byte)(p) {
		n += 1 + int(b)*100
	}
	return
}

func rangeKeyVal(p unsafe.Pointer) (n int) {
	for i, b := range *(*[4]byte)(p) {
		n += i + int(b)*100
	}
	return
}

func rangeKey(p unsafe.Pointer) (n int) {
	for i := range *(*[4]byte)(p) {
		n += i + 1
	}
	return
}

func rangeNone(p unsafe.Pointer) (n int) {
	for range *(*[4]byte)(p) {
		n++
	}
	return
}

func rangePtrKey(p unsafe.Pointer) (n int) {
	for i := range (*[4]byte)(p) {
		n += i + 1
	}
	return
}

func rangePtrVal(p unsafe.Pointer) (n int) {
	for _, b := range (*[4]byte)(p) {
		n += 1 + int(b)*100
	}
	return
}

// Go copies the array before the loop: a store in the body does not reach the values ranged.
func rangeCopy(p unsafe.Pointer) (n int) {
	for i, b := range *(*[4]byte)(p) {
		(*[4]byte)(p)[3] = 9
		n += i + int(b)
	}
	return
}

// Go reads each element through the pointer as the loop reaches it: a store in the body is seen.
func rangePtrLive(p unsafe.Pointer) (n int) {
	for i, b := range (*[4]byte)(p) {
		if i == 0 {
			(*[4]byte)(p)[3] = 50
		}
		n += int(b)
	}
	return
}

func rangePtrNil() (n int) {
	defer func() { recover(); n = -1 }()
	for _, b := range (*[4]byte)(nil) {
		n += int(b)
	}
	return
}

func main() {
	var arr [64]byte
	for i := range arr {
		arr[i] = byte(i + 1)
	}
	var x uint64 = 0x0807060504030201
	p := unsafe.Pointer(&arr)
	fmt.Println(lenDeref(p), capDeref(p), lenPtr(p), capPtr(p), lenAddr(&x))
	fmt.Println(lenCall(p), calls)
	fmt.Println(rangeVal(p), rangeKeyVal(p), rangeKey(p), rangeNone(p), rangePtrKey(p), rangePtrVal(p))
	fmt.Println(rangeVal(unsafe.Pointer(&x)), rangeKey(unsafe.Pointer(&x)), rangePtrVal(unsafe.Pointer(&x)))
	fmt.Println(rangeCopy(p), arr[3])
	fmt.Println(lenDeref(nil), capDeref(nil), lenPtr(nil), rangeKey(nil), rangeNone(nil), rangePtrKey(nil))
	fmt.Println(lenNamed(p))
	fmt.Println(rangePtrLive(p), rangePtrLive(unsafe.Pointer(&x)), x == 0x0807060532030201, rangePtrNil())
}
