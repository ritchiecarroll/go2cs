package main

import "fmt"

// A function literal whose whole body is a call to panic has no return statement, so its
// result list can only come from the declared signature. Bound to a variable first and passed
// on afterwards, it must still be a func() (interface{}, error), not an Action.

func call(fn func() (interface{}, error)) (v interface{}, err error) {
	defer func() {
		if r := recover(); r != nil {
			err = fmt.Errorf("recovered: %v", r)
		}
	}()

	return fn()
}

func main() {
	fn := func() (interface{}, error) {
		panic("boom")
	}

	v, err := call(fn)
	fmt.Println(v, err)
}
