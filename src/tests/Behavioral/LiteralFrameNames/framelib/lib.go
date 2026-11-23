// Package framelib holds two types whose same-named methods each declare a function literal. In the converted
// package the two method bodies share one package class, so only the literal's own record can say which receiver
// it belongs to.
package framelib

import "runtime"

type counter struct{ n int }

type gauge struct{ v int }

//go:noinline
func (c *counter) Name() string {
	f := func() string {
		c.n++
		return here()
	}
	return f()
}

//go:noinline
func (g gauge) Name() string {
	f := func() string {
		g.v++
		return here()
	}
	return f()
}

// CounterName returns the name of the literal in (*counter).Name.
func CounterName() string { return (&counter{}).Name() }

// GaugeName returns the name of the literal in gauge.Name.
func GaugeName() string { return gauge{}.Name() }

func here() string {
	pc, _, _, _ := runtime.Caller(1)
	return runtime.FuncForPC(pc).Name()
}
