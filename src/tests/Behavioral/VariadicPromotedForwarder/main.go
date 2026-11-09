// VariadicPromotedForwarder guards a VARIADIC method promoted from an embedded interface onto a struct that only
// satisfies the interface through its pointer (catalogue row C3 of EXPERIMENT-ts-readiness-2026-11-07.md). The
// pointer-receiver override of Name shadows it out of the value method set, so *cached implements Host and cached
// does not; Trace stays promoted from the field into BOTH method sets. Every way of reaching Trace must see it:
// the interface, a direct call, a method value, and reflect's method sets.
package main

import (
	"fmt"
	"reflect"

	host "go/varhost"
)

type cached struct {
	host.Host
	hits int
}

func (c *cached) Name() string { c.hits++; return fmt.Sprintf("cached(%d):%s", c.hits, c.Host.Name()) }

func main() {
	c := &cached{Host: &host.Base{Tag: "t"}}
	fmt.Println(host.Use(c))
	fmt.Println(c.Trace("direct %v", true))

	trace := c.Trace
	fmt.Println(trace("method value %d", 7))
	fmt.Println(trace("no args"))

	var h host.Host = c
	_, valueIsHost := any(*c).(host.Host)
	fmt.Println(h.Name(), valueIsHost)

	pt, vt := reflect.TypeOf(c), reflect.TypeOf(*c)
	fmt.Println("methods:", pt.NumMethod(), vt.NumMethod())
	for i := range pt.NumMethod() {
		fmt.Println(" *cached:", pt.Method(i).Name)
	}
	for i := range vt.NumMethod() {
		fmt.Println("  cached:", vt.Method(i).Name)
	}
	if m, ok := pt.MethodByName("Trace"); ok {
		out := m.Func.Call([]reflect.Value{reflect.ValueOf(c), reflect.ValueOf("via %s"), reflect.ValueOf("reflect")})
		fmt.Println(out[0].String())
	} else {
		fmt.Println("MethodByName(Trace) missing")
	}
}
