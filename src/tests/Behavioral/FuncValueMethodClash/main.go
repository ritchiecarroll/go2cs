// FuncValueMethodClash guards a package-level func used as a VALUE whose name equals a method declared in
// the same package. Go methods are extension methods in the package class, so `Run` (func) and `T.Run`
// (method) are one C# method group; C# can pick the func only when the site supplies a delegate type.
// The forms below supply none: `:=`, `var x =`, an `any` argument, a blank assignment, and the other
// interface slots (a typed `any` var, an assignment, a composite element, a return). Each is exercised
// for a same-package clash and for an imported one (time.After against the exported Time.After). The
// control at the end has a func-typed target and must stay byte-identical.
package main

import (
	"fmt"
	"reflect"
	"time"
)

type T struct{}

func (T) Run() string { return "method" }

func Run() string { return "func" }

func use(f func() string) string { return f() }

func show(v any) string {
	t := reflect.TypeOf(v)
	return fmt.Sprintf("%s in=%d out=%d", t.Kind(), t.NumIn(), t.NumOut())
}

func retAny() any { return Run }

func main() {
	// same package
	a := Run
	var b = Run
	fmt.Println("same := ", a(), "| var = ", b())
	fmt.Println("same any:", show(Run))
	_ = Run

	// imported: time.After against the method Time.After
	c := time.After
	var d = time.After
	fmt.Println("imported := ", show(c), "| var = ", show(d))
	fmt.Println("imported any:", show(time.After))
	_ = time.After

	// the other interface slots: a typed var, an assignment, a composite element, a return
	var f any = Run
	var h any
	h = time.After
	g := []any{time.After, Run}
	fmt.Println("slots:", show(f), "|", show(h), "|", show(g[0]), "|", show(g[1]), "|", show(retAny()))

	// control: a func-typed target already selects the func
	var e func() string = Run
	fmt.Println("control:", e(), use(Run), T{}.Run())
}
