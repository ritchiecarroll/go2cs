// StatementTableFuncNames pins runtime.FuncForPC(...).Name() for function values written INSIDE a
// multi-line Go statement -- a table of `{fn, label}` rows, the shape of go-cmp's TestNameOf. The
// GoPositionMap line table has one entry per STATEMENT, so every lambda in the table resolves to the
// statement's first line; the record still holds each literal and value-receiver method value on its
// own line, and the runtime reads each lambda's by its place in the statement. Go's spelling, from the
// gc toolchain:
//
//   - a function literal is `Outer.funcN`, counted from 1 in the enclosing function;
//   - a VALUE-receiver method value is its `-fm` wrapper, `main.T.valueMethod-fm`, whether the receiver
//     is a composite literal (`T{}.valueMethod`) or reached through a pointer (`(&T{}).valueMethod`);
//   - a POINTER-receiver method value (the control: a method group, never a lambda) is
//     `main.(*T).pointerMethod-fm`;
//   - a method value of a method PROMOTED from an embedded foreign type is named by the method it
//     promotes: `bytes.(*Buffer).Write-fm`.
//
// The table's enclosing function is //go:noinline, for the reason FuncLiteralCallerNames records: gc
// renames a literal whose enclosing function was inlined, and go2cs performs no inlining.
package main

import (
	"bytes"
	"fmt"
	"reflect"
	"runtime"
)

type T struct{ n int }

func (t T) valueMethod() int { return t.n }

func (t T) ValueMethod() int { return t.n + 1 }

func (t *T) pointerMethod() int { return t.n }

type W struct{ bytes.Buffer }

func nameOf(fn any) string {
	f := runtime.FuncForPC(reflect.ValueOf(fn).Pointer())
	if f == nil {
		return "<nil Func>"
	}
	return f.Name()
}

//go:noinline
func table() {
	rows := []struct {
		fn    any
		label string
	}{
		{func() {}, "literal in the table"},
		{T{}.valueMethod, "value method value, composite receiver"},
		{T{}.ValueMethod, "exported value method value, composite receiver"},
		{(&T{}).valueMethod, "value method value, pointer receiver"},
		{(&T{}).pointerMethod, "pointer method value (control)"},
		{(&W{}).Write, "promoted method value"},
		{func() int { return 2 }, "second literal in the table"},
	}

	for _, row := range rows {
		fmt.Printf("%-48s %s\n", row.label+":", nameOf(row.fn))
	}
}

func main() {
	table()
}
