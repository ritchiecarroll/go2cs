// Guards Go's pointer IDENTITY over NATIVE memory: one address is one pointer, whichever way the program reached it.
// syscall.Mmap hands back pages the kernel owns; `&b[4]` takes an element of that slice, and `&p[4]` takes the same
// element through an array pointer laid over the same pages, `(*[8]byte)(unsafe.Pointer(&b[0]))`. Go answers that the
// two pointers are equal. The converted program reached the two through different golib box kinds (a NativeBox for
// the slice element, an element box through the array view), and each kind compared only against its own, so `==`
// read false (1.26.10.2 seat #16, docs/phase4/BOARD-next-validation-candidates.md).
//
// Every comparison here keeps the BOXED form: one operand is a stored pointer, a pointer rebuilt through
// unsafe.Pointer, or a map key, so none takes the element-address compare (`Ꮡelem`, seat #10). The different-element
// and managed-slice lines are the controls: a different address, or managed storage, is never equal.
//
// PLATFORM-EXCLUSIVE: [GoPlatformExclusive("linux")], following NativeFieldPortAlias. syscall.Mmap with MAP_ANON is
// not on Windows.
package main

import (
	"fmt"
	"syscall"
	"unsafe"
)

func main() {
	b, err := syscall.Mmap(-1, 0, 4096, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)

	if err != nil {
		fmt.Println("mmap:", err)
		return
	}

	defer syscall.Munmap(b)

	p := (*[8]byte)(unsafe.Pointer(&b[0]))

	q := &b[4]
	r := &p[4]
	fmt.Println("stored slice element == array element:", q == &p[4], r == q)
	fmt.Println("different elements                    :", q == &p[3], &p[5] == q)

	rebuilt := (*byte)(unsafe.Pointer(&b[4]))
	fmt.Println("unsafe.Pointer round trip             :", rebuilt == &p[4], r == rebuilt)

	viaUintptr := (*byte)(unsafe.Pointer(uintptr(unsafe.Pointer(&b[0])) + 4))
	fmt.Println("uintptr arithmetic                    :", viaUintptr == r, q == viaUintptr)

	seen := map[*byte]string{q: "slice element"}
	fmt.Println("map keyed by one finds the other      :", seen[r])

	m := make([]byte, 8)
	fmt.Println("managed element vs native element     :", &m[4] == r, q == &m[4])

	*r = 42
	fmt.Println("one storage                           :", *q, b[4], *rebuilt)
}
