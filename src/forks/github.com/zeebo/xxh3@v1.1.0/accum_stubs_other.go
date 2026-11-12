// go2cs fork: the vector paths need assembly, so every platform takes the scalar path (this file was the
// original's stub for platforms with no assembly).

package xxh3

const (
	hasAVX2   = false
	hasSSE2   = false
	hasAVX512 = false
	hasNEON   = false
)

func accumAVX2(acc *[8]u64, data, key ptr, len u64)   { panic("unreachable") }
func accumSSE(acc *[8]u64, data, key ptr, len u64)    { panic("unreachable") }
func accumBlockAVX2(acc *[8]u64, data, key ptr)       { panic("unreachable") }
func accumBlockSSE(acc *[8]u64, data, key ptr)        { panic("unreachable") }
func accumAVX512(acc *[8]u64, data, key ptr, len u64) { panic("unreachable") }
func accumBlockAVX512(acc *[8]u64, data, key ptr)     { panic("unreachable") }
func accumNEON(acc *[8]u64, data, key ptr, len u64)   { panic("unreachable") }
func accumBlockNEON(acc *[8]u64, data, key ptr)       { panic("unreachable") }

func withAVX512(cb func())  { cb() }
func withAVX2(cb func())    { cb() }
func withSSE2(cb func())    { cb() }
func withGeneric(cb func()) { cb() }
