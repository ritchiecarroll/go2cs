// The native-memory arm of AddressOfFieldThroughConversion (sqlite-cs W9b): `&(*T)(p).f` where p addresses pages
// the kernel owns, as libc's calloc'd memory does for sqlite's mutexes and Mem cells. go2cs rendered the address
// as Ꮡ(<conversion>.Value.f), a box over a COPY of the struct, so a write through it never reached the mapping.
// Each line writes through the address and reads back off the mapping's own bytes.
//
// PLATFORM-EXCLUSIVE: [GoPlatformExclusive("linux")], following NativeFieldPortAlias. syscall.Mmap with MAP_ANON
// is not on Windows.
package main

import (
	"encoding/binary"
	"fmt"
	"sync/atomic"
	"syscall"
	"unsafe"
)

type rec struct {
	cnt int32
	id  int32
}

func main() {
	page, err := syscall.Mmap(-1, 0, 4096, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)
	if err != nil {
		fmt.Println("mmap:", err)
		return
	}
	defer syscall.Munmap(page)

	p := unsafe.Pointer(&page[0])

	atomic.StoreInt32(&(*rec)(p).id, 7)
	fmt.Println("atomic store:", (*rec)(p).id, binary.LittleEndian.Uint32(page[4:]))

	*(*int32)(unsafe.Pointer(&(*rec)(p).cnt)) = 11
	fmt.Println("union write:", (*rec)(p).cnt, binary.LittleEndian.Uint32(page[0:]))

	q := &(*rec)(p).id
	*q = 9
	fmt.Println("kept address:", (*rec)(p).id, binary.LittleEndian.Uint32(page[4:]), uintptr(unsafe.Pointer(q))-uintptr(p))

	fmt.Println("parenthesized CAS:", atomic.CompareAndSwapInt32(&((*rec)(p)).id, 9, 4), binary.LittleEndian.Uint32(page[4:]))
}
