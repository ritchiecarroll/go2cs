package main

import (
	"fmt"
	"unsafe"
)

type RawMem [1<<50 - 1]byte

type small [64]byte

type sibling [64]byte

type pair struct {
	a int32
	b int32
}

func (p *pair) bump() { p.a++ }

type hdr struct {
	n    uint32
	port uint16
	tail [8]byte
}

func take(v int32) int32 { return v }

func takeArr(v [2]byte) byte { return v[1] }

// ---- index: the forms over a plain unsafe.Pointer and over a number.
func idxRead(p unsafe.Pointer, i int) int32 {
	return (*[4]int32)(p)[i]
}

func idxStore(u uintptr) {
	(*[4]int32)(unsafe.Pointer(u))[1] = 7
}

func idxCompound(p unsafe.Pointer, i int) {
	(*[4]int32)(p)[i] += 3
	(*[4]int32)(p)[i]++
}

func idxNamedStore(u uintptr, n int) {
	(*RawMem)(unsafe.Pointer(u))[n] = 0
}

func idxNamedRead(p unsafe.Pointer) byte {
	return (*small)(p)[5]
}

func idxAddr(p unsafe.Pointer) *int32 {
	return &(*[4]int32)(p)[2]
}

func idxArg(p unsafe.Pointer) int32 {
	return take((*[4]int32)(p)[1])
}

func idxBound(p unsafe.Pointer, i uint64) int32 {
	x := (*[4]int32)(p)[i]
	return x
}

func idxField(p unsafe.Pointer) int32 {
	(*[3]pair)(p)[1].a = 5
	return (*[3]pair)(p)[1].b
}

func idxFieldAddr(p unsafe.Pointer) *int32 {
	return &(*[3]pair)(p)[1].a
}

func idxMethod(p unsafe.Pointer) {
	(*[3]pair)(p)[1].bump()
}

func idxParen(p unsafe.Pointer) int32 {
	return ((*[4]int32)(p))[1]
}

func idxDeref(p unsafe.Pointer) int32 {
	return (*(*[4]int32)(p))[1]
}

func idxTuple(p unsafe.Pointer) {
	(*[4]int32)(p)[0], (*[4]int32)(p)[1] = 1, 2
}

func idxClosure(p unsafe.Pointer) func() int32 {
	return func() int32 { return (*[4]int32)(p)[3] }
}

func idxAdded(p unsafe.Pointer, off uintptr) byte {
	return (*[8]byte)(unsafe.Add(p, off))[0]
}

func idxNested(p unsafe.Pointer) int32 {
	return (*[2][2]int32)(p)[1][0]
}

// ---- index: the routes that render the conversion whole.
func idxIdentity(a *[4]int) int {
	return (*[4]int)(unsafe.Pointer(a))[1]
}

func idxIdentityNamed(a *small) byte {
	return (*small)(unsafe.Pointer(a))[1]
}

func idxIdentityAddr(a *[4]int) *int {
	return &(*[4]int)(unsafe.Pointer(a))[1]
}

func idxAlias(buf *uint16, i int) uint16 {
	return (*[10000]uint16)(unsafe.Pointer(buf))[i]
}

func idxAliasNamed(b *byte, i int) byte {
	return (*RawMem)(unsafe.Pointer(b))[i]
}

func idxSibling(a *sibling) byte {
	return (*small)(unsafe.Pointer(a))[1]
}

func idxSiblingUnnamed(a *[64]byte) byte {
	return (*small)(unsafe.Pointer(a))[1]
}

func idxUnnamedFromNamed(a *small) byte {
	return (*[64]byte)(unsafe.Pointer(a))[1]
}

func idxFieldView(h *hdr) byte {
	return (*[2]byte)(unsafe.Pointer(&h.port))[1]
}

// ---- index: Go values of another type.
func idxScalar(x *uint32) byte {
	return (*[4]byte)(unsafe.Pointer(x))[0]
}

func idxScalarLocal() byte {
	x := uint32(0x01020304)
	return (*[4]byte)(unsafe.Pointer(&x))[0]
}

func idxScalarAddr(x *uintptr, i int) *uint8 {
	return &(*[8]uint8)(unsafe.Pointer(x))[i]
}

func idxOtherElem(w *[4]uint32) byte {
	return (*[16]byte)(unsafe.Pointer(w))[8]
}

func idxStructBytes(h *hdr) byte {
	return (*[16]byte)(unsafe.Pointer(h))[4]
}

// ---- index: outside the doors.
func idxManaged(p unsafe.Pointer) unsafe.Pointer {
	return (*[2]unsafe.Pointer)(p)[1]
}

func idxManagedPtr(p unsafe.Pointer) *int {
	return (*[4]*int)(p)[0]
}

func (h *hdr) first() byte {
	return (*[12]byte)(unsafe.Pointer(h))[0]
}

// ---- the whole array: loads.
func loadBound(p unsafe.Pointer) int64 {
	x := *(*[2]int64)(p)
	return x[1]
}

