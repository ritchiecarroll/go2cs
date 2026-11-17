package main

import (
	"fmt"
	"unsafe"
)

func fp(f interface{}) uintptr {
	type iface [2]uintptr
	return (*iface)(unsafe.Pointer(&f))[1]
}

func one() int { return 1 }

func main() {
	fmt.Println(fp(one) != 0)
	fmt.Println(fp(one) == fp(one))
}
