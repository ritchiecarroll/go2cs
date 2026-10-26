// GenericLiftedInterfaceField guards an anonymous interface FIELD of a GENERIC struct -- Go 1.26
// crypto/x509's `type constraints[T, V any] struct { permitted, excluded interface{ query(V) (T, bool) } }`
// filled from functions returning `interface{ query(string) (string, bool) }`.
//
// The field's lift must carry the struct's type parameters (constraints_permitted<T, V>), and the
// function results -- Go's SAME type at each instantiation -- must convert to the field: every value
// below is made through the concrete function result and used through the generic field, and both
// calls must reach the one method body. Go's structural assertion on the field value must hold too.
package main

import "fmt"

type constraints[T any, V any] struct {
	kind      string
	permitted interface{ query(V) (T, bool) }
	excluded  interface{ query(V) (T, bool) }
}

// A value-receiver implementation over string, as crypto/x509's dnsConstraints.
type dnsC struct{ allow string }

func (d dnsC) query(s string) (string, bool) { return "dns:" + d.allow, s == d.allow }

// A pointer-receiver implementation with state, so one body is visibly shared by both call paths.
type counter struct{ n int }

func (c *counter) query(v int) (*counter, bool) {
	c.n++
	return c, v == c.n
}

func newDNS(allow string) interface{ query(string) (string, bool) } { return dnsC{allow} }

func newCounter() interface{ query(int) (*counter, bool) } { return &counter{} }

func check[T any, V any](c constraints[T, V], s V) (T, bool) {
	if c.excluded != nil {
		if t, found := c.excluded.query(s); found {
			var zero T
			_ = t
			return zero, false
		}
	}

	if c.permitted != nil {
		if t, found := c.permitted.query(s); found {
			return t, true
		}
	}

	var zero T
	return zero, false
}

func main() {
	// Made as the concrete lift, held by the generic struct.
	d := constraints[string, string]{"dns", newDNS("a"), newDNS("x")}
	fmt.Println(check(d, "a"))
	fmt.Println(check(d, "b"))
	fmt.Println(check(d, "x"))

	// One value, called through the concrete interface and through the generic field.
	q := newDNS("z")
	d.permitted = q
	t1, ok1 := q.query("z")
	t2, ok2 := d.permitted.query("z")
	fmt.Println(t1, ok1, t2, ok2)

	// Go's structural assertion on the field's value holds.
	var x any = d.permitted
	_, isShape := x.(interface{ query(string) (string, bool) })
	fmt.Println("assert:", isShape)

	// A second instantiation, pointer receiver: the same counter advances through both paths.
	c := newCounter()
	k := constraints[*counter, int]{"count", c, nil}
	r, ok := check(k, 1)
	fmt.Println(r.n, ok)
	_, _ = c.query(0)
	r, ok = check(k, 3)
	fmt.Println(r.n, ok)
}
