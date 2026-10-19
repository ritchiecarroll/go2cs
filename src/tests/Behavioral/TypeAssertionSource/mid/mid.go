// Package mid returns lib.RW values to a caller that never imports lib.
package mid

import (
	"io"

	"TypeAssertionSource/lib"
)

// Get returns a lib.RW whose dynamic type is *lib.Impl.
func Get() lib.RW { return &lib.Impl{} }

// Other is a different RW implementation, for a failing assertion.
type Other struct{}

func (*Other) Read(p []byte) (int, error)  { return 0, io.EOF }
func (*Other) Write(p []byte) (int, error) { return len(p), nil }
