package pos

import (
	"reflect"
	"unsafe"
)

var __ccgo_ts1 = "a\x00bc\x00"
var __ccgo_ts = (*reflect.StringHeader)(unsafe.Pointer(&__ccgo_ts1)).Data

type TLS struct{}

func Xmemcpy(t *TLS, d, s uintptr, n uint64) uintptr { return d }

func f(tls *TLS, other uintptr) {
	_ = __ccgo_ts == other            // compare
	_ = other - __ccgo_ts             // subtract
	_ = unsafe.StringData(__ccgo_ts1) // ts1use
	p := __ccgo_ts + 2
	q := p + 1
	*(*byte)(unsafe.Pointer(q)) = 'x'         // store via a 2-step taint
	*(*byte)(unsafe.Pointer(__ccgo_ts + 1))++ // store via incdec
	Xmemcpy(tls, __ccgo_ts+0, other, 1)       // memwrite (dest)
	g(p)                                      // escape
	_ = __ccgo_ts + uintptr(len(__ccgo_ts1))  // plusExpr and ts1use
}

func g(uintptr) {}
