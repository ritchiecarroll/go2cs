// An exported func whose parameter is an ANONYMOUS interface: the lifted interface is public, as the
// method taking it is (CS0051 when the lift stayed internal). An unexported func's lift stays internal.

package main

import "fmt"

type named struct{ n string }

func (v named) Name() string  { return "name:" + v.n }
func (v named) Label() string { return "label:" + v.n }

// Report takes an anonymous interface, the go-cmp `Reporter(r interface{...})` shape.
func Report(r interface {
	Name() string
	Label() string
}) string {
	return r.Name() + "," + r.Label()
}

func report(r interface{ Label() string }) string { return r.Label() }

func main() {
	fmt.Println(Report(named{"a"}))
	fmt.Println(report(named{"b"}))
}
