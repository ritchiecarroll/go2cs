// Package sinks is the FIRST dot-imported package. It adapts *bytes.Buffer to io.Writer, so its package class
// declares the adapter `bytes_BufferжWriter`, and its own unexported sink to io.Writer, so it declares `sinkжWriter`.
package sinks

import (
	"bytes"
	"io"
)

type sink struct{ n int }

func (s *sink) Write(p []byte) (int, error) {
	s.n += len(p)
	return len(p), nil
}

func NewWriter() (io.Writer, *bytes.Buffer) {
	buf := &bytes.Buffer{}
	return buf, buf
}

func Counted() (io.Writer, func() int) {
	s := &sink{}
	return s, func() int { return s.n }
}