func loadVar(p unsafe.Pointer) byte {
	var v [2]byte = *(*[2]byte)(p)
	return v[0]
}

func loadReturn(p unsafe.Pointer) [2]uintptr {
	return *(*[2]uintptr)(p)
}

func loadCompare(p, q unsafe.Pointer) bool {
	return *(*[2]int64)(p) == *(*[2]int64)(q)
}

func loadArg(p unsafe.Pointer) byte {
	return takeArr(*(*[2]byte)(p))
}

func loadStruct(p unsafe.Pointer) int32 {
	x := *(*[3]pair)(p)
	return x[2].b
}

func loadNamed(p unsafe.Pointer) byte {
	x := *(*small)(p)
	return x[3]
}

func loadIdentity(a *[4]int) int {
	x := *(*[4]int)(unsafe.Pointer(a))
	return x[0]
}

func loadScalar(x *uint32) byte {
	v := *(*[4]byte)(unsafe.Pointer(x))
	return v[0]
}

func loadManaged(p unsafe.Pointer) *int {
	v := *(*[2]*int)(p)
	return v[0]
}

func loadRange(p unsafe.Pointer) (n int) {
	for _, b := range *(*[4]byte)(p) {
		n += int(b)
	}
	return
}

func loadLen(p unsafe.Pointer) int {
	return len(*(*[4]byte)(p))
}

// ---- the whole array: stores.
func storeLiteral(u uintptr) {
	*(*[2]byte)(unsafe.Pointer(u)) = [2]byte{'C', 0}
}

func storeZero(p unsafe.Pointer) {
	*(*[2]uintptr)(p) = [2]uintptr{}
}

func storeVar(p unsafe.Pointer, v [2]byte) {
	*(*[2]byte)(p) = v
}

func storeLoad(p, q unsafe.Pointer) {
	*(*[2]byte)(p) = *(*[2]byte)(q)
}

func storeNamed(p unsafe.Pointer, s small) {
	*(*small)(p) = s
}

func storeIdentity(a *[4]int, v [4]int) {
	*(*[4]int)(unsafe.Pointer(a)) = v
}

func storeScalar(x *uint32, v [4]byte) {
	*(*[4]byte)(unsafe.Pointer(x)) = v
}

func storeTuple(p unsafe.Pointer, v [2]byte) (n int) {
	*(*[2]byte)(p), n = v, 1
	return
}

func storeManaged(p unsafe.Pointer, v [2]*int) {
	*(*[2]*int)(p) = v
}

func main() {
	var a [4]int
	var s small
	var sib sibling
	var arr64 [64]byte
	var h hdr
	var x uint32
	var up uintptr
	var w [4]uint32
	var buf [16]uint16
	var i32 [4]int32
	var prs [3]pair
	var i64 [2]int64
	var ptrs [4]*int
	p := unsafe.Pointer(&i32)
	idxStore(uintptr(p))
	idxCompound(p, 0)
	idxTuple(p)
	*idxAddr(p) = 9
	fmt.Println(idxRead(p, 1), idxArg(p), idxBound(p, 2), idxParen(p), idxDeref(p), idxClosure(p)())
	_ = idxNested
	q := unsafe.Pointer(&prs)
	idxMethod(q)
	*idxFieldAddr(q) += 2
	fmt.Println(idxField(q), prs[1].a, loadStruct(q))
	r := unsafe.Pointer(&arr64)
	idxNamedStore(uintptr(r), 3)
	fmt.Println(idxNamedRead(r), idxAdded(r, 8), idxIdentity(&a), idxIdentityNamed(&s), *idxIdentityAddr(&a), idxAlias(&buf[0], 2), idxAliasNamed(&s[0], 2))
	fmt.Println(idxSibling(&sib), idxSiblingUnnamed(&arr64), idxUnnamedFromNamed(&s), idxFieldView(&h), idxScalar(&x), idxScalarLocal(), *idxScalarAddr(&up, 1), idxOtherElem(&w), idxStructBytes(&h))
	fmt.Println(idxManaged(unsafe.Pointer(&ptrs)) == nil, idxManagedPtr(unsafe.Pointer(&ptrs)) == nil, h.first())
	t := unsafe.Pointer(&i64)
	storeLiteral(uintptr(r))
	storeVar(r, [2]byte{1, 2})
	storeLoad(r, unsafe.Pointer(&s))
	storeNamed(r, s)
	storeIdentity(&a, [4]int{1})
	storeScalar(&x, [4]byte{1})
	storeManaged(unsafe.Pointer(&ptrs), [2]*int{})
	fmt.Println(loadBound(t), loadVar(r), loadReturn(t), loadCompare(t, t), loadArg(r), loadNamed(r), loadIdentity(&a), loadScalar(&x), loadManaged(unsafe.Pointer(&ptrs)) == nil, loadRange(r), loadLen(r), storeTuple(r, [2]byte{}))
	storeZero(t)
}
