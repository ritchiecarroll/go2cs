package main

import (
	"fmt"
	"strings"
)

func header(w func(format string, args ...interface{}), n int) { w("n=%d %s", n, "x") }

type logFunc func(format string, args ...any)

func headerNamed(w logFunc, n int) { w("named n=%d", n) }

type sink struct {
	write func(format string, args ...any)
}

func sum(add func(xs ...int) int) int { return add(1, 2, 3) }

// TS dep: gotest.tools/v3/internal/format.UnifiedDiff — a variadic closure bound by := and passed
// as a func-typed argument.
func main() {
	out := ""
	writeFormat := func(format string, args ...interface{}) { out += fmt.Sprintf(format, args...) + ";" }
	header(writeFormat, 2)
	headerNamed(writeFormat, 3)
	s := sink{write: writeFormat}
	s.write("field %s", "ok")
	writeFormat("direct %d %d", 4, 5)
	fmt.Println(out)

	// The closure reassigned to another literal of the same type.
	var b strings.Builder
	emit := func(format string, args ...any) { fmt.Fprintf(&b, "["+format+"]", args...) }
	header(emit, 6)
	emit = func(format string, args ...any) { fmt.Fprintf(&b, "<"+format+">", args...) }
	header(emit, 7)
	fmt.Println(b.String())

	// A variadic closure with a result and a non-string element.
	total := 0
	add := func(xs ...int) int {
		for _, x := range xs {
			total += x
		}
		return total
	}
	fmt.Println(sum(add), add(10), total)
}
