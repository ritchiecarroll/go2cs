// t3: a struct embeds a FOREIGN interface (database/sql/driver.Connector) and overrides one of its
// methods with a pointer receiver, so go2cs emits a forwarder for the other promoted method, Driver().
// This package also declares a type named Driver AND a method named Driver, so "Driver" is in the
// package's type-vs-method collision set. Shape from modernc.org/sqlite connector_test.go.
package main

import (
	"context"
	"database/sql/driver"
	"fmt"
)

type Driver struct{}

func (d *Driver) Open(string) (driver.Conn, error) { return nil, nil }

type base struct{}

func (base) Connect(context.Context) (driver.Conn, error) { return nil, nil }
func (base) Driver() driver.Driver                        { return &Driver{} }

type counting struct {
	driver.Connector
	n int
}

func (c *counting) Connect(ctx context.Context) (driver.Conn, error) {
	c.n++
	return c.Connector.Connect(ctx)
}

func main() {
	c := &counting{Connector: base{}}
	var k driver.Connector = c
	k.Connect(context.Background())
	_, ok := k.Driver().(*Driver)
	fmt.Println(ok, c.n)
}
