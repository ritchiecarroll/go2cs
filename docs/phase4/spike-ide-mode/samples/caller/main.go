package main

import (
	"fmt"
	"runtime"
)

func where() string {
	pc, file, line, ok := runtime.Caller(1)
	name := runtime.FuncForPC(pc).Name()
	return fmt.Sprintf("%s:%d %s %v", file, line, name, ok)
}

func main() {
	fmt.Println(where())

	outer := func() string {
		inner := func() string {
			return where()
		}
		return inner()
	}
	fmt.Println(outer())

	func() {
		fmt.Println(where())
	}()

	total := add(
		1,
		2,
	)
	fmt.Println(total, where())
}

func add(a, b int) int {
	return a + b
}
