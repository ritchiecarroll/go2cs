// PromotedForwarderTypeNameCollision guards a method promoted from an embedded interface onto a struct that only
// satisfies the interface through its pointer, when the method shares its name with a TYPE of the same package
// (modernc.org/sqlite's tests: database/sql/driver's Connector.Driver beside type Driver, CS1929 in the TS session
// 2026-11-16). The converter renames the TYPE (ΔDriver) and keeps the METHOD's own name, so the promoted
// forwarder and the interface it forwards to must both spell the method Driver. Every way of reaching it must see
// it: the interface, a direct call, a method value, and reflect's method sets. The second arm, fcounting, is the TS
// probe's t3 repro (claude/ts-repros-1116): the embedded interface is FOREIGN (database/sql/driver.Connector), so
// the forwarder returns driver.Driver while this package's Driver type still takes the Δ (CS0102 before the fix).
package main

import (
	"context"
	"database/sql/driver"
	"fmt"
	"reflect"
)

// Driver is both a type and a method name in this package.
type Driver struct{ name string }

func (d *Driver) Open(string) (driver.Conn, error) { return nil, nil }

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

// fbase and fcounting are t3: fcounting overrides the foreign Connect through its pointer, so the converter
// forwards the other promoted method, Driver, whose name this package's type Driver also uses.
type fbase struct{}

func (fbase) Connect(context.Context) (driver.Conn, error) { return nil, nil }
func (fbase) Driver() driver.Driver                        { return &Driver{name: "foreign"} }

type fcounting struct {
	driver.Connector
	n int
}

func (c *fcounting) Connect(ctx context.Context) (driver.Conn, error) {
	c.n++
	return c.Connector.Connect(ctx)
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

	foreign()
}

func foreign() {
	fc := &fcounting{Connector: fbase{}}
	var k driver.Connector = fc
	k.Connect(context.Background())
	d, ok := k.Driver().(*Driver)
	fmt.Println("foreign:", ok, fc.n, d.name, fc.Driver() != nil)
}
