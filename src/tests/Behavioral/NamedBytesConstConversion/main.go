// A string CONSTANT converted to a NAMED []byte type: Go makes a fresh copy of the constant's bytes.
// tsc's internal/ipc writes json.Value("null"), the re-exported shape below.
package main

import (
	"fmt"

	"NamedBytesConstConversion/lib"
	"NamedBytesConstConversion/wrap"
)

type Value []byte

type Label string

const null = "null"

const typedLabel Label = "label"

var global = wrap.Value("global")

func show(name string, b []byte) { fmt.Printf("%s: %q len=%d\n", name, b, len(b)) }

func ret() wrap.Value { return wrap.Value("returned") }

func main() {
	show("local literal", Value("null"))
	show("local const", Value(null))
	show("local concat", Value("nu"+"ll"))
	show("local typed const", Value(typedLabel))
	show("local raw", Value(`raw`))
	show("local high byte", Value("\xff\x00"))

	show("imported literal", lib.Value("true"))
	show("re-exported literal", wrap.Value("null"))
	show("re-exported const", wrap.Value(null))
	show("package-level", global)
	show("returned", ret())

	// Each conversion is a fresh copy: writing one leaves the next conversion of the same constant intact.
	m := wrap.Value("abc")
	m[0] = 'X'
	n := wrap.Value("abc")
	show("mutated", m)
	show("fresh", n)

	lib.Print(wrap.Value("argument"))
	fmt.Println(wrap.Value("x").IsNull(), wrap.Value("null").IsNull(), wrap.Value(null).IsNull())

	// The control: a string VARIABLE converts through the general path.
	s := "variable"
	show("variable", wrap.Value(s))
}

func double(x int) int { return x * 2 }

// The SIBLING conversions through the same alias: a string to a re-exported named STRING, and a function
// value to a re-exported named FUNC type.
func init() {
	s := "variable"
	fmt.Println(wrap.Name("constant").Len(), wrap.Name(null).Len(), wrap.Name(s).Len())

	f := double
	fmt.Println(wrap.Handler(f).Run(3), wrap.Handler(double).Run(4), wrap.Handler(func(x int) int { return x + 1 }).Run(5), wrap.Handler(nil).Run(6))
}
