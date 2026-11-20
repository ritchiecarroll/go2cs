// AnonIfaceEmbedCrossPackage guards ANOTHER package's anonymous interface that embeds named interfaces, used as a
// result and a parameter (goldmark's interface{ renderer.Option; html.Option }, board row ii). The importer must name
// the producer's published lift, and a lift that inherits a named interface needs no adapter to reach it.
package main

import (
	"fmt"

	lib "go/anonifaceembed"
)

type mine struct{}

func (mine) Apply() int   { return 5 }
func (mine) Name() string { return "mine" }

func useOption(o lib.Option) int { return o.Apply() }

func useOther(o lib.Other) string { return o.Name() }

func main() {
	fmt.Println(useOption(lib.With(3)))
	fmt.Println(useOther(lib.With(3)))
	fmt.Println(useOption(lib.WithMixed(4)))
	fmt.Println(lib.WithMixed(4).Extra())
	fmt.Println(lib.Take(mine{}))
	fmt.Println(lib.Take(lib.With(6)))

	w := lib.With(7)
	var o lib.Option = w
	fmt.Println(o.Apply(), w.Name())
}
