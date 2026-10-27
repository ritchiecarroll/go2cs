package main

import "fmt"

// A type switch matches the dynamic type EXACTLY: `case int:` does not match an int32 or an int64,
// and `case uint:` does not match a uint32 or a uint64, whatever the platform's word size. An
// untyped integer constant converted to an interface takes its default type, int.

func whatAmI(i any) string {
	switch i.(type) {
	case int:
		return "I'm an int"
	case uint:
		return "I'm a uint"
	default:
		return fmt.Sprintf("Don't know type %T", i)
	}
}

const c = 5

func main() {
	fmt.Println("int(5):", whatAmI(int(5)))
	fmt.Println("int32(5):", whatAmI(int32(5)))
	fmt.Println("int64(5):", whatAmI(int64(5)))
	fmt.Println("uint(5):", whatAmI(uint(5)))
	fmt.Println("uint32(5):", whatAmI(uint32(5)))
	fmt.Println("uint64(5):", whatAmI(uint64(5)))
	fmt.Println("rune(5):", whatAmI(rune(5)))
	fmt.Println("untyped 5:", whatAmI(5))
	fmt.Println("untyped const:", whatAmI(c))
	x := 5
	fmt.Println("inferred var:", whatAmI(x))
	fmt.Println("len:", whatAmI(len("abc")))
}
