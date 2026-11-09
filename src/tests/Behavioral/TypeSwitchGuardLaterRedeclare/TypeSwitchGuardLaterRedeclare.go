// TypeSwitchGuardLaterRedeclare guards a type-switch binding whose name the ENCLOSING block declares again AFTER the
// switch (catalogue row C15 of EXPERIMENT-ts-readiness-2026-11-07.md; gotest.tools/v3 assert/cmp.ErrorType). Go
// scopes the later declaration from its own position, C# scopes a local over its whole block, so the case bindings
// must not keep the later declaration's name.
package main

import (
	"errors"
	"fmt"
	"reflect"
)

// kind: the later short declaration sits in the function body.
func kind(err error, expected any) string {
	switch expectedType := expected.(type) {
	case func(error) bool:
		return fmt.Sprint("func:", expectedType(err))
	case reflect.Type:
		return "type:" + expectedType.Kind().String()
	case nil:
		return "nil"
	}

	expectedType := reflect.TypeOf(expected)
	return "after:" + expectedType.Kind().String()
}

type Result string

// errorType: the ErrorType shape, inside a returned closure.
func errorType(err error, expected any) func() Result {
	return func() Result {
		switch expectedType := expected.(type) {
		case func(error) bool:
			return Result(fmt.Sprint("func:", expectedType(err)))
		case reflect.Type:
			return Result("type:" + expectedType.Kind().String())
		}

		expectedType := reflect.TypeOf(expected)
		return Result("after:" + expectedType.Kind().String())
	}
}

// describe: multi-type and default bindings, a later `var` declaration, and the switch inside a nested block.
func describe(v any, nested bool) string {
	if nested {
		switch x := v.(type) {
		case int, int64:
			return fmt.Sprintf("nested-num:%v", x)
		default:
			_ = x
		}

		var x = fmt.Sprintf("nested-after:%T", v)
		return x
	}

	switch x := v.(type) {
	case string:
		return "str:" + x
	default:
		return fmt.Sprintf("default:%v", x)
	case bool:
	}

	var x string = "bool-after"
	return x
}

func main() {
	e := errors.New("x")
	fmt.Println(kind(e, func(error) bool { return true }))
	fmt.Println(kind(e, reflect.TypeOf(1)))
	fmt.Println(kind(e, nil))
	fmt.Println(kind(e, struct{}{}))

	fmt.Println(errorType(e, func(err error) bool { return err == e })())
	fmt.Println(errorType(e, reflect.TypeOf(""))())
	fmt.Println(errorType(e, 2.5)())

	fmt.Println(describe(7, true))
	fmt.Println(describe("s", true))
	fmt.Println(describe("s", false))
	fmt.Println(describe(1.5, false))
	fmt.Println(describe(true, false))
}
