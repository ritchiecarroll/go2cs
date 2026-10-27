package main

import "fmt"

// store mimics a non-variadic `any` sink (e.g. atomic.Value.Store, context.WithValue,
// sync.Map.Store): the value is boxed into an interface and later type-asserted, so an untyped
// int literal MUST box as Go's default type `int` (go2cs nint), not a bare C# System.Int32 — else
// the `.(int)` round-trip below panics ("interface conversion: interface {} is int32, not int").
func store(v any) any { return v }

func ret() any { return 42 }

type holder struct{ v any }

// classify distinguishes Go int from int32 through a type switch — a bare int literal boxed as the
// wrong width would report the wrong case.
func classify(v any) string {
	switch v.(type) {
	case int:
		return "int"
	case int32:
		return "int32"
	default:
		return "other"
	}
}

func main() {
	// 1. non-variadic call argument + assertion round-trip (the reported atomic.Value.Store case)
	fmt.Println(store(42).(int))

	// 2. var-spec declaration
	var a any = 7
	fmt.Println(a.(int))

	// 3. reassignment to an existing any
	var b any
	b = 8
	fmt.Println(b.(int))

	// 4. return of an untyped int as any
	fmt.Println(ret().(int))

	// 5. slice element
	s := []any{5}
	fmt.Println(s[0].(int))

	// 6. map value
	m := map[string]any{"k": 9}
	fmt.Println(m["k"].(int))

	// 7. keyed struct field
	h := holder{v: 3}
	fmt.Println(h.v.(int))

	// 8. negative and arithmetic constants
	fmt.Println(store(-5).(int))
	fmt.Println(store(1 + 2).(int))

	// 9. type switch distinguishes int from int32
	fmt.Println(classify(1), classify(int32(1)))

	// 10. comma-ok assertion succeeds for a boxed int
	if n, ok := store(11).(int); ok {
		fmt.Println("ok", n)
	}
}
