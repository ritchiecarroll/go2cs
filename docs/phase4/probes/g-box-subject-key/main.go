package main

import (
	"errors"
	"fmt"
	"io/fs"
)

type localErr struct{ msg string }

func (e *localErr) Error() string { return "local: " + e.msg }

func main() {
	var e error = &fs.PathError{Op: "read", Path: "|0", Err: errors.New("i/o timeout")}
	fmt.Printf("A %%v of a foreign *PathError as error: %v\n", e)
	fmt.Println("B direct Error():", e.Error())

	var a any = e
	_, ok := a.(error)
	fmt.Println("C any(error) holding *PathError asserts to error:", ok)

	var raw any = &fs.PathError{Op: "open", Path: "x", Err: errors.New("boom")}
	_, ok = raw.(error)
	fmt.Println("D any holding a raw *PathError asserts to error:", ok)
	fmt.Printf("E %%v of the raw one: %v\n", raw)

	var l error = &localErr{msg: "same assembly"}
	fmt.Printf("F %%v of a same-package *localErr: %v\n", l)
	var rawl any = &localErr{msg: "raw"}
	_, ok = rawl.(error)
	fmt.Println("G any holding a raw *localErr asserts to error:", ok)

	var s fmt.Stringer
	_, ok = any(e).(fmt.Stringer)
	fmt.Println("H *PathError asserts to Stringer (want false):", ok, s == nil)
}
