// An untyped constant appended to an []any boxes at Go's default type: `append(path, 0)` stores an
// int, not a 32-bit integer. gojq's --stream encoder rejected the C# box with "invalid type: int32 (0)".
package main

import "fmt"

const named = 7
const ratio = 0.5
const letter = 'z'
const word = "w"

func kind(v any) string {
	switch v.(type) {
	case int:
		return "int"
	case int32:
		return "int32 (rune)"
	case float64:
		return "float64"
	case string:
		return "string"
	}
	return fmt.Sprintf("other %T", v)
}

func main() {
	path := []any{"a"}
	path = append(path, 0)
	path = append(path, 1<<3, -5, 'r', 2.5, "s")
	path = append(path, named, named+1, ratio, letter, word, word+"x")

	for i, v := range path {
		fmt.Printf("%d: %T %v %s\n", i, v, v, kind(v))
	}

	fmt.Println(path[1] == any(0), path[2] == any(8))
}
