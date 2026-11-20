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

// local has its own unexported run, so interface{ run() int } in this package is not the library's type of the same
// spelling.
type local struct{ n int }

func (l local) run() int { return l.n + 1 }

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

	fmt.Println(useRun(local{8}))
	var rr runs = mkRun(9)
	fmt.Println(rr.run())
	_ = lib.Runner(9)
}

// useRun takes this package's own interface{ run() int }.
func useRun(r interface{ run() int }) int { return r.run() }

// runs is a named interface with the same unexported method.
type runs interface{ run() int }

// mkRun is declared below main, so main's conversion of its result to runs names the result type before this file
// lifts it, and the library's lift of the same spelling must not answer.
func mkRun(n int) interface{ run() int } { return local{n} }
