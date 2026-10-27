package main

import (
	"fmt"
	"runtime"
)

// Each panic text names a dynamic type the way Go's %T spells it: an int32 (or a rune) is
// "int32", a byte is "uint8", and only a Go int is "int".

func pin(label string, v any) {
	defer func() {
		fmt.Println(label+":", recover())
	}()

	var p runtime.Pinner
	p.Pin(v)
}

func assertString(label string, v any) {
	defer func() {
		fmt.Println(label+":", recover())
	}()

	_ = v.(string)
}

func hashKey(label string, k any) {
	defer func() {
		fmt.Println(label+":", recover())
	}()

	m := map[any]int{}
	m[k] = 1
}

func main() {
	pin("pin int32", int32(5))
	pin("pin rune", 'a')
	pin("pin uint8", uint8(1))
	pin("pin byte", byte(2))
	pin("pin int", 5)
	pin("pin uint", uint(5))
	pin("pin int64", int64(5))
	pin("pin uint32", uint32(5))
	pin("pin float64", 1.5)
	pin("pin string", "s")

	assertString("assert int32", int32(5))
	assertString("assert uint8", uint8(1))
	assertString("assert int", 5)

	hashKey("hash []int32", []int32{1})
	hashKey("hash []byte", []byte{1})
	hashKey("hash []int", []int{1})
}
