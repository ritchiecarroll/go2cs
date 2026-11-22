// Package framelib is a library, so its methods are converted in the suffix form: each body is `Name⁔` in the
// package class, behind a Go-named forwarder. Each method reports the Go names of its own frames.
package framelib

import "runtime"

type Counter struct {
	n int
}

// Here names the function that called it.
func Here() string {
	pc, _, _, _ := runtime.Caller(1)
	return runtime.FuncForPC(pc).Name()
}

// Name is a value-receiver method that names its own frame.
func (c Counter) Name() string {
	return Here()
}

// Closure names a function literal inside a value-receiver method.
func (c Counter) Closure() string {
	f := func() string {
		return Here()
	}

	return f()
}

// Bump names a function literal inside a pointer-receiver method.
func (c *Counter) Bump() string {
	c.n++

	f := func() string {
		return Here()
	}

	return f()
}
