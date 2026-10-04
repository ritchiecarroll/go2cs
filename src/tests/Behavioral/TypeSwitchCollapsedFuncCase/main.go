package main

import (
	"errors"
	"fmt"
	"reflect"
)

// Methodless named func types: go2cs renders each as its base delegate and declares no type for
// it, so a type-switch CASE naming one must spell the delegate, as a type assertion already does.
// mapstructure's DecodeHookExec dispatches on exactly this shape (DecodeHookFuncType / Kind / Value).
type HookType func(reflect.Type, reflect.Type, interface{}) (interface{}, error)
type HookKind func(reflect.Kind, reflect.Kind, interface{}) (interface{}, error)
type HookValue func(from reflect.Value, to reflect.Value) (interface{}, error)

func exec(raw interface{}, from, to reflect.Value) (interface{}, error) {
	switch f := raw.(type) {
	case HookType:
		return f(from.Type(), to.Type(), from.Interface())
	case HookKind:
		return f(from.Kind(), to.Kind(), from.Interface())
	case HookValue:
		return f(from, to)
	case func(int) int:
		return f(from.Interface().(int)), nil
	default:
		return nil, errors.New("invalid decode hook signature")
	}
}

// A case WITHOUT a binding, and one listing two collapsed types.
func kind(raw interface{}) string {
	switch raw.(type) {
	case HookType:
		return "type"
	case HookKind, HookValue:
		return "kind or value"
	}
	return "other"
}

func main() {
	from, to := reflect.ValueOf(21), reflect.ValueOf("")
	hooks := []interface{}{
		HookType(func(f, t reflect.Type, data interface{}) (interface{}, error) {
			return fmt.Sprintf("%s->%s:%v", f, t, data), nil
		}),
		HookKind(func(f, t reflect.Kind, data interface{}) (interface{}, error) {
			return fmt.Sprintf("%s->%s:%v", f, t, data), nil
		}),
		HookValue(func(f, t reflect.Value) (interface{}, error) {
			return f.Int() * 2, nil
		}),
		func(x int) int { return x + 1 },
		42,
	}
	for _, h := range hooks {
		out, err := exec(h, from, to)
		fmt.Println(out, err, kind(h))
	}

	// The assertion path, which already spells the delegate.
	if f, ok := hooks[2].(HookValue); ok {
		out, _ := f(reflect.ValueOf(5), to)
		fmt.Println("asserted", out)
	}
}
