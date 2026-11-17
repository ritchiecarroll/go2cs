package main

import (
	"fmt"
	"unsafe"

	"ForeignAnonStructAliasElemAddress/stdio"
)

// sqlite-cs W5a: modernc.org/libc libc_windows.go keys a map by the address of each element of a package-level array of
// another package's alias of an alias of an anonymous struct (`X_iob [stdio.X_IOB_ENTRIES]stdio.FILE`). The array's
// declaration spelled the element through the alias table (`stdioꓸFILE`), but the element ADDRESS took the Go-shaped
// name, `ᏑX_iob.at<stdio.FILE>(i)`: a type argument naming a member stdio_package does not have (CS0426).
var X_iob [stdio.X_IOB_ENTRIES]stdio.FILE

var iobMap = map[uintptr]int32{}

func init() {
	for i := range X_iob {
		iobMap[uintptr(unsafe.Pointer(&X_iob[i]))] = int32(i)
	}
}

func main() {
	// Writes through each element's address land in the array itself.
	for i := range X_iob {
		p := &X_iob[i]
		p.F_cnt = int32(10 * (i + 1))
		p.F__ccgo_pad1[i] = byte(i + 1)
	}

	first, last := uintptr(unsafe.Pointer(&X_iob[0])), uintptr(unsafe.Pointer(&X_iob[2]))

	fmt.Println(len(iobMap), iobMap[first], iobMap[last], last-first == 2*unsafe.Sizeof(X_iob[0]))
	fmt.Println(X_iob[0].F_cnt, X_iob[1].F_cnt, X_iob[2].F_cnt, X_iob[1].F__ccgo_pad1, X_iob[2].F__ccgo_pad1)
}
