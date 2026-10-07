// Promoted method with a parameter named `target` guard (testify's suite.Suite promoting
// assert.Assertions' `ErrorAs(t, err, target, ...)`).
//
// go2cs-gen writes a forwarder for every method promoted through an embedded field, and named that
// forwarder's receiver `target`. A promoted method with its own `target` parameter then declared the
// name twice (CS0100), and inside the forwarder `target` could bind the parameter instead of the
// receiver. Every line below prints the RECEIVER's state beside the PARAMETER, so a forwarder that
// bound the wrong one prints the wrong value. The local Base covers forwarders for an embed in the
// same package; targetlib.Finder covers the path for an embed from another package.
package main

import (
	"fmt"

	"PromotedTargetParam/targetlib"
)

type Base struct {
	name string
}

func (b Base) Pair(target string) string {
	return b.name + "->" + target
}

func (b *Base) Rename(target string) {
	b.name = target
}

// Same-package embeds, by value and through a pointer.
type ByValue struct{ Base }

type ByPointer struct{ *Base }

// Embeds from another package, by value and through a pointer.
type ForeignByValue struct{ targetlib.Finder }

type ForeignByPointer struct{ *targetlib.Finder }

func main() {
	v := ByValue{Base{"value-recv"}}
	fmt.Println(v.Pair("param"))
	v.Rename("value-renamed")
	fmt.Println(v.Pair("param"))

	p := ByPointer{&Base{"pointer-recv"}}
	fmt.Println(p.Pair("param"))
	p.Rename("pointer-renamed")
	fmt.Println(p.Pair("param"))

	fv := ForeignByValue{targetlib.Finder{Name: "foreign-value"}}
	fmt.Println(fv.Match("param"))
	fv.Retarget("foreign-value-renamed")
	fmt.Println(fv.Match("param"))

	fp := ForeignByPointer{&targetlib.Finder{Name: "foreign-pointer"}}
	fmt.Println(fp.Match("param"))
	fp.Retarget("foreign-pointer-renamed")
	fmt.Println(fp.Match("param"))
}
