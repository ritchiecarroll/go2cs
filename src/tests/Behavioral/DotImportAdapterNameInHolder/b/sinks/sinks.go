// Package sinks is the SECOND dot-imported package, with the same two adapters as the first, so a method holder
// sees `bytes_BufferжWriter` and `sinkжWriter` from both of them and from the main package.
package sinks

import (
	"bytes"
	"io"
)

type sink struct{ total int }

func (s *sink) Write(p []byte) (int, error) {
	s.total += 2 * len(p)
	return len(p), nil
}

func Wrap(buf *bytes.Buffer) io.Writer { return buf }

func Doubled() (io.Writer, func() int) {
	s := &sink{}
	return s, func() int { return s.total }
}
