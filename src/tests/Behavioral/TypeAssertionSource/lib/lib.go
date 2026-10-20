// Package lib declares an interface that embeds two others and hands out a value of it.
package lib

import "io"

// RW embeds two interfaces, so a call on a value of this type has two embedded parents whose
// assertion overloads could compete if RW's own were out of reach of the asserting file.
type RW interface {
	io.Reader
	io.Writer
}

type Impl struct{}

func (*Impl) Read(p []byte) (int, error)  { return 0, io.EOF }
func (*Impl) Write(p []byte) (int, error) { return len(p), nil }
