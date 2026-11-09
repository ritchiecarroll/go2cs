// Package transform is a small local stand-in for golang.org/x/text/transform. The behavioral test
// needs a MODULE package at an import path the standard library also vendors
// (GOROOT/src/vendor/golang.org/x/text/transform), and a replace directive gives it one without a
// network fetch. The API mirrors the real package's names and shapes; the bodies are minimal.
package transform

import "errors"

// ErrShortDst means that dst was too short to receive all transformed bytes.
var ErrShortDst = errors.New("transform: short destination buffer")

// Transformer transforms bytes.
type Transformer interface {
	Transform(dst, src []byte, atEOF bool) (nDst, nSrc int, err error)
	Reset()
}

// SpanningTransformer extends the Transformer interface with a Span method.
type SpanningTransformer interface {
	Transformer
	Span(src []byte, atEOF bool) (n int, err error)
}

// NopResetter can be embedded by implementations of Transformer to add a nop Reset method.
type NopResetter struct{}

// Reset implements the Reset method of the Transformer interface.
func (NopResetter) Reset() {}

type nop struct{ NopResetter }

func (nop) Transform(dst, src []byte, atEOF bool) (nDst, nSrc int, err error) {
	n := copy(dst, src)

	if n < len(src) {
		err = ErrShortDst
	}

	return n, n, err
}

func (nop) Span(src []byte, atEOF bool) (n int, err error) {
	return len(src), nil
}

// Nop is a SpanningTransformer that copies src to dst.
var Nop SpanningTransformer = nop{}

// String returns a string with the result of converting s[:n] using t, where n <= len(s).
func String(t Transformer, s string) (result string, n int, err error) {
	t.Reset()

	src := []byte(s)
	dst := make([]byte, 0, len(src))
	buf := make([]byte, len(src)+1)

	for len(src) > 0 {
		nDst, nSrc, err := t.Transform(buf, src, true)
		dst = append(dst, buf[:nDst]...)
		n += nSrc
		src = src[nSrc:]

		if err == ErrShortDst {
			buf = make([]byte, 2*len(buf))
			continue
		}

		if err != nil {
			return string(dst), n, err
		}

		if nSrc == 0 && nDst == 0 {
			break
		}
	}

	return string(dst), n, nil
}
