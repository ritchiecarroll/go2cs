package main

import (
	"fmt"

	"example.com/promolib"
)

// A struct satisfies ANOTHER package's interface only through methods promoted from that package's embedded
// struct (golang-jwt's shape: a test type embeds jwt.RegisteredClaims and is used as a jwt.Claims). The
// interface implementation has to forward each method through the embedded field, for the value and the
// pointer conversion alike.

type My struct {
	Extra string
	promolib.Base
}

func show(n promolib.Namer) {
	fmt.Println(n.Name(), n.Count())
}

func main() {
	show(My{"v", promolib.Base{N: 1}})
	show(&My{"p", promolib.Base{N: 2}})
}
