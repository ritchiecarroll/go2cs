// EmbedLayoutMatchesGo guards owner ruling (A): a converted struct is laid out where Go lays it, so a struct read
// or written through a pointer over raw bytes touches the bytes Go's would. Each shape is viewed over a byte
// buffer with `(*T)(unsafe.Pointer(&buf[0]))`, written field by field and as a whole value, and the buffer is
// printed: the output equals Go's only if every field sits at its Go offset.
//
//   - ordered: an embed declared BEFORE a field (`struct{ pair; c uint32 }`). The generator used to declare an
//     embed's storage in a later partial part, after every converter field, so the CLR put c first.
//   - leader: a zero-size blank field before an embed (`struct{ _ nih; pair }`, runtime's lfstack-list node
//     shape). Go gives the blank no bytes; the CLR gives an empty struct one, so the embed sat at offset 4.
//   - ptrEmbed: a pointer embed beside a field, the reference-bearing shape, read through its own fields.
package main

import (
	"fmt"
	"unsafe"
)

type pair struct{ a, b uint32 }

type ordered struct {
	pair
	c uint32
}

type nih struct{}

type leader struct {
	_ nih
	pair
}

type ptrEmbed struct {
	*pair
	d uint32
}

func main() {
	buf := make([]byte, 16)

	o := (*ordered)(unsafe.Pointer(&buf[0]))
	o.a, o.b, o.c = 1, 2, 3
	fmt.Println("ordered fields:", buf[:12])

	*o = ordered{pair{4, 5}, 6}
	fmt.Println("ordered whole: ", buf[:12])

	v := *o
	fmt.Println("ordered read:  ", v.a, v.b, v.c, unsafe.Sizeof(v), unsafe.Offsetof(v.c))

	for i := range buf {
		buf[i] = 0
	}

	l := (*leader)(unsafe.Pointer(&buf[0]))
	l.a, l.b = 7, 8
	fmt.Println("leader fields: ", buf[:8])

	w := *l
	fmt.Println("leader read:   ", w.a, w.b, unsafe.Sizeof(w))

	p := ptrEmbed{&pair{9, 10}, 11}
	q := p.pair
	q.a = 12
	fmt.Println("ptrEmbed:      ", p.a, p.b, p.d, p.pair == q)
}
