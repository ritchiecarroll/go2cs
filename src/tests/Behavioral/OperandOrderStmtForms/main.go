// Go's order in the statement forms with a prelude of their own: a local var declaration, the
// arguments of a defer and a go statement, and a switch tag. gc reads each plain operand after the
// statement's calls, and bump writes the package var g each time it runs.
package main

import "fmt"

var g int

func bump() int { g++; return g * 10 }

func show(a, b int) { fmt.Println("defer arguments:", a, b) }

func pick(a, b int) int { return a*100 + b }

func declCase() {
	var v = pick(g, bump())
	fmt.Println("var with a call:", v)

	var w, z = g, bump()
	fmt.Println("var pair:", w, z)
}

func deferCase() {
	defer show(g, bump())
	fmt.Println("defer registered at:", g)
}

func goCase() {
	done := make(chan bool)

	go func(a, b int) {
		fmt.Println("go arguments:", a, b)
		done <- true
	}(g, bump())

	<-done
}

func switchCase() {
	switch g + bump() {
	case 44:
		fmt.Println("switch tag: read before the call")
	case 55:
		fmt.Println("switch tag: read after the call")
	default:
		fmt.Println("switch tag: other", g)
	}
}

func main() {
	declCase()
	deferCase()
	goCase()
	switchCase()
}
