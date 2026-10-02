package main

import "fmt"

// A local closure with a variadic parameter named like a parameter of the enclosing function. The
// closure's own `args` shadows the outer one, so each reads its own: the closure body forwards its
// variadic `args...`, and the enclosing body reads the outer slice before and after the call.

func add(verb string, args []string) {
	errorf := func(format string, args ...any) {
		fmt.Println(fmt.Sprintf(format, args...))
	}

	if len(args) != 1 {
		errorf("%s: want 1 argument, got %d", verb, len(args))
		return
	}

	errorf("%s: %s", verb, args[0])
}

func main() {
	add("go", []string{"1.23.0"})
	add("go", nil)
}
