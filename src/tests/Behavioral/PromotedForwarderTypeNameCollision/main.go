// PromotedForwarderTypeNameCollision guards a method promoted from an embedded interface onto a struct that only
// satisfies the interface through its pointer, when the method shares its name with a TYPE of the same package
// (modernc.org/sqlite's tests: database/sql/driver's Connector.Driver beside type Driver, CS1929 in the TS session
// 2026-11-16). The converter renames the TYPE (ΔDriver) and keeps the METHOD's own name, so the promoted
// forwarder and the interface it forwards to must both spell the method Driver. Every way of reaching it must see
// it: the interface, a direct call, a method value, and reflect's method sets.
package main

import (
	"fmt"
	"reflect"
)

// Driver is both a type and a method name in this package.
type Driver struct{ name string }

type Connector interface {
	Driver() Driver
	Name() string
}

type base struct{ tag string }

func (b *base) Driver() Driver { return Driver{name: "driver:" + b.tag} }
func (b *base) Name() string   { return "base:" + b.tag }

// counting overrides Name through its pointer, so *counting implements Connector and counting does not; Driver
// stays promoted from the embedded interface into both method sets.
type counting struct {
	Connector
	hits int
}

func (c *counting) Name() string {
	c.hits++
	return fmt.Sprintf("counting(%d):%s", c.hits, c.Connector.Name())
}

func use(c Connector) string { return c.Name() + " / " + c.Driver().name }

func main() {
	c := &counting{Connector: &base{tag: "t"}}
	fmt.Println(use(c))
	fmt.Println(c.Driver().name)

	driver := c.Driver
	fmt.Println(driver().name)

	_, valueIsConnector := any(*c).(Connector)
	fmt.Println(valueIsConnector)

	pt, vt := reflect.TypeOf(c), reflect.TypeOf(*c)
	fmt.Println("methods:", pt.NumMethod(), vt.NumMethod())
	for i := range pt.NumMethod() {
		fmt.Println(" *counting:", pt.Method(i).Name)
	}
	for i := range vt.NumMethod() {
		fmt.Println("  counting:", vt.Method(i).Name)
	}
}
