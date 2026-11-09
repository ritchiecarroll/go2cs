// Vendored-path module import guard: golang.org/x/text/transform is BOTH a module package (here a
// local replace) and a path the standard library vendors (GOROOT/src/vendor/golang.org/x/text/transform).
// Outside std, Go resolves the import to the module. The converter used to bind every importer to
// std's vendored copy (`vendor.golang.org.x.text.transform_package`), an assembly this program does
// not reference: CS0246 'vendor' on the using, and on every go2cs-gen file that inherits it.
package main

import (
	"fmt"
	"reflect"

	"golang.org/x/text/transform"
)

// upper implements transform.Transformer in this package, so go2cs-gen output names the module's types.
type upper struct{ transform.NopResetter }

func (upper) Transform(dst, src []byte, atEOF bool) (nDst, nSrc int, err error) {
	n := copy(dst, src)

	for i := 0; i < n; i++ {
		if c := dst[i]; 'a' <= c && c <= 'z' {
			dst[i] = c - 'a' + 'A'
		}
	}

	if n < len(src) {
		err = transform.ErrShortDst
	}

	return n, n, err
}

func main() {
	s, n, err := transform.String(transform.Nop, "abc")
	fmt.Println(s, n, err)

	s, n, err = transform.String(upper{}, "vendored path")
	fmt.Println(s, n, err)

	var t transform.Transformer = upper{}
	_, spanning := t.(transform.SpanningTransformer)
	fmt.Println(spanning)

	_, spanning = transform.Transformer(transform.Nop).(transform.SpanningTransformer)
	fmt.Println(spanning)

	fmt.Println(reflect.TypeOf(transform.NopResetter{}).PkgPath())
	fmt.Println(transform.ErrShortDst)
}
