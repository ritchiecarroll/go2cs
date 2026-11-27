// Program ForeignInterfaceFieldPromotion is the near miss of go2cs's GO2CS2016 refusal: a struct
// meets another package's interface's UNEXPORTED method through an embedded interface FIELD, not a
// struct. The adapter forwards to the field, whose method is the dynamic value's own, so lib.m runs,
// exactly as in Go, and the conversion is not refused. D meets it one level further down, through
// its own W, and its value implementation forwards through W rather than stubbing the member.
package main

import (
	"fmt"

	"ForeignInterfaceFieldPromotion/lib"
)

// S embeds lib.I as an interface field.
type S struct {
	lib.I
	label string
}

// W embeds lib.I as an interface field, and D embeds W: D meets lib.I's m one level further down.
type W struct{ lib.I }

type D struct{ W }

// R embeds lib.Sentinel, the crypto/internal/rand reader shape.
type R struct {
	lib.Sentinel
}

func main() {
	s := S{I: lib.New("s"), label: "own"}
	fmt.Println(lib.CallM(s), s.Name(), s.label)

	var p lib.I = &S{I: lib.New("p")}
	fmt.Println(lib.CallM(p), p.Name())

	fmt.Println(lib.CallM(D{W{lib.New("d")}}))
	fmt.Println(lib.CallM(&D{W{lib.New("pd")}}))

	var sentinel lib.Sentinel = R{lib.Mark}
	fmt.Println(sentinel != nil, lib.IsMarked(sentinel))
	fmt.Println(lib.IsMarked(lib.Mark))
}
