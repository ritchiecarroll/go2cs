package main

import "fmt"

// The address of a variable holding a POINTER to an anonymous struct, `**struct{...}`, passed as an
// interface (mapstructure's tests: `data := &struct{...}{}` then `Decode(input, &data)`). go2cs leaves
// the anonymous struct of that type unresolved and emits it as raw Go; a single `*struct{...}` converts.
func decode(output interface{}) string {
	return fmt.Sprintf("%T", output)
}

func main() {
	data := &struct{ Foo string }{}
	fmt.Println(decode(&data))
	data.Foo = "baz"
	fmt.Println(data.Foo)
}
