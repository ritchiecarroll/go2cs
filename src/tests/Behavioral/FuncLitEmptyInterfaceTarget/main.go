package main

import (
	"errors"
	"fmt"
)

// Hook is a NAMED empty interface, the shape of mapstructure's DecodeHookFunc.
type Hook interface{}

type holder struct{ h interface{} }

// A func literal stored into an empty interface has no delegate target in C#, so its delegate
// type must come from the literal's declared Go signature at EVERY position Go allows it: return,
// declared variable, assignment, argument, conversion, composite element (keyed and unkeyed),
// struct field and channel send. Two literal shapes defeat C# inference: a multi-result literal
// whose every arm carries an untyped nil (no arm has a natural type), and a single result whose
// only arm is an untyped constant (C# infers int where Go declared int64).

func scaled(n int) func(int) (interface{}, error) {
	return func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * n, nil
	}
}

func retMulti() interface{} {
	return func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 2, nil
	}
}

func retNamed() Hook {
	return func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 3, nil
	}
}

func retConst() interface{} { return func() int64 { return 1 } }

func take(h interface{}) interface{} { return h }

func call(h interface{}) string {
	switch f := h.(type) {
	case func(int) (interface{}, error):
		out, err := f(5)
		_, negErr := f(-1)
		return fmt.Sprint(out, " ", err, " ", negErr)
	case func() int64:
		return fmt.Sprint(f())
	}
	return "WRONG DYNAMIC TYPE"
}

func main() {
	var declared interface{} = func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 4, nil
	}
	var assigned interface{}
	assigned = func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 5, nil
	}
	argument := take(func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 6, nil
	})
	keyed := holder{h: func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 7, nil
	}}
	elements := []interface{}{func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 8, nil
	}, func() int64 { return 2 }}
	converted := interface{}(func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 9, nil
	})
	namedConverted := Hook(func() int64 { return 3 })
	ch := make(chan interface{}, 1)
	ch <- func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 10, nil
	}
	sent := <-ch
	positional := holder{func() int64 { return 4 }}
	var first, second interface{} = func() int64 { return 5 }, func(x int) (interface{}, error) {
		if x < 0 {
			return nil, errors.New("negative")
		}
		return x * 11, nil
	}
	var declaredConst interface{} = func() int64 { return 6 }
	byName := map[string]interface{}{"seven": func() int64 { return 7 }}
	var named interface{} = scaled(12)

	all := []interface{}{retMulti(), retNamed(), retConst(), declared, assigned, argument, keyed.h,
		elements[0], elements[1], converted, namedConverted, sent, positional.h, first, second,
		declaredConst, byName["seven"], named}

	for i, f := range all {
		fmt.Printf("%2d: %s\n", i, call(f))
	}
}
