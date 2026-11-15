package neg

import (
	"reflect"
	"unsafe"
)

var __ccgo_ts1 = "a\x00bc\x00"
var __ccgo_ts = (*reflect.StringHeader)(unsafe.Pointer(&__ccgo_ts1)).Data

type TLS struct{}

func Xmemcpy(t *TLS, d, s uintptr, n uint64) uintptr { return d }
func Xprintf(t *TLS, f uintptr, v uintptr) int32     { return 0 }

func f(tls *TLS, buf uintptr) {
	x := __ccgo_ts                        // value
	Xprintf(tls, __ccgo_ts+2, 0)          // plusN, escape (a read)
	Xmemcpy(tls, buf, __ccgo_ts+(1+1), 1) // plusN as SOURCE: not a memwrite
	_ = *(*byte)(unsafe.Pointer(x))       // a READ through it: not a store
}
