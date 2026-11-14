package main

import (
	"fmt"
	"unsafe"

	"ForeignAliasElementAddress/stdio"
)

// sqlite-cs S12: modernc.org/libc libc_windows.go `iobMap[uintptr(unsafe.Pointer(&X_iob[i]))] = int32(i)` over
// `X_iob [3]stdio.FILE`: another package's ALIAS named by qualifier, here in an element-address position.
var X_iob [3]stdio.FILE

func main() {
	m := map[uintptr]int32{}
	for i := range X_iob {
		m[uintptr(unsafe.Pointer(&X_iob[i]))] = int32(i)
	}
	X_iob[1].Fcnt = 4
	fmt.Println(len(m), X_iob[1].Fcnt, unsafe.Sizeof(X_iob[0])) // Go: 3 4 16
}
